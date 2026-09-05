using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// W-0197 / <c>OD-V1-16</c>. The hours a customer may be telephoned.
/// <para>
/// Before this existed the answer was "any", and it was an absence rather than a decision: a task
/// arriving at three in the morning was dispatched at three in the morning and nothing anywhere
/// had an opinion. These pin the boundary minutes, because a window that is right at noon and
/// wrong at 07:59 is the one that reaches a customer.
/// </para>
/// </summary>
public sealed class CallingWindowTests
{
    private static CallingWindow Window(
        int startMinute = 8 * 60,
        int endMinute = 21 * 60,
        bool enabled = true,
        int offsetMinutes = 420) =>
        new(Options.Create(new CallingWindowOptions
        {
            Enabled = enabled,
            UtcOffsetMinutes = offsetMinutes,
            StartMinuteOfLocalDay = startMinute,
            EndMinuteOfLocalDay = endMinute,
        }));

    /// <summary>Local wall-clock time in Vietnam, expressed as the UTC instant it happens at.</summary>
    private static DateTimeOffset LocalVietnam(int hour, int minute) =>
        new DateTimeOffset(2026, 9, 5, hour, minute, 0, TimeSpan.FromHours(7)).ToUniversalTime();

    [Theory]
    [InlineData(7, 59, false)]
    [InlineData(8, 0, true)]
    [InlineData(12, 30, true)]
    [InlineData(20, 59, true)]
    [InlineData(21, 0, false)]
    [InlineData(23, 30, false)]
    [InlineData(3, 0, false)]
    [Trait("TestId", "UT-SCH-WINDOW-01")]
    public void TheBoundaryMinutesAreExactlyWhereTheOwnerPutThem(int hour, int minute, bool open)
    {
        CallingWindowDecision decision = Window().Evaluate(LocalVietnam(hour, minute));

        Assert.Equal(open, decision.Open);
        Assert.Equal(hour, decision.LocalTime.Hour);
        Assert.Equal(minute, decision.LocalTime.Minute);
    }

    /// <summary>
    /// The decision is made against Vietnam local time, not against the server's. A worker running
    /// in a UTC container must reach the same answer as one running anywhere else, or "no calls
    /// after nine" would mean a different hour per deployment.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-02")]
    public void TheAnswerFollowsLocalTimeRatherThanTheServerClock()
    {
        // 22:00 UTC is 05:00 the next morning in Vietnam - closed - even though a server reading
        // its own clock would call it late evening.
        DateTimeOffset lateEveningUtc = new(2026, 9, 5, 22, 0, 0, TimeSpan.Zero);
        Assert.False(Window().Evaluate(lateEveningUtc).Open);

        // 04:00 UTC is 11:00 in Vietnam - open - even though a server reading its own clock would
        // call it the middle of the night.
        DateTimeOffset earlyMorningUtc = new(2026, 9, 5, 4, 0, 0, TimeSpan.Zero);
        Assert.True(Window().Evaluate(earlyMorningUtc).Open);
    }

    /// <summary>
    /// When the window is shut it says when it opens again, so an operator looking at a quiet
    /// queue at midnight can tell "waiting until 08:00" from "broken".
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-03")]
    public void AClosedWindowNamesWhenItOpensAgain()
    {
        CallingWindowDecision beforeOpening = Window().Evaluate(LocalVietnam(6, 0));
        Assert.False(beforeOpening.Open);
        Assert.Equal(LocalVietnam(8, 0), beforeOpening.OpensAt);

        // After close it is tomorrow morning, not this morning.
        CallingWindowDecision afterClosing = Window().Evaluate(LocalVietnam(22, 15));
        Assert.False(afterClosing.Open);
        Assert.Equal(LocalVietnam(8, 0).AddDays(1), afterClosing.OpensAt);

        Assert.Contains("opens", afterClosing.Describe(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Disabled means no hour restriction at all, and it is the setting a deployment that dials
    /// real customers must never be on. Asserted so that turning it off is a visible decision
    /// rather than something discovered from behaviour.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-04")]
    public void DisablingTheWindowRemovesTheHourRestrictionEntirely()
    {
        CallingWindow disabled = Window(enabled: false);

        Assert.True(disabled.Evaluate(LocalVietnam(3, 0)).Open);
        Assert.False(disabled.Enabled);
    }

    /// <summary>
    /// An inverted or empty window would stop every call while looking like configuration. It is
    /// refused at startup instead, so the failure is a deployment that does not start rather than
    /// a night nobody was called.
    /// </summary>
    [Theory]
    [InlineData(21 * 60, 8 * 60)]
    [InlineData(8 * 60, 8 * 60)]
    [Trait("TestId", "UT-SCH-WINDOW-05")]
    public void AnEmptyOrInvertedWindowIsRefusedAtStartup(int startMinute, int endMinute)
    {
        ValidateOptionsResult result = new CallingWindowOptionsValidator().Validate(
            null,
            new CallingWindowOptions
            {
                StartMinuteOfLocalDay = startMinute,
                EndMinuteOfLocalDay = endMinute,
            });

        Assert.True(result.Failed);
        Assert.Contains("end after it starts", result.FailureMessage, StringComparison.Ordinal);
    }

    /// <summary>
    /// The default is the signed one. A default that drifted from OD-V1-16 would be the kind of
    /// difference nobody notices until a customer is telephoned at seven in the morning.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-06")]
    public void TheShippedDefaultIsTheSignedWindow()
    {
        CallingWindowOptions defaults = new();

        Assert.True(defaults.Enabled);
        Assert.Equal(420, defaults.UtcOffsetMinutes);
        Assert.Equal(8 * 60, defaults.StartMinuteOfLocalDay);
        Assert.Equal(21 * 60, defaults.EndMinuteOfLocalDay);
    }
}
