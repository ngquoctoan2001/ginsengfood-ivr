"use server";

import { revalidatePath } from "next/cache";

import { mutateFeatureFlags } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrFeatureFlagChangeSet } from "@/lib/api/types";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requirePermission } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";

/**
 * Runtime-gate mutations (W-0110).
 *
 * The asymmetry in `specs/api/03` is the whole point of this file: a change that
 * *reduces* risk needs only a reason, and a change that *raises* it needs a
 * verified four-eyes approval. Both halves are enforced by Ivr.Api — it computes
 * `increasedRiskKeys` from before/after and refuses without an approval
 * reference. What these actions add is that the risk-raising path cannot be
 * submitted from this console without one, so an operator meets the rule at the
 * form rather than as a 409 after the fact.
 *
 * They deliberately do not re-derive which keys are risky. That table lives in
 * `FeatureFlagGuardrails.AssessRisk`, and a second copy here would be a second
 * answer to "is this dangerous" that drifts from the one that actually decides.
 */

/** Engaging the kill switch. Unconditional risk reduction: reason only. */
export async function engageKillSwitchAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runMutation(formData, { globalDialKillSwitch: true }, false);
}

/** Withdrawing customer-call permission. Unconditional risk reduction. */
export async function revokeRealCustomerCallsAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runMutation(formData, { realCustomerCallAllowed: false }, false);
}

/** Emptying the lab allowlist. Unconditional risk reduction. */
export async function clearLabAllowlistAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runMutation(formData, { labDestinationAllowlist: [] }, false);
}

/**
 * Releasing the kill switch. Risk-raising, so it carries an approval reference
 * and is refused outright in PRODUCTION_REAL — see `runMutation`.
 */
export async function releaseKillSwitchAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  return runMutation(formData, { globalDialKillSwitch: false }, true);
}

/**
 * Widening the lab allowlist.
 *
 * The server refuses an actor who adds their own call destination — but only
 * when it knows that destination, and a console session carries no such claim
 * today (see the note on the screen). Until it does, the four-eyes approval is
 * the control that actually holds here.
 */
export async function widenLabAllowlistAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const raw = formData.get("destinations");
  if (typeof raw !== "string" || raw.trim().length === 0) {
    return { status: "invalid", messageKey: "flags.destinationsRequired" };
  }

  const destinations = [
    ...new Set(
      raw
        .split(/[\n,]/u)
        .map((entry) => entry.trim())
        .filter((entry) => entry.length > 0),
    ),
  ];
  if (destinations.length === 0) {
    return { status: "invalid", messageKey: "flags.destinationsRequired" };
  }

  return runMutation(formData, { labDestinationAllowlist: destinations }, true);
}

async function runMutation(
  formData: FormData,
  changes: IvrFeatureFlagChangeSet,
  raisesRisk: boolean,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const config = readConfig();
  const approvalReference = formData.get("approval_reference");

  if (raisesRisk) {
    // Production raises go through a deployment with its own approval
    // (P7-3/P9-1), not through a console form. Refused here as well as in the
    // rendering, so a stale page or a hand-built POST cannot reach it either.
    //
    // The environment check matters as much as the mode one: flipping the mode
    // *to* PRODUCTION_REAL is itself a risk increase, and it is reachable from a
    // production deployment while the mode is still MOCK. isNonProductionEnvironment
    // is an allowlist, so an unfamiliar label locks rather than opens.
    if (!config.isNonProductionEnvironment || config.executionMode === "PRODUCTION_REAL") {
      return { status: "invalid", messageKey: "flags.productionBlocked" };
    }

    if (typeof approvalReference !== "string" || approvalReference.trim().length === 0) {
      return { status: "invalid", messageKey: "flags.approvalRequired" };
    }
  }

  const session = await requirePermission("IVR_RUNTIME_GATE_ADMIN");

  try {
    const response = await mutateFeatureFlags({ session, config }, config.environmentLabel, {
      changes,
      reason: validation.reason,
      ...(raisesRisk && typeof approvalReference === "string"
        ? { approvalReference: approvalReference.trim() }
        : {}),
    });

    revalidatePath("/flags");
    return {
      status: "success",
      adminActionId: `revision-${response.data.snapshot.revision}`,
      correlationId: response.correlationId,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}
