using System.Text.Json;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ResultNormalizationPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 14, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-NORM-PERSIST-01")]
    public async Task AnsweredOnePersistsFinalSignalEvidenceAndPrivacySafeAuditAtomically()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "ANSWERED", "1");
        ResultRepository repository = Repository(factory);

        NormalizationPersistenceResult result = Assert.IsType<NormalizationPersistenceResult>(
            await repository.NormalizeNextAsync("normalizer-test"));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        CallResultEntity stored = await verification.CallResults.AsNoTracking().SingleAsync();
        ResultCallbackEntity callback = await verification.ResultCallbacks.AsNoTracking().SingleAsync();
        EvidenceEntity evidence = await verification.Evidence.AsNoTracking().SingleAsync();
        AuditLogEntity audit = await verification.AuditLog.AsNoTracking()
            .SingleAsync(row => row.Action == "IVR_RESULT_NORMALIZED");
        Assert.Equal("IVR_CONFIRMED", result.ResultStatus);
        Assert.True(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.Equal("IVR_CONFIRMED", stored.ResultType);
        Assert.Equal("1", stored.DtmfKey);
        Assert.True(stored.InputSignalOnly);
        Assert.True(stored.NoDirectOrderUpdate);
        Assert.True(stored.NoPaymentOrRevenueEffect);
        Assert.Equal("NORMALIZED_FINAL", attempt.Status);
        Assert.True(attempt.IsCountedCustomerAttempt);
        Assert.Equal("RESULT_READY_FOR_CALLBACK", job.Status);
        Assert.Equal("HELD_CALLBACK", job.QueueStatus);
        Assert.Equal("READY", callback.DeliveryStatus);
        Assert.Equal("PENDING_CORE_REVALIDATION", callback.ResultState);
        Assert.Equal("ivr-result:RESULT-RAW-NORM-001", callback.IdempotencyKey);
        Assert.Equal(64, callback.PayloadSha256.Length);
        using (JsonDocument payload = JsonDocument.Parse(callback.PayloadJson))
        {
            Assert.Equal(
                "IVR_CONFIRMED",
                payload.RootElement.GetProperty("result_type").GetString());
            Assert.Equal(
                "CORE_REVALIDATE_AND_CONFIRM_ORDER",
                payload.RootElement.GetProperty("recommended_core_action").GetString());
            Assert.Equal(
                "ORDER-NORM-001",
                payload.RootElement.GetProperty("order_id").GetString());
        }
        Assert.Equal("W-0022", evidence.WorkId);
        Assert.Equal(3, await verification.EvidenceLinks.CountAsync());
        Assert.Equal(1, await verification.RawCallEvents.CountAsync());
        Assert.DoesNotContain("\"dtmf\"", audit.DataJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("84xxxxx0021", audit.DataJson, StringComparison.Ordinal);
        Assert.Null(await repository.NormalizeNextAsync("normalizer-test-repeat"));
        Assert.Equal(1, await verification.CallResults.CountAsync());
        Assert.Equal(1, await verification.ResultCallbacks.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-NORM-TECH-02")]
    public async Task TechnicalAndUnknownEventsNeverCountAndRetryOnlyOnce()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "NETWORKERROR", null, "NETWORK_TEMPORARY");
        ResultRepository repository = Repository(factory);

        NormalizationPersistenceResult first = Assert.IsType<NormalizationPersistenceResult>(
            await repository.NormalizeNextAsync("normalizer-tech-1"));
        await AddPendingAttemptAsync(
            factory,
            "ATTEMPT-NORM-002",
            "RAW-NORM-002",
            "FUTURE_VENDOR_STATUS",
            "vendor unsafe code!",
            Now.AddMinutes(1));
        NormalizationPersistenceResult second = Assert.IsType<NormalizationPersistenceResult>(
            await repository.NormalizeNextAsync("normalizer-tech-2"));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        TechnicalExceptionEntity[] technical = await verification.TechnicalExceptions
            .AsNoTracking()
            .OrderBy(row => row.CreatedAt)
            .ToArrayAsync();
        Assert.Equal("IVR_TECHNICAL_EXCEPTION", first.ResultStatus);
        Assert.False(first.IsCounted);
        Assert.False(first.IsFinal);
        Assert.True(first.TechnicalRetryAllowed);
        Assert.Equal(1, first.TechnicalRetryCount);
        Assert.False(second.IsCounted);
        Assert.False(second.IsFinal);
        Assert.False(second.TechnicalRetryAllowed);
        Assert.Equal(2, second.TechnicalRetryCount);
        Assert.True(second.HumanReviewRequired);
        Assert.Equal(2, technical.Length);
        Assert.All(technical, row => Assert.False(row.CustomerAttemptCounted));
        Assert.Equal("VENDORUNSAFECODE", technical[1].ExceptionType);
        Assert.Equal(0, await verification.CallAttempts.CountAsync(
            row => row.IsCountedCustomerAttempt));
        Assert.Equal("HELD_ADMIN_REVIEW", job.Status);
        Assert.Equal("HELD_TECHNICAL_REVIEW", job.QueueStatus);
        Assert.Single(await verification.ReviewItems.AsNoTracking().ToListAsync());
        Assert.Empty(await verification.ResultCallbacks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-NORM-REJECT-03")]
    public async Task RejectedIsCountedNoAnswerAndCreatesReviewWithoutCancelling()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "REJECTED", null);

        NormalizationPersistenceResult result = Assert.IsType<NormalizationPersistenceResult>(
            await Repository(factory).NormalizeNextAsync("normalizer-rejected"));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallResultEntity stored = await verification.CallResults.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("IVR_NO_ANSWER_ATTEMPT", result.ResultStatus);
        Assert.NotEqual("IVR_CUSTOMER_CANCELLED", stored.ResultType);
        Assert.True(result.IsCounted);
        Assert.False(result.IsFinal);
        Assert.True(result.HumanReviewRequired);
        Assert.Equal("NO_STATE_CHANGE_WAIT_FOR_TIMEOUT", stored.RecommendedCoreAction);
        Assert.Equal("DRY_RUN", job.Status);
        Assert.Equal("HELD_MOCK", job.QueueStatus);
        Assert.Single(await verification.ReviewItems.AsNoTracking().ToListAsync());
        Assert.Empty(await verification.ResultCallbacks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-NORM-INVALID-04")]
    public async Task InvalidDestinationIsFinalReviewAndDoesNotConsumeAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "INVALID_DESTINATION", null);

        NormalizationPersistenceResult result = Assert.IsType<NormalizationPersistenceResult>(
            await Repository(factory).NormalizeNextAsync("normalizer-invalid"));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        Assert.Equal("IVR_INVALID_PHONE_FINAL", result.ResultStatus);
        Assert.False(result.IsCounted);
        Assert.True(result.IsFinal);
        Assert.True(attempt.InvalidPhone);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Empty(await verification.TechnicalExceptions.AsNoTracking().ToListAsync());
        Assert.Single(await verification.ResultCallbacks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-NORM-CONCURRENCY-05")]
    public async Task ConcurrentNormalizersPersistSingleResultForOneRawEvent()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "BUSY", null);
        ResultRepository firstRepository = Repository(factory);
        ResultRepository secondRepository = Repository(factory);

        NormalizationPersistenceResult?[] results = await Task.WhenAll(
            firstRepository.NormalizeNextAsync("normalizer-concurrent-a"),
            secondRepository.NormalizeNextAsync("normalizer-concurrent-b"));

        Assert.Single(results, result => result is not null);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(1, await verification.CallResults.CountAsync());
        Assert.Equal(1, await verification.Evidence.CountAsync());
        Assert.Equal(3, await verification.EvidenceLinks.CountAsync());
        Assert.Empty(await verification.ResultCallbacks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-RACE-12")]
    public async Task BlockerRaisedAfterKeyOneBlocksTheSignalWithoutRewritingTheCallResult()
    {
        // Seed 'SCN-009-race-recall-after-key1': the customer confirmed, and only afterwards did
        // Sales find a blocker during revalidation. P4-2 §3 and D-02 both hinge on this case.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "ANSWERED", "1");
        await Repository(factory).NormalizeNextAsync("normalizer-race-12");
        ICallbackOutboxRepository outbox = fixture.Services
            .GetRequiredService<ICallbackOutboxRepository>();

        CallbackOutboxMessage leased = Assert.Single(await outbox.DequeueReadyAsync(
            10,
            TimeSpan.FromMinutes(1)));
        Assert.True(await outbox.CompleteDeliveryAsync(
            leased.CallbackId,
            leased.LeaseToken,
            new CallbackDeliveryUpdate(
                "DELIVERED_BLOCKED",
                200,
                "BLOCKED_BY_CORE",
                null,
                0,
                null,
                true,
                true)));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallResultEntity result = await verification.CallResults.AsNoTracking().SingleAsync();
        ResultCallbackEntity callback = await verification.ResultCallbacks.AsNoTracking()
            .SingleAsync();

        // What the customer actually did is a fact IVR observed. Sales blocking the order later
        // does not retroactively make the keypress something else, so the stored result stays
        // IVR_CONFIRMED and the counted attempt stays counted.
        Assert.Equal("IVR_CONFIRMED", result.ResultType);
        Assert.True(result.IsCountedCustomerAttempt);

        // The block is recorded on the signal, not on the observation, and surfaces for review.
        Assert.Equal("DELIVERED_BLOCKED", callback.DeliveryStatus);
        Assert.Equal("BLOCKED_BY_CORE", callback.CoreResponseCode);
        Assert.Contains(
            await verification.ReviewItems.AsNoTracking().ToListAsync(),
            item => item.SourceType == "IVR_RESULT_CALLBACK" && item.SourceId == callback.CallbackId);

        // D-02: nothing IVR owns carries an order state it could have written. The task still
        // holds the state Sales sent at intake, untouched by the confirm or by the block.
        ConfirmationTaskEntity task = await verification.ConfirmationTasks.AsNoTracking()
            .SingleAsync();
        Assert.Equal("CONFIRMING", task.OrderState);
        Assert.Equal("1", task.OrderVersion);
    }

    [Fact]
    [Trait("TestId", "IT-CALLBACK-OUTBOX-06")]
    public async Task DeliveryCompletionUsesLeaseFencingAndCreatesAdminVisibleReview()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedPendingAsync(factory, "ANSWERED", "1");
        await Repository(factory).NormalizeNextAsync("normalizer-callback");
        ICallbackOutboxRepository outbox = fixture.Services
            .GetRequiredService<ICallbackOutboxRepository>();

        CallbackOutboxMessage leased = Assert.Single(await outbox.DequeueReadyAsync(
            10,
            TimeSpan.FromMinutes(1)));
        Assert.Equal("GOLDEN_HOUR", leased.ProgramCode);
        Assert.Equal("corr-normalization-001", leased.CorrelationId);
        Assert.Equal("ORDER-NORM-001", leased.OfficialOrderId);
        Assert.False(await outbox.CompleteDeliveryAsync(
            leased.CallbackId,
            "stale-lease",
            new CallbackDeliveryUpdate(
                "DELIVERED_ACCEPTED",
                200,
                "ACCEPTED",
                null,
                0,
                null,
                true,
                false)));
        var update = new CallbackDeliveryUpdate(
            "DELIVERED_BLOCKED",
            200,
            "BLOCKED_BY_CORE",
            null,
            0,
            null,
            true,
            true);
        bool[] completions = await Task.WhenAll(
            outbox.CompleteDeliveryAsync(leased.CallbackId, leased.LeaseToken, update),
            outbox.CompleteDeliveryAsync(leased.CallbackId, leased.LeaseToken, update));
        Assert.Single(completions, completed => completed);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ResultCallbackEntity callback = await verification.ResultCallbacks.AsNoTracking().SingleAsync();
        ReviewItemEntity review = await verification.ReviewItems.AsNoTracking()
            .SingleAsync(item => item.SourceType == "IVR_RESULT_CALLBACK");
        Assert.Equal("DELIVERED_BLOCKED", callback.DeliveryStatus);
        Assert.Equal(200, callback.CoreHttpStatus);
        Assert.Equal("BLOCKED_BY_CORE", callback.CoreResponseCode);
        Assert.NotNull(callback.AcknowledgedAt);
        Assert.Null(callback.LeaseToken);
        Assert.Equal(callback.CallbackId, review.SourceId);
        Assert.Contains(await verification.AuditLog.AsNoTracking().ToListAsync(),
            audit => audit.Action == "IVR_CALLBACK_DELIVERY_STATE_CHANGED"
                && audit.TargetId == callback.CallbackId);
        Assert.Single(await verification.AuditLog.AsNoTracking()
            .Where(audit => audit.Action == "IVR_CALLBACK_DELIVERY_STATE_CHANGED")
            .ToListAsync());
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static ResultRepository Repository(IDbContextFactory<IvrDbContext> factory) => new(
        factory,
        new RawEventRepository(),
        Options.Create(new SchedulerOptions { TechnicalRetryLimit = 1 }),
        new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
        new FixedTimeProvider(Now.AddSeconds(30)));

    private static async Task SeedPendingAsync(
        IDbContextFactory<IvrDbContext> factory,
        string rawStatus,
        string? rawDtmf,
        string? technicalErrorCode = null)
    {
        DateTimeOffset deadline = Now.AddMinutes(10);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = "TASK-NORM-001",
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = "normalization:TASK-NORM-001",
            CorrelationId = "corr-normalization-001",
            OfficialOrderId = "ORDER-NORM-001",
            OrderCode = "GF-NORM-001",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "GH-CANDIDATE-v1",
            MaxAttempts = 3,
            AttemptOffsetsSecondsJson = "[0,150,300]",
            ConfirmationWindowStartedAt = Now,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = "phone-ref-normalization-test",
            PhoneMasked = "84xxxxx0021",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:normalization-token",
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            SellableStatusJson = "[]",
            CreatedAt = Now,
            ExpiresAt = deadline,
            AcceptedAt = Now,
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = "JOB-NORM-001",
            TaskId = "TASK-NORM-001",
            OfficialOrderId = "ORDER-NORM-001",
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "GH-CANDIDATE-v1",
            Status = "DISPOSITION_PENDING_NORMALIZATION",
            MaxAttempts = 3,
            AttemptOffsetsSecondsJson = "[0,150,300]",
            ConfirmationWindowSeconds = 600,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                Now,
                Now.AddSeconds(150),
                Now.AddSeconds(300),
            }),
            T0At = Now,
            ExpiresAt = deadline,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE",
            QueueStatus = "HELD_NORMALIZATION",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = Now,
        });
        AddAttemptAndRaw(
            context,
            "ATTEMPT-NORM-001",
            "RAW-NORM-001",
            rawStatus,
            rawDtmf,
            technicalErrorCode,
            Now);
        await context.SaveChangesAsync();
    }

    private static async Task AddPendingAttemptAsync(
        IDbContextFactory<IvrDbContext> factory,
        string attemptId,
        string rawEventId,
        string rawStatus,
        string? technicalErrorCode,
        DateTimeOffset receivedAt)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        CallJobEntity job = await context.CallJobs.SingleAsync();
        job.Status = "DISPOSITION_PENDING_NORMALIZATION";
        job.QueueStatus = "HELD_NORMALIZATION";
        AddAttemptAndRaw(
            context,
            attemptId,
            rawEventId,
            rawStatus,
            null,
            technicalErrorCode,
            receivedAt);
        await context.SaveChangesAsync();
    }

    private static void AddAttemptAndRaw(
        IvrDbContext context,
        string attemptId,
        string rawEventId,
        string rawStatus,
        string? rawDtmf,
        string? technicalErrorCode,
        DateTimeOffset receivedAt)
    {
        context.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = attemptId,
            IvrCallJobId = "JOB-NORM-001",
            TaskId = "TASK-NORM-001",
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 3,
            ScheduledAt = Now,
            ScheduledWindowExpiresAt = Now.AddMinutes(10),
            StartedAt = receivedAt.AddSeconds(-20),
            EndedAt = receivedAt,
            Status = "PROVIDER_EVENT_PENDING_NORMALIZATION",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = false,
            TechnicalRetryCount = 0,
            NoAnswer = false,
            InvalidPhone = false,
            RawCallEventId = rawEventId,
            Disposition = rawStatus,
            DtmfKey = rawDtmf,
            PolicyVersion = "GH-CANDIDATE-v1",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            EvidenceRefsJson = "[]",
        });
        context.RawCallEvents.Add(new RawCallEventEntity
        {
            RawEventId = rawEventId,
            IvrCallAttemptId = attemptId,
            IvrCallJobId = "JOB-NORM-001",
            ProviderInternalPayloadRef = string.Concat("mock://provider-event/", attemptId),
            RawCallStatus = rawStatus,
            RawDtmf = rawDtmf,
            AudioStatus = "PLAYED",
            TechnicalErrorCode = technicalErrorCode,
            RecordingRef = null,
            ReceivedAt = receivedAt,
        });
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
