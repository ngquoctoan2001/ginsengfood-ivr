import Link from "next/link";
import { Suspense } from "react";

import { EmptyState } from "@/components/feedback/EmptyState";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { StatusBadge } from "@/components/data/StatusBadge";
import { MaskedPhone } from "@/components/privacy/MaskedPhone";
import { listCallJobs } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrCallJobPage } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { CallLogFilters } from "./CallLogFilters";
import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

const PAGE_SIZE = 25;

interface CallLogQuery {
  readonly program: string;
  readonly status: string;
  readonly queueStatus: string;
  readonly resultType: string;
  readonly orderCode: string;
  readonly correlationId: string;
  readonly nearExpiry: boolean;
  readonly page: number;
}

export default async function CallLogPage({ searchParams }: PageProps<"/calls">) {
  const params = await searchParams;
  const query: CallLogQuery = {
    program: readParam(params.program),
    status: readParam(params.status),
    queueStatus: readParam(params.queue_status),
    resultType: readParam(params.result_type),
    orderCode: readParam(params.order_code),
    correlationId: readParam(params.correlation_id),
    nearExpiry: readParam(params.near_expiry) === "true",
    page: Math.max(1, Number.parseInt(readParam(params.page), 10) || 1),
  };

  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("calls.title")}</h1>
        <p className={styles.subtitle}>{t("calls.subtitle")}</p>
      </header>
      <CallLogFilters query={query} />
      <Suspense key={JSON.stringify(query)} fallback={<LoadingSkeleton rows={6} />}>
        <CallLogTable query={query} />
      </Suspense>
    </>
  );
}

async function CallLogTable({ query }: { query: CallLogQuery }) {
  const session = await requireSession();
  const config = readConfig();

  let page: IvrCallJobPage | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    page = (
      await listCallJobs(
        { session, config },
        {
          program: blankToUndefined(query.program),
          status: blankToUndefined(query.status),
          queueStatus: blankToUndefined(query.queueStatus),
          resultType: blankToUndefined(query.resultType),
          orderCode: blankToUndefined(query.orderCode),
          correlationId: blankToUndefined(query.correlationId),
          nearExpiry: query.nearExpiry ? true : undefined,
          page: query.page,
          pageSize: PAGE_SIZE,
        },
      )
    ).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || page === null) {
    return <ErrorAlert error={error!} />;
  }

  if (page.items.length === 0) {
    return <EmptyState />;
  }

  const lastPage = Math.max(1, Math.ceil(page.total_count / page.page_size));

  return (
    <>
      <div className={table.scroll}>
        <table className={table.table}>
          <caption className={table.caption}>{t("calls.viewOnly")}</caption>
          <thead>
            <tr>
              <th scope="col">{t("calls.colOrderCode")}</th>
              <th scope="col">{t("calls.colPhone")}</th>
              <th scope="col">{t("calls.colProgram")}</th>
              <th scope="col">{t("calls.colStatus")}</th>
              <th scope="col">{t("calls.colQueueStatus")}</th>
              <th scope="col">{t("calls.colAttempts")}</th>
              <th scope="col">{t("calls.colResult")}</th>
              <th scope="col">{t("calls.colDeadline")}</th>
              <th scope="col">{t("calls.colAction")}</th>
            </tr>
          </thead>
          <tbody>
            {page.items.map((item) => (
              <tr key={item.ivr_call_job_id}>
                <td className={table.mono}>{item.order_code_short || "—"}</td>
                <td>
                  <MaskedPhone value={item.phone_masked} />
                </td>
                <td>{item.program_type}</td>
                <td>{item.status}</td>
                <td>{item.queue_status}</td>
                <td>{`${formatNumber(item.attempt_count)}/${formatNumber(item.max_attempts)}`}</td>
                <td>{item.result_type ?? "—"}</td>
                <td>
                  {formatDateTime(item.expires_at)}
                  {item.near_expiry ? (
                    <span className={styles.badgeSlot}>
                      <StatusBadge tone="warning">{t("calls.nearExpiryBadge")}</StatusBadge>
                    </span>
                  ) : null}
                </td>
                <td>
                  <Link href={`/calls/${encodeURIComponent(item.ivr_call_job_id)}`}>
                    {t("calls.viewDetail")}
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <nav className={styles.pager} aria-label={t("calls.page")}>
        <span className={styles.muted}>
          {`${t("calls.total")}: ${formatNumber(page.total_count)} · ${t("calls.page")} ${formatNumber(page.page)}/${formatNumber(lastPage)}`}
        </span>
        {page.page > 1 ? (
          <Link href={pageHref(query, page.page - 1)}>{t("calls.previous")}</Link>
        ) : null}
        {page.page < lastPage ? (
          <Link href={pageHref(query, page.page + 1)}>{t("calls.next")}</Link>
        ) : null}
      </nav>
    </>
  );
}

function pageHref(query: CallLogQuery, page: number): string {
  const search = new URLSearchParams();
  const entries: [string, string][] = [
    ["program", query.program],
    ["status", query.status],
    ["queue_status", query.queueStatus],
    ["result_type", query.resultType],
    ["order_code", query.orderCode],
    ["correlation_id", query.correlationId],
    ["near_expiry", query.nearExpiry ? "true" : ""],
    ["page", String(page)],
  ];

  for (const [key, value] of entries) {
    if (value !== "") {
      search.set(key, value);
    }
  }

  return `/calls?${search.toString()}`;
}

function readParam(value: string | string[] | undefined): string {
  return typeof value === "string" ? value : "";
}

function blankToUndefined(value: string): string | undefined {
  return value === "" ? undefined : value;
}
