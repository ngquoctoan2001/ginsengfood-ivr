using System.Collections.Concurrent;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed class FeatureFlagPlatform(
    IFeatureFlagStore store,
    TimeProvider timeProvider)
    : IFeatureFlags, IDynamicConfig, IFeatureFlagRefresher
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(15);
    private readonly ConcurrentDictionary<string, CacheEntry> cache = new(StringComparer.Ordinal);

    public Task<FeatureFlagReadResult> GetConfigAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default) =>
        GetSnapshotAsync(environment, forceFresh, cancellationToken);

    public async Task<FeatureFlagReadResult> GetSnapshotAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default)
    {
        ValidateEnvironment(environment);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (!forceFresh
            && cache.TryGetValue(environment, out CacheEntry? cached)
            && cached.ExpiresAt > now)
        {
            return new FeatureFlagReadResult(cached.Snapshot, true, true);
        }

        try
        {
            FeatureFlagSnapshot snapshot = await store.ReadFreshAsync(
                environment,
                cancellationToken);
            cache[environment] = new CacheEntry(snapshot, now.Add(CacheLifetime));
            return new FeatureFlagReadResult(snapshot, true, false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            cache.TryRemove(environment, out _);
            return new FeatureFlagReadResult(
                FeatureFlagSnapshot.SafeDefault(environment),
                false,
                false);
        }
    }

    public Task<FeatureFlagReadResult> RefreshAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
        GetSnapshotAsync(environment, true, cancellationToken);

    private static void ValidateEnvironment(string environment)
    {
        if (!FeatureFlagEnvironments.All.Contains(environment))
        {
            throw new ArgumentOutOfRangeException(nameof(environment), environment, "Unknown environment.");
        }
    }

    private sealed record CacheEntry(FeatureFlagSnapshot Snapshot, DateTimeOffset ExpiresAt);
}
