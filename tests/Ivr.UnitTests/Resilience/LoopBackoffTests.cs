using Ivr.Infrastructure.Resilience;

namespace Ivr.UnitTests.Resilience;

/// <summary>
/// The worker loops used to retry at exactly their poll interval no matter what was wrong, so a
/// dependency outage produced a stack trace and a fresh connection attempt every 100 ms per loop
/// per replica. These pin the schedule that replaced it.
/// </summary>
public sealed class LoopBackoffTests
{
    private static readonly TimeSpan Poll = TimeSpan.FromSeconds(1);

    [Fact]
    [Trait("TestId", "UT-BACKOFF-SCHEDULE-01")]
    public void TheDelayDoublesPerConsecutiveFailure()
    {
        // The first failure waits one poll interval, not zero: retrying instantly is the behaviour
        // being removed, and it costs the most at the start, because that is when a burst of
        // replicas all discover the same outage at once.
        Assert.Equal(TimeSpan.FromSeconds(1), LoopBackoff.Compute(Poll, 1));
        Assert.Equal(TimeSpan.FromSeconds(2), LoopBackoff.Compute(Poll, 2));
        Assert.Equal(TimeSpan.FromSeconds(4), LoopBackoff.Compute(Poll, 3));
        Assert.Equal(TimeSpan.FromSeconds(16), LoopBackoff.Compute(Poll, 5));

        // A pass that has not failed waits for nothing.
        Assert.Equal(TimeSpan.Zero, LoopBackoff.Compute(Poll, 0));
    }

    [Fact]
    [Trait("TestId", "UT-BACKOFF-CEILING-02")]
    public void TheDelayStopsGrowingAtTheCeiling()
    {
        // Bounded on purpose. A loop that backed off without limit would still be "retrying"
        // hours after the dependency came back, which is a different way of being broken.
        Assert.Equal(LoopBackoff.Ceiling, LoopBackoff.Compute(Poll, 6));
        Assert.Equal(LoopBackoff.Ceiling, LoopBackoff.Compute(Poll, 50));

        // The doubling is clamped below the point where it would overflow, so a long outage or an
        // absurd poll interval cannot turn a backoff into a negative delay.
        Assert.Equal(LoopBackoff.Ceiling, LoopBackoff.Compute(Poll, int.MaxValue));
        Assert.True(LoopBackoff.Compute(TimeSpan.FromDays(1), int.MaxValue) > TimeSpan.Zero);
    }

    [Theory]
    [Trait("TestId", "UT-BACKOFF-JITTER-03")]
    [InlineData(0d)]
    [InlineData(0.5d)]
    [InlineData(0.999d)]
    public void JitterNeverReturnsLessThanHalfTheComputedDelay(double sample)
    {
        // Replicas that failed together must not all return on the same tick, so the wait is
        // randomised. Half is kept rather than jittering the whole span: full jitter can return a
        // near-zero wait, which is exactly the case the backoff exists to prevent. Asserted at
        // both ends of the range instead of sampled, so the floor is a property and not a
        // probability.
        TimeSpan computed = LoopBackoff.Compute(Poll, 3);
        TimeSpan jittered = LoopBackoff.ApplyJitter(computed, sample);

        Assert.True(jittered >= computed / 2, $"{jittered} fell below the guaranteed floor");
        Assert.True(jittered <= computed, $"{jittered} exceeded the computed delay");
    }

    [Fact]
    [Trait("TestId", "UT-BACKOFF-RESET-04")]
    public async Task OneSuccessfulPassClearsTheStreak()
    {
        // Cancelled up front so no test sleeps: the streak is counted before the wait begins, and
        // the streak is the part with the bug in it.
        using var stopped = new CancellationTokenSource();
        await stopped.CancelAsync();
        var backoff = new LoopBackoff(Poll, TimeProvider.System);

        Assert.False(await backoff.DelayAfterFailureAsync(stopped.Token));
        Assert.False(await backoff.DelayAfterFailureAsync(stopped.Token));
        Assert.Equal(2, backoff.ConsecutiveFailures);

        backoff.RecordSuccess();
        Assert.Equal(0, backoff.ConsecutiveFailures);

        // Back to the shortest wait rather than resuming where the previous outage left off: a
        // loop that recovered is not a loop that is still failing.
        Assert.False(await backoff.DelayAfterFailureAsync(stopped.Token));
        Assert.Equal(1, backoff.ConsecutiveFailures);
    }

    [Fact]
    [Trait("TestId", "UT-BACKOFF-SHUTDOWN-05")]
    public async Task ShutdownDuringTheWaitReportsStopRatherThanThrowing()
    {
        // The callers use the result to break out of their loop, exactly as they do with
        // PeriodicTimer.WaitForNextTickAsync. Throwing here would escape from inside a catch block
        // on a shutdown path — not an error, and it must not be logged as one.
        using var stopped = new CancellationTokenSource();
        await stopped.CancelAsync();
        var backoff = new LoopBackoff(Poll, TimeProvider.System);

        Assert.False(await backoff.DelayAfterFailureAsync(stopped.Token));
    }
}
