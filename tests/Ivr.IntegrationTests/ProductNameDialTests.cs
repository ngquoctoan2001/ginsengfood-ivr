using System.Net;
using System.Net.Http.Json;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

/// <summary>
/// Q-12 (PA1, owner 2026-09-26). Intake admits a product name with the product guard (W-0243),
/// and until Q-12 the dial path read the finished script with the full guard: an order of tổ yến,
/// đường phèn or ấp trứng was accepted, Module 3 was told so, and every dial was refused at render,
/// twice, before the window expired with nobody called. This takes one such order the whole way on
/// real PostgreSQL - intake, eligibility through the internal API, a MOCK dial with the approved
/// script, the real renderer and the real synthesis service, then normalisation - and asks that the
/// customer hears all three names and that the order is confirmed.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ProductNameDialTests(PostgresPersistenceFixture fixture)
{
    private const string TaskIdValue = "TASK-PRODUCT-NAME";
    private const string ChannelId = "SIM-MOCK-PRODUCT";

    [Fact]
    [Trait("TestId", "IT-TEL-PRODUCT-NAME-12")]
    public async Task AnOrderOfBirdsNestIsAcceptedAtIntakeAndConfirmedOnTheCall()
    {
        // The admin API evaluates eligibility on the real clock, so the run is anchored to it, and
        // the calling window is opened to the whole day so the test does not depend on the hour.
        DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAndChannelAsync(factory, now);
        var clock = new FixedTimeProvider(now);
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString);

        // 1. Intake admits the three names, as it has since W-0243.
        TaskIntakeOutcome outcome = await CreateIntake(factory, clock).IntakeAsync(
            new TaskIntakeCommand(
                CreateTask(now),
                "idem-product-name",
                "corr-product-name",
                new string('B', 64),
                ExecutionMode.Mock));
        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);

        // 2. Eligibility, the way the worker asks for it.
        using (HttpRequestMessage eligibility = InternalRequest(
            HttpMethod.Post, "/v1/ivr/order-confirmation/eligibility-checks"))
        {
            eligibility.Headers.Add("Idempotency-Key", "idem-product-name-eligibility");
            eligibility.Content = JsonContent.Create(new Ivr.Api.Internal.EligibilityLifecycleRequest(TaskIdValue));
            using HttpResponseMessage answered = await app.Client.SendAsync(eligibility);
            string body = await answered.Content.ReadAsStringAsync();
            Assert.True(answered.StatusCode == HttpStatusCode.OK, $"eligibility answered {(int)answered.StatusCode}: {body}");
        }

        // 3. The dial, with the approved script, the real renderer and the real synthesis service:
        //    both last checks on the text run, and before Q-12 the first of them refused it.
        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(
            await new PostgresSchedulerStore(factory, clock).TryClaimDueDispatchAsync(
                "worker-product-name",
                IvrOptions.MockExecutionMode,
                TimeSpan.FromMinutes(2)));
        var sim = new FakeSimGateway(
            new Dictionary<string, FakeSimScenario>
            {
                [lease.AttemptId] = new(SimProviderDisposition.Answered, "1"),
            },
            new Dictionary<string, SimChannelHealthState>(),
            clock);
        await CreateGateway(factory, clock, lease, sim).DispatchAsync(lease);

        // What the customer heard names all three.
        RenderedSpeech played = Assert.Single(sim.PlayedSpeech.Values);
        Assert.Contains("Tổ yến", played.ExactText, StringComparison.Ordinal);
        Assert.Contains("Đường phèn", played.ExactText, StringComparison.Ordinal);
        Assert.Contains("Ấp trứng", played.ExactText, StringComparison.Ordinal);

        // 4. Normalisation: confirmed, counted, and on the callback outbox for Module 3.
        await new ResultRepository(
            factory,
            new RawEventRepository(),
            Options.Create(new SchedulerOptions { TechnicalRetryLimit = 1 }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            new FixedTimeProvider(now.AddSeconds(30)))
            .NormalizeNextAsync("normalizer-product-name");
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        CallResultEntity result = await context.CallResults.AsNoTracking().SingleAsync();
        Assert.Equal("IVR_CONFIRMED", result.ResultType);
        Assert.True(result.IsCountedCustomerAttempt);
        Assert.NotNull(await context.ResultCallbacks.AsNoTracking().SingleOrDefaultAsync());
    }

    private static HttpRequestMessage InternalRequest(HttpMethod method, string route)
    {
        HttpRequestMessage request = new(method, route);
        request.Headers.Authorization = new("Bearer", InternalAdminApiTestApplication.InternalToken);
        request.Headers.Add("X-Source-System", "ivr-worker");
        request.Headers.Add("X-Service-Scope", Ivr.Api.Internal.InternalServiceOptions.RequiredScope);
        request.Headers.Add("X-Correlation-Id", "corr-product-name");
        return request;
    }

    private static TaskIntakeService CreateIntake(
        IDbContextFactory<IvrDbContext> factory,
        TimeProvider clock) =>
        new(
            new PostgresTaskIntakeStore(factory, clock),
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(factory, clock, Options.Create(new ScriptContentOptions())),
            new UnavailableOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions
            {
                Enabled = true,
                StartMinuteOfLocalDay = 0,
                EndMinuteOfLocalDay = 1440,
            })),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }));

    private static MockSchedulerDispatchGateway CreateGateway(
        IDbContextFactory<IvrDbContext> factory,
        TimeProvider clock,
        SchedulerDispatchLease lease,
        FakeSimGateway sim)
    {
        Dictionary<string, string> tokens = new()
        {
            ["*"] = "mock-destination-allowlisted",
        };
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
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        return new MockSchedulerDispatchGateway(
            new PostgresTelephonyDispatchStore(
                factory,
                SpeechSummaryLimits.Create(100, 100),
                Options.Create(new SchedulerOptions()),
                clock),
            new FakeDialTokenResolver(tokens, options.DestinationAllowlist),
            new ApprovedVietnameseSpeechRenderer(
                new PostgresScriptRegistry(factory, clock, Options.Create(new ScriptContentOptions())),
                new VietnameseOrderScriptRenderer(),
                new RegionalVoiceMap(tts)),
            new SpeechSynthesisService(
                new FakeDeterministicTtsProvider(tts),
                new AudioCache(clock),
                new TtsRequestBudget(clock),
                new TtsUsageMeter(),
                new RegionalVoiceMap(tts),
                tts,
                clock),
            sim,
            Options.Create(options),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.MockExecutionMode),
            clock);
    }

    private static IvrConfirmationTaskV1 CreateTask(DateTimeOffset now)
    {
        DateTimeOffset start = now.AddMinutes(-1);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = TaskIdValue,
            Correlation_id = "corr-product-name",
            Created_at = start,
            Order_id = "ORDER-PRODUCT-NAME",
            Order_code = "GF-PRODUCT-001",
            Order_code_short = "PN001",
            Order_version = "3",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = ProgramCode.GOLDEN_HOUR,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddMinutes(5),
            Attempt_policy_version = CandidateAttemptPolicies.Version,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Phone_ref = "phone-ref-product-name",
            Phone_masked = "84xxxxx0002",
            Phone_validation_status = IvrConfirmationTaskV1Phone_validation_status.VALID,
            // Number-only, the shape intake accepts without a protector; MOCK never dials it.
            Phone_e164 = "+84900000002",
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "chị Lan",
                Order_code_short = "PN001",
                Items =
                [
                    new OrderSpeechItem { Public_name = "Tổ yến", Quantity = 2, Unit_label = "hộp" },
                    new OrderSpeechItem { Public_name = "Đường phèn", Quantity = 1, Unit_label = "gói" },
                    new OrderSpeechItem { Public_name = "Ấp trứng", Quantity = 1, Unit_label = "hộp" },
                ],
                Total_amount = 1_250_000,
                Currency = PrivacySafeOrderSummaryCurrency.VND,
                Delivery_area_short = "Phường Bến Nghé, Quận Một",
                Program_display_name = "Giờ Vàng",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Call_restriction = false,
            Eligibility_snapshot = new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = start,
                source_available = true,
                blockers = Array.Empty<string>(),
            },
            Evidence_ref = "evidence://product-name-dial",
        };
    }

    private static async Task SeedPoliciesAndChannelAsync(
        IDbContextFactory<IvrDbContext> factory,
        DateTimeOffset now)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.AttemptPolicies.AddRange(
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "GOLDEN_HOUR",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,150]",
                ConfirmationWindowSeconds = 300,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = now,
            },
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "TWENTY_FOUR_SEVEN",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,450]",
                ConfirmationWindowSeconds = 900,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = now,
            });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = ChannelId,
            SimNumberRef = string.Concat("sim-ref-", ChannelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            LastHealthCheckAt = now,
        });
        await context.SaveChangesAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
