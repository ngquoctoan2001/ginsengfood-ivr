using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Errors;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IInternalAdminApiService
{
    public Task<EligibilityApiResult> EvaluateEligibilityAsync(string taskId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<CallJobApiResult> GetCallJobAsync(string jobId, CancellationToken cancellationToken);
    public Task<CallJobApiResult> ReassertCallJobAsync(CallJobLifecycleRequest request, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<CallAttemptApiResult> ReassertAttemptAsync(CallAttemptLifecycleRequest request, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<CallResultApiResult> ReassertResultAsync(CallResultLifecycleRequest request, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<ResultCallbackApiResult> ReassertCallbackAsync(ResultCallbackLifecycleRequest request, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<QueueProjectionApiResult> GetQueueAsync(CancellationToken cancellationToken);
    public Task<AdminActionApiResult> PauseQueueAsync(AdminMutationRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<AdminActionApiResult> ResumeQueueAsync(AdminMutationRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<AdminActionApiResult> DisableChannelAsync(string channelId, AdminMutationRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<AdminActionApiResult> EnableChannelAsync(string channelId, AdminMutationRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<TechnicalRetryApiResult> RetryTechnicalExceptionAsync(TechnicalRetryRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
    public Task<AdminReviewApiResult> ReviewAsync(AdminReviewRequest request, string actorId, string correlationId, string idempotencyKey, CancellationToken cancellationToken);
}

public sealed class InternalAdminApiService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IEligibilityService eligibilityService,
    IIdempotencyStore idempotencyStore,
    IFeatureFlags featureFlags,
    IOptions<IvrOptions> ivrOptions,
    IOptions<SchedulerOptions> schedulerOptions,
    TimeProvider timeProvider) : IInternalAdminApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string AdminPauseScope = "ADMIN_QUEUE_PAUSE";
    private const string FoundationIdempotencyScope = "foundation";

    public Task<EligibilityApiResult> EvaluateEligibilityAsync(
        string taskId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        RequireSafe(taskId, nameof(taskId));
        return IdempotentAsync(
            "record-eligibility",
            idempotencyKey,
            new { taskId },
            async token =>
            {
                EligibilityEvaluation evaluation;
                try
                {
                    evaluation = await eligibilityService.EvaluateAsync(taskId, correlationId, token)
                        .ConfigureAwait(false);
                }
                catch (KeyNotFoundException)
                {
                    throw IvrErrors.NotFound("The confirmation task was not found.");
                }
                catch (InvalidOperationException exception) when (string.Equals(
                    exception.Message,
                    "The task already has a different eligibility decision.",
                    StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch(
                        "The task already has a different eligibility decision.");
                }

                return new EligibilityApiResult(
                    taskId,
                    evaluation.Decision,
                    evaluation.Reasons.Select(reason => reason.Code).ToArray(),
                    evaluation.EvidenceRefs.Count > 0
                        ? evaluation.EvidenceRefs[0]
                        : "evidence://ivr/p2-8/eligibility/fail-closed");
            },
            cancellationToken);
    }

    public async Task<CallJobApiResult> GetCallJobAsync(
        string jobId,
        CancellationToken cancellationToken)
    {
        RequireSafe(jobId, nameof(jobId));
        await using IvrDbContext context = await CreateContextAsync(cancellationToken);
        CallJobEntity job = await context.CallJobs.AsNoTracking().SingleOrDefaultAsync(
            entity => entity.IvrCallJobId == jobId,
            cancellationToken).ConfigureAwait(false)
            ?? throw IvrErrors.NotFound("The call job was not found.");
        return Map(job);
    }

    public Task<CallJobApiResult> ReassertCallJobAsync(
        CallJobLifecycleRequest request,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return IdempotentAsync(
            "record-call-job",
            idempotencyKey,
            request,
            async token =>
            {
                CallJobApiResult job = await GetCallJobAsync(request.IvrCallJobId, token);
                if (!string.Equals(job.TaskId, request.TaskId, StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The call job does not belong to the supplied task.");
                }

                return job;
            },
            cancellationToken);
    }

    public Task<CallAttemptApiResult> ReassertAttemptAsync(
        CallAttemptLifecycleRequest request,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return IdempotentAsync(
            "record-call-attempt",
            idempotencyKey,
            request,
            async token =>
            {
                await using IvrDbContext context = await CreateContextAsync(token);
                CallAttemptEntity attempt = await context.CallAttempts.AsNoTracking()
                    .SingleOrDefaultAsync(
                        entity => entity.IvrCallAttemptId == request.IvrCallAttemptId,
                        token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The call attempt was not found.");
                if (!string.Equals(attempt.IvrCallJobId, request.IvrCallJobId, StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The attempt does not belong to the supplied call job.");
                }

                return Map(attempt);
            },
            cancellationToken);
    }

    public Task<CallResultApiResult> ReassertResultAsync(
        CallResultLifecycleRequest request,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return IdempotentAsync(
            "record-call-result",
            idempotencyKey,
            request,
            async token =>
            {
                await using IvrDbContext context = await CreateContextAsync(token);
                CallResultEntity result = await context.CallResults.AsNoTracking()
                    .SingleOrDefaultAsync(
                        entity => entity.IvrCallResultId == request.IvrCallResultId,
                        token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The normalized result was not found.");
                if (!string.Equals(result.IvrCallJobId, request.IvrCallJobId, StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The result does not belong to the supplied call job.");
                }

                return Map(result);
            },
            cancellationToken);
    }

    public Task<ResultCallbackApiResult> ReassertCallbackAsync(
        ResultCallbackLifecycleRequest request,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return IdempotentAsync(
            "record-result-callback",
            idempotencyKey,
            request,
            async token =>
            {
                await using IvrDbContext context = await CreateContextAsync(token);
                ResultCallbackEntity callback = await context.ResultCallbacks.AsNoTracking()
                    .SingleOrDefaultAsync(
                        entity => entity.CallbackId == request.CallbackId,
                        token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The result callback lifecycle was not found.");
                if (!string.Equals(callback.IvrCallResultId, request.IvrCallResultId, StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The callback does not belong to the supplied result.");
                }

                return Map(callback);
            },
            cancellationToken);
    }

    public async Task<QueueProjectionApiResult> GetQueueAsync(CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await CreateContextAsync(cancellationToken);
        int pending = await context.CallJobs.AsNoTracking().CountAsync(
            job => job.ClosedAt == null
                && (job.QueueStatus == "QUEUED" || job.QueueStatus == "HELD_MOCK"),
            cancellationToken).ConfigureAwait(false);
        int active = await context.CallAttempts.AsNoTracking().CountAsync(
            attempt => attempt.Status == "LEASED_PENDING_DISPATCH"
                || attempt.Status == "DIALING"
                || attempt.Status == "ACTIVE_CALL",
            cancellationToken).ConfigureAwait(false);
        int channels = await context.SimChannels.AsNoTracking().CountAsync(
            channel => channel.Enabled,
            cancellationToken).ConfigureAwait(false);
        int holds = await context.CapacityIncidents.AsNoTracking().CountAsync(
            incident => incident.Status == "OPEN"
                && incident.HoldNewCalls
                && incident.Scope == AdminPauseScope,
            cancellationToken).ConfigureAwait(false);
        return new QueueProjectionApiResult(
            holds > 0,
            pending,
            active,
            channels,
            holds,
            timeProvider.GetUtcNow());
    }

    public Task<AdminActionApiResult> PauseQueueAsync(
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAdminAsync(
            "pause-queue",
            idempotencyKey,
            request,
            actorId,
            correlationId,
            IvrPermissions.QueuePause,
            async (context, now, token) =>
            {
                Validate(request);
                CapacityIncidentEntity? incident = await context.CapacityIncidents
                    .SingleOrDefaultAsync(
                        item => item.Status == "OPEN" && item.Scope == AdminPauseScope,
                        token).ConfigureAwait(false);
                string before = incident is null ? "RUNNING" : "PAUSED";
                if (incident is null)
                {
                    incident = new CapacityIncidentEntity
                    {
                        CapacityIncidentId = string.Concat("CAP-ADMIN-", Guid.NewGuid().ToString("N")),
                        SessionId = string.Concat("ADMIN-QUEUE-", now.ToUnixTimeMilliseconds()),
                        ProgramCode = "ALL",
                        Status = "OPEN",
                        Scope = AdminPauseScope,
                        HoldNewCalls = true,
                        ActiveSimCount = await context.SimChannels.CountAsync(channel => channel.Enabled, token),
                        PendingCallJobs = await context.CallJobs.CountAsync(job => job.ClosedAt == null, token),
                        OpenedAt = now,
                        Reason = request.Reason.Trim(),
                    };
                    context.CapacityIncidents.Add(incident);
                }

                return CreateAdminMutation(
                    "QUEUE_PAUSE",
                    IvrPermissions.QueuePause,
                    actorId,
                    "queue",
                    "global",
                    request,
                    correlationId,
                    JsonSerializer.Serialize(new { status = before }, JsonOptions),
                    JsonSerializer.Serialize(new { status = "PAUSED" }, JsonOptions),
                    now);
            },
            cancellationToken);

    public Task<AdminActionApiResult> ResumeQueueAsync(
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAdminAsync(
            "resume-queue",
            idempotencyKey,
            request,
            actorId,
            correlationId,
            IvrPermissions.QueueResume,
            async (context, now, token) =>
            {
                Validate(request);
                CapacityIncidentEntity? incident = await context.CapacityIncidents
                    .SingleOrDefaultAsync(
                        item => item.Status == "OPEN" && item.Scope == AdminPauseScope,
                        token).ConfigureAwait(false);
                string before = incident is null ? "RUNNING" : "PAUSED";
                if (incident is not null)
                {
                    incident.Status = "RESOLVED";
                    incident.HoldNewCalls = false;
                    incident.ResolvedAt = now;
                }

                return CreateAdminMutation(
                    "QUEUE_RESUME",
                    IvrPermissions.QueueResume,
                    actorId,
                    "queue",
                    "global",
                    request,
                    correlationId,
                    JsonSerializer.Serialize(new { status = before }, JsonOptions),
                    JsonSerializer.Serialize(new { status = "RUNNING" }, JsonOptions),
                    now);
            },
            cancellationToken);

    public Task<AdminActionApiResult> DisableChannelAsync(
        string channelId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        MutateChannelAsync(
            false,
            channelId,
            request,
            actorId,
            correlationId,
            idempotencyKey,
            cancellationToken);

    public Task<AdminActionApiResult> EnableChannelAsync(
        string channelId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        MutateChannelAsync(
            true,
            channelId,
            request,
            actorId,
            correlationId,
            idempotencyKey,
            cancellationToken);

    public Task<TechnicalRetryApiResult> RetryTechnicalExceptionAsync(
        TechnicalRetryRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return AtomicAdminAsync(
            "technical-retry",
            idempotencyKey,
            request,
            async (context, token) =>
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                TechnicalExceptionEntity technical = await context.TechnicalExceptions
                    .SingleOrDefaultAsync(
                        item => item.TechnicalExceptionId == request.TechnicalExceptionId,
                        token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The technical exception was not found.");
                if (technical.TechnicalRetryCount >= schedulerOptions.Value.TechnicalRetryLimit)
                {
                    throw IvrErrors.PolicyMismatch(
                        "The technical retry limit has already been reached.");
                }

                CallAttemptEntity attempt = await context.CallAttempts.SingleOrDefaultAsync(
                    item => item.IvrCallAttemptId == request.TargetAttemptId,
                    token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The target attempt was not found.");
                if (!string.Equals(technical.IvrCallAttemptId, attempt.IvrCallAttemptId, StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The technical exception does not belong to the target attempt.");
                }

                CallJobEntity job = await context.CallJobs.SingleAsync(
                    item => item.IvrCallJobId == attempt.IvrCallJobId,
                    token).ConfigureAwait(false);
                ConfirmationTaskEntity task = await context.ConfirmationTasks.SingleAsync(
                    item => item.TaskId == job.TaskId,
                    token).ConfigureAwait(false);
                await RequireRetryAllowedAsync(context, technical, attempt, job, task, now, token);

                int beforeCount = technical.TechnicalRetryCount;
                technical.TechnicalRetryCount++;
                technical.RetryReason = request.Reason.Trim();
                attempt.TechnicalRetryCount++;
                attempt.Status = "TECHNICAL_RETRY_QUEUED";
                attempt.IsCountedCustomerAttempt = false;
                job.Status = string.Equals(
                    ivrOptions.Value.ExecutionMode,
                    IvrOptions.MockExecutionMode,
                    StringComparison.OrdinalIgnoreCase)
                    ? "DRY_RUN"
                    : "READY_FOR_SCHEDULER";
                job.QueueStatus = string.Equals(
                    ivrOptions.Value.ExecutionMode,
                    IvrOptions.MockExecutionMode,
                    StringComparison.OrdinalIgnoreCase)
                    ? "HELD_MOCK"
                    : "QUEUED";
                AdminMutation mutation = CreateAdminMutation(
                    "TECHNICAL_RETRY",
                    IvrPermissions.ManualRetry,
                    actorId,
                    "technical-exception",
                    technical.TechnicalExceptionId,
                    new AdminMutationRequest(request.Reason, request.EvidenceRef),
                    correlationId,
                    JsonSerializer.Serialize(new { retry_count = beforeCount }, JsonOptions),
                    JsonSerializer.Serialize(new
                    {
                        retry_count = technical.TechnicalRetryCount,
                        customer_attempt_counted = false,
                        job.QueueStatus,
                    }, JsonOptions),
                    now);
                PersistAdminMutation(context, mutation);
                return new TechnicalRetryApiResult(
                    mutation.Action.AdminActionId,
                    technical.TechnicalExceptionId,
                    attempt.IvrCallAttemptId,
                    technical.TechnicalRetryCount,
                    false,
                    job.QueueStatus,
                    true);
            },
            cancellationToken);
    }

    public Task<AdminReviewApiResult> ReviewAsync(
        AdminReviewRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        Validate(request);
        return AtomicAdminAsync(
            "admin-review",
            idempotencyKey,
            request,
            async (context, token) =>
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                ReviewItemEntity review = await context.ReviewItems.SingleOrDefaultAsync(
                    item => item.ReviewItemId == request.ReviewItemId,
                    token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The review item was not found.");
                if (!string.Equals(review.Status, "OPEN", StringComparison.Ordinal))
                {
                    throw IvrErrors.PolicyMismatch("The review item is already resolved.");
                }
                string before = JsonSerializer.Serialize(new
                {
                    review.Status,
                    review.Resolution,
                    review.AssignedTo,
                }, JsonOptions);
                review.Status = "RESOLVED";
                review.Resolution = request.Resolution.Trim();
                review.AssignedTo = actorId;
                review.ResolvedAt = now;
                string after = JsonSerializer.Serialize(new
                {
                    review.Status,
                    review.Resolution,
                    review.AssignedTo,
                }, JsonOptions);
                AdminMutation mutation = CreateAdminMutation(
                    "RESULT_REVIEW",
                    IvrPermissions.ResultReview,
                    actorId,
                    "review-item",
                    review.ReviewItemId,
                    new AdminMutationRequest(request.Reason, request.EvidenceRef),
                    correlationId,
                    before,
                    after,
                    now);
                PersistAdminMutation(context, mutation);
                return new AdminReviewApiResult(
                    mutation.Action.AdminActionId,
                    review.ReviewItemId,
                    review.Status,
                    review.Resolution,
                    true,
                    true);
            },
            cancellationToken);
    }

    private Task<AdminActionApiResult> MutateChannelAsync(
        bool enable,
        string channelId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        RequireSafe(channelId, nameof(channelId));
        Validate(request);
        string operation = enable ? "enable-channel" : "disable-channel";
        string permission = enable ? IvrPermissions.SimEnable : IvrPermissions.SimDisable;
        return ExecuteAdminAsync(
            operation,
            idempotencyKey,
            new { channelId, request },
            actorId,
            correlationId,
            permission,
            async (context, now, token) =>
            {
                SimChannelEntity channel = await context.SimChannels.SingleOrDefaultAsync(
                    item => item.SimChannelId == channelId,
                    token).ConfigureAwait(false)
                    ?? throw IvrErrors.NotFound("The SIM channel was not found.");
                string before = JsonSerializer.Serialize(new
                {
                    channel.Enabled,
                    channel.Status,
                    channel.ActiveCallJobId,
                    channel.LeaseFencingGeneration,
                }, JsonOptions);
                if (enable)
                {
                    if (!string.Equals(channel.AdapterMode, "MOCK", StringComparison.Ordinal)
                        || channel.Status is "QUARANTINED" or "HEALTH_FAILED"
                        || channel.QuarantineUntil is not null
                        || channel.FailCount > 0
                        || channel.ActiveCallJobId is not null
                        || channel.LeaseToken is not null)
                    {
                        throw IvrErrors.OperationalBlocked(
                            "The channel cannot be enabled until adapter and health reconciliation pass.");
                    }

                    channel.Enabled = true;
                    channel.Status = "IDLE";
                    channel.DisabledReason = null;
                }
                else
                {
                    channel.Enabled = false;
                    channel.DisabledReason = request.Reason.Trim();
                    if (channel.ActiveCallJobId is null && channel.LeaseToken is null)
                    {
                        channel.Status = "DISABLED";
                    }
                }

                string after = JsonSerializer.Serialize(new
                {
                    channel.Enabled,
                    channel.Status,
                    channel.ActiveCallJobId,
                    channel.LeaseFencingGeneration,
                }, JsonOptions);
                return CreateAdminMutation(
                    enable ? "SIM_ENABLE" : "SIM_DISABLE",
                    permission,
                    actorId,
                    "sim-channel",
                    channel.SimChannelId,
                    request,
                    correlationId,
                    before,
                    after,
                    now);
            },
            cancellationToken);
    }

    private Task<AdminActionApiResult> ExecuteAdminAsync<TPayload>(
        string operation,
        string idempotencyKey,
        TPayload payload,
        string actorId,
        string correlationId,
        string permission,
        Func<IvrDbContext, DateTimeOffset, CancellationToken, Task<AdminMutation>> mutation,
        CancellationToken cancellationToken)
    {
        RequireSafe(actorId, nameof(actorId));
        RequireSafe(correlationId, nameof(correlationId));
        return AtomicAdminAsync(
            operation,
            idempotencyKey,
            payload,
            async (context, token) =>
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                AdminMutation created = await mutation(context, now, token).ConfigureAwait(false);
                if (!string.Equals(created.Action.Permission, permission, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Admin action permission mapping drifted.");
                }

                PersistAdminMutation(context, created);
                return new AdminActionApiResult(
                    created.Action.AdminActionId,
                    created.Action.ActionType,
                    created.Action.TargetType,
                    created.Action.TargetId,
                    "APPLIED",
                    created.Action.CorrelationId,
                    created.Action.NoPolicyBypass);
            },
            cancellationToken);
    }

    private async Task RequireRetryAllowedAsync(
        IvrDbContext context,
        TechnicalExceptionEntity technical,
        CallAttemptEntity attempt,
        CallJobEntity job,
        ConfirmationTaskEntity task,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (technical.CustomerAttemptCounted
            || attempt.IsCountedCustomerAttempt
            || !technical.TechnicalRetryAllowed
            || !attempt.TechnicalRetryAllowed
            || technical.TechnicalRetryCount >= schedulerOptions.Value.TechnicalRetryLimit
            || attempt.TechnicalRetryCount >= schedulerOptions.Value.TechnicalRetryLimit
            || !job.Eligible
            || job.ClosedAt is not null
            || job.ExpiresAt <= now
            || task.CallRestriction
            || !string.Equals(
                task.EligibilityDecision,
                EligibilityDecisions.Eligible,
                StringComparison.Ordinal))
        {
            throw IvrErrors.PolicyMismatch(
                "The technical retry is not allowed by the stored policy snapshot.");
        }

        bool finalExists = await context.CallResults.AnyAsync(
            result => result.IvrCallJobId == job.IvrCallJobId && result.IsFinalForIvr,
            cancellationToken).ConfigureAwait(false);
        bool queueHeld = await context.CapacityIncidents.AnyAsync(
            incident => incident.Status == "OPEN"
                && incident.HoldNewCalls
                && incident.Scope == AdminPauseScope,
            cancellationToken).ConfigureAwait(false);
        if (finalExists || queueHeld)
        {
            throw IvrErrors.OperationalBlocked(
                "The technical retry is blocked by final state or an active queue hold.");
        }

        string environment = ivrOptions.Value.ExecutionMode switch
        {
            "MOCK" => FeatureFlagEnvironments.Development,
            "LAB_REAL_SIM" => FeatureFlagEnvironments.Lab,
            "PRODUCTION_REAL" => FeatureFlagEnvironments.Production,
            _ => throw IvrErrors.OperationalBlocked("The execution mode is unsupported."),
        };
        FeatureFlagReadResult flags = await featureFlags.GetSnapshotAsync(
            environment,
            true,
            cancellationToken).ConfigureAwait(false);
        if (!flags.ProviderReadable
            || flags.Snapshot.GlobalDialKillSwitch
            || !flags.Snapshot.LabDestinationAllowlist.Contains(task.PhoneRef))
        {
            throw IvrErrors.OperationalBlocked(
                "Runtime configuration or destination allowlist blocks the technical retry.");
        }

        if (!string.Equals(
            ivrOptions.Value.ExecutionMode,
            IvrOptions.MockExecutionMode,
            StringComparison.OrdinalIgnoreCase))
        {
            if (!ivrOptions.Value.RealCustomerCallAllowed)
            {
                throw IvrErrors.OperationalBlocked("Real customer calls are disabled.");
            }

        }
    }

    private Task<TResponse> IdempotentAsync<TPayload, TResponse>(
        string operation,
        string key,
        TPayload payload,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken)
    {
        RequireSafe(key, nameof(key));
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new { operation, payload }, JsonOptions);
        string rawHash = Convert.ToHexString(SHA256.HashData(bytes));
        string payloadHash = string.Concat(
            "SHA256-",
            string.Join('-', rawHash.Chunk(8).Select(chunk => new string(chunk))));
        return idempotencyStore.ExecuteAsync(key, payloadHash, factory, cancellationToken);
    }

    private async Task<TResponse> AtomicAdminAsync<TPayload, TResponse>(
        string operation,
        string key,
        TPayload payload,
        Func<IvrDbContext, CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken)
    {
        RequireSafe(key, nameof(key));
        ArgumentNullException.ThrowIfNull(factory);
        string payloadHash = ComputePayloadHash(operation, payload);
        await using IvrDbContext context = await CreateContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken).ConfigureAwait(false);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))",
            cancellationToken).ConfigureAwait(false);
        IdempotencyKeyEntity? existing = await context.IdempotencyKeys.FindAsync(
            [FoundationIdempotencyScope, key],
            cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
            {
                throw IvrErrors.IdempotencyConflict();
            }
            TResponse replay = JsonSerializer.Deserialize<TResponse>(
                existing.ResponseSnapshotJson,
                JsonOptions)
                ?? throw new InvalidOperationException(
                    "The stored admin idempotency response could not be restored.");
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
        }

        TResponse response = await factory(context, cancellationToken).ConfigureAwait(false);
        string snapshot = JsonSerializer.Serialize(response, JsonOptions);
        PiiGuard.EnsureSafeText(snapshot);
        context.IdempotencyKeys.Add(new IdempotencyKeyEntity
        {
            Scope = FoundationIdempotencyScope,
            Key = key,
            PayloadHash = payloadHash,
            ResponseSnapshotJson = snapshot,
            CreatedAt = timeProvider.GetUtcNow(),
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return response;
    }

    private static string ComputePayloadHash<TPayload>(string operation, TPayload payload)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new { operation, payload }, JsonOptions);
        string rawHash = Convert.ToHexString(SHA256.HashData(bytes));
        return string.Concat(
            "SHA256-",
            string.Join('-', rawHash.Chunk(8).Select(chunk => new string(chunk))));
    }

    private Task<IvrDbContext> CreateContextAsync(CancellationToken cancellationToken) =>
        dbContextFactory.CreateDbContextAsync(cancellationToken);

    private static AdminMutation CreateAdminMutation(
        string actionType,
        string permission,
        string actorId,
        string targetType,
        string targetId,
        AdminMutationRequest request,
        string correlationId,
        string before,
        string after,
        DateTimeOffset now)
    {
        Validate(request);
        RequireSafe(before, nameof(before));
        RequireSafe(after, nameof(after));
        string actionId = string.Concat("ADMIN-", Guid.NewGuid().ToString("N"));
        AdminActionEntity action = new()
        {
            AdminActionId = actionId,
            ActionType = actionType,
            Permission = permission,
            ActorId = actorId,
            TargetType = targetType,
            TargetId = targetId,
            Reason = request.Reason.Trim(),
            BeforeStateJson = before,
            AfterStateJson = after,
            CorrelationId = correlationId,
            EvidenceRef = request.EvidenceRef,
            NoPolicyBypass = true,
            CreatedAt = now,
        };
        AuditLogEntity audit = new()
        {
            AuditId = Guid.NewGuid(),
            ActorId = actorId,
            ActorType = "admin",
            Action = actionType,
            TargetType = targetType,
            TargetId = targetId,
            Reason = request.Reason.Trim(),
            BeforeStateJson = before,
            AfterStateJson = after,
            CorrelationId = correlationId,
            DataJson = JsonSerializer.Serialize(new
            {
                admin_action_id = actionId,
                permission,
                evidence_ref = request.EvidenceRef,
                no_policy_bypass = true,
            }, JsonOptions),
            CreatedAt = now,
        };
        RequireSafe(JsonSerializer.Serialize(action, JsonOptions), nameof(action));
        RequireSafe(JsonSerializer.Serialize(audit, JsonOptions), nameof(audit));
        return new AdminMutation(action, audit);
    }

    private static void PersistAdminMutation(IvrDbContext context, AdminMutation mutation)
    {
        context.AdminActions.Add(mutation.Action);
        context.AuditLog.Add(mutation.Audit);
    }

    private static void Validate(CallJobLifecycleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.IvrCallJobId, nameof(request.IvrCallJobId));
        RequireSafe(request.TaskId, nameof(request.TaskId));
    }

    private static void Validate(CallAttemptLifecycleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.IvrCallAttemptId, nameof(request.IvrCallAttemptId));
        RequireSafe(request.IvrCallJobId, nameof(request.IvrCallJobId));
    }

    private static void Validate(CallResultLifecycleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.IvrCallResultId, nameof(request.IvrCallResultId));
        RequireSafe(request.IvrCallJobId, nameof(request.IvrCallJobId));
    }

    private static void Validate(ResultCallbackLifecycleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.CallbackId, nameof(request.CallbackId));
        RequireSafe(request.IvrCallResultId, nameof(request.IvrCallResultId));
    }

    private static void Validate(AdminMutationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.Reason, nameof(request.Reason));
        if (request.Reason.Trim().Length > 500)
        {
            throw IvrErrors.MalformedRequest("The admin reason is too long.");
        }

        if (request.EvidenceRef is not null)
        {
            RequireSafe(request.EvidenceRef, nameof(request.EvidenceRef));
        }
    }

    private static void Validate(TechnicalRetryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.TechnicalExceptionId, nameof(request.TechnicalExceptionId));
        RequireSafe(request.TargetAttemptId, nameof(request.TargetAttemptId));
        Validate(new AdminMutationRequest(request.Reason, request.EvidenceRef));
    }

    private static void Validate(AdminReviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireSafe(request.ReviewItemId, nameof(request.ReviewItemId));
        RequireSafe(request.Resolution, nameof(request.Resolution));
        if (request.Resolution.Trim().Length > 500)
        {
            throw IvrErrors.MalformedRequest("The review resolution is too long.");
        }

        Validate(new AdminMutationRequest(request.Reason, request.EvidenceRef));
    }

    private static void RequireSafe(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw IvrErrors.MalformedRequest($"{field} is required.");
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

    private static CallJobApiResult Map(CallJobEntity job) => new(
        job.IvrCallJobId,
        job.TaskId,
        job.ProgramType,
        job.Status,
        job.QueueStatus,
        job.Eligible,
        job.MaxAttempts,
        job.ExpiresAt,
        job.InputSignalOnly,
        job.NoDirectOrderUpdate);

    private static CallAttemptApiResult Map(CallAttemptEntity attempt) => new(
        attempt.IvrCallAttemptId,
        attempt.IvrCallJobId,
        attempt.AttemptNumber,
        attempt.Status,
        attempt.ResultStatus,
        attempt.IsCountedCustomerAttempt,
        attempt.TechnicalRetryCount,
        attempt.SimChannelId);

    private static CallResultApiResult Map(CallResultEntity result) => new(
        result.IvrCallResultId,
        result.IvrCallJobId,
        result.ResultType,
        result.ResultReason,
        result.IsCountedCustomerAttempt,
        result.IsFinalForIvr,
        result.RecommendedCoreAction,
        result.InputSignalOnly,
        result.NoDirectOrderUpdate,
        result.NoPaymentOrRevenueEffect);

    private static ResultCallbackApiResult Map(ResultCallbackEntity callback) => new(
        callback.CallbackId,
        callback.IvrCallResultId,
        callback.ResultState,
        callback.DeliveryStatus,
        callback.RetryCount,
        callback.RequiresCoreRevalidation);

    private sealed record AdminMutation(AdminActionEntity Action, AuditLogEntity Audit);
}

public static class InternalAdminApiServiceCollectionExtensions
{
    public static IServiceCollection AddIvrInternalAdminApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<InternalServiceOptions>()
            .Configure(options =>
            {
                options.ServiceToken = configuration[InternalServiceOptions.TokenConfigurationKey]
                    ?? string.Empty;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceToken),
                $"{InternalServiceOptions.TokenConfigurationKey} is required.")
            .ValidateOnStart();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddSingleton<IInternalAdminApiService, InternalAdminApiService>();
        return services;
    }
}
