using Ivr.Api.Admin;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IAnalyticsReadService
{
    public Task<AnalyticsSummaryApiResult> GetSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken);

    public Task<AnalyticsTrendApiResult> GetTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken);

    public Task<AnalyticsBreakdownApiResult> GetBreakdownAsync(
        AnalyticsFilter filter,
        string? dimension,
        CancellationToken cancellationToken);

    public Task<AnalyticsExportApiResult> ExportAsync(
        AnalyticsFilter filter,
        string? dimension,
        string? reason,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Privacy-safe reporting projections (W-0098), consumed by the P3-4 reporting
/// console.
///
/// Three properties hold this together and are asserted by the tests:
///
/// 1. <b>Aggregate only.</b> Every value leaving this service is a count, a rate
///    or a dimension label. No task, job, order code, phone or evidence ref is
///    ever projected, so there is nothing to mask downstream (D-05).
/// 2. <b>k-anonymity is server-side.</b> <see cref="MinBucketSize"/> is a
///    constant, never a request parameter: a caller can narrow a filter but can
///    never lower the threshold that protects a small bucket.
/// 3. <b>The source is stated, not implied.</b> This computes from the
///    operational tables because the P10-4 warehouse (`W-0055`) does not exist
///    yet. `warehouse_backed=false` says so on every payload rather than letting
///    the console present operational reads as a BI pipeline.
/// </summary>
public sealed class AnalyticsReadService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IOptions<IvrOptions> ivrOptions,
    IAuditLogger auditLogger,
    TimeProvider timeProvider) : IAnalyticsReadService
{
    /// <summary>
    /// k-anonymity threshold. A bucket holding fewer results than this is
    /// dropped before serialization, because a two-call bucket plus a known
    /// call time is a re-identification path even without a customer field.
    /// </summary>
    public const int MinBucketSize = 5;

    /// <summary>
    /// Scan cap for the fact projection. Reaching it is reported as
    /// `truncated: true` rather than silently returning partial numbers.
    /// </summary>
    public const int MaxFactRows = 50_000;

    public const string SourceLabel = "OPERATIONAL_READ_MODEL";
    public const string PipelineWorkId = "W-0055";
    public const string ExportAuditAction = "IVR_ANALYTICS_EXPORT";

    /// <summary>Beyond this the console must warn that the numbers lag.</summary>
    public static readonly TimeSpan FreshnessBudget = TimeSpan.FromMinutes(15);

    private const string DimensionResultType = "RESULT_TYPE";
    private const string DimensionScriptVariant = "SCRIPT_VARIANT";
    private const string DimensionProgram = "PROGRAM";
    private const string BucketDay = "DAY";
    private const string BucketHour = "HOUR";
    private const int MinExportReasonLength = 8;

    private static readonly string[] NoAnswerResultTypes =
        ["IVR_NO_ANSWER_ATTEMPT", "IVR_NO_ANSWER_FINAL"];

    private static readonly string[] ExportColumns =
        ["dimension", "key", "total", "confirmed", "confirm_rate", "share"];

    public async Task<AnalyticsSummaryApiResult> GetSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        NormalizedFilter normalized = Normalize(filter);
        FactSet facts = await LoadAsync(normalized, cancellationToken).ConfigureAwait(false);

        (IReadOnlyList<AnalyticsBreakdownRowView> taxonomy, int suppressed) =
            BuildBreakdown(facts, DimensionResultType);

        return new AnalyticsSummaryApiResult(
            normalized.ToView(),
            ivrOptions.Value.ExecutionMode,
            BuildKpi(facts),
            taxonomy,
            BuildDataQuality(facts, suppressed));
    }

    public async Task<AnalyticsTrendApiResult> GetTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken)
    {
        NormalizedFilter normalized = Normalize(filter);
        FactSet facts = await LoadAsync(normalized, cancellationToken).ConfigureAwait(false);

        var grouped = facts.Rows
            .GroupBy(row => new
            {
                BucketStart = TruncateTo(row.CreatedAt, normalized.Bucket),
                row.Program,
            })
            .OrderBy(group => group.Key.BucketStart)
            .ThenBy(group => group.Key.Program, StringComparer.Ordinal)
            .ToList();

        List<AnalyticsTrendBucketView> buckets = [];
        int suppressed = 0;
        foreach (var group in grouped)
        {
            // A bucket below the threshold is omitted entirely rather than
            // returned with zeroed counts: a zero row reads as "no calls", which
            // is a different and false statement.
            if (group.Count() < MinBucketSize)
            {
                suppressed++;
                continue;
            }

            int total = group.Count();
            int confirmed = group.Count(row => row.ResultType == "IVR_CONFIRMED");
            buckets.Add(new AnalyticsTrendBucketView(
                group.Key.BucketStart,
                group.Key.Program,
                total,
                confirmed,
                group.Count(row => row.ResultType == "IVR_CUSTOMER_CANCELLED"),
                group.Count(row => NoAnswerResultTypes.Contains(row.ResultType)),
                group.Count(row => row.ResultType == "IVR_INVALID_PHONE_FINAL"),
                group.Count(row => row.ResultType == "IVR_TECHNICAL_EXCEPTION"),
                group.Count(row => row.ResultType == "IVR_OPERATIONAL_BLOCKED"),
                Share(confirmed, total)));
        }

        return new AnalyticsTrendApiResult(
            normalized.ToView(),
            buckets,
            BuildDataQuality(facts, suppressed));
    }

    public async Task<AnalyticsBreakdownApiResult> GetBreakdownAsync(
        AnalyticsFilter filter,
        string? dimension,
        CancellationToken cancellationToken)
    {
        NormalizedFilter normalized = Normalize(filter);
        string normalizedDimension = NormalizeDimension(dimension);
        FactSet facts = await LoadAsync(normalized, cancellationToken).ConfigureAwait(false);

        (IReadOnlyList<AnalyticsBreakdownRowView> rows, int suppressed) =
            BuildBreakdown(facts, normalizedDimension);

        return new AnalyticsBreakdownApiResult(
            normalized.ToView(),
            normalizedDimension,
            rows,
            BuildDataQuality(facts, suppressed));
    }

    public async Task<AnalyticsExportApiResult> ExportAsync(
        AnalyticsFilter filter,
        string? dimension,
        string? reason,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        NormalizedFilter normalized = Normalize(filter);
        string normalizedDimension = NormalizeDimension(dimension);
        string normalizedReason = NormalizeExportReason(reason);
        FactSet facts = await LoadAsync(normalized, cancellationToken).ConfigureAwait(false);

        (IReadOnlyList<AnalyticsBreakdownRowView> rows, int suppressed) =
            BuildBreakdown(facts, normalizedDimension);

        // Data exists but nothing survived k-anonymity: the filter is narrow
        // enough that any extract would be a re-identification vector, so the
        // export is refused instead of returned empty (P3-4 §11).
        if (rows.Count == 0 && facts.Rows.Count > 0)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        AnalyticsDataQualityView dataQuality = BuildDataQuality(facts, suppressed);
        AuditLogEntry entry = await auditLogger.AppendAsync(
            new AuditEvent(
                actorId,
                ExportAuditAction,
                $"analytics:{normalizedDimension}",
                normalizedReason,
                correlationId,
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["dimension"] = normalizedDimension,
                    ["program"] = normalized.Program,
                    ["result_type"] = normalized.ResultType,
                    ["script_variant"] = normalized.ScriptVariant,
                    ["bucket"] = normalized.Bucket,
                    ["from"] = normalized.From,
                    ["to"] = normalized.To,
                    ["row_count"] = rows.Count,
                    ["suppressed_row_count"] = suppressed,
                    ["min_bucket_size"] = MinBucketSize,
                    ["source"] = SourceLabel,
                }),
            cancellationToken).ConfigureAwait(false);

        return new AnalyticsExportApiResult(
            normalized.ToView(),
            normalizedDimension,
            normalizedReason,
            actorId,
            correlationId,
            entry.Id.ToString("D"),
            ExportColumns,
            rows.Select(row => (IReadOnlyList<string>)
            [
                normalizedDimension,
                row.Key,
                row.Total.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.Confirmed.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.ConfirmRate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                row.Share.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
            ]).ToArray(),
            suppressed,
            dataQuality);
    }

    private async Task<FactSet> LoadAsync(
        NormalizedFilter filter,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);

        IQueryable<Ivr.Infrastructure.Persistence.Entities.CallJobEntity> jobs =
            context.CallJobs.AsNoTracking();
        if (filter.Program is not null)
        {
            jobs = jobs.Where(job => job.ProgramType == filter.Program);
        }

        if (filter.ScriptVariant is not null)
        {
            jobs = jobs.Where(job => job.ScriptVersion == filter.ScriptVariant);
        }

        // Filter and order on the entities, project last: EF cannot translate a
        // predicate written against an already-constructed projection type.
        var joined = from result in context.CallResults.AsNoTracking()
                     join job in jobs on result.IvrCallJobId equals job.IvrCallJobId
                     select new { Result = result, Job = job };

        if (filter.ResultType is not null)
        {
            joined = joined.Where(row => row.Result.ResultType == filter.ResultType);
        }

        if (filter.From is not null)
        {
            joined = joined.Where(row => row.Result.CreatedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            joined = joined.Where(row => row.Result.CreatedAt <= filter.To);
        }

        // One extra row so a full page can be distinguished from a truncated one.
        List<FactRow> rows = await joined
            .OrderBy(row => row.Result.CreatedAt)
            .ThenBy(row => row.Result.IvrCallJobId)
            .Take(MaxFactRows + 1)
            .Select(row => new FactRow(
                row.Result.IvrCallJobId,
                row.Result.CreatedAt,
                row.Job.ProgramType,
                row.Job.ScriptVersion,
                row.Result.ResultType,
                row.Result.IsFinalForIvr,
                row.Job.T0At))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        bool truncated = rows.Count > MaxFactRows;
        if (truncated)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        IQueryable<string> scopedJobIds = jobs.Select(job => job.IvrCallJobId);
        int totalJobs = await jobs.CountAsync(cancellationToken).ConfigureAwait(false);
        int eligibleTasks = await jobs
            .CountAsync(job => job.Eligible, cancellationToken)
            .ConfigureAwait(false);

        // Only counted customer attempts qualify: a technical retry must never
        // inflate the attempt-2 rate (DT-02).
        int secondAttemptJobs = await context.CallAttempts.AsNoTracking()
            .Where(attempt => attempt.IsCountedCustomerAttempt
                && attempt.AttemptNumber >= 2
                && scopedJobIds.Contains(attempt.IvrCallJobId))
            .Select(attempt => attempt.IvrCallJobId)
            .Distinct()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new FactSet(rows, totalJobs, eligibleTasks, secondAttemptJobs, truncated);
    }

    private static AnalyticsKpiView BuildKpi(FactSet facts)
    {
        int total = facts.Rows.Count;
        int Count(Func<FactRow, bool> predicate) => facts.Rows.Count(predicate);

        double[] secondsToFinal = facts.Rows
            .Where(row => row.IsFinal)
            .Select(row => (row.CreatedAt - row.T0At).TotalSeconds)
            .Where(seconds => seconds >= 0)
            .ToArray();

        return new AnalyticsKpiView(
            total,
            Count(row => row.IsFinal),
            facts.TotalJobs,
            facts.EligibleTasks,
            Share(Count(row => row.ResultType == "IVR_CONFIRMED"), total),
            Share(Count(row => row.ResultType == "IVR_CUSTOMER_CANCELLED"), total),
            Share(Count(row => NoAnswerResultTypes.Contains(row.ResultType)), total),
            Share(Count(row => row.ResultType == "IVR_INVALID_PHONE_FINAL"), total),
            Share(Count(row => row.ResultType == "IVR_TECHNICAL_EXCEPTION"), total),
            Share(Count(row => row.ResultType == "IVR_OPERATIONAL_BLOCKED"), total),
            Share(facts.SecondAttemptJobs, facts.TotalJobs),
            secondsToFinal.Length == 0 ? null : Math.Round(secondsToFinal.Average(), 2));
    }

    private static (IReadOnlyList<AnalyticsBreakdownRowView> Rows, int Suppressed) BuildBreakdown(
        FactSet facts,
        string dimension)
    {
        int total = facts.Rows.Count;
        List<AnalyticsBreakdownRowView> rows = [];
        int suppressed = 0;

        foreach (var group in facts.Rows
            .GroupBy(row => KeyOf(row, dimension), StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal))
        {
            if (group.Count() < MinBucketSize)
            {
                suppressed++;
                continue;
            }

            int groupTotal = group.Count();
            int confirmed = group.Count(row => row.ResultType == "IVR_CONFIRMED");
            rows.Add(new AnalyticsBreakdownRowView(
                group.Key,
                groupTotal,
                confirmed,
                Share(confirmed, groupTotal),
                Share(groupTotal, total)));
        }

        return (rows, suppressed);
    }

    private AnalyticsDataQualityView BuildDataQuality(FactSet facts, int suppressedBuckets)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset? latest = facts.Rows.Count == 0
            ? null
            : facts.Rows.Max(row => row.CreatedAt);

        long? freshnessSeconds = latest is null
            ? null
            : (long)Math.Max(0d, (now - latest.Value).TotalSeconds);

        string status = latest is null
            ? "NO_DATA"
            : freshnessSeconds <= (long)FreshnessBudget.TotalSeconds ? "FRESH" : "STALE";

        return new AnalyticsDataQualityView(
            now,
            SourceLabel,
            WarehouseBacked: false,
            PipelineWorkId,
            latest,
            freshnessSeconds,
            status,
            MinBucketSize,
            suppressedBuckets,
            facts.Rows.Count,
            facts.Truncated);
    }

    private static string KeyOf(FactRow row, string dimension) => dimension switch
    {
        DimensionScriptVariant => string.IsNullOrWhiteSpace(row.ScriptVersion)
            ? "UNKNOWN"
            : row.ScriptVersion,
        DimensionProgram => row.Program,
        _ => row.ResultType,
    };

    private static DateTimeOffset TruncateTo(DateTimeOffset value, string bucket)
    {
        DateTimeOffset utc = value.ToUniversalTime();
        return bucket == BucketHour
            ? new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero)
            : new DateTimeOffset(utc.Year, utc.Month, utc.Day, 0, 0, 0, TimeSpan.Zero);
    }

    private static double Share(int value, int total) =>
        total == 0 ? 0d : Math.Round((double)value / total, 4);

    private static NormalizedFilter Normalize(AnalyticsFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (filter.From is not null && filter.To is not null && filter.From > filter.To)
        {
            throw IvrErrors.MalformedRequest("from must not be later than to.");
        }

        return new NormalizedFilter(
            NormalizeProgram(filter.Program),
            NormalizeFreeText(filter.ResultType, nameof(filter.ResultType)),
            NormalizeFreeText(filter.ScriptVariant, nameof(filter.ScriptVariant)),
            NormalizeBucket(filter.Bucket),
            filter.From,
            filter.To);
    }

    private static string NormalizeBucket(string? bucket)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return BucketDay;
        }

        string normalized = bucket.Trim().ToUpperInvariant();
        return normalized is BucketDay or BucketHour
            ? normalized
            : throw IvrErrors.MalformedRequest("bucket must be DAY or HOUR.");
    }

    private static string NormalizeDimension(string? dimension)
    {
        if (string.IsNullOrWhiteSpace(dimension))
        {
            return DimensionResultType;
        }

        string normalized = dimension.Trim().ToUpperInvariant();
        return normalized is DimensionResultType or DimensionScriptVariant or DimensionProgram
            ? normalized
            : throw IvrErrors.MalformedRequest(
                "dimension must be RESULT_TYPE, SCRIPT_VARIANT or PROGRAM.");
    }

    private static string? NormalizeProgram(string? program)
    {
        if (string.IsNullOrWhiteSpace(program))
        {
            return null;
        }

        string normalized = program.Trim().ToUpperInvariant();
        return normalized is "GOLDEN_HOUR" or "TWENTY_FOUR_SEVEN"
            ? normalized
            : throw IvrErrors.MalformedRequest(
                "program must be GOLDEN_HOUR or TWENTY_FOUR_SEVEN.");
    }

    /// <summary>
    /// An export reason is written to the audit log, so it is length-bounded and
    /// PII-scanned like any other free text crossing the boundary.
    /// </summary>
    private static string NormalizeExportReason(string? reason)
    {
        string trimmed = (reason ?? string.Empty).Trim();
        if (trimmed.Length < MinExportReasonLength)
        {
            throw IvrErrors.MalformedRequest(
                $"reason is required and must be at least {MinExportReasonLength} characters.");
        }

        return NormalizeFreeText(trimmed, "reason")!;
    }

    private static string? NormalizeFreeText(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (value.Length > 200)
        {
            throw IvrErrors.MalformedRequest($"{field} is too long.");
        }

        try
        {
            PiiGuard.EnsureSafeText(value);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        return value;
    }

    private sealed record NormalizedFilter(
        string? Program,
        string? ResultType,
        string? ScriptVariant,
        string Bucket,
        DateTimeOffset? From,
        DateTimeOffset? To)
    {
        public AnalyticsFilterView ToView() =>
            new(Program, ResultType, ScriptVariant, Bucket, From, To);
    }

    private sealed record FactRow(
        string IvrCallJobId,
        DateTimeOffset CreatedAt,
        string Program,
        string? ScriptVersion,
        string ResultType,
        bool IsFinal,
        DateTimeOffset T0At);

    private sealed record FactSet(
        List<FactRow> Rows,
        int TotalJobs,
        int EligibleTasks,
        int SecondAttemptJobs,
        bool Truncated);
}
