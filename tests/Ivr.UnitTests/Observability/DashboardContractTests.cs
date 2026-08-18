using System.Reflection;
using System.Text.RegularExpressions;
using Ivr.Infrastructure.Observability;

namespace Ivr.UnitTests.Observability;

/// <summary>
/// W-0041 / P6-2 section 11. Dashboards and alert rules are the one artifact that can look
/// finished while measuring nothing: a panel naming an instrument nobody records scrapes as a flat
/// line, and a flat line reads as health. These tests walk from the expressions back to the
/// production call sites, so an artifact can never claim more than the instrumentation delivers.
/// </summary>
public sealed class DashboardContractTests
{
    private static readonly Regex IvrToken = new(
        @"\bivr_[a-z0-9_]+\b",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex RecorderCall = new(
        @"IvrTelemetry\.(Record[A-Za-z]+)\s*\(",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    [Fact]
    [Trait("TestId", "UT-DASH-PII-04")]
    public void EveryDashboardAndAlertTokenIsAnEmittedMetricOrAnAllowlistedLabel()
    {
        string root = FindRepositoryRoot();
        HashSet<string> emitted = EmittedMetricNames(root);

        // A production call site must exist for something, or the whole check is vacuous.
        Assert.NotEmpty(emitted);

        // Prometheus labels cannot carry a dot; the OTLP exporter maps `ivr.program` to
        // `ivr_program`, so the allowlist is compared in the shape the artifacts actually use.
        HashSet<string> allowedLabels = TelemetryTags.Allowed
            .Select(tag => tag.Replace('.', '_'))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> traceOnlyLabels = TelemetryTags.TraceOnly
            .Select(tag => tag.Replace('.', '_'))
            .ToHashSet(StringComparer.Ordinal);

        string[] artifacts = Directory.GetFiles(
            Path.Combine(root, "deploy", "observability"),
            "*.*",
            SearchOption.AllDirectories);
        Assert.NotEmpty(artifacts);

        foreach (string artifact in artifacts)
        {
            string text = File.ReadAllText(artifact);
            string name = Path.GetFileName(artifact);

            foreach (Match match in IvrToken.Matches(text))
            {
                string token = match.Value;

                // A trace-only dimension on a metric is a cardinality bomb, not a privacy slip:
                // every request would become its own time series. Checked first so the failure
                // names the real reason rather than "not on the allowlist".
                Assert.False(
                    traceOnlyLabels.Contains(token),
                    $"{name} uses trace-only dimension '{token}' on a metric.");

                bool isMetric = emitted.Contains(StripHistogramSuffix(token));
                bool isLabel = allowedLabels.Contains(token);

                Assert.True(
                    isMetric || isLabel,
                    $"{name} names '{token}', which is neither a metric any production call site "
                    + "records nor an allowlisted tag. A declared-but-never-recorded instrument "
                    + "renders as a flat line, which reads as health.");
            }
        }
    }

    [Fact]
    [Trait("TestId", "UT-DASH-PII-04B")]
    public void EveryPublicRecorderDeclaresWhichInstrumentItFeeds()
    {
        // The map is the hop from call site to metric name. If a recorder is missing from it, the
        // test above silently stops covering that metric — so the map itself is asserted complete.
        string[] recorders = typeof(IvrTelemetry)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.Name)
            .Where(methodName => methodName.StartsWith("Record", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(recorders);
        Assert.All(
            recorders,
            recorder => Assert.True(
                IvrTelemetry.InstrumentsByRecorder.ContainsKey(recorder),
                $"{recorder} feeds an instrument that no artifact check knows about."));
    }

    [Fact]
    [Trait("TestId", "UT-DASH-RUNBOOK-05")]
    public void EveryAlertPointsAtARunbookSectionThatExists()
    {
        // P6-2 section 4 requires a runbook link on every alert. A link is only worth requiring if
        // it resolves: an on-call who clicks one dead link stops clicking them, and after that the
        // annotation is decoration. Both halves are checked -- every alert has a link, and every
        // link lands on a real anchor.
        string root = FindRepositoryRoot();
        string rules = File.ReadAllText(Path.Combine(
            root, "deploy", "observability", "alerts", "ivr-slo.rules.yml"));
        string slo = File.ReadAllText(Path.Combine(root, "docs", "slo.md"));

        int alertCount = Regex.Count(rules, @"^\s*- alert: ", RegexOptions.Multiline);
        MatchCollection links = Regex.Matches(rules, @"runbook_url: (\S+)#(\S+)");

        Assert.NotEqual(0, alertCount);
        Assert.Equal(alertCount, links.Count);

        foreach (Match link in links)
        {
            string target = Path.Combine(root, link.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(target), $"runbook target {link.Groups[1].Value} does not exist.");
            Assert.Contains(
                $"id=\"{link.Groups[2].Value}\"",
                slo,
                StringComparison.Ordinal);
        }
    }

    private static HashSet<string> EmittedMetricNames(string root)
    {
        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (string file in Directory.GetFiles(
            Path.Combine(root, "src"),
            "*.cs",
            SearchOption.AllDirectories))
        {
            // The recorder definitions themselves are not call sites.
            if (Path.GetFileName(file) == "IvrTelemetry.cs")
            {
                continue;
            }

            foreach (Match match in RecorderCall.Matches(File.ReadAllText(file)))
            {
                if (IvrTelemetry.InstrumentsByRecorder.TryGetValue(
                    match.Groups[1].Value,
                    out IReadOnlySet<string>? names))
                {
                    emitted.UnionWith(names);
                }
            }
        }

        return emitted;
    }

    private static string StripHistogramSuffix(string token)
    {
        foreach (string suffix in (string[])["_bucket", "_sum", "_count"])
        {
            if (token.EndsWith(suffix, StringComparison.Ordinal))
            {
                return token[..^suffix.Length];
            }
        }

        return token;
    }

    private static string FindRepositoryRoot()
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
