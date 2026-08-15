"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { t } from "@/lib/i18n";

import { pauseQueueAction, resumeQueueAction } from "./actions";
import styles from "./QueueActions.module.css";

/**
 * Admin controls for the queue.
 *
 * There is no "confirm order" or "cancel order" control here, and there will
 * not be one: order state belongs to Order Core (D-02). The console can only
 * hold and release IVR's own queue.
 */
export function QueueActions() {
  return (
    <div className={styles.actions} aria-label={t("queue.actionsLabel")}>
      <AdminActionDialog
        perm="IVR_QUEUE_PAUSE"
        label={t("queue.pause")}
        description={t("queue.pauseDescription")}
        action={pauseQueueAction}
      />
      <AdminActionDialog
        perm="IVR_QUEUE_RESUME"
        label={t("queue.resume")}
        description={t("queue.resumeDescription")}
        action={resumeQueueAction}
      />
    </div>
  );
}
