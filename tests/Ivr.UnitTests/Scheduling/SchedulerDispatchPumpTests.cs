using System.Collections.Concurrent;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// SIP-05. What one worker may hold and how fast it may start, from
/// <c>plan/ivr-orther/mobile-sip-trunk-production-32-channels-plan-2026-09-15.md</c>.
/// <para>
/// These prove the software side of T06 and nothing beyond it. Every call here is a fake that
/// blocks until the test lets it go: no Asterisk, no trunk, no carrier. A worker that holds 32
/// of these is a worker whose orchestration can hold 32, which is the claim T06 makes; whether
/// 32 calls actually reach customers is T07, and only a carrier can answer it.
/// </para>
/// </summary>
public sealed class SchedulerDispatchPumpTests
{
    private static readonly DateTimeOffset T0 =
        new(2026, 8, 13, 8, 0, 0, TimeSpan.Zero);

    private static readonly CallingWindow AlwaysOpenWindow =
        new(Options.Create(new CallingWindowOptions { Enabled = false }));

    /// <summary>
    /// Thirty-two held at once, and the thirty-third not merely refused but never claimed.
    /// <para>
    /// The second half is the one that matters operationally. Claiming a lease the worker cannot
    /// run would leave an <c>ivr_sim_channels</c> row RESERVED and its fencing generation spent,
    /// and nothing would put it back for RecoveryQuarantineSeconds - ten minutes - on a job that
    /// was due the instant it was taken. So the assertion is on the claim count, not on how many
    /// dispatches were refused afterwards.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-01")]
    public async Task ThirtyTwoCallsAreHeldAtOnceAndTheThirtyThirdIsNeverClaimed()
    {
        var store = new QueueSchedulerStore { Available = 40 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 32, startsPerSecond: 32);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-32");

        Assert.Equal(32, first.DispatchesStarted);
        Assert.Equal(32, first.ActiveDispatches);
        Assert.Equal(32, gateway.Started);

        // Thirty-two, not thirty-three: the reservation for the next one is refused before any
        // claim is attempted, so the database is never asked for work this worker cannot run.
        Assert.Equal(32, store.ClaimCalls);

        // A fresh rate window, so that the pass below starting nothing can only mean the ceiling
        // is full. Without this the rate limit would refuse first and the test would pass while
        // proving the wrong thing.
        harness.OpenANewRateWindow();
        SchedulerRunResult second = await harness.Runtime.RunOnceAsync("worker-32");

        Assert.Equal(0, second.DispatchesStarted);
        Assert.Equal(32, second.ActiveDispatches);
        Assert.Equal(32, store.ClaimCalls);

        // One call ends; exactly one slot opens, and the queued work moves by exactly one.
        Assert.True(gateway.ReleaseOne());
        await WaitUntilAsync(() => harness.Pump.Active == 31, "the finished call released its slot");

        harness.OpenANewRateWindow();
        SchedulerRunResult third = await harness.Runtime.RunOnceAsync("worker-32");

        Assert.Equal(1, third.DispatchesStarted);
        Assert.Equal(32, third.ActiveDispatches);
        Assert.Equal(33, store.ClaimCalls);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// A full worker still recovers leases and closes missed deadlines, and its pass still returns.
    /// <para>
    /// This is the defect SIP-05 actually fixes, and it is not about capacity. A pass used to end
    /// by awaiting its own call, so a pass was as long as a call - up to audio 120s plus ring 30s
    /// plus DTMF 15s. <c>PollingJobHost</c> ticks <c>WorkerLiveness</c> only when a pass returns,
    /// and <c>WorkerLiveness</c> calls a loop stale after max(3 x poll, 30s), which is 30s at the
    /// default one-second poll. Every ordinary long call therefore reported a wedged scheduler,
    /// and the overnight bookkeeping stopped for the duration of each one.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-02")]
    public async Task MaintenanceKeepsRunningAndThePassKeepsReturningWhileEveryCallIsHeld()
    {
        var store = new QueueSchedulerStore { Available = 8 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 8, startsPerSecond: 8);

        await harness.Runtime.RunOnceAsync("worker-busy");
        Assert.Equal(8, harness.Pump.Active);

        int maintenanceBefore = store.MaintenanceCalls;

        // Three more passes with every slot occupied. Each returns - that is the liveness tick -
        // and each does its recovery work.
        for (int pass = 0; pass < 3; pass++)
        {
            SchedulerRunResult result = await harness.Runtime.RunOnceAsync("worker-busy");
            Assert.Equal(0, result.DispatchesStarted);
            Assert.Equal(8, result.ActiveDispatches);
        }

        // Two maintenance calls per pass: quarantine expired leases, then close missed deadlines.
        Assert.Equal(maintenanceBefore + 6, store.MaintenanceCalls);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// Free slots are not permission to start: the rate limit is a second, separate allowance.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-03")]
    public async Task FreeSlotsDoNotLicenseABurstBeyondTheStartRate()
    {
        var store = new QueueSchedulerStore { Available = 40 };
        var gateway = new BlockingDispatchGateway();
        var clock = new MovableTimeProvider(T0);
        Harness harness = Harness.Create(
            store,
            gateway,
            ceiling: 32,
            startsPerSecond: 4,
            clock: clock);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-cps");

        // Twenty-eight slots are free and the queue has plenty of work. The rate says four.
        Assert.Equal(4, first.DispatchesStarted);
        Assert.Equal(4, store.ClaimCalls);

        SchedulerRunResult sameSecond = await harness.Runtime.RunOnceAsync("worker-cps");

        Assert.Equal(0, sameSecond.DispatchesStarted);
        Assert.Equal(4, store.ClaimCalls);

        clock.Advance(TimeSpan.FromSeconds(1));
        SchedulerRunResult nextSecond = await harness.Runtime.RunOnceAsync("worker-cps");

        Assert.Equal(4, nextSecond.DispatchesStarted);
        Assert.Equal(8, nextSecond.ActiveDispatches);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// Lowering the ceiling under load stops new calls and does not touch the calls already up.
    /// <para>
    /// The plan asks for exactly this shape - drop 32 to 8 and admissions stop until the active
    /// count has drained - and it is why the ceiling is a counter under a lock rather than a
    /// <c>SemaphoreSlim</c>, which cannot shrink without deciding which live call to drop.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-04")]
    public async Task LoweringTheCeilingStopsNewCallsWithoutDroppingLiveOnes()
    {
        var store = new QueueSchedulerStore { Available = 40 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 8, startsPerSecond: 8);

        await harness.Runtime.RunOnceAsync("worker-shrink");
        Assert.Equal(8, harness.Pump.Active);

        harness.Configured.MaxConcurrentDispatches = 2;

        // Rate window reopened so the refusals below are unambiguously the ceiling and not the
        // rate. Nothing has failed here, so shedding is not in play either.
        harness.OpenANewRateWindow();
        SchedulerRunResult afterShrink = await harness.Runtime.RunOnceAsync("worker-shrink");

        Assert.Equal(0, afterShrink.DispatchesStarted);
        Assert.Equal(8, afterShrink.ActiveDispatches);
        Assert.Equal(8, gateway.Started);

        // Down to two before a single new call is admitted, and not one of the eight was cut off
        // to get there.
        for (int released = 0; released < 6; released++)
        {
            Assert.True(gateway.ReleaseOne());
        }

        await WaitUntilAsync(() => harness.Pump.Active == 2, "six finished calls released six slots");

        harness.OpenANewRateWindow();
        SchedulerRunResult atCeiling = await harness.Runtime.RunOnceAsync("worker-shrink");
        Assert.Equal(0, atCeiling.DispatchesStarted);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// A call that throws after its pass has returned is reported once, and its slot comes back.
    /// <para>
    /// Before SIP-05 this exception left through <c>RunOnceAsync</c> and <c>PollingJobHost</c>
    /// logged it. A call that outlives its pass has no stack to leave through, so the alternative
    /// to carrying it is an unobserved <see cref="Task"/> exception - a failure nobody sees and a
    /// slot nobody gets back.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-05")]
    public async Task AFailedCallIsReportedOnceAndItsSlotIsReturned()
    {
        var store = new QueueSchedulerStore { Available = 2 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 2, startsPerSecond: 2);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-fail");

        // The pass itself succeeds. A call failing is not the pass failing, and treating it as one
        // would fault the loop over a single bad number.
        Assert.Equal(2, first.DispatchesStarted);
        Assert.Empty(first.DispatchFailures ?? []);

        // Both fail once connected, rather than being refused up front, so that both are started
        // before either failure lands. A call that fails before the pass moves on would shed the
        // rest of that pass, which UT-SCH-PUMP-08 covers and this test is not about.
        var refused = new InvalidOperationException("ARI refused the originate.");
        Assert.True(gateway.ReleaseOne(refused));
        Assert.True(gateway.ReleaseOne(refused));

        await WaitUntilAsync(() => harness.Pump.Active == 0, "both failed calls released their slots");

        SchedulerRunResult second = await harness.Runtime.RunOnceAsync("worker-fail");

        Assert.Equal(2, (second.DispatchFailures ?? []).Count);
        Assert.All(
            second.DispatchFailures ?? [],
            failure => Assert.IsType<InvalidOperationException>(failure.Exception));

        // Taken, so the pass after it is clean. A failure reported on every pass until restart
        // would be indistinguishable from a failure happening on every pass.
        SchedulerRunResult third = await harness.Runtime.RunOnceAsync("worker-fail");
        Assert.Empty(third.DispatchFailures ?? []);
    }

    /// <summary>
    /// A claim that throws gives its reservation back.
    /// <para>
    /// Without the <c>finally</c> that does this, every database blip would shrink the worker
    /// ceiling by one, permanently and invisibly, until a restart. A worker configured for 32
    /// would quietly become a worker that dials nothing.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-06")]
    public async Task AClaimThatThrowsDoesNotStrandTheReservation()
    {
        var store = new QueueSchedulerStore
        {
            Available = 4,
            ClaimThrows = () => new TimeoutException("Npgsql connection timed out."),
        };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 4, startsPerSecond: 4);

        await Assert.ThrowsAsync<TimeoutException>(
            () => harness.Runtime.RunOnceAsync("worker-blip"));

        Assert.Equal(0, harness.Pump.Active);

        // And the worker is whole afterwards: all four slots are still there to be used. A new
        // rate window first, because the refused claim did spend a start-rate token and does not
        // get it back - deliberately, since refunding one into the wrong window would hand out
        // more starts per second than the carrier allows. That is a separate property from the
        // slot coming back, which is what this test is about.
        store.ClaimThrows = null;
        harness.OpenANewRateWindow();
        SchedulerRunResult recovered = await harness.Runtime.RunOnceAsync("worker-blip");

        Assert.Equal(4, recovered.DispatchesStarted);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// Drain reports the truth rather than a convenient one: false while a call is still up.
    /// <para>
    /// It is what shutdown asks before the process ends. A drain that returned true with calls
    /// connected would turn every deploy into a set of attempts nobody knows the outcome of.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-07")]
    public async Task DrainReportsFalseWhileACallIsStillRunning()
    {
        var store = new QueueSchedulerStore { Available = 1 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 1, startsPerSecond: 1);

        await harness.Runtime.RunOnceAsync("worker-drain");
        Assert.Equal(1, harness.Pump.Active);

        Assert.False(await harness.Pump.DrainAsync(TimeSpan.FromMilliseconds(200)));

        gateway.ReleaseAll();

        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal(0, harness.Pump.Active);
    }

    /// <summary>
    /// One failed call stops the rest of that pass, and the recovery work carries on regardless.
    /// <para>
    /// This is the property that replaces what the concurrency change took away. A failing
    /// dispatch used to throw out of the pass into <c>PollingJobHost</c>, whose <c>LoopBackoff</c>
    /// slowed the loop; calls that outlive their pass have no such route. Without shedding, a
    /// trunk refusing every originate would be dialled at the full claim rate for as long as it
    /// stayed broken.
    /// </para>
    /// <para>
    /// Stopping after the first failure rather than the eighth is the point: eight free slots and
    /// eight jobs due is not a reason to throw seven more customers at a route that just refused
    /// one.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-08")]
    public async Task AFailedCallShedsTheRestOfThePassWithoutStoppingRecovery()
    {
        var store = new QueueSchedulerStore { Available = 8 };
        var gateway = new BlockingDispatchGateway
        {
            FailWith = () => new InvalidOperationException("Trunk rejected the originate."),
        };
        Harness harness = Harness.Create(store, gateway, ceiling: 8, startsPerSecond: 8);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-shed");

        // One, not eight. Seven slots and seven due jobs were available the whole time.
        Assert.Equal(1, first.DispatchesStarted);
        Assert.Equal(1, store.ClaimCalls);
        Assert.NotNull(first.DispatchSheddingUntil);

        // No clock movement here. One failure sheds for about one poll interval, so advancing to
        // open a fresh rate window would also expire the shed and this pass would dial again -
        // which is UT-SCH-PUMP-10, not this test. Seven of the eight rate tokens are unused
        // anyway, so the rate cannot be what refuses below.
        int maintenanceBefore = store.MaintenanceCalls;
        SchedulerRunResult second = await harness.Runtime.RunOnceAsync("worker-shed");

        Assert.Equal(0, second.DispatchesStarted);
        Assert.Equal(1, store.ClaimCalls);
        Assert.NotNull(second.DispatchSheddingUntil);

        // Shedding stops dialling and nothing else. Lease recovery and deadline closing still run,
        // and the pass still returns - which is what ticks liveness.
        Assert.Equal(maintenanceBefore + 2, store.MaintenanceCalls);

        // Reported once, on the pass after the one that started the call.
        Assert.Single(second.DispatchFailures ?? []);
    }

    /// <summary>
    /// A single call that goes through lifts the shed, even with failures all around it.
    /// <para>
    /// The distinction the plan draws is between a route that is down and numbers that are bad,
    /// and the pump cannot read a SIP cause code. What it can see is whether anything at all is
    /// getting through: one completed call is stronger evidence the trunk is up than several
    /// failures are that it is down. Matches <c>LoopBackoff.RecordSuccess</c>, which also clears
    /// the streak outright rather than decaying it.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-09")]
    public async Task OneCallGettingThroughLiftsTheShed()
    {
        var store = new QueueSchedulerStore { Available = 40 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(store, gateway, ceiling: 8, startsPerSecond: 8);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-mixed");

        // All eight get started, because none of them has failed yet.
        Assert.Equal(8, first.DispatchesStarted);
        Assert.Null(first.DispatchSheddingUntil);

        var refused = new InvalidOperationException("Trunk rejected the originate.");
        for (int failed = 0; failed < 7; failed++)
        {
            Assert.True(gateway.ReleaseOne(refused));
        }

        await WaitUntilAsync(
            () => harness.Pump.SheddingUntil is not null,
            "seven consecutive failures put the pump into shedding");

        // Seven failures on the trot reach the backoff ceiling, so this is a long shed - it is not
        // about to expire on its own during the next two lines.
        Assert.True(gateway.ReleaseOne());

        await WaitUntilAsync(
            () => harness.Pump.SheddingUntil is null,
            "the one call that went through cleared the streak");

        harness.OpenANewRateWindow();
        SchedulerRunResult resumed = await harness.Runtime.RunOnceAsync("worker-mixed");

        Assert.Equal(8, resumed.DispatchesStarted);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// The shed expires on its own. Backing off is not giving up: nobody has to restart anything.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-PUMP-10")]
    public async Task TheShedExpiresAndDiallingResumesWithoutIntervention()
    {
        var store = new QueueSchedulerStore { Available = 8 };
        var gateway = new BlockingDispatchGateway
        {
            FailWith = () => new InvalidOperationException("Trunk rejected the originate."),
        };
        Harness harness = Harness.Create(store, gateway, ceiling: 8, startsPerSecond: 8);

        SchedulerRunResult first = await harness.Runtime.RunOnceAsync("worker-expire");
        Assert.NotNull(first.DispatchSheddingUntil);

        // One failure means a streak of one, so the wait is one poll interval before jitter, and
        // jitter only ever shortens it. Two seconds clears it with room to spare - asserted off
        // the schedule LoopBackoff.Compute defines, not off a slept-through guess.
        Assert.True(
            first.DispatchSheddingUntil <= harness.Clock.GetUtcNow().AddSeconds(2),
            "a single failure must not shed for longer than one poll interval");

        harness.Clock.Advance(TimeSpan.FromSeconds(2));
        gateway.FailWith = null;

        SchedulerRunResult resumed = await harness.Runtime.RunOnceAsync("worker-expire");

        Assert.Null(resumed.DispatchSheddingUntil);

        // Seven, not one: the queue held eight and the call that failed consumed one. The shed
        // delayed this work, it did not discard it.
        Assert.Equal(7, resumed.DispatchesStarted);

        gateway.ReleaseAll();
        Assert.True(await harness.Pump.DrainAsync(TimeSpan.FromSeconds(10)));
    }

    /// <summary>
    /// A worker that does not hold the Asterisk application claims nothing, and keeps recovering.
    /// <para>
    /// The ownership state machine is proved against a real database in
    /// <c>AriControllerOwnershipTests</c>; what is proved here is that the scheduler asks it and
    /// obeys the answer. Those are different failures - a correct state machine nobody consults is
    /// exactly as good as no state machine - and the second one is invisible in a single-worker
    /// deployment right up until the day there are two.
    /// </para>
    /// <para>
    /// Both statuses that mean "not yet" are covered, because they arrive by different routes: one
    /// is a rival still holding the socket, the other is this worker having seized it and not yet
    /// accounted for the calls of the generation before.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData(AriControllerStatus.HeldByAnotherWorker)]
    [InlineData(AriControllerStatus.AwaitingIsolation)]
    [InlineData(AriControllerStatus.AwaitingReconciliation)]
    [Trait("TestId", "UT-SCH-CTRL-07")]
    public async Task AWorkerThatDoesNotHoldTheApplicationClaimsNothing(
        AriControllerStatus status)
    {
        var store = new QueueSchedulerStore { Available = 8 };
        var gateway = new BlockingDispatchGateway();
        Harness harness = Harness.Create(
            store,
            gateway,
            ceiling: 8,
            startsPerSecond: 8,
            ownership: new StubAriControllerOwnership(status));

        SchedulerRunResult result = await harness.Runtime.RunOnceAsync("worker-nonowner");

        Assert.Equal(0, result.DispatchesStarted);
        Assert.Equal(status, result.ControllerStatus);

        // Never asked. A worker that may not dial must not be taking leases out of the queue and
        // holding them where the owner cannot see them.
        Assert.Equal(0, store.ClaimCalls);
        Assert.Equal(0, gateway.Started);

        // The bookkeeping is nobody's exclusive right, so it carries on. A worker waiting for an
        // isolation may be waiting a long time, and lease recovery and deadline closing must not
        // wait with it.
        Assert.Equal(2, store.MaintenanceCalls);
    }

    /// <summary>
    /// Waits for an asynchronous continuation to be observable, rather than assuming it already
    /// is. A slot is released in the <c>finally</c> of the dispatch task, which runs after the
    /// test signals the call to finish.
    /// </summary>
    private static async Task WaitUntilAsync(Func<bool> condition, string because)
    {
        for (int attempt = 0; attempt < 500 && !condition(); attempt++)
        {
            await Task.Delay(10);
        }

        Assert.True(condition(), because);
    }

    private sealed record Harness(
        SchedulerRuntime Runtime,
        SchedulerDispatchPump Pump,
        SchedulerOptions Configured,
        MovableTimeProvider Clock)
    {
        /// <summary>
        /// Moves past the current start-rate window, so that a pass which starts nothing can only
        /// be the concurrency ceiling refusing and never the rate. Tests that mean to assert the
        /// ceiling have to say this, or they assert whichever limit happened to fire first.
        /// </summary>
        public void OpenANewRateWindow() => Clock.Advance(TimeSpan.FromSeconds(1));

        public static Harness Create(
            IPostgresSchedulerStore store,
            ISchedulerDispatchGateway gateway,
            int ceiling,
            int startsPerSecond,
            MovableTimeProvider? clock = null,
            IAriControllerOwnership? ownership = null)
        {
            MovableTimeProvider timeProvider = clock ?? new MovableTimeProvider(T0);
            var options = new SchedulerOptions
            {
                Enabled = true,
                MaxConcurrentDispatches = ceiling,
                MaxCallStartsPerSecond = startsPerSecond,
            };
            IOptions<SchedulerOptions> wrapped = Options.Create(options);
            var pump = new SchedulerDispatchPump(wrapped, timeProvider);
            return new Harness(
                new SchedulerRuntime(
                    store,
                    gateway,
                    pump,
                    ownership ?? new UncontendedAriControllerOwnership("unit-test-scope"),
                    wrapped,
                    new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
                    AlwaysOpenWindow,
                    timeProvider),
                pump,
                options,
                timeProvider);
        }
    }

    /// <summary>
    /// A call that lasts exactly as long as the test wants it to. Every dispatch parks on its own
    /// gate, so the test can end one call without ending the others - which is what makes "one
    /// slot opened, one job moved" an assertion rather than a hope.
    /// </summary>
    private sealed class BlockingDispatchGateway : ISchedulerDispatchGateway
    {
        private readonly ConcurrentQueue<TaskCompletionSource> gates = new();
        private int started;

        public bool IsReady => true;

        /// <summary>
        /// Fails before parking, the shape of an originate the adapter rejects outright. Settable
        /// mid-test so a route can be made to break and then recover.
        /// </summary>
        public Func<Exception>? FailWith { get; set; }

        public int Started => Volatile.Read(ref started);

        public async Task DispatchAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(lease);
            Interlocked.Increment(ref started);
            if (FailWith is not null)
            {
                throw FailWith();
            }

            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            gates.Enqueue(gate);
            await gate.Task.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Ends the longest-running call, as a success or with <paramref name="error"/>. Per-call
        /// so a test can have seven of eight fail and the eighth come good, which is the mix that
        /// separates a broken route from a handful of bad numbers.
        /// </summary>
        public bool ReleaseOne(Exception? error = null)
        {
            if (!gates.TryDequeue(out TaskCompletionSource? gate))
            {
                return false;
            }

            return error is null ? gate.TrySetResult() : gate.TrySetException(error);
        }

        public void ReleaseAll()
        {
            while (ReleaseOne())
            {
            }
        }
    }

    private sealed class QueueSchedulerStore : IPostgresSchedulerStore
    {
        private int issued;

        /// <summary>How many leases the queue and the channel pool between them can supply.</summary>
        public int Available { get; init; }

        public Func<Exception>? ClaimThrows { get; set; }

        public int ClaimCalls { get; private set; }

        public int MaintenanceCalls { get; private set; }

        public Task<SchedulerDispatchLease?> TryClaimDueDispatchAsync(
            string workerId,
            string executionMode,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            ClaimCalls++;
            if (ClaimThrows is not null)
            {
                throw ClaimThrows();
            }

            if (issued >= Available)
            {
                return Task.FromResult<SchedulerDispatchLease?>(null);
            }

            issued++;
            return Task.FromResult<SchedulerDispatchLease?>(new SchedulerDispatchLease(
                $"JOB-{issued}",
                $"ATTEMPT-{issued}",
                1,
                T0,
                T0.AddMinutes(5),
                $"CHANNEL-{issued}",
                Guid.NewGuid(),
                7,
                T0.AddMinutes(2),
                "MOCK",
                "MOCK"));
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

    /// <summary>
    /// Answers with one status, forever. The state machine that produces these for real is proved
    /// against PostgreSQL in <c>AriControllerOwnershipTests</c>; here the question is only whether
    /// the scheduler does as it is told.
    /// </summary>
    private sealed class StubAriControllerOwnership(AriControllerStatus status)
        : IAriControllerOwnership
    {
        public string Scope => "stub-scope";

        public Task<AriControllerGrant> AcquireOrRenewAsync(
            string workerId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AriControllerGrant(status, 7, workerId, null));

        public Task ReleaseAsync(
            string workerId,
            string reason,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class MovableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan by) => now = now.Add(by);
    }
}
