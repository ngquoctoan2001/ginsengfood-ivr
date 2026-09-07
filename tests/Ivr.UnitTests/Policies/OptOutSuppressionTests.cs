using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;

namespace Ivr.UnitTests.Policies;

/// <summary>
/// W-0034 / P4-6. The opt-out loop is the one place IVR speaks about a customer's future
/// contactability, so every test here is really asking the same question: can a customer end up
/// suppressed without having asked to be?
/// </summary>
public sealed class OptOutSuppressionTests
{
    [Fact]
    [Trait("TestId", "UT-OPTOUT-CAP-01")]
    public void ARejectedCallIsCapturedForReviewAndCountedButIsNeverACancellation()
    {
        DateTimeOffset now = new(2026, 8, 18, 6, 0, 0, TimeSpan.Zero);
        var context = new AttemptNormalizationContext(
            AttemptNumber: 1,
            MaxAttempts: 2,
            OccurredAt: now,
            ConfirmationWindowExpiresAt: now.AddMinutes(5),
            PriorTechnicalRetryCount: 0,
            TechnicalRetryLimit: 2);

        NormalizedResult rejected = DispositionMapper.Normalize(
            SimProviderDisposition.Rejected,
            rawDtmf: null,
            technicalErrorCode: null,
            context);

        // DT-02. Declining a call is not cancelling an order: reading it as a cancellation would
        // cancel orders customers never asked to cancel.
        Assert.NotEqual(IvrResultType.IvrCustomerCancelled, rejected.ResultType);
        Assert.Equal(IvrResultType.IvrNoAnswerAttempt, rejected.ResultType);

        // It still counts as a customer attempt, and it raises the review flag that feeds the
        // opt-out queue — that flag is the whole capture mechanism.
        Assert.True(rejected.IsCounted);
        Assert.True(rejected.HumanReviewRequired);
        Assert.Equal(OptOutReasonCodes.CallRejected, rejected.Reason);
    }

    [Fact]
    [Trait("TestId", "UT-OPTOUT-THRESH-02")]
    public void OneSignalNeverProposesAndNoConfigurationCanLowerThatFloor()
    {
        OptOutThresholdPolicy policy = OptOutThresholdPolicy.Default;

        SuppressionDecision single = OptOutSuppressionPolicy.Decide(1, policy, adminConfirmed: false);
        Assert.Equal(SuppressionOutcome.Hold, single.Outcome);
        Assert.Equal(OptOutReasonCodes.SingleSignalNeverProposes, single.ReasonCode);

        // Even an administrator cannot act on a single declined call: confirming one refusal is
        // confirming an inference, not a request the customer made.
        SuppressionDecision singleConfirmed =
            OptOutSuppressionPolicy.Decide(1, policy, adminConfirmed: true);
        Assert.Equal(SuppressionOutcome.Hold, singleConfirmed.Outcome);

        SuppressionDecision below = OptOutSuppressionPolicy.Decide(2, policy, adminConfirmed: false);
        Assert.Equal(SuppressionOutcome.Hold, below.Outcome);
        Assert.Equal(OptOutReasonCodes.BelowThreshold, below.ReasonCode);

        SuppressionDecision reached =
            OptOutSuppressionPolicy.Decide(3, policy, adminConfirmed: false);
        Assert.Equal(SuppressionOutcome.Propose, reached.Outcome);
        Assert.Equal(OptOutReasonCodes.ThresholdReached, reached.ReasonCode);

        // An admin who reviewed the case may act before the threshold, and the decision records
        // that it was a human — that distinction is what makes the audit trail worth having.
        SuppressionDecision confirmed =
            OptOutSuppressionPolicy.Decide(2, policy, adminConfirmed: true);
        Assert.Equal(SuppressionOutcome.Propose, confirmed.Outcome);
        Assert.Equal(OptOutReasonCodes.AdminConfirmed, confirmed.ReasonCode);
        Assert.True(confirmed.AdminConfirmed);

        // The floor is code, not configuration.
        Assert.Throws<InvalidOperationException>(() =>
            OptOutSuppressionPolicy.Decide(5, new OptOutThresholdPolicy(1), adminConfirmed: false));
    }

