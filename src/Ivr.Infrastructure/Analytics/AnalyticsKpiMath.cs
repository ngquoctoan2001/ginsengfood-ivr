namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// The KPI fold, kept as a pure function of a fact set.
///
/// <para>Separated from the ETL for one reason: the arithmetic is the part that can
/// be wrong in a way nothing else notices. A miscounted taxonomy split does not
/// throw, does not fail a reconcile and does not show up as a gap — it shows up as
/// a plausible number on a dashboard. Pure and unit-testable means the numbers can
/// be checked against hand-computed expectations without a database in the way.</para>
///
/// <para>Sums are stored instead of means (<c>SecondsToResultSum</c> plus
/// <c>SecondsToResultCount</c>, never an average). Averages do not add up: two
/// buckets of averages cannot be combined into a correct third, so storing them
/// would quietly break every roll-up a BI tool performs.</para>
/// </summary>
public static class AnalyticsKpiMath
{
    public const string ResultConfirmed = "IVR_CONFIRMED";
    public const string ResultCancelled = "IVR_CUSTOMER_CANCELLED";
    public const string ResultInvalidPhone = "IVR_INVALID_PHONE_FINAL";
    public const string ResultTechnical = "IVR_TECHNICAL_EXCEPTION";
    public const string ResultOperationalBlocked = "IVR_OPERATIONAL_BLOCKED";

    public static readonly string[] NoAnswerResultTypes =
        ["IVR_NO_ANSWER_ATTEMPT", "IVR_NO_ANSWER_FINAL"];

    /// <summary>
    /// Folds facts into one row per (date, program, script variant). Grouping by
    /// the variant is what makes the A/B comparison P2-7 produces measurable — a
    /// bucket keyed on date and program alone would average the variants together
    /// and hide exactly the difference the experiment exists to find.
    /// </summary>
    public static List<AnalyticsKpiDailyEntity> Fold(
        IEnumerable<AnalyticsFactCallOutcomeEntity> facts,
        DateTimeOffset computedAt)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return facts
            .GroupBy(fact => new { fact.EventDate, fact.ProgramKey, fact.ScriptVariantKey })
            .Select(group => new AnalyticsKpiDailyEntity
            {
                BucketDate = group.Key.EventDate,
                ProgramKey = group.Key.ProgramKey,
                ScriptVariantKey = group.Key.ScriptVariantKey,
                TotalResults = group.Count(),
                FinalResults = group.Count(fact => fact.IsFinal),
                DistinctOrders = group
                    .Select(fact => fact.OrderRefHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                ConfirmedCount = group.Count(fact => fact.ResultTypeKey == ResultConfirmed),
                CancelledCount = group.Count(fact => fact.ResultTypeKey == ResultCancelled),
                NoAnswerCount = group.Count(fact =>
                    NoAnswerResultTypes.Contains(fact.ResultTypeKey)),
                InvalidPhoneCount = group.Count(fact => fact.ResultTypeKey == ResultInvalidPhone),
                TechnicalCount = group.Count(fact => fact.ResultTypeKey == ResultTechnical),
                OperationalBlockedCount = group.Count(fact =>
                    fact.ResultTypeKey == ResultOperationalBlocked),
                SecondAttemptResults = group.Count(fact => fact.CountedAttemptNumber >= 2),
                // Only final results contribute a duration. A non-final result is an attempt
                // that has not finished, and averaging it in would report the job as faster
                // than it was by counting a partial elapsed time as a completed one.
                SecondsToResultSum = group
                    .Where(fact => fact.IsFinal && fact.SecondsToResult is not null)
                    .Sum(fact => (long)fact.SecondsToResult!.Value),
                SecondsToResultCount = group
                    .Count(fact => fact.IsFinal && fact.SecondsToResult is not null),
                ComputedAt = computedAt,
            })
            .ToList();
    }

    /// <summary>
    /// Mean seconds to a final result, or null when the bucket holds none. Null
    /// rather than zero: zero is a measurement, and "nothing finished yet" is not.
    /// </summary>
    public static double? AverageSecondsToResult(AnalyticsKpiDailyEntity bucket)
    {
        ArgumentNullException.ThrowIfNull(bucket);
        return bucket.SecondsToResultCount == 0
            ? null
            : (double)bucket.SecondsToResultSum / bucket.SecondsToResultCount;
    }

    /// <summary>Share of <paramref name="value"/> in <paramref name="total"/>, 4dp.</summary>
    public static double Rate(int value, int total) =>
        total == 0 ? 0d : Math.Round((double)value / total, 4);
}
