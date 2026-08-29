using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Enforces DT-04 for every runtime path that records a SIM-channel failure.
/// </summary>
internal static class SimChannelFailurePolicy
{
    internal const int AutoDisableThreshold = 3;

    internal static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(10);

    internal static bool RecordFailure(SimChannelEntity channel, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(channel);

        bool startsNewWindow = channel.FailureWindowStartedAt is null
            || occurredAt < channel.FailureWindowStartedAt.Value
            || occurredAt - channel.FailureWindowStartedAt.Value > FailureWindow;
        if (startsNewWindow)
        {
            channel.FailureWindowStartedAt = occurredAt;
            channel.FailCount = 1;
        }
        else
        {
            channel.FailCount++;
        }

        return channel.FailCount >= AutoDisableThreshold;
    }

    internal static void RecordHealthy(SimChannelEntity channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        channel.FailCount = 0;
        channel.FailureWindowStartedAt = null;
    }
}
