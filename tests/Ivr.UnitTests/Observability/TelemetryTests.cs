using System.Diagnostics;
using System.Diagnostics.Metrics;
using Ivr.Infrastructure.Observability;

namespace Ivr.UnitTests.Observability;

/// <summary>
/// W-0040 / P6-1. Observability is the one subsystem whose whole job is to copy production data
/// somewhere else, so every test here asks the same question in a different place: can a customer
/// detail ride out on a signal.
/// </summary>
public sealed class TelemetryTests
{
    [Fact]
    [Trait("TestId", "UT-OBS-PII-01")]
    public void NoSpanOrMetricTagCanCarryAPhoneNumberAnAddressOrAnUnlistedName()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == IvrTelemetry.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        // A raw MSISDN is refused wherever it is offered.
        InvalidOperationException spanLeak = Assert.Throws<InvalidOperationException>(() =>
            IvrTelemetry.StartSpan(
                "ivr.intake",
                (TelemetryTags.ReasonCode, "called 0912345678")));
        Assert.Contains("PII guard", spanLeak.Message, StringComparison.Ordinal);

        // The message must not echo the offending value: an exception message ends up in a log,
        // and reporting a leak by quoting the leak is still a leak.
        Assert.DoesNotContain("0912345678", spanLeak.Message, StringComparison.Ordinal);

        Assert.Throws<InvalidOperationException>(() =>
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, "Đường Nguyễn Huệ, Phường Bến Nghé")));

        // A tag nobody put on the allowlist is refused even when its value is harmless. A
        // denylist protects against the leaks someone thought of; this protects against the rest.
        InvalidOperationException unlisted = Assert.Throws<InvalidOperationException>(() =>
            IvrTelemetry.RecordResult(("ivr.customer_phone", "84xxxxx0001")));
        Assert.Contains("allowlist", unlisted.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-OBS-PII-01B")]
    public void ARequestIdentifierMayTagASpanButNeverAMetricDimension()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == IvrTelemetry.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        // A span is one request an engineer is investigating: the correlation id is the point.
        using Activity? span = IvrTelemetry.StartSpan(
            "ivr.intake",
            (TelemetryTags.CorrelationId, "corr-obs-01"),
            (TelemetryTags.Program, "GOLDEN_HOUR"));
        Assert.NotNull(span);
        Assert.Equal("corr-obs-01", span!.GetTagItem(TelemetryTags.CorrelationId));

        // The same tag on a metric would make every request its own time series — a cardinality
        // explosion that takes the metrics backend down, not a privacy issue.
        InvalidOperationException cardinality = Assert.Throws<InvalidOperationException>(() =>
            IvrTelemetry.RecordIntakeDecision((TelemetryTags.CorrelationId, "corr-obs-01")));
        Assert.Contains("time series", cardinality.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-OBS-METRIC-03")]
    public void BusinessCountersAndHistogramsEmitWithTheirDimensions()
    {
        var observed = new List<(string Instrument, double Value, string Program)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, target) =>
        {
            if (instrument.Meter.Name == IvrTelemetry.ServiceName)
            {
                target.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            observed.Add((instrument.Name, value, ReadProgram(tags))));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            observed.Add((instrument.Name, value, ReadProgram(tags))));
        listener.Start();

        IvrTelemetry.RecordIntakeDecision(
            (TelemetryTags.Program, "GOLDEN_HOUR"),
            (TelemetryTags.PaymentMethod, "ONLINE"),
            (TelemetryTags.Decision, "TASK_ACCEPTED_DRY_RUN_ONLY"),
            (TelemetryTags.ExecutionMode, "MOCK"));
        IvrTelemetry.RecordAttempt(
            (TelemetryTags.Program, "GOLDEN_HOUR"),
            (TelemetryTags.AttemptPolicyVersion, "mock-lab-v1"),
            (TelemetryTags.Counted, false));
        IvrTelemetry.RecordResult(
            (TelemetryTags.Program, "GOLDEN_HOUR"),
            (TelemetryTags.ResultType, "IVR_CONFIRMED"));
        IvrTelemetry.RecordCallback(
            0.42,
            (TelemetryTags.Program, "GOLDEN_HOUR"),
            (TelemetryTags.CallbackContract, "TARGET_V1"),
            (TelemetryTags.AckCode, "ACCEPTED"),
            (TelemetryTags.HttpStatus, 200));
        IvrTelemetry.RecordFailClosed(
            (TelemetryTags.Program, "GOLDEN_HOUR"),
            (TelemetryTags.ReasonCode, "CAPACITY_SOURCE_UNAVAILABLE"));
        listener.RecordObservableInstruments();

        string[] names = observed.Select(entry => entry.Instrument).Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(
            [
                "ivr_call_attempts_total",
                "ivr_call_results_total",
                "ivr_fail_closed_total",
                "ivr_intake_decisions_total",
                "ivr_result_callback_duration_seconds",
                "ivr_result_callbacks_total",
            ],
            names);

        // The latency histogram carries the real measurement, not a placeholder.
        Assert.Contains(observed, entry =>
            entry.Instrument == "ivr_result_callback_duration_seconds"
            && Math.Abs(entry.Value - 0.42) < 0.0001);

        // Every measurement carries the program dimension it was given: a counter without its
        // dimensions answers "how many" and never "which kind", which is the question ops asks.
        Assert.All(observed, entry => Assert.Equal("GOLDEN_HOUR", entry.Program));
    }

    private static string ReadProgram(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        foreach (KeyValuePair<string, object?> tag in tags)
        {
            if (tag.Key == TelemetryTags.Program)
            {
                return tag.Value?.ToString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
