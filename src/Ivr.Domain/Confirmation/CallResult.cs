namespace Ivr.Domain.Confirmation;

public enum IvrResultType
{
    IvrConfirmed,
    IvrCustomerCancelled,
    IvrNoAnswerAttempt,
    IvrNoAnswerFinal,
    IvrConfirmationWindowExpired,
    IvrInvalidPhoneFinal,
    IvrWrongInput,
    IvrTechnicalException,
    IvrCapacityException,
    IvrOperationalBlocked,
    IvrPolicyBlocked,
}

public enum CoreActionRecommendation
{
    RevalidateAndConfirmOrder,
    RevalidateAndCancelCustomerRequest,
    NoStateChangeWaitForTimeout,
    RevalidateAndExpireConfirmation,
    RevalidateAndHoldAdminReview,
    IgnoreStaleCallback,
    BlockDueToOperationalConstraint,
}

public enum CallbackAcknowledgementCode
{
    Accepted,
    DuplicateAccepted,
    BlockedByCore,
    ReviewRequired,
    RejectedStale,
    IdempotencyConflict,
}

public enum CallbackDisposition
{
    Completed,
    ManualReviewNoRetry,
}

public sealed record CallbackAcknowledgement(
    CallbackAcknowledgementCode Code,
    CallbackId CallbackId,
    CorrelationId CorrelationId)
{
    public CallbackDisposition Disposition => Code is CallbackAcknowledgementCode.Accepted
        or CallbackAcknowledgementCode.DuplicateAccepted
        ? CallbackDisposition.Completed
        : CallbackDisposition.ManualReviewNoRetry;
}

public sealed class CallResultSnapshot
{
    private CallResultSnapshot(
        CallbackId callbackId,
        TaskId taskId,
        OrderId orderId,
        OrderVersion orderVersion,
        IvrProgramCode program,
        IvrResultType resultType,
        string? resultReason,
        bool isCountedCustomerAttempt,
        bool isFinalForIvr,
        int attemptNumber,
        DateTimeOffset occurredAt,
        CoreActionRecommendation recommendedCoreAction,
        EvidenceReference evidenceReference,
        AuditReference auditReference)
    {
        CallbackId = callbackId;
        TaskId = taskId;
        OrderId = orderId;
        OrderVersion = orderVersion;
        Program = program;
        ResultType = resultType;
        ResultReason = resultReason;
        IsCountedCustomerAttempt = isCountedCustomerAttempt;
        IsFinalForIvr = isFinalForIvr;
        AttemptNumber = attemptNumber;
        OccurredAt = occurredAt;
        RecommendedCoreAction = recommendedCoreAction;
        EvidenceReference = evidenceReference;
        AuditReference = auditReference;
    }

    public CallbackId CallbackId { get; }
    public TaskId TaskId { get; }
    public OrderId OrderId { get; }
    public OrderVersion OrderVersion { get; }
    public IvrProgramCode Program { get; }
    public IvrResultType ResultType { get; }
    public string? ResultReason { get; }
    public bool IsCountedCustomerAttempt { get; }
    public bool IsFinalForIvr { get; }
    public int AttemptNumber { get; }
    public DateTimeOffset OccurredAt { get; }
    public CoreActionRecommendation RecommendedCoreAction { get; }
    public EvidenceReference EvidenceReference { get; }
    public AuditReference AuditReference { get; }

    public static CallResultSnapshot Create(
        CallbackId callbackId,
        TaskId taskId,
        OrderId orderId,
        OrderVersion orderVersion,
        IvrProgramCode program,
        IvrResultType resultType,
        string? resultReason,
        bool isCountedCustomerAttempt,
        bool isFinalForIvr,
        int attemptNumber,
        DateTimeOffset occurredAt,
        CoreActionRecommendation recommendedCoreAction,
        EvidenceReference evidenceReference,
        AuditReference auditReference)
    {
        if (attemptNumber is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        }

        bool noAnswer = resultType is IvrResultType.IvrNoAnswerAttempt or IvrResultType.IvrNoAnswerFinal;
        if (noAnswer && recommendedCoreAction != CoreActionRecommendation.NoStateChangeWaitForTimeout)
        {
            throw new InvalidOperationException("No-answer is advisory and cannot request an order transition.");
        }

        ResultContractPolicy.EnsureSnapshotSemantics(
            resultType,
            isCountedCustomerAttempt,
            isFinalForIvr,
            recommendedCoreAction);

        string? safeReason = string.IsNullOrWhiteSpace(resultReason)
            ? null
            : PrivacySafeOrderSummary.EnsureSafeBounded(resultReason, 500, nameof(resultReason));
        return new CallResultSnapshot(
            callbackId,
            taskId,
            orderId,
            orderVersion,
            program,
            resultType,
            safeReason,
            isCountedCustomerAttempt,
            isFinalForIvr,
            attemptNumber,
            occurredAt,
            recommendedCoreAction,
            evidenceReference,
            auditReference);
    }

    public string ComputeHash() => DeterministicSnapshotHasher.Compute(
        CallbackId.Value,
        TaskId.Value,
        OrderId.Value,
        OrderVersion.Value,
        Program.ToString(),
        ResultType.ToString(),
        ResultReason,
        IsCountedCustomerAttempt.ToString(),
        IsFinalForIvr.ToString(),
        AttemptNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
        OccurredAt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture),
        RecommendedCoreAction.ToString(),
        EvidenceReference.Value,
        AuditReference.Value);
}
