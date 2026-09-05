using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Ivr.Infrastructure.FeatureFlags;

/// <summary>
/// Reads the runtime flag snapshot, with a fail-closed fallback when the store cannot answer.
/// <para>
/// W-0190 added the logger and the counter. The fallback was already correct - an unreadable
/// provider must degrade to the safe default rather than to the last permissive value - but it
/// was silent, and a silent fallback is indistinguishable from a working read. That is how an
/// empty store survived: every caller saw plausible safe values and nothing anywhere said the
/// read had failed. The logger is optional so the many hand-constructed instances in the test
/// suite keep compiling; the container supplies one in every real host.
/// </para>
/// </summary>
public sealed partial class FeatureFlagPlatform(
    IFeatureFlagStore store,
    TimeProvider timeProvider,
    ILogger<FeatureFlagPlatform>? logger = null)
    : IFeatureFlags, IDynamicConfig, IFeatureFlagRefresher
{
    /// <summary>
    /// Counts reads that fell back to the safe default. Non-zero is always a defect: either the
    /// store is down or it is misconfigured, and both need an operator.
    /// </summary>
    public const string ReadFallbackCounterName = "ivr.feature_flags.read_fallback";

    private static readonly Meter Meter = new("Ivr.FeatureFlags");

    private static readonly Counter<long> ReadFallbacks = Meter.CreateCounter<long>(
        ReadFallbackCounterName,
        unit: "{read}",
        description: "Flag reads that could not reach the store and returned the safe default.");

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
        catch (Exception exception)
        {
            cache.TryRemove(environment, out _);
            ReadFallbacks.Add(1, new KeyValuePair<string, object?>("environment", environment));

            // The exception type, not the exception: its message can carry a store detail, and
            // this line is written on a path that runs before every dispatch decision. Nothing
            // here is customer data, but not passing raw provider text into a log is the habit
            // that keeps it that way.
            if (logger is not null)
            {
                LogReadFallback(logger, environment, exception.GetType().Name);
            }
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

    [LoggerMessage(
        EventId = 2400,
        Level = LogLevel.Warning,
        Message = "Feature flag read for {Environment} fell back to the safe default; "
            + "the store did not answer. ExceptionType={ExceptionType}")]
    private static partial void LogReadFallback(
        ILogger logger,
        string environment,
        string exceptionType);

    private sealed record CacheEntry(FeatureFlagSnapshot Snapshot, DateTimeOffset ExpiresAt);
}
