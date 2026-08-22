using System.Collections.Concurrent;
using Ivr.Domain.Errors;

namespace Ivr.Api.Accounts;

/// <summary>
/// Two independent sliding counters, not one keyed on the pair.
/// <para>
/// Keying on <c>"{ip}|{username}"</c> produced a limit that constrained neither axis: one host
/// could spend the full budget against every username it cared to try, and a pool of hosts could
/// spend the full budget each against a single username. The per-username counter is what makes
/// a distributed password-guessing run expensive; the per-IP counter is what stops one host
/// enumerating. A request has to satisfy both.
/// </para>
/// <para>
/// The per-username limit must stay <b>above</b> <see cref="ConsoleLockoutPolicy"/>'s threshold.
/// Setting the two equal makes the limiter answer 429 on the request that would have revealed
/// the lockout, so the lockout stops being observable and the account-level control becomes dead
/// code — <c>IT-ACCOUNT-LOCK-03</c> caught exactly that. The account lockout is the control for
/// an account that exists; the per-username rate limit is what covers the case it cannot, which
/// is a username that does not exist and therefore has no row to count failures on.
/// </para>
/// </summary>
public sealed class ConsoleSignInRateLimiter(TimeProvider timeProvider)
{
    public const int PerAddressLimit = 30;
    public const int PerUsernameLimit = 10;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private const int SweepThreshold = 10_000;

    private readonly ConcurrentDictionary<string, Counter> byAddress =
        new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, Counter> byUsername =
        new(StringComparer.Ordinal);

    public void RequireAllowed(string username, string remoteAddress)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        // Both are recorded before either is judged, so a request that trips one limit still
        // counts against the other. Charging only the first tripped axis would let a caller
        // that is already rate-limited by address probe usernames for free.
        int addressCount = Record(byAddress, remoteAddress, now);
        int usernameCount = Record(byUsername, username, now);

        if (addressCount > PerAddressLimit || usernameCount > PerUsernameLimit)
        {
            throw IvrErrors.RateLimited();
        }
    }

    private static int Record(
        ConcurrentDictionary<string, Counter> counters,
        string key,
        DateTimeOffset now)
    {
        Counter counter = counters.AddOrUpdate(
            key,
            _ => new Counter(now, 1),
            (_, current) => now - current.StartedAt >= Window
                ? new Counter(now, 1)
                : current with { Count = current.Count + 1 });

        if (counters.Count > SweepThreshold)
        {
            foreach ((string staleKey, Counter value) in counters)
            {
                if (now - value.StartedAt >= Window)
                {
                    counters.TryRemove(staleKey, out _);
                }
            }
        }

        return counter.Count;
    }

    private sealed record Counter(DateTimeOffset StartedAt, int Count);
}
