using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Server = Ivr.Contracts.Generated.IvrServer.V1;
using Sales = Ivr.Contracts.Generated.SalesTarget.V1;

namespace Ivr.Infrastructure.Contracts;

public sealed class TargetV1TaskMapper(
    IAttemptPolicyRegistry policyRegistry,
    SpeechSummaryLimits speechLimits)
{
    public async ValueTask<ConfirmationTaskSnapshot> ToDomainAsync(
        Server.IvrConfirmationTaskV1 source,
        ExecutionMode executionMode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Call_restriction || source.Is_ivr_callable == false)
        {
            throw new InvalidOperationException("Task is not callable.");
        }

        IvrProgramCode program = MapProgram(source.Program_code);
        PaymentMethod paymentMethod = source.Payment_method_snapshot switch
        {
            Server.IvrConfirmationTaskV1Payment_method_snapshot.ONLINE => PaymentMethod.Online,
            Server.IvrConfirmationTaskV1Payment_method_snapshot.COD => PaymentMethod.CashOnDelivery,
            _ => throw new InvalidOperationException("Unsupported payment method."),
        };
        _ = ProgramPaymentPolicy.EnsureAllowed(
            program,
            paymentMethod,
            source.Ivr_confirmation_required);
        PolicyVersion policyVersion = PolicyVersion.Create(source.Attempt_policy_version);
        AttemptPolicySnapshot policy = await policyRegistry.ResolveAsync(
            policyVersion,
            program,
            executionMode,
            cancellationToken);
        EnsureWirePolicyMatchesSnapshot(source, policy);

        PrivacySafeOrderSummary speech = MapSpeechSummary(
            source.Privacy_safe_order_summary,
            speechLimits);
        return CreateDomainSnapshot(source, executionMode, policy, speech);
    }

    internal static ConfirmationTaskSnapshot CreateDomainSnapshot(
        Server.IvrConfirmationTaskV1 source,
        ExecutionMode executionMode,
        AttemptPolicySnapshot policy,
        PrivacySafeOrderSummary speech)
    {
        IvrProgramCode program = MapProgram(source.Program_code);
        PaymentMethod paymentMethod = source.Payment_method_snapshot switch
        {
            Server.IvrConfirmationTaskV1Payment_method_snapshot.ONLINE => PaymentMethod.Online,
            Server.IvrConfirmationTaskV1Payment_method_snapshot.COD => PaymentMethod.CashOnDelivery,
            _ => throw new InvalidOperationException("Unsupported payment method."),
        };
        ProgramPayment programPayment = ProgramPaymentPolicy.EnsureAllowed(
            program,
            paymentMethod,
            source.Ivr_confirmation_required);
        ConfirmationWindow window = ConfirmationWindow.Create(
            source.Confirmation_window_started_at,
            source.Confirmation_window_expires_at);
        return ConfirmationTaskSnapshot.Create(
            TaskId.Create(source.Task_id),
            OrderId.Create(source.Order_id),
            OrderVersion.Create(source.Order_version),
            source.Order_state,
            programPayment,
            window,
            policy,
            DialTokenReference.Create(source.Dial_token, source.Dial_token_expires_at),
            speech,
            EvidenceReference.Create(source.Evidence_ref),
            string.IsNullOrWhiteSpace(source.Correlation_id)
                ? null
                : CorrelationId.Create(source.Correlation_id),
            executionMode);
    }

    private static void EnsureWirePolicyMatchesSnapshot(
        Server.IvrConfirmationTaskV1 source,
        AttemptPolicySnapshot policy)
    {
        int[] sourceOffsets = [.. source.Attempt_offsets_seconds];
        int[] policyOffsets = [.. policy.AttemptOffsets.Select(offset => checked((int)offset.TotalSeconds))];
        if (source.Max_customer_attempts != policy.MaxCustomerAttempts
            || !sourceOffsets.SequenceEqual(policyOffsets))
        {
            throw new InvalidOperationException("Task attempt policy differs from the versioned registry snapshot.");
        }
    }

    private static IvrProgramCode MapProgram(Server.ProgramCode program) => program switch
    {
        Server.ProgramCode.GOLDEN_HOUR => IvrProgramCode.GoldenHour,
        Server.ProgramCode.TWENTY_FOUR_SEVEN => IvrProgramCode.TwentyFourSeven,
        _ => throw new InvalidOperationException("Unsupported program code."),
    };

    internal static PrivacySafeOrderSummary MapSpeechSummary(
        Server.PrivacySafeOrderSummary source,
        SpeechSummaryLimits limits)
    {
        if (source.Currency != Server.PrivacySafeOrderSummaryCurrency.VND
            || source.Locale != Server.PrivacySafeOrderSummaryLocale.ViVN)
        {
            throw new InvalidOperationException("Only VND and vi-VN speech are supported.");
        }

        SpeechItem[] items = [.. source.Items.Select(item => SpeechItem.Create(
            item.Public_name,
            Convert.ToDecimal(item.Quantity),
            item.Unit_label))];
        return PrivacySafeOrderSummary.Create(
            source.Customer_display_name,
            source.Order_code_short,
            items,
            Money.Vnd(Convert.ToDecimal(source.Total_amount)),
            ShortDeliveryArea.Create(source.Delivery_area_short),
            source.Program_display_name,
            source.Pronunciation_hints is null
                ? null
                : new Dictionary<string, string>(source.Pronunciation_hints),
            limits);
    }
}

