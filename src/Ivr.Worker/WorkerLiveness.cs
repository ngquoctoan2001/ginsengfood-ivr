using System.Collections.Concurrent;

namespace Ivr.Worker;

/// <summary>
/// One record per background loop, and the distinction it draws is the whole point.
/// <para>
/// <b>Ticking</b> means the loop completed a pass — successfully or not. <b>Faulting</b> means the
/// last pass threw. They are reported separately because only one of them is a reason to restart
/// the process: a wedged loop is fixed by a restart, and a loop failing because PostgreSQL is down
/// is not. A liveness probe that restarted on downstream failure would turn one outage into a
/// restart storm during the outage, which is the worst possible moment for it.
/// </para>
/// </summary>
public sealed record WorkerLoopHealth(
    string Loop,
    bool Enabled,
    DateTimeOffset LastTickAt,
    bool Stale,
    int ConsecutiveFaults,
    string? LastFaultKind);

/// <summary>
/// Three states, because three different things are wrong and only one of them is fixed by a
/// restart. Running the endpoint for the first time is what made the third one obvious: the shipped
/// worker has every loop disabled, so a two-state report answered 503 forever on a worker that was
/// behaving exactly as configured, and a liveness probe would have restarted it in a loop.
/// </summary>
public enum WorkerLivenessStatus
{
    /// <summary>A loop that was meant to be running has stopped. A restart is the remedy.</summary>
    Stalled,

    /// <summary>Nothing is configured to run. Visible, but a restart changes nothing.</summary>
    Idle,

    /// <summary>Every enabled loop is turning.</summary>
    Live,
}

public sealed record WorkerLivenessReport(
    WorkerLivenessStatus Status,
    IReadOnlyList<WorkerLoopHealth> Loops)
{
    /// <summary>
    /// Idle counts as live for probing purposes, and that is the point of separating them: a
    /// restart cannot start a loop the configuration turned off, so failing the probe would just
    /// convert a deliberate configuration into a crash loop. The status field is what says which.
    /// </summary>
    public bool Live => Status is WorkerLivenessStatus.Live or WorkerLivenessStatus.Idle;

    public IReadOnlyList<string> StaleLoops =>
        [.. Loops.Where(loop => loop.Stale).Select(loop => loop.Loop)];
}

/// <summary>
/// Tracks whether each worker loop is still turning (<c>W-0043</c> §2).
/// <para>
/// Every job host catches its own exceptions and keeps polling, which is right — one failed pass
/// must not take the process down. The cost is that a loop which fails forever, or one that hangs
/// inside a call that never returns, is indistinguishable from a healthy one: the process is up,
/// the container is running, and nothing is being processed. Kubernetes restarting the pod when
/// the process exits was the only liveness signal the worker had, and a wedge does not exit.
/// </para>
/// <para>
/// Registration is explicit rather than inferred. A loop that forgot to register would otherwise
/// be silently exempt from the check, and the loops most worth watching are the ones somebody
/// added without thinking about health.
/// </para>
/// </summary>
public sealed class WorkerLiveness(TimeProvider timeProvider)
{
    // Three intervals before a loop is called stale. One is far too tight -- a single slow database
    // round trip would trip it -- and the point of the check is to catch a loop that has stopped,
    // not one that is having a bad second.
    private const int StaleIntervalMultiple = 3;

    // ...and never less than this, however fast the loop polls. The scheduler polls every second;
    // three seconds of grace would make an ordinary GC pause look like a wedge.
    private static readonly TimeSpan MinimumGrace = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<string, LoopState> loops =
        new(StringComparer.Ordinal);

    public void Register(string loop, TimeSpan pollInterval)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loop);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pollInterval, TimeSpan.Zero);
        TimeSpan grace = pollInterval * StaleIntervalMultiple;
        loops[loop] = new LoopState(
            true,
            grace < MinimumGrace ? MinimumGrace : grace,
            timeProvider.GetUtcNow());
    }

    /// <summary>
    /// The loop is wired but configuration says do not run it. Registered anyway, so the report can
    /// tell "turned off" from "never wired" -- the first is a decision, the second is a defect.
    /// </summary>
    public void RegisterDisabled(string loop)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loop);
        loops[loop] = new LoopState(false, Timeout.InfiniteTimeSpan, timeProvider.GetUtcNow());
    }

    /// <summary>
    /// The loop completed a pass. Called whether the pass succeeded or threw, because this answers
    /// "is it turning", not "is it working".
    /// </summary>
    public void Tick(string loop)
    {
        if (loops.TryGetValue(loop, out LoopState? state))
        {
            state.LastTickAt = timeProvider.GetUtcNow();
            state.ConsecutiveFaults = 0;
            state.LastFaultKind = null;
        }
    }

    /// <summary>
    /// The pass threw. Recorded and surfaced, but deliberately NOT a liveness failure — see the
    /// note on <see cref="WorkerLoopHealth"/>.
    /// </summary>
    public void Fault(string loop, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (loops.TryGetValue(loop, out LoopState? state))
        {
            state.LastTickAt = timeProvider.GetUtcNow();
            state.ConsecutiveFaults = checked(state.ConsecutiveFaults + 1);
            // The TYPE only. An exception message can carry a connection string, a row value or a
            // masked-but-not-quite phone number, and this report is read by anything that can
            // reach the probe.
            state.LastFaultKind = exception.GetType().Name;
        }
    }

    public WorkerLivenessReport Read()
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        WorkerLoopHealth[] health = [.. loops
            .Select(entry => new WorkerLoopHealth(
                entry.Key,
                entry.Value.Enabled,
                entry.Value.LastTickAt,
                entry.Value.Enabled && now - entry.Value.LastTickAt > entry.Value.Grace,
                entry.Value.ConsecutiveFaults,
                entry.Value.LastFaultKind))
            .OrderBy(loop => loop.Loop, StringComparer.Ordinal)];

        // An empty registry is STALLED, not idle. Empty means no host reached its registration at
        // all -- the wiring was removed, or every host crashed before starting -- and that is a
        // defect rather than a configuration.
        if (health.Length == 0)
        {
            return new WorkerLivenessReport(WorkerLivenessStatus.Stalled, health);
        }

        if (health.Any(loop => loop.Stale))
        {
            return new WorkerLivenessReport(WorkerLivenessStatus.Stalled, health);
        }

        return new WorkerLivenessReport(
            health.Any(loop => loop.Enabled)
                ? WorkerLivenessStatus.Live
                : WorkerLivenessStatus.Idle,
            health);
    }

    private sealed class LoopState(bool enabled, TimeSpan grace, DateTimeOffset lastTickAt)
    {
        public bool Enabled { get; } = enabled;

        public TimeSpan Grace { get; } = grace;

        public DateTimeOffset LastTickAt { get; set; } = lastTickAt;

        public int ConsecutiveFaults { get; set; }

        public string? LastFaultKind { get; set; }
    }
}
