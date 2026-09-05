using Ivr.Domain.Confirmation;
using Ivr.Domain.Scheduling;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Scheduling;

public sealed class DeadlineSchedulerTests
{
    private static readonly DateTimeOffset T0 =
        new(2026, 8, 13, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-SCH-POLICY-01")]
    public void CandidateAndAlternatePolicyProduceExactDataDrivenSchedules()
    {
        AttemptPolicySnapshot candidate = AttemptPolicySnapshot.Create(
            PolicyVersion.Create("mock-lab-v1"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromSeconds(300),
            AttemptPolicyApproval.CandidateMockLabOnly);
        AttemptPolicySnapshot alternate = AttemptPolicySnapshot.Create(
            PolicyVersion.Create("owner-approved-three-v1"),
            IvrProgramCode.GoldenHour,
            3,
            [TimeSpan.Zero, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(105)],
            TimeSpan.FromSeconds(180),
            AttemptPolicyApproval.OwnerApproved);

        Assert.Equal([0, 150], candidate.AttemptOffsets.Select(Value).ToArray());
        Assert.Equal([0, 45, 105], alternate.AttemptOffsets.Select(Value).ToArray());
        SchedulerCapacityRequest request = Request(
            "JOB-ALT",
            alternate,
            T0,
            riskScore: 0);
        IReadOnlyList<SchedulerQueueItem> items = SchedulerCapacityMapper.CreateQueueItems(request);

        Assert.Equal(3, items.Count);
        Assert.Equal([T0, T0.AddSeconds(45), T0.AddSeconds(105)],
            items.Select(item => item.DueAt).ToArray());
        Assert.Equal([1, 2, 3], items.Select(item => item.AttemptNumber).ToArray());
    }

    [Fact]
    [Trait("TestId", "UT-SCH-ORDER-02")]
    public void QueueOrderingUsesDeadlineProgramOffsetRiskCreationAndStableId()
    {
        DateTimeOffset deadline = T0.AddMinutes(5);
        SchedulerQueueItem[] items =
        [
            Item("JOB-24", IvrProgramCode.TwentyFourSeven, deadline, 0, 9, T0),
            Item("JOB-GH-LOW", IvrProgramCode.GoldenHour, deadline, 0, 1, T0),
            Item("JOB-GH-HIGH-B", IvrProgramCode.GoldenHour, deadline, 0, 5, T0),
            Item("JOB-GH-HIGH-A", IvrProgramCode.GoldenHour, deadline, 0, 5, T0),
            Item("JOB-LATER-OFFSET", IvrProgramCode.GoldenHour, deadline, 30, 99, T0),
            Item("JOB-EARLIEST", IvrProgramCode.TwentyFourSeven, deadline.AddSeconds(-1), 30, 0, T0),
        ];

        IReadOnlyList<SchedulerQueueItem> ordered = DeadlineScheduler.Order(items);

        Assert.Equal(
        [
            "JOB-EARLIEST",
            "JOB-GH-HIGH-A",
            "JOB-GH-HIGH-B",
            "JOB-GH-LOW",
            "JOB-LATER-OFFSET",
            "JOB-24",
        ],
        ordered.Select(item => item.JobId).ToArray());
    }

    [Fact]
    [Trait("TestId", "UT-SCH-CLOCK-03")]
    public void DispatchAtDueBoundaryFitsButDispatchAtExpiryFails()
    {
        SchedulerQueueItem item = Item(
            "JOB-BOUNDARY",
            IvrProgramCode.GoldenHour,
            T0.AddMinutes(1),
            0,
            0,
            T0);
        SchedulerChannelAvailability channel = new("CHANNEL-1", T0);

        SchedulerCapacityPlan atDue = DeadlineScheduler.CalculateCapacity(
            [item],
            [channel],
            T0,
            TimeSpan.FromSeconds(30),
            item.JobId);
        SchedulerCapacityPlan atExpiry = DeadlineScheduler.CalculateCapacity(
            [item],
            [channel with { }],
            item.Deadline,
            TimeSpan.FromSeconds(30),
            item.JobId);

        Assert.True(atDue.FitsTargetDeadline);
        Assert.Equal(T0, atDue.LastTargetDispatchAt);
        Assert.False(atExpiry.FitsTargetDeadline);
        Assert.Equal(1, atExpiry.MissedDeadlineCount);
    }

    [Fact]
    [Trait("TestId", "UT-SCH-CAPACITY-04")]
    public void OneAndThirtyTwoChannelSimulationsAreConfigurationDriven()
    {
        SchedulerQueueItem[] oneChannelItems =
        [
            Item("TARGET-ONE", IvrProgramCode.GoldenHour, T0.AddSeconds(120), 0, 0, T0),
            Item("TARGET-ONE", IvrProgramCode.GoldenHour, T0.AddSeconds(120), 60, 0, T0, 2),
        ];
        SchedulerCapacityPlan oneChannel = DeadlineScheduler.CalculateCapacity(
            oneChannelItems,
            [new SchedulerChannelAvailability("CHANNEL-001", T0)],
            T0,
            TimeSpan.FromSeconds(60),
            "TARGET-ONE");
        SchedulerQueueItem[] thirtyTwoJobs = Enumerable.Range(1, 32)
            .Select(index => Item(
                $"JOB-{index:D2}",
                IvrProgramCode.GoldenHour,
                T0.AddMinutes(2),
                0,
                0,
                T0))
            .ToArray();
        SchedulerChannelAvailability[] thirtyTwoChannels = Enumerable.Range(1, 32)
            .Select(index => new SchedulerChannelAvailability($"CHANNEL-{index:D2}", T0))
            .ToArray();
        SchedulerCapacityPlan thirtyTwo = DeadlineScheduler.CalculateCapacity(
            thirtyTwoJobs,
            thirtyTwoChannels,
            T0,
            TimeSpan.FromSeconds(60),
            "JOB-32");

        Assert.True(oneChannel.FitsTargetDeadline);
        Assert.Equal(T0.AddSeconds(60), oneChannel.LastTargetDispatchAt);
        Assert.True(thirtyTwo.FitsTargetDeadline);
        Assert.Equal(32, thirtyTwo.ChannelCount);
        Assert.Equal(0, thirtyTwo.MissedDeadlineCount);
    }

    [Fact]
    [Trait("TestId", "UT-SCH-CAPACITY-05")]
    public void OverloadMarksTargetMissInsteadOfSilentlyExtendingWindow()
    {
        SchedulerQueueItem[] items = Enumerable.Range(1, 3)
            .Select(index => Item(
                $"JOB-{index}",
                IvrProgramCode.GoldenHour,
                T0.AddSeconds(90),
                0,
                0,
                T0))
            .ToArray();

        SchedulerCapacityPlan plan = DeadlineScheduler.CalculateCapacity(
            items,
            [new SchedulerChannelAvailability("CHANNEL-1", T0)],
            T0,
            TimeSpan.FromSeconds(60),
            "JOB-3");

        Assert.False(plan.FitsTargetDeadline);
        Assert.Equal(1, plan.MissedDeadlineCount);
    }

    [Fact]
    [Trait("TestId", "UT-SCH-RETRY-06")]
    public void FinalResultStopsFutureAttemptAndTechnicalRetryHasSeparateBound()
    {
        var active = new SchedulerAttemptProgress(1, 0, false);
        var final = new SchedulerAttemptProgress(1, 0, true);
        var technicalBoundReached = new SchedulerAttemptProgress(1, 1, false);

        Assert.Equal(2, active.NextCustomerAttemptNumber(2));
        Assert.True(active.CanUseTechnicalRetry(1));
        Assert.Null(final.NextCustomerAttemptNumber(2));
        Assert.False(final.CanUseTechnicalRetry(1));
        Assert.False(technicalBoundReached.CanUseTechnicalRetry(1));
        Assert.Equal(2, technicalBoundReached.NextCustomerAttemptNumber(2));
    }

    [Fact]
    [Trait("TestId", "UT-SCH-CONFIG-07")]
    public void SchedulerConfigurationRejectsInvalidCapacityAndUnboundedRetry()
    {
        var validator = new SchedulerOptionsValidator();
        Microsoft.Extensions.Options.ValidateOptionsResult result = validator.Validate(
            null,
            new SchedulerOptions
            {
                MockChannelCount = 0,
                ExpectedCallDurationSeconds = 60,
                LeaseDurationSeconds = 30,
                RecoveryQuarantineSeconds = 0,
                TechnicalRetryLimit = 11,
                ClaimBatchSize = 0,
                PollIntervalMilliseconds = 99,
            });

        Assert.False(result.Succeeded);
        Assert.Equal(6, Assert.IsAssignableFrom<IEnumerable<string>>(result.Failures).Count());
    }

    /// <summary>
    /// W-0198. These assert scheduler mechanics, not the hour of day, so they run against a
    /// window that is always open. The window itself is covered by UT-SCH-WINDOW-*.
    /// </summary>
    private static readonly CallingWindow AlwaysOpenWindow =
        new(Options.Create(new CallingWindowOptions { Enabled = false }));

    [Fact]
    [Trait("TestId", "UT-SCH-RUNTIME-08")]
    public async Task RuntimeIsDisabledByDefaultAndCannotClaimWithoutReadyGateway()
    {
        var store = new RecordingSchedulerStore();
        var unavailable = new UnavailableSchedulerDispatchGateway();
        var disabled = new SchedulerRuntime(
            store,
            unavailable,
            Options.Create(new SchedulerOptions()),
            new SchedulerExecutionContext(IvrOptions.LabRealSimExecutionMode),
            AlwaysOpenWindow,
            new FixedTimeProvider(T0));

        SchedulerRunResult disabledResult = await disabled.RunOnceAsync("worker-disabled");

        Assert.False(disabledResult.Enabled);
        Assert.Equal(0, store.MaintenanceCalls);
        SchedulerRuntime enabled = new(
            store,
            unavailable,
            Options.Create(new SchedulerOptions { Enabled = true }),
            new SchedulerExecutionContext(IvrOptions.LabRealSimExecutionMode),
            AlwaysOpenWindow,
            new FixedTimeProvider(T0));
        SchedulerRunResult held = await enabled.RunOnceAsync("worker-held");
        Assert.True(held.Enabled);
        Assert.False(held.DispatchGatewayReady);
        Assert.Equal(2, store.MaintenanceCalls);
        Assert.Equal(0, store.ClaimCalls);
    }

    [Fact]
    [Trait("TestId", "UT-SCH-RUNTIME-09")]
    public async Task ReadyGatewayReceivesExactlyOneAtomicDispatchLease()
    {
        var store = new RecordingSchedulerStore
        {
            Lease = new SchedulerDispatchLease(
                "JOB-RUNTIME",
                "ATTEMPT-RUNTIME",
                1,
                T0,
                T0.AddMinutes(5),
                "CHANNEL-RUNTIME",
                Guid.NewGuid(),
                7,
                T0.AddMinutes(2),
                "MOCK",
                "MOCK"),
        };
        var gateway = new RecordingDispatchGateway();
        var runtime = new SchedulerRuntime(
            store,
            gateway,
            Options.Create(new SchedulerOptions { Enabled = true }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            AlwaysOpenWindow,
            new FixedTimeProvider(T0));

        SchedulerRunResult result = await runtime.RunOnceAsync("worker-ready");

        Assert.True(result.DispatchClaimed);
        Assert.Equal(1, store.ClaimCalls);
        Assert.Equal(IvrOptions.MockExecutionMode, store.LastExecutionMode);
        Assert.Same(store.Lease, gateway.Lease);
    }

    /// <summary>
    /// W-0198 / OD-V1-16. Outside the calling window no dial is claimed — and the recovery work
    /// still runs.
    /// <para>
    /// The second half is the part worth pinning. Suspending the whole loop overnight would mean
    /// every morning started with a backlog of leases nobody released and confirmation windows
    /// that expired hours earlier with no record. Only dialling stops; bookkeeping wakes nobody.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-07")]
    public async Task OutsideTheCallingWindowNothingDialsButRecoveryStillRuns()
    {
        var store = new RecordingSchedulerStore
        {
            Lease = new SchedulerDispatchLease(
                "JOB-NIGHT",
                "ATTEMPT-NIGHT",
                1,
                T0,
                T0.AddMinutes(5),
                "CHANNEL-NIGHT",
                Guid.NewGuid(),
                7,
                T0.AddMinutes(2),
                "MOCK",
                "MOCK"),
        };
        var gateway = new RecordingDispatchGateway();

        // 03:00 in Vietnam, expressed as the instant it happens at.
        DateTimeOffset threeInTheMorning =
            new DateTimeOffset(2026, 9, 5, 3, 0, 0, TimeSpan.FromHours(7)).ToUniversalTime();
        var runtime = new SchedulerRuntime(
            store,
            gateway,
            Options.Create(new SchedulerOptions { Enabled = true }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            new FixedTimeProvider(threeInTheMorning));

        SchedulerRunResult result = await runtime.RunOnceAsync("worker-night");

        Assert.False(result.CallingWindowOpen);
        Assert.False(result.DispatchClaimed);
        Assert.Equal(0, store.ClaimCalls);
        Assert.Null(gateway.Lease);

        // Reported separately from gateway readiness: a healthy night must not read as a broken
        // telephony stack.
        Assert.True(result.DispatchGatewayReady);
        Assert.True(store.MaintenanceCalls > 0);
    }

    private static SchedulerCapacityRequest Request(
        string jobId,
        AttemptPolicySnapshot policy,
        DateTimeOffset startedAt,
        int riskScore) => new(
        jobId,
        policy.Program,
        startedAt,
        startedAt.Add(policy.ConfirmationWindowDuration),
        startedAt,
        policy.AttemptOffsets.Select(Value).ToArray(),
        riskScore);

    private static SchedulerQueueItem Item(
        string jobId,
        IvrProgramCode program,
        DateTimeOffset deadline,
        int dueOffsetSeconds,
        int riskScore,
        DateTimeOffset createdAt,
        int attemptNumber = 1) => new(
        jobId,
        program,
        deadline,
        createdAt.AddSeconds(dueOffsetSeconds),
        dueOffsetSeconds,
        attemptNumber,
        riskScore,
        createdAt);

    private static int Value(TimeSpan value) => checked((int)value.TotalSeconds);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecordingDispatchGateway : ISchedulerDispatchGateway
    {
        public bool IsReady => true;

        public SchedulerDispatchLease? Lease { get; private set; }

        public Task DispatchAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Lease = lease;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingSchedulerStore : IPostgresSchedulerStore
    {
        public SchedulerDispatchLease? Lease { get; init; }

        public int MaintenanceCalls { get; private set; }

        public int ClaimCalls { get; private set; }

        public string? LastExecutionMode { get; private set; }

        public Task<SchedulerDispatchLease?> TryClaimDueDispatchAsync(
            string workerId,
            string executionMode,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            ClaimCalls++;
            LastExecutionMode = executionMode;
            return Task.FromResult(Lease);
        }

        public Task<int> QuarantineExpiredLeasesAsync(
            DateTimeOffset detectedAt,
            TimeSpan quarantineDuration,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            MaintenanceCalls++;
            return Task.FromResult(0);
        }

        public Task<int> CloseMissedDeadlinesAsync(
            DateTimeOffset detectedAt,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            MaintenanceCalls++;
            return Task.FromResult(0);
        }
    }
}
