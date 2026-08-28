using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

/// <summary>
/// W-0128. The only authentication scheme on the admin surface once console accounts are gone.
/// <para>
/// It answers one question — which of the three tiers does this caller hold — by matching the
/// bearer token against the configured credentials. It deliberately does not decide whether the
/// caller may reach a given endpoint; that is the policy's job, and keeping the two apart is what
/// lets a single handler serve read, write and danger without any of them widening the others.
/// </para>
/// <para>
/// Tiers nest: danger implies write implies read. A leaked read credential still cannot stop a
/// call in flight, which is the property the split exists for. The reverse containment costs
/// nothing, and without it Module 3 would have to carry three tokens through every call path.
/// </para>
/// </summary>
public sealed class AdminTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AdminCredentialSource adminCredentials)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IvrAdminServiceToken";
    public const string ScopeClaimType = "ivr:admin_scope";

    /// <summary>
    /// Script approvals Module 3 asserts for the acting operator, one claim per approval.
    /// <para>
    /// Self-asserted by design: Module 3 owns operator identity now, so it is the authority on
    /// what its user holds. IVR records the assertion and still enforces the domain rule that an
    /// actor cannot approve a script it edited.
    /// </para>
    /// </summary>
    public const string ScriptPermissionClaimType = "ivr:script_permission";

    public const string ScriptPermissionsHeaderName = "X-Script-Permissions";

    private const string BearerPrefix = "Bearer ";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith(BearerPrefix, StringComparison.Ordinal)
            || authorization.Length == BearerPrefix.Length)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string supplied = authorization[BearerPrefix.Length..];
        // Highest tier first: a token configured for danger also satisfies write and read, and
        // checking downward keeps that ordering explicit rather than implied by claim arithmetic
        // somewhere else.
        AdminScope? scope = AdminTokenMatcher.Match(supplied, adminCredentials);
        if (scope is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims =
        [
            new Claim(ScopeClaimType, AdminScopeGuard.ScopeValueOf(scope.Value)),
        ];
        claims.AddRange(Request.Headers[ScriptPermissionsHeaderName]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(16)
            .Select(permission => new Claim(ScriptPermissionClaimType, permission)));
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}

internal static class AdminTokenMatcher
{
    /// <summary>
    /// Returns the highest tier the supplied token satisfies, or <see langword="null"/>.
    /// <para>
    /// A tier whose token is blank never matches. That is the fail-closed rule: a half-provisioned
    /// environment must reject rather than treat "no credential configured" as "no credential
    /// required".
    /// </para>
    /// </summary>
    public static AdminScope? Match(string supplied, AdminCredentialSource credentials) =>
        credentials.Match(supplied);

    /// <summary>Ranks a tier so policies can express "at least this much".</summary>
    public static int RankOf(AdminScope scope) => scope switch
    {
        AdminScope.Read => 0,
        AdminScope.Write => 1,
        AdminScope.Danger => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };

    public static bool Satisfies(string? heldScopeValue, AdminScope required) =>
        heldScopeValue switch
        {
            AdminScopeGuard.DangerScopeValue => RankOf(AdminScope.Danger) >= RankOf(required),
            AdminScopeGuard.WriteScopeValue => RankOf(AdminScope.Write) >= RankOf(required),
            AdminScopeGuard.ReadScopeValue => RankOf(AdminScope.Read) >= RankOf(required),
            _ => false,
        };

}
