using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Scripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0096 — read-only back-office projections consumed by the P3-3 console.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AdminConfigApiTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    [Trait("TestId", "IT-ADMIN-CONFIG-01")]
    public async Task BackOfficeRoutesAreMappedAndGatedByQueueViewPermission()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        foreach (string route in Routes)
        {
            using HttpResponseMessage allowed = await SendAsync(app, route, IvrPermissions.QueueView);
            Assert.True(
                allowed.StatusCode == HttpStatusCode.OK,
                $"{route} -> {allowed.StatusCode}: {await allowed.Content.ReadAsStringAsync()} || "
                    + string.Join(" || ", app.Logs.Entries.TakeLast(10)));

            using HttpResponseMessage forbidden = await SendAsync(app, route, IvrPermissions.ManualRetry);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            using JsonDocument envelope = JsonDocument.Parse(
                await forbidden.Content.ReadAsStringAsync());
            Assert.Equal(
                IvrErrorCodes.ForbiddenCaller,
                envelope.RootElement.GetProperty("error").GetProperty("code").GetString());
        }
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-CONFIG-02")]
    public async Task ScriptCatalogReportsApprovalStateKeyNineAndTheOwnerDecisionLock()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app,
            "/v1/ivr/order-confirmation/scripts",
            IvrPermissions.QueueView);
        IvrServer.IvrScriptCatalog catalog =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrScriptCatalog>())!;

        // OD-V1-15 is still open, so the Target V1 field set is not production approved.
        Assert.False(catalog.Production_target_v1_fields_approved);

        // AS-07: key 9 exists in the map and is reported disabled.
        IvrServer.IvrDtmfKey keyNine = catalog.Dtmf_map.Single(key => key.Key == "9");
        Assert.False(keyNine.Enabled);
        Assert.Equal("NOT_ENABLED", keyNine.Meaning);
        Assert.True(catalog.Dtmf_map.Single(key => key.Key == "1").Enabled);
        Assert.True(catalog.Dtmf_map.Single(key => key.Key == "0").Enabled);

        Assert.Contains("FULL_ADDRESS", catalog.Prohibited_variables);
        Assert.Contains("PAYMENT_DETAIL", catalog.Prohibited_variables);
        Assert.Contains("order_code_short", catalog.Allowed_input_fields);

        IvrServer.IvrScriptVersion approved = catalog.Versions
            .Single(version => version.Version == "v1-approved");
        Assert.Equal("APPROVED", approved.Status);
        Assert.True(approved.Template_valid);
        Assert.Empty(approved.Missing_approvals);
        Assert.Equal(4, approved.Approvals.Count);

        IvrServer.IvrScriptVersion draft = catalog.Versions
            .Single(version => version.Version == "v2-draft");
        Assert.Equal("DRAFT", draft.Status);
        // A stored template that no longer validates is surfaced, not thrown.
        Assert.False(draft.Template_valid);
        Assert.False(draft.Uses_production_decision_fields);
        // The unapproved version reports exactly which gates it still lacks.
        Assert.Equal(4, draft.Missing_approvals.Count);
        Assert.Contains("MOCK_TEST", draft.Missing_approvals);
        Assert.Contains("PRIVACY_LEGAL", draft.Missing_approvals);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-CONFIG-03")]
    public async Task IntegrationStatusMarksUnprobedDependenciesAsNotWired()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app,
            "/v1/ivr/order-confirmation/integration-status",
            IvrPermissions.QueueView);
        IvrServer.IvrIntegrationStatus status =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrIntegrationStatus>())!;

        // The console must not present any dependency as verified until W-0040.
        Assert.False(status.Dependency_probing_available);
        Assert.Equal(IvrOptions.MockExecutionMode, status.Execution_mode);
        Assert.False(status.Real_customer_call_allowed);

        foreach (string dependency in new[]
        {
            "ORDER_CORE",
            "OPS_SELLABLE_GATE",
            "CRM_DO_NOT_CALL",
            "EVIDENCE_REGISTRY",
        })
        {
            IvrServer.IvrDependencyStatus card = status.Dependencies
                .Single(item => item.Dependency == dependency);
            Assert.Equal(IvrServer.IvrDependencyStatusState.NOT_WIRED, card.State);
            Assert.False(card.Observed);
            Assert.False(string.IsNullOrWhiteSpace(card.Fail_closed_effect));
        }

        // W-0029 / P4-1 §3.5. ORDER_CORE stays unobserved while delivery is off, but its
        // detail now names what IVR genuinely knows — the selected provider profile and its own
        // outbound circuit — instead of an empty placeholder.
        IvrServer.IvrDependencyStatus orderCore = status.Dependencies
            .Single(item => item.Dependency == "ORDER_CORE");
        Assert.Contains("provider=", orderCore.Detail, StringComparison.Ordinal);
        Assert.Contains("circuit=", orderCore.Detail, StringComparison.Ordinal);
        Assert.Contains("BLOCKED_EXTERNAL", orderCore.Detail, StringComparison.Ordinal);
        Assert.Contains("Nhà cung cấp=", orderCore.Detail_vi, StringComparison.Ordinal);
        Assert.Contains("endpoint thật vẫn BLOCKED_EXTERNAL", orderCore.Detail_vi, StringComparison.Ordinal);

        // W-0031 landed, so the CRM card no longer promises a provider that will never be wired.
        IvrServer.IvrDependencyStatus crm = status.Dependencies
            .Single(item => item.Dependency == "CRM_DO_NOT_CALL");
        Assert.Contains("W-0031", crm.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("No CRM provider wired", crm.Detail, StringComparison.Ordinal);

        // What IVR does own is reported truthfully.
        IvrServer.IvrDependencyStatus sim = status.Dependencies
            .Single(item => item.Dependency == "SIM_GATEWAY");
        Assert.True(sim.Observed);
        Assert.Equal(IvrServer.IvrDependencyStatusState.UP, sim.State);
        Assert.Contains("provider=MOCK", sim.Detail, StringComparison.Ordinal);
        Assert.Contains("Nhà cung cấp=MOCK", sim.Detail_vi, StringComparison.Ordinal);
        Assert.Contains("1/1 kênh đang bật", sim.Detail_vi, StringComparison.Ordinal);

        IvrServer.IvrFailClosedEvent incident = status.Recent_fail_closed_events
            .Single(item => item.Source == "CAPACITY_INCIDENT");
        Assert.Contains("SCHEDULER_DEADLINE", incident.Effect, StringComparison.Ordinal);
        Assert.Equal(false, incident.Hold_new_calls);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-CONFIG-04")]
    public async Task ReviewQueueResolvesEachItemBackToItsCallJob()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app,
            "/v1/ivr/order-confirmation/review-items?status=OPEN",
            IvrPermissions.QueueView);
        string payload = await response.Content.ReadAsStringAsync();
        IvrServer.IvrReviewQueue queue =
            JsonSerializer.Deserialize<IvrServer.IvrReviewQueue>(payload)!;

        Assert.Equal(1, queue.Total_count);
        IvrServer.IvrReviewQueueItem item = Assert.Single(queue.Items);
        Assert.Equal("REVIEW-CFG-01", item.Review_item_id);
        Assert.Equal("IVR_CALL_RESULT", item.Source_type);
        // Resolved through the result back to the job, so the console can link out.
        Assert.Equal("JOB-CFG", item.Ivr_call_job_id);
        Assert.Equal("GF-CFG", item.Order_code_short);
        Assert.Equal("IVR_CONFIRMED", item.Result_type);

        // Masked only: no raw contact data reaches the queue projection.
        Assert.DoesNotContain("phone_ref", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dial_token", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GF-ORDER-CFG-FULL", payload, StringComparison.Ordinal);

        using HttpResponseMessage resolved = await SendAsync(
            app,
            "/v1/ivr/order-confirmation/review-items?status=RESOLVED",
            IvrPermissions.QueueView);
        IvrServer.IvrReviewQueue empty =
            (await resolved.Content.ReadFromJsonAsync<IvrServer.IvrReviewQueue>())!;
        Assert.Equal(0, empty.Total_count);
        Assert.Empty(empty.Items);
    }

    [Fact]
    [Trait("TestId", "IT-ADMIN-CONFIG-05")]
    public async Task NoBackOfficeRouteExposesAMutation()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        // Integration status and the review queue are still read-only surfaces, and seed
        // loading and permission assignment are still absent by design: permissions belong to
        // Permission Core (DF-01).
        foreach (string route in ReadOnlyRoutes)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, route.Split('?')[0]);
            request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, IvrPermissions.QueueView);
            request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-config");
            request.Headers.Add("X-Actor-Id", "operator-config");
            request.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
            request.Content = JsonContent.Create(new { reason = "attempted write" });
            using HttpResponseMessage response = await app.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        // Scripts is the one that changed, and the replacement assertion is stronger than the
        // one it replaces. W-0109 opened the lifecycle so a Privacy/Legal signature has a path
        // that carries audit, a reason and "creator cannot approve" -- the previous alternative
        // was editing rows by hand, which carries none of them. What must NOT come with that
        // opening is reachability from the mock permission seam: the seam mints whatever
        // X-Permissions asks for, MOCK is the default mode, and one of these permissions signs
        // off the wording a customer is read. 401, not 405 and not 200.
        foreach (string route in ScriptMutationRoutes)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, route);
            request.Headers.Add(
                MockPermissionAuthenticationHandler.HeaderName,
                IvrPermissions.ScriptApproveContent);
            request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-config");
            request.Headers.Add("X-Actor-Id", "operator-config");
            request.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
            request.Content = JsonContent.Create(new { reason = "attempted write" });
            using HttpResponseMessage response = await app.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    /// <summary>Read surfaces. GET /scripts is unchanged by W-0109; only POST was added.</summary>
    private static readonly string[] Routes =
    [
        "/v1/ivr/order-confirmation/scripts",
        "/v1/ivr/order-confirmation/integration-status",
        "/v1/ivr/order-confirmation/review-items",
    ];

    private static readonly string[] ReadOnlyRoutes =
    [
        "/v1/ivr/order-confirmation/integration-status",
        "/v1/ivr/order-confirmation/review-items",
    ];

    private static readonly string[] ScriptMutationRoutes =
    [
        "/v1/ivr/order-confirmation/scripts",
        "/v1/ivr/order-confirmation/scripts/",
        "/v1/ivr/order-confirmation/scripts/SCRIPT-ORDER-CONFIRM/v3-test-approved:submit",
        "/v1/ivr/order-confirmation/scripts/SCRIPT-ORDER-CONFIRM/v3-test-approved:approve",
        "/v1/ivr/order-confirmation/scripts/SCRIPT-ORDER-CONFIRM/v3-test-approved:retire",
    ];

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private static Task<HttpResponseMessage> SendAsync(
        InternalAdminApiTestApplication app,
        string route,
        string permission)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, route);
        request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, permission);
        request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-config");
        request.Headers.Add("X-Actor-Id", "operator-config");
        request.Headers.Add(
            "X-Correlation-Id",
            string.Concat("corr-", Guid.NewGuid().ToString("N")));
        return app.Client.SendAsync(request);
    }

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private async Task SeedAsync()
    {
        DateTimeOffset startedAt = Now.AddMinutes(-5);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        Guid approvedId = Guid.NewGuid();
        context.ScriptVersions.AddRange(
            new ScriptVersionEntity
            {
                Id = approvedId,
                TemplateId = "SCRIPT-ORDER-CONFIRM",
                Version = "v1-approved",
                Status = "APPROVED",
                TemplateText = TargetV1SpeechPolicy.CanonicalVietnameseTemplate,
                TemplateHash = new string('a', 64),
                AllowedInputFieldsJson = "[\"customer_display_name\",\"order_code_short\"]",
                CreatedBy = "author-01",
                CreateReason = "initial",
                CreatedAt = startedAt,
                SubmittedBy = "author-01",
                SubmitReason = "ready",
                SubmittedAt = startedAt,
            },
            new ScriptVersionEntity
            {
                Id = Guid.NewGuid(),
                TemplateId = "SCRIPT-ORDER-CONFIRM",
                Version = "v2-draft",
                Status = "DRAFT",
                // Deliberately non-conforming: the catalogue must report it, not crash.
                TemplateText = "Bản nháp {{order_code_short}}.",
                TemplateHash = new string('b', 64),
                AllowedInputFieldsJson = "[\"order_code_short\"]",
                CreatedBy = "author-02",
                CreateReason = "draft",
                CreatedAt = startedAt,
            });

        foreach (string approvalType in new[] { "MOCK_TEST", "LAB", "CONTENT", "PRIVACY_LEGAL" })
        {
            context.ScriptApprovals.Add(new ScriptApprovalEntity
            {
                Id = Guid.NewGuid(),
                ScriptVersionId = approvedId,
                ApprovalType = approvalType,
                ActorId = string.Concat("approver-", approvalType.ToLowerInvariant().Replace("_", "-")),
                Reason = "reviewed",
                CorrelationId = "corr-cfg-approval",
                ApprovedAt = startedAt,
            });
        }

        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = "SIM-CFG-01",
            SimNumberRef = "sim-ref-cfg",
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            LastHealthCheckAt = startedAt,
        });

        context.CapacityIncidents.Add(new CapacityIncidentEntity
        {
            CapacityIncidentId = "INCIDENT-CFG-01",
            SessionId = "corr-cfg-incident",
            ProgramCode = "GOLDEN_HOUR",
            Status = "OPEN",
            Scope = "SCHEDULER_DEADLINE",
            HoldNewCalls = false,
            ActiveSimCount = 1,
            PendingCallJobs = 1,
            ExpiredCallJobs = 0,
            MissedDeadlineCount = 1,
            ShortageReason = "MOCK_CAPACITY_SHORTAGE",
            OpenedAt = startedAt,
        });

        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = "TASK-CFG",
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = "cfg-idem",
            CorrelationId = "corr-cfg",
            OfficialOrderId = "ORDER-CFG",
            OrderCode = "GF-ORDER-CFG-FULL",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "COD",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "TWENTY_FOUR_SEVEN",
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = Now.AddHours(4),
            PhoneRef = "phone-ref-cfg",
            PhoneMasked = "84xxxxx0311",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:cfg-dial-token",
            DialTokenExpiresAt = Now.AddHours(4),
            PrivacySafeOrderSummaryJson = "{\"order_code_short\":\"GF-CFG\"}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            SellableStatusJson = "[]",
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = Now.AddHours(4),
            AcceptedAt = startedAt,
        });

        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = "JOB-CFG",
            TaskId = "TASK-CFG",
            OfficialOrderId = "ORDER-CFG",
            OrderVersionSnapshot = "1",
            ProgramType = "TWENTY_FOUR_SEVEN",
            AttemptPolicyCode = "mock-lab-v1",
            Status = "DRY_RUN",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowSeconds = 900,
            AttemptScheduleJson = "[]",
            T0At = startedAt,
            ExpiresAt = Now.AddHours(4),
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-approved",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
        });

        context.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = "RESULT-CFG",
            IvrCallJobId = "JOB-CFG",
            TaskId = "TASK-CFG",
            OfficialOrderId = "ORDER-CFG",
            OrderVersionSnapshot = "1",
            OrderVersionSeenByIvr = "1",
            FinalResultStatus = "IVR_CONFIRMED",
            ResultType = "IVR_CONFIRMED",
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = "REVALIDATE_AND_CONFIRM_ORDER",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = true,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            CreatedAt = startedAt,
        });

        context.ReviewItems.Add(new ReviewItemEntity
        {
            ReviewItemId = "REVIEW-CFG-01",
            SourceType = "IVR_CALL_RESULT",
            SourceId = "RESULT-CFG",
            Reason = "verify final result evidence",
            Status = "OPEN",
            CorrelationId = "corr-cfg",
            CreatedAt = startedAt,
        });

        await context.SaveChangesAsync();
    }
}
