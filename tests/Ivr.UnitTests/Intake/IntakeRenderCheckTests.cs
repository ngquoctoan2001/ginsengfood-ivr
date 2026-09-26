using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Policies;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Intake;

#pragma warning disable CA2000 // TestContext owns and disposes its in-memory dependencies.

/// <summary>
/// Q-16 (PA2, owner 2026-09-26). Intake used to accept an order whose summary the dial path could
/// not speak - an amount past what the speller says, a quantity whose digits read as a telephone
/// number, a script past its length bound - and the order then failed at every dial and expired
/// with nobody called, after Module 3 had been told it was accepted (B16). Intake now renders the
/// summary with the dial path's own renderer before accepting, so the two give the same answer.
/// </summary>
public sealed class IntakeRenderCheckTests
{
    // 13:00 in Vietnam, inside the calling hours.
    private static readonly DateTimeOffset Now = new(2026, 8, 13, 6, 0, 0, TimeSpan.Zero);

    // The last column is the reason a refused order is given. The amount is past what the speller
    // says, so only the render catches it. The quantity's digits read as a telephone number, and
    // intake's own summary guard already refused that before this check existed; it is here to
    // show the two still agree.
    public static TheoryData<string, double, decimal, bool, string?> Orders => new()
    {
        { "Nước hồng sâm", 2d, 560_000m, true, null },
        { "Tổ yến", 2d, 560_000m, true, null },
        { "Nước hồng sâm", 2d, 1_000_000_000_000m, false, TaskIntakeService.SpeechSummaryNotRenderable },
        { "Nước hồng sâm", 0.0912345678d, 560_000m, false, "PRIVACY_SAFE_SPEECH_REJECTED" },
    };

    [Theory]
    [Trait("TestId", "UT-INTAKE-RENDER-01")]
    [MemberData(nameof(Orders))]
    public async Task IntakeRefusesExactlyWhatTheDialPathCannotSpeak(
        string productName,
        double quantity,
        decimal totalAmount,
        bool speakable,
        string? reason)
    {
        using TestContext test = CreateContext();
        IvrConfirmationTaskV1 source = CreateTask(productName, quantity, totalAmount);

        // What the dial path does with the same order, asked first so the expectation is its answer.
        Assert.Equal(speakable, await DialPathCanSpeakAsync(test.Scripts, productName, quantity, totalAmount));

        TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(source));

        if (speakable)
        {
            Assert.False(outcome.IsFailure);
            Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
            Assert.Equal(1, test.Store.CallJobCount);
            return;
        }

