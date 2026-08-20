using System.Data;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Scheduling;

public sealed record SchedulerDispatchLease(
    string JobId,
    string AttemptId,
    int AttemptNumber,
    DateTimeOffset DueAt,
    DateTimeOffset Deadline,
    string SimChannelId,
    Guid LeaseToken,
    long FencingGeneration,
    DateTimeOffset LeaseExpiresAt,
    string AdapterMode,
    string ProviderName);

public interface IPostgresSchedulerStore
{
    public Task<SchedulerDispatchLease?> TryClaimDueDispatchAsync(
        string workerId,
        string executionMode,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    public Task<int> QuarantineExpiredLeasesAsync(
        DateTimeOffset detectedAt,
        TimeSpan quarantineDuration,
        int batchSize,
        CancellationToken cancellationToken = default);

    public Task<int> CloseMissedDeadlinesAsync(
        DateTimeOffset detectedAt,
        int batchSize,
        CancellationToken cancellationToken = default);
}

public sealed class PostgresSchedulerStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IPostgresSchedulerStore
{
    public async Task<SchedulerDispatchLease?> TryClaimDueDispatchAsync(
        string workerId,
        string executionMode,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(executionMode);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(leaseDuration, TimeSpan.Zero);
        bool mockExecution = string.Equals(
            executionMode,
            "MOCK",
            StringComparison.OrdinalIgnoreCase);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        await context.SimChannels
            .Where(channel => channel.Status == "QUARANTINED"
                && channel.QuarantineUntil <= now
                && channel.LeaseToken == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(channel => channel.Status, "IDLE")
                    .SetProperty(channel => channel.QuarantineUntil, (DateTimeOffset?)null)
                    .SetProperty(channel => channel.DisabledReason, (string?)null),
                cancellationToken)
            .ConfigureAwait(false);
        CallJobEntity? job = await context.CallJobs.FromSqlInterpolated($$"""
            SELECT job.*
            FROM ivr_call_jobs job
            JOIN ivr_confirmation_tasks task ON task.task_id = job.task_id
            CROSS JOIN LATERAL (
                SELECT COUNT(*)::integer AS counted
                FROM ivr_call_attempts attempt
                WHERE attempt.ivr_call_job_id = job.ivr_call_job_id
                  AND attempt.is_counted_customer_attempt IS TRUE
            ) progress
            WHERE job.eligible IS TRUE
              AND (({{mockExecution}} IS TRUE
                    AND job.status = 'DRY_RUN'
                    AND job.queue_status = 'HELD_MOCK')
                   OR ({{mockExecution}} IS FALSE
                    AND job.status = 'READY_FOR_SCHEDULER'
                    AND job.queue_status = 'QUEUED'))
              AND progress.counted < job.max_attempts
              AND ((job.attempt_schedule_json ->> progress.counted)::timestamptz) <= {{now}}
              AND ((job.attempt_schedule_json ->> progress.counted)::timestamptz) < job.expires_at
              AND job.expires_at > {{now}}
              AND NOT EXISTS (
                  SELECT 1 FROM ivr_call_results result
                  WHERE result.ivr_call_job_id = job.ivr_call_job_id
                    AND result.is_final_for_ivr IS TRUE
              )
              AND NOT EXISTS (
                  SELECT 1 FROM ivr_call_attempts active_attempt
                  WHERE active_attempt.ivr_call_job_id = job.ivr_call_job_id
                    AND active_attempt.status IN (
                        'LEASED_PENDING_DISPATCH', 'DIALING', 'ACTIVE_CALL')
              )
              AND NOT EXISTS (
                  SELECT 1 FROM ivr_capacity_incidents incident
                  WHERE incident.status = 'OPEN'
                    AND incident.hold_new_calls IS TRUE
                    AND incident.scope = 'ADMIN_QUEUE_PAUSE'
              )
            ORDER BY job.expires_at,
                     CASE job.program_type
                         WHEN 'GOLDEN_HOUR' THEN 0
                         WHEN 'TWENTY_FOUR_SEVEN' THEN 1
                         ELSE 2
                     END,
                     ((job.attempt_offsets_seconds_json ->> progress.counted)::integer),
                     jsonb_array_length(COALESCE(task.risk_flags_json, '[]'::jsonb)) DESC,
                     job.created_at,
                     job.ivr_call_job_id
            FOR UPDATE OF job SKIP LOCKED
            LIMIT 1
            """).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (job is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        SimChannelEntity? channel = await context.SimChannels.FromSqlInterpolated($$"""
            SELECT channel.*
            FROM ivr_sim_channels channel
            WHERE channel.enabled IS TRUE
              AND channel.execution_mode = {{executionMode}}
              AND channel.status = 'IDLE'
              AND channel.lease_token IS NULL
              AND (channel.cooldown_until IS NULL OR channel.cooldown_until <= {{now}})
              AND channel.quarantine_until IS NULL
            ORDER BY channel.fail_count, channel.sim_channel_id
            FOR UPDATE OF channel SKIP LOCKED
            LIMIT 1
            """).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (channel is null)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        int completedCustomerAttempts = await context.CallAttempts
            .CountAsync(
                attempt => attempt.IvrCallJobId == job.IvrCallJobId
                    && attempt.IsCountedCustomerAttempt,
                cancellationToken)
            .ConfigureAwait(false);
        int attemptNumber = completedCustomerAttempts + 1;
        DateTimeOffset[] schedule = DeserializeSchedule(job.AttemptScheduleJson, job.MaxAttempts);
        DateTimeOffset dueAt = schedule[attemptNumber - 1];
        Guid leaseToken = Guid.NewGuid();
        DateTimeOffset leaseExpiresAt = now.Add(leaseDuration);
        channel.Status = "RESERVED";
        channel.ActiveCallJobId = job.IvrCallJobId;
        channel.LeaseToken = leaseToken;
        channel.LeaseFencingGeneration++;
        channel.LeasedByWorkerId = workerId;
        channel.LeaseAcquiredAt = now;
        channel.LeaseExpiresAt = leaseExpiresAt;
        string attemptId = string.Concat("ATTEMPT-", Guid.NewGuid().ToString("N"));
        context.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = attemptId,
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            AttemptNumber = attemptNumber,
            MaxAttemptsSnapshot = job.MaxAttempts,
            ScheduledAt = dueAt,
            ScheduledWindowExpiresAt = job.ExpiresAt,
            Status = "LEASED_PENDING_DISPATCH",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = false,
            TechnicalRetryCount = 0,
            NoAnswer = false,
            InvalidPhone = false,
            SimChannelId = channel.SimChannelId,
            PolicyVersion = job.AttemptPolicyCode,
            ScriptVersion = job.ScriptVersion,
            EvidenceRefsJson = JsonSerializer.Serialize(new[]
            {
                string.Concat("evidence://ivr/p2-3/dispatch/", attemptId),
            }),
        });
        job.Status = "DISPATCH_LEASED";
        job.QueueStatus = "LEASED";
        context.AuditLog.Add(CreateAudit(
            "SCHEDULER_DISPATCH_LEASED",
            "call-attempt",
            attemptId,
            job.TaskId,
            now,
            new Dictionary<string, object?>
            {
                ["job_id"] = job.IvrCallJobId,
                ["attempt_number"] = attemptNumber,
                ["due_at"] = dueAt,
                ["deadline"] = job.ExpiresAt,
                ["sim_channel_id"] = channel.SimChannelId,
                ["fencing_generation"] = channel.LeaseFencingGeneration,
                ["is_counted_customer_attempt"] = false,
            }));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        // W-0041 left ivr_call_attempts_total declared with no call site, so confirm/cancel/
        // no-answer rates (ARCH-06 section 1) had no denominator. Counted here, AFTER the commit:
        // an attempt counted before it durably exists inflates the metric relative to the database
        // every time a transaction rolls back, and a rate whose denominator is bigger than reality
        // reads as better performance than reality.
        //
        // is_counted_customer_attempt is false at dispatch and only becomes true when the result
        // normalizes (DT-02). Tagging it here as the attempt's state at dispatch, not as a
        // prediction, keeps the two counts honest about what each moment knows.
        IvrTelemetry.RecordAttempt(
            (TelemetryTags.AttemptPolicyVersion, job.AttemptPolicyCode),
            (TelemetryTags.AttemptNumber, attemptNumber),
            (TelemetryTags.Counted, false));

