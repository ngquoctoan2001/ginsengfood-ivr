using System.Security.Claims;
using Ivr.Api.Auth;
using Ivr.Api.Filters;
using Ivr.Api.Internal;
using Ivr.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Ivr.Api.Accounts;

public static class ConsoleAccountEndpoints
{
    public static IEndpointRouteBuilder MapIvrConsoleAccountEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/v1/ivr/order-confirmation")
            .AddEndpointFilter<PiiMaskingFilter>();

        // W-0105. Sign-in is the only anonymous route here. Every other console route is pinned
        // to the console bearer scheme via RequireConsoleSession, so the MOCK X-Permissions seam
        // cannot reach account administration. Removing a pin re-opens a password-free path to
        // account creation and password reset; IT-ACCOUNT-SCHEME-06 fails if one goes missing.
        group.MapPost("/auth/sign-in", SignInAsync).AllowAnonymous();
        group.MapGet("/auth/session", GetSessionAsync)
            .RequireConsoleSession();
        group.MapPost("/auth/sign-out", SignOutAsync)
            .RequireConsoleSession();
        group.MapGet("/accounts", ListAccountsAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountView));
        group.MapGet("/accounts/me", GetMeAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountSelfView));
        group.MapGet("/accounts/{accountId:guid}", GetAccountAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountView));
        group.MapPost("/accounts", CreateAccountAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountManage));
        group.MapPatch("/accounts/{accountId:guid}", UpdateAccountAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountManage));
        group.MapPost("/accounts/{accountId:guid}:reset-password", ResetPasswordAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountPasswordReset));
        group.MapDelete("/accounts/{accountId:guid}", DeleteAccountAsync)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountManage));
        group.MapGet("/account-roles", GetRoles)
            .RequireConsoleSession()
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountView));
        return endpoints;
    }

    private static TBuilder RequireConsoleSession<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.RequireAuthorization(IvrRoles.ConsoleSessionPolicy);

    private static Task<ConsoleSignInApiResult> SignInAsync(
        ConsoleSignInRequest request,
        HttpContext context,
        ConsoleAccountService service,
        ConsoleSignInRateLimiter limiter,
        CancellationToken cancellationToken)
    {
        string username = ConsoleUsernamePolicy.Normalize(request.Username);
        limiter.RequireAllowed(
            username,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        return service.SignInAsync(
            request,
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);
    }

    private static async Task<ConsoleSessionView> GetSessionAsync(
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken)
    {
        ClaimsPrincipal user = context.User;
        ConsoleAccountView account = await service.GetByUsernameAsync(
            user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("The account subject is missing."),
            cancellationToken);
        string role = user.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("The role claim is missing.");
        DateTimeOffset expiresAt = DateTimeOffset.Parse(
            user.FindFirstValue(ConsoleSessionAuthenticationHandler.ExpiresAtClaimType)
                ?? throw new InvalidOperationException("The expiry claim is missing."),
            System.Globalization.CultureInfo.InvariantCulture);
        return new ConsoleSessionView(
            account,
            IvrRoles.PermissionsFor(role).Order(StringComparer.Ordinal).ToArray(),
            expiresAt);
    }

    private static async Task<ConsoleSignOutApiResult> SignOutAsync(
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken)
    {
        string rawToken = RequireBearerToken(context);
        bool revoked = await service.SignOutAsync(
            rawToken,
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);
        return new ConsoleSignOutApiResult(revoked);
    }

    private static Task<ConsoleAccountPageApiResult> ListAccountsAsync(
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 50,
        [FromQuery(Name = "include_deleted")] bool includeDeleted = false)
    {
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.ListAsync(page, pageSize, includeDeleted, cancellationToken);
    }

    private static Task<ConsoleAccountView> GetMeAsync(
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken) => service.GetByUsernameAsync(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The account subject is missing."),
        cancellationToken);

    private static Task<ConsoleAccountView> GetAccountAsync(
        Guid accountId,
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetAsync(accountId, cancellationToken);
    }

    private static Task<ConsoleAccountView> CreateAccountAsync(
        CreateConsoleAccountRequest request,
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken) => service.CreateAsync(
        request,
        InternalRequestGuard.RequireAdminActor(context),
        InternalRequestGuard.RequireCorrelation(context),
        InternalRequestGuard.RequireIdempotencyKey(context),
        cancellationToken);

    private static Task<ConsoleAccountView> UpdateAccountAsync(
        Guid accountId,
        UpdateConsoleAccountRequest request,
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken) => service.UpdateAsync(
        accountId,
        request,
        InternalRequestGuard.RequireAdminActor(context),
        InternalRequestGuard.RequireCorrelation(context),
        InternalRequestGuard.RequireIdempotencyKey(context),
        cancellationToken);

    private static Task<ConsoleAccountView> ResetPasswordAsync(
        Guid accountId,
        ResetConsolePasswordRequest request,
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken) => service.ResetPasswordAsync(
        accountId,
        request,
        InternalRequestGuard.RequireAdminActor(context),
        InternalRequestGuard.RequireCorrelation(context),
        InternalRequestGuard.RequireIdempotencyKey(context),
        cancellationToken);

    private static Task<ConsoleAccountView> DeleteAccountAsync(
        Guid accountId,
        [FromBody] DeleteConsoleAccountRequest request,
        HttpContext context,
        ConsoleAccountService service,
        CancellationToken cancellationToken) => service.DeleteAsync(
        accountId,
        request,
        InternalRequestGuard.RequireAdminActor(context),
        InternalRequestGuard.RequireCorrelation(context),
        InternalRequestGuard.RequireIdempotencyKey(context),
        cancellationToken);

    private static ConsoleRoleMatrixApiResult GetRoles(HttpContext context)
    {
        _ = InternalRequestGuard.RequireAdminActor(context);
        return new ConsoleRoleMatrixApiResult(
        [
            new ConsoleRoleView(
                IvrRoles.Admin,
                "Quản trị viên",
                IvrRoles.PermissionsFor(IvrRoles.Admin).Order(StringComparer.Ordinal).ToArray()),
            new ConsoleRoleView(
                IvrRoles.Operator,
                "Nhân viên vận hành",
                IvrRoles.PermissionsFor(IvrRoles.Operator).Order(StringComparer.Ordinal).ToArray()),
        ]);
    }

    private static string RequireBearerToken(HttpContext context)
    {
        string value = context.Request.Headers.Authorization.ToString();
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The bearer token is missing.");
        }

        return value["Bearer ".Length..].Trim();
    }

}
