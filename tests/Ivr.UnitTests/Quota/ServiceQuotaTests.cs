using Ivr.Infrastructure.Quota;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Quota;

/// <summary>
/// W-0282 / B2. The per-account ceiling that finally makes <c>IVR_RATE_LIMITED</c> reachable.
/// </summary>
public sealed class ServiceQuotaCounterTests
{
    private static readonly DateTimeOffset Origin =
        new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    [Fact]
    [Trait("TestId", "UT-QUOTA-01")]
    public void ABudgetIsSpentOneRequestAtATimeAndThenRefused()
    {
        MutableClock clock = new(Origin);
        ServiceQuotaCounter counter = new(clock);

        Assert.Equal(
            [(true, 2), (true, 1), (true, 0)],
            Enumerable.Range(0, 3)
                .Select(_ => counter.Admit("service/order-core", 3, Window))
                .Select(decision => (decision.Allowed, decision.Remaining)));

        ServiceQuotaDecision refused = counter.Admit("service/order-core", 3, Window);
        Assert.False(refused.Allowed);
        Assert.Equal(0, refused.Remaining);
        Assert.Equal(3, refused.Limit);
        Assert.InRange(refused.RetryAfter, TimeSpan.Zero, Window);
    }

    [Fact]
    [Trait("TestId", "UT-QUOTA-02")]
    public void TheWindowRollsOverAndTheBudgetComesBackWhole()
    {
        MutableClock clock = new(Origin);
        ServiceQuotaCounter counter = new(clock);
        for (int i = 0; i < 3; i++)
        {
            counter.Admit("service/order-core", 3, Window);
        }

        Assert.False(counter.Admit("service/order-core", 3, Window).Allowed);

        clock.Advance(Window);
        ServiceQuotaDecision afterRollover = counter.Admit("service/order-core", 3, Window);
        Assert.True(afterRollover.Allowed);
        Assert.Equal(2, afterRollover.Remaining);
    }

    /// <summary>
    /// A refused request must not extend the refusal. If refusals incremented the counter, a
    /// client that retried on a tight loop would hold its own window open forever and the ceiling
    /// would read as a permanent outage rather than a budget.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-QUOTA-03")]
    public void BeingRefusedDoesNotSpendMoreOfTheNextWindow()
    {
        MutableClock clock = new(Origin);
        ServiceQuotaCounter counter = new(clock);
        counter.Admit("service/order-core", 1, Window);
        for (int i = 0; i < 50; i++)
        {
            Assert.False(counter.Admit("service/order-core", 1, Window).Allowed);
        }

        clock.Advance(Window);
        Assert.True(counter.Admit("service/order-core", 1, Window).Allowed);
    }

    [Fact]
    [Trait("TestId", "UT-QUOTA-04")]
    public void OneAccountSpendingItsBudgetDoesNotSpendAnother()
    {
        MutableClock clock = new(Origin);
        ServiceQuotaCounter counter = new(clock);
        counter.Admit("service/order-core", 1, Window);

        Assert.False(counter.Admit("service/order-core", 1, Window).Allowed);
        Assert.True(counter.Admit("admin/ivr.admin.read", 1, Window).Allowed);
    }

    /// <summary>
    /// The remainder shrinks as the window ages, so a client that is refused late in a window is
    /// told to wait the short time that is actually left rather than a whole window.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-QUOTA-05")]
    public void RetryAfterIsWhatIsLeftOfThisWindowNotAWholeOne()
    {
        MutableClock clock = new(Origin);
        ServiceQuotaCounter counter = new(clock);
        counter.Admit("service/order-core", 1, Window);

        clock.Advance(TimeSpan.FromSeconds(45));
        ServiceQuotaDecision refused = counter.Admit("service/order-core", 1, Window);

        Assert.False(refused.Allowed);
        Assert.Equal(TimeSpan.FromSeconds(15), refused.RetryAfter);
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset current = now;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan by) => current += by;
    }
}

public sealed class ServiceQuotaOptionsValidatorTests
{
    [Fact]
    [Trait("TestId", "UT-QUOTA-06")]
    public void ADisabledSectionIsNotValidatedBecauseItChangesNothing()
    {
        ServiceQuotaOptions options = new() { Enabled = false, WindowSeconds = 0, RequestsPerWindow = 0 };

        Assert.True(new ServiceQuotaOptionsValidator().Validate(null, options).Succeeded);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(3601, 10)]
    [InlineData(60, 0)]
    [InlineData(60, 1_000_001)]
    [Trait("TestId", "UT-QUOTA-07")]
    public void ACeilingThatWouldRefuseEveryoneFailsTheDeploymentInstead(
        int windowSeconds,
        int requestsPerWindow)
    {
        ServiceQuotaOptions options = new()
        {
            Enabled = true,
            WindowSeconds = windowSeconds,
            RequestsPerWindow = requestsPerWindow,
        };

        Assert.True(new ServiceQuotaOptionsValidator().Validate(null, options).Failed);
    }

    [Fact]
    [Trait("TestId", "UT-QUOTA-08")]
    public void APerAccountOverrideIsHeldToTheSameRange()
    {
        ServiceQuotaOptions options = new() { Enabled = true };
        options.Accounts["order-core"] = new ServiceQuotaAccountOptions { RequestsPerWindow = 0 };

        ValidateOptionsResult result = new ServiceQuotaOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("Accounts:order-core:RequestsPerWindow", result.FailureMessage!, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-QUOTA-09")]
    public void AValidSectionWithAnOverridePasses()
    {
        ServiceQuotaOptions options = new() { Enabled = true, WindowSeconds = 60, RequestsPerWindow = 600 };
        options.Accounts["order-core"] = new ServiceQuotaAccountOptions
        {
            RequestsPerWindow = 30,
            WindowSeconds = 60,
        };

        Assert.True(new ServiceQuotaOptionsValidator().Validate(null, options).Succeeded);
    }
}
