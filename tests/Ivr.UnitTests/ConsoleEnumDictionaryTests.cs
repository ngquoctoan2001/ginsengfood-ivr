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
        ["ck_ivr_console_accounts_role"] = "accountRole",
        ["ck_ivr_console_accounts_status"] = "accountStatus",
        ["ck_ivr_task_intake_outbox_status"] = "intakeOutboxStatus",
        ["ck_ivr_sim_channels_mode"] = "executionMode",
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
    /// Pulls <c>"column IN ('A','B')"</c> constraints out of the model source,
    /// paired with the constraint name declared on the line above.
    /// </summary>
    private static Dictionary<string, string[]> ReadCheckConstraints(string source)
    {
        Dictionary<string, string[]> found = new(StringComparer.Ordinal);

        // HasCheckConstraint("name", "col IN ('A','B')") — the two arguments are
        // routinely split across lines by the formatter, so the pattern spans
        // whitespace rather than assuming they share one.
        const string pattern =
            @"HasCheckConstraint\(\s*""(?<name>[a-z0-9_]+)""\s*,\s*""(?<column>[a-z_]+) IN \((?<values>[^)]*)\)""";

        foreach (Match match in Regex.Matches(source, pattern, RegexOptions.Singleline))
        {
            string[] values = Regex.Matches(match.Groups["values"].Value, @"'([^']+)'")
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
