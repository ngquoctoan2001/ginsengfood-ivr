import { t } from "@/lib/i18n";

import styles from "./LoadingSkeleton.module.css";

export interface LoadingSkeletonProps {
  /** Number of placeholder rows to draw. */
  readonly rows?: number;
}

export function LoadingSkeleton({ rows = 3 }: LoadingSkeletonProps) {
  return (
    <div className={styles.skeleton} role="status" aria-live="polite">
      <span className={styles.label}>{t("state.loading")}</span>
      {Array.from({ length: rows }, (_, index) => (
        <span key={index} className={styles.row} aria-hidden="true" />
      ))}
    </div>
  );
}
