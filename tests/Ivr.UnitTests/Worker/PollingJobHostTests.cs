using Ivr.Worker;
using Ivr.Worker.Jobs;

namespace Ivr.UnitTests.Worker;

/// <summary>
/// The loop four worker hosts share. It was four copies until <c>W-0263</c>, none of them under a
/// unit test, and they had already drifted twice — one used <c>do/while</c> where the others used
/// <c>while</c>, and only one passed a <see cref="TimeProvider"/> to its timer.
/// </summary>
public sealed class PollingJobHostTests
{
    [Fact]
    [Trait("TestId", "UT-WORKER-LOOP-01")]
    public async Task ADisabledLoopRegistersAsDisabledAndDoesNotRun()
    {
        var liveness = new WorkerLiveness(TimeProvider.System);
        using var host = new ProbeHost(liveness) { Enabled = false };

        await host.StartAsync(CancellationToken.None);

        // Awaited rather than assumed. A disabled loop returns before its first await, so
        // ExecuteTask is already complete here — but reading the counters without joining on it
        // would be a race the test would win most of the time and lose on a loaded runner.
        Assert.NotNull(host.ExecuteTask);
        await host.ExecuteTask!;
        await host.StopAsync(CancellationToken.None);

        Assert.Equal(0, host.Passes);
        Assert.Equal(1, host.DisabledNotifications);

        // Registered rather than absent, so the health report can tell a loop that was turned OFF
        // from one that was never wired.
        Assert.Contains("probe", liveness.Read().Loops.Select(loop => loop.Loop));
    }

    /// <summary>
    /// The scheduler's exception, and the one that would be dangerous to get wrong: its enable
    /// gate lives inside SchedulerRuntime, so its loop must keep turning when disabled or the
    /// recovery and deadline-closing work in the same pass stops with it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WORKER-LOOP-02")]
    public async Task ALoopThatKeepsTurningWhenDisabledStillRuns()
    {
        var liveness = new WorkerLiveness(TimeProvider.System);
        using var host = new ProbeHost(liveness) { Enabled = false, KeepTurningWhenDisabled = true };

        await host.StartAsync(CancellationToken.None);
        await host.FirstPass.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        Assert.True(host.Passes > 0);
        Assert.Equal(1, host.DisabledNotifications);
    }

    [Fact]
    [Trait("TestId", "UT-WORKER-LOOP-03")]
    public async Task AFailingPassIsReportedWithItsStreakAndTheLoopKeepsGoing()
    {
        var liveness = new WorkerLiveness(TimeProvider.System);
        using var host = new ProbeHost(liveness) { Enabled = true, ThrowUntilPass = 3 };

        await host.StartAsync(CancellationToken.None);
        await host.ThirdPass.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        // Reported once per failed pass, with a streak that grows rather than repeating 1 — a
        // blip and a two-hour outage printing the same line is what the count is there to stop.
        Assert.Equal([1, 2], host.ReportedStreaks.Take(2));

        // And it did not stop at the failures: the loop survived them and ran again.
        Assert.True(host.Passes >= 3);
    }

    [Fact]
    [Trait("TestId", "UT-WORKER-LOOP-04")]
    public async Task ShutdownEndsTheLoopWithoutReportingAFailure()
    {
        var liveness = new WorkerLiveness(TimeProvider.System);
        using var host = new ProbeHost(liveness) { Enabled = true };

        await host.StartAsync(CancellationToken.None);
        await host.FirstPass.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        // A cancelled pass is a stop, not an error. Reporting it would put a fault on the health
        // record of every worker that ever shut down cleanly.
        Assert.Empty(host.ReportedStreaks);
    }

    private sealed class ProbeHost(WorkerLiveness liveness)
        : PollingJobHost(liveness, TimeProvider.System)
    {
        public bool Enabled { get; init; } = true;

        public bool KeepTurningWhenDisabled { get; init; }

        public int ThrowUntilPass { get; init; }

        public int Passes;

        public int DisabledNotifications;

        public List<int> ReportedStreaks { get; } = [];

        public TaskCompletionSource FirstPass { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource ThirdPass { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override string LoopName => "probe";

        protected override bool IsEnabled => Enabled;

        // Short enough that the test does not wait on a clock, long enough that a slow CI runner
        // does not spin thousands of passes before StopAsync lands.
        protected override TimeSpan Period => TimeSpan.FromMilliseconds(20);

        protected override bool StopWhenDisabled => !KeepTurningWhenDisabled;

        protected override Task RunOnceAsync(CancellationToken cancellationToken)
        {
            int pass = Interlocked.Increment(ref Passes);
            if (pass == 1)
            {
                FirstPass.TrySetResult();
            }

            if (pass >= 3)
            {
                ThirdPass.TrySetResult();
            }

            if (pass < ThrowUntilPass)
            {
                throw new InvalidOperationException($"probe failure on pass {pass}");
            }

            return Task.CompletedTask;
        }

        protected override void OnDisabled() => Interlocked.Increment(ref DisabledNotifications);

        protected override void OnFailure(Exception exception, int consecutiveFailures)
        {
            lock (ReportedStreaks)
            {
                ReportedStreaks.Add(consecutiveFailures);
            }
        }
    }
}
