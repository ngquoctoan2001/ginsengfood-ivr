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
 * Two things this deliberately refuses to hide:
 *
 * - While `warehouse_backed` is false the figures are operational reads, not the
 *   P10-4 pipeline. Presenting them as BI output would be a claim nobody has
 *   earned yet.
 * - A suppressed bucket is announced with its count. Silently dropping rows
 *   would make a filtered view look complete when it is not.
 */
export function FreshnessBanner({ quality }: FreshnessBannerProps) {
  const tone =
    quality.status === "FRESH" ? "success" : quality.status === "NO_DATA" ? "neutral" : "warning";

  return (
    <section className={styles.banner} data-tone={tone} data-testid="freshness-banner">
      <dl className={styles.facts}>
        <div>
          <dt>{t("reports.freshnessStatus")}</dt>
          <dd data-testid="freshness-status">{t(`reports.freshness.${quality.status}`)}</dd>
        </div>
        <div>
          <dt>{t("reports.freshnessLag")}</dt>
          <dd>{formatFreshness(quality.freshness_seconds)}</dd>
        </div>
        <div>
          <dt>{t("reports.latestEvent")}</dt>
          <dd>
            {quality.latest_event_at === undefined
              ? "—"
              : formatDateTime(quality.latest_event_at)}
          </dd>
        </div>
        <div>
          <dt>{t("reports.scannedRows")}</dt>
          <dd>{formatNumber(quality.scanned_rows)}</dd>
        </div>
      </dl>

      <p className={styles.source} data-testid="analytics-source">
        {quality.warehouse_backed
          ? `${t("reports.sourceWarehouse")} (${quality.source})`
          : `${t("reports.sourceOperational")} (${quality.source}, ${quality.pipeline_work_id})`}
      </p>

      {quality.suppressed_bucket_count > 0 ? (
        <p className={styles.suppressed} data-testid="suppressed-notice">
          {`${t("reports.suppressedNotice")} ${formatNumber(
            quality.suppressed_bucket_count,
          )} (k=${formatNumber(quality.min_bucket_size)})`}
        </p>
      ) : null}

      {quality.truncated ? (
        <p className={styles.suppressed} data-testid="truncated-notice">
          {t("reports.truncatedNotice")}
        </p>
      ) : null}
    </section>
  );
}
