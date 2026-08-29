using System.Diagnostics;
using System.Diagnostics.Metrics;
using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Observability;

/// <summary>
/// Tag names IVR is allowed to attach to a span or a metric (W-0040 / P6-1 §4).
/// <para>
/// An allowlist rather than a denylist. A denylist protects against the leaks someone thought of;
/// this shape means a new tag has to be added here deliberately, and the person adding it has to
/// look at the rule while doing so.
/// </para>
/// </summary>
public static class TelemetryTags
{
    public const string CorrelationId = "ivr.correlation_id";
    public const string TaskId = "ivr.task_id";
    public const string JobId = "ivr.job_id";
    public const string AttemptId = "ivr.attempt_id";
    public const string CallbackId = "ivr.callback_id";
    public const string TraceContextMissing = "ivr.trace_context_missing";
    public const string Program = "ivr.program";
    public const string PaymentMethod = "ivr.payment_method";
    public const string Decision = "ivr.decision";
    public const string ResultType = "ivr.result_type";
    public const string ExecutionMode = "ivr.execution_mode";
    public const string SalesProvider = "ivr.sales_provider";
    public const string SimProvider = "ivr.sim_provider";
    public const string AttemptPolicyVersion = "ivr.attempt_policy_version";
    public const string AttemptNumber = "ivr.attempt_number";
    public const string CallbackContract = "ivr.callback_contract";
    public const string AckCode = "ivr.ack_code";
    public const string HttpStatus = "ivr.http_status";
    public const string ReasonCode = "ivr.reason_code";
    public const string Counted = "ivr.is_counted_customer_attempt";
    public const string Outcome = "ivr.outcome";

    public static IReadOnlySet<string> Allowed { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        CorrelationId,
        TaskId,
        JobId,
        AttemptId,
        CallbackId,
        TraceContextMissing,
        Program,
        PaymentMethod,
        Decision,
        ResultType,
        ExecutionMode,
        SalesProvider,
        SimProvider,
        AttemptPolicyVersion,
        AttemptNumber,
        CallbackContract,
        AckCode,
        HttpStatus,
        ReasonCode,
        Counted,
        Outcome,
    };

    /// <summary>
    /// Tags that identify one customer or one order. They are safe in a TRACE, where a span is a
    /// single request an engineer is investigating, and unsafe as a METRIC dimension, where every
    /// distinct value becomes its own time series. Kept apart so the difference is enforced
    /// rather than remembered.
    /// </summary>
    public static IReadOnlySet<string> TraceOnly { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        CorrelationId,
        TaskId,
        JobId,
        AttemptId,
        CallbackId,
        TraceContextMissing,
    };
}

/// <summary>
/// A privacy-checked W3C propagation snapshot that can cross the database/worker boundary.
/// Invalid or unsafe input is treated as missing observability context and never blocks work.
/// </summary>
public sealed record TraceContextSnapshot(string TraceParent, string? TraceState)
{
    public const int TraceParentMaxLength = 256;
    public const int TraceStateMaxLength = 512;

    public static TraceContextSnapshot? Capture(Activity? activity)
    {
        if (activity?.Id is not { Length: > 0 } traceParent
            || traceParent.Length > TraceParentMaxLength
            || !PiiGuard.IsSafeText(traceParent)
            || !ActivityContext.TryParse(
                traceParent,
                activity.TraceStateString,
                isRemote: false,
                out _))
        {
            return null;
        }

        string? traceState = activity.TraceStateString;
        if (traceState is { Length: > 0 }
            && (traceState.Length > TraceStateMaxLength || !PiiGuard.IsSafeText(traceState)))
        {
            traceState = null;
        }

        return new TraceContextSnapshot(traceParent, traceState);
    }

