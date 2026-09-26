using System.Data;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Telephony;
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
    string ProviderName)
{
    public string TaskId { get; init; } = string.Empty;

    public string CorrelationId { get; init; } = string.Empty;

    public string? TraceParent { get; init; }

    public string? TraceState { get; init; }
}

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

/// <param name="callingWindow">
/// Q-22.2. The calling hours, so the missed-deadline sweep can tell a job that ran out of them from
/// one that ran out of channels. Without it (tests, tools) every zero-attempt job is a capacity
/// miss, as before.
/// </param>
/// <param name="schedulerOptions">
/// Q-22.2. Supplies <see cref="SchedulerOptions.ExpectedCallDurationSeconds"/>, the least calling
/// time a job must have had for its miss to count as a capacity shortage.
/// </param>
public sealed class PostgresSchedulerStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider,
    CallingWindow? callingWindow = null,
    Microsoft.Extensions.Options.IOptions<SchedulerOptions>? schedulerOptions = null) : IPostgresSchedulerStore
{
    /// <summary>
    /// Q-22.2 (2026-09-26). The <c>ivr.reason_code</c> on <c>ivr_missed_deadline_total</c> for a job
    /// that was never dialled because the calling hours left it less than one expected call's time
    /// between arriving and its deadline - a task that arrived in the last seconds before 21:08, or
    /// after it. Not a channel shortage, so no capacity incident is opened and the counter that
    /// sizes the SIM order is not raised under the capacity reason. What Module 3 receives does not
    /// change: the result is still <c>IVR_CAPACITY_EXCEPTION</c> with its usual reason.
    /// </summary>
    public const string CallingHoursClosedBeforeDispatch = "CALLING_HOURS_CLOSED_BEFORE_DISPATCH";

    /// <summary>
    /// W-0372 / K-64 (2026-09-26). The <c>ivr.reason_code</c> on <c>ivr_missed_deadline_total</c> for
    /// a job whose window closed while it was still waiting on eligibility
    /// (<c>PENDING_ELIGIBILITY</c>): nobody ever decided whether the order could be called. Both
    /// shapes intake writes get it, the lab job and the dry run - in MOCK the eligibility loop
    /// evaluates the dry run exactly as it evaluates the lab job elsewhere, so a dry run it never
    /// reached is the same fault in a sandbox. Under the window reason these jobs sat beside
    /// customers dialled and not reached, dry runs and orders held for review, most of which need
    /// nobody, so an eligibility loop answering some orders and not others moved nothing an alert
    /// could read. What Module 3 receives does not change: the result is still
    /// <c>IVR_CONFIRMATION_WINDOW_EXPIRED</c> for <c>WINDOW_EXPIRED_BEFORE_FINAL_RESULT</c>, and the
    /// audit row already names the decision.
    /// </summary>
    public const string EligibilityNotEvaluatedBeforeDeadline = "ELIGIBILITY_NOT_EVALUATED_BEFORE_DEADLINE";

    /// <summary>
    /// Q-22.2. Whether this store was given the calling hours and the call length, so that its
    /// missed-deadline sweep can tell a job that ran out of hours from one that ran out of channels.
    /// The scheduler's own store always is; a store built by hand without them is not.
    /// </summary>
    public bool ClassifiesByCallingHours => callingWindow is not null && schedulerOptions is not null;

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
            ExecutionModes.Mock,
            StringComparison.OrdinalIgnoreCase);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        await context.SimChannels
            .Where(channel => channel.Status == "QUARANTINED"
                && channel.QuarantineUntil <= now
                && channel.LeaseToken == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(channel => channel.Status, "IDLE")
                    .SetProperty(channel => channel.QuarantineUntil, (DateTimeOffset?)null)
                    .SetProperty(channel => channel.DisabledReason, (string?)null),
                cancellationToken);
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
              -- W-0249, fence 1 of 2. The task row is already joined for the window columns, so
              -- an order revoked before the claim costs one predicate and no extra read. This is
              -- where most revocations land: a confirmation window runs to minutes while
              -- claim-to-dial runs in seconds. The second fence is in
              -- PostgresTelephonyDispatchStore.LoadAsync, and neither closes the gap entirely --
              -- an outbound call is not a transaction.
              AND task.revoked_at IS NULL
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
            """).SingleOrDefaultAsync(cancellationToken);
        if (job is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()
            .SingleAsync(
                candidate => candidate.TaskId == job.TaskId,
                cancellationToken);

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
            """).SingleOrDefaultAsync(cancellationToken);
        if (channel is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        int completedCustomerAttempts = await context.CallAttempts
            .CountAsync(
                attempt => attempt.IvrCallJobId == job.IvrCallJobId
                    && attempt.IsCountedCustomerAttempt,
                cancellationToken);
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
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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
            channel.ProviderName)
        {
            TaskId = task.TaskId,
            CorrelationId = task.CorrelationId,
            TraceParent = task.TraceParent,
            TraceState = task.TraceState,
        };
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
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        List<SimChannelEntity> channels = await context.SimChannels.FromSqlInterpolated($$"""
            SELECT channel.*
            FROM ivr_sim_channels channel
            WHERE channel.lease_token IS NOT NULL
              AND channel.lease_expires_at <= {{detectedAt}}
              AND channel.status IN ('RESERVED', 'ACTIVE_CALL')
            ORDER BY channel.lease_expires_at, channel.sim_channel_id
            FOR UPDATE OF channel SKIP LOCKED
            LIMIT {{batchSize}}
            """).ToListAsync(cancellationToken);
        foreach (SimChannelEntity channel in channels)
        {
            string? activeJobId = channel.ActiveCallJobId;
            if (!string.IsNullOrWhiteSpace(activeJobId))
            {
                CallJobEntity? job = await context.CallJobs.SingleOrDefaultAsync(
                    candidate => candidate.IvrCallJobId == activeJobId,
                    cancellationToken);
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
                    .FirstOrDefaultAsync(cancellationToken);
                if (attempt is not null)
                {
                    attempt.Status = "RECOVERY_REQUIRED";
                    attempt.BlockedReason = "LEASE_EXPIRED_RECONCILIATION_REQUIRED";
                }
            }

            bool autoDisabled = SimChannelFailurePolicy.RecordFailure(channel, detectedAt);
            channel.Status = autoDisabled ? "HEALTH_FAILED" : "QUARANTINED";
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
                    ["fail_count"] = channel.FailCount,
                    ["failure_window_started_at"] = channel.FailureWindowStartedAt,
                    ["auto_disabled"] = autoDisabled,
                }));

            // W-0041 / P6-2, DT-04. Count every transition caused by an expired lease. The shared
            // failure policy owns the per-channel ten-minute threshold; this metric separately
            // gives operations a team-wide burst signal without reconstructing events from rows.
            Observability.IvrTelemetry.RecordChannelQuarantine(
                (Observability.TelemetryTags.ReasonCode, channel.DisabledReason));
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return channels.Count;
    }

    public async Task<int> CloseMissedDeadlinesAsync(
        DateTimeOffset detectedAt,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ValidateBatchSize(batchSize);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        List<CallJobEntity> jobs = await context.CallJobs.FromSqlInterpolated($$"""
            SELECT job.*
            FROM ivr_call_jobs job
            WHERE ((job.eligible IS TRUE
                    AND job.status IN (
                        'READY_FOR_SCHEDULER',
                        'DISPATCH_LEASED',
                        'DRY_RUN',
                        'HELD_ADMIN_REVIEW'))
                   -- W-0362 / K-52. A job the eligibility check held for capacity is not eligible
                   -- and never becomes so. The branch above never saw it and nothing else would
                   -- ever close it, so Module 3 waited on an order IVR had already given up on.
                   -- The fences below apply to every branch alike.
                   OR (job.eligible IS FALSE
                    AND job.status = 'CAPACITY_HELD')
                   -- W-0365 / K-54. The same gap, twice more. A job eligibility held for a human
                   -- is never released: resolving its review item leaves the job as it was. And a
                   -- job whose window passes before eligibility answers is never evaluated at all,
                   -- because the poller reads only open windows. Keyed on the decision, not on the
                   -- status alone: HELD_ADMIN_REVIEW on an eligible job is a technical or
                   -- lease-recovery hold and already the first branch's, and CREATED or DRY_RUN
                   -- under any other decision is not a job waiting on eligibility.
                   OR (job.eligible IS FALSE
                    AND job.eligibility_decision = 'TASK_HELD_ADMIN_REVIEW'
                    AND job.status = 'HELD_ADMIN_REVIEW')
                   OR (job.eligible IS FALSE
                    AND job.eligibility_decision = 'PENDING_ELIGIBILITY'
                    AND ((job.status = 'CREATED' AND job.queue_status = 'HELD_ELIGIBILITY')
                         OR (job.status = 'DRY_RUN' AND job.queue_status = 'HELD_MOCK'))))
              AND job.closed_at IS NULL
              AND job.expires_at <= {{detectedAt}}
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
            """).ToListAsync(cancellationToken);
        int activeChannels = await context.SimChannels.CountAsync(
            channel => channel.Enabled
                && channel.Status != "DISABLED"
                && channel.Status != "QUARANTINED"
                && channel.Status != "HEALTH_FAILED",
            cancellationToken);
        int pendingJobs = await context.CallJobs.CountAsync(
            candidate => candidate.Eligible && candidate.ClosedAt == null,
            cancellationToken);
        string[] jobIds = [.. jobs.Select(job => job.IvrCallJobId)];

        // Three numbers per job, not one. The attempt number alone cannot tell the two ways a job
        // reaches this sweep apart, and they call for opposite handling: a job that sat in the
        // queue and never dialled is a system failure the customer never heard about, while a job
        // that dialled and ran out of window gave the customer a real chance. Deciding both from
        // Max(attempt_number) would collapse them into whichever the last row happened to be.
        Dictionary<string, AttemptProgress> attemptProgress = await context.CallAttempts
            .AsNoTracking()
            .Where(attempt => jobIds.Contains(attempt.IvrCallJobId))
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new
            {
                JobId = group.Key,
                Progress = new AttemptProgress(
                    group.Max(attempt => attempt.AttemptNumber),
                    group.Count(),
                    group.Count(attempt => attempt.IsCountedCustomerAttempt)),
            })
            .ToDictionaryAsync(
                item => item.JobId,
                item => item.Progress,
                StringComparer.Ordinal,
                cancellationToken);
        var closed = new List<(string Program, string ResultStatus, string ReasonCode)>(jobs.Count);
        foreach (CallJobEntity job in jobs)
        {
            AttemptProgress progress = attemptProgress.TryGetValue(
                job.IvrCallJobId,
                out AttemptProgress recorded)
                ? recorded
                : AttemptProgress.None;

            // A capacity miss is a claim about why the deadline passed, and only two shapes support
            // it: the job was queued for dispatch and no channel ever came, or the eligibility
            // check held it because none would (W-0362 / K-52). A job held for review was kept
            // back on purpose, a dry run never wanted a channel, and a leased job already had one.
            // A job still waiting on eligibility never asked for one either: nobody had yet
            // decided it could be called (W-0365 / K-54). Reporting those as capacity shortage
            // inflates the very counter that sizes the SIM order (M8-OD-A), so they close as what
            // they are -- a confirmation window that ran out.
            bool capacityHeld = string.Equals(job.Status, "CAPACITY_HELD", StringComparison.Ordinal);
            bool capacityMiss = progress.TotalAttempts == 0
                && (capacityHeld
                    || string.Equals(job.Status, "READY_FOR_SCHEDULER", StringComparison.Ordinal));

            // Q-22.2 (2026-09-26). A queued job the calling hours left less than one expected
            // call's time to be dialled in - it arrived in the last seconds before 21:08, or after -
            // ran out of hours, not of channels. It closes exactly as before for Module 3, but opens
            // no capacity incident and is counted under its own reason, so the figure that sizes
            // the SIM order is not raised by it. A job eligibility held for capacity keeps its
            // incident: that shortage was measured when it was held.
            bool hoursRanOut = capacityMiss && !capacityHeld && CallingHoursRanOut(job);

            // W-0372 / K-64. A job still waiting on eligibility when its window closed was never
            // evaluated at all: the eligibility loop was off, failing, or never reached this order.
            // The decision is the whole test - eligibility replaces PENDING_ELIGIBILITY with
            // whatever it decides, and since K-60 a late evaluation refuses under the row lock, so
            // every such job ends here still pending. It closes exactly as K-54 made it close, and
            // is counted under its own reason: under the window reason it hid among customers
            // dialled and not reached, dry runs and orders held for review, and a loop that
            // answered some orders and not others raised nothing. Never a capacity miss either: its
            // status is CREATED or DRY_RUN, so no channel was ever asked for.
            bool eligibilityNeverAnswered = string.Equals(
                job.EligibilityDecision,
                EligibilityDecisions.Pending,
                StringComparison.Ordinal);

            // The customer side of the same question, and it is not the same question. Reaching
            // the customer at least once means the confirmation genuinely lapsed and Core can
            // expire it. Never reaching them means the order would die for a call that was never
            // placed, which is the outcome the technical-error boundary exists to prevent, so it
            // goes to a human instead.
            bool customerWasReached = progress.CountedAttempts > 0;

            // W-0362 / K-52. A held job already has its incident: eligibility opened one when it
            // decided no channel would come in time. Opening a second here would count a single
            // shortage twice in the table that sizes the SIM order, so the close points at that
            // one, and only a job that was queued and never served opens a new incident.
            string? heldIncidentId = capacityMiss && capacityHeld ? job.CapacityIncidentId : null;
            string incidentId = heldIncidentId ?? string.Concat("CAP-", Guid.NewGuid().ToString("N"));
            if (capacityMiss && heldIncidentId is null && !hoursRanOut)
            {
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
            }

            string resultId = string.Concat("RESULT-", Guid.NewGuid().ToString("N"));
            string evidenceRef = string.Concat(
                capacityMiss
                    ? "evidence://ivr/p2-3/capacity-miss/"
                    : "evidence://ivr/p2-3/window-expired/",
                job.IvrCallJobId);
            AuditLogEntity audit = CreateAudit(
                capacityMiss
                    ? "SCHEDULER_DEADLINE_MISSED"
                    : "SCHEDULER_WINDOW_EXPIRED",
                "call-job",
                job.IvrCallJobId,
                job.TaskId,
                detectedAt,
                new Dictionary<string, object?>
                {
                    ["capacity_incident_id"] = capacityMiss && !hoursRanOut ? incidentId : null,
                    ["calling_hours_ran_out"] = hoursRanOut,
                    ["result_id"] = resultId,
                    ["deadline"] = job.ExpiresAt,
                    ["is_counted_customer_attempt"] = false,
                    ["counted_attempts_before_deadline"] = progress.CountedAttempts,
                    ["customer_was_reached"] = customerWasReached,

                    // W-0365 / K-54. The result and its reason read the same whether eligibility
                    // cleared the job, held it for a human or never answered -- the contract with
                    // Module 3 has no code for the difference -- so the audit row names which.
                    ["eligibility_decision"] = job.EligibilityDecision,
                });
            string auditRef = string.Concat("audit://ivr/", audit.AuditId.ToString("D"));
            string reasonCode = capacityMiss
                ? "NO_DISPATCH_BEFORE_DEADLINE"
                : "WINDOW_EXPIRED_BEFORE_FINAL_RESULT";
            var normalized = new NormalizedResult(
                capacityMiss
                    ? IvrResultType.IvrCapacityException
                    : IvrResultType.IvrConfirmationWindowExpired,
                // Never counted, in either branch. No call was placed by this sweep, so no
                // customer attempt was consumed by it.
                false,
                true,
                reasonCode,
                null,
                capacityMiss ? "CAPACITY_UNAVAILABLE" : null,
                customerWasReached
                    ? CoreActionRecommendation.RevalidateAndExpireConfirmation
                    : CoreActionRecommendation.RevalidateAndHoldAdminReview,
                !customerWasReached,
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
                FinalResultStatus = normalized.ResultStatus,
                ResultType = normalized.ResultStatus,
                ResultReason = reasonCode,
                IsCountedCustomerAttempt = normalized.IsCounted,
                IsFinalForIvr = true,
                RecommendedCoreAction = ResultStorageVocabulary.ToCoreAction(
                    normalized.RecommendedCoreAction),
                CoreOrderHandoffRequired = true,
                HumanReviewRequired = normalized.HumanReviewRequired,
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
            int attemptNumber = progress.LastAttemptNumber > 0
                ? Math.Min(checked(progress.LastAttemptNumber + 1), job.MaxAttempts)
                : 1;
            context.ResultCallbacks.Add(CallbackOutboxSnapshotFactory.Create(
                resultId,
                job,
                attemptNumber,
                normalized,
                evidenceRef,
                auditRef,
                detectedAt));
            job.CapacityIncidentId = capacityMiss && !hoursRanOut ? incidentId : null;
            job.Status = capacityMiss ? "CAPACITY_MISSED" : "WINDOW_EXPIRED";
            job.QueueStatus = capacityMiss ? "CLOSED_CAPACITY" : "CLOSED_WINDOW_EXPIRED";
            job.ClosedAt = detectedAt;
            job.ClosedReason = normalized.ResultStatus;
            context.AuditLog.Add(audit);

            // The reason the counter carries, which is not always the result's: running out of
            // calling hours (Q-22.2) and never being evaluated (W-0372 / K-64) are told apart here
            // and nowhere in what Module 3 receives.
            closed.Add((
                job.ProgramType,
                normalized.ResultStatus,
                hoursRanOut
                    ? CallingHoursClosedBeforeDispatch
                    : eligibilityNeverAnswered ? EligibilityNotEvaluatedBeforeDeadline : reasonCode));
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Recorded after the commit and once per job, for the same reason as the dispatch counter:
        // a metric moved before the rows are durable runs ahead of the database on every rollback.
        //
        // Two instruments, because this method is the ONE path where a job reaches a final result
        // without passing through normalization -- the scheduler writes the IVR_CAPACITY_EXCEPTION
        // row itself. Recording only the deadline counter would leave every capacity miss out of
        // ivr_call_results_total, and a confirm_rate whose denominator omits the failures reads
        // higher than the truth. The gap is largest exactly when capacity is worst.
        //
        // The reason tag carries the branch. Every job here did miss its deadline, so the deadline
        // counter still moves for both -- but only one of them is evidence of a channel shortage.
        // Without the tag, procurement reads a single number that mixes real shortage with jobs
        // that were held on purpose, and buys SIMs against noise.
        foreach ((string program, string resultStatus, string reasonCode) in closed)
        {
            IvrTelemetry.RecordMissedDeadline(
                (TelemetryTags.Program, program),
                (TelemetryTags.ReasonCode, reasonCode));
            IvrTelemetry.RecordResult(
                (TelemetryTags.ResultType, resultStatus),
                (TelemetryTags.Counted, false));
        }

        return jobs.Count;
    }

    /// <summary>
    /// Q-22.2. True when the calling hours closed at some point between the job's arrival (or its
    /// T0, whichever is later) and its deadline, and what they left open in that span was less than
    /// one expected call. The first half keeps a job that simply arrived late in an open window a
    /// capacity miss: the hours did not run out on it, the window did.
    /// </summary>
    private bool CallingHoursRanOut(CallJobEntity job)
    {
        if (callingWindow is null || schedulerOptions is null)
        {
            return false;
        }

        DateTimeOffset from = job.CreatedAt > job.T0At ? job.CreatedAt : job.T0At;
        TimeSpan span = job.ExpiresAt - from;
        TimeSpan open = callingWindow.OpenTimeBetween(from, job.ExpiresAt);
        TimeSpan oneCall = TimeSpan.FromSeconds(schedulerOptions.Value.ExpectedCallDurationSeconds);
        return open < span && open < oneCall;
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

    /// <summary>
    /// What a job's attempt rows say about it at the moment its window closed.
    /// <para>
    /// <paramref name="TotalAttempts"/> answers "was this job ever dispatched", and
    /// <paramref name="CountedAttempts"/> answers "did the customer ever get a real call". They
    /// differ whenever an attempt failed technically, and the sweep needs both: the first decides
    /// whether a channel shortage is a supportable claim, the second decides whether Core may
    /// expire the order or must route it to a human.
    /// </para>
    /// </summary>
    private readonly record struct AttemptProgress(
        int LastAttemptNumber,
        int TotalAttempts,
        int CountedAttempts)
    {
        public static AttemptProgress None => new(0, 0, 0);
    }
}
