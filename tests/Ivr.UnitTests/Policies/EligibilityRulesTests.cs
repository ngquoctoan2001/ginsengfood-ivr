using Ivr.Domain.Policies;

namespace Ivr.UnitTests.Policies;

public sealed class EligibilityRulesTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-ELIG-BLOCK-01")]
    public void NotSellableLineBlocksWhileAllSellableLinesAreEligible()
    {
        EligibilityEvaluation blocked = EligibilityRules.Evaluate(
            CreateSnapshot(sellableLines:
            [
                Sellable(EligibilitySellableDecision.NotSellable),
            ]));
        EligibilityEvaluation eligible = EligibilityRules.Evaluate(
            CreateSnapshot(sellableLines:
            [
                Sellable(EligibilitySellableDecision.Sellable),
                Sellable(EligibilitySellableDecision.Sellable),
            ]));

        Assert.False(blocked.Eligible);
        Assert.Equal(EligibilityDecisions.BlockedOperational, blocked.Decision);
        Assert.Equal(
            EligibilityReasonCodes.InventoryNotSellable,
            Assert.Single(blocked.Reasons).Code);
        Assert.True(eligible.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, eligible.Decision);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-DNC-02")]
    public void PhoneCallRestrictionBlocksButSmsOptOutAloneDoesNotBlockVoice()
    {
        EligibilityEvaluation restricted = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: true, smsOptOut: false));
        EligibilityEvaluation smsOnly = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: false, smsOptOut: true));

        Assert.False(restricted.Eligible);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestricted,
            Assert.Single(restricted.Reasons).Code);
        Assert.True(smsOnly.Eligible);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-TRUST-03")]
    public void DisabledTrustSkipStillRequiresIvrForTrustedCustomer()
    {
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
            CreateSnapshot(
                customerTrustStatus: "TRUSTED",
                trustedSkipAllowed: true,
                trustSkipFeatureEnabled: false));

        Assert.True(evaluation.Eligible);
        Assert.False(evaluation.TrustedCustomerSkipped);
        Assert.Contains(
            EligibilityReasonCodes.TrustSkipDisabledRequireIvr,
            evaluation.Advisories);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-FAILCLOSED-04")]
    public void MissingRequiredSourcesAndUnknownSellableStateFailClosed()
    {
        EligibilityEvaluation missingSellable = EligibilityRules.Evaluate(
            CreateSnapshot(sellableLines: null, useDefaultSellable: false));
        EligibilityEvaluation missingRestriction = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: null));
        EligibilityEvaluation unknownSellable = EligibilityRules.Evaluate(
            CreateSnapshot(sellableLines:
            [
                Sellable(EligibilitySellableDecision.Unknown),
            ]));
        EligibilityEvaluation missingEvidence = EligibilityRules.Evaluate(
            CreateSnapshot(evidenceAvailable: false));
        EligibilityEvaluation missingEligibility = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibilityDecision: null));
        EligibilityEvaluation blockedEligibility = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibilityDecision: "BLOCKED"));
        EligibilityEvaluation undefinedSellable = EligibilityRules.Evaluate(
            CreateSnapshot(sellableLines:
            [
                Sellable((EligibilitySellableDecision)999),
            ]));

        Assert.Equal(
            EligibilityDecisions.HeldAdminReview,
            missingSellable.Decision);
        Assert.Equal(
            EligibilityReasonCodes.SellableSnapshotMissing,
            Assert.Single(missingSellable.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestrictionMissing,
            Assert.Single(missingRestriction.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.SellableUnknown,
            Assert.Single(unknownSellable.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EvidenceMissing,
            Assert.Single(missingEvidence.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotMissing,
            Assert.Single(missingEligibility.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotBlocked,
            Assert.Single(blockedEligibility.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.SellableUnknown,
            Assert.Single(undefinedSellable.Reasons).Code);
        Assert.All(
            new[]
            {
                missingSellable,
                missingRestriction,
                unknownSellable,
                missingEvidence,
                missingEligibility,
                blockedEligibility,
                undefinedSellable,
            },
            evaluation => Assert.False(evaluation.IsCountedCustomerAttempt));
    }

    private static EligibilitySnapshot CreateSnapshot(
        IReadOnlyList<EligibilitySellableLine>? sellableLines = null,
        bool useDefaultSellable = true,
        bool? phoneCallRestriction = false,
        bool smsOptOut = false,
        string? customerTrustStatus = null,
        bool trustedSkipAllowed = false,
        bool trustSkipFeatureEnabled = false,
        bool evidenceAvailable = true,
        string? sourceEligibilityDecision = "ELIGIBLE")
    {
        IReadOnlyList<EligibilitySellableLine>? resolvedSellable = sellableLines;
        if (sellableLines is null && useDefaultSellable)
        {
            resolvedSellable = [Sellable(EligibilitySellableDecision.Sellable)];
        }

        return new EligibilitySnapshot(
            "CONFIRMING",
            "GOLDEN_HOUR",
            "ONLINE",
            true,
            true,
            sourceEligibilityDecision,
            resolvedSellable,
            phoneCallRestriction,
            smsOptOut,
            "VALID",
            Now.AddMinutes(3),
            Now.AddMinutes(-2),
            Now.AddMinutes(3),
            customerTrustStatus,
            trustedSkipAllowed,
            [],
            trustSkipFeatureEnabled,
            new EligibilityCapacitySnapshot(
                true,
                true,
                "UNIT-CAPACITY",
                1,
                0,
                0,
                0,
                null,
                "evidence://unit/p2-2/capacity"),
            evidenceAvailable,
            "evidence://unit/p2-2/task",
            Now);
    }

    private static EligibilitySellableLine Sellable(
        EligibilitySellableDecision decision) => new(
            decision,
            false,
            false,
            false,
            true,
            true,
            true,
            Now.AddMinutes(-1));
}
