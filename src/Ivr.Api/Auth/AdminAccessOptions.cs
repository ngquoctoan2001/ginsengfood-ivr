using Ivr.Domain.Privacy;

namespace Ivr.Api.Auth;

/// <summary>
/// W-0128. Service credentials for the IVR admin surface, split into three tiers.
/// <para>
/// This replaces the console account system. IVR no longer holds human identities: Module 3 owns
/// the operator console and its accounts, and reaches IVR as a service. What IVR still owes is a
/// blast radius, so the tiers are three <b>separate tokens</b> rather than one token plus a scope
/// header. A header the caller writes about itself is a declaration, not a boundary — anyone
/// holding the token can write any header. Splitting the secret is what actually stops the
/// credential behind a reporting screen from also stopping every call in flight.
/// </para>
/// </summary>
public sealed class AdminAccessOptions
{
    public const string ReadTokenConfigurationKey = "IVR_ADMIN_READ_TOKEN";
    public const string ReadTokenPreviousConfigurationKey = "IVR_ADMIN_READ_TOKEN_PREVIOUS";
    public const string ReadTokenPreviousRetiresAtConfigurationKey =
        "IVR_ADMIN_READ_TOKEN_PREVIOUS_RETIRES_AT";
    public const string WriteTokenConfigurationKey = "IVR_ADMIN_WRITE_TOKEN";
    public const string WriteTokenPreviousConfigurationKey = "IVR_ADMIN_WRITE_TOKEN_PREVIOUS";
    public const string WriteTokenPreviousRetiresAtConfigurationKey =
        "IVR_ADMIN_WRITE_TOKEN_PREVIOUS_RETIRES_AT";
    public const string DangerTokenConfigurationKey = "IVR_ADMIN_DANGER_TOKEN";
    public const string DangerTokenPreviousConfigurationKey = "IVR_ADMIN_DANGER_TOKEN_PREVIOUS";
    public const string DangerTokenPreviousRetiresAtConfigurationKey =
        "IVR_ADMIN_DANGER_TOKEN_PREVIOUS_RETIRES_AT";

    public string ReadToken { get; set; } = string.Empty;

    public string ReadTokenPrevious { get; set; } = string.Empty;

    public DateTimeOffset? ReadTokenPreviousRetiresAt { get; set; }

    public string WriteToken { get; set; } = string.Empty;

    public string WriteTokenPrevious { get; set; } = string.Empty;

    public DateTimeOffset? WriteTokenPreviousRetiresAt { get; set; }

    public string DangerToken { get; set; } = string.Empty;

    public string DangerTokenPrevious { get; set; } = string.Empty;

    public DateTimeOffset? DangerTokenPreviousRetiresAt { get; set; }
}

/// <summary>
/// The three admin tiers. Ordered by what a leaked credential costs, not by how often it is used.
/// </summary>
public enum AdminScope
{
    /// <summary>Dashboards, queue, call jobs, reports, review lists, SIM channel status.</summary>
    Read,

    /// <summary>Creating tasks, recording results, opening admin reviews.</summary>
    Write,

    /// <summary>
    /// Kill switch, call termination, SIM disable, manual retry. Everything here changes what the
    /// system does to customers in flight, and every one of them additionally requires a named
    /// human and a reason.
    /// </summary>
    Danger,
}

public static class AdminScopeGuard
{
    public const string ScopeHeaderName = "X-Service-Scope";
    public const string ActorHeaderName = "X-Actor-Id";
    public const string ReasonHeaderName = "X-Action-Reason";

    public const string ReadScopeValue = "ivr.admin.read";
    public const string WriteScopeValue = "ivr.admin.write";
    public const string DangerScopeValue = "ivr.admin.danger";

    private const int MaxActorLength = 128;
    private const int MaxReasonLength = 500;

    public static string ScopeValueOf(AdminScope scope) => scope switch
    {
        AdminScope.Read => ReadScopeValue,
        AdminScope.Write => WriteScopeValue,
        AdminScope.Danger => DangerScopeValue,
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };

    /// <summary>
    /// The extra evidence the danger tier requires beyond a valid credential.
    /// <para>
    /// Authentication proves which tier the caller holds; this proves who inside Module 3 asked
    /// for it and why. Without both, the audit row for "who stopped every call at 3am" would read
    /// <c>service</c>, which answers nothing on the morning after.
    /// </para>
    /// </summary>
    public static bool HasDangerEvidence(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string actor = context.Request.Headers[ActorHeaderName].ToString();
        string reason = context.Request.Headers[ReasonHeaderName].ToString();
        return !string.IsNullOrWhiteSpace(actor)
            && actor.Length <= MaxActorLength
            && PiiGuard.IsSafeText(actor)
            && !string.IsNullOrWhiteSpace(reason)
            && reason.Length <= MaxReasonLength
            && PiiGuard.IsSafeText(reason);
    }

}
