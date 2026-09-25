using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Ivr.Api.Auth;
using Ivr.Api.Intake;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

public sealed class TaskIntakeApiTests
{
    [Theory]
    [InlineData("GOLDEN_HOUR", "ONLINE")]
    [InlineData("TWENTY_FOUR_SEVEN", "COD")]
    [Trait("TestId", "IT-INTAKE-HAPPY-01")]
    // W-0353: P5-2 §8 IDs this test satisfies, per the W-0036 addendum.
    [Trait("TestId", "CT-TASK-01")]
    [Trait("TestId", "CT-TASK-03")]
    public async Task SupportedProgramsReturnDryRunAndNeverInvokeRealCallPath(
        string program,
        string payment)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app.Client,
            CreateBody(program, payment));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrTaskIntakeResult result = (await response.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
        Assert.Equal(IvrTaskIntakeResultDecision.TASK_ACCEPTED_DRY_RUN_ONLY, result.Decision);
        Assert.NotNull(result.Ivr_call_job_id);
        Assert.Equal(1, app.Store.CallJobCount);
        Assert.Equal(1, app.Store.OutboxCount);
    }

    /// <summary>
    /// W-0129. Detailed service reasons are not permission to change the public intake contract:
    /// invalid policy shapes still fail schema validation, and invalid contact still uses the
    /// stable 422 error envelope.
    /// </summary>
    [Theory]
    [InlineData("required-flag", HttpStatusCode.BadRequest, IvrErrorCodes.MalformedRequest)]
    [InlineData("program-payment", HttpStatusCode.BadRequest, IvrErrorCodes.MalformedRequest)]
    // W-0250 moved phone_validation_status behind enum [VALID], so the status that used to reach
    // the 422 contact envelope is now refused at schema validation exactly as the required flag
    // above it is. Both spellings of the rule are kept: "contact-schema" pins the new 400 for the
    // closed field, "contact" pins that the 422 envelope itself did not move - it is still what an
    // unmasked phone gets, and specs/api/06-error-codes.md lists six more triggers that reach it.
    [InlineData("contact-schema", HttpStatusCode.BadRequest, IvrErrorCodes.MalformedRequest)]
    [InlineData("contact", HttpStatusCode.UnprocessableEntity, IvrErrorCodes.ContactInvalid)]
    [Trait("TestId", "IT-INTAKE-REASON-WIRE-15")]
    // W-0353: P5-2 §8 IDs this test satisfies, per the W-0036 addendum.
    [Trait("TestId", "CT-TASK-03")]
    public async Task ReasonRefinementPreservesTheWireStatusAndErrorCode(
        string scenario,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        switch (scenario)
        {
            case "required-flag":
                body["ivr_confirmation_required"] = false;
                break;
            case "program-payment":
                body["payment_method_snapshot"] = "COD";
                break;
            case "contact-schema":
                body["phone_validation_status"] = "PHONE_VALID";
                break;
            case "contact":
                body["phone_masked"] = "84901234567";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(expectedCode, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-M3-AUTHORITY-12")]
    public async Task TheRetiredSkipShapeIsCountedAndStillCalled()
    {
        // W-0124 F1. W-0123 argued from W-0118 that no producer sends risk evidence, so removing
        // the skip branch changed nobody's outcome — an inference no available database could
        // confirm. This asserts the two halves that make the argument checkable in production:
        // the population is counted, and it is still called.
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        var counted = new List<string>();
        using MeterListener listener = ListenForLegacySkipCandidates(counted);

        JsonObject legacySkipShape = CreateBody();
        legacySkipShape["task_id"] = "TASK-API-LEGACY-SKIP";
        legacySkipShape["order_id"] = "ORDER-API-LEGACY-SKIP";
        legacySkipShape["risk_flags"] = new JsonArray();
        legacySkipShape["eligibility_snapshot"] = new JsonObject
        {
            ["decision"] = "ELIGIBLE",
            ["source"] = "api-test",
            ["trust"] = new JsonObject { ["risk_evidence_available"] = true },
        };

        using HttpResponseMessage skipShapeResponse = await SendAsync(app.Client, legacySkipShape);

        Assert.Equal(HttpStatusCode.OK, skipShapeResponse.StatusCode);
        IvrTaskIntakeResult accepted = (await skipShapeResponse.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;

        // The point of OD-18: this exact payload used to end as TASK_SKIPPED_TRUSTED_CUSTOMER.
        Assert.Equal(IvrTaskIntakeResultDecision.TASK_ACCEPTED_DRY_RUN_ONLY, accepted.Decision);
        Assert.NotNull(accepted.Ivr_call_job_id);
        Assert.Equal(1, app.Store.CallJobCount);

        // A risk flag is what the retired predicate itself treated as "do call", so a payload
        // carrying one was never a skip candidate and must not be counted as the change OD-18
        // made. Without this half the counter would report the whole intake volume.
        JsonObject riskFlagged = CreateBody();
        riskFlagged["task_id"] = "TASK-API-RISK-FLAGGED";
        riskFlagged["order_id"] = "ORDER-API-RISK-FLAGGED";
        riskFlagged["risk_flags"] = new JsonArray("COD_FAIL_HISTORY");
        riskFlagged["eligibility_snapshot"] = new JsonObject
        {
            ["decision"] = "ELIGIBLE",
            ["source"] = "api-test",
            ["trust"] = new JsonObject { ["risk_evidence_available"] = true },
        };

        using HttpResponseMessage riskFlaggedResponse = await SendAsync(app.Client, riskFlagged);

        Assert.Equal(HttpStatusCode.OK, riskFlaggedResponse.StatusCode);
        Assert.Equal(2, app.Store.CallJobCount);

        listener.Dispose();

        // Exactly one measurement, not "at least one": the second send proving nothing was counted
        // is half the assertion. A MeterListener is process-wide and xUnit runs collections in
        // parallel, so this holds because no other fixture in this assembly sends
        // trust.risk_evidence_available through intake. If you add one, this is the test it breaks,
        // and the fix is to give that fixture a risk flag rather than to loosen the count.
        Assert.Equal(["ivr_legacy_skip_candidate_total"], counted);
    }

    private static MeterListener ListenForLegacySkipCandidates(List<string> observed)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName
                    && instrument.Name == "ivr_legacy_skip_candidate_total")
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, _, _) =>
        {
            lock (observed)
            {
                observed.Add(instrument.Name);
            }
        });
        listener.Start();
        return listener;
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-JSON-NULL-OMISSION-14")]
    public async Task OptionalNullResponseFieldsAreOmittedPerOpenApiContract()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        body["attempt_policy_version"] = "unknown-policy";

        using HttpResponseMessage response = await SendAsync(app.Client, body);
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("TASK_HELD_POLICY_MISSING", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ivr_call_job_id", json, StringComparison.Ordinal);
        Assert.DoesNotContain(":null", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-IDEMPOTENCY-02")]
    public async Task ExactReplayReturnsOriginalResponseAndChangedPayloadConflicts()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();

        using HttpResponseMessage first = await SendAsync(app.Client, body);
        using HttpResponseMessage replay = await SendAsync(app.Client, body);
        IvrTaskIntakeResult firstResult = (await first.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
        IvrTaskIntakeResult replayResult = (await replay.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
        Assert.Equal(firstResult.Ivr_call_job_id, replayResult.Ivr_call_job_id);
        Assert.Equal(firstResult.Decision, replayResult.Decision);
        Assert.Equal(1, app.Store.CallJobCount);

        JsonObject changed = (JsonObject)body.DeepClone();
        changed["order_version"] = "18";
        using HttpResponseMessage conflict = await SendAsync(app.Client, changed);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(IvrErrorCodes.IdempotencyConflict, await ErrorCodeAsync(conflict));
        Assert.Equal(1, app.Store.CallJobCount);
    }

    [Theory]
    [InlineData("matrix")]
    [InlineData("required-flag")]
    [InlineData("unknown-speech-field")]
    [Trait("TestId", "IT-INTAKE-SCHEMA-03")]
    // W-0353: P5-2 §8 IDs this test satisfies, per the W-0036 addendum.
    [Trait("TestId", "CT-TASK-02")]
    public async Task SchemaViolationsReturnMalformed400(string scenario)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        switch (scenario)
        {
            case "matrix":
                body["payment_method_snapshot"] = "COD";
                break;
            case "required-flag":
                body["ivr_confirmation_required"] = false;
                break;
            case "unknown-speech-field":
                body["privacy_safe_order_summary"]!["full_address"] = "forbidden";
                break;
        }

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0359 / K-28. A schema refusal used to carry an empty <c>details</c>, so a producer could
    /// not tell which field to fix. A missing required field is now named in <c>details.field</c>
    /// by its path: the parent's path and the missing name, which comes from IVR's own required
    /// list, not from the body.
    /// </summary>
    [Theory]
    [InlineData("root", "evidence_ref")]
    [InlineData("summary", "privacy_safe_order_summary.locale")]
    [InlineData("item", "privacy_safe_order_summary.items[0].quantity")]
    [Trait("TestId", "IT-INTAKE-SCHEMA-04")]
    public async Task AMissingRequiredFieldIsNamedInTheSchemaError(
        string scenario,
        string expectedField)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        JsonObject summary = body["privacy_safe_order_summary"]!.AsObject();
        bool removed = scenario switch
        {
            "root" => body.Remove("evidence_ref"),
            "summary" => summary.Remove("locale"),
            "item" => summary["items"]![0]!.AsObject().Remove("quantity"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        Assert.True(removed);

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(expectedField, await ErrorFieldAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0359 / K-28. A bad value is named by its own path - inside a nested object, at an array
    /// index, or on a field only the serializer checks, whose path it reports - and never by the
    /// value. The payment case is one half of a two-field rule, reported against the field that
    /// breaks it for the program sent.
    /// </summary>
    [Theory]
    [InlineData("item-quantity", "privacy_safe_order_summary.items[1].quantity")]
    [InlineData("attempt-offset", "attempt_offsets_seconds[1]")]
    [InlineData("payment-for-program", "payment_method_snapshot")]
    [InlineData("phone-status", "phone_validation_status")]
    [Trait("TestId", "IT-INTAKE-SCHEMA-05")]
    public async Task AnInvalidValueIsNamedByItsPathInTheSchemaError(
        string scenario,
        string expectedField)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        switch (scenario)
        {
            case "item-quantity":
                body["privacy_safe_order_summary"]!["items"]!.AsArray().Add(new JsonObject
                {
                    ["public_name"] = "Trà hồng sâm",
                    ["quantity"] = 0,
                    ["unit_label"] = "gói",
                });
                break;
            case "attempt-offset":
                body["attempt_offsets_seconds"] = new JsonArray(0, -150);
                break;
            case "payment-for-program":
                body["payment_method_snapshot"] = "COD";
                break;
            case "phone-status":
                // Not checked by hand: the serializer's enum binding refuses it.
                body["phone_validation_status"] = "PHONE_VALID";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(expectedField, await ErrorFieldAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0359 / K-28. An unknown field is named by its parent's path, never by its own name: that
    /// name is text the producer chose, and <c>details.field</c> carries only names IVR defines.
    /// <c>$</c> is the task object itself. A dictionary key the producer chose - a pronunciation
    /// hint's - is held to the same rule, so a bad value under one is refused with no field at all
    /// rather than with the key in it.
    /// </summary>
    [Theory]
    [InlineData("root", "$")]
    [InlineData("summary", "privacy_safe_order_summary")]
    [InlineData("item", "privacy_safe_order_summary.items[0]")]
    [InlineData("hint-key", null)]
    [Trait("TestId", "IT-INTAKE-SCHEMA-06")]
    public async Task AnUnknownFieldIsNamedByItsParentAndNeverByItsOwnName(
        string scenario,
        string? expectedField)
    {
        // Lower case and underscores, so it would pass for a path step if anything built a path
        // out of the body.
        const string ProducerName = "nguyen_van_an";
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        JsonObject summary = body["privacy_safe_order_summary"]!.AsObject();
        switch (scenario)
        {
            case "root":
                body[ProducerName] = "x";
                break;
            case "summary":
                summary[ProducerName] = "x";
                break;
            case "item":
                summary["items"]![0]![ProducerName] = "x";
                break;
            case "hint-key":
                summary["pronunciation_hints"] = new JsonObject { [ProducerName] = 5 };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(expectedField, await ErrorFieldAsync(response));
        Assert.DoesNotContain(
            ProducerName,
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0354 / B16 (chief worklist 2026-09-25). total_amount is read aloud and VND has no spoken
    /// subunit, so a fraction used to pass intake and fail at dial time, where the gateway took it
    /// for a broken SIM. It is refused at the door now, before anything is stored.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-AMOUNT-16")]
    public async Task AFractionalTotalAmountIsMalformed()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        body["privacy_safe_order_summary"]!["total_amount"] = 210636.8m;

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0359 (K-30). The other side of <c>IT-INTAKE-AMOUNT-16</c>: W-0354 refuses a fraction, not
    /// a spelling. A whole number of dong written with a zero fraction or with an exponent is still
    /// whole, and is accepted. The body is checked to carry each spelling as written, because a
    /// writer that normalised it to 210636 would let this pass without ever sending the case.
    /// </summary>
    [Theory]
    [InlineData("210636.0")]
    [InlineData("2.10636E5")]
    [Trait("TestId", "IT-INTAKE-AMOUNT-17")]
    public async Task AWholeTotalAmountIsAcceptedHoweverItIsSpelled(string spelling)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();

        // A node parsed from the text keeps that text when written back; one built from a decimal
        // or a double would be written in the number's own form.
        body["privacy_safe_order_summary"]!["total_amount"] = JsonNode.Parse(spelling);
        Assert.Contains(
            string.Concat("\"total_amount\":", spelling),
            body.ToJsonString(),
            StringComparison.Ordinal);

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrTaskIntakeResult result = (await response.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
        Assert.Equal(IvrTaskIntakeResultDecision.TASK_ACCEPTED_DRY_RUN_ONLY, result.Decision);
        Assert.Equal(1, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-PRIVACY-04")]
    public async Task SemanticStreetAddressIsPiiViolationAndDoesNotLeakToAudit()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        const string forbiddenStreet = "Đường Nguyễn Huệ, Phường Bến Nghé, Quận Một";
        body["privacy_safe_order_summary"]!["delivery_area_short"] = forbiddenStreet;

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.DoesNotContain(
            app.Audit.Entries,
            entry => entry.DataJson.Contains("Nguyễn Huệ", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(false, true, HttpStatusCode.Forbidden)]
    [InlineData(true, false, HttpStatusCode.Unauthorized)]
    [Trait("TestId", "IT-INTAKE-AUTH-05")]
    public async Task SourceAndServiceAuthenticationFailBeforeIntake(
        bool includeSource,
        bool includeAuthorization,
        HttpStatusCode expected)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app.Client,
            CreateBody(),
            includeSource: includeSource,
            includeAuthorization: includeAuthorization);

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(0, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-AUTH-INGRESS-12")]
    [Trait("TestId", "SEC-AUTHZ-05")]
    public async Task ServiceJwtAuthenticatesIngressAndAnUntrustedOneDoesNot()
    {
        // W-0032 / P4-4 §2.2. The unit suite proves the validator; this proves it is actually on
        // the request path, and that the thing authenticating the caller is the signature.
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        var issuer = app.Services.GetRequiredService<MockOidcIssuer>();
        using var untrusted = new MockOidcIssuer(
            TimeProvider.System,
            app.Services.GetRequiredService<IOptions<ServiceIdentityOptions>>());

        using HttpResponseMessage accepted = await SendAsync(
            app.Client,
            CreateBody(),
            bearerOverride: issuer.Issue("sales-platform", [ServiceIdentityScopes.TaskWrite]));
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);

        // Correct shape, correct claims, wrong signer.
        using HttpResponseMessage refused = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: "idem-auth-ingress-untrusted",
            bearerOverride: untrusted.Issue(
                "sales-platform",
                [ServiceIdentityScopes.TaskWrite]));
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);

        // A verified caller lacking the surface scope is refused too.
        using HttpResponseMessage wrongScope = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: "idem-auth-ingress-scope",
            bearerOverride: issuer.Issue(
                "sales-platform",
                [ServiceIdentityScopes.AdminRead]));
        Assert.Equal(HttpStatusCode.Forbidden, wrongScope.StatusCode);

        Assert.Equal(1, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-TRACE-06")]
    public async Task MissingCorrelationHeaderReturnsStableMissingTrace()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app.Client,
            CreateBody(),
            includeCorrelation: false);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MissingTrace, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-TRACE-16")]
    public async Task MissingIdempotencyIsMissingTraceButInvalidSyntaxIsMalformed()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        using HttpResponseMessage missing = await SendAsync(
            app.Client,
            CreateBody(),
            includeIdempotency: false);
        using HttpResponseMessage invalid = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: "invalid key with spaces");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
        Assert.Equal(IvrErrorCodes.MissingTrace, await ErrorCodeAsync(missing));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(invalid));
    }

    /// <summary>
    /// The syntax both required headers are actually held to, pinned at the HTTP edge.
    /// <para>
    /// <c>TaskIntakeEndpoint.RequiredHeader</c> accepts 1..128 characters drawn from
    /// <c>[A-Za-z0-9._:-]</c> and nothing else. The handover document promised Module 3 up to 200
    /// characters and said nothing at all about the alphabet, so a producer minting base64 keys -
    /// an ordinary thing to do, and <c>+</c>, <c>/</c> and <c>=</c> all appear in base64 - would
    /// have been refused by a rule it was never shown. W-0209.
    /// </para>
    /// <para>
    /// Length and alphabet are asserted here rather than in the OpenAPI schema because the schema
    /// does not carry them yet: the <c>CorrelationId</c> and <c>IdempotencyKey</c> parameters are
    /// bare strings, while <c>GeneratedCorrelationId</c> on other routes already spells this exact
    /// rule out. Correcting that is a pinned-hash change and belongs to a reviewed re-pin.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-HEADER-07")]
    public async Task RequiredHeadersAreBoundedAtOneHundredTwentyEightSafeCharacters()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        // 128 is the last accepted length, not the first rejected one.
        using HttpResponseMessage atLimit = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: new string('k', 128));

        Assert.Equal(HttpStatusCode.OK, atLimit.StatusCode);

        // 129 - the length the handover told Module 3 it could use, up to 200.
        using HttpResponseMessage overLimit = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: new string('k', 129));

        Assert.Equal(HttpStatusCode.BadRequest, overLimit.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(overLimit));

        // Base64 padding and its two non-alphanumeric characters are outside the alphabet.
        // Nothing in the handover said so.
        using HttpResponseMessage base64Key = await SendAsync(
            app.Client,
            CreateBody(),
            idempotencyKey: "sB3+xQ/9dGVzdA==");

        Assert.Equal(HttpStatusCode.BadRequest, base64Key.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(base64Key));

        // The correlation header is held to the same rule, one layer earlier.
        using HttpResponseMessage longCorrelation = await SendAsync(
            app.Client,
            CreateBody(),
            correlationId: new string('c', 129));

        Assert.Equal(HttpStatusCode.BadRequest, longCorrelation.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(longCorrelation));

        // Only the accepted call reached the store.
        Assert.Equal(1, app.Store.CallJobCount);
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-BLOCKED-17")]
    public async Task CallRestrictionReturnsOperationalBlocked409()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        body["call_restriction"] = true;

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(IvrErrorCodes.OperationalBlocked, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
    }

    [Fact]
#pragma warning disable CA2000 // await using owns each per-fixture test application.
    [Trait("TestId", "IT-INTAKE-NEGATIVE-18")]
    public async Task EveryCanonicalDomainNegativeFixtureExecutesItsExpectedRuntimeBranch()
    {
        JsonObject catalog = JsonNode.Parse(await File.ReadAllTextAsync(
            FindRepositoryFile("seed", "sales-target-v1.sample.json")))!.AsObject();
        JsonArray tasks = catalog["tasks"]!.AsArray();
        foreach (JsonNode? fixtureNode in catalog["domain_negative"]!.AsArray())
        {
            JsonObject fixture = fixtureNode!.AsObject();
            string scenario = fixture["from"]!.GetValue<string>();
            JsonObject task = tasks.Single(node =>
                    node!["scenario"]!.GetValue<string>() == scenario)!["body"]!
                .DeepClone()
                .AsObject();
            NormalizeFixtureWindow(task);
            ApplyFixtureObject(task, fixture["replace"] as JsonObject);
            ApplyFixtureObject(task, fixture["add"] as JsonObject);
            await using TaskIntakeApiTestApplication app =
                await TaskIntakeApiTestApplication.StartAsync();
            HttpResponseMessage response;
            string fixtureId = fixture["id"]!.GetValue<string>();
            if (fixture["replay_with_same_key_different_payload"] is JsonObject changedFields)
            {
                using HttpResponseMessage first = await SendAsync(app.Client, task);
                Assert.Equal(HttpStatusCode.OK, first.StatusCode);
                JsonObject changed = task.DeepClone().AsObject();
                ApplyFixtureObject(changed, changedFields);
                response = await SendAsync(app.Client, changed);
            }
            else if (fixture["replay_identical"]?.GetValue<bool>() == true)
            {
                using HttpResponseMessage first = await SendAsync(app.Client, task);
                Assert.Equal(HttpStatusCode.OK, first.StatusCode);
                response = await SendAsync(app.Client, task);
            }
            else if (fixture["concurrent_identical_replays"] is JsonValue replayCountNode)
            {
                int replayCount = replayCountNode.GetValue<int>();
                HttpResponseMessage[] responses = await Task.WhenAll(
                    Enumerable.Range(0, replayCount)
                        .Select(_ => SendAsync(app.Client, task.DeepClone().AsObject())));
                response = responses[0];
                foreach (HttpResponseMessage extra in responses.Skip(1))
                {
                    Assert.Equal(response.StatusCode, extra.StatusCode);
                    extra.Dispose();
                }

                Assert.Equal(
                    fixture["expect_call_job_count"]!.GetValue<int>(),
                    app.Store.CallJobCount);
            }
            else
            {
                response = await SendAsync(app.Client, task);
            }

            using (response)
            {
                Assert.Equal(
                    (HttpStatusCode)fixture["expect_http"]!.GetValue<int>(),
                    response.StatusCode);
                if (fixture["expect_error_code"] is JsonValue errorCode)
                {
                    Assert.Equal(errorCode.GetValue<string>(), await ErrorCodeAsync(response));
                }
                else
                {
                    IvrTaskIntakeResult result = (await response.Content
                        .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
                    Assert.Equal(
                        fixture["expect_decision"]!.GetValue<string>(),
                        result.Decision.ToString());
                }
            }

            Assert.False(string.IsNullOrWhiteSpace(fixtureId));
        }
    }
#pragma warning restore CA2000

    /// <summary>
    /// W-0353 / P5-2 §8 <c>CT-TASK-02</c>. Every <c>schema_negative</c> fixture in the seed
    /// catalogue, sent to the endpoint.
    /// <para>
    /// <c>validate-openapi.mjs</c> proves each of these bodies is invalid against the OpenAPI
    /// schema, and <c>IT-INTAKE-NEGATIVE-18</c> sends the <c>domain_negative</c> half over the
    /// wire. Nothing sent this half. The seed tells Module 3 that a schema-invalid task answers
    /// <c>400 IVR_MALFORMED_REQUEST</c>, while <c>TaskIntakeEndpoint</c> checks the schema by
    /// hand, so the schema and the code that answers could drift apart with both checks green.
    /// </para>
    /// <para>
    /// Each source task is first sent unchanged and must be accepted. Without that, a base body
    /// that stopped being valid would get every fixture "refused" and this test would pass while
    /// testing nothing.
    /// </para>
    /// </summary>
    [Fact]
#pragma warning disable CA2000 // await using owns each per-fixture test application.
    [Trait("TestId", "CT-TASK-02")]
    public async Task EveryCanonicalSchemaNegativeFixtureIsRefusedOverTheWire()
    {
        JsonObject catalog = await ReadSeedCatalogAsync();
        JsonArray tasks = catalog["tasks"]!.AsArray();
        JsonObject[] fixtures =
        [
            .. catalog["schema_negative"]!.AsArray().Select(node => node!.AsObject()),
        ];

        foreach (string scenario in fixtures
                     .Select(fixture => fixture["from"]!.GetValue<string>())
                     .Distinct(StringComparer.Ordinal))
        {
            await using TaskIntakeApiTestApplication app =
                await TaskIntakeApiTestApplication.StartAsync();
            using HttpResponseMessage accepted = await SendAsync(
                app.Client,
                SeedTaskBody(tasks, scenario));

            Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        }

        // Compared as one list, so a failure names every fixture that answered wrongly rather
        // than stopping at the first.
        List<string> expected = [];
        List<string> observed = [];
        foreach (JsonObject fixture in fixtures)
        {
            string fixtureId = fixture["id"]!.GetValue<string>();
            await using TaskIntakeApiTestApplication app =
                await TaskIntakeApiTestApplication.StartAsync();
            using HttpResponseMessage response = await SendAsync(
                app.Client,
                FixtureBody(tasks, fixture));

            expected.Add(WireOutcome(
                fixtureId,
                fixture["expect_http"]!.GetValue<int>(),
                fixture["expect_error_code"]!.GetValue<string>(),
                callJobs: 0,
                auditEntries: 0));
            observed.Add(WireOutcome(
                fixtureId,
                (int)response.StatusCode,
                await ErrorCodeOrDecisionAsync(response),
                app.Store.CallJobCount,
                app.Audit.Entries.Count));
        }

        Assert.Equal(expected, observed);

        // A floor, not a count: a fixture added to the seed should widen this test, not break it.
        Assert.InRange(fixtures.Length, 16, int.MaxValue);
    }
#pragma warning restore CA2000

    private const string GoldenHourWithTwentyFourSevenWindow = "golden-hour-window-900s";

    /// <summary>
    /// W-0353 / P5-2 §8 <c>CT-TASK-04</c>. A task whose attempt-policy snapshot disagrees with the
    /// policy IVR resolves for it answers <c>409 IVR_POLICY_MISMATCH</c> over the wire, and writes
    /// nothing: no task, no call job, no outbox row.
    /// <para>
    /// <c>TaskIntakeService.WirePolicyMatches</c> compares three things: the attempt count, the
    /// offsets and the window length. <c>NEG-DOMAIN-POLICY-03</c> and <c>NEG-DOMAIN-POLICY-02</c>
    /// break the first two. The window length has no fixture, so the third case builds one: a
    /// Golden Hour task whose window runs 900 seconds, the 24/7 length, instead of 300. Its token
    /// expiry moves with the window, as OD-V1-17 requires, so the length is the only thing wrong
    /// with it and the answer does not depend on which check runs first. Without the length
    /// comparison this body is not accepted either: it reaches a later invariant and answers 500.
    /// </para>
    /// <para>
    /// Each fixture is looked up by id with <c>Assert.Single</c>, so deleting or renaming one fails
    /// this test instead of quietly shrinking it.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("NEG-DOMAIN-POLICY-02")]
    [InlineData("NEG-DOMAIN-POLICY-03")]
    [InlineData(GoldenHourWithTwentyFourSevenWindow)]
    [Trait("TestId", "CT-TASK-04")]
    public async Task PolicySnapshotMismatchIs409AndCreatesNothing(string policyCase)
    {
        JsonObject body = policyCase == GoldenHourWithTwentyFourSevenWindow
            ? GoldenHourBodyWithWindowSeconds(900)
            : await DomainNegativeFixtureBodyAsync(policyCase);
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(IvrErrorCodes.PolicyMismatch, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.TaskCount);
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Equal(0, app.Store.OutboxCount);
    }

    // W-0312. A number of the contract's shape, sent where the token pair used to go. Nothing in this
    // suite dials.
    private const string SentNumber = "+84900000001";

    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-01")]
    public async Task ANumberOnlyBodyIsAcceptedOverTheWire()
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        body.Remove("dial_token");
        body.Remove("dial_token_expires_at");
        body["phone_e164"] = SentNumber;

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IvrTaskIntakeResult result = (await response.Content
            .ReadFromJsonAsync<IvrTaskIntakeResult>())!;
        Assert.Equal(IvrTaskIntakeResultDecision.TASK_ACCEPTED_DRY_RUN_ONLY, result.Decision);
        Assert.Equal(1, app.Store.CallJobCount);
    }

    /// <summary>
    /// W-0312. The one anyOf on IvrConfirmationTaskV1, from the refusing side: the number with no
    /// token field beside it, or the whole token pair. Half a pair is refused even with a number,
    /// because a producer that sent half a token has a bug worth hearing about.
    /// </summary>
    [Theory]
    [InlineData("neither")]
    [InlineData("number-beside-token-only")]
    [InlineData("number-beside-expiry-only")]
    [InlineData("token-only")]
    [InlineData("expiry-only")]
    [Trait("TestId", "IT-INTAKE-NUMBER-02")]
    public async Task ABodyWithoutTheNumberOrTheWholeTokenPairIsMalformed(string shape)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        switch (shape)
        {
            case "neither":
                body.Remove("dial_token");
                body.Remove("dial_token_expires_at");
                break;
            case "number-beside-token-only":
                body.Remove("dial_token_expires_at");
                body["phone_e164"] = SentNumber;
                break;
            case "number-beside-expiry-only":
                body.Remove("dial_token");
                body["phone_e164"] = SentNumber;
                break;
            case "token-only":
                body.Remove("dial_token_expires_at");
                break;
            case "expiry-only":
                body.Remove("dial_token");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape));
        }

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
    }

    /// <summary>
    /// W-0312. draft.30 declared this pattern and nothing enforced it, so every one of these was
    /// accepted, stored in the clear, and failed only when dialled. Cases are named rather than
    /// passed as data so no phone-shaped value reaches a test report.
    /// </summary>
    [Theory]
    [InlineData("leading-zero")]
    [InlineData("missing-plus")]
    [InlineData("one-digit-short")]
    [InlineData("one-digit-long")]
    [InlineData("trailing-newline")]
    [InlineData("inner-space")]
    [InlineData("empty")]
    [InlineData("not-a-string")]
    [Trait("TestId", "IT-INTAKE-NUMBER-03")]
    public async Task ANumberThatBreaksTheContractPatternIsMalformed(string defect)
    {
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject body = CreateBody();
        body.Remove("dial_token");
        body.Remove("dial_token_expires_at");
        body["phone_e164"] = defect switch
        {
            "leading-zero" => JsonValue.Create(string.Concat("0", SentNumber[3..])),
            "missing-plus" => JsonValue.Create(SentNumber[1..]),
            "one-digit-short" => JsonValue.Create(SentNumber[..^1]),
            "one-digit-long" => JsonValue.Create(string.Concat(SentNumber, "1")),
            // .NET's $ also matches before a final newline; the contract's validator does not.
            "trailing-newline" => JsonValue.Create(string.Concat(SentNumber, "\n")),
            "inner-space" => JsonValue.Create(string.Concat(SentNumber[..6], " ", SentNumber[6..])),
            "empty" => JsonValue.Create(string.Empty),
            "not-a-string" => JsonValue.Create(84900000001L),
            _ => throw new ArgumentOutOfRangeException(nameof(defect)),
        };

        using HttpResponseMessage response = await SendAsync(app.Client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
    }

    /// <summary>
    /// W-0312. The endpoint enforces every pattern NSwag generates onto the task, so a pattern
    /// added to the contract is enforced the day it lands. What that cannot do by itself is prove
    /// the enforcement against inputs a person chose. This fails the day a second pattern appears,
    /// so it gets named refusal cases beside IT-INTAKE-NUMBER-03 before anyone relies on it.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-04")]
    public void EveryPatternOnTheGeneratedTaskHasRefusalCasesHere()
    {
        string[] patterned =
        [
            .. typeof(IvrConfirmationTaskV1)
                .GetProperties()
                .Where(property => property.GetCustomAttribute<RegularExpressionAttribute>() is not null)
                .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name)
                .Order(StringComparer.Ordinal),
        ];

        Assert.Equal(["phone_e164"], patterned);
    }

    /// <summary>
    /// W-0361 / K-41 (B11 step 0, chief worklist 2026-09-25). IT-PHONE-CONTAIN-01 follows a number
    /// from intake to the admin API on real PostgreSQL, but it enters through the service, so the
    /// HTTP surface Module 3 actually calls was never searched. These are the four answers that
    /// surface gives a number-only body - accepted, replayed, conflicting, malformed - and none of
    /// them, no log line the host wrote and no audit row may give the number back.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-PHONE-CONTAIN-02")]
    public async Task NoAnswerOfTheIntakeEndpointGivesTheNumberBack()
    {
        // The national number: every spelling of the number sent contains it.
        const string nsn = "900000001";
        await using TaskIntakeApiTestApplication app =
            await TaskIntakeApiTestApplication.StartAsync();
        JsonObject sent = CreateBody();
        sent.Remove("dial_token");
        sent.Remove("dial_token_expires_at");
        sent["phone_e164"] = SentNumber;
        JsonObject changed = (JsonObject)sent.DeepClone();
        changed["evidence_ref"] = "evidence://api/p2-1-resent";
        JsonObject malformed = (JsonObject)sent.DeepClone();
        malformed["phone_e164"] = SentNumber[1..];

        (string Case, JsonObject Body, string Key, HttpStatusCode Expected)[] cases =
        [
            ("accepted", sent, "idem-contain-02", HttpStatusCode.OK),
            ("replayed", sent, "idem-contain-02", HttpStatusCode.OK),
            ("conflicting", changed, "idem-contain-02", HttpStatusCode.Conflict),
            ("malformed", malformed, "idem-contain-02-malformed", HttpStatusCode.BadRequest),
        ];
        foreach ((string name, JsonObject body, string key, HttpStatusCode expected) in cases)
        {
            using HttpResponseMessage response = await SendAsync(app.Client, body, idempotencyKey: key);
            string answer = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == expected, $"{name} answered {(int)response.StatusCode}: {answer}");
            Assert.DoesNotContain(nsn, answer, StringComparison.Ordinal);
            Assert.DoesNotContain(
                response.Headers.Concat(response.Content.Headers).SelectMany(header => header.Value),
                value => value.Contains(nsn, StringComparison.Ordinal));
        }

        // One task, from the first send; the replay and the conflict created nothing.
        Assert.Equal(1, app.Store.TaskCount);
        Assert.DoesNotContain(
            app.Logs.Entries,
            entry => entry.Contains(nsn, StringComparison.Ordinal));
        Assert.NotEmpty(app.Audit.Entries);
        Assert.DoesNotContain(
            app.Audit.Entries,
            entry => JsonSerializer.Serialize(entry).Contains(nsn, StringComparison.Ordinal));
    }

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        JsonObject body,
        bool includeSource = true,
        bool includeAuthorization = true,
        bool includeCorrelation = true,
        bool includeIdempotency = true,
        string idempotencyKey = "idem-api-p2-1",
        string? bearerOverride = null,
        string correlationId = "corr-api-p2-1")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TaskIntakeEndpoint.Route)
        {
            Content = new StringContent(
                body.ToJsonString(),
                Encoding.UTF8,
                "application/json"),
        };
        if (includeSource)
        {
            request.Headers.Add(
                OrderCoreAllowlistMiddleware.SourceHeaderName,
                OrderCoreAllowlistOptions.SourceSystem);
        }

        if (bearerOverride is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                bearerOverride);
        }
        else if (includeAuthorization)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                FoundationApiTestApplication.ServiceToken);
        }

        if (includeCorrelation)
        {
            request.Headers.Add(CorrelationPropagationHandler.HeaderName, correlationId);
        }

        if (includeIdempotency)
        {
            request.Headers.Add(TaskIntakeEndpoint.IdempotencyHeader, idempotencyKey);
        }
        return await client.SendAsync(request);
    }

    private static JsonObject CreateBody(
        string program = "GOLDEN_HOUR",
        string payment = "ONLINE")
    {
        DateTimeOffset start = TaskIntakeApiTestApplication.Now.AddMinutes(-1);
        int windowSeconds = program == "GOLDEN_HOUR" ? 300 : 900;
        int secondOffset = program == "GOLDEN_HOUR" ? 150 : 450;
        return new JsonObject
        {
            ["contract_version"] = "ivr-order-confirmation.v1",
            ["task_id"] = string.Concat("TASK-API-", program),
            ["correlation_id"] = "corr-api-p2-1",
            ["created_at"] = start,
            ["order_id"] = string.Concat("ORDER-API-", program),
            ["order_code"] = "GF-API-001",
            ["order_code_short"] = "API001",
            ["order_version"] = "17",
            ["order_state"] = "CONFIRMING",
            ["payment_method_snapshot"] = payment,
            ["ivr_confirmation_required"] = true,
            ["is_ivr_callable"] = true,
            ["program_code"] = program,
            ["confirmation_window_started_at"] = start,
            ["confirmation_window_expires_at"] = start.AddSeconds(windowSeconds),
            ["attempt_policy_version"] = CandidateAttemptPolicies.Version,
            ["max_customer_attempts"] = 2,
            ["attempt_offsets_seconds"] = new JsonArray(0, secondOffset),
            ["phone_ref"] = "phone-ref-api-p2-1",
            ["phone_masked"] = "84xxxxx0001",
            ["phone_validation_status"] = "VALID",
            ["dial_token"] = "dial-token-api-p2-1",
            ["dial_token_expires_at"] = start.AddSeconds(windowSeconds),
            ["privacy_safe_order_summary"] = new JsonObject
            {
                ["customer_display_name"] = "chị An",
                ["order_code_short"] = "API001",
                ["items"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["public_name"] = "Nước hồng sâm",
                        ["quantity"] = 2,
                        ["unit_label"] = "hộp",
                    },
                },
                ["total_amount"] = 560_000,
                ["currency"] = "VND",
                ["delivery_area_short"] = "Phường Bến Nghé, Quận Một",
                ["program_display_name"] = program == "GOLDEN_HOUR"
                    ? "Giờ Vàng"
                    : "Bán hàng hai mươi tư trên bảy",
                ["locale"] = "vi-VN",
            },
            ["call_restriction"] = false,
            ["eligibility_snapshot"] = new JsonObject
            {
                ["decision"] = "ELIGIBLE",
                ["source"] = "api-test",
            },
            ["evidence_ref"] = "evidence://api/p2-1",
        };
    }

    private static void NormalizeFixtureWindow(JsonObject body)
    {
        DateTimeOffset start = TaskIntakeApiTestApplication.Now.AddMinutes(-1);
        int seconds = body["program_code"]!.GetValue<string>() == "GOLDEN_HOUR"
            ? 300
            : 900;
        body["created_at"] = start;
        body["confirmation_window_started_at"] = start;
        body["confirmation_window_expires_at"] = start.AddSeconds(seconds);

        // W-0312. A fixture that sends the number has no token expiry to move, and writing one
        // would turn it into the half-pair shape the contract refuses.
        if (body.ContainsKey("dial_token_expires_at"))
        {
            body["dial_token_expires_at"] = start.AddSeconds(seconds);
        }

        body["correlation_id"] = "corr-api-p2-1";
    }

    private static void ApplyFixtureObject(JsonObject body, JsonObject? changes)
    {
        if (changes is null)
        {
            return;
        }

        foreach ((string path, JsonNode? value) in changes)
        {
            string[] segments = path.Split('.');
            JsonObject owner = body;
            foreach (string segment in segments[..^1])
            {
                owner = owner[segment]!.AsObject();
            }

            owner[segments[^1]] = value?.DeepClone();
        }
    }

    private static async Task<JsonObject> ReadSeedCatalogAsync() =>
        JsonNode.Parse(await File.ReadAllTextAsync(
            FindRepositoryFile("seed", "sales-target-v1.sample.json")))!.AsObject();

    /// <summary>
    /// W-0353. A seed task by scenario, moved onto the test clock the way
    /// <c>IT-INTAKE-NEGATIVE-18</c> moves it.
    /// </summary>
    private static JsonObject SeedTaskBody(JsonArray tasks, string scenario)
    {
        JsonObject body = tasks.Single(node =>
                node!["scenario"]!.GetValue<string>() == scenario)!["body"]!
            .DeepClone()
            .AsObject();
        NormalizeFixtureWindow(body);
        return body;
    }

    /// <summary>
    /// W-0353. A seed fixture's body: its source task on the test clock, then <c>replace</c>,
    /// <c>add</c> and <c>remove</c>, in the order <c>validate-openapi.mjs</c> applies them.
    /// </summary>
    private static JsonObject FixtureBody(JsonArray tasks, JsonObject fixture)
    {
        JsonObject body = SeedTaskBody(tasks, fixture["from"]!.GetValue<string>());
        ApplyFixtureObject(body, fixture["replace"] as JsonObject);
        ApplyFixtureObject(body, fixture["add"] as JsonObject);
        if (fixture["remove"] is JsonValue removed)
        {
            RemoveFixturePath(body, removed.GetValue<string>());
        }

        return body;
    }

    /// <summary>
    /// W-0353. The <c>remove</c> half of a fixture: one dotted path, deleted. A path that is not on
    /// the body fails here rather than removing nothing, because a fixture that deletes a field
    /// the task never had is really sending the unchanged task.
    /// </summary>
    private static void RemoveFixturePath(JsonObject body, string path)
    {
        string[] segments = path.Split('.');
        JsonObject owner = body;
        foreach (string segment in segments[..^1])
        {
            owner = owner[segment]!.AsObject();
        }

        Assert.True(
            owner.Remove(segments[^1]),
            string.Concat(path, " is not on the fixture's source task."));
    }

    private static async Task<JsonObject> DomainNegativeFixtureBodyAsync(string fixtureId)
    {
        JsonObject catalog = await ReadSeedCatalogAsync();
        JsonObject fixture = Assert.Single(
            catalog["domain_negative"]!.AsArray().Select(node => node!.AsObject()),
            candidate => candidate["id"]!.GetValue<string>() == fixtureId);
        return FixtureBody(catalog["tasks"]!.AsArray(), fixture);
    }

    /// <summary>
    /// W-0353. A Golden Hour body whose confirmation window, and the dial token that expires with
    /// it, runs <paramref name="seconds"/> instead of the 300 its policy snapshot declares.
    /// </summary>
    private static JsonObject GoldenHourBodyWithWindowSeconds(int seconds)
    {
        JsonObject body = CreateBody("GOLDEN_HOUR", "ONLINE");
        DateTimeOffset start = body["confirmation_window_started_at"]!.GetValue<DateTimeOffset>();
        body["confirmation_window_expires_at"] = start.AddSeconds(seconds);
        body["dial_token_expires_at"] = start.AddSeconds(seconds);
        return body;
    }

    private static string WireOutcome(
        string fixtureId,
        int status,
        string code,
        int callJobs,
        int auditEntries) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{fixtureId}: {status} {code}, call jobs {callJobs}, audit entries {auditEntries}");

    private static string FindRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = segments.Aggregate(directory.FullName, Path.Combine);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(Path.Combine(segments));
    }

    private static async Task<string> ErrorCodeAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("error").GetProperty("code").GetString()!;
    }

    /// <summary>
    /// W-0359 / K-28. <c>details.field</c> of an error envelope, or null when it names no field.
    /// </summary>
    private static async Task<string?> ErrorFieldAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement error = body.RootElement.GetProperty("error");
        return error.TryGetProperty("details", out JsonElement details)
            && details.TryGetProperty("field", out JsonElement field)
                ? field.GetString()
                : null;
    }

    /// <summary>
    /// W-0353. The error code of an error envelope, or the decision of an accepted task, so a
    /// body that is wrongly accepted shows up in a comparison as what it became instead of as a
    /// parse failure.
    /// </summary>
    private static async Task<string> ErrorCodeOrDecisionAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.TryGetProperty("error", out JsonElement error)
            ? error.GetProperty("code").GetString()!
            : body.RootElement.GetProperty("decision").GetString()!;
    }
}
