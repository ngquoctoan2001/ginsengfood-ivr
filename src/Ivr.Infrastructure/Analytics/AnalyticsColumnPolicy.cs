using System.Collections.Frozen;
using Ivr.Domain.Privacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// The privacy filter for the analytics warehouse, in two layers that fail for
/// different reasons.
///
/// <para><b>Layer 1 — structure.</b> <see cref="ValidateModel"/> reads the EF
/// model, not a hand-written list, and requires every column in the
/// <c>analytics</c> schema to be explicitly allowed. A developer who adds a
/// column gets a red test naming the column. This is the layer that matters,
/// because the realistic way PII reaches a warehouse is not a bug in the copy
/// loop — it is someone widening the fact table a year from now and nobody
/// noticing.</para>
///
/// <para><b>Layer 2 — values.</b> <see cref="InspectValue"/> runs the production
/// <see cref="PiiGuard"/> over every string actually written. Layer 1 cannot see
/// that an allowed column holds a phone number because someone stored one in
/// <c>script_version</c> upstream. This layer can, and rejects the row.</para>
///
/// <para>A rejected row is <b>dropped and counted</b>, never written. Dropping
/// loses a data point; writing loses the property that makes the warehouse
/// shareable, so the trade is not close. The count is surfaced on the checkpoint
/// so a silent drop is impossible to confuse with an empty source (D-05).</para>
/// </summary>
public static class AnalyticsColumnPolicy
{
    public const string Schema = "analytics";

    /// <summary>
    /// The complete set of columns permitted in the analytics schema, keyed by
    /// table. Anything else is a violation, including a column that looks
    /// harmless — the point is that the decision is made here and reviewed,
    /// rather than made in a migration nobody reads.
    /// </summary>
    public static FrozenDictionary<string, FrozenSet<string>> AllowedColumns { get; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["fact_call_outcome"] =
            [
                "ivr_call_result_id", "ivr_call_job_id", "order_ref_hash", "program_key",
                "script_variant_key", "result_type_key", "final_result_status", "dtmf_key",
                "is_final", "is_counted_customer_attempt", "counted_attempt_number",
                "event_at", "event_date", "event_hour", "seconds_to_result", "loaded_at",
            ],
            ["fact_call_job"] =
            [
                "ivr_call_job_id", "order_ref_hash", "program_key", "script_variant_key",
                "eligible", "counted_attempt_count", "closed", "created_at", "created_date",
                "loaded_at",
            ],
            ["dim_program"] = ["program_key", "first_seen_at", "last_seen_at", "fact_row_count"],
            ["dim_script_variant"] =
                ["script_variant_key", "first_seen_at", "last_seen_at", "fact_row_count"],
            ["dim_result_type"] =
                ["result_type_key", "is_final", "first_seen_at", "last_seen_at", "fact_row_count"],
            ["agg_kpi_daily"] =
            [
                "bucket_date", "program_key", "script_variant_key", "total_results",
                "final_results", "distinct_orders", "confirmed_count", "cancelled_count",
                "no_answer_count", "invalid_phone_count", "technical_count",
                "operational_blocked_count", "second_attempt_results", "seconds_to_result_sum",
                "seconds_to_result_count", "computed_at",
            ],
            ["etl_checkpoint"] =
            [
                "pipeline_name", "last_run_at", "last_run_loaded_rows", "last_run_rejected_rows",
                "last_run_duration_ms", "total_loaded_rows", "total_rejected_rows",
                "high_water_event_at", "last_reconciled_at", "source_row_count",
                "fact_row_count", "reconcile_status",
            ],
        }.ToFrozenDictionary(
            pair => pair.Key,
            pair => pair.Value.ToFrozenSet(StringComparer.Ordinal),
            StringComparer.Ordinal);

