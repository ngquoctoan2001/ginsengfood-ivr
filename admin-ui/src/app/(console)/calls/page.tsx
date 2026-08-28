import Link from "next/link";
import { Suspense } from "react";

import { EmptyState } from "@/components/feedback/EmptyState";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { EnumLabel } from "@/components/data/EnumLabel";
import { StatusBadge } from "@/components/data/StatusBadge";
import { MaskedPhone } from "@/components/privacy/MaskedPhone";
import { DataTable, LinkButton, PageHeader, Pagination, type Column } from "@/components/ui";
import { listCallJobs } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrCallJobPage, IvrCallJobListItem } from "@/lib/api/types";
import { requireScope } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { CallLogFilters } from "./CallLogFilters";

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
  readonly from: string;
  readonly to: string;
  readonly page: number;
}

export default async function CallLogPage({ searchParams }: PageProps<"/calls">) {
  await requireScope("read");
  const params = await searchParams;
  const query: CallLogQuery = {
    program: readParam(params.program),
    status: readParam(params.status),
    queueStatus: readParam(params.queue_status),
    resultType: readParam(params.result_type),
    orderCode: readParam(params.order_code),
    correlationId: readParam(params.correlation_id),
    nearExpiry: readParam(params.near_expiry) === "true",
    from: readParam(params.from),
    to: readParam(params.to),
    page: Math.max(1, Number.parseInt(readParam(params.page), 10) || 1),
  };

  return (
    <>
      <PageHeader
        title={t("calls.title")}
        subtitle={t("calls.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.callLog") },
          ],
        }}
      />
      <CallLogFilters query={query} />
      <Suspense key={JSON.stringify(query)} fallback={<LoadingSkeleton rows={6} variant="table" />}>
        <CallLogTable query={query} />
      </Suspense>
    </>
  );
}

async function CallLogTable({ query }: { query: CallLogQuery }) {
  const session = await requireScope("read");
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
          // A date input yields a day; the API takes instants, so the range
          // covers the whole day at both ends.
          from: query.from === "" ? undefined : `${query.from}T00:00:00Z`,
          to: query.to === "" ? undefined : `${query.to}T23:59:59Z`,
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

  return (
    <>
      <DataTable
        label={t("calls.title")}
        caption={t("calls.viewOnly")}
        columns={CALL_COLUMNS}
        rows={page.items}
        rowKey={(item) => item.ivr_call_job_id}
        pinFirstColumn
        zebra
        empty={
          <EmptyState
            inTable
            body={t("state.emptyFilteredBody")}
            action={
              <LinkButton href="/calls" variant="secondary" size="sm">
                {t("dashboard.filterReset")}
              </LinkButton>
            }
          />
        }
      />

      {page.items.length === 0 ? null : (
        <Pagination
          page={page.page}
          pageSize={page.page_size}
          totalCount={page.total_count}
          hrefFor={(target) => pageHref(query, target)}
          label={t("calls.page")}
        />
      )}
    </>
  );
}

const CALL_COLUMNS: readonly Column<IvrCallJobListItem>[] = [
  {
    key: "orderCode",
    header: t("calls.colOrderCode"),
    variant: "mono",
    cell: (item) => item.order_code_short || "—",
  },
  {
    key: "phone",
    header: t("calls.colPhone"),
    cell: (item) => <MaskedPhone value={item.phone_masked} />,
  },
  {
    key: "program",
    header: t("calls.colProgram"),
    cell: (item) => <EnumLabel family="programType" value={item.program_type} />,
  },
  {
    key: "status",
    header: t("calls.colStatus"),
    cell: (item) => <EnumLabel family="jobStatus" value={item.status} />,
  },
  {
    key: "queueStatus",
    header: t("calls.colQueueStatus"),
    cell: (item) => <EnumLabel family="jobStatus" value={item.queue_status} />,
  },
  {
    key: "attempts",
    header: t("calls.colAttempts"),
    variant: "numeric",
    cell: (item) => `${formatNumber(item.attempt_count)}/${formatNumber(item.max_attempts)}`,
  },
  {
    key: "result",
    header: t("calls.colResult"),
    cell: (item) => <EnumLabel family="resultType" value={item.result_type} />,
  },
  {
    key: "deadline",
    header: t("calls.colDeadline"),
    cell: (item) => (
      <>
        {formatDateTime(item.expires_at)}
        {item.near_expiry ? (
          <>
            {" "}
            <StatusBadge tone="warning">{t("calls.nearExpiryBadge")}</StatusBadge>
          </>
        ) : null}
      </>
    ),
  },
  {
    key: "action",
    header: t("calls.colAction"),
    cell: (item) => (
      <Link href={`/calls/${encodeURIComponent(item.ivr_call_job_id)}`}>
        {t("calls.viewDetail")}
      </Link>
    ),
  },
];

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
    ["from", query.from],
    ["to", query.to],
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
