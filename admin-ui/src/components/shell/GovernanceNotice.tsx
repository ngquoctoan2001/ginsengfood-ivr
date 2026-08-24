import { t } from "@/lib/i18n";

import styles from "./GovernanceNotice.module.css";

export interface GovernanceNoticeProps {
  readonly realCustomerCallAllowed: boolean;
}

/** Warn operators only when real-customer calling has been enabled. */
export function GovernanceNotice({ realCustomerCallAllowed }: GovernanceNoticeProps) {
  if (!realCustomerCallAllowed) {
    return null;
  }

  return (
    <aside
      className={styles.notice}
      data-real-call="allowed"
      aria-label={t("governance.ariaLabel")}
    >
      <p className={styles.line} data-testid="real-call-notice">
        {t("governance.realCallOn")}
      </p>
    </aside>
  );
}
