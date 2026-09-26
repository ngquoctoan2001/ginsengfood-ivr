using System.Diagnostics.Metrics;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Logging;

namespace Ivr.UnitTests.Telephony;

// W-0359. Test doubles shared by the two dispatch gateways' unit tests (AsteriskLabTelephonyTests
// and MockTelephonyTests): what a gateway recorded, what it logged, and what it counted.

/// <summary>
/// One <c>FailAsync</c> call, exactly as the gateway made it. <paramref name="ChannelHealthy"/> is null
/// when the gateway said the failure tells nothing about the SIM, and <paramref name="PlaybackStarted"/>
/// is whether the speech had started playing (W-0367 / K-57).
/// </summary>
internal sealed record RecordedDispatchFailure(
    SimCallSession? Session,
    SimProviderDisposition Disposition,
    string TechnicalErrorCode,
    bool? ChannelHealthy,
    TimeSpan Cooldown,
    bool PlaybackStarted);

/// <summary>
/// W-0359 / K-31. What one dispatch left behind: the failures it recorded, the warnings it wrote,
/// the fail-closed measurements it made, and how often it reached the SIM's hangup.
/// </summary>
internal sealed record HangupScenarioRun(
    IReadOnlyList<RecordedDispatchFailure> Failures,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<long> FailClosed,
    int HangupCalls,
    int ActivatedCalls);

/// <summary>
/// A dispatch store that hands the gateway one fixed context and records every failure the
/// gateway reports, so a test asserts the exact arguments rather than their effect on a database.
/// Nothing completes: every scenario that uses it ends in a failure. Given
/// <paramref name="loadFailure"/>, loading the context fails with it instead (W-0362 / K-43).
/// </summary>
internal sealed class RecordingDispatchStore(
    TelephonyDispatchContext context,
    Exception? loadFailure = null) : ITelephonyDispatchStore
{
    private readonly List<RecordedDispatchFailure> failures = [];

    public IReadOnlyList<RecordedDispatchFailure> Failures => failures;

    /// <summary>How many calls were marked active, i.e. dialled and connected.</summary>
    public int ActivatedCalls { get; private set; }

    public Task<CallTerminationRequest?> ReadTerminationAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<CallTerminationRequest?>(null);

    public Task<TelephonyDispatchContext> LoadAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default) =>
        loadFailure is null
            ? Task.FromResult(context)
            : Task.FromException<TelephonyDispatchContext>(loadFailure);

    public Task MarkActiveAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        DispatchedVoice? voice = null,
        CancellationToken cancellationToken = default)
    {
        ActivatedCalls++;
        return Task.CompletedTask;
    }

    public Task CompleteAsync(
        SchedulerDispatchLease lease,
        SimCallSession session,
        SimDtmfCapture dtmf,
        SimDispositionReport disposition,
        TimeSpan cooldown,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("These scenarios end in a failure; nothing completes.");

    public Task FailAsync(
        SchedulerDispatchLease lease,
        SimCallSession? session,
        SimProviderDisposition disposition,
        string technicalErrorCode,
        bool? channelHealthy,
        TimeSpan cooldown,
        bool playbackStarted = false,
        CancellationToken cancellationToken = default)
    {
        failures.Add(new RecordedDispatchFailure(
            session,
            disposition,
            technicalErrorCode,
            channelHealthy,
            cooldown,
            playbackStarted));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Speech synthesis that hands the rendered script straight back. These scenarios are about what a
/// gateway does once it has audio, and a real synthesis path would only add places for them to
/// fail other than the one each is about.
/// </summary>
internal sealed class PassThroughSpeechSynthesisService : ISpeechSynthesisService
{
    public Task<RenderedSpeech> SynthesizeAsync(
        RenderedSpeech renderedSpeech,
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        DateTimeOffset confirmationWindowExpiresAt,
        CancellationToken cancellationToken) =>
        Task.FromResult(renderedSpeech);
}

/// <summary>
/// W-0359 / K-31. Keeps every warning-or-worse line, and the exception attached to it when one
/// was, so "the log never carries the exception's message" is checked against both places a
/// message could ride out on: the formatted text, and an exception handed to the logger whole.
/// Same shape as the one in <c>FeatureFlagPlatformTests</c> (W-0193).
/// </summary>
internal sealed class WarningCapturingLogger<T> : ILogger<T>
{
    private readonly List<string> warnings = [];

    public IReadOnlyList<string> Warnings
    {
        get
        {
            lock (warnings)
            {
                return [.. warnings];
            }
        }
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (logLevel < LogLevel.Warning)
        {
            return;
        }

        string line = exception is null
            ? formatter(state, exception)
            : string.Concat(formatter(state, exception), " | ", exception.ToString());
        lock (warnings)
        {
            warnings.Add(line);
        }
    }
}

/// <summary>
/// W-0359 / K-31. Listens to <c>ivr_fail_closed_total</c> and keeps the value of every measurement
/// tagged with one reason code.
/// <para>
/// Filtered by reason code rather than by thread. The meter is process-wide and xUnit runs test
/// classes in parallel, and a gateway records from whichever thread its awaits resumed on, so the
/// thread filter <c>TelemetryTests</c> uses would miss the measurement. Each hangup code is recorded
/// by one gateway only, and only when a hangup throws, which nothing else in this assembly arranges;
/// if something else ever does, this is the listener it will disturb.
/// </para>
/// </summary>
internal static class FailClosedMeasurements
{
    public const string InstrumentName = "ivr_fail_closed_total";

    public static MeterListener Listen(string reasonCode, List<long> observed)
    {
        ArgumentNullException.ThrowIfNull(observed);
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName
                    && instrument.Name == InstrumentName)
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == TelemetryTags.ReasonCode
                    && string.Equals(tag.Value as string, reasonCode, StringComparison.Ordinal))
                {
                    lock (observed)
                    {
                        observed.Add(value);
                    }
                }
            }
        });
        listener.Start();
        return listener;
    }
}
