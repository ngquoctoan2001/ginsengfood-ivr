import { t } from "@/lib/i18n";

import styles from "./GovernanceNotice.module.css";

export interface GovernanceNoticeProps {
  readonly realCustomerCallAllowed: boolean;
}

/**
 * The three standing constraints an operator is accountable to, restated on
 * every screen: real customer calls are off, this console never transitions an
 * order (D-02), and everything shown is masked (D-05).
 */
export function GovernanceNotice({ realCustomerCallAllowed }: GovernanceNoticeProps) {
  return (
    <aside
      className={styles.notice}
      data-real-call={realCustomerCallAllowed ? "allowed" : "blocked"}
      aria-label="Governance"
    >
      <p className={styles.line} data-testid="real-call-notice">
        {realCustomerCallAllowed
          ? t("governance.realCallOn")
          : t("governance.realCallOff")}
      </p>
      <p className={styles.line}>{t("governance.noOrderTransition")}</p>
      <p className={styles.line}>{t("governance.maskedOnly")}</p>
    </aside>
  );
}
