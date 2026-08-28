using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ivr.UnitTests;

/// <summary>
/// W-0107 §6.2. The second coverage layer for the console's data dictionary.
///
/// The first layer lives in admin-ui and reads the OpenAPI spec. It cannot see
/// the value sets that never reach the wire contract — <c>account.status</c>,
/// <c>intake_outbox.status</c> and <c>approval_type</c> are constrained only by
/// a database CHECK constraint, so a value added there would ship untranslated
/// with every TypeScript test still green.
///
/// This test closes that gap from the .NET side: it reads the CHECK constraints
/// out of the EF model configuration and asserts the console has a Vietnamese
/// label for every value they allow.
/// </summary>
public sealed class ConsoleEnumDictionaryTests
{
    /// <summary>
    /// Which dictionary family answers for each constrained column.
    ///
    /// Keyed by constraint name rather than by column name because column names
    /// repeat across tables — three different tables constrain something called
    /// <c>status</c>, and they mean three unrelated things.
    /// </summary>
    private static readonly Dictionary<string, string> FamilyByConstraint = new(StringComparer.Ordinal)
    {
        ["ck_ivr_task_intake_outbox_status"] = "intakeOutboxStatus",
        ["ck_ivr_confirmation_tasks_eligibility_decision"] = "eligibilityDecision",
        ["ck_ivr_call_jobs_status"] = "jobStatus",
        ["ck_ivr_call_jobs_queue_status"] = "jobStatus",
        ["ck_ivr_call_jobs_eligibility_decision"] = "eligibilityDecision",
        ["ck_ivr_call_attempts_status"] = "attemptStatus",
        ["ck_ivr_call_attempts_result_status"] = "resultType",
        ["ck_ivr_call_attempts_voice_region"] = "voiceRegion",
        ["ck_ivr_call_results_result_type"] = "resultType",
        ["ck_ivr_call_results_recommended_core_action"] = "recommendedCoreAction",
        ["ck_ivr_result_callbacks_result_status"] = "resultType",
        ["ck_ivr_result_callbacks_result_state"] = "callbackResultState",
        ["ck_ivr_result_callbacks_delivery_status"] = "deliveryStatus",
        ["ck_ivr_sim_channels_mode"] = "executionMode",
        ["ck_ivr_sim_channels_status"] = "simStatus",
        ["ck_ivr_capacity_incidents_status"] = "incidentStatus",
        ["ck_ivr_capacity_incidents_scope"] = "incidentScope",
        ["ck_ivr_review_items_source_type"] = "reviewSourceType",
        ["ck_ivr_review_items_status"] = "reviewStatus",
        ["ck_ivr_script_versions_status"] = "scriptStatus",
        ["ck_ivr_script_approvals_type"] = "approvalType",
    };

    [Fact]
    [Trait("TestId", "IT-L10N-DBENUM-04")]
    public void EveryDatabaseConstrainedEnumValueHasAVietnameseLabel()
    {
        string root = FindRepositoryRoot();
        string modelSource = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Ivr.Infrastructure",
            "Persistence",
            "PersistenceModelConfiguration.cs"));

        Dictionary<string, string[]> constrained = ReadCheckConstraints(modelSource);

        // If this trips, the constraint set moved and the map above is stale —
        // which is exactly the drift the test exists to notice, so it fails
        // rather than silently covering fewer columns than it claims.
        Assert.NotEmpty(constrained);
        Assert.Equal(
            FamilyByConstraint.Keys.Order(StringComparer.Ordinal),
            constrained.Keys.Order(StringComparer.Ordinal));

        using JsonDocument dictionary = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            root,
            "admin-ui",
            "src",
            "i18n",
            "enums.vi.json")));

        List<string> missing = [];
        foreach ((string constraintName, string[] values) in constrained)
        {
            if (!FamilyByConstraint.TryGetValue(constraintName, out string? family))
            {
                missing.Add(
                    $"CHECK constraint '{constraintName}' allows [{string.Join(", ", values)}] "
                    + "but no console dictionary family is mapped to it (W-0107 §6.2).");
                continue;
            }

            if (!dictionary.RootElement.TryGetProperty(family, out JsonElement table))
            {
                missing.Add($"enums.vi.json has no family '{family}'.");
                continue;
            }

            foreach (string value in values)
            {
                if (!table.TryGetProperty(value, out _))
                {
                    missing.Add($"enums.vi.json {family} is missing '{value}' (from {constraintName}).");
                }
            }
        }

        Assert.Empty(missing);
    }

    /// <summary>
    /// Pulls closed-set <c>IN</c> constraints out of the model source, including nullable
    /// <c>column IS NULL OR column IN (...)</c> forms, paired with their constraint names.
    /// </summary>
    private static Dictionary<string, string[]> ReadCheckConstraints(string source)
    {
        Dictionary<string, string[]> found = new(StringComparer.Ordinal);

        // The SQL argument is routinely split into concatenated C# literals. Capture every
        // literal first, then parse the reconstructed SQL so a line break cannot hide a value.
        const string pattern =
            @"HasCheckConstraint\(\s*""(?<name>[a-z0-9_]+)""\s*,\s*"
            + @"(?<sql>(?:""(?:[^""]|"""")*""\s*(?:\+\s*)?)+)\s*\)";

        foreach (Match match in Regex.Matches(source, pattern, RegexOptions.Singleline))
        {
            string sql = string.Concat(
                Regex.Matches(match.Groups["sql"].Value, @"""(?<part>(?:[^""]|"""")*)""")
                    .Select(literal => literal.Groups["part"].Value.Replace("\"\"", "\"", StringComparison.Ordinal)));
            Match closedSet = Regex.Match(
                sql,
                @"^(?<column>[a-z_]+)(?: IS NULL OR \k<column>)? IN \((?<values>[^)]*)\)$");
            if (!closedSet.Success)
            {
                continue;
            }

            string[] values = Regex.Matches(closedSet.Groups["values"].Value, @"'([^']+)'")
                .Select(quoted => quoted.Groups[1].Value)
                .ToArray();

            if (values.Length > 0)
            {
                found[match.Groups["name"].Value] = values;
            }
        }

        return found;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
            && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The repository root was not found.");
    }
}
