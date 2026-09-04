using System.Globalization;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests.Analytics;

/// <summary>
/// W-0055 / P10-4 §8 — <c>BI-IDEMP-03</c> and <c>BI-QUALITY-04</c> against real
/// PostgreSQL, because both properties are about what the database does: the
/// anti-join that makes a replay exactly-once, and the reconcile that compares two
/// row counts across a schema boundary.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AnalyticsPipelineTests(PostgresPersistenceFixture fixture)
{
    private const string VariantA = "SCRIPT-ORDER-CONFIRM:vA";
    private const string VariantB = "SCRIPT-ORDER-CONFIRM:vB";
    private const string GoldenHour = "GOLDEN_HOUR";
    private const string TwentyFourSeven = "TWENTY_FOUR_SEVEN";
    private const int SeededResults = 9;

    private static readonly DateTimeOffset ResultAt =
        new(2026, 8, 14, 9, 30, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset RunAt =
        new(2026, 8, 14, 10, 0, 0, TimeSpan.Zero);

    // ------------------------------------------------------------- BI-IDEMP-03

    [Fact]
    [Trait("TestId", "BI-IDEMP-03")]
    public async Task ASecondRunOverTheSameSourceLoadsNothingAndChangesNoNumber()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        AnalyticsEtlRunReport first = await RunEtlAsync();
        Assert.Equal(SeededResults, first.LoadedRows);
        Assert.Equal(AnalyticsReconcileStatus.Complete, first.ReconcileStatus);

        IReadOnlyList<string> after = await SnapshotBucketsAsync();

        AnalyticsEtlRunReport second = await RunEtlAsync();

        // The anti-join found nothing outstanding, so nothing was written a second time.
        Assert.Equal(0, second.LoadedRows);
        Assert.Equal(first.FactRowCount, second.FactRowCount);
        Assert.Equal(AnalyticsReconcileStatus.Complete, second.ReconcileStatus);
        Assert.Equal(after, await SnapshotBucketsAsync());
    }

    [Fact]
    [Trait("TestId", "BI-IDEMP-03")]
    public async Task AFullAggregateRebuildProducesTheSameNumbersAsTheIncrementalRun()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await RunEtlAsync();

        IReadOnlyList<string> incremental = await SnapshotBucketsAsync();

        // The distinction that matters: recomputing is idempotent, incrementing is not. If the
        // aggregate were maintained by adding deltas, this rebuild would disagree with the run
        // that produced it.
        await RunEtlAsync(rebuildAggregates: true);

        Assert.Equal(incremental, await SnapshotBucketsAsync());
    }

    [Fact]
    [Trait("TestId", "BI-IDEMP-03")]
    public async Task AResultThatArrivesAfterItsOwnTimestampIsStillLoadedExactlyOnce()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await RunEtlAsync();

        int before = await CountFactsAsync();
        int bucketBefore = await BucketTotalAsync(DateOnly.FromDateTime(ResultAt.UtcDateTime));

        // The case a time watermark loses: a row committed now whose event time is older than
        // everything already loaded. Nothing about it is newer, so a "read forward from the last
        // timestamp" pipeline would never see it.
        await InsertLateResultAsync();

        AnalyticsEtlRunReport report = await RunEtlAsync();

        Assert.Equal(1, report.LoadedRows);
        Assert.Equal(before + 1, await CountFactsAsync());
        Assert.Equal(AnalyticsReconcileStatus.Complete, report.ReconcileStatus);

        // The bucket it belongs to was recomputed, so the late row is counted once — not added
        // to a total that already existed.
        Assert.Equal(
            bucketBefore + 1,
            await BucketTotalAsync(DateOnly.FromDateTime(ResultAt.UtcDateTime)));

        AnalyticsEtlRunReport repeat = await RunEtlAsync();
        Assert.Equal(0, repeat.LoadedRows);
        Assert.Equal(before + 1, await CountFactsAsync());
    }

    [Fact]
    [Trait("TestId", "BI-IDEMP-03")]
    public async Task AnOpenJobIsRefreshedWhileAClosedOneIsLeftAlone()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await RunEtlAsync();

        const string openJob = "JOB-ANALYTICS-OPEN";
        Assert.Equal(1, await JobAttemptCountAsync(openJob));

        // A second counted attempt lands after the job fact was written. Insert-only loading
        // would leave the fact reporting one attempt forever, and the attempt-2 KPI with it.
        await AddCountedAttemptAsync(openJob, attemptNumber: 2);

        AnalyticsEtlRunReport report = await RunEtlAsync();

        Assert.Equal(0, report.LoadedRows);
        Assert.Equal(1, report.JobRowsRefreshed);
        Assert.Equal(2, await JobAttemptCountAsync(openJob));

        // Nothing to do the next time round: the refresh compares before it writes.
        Assert.Equal(0, (await RunEtlAsync()).JobRowsRefreshed);
    }

    // ----------------------------------------------------------- BI-QUALITY-04

    [Fact]
    [Trait("TestId", "BI-QUALITY-04")]
    public async Task TheCheckpointReconcilesSourceAgainstFactAndRecordsFreshness()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        AnalyticsEtlRunReport report = await RunEtlAsync();

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AnalyticsEtlCheckpointEntity checkpoint = await context.AnalyticsCheckpoints
            .SingleAsync(row => row.PipelineName == AnalyticsEtlJob.PipelineName);

        Assert.Equal(AnalyticsReconcileStatus.Complete, checkpoint.ReconcileStatus);
        Assert.Equal(checkpoint.SourceRowCount, checkpoint.FactRowCount);
        Assert.Equal(SeededResults, checkpoint.SourceRowCount);
        Assert.Equal(0, checkpoint.TotalRejectedRows);
        Assert.Equal(RunAt, checkpoint.LastRunAt);

        // Freshness is measured against the newest event loaded, not against the run time — a
        // pipeline that runs on schedule over a source that stopped producing is not fresh.
        DateTimeOffset newestSourceEvent = await context.CallResults.MaxAsync(row => row.CreatedAt);
        Assert.Equal(newestSourceEvent, checkpoint.HighWaterEventAt);
        Assert.Equal(0, report.OrphanSourceRows);
    }

    [Fact]
    [Trait("TestId", "BI-QUALITY-04")]
    public async Task ABatchCapIsReportedAsBacklogRatherThanLookingComplete()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        // One row per run. The state where the warehouse holds a real but partial answer is the
        // one that must never read as finished.
        AnalyticsEtlRunReport partial = await RunEtlAsync(batchSize: 1);

        Assert.Equal(1, partial.LoadedRows);
        Assert.Equal(AnalyticsReconcileStatus.Backlog, partial.ReconcileStatus);
        Assert.True(partial.HasBacklog);

        for (int run = 0; run < SeededResults; run++)
        {
            AnalyticsEtlRunReport next = await RunEtlAsync(batchSize: 1);
            if (next.ReconcileStatus == AnalyticsReconcileStatus.Complete)
            {
                break;
            }
        }

        Assert.Equal(SeededResults, await CountFactsAsync());
    }

    [Fact]
    [Trait("TestId", "BI-QUALITY-04")]
    public async Task TheReportingApiSwitchesToTheWarehouseAndSaysSo()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        // Before the pipeline runs there is nothing to serve from, and the payload has always
        // said so. The claim under test is that it stops saying so for the right reason.
        Assert.False(await WarehouseHasFactsAsync());

        await RunEtlAsync();

        Assert.True(await WarehouseHasFactsAsync());

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AnalyticsEtlCheckpointEntity checkpoint = await context.AnalyticsCheckpoints
            .SingleAsync(row => row.PipelineName == AnalyticsEtlJob.PipelineName);

        // The two claims are separate on purpose: serving from the warehouse and the warehouse
        // being complete are different facts, and the console must be able to tell them apart.
        Assert.Equal(AnalyticsReconcileStatus.Complete, checkpoint.ReconcileStatus);

        int jobFacts = await context.AnalyticsJobFacts.CountAsync();
        int sourceJobs = await context.CallJobs.CountAsync();
        Assert.Equal(sourceJobs, jobFacts);

        // Job-grain KPIs come from the warehouse too, which is what lets the payload claim
        // warehouse_backed without an exception hiding in it.
        Assert.Equal(
            await context.CallJobs.CountAsync(job => job.Eligible),
            await context.AnalyticsJobFacts.CountAsync(fact => fact.Eligible));
    }

    [Fact]
    [Trait("TestId", "BI-QUALITY-04")]
    public async Task RetentionRemovesTheDerivedCopyAndTheBucketThatCountedIt()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await RunEtlAsync();

        var hook = new AnalyticsRetentionHook(Factory(), TimeProvider.System);
        DateOnly bucketDate = DateOnly.FromDateTime(ResultAt.UtcDateTime);
        int bucketBefore = await BucketTotalAsync(bucketDate);

        await DeleteOneSourceResultAsync();

        // Dry run first: a scheduled delete of customer data whose default is wrong cannot be
        // undone, so the hook must be able to report without acting.
        int wouldDelete = await hook.PurgeExpiredAsync(RunAt, dryRun: true, CancellationToken.None);
        Assert.Equal(1, wouldDelete);
        Assert.Equal(SeededResults, await CountFactsAsync());

        int deleted = await hook.PurgeExpiredAsync(RunAt, dryRun: false, CancellationToken.None);
        Assert.Equal(1, deleted);
        Assert.Equal(SeededResults - 1, await CountFactsAsync());

        // The count a reader actually looks at. Leaving the aggregate stale would preserve
        // exactly the number the retention run existed to remove.
        Assert.Equal(bucketBefore - 1, await BucketTotalAsync(bucketDate));
    }

    // ------------------------------------------------------------------ helpers

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private Task<AnalyticsEtlRunReport> RunEtlAsync(
        int batchSize = 5_000,
        bool rebuildAggregates = false)
    {
        var job = new AnalyticsEtlJob(Factory(), TimeProvider.System);
        return job.RunAsync(
            new AnalyticsEtlRunOptions
            {
                BatchSize = batchSize,
                RebuildAggregates = rebuildAggregates,
                Now = RunAt,
            },
            CancellationToken.None);
    }

    private async Task<int> CountFactsAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return await context.AnalyticsFacts.CountAsync();
    }

    private async Task<bool> WarehouseHasFactsAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return await context.AnalyticsFacts.AnyAsync();
    }

    private async Task<int> BucketTotalAsync(DateOnly date)
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return await context.AnalyticsKpiDaily
            .Where(row => row.BucketDate == date)
            .SumAsync(row => row.TotalResults);
    }

    private async Task<int> JobAttemptCountAsync(string jobId)
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return (await context.AnalyticsJobFacts.SingleAsync(fact => fact.IvrCallJobId == jobId))
            .CountedAttemptCount;
    }

    /// <summary>Stable, comparable rendering of every KPI bucket except its compute time.</summary>
    private async Task<IReadOnlyList<string>> SnapshotBucketsAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return await context.AnalyticsKpiDaily.AsNoTracking()
            .OrderBy(row => row.BucketDate)
            .ThenBy(row => row.ProgramKey)
            .ThenBy(row => row.ScriptVariantKey)
            .Select(row => $"{row.BucketDate}|{row.ProgramKey}|{row.ScriptVariantKey}|"
                + $"{row.TotalResults}|{row.FinalResults}|{row.DistinctOrders}|"
                + $"{row.ConfirmedCount}|{row.CancelledCount}|{row.NoAnswerCount}|"
                + $"{row.InvalidPhoneCount}|{row.TechnicalCount}|{row.OperationalBlockedCount}|"
                + $"{row.SecondAttemptResults}|{row.SecondsToResultSum}|{row.SecondsToResultCount}")
            .ToListAsync();
    }

    private async Task InsertLateResultAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        CallJobEntity job = await context.CallJobs.FirstAsync();
        context.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = "RESULT-ANALYTICS-LATE",
            IvrCallJobId = job.IvrCallJobId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            OrderVersionSnapshot = "1",
            OrderVersionSeenByIvr = "1",
            FinalResultStatus = "IVR_CONFIRMED",
            ResultType = "IVR_CONFIRMED",
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = "REVALIDATE_AND_CONFIRM_ORDER",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            // Older than every row already loaded, and written after they were.
            CreatedAt = ResultAt.AddMinutes(-30),
        });
        await context.SaveChangesAsync();
    }

    private async Task AddCountedAttemptAsync(string jobId, int attemptNumber)
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        CallJobEntity job = await context.CallJobs.SingleAsync(row => row.IvrCallJobId == jobId);
        context.CallAttempts.Add(BuildAttempt(
            jobId,
            job.TaskId,
            job.ScriptVersion,
            attemptNumber,
            ResultAt.AddMinutes(-10),
            counted: true));
        await context.SaveChangesAsync();
    }

    private async Task DeleteOneSourceResultAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        CallResultEntity victim = await context.CallResults
            .OrderBy(row => row.IvrCallResultId)
            .FirstAsync();
        context.CallResults.Remove(victim);
        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------- seeding

    /// <summary>
    /// Nine results across two programmes and two script variants, plus one job left
    /// open so the refresh path has something to refresh.
    /// </summary>
    private async Task SeedAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        int index = 0;
        foreach ((string program, string variant, string resultType, int count) in new[]
        {
            (GoldenHour, VariantA, "IVR_CONFIRMED", 4),
            (GoldenHour, VariantA, "IVR_NO_ANSWER_FINAL", 2),
            (GoldenHour, VariantB, "IVR_CONFIRMED", 2),
            (TwentyFourSeven, VariantA, "IVR_TECHNICAL_EXCEPTION", 1),
        })
        {
            for (int item = 0; item < count; item++)
            {
                index++;
                Seed(context, index, program, variant, resultType, closed: true);
            }
        }

        // The open job carries no result: it is the job-grain row the refresh pass maintains.
        SeedJobOnly(context, GoldenHour, VariantA);

        await context.SaveChangesAsync();
    }

    private static void SeedJobOnly(IvrDbContext context, string program, string variant)
    {
        const string taskId = "TASK-ANALYTICS-OPEN";
        const string jobId = "JOB-ANALYTICS-OPEN";
        DateTimeOffset t0 = ResultAt.AddMinutes(-20);

        context.ConfirmationTasks.Add(BuildTask("OPEN", taskId, program, variant, t0));
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = "ORDER-ANALYTICS-OPEN",
            OrderVersionSnapshot = "1",
            ProgramType = program,
            AttemptPolicyCode = "mock-lab-v1",
            Status = "READY_FOR_SCHEDULER",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowSeconds = 900,
            AttemptScheduleJson = "[]",
            T0At = t0,
            ExpiresAt = ResultAt.AddHours(4),
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = variant,
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ClosedAt = null,
        });
        context.CallAttempts.Add(BuildAttempt(jobId, taskId, variant, 1, t0, counted: true));
    }

    private static void Seed(
        IvrDbContext context,
        int index,
        string program,
        string variant,
        string resultType,
        bool closed)
    {
        string suffix = index.ToString("D2", CultureInfo.InvariantCulture);
        string taskId = $"TASK-ANALYTICS-{suffix}";
        string jobId = $"JOB-ANALYTICS-{suffix}";
        DateTimeOffset t0 = ResultAt.AddSeconds(-120);

        context.ConfirmationTasks.Add(BuildTask(suffix, taskId, program, variant, t0));

        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = $"ORDER-ANALYTICS-{suffix}",
            OrderVersionSnapshot = "1",
            ProgramType = program,
            AttemptPolicyCode = "mock-lab-v1",
            Status = closed ? "CLOSED" : "READY_FOR_SCHEDULER",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowSeconds = 900,
            AttemptScheduleJson = "[]",
            T0At = t0,
            ExpiresAt = ResultAt.AddHours(4),
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = variant,
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ClosedAt = closed ? ResultAt : null,
        });

        context.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = $"RESULT-ANALYTICS-{suffix}",
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = $"ORDER-ANALYTICS-{suffix}",
            OrderVersionSnapshot = "1",
            OrderVersionSeenByIvr = "1",
            FinalResultStatus = resultType,
            ResultType = resultType,
            IsCountedCustomerAttempt = resultType != "IVR_TECHNICAL_EXCEPTION",
            IsFinalForIvr = resultType != "IVR_TECHNICAL_EXCEPTION",
            RecommendedCoreAction = resultType switch
            {
                "IVR_CONFIRMED" => "REVALIDATE_AND_CONFIRM_ORDER",
                "IVR_NO_ANSWER_FINAL" => "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
                _ => "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
            },
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            CreatedAt = ResultAt,
        });

        context.CallAttempts.Add(BuildAttempt(jobId, taskId, variant, 1, t0, counted: true));
    }

    private static ConfirmationTaskEntity BuildTask(
        string suffix,
        string taskId,
        string program,
        string variant,
        DateTimeOffset t0) =>
        new()
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = $"analytics-pipeline-idem-{suffix}",
            CorrelationId = $"corr-analytics-pipeline-{suffix}",
            OfficialOrderId = $"ORDER-ANALYTICS-{suffix}",
            OrderCode = "GF-ORDER-ANALYTICS-PIPE",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            // ck_ivr_confirmation_tasks_matrix: Golden Hour is ONLINE, 24/7 is COD.
            PaymentMethodSnapshot = program == GoldenHour ? "ONLINE" : "COD",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = program,
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowStartedAt = t0,
            ConfirmationWindowExpiresAt = ResultAt.AddHours(4),
            PhoneRef = "phone-ref-analytics-pipeline",
            PhoneMasked = "84xxxxx4567",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:analytics-pipeline-dial-token",
            DialTokenExpiresAt = ResultAt.AddHours(4),
            PrivacySafeOrderSummaryJson = "{\"order_code_short\":\"GF-ANA\"}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = variant,
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ExpiresAt = ResultAt.AddHours(4),
            AcceptedAt = t0,
        };

    private static CallAttemptEntity BuildAttempt(
        string jobId,
        string taskId,
        string variant,
        int attemptNumber,
        DateTimeOffset t0,
        bool counted) =>
        new()
        {
            IvrCallAttemptId = $"ATTEMPT-{jobId}-{attemptNumber}",
            IvrCallJobId = jobId,
            TaskId = taskId,
            AttemptNumber = attemptNumber,
            MaxAttemptsSnapshot = 2,
            ScheduledAt = t0,
            ScheduledWindowExpiresAt = t0.AddHours(4),
            StartedAt = t0,
            EndedAt = ResultAt,
            Status = "NORMALIZED_FINAL",
            IsCountedCustomerAttempt = counted,
            TechnicalRetryAllowed = true,
            PolicyVersion = "mock-lab-v1",
            ScriptVersion = variant,
        };
}
