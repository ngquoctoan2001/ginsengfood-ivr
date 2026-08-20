using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Contracts.Generated.SalesTarget.V1;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Providers.Fakes;
using SalesRecommendedCoreAction = Ivr.Contracts.Generated.SalesTarget.V1.RecommendedCoreAction;
using SalesResultType = Ivr.Contracts.Generated.SalesTarget.V1.ResultType;

namespace Ivr.UnitTests.Confirmation;

public sealed class CallResultAndMapperTests
{
    [Fact]
    public void NoAnswerIsAdvisoryAndCannotRequestCoreTransition()
    {
        CallResultSnapshot result = TestData.Result(
            IvrResultType.IvrNoAnswerFinal,
            counted: true,
            CoreActionRecommendation.NoStateChangeWaitForTimeout);
        Assert.Equal(CoreActionRecommendation.NoStateChangeWaitForTimeout, result.RecommendedCoreAction);

        Assert.Throws<InvalidOperationException>(() => TestData.Result(
            IvrResultType.IvrNoAnswerFinal,
            counted: true,
            CoreActionRecommendation.RevalidateAndCancelCustomerRequest));
    }

    [Theory]
    [InlineData(IvrResultType.IvrTechnicalException)]
    [InlineData(IvrResultType.IvrCapacityException)]
    public void TechnicalAndCapacityResultsNeverCountAsCustomerAttempts(IvrResultType resultType)
    {
        Assert.Throws<InvalidOperationException>(() => TestData.Result(
            resultType,
            counted: true,
            CoreActionRecommendation.RevalidateAndHoldAdminReview));

        CallResultSnapshot valid = TestData.Result(
            resultType,
            counted: false,
            CoreActionRecommendation.RevalidateAndHoldAdminReview);
        Assert.False(valid.IsCountedCustomerAttempt);
    }

    [Theory]
    [InlineData(IvrResultType.IvrOperationalBlocked)]
    [InlineData(IvrResultType.IvrPolicyBlocked)]
    public void PreCallBlockedTaxonomyCannotBeSentAsAnIvrCallback(IvrResultType resultType)
    {
        CallResultSnapshot reserved = TestData.Result(
            resultType,
            counted: false,
            CoreActionRecommendation.RevalidateAndHoldAdminReview);

        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => TargetV1CallbackMapper.ToSalesWire(reserved));