        return new SchedulerDispatchLease(
            job.IvrCallJobId,
            attemptId,
            attemptNumber,
            dueAt,
            job.ExpiresAt,
            channel.SimChannelId,
            leaseToken,
            channel.LeaseFencingGeneration,
            leaseExpiresAt,
            channel.AdapterMode,
            channel.ProviderName);
    }

    public async Task<int> QuarantineExpiredLeasesAsync(
        DateTimeOffset detectedAt,
        TimeSpan quarantineDuration,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quarantineDuration, TimeSpan.Zero);
        ValidateBatchSize(batchSize);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        List<SimChannelEntity> channels = await context.SimChannels.FromSqlInterpolated($$"""
            SELECT channel.*
            FROM ivr_sim_channels channel
            WHERE channel.lease_token IS NOT NULL
              AND channel.lease_expires_at <= {{detectedAt}}
              AND channel.status IN ('RESERVED', 'ACTIVE_CALL')
            ORDER BY channel.lease_expires_at, channel.sim_channel_id
            FOR UPDATE OF channel SKIP LOCKED
            LIMIT {{batchSize}}
            """).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (SimChannelEntity channel in channels)
        {
            string? activeJobId = channel.ActiveCallJobId;
            if (!string.IsNullOrWhiteSpace(activeJobId))
            {
                CallJobEntity? job = await context.CallJobs.SingleOrDefaultAsync(
                    candidate => candidate.IvrCallJobId == activeJobId,
                    cancellationToken).ConfigureAwait(false);
                if (job is not null && job.ClosedAt is null)
                {
                    job.Status = "HELD_ADMIN_REVIEW";
                    job.QueueStatus = "HELD_LEASE_RECOVERY";
                }

                CallAttemptEntity? attempt = await context.CallAttempts
                    .Where(candidate => candidate.IvrCallJobId == activeJobId
                        && candidate.SimChannelId == channel.SimChannelId
                        && (candidate.Status == "LEASED_PENDING_DISPATCH"
                            || candidate.Status == "ACTIVE_CALL"))
                    .OrderByDescending(candidate => candidate.ScheduledAt)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (attempt is not null)
                {
                    attempt.Status = "RECOVERY_REQUIRED";
                    attempt.BlockedReason = "LEASE_EXPIRED_RECONCILIATION_REQUIRED";
                }
            }

            channel.Status = "QUARANTINED";
            channel.FailCount++;
            channel.QuarantineUntil = detectedAt.Add(quarantineDuration);
            channel.DisabledReason = "LEASE_EXPIRED_RECONCILIATION_REQUIRED";
            channel.ActiveCallJobId = null;
            channel.LeaseToken = null;
            channel.LeaseFencingGeneration++;
            channel.LeasedByWorkerId = null;
            channel.LeaseAcquiredAt = null;
            channel.LeaseExpiresAt = null;
            context.AuditLog.Add(CreateAudit(
                "SIM_CHANNEL_LEASE_QUARANTINED",
                "sim-channel",
                channel.SimChannelId,
                channel.SimChannelId,
                detectedAt,
                new Dictionary<string, object?>
                {
                    ["active_job_id"] = activeJobId,
                    ["fencing_generation"] = channel.LeaseFencingGeneration,
                    ["reason"] = channel.DisabledReason,
                }));

            // W-0041 / P6-2, DT-04. The auto-disable moment is the one ops must be woken for, and
            // it is only observable here: the row afterwards shows a channel that is quarantined,
            // never that it just became so. Counting at the transition is what lets an alert say
            // "three in ten minutes" instead of "some channels are down".
            Observability.IvrTelemetry.RecordChannelQuarantine(
                (Observability.TelemetryTags.ReasonCode, channel.DisabledReason));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return channels.Count;
    }

