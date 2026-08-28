using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0099 — the SIM channel roster that gives the P2-8 enable/disable
/// operations a console surface (`specs/ui/08` §3).
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class SimChannelReadApiTests(PostgresPersistenceFixture fixture)
{
    private const string Route = "/v1/ivr/order-confirmation/sim-channels";

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    [Trait("TestId", "IT-SIM-READ-01")]
    public async Task RosterIsGatedByQueueViewAndExposesNoMutation()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage allowed = await SendAsync(app, IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        // W-0122. Tiers nest, so a higher credential legitimately reads. The negative that still
        // means something is no credential at all.
        using HttpResponseMessage forbidden = await SendAsync(app, null);
        Assert.Equal(HttpStatusCode.Unauthorized, forbidden.StatusCode);
        using JsonDocument envelope = JsonDocument.Parse(
            await forbidden.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.Unauthenticated,
            envelope.RootElement.GetProperty("error").GetProperty("code").GetString());

        // The roster itself is a read. Enabling and disabling stay on their own
        // permission-gated POST routes.
        using HttpRequestMessage write = new(HttpMethod.Post, Route);
        TestAdminTokens.Authorize(write, AdminScope.Read, "operator-sim");
        write.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
        write.Content = JsonContent.Create(new { reason = "attempted write" });
        using HttpResponseMessage rejected = await app.Client.SendAsync(write);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, rejected.StatusCode);
    }

    [Fact]
    [Trait("TestId", "IT-SIM-READ-02")]
    public async Task RosterReportsOperatorStateAndNeverThePhoneIdentity()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(app, IvrPermissions.QueueView);
        string body = await response.Content.ReadAsStringAsync();
        IvrServer.IvrSimChannelList roster =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrSimChannelList>())!;

        Assert.Equal(3, roster.Channels.Count);
        Assert.False(roster.Real_customer_call_allowed);

        IvrServer.IvrSimChannel idle = roster.Channels.Single(c => c.Sim_channel_id == "SIM-01");
        Assert.True(idle.Enabled);
        Assert.False(idle.Busy);
        Assert.False(idle.Quarantined);

        // Busy is what tells an operator a disable will not take effect at once.
        IvrServer.IvrSimChannel busy = roster.Channels.Single(c => c.Sim_channel_id == "SIM-02");
        Assert.True(busy.Busy);
        Assert.Equal("JOB-SIM-BUSY", busy.Active_call_job_id);

        IvrServer.IvrSimChannel disabled = roster.Channels.Single(c => c.Sim_channel_id == "SIM-03");
        Assert.False(disabled.Enabled);
        Assert.True(disabled.Quarantined);
        Assert.Equal(4, disabled.Fail_count);
        Assert.Equal("health probe failed", disabled.Disabled_reason);

        // The phone identity and the scheduler's lease mechanics stay server-side.
        foreach (string forbidden in new[]
        {
            "sim_number_ref",
            "sim-ref-secret",
            "lease_token",
            "lease_fencing_generation",
            "leased_by_worker_id",
        })
        {
            Assert.DoesNotContain(forbidden, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static Task<HttpResponseMessage> SendAsync(
        InternalAdminApiTestApplication app,
        string? permission)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, Route);
        if (permission is not null)
        {
            TestAdminTokens.AuthorizeForPermission(request, permission, "operator-sim");
        }
        request.Headers.Add(
            "X-Correlation-Id",
            string.Concat("corr-", Guid.NewGuid().ToString("N")));
        return app.Client.SendAsync(request);
    }

    private async Task SeedAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        context.SimChannels.AddRange(
            new SimChannelEntity
            {
                SimChannelId = "SIM-01",
                SimNumberRef = "sim-ref-secret-01",
                Enabled = true,
                Status = "IDLE",
                AdapterMode = "MOCK",
                ExecutionMode = IvrOptions.MockExecutionMode,
                ProviderName = "MOCK",
                LastHealthCheckAt = Now.AddMinutes(-2),
            },
            new SimChannelEntity
            {
                SimChannelId = "SIM-02",
                SimNumberRef = "sim-ref-secret-02",
                Enabled = true,
                Status = "ACTIVE_CALL",
                AdapterMode = "MOCK",
                ExecutionMode = IvrOptions.MockExecutionMode,
                ProviderName = "MOCK",
                ActiveCallJobId = "JOB-SIM-BUSY",
                LastHealthCheckAt = Now.AddMinutes(-1),
                LeaseToken = Guid.NewGuid(),
                LeaseFencingGeneration = 7,
                LeasedByWorkerId = "worker-01",
            },
            new SimChannelEntity
            {
                SimChannelId = "SIM-03",
                SimNumberRef = "sim-ref-secret-03",
                Enabled = false,
                Status = "HEALTH_FAILED",
                AdapterMode = "MOCK",
                ExecutionMode = IvrOptions.MockExecutionMode,
                ProviderName = "MOCK",
                FailCount = 4,
                DisabledReason = "health probe failed",
                QuarantineUntil = Now.AddHours(1),
                CooldownUntil = Now.AddMinutes(10),
                LastHealthCheckAt = Now.AddMinutes(-30),
            });

        await context.SaveChangesAsync();
    }
}
