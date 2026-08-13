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
    public const string SellableSnapshotMissing = "SELLABLE_SNAPSHOT_MISSING";
    public const string SellableSnapshotStale = "SELLABLE_SNAPSHOT_STALE";
    public const string SellableUnknown = "SELLABLE_STATUS_UNKNOWN";
    public const string InventoryNotSellable = "INVENTORY_NOT_SELLABLE";
    public const string RecallHoldActive = "RECALL_HOLD_ACTIVE";
    public const string SaleLockActive = "SALE_LOCK_ACTIVE";
    public const string QualityHoldActive = "QUALITY_HOLD_ACTIVE";
    public const string StockUnavailable = "STOCK_UNAVAILABLE";
    public const string BatchNotReleased = "BATCH_NOT_RELEASED";
    public const string TraceNotReady = "TRACE_NOT_READY";
    public const string PhoneCallRestrictionMissing = "PHONE_CALL_RESTRICTION_MISSING";
    public const string PhoneCallRestricted = "PHONE_CALL_RESTRICTED";
    public const string ContactInvalid = "CONTACT_INVALID";
    public const string ConfirmationWindowExpired = "CONFIRMATION_WINDOW_EXPIRED";
    public const string CapacitySourceUnavailable = "CAPACITY_SOURCE_UNAVAILABLE";
    public const string CapacityDeadlineUnavailable = "CAPACITY_DEADLINE_UNAVAILABLE";
    public const string TrustSkipDisabledRequireIvr = "TRUST_SKIP_DISABLED_REQUIRE_IVR";
    public const string TrustedCustomerSkip = "TRUSTED_CUSTOMER_SKIP";
}

public enum EligibilitySellableDecision
{
    Sellable,
    NotSellable,
    Blocked,
    Unknown,
}

