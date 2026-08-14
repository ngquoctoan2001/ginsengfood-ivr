namespace Ivr.Domain.Retention;

/// <summary>
/// Purges ephemeral data that is not represented by a PostgreSQL retention target.
/// </summary>
public interface IRetentionPurgeHook
{
    public string Name { get; }

    public Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken);
}
