using System.Security.Claims;
using System.Text.Encodings.Web;
using Ivr.Api.Accounts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

public sealed class ConsoleSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ConsoleAccountService accountService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IvrConsoleSession";
    public const string SessionIdClaimType = "ivr_console_session_id";
    public const string AccountIdClaimType = "ivr_console_account_id";
    public const string DisplayNameClaimType = "ivr_console_display_name";
    public const string ExpiresAtClaimType = "ivr_console_expires_at";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        string rawToken = authorization["Bearer ".Length..].Trim();
        AuthenticatedConsoleSession? session = await accountService.AuthenticateAsync(
            rawToken,
            Context.RequestAborted);
        if (session is null)
        {
            return AuthenticateResult.Fail("The console session is invalid.");
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.Username),
            new(ClaimTypes.Name, session.DisplayName),
            new(ClaimTypes.Role, session.Role),
            new(SessionIdClaimType, session.SessionId.ToString("D")),
            new(AccountIdClaimType, session.AccountId.ToString("D")),
            new(DisplayNameClaimType, session.DisplayName),
            new(ExpiresAtClaimType, session.ExpiresAt.ToString("O")),
        ];
        claims.AddRange(session.Permissions.Select(permission =>
            new Claim(ClaimsPermissionEvaluator.PermissionClaimType, permission)));
        ClaimsIdentity identity = new(claims, SchemeName);
        return AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            SchemeName));
    }
}
