using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scheduling;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
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

    /// <summary>
    /// How many calls one worker may hold at once. SIP-05.
    /// <para>
    /// One by default, which is the behaviour every caller had before this option existed: a pass
    /// claimed a single lease and waited for the call. The plan raises it under evidence,
    /// 1 to 4 to 8 to 16 to 32, and the figure must never be set above the sessions the carrier
    /// has actually granted on the trunk.
    /// </para>
    /// <para>
    /// Per process, not per system. The shared ceiling is the row count in
    /// <c>ivr_sim_channels</c>, which the claim enforces under <c>SKIP LOCKED</c>.
    /// </para>
    /// </summary>
    public int MaxConcurrentDispatches { get; set; } = 1;

    /// <summary>
    /// How many calls one worker may start per second, separately from how many it may hold. SIP-05.
    /// <para>
    /// Separate because they are separate allowances: 32 free slots does not license 32 originate
    /// requests inside one second, and a carrier that tolerates the first will still reject the
    /// second. One by default, so a raised concurrency ceiling cannot turn into a burst on its own.
    /// </para>
    /// </summary>
    public int MaxCallStartsPerSecond { get; set; } = 1;

    /// <summary>
    /// How long shutdown waits for calls already in flight before reporting that it gave up.
    /// <para>
    /// Waiting only - it does not end them. The default covers a whole call rather than a polite
    /// pause: audio up to 120s, ring 30s and DTMF 15s, plus margin. A shorter figure looks tidier
    /// and is wrong twice over. It hangs up on a customer mid-sentence, and - because the ARI
    /// application is handed back only after a drain that finished - it leaves the scope held by a
    /// pod that is gone, so the next pod waits out the lease and then needs a person to isolate it.
    /// A deploy during a call would need human intervention every time.
    /// </para>
    /// <para>
    /// The pod has to be allowed to take this long: <c>terminationGracePeriodSeconds</c> on the
    /// worker Deployment is set from this figure, and a grace period shorter than it turns every
    /// drain into a SIGKILL.
    /// </para>
    /// </summary>
    public int DispatchDrainSeconds { get; set; } = 180;

    /// <summary>
    /// How long a controller grant stands without a heartbeat. SIP-05.
    /// <para>
    /// Every pass renews it, so the default is hundreds of missed renewals - long enough that a
    /// slow database or a GC pause cannot expire it. Expiry is not a handover: it only changes the
    /// answer a rival gets from "wait, it is held" to "wait, somebody needs to confirm it is
    /// gone", which is why this is not a failover time and why a longer value costs little.
    /// </para>
    /// <para>
    /// It must outlast <see cref="DispatchDrainSeconds"/>, and the validator enforces that. During
    /// a drain the loop has stopped, so nothing is renewing; a lease that expired mid-drain would
    /// show an orderly shutdown to onlookers as a controller that needs isolating, and an operator
    /// who acted on that would isolate a worker that was about to hand the scope back cleanly.
    /// </para>
    /// </summary>
    public int ControllerLeaseSeconds { get; set; } = 240;
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

        // 256 rather than 32 on both: the trunk figure is a contract number that belongs in the
        // deployed configuration, and hard-coding 32 here would be this file asserting a capacity
        // no carrier has granted yet. The lower bound is the load-bearing one - zero would be a
        // scheduler that is enabled, claims nothing, and reports itself healthy.
        RequireRange(
            options.MaxConcurrentDispatches,
            1,
            256,
            nameof(options.MaxConcurrentDispatches),
            failures);
        RequireRange(
            options.MaxCallStartsPerSecond,
            1,
            256,
            nameof(options.MaxCallStartsPerSecond),
            failures);
        RequireRange(
            options.DispatchDrainSeconds,
            1,
            600,
            nameof(options.DispatchDrainSeconds),
            failures);

        // The floor is the load-bearing end again. A lease shorter than a few poll intervals
        // would expire between heartbeats under ordinary latency, and every expiry is a scope
        // that needs a person to confirm it before anything dials on it again.
        RequireRange(
            options.ControllerLeaseSeconds,
            5,
            3600,
            nameof(options.ControllerLeaseSeconds),
            failures);

        // Cross-field, because the two are only wrong in relation to each other. Nothing renews
        // the grant during a drain - the loop has already stopped - so a lease shorter than the
        // drain expires every time a pod shuts down with a call still up. The scope then reads as
        // needing isolation at the exact moment it was about to be handed back cleanly, and an
        // operator who believed it would isolate a worker that was doing the right thing.
        if (options.ControllerLeaseSeconds < options.DispatchDrainSeconds)
        {
            failures.Add(
                $"{nameof(options.ControllerLeaseSeconds)} must be at least "
                + $"{nameof(options.DispatchDrainSeconds)}; nothing renews the controller grant "
                + "while a drain is running.");
        }

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
            .CreateDbContextAsync(cancellationToken);
        string executionMode = executionContext.ExecutionMode;
        List<SimChannelEntity> storedChannels = await context.SimChannels
            .AsNoTracking()
            .Where(channel => channel.Enabled
                && channel.ExecutionMode == executionMode
                && channel.Status != "DISABLED"
                && (channel.Status != "QUARANTINED"
                    || channel.QuarantineUntil <= evaluatedAt)
                && channel.Status != "HEALTH_FAILED")
            .ToListAsync(cancellationToken);
        SchedulerChannelAvailability[] channels = storedChannels
            .Where(channel => channel.LeaseToken is null
                && (channel.Status == "IDLE"
                    || (channel.Status == "QUARANTINED"
                        && channel.QuarantineUntil <= evaluatedAt)))
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
            .ToListAsync(cancellationToken);
        string[] jobIds = jobs.Select(job => job.IvrCallJobId).ToArray();
        Dictionary<string, int> countedAttempts = await context.CallAttempts
            .AsNoTracking()
            .Where(attempt => jobIds.Contains(attempt.IvrCallJobId)
                && attempt.IsCountedCustomerAttempt)
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new { JobId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken);
        HashSet<string> finalJobs = (await context.CallResults
                .AsNoTracking()
                .Where(result => jobIds.Contains(result.IvrCallJobId) && result.IsFinalForIvr)
                .Select(result => result.IvrCallJobId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        Dictionary<string, int> riskScores = await context.ConfirmationTasks
            .AsNoTracking()
            .Where(task => jobs.Select(job => job.TaskId).Contains(task.TaskId))
            .ToDictionaryAsync(
                task => task.TaskId,
                task => SchedulerCapacityMapper.RiskScore(task.RiskFlagsJson),
                cancellationToken);

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
        IConfigurationSection normalizationSection = configuration.GetSection(
            NormalizationOptions.SectionName);
        IConfigurationSection asteriskSection = configuration.GetSection(
            AsteriskAriOptions.SectionName);
        bool asteriskLab = string.Equals(
                executionMode,
                IvrOptions.LabRealSimExecutionMode,
                StringComparison.OrdinalIgnoreCase)
            && asteriskSection.GetValue<bool>(nameof(AsteriskAriOptions.Enabled));
        // W-0198 / OD-V1-16. The hours a customer may be telephoned. Bound and validated at
        // startup so an inverted or empty window is a deployment that refuses to start, rather
        // than a night on which nobody was called and nothing said why.
        IConfigurationSection callingWindowSection =
            configuration.GetSection(CallingWindowOptions.SectionName);
        services.AddOptions<CallingWindowOptions>()
            .Configure(options =>
            {
                options.Enabled = callingWindowSection.GetValue(
                    nameof(CallingWindowOptions.Enabled),
                    options.Enabled);
                options.UtcOffsetMinutes = callingWindowSection.GetValue(
                    nameof(CallingWindowOptions.UtcOffsetMinutes),
                    options.UtcOffsetMinutes);
                options.StartMinuteOfLocalDay = callingWindowSection.GetValue(
                    nameof(CallingWindowOptions.StartMinuteOfLocalDay),
                    options.StartMinuteOfLocalDay);
                options.EndMinuteOfLocalDay = callingWindowSection.GetValue(
                    nameof(CallingWindowOptions.EndMinuteOfLocalDay),
                    options.EndMinuteOfLocalDay);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CallingWindowOptions>,
                CallingWindowOptionsValidator>());
        services.TryAddSingleton<CallingWindow>();

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
                options.MaxConcurrentDispatches = section.GetValue(
                    nameof(SchedulerOptions.MaxConcurrentDispatches),
                    options.MaxConcurrentDispatches);
                options.MaxCallStartsPerSecond = section.GetValue(
                    nameof(SchedulerOptions.MaxCallStartsPerSecond),
                    options.MaxCallStartsPerSecond);
                options.DispatchDrainSeconds = section.GetValue(
                    nameof(SchedulerOptions.DispatchDrainSeconds),
                    options.DispatchDrainSeconds);
                options.ControllerLeaseSeconds = section.GetValue(
                    nameof(SchedulerOptions.ControllerLeaseSeconds),
                    options.ControllerLeaseSeconds);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<SchedulerOptions>, SchedulerOptionsValidator>());
        services.AddOptions<NormalizationOptions>()
            .Configure(options =>
            {
                options.Enabled = normalizationSection.GetValue(
                    nameof(NormalizationOptions.Enabled),
                    options.Enabled);
                options.BatchSize = normalizationSection.GetValue(
                    nameof(NormalizationOptions.BatchSize),
                    options.BatchSize);
                options.PollIntervalMilliseconds = normalizationSection.GetValue(
                    nameof(NormalizationOptions.PollIntervalMilliseconds),
                    options.PollIntervalMilliseconds);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<NormalizationOptions>, NormalizationOptionsValidator>());
        services.AddOptions<MockTelephonyOptions>()
            .Bind(configuration.GetSection(MockTelephonyOptions.SectionName))
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<MockTelephonyOptions>, MockTelephonyOptionsValidator>());
        services.AddOptions<AsteriskAriOptions>()
            .Bind(asteriskSection)
            .PostConfigure(options => options.ExecutionMode = executionMode)
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<AsteriskAriOptions>, AsteriskAriOptionsValidator>());
        services.TryAddSingleton(new SchedulerExecutionContext(executionMode));
        services.AddIvrSpeech(configuration, executionMode);
        if (useMockCapacity)
        {
            services.TryAddSingleton<ISchedulerCapacityService,
                MockSchedulerCapacityService>();
            services.TryAddSingleton(provider => new MockDialTokenVault(
                provider.GetRequiredService<IOptions<MockTelephonyOptions>>(),
                provider.GetRequiredService<IAuditLogger>()));
            services.Replace(ServiceDescriptor.Singleton<IOpaqueValueProtector>(provider =>
                provider.GetRequiredService<MockDialTokenVault>()));
            services.TryAddSingleton<IDialTokenResolver>(provider =>
                provider.GetRequiredService<MockDialTokenVault>());
            services.TryAddSingleton<ISpeechRenderer, ApprovedVietnameseSpeechRenderer>();
            services.TryAddSingleton<ISimGateway>(provider =>
            {
                MockTelephonyOptions options = provider
                    .GetRequiredService<IOptions<MockTelephonyOptions>>().Value;
                Dictionary<string, FakeSimScenario> scenarios = options.Scenarios.ToDictionary(
                    pair => pair.Key,
                    pair => new FakeSimScenario(
                        Enum.Parse<SimProviderDisposition>(
                            pair.Value.Disposition,
                            ignoreCase: true),
                        pair.Value.DtmfKey,
                        pair.Value.TechnicalErrorCode,
                        TimeSpan.FromMilliseconds(pair.Value.DialDelayMilliseconds),
                        TimeSpan.FromMilliseconds(pair.Value.PlayDelayMilliseconds),
                        TimeSpan.FromMilliseconds(pair.Value.CaptureDelayMilliseconds)),
                    StringComparer.Ordinal);
                return new FakeSimGateway(
                    scenarios,
                    timeProvider: provider.GetRequiredService<TimeProvider>());
            });
            services.TryAddSingleton<ITelephonyDispatchStore,
                PostgresTelephonyDispatchStore>();
            services.TryAddSingleton<ISchedulerDispatchGateway,
                MockSchedulerDispatchGateway>();
        }
        else if (asteriskLab)
        {
            services.TryAddSingleton<ISchedulerCapacityService,
                PostgresSchedulerCapacityService>();
            // SIP-02. The durable ledger, not the process-local one. The lab dials a real
            // softphone through a real Asterisk, so the ceiling it enforces has to be the ceiling
            // production enforces - a per-process count that a restart clears is a different rule
            // wearing the same name.
            services.TryAddSingleton<IDialTokenResolveLedger>(provider =>
                new PostgresDialTokenResolveLedger(
                    provider.GetRequiredService<IDbContextFactory<IvrDbContext>>()));
            services.TryAddSingleton(provider => new LabDialTokenVault(
                provider.GetRequiredService<IOptions<AsteriskAriOptions>>(),
                provider.GetRequiredService<IDialTokenResolveLedger>(),
                provider.GetRequiredService<IAuditLogger>()));
            services.Replace(ServiceDescriptor.Singleton<IOpaqueValueProtector>(provider =>
                provider.GetRequiredService<LabDialTokenVault>()));
            services.TryAddSingleton<IDialTokenResolver>(provider =>
                provider.GetRequiredService<LabDialTokenVault>());
            services.TryAddSingleton<ISpeechRenderer, ApprovedVietnameseSpeechRenderer>();
            services.AddHttpClient(nameof(AsteriskAriSimGateway));
            services.TryAddSingleton<ISimGateway, AsteriskAriSimGateway>();

            // The scope is the Asterisk application, because that is the unit a second WebSocket
            // can take over. Environment and mode join it so a lab controller and a production one
            // are never treated as rivals for the same socket.
            services.TryAddSingleton<IAriControllerOwnership>(provider =>
            {
                AsteriskAriOptions ari = provider
                    .GetRequiredService<IOptions<AsteriskAriOptions>>().Value;
                return new PostgresAriControllerOwnership(
                    provider.GetRequiredService<IDbContextFactory<IvrDbContext>>(),
                    provider.GetRequiredService<IOptions<SchedulerOptions>>(),
                    provider.GetRequiredService<TimeProvider>(),
                    string.Join(':', ari.ExecutionMode, ari.Environment, ari.Application),
                    provider.GetRequiredService<IAuditLogger>());
            });
            services.TryAddSingleton<ITelephonyDispatchStore,
                PostgresTelephonyDispatchStore>();
            services.TryAddSingleton<ISchedulerDispatchGateway,
                AsteriskSchedulerDispatchGateway>();
        }
        else
        {
            services.TryAddSingleton<ISchedulerCapacityService,
                PostgresSchedulerCapacityService>();
            services.TryAddSingleton<ISchedulerDispatchGateway,
                UnavailableSchedulerDispatchGateway>();
        }

        services.TryAddSingleton<IPostgresSchedulerStore, PostgresSchedulerStore>();

        // Registered for every mode, including the one whose dispatch gateway is unavailable. The
        // pump holds the count of calls in flight, and shutdown has to be able to ask that
        // question of a worker that was never allowed to dial as well as one that was.
        services.TryAddSingleton<SchedulerDispatchPump>();

        // Fallback for every mode with no ARI socket to lose - MOCK, whose dispatch is in-process,
        // and the unavailable branch, which never reaches the question. TryAdd, so the Asterisk
        // branch above keeps the real one.
        services.TryAddSingleton<IAriControllerOwnership>(
            _ => new UncontendedAriControllerOwnership("in-process"));
        services.TryAddSingleton<ISchedulerRuntime, SchedulerRuntime>();
        services.TryAddSingleton<IRawEventRepository, RawEventRepository>();
        services.TryAddSingleton<IResultRepository, ResultRepository>();
        return services;
    }
}
