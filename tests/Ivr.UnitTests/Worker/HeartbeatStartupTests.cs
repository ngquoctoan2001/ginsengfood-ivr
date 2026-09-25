using Ivr.Worker;
using Microsoft.Extensions.Logging;

namespace Ivr.UnitTests.Worker;

/// <summary>
/// W-0360 / K-33. The heartbeat's first report runs the moment the worker starts, before the job
/// hosts have registered, and every worker opened its log with a false "loops have stopped
/// ticking: (none registered)". The healthy line also counted loops configured off as turning.
/// </summary>
public sealed class HeartbeatStartupTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 25, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-WORKER-HEARTBEAT-01")]
    public void AnEmptyRegistryIsStartingOnlyInsideTheGrace()
    {
        WorkerLivenessReport empty = new WorkerLiveness(new FixedClock(Start)).Read();

        Assert.True(IvrHeartbeat.IsStillStarting(empty, TimeSpan.Zero));
        Assert.True(IvrHeartbeat.IsStillStarting(empty, IvrHeartbeat.StartupGrace - TimeSpan.FromSeconds(1)));

        // Past the grace an empty registry is the defect WorkerLiveness calls Stalled on purpose:
        // no host reached its registration at all. That must still be reported.
        Assert.False(IvrHeartbeat.IsStillStarting(empty, IvrHeartbeat.StartupGrace));

        var liveness = new WorkerLiveness(new FixedClock(Start));
        liveness.Register("scheduler", TimeSpan.FromSeconds(1));
        Assert.False(IvrHeartbeat.IsStillStarting(liveness.Read(), TimeSpan.Zero));
    }

    [Fact]
    [Trait("TestId", "UT-WORKER-HEARTBEAT-02")]
    public void TheFirstReportNeitherCriesStalledAtStartNorCountsLoopsThatAreOff()
    {
        var clock = new FixedClock(Start);

        // Before any host registers: nothing is said, and in particular no warning.
        var starting = new CapturingLogger();
        using (var heartbeat = new IvrHeartbeat(new WorkerLiveness(clock), starting, clock))
        {
            heartbeat.ReportOnce(TimeSpan.Zero);
        }

        Assert.Empty(starting.Entries);

        // Still nothing registered once the grace is over: that is the defect, and it is said.
        var late = new CapturingLogger();
        using (var heartbeat = new IvrHeartbeat(new WorkerLiveness(clock), late, clock))
        {
            heartbeat.ReportOnce(IvrHeartbeat.StartupGrace);
        }

        (LogLevel lateLevel, string lateText) = Assert.Single(late.Entries);
        Assert.Equal(LogLevel.Warning, lateLevel);
        Assert.Contains("(none registered)", lateText, StringComparison.Ordinal);

        // One loop on, two off: the healthy line counts the one that turns.
        var liveness = new WorkerLiveness(clock);
        liveness.Register("scheduler", TimeSpan.FromSeconds(1));
        liveness.RegisterDisabled("analytics");
        liveness.RegisterDisabled("retention");
        var running = new CapturingLogger();
        using (var heartbeat = new IvrHeartbeat(liveness, running, clock))
        {
            heartbeat.ReportOnce(TimeSpan.Zero);
        }

        (LogLevel level, string text) = Assert.Single(running.Entries);
        Assert.Equal(LogLevel.Information, level);
        Assert.Equal("IVR worker heartbeat: 1 background loops turning.", text);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CapturingLogger : ILogger<IvrHeartbeat>
    {
        private readonly List<(LogLevel Level, string Text)> entries = [];

        public IReadOnlyList<(LogLevel Level, string Text)> Entries
        {
            get
            {
                lock (entries)
                {
                    return [.. entries];
                }
            }
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            lock (entries)
            {
                entries.Add((logLevel, formatter(state, exception)));
            }
        }
    }
}
