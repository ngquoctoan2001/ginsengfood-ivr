using System.Collections.Concurrent;

namespace Ivr.Infrastructure.Quota;

/// <summary>What the ceiling decided about one request.</summary>
/// <param name="Allowed">Whether the request may proceed.</param>
/// <param name="Limit">The ceiling that applied, so a caller can report its own budget.</param>
/// <param name="Remaining">Requests left in this window after this one. Zero once refused.</param>
/// <param name="RetryAfter">How long until the window rolls over.</param>
public readonly record struct ServiceQuotaDecision(
    bool Allowed,
    int Limit,
    int Remaining,
    TimeSpan RetryAfter);

/// <summary>
/// W-0282 / B2. Fixed-window request counter, one window per account.
/// <para>
/// In memory, and therefore per instance. That is a real limitation and it is stated rather than
/// hidden: behind two replicas an account gets two windows, so the effective ceiling is the
/// configured one times the replica count. It is the right trade for what this is for — a sandbox
/// runs one API — and the alternative, a shared counter in PostgreSQL, would put a write on the
/// path of every request in order to make a non-production ceiling exact. When a real deployment
/// needs an exact ceiling, the gateway in front of it is where that belongs.
/// </para>
/// <para>
/// The window starts at an account's first request rather than on a clock boundary, so
/// <c>Retry-After</c> is the honest remainder of that account's own window and not a number a
/// caller has to align to.
/// </para>
/// </summary>
public sealed class ServiceQuotaCounter(TimeProvider timeProvider)
{
    /// <summary>
    /// Ceiling on tracked accounts. A key is derived from an authenticated identity, so the
    /// dictionary cannot be grown by an anonymous caller — but a long-lived process that rotates
    /// service accounts would accumulate windows nobody reads again, so stale ones are swept.
    /// </summary>
    private const int MaximumTrackedAccounts = 10_000;

    private readonly ConcurrentDictionary<string, Window> windows = new(StringComparer.Ordinal);

    private long lastSweepUtcTicks;

    public ServiceQuotaDecision Admit(string accountKey, int limit, TimeSpan window)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountKey);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);

        DateTimeOffset now = timeProvider.GetUtcNow();
        Window tracked = windows.GetOrAdd(accountKey, _ => new Window(now));

        ServiceQuotaDecision decision;
        lock (tracked)
        {
            if (now - tracked.StartedAt >= window)
            {
                tracked.StartedAt = now;
                tracked.Count = 0;
            }

            TimeSpan retryAfter = tracked.StartedAt + window - now;
            if (tracked.Count >= limit)
            {
                // Not incremented on refusal. Counting refusals would extend nothing — the window
                // is fixed — but it would make `Remaining` lie about a budget the caller still has
                // once the window rolls, and a client reading that number would back off twice.
                decision = new ServiceQuotaDecision(false, limit, 0, retryAfter);
            }
            else
            {
                tracked.Count++;
                decision = new ServiceQuotaDecision(true, limit, limit - tracked.Count, retryAfter);
            }
        }

        Sweep(now, window);
        return decision;
    }

    /// <summary>
    /// Drops windows that have already rolled over, then the oldest of whatever remains above the
    /// ceiling. Rate-limited by time the way <c>InMemoryIdempotencyStore</c> does it, because a
    /// sweep walks the whole dictionary and would otherwise make a soak run quadratic; the overflow
    /// check is not rate-limited, since it exists for the burst an interval lets through.
    /// </summary>
    private void Sweep(DateTimeOffset now, TimeSpan window)
    {
        long previous = Interlocked.Read(ref lastSweepUtcTicks);
        bool dueByTime = now.UtcTicks - previous >= window.Ticks;
        if (!dueByTime && windows.Count <= MaximumTrackedAccounts)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref lastSweepUtcTicks, now.UtcTicks, previous) != previous)
        {
            return;
        }

        foreach (KeyValuePair<string, Window> entry in windows)
        {
            DateTimeOffset startedAt;
            lock (entry.Value)
            {
                startedAt = entry.Value.StartedAt;
            }

            if (now - startedAt >= window)
            {
                windows.TryRemove(entry);
            }
        }

        int overflow = windows.Count - MaximumTrackedAccounts;
        if (overflow <= 0)
        {
            return;
        }

        foreach (KeyValuePair<string, Window> entry in windows
                     .OrderBy(pair => pair.Value.StartedAt)
                     .Take(overflow))
        {
            windows.TryRemove(entry);
        }
    }

    private sealed class Window(DateTimeOffset startedAt)
    {
        public DateTimeOffset StartedAt { get; set; } = startedAt;

        public int Count { get; set; }
    }
}
