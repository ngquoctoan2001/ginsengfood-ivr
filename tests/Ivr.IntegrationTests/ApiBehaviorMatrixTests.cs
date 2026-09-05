using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Scripts;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Intake;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivr.IntegrationTests;

/// <summary>W-0197: HTTP behavior, not substitute-host route parity. Fixtures are synthetic.</summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ApiBehaviorMatrixTests(PostgresPersistenceFixture fixture)
{
    private const string Prefix = "/v1/ivr/order-confirmation";
    private const string Reason = "Matrix synthetic rehearsal";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly List<JsonObject> observations = [];
    private readonly List<string> failures = [];
    private WebApplicationFactory<Program> application = null!;
    private HttpClient client = null!;
    private JsonObject inventory = null!;
    private string repository = null!;

    [Fact]
    [Trait("TestId", "IT-API-MATRIX-38")]
    public async Task AllOpenApiOperationsHaveExecutedBehaviorEvidence()
    {
        repository = FindRepository();
        inventory = JsonNode.Parse(await RunNodeAsync("deploy/ci/scripts/api-behavior-matrix.mjs"))!.AsObject();
        await fixture.ResetAsync();
        await using WebApplicationFactory<Program> baseline = new();
        using var databaseDiagnostics = new ApiMatrixDatabaseDiagnostics();
        await using WebApplicationFactory<Program> app = baseline.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.AddProvider(databaseDiagnostics));
            builder.UseEnvironment("Development");
            builder.UseSetting("IVR_EXECUTION_MODE", "MOCK");
            builder.UseSetting("SALES_PROVIDER", "FAKE_TARGET_V1");
            builder.UseSetting("SIM_PROVIDER", "MOCK");
            builder.UseSetting("REAL_CUSTOMER_CALL_ALLOWED", "NO");
            builder.UseSetting("ConnectionStrings:IvrDb", fixture.ConnectionString);
            builder.UseSetting(OrderCoreAllowlistOptions.TokenConfigurationKey, FoundationApiTestApplication.ServiceToken);
            builder.UseSetting(InternalServiceOptions.TokenConfigurationKey, InternalAdminApiTestApplication.InternalToken);
            builder.UseSetting(AdminAccessOptions.ReadTokenConfigurationKey, TestAdminTokens.Read);
            builder.UseSetting(AdminAccessOptions.WriteTokenConfigurationKey, TestAdminTokens.Write);
            builder.UseSetting(AdminAccessOptions.DangerTokenConfigurationKey, TestAdminTokens.Danger);
            builder.UseSetting("Ivr:DevTooling:SeedDirectory", Path.Combine(repository, "seed"));
        });
        application = app;
        using HttpClient http = app.CreateClient();
        client = http;
        string[] routes = app.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .SelectMany(endpoint => (endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(method => $"{method} {endpoint.RoutePattern.RawText?.TrimEnd('/')}"))
            .Where(route => route.Contains(Prefix, StringComparison.Ordinal)).Order().ToArray();
        JsonArray operations = inventory["operations"]!.AsArray();
        foreach (JsonNode? node in operations)
        {
            JsonObject operation = node!.AsObject();
            string id = operation["id"]!.GetValue<string>();
            string route = $"{operation["method"]} {Prefix}{operation["path"]}";
            if (!routes.Contains(route)) failures.Add($"{id}: runtime route missing: {route}");
            try
            {
                await fixture.ResetAsync();
                await new ApiMatrixFixture(fixture).SeedGraphAsync(
                    includeTerminalResult: id != "technicalRetry", eligible: id != "recordEligibility");
                await PrepareAsync(id);
                await ExerciseAsync(operation);
            }
            catch (Exception exception)
            {
                failures.Add($"{id}: harness {exception.GetType().Name}: {exception.Message}");
            }
        }

        JsonArray resultCodes = await ExerciseResultCodesAsync();
        JsonObject report = new()
        {
            ["schema_version"] = "ivr.api-behavior-matrix.v1",
            ["generated_at"] = DateTimeOffset.UtcNow,
            ["composition_root"] = "Ivr.Api.Program",
            ["database"] = "isolated Testcontainers PostgreSQL 16; migrated per operation",
            ["safety"] = "MOCK/MOCK/NO; synthetic data; no worker or vendor calls",
            ["inventory"] = inventory.DeepClone(),
            ["result_codes"] = resultCodes,
            ["runtime_routes"] = JsonSerializer.SerializeToNode(routes),
            ["cases"] = new JsonArray(observations.Select(item => (JsonNode)item).ToArray()),
            ["behavior_failures"] = JsonSerializer.SerializeToNode(failures),
            ["internal_db_sql_states"] = JsonSerializer.SerializeToNode(databaseDiagnostics.SqlStates),
        };
        string output = Path.Combine(repository, ".artifacts", "api-matrix", "http-observations.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        await File.WriteAllTextAsync(output, report.ToJsonString(JsonOptions));
        // Schema/allowlist and complete-coverage checks are part of the test, not an optional
        // manual follow-up that CI could forget to run.
        await RunNodeAsync("deploy/ci/scripts/verify-api-behavior-matrix.mjs");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private async Task ExerciseAsync(JsonObject operation)
    {
        string id = operation["id"]!.GetValue<string>();
        string path = ConcretePath(operation["path"]!.GetValue<string>());
        JsonObject? body = Body(id);
        bool write = operation["method"]!.GetValue<string>() != "GET";
        string key = $"matrix-{id}";
        await ObserveAsync(operation, "auth_missing", path, body, "missing", [401]);
        await ObserveAsync(operation, "auth_invalid", path, body, "invalid", [IsInternal(id) || id == "intakeTask" ? 403 : 401]);
        await ObserveAsync(operation, "auth_wrong_tier", path, body, "tier", [!IsInternal(id) && !write ? 401 : 403]);
        await ObserveAsync(operation, "scope_wrong", path, body, "scope", [403]);
        if (write)
        {
            await ObserveAsync(operation, "malformed_json", path, body, "malformed", [400]);
            await ObserveAsync(operation, "malformed_body", path, new JsonObject(), "normal", [400, 422]);
        }
        else
        {
            JsonObject? query = operation["parameters"]!.AsArray().Select(p => p!.AsObject())
                .FirstOrDefault(p => p["in"]!.GetValue<string>() == "query"
                    && (p["schema"]?["type"]?.ToString() is "integer" or "boolean"
                        || p["schema"]?["format"]?.ToString() == "date-time"));
            if (query is not null)
                await ObserveAsync(operation, "malformed_query", path + (path.Contains('?') ? "&" : "?") + query["name"] + "=not-valid", null, "normal", [400, 422]);
            else if (id == "getIntegrationStatus")
                await ObserveAsync(operation, "malformed_query", path + "?environment=invalid-environment", null, "normal", [400]);
            else
                NotApplicable(id, "malformed_body_query", "GET declares no body or typed query; malformed correlation header is exercised separately.");
        }
        await ObserveAsync(operation, "correlation_malformed", path, body, "correlation", [400, 404]);
        await ObserveAsync(operation, "correlation_missing", path, body, "missing-correlation",
            [id.Contains("FeatureFlag", StringComparison.Ordinal) ? 200 : id == "intakeTask" ? 422 : 400]);
        if (path.Contains("JOB-P2-8", StringComparison.Ordinal) || path.Contains("SIM-P2-8", StringComparison.Ordinal)
            || path.Contains("v-matrix", StringComparison.Ordinal) || path.Contains("SCN-001", StringComparison.Ordinal)
            || path.Contains("STATUS-all-up", StringComparison.Ordinal) || path.Contains("feature-flags/dev", StringComparison.Ordinal))
        {
            string missing = path.Replace("JOB-P2-8", "missing-job").Replace("SIM-P2-8", "missing-sim")
                .Replace("v-matrix", "missing-version").Replace("SCN-001-confirm", "missing-scenario")
                .Replace("STATUS-all-up", "missing-profile").Replace("feature-flags/dev", "feature-flags/missing");
            await ObserveAsync(operation, "not_found", missing, body, "normal", [404]);
        }
        else if (id is "recordEligibility" or "createCallJob" or "recordAttempt" or "recordResult" or "recordResultCallback"
            or "technicalRetry" or "adminReview")
        {
            JsonObject missing = body!.DeepClone().AsObject();
            missing[missing.First().Key] = "missing-resource";
            await ObserveAsync(operation, "not_found", path, missing, "normal", [404]);
        }
        else if (!write)
            NotApplicable(id, "not_found_conflict", "Collection/singleton projection has no resource lookup or mutation; HTTP 404/409 is not a contract branch.");

        JsonObject happy = await ObserveAsync(operation, "happy", path, body, "normal", [200], key);
        CheckHappySemantics(id, happy);
        JsonNode? state = write ? await MutationStateAsync() : null;
        if (id == "createScriptDraft")
            await ObserveAsync(operation, "resource_conflict", path, body, "normal", [409]);
        if (write)
        {
            JsonObject retry = await ObserveAsync(operation, "retry_same_key", path, body, "normal", [200], key);
            JsonObject reordered = new();
            foreach (var field in body!.Reverse()) reordered[field.Key] = field.Value?.DeepClone();
            JsonObject replay = await ObserveAsync(operation, "replay_same_payload", path, reordered, "normal", [200], key);
            bool unchanged = JsonNode.DeepEquals(state, await MutationStateAsync());
            replay["persisted_state_unchanged"] = unchanged;
            if (!unchanged) failures.Add(id + ": retry/replay changed persisted business state");
            if (id == "dryRunDevScenario")
            {
                NotApplicable(id, "payload_conflict", "POST is a pure scenario simulation, not a write; no idempotency key in OpenAPI. Repeats are compared excluding generated_at/correlation_id.");
                CompareReplay(id, happy, retry, true);
                CompareReplay(id, happy, replay, true);
            }
            else
            {
                CompareReplay(id, happy, retry, false);
                CompareReplay(id, happy, replay, false);
                JsonObject changed = body!.DeepClone().AsObject();
                string field = id == "intakeTask" ? "order_code" : changed.ContainsKey("reason") ? "reason" : changed.First().Key;
                changed[field] = changed[field]!.GetValue<string>() + "-changed";
                await ObserveAsync(operation, "payload_conflict", path, changed, "normal", [409], key);
            }
        }
        else NotApplicable(id, "idempotency", "Read-only GET has no mutation or Idempotency-Key contract.");
    }

    private async Task<JsonObject> ObserveAsync(JsonObject op, string name, string path, JsonObject? body,
        string mode, int[] expected, string? key = null)
    {
        string id = op["id"]!.GetValue<string>();
        string correlation = id == "intakeTask" && body?["correlation_id"] is not null
            ? body["correlation_id"]!.GetValue<string>() : "corr-matrix-" + string.Join('-', Guid.NewGuid().ToString("N").Chunk(8).Select(part => new string(part)));
        using HttpRequestMessage request = Request(op, path, body, mode, key ?? $"matrix-{Guid.NewGuid():N}", correlation);
        using HttpResponseMessage response = await client.SendAsync(request);
        string raw = await response.Content.ReadAsStringAsync();
        JsonNode? parsed = null;
        try { parsed = JsonNode.Parse(raw); } catch (JsonException) { }
        string? returnedCorrelation = response.Headers.TryGetValues("X-Correlation-Id", out IEnumerable<string>? values) ? values.SingleOrDefault() : null;
        bool statusOk = expected.Contains((int)response.StatusCode);
        if (name == "payload_conflict") statusOk &= parsed?["error"]?["code"]?.ToString() == "IVR_IDEMPOTENCY_CONFLICT";
        bool correlationOk = mode is "correlation" or "missing-correlation"
            ? !string.IsNullOrWhiteSpace(returnedCorrelation) && returnedCorrelation != "unsafe value"
            : returnedCorrelation == correlation;
        JsonObject observation = new()
        {
            ["operation_id"] = id,
            ["case"] = name,
            ["method"] = op["method"]!.DeepClone(),
            ["path"] = path,
            ["expected_status"] = JsonSerializer.SerializeToNode(expected),
            ["status"] = (int)response.StatusCode,
            ["status_pass"] = statusOk,
            ["correlation_pass"] = correlationOk,
            ["content_type"] = response.Content.Headers.ContentType?.MediaType,
            ["request_correlation_id"] = correlation,
            ["response_correlation_id"] = returnedCorrelation,
            ["response"] = parsed,
            ["response_is_json"] = parsed is not null,
        };
        observations.Add(observation);
        if (!statusOk || !correlationOk) failures.Add($"{id}/{name}: HTTP {(int)response.StatusCode}, correlation={correlationOk}, body={raw}");
        return observation;
    }

    private HttpRequestMessage Request(JsonObject op, string path, JsonObject? body, string mode, string key, string correlation)
    {
        string id = op["id"]!.GetValue<string>();
        bool internalService = IsInternal(id);
        bool intake = id == "intakeTask";
        HttpRequestMessage request = new(new HttpMethod(op["method"]!.GetValue<string>()), Prefix + path);
        if (internalService || intake)
        {
            request.Headers.Authorization = new("Bearer", intake ? FoundationApiTestApplication.ServiceToken : InternalAdminApiTestApplication.InternalToken);
            request.Headers.Add("X-Source-System", intake ? "order-core" : "ivr-worker");
            request.Headers.Add("X-Service-Scope", internalService ? InternalServiceOptions.RequiredScope : "ivr.task.write");
        }
        else
        {
            AdminScope tier = Tier(id, request.Method);
            TestAdminTokens.Authorize(request, tier, id == "approveScriptVersion" ? "matrix-approver" : "matrix-author", Reason);
            request.Headers.Add(AdminTokenAuthenticationHandler.ScriptPermissionsHeaderName, ScriptPermissionSets.Full);
        }
        if (mode == "missing") request.Headers.Authorization = null;
        if (mode == "invalid") request.Headers.Authorization = new("Bearer", "synthetic-invalid-token");
        if (mode == "tier") request.Headers.Authorization = new("Bearer", internalService || intake ? TestAdminTokens.Danger
            : Tier(id, request.Method) == AdminScope.Read ? InternalAdminApiTestApplication.InternalToken : TestAdminTokens.Read);
        if (mode == "scope")
        {
            request.Headers.Remove("X-Service-Scope");
            request.Headers.Add("X-Service-Scope", "ivr.unrelated");
            if (intake) request.Headers.Authorization = new("Bearer", application.Services.GetRequiredService<MockOidcIssuer>()
                .Issue("matrix-sales", [ServiceIdentityScopes.AdminRead]));
        }
        if (mode != "missing-correlation") request.Headers.Add("X-Correlation-Id", mode == "correlation" ? "unsafe value" : correlation);
        request.Headers.Add("Idempotency-Key", key);
        if (request.Method != HttpMethod.Get)
            request.Content = new StringContent(mode == "malformed" ? "{" : body!.ToJsonString(), Encoding.UTF8, "application/json");
        return request;
    }

    private async Task PrepareAsync(string id)
    {
        if (id == "intakeTask") await RegisterPoliciesAsync();
        IFeatureFlagStore store = application.Services.GetRequiredService<IFeatureFlagStore>();
        FeatureFlagSnapshot current = await store.ReadFreshAsync("dev");
        await store.ApplyAuditedAsync(current, FeatureFlagSnapshot.SafeDefault("dev") with
        {
            Revision = current.Revision + 1,
            GlobalDialKillSwitch = id != "technicalRetry",
            LabDestinationAllowlist = id == "technicalRetry" ? new HashSet<string> { "phone-ref-p2-8" } : new HashSet<string>(),
        }, new AuditEvent("matrix-fixture", "matrix-setup", "feature-flags:dev", Reason, "corr-matrix-setup", new Dictionary<string, object?>()));
        if (id is "terminateCallJob" or "terminateAllCallJobs")
        {
            await using IvrDbContext db = await fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>().CreateDbContextAsync();
            var attempt = await db.CallAttempts.SingleAsync();
            attempt.EndedAt = null;
            attempt.StartedAt = DateTimeOffset.UtcNow;
            attempt.ProviderCallId = "mock-matrix-call";
            await db.SaveChangesAsync();
        }
        if (id is "enableSim" or "applyDevIntegrationProfile")
        {
            await using IvrDbContext db = await fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>().CreateDbContextAsync();
            (await db.SimChannels.SingleAsync()).Enabled = false;
            await db.SaveChangesAsync();
        }
        if (id == "resumeQueue")
        {
            JsonObject pause = inventory["operations"]!.AsArray().Single(op => op!["id"]!.ToString() == "pauseQueue")!.AsObject();
            await ObserveAsync(pause, "setup_resumeQueue", "/queue:pause", Body("pauseQueue"), "normal", [200]);
        }
        if (id.StartsWith("getAnalytics", StringComparison.Ordinal) || id == "exportAnalytics")
            await new ApiMatrixFixture(fixture).SeedAnalyticsBucketAsync();
        if (id is "getScriptVersion" or "submitScriptForReview" or "approveScriptVersion" or "retireScriptVersion")
        {
            JsonObject draft = inventory["operations"]!.AsArray().Single(op => op!["id"]!.ToString() == "createScriptDraft")!.AsObject();
            await ObserveAsync(draft, "setup_" + id, "/scripts", ApiMatrixFixture.Draft(), "normal", [200]);
            if (id is "approveScriptVersion" or "retireScriptVersion")
            {
                JsonObject submit = inventory["operations"]!.AsArray().Single(op => op!["id"]!.ToString() == "submitScriptForReview")!.AsObject();
                await ObserveAsync(submit, "setup_" + id, ConcretePath(submit["path"]!.ToString()), Body("submitScriptForReview"), "normal", [200]);
            }
        }
    }

    private void CompareReplay(string id, JsonObject first, JsonObject next, bool simulation)
    {
        JsonNode? left = first["response"]?.DeepClone();
        JsonNode? right = next["response"]?.DeepClone();
        if (simulation)
            foreach (string key in new[] { "generated_at", "correlation_id" }) { left?.AsObject().Remove(key); right?.AsObject().Remove(key); }
        bool same = JsonNode.DeepEquals(left, right);
        next["replay_equal"] = same;
        if (!same) failures.Add($"{id}/{next["case"]}: replay response differs");
    }

    private async Task<JsonArray> ExerciseResultCodesAsync()
    {
        JsonArray results = [];
        await fixture.ResetAsync();
        await new ApiMatrixFixture(fixture).SeedGraphAsync();
        await RegisterPoliciesAsync();
        JsonObject operation = inventory["operations"]!.AsArray().Single(op => op!["id"]!.ToString() == "recordResult")!.AsObject();
        foreach (IvrResultType type in Enum.GetValues<IvrResultType>())
        {
            bool runtime = ResultContractPolicy.IsRuntimeResult(type);
            bool counted = ResultContractPolicy.IsCountedCustomerAttemptResult(type);
            bool final = ResultContractPolicy.IsFinalCallbackResult(type);
            CoreActionRecommendation action = type switch
            {
                IvrResultType.IvrConfirmed => CoreActionRecommendation.RevalidateAndConfirmOrder,
                IvrResultType.IvrCustomerCancelled => CoreActionRecommendation.RevalidateAndCancelCustomerRequest,
                IvrResultType.IvrNoAnswerAttempt or IvrResultType.IvrNoAnswerFinal or IvrResultType.IvrWrongInput => CoreActionRecommendation.NoStateChangeWaitForTimeout,
                IvrResultType.IvrConfirmationWindowExpired => CoreActionRecommendation.RevalidateAndExpireConfirmation,
                _ => CoreActionRecommendation.RevalidateAndHoldAdminReview,
            };
            string wire = new NormalizedResult(type, counted, final, "MATRIX", null, null, action, false, false, 0).ResultStatus;
            bool defined = inventory["result_codes"]!.AsArray().Any(code => code!.ToString() == wire);
            JsonObject evidence = new() { ["wire"] = wire, ["in_openapi"] = defined, ["runtime_result"] = runtime };
            if (runtime)
            {
                _ = ConstructResult(type, counted, final, action);
                await using IvrDbContext db = await fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>().CreateDbContextAsync();
                var result = await db.CallResults.SingleAsync();
                result.ResultType = wire;
                result.FinalResultStatus = wire;
                result.IsCountedCustomerAttempt = counted;
                result.IsFinalForIvr = final;
                result.RecommendedCoreAction = string.Concat(action.ToString().Select((character, index) => index > 0 && char.IsUpper(character) ? "_" + character : character.ToString())).ToUpperInvariant();
                await db.SaveChangesAsync();
                JsonObject observed = await ObserveAsync(operation, "wire_" + wire, "/call-results", Body("recordResult"), "normal", [200]);
                bool emitted = observed["response"]?["result_type"]?.ToString() == wire;
                evidence["http_status"] = observed["status"]!.DeepClone();
                evidence["wire_matches"] = emitted;
                if (!defined || !emitted) failures.Add("Wire result mismatch: " + wire);
            }
            else
            {
                bool refused = false;
                try { _ = ConstructResult(type, false, false, action); }
                catch (InvalidOperationException) { refused = true; }
                evidence["call_result_construction_rejected"] = refused;
                evidence["usage"] = "pre-call compatibility code only; never construct a CallResultSnapshot";
                if (!defined || !refused) failures.Add("Pre-call code admitted as result: " + wire);
            }
            results.Add(evidence);
        }
        JsonObject intake = inventory["operations"]!.AsArray().Single(op => op!["id"]!.ToString() == "intakeTask")!.AsObject();
        foreach (string wire in results.Where(row => !row!["runtime_result"]!.GetValue<bool>())
            .Select(row => row!["wire"]!.ToString()))
        {
            JsonObject restricted = ApiMatrixFixture.CreateBody();
            bool policy = wire == "IVR_POLICY_BLOCKED";
            restricted["task_id"] = policy ? "TASK-MATRIX-PRECALL-POLICY" : "TASK-MATRIX-PRECALL-OPERATIONAL";
            if (policy) restricted["attempt_policy_version"] = "matrix-missing-policy";
            else restricted["call_restriction"] = true;
            JsonObject observed = await ObserveAsync(intake, policy ? "pre_call_policy_hold" : "pre_call_restriction",
                "/tasks", restricted, "normal", [policy ? 200 : 409]);
            await using IvrDbContext check = await fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>().CreateDbContextAsync();
            bool noCall = !await check.CallJobs.AnyAsync(job => job.TaskId == restricted["task_id"]!.ToString())
                && !await check.CallAttempts.AnyAsync(attempt => attempt.TaskId == restricted["task_id"]!.ToString());
            JsonNode evidence = results.Single(row => row!["wire"]!.ToString() == wire)!;
            evidence["pre_call_no_job_or_attempt"] = noCall;
            evidence["pre_call_http_status"] = observed["status"]!.DeepClone();
            evidence["pre_call_error_code"] = observed["response"]?["error"]?["code"]?.DeepClone();
            evidence["pre_call_decision"] = observed["response"]?["decision"]?.DeepClone();
            bool outcome = policy
                ? observed["response"]?["decision"]?.ToString() == TaskIntakeDecisions.HeldPolicyMissing
                : observed["response"]?["error"]?["code"]?.ToString() == "IVR_OPERATIONAL_BLOCKED";
            evidence["pre_call_outcome_pass"] = outcome;
            if (!outcome) failures.Add(wire + ": pre-call outcome differs from the contract");
            if (!noCall) failures.Add(wire + ": rejected intake created a call job or attempt");
        }
        JsonObject invalidContact = ApiMatrixFixture.CreateBody();
        invalidContact["task_id"] = "TASK-MATRIX-PRECALL-CONTACT";
        invalidContact["phone_validation_status"] = "PHONE_VALID";
        await ObserveAsync(intake, "pre_call_contact_invalid", "/tasks", invalidContact, "normal", [422]);
        return results;
    }

    private async Task RegisterPoliciesAsync()
    {
        IAttemptPolicyRegistryWriter writer = application.Services.GetRequiredService<IAttemptPolicyRegistryWriter>();
        foreach (AttemptPolicySnapshot policy in CandidateAttemptPolicies.Create())
            await writer.RegisterNewVersionAsync(policy, [ExecutionMode.Mock], "matrix-fixture", Reason, "corr-matrix-policies", CancellationToken.None);
    }

    private static CallResultSnapshot ConstructResult(IvrResultType type, bool counted, bool final, CoreActionRecommendation action) =>
        CallResultSnapshot.Create(CallbackId.Create("matrix-callback"), TaskId.Create("matrix-task"),
            OrderId.Create("matrix-order"), OrderVersion.Create("v-matrix"), IvrProgramCode.GoldenHour,
            type, "Matrix synthetic result", counted, final, 1, DateTimeOffset.UtcNow, action,
            EvidenceReference.Create("evidence://matrix/result"), AuditReference.Create("audit://matrix/result"));

    private void CheckHappySemantics(string id, JsonObject observation)
    {
        JsonNode? body = observation["response"];
        bool pass = id switch
        {
            "intakeTask" => body?["decision"]?.ToString() == "TASK_ACCEPTED_DRY_RUN_ONLY" && body["ivr_call_job_id"] is not null,
            "recordEligibility" => body?["decision"]?.ToString() == "ELIGIBLE_FOR_IVR",
            "loadDevSeed" => body?["task_count"]?.GetValue<int>() == 9 && body["accepted_count"]?.GetValue<int>() == 8,
            "dryRunDevScenario" => body?["matches"]?.GetValue<bool>() == true && body["actual_result_type"]?.ToString() == "IVR_CONFIRMED",
            "applyDevIntegrationProfile" => body?["effects"]?.AsArray().Any(effect => effect?["detail"]?.ToString() == "1 channel(s) enabled") == true,
            "createScriptDraft" => body?["version"]?["status"]?.ToString() == "DRAFT",
            "submitScriptForReview" => body?["version"]?["status"]?.ToString() == "IN_REVIEW",
            "approveScriptVersion" => body?["version"]?["status"]?.ToString() == "APPROVED",
            "retireScriptVersion" => body?["version"]?["status"]?.ToString() == "RETIRED",
            "technicalRetry" => body?["technical_retry_count"]?.GetValue<int>() == 1 && body["queue_status"]?.ToString() == "HELD_MOCK",
            "adminReview" => body?["status"]?.ToString() == "RESOLVED",
            "exportAnalytics" => body?["rows"]?.AsArray().Count > 0,
            "getAnalyticsSummary" => body?["kpi"]?["total_results"]?.GetValue<int>() >= 5,
            "listCallJobs" or "listReviewItems" => body?["items"]?.AsArray().Count > 0,
            _ => observation["status"]?.GetValue<int>() == 200,
        };
        observation["happy_semantics_pass"] = pass;
        if (!pass) failures.Add(id + ": happy response did not reach its expected business outcome");
    }

    private async Task<JsonNode> MutationStateAsync()
    {
        await using IvrDbContext db = await fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>().CreateDbContextAsync();
        return JsonSerializer.SerializeToNode(new
        {
            tasks = await db.ConfirmationTasks.CountAsync(),
            jobs = await db.CallJobs.CountAsync(),
            attempts = await db.CallAttempts.OrderBy(row => row.IvrCallAttemptId).Select(row => new
            { row.IvrCallAttemptId, row.Status, row.TechnicalRetryCount, row.TerminationRequestedAt, row.TerminationReason }).ToArrayAsync(),
            results = await db.CallResults.CountAsync(),
            callbacks = await db.ResultCallbacks.CountAsync(),
            actions = await db.AdminActions.CountAsync(),
            audits = await db.AuditLog.CountAsync(),
            scripts = await db.ScriptVersions.CountAsync(),
            holds = await db.CapacityIncidents.OrderBy(row => row.CapacityIncidentId).Select(row => new { row.CapacityIncidentId, row.Status, row.HoldNewCalls }).ToArrayAsync(),
            channels = await db.SimChannels.OrderBy(row => row.SimChannelId).Select(row => new { row.SimChannelId, row.Enabled, row.Status }).ToArrayAsync(),
        })!;
    }

    private void NotApplicable(string id, string name, string reason) => observations.Add(new JsonObject
    { ["operation_id"] = id, ["case"] = name, ["applicability"] = "NOT_APPLICABLE", ["reason"] = reason });

    private static bool IsInternal(string id) => id is "recordEligibility" or "createCallJob" or "getCallJob" or "recordAttempt" or "recordResult" or "recordResultCallback";
    private static AdminScope Tier(string id, HttpMethod method) => method == HttpMethod.Get ? AdminScope.Read
        : id is "mutateFeatureFlags" or "terminateAllCallJobs" or "terminateCallJob" or "pauseQueue" or "resumeQueue" or "disableSim" or "enableSim" or "technicalRetry"
            ? AdminScope.Danger : AdminScope.Write;
    private static string ConcretePath(string path) => path.Replace("{ivrCallJobId}", "JOB-P2-8").Replace("{simChannelId}", "SIM-P2-8")
        .Replace("{environment}", "dev").Replace("{templateId}", TargetV1SpeechPolicy.MockTemplateId).Replace("{version}", "v-matrix")
        .Replace("{scenarioId}", "SCN-001-confirm").Replace("{profileId}", "STATUS-all-up")
        + (path == "/analytics/export" ? "?reason=Matrix%20synthetic%20export" : "");
    private static JsonObject? Body(string id) => id switch
    {
        "intakeTask" => ApiMatrixFixture.CreateBody(),
        "recordEligibility" => new() { ["task_id"] = "TASK-P2-8" },
        "createCallJob" => new() { ["ivr_call_job_id"] = "JOB-P2-8", ["task_id"] = "TASK-P2-8" },
        "recordAttempt" => new() { ["ivr_call_attempt_id"] = "ATTEMPT-P2-8", ["ivr_call_job_id"] = "JOB-P2-8" },
        "recordResult" => new() { ["ivr_call_result_id"] = "RESULT-P2-8", ["ivr_call_job_id"] = "JOB-P2-8" },
        "recordResultCallback" => new() { ["callback_id"] = "CALLBACK-P2-8", ["ivr_call_result_id"] = "RESULT-P2-8" },
        "createScriptDraft" => ApiMatrixFixture.Draft(),
        "approveScriptVersion" => new() { ["approval_type"] = "MOCK_TEST", ["reason"] = Reason },
        "technicalRetry" => new() { ["technical_exception_id"] = "TECH-P2-8", ["target_attempt_id"] = "ATTEMPT-P2-8", ["reason"] = Reason },
        "adminReview" => new() { ["review_item_id"] = "REVIEW-P2-8", ["resolution"] = "Evidence reviewed", ["reason"] = Reason },
        "mutateFeatureFlags" => new() { ["changes"] = new JsonObject { ["globalDialKillSwitch"] = true }, ["reason"] = Reason },
        _ => new() { ["reason"] = Reason },
    };

    private async Task<string> RunNodeAsync(string script)
    {
        ProcessStartInfo start = new("node") { WorkingDirectory = repository, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add(script);
        using Process process = Process.Start(start)!;
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string standardOutput = await output;
        Assert.True(process.ExitCode == 0, standardOutput + await error);
        return standardOutput;
    }

    private static string FindRepository()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "specs", "api", "openapi", "ivr-order-confirmation.v1.yaml"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("IVR checkout not found.");
    }
}
