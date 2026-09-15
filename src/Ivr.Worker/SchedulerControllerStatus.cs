using Ivr.Infrastructure.Telephony;

namespace Ivr.Worker;

/// <param name="MayDial">
/// Whether this worker is allowed to place calls. The other three statuses each mean "no" for a
/// different reason, and an operator reading a stopped queue needs the reason more than the bit.
/// </param>
public sealed record SchedulerControllerSnapshot(
    string Scope,
    string Status,
    long FencingGeneration,
    bool MayDial,
    DateTimeOffset ObservedAt);

/// <summary>
/// The last ARI controller answer this worker got, so a probe can report it. SIP-05.
/// <para>
/// V1 deliberately has no button for taking an application away from a controller: the deployment
/// profile is a single replica and the cost of getting it wrong is two controllers dialling the
/// same customers, so isolation is a considered action against the database. Not having a button
/// is a decision. Not being able to <i>see</i> the state was not - it was a gap, and it is the one
/// that matters more, because a worker stuck on <c>AwaitingIsolation</c> waits for somebody to
/// notice and nothing was telling anybody.
/// </para>
/// <para>
/// Last observed, never queried. The scheduler pass writes here; the health endpoint reads. A
/// probe that hit the database would fail whenever the database did, turning one outage into a
/// restart of a worker that was correctly waiting out that same outage.
/// </para>
/// </summary>
public sealed class SchedulerControllerStatus(TimeProvider timeProvider)
{
    private SchedulerControllerSnapshot? current;

    /// <summary>Null until the first scheduler pass answers, which a disabled worker never does.</summary>
    public SchedulerControllerSnapshot? Current => Volatile.Read(ref current);

    public void Report(string scope, AriControllerStatus status, long fencingGeneration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        Volatile.Write(
            ref current,
            new SchedulerControllerSnapshot(
                scope,
                status.ToString(),
                fencingGeneration,
                status == AriControllerStatus.Held,
                timeProvider.GetUtcNow()));
    }
}
