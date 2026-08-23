"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { usePermissions } from "@/components/rbac/PermissionProvider";
import { REASON_MAX_LENGTH } from "@/lib/admin/action-state";
import type { IvrReviewItemDetail, IvrTechnicalExceptionDetail } from "@/lib/api/types";
import { ButtonGroup, TextareaField } from "@/components/ui";
import { t } from "@/lib/i18n";

import { adminReviewAction, technicalRetryAction, terminateCallAction } from "./actions";

export interface CallDetailActionsProps {
  readonly technicalExceptions: readonly IvrTechnicalExceptionDetail[];
  readonly reviewItems: readonly IvrReviewItemDetail[];
  readonly ivrCallJobId: string;
  /** True while an attempt is dialling or connected, i.e. there is something to cut. */
  readonly hasCallInProgress: boolean;
}

/**
 * The only two admin actions on this screen.
 *
 * There is deliberately no confirm-order, cancel-order, reset-attempt or
 * force-dispatch control: order state belongs to Order Core (D-02) and the
 * attempt policy is not negotiable from a console (D-10).
 */
export function CallDetailActions({
  technicalExceptions,
  reviewItems,
  ivrCallJobId,
  hasCallInProgress,
}: CallDetailActionsProps) {
  const { can } = usePermissions();
  const retryable = can("IVR_MANUAL_RETRY")
    ? technicalExceptions.filter((exception) => exception.technical_retry_allowed)
    : [];
  const openReviews = can("IVR_RESULT_REVIEW")
    ? reviewItems.filter((item) => item.status === "OPEN")
    : [];

  // Render nothing rather than an empty toolbar when the actor holds neither
  // permission. Each dialog is still wrapped in RequirePermission below, which
  // stays the actual gate.
  const canTerminate = hasCallInProgress && can("IVR_CALL_TERMINATE");
  if (retryable.length === 0 && openReviews.length === 0 && !canTerminate) {
    return null;
  }

  return (
    <ButtonGroup>
      {/* W-0111. Shown only while something is actually on the line — the button is
          hidden by state for readability, and the server still answers 409 if the
          call ended between this render and the press. */}
      {canTerminate ? (
        <AdminActionDialog
          perm="IVR_CALL_TERMINATE"
          label={t("detail.terminate")}
          description={t("detail.terminateDescription")}
          action={terminateCallAction}
          hiddenFields={{ ivrCallJobId }}
        />
      ) : null}

      {retryable.map((exception) => (
        <AdminActionDialog
          key={exception.technical_exception_id}
          perm="IVR_MANUAL_RETRY"
          label={`${t("detail.retryAction")} · ${exception.exception_type}`}
          description={t("detail.retryDescription")}
          action={technicalRetryAction}
          hiddenFields={{
            technicalExceptionId: exception.technical_exception_id,
            targetAttemptId: exception.ivr_call_attempt_id,
          }}
        />
      ))}

      {openReviews.map((item) => (
        <AdminActionDialog
          key={item.review_item_id}
          perm="IVR_RESULT_REVIEW"
          label={`${t("detail.reviewAction")} · ${item.review_item_id}`}
          description={t("detail.reviewDescription")}
          action={adminReviewAction}
          hiddenFields={{ reviewItemId: item.review_item_id }}
        >
          <TextareaField
            label={t("detail.resolutionLabel")}
            name="resolution"
            required
            rows={2}
            maxLength={REASON_MAX_LENGTH}
            placeholder={t("detail.resolutionPlaceholder")}
          />
        </AdminActionDialog>
      ))}
    </ButtonGroup>
  );
}