    public static TraceContextSnapshot? FromPersisted(string? traceParent, string? traceState)
    {
        if (string.IsNullOrWhiteSpace(traceParent)
            || traceParent.Length > TraceParentMaxLength
            || !PiiGuard.IsSafeText(traceParent))
        {
            return null;
        }

        if (traceState is { Length: > 0 }
            && (traceState.Length > TraceStateMaxLength || !PiiGuard.IsSafeText(traceState)))
        {
            return null;
        }

        return ActivityContext.TryParse(traceParent, traceState, isRemote: true, out _)
            ? new TraceContextSnapshot(traceParent, traceState)
            : null;
    }

    public bool TryParse(out ActivityContext context) => ActivityContext.TryParse(
        TraceParent,
        TraceState,
        isRemote: true,
        out context);
}

/// <summary>
/// The one place IVR emits spans and business metrics (W-0040 / P6-1).
/// <para>
/// Every tag passes <see cref="PiiGuard"/> before it is attached. A phone number or a full
/// address reaching a log line is the same leak whether it arrives through an audit row or a
/// metric label, so the check lives here rather than in each call site's good intentions.
/// </para>
/// <para>W-0139 attaches this BCL source and meter to a vendor-neutral OTLP pipeline.</para>
/// </summary>
public static class IvrTelemetry
{
    public const string ServiceName = "ivr-order-confirmation";
    public const string Version = "1.0.0";

    public static ActivitySource Source { get; } = new(ServiceName, Version);

    private static readonly Meter Meter = new(ServiceName, Version);

    private static readonly Counter<long> IntakeDecisions = Meter.CreateCounter<long>(
        "ivr_intake_decisions_total",
        description: "Task intake decisions by program, payment, mode and provider.");

    /// <summary>
    /// Tasks Module 3 sent that carry the retired <c>OD-15</c> skip shape — Sales said it
    /// evaluated risk, found none, and did not veto. Under <c>OD-18</c> these are called like any
    /// other task, so this counter is the size of the behaviour change the cutover actually made
    /// on live traffic (<c>W-0124</c> F1).
    /// <para>
    /// It exists because the alternative was an argument. <c>W-0123</c> reasoned from
    /// <c>W-0118</c> that no environment sends <c>trust.risk_evidence_available</c>, so nobody was
    /// being skipped and removing the branch changed nothing — a sound inference that no target
    /// database was available to confirm. A counter that stays at zero proves the inference; a
    /// counter that moves names the orders that used to be skipped and now get a call, before an
    /// operator has to discover them one complaint at a time.
    /// </para>
    /// <para>
    /// It is deliberately recorded at intake, never in the eligibility path. Reading trust
    /// metadata to decide anything is precisely what <c>OD-18</c> forbids; reading it to count
    /// what M3 sent is an audit of the producer, and <c>UT-M3-AUTHORITY-11</c> keeps the two
    /// apart by file.
    /// </para>
    /// </summary>
    private static readonly Counter<long> LegacySkipCandidates = Meter.CreateCounter<long>(
        "ivr_legacy_skip_candidate_total",
        description:
            "Tasks whose LEGACY_READ trust metadata would have been skipped before OD-18, "
            + "tagged with the intake decision they actually got.");

    private static readonly Counter<long> Attempts = Meter.CreateCounter<long>(
        "ivr_call_attempts_total",
        description: "Call attempts by policy version and whether the attempt counted (DT-02).");

    private static readonly Counter<long> Results = Meter.CreateCounter<long>(
        "ivr_call_results_total",
        description: "Normalized call results by result type.");

    private static readonly Counter<long> Callbacks = Meter.CreateCounter<long>(
        "ivr_result_callbacks_total",
        description: "Callback deliveries by contract, HTTP status and semantic ACK.");

    private static readonly Counter<long> FailClosed = Meter.CreateCounter<long>(
        "ivr_fail_closed_total",
        description: "Fail-closed holds and blocks by reason code.");

    private static readonly Counter<long> ChannelQuarantines = Meter.CreateCounter<long>(
        "ivr_channel_quarantines_total",
        description: "Channel auto-disable events by reason (DT-04).");

