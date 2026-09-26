using Ivr.Infrastructure.Callbacks;

namespace Ivr.UnitTests.Callbacks;

/// <summary>
/// Q-19 (PA2, 2026-09-26). The replay age limit: seven days unless configured, and a configuration
/// outside one to thirty days fails validation, so a deployment with a limit of zero does not
/// start rather than refusing every replay.
/// </summary>
public sealed class CallbackReplayOptionsTests
{
    [Theory]
    [Trait("TestId", "UT-CB-REPLAY-AGE-01")]
    [InlineData(1, true)]
    [InlineData(7, true)]
    [InlineData(30, true)]
    [InlineData(0, false)]
    [InlineData(31, false)]
    [InlineData(-7, false)]
    public void TheLimitIsOneToThirtyDays(int days, bool valid)
    {
        Assert.Equal(7, new CallbackReplayOptions().MaxAgeDays);
        Assert.Equal(TimeSpan.FromDays(7), new CallbackReplayOptions().MaxAge);

        var validator = new CallbackReplayOptionsValidator();
        Assert.Equal(valid, validator.Validate(null, new CallbackReplayOptions { MaxAgeDays = days }).Succeeded);
    }
}
