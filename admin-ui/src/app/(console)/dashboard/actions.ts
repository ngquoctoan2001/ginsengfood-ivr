"use server";

import { revalidatePath } from "next/cache";

import { disableSimChannel, enableSimChannel, pauseQueue, resumeQueue } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrApiResponse } from "@/lib/api/client";
import type { IvrAdminActionResult } from "@/lib/api/types";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requirePermission } from "@/lib/auth/guard";
import type { AdminSession } from "@/lib/auth/session";
import { readConfig, type AdminUiConfig } from "@/lib/config/env";
import type { IvrPermission } from "@/lib/rbac/permissions";

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
  permission: IvrPermission,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const session = await requirePermission(permission);
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
  return runQueueMutation(formData, pauseQueue, "IVR_QUEUE_PAUSE");
}

export async function resumeQueueAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runQueueMutation(formData, resumeQueue, "IVR_QUEUE_RESUME");
}

type SimChannelMutation = (
  context: { session: AdminSession; config: AdminUiConfig },
  simChannelId: string,
  request: { reason: string; evidence_ref?: string },
) => Promise<IvrApiResponse<IvrAdminActionResult>>;

/**
 * Enable/disable for one SIM channel (`specs/ui/08` §3).
 *
 * The channel id travels as a hidden field rather than as a closure argument so
 * the control keeps working without client JavaScript, like every other admin
 * action in this console. It is still the server that decides: a missing
 * permission comes back as `403 IVR_FORBIDDEN_CALLER`.
 */
async function runSimChannelMutation(
  formData: FormData,
  mutate: SimChannelMutation,
  permission: IvrPermission,
): Promise<AdminActionState> {
  const simChannelId = String(formData.get("simChannelId") ?? "").trim();
  if (simChannelId === "") {
    return { status: "invalid", messageKey: "sim.channelRequired" };
  }

  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const session = await requirePermission(permission);
  const config = readConfig();

  try {
    const response = await mutate(
      { session, config },
      simChannelId,
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

export async function disableSimChannelAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runSimChannelMutation(formData, disableSimChannel, "IVR_SIM_DISABLE");
}

export async function enableSimChannelAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runSimChannelMutation(formData, enableSimChannel, "IVR_SIM_ENABLE");
}
