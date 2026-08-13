using Ivr.Domain.Confirmation;

namespace Ivr.Domain.Scheduling;

public sealed record SchedulerQueueItem
{
    public SchedulerQueueItem(
        string jobId,
        IvrProgramCode program,
        DateTimeOffset deadline,
        DateTimeOffset dueAt,
        int dueOffsetSeconds,
        int attemptNumber,
        int riskScore,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        if (!Enum.IsDefined(program))
        {
            throw new InvalidOperationException("Scheduler queue item has an unknown program.");
        }

        if (deadline <= createdAt || dueAt >= deadline)
        {
            throw new InvalidOperationException("Scheduler queue timing must remain inside the confirmation window.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(dueOffsetSeconds);
        if (attemptNumber is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(riskScore);
        JobId = jobId;
        Program = program;
        Deadline = deadline;
        DueAt = dueAt;
        DueOffsetSeconds = dueOffsetSeconds;
        AttemptNumber = attemptNumber;
        RiskScore = riskScore;
        CreatedAt = createdAt;
    }

    public string JobId { get; }

    public IvrProgramCode Program { get; }

    public DateTimeOffset Deadline { get; }

    public DateTimeOffset DueAt { get; }

    public int DueOffsetSeconds { get; }

    public int AttemptNumber { get; }

    public int RiskScore { get; }

    public DateTimeOffset CreatedAt { get; }
}

public sealed record SchedulerChannelAvailability
{
    public SchedulerChannelAvailability(string channelId, DateTimeOffset availableAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        ChannelId = channelId;
        AvailableAt = availableAt;
    }

    public string ChannelId { get; }

    public DateTimeOffset AvailableAt { get; }
}

public sealed record SchedulerCapacityPlan(
    bool FitsTargetDeadline,
    int ChannelCount,
    int PendingDispatches,
    int MissedDeadlineCount,
    DateTimeOffset? LastTargetDispatchAt);

public static class DeadlineScheduler
{
    public static IReadOnlyList<SchedulerQueueItem> Order(
        IEnumerable<SchedulerQueueItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items
            .OrderBy(item => item.Deadline)
            .ThenBy(item => ProgramPriority(item.Program))
            .ThenBy(item => item.DueOffsetSeconds)
            .ThenByDescending(item => item.RiskScore)
            .ThenBy(item => item.CreatedAt)
            .ThenBy(item => item.JobId, StringComparer.Ordinal)
            .ThenBy(item => item.AttemptNumber)
            .ToArray();
    }

    public static SchedulerCapacityPlan CalculateCapacity(
        IEnumerable<SchedulerQueueItem> items,
        IEnumerable<SchedulerChannelAvailability> channels,
        DateTimeOffset evaluatedAt,
        TimeSpan expectedCallDuration,
        string targetJobId)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetJobId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            expectedCallDuration,
            TimeSpan.Zero);

        IReadOnlyList<SchedulerQueueItem> ordered = Order(items);
        List<ChannelSlot> slots = channels
            .Select(channel => new ChannelSlot(
                channel.ChannelId,
                channel.AvailableAt < evaluatedAt ? evaluatedAt : channel.AvailableAt))
            .OrderBy(channel => channel.AvailableAt)
            .ThenBy(channel => channel.ChannelId, StringComparer.Ordinal)
            .ToList();
        int missed = 0;
        bool targetMissed = false;
        DateTimeOffset? lastTargetDispatch = null;
        foreach (SchedulerQueueItem item in ordered)
        {
            if (slots.Count == 0)
            {
                missed++;
                targetMissed |= string.Equals(item.JobId, targetJobId, StringComparison.Ordinal);
                continue;
            }

            ChannelSlot slot = slots[0];
            DateTimeOffset startsAt = Max(evaluatedAt, item.DueAt, slot.AvailableAt);
            if (startsAt >= item.Deadline)
            {
                missed++;
                targetMissed |= string.Equals(item.JobId, targetJobId, StringComparison.Ordinal);
                continue;
            }

            slot.AvailableAt = startsAt.Add(expectedCallDuration);
            slots.Sort(ChannelSlotComparer.Instance);
            if (string.Equals(item.JobId, targetJobId, StringComparison.Ordinal))
            {
                lastTargetDispatch = startsAt;
            }
        }

        bool targetPresent = ordered.Any(item =>
            string.Equals(item.JobId, targetJobId, StringComparison.Ordinal));
        return new SchedulerCapacityPlan(
            targetPresent && !targetMissed,
            slots.Count,
            ordered.Count,
            missed,
            lastTargetDispatch);
    }

    private static int ProgramPriority(IvrProgramCode program) => program switch
    {
        IvrProgramCode.GoldenHour => 0,
        IvrProgramCode.TwentyFourSeven => 1,
        _ => throw new InvalidOperationException("Unknown IVR program cannot be scheduled."),
    };

    private static DateTimeOffset Max(
        DateTimeOffset first,
        DateTimeOffset second,
        DateTimeOffset third) =>
        first >= second
            ? first >= third ? first : third
            : second >= third ? second : third;

    private sealed class ChannelSlot(string channelId, DateTimeOffset availableAt)
    {
        public string ChannelId { get; } = channelId;

        public DateTimeOffset AvailableAt { get; set; } = availableAt;
    }

    private sealed class ChannelSlotComparer : IComparer<ChannelSlot>
    {
        public static ChannelSlotComparer Instance { get; } = new();

        public int Compare(ChannelSlot? left, ChannelSlot? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            int byAvailability = left.AvailableAt.CompareTo(right.AvailableAt);
            return byAvailability != 0
                ? byAvailability
                : StringComparer.Ordinal.Compare(left.ChannelId, right.ChannelId);
        }
    }
}

public sealed record SchedulerAttemptProgress(
    int CompletedCustomerAttempts,
    int TechnicalRetryCount,
    bool HasFinalResult)
{
    public int? NextCustomerAttemptNumber(int maxCustomerAttempts)
    {
        if (maxCustomerAttempts is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCustomerAttempts));
        }

        if (CompletedCustomerAttempts < 0 || CompletedCustomerAttempts > maxCustomerAttempts)
        {
            throw new InvalidOperationException("Customer-attempt progress is outside the policy snapshot.");
        }

        return HasFinalResult || CompletedCustomerAttempts == maxCustomerAttempts
            ? null
            : CompletedCustomerAttempts + 1;
    }

    public bool CanUseTechnicalRetry(int boundedRetryLimit)
    {
        if (boundedRetryLimit is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(boundedRetryLimit));
        }

        if (TechnicalRetryCount < 0)
        {
            throw new InvalidOperationException("Technical retry count cannot be negative.");
        }

        return !HasFinalResult && TechnicalRetryCount < boundedRetryLimit;
    }
}
