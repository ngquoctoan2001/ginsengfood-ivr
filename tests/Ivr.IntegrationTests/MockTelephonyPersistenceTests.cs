using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class MockTelephonyPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-TEL-DISPATCH-01")]
    public async Task SchedulerLeaseRunsThroughMockGatewayAndPersistsFencedProviderEvent()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-01", "JOB-TEL-01", "SIM-MOCK-01");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-mock-tel",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        PostgresTelephonyDispatchStore store = CreateStore(factory);
        MockSchedulerDispatchGateway gateway = CreateGateway(
            store,
            lease,
            sim,
            includeToken: true);

        await gateway.DispatchAsync(lease);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("PROVIDER_EVENT_PENDING_NORMALIZATION", attempt.Status);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("ANSWERED", attempt.Disposition);
        Assert.Equal("1", attempt.DtmfKey);
        Assert.Equal(rawEvent.RawEventId, attempt.RawCallEventId);
        Assert.Equal("ANSWERED", rawEvent.RawCallStatus);
        Assert.Null(rawEvent.RecordingRef);
        Assert.Equal("IDLE", channel.Status);
        Assert.Null(channel.LeaseToken);
        Assert.Null(channel.ActiveCallJobId);
        Assert.Equal(lease.FencingGeneration + 1, channel.LeaseFencingGeneration);
        Assert.Equal(Now.AddSeconds(5), channel.CooldownUntil);
        Assert.Equal("DISPOSITION_PENDING_NORMALIZATION", job.Status);
        Assert.Equal("HELD_NORMALIZATION", job.QueueStatus);
        Assert.Equal(6, sim.Events.Count);
        RenderedSpeech played = Assert.Single(sim.PlayedSpeech).Value;
        Assert.NotNull(played.Audio);
        Assert.Equal("audio/L16", played.Audio.Format);
        Assert.Equal(8_000, played.Audio.SampleRate);
        Assert.StartsWith("memory://tts/fake/", played.Audio.ContentRef, StringComparison.Ordinal);
        Assert.DoesNotContain(
            await verification.AuditLog.Select(row => row.DataJson).ToListAsync(),
            json => json.Contains("Anh Đạt", StringComparison.Ordinal)
                || json.Contains("enc:mock-token", StringComparison.Ordinal));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.LoadAsync(lease));
    }

    [Fact]
    [Trait("TestId", "IT-TEL-TOKENFAIL-02")]
    public async Task MissingMockTokenFailsClosedReleasesLeaseAndDoesNotCountAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-02", "JOB-TEL-02", "SIM-MOCK-02");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-token-fail",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            timeProvider: new FixedTimeProvider(Now));
        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => gateway.DispatchAsync(lease));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        Assert.Equal("PROVIDER_EVENT_PENDING_NORMALIZATION", attempt.Status);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Equal("MOCK_DEPENDENCY_NOT_FOUND", attempt.TechnicalExceptionType);
        Assert.Equal("MOCK_DEPENDENCY_NOT_FOUND", rawEvent.TechnicalErrorCode);
        Assert.Equal("IDLE", channel.Status);
        Assert.Null(channel.LeaseToken);
        Assert.Single(sim.Events);
        Assert.Equal(SimProviderEventType.HealthChecked, sim.Events.Single().Type);
    }

    [Fact]
    [Trait("TestId", "IT-TEL-HEALTH-03")]
    public async Task UnhealthyMockChannelIsQuarantinedBeforeDial()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await SeedMockDispatchAsync(factory, "TASK-TEL-03", "JOB-TEL-03", "SIM-MOCK-03");
        var scheduler = new PostgresSchedulerStore(factory, new FixedTimeProvider(Now));
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await scheduler.TryClaimDueDispatchAsync(
                "worker-health-fail",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            new Dictionary<string, SimChannelHealthState>
            {
                [lease.SimChannelId] = SimChannelHealthState.Unavailable,
            },
            new FixedTimeProvider(Now));
        MockSchedulerDispatchGateway gateway = CreateGateway(
            CreateStore(factory),
            lease,
            sim,
            includeToken: true);

        MockSimOperationException error = await Assert.ThrowsAsync<MockSimOperationException>(
            () => gateway.DispatchAsync(lease));

        Assert.Equal("MOCK_CHANNEL_HEALTH_NOT_READY", error.TechnicalErrorCode);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        SimChannelEntity channel = await verification.SimChannels.AsNoTracking().SingleAsync();
        RawCallEventEntity rawEvent = await verification.RawCallEvents.AsNoTracking().SingleAsync();
        CallAttemptEntity attempt = await verification.CallAttempts.AsNoTracking().SingleAsync();
        Assert.Equal("QUARANTINED", channel.Status);
        Assert.Equal(1, channel.FailCount);
        Assert.Equal(Now.AddSeconds(5), channel.QuarantineUntil);
        Assert.Null(channel.LeaseToken);
        Assert.Equal("SIMERROR", rawEvent.RawCallStatus);
        Assert.Equal("MOCK_CHANNEL_HEALTH_NOT_READY", attempt.TechnicalExceptionType);
        Assert.False(attempt.IsCountedCustomerAttempt);
        Assert.Single(sim.Events);
        Assert.Equal(SimProviderEventType.HealthChecked, sim.Events.Single().Type);
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static PostgresTelephonyDispatchStore CreateStore(
        IDbContextFactory<IvrDbContext> factory) => new(
        factory,
        SpeechSummaryLimits.Create(100, 100),
        new FixedTimeProvider(Now));

    private static MockSchedulerDispatchGateway CreateGateway(
        ITelephonyDispatchStore store,
        SchedulerDispatchLease lease,
        FakeSimGateway sim,
        bool includeToken)
    {
        Dictionary<string, string> tokens = includeToken
            ? new Dictionary<string, string>
            {
                ["enc:mock-token"] = "mock-destination-allowlisted",
            }
            : [];
        var options = new MockTelephonyOptions
        {
            Enabled = true,
            KillSwitchEngaged = false,
            DtmfTimeoutSeconds = 10,
            CooldownSeconds = 5,
            TokenDestinations = tokens,
            DestinationAllowlist = ["mock-destination-allowlisted"],
            Scenarios = new Dictionary<string, MockSimScenarioOptions>
            {
                [lease.AttemptId] = new() { Disposition = "Answered", DtmfKey = "1" },
            },
        };
        var timeProvider = new FixedTimeProvider(Now);
        IOptions<TtsProviderOptions> ttsOptions = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        var usage = new TtsUsageMeter();
        var synthesis = new SpeechSynthesisService(
            new FakeDeterministicTtsProvider(ttsOptions),
            new AudioCache(timeProvider),
            new TtsRequestBudget(timeProvider),
            usage,
            ttsOptions,
            timeProvider);
        return new MockSchedulerDispatchGateway(
            store,
            new FakeDialTokenResolver(tokens, options.DestinationAllowlist),
            new FakeSpeechRenderer(),
            synthesis,
            sim,
            Options.Create(options),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            timeProvider);
    }

    private static async Task SeedMockDispatchAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        string channelId)
    {
        DateTimeOffset deadline = Now.AddMinutes(5);
        string summaryJson = JsonSerializer.Serialize(new
        {
            customer_display_name = "Anh Đạt",
            order_code_short = "DH-TEL-001",
            items = new[]
            {
                new { public_name = "Sâm lát", quantity = 2, unit_label = "hộp" },
            },
            total_amount = 1_250_000,
            currency = "VND",
            delivery_area_short = "Quận 7",
            program_display_name = "Giờ Vàng",
            locale = "vi-VN",
        });
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = string.Concat("mock-telephony:", taskId),
            CorrelationId = string.Concat("corr-", taskId),
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderCode = "GF-TEL-001",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = CandidateAttemptPolicies.Version,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = Now,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = "phone-ref-mock-test",
            PhoneMasked = "84xxxxx0021",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:mock-token",
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = summaryJson,
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            SellableStatusJson = "[]",
            CallRestriction = false,
            CreatedAt = Now,
            ExpiresAt = deadline,
            AcceptedAt = Now,
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = CandidateAttemptPolicies.Version,
            Status = "DRY_RUN",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                Now,
                Now.AddSeconds(150),
            }),
            T0At = Now,
            ExpiresAt = deadline,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = string.Concat(
                "SCRIPT-ORDER-CONFIRM:",
                Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion),
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = Now,
        });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = channelId,
            SimNumberRef = string.Concat("sim-ref-", channelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            LastHealthCheckAt = Now,
        });
        await context.SaveChangesAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
