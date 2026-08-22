using System.Net;
using System.Net.Http.Json;
using Ivr.Api.Accounts;
using Ivr.Domain.Accounts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0105 remediation. The W-0105 evidence pack claimed <c>IT-ACCOUNT-CRUD-02</c> covered role
/// change, reactivation, the built-in and last-active-admin invariants and audit. It did not —
/// those paths existed in <see cref="ConsoleAccountService"/> but nothing exercised them, so the
/// claims had no backing test. These tests supply the missing proof rather than the claims being
/// withdrawn, because each invariant is real and worth keeping.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ConsoleAccountInvariantTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-CRUD-08")]
    public async Task RoleChangeAndReactivationRevokeSessionsAndSurviveASignInRoundTrip()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "admin");

        ConsoleSignInApiResult before = await SignInAsync(app, "trcongphuc2003");
        Assert.Equal(ConsoleAccountRoles.Operator, before.Session.Account.Role);
        Guid targetId = before.Session.Account.AccountId;
        string route = $"/accounts/{targetId:D}";

        // Promote. A role change must invalidate the session that was issued under the old role,
        // otherwise the old permission set stays live for up to eight hours.
        ConsoleAccountView promoted = await ConsoleAccountTestAccounts
            .MutateAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Patch,
                route,
                new UpdateConsoleAccountRequest(
                    null,
                    ConsoleAccountRoles.Admin,
                    null,
                    before.Session.Account.Version,
                    "W-0105 promote operator to admin"),
                admin);
        Assert.Equal(ConsoleAccountRoles.Admin, promoted.Role);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await ConsoleAccountTestAccounts.SendAsync(
                app.Client, HttpMethod.Get, "/auth/session", before, false)).StatusCode);

        ConsoleSignInApiResult asAdmin = await SignInAsync(app, "trcongphuc2003");
        Assert.Equal(ConsoleAccountRoles.Admin, asAdmin.Session.Account.Role);
        Assert.Contains("IVR_ACCOUNT_MANAGE", asAdmin.Session.Permissions);

        // Disable, then reactivate. Both are status transitions through the same route, and the
        // account must be usable again afterwards rather than merely reporting ACTIVE.
        //
        // The version comes from `asAdmin`, not from `promoted`: a successful sign-in updates
        // last_login_at and therefore bumps the optimistic-concurrency version. Reusing the
        // pre-login version here answers 409, which is the same surprise an administrator meets
        // when the user they are editing signs in between loading the form and saving it.
        ConsoleAccountView disabled = await ConsoleAccountTestAccounts
            .MutateAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Patch,
                route,
                new UpdateConsoleAccountRequest(
                    null,
                    null,
                    ConsoleAccountStatuses.Disabled,
                    asAdmin.Session.Account.Version,
                    "W-0105 disable for reactivation check"),
                admin);
        Assert.Equal(ConsoleAccountStatuses.Disabled, disabled.Status);
        _ = await ConsoleAccountTestAccounts.SignInAsync(
            app.Client,
            "trcongphuc2003",
            ConsoleAccountTestAccounts.Password,
            HttpStatusCode.Unauthorized);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await ConsoleAccountTestAccounts.SendAsync(
                app.Client, HttpMethod.Get, "/auth/session", asAdmin, false)).StatusCode);

        ConsoleAccountView reactivated = await ConsoleAccountTestAccounts
            .MutateAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Patch,
                route,
                new UpdateConsoleAccountRequest(
                    null,
                    null,
                    ConsoleAccountStatuses.Active,
                    disabled.Version,
                    "W-0105 reactivate"),
                admin);
        Assert.Equal(ConsoleAccountStatuses.Active, reactivated.Status);
        ConsoleSignInApiResult afterReactivation = await SignInAsync(app, "trcongphuc2003");
        Assert.Equal(ConsoleAccountRoles.Admin, afterReactivation.Session.Account.Role);

        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        Assert.Equal(
            3,
            await dbContext.AuditLog.CountAsync(entry =>
                entry.Action == "ADMIN_ACCOUNT_UPDATE" && entry.TargetId == "trcongphuc2003"));
    }

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-CRUD-08")]
    public async Task BuiltInAdminCannotBeDemotedDisabledOrDeleted()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "admin");
        Guid adminId = admin.Session.Account.AccountId;
        long version = admin.Session.Account.Version;
        string route = $"/accounts/{adminId:D}";

        Assert.True(admin.Session.Account.IsBuiltin);

        await ConsoleAccountTestAccounts.MutateAsync<UpdateConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Patch,
            route,
            new UpdateConsoleAccountRequest(
                null, ConsoleAccountRoles.Operator, null, version, "W-0105 demote built-in"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");

        await ConsoleAccountTestAccounts.MutateAsync<UpdateConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Patch,
            route,
            new UpdateConsoleAccountRequest(
                null, null, ConsoleAccountStatuses.Disabled, version, "W-0105 disable built-in"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");

        await ConsoleAccountTestAccounts.MutateAsync<DeleteConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Delete,
            route,
            new DeleteConsoleAccountRequest(version, "W-0105 delete built-in"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");

        // A refused mutation must leave no trace: same role, same status, same version.
        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        ConsoleAccountEntity stored = await dbContext.ConsoleAccounts
            .SingleAsync(account => account.Username == "admin");
        Assert.Equal(ConsoleAccountRoles.Admin, stored.Role);
        Assert.Equal(ConsoleAccountStatuses.Active, stored.Status);
        Assert.Null(stored.DeletedAt);
        Assert.Equal(version, stored.Version);
        Assert.Equal(
            0,
            await dbContext.AuditLog.CountAsync(entry =>
                entry.TargetId == "admin" && entry.Action.StartsWith("ADMIN_ACCOUNT_")));
    }

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-CRUD-08")]
    public async Task TheLastActiveAdminCannotDemoteOrDeleteItself()
    {
        await fixture.ResetAsync();
        // No built-in account here on purpose: the built-in guard fires first and would mask the
        // last-active-admin rule, so this seed makes the sole admin an ordinary one.
        await ConsoleAccountTestAccounts.SeedAsync(
            Factory(),
            new ConsoleAccountSeed("solo.admin", "Quản trị duy nhất", ConsoleAccountRoles.Admin, false),
            new ConsoleAccountSeed("solo.operator", "Nhân viên", ConsoleAccountRoles.Operator, false));
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "solo.admin");
        string route = $"/accounts/{admin.Session.Account.AccountId:D}";
        long version = admin.Session.Account.Version;

        await ConsoleAccountTestAccounts.MutateAsync<UpdateConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Patch,
            route,
            new UpdateConsoleAccountRequest(
                null, ConsoleAccountRoles.Operator, null, version, "W-0105 demote last admin"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");

        await ConsoleAccountTestAccounts.MutateAsync<DeleteConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Delete,
            route,
            new DeleteConsoleAccountRequest(version, "W-0105 delete last admin"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");

        // Promoting a second admin is what unblocks it, which is the escape hatch an operator
        // team needs when the sole admin is leaving.
        ConsoleSignInApiResult operatorSession = await SignInAsync(app, "solo.operator");
        _ = await ConsoleAccountTestAccounts
            .MutateAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Patch,
                $"/accounts/{operatorSession.Session.Account.AccountId:D}",
                new UpdateConsoleAccountRequest(
                    null,
                    ConsoleAccountRoles.Admin,
                    null,
                    operatorSession.Session.Account.Version,
                    "W-0105 promote second admin"),
                admin);

        ConsoleAccountView demoted = await ConsoleAccountTestAccounts
            .MutateAsync<UpdateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Patch,
                route,
                new UpdateConsoleAccountRequest(
                    null, ConsoleAccountRoles.Operator, null, version, "W-0105 demote once safe"),
                admin);
        Assert.Equal(ConsoleAccountRoles.Operator, demoted.Role);
    }

    [Fact]
    [Trait("TestId", "IT-ACCOUNT-CRUD-08")]
    public async Task EveryMutationWritesExactlyOneAuditRowAndNoneCarriesCredentialMaterial()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "admin");
        const string created = "audited.operator";
        const string createdPassword = "AuditedStrong1!";
        const string resetPassword = "AuditedReplaced2!";

        ConsoleAccountView account = await ConsoleAccountTestAccounts
            .MutateAsync<CreateConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Post,
                "/accounts",
                new CreateConsoleAccountRequest(
                    created,
                    "Nhân viên có kiểm toán",
                    ConsoleAccountRoles.Operator,
                    createdPassword,
                    "W-0105 audit coverage create"),
                admin);
        ConsoleAccountView reset = await ConsoleAccountTestAccounts
            .MutateAsync<ResetConsolePasswordRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Post,
                $"/accounts/{account.AccountId:D}:reset-password",
                new ResetConsolePasswordRequest(
                    resetPassword, account.Version, "W-0105 audit coverage reset"),
                admin);
        _ = await ConsoleAccountTestAccounts
            .MutateAsync<DeleteConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Delete,
                $"/accounts/{account.AccountId:D}",
                new DeleteConsoleAccountRequest(reset.Version, "W-0105 audit coverage delete"),
                admin);

        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        AuditLogEntity[] entries = await dbContext.AuditLog
            .Where(entry => entry.TargetId == created)
            .OrderBy(entry => entry.CreatedAt)
            .ToArrayAsync();

        Assert.Equal(
            ["ADMIN_ACCOUNT_CREATE", "ADMIN_ACCOUNT_PASSWORD_RESET", "ADMIN_ACCOUNT_DELETE"],
            entries.Select(entry => entry.Action));
        Assert.All(entries, entry =>
        {
            Assert.Equal("admin", entry.ActorId);
            Assert.Equal("console_account", entry.TargetType);
            Assert.False(string.IsNullOrWhiteSpace(entry.Reason));
            Assert.False(string.IsNullOrWhiteSpace(entry.CorrelationId));
        });

        // The audit trail is read by more people than the API is, so it must never become the
        // place a password, a verifier or a live bearer token leaks.
        string[] forbidden =
            [createdPassword, resetPassword, "PBKDF2", "ivr_session_", "password_hash"];
        Assert.All(entries, entry =>
        {
            string payload = string.Concat(
                entry.BeforeStateJson, entry.AfterStateJson, entry.DataJson, entry.Reason);
            Assert.All(forbidden, secret =>
                Assert.DoesNotContain(secret, payload, StringComparison.OrdinalIgnoreCase));
        });
    }

    /// <summary>
    /// A soft-deleted account stays in the table so audit identity survives and its username is
    /// never reassigned. It is not an administrable row, so the roster excludes it unless asked
    /// — and `total_count` has to follow the same filter, or paging reports a total the caller
    /// can never page to.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-LIST-10")]
    public async Task SoftDeletedAccountsAreExcludedFromTheRosterUnlessRequested()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "admin");

        ConsoleAccountPageApiResult before = await ListAsync(app, admin, includeDeleted: null);
        Assert.Equal(3, before.TotalCount);

        ConsoleAccountView target = Assert.Single(
            before.Items,
            item => item.Username == "trcongphuc2003");
        _ = await ConsoleAccountTestAccounts
            .MutateAsync<DeleteConsoleAccountRequest, ConsoleAccountView>(
                app.Client,
                HttpMethod.Delete,
                $"/accounts/{target.AccountId:D}",
                new DeleteConsoleAccountRequest(target.Version, "W-0105 roster filter check"),
                admin);

        ConsoleAccountPageApiResult defaultView = await ListAsync(app, admin, includeDeleted: null);
        Assert.Equal(2, defaultView.TotalCount);
        Assert.Equal(2, defaultView.Items.Count);
        Assert.DoesNotContain(defaultView.Items, item => item.Username == "trcongphuc2003");
        Assert.All(defaultView.Items, item => Assert.Null(item.DeletedAt));

        ConsoleAccountPageApiResult explicitFalse = await ListAsync(app, admin, includeDeleted: false);
        Assert.Equal(2, explicitFalse.TotalCount);

        ConsoleAccountPageApiResult withDeleted = await ListAsync(app, admin, includeDeleted: true);
        Assert.Equal(3, withDeleted.TotalCount);
        ConsoleAccountView deleted = Assert.Single(
            withDeleted.Items,
            item => item.Username == "trcongphuc2003");
        Assert.Equal(ConsoleAccountStatuses.Deleted, deleted.Status);
        Assert.NotNull(deleted.DeletedAt);
    }

    /// <summary>
    /// An unaccented Vietnamese surname used to reach the customer-PII guard through
    /// <c>display_name</c>, which threw <c>InvalidOperationException</c> and surfaced as
    /// <c>500 IVR_INTERNAL_ERROR</c> — the admin could not create the account at all, and the
    /// error said the system was broken rather than the name refused. The name must now round
    /// trip through create, the idempotency snapshot, the response filter and a re-read, while a
    /// contact number in the same field is still refused, as 422 rather than 500.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-NAME-11")]
    public async Task AnUnaccentedVietnameseSurnameRoundTripsAndAContactNumberIsStillRefused()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        ConsoleSignInApiResult admin = await SignInAsync(app, "admin");

        foreach (string surname in (string[])["Duong Minh Tuan", "Ngo Van A", "Nguyễn Quốc Toàn B"])
        {
            string username = $"name.{Guid.NewGuid():N}"[..20];
            ConsoleAccountView created = await ConsoleAccountTestAccounts
                .MutateAsync<CreateConsoleAccountRequest, ConsoleAccountView>(
                    app.Client,
                    HttpMethod.Post,
                    "/accounts",
                    new CreateConsoleAccountRequest(
                        username,
                        surname,
                        ConsoleAccountRoles.Operator,
                        "NameRoundTrip1!",
                        "W-0105 display-name policy check"),
                    admin);
            Assert.Equal(surname, created.DisplayName);

            using HttpResponseMessage read = await ConsoleAccountTestAccounts.SendAsync(
                app.Client, HttpMethod.Get, $"/accounts/{created.AccountId:D}", admin);
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            ConsoleAccountView stored =
                await read.Content.ReadFromJsonAsync<ConsoleAccountView>()
                ?? throw new InvalidOperationException("Account read was empty.");
            Assert.Equal(surname, stored.DisplayName);
        }

        await ConsoleAccountTestAccounts.MutateAsync<CreateConsoleAccountRequest, object>(
            app.Client,
            HttpMethod.Post,
            "/accounts",
            new CreateConsoleAccountRequest(
                "name.withphone",
                "Nhân viên 0912345678",
                ConsoleAccountRoles.Operator,
                "NameRoundTrip1!",
                "W-0105 contact number must still be refused"),
            admin,
            HttpStatusCode.UnprocessableEntity,
            "IVR_ACCOUNT_POLICY_VIOLATION");
    }

    private static async Task<ConsoleAccountPageApiResult> ListAsync(
        ConsoleAccountApiTestApplication app,
        ConsoleSignInApiResult session,
        bool? includeDeleted)
    {
        string path = includeDeleted is null
            ? "/accounts"
            : $"/accounts?include_deleted={(includeDeleted.Value ? "true" : "false")}";
        using HttpResponseMessage response = await ConsoleAccountTestAccounts.SendAsync(
            app.Client, HttpMethod.Get, path, session);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<ConsoleAccountPageApiResult>()
            ?? throw new InvalidOperationException("Account page response was empty.");
    }

    private static Task<ConsoleSignInApiResult> SignInAsync(
        ConsoleAccountApiTestApplication app,
        string username) => ConsoleAccountTestAccounts.SignInAsync(
        app.Client,
        username,
        ConsoleAccountTestAccounts.Password);

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
}
