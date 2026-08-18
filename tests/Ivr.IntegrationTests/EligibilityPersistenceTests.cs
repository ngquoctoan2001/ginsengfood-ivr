using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
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

    // W-0030 / P4-2. The old fixture carried only {"decision":"ELIGIBLE"}; the typed evidence
    // rules now also require a source version and a fresh capture stamp, so the fixture supplies
    // them. The rule was not relaxed to keep the old fixture passing — the fixture was corrected.
    private static readonly string EligibleSnapshotJson = JsonSerializer.Serialize(new
    {
        decision = "ELIGIBLE",
        source_version = "sales-eligibility-v1",
        captured_at = Now.AddSeconds(-30),
        source_available = true,
        blockers = Array.Empty<string>(),
    });

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

    [Theory]
    [Trait("TestId", "IT-ELIG-EVIDENCE-10")]
    [InlineData("pass", null, true, null)]
    [InlineData("block", "blocked", false, EligibilityReasonCodes.EligibilitySnapshotBlocked)]
    [InlineData("stale", "stale", false, EligibilityReasonCodes.EligibilitySnapshotStale)]
    [InlineData("source-down", "unavailable", false,
        EligibilityReasonCodes.EligibilitySourceUnavailable)]
    [InlineData("unreadable", "unreadable", false,
        EligibilityReasonCodes.EligibilitySnapshotUnreadable)]
    public async Task SalesEligibilityEvidenceIsValidatedFailClosedOnRealStorage(
        string caseId,
        string? scenario,
        bool expectedEligible,
        string? expectedReasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string taskId = string.Concat("TASK-ELIG-EV-", caseId.ToUpperInvariant());
        string snapshotJson = SnapshotFor(scenario);
        await SeedPendingTaskAsync(
            factory,
            taskId,
            string.Concat("JOB-ELIG-EV-", caseId.ToUpperInvariant()),
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: snapshotJson);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            taskId,
            string.Concat("corr-elig-ev-", caseId));

        Assert.Equal(expectedEligible, result.Eligible);
        if (expectedReasonCode is not null)
        {
            Assert.Equal(expectedReasonCode, Assert.Single(result.Reasons).Code);
        }

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        // Nothing that fails evidence validation may leave a dispatched attempt behind.
        Assert.Equal(0, await verification.CallAttempts.CountAsync());

        // The snapshot digest is persisted for every case, including the rejected ones: the
        // evidence trail must show which bytes were judged, not only the ones that passed.
        ConfirmationTaskEntity task = await verification.ConfirmationTasks.AsNoTracking()
            .SingleAsync(entity => entity.TaskId == taskId);
        Assert.Equal(
            DeterministicSnapshotHasher.Compute(snapshotJson),
            task.EligibilitySnapshotHash);

        if (expectedEligible)
        {
            Assert.Contains(
                result.EvidenceRefs,
                reference => reference.EndsWith(
                    string.Concat("#eligibility/snapshot/", task.EligibilitySnapshotHash),
                    StringComparison.Ordinal));
        }
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-EVIDENCE-11")]
    public async Task EligibilitySnapshotHashColumnRejectsAnythingThatIsNotADigest()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-EV-HASHGUARD",
            "JOB-ELIG-EV-HASHGUARD",
            "CREATED",
            "READY_FOR_ELIGIBILITY");

        await using IvrDbContext context = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(entity => entity.TaskId == "TASK-ELIG-EV-HASHGUARD");

        // Uppercase hex, a truncated digest, and a raw snapshot body must all be refused by the
        // database itself — the column is a digest field, not a second place to park evidence.
        foreach (string invalid in new[]
        {
            "ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789",
            "abc123",
            "{\"decision\":\"ELIGIBLE\"}",
        })
        {
            task.EligibilitySnapshotHash = invalid;
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();
            task = await context.ConfirmationTasks
                .SingleAsync(entity => entity.TaskId == "TASK-ELIG-EV-HASHGUARD");
        }
    }

    [Theory]
    [Trait("TestId", "IT-ELIG-VOICE-13")]
    [InlineData("allowed", true, null)]
    [InlineData("restricted", false, EligibilityReasonCodes.PhoneCallRestricted)]
    [InlineData("resolver-down", false,
        EligibilityReasonCodes.PhoneCallRestrictionSourceUnavailable)]
    [InlineData("marketing-noise", true, null)]
    public async Task VoiceRestrictionEvidenceIsHonouredAndMarketingConsentIsNot(
        string caseId,
        bool expectedEligible,
        string? expectedReasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string taskId = string.Concat("TASK-ELIG-VOICE-", caseId.ToUpperInvariant());
        await SeedPendingTaskAsync(
            factory,
            taskId,
            string.Concat("JOB-ELIG-VOICE-", caseId.ToUpperInvariant()),
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: VoiceSnapshotFor(caseId));
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            taskId,
            string.Concat("corr-elig-voice-", caseId));

        Assert.Equal(expectedEligible, result.Eligible);
        if (expectedReasonCode is not null)
        {
            Assert.Equal(expectedReasonCode, Assert.Single(result.Reasons).Code);
        }

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-TRUST-14")]
    public async Task TrustResolverEvidenceNeverProducesASkipWhileTheFeatureStaysOwnerGated()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        // A fully evidenced, versioned resolver decision for a TRUSTED customer with no risk —
        // the single case that could ever justify skipping the call.
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-TRUST-14",
            "JOB-ELIG-TRUST-14",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = Now.AddSeconds(-30),
                source_available = true,
                trust = new
                {
                    resolver_available = true,
                    resolver_version = "sales-trust-v1",
                    risk_evidence_available = true,
                },
            }),
            customerTrustStatus: "TRUSTED",
            trustedSkipAllowed: true);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-TRUST-14",
            "corr-elig-trust-14");

        // Even with every piece of evidence present, the call still happens: enabling the skip is
        // an owner decision that needs a versioned Sales resolver contract, and that contract does
        // not exist. `AS-07`-style default — the evidence plumbing is ready, the gate is not open.
        Assert.True(result.Eligible);
        Assert.NotEqual(EligibilityDecisions.SkippedTrustedCustomer, result.Decision);
        Assert.Contains(EligibilityReasonCodes.TrustSkipDisabledRequireIvr, result.Advisories);
    }

    private static string VoiceSnapshotFor(string caseId)
    {
        object voice = caseId switch
        {
            "restricted" => new { restricted = true, source_available = true, source_version = "sales-voice-v1" },
            "resolver-down" => new { restricted = false, source_available = false, source_version = "sales-voice-v1" },
            _ => new { restricted = false, source_available = true, source_version = "sales-voice-v1" },
        };

        if (caseId != "marketing-noise")
        {
            return JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = Now.AddSeconds(-30),
                source_available = true,
                voice_restriction = voice,
            });
        }

        // Same allowed voice decision, but the bag also carries every marketing-consent signal
        // set to its most restrictive value. None of them may reach the voice decision.
        return JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = true,
            voice_restriction = voice,
            sms_opt_out = true,
            marketing_consent = false,
            email_opt_out = true,
            newsletter_subscribed = false,
            promo_calls_allowed = false,
        });
    }

    private static string SnapshotFor(string? scenario) => scenario switch
    {
        null => EligibleSnapshotJson,
        "blocked" => JsonSerializer.Serialize(new
        {
            decision = "BLOCKED",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = true,
        }),
        "stale" => JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddHours(-3),
            source_available = true,
        }),
        "unavailable" => JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = false,
        }),
        "unreadable" => "[\"not-an-object\"]",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
    };

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
        bool callRestriction = false,
        string? eligibilitySnapshotJson = null,
        bool omitSnapshotHash = false,
        string? customerTrustStatus = null,
        bool trustedSkipAllowed = false)
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
            CustomerTrustStatus = customerTrustStatus,
            TrustedSkipAllowed = trustedSkipAllowed,
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
            EligibilitySnapshotJson = eligibilitySnapshotJson ?? EligibleSnapshotJson,
            EligibilitySnapshotHash = omitSnapshotHash
                ? null
                : DeterministicSnapshotHasher.Compute(
                    eligibilitySnapshotJson ?? EligibleSnapshotJson),
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
