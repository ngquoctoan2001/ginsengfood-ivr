import Link from "next/link";
import { Suspense } from "react";

import { EnumLabel } from "@/components/data/EnumLabel";
import { ReviewReason } from "@/components/data/ReviewReason";
import { EmptyState } from "@/components/feedback/EmptyState";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import {
  Callout,
  CalloutStack,
  DataTable,
  FilterBar,
  PageHeader,
  SelectField,
  type Column,
} from "@/components/ui";
import { listReviewItems } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrReviewQueueItem, IvrReviewQueue } from "@/lib/api/types";
import { requireAdmin, requireScope } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

export const dynamic = "force-dynamic";

const STATUSES = [
  { value: "OPEN", label: "OPEN" },
  { value: "RESOLVED", label: "RESOLVED" },
];

/**
 * UI-06 review queue.
 *
 * The two actions this queue feeds — record a review, request a technical retry —
 * live on the call-detail screen, where the operator can see the evidence they
 * are deciding on. There is no resend or replay control: callback delivery is
 * owned by the outbox and its circuit breaker, not by an operator button.
 */
export default async function ReviewQueuePage({ searchParams }: PageProps<"/review">) {
  await requireAdmin();
  const params = await searchParams;
  const status = typeof params.status === "string" ? params.status : "OPEN";

  return (
    <>
      <PageHeader
        title={t("review.title")}
        subtitle={t("review.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.review") },
          ],
        }}
      />

      <FilterBar
        label={t("review.filterStatus")}
        submitLabel={t("calls.filterSubmit")}
        resetHref="/review"
      >
        <SelectField
          label={t("review.filterStatus")}
          name="status"
          defaultValue={status}
          options={STATUSES}
          includeAll
        />
      </FilterBar>

      <Suspense key={status} fallback={<LoadingSkeleton rows={5} variant="table" />}>
        <ReviewQueueTable status={status} />
      </Suspense>

      <CalloutStack>
        <Callout tone="info">{t("review.actionNotice")}</Callout>
        <Callout tone="locked" testId="no-replay-notice">
          {t("review.noReplay")}
        </Callout>
      </CalloutStack>
    </>
  );
}

async function ReviewQueueTable({ status }: { status: string }) {
  const session = await requireScope("read");
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

  return (
    <DataTable
      label={t("review.title")}
      caption={`${t("calls.total")}: ${formatNumber(queue.total_count)}`}
      columns={REVIEW_COLUMNS}
      rows={queue.items}
      rowKey={(item) => item.review_item_id}
      density="compact"
      pinFirstColumn
      empty={<EmptyState inTable />}
    />
  );
}

const REVIEW_COLUMNS: readonly Column<IvrReviewQueueItem>[] = [
  {
    key: "id",
    header: t("review.colId"),
    variant: "mono",
    cell: (item) => item.review_item_id,
  },
  {
    key: "source",
    header: t("review.colSource"),
    cell: (item) => <EnumLabel family="reviewSourceType" value={item.source_type} />,
  },
  {
    key: "order",
    header: t("review.colOrder"),
    variant: "mono",
    cell: (item) => item.order_code_short ?? "—",
  },
  {
    key: "result",
    header: t("review.colResult"),
    cell: (item) => <EnumLabel family="resultType" value={item.result_type} />,
  },
  {
    key: "reason",
    header: t("review.colReason"),
    variant: "wrap",
    cell: (item) => <ReviewReason value={item.reason} />,
  },
  {
    key: "status",
    header: t("review.colStatus"),
    cell: (item) => <EnumLabel family="reviewStatus" value={item.status} />,
  },
  {
    key: "created",
    header: t("review.colCreated"),
    cell: (item) => formatDateTime(item.created_at),
  },
  {
    key: "action",
    header: t("review.colAction"),
    cell: (item) =>
      item.ivr_call_job_id === undefined ? (
        t("review.noJob")
      ) : (
        <Link href={`/calls/${encodeURIComponent(item.ivr_call_job_id)}`}>
          {t("review.openDetail")}
        </Link>
      ),
  },
];
