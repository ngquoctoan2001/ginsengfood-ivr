using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.Infrastructure.Governance;

/// <summary>
/// Legal basis for one field. The confirmation call is a step in performing a
/// contract the customer already entered into; nothing here rests on consent,
/// because IVR never asks for any and a basis nobody obtained is not a basis.
/// </summary>
public enum PersonalDataLegalBasis
{
    /// <summary>
    /// Processing necessary to perform the order the customer placed. Confirming a
    /// COD order is part of fulfilling it, not marketing about it — which is also
    /// why do-not-call for <c>PHONE_CALL</c> is honoured as a hard block rather
    /// than balanced against a legitimate interest.
    /// </summary>
    ContractPerformance,

    /// <summary>
    /// Retained because a record of what was decided must exist independently of
    /// the party it concerns. Audit rows are append-only and survive erasure, and
    /// that is a deliberate limit on the erasure right, stated rather than hidden.
    /// </summary>
    LegalRecordKeeping,
}

/// <param name="Table">Physical table.</param>
/// <param name="Column">Physical column.</param>
/// <param name="Purpose">Why IVR holds it, in one sentence.</param>
/// <param name="Basis">Legal basis.</param>
/// <param name="ErasureBehaviour">What a DSAR erasure does to it, and why.</param>
public sealed record PersonalDataField(
    string Table,
    string Column,
    string Purpose,
    PersonalDataLegalBasis Basis,
    string ErasureBehaviour);

