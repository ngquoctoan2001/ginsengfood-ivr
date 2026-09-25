using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    /// <summary>
    /// W-0355. The advisory lock every run takes first, keyed like the other locks in this
    /// codebase through <c>hashtextextended</c>. Public so a test can hold it the way a second
    /// worker would.
    /// </summary>
    public const string RunLockName = "ivr:analytics-etl:" + PipelineName;

    private const string UnknownVariant = "UNKNOWN";

    /// <summary>
    /// The warehouse column is <c>varchar(1)</c> under <c>ck_analytics_fact_dtmf</c>: it holds the key
    /// the customer actually pressed, or nothing. The operational column it is copied from is plain
    /// text, and <c>SanitizeDtmf</c> on the dispatch path writes the classifications <c>INVALID</c> and
    /// <c>NO_INPUT</c> into it -- both correct there, and neither of them a key. Copied across verbatim
    /// they overflow the column, and because the ETL loads a whole batch in one transaction a single
    /// such row stopped every later run too: the job failed and retried forever, so the warehouse
    /// silently stopped moving while every test stayed green.
    ///
    /// Nothing is lost by dropping them. "Pressed something that is not an option" and "pressed
    /// nothing" are already carried by <c>ResultTypeKey</c> and <c>FinalResultStatus</c>; this column
    /// answers only "which key", and for those two rows the honest answer is none.
    /// </summary>
    private static string? KeypadDigitOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length == 1 && (char.IsAsciiDigit(trimmed[0]) || trimmed[0] is '*' or '#')
            ? trimmed
            : null;
    }

    /// <summary>
    /// One run, counted on <c>ivr_analytics_etl_runs_total</c> by how it ended (<c>W-0055</c>).
    ///
    /// <para>The verdict is counted only after the checkpoint that carries it has been written,
    /// so the metric never reports a verdict the reporting API cannot also show. A run that throws
    /// is counted as <c>FAILED</c> -- including one refused for invalid options, because a
    /// misconfigured pipeline that fails every run must read as runs that never complete rather
    /// than as no runs at all. Cancellation during shutdown is not counted: stopping is not a
    /// failure, and counting it would put a failure on every deployment.</para>
    ///
    /// <para>Counted here rather than in the worker host that calls it: this is the only place
    /// that knows the verdict and sees every run, and it is reachable from the integration suite,
    /// which the internal host is not.</para>
    /// </summary>
    public async Task<AnalyticsEtlRunReport> RunAsync(
        AnalyticsEtlRunOptions options,
        CancellationToken cancellationToken)
    {
        AnalyticsEtlRunReport report;
        try
        {
            report = await RunOnceAsync(options, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            IvrTelemetry.RecordAnalyticsEtlRun(AnalyticsEtlRunOutcome.Failed);
            throw;
        }

        if (!report.Skipped)
        {
            IvrTelemetry.RecordAnalyticsEtlRun(report.ReconcileStatus);
        }

        return report;
    }

    private async Task<AnalyticsEtlRunReport> RunOnceAsync(
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
            .CreateDbContextAsync(cancellationToken);

        // W-0355. One run at a time across every worker. The anti-join makes a run exactly-once
        // against the runs before it, not against a run beside it: two workers read the same
        // missing results and both insert them, and the slower one fails on PK_fact_call_outcome
        // and is counted FAILED, as the two-worker soak of 2026-09-25 kept doing. A session lock,
        // because a run spans several transactions; the connection stays open so the lock and the
        // work share one session. A run that finds the lock taken does nothing, not even a
        // checkpoint, and is not counted: the run beside it is the one doing the work.
        await context.Database.OpenConnectionAsync(cancellationToken);
        if (!await TryTakeRunLockAsync(context, cancellationToken))
        {
            return new AnalyticsEtlRunReport(
                0,
                0,
                0,
                0,
                0,
                0,
                AnalyticsReconcileStatus.NotRun,
                (long)Stopwatch.GetElapsedTime(startedTicks).TotalMilliseconds,
                Skipped: true);
        }

        try
        {
            return await RunLockedAsync(context, options, startedTicks, now, cancellationToken);
        }
        finally
        {
            await ReleaseRunLockAsync(context);
        }
    }

    private static async Task<bool> TryTakeRunLockAsync(
        IvrDbContext context,
        CancellationToken cancellationToken) =>
        await context.Database.SqlQuery<bool>(
            $"SELECT pg_try_advisory_lock(hashtextextended({RunLockName}, 0)) AS \"Value\"")
            .SingleAsync(cancellationToken);

    /// <summary>
    /// Not cancellable: a run stopped at shutdown still gives the lock back. A connection that is
    /// no longer open took its session with it, and the server released the lock then; failing
    /// here would only hide the exception that broke the run.
    /// </summary>
    private static async Task ReleaseRunLockAsync(IvrDbContext context)
    {
        if (context.Database.GetDbConnection().State != ConnectionState.Open)
        {
            return;
        }

        try
        {
            await context.Database.SqlQuery<bool>(
                $"SELECT pg_advisory_unlock(hashtextextended({RunLockName}, 0)) AS \"Value\"")
                .SingleAsync(CancellationToken.None);
        }
        catch (Npgsql.NpgsqlException)
        {
            // The session broke between the run and this call; the lock ended with it.
        }
    }

    private static async Task<AnalyticsEtlRunReport> RunLockedAsync(
        IvrDbContext context,
        AnalyticsEtlRunOptions options,
        long startedTicks,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        (List<AnalyticsFactCallOutcomeEntity> loaded, int rejected) =
            await ExtractAsync(context, options.BatchSize, now, cancellationToken);

        if (loaded.Count > 0)
        {
            context.AnalyticsFacts.AddRange(loaded);
            await context.SaveChangesAsync(cancellationToken);
            await UpsertDimensionsAsync(context, loaded, now, cancellationToken);
        }

        JobGrainSync jobs =
            await SyncJobFactsAsync(context, options.BatchSize, now, cancellationToken);

        int buckets = await RecomputeAggregatesAsync(
            context,
            options.RebuildAggregates
                ? null
                : loaded.Select(fact => fact.EventDate).Distinct().ToArray(),
            now,
            cancellationToken);

        (int sourceRows, int orphanRows) = await CountSourceAsync(context, cancellationToken);
        int factRows = await context.AnalyticsFacts.CountAsync(cancellationToken);

        // A row rejected by the privacy filter is deliberately absent from the facts, so it is
        // subtracted before the counts are compared. Otherwise every rejection would masquerade
        // as a pipeline fault and the real signal would be lost inside the noise.
        //
        // This run's count, not the checkpoint's running total. A rejected row never gets a fact,
        // so the anti-join hands it back and the filter refuses it again on every run: `rejected`
        // already counts every row currently refused. Adding the total on top counted the same row
        // once per run, so from the second run on the expected count fell below the fact count and
        // one refused row turned into a MISMATCH that never cleared (W-0055).
        string status = ResolveStatus(sourceRows, orphanRows, factRows, rejected, jobs);
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
            cancellationToken);

        return new AnalyticsEtlRunReport(
            loaded.Count,
            rejected,
            buckets,
            sourceRows,
            factRows,
            orphanRows,
            status,
            durationMs,
            jobs.Inserted,
            jobs.Refreshed,
            jobs.SourceJobs,
            jobs.JobFacts,
            jobs.Rejected,
            jobs.DriftedAfterRefresh);
    }

    // ----------------------------------------------------------------- job grain

    /// <summary>
    /// Loads and maintains the job-grain fact, and reconciles it against its source.
    ///
    /// <para>A call job is not immutable the way a result is: attempts accumulate
    /// and eligibility is decided after the row exists. Insert-only would therefore
    /// freeze whatever the job looked like the first time the ETL happened to see
    /// it, and the attempt-2 KPI would read low forever with nothing to indicate
    /// it.</para>
    ///
    /// <para>So the pass is two-part and both parts are idempotent: insert jobs
    /// with no fact, then refresh every fact whose eligible, closed or counted-attempt
    /// value no longer matches its job. The refresh finds those facts by comparing them
    /// with the source, not by reading the fact's own <c>Closed</c> flag. It used to
    /// re-read open facts only, on the premise that a job with <c>ClosedAt</c> set can
    /// no longer change. Nothing enforced that premise, and a closed job that gained a
    /// counted attempt kept its old count for good while the row count still reconciled
    /// (<c>BI-DRIFT-06</c>).</para>
    ///
    /// <para><b>One snapshot.</b> Insert, refresh and reconcile run inside a single
    /// <c>REPEATABLE READ</c> transaction. The reconcile asks whether any fact still
    /// disagrees with its job after the refresh; asked in a later snapshot, every attempt
    /// normalised in the milliseconds between the two would read as drift, and a busy
    /// afternoon would raise a MISMATCH that cleared itself on the next run. Inside one
    /// snapshot the answer is zero unless the refresh failed at its one job.</para>
    ///
    /// <para>A fact whose job is gone is left alone: the retention hook owns that delete,
    /// and touching it here would put two owners on the same row. It still counts toward
    /// the job-fact total, so an orphan that outlives the hook reads as MISMATCH, which is
    /// the rule the result grain has always applied to its own orphans.</para>
    /// </summary>
    private static async Task<JobGrainSync> SyncJobFactsAsync(
        IvrDbContext context,
        int batchSize,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction snapshot = await context.Database
            .BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

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
            .ToListAsync(cancellationToken);

        // Bounded like the insert: the cap limits one transaction, not correctness. Whatever
        // is left over is found again by the same comparison on the next run.
        List<JobFactAgainstSource> drifted = await DriftedJobFacts(context)
            .OrderBy(row => row.Fact.IvrCallJobId)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        string[] missingIds = missing.Select(job => job.JobId).ToArray();
        Dictionary<string, int> attemptCounts = missingIds.Length == 0
            ? []
            : await context.CallAttempts.AsNoTracking()
                .Where(attempt => attempt.IsCountedCustomerAttempt
                    && missingIds.Contains(attempt.IvrCallJobId))
                .GroupBy(attempt => attempt.IvrCallJobId)
                .Select(group => new { JobId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken);

        int inserted = 0;
        int rejected = 0;
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
                // Counted as well as skipped. A refused job never gets a fact, so without the
                // count the reconcile below would read it as a missing row on every run.
                rejected++;
                continue;
            }

            context.AnalyticsJobFacts.Add(fact);
            inserted++;
        }

        foreach (JobFactAgainstSource row in drifted)
        {
            row.Fact.CountedAttemptCount = row.CountedAttempts;
            row.Fact.Eligible = row.Eligible;
            row.Fact.Closed = row.Closed;
            row.Fact.LoadedAt = now;
        }

        if (inserted > 0 || drifted.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        // Same snapshot, after this run's own writes: what is counted here is exactly what the
        // insert and the refresh were looking at, and nothing that committed since.
        int sourceJobs = await context.CallJobs.CountAsync(cancellationToken);
        int jobFacts = await context.AnalyticsJobFacts.CountAsync(cancellationToken);
        int driftedAfterRefresh = await DriftedJobFacts(context).CountAsync(cancellationToken);

        await snapshot.CommitAsync(cancellationToken);

        return new JobGrainSync(
            inserted,
            drifted.Count,
            rejected,
            sourceJobs,
            jobFacts,
            driftedAfterRefresh,
            RefreshCapped: drifted.Count >= batchSize);
    }

    /// <summary>
    /// Every job fact paired with what its source job holds now, keeping the pairs that
    /// disagree. The refresh fixes what this returns and the reconcile counts what is left,
    /// so the two cannot come to mean different things by drift.
    ///
    /// <para><b>One pass over the attempts</b> (<c>W-0355</c>). The counts are grouped once and
    /// joined, not counted per job inside the projection. Counted per job, the query compiled to a
    /// correlated subquery that PostgreSQL ran three times for every job fact -- once for the
    /// projection, twice for the filter -- each time as a sequential scan of
    /// <c>ivr_call_attempts</c>: the one index that leads with the job id is partial
    /// (<c>WHERE is_counted_customer_attempt IS TRUE</c>), and the planner does not prove that
    /// predicate from the bare boolean filter EF writes. Quadratic in the data, it took about thirty
    /// seconds at 7,500 job facts in the soak of 2026-09-25, so runs began to hit the 30-second
    /// command timeout. <see cref="DriftedJobFactsSql"/> lets a test hold the plan to this shape.</para>
    /// </summary>
    private static IQueryable<JobFactAgainstSource> DriftedJobFacts(IvrDbContext context)
    {
        // Counted customer attempts only, as at insert: a technical retry is not a second attempt
        // at the customer (DT-02).
        var countedByJob = context.CallAttempts
            .Where(attempt => attempt.IsCountedCustomerAttempt)
            .GroupBy(attempt => attempt.IvrCallJobId)
            .Select(group => new { JobId = group.Key, Count = group.Count() });

        return
            from fact in context.AnalyticsJobFacts
            join job in context.CallJobs on fact.IvrCallJobId equals job.IvrCallJobId
            join counted in countedByJob on job.IvrCallJobId equals counted.JobId into matches
            from counted in matches.DefaultIfEmpty()
            select new JobFactAgainstSource
            {
                Fact = fact,
                Eligible = job.Eligible,
                Closed = job.ClosedAt != null,
                // A job with no counted attempt has no group, so the left join leaves it null.
                CountedAttempts = (int?)counted!.Count ?? 0,
            }
            into row
            where row.Fact.Eligible != row.Eligible
                || row.Fact.Closed != row.Closed
                || row.Fact.CountedAttemptCount != row.CountedAttempts
            select row;
    }

    /// <summary>
    /// The SQL of the drift comparison, for a test that asks PostgreSQL for its plan
    /// (<c>W-0355</c>). The query itself stays private; only its text leaves this class.
    /// </summary>
    public static string DriftedJobFactsSql(IvrDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return DriftedJobFacts(context).ToQueryString();
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

    /// <summary>
    /// A job fact beside the three values its source job holds now. A class with settable
    /// members rather than a positional record, because the query filters and orders on these
    /// members after projecting them, and only a member initialiser lets that translate to SQL.
    /// </summary>
    private sealed class JobFactAgainstSource
    {
        public AnalyticsFactCallJobEntity Fact { get; init; } = null!;

        public bool Eligible { get; init; }

        public bool Closed { get; init; }

        public int CountedAttempts { get; init; }
    }

    /// <summary>What the job-grain pass did, and what it counted in the snapshot it did it in.</summary>
    private readonly record struct JobGrainSync(
        int Inserted,
        int Refreshed,
        int Rejected,
        int SourceJobs,
        int JobFacts,
        int DriftedAfterRefresh,
        bool RefreshCapped);

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
            .ToListAsync(cancellationToken);

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
            .ToDictionaryAsync(row => row.JobId, row => row.Count, cancellationToken);

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
                DtmfKey = KeypadDigitOrNull(row.DtmfKey),
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
                .FirstOrDefaultAsync(row => row.ProgramKey == group.Key, cancellationToken);
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
                .FirstOrDefaultAsync(row => row.ScriptVariantKey == group.Key, cancellationToken);
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
                .FirstOrDefaultAsync(row => row.ResultTypeKey == group.Key, cancellationToken);
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

        await context.SaveChangesAsync(cancellationToken);
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

        await stale.ExecuteDeleteAsync(cancellationToken);

        // Materialized before grouping: the distinct-order count and the taxonomy splits are
        // clearer in memory than as a translated aggregate, and the set is one batch of dates.
        List<AnalyticsFactCallOutcomeEntity> rows = await facts
            .ToListAsync(cancellationToken);

        List<AnalyticsKpiDailyEntity> buckets = AnalyticsKpiMath.Fold(rows, now);

        context.AnalyticsKpiDaily.AddRange(buckets);
        await context.SaveChangesAsync(cancellationToken);
        return buckets.Count;
    }

    // ----------------------------------------------------------------- reconcile

    private static async Task<(int SourceRows, int OrphanRows)> CountSourceAsync(
        IvrDbContext context,
        CancellationToken cancellationToken)
    {
        int total = await context.CallResults.AsNoTracking()
            .CountAsync(cancellationToken);

        // A result whose job is gone cannot be projected — the fact needs the program and the
        // script variant. Counting it separately is what keeps the reconcile honest: without
        // this the pipeline would report MISMATCH forever and nobody would know why.
        int orphan = await context.CallResults.AsNoTracking()
            .CountAsync(
                result => !context.CallJobs.Any(job => job.IvrCallJobId == result.IvrCallJobId),
                cancellationToken);

        return (total, orphan);
    }

    /// <summary>
    /// The run's verdict: the worse of the two grains, MISMATCH before BACKLOG before COMPLETE.
    ///
    /// <para>The result grain is judged on counts, as it always was. The job grain is judged on
    /// contents as well: a stale job fact has the right count and the wrong values, which is
    /// exactly the state a count-only reconcile calls COMPLETE. So a fact that still disagrees
    /// with its job after the refresh is a MISMATCH by itself, unless the refresh stopped at the
    /// batch cap, in which case the remainder is the next run's work and the verdict is BACKLOG.</para>
    /// </summary>
    private static string ResolveStatus(
        int sourceRows,
        int orphanRows,
        int factRows,
        int rejectedRows,
        JobGrainSync jobs)
    {
        string results = CompareCounts(factRows, (long)sourceRows - orphanRows - rejectedRows);
        string jobGrain;
        if (jobs.DriftedAfterRefresh > 0)
        {
            jobGrain = jobs.RefreshCapped
                ? AnalyticsReconcileStatus.Backlog
                : AnalyticsReconcileStatus.Mismatch;
        }
        else
        {
            jobGrain = CompareCounts(jobs.JobFacts, (long)jobs.SourceJobs - jobs.Rejected);
        }

        return Severity(jobGrain) > Severity(results) ? jobGrain : results;
    }

    private static string CompareCounts(long facts, long expected)
    {
        if (facts == expected)
        {
            return AnalyticsReconcileStatus.Complete;
        }

        return facts < expected
            ? AnalyticsReconcileStatus.Backlog
            : AnalyticsReconcileStatus.Mismatch;
    }

    private static int Severity(string status) => status switch
    {
        AnalyticsReconcileStatus.Mismatch => 2,
        AnalyticsReconcileStatus.Backlog => 1,
        _ => 0,
    };

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
            .FirstOrDefaultAsync(row => row.PipelineName == PipelineName, cancellationToken);

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

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Culture-independent bucket label used by the KPI catalog examples.</summary>
    public static string FormatBucket(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
