using System.Security.Cryptography;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Middleware;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
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
                cancellationToken).ConfigureAwait(false);
            using JsonDocument document = JsonDocument.Parse(
                requestBytes,
                new JsonDocumentOptions
                {
                    MaxDepth = 64,
                    CommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = false,
                });
            ValidateSchema(document.RootElement);
            source = JsonSerializer.Deserialize<IvrConfirmationTaskV1>(
                    requestBytes,
                    RequestJsonOptions)
                ?? throw new JsonException("The task body is required.");
            payloadHash = CanonicalJsonSha256(document.RootElement);
        }
        catch (Exception exception) when (
            exception is JsonException
                or InvalidDataException
                or InvalidOperationException
                or ArgumentException)
        {
            return SchemaError(correlationId);
        }

        TaskIntakeOutcome outcome = await intakeService.IntakeAsync(
            new TaskIntakeCommand(
                source,
                idempotencyKey,
                correlationId,
                payloadHash,
                ParseExecutionMode(options.Value.ExecutionMode)),
            cancellationToken).ConfigureAwait(false);
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

        if (value.Length is > 0 and <= 128
            && PiiGuard.IsSafeText(value)
            && value.All(character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '-' or '_' or '.' or ':'))
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
            int read = await request.Body.ReadAsync(chunk, cancellationToken)
                .ConfigureAwait(false);
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

    private static void ValidateSchema(JsonElement root)
    {
        EnsureObject(root, "task");
        EnsureProperties(
            root,
            TaskProperties,
            RequiredTaskProperties,
            "task");
        if (root.GetProperty("contract_version").GetString()
            != "ivr-order-confirmation.v1")
        {
            throw new InvalidDataException("Unknown contract version.");
        }

        foreach (string field in RequiredTaskStringProperties)
        {
            EnsureNonBlankString(root.GetProperty(field), field);
        }

        if (root.GetProperty("call_restriction").ValueKind is not (
                JsonValueKind.True or JsonValueKind.False)
            || root.GetProperty("max_customer_attempts").ValueKind
                != JsonValueKind.Number
            || !root.GetProperty("max_customer_attempts").TryGetInt32(
                out int maxAttempts)
            || maxAttempts is < 1 or > 10)
        {
            throw new InvalidDataException("A task scalar violates its schema.");
        }

        string program = root.GetProperty("program_code").GetString() ?? string.Empty;
        string payment = root.GetProperty("payment_method_snapshot").GetString()
            ?? string.Empty;
        bool allowedMatrix = (program, payment) is
            ("GOLDEN_HOUR", "ONLINE") or ("TWENTY_FOUR_SEVEN", "COD");
        if (!allowedMatrix || root.GetProperty("ivr_confirmation_required").ValueKind
                != JsonValueKind.True)
        {
            throw new InvalidDataException("Unsupported program/payment matrix.");
        }

        JsonElement offsets = root.GetProperty("attempt_offsets_seconds");
        ValidateArray(offsets, 1, 10, false);
        if (offsets.EnumerateArray().Any(offset =>
                offset.ValueKind != JsonValueKind.Number
                || !offset.TryGetInt32(out int value)
                || value < 0))
        {
            throw new InvalidDataException("Attempt offsets must be nonnegative integers.");
        }
        JsonElement speech = root.GetProperty("privacy_safe_order_summary");
        EnsureObject(speech, "privacy_safe_order_summary");
        EnsureProperties(
            speech,
            SpeechProperties,
            RequiredSpeechProperties,
            "privacy_safe_order_summary");
        foreach (string field in RequiredSpeechStringProperties)
        {
            EnsureNonBlankString(speech.GetProperty(field), field);
        }

        if (speech.GetProperty("currency").GetString() != "VND"
            || speech.GetProperty("locale").GetString() != "vi-VN"
            || speech.GetProperty("total_amount").ValueKind != JsonValueKind.Number
            || !speech.GetProperty("total_amount").TryGetDecimal(out decimal totalAmount)
            || totalAmount < 0)
        {
            throw new InvalidDataException("The speech summary violates its schema.");
        }
        JsonElement items = speech.GetProperty("items");
        ValidateArray(items, 1, 100, true);
        foreach (JsonElement item in items.EnumerateArray())
        {
            EnsureObject(item, "privacy_safe_order_summary.items[]");
            EnsureProperties(
                item,
                SpeechItemProperties,
                RequiredSpeechItemProperties,
                "privacy_safe_order_summary.items[]");
            EnsureNonBlankString(item.GetProperty("public_name"), "public_name");
            if (item.GetProperty("quantity").ValueKind != JsonValueKind.Number
                || !item.GetProperty("quantity").TryGetDecimal(out decimal quantity)
                || quantity <= 0)
            {
                throw new InvalidDataException("Speech quantity must be positive.");
            }
        }

        string area = speech.GetProperty("delivery_area_short").GetString()
            ?? string.Empty;
        if (area.Length == 0
            || char.IsDigit(area.TrimStart().FirstOrDefault())
            || HasSlashHouseNumber(area))
        {
            throw new InvalidDataException("Delivery area is not schema safe.");
        }

        EnsureObject(root.GetProperty("eligibility_snapshot"), "eligibility_snapshot");
    }

    private static void EnsureObject(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException(string.Concat(name, " must be an object."));
        }
    }

    private static void EnsureNonBlankString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(element.GetString()))
        {
            throw new InvalidDataException(string.Concat(name, " must be a string."));
        }
    }

    private static void EnsureProperties(
        JsonElement element,
        HashSet<string> allowed,
        HashSet<string> required,
        string name)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name) || !seen.Add(property.Name))
            {
                throw new InvalidDataException(
                    string.Concat(name, " contains an unknown or duplicate field."));
            }
        }

        if (!required.IsSubsetOf(seen))
        {
            throw new InvalidDataException(string.Concat(name, " is missing a required field."));
        }
    }

    private static void ValidateArray(
        JsonElement element,
        int minimum,
        int maximum,
        bool elementsMustBeObjects)
    {
        if (element.ValueKind != JsonValueKind.Array
            || element.GetArrayLength() < minimum
            || element.GetArrayLength() > maximum
            || (elementsMustBeObjects
                && element.EnumerateArray().Any(item => item.ValueKind != JsonValueKind.Object)))
        {
            throw new InvalidDataException("An array violates its schema bounds.");
        }
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

    private static IResult SchemaError(string correlationId) =>
        Results.Json(
            new IvrErrorEnvelope(new IvrErrorBody(
                IvrErrorCodes.MalformedRequest,
                "The task body does not match ivr-order-confirmation.v1.",
                new Dictionary<string, string>(),
                correlationId)),
            statusCode: StatusCodes.Status400BadRequest);

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
        "phone_masked", "dial_token", "dial_token_expires_at",
        "privacy_safe_order_summary", "call_restriction", "eligibility_snapshot",
        "evidence_ref",
    ];

    private static readonly HashSet<string> RequiredTaskStringProperties =
    [
        "contract_version", "task_id", "order_id", "order_code", "order_version",
        "order_state", "payment_method_snapshot", "program_code",
        "attempt_policy_version", "phone_ref", "phone_masked", "dial_token",
        "evidence_ref",
    ];

    private static readonly HashSet<string> TaskProperties =
    [
        .. RequiredTaskProperties,
        "correlation_id", "created_at", "order_code_short", "is_ivr_callable",
        "customer_ref", "customer_trust_status", "trusted_skip_allowed", "risk_flags",
        "phone_validation_status", "call_script_template_id", "call_script_version",
        "allowed_script_variables", "sellable_status", "evidence_policy_version",
        "privacy_policy_version",
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
}
