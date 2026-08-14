using System.Data;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Repositories;

public sealed class NormalizationOptions
{
    public const string SectionName = "Ivr:Normalization";

    public bool Enabled { get; set; }

    public int BatchSize { get; set; } = 32;

    public int PollIntervalMilliseconds { get; set; } = 1000;
}

public sealed class NormalizationOptionsValidator : IValidateOptions<NormalizationOptions>
{
    public ValidateOptionsResult Validate(string? name, NormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.BatchSize is < 1 or > 512)
        {
            return ValidateOptionsResult.Fail("BatchSize must be between 1 and 512.");
        }

        return options.PollIntervalMilliseconds is < 100 or > 60000
            ? ValidateOptionsResult.Fail(
                "PollIntervalMilliseconds must be between 100 and 60000.")
            : ValidateOptionsResult.Success;
    }
}

public sealed record NormalizationPersistenceResult(
    string RawEventId,
    string ResultId,
    string ResultStatus,
    bool IsCounted,
    bool IsFinal,
    bool TechnicalRetryAllowed,
    int TechnicalRetryCount,
    bool HumanReviewRequired);

public interface IResultRepository
{
    public Task<NormalizationPersistenceResult?> NormalizeNextAsync(
        string workerId,
        CancellationToken cancellationToken = default);
}

