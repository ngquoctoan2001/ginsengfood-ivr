using System.Data;
using System.Text.Json;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Repositories;

public sealed record EligibilityTaskRecord(
    ConfirmationTaskEntity Task,
    CallJobEntity CallJob,
    TaskIntakeOutboxEntity Outbox);

public interface IEligibilityRepository
{
    public Task<EligibilityTaskRecord?> FindAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    public Task<EligibilityEvaluation> PersistAsync(
        string taskId,
        EligibilityEvaluation evaluation,
        EligibilityCapacitySnapshot capacity,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed class PostgresEligibilityRepository(
    IDbContextFactory<IvrDbContext> dbContextFactory) : IEligibilityRepository
{
    public async Task<EligibilityTaskRecord?> FindAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(taskId);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        ConfirmationTaskEntity? task = await context.ConfirmationTasks
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
        if (task is null)
        {
            return null;
        }

        CallJobEntity job = await context.CallJobs
            .AsNoTracking()
            .SingleAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
        TaskIntakeOutboxEntity outbox = await context.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
        return new EligibilityTaskRecord(task, job, outbox);
    }

    public async Task<EligibilityEvaluation> PersistAsync(
        string taskId,
        EligibilityEvaluation evaluation,
        EligibilityCapacitySnapshot capacity,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        Validate(taskId, evaluation, capacity, correlationId);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        await PostgresIdempotencyStore.AcquireKeyLockAsync(
            context,
            string.Concat("eligibility:", taskId),
            cancellationToken).ConfigureAwait(false);

        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
        CallJobEntity job = await context.CallJobs
            .SingleAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
        TaskIntakeOutboxEntity outbox = await context.TaskIntakeOutbox
            .SingleAsync(item => item.TaskId == taskId, cancellationToken)
            .ConfigureAwait(false);
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
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return evaluation with { CapacityIncidentId = job.CapacityIncidentId };
            }

            throw new InvalidOperationException(
                "The task already has a different eligibility decision.");
        }

        EligibilityPersistenceArtifacts artifacts = EligibilityPersistence.Apply(
            task,
            job,
            outbox,
            evaluation,
            capacity,
            correlationId);
        if (artifacts.CapacityIncident is not null)
        {
            context.CapacityIncidents.Add(artifacts.CapacityIncident);
        }

        if (artifacts.ReviewItem is not null)
        {
            context.ReviewItems.Add(artifacts.ReviewItem);
        }

        context.EvidenceLinks.AddRange(artifacts.EvidenceLinks);
        context.AuditLog.Add(artifacts.Audit);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return artifacts.Evaluation;
    }

    internal static void Validate(
        string taskId,
        EligibilityEvaluation evaluation,
        EligibilityCapacitySnapshot capacity,
        string correlationId)
    {
        ValidateKey(taskId);
        ArgumentNullException.ThrowIfNull(evaluation);
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        PiiGuard.EnsureSafeText(correlationId);
        PiiGuard.EnsureSafeText(JsonSerializer.Serialize(evaluation));
        PiiGuard.EnsureSafeText(JsonSerializer.Serialize(capacity));
    }

    private static void ValidateKey(string taskId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        PiiGuard.EnsureSafeText(taskId);
    }
}

internal sealed record EligibilityPersistenceArtifacts(
    EligibilityEvaluation Evaluation,
    CapacityIncidentEntity? CapacityIncident,
    ReviewItemEntity? ReviewItem,
    IReadOnlyList<EvidenceLinkEntity> EvidenceLinks,
    AuditLogEntity Audit);

