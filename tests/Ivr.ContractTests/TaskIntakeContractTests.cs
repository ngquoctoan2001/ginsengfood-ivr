using System.Text.Json;

namespace Ivr.ContractTests;

public sealed class TaskIntakeContractTests
{
    [Fact]
    [Trait("TestId", "CT-INTAKE-OPENAPI-01")]
    public async Task OpenApiPublishesTaskIntakeHeadersMatrixAnd422()
    {
        string openApi = await File.ReadAllTextAsync(FindRepositoryFile(
            "specs", "api", "openapi", "ivr-order-confirmation.v1.yaml"));

        Assert.Contains("/tasks:", openApi, StringComparison.Ordinal);
        Assert.Contains("operationId: intakeTask", openApi, StringComparison.Ordinal);
        Assert.Contains("#\u002Fcomponents\u002Fparameters\u002FCorrelationId".Replace("\u002F", "/"),
            openApi,
            StringComparison.Ordinal);
        Assert.Contains("#\u002Fcomponents\u002Fparameters\u002FIdempotencyKey".Replace("\u002F", "/"),
            openApi,
            StringComparison.Ordinal);
        Assert.Contains("program_code: { const: GOLDEN_HOUR }", openApi,
            StringComparison.Ordinal);
        Assert.Contains("payment_method_snapshot: { const: ONLINE }", openApi,
            StringComparison.Ordinal);
        Assert.Contains("program_code: { const: TWENTY_FOUR_SEVEN }", openApi,
            StringComparison.Ordinal);
        Assert.Contains("payment_method_snapshot: { const: COD }", openApi,
            StringComparison.Ordinal);
        Assert.Contains("'422': { $ref: '#/components/responses/Error' }", openApi,
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "CT-M3-AUTHORITY-08")]
    public async Task OpenApiPublishesM3AuthorityAndOnlyLegacyTrustCompatibility()
    {
        string openApi = await File.ReadAllTextAsync(FindRepositoryFile(
            "specs", "api", "openapi", "ivr-order-confirmation.v1.yaml"));

        Assert.Contains("version: 1.0.0-draft.22", openApi, StringComparison.Ordinal);
        Assert.Contains(
            "M3 has already decided that the order requires a call",
            openApi,
            StringComparison.Ordinal);
        Assert.Contains("LEGACY_READ compatibility field", openApi, StringComparison.Ordinal);
        Assert.Contains(
            "Draft.21 runtime no",
            openApi,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Any entry forces the call", openApi, StringComparison.Ordinal);
        Assert.DoesNotContain("An EMPTY list cancels the call", openApi, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "CT-M3-AUTHORITY-09")]
    public async Task LinkedEvidenceSchemaMarksTrustAsIgnoredLegacyInput()
    {
        using JsonDocument schema = JsonDocument.Parse(await File.ReadAllTextAsync(
            FindRepositoryFile(
                "specs", "api", "evidence", "eligibility-snapshot.v1.schema.json")));
        JsonElement trust = schema.RootElement
            .GetProperty("properties")
            .GetProperty("trust");

        Assert.True(trust.GetProperty("deprecated").GetBoolean());
        Assert.Contains(
            "Active IVR eligibility ignores this entire object",
            trust.GetProperty("description").GetString(),
            StringComparison.Ordinal);
        Assert.Contains(
            "M3 owns that business decision",
            schema.RootElement
                .GetProperty("x-ivr-fail-closed-directions")
                .GetProperty("trust")
                .GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "CT-INTAKE-FIXTURES-02")]
    public async Task CanonicalFakeCatalogCoversAllP21AcceptanceClasses()
    {
        using JsonDocument fixture = JsonDocument.Parse(await File.ReadAllTextAsync(
            FindRepositoryFile("seed", "sales-target-v1.sample.json")));
        JsonElement root = fixture.RootElement;
        string[] taskScenarios = root.GetProperty("tasks")
            .EnumerateArray()
            .Select(item => item.GetProperty("scenario").GetString()!)
            .ToArray();
        string[] schemaIds = root.GetProperty("schema_negative")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetString()!)
            .ToArray();
        JsonElement[] domain = root.GetProperty("domain_negative")
            .EnumerateArray()
            .ToArray();
        string[] domainIds = domain
            .Select(item => item.GetProperty("id").GetString()!)
            .ToArray();

        Assert.Contains("golden-hour-online-confirmable", taskScenarios);
        Assert.Contains("twenty-four-seven-cod-confirmable", taskScenarios);
        Assert.Contains("NEG-SCHEMA-FLAG-ABSENT-01", schemaIds);
        Assert.Contains("NEG-SCHEMA-EVIDENCE-01", schemaIds);
        Assert.Contains("NEG-DOMAIN-PII-01", domainIds);
        Assert.Contains("NEG-DOMAIN-TOKEN-01", domainIds);
        Assert.Contains("NEG-DOMAIN-POLICY-01", domainIds);
        Assert.Contains("NEG-DOMAIN-RESTRICTION-01", domainIds);
        Assert.Contains("NEG-DOMAIN-WINDOW-01", domainIds);
        Assert.Contains("NEG-DOMAIN-SCRIPT-01", domainIds);
        Assert.Contains("NEG-DOMAIN-IDEMPOTENCY-01", domainIds);
        Assert.Contains("NEG-DOMAIN-IDEMPOTENCY-03", domainIds);
        JsonElement replay = Assert.Single(domain, item =>
            item.GetProperty("id").GetString() == "NEG-DOMAIN-IDEMPOTENCY-02");
        Assert.Equal(
            "TASK_ACCEPTED_DRY_RUN_ONLY",
            replay.GetProperty("expect_decision").GetString());
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = segments.Aggregate(directory.FullName, Path.Combine);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            string.Concat("Repository file was not found: ", Path.Combine(segments)));
    }
}
