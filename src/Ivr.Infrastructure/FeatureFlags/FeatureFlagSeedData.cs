using System.Text.Json;

namespace Ivr.Infrastructure.FeatureFlags;

internal static class FeatureFlagSeedData
{
    public static IReadOnlyCollection<FeatureFlagEntity> All() =>
        FeatureFlagEnvironments.All
            .SelectMany(CreateRows)
            .ToArray();

    private static IEnumerable<FeatureFlagEntity> CreateRows(string environment)
    {
        FeatureFlagSnapshot snapshot = FeatureFlagSnapshot.SafeDefault(environment);
        yield return Row(environment, FeatureFlagKeys.ExecutionMode, true, snapshot.ExecutionMode);
        yield return Row(environment, FeatureFlagKeys.SalesProvider, true, snapshot.SalesProvider);
        yield return Row(environment, FeatureFlagKeys.SimProvider, true, snapshot.SimProvider);
        yield return Row(
            environment,
            FeatureFlagKeys.AttemptPolicyVersion,
            true,
            snapshot.AttemptPolicyVersion);
        yield return Row(
            environment,
            FeatureFlagKeys.RealCustomerCallAllowed,
            snapshot.RealCustomerCallAllowed,
            snapshot.RealCustomerCallAllowed);
        yield return Row(
            environment,
            FeatureFlagKeys.LabDestinationAllowlist,
            false,
            snapshot.LabDestinationAllowlist);
        yield return Row(
            environment,
            FeatureFlagKeys.GlobalDialKillSwitch,
            snapshot.GlobalDialKillSwitch,
            snapshot.GlobalDialKillSwitch);
        yield return Row(
            environment,
            FeatureFlagKeys.V1NotificationEnabled,
            snapshot.V1NotificationEnabled,
            snapshot.V1NotificationEnabled);
        yield return Row(
            environment,
            FeatureFlagKeys.RecordingEnabled,
            snapshot.RecordingEnabled,
            snapshot.RecordingEnabled);
    }

    private static FeatureFlagEntity Row(
        string environment,
        string key,
        bool enabled,
        object value) => new()
        {
            Key = key,
            Environment = environment,
            Enabled = enabled,
            ValueJson = JsonSerializer.Serialize(value),
            UpdatedBy = "bootstrap",
            UpdatedAt = DateTimeOffset.UnixEpoch,
            Reason = "safe bootstrap seed",
        };
}
