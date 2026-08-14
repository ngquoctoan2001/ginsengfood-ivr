using Ivr.Domain.Ports;

namespace Ivr.Domain.Confirmation;

public sealed record AttemptNormalizationContext(
    int AttemptNumber,
    int MaxAttempts,
    DateTimeOffset OccurredAt,
    DateTimeOffset ConfirmationWindowExpiresAt,
    int PriorTechnicalRetryCount,
    int TechnicalRetryLimit)
{
    public void Validate()
    {
        if (AttemptNumber < 1 || AttemptNumber > MaxAttempts || MaxAttempts is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(AttemptNumber));
        }

        if (PriorTechnicalRetryCount < 0 || TechnicalRetryLimit is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(PriorTechnicalRetryCount));
        }
    }
}

public sealed record NormalizedResult(
    IvrResultType ResultType,
    bool IsCounted,
    bool IsFinal,
    string Reason,
    string? DtmfKey,
    string? TechnicalErrorCode,
    CoreActionRecommendation RecommendedCoreAction,
    bool HumanReviewRequired,
    bool TechnicalRetryAllowed,
    int TechnicalRetryCount)
{
    public bool IsNoAnswer => ResultType is IvrResultType.IvrNoAnswerAttempt
        or IvrResultType.IvrNoAnswerFinal;

    public bool IsInvalidPhone => ResultType == IvrResultType.IvrInvalidPhoneFinal;

    public string ResultStatus => ResultType switch
    {
        IvrResultType.IvrConfirmed => "IVR_CONFIRMED",
        IvrResultType.IvrCustomerCancelled => "IVR_CUSTOMER_CANCELLED",
        IvrResultType.IvrNoAnswerAttempt => "IVR_NO_ANSWER_ATTEMPT",
        IvrResultType.IvrNoAnswerFinal => "IVR_NO_ANSWER_FINAL",
        IvrResultType.IvrConfirmationWindowExpired => "IVR_CONFIRMATION_WINDOW_EXPIRED",
        IvrResultType.IvrInvalidPhoneFinal => "IVR_INVALID_PHONE_FINAL",
        IvrResultType.IvrWrongInput => "IVR_WRONG_INPUT",
        IvrResultType.IvrTechnicalException => "IVR_TECHNICAL_EXCEPTION",
        IvrResultType.IvrCapacityException => "IVR_CAPACITY_EXCEPTION",
        IvrResultType.IvrOperationalBlocked => "IVR_OPERATIONAL_BLOCKED",
        IvrResultType.IvrPolicyBlocked => "IVR_POLICY_BLOCKED",
        _ => throw new InvalidOperationException("Unsupported normalized result type."),
    };
}

public static class DispositionMapper
{
    public static NormalizedResult Normalize(
        SimProviderDisposition? disposition,
        string? rawDtmf,
        string? technicalErrorCode,
        AttemptNormalizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Validate();
        return disposition switch
        {
            SimProviderDisposition.Answered => MapAnswered(rawDtmf, context),
            SimProviderDisposition.RingTimeout => MapNoAnswer("RING_TIMEOUT", false, context),
            SimProviderDisposition.Busy => MapNoAnswer("BUSY", false, context),
            SimProviderDisposition.Rejected => MapNoAnswer("REJECTED_REVIEW_REQUIRED", true, context),
            SimProviderDisposition.Unreachable => InvalidPhone("UNREACHABLE"),
            SimProviderDisposition.InvalidDestination => InvalidPhone("INVALID_DESTINATION"),
            SimProviderDisposition.Dropped => Technical("PROVIDER_DROPPED", technicalErrorCode, context),
            SimProviderDisposition.NetworkError => Technical("NETWORK_ERROR", technicalErrorCode, context),
            SimProviderDisposition.SimError => Technical("SIM_ERROR", technicalErrorCode, context),
            SimProviderDisposition.AudioError => Technical("AUDIO_ERROR", technicalErrorCode, context),
            SimProviderDisposition.DtmfError => Technical("DTMF_ERROR", technicalErrorCode, context),
            null => Technical("UNMAPPED_PROVIDER_DISPOSITION", technicalErrorCode, context),
            _ => Technical("UNMAPPED_PROVIDER_DISPOSITION", technicalErrorCode, context),
        };
    }

