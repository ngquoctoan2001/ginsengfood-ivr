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

    JsonDocument document;
    try
    {
        document = JsonDocument.Parse(File.ReadAllText(arguments[0]));
    }
    catch (JsonException)
    {
        return Fail("NuGet vulnerability report is not valid JSON.");
    }
    catch (IOException)
    {
        return Fail("NuGet vulnerability report could not be read.");
    }
    catch (UnauthorizedAccessException)
    {
        return Fail("NuGet vulnerability report could not be read.");
    }

    using (document)
    {
        if (!IsValidVulnerabilityReport(document.RootElement))
        {
            return Fail("NuGet vulnerability report has an invalid or incomplete schema.");
        }

        List<string> blockedSeverities = [];
        int invalidSeverityCount = 0;
        CollectSeverities(
            document.RootElement,
            blockedSeverities,
            threshold,
            ref invalidSeverityCount);

        if (invalidSeverityCount > 0)
        {
            return Fail(
                $"NuGet vulnerability report contains {invalidSeverityCount} unknown or malformed severity value(s).");
        }

        if (blockedSeverities.Count > 0)
        {
            return Fail(
                $"NuGet vulnerability policy failed: {blockedSeverities.Count} finding(s) at or above {arguments[1]}.");
        }
    }

    Console.WriteLine(
        $"DOTNET_VULNERABILITY_GATE_PASS minimumSeverity={arguments[1].ToUpperInvariant()}");
    return 0;
}

static bool IsValidVulnerabilityReport(JsonElement root)
{
    if (root.ValueKind != JsonValueKind.Object
        || !root.TryGetProperty("version", out JsonElement version)
        || version.ValueKind != JsonValueKind.Number
        || !version.TryGetInt32(out int versionNumber)
        || versionNumber != 1
        || !root.TryGetProperty("parameters", out JsonElement parameters)
        || parameters.ValueKind != JsonValueKind.String
        || !HasExpectedVulnerabilityParameters(parameters.GetString())
        || !root.TryGetProperty("projects", out JsonElement projects)
        || projects.ValueKind != JsonValueKind.Array
        || projects.GetArrayLength() == 0)
    {
        return false;
    }

    foreach (JsonElement project in projects.EnumerateArray())
    {
        if (project.ValueKind != JsonValueKind.Object
            || !project.TryGetProperty("path", out JsonElement projectPath)
            || projectPath.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(projectPath.GetString()))
        {
            return false;
        }
    }

    return true;
}

static bool HasExpectedVulnerabilityParameters(string? parameters) =>
    parameters?.Contains("--vulnerable", StringComparison.Ordinal) == true
    && parameters.Contains("--include-transitive", StringComparison.Ordinal);

static void CollectSeverities(
    JsonElement element,
    List<string> blocked,
    int threshold,
    ref int invalidSeverityCount)
{
    if (element.ValueKind == JsonValueKind.Object)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, "severity", StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    invalidSeverityCount++;
                }
                else
                {
                    string severity = property.Value.GetString() ?? string.Empty;
                    int rank = SeverityRank(severity);
                    if (rank < 0)
                    {
                        invalidSeverityCount++;
                    }
                    else if (rank >= threshold)
                    {
                        blocked.Add(severity);
                    }
                }
            }

            CollectSeverities(
                property.Value,
                blocked,
                threshold,
                ref invalidSeverityCount);
        }
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (JsonElement child in element.EnumerateArray())
        {
            CollectSeverities(child, blocked, threshold, ref invalidSeverityCount);
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