public sealed record EligibilitySellableLine(
    EligibilitySellableDecision Decision,
    bool? RecallHold,
    bool? SaleLock,
    bool? QualityHold,
    bool? StockAvailable,
    bool? BatchReleased,
    bool? TraceReady,
    DateTimeOffset CapturedAt);

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
    string? SourceEligibilityDecision,
    IReadOnlyList<EligibilitySellableLine>? SellableLines,
    bool? PhoneCallRestriction,
    bool SmsOptOut,
    string PhoneValidationStatus,
    DateTimeOffset DialTokenExpiresAt,
    DateTimeOffset ConfirmationWindowStartedAt,
    DateTimeOffset ConfirmationWindowExpiresAt,
    string? CustomerTrustStatus,
    bool TrustedSkipAllowed,
    IReadOnlyList<string>? RiskFlags,
    bool TrustSkipFeatureEnabled,
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

        if (string.IsNullOrWhiteSpace(snapshot.SourceEligibilityDecision))
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.EligibilitySnapshotMissing,
                "eligibility.decision",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (!string.Equals(
                snapshot.SourceEligibilityDecision,
                "ELIGIBLE",
                StringComparison.Ordinal))
        {
            bool knownBlock = snapshot.SourceEligibilityDecision is
                "BLOCKED" or "NOT_ELIGIBLE" or "INELIGIBLE";
            return Block(
                knownBlock
                    ? EligibilityDecisions.BlockedOperational
                    : EligibilityDecisions.HeldAdminReview,
                knownBlock
                    ? EligibilityReasonCodes.EligibilitySnapshotBlocked
                    : EligibilityReasonCodes.EligibilitySnapshotUnknown,
                "eligibility.decision",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        EligibilityEvaluation? sellableResult = EvaluateSellable(snapshot, rootEvidence);
        if (sellableResult is not null)
        {
            return sellableResult;
        }

        if (snapshot.PhoneCallRestriction is null)
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.PhoneCallRestrictionMissing,
                "crm.phone_call_restriction",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        if (snapshot.PhoneCallRestriction.Value)
        {
            return Block(
                EligibilityDecisions.BlockedOperational,
                EligibilityReasonCodes.PhoneCallRestricted,
                "crm.phone_call_restriction",
                rootEvidence,
                snapshot.EvaluatedAt);
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

        bool hasRisk = snapshot.RiskFlags is { Count: > 0 };
        bool trusted = string.Equals(
            snapshot.CustomerTrustStatus,
            "TRUSTED",
            StringComparison.Ordinal);
        if (snapshot.TrustSkipFeatureEnabled
            && snapshot.TrustedSkipAllowed
            && trusted
            && !hasRisk)
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
        string[] advisories = trusted && !snapshot.TrustSkipFeatureEnabled
            ? [EligibilityReasonCodes.TrustSkipDisabledRequireIvr]
            : [];
        return new EligibilityEvaluation(
            true,
            EligibilityDecisions.Eligible,
            [],
            advisories,
            new[] { evaluationEvidence, snapshot.Capacity.EvidenceRef }
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            false,
            false,
            snapshot.EvaluatedAt);
    }

    private static EligibilityEvaluation? EvaluateSellable(
        EligibilitySnapshot snapshot,
        string rootEvidence)
    {
        if (snapshot.SellableLines is not { Count: > 0 })
        {
            return Block(
                EligibilityDecisions.HeldAdminReview,
                EligibilityReasonCodes.SellableSnapshotMissing,
                "sellable_status",
                rootEvidence,
                snapshot.EvaluatedAt);
        }

        for (int index = 0; index < snapshot.SellableLines.Count; index++)
        {
            EligibilitySellableLine line = snapshot.SellableLines[index];
            string signal = string.Concat("sellable_status[", index, "]");
            string evidence = Evidence(rootEvidence, string.Concat("sellable/", index));
            if (line.CapturedAt < snapshot.ConfirmationWindowStartedAt
                || line.CapturedAt > snapshot.EvaluatedAt)
            {
                return Block(
                    EligibilityDecisions.HeldAdminReview,
                    EligibilityReasonCodes.SellableSnapshotStale,
                    signal,
                    evidence,
                    snapshot.EvaluatedAt);
            }

            if (!Enum.IsDefined(line.Decision))
            {
                return Block(
                    EligibilityDecisions.HeldAdminReview,
                    EligibilityReasonCodes.SellableUnknown,
                    signal,
                    evidence,
                    snapshot.EvaluatedAt);
            }

            string? decisionReason = line.Decision switch
            {
                EligibilitySellableDecision.NotSellable or
                    EligibilitySellableDecision.Blocked =>
                    EligibilityReasonCodes.InventoryNotSellable,
                EligibilitySellableDecision.Unknown => EligibilityReasonCodes.SellableUnknown,
                _ => null,
            };
            if (decisionReason is not null)
            {
                string decision = line.Decision == EligibilitySellableDecision.Unknown
                    ? EligibilityDecisions.HeldAdminReview
                    : EligibilityDecisions.BlockedOperational;
                return Block(decision, decisionReason, signal, evidence, snapshot.EvaluatedAt);
            }

            (string Code, bool? Value)[] gates =
            [
                (EligibilityReasonCodes.RecallHoldActive, line.RecallHold),
                (EligibilityReasonCodes.SaleLockActive, line.SaleLock),
                (EligibilityReasonCodes.QualityHoldActive, line.QualityHold),
            ];
            foreach ((string code, bool? value) in gates)
            {
                if (value is null)
                {
                    return Block(
                        EligibilityDecisions.HeldAdminReview,
                        EligibilityReasonCodes.SellableUnknown,
                        signal,
                        evidence,
                        snapshot.EvaluatedAt);
                }

                if (value.Value)
                {
                    return Block(
                        EligibilityDecisions.BlockedOperational,
                        code,
                        signal,
                        evidence,
                        snapshot.EvaluatedAt);
                }
            }

            (string Code, bool? Value)[] requiredTrue =
            [
                (EligibilityReasonCodes.StockUnavailable, line.StockAvailable),
                (EligibilityReasonCodes.BatchNotReleased, line.BatchReleased),
                (EligibilityReasonCodes.TraceNotReady, line.TraceReady),
            ];
            foreach ((string code, bool? value) in requiredTrue)
            {
                if (value is null)
                {
                    return Block(
                        EligibilityDecisions.HeldAdminReview,
                        EligibilityReasonCodes.SellableUnknown,
                        signal,
                        evidence,
                        snapshot.EvaluatedAt);
                }

                if (!value.Value)
                {
                    return Block(
                        EligibilityDecisions.BlockedOperational,
                        code,
                        signal,
                        evidence,
                        snapshot.EvaluatedAt);
                }
            }
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
