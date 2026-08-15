"use client";

import { useActionState, useEffect, useRef, type ReactNode } from "react";

import { ErrorAlert } from "@/components/feedback/ErrorAlert";
import { RequirePermission } from "@/components/rbac/RequirePermission";
import { IDLE_ACTION_STATE, REASON_MAX_LENGTH, type AdminActionState } from "@/lib/admin/action-state";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import controls from "@/components/forms/Controls.module.css";
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

  useEffect(() => {
    if (state.status === "success") {
      dialogRef.current?.close();
    }
  }, [state]);

  return (
    <RequirePermission perm={perm}>
      <div className={styles.wrapper}>
        <button
          type="button"
          className={controls.secondary}
          onClick={() => dialogRef.current?.showModal?.()}
        >
          {label}
        </button>

        <dialog ref={dialogRef} className={styles.dialog} aria-label={label}>
          <form action={formAction} className={controls.stack}>
            <h2 className={styles.title}>{label}</h2>
            <p className={styles.description}>{description}</p>

            {Object.entries(hiddenFields ?? {}).map(([name, value]) => (
              <input key={name} type="hidden" name={name} value={value} />
            ))}
            {children}

            <label className={controls.field}>
              <span className={controls.label}>{t("action.reasonLabel")}</span>
              <textarea
                name="reason"
                required
                maxLength={REASON_MAX_LENGTH}
                rows={3}
                placeholder={t("action.reasonPlaceholder")}
                className={controls.textarea}
              />
            </label>

            <label className={controls.field}>
              <span className={controls.label}>{t("action.evidenceLabel")}</span>
              <input
                type="text"
                name="evidenceRef"
                placeholder={t("action.evidencePlaceholder")}
                className={controls.control}
              />
            </label>

            <p className={styles.notice}>{t("action.auditNotice")}</p>

            {state.status === "invalid" ? (
              <p className={controls.invalid} role="alert">
                {t(state.messageKey)}
              </p>
            ) : null}
            {state.status === "error" ? <ErrorAlert error={state.error} /> : null}

            <div className={styles.actions}>
              <button
                type="button"
                className={controls.secondary}
                onClick={() => dialogRef.current?.close()}
              >
                {t("action.cancel")}
              </button>
              <button type="submit" className={controls.primary} disabled={isPending}>
                {isPending ? t("action.submitting") : t("action.confirm")}
              </button>
            </div>
          </form>
        </dialog>
      </div>
    </RequirePermission>
  );
}
