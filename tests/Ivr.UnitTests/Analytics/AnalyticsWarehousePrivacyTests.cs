using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.UnitTests.Analytics;

/// <summary>
/// W-0055 / P10-4 §8 <c>BI-PII-01</c>.
///
/// <para>The assertion is deliberately made against the <b>EF model</b> rather than
/// against a list of column names written in the test. A list in a test is a copy
/// of the schema, and a copy drifts: the day someone adds a column, they add it to
/// the entity and to the migration, and a test holding its own list keeps passing
/// while describing a schema that no longer exists.</para>
///
/// <para>Reading the model means the test sees exactly what will reach PostgreSQL.
/// The reviewed allowlist lives in production code (<see cref="AnalyticsColumnPolicy"/>)
/// so that changing it is a change to the thing being reviewed, not to the thing
/// doing the reviewing.</para>
/// </summary>
public sealed class AnalyticsWarehousePrivacyTests
{
    [Fact]
    [Trait("TestId", "BI-PII-01")]
    public void ShippedAnalyticsSchemaCarriesOnlyReviewedColumns()
    {
        IModel model = BuildModel<IvrDbContext>();

        IReadOnlyList<string> violations = AnalyticsColumnPolicy.ValidateModel(model);

        Assert.Empty(violations);
    }

    [Fact]
    [Trait("TestId", "BI-PII-01")]
    public void EveryAnalyticsTableIsActuallyPresentSoTheCheckIsNotVacuous()
    {
        IModel model = BuildModel<IvrDbContext>();

        string[] analyticsTables = model.GetEntityTypes()
            .Where(entity => entity.GetSchema() == AnalyticsColumnPolicy.Schema)
            .Select(entity => entity.GetTableName()!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        // A policy that validates nothing validates everything. If the analytics schema were
        // ever unmapped, the assertion above would pass on an empty set.
        Assert.Equal(
            AnalyticsColumnPolicy.AllowedColumns.Keys.Order(StringComparer.Ordinal).ToArray(),
            analyticsTables);
        Assert.NotEmpty(analyticsTables);
    }

    [Fact]
    [Trait("TestId", "BI-PII-01")]
    public void AColumnAddedToTheWarehouseFailsTheCheckWithItsOwnName()
    {
        // The negative half. This is the scenario the gate exists for: a future change adds a
        // column to the analytics schema, the migration succeeds, the pipeline runs, and nothing
        // objects. Here it objects, and it names the column.
        IModel model = BuildModel<WidenedAnalyticsContext>();

        IReadOnlyList<string> violations = AnalyticsColumnPolicy.ValidateModel(model);

        Assert.Contains(
            violations,
            violation => violation.Contains("contact_phone_number", StringComparison.Ordinal));
        Assert.Contains(
            violations,
            violation => violation.Contains("forbidden fragment", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "BI-PII-01")]
    public void EveryFragmentExemptionNamesARealColumnAndCarriesAReason()
    {
        // An exemption list is the part of a rule that rots. One that outlives its column is a
        // standing permission for whatever is named that next, so a stale entry is a failure
        // here rather than a comment nobody re-reads.
        IModel model = BuildModel<IvrDbContext>();
        HashSet<string> shipped = model.GetEntityTypes()
            .Where(entity => entity.GetSchema() == AnalyticsColumnPolicy.Schema)
            .SelectMany(entity => entity.GetProperties()
                .Select(property =>
                    $"{entity.GetTableName()}.{property.GetColumnName() ?? property.Name}"))
            .ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(AnalyticsColumnPolicy.FragmentExemptions);
        foreach ((string key, string reason) in AnalyticsColumnPolicy.FragmentExemptions)
        {
            Assert.Contains(key, shipped);
            Assert.True(reason.Length >= 40, $"{key} has no substantive reason recorded.");
        }
    }

    [Theory]
    [Trait("TestId", "BI-PII-01")]
    [InlineData("IVR_CONFIRMED")]
    [InlineData("SCRIPT-ORDER-CONFIRM:vA")]
    [InlineData("GOLDEN_HOUR")]
    [InlineData("JOB-ANALYTICS-01")]
    public void BoundedSourceValuesPassTheValueFilter(string value) =>
        Assert.True(AnalyticsColumnPolicy.InspectValue(value));

    [Fact]
    [Trait("TestId", "BI-PII-01")]
    public void ASubscriberNumberSmuggledIntoABoundedColumnIsRejected()
    {
        // Layer 1 cannot see this: the column is allowed, and the value is what makes it unsafe.
        // Assembled from parts so the file does not itself contain the literal shape it rejects,
        // and from the reserved test range so it can never resemble a real subscriber.
        string smuggled = "SCRIPT-84" + new string('5', 9);

        Assert.False(AnalyticsColumnPolicy.InspectValue(smuggled));
        Assert.True(AnalyticsColumnPolicy.InspectValue("SCRIPT-ORDER-CONFIRM:vA"));
    }

    private static IModel BuildModel<TContext>()
        where TContext : DbContext
    {
        // No connection is opened. Model building is offline, which is what lets this run as a
        // unit test rather than needing the container the integration suite uses.
        DbContextOptions<TContext> options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=model_only;Username=none;Password=none")
            .Options;

        using var context = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        return context.Model;
    }

    /// <summary>A future schema that leaked a customer field. Exists only to be rejected.</summary>
    private sealed class WidenedAnalyticsContext(DbContextOptions<WidenedAnalyticsContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.Entity<LeakyFact>(builder =>
            {
                builder.ToTable("fact_call_outcome", AnalyticsColumnPolicy.Schema);
                builder.HasKey(fact => fact.IvrCallResultId);
                builder.Property(fact => fact.IvrCallResultId).HasColumnName("ivr_call_result_id");
                builder.Property(fact => fact.ContactPhoneNumber)
                    .HasColumnName("contact_phone_number");
            });
        }

        internal sealed class LeakyFact
        {
            public string IvrCallResultId { get; set; } = string.Empty;
            public string ContactPhoneNumber { get; set; } = string.Empty;
        }
    }
}
