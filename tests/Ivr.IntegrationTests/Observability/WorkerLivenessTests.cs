using Ivr.Worker;

namespace Ivr.IntegrationTests.Observability;

/// <summary>
/// W-0043 §2. The worker had no health signal beyond "the process has not exited", and the failure
/// mode that matters does not exit: every job host catches its own exceptions and keeps polling, so
/// a loop failing forever — or hanging inside a call that never returns — looks exactly like a
/// healthy one from outside.
/// </summary>
public sealed class WorkerLivenessTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void ALoopThatStopsTickingIsReportedByName()
    {
        var clock = new MutableClock(Start);
        var liveness = new WorkerLiveness(clock);
        liveness.Register("scheduler", TimeSpan.FromSeconds(1));
        liveness.Register("callback-delivery", TimeSpan.FromSeconds(1));

        // Both turning.
        clock.Advance(TimeSpan.FromSeconds(20));
        liveness.Tick("scheduler");
        liveness.Tick("callback-delivery");
        Assert.Equal(WorkerLivenessStatus.Live, liveness.Read().Status);

        // One keeps turning; the other wedges. The process is alive throughout, which is the whole
        // problem this replaces.
        clock.Advance(TimeSpan.FromSeconds(45));
        liveness.Tick("scheduler");

        WorkerLivenessReport report = liveness.Read();
        Assert.Equal(WorkerLivenessStatus.Stalled, report.Status);
        Assert.False(report.Live);
        Assert.Equal(["callback-delivery"], report.StaleLoops);
    }

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void GraceIsThreeIntervalsButNeverLessThanThirtySeconds()
    {
        var clock = new MutableClock(Start);
        var liveness = new WorkerLiveness(clock);
        // A one-second poll: three intervals would be three seconds, which an ordinary GC pause
        // would trip. The floor is what stops the check from being noise on the fastest loop.
        liveness.Register("scheduler", TimeSpan.FromSeconds(1));
        // A slow loop gets three of its own intervals rather than the floor, because for this one
        // thirty seconds is a perfectly normal gap between passes.
        liveness.Register("analytics", TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.Equal(WorkerLivenessStatus.Live, liveness.Read().Status);

        clock.Advance(TimeSpan.FromSeconds(2));
        Assert.Equal(["scheduler"], liveness.Read().StaleLoops);

        clock.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal(["analytics", "scheduler"], liveness.Read().StaleLoops);
    }

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void FailingEveryPassIsReportedButIsNotALivenessFailure()
    {
        // The distinction the probe depends on. A loop failing because PostgreSQL is down is
        // turning correctly; restarting the pod does not repair PostgreSQL, it just adds a restart
        // storm to an outage. Only a loop that STOPPED is a restart signal.
        var clock = new MutableClock(Start);
        var liveness = new WorkerLiveness(clock);
        liveness.Register("normalization", TimeSpan.FromSeconds(1));

        for (int pass = 0; pass < 20; pass += 1)
        {
            clock.Advance(TimeSpan.FromSeconds(10));
            liveness.Fault("normalization", new InvalidOperationException("downstream is down"));
        }

        WorkerLivenessReport report = liveness.Read();
        Assert.Equal(WorkerLivenessStatus.Live, report.Status);
        WorkerLoopHealth loop = Assert.Single(report.Loops);
        Assert.Equal(20, loop.ConsecutiveFaults);

        // The exception TYPE, never its message: a message can carry a connection string or a row
        // value, and this report is readable by anything that can reach the probe.
        Assert.Equal(nameof(InvalidOperationException), loop.LastFaultKind);
        Assert.DoesNotContain("downstream", loop.LastFaultKind, StringComparison.OrdinalIgnoreCase);

        liveness.Tick("normalization");
        Assert.Equal(0, Assert.Single(liveness.Read().Loops).ConsecutiveFaults);
    }

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void AWorkerWithNoRegisteredLoopsIsStalled()
    {
        // An empty registry means no host reached its registration at all -- the wiring was removed,
        // or every host crashed before starting. That is a defect, so it is STALLED rather than
        // idle, and it fails the probe.
        WorkerLivenessReport report = new WorkerLiveness(new MutableClock(Start)).Read();
        Assert.Equal(WorkerLivenessStatus.Stalled, report.Status);
        Assert.False(report.Live);
    }

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void EveryLoopTurnedOffIsIdleAndStillPassesTheProbe()
    {
        // Found by running the endpoint rather than by reading the code: the shipped worker has
        // every loop disabled, so a two-state report answered 503 forever on a worker behaving
        // exactly as configured -- and a liveness probe would have restarted it in a loop.
        //
        // A restart cannot start a loop the configuration turned off, so idle must pass the probe.
        // The status field is what carries the difference to a human.
        var clock = new MutableClock(Start);
        var liveness = new WorkerLiveness(clock);
        liveness.RegisterDisabled("scheduler");
        liveness.RegisterDisabled("normalization");

        clock.Advance(TimeSpan.FromHours(9));

        WorkerLivenessReport report = liveness.Read();
        Assert.Equal(WorkerLivenessStatus.Idle, report.Status);
        Assert.True(report.Live);
        Assert.Empty(report.StaleLoops);
        Assert.All(report.Loops, loop => Assert.False(loop.Enabled));
    }

    [Fact]
    [Trait("TestId", "IT-WORKER-LIVENESS-12")]
    public void OneEnabledLoopAmongDisabledOnesIsStillWatched()
    {
        // The mixed case, which is the normal one: a deployment runs the scheduler and leaves
        // analytics off. Turning anything off must not buy the loops that stayed on an exemption.
        var clock = new MutableClock(Start);
        var liveness = new WorkerLiveness(clock);
        liveness.RegisterDisabled("analytics");
        liveness.Register("scheduler", TimeSpan.FromSeconds(1));

        clock.Advance(TimeSpan.FromSeconds(20));
        Assert.Equal(WorkerLivenessStatus.Live, liveness.Read().Status);

        clock.Advance(TimeSpan.FromSeconds(20));
        WorkerLivenessReport report = liveness.Read();
        Assert.Equal(WorkerLivenessStatus.Stalled, report.Status);
        Assert.Equal(["scheduler"], report.StaleLoops);
    }

    // The repo's own shape rather than Microsoft.Extensions.TimeProvider.Testing: a staleness check
    // has to be driven by a clock the test moves, and adding a package for four lines would put a
    // dependency in the lockfile that the package policy then has to justify.
    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset current = now;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan amount) => current = current.Add(amount);
    }
}
