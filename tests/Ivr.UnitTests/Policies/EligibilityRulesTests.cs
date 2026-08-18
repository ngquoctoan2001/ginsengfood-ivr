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
    public void PhoneCallRestrictionBlocksAndMarketingConsentCannotReachTheVoiceDecision()
    {
        EligibilityEvaluation restricted = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: true));
        EligibilityEvaluation allowed = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: false));

        Assert.False(restricted.Eligible);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestricted,
            Assert.Single(restricted.Reasons).Code);
        Assert.True(allowed.Eligible);

        // W-0031 / P4-3 §2.2. This used to be proved by a rule choosing not to read an SMS
        // opt-out field. A rule can be edited by someone who does not know why; a field that does
        // not exist cannot be read. The voice decision type must carry no marketing-consent
        // member at all — a customer who declined marketing has not declined a transactional
        // order-confirmation call, and the two rest on different legal bases.
        string[] forbidden = ["sms", "marketing", "email", "consent", "newsletter", "promo"];
        foreach (
            System.Reflection.PropertyInfo property in typeof(VoiceContactEvidence).GetProperties())
        {
            foreach (string term in forbidden)
            {
                Assert.DoesNotContain(term, property.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
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

    [Fact]
    [Trait("TestId", "UT-ELIG-EVIDENCE-10")]
    public void StructurallyInvalidEligibilityEvidenceFailsClosedWithItsOwnReasonCode()
    {
        EligibilityEvaluation unreadable = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: EligibilityEvidence.Malformed(SnapshotHash)));
        EligibilityEvaluation absent = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: EligibilityEvidence.Absent));
        EligibilityEvaluation sourceDown = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(sourceAvailable: false)));
        EligibilityEvaluation noVersion = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(sourceVersion: "   ")));

        Assert.Equal(EligibilityDecisions.HeldAdminReview, unreadable.Decision);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotUnreadable,
            Assert.Single(unreadable.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotMissing,
            Assert.Single(absent.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySourceUnavailable,
            Assert.Single(sourceDown.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySourceVersionMissing,
            Assert.Single(noVersion.Reasons).Code);

        // Not one of these may dispatch, and none may count a customer attempt.
        foreach (EligibilityEvaluation evaluation in
            new[] { unreadable, absent, sourceDown, noVersion })
        {
            Assert.False(evaluation.Eligible);
            Assert.False(evaluation.IsCountedCustomerAttempt);
        }
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-EVIDENCE-11")]
    public void StaleOrUnstampedEligibilityEvidenceIsHeldNotDispatched()
    {
        EligibilityEvaluation unstamped = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(capturedAtMissing: true)));
        // Captured before the confirmation window opened: describes a different order state.
        EligibilityEvaluation beforeWindow = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(capturedAt: Now.AddMinutes(-10))));
        // Captured after evaluation: a clock or producer defect, never a fresher truth.
        EligibilityEvaluation future = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(capturedAt: Now.AddMinutes(1))));

        foreach (EligibilityEvaluation evaluation in new[] { unstamped, beforeWindow, future })
        {
            Assert.False(evaluation.Eligible);
            Assert.Equal(EligibilityDecisions.HeldAdminReview, evaluation.Decision);
            Assert.Equal(
                EligibilityReasonCodes.EligibilitySnapshotStale,
                Assert.Single(evaluation.Reasons).Code);
        }
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-EVIDENCE-12")]
    public void EligibleVerdictCarryingBlockersIsStillBlocked()
    {
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(blockers: ["RECALL_HOLD"])));

        // Sales said ELIGIBLE in the summary field and listed a blocker in the detail.
        // Believing the summary would place a call on an order Sales itself flagged.
        Assert.False(evaluation.Eligible);
        Assert.Equal(EligibilityDecisions.BlockedOperational, evaluation.Decision);
        EligibilityReason reason = Assert.Single(evaluation.Reasons);
        Assert.Equal(EligibilityReasonCodes.EligibilitySnapshotBlocked, reason.Code);
        Assert.Equal("eligibility.snapshot.blockers", reason.Signal);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-EVIDENCE-13")]
    public void StructuralFailuresAreReportedBeforeContentFailures()
    {
        // Unreadable AND missing a version AND stale. The reported code must be the structural
        // one: a snapshot IVR could not read must never surface as a decision it disagrees with.
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: new EligibilityEvidence(
                EligibilityEvidenceState.Unreadable,
                "BLOCKED",
                null,
                null,
                false,
                ["RECALL_HOLD"],
                SnapshotHash)));

        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotUnreadable,
            Assert.Single(evaluation.Reasons).Code);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-EVIDENCE-14")]
    public void EligibleEvaluationCarriesTheSnapshotHashAsEvidenceNeverTheSnapshotBody()
    {
        EligibilityEvaluation withHash = EligibilityRules.Evaluate(CreateSnapshot());
        EligibilityEvaluation withoutHash = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibility: Evidence(snapshotHash: null)));

        Assert.True(withHash.Eligible);
        Assert.Contains(
            withHash.EvidenceRefs,
            reference => reference.EndsWith(
                string.Concat("#eligibility/snapshot/", SnapshotHash),
                StringComparison.Ordinal));

        // No hash recorded (rows written before the P4-2 migration) still evaluates; it simply
        // cannot offer the traceability ref. Absence of the digest is not a reason to block.
        Assert.True(withoutHash.Eligible);
        Assert.DoesNotContain(
            withoutHash.EvidenceRefs,
            reference => reference.Contains("snapshot/", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-VOICE-15")]
    public void UnknownOrUnavailableVoiceDecisionBlocksInsteadOfDefaultingToAllowed()
    {
        EligibilityEvaluation sourceDown = EligibilityRules.Evaluate(
            CreateSnapshot(voiceContact: new VoiceContactEvidence(false, false, "sales-voice-v1")));
        EligibilityEvaluation unknown = EligibilityRules.Evaluate(
            CreateSnapshot(voiceContact: VoiceContactEvidence.Unknown));

        // "The resolver could not answer" is not permission, and neither is "nobody said".
        Assert.False(sourceDown.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, sourceDown.Decision);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestrictionSourceUnavailable,
            Assert.Single(sourceDown.Reasons).Code);

        Assert.False(unknown.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, unknown.Decision);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestrictionMissing,
            Assert.Single(unknown.Reasons).Code);

        Assert.False(sourceDown.IsCountedCustomerAttempt);
        Assert.False(unknown.IsCountedCustomerAttempt);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-TRUST-16")]
    public void IncompleteTrustEvidenceRequiresTheCallAndSaysWhichPartWasMissing()
    {
        TrustResolverEvidence complete = new(
            SkipFeatureEnabled: true,
            SkipAllowedBySales: true,
            ResolverAvailable: true,
            ResolverVersion: "sales-trust-v1",
            TrustStatus: "TRUSTED",
            RiskEvidenceAvailable: true,
            RiskFlags: []);

        Assert.True(complete.CanSkip);
        Assert.Equal(
            EligibilityDecisions.SkippedTrustedCustomer,
            EligibilityRules.Evaluate(CreateSnapshot(trust: complete)).Decision);

        (TrustResolverEvidence Trust, string Advisory)[] cases =
        [
            (complete with { SkipFeatureEnabled = false },
                EligibilityReasonCodes.TrustSkipDisabledRequireIvr),
            (complete with { ResolverAvailable = false },
                EligibilityReasonCodes.TrustResolverUnavailable),
            (complete with { ResolverVersion = "  " },
                EligibilityReasonCodes.TrustResolverVersionMissing),
            (complete with { RiskEvidenceAvailable = false },
                EligibilityReasonCodes.TrustRiskEvidenceUnavailable),
        ];

        foreach ((TrustResolverEvidence trust, string advisory) in cases)
        {
            Assert.False(trust.CanSkip);
            EligibilityEvaluation evaluation = EligibilityRules.Evaluate(
                CreateSnapshot(trust: trust));

            // Missing trust evidence must never block — it must make the call happen.
            Assert.True(evaluation.Eligible);
            Assert.Equal(EligibilityDecisions.Eligible, evaluation.Decision);
            Assert.Contains(advisory, evaluation.Advisories);
        }

        // A risk flag alone also cancels the skip, without needing any part to be missing.
        Assert.False((complete with { RiskFlags = ["COD_FAIL_HISTORY"] }).CanSkip);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-TRUST-17")]
    public void VoiceEvidenceAndTrustEvidenceFailClosedInOppositeDirections()
    {
        // The whole point of separating these two types. Both are fail-closed, but "closed"
        // means the opposite thing for each, because the harm is asymmetric: not knowing whether
        // we may call must not produce a call; not knowing whether we may skip must not produce
        // a skip. Collapsing them into one flag would silently pick one harm over the other.
        EligibilityEvaluation voiceUnknown = EligibilityRules.Evaluate(
            CreateSnapshot(voiceContact: VoiceContactEvidence.Unknown));
        EligibilityEvaluation trustUnknown = EligibilityRules.Evaluate(
            CreateSnapshot(trust: TrustResolverEvidence.RequireIvr with
            {
                TrustStatus = "TRUSTED",
            }));

        Assert.False(voiceUnknown.Eligible);
        Assert.True(trustUnknown.Eligible);
        Assert.NotEqual(
            EligibilityDecisions.SkippedTrustedCustomer,
            trustUnknown.Decision);
    }

    private static EligibilitySnapshot CreateSnapshot(
        IReadOnlyList<EligibilitySellableLine>? sellableLines = null,
        bool useDefaultSellable = true,
        bool? phoneCallRestriction = false,
        string? customerTrustStatus = null,
        bool trustedSkipAllowed = false,
        bool trustSkipFeatureEnabled = false,
        bool evidenceAvailable = true,
        string? sourceEligibilityDecision = "ELIGIBLE",
        EligibilityEvidence? sourceEligibility = null,
        VoiceContactEvidence? voiceContact = null,
        TrustResolverEvidence? trust = null)
    {
        IReadOnlyList<EligibilitySellableLine>? resolvedSellable = sellableLines;
        if (sellableLines is null && useDefaultSellable)
        {
            resolvedSellable = [Sellable(EligibilitySellableDecision.Sellable)];
        }

        // Default evidence is structurally valid so existing assertions still exercise the
        // decision rules rather than tripping on the W-0030 structural checks that run first.
        EligibilityEvidence resolvedEvidence = sourceEligibility ?? Evidence(
            decision: sourceEligibilityDecision);

        return new EligibilitySnapshot(
            "CONFIRMING",
            "GOLDEN_HOUR",
            "ONLINE",
            true,
            true,
            resolvedEvidence,
            resolvedSellable,
            voiceContact ?? new VoiceContactEvidence(
                phoneCallRestriction,
                true,
                "sales-voice-v1"),
            "VALID",
            Now.AddMinutes(3),
            Now.AddMinutes(-2),
            Now.AddMinutes(3),
            trust ?? new TrustResolverEvidence(
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

    private static EligibilityEvidence Evidence(
        EligibilityEvidenceState state = EligibilityEvidenceState.Present,
        string? decision = "ELIGIBLE",
        string? sourceVersion = "sales-elig-v1",
        DateTimeOffset? capturedAt = null,
        bool capturedAtMissing = false,
        bool sourceAvailable = true,
        IReadOnlyList<string>? blockers = null,
        string? snapshotHash = SnapshotHash) => new(
            state,
            decision,
            sourceVersion,
            // capturedAt: null means "use the valid default"; an absent stamp needs its own flag,
            // otherwise the omitted-stamp case silently gets a fresh one and proves nothing.
            capturedAtMissing ? null : capturedAt ?? Now.AddMinutes(-1),
            sourceAvailable,
            blockers ?? [],
            snapshotHash);

    private const string SnapshotHash =
        "1111111111111111111111111111111111111111111111111111111111111111";
}