    public static NormalizedResult Capacity(AttemptNormalizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Validate();
        return new NormalizedResult(
            IvrResultType.IvrCapacityException,
            false,
            true,
            "CAPACITY_UNAVAILABLE",
            null,
            "CAPACITY_UNAVAILABLE",
            CoreActionRecommendation.RevalidateAndHoldAdminReview,
            true,
            false,
            context.PriorTechnicalRetryCount);
    }

    private static NormalizedResult MapAnswered(
        string? rawDtmf,
        AttemptNormalizationContext context)
    {
        string? key = string.IsNullOrWhiteSpace(rawDtmf)
            ? null
            : rawDtmf.Trim();
        return key switch
        {
            "1" => new NormalizedResult(
                IvrResultType.IvrConfirmed,
                true,
                true,
                "CUSTOMER_PRESSED_1",
                "1",
                null,
                CoreActionRecommendation.RevalidateAndConfirmOrder,
                false,
                false,
                context.PriorTechnicalRetryCount),
            "0" => new NormalizedResult(
                IvrResultType.IvrCustomerCancelled,
                true,
                true,
                "CUSTOMER_PRESSED_0",
                "0",
                null,
                CoreActionRecommendation.RevalidateAndCancelCustomerRequest,
                false,
                false,
                context.PriorTechnicalRetryCount),
            null => MapNoAnswer("ANSWERED_NO_INPUT", false, context),
            _ => MapWrongInput(context),
        };
    }

    private static NormalizedResult MapWrongInput(AttemptNormalizationContext context)
    {
        if (ReachedFinalAttempt(context))
        {
            return MapNoAnswer("WRONG_INPUT_MAX_ATTEMPTS", false, context);
        }

        return new NormalizedResult(
            IvrResultType.IvrWrongInput,
            true,
            false,
            "UNSUPPORTED_DTMF_KEY",
            "INVALID",
            null,
            CoreActionRecommendation.NoStateChangeWaitForTimeout,
            false,
            false,
            context.PriorTechnicalRetryCount);
    }

    private static NormalizedResult MapNoAnswer(
        string reason,
        bool humanReviewRequired,
        AttemptNormalizationContext context)
    {
        bool final = ReachedFinalAttempt(context);
        return new NormalizedResult(
            final ? IvrResultType.IvrNoAnswerFinal : IvrResultType.IvrNoAnswerAttempt,
            true,
            final,
            reason,
            null,
            null,
            CoreActionRecommendation.NoStateChangeWaitForTimeout,
            humanReviewRequired,
            false,
            context.PriorTechnicalRetryCount);
    }

    private static NormalizedResult InvalidPhone(string reason) => new(
        IvrResultType.IvrInvalidPhoneFinal,
        false,
        true,
        reason,
        null,
        null,
        CoreActionRecommendation.RevalidateAndHoldAdminReview,
        true,
        false,
        0);

    private static NormalizedResult Technical(
        string fallbackCode,
        string? rawTechnicalCode,
        AttemptNormalizationContext context)
    {
        int retryCount = checked(context.PriorTechnicalRetryCount + 1);
        bool withinWindow = context.OccurredAt < context.ConfirmationWindowExpiresAt;
        bool retryAllowed = withinWindow && retryCount <= context.TechnicalRetryLimit;
        string safeCode = NormalizeTechnicalCode(rawTechnicalCode, fallbackCode);
        return new NormalizedResult(
            IvrResultType.IvrTechnicalException,
            false,
            false,
            safeCode,
            null,
            safeCode,
            CoreActionRecommendation.RevalidateAndHoldAdminReview,
            !retryAllowed,
            retryAllowed,
            retryCount);
    }

    private static bool ReachedFinalAttempt(AttemptNormalizationContext context) =>
        context.AttemptNumber >= context.MaxAttempts
        || context.OccurredAt >= context.ConfirmationWindowExpiresAt;

    private static string NormalizeTechnicalCode(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        string normalized = new(
            value.Trim().ToUpperInvariant()
                .Where(character => char.IsAsciiLetterOrDigit(character) || character == '_')
                .Take(80)
                .ToArray());
        return normalized.Length == 0 ? fallback : normalized;
    }
}