        Assert.True(outcome.IsFailure);
        Assert.Equal(TaskIntakeDecisions.HeldAdminReview, outcome.Decision);
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, outcome.FailureCode);
        Assert.Equal(reason, Assert.Single(outcome.BlockedReasons));
        Assert.Null(outcome.IvrCallJobId);
        Assert.Equal(0, test.Store.TaskCount);
        Assert.Equal(0, test.Store.CallJobCount);
    }

    /// <summary>
    /// Both refusals the dial path's renderer makes - a value the speller cannot say, and a script
    /// the policy refuses (length bound, spoken-text guard, a placeholder it cannot fill) - become
    /// the same refusal here. Anything else the renderer throws is not a verdict on the order, so
    /// it is not reported as one.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-INTAKE-RENDER-02")]
    public async Task BothOfTheRenderersRefusalsAreRefusalsAndNothingElseIs()
    {
        foreach (Exception refusal in new Exception[]
        {
            new SpeechRenderRejectedException(new ArgumentOutOfRangeException("amount")),
            new SpeechRenderPolicyRejectedException(new InvalidOperationException("length")),
        })
        {
            using TestContext test = CreateContext(new ThrowingRenderer(refusal));
            TaskIntakeOutcome outcome = await test.Service.IntakeAsync(Command(CreateTask("Nước hồng sâm", 2d, 560_000m)));
            Assert.True(outcome.IsFailure);
            Assert.Equal(TaskIntakeDecisions.HeldAdminReview, outcome.Decision);
            Assert.Equal(IvrErrorCodes.PiiPolicyViolation, outcome.FailureCode);
            Assert.Equal(TaskIntakeService.SpeechSummaryNotRenderable, Assert.Single(outcome.BlockedReasons));
            Assert.Equal(0, test.Store.TaskCount);
        }

        using TestContext broken = CreateContext(new ThrowingRenderer(new TimeoutException("registry")));
        await Assert.ThrowsAsync<TimeoutException>(
            () => broken.Service.IntakeAsync(Command(CreateTask("Nước hồng sâm", 2d, 560_000m))));
    }

    private sealed class ThrowingRenderer(Exception exception) : ISpeechRenderer
    {
        public ValueTask<RenderedSpeech> RenderAsync(
            Ivr.Domain.Confirmation.PrivacySafeOrderSummary summary,
            string scriptTemplateId,
            string scriptVersion,
            ExecutionMode executionMode,
            CancellationToken cancellationToken) => throw exception;
    }

    private static async Task<bool> DialPathCanSpeakAsync(
        InMemoryScriptRegistry scripts,
        string productName,
        double quantity,
        decimal totalAmount)
    {
        Ivr.Domain.Confirmation.PrivacySafeOrderSummary summary = Ivr.Domain.Confirmation.PrivacySafeOrderSummary.Create(
            "chị An",
            "P16001",
            [SpeechItem.Create(productName, (decimal)quantity, "hộp")],
            Money.Vnd(totalAmount),
            ShortDeliveryArea.Create("Phường Bến Nghé, Quận Một"),
            "Giờ Vàng",
            null,
            SpeechSummaryLimits.Create(100, 100));
        try
        {
            _ = await CreateRenderer(scripts).RenderAsync(
                summary,
                TargetV1SpeechPolicy.MockTemplateId,
                TargetV1SpeechPolicy.MockTemplateVersion,
                ExecutionMode.Mock,
                CancellationToken.None);
            return true;
        }
        catch (Exception exception) when (
            exception is SpeechRenderRejectedException or SpeechRenderPolicyRejectedException)
        {
            return false;
        }
    }

    private static ApprovedVietnameseSpeechRenderer CreateRenderer(InMemoryScriptRegistry scripts)
    {
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.MockExecutionMode,
            Provider = TtsProviderOptions.FakeProvider,
        });
        return new ApprovedVietnameseSpeechRenderer(
            scripts,
            new VietnameseOrderScriptRenderer(),
            new RegionalVoiceMap(tts));
    }

    private static TestContext CreateContext(ISpeechRenderer? renderer = null)
    {
        var clock = new FixedTimeProvider(Now);
        var audit = new InMemoryAuditLogger(clock);
        var scripts = new InMemoryScriptRegistry(audit, clock, Options.Create(new ScriptContentOptions()));
        var store = new InMemoryTaskIntakeStore(audit, clock);
        var service = new TaskIntakeService(
            store,
            new FakeAttemptPolicyRegistry(CandidateAttemptPolicies.Create()),
            scripts,
            new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }),
            renderer ?? CreateRenderer(scripts));
        return new TestContext(service, store, scripts);
    }

    private static TaskIntakeCommand Command(IvrConfirmationTaskV1 source) =>
        new(source, "idem-q16", source.Correlation_id!, new string('A', 64), ExecutionMode.Mock);

    private static IvrConfirmationTaskV1 CreateTask(string productName, double quantity, decimal totalAmount)
    {
        DateTimeOffset start = Now.AddMinutes(-1);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = "TASK-Q16",
            Correlation_id = "corr-q16",
            Created_at = start,
            Order_id = "ORDER-Q16",
            Order_code = "GF-Q16-001",
            Order_code_short = "P16001",
            Order_version = "1",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = ProgramCode.GOLDEN_HOUR,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddSeconds(300),
            Attempt_policy_version = CandidateAttemptPolicies.Version,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Phone_ref = "phone-ref-q16",
            Phone_masked = "84xxxxx0016",
            Phone_validation_status = IvrConfirmationTaskV1Phone_validation_status.VALID,
            Dial_token = "dial-token-q16",
            Dial_token_expires_at = start.AddSeconds(300),
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "chị An",
                Order_code_short = "P16001",
                Items =
                [
                    new OrderSpeechItem { Public_name = productName, Quantity = quantity, Unit_label = "hộp" },
                ],
                Total_amount = (double)totalAmount,
                Currency = PrivacySafeOrderSummaryCurrency.VND,
                Delivery_area_short = "Phường Bến Nghé, Quận Một",
                Program_display_name = "Giờ Vàng",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Allowed_script_variables = new { },
            Call_restriction = false,
            Eligibility_snapshot = new { decision = "ELIGIBLE", source = "unit-test" },
            Evidence_ref = "evidence://unit/q16",
        };
    }

    private sealed record TestContext(
        TaskIntakeService Service,
        InMemoryTaskIntakeStore Store,
        InMemoryScriptRegistry Scripts) : IDisposable
    {
        public void Dispose()
        {
            Store.Dispose();
            Scripts.Dispose();
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
