using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.Infrastructure.Governance;

/// <summary>
/// Protection class for one physical table. Distinct from the P1-5 <i>retention</i>
/// class, and the distinction matters: retention answers "when does this go away",
/// classification answers "what has to be true while it is here".
///
/// <para>A table can be short-lived and highly sensitive, or permanent and
/// harmless. Folding the two into one label is how a backup ends up encrypted
/// because it is old rather than because of what is in it.</para>
/// </summary>
public enum DataProtectionClass
{
    /// <summary>
    /// Holds a customer-linked field — phone reference, masked phone, dial-token
    /// ciphertext, order code, speech snapshot. Encryption at rest and in transit
    /// is mandatory; a backup carrying this may never be written unencrypted, even
    /// transiently to a local file.
    /// </summary>
    PiiDirect,

    /// <summary>
    /// No customer field, but carries a key that resolves to one in a system IVR
    /// does not own (the hashed Sales order reference). Same crypto obligations —
    /// a pseudonym is not anonymity — but it may be shared with a reporting reader
    /// that must never see <see cref="PiiDirect"/>.
    /// </summary>
    PiiDerived,

    /// <summary>
    /// Append-only record of who did what. Never purged by P1-5, so it is the class
    /// most likely to outlive every other copy of a fact; encrypted at rest,
    /// integrity-protected in backup, and excluded from anonymisation.
    /// </summary>
    AuditTrail,

    /// <summary>
    /// Operational state with no customer field: leases, counters, checkpoints,
    /// incidents. Encrypted at rest because it shares a volume, but it is not what
    /// the crypto exists for.
    /// </summary>
    Operational,

    /// <summary>
    /// Versioned configuration: flags, policies, script versions. Preserved rather
    /// than purged, and its integrity matters more than its confidentiality — a
    /// tampered attempt policy changes how often customers are called.
    /// </summary>
    Configuration,
}

