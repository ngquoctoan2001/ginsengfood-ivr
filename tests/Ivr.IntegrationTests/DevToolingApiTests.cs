using Ivr.Api.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ivr.Api.Admin;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// The UI-07 non-production developer surface over HTTP (W-0112).
/// <para>
/// `vi.json` said "no seed or scenario API exists; do it through CLI/SQL". Every acceptance
/// session rebuilt its demo state by hand, which is a session that can be set up wrongly and that
/// the person being shown it cannot re-run. These assert the two things that make it safe to
/// close that gap: production cannot reach it at all, and a rehearsal cannot place a call.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class DevToolingApiTests(PostgresPersistenceFixture fixture)
{
    private const string Root = "/v1/ivr/order-confirmation/dev";

    /// <summary>
    /// The acceptance criterion, and the reason it is 404 rather than 403: a 403 tells a caller
    /// that a seed loader exists at this address in production and that only a permission stands
    /// between them and it. The routes are not mapped at all, so 404 is also simply true.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-PRODGUARD-01")]
    public async Task ProductionServesNoDeveloperRouteAtAll()
    {
        await using DevToolingApiTestApplication app = await StartAsync(
            environmentName: "Production",
            executionMode: IvrOptions.ProductionRealExecutionMode);
        const AdminScope admin = AdminScope.Write;

        foreach (string path in Paths())
        {
            using HttpResponseMessage response = await SendAsync(app, admin, path);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // And nothing was written on the way to that answer. A seed loader that refused the
        // request but left a task behind would have failed at the only thing that matters.
        Assert.Equal(0, await SeededTaskCountAsync(app));
    }

    /// <summary>
    /// The environment label alone is not the control. A deployment calling itself Staging while
    /// running <c>PRODUCTION_REAL</c> dials the same phone network as production, and a name
    /// nobody put on the allowlist is refused rather than assumed safe.
    /// <para>
    /// The third input the guard checks — <c>REAL_CUSTOMER_CALL_ALLOWED</c> — has no case here on
    /// purpose. <c>IvrOptionsValidator</c> already refuses <c>YES</c> at startup, so that
    /// deployment cannot be hosted at all and there is no HTTP surface to assert against. It is
    /// covered at the predicate level by <c>UT-DEVGUARD-02</c>, which is where the guard would
    /// still have to hold if that startup rule were ever relaxed.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("Staging", IvrOptions.ProductionRealExecutionMode)]
    [InlineData("Sandbox", IvrOptions.MockExecutionMode)]
    [Trait("TestId", "IT-DEV-PRODGUARD-02")]
    public async Task ADeploymentThatOnlyLooksNonProductionIsAlsoRefused(
        string environmentName,
        string executionMode)
    {
        await using DevToolingApiTestApplication app = await StartAsync(
            environmentName: environmentName,
            executionMode: executionMode);
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(app, admin, $"{Root}/seed:load");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [Trait("TestId", "IT-DEV-SEED-03")]
    public async Task ASeedLoadAdmitsTheFixturesThroughTheRealIntakePath()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(app, admin, $"{Root}/seed:load");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        SeedLoadApiResult result = (await response.Content
            .ReadFromJsonAsync<SeedLoadApiResult>())!;

        Assert.True(result.TaskCount > 0);
        Assert.Equal(result.TaskCount, result.Tasks.Count);
        Assert.Equal(IvrOptions.MockExecutionMode, result.ExecutionMode);
        Assert.True(result.WindowsRebased);
        Assert.Equal(result.TaskCount, result.RebasedCount);
        // Eight of nine, and the ninth is the point. TASK-TARGET-247-0005 carries
        // call_restriction=true with a BLOCKED_DO_NOT_CALL eligibility snapshot, and it comes
        // back IVR_OPERATIONAL_BLOCKED because the loader goes through intake rather than
        // writing rows. A seed loader that could put a do-not-call customer into the call queue
        // would be the most expensive convenience in this repository.
        Assert.Equal(result.TaskCount - 1, result.AcceptedCount);
        SeedTaskOutcomeView blocked = Assert.Single(
            result.Tasks,
            task => !task.Decision.StartsWith("TASK_ACCEPTED", StringComparison.Ordinal));
        Assert.Equal("TASK-TARGET-247-0005", blocked.TaskId);
        Assert.Equal("IVR_OPERATIONAL_BLOCKED", blocked.Decision);

        // MOCK admits a task as a dry-run job rather than a dispatchable one. Asserting the exact
        // decision matters: if the loader ever started producing CALL_JOB_CREATED in MOCK, a
        // rehearsal would be queuing calls it was never meant to queue.
        Assert.Contains(result.Tasks, task => task.Decision == "TASK_ACCEPTED_DRY_RUN_ONLY");
        Assert.DoesNotContain(
            result.Tasks,
            task => task.Decision == "TASK_ACCEPTED_CALL_JOB_CREATED");
        Assert.True(await SeededTaskCountAsync(app) > 0);
    }

    /// <summary>
    /// The fixtures carry absolute August-2026 instants, so without rebasing every one of them is
    /// refused for an expired confirmation window. This is asserted rather than assumed because
    /// it is the reason the loader rewrites those four timestamps at all — and because a future
    /// change that silently stopped rebasing would otherwise show up as an empty demo on the
    /// morning of an acceptance session.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-SEEDWINDOW-10")]
    public async Task WithoutRebasingEveryFixtureIsRefusedForAnExpiredWindow()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(
            app,
            admin,
            $"{Root}/seed:load",
            new { reason = "Load the fixtures exactly as written", rebase_windows = false });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        SeedLoadApiResult result = (await response.Content
            .ReadFromJsonAsync<SeedLoadApiResult>())!;

        Assert.False(result.WindowsRebased);
        Assert.Equal(0, result.RebasedCount);
        Assert.Equal(0, result.AcceptedCount);
        Assert.All(
            result.Tasks,
            task => Assert.Equal("IVR_STATE_NOT_CALLABLE", task.Decision));
        Assert.Equal(0, await SeededTaskCountAsync(app));
    }

    /// <summary>
    /// Re-running the loader is what an acceptance session actually does — twice in a morning,
    /// after something went wrong the first time. The second run must not double the demo data.
    /// <para>
    /// It also must not fail the whole request. The fixture keys are stable but the rebased
    /// windows are not, so every task is an idempotency conflict by definition on a second run;
    /// reported per fixture, the response says "already loaded" instead of returning one 409 that
    /// hides which eight tasks are sitting in the database.
    /// </para>
    /// <para>
    /// What this does NOT do is refresh the windows. A reloaded fixture keeps the window it was
    /// admitted with, so a demo left running past that window needs a database reset rather than
    /// a second load. Stated here because a test named "loading twice" is exactly where someone
    /// will look for that answer.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-SEED-04")]
    public async Task LoadingTwiceReportsTheConflictPerTaskAndAddsNothing()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;

        using (HttpResponseMessage first = await SendAsync(app, admin, $"{Root}/seed:load"))
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        }

        int afterFirst = await SeededTaskCountAsync(app);
        Assert.True(afterFirst > 0);

        using HttpResponseMessage second = await SendAsync(app, admin, $"{Root}/seed:load");
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        SeedLoadApiResult repeat = (await second.Content
            .ReadFromJsonAsync<SeedLoadApiResult>())!;

        Assert.Equal(0, repeat.AcceptedCount);
        Assert.Contains(repeat.Tasks, task => task.Decision == "IVR_IDEMPOTENCY_CONFLICT");
        Assert.Equal(afterFirst, await SeededTaskCountAsync(app));

        // The policy registration is immutable, so the second run registers nothing new either.
        Assert.Equal(0, repeat.AttemptPoliciesRegistered);
    }

    /// <summary>
    /// The acceptance criterion for the runner: a dry run places no call. Counted by the rows a
    /// dispatch would have to create — an attempt row and a raw call event — because those are
    /// what exist if a call happened, whatever the code path claimed.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-DRYRUN-05")]
    public async Task AScenarioDryRunReplaysTheResultAndDispatchesNothing()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;
        (int attemptsBefore, int eventsBefore) = await CallActivityAsync(app);

        using HttpResponseMessage response = await SendAsync(
            app,
            admin,
            $"{Root}/scenarios/SCN-001-confirm:dry-run");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ScenarioDryRunApiResult result = (await response.Content
            .ReadFromJsonAsync<ScenarioDryRunApiResult>())!;

        Assert.Equal("REPLAYED", result.Coverage);
        Assert.Equal("IVR_CONFIRMED", result.ActualResultType);
        Assert.True(result.Matches);

        (int attemptsAfter, int eventsAfter) = await CallActivityAsync(app);
        Assert.Equal(attemptsBefore, attemptsAfter);
        Assert.Equal(eventsBefore, eventsAfter);
    }

    /// <summary>
    /// A scenario the mapper cannot answer for is reported as out of scope, with 200 rather than
    /// an error. It is not a failure — the runner is saying which question it can answer, and
    /// turning that into a red response would train people to ignore red responses.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-DRYRUN-06")]
    public async Task AnOutOfScopeScenarioAnswersWithoutAVerdict()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(
            app,
            admin,
            $"{Root}/scenarios/SCN-008-operational-block-recall:dry-run");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ScenarioDryRunApiResult result = (await response.Content
            .ReadFromJsonAsync<ScenarioDryRunApiResult>())!;

        Assert.Equal("NOT_REPLAYABLE", result.Coverage);
        Assert.Null(result.Matches);
        Assert.NotEmpty(result.Notes);

        using HttpResponseMessage missing = await SendAsync(
            app,
            admin,
            $"{Root}/scenarios/SCN-000-nonexistent:dry-run");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Applying a profile reports which dependencies it actually moved and which it only
    /// declared. Three of the four are declared-only because IVR never probes them, and a screen
    /// that showed all five as applied would be rehearsing a fail-closed path that does not run.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-PROFILE-07")]
    public async Task ApplyingAProfileSeparatesWhatItEnforcesFromWhatItOnlyDeclares()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(
            app,
            admin,
            $"{Root}/integration-profiles/STATUS-order-core-down:apply");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IntegrationProfileApiResult result = (await response.Content
            .ReadFromJsonAsync<IntegrationProfileApiResult>())!;

        Assert.Equal("STATUS-order-core-down", result.ProfileId);
        Assert.Equal(4, result.Effects.Count);
        Assert.Equal(1, result.EnforcedCount);
        Assert.Equal(3, result.DeclaredOnlyCount);

        IntegrationProfileEffectView orderCore = Assert.Single(
            result.Effects,
            effect => effect.Dependency == "ORDER_CORE");
        Assert.False(orderCore.Enforced);
        Assert.Equal("down", orderCore.RequestedState);

        IntegrationProfileEffectView sim = Assert.Single(
            result.Effects,
            effect => effect.Dependency == "SIM_GATEWAY");
        Assert.True(sim.Enforced);

        using HttpResponseMessage missing = await SendAsync(
            app,
            admin,
            $"{Root}/integration-profiles/STATUS-nope:apply");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Two independent halves of the same control. An Operator is refused because the permission
    /// is Admin-only, and the MOCK permission header is refused because the routes are pinned to
    /// the console scheme — the header can mint any permission it likes and still arrive
    /// unauthenticated.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-AUTHZ-08")]
    public async Task AnOperatorAndTheMockHeaderAreBothRefused()
    {
        await using DevToolingApiTestApplication app = await StartAsync();
        // W-0122. The read tier is the closest thing left to the operator this test used to sign
        // in as: a real credential that simply does not reach a write endpoint.
        using HttpResponseMessage forbidden = await SendAsync(
            app,
            AdminScope.Read,
            $"{Root}/seed:load");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var minted = new HttpRequestMessage(HttpMethod.Post, $"{Root}/seed:load");
        minted.Headers.Add("X-Permissions", "IVR_DEV_TOOLING");
        minted.Headers.Add("X-Actor-Id", "admin");
        minted.Headers.Add("X-Correlation-Id", $"corr-dev-{Guid.NewGuid():N}");
        minted.Headers.Add("Idempotency-Key", $"idem-dev-{Guid.NewGuid():N}");
        minted.Content = JsonContent.Create(new { reason = "minted permission" });
        using HttpResponseMessage seam = await app.Client.SendAsync(minted);
        Assert.Equal(HttpStatusCode.Unauthorized, seam.StatusCode);

        Assert.Equal(0, await SeededTaskCountAsync(app));
    }

    /// <summary>
    /// With no seed directory configured the surface refuses rather than searching for one. A dev
    /// tool that guesses at a path is a dev tool that eventually guesses at the wrong one.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-DEV-SEEDPATH-09")]
    public async Task AnUnconfiguredSeedDirectoryIsRefusedRatherThanGuessed()
    {
        await using DevToolingApiTestApplication app = await StartAsync(
            configureSeedDirectory: false);
        const AdminScope admin = AdminScope.Write;

        using HttpResponseMessage response = await SendAsync(app, admin, $"{Root}/seed:load");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0, await SeededTaskCountAsync(app));
    }

    private static IEnumerable<string> Paths() =>
    [
        $"{Root}/seed:load",
        $"{Root}/scenarios/SCN-001-confirm:dry-run",
        $"{Root}/integration-profiles/STATUS-all-up:apply",
    ];

    private async Task<DevToolingApiTestApplication> StartAsync(
        string environmentName = "Testing",
        string executionMode = IvrOptions.MockExecutionMode,
        bool configureSeedDirectory = true)
    {
        await fixture.ResetAsync();
        return await DevToolingApiTestApplication.StartAsync(
            fixture.ConnectionString,
            environmentName,
            executionMode,
            configureSeedDirectory);
    }

    private static async Task<HttpResponseMessage> SendAsync(
        DevToolingApiTestApplication app,
        AdminScope scope,
        string path,
        object? body = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        TestAdminTokens.Authorize(request, scope);
        request.Headers.Add("X-Correlation-Id", $"corr-dev-{Guid.NewGuid():N}");
        request.Headers.Add("Idempotency-Key", $"idem-dev-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(body ?? new { reason = "Acceptance rehearsal" });
        return await app.Client.SendAsync(request);
    }

    private static async Task<int> SeededTaskCountAsync(DevToolingApiTestApplication app)
    {
        IDbContextFactory<IvrDbContext> factory = app.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        return await context.ConfirmationTasks.AsNoTracking().CountAsync();
    }

    private static async Task<(int Attempts, int RawEvents)> CallActivityAsync(
        DevToolingApiTestApplication app)
    {
        IDbContextFactory<IvrDbContext> factory = app.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        return (
            await context.CallAttempts.AsNoTracking().CountAsync(),
            await context.RawCallEvents.AsNoTracking().CountAsync());
    }
}
