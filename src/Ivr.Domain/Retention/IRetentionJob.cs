namespace Ivr.Domain.Retention;

/// <summary>
/// Applies configured data-lifecycle policies without inventing legal retention periods.
/// </summary>
public interface IRetentionJob
{
    /// <summary>
    /// Runs one resumable retention pass. Dry-run is the safe default.
    /// </summary>
    public Task<RetentionRunReport> RunAsync(
        RetentionRunOptions options,
        CancellationToken cancellationToken);
}
