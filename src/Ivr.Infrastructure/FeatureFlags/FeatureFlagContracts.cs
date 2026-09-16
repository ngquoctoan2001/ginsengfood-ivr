using Ivr.Infrastructure.Audit;

namespace Ivr.Infrastructure.FeatureFlags;

public interface IFeatureFlagStore
{
    public Task<FeatureFlagSnapshot> ReadFreshAsync(
        string environment,
        CancellationToken cancellationToken = default);

    public Task<FeatureFlagSnapshot> ApplyAuditedAsync(
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot proposed,
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlags
{
    public Task<FeatureFlagReadResult> GetSnapshotAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default);
}

public interface IDynamicConfig
{
    public Task<FeatureFlagReadResult> GetConfigAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlagRefresher
{
    public Task<FeatureFlagReadResult> RefreshAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

public interface IKillSwitch
{
    public Task<bool> RealCallsEnabledAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The release decision behind real customer dialling.
/// <para>
/// SIP-04. The environment is a parameter, and it has to be. This gate used to ask only whether
/// <i>any</i> live <c>PRODUCTION_CALL</c> approval existed, so one signature granted for a pilot
/// opened every deployment that could reach the same database - which is not what anybody signing
/// it would have believed they were signing.
/// </para>
/// <para>
/// Nothing narrows this one. There is no per-change row behind a production call: the approval
/// <i>is</i> the decision.
/// </para>
/// </summary>
public interface IProductionCallGate
{
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Whether runtime-gate administration is approved <b>for one environment</b>.
/// </summary>
/// <remarks>
/// <para>
/// W-0301. The environment used to be absent here, and the argument for leaving it out was that
/// administration is coarse on purpose: the per-change four-eyes row carries the environment, so
/// this gate only had to answer whether administration was permitted at all. That argument reads
/// well and is wrong in one specific way — the row in the table has an <c>environment</c> column,
/// so an approver could fill it in, believe they had limited the grant to lab, and have granted
/// production as well. A column nobody reads is worse than no column: it invites a promise the
/// system never made.
/// </para>
/// <para>
/// <c>PRODUCTION_CALL</c> was fixed the same way in SIP-04. Applying the rule to one kind and not
/// the other left the weaker half in place for the kind that opens every flag change.
/// </para>
/// <para>
/// The per-change four-eyes row still exists and still carries its own environment fingerprint.
/// This is a second lock on the same door, not a replacement for it.
/// </para>
/// </remarks>
public interface IRuntimeGateAuthorization
{
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

public interface IRuntimeSafetyHealth
{
    public Task<bool> IsAuditProviderHealthyAsync(
        CancellationToken cancellationToken = default);
}

public interface IFourEyesApprovalVerifier
{
    public Task<string?> VerifyAsync(
        string approvalReference,
        string proposerActorId,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        CancellationToken cancellationToken = default);
}

public interface IDispatchGate
{
    public Task<DispatchGateDecision> EvaluateAsync(
        string environment,
        string destinationReference,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlagAdminService
{
    public Task<FeatureFlagMutationResult> MutateAsync(
        FeatureFlagMutationCommand command,
        CancellationToken cancellationToken = default);
}
