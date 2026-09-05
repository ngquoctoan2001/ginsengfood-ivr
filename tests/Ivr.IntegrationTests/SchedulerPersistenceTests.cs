using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class SchedulerPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-SCH-CLAIM-01")]
    public async Task DuplicateWorkersCreateOneAttemptAndOneActiveChannelLease()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(factory, "TASK-SCH-CLAIM-01", "JOB-SCH-CLAIM-01", Now);
        await SeedChannelAsync(factory, "SIM-LAB-001");
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        SchedulerDispatchLease?[] leases = await Task.WhenAll(
            store.TryClaimDueDispatchAsync(
                "worker-a",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2)),
            store.TryClaimDueDispatchAsync(
                "worker-b",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2)));

        SchedulerDispatchLease lease = Assert.Single(leases.OfType<SchedulerDispatchLease>());
        Assert.Equal(1, lease.AttemptNumber);
        Assert.Equal(1, lease.FencingGeneration);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("LEASED_PENDING_DISPATCH", attempt.Status);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("RESERVED", channel.Status);
        Assert.Equal(job.IvrCallJobId, channel.ActiveCallJobId);
        Assert.Equal("DISPATCH_LEASED", job.Status);
        Assert.Equal("LEASED", job.QueueStatus);
        Assert.Single(await verification.AuditLog.AsNoTracking()
            .Where(row => row.Action == "SCHEDULER_DISPATCH_LEASED")
            .ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-SCH-RECOVERY-02")]
    public async Task ExpiredLeaseIsQuarantinedWithNewFenceAndCannotBeReclaimedBlindly()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(factory, "TASK-SCH-RECOVERY-02", "JOB-SCH-RECOVERY-02", Now);
        await SeedChannelAsync(factory, "SIM-LAB-RECOVERY-001");
        var claimStore = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await claimStore.TryClaimDueDispatchAsync(
                "worker-crashed",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromSeconds(30)));
        DateTimeOffset recoveryAt = Now.AddSeconds(31);
        var recoveryStore = new PostgresSchedulerStore(
            factory,
            new FixedTimeProvider(recoveryAt));

        int recovered = await recoveryStore.QuarantineExpiredLeasesAsync(
            recoveryAt,
            TimeSpan.FromMinutes(10),
            16);

        Assert.Equal(1, recovered);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("QUARANTINED", channel.Status);
        Assert.Equal(lease.FencingGeneration + 1, channel.LeaseFencingGeneration);
        Assert.Null(channel.LeaseToken);
        Assert.Equal("RECOVERY_REQUIRED", attempt.Status);
        Assert.Equal("HELD_ADMIN_REVIEW", job.Status);
        Assert.Equal("HELD_LEASE_RECOVERY", job.QueueStatus);
        Assert.Null(await recoveryStore.TryClaimDueDispatchAsync(
            "worker-new",
            IvrOptions.LabRealSimExecutionMode,
            TimeSpan.FromMinutes(2)));

        DateTimeOffset quarantineExpiredAt = recoveryAt.AddMinutes(10).AddSeconds(1);
        var expiredQuarantineStore = new PostgresSchedulerStore(
            factory,
            new FixedTimeProvider(quarantineExpiredAt));
        Assert.Null(await expiredQuarantineStore.TryClaimDueDispatchAsync(
            "worker-after-quarantine",
            IvrOptions.LabRealSimExecutionMode,
            TimeSpan.FromMinutes(2)));
        await using IvrDbContext afterQuarantine = await factory.CreateDbContextAsync();
        SimChannelEntity recoveredChannel = await afterQuarantine.SimChannels
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("IDLE", recoveredChannel.Status);
        Assert.Null(recoveredChannel.QuarantineUntil);
        Assert.Null(recoveredChannel.DisabledReason);
    }

    [Fact]
    [Trait("TestId", "IT-SCH-FAIL-WINDOW-12")]
    public async Task ThirdLeaseFailureInsideTenMinuteWindowAutoDisablesChannel()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        DateTimeOffset recoveryAt = Now.AddSeconds(31);
        await SeedReadyJobAsync(factory, "TASK-SCH-WINDOW-12", "JOB-SCH-WINDOW-12", Now);
        await SeedChannelAsync(factory, "SIM-LAB-WINDOW-012");
        await using (IvrDbContext setup = await factory.CreateDbContextAsync())
        {
            SimChannelEntity channel = await setup.SimChannels.SingleAsync();
            channel.FailCount = 2;
            channel.FailureWindowStartedAt = recoveryAt.AddMinutes(-10);
            await setup.SaveChangesAsync();
        }

        var claimStore = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        _ = Assert.IsType<SchedulerDispatchLease>(await claimStore.TryClaimDueDispatchAsync(
            "worker-window-crashed",
            IvrOptions.LabRealSimExecutionMode,
            TimeSpan.FromSeconds(30)));
        var recoveryStore = new PostgresSchedulerStore(
            factory,
            new FixedTimeProvider(recoveryAt));

        Assert.Equal(1, await recoveryStore.QuarantineExpiredLeasesAsync(
            recoveryAt,
            TimeSpan.FromMinutes(10),
            16));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity persisted = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal(3, persisted.FailCount);
        Assert.Equal(recoveryAt.AddMinutes(-10), persisted.FailureWindowStartedAt);
        Assert.Equal("HEALTH_FAILED", persisted.Status);
        Assert.Equal("LEASE_EXPIRED_RECONCILIATION_REQUIRED", persisted.DisabledReason);
    }

    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-03")]
    public async Task MissedDeadlineCreatesCapacityIncidentAndFinalNonCountedResult()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        DateTimeOffset startedAt = Now.AddMinutes(-5);
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-DEADLINE-03",
            "JOB-SCH-DEADLINE-03",
            startedAt,
            expiresAt: Now);
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        int closed = await store.CloseMissedDeadlinesAsync(Now, 16);

        Assert.Equal(1, closed);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CapacityIncidentEntity incident = await verification.CapacityIncidents
            .AsNoTracking()
            .SingleAsync();
        CallResultEntity result = await verification.CallResults.AsNoTracking().SingleAsync();
        ResultCallbackEntity callback = await verification.ResultCallbacks.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("SCHEDULER_DEADLINE", incident.Scope);
        Assert.Equal(1, incident.MissedDeadlineCount);
        Assert.Equal("IVR_CAPACITY_EXCEPTION", result.ResultType);
        Assert.True(result.IsFinalForIvr);
        Assert.False(result.IsCountedCustomerAttempt);
        Assert.Equal("READY", callback.DeliveryStatus);
        Assert.Equal(result.IvrCallResultId, callback.IvrCallResultId);
        using (JsonDocument payload = JsonDocument.Parse(callback.PayloadJson))
        {
            Assert.Equal(
                "IVR_CAPACITY_EXCEPTION",
                payload.RootElement.GetProperty("result_type").GetString());
            Assert.False(payload.RootElement.GetProperty("is_counted_customer_attempt").GetBoolean());
            Assert.Equal(
                "CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW",
                payload.RootElement.GetProperty("recommended_core_action").GetString());
        }
        Assert.Equal("CAPACITY_MISSED", job.Status);
        Assert.Equal(incident.CapacityIncidentId, job.CapacityIncidentId);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-OBS-DEADLINE-10")]
    public async Task AMissedDeadlineMovesBothTheDeadlineAndTheResultCounters()
    {
        // W-0054 CAP-ALERT-04 could only report NOT_PROVEN while ivr_missed_deadline_total had no
        // call site. This is the call site, measured through the real Meter rather than by reading
        // the source.
        //
        // Both counters, and the second is the one worth having a test for. This method is the one
        // path that writes a FINAL result without going through normalization, so before this the
        // capacity misses were absent from ivr_call_results_total -- and a confirm_rate whose
        // denominator omits the failures reads higher than the truth, by exactly the amount that
        // matters most.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-OBS-DEADLINE-10",
            "JOB-OBS-DEADLINE-10",
            Now.AddMinutes(-5),
            expiresAt: Now);
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        List<(string Instrument, string Taxonomy)> observed = [];
        using (MeterListener listener = ListenForCapacityMetrics(observed))
        {
            Assert.Equal(1, await store.CloseMissedDeadlinesAsync(Now, 16));
            listener.RecordObservableInstruments();
        }

        Assert.Equal(2, observed.Count);
        Assert.Contains(
            observed,
            measurement => measurement.Instrument == "ivr_missed_deadline_total"
                && measurement.Taxonomy == "GOLDEN_HOUR");

        // The taxonomy value the rate queries filter on, not a generic "failed".
        Assert.Contains(
            observed,
            measurement => measurement.Instrument == "ivr_call_results_total"
                && measurement.Taxonomy == "IVR_CAPACITY_EXCEPTION");
    }

    [Fact]
    [Trait("TestId", "IT-OBS-DEADLINE-10")]
    public async Task ASweepThatFindsNothingMovesNoCounter()
    {
        // The half that makes the zero-tolerance alert usable at all. The sweep runs on every
        // scheduler tick and finds nothing almost every time; a counter that moved on the empty
        // sweeps would make an idle system fire IvrConfirmationDeadlineMissed continuously, and an
        // alert that fires continuously is an alert that gets muted.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-OBS-DEADLINE-10B",
            "JOB-OBS-DEADLINE-10B",
            Now,
            expiresAt: Now.AddHours(1));
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        List<(string Instrument, string Taxonomy)> observed = [];
        using (MeterListener listener = ListenForCapacityMetrics(observed))
        {
            Assert.Equal(0, await store.CloseMissedDeadlinesAsync(Now, 16));
            listener.RecordObservableInstruments();
        }

        Assert.Empty(observed);
    }

    private static MeterListener ListenForCapacityMetrics(
        List<(string Instrument, string Taxonomy)> observed)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName
                    && instrument.Name is "ivr_missed_deadline_total" or "ivr_call_results_total")
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            string taxonomy = string.Empty;
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key is TelemetryTags.Program or TelemetryTags.ResultType)
                {
                    taxonomy = tag.Value?.ToString() ?? string.Empty;
                }
            }

            lock (observed)
            {
                observed.Add((instrument.Name, taxonomy));
            }
        });
        listener.Start();
        return listener;
    }

    /// <summary>
    /// W-0116. A job held for admin review still closes at its deadline, but it closes as a window
    /// expiry rather than as a capacity miss. It was kept back deliberately; no channel was ever
    /// requested for it, so calling its deadline a channel shortage would spell an operational
    /// decision into the counter that sizes the SIM order.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-09")]
    public async Task HeldAdminReviewJobClosesAsWindowExpiredNotCapacityMiss()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-DEADLINE-09",
            "JOB-SCH-DEADLINE-09",
            Now.AddMinutes(-5),
            expiresAt: Now);
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await seed.CallJobs.SingleAsync();
            job.Status = "HELD_ADMIN_REVIEW";
            job.QueueStatus = "HELD_ADMIN_REVIEW";
            await seed.SaveChangesAsync();
        }

        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        Assert.Equal(1, await store.CloseMissedDeadlinesAsync(Now, 16));
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity closed = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("WINDOW_EXPIRED", closed.Status);
        Assert.Equal("CLOSED_WINDOW_EXPIRED", closed.QueueStatus);
        Assert.NotNull(closed.ClosedAt);
        Assert.Null(closed.CapacityIncidentId);

        // The point of the change: no incident at all, not merely one that does not hold calls.
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());

        CallResultEntity result = await verification.CallResults.AsNoTracking().SingleAsync();
        Assert.Equal("IVR_CONFIRMATION_WINDOW_EXPIRED", result.ResultType);
        Assert.False(result.IsCountedCustomerAttempt);
        Assert.True(result.IsFinalForIvr);

        // Nobody called this customer, so the order must not expire on its own.
        Assert.Equal("REVALIDATE_AND_HOLD_ADMIN_REVIEW", result.RecommendedCoreAction);
        Assert.True(result.HumanReviewRequired);
    }

    /// <summary>
    /// W-0116. The customer was reached once and the window ran out before the policy's second
    /// attempt was due. The confirmation genuinely lapsed, so Core is advised to expire it rather
    /// than to park it in a review queue -- the outcome that keeps the queue small enough to stay
    /// meaningful for the cases where nobody was reached at all.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-11")]
    public async Task ReachedCustomerWhoseWindowLapsedIsAdvisedToExpireNotToReview()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        DateTimeOffset startedAt = Now.AddMinutes(-5);
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-DEADLINE-11",
            "JOB-SCH-DEADLINE-11",
            startedAt,
            expiresAt: Now);
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await seed.CallJobs.SingleAsync();
            seed.CallAttempts.Add(new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-SCH-DEADLINE-11",
                IvrCallJobId = job.IvrCallJobId,
                TaskId = job.TaskId,
                AttemptNumber = 1,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = startedAt,
                ScheduledWindowExpiresAt = Now,
                StartedAt = startedAt,
                Status = "NORMALIZED_ATTEMPT_COMPLETE",
                ResultStatus = "IVR_NO_ANSWER_ATTEMPT",
                IsCountedCustomerAttempt = true,
                PolicyVersion = job.AttemptPolicyCode,
                ScriptVersion = job.ScriptVersion,
            });
            await seed.SaveChangesAsync();
        }

        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        Assert.Equal(1, await store.CloseMissedDeadlinesAsync(Now, 16));
        await using IvrDbContext verification = await factory.CreateDbContextAsync();

        // One real call went out, so this is not a shortage story either.
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        CallResultEntity result = await verification.CallResults.AsNoTracking().SingleAsync();
        Assert.Equal("IVR_CONFIRMATION_WINDOW_EXPIRED", result.ResultType);
        Assert.Equal("REVALIDATE_AND_EXPIRE_CONFIRMATION", result.RecommendedCoreAction);
        Assert.False(result.HumanReviewRequired);

        // The sweep placed no call of its own, so the customer's quota is untouched by it.
        Assert.False(result.IsCountedCustomerAttempt);
    }

    /// <summary>
    /// W-0117. The counted-attempt invariant, proven against a writer that bypasses the domain
    /// guard entirely.
    /// <para>
    /// §16 claims this rule is enforced "at the data layer". Until W-0117 it was enforced by
    /// <c>CallResultSnapshot.Create</c>, which the scheduler sweep never calls — so the claim was
    /// true of one writer and merely conventional for the other. This test writes the entity
    /// straight to the table, exactly as the sweep does, and requires the database itself to say
    /// no.
    /// </para>
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-SCH-COUNTED-13")]
    [InlineData("IVR_TECHNICAL_EXCEPTION")]
    [InlineData("IVR_CAPACITY_EXCEPTION")]
    public async Task ANonCustomerResultCannotBeStoredAsACustomerAttempt(string resultType)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-COUNTED-13",
            "JOB-SCH-COUNTED-13",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = string.Concat("RESULT-COUNTED-13-", resultType),
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            OrderVersionSnapshot = job.OrderVersionSnapshot,
            OrderVersionSeenByIvr = job.OrderVersionSnapshot,
            FinalResultStatus = resultType,
            ResultType = resultType,
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = resultType == "IVR_CAPACITY_EXCEPTION",
            RecommendedCoreAction = "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = true,
            CreatedAt = Now,
        });

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());

        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);
        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.Equal("ck_ivr_call_results_counted_matches_type", rejected.ConstraintName);
    }

    [Theory]
    [Trait("TestId", "IT-RESULT-CONTRACT-PRECALL-17")]
    [InlineData("IVR_OPERATIONAL_BLOCKED")]
    [InlineData("IVR_POLICY_BLOCKED")]
    public async Task APreCallCompatibilityValueCannotBeStoredAsACallResult(string resultType)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-RESULT-PRECALL-17",
            "JOB-RESULT-PRECALL-17",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallResults.Add(CreateResult(
            job,
            string.Concat("RESULT-PRECALL-17-", resultType),
            resultType,
            counted: false,
            final: false));

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());
        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);

        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.True(
            rejected.ConstraintName is "ck_ivr_call_results_result_type"
                or "ck_ivr_call_results_counted_matches_type"
                or "ck_ivr_call_results_finality_matches_type"
                or "ck_ivr_call_results_action_matches_type",
            $"Unexpected constraint: {rejected.ConstraintName}");
    }

    [Theory]
    [Trait("TestId", "IT-RESULT-CONTRACT-FINALITY-18")]
    [InlineData("IVR_CONFIRMED", true, false)]
    [InlineData("IVR_WRONG_INPUT", true, true)]
    public async Task ResultFinalityMustMatchTheClosedTaxonomy(
        string resultType,
        bool counted,
        bool final)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-RESULT-FINALITY-18",
            "JOB-RESULT-FINALITY-18",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallResults.Add(CreateResult(
            job,
            string.Concat("RESULT-FINALITY-18-", resultType),
            resultType,
            counted,
            final));

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());
        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);

        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.Equal("ck_ivr_call_results_finality_matches_type", rejected.ConstraintName);
    }

    [Fact]
    [Trait("TestId", "IT-RESULT-CONTRACT-ACTION-19")]
    public async Task RecommendedCoreActionMustMatchTheResultType()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-RESULT-ACTION-19",
            "JOB-RESULT-ACTION-19",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        CallResultEntity result = CreateResult(
            job,
            "RESULT-ACTION-19",
            "IVR_CONFIRMED",
            counted: true,
            final: true);
        result.RecommendedCoreAction = "REVALIDATE_AND_HOLD_ADMIN_REVIEW";
        seed.CallResults.Add(result);

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());
        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);

        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.Equal("ck_ivr_call_results_action_matches_type", rejected.ConstraintName);
    }

    [Fact]
    [Trait("TestId", "IT-RESULT-CONTRACT-OUTBOX-19")]
    public async Task CallbackOutboxAcceptsOnlyTheSixFinalResultTypes()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-RESULT-OUTBOX-19",
            "JOB-RESULT-OUTBOX-19",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        CallResultEntity result = CreateResult(
            job,
            "RESULT-OUTBOX-19",
            "IVR_CONFIRMED",
            counted: true,
            final: true);
        seed.CallResults.Add(result);
        await seed.SaveChangesAsync();
        seed.ResultCallbacks.Add(new ResultCallbackEntity
        {
            CallbackId = "CALLBACK-OUTBOX-19",
            IvrCallResultId = result.IvrCallResultId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            IdempotencyKey = "callback-outbox-19",
            ResultStatus = "IVR_WRONG_INPUT",
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "READY",
            RequiresCoreRevalidation = true,
            PayloadJson = "{}",
            PayloadSha256 = new string('A', 64),
            CreatedAt = Now,
        });

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());
        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);

        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.Equal("ck_ivr_result_callbacks_result_status", rejected.ConstraintName);
    }

    /// <summary>
    /// W-0118. The same invariant on the table the scheduler actually counts.
    /// <para>
    /// <c>TryClaimDueDispatchAsync</c> decides whether another call is owed by counting rows in
    /// <c>ivr_call_attempts</c> where <c>is_counted_customer_attempt IS TRUE</c>. So a non-customer
    /// outcome counted here does not merely misreport — it spends one of the two attempts the
    /// policy promised, and the order can reach its final attempt without the phone having rung
    /// twice.
    /// </para>
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-SCH-COUNTED-15")]
    [InlineData("IVR_TECHNICAL_EXCEPTION")]
    [InlineData("IVR_CAPACITY_EXCEPTION")]
    public async Task ANonCustomerAttemptCannotBeStoredAsACustomerAttempt(string resultStatus)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-COUNTED-15",
            "JOB-SCH-COUNTED-15",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = string.Concat("ATTEMPT-COUNTED-15-", resultStatus),
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 2,
            ScheduledAt = Now.AddMinutes(-5),
            ScheduledWindowExpiresAt = Now,
            Status = "NORMALIZED_FINAL",
            ResultStatus = resultStatus,
            IsCountedCustomerAttempt = true,
            PolicyVersion = job.AttemptPolicyCode,
            ScriptVersion = job.ScriptVersion,
        });

        DbUpdateException failure = await Assert.ThrowsAsync<DbUpdateException>(
            () => seed.SaveChangesAsync());

        PostgresException rejected = Assert.IsType<PostgresException>(failure.InnerException);
        Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
        Assert.Equal("ck_ivr_call_attempts_counted_matches_type", rejected.ConstraintName);
    }

    [Fact]
    [Trait("TestId", "IT-RESULT-CONTRACT-PREFLIGHT-20")]
    public async Task MigrationPreflightNamesLegacyRowsThatViolateTheSignedTaxonomy()
    {
        IDbContextFactory<IvrDbContext> factory = Factory();
        try
        {
            await using IvrDbContext migration = await factory.CreateDbContextAsync();
            await migration.Database.EnsureDeletedAsync();
            string[] migrations = [.. migration.Database.GetMigrations()];
            int targetIndex = Array.FindIndex(
                migrations,
                item => item.EndsWith(
                    "_W0172ProgramResultContractInvariants",
                    StringComparison.Ordinal));
            Assert.True(targetIndex > 0);
            string previous = migrations[targetIndex - 1];
            string target = migrations[targetIndex];
            await migration.GetService<IMigrator>().MigrateAsync(previous);

            await SeedReadyJobAsync(
                factory,
                "TASK-RESULT-PREFLIGHT-20",
                "JOB-RESULT-PREFLIGHT-20",
                Now.AddMinutes(-5),
                expiresAt: Now);
            await using IvrDbContext legacy = await factory.CreateDbContextAsync();
            CallJobEntity job = await legacy.CallJobs.SingleAsync();
            legacy.CallResults.Add(CreateResult(
                job,
                "RESULT-PREFLIGHT-20",
                "IVR_CONFIRMED",
                counted: false,
                final: true));
            await legacy.SaveChangesAsync();

            PostgresException blocked = await Assert.ThrowsAsync<PostgresException>(
                () => legacy.GetService<IMigrator>().MigrateAsync(target));

            Assert.Equal(PostgresErrorCodes.CheckViolation, blocked.SqlState);
            Assert.Contains(
                "W-0172 program/result preflight blocked",
                blocked.MessageText,
                StringComparison.Ordinal);
            Assert.Contains("result:RESULT-PREFLIGHT-20", blocked.MessageText, StringComparison.Ordinal);
            Assert.Equal(previous, (await legacy.Database.GetAppliedMigrationsAsync()).Last());
        }
        finally
        {
            await fixture.ResetAsync();
        }
    }

    /// <summary>
    /// W-0118. An attempt row exists before its outcome is known, and the constraint must not
    /// stand in the way of that. A leased attempt has no result_status yet, so the rule has
    /// nothing to judge and must let the row through.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-COUNTED-16")]
    public async Task AnAttemptWithNoResultYetIsNotJudgedByTheInvariant()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-COUNTED-16",
            "JOB-SCH-COUNTED-16",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = "ATTEMPT-COUNTED-16",
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 2,
            ScheduledAt = Now.AddMinutes(-5),
            ScheduledWindowExpiresAt = Now,
            Status = "LEASED_PENDING_DISPATCH",
            ResultStatus = null,
            IsCountedCustomerAttempt = false,
            PolicyVersion = job.AttemptPolicyCode,
            ScriptVersion = job.ScriptVersion,
        });

        await seed.SaveChangesAsync();

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity stored = await verification.CallAttempts.AsNoTracking().SingleAsync();
        Assert.Null(stored.ResultStatus);
        Assert.False(stored.IsCountedCustomerAttempt);
    }

    /// <summary>
    /// W-0117. The other half: the constraint must not have been written so broadly that it also
    /// refuses the results that genuinely are customer attempts. A rule that rejects everything
    /// passes the negative test above while quietly breaking every confirmed order.
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-SCH-COUNTED-14")]
    [InlineData("IVR_CONFIRMED", "REVALIDATE_AND_CONFIRM_ORDER")]
    [InlineData("IVR_CUSTOMER_CANCELLED", "REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST")]
    [InlineData("IVR_NO_ANSWER_FINAL", "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT")]
    public async Task AGenuineCustomerAttemptIsStillAccepted(string resultType, string coreAction)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-COUNTED-14",
            "JOB-SCH-COUNTED-14",
            Now.AddMinutes(-5),
            expiresAt: Now);

        await using IvrDbContext seed = await factory.CreateDbContextAsync();
        CallJobEntity job = await seed.CallJobs.SingleAsync();
        seed.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = string.Concat("RESULT-COUNTED-14-", resultType),
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            OrderVersionSnapshot = job.OrderVersionSnapshot,
            OrderVersionSeenByIvr = job.OrderVersionSnapshot,
            FinalResultStatus = resultType,
            ResultType = resultType,
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = coreAction,
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            CreatedAt = Now,
        });

        await seed.SaveChangesAsync();

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallResultEntity stored = await verification.CallResults.AsNoTracking().SingleAsync();
        Assert.Equal(resultType, stored.ResultType);
        Assert.True(stored.IsCountedCustomerAttempt);
    }

    /// <summary>
    /// W-0116. A dry run never asks for a channel, so its deadline cannot be evidence of a channel
    /// shortage. This is the case that made the old counter unusable for procurement: mock traffic
    /// and real queue pressure were indistinguishable once both closed as CAPACITY_MISSED.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-12")]
    public async Task DryRunDeadlineIsNeverReportedAsCapacityShortage()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-DEADLINE-12",
            "JOB-SCH-DEADLINE-12",
            Now.AddMinutes(-5),
            expiresAt: Now);
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await seed.CallJobs.SingleAsync();
            job.Status = "DRY_RUN";
            job.QueueStatus = "HELD_MOCK";
            await seed.SaveChangesAsync();
        }

        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        Assert.Equal(1, await store.CloseMissedDeadlinesAsync(Now, 16));
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.Equal(
            "IVR_CONFIRMATION_WINDOW_EXPIRED",
            (await verification.CallResults.AsNoTracking().SingleAsync()).ResultType);
        Assert.Equal(
            "WINDOW_EXPIRED",
            (await verification.CallJobs.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    [Trait("TestId", "IT-SCH-HOLD-10")]
    public async Task JobScopedCapacityIncidentDoesNotFreezeUnrelatedDispatch()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(factory, "TASK-SCH-HOLD-10", "JOB-SCH-HOLD-10", Now);
        await SeedChannelAsync(factory, "SIM-LAB-HOLD-010");
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            seed.CapacityIncidents.Add(new CapacityIncidentEntity
            {
                CapacityIncidentId = "CAP-JOB-SCOPED-10",
                SessionId = "SESSION-JOB-SCOPED-10",
                ProgramCode = "GOLDEN_HOUR",
                Status = "OPEN",
                Scope = "ELIGIBILITY_DEADLINE",
                HoldNewCalls = true,
                OpenedAt = Now,
                Reason = "LEGACY_JOB_SCOPED_INCIDENT",
            });
            await seed.SaveChangesAsync();
        }
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        SchedulerDispatchLease? lease = await store.TryClaimDueDispatchAsync(
            "worker-job-scoped",
            IvrOptions.LabRealSimExecutionMode,
            TimeSpan.FromMinutes(2));

        Assert.NotNull(lease);
    }

    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-08")]
    public async Task AttemptClaimedBeforeDeadlineIsNotMisclassifiedAsCapacityMiss()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        DateTimeOffset startedAt = Now.AddMinutes(-5);
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-DEADLINE-08",
            "JOB-SCH-DEADLINE-08",
            startedAt,
            expiresAt: Now);
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await seed.CallJobs.SingleAsync();
            job.Status = "DISPATCH_LEASED";
            job.QueueStatus = "LEASED";
            seed.CallAttempts.Add(new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-SCH-DEADLINE-08",
                IvrCallJobId = job.IvrCallJobId,
                TaskId = job.TaskId,
                AttemptNumber = 1,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = startedAt,
                ScheduledWindowExpiresAt = Now,
                StartedAt = Now.AddSeconds(-10),
                Status = "ACTIVE_CALL",
                IsCountedCustomerAttempt = false,
                PolicyVersion = job.AttemptPolicyCode,
                ScriptVersion = job.ScriptVersion,
            });
            await seed.SaveChangesAsync();
        }
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        int closed = await store.CloseMissedDeadlinesAsync(Now, 16);

        Assert.Equal(0, closed);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.Equal(0, await verification.CallResults.CountAsync());
        Assert.Equal("DISPATCH_LEASED", (await verification.CallJobs.SingleAsync()).Status);
    }

    [Fact]
    [Trait("TestId", "IT-SCH-FINAL-04")]
    public async Task FinalResultPreventsFutureAttemptEvenWhenSecondOffsetIsDue()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(
            factory,
            "TASK-SCH-FINAL-04",
            "JOB-SCH-FINAL-04",
            Now.AddMinutes(-4),
            Now.AddMinutes(1));
        await SeedChannelAsync(factory, "SIM-LAB-FINAL-001");
        await using (IvrDbContext seed = await factory.CreateDbContextAsync())
        {
            CallJobEntity job = await seed.CallJobs.SingleAsync();
            seed.CallAttempts.Add(new CallAttemptEntity
            {
                IvrCallAttemptId = "ATTEMPT-SCH-FINAL-04-1",
                IvrCallJobId = job.IvrCallJobId,
                TaskId = job.TaskId,
                AttemptNumber = 1,
                MaxAttemptsSnapshot = 2,
                ScheduledAt = Now.AddMinutes(-4),
                ScheduledWindowExpiresAt = Now.AddMinutes(1),
                StartedAt = Now.AddMinutes(-4),
                EndedAt = Now.AddMinutes(-3),
                Status = "NORMALIZED_FINAL",
                ResultStatus = "IVR_CONFIRMED",
                IsCountedCustomerAttempt = true,
                PolicyVersion = job.AttemptPolicyCode,
                ScriptVersion = job.ScriptVersion,
            });
            seed.CallResults.Add(new CallResultEntity
            {
                IvrCallResultId = "RESULT-SCH-FINAL-04",
                IvrCallJobId = job.IvrCallJobId,
                TaskId = job.TaskId,
                OfficialOrderId = job.OfficialOrderId,
                OrderVersionSnapshot = job.OrderVersionSnapshot,
                OrderVersionSeenByIvr = job.OrderVersionSnapshot,
                FinalResultStatus = "IVR_CONFIRMED",
                ResultType = "IVR_CONFIRMED",
                IsCountedCustomerAttempt = true,
                IsFinalForIvr = true,
                RecommendedCoreAction = "REVALIDATE_AND_CONFIRM_ORDER",
                CoreOrderHandoffRequired = true,
                HumanReviewRequired = false,
                CreatedAt = Now.AddMinutes(-3),
            });
            await seed.SaveChangesAsync();
        }
        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        SchedulerDispatchLease? next = await store.TryClaimDueDispatchAsync(
            "worker-final",
            IvrOptions.LabRealSimExecutionMode,
            TimeSpan.FromMinutes(2));

        Assert.Null(next);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(1, await verification.CallAttempts.CountAsync());
        Assert.Null((await verification.SimChannels.AsNoTracking().SingleAsync()).LeaseToken);
    }

    [Fact]
    [Trait("TestId", "IT-POLICY-AUDIT-05")]
    public async Task AlternatePolicyIsRegisteredAsNewVersionAndAudited()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        var writer = new PostgresAttemptPolicyRegistryWriter(
            factory,
            new FixedTimeProvider(Now));
        AttemptPolicySnapshot policy = AttemptPolicySnapshot.Create(
            PolicyVersion.Create("alternate-three-v1"),
            IvrProgramCode.GoldenHour,
            3,
            [TimeSpan.Zero, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(105)],
            TimeSpan.FromSeconds(180),
            AttemptPolicyApproval.OwnerApproved);

        await writer.RegisterNewVersionAsync(
            policy,
            [ExecutionMode.Mock, ExecutionMode.LabRealSim, ExecutionMode.ProductionReal],
            "policy-owner-test",
            "owner-approved-test-policy",
            "corr-policy-audit-05");
        var registry = new PostgresAttemptPolicyRegistry(factory);
        AttemptPolicySnapshot resolved = await registry.ResolveAsync(
            policy.Version,
            policy.Program,
            ExecutionMode.ProductionReal,
            CancellationToken.None);

        Assert.Equal(3, resolved.MaxCustomerAttempts);
        Assert.Equal([0, 45, 105], resolved.AttemptOffsets.Select(Value).ToArray());
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        AuditLogEntity audit = await verification.AuditLog.AsNoTracking().SingleAsync();
        Assert.Equal("ATTEMPT_POLICY_VERSION_REGISTERED", audit.Action);
        Assert.Contains(policy.ComputeHash(), audit.DataJson, StringComparison.Ordinal);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            writer.RegisterNewVersionAsync(
                policy,
                [ExecutionMode.Mock],
                "policy-owner-test",
                "duplicate-test",
                "corr-policy-audit-duplicate"));
    }

    [Fact]
    [Trait("TestId", "IT-POLICY-PROD-06")]
    public async Task CandidatePolicyRegistrationFailsClosedForProduction()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        var writer = new PostgresAttemptPolicyRegistryWriter(
            factory,
            new FixedTimeProvider(Now));
        AttemptPolicySnapshot candidate = AttemptPolicySnapshot.Create(
            PolicyVersion.Create("candidate-prod-forbidden-v1"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromSeconds(300),
            AttemptPolicyApproval.CandidateMockLabOnly);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            writer.RegisterNewVersionAsync(
                candidate,
                [ExecutionMode.ProductionReal],
                "policy-owner-test",
                "candidate-must-fail",
                "corr-policy-prod-06"));
        await using IvrDbContext verification = await factory.CreateDbContextAsync();

        // W-0196. The claim is "the refused registration wrote nothing", which used to be said as
        // "the table is empty". The shipped schema now seeds the signed gh-247-prod-v1 rows, so
        // emptiness no longer means what it did - the refused version's absence does, and says it
        // more precisely than a count ever did.
        Assert.False(await verification.AttemptPolicies.AnyAsync(
            policy => policy.PolicyVersion == "candidate-prod-forbidden-v1"));

        // The audit log genuinely is empty, and stays a count: a refused registration must not
        // leave a trace of a registration that did not happen, and nothing seeds this table.
        Assert.Equal(0, await verification.AuditLog.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-SCH-CAPACITY-07")]
    public async Task EligibilityCapacityProviderUsesSchedulerCalculationInNonMockMode()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedReadyJobAsync(factory, "TASK-SCH-CAP-07", "JOB-SCH-CAP-07", Now);
        await SeedChannelAsync(factory, "SIM-LAB-CAP-001");
        await using IvrDbContext read = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await read.ConfirmationTasks.AsNoTracking().SingleAsync();
        CallJobEntity job = await read.CallJobs.AsNoTracking().SingleAsync();
        IEligibilityCapacityProvider provider = fixture.Services
            .GetRequiredService<IEligibilityCapacityProvider>();

        EligibilityCapacitySnapshot capacity = await provider.GetCapacityAsync(
            new EligibilityTaskRecord(
                task,
                job,
                new TaskIntakeOutboxEntity
                {
                    TaskId = task.TaskId,
                    IvrCallJobId = job.IvrCallJobId,
                }),
            Now);

        Assert.IsType<SchedulerEligibilityCapacityProvider>(provider);
        Assert.True(capacity.SourceAvailable);
        Assert.True(capacity.CanMeetDeadline);
        Assert.Equal(1, capacity.ActiveSimCount);
        Assert.Equal(2, capacity.PendingCallJobs);
        Assert.StartsWith("evidence://ivr/p2-3/scheduler-capacity/", capacity.EvidenceRef,
            StringComparison.Ordinal);
    }

    [Theory]
    [Trait("TestId", "PT-CAP-01")]
    [InlineData(1, 8)]
    [InlineData(4, 24)]
    public async Task OverCapacityHoldsJobsWithoutLosingOneAndNeverDoubleBooksAChannel(
        int channels,
        int jobs)
    {
        // W-0037 / P5-3 §6.1-6.2. Two shapes on purpose: the one-SIM lab that is actually going
        // to happen first, and a wider pool. Both are pushed past capacity, because the question
        // is not "does it work when there is room" — it is what happens when there is not.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        for (int index = 0; index < channels; index++)
        {
            await SeedChannelAsync(
                factory,
                string.Concat("SIM-PT-CAP-", index.ToString("D3", CultureInfo.InvariantCulture)));
        }

        for (int index = 0; index < jobs; index++)
        {
            string suffix = index.ToString("D3", CultureInfo.InvariantCulture);
            await SeedReadyJobAsync(
                factory,
                string.Concat("TASK-PT-CAP-", suffix),
                string.Concat("JOB-PT-CAP-", suffix),
                Now);
        }

        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        // Every worker races at once. Serial claiming would prove nothing about contention.
        SchedulerDispatchLease?[] leases = await Task.WhenAll(
            Enumerable.Range(0, jobs).Select(worker => store.TryClaimDueDispatchAsync(
                string.Concat("worker-", worker.ToString(CultureInfo.InvariantCulture)),
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2))));

        SchedulerDispatchLease[] granted = leases.OfType<SchedulerDispatchLease>().ToArray();
        await using IvrDbContext verification = await factory.CreateDbContextAsync();

        // ONE_SIM_ONE_ACTIVE_CALL. The hard invariant: a channel carrying two calls is two
        // customers hearing each other's order, so this is the assertion that matters most.
        Assert.True(granted.Length <= channels, $"granted {granted.Length} leases for {channels} channels");
        List<SimChannelEntity> reserved = await verification.SimChannels.AsNoTracking()
            .Where(channel => channel.Status == "RESERVED")
            .ToListAsync();
        Assert.Equal(granted.Length, reserved.Count);
        Assert.Equal(
            reserved.Count,
            reserved.Select(channel => channel.ActiveCallJobId).Distinct(StringComparer.Ordinal).Count());

        // No task is lost under overload. Every job seeded is still accounted for — the ones that
        // could not get a channel are waiting, not dropped, and none was silently marked done.
        List<CallJobEntity> allJobs = await verification.CallJobs.AsNoTracking().ToListAsync();
        Assert.Equal(jobs, allJobs.Count);
        Assert.Equal(granted.Length, allJobs.Count(job => job.Status == "DISPATCH_LEASED"));

        // A lease is not a customer attempt: it is IVR reserving a channel, and overload must
        // never spend a customer's limited attempts (DT-02 / DT-04).
        Assert.Empty(await verification.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IsCountedCustomerAttempt)
            .ToListAsync());
    }

    [Fact]
    [Trait("TestId", "PT-CAP-02")]
    public async Task DeliberateOverloadRecordsCapacityIncidentForEveryJobThatNeverGotAChannel()
    {
        // W-0131 / M8-P0-009 (spec §23). PT-CAP-01 already proves contention is safe at 1/8 and
        // 4/24, but it never asserts the one thing the SIM order is actually sized by: that a burst
        // which cannot fit leaves a capacity incident behind. The cross-audit flagged this exact
        // acceptance as missing, so this test is that scenario and nothing else -- 32 channels, 800
        // jobs dropped into a single 5-minute window, where five-minute capacity is only ~192. The
        // shortfall has to be recorded, not quietly absorbed.
        //
        // Scope kept deliberately narrow: this does NOT model channels recycling between calls, so
        // it does not prove the ~192-per-five-minutes throughput figure. That number stays
        // UNCALIBRATED until W-0008 supplies a measured call duration.
        const int channels = 32;
        const int jobs = 800;

        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        DateTimeOffset windowExpiresAt = Now.AddMinutes(5);

        await SeedChannelPoolAsync(factory, channels);
        await SeedReadyJobBurstAsync(factory, jobs, Now, windowExpiresAt);

        var store = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));

        // All 800 want a channel at once. Parallelism is bounded because the point is database
        // contention, not exhausting the Npgsql pool -- 64 in flight is still twice the channel
        // count, which is what makes the ONE_SIM_ONE_ACTIVE_CALL assertion below mean something.
        using var gate = new SemaphoreSlim(64);
        SchedulerDispatchLease?[] leases = await Task.WhenAll(
            Enumerable.Range(0, jobs).Select(async worker =>
            {
                await gate.WaitAsync();
                try
                {
                    return await store.TryClaimDueDispatchAsync(
                        string.Concat("worker-", worker.ToString(CultureInfo.InvariantCulture)),
                        IvrOptions.LabRealSimExecutionMode,
                        TimeSpan.FromMinutes(2));
                }
                finally
                {
                    gate.Release();
                }
            }));

        // ONE_SIM_ONE_ACTIVE_CALL under a burst 25x the channel count.
        List<SchedulerDispatchLease> granted = [.. leases.OfType<SchedulerDispatchLease>()];
        Assert.True(granted.Count <= channels, $"granted {granted.Count} leases for {channels} channels");

        // SKIP LOCKED lets a racing worker miss a channel that was only briefly locked, which would
        // make the count below flaky. A short sequential top-up removes the flake without weakening
        // the claim: given an unhurried chance the scheduler fills every channel it has, and only
        // what is left over after that is honestly a shortage.
        while (granted.Count < channels)
        {
            SchedulerDispatchLease? extra = await store.TryClaimDueDispatchAsync(
                string.Concat("worker-topup-", granted.Count.ToString(CultureInfo.InvariantCulture)),
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2));
            if (extra is null)
            {
                break;
            }

            granted.Add(extra);
        }

        Assert.Equal(channels, granted.Count);

        await using (IvrDbContext underLoad = await factory.CreateDbContextAsync())
        {
            List<SimChannelEntity> reserved = await underLoad.SimChannels.AsNoTracking()
                .Where(channel => channel.Status == "RESERVED")
                .ToListAsync();
            Assert.Equal(channels, reserved.Count);
            Assert.Equal(
                reserved.Count,
                reserved.Select(channel => channel.ActiveCallJobId).Distinct(StringComparer.Ordinal).Count());
        }

        // The window closes with 768 jobs that never got dispatched. Sweeping in a loop is required
        // because the store caps batchSize at 512, and it doubles as the "khong batch" half of
        // M8-P0-009: draining until the sweep returns zero proves nothing was left behind by the
        // batch boundary.
        int closed = 0;
        int swept;
        do
        {
            swept = await store.CloseMissedDeadlinesAsync(windowExpiresAt.AddSeconds(1), 512);
            closed += swept;
        }
        while (swept > 0);

        int expectedMisses = jobs - channels;
        Assert.Equal(expectedMisses, closed);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();

        // Nothing lost under overload: every seeded job survives the sweep.
        Assert.Equal(jobs, await verification.CallJobs.AsNoTracking().CountAsync());

        // The assertion PT-CAP-01 does not make. Overload has to be visible as a capacity incident,
        // because that counter is what sizes the SIM purchase (M8-OD-A). A silent shortage would
        // read as "we had enough channels".
        List<CapacityIncidentEntity> incidents =
            await verification.CapacityIncidents.AsNoTracking().ToListAsync();
        Assert.Equal(expectedMisses, incidents.Count);
        Assert.All(incidents, incident =>
        {
            Assert.Equal("SCHEDULER_DEADLINE", incident.Scope);
            Assert.Equal("NO_DISPATCH_BEFORE_DEADLINE", incident.ShortageReason);
            Assert.Equal("IVR_CAPACITY_EXCEPTION", incident.Reason);
            Assert.Equal("OPEN", incident.Status);
            Assert.Equal(channels, incident.ActiveSimCount);
        });

        // Overload must never spend a customer's limited attempts (DT-02 / DT-04). The 768 that
        // missed were never called, so no counted attempt may exist anywhere.
        Assert.Empty(await verification.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IsCountedCustomerAttempt)
            .ToListAsync());
    }

    // Bulk seeders for PT-CAP-02 only. SeedReadyJobAsync/SeedChannelAsync open one DbContext and
    // one SaveChanges per row, which is right for a handful of rows and wrong for 832 of them, so
    // these batch into a single context rather than changing the behaviour of a shared helper that
    // twenty other tests depend on.
    private static async Task SeedChannelPoolAsync(
        IDbContextFactory<IvrDbContext> factory,
        int channels)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        for (int index = 0; index < channels; index++)
        {
            string channelId = string.Concat(
                "SIM-PT-CAP2-",
                index.ToString("D3", CultureInfo.InvariantCulture));
            context.SimChannels.Add(new SimChannelEntity
            {
                SimChannelId = channelId,
                SimNumberRef = string.Concat("sim-ref-", channelId),
                Enabled = true,
                Status = "IDLE",
                AdapterMode = "VENDOR",
                ExecutionMode = IvrOptions.LabRealSimExecutionMode,
                ProviderName = "VENDOR",
                LastHealthCheckAt = Now,
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedReadyJobBurstAsync(
        IDbContextFactory<IvrDbContext> factory,
        int jobs,
        DateTimeOffset startedAt,
        DateTimeOffset expiresAt)
    {
        string schedule = JsonSerializer.Serialize(new[]
        {
            startedAt,
            startedAt.AddSeconds(150),
        });

        await using IvrDbContext context = await factory.CreateDbContextAsync();
        for (int index = 0; index < jobs; index++)
        {
            string suffix = index.ToString("D4", CultureInfo.InvariantCulture);
            string taskId = string.Concat("TASK-PT-CAP2-", suffix);
            context.ConfirmationTasks.Add(new ConfirmationTaskEntity
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                ContractVersion = "ivr-order-confirmation.v1",
                IdempotencyKey = string.Concat("scheduler:", taskId),
                CorrelationId = string.Concat("corr-", taskId),
                OfficialOrderId = string.Concat("ORDER-", taskId),
                OrderCode = string.Concat("GF-", taskId),
                OrderVersion = "1",
                OrderState = "CONFIRMING",
                PaymentMethodSnapshot = "ONLINE",
                IvrConfirmationRequired = true,
                RiskFlagsJson = "[]",
                ProgramType = "GOLDEN_HOUR",
                AttemptPolicyVersion = CandidateAttemptPolicies.Version,
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,150]",
                ConfirmationWindowStartedAt = startedAt,
                ConfirmationWindowExpiresAt = expiresAt,
                PhoneRef = string.Concat("phone-ref-", taskId),
                PhoneMasked = "84xxxxx0020",
                PhoneValidationStatus = "VALID",
                DialTokenCiphertext = string.Concat("enc:", taskId),
                DialTokenExpiresAt = expiresAt,
                PrivacySafeOrderSummaryJson = "{}",
                CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
                CallScriptVersion = "v1-test-approved",
                EvidencePolicyVersion = "evidence-v1",
                PrivacyPolicyVersion = "privacy-v1",
                EligibilityDecision = "ELIGIBLE_FOR_IVR",
                EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
                CallRestriction = false,
                CreatedAt = startedAt,
                ExpiresAt = expiresAt,
                AcceptedAt = startedAt,
            });
            context.CallJobs.Add(new CallJobEntity
            {
                IvrCallJobId = string.Concat("JOB-PT-CAP2-", suffix),
                TaskId = taskId,
                OfficialOrderId = string.Concat("ORDER-", taskId),
                OrderVersionSnapshot = "1",
                ProgramType = "GOLDEN_HOUR",
                AttemptPolicyCode = CandidateAttemptPolicies.Version,
                Status = "READY_FOR_SCHEDULER",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,150]",
                ConfirmationWindowSeconds = 300,
                AttemptScheduleJson = schedule,
                T0At = startedAt,
                ExpiresAt = expiresAt,
                Eligible = true,
                EligibilityDecision = "ELIGIBLE_FOR_IVR",
                QueueStatus = "QUEUED",
                ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
                PrivacyPolicyVersion = "privacy-v1",
                CreatedAt = startedAt,
            });
        }

        await context.SaveChangesAsync();
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static CallResultEntity CreateResult(
        CallJobEntity job,
        string resultId,
        string resultType,
        bool counted,
        bool final) => new()
        {
            IvrCallResultId = resultId,
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            OrderVersionSnapshot = job.OrderVersionSnapshot,
            OrderVersionSeenByIvr = job.OrderVersionSnapshot ?? "1",
            FinalResultStatus = resultType,
            ResultType = resultType,
            IsCountedCustomerAttempt = counted,
            IsFinalForIvr = final,
            RecommendedCoreAction = resultType switch
            {
                "IVR_CONFIRMED" => "REVALIDATE_AND_CONFIRM_ORDER",
                "IVR_WRONG_INPUT" => "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
                _ => "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
            },
            CoreOrderHandoffRequired = final,
            HumanReviewRequired = false,
            CreatedAt = Now,
        };

    private static async Task SeedReadyJobAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        DateTimeOffset startedAt,
        DateTimeOffset? expiresAt = null)
    {
        DateTimeOffset deadline = expiresAt ?? startedAt.AddMinutes(5);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = string.Concat("scheduler:", taskId),
            CorrelationId = string.Concat("corr-", taskId),
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderCode = string.Concat("GF-", taskId),
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = CandidateAttemptPolicies.Version,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = string.Concat("phone-ref-", taskId),
            PhoneMasked = "84xxxxx0020",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = string.Concat("enc:", taskId),
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            CallRestriction = false,
            CreatedAt = startedAt,
            ExpiresAt = deadline,
            AcceptedAt = startedAt,
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = CandidateAttemptPolicies.Version,
            Status = "READY_FOR_SCHEDULER",
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
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "QUEUED",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = startedAt,
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedChannelAsync(
        IDbContextFactory<IvrDbContext> factory,
        string channelId)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = channelId,
            SimNumberRef = string.Concat("sim-ref-", channelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "VENDOR",
            ExecutionMode = IvrOptions.LabRealSimExecutionMode,
            ProviderName = "VENDOR",
            LastHealthCheckAt = Now,
        });
        await context.SaveChangesAsync();
    }

    private static int Value(TimeSpan value) => checked((int)value.TotalSeconds);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
