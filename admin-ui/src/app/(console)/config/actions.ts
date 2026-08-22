"use server";

import { revalidatePath } from "next/cache";

import {
  approveScriptVersion,
  retireScriptVersion,
  submitScriptForReview,
} from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requirePermission } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import type { IvrPermission } from "@/lib/rbac/permissions";

const APPROVAL_PERMISSIONS = {
  MOCK_TEST: "IVR_SCRIPT_APPROVE_MOCK",
  LAB: "IVR_SCRIPT_APPROVE_LAB",
  CONTENT: "IVR_SCRIPT_APPROVE_CONTENT",
  PRIVACY_LEGAL: "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
} as const satisfies Readonly<Record<string, IvrPermission>>;

type ApprovalType = keyof typeof APPROVAL_PERMISSIONS;

/**
 * Script lifecycle transitions (W-0109).
 *
 * These decide nothing about who may act. Ivr.Api answers 403 when the caller is
 * the creator or the account that already signed the other half of the production
 * pair, and 409 when the version's own state refuses — and both answers are
 * rendered here rather than pre-empted, because a client-side guess at the rule
 * is a second copy of the rule.
 */
export async function submitScriptAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runTransition(formData, "IVR_SCRIPT_REVIEW", (context, key, reason) =>
    submitScriptForReview(context, key.templateId, key.version, { reason }));
}

export async function approveScriptAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const approvalType = formData.get("approval_type");
  if (typeof approvalType !== "string" || !(approvalType in APPROVAL_PERMISSIONS)) {
    return { status: "invalid", messageKey: "action.reasonRequired" };
  }

  const typed = approvalType as ApprovalType;
  return runTransition(formData, APPROVAL_PERMISSIONS[typed], (context, key, reason) =>
    approveScriptVersion(context, key.templateId, key.version, {
      approval_type: typed,
      reason,
    }));
}

export async function retireScriptAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runTransition(formData, "IVR_SCRIPT_RETIRE", (context, key, reason) =>
    retireScriptVersion(context, key.templateId, key.version, { reason }));
}

interface VersionKey {
  readonly templateId: string;
  readonly version: string;
}

async function runTransition(
  formData: FormData,
  permission: IvrPermission,
  mutate: (
    context: Parameters<typeof submitScriptForReview>[0],
    key: VersionKey,
    reason: string,
  ) => Promise<{ data: { correlation_id: string; target_id: string } }>,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const templateId = formData.get("template_id");
  const version = formData.get("version");
  if (typeof templateId !== "string" || typeof version !== "string") {
    return { status: "invalid", messageKey: "action.reasonRequired" };
  }

  const session = await requirePermission(permission);
  const config = readConfig();

  try {
    const response = await mutate({ session, config }, { templateId, version }, validation.reason);
    revalidatePath("/config");
    return {
      status: "success",
      adminActionId: response.data.target_id,
      correlationId: response.data.correlation_id,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}
