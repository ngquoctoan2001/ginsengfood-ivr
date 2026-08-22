using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <summary>
/// Creates a new script version. There is no update: a version is immutable once created, so a
/// wording change is a new version and the old one keeps its own approvals and audit trail.
/// </summary>
public sealed record ScriptDraftRequest(
    [property: JsonPropertyName("template_id")] string TemplateId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("template_text")] string TemplateText,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record ScriptTransitionRequest(
    [property: JsonPropertyName("reason")] string Reason);

/// <summary>
/// One approval. <c>approval_type</c> is <c>MOCK_TEST</c>, <c>LAB</c>, <c>CONTENT</c> or
/// <c>PRIVACY_LEGAL</c>; each maps to its own permission, and Content and Privacy/Legal must
/// come from two different accounts.
/// </summary>
public sealed record ScriptApprovalRequest(
    [property: JsonPropertyName("approval_type")] string ApprovalType,
    [property: JsonPropertyName("reason")] string Reason);

/// <summary>Matches the catalogue's <c>IvrScriptApproval</c>, correlation id included.</summary>
public sealed record ScriptApprovalApiResult(
    [property: JsonPropertyName("approval_type")] string ApprovalType,
    [property: JsonPropertyName("actor_id")] string ActorId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("approved_at")] DateTimeOffset ApprovedAt);

/// <summary>
/// A script version as the console sees it.
/// </summary>
/// <param name="ApprovedForModes">
/// Modes this version may actually be spoken in right now. Derived from the approvals rather
/// than stored, because the same approval set means different things once
/// <c>ProductionTargetV1FieldsApproved</c> changes — reporting a stored answer would let the
/// console claim production readiness that <c>OD-V1-15</c> has not granted.
/// </param>
/// <param name="ProductionBlockedReason">
/// Why production is not available, or <see langword="null"/> when it is. Stated rather than
/// left for the reader to derive from an empty mode list.
/// </param>
public sealed record ScriptVersionApiResult(
    [property: JsonPropertyName("template_id")] string TemplateId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("template_text")] string TemplateText,
    [property: JsonPropertyName("template_hash")] string TemplateHash,
    [property: JsonPropertyName("allowed_input_fields")] IReadOnlyList<string> AllowedInputFields,
    [property: JsonPropertyName("approvals")] IReadOnlyList<ScriptApprovalApiResult> Approvals,
    [property: JsonPropertyName("created_by")] string CreatedBy,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("submitted_by")] string? SubmittedBy,
    [property: JsonPropertyName("submitted_at")] DateTimeOffset? SubmittedAt,
    [property: JsonPropertyName("retired_by")] string? RetiredBy,
    [property: JsonPropertyName("retired_at")] DateTimeOffset? RetiredAt,
    [property: JsonPropertyName("uses_production_decision_fields")] bool UsesProductionDecisionFields,
    [property: JsonPropertyName("approved_for_modes")] IReadOnlyList<string> ApprovedForModes,
    [property: JsonPropertyName("production_blocked_reason")] string? ProductionBlockedReason);

/// <summary>
/// Result of a lifecycle transition. Mirrors <c>AdminActionApiResult</c>'s shape, including
/// <c>no_policy_bypass</c>, so every admin mutation reads the same way in the audit trail.
/// </summary>
public sealed record ScriptActionApiResult(
    [property: JsonPropertyName("action_type")] string ActionType,
    [property: JsonPropertyName("target_type")] string TargetType,
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("no_policy_bypass")] bool NoPolicyBypass,
    [property: JsonPropertyName("version")] ScriptVersionApiResult Version);
