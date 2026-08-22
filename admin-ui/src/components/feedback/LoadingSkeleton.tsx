import { t } from "@/lib/i18n";

import styles from "./LoadingSkeleton.module.css";

export interface LoadingSkeletonProps {
  /** Number of placeholder rows to draw. */
  readonly rows?: number;
  /**
   * The shape the skeleton should take: loose text rows, a framed table with a
   * header bar, or the dashboard's tile grid. A placeholder that does not
   * resemble what replaces it makes the layout jump when the data lands, which
   * is the one thing a skeleton exists to prevent.
   */
  readonly variant?: "rows" | "table" | "metrics";
}

export function LoadingSkeleton({ rows = 3, variant = "rows" }: LoadingSkeletonProps) {
  const variantClass = variant === "table" ? styles.table : variant === "metrics" ? styles.metrics : "";

  return (
    <div className={`${styles.skeleton} ${variantClass}`} role="status" aria-live="polite">
      <span className={styles.label}>{t("state.loading")}</span>
      {variant === "table" ? <span className={`${styles.row} ${styles.header}`} aria-hidden="true" /> : null}
      {Array.from({ length: rows }, (_, index) => (
        <span key={index} className={styles.row} aria-hidden="true" />
      ))}
    </div>
  );
}
