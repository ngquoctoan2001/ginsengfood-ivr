namespace Ivr.Infrastructure.Persistence.Entities;

public abstract class RetainedEntity
{
    public string RetentionClass { get; set; } = "LEGAL_DECISION_PENDING";

    public DateTimeOffset? RetainUntil { get; set; }

    public DateTimeOffset? LegalHoldUntil { get; set; }

    public DateTimeOffset? AnonymizedAt { get; set; }
}

public sealed class ConfirmationTaskEntity : RetainedEntity
{
    public Guid Id { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string ContractVersion { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string OfficialOrderId { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public string OrderVersion { get; set; } = string.Empty;
    public string OrderState { get; set; } = string.Empty;
    public string PaymentMethodSnapshot { get; set; } = string.Empty;
    public bool IvrConfirmationRequired { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerTrustStatus { get; set; }
    public bool? TrustedSkipAllowed { get; set; }
    public string? RiskFlagsJson { get; set; }
    public string ProgramType { get; set; } = string.Empty;
    public string AttemptPolicyVersion { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public string AttemptOffsetsSecondsJson { get; set; } = "[]";
    public DateTimeOffset ConfirmationWindowStartedAt { get; set; }
    public DateTimeOffset ConfirmationWindowExpiresAt { get; set; }
    public string? OfficialContactId { get; set; }
    public string PhoneRef { get; set; } = string.Empty;
    public string PhoneMasked { get; set; } = string.Empty;
    public string? PhoneValidationStatus { get; set; }
    public string DialTokenCiphertext { get; set; } = string.Empty;
    public DateTimeOffset DialTokenExpiresAt { get; set; }
    public string PrivacySafeOrderSummaryJson { get; set; } = "{}";
    public string CallScriptTemplateId { get; set; } = string.Empty;
    public string CallScriptVersion { get; set; } = string.Empty;
    public string EvidencePolicyVersion { get; set; } = string.Empty;
    public string PrivacyPolicyVersion { get; set; } = string.Empty;
    public string? EligibilityDecision { get; set; }
    public string? EligibilitySnapshotJson { get; set; }
    public string? BlockedReasonsJson { get; set; }
    public string? SellableStatusJson { get; set; }
    public DateTimeOffset? SellableCapturedAt { get; set; }
    public bool CallRestriction { get; set; }
    public bool NotForQuoteCartDraft { get; set; } = true;
    public bool NoDirectOrderUpdate { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public string? EvidenceRefsJson { get; set; }
    public string? AuditRefsJson { get; set; }
}

public sealed class AttemptPolicyEntity : RetainedEntity
{
    public string PolicyVersion { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public string AttemptOffsetsSecondsJson { get; set; } = "[]";
    public int ConfirmationWindowSeconds { get; set; }
    public string AllowedExecutionModesJson { get; set; } = "[]";
    public bool ApprovedForProduction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CallJobEntity : RetainedEntity
{
    public string IvrCallJobId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string OfficialOrderId { get; set; } = string.Empty;
    public string OrderVersionSnapshot { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public string AttemptPolicyCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public string AttemptOffsetsSecondsJson { get; set; } = "[]";
    public int ConfirmationWindowSeconds { get; set; }
    public string AttemptScheduleJson { get; set; } = "[]";
    public DateTimeOffset T0At { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Eligible { get; set; }
    public string EligibilityDecision { get; set; } = string.Empty;
    public string QueueStatus { get; set; } = string.Empty;
    public string? CapacityIncidentId { get; set; }
    public string ScriptVersion { get; set; } = string.Empty;
    public string PrivacyPolicyVersion { get; set; } = string.Empty;
    public bool InputSignalOnly { get; set; } = true;
    public bool NoDirectOrderUpdate { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosedReason { get; set; }
    public string? EvidenceRefsJson { get; set; }
    public string? AuditRefsJson { get; set; }
}

public sealed class TaskIntakeOutboxEntity : RetainedEntity
{
    public Guid OutboxId { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string IvrCallJobId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string PayloadSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public sealed class CallAttemptEntity : RetainedEntity
{
    public string IvrCallAttemptId { get; set; } = string.Empty;
    public string IvrCallJobId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public int MaxAttemptsSnapshot { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset ScheduledWindowExpiresAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResultStatus { get; set; }
    public string? DtmfKey { get; set; }
    public string? Disposition { get; set; }
    public bool IsCountedCustomerAttempt { get; set; }
    public bool TechnicalRetryAllowed { get; set; }
    public int TechnicalRetryCount { get; set; }
    public bool NoAnswer { get; set; }
    public bool InvalidPhone { get; set; }
    public string? TechnicalExceptionType { get; set; }
    public string? SimChannelId { get; set; }
    public string? ProviderCallId { get; set; }
    public string? RawCallEventId { get; set; }
    public string? BlockedReason { get; set; }
    public string PolicyVersion { get; set; } = string.Empty;
    public string ScriptVersion { get; set; } = string.Empty;
    public string? EvidenceRefsJson { get; set; }
    public string? AuditRefsJson { get; set; }
}

public sealed class RawCallEventEntity : RetainedEntity
{
    public string RawEventId { get; set; } = string.Empty;
    public string IvrCallAttemptId { get; set; } = string.Empty;
    public string IvrCallJobId { get; set; } = string.Empty;
    public string? ProviderInternalPayloadRef { get; set; }
    public string RawCallStatus { get; set; } = string.Empty;
    public string? RawDtmf { get; set; }
    public string? AudioStatus { get; set; }
    public string? TechnicalErrorCode { get; set; }
    public string? RecordingRef { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}

public sealed class CallResultEntity : RetainedEntity
{
    public string IvrCallResultId { get; set; } = string.Empty;
    public string IvrCallJobId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string OfficialOrderId { get; set; } = string.Empty;
    public string? OrderVersionSnapshot { get; set; }
    public string OrderVersionSeenByIvr { get; set; } = string.Empty;
    public string FinalResultStatus { get; set; } = string.Empty;
    public string ResultType { get; set; } = string.Empty;
    public string? ResultReason { get; set; }
    public string? DtmfKey { get; set; }
    public bool IsCountedCustomerAttempt { get; set; }
    public bool IsFinalForIvr { get; set; }
    public string RecommendedCoreAction { get; set; } = string.Empty;
    public bool CoreOrderHandoffRequired { get; set; }
    public bool HumanReviewRequired { get; set; }
    public bool InputSignalOnly { get; set; } = true;
    public bool NoDirectOrderUpdate { get; set; } = true;
    public bool NoPaymentOrRevenueEffect { get; set; } = true;
    public string? TechnicalErrorCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? EvidenceRefsJson { get; set; }
    public string? AuditRefsJson { get; set; }
}

public sealed class ResultCallbackEntity : RetainedEntity
{
    public string CallbackId { get; set; } = string.Empty;
    public string IvrCallResultId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string OfficialOrderId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;
    public string ResultState { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public bool RequiresCoreRevalidation { get; set; } = true;
    public string PayloadJson { get; set; } = "{}";
    public string PayloadSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public int? CoreHttpStatus { get; set; }
    public string? CoreResponseCode { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? LastRetryAt { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public string? LastError { get; set; }
    public string? LeaseToken { get; set; }
    public DateTimeOffset? LeaseExpiresAt { get; set; }
}

public sealed class SimChannelEntity : RetainedEntity
{
    public string SimChannelId { get; set; } = string.Empty;
    public string SimNumberRef { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Status { get; set; } = "DISABLED";
    public string AdapterMode { get; set; } = "MOCK";
    public string ExecutionMode { get; set; } = "MOCK";
    public string ProviderName { get; set; } = "MOCK";
    public string? ActiveCallJobId { get; set; }
    public int FailCount { get; set; }
    public DateTimeOffset? LastHealthCheckAt { get; set; }
    public DateTimeOffset? CooldownUntil { get; set; }
    public DateTimeOffset? QuarantineUntil { get; set; }
    public string? DisabledReason { get; set; }
    public Guid? LeaseToken { get; set; }
    public long LeaseFencingGeneration { get; set; }
    public string? LeasedByWorkerId { get; set; }
    public DateTimeOffset? LeaseAcquiredAt { get; set; }
    public DateTimeOffset? LeaseExpiresAt { get; set; }
}

public sealed class CapacityIncidentEntity : RetainedEntity
{
    public string CapacityIncidentId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ProgramCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public bool HoldNewCalls { get; set; }
    public int ActiveSimCount { get; set; }
    public int PendingCallJobs { get; set; }
    public int ExpiredCallJobs { get; set; }
    public int MissedDeadlineCount { get; set; }
    public string? ShortageReason { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? Reason { get; set; }
}

public sealed class TechnicalExceptionEntity : RetainedEntity
{
    public string TechnicalExceptionId { get; set; } = string.Empty;
    public string IvrCallAttemptId { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public bool CustomerAttemptCounted { get; set; }
    public bool TechnicalRetryAllowed { get; set; }
    public int TechnicalRetryCount { get; set; }
    public string? RetryReason { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AdminActionEntity : RetainedEntity
{
    public string AdminActionId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? BeforeStateJson { get; set; }
    public string? AfterStateJson { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? EvidenceRef { get; set; }
    public bool NoPolicyBypass { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class EvidenceLinkEntity : RetainedEntity
{
    public long Id { get; set; }
    public string OwnerTable { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string EvidenceRef { get; set; } = string.Empty;
    public string? AuditRef { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}

public sealed class IdempotencyKeyEntity : RetainedEntity
{
    public string Scope { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public string ResponseSnapshotJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class AuditLogEntity : RetainedEntity
{
    public Guid AuditId { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string ActorType { get; set; } = "service";
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? BeforeStateJson { get; set; }
    public string? AfterStateJson { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class EvidenceEntity : RetainedEntity
{
    public string EvidenceRef { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string PayloadRef { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}

public sealed class ReviewItemEntity : RetainedEntity
{
    public string ReviewItemId { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}

public sealed class RetentionCheckpointEntity
{
    public string DataClass { get; set; } = string.Empty;
    public string Segment { get; set; } = string.Empty;
    public Guid RunId { get; set; }
    public string Status { get; set; } = string.Empty;
    public long ProcessedCount { get; set; }
    public DateTimeOffset? FirstNotConfiguredAt { get; set; }
    public DateTimeOffset LastRunAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
