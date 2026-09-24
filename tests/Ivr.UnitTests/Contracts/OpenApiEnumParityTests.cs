using System.Reflection;
using System.Text.Json.Serialization;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Intake;

namespace Ivr.UnitTests.Contracts;

/// <summary>
/// W-0353 / P5-2 §8 <c>CT-OAS-03</c>. The two closed lists the intake contract publishes,
/// <c>ErrorCode</c> and <c>ProgramCode</c>, read out of the OpenAPI document and held against
/// the code that claims to be the same list.
/// <para>
/// Each list lives in three places: the document, the enum NSwag generates from it, and a
/// hand-written counterpart. Regeneration keeps the first two together only when someone runs
/// it, and nothing held the third to either. An error code added to <c>IvrErrorCodes</c> alone
/// reaches the wire as a value the contract says cannot exist. A program added to the document
/// alone passes the schema and then has no signed D-10 policy to be called under.
/// </para>
/// <para>
/// The document is read as text, the way <c>CT-API-ADMIN-PARITY-01</c> reads it: this project
/// has no YAML parser, and both schemas are plain enums.
/// </para>
/// </summary>
public sealed class OpenApiEnumParityTests
{
    /// <summary>
    /// Sorted rather than set-compared, so a code listed twice in the document is a difference
    /// too.
    /// </summary>
    [Fact]
    [Trait("TestId", "CT-OAS-03")]
    public void TheErrorCodeEnumIsOneListInTheSpecTheDomainAndTheGeneratedContract()
    {
        string[] documented = [.. ReadSchemaEnum("ErrorCode").Order(StringComparer.Ordinal)];

        Assert.NotEmpty(documented);
        Assert.Equal(documented, IvrErrorCodes.All.Order(StringComparer.Ordinal));
        Assert.Equal(documented, WireValues<ErrorCode>().Order(StringComparer.Ordinal));
    }

    /// <summary>
    /// D-10 as it applies to every program the contract admits: a policy signed for production,
    /// two customer attempts, the second at the middle of the window. The per-program numbers
    /// themselves are pinned by <c>UT-POLICY-SIGNED-03</c>; this is the link from the wire value
    /// a producer sends to the policy it will be called under.
    /// </summary>
    [Fact]
    [Trait("TestId", "CT-OAS-03")]
    public void EveryProgramInTheSpecIsGeneratedAndHasASignedD10AttemptPolicy()
    {
        string[] documented = [.. ReadSchemaEnum("ProgramCode").Order(StringComparer.Ordinal)];

        Assert.NotEmpty(documented);
        Assert.Equal(documented, WireValues<ProgramCode>().Order(StringComparer.Ordinal));

        IReadOnlyList<AttemptPolicySnapshot> signed = SignedProductionAttemptPolicies.Create();
        foreach (ProgramCode program in Enum.GetValues<ProgramCode>())
        {
            // The same mapping TargetV1ContractMapper and TaskIntakeService make privately. A
            // program generated from the document and missing here fails with its own name.
            IvrProgramCode domainProgram = program switch
            {
                ProgramCode.GOLDEN_HOUR => IvrProgramCode.GoldenHour,
                ProgramCode.TWENTY_FOUR_SEVEN => IvrProgramCode.TwentyFourSeven,
                _ => throw new InvalidOperationException(
                    string.Concat(program.ToString(), " has no domain program to hold a policy.")),
            };
            AttemptPolicySnapshot policy = Assert.Single(
                signed,
                candidate => candidate.Program == domainProgram);

            Assert.Equal(SignedProductionAttemptPolicies.Version, policy.Version.Value);
            Assert.Equal(AttemptPolicyApproval.OwnerApproved, policy.Approval);
            Assert.Equal(2, policy.MaxCustomerAttempts);
            Assert.Equal(
                [TimeSpan.Zero, policy.ConfirmationWindowDuration / 2],
                policy.AttemptOffsets);
        }
    }

    /// <summary>
    /// What <c>JsonStringEnumConverter</c> writes for each member, which is what the generated
    /// properties serialize with: the <c>JsonStringEnumMemberName</c> when there is one, otherwise
    /// the member's own name.
    /// </summary>
    private static IEnumerable<string> WireValues<TEnum>()
        where TEnum : struct, Enum =>
        typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(member =>
                member.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name
                ?? member.Name);

    /// <summary>
    /// The <c>enum</c> of one schema under <c>components.schemas</c>, in either YAML spelling the
    /// document uses: a flow list on the <c>enum:</c> line, or a block list under it.
    /// </summary>
    private static string[] ReadSchemaEnum(string schemaName)
    {
        string[] lines = File.ReadAllLines(Path.Combine(
            FindRepositoryRoot(),
            "specs",
            "api",
            "openapi",
            "ivr-order-confirmation.v1.yaml"));
        int components = Array.IndexOf(lines, "components:");
        int schemas = components < 0 ? -1 : Array.IndexOf(lines, "  schemas:", components);
        int schema = schemas < 0
            ? -1
            : Array.IndexOf(lines, string.Concat("    ", schemaName, ":"), schemas);
        if (schema < 0)
        {
            throw new InvalidOperationException(
                string.Concat("components.schemas.", schemaName, " is not in the document."));
        }

        const string enumKey = "      enum:";
        const string blockItem = "        - ";
        for (int index = schema + 1;
             index < lines.Length
                 && (lines[index].Length == 0
                     || lines[index].StartsWith("      ", StringComparison.Ordinal));
             index++)
        {
            if (!lines[index].StartsWith(enumKey, StringComparison.Ordinal))
            {
                continue;
            }

            string flow = lines[index][enumKey.Length..].Trim();
            if (flow.Length > 0)
            {
                return flow.StartsWith('[') && flow.EndsWith(']')
                    ? [.. flow[1..^1].Split(',').Select(Unquote)]
                    : throw new InvalidOperationException(
                        string.Concat(schemaName, " has an enum this reader does not understand."));
            }

            return
            [
                .. lines
                    .Skip(index + 1)
                    .TakeWhile(line => line.StartsWith(blockItem, StringComparison.Ordinal))
                    .Select(line => Unquote(line[blockItem.Length..])),
            ];
        }

        throw new InvalidOperationException(
            string.Concat("components.schemas.", schemaName, " has no enum of its own."));
    }

    private static string Unquote(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length >= 2
            && (trimmed[0] is '"' or '\'')
            && trimmed[^1] == trimmed[0]
                ? trimmed[1..^1]
                : trimmed;
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
