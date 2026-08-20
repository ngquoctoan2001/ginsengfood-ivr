using Ivr.Infrastructure.Governance;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.UnitTests.Governance;

/// <summary>
/// W-0053 / P10-2 §8 — the structural half of <c>DG-RETENTION-04</c>.
///
/// <para>The drill half (a real encrypted backup, restored, pruned by age) runs in
/// <c>deploy/ci/scripts/dr-selftest.mjs</c> against real PostgreSQL. This half is
/// the part that has to hold at compile time: a table nobody classified cannot be
/// given a retention rule, a crypto rule or a backup rule, so it silently gets
/// none of them.</para>
/// </summary>
public sealed class DataClassificationTests
{
    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void EveryShippedTableIsClassified()
    {
        IModel model = BuildModel();

        IReadOnlyList<string> unclassified = DataClassification.FindUnclassifiedTables(model);

        Assert.Empty(unclassified);
    }

    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void NoClassificationEntrySurvivesTheTableItDescribes()
    {
        IModel model = BuildModel();

        // A stale entry keeps the coverage count looking right while describing something that
        // no longer exists — the same failure the analytics exemption list is guarded against.
        Assert.Empty(DataClassification.FindStaleEntries(model));
    }

    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void EveryClassificationCarriesAReasonAReviewerCouldDisagreeWith()
    {
        Assert.NotEmpty(DataClassification.Tables);
        foreach ((string table, DataClassEntry entry) in DataClassification.Tables)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(entry.RetentionClass),
                $"{table} has no retention class.");
            Assert.True(
                entry.Reason.Length >= 40,
                $"{table} has no substantive reason recorded ({entry.Reason.Length} chars).");
        }
    }

    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void TheReportingGrantReachesOnlyDerivedAnalyticsTables()
    {
        // The grant is derived from the classification rather than listed beside it, so it
        // cannot drift. What this asserts is the property that makes the grant defensible:
        // nothing a reporting reader can reach is PiiDirect.
        Assert.NotEmpty(DataClassification.ReportingReadableTables);

        foreach (string table in DataClassification.ReportingReadableTables)
        {
            DataClassEntry entry = DataClassification.Tables[table];
            Assert.NotEqual(DataProtectionClass.PiiDirect, entry.Protection);
            Assert.Equal("analytics_derived", entry.RetentionClass);
        }

        // And the operational tables are not in it, which is the half that would break first
        // if someone widened the grant by reclassifying rather than by deciding.
        Assert.DoesNotContain("ivr_confirmation_tasks", DataClassification.ReportingReadableTables);
        Assert.DoesNotContain("ivr_audit_log", DataClassification.ReportingReadableTables);
    }

    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void EveryClassRequiresAnEncryptedBackup()
    {
        // Stated as a set rather than as a blanket sentence so the document can say why per
        // class. If a class is ever added, this fails until someone decides which side it is on
        // instead of defaulting to the permissive one.
        foreach (DataProtectionClass value in Enum.GetValues<DataProtectionClass>())
        {
            Assert.True(
                DataClassification.RequiresEncryptedBackup.Contains(value),
                $"{value} has no backup-encryption decision recorded.");
        }
    }

    [Fact]
    [Trait("TestId", "DG-RETENTION-04")]
    public void TheGovernanceDocumentDescribesTheSameSchemaAsTheCode()
    {
        // The document is checked against the code, never the other way round. A row here that
        // the code does not have is a claim about a table that does not exist.
        string document = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "docs",
            "data-governance.md"));

        string[] rows = document
            .Split('\n')
            .Where(line => line.StartsWith("| `", StringComparison.Ordinal))
            .Select(line => line.Split('`')[1])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (string table in DataClassification.Tables.Keys)
        {
            Assert.Contains(table, rows);
        }
    }

    private static IModel BuildModel()
    {
        DbContextOptions<IvrDbContext> options = new DbContextOptionsBuilder<IvrDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=model_only;Username=none;Password=none")
            .Options;

        using var context = new IvrDbContext(options);
        return context.Model;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Repository root not found.");
    }
}
