import { t } from "@/lib/i18n";

import styles from "./SignOutButton.module.css";

/**
 * Plain form post — no client JavaScript required to end a session.
 *
 * A glyph rather than the word: it sits beside the account card in a 232px rail,
 * and every pixel the word would take is a pixel off the operator's own name.
 * The word is still the control's accessible name, carried by `sr-only` for a
 * screen reader and by `title` for a mouse, so nothing about it is guesswork.
 */
export function SignOutButton() {
  return (
    <form method="post" action="/api/auth/sign-out">
      <button type="submit" className={styles.button} title={t("auth.signOut")}>
        <svg
          viewBox="0 0 16 16"
          width="16"
          height="16"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
          focusable="false"
        >
          <path d="M6.4 13.4H3.8a1.2 1.2 0 0 1-1.2-1.2V3.8a1.2 1.2 0 0 1 1.2-1.2h2.6" />
          <path d="M10.2 10.9 13.1 8l-2.9-2.9" />
          <path d="M13.1 8H6.4" />
        </svg>
        <span className="sr-only">{t("auth.signOut")}</span>
      </button>
    </form>
  );
}
