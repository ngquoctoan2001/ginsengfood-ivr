import { StatusBadge } from "@/components/data/StatusBadge";
import { t } from "@/lib/i18n";

import styles from "./EnvironmentBadge.module.css";

export interface EnvironmentBadgeProps {
  readonly environmentLabel: string;
  readonly executionMode: string;
  readonly isMockMode: boolean;
}

/**
 * Always-visible statement of which mode the console is driving.
 *
 * An operator must never have to guess whether an action they are about to take
 * lands in MOCK or against real telephony, so the badge is part of the header
 * rather than a page-level detail. The mode carries a glyph as well as a colour:
 * "not MOCK" is the single most consequential thing on this screen and must not
 * depend on hue alone.
 */
export function EnvironmentBadge({
  environmentLabel,
  executionMode,
  isMockMode,
}: EnvironmentBadgeProps) {
  return (
    <div className={styles.badge}>
      <span className={styles.item}>
        <span className={styles.key}>{t("governance.environment")}</span>
        <span className={styles.value}>{environmentLabel}</span>
      </span>
      <span className={styles.item}>
        <span className={styles.key}>{t("governance.executionMode")}</span>
        <StatusBadge
          tone={isMockMode ? "success" : "warning"}
          mono
          testId="execution-mode"
        >
          {executionMode}
        </StatusBadge>
      </span>
    </div>
  );
}
