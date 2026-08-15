import { t } from "@/lib/i18n";

import styles from "./EmptyState.module.css";

export interface EmptyStateProps {
  readonly title?: string;
  readonly body?: string;
}

export function EmptyState({ title, body }: EmptyStateProps) {
  return (
    <div className={styles.empty}>
      <p className={styles.title}>{title ?? t("state.emptyTitle")}</p>
      <p className={styles.body}>{body ?? t("state.emptyBody")}</p>
    </div>
  );
}
