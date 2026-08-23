"use server";

import { revalidatePath } from "next/cache";

import {
  applyDevIntegrationProfile,
  dryRunDevScenario,
  loadDevSeed,
} from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import { validateAdminMutation, type AdminActionState } from "@/lib/admin/action-state";
import { requirePermission } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";

/**
 * UI-07 developer surface (W-0112).
 *
 * These three actions do not guard against production themselves, and that is deliberate.
 * `Ivr.Api` does not serve the routes at all outside a non-production deployment, so a console
 * that tried to decide the same question here would be a second answer that could drift from
 * the one that actually holds. The screen hides the controls off non-production; the API is what
 * refuses them.
 */

/** Loads the seed fixtures through the real intake path. */
export async function loadSeedAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const session = await requirePermission("IVR_DEV_TOOLING");
  const config = readConfig();

  try {
    const response = await loadDevSeed(
      { session, config },
      { reason: validation.reason },
    );
    revalidatePath("/seed");
    // No row in ivr_admin_actions to point at: the loader's effects are the tasks themselves.
    // The summary goes in that slot instead, following the same precedent as the feature-flag
    // action, which reports a revision rather than inventing an id it does not have.
    return {
      status: "success",
      adminActionId:
        `${response.data.dataset}: ${response.data.accepted_count}/${response.data.task_count}`,
      correlationId: response.data.correlation_id,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}

/** Replays one scenario. Places no call — the replay engine holds no telephony port. */
export async function dryRunScenarioAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const scenarioId = formData.get("scenarioId");
  if (typeof scenarioId !== "string" || scenarioId.trim().length === 0) {
    return { status: "invalid", messageKey: "seed.scenarioRequired" };
  }

  const session = await requirePermission("IVR_DEV_TOOLING");
  const config = readConfig();

  try {
    const response = await dryRunDevScenario(
      { session, config },
      scenarioId.trim(),
      { reason: validation.reason },
    );
    revalidatePath("/seed");
    return {
      status: "success",
      adminActionId: `${response.data.scenario_id}: ${response.data.coverage}`,
      correlationId: response.data.correlation_id,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}

/**
 * Applies an integration-status profile.
 *
 * Only SIM_GATEWAY is enforced; the other four dependencies are declared and nothing consults
 * them, because IVR never probes them. The screen says so next to the control rather than
 * leaving an operator to believe a fail-closed path was just rehearsed.
 */
export async function applyIntegrationProfileAction(
  _state: AdminActionState,
  formData: FormData,
): Promise<AdminActionState> {
  const validation = validateAdminMutation(formData);
  if (!validation.ok) {
    return { status: "invalid", messageKey: validation.messageKey };
  }

  const profileId = formData.get("profileId");
  if (typeof profileId !== "string" || profileId.trim().length === 0) {
    return { status: "invalid", messageKey: "seed.profileRequired" };
  }

  const session = await requirePermission("IVR_DEV_TOOLING");
  const config = readConfig();

  try {
    const response = await applyDevIntegrationProfile(
      { session, config },
      profileId.trim(),
      { reason: validation.reason },
    );
    revalidatePath("/seed");
    return {
      status: "success",
      adminActionId:
        `${response.data.profile_id}: ${response.data.enforced_count}/${response.data.effects.length}`,
      correlationId: response.data.correlation_id,
    };
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      return { status: "error", error: cause.toEnvelope() };
    }

    throw cause;
  }
}
