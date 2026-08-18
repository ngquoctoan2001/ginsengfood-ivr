using System.Diagnostics.Metrics;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §8, DT-04 and P0-IVR-004. A SIM that drops the call is a fault in our equipment,
/// not a customer who declined. Recording it as no-answer would burn one of the customer's two
/// allowed attempts on our own hardware failing, and the customer would never know a call had been
/// counted against them.
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class SimFaultScenario(ChaosEnvironment chaos)
{
    private static readonly SimProviderDisposition[] EquipmentFaults =
    [
        SimProviderDisposition.Dropped,
        SimProviderDisposition.NetworkError,
        SimProviderDisposition.SimError,
        SimProviderDisposition.AudioError,
        SimProviderDisposition.DtmfError,
    ];

    [Fact]
    [Trait("TestId", "CHAOS-SIM-03")]
    public async Task ADroppedCallIsATechnicalExceptionAndTheThirdFailureTakesTheChannelOutOfService()
    {
        // Asserted across every equipment-fault disposition, not just the one this scenario
        // drives: a new provider disposition added to the enum tomorrow must land on the same
        // side of this line, and one example would not catch it.
        var context = new AttemptNormalizationContext(
            AttemptNumber: 1,
            MaxAttempts: 2,
            OccurredAt: ChaosFixtures.Now,
            ConfirmationWindowExpiresAt: ChaosFixtures.Now.AddMinutes(5),
            PriorTechnicalRetryCount: 0,
            TechnicalRetryLimit: 2);
        foreach (SimProviderDisposition fault in EquipmentFaults)
        {
            NormalizedResult mapped = DispositionMapper.Normalize(fault, null, "TECH", context);
            Assert.False(
                mapped.IsNoAnswer,
                $"{fault} was normalized as a no-answer; that charges our equipment failure to "
                + "the customer's attempt budget (P0-IVR-004).");
            Assert.Equal(IvrResultType.IvrTechnicalException, mapped.ResultType);
        }

        string suffix = $"{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        string jobId = $"JOB-CHAOS-SIM-{suffix}";
        string channelId = $"SIM-CHAOS-{suffix}";
        IDbContextFactory<IvrDbContext> factory = chaos.DbContextFactory;

        await ChaosFixtures.SeedReadyJobAsync(
            factory,
            $"TASK-CHAOS-SIM-{suffix}",
            jobId,
            ChaosFixtures.Now);
        // Two failures already on the clock: DT-04 disables at three.
        await ChaosFixtures.SeedChannelAsync(factory, channelId, priorFailCount: 2);

        var clock = new FixedClock(ChaosFixtures.Now);
        var scheduler = new PostgresSchedulerStore(factory, clock);
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "chaos-worker",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2)));

        var quarantines = new List<string>();
        using MeterListener listener = ListenForQuarantines(quarantines);

        var telephony = new PostgresTelephonyDispatchStore(
            factory,
            SpeechSummaryLimits.Create(8, 8),
            clock);
        await telephony.FailAsync(
            lease,
            session: null,
            SimProviderDisposition.Dropped,
            technicalErrorCode: "PROVIDER_DROPPED",
            channelHealthy: false,
            cooldown: TimeSpan.FromSeconds(5));

        await using IvrDbContext verify = await factory.CreateDbContextAsync();
        SimChannelEntity channel = await verify.SimChannels
            .SingleAsync(row => row.SimChannelId == channelId);

        // DT-04: the third failure takes the channel out of service rather than merely cooling it.
        Assert.Equal(3, channel.FailCount);
        Assert.Equal("HEALTH_FAILED", channel.Status);
        Assert.NotNull(channel.QuarantineUntil);

        CallAttemptEntity attempt = await verify.CallAttempts
            .SingleAsync(row => row.IvrCallJobId == jobId);

        // DT-02: a technical exception is not a customer attempt. This is the invariant that keeps
        // our own outage from consuming the customer's two tries.
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("PROVIDER_DROPPED", attempt.TechnicalExceptionType);

        // And the auto-disable reached the counter the DT-04 alert reads. W-0041 wired only the
        // lease-expiry path, so before this scenario the alert labelled DT-04 was watching an
        // event the DT-04 transition never raised.
        Assert.NotEmpty(quarantines);
    }

    private static MeterListener ListenForQuarantines(List<string> observed)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName
                    && instrument.Name == "ivr_channel_quarantines_total")
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, _, _) =>
        {
            lock (observed)
            {
                observed.Add(instrument.Name);
            }
        });
        listener.Start();
        return listener;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
