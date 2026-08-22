import { Button, Callout, TextField } from "@/components/ui";
import { t } from "@/lib/i18n";

import styles from "./LoginForm.module.css";

export interface LoginFormProps {
  /** Path to return to after sign-in, already validated as same-origin. */
  readonly next: string | null;
  readonly errorMessage: string | null;
}

export function LoginForm({ next, errorMessage }: LoginFormProps) {
  return (
    <form method="post" action="/api/auth/sign-in" className={styles.form}>
      {next === null ? null : <input type="hidden" name="next" value={next} />}

      <TextField
        label={t("auth.signIn.usernameLabel")}
        name="username"
        width="full"
        autoComplete="username"
        required
        minLength={3}
        maxLength={64}
        mono
      />

      <TextField
        label={t("auth.signIn.passwordLabel")}
        name="password"
        type="password"
        width="full"
        autoComplete="current-password"
        required
        maxLength={128}
      />

      {errorMessage === null ? null : (
        <Callout tone="danger" role="alert">
          {errorMessage}
        </Callout>
      )}

      <Button type="submit" variant="primary" size="lg" block>
        {t("auth.signIn.submit")}
      </Button>
    </form>
  );
}
