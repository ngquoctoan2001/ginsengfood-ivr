namespace Ivr.Infrastructure.Resilience;

/// <summary>
/// Extra wait a polling loop takes after a failure, on top of its own poll interval.
/// <para>
/// Without it a worker loop retries at exactly its poll interval no matter what is wrong. The
/// scheduler polls every 1000 ms by default and every 100 ms under the LocalMockE2E profile, so a
/// database outage produced a full error line with a stack trace, and a fresh connection attempt,
/// ten times a second per loop per replica — for as long as the outage lasted. That buries the one
/// log line explaining the outage under thousands of identical ones, and the connection storm
/// keeps the database from coming back.
/// </para>
/// <para>
/// Backing off is not the same as giving up: the loop keeps trying, just at a rate that leaves
/// room for recovery. There is no failure count at which it stops, because a loop that stopped
/// would need a human to notice and restart it, which is strictly worse than one that is slow.
/// </para>
/// </summary>
public sealed class LoopBackoff(TimeSpan pollInterval, TimeProvider timeProvider)
{
    /// <summary>
    /// Longest extra wait, however long the outage runs. A loop that backed off unboundedly would
    /// still be "retrying" hours after the dependency returned.
    /// </summary>
    public static readonly TimeSpan Ceiling = TimeSpan.FromSeconds(30);

    private int consecutiveFailures;

    public int ConsecutiveFailures => consecutiveFailures;

    /// <summary>Clears the streak. A loop that worked once is not backing off any more.</summary>
    public void RecordSuccess() => consecutiveFailures = 0;

    /// <summary>
    /// Counts a failure and waits before the caller's next pass.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> when the wait was cut short by shutdown, so a caller can
    /// <c>break</c> on it exactly as it does on <c>PeriodicTimer.WaitForNextTickAsync</c>.
    /// Cancellation is a normal stop here, not an error, so it is returned rather than thrown.
    /// </returns>
    public async Task<bool> DelayAfterFailureAsync(CancellationToken cancellationToken)
    {
        consecutiveFailures++;
        TimeSpan delay = ApplyJitter(
            Compute(pollInterval, consecutiveFailures),
            Random.Shared.NextDouble());
        try
        {
            await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// The wait for a given streak length, before jitter. Pure, so the schedule can be asserted
    /// rather than observed through a sleeping test.
    /// </summary>
    public static TimeSpan Compute(TimeSpan pollInterval, int consecutiveFailures)
    {
        if (consecutiveFailures < 1)
        {
            return TimeSpan.Zero;
        }

        // The shift is clamped well below the point where the doubling would overflow. The
        // ceiling bites long before that for any sane poll interval; the clamp is here so an
        // absurd one cannot turn a backoff into a negative delay.
        int shift = Math.Min(consecutiveFailures - 1, 30);
        double milliseconds = pollInterval.TotalMilliseconds * Math.Pow(2, shift);
        return milliseconds >= Ceiling.TotalMilliseconds
            ? Ceiling
            : TimeSpan.FromMilliseconds(milliseconds);
    }

    /// <summary>
    /// Spreads replicas that failed together so they do not all come back on the same tick.
    /// <para>
    /// Half the computed delay is kept and half is randomised, rather than randomising the whole
    /// thing: full jitter can return a near-zero wait, which is the case the backoff exists to
    /// prevent. Keeping the floor means the delay is always at least half of what was computed.
    /// </para>
    /// <para>
    /// Takes the random sample rather than drawing it, so the property that matters — the floor —
    /// can be asserted at both ends of the range instead of sampled and hoped for.
    /// </para>
    /// </summary>
    /// <param name="sample">A value in <c>[0, 1)</c>, as from <see cref="Random.NextDouble"/>.</param>
    public static TimeSpan ApplyJitter(TimeSpan delay, double sample)
    {
        double half = delay.TotalMilliseconds / 2d;
        return TimeSpan.FromMilliseconds(half + (Math.Clamp(sample, 0d, 1d) * half));
    }
}
