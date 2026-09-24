using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Scheduling;

/// <summary>
/// When the oldest call attempt that could be claimed right now fell due (<c>W-0041</c> / P6-2
/// section 11, the queue-backlog alert).
/// </summary>
public interface ISchedulerQueueBacklogReader
{
    /// <summary>
    /// The due time of the oldest attempt the dispatch claim would accept at
    /// <paramref name="now"/>, or null when nothing is waiting to be claimed.
    /// </summary>
    public Task<DateTimeOffset?> ReadOldestDueAtAsync(
        string executionMode,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads the backlog from the conditions the dispatch claim itself uses.
/// <para>
/// The WHERE clause is <c>PostgresSchedulerStore.TryClaimDueDispatchAsync</c>'s, condition for
/// condition, and it has to stay that way. A backlog measured over any other set either reports
/// work the dialler was never going to do -- a revoked order, a paused queue, an attempt already
/// ringing -- or misses work it is failing to do. It is a copy rather than a shared fragment
/// because the claim sits in the most depended-on class in the scheduler, which this change does
/// not touch; <c>IT-OBS-BACKLOG-15</c> holds the two together instead, by claiming what this
/// reports and checking that it then reports nothing.
/// </para>
/// <para>
/// Read-only and unlocked. The claim takes <c>FOR UPDATE SKIP LOCKED</c> because it is about to
/// write; a sample that took the same locks would compete with the dialler it is measuring.
/// </para>
/// </summary>
public sealed class PostgresSchedulerQueueBacklogReader(
    IDbContextFactory<IvrDbContext> dbContextFactory) : ISchedulerQueueBacklogReader
{
    public async Task<DateTimeOffset?> ReadOldestDueAtAsync(
        string executionMode,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executionMode);
        bool mockExecution = string.Equals(
            executionMode,
            ExecutionModes.Mock,
            StringComparison.OrdinalIgnoreCase);
        DateTimeOffset at = now.ToUniversalTime();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        return await context.Database.SqlQuery<DateTimeOffset?>($$"""
            SELECT MIN((job.attempt_schedule_json ->> progress.counted)::timestamptz) AS "Value"
            FROM ivr_call_jobs job
            JOIN ivr_confirmation_tasks task ON task.task_id = job.task_id
            CROSS JOIN LATERAL (
                SELECT COUNT(*)::integer AS counted
                FROM ivr_call_attempts attempt
                WHERE attempt.ivr_call_job_id = job.ivr_call_job_id
                  AND attempt.is_counted_customer_attempt IS TRUE
            ) progress
            WHERE job.eligible IS TRUE
              AND task.revoked_at IS NULL
              AND (({{mockExecution}} IS TRUE
                    AND job.status = 'DRY_RUN'
                    AND job.queue_status = 'HELD_MOCK')
                   OR ({{mockExecution}} IS FALSE
                    AND job.status = 'READY_FOR_SCHEDULER'
                    AND job.queue_status = 'QUEUED'))
              AND progress.counted < job.max_attempts
              AND ((job.attempt_schedule_json ->> progress.counted)::timestamptz) <= {{at}}
              AND ((job.attempt_schedule_json ->> progress.counted)::timestamptz) < job.expires_at
              AND job.expires_at > {{at}}
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
            """).SingleAsync(cancellationToken);
    }
}

/// <summary>
/// Samples <c>ivr_call_queue_oldest_due_age_seconds</c>: how long the oldest dialable attempt has
/// been waiting to be claimed (<c>W-0041</c>).
/// <para>
/// <b>Why the wait starts at the later of two times.</b> An attempt that fell due at 07:55 could
/// not be dialled before the calling window opened at 08:00 (<c>OD-V1-16</c>); the hour rule held
/// it, the dialler did not. Counting from 07:55 would put five minutes of designed waiting into
/// every morning's first sample, and an alert on that number would fire because the night
/// happened. So the wait runs from whichever came later, the due time or today's opening, and a
/// closed window reports zero -- nothing is being kept waiting by a dialler that is not allowed
/// to dial.
/// </para>
/// <para>
/// <b>Zero is recorded, not skipped.</b> The gauge reports the last value it was given, so a
/// sampler that stayed silent when the queue drained would go on reporting the backlog it last
/// saw.
/// </para>
/// </summary>
public sealed class SchedulerQueueBacklogSampler(
    ISchedulerQueueBacklogReader reader,
    CallingWindow callingWindow,
    SchedulerExecutionContext executionContext,
    TimeProvider timeProvider)
{
    /// <summary>
    /// How often the worker loop samples. The loop itself turns every poll interval -- 100ms under
    /// the LocalMockE2E profile -- and a query per turn would cost more than the dialling it
    /// measures. Fifteen seconds is well inside both the metric export interval and the alert's
    /// ten-minute hold, so a coarser sample loses nothing the rule can see.
    /// </summary>
    public static TimeSpan SampleInterval { get; } = TimeSpan.FromSeconds(15);

    // Written only by the scheduler loop, which calls SampleIfDueAsync one pass at a time.
    private DateTimeOffset nextSampleAt = DateTimeOffset.MinValue;

    /// <summary>
    /// Samples unless a sample was taken less than <see cref="SampleInterval"/> ago; returns
    /// null when it skipped. For the scheduler loop, which turns far more often than that.
    /// </summary>
    public async Task<TimeSpan?> SampleIfDueAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (now < nextSampleAt)
        {
            return null;
        }

        nextSampleAt = now + SampleInterval;
        return await SampleAtAsync(now, cancellationToken);
    }

    /// <summary>Measures the current wait, records it on the gauge, and returns it.</summary>
    public Task<TimeSpan> SampleAsync(CancellationToken cancellationToken = default) =>
        SampleAtAsync(timeProvider.GetUtcNow(), cancellationToken);

    private async Task<TimeSpan> SampleAtAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        TimeSpan waited = await MeasureAsync(now, cancellationToken);
        IvrTelemetry.RecordCallQueueOldestDueAge(waited.TotalSeconds);
        return waited;
    }

    private async Task<TimeSpan> MeasureAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        CallingWindowDecision window = callingWindow.Evaluate(now);
        if (!window.Open)
        {
            return TimeSpan.Zero;
        }

        DateTimeOffset? oldestDueAt = await reader.ReadOldestDueAtAsync(
            executionContext.ExecutionMode,
            now,
            cancellationToken);
        if (oldestDueAt is not { } waitingSince)
        {
            return TimeSpan.Zero;
        }

        if (callingWindow.Enabled)
        {
            // Today's opening, asked of the window rather than recomputed from its options, so
            // this cannot come to disagree with the gate the scheduler dials behind. Local
            // midnight is now minus the local time of day; from there the window names its next
            // opening, which is today's -- or midnight itself for a window that starts at 00:00.
            DateTimeOffset localMidnight = now - window.LocalTime.ToTimeSpan();
            DateTimeOffset openedAt = callingWindow.Evaluate(localMidnight).OpensAt;
            if (openedAt > waitingSince)
            {
                waitingSince = openedAt;
            }
        }

        TimeSpan waited = now - waitingSince;
        return waited > TimeSpan.Zero ? waited : TimeSpan.Zero;
    }
}
