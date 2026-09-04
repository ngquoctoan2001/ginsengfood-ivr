namespace Ivr.Domain.Confirmation;

/// <summary>
/// The closed runtime subset of the shared Target V1 result vocabulary.
/// </summary>
public static class ResultContractPolicy
{
    public static bool IsRuntimeResult(IvrResultType resultType) => resultType is
        IvrResultType.IvrConfirmed
        or IvrResultType.IvrCustomerCancelled
        or IvrResultType.IvrNoAnswerAttempt
        or IvrResultType.IvrNoAnswerFinal
        or IvrResultType.IvrConfirmationWindowExpired
        or IvrResultType.IvrInvalidPhoneFinal
        or IvrResultType.IvrWrongInput
        or IvrResultType.IvrTechnicalException
        or IvrResultType.IvrCapacityException;

    public static bool IsFinalCallbackResult(IvrResultType resultType) => resultType is
        IvrResultType.IvrConfirmed
        or IvrResultType.IvrCustomerCancelled
        or IvrResultType.IvrNoAnswerFinal
        or IvrResultType.IvrConfirmationWindowExpired
        or IvrResultType.IvrInvalidPhoneFinal
        or IvrResultType.IvrCapacityException;

    public static bool IsCountedCustomerAttemptResult(IvrResultType resultType) => resultType is
        IvrResultType.IvrConfirmed
        or IvrResultType.IvrCustomerCancelled
        or IvrResultType.IvrNoAnswerAttempt
        or IvrResultType.IvrNoAnswerFinal
        or IvrResultType.IvrWrongInput;

    public static void EnsureSnapshotSemantics(
        IvrResultType resultType,
        bool isCountedCustomerAttempt,
        bool isFinalForIvr,
        CoreActionRecommendation recommendedCoreAction)
    {
        if (!IsRuntimeResult(resultType))
        {
            throw new InvalidOperationException(
                "Pre-call operational and policy blocks cannot become IVR call results.");
        }

        bool expectedFinal = IsFinalCallbackResult(resultType);
        if (isFinalForIvr != expectedFinal)
        {
            throw new InvalidOperationException(
                $"Result {resultType} must have is_final_for_ivr={expectedFinal}.");
        }

        bool expectedCounted = IsCountedCustomerAttemptResult(resultType);
        if (isCountedCustomerAttempt != expectedCounted)
        {
            throw new InvalidOperationException(
                $"Result {resultType} must have is_counted_customer_attempt={expectedCounted}.");
        }

        if (!IsAllowedAction(resultType, recommendedCoreAction))
        {
            throw new InvalidOperationException(
                $"Result {resultType} cannot recommend {recommendedCoreAction}.");
        }
    }

    private static bool IsAllowedAction(
        IvrResultType resultType,
        CoreActionRecommendation action) => resultType switch
        {
            IvrResultType.IvrConfirmed =>
                action == CoreActionRecommendation.RevalidateAndConfirmOrder,
            IvrResultType.IvrCustomerCancelled =>
                action == CoreActionRecommendation.RevalidateAndCancelCustomerRequest,
            IvrResultType.IvrNoAnswerAttempt or IvrResultType.IvrNoAnswerFinal
                or IvrResultType.IvrWrongInput =>
                action == CoreActionRecommendation.NoStateChangeWaitForTimeout,
            IvrResultType.IvrConfirmationWindowExpired =>
                action is CoreActionRecommendation.RevalidateAndExpireConfirmation
                    or CoreActionRecommendation.RevalidateAndHoldAdminReview,
            IvrResultType.IvrInvalidPhoneFinal or IvrResultType.IvrTechnicalException
                or IvrResultType.IvrCapacityException =>
                action == CoreActionRecommendation.RevalidateAndHoldAdminReview,
            _ => false,
        };
}
