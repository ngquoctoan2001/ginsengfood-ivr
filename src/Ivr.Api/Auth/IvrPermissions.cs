using System.Collections.Frozen;

namespace Ivr.Api.Auth;

public static class IvrPermissions
{
    public const string QueueView = "IVR_QUEUE_VIEW";
    public const string QueuePause = "IVR_QUEUE_PAUSE";
    public const string QueueResume = "IVR_QUEUE_RESUME";
    public const string SimEnable = "IVR_SIM_ENABLE";
    public const string SimDisable = "IVR_SIM_DISABLE";
    public const string ManualRetry = "IVR_MANUAL_RETRY";
    public const string ResultReview = "IVR_RESULT_REVIEW";
    public const string FlagRead = "IVR_FLAG_READ";
    public const string RuntimeGateAdmin = "IVR_RUNTIME_GATE_ADMIN";
    public const string AccountView = "IVR_ACCOUNT_VIEW";
    public const string AccountManage = "IVR_ACCOUNT_MANAGE";
    public const string AccountPasswordReset = "IVR_ACCOUNT_PASSWORD_RESET";
    public const string AccountSelfView = "IVR_ACCOUNT_SELF_VIEW";

    /// <summary>
    /// Script lifecycle (W-0109). One console permission per domain permission in
    /// <see cref="Ivr.Domain.Scripts.ScriptPermissions"/>, deliberately not collapsed into a
    /// single "manage scripts" grant.
    /// <para>
    /// Today all seven land on <c>Admin</c>, so the separation buys nothing at the role level —
    /// the real four-eyes control is that Content and Privacy/Legal approvals must come from
    /// two different <em>accounts</em>. Keeping them apart is what makes it possible to hand
    /// Privacy/Legal its own role later without a migration, and it keeps this catalogue
    /// readable against `specs/ui/04`, which names all seven.
    /// </para>
    /// </summary>
    public const string ScriptEdit = "IVR_SCRIPT_EDIT";
    public const string ScriptReview = "IVR_SCRIPT_REVIEW";
    public const string ScriptApproveMock = "IVR_SCRIPT_APPROVE_MOCK";
    public const string ScriptApproveLab = "IVR_SCRIPT_APPROVE_LAB";
    public const string ScriptApproveContent = "IVR_SCRIPT_APPROVE_CONTENT";
    public const string ScriptApprovePrivacyLegal = "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL";
    public const string ScriptRetire = "IVR_SCRIPT_RETIRE";

    /// <summary>
    /// Cutting a call that is already in progress (W-0111).
    /// <para>
    /// Granted to Operator as well as Admin. This is the risk-reducing direction, and making an
    /// operator find an admin while a customer is being read the wrong script would be a control
    /// that costs more than it protects.
    /// </para>
    /// </summary>
    public const string CallTerminate = "IVR_CALL_TERMINATE";

    /// <summary>
    /// The UI-07 non-production developer surface: seed loader, scenario runner and
    /// integration-status profiles (W-0112).
    /// <para>
    /// One permission rather than reusing <see cref="SimEnable"/>/<see cref="SimDisable"/> as
    /// `specs/ui/07` first proposed. Loading fixture tasks and replaying a scenario are not SIM
    /// operations, and folding them into the SIM grants would mean an operator who may take a
    /// faulty channel out of service could also write rows into the database.
    /// </para>
    /// <para>
    /// The permission is not the control that keeps this out of production —
    /// <see cref="Ivr.Infrastructure.Configuration.NonProductionSurface"/> is, and it refuses
    /// whatever the caller holds.
    /// </para>
    /// </summary>
    public const string DevTooling = "IVR_DEV_TOOLING";

    public static IReadOnlySet<string> All { get; } = new[]
        {
            QueueView,
            QueuePause,
            QueueResume,
            SimEnable,
            SimDisable,
            ManualRetry,
            ResultReview,
            FlagRead,
            RuntimeGateAdmin,
            AccountView,
            AccountManage,
            AccountPasswordReset,
            AccountSelfView,
            ScriptEdit,
            ScriptReview,
            ScriptApproveMock,
            ScriptApproveLab,
            ScriptApproveContent,
            ScriptApprovePrivacyLegal,
            ScriptRetire,
            CallTerminate,
            DevTooling,
        }
        .ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// W-0105. Permissions that only a signed-in console session may ever carry.
    /// <para>
    /// <see cref="MockPermissionAuthenticationHandler"/> mints whatever a caller writes in
    /// <c>X-Permissions</c>. That is an acceptable seam for the queue and SIM surfaces, whose
    /// tests predate console login — but account administration is the surface that hands out
    /// and resets passwords. Letting the seam mint these would make every account endpoint
    /// reachable with no credential at all whenever <c>IVR_EXECUTION_MODE=MOCK</c>, which is
    /// the default mode and the one development runs on.
    /// </para>
    /// <para>
    /// This is the source-level half of the control; the endpoint-level half pins those routes
    /// to <c>ConsoleSessionAuthenticationHandler</c> so the seam is never even consulted. Both
    /// exist so a future route that forgets the pin still cannot be reached by a mock caller.
    /// </para>
    /// </summary>
    public static IReadOnlySet<string> ConsoleSessionOnly { get; } = new[]
        {
            AccountView,
            AccountManage,
            AccountPasswordReset,
            AccountSelfView,

            // Script lifecycle joins the account surface here for the same reason, and it is
            // the sharper case: the mock seam mints whatever X-Permissions asks for, MOCK is
            // the default mode, and one of these permissions signs off the wording a customer
            // is read before pressing a key. A header that can mint IVR_SCRIPT_APPROVE_CONTENT
            // is a header that can approve production speech with no credential at all.
            ScriptEdit,
            ScriptReview,
            ScriptApproveMock,
            ScriptApproveLab,
            ScriptApproveContent,
            ScriptApprovePrivacyLegal,
            ScriptRetire,

            // W-0112. The developer surface writes fixture tasks and moves SIM channels. The
            // MOCK seam mints whatever X-Permissions asks for and MOCK is the default mode —
            // which is also the mode every non-production deployment runs in, so this is
            // precisely where the seam and the surface would otherwise overlap.
            DevTooling,
        }
        .ToFrozenSet(StringComparer.Ordinal);

    /// <summary>Whether the MOCK permission seam is allowed to mint this permission.</summary>
    public static bool IsMockGrantable(string permission) =>
        All.Contains(permission) && !ConsoleSessionOnly.Contains(permission);
}