    /// <summary>
    /// Pressing 0 cancels one order. It is not a request to stop being called, and nothing in the
    /// runtime treats it as one.
    /// <para>
    /// This test exists because a document said otherwise. <c>OD-V1-23</c>, signed 2026-09-06 as an
    /// owner position and still short of quorum, names "DTMF-0 / handoff" as the explicit opt-out
    /// signal. Both halves are wrong against this codebase. Key 0 is the cancel key: the only
    /// wording a script may carry says <i>"bấm phím 0 để hủy"</i> - press 0 to cancel - and
    /// <c>TargetV1SpeechPolicy</c> refuses any template that drops it. Key 9 and human handoff are
    /// out of scope entirely (<c>specs/01-context-and-scope.md</c>), and the same policy refuses a
    /// template that so much as mentions "phím 9".
    /// </para>
    /// <para>
    /// M8-08 §4.2 - the pack OD-V1-23 itself names as its closure evidence - states the rule this
    /// test enforces: DTMF 1 confirms, DTMF 0 cancels, and neither key may be reused for opt-out.
    /// Reading a cancellation as a contact ban would take consent the customer never gave, from a
    /// word they were never told. W-0210.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-OPTOUT-DTMF0-05")]
    public void PressingZeroCancelsAnOrderAndSaysNothingAboutFutureContactability()
    {
        DateTimeOffset now = new(2026, 8, 18, 6, 0, 0, TimeSpan.Zero);
        var context = new AttemptNormalizationContext(
            AttemptNumber: 1,
            MaxAttempts: 2,
            OccurredAt: now,
            ConfirmationWindowExpiresAt: now.AddMinutes(5),
            PriorTechnicalRetryCount: 0,
            TechnicalRetryLimit: 2);

        NormalizedResult cancelled = DispositionMapper.Normalize(
            SimProviderDisposition.Answered,
            rawDtmf: "0",
            technicalErrorCode: null,
            context);

        // What key 0 means, and the only thing it means.
        Assert.Equal(IvrResultType.IvrCustomerCancelled, cancelled.ResultType);
        Assert.Equal(
            CoreActionRecommendation.RevalidateAndCancelCustomerRequest,
            cancelled.RecommendedCoreAction);

        // It is not the weak opt-out signal either. That reason code belongs to a declined call,
        // and a cancellation is not a declined call.
        Assert.NotEqual(OptOutReasonCodes.CallRejected, cancelled.Reason);
        Assert.False(cancelled.HumanReviewRequired);

        // The script the customer heard offered exactly two keys, and 0 was labelled "hủy".
        Assert.Contains(
            "bấm phím 0 để hủy",
            TargetV1SpeechPolicy.LegacyVietnameseTemplate,
            StringComparison.OrdinalIgnoreCase);

        // The other signal OD-V1-23 names is not merely absent - a script mentioning it is refused.
        Assert.Throws<InvalidOperationException>(() =>
            TargetV1SpeechPolicy.ValidateTemplate(
                TargetV1SpeechPolicy.LegacyVietnameseTemplate
                    + " Bấm phím 9 để gặp nhân viên."));
    }

    [Fact]
    [Trait("TestId", "UT-OPTOUT-CHANNEL-04")]
    public void EveryDecisionIsAboutThePhoneChannelOnlyAndNeverSuppressesInsideIvr()
    {
        // DC-02. IVR observes voice calls, so voice is the only channel it can speak about.
        // The enum has exactly one member: there is no value that could name SMS or marketing.
        Assert.Equal([SuppressionChannel.PhoneCall], Enum.GetValues<SuppressionChannel>());

        foreach (int count in new[] { 0, 1, 2, 3, 10 })
        {
            foreach (bool admin in new[] { false, true })
            {
                SuppressionDecision decision = OptOutSuppressionPolicy.Decide(
                    count,
                    OptOutThresholdPolicy.Default,
                    admin);
                Assert.Equal(SuppressionChannel.PhoneCall, decision.Channel);
            }
        }

        // DO-CORR-2. Whatever the decision, IVR holds no registry of its own.
        Assert.False(SuppressionDecision.SuppressedLocally);
    }
}
