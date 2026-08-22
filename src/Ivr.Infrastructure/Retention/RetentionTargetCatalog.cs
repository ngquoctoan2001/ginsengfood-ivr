using Ivr.Domain.Retention;

namespace Ivr.Infrastructure.Retention;

internal sealed record RetentionTarget(
    string Segment,
    string TableName,
    string TimestampColumn,
    RetentionStrategy Strategy,
    string? ExtraEligibilitySql = null,
    string? ProtectedSql = null,
    string? DependencyBlockedSql = null,
    string? AnonymizeSetSql = null);

internal static class RetentionTargetCatalog
{
    /// <summary>
    /// The redaction applied to a confirmation task's speech snapshot, shared with the P10-1 DSAR
    /// erasure path (<c>W-0052</c>). One definition, so the scheduled path and the on-request path
    /// cannot come to disagree about which columns are personal data -- and if they did, the one
    /// that runs less often would be the stale one.
    ///
    /// phone_validation_status joined this list when the field inventory was built: it is a fact
    /// about the customer's contact details, so leaving VALID behind after redacting the reference
    /// it describes keeps a weak signal about a person whose data was supposed to be gone.
    /// </summary>
    internal const string SpeechSnapshotRedactionSql =
        "phone_ref = 'redacted', phone_masked = '***', "
        + "phone_validation_status = 'REDACTED', "
        + "dial_token_ciphertext = 'enc:redacted', "
        + "privacy_safe_order_summary_json = '{}'::jsonb";

    private static readonly Dictionary<string, IReadOnlyList<RetentionTarget>> Targets =
        new Dictionary<string, IReadOnlyList<RetentionTarget>>(StringComparer.Ordinal)
        {
            [RetentionDataClasses.CallbackMetadata] =
            [
                Delete("callbacks", "ivr_result_callbacks", "created_at"),
            ],
            [RetentionDataClasses.RawCallEvent] =
            [
                Delete("raw_call_events", "ivr_raw_call_events", "received_at"),
            ],
            [RetentionDataClasses.AttemptMetadata] =
            [
                Delete("technical_exceptions", "ivr_technical_exceptions", "created_at"),
                Delete(
                    "call_attempts",
                    "ivr_call_attempts",
                    "scheduled_at",
                    dependencyBlockedSql:
                        "EXISTS (SELECT 1 FROM ivr_raw_call_events child "
                        + "WHERE child.ivr_call_attempt_id = t.ivr_call_attempt_id) "
                        + "OR EXISTS (SELECT 1 FROM ivr_technical_exceptions child "
                        + "WHERE child.ivr_call_attempt_id = t.ivr_call_attempt_id)"),
            ],
            [RetentionDataClasses.ResultMetadata] =
            [
                Delete(
                    "call_results",
                    "ivr_call_results",
                    "created_at",
                    dependencyBlockedSql:
                        "EXISTS (SELECT 1 FROM ivr_result_callbacks child "
                        + "WHERE child.ivr_call_result_id = t.ivr_call_result_id)"),
            ],
            [RetentionDataClasses.SpeechSnapshot] =
            [
                Anonymize(
                    "confirmation_task_speech",
                    "ivr_confirmation_tasks",
                    "created_at",
                    SpeechSnapshotRedactionSql),
            ],
            [RetentionDataClasses.EvidenceLink] =
            [
                Delete(
                    "evidence_links",
                    "ivr_evidence_links",
                    "created_at",
                    protectedSql: "t.accepted_at IS NOT NULL"),
                Delete(
                    "evidence_registry",
                    "ivr_evidence",
                    "created_at",
                    protectedSql: "t.accepted_at IS NOT NULL"),
            ],
            [RetentionDataClasses.IdempotencyKey] =
            [
                Delete("idempotency_keys", "ivr_idempotency_keys", "created_at"),
            ],
            [RetentionDataClasses.ReviewItem] =
            [
                Anonymize(
                    "review_items",
                    "ivr_review_items",
                    "resolved_at",
                    "source_id = 'anonymized', reason = '[retained-record-anonymized]', "
                    + "assigned_to = NULL, resolution = NULL",
                    extraEligibilitySql: "t.resolved_at IS NOT NULL"),
            ],
            [RetentionDataClasses.ConsoleSession] =
            [
                Delete("console_sessions", "ivr_console_sessions", "expires_at"),
            ],
            [RetentionDataClasses.StaffAccount] =
            [
                Delete(
                    "console_accounts",
                    "ivr_console_accounts",
                    "deleted_at",
                    extraEligibilitySql: "t.deleted_at IS NOT NULL",
                    dependencyBlockedSql:
                        "EXISTS (SELECT 1 FROM ivr_console_sessions child "
                        + "WHERE child.account_id = t.id)"),
            ],
            [RetentionDataClasses.TaskMetadata] =
            [
                Delete("task_intake_outbox", "ivr_task_intake_outbox", "created_at"),
                Delete(
                    "call_jobs",
                    "ivr_call_jobs",
                    "created_at",
                    dependencyBlockedSql:
                        "EXISTS (SELECT 1 FROM ivr_call_attempts child "
                        + "WHERE child.ivr_call_job_id = t.ivr_call_job_id) "
                        + "OR EXISTS (SELECT 1 FROM ivr_call_results child "
                        + "WHERE child.ivr_call_job_id = t.ivr_call_job_id) "
                        + "OR EXISTS (SELECT 1 FROM ivr_task_intake_outbox child "
                        + "WHERE child.ivr_call_job_id = t.ivr_call_job_id)"),
                Delete(
                    "confirmation_tasks",
                    "ivr_confirmation_tasks",
                    "created_at",
                    dependencyBlockedSql:
                        "EXISTS (SELECT 1 FROM ivr_call_jobs child "
                        + "WHERE child.task_id = t.task_id) "
                        + "OR EXISTS (SELECT 1 FROM ivr_task_intake_outbox child "
                        + "WHERE child.task_id = t.task_id)"),
                Delete(
                    "capacity_incidents",
                    "ivr_capacity_incidents",
                    "opened_at",
                    extraEligibilitySql: "t.resolved_at IS NOT NULL"),
            ],
        };

    public static IReadOnlyList<RetentionTarget> Get(string dataClass) =>
        Targets.TryGetValue(dataClass, out IReadOnlyList<RetentionTarget>? targets)
            ? targets
            : throw new ArgumentOutOfRangeException(
                nameof(dataClass),
                dataClass,
                "Unknown retention data class.");

    public static RetentionStrategy GetStrategy(string dataClass)
    {
        IReadOnlyList<RetentionTarget> targets = Get(dataClass);
        RetentionStrategy strategy = targets[0].Strategy;
        if (targets.Any(target => target.Strategy != strategy))
        {
            throw new InvalidOperationException(
                $"Retention class '{dataClass}' has inconsistent target strategies.");
        }

        return strategy;
    }

    private static RetentionTarget Delete(
        string segment,
        string table,
        string timestamp,
        string? extraEligibilitySql = null,
        string? protectedSql = null,
        string? dependencyBlockedSql = null) => new(
            segment,
            table,
            timestamp,
            RetentionStrategy.Delete,
            extraEligibilitySql,
            protectedSql,
            dependencyBlockedSql);

    private static RetentionTarget Anonymize(
        string segment,
        string table,
        string timestamp,
        string setSql,
        string? extraEligibilitySql = null) => new(
            segment,
            table,
            timestamp,
            RetentionStrategy.Anonymize,
            extraEligibilitySql,
            AnonymizeSetSql: setSql);
}
