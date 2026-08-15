"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { t } from "@/lib/i18n";

import { disableSimChannelAction, enableSimChannelAction } from "./actions";

export interface SimChannelActionsProps {
  readonly simChannelId: string;
  readonly enabled: boolean;
  readonly busy: boolean;
}

/**
 * The two channel controls from `specs/ui/08` §3.
 *
 * Only the meaningful one is offered: an enabled channel can be disabled, a
 * disabled one enabled. Each is wrapped in `RequirePermission` by the dialog, so
 * an actor holding only `IVR_SIM_DISABLE` never sees an enable button.
 *
 * Disabling a channel that is carrying a call is allowed — it stops new dispatch
 * and takes effect when the call ends — so the description says so rather than
 * the control pretending the change is immediate.
 */
export function SimChannelActions({ simChannelId, enabled, busy }: SimChannelActionsProps) {
  return enabled ? (
    <AdminActionDialog
      perm="IVR_SIM_DISABLE"
      label={t("sim.disable")}
      description={busy ? t("sim.disableBusyDescription") : t("sim.disableDescription")}
      action={disableSimChannelAction}
      hiddenFields={{ simChannelId }}
    />
  ) : (
    <AdminActionDialog
      perm="IVR_SIM_ENABLE"
      label={t("sim.enable")}
      description={t("sim.enableDescription")}
      action={enableSimChannelAction}
      hiddenFields={{ simChannelId }}
    />
  );
}
