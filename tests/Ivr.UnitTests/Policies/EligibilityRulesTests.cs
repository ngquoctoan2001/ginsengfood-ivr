using Ivr.Domain.Policies;

namespace Ivr.UnitTests.Policies;

public sealed class EligibilityRulesTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 7, 0, 0, TimeSpan.Zero);

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
    [Trait("TestId", "UT-M3-AUTHORITY-01")]
    [Trait("TestId", "UT-M3-AUTHORITY-02")]
    public void M3AuthorityIsNotRepresentedByAnIvrTrustPredicate()
    {
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(CreateSnapshot());

        Assert.True(evaluation.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, evaluation.Decision);
        Assert.Empty(evaluation.Advisories);
        Assert.Null(typeof(EligibilitySnapshot).GetProperty("Trust"));
        Assert.Null(typeof(EligibilitySnapshot).Assembly.GetType(
            "Ivr.Domain.Policies.TrustResolverEvidence"));
    }

    [Fact]
    [Trait("TestId", "UT-M3-AUTHORITY-10")]
    public void TheActiveDecisionVocabularyHasNoBusinessSkipAndEvaluateNeverLeavesIt()
    {
        // W-0124 F5 closes a gap in UT-M3-AUTHORITY-02: that test proves the OLD trust types are
        // gone, so it only catches a re-introduction that rebuilds them by name. This one is
        // shape-independent — it pins the vocabulary itself. A new decision constant, or an
        // Evaluate path returning anything outside the active set, fails here no matter how the
        // re-introduction is spelled.
        // What Evaluate is allowed to RETURN. Pending is the pre-evaluation state a task carries
        // before this method runs, so it belongs to the vocabulary but never to an outcome.
        string[] active =
        [
            EligibilityDecisions.Eligible,
            EligibilityDecisions.BlockedOperational,
            EligibilityDecisions.HeldAdminReview,
            EligibilityDecisions.CapacityException,
        ];
        string[] vocabulary = [EligibilityDecisions.Pending, .. active];

        string[] declared = [.. typeof(EligibilityDecisions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal)];

        Assert.Equal([.. vocabulary.Order(StringComparer.Ordinal)], declared);

        // Every axis the domain can still see, crossed. Business selection is upstream in M3, so
        // no combination of what IVR does see may produce a fifth decision.
        var observed = new HashSet<string>(StringComparer.Ordinal);
        foreach (string orderState in new[] { "CONFIRMING", "QUOTE", "PAID" })
        {
            foreach (bool required in new[] { true, false })
            {
                foreach (bool evidenceAvailable in new[] { true, false })
                {
                    foreach (string? sourceDecision in new[] { "ELIGIBLE", "BLOCKED", null })
                    {
                        foreach (bool? restriction in new bool?[] { false, true, null })
                        {
                            foreach (bool capacityOk in new[] { true, false })
                            {
                                observed.Add(EligibilityRules.Evaluate(CreateSnapshot(
                                    phoneCallRestriction: restriction,
                                    evidenceAvailable: evidenceAvailable,
                                    sourceEligibilityDecision: sourceDecision,
                                    orderState: orderState,
                                    ivrConfirmationRequired: required,
                                    capacityCanMeetDeadline: capacityOk)).Decision);
                            }
                        }
                    }
                }
            }
        }

        Assert.All(observed, decision => Assert.Contains(decision, active));

        // Not vacuous: the matrix must actually reach the eligible branch and at least one
        // fail-closed branch, otherwise a rule that blocked everything would also pass.
        Assert.Contains(EligibilityDecisions.Eligible, observed);
        Assert.Contains(EligibilityDecisions.BlockedOperational, observed);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-FAILCLOSED-04")]
    [Trait("TestId", "UT-M3-AUTHORITY-04")]
    public void MissingRequiredSourcesFailClosed()
    {
        EligibilityEvaluation missingRestriction = EligibilityRules.Evaluate(
            CreateSnapshot(phoneCallRestriction: null));
        EligibilityEvaluation missingEvidence = EligibilityRules.Evaluate(
            CreateSnapshot(evidenceAvailable: false));
        EligibilityEvaluation missingEligibility = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibilityDecision: null));
        EligibilityEvaluation blockedEligibility = EligibilityRules.Evaluate(
            CreateSnapshot(sourceEligibilityDecision: "BLOCKED"));

        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestrictionMissing,
            Assert.Single(missingRestriction.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EvidenceMissing,
            Assert.Single(missingEvidence.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotMissing,
            Assert.Single(missingEligibility.Reasons).Code);
        Assert.Equal(
            EligibilityReasonCodes.EligibilitySnapshotBlocked,
            Assert.Single(blockedEligibility.Reasons).Code);
        Assert.All(
            new[]
            {
                missingRestriction,
                missingEvidence,
                missingEligibility,
                blockedEligibility,
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

    private static EligibilitySnapshot CreateSnapshot(
        bool? phoneCallRestriction = false,
        bool evidenceAvailable = true,
        string? sourceEligibilityDecision = "ELIGIBLE",
        EligibilityEvidence? sourceEligibility = null,
        VoiceContactEvidence? voiceContact = null,
        // W-0124: only the decision-vocabulary matrix varies these. Defaults keep every existing
        // assertion on the same fixture it was written against.
        string orderState = "CONFIRMING",
        bool ivrConfirmationRequired = true,
        bool capacityCanMeetDeadline = true)
    {
        // Default evidence is structurally valid so existing assertions still exercise the
        // decision rules rather than tripping on the W-0030 structural checks that run first.
        EligibilityEvidence resolvedEvidence = sourceEligibility ?? Evidence(
            decision: sourceEligibilityDecision);

        return new EligibilitySnapshot(
            orderState,
            "GOLDEN_HOUR",
            "ONLINE",
            ivrConfirmationRequired,
            true,
            resolvedEvidence,
            voiceContact ?? new VoiceContactEvidence(
                phoneCallRestriction,
                true,
                "sales-voice-v1"),
            "VALID",
            Now.AddMinutes(3),
            Now.AddMinutes(-2),
            Now.AddMinutes(3),
            new EligibilityCapacitySnapshot(
                true,
                capacityCanMeetDeadline,
                "UNIT-CAPACITY",
                1,
                0,
                0,
                0,
                capacityCanMeetDeadline ? null : "CAPACITY_DEADLINE_UNREACHABLE",
                "evidence://unit/p2-2/capacity"),
            evidenceAvailable,
            "evidence://unit/p2-2/task",
            Now);
    }


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
