using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.Infrastructure.Intake;

public static class TaskIntakeDecisions
{
    public const string AcceptedCallJobCreated = "TASK_ACCEPTED_CALL_JOB_CREATED";
    public const string AcceptedDryRunOnly = "TASK_ACCEPTED_DRY_RUN_ONLY";
    public const string RejectedNotOfficialOrder = "TASK_REJECTED_NOT_OFFICIAL_ORDER";
    public const string RejectedStateNotCallable = "TASK_REJECTED_STATE_NOT_CALLABLE";
    public const string RejectedPolicyMismatch = "TASK_REJECTED_POLICY_MISMATCH";
    public const string RejectedContactInvalid = "TASK_REJECTED_CONTACT_INVALID";
    public const string RejectedScriptNotApproved = "TASK_REJECTED_SCRIPT_NOT_APPROVED";
    public const string BlockedOperational = "TASK_BLOCKED_OPERATIONAL";
    public const string HeldAdminReview = "TASK_HELD_ADMIN_REVIEW";
    public const string HeldPolicyMissing = "TASK_HELD_POLICY_MISSING";
}

public sealed record TaskIntakeCommand(
    IvrConfirmationTaskV1 Source,
    string IdempotencyKey,
    string CorrelationId,
    string PayloadHash,
    ExecutionMode ExecutionMode)
{
    public TraceContextSnapshot? TraceContext { get; init; }

    public string TaskScope => string.Concat("order-core:", Source.Task_id, ":");

    public string ScopedIdempotencyKey => string.Concat(TaskScope, IdempotencyKey);
}

public sealed record TaskIntakeOutcome(
    string Decision,
    string? IvrCallJobId,
    string[] BlockedReasons,
    string EvidenceRef,
    string? FailureCode = null,
    string? FailureMessage = null)
{
    public bool IsFailure => FailureCode is not null;
}

public sealed record TaskIntakePersistencePlan(
    TaskIntakeOutcome Outcome,
    ConfirmationTaskEntity? Task = null,
    CallJobEntity? CallJob = null,
    TaskIntakeOutboxEntity? Outbox = null);

public interface ITaskIntakeStore
{
    public Task<TaskIntakeOutcome> ExecuteAsync(
        TaskIntakeCommand command,
        Func<CancellationToken, Task<TaskIntakePersistencePlan>> factory,
        CancellationToken cancellationToken = default);
}

public interface ITaskIntakeService
{
    public Task<TaskIntakeOutcome> IntakeAsync(
        TaskIntakeCommand command,
        CancellationToken cancellationToken = default);
}
