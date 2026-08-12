namespace Ivr.Infrastructure.Idempotency;

public interface IIdempotencyStore
{
    public Task<TResponse> ExecuteAsync<TResponse>(
        string key,
        string payloadHash,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken = default);
}
