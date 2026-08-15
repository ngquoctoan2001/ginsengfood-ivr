import type { DirectoryEntry } from "@/lib/auth/directory";
import { formatNumber, t } from "@/lib/i18n";

import controls from "@/components/forms/Controls.module.css";

export interface LoginFormProps {
  readonly directory: readonly DirectoryEntry[];
  /** Path to return to after sign-in, already validated as same-origin. */
  readonly next: string | null;
  readonly errorMessage: string | null;
}

export function LoginForm({ directory, next, errorMessage }: LoginFormProps) {
  return (
    <form method="post" action="/api/auth/sign-in" className={controls.stack}>
      {next === null ? null : <input type="hidden" name="next" value={next} />}

      <label className={controls.field}>
        <span className={controls.label}>{t("auth.signIn.actorLabel")}</span>
        <select
          name="actorId"
          className={controls.control}
          defaultValue={directory[0]?.actorId}
        >
          {directory.map((entry) => (
            <option key={entry.actorId} value={entry.actorId}>
              {`${entry.actorId} · ${entry.role} · ${formatNumber(entry.permissions.length)}`}
            </option>
          ))}
        </select>
      </label>

      {errorMessage === null ? null : (
        <p className={controls.invalid} role="alert">
          {errorMessage}
        </p>
      )}

      <button type="submit" className={controls.primary}>
        {t("auth.signIn.submit")}
      </button>
    </form>
  );
}
