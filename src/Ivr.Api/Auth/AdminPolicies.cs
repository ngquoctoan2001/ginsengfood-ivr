namespace Ivr.Api.Auth;

/// <summary>
/// W-0122. Policy names for the three admin tiers.
/// <para>
/// Endpoints name a tier rather than a permission. The permission catalogue this replaced had
/// nineteen entries for a surface with one caller, and every new endpoint had to invent a name
/// before it could be protected. A tier answers the question that actually matters at the call
/// site — how much damage can this do — and there are only three answers.
/// </para>
/// </summary>
public static class AdminPolicies
{
    /// <summary>Reads: dashboards, queue, call jobs, reports, review lists, SIM status.</summary>
    public const string Read = "ivr.admin.read";

    /// <summary>Writes that do not interrupt work in flight: tasks, results, script lifecycle.</summary>
    public const string Write = "ivr.admin.write";

    /// <summary>
    /// Operations that change what the system is doing to customers right now: kill switch, call
    /// termination, SIM disable, manual retry, queue pause/resume. These additionally require
    /// <c>X-Actor-Id</c> and <c>X-Action-Reason</c>.
    /// </summary>
    public const string Danger = "ivr.admin.danger";

    public static string NameOf(AdminScope scope) => scope switch
    {
        AdminScope.Read => Read,
        AdminScope.Write => Write,
        AdminScope.Danger => Danger,
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };
}