    public async Task<int> CloseMissedDeadlinesAsync(
        DateTimeOffset detectedAt,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ValidateBatchSize(batchSize);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        List<CallJobEntity> jobs = await context.CallJobs.FromSqlInterpolated($$"""
            SELECT job.*
            FROM ivr_call_jobs job
            WHERE job.eligible IS TRUE
              AND job.closed_at IS NULL
              AND job.expires_at <= {{detectedAt}}
              AND job.status IN (
                  'READY_FOR_SCHEDULER',
                  'DISPATCH_LEASED',
                  'DRY_RUN',
                  'HELD_ADMIN_REVIEW')
              AND NOT EXISTS (
                  SELECT 1 FROM ivr_call_results result
                  WHERE result.ivr_call_job_id = job.ivr_call_job_id
                    AND result.is_final_for_ivr IS TRUE
              )
              AND NOT EXISTS (
                  SELECT 1 FROM ivr_call_attempts active_attempt
                  WHERE active_attempt.ivr_call_job_id = job.ivr_call_job_id
                    AND active_attempt.status IN (
                        'LEASED_PENDING_DISPATCH', 'DIALING', 'ACTIVE_CALL')
              )
            ORDER BY job.expires_at, job.ivr_call_job_id
            FOR UPDATE OF job SKIP LOCKED
            LIMIT {{batchSize}}
            """).ToListAsync(cancellationToken).ConfigureAwait(false);
        int activeChannels = await context.SimChannels.CountAsync(
            channel => channel.Enabled
                && channel.Status != "DISABLED"
                && channel.Status != "QUARANTINED"
                && channel.Status != "HEALTH_FAILED",
            cancellationToken).ConfigureAwait(false);
        int pendingJobs = await context.CallJobs.CountAsync(
            candidate => candidate.Eligible && candidate.ClosedAt == null,
            cancellationToken).ConfigureAwait(false);
        string[] jobIds = [.. jobs.Select(job => job.IvrCallJobId)];
        Dictionary<string, int> lastAttemptNumbers = await context.CallAttempts
            .AsNoTracking()
            .Where(attempt => jobIds.Contains(attempt.IvrCallJobId))
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new
            {
                JobId = group.Key,
                AttemptNumber = group.Max(attempt => attempt.AttemptNumber),
            })
            .ToDictionaryAsync(
                item => item.JobId,
                item => item.AttemptNumber,
                StringComparer.Ordinal,
                cancellationToken)
            .ConfigureAwait(false);
        var closed = new List<(string Program, string ResultStatus)>(jobs.Count);
        foreach (CallJobEntity job in jobs)
        {
            string incidentId = string.Concat("CAP-", Guid.NewGuid().ToString("N"));
            context.CapacityIncidents.Add(new CapacityIncidentEntity
            {
                CapacityIncidentId = incidentId,
                SessionId = string.Concat("SCHED-DEADLINE-", job.IvrCallJobId),
                ProgramCode = job.ProgramType,
                Status = "OPEN",
                Scope = "SCHEDULER_DEADLINE",
                HoldNewCalls = false,
                ActiveSimCount = activeChannels,
                PendingCallJobs = pendingJobs,
                ExpiredCallJobs = 1,
                MissedDeadlineCount = 1,
                ShortageReason = "NO_DISPATCH_BEFORE_DEADLINE",
                OpenedAt = detectedAt,
                Reason = "IVR_CAPACITY_EXCEPTION",
            });
            string resultId = string.Concat("RESULT-", Guid.NewGuid().ToString("N"));
            string evidenceRef = string.Concat(
                "evidence://ivr/p2-3/capacity-miss/",
                job.IvrCallJobId);
            AuditLogEntity audit = CreateAudit(
                "SCHEDULER_DEADLINE_MISSED",
                "call-job",
                job.IvrCallJobId,
                job.TaskId,
                detectedAt,
                new Dictionary<string, object?>
                {
                    ["capacity_incident_id"] = incidentId,
                    ["result_id"] = resultId,
                    ["deadline"] = job.ExpiresAt,
                    ["is_counted_customer_attempt"] = false,
                });
            string auditRef = string.Concat("audit://ivr/", audit.AuditId.ToString("D"));
            var normalized = new NormalizedResult(
                IvrResultType.IvrCapacityException,
                false,
                true,
                "NO_DISPATCH_BEFORE_DEADLINE",
                null,
                "CAPACITY_UNAVAILABLE",
                CoreActionRecommendation.RevalidateAndHoldAdminReview,
                true,
                false,
                0);
            context.CallResults.Add(new CallResultEntity
            {
                IvrCallResultId = resultId,
                IvrCallJobId = job.IvrCallJobId,
                TaskId = job.TaskId,
                OfficialOrderId = job.OfficialOrderId,
                OrderVersionSnapshot = job.OrderVersionSnapshot,
                OrderVersionSeenByIvr = job.OrderVersionSnapshot,
                FinalResultStatus = "IVR_CAPACITY_EXCEPTION",
                ResultType = "IVR_CAPACITY_EXCEPTION",
                ResultReason = "NO_DISPATCH_BEFORE_DEADLINE",
                IsCountedCustomerAttempt = false,
                IsFinalForIvr = true,
                RecommendedCoreAction = "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
                CoreOrderHandoffRequired = true,
                HumanReviewRequired = true,
                InputSignalOnly = true,
                NoDirectOrderUpdate = true,
                NoPaymentOrRevenueEffect = true,
                CreatedAt = detectedAt,
                EvidenceRefsJson = JsonSerializer.Serialize(new[]
                {
                    evidenceRef,
                }),
                AuditRefsJson = JsonSerializer.Serialize(new[] { auditRef }),
            });
            int attemptNumber = lastAttemptNumbers.TryGetValue(
                job.IvrCallJobId,
                out int lastAttemptNumber)
                ? Math.Min(checked(lastAttemptNumber + 1), job.MaxAttempts)
                : 1;
            context.ResultCallbacks.Add(CallbackOutboxSnapshotFactory.Create(
                resultId,
                job,
                attemptNumber,
                normalized,
                evidenceRef,
                auditRef,
                detectedAt));
            job.CapacityIncidentId = incidentId;
            job.Status = "CAPACITY_MISSED";
            job.QueueStatus = "CLOSED_CAPACITY";
            job.ClosedAt = detectedAt;
            job.ClosedReason = "IVR_CAPACITY_EXCEPTION";
            context.AuditLog.Add(audit);
            closed.Add((job.ProgramType, normalized.ResultStatus));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        // Recorded after the commit and once per job, for the same reason as the dispatch counter:
        // a metric moved before the rows are durable runs ahead of the database on every rollback.
        //
        // Two instruments, because this method is the ONE path where a job reaches a final result
        // without passing through normalization -- the scheduler writes the IVR_CAPACITY_EXCEPTION
        // row itself. Recording only the deadline counter would leave every capacity miss out of
        // ivr_call_results_total, and a confirm_rate whose denominator omits the failures reads
        // higher than the truth. The gap is largest exactly when capacity is worst.
        foreach ((string program, string resultStatus) in closed)
        {
            IvrTelemetry.RecordMissedDeadline(
                (TelemetryTags.Program, program),
                (TelemetryTags.ReasonCode, "NO_DISPATCH_BEFORE_DEADLINE"));
            IvrTelemetry.RecordResult(
                (TelemetryTags.ResultType, resultStatus),
                (TelemetryTags.Counted, false));
        }

        return jobs.Count;
    }

    private static DateTimeOffset[] DeserializeSchedule(string json, int expectedCount)
    {
        DateTimeOffset[] schedule;
        try
        {
            schedule = JsonSerializer.Deserialize<DateTimeOffset[]>(json) ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Stored attempt schedule is unreadable.",
                exception);
        }

        if (schedule.Length != expectedCount)
        {
            throw new InvalidOperationException(
                "Stored attempt schedule does not match the policy snapshot.");
        }

        return schedule;
    }

    private static AuditLogEntity CreateAudit(
        string action,
        string targetType,
        string targetId,
        string correlationId,
        DateTimeOffset occurredAt,
        IReadOnlyDictionary<string, object?> data) => new()
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-scheduler",
            ActorType = "service",
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Reason = action,
            CorrelationId = correlationId,
            DataJson = JsonSerializer.Serialize(data),
            CreatedAt = occurredAt,
        };

    private static void ValidateBatchSize(int batchSize)
    {
        if (batchSize is < 1 or > 512)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }
    }
}