public static class TargetV1CallbackMapper
{
    public static Sales.IvrResultCallbackV1 ToSalesWire(CallResultSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new Sales.IvrResultCallbackV1
        {
            Contract_version = Sales.IvrResultCallbackV1Contract_version.IvrOrderConfirmation_v1,
            Callback_id = source.CallbackId.Value,
            Task_id = source.TaskId.Value,
            Order_id = source.OrderId.Value,
            Order_version_seen_by_ivr = source.OrderVersion.Value,
            Result_type = MapResult(source.ResultType),
            Result_reason = source.ResultReason,
            Is_counted_customer_attempt = source.IsCountedCustomerAttempt,
            Is_final_for_ivr = source.IsFinalForIvr,
            Attempt_number = source.AttemptNumber,
            Occurred_at = source.OccurredAt,
            Recommended_core_action = MapAction(source.RecommendedCoreAction),
            Evidence_ref = source.EvidenceReference.Value,
            Audit_ref = source.AuditReference.Value,
        };
    }

    public static CallbackAcknowledgement FromSalesAck(Sales.CallbackAck200 source)
    {
        ArgumentNullException.ThrowIfNull(source);
        CallbackAcknowledgementCode code = source.Code switch
        {
            Sales.CallbackAck200Code.ACCEPTED => CallbackAcknowledgementCode.Accepted,
            Sales.CallbackAck200Code.DUPLICATE_ACCEPTED => CallbackAcknowledgementCode.DuplicateAccepted,
            Sales.CallbackAck200Code.BLOCKED_BY_CORE => CallbackAcknowledgementCode.BlockedByCore,
            Sales.CallbackAck200Code.REVIEW_REQUIRED => CallbackAcknowledgementCode.ReviewRequired,
            _ => throw new InvalidOperationException("Unsupported successful callback ACK."),
        };
        return new CallbackAcknowledgement(
            code,
            CallbackId.Create(source.Callback_id),
            CorrelationId.Create(source.Correlation_id));
    }

    public static CallbackAcknowledgement FromSalesAck(Sales.CallbackAck409 source)
    {
        ArgumentNullException.ThrowIfNull(source);
        CallbackAcknowledgementCode code = source.Code switch
        {
            Sales.CallbackAck409Code.REJECTED_STALE => CallbackAcknowledgementCode.RejectedStale,
            Sales.CallbackAck409Code.IDEMPOTENCY_CONFLICT => CallbackAcknowledgementCode.IdempotencyConflict,
            _ => throw new InvalidOperationException("Unsupported conflict callback ACK."),
        };
        return new CallbackAcknowledgement(
            code,
            CallbackId.Create(source.Callback_id),
            CorrelationId.Create(source.Correlation_id));
    }

    private static Sales.ResultType MapResult(IvrResultType result) => result switch
    {
        IvrResultType.IvrConfirmed => Sales.ResultType.IVR_CONFIRMED,
        IvrResultType.IvrCustomerCancelled => Sales.ResultType.IVR_CUSTOMER_CANCELLED,
        IvrResultType.IvrNoAnswerAttempt => Sales.ResultType.IVR_NO_ANSWER_ATTEMPT,
        IvrResultType.IvrNoAnswerFinal => Sales.ResultType.IVR_NO_ANSWER_FINAL,
        IvrResultType.IvrConfirmationWindowExpired => Sales.ResultType.IVR_CONFIRMATION_WINDOW_EXPIRED,
        IvrResultType.IvrInvalidPhoneFinal => Sales.ResultType.IVR_INVALID_PHONE_FINAL,
        IvrResultType.IvrWrongInput => Sales.ResultType.IVR_WRONG_INPUT,
        IvrResultType.IvrTechnicalException => Sales.ResultType.IVR_TECHNICAL_EXCEPTION,
        IvrResultType.IvrCapacityException => Sales.ResultType.IVR_CAPACITY_EXCEPTION,
        IvrResultType.IvrOperationalBlocked or IvrResultType.IvrPolicyBlocked =>
            throw new InvalidOperationException(
                "Pre-call operational and policy blocks are not IVR callback results."),
        _ => throw new InvalidOperationException("Unsupported result type."),
    };

    private static Sales.RecommendedCoreAction MapAction(CoreActionRecommendation action) => action switch
    {
        CoreActionRecommendation.RevalidateAndConfirmOrder => Sales.RecommendedCoreAction.CORE_REVALIDATE_AND_CONFIRM_ORDER,
        CoreActionRecommendation.RevalidateAndCancelCustomerRequest => Sales.RecommendedCoreAction.CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST,
        CoreActionRecommendation.NoStateChangeWaitForTimeout => Sales.RecommendedCoreAction.CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT,
        CoreActionRecommendation.RevalidateAndExpireConfirmation => Sales.RecommendedCoreAction.CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION,
        CoreActionRecommendation.RevalidateAndHoldAdminReview => Sales.RecommendedCoreAction.CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW,
        CoreActionRecommendation.IgnoreStaleCallback => Sales.RecommendedCoreAction.CORE_IGNORE_STALE_CALLBACK,
        CoreActionRecommendation.BlockDueToOperationalConstraint => Sales.RecommendedCoreAction.CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT,
        _ => throw new InvalidOperationException("Unsupported core-action recommendation."),
    };
}
