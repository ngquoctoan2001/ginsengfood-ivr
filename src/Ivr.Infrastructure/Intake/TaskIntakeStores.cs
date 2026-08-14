using System.Data;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Intake;

public sealed class InMemoryTaskIntakeStore(
    IAuditLogger auditLogger,
    TimeProvider timeProvider) : ITaskIntakeStore, IEligibilityRepository, IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<string, StoredOutcome> byKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StoredOutcome> byTask = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ConfirmationTaskEntity> tasks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CallJobEntity> jobs = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, TaskIntakeOutboxEntity> outbox = [];
    private readonly Dictionary<string, CapacityIncidentEntity> capacityIncidents =
        new(StringComparer.Ordinal);
    private readonly List<EvidenceLinkEntity> evidenceLinks = [];

    public int TaskCount => tasks.Count;

    public int CallJobCount => jobs.Count;

    public int OutboxCount => outbox.Count;

    public int CapacityIncidentCount => capacityIncidents.Count;

    public int EvidenceLinkCount => evidenceLinks.Count;

    public async Task<TaskIntakeOutcome> ExecuteAsync(
        TaskIntakeCommand command,
        Func<CancellationToken, Task<TaskIntakePersistencePlan>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(factory);
        ValidateCommand(command);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (byKey.TryGetValue(command.ScopedIdempotencyKey, out StoredOutcome? keyed))
            {
                EnsureSamePayload(keyed, command.PayloadHash);
                return keyed.Outcome;
            }

            if (byTask.TryGetValue(command.Source.Task_id, out StoredOutcome? taskOutcome))
            {
                EnsureSamePayload(taskOutcome, command.PayloadHash);
                byKey.Add(command.ScopedIdempotencyKey, taskOutcome);
                return taskOutcome.Outcome;
            }

            TaskIntakePersistencePlan plan = await factory(cancellationToken).ConfigureAwait(false);
            ValidatePlan(plan);
            await AppendAuditAsync(command, plan.Outcome, cancellationToken).ConfigureAwait(false);

            var stored = new StoredOutcome(command.PayloadHash, plan.Outcome);
            byKey.Add(command.ScopedIdempotencyKey, stored);
            if (plan.Task is not null)
            {
                byTask.Add(command.Source.Task_id, stored);
                tasks.Add(plan.Task.TaskId, plan.Task);
                jobs.Add(plan.CallJob!.IvrCallJobId, plan.CallJob);
                outbox.Add(plan.Outbox!.OutboxId, plan.Outbox);
            }

            return plan.Outcome;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();

    public async Task<EligibilityTaskRecord?> FindAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        PiiGuard.EnsureSafeText(taskId);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!tasks.TryGetValue(taskId, out ConfirmationTaskEntity? task))
            {
                return null;
            }

            CallJobEntity job = jobs.Values.Single(item => item.TaskId == taskId);
            TaskIntakeOutboxEntity intakeOutbox = outbox.Values
                .Single(item => item.TaskId == taskId);
            return new EligibilityTaskRecord(task, job, intakeOutbox);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<EligibilityEvaluation> PersistAsync(
        string taskId,
        EligibilityEvaluation evaluation,
        EligibilityCapacitySnapshot capacity,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        PostgresEligibilityRepository.Validate(
            taskId,
            evaluation,
            capacity,
            correlationId);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ConfirmationTaskEntity task = tasks[taskId];
            CallJobEntity job = jobs.Values.Single(item => item.TaskId == taskId);
            if (!string.Equals(
                    job.EligibilityDecision,
                    EligibilityDecisions.Pending,
                    StringComparison.Ordinal))
            {
                if (string.Equals(
                        job.EligibilityDecision,
                        evaluation.Decision,
                        StringComparison.Ordinal))
                {
                    return evaluation with { CapacityIncidentId = job.CapacityIncidentId };
                }

                throw new InvalidOperationException(
                    "The task already has a different eligibility decision.");
            }

            TaskIntakeOutboxEntity intakeOutbox = outbox.Values
                .Single(item => item.TaskId == taskId);
            EligibilityPersistenceArtifacts artifacts = EligibilityPersistence.Apply(
                task,
                job,
                intakeOutbox,
                evaluation,
                capacity,
                correlationId);
            if (artifacts.CapacityIncident is not null)
            {
                capacityIncidents.Add(
                    artifacts.CapacityIncident.CapacityIncidentId,
                    artifacts.CapacityIncident);
            }

            evidenceLinks.AddRange(artifacts.EvidenceLinks);
            await auditLogger.AppendAsync(
                new AuditEvent(
                    artifacts.Audit.ActorId,
                    artifacts.Audit.Action,
                    string.Concat("confirmation-task:", taskId),
                    artifacts.Audit.Reason,
                    correlationId,
                    new Dictionary<string, object?>
                    {
                        ["decision"] = artifacts.Evaluation.Decision,
                        ["eligible"] = artifacts.Evaluation.Eligible,
                        ["reason_codes"] = artifacts.Evaluation.Reasons
                            .Select(reason => reason.Code)
                            .ToArray(),
                        ["advisories"] = artifacts.Evaluation.Advisories,
                        ["capacity_incident_id"] =
                            artifacts.Evaluation.CapacityIncidentId,
                        ["is_counted_customer_attempt"] = false,
                    }),
                cancellationToken).ConfigureAwait(false);
            return artifacts.Evaluation;
        }
        finally
        {
            gate.Release();
        }
    }

    private Task<AuditLogEntry> AppendAuditAsync(
        TaskIntakeCommand command,
        TaskIntakeOutcome outcome,
        CancellationToken cancellationToken) =>
        auditLogger.AppendAsync(
            TaskIntakeAudit.Create(command, outcome, timeProvider.GetUtcNow()),
            cancellationToken);

    private static void EnsureSamePayload(StoredOutcome stored, string payloadHash)
    {
        if (!string.Equals(stored.PayloadHash, payloadHash, StringComparison.Ordinal))
        {
            throw IvrErrors.IdempotencyConflict();
        }
    }

    private sealed record StoredOutcome(string PayloadHash, TaskIntakeOutcome Outcome);

    internal static void ValidateCommand(TaskIntakeCommand command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.IdempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.CorrelationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.PayloadHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Source.Task_id);
        if (command.Source.Task_id.Length > 120)
        {
            throw new ArgumentException(
                "The task identifier exceeds the supported length.",
                nameof(command));
        }

        PiiGuard.EnsureSafeText(command.IdempotencyKey);
        PiiGuard.EnsureSafeText(command.CorrelationId);
        PiiGuard.EnsureSafeText(command.PayloadHash);
        PiiGuard.EnsureSafeText(command.Source.Task_id);
    }

    internal static void ValidatePlan(TaskIntakePersistencePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        bool hasTask = plan.Task is not null;
        if (hasTask != (plan.CallJob is not null) || hasTask != (plan.Outbox is not null))
        {
            throw new InvalidOperationException(
                "Task, call job and intake outbox must be persisted as one atomic unit.");
        }
    }
}

