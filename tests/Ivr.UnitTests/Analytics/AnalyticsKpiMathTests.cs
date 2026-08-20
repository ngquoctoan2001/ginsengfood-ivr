using Ivr.Infrastructure.Analytics;

namespace Ivr.UnitTests.Analytics;

/// <summary>
/// W-0055 / P10-4 §8 <c>BI-KPI-02</c>.
///
/// <para>Expectations are hand-counted from the fixture below, not recomputed with
/// the same expression the production code uses. Asserting <c>Fold</c> against a
/// second copy of <c>Fold</c>'s own arithmetic would pass for any arithmetic.</para>
/// </summary>
public sealed class AnalyticsKpiMathTests
{
    private static readonly DateOnly Day = new(2026, 8, 14);
    private static readonly DateOnly NextDay = new(2026, 8, 15);
    private static readonly DateTimeOffset ComputedAt =
        new(2026, 8, 15, 3, 0, 0, TimeSpan.Zero);

    private const string VariantA = "SCRIPT-ORDER-CONFIRM:vA";
    private const string VariantB = "SCRIPT-ORDER-CONFIRM:vB";
    private const string GoldenHour = "GOLDEN_HOUR";
    private const string TwentyFourSeven = "TWENTY_FOUR_SEVEN";

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void BucketsSplitByDateProgramAndScriptVariant()
    {
        List<AnalyticsKpiDailyEntity> buckets = AnalyticsKpiMath.Fold(Fixture(), ComputedAt);

        Assert.Equal(
            new[]
            {
                $"{Day}|{GoldenHour}|{VariantA}",
                $"{Day}|{GoldenHour}|{VariantB}",
                $"{Day}|{TwentyFourSeven}|{VariantA}",
                $"{NextDay}|{GoldenHour}|{VariantA}",
            },
            buckets
                .Select(bucket =>
                    $"{bucket.BucketDate}|{bucket.ProgramKey}|{bucket.ScriptVariantKey}")
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void TaxonomyCountsAndRatesMatchTheHandCountedFixture()
    {
        AnalyticsKpiDailyEntity bucket = Bucket(Day, GoldenHour, VariantA);

        // Hand count of the vA / Golden Hour / 14 Aug rows in Fixture():
        // 4 confirmed, 1 cancelled, 2 no-answer (one attempt, one final), 1 technical.
        Assert.Equal(8, bucket.TotalResults);
        Assert.Equal(4, bucket.ConfirmedCount);
        Assert.Equal(1, bucket.CancelledCount);
        Assert.Equal(2, bucket.NoAnswerCount);
        Assert.Equal(1, bucket.TechnicalCount);
        Assert.Equal(0, bucket.InvalidPhoneCount);
        Assert.Equal(0, bucket.OperationalBlockedCount);

        Assert.Equal(0.5d, AnalyticsKpiMath.Rate(bucket.ConfirmedCount, bucket.TotalResults));
        Assert.Equal(0.25d, AnalyticsKpiMath.Rate(bucket.NoAnswerCount, bucket.TotalResults));
        Assert.Equal(0.125d, AnalyticsKpiMath.Rate(bucket.TechnicalCount, bucket.TotalResults));

        // Both no-answer types collapse into one KPI, and only one of them is final.
        Assert.Equal(7, bucket.FinalResults);
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void TheVariantBBucketIsMeasuredSeparatelySoAnAbComparisonIsPossible()
    {
        AnalyticsKpiDailyEntity variantA = Bucket(Day, GoldenHour, VariantA);
        AnalyticsKpiDailyEntity variantB = Bucket(Day, GoldenHour, VariantB);

        Assert.Equal(4, variantB.TotalResults);
        Assert.Equal(1, variantB.ConfirmedCount);

        // The point of splitting on variant: pooled, these two would read 0.4167 and the
        // difference the experiment exists to measure would be gone.
        Assert.Equal(0.5d, AnalyticsKpiMath.Rate(variantA.ConfirmedCount, variantA.TotalResults));
        Assert.Equal(0.25d, AnalyticsKpiMath.Rate(variantB.ConfirmedCount, variantB.TotalResults));
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void DistinctOrdersCountsOrdersNotResults()
    {
        AnalyticsKpiDailyEntity bucket = Bucket(Day, GoldenHour, VariantA);

        // Eight results, but two of them are a second attempt on an order already counted.
        Assert.Equal(8, bucket.TotalResults);
        Assert.Equal(6, bucket.DistinctOrders);
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void SecondAttemptCountsOnlyCountedCustomerAttempts()
    {
        AnalyticsKpiDailyEntity bucket = Bucket(Day, GoldenHour, VariantA);

        // Two rows carry CountedAttemptNumber = 2. A third row in the fixture went through a
        // technical retry, which never increments the counted attempt number (DT-02).
        Assert.Equal(2, bucket.SecondAttemptResults);
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void ElapsedTimeIsStoredAsSumAndCountSoBucketsStayAddable()
    {
        AnalyticsKpiDailyEntity day = Bucket(Day, GoldenHour, VariantA);
        AnalyticsKpiDailyEntity next = Bucket(NextDay, GoldenHour, VariantA);

        // Final vA rows on 14 Aug: 60, 120, 180, 240, 300, 360, 420 -> 7 rows, 1680 s.
        Assert.Equal(7, day.SecondsToResultCount);
        Assert.Equal(1680L, day.SecondsToResultSum);
        Assert.Equal(240d, AnalyticsKpiMath.AverageSecondsToResult(day));

        // 15 Aug holds one final row at 60 s. Rolling the two days up must use sums, and the
        // combined mean is not the mean of the means.
        double combined =
            (double)(day.SecondsToResultSum + next.SecondsToResultSum)
            / (day.SecondsToResultCount + next.SecondsToResultCount);
        Assert.Equal(217.5d, combined);
        Assert.NotEqual(
            (AnalyticsKpiMath.AverageSecondsToResult(day)!.Value
                + AnalyticsKpiMath.AverageSecondsToResult(next)!.Value) / 2,
            combined);
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void ABucketWithNoFinishedCallReportsNoAverageRatherThanZero()
    {
        AnalyticsKpiDailyEntity bucket = Bucket(Day, TwentyFourSeven, VariantA);

        Assert.Equal(0, bucket.SecondsToResultCount);
        Assert.Null(AnalyticsKpiMath.AverageSecondsToResult(bucket));
    }

    [Fact]
    [Trait("TestId", "BI-KPI-02")]
    public void FactsCarryTheHourSoAnHourlyTrendIsDerivedFromTheSameRows()
    {
        // The stored aggregate is daily; the hourly view the reporting API serves is derived
        // from the facts. Both must read the same event, or the two views disagree.
        AnalyticsFactCallOutcomeEntity[] facts = Fixture()
            .Where(fact => fact.EventDate == Day && fact.ProgramKey == GoldenHour)
            .ToArray();

        Assert.All(facts, fact => Assert.Equal(fact.EventAt.Hour, fact.EventHour));
        Assert.Equal([9, 10], facts.Select(fact => fact.EventHour).Distinct().Order().ToArray());
    }

    private static AnalyticsKpiDailyEntity Bucket(DateOnly date, string program, string variant) =>
        AnalyticsKpiMath.Fold(Fixture(), ComputedAt)
            .Single(bucket => bucket.BucketDate == date
                && bucket.ProgramKey == program
                && bucket.ScriptVariantKey == variant);

    /// <summary>
    /// Twelve result rows across four buckets. Order hashes repeat where a job made a
    /// second attempt, so distinct-order counting has something to distinguish.
    /// </summary>
    private static List<AnalyticsFactCallOutcomeEntity> Fixture() =>
    [
        // Golden Hour / vA / 14 Aug — 8 rows, 6 distinct orders.
        Fact("r01", "o01", GoldenHour, VariantA, "IVR_CONFIRMED", 9, 60, final: true),
        Fact("r02", "o02", GoldenHour, VariantA, "IVR_CONFIRMED", 9, 120, final: true),
        Fact("r03", "o03", GoldenHour, VariantA, "IVR_CONFIRMED", 9, 180, final: true),
        Fact("r04", "o04", GoldenHour, VariantA, "IVR_CONFIRMED", 10, 240, final: true),
        Fact("r05", "o05", GoldenHour, VariantA, "IVR_CUSTOMER_CANCELLED", 10, 300, final: true),
        // Same order as r05: a second counted attempt that ended without an answer.
        Fact("r06", "o05", GoldenHour, VariantA, "IVR_NO_ANSWER_ATTEMPT", 10, 330, final: false,
            attempt: 2),
        Fact("r07", "o06", GoldenHour, VariantA, "IVR_NO_ANSWER_FINAL", 10, 360, final: true,
            attempt: 2),
        // Same order as r06's job family; a technical exception after a technical retry, which
        // must not raise the counted attempt number.
        Fact("r08", "o06", GoldenHour, VariantA, "IVR_TECHNICAL_EXCEPTION", 10, 420, final: true),

        // Golden Hour / vB / 14 Aug — the comparison arm.
        Fact("r09", "o07", GoldenHour, VariantB, "IVR_CONFIRMED", 9, 90, final: true),
        Fact("r10", "o08", GoldenHour, VariantB, "IVR_NO_ANSWER_FINAL", 9, 150, final: true),
        Fact("r11", "o09", GoldenHour, VariantB, "IVR_NO_ANSWER_FINAL", 9, 210, final: true),
        Fact("r12", "o10", GoldenHour, VariantB, "IVR_TECHNICAL_EXCEPTION", 9, 270, final: true),

        // 24/7 / vA / 14 Aug — nothing finished.
        Fact("r13", "o11", TwentyFourSeven, VariantA, "IVR_NO_ANSWER_ATTEMPT", 11, null,
            final: false),

        // Golden Hour / vA / 15 Aug — the roll-up partner.
        Fact("r14", "o12", GoldenHour, VariantA, "IVR_CONFIRMED", 8, 60, final: true,
            nextDay: true),
    ];

    private static AnalyticsFactCallOutcomeEntity Fact(
        string resultId,
        string orderKey,
        string program,
        string variant,
        string resultType,
        int hour,
        int? seconds,
        bool final,
        int attempt = 1,
        bool nextDay = false)
    {
        DateOnly date = nextDay ? NextDay : Day;
        return new AnalyticsFactCallOutcomeEntity
        {
            IvrCallResultId = resultId,
            IvrCallJobId = $"job-{resultId}",
            // Stand-in for the SHA-256 the ETL writes; the fold only ever compares it.
            OrderRefHash = orderKey,
            ProgramKey = program,
            ScriptVariantKey = variant,
            ResultTypeKey = resultType,
            FinalResultStatus = final ? "FINAL" : "PENDING",
            IsFinal = final,
            IsCountedCustomerAttempt = true,
            CountedAttemptNumber = attempt,
            EventAt = new DateTimeOffset(
                date.Year, date.Month, date.Day, hour, 0, 0, TimeSpan.Zero),
            EventDate = date,
            EventHour = hour,
            SecondsToResult = seconds,
            LoadedAt = ComputedAt,
        };
    }
}
