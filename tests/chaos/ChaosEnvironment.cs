using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Ivr.Api.Application;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Ivr.ChaosTests;

[CollectionDefinition(Name)]
public sealed class ChaosTestGroup : ICollectionFixture<ChaosEnvironment>
{
    public const string Name = "P6-3 chaos scenarios";
}

/// <summary>
/// W-0042 / P6-3. The database is reached through a Toxiproxy hop, so a fault here is a real
/// network fault: connections are cut and refused by something between the process and Postgres,
/// exactly as a partition would. Stopping the container would also work, but it tests a different
/// thing — a server that went away cleanly, rather than a link that stopped carrying traffic.
/// <para>
/// Blast radius is the test network and nothing else (P6-3 section 4): both containers are
/// created for this run, torn down with it, and reachable only from it. No fault can escape to a
/// shared environment because there is no route to one.
/// </para>
/// </summary>
public sealed class ChaosEnvironment : IAsyncLifetime, IDisposable
{
    private const string DatabaseAlias = "chaos-db";
    private const string ProxyName = "chaos-db";
    private const int ProxyApiPort = 8474;
    private const int ProxyListenPort = 15432;

    private readonly INetwork network = new NetworkBuilder().Build();
    private readonly PostgreSqlContainer database;
    private readonly IContainer proxy;
    private HttpClient api = null!;

    public ChaosEnvironment()
    {
        network = new NetworkBuilder().Build();
        database = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("ivr_chaos")
            .WithUsername("ivr_test")
            .WithPassword("ivr-test-password")
            .WithNetwork(network)
            .WithNetworkAliases(DatabaseAlias)
            .Build();
        proxy = new ContainerBuilder("ghcr.io/shopify/toxiproxy:2.9.0")
            .WithNetwork(network)
            .WithPortBinding(ProxyApiPort, true)
            .WithPortBinding(ProxyListenPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                request => request.ForPort(ProxyApiPort).ForPath("/version")))
            .Build();
    }

    public ServiceProvider Services { get; private set; } = null!;

    /// <summary>Connection string that routes through the fault-injection hop.</summary>
    public string ConnectionString =>
        $"Host=127.0.0.1;Port={proxy.GetMappedPublicPort(ProxyListenPort)};"
        + "Database=ivr_chaos;Username=ivr_test;Password=ivr-test-password;"
        // Short timeouts so a cut link surfaces as a failure inside the test rather than as a hang.
        + "Timeout=5;Command Timeout=10;Maximum Pool Size=8";

    public async Task InitializeAsync()
    {
        await network.CreateAsync();
        await database.StartAsync();
        await proxy.StartAsync();

        api = new HttpClient
        {
            BaseAddress = new Uri($"http://127.0.0.1:{proxy.GetMappedPublicPort(ProxyApiPort)}"),
            Timeout = TimeSpan.FromSeconds(20),
        };
        HttpResponseMessage created = await api.PostAsJsonAsync("/proxies", new
        {
            name = ProxyName,
            listen = $"0.0.0.0:{ProxyListenPort}",
            upstream = $"{DatabaseAlias}:5432",
            enabled = true,
        });
        created.EnsureSuccessStatusCode();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.LabRealSimExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "VENDOR",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] = ConnectionString,
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        services.AddIvrEligibility(configuration);
        services.AddIvrFeatureFlags(configuration);
        Services = services.BuildServiceProvider(validateScopes: true);

        await MigrateAsync();
    }

    public void Dispose() => api?.Dispose();

    public async Task DisposeAsync()
    {
        Dispose();
        await Services.DisposeAsync();
        await proxy.DisposeAsync();
        await database.DisposeAsync();
        await network.DeleteAsync();
    }

    public IDbContextFactory<IvrDbContext> DbContextFactory =>
        Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    public async Task MigrateAsync()
    {
        await using IvrDbContext context = await DbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    /// <summary>Cuts the link. Open connections are closed and new ones are refused.</summary>
    public async Task CutDatabaseLinkAsync()
    {
        HttpResponseMessage response = await api.PostAsJsonAsync(
            $"/proxies/{ProxyName}",
            new { enabled = false });
        response.EnsureSuccessStatusCode();

        // Npgsql keeps a pool of physical connections; without clearing it the next command is
        // handed a socket that was already dead, which fails in a different way than a partition
        // and would let the scenario pass for the wrong reason.
        Npgsql.NpgsqlConnection.ClearAllPools();
    }

    /// <summary>Restores the link.</summary>
    public async Task RestoreDatabaseLinkAsync()
    {
        HttpResponseMessage response = await api.PostAsJsonAsync(
            $"/proxies/{ProxyName}",
            new { enabled = true });
        response.EnsureSuccessStatusCode();
    }

    /// <summary>Adds latency to every packet, for the "slow, not down" half of a fault.</summary>
    public async Task SlowDatabaseLinkAsync(int milliseconds)
    {
        HttpResponseMessage response = await api.PostAsJsonAsync(
            $"/proxies/{ProxyName}/toxics",
            new
            {
                name = "latency",
                type = "latency",
                stream = "downstream",
                attributes = new { latency = milliseconds, jitter = 0 },
            });
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearLatencyAsync()
    {
        using HttpResponseMessage response = await api.DeleteAsync(
            $"/proxies/{ProxyName}/toxics/latency");
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}