public sealed class PostgresTaskIntakeStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : ITaskIntakeStore
{
    private const string Scope = "task-intake";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<TaskIntakeOutcome> ExecuteAsync(
        TaskIntakeCommand command,
        Func<CancellationToken, Task<TaskIntakePersistencePlan>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(factory);
        InMemoryTaskIntakeStore.ValidateCommand(command);

        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
            .ConfigureAwait(false);
        await AcquireLocksAsync(context, command, cancellationToken).ConfigureAwait(false);

        IdempotencyKeyEntity? keyed = await context.IdempotencyKeys.FindAsync(
            [Scope, command.ScopedIdempotencyKey],
            cancellationToken).ConfigureAwait(false);
        if (keyed is not null)
        {
            PostgresIdempotencyStore.EnsureSamePayload(keyed, command.PayloadHash);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Deserialize(keyed.ResponseSnapshotJson);
        }

        List<IdempotencyKeyEntity> taskRecords = await context.IdempotencyKeys
            .AsNoTracking()
            .Where(record => record.Scope == Scope
                && record.Key.StartsWith(command.TaskScope))
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (IdempotencyKeyEntity taskRecord in taskRecords)
        {
            TaskIntakeOutcome replay = Deserialize(taskRecord.ResponseSnapshotJson);
            if (replay.IvrCallJobId is not null)
            {
                PostgresIdempotencyStore.EnsureSamePayload(taskRecord, command.PayloadHash);
                context.IdempotencyKeys.Add(CreateIdempotency(command, replay));
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return replay;
            }
        }

        TaskIntakePersistencePlan plan = await factory(cancellationToken).ConfigureAwait(false);
        InMemoryTaskIntakeStore.ValidatePlan(plan);
        context.IdempotencyKeys.Add(CreateIdempotency(command, plan.Outcome));
        if (plan.Task is not null)
        {
            context.ConfirmationTasks.Add(plan.Task);
            context.CallJobs.Add(plan.CallJob!);
            context.TaskIntakeOutbox.Add(plan.Outbox!);
        }

        context.AuditLog.Add(TaskIntakeAudit.CreateEntity(
            command,
            plan.Outcome,
            timeProvider.GetUtcNow()));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return plan.Outcome;
    }

