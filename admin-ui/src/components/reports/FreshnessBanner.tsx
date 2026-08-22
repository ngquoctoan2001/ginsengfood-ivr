import { EnumLabel } from "@/components/data/EnumLabel";
import { Callout, CalloutStack, DescriptionList } from "@/components/ui";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";
import { formatFreshness } from "@/lib/analytics/format";
import type { IvrAnalyticsDataQuality } from "@/lib/api/types";

import styles from "./FreshnessBanner.module.css";

export interface FreshnessBannerProps {
  readonly quality: IvrAnalyticsDataQuality;
}

/**
 * States where the reporting numbers came from and how far behind they are.
 *
 * Three things this deliberately refuses to hide:
 *
 * - While `warehouse_backed` is false the figures are operational reads, not the
 *   P10-4 pipeline. Presenting them as BI output would be a claim nobody has
 *   earned yet.
 * - A warehouse that is serving is not the same as a warehouse that is caught
 *   up, so `warehouse_status` is shown alongside the source rather than folded
 *   into it. BACKLOG and MISMATCH are real but partial answers, and a reader who
 *   only saw "from the pipeline" would have no way to know that.
 * - A suppressed bucket is announced with its count. Silently dropping rows
 *   would make a filtered view look complete when it is not.
 */
export function FreshnessBanner({ quality }: FreshnessBannerProps) {
  const tone =
    quality.status === "FRESH" ? "success" : quality.status === "NO_DATA" ? "neutral" : "warning";

  return (
    <section className={styles.banner} data-tone={tone} data-testid="freshness-banner">
      <DescriptionList
        items={[
          {
            label: t("reports.freshnessStatus"),
            value: <EnumLabel family="freshnessStatus" value={quality.status} />,
            testId: "freshness-status",
          },
          {
            label: t("reports.freshnessLag"),
            value: formatFreshness(quality.freshness_seconds),
          },
          {
            label: t("reports.latestEvent"),
            value:
              quality.latest_event_at === undefined
                ? "—"
                : formatDateTime(quality.latest_event_at),
          },
          {
            label: t("reports.scannedRows"),
            value: formatNumber(quality.scanned_rows),
          },
        ]}
      />

      <CalloutStack>
        {/* The source is stated as a fact, not a warning — but an operational
            read is not the pipeline, and the tone says which one you are
            looking at without the reader having to parse the sentence. */}
        <Callout
          tone={quality.warehouse_backed ? "info" : "warning"}
          testId="analytics-source"
        >
          {quality.warehouse_backed
            ? `${t("reports.sourceWarehouse")} (${quality.source})`
            : `${t("reports.sourceOperational")} (${quality.source}, ${quality.pipeline_work_id})`}
        </Callout>

        {quality.warehouse_status === "BACKLOG" || quality.warehouse_status === "MISMATCH" ? (
          <Callout tone="warning" testId="warehouse-status">
            <EnumLabel family="warehouseStatus" value={quality.warehouse_status} />
          </Callout>
        ) : null}

        {quality.suppressed_bucket_count > 0 ? (
          <Callout tone="warning" testId="suppressed-notice">
            {`${t("reports.suppressedNotice")} ${formatNumber(
              quality.suppressed_bucket_count,
            )} (k=${formatNumber(quality.min_bucket_size)})`}
          </Callout>
        ) : null}

        {quality.truncated ? (
          <Callout tone="warning" testId="truncated-notice">
            {t("reports.truncatedNotice")}
          </Callout>
        ) : null}
      </CalloutStack>
    </section>
  );
}
