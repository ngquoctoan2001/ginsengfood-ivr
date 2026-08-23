"use server";

import { revalidatePath } from "next/cache";

import { requestTechnicalRetry, submitAdminReview, terminateCallJob } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requirePermission } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";

/**
 * Technical retry (`IVR_MANUAL_RETRY`).
 *
 * The exception and attempt ids come from the detail projection the operator is
 * looking at, so the request always targets a recorded technical failure. The
 * API still re-checks every precondition — window, bounded limit, blockers —
 * and refuses otherwise; nothing here can widen the attempt policy (D-10).
 */
/**
 * Asks for the call on this job to be cut (W-0111).
 *
 * Operator holds this as well as Admin: it is the risk-reducing direction, and
 * an operator who has to find an admin is an operator watching a call they were
 * already told to end. The server refuses with 409 when nothing is running, so
 * this action deliberately does not try to guess liveness from a rendered page.
 */
export async function terminateCallAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const ivrCallJobId = String(formData.get("ivrCallJobId") ?? "").trim();
  if (ivrCallJobId === "") {
    return { status: "invalid", messageKey: "action.reasonRequired" };
  }

  const session = await requirePermission("IVR_CALL_TERMINATE");
  const config = readConfig();

  try {
    const response = await terminateCallJob({ session, config }, ivrCallJobId, {
      reason: validation.reason,
      ...(validation.evidenceRef === undefined
        ? {}
        : { evidence_ref: validation.evidenceRef }),
    });

    revalidatePath(`/calls/${ivrCallJobId}`);
    return {
      status: "success",
      adminActionId: response.data.admin_action_id,
      correlationId: response.data.correlation_id,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}

export async function technicalRetryAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const technicalExceptionId = String(formData.get("technicalExceptionId") ?? "").trim();
  const targetAttemptId = String(formData.get("targetAttemptId") ?? "").trim();
  if (technicalExceptionId === "" || targetAttemptId === "") {
    return { status: "invalid", messageKey: "action.reasonRequired" };
  }

  const session = await requirePermission("IVR_MANUAL_RETRY");
  const config = readConfig();

  try {
    const response = await requestTechnicalRetry(
      { session, config },
      {
        technical_exception_id: technicalExceptionId,
        target_attempt_id: targetAttemptId,
        reason: validation.reason,
        ...(validation.evidenceRef === undefined
          ? {}
          : { evidence_ref: validation.evidenceRef }),
      },
    );

    revalidatePath("/calls");
    return {
      status: "success",
      adminActionId: response.data.admin_action_id,
      correlationId: response.correlationId,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}

/**
 * Admin review (`IVR_RESULT_REVIEW`).
 *
 * Resolves or annotates a review item. It cannot edit `ivr_call_results`,
 * cannot synthesise a result, and cannot touch order state — the API returns
 * `result_unchanged=true` precisely because that is the whole contract.
 */
export async function adminReviewAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const reviewItemId = String(formData.get("reviewItemId") ?? "").trim();
  const resolution = String(formData.get("resolution") ?? "").trim();
  if (reviewItemId === "") {
    return { status: "invalid", messageKey: "action.reasonRequired" };
  }

  if (resolution === "") {
    return { status: "invalid", messageKey: "detail.resolutionRequired" };
  }

  const session = await requirePermission("IVR_RESULT_REVIEW");
  const config = readConfig();

  try {
    const response = await submitAdminReview(
      { session, config },
      {
        review_item_id: reviewItemId,
        resolution,
        reason: validation.reason,
        ...(validation.evidenceRef === undefined
          ? {}
          : { evidence_ref: validation.evidenceRef }),
      },
    );

    revalidatePath("/calls");
    return {
      status: "success",
      adminActionId: response.data.admin_action_id,
      correlationId: response.correlationId,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}