public sealed class ResultRepository(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IRawEventRepository rawEventRepository,
    IOptions<SchedulerOptions> schedulerOptions,
    SchedulerExecutionContext executionContext,
    TimeProvider timeProvider) : IResultRepository
{
    public async Task<NormalizationPersistenceResult?> NormalizeNextAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        RawCallEventEntity? rawEvent = await context.RawCallEvents.FromSqlRaw("""
            SELECT raw_event.*
            FROM ivr_raw_call_events raw_event
            JOIN ivr_call_attempts attempt
              ON attempt.ivr_call_attempt_id = raw_event.ivr_call_attempt_id
            WHERE attempt.status = 'PROVIDER_EVENT_PENDING_NORMALIZATION'
              AND attempt.raw_call_event_id = raw_event.raw_event_id
              AND NOT EXISTS (
                  SELECT 1
                  FROM ivr_call_results result
                  WHERE result.ivr_call_result_id = CONCAT('RESULT-', raw_event.raw_event_id)
              )
            ORDER BY raw_event.received_at, raw_event.raw_event_id
            FOR UPDATE OF raw_event SKIP LOCKED
            LIMIT 1
            """).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (rawEvent is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        CallAttemptEntity attempt = await context.CallAttempts.FromSqlInterpolated($$"""
            SELECT attempt.*
            FROM ivr_call_attempts attempt
            WHERE attempt.ivr_call_attempt_id = {{rawEvent.IvrCallAttemptId}}
            FOR UPDATE OF attempt
            """).SingleAsync(cancellationToken).ConfigureAwait(false);
        CallJobEntity job = await context.CallJobs.FromSqlInterpolated($$"""
            SELECT job.*
            FROM ivr_call_jobs job
            WHERE job.ivr_call_job_id = {{rawEvent.IvrCallJobId}}
            FOR UPDATE OF job
            """).SingleAsync(cancellationToken).ConfigureAwait(false);
        ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()
            .SingleAsync(
                candidate => candidate.TaskId == attempt.TaskId,
                cancellationToken)
            .ConfigureAwait(false);
        int priorTechnicalRetryCount = await (
            from technical in context.TechnicalExceptions.AsNoTracking()
            join priorAttempt in context.CallAttempts.AsNoTracking()
                on technical.IvrCallAttemptId equals priorAttempt.IvrCallAttemptId
            where priorAttempt.IvrCallJobId == attempt.IvrCallJobId
                && priorAttempt.AttemptNumber == attempt.AttemptNumber
            select technical.TechnicalExceptionId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        var normalizationContext = new AttemptNormalizationContext(
            attempt.AttemptNumber,
            attempt.MaxAttemptsSnapshot,
            rawEvent.ReceivedAt,
            attempt.ScheduledWindowExpiresAt,
            priorTechnicalRetryCount,
            schedulerOptions.Value.TechnicalRetryLimit);
        RawDispositionSignal signal = rawEventRepository.Parse(rawEvent);
        NormalizedResult normalized = signal.IsCapacity
            ? DispositionMapper.Capacity(normalizationContext)
            : DispositionMapper.Normalize(
                signal.Disposition,
                signal.DtmfKey,
                rawEvent.TechnicalErrorCode,
                normalizationContext);

        DateTimeOffset now = timeProvider.GetUtcNow();
        string resultId = string.Concat("RESULT-", rawEvent.RawEventId);
        string evidenceRef = string.Concat(
            "evidence://ivr/W-0022/normalization/",
            rawEvent.RawEventId);
        Guid auditId = Guid.NewGuid();
        string auditRef = string.Concat("audit://ivr/", auditId.ToString("D"));
        CallResultEntity result = CreateResult(
            resultId,
            job,
            normalized,
            evidenceRef,
            auditRef,
            now);
        context.CallResults.Add(result);
        if (normalized.IsFinal)
        {
            context.ResultCallbacks.Add(CallbackOutboxSnapshotFactory.Create(
                resultId,
                job,
                attempt,
                task,
                normalized,
                evidenceRef,
                auditRef,
                rawEvent.ReceivedAt));
        }
        if (normalized.ResultType == IvrResultType.IvrTechnicalException)
        {
            context.TechnicalExceptions.Add(new TechnicalExceptionEntity
            {
                TechnicalExceptionId = string.Concat("TECH-", rawEvent.RawEventId),
                IvrCallAttemptId = attempt.IvrCallAttemptId,
                ExceptionType = normalized.TechnicalErrorCode!,
                CustomerAttemptCounted = false,
                TechnicalRetryAllowed = normalized.TechnicalRetryAllowed,
                TechnicalRetryCount = normalized.TechnicalRetryCount,
                RetryReason = normalized.TechnicalRetryAllowed
                    ? "BOUNDED_TECHNICAL_RETRY"
                    : "TECHNICAL_RETRY_EXHAUSTED_OR_WINDOW_CLOSED",
                CorrelationId = task.CorrelationId,
                CreatedAt = now,
            });
        }

        if (normalized.HumanReviewRequired)
        {
            context.ReviewItems.Add(new ReviewItemEntity
            {
                ReviewItemId = string.Concat("REVIEW-", rawEvent.RawEventId),
                SourceType = "IVR_CALL_RESULT",
                SourceId = resultId,
                Reason = normalized.Reason,
                Status = "OPEN",
                CorrelationId = task.CorrelationId,
                CreatedAt = now,
            });
        }

        context.Evidence.Add(new EvidenceEntity
        {
            EvidenceRef = evidenceRef,
            Kind = "IVR_RESULT_NORMALIZATION",
            CorrelationId = task.CorrelationId,
            WorkId = "W-0022",
            PayloadRef = string.Concat("db://ivr_raw_call_events/", rawEvent.RawEventId),
            CreatedAt = now,
        });
        AddEvidenceLinks(
            context,
            rawEvent,
            attempt,
            resultId,
            evidenceRef,
            auditRef,
            now);
        context.AuditLog.Add(new AuditLogEntity
        {
            AuditId = auditId,
            ActorId = workerId,
            ActorType = "service",
            Action = "IVR_RESULT_NORMALIZED",
            TargetType = "call-result",
            TargetId = resultId,
            Reason = normalized.ResultStatus,
            CorrelationId = task.CorrelationId,
            DataJson = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["raw_event_id"] = rawEvent.RawEventId,
                ["attempt_id"] = attempt.IvrCallAttemptId,
                ["attempt_number"] = attempt.AttemptNumber,
                ["result_status"] = normalized.ResultStatus,
                ["is_counted_customer_attempt"] = normalized.IsCounted,
                ["is_final_for_ivr"] = normalized.IsFinal,
                ["technical_retry_allowed"] = normalized.TechnicalRetryAllowed,
                ["technical_retry_count"] = normalized.TechnicalRetryCount,
                ["human_review_required"] = normalized.HumanReviewRequired,
            }),
            CreatedAt = now,
        });

        ApplyAttemptOutcome(attempt, normalized, signal, evidenceRef, auditRef);
        ApplyJobOutcome(job, normalized, now, executionContext.ExecutionMode);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new NormalizationPersistenceResult(
            rawEvent.RawEventId,
            resultId,
            normalized.ResultStatus,
            normalized.IsCounted,
            normalized.IsFinal,
            normalized.TechnicalRetryAllowed,
            normalized.TechnicalRetryCount,
            normalized.HumanReviewRequired);
    }

    private static CallResultEntity CreateResult(
        string resultId,
        CallJobEntity job,
        NormalizedResult normalized,
        string evidenceRef,
        string auditRef,
        DateTimeOffset createdAt) => new()
        {
            IvrCallResultId = resultId,
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            OrderVersionSnapshot = job.OrderVersionSnapshot,
            OrderVersionSeenByIvr = job.OrderVersionSnapshot,
            FinalResultStatus = normalized.ResultStatus,
            ResultType = normalized.ResultStatus,
            ResultReason = normalized.Reason,
            DtmfKey = normalized.DtmfKey,
            IsCountedCustomerAttempt = normalized.IsCounted,
            IsFinalForIvr = normalized.IsFinal,
            RecommendedCoreAction = ToCoreAction(normalized.RecommendedCoreAction),
            CoreOrderHandoffRequired = normalized.IsFinal,
            HumanReviewRequired = normalized.HumanReviewRequired,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            TechnicalErrorCode = normalized.TechnicalErrorCode,
            CreatedAt = createdAt,
            EvidenceRefsJson = JsonSerializer.Serialize(new[] { evidenceRef }),
            AuditRefsJson = JsonSerializer.Serialize(new[] { auditRef }),
        };

    private static void ApplyAttemptOutcome(
        CallAttemptEntity attempt,
        NormalizedResult normalized,
        RawDispositionSignal signal,
        string evidenceRef,
        string auditRef)
    {
        attempt.Status = normalized.IsFinal
            ? "NORMALIZED_FINAL"
            : normalized.ResultType == IvrResultType.IvrTechnicalException
                ? normalized.TechnicalRetryAllowed
                    ? "NORMALIZED_TECHNICAL_RETRY"
                    : "NORMALIZED_REVIEW_REQUIRED"
                : "NORMALIZED_ATTEMPT_COMPLETE";
        attempt.ResultStatus = normalized.ResultStatus;
        attempt.DtmfKey = normalized.DtmfKey;
        attempt.IsCountedCustomerAttempt = normalized.IsCounted;
        attempt.TechnicalRetryAllowed = normalized.TechnicalRetryAllowed;
        attempt.TechnicalRetryCount = normalized.TechnicalRetryCount;
        attempt.NoAnswer = normalized.IsNoAnswer;
        attempt.InvalidPhone = normalized.IsInvalidPhone;
        attempt.TechnicalExceptionType = normalized.ResultType == IvrResultType.IvrTechnicalException
            ? normalized.TechnicalErrorCode
            : null;
        attempt.Disposition = signal.IsCapacity
            ? "CAPACITY_EXCEPTION"
            : attempt.Disposition;
        attempt.BlockedReason = normalized.HumanReviewRequired
            ? normalized.Reason
            : null;
        attempt.EvidenceRefsJson = AppendReference(attempt.EvidenceRefsJson, evidenceRef);
        attempt.AuditRefsJson = AppendReference(attempt.AuditRefsJson, auditRef);
    }

    private static void ApplyJobOutcome(
        CallJobEntity job,
        NormalizedResult normalized,
        DateTimeOffset occurredAt,
        string executionMode)
    {
        if (normalized.IsFinal)
        {
            job.Status = "RESULT_READY_FOR_CALLBACK";
            job.QueueStatus = "HELD_CALLBACK";
            job.ClosedAt = occurredAt;
            job.ClosedReason = normalized.ResultStatus;
            return;
        }

        if (normalized.ResultType == IvrResultType.IvrTechnicalException
            && !normalized.TechnicalRetryAllowed)
        {
            job.Status = "HELD_ADMIN_REVIEW";
            job.QueueStatus = "HELD_TECHNICAL_REVIEW";
            job.ClosedAt = null;
            job.ClosedReason = null;
            return;
        }

        bool mock = string.Equals(
            executionMode,
            IvrOptions.MockExecutionMode,
            StringComparison.OrdinalIgnoreCase);
        job.Status = mock ? "DRY_RUN" : "READY_FOR_SCHEDULER";
        job.QueueStatus = mock ? "HELD_MOCK" : "QUEUED";
        job.ClosedAt = null;
        job.ClosedReason = null;
    }

    private static void AddEvidenceLinks(
        IvrDbContext context,
        RawCallEventEntity rawEvent,
        CallAttemptEntity attempt,
        string resultId,
        string evidenceRef,
        string auditRef,
        DateTimeOffset createdAt)
    {
        context.EvidenceLinks.AddRange(
            CreateEvidenceLink(
                "ivr_raw_call_events",
                rawEvent.RawEventId,
                evidenceRef,
                auditRef,
                createdAt),
            CreateEvidenceLink(
                "ivr_call_attempts",
                attempt.IvrCallAttemptId,
                evidenceRef,
                auditRef,
                createdAt),
            CreateEvidenceLink(
                "ivr_call_results",
                resultId,
                evidenceRef,
                auditRef,
                createdAt));
    }

    private static EvidenceLinkEntity CreateEvidenceLink(
        string ownerTable,
        string ownerId,
        string evidenceRef,
        string auditRef,
        DateTimeOffset createdAt) => new()
        {
            OwnerTable = ownerTable,
            OwnerId = ownerId,
            EvidenceRef = evidenceRef,
            AuditRef = auditRef,
            CreatedAt = createdAt,
        };

    private static string AppendReference(string? json, string reference)
    {
        List<string> references;
        try
        {
            references = string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            references = [];
        }

        if (!references.Contains(reference, StringComparer.Ordinal))
        {
            references.Add(reference);
        }

        return JsonSerializer.Serialize(references);
    }

    private static string ToCoreAction(CoreActionRecommendation recommendation) =>
        recommendation switch
        {
            CoreActionRecommendation.RevalidateAndConfirmOrder =>
                "REVALIDATE_AND_CONFIRM_ORDER",
            CoreActionRecommendation.RevalidateAndCancelCustomerRequest =>
                "REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST",
            CoreActionRecommendation.NoStateChangeWaitForTimeout =>
                "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
            CoreActionRecommendation.RevalidateAndExpireConfirmation =>
                "REVALIDATE_AND_EXPIRE_CONFIRMATION",
            CoreActionRecommendation.RevalidateAndHoldAdminReview =>
                "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
            CoreActionRecommendation.IgnoreStaleCallback => "IGNORE_STALE_CALLBACK",
            CoreActionRecommendation.BlockDueToOperationalConstraint =>
                "BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT",
            _ => throw new InvalidOperationException("Unsupported Core action recommendation."),
        };
}
