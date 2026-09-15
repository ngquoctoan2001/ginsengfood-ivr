using System.Collections.Concurrent;
using System.Text.Json;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

/// <summary>
/// SIP-05. The ceiling that actually binds across workers, proved against PostgreSQL.
/// <para>
/// <c>SchedulerDispatchPumpTests</c> proves the per-process ceiling with an in-memory store, and
/// says in its own comments that this is the weaker claim. The bound that holds when there are two
/// workers is the channel pool in <c>ivr_sim_channels</c>, taken by
/// <see cref="PostgresSchedulerStore.TryClaimDueDispatchAsync"/> under
/// <c>FOR UPDATE ... SKIP LOCKED</c> - and a bound nobody tested against the real query is a
/// bound that exists in a comment.
/// </para>
/// <para>
/// Note what is deliberately NOT proved here: that any of this reaches a customer. The dispatch
/// gateway is a fake that parks. These tests answer "can two workers over-commit the pool", which
/// is T06; whether 32 calls reach three mobile networks is T07 and needs a carrier.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class SchedulerSharedPoolTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A worker configured for more calls than the pool holds takes the pool, and no more.
    /// <para>
    /// This is the shape a misconfiguration takes in production: somebody sets the ceiling to the
    /// figure the carrier quoted before the channels for it exist. The answer has to be "eight
    /// calls", not "eight calls and twenty-four leases nobody can run".
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-POOL-01")]
    public async Task AWorkerCannotClaimMoreCallsThanThePoolHolds()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedJobsAsync(factory, count: 20);
        await SeedChannelsAsync(factory, count: 8);

        var gateway = new ParkingDispatchGateway();
        Worker worker = NewWorker(gateway, ceiling: 32, startsPerSecond: 32);

        SchedulerRunResult result = await worker.Runtime.RunOnceAsync(worker.Id);

        // Eight, from a ceiling of 32 and a queue of 20. The pool is what decides.
        Assert.Equal(8, result.DispatchesStarted);
        Assert.Equal(8, gateway.Started);

        await AssertPoolIntegrityAsync(factory, expectedReserved: 8);

        gateway.ReleaseAll();
        Assert.True(await worker.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// Two workers sharing one pool never hold more than the pool between them.
    /// <para>
    /// The plan asks for exactly this, and it is the half of T06 an in-memory ceiling cannot
    /// reach: each worker is separately configured for the whole pool, so anything short of the
    /// database arbitrating would let them take eight each.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-POOL-02")]
    public async Task TwoWorkersRacingForOnePoolNeverExceedItBetweenThem()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedJobsAsync(factory, count: 20);
        await SeedChannelsAsync(factory, count: 8);

        var gatewayA = new ParkingDispatchGateway();
        var gatewayB = new ParkingDispatchGateway();
        Worker workerA = NewWorker(gatewayA, ceiling: 8, startsPerSecond: 8);
        Worker workerB = NewWorker(gatewayB, ceiling: 8, startsPerSecond: 8);

        // Started together on purpose. Run one after the other and the first would empty the pool
        // before the second looked, which proves nothing about what they do when they collide.
        SchedulerRunResult[] results = await Task.WhenAll(
            workerA.Runtime.RunOnceAsync(workerA.Id),
            workerB.Runtime.RunOnceAsync(workerB.Id));

        int startedTogether = results.Sum(result => result.DispatchesStarted);

        Assert.Equal(8, startedTogether);
        Assert.Equal(8, gatewayA.Started + gatewayB.Started);

        // How the eight split between the two is not asserted, and deliberately not: 5/3, 8/0 and
        // 4/4 are all correct outcomes of a race, and pinning one would be pinning the scheduling
        // of the test host rather than a property of the system. The total is the property, and
        // neither worker exceeding its own ceiling follows from the total being the pool size.
        Assert.All(results, result => Assert.True(result.DispatchesStarted <= 8));

        await AssertPoolIntegrityAsync(factory, expectedReserved: 8);

        // And no job was handed to both. The claim excludes jobs with an active attempt, which is
        // the invariant that stops one customer being dialled twice at once.
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        List<string> activeJobs = await context.CallAttempts
            .Where(attempt => attempt.Status == "LEASED_PENDING_DISPATCH"
                || attempt.Status == "DIALING"
                || attempt.Status == "ACTIVE_CALL")
            .Select(attempt => attempt.IvrCallJobId)
            .ToListAsync();

        Assert.Equal(8, activeJobs.Count);
        Assert.Equal(activeJobs.Count, activeJobs.Distinct(StringComparer.Ordinal).Count());

        gatewayA.ReleaseAll();
        gatewayB.ReleaseAll();
        Assert.True(await workerA.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
        Assert.True(await workerB.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// An empty pool is a wait, not an error and not a dropped job.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-POOL-03")]
    public async Task WorkQueuedAgainstAnExhaustedPoolWaitsRatherThanFailing()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedJobsAsync(factory, count: 4);
        await SeedChannelsAsync(factory, count: 2);

        var gateway = new ParkingDispatchGateway();
        Worker worker = NewWorker(gateway, ceiling: 8, startsPerSecond: 8);

        SchedulerRunResult first = await worker.Runtime.RunOnceAsync(worker.Id);
        Assert.Equal(2, first.DispatchesStarted);

        // Pool empty, ceiling not reached, work still queued. Nothing fails and nothing is lost.
        SchedulerRunResult second = await worker.Runtime.RunOnceAsync(worker.Id);

        Assert.Equal(0, second.DispatchesStarted);
        Assert.True(second.DispatchGatewayReady);
        Assert.Empty(second.DispatchFailures ?? []);
        Assert.Null(second.DispatchSheddingUntil);

        await using IvrDbContext context = await factory.CreateDbContextAsync();
        Assert.Equal(2, await context.CallJobs.CountAsync(job => job.QueueStatus == "QUEUED"));

        gateway.ReleaseAll();
        Assert.True(await worker.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// Every reserved channel carries a lease token and a generation, and no two share a job.
    /// </summary>
    private static async Task AssertPoolIntegrityAsync(
        IDbContextFactory<IvrDbContext> factory,
        int expectedReserved)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        List<SimChannelEntity> reserved = await context.SimChannels
            .Where(channel => channel.Status == "RESERVED")
            .ToListAsync();

        Assert.Equal(expectedReserved, reserved.Count);
        Assert.All(reserved, channel => Assert.NotNull(channel.LeaseToken));
        Assert.All(reserved, channel => Assert.True(channel.LeaseFencingGeneration > 0));

        string[] jobs = [.. reserved.Select(channel => channel.ActiveCallJobId!)];
        Assert.Equal(jobs.Length, jobs.Distinct(StringComparer.Ordinal).Count());
    }

    private Worker NewWorker(
        ISchedulerDispatchGateway gateway,
        int ceiling,
        int startsPerSecond)
    {
        var clock = new FixedClock(Now);
        IOptions<SchedulerOptions> options = Options.Create(new SchedulerOptions
        {
            Enabled = true,
            MaxConcurrentDispatches = ceiling,
            MaxCallStartsPerSecond = startsPerSecond,
        });
        var pump = new SchedulerDispatchPump(options, clock);
        var store = new PostgresSchedulerStore(Factory(), clock);
        return new Worker(
            string.Concat("worker-", Guid.NewGuid().ToString("N")[..8]),
            new SchedulerRuntime(
                store,
                gateway,
                pump,
                new UncontendedAriControllerOwnership("shared-pool-test"),
                options,
                new SchedulerExecutionContext(IvrOptions.LabRealSimExecutionMode),
                new CallingWindow(Options.Create(new CallingWindowOptions { Enabled = false })),
                clock),
            pump);
    }

    private static async Task SeedChannelsAsync(IDbContextFactory<IvrDbContext> factory, int count)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        for (int index = 1; index <= count; index++)
        {
            string channelId = $"SIM-POOL-{index:D3}";
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

    private static async Task SeedJobsAsync(IDbContextFactory<IvrDbContext> factory, int count)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        DateTimeOffset deadline = Now.AddMinutes(5);
        for (int index = 1; index <= count; index++)
        {
            string taskId = $"TASK-POOL-{index:D3}";
            string jobId = $"JOB-POOL-{index:D3}";
            context.ConfirmationTasks.Add(new ConfirmationTaskEntity
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                ContractVersion = "ivr-order-confirmation.v1",
                IdempotencyKey = string.Concat("pool:", taskId),
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
                ConfirmationWindowStartedAt = Now,
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
                CreatedAt = Now,
                ExpiresAt = deadline,
                AcceptedAt = Now,
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
                    Now,
                    Now.AddSeconds(150),
                }),
                T0At = Now,
                ExpiresAt = deadline,
                Eligible = true,
                EligibilityDecision = "ELIGIBLE_FOR_IVR",
                QueueStatus = "QUEUED",
                ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
                PrivacyPolicyVersion = "privacy-v1",
                CreatedAt = Now,
            });
        }

        await context.SaveChangesAsync();
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private sealed record Worker(string Id, SchedulerRuntime Runtime, SchedulerDispatchPump Pump);

    /// <summary>
    /// Holds every call until the test lets it go, so a claimed channel stays claimed for as long
    /// as the assertions need to look at it.
    /// </summary>
    private sealed class ParkingDispatchGateway : ISchedulerDispatchGateway
    {
        private readonly ConcurrentQueue<TaskCompletionSource> gates = new();
        private int started;

        public bool IsReady => true;

        public int Started => Volatile.Read(ref started);

        public async Task DispatchAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lease);
            Interlocked.Increment(ref started);
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            gates.Enqueue(gate);
            await gate.Task.WaitAsync(cancellationToken);
        }

        public void ReleaseAll()
        {
            while (gates.TryDequeue(out TaskCompletionSource? gate))
            {
                gate.TrySetResult();
            }
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