    private static readonly Counter<long> MissedDeadlines = Meter.CreateCounter<long>(
        "ivr_missed_deadline_total",
        description: "Confirmation windows that closed with no call placed (ARCH-06 section 1).");

    private static readonly Histogram<double> CallbackLatency = Meter.CreateHistogram<double>(
        "ivr_result_callback_duration_seconds",
        unit: "s",
        description: "Wall time of a callback delivery attempt.");

    private static readonly Histogram<double> IntakeLatency = Meter.CreateHistogram<double>(
        "ivr_task_intake_duration_seconds",
        unit: "s",
        description: "Wall time from task receipt to intake decision.");

    /// <summary>
    /// Starts a span. Returns null when nothing is listening, which is the normal case in tests
    /// and in a deployment with no collector — callers must tolerate it rather than assume a span.
    /// </summary>
    public static Activity? StartSpan(string name, params (string Key, object? Value)[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        Activity? activity = Source.StartActivity(name, ActivityKind.Internal);
        if (activity is null)
        {
            return null;
        }

        foreach ((string key, object? value) in tags)
        {
            activity.SetTag(key, RequireSafeTag(key, value, allowTraceOnly: true));
        }

        return activity;
    }

    /// <summary>
    /// Starts an asynchronous workflow stage from the task context persisted at intake. When the
    /// context is missing or invalid, work continues on a local span carrying an explicit marker.
    /// An ambient request span is linked, rather than made parent, so all five business stages
    /// retain the task TraceId.
    /// </summary>
    public static Activity? StartWorkflowSpan(
        string name,
        ActivityKind kind,
        TraceContextSnapshot? parent,
        bool linkCurrent,
        params (string Key, object? Value)[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        ActivityContext ambient = Activity.Current?.Context ?? default;
        bool parsed = parent is not null && parent.TryParse(out ActivityContext parentContext);
        ActivityLink[]? links = parsed
            && linkCurrent
            && ambient != default
            && ambient.TraceId != parentContext.TraceId
                ? [new ActivityLink(ambient)]
                : null;
        Activity? activity = parsed
            ? Source.StartActivity(name, kind, parentContext, tags: null, links)
            : Source.StartActivity(name, kind);
        if (activity is null)
        {
            return null;
        }

        activity.SetTag(TelemetryTags.TraceContextMissing, !parsed);
        foreach ((string key, object? value) in tags)
        {
            activity.SetTag(key, RequireSafeTag(key, value, allowTraceOnly: true));
        }

        return activity;
    }

    public static void RecordIntakeDecision(params (string Key, object? Value)[] tags) =>
        IntakeDecisions.Add(1, ToMetricTags(tags));

    /// <summary>
    /// One task that the retired OD-15 predicate would have skipped. See
    /// <see cref="LegacySkipCandidates"/> for why this is counted rather than argued.
    /// </summary>
    public static void RecordLegacySkipCandidate(params (string Key, object? Value)[] tags) =>
        LegacySkipCandidates.Add(1, ToMetricTags(tags));

    public static void RecordAttempt(params (string Key, object? Value)[] tags) =>
        Attempts.Add(1, ToMetricTags(tags));

    public static void RecordResult(params (string Key, object? Value)[] tags) =>
        Results.Add(1, ToMetricTags(tags));

    public static void RecordCallback(double seconds, params (string Key, object? Value)[] tags)
    {
        KeyValuePair<string, object?>[] metricTags = ToMetricTags(tags);
        Callbacks.Add(1, metricTags);
        CallbackLatency.Record(seconds, metricTags);
    }

    public static void RecordIntakeLatency(double seconds, params (string Key, object? Value)[] tags) =>
        IntakeLatency.Record(seconds, ToMetricTags(tags));

    public static void RecordFailClosed(params (string Key, object? Value)[] tags) =>
        FailClosed.Add(1, ToMetricTags(tags));

    public static void RecordChannelQuarantine(params (string Key, object? Value)[] tags) =>
        ChannelQuarantines.Add(1, ToMetricTags(tags));

    /// <summary>
    /// One confirmation window that closed with no call placed. Counted once per closed job, not
    /// once per sweep: a sweep that finds nothing is the normal case, and a counter that also moved
    /// on the empty sweeps would make an idle system look like a failing one.
    /// <para>
    /// This is the OBSERVED miss, not the predicted one. <c>SchedulerCapacityPlan.MissedDeadlineCount</c>
    /// is a forecast recomputed on every eligibility evaluation, so the same pending job appears in
    /// many forecasts; adding that to a counter would count one order dozens of times. The forecast
    /// answers "will this fit", this answers "did it".
    /// </para>
    /// </summary>
    public static void RecordMissedDeadline(params (string Key, object? Value)[] tags) =>
        MissedDeadlines.Add(1, ToMetricTags(tags));

    /// <summary>
    /// Which instrument each recorder feeds (W-0041 / P6-2 section 11). A dashboard panel or an
    /// alert rule may only name a metric that some production call site actually records -- an
    /// instrument that exists but is never called reads as a healthy flat line rather than as the
    /// silence it is. The check walks from call site to instrument, and this map is the hop in
    /// between. It lives beside the instruments so the two move together, and
    /// <c>UT-DASH-PII-04</c> asserts every public recorder appears here.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlySet<string>> InstrumentsByRecorder { get; } =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [nameof(RecordIntakeDecision)] = Names("ivr_intake_decisions_total"),
            [nameof(RecordLegacySkipCandidate)] = Names("ivr_legacy_skip_candidate_total"),
            [nameof(RecordAttempt)] = Names("ivr_call_attempts_total"),
            [nameof(RecordResult)] = Names("ivr_call_results_total"),
            [nameof(RecordCallback)] = Names(
                "ivr_result_callbacks_total",
                "ivr_result_callback_duration_seconds"),
            [nameof(RecordIntakeLatency)] = Names("ivr_task_intake_duration_seconds"),
            [nameof(RecordFailClosed)] = Names("ivr_fail_closed_total"),
            [nameof(RecordChannelQuarantine)] = Names("ivr_channel_quarantines_total"),
            [nameof(RecordMissedDeadline)] = Names("ivr_missed_deadline_total"),
        };

    private static HashSet<string> Names(params string[] names) =>
        new HashSet<string>(names, StringComparer.Ordinal);

    private static KeyValuePair<string, object?>[] ToMetricTags(
        (string Key, object? Value)[] tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        var converted = new KeyValuePair<string, object?>[tags.Length];
        for (int index = 0; index < tags.Length; index++)
        {
            (string key, object? value) = tags[index];
            converted[index] = new KeyValuePair<string, object?>(
                key,
                RequireSafeTag(key, value, allowTraceOnly: false));
        }

        return converted;
    }

    private static object? RequireSafeTag(string key, object? value, bool allowTraceOnly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!TelemetryTags.Allowed.Contains(key))
        {
            throw new InvalidOperationException(
                $"Telemetry tag '{key}' is not on the allowlist in TelemetryTags. Add it there "
                + "deliberately, after checking it carries no customer data (D-05).");
        }

        if (!allowTraceOnly && TelemetryTags.TraceOnly.Contains(key))
        {
            throw new InvalidOperationException(
                $"Telemetry tag '{key}' identifies a single request and must not become a metric "
                + "dimension: every distinct value would be its own time series.");
        }

        if (value is string text && !PiiGuard.IsSafeText(text))
        {
            // Deliberately does not echo the value: an exception message ends up in a log, and a
            // leak reported by quoting the leak is still a leak.
            throw new InvalidOperationException(
                $"Telemetry tag '{key}' carries a value the PII guard rejected (D-05).");
        }

        return value;
    }
}
