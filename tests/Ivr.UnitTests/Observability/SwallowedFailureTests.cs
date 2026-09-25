using System.Diagnostics.Metrics;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.UnitTests.Observability;

/// <summary>
/// W-0360 / K-31. Checks that answered "no" when they could not ask used to leave no trace, so an
/// outage of the check read exactly like the thing it checks being absent. They still answer "no";
/// they now say why on <c>ivr_fail_closed_total</c>.
/// </summary>
public sealed class SwallowedFailureTests
{
    [Fact]
    [Trait("TestId", "UT-OBS-FAILCLOSED-11")]
    public async Task AnAuditStoreCheckThatThrowsStillSaysNoAndIsCounted()
    {
        List<string> reasons = [];
        bool healthy;
        using (MeterListener listener = ListenForFailClosed(reasons))
        {
            healthy = await new PostgresRuntimeSafetyHealth(new ThrowingFactory())
                .IsAuditProviderHealthyAsync();
            listener.RecordObservableInstruments();
        }

        Assert.False(healthy);
        Assert.Contains(PostgresRuntimeSafetyHealth.AuditStoreUnreadable, reasons);
    }

    private static MeterListener ListenForFailClosed(List<string> reasons)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName
                    && instrument.Name == "ivr_fail_closed_total")
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == TelemetryTags.ReasonCode)
                {
                    lock (reasons)
                    {
                        reasons.Add(tag.Value?.ToString() ?? string.Empty);
                    }
                }
            }
        });
        listener.Start();
        return listener;
    }

    /// <summary>The database cannot be reached at all, the case the catch exists for.</summary>
    private sealed class ThrowingFactory : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext() =>
            throw new InvalidOperationException("The database is unreachable in this test.");
    }
}
