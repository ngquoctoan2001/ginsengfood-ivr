"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { ButtonGroup, Callout, TextField, TextareaField } from "@/components/ui";
import { t } from "@/lib/i18n";

import {
  clearLabAllowlistAction,
  engageKillSwitchAction,
  releaseKillSwitchAction,
  revokeRealCustomerCallsAction,
  widenLabAllowlistAction,
} from "./actions";

export interface RuntimeGateActionsProps {
  /**
   * Whether this deployment may raise risk from a console form at all.
   *
   * False for PRODUCTION_REAL, and also false for any production or unfamiliar
   * environment label. Gating on execution mode alone would leave the widest
   * raise of all available in a production deployment: flipping the mode *to*
   * PRODUCTION_REAL is itself a risk increase, and it is reachable while the
   * mode is still MOCK.
   */
  readonly allowRiskIncrease: boolean;
}

/**
 * Runtime-gate controls, split by direction rather than by flag (W-0110).
 *
 * Grouping by flag would put "engage the kill switch" and "release the kill
 * switch" side by side as if they were the same weight of decision. They are
 * not: one is always allowed and costs a reason, the other needs a second
 * person. The layout is the first place an operator meets that asymmetry, so it
 * is the layout that has to carry it.
 */
export function RuntimeGateActions({ allowRiskIncrease }: RuntimeGateActionsProps) {
  return (
    <>
      <ButtonGroup label={t("flags.reduceLabel")}>
        <AdminActionDialog
          perm="IVR_RUNTIME_GATE_ADMIN"
          label={t("flags.engageKillSwitch")}
          description={t("flags.engageKillSwitchDescription")}
          action={engageKillSwitchAction}
        />
        <AdminActionDialog
          perm="IVR_RUNTIME_GATE_ADMIN"
          label={t("flags.revokeRealCalls")}
          description={t("flags.revokeRealCallsDescription")}
          action={revokeRealCustomerCallsAction}
        />
        <AdminActionDialog
          perm="IVR_RUNTIME_GATE_ADMIN"
          label={t("flags.clearAllowlist")}
          description={t("flags.clearAllowlistDescription")}
          action={clearLabAllowlistAction}
        />
      </ButtonGroup>

      {allowRiskIncrease ? (
        <ButtonGroup label={t("flags.raiseLabel")}>
          <AdminActionDialog
            perm="IVR_RUNTIME_GATE_ADMIN"
            label={t("flags.releaseKillSwitch")}
            description={t("flags.releaseKillSwitchDescription")}
            action={releaseKillSwitchAction}
          >
            <ApprovalReferenceField />
          </AdminActionDialog>
          <AdminActionDialog
            perm="IVR_RUNTIME_GATE_ADMIN"
            label={t("flags.widenAllowlist")}
            description={t("flags.widenAllowlistDescription")}
            action={widenLabAllowlistAction}
          >
            <Callout tone="warning" role="alert" testId="flags-self-destination-warning">
              {t("flags.selfDestinationWarning")}
            </Callout>
            <TextareaField
              label={t("flags.destinationsLabel")}
              name="destinations"
              required
              rows={3}
              placeholder={t("flags.destinationsPlaceholder")}
            />
            <ApprovalReferenceField />
          </AdminActionDialog>
        </ButtonGroup>
      ) : (
        <Callout tone="locked" role="alert" testId="flags-production-blocked">
          {t("flags.productionBlocked")}
        </Callout>
      )}
    </>
  );
}

/**
 * The approval reference is opaque and verified server-side. It is never an
 * approver's name typed by the person doing the change — that would be a
 * four-eyes control the actor fills in for themselves.
 */
function ApprovalReferenceField() {
  return (
    <>
      <Callout tone="info">{t("flags.approvalNotice")}</Callout>
      <TextField
        label={t("flags.approvalLabel")}
        name="approval_reference"
        width="full"
        required
        placeholder={t("flags.approvalPlaceholder")}
      />
    </>
  );
}
