using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ivr.Api.Auth;
using Ivr.Api.Middleware;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Api.Foundation;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Intake;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Intake;

public static class TaskIntakeEndpoint
{
    public const string Route = "/v1/ivr/order-confirmation/tasks";
    public const string IdempotencyHeader = "Idempotency-Key";
    private const int MaximumBodyBytes = 1_048_576;

    // W-0359 / K-28. What a schema refusal's details.field can hold: "$" for the task object
    // itself, and below it only the wire names in the property sets at the end of this class and
    // array indices. Never a value, and never a name the producer chose.
    private const string RootPath = "$";
    private const string SpeechPath = "privacy_safe_order_summary";

    private static readonly JsonSerializerOptions RequestJsonOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
    };

    public static IEndpointRouteBuilder MapIvrTaskIntakeEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapPost(Route, HandleAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireOrderCoreAttribute());
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        ITaskIntakeService intakeService,
        IOptions<IvrOptions> options,
        CancellationToken cancellationToken)
    {
        string correlationId = RequiredHeader(
            context,
            CorrelationPropagationHandler.HeaderName);
        string idempotencyKey = RequiredHeader(
            context,
            IdempotencyHeader);
        byte[] requestBytes;
        IvrConfirmationTaskV1 source;
        string payloadHash;
        try
        {
            requestBytes = await ReadBoundedBodyAsync(
                context.Request,
                cancellationToken);
            using JsonDocument document = JsonDocument.Parse(
                requestBytes,
                new JsonDocumentOptions
                {
                    MaxDepth = 64,
                    CommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = false,
                });
            ValidateSchema(document.RootElement);
            source = BindTask(requestBytes);
            payloadHash = CanonicalJsonSha256(document.RootElement);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or InvalidOperationException
                or ArgumentException)
        {
            // W-0359 / K-28. Only a refusal that knows its field names one. A body that is too
            // large, empty or not JSON at all has no field to name.
            return SchemaError(
                correlationId,
                (exception as SchemaViolationException)?.Field);
        }

        TaskIntakeOutcome outcome = await intakeService.IntakeAsync(
            new TaskIntakeCommand(
                source,
                idempotencyKey,
                correlationId,
                payloadHash,
                ParseExecutionMode(options.Value.ExecutionMode)),
            cancellationToken);
        if (outcome.IsFailure)
        {
            throw new IvrFailureException(
                outcome.FailureCode!,
                outcome.FailureMessage!);
        }

        if (!Enum.TryParse(
                outcome.Decision,
                false,
                out IvrTaskIntakeResultDecision decision))
        {
            throw IvrErrors.InternalError();
        }

        return Results.Ok(new IvrTaskIntakeResult
        {
            Decision = decision,
            Ivr_call_job_id = outcome.IvrCallJobId,
            Blocked_reasons = outcome.BlockedReasons,
            Evidence_ref = outcome.EvidenceRef,
        });
    }

    private static string RequiredHeader(
        HttpContext context,
        string headerName)
    {
        string value = context.Request.Headers[headerName].ToString();
        if (string.IsNullOrEmpty(value))
        {
            throw new IvrFailureException(
                IvrErrorCodes.MissingTrace,
                string.Concat(headerName, " is required."));
        }

        // W-0221. One rule, shared with the internal guard and the correlation middleware.
        if (TraceHeaderSyntax.IsValid(value))
        {
            return value;
        }

        throw new IvrFailureException(
            IvrErrorCodes.MalformedRequest,
            string.Concat(headerName, " has invalid syntax."));
    }

    private static async Task<byte[]> ReadBoundedBodyAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is > MaximumBodyBytes)
        {
            throw new InvalidDataException("The task body is too large.");
        }

        using var buffer = new MemoryStream();
        byte[] chunk = new byte[16_384];
        while (true)
        {
            int read = await request.Body.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                break;
            }

            if (buffer.Length + read > MaximumBodyBytes)
            {
                throw new InvalidDataException("The task body is too large.");
            }

            buffer.Write(chunk, 0, read);
        }

        if (buffer.Length == 0)
        {
            throw new InvalidDataException("The task body is required.");
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// W-0359 / K-28. Binds a body <see cref="ValidateSchema"/> has passed to the generated type.
    /// What is not checked by hand - an enum member, a timestamp, the type of an optional field -
    /// fails here, and the serializer's path is what names the field.
    /// </summary>
    private static IvrConfirmationTaskV1 BindTask(byte[] requestBytes)
    {
        IvrConfirmationTaskV1? source;
        try
        {
            source = JsonSerializer.Deserialize<IvrConfirmationTaskV1>(
                requestBytes,
                RequestJsonOptions);
        }
        catch (JsonException exception)
        {
            throw new SchemaViolationException(
                SerializerField(exception.Path),
                "The task body does not bind to ivr-order-confirmation.v1.",
                exception);
        }

        return source ?? throw new JsonException("The task body is required.");
    }

    private static void ValidateSchema(JsonElement root)
    {
        EnsureObject(root, RootPath);
        EnsureProperties(
            root,
            TaskProperties,
            RequiredTaskProperties,
            RootPath);

        // W-0359 / K-28. The kind is checked first: GetString on a number threw
        // InvalidOperationException, which still became the 400 but could not name the field.
        JsonElement contractVersion = root.GetProperty("contract_version");
        if (contractVersion.ValueKind != JsonValueKind.String
            || contractVersion.GetString() != "ivr-order-confirmation.v1")
        {
            throw new SchemaViolationException("contract_version", "Unknown contract version.");
        }

        foreach (string field in RequiredTaskStringProperties)
        {
            EnsureNonBlankString(root.GetProperty(field), field);
        }

        // W-0312. The single anyOf on IvrConfirmationTaskV1, as a runtime rule: the full token
        // pair, or the number with no token field beside it. Half a pair is refused whether or not
        // a number came with it, because a producer that sent half a token has a bug worth hearing
        // about even on the days the number would have carried the call.
        bool hasToken = root.TryGetProperty("dial_token", out JsonElement dialToken);
        bool hasTokenExpiry = root.TryGetProperty("dial_token_expires_at", out _);
        bool hasNumber = root.TryGetProperty("phone_e164", out _);
        bool tokenPair = hasToken && hasTokenExpiry;
        bool numberAlone = hasNumber && !hasToken && !hasTokenExpiry;
        if (!tokenPair && !numberAlone)
        {
            // W-0359 / K-28. Half a pair names the half that is missing. A body with neither shape
            // names phone_e164, the shape option B asks producers to send.
            string missing = (hasToken, hasTokenExpiry) switch
            {
                (true, false) => "dial_token_expires_at",
                (false, true) => "dial_token",
                _ => "phone_e164",
            };
            throw new SchemaViolationException(
                missing,
                "A task carries the dial-token pair or phone_e164 with no token field.");
        }

        if (hasToken)
        {
            EnsureNonBlankString(dialToken, "dial_token");
        }

        foreach ((string wireName, Regex pattern) in TaskPatternProperties)
        {
            if (root.TryGetProperty(wireName, out JsonElement patterned)
                && (patterned.ValueKind != JsonValueKind.String
                    || !pattern.IsMatch(patterned.GetString()!)))
            {
                throw new SchemaViolationException(
                    wireName,
                    string.Concat(wireName, " violates its pattern."));
            }
        }

        // W-0359 / K-28. The checks below used to share one throw per group, so the refusal could
        // not say which field broke them. Each now names its own; what they accept is unchanged.
        if (root.GetProperty("call_restriction").ValueKind is not (
                JsonValueKind.True or JsonValueKind.False))
        {
            throw new SchemaViolationException(
                "call_restriction",
                "A task scalar violates its schema.");
        }

        if (root.GetProperty("max_customer_attempts").ValueKind
                != JsonValueKind.Number
            || !root.GetProperty("max_customer_attempts").TryGetInt32(
                out int maxAttempts)
            || maxAttempts is < 1 or > 10)
        {
            throw new SchemaViolationException(
                "max_customer_attempts",
                "A task scalar violates its schema.");
        }

        // The matrix is one rule over two fields. A program IVR does not run names program_code; a
        // known program sent with the other program's payment names payment_method_snapshot.
        string program = root.GetProperty("program_code").GetString() ?? string.Empty;
        string payment = root.GetProperty("payment_method_snapshot").GetString()
            ?? string.Empty;
        string? paymentForProgram = program switch
        {
            "GOLDEN_HOUR" => "ONLINE",
            "TWENTY_FOUR_SEVEN" => "COD",
            _ => null,
        };
        if (paymentForProgram is null)
        {
            throw new SchemaViolationException(
                "program_code",
                "Unsupported program/payment matrix.");
        }

        if (payment != paymentForProgram)
        {
            throw new SchemaViolationException(
                "payment_method_snapshot",
                "Unsupported program/payment matrix.");
        }

        if (root.GetProperty("ivr_confirmation_required").ValueKind != JsonValueKind.True)
        {
            throw new SchemaViolationException(
                "ivr_confirmation_required",
                "ivr_confirmation_required must be true.");
        }

        JsonElement offsets = root.GetProperty("attempt_offsets_seconds");
        ValidateArray(offsets, "attempt_offsets_seconds", 1, 10);
        int offsetIndex = 0;
        foreach (JsonElement offset in offsets.EnumerateArray())
        {
            if (offset.ValueKind != JsonValueKind.Number
                || !offset.TryGetInt32(out int value)
                || value < 0)
            {
                throw new SchemaViolationException(
                    ElementPath("attempt_offsets_seconds", offsetIndex),
                    "Attempt offsets must be nonnegative integers.");
            }

            offsetIndex++;
        }

        JsonElement speech = root.GetProperty(SpeechPath);
        EnsureObject(speech, SpeechPath);
        EnsureProperties(
            speech,
            SpeechProperties,
            RequiredSpeechProperties,
            SpeechPath);
        foreach (string field in RequiredSpeechStringProperties)
        {
            EnsureNonBlankString(speech.GetProperty(field), ChildPath(SpeechPath, field));
        }

        if (speech.GetProperty("currency").GetString() != "VND")
        {
            throw new SchemaViolationException(
                ChildPath(SpeechPath, "currency"),
                "The speech summary violates its schema.");
        }

        if (speech.GetProperty("locale").GetString() != "vi-VN")
        {
            throw new SchemaViolationException(
                ChildPath(SpeechPath, "locale"),
                "The speech summary violates its schema.");
        }

        // W-0354 / B16. total_amount is what the customer pays, in whole dong, after the order's one
        // final rounding. A fraction used to pass here and then fail at DIAL time, inside the
        // speller ("VND has no spoken subunit"), where the gateway read it as a broken channel and
        // quarantined a SIM: the order was never called and other orders lost dialling capacity.
        // Refused at the door instead, where Module 3 can see it and fix the producer.
        if (speech.GetProperty("total_amount").ValueKind != JsonValueKind.Number
            || !speech.GetProperty("total_amount").TryGetDecimal(out decimal totalAmount)
            || totalAmount < 0
            || totalAmount != decimal.Truncate(totalAmount))
        {
            throw new SchemaViolationException(
                ChildPath(SpeechPath, "total_amount"),
                "The speech summary violates its schema.");
        }

        // W-0359 / K-28. Each element is checked as an object inside the loop, with its index, so
        // a stray element is named where it is; the whole-array check this replaced could not.
        string itemsPath = ChildPath(SpeechPath, "items");
        JsonElement items = speech.GetProperty("items");
        ValidateArray(items, itemsPath, 1, 100);
        int itemIndex = 0;
        foreach (JsonElement item in items.EnumerateArray())
        {
            string itemPath = ElementPath(itemsPath, itemIndex);
            EnsureObject(item, itemPath);
            EnsureProperties(
                item,
                SpeechItemProperties,
                RequiredSpeechItemProperties,
                itemPath);
            EnsureNonBlankString(
                item.GetProperty("public_name"),
                ChildPath(itemPath, "public_name"));
            if (item.GetProperty("quantity").ValueKind != JsonValueKind.Number
                || !item.GetProperty("quantity").TryGetDecimal(out decimal quantity)
                || quantity <= 0)
            {
                throw new SchemaViolationException(
                    ChildPath(itemPath, "quantity"),
                    "Speech quantity must be positive.");
            }

            itemIndex++;
        }

        string area = speech.GetProperty("delivery_area_short").GetString()
            ?? string.Empty;
        if (area.Length == 0
            || char.IsDigit(area.TrimStart().FirstOrDefault())
            || HasSlashHouseNumber(area))
        {
            throw new SchemaViolationException(
                ChildPath(SpeechPath, "delivery_area_short"),
                "Delivery area is not schema safe.");
        }

        EnsureObject(root.GetProperty("eligibility_snapshot"), "eligibility_snapshot");
    }

    private static void EnsureObject(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new SchemaViolationException(path, string.Concat(path, " must be an object."));
        }
    }

    private static void EnsureNonBlankString(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(element.GetString()))
        {
            throw new SchemaViolationException(path, string.Concat(path, " must be a string."));
        }
    }

    private static void EnsureProperties(
        JsonElement element,
        HashSet<string> allowed,
        HashSet<string> required,
        string path)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            // W-0359 / K-28. The parent's path, never the property's own name: an unknown name is
            // text the producer wrote, and details.field holds only text this class defines.
            if (!allowed.Contains(property.Name) || !seen.Add(property.Name))
            {
                throw new SchemaViolationException(
                    path,
                    string.Concat(path, " contains an unknown or duplicate field."));
            }
        }

        // The missing name is taken from the required set, so it is this class's own text too; the
        // first in ordinal order, so that one body always names the same field.
        string? missing = required
            .Where(name => !seen.Contains(name))
            .Min(StringComparer.Ordinal);
        if (missing is not null)
        {
            string field = ChildPath(path, missing);
            throw new SchemaViolationException(field, string.Concat(field, " is required."));
        }
    }

    private static void ValidateArray(
        JsonElement element,
        string path,
        int minimum,
        int maximum)
    {
        if (element.ValueKind != JsonValueKind.Array
            || element.GetArrayLength() < minimum
            || element.GetArrayLength() > maximum)
        {
            throw new SchemaViolationException(path, "An array violates its schema bounds.");
        }
    }

    private static string ChildPath(string parentPath, string name) =>
        parentPath == RootPath ? name : string.Concat(parentPath, ".", name);

    private static string ElementPath(string arrayPath, int index) =>
        string.Concat(arrayPath, "[", index.ToString(CultureInfo.InvariantCulture), "]");

    /// <summary>
    /// W-0359 / K-28. A path the serializer reported, in the form details.field uses: without its
    /// leading <c>$.</c>, and only when every step is one of this contract's wire names or an
    /// array index - otherwise null. The serializer writes a dictionary key into its path as it
    /// found it, and a pronunciation hint's key is text the producer wrote, often a name.
    /// </summary>
    private static string? SerializerField(string? path)
    {
        if (path is null)
        {
            return null;
        }

        Match shape;
        try
        {
            shape = SerializerPathShape.Match(path);
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }

        if (!shape.Success
            || shape.Groups["name"].Captures.Any(name => !WireNames.Contains(name.Value)))
        {
            return null;
        }

        return path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : path;
    }

    private static bool HasSlashHouseNumber(string value)
    {
        for (int index = 1; index < value.Length - 1; index++)
        {
            if (value[index] == '/'
                && char.IsDigit(value[index - 1])
                && char.IsDigit(value[index + 1]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The payload fingerprint intake stores and later compares idempotent replays against.
    /// <para>
    /// Internal rather than private because the UI-07 seed loader (W-0112) admits fixtures
    /// through the same intake service and must produce the same fingerprint for the same body.
    /// Two canonicalisations would agree until the day one of them changed.
    /// </para>
    /// </summary>
    internal static string CanonicalJsonSha256(JsonElement root)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            WriteCanonical(root, writer);
        }

        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()));
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty property in element.EnumerateObject()
                             .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(property.Value, writer);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (JsonElement item in element.EnumerateArray())
                {
                    WriteCanonical(item, writer);
                }

                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static IResult SchemaError(string correlationId, string? field)
    {
        // W-0359 / K-28. details used to be empty, so a producer could not tell which field was
        // wrong. It names the field's path now, never its value. The envelope has always declared
        // details as a map of strings, so the new key moves neither the contract nor the message,
        // status and code a producer already branches on.
        Dictionary<string, string> details = new(StringComparer.Ordinal);
        if (field is not null)
        {
            details["field"] = field;
        }

        return Results.Json(
            new IvrErrorEnvelope(new IvrErrorBody(
                IvrErrorCodes.MalformedRequest,
                "The task body does not match ivr-order-confirmation.v1.",
                details,
                correlationId)),
            statusCode: StatusCodes.Status400BadRequest);
    }

    private static ExecutionMode ParseExecutionMode(string value) => value switch
    {
        IvrOptions.MockExecutionMode => ExecutionMode.Mock,
        IvrOptions.LabRealSimExecutionMode => ExecutionMode.LabRealSim,
        IvrOptions.ProductionRealExecutionMode => ExecutionMode.ProductionReal,
        _ => throw new InvalidOperationException("Unsupported IVR execution mode."),
    };

    private static readonly HashSet<string> RequiredTaskProperties =
    [
        "contract_version", "task_id", "order_id", "order_code", "order_version",
        "order_state", "payment_method_snapshot", "ivr_confirmation_required",
        "program_code", "confirmation_window_started_at",
        "confirmation_window_expires_at", "attempt_policy_version",
        "max_customer_attempts", "attempt_offsets_seconds", "phone_ref",
        "phone_masked",
        "privacy_safe_order_summary", "call_restriction", "eligibility_snapshot",
        "evidence_ref",
    ];

    private static readonly HashSet<string> RequiredTaskStringProperties =
    [
        "contract_version", "task_id", "order_id", "order_code", "order_version",
        "order_state", "payment_method_snapshot", "program_code",
        "attempt_policy_version", "phone_ref", "phone_masked",
        "evidence_ref",
    ];

    /// <summary>
    /// W-0312. Every pattern the contract puts on a task field, read off the attributes NSwag
    /// generates from the OpenAPI file instead of being written a second time here.
    /// <para>
    /// draft.30 declared <c>^\+84[0-9]{9}$</c> on <c>phone_e164</c> and nothing enforced it. This
    /// endpoint parses by hand, DataAnnotations never run, so a malformed number was accepted,
    /// stored in the clear and failed only at dial time. Reading the attribute makes the OpenAPI
    /// file the one place a pattern is written, for this field and for the next one.
    /// </para>
    /// <para>
    /// Matched here rather than through <c>RegularExpressionAttribute.IsValid</c>, which answers
    /// true for an empty string and demands a whole-string match. The contract's validator does
    /// neither: a JSON Schema pattern is an ECMAScript search. Either difference would be a body
    /// the specification refuses and this endpoint accepts, or the reverse.
    /// </para>
    /// </summary>
    private static readonly (string WireName, Regex Pattern)[] TaskPatternProperties =
    [
        .. typeof(IvrConfirmationTaskV1)
            .GetProperties()
            .Select(property => (
                WireName: property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name,
                Attribute: property.GetCustomAttribute<RegularExpressionAttribute>()))
            .Where(entry => entry.WireName is not null && entry.Attribute is not null)
            .Select(entry => (
                entry.WireName!,
                new Regex(
                    WithEcmaScriptEndAnchor(entry.Attribute!.Pattern),
                    RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(250)))),
    ];

    /// <summary>
    /// ECMAScript's <c>$</c>, without the multiline flag, matches only at the end of the input.
    /// .NET's also matches before a final newline, so <c>^\+84[0-9]{9}$</c> would accept a valid
    /// number followed by <c>\n</c> that the contract's validator refuses. A trailing <c>$</c>
    /// becomes <c>\z</c>, which means in .NET what <c>$</c> means in the schema.
    /// </summary>
    internal static string WithEcmaScriptEndAnchor(string pattern) =>
        pattern.EndsWith('$') && !pattern.EndsWith(@"\$", StringComparison.Ordinal)
            ? string.Concat(pattern.AsSpan(0, pattern.Length - 1), @"\z")
            : pattern;

    /// <summary>
    /// W-0359 / K-28. <c>$</c>, then <c>.name</c> and <c>[index]</c> steps and nothing else. A key
    /// holding any other character is written <c>['key']</c> by the serializer and never matches.
    /// Ends in <c>\z</c> for the reason given on <see cref="WithEcmaScriptEndAnchor"/>.
    /// </summary>
    private static readonly Regex SerializerPathShape = new(
        @"^\$(?:\.(?<name>[a-z_]+)|\[[0-9]+\])*\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(250));

    // OD-18 compatibility window: customer_trust_status and trusted_skip_allowed stay in the
    // strict allowlist as LEGACY_READ inputs so an older M3 producer is not rejected during a
    // rolling deploy. Runtime eligibility ignores both. risk_flags remains audit/scheduler
    // priority metadata and likewise cannot decide call/skip.
    private static readonly HashSet<string> TaskProperties =
    [
        .. RequiredTaskProperties,
        "correlation_id", "created_at", "order_code_short", "is_ivr_callable",
        "customer_ref", "customer_trust_status", "trusted_skip_allowed", "risk_flags",
        "phone_validation_status", "call_script_template_id", "call_script_version",
        "allowed_script_variables", "evidence_policy_version",
        "privacy_policy_version",

        // W-0311 option B, W-0312. Neither the number nor the token pair is required on its own:
        // the task needs one of the two, which ValidateSchema checks as a pair of alternatives -
        // the same single anyOf the contract declares. Listing either as required would refuse
        // the other shape outright, which is exactly what draft.30 did to a number-only task.
        "phone_e164",
        "dial_token", "dial_token_expires_at",
    ];

    private static readonly HashSet<string> RequiredSpeechProperties =
    [
        "customer_display_name", "order_code_short", "items", "total_amount",
        "currency", "delivery_area_short", "program_display_name", "locale",
    ];

    private static readonly HashSet<string> RequiredSpeechStringProperties =
    [
        "customer_display_name", "order_code_short", "currency",
        "delivery_area_short", "program_display_name", "locale",
    ];

    private static readonly HashSet<string> SpeechProperties =
    [
        .. RequiredSpeechProperties,
        "pronunciation_hints",
    ];

    private static readonly HashSet<string> RequiredSpeechItemProperties =
    [
        "public_name", "quantity",
    ];

    private static readonly HashSet<string> SpeechItemProperties =
    [
        .. RequiredSpeechItemProperties,
        "unit_label",
    ];

    // W-0359 / K-28. Every property name this contract defines: the only names a serializer path
    // may contain and still be reported. Declared after the sets it reads, because static fields
    // initialise in textual order and an earlier declaration would read them while still null.
    private static readonly HashSet<string> WireNames =
    [
        .. TaskProperties,
        .. SpeechProperties,
        .. SpeechItemProperties,
    ];

    /// <summary>
    /// W-0359 / K-28. A schema refusal that knows which field it is about.
    /// <para>
    /// <see cref="Field"/> is a path built from this class's own wire names and array indices,
    /// never a value and never a property name the producer chose; null when there is no path it
    /// can name that way. The type derives from <see cref="JsonException"/>, one of the types
    /// <c>HandleAsync</c> already turns into the 400, because <see cref="InvalidDataException"/>,
    /// which these refusals were thrown as before, is sealed.
    /// </para>
    /// </summary>
    private sealed class SchemaViolationException(
        string? path,
        string message,
        Exception? innerException = null) : JsonException(message, innerException)
    {
        public string? Field { get; } = path;
    }
}
