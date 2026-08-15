"use server";

import { revalidatePath } from "next/cache";

import { pauseQueue, resumeQueue } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrApiResponse } from "@/lib/api/client";
import type { IvrAdminActionResult } from "@/lib/api/types";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requireSession } from "@/lib/auth/guard";
import type { AdminSession } from "@/lib/auth/session";
import { readConfig, type AdminUiConfig } from "@/lib/config/env";

type QueueMutation = (
  context: { session: AdminSession; config: AdminUiConfig },
  request: { reason: string; evidence_ref?: string },
) => Promise<IvrApiResponse<IvrAdminActionResult>>;

/**
 * Shared body for the queue mutations.
 *
 * Nothing here decides whether the actor may act — the call is made with the
 * session's identity and Ivr.Api answers 403 `IVR_FORBIDDEN_CALLER` if the
 * permission is missing. The UI's job is to carry a reason and render whatever
 * envelope comes back.
 */
async function runQueueMutation(
  formData: FormData,
  mutate: QueueMutation,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const session = await requireSession();
  const config = readConfig();

  try {
    const response = await mutate(
      { session, config },
      validation.evidenceRef === undefined
        ? { reason: validation.reason }
        : { reason: validation.reason, evidence_ref: validation.evidenceRef },
    );

    revalidatePath("/dashboard");
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

export async function pauseQueueAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runQueueMutation(formData, pauseQueue);
}

export async function resumeQueueAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runQueueMutation(formData, resumeQueue);
}
