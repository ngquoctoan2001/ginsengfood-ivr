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

    private static readonly FrozenSet<string> AdminPermissions = new[]
        {
            IvrPermissions.QueueView,
            IvrPermissions.QueuePause,
            IvrPermissions.QueueResume,
            IvrPermissions.SimEnable,
            IvrPermissions.SimDisable,
            IvrPermissions.ManualRetry,
            IvrPermissions.ResultReview,
            IvrPermissions.AccountView,
            IvrPermissions.AccountManage,
            IvrPermissions.AccountPasswordReset,
            IvrPermissions.AccountSelfView,
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