    private static async Task AcquireLocksAsync(
        IvrDbContext context,
        TaskIntakeCommand command,
        CancellationToken cancellationToken)
    {
        string[] locks = [command.ScopedIdempotencyKey, command.TaskScope];
        foreach (string key in locks.Order(StringComparer.Ordinal))
        {
            await PostgresIdempotencyStore.AcquireKeyLockAsync(
                context,
                string.Concat(Scope, ":", key),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private IdempotencyKeyEntity CreateIdempotency(
        TaskIntakeCommand command,
        TaskIntakeOutcome outcome) => new()
        {
            Scope = Scope,
            Key = command.ScopedIdempotencyKey,
            PayloadHash = command.PayloadHash,
            ResponseSnapshotJson = JsonSerializer.Serialize(outcome, SerializerOptions),
            CreatedAt = timeProvider.GetUtcNow(),
            RetentionClass = RetentionDataClasses.IdempotencyKey,
        };

    private static TaskIntakeOutcome Deserialize(string snapshot) =>
        JsonSerializer.Deserialize<TaskIntakeOutcome>(snapshot, SerializerOptions)
        ?? throw new InvalidOperationException(
            "The persisted task-intake response could not be restored.");
}

internal static class TaskIntakeAudit
{
    public static AuditEvent Create(
        TaskIntakeCommand command,
        TaskIntakeOutcome outcome,
        DateTimeOffset occurredAt) =>
        new(
            "order-core",
            Action(outcome),
            string.Concat("confirmation-task:", command.Source.Task_id),
            outcome.Decision,
            command.CorrelationId,
            SafeData(command, outcome, occurredAt));

    public static AuditLogEntity CreateEntity(
        TaskIntakeCommand command,
        TaskIntakeOutcome outcome,
        DateTimeOffset occurredAt)
    {
        Dictionary<string, object?> data = SafeData(command, outcome, occurredAt);
        string dataJson = JsonSerializer.Serialize(data);
        PiiGuard.EnsureSafeText(dataJson);
        return new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "order-core",
            ActorType = "service",
            Action = Action(outcome),
            TargetType = "confirmation-task",
            TargetId = command.Source.Task_id,
            Reason = outcome.Decision,
            CorrelationId = command.CorrelationId,
            DataJson = dataJson,
            CreatedAt = occurredAt,
        };
    }

    private static string Action(TaskIntakeOutcome outcome) =>
        outcome.IvrCallJobId is null
            ? "TASK_INTAKE_REJECTED_OR_HELD"
            : "TASK_INTAKE_ACCEPTED";

    private static Dictionary<string, object?> SafeData(
        TaskIntakeCommand command,
        TaskIntakeOutcome outcome,
        DateTimeOffset occurredAt) =>
        new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["decision"] = outcome.Decision,
            ["program_code"] = command.Source.Program_code.ToString(),
            ["payment_method"] = command.Source.Payment_method_snapshot.ToString(),
            ["execution_mode"] = command.ExecutionMode.ToString(),
            ["payload_sha256"] = command.PayloadHash,
            ["failure_code"] = outcome.FailureCode,
            ["occurred_at"] = occurredAt,
        };
}
