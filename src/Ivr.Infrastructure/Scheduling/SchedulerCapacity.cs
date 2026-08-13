using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Scheduling;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scheduling;

public sealed class SchedulerOptions
{
    public const string SectionName = "Ivr:Scheduler";

    public bool Enabled { get; set; }

    public int MockChannelCount { get; set; } = 1;

    public int ExpectedCallDurationSeconds { get; set; } = 60;

    public int LeaseDurationSeconds { get; set; } = 120;

    public int RecoveryQuarantineSeconds { get; set; } = 600;

    public int TechnicalRetryLimit { get; set; } = 1;

    public int ClaimBatchSize { get; set; } = 64;

    public int PollIntervalMilliseconds { get; set; } = 1000;
}

public sealed class SchedulerOptionsValidator : IValidateOptions<SchedulerOptions>
{
    public ValidateOptionsResult Validate(string? name, SchedulerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string> failures = [];
        RequireRange(options.MockChannelCount, 1, 256, nameof(options.MockChannelCount), failures);
        RequireRange(
            options.ExpectedCallDurationSeconds,
            1,
            900,
            nameof(options.ExpectedCallDurationSeconds),
            failures);
        RequireRange(
            options.LeaseDurationSeconds,
            options.ExpectedCallDurationSeconds,
            3600,
            nameof(options.LeaseDurationSeconds),
            failures);
        RequireRange(
            options.RecoveryQuarantineSeconds,
            1,
            86400,
            nameof(options.RecoveryQuarantineSeconds),
            failures);
        RequireRange(
            options.TechnicalRetryLimit,
            0,
            10,
            nameof(options.TechnicalRetryLimit),
            failures);
        RequireRange(options.ClaimBatchSize, 1, 512, nameof(options.ClaimBatchSize), failures);
        RequireRange(
            options.PollIntervalMilliseconds,
            100,
            60000,
            nameof(options.PollIntervalMilliseconds),
            failures);
        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void RequireRange(
        int value,
        int minimum,
        int maximum,
        string key,
        List<string> failures)
    {
        if (value < minimum || value > maximum)
        {
            failures.Add($"{key} must be between {minimum} and {maximum}.");
        }
    }
}

public sealed record SchedulerCapacityRequest(
    string JobId,
    IvrProgramCode Program,
    DateTimeOffset WindowStartedAt,
    DateTimeOffset Deadline,
    DateTimeOffset CreatedAt,
    IReadOnlyList<int> AttemptOffsetsSeconds,
    int RiskScore);

public sealed record SchedulerCapacitySnapshot(
    bool SourceAvailable,
    bool FitsBeforeDeadline,
    string SessionId,
    int ActiveChannelCount,
    int PendingDispatches,
    int ExpiredJobs,
    int MissedDeadlineCount,
    string? ShortageReason,
    string EvidenceRef);

public sealed record SchedulerExecutionContext(string ExecutionMode);

public interface ISchedulerCapacityService
{
    public ValueTask<SchedulerCapacitySnapshot> CalculateAsync(
        SchedulerCapacityRequest request,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default);
}

public sealed class MockSchedulerCapacityService(
    IOptions<SchedulerOptions> options) : ISchedulerCapacityService
{
    public ValueTask<SchedulerCapacitySnapshot> CalculateAsync(
        SchedulerCapacityRequest request,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        string sessionId = string.Concat("MOCK-SCHED-", request.JobId);
        SchedulerCapacityPlan plan = DeadlineScheduler.CalculateCapacity(
            SchedulerCapacityMapper.CreateQueueItems(request),
            Enumerable.Range(1, options.Value.MockChannelCount)
                .Select(index => new SchedulerChannelAvailability(
                    $"MOCK-CHANNEL-{index:D3}",
                    evaluatedAt)),
            evaluatedAt,
            TimeSpan.FromSeconds(options.Value.ExpectedCallDurationSeconds),
            request.JobId);
        return ValueTask.FromResult(SchedulerCapacityMapper.Map(
            plan,
            sessionId,
            expiredJobs: 0,
            "evidence://ivr/p2-3/mock-scheduler-capacity"));
    }
}

public sealed class PostgresSchedulerCapacityService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    SchedulerExecutionContext executionContext,
    IOptions<SchedulerOptions> schedulerOptions) : ISchedulerCapacityService
{
    public async ValueTask<SchedulerCapacitySnapshot> CalculateAsync(
        SchedulerCapacityRequest request,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        string executionMode = executionContext.ExecutionMode;
        List<SimChannelEntity> storedChannels = await context.SimChannels
            .AsNoTracking()
            .Where(channel => channel.Enabled
                && channel.ExecutionMode == executionMode
                && channel.Status != "DISABLED"
                && channel.Status != "QUARANTINED"
                && channel.Status != "HEALTH_FAILED")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        SchedulerChannelAvailability[] channels = storedChannels
            .Where(channel => channel.Status == "IDLE" && channel.LeaseToken is null)
            .Select(channel => new SchedulerChannelAvailability(
                channel.SimChannelId,
                AvailableAt(channel, evaluatedAt)))
            .ToArray();

        List<CallJobEntity> jobs = await context.CallJobs
            .AsNoTracking()
            .Where(job => job.IvrCallJobId != request.JobId
                && job.Eligible
                && (job.Status == "READY_FOR_SCHEDULER"
                    || job.Status == "DISPATCH_LEASED")
                && (job.QueueStatus == "QUEUED" || job.QueueStatus == "LEASED"))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        string[] jobIds = jobs.Select(job => job.IvrCallJobId).ToArray();
        Dictionary<string, int> countedAttempts = await context.CallAttempts
            .AsNoTracking()
            .Where(attempt => jobIds.Contains(attempt.IvrCallJobId)
                && attempt.IsCountedCustomerAttempt)
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new { JobId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken)
            .ConfigureAwait(false);
        HashSet<string> finalJobs = (await context.CallResults
                .AsNoTracking()
                .Where(result => jobIds.Contains(result.IvrCallJobId) && result.IsFinalForIvr)
                .Select(result => result.IvrCallJobId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        Dictionary<string, int> riskScores = await context.ConfirmationTasks
            .AsNoTracking()
            .Where(task => jobs.Select(job => job.TaskId).Contains(task.TaskId))
            .ToDictionaryAsync(
                task => task.TaskId,
                task => SchedulerCapacityMapper.RiskScore(task.RiskFlagsJson),
                cancellationToken)
            .ConfigureAwait(false);

        var items = new List<SchedulerQueueItem>();
        foreach (CallJobEntity job in jobs.Where(job => !finalJobs.Contains(job.IvrCallJobId)))
        {
            int completed = countedAttempts.GetValueOrDefault(job.IvrCallJobId);
            items.AddRange(SchedulerCapacityMapper.CreateQueueItems(
                job,
                completed,
                riskScores.GetValueOrDefault(job.TaskId)));
        }

        items.AddRange(SchedulerCapacityMapper.CreateQueueItems(request));
        SchedulerCapacityPlan plan = DeadlineScheduler.CalculateCapacity(
            items,
            channels,
            evaluatedAt,
            TimeSpan.FromSeconds(schedulerOptions.Value.ExpectedCallDurationSeconds),
            request.JobId);
        int expiredJobs = jobs.Count(job => job.ExpiresAt <= evaluatedAt);
        string sessionId = string.Concat(
            "SCHED-",
            evaluatedAt.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            "-",
            request.JobId);
        return SchedulerCapacityMapper.Map(
            plan,
            sessionId,
            expiredJobs,
            string.Concat("evidence://ivr/p2-3/scheduler-capacity/", request.JobId));
    }

    private static DateTimeOffset AvailableAt(
        SimChannelEntity channel,
        DateTimeOffset evaluatedAt)
    {
        DateTimeOffset availableAt = evaluatedAt;
        if (channel.LeaseExpiresAt is { } leaseExpiresAt && leaseExpiresAt > availableAt)
        {
            availableAt = leaseExpiresAt;
        }

        if (channel.CooldownUntil is { } cooldownUntil && cooldownUntil > availableAt)
        {
            availableAt = cooldownUntil;
        }

        return availableAt;
    }
}

public sealed class UnavailableSchedulerCapacityService : ISchedulerCapacityService
{
    public ValueTask<SchedulerCapacitySnapshot> CalculateAsync(
        SchedulerCapacityRequest request,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new SchedulerCapacitySnapshot(
            false,
            false,
            "SCHEDULER-CAPACITY-UNAVAILABLE",
            0,
            0,
            0,
            0,
            "SCHEDULER_CAPACITY_NOT_CONFIGURED",
            "evidence://ivr/p2-3/scheduler-capacity-unavailable"));
    }
}

public static class SchedulerCapacityMapper
{
    public static IReadOnlyList<SchedulerQueueItem> CreateQueueItems(
        SchedulerCapacityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.AttemptOffsetsSeconds.Select((offset, index) => new SchedulerQueueItem(
            request.JobId,
            request.Program,
            request.Deadline,
            request.WindowStartedAt.AddSeconds(offset),
            offset,
            index + 1,
            request.RiskScore,
            request.CreatedAt)).ToArray();
    }

    public static IReadOnlyList<SchedulerQueueItem> CreateQueueItems(
        CallJobEntity job,
        int completedCustomerAttempts,
        int riskScore)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (completedCustomerAttempts < 0 || completedCustomerAttempts > job.MaxAttempts)
        {
            throw new InvalidOperationException("Stored customer-attempt progress is invalid.");
        }

        int[] offsets = Deserialize<int[]>(job.AttemptOffsetsSecondsJson) ?? [];
        DateTimeOffset[] schedule = Deserialize<DateTimeOffset[]>(job.AttemptScheduleJson) ?? [];
        if (offsets.Length != job.MaxAttempts || schedule.Length != job.MaxAttempts)
        {
            throw new InvalidOperationException("Stored scheduler policy snapshot is inconsistent.");
        }

        IvrProgramCode program = job.ProgramType switch
        {
            "GOLDEN_HOUR" => IvrProgramCode.GoldenHour,
            "TWENTY_FOUR_SEVEN" => IvrProgramCode.TwentyFourSeven,
            _ => throw new InvalidOperationException("Stored scheduler program is unknown."),
        };
        return Enumerable.Range(completedCustomerAttempts, job.MaxAttempts - completedCustomerAttempts)
            .Select(index => new SchedulerQueueItem(
                job.IvrCallJobId,
                program,
                job.ExpiresAt,
                schedule[index],
                offsets[index],
                index + 1,
                riskScore,
                job.CreatedAt))
            .ToArray();
    }

    public static SchedulerCapacitySnapshot Map(
        SchedulerCapacityPlan plan,
        string sessionId,
        int expiredJobs,
        string evidenceRef)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceRef);
        ArgumentOutOfRangeException.ThrowIfNegative(expiredJobs);
        return new SchedulerCapacitySnapshot(
            true,
            plan.FitsTargetDeadline,
            sessionId,
            plan.ChannelCount,
            plan.PendingDispatches,
            expiredJobs,
            plan.MissedDeadlineCount,
            plan.FitsTargetDeadline ? null : "NO_CHANNEL_BEFORE_DEADLINE",
            evidenceRef);
    }

    public static int RiskScore(string? riskFlagsJson)
    {
        string[]? flags = Deserialize<string[]>(riskFlagsJson);
        return flags?.Length ?? (string.IsNullOrWhiteSpace(riskFlagsJson) ? 0 : 1);
    }

    private static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

