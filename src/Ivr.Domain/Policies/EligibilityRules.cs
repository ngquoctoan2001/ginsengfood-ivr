namespace Ivr.Domain.Policies;

public static class EligibilityDecisions
{
    public const string Pending = "PENDING_ELIGIBILITY";
    public const string Eligible = "ELIGIBLE_FOR_IVR";
    public const string BlockedOperational = "TASK_BLOCKED_OPERATIONAL";
    public const string HeldAdminReview = "TASK_HELD_ADMIN_REVIEW";
    public const string SkippedTrustedCustomer = "TASK_SKIPPED_TRUSTED_CUSTOMER";
    public const string CapacityException = "IVR_CAPACITY_EXCEPTION";
}

public static class EligibilityReasonCodes
{
    public const string NotOfficialOrder = "NOT_OFFICIAL_ORDER";
    public const string OrderStateNotCallable = "ORDER_STATE_NOT_CALLABLE";
    public const string ProgramPaymentMatrixRejected = "PROGRAM_PAYMENT_MATRIX_REJECTED";
    public const string EvidenceMissing = "ELIGIBILITY_EVIDENCE_MISSING";
    public const string EligibilitySnapshotMissing = "ELIGIBILITY_SNAPSHOT_MISSING";
    public const string EligibilitySnapshotUnknown = "ELIGIBILITY_SNAPSHOT_UNKNOWN";
    public const string EligibilitySnapshotBlocked = "ELIGIBILITY_SNAPSHOT_BLOCKED";
    public const string EligibilitySnapshotUnreadable = "ELIGIBILITY_SNAPSHOT_UNREADABLE";
    public const string EligibilitySnapshotStale = "ELIGIBILITY_SNAPSHOT_STALE";
    public const string EligibilitySourceUnavailable = "ELIGIBILITY_SOURCE_UNAVAILABLE";
    public const string EligibilitySourceVersionMissing = "ELIGIBILITY_SOURCE_VERSION_MISSING";
    public const string PhoneCallRestrictionMissing = "PHONE_CALL_RESTRICTION_MISSING";
    public const string PhoneCallRestricted = "PHONE_CALL_RESTRICTED";
    public const string PhoneCallRestrictionSourceUnavailable =
        "PHONE_CALL_RESTRICTION_SOURCE_UNAVAILABLE";
    /// <summary>
    /// Retained vocabulary, no longer emitted since <c>OD-15</c> dropped the customer-trust score
    /// from the skip predicate. Rows persisted before that decision still carry it, so the code
    /// stays readable rather than becoming an unknown value on an old evidence record.
    /// </summary>
    public const string TrustResolverUnavailable = "TRUST_RESOLVER_UNAVAILABLE";
    public const string TrustResolverVersionMissing = "TRUST_RESOLVER_VERSION_MISSING";
    public const string TrustRiskEvidenceUnavailable = "TRUST_RISK_EVIDENCE_UNAVAILABLE";
    public const string ContactInvalid = "CONTACT_INVALID";
    public const string ConfirmationWindowExpired = "CONFIRMATION_WINDOW_EXPIRED";
    public const string CapacitySourceUnavailable = "CAPACITY_SOURCE_UNAVAILABLE";
    public const string CapacityDeadlineUnavailable = "CAPACITY_DEADLINE_UNAVAILABLE";
    public const string TrustSkipDisabledRequireIvr = "TRUST_SKIP_DISABLED_REQUIRE_IVR";
    public const string TrustSkipVetoedBySales = "TRUST_SKIP_VETOED_BY_SALES";
    public const string RiskFlagsPresentRequireIvr = "RISK_FLAGS_PRESENT_REQUIRE_IVR";
    public const string TrustedCustomerSkip = "TRUSTED_CUSTOMER_SKIP";
}

/// <summary>
/// Readability of the Sales-supplied <c>eligibility_snapshot</c> as IVR received it.
/// <see cref="Missing"/> and <see cref="Unreadable"/> are deliberately distinct: the first
/// says Sales sent nothing, the second says Sales sent something IVR cannot interpret.
/// They fail closed the same way but need different follow-up, so they must not collapse.
/// </summary>
public enum EligibilityEvidenceState
{
    Missing,
    Unreadable,
    Present,
}

