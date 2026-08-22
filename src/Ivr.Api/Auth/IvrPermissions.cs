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
        }
        .ToFrozenSet(StringComparer.Ordinal);

    /// <summary>Whether the MOCK permission seam is allowed to mint this permission.</summary>
    public static bool IsMockGrantable(string permission) =>
        All.Contains(permission) && !ConsoleSessionOnly.Contains(permission);
}
