using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// The P10-4 ETL (<c>W-0055</c>): operational call results in, PII-free star
/// schema out.
///
/// <para><b>Why there is no time watermark.</b> The obvious design keeps the last
/// processed <c>created_at</c> and reads forward from it. That design loses rows,
/// and not rarely: two transactions can take timestamps in one order and commit in
/// the other, so a row whose <c>created_at</c> is already behind the watermark can
/// appear after the watermark moved past it. Nothing ever reads it again, and
/// nothing reports it missing — the KPI is simply wrong by an amount no one can
/// measure.</para>
///
/// <para>So selection is an <b>anti-join on the natural key</b>: load every source
/// result that has no fact row yet. Ordering and commit time stop mattering, a
/// replay is exactly-once by construction rather than by convention, and the
/// checkpoint becomes pure observability — deleting it costs one slower run, not
/// one missing fact. The cost is a full anti-join per run instead of a range scan;
/// the source is bounded by retention (DF-07), and correctness is worth more than
/// the scan.</para>
///
/// <para><b>Aggregates are recomputed, never incremented.</b> Each touched
/// (date, program, variant) bucket is rebuilt from the facts it covers. An
/// increment would be faster and would double-count the first time anything ran
/// twice — which, for a pipeline whose contract is idempotency, is the one bug
/// that must be impossible rather than tested for.</para>
///
/// <para><b>Reads only.</b> This never writes an operational table, never touches
/// audit or evidence, and never calls out (D-14). The direction of the arrow is
/// the whole safety argument.</para>
/// </summary>
public sealed class AnalyticsEtlJob(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IAnalyticsEtlJob
{
    public const string PipelineName = "call_outcome";

    private const string UnknownVariant = "UNKNOWN";

    public async Task<AnalyticsEtlRunReport> RunAsync(
        AnalyticsEtlRunOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.BatchSize is < 1 or > 50_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Analytics ETL batch size must be between 1 and 50,000.");
        }

        long startedTicks = Stopwatch.GetTimestamp();
        DateTimeOffset now = options.Now ?? timeProvider.GetUtcNow();

        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        (List<AnalyticsFactCallOutcomeEntity> loaded, int rejected) =
            await ExtractAsync(context, options.BatchSize, now, cancellationToken)
                .ConfigureAwait(false);

        if (loaded.Count > 0)
        {
            context.AnalyticsFacts.AddRange(loaded);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await UpsertDimensionsAsync(context, loaded, now, cancellationToken)
                .ConfigureAwait(false);
        }

        (int jobsInserted, int jobsRefreshed) =
            await SyncJobFactsAsync(context, options.BatchSize, now, cancellationToken)
                .ConfigureAwait(false);

        int buckets = await RecomputeAggregatesAsync(
            context,
            options.RebuildAggregates
                ? null
                : loaded.Select(fact => fact.EventDate).Distinct().ToArray(),
            now,
            cancellationToken).ConfigureAwait(false);

        (int sourceRows, int orphanRows) = await CountSourceAsync(context, cancellationToken)
            .ConfigureAwait(false);
        int factRows = await context.AnalyticsFacts.CountAsync(cancellationToken)
            .ConfigureAwait(false);

        // A row rejected by the privacy filter is deliberately absent from the facts, so it is
        // subtracted before the counts are compared. Otherwise every rejection would masquerade
        // as a pipeline fault and the real signal would be lost inside the noise.
        long totalRejected = await ResolveTotalRejectedAsync(context, rejected, cancellationToken)
            .ConfigureAwait(false);

        string status = ResolveStatus(sourceRows, orphanRows, factRows, totalRejected);
        long durationMs = (long)Stopwatch.GetElapsedTime(startedTicks).TotalMilliseconds;

        await WriteCheckpointAsync(
            context,
            now,
            loaded,
            rejected,
            durationMs,
            sourceRows,
            factRows,
            status,
            cancellationToken).ConfigureAwait(false);

        return new AnalyticsEtlRunReport(
            loaded.Count,
            rejected,
            buckets,
            sourceRows,
            factRows,
            orphanRows,
            status,
            durationMs,
            jobsInserted,
            jobsRefreshed);
    }

    // ----------------------------------------------------------------- job grain

    /// <summary>
    /// Loads and maintains the job-grain fact.
    ///
    /// <para>A call job is not immutable the way a result is: attempts accumulate
    /// and eligibility is decided after the row exists. Insert-only would therefore
    /// freeze whatever the job looked like the first time the ETL happened to see
    /// it, and the attempt-2 KPI would read low forever with nothing to indicate
    /// it.</para>
    ///
    /// <para>So the pass is two-part and both parts are idempotent: insert jobs
    /// with no fact, then re-read the ones still open. <c>ClosedAt</c> is the
    /// boundary — once set, the job is finished and re-reading it would be work
    /// with no possible effect.</para>
    ///
    /// <para><b>Stated limit:</b> the reconcile compares row counts, and a stale
    /// open-job row has the right count with the wrong contents. If <c>ClosedAt</c>
    /// were ever set while a job could still change, this pass would go stale
    /// silently. <c>BI-IDEMP-03</c> covers the refresh; nothing covers that
    /// premise.</para>
    /// </summary>
    private static async Task<(int Inserted, int Refreshed)> SyncJobFactsAsync(
        IvrDbContext context,
        int batchSize,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var missing = await context.CallJobs.AsNoTracking()
            .Where(job => !context.AnalyticsJobFacts
                .Any(fact => fact.IvrCallJobId == job.IvrCallJobId))
            .OrderBy(job => job.CreatedAt)
            .ThenBy(job => job.IvrCallJobId)
            .Take(batchSize)
            .Select(job => new JobProjection(
                job.IvrCallJobId,
                job.OfficialOrderId,
                job.ProgramType,
                job.ScriptVersion,
                job.Eligible,
                job.CreatedAt,
                job.ClosedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AnalyticsFactCallJobEntity> openFacts = await context.AnalyticsJobFacts
            .Where(fact => !fact.Closed)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        string[] refreshIds = openFacts.Select(fact => fact.IvrCallJobId).ToArray();
        Dictionary<string, JobProjection> refreshSource = refreshIds.Length == 0
            ? []
            : await context.CallJobs.AsNoTracking()
                .Where(job => refreshIds.Contains(job.IvrCallJobId))
                .Select(job => new JobProjection(
                    job.IvrCallJobId,
                    job.OfficialOrderId,
                    job.ProgramType,
                    job.ScriptVersion,
                    job.Eligible,
                    job.CreatedAt,
                    job.ClosedAt))
                .ToDictionaryAsync(job => job.JobId, cancellationToken)
                .ConfigureAwait(false);

        string[] countIds = missing.Select(job => job.JobId).Concat(refreshIds).Distinct().ToArray();
        Dictionary<string, int> attemptCounts = countIds.Length == 0
            ? []
            : await context.CallAttempts.AsNoTracking()
                .Where(attempt => attempt.IsCountedCustomerAttempt
                    && countIds.Contains(attempt.IvrCallJobId))
                .GroupBy(attempt => attempt.IvrCallJobId)
                .Select(group => new { JobId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken)
                .ConfigureAwait(false);

        int inserted = 0;
        foreach (JobProjection job in missing)
        {
            var fact = new AnalyticsFactCallJobEntity
            {
                IvrCallJobId = job.JobId,
                OrderRefHash = HashOrderRef(job.OfficialOrderId),
                ProgramKey = job.ProgramType,
                ScriptVariantKey = string.IsNullOrWhiteSpace(job.ScriptVersion)
                    ? UnknownVariant
                    : job.ScriptVersion,
                Eligible = job.Eligible,
                CountedAttemptCount = attemptCounts.GetValueOrDefault(job.JobId),
                Closed = job.ClosedAt is not null,
                CreatedAt = job.CreatedAt.ToUniversalTime(),
                CreatedDate = DateOnly.FromDateTime(job.CreatedAt.UtcDateTime),
                LoadedAt = now,
            };

            if (!IsJobSafeToLoad(fact))
            {
                continue;
            }

            context.AnalyticsJobFacts.Add(fact);
            inserted++;
        }

        int refreshed = 0;
        foreach (AnalyticsFactCallJobEntity fact in openFacts)
        {
            if (!refreshSource.TryGetValue(fact.IvrCallJobId, out JobProjection? job))
            {
                // The job is gone. The retention hook owns the delete; touching it here would
                // put two owners on the same row.
                continue;
            }

            int attempts = attemptCounts.GetValueOrDefault(fact.IvrCallJobId);
            bool closed = job.ClosedAt is not null;
            if (fact.CountedAttemptCount == attempts
                && fact.Eligible == job.Eligible
                && fact.Closed == closed)
            {
                continue;
            }

            fact.CountedAttemptCount = attempts;
            fact.Eligible = job.Eligible;
            fact.Closed = closed;
            fact.LoadedAt = now;
            refreshed++;
        }

        if (inserted > 0 || refreshed > 0)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (inserted, refreshed);
    }

    private static bool IsJobSafeToLoad(AnalyticsFactCallJobEntity fact) =>
        AnalyticsColumnPolicy.InspectValue(fact.IvrCallJobId)
        && AnalyticsColumnPolicy.InspectValue(fact.OrderRefHash)
        && AnalyticsColumnPolicy.InspectValue(fact.ProgramKey)
        && AnalyticsColumnPolicy.InspectValue(fact.ScriptVariantKey);

    private sealed record JobProjection(
        string JobId,
        string OfficialOrderId,
        string ProgramType,
        string ScriptVersion,
        bool Eligible,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt);

    // ------------------------------------------------------------------ extract

    private static async Task<(List<AnalyticsFactCallOutcomeEntity> Loaded, int Rejected)>
        ExtractAsync(
            IvrDbContext context,
            int batchSize,
            DateTimeOffset now,
            CancellationToken cancellationToken)
    {
        // The anti-join. AnalyticsFacts is the set of results already represented, so what comes
        // back is exactly the outstanding work — regardless of when anything committed.
        var candidates = await (
            from result in context.CallResults.AsNoTracking()
            join job in context.CallJobs.AsNoTracking()
                on result.IvrCallJobId equals job.IvrCallJobId
            where !context.AnalyticsFacts
                .Any(fact => fact.IvrCallResultId == result.IvrCallResultId)
            orderby result.CreatedAt, result.IvrCallResultId
            select new
            {
                result.IvrCallResultId,
                result.IvrCallJobId,
                result.OfficialOrderId,
                result.ResultType,
                result.FinalResultStatus,
                result.DtmfKey,
                result.IsFinalForIvr,
                result.IsCountedCustomerAttempt,
                result.CreatedAt,
                job.ProgramType,
                job.ScriptVersion,
                job.T0At,
            })
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (candidates.Count == 0)
        {
            return ([], 0);
        }

        string[] jobIds = candidates.Select(row => row.IvrCallJobId).Distinct().ToArray();

        // Counted customer attempts only. A technical retry is not a second attempt at the
        // customer (DT-02), and counting it would inflate the attempt-2 KPI silently.
        Dictionary<string, int> attemptCounts = await context.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IsCountedCustomerAttempt
                && jobIds.Contains(attempt.IvrCallJobId))
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new { JobId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken)
            .ConfigureAwait(false);

        List<AnalyticsFactCallOutcomeEntity> loaded = new(candidates.Count);
        int rejected = 0;

        foreach (var row in candidates)
        {
            DateTimeOffset eventAt = row.CreatedAt.ToUniversalTime();
            double seconds = (eventAt - row.T0At.ToUniversalTime()).TotalSeconds;

            var fact = new AnalyticsFactCallOutcomeEntity
            {
                IvrCallResultId = row.IvrCallResultId,
                IvrCallJobId = row.IvrCallJobId,
                OrderRefHash = HashOrderRef(row.OfficialOrderId),
                ProgramKey = row.ProgramType,
                ScriptVariantKey = string.IsNullOrWhiteSpace(row.ScriptVersion)
                    ? UnknownVariant
                    : row.ScriptVersion,
                ResultTypeKey = row.ResultType,
                FinalResultStatus = row.FinalResultStatus,
                DtmfKey = string.IsNullOrWhiteSpace(row.DtmfKey) ? null : row.DtmfKey,
                IsFinal = row.IsFinalForIvr,
                IsCountedCustomerAttempt = row.IsCountedCustomerAttempt,
                CountedAttemptNumber = attemptCounts.GetValueOrDefault(row.IvrCallJobId),
                EventAt = eventAt,
                EventDate = DateOnly.FromDateTime(eventAt.UtcDateTime),
                EventHour = eventAt.Hour,
                // Negative means the source clocks disagree. Recording null keeps a nonsense
                // duration out of the average instead of dragging it below zero.
                SecondsToResult = seconds is >= 0 and <= int.MaxValue ? (int)seconds : null,
                LoadedAt = now,
            };

            if (!IsSafeToLoad(fact))
            {
                rejected++;
                continue;
            }

            loaded.Add(fact);
        }

        return (loaded, rejected);
    }

    /// <summary>
    /// Layer 2 of the privacy filter, applied to the values this run would write.
    /// Every string on the fact is inspected — including the ones that come from
    /// bounded enumerations upstream, because "bounded upstream" is an assumption
    /// about other code, and this is the last place it can be checked.
    /// </summary>
    private static bool IsSafeToLoad(AnalyticsFactCallOutcomeEntity fact) =>
        AnalyticsColumnPolicy.InspectValue(fact.IvrCallResultId)
        && AnalyticsColumnPolicy.InspectValue(fact.IvrCallJobId)
        && AnalyticsColumnPolicy.InspectValue(fact.OrderRefHash)
        && AnalyticsColumnPolicy.InspectValue(fact.ProgramKey)
        && AnalyticsColumnPolicy.InspectValue(fact.ScriptVariantKey)
        && AnalyticsColumnPolicy.InspectValue(fact.ResultTypeKey)
        && AnalyticsColumnPolicy.InspectValue(fact.FinalResultStatus)
        && AnalyticsColumnPolicy.InspectValue(fact.DtmfKey);

    private static string HashOrderRef(string? officialOrderId)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(officialOrderId ?? string.Empty));
        return Convert.ToHexStringLower(hash);
    }

    // --------------------------------------------------------------- dimensions

    private static async Task UpsertDimensionsAsync(
        IvrDbContext context,
        List<AnalyticsFactCallOutcomeEntity> loaded,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var group in loaded.GroupBy(fact => fact.ProgramKey, StringComparer.Ordinal))
        {
            AnalyticsDimProgramEntity? dim = await context.AnalyticsPrograms
                .FirstOrDefaultAsync(row => row.ProgramKey == group.Key, cancellationToken)
                .ConfigureAwait(false);
            if (dim is null)
            {
                dim = new AnalyticsDimProgramEntity { ProgramKey = group.Key, FirstSeenAt = now };
                context.AnalyticsPrograms.Add(dim);
            }

            dim.LastSeenAt = now;
            dim.FactRowCount += group.Count();
        }

        foreach (var group in loaded.GroupBy(fact => fact.ScriptVariantKey, StringComparer.Ordinal))
        {
            AnalyticsDimScriptVariantEntity? dim = await context.AnalyticsScriptVariants
                .FirstOrDefaultAsync(row => row.ScriptVariantKey == group.Key, cancellationToken)
                .ConfigureAwait(false);
            if (dim is null)
            {
                dim = new AnalyticsDimScriptVariantEntity
                {
                    ScriptVariantKey = group.Key,
                    FirstSeenAt = now,
                };
                context.AnalyticsScriptVariants.Add(dim);
            }

            dim.LastSeenAt = now;
            dim.FactRowCount += group.Count();
        }

        foreach (var group in loaded.GroupBy(fact => fact.ResultTypeKey, StringComparer.Ordinal))
        {
            AnalyticsDimResultTypeEntity? dim = await context.AnalyticsResultTypes
                .FirstOrDefaultAsync(row => row.ResultTypeKey == group.Key, cancellationToken)
                .ConfigureAwait(false);
            if (dim is null)
            {
                dim = new AnalyticsDimResultTypeEntity
                {
                    ResultTypeKey = group.Key,
                    FirstSeenAt = now,
                };
                context.AnalyticsResultTypes.Add(dim);
            }

            dim.LastSeenAt = now;
            dim.IsFinal = group.Any(fact => fact.IsFinal);
            dim.FactRowCount += group.Count();
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // --------------------------------------------------------------- aggregates

    /// <summary>
    /// Rebuilds KPI buckets from facts. <paramref name="dates"/> null means every
    /// bucket; otherwise only the dates this run touched. Either way each bucket
    /// is deleted and recomputed, so running twice produces the same numbers.
    /// </summary>
    internal static async Task<int> RecomputeAggregatesAsync(
        IvrDbContext context,
        DateOnly[]? dates,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (dates is { Length: 0 })
        {
            return 0;
        }

        IQueryable<AnalyticsFactCallOutcomeEntity> facts = context.AnalyticsFacts.AsNoTracking();
        IQueryable<AnalyticsKpiDailyEntity> stale = context.AnalyticsKpiDaily;
        if (dates is not null)
        {
            facts = facts.Where(fact => dates.Contains(fact.EventDate));
            stale = stale.Where(row => dates.Contains(row.BucketDate));
        }

        await stale.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        // Materialized before grouping: the distinct-order count and the taxonomy splits are
        // clearer in memory than as a translated aggregate, and the set is one batch of dates.
        List<AnalyticsFactCallOutcomeEntity> rows = await facts
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<AnalyticsKpiDailyEntity> buckets = AnalyticsKpiMath.Fold(rows, now);

        context.AnalyticsKpiDaily.AddRange(buckets);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return buckets.Count;
    }

    // ----------------------------------------------------------------- reconcile

    private static async Task<(int SourceRows, int OrphanRows)> CountSourceAsync(
        IvrDbContext context,
        CancellationToken cancellationToken)
    {
        int total = await context.CallResults.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        // A result whose job is gone cannot be projected — the fact needs the program and the
        // script variant. Counting it separately is what keeps the reconcile honest: without
        // this the pipeline would report MISMATCH forever and nobody would know why.
        int orphan = await context.CallResults.AsNoTracking()
            .CountAsync(
                result => !context.CallJobs.Any(job => job.IvrCallJobId == result.IvrCallJobId),
                cancellationToken)
            .ConfigureAwait(false);

        return (total, orphan);
    }

    private static string ResolveStatus(
        int sourceRows,
        int orphanRows,
        int factRows,
        long totalRejected)
    {
        long expected = sourceRows - orphanRows - totalRejected;
        if (factRows == expected)
        {
            return AnalyticsReconcileStatus.Complete;
        }

        return factRows < expected
            ? AnalyticsReconcileStatus.Backlog
            : AnalyticsReconcileStatus.Mismatch;
    }

    private static async Task<long> ResolveTotalRejectedAsync(
        IvrDbContext context,
        int rejectedThisRun,
        CancellationToken cancellationToken)
    {
        AnalyticsEtlCheckpointEntity? checkpoint = await context.AnalyticsCheckpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.PipelineName == PipelineName, cancellationToken)
            .ConfigureAwait(false);

        return (checkpoint?.TotalRejectedRows ?? 0) + rejectedThisRun;
    }

    private static async Task WriteCheckpointAsync(
        IvrDbContext context,
        DateTimeOffset now,
        List<AnalyticsFactCallOutcomeEntity> loaded,
        int rejected,
        long durationMs,
        int sourceRows,
        int factRows,
        string status,
        CancellationToken cancellationToken)
    {
        AnalyticsEtlCheckpointEntity? checkpoint = await context.AnalyticsCheckpoints
            .FirstOrDefaultAsync(row => row.PipelineName == PipelineName, cancellationToken)
            .ConfigureAwait(false);

        if (checkpoint is null)
        {
            checkpoint = new AnalyticsEtlCheckpointEntity { PipelineName = PipelineName };
            context.AnalyticsCheckpoints.Add(checkpoint);
        }

        DateTimeOffset? batchHighWater = loaded.Count == 0
            ? null
            : loaded.Max(fact => fact.EventAt);

        checkpoint.LastRunAt = now;
        checkpoint.LastRunLoadedRows = loaded.Count;
        checkpoint.LastRunRejectedRows = rejected;
        checkpoint.LastRunDurationMs = durationMs;
        checkpoint.TotalLoadedRows += loaded.Count;
        checkpoint.TotalRejectedRows += rejected;
        if (batchHighWater is not null
            && (checkpoint.HighWaterEventAt is null
                || batchHighWater > checkpoint.HighWaterEventAt))
        {
            // Only ever forward. A late row carries an older event time, and letting it drag the
            // high-water mark back would make freshness read as a regression rather than as the
            // backfill it is.
            checkpoint.HighWaterEventAt = batchHighWater;
        }

        checkpoint.LastReconciledAt = now;
        checkpoint.SourceRowCount = sourceRows;
        checkpoint.FactRowCount = factRows;
        checkpoint.ReconcileStatus = status;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Culture-independent bucket label used by the KPI catalog examples.</summary>
    public static string FormatBucket(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
