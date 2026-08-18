using Ivr.Api.Health;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §8, ARCH-05 §1 (DB row) and DO-06. The question is not whether the process
/// notices a dead database — it is whether it says so out loud, refuses to pretend, and still has
/// the data afterwards.
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class DatabaseFaultScenario(ChaosEnvironment chaos, Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    [Trait("TestId", "CHAOS-DB-02")]
    public async Task ACutDatabaseLinkTurnsReadinessRedLosesNothingAndRecoversWhenTheLinkReturns()
    {
        string marker = $"CHAOS-DB-02-{Guid.NewGuid():N}"[..24];
        await using (IvrDbContext seed = await chaos.DbContextFactory.CreateDbContextAsync())
        {
            seed.SimChannels.Add(new SimChannelEntity
            {
                SimChannelId = marker,
                SimNumberRef = "sim-ref-chaos-db-02",
                Enabled = true,
                Status = "IDLE",
            });
            await seed.SaveChangesAsync();
        }

        var probe = new IvrReadinessProbe(chaos.DbContextFactory);
        ReadinessReport healthy = await probe.CheckAsync(CancellationToken.None);
        Assert.True(healthy.Ready);
        Assert.Equal(200, healthy.StatusCode);

        // ---- fault ----
        await chaos.CutDatabaseLinkAsync();

        ReadinessReport down = await probe.CheckAsync(CancellationToken.None);
        Assert.False(down.Ready);
        Assert.Equal(503, down.StatusCode);
        ReadinessCheck database = Assert.Single(down.Checks, check => check.Name == "database");
        Assert.False(database.Ready);

        // Fail-closed means the write must FAIL, not quietly vanish. A swallowed write is the one
        // outcome worse than an outage: the caller is told the task was accepted and nothing holds
        // it (DO-06).
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await using IvrDbContext blocked = await chaos.DbContextFactory.CreateDbContextAsync();
            blocked.SimChannels.Add(new SimChannelEntity
            {
                SimChannelId = $"{marker}-blocked",
                SimNumberRef = "sim-ref-chaos-db-02-blocked",
                Enabled = true,
                Status = "IDLE",
            });
            await blocked.SaveChangesAsync();
        });

        // ---- recovery ----
        await chaos.RestoreDatabaseLinkAsync();
        TimeSpan? recoveryTime = await EventuallyReadyAsync(probe);
        Assert.True(recoveryTime.HasValue, "Readiness never returned after the link was restored.");

        // Reported rather than asserted against a fixed budget: there is no owner-approved recovery
        // objective yet, and inventing one here would turn a measurement into a fake requirement.
        output.WriteLine(
            $"RECOVERY_TIME_MS={recoveryTime!.Value.TotalMilliseconds:F0}");

        await using IvrDbContext recovered = await chaos.DbContextFactory.CreateDbContextAsync();

        // The row written before the fault is still there: no data loss across the outage.
        Assert.True(await recovered.SimChannels.AnyAsync(c => c.SimChannelId == marker));

        // And the write that failed during the fault left nothing behind. A partial write would be
        // worse than a rejected one, because nothing downstream knows to reconcile it.
        Assert.False(await recovered.SimChannels.AnyAsync(
            c => c.SimChannelId == $"{marker}-blocked"));
    }

    private static async Task<TimeSpan?> EventuallyReadyAsync(IvrReadinessProbe probe)
    {
        // Recovery is asserted with a bounded wait, not a fixed sleep: the pool has to notice the
        // link is back, and pinning that to one duration would make the scenario flaky on a slower
        // machine while proving nothing extra on a fast one.
        long startedTicks = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int attempt = 0; attempt < 30; attempt++)
        {
            ReadinessReport report = await probe.CheckAsync(CancellationToken.None);
            if (report.Ready)
            {
                return System.Diagnostics.Stopwatch.GetElapsedTime(startedTicks);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        return null;
    }
}
