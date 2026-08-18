using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Domain.Ports;

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