/// <summary>
/// Typed projection of the Sales <c>eligibility_snapshot</c> evidence bag (W-0030 / P4-2).
/// The wire field stays an open object because its shape is not owner-approved yet
/// (<c>OD-V1-03</c>, closure ticket T-02); the contract-side shape IVR expects lives in
/// <c>specs/api/evidence/eligibility-snapshot.v1.schema.json</c>. IVR validates against that
/// shape and fails closed, rather than tightening a contract Sales has not agreed to.
/// </summary>
public sealed record EligibilityEvidence(
    EligibilityEvidenceState State,
    string? Decision,
    string? SourceVersion,
    DateTimeOffset? CapturedAt,
    bool SourceAvailable,
    IReadOnlyList<string> Blockers,
    string? SnapshotHash)
{
    public static EligibilityEvidence Absent { get; } = new(
        EligibilityEvidenceState.Missing,
        null,
        null,
        null,
        true,
        [],
        null);

    public static EligibilityEvidence Malformed(string? snapshotHash) => new(
        EligibilityEvidenceState.Unreadable,
        null,
        null,
        null,
        true,
        [],
        snapshotHash);
}

/// <summary>
/// The transactional voice-contact decision Sales supplies (W-0031 / P4-2 §2.1-2.2).
/// <para>
/// This type deliberately carries no SMS, email, marketing-consent or newsletter member. The
/// separation between a transactional voice decision and marketing consent is enforced by the
/// shape of the type, not by a rule that has to remember to ignore a field — a rule can be
/// edited by someone who does not know why the field was there; a missing field cannot be read.
/// </para>
/// </summary>
public sealed record VoiceContactEvidence(
    bool? Restricted,
    bool SourceAvailable,
    string? SourceVersion)
{
    public static VoiceContactEvidence Unknown { get; } = new(null, true, null);
}

/// <summary>
/// Risk evidence backing the "do not call a returning customer" decision (W-0031 / P4-3 §2.3,
/// owner decision <c>OD-15</c>). The decision it produces is still reported as
/// <c>TASK_SKIPPED_TRUSTED_CUSTOMER</c>: the wire enum and the database check constraints predate
/// <c>OD-15</c> and renaming them would break stored rows for a wording change.
/// <para>
/// Note the asymmetry with <see cref="VoiceContactEvidence"/>, which is deliberate. Missing voice
/// evidence <b>blocks</b>: not knowing whether we may call must never resolve to calling. Missing
/// trust evidence <b>requires the call</b>: not knowing whether we may skip must never resolve to
/// skipping. Both are fail-closed; they close in opposite directions because the harm differs —
/// one is an unwanted call, the other is an unconfirmed order.
/// </para>
/// </summary>
public sealed record TrustResolverEvidence(
    bool SkipFeatureEnabled,
    bool? SkipAllowedBySales,
    bool ResolverAvailable,
    string? ResolverVersion,
    string? TrustStatus,
    bool RiskEvidenceAvailable,
    IReadOnlyList<string> RiskFlags)
{
    /// <summary>Default posture: require IVR. No risk decision is contractually available.</summary>
    public static TrustResolverEvidence RequireIvr { get; } =
        new(false, null, false, null, null, false, []);

    /// <summary>
    /// True only when Sales has evidenced that this is a returning customer with nothing on the
    /// order worth calling about, and that evidence is attributable to a version.
    /// <para>
    /// The predicate reads risk, not a trust score. Under <c>OD-15</c> "returning customer" is the
    /// <em>absence</em> of a risk flag, because that is how Sales already reports it: a first-time
    /// buyer arrives carrying <c>NEW_CUSTOMER</c> / <c>VERIFIED_ORDER_COUNT_0</c>, exactly like
    /// <c>COD_FAIL_HISTORY</c> or <c>SUSPICIOUS_DUPLICATE</c> arrive. One empty list therefore
    /// answers both halves of the question — is this a returning customer, and is this order
    /// ordinary — without waiting on the customer-trust scoring engine that <c>DC-06</c> records
    /// as unbuilt.
    /// </para>
    /// <para>
    /// <see cref="RiskEvidenceAvailable"/> is what keeps that safe, and it is not optional. An
    /// empty <see cref="RiskFlags"/> means "Sales looked and found nothing" only when Sales says
    /// it looked; on its own, an empty list is equally consistent with Sales never having
    /// evaluated the order — and silently reading that as "no risk" would skip the confirmation
    /// call on precisely the fake orders this module exists to catch.
    /// </para>
    /// </summary>
    public bool CanSkip =>
        SkipFeatureEnabled
        && SkipAllowedBySales is not false
        && RiskEvidenceAvailable
        && !string.IsNullOrWhiteSpace(ResolverVersion)
        && RiskFlags.Count == 0;
}