        Assert.Equal(
            "Pre-call operational and policy blocks are not IVR callback results.",
            failure.Message);
    }

    [Fact]
    public void TargetCallbackMapperPreservesSemanticsAndStaleGuard()
    {
        CallResultSnapshot domain = TestData.Result();

        IvrResultCallbackV1 wire = TargetV1CallbackMapper.ToSalesWire(domain);

        Assert.Equal(domain.OrderVersion.Value, wire.Order_version_seen_by_ivr);
        Assert.Equal(SalesResultType.IVR_CONFIRMED, wire.Result_type);
        Assert.Equal(
            SalesRecommendedCoreAction.CORE_REVALIDATE_AND_CONFIRM_ORDER,
            wire.Recommended_core_action);
        Assert.Equal(domain.ComputeHash(), TestData.Result().ComputeHash());
    }

    [Fact]
    public void TargetAckSemanticsAreIndependentOfHttpStatus()
    {
        CallbackAcknowledgement accepted = TargetV1CallbackMapper.FromSalesAck(new CallbackAck200
        {
            Code = CallbackAck200Code.DUPLICATE_ACCEPTED,
            Callback_id = "callback-1",
            Correlation_id = "correlation-1",
        });
        CallbackAcknowledgement stale = TargetV1CallbackMapper.FromSalesAck(new CallbackAck409
        {
            Code = CallbackAck409Code.REJECTED_STALE,
            Callback_id = "callback-1",
            Correlation_id = "correlation-1",
        });

        Assert.Equal(CallbackDisposition.Completed, accepted.Disposition);
        Assert.Equal(CallbackDisposition.ManualReviewNoRetry, stale.Disposition);
    }

    [Fact]
    public void CurrentMapperIsExplicitAndRejectsTwentyFourSeven()
    {
        CurrentGoldenHourIdentity identity = new(1, 2, 3, 4);
        var current = CurrentGoldenHourCompatMapper.ToCompatibilityWire(
            identity,
            TestData.Result(),
            "idempotency-1");
        Assert.Equal(Ivr.Contracts.Sales.CurrentGoldenHour.CurrentGoldenHourCallbackResult.Confirmed, current.Result);

        Assert.Throws<InvalidOperationException>(() =>
            CurrentGoldenHourCompatMapper.ToCompatibilityWire(
                identity,
                TestData.Result(program: IvrProgramCode.TwentyFourSeven),
                "idempotency-2"));
    }

    [Fact]
    public void CurrentMapperRejectsTargetOnlyResultInsteadOfLossyMapping()
    {
        CurrentGoldenHourIdentity identity = new(1, 2, 3, 4);

        Assert.Throws<InvalidOperationException>(() =>
            CurrentGoldenHourCompatMapper.ToCompatibilityWire(
                identity,
                TestData.Result(
                    IvrResultType.IvrPolicyBlocked,
                    counted: false,
                    CoreActionRecommendation.RevalidateAndHoldAdminReview),
                "idempotency-3"));
    }

    [Fact]
    public async Task TargetTaskMapperUsesRegistryAndDoesNotMapPhoneFields()
    {
        AttemptPolicySnapshot policy = TestData.Policy(AttemptPolicyApproval.CandidateMockLabOnly);
        TargetV1TaskMapper mapper = new(
            new FakeAttemptPolicyRegistry([policy]),
            SpeechSummaryLimits.Create(20, 20));

        ConfirmationTaskSnapshot domain = await mapper.ToDomainAsync(
            CreateWireTask(),
            ExecutionMode.Mock,
            CancellationToken.None);

        Assert.Equal("task-1", domain.TaskId.Value);
        Assert.Same(policy, domain.AttemptPolicy);
        Assert.Equal("[REDACTED_DIAL_TOKEN]", domain.DialToken.ToString());
        string serializedDomain = JsonSerializer.Serialize(domain);
        Assert.DoesNotContain("phone-ref-1", serializedDomain, StringComparison.Ordinal);
        Assert.DoesNotContain("***1234", serializedDomain, StringComparison.Ordinal);
        Assert.DoesNotContain(
            typeof(ConfirmationTaskSnapshot).GetProperties(),
            property => property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TargetTaskMapperRejectsCandidatePolicyInProduction()
    {
        TargetV1TaskMapper mapper = new(
            new FakeAttemptPolicyRegistry([TestData.Policy(AttemptPolicyApproval.CandidateMockLabOnly)]),
            SpeechSummaryLimits.Create(20, 20));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mapper.ToDomainAsync(CreateWireTask(), ExecutionMode.ProductionReal, CancellationToken.None));
    }

    private static IvrConfirmationTaskV1 CreateWireTask()
    {
        DateTimeOffset start = new(2026, 8, 13, 1, 0, 0, TimeSpan.Zero);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = "task-1",
            Correlation_id = "correlation-1",
            Created_at = start,
            Order_id = "order-1",
            Order_code = "DH-2026-001",
            Order_code_short = "DH-001",
            Order_version = "order-version-1",
            Order_state = "WAITING_IVR_CONFIRMATION",
            Payment_method_snapshot = IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = ProgramCode.GOLDEN_HOUR,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddSeconds(300),
            Attempt_policy_version = "mock-lab-v1",
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Customer_ref = "customer-ref-1",
            Customer_trust_status = "STANDARD",
            Trusted_skip_allowed = false,
            Risk_flags = [],
            Phone_ref = "phone-ref-1",
            Phone_masked = "***1234",
            Phone_validation_status = "VALID",
            Dial_token = "opaque-dial-token-1",
            Dial_token_expires_at = start.AddSeconds(300),
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "Anh Đạt",
                Order_code_short = "DH-001",
                Items = [new OrderSpeechItem { Public_name = "Sâm lát", Quantity = 2, Unit_label = "Hộp" }],
                Total_amount = 1_250_000,
                Currency = PrivacySafeOrderSummaryCurrency.VND,
                Delivery_area_short = "Phường Bến Nghé, Quận 1",
                Program_display_name = "Giờ Vàng",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
                Pronunciation_hints = new Dictionary<string, string> { ["Sâm"] = "xâm" },
            },
            Call_script_template_id = "ivr-confirm-v1",
            Call_script_version = "1",
            Allowed_script_variables = new { },
            Sellable_status = [],
            Call_restriction = false,
            Eligibility_snapshot = new { eligible = true },
            Evidence_ref = "evidence-1",
            Evidence_policy_version = "evidence-policy-1",
            Privacy_policy_version = "privacy-policy-1",
        };
    }
}
