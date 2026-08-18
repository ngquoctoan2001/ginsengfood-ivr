using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Ivr.IntegrationTests.Observability;

[CollectionDefinition(Name)]
public sealed class PromtoolTestGroup : ICollectionFixture<PromtoolFixture>
{
    public const string Name = "P6-2 Prometheus rule evaluation";
}

/// <summary>
/// W-0041 / P6-2 section 8. Alert rules are evaluated by the real Prometheus rule engine rather
/// than inspected as text: a rule that parses, names live metrics and still never fires is exactly
/// the failure these tests exist to catch, and no amount of reading the YAML finds it.
/// </summary>
public sealed class PromtoolFixture : IAsyncLifetime
{
    private const string AlertsDirectory = "deploy/observability/alerts";

    private readonly IContainer container = BuildContainer();

    public Task InitializeAsync() => container.StartAsync();

    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    public async Task<(long ExitCode, string Output)> RunAsync(string testFile)
    {
        ExecResult result = await container.ExecAsync(
            ["promtool", "test", "rules", $"/work/{testFile}"]);
        return (result.ExitCode ?? -1, string.Concat(result.Stdout, result.Stderr));
    }

    private static IContainer BuildContainer()
    {
        string alerts = Path.Combine(FindRepositoryRoot(), AlertsDirectory);
        // The image's entrypoint is the server; the rule evaluator is a second binary in the
        // same image, so the container is kept alive and promtool is run inside it.
        ContainerBuilder builder = new ContainerBuilder("prom/prometheus:v2.54.1")
            .WithEntrypoint("sleep")
            .WithCommand("600");

        // Mapped file by file rather than as a directory: a directory mapping lands the folder
        // itself under the target, and the rule files reference each other by bare filename.
        foreach (string file in Directory.GetFiles(alerts, "*.yml"))
        {
            builder = builder.WithResourceMapping(new FileInfo(file), "/work");
        }

        return builder.Build();
    }

    internal static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The repository root was not found.");
    }
}

[Collection(PromtoolTestGroup.Name)]
public sealed class SloAlertRuleTests(PromtoolFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-SLO-ALERT-01")]
    public async Task AFailClosedSpikePagesAndAnOrdinaryTrickleDoesNot()
    {
        // DO-06. Holding when a dependency cannot be proven healthy is the designed behaviour, so
        // the paired silent case matters as much as the firing one: a rule that pages on every
        // hold would be turned off within a week, and then it catches nothing at all.
        await AssertRuleTestPassesAsync("ivr-slo.failclosed.test.yml");
    }

    [Fact]
    [Trait("TestId", "IT-SLO-SIM-02")]
    public async Task ThreeChannelAutoDisablesInTenMinutesPagesAndOneDoesNot()
    {
        // DT-04 fixes the threshold at three failures in ten minutes.
        await AssertRuleTestPassesAsync("ivr-slo.channel.test.yml");
    }

    [Fact]
    [Trait("TestId", "IT-SLO-LAT-03")]
    public async Task CallbackP95AboveFiveSecondsPagesAndBelowItDoesNot()
    {
        // D-04 puts Core revalidate at 3-5s; the objective takes the upper bound.
        await AssertRuleTestPassesAsync("ivr-slo.latency.test.yml");
    }

    private async Task AssertRuleTestPassesAsync(string testFile)
    {
        (long exitCode, string output) = await fixture.RunAsync(testFile);
        Assert.True(
            exitCode == 0,
            $"promtool rejected {testFile}:{Environment.NewLine}{output}");
        Assert.Contains("SUCCESS", output, StringComparison.Ordinal);
    }
}