public sealed record EligibilityCapacitySnapshot(
    bool SourceAvailable,
    bool CanMeetDeadline,
    string SessionId,
    int ActiveSimCount,
    int PendingCallJobs,
    int ExpiredCallJobs,
    int MissedDeadlineCount,
    string? ShortageReason,
    string EvidenceRef);

public sealed record EligibilitySnapshot(
    string OrderState,
    string ProgramCode,
    string PaymentMethod,
    bool IvrConfirmationRequired,
    bool NotForQuoteCartDraft,
    EligibilityEvidence SourceEligibility,
    VoiceContactEvidence VoiceContact,
    string PhoneValidationStatus,
    DateTimeOffset DialTokenExpiresAt,
    DateTimeOffset ConfirmationWindowStartedAt,
    DateTimeOffset ConfirmationWindowExpiresAt,
    TrustResolverEvidence Trust,
    EligibilityCapacitySnapshot Capacity,
    bool EvidenceAvailable,
    string EvidenceRef,
    DateTimeOffset EvaluatedAt);

public sealed record EligibilityReason(
    string Code,
    string Signal,
    string EvidenceRef);

public sealed record EligibilityEvaluation(
    bool Eligible,
    string Decision,
    IReadOnlyList<EligibilityReason> Reasons,
    IReadOnlyList<string> Advisories,
    IReadOnlyList<string> EvidenceRefs,
    bool TrustedCustomerSkipped,
    bool IsCountedCustomerAttempt,
    DateTimeOffset EvaluatedAt,
    string? CapacityIncidentId = null);

