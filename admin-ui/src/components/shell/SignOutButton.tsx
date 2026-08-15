import { t } from "@/lib/i18n";

import styles from "./SignOutButton.module.css";

/**
 * Plain form post — no client JavaScript required to end a session.
 */
export function SignOutButton() {
  return (
    <form method="post" action="/api/auth/sign-out">
      <button type="submit" className={styles.button}>
        {t("auth.signOut")}
      </button>
    </form>
  );
}
