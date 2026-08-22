using System.Collections.Frozen;

namespace Ivr.Domain.Retention;

/// <summary>
/// Canonical data classes managed by the P1-5 retention job.
/// </summary>
public static class RetentionDataClasses
{
    public const string TaskMetadata = "task_metadata";
    public const string AttemptMetadata = "attempt_metadata";
    public const string ResultMetadata = "result_metadata";
    public const string CallbackMetadata = "callback_metadata";
    public const string RawCallEvent = "raw_call_event";
    public const string SpeechSnapshot = "speech_snapshot";
    public const string EvidenceLink = "evidence_link";
    public const string IdempotencyKey = "idempotency_key";
    public const string ReviewItem = "review_item";
    public const string StaffAccount = "staff_account";
    public const string ConsoleSession = "console_session";

    /// <summary>
    /// Child-first order prevents parent rows from being removed ahead of dependent evidence.
    /// </summary>
    public static IReadOnlyList<string> DefaultExecutionOrder { get; } =
    [
        CallbackMetadata,
        RawCallEvent,
        AttemptMetadata,
        ResultMetadata,
        SpeechSnapshot,
        EvidenceLink,
        IdempotencyKey,
        ReviewItem,
        ConsoleSession,
        StaffAccount,
        TaskMetadata,
    ];

    /// <summary>
    /// All supported data-class identifiers.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = DefaultExecutionOrder
        .ToFrozenSet(StringComparer.Ordinal);
}
