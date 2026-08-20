using Ivr.Infrastructure.Governance;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.UnitTests.Governance;

/// <summary>
/// W-0052 / P10-1 §8 <c>COMP-PII-01</c> — the inventory matches what actually ships.
///
/// <para>A privacy inventory is the artefact most likely to be true on the day it
/// is written and false a month later, because nothing about adding a column makes
/// anyone open it. Reading the EF model turns that from a habit into a gate.</para>
/// </summary>
public sealed class PersonalDataInventoryTests
{
    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void NoPersonalDataFieldShipsOutsideTheInventory()
    {
        IModel model = BuildModel();

        IReadOnlyList<string> missing = PersonalDataInventory.FindUninventoriedFields(model);

        Assert.Empty(missing);
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void TheInventoryDescribesTheSchemaThatShipsAndNothingElse()
    {
        IModel model = BuildModel();

        Assert.Empty(PersonalDataInventory.FindStaleEntries(model));
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void ANewCustomerColumnFailsUntilSomeoneInventoriesIt()
    {
        // The negative half, and the exact scenario the gate exists for: a migration adds a
        // customer field and nobody opens the inventory. Here that is a red test naming the
        // column rather than a document that quietly stops being true.
        IModel model = BuildModel<LeakyContext>();

        IReadOnlyList<string> missing = PersonalDataInventory.FindUninventoriedFields(model);

        Assert.Contains("ivr_leaky.customer_email", missing);
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void EveryFieldStatesAPurposeALegalBasisAndWhatErasureDoesToIt()
    {
        Assert.NotEmpty(PersonalDataInventory.Fields);

        foreach (PersonalDataField field in PersonalDataInventory.Fields)
        {
            Assert.True(
                field.Purpose.Length >= 40,
                $"{field.Table}.{field.Column} has no substantive purpose recorded.");
            Assert.True(
                field.ErasureBehaviour.Length >= 30,
                $"{field.Table}.{field.Column} does not say what erasure does to it.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void NothingRestsOnConsentBecauseIvrNeverAsksForAny()
    {
        // The legal basis has to match the system that exists. IVR never presents a consent
        // dialogue and never records a consent decision, so a field claiming consent as its basis
        // would be claiming a basis nobody obtained.
        Assert.All(
            PersonalDataInventory.Fields,
            field => Assert.True(
                field.Basis is PersonalDataLegalBasis.ContractPerformance
                    or PersonalDataLegalBasis.LegalRecordKeeping,
                $"{field.Table}.{field.Column} claims a basis IVR cannot support."));
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void AuditFieldsAreRecordedAsNeverErasable()
    {
        // The limit on the erasure right, written down rather than discovered during a request.
        // Append-only is enforced by the database; this asserts the inventory says so, because a
        // DSAR runbook promising erasure of an audit row would promise something impossible.
        foreach (PersonalDataField field in PersonalDataInventory.Fields
            .Where(field => field.Table is "ivr_audit_log" or "ivr_admin_actions"))
        {
            Assert.Equal(PersonalDataLegalBasis.LegalRecordKeeping, field.Basis);
            Assert.Contains("NEVER erased", field.ErasureBehaviour, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void EveryNonPersonalExemptionNamesARealColumnAndCarriesAReason()
    {
        Assert.NotEmpty(PersonalDataInventory.NonPersonalExemptions);
        foreach ((string key, string reason) in PersonalDataInventory.NonPersonalExemptions)
        {
            Assert.True(reason.Length >= 30, $"{key} has no substantive reason recorded.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-PII-01")]
    public void TheInventoryDocumentDescribesTheSameFieldsAsTheCode()
    {
        string document = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "docs", "compliance", "data-inventory.md"));

        foreach (PersonalDataField field in PersonalDataInventory.Fields)
        {
            Assert.Contains($"{field.Table}.{field.Column}", document, StringComparison.Ordinal);
        }
    }

    private static IModel BuildModel() => BuildModel<IvrDbContext>();

    private static IModel BuildModel<TContext>()
        where TContext : DbContext
    {
        DbContextOptions<TContext> options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=model_only;Username=none;Password=none")
            .Options;

        using var context = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        return context.Model;
    }

    /// <summary>A future schema that added a customer field. Exists only to be caught.</summary>
    private sealed class LeakyContext(DbContextOptions<LeakyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.Entity<LeakyRow>(builder =>
            {
                builder.ToTable("ivr_leaky");
                builder.HasKey(row => row.Id);
                builder.Property(row => row.Id).HasColumnName("id");
                builder.Property(row => row.CustomerEmail).HasColumnName("customer_email");
            });
        }

        internal sealed class LeakyRow
        {
            public string Id { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
        }
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