public static class EligibilityRules
{
    public static EligibilityEvaluation Evaluate(EligibilitySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        string rootEvidence = RequireEvidence(snapshot.EvidenceRef);

        if (!snapshot.NotForQuoteCartDraft
            || snapshot.OrderState is "QUOTE" or "CART" or "DRAFT")
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.NotOfficialOrder,
                "order.official",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!string.Equals(snapshot.OrderState, "CONFIRMING", StringComparison.Ordinal)
            || !snapshot.IvrConfirmationRequired)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.OrderStateNotCallable,
                "order.state",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        bool allowedMatrix = (snapshot.ProgramCode, snapshot.PaymentMethod) is
            ("GOLDEN_HOUR", "ONLINE") or ("TWENTY_FOUR_SEVEN", "COD");
        if (!allowedMatrix)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.ProgramPaymentMatrixRejected,
                "order.program_payment",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!snapshot.EvidenceAvailable)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EvidenceMissing,
                "eligibility.evidence",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        EligibilityEvaluation? sourceResult = EvaluateSourceEligibility(snapshot, rootEvidence);
        if (sourceResult is not null)
        {
            return sourceResult;
        }

        EligibilityEvaluation? voiceResult = EvaluateVoiceContact(snapshot, rootEvidence);
        if (voiceResult is not null)
        {
            return voiceResult;
        }

        if (!string.Equals(
                snapshot.PhoneValidationStatus,
                "VALID",
                StringComparison.Ordinal)
            || snapshot.DialTokenExpiresAt < snapshot.ConfirmationWindowExpiresAt
            || snapshot.DialTokenExpiresAt <= snapshot.EvaluatedAt)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.ContactInvalid,
                "contact.phone_validation",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (snapshot.ConfirmationWindowStartedAt > snapshot.EvaluatedAt
            || snapshot.ConfirmationWindowExpiresAt <= snapshot.EvaluatedAt)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.ConfirmationWindowExpired,
                "task.confirmation_window",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!snapshot.Capacity.SourceAvailable)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.CapacitySourceUnavailable,
                "capacity.source",
                snapshot.Capacity.EvidenceRef,
                snapshot.EvaluatedAt);
        }

        if (!snapshot.Capacity.CanMeetDeadline)
        {
            return Block(
                EligibilityDecisions.CapacityException,
                EligibilityReasonCodes.CapacityDeadlineUnavailable,
                "capacity.deadline",
                snapshot.Capacity.EvidenceRef,
                snapshot.EvaluatedAt);
        }

        if (snapshot.Trust.CanSkip)
        {
            string skipEvidence = Evidence(rootEvidence, "trust-skip");
            return new EligibilityEvaluation(
                false,
                EligibilityDecisions.SkippedTrustedCustomer,
                [],
                [EligibilityReasonCodes.TrustedCustomerSkip],
                [skipEvidence],
                true,
                false,
                snapshot.EvaluatedAt);
        }

        string evaluationEvidence = Evidence(rootEvidence, "eligible");
        string[] advisories = DescribeRequiredIvr(snapshot.Trust);

        // P4-2 §2.3: the immutable snapshot hash travels with the evaluation evidence so the
        // result callback can be traced back to the exact evidence bag the decision was made on.
        // The hash is the only thing carried — never the snapshot body, which is Sales content.
        string[] evidenceRefs = snapshot.SourceEligibility.SnapshotHash is { Length: > 0 } hash
            ? [evaluationEvidence, Evidence(rootEvidence, string.Concat("snapshot/", hash)), snapshot.Capacity.EvidenceRef]
            : [evaluationEvidence, snapshot.Capacity.EvidenceRef];
        return new EligibilityEvaluation(
            true,
            EligibilityDecisions.Eligible,
            [],
            advisories,
            evidenceRefs
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            false,
            false,
            snapshot.EvaluatedAt);
    }

    /// <summary>
    /// Validates the Sales eligibility evidence bag before any dispatch decision (P4-2 §2.2).
    /// Structural problems are checked before content so the reason code names the real defect:
    /// a snapshot IVR could not read must not be reported as a decision it disagrees with.
    /// Every failure holds or blocks — there is no path where missing evidence dispatches a call.
    /// </summary>
    private static EligibilityEvaluation? EvaluateSourceEligibility(
        EligibilitySnapshot snapshot,
        string rootEvidence)
    {
        EligibilityEvidence evidence = snapshot.SourceEligibility;
        string signal = "eligibility.snapshot";

        if (evidence.State == EligibilityEvidenceState.Missing)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySnapshotMissing,
                signal,
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (evidence.State == EligibilityEvidenceState.Unreadable)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySnapshotUnreadable,
                signal,
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!evidence.SourceAvailable)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySourceUnavailable,
                string.Concat(signal, ".source_available"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (string.IsNullOrWhiteSpace(evidence.SourceVersion))
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySourceVersionMissing,
                string.Concat(signal, ".source_version"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        // Evidence captured before the confirmation window opened describes a different order
        // state, and evidence stamped in
        // the future is a clock or producer defect. Both are held, never dispatched.
        if (evidence.CapturedAt is not { } capturedAt
            || capturedAt < snapshot.ConfirmationWindowStartedAt
            || capturedAt > snapshot.EvaluatedAt)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySnapshotStale,
                string.Concat(signal, ".captured_at"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (string.IsNullOrWhiteSpace(evidence.Decision))
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySnapshotMissing,
                string.Concat(signal, ".decision"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!string.Equals(evidence.Decision, "ELIGIBLE", StringComparison.Ordinal))
        {
            bool knownBlock = evidence.Decision is "BLOCKED" or "NOT_ELIGIBLE" or "INELIGIBLE";
            return Block(
                knownBlock
                    ? EligibilityDecisions.BlockedOperational
                    : EligibilityDecisions.HeldAdminReview,
                knownBlock
                    ? EligibilityReasonCodes.EligibilitySnapshotBlocked
                    : EligibilityReasonCodes.EligibilitySnapshotUnknown,
                string.Concat(signal, ".decision"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        // Sales says ELIGIBLE while still listing blockers. Trusting the summary field over the
        // detail would dispatch a call on an order Sales itself flagged, so hold for review.
        if (evidence.Blockers.Count > 0)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.EligibilitySnapshotBlocked,
                string.Concat(signal, ".blockers"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        return null;
    }

    /// <summary>
    /// Validates the transactional voice-contact decision (W-0031 / P4-3 §2.1).
    /// Closes toward blocking: restricted, unknown and source-unavailable all stop the dispatch,
    /// because not knowing whether we may call a customer must never resolve to calling them.
    /// </summary>
    private static EligibilityEvaluation? EvaluateVoiceContact(
        EligibilitySnapshot snapshot,
        string rootEvidence)
    {
        VoiceContactEvidence voice = snapshot.VoiceContact;
        const string signal = "voice.call_restriction";

        // The resolver told us it could not answer. That is not permission.
        if (!voice.SourceAvailable)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.PhoneCallRestrictionSourceUnavailable,
                string.Concat(signal, ".source_available"),
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (voice.Restricted is null)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.PhoneCallRestrictionMissing,
                signal,
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (voice.Restricted.Value)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.PhoneCallRestricted,
                signal,
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        return null;
    }

    /// <summary>
    /// Explains, as advisories, why this order is being called instead of skipped (P4-3 §2.3,
    /// <c>OD-15</c>). These never block: requiring the call is the safe direction whenever the
    /// skip cannot be fully evidenced. Naming <em>which</em> part was missing is what makes the
    /// default auditable rather than merely conservative, and it is the signal that separates
    /// "Sales says this order carries risk" from "Sales is not sending risk evidence yet".
    /// </summary>
    private static string[] DescribeRequiredIvr(TrustResolverEvidence trust)
    {
        if (trust.CanSkip)
        {
            return [];
        }

        if (!trust.SkipFeatureEnabled)
        {
            // Nothing below is actionable while the policy switch itself is off, and reporting
            // the rest would read as an upstream gap when the cause is a local setting.
            return [EligibilityReasonCodes.TrustSkipDisabledRequireIvr];
        }

        var advisories = new List<string>();
        if (trust.SkipAllowedBySales is false)
        {
            advisories.Add(EligibilityReasonCodes.TrustSkipVetoedBySales);
        }

        if (!trust.RiskEvidenceAvailable)
        {
            advisories.Add(EligibilityReasonCodes.TrustRiskEvidenceUnavailable);
        }
        else if (trust.RiskFlags.Count > 0)
        {
            // The ordinary first-time-buyer path. Sales evaluated the order and flagged it —
            // NEW_CUSTOMER and VERIFIED_ORDER_COUNT_0 arrive here like any other risk flag.
            advisories.Add(EligibilityReasonCodes.RiskFlagsPresentRequireIvr);
        }

        if (string.IsNullOrWhiteSpace(trust.ResolverVersion))
        {
            advisories.Add(EligibilityReasonCodes.TrustResolverVersionMissing);
        }

        return [.. advisories];
    }

    private static EligibilityEvaluation Block(
        string decision,
        string code,
        string signal,
        string evidenceRef,
        DateTimeOffset evaluatedAt)
    {
        string safeEvidence = RequireEvidence(evidenceRef);
        return new EligibilityEvaluation(
            false,
            decision,
            [new EligibilityReason(code, signal, safeEvidence)],
            [],
            [safeEvidence],
            false,
            false,
            evaluatedAt);
    }

    private static string Evidence(string root, string suffix) =>
        string.Concat(root.Split('#')[0], "#eligibility/", suffix);

    private static string RequireEvidence(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value;
    }
}
