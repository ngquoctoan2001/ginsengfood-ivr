namespace Ivr.Infrastructure.FeatureFlags;

public sealed class KillSwitch(IFeatureFlags featureFlags) : IKillSwitch
{
    public async Task<bool> RealCallsEnabledAsync(
        string environment,
        CancellationToken cancellationToken = default)
    {
        FeatureFlagReadResult result = await featureFlags.GetSnapshotAsync(
            environment,
            true,
            cancellationToken);
        return result.ProviderReadable && !result.Snapshot.GlobalDialKillSwitch;
    }
}
