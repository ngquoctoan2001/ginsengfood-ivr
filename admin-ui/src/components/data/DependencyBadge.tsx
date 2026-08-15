import { t } from "@/lib/i18n";

import { StatusBadge } from "./StatusBadge";
import type { StatusTone } from "./StatusIcon";
import styles from "./DependencyBadge.module.css";

export type DependencyStateValue = "UP" | "DOWN" | "READY_503" | "NOT_WIRED";

export interface DependencyBadgeProps {
  readonly state: DependencyStateValue;
  readonly observed: boolean;
}

const TONE: Readonly<Record<DependencyStateValue, StatusTone>> = {
  UP: "success",
  DOWN: "danger",
  READY_503: "danger",
  NOT_WIRED: "neutral",
};

/**
 * Renders a dependency state.
 *
 * `DOWN` and `READY_503` are labelled fail-closed because that is what they mean
 * operationally (DO-06). `NOT_WIRED` is neither green nor red — it says only that
 * IVR has no probe yet, which is the honest answer until P6-1 (W-0040).
 */
export function DependencyBadge({ state, observed }: DependencyBadgeProps) {
  const failClosed = state === "DOWN" || state === "READY_503";

  return (
    <span className={styles.wrapper}>
      <StatusBadge tone={TONE[state]} mono testId={`state-${state}`}>
        {state}
      </StatusBadge>
      {failClosed ? (
        <span className={styles.failClosed} data-testid="fail-closed-badge">
          fail-closed
        </span>
      ) : null}
      <span className={styles.observed}>
        {observed ? t("integration.observed") : t("integration.notObserved")}
      </span>
    </span>
  );
}
