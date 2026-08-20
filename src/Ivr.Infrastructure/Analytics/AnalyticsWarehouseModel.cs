using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// EF configuration for the analytics schema. Kept beside the entities rather
/// than folded into <c>PersistenceModelConfiguration</c> so the operational model
/// and the derived model stay visibly separate — they have different owners,
/// different retention and different grant boundaries.
/// </summary>
internal static class AnalyticsWarehouseModel
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<AnalyticsFactCallOutcomeEntity>(builder =>
        {
            builder.ToTable(
                "fact_call_outcome",
                AnalyticsColumnPolicy.Schema,
                table =>
                {
                    // The hash is the only representation of a Sales order id permitted
                    // here, so the shape is enforced by the database rather than trusted
                    // to the loader: a raw order id would not be 64 lowercase hex chars.
                    table.HasCheckConstraint(
                        "ck_analytics_fact_order_ref_hash",
                        "order_ref_hash ~ '^[a-f0-9]{64}$'");
                    table.HasCheckConstraint(
                        "ck_analytics_fact_event_hour",
                        "event_hour BETWEEN 0 AND 23");
                    // A single DTMF digit. Anything longer is free text arriving where
                    // free text is not allowed.
                    table.HasCheckConstraint(
                        "ck_analytics_fact_dtmf",
                        "dtmf_key IS NULL OR dtmf_key ~ '^[0-9*#]$'");
                });

            builder.HasKey(fact => fact.IvrCallResultId);
            builder.Property(fact => fact.IvrCallResultId).HasMaxLength(64);
            builder.Property(fact => fact.IvrCallJobId).HasMaxLength(64);
            builder.Property(fact => fact.OrderRefHash).HasMaxLength(64);
            builder.Property(fact => fact.ProgramKey).HasMaxLength(64);
            builder.Property(fact => fact.ScriptVariantKey).HasMaxLength(64);
            builder.Property(fact => fact.ResultTypeKey).HasMaxLength(64);
            builder.Property(fact => fact.FinalResultStatus).HasMaxLength(64);
            builder.Property(fact => fact.DtmfKey).HasMaxLength(1);

            builder.HasIndex(fact => fact.EventDate);
            builder.HasIndex(fact => new { fact.EventDate, fact.ProgramKey });
            builder.HasIndex(fact => fact.LoadedAt);
        });

        modelBuilder.Entity<AnalyticsFactCallJobEntity>(builder =>
        {
            builder.ToTable(
                "fact_call_job",
                AnalyticsColumnPolicy.Schema,
                table => table.HasCheckConstraint(
                    "ck_analytics_job_order_ref_hash",
                    "order_ref_hash ~ '^[a-f0-9]{64}$'"));

            builder.HasKey(fact => fact.IvrCallJobId);
            builder.Property(fact => fact.IvrCallJobId).HasMaxLength(64);
            builder.Property(fact => fact.OrderRefHash).HasMaxLength(64);
            builder.Property(fact => fact.ProgramKey).HasMaxLength(64);
            builder.Property(fact => fact.ScriptVariantKey).HasMaxLength(64);

            builder.HasIndex(fact => fact.CreatedDate);
            // The refresh pass reads exactly this predicate on every run.
            builder.HasIndex(fact => fact.Closed);
        });

        modelBuilder.Entity<AnalyticsDimProgramEntity>(builder =>
        {
            builder.ToTable("dim_program", AnalyticsColumnPolicy.Schema);
            builder.HasKey(dim => dim.ProgramKey);
            builder.Property(dim => dim.ProgramKey).HasMaxLength(64);
        });

        modelBuilder.Entity<AnalyticsDimScriptVariantEntity>(builder =>
        {
            builder.ToTable("dim_script_variant", AnalyticsColumnPolicy.Schema);
            builder.HasKey(dim => dim.ScriptVariantKey);
            builder.Property(dim => dim.ScriptVariantKey).HasMaxLength(64);
        });

        modelBuilder.Entity<AnalyticsDimResultTypeEntity>(builder =>
        {
            builder.ToTable("dim_result_type", AnalyticsColumnPolicy.Schema);
            builder.HasKey(dim => dim.ResultTypeKey);
            builder.Property(dim => dim.ResultTypeKey).HasMaxLength(64);
        });

        modelBuilder.Entity<AnalyticsKpiDailyEntity>(builder =>
        {
            builder.ToTable("agg_kpi_daily", AnalyticsColumnPolicy.Schema);
            builder.HasKey(row => new { row.BucketDate, row.ProgramKey, row.ScriptVariantKey });
            builder.Property(row => row.ProgramKey).HasMaxLength(64);
            builder.Property(row => row.ScriptVariantKey).HasMaxLength(64);
        });

        modelBuilder.Entity<AnalyticsEtlCheckpointEntity>(builder =>
        {
            builder.ToTable("etl_checkpoint", AnalyticsColumnPolicy.Schema);
            builder.HasKey(row => row.PipelineName);
            builder.Property(row => row.PipelineName).HasMaxLength(64);
            builder.Property(row => row.ReconcileStatus).HasMaxLength(32);
        });
    }
}
