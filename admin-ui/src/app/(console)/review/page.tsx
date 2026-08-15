import Link from "next/link";
import { Suspense } from "react";

import { EmptyState } from "@/components/feedback/EmptyState";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { listReviewItems } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrReviewQueue } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import table from "@/components/data/DataTable.module.css";
import controls from "@/components/forms/Controls.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

/**
 * UI-06 review queue.
 *
 * The two actions this queue feeds — record a review, request a technical retry —
 * live on the call-detail screen, where the operator can see the evidence they
 * are deciding on. There is no resend or replay control: callback delivery is
 * owned by the outbox and its circuit breaker, not by an operator button.
 */
export default async function ReviewQueuePage({ searchParams }: PageProps<"/review">) {
  const params = await searchParams;
  const status = typeof params.status === "string" ? params.status : "OPEN";

  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("review.title")}</h1>
        <p className={styles.subtitle}>{t("review.subtitle")}</p>
      </header>

      <form method="get" className={controls.bar} aria-label={t("review.filterStatus")}>
        <label className={controls.field}>
          <span className={controls.label}>{t("review.filterStatus")}</span>
          <select name="status" defaultValue={status} className={controls.control}>
            <option value="OPEN">OPEN</option>
            <option value="RESOLVED">RESOLVED</option>
            <option value="">{t("dashboard.filterAll")}</option>
          </select>
        </label>
        <button type="submit" className={controls.primary}>
          {t("calls.filterSubmit")}
        </button>
      </form>

      <Suspense key={status} fallback={<LoadingSkeleton rows={5} />}>
        <ReviewQueueTable status={status} />
      </Suspense>

      <p className={styles.notice}>{t("review.actionNotice")}</p>
      <p className={styles.notice} data-testid="no-replay-notice">
        {t("review.noReplay")}
      </p>
    </>
  );
}

async function ReviewQueueTable({ status }: { status: string }) {
  const session = await requireSession();
  const config = readConfig();

  let queue: IvrReviewQueue | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    queue = (
      await listReviewItems(
        { session, config },
        { status: status === "" ? undefined : status },
      )
    ).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || queue === null) {
    return <ErrorAlert error={error!} />;
  }

  if (queue.items.length === 0) {
    return <EmptyState />;
  }

  return (
    <>
      <div className={table.scroll}>
        <table className={table.table}>
          <thead>
            <tr>
              <th scope="col">{t("review.colId")}</th>
              <th scope="col">{t("review.colSource")}</th>
              <th scope="col">{t("review.colOrder")}</th>
              <th scope="col">{t("review.colResult")}</th>
              <th scope="col">{t("review.colReason")}</th>
              <th scope="col">{t("review.colStatus")}</th>
              <th scope="col">{t("review.colCreated")}</th>
              <th scope="col">{t("review.colAction")}</th>
            </tr>
          </thead>
          <tbody>
            {queue.items.map((item) => (
              <tr key={item.review_item_id}>
                <td className={table.mono}>{item.review_item_id}</td>
                <td>{item.source_type}</td>
                <td className={table.mono}>{item.order_code_short ?? "—"}</td>
                <td>{item.result_type ?? "—"}</td>
                <td className={table.wrap}>{item.reason}</td>
                <td>{item.status}</td>
                <td>{formatDateTime(item.created_at)}</td>
                <td>
                  {item.ivr_call_job_id === undefined ? (
                    <span className={styles.muted}>{t("review.noJob")}</span>
                  ) : (
                    <Link href={`/calls/${encodeURIComponent(item.ivr_call_job_id)}`}>
                      {t("review.openDetail")}
                    </Link>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className={styles.muted}>
        {`${t("calls.total")}: ${formatNumber(queue.total_count)}`}
      </p>
    </>
  );
}
