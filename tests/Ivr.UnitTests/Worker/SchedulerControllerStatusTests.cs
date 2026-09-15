using Ivr.Infrastructure.Telephony;
using Ivr.Worker;

namespace Ivr.UnitTests.Worker;

/// <summary>
/// SIP-05. What an operator can see about the ARI controller, and what it must not do to a probe.
/// </summary>
public sealed class SchedulerControllerStatusTests
{
    private static readonly DateTimeOffset T0 =
        new(2026, 9, 15, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Nothing reported until a pass answers, and a worker with the scheduler switched off never
    /// has one. Null rather than an invented "unknown", so a report that says nothing is visibly
    /// saying nothing.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WRK-CTRL-01")]
    public void NothingIsReportedBeforeTheFirstPass()
    {
        var status = new SchedulerControllerStatus(new FixedTimeProvider(T0));

        Assert.Null(status.Current);
    }

    /// <summary>
    /// Only <c>Held</c> permits a dial, and the other three say so while naming which one they are.
    /// <para>
    /// The distinction is the whole point of the surface. "Not dialling" is one bit; "waiting for
    /// somebody to confirm the previous controller is gone" is an instruction to a person, and
    /// the difference between those two is how long a stopped queue stays stopped.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData(AriControllerStatus.Held, true)]
    [InlineData(AriControllerStatus.HeldByAnotherWorker, false)]
    [InlineData(AriControllerStatus.AwaitingIsolation, false)]
    [InlineData(AriControllerStatus.AwaitingReconciliation, false)]
    [Trait("TestId", "UT-WRK-CTRL-02")]
    public void EveryStatusIsReportedAndOnlyHeldMayDial(
        AriControllerStatus reported,
        bool expectedMayDial)
    {
        var status = new SchedulerControllerStatus(new FixedTimeProvider(T0));

        status.Report("LAB_REAL_SIM:lab:ivr-lab", reported, 7);

        SchedulerControllerSnapshot snapshot = Assert.IsType<SchedulerControllerSnapshot>(
            status.Current);
        Assert.Equal("LAB_REAL_SIM:lab:ivr-lab", snapshot.Scope);
        Assert.Equal(reported.ToString(), snapshot.Status);
        Assert.Equal(expectedMayDial, snapshot.MayDial);
        Assert.Equal(7, snapshot.FencingGeneration);
        Assert.Equal(T0, snapshot.ObservedAt);
    }

    /// <summary>
    /// The latest answer replaces the last, including a generation that moved after a handover.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WRK-CTRL-03")]
    public void TheLatestPassReplacesTheOneBefore()
    {
        var clock = new FixedTimeProvider(T0);
        var status = new SchedulerControllerStatus(clock);

        status.Report("scope", AriControllerStatus.AwaitingIsolation, 4);
        status.Report("scope", AriControllerStatus.Held, 5);

        SchedulerControllerSnapshot snapshot = Assert.IsType<SchedulerControllerSnapshot>(
            status.Current);
        Assert.Equal(nameof(AriControllerStatus.Held), snapshot.Status);
        Assert.True(snapshot.MayDial);
        Assert.Equal(5, snapshot.FencingGeneration);
    }

    /// <summary>
    /// A controller that may not dial is reported in the body and does NOT fail the probe.
    /// <para>
    /// This is the rule the surface exists to get right. A worker waiting for somebody to confirm
    /// the previous controller is gone is behaving exactly as designed; failing the probe would
    /// restart it, which cannot grant it the application and would drop the calls it is still
    /// draining. The endpoint already makes this argument for Idle - a restart cannot start a loop
    /// configuration turned off - and this is the same argument for the one other state that looks
    /// broken and is not.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData(AriControllerStatus.AwaitingIsolation)]
    [InlineData(AriControllerStatus.AwaitingReconciliation)]
    [InlineData(AriControllerStatus.HeldByAnotherWorker)]
    [Trait("TestId", "UT-WRK-CTRL-04")]
    public void AControllerThatMayNotDialIsVisibleButDoesNotFailTheProbe(
        AriControllerStatus reported)
    {
        var status = new SchedulerControllerStatus(new FixedTimeProvider(T0));
        status.Report("LAB_REAL_SIM:lab:ivr-lab", reported, 3);
        var live = new WorkerLivenessReport(WorkerLivenessStatus.Live, []);

        (int statusCode, byte[] body) = WorkerHealthEndpoint.BuildResponse(live, status.Current);
        string json = System.Text.Encoding.UTF8.GetString(body);

        Assert.Equal(200, statusCode);
        Assert.Contains(reported.ToString(), json, StringComparison.Ordinal);
        Assert.Contains("\"may_dial\":false", json, StringComparison.Ordinal);
        Assert.Contains("LAB_REAL_SIM:lab:ivr-lab", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// A stalled loop still fails the probe, with the controller section present. The controller
    /// is reported alongside the liveness verdict, never instead of it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WRK-CTRL-05")]
    public void AStalledLoopStillFailsTheProbeWhileHoldingTheApplication()
    {
        var status = new SchedulerControllerStatus(new FixedTimeProvider(T0));
        status.Report("scope", AriControllerStatus.Held, 1);
        var stalled = new WorkerLivenessReport(
            WorkerLivenessStatus.Stalled,
            [new WorkerLoopHealth("scheduler", true, T0, true, 0, null)]);

        (int statusCode, byte[] body) = WorkerHealthEndpoint.BuildResponse(stalled, status.Current);
        string json = System.Text.Encoding.UTF8.GetString(body);

        Assert.Equal(503, statusCode);
        Assert.Contains("\"may_dial\":true", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Before the first pass the section is absent rather than invented. A body that guessed
    /// "Held" would be the one lie this surface cannot afford.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WRK-CTRL-06")]
    public void TheControllerSectionIsAbsentUntilAPassAnswers()
    {
        var live = new WorkerLivenessReport(WorkerLivenessStatus.Idle, []);

        (int statusCode, byte[] body) = WorkerHealthEndpoint.BuildResponse(live, null);
        string json = System.Text.Encoding.UTF8.GetString(body);

        Assert.Equal(200, statusCode);
        Assert.Contains("\"ari_controller\":null", json, StringComparison.Ordinal);
        Assert.DoesNotContain("may_dial", json, StringComparison.Ordinal);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
