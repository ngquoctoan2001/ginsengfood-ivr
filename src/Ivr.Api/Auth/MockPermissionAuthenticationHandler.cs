using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

public sealed class MockPermissionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IvrMockPermissions";
    public const string HeaderName = "X-Permissions";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, "mock-permission-caller"),
        ];

        string[] requestedPermissions = Request.Headers[HeaderName]
            .SelectMany(value => (value ?? string.Empty).Split(
                [',', ';', ' '],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        claims.AddRange(
            requestedPermissions
                .Where(IvrPermissions.All.Contains)
                .Select(permission => new Claim(
                    ClaimsPermissionEvaluator.PermissionClaimType,
                    permission)));

        ClaimsIdentity identity = new(claims, SchemeName);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
