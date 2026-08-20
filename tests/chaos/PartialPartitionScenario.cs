using System.Data.Common;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §6.1 — the partition that is worse than an outage, because nothing looks broken.
/// <para>
/// A full outage is easy: nothing reaches Sales, the outbox holds the result, and
/// <c>CHAOS-DOWNSTREAM-01</c> already proves IVR neither invents a confirmation nor drops one.
/// A PARTIAL partition is the dangerous shape. The worker can still reach Sales — the callback is
/// delivered and acted on — but the worker cannot reach the DATABASE, so it cannot record that it
/// delivered. Its lease expires while it is still alive and still right, another worker picks the
/// row up, and Sales is told the same thing a second time.
/// </para>
/// <para>
/// The promise an at-least-once outbox can actually keep is NOT "never twice". Cutting the link
/// between doing the work and recording it makes a second delivery unavoidable, and any design
/// claiming otherwise has simply not been partitioned yet. The two promises that can be kept, and
/// that this scenario measures, are:
/// </para>
/// <list type="number">
///   <item>the second delivery is <b>recognisable</b> as the same one — same callback id, same
///     idempotency key, byte-identical payload and hash — so a correct Sales can discard it;</item>
///   <item>the first worker, coming back late with a stale lease, <b>cannot win</b> — it may not
///     overwrite the outcome, and above all may not drag an acknowledged row back into the queue.
///   </item>
/// </list>
/// <para>
/// The partition is a REAL network fault: the database link is cut at the Toxiproxy hop, so the
/// worker's write fails the way a partitioned write fails rather than the way a mocked one does.
/// The Sales side is not modelled at all here — this scenario is about what happens after the
/// delivery has already landed, and inventing a transport would only add a way to be wrong.
/// </para>
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class PartialPartitionScenario(ChaosEnvironment chaos)
{
    private static readonly TimeSpan ShortLease = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan LongLease = TimeSpan.FromMinutes(5);

    [Fact]
    [Trait("TestId", "CHAOS-DUPLICATE-06")]
    public async Task AWorkerPartitionedAfterDeliveringLeavesARecognisableDuplicateAndCannotWinLate()
    {
        string suffix = $"chaos06{Guid.NewGuid():N}"[..14];
        ResultCallbackEntity queued = await ChaosFixtures.SeedReadyCallbackAsync(chaos, suffix);
        ICallbackOutboxRepository repository = chaos.Services
            .GetRequiredService<ICallbackOutboxRepository>();

        // Worker A claims the row on a short lease and (in the world outside this process) delivers
        // it. Sales has the callback from this moment on; everything after is about recording it.
        CallbackOutboxMessage first = Assert.Single(
            await repository.DequeueReadyAsync(8, ShortLease),
            message => message.CallbackId == queued.CallbackId);

        try
        {
            await chaos.CutDatabaseLinkAsync();

            // The write that is lost. It has to FAIL, not return false: a false would mean the
            // database refused the update, which is a different world from one where the update
            // never arrived, and only the second is a partition.
            var accepted = new CallbackDeliveryUpdate(
                "DELIVERED_ACCEPTED", 200, "ACCEPTED", null, first.RetryCount, null, true, false);
            bool writeLost = false;
            try
            {
                await repository.CompleteDeliveryAsync(first.CallbackId, first.LeaseToken, accepted);
            }
            catch (Exception exception)
                when (exception is DbException or InvalidOperationException or TimeoutException)
            {
                writeLost = true;
            }

            Assert.True(
                writeLost,
                "The completion did not fail while the database link was cut, so this run never "
                + "partitioned anything and every assertion below would be measuring a healthy "
                + "system.");
        }
        finally
        {
            await chaos.RestoreDatabaseLinkAsync();
        }

        // Worker B picks the row up because the lease lapsed. This IS the duplicate: Sales is about
        // to be told the same thing twice, and no amount of care upstream could have prevented it.
        CallbackOutboxMessage second = await EventuallyReclaimedAsync(repository, queued.CallbackId);

        Assert.NotEqual(first.LeaseToken, second.LeaseToken);

        // Promise 1: the duplicate is recognisable. Every field Sales could dedupe on is identical,
        // and the payload is compared as BYTES rather than as parsed JSON -- the transport checks
        // the delivered body against PayloadSha256, so a re-serialisation that merely means the
        // same thing would not survive, and a Sales deduping on a hash would see two distinct
        // callbacks carrying one decision.
        Assert.Equal(first.CallbackId, second.CallbackId);
        Assert.Equal(first.IdempotencyKey, second.IdempotencyKey);
        Assert.Equal(first.PayloadJson, second.PayloadJson);
        Assert.Equal(first.PayloadSha256, second.PayloadSha256);

        // And no second row was minted. A duplicate DELIVERY is survivable; a duplicate outbox ROW
        // would mean two independent callbacks for one decision, which nothing downstream could
        // collapse because they would not share an id.
        await using (IvrDbContext duringPartition = await chaos.DbContextFactory.CreateDbContextAsync())
        {
            Assert.Equal(
                1,
                await duringPartition.ResultCallbacks.CountAsync(
                    row => row.IvrCallResultId == queued.IvrCallResultId));
        }

        // Promise 2, asserted HERE and not later, and the placement is the whole point. Worker A
        // comes back while B still holds the row in SENDING -- which is the only moment the LEASE
        // is the thing refusing it. Asserted after B finishes instead, this passes with the lease
        // check deleted, because CompleteDeliveryAsync also requires SENDING and an acknowledged
        // row is no longer SENDING. Two guards, one behaviour: the first draft of this scenario
        // asserted the late write down there, survived having the fence removed, and was therefore
        // not the check its own message claimed to be.
        Assert.False(
            await repository.CompleteDeliveryAsync(
                first.CallbackId,
                first.LeaseToken,
                new CallbackDeliveryUpdate(
                    "DELIVERED_ACCEPTED", 200, "ACCEPTED", null, first.RetryCount, null, true, false)),
            "A worker whose lease had expired wrote the outcome of a row another worker was still "
            + "holding. The lease is not fencing anything, and two workers can now disagree about "
            + "one callback with the last write winning.");

        // B still owns it, so B can still finish. Without this the assertion above would also pass
        // on a repository that refuses EVERY completion.
        Assert.True(
            await repository.CompleteDeliveryAsync(
                second.CallbackId,
                second.LeaseToken,
                new CallbackDeliveryUpdate(
                    "DELIVERED_ACCEPTED", 200, "ACCEPTED", null, second.RetryCount, null, true, false)),
            "The worker holding the live lease could not record its own delivery.");

        // A different guard, named as such: once the row is acknowledged it is no longer SENDING,
        // and the status is what refuses a late writer from here on. Worth asserting in the
        // dangerous direction -- overwriting one acceptance with another is untidy, but pushing an
        // acknowledged row back to RETRY_PENDING would schedule a THIRD delivery of an order Sales
        // has already confirmed.
        Assert.False(
            await repository.CompleteDeliveryAsync(
                first.CallbackId,
                first.LeaseToken,
                new CallbackDeliveryUpdate(
                    "RETRY_PENDING",
                    null,
                    null,
                    "late worker",
                    first.RetryCount + 1,
                    DateTimeOffset.UtcNow.AddSeconds(30),
                    false,
                    false)),
            "An acknowledged callback was dragged back into the queue.");

        await using IvrDbContext settled = await chaos.DbContextFactory.CreateDbContextAsync();
        ResultCallbackEntity stored = await settled.ResultCallbacks
            .SingleAsync(row => row.CallbackId == queued.CallbackId);
        Assert.Equal("DELIVERED_ACCEPTED", stored.DeliveryStatus);
        Assert.NotNull(stored.AcknowledgedAt);
        Assert.Null(stored.NextRetryAt);
        Assert.Null(stored.LeaseToken);

        // Nothing is owed any more. An acknowledged row that stayed dequeueable would deliver again
        // on the next poll, which is the same duplicate as before but with no partition to blame.
        Assert.DoesNotContain(
            await repository.DequeueReadyAsync(8, LongLease),
            message => message.CallbackId == queued.CallbackId);
    }

    /// <summary>
    /// Waits for the lapsed lease to become claimable again. A bounded poll rather than a sleep:
    /// the lease expiry and the link restore are two clocks, and pinning this to one duration
    /// would be flaky on a slow machine while proving nothing on a fast one.
    /// </summary>
    private static async Task<CallbackOutboxMessage> EventuallyReclaimedAsync(
        ICallbackOutboxRepository repository,
        string callbackId)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            IReadOnlyList<CallbackOutboxMessage> ready =
                await repository.DequeueReadyAsync(8, LongLease);
            CallbackOutboxMessage? claimed = ready
                .FirstOrDefault(message => message.CallbackId == callbackId);
            if (claimed is not null)
            {
                return claimed;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        Assert.Fail(
            $"{callbackId} was never re-claimed after its lease lapsed. A row stuck in SENDING "
            + "behind a dead lease is not a duplicate risk, it is a lost callback: Sales was told "
            + "once and IVR believes it was never told at all.");
        throw new InvalidOperationException("unreachable");
    }
}
