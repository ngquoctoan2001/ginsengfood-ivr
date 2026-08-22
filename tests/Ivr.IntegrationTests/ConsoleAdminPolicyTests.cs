using System.Net;
using Ivr.Api.Accounts;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0105 remediation. <c>IvrRoles.ConsoleAdminPolicy</c> is what keeps an Operator out of the
/// back-office reads that were written before console login existed — every one of them is
/// declared with the broad <c>IVR_QUEUE_VIEW</c> permission, which Operators legitimately hold,
/// so the role check is the only thing separating them.
///
/// The original W-0105 suite exercised authorization through synthetic <c>/rbac/*</c> probes and
/// never mapped <see cref="Ivr.Api.Admin.IvrAdminEndpoints"/>, so that policy had no coverage at
/// all: it could have been deleted and every test would still have passed. These tests drive the
/// real routes through the real pipeline.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ConsoleAdminPolicyTests(PostgresPersistenceFixture fixture)
{
    private const string ExportReason = "W-0105 regression export reason for admin policy check";

    /// <summary>
    /// Back-office reads an Operator must never reach. All are declared with IVR_QUEUE_VIEW,
    /// which the Operator role holds, so a missing ConsoleAdminPolicy shows up here as 200.
    /// </summary>
    public static TheoryData<string> AdminOnlyReads() =>
    [
        "/scripts",
        "/integration-status",
        "/review-items",
        "/analytics/summary",
        "/analytics/trend",
        "/analytics/breakdown",
        $"/analytics/export?reason={Uri.EscapeDataString(ExportReason)}",
    ];

    /// <summary>Reads an Operator legitimately needs for queue work (Decision B).</summary>
    public static TheoryData<string> OperatorReads() =>
    [
        "/queue",
        "/dashboard",
        "/call-jobs",
        "/sim-channels",
    ];

    [Theory]
    [MemberData(nameof(AdminOnlyReads))]
    [Trait("TestId", "IT-ACCOUNT-ADMINPOLICY-07")]
    public async Task OperatorIsRefusedEveryBackOfficeRead(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await ConsoleAccountTestAccounts.SignInAsync(
            app.Client,
            "ngquoctoan2001",
            ConsoleAccountTestAccounts.Password);

        using HttpResponseMessage response = await ConsoleAccountTestAccounts.SendAsync(
            app.Client,
            HttpMethod.Get,
            path,
            session);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains(
            "IVR_FORBIDDEN_CALLER",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(AdminOnlyReads))]
    [Trait("TestId", "IT-ACCOUNT-ADMINPOLICY-07")]
    public async Task AdminReachesEveryBackOfficeRead(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await ConsoleAccountTestAccounts.SignInAsync(
            app.Client,
            "admin",
            ConsoleAccountTestAccounts.Password);

        using HttpResponseMessage response = await ConsoleAccountTestAccounts.SendAsync(
            app.Client,
            HttpMethod.Get,
            path,
            session);

        // Asserting OK rather than "not 403" on purpose: a policy that refused everyone would
        // satisfy the Operator test above while quietly breaking the console for Admins too.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(OperatorReads))]
    [Trait("TestId", "IT-ACCOUNT-ADMINPOLICY-07")]
    public async Task OperatorKeepsTheQueueReadsDecisionBGrants(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await ConsoleAccountTestAccounts.SignInAsync(
            app.Client,
            "ngquoctoan2001",
            ConsoleAccountTestAccounts.Password);

        using HttpResponseMessage response = await ConsoleAccountTestAccounts.SendAsync(
            app.Client,
            HttpMethod.Get,
            path,
            session);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Mutations are gated by permission rather than by ConsoleAdminPolicy. Authorization runs
    /// before the handler, so a refused call never reaches the queue or the SIM roster.
    /// </summary>
    [Theory]
    [InlineData("/queue:pause")]
    [InlineData("/queue:resume")]
    [InlineData("/sim-channels/SIM-TEST-001:enable")]
    [InlineData("/admin-reviews")]
    [Trait("TestId", "IT-ACCOUNT-ADMINPOLICY-07")]
    public async Task OperatorIsRefusedEveryAdminMutation(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedRequestedAsync(Factory());
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        ConsoleSignInApiResult session = await ConsoleAccountTestAccounts.SignInAsync(
            app.Client,
            "ngquoctoan2001",
            ConsoleAccountTestAccounts.Password);

        using HttpResponseMessage response = await ConsoleAccountTestAccounts.SendAsync(
            app.Client,
            HttpMethod.Post,
            path,
            session);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
}
