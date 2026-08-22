using System.Collections.Frozen;
using Ivr.Domain.Accounts;

namespace Ivr.Api.Auth;

public static class IvrRoles
{
    public const string Admin = ConsoleAccountRoles.Admin;
    public const string Operator = ConsoleAccountRoles.Operator;
    public const string ConsoleAdminPolicy = "IVR_CONSOLE_ADMIN";

    /// <summary>
    /// W-0105. Restricts an endpoint to the console bearer scheme, so the MOCK permission seam
    /// is not even consulted for it. Applied to every authenticated console auth/account route.
    /// </summary>
    public const string ConsoleSessionPolicy = "IVR_CONSOLE_SESSION";

    /// <summary>
    /// <para>
    /// <c>OD-V1-20</c> approved 2026-08-22 by the IVR module owner: Admin now carries
    /// <see cref="IvrPermissions.FlagRead"/> and <see cref="IvrPermissions.RuntimeGateAdmin"/>.
    /// </para>
    /// <para>
    /// <see cref="IvrPermissions.RuntimeGateAdmin"/> is the permission on
    /// <c>POST /v1/ivr/order-confirmation/feature-flags/{environment}</c>. Granting it moves an
    /// Admin past the authorization layer only — it does not make the mutation succeed.
    /// <c>FeatureFlagAdminService.MutateAsync</c> calls
    /// <c>IRuntimeGateAuthorization.IsApprovedAsync</c> first, and the only implementation
    /// registered outside tests is <c>PendingRuntimeGateAuthorization</c>, which returns
    /// <see langword="false"/> unconditionally. So an Admin POST now returns
    /// <c>409 IVR_OPERATIONAL_BLOCKED</c> where it previously returned
    /// <c>403 IVR_FORBIDDEN_CALLER</c>: a different refusal, not an open door.
    /// </para>
    /// <para>
    /// What did change for real is <see cref="IvrPermissions.FlagRead"/> — the two flag GETs now
    /// answer 200 for an Admin. And the layering changed: permission is no longer the outermost
    /// lock on the runtime gates, so anything that relied on "no role holds this" must now rely on
    /// <c>PendingRuntimeGateAuthorization</c>, the flag values themselves, four-eyes on the
    /// mutation, and the audit trail. Replacing that pending implementation is what would
    /// actually open the gate.
    /// </para>
    /// </summary>
    private static readonly FrozenSet<string> AdminPermissions = new[]
        {
            IvrPermissions.QueueView,
            IvrPermissions.QueuePause,
            IvrPermissions.QueueResume,
            IvrPermissions.SimEnable,
            IvrPermissions.SimDisable,
            IvrPermissions.ManualRetry,
            IvrPermissions.ResultReview,
            IvrPermissions.FlagRead,
            IvrPermissions.RuntimeGateAdmin,
            IvrPermissions.AccountView,
            IvrPermissions.AccountManage,
            IvrPermissions.AccountPasswordReset,
            IvrPermissions.AccountSelfView,

            // W-0109. All seven script permissions land on Admin, which means the role matrix
            // cannot tell a Privacy/Legal officer from any other Admin. The control that still
            // holds is per-account: Content and Privacy/Legal approvals must come from two
            // different signed-in accounts, enforced in ScriptApprovalPolicy and re-checked at
            // read time. A deployment with one Admin account therefore cannot reach production
            // approval at all — which is the correct fail-closed answer, not a bug.
            IvrPermissions.ScriptEdit,
            IvrPermissions.ScriptReview,
            IvrPermissions.ScriptApproveMock,
            IvrPermissions.ScriptApproveLab,
            IvrPermissions.ScriptApproveContent,
            IvrPermissions.ScriptApprovePrivacyLegal,
            IvrPermissions.ScriptRetire,
        }
        .ToFrozenSet(StringComparer.Ordinal);

    private static readonly FrozenSet<string> OperatorPermissions = new[]
        {
            IvrPermissions.QueueView,
            IvrPermissions.SimDisable,
            IvrPermissions.ManualRetry,
            IvrPermissions.AccountSelfView,
        }
        .ToFrozenSet(StringComparer.Ordinal);

    public static IReadOnlyList<string> All { get; } = [Admin, Operator];

    public static IReadOnlySet<string> PermissionsFor(string role) => role switch
    {
        Admin => AdminPermissions,
        Operator => OperatorPermissions,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown console role."),
    };
}
