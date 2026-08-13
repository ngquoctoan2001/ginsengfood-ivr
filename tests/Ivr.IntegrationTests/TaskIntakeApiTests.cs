using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivr.Api.Auth;
using Ivr.Api.Intake;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Intake;

namespace Ivr.IntegrationTests;

public sealed class TaskIntakeApiTests
{
    [Theory]
    [InlineData("GOLDEN_HOUR", "ONLINE")]
    [InlineData("TWENTY_FOUR_SEVEN", "COD")]
    [Trait("TestId", "IT-INTAKE-HAPPY-01")]
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
    public async Task SchemaViolationsReturnEndpointSpecific422(string scenario)
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

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(IvrErrorCodes.MalformedRequest, await ErrorCodeAsync(response));
        Assert.Equal(0, app.Store.CallJobCount);
        Assert.Empty(app.Audit.Entries);
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

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        JsonObject body,
        bool includeSource = true,
        bool includeAuthorization = true,
        bool includeCorrelation = true)
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

        if (includeAuthorization)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                FoundationApiTestApplication.ServiceToken);
        }

        if (includeCorrelation)
        {
            request.Headers.Add(CorrelationPropagationHandler.HeaderName, "corr-api-p2-1");
        }

        request.Headers.Add(TaskIntakeEndpoint.IdempotencyHeader, "idem-api-p2-1");
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

    private static async Task<string> ErrorCodeAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("error").GetProperty("code").GetString()!;
    }
}