    /// <summary>
    /// Fragments that may never appear in an analytics column name. This is a
    /// second, independent net: it catches a name that someone did remember to
    /// add to <see cref="AllowedColumns"/> — which is exactly how an allowlist
    /// stops working.
    /// </summary>
    public static FrozenSet<string> ForbiddenNameFragments { get; } = new[]
    {
        "phone", "msisdn", "dial_token", "dialtoken", "order_code", "ordercode",
        "customer", "address", "email", "recording", "transcript", "evidence",
        "audit", "sim_channel", "provider", "trust", "risk", "note", "summary",
    }.ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// Columns whose name trips <see cref="ForbiddenNameFragments"/> and which are
    /// nonetheless correct, each with the reason it is correct.
    ///
    /// <para>This exists because the first run of the check flagged two of the
    /// columns defined in this very file — the same class of false positive
    /// <c>W-0100</c> hit when a substring rule flagged <c>invalid_phone</c>, a
    /// count of a result type, as a phone number. The resolution there was to stop
    /// matching substrings. The resolution here is narrower on purpose: matching
    /// substrings is what catches <c>customer_id</c> in a schema nobody re-reads,
    /// so the rule stays and the exceptions become <b>named, keyed and justified</b>
    /// instead of the rule becoming weaker.</para>
    ///
    /// <para>Keyed by <c>table.column</c>, so a different column carrying the same
    /// fragment is still a violation. Exemption from the fragment net is not
    /// exemption from <see cref="AllowedColumns"/>: an exempt column must still be
    /// reviewed and listed there.</para>
    /// </summary>
    public static FrozenDictionary<string, string> FragmentExemptions { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fact_call_outcome.is_counted_customer_attempt"] =
                "Attempt accounting (DT-02): whether this result consumed a counted customer "
                + "attempt rather than a technical retry. A boolean about attempts, not a "
                + "customer field.",
            ["agg_kpi_daily.invalid_phone_count"] =
                "Count of results with type IVR_INVALID_PHONE_FINAL. An integer per bucket; "
                + "the taxonomy name is what carries the word, not the data.",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Reads the EF model and returns every violation found in the analytics
    /// schema. Empty means the shipped schema matches the reviewed one.
    /// </summary>
    public static IReadOnlyList<string> ValidateModel(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        List<string> violations = [];
        HashSet<string> seenTables = new(StringComparer.Ordinal);

        foreach (IEntityType entityType in model.GetEntityTypes())
        {
            if (!string.Equals(entityType.GetSchema(), Schema, StringComparison.Ordinal))
            {
                continue;
            }

            string table = entityType.GetTableName() ?? entityType.ClrType.Name;
            seenTables.Add(table);

            if (!AllowedColumns.TryGetValue(table, out FrozenSet<string>? allowed))
            {
                violations.Add(
                    $"table {Schema}.{table} is not in the reviewed analytics schema.");
                continue;
            }

            foreach (IProperty property in entityType.GetProperties())
            {
                string column = property.GetColumnName() ?? property.Name;

                if (!allowed.Contains(column))
                {
                    violations.Add(
                        $"{Schema}.{table}.{column} is not an allowed analytics column.");
                }

                if (FragmentExemptions.ContainsKey($"{table}.{column}"))
                {
                    continue;
                }

                foreach (string fragment in ForbiddenNameFragments)
                {
                    if (column.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(
                            $"{Schema}.{table}.{column} contains the forbidden fragment "
                            + $"'{fragment}'.");
                    }
                }
            }
        }

        foreach (string declared in AllowedColumns.Keys)
        {
            if (!seenTables.Contains(declared))
            {
                violations.Add(
                    $"{Schema}.{declared} is declared here but absent from the model; the "
                    + "allowlist has drifted from the schema.");
            }
        }

        return violations;
    }

    /// <summary>
    /// Layer 2. Returns true when the value is safe to write. Null and empty are
    /// safe; anything <see cref="PiiGuard"/> rejects — including a regex timeout,
    /// which the guard reports as unsafe (DO-06) — is not.
    /// </summary>
    public static bool InspectValue(string? value) => PiiGuard.IsSafeText(value);
}
