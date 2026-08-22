"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { ButtonGroup } from "@/components/ui";
import { t } from "@/lib/i18n";

import { approveScriptAction, retireScriptAction, submitScriptAction } from "./actions";

export interface ScriptLifecycleActionsProps {
  readonly templateId: string;
  readonly version: string;
  readonly status: "DRAFT" | "IN_REVIEW" | "APPROVED" | "RETIRED";
}

/**
 * Lifecycle controls for one script version (W-0109).
 *
 * Two spec rules stay structural rather than optional, and neither is enforced
 * here: the template variable whitelist and the disabled KEY_9 live in
 * `TargetV1SpeechPolicy.ValidateTemplate`, which every draft passes through on
 * the server. There is deliberately no field on this screen that could widen
 * either one.
 *
 * Buttons are hidden by lifecycle state only as a convenience. The server still
 * refuses an invalid transition, so a stale page cannot approve a retired
 * version by having rendered before it was retired.
 */
export function ScriptLifecycleActions({
  templateId,
  version,
  status,
}: ScriptLifecycleActionsProps) {
  const hiddenFields = { template_id: templateId, version };
  if (status === "RETIRED") {
    return null;
  }

  return (
    <ButtonGroup label={t("config.actionsLabel")}>
      {status === "DRAFT" ? (
        <AdminActionDialog
          perm="IVR_SCRIPT_REVIEW"
          label={t("config.submitReview")}
          description={t("config.submitReviewDescription")}
          action={submitScriptAction}
          hiddenFields={hiddenFields}
        />
      ) : null}

      {status === "IN_REVIEW" || status === "APPROVED" ? (
        <>
          <AdminActionDialog
            perm="IVR_SCRIPT_APPROVE_MOCK"
            label={t("config.approveMock")}
            description={t("config.approveMockDescription")}
            action={approveScriptAction}
            hiddenFields={{ ...hiddenFields, approval_type: "MOCK_TEST" }}
          />
          <AdminActionDialog
            perm="IVR_SCRIPT_APPROVE_LAB"
            label={t("config.approveLab")}
            description={t("config.approveLabDescription")}
            action={approveScriptAction}
            hiddenFields={{ ...hiddenFields, approval_type: "LAB" }}
          />
          <AdminActionDialog
            perm="IVR_SCRIPT_APPROVE_CONTENT"
            label={t("config.approveContent")}
            description={t("config.approveContentDescription")}
            action={approveScriptAction}
            hiddenFields={{ ...hiddenFields, approval_type: "CONTENT" }}
          />
          <AdminActionDialog
            perm="IVR_SCRIPT_APPROVE_PRIVACY_LEGAL"
            label={t("config.approvePrivacyLegal")}
            description={t("config.approvePrivacyLegalDescription")}
            action={approveScriptAction}
            hiddenFields={{ ...hiddenFields, approval_type: "PRIVACY_LEGAL" }}
          />
        </>
      ) : null}

      {status === "IN_REVIEW" || status === "APPROVED" ? (
        <AdminActionDialog
          perm="IVR_SCRIPT_RETIRE"
          label={t("config.retire")}
          description={t("config.retireDescription")}
          action={retireScriptAction}
          hiddenFields={hiddenFields}
        />
      ) : null}
    </ButtonGroup>
  );
}
