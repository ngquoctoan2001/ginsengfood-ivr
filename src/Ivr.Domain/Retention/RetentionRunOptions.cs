namespace Ivr.Domain.Retention;

/// <summary>
/// Controls one retention pass. The default never mutates data.
/// </summary>
/// <param name="DryRun">When true, only counts eligible rows.</param>
/// <param name="DataClasses">Optional subset; null or empty means all canonical classes.</param>
/// <param name="Now">Optional deterministic clock value; null uses the registered clock.</param>
/// <param name="BatchSize">Maximum rows changed in one short transaction.</param>
public sealed record RetentionRunOptions(
    bool DryRun = true,
    IReadOnlyCollection<string>? DataClasses = null,
    DateTimeOffset? Now = null,
    int BatchSize = 250);
