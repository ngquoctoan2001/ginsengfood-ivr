using System.Collections.Frozen;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;

namespace Ivr.Domain.DevTooling;

/// <summary>
/// One recorded attempt of a rehearsal scenario, in the vocabulary of
/// <c>seed/call-scenarios.sample.json</c>.
/// </summary>
public sealed record ScenarioAttempt(int AttemptNumber, string RawCallStatus, string? RawDtmf);

/// <summary>
/// A rehearsal scenario as the seed file declares it: what the provider reported on each attempt,
/// and what the result is supposed to be.
/// </summary>
/// <param name="ExpectedCounted">
/// Null when the file does not assert it. Several scenarios pin the result type and deliberately
/// say nothing about the attempt count, and inventing a value here would manufacture agreement.
/// </param>
public sealed record ScenarioDefinition(
    string Id,
    string? TaskRef,
    IReadOnlyList<ScenarioAttempt> Attempts,
    string? ExpectedResultType,
    bool? ExpectedCounted);

public sealed record ScenarioAttemptOutcome(
    int AttemptNumber,
    string RawCallStatus,
    string? RawDtmf,
    string ResultType,
    bool Counted,
    bool Final,
    string Reason);

/// <summary>Whether the disposition mapper is able to answer for this scenario at all.</summary>
public enum ScenarioCoverage
{
    /// <summary>Replayed end to end; <c>Matches</c> is meaningful.</summary>
    Replayed,

    /// <summary>
    /// Out of reach for this engine. Not a failure of the scenario — the expected result is
    /// produced somewhere other than disposition normalisation.
    /// </summary>
    NotReplayable,
}

public sealed record ScenarioDryRunReport(
    string ScenarioId,
    string? TaskRef,
    ScenarioCoverage Coverage,
    string? ExpectedResultType,
    bool? ExpectedCounted,
    string? ActualResultType,
    bool? ActualCounted,
    bool? Matches,
    IReadOnlyList<ScenarioAttemptOutcome> Attempts,
    IReadOnlyList<string> Notes);

/// <summary>
/// Replays a call scenario through <see cref="DispositionMapper"/> and compares the result with
/// what the seed file says it should be (UI-07 scenario runner, W-0112).
/// <para>
/// The important property is structural, not behavioural: this type depends on
/// <see cref="DispositionMapper"/> and nothing else. It holds no gateway, no scheduler and no
/// repository, so a dry run cannot place a call however it is invoked or misconfigured. That is a
/// stronger guarantee than a flag that suppresses dialling, because there is no dialling code on
/// this path to suppress.
/// </para>
/// </summary>
public static class CallScenarioDryRun
{
    /// <summary>
    /// Result types <see cref="DispositionMapper"/> can actually produce.
    /// <para>
    /// Derived from the mapper's own outputs rather than from a list of scenario names, so a
    /// scenario expecting <c>IVR_CONFIRMATION_WINDOW_EXPIRED</c> or <c>IVR_OPERATIONAL_BLOCKED</c>
    /// is reported as out of scope instead of as a mismatch. Those results come from the expiry
    /// sweep and from intake, and calling their absence here a failure would send someone looking
    /// for a bug in the wrong file.
    /// </para>
    /// </summary>
    private static readonly FrozenSet<string> ReplayableResultTypes = new[]
        {
            "IVR_CONFIRMED",
            "IVR_CUSTOMER_CANCELLED",
            "IVR_NO_ANSWER_ATTEMPT",
            "IVR_NO_ANSWER_FINAL",
            "IVR_INVALID_PHONE_FINAL",
            "IVR_WRONG_INPUT",
            "IVR_TECHNICAL_EXCEPTION",
        }
        .ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// The seed vocabulary, spelled out. An unknown status is an error rather than a fallback:
    /// <see cref="DispositionMapper"/> maps a null disposition to a technical exception, so a
    /// typo in the seed file would otherwise replay as a plausible-looking technical result and
    /// the scenario would appear to pass for a reason nobody intended.
    /// </summary>
    private static readonly FrozenDictionary<string, SimProviderDisposition> Dispositions =
        new Dictionary<string, SimProviderDisposition>(StringComparer.OrdinalIgnoreCase)
        {
            ["answered"] = SimProviderDisposition.Answered,
            ["ring_timeout"] = SimProviderDisposition.RingTimeout,
            ["busy"] = SimProviderDisposition.Busy,
            ["rejected"] = SimProviderDisposition.Rejected,
            ["unreachable"] = SimProviderDisposition.Unreachable,
            ["invalid_destination"] = SimProviderDisposition.InvalidDestination,
            ["dropped"] = SimProviderDisposition.Dropped,
            ["network_error"] = SimProviderDisposition.NetworkError,
            ["sim_error"] = SimProviderDisposition.SimError,
            ["audio_error"] = SimProviderDisposition.AudioError,
            ["dtmf_error"] = SimProviderDisposition.DtmfError,
        }
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public const int MaximumAttempts = 10;

