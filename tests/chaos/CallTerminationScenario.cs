using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0111. An operator's request to cut a call, arriving while the call is finishing anyway.
/// <para>
/// The window is real and cannot be closed by locking: <c>Ivr.Api</c> reads "this call is live",
/// the worker finalizes it a moment later, and the request lands on an attempt that has just
/// ended. The console and the worker are separate processes and the customer's phone does not
/// wait for either of them.
/// </para>
/// <para>
/// What must survive the race is the record. Exactly one raw call event, one result, and no
/// silent overwrite of what actually happened on the line — because the result is what Order
/// Core is told, and a duplicate or a rewritten one is a customer whose order state now
/// disagrees with their call.
/// </para>
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class CallTerminationScenario(ChaosEnvironment chaos)
{
    [Fact]
    [Trait("TestId", "CHAOS-TERMINATE-07")]
    public async Task ARequestLandingAfterTheCallFinishedLeavesTheRecordedResultIntact()
    {
        string suffix = $"{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        string jobId = $"JOB-CHAOS-TERM-{suffix}";
        string channelId = $"SIM-CHAOS-T{suffix}";
        IDbContextFactory<IvrDbContext> factory = chaos.DbContextFactory;
        await ChaosFixtures.SeedReadyJobAsync(
            factory,
            $"TASK-CHAOS-TERM-{suffix}",
            jobId,
            ChaosFixtures.Now);
        await ChaosFixtures.SeedChannelAsync(factory, channelId, priorFailCount: 0);

        var clock = new FixedClock(ChaosFixtures.Now);
        var scheduler = new PostgresSchedulerStore(factory, clock);
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "chaos-terminate",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2)));
        var telephony = new PostgresTelephonyDispatchStore(
            factory,
            SpeechSummaryLimits.Create(8, 8),
            clock);
        SimCallSession session = new(
            AttemptId.Create(lease.AttemptId),
            lease.SimChannelId,
            $"prov-{suffix}",
            lease.FencingGeneration,
            ChaosFixtures.Now,
            IsConnected: true);
        await telephony.MarkActiveAsync(lease, session);

        // The customer answered and pressed 1. The call is over.
        await telephony.CompleteAsync(
            lease,
            session,
            new SimDtmfCapture("1", false, null),
            new SimDispositionReport(
                SimProviderDisposition.Answered,
                ChaosFixtures.Now,
                ChaosFixtures.Now.AddSeconds(20),
                null,
                ChannelHealthy: true),
            cooldown: TimeSpan.FromSeconds(5));

        // The operator's request arrives now, one moment too late. Written directly because this
        // scenario is about the race, not about the API guard that normally prevents it.
        await using (IvrDbContext late = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity row = await late.CallAttempts
                .SingleAsync(item => item.IvrCallAttemptId == lease.AttemptId);
            row.TerminationRequestedAt = ChaosFixtures.Now.AddSeconds(21);
            row.TerminationRequestedBy = "operator-late";
            row.TerminationReason = "cut it";
            await late.SaveChangesAsync();
        }

        // Nothing acts on the request: the loop has already gone. What matters is that the
        // customer's answer is still the recorded one.
        await using IvrDbContext verify = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verify.CallAttempts
            .AsNoTracking()
            .SingleAsync(item => item.IvrCallJobId == jobId);
        List<RawCallEventEntity> rawEvents = await verify.RawCallEvents
            .AsNoTracking()
            .Where(item => item.IvrCallAttemptId == lease.AttemptId)
            .ToListAsync();

        Assert.Single(rawEvents);
        Assert.Equal("1", attempt.DtmfKey);
        Assert.Null(attempt.TechnicalExceptionType);
        Assert.NotNull(attempt.EndedAt);

        // The stale request is visible but inert. Left on the row on purpose: an operator who
        // pressed the button deserves to see that they did, even when it arrived too late.
        Assert.Equal("operator-late", attempt.TerminationRequestedBy);

        // And the channel came back exactly once. A second release would have shown up as a
        // lease that no longer matches its fencing generation.
        SimChannelEntity channel = await verify.SimChannels
            .AsNoTracking()
            .SingleAsync(item => item.SimChannelId == channelId);
        Assert.Null(channel.LeaseToken);
        Assert.Null(channel.ActiveCallJobId);
    }

    /// <summary>
    /// The other order: the request lands first and the loop finalizes as a cut. One raw event,
    /// one technical exception, and the customer's attempt budget untouched.
    /// </summary>
    [Fact]
    [Trait("TestId", "CHAOS-TERMINATE-08")]
    public async Task ACutRecordsOneTechnicalExceptionAndDoesNotSpendACustomerAttempt()
    {
        string suffix = $"{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        string jobId = $"JOB-CHAOS-CUT-{suffix}";
        string channelId = $"SIM-CHAOS-C{suffix}";
        IDbContextFactory<IvrDbContext> factory = chaos.DbContextFactory;
        await ChaosFixtures.SeedReadyJobAsync(
            factory,
            $"TASK-CHAOS-CUT-{suffix}",
            jobId,
            ChaosFixtures.Now);
        await ChaosFixtures.SeedChannelAsync(factory, channelId, priorFailCount: 0);

        var clock = new FixedClock(ChaosFixtures.Now);
        var scheduler = new PostgresSchedulerStore(factory, clock);
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "chaos-cut",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2)));
        var telephony = new PostgresTelephonyDispatchStore(
            factory,
            SpeechSummaryLimits.Create(8, 8),
            clock);
        SimCallSession session = new(
            AttemptId.Create(lease.AttemptId),
            lease.SimChannelId,
            $"prov-{suffix}",
            lease.FencingGeneration,
            ChaosFixtures.Now,
            IsConnected: true);
        await telephony.MarkActiveAsync(lease, session);

        await using (IvrDbContext requested = await factory.CreateDbContextAsync())
        {
            CallAttemptEntity row = await requested.CallAttempts
                .SingleAsync(item => item.IvrCallAttemptId == lease.AttemptId);
            row.TerminationRequestedAt = ChaosFixtures.Now.AddSeconds(3);
            row.TerminationRequestedBy = "operator-cut";
            row.TerminationReason = "wrong script on air";
            await requested.SaveChangesAsync();
        }

        // The loop observes the request and unwinds through its failure path, exactly as
        // CallTerminatedException drives it in the gateways.
        Assert.NotNull(await telephony.ReadTerminationAsync(lease));
        await telephony.FailAsync(
            lease,
            session,
            SimProviderDisposition.Dropped,
            CallTerminatedException.TechnicalCode,
            channelHealthy: true,
            cooldown: TimeSpan.FromSeconds(5));

        await using IvrDbContext verify = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verify.CallAttempts
            .AsNoTracking()
            .SingleAsync(item => item.IvrCallJobId == jobId);
        List<RawCallEventEntity> rawEvents = await verify.RawCallEvents
            .AsNoTracking()
            .Where(item => item.IvrCallAttemptId == lease.AttemptId)
            .ToListAsync();

        Assert.Single(rawEvents);
        Assert.Equal(CallTerminatedException.TechnicalCode, attempt.TechnicalExceptionType);

        // The line this whole work item is about. A cut customer never got to answer, so
        // spending one of their attempts on the operator's decision would charge them for it.
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Null(attempt.DtmfKey);

        // Channel healthy and back in service: the cut was ours, not the equipment's.
        SimChannelEntity channel = await verify.SimChannels
            .AsNoTracking()
            .SingleAsync(item => item.SimChannelId == channelId);
        Assert.NotEqual("HEALTH_FAILED", channel.Status);
        Assert.Null(channel.LeaseToken);
    }

    /// <summary>
    /// A clock that does not move, so a race is exercised by ordering rather than by timing.
    /// <c>SimFaultScenario</c> keeps its own private copy; duplicated here rather than shared to
    /// avoid turning a one-line test helper into a fixture two scenarios have to agree on.
    /// </summary>
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
