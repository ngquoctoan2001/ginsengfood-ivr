using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.DevTooling;
using Ivr.Infrastructure.FeatureFlags;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0190. Assertions about the container the service actually builds, rather than about a
/// container a test assembled for itself.
/// <para>
/// Every other feature-flag test substitutes the store — <c>RemoveAll</c>, then a hand-built
/// <c>InMemoryFeatureFlagStore</c> with explicit seeds. That is the right shape for testing the
/// platform's behaviour, and it is exactly why a registration defect could sit in the composition
/// root through 781 green tests: the registration under test was never the registration that ran.
/// </para>
/// <para>
/// So these build the graph the way <c>Program</c> does and ask it the plainest possible
/// questions. There is no HTTP here and no database; the subject is wiring.
/// </para>
/// </summary>
public sealed class CompositionRootTests
{
    /// <summary>
    /// The defect this exists for: <c>TryAddSingleton&lt;InMemoryFeatureFlagStore&gt;()</c> let the
    /// container choose the constructor, the container chose the greediest one it could resolve,
    /// and <c>IEnumerable&lt;FeatureFlagSnapshot&gt;</c> resolves to an empty sequence. The store
    /// held no environments, every read threw, and the platform's fail-closed fallback reported
    /// the provider as unreadable — in MOCK, the mode every non-production deployment runs in.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-COMPROOT-FLAGS-01")]
    public async Task EveryEnvironmentIsReadableFromTheRegistrationTheServiceActuallyUses()
    {
        await using ServiceProvider provider = BuildProvider();
        IFeatureFlags flags = provider.GetRequiredService<IFeatureFlags>();

        foreach (string environment in FeatureFlagEnvironments.All)
        {
            FeatureFlagReadResult result = await flags.GetSnapshotAsync(
                environment,
                forceFresh: true);

            // ProviderReadable is the whole assertion. A false here is the fail-closed fallback,
            // which returns a snapshot that looks entirely plausible — safe defaults, revision 0 —
            // and is why the defect was invisible from the outside.
            Assert.True(
                result.ProviderReadable,
                $"The flag store could not answer for environment '{environment}'.");
            Assert.Equal(environment, result.Snapshot.Environment);
        }
    }

    /// <summary>
    /// The kill switch has to be answerable, not merely defaulted. An unreadable provider also
    /// reports "real calls disabled", so this asserts the answer came from the store rather than
    /// from the fallback that happens to say the same thing.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-COMPROOT-FLAGS-02")]
    public async Task TheKillSwitchAnswerComesFromTheStoreRatherThanTheFallback()
    {
        await using ServiceProvider provider = BuildProvider();
        IFeatureFlags flags = provider.GetRequiredService<IFeatureFlags>();
        IKillSwitch killSwitch = provider.GetRequiredService<IKillSwitch>();

        FeatureFlagReadResult read = await flags.GetSnapshotAsync(
            FeatureFlagEnvironments.Development,
            forceFresh: true);

        Assert.True(read.ProviderReadable);
        Assert.True(read.Snapshot.GlobalDialKillSwitch);
        Assert.False(await killSwitch.RealCallsEnabledAsync(FeatureFlagEnvironments.Development));
    }

    /// <summary>
    /// W-0190. A relative seed directory is resolved against the content root.
    /// <para>
    /// The value committed for development is <c>../../seed</c>. Resolved against the process
    /// working directory it points at whatever directory the runner happened to start in — which
    /// is a different one for <c>dotnet run</c>, <c>dotnet test</c> and the container image, and
    /// is why the developer surface answered "no seed directory is configured" for anyone who had
    /// not read the source to find the setting.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-COMPROOT-SEEDPATH-03")]
    public void ARelativeSeedDirectoryResolvesAgainstTheContentRootNotTheWorkingDirectory()
    {
        string contentRoot = Path.Combine(Path.GetTempPath(), $"ivr-root-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(contentRoot, "fixtures"));
        try
        {
            using ServiceProvider provider = BuildProvider(
                contentRootPath: contentRoot,
                seedDirectory: Path.Combine("..", Path.GetFileName(contentRoot), "fixtures"));

            SeedCatalog catalog = provider.GetRequiredService<SeedCatalog>();

            Assert.True(Path.IsPathRooted(catalog.SeedDirectory));
            Assert.Equal(
                Path.GetFullPath(Path.Combine(contentRoot, "fixtures")),
                catalog.SeedDirectory);
            Assert.True(catalog.IsConfigured);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    /// <summary>
    /// An absolute path is taken as given, and an unset one still disables the surface. The
    /// second half is the safety property: outside development there is no seed directory, and a
    /// resolver that helpfully guessed one would be the defect, not the fix.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("TestId", "IT-COMPROOT-SEEDPATH-04")]
    public void AnUnsetSeedDirectoryStaysUnsetAndKeepsTheSurfaceDisabled(string? configured)
    {
        using ServiceProvider provider = BuildProvider(seedDirectory: configured);

        SeedCatalog catalog = provider.GetRequiredService<SeedCatalog>();

        Assert.Equal(string.Empty, catalog.SeedDirectory);
        Assert.False(catalog.IsConfigured);
    }

    [Fact]
    [Trait("TestId", "IT-COMPROOT-SEEDPATH-05")]
    public void AnAbsoluteSeedDirectoryIsTakenAsGiven()
    {
        string absolute = Path.Combine(Path.GetTempPath(), $"ivr-seed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(absolute);
        try
        {
            using ServiceProvider provider = BuildProvider(seedDirectory: absolute);

            Assert.Equal(
                absolute,
                provider.GetRequiredService<SeedCatalog>().SeedDirectory);
        }
        finally
        {
            Directory.Delete(absolute, recursive: true);
        }
    }

    /// <summary>
    /// Builds the graph the same way <c>Program</c> does for the parts under test: foundation,
    /// feature flags, then the internal/admin API that owns the developer-surface options. The
    /// in-memory data doubles stand in for PostgreSQL because none of these assertions is about
    /// persistence.
    /// </summary>
    private static ServiceProvider BuildProvider(
        string? contentRootPath = null,
        string? seedDirectory = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
            ["SALES_PROVIDER"] = FeatureFlagValues.FakeTargetV1,
            ["SIM_PROVIDER"] = FeatureFlagValues.MockSimProvider,
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["ConnectionStrings:IvrDb"] =
                "Host=localhost;Database=ivr_test;Username=ivr;Password=unused",
            [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                FoundationApiTestApplication.ServiceToken,
            [AdminAccessOptions.ReadTokenConfigurationKey] = TestAdminTokens.Read,
            [AdminAccessOptions.WriteTokenConfigurationKey] = TestAdminTokens.Write,
            [AdminAccessOptions.DangerTokenConfigurationKey] = TestAdminTokens.Danger,
            [$"{DevToolingOptions.SectionName}:{nameof(DevToolingOptions.SeedDirectory)}"] =
                seedDirectory,
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new StubHostEnvironment(
            contentRootPath ?? AppContext.BaseDirectory));
        services.AddIvrFoundation(configuration, useInMemoryTestDoubles: true);
        services.AddIvrFeatureFlags(configuration);
        services.AddIvrInternalAdminApi(configuration);
        return services.BuildServiceProvider();
    }

    private sealed class StubHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";

        public string ApplicationName { get; set; } = "Ivr.Api";

        public string ContentRootPath { get; set; } = contentRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}
