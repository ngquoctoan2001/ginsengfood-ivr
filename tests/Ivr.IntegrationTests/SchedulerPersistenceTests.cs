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
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    [Trait("TestId", "IT-SCH-DEADLINE-09")]
    public async Task HeldAdminReviewJobStillClosesAtItsDeadline()
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
        Assert.Equal("CAPACITY_MISSED", closed.Status);
        Assert.NotNull(closed.ClosedAt);
        Assert.False((await verification.CapacityIncidents.AsNoTracking().SingleAsync()).HoldNewCalls);
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
                Status = "COMPLETED",
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
        Assert.Equal(0, await verification.AttemptPolicies.CountAsync());
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

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

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
            EligibilityDecision = "ELIGIBLE",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            SellableStatusJson = "[]",
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
            EligibilityDecision = "ELIGIBLE",
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
