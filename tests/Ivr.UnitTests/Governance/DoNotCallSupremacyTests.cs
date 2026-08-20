using Ivr.Domain.Policies;

namespace Ivr.UnitTests.Governance;

/// <summary>
/// W-0052 / P10-1 §8 <c>COMP-DNC-03</c>.
///
/// <para><c>UT-ELIG-VOICE-15</c> already covers the two <i>uncertain</i> cases —
/// resolver down, and nobody said. This covers the certain one, and it covers it
/// as a property rather than as an example: when the resolver says the customer
/// must not be called, <b>no combination of other signals</b> produces a callable
/// decision.</para>
///
/// <para>That is the shape the legal basis requires. The confirmation call rests on
/// performing a contract, not on a balanced legitimate interest, so a do-not-call
/// flag is a hard block and never an input to a trade-off. An implementation where
/// a sufficiently favourable set of other signals could outweigh it would be a
/// different legal position than the one <c>docs/compliance/data-inventory.md</c>
/// states.</para>
/// </summary>
public sealed class DoNotCallSupremacyTests
{
    [Theory]
    [Trait("TestId", "COMP-DNC-03")]
    // Every other signal set as favourably as it can be, one axis at a time.
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void ARestrictedCustomerIsNeverCallableWhateverElseIsTrue(
        bool trustedCustomer,
        bool trustSkipEnabled,
        bool trustedSkipAllowed)
    {
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
            DoNotCallSnapshotFactory.Create(
                phoneCallRestriction: true,
                customerTrustStatus: trustedCustomer ? "TRUSTED" : null,
                trustSkipFeatureEnabled: trustSkipEnabled,
                trustedSkipAllowed: trustedSkipAllowed));

        Assert.False(evaluation.Eligible);
        Assert.Equal(EligibilityDecisions.BlockedOperational, evaluation.Decision);
        Assert.Contains(
            evaluation.Reasons,
            reason => reason.Code == EligibilityReasonCodes.PhoneCallRestricted);
    }

    [Fact]
    [Trait("TestId", "COMP-DNC-03")]
    public void TheControlCaseIsCallableSoTheBlockAboveIsNotVacuous()
    {
        // Same factory, restriction off. Without this the theory above would pass on a snapshot
        // that was unusable for some unrelated reason, and prove nothing about do-not-call.
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
            DoNotCallSnapshotFactory.Create(phoneCallRestriction: false));

        Assert.True(
            evaluation.Eligible,
            $"the control snapshot was not callable: {string.Join(", ", evaluation.Reasons.Select(r => r.Code))}");
    }

    [Fact]
    [Trait("TestId", "COMP-DNC-03")]
    public void BlockedOperationalIsNotAnEligibleDecision()
    {
        // The decision constant itself, asserted once. If BlockedOperational ever became a
        // callable decision, every test above would still pass while the system called restricted
        // customers.
        EligibilityEvaluation restricted = EligibilityRules.Evaluate(
            DoNotCallSnapshotFactory.Create(phoneCallRestriction: true));

        // Neither of the two decisions that lead to a dial: Eligible, and the trust skip that
        // closes the task without calling. The second matters because a "skip" that suppressed the
        // restriction would also suppress the audit trail explaining why nobody was called.
        Assert.NotEqual(EligibilityDecisions.Eligible, restricted.Decision);
        Assert.NotEqual(EligibilityDecisions.SkippedTrustedCustomer, restricted.Decision);
    }
}

/// <summary>
/// Minimal snapshot builder for the do-not-call property. Deliberately separate from the
/// P2-2 test helper: this one exists to hold every other signal at its most permissive so
/// the restriction is the only thing that can be doing the blocking.
/// </summary>
internal static class DoNotCallSnapshotFactory
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);

    private const string SnapshotHash =
        "1111111111111111111111111111111111111111111111111111111111111111";

    public static EligibilitySnapshot Create(
        bool? phoneCallRestriction,
        string? customerTrustStatus = null,
        bool trustSkipFeatureEnabled = false,
        bool trustedSkipAllowed = false) => new(
            "CONFIRMING",
            "GOLDEN_HOUR",
            "ONLINE",
            true,
            true,
            new EligibilityEvidence(
                EligibilityEvidenceState.Present,
                "ELIGIBLE",
                "sales-elig-v1",
                Now.AddMinutes(-1),
                true,
                [],
                SnapshotHash),
            [
                new EligibilitySellableLine(
                    EligibilitySellableDecision.Sellable,
                    false,
                    false,
                    false,
                    true,
                    true,
                    true,
                    Now.AddMinutes(-1)),
            ],
            new VoiceContactEvidence(phoneCallRestriction, true, "sales-voice-v1"),
            "VALID",
            Now.AddMinutes(3),
            Now.AddMinutes(-2),
            Now.AddMinutes(3),
            new TrustResolverEvidence(
                trustSkipFeatureEnabled,
                trustedSkipAllowed,
                trustSkipFeatureEnabled,
                trustSkipFeatureEnabled ? "sales-trust-v1" : null,
                customerTrustStatus,
                trustSkipFeatureEnabled,
                []),
            new EligibilityCapacitySnapshot(
                true,
                true,
                "COMP-CAPACITY",
                1,
                0,
                0,
                0,
                null,
                "evidence://compliance/p10-1/capacity"),
            true,
            "evidence://compliance/p10-1/task",
            Now);
}
