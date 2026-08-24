using System.Globalization;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Governance;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests.Governance;

/// <summary>
/// W-0052 / P10-1 §8 — <c>COMP-DSAR-02</c> and <c>COMP-RETENTION-04</c> against real
/// PostgreSQL, because both properties are enforced by the database rather than by
/// the code that talks to it: audit immutability is a trigger, and a retention pass
/// is a transaction.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ComplianceTests(PostgresPersistenceFixture fixture)
{
    private const string OrderCode = "GF-ORDER-DSAR-001";
    private const string OtherOrderCode = "GF-ORDER-DSAR-002";

    private static readonly DateTimeOffset Now = new(2026, 8, 14, 9, 30, 0, TimeSpan.Zero);

    // ------------------------------------------------------------- COMP-DSAR-02

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task FindReportsCountsAndTheLimitsBeforeAnythingIsPromised()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarFindReport report = await Service().FindAsync(OrderCode, CancellationToken.None);

        Assert.True(report.Found);
        Assert.Equal(1, Rows(report, "ivr_confirmation_tasks"));
        Assert.Equal(1, Rows(report, "ivr_call_jobs"));
        Assert.Equal(1, Rows(report, "ivr_call_results"));

        // Counts, never values. A service that printed the stored personal data would be a new
        // way to read it, available to whoever can call the service.
        Assert.All(report.Holdings, holding => Assert.False(
            holding.Table.Contains("phone", StringComparison.OrdinalIgnoreCase)));

        // The limits are part of the answer, not a discovery made while answering.
        Assert.Equal(3, report.NotErasable.Count);
        Assert.Contains(report.NotErasable, limit =>
            limit.Contains("append-only", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task AnUnknownOrderIsAnAnsweredRequestNotAnError()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarFindReport report = await Service()
            .FindAsync("GF-ORDER-NOT-HERE", CancellationToken.None);

        Assert.False(report.Found);
        Assert.Empty(report.Holdings);
        // Still returns the limits: "we hold nothing" is an answer that has to be as complete as
        // any other, or the next request asks the same question again.
        Assert.NotEmpty(report.NotErasable);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ADryRunChangesNothingAndIsStillAudited()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarErasureReport report = await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-dry",
            dryRun: true,
            CancellationToken.None);

        Assert.True(report.DryRun);
        Assert.Equal(0, report.TasksRedacted);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);
        Assert.NotEqual("redacted", task.PhoneRef);
        Assert.Null(task.AnonymizedAt);

        // Audited anyway. A request that changed nothing is still a request that was answered,
        // and the answer has to be as durable as the erasure would have been.
        Assert.Single(await context.AuditLog
            .Where(entry => entry.Action == DsarService.EraseAuditAction)
            .ToListAsync());
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ErasureRedactsTheSubjectAndLeavesEveryOtherOrderAlone()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarErasureReport report = await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-real",
            dryRun: false,
            CancellationToken.None);

        Assert.False(report.DryRun);
        Assert.Equal(1, report.TasksRedacted);
        Assert.False(string.IsNullOrWhiteSpace(report.AuditRef));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity erased = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);

        Assert.Equal("redacted", erased.PhoneRef);
        Assert.Equal("***", erased.PhoneMasked);
        Assert.Equal("REDACTED", erased.PhoneValidationStatus);
        Assert.Equal("enc:redacted", erased.DialTokenCiphertext);
        Assert.Equal("{}", erased.PrivacySafeOrderSummaryJson);
        Assert.NotNull(erased.AnonymizedAt);

        // The key the request arrived with survives. Erasing it would make every later request
        // about this order unanswerable, including the subject's own.
        Assert.Equal(OrderCode, erased.OrderCode);

        // Blast radius: exactly one order. A DSAR erasure that reached a second customer would be
        // a breach committed while honouring a privacy request.
        ConfirmationTaskEntity untouched = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OtherOrderCode);
        Assert.NotEqual("redacted", untouched.PhoneRef);
        Assert.Null(untouched.AnonymizedAt);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ErasureLeavesTheAuditTrailAndTheDeliveryRecordIntact()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        await using (IvrDbContext before = await Factory().CreateDbContextAsync())
        {
            Assert.Equal(1, await before.ResultCallbacks.CountAsync());
        }

        await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-audit",
            dryRun: false,
            CancellationToken.None);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        // The delivery record keeps its payload. Removing it leaves a record that cannot settle
        // the dispute it exists for; it expires with retention instead.
        ResultCallbackEntity callback = await context.ResultCallbacks.SingleAsync();
        Assert.Contains("order", callback.PayloadJson, StringComparison.OrdinalIgnoreCase);

        // And the audit trail gained a row rather than losing one.
        AuditLogEntity dsarEntry = await context.AuditLog
            .SingleAsync(entry => entry.Action == DsarService.EraseAuditAction);
        Assert.Equal("AGT-PRIVACY-01", dsarEntry.ActorId);
        Assert.Contains("erasure request", dsarEntry.Reason!, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task AnErasureWithoutARealReasonIsRefused()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        // The reason ends up in the audit row, and "ok" in that field is the same as no record.
        await Assert.ThrowsAsync<ArgumentException>(() => Service().EraseAsync(
            OrderCode,
            "ok",
            "AGT-PRIVACY-01",
            "corr-dsar-thin",
            dryRun: false,
            CancellationToken.None));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);
        Assert.NotEqual("redacted", task.PhoneRef);
    }

    // -------------------------------------------------------- COMP-RETENTION-04

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task EveryRetentionClassInTheCatalogIsClassifiedInTheGovernanceMap()
    {
        // The compliance claim is that the retention job enforces the policy the inventory
        // describes. That only holds if the two vocabularies line up: a data class the job
        // executes but nobody classified is a deletion happening under no stated policy.
        await Task.CompletedTask;

        foreach (string dataClass in RetentionDataClasses.All)
        {
            Assert.True(
                DataClassification.GovernedRetentionClasses.Contains(dataClass),
                $"the retention job executes '{dataClass}' but no table declares it.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task EveryTableHoldingPersonalDataHasARetentionClassThatActuallyRemovesIt()
    {
        await Task.CompletedTask;

        // PiiDirect and PiiDerived tables must map to a class the P1-5 job executes. The two
        // exceptions are audit tables, which are PRESERVE on purpose and say so in the inventory.
        foreach ((string table, DataClassEntry entry) in DataClassification.Tables)
        {
            if (entry.Protection is not (DataProtectionClass.PiiDirect or DataProtectionClass.PiiDerived))
            {
                continue;
            }

            bool executed = RetentionDataClasses.All.Contains(entry.RetentionClass)
                || string.Equals(entry.RetentionClass, "analytics_derived", StringComparison.Ordinal);

            Assert.True(
                executed,
                $"{table} holds personal data under retention class '{entry.RetentionClass}', "
                + "which the P1-5 job never executes.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task DerivedAnalyticsCopiesInheritTheirPeriodFromTheSourceRatherThanOwningOne()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        var hook = new AnalyticsRetentionHook(Factory(), TimeProvider.System);

        await using (IvrDbContext seed = await Factory().CreateDbContextAsync())
        {
            // A fact whose source result is already gone: exactly the state a retention pass on
            // the operational tables leaves behind.
            seed.AnalyticsFacts.Add(new Ivr.Infrastructure.Analytics.AnalyticsFactCallOutcomeEntity
            {
                IvrCallResultId = "RESULT-DSAR-GONE",
                IvrCallJobId = "JOB-DSAR-01",
                OrderRefHash = new string('b', 64),
                ProgramKey = "GOLDEN_HOUR",
                ScriptVariantKey = "SCRIPT-ORDER-CONFIRM:vA",
                ResultTypeKey = "IVR_CONFIRMED",
                FinalResultStatus = "IVR_CONFIRMED",
                IsFinal = true,
                IsCountedCustomerAttempt = true,
                CountedAttemptNumber = 1,
                EventAt = Now,
                EventDate = DateOnly.FromDateTime(Now.UtcDateTime),
                EventHour = Now.Hour,
                SecondsToResult = 60,
                LoadedAt = Now,
            });
            await seed.SaveChangesAsync();
        }

        int deleted = await hook.PurgeExpiredAsync(Now, dryRun: false, CancellationToken.None);

        // The warehouse period equals the source period by construction: there is no second
        // period to configure, so there is no way to configure the two inconsistently.
        Assert.Equal(1, deleted);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Equal(0, await context.AnalyticsFacts
            .CountAsync(fact => fact.IvrCallResultId == "RESULT-DSAR-GONE"));
    }

    // ------------------------------------------------------------------ helpers

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private DsarService Service() => new(
        Factory(),
        fixture.Services.GetRequiredService<IAuditLogger>(),
        TimeProvider.System);

    private static int Rows(DsarFindReport report, string table) =>
        report.Holdings.Single(holding => holding.Table == table).RowCount;

    private async Task SeedAsync()
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Seed(context, "01", OrderCode, withCallback: true);
        Seed(context, "02", OtherOrderCode, withCallback: false);
        await context.SaveChangesAsync();
    }

    private static void Seed(
        IvrDbContext context,
        string suffix,
        string orderCode,
        bool withCallback)
    {
        string taskId = $"TASK-DSAR-{suffix}";
        string jobId = $"JOB-DSAR-{suffix}";
        string orderId = $"ORDER-DSAR-{suffix}";
        DateTimeOffset t0 = Now.AddMinutes(-5);

        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = $"dsar-idem-{suffix}",
            CorrelationId = $"corr-dsar-{suffix}",
            OfficialOrderId = orderId,
            OrderCode = orderCode,
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowStartedAt = t0,
            ConfirmationWindowExpiresAt = Now.AddHours(4),
            PhoneRef = $"phone-ref-dsar-{suffix}",
            PhoneMasked = "84xxxxx4567",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = $"enc:dsar-token-{suffix}",
            DialTokenExpiresAt = Now.AddHours(4),
            PrivacySafeOrderSummaryJson = "{\"order_code_short\":\"GF-DSAR\"}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "SCRIPT-ORDER-CONFIRM:vA",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            SellableStatusJson = "[]",
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ExpiresAt = Now.AddHours(4),
            AcceptedAt = t0,
        });

        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = orderId,
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "mock-lab-v1",
            Status = "CLOSED",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowSeconds = 900,
            AttemptScheduleJson = "[]",
            T0At = t0,
            ExpiresAt = Now.AddHours(4),
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:vA",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ClosedAt = Now,
        });

        context.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = $"RESULT-DSAR-{suffix}",
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = orderId,
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
            CreatedAt = Now,
        });

        if (!withCallback)
        {
            return;
        }

        context.ResultCallbacks.Add(new ResultCallbackEntity
        {
            CallbackId = $"CALLBACK-DSAR-{suffix}",
            IvrCallResultId = $"RESULT-DSAR-{suffix}",
            TaskId = taskId,
            OfficialOrderId = orderId,
            IdempotencyKey = $"dsar-callback-idem-{suffix}",
            ResultStatus = "IVR_CONFIRMED",
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "DELIVERED_ACCEPTED",
            RequiresCoreRevalidation = true,
            PayloadJson = string.Create(
                CultureInfo.InvariantCulture,
                $"{{\"order_ref\":\"{orderId}\",\"result\":\"IVR_CONFIRMED\"}}"),
            // ck_ivr_result_callbacks_hash: uppercase hex only.
            PayloadSha256 = new string('A', 64),
            CreatedAt = Now,
            SentAt = Now,
            AcknowledgedAt = Now,
        });
    }
}