internal static class EligibilityPersistence
{
    public static EligibilityPersistenceArtifacts Apply(
        ConfirmationTaskEntity task,
        CallJobEntity job,
        TaskIntakeOutboxEntity outbox,
        EligibilityEvaluation evaluation,
        EligibilityCapacitySnapshot capacity,
        string correlationId)
    {
        bool mock = string.Equals(job.Status, "DRY_RUN", StringComparison.Ordinal);
        string? capacityIncidentId = null;
        CapacityIncidentEntity? capacityIncident = null;
        if (string.Equals(
                evaluation.Decision,
                EligibilityDecisions.CapacityException,
                StringComparison.Ordinal))
        {
            capacityIncidentId = string.Concat("CAP-", Guid.NewGuid().ToString("N"));
            capacityIncident = new CapacityIncidentEntity
            {
                CapacityIncidentId = capacityIncidentId,
                SessionId = capacity.SessionId,
                ProgramCode = task.ProgramType,
                Status = "OPEN",
                Scope = "ELIGIBILITY_DEADLINE",
                HoldNewCalls = false,
                ActiveSimCount = capacity.ActiveSimCount,
                PendingCallJobs = capacity.PendingCallJobs,
                ExpiredCallJobs = capacity.ExpiredCallJobs,
                MissedDeadlineCount = capacity.MissedDeadlineCount,
                ShortageReason = capacity.ShortageReason,
                OpenedAt = evaluation.EvaluatedAt,
                Reason = EligibilityReasonCodes.CapacityDeadlineUnavailable,
            };
        }

        EligibilityEvaluation persisted = evaluation with
        {
            CapacityIncidentId = capacityIncidentId,
        };
        string reasonsJson = JsonSerializer.Serialize(persisted.Reasons);
        string evidenceJson = MergeEvidence(task.EvidenceRefsJson, persisted.EvidenceRefs);
        PiiGuard.EnsureSafeText(reasonsJson);
        PiiGuard.EnsureSafeText(evidenceJson);

        task.EligibilityDecision = persisted.Decision;
        task.BlockedReasonsJson = reasonsJson;
        task.EvidenceRefsJson = evidenceJson;
        job.Eligible = persisted.Eligible;
        job.EligibilityDecision = persisted.Decision;
        job.CapacityIncidentId = capacityIncidentId;
        job.EvidenceRefsJson = evidenceJson;
        ApplyJobState(job, persisted, mock);
        if (!mock)
        {
            outbox.Status = "PUBLISHED";
            outbox.PublishedAt = persisted.EvaluatedAt;
        }

        EvidenceLinkEntity[] links = persisted.EvidenceRefs
            .Distinct(StringComparer.Ordinal)
            .Select(evidenceRef =>
            {
                PiiGuard.EnsureSafeText(evidenceRef);
                return new EvidenceLinkEntity
                {
                    OwnerTable = "ivr_call_jobs",
                    OwnerId = job.IvrCallJobId,
                    EvidenceRef = evidenceRef,
                    CreatedAt = persisted.EvaluatedAt,
                    AcceptedAt = persisted.EvaluatedAt,
                };
            })
            .ToArray();
        string auditData = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["decision"] = persisted.Decision,
            ["eligible"] = persisted.Eligible,
            ["reason_codes"] = persisted.Reasons.Select(reason => reason.Code).ToArray(),
            ["advisories"] = persisted.Advisories,
            ["capacity_incident_id"] = capacityIncidentId,
            ["is_counted_customer_attempt"] = false,
        });
        PiiGuard.EnsureSafeText(auditData);
        var audit = new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-eligibility",
            ActorType = "service",
            Action = "ELIGIBILITY_EVALUATED",
            TargetType = "confirmation-task",
            TargetId = task.TaskId,
            Reason = persisted.Decision,
            CorrelationId = correlationId,
            DataJson = auditData,
            CreatedAt = persisted.EvaluatedAt,
        };
        ReviewItemEntity? reviewItem = string.Equals(
            persisted.Decision,
            EligibilityDecisions.HeldAdminReview,
            StringComparison.Ordinal)
            ? new ReviewItemEntity
            {
                ReviewItemId = string.Concat("REVIEW-ELIG-", Guid.NewGuid().ToString("N")),
                SourceType = "ELIGIBILITY_DECISION",
                SourceId = task.TaskId,
                Reason = persisted.Reasons.Count > 0
                    ? persisted.Reasons[0].Code
                    : EligibilityDecisions.HeldAdminReview,
                Status = "OPEN",
                CorrelationId = correlationId,
                CreatedAt = persisted.EvaluatedAt,
            }
            : null;
        return new EligibilityPersistenceArtifacts(
            persisted,
            capacityIncident,
            reviewItem,
            links,
            audit);
    }

    private static void ApplyJobState(
        CallJobEntity job,
        EligibilityEvaluation evaluation,
        bool mock)
    {
        switch (evaluation.Decision)
        {
            case EligibilityDecisions.Eligible:
                job.Status = mock ? "DRY_RUN" : "READY_FOR_SCHEDULER";
                job.QueueStatus = mock ? "HELD_MOCK" : "QUEUED";
                break;
            case EligibilityDecisions.CapacityException:
                job.Status = "CAPACITY_HELD";
                job.QueueStatus = "HELD_CAPACITY";
                break;
            case EligibilityDecisions.HeldAdminReview:
                job.Status = "HELD_ADMIN_REVIEW";
                job.QueueStatus = "HELD_ADMIN_REVIEW";
                break;
            case EligibilityDecisions.SkippedTrustedCustomer:
                job.Status = "SKIPPED";
                job.QueueStatus = "SKIPPED";
                job.ClosedAt = evaluation.EvaluatedAt;
                job.ClosedReason = evaluation.Decision;
                break;
            default:
                job.Status = "BLOCKED";
                job.QueueStatus = "BLOCKED";
                job.ClosedAt = evaluation.EvaluatedAt;
                job.ClosedReason = evaluation.Decision;
                break;
        }
    }

    private static string MergeEvidence(
        string? storedJson,
        IReadOnlyList<string> evaluationRefs)
    {
        string[] stored;
        try
        {
            stored = string.IsNullOrWhiteSpace(storedJson)
                ? []
                : JsonSerializer.Deserialize<string[]>(storedJson) ?? [];
        }
        catch (JsonException)
        {
            stored = [];
        }

        return JsonSerializer.Serialize(stored
            .Concat(evaluationRefs)
            .Distinct(StringComparer.Ordinal));
    }
}
