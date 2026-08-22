import { EmptyState } from "@/components/feedback/EmptyState";
import { LinkButton } from "@/components/ui";
import { t } from "@/lib/i18n";

import styles from "./not-found.module.css";

export default function NotFound() {
  return (
    <div className={styles.wrapper}>
      <EmptyState
        title={t("state.notFoundTitle")}
        body={t("state.notFoundBody")}
        action={
          <LinkButton href="/dashboard" variant="primary">
            {t("nav.dashboard")}
          </LinkButton>
        }
      />
    </div>
  );
}
