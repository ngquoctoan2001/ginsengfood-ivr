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

    /// <summary>
    /// W-0360 / K-31. The two release gates that read the approval store: production dialling and
    /// runtime-gate administration. The literal is pinned on purpose: it is a metric label, and a
    /// dashboard or an alert keys on it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-OBS-FAILCLOSED-12")]
    public async Task BothReleaseGatesThatCannotAskStillSayNoAndAreCounted()
    {
        const string unreadable = "RUNTIME_GATE_APPROVAL_UNREADABLE";
        List<string> reasons = [];
        bool blank;
        bool production;
        bool administration;
        int afterBlank;
        using (MeterListener listener = ListenForFailClosed(reasons))
        {
            // A blank environment is refused before any question is asked: a refusal, not an
            // outage, so it is not counted.
            blank = await new PostgresProductionCallGate(new ThrowingFactory(), TimeProvider.System)
                .IsApprovedAsync(" ");
            lock (reasons)
            {
                afterBlank = reasons.Count(reason => reason == unreadable);
            }

            production = await new PostgresProductionCallGate(new ThrowingFactory(), TimeProvider.System)
                .IsApprovedAsync(FeatureFlagEnvironments.Production);
            administration = await new PostgresRuntimeGateAuthorization(new ThrowingFactory(), TimeProvider.System)
                .IsApprovedAsync(FeatureFlagEnvironments.Production);
            listener.RecordObservableInstruments();
        }

        Assert.False(blank);
        Assert.Equal(0, afterBlank);
        Assert.False(production);
        Assert.False(administration);
        Assert.Equal(2, reasons.Count(reason => reason == unreadable));
    }

    [Fact]
    [Trait("TestId", "UT-OBS-FAILCLOSED-13")]
    public async Task AFourEyesLookupThatCannotAskFindsNoApproverAndIsCounted()
    {
        FeatureFlagSnapshot before = FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot after = before with { GlobalDialKillSwitch = false };
        List<string> reasons = [];
        string? approver;
        using (MeterListener listener = ListenForFailClosed(reasons))
        {
            approver = await new PostgresFourEyesApprovalVerifier(new ThrowingFactory(), TimeProvider.System)
                .VerifyAsync("APPROVAL-REF-1", "operator-a", before, after);
            listener.RecordObservableInstruments();
        }

        Assert.Null(approver);
        Assert.Contains("FOUR_EYES_APPROVAL_UNREADABLE", reasons);
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
