using System.Net;
using System.Net.Http.Json;
using Ivr.Api.Admin;
using Ivr.Api.Auth;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0307 / B9 — the audit-evidence read, end to end against a real database.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AuditEvidenceApiTests(PostgresPersistenceFixture fixture)
{
    private const string Route = "/v1/ivr/order-confirmation/audit-evidence";
    private const string TargetType = "confirmation-task";
    private const string TargetId = "TASK-AUDITEV-1";
    private const string Reason = "incident review 2026-09-16";

    private static readonly DateTimeOffset Base =
        new(2026, 9, 16, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-AUDITEV-10")]
    public async Task TheTrailComesBackNewestFirstForTheRequestedObjectAndNothingElse()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 3, rowsForOtherTarget: 2);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(app, Query());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuditEvidenceApiResult result =
            (await response.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;

        Assert.Equal(3, result.Rows.Count);
        Assert.All(result.Rows, row => Assert.Equal(TargetId, row.TargetId));

        // Newest first, and asserted as an ordering rather than by index equality so the test
        // fails on a wrong sort rather than on a coincidental arrangement.
        IReadOnlyList<DateTimeOffset> created = result.Rows.Select(row => row.CreatedAt).ToList();
        Assert.Equal(created.OrderByDescending(value => value).ToList(), created);

        // The two rows written against a different object are not in the answer. This is the
        // assertion that the filter is a filter and not decoration.
        Assert.DoesNotContain(result.Rows, row => row.TargetId != TargetId);
        Assert.False(result.Truncated);
    }

    /// <summary>
    /// The read records itself. Of every admin access this is the one most worth a row: pulling
    /// the history of an object is what someone reconstructing — or covering — a sequence of
    /// actions does.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-AUDITEV-11")]
    public async Task ReadingTheTrailWritesAnAuditRowAndHandsBackItsId()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 1, rowsForOtherTarget: 0);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(app, Query());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuditEvidenceApiResult result =
            (await response.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;

        Assert.True(Guid.TryParse(result.AccessAuditId, out Guid accessAuditId));

        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        AuditLogEntity access = await dbContext.AuditLog
            .AsNoTracking()
            .SingleAsync(row => row.AuditId == accessAuditId);

        Assert.Equal("IVR_AUDIT_EVIDENCE_READ", access.Action);
        Assert.Equal(Reason, access.Reason);
        Assert.Contains(TargetId, access.DataJson, StringComparison.Ordinal);

        // The id in the response resolves to a row that exists. Returning an id the caller cannot
        // find would be worse than returning none, because it reads as proof.
        Assert.Equal($"{TargetType}:{TargetId}", access.TargetType + ":" + access.TargetId);
    }

    /// <summary>
    /// truncated is measured, not inferred. The service asks for one row more than the limit
    /// precisely so that a trail ending at the limit is distinguishable from one cut at it.
    /// </summary>
    /// <remarks>
    /// Exactly one read per assertion, because each read appends its own access row to this very
    /// trail (see <c>ReadingAnObjectsHistoryBecomesPartOfThatHistory</c>). A second read inside one
    /// test would be measuring a store the first read had already changed — which is how the first
    /// version of this test failed, and the failure was the endpoint telling the truth.
    /// </remarks>
    [Fact]
    [Trait("TestId", "IT-AUDITEV-12")]
    public async Task TruncatedIsTrueWhenRowsWereLeftBehind()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 3, rowsForOtherTarget: 0);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage cut = await SendAsync(app, Query(limit: 2));
        AuditEvidenceApiResult cutResult =
            (await cut.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;
        Assert.Equal(2, cutResult.Rows.Count);
        Assert.True(cutResult.Truncated);
    }

    /// <summary>
    /// The boundary case the `rows.Count == limit` inference gets wrong: three rows, limit three,
    /// nothing left behind. This is why <c>truncated</c> is a field and not something the caller
    /// works out.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-AUDITEV-16")]
    public async Task TruncatedIsFalseWhenTheCountLandsExactlyOnTheLimit()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 3, rowsForOtherTarget: 0);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage exact = await SendAsync(app, Query(limit: 3));
        AuditEvidenceApiResult exactResult =
            (await exact.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;
        Assert.Equal(3, exactResult.Rows.Count);
        Assert.False(exactResult.Truncated);
    }

    /// <summary>
    /// Reading an object's history becomes part of that history. Surprising, deliberate, and
    /// pinned here so nobody later removes it as a bug.
    /// </summary>
    /// <remarks>
    /// Who looked at a record is part of what happened to that record. An auditor who can see
    /// every mutation but none of the lookups is missing the half of the trail most likely to
    /// matter when the question is "who knew".
    /// </remarks>
    [Fact]
    [Trait("TestId", "IT-AUDITEV-15")]
    public async Task ReadingAnObjectsHistoryBecomesPartOfThatHistory()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 2, rowsForOtherTarget: 0);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage first = await SendAsync(app, Query());
        AuditEvidenceApiResult firstResult =
            (await first.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;
        Assert.Equal(2, firstResult.Rows.Count);

        using HttpResponseMessage second = await SendAsync(app, Query());
        AuditEvidenceApiResult secondResult =
            (await second.Content.ReadFromJsonAsync<AuditEvidenceApiResult>())!;

        // Exactly one more: the first read's own access row, and nothing else.
        Assert.Equal(3, secondResult.Rows.Count);
        Assert.Equal(
            firstResult.AccessAuditId,
            Assert.Single(
                secondResult.Rows,
                row => row.Action == "IVR_AUDIT_EVIDENCE_READ").AuditId);
    }

    [Fact]
    [Trait("TestId", "IT-AUDITEV-13")]
    public async Task TheRouteIsGatedByQueueViewAndRefusesAnUnauthorizedCaller()
    {
        await fixture.ResetAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(app, Query(), permission: null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A request with no selector is refused at the API boundary, not defaulted to every object in
    /// the system. Proved here as well as in the service unit tests because the route's own
    /// binding is what a caller actually meets.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-AUDITEV-14")]
    public async Task AQueryWithNoSelectorIsRefusedAtTheRoute()
    {
        await fixture.ResetAsync();
        await SeedAsync(rowsForTarget: 1, rowsForOtherTarget: 1);
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app, $"?reason={Uri.EscapeDataString(Reason)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string Query(int? limit = null)
    {
        string query = $"?target_type={Uri.EscapeDataString(TargetType)}"
            + $"&target_id={Uri.EscapeDataString(TargetId)}"
            + $"&reason={Uri.EscapeDataString(Reason)}";
        return limit is null ? query : query + $"&limit={limit.Value}";
    }

    private async Task SeedAsync(int rowsForTarget, int rowsForOtherTarget)
    {
        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        for (int index = 0; index < rowsForTarget; index += 1)
        {
            dbContext.AuditLog.Add(Row(TargetId, index));
        }

        for (int index = 0; index < rowsForOtherTarget; index += 1)
        {
            dbContext.AuditLog.Add(Row("TASK-AUDITEV-OTHER", index));
        }

        await dbContext.SaveChangesAsync();
    }

    private static AuditLogEntity Row(string targetId, int index) => new()
    {
        AuditId = Guid.NewGuid(),
        ActorId = "order-core",
        ActorType = "service",
        Action = "TASK_INTAKE_ACCEPTED",
        TargetType = TargetType,
        TargetId = targetId,
        Reason = "ACCEPTED",
        CorrelationId = $"corr-seed-{index}",
        DataJson = $"{{\"seq\":{index}}}",
        CreatedAt = Base.AddMinutes(index),
    };

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private static Task<HttpResponseMessage> SendAsync(
        InternalAdminApiTestApplication app,
        string query,
        string? permission = IvrPermissions.QueueView)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, Route + query);
        if (permission is not null)
        {
            TestAdminTokens.AuthorizeForPermission(request, permission, "operator-read");
        }
        else
        {
            request.Headers.Add("X-Actor-Id", "operator-read");
        }

        request.Headers.Add(
            "X-Correlation-Id",
            string.Concat("corr-", Guid.NewGuid().ToString("N")));
        return app.Client.SendAsync(request);
    }

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
}
