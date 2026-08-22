using Ivr.Api.Accounts;
using Ivr.Domain.Accounts;
using Ivr.Domain.Errors;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0105 remediation. The limiter keyed its single counter on <c>"{ip}|{username}"</c>, which
/// constrained neither axis: one host got a full budget per username it tried, and a pool of
/// hosts got a full budget each against one username. These tests drive each axis on its own,
/// which is the only way to tell a real two-axis limit from the pair-keyed one — the pair key
/// passes any test that varies both together.
///
/// Lives in the integration project because <c>ConsoleSignInRateLimiter</c> is an Ivr.Api type
/// and the unit project deliberately does not reference Ivr.Api. No database is touched.
/// </summary>
public sealed class ConsoleSignInRateLimiterTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 22, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-RATE-12")]
    public void OneAddressIsCappedAcrossEveryUsernameItTries()
    {
        var clock = new FixedClock(Start);
        var limiter = new ConsoleSignInRateLimiter(clock);

        // Under the old pair key this loop never tripped: each username had its own counter.
        for (int attempt = 0; attempt < ConsoleSignInRateLimiter.PerAddressLimit; attempt++)
        {
            limiter.RequireAllowed($"user{attempt}", "203.0.113.9");
        }

        IvrFailureException failure = Assert.Throws<IvrFailureException>(
            () => limiter.RequireAllowed("user-never-seen-before", "203.0.113.9"));
        Assert.Equal(IvrErrorCodes.RateLimited, failure.ErrorCode);
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-RATE-12")]
    public void OneUsernameIsCappedAcrossEveryAddressThatAttacksIt()
    {
        var clock = new FixedClock(Start);
        var limiter = new ConsoleSignInRateLimiter(clock);

        // Under the old pair key this loop never tripped either: each address had its own counter,
        // so a distributed run against a single account was unlimited.
        for (int attempt = 0; attempt < ConsoleSignInRateLimiter.PerUsernameLimit; attempt++)
        {
            limiter.RequireAllowed("admin", $"198.51.100.{attempt}");
        }

        IvrFailureException failure = Assert.Throws<IvrFailureException>(
            () => limiter.RequireAllowed("admin", "198.51.100.200"));
        Assert.Equal(IvrErrorCodes.RateLimited, failure.ErrorCode);
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-RATE-12")]
    public void ARequestRefusedByOneAxisStillCountsAgainstTheOther()
    {
        var clock = new FixedClock(Start);
        var limiter = new ConsoleSignInRateLimiter(clock);

        // Exhaust the username axis from one address.
        for (int attempt = 0; attempt < ConsoleSignInRateLimiter.PerUsernameLimit; attempt++)
        {
            limiter.RequireAllowed("admin", "203.0.113.10");
        }

        // Keep hammering the now-blocked username. If refusals were free the address counter
        // would never move, and the host could probe indefinitely at no cost.
        int spentOnAddress = ConsoleSignInRateLimiter.PerUsernameLimit;
        while (spentOnAddress < ConsoleSignInRateLimiter.PerAddressLimit)
        {
            Assert.Throws<IvrFailureException>(
                () => limiter.RequireAllowed("admin", "203.0.113.10"));
            spentOnAddress++;
        }

        // The address budget is now spent too, so even a fresh username from this host is refused.
        Assert.Throws<IvrFailureException>(
            () => limiter.RequireAllowed("someone.else", "203.0.113.10"));
    }

    [Fact]
    [Trait("TestId", "UT-ACCOUNT-RATE-12")]
    public void BothWindowsReopenOnceTheMinuteHasPassed()
    {
        var clock = new FixedClock(Start);
        var limiter = new ConsoleSignInRateLimiter(clock);
        for (int attempt = 0; attempt < ConsoleSignInRateLimiter.PerUsernameLimit; attempt++)
        {
            limiter.RequireAllowed("admin", "203.0.113.11");
        }

        Assert.Throws<IvrFailureException>(() => limiter.RequireAllowed("admin", "203.0.113.11"));

        clock.Advance(ConsoleSignInRateLimiter.Window + TimeSpan.FromSeconds(1));
        limiter.RequireAllowed("admin", "203.0.113.11");
    }

    /// <summary>
    /// The two anti-guessing controls have to be ordered, not merely both present.
    ///
    /// Introducing the per-username limit at the same value as the lockout threshold made the
    /// limiter answer 429 on the very request that would have revealed the lockout, so the
    /// account-level control stopped being reachable through the API and
    /// <c>IT-ACCOUNT-LOCK-03</c> went red. That was a real regression, not a test artefact:
    /// a lockout nobody can observe is a lockout nobody can rely on. This test pins the
    /// ordering so the next person to tune either number is told, rather than finding out
    /// through an unrelated failing test.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-ACCOUNT-RATE-12")]
    public void ThePerUsernameLimitStaysAboveTheLockoutThreshold()
    {
        Assert.True(
            ConsoleSignInRateLimiter.PerUsernameLimit
                > ConsoleLockoutPolicy.MaximumFailedAttempts,
            "The per-username rate limit must leave room for the lockout to fire and be observed; "
            + "equal or lower values mask it behind a 429.");

        // And the address budget must leave room for the username budget, or the per-username
        // axis could never be reached from a single host and would only ever be exercised by a
        // distributed run.
        Assert.True(
            ConsoleSignInRateLimiter.PerAddressLimit > ConsoleSignInRateLimiter.PerUsernameLimit,
            "The per-address budget must exceed the per-username budget.");
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset current = now;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan delta) => current = current.Add(delta);
    }
}
