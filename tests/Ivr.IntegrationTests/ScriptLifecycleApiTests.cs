using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Accounts;
using Ivr.Api.Admin;
using Ivr.Domain.Accounts;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// Script lifecycle over HTTP (W-0109).
/// <para>
/// The point of these is that a Privacy/Legal signature now has a path that carries an audit
/// record, a reason, and "not the same person twice". The previous path was editing rows by
/// hand, which carries none of them — so the assertions here are about the controls, not about
/// whether the transition happens.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ScriptLifecycleApiTests(PostgresPersistenceFixture fixture)
{
    private const string Password = ConsoleAccountTestAccounts.Password;

    /// <summary>
    /// Three Admins, and the count is the point. The creator cannot approve, and Content and
    /// Privacy/Legal cannot be the same account, so a production sign-off needs three distinct
    /// people. A deployment with fewer simply cannot reach production approval — which is the
    /// intended fail-closed answer, and the first draft of this test got it wrong by trying to
    /// let the author sign one of the two halves.
    /// </summary>
    private static readonly ConsoleAccountSeed[] ThreeAdmins =
    [
        new("admin", "Quản trị hệ thống", ConsoleAccountRoles.Admin, true),
        new("admin2", "Quản trị thứ hai", ConsoleAccountRoles.Admin, false),
        new("admin3", "Quản trị thứ ba", ConsoleAccountRoles.Admin, false),
        new("ngquoctoan2001", "Nguyễn Quốc Toàn", ConsoleAccountRoles.Operator, false),
    ];

    [Fact]
    [Trait("TestId", "IT-SCRIPT-LIFECYCLE-01")]
    public async Task DraftMovesThroughReviewAndApprovalAndBecomesSpeakable()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);

        await harness.CreateDraftAsync(harness.Author, "v9-console", HttpStatusCode.OK);
        await harness.PostAsync(harness.Author, "v9-console:submit", new { reason = "Ready for review" });

        using HttpResponseMessage approved = await harness.PostAsync(
            harness.Approver,
            "v9-console:approve",
            new { approval_type = "MOCK_TEST", reason = "Synthetic MOCK approval" });
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);

        ScriptVersionApiResult version = await harness.GetAsync(harness.Author, "v9-console");
        Assert.Equal("APPROVED", version.Status);
        Assert.Equal(["MOCK"], version.ApprovedForModes);
        Assert.Single(version.Approvals);
        Assert.Equal("admin2", version.Approvals[0].ActorId);

        // SCREAMING_SNAKE, matching the keys in admin-ui/src/i18n/enums.vi.json. Enum.ToString()
        // would give "Approved"/"Mock"/"MockTest" here and the console would render a warning
        // badge next to an unrecognised code instead of a label.
        Assert.Equal("MOCK_TEST", version.Approvals[0].ApprovalType);

        // Named, not left for the reader to infer from a short mode list.
        Assert.Equal(
            "Production needs both a Content and a Privacy/Legal approval.",
            version.ProductionBlockedReason);
    }

    /// <summary>
    /// Both four-eyes rules, and both answer 403 rather than 409: the caller holds the
    /// permission and is simply the wrong person, which no amount of retrying fixes.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCRIPT-FOUREYES-02")]
    public async Task TheCreatorCannotApproveAndOneAccountCannotHoldBothProductionApprovals()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);
        await harness.CreateDraftAsync(harness.Author, "v9-eyes", HttpStatusCode.OK);
        await harness.PostAsync(harness.Author, "v9-eyes:submit", new { reason = "Ready for review" });

        using HttpResponseMessage selfApproval = await harness.PostAsync(
            harness.Author,
            "v9-eyes:approve",
            new { approval_type = "MOCK_TEST", reason = "Creator approving own draft" });
        Assert.Equal(HttpStatusCode.Forbidden, selfApproval.StatusCode);

        await harness.PostAsync(
            harness.Approver,
            "v9-eyes:approve",
            new { approval_type = "CONTENT", reason = "Content approved" });

        using HttpResponseMessage sameActor = await harness.PostAsync(
            harness.Approver,
            "v9-eyes:approve",
            new { approval_type = "PRIVACY_LEGAL", reason = "Same person signing both halves" });
        Assert.Equal(HttpStatusCode.Forbidden, sameActor.StatusCode);

        ScriptVersionApiResult version = await harness.GetAsync(harness.Author, "v9-eyes");
        Assert.Single(version.Approvals);
        Assert.DoesNotContain("PRODUCTION_REAL", version.ApprovedForModes);
        Assert.Equal(
            "Production is waiting on the Privacy/Legal approval.",
            version.ProductionBlockedReason);
    }

    /// <summary>
    /// Two distinct approvers are necessary but not sufficient: the speech field whitelist is
    /// still unsigned, so <c>OD-V1-15</c> keeps production closed. Approving through the console
    /// must not be able to talk its way past that.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCRIPT-PRODGATE-03")]
    public async Task BothProductionApprovalsStillLeaveProductionBlockedByTheUnsignedWhitelist()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);
        await harness.CreateDraftAsync(harness.Author, "v9-prod", HttpStatusCode.OK);
        await harness.PostAsync(harness.Author, "v9-prod:submit", new { reason = "Ready for review" });
        await harness.PostAsync(
            harness.Approver,
            "v9-prod:approve",
            new { approval_type = "CONTENT", reason = "Content approved" });
        await harness.PostAsync(
            harness.SecondApprover,
            "v9-prod:approve",
            new { approval_type = "PRIVACY_LEGAL", reason = "Privacy and Legal approved" });

        ScriptVersionApiResult version = await harness.GetAsync(harness.Author, "v9-prod");

        Assert.Equal(2, version.Approvals.Count);
        Assert.True(version.UsesProductionDecisionFields);
        Assert.DoesNotContain("PRODUCTION_REAL", version.ApprovedForModes);
        Assert.Equal(
            "The speech field whitelist is unsigned (OD-V1-15), so production stays blocked.",
            version.ProductionBlockedReason);
    }

    [Fact]
    [Trait("TestId", "IT-SCRIPT-RETIRED-04")]
    public async Task ARetiredVersionFailsClosedInEveryMode()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);
        await harness.CreateDraftAsync(harness.Author, "v9-retire", HttpStatusCode.OK);
        await harness.PostAsync(harness.Author, "v9-retire:submit", new { reason = "Ready for review" });
        await harness.PostAsync(
            harness.Approver,
            "v9-retire:approve",
            new { approval_type = "MOCK_TEST", reason = "Synthetic MOCK approval" });

        using HttpResponseMessage retired = await harness.PostAsync(
            harness.Author,
            "v9-retire:retire",
            new { reason = "Superseded by a newer wording" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        ScriptVersionApiResult version = await harness.GetAsync(harness.Author, "v9-retire");
        Assert.Equal("RETIRED", version.Status);
        Assert.Empty(version.ApprovedForModes);
        Assert.Equal(
            "The version is retired and fails closed in every mode.",
            version.ProductionBlockedReason);

        // Retired stays retired: a second approval must not resurrect it.
        using HttpResponseMessage reapproval = await harness.PostAsync(
            harness.Approver,
            "v9-retire:approve",
            new { approval_type = "LAB", reason = "Trying to revive a retired version" });
        Assert.Equal(HttpStatusCode.Conflict, reapproval.StatusCode);
    }

    /// <summary>
    /// Every transition leaves an audit row carrying both the state it came from and the state
    /// it went to — and none of them carries the script text, which is what a customer hears
    /// and therefore not something an audit table should accumulate.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCRIPT-AUDIT-05")]
    public async Task EveryTransitionIsAuditedWithBeforeAndAfterAndWithoutScriptText()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);
        await harness.CreateDraftAsync(harness.Author, "v9-audit", HttpStatusCode.OK);
        await harness.PostAsync(harness.Author, "v9-audit:submit", new { reason = "Ready for review" });
        await harness.PostAsync(
            harness.Approver,
            "v9-audit:approve",
            new { approval_type = "MOCK_TEST", reason = "Synthetic MOCK approval" });

        // Read from the table, not from an in-memory logger: this harness runs on PostgreSQL,
        // and the row that matters is the one an auditor would actually be shown.
        await using IvrDbContext dbContext = await fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>()
            .CreateDbContextAsync();
        List<AuditLogEntity> entries = await dbContext.AuditLog
            .AsNoTracking()
            .Where(entry => entry.TargetId == "SCRIPT-ORDER-CONFIRM:v9-audit")
            .ToListAsync();
        Assert.Equal(3, entries.Count);

        AuditLogEntity submitted = entries.Single(entry =>
            entry.Action == "ADMIN_SCRIPT_REVIEW_SUBMITTED");
        using JsonDocument payload = JsonDocument.Parse(submitted.DataJson);
        Assert.Equal("DRAFT", payload.RootElement.GetProperty("previous_status").GetString());
        Assert.Equal("IN_REVIEW", payload.RootElement.GetProperty("status").GetString());

        Assert.All(entries, entry =>
        {
            Assert.DoesNotContain("Xin chào", entry.DataJson, StringComparison.Ordinal);
            Assert.DoesNotContain("Bấm phím", entry.DataJson, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// Operator holds none of the seven script permissions, and the console must answer 403
    /// rather than hiding the buttons and hoping.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-SCRIPT-OPERATOR-06")]
    public async Task OperatorIsRefusedEveryScriptTransition()
    {
        await using ConsoleAccountApiTestApplication app = await StartAppAsync();
        ScriptLifecycleHarness harness = await HarnessAsync(app);
        await harness.CreateDraftAsync(harness.Author, "v9-operator", HttpStatusCode.OK);

        await harness.CreateDraftAsync(harness.Operator, "v9-operator-denied", HttpStatusCode.Forbidden);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await harness.PostAsync(
                harness.Operator,
                "v9-operator:submit",
                new { reason = "Operator submitting" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await harness.PostAsync(
                harness.Operator,
                "v9-operator:approve",
                new { approval_type = "MOCK_TEST", reason = "Operator approving" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await harness.PostAsync(
                harness.Operator,
                "v9-operator:retire",
                new { reason = "Operator retiring" })).StatusCode);
    }

    private async Task<ConsoleAccountApiTestApplication> StartAppAsync()
    {
        await fixture.ResetAsync();
        await ConsoleAccountTestAccounts.SeedAsync(
            fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>(),
            ThreeAdmins);
        return await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
    }

    /// <summary>
    /// The harness borrows the application rather than owning it, so each test keeps the
    /// `await using` and there is exactly one place that decides when the server stops.
    /// </summary>
    private static async Task<ScriptLifecycleHarness> HarnessAsync(
        ConsoleAccountApiTestApplication app) => new(
        app,
        await ConsoleAccountTestAccounts.SignInAsync(
            app.Client, "admin", Password, HttpStatusCode.OK),
        await ConsoleAccountTestAccounts.SignInAsync(
            app.Client, "admin2", Password, HttpStatusCode.OK),
        await ConsoleAccountTestAccounts.SignInAsync(
            app.Client, "admin3", Password, HttpStatusCode.OK),
        await ConsoleAccountTestAccounts.SignInAsync(
            app.Client, "ngquoctoan2001", Password, HttpStatusCode.OK));

    private sealed class ScriptLifecycleHarness(
        ConsoleAccountApiTestApplication app,
        ConsoleSignInApiResult author,
        ConsoleSignInApiResult approver,
        ConsoleSignInApiResult secondApprover,
        ConsoleSignInApiResult @operator)
    {
        private const string Root = "/v1/ivr/order-confirmation/scripts";

        public ConsoleAccountApiTestApplication App { get; } = app;

        public ConsoleSignInApiResult Author { get; } = author;

        public ConsoleSignInApiResult Approver { get; } = approver;

        public ConsoleSignInApiResult SecondApprover { get; } = secondApprover;

        public ConsoleSignInApiResult Operator { get; } = @operator;

        public async Task CreateDraftAsync(
            ConsoleSignInApiResult session,
            string version,
            HttpStatusCode expected)
        {
            using HttpResponseMessage response = await SendAsync(
                session,
                HttpMethod.Post,
                $"{Root}/",
                new
                {
                    template_id = TargetV1SpeechPolicy.MockTemplateId,
                    version,
                    template_text = TargetV1SpeechPolicy.CanonicalVietnameseTemplate,
                    reason = "Console-created draft",
                });
            Assert.Equal(expected, response.StatusCode);
        }

        public Task<HttpResponseMessage> PostAsync(
            ConsoleSignInApiResult session,
            string suffix,
            object body) =>
            SendAsync(
                session,
                HttpMethod.Post,
                $"{Root}/{TargetV1SpeechPolicy.MockTemplateId}/{suffix}",
                body);

        public async Task<ScriptVersionApiResult> GetAsync(
            ConsoleSignInApiResult session,
            string version)
        {
            using HttpResponseMessage response = await SendAsync(
                session,
                HttpMethod.Get,
                $"{Root}/{TargetV1SpeechPolicy.MockTemplateId}/{version}",
                null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<ScriptVersionApiResult>())!;
        }

        private async Task<HttpResponseMessage> SendAsync(
            ConsoleSignInApiResult session,
            HttpMethod method,
            string path,
            object? body)
        {
            using var request = new HttpRequestMessage(method, path);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", session.AccessToken);
            request.Headers.Add("X-Correlation-Id", $"corr-script-{Guid.NewGuid():N}");
            request.Headers.Add("X-Actor-Id", session.Session.Account.Username);
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            return await App.Client.SendAsync(request);
        }
    }
}
