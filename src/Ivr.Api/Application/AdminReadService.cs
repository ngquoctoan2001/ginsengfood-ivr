using System.Text.Json;
using Ivr.Api.Admin;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IAdminReadService
{
    public Task<DashboardApiResult> GetDashboardAsync(
        string? program,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        CancellationToken cancellationToken);

    public Task<CallJobPageApiResult> ListCallJobsAsync(
        CallJobFilter filter,
        CancellationToken cancellationToken);

    public Task<CallJobDetailApiResult> GetCallJobDetailAsync(
        string ivrCallJobId,
        CancellationToken cancellationToken);

    public Task<SimChannelListApiResult> ListSimChannelsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Read-only admin projections (W-0095).
///
/// This service never writes. It exists so the console can show operational
/// state without the browser reaching the service-only lifecycle endpoints
/// (specs/ui/08 §4), and so every number on the dashboard is computed here
/// rather than in the client (P3-2 §9).
/// </summary>
public sealed class AdminReadService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IOptions<IvrOptions> ivrOptions,
    TimeProvider timeProvider) : IAdminReadService
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 25;

    /// <summary>
    /// `specs/ui/02` prioritises a five-minute Golden Hour deadline warning. The
    /// same window is applied to both programs until Product signs off on a
    /// per-program value.
    /// </summary>
    public static readonly TimeSpan NearExpiryWindow = TimeSpan.FromMinutes(5);

    private const string AdminPauseScope = "ADMIN_QUEUE_PAUSE";

    private static readonly string[] OpenQueueStatuses =
        ["QUEUED", "HELD_MOCK", "HELD_ADMIN_REVIEW", "DISPATCHING"];

    private static readonly string[] ActiveAttemptStatuses =
        ["LEASED_PENDING_DISPATCH", "DIALING", "ACTIVE_CALL"];

    private static readonly string[] NoAnswerResultTypes =
        ["IVR_NO_ANSWER_ATTEMPT", "IVR_NO_ANSWER_FINAL"];

    /// <summary>
    /// Results that prove the call itself worked: the customer answered and gave
    /// an input. Everything else — no answer, invalid phone, technical, capacity,
    /// policy or operational block — is a call that did not reach anyone.
    /// </summary>
    private static readonly string[] ReachedCustomerResultTypes =
        ["IVR_CONFIRMED", "IVR_CUSTOMER_CANCELLED", "IVR_WRONG_INPUT"];

    public async Task<DashboardApiResult> GetDashboardAsync(
        string? program,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        CancellationToken cancellationToken)
    {
        string? normalizedProgram = NormalizeProgram(program);
        RequireOrderedRange(createdFrom, createdTo);

        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);
        DateTimeOffset now = timeProvider.GetUtcNow();

        IQueryable<CallJobEntity> jobs = context.CallJobs.AsNoTracking();
        if (normalizedProgram is not null)
        {
            jobs = jobs.Where(job => job.ProgramType == normalizedProgram);
        }

        if (createdFrom is not null)
        {
            jobs = jobs.Where(job => job.CreatedAt >= createdFrom);
        }

        if (createdTo is not null)
        {
            jobs = jobs.Where(job => job.CreatedAt <= createdTo);
        }

        List<QueueStatusCount> queueCounts = await jobs
            .GroupBy(job => new { job.QueueStatus, Closed = job.ClosedAt != null })
            .Select(group => new QueueStatusCount(
                group.Key.QueueStatus,
                group.Key.Closed,
                group.Count()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTimeOffset nearExpiryCutoff = now + NearExpiryWindow;
        int nearExpiry = await jobs.CountAsync(
            job => job.ClosedAt == null
                && job.ExpiresAt > now
                && job.ExpiresAt <= nearExpiryCutoff,
            cancellationToken).ConfigureAwait(false);

        bool paused = await context.CapacityIncidents.AsNoTracking().AnyAsync(
            incident => incident.Status == "OPEN"
                && incident.HoldNewCalls
                && incident.Scope == AdminPauseScope,
            cancellationToken).ConfigureAwait(false);

        IQueryable<string> jobIds = jobs.Select(job => job.IvrCallJobId);

        // `specs/ui/01` asks for a blocked tile and an attempt-2 tile. Blocked is
        // the eligibility refusal; attempt-2 is an open job that has spent one
        // counted customer attempt and still has one left.
        int blocked = await jobs
            .CountAsync(job => job.ClosedAt == null && !job.Eligible, cancellationToken)
            .ConfigureAwait(false);

        IQueryable<string> openJobIds = jobs
            .Where(job => job.ClosedAt == null && job.MaxAttempts >= 2)
            .Select(job => job.IvrCallJobId);
        int attemptTwoPending = await context.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IsCountedCustomerAttempt
                && openJobIds.Contains(attempt.IvrCallJobId))
            .GroupBy(attempt => attempt.IvrCallJobId)
            .CountAsync(group => group.Count() == 1, cancellationToken)
            .ConfigureAwait(false);

        List<ResultTypeCount> resultCounts = await context.CallResults.AsNoTracking()
            .Where(result => jobIds.Contains(result.IvrCallJobId))
            .GroupBy(result => result.ResultType)
            .Select(group => new ResultTypeCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<CallAttemptEntity> attempts = context.CallAttempts.AsNoTracking()
            .Where(attempt => jobIds.Contains(attempt.IvrCallJobId));
        int attemptTotal = await attempts.CountAsync(cancellationToken).ConfigureAwait(false);
        int countedAttempts = await attempts.CountAsync(
            attempt => attempt.IsCountedCustomerAttempt,
            cancellationToken).ConfigureAwait(false);
        int technicalRetries = await attempts.SumAsync(
            attempt => attempt.TechnicalRetryCount,
            cancellationToken).ConfigureAwait(false);
        int activeAttempts = await attempts.CountAsync(
            attempt => ActiveAttemptStatuses.Contains(attempt.Status),
            cancellationToken).ConfigureAwait(false);

        // The SIM pool and open incidents are pool-wide state, not per-program,
        // so the program/time filter deliberately does not apply to them.
        List<SimChannelEntity> channels = await context.SimChannels.AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CapacityIncidentEntity> incidents = await context.CapacityIncidents.AsNoTracking()
            .Where(incident => incident.Status == "OPEN")
            .OrderByDescending(incident => incident.OpenedAt)
            .Take(20)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new DashboardApiResult(
            now,
            ivrOptions.Value.ExecutionMode,
            ivrOptions.Value.SimProvider,
            ivrOptions.Value.RealCustomerCallAllowed,
            normalizedProgram,
            createdFrom,
            createdTo,
            BuildQueuePanel(queueCounts, paused, nearExpiry, attemptTwoPending, blocked),
            BuildResultPanel(resultCounts),
            new DashboardAttemptPanel(
                attemptTotal,
                countedAttempts,
                technicalRetries,
                activeAttempts),
            BuildSimPanel(channels, now, ivrOptions.Value.ExecutionMode),
            incidents.Select(incident => new CapacityIncidentSummary(
                incident.CapacityIncidentId,
                incident.Scope,
                incident.Status,
                incident.HoldNewCalls,
                incident.ShortageReason,
                incident.MissedDeadlineCount,
                incident.OpenedAt)).ToArray(),
            incidents.Sum(incident => incident.MissedDeadlineCount));
    }

    public async Task<CallJobPageApiResult> ListCallJobsAsync(
        CallJobFilter filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        string? normalizedProgram = NormalizeProgram(filter.Program);
        RequireOrderedRange(filter.From, filter.To);
        RequireSafeFilter(filter.Status, nameof(filter.Status));
        RequireSafeFilter(filter.QueueStatus, nameof(filter.QueueStatus));
        RequireSafeFilter(filter.ResultType, nameof(filter.ResultType));
        RequireSafeFilter(filter.OrderCode, nameof(filter.OrderCode));
        RequireSafeFilter(filter.CorrelationId, nameof(filter.CorrelationId));

        int page = filter.Page < 1 ? 1 : filter.Page;
        int pageSize = filter.PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => filter.PageSize,
        };

        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset nearExpiryCutoff = now + NearExpiryWindow;

        IQueryable<CallJobEntity> query = context.CallJobs.AsNoTracking();
        if (normalizedProgram is not null)
        {
            query = query.Where(job => job.ProgramType == normalizedProgram);
        }

        if (filter.Status is not null)
        {
            query = query.Where(job => job.Status == filter.Status);
        }

        if (filter.QueueStatus is not null)
        {
            query = query.Where(job => job.QueueStatus == filter.QueueStatus);
        }

        if (filter.From is not null)
        {
            query = query.Where(job => job.CreatedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(job => job.CreatedAt <= filter.To);
        }

        if (filter.NearExpiryOnly)
        {
            query = query.Where(job => job.ClosedAt == null
                && job.ExpiresAt > now
                && job.ExpiresAt <= nearExpiryCutoff);
        }

        if (filter.ResultType is not null)
        {
            IQueryable<string> matching = context.CallResults.AsNoTracking()
                .Where(result => result.ResultType == filter.ResultType)
                .Select(result => result.IvrCallJobId);
            query = query.Where(job => matching.Contains(job.IvrCallJobId));
        }

        // The order code is a filter input only. It is never echoed back: the
        // console renders `order_code_short` (specs/ui/02).
        if (filter.OrderCode is not null || filter.CorrelationId is not null)
        {
            IQueryable<ConfirmationTaskEntity> tasks = context.ConfirmationTasks.AsNoTracking();
            if (filter.OrderCode is not null)
            {
                tasks = tasks.Where(task => task.OrderCode == filter.OrderCode);
            }

            if (filter.CorrelationId is not null)
            {
                tasks = tasks.Where(task => task.CorrelationId == filter.CorrelationId);
            }

            IQueryable<string> taskIds = tasks.Select(task => task.TaskId);
            query = query.Where(job => taskIds.Contains(job.TaskId));
        }

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        List<CallJobEntity> pageJobs = await query
            .OrderByDescending(job => job.CreatedAt)
            .ThenBy(job => job.IvrCallJobId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (pageJobs.Count == 0)
        {
            return new CallJobPageApiResult(page, pageSize, totalCount, []);
        }

        string[] pageJobIds = pageJobs.Select(job => job.IvrCallJobId).ToArray();
        string[] pageTaskIds = pageJobs.Select(job => job.TaskId).Distinct().ToArray();

        Dictionary<string, string> summaries = await context.ConfirmationTasks.AsNoTracking()
            .Where(task => pageTaskIds.Contains(task.TaskId))
            .Select(task => new { task.TaskId, task.PrivacySafeOrderSummaryJson })
            .ToDictionaryAsync(
                item => item.TaskId,
                item => item.PrivacySafeOrderSummaryJson,
                cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, string> maskedPhones = await context.ConfirmationTasks.AsNoTracking()
            .Where(task => pageTaskIds.Contains(task.TaskId))
            .Select(task => new { task.TaskId, task.PhoneMasked })
            .ToDictionaryAsync(item => item.TaskId, item => item.PhoneMasked, cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, int> attemptCounts = await context.CallAttempts.AsNoTracking()
            .Where(attempt => pageJobIds.Contains(attempt.IvrCallJobId))
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new { JobId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.JobId, item => item.Count, cancellationToken)
            .ConfigureAwait(false);

        List<CallResultEntity> results = await context.CallResults.AsNoTracking()
            .Where(result => pageJobIds.Contains(result.IvrCallJobId))
            .OrderBy(result => result.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        Dictionary<string, string> latestResultTypes = results
            .GroupBy(result => result.IvrCallJobId)
            .ToDictionary(group => group.Key, group => group.Last().ResultType);

        CallJobListItem[] items = pageJobs.Select(job => new CallJobListItem(
            job.IvrCallJobId,
            job.TaskId,
            ReadOrderCodeShort(summaries.GetValueOrDefault(job.TaskId)),
            maskedPhones.GetValueOrDefault(job.TaskId, string.Empty),
            job.ProgramType,
            job.Status,
            job.QueueStatus,
            attemptCounts.GetValueOrDefault(job.IvrCallJobId),
            job.MaxAttempts,
            latestResultTypes.GetValueOrDefault(job.IvrCallJobId),
            job.ExpiresAt,
            job.CreatedAt,
            job.ClosedAt,
            job.ClosedAt is null && job.ExpiresAt > now && job.ExpiresAt <= nearExpiryCutoff))
            .ToArray();

        return new CallJobPageApiResult(page, pageSize, totalCount, items);
    }

    public async Task<CallJobDetailApiResult> GetCallJobDetailAsync(
        string ivrCallJobId,
        CancellationToken cancellationToken)
    {
        RequireSafeFilter(ivrCallJobId, nameof(ivrCallJobId));
        if (string.IsNullOrWhiteSpace(ivrCallJobId))
        {
            throw IvrErrors.MalformedRequest("ivrCallJobId is required.");
        }

        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);

        CallJobEntity job = await context.CallJobs.AsNoTracking().SingleOrDefaultAsync(
            entity => entity.IvrCallJobId == ivrCallJobId,
            cancellationToken).ConfigureAwait(false)
            ?? throw IvrErrors.NotFound("The call job was not found.");

        ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.TaskId == job.TaskId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw IvrErrors.NotFound("The confirmation task was not found.");

        List<CallAttemptEntity> attempts = await context.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IvrCallJobId == job.IvrCallJobId)
            .OrderBy(attempt => attempt.AttemptNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<CallResultEntity> results = await context.CallResults.AsNoTracking()
            .Where(result => result.IvrCallJobId == job.IvrCallJobId)
            .OrderBy(result => result.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] resultIds = results.Select(result => result.IvrCallResultId).ToArray();
        List<ResultCallbackEntity> callbacks = await context.ResultCallbacks.AsNoTracking()
            .Where(callback => resultIds.Contains(callback.IvrCallResultId))
            .OrderBy(callback => callback.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] attemptIds = attempts.Select(attempt => attempt.IvrCallAttemptId).ToArray();
        List<TechnicalExceptionEntity> technicalExceptions = await context.TechnicalExceptions
            .AsNoTracking()
            .Where(exception => attemptIds.Contains(exception.IvrCallAttemptId))
            .OrderBy(exception => exception.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Review items point at whatever produced them, so a job's review queue is
        // the union of its task, its results and its callbacks.
        string[] reviewSourceIds =
        [
            task.TaskId,
            .. resultIds,
            .. callbacks.Select(callback => callback.CallbackId),
        ];
        List<ReviewItemEntity> reviewItems = await context.ReviewItems.AsNoTracking()
            .Where(item => reviewSourceIds.Contains(item.SourceId))
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // W-0113. The recorded voice wins over the derived one, and the LAST attempt that
        // recorded a voice wins over earlier ones: configuration can change between two attempts
        // of the same job, so "the voice this job used" is only well defined per attempt. The
        // per-attempt values are on the rows below; this is the summary a screen leads with.
        RecordedVoiceRegion recordedVoice = ReadRecordedVoiceRegion(attempts);

        return new CallJobDetailApiResult(
            job.IvrCallJobId,
            job.TaskId,
            ReadOrderCodeShort(task.PrivacySafeOrderSummaryJson),
            task.PhoneMasked,
            job.ProgramType,
            task.OrderState,
            job.OrderVersionSnapshot,
            job.Status,
            job.QueueStatus,
            job.Eligible,
            job.EligibilityDecision,
            ReadStringArray(task.BlockedReasonsJson),
            task.CallRestriction,
            recordedVoice.Region ?? ReadVoiceRegion(task.PrivacySafeOrderSummaryJson),
            recordedVoice.Source ?? (
                ReadVoiceRegion(task.PrivacySafeOrderSummaryJson) is null ? null : DerivedVoiceRegion),
            job.MaxAttempts,
            job.AttemptPolicyCode,
            job.ScriptVersion,
            job.PrivacyPolicyVersion,
            job.T0At,
            job.ExpiresAt,
            job.CreatedAt,
            job.ClosedAt,
            job.ClosedReason,
            attempts.Select(attempt => new CallAttemptDetail(
                attempt.IvrCallAttemptId,
                attempt.AttemptNumber,
                attempt.ScheduledAt,
                attempt.StartedAt,
                attempt.EndedAt,
                attempt.Status,
                attempt.ResultStatus,
                attempt.Disposition,
                attempt.DtmfKey,
                attempt.IsCountedCustomerAttempt,
                attempt.TechnicalRetryCount,
                attempt.TechnicalExceptionType,
                attempt.SimChannelId,
                attempt.BlockedReason,
                attempt.PolicyVersion,
                attempt.ScriptVersion,
                attempt.VoiceId,
                attempt.VoiceRegion,
                attempt.VoiceRegionResolved)).ToArray(),
            results.Select(result => new CallResultDetail(
                result.IvrCallResultId,
                result.ResultType,
                result.ResultReason,
                result.DtmfKey,
                result.IsCountedCustomerAttempt,
                result.IsFinalForIvr,
                result.RecommendedCoreAction,
                result.HumanReviewRequired,
                result.CreatedAt)).ToArray(),
            callbacks.Select(callback => new ResultCallbackDetail(
                callback.CallbackId,
                callback.IvrCallResultId,
                callback.ResultState,
                callback.DeliveryStatus,
                callback.CoreHttpStatus,
                callback.CoreResponseCode,
                callback.RetryCount,
                callback.RequiresCoreRevalidation,
                callback.CreatedAt,
                callback.SentAt,
                callback.AcknowledgedAt)).ToArray(),
            technicalExceptions.Select(exception => new TechnicalExceptionDetail(
                exception.TechnicalExceptionId,
                exception.IvrCallAttemptId,
                exception.ExceptionType,
                exception.CustomerAttemptCounted,
                exception.TechnicalRetryAllowed,
                exception.TechnicalRetryCount,
                exception.CreatedAt)).ToArray(),
            reviewItems.Select(item => new ReviewItemDetail(
                item.ReviewItemId,
                item.SourceType,
                item.SourceId,
                item.Reason,
                item.Status,
                item.Resolution,
                item.CreatedAt,
                item.ResolvedAt)).ToArray(),
            CollectReferences(task.EvidenceRefsJson, job.EvidenceRefsJson, attempts, results),
            CollectReferences(task.AuditRefsJson, job.AuditRefsJson, attempts, results, audit: true),
            task.CorrelationId,
            job.InputSignalOnly,
            job.NoDirectOrderUpdate);
    }

    /// <summary>
    /// The channel roster behind the dashboard's SIM panel (W-0099).
    ///
    /// The panel has always shown counts; without the per-channel list the
    /// `IVR_SIM_ENABLE` / `IVR_SIM_DISABLE` operations from P2-8 had no console
    /// surface at all, even though `specs/ui/08` §3 lists both as console
    /// actions.
    /// </summary>
    public async Task<SimChannelListApiResult> ListSimChannelsAsync(
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);
        DateTimeOffset now = timeProvider.GetUtcNow();

        List<SimChannelEntity> channels = await context.SimChannels.AsNoTracking()
            .OrderBy(channel => channel.SimChannelId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SimChannelListApiResult(
            now,
            ivrOptions.Value.ExecutionMode,
            ivrOptions.Value.RealCustomerCallAllowed,
            channels.Select(channel => new SimChannelView(
                channel.SimChannelId,
                channel.Enabled,
                channel.Status,
                channel.AdapterMode,
                channel.ProviderName,
                channel.ActiveCallJobId is not null,
                channel.ActiveCallJobId,
                channel.FailCount,
                channel.QuarantineUntil is not null && channel.QuarantineUntil > now,
                channel.QuarantineUntil,
                channel.CooldownUntil,
                channel.LastHealthCheckAt,
                channel.DisabledReason)).ToArray());
    }

    private static DashboardQueuePanel BuildQueuePanel(
        IReadOnlyList<QueueStatusCount> counts,
        bool paused,
        int nearExpiry,
        int attemptTwoPending,
        int blocked)
    {
        int Open(string queueStatus) => counts
            .Where(count => !count.Closed
                && string.Equals(count.QueueStatus, queueStatus, StringComparison.Ordinal))
            .Sum(count => count.Count);

        return new DashboardQueuePanel(
            paused,
            Open("QUEUED"),
            Open("HELD_MOCK"),
            Open("HELD_ADMIN_REVIEW"),
            Open("DISPATCHING"),
            counts.Where(count => !count.Closed).Sum(count => count.Count),
            counts.Where(count => count.Closed).Sum(count => count.Count),
            nearExpiry,
            attemptTwoPending,
            blocked);
    }

    private static DashboardResultPanel BuildResultPanel(IReadOnlyList<ResultTypeCount> counts)
    {
        int total = counts.Sum(count => count.Count);
        int Rate(Func<ResultTypeCount, bool> predicate) => counts.Where(predicate).Sum(c => c.Count);

        double Share(int value) => total == 0 ? 0d : Math.Round((double)value / total, 4);

        return new DashboardResultPanel(
            total,
            counts.ToDictionary(count => count.ResultType, count => count.Count, StringComparer.Ordinal),
            Share(Rate(count => count.ResultType == "IVR_CONFIRMED")),
            Share(Rate(count => count.ResultType == "IVR_CUSTOMER_CANCELLED")),
            Share(Rate(count => NoAnswerResultTypes.Contains(count.ResultType))),
            Share(Rate(count => count.ResultType == "IVR_TECHNICAL_EXCEPTION")),
            // Reached-the-customer share: the call worked and an input came
            // back. A cancel counts — the call succeeded, the answer was no.
            Share(Rate(count => ReachedCustomerResultTypes.Contains(count.ResultType))));
    }

    private static DashboardSimPanel BuildSimPanel(
        List<SimChannelEntity> channels,
        DateTimeOffset now,
        string executionMode)
    {
        int Count(Func<SimChannelEntity, bool> predicate) => channels.Count(predicate);

        return new DashboardSimPanel(
            channels.Count,
            Count(channel => channel.Enabled),
            Count(channel => channel.Enabled && channel.Status == "IDLE"),
            Count(channel => channel.ActiveCallJobId != null),
            Count(channel => !channel.Enabled),
            Count(channel => channel.Status == "HEALTH_FAILED"),
            Count(channel => channel.QuarantineUntil != null && channel.QuarantineUntil > now),
            Ratio(Count(channel => channel.Status == "HEALTH_FAILED"), channels.Count),
            channels.Count > 0 ? channels[0].AdapterMode : executionMode);
    }

    /// <summary>
    /// Reads `order_code_short` out of the persisted privacy-safe summary. The
    /// full order code stays in the database: only the short form is approved
    /// for display (specs/ui/02, specs/api/04).
    /// </summary>
    /// <summary>
    /// Derives the regional voice from the stored delivery area (W-0106). Reuses
    /// <see cref="DeliveryRegionResolver"/> rather than reimplementing the province table here:
    /// two copies of a 63-name mapping would drift, and the copy that drifted would be the one
    /// the console shows while the one the customer hears stays right — the worst way round.
    /// <para>
    /// A malformed or absent summary yields null rather than failing the whole detail screen.
    /// </para>
    /// </summary>
    /// <summary>Where a rendered <c>voice_region</c> came from.</summary>
    private readonly record struct RecordedVoiceRegion(string? Region, string? Source);

    private const string RecordedVoiceRegionSource = "RECORDED";
    private const string DerivedVoiceRegion = "DERIVED";

    /// <summary>
    /// The region recorded by the most recent attempt that recorded one (W-0113).
    /// <para>
    /// Attempts arrive ordered by attempt number, so the last one carrying a voice is the most
    /// recent thing that actually happened. Earlier attempts are not overwritten or averaged —
    /// they keep their own recorded voices on their own rows, because two attempts of one job
    /// can genuinely have used different voices.
    /// </para>
    /// </summary>
    private static RecordedVoiceRegion ReadRecordedVoiceRegion(
        List<CallAttemptEntity> attempts)
    {
        for (int index = attempts.Count - 1; index >= 0; index--)
        {
            string? region = attempts[index].VoiceRegion;
            if (!string.IsNullOrWhiteSpace(region))
            {
                return new RecordedVoiceRegion(region, RecordedVoiceRegionSource);
            }
        }

        return new RecordedVoiceRegion(null, null);
    }

    private static string? ReadVoiceRegion(string? summaryJson)
    {
        if (string.IsNullOrWhiteSpace(summaryJson))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(summaryJson);
            string? deliveryArea = ReadString(document.RootElement, "delivery_area_short");
            return DeliveryRegionResolver.TryResolve(deliveryArea)?.ToString();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ReadOrderCodeShort(string? summaryJson)
    {
        if (string.IsNullOrWhiteSpace(summaryJson))
        {
            return string.Empty;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(summaryJson);
            return document.RootElement.TryGetProperty("order_code_short", out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static double Ratio(int value, int total) =>
        total == 0 ? 0d : Math.Round((double)value / total, 4);

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? ReadBool(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value)
            && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    private static DateTimeOffset? ReadTimestamp(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            && value.TryGetDateTimeOffset(out DateTimeOffset parsed)
            ? parsed
            : null;

    private static string[] ReadStringArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string[] CollectReferences(
        string? taskJson,
        string? jobJson,
        IEnumerable<CallAttemptEntity> attempts,
        IEnumerable<CallResultEntity> results,
        bool audit = false)
    {
        IEnumerable<string> attemptRefs = attempts.SelectMany(
            attempt => ReadStringArray(audit ? attempt.AuditRefsJson : attempt.EvidenceRefsJson));
        IEnumerable<string> resultRefs = results.SelectMany(
            result => ReadStringArray(audit ? result.AuditRefsJson : result.EvidenceRefsJson));

        return ReadStringArray(taskJson)
            .Concat(ReadStringArray(jobJson))
            .Concat(attemptRefs)
            .Concat(resultRefs)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string? NormalizeProgram(string? program)
    {
        if (string.IsNullOrWhiteSpace(program))
        {
            return null;
        }

        string normalized = program.Trim().ToUpperInvariant();
        return normalized is "GOLDEN_HOUR" or "TWENTY_FOUR_SEVEN"
            ? normalized
            : throw IvrErrors.MalformedRequest(
                "program must be GOLDEN_HOUR or TWENTY_FOUR_SEVEN.");
    }

    private static void RequireOrderedRange(DateTimeOffset? rangeStart, DateTimeOffset? rangeEnd)
    {
        if (rangeStart is not null && rangeEnd is not null && rangeStart > rangeEnd)
        {
            throw IvrErrors.MalformedRequest("from must not be later than to.");
        }
    }

    private static void RequireSafeFilter(string? value, string field)
    {
        if (value is null)
        {
            return;
        }

        if (value.Length > 128)
        {
            throw IvrErrors.MalformedRequest($"{field} is too long.");
        }

        try
        {
            PiiGuard.EnsureSafeText(value);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }
    }

    private sealed record QueueStatusCount(string QueueStatus, bool Closed, int Count);

    private sealed record ResultTypeCount(string ResultType, int Count);
}
