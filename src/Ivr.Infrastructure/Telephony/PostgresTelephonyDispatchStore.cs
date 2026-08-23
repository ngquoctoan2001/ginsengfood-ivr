using System.Data;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Server = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.Infrastructure.Telephony;

public sealed record TelephonyDispatchContext(
    TaskId TaskId,
    DialTokenReference DialToken,
    PrivacySafeOrderSummary SpeechSummary,
    string ScriptTemplateId,
    string ScriptVersion);

/// <summary>
/// An operator's request to cut a call that is already in progress (W-0111).
/// </summary>
public sealed record CallTerminationRequest(
    string ActorId,
    string Reason,
    DateTimeOffset RequestedAt);

/// <summary>
/// Thrown inside the dispatch loop when an operator has asked for the call to be cut.
/// <para>
/// A distinct type rather than a flag so it lands in the loop's existing failure path, which
/// already releases the lease, returns the channel and records the attempt as a technical
/// exception. A cut is not a customer outcome — the customer never got to answer.
/// </para>
/// </summary>
public sealed class CallTerminatedException(CallTerminationRequest request)
    : Exception("The call was terminated by an operator.")
{
    /// <summary>Recorded as the attempt's technical exception type.</summary>
    public const string TechnicalCode = "CALL_TERMINATED_BY_OPERATOR";

    public CallTerminationRequest Request { get; } = request;
}

public interface ITelephonyDispatchStore
{
    /// <summary>
    /// Reads the termination request for the attempt this lease holds, or null.
    /// <para>
    /// A read rather than a signal because the request is written by <c>Ivr.Api</c> and acted on
    /// by the worker: two processes, so the database is the only thing both can see. The loop
    /// asks at its own checkpoints, which is also why a cut is reported to the operator as
    /// requested rather than done.
    /// </para>
    /// </summary>
    public Task<CallTerminationRequest?> ReadTerminationAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default);

    public Task<TelephonyDispatchContext> LoadAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default);

    public Task MarkActiveAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        CancellationToken cancellationToken = default);

    public Task CompleteAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        SimDtmfCapture dtmf,
        SimDispositionReport disposition,
        TimeSpan cooldown,
        CancellationToken cancellationToken = default);

    public Task FailAsync(
        SchedulerDispatchLease lease,
        SimCallSession? session,
        SimProviderDisposition disposition,
        string technicalErrorCode,
        bool channelHealthy,
        TimeSpan cooldown,
        CancellationToken cancellationToken = default);
}

public sealed class PostgresTelephonyDispatchStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    SpeechSummaryLimits speechLimits,
    TimeProvider timeProvider) : ITelephonyDispatchStore
{
    public async Task<CallTerminationRequest?> ReadTerminationAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        var row = await context.CallAttempts
            .AsNoTracking()
            .Where(attempt => attempt.IvrCallAttemptId == lease.AttemptId)
            .Select(attempt => new
            {
                attempt.TerminationRequestedAt,
                attempt.TerminationRequestedBy,
                attempt.TerminationReason,
            })
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return row?.TerminationRequestedAt is null
            ? null
            : new CallTerminationRequest(
                row.TerminationRequestedBy!,
                row.TerminationReason!,
                row.TerminationRequestedAt.Value);
    }

    public async Task<TelephonyDispatchContext> LoadAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        CallAttemptEntity attempt = await context.CallAttempts.AsNoTracking().SingleAsync(
            candidate => candidate.IvrCallAttemptId == lease.AttemptId,
            cancellationToken).ConfigureAwait(false);
        SimChannelEntity channel = await context.SimChannels.AsNoTracking().SingleAsync(
            candidate => candidate.SimChannelId == lease.SimChannelId,
            cancellationToken).ConfigureAwait(false);
        EnsureCurrentLease(lease, attempt, channel, timeProvider.GetUtcNow());
        ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking().SingleAsync(
            candidate => candidate.TaskId == attempt.TaskId,
            cancellationToken).ConfigureAwait(false);
        if (task.DialTokenExpiresAt > lease.Deadline)
        {
            throw new InvalidOperationException("Stored dial-token expiry exceeds the call deadline.");
        }

        Server.PrivacySafeOrderSummary wireSummary;
        try
        {
            wireSummary = JsonSerializer.Deserialize<Server.PrivacySafeOrderSummary>(
                task.PrivacySafeOrderSummaryJson)
                ?? throw new InvalidOperationException("Stored speech summary was empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Stored speech summary is unreadable.", exception);
        }

        return new TelephonyDispatchContext(
            TaskId.Create(task.TaskId),
            DialTokenReference.Create(task.DialTokenCiphertext, task.DialTokenExpiresAt),
            TargetV1TaskMapper.MapSpeechSummary(wireSummary, speechLimits),
            task.CallScriptTemplateId,
            task.CallScriptVersion);
    }

    public async Task MarkActiveAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(session);
        await MutateLeaseAsync(
            lease,
            async (context, attempt, channel, job, now) =>
            {
                if (session.AttemptId.Value != attempt.IvrCallAttemptId
                    || !string.Equals(session.SimChannelId, channel.SimChannelId, StringComparison.Ordinal)
                    || session.FencingGeneration != lease.FencingGeneration)
                {
                    throw new InvalidOperationException("Provider session does not match the scheduler lease.");
                }

                attempt.Status = "ACTIVE_CALL";
                attempt.StartedAt = session.StartedAt;
                attempt.ProviderCallId = session.ProviderCallReference;
                channel.Status = "ACTIVE_CALL";
                job.Status = "ACTIVE_CALL";
                context.AuditLog.Add(CreateAudit(
                    "SIM_CALL_STARTED",
                    attempt,
                    now,
                    new Dictionary<string, object?>
                    {
                        ["job_id"] = lease.JobId,
                        ["sim_channel_id"] = lease.SimChannelId,
                        ["fencing_generation"] = lease.FencingGeneration,
                        ["provider_call_ref"] = session.ProviderCallReference,
                        ["recording"] = "DISABLED",
                    }));
                await Task.CompletedTask;
            },
            cancellationToken).ConfigureAwait(false);
    }

    public Task CompleteAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        SimDtmfCapture dtmf,
        SimDispositionReport disposition,
        TimeSpan cooldown,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(dtmf);
        ArgumentNullException.ThrowIfNull(disposition);
        return FinalizeAsync(
            lease,
            session,
            disposition.Disposition,
            SanitizeDtmf(dtmf.Key),
            disposition.TechnicalErrorCode,
            disposition.ChannelHealthy,
            disposition.StartedAt,
            disposition.EndedAt,
            cooldown,
            cancellationToken);
    }

    public Task FailAsync(
        SchedulerDispatchLease lease,
        SimCallSession? session,
        SimProviderDisposition disposition,
        string technicalErrorCode,
        bool channelHealthy,
        TimeSpan cooldown,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(technicalErrorCode);
        return FinalizeAsync(
            lease,
            session,
            disposition,
            null,
            SafeTechnicalCode(technicalErrorCode),
            channelHealthy,
            session?.StartedAt,
            timeProvider.GetUtcNow(),
            cooldown,
            cancellationToken);
    }

    private Task FinalizeAsync(
        SchedulerDispatchLease lease,
        SimCallSession? session,
        SimProviderDisposition disposition,
        string? dtmf,
        string? technicalErrorCode,
        bool channelHealthy,
        DateTimeOffset? startedAt,
        DateTimeOffset endedAt,
        TimeSpan cooldown,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentOutOfRangeException.ThrowIfLessThan(cooldown, TimeSpan.Zero);
        return MutateLeaseAsync(
            lease,
            async (context, attempt, channel, job, now) =>
            {
                if (session is not null
                    && (session.AttemptId.Value != attempt.IvrCallAttemptId
                        || !string.Equals(
                            session.SimChannelId,
                            channel.SimChannelId,
                            StringComparison.Ordinal)
                        || session.FencingGeneration != lease.FencingGeneration))
                {
                    throw new InvalidOperationException("Provider session does not match the scheduler lease.");
                }

                string rawEventId = string.Concat("RAW-", Guid.NewGuid().ToString("N"));
                string rawStatus = disposition.ToString().ToUpperInvariant();
                context.RawCallEvents.Add(new RawCallEventEntity
                {
                    RawEventId = rawEventId,
                    IvrCallAttemptId = attempt.IvrCallAttemptId,
                    IvrCallJobId = attempt.IvrCallJobId,
                    ProviderInternalPayloadRef = string.Concat(
                        "provider://event/",
                        lease.ProviderName.ToLowerInvariant(),
                        "/",
                        attempt.IvrCallAttemptId),
                    RawCallStatus = rawStatus,
                    RawDtmf = dtmf,
                    AudioStatus = disposition == SimProviderDisposition.AudioError
                        ? "ERROR"
                        : session is null ? "NOT_STARTED" : "PLAYED",
                    TechnicalErrorCode = technicalErrorCode,
                    RecordingRef = null,
                    ReceivedAt = endedAt,
                });
                attempt.Status = "PROVIDER_EVENT_PENDING_NORMALIZATION";
                attempt.StartedAt ??= startedAt;
                attempt.EndedAt = endedAt;
                attempt.DtmfKey = dtmf;
                attempt.Disposition = rawStatus;
                attempt.TechnicalExceptionType = technicalErrorCode;
                attempt.ProviderCallId = session?.ProviderCallReference;
                attempt.RawCallEventId = rawEventId;
                attempt.IsCountedCustomerAttempt = false;
                job.Status = "DISPOSITION_PENDING_NORMALIZATION";
                job.QueueStatus = "HELD_NORMALIZATION";

                channel.FailCount = channelHealthy ? 0 : channel.FailCount + 1;
                channel.LastHealthCheckAt = endedAt;
                channel.CooldownUntil = endedAt.Add(cooldown);
                channel.Status = channelHealthy
                    ? "IDLE"
                    : channel.FailCount >= 3 ? "HEALTH_FAILED" : "QUARANTINED";
                channel.QuarantineUntil = channelHealthy
                    ? null
                    : endedAt.Add(cooldown);
                channel.DisabledReason = channelHealthy
                    ? null
                    : technicalErrorCode ?? "CHANNEL_UNHEALTHY";
                ReleaseLease(channel);

                // W-0042 / P6-3, DT-04. The second place a channel is taken out of service, and
                // the one DT-04 actually names: fail_count crossing its threshold. W-0041 counted
                // only the lease-expiry path, so the alert that claimed to cover DT-04 was reading
                // a metric the DT-04 transition never touched -- an alert wired to the wrong
                // event, which is worse than none because it looks covered.
                if (!channelHealthy)
                {
                    Observability.IvrTelemetry.RecordChannelQuarantine(
                        (Observability.TelemetryTags.ReasonCode,
                            channel.DisabledReason ?? "CHANNEL_UNHEALTHY"));
                }

                context.AuditLog.Add(CreateAudit(
                    "SIM_PROVIDER_EVENT_CAPTURED",
                    attempt,
                    now,
                    new Dictionary<string, object?>
                    {
                        ["raw_event_id"] = rawEventId,
                        ["raw_call_status"] = rawStatus,
                        ["technical_error_code"] = technicalErrorCode,
                        ["channel_healthy"] = channelHealthy,
                        ["recording"] = "DISABLED",
                        ["is_counted_customer_attempt"] = false,
                    }));
                await Task.CompletedTask;
            },
            cancellationToken);
    }

    private async Task MutateLeaseAsync(
        SchedulerDispatchLease lease,
        Func<
            IvrDbContext,
            CallAttemptEntity,
            SimChannelEntity,
            CallJobEntity,
            DateTimeOffset,
            Task> mutation,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken).ConfigureAwait(false);
        CallAttemptEntity attempt = await context.CallAttempts.FromSqlInterpolated($$"""
            SELECT attempt.* FROM ivr_call_attempts attempt
            WHERE attempt.ivr_call_attempt_id = {{lease.AttemptId}}
            FOR UPDATE OF attempt
            """).SingleAsync(cancellationToken).ConfigureAwait(false);
        SimChannelEntity channel = await context.SimChannels.FromSqlInterpolated($$"""
            SELECT channel.* FROM ivr_sim_channels channel
            WHERE channel.sim_channel_id = {{lease.SimChannelId}}
            FOR UPDATE OF channel
            """).SingleAsync(cancellationToken).ConfigureAwait(false);
        CallJobEntity job = await context.CallJobs.FromSqlInterpolated($$"""
            SELECT job.* FROM ivr_call_jobs job
            WHERE job.ivr_call_job_id = {{lease.JobId}}
            FOR UPDATE OF job
            """).SingleAsync(cancellationToken).ConfigureAwait(false);
        DateTimeOffset now = timeProvider.GetUtcNow();
        EnsureCurrentLease(lease, attempt, channel, now);
        await mutation(context, attempt, channel, job, now).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureCurrentLease(
        SchedulerDispatchLease lease,
        CallAttemptEntity attempt,
        SimChannelEntity channel,
        DateTimeOffset now)
    {
        if (!string.Equals(attempt.IvrCallJobId, lease.JobId, StringComparison.Ordinal)
            || !string.Equals(attempt.SimChannelId, lease.SimChannelId, StringComparison.Ordinal)
            || attempt.Status is not ("LEASED_PENDING_DISPATCH" or "ACTIVE_CALL")
            || !string.Equals(channel.ActiveCallJobId, lease.JobId, StringComparison.Ordinal)
            || channel.LeaseToken != lease.LeaseToken
            || channel.LeaseFencingGeneration != lease.FencingGeneration
            || channel.LeaseExpiresAt is null
            || channel.LeaseExpiresAt <= now
            || channel.Status is not ("RESERVED" or "ACTIVE_CALL"))
        {
            throw new InvalidOperationException("Scheduler dispatch lease is stale or no longer active.");
        }
    }

    private static void ReleaseLease(SimChannelEntity channel)
    {
        channel.ActiveCallJobId = null;
        channel.LeaseToken = null;
        channel.LeasedByWorkerId = null;
        channel.LeaseAcquiredAt = null;
        channel.LeaseExpiresAt = null;
        channel.LeaseFencingGeneration++;
    }

    private static AuditLogEntity CreateAudit(
        string action,
        CallAttemptEntity attempt,
        DateTimeOffset occurredAt,
        IReadOnlyDictionary<string, object?> data) => new()
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-telephony",
            ActorType = "service",
            Action = action,
            TargetType = "call-attempt",
            TargetId = attempt.IvrCallAttemptId,
            Reason = action,
            CorrelationId = attempt.TaskId,
            DataJson = JsonSerializer.Serialize(data),
            CreatedAt = occurredAt,
        };

    private static string? SanitizeDtmf(string? key) => key switch
    {
        null or "" => null,
        "0" => "0",
        "1" => "1",
        _ => "INVALID",
    };

    private static string SafeTechnicalCode(string value)
    {
        string normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 120
            || normalized.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '_' or '-')))
        {
            throw new ArgumentException("Technical error code is invalid.", nameof(value));
        }

        return normalized;
    }
}
