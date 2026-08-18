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
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Intake;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

    [Fact]
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

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        JsonObject body,
        bool includeSource = true,
        bool includeAuthorization = true,
        bool includeCorrelation = true,
        bool includeIdempotency = true,
        string idempotencyKey = "idem-api-p2-1",
        string? bearerOverride = null)
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
            request.Headers.Add(CorrelationPropagationHandler.HeaderName, "corr-api-p2-1");
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
        body["dial_token_expires_at"] = start.AddSeconds(seconds);
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
}
