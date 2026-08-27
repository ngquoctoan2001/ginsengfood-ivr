using Ivr.Domain.Confirmation;

namespace Ivr.Infrastructure.Persistence;

/// <summary>
/// The storage spelling of <see cref="CoreActionRecommendation"/>.
/// <para>
/// One writer is the normalizer; the other is the scheduler's confirmation-window sweep, which
/// writes a final result without passing through normalization. Both land in the same column under
/// the same closed CHECK constraint, so the mapping lives here rather than once per writer: a
/// second copy drifts silently until a constraint violation surfaces it in production, which is
/// the worst possible moment to learn the two disagreed.
/// </para>
/// </summary>
public static class ResultStorageVocabulary
{
    public static string ToCoreAction(CoreActionRecommendation recommendation) =>
        recommendation switch
        {
            CoreActionRecommendation.RevalidateAndConfirmOrder =>
                "REVALIDATE_AND_CONFIRM_ORDER",
            CoreActionRecommendation.RevalidateAndCancelCustomerRequest =>
                "REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST",
            CoreActionRecommendation.NoStateChangeWaitForTimeout =>
                "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
            CoreActionRecommendation.RevalidateAndExpireConfirmation =>
                "REVALIDATE_AND_EXPIRE_CONFIRMATION",
            CoreActionRecommendation.RevalidateAndHoldAdminReview =>
                "REVALIDATE_AND_HOLD_ADMIN_REVIEW",
            CoreActionRecommendation.IgnoreStaleCallback => "IGNORE_STALE_CALLBACK",
            CoreActionRecommendation.BlockDueToOperationalConstraint =>
                "BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT",
            _ => throw new InvalidOperationException("Unsupported Core action recommendation."),
        };
}
