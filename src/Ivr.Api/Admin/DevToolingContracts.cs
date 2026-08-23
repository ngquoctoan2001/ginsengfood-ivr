using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <param name="RebaseWindows">
/// Defaults to true, because the fixtures carry absolute August-2026 instants and load as nine
/// rejections without it. False loads them exactly as written, which is how the refusal itself
/// can be demonstrated.
/// </param>
public sealed record SeedLoadRequest(
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("evidence_ref")] string? EvidenceRef = null,
    [property: JsonPropertyName("rebase_windows")] bool RebaseWindows = true);

/// <summary>Outcome of feeding one seed fixture through the real intake path.</summary>
public sealed record SeedTaskOutcomeView(
    [property: JsonPropertyName("scenario")] string Scenario,
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("ivr_call_job_id")] string? IvrCallJobId,
    [property: JsonPropertyName("blocked_reasons")] IReadOnlyList<string> BlockedReasons);

public sealed record SeedLoadApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("dataset")] string Dataset,
    [property: JsonPropertyName("execution_mode")] string ExecutionMode,
    [property: JsonPropertyName("task_count")] int TaskCount,
    [property: JsonPropertyName("accepted_count")] int AcceptedCount,
    [property: JsonPropertyName("windows_rebased")] bool WindowsRebased,
    [property: JsonPropertyName("rebased_count")] int RebasedCount,
    [property: JsonPropertyName("attempt_policies_registered")] int AttemptPoliciesRegistered,
    [property: JsonPropertyName("tasks")] IReadOnlyList<SeedTaskOutcomeView> Tasks,
    [property: JsonPropertyName("correlation_id")] string CorrelationId);

public sealed record ScenarioAttemptView(
    [property: JsonPropertyName("attempt_number")] int AttemptNumber,
    [property: JsonPropertyName("raw_call_status")] string RawCallStatus,
    [property: JsonPropertyName("raw_dtmf")] string? RawDtmf,
    [property: JsonPropertyName("result_type")] string ResultType,
    [property: JsonPropertyName("customer_attempt_counted")] bool CustomerAttemptCounted,
    [property: JsonPropertyName("final")] bool Final,
    [property: JsonPropertyName("reason")] string Reason);

/// <param name="Coverage">
/// <c>REPLAYED</c> or <c>NOT_REPLAYABLE</c>. A scenario whose expected result comes from task
/// intake or from the confirmation-window sweep is reported as out of scope rather than as a
/// mismatch, because a red verdict there would point at the wrong file.
/// </param>
/// <param name="Matches">Null when <paramref name="Coverage"/> is <c>NOT_REPLAYABLE</c>.</param>
public sealed record ScenarioDryRunApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("scenario_id")] string ScenarioId,
    [property: JsonPropertyName("task_ref")] string? TaskRef,
    [property: JsonPropertyName("coverage")] string Coverage,
    [property: JsonPropertyName("expected_result_type")] string? ExpectedResultType,
    [property: JsonPropertyName("expected_counted")] bool? ExpectedCounted,
    [property: JsonPropertyName("actual_result_type")] string? ActualResultType,
    [property: JsonPropertyName("actual_counted")] bool? ActualCounted,
    [property: JsonPropertyName("matches")] bool? Matches,
    [property: JsonPropertyName("attempts")] IReadOnlyList<ScenarioAttemptView> Attempts,
    [property: JsonPropertyName("notes")] IReadOnlyList<string> Notes,
    [property: JsonPropertyName("correlation_id")] string CorrelationId);

/// <param name="Enforced">
/// False when IVR declares the dependency state but nothing in the running system consults it.
/// Four of the five dependencies are in that position today because IVR never probes them — see
/// <c>AdminConfigReadService</c>, which reports them <c>NOT_WIRED</c>.
/// </param>
public sealed record IntegrationProfileEffectView(
    [property: JsonPropertyName("dependency")] string Dependency,
    [property: JsonPropertyName("requested_state")] string RequestedState,
    [property: JsonPropertyName("enforced")] bool Enforced,
    [property: JsonPropertyName("detail")] string Detail);

public sealed record IntegrationProfileApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("expected")] string Expected,
    [property: JsonPropertyName("enforced_count")] int EnforcedCount,
    [property: JsonPropertyName("declared_only_count")] int DeclaredOnlyCount,
    [property: JsonPropertyName("effects")] IReadOnlyList<IntegrationProfileEffectView> Effects,
    [property: JsonPropertyName("correlation_id")] string CorrelationId);
