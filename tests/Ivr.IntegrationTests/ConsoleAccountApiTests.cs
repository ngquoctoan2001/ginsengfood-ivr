using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ivr.Api.Accounts;
using Ivr.Domain.Accounts;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Accounts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ConsoleAccountApiTests(PostgresPersistenceFixture fixture)
{
    private const string Password = "123123123zZ*";

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-RBAC-01")]
    public async Task OperatorHasExactlyTheApprovedOperationalPermissions()
    {
        await fixture.ResetAsync();
        await SeedRequestedAccountsAsync();
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await SignInAsync(
            app.Client,
            "ngquoctoan2001",
            Password,
            HttpStatusCode.OK);
        Assert.Equal(
            ["IVR_ACCOUNT_SELF_VIEW", "IVR_MANUAL_RETRY", "IVR_QUEUE_VIEW", "IVR_SIM_DISABLE"],
            session.Session.Permissions);

        Assert.Equal(HttpStatusCode.OK,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/accounts/me", session, false)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/rbac/queue", session)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await SendBearerAsync(app.Client, HttpMethod.Post, "/rbac/sim-disable", session)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await SendBearerAsync(app.Client, HttpMethod.Post, "/rbac/manual-retry", session)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await SendBearerAsync(app.Client, HttpMethod.Post, "/rbac/queue-pause", session)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/accounts", session)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/account-roles", session)).StatusCode);
    }

    /// <summary>
    /// <c>OD-V1-20</c> (2026-08-22) granted <c>IVR_FLAG_READ</c> and <c>IVR_RUNTIME_GATE_ADMIN</c>
    /// to Admin. The Operator case above pins a set that must not grow; this pins the Admin set,
    /// which just did. It asserts the whole ordered list rather than only the two new entries, so
    /// a later grant cannot ride into the session projection unnoticed on the back of this one.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-RBAC-02")]
    public async Task AdminCarriesTheRuntimeFlagPermissionsGrantedByOdV120()
    {
        await fixture.ResetAsync();
        await SeedRequestedAccountsAsync();
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await SignInAsync(
            app.Client,
            "admin",
            Password,
            HttpStatusCode.OK);

        Assert.Equal(
            [
                "IVR_ACCOUNT_MANAGE",
                "IVR_ACCOUNT_PASSWORD_RESET",
                "IVR_ACCOUNT_SELF_VIEW",
                "IVR_ACCOUNT_VIEW",
                "IVR_FLAG_READ",
                "IVR_MANUAL_RETRY",
                "IVR_QUEUE_PAUSE",
                "IVR_QUEUE_RESUME",
                "IVR_QUEUE_VIEW",
                "IVR_RESULT_REVIEW",
                "IVR_RUNTIME_GATE_ADMIN",
                "IVR_SIM_DISABLE",
                "IVR_SIM_ENABLE",
            ],
            session.Session.Permissions);
    }

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-CRUD-02")]
    public async Task AdminCanCreateResetDisableAndDeleteWithoutReusingAUsername()
    {
        await fixture.ResetAsync();
        await SeedRequestedAccountsAsync();
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app.Client, "admin", Password, HttpStatusCode.OK);

        var create = new CreateConsoleAccountRequest(
            "operator.test",
            "Nhân viên thử nghiệm",
            ConsoleAccountRoles.Operator,
            "AnotherStrong1!",
            "Tạo tài khoản kiểm thử W-0105");
        ConsoleAccountView created = await SendMutationAsync<CreateConsoleAccountRequest, ConsoleAccountView>(
            app.Client,
            HttpMethod.Post,
            "/accounts",
            create,
            admin,
            "account-create-1",
            HttpStatusCode.OK);
        ConsoleAccountView replay = await SendMutationAsync<CreateConsoleAccountRequest, ConsoleAccountView>(
            app.Client,
            HttpMethod.Post,
            "/accounts",
            create,
            admin,
            "account-create-1",
            HttpStatusCode.OK);
        Assert.Equal(created.AccountId, replay.AccountId);

        ConsoleSignInApiResult oldSession = await SignInAsync(
            app.Client,
            created.Username,
            "AnotherStrong1!",
            HttpStatusCode.OK);
        var reset = new ResetConsolePasswordRequest(
            "ReplacementStrong2!",
            oldSession.Session.Account.Version,
            "Reset mật khẩu kiểm thử W-0105");
        _ = await SendMutationAsync<ResetConsolePasswordRequest, ConsoleAccountView>(
            app.Client,
            HttpMethod.Post,
            $"/accounts/{created.AccountId:D}:reset-password",
            reset,
            admin,
            "account-reset-1",
            HttpStatusCode.OK);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/auth/session", oldSession, false)).StatusCode);
        _ = await SignInAsync(app.Client, created.Username, "AnotherStrong1!", HttpStatusCode.Unauthorized);
        ConsoleSignInApiResult newSession = await SignInAsync(
            app.Client,
            created.Username,
            "ReplacementStrong2!",
            HttpStatusCode.OK);

        var update = new UpdateConsoleAccountRequest(
            null,
            null,
            ConsoleAccountStatuses.Disabled,
            newSession.Session.Account.Version,
            "Disable tài khoản kiểm thử W-0105");
        ConsoleAccountView disabled = await SendMutationAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
            app.Client,
            HttpMethod.Patch,
            $"/accounts/{created.AccountId:D}",
            update,
            admin,
            "account-update-1",
            HttpStatusCode.OK);
        Assert.Equal(ConsoleAccountStatuses.Disabled, disabled.Status);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await SendBearerAsync(app.Client, HttpMethod.Get, "/auth/session", newSession, false)).StatusCode);

        var delete = new DeleteConsoleAccountRequest(
            disabled.Version,
            "Xóa mềm tài khoản kiểm thử W-0105");
        ConsoleAccountView deleted = await SendMutationAsync<DeleteConsoleAccountRequest, ConsoleAccountView>(
            app.Client,
            HttpMethod.Delete,
            $"/accounts/{created.AccountId:D}",
            delete,
            admin,
            "account-delete-1",
            HttpStatusCode.OK);
        Assert.Equal(ConsoleAccountStatuses.Deleted, deleted.Status);

        _ = await SendMutationAsync<CreateConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Post,
            "/accounts",
            create with { Password = "FreshPassword3!" },
            admin,
            "account-create-reuse",
            HttpStatusCode.Conflict);
    }

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-LOCK-03")]
    public async Task FifthBadPasswordLocksTheAccountAndResponsesStayGeneric()
    {
        await fixture.ResetAsync();
        await SeedRequestedAccountsAsync();
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        for (int attempt = 0; attempt < 5; attempt++)
        {
            _ = await SignInAsync(
                app.Client,
                "trcongphuc2003",
                "WrongPassword1!",
                HttpStatusCode.Unauthorized);
        }

        _ = await SignInAsync(
            app.Client,
            "trcongphuc2003",
            Password,
            HttpStatusCode.Unauthorized);
        _ = await SignInAsync(
            app.Client,
            "unknown-user",
            Password,
            HttpStatusCode.Unauthorized);

        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        ConsoleAccountEntity account = await dbContext.ConsoleAccounts
            .SingleAsync(item => item.Username == "trcongphuc2003");
        Assert.Equal(5, account.FailedLoginCount);
        Assert.NotNull(account.LockedUntil);
    }

    /// <summary>
    /// A lockout that has expired must hand back the full attempt budget. Before this was fixed
    /// the counter stayed at the threshold, so the first failure after the window re-locked the
    /// account immediately and it never recovered without an administrator resetting the
    /// password. The expired state is written directly because the production TimeProvider is the
    /// system clock; the behaviour under test is the recovery path, not how time passes.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-LOCK-09")]
    public async Task AnExpiredLockoutRestoresTheFullAttemptBudget()
    {
        await fixture.ResetAsync();
        await SeedRequestedAccountsAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using (IvrDbContext setup = await factory.CreateDbContextAsync())
        {
            ConsoleAccountEntity locked = await setup.ConsoleAccounts
                .SingleAsync(item => item.Username == "trcongphuc2003");
            locked.FailedLoginCount = ConsoleLockoutPolicy.MaximumFailedAttempts;
            locked.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(-1);
            await setup.SaveChangesAsync();
        }

        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        // Four failures after the window must not re-lock: the budget is five, not one.
        for (int attempt = 0; attempt < ConsoleLockoutPolicy.MaximumFailedAttempts - 1; attempt++)
        {
            _ = await SignInAsync(
                app.Client, "trcongphuc2003", "WrongPassword1!", HttpStatusCode.Unauthorized);
        }

        await using (IvrDbContext probe = await factory.CreateDbContextAsync())
        {
            ConsoleAccountEntity account = await probe.ConsoleAccounts
                .SingleAsync(item => item.Username == "trcongphuc2003");
            Assert.Equal(ConsoleLockoutPolicy.MaximumFailedAttempts - 1, account.FailedLoginCount);
            Assert.Null(account.LockedUntil);
        }

        // The correct password still works while inside the restored budget.
        ConsoleSignInApiResult session = await SignInAsync(
            app.Client, "trcongphuc2003", Password, HttpStatusCode.OK);
        Assert.Equal("trcongphuc2003", session.Session.Account.Username);

        await using IvrDbContext after = await factory.CreateDbContextAsync();
        ConsoleAccountEntity restored = await after.ConsoleAccounts
            .SingleAsync(item => item.Username == "trcongphuc2003");
        Assert.Equal(0, restored.FailedLoginCount);
        Assert.Null(restored.LockedUntil);
    }

    // Seeding and sign-in live in ConsoleAccountTestAccounts so the three requested W-0105
    // accounts are described once and cannot drift between the account suites.
    private Task SeedRequestedAccountsAsync() => ConsoleAccountTestAccounts.SeedRequestedAsync(
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>());

    private static Task<ConsoleSignInApiResult> SignInAsync(
        HttpClient client,
        string username,
        string password,
        HttpStatusCode expectedStatus) =>
        ConsoleAccountTestAccounts.SignInAsync(client, username, password, expectedStatus);

    private static async Task<HttpResponseMessage> SendBearerAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        ConsoleSignInApiResult session,
        bool includeActor = true)
    {
        using var request = new HttpRequestMessage(
            method,
            path.StartsWith("/v1/", StringComparison.Ordinal)
                ? path
                : $"/v1/ivr/order-confirmation{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        request.Headers.Add("X-Correlation-Id", $"corr-bearer-{Guid.NewGuid():N}");
        if (includeActor)
        {
            request.Headers.Add("X-Actor-Id", session.Session.Account.Username);
        }

        return await client.SendAsync(request);
    }

    private static async Task<TResponse> SendMutationAsync<TRequest, TResponse>(
        HttpClient client,
        HttpMethod method,
        string path,
        TRequest body,
        ConsoleSignInApiResult session,
        string idempotencyKey,
        HttpStatusCode expectedStatus)
    {
        using var request = new HttpRequestMessage(
            method,
            $"/v1/ivr/order-confirmation{path}")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        request.Headers.Add("X-Correlation-Id", $"corr-mutation-{Guid.NewGuid():N}");
        request.Headers.Add("X-Actor-Id", session.Session.Account.Username);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedStatus != HttpStatusCode.OK)
        {
            return default!;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>()
            ?? throw new InvalidOperationException("Mutation response was empty.");
    }
}
