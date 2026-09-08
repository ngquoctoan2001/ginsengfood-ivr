using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// W-0198 / <c>OD-V1-16</c>. The hours a customer may be telephoned.
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
        int endMinute = (21 * 60) + 8,
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
    [InlineData(21, 0, true)]
    [InlineData(21, 7, true)]
    [InlineData(21, 8, false)]
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
        Assert.Equal((21 * 60) + 8, defaults.EndMinuteOfLocalDay);

        // W-0220. Eight minutes past nine is not a round number anyone would pick; it is the 24/7
        // second attempt of a task admitted at 21:00, rounded up to the next whole minute because
        // this field cannot hold 21:07:30. If someone "tidies" it back to 21:00, UT-SCH-WINDOW-09
        // fails and says which decision they just undid.
        Assert.Equal(
            TimeSpan.FromMinutes(defaults.EndMinuteOfLocalDay),
            new TimeSpan(21, 8, 0));
    }

    /// <summary>
    /// Two decisions signed on the same day, by the same signature, that were never multiplied
    /// together: the calling window closes at 21:00 (<c>OD-V1-16</c>) and a Golden Hour task gets
    /// a second attempt 150 seconds after the first (<c>OD-V1-08</c>). Their product is a cutoff
    /// nobody wrote down.
    /// <para>
    /// A task admitted after the cutoff schedules its second attempt past the window end, and the
    /// hour gate refuses to claim it. The customer is telephoned once instead of twice, and — this
    /// is the part that reaches Sales — the order ends as
    /// <c>IVR_CONFIRMATION_WINDOW_EXPIRED</c> rather than <c>IVR_NO_ANSWER_FINAL</c>, because
    /// finality comes from exhausting the attempts and the attempts were not exhausted. Identical
    /// customer behaviour, two different results, decided by the wall clock.
    /// </para>
    /// <para>
    /// W-0215 measured that cutoff at 20:57:30 and 20:52:30, against a window closing at 21:00.
    /// W-0220 is the owner answering it: the window now closes at 21:08, chosen so that the last
    /// task admitted at nine o'clock still gets its second call. This test went red at that change
    /// and was updated deliberately, which is the whole reason it derives the cutoff from the two
    /// signed sources instead of hard-coding it.
    /// </para>
    /// <para>
    /// It still does not assert the cutoff is <i>right</i> — only that it is where the signatures
    /// put it. Note that <c>LOCK-05</c>, the Golden Hour session said to run 20:15–21:00, has no
    /// source anywhere in this repository and was not the basis for the new value.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-WINDOW-09")]
    public void TheSignedPolicyAndTheSignedWindowProduceACutoffNobodyWroteDown()
    {
        AttemptPolicySnapshot goldenHour = SignedProductionAttemptPolicies.Create()
            .Single(policy => policy.Program == IvrProgramCode.GoldenHour);
        CallingWindowOptions window = new();

        // Derived from the two signatures, not typed in: the last attempt's offset, subtracted
        // from the minute the window stops accepting a dial.
        TimeSpan lastOffset = goldenHour.AttemptOffsets[goldenHour.AttemptOffsets.Count - 1];
        Assert.Equal(TimeSpan.FromSeconds(150), lastOffset);

        TimeSpan windowCloses = TimeSpan.FromMinutes(window.EndMinuteOfLocalDay);
        TimeSpan cutoff = windowCloses - lastOffset;

        Assert.Equal(new TimeSpan(21, 5, 30), cutoff);

        // At the cutoff the second attempt lands exactly on the end minute, and the gate is
        // exclusive of it, so it is refused.
        DateTimeOffset admittedAtCutoff = LocalVietnamAtSecond(21, 5, 30);
        Assert.True(Window().Evaluate(admittedAtCutoff).Open);
        Assert.False(Window().Evaluate(admittedAtCutoff + lastOffset).Open);

        // One second earlier and both attempts fit.
        DateTimeOffset admittedJustBefore = LocalVietnamAtSecond(21, 5, 29);
        Assert.True(Window().Evaluate(admittedJustBefore).Open);
        Assert.True(Window().Evaluate(admittedJustBefore + lastOffset).Open);

        // The confirmation window outliving the calling window is not itself the problem: expiry
        // is bookkeeping and runs at any hour. Only dialling stops.
        Assert.False(
            Window().Evaluate(
                admittedJustBefore + goldenHour.ConfirmationWindowDuration).Open);

        // The other programme has the earlier cutoff, which is the opposite of what its name
        // suggests. TWENTY_FOUR_SEVEN describes when Sales takes the order, not when IVR may
        // telephone: the window is not per-programme, and a 450-second second attempt has to
        // clear the same 21:00. So its last safe admission is 20:52:30, five minutes before
        // Golden Hour's.
        AttemptPolicySnapshot twentyFourSeven = SignedProductionAttemptPolicies.Create()
            .Single(policy => policy.Program == IvrProgramCode.TwentyFourSeven);
        TimeSpan lastOffset247 =
            twentyFourSeven.AttemptOffsets[twentyFourSeven.AttemptOffsets.Count - 1];

        Assert.Equal(TimeSpan.FromSeconds(450), lastOffset247);
        Assert.Equal(new TimeSpan(21, 0, 30), windowCloses - lastOffset247);

        // And this is what W-0220 bought: the last 24/7 order of the evening, taken on the stroke
        // of nine, still gets both calls. Under the old 21:00 window its second attempt landed at
        // 21:07:30 and was refused.
        DateTimeOffset onTheStrokeOfNine = LocalVietnamAtSecond(21, 0, 0);
        Assert.True(Window().Evaluate(onTheStrokeOfNine).Open);
        Assert.True(Window().Evaluate(onTheStrokeOfNine + lastOffset247).Open);
    }

    private static DateTimeOffset LocalVietnamAtSecond(int hour, int minute, int second) =>
        new DateTimeOffset(2026, 9, 5, hour, minute, second, TimeSpan.FromHours(7))
            .ToUniversalTime();
}
