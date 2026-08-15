import { t } from "@/lib/i18n";
import { isMaskedPhone } from "@/lib/privacy/mask";

import styles from "./MaskedPhone.module.css";

export interface MaskedPhoneProps {
  readonly value: string | null | undefined;
}

/**
 * The only sanctioned way to render a customer phone number in this console.
 * Anything that is not already masked renders as a redaction marker — the
 * component never falls back to printing the value it was handed (D-05).
 */
export function MaskedPhone({ value }: MaskedPhoneProps) {
  if (value === null || value === undefined || value.trim() === "") {
    return <span className={styles.empty}>—</span>;
  }

  const candidate = value.trim();
  if (!isMaskedPhone(candidate)) {
    return (
      <span className={styles.redacted} title={t("privacy.redactedTitle")}>
        {t("privacy.redacted")}
      </span>
    );
  }

  return (
    <span className={styles.masked} aria-label={t("privacy.maskedPhone")}>
      {candidate}
    </span>
  );
}