public static class SchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddIvrScheduling(
        this IServiceCollection services,
        IConfiguration configuration,
        string executionMode,
        bool useMockCapacity)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(executionMode);
        IConfigurationSection section = configuration.GetSection(SchedulerOptions.SectionName);
        services.AddOptions<SchedulerOptions>()
            .Configure(options =>
            {
                options.MockChannelCount = section.GetValue(
                    nameof(SchedulerOptions.MockChannelCount),
                    options.MockChannelCount);
                options.Enabled = section.GetValue(
                    nameof(SchedulerOptions.Enabled),
                    options.Enabled);
                options.ExpectedCallDurationSeconds = section.GetValue(
                    nameof(SchedulerOptions.ExpectedCallDurationSeconds),
                    options.ExpectedCallDurationSeconds);
                options.LeaseDurationSeconds = section.GetValue(
                    nameof(SchedulerOptions.LeaseDurationSeconds),
                    options.LeaseDurationSeconds);
                options.RecoveryQuarantineSeconds = section.GetValue(
                    nameof(SchedulerOptions.RecoveryQuarantineSeconds),
                    options.RecoveryQuarantineSeconds);
                options.TechnicalRetryLimit = section.GetValue(
                    nameof(SchedulerOptions.TechnicalRetryLimit),
                    options.TechnicalRetryLimit);
                options.ClaimBatchSize = section.GetValue(
                    nameof(SchedulerOptions.ClaimBatchSize),
                    options.ClaimBatchSize);
                options.PollIntervalMilliseconds = section.GetValue(
                    nameof(SchedulerOptions.PollIntervalMilliseconds),
                    options.PollIntervalMilliseconds);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<SchedulerOptions>, SchedulerOptionsValidator>());
        services.TryAddSingleton(new SchedulerExecutionContext(executionMode));
        if (useMockCapacity)
        {
            services.TryAddSingleton<ISchedulerCapacityService,
                MockSchedulerCapacityService>();
        }
        else
        {
            services.TryAddSingleton<ISchedulerCapacityService,
                PostgresSchedulerCapacityService>();
        }

        services.TryAddSingleton<IPostgresSchedulerStore, PostgresSchedulerStore>();
        services.TryAddSingleton<ISchedulerDispatchGateway,
            UnavailableSchedulerDispatchGateway>();
        services.TryAddSingleton<ISchedulerRuntime, SchedulerRuntime>();
        return services;
    }
}
