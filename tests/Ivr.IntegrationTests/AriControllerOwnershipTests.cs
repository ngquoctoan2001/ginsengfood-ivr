using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

/// <summary>
/// SIP-05. Who may hold the ARI event socket, decided in the database against a real one.
/// <para>
/// These run against PostgreSQL rather than a fake because the property being tested is what two
/// processes see when they ask at the same time, and an in-memory double answers that question
/// with its own implementation instead of the one that ships.
/// </para>
/// <para>
/// The rule they exist to defend is the unintuitive one: an expired lease is not a vacancy. Every
/// lease-based design reaches for automatic failover on expiry, and here that would be a bug with
/// customers on the other end of it - Asterisk hands the application to whichever socket connected
/// most recently and tells the loser afterwards, so a worker that took over from a holder which had
/// merely gone quiet would start dialling while the old one was still on the line.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AriControllerOwnershipTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 9, 0, 0, TimeSpan.Zero);

    private const string Scope = "LAB_REAL_SIM:lab:ivr-lab";

    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-01")]
    public async Task OnlyOneWorkerHoldsTheApplicationAndTheOtherIsToldToWait()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        PostgresAriControllerOwnership first = Ownership(clock);
        PostgresAriControllerOwnership second = Ownership(clock);

        AriControllerGrant held = await first.AcquireOrRenewAsync("worker-a");
        AriControllerGrant refused = await second.AcquireOrRenewAsync("worker-b");

        Assert.Equal(AriControllerStatus.Held, held.Status);
        Assert.True(held.MayDial);
        Assert.Equal("worker-a", held.OwnerWorkerId);

        Assert.Equal(AriControllerStatus.HeldByAnotherWorker, refused.Status);
        Assert.False(refused.MayDial);

        // The loser is told who has it, not given a generation of its own. Two workers holding two
        // different generations of the same application is the state this exists to prevent.
        Assert.Equal("worker-a", refused.OwnerWorkerId);
        Assert.Equal(held.FencingGeneration, refused.FencingGeneration);
    }

    /// <summary>
    /// The rule worth paying for: a lease that ran out does not let the next worker in.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-02")]
    public async Task AnExpiredLeaseIsNotAVacancy()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        PostgresAriControllerOwnership incumbent = Ownership(clock);
        PostgresAriControllerOwnership challenger = Ownership(clock);

        AriControllerGrant held = await incumbent.AcquireOrRenewAsync("worker-a");
        Assert.True(held.MayDial);

        // Well past the 60s default. A node that lost its database connection but kept its ARI
        // socket reports exactly like this, and so does one that died - which is the point.
        clock.Advance(TimeSpan.FromMinutes(10));

        AriControllerGrant waiting = await challenger.AcquireOrRenewAsync("worker-b");

        Assert.Equal(AriControllerStatus.AwaitingIsolation, waiting.Status);
        Assert.False(waiting.MayDial);
        Assert.Equal("worker-a", waiting.OwnerWorkerId);
        Assert.Equal(held.FencingGeneration, waiting.FencingGeneration);

        // And the holder that went quiet can still come back and carry on. Nothing took the socket
        // from it, so nothing should have taken the right to dial on it either.
        AriControllerGrant resumed = await incumbent.AcquireOrRenewAsync("worker-a");

        Assert.Equal(AriControllerStatus.Held, resumed.Status);
        Assert.Equal(held.FencingGeneration, resumed.FencingGeneration);
    }

    /// <summary>
    /// A clean release is the one handover that needs nobody, because the worker making it has
    /// said it stopped.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-03")]
    public async Task ACleanReleaseLetsTheNextWorkerInWithoutAPerson()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        PostgresAriControllerOwnership outgoing = Ownership(clock);
        PostgresAriControllerOwnership incoming = Ownership(clock);

        AriControllerGrant held = await outgoing.AcquireOrRenewAsync("worker-a");
        await outgoing.ReleaseAsync("worker-a", "Deploy; drained.");

        AriControllerGrant next = await incoming.AcquireOrRenewAsync("worker-b");

        Assert.Equal(AriControllerStatus.Held, next.Status);
        Assert.True(next.MayDial);

        // No reconciliation demanded. The previous holder drained before releasing, so there is no
        // generation of calls left unaccounted for.
        Assert.Equal(held.FencingGeneration + 1, next.FencingGeneration);
    }

    /// <summary>
    /// After a forced handover the new holder owns the application and still may not dial.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-04")]
    public async Task ASeizedApplicationOwnsButDoesNotDialUntilReconciled()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        PostgresAriControllerOwnership incumbent = Ownership(clock);
        PostgresAriControllerOwnership challenger = Ownership(clock);

        await incumbent.AcquireOrRenewAsync("worker-a");
        clock.Advance(TimeSpan.FromMinutes(10));
        await challenger.IsolateAsync("ops-nguyen", "Node lost; ARI socket confirmed closed.");

        AriControllerGrant seized = await challenger.AcquireOrRenewAsync("worker-b");

        Assert.Equal(AriControllerStatus.AwaitingReconciliation, seized.Status);
        Assert.False(seized.MayDial);
        Assert.Equal("worker-b", seized.OwnerWorkerId);

        // Renewing does not quietly promote it. Owning and being allowed to dial stay separate
        // until somebody answers the second question.
        AriControllerGrant renewed = await challenger.AcquireOrRenewAsync("worker-b");
        Assert.Equal(AriControllerStatus.AwaitingReconciliation, renewed.Status);
        Assert.Equal(seized.FencingGeneration, renewed.FencingGeneration);

        await challenger.ConfirmReconciledAsync("ops-nguyen", "Channels accounted for.");
        AriControllerGrant cleared = await challenger.AcquireOrRenewAsync("worker-b");

        Assert.Equal(AriControllerStatus.Held, cleared.Status);
        Assert.True(cleared.MayDial);
        Assert.Equal(seized.FencingGeneration, cleared.FencingGeneration);
    }

    /// <summary>
    /// The isolated controller is the one worker that must never get the application back, and the
    /// one most likely to ask - it never noticed anything was wrong.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-05")]
    public async Task TheIsolatedControllerCannotTakeItselfBack()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        PostgresAriControllerOwnership zombie = Ownership(clock);

        AriControllerGrant held = await zombie.AcquireOrRenewAsync("worker-a");
        clock.Advance(TimeSpan.FromMinutes(10));
        await zombie.IsolateAsync("ops-nguyen", "Assumed gone.");

        AriControllerGrant refused = await zombie.AcquireOrRenewAsync("worker-a");

        Assert.Equal(AriControllerStatus.AwaitingIsolation, refused.Status);
        Assert.False(refused.MayDial);
        Assert.Equal(held.FencingGeneration, refused.FencingGeneration);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AriControllerOwnershipEntity row = await context.AriControllerOwnership
            .SingleAsync(candidate => candidate.ControllerScope == Scope);

        Assert.Equal(AriControllerOwnershipEntity.StateIsolated, row.State);
        Assert.Equal("ops-nguyen", row.IsolatedByActorId);
    }

    /// <summary>
    /// The database refuses an isolation that names nobody, so the most dangerous transition here
    /// cannot be performed anonymously even by something that bypasses this class.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCH-CTRL-06")]
    public async Task AnIsolationWithNoActorIsRefusedByTheDatabase()
    {
        await fixture.ResetAsync();
        var clock = new MovableClock(Now);
        await Ownership(clock).AcquireOrRenewAsync("worker-a");

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Exception? failure = await Record.ExceptionAsync(() =>
            context.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE ivr_ari_controller_ownership
                SET state = 'ISOLATED', isolated_at = {Now}
                WHERE controller_scope = {Scope}
                """));

        Assert.NotNull(failure);
    }

    private PostgresAriControllerOwnership Ownership(TimeProvider clock) => new(
        Factory(),
        Options.Create(new SchedulerOptions { Enabled = true }),
        clock,
        Scope);

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private sealed class MovableClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan by) => now = now.Add(by);
    }
}
