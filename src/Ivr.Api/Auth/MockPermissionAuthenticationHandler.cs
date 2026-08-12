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
    public const string ActorHeaderName = "X-Mock-Actor-Id";
    public const string DestinationRefHeaderName = "X-Mock-Destination-Ref";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string actorId = Request.Headers[ActorHeaderName].FirstOrDefault()
            ?? "mock-permission-caller";
        List<Claim> claims = [new(ClaimTypes.NameIdentifier, actorId)];

        string? destinationRef = Request.Headers[DestinationRefHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(destinationRef))
        {
            claims.Add(new Claim(FeatureFlagClaims.DestinationRef, destinationRef));
        }

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
