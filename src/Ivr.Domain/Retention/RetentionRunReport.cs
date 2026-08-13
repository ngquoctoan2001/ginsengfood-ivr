namespace Ivr.Domain.Retention;

/// <summary>
/// Supported lifecycle mutations. Audit and accepted evidence are intentionally outside both.
/// </summary>
public enum RetentionStrategy
{
    Delete,
    Anonymize,
}

/// <summary>
/// Outcome for one data class in a retention run.
/// </summary>
public enum RetentionClassRunStatus
{
    NotConfigured,
    DryRun,
    Completed,
}

/// <summary>
/// Privacy-safe aggregate evidence for one data class.
/// </summary>
public sealed record RetentionClassReport(
    string DataClass,
    RetentionStrategy Strategy,
    RetentionClassRunStatus Status,
    long ExaminedCount,
    long DeletedCount,
    long AnonymizedCount,
    long LegalHoldCount,
    long ProtectedCount,
    long DependencyBlockedCount,
    double? NotConfiguredAgeDays = null);

/// <summary>
/// Privacy-safe aggregate report for one resumable retention run.
/// </summary>
public sealed record RetentionRunReport(
    Guid RunId,
    bool DryRun,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<RetentionClassReport> Classes)
{
    public long DeletedCount => Classes.Sum(item => item.DeletedCount);

    public long AnonymizedCount => Classes.Sum(item => item.AnonymizedCount);

    public long LegalHoldCount => Classes.Sum(item => item.LegalHoldCount);
}
