using System.Reflection;
using System.Text.Json;
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

    private const string MissedDeadlineMetric = "ivr_missed_deadline_total";

    // W-0372 / K-64. What UT-DASH-REASON-06 reads. The first three read the store the way
    // capacity-selftest.mjs CAP-ALERT-04 does: how the sweep picks between a capacity miss and a
    // window that ran out, the tuple each closed job is counted with, and the constants that tuple
    // may name. The rest read the rules and the panel.
    private static readonly Regex SweepBranch = new(
        @"string reasonCode = capacityMiss\s*\?\s*""([A-Z_]+)""\s*:\s*""([A-Z_]+)"";",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex SweepCounted = new(
        @"closed\.Add\(\(([^;]*)\)\);",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex StoreConstant = new(
        @"public const string (\w+) = ""([A-Z_]+)"";",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex AlertRule = new(
        @"- alert: (?<name>\w+)\s*\n\s*expr: (?<expr>[\s\S]*?)\n\s*for: [\s\S]*?runbook_url: (?<runbook>\S+)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex CounterRead = new(
        @"\bivr_missed_deadline_total\b(?:\{(?<matchers>[^}]*)\})?",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex OneReason = new(
        @"^\s*ivr_reason_code\s*=\s*""[A-Z_]+""\s*$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex SplitByReason = new(
        @"\bby\s*\([^)]*\bivr_reason_code\b[^)]*\)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex SentenceEnd = new(
        @"(?<=[.!?])\s+",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    private static readonly Regex ModelClaim = new(
        @"\bmodel",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
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

        // W-0046 / P7-4. deploy/rollouts is covered by the same rule, and it needs it more: an SLO
        // analysis querying a metric nobody emits returns "no data", and most analysis engines read
        // no-data as "not failing" -- so the canary would promote itself on silence.
        string[] artifacts =
        [
            .. Directory.GetFiles(
                Path.Combine(root, "deploy", "observability"),
                "*.*",
                SearchOption.AllDirectories),
            .. Directory.GetFiles(
                Path.Combine(root, "deploy", "rollouts"),
                "*.*",
                SearchOption.AllDirectories),
        ];
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

    /// <summary>
    /// W-0372 / K-64. The panel on <c>ivr_missed_deadline_total</c> summed every reason the sweep
    /// records and said that anything above zero falsified the capacity model. That stopped being
    /// true as reasons were added: an order held for review or never evaluated (K-52, K-54) and one
    /// that ran out of calling hours (Q-22.2) close on the same counter, and none of them says
    /// anything about channels. K-61 narrowed the alert to the capacity reason and left the panel
    /// reading the sum, so an on-call who followed the ticket to the dashboard met the very claim
    /// the ticket had stopped making.
    /// <para>
    /// Held here: every read of the counter on a panel is split by, or narrowed to, the reason, and
    /// a split names the reason in its legend. The description names every reason the sweep
    /// records, read back from the store as CAP-ALERT-04 reads it, so a reason added later is
    /// explained on the panel or fails here; and it names the runbook of every rule on the
    /// counter. And every sentence of it that speaks of the model names the capacity reason, so
    /// the claim cannot spread back over the whole counter.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DASH-REASON-06")]
    public void TheMissedDeadlinePanelIsSplitByReasonAndTiesTheModelToTheCapacityReasonAlone()
    {
        string root = FindRepositoryRoot();
        (string capacityReason, IReadOnlySet<string> reasons) = MissedDeadlineReasons(root);
        string[] runbooks = MissedDeadlineRunbooks(root);

        // No rule on the counter would leave the runbook half of this check with nothing to hold.
        Assert.NotEmpty(runbooks);

        using JsonDocument dashboard = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            root, "deploy", "observability", "dashboards", "ivr-slo-health.json")));
        JsonElement[] panels =
        [
            .. dashboard.RootElement.GetProperty("panels").EnumerateArray()
                .Where(panel => Targets(panel).Any(target => CounterRead.IsMatch(Expr(target)))),
        ];
        Assert.NotEmpty(panels);

        foreach (JsonElement panel in panels)
        {
            string title = panel.GetProperty("title").GetString() ?? string.Empty;
            foreach (JsonElement target in Targets(panel))
            {
                string expr = Expr(target);
                MatchCollection reads = CounterRead.Matches(expr);
                if (reads.Count == 0)
                {
                    continue;
                }

                bool narrowed = reads.All(read => read.Groups["matchers"].Value
                    .Split(',')
                    .Count(OneReason.IsMatch) == 1);
                bool split = SplitByReason.IsMatch(expr);
                Assert.True(
                    narrowed || split,
                    $"'{title}' reads {MissedDeadlineMetric} without splitting by ivr_reason_code or "
                    + "narrowing to one reason, so its line sums reasons that mean different things: "
                    + expr);
                if (split)
                {
                    string legend = target.TryGetProperty("legendFormat", out JsonElement format)
                        ? format.GetString() ?? string.Empty
                        : string.Empty;
                    Assert.True(
                        legend.Contains("{{ivr_reason_code}}", StringComparison.Ordinal),
                        $"'{title}' splits by ivr_reason_code but its legend ({legend}) does not "
                        + "name the reason, so two of its lines can read the same.");
                }
            }

            string description = panel.GetProperty("description").GetString() ?? string.Empty;
            foreach (string reason in reasons)
            {
                Assert.True(
                    description.Contains(reason, StringComparison.Ordinal),
                    $"'{title}' does not say what {reason} means. The sweep records it on "
                    + $"{MissedDeadlineMetric}, and a line nobody explained reads as the capacity miss "
                    + "the panel used to be about.");
            }

            foreach (string runbook in runbooks)
            {
                Assert.True(
                    description.Contains(runbook, StringComparison.Ordinal),
                    $"'{title}' does not point at {runbook}, the runbook of a rule on its counter.");
            }

            foreach (string sentence in SentenceEnd.Split(description)
                .Where(candidate => ModelClaim.IsMatch(candidate)))
            {
                Assert.True(
                    sentence.Contains(capacityReason, StringComparison.Ordinal),
                    $"'{title}' speaks of the capacity model without naming {capacityReason}, the "
                    + $"only reason the model speaks to: \"{sentence}\"");
            }
        }
    }

    /// <summary>
    /// W-0372 / K-64. Every reason the missed-deadline sweep can count a job under, and which of
    /// them is the capacity miss - read from the code rather than listed here, so a reason added
    /// to the sweep reaches this test without anyone remembering to add it.
    /// </summary>
    private static (string CapacityReason, IReadOnlySet<string> Reasons) MissedDeadlineReasons(string root)
    {
        string store = File.ReadAllText(Path.Combine(
            root, "src", "Ivr.Infrastructure", "Scheduling", "PostgresSchedulerStore.cs"));
        Match branch = SweepBranch.Match(store);
        Assert.True(
            branch.Success,
            "could not find how CloseMissedDeadlinesAsync picks its reason "
            + "(string reasonCode = capacityMiss ? ... : ...).");
        Match counted = SweepCounted.Match(store);
        Assert.True(
            counted.Success,
            "could not find the tuple CloseMissedDeadlinesAsync counts each closed job with.");
        Dictionary<string, string> constants = StoreConstant.Matches(store).ToDictionary(
            constant => constant.Groups[1].Value,
            constant => constant.Groups[2].Value,
            StringComparer.Ordinal);

        // The last element of the tuple is the reason the counter carries. It may be the branch's
        // own reason, or a constant that overrides it (Q-22.2, K-64).
        string recorded = counted.Groups[1].Value.Split(',')[^1];
        var reasons = new HashSet<string>(StringComparer.Ordinal);
        if (Regex.IsMatch(recorded, @"\breasonCode\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
        {
            reasons.Add(branch.Groups[1].Value);
            reasons.Add(branch.Groups[2].Value);
        }

        foreach (Match identifier in Regex.Matches(
            recorded,
            @"\b[A-Z]\w*\b",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(5)))
        {
            Assert.True(
                constants.TryGetValue(identifier.Value, out string? value),
                $"the counted reason names {identifier.Value}, which is not a string constant of "
                + "PostgresSchedulerStore, so this test cannot tell which value it carries.");
            reasons.Add(value);
        }

        Assert.Contains(branch.Groups[1].Value, reasons);
        return (branch.Groups[1].Value, reasons);
    }

    /// <summary>
    /// W-0372 / K-64. The runbook of every alert rule whose expression reads the missed-deadline
    /// counter. Every rule in the file has to parse, or a rule the pattern skipped would drop out
    /// of the check without anyone noticing.
    /// </summary>
    private static string[] MissedDeadlineRunbooks(string root)
    {
        string rules = File.ReadAllText(Path.Combine(
            root, "deploy", "observability", "alerts", "ivr-slo.rules.yml"));
        MatchCollection parsed = AlertRule.Matches(rules);
        Assert.Equal(Regex.Count(rules, @"^\s*- alert: ", RegexOptions.Multiline), parsed.Count);
        return
        [
            .. parsed
                .Where(rule => CounterRead.IsMatch(rule.Groups["expr"].Value))
                .Select(rule => rule.Groups["runbook"].Value),
        ];
    }

    private static JsonElement[] Targets(JsonElement panel) =>
        panel.TryGetProperty("targets", out JsonElement targets)
            ? [.. targets.EnumerateArray()]
            : [];

    private static string Expr(JsonElement target) =>
        target.TryGetProperty("expr", out JsonElement expr)
            ? expr.GetString() ?? string.Empty
            : string.Empty;

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