/// <summary>
/// The P10-1 field-level inventory (<c>W-0052</c>), and the gate that keeps it true.
///
/// <para><see cref="DataClassification"/> answers "what class is this table".
/// This answers the question a regulator actually asks: <i>which fields hold
/// personal data, why do you have them, and what happens when someone asks you to
/// delete them</i>. A table-level answer cannot address any of the three.</para>
///
/// <para>The gate is <see cref="FindUninventoriedFields"/>: any column whose name
/// indicates personal data must appear here. It is a name heuristic and it is
/// deliberately blunt — the failure it prevents is somebody adding
/// <c>customer_email</c> in a migration and nobody updating the inventory, and a
/// blunt rule catches that while a precise one requires the very knowledge that
/// went missing.</para>
/// </summary>
public static class PersonalDataInventory
{
    /// <summary>
    /// Column-name fragments that indicate personal data. A column matching one of
    /// these must be inventoried or explicitly exempted.
    /// </summary>
    public static FrozenSet<string> PersonalDataIndicators { get; } = new[]
    {
        "phone", "msisdn", "contact", "customer", "address", "email", "recording",
        "dial_token", "speech", "summary", "order_code", "transcript",
    }.ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// Columns whose name matches an indicator and which hold no personal data,
    /// each with the reason. Same discipline as the analytics exemptions: named,
    /// keyed, justified, and checked against the shipped schema.
    /// </summary>
    public static FrozenDictionary<string, string> NonPersonalExemptions { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ivr_call_results.is_counted_customer_attempt"] =
                "Attempt accounting (DT-02): whether this result consumed a counted customer "
                + "attempt. A boolean about the attempt policy, not about a person.",
            ["ivr_call_attempts.is_counted_customer_attempt"] =
                "Same boolean at attempt grain.",
            ["fact_call_outcome.is_counted_customer_attempt"] =
                "Same boolean, copied into the analytics fact.",
            ["ivr_sim_channels.sim_number_ref"] =
                "Reference to a SIM the business owns, resolvable only inside the adapter vault. "
                + "It identifies an outbound channel, never a subscriber (D-05).",
            ["ivr_technical_exceptions.customer_attempt_counted"] =
                "Whether the fault consumed a counted customer attempt (DT-02). A boolean about "
                + "the attempt policy, not about a person.",
            ["ivr_call_attempts.invalid_phone"] =
                "Outcome flag: the carrier rejected the number as unreachable. Records that a "
                + "dial failed, and holds no number of any kind.",
            ["agg_kpi_daily.invalid_phone_count"] =
                "Count of results with type IVR_INVALID_PHONE_FINAL per reporting bucket. An "
                + "integer; the taxonomy name is what carries the word.",
            ["ivr_confirmation_tasks.dial_token_expires_at"] =
                "Expiry timestamp of the dialling token. A time, not the token, and the token it "
                + "describes is itself only a ciphertext.",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Every field IVR holds that relates to an identifiable person, directly or
    /// through a key it does not control.
    /// </summary>
    public static IReadOnlyList<PersonalDataField> Fields { get; } =
    [
        new("ivr_confirmation_tasks", "phone_ref",
            "Opaque reference the SIM adapter resolves to a number at dial time. IVR never sees "
            + "the number itself (D-05).",
            PersonalDataLegalBasis.ContractPerformance,
            "Replaced with a redacted value by the speech_snapshot anonymisation strategy."),
        new("ivr_confirmation_tasks", "phone_masked",
            "Display form for the console, so an operator can tell two orders apart without "
            + "seeing a number.",
            PersonalDataLegalBasis.ContractPerformance,
            "Replaced with a redacted value."),
        new("ivr_confirmation_tasks", "dial_token_ciphertext",
            "Encrypted one-use dialling token, TTL bounded by the confirmation window.",
            PersonalDataLegalBasis.ContractPerformance,
            "Replaced with a redacted value; the token has expired long before erasure is possible."),
        new("ivr_confirmation_tasks", "privacy_safe_order_summary_json",
            "The whitelisted fields the script may read aloud. Contains no address, no payment "
            + "detail and no health note (OD-V1-15).",
            PersonalDataLegalBasis.ContractPerformance,
            "Replaced with a redacted value."),
        new("ivr_confirmation_tasks", "order_code",
            "Sales-owned business key, used to correlate with the order system.",
            PersonalDataLegalBasis.ContractPerformance,
            "Retained: it is the key a DSAR request arrives with, and erasing it would make the "
            + "next request unanswerable."),
        new("ivr_confirmation_tasks", "customer_id",
            "Sales-owned customer key, present only when the task carries one. IVR reads it and "
            + "never resolves it to a person.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted with the task row when its retention period expires."),
        new("ivr_confirmation_tasks", "customer_trust_status",
            "Trust signal from CRM, used to decide whether a confirmation call can be skipped "
            + "for a customer with an established history.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted with the task row when its retention period expires. Not separately "
            + "erasable: it is an input to a decision already recorded in audit."),
        new("ivr_confirmation_tasks", "official_contact_id",
            "Sales-owned contact key identifying which contact on the order is to be called.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted with the task row when its retention period expires."),
        new("ivr_confirmation_tasks", "phone_validation_status",
            "Whether the contact number passed validation. A fact about the customer's contact "
            + "details, so it is inventoried even though it holds no number.",
            PersonalDataLegalBasis.ContractPerformance,
            "Replaced with a redacted value by the speech_snapshot anonymisation strategy, "
            + "alongside the reference it describes."),
        new("ivr_raw_call_events", "recording_ref",
            "Reference to a recording. Recording is OFF by default (DT-05) and this is null "
            + "unless a separate approval exists.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted with the raw event row; no recording exists to erase in the first place."),
        new("ivr_idempotency_keys", "response_snapshot_json",
            "Stored response so a retried request returns the same answer. Its contents are "
            + "whatever the endpoint returned, which is why the table is classified PiiDirect.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted when the key expires; the shortest-lived personal data IVR holds."),
        new("ivr_result_callbacks", "payload_json",
            "Immutable copy of what was sent to Sales, kept so a delivery dispute has an answer.",
            PersonalDataLegalBasis.LegalRecordKeeping,
            "Deleted with the callback row at retention; not erasable earlier, because a "
            + "delivery record with the payload removed cannot settle the dispute it exists for."),
        new("ivr_audit_log", "actor_id",
            "Who performed an administrative action. Usually staff rather than a customer, but "
            + "it is personal data about whoever it names.",
            PersonalDataLegalBasis.LegalRecordKeeping,
            "NEVER erased. Append-only, enforced by the database: UPDATE and DELETE are refused. "
            + "A record of who did what that the subject can delete is not a record."),
        new("ivr_admin_actions", "actor_id",
            "Who performed an administrative action against a specific target, with the reason "
            + "they gave for it.",
            PersonalDataLegalBasis.LegalRecordKeeping,
            "NEVER erased, for the same reason as the audit log."),
        new("fact_call_outcome", "order_ref_hash",
            "SHA-256 of the Sales order id, so reporting can count distinct orders without "
            + "carrying the id.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted when the source result is deleted; the analytics retention hook makes the "
            + "warehouse period equal to the source period by construction."),
        new("fact_call_job", "order_ref_hash",
            "Same hash at job grain, so job-level KPIs can count distinct orders too.",
            PersonalDataLegalBasis.ContractPerformance,
            "Deleted when the source job is deleted, by the same retention hook."),
    ];

    /// <summary>
    /// Columns that look like personal data and are neither inventoried nor
    /// exempted. Empty means the inventory covers the schema that ships.
    /// </summary>
    public static IReadOnlyList<string> FindUninventoriedFields(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        HashSet<string> known = Fields
            .Select(field => $"{field.Table}.{field.Column}")
            .ToHashSet(StringComparer.Ordinal);

        List<string> missing = [];
        foreach (IEntityType entityType in model.GetEntityTypes())
        {
            string? table = entityType.GetTableName();
            if (table is null)
            {
                continue;
            }

            foreach (IProperty property in entityType.GetProperties())
            {
                string column = property.GetColumnName() ?? property.Name;
                string key = $"{table}.{column}";
                if (known.Contains(key) || NonPersonalExemptions.ContainsKey(key))
                {
                    continue;
                }

                if (PersonalDataIndicators.Any(indicator =>
                    column.Contains(indicator, StringComparison.OrdinalIgnoreCase)))
                {
                    missing.Add(key);
                }
            }
        }

        return missing.Order(StringComparer.Ordinal).ToArray();
    }

    /// <summary>
    /// Inventory entries and exemptions naming a column the model does not have.
    /// An inventory that outlives its schema describes a system nobody is running.
    /// </summary>
    public static IReadOnlyList<string> FindStaleEntries(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        HashSet<string> shipped = model.GetEntityTypes()
            .Where(entity => entity.GetTableName() is not null)
            .SelectMany(entity => entity.GetProperties()
                .Select(property =>
                    $"{entity.GetTableName()}.{property.GetColumnName() ?? property.Name}"))
            .ToHashSet(StringComparer.Ordinal);

        return Fields
            .Select(field => $"{field.Table}.{field.Column}")
            .Concat(NonPersonalExemptions.Keys)
            .Where(key => !shipped.Contains(key))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
