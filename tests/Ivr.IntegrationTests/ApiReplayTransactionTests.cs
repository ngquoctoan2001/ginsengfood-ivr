using Ivr.Domain.Errors;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ApiReplayTransactionTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-API-REPLAY-TX-01")]
    public async Task NestedCommandAndConcurrentHttpRetriesCommitOneResponse()
    {
        await fixture.ResetAsync();
        var factory = fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
        var store = new PostgresIdempotencyStore(factory, TimeProvider.System);
        int calls = 0;
        Task<int>[] retries = Enumerable.Range(0, 8).Select(_ => store.ExecuteCoordinatedAsync(
            "matrix-parent", "matrix-payload", async cancellation =>
            {
                Interlocked.Increment(ref calls);
                return await store.ExecuteAsync("matrix-child", "matrix-child-payload",
                    _ => Task.FromResult(1), cancellation) + 1;
            })).ToArray();
        Assert.All(await Task.WhenAll(retries), result => Assert.Equal(2, result));
        Assert.Equal(1, calls);
        IvrFailureException conflict = await Assert.ThrowsAsync<IvrFailureException>(() =>
            store.ExecuteCoordinatedAsync("matrix-parent", "changed-payload", _ => Task.FromResult(3)));
        Assert.Equal(IvrErrorCodes.IdempotencyConflict, conflict.ErrorCode);
        await using IvrDbContext db = await factory.CreateDbContextAsync();
        Assert.Equal(2, await db.IdempotencyKeys.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-API-REPLAY-TX-02")]
    public async Task FailedFactoryDoesNotCacheAnErrorResponse()
    {
        await fixture.ResetAsync();
        var factory = fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
        var store = new PostgresIdempotencyStore(factory, TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ExecuteCoordinatedAsync<int>(
            "matrix-failed", "matrix-payload", _ => throw new InvalidOperationException("Synthetic failure")));
        await using (IvrDbContext db = await factory.CreateDbContextAsync())
            Assert.Equal(0, await db.IdempotencyKeys.CountAsync());
        Assert.Equal(7, await store.ExecuteCoordinatedAsync("matrix-failed", "matrix-payload", _ => Task.FromResult(7)));
    }
}
