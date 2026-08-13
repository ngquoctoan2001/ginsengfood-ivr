using System.Diagnostics.Metrics;

namespace Ivr.Infrastructure.Retention;

/// <summary>
/// Telemetry seam for retention metrics and deterministic interruption tests.
/// </summary>
public interface IRetentionTelemetry
{
    /// <summary>
    /// Records a committed batch. The callback runs only after its transaction commits.
    /// </summary>
    public ValueTask BatchCommittedAsync(
        string dataClass,
        string segment,
        int affectedRows,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records that an owner period is missing and the class stayed fail-closed.
    /// </summary>
    public void RecordNotConfigured(string dataClass, double ageDays);

    /// <summary>
    /// Records a completed class using privacy-safe aggregate counts.
    /// </summary>
    public void RecordClassCompleted(string dataClass, long affectedRows);

    /// <summary>
    /// Records a failed or cancelled run without emitting row identifiers.
    /// </summary>
    public void RecordRunFailed();
}

internal sealed class RetentionTelemetry : IRetentionTelemetry
{
    private static readonly Meter Meter = new("Ivr.Retention", "1.0.0");
    private static readonly Counter<long> CommittedRows = Meter.CreateCounter<long>(
        "ivr.retention.committed.rows");
    private static readonly Counter<long> NotConfigured = Meter.CreateCounter<long>(
        "ivr.retention.not_configured");
    private static readonly Histogram<double> NotConfiguredAge = Meter.CreateHistogram<double>(
        "ivr.retention.not_configured.age.days",
        "days");
    private static readonly Counter<long> CompletedClasses = Meter.CreateCounter<long>(
        "ivr.retention.completed.classes");
    private static readonly Counter<long> FailedRuns = Meter.CreateCounter<long>(
        "ivr.retention.failed.runs");

    public ValueTask BatchCommittedAsync(
        string dataClass,
        string segment,
        int affectedRows,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CommittedRows.Add(
            affectedRows,
            new KeyValuePair<string, object?>("data_class", dataClass),
            new KeyValuePair<string, object?>("segment", segment));
        return ValueTask.CompletedTask;
    }

    public void RecordNotConfigured(string dataClass, double ageDays)
    {
        KeyValuePair<string, object?> tag = new("data_class", dataClass);
        NotConfigured.Add(1, tag);
        NotConfiguredAge.Record(ageDays, tag);
    }

    public void RecordClassCompleted(string dataClass, long affectedRows) => CompletedClasses.Add(
        1,
        new KeyValuePair<string, object?>("data_class", dataClass),
        new KeyValuePair<string, object?>("affected_rows", affectedRows));

    public void RecordRunFailed() => FailedRuns.Add(1);
}