    /// <summary>Replays one scenario. Never throws for scenario content; it reports instead.</summary>
    /// <param name="windowStartedAt">Start of the simulated confirmation window.</param>
    /// <param name="window">Simulated window length. Attempts are spread evenly inside it.</param>
    /// <param name="technicalRetryLimit">Mirrors the scheduler's limit for the replay.</param>
    public static ScenarioDryRunReport Execute(
        ScenarioDefinition scenario,
        DateTimeOffset windowStartedAt,
        TimeSpan window,
        int technicalRetryLimit)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);
        if (technicalRetryLimit is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(technicalRetryLimit));
        }

        List<string> notes = [];
        if (scenario.Attempts.Count == 0)
        {
            notes.Add(
                "The scenario declares no call attempt, so there is nothing for the disposition "
                + "mapper to replay. Its expected result is decided at intake or by a sweep.");
            return NotReplayable(scenario, notes);
        }

        if (scenario.Attempts.Count > MaximumAttempts)
        {
            notes.Add(
                $"The scenario declares {scenario.Attempts.Count} attempts; the attempt policy "
                + $"allows at most {MaximumAttempts}.");
            return NotReplayable(scenario, notes);
        }

        if (scenario.ExpectedResultType is null)
        {
            notes.Add(
                "The scenario asserts no expected result type, so the replay below is reported "
                + "without a verdict.");
        }
        else if (!ReplayableResultTypes.Contains(scenario.ExpectedResultType))
        {
            notes.Add(
                $"'{scenario.ExpectedResultType}' is never produced by disposition normalisation "
                + "— it comes from the confirmation-window sweep or from task intake. Replaying "
                + "the attempts cannot confirm or refute it.");
            return NotReplayable(scenario, notes);
        }

        // Attempts are spread across the window so every one of them lands strictly inside it.
        // Placing them all at the same instant, or at the boundary, would silently change what
        // ReachedFinalAttempt answers and make the replay depend on an accident of arithmetic.
        int count = scenario.Attempts.Count;
        DateTimeOffset expiresAt = windowStartedAt + window;
        List<ScenarioAttemptOutcome> outcomes = new(count);
        int priorTechnicalRetries = 0;
        NormalizedResult? last = null;

        for (int index = 0; index < count; index++)
        {
            ScenarioAttempt attempt = scenario.Attempts[index];
            if (!Dispositions.TryGetValue(attempt.RawCallStatus, out SimProviderDisposition disposition))
            {
                notes.Add(
                    $"Attempt {attempt.AttemptNumber} reports raw_call_status "
                    + $"'{attempt.RawCallStatus}', which is not a provider disposition.");
                return NotReplayable(scenario, notes);
            }

            DateTimeOffset occurredAt = windowStartedAt + (window * (index + 1) / (count + 1));
            var context = new AttemptNormalizationContext(
                index + 1,
                count,
                occurredAt,
                expiresAt,
                priorTechnicalRetries,
                technicalRetryLimit);
            NormalizedResult result = DispositionMapper.Normalize(
                disposition,
                attempt.RawDtmf,
                null,
                context);
            priorTechnicalRetries = result.TechnicalRetryCount;
            last = result;
            outcomes.Add(new ScenarioAttemptOutcome(
                attempt.AttemptNumber,
                attempt.RawCallStatus,
                attempt.RawDtmf,
                result.ResultStatus,
                result.IsCounted,
                result.IsFinal,
                result.Reason));
        }

        // The scenario's outcome is the last attempt's, because that is the one that ends the
        // job. Earlier attempts are shown so a rehearsal can see the path, not just the verdict.
        NormalizedResult final = last!;
        bool? matches = null;
        if (scenario.ExpectedResultType is not null)
        {
            bool typeMatches = string.Equals(
                final.ResultStatus,
                scenario.ExpectedResultType,
                StringComparison.Ordinal);
            bool countedMatches = scenario.ExpectedCounted is null
                || scenario.ExpectedCounted.Value == final.IsCounted;
            matches = typeMatches && countedMatches;
            if (!typeMatches)
            {
                notes.Add(
                    $"Expected {scenario.ExpectedResultType} but the attempts normalise to "
                    + $"{final.ResultStatus}.");
            }

            if (!countedMatches)
            {
                notes.Add(
                    $"Expected customer_attempt_counted={scenario.ExpectedCounted} but the "
                    + $"attempts normalise to {final.IsCounted}.");
            }
        }

        return new ScenarioDryRunReport(
            scenario.Id,
            scenario.TaskRef,
            ScenarioCoverage.Replayed,
            scenario.ExpectedResultType,
            scenario.ExpectedCounted,
            final.ResultStatus,
            final.IsCounted,
            matches,
            outcomes,
            notes);
    }

    private static ScenarioDryRunReport NotReplayable(
        ScenarioDefinition scenario,
        List<string> notes) => new(
            scenario.Id,
            scenario.TaskRef,
            ScenarioCoverage.NotReplayable,
            scenario.ExpectedResultType,
            scenario.ExpectedCounted,
            null,
            null,
            null,
            [],
            notes);
}
