using Ivr.Domain.Retention;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// Keeps the warehouse from outliving its source (DF-07).
///
/// <para>Derived data is the usual way a retention policy quietly fails: the
/// operational row is deleted on schedule, the copy in the reporting store is
/// not, and the deletion becomes a rename. So the rule here is not a second
/// period to configure — it is a <b>dependency</b>: a fact exists only while the
/// result it was built from exists. That makes the analytics period equal to the
/// source period automatically, and there is no way to set the two
/// inconsistently because there is only one.</para>
///
/// <para>Runs as a purge hook, after the retention job has finished deleting the
/// operational classes, so the orphans it looks for are exactly the ones that run
/// created. Honours dry-run: it reports the count it would delete and deletes
/// nothing, because a scheduled customer-data delete whose default is wrong is
/// not recoverable.</para>
/// </summary>
public sealed class AnalyticsRetentionHook(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IRetentionPurgeHook
{
    public string Name => "analytics_warehouse";

    public async Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        IQueryable<AnalyticsFactCallOutcomeEntity> orphans = context.AnalyticsFacts
            .Where(fact => !context.CallResults
                .Any(result => result.IvrCallResultId == fact.IvrCallResultId));

        IQueryable<AnalyticsFactCallJobEntity> orphanJobs = context.AnalyticsJobFacts
            .Where(fact => !context.CallJobs
                .Any(job => job.IvrCallJobId == fact.IvrCallJobId));

        if (dryRun)
        {
            int wouldDeleteResults = await orphans.CountAsync(cancellationToken)
                .ConfigureAwait(false);
            int wouldDeleteJobs = await orphanJobs.CountAsync(cancellationToken)
                .ConfigureAwait(false);
            return wouldDeleteResults + wouldDeleteJobs;
        }

        // Captured before the delete: afterwards the rows are gone and there is nothing left to
        // tell the aggregates which buckets are now wrong.
        DateOnly[] affected = await orphans
            .Select(fact => fact.EventDate)
            .Distinct()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        int deleted = await orphans.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        deleted += await orphanJobs.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        if (affected.Length > 0)
        {
            // Without this the KPI buckets would keep reporting the deleted calls. The counts
            // are what a reader actually looks at, so leaving them stale would preserve exactly
            // the number the retention run was supposed to remove.
            await AnalyticsEtlJob.RecomputeAggregatesAsync(
                context,
                affected,
                timeProvider.GetUtcNow(),
                cancellationToken).ConfigureAwait(false);
        }

        return deleted;
    }
}
