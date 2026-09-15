using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// SIP-02. The <c>OD-V1-17</c> dial-token ceiling, now that it outlives the process.
/// <para>
/// <c>DialTokenResolveLedgerTests</c> pins the rules against the in-memory ledger and is the
/// authority on what they are. These tests exist for the property that one cannot check: the count
/// used to live in a <c>ConcurrentDictionary</c> owned by a vault instance, so a restart cleared it
/// and two workers each kept their own. A token policy allowed to dial twice could dial twice per
/// process, indefinitely, as long as processes kept restarting - which is the whole guarantee
/// <c>OD-V1-17</c> was chosen to provide.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class DialTokenResolveLedgerPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A restart does not hand the token a fresh budget. This is the defect, stated as a test.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-01")]
    public async Task TheCeilingSurvivesARestart()
    {
        await fixture.ResetAsync();
        const string token = "enc:lab-sha256:DURABLE-RESTART";

        // Each ledger instance stands for a process. Nothing is shared between them but the
        // database, which is the point.
        var before = new PostgresDialTokenResolveLedger(Factory());
        Assert.True((await before.EvaluateAsync(Request("attempt-1", token), Now)).Allowed);
        Assert.True((await before.EvaluateAsync(Request("attempt-2", token), Now)).Allowed);

        var afterRestart = new PostgresDialTokenResolveLedger(Factory());
        DialTokenResolveDecision third = await afterRestart.EvaluateAsync(
            Request("attempt-3", token),
            Now);

        Assert.False(third.Allowed);
        Assert.Equal(DialTokenRefusalCodes.ResolveLimitExceeded, third.RefusalCode);
        Assert.Equal(2, third.ResolveCount);
    }

    /// <summary>
    /// Two processes racing the same token share one budget rather than getting one each.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-02")]
    public async Task TwoProcessesRacingOneTokenShareItsBudget()
    {
        await fixture.ResetAsync();
        const string token = "enc:lab-sha256:DURABLE-RACE";
        var workerA = new PostgresDialTokenResolveLedger(Factory());
        var workerB = new PostgresDialTokenResolveLedger(Factory());

        // Four distinct attempts against a ceiling of two, started together. Run in sequence this
        // would prove nothing: the interesting question is what the advisory lock does when both
        // read before either writes.
        DialTokenResolveDecision[] decisions = await Task.WhenAll(
            workerA.EvaluateAsync(Request("attempt-1", token), Now).AsTask(),
            workerB.EvaluateAsync(Request("attempt-2", token), Now).AsTask(),
            workerA.EvaluateAsync(Request("attempt-3", token), Now).AsTask(),
            workerB.EvaluateAsync(Request("attempt-4", token), Now).AsTask());

        Assert.Equal(2, decisions.Count(decision => decision.Allowed));
        Assert.All(
            decisions.Where(decision => !decision.Allowed),
            decision => Assert.Equal(
                DialTokenRefusalCodes.ResolveLimitExceeded,
                decision.RefusalCode));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Equal(2, await context.DialTokenResolves.CountAsync());
    }

    /// <summary>
    /// The token is bound to the first task that used it, across processes and across restarts.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-03")]
    public async Task BindingToTheFirstTaskOutlivesTheProcessThatMadeIt()
    {
        await fixture.ResetAsync();
        const string token = "enc:lab-sha256:DURABLE-BINDING";

        var first = new PostgresDialTokenResolveLedger(Factory());
        Assert.True((await first.EvaluateAsync(
            Request("attempt-1", token, taskId: "TASK-OWNS-IT"),
            Now)).Allowed);

        var second = new PostgresDialTokenResolveLedger(Factory());
        DialTokenResolveDecision stolen = await second.EvaluateAsync(
            Request("attempt-2", token, taskId: "TASK-DOES-NOT"),
            Now);

        Assert.False(stolen.Allowed);
        Assert.Equal(DialTokenRefusalCodes.TaskMismatch, stolen.RefusalCode);

        // And the refusal costs the owning task nothing. A token refused for the wrong task must
        // not spend the budget of the task it does belong to.
        DialTokenResolveDecision own = await second.EvaluateAsync(
            Request("attempt-3", token, taskId: "TASK-OWNS-IT"),
            Now);

        Assert.True(own.Allowed);
        Assert.Equal(2, own.ResolveCount);
    }

    /// <summary>
    /// A replayed attempt is refused as a replay and costs nothing, even from another process.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-04")]
    public async Task AReplayedAttemptIsRefusedAcrossProcessesAndCostsNothing()
    {
        await fixture.ResetAsync();
        const string token = "enc:lab-sha256:DURABLE-REPLAY";

        var first = new PostgresDialTokenResolveLedger(Factory());
        Assert.True((await first.EvaluateAsync(Request("attempt-1", token), Now)).Allowed);

        var second = new PostgresDialTokenResolveLedger(Factory());
        DialTokenResolveDecision replay = await second.EvaluateAsync(
            Request("attempt-1", token),
            Now);

        Assert.False(replay.Allowed);
        Assert.Equal(DialTokenRefusalCodes.AttemptReplay, replay.RefusalCode);
        Assert.Equal(1, replay.ResolveCount);

        // Retrying the transport of one resolve is not a second dial, so the budget is intact.
        Assert.True((await second.EvaluateAsync(Request("attempt-2", token), Now)).Allowed);
    }

    /// <summary>
    /// An expired token and a missing ceiling are refused without writing anything.
    /// <para>
    /// Worth pinning separately from the in-memory ledger: these two return before the lock, so
    /// the thing being checked is that the short-circuit did not also skip being correct.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-05")]
    public async Task AnExpiredTokenAndAMissingCeilingRecordNothing()
    {
        await fixture.ResetAsync();
        var ledger = new PostgresDialTokenResolveLedger(Factory());

        DialTokenResolveDecision expired = await ledger.EvaluateAsync(
            Request("attempt-1", "enc:lab-sha256:DURABLE-EXPIRED", expiresAt: Now),
            Now);
        DialTokenResolveDecision noCeiling = await ledger.EvaluateAsync(
            Request("attempt-1", "enc:lab-sha256:DURABLE-NO-CEILING", maxResolves: 0),
            Now);

        Assert.False(expired.Allowed);
        Assert.Equal(DialTokenRefusalCodes.Expired, expired.RefusalCode);
        Assert.False(noCeiling.Allowed);
        Assert.Equal(DialTokenRefusalCodes.CeilingMissing, noCeiling.RefusalCode);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Equal(0, await context.DialTokenResolves.CountAsync());
    }

    /// <summary>
    /// What lands in the table is a hash, never the value the vault revealed.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-TOKEN-DURABLE-06")]
    public async Task TheTableStoresAHashAndNeverTheToken()
    {
        await fixture.ResetAsync();
        const string token = "enc:lab-sha256:DURABLE-SECRECY";
        var ledger = new PostgresDialTokenResolveLedger(Factory());

        Assert.True((await ledger.EvaluateAsync(Request("attempt-1", token), Now)).Allowed);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        DialTokenResolveEntity row = await context.DialTokenResolves.SingleAsync();

        Assert.DoesNotContain(token, row.TokenHash, StringComparison.Ordinal);
        Assert.Equal(64, row.TokenHash.Length);
        Assert.Matches("^[0-9A-F]{64}$", row.TokenHash);
    }

    private static DialTokenResolutionRequest Request(
        string attemptId,
        string token,
        string taskId = "TASK-DURABLE",
        int maxResolves = 2,
        DateTimeOffset? expiresAt = null) => new(
        DialTokenReference.Create(token, expiresAt ?? Now.AddMinutes(5)),
        AttemptId.Create(attemptId),
        TaskId.Create(taskId),
        maxResolves);

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();
}
