"use client";

import { useActionState, useRef, type ReactNode } from "react";

import { ErrorAlert } from "@/components/feedback/ErrorAlert";
import { RequirePermission } from "@/components/rbac/RequirePermission";
import {
  Button,
  ButtonGroup,
  Callout,
  DescriptionList,
  TextField,
  TextareaField,
} from "@/components/ui";
import { IDLE_ACTION_STATE, REASON_MAX_LENGTH, type AdminActionState } from "@/lib/admin/action-state";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import styles from "./AdminActionDialog.module.css";

export interface AdminActionDialogProps {
  readonly perm: IvrPermission;
  readonly label: string;
  readonly description: string;
  readonly action: (
    state: AdminActionState,
    formData: FormData,
  ) => Promise<AdminActionState>;
  /** Target identifiers submitted with the action, e.g. the review item id. */
  readonly hiddenFields?: Readonly<Record<string, string>>;
  /** Extra required inputs rendered above the reason field. */
  readonly children?: ReactNode;
}

/**
 * Confirmation dialog for a single admin mutation.
 *
 * Two rules are structural rather than advisory: the trigger is wrapped in
 * `RequirePermission` so it never appears for an actor who lacks the permission,
 * and the form cannot be submitted without a reason, because every admin
 * mutation is audited with one (specs/api/03 §2).
 */
export function AdminActionDialog({
  perm,
  label,
  description,
  action,
  hiddenFields,
  children,
}: AdminActionDialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const [state, formAction, isPending] = useActionState(action, IDLE_ACTION_STATE);
  const succeeded = state.status === "success";

  return (
    <RequirePermission perm={perm}>
      <div className={styles.wrapper}>
        <Button
          variant="secondary"
          size="sm"
          onClick={() => dialogRef.current?.showModal?.()}
        >
          {label}
        </Button>

        <dialog ref={dialogRef} className={styles.dialog} aria-label={label}>
          <form action={formAction} className={styles.form}>
            <h2 className={styles.title}>{label}</h2>
            <p className={styles.description}>{description}</p>

            {Object.entries(hiddenFields ?? {}).map(([name, value]) => (
              <input key={name} type="hidden" name={name} value={value} />
            ))}
            {children}

            <TextareaField
              label={t("action.reasonLabel")}
              name="reason"
              required
              maxLength={REASON_MAX_LENGTH}
              rows={3}
              placeholder={t("action.reasonPlaceholder")}
            />

            <TextField
              label={t("action.evidenceLabel")}
              name="evidenceRef"
              width="full"
              placeholder={t("action.evidencePlaceholder")}
            />

            <Callout tone="info">{t("action.auditNotice")}</Callout>

            {state.status === "invalid" ? (
              <Callout tone="danger" role="alert">
                {t(state.messageKey)}
              </Callout>
            ) : null}
            {state.status === "error" ? <ErrorAlert error={state.error} /> : null}
            {/* The dialog stays open on success and shows the ids. Closing
                silently left the operator with no record to quote, and the
                admin action id plus correlation id are exactly what a ticket or
                an audit lookup needs. */}
            {state.status === "success" ? (
              <div className={styles.success} role="status" data-testid="action-success">
                <Callout tone="success">{t("action.succeeded")}</Callout>
                <DescriptionList
                  layout="rows"
                  items={[
                    {
                      label: t("action.adminActionId"),
                      value: state.adminActionId,
                      mono: true,
                      testId: "action-admin-action-id",
                    },
                    {
                      label: t("error.correlationId"),
                      value: state.correlationId,
                      mono: true,
                      testId: "action-correlation-id",
                    },
                  ]}
                />
              </div>
            ) : null}

            <ButtonGroup align="end">
              <Button variant="ghost" onClick={() => dialogRef.current?.close()}>
                {succeeded ? t("action.close") : t("action.cancel")}
              </Button>
              {succeeded ? null : (
                <Button type="submit" variant="primary" pending={isPending}>
                  {t("action.confirm")}
                </Button>
              )}
            </ButtonGroup>
          </form>
        </dialog>
      </div>
    </RequirePermission>
  );
}
