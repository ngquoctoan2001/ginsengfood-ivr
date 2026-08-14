using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class EligibilityPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-ELIG-CAP-05")]
    public async Task CapacityShortageCreatesIncidentWithoutCountedAttemptOrDispatch()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-CAP-05",
            "JOB-ELIG-CAP-05",
            "CREATED",
            "READY_FOR_ELIGIBILITY");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityShortageProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-CAP-05",
            "corr-elig-cap-05");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.CapacityException, result.Decision);
        Assert.False(result.IsCountedCustomerAttempt);
        Assert.NotNull(result.CapacityIncidentId);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        CapacityIncidentEntity incident = await verification.CapacityIncidents
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal(EligibilityDecisions.CapacityException, task.EligibilityDecision);
        Assert.Equal("CAPACITY_HELD", job.Status);
        Assert.Equal("HELD_CAPACITY", job.QueueStatus);
        Assert.Equal(incident.CapacityIncidentId, job.CapacityIncidentId);
        Assert.False(incident.HoldNewCalls);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.NotEmpty(await verification.EvidenceLinks.AsNoTracking().ToListAsync());
        AuditLogEntity audit = await verification.AuditLog
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("ELIGIBILITY_EVALUATED", audit.Action);
        using JsonDocument auditData = JsonDocument.Parse(audit.DataJson);
        Assert.False(auditData.RootElement
            .GetProperty("is_counted_customer_attempt")
            .GetBoolean());
        TaskIntakeOutboxEntity outbox = await verification.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("PUBLISHED", outbox.Status);
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-MOCK-06")]
    public async Task MockEligibleDecisionRemainsHeldWithNoEgressOrAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-MOCK-06",
            "JOB-ELIG-MOCK-06",
            "DRY_RUN",
            "HELD_MOCK");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-MOCK-06",
            "corr-elig-mock-06");

        Assert.True(result.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, result.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        TaskIntakeOutboxEntity outbox = await verification.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync();
        Assert.True(job.Eligible);
        Assert.Equal("DRY_RUN", job.Status);
        Assert.Equal("HELD_MOCK", job.QueueStatus);
        Assert.Equal("HELD_MOCK", outbox.Status);
        Assert.Null(outbox.PublishedAt);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-DNC-07")]
    public async Task StoredPhoneRestrictionBlocksBeforeCapacityWithEvidenceAndNoAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-DNC-07",
            "JOB-ELIG-DNC-07",
            "DRY_RUN",
            "HELD_MOCK",
            callRestriction: true);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new UnexpectedCapacityProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-DNC-07",
            "corr-elig-dnc-07");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.BlockedOperational, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestricted,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("BLOCKED", job.Status);
        Assert.Equal("BLOCKED", job.QueueStatus);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.NotEmpty(await verification.EvidenceLinks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-FAILCLOSED-08")]
    public async Task CapacityResponseWithoutEvidenceFailsClosed()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-FAILCLOSED-08",
            "JOB-ELIG-FAILCLOSED-08",
            "DRY_RUN",
            "HELD_MOCK");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new MissingEvidenceCapacityProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-FAILCLOSED-08",
            "corr-elig-failclosed-08");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.CapacitySourceUnavailable,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        ReviewItemEntity review = await verification.ReviewItems.AsNoTracking().SingleAsync();
        Assert.Equal("ELIGIBILITY_DECISION", review.SourceType);
        Assert.Equal("TASK-ELIG-FAILCLOSED-08", review.SourceId);
        Assert.Equal("OPEN", review.Status);
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-SCHED-09")]
    public async Task SchedulerCapacitySourceUnavailableFailsClosedWithoutAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-SCHED-09",
            "JOB-ELIG-SCHED-09",
            "CREATED",
            "READY_FOR_ELIGIBILITY");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new SchedulerEligibilityCapacityProvider(
                new UnavailableSchedulerCapacityService()),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-SCHED-09",
            "corr-elig-sched-09");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.CapacitySourceUnavailable,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.Equal(
            "TASK-ELIG-SCHED-09",
            (await verification.ReviewItems.AsNoTracking().SingleAsync()).SourceId);
    }

    private static async Task SeedPendingTaskAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        string jobStatus,
        string outboxStatus,
        bool callRestriction = false)
    {
        DateTimeOffset startedAt = Now.AddMinutes(-1);
        DateTimeOffset expiresAt = Now.AddMinutes(4);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = "order-core:TASK-ELIG-CAP-05:idem-cap-05",
            CorrelationId = "corr-elig-cap-05",
            OfficialOrderId = "ORDER-ELIG-CAP-05",
            OrderCode = "GF-CAP-05",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            TrustedSkipAllowed = false,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "gh-v1-candidate",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = expiresAt,
            PhoneRef = "phone-ref-elig-cap-05",
            PhoneMasked = "84xxxxx0005",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:integration-cap-05",
            DialTokenExpiresAt = expiresAt,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "test-evidence-v1",
            PrivacyPolicyVersion = "test-privacy-v1",
            EligibilityDecision = null,
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            SellableStatusJson = JsonSerializer.Serialize(new[]
            {
                new SellableStatusLine
                {
                    Sku_id = "SKU-CAP-05",
                    Decision = SellableStatusLineDecision.SELLABLE,
                    Recall_hold = false,
                    Sale_lock = false,
                    Quality_hold = false,
                    Stock_available = true,
                    Batch_released = true,
                    Trace_ready = true,
                    Captured_at = Now,
                },
            }),
            SellableCapturedAt = Now,
            CallRestriction = callRestriction,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = expiresAt,
            AcceptedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://integration/p2-2/task\"]",
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = "ORDER-ELIG-CAP-05",
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "gh-v1-candidate",
            Status = jobStatus,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                startedAt,
                startedAt.AddSeconds(150),
            }),
            T0At = startedAt,
            ExpiresAt = expiresAt,
            Eligible = false,
            EligibilityDecision = EligibilityDecisions.Pending,
            QueueStatus = "HELD_ELIGIBILITY",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "test-privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://integration/p2-2/task\"]",
        });
        context.TaskIntakeOutbox.Add(new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = taskId,
            IvrCallJobId = jobId,
            EventType = "IVR_TASK_READY_FOR_ELIGIBILITY",
            Status = outboxStatus,
            CorrelationId = "corr-elig-cap-05",
            PayloadSha256 = new string('A', 64),
            CreatedAt = startedAt,
        });
        await context.SaveChangesAsync();
    }

    private sealed class CapacityShortageProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                false,
                "SESSION-CAP-05",
                1,
                20,
                0,
                1,
                "NO_CHANNEL_BEFORE_DEADLINE",
                "evidence://integration/p2-2/capacity-shortage"));
    }

    private sealed class CapacityAvailableProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                true,
                "SESSION-MOCK-06",
                1,
                0,
                0,
                0,
                null,
                "evidence://integration/p2-2/capacity-available"));
    }

    private sealed class UnexpectedCapacityProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "Capacity must not be evaluated after a voice restriction blocks the task.");
    }

    private sealed class MissingEvidenceCapacityProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                true,
                "SESSION-MISSING-EVIDENCE",
                1,
                0,
                0,
                0,
                null,
                string.Empty));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
