using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0098 — aggregate-only reporting projections consumed by the P3-4 console.
///
/// The seeded shape is chosen so the k-anonymity threshold is actually exercised:
/// 11 Golden Hour results on script variant `vA` stay above
/// <see cref="AnalyticsReadService.MinBucketSize"/>, while the 2 results on the
/// 24/7 variant `vB` are below it and must never leave the service.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class AnalyticsApiTests(PostgresPersistenceFixture fixture)
{
    private const string BasePath = "/v1/ivr/order-confirmation/analytics";
    private const string VariantA = "SCRIPT-ORDER-CONFIRM:vA";
    private const string VariantB = "SCRIPT-ORDER-CONFIRM:vB";
    private const int ConfirmedCount = 6;
    private const int NoAnswerCount = 5;
    private const int TechnicalCount = 2;
    private const int TotalResults = ConfirmedCount + NoAnswerCount + TechnicalCount;
    private const int SecondAttemptJobs = 3;
    private const int SecondsToFinal = 120;

    private static readonly DateTimeOffset ResultAt =
        new(2026, 8, 14, 9, 30, 0, TimeSpan.Zero);

    /// <summary>The two taxonomy buckets that stay above the threshold.</summary>
    private static readonly string[] SurvivingResultTypes =
        ["IVR_CONFIRMED", "IVR_NO_ANSWER_FINAL"];

    private static readonly string[] Routes =
    [
        $"{BasePath}/summary",
        $"{BasePath}/trend",
        $"{BasePath}/breakdown",
        $"{BasePath}/export?reason=weekly%20review",
    ];

    [Fact]
    [Trait("TestId", "IT-ANALYTICS-01")]
    public async Task ReportingRoutesAreMappedAndGatedByQueueViewPermission()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        foreach (string route in Routes)
        {
            using HttpResponseMessage allowed = await SendAsync(app, route, IvrPermissions.QueueView);
            Assert.True(
                allowed.StatusCode == HttpStatusCode.OK,
                $"{route} -> {allowed.StatusCode}: {await allowed.Content.ReadAsStringAsync()}");

            using HttpResponseMessage forbidden = await SendAsync(app, route, IvrPermissions.ManualRetry);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            using JsonDocument envelope = JsonDocument.Parse(
                await forbidden.Content.ReadAsStringAsync());
            Assert.Equal(
                IvrErrorCodes.ForbiddenCaller,
                envelope.RootElement.GetProperty("error").GetProperty("code").GetString());
        }
    }

    [Fact]
    [Trait("TestId", "IT-ANALYTICS-02")]
    public async Task SummaryComputesKpiFromTheServerAndStatesItsOwnSource()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage response = await SendAsync(
            app,
            $"{BasePath}/summary",
            IvrPermissions.QueueView);
        IvrServer.IvrAnalyticsSummary summary =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrAnalyticsSummary>())!;

        Assert.Equal(TotalResults, summary.Kpi.Total_results);
        Assert.Equal(TotalResults, summary.Kpi.Total_call_jobs);
        Assert.Equal(TotalResults, summary.Kpi.Total_eligible_tasks);
        Assert.Equal(Round(ConfirmedCount, TotalResults), summary.Kpi.Confirm_rate);
        Assert.Equal(Round(NoAnswerCount, TotalResults), summary.Kpi.No_answer_rate);
        Assert.Equal(Round(TechnicalCount, TotalResults), summary.Kpi.Technical_rate);
        Assert.Equal(0d, summary.Kpi.Cancel_rate);
        Assert.Null(summary.Kpi.Operational_blocked_rate);

        // Only counted customer attempts move this number; the seeded technical
        // retries must not (DT-02).
        Assert.Equal(Round(SecondAttemptJobs, TotalResults), summary.Kpi.Attempt_2_rate);
        Assert.Equal(SecondsToFinal, summary.Kpi.Avg_seconds_to_final);

        // The console must be able to say where the numbers came from.
        Assert.False(summary.Data_quality.Warehouse_backed);
        Assert.Equal(AnalyticsReadService.SourceLabel, summary.Data_quality.Source);
        Assert.Equal(AnalyticsReadService.PipelineWorkId, summary.Data_quality.Pipeline_work_id);
        Assert.Equal(AnalyticsReadService.MinBucketSize, summary.Data_quality.Min_bucket_size);
        Assert.False(summary.Data_quality.Truncated);
        Assert.Equal(TotalResults, summary.Data_quality.Scanned_rows);

        // The 2-result technical bucket is below the threshold and is reported as
        // suppressed rather than listed.
        Assert.Equal(1, summary.Data_quality.Suppressed_bucket_count);
        Assert.Equal(
            SurvivingResultTypes,
            summary.Result_taxonomy.Select(row => row.Key).Order().ToArray());
    }

    [Fact]
    [Trait("TestId", "IT-ANALYTICS-03")]
    public async Task SmallBucketsAreSuppressedInTrendAndEveryBreakdownDimension()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        using HttpResponseMessage trendResponse = await SendAsync(
            app,
            $"{BasePath}/trend?bucket=DAY",
            IvrPermissions.QueueView);
        IvrServer.IvrAnalyticsTrend trend =
            (await trendResponse.Content.ReadFromJsonAsync<IvrServer.IvrAnalyticsTrend>())!;

        // Only the Golden Hour bucket survives; the 24/7 bucket is omitted, not
        // returned with zeroed counts.
        IvrServer.IvrAnalyticsTrendBucket bucket = Assert.Single(trend.Buckets);
        Assert.Equal("GOLDEN_HOUR", bucket.Program);
        Assert.Equal(ConfirmedCount + NoAnswerCount, bucket.Total);
        Assert.Equal(ConfirmedCount, bucket.Confirmed);
        Assert.Equal(NoAnswerCount, bucket.No_answer);
        Assert.Equal(0, bucket.Technical);
        Assert.Equal(1, trend.Data_quality.Suppressed_bucket_count);
        Assert.DoesNotContain("TWENTY_FOUR_SEVEN", trend.Buckets.Select(entry => entry.Program));

        foreach ((string dimension, string keptKey, string hiddenKey) in new[]
        {
            ("SCRIPT_VARIANT", VariantA, VariantB),
            ("PROGRAM", "GOLDEN_HOUR", "TWENTY_FOUR_SEVEN"),
        })
        {
            using HttpResponseMessage response = await SendAsync(
                app,
                $"{BasePath}/breakdown?dimension={dimension}",
                IvrPermissions.QueueView);
            IvrServer.IvrAnalyticsBreakdown breakdown =
                (await response.Content.ReadFromJsonAsync<IvrServer.IvrAnalyticsBreakdown>())!;

            IvrServer.IvrAnalyticsBreakdownRow row = Assert.Single(breakdown.Rows);
            Assert.Equal(keptKey, row.Key);
            Assert.Equal(ConfirmedCount + NoAnswerCount, row.Total);
            Assert.Equal(1, breakdown.Data_quality.Suppressed_bucket_count);
            Assert.DoesNotContain(hiddenKey, breakdown.Rows.Select(entry => entry.Key));
        }
    }

    [Fact]
    [Trait("TestId", "IT-ANALYTICS-04")]
    public async Task ExportDemandsAReasonWritesAuditAndRefusesAReIdentifyingSlice()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        // A missing or too-short reason is rejected before any data is read.
        foreach (string query in new[] { string.Empty, "?reason=short" })
        {
            using HttpResponseMessage rejected = await SendAsync(
                app,
                $"{BasePath}/export{query}",
                IvrPermissions.QueueView);
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        using HttpResponseMessage response = await SendAsync(
            app,
            $"{BasePath}/export?dimension=PROGRAM&reason=weekly%20confirm-rate%20review",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        IvrServer.IvrAnalyticsExport export =
            (await response.Content.ReadFromJsonAsync<IvrServer.IvrAnalyticsExport>())!;

        Assert.Equal(IvrServer.IvrAnalyticsExportDimension.PROGRAM, export.Dimension);
        Assert.Equal(1, export.Suppressed_row_count);
        string[] row = Assert.Single(export.Rows).ToArray();
        Assert.Equal("GOLDEN_HOUR", row[1]);
        Assert.False(string.IsNullOrWhiteSpace(export.Audit_ref));

        // The extract carries aggregates only: no identifier from the seeded
        // order, phone or dial token can appear anywhere in the payload (D-05).
        foreach (string forbidden in new[]
        {
            "GF-ORDER-ANALYTICS-FULL",
            "84901234567",
            "84xxxxx4567",
            "enc:analytics-dial-token",
            "phone_ref",
            "dial_token",
            "TASK-ANALYTICS",
        })
        {
            Assert.DoesNotContain(forbidden, body, StringComparison.OrdinalIgnoreCase);
        }

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AuditLogEntity audit = await context.AuditLog.AsNoTracking()
            .SingleAsync(entry => entry.Action == AnalyticsReadService.ExportAuditAction);
        Assert.Equal("weekly confirm-rate review", audit.Reason);
        Assert.Equal("operator-analytics", audit.ActorId);

        // A slice narrow enough that nothing survives suppression is refused
        // rather than answered with an empty extract.
        using HttpResponseMessage refused = await SendAsync(
            app,
            $"{BasePath}/export?program=TWENTY_FOUR_SEVEN&reason=isolate%20the%20small%20cohort",
            IvrPermissions.QueueView);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        using JsonDocument envelope = JsonDocument.Parse(await refused.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.PiiPolicyViolation,
            envelope.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    [Trait("TestId", "IT-ANALYTICS-05")]
    public async Task NoReportingRouteExposesAMutationOrACallerTunableThreshold()
    {
        await fixture.ResetAsync();
        await SeedAsync();
        await using InternalAdminApiTestApplication app = await StartAsync();

        foreach (string route in Routes)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, route.Split('?')[0]);
            request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, IvrPermissions.QueueView);
            request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-analytics");
            request.Headers.Add("X-Actor-Id", "operator-analytics");
            request.Headers.Add("X-Correlation-Id", string.Concat("corr-", Guid.NewGuid().ToString("N")));
            request.Content = JsonContent.Create(new { reason = "attempted write" });
            using HttpResponseMessage response = await app.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        // k-anonymity is a server constant. Supplying the parameter must not
        // widen what a caller can see.
        using HttpResponseMessage tampered = await SendAsync(
            app,
            $"{BasePath}/breakdown?dimension=PROGRAM&min_bucket_size=1",
            IvrPermissions.QueueView);
        IvrServer.IvrAnalyticsBreakdown breakdown =
            (await tampered.Content.ReadFromJsonAsync<IvrServer.IvrAnalyticsBreakdown>())!;
        Assert.Equal(AnalyticsReadService.MinBucketSize, breakdown.Data_quality.Min_bucket_size);
        Assert.Single(breakdown.Rows);
        Assert.Equal(1, breakdown.Data_quality.Suppressed_bucket_count);
    }

    private static double Round(int value, int total) =>
        Math.Round((double)value / total, 4);

    private Task<InternalAdminApiTestApplication> StartAsync() =>
        InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static Task<HttpResponseMessage> SendAsync(
        InternalAdminApiTestApplication app,
        string route,
        string permission)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, route);
        request.Headers.Add(MockPermissionAuthenticationHandler.HeaderName, permission);
        request.Headers.Add(MockPermissionAuthenticationHandler.ActorHeaderName, "operator-analytics");
        request.Headers.Add("X-Actor-Id", "operator-analytics");
        request.Headers.Add(
            "X-Correlation-Id",
            string.Concat("corr-", Guid.NewGuid().ToString("N")));
        return app.Client.SendAsync(request);
    }

    private async Task SeedAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        int index = 0;
        foreach ((string program, string variant, string resultType, int count) in new[]
        {
            ("GOLDEN_HOUR", VariantA, "IVR_CONFIRMED", ConfirmedCount),
            ("GOLDEN_HOUR", VariantA, "IVR_NO_ANSWER_FINAL", NoAnswerCount),
            ("TWENTY_FOUR_SEVEN", VariantB, "IVR_TECHNICAL_EXCEPTION", TechnicalCount),
        })
        {
            for (int item = 0; item < count; item++)
            {
                index++;
                Seed(context, index, program, variant, resultType, secondAttempt: index <= SecondAttemptJobs);
            }
        }

        await context.SaveChangesAsync();
    }

    private static void Seed(
        IvrDbContext context,
        int index,
        string program,
        string variant,
        string resultType,
        bool secondAttempt)
    {
        string suffix = index.ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
        string taskId = $"TASK-ANALYTICS-{suffix}";
        string jobId = $"JOB-ANALYTICS-{suffix}";
        DateTimeOffset t0 = ResultAt.AddSeconds(-SecondsToFinal);

        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = $"analytics-idem-{suffix}",
            CorrelationId = $"corr-analytics-{suffix}",
            OfficialOrderId = $"ORDER-ANALYTICS-{suffix}",
            OrderCode = "GF-ORDER-ANALYTICS-FULL",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            // ck_ivr_confirmation_tasks_matrix: Golden Hour is the ONLINE-payment
            // programme, 24/7 is COD.
            PaymentMethodSnapshot = program == "GOLDEN_HOUR" ? "ONLINE" : "COD",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = program,
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowStartedAt = t0,
            ConfirmationWindowExpiresAt = ResultAt.AddHours(4),
            PhoneRef = "phone-ref-analytics",
            PhoneMasked = "84xxxxx4567",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:analytics-dial-token",
            DialTokenExpiresAt = ResultAt.AddHours(4),
            PrivacySafeOrderSummaryJson = "{\"order_code_short\":\"GF-ANA\"}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = variant,
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            SellableStatusJson = "[]",
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ExpiresAt = ResultAt.AddHours(4),
            AcceptedAt = t0,
        });

        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = $"ORDER-ANALYTICS-{suffix}",
            OrderVersionSnapshot = "1",
            ProgramType = program,
            AttemptPolicyCode = "mock-lab-v1",
            Status = "CLOSED",
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
            ClosedAt = ResultAt,
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
            IsFinalForIvr = true,
            RecommendedCoreAction = "CORE_REVALIDATE_AND_CONTINUE",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            CreatedAt = ResultAt,
        });

        context.CallAttempts.Add(BuildAttempt(jobId, taskId, variant, 1, t0, counted: true));
        if (secondAttempt)
        {
            context.CallAttempts.Add(BuildAttempt(jobId, taskId, variant, 2, t0, counted: true));
        }
        else
        {
            // A technical retry on attempt 1 must never register as attempt 2.
            CallAttemptEntity retried = BuildAttempt(jobId, taskId, variant, 1, t0, counted: false);
            retried.IvrCallAttemptId = $"ATTEMPT-ANALYTICS-{suffix}-R";
            retried.TechnicalRetryCount = 1;
            context.CallAttempts.Add(retried);
        }
    }

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
            Status = "COMPLETED",
            IsCountedCustomerAttempt = counted,
            TechnicalRetryAllowed = true,
            PolicyVersion = "mock-lab-v1",
            ScriptVersion = variant,
        };
}
