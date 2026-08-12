using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

return args.Length > 0
    ? args[0] switch
    {
        "coverage" => RunCoverage(args[1..]),
        "vulnerabilities" => RunVulnerabilities(args[1..]),
        _ => Usage($"Unknown command: {args[0]}"),
    }
    : Usage("A command is required.");

static int RunCoverage(string[] arguments)
{
    if (arguments.Length is < 2 or > 4)
    {
        return Usage("coverage requires <report-directory> <minimum-percent> [--output <file>].");
    }

    string reportDirectory = Path.GetFullPath(arguments[0]);
    if (!decimal.TryParse(
            arguments[1],
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out decimal minimumPercent))
    {
        return Usage("minimum-percent must be numeric.");
    }

    string[] reports = Directory.Exists(reportDirectory)
        ? Directory.GetFiles(reportDirectory, "coverage.cobertura.xml", SearchOption.AllDirectories)
        : [];

    if (reports.Length == 0)
    {
        return Fail($"No coverage.cobertura.xml report found under {reportDirectory}.");
    }

    Dictionary<string, bool> lineCoverage = new(StringComparer.Ordinal);

    foreach (string report in reports)
    {
        XElement coverage = XDocument.Load(report).Root
            ?? throw new InvalidDataException($"Coverage report has no root: {report}");
        CollectCoverageLines(coverage, report, lineCoverage);
    }

    long valid = lineCoverage.Count;
    if (valid == 0)
    {
        return Fail("Coverage reports contain no executable lines.");
    }

    long covered = lineCoverage.Values.LongCount(isCovered => isCovered);
    decimal percentage = covered * 100m / valid;
    string summary = FormattableString.Invariant(
        $"TOTAL_LINE_COVERAGE={percentage:F2}% COVERED={covered} VALID={valid} REPORTS={reports.Length}");

    Console.WriteLine(summary);

    if (arguments.Length == 4)
    {
        if (!string.Equals(arguments[2], "--output", StringComparison.Ordinal))
        {
            return Usage("Expected --output before the summary file path.");
        }

        string outputPath = Path.GetFullPath(arguments[3]);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, summary + Environment.NewLine);
    }

    return percentage >= minimumPercent
        ? 0
        : Fail(FormattableString.Invariant(
            $"Coverage {percentage:F2}% is below the required {minimumPercent:F2}%."));
}

static int RunVulnerabilities(string[] arguments)
{
    if (arguments.Length != 2)
    {
        return Usage("vulnerabilities requires <dotnet-list-json> <minimum-severity>.");
    }

    int threshold = SeverityRank(arguments[1]);
    if (threshold < 0)
    {
        return Usage("minimum-severity must be low, moderate, high, or critical.");
    }

    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(arguments[0]));
    List<string> blockedSeverities = [];
    CollectSeverities(document.RootElement, blockedSeverities, threshold);

    if (blockedSeverities.Count > 0)
    {
        return Fail(
            $"NuGet vulnerability policy failed: {blockedSeverities.Count} finding(s) at or above {arguments[1]}.");
    }

    Console.WriteLine(
        $"DOTNET_VULNERABILITY_GATE_PASS minimumSeverity={arguments[1].ToUpperInvariant()}");
    return 0;
}

static void CollectSeverities(JsonElement element, List<string> blocked, int threshold)
{
    if (element.ValueKind == JsonValueKind.Object)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, "severity", StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String)
            {
                string severity = property.Value.GetString() ?? string.Empty;
                if (SeverityRank(severity) >= threshold)
                {
                    blocked.Add(severity);
                }
            }

            CollectSeverities(property.Value, blocked, threshold);
        }
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (JsonElement child in element.EnumerateArray())
        {
            CollectSeverities(child, blocked, threshold);
        }
    }
}

static int SeverityRank(string value) => value.ToUpperInvariant() switch
{
    "LOW" => 0,
    "MODERATE" => 1,
    "MEDIUM" => 1,
    "HIGH" => 2,
    "CRITICAL" => 3,
    _ => -1,
};

static long ParseLongAttribute(XElement element, string name, string report)
{
    string? value = element.Attribute(name)?.Value;
    return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
        ? parsed
        : throw new InvalidDataException($"{report} has no valid {name} attribute.");
}

static void CollectCoverageLines(
    XElement coverage,
    string report,
    Dictionary<string, bool> lineCoverage)
{
    foreach (XElement package in coverage.Descendants("package"))
    {
        string packageName = package.Attribute("name")?.Value
            ?? throw new InvalidDataException($"Coverage package has no name: {report}");

        foreach (XElement coveredClass in package.Descendants("class"))
        {
            string className = coveredClass.Attribute("name")?.Value
                ?? coveredClass.Attribute("filename")?.Value
                ?? throw new InvalidDataException($"Coverage class has no identity: {report}");

            foreach (XElement line in coveredClass.Element("lines")?.Elements("line") ?? [])
            {
                long lineNumber = ParseLongAttribute(line, "number", report);
                long hits = ParseLongAttribute(line, "hits", report);
                string key = $"{packageName}|{className}|{lineNumber}";
                lineCoverage[key] = hits > 0 || lineCoverage.GetValueOrDefault(key);
            }
        }
    }
}

static int Usage(string message)
{
    Console.Error.WriteLine(message);
    Console.Error.WriteLine("Usage: Ivr.CiPolicy coverage|vulnerabilities ...");
    return 2;
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
