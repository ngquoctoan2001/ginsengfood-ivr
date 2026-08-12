using Ivr.Domain.Errors;
using Ivr.Infrastructure.Idempotency;

namespace Ivr.Infrastructure.FeatureFlags;

public interface IFeatureFlagCommandIdempotency
{
    public Task<TResponse> ExecuteAsync<TResponse>(
        string key,
        string payloadHash,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken = default);
}

public sealed class FeatureFlagCommandIdempotency(IIdempotencyStore store)
    : IFeatureFlagCommandIdempotency
{
    public Task<TResponse> ExecuteAsync<TResponse>(
        string key,
        string payloadHash,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken = default) =>
        store.ExecuteAsync(key, payloadHash, factory, cancellationToken);
}

public sealed class UnavailableFeatureFlagCommandIdempotency
    : IFeatureFlagCommandIdempotency
{
    public Task<TResponse> ExecuteAsync<TResponse>(
        string key,
        string payloadHash,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken = default) =>
        throw IvrErrors.OperationalBlocked(
            "Persistent command idempotency requires the P1-2 migration.");
}
