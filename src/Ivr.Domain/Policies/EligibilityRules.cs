namespace Ivr.Domain.Policies;

public static class EligibilityDecisions
{
    public const string Pending = "PENDING_ELIGIBILITY";
    public const string Eligible = "ELIGIBLE_FOR_IVR";
    public const string BlockedOperational = "TASK_BLOCKED_OPERATIONAL";
    public const string HeldAdminReview = "TASK_HELD_ADMIN_REVIEW";
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
    public const string ContactInvalid = "CONTACT_INVALID";

    // W-0129. One bool used to collapse seven distinct contact failures into a single opaque
    // internal reason. The reason now names the first failed field without changing the
    // TASK_REJECTED_CONTACT_INVALID decision. The HTTP endpoint still maps that decision to the
    // stable 422 IVR_CONTACT_INVALID envelope; these detailed values are not a new wire enum.
    public const string PhoneValidationStatusNotValid = "PHONE_VALIDATION_STATUS_NOT_VALID";
    public const string PhoneMaskedNotMasked = "PHONE_MASKED_NOT_MASKED";
    public const string DialTokenExpiresBeforeWindow = "DIAL_TOKEN_EXPIRES_BEFORE_WINDOW";
    public const string DialTokenAlreadyExpired = "DIAL_TOKEN_ALREADY_EXPIRED";
    public const string PhoneRefLooksLikeRawPhone = "PHONE_REF_LOOKS_LIKE_RAW_PHONE";
    public const string DialTokenLooksLikeRawPhone = "DIAL_TOKEN_LOOKS_LIKE_RAW_PHONE";
    public const string ContactFailedPrivacyGuard = "CONTACT_FAILED_PRIVACY_GUARD";

    // W-0129. The direct service boundary also distinguishes a false required flag from an invalid
    // program/payment pair. TaskIntakeEndpoint rejects both shapes during schema validation, so
    // this remains defensive classification for trusted in-process callers rather than a promise
    // that Module 3 receives either value in IvrTaskIntakeResult.blocked_reasons.
    public const string IvrConfirmationNotRequired = "IVR_CONFIRMATION_REQUIRED_NOT_TRUE";
    public const string ConfirmationWindowExpired = "CONFIRMATION_WINDOW_EXPIRED";
    public const string CapacitySourceUnavailable = "CAPACITY_SOURCE_UNAVAILABLE";
    public const string CapacityDeadlineUnavailable = "CAPACITY_DEADLINE_UNAVAILABLE";
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

        string evaluationEvidence = Evidence(rootEvidence, "eligible");

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
            [],
            evidenceRefs
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
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
