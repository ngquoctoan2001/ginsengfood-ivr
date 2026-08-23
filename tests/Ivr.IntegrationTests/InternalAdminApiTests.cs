using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Errors;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class InternalAdminApiTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    [Trait("TestId", "CT-API-OAS-10")]
    public async Task AllThirteenInternalAndAdminOperationsAreMappedAndReachable()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(eligible: false);
        await using InternalAdminApiTestApplication app = await StartAsync();
        string[] expected =
        [
            "POST /v1/ivr/order-confirmation/eligibility-checks",
            "POST /v1/ivr/order-confirmation/call-jobs",
            "GET /v1/ivr/order-confirmation/call-jobs/{ivrCallJobId}",
            "POST /v1/ivr/order-confirmation/call-attempts",
            "POST /v1/ivr/order-confirmation/call-results",
            "POST /v1/ivr/order-confirmation/result-callbacks",
            "GET /v1/ivr/order-confirmation/queue",
            "POST /v1/ivr/order-confirmation/queue:pause",
            "POST /v1/ivr/order-confirmation/queue:resume",
            "POST /v1/ivr/order-confirmation/sim-channels/{simChannelId}:disable",
            "POST /v1/ivr/order-confirmation/sim-channels/{simChannelId}:enable",
            "POST /v1/ivr/order-confirmation/technical-retries",
            "POST /v1/ivr/order-confirmation/admin-reviews",
        ];
        string[] actual = app.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!
                .HttpMethods.Select(method =>
                    string.Concat(method, " ", endpoint.RoutePattern.RawText)))
            .Where(route => expected.Contains(route, StringComparer.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);

        using HttpResponseMessage eligibility = await SendInternalAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/eligibility-checks",
            new EligibilityLifecycleRequest("TASK-P2-8"));
        using HttpResponseMessage job = await SendInternalAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-jobs",
            new CallJobLifecycleRequest("JOB-P2-8", "TASK-P2-8"));
        using HttpResponseMessage jobRead = await SendInternalAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/JOB-P2-8");
        using HttpResponseMessage attempt = await SendInternalAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-attempts",
            new CallAttemptLifecycleRequest("ATTEMPT-P2-8", "JOB-P2-8"));
        using HttpResponseMessage result = await SendInternalAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-results",
            new CallResultLifecycleRequest("RESULT-P2-8", "JOB-P2-8"));
        using HttpResponseMessage callback = await SendInternalAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/result-callbacks",
            new ResultCallbackLifecycleRequest("CALLBACK-P2-8", "RESULT-P2-8"));
        Assert.True(
            eligibility.IsSuccessStatusCode,
            await eligibility.Content.ReadAsStringAsync());
        Assert.All(
            new[] { job, jobRead, attempt, result, callback },
            response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

        IvrServer.IvrEligibilityDecision eligibilityBody = (await eligibility.Content
            .ReadFromJsonAsync<IvrServer.IvrEligibilityDecision>())!;
        IvrServer.IvrCallJob jobBody = (await job.Content
            .ReadFromJsonAsync<IvrServer.IvrCallJob>())!;
        IvrServer.IvrCallJob jobReadBody = (await jobRead.Content
            .ReadFromJsonAsync<IvrServer.IvrCallJob>())!;
        IvrServer.IvrCallAttempt attemptBody = (await attempt.Content
            .ReadFromJsonAsync<IvrServer.IvrCallAttempt>())!;
        IvrServer.IvrCallResult resultBody = (await result.Content
            .ReadFromJsonAsync<IvrServer.IvrCallResult>())!;
        IvrServer.IvrResultCallbackLifecycle callbackBody = (await callback.Content
            .ReadFromJsonAsync<IvrServer.IvrResultCallbackLifecycle>())!;
        Assert.Equal("TASK-P2-8", eligibilityBody.Task_id);
        Assert.Equal("JOB-P2-8", jobBody.Ivr_call_job_id);
        Assert.Equal(jobBody.Ivr_call_job_id, jobReadBody.Ivr_call_job_id);
        Assert.Equal("ATTEMPT-P2-8", attemptBody.Ivr_call_attempt_id);
        Assert.Equal("RESULT-P2-8", resultBody.Ivr_call_result_id);
        Assert.Equal("CALLBACK-P2-8", callbackBody.Callback_id);
        Assert.True(jobBody.Input_signal_only);
        Assert.True(jobBody.No_direct_order_update);
        Assert.True(resultBody.No_payment_or_revenue_effect);
        Assert.True(callbackBody.Requires_core_revalidation);
    }

    [Fact]
    [Trait("TestId", "IT-API-AUTHZ-02")]
    public async Task InternalSurfaceRequiresDedicatedServiceIdentityScopeAndSource()
    {
        await fixture.ResetAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage allowed = await SendInternalAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/missing");
        Assert.Equal(HttpStatusCode.NotFound, allowed.StatusCode);

        using HttpResponseMessage wrongScope = await SendInternalAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/missing",
            scope: "ivr.admin.write");
        await AssertErrorAsync(wrongScope, HttpStatusCode.Forbidden, IvrErrorCodes.ForbiddenCaller);

        using HttpResponseMessage wrongSource = await SendInternalAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/missing",
            source: "order-core");
        await AssertErrorAsync(wrongSource, HttpStatusCode.Forbidden, IvrErrorCodes.ForbiddenCaller);

        using HttpResponseMessage wrongToken = await SendInternalAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/missing",
            token: "wrong-token");
        await AssertErrorAsync(wrongToken, HttpStatusCode.Forbidden, IvrErrorCodes.ForbiddenCaller);

        using HttpRequestMessage adminImpersonation = CreateInternalRequest(
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/call-jobs/missing");
        adminImpersonation.Headers.Add(MockPermissionAuthenticationHandler.HeaderName,
            IvrPermissions.QueueView);
        using HttpResponseMessage impersonated = await app.Client.SendAsync(adminImpersonation);
        await AssertErrorAsync(
            impersonated,
            HttpStatusCode.Forbidden,
            IvrErrorCodes.ForbiddenCaller);
    }

    [Fact]
    [Trait("TestId", "IT-API-AUTHZ-01")]
    public async Task AdminOperationsEnforceOneToOnePermissionAndActorBinding()
    {
        await fixture.ResetAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage queue = await SendAdminAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/queue",
            null,
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, queue.StatusCode);

        using HttpResponseMessage queueWrongPermission = await SendAdminAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/queue",
            null,
            IvrPermissions.ManualRetry);
        await AssertErrorAsync(
            queueWrongPermission,
            HttpStatusCode.Forbidden,
            IvrErrorCodes.ForbiddenCaller);

        (string Route, string RequiredPermission)[] protectedRoutes =
        [
            ("/v1/ivr/order-confirmation/queue:pause", IvrPermissions.QueuePause),
            ("/v1/ivr/order-confirmation/queue:resume", IvrPermissions.QueueResume),
            ("/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:disable", IvrPermissions.SimDisable),
            ("/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:enable", IvrPermissions.SimEnable),
            ("/v1/ivr/order-confirmation/technical-retries", IvrPermissions.ManualRetry),
            ("/v1/ivr/order-confirmation/admin-reviews", IvrPermissions.ResultReview),
        ];
        foreach ((string route, string required) in protectedRoutes)
        {
            using HttpResponseMessage response = await SendAdminAsync(
                app,
                HttpMethod.Post,
                route,
                new AdminMutationRequest("permission test"),
                IvrPermissions.QueueView);
            await AssertErrorAsync(
                response,
                HttpStatusCode.Forbidden,
                IvrErrorCodes.ForbiddenCaller);
            Assert.NotEqual(IvrPermissions.QueueView, required);
        }

        using HttpResponseMessage actorMismatch = await SendAdminAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/queue",
            null,
            IvrPermissions.QueueView,
            actorHeader: "different-actor");
        await AssertErrorAsync(
            actorMismatch,
            HttpStatusCode.Forbidden,
            IvrErrorCodes.ForbiddenCaller);
    }

    [Fact]
    [Trait("TestId", "IT-API-IDEMP-03")]
    public async Task AdminPostReplaysSameKeyAndRejectsDifferentPayload()
    {
        await fixture.ResetAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();
        string key = string.Concat("pause-", Guid.NewGuid().ToString("N"));
        using HttpResponseMessage first = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/queue:pause",
            new AdminMutationRequest("maintenance window"),
            IvrPermissions.QueuePause,
            idempotencyKey: key);
        using HttpResponseMessage replay = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/queue:pause",
            new AdminMutationRequest("maintenance window"),
            IvrPermissions.QueuePause,
            idempotencyKey: key);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(
            await first.Content.ReadAsStringAsync(),
            await replay.Content.ReadAsStringAsync());

        using HttpResponseMessage conflict = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/queue:pause",
            new AdminMutationRequest("different reason"),
            IvrPermissions.QueuePause,
            idempotencyKey: key);
        await AssertErrorAsync(
            conflict,
            HttpStatusCode.Conflict,
            IvrErrorCodes.IdempotencyConflict);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Single(await context.AdminActions.ToListAsync());
        Assert.Single(await context.AuditLog.ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-API-AUDIT-04")]
    public async Task EveryAdminMutationCommitsActionAndAppendOnlyAuditWithBeforeAfter()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage response = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:disable",
            new AdminMutationRequest(
                "maintenance isolation",
                "evidence://ivr/p2-8/sim-disable"),
            IvrPermissions.SimDisable);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AdminActionEntity action = Assert.Single(await context.AdminActions.ToListAsync());
        AuditLogEntity audit = Assert.Single(await context.AuditLog.ToListAsync());
        Assert.Equal("operator-p2-8", action.ActorId);
        Assert.Equal(IvrPermissions.SimDisable, action.Permission);
        Assert.True(action.NoPolicyBypass);
        Assert.NotNull(action.BeforeStateJson);
        Assert.NotNull(action.AfterStateJson);
        Assert.Equal(action.ActionType, audit.Action);
        Assert.Equal(action.BeforeStateJson, audit.BeforeStateJson);
        Assert.Equal(action.AfterStateJson, audit.AfterStateJson);
        audit.Action = "MUTATION_MUST_FAIL";
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task AdminMutationRollsBackWhenIdempotencySnapshotCannotCommit()
    {
        await fixture.ResetAsync();
        await using IvrDbContext setup = await Factory().CreateDbContextAsync();
        await setup.Database.ExecuteSqlRawAsync(
            """
            CREATE OR REPLACE FUNCTION ivr_test_reject_idempotency_insert()
            RETURNS trigger AS $$
            BEGIN
              RAISE EXCEPTION 'synthetic idempotency failure';
            END;
            $$ LANGUAGE plpgsql;
            CREATE TRIGGER trg_test_reject_idempotency_insert
            BEFORE INSERT ON ivr_idempotency_keys
            FOR EACH ROW EXECUTE FUNCTION ivr_test_reject_idempotency_insert();
            """);
        await using InternalAdminApiTestApplication app = await StartAsync();
        try
        {
            using HttpResponseMessage response = await SendAdminAsync(
                app,
                HttpMethod.Post,
                "/v1/ivr/order-confirmation/queue:pause",
                new AdminMutationRequest("atomic rollback proof"),
                IvrPermissions.QueuePause,
                idempotencyKey: "idem-atomic-rollback-proof");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            await using IvrDbContext verification = await Factory().CreateDbContextAsync();
            Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
            Assert.Equal(0, await verification.AdminActions.CountAsync());
            Assert.Equal(0, await verification.AuditLog.CountAsync());
            Assert.Equal(0, await verification.IdempotencyKeys.CountAsync());
        }
        finally
        {
            await setup.Database.ExecuteSqlRawAsync(
                """
                DROP TRIGGER IF EXISTS trg_test_reject_idempotency_insert
                ON ivr_idempotency_keys;
                DROP FUNCTION IF EXISTS ivr_test_reject_idempotency_insert();
                """);
        }
    }

    /// <summary>
    /// Cutting a call that is not running (W-0111).
    /// <para>
    /// The seeded attempt has already ended, so there is no open channel for a hangup to reach.
    /// A request written against it would be a row no dispatch loop will ever read, and an
    /// operator left believing something is about to happen. It has to refuse, and it has to
    /// refuse without writing anything — a rejected safety action that still leaves a partial
    /// record is worse than one that leaves none.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-API-TERMINATE-09")]
    public async Task TerminatingACallThatIsNotRunningIsRefusedAndWritesNothing()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(includeTerminalResult: true, attemptStatus: "COMPLETED");
        await using InternalAdminApiTestApplication app = await StartAsync();

        int auditBefore;
        await using (IvrDbContext before = await Factory().CreateDbContextAsync())
        {
            auditBefore = await before.AuditLog.AsNoTracking().CountAsync();
        }

        using HttpResponseMessage refused = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-jobs/JOB-P2-8:terminate",
            new AdminMutationRequest("cut a call that already ended"),
            IvrPermissions.CallTerminate);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        await using IvrDbContext after = await Factory().CreateDbContextAsync();
        CallAttemptEntity attempt = await after.CallAttempts.AsNoTracking().SingleAsync();
        Assert.Null(attempt.TerminationRequestedAt);
        Assert.Null(attempt.TerminationRequestedBy);
        Assert.Null(attempt.TerminationReason);
        Assert.Equal(auditBefore, await after.AuditLog.AsNoTracking().CountAsync());
        Assert.Empty(await after.AdminActions.AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// Operator holds the permission, and an unknown job is still a 404 rather than a 409 —
    /// "there is no such call" and "that call is not running" are different answers.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-API-TERMINATE-10")]
    public async Task OperatorMayTerminateAndAnUnknownJobIsNotFound()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(includeTerminalResult: true, attemptStatus: "COMPLETED");
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage unknown = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-jobs/JOB-DOES-NOT-EXIST:terminate",
            new AdminMutationRequest("cut an unknown call"),
            IvrPermissions.CallTerminate);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);

        // Wrong permission is still refused, so granting Operator the cut did not widen anything
        // else it can reach.
        using HttpResponseMessage wrongPermission = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-jobs/JOB-P2-8:terminate",
            new AdminMutationRequest("cut without the permission"),
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.Forbidden, wrongPermission.StatusCode);
    }

    [Fact]
    [Trait("TestId", "IT-API-QUEUE-08")]
    public async Task PauseBlocksOnlyNewClaimsAndResumeRestoresClaiming()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(includeTerminalResult: false, attemptStatus: "TECHNICAL_FAILED");
        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage pause = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/queue:pause",
            new AdminMutationRequest("controlled pause"),
            IvrPermissions.QueuePause);
        Assert.Equal(HttpStatusCode.OK, pause.StatusCode);
        await using (IvrDbContext paused = await Factory().CreateDbContextAsync())
        {
            Assert.Equal(
                "TECHNICAL_FAILED",
                (await paused.CallAttempts.AsNoTracking().SingleAsync()).Status);
            Assert.Contains(
                await paused.AuditLog.AsNoTracking().ToListAsync(),
                audit => audit.Action == "QUEUE_PAUSE");
        }
        IPostgresSchedulerStore scheduler = app.Services
            .GetRequiredService<IPostgresSchedulerStore>();
        Assert.Null(await scheduler.TryClaimDueDispatchAsync(
            "p2-8-worker",
            IvrOptions.MockExecutionMode,
            TimeSpan.FromMinutes(2)));

        using HttpResponseMessage resume = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/queue:resume",
            new AdminMutationRequest("controlled resume"),
            IvrPermissions.QueueResume);
        Assert.Equal(HttpStatusCode.OK, resume.StatusCode);
        await using (IvrDbContext resumed = await Factory().CreateDbContextAsync())
        {
            Assert.Equal(
                "TECHNICAL_FAILED",
                (await resumed.CallAttempts.AsNoTracking().SingleAsync()).Status);
            Assert.Contains(
                await resumed.AuditLog.AsNoTracking().ToListAsync(),
                audit => audit.Action == "QUEUE_RESUME");
        }
        Assert.NotNull(await scheduler.TryClaimDueDispatchAsync(
            "p2-8-worker",
            IvrOptions.MockExecutionMode,
            TimeSpan.FromMinutes(2)));
    }

    [Fact]
    [Trait("TestId", "IT-API-SIM-09")]
    public async Task DisablePreservesActiveLeaseAndUnsafeEnableFailsClosed()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(activeChannel: true);
        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage disable = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:disable",
            new AdminMutationRequest("isolate after current call"),
            IvrPermissions.SimDisable);
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);
        await using (IvrDbContext context = await Factory().CreateDbContextAsync())
        {
            SimChannelEntity channel = await context.SimChannels.SingleAsync();
            Assert.False(channel.Enabled);
            Assert.Equal("ACTIVE_CALL", channel.Status);
            Assert.Equal("JOB-P2-8", channel.ActiveCallJobId);
            Assert.NotNull(channel.LeaseToken);
        }

        using HttpResponseMessage enable = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:enable",
            new AdminMutationRequest("unsafe direct enable"),
            IvrPermissions.SimEnable);
        await AssertErrorAsync(
            enable,
            HttpStatusCode.Conflict,
            IvrErrorCodes.OperationalBlocked);
    }

    [Theory]
    [InlineData("REAL", "IDLE", false)]
    [InlineData("MOCK", "QUARANTINED", true)]
    public async Task DirectEnableRejectsRealOrQuarantinedChannel(
        string adapterMode,
        string status,
        bool quarantined)
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(activeChannel: false);
        await using (IvrDbContext seed = await Factory().CreateDbContextAsync())
        {
            SimChannelEntity channel = await seed.SimChannels.SingleAsync();
            channel.Enabled = false;
            channel.AdapterMode = adapterMode;
            channel.Status = status;
            channel.QuarantineUntil = quarantined ? Now.AddMinutes(10) : null;
            await seed.SaveChangesAsync();
        }
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:enable",
            new AdminMutationRequest("direct enable safety proof"),
            IvrPermissions.SimEnable);

        await AssertErrorAsync(
            response,
            HttpStatusCode.Conflict,
            IvrErrorCodes.OperationalBlocked);
        await using IvrDbContext verification = await Factory().CreateDbContextAsync();
        Assert.False((await verification.SimChannels.AsNoTracking().SingleAsync()).Enabled);
    }

    [Fact]
    [Trait("TestId", "IT-API-RETRY-06")]
    public async Task TechnicalRetryIsBoundedNonCountingAndDoesNotResetAttempts()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(includeTerminalResult: false);
        await using InternalAdminApiTestApplication app = await StartAsync();
        TechnicalRetryRequest body = new(
            "TECH-P2-8",
            "ATTEMPT-P2-8",
            "retry after mock adapter fault");
        using HttpResponseMessage first = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/technical-retries",
            body,
            IvrPermissions.ManualRetry);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        TechnicalRetryApiResult result = (await first.Content
            .ReadFromJsonAsync<TechnicalRetryApiResult>())!;
        Assert.False(result.CustomerAttemptCounted);
        Assert.Equal(1, result.TechnicalRetryCount);

        using HttpResponseMessage second = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/technical-retries",
            body with { Reason = "second retry must fail" },
            IvrPermissions.ManualRetry);
        await AssertErrorAsync(second, HttpStatusCode.Conflict, IvrErrorCodes.PolicyMismatch);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        CallAttemptEntity attempt = await context.CallAttempts.SingleAsync();
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal(1, attempt.TechnicalRetryCount);
        Assert.Equal(1, (await context.TechnicalExceptions.SingleAsync()).TechnicalRetryCount);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [Trait("TestId", "IT-API-RETRY-06")]
    public async Task TechnicalRetryFailsClosedForKillSwitchOrDestinationOutsideAllowlist(
        bool killSwitch,
        bool destinationAllowed)
    {
        await fixture.ResetAsync();
        await SeedGraphAsync(includeTerminalResult: false);
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(
                fixture.ConnectionString,
                killSwitch,
                destinationAllowed);
        using HttpResponseMessage response = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/technical-retries",
            new TechnicalRetryRequest(
                "TECH-P2-8",
                "ATTEMPT-P2-8",
                "runtime gate verification"),
            IvrPermissions.ManualRetry);
        await AssertErrorAsync(
            response,
            HttpStatusCode.Conflict,
            IvrErrorCodes.OperationalBlocked);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Equal(0, (await context.TechnicalExceptions.SingleAsync()).TechnicalRetryCount);
        Assert.Equal(0, (await context.CallAttempts.SingleAsync()).TechnicalRetryCount);
    }

    [Fact]
    [Trait("TestId", "IT-API-REVIEW-07")]
    public async Task ReviewAnnotatesReviewItemWithoutChangingNormalizedResult()
    {
        await fixture.ResetAsync();
        await SeedGraphAsync();
        string before;
        await using (IvrDbContext context = await Factory().CreateDbContextAsync())
        {
            before = JsonSerializer.Serialize(await context.CallResults.AsNoTracking().SingleAsync());
        }

        await using InternalAdminApiTestApplication app = await StartAsync();
        using HttpResponseMessage response = await SendAdminAsync(
            app,
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/admin-reviews",
            new AdminReviewRequest(
                "REVIEW-P2-8",
                "verified callback evidence",
                "manual review complete"),
            IvrPermissions.ResultReview);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AdminReviewApiResult result = (await response.Content
            .ReadFromJsonAsync<AdminReviewApiResult>())!;
        Assert.True(result.ResultUnchanged);

        await using IvrDbContext verification = await Factory().CreateDbContextAsync();
        string after = JsonSerializer.Serialize(
            await verification.CallResults.AsNoTracking().SingleAsync());
        Assert.Equal(before, after);
        Assert.Equal("RESOLVED", (await verification.ReviewItems.SingleAsync()).Status);
    }

    [Fact]
    [Trait("TestId", "IT-API-PII-05")]
    public async Task RequestsAndResponsesFailClosedOnRawPiiAndMalformedJson()
    {
        await fixture.ResetAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();
        const string rawPhone = "0901234567";
        (string Route, object Body, string Permission)[] unsafeRequests =
        [
            (
                "/v1/ivr/order-confirmation/queue:pause",
                new AdminMutationRequest($"call customer {rawPhone}"),
                IvrPermissions.QueuePause),
            (
                "/v1/ivr/order-confirmation/sim-channels/SIM-P2-8:disable",
                new AdminMutationRequest($"isolate {rawPhone}"),
                IvrPermissions.SimDisable),
            (
                "/v1/ivr/order-confirmation/technical-retries",
                new TechnicalRetryRequest("TECH-P2-8", "ATTEMPT-P2-8", $"retry {rawPhone}"),
                IvrPermissions.ManualRetry),
            (
                "/v1/ivr/order-confirmation/admin-reviews",
                new AdminReviewRequest("REVIEW-P2-8", "safe resolution", $"review {rawPhone}"),
                IvrPermissions.ResultReview),
        ];
        foreach ((string route, object body, string permission) in unsafeRequests)
        {
            using HttpResponseMessage unsafeResponse = await SendAdminAsync(
                app,
                HttpMethod.Post,
                route,
                body,
                permission);
            await AssertErrorAsync(
                unsafeResponse,
                HttpStatusCode.UnprocessableEntity,
                IvrErrorCodes.PiiPolicyViolation);
            Assert.DoesNotContain(rawPhone, await unsafeResponse.Content.ReadAsStringAsync(),
                StringComparison.Ordinal);
        }

        using HttpRequestMessage malformed = CreateInternalRequest(
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/call-jobs");
        malformed.Content = new StringContent("{bad", Encoding.UTF8, "application/json");
        using HttpResponseMessage malformedResponse = await app.Client.SendAsync(malformed);
        await AssertErrorAsync(
            malformedResponse,
            HttpStatusCode.BadRequest,
            IvrErrorCodes.MalformedRequest);

        using HttpResponseMessage queue = await SendAdminAsync(
            app,
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/queue",
            null,
            IvrPermissions.QueueView);
        string payload = await queue.Content.ReadAsStringAsync();
        Assert.DoesNotContain("phone", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("address", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dial_token", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            app.Logs.Entries,
            entry => entry.Contains(rawPhone, StringComparison.Ordinal));
    }

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private async Task SeedGraphAsync(
        bool includeTerminalResult = true,
        string attemptStatus = "TECHNICAL_FAILED",
        bool activeChannel = false,
        bool eligible = true)
    {
        DateTimeOffset startedAt = Now.AddMinutes(-1);
        DateTimeOffset deadline = Now.AddMinutes(4);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = "TASK-P2-8",
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = "p2-8-task-idempotency",
            CorrelationId = "corr-p2-8-seed",
            OfficialOrderId = "ORDER-P2-8",
            OrderCode = "GF-P2-8",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "gh-v1-candidate",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = "phone-ref-p2-8",
            PhoneMasked = "84xxxxx0065",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:p2-8-test-token",
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = eligible ? EligibilityDecisions.Eligible : null,
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            SellableStatusJson = "[]",
            SellableCapturedAt = Now,
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = deadline,
            AcceptedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/task\"]",
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = "JOB-P2-8",
            TaskId = "TASK-P2-8",
            OfficialOrderId = "ORDER-P2-8",
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "gh-v1-candidate",
            Status = "DRY_RUN",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                startedAt,
                startedAt.AddSeconds(150),
            }),
            T0At = startedAt,
            ExpiresAt = deadline,
            Eligible = eligible,
            EligibilityDecision = eligible
                ? EligibilityDecisions.Eligible
                : EligibilityDecisions.Pending,
            QueueStatus = "HELD_MOCK",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/job\"]",
        });
        context.TaskIntakeOutbox.Add(new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = "TASK-P2-8",
            IvrCallJobId = "JOB-P2-8",
            EventType = "IVR_TASK_READY_FOR_ELIGIBILITY",
            Status = "HELD_MOCK",
            CorrelationId = "corr-p2-8-seed",
            PayloadSha256 = new string('B', 64),
            CreatedAt = startedAt,
        });
        context.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = "ATTEMPT-P2-8",
            IvrCallJobId = "JOB-P2-8",
            TaskId = "TASK-P2-8",
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 2,
            ScheduledAt = startedAt,
            ScheduledWindowExpiresAt = deadline,
            EndedAt = startedAt.AddSeconds(5),
            Status = attemptStatus,
            ResultStatus = "IVR_TECHNICAL_EXCEPTION",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 0,
            TechnicalExceptionType = "MOCK_ADAPTER_FAULT",
            SimChannelId = "SIM-P2-8",
            PolicyVersion = "gh-v1-candidate",
            ScriptVersion = "v1-test-approved",
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/attempt\"]",
        });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = "SIM-P2-8",
            SimNumberRef = "sim-ref-p2-8",
            Enabled = true,
            Status = activeChannel ? "ACTIVE_CALL" : "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            ActiveCallJobId = activeChannel ? "JOB-P2-8" : null,
            LastHealthCheckAt = Now,
            LeaseToken = activeChannel ? Guid.NewGuid() : null,
            LeaseFencingGeneration = activeChannel ? 1 : 0,
            LeasedByWorkerId = activeChannel ? "active-worker" : null,
            LeaseAcquiredAt = activeChannel ? startedAt : null,
            LeaseExpiresAt = activeChannel ? deadline : null,
        });
        context.TechnicalExceptions.Add(new TechnicalExceptionEntity
        {
            TechnicalExceptionId = "TECH-P2-8",
            IvrCallAttemptId = "ATTEMPT-P2-8",
            ExceptionType = "MOCK_ADAPTER_FAULT",
            CustomerAttemptCounted = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 0,
            CorrelationId = "corr-p2-8-tech",
            CreatedAt = startedAt,
        });
        if (includeTerminalResult)
        {
            context.CallResults.Add(new CallResultEntity
            {
                IvrCallResultId = "RESULT-P2-8",
                IvrCallJobId = "JOB-P2-8",
                TaskId = "TASK-P2-8",
                OfficialOrderId = "ORDER-P2-8",
                OrderVersionSnapshot = "1",
                OrderVersionSeenByIvr = "1",
                FinalResultStatus = "IVR_NO_ANSWER_FINAL",
                ResultType = "IVR_NO_ANSWER_FINAL",
                ResultReason = "MAX_CUSTOMER_ATTEMPTS_REACHED",
                IsCountedCustomerAttempt = true,
                IsFinalForIvr = true,
                RecommendedCoreAction = "CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION",
                CoreOrderHandoffRequired = true,
                HumanReviewRequired = true,
                InputSignalOnly = true,
                NoDirectOrderUpdate = true,
                NoPaymentOrRevenueEffect = true,
                CreatedAt = startedAt.AddSeconds(10),
                EvidenceRefsJson = "[\"evidence://ivr/p2-8/result\"]",
            });
            context.ResultCallbacks.Add(new ResultCallbackEntity
            {
                CallbackId = "CALLBACK-P2-8",
                IvrCallResultId = "RESULT-P2-8",
                TaskId = "TASK-P2-8",
                OfficialOrderId = "ORDER-P2-8",
                IdempotencyKey = "callback-p2-8",
                ResultStatus = "IVR_NO_ANSWER_FINAL",
                ResultState = "PENDING",
                DeliveryStatus = "PENDING",
                RequiresCoreRevalidation = true,
                PayloadJson = "{}",
                PayloadSha256 = new string('A', 64),
                CreatedAt = startedAt.AddSeconds(10),
            });
            context.ReviewItems.Add(new ReviewItemEntity
            {
                ReviewItemId = "REVIEW-P2-8",
                SourceType = "call-result",
                SourceId = "RESULT-P2-8",
                Reason = "verify final result evidence",
                Status = "OPEN",
                CorrelationId = "corr-p2-8-review",
                CreatedAt = startedAt.AddSeconds(10),
            });
        }

        await context.SaveChangesAsync();
    }

    private static HttpRequestMessage CreateInternalRequest(
        HttpMethod method,
        string route,
        object? body = null,
        string token = InternalAdminApiTestApplication.InternalToken,
        string scope = InternalServiceOptions.RequiredScope,
        string source = "ivr-worker",
        string? idempotencyKey = null)
    {
        HttpRequestMessage request = new(method, route);
        request.Headers.Authorization = new("Bearer", token);
        request.Headers.Add("X-Source-System", source);
        request.Headers.Add("X-Service-Scope", scope);
        request.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
        if (method != HttpMethod.Get)
        {
            request.Headers.Add(
                "Idempotency-Key",
                idempotencyKey ?? string.Concat("idem-", Guid.NewGuid().ToString("N")));
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static async Task<HttpResponseMessage> SendInternalAsync(
        InternalAdminApiTestApplication app,
        HttpMethod method,
        string route,
        object? body = null,
        string token = InternalAdminApiTestApplication.InternalToken,
        string scope = InternalServiceOptions.RequiredScope,
        string source = "ivr-worker")
    {
        using HttpRequestMessage request = CreateInternalRequest(
            method,
            route,
            body,
            token,
            scope,
            source);
        return await app.Client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> SendAdminAsync(
        InternalAdminApiTestApplication app,
        HttpMethod method,
        string route,
        object? body,
        string permission,
        string actorHeader = "operator-p2-8",
        string? idempotencyKey = null)
    {
        using HttpRequestMessage request = new(method, route);
        request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, permission);
        request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-p2-8");
        request.Headers.Add("X-Actor-Id", actorHeader);
        request.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
        if (method != HttpMethod.Get)
        {
            request.Headers.Add(
                "Idempotency-Key",
                idempotencyKey ?? string.Concat("idem-", Guid.NewGuid().ToString("N")));
            request.Content = JsonContent.Create(body);
        }

        return await app.Client.SendAsync(request);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode status,
        string code)
    {
        Assert.Equal(status, response.StatusCode);
        using JsonDocument document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            code,
            document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }
}