/// <summary>
/// The governance map behind <c>docs/data-governance.md</c> (<c>W-0053</c> / P10-2).
///
/// <para>It lives in code rather than in the document for the same reason the
/// analytics allowlist does: a classification table maintained by hand in Markdown
/// describes the schema as it was on the day someone last read it. Here a new
/// table fails <c>DG-RETENTION-04</c> until somebody classifies it, and the
/// document is checked against this rather than the other way round.</para>
/// </summary>
public static class DataClassification
{
    /// <summary>
    /// Every physical table in the model, with its protection class and the reason.
    /// The reason is not decoration: it is what a reviewer needs in order to
    /// disagree, and a class with no stated reason is a class nobody can audit.
    /// </summary>
    public static FrozenDictionary<string, DataClassEntry> Tables { get; } =
        new Dictionary<string, DataClassEntry>(StringComparer.Ordinal)
        {
            ["ivr_confirmation_tasks"] = new(
                DataProtectionClass.PiiDirect,
                "task_metadata",
                "Phone reference, masked phone, dial-token ciphertext and the speech snapshot "
                + "all live here. This is the table the whole privacy policy is about.",
                PreDeletionAnonymizeClass: "speech_snapshot"),
            ["ivr_task_intake_outbox"] = new(
                DataProtectionClass.Operational,
                "task_metadata",
                "Hash and correlation id only; the request body is deliberately not kept."),
            ["ivr_attempt_policies"] = new(
                DataProtectionClass.Configuration,
                "active_config",
                "Immutable versioned policy. Integrity outweighs confidentiality: a tampered "
                + "policy changes how often a customer is called."),
            ["ivr_call_jobs"] = new(
                DataProtectionClass.PiiDerived,
                "task_metadata",
                "No customer field, but the Sales order id resolves to one in a system IVR does "
                + "not own."),
            ["ivr_call_attempts"] = new(
                DataProtectionClass.PiiDerived,
                "attempt_metadata",
                "Attempt accounting keyed to a job; the SIM channel it used is operational, the "
                + "order behind it is not."),
            ["ivr_raw_call_events"] = new(
                DataProtectionClass.PiiDirect,
                "raw_call_event",
                "Provider payload reference and the recording ref. Recording is OFF by default, "
                + "which is a setting rather than a property of the table."),
            ["ivr_call_results"] = new(
                DataProtectionClass.PiiDerived,
                "result_metadata",
                "Outcome plus the Sales order id. The DTMF digit is the customer's answer, not "
                + "their identity."),
            ["ivr_result_callbacks"] = new(
                DataProtectionClass.PiiDirect,
                "callback_metadata",
                "Holds the immutable payload sent to Sales, which carries order-scoped fields."),
            ["ivr_sim_channels"] = new(
                DataProtectionClass.Operational,
                "active_config",
                "Channel lease and health. Carries sim_number_ref, a reference rather than a "
                + "number, and never a customer field."),
            ["ivr_capacity_incidents"] = new(
                DataProtectionClass.Operational,
                "task_metadata",
                "Counts and timestamps describing shortage, no per-customer row."),
            ["ivr_technical_exceptions"] = new(
                DataProtectionClass.Operational,
                "attempt_metadata",
                "Device and provider faults keyed to an attempt."),
            ["ivr_admin_actions"] = new(
                DataProtectionClass.AuditTrail,
                "audit_log",
                "Append-only. Outlives every other copy of the fact it records, so it is the "
                + "class most exposed by a backup that is kept too long."),
            ["ivr_evidence_links"] = new(
                DataProtectionClass.PiiDerived,
                "evidence_link",
                "Points at evidence for a specific order; accepted links are protected from "
                + "purge entirely."),
            ["ivr_idempotency_keys"] = new(
                DataProtectionClass.PiiDirect,
                "idempotency_key",
                "Stores a response snapshot, which is whatever the endpoint returned — the one "
                + "table whose contents are defined by other tables."),
            ["ivr_audit_log"] = new(
                DataProtectionClass.AuditTrail,
                "audit_log",
                "Append-only; UPDATE and DELETE are refused by the database itself."),
            ["ivr_evidence"] = new(
                DataProtectionClass.PiiDerived,
                "evidence_link",
                "Evidence bodies are order-scoped; accepted evidence is never purged."),
            ["ivr_feature_flags"] = new(
                DataProtectionClass.Configuration,
                "active_config",
                "Runtime gates including the kill switch. Integrity is the property that "
                + "matters — a flipped flag is a governance event."),
            ["ivr_review_items"] = new(
                DataProtectionClass.PiiDerived,
                "review_item",
                "Human review queue keyed to a job; anonymised rather than deleted so the "
                + "audit shape survives."),
            ["ivr_retention_checkpoints"] = new(
                DataProtectionClass.Operational,
                "retention_control",
                "Aggregate counters per (data class, segment). No row identifiers."),
            ["ivr_script_versions"] = new(
                DataProtectionClass.Configuration,
                "active_config",
                "Approved call script text and its hash. What the customer hears, versioned."),
            ["ivr_script_approvals"] = new(
                DataProtectionClass.AuditTrail,
                "active_config",
                "Who approved which script version. An approval record is an audit record."),

            // W-0055. Derived reporting copies. Classified separately because they are the
            // tables a BI reader is granted, and that grant is only defensible while nothing
            // here is PiiDirect.
            ["fact_call_outcome"] = new(
                DataProtectionClass.PiiDerived,
                "analytics_derived",
                "Hashed order reference plus IVR-internal ids. No customer field by "
                + "construction, enforced by AnalyticsColumnPolicy."),
            ["fact_call_job"] = new(
                DataProtectionClass.PiiDerived,
                "analytics_derived",
                "Same shape at job grain. Holds the eligibility boolean, which is a decision "
                + "about an order rather than a fact about a person."),
            ["dim_program"] = new(
                DataProtectionClass.Operational,
                "analytics_derived",
                "Two programme labels and their row counts. Nothing here varies per order, so "
                + "there is nothing to re-identify from."),
            ["dim_script_variant"] = new(
                DataProtectionClass.Operational,
                "analytics_derived",
                "Variant identifiers from P2-7 and their row counts; the script text itself "
                + "lives in ivr_script_versions, not here."),
            ["dim_result_type"] = new(
                DataProtectionClass.Operational,
                "analytics_derived",
                "The DT-02 taxonomy and its row counts. A closed vocabulary defined in the "
                + "spec, not derived from any customer."),
            ["agg_kpi_daily"] = new(
                DataProtectionClass.Operational,
                "analytics_derived",
                "Counts per (date, programme, variant). No row of source survives into it, "
                + "which is what makes the k-anonymity threshold meaningful downstream."),
            ["etl_checkpoint"] = new(
                DataProtectionClass.Operational,
                "analytics_derived",
                "Pipeline bookkeeping: run times, row counts and the reconcile verdict. "
                + "Deliberately not a correctness input, so losing it costs a slow run."),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Every P1-5 retention class some table is governed by, primary and pre-deletion alike.
    /// A class the job executes that appears in neither is a deletion happening under no stated
    /// policy.
    /// </summary>
    public static FrozenSet<string> GovernedRetentionClasses { get; } = Tables.Values
        .SelectMany(entry => entry.PreDeletionAnonymizeClass is null
            ? new[] { entry.RetentionClass }
            : [entry.RetentionClass, entry.PreDeletionAnonymizeClass])
        .ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// Classes whose tables may never appear in an unencrypted backup artefact, even
    /// transiently. In practice this is every class — the split exists so the
    /// document can state <i>why</i> per class rather than asserting one blanket
    /// rule nobody can check.
    /// </summary>
    public static FrozenSet<DataProtectionClass> RequiresEncryptedBackup { get; } = new[]
    {
        DataProtectionClass.PiiDirect,
        DataProtectionClass.PiiDerived,
        DataProtectionClass.AuditTrail,
        DataProtectionClass.Operational,
        DataProtectionClass.Configuration,
    }.ToFrozenSet();

    /// <summary>
    /// Tables a reporting reader may be granted. Deliberately derived from the map
    /// rather than listed: the grant follows the classification, so it cannot drift
    /// away from it.
    /// </summary>
    public static IReadOnlyList<string> ReportingReadableTables { get; } = Tables
        .Where(pair => string.Equals(pair.Value.RetentionClass, "analytics_derived", StringComparison.Ordinal))
        .Select(pair => pair.Key)
        .Order(StringComparer.Ordinal)
        .ToArray();

    /// <summary>
    /// Returns every physical table in <paramref name="model"/> that has no entry
    /// here. Empty means the classification covers the schema that ships.
    /// </summary>
    public static IReadOnlyList<string> FindUnclassifiedTables(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(table => table is not null)
            .Select(table => table!)
            .Distinct(StringComparer.Ordinal)
            .Where(table => !Tables.ContainsKey(table))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Returns entries naming a table the model no longer has. A stale entry is not
    /// harmless: it makes the coverage count look right while describing something
    /// that is gone.
    /// </summary>
    public static IReadOnlyList<string> FindStaleEntries(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        HashSet<string> shipped = model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(table => table is not null)
            .Select(table => table!)
            .ToHashSet(StringComparer.Ordinal);

        return Tables.Keys
            .Where(table => !shipped.Contains(table))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}

/// <param name="Protection">What must be true while the data is here.</param>
/// <param name="RetentionClass">The P1-5 class that decides when the row goes away.</param>
/// <param name="Reason">Why this class, in terms a reviewer can disagree with.</param>
/// <param name="PreDeletionAnonymizeClass">
/// A second P1-5 class that redacts fields <i>inside</i> the row before the row itself is
/// eligible for deletion. A table can be governed by two classes on different clocks, and
/// modelling it as one was a defect <c>COMP-RETENTION-04</c> caught: <c>speech_snapshot</c>
/// executes against <c>ivr_confirmation_tasks</c> long before <c>task_metadata</c> removes it,
/// and a single-valued field made that class look like one nobody had classified.
/// </param>
public sealed record DataClassEntry(
    DataProtectionClass Protection,
    string RetentionClass,
    string Reason,
    string? PreDeletionAnonymizeClass = null);
