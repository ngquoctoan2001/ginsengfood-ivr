import { formatBucketLabel, formatRate } from "@/lib/analytics/format";
import type { IvrAnalyticsTrendBucket } from "@/lib/api/types";
import { formatNumber, t } from "@/lib/i18n";

import styles from "./TrendChart.module.css";

export interface TrendChartProps {
  readonly buckets: readonly IvrAnalyticsTrendBucket[];
  readonly bucket: "DAY" | "HOUR";
}

const PROGRAM_TONES: Readonly<Record<string, string>> = {
  GOLDEN_HOUR: "gh",
  TWENTY_FOUR_SEVEN: "247",
};

/**
 * Confirm rate per bucket, drawn as plain markup rather than a charting
 * dependency: the console ships no third-party runtime, and a bar whose width is
 * a percentage is legible without JavaScript.
 *
 * The chart is a presentation of the table beneath it, so it carries
 * `aria-hidden` and the numbers stay in the table for assistive technology.
 */
export function TrendChart({ buckets, bucket }: TrendChartProps) {
  if (buckets.length === 0) {
    return (
      <p className={styles.empty} data-testid="trend-empty">
        {t("reports.trendEmpty")}
      </p>
    );
  }

  const peak = Math.max(...buckets.map((entry) => entry.total));

  return (
    <div className={styles.chart}>
      <ul className={styles.bars} aria-hidden="true">
        {buckets.map((entry) => (
          <li key={`${entry.bucket_start}|${entry.program}`} className={styles.bar}>
            <span className={styles.barLabel}>{formatBucketLabel(entry.bucket_start, bucket)}</span>
            <span className={styles.track}>
              <span
                className={styles.fill}
                data-program={PROGRAM_TONES[entry.program] ?? "other"}
                style={{ inlineSize: `${peak === 0 ? 0 : (entry.total / peak) * 100}%` }}
              />
            </span>
            <span className={styles.barValue}>{formatRate(entry.confirm_rate)}</span>
          </li>
        ))}
      </ul>

      <table className={styles.table}>
        <caption className={styles.caption}>{t("reports.trendCaption")}</caption>
        <thead>
          <tr>
            <th scope="col">{t("reports.colBucket")}</th>
            <th scope="col">{t("reports.colProgram")}</th>
            <th scope="col">{t("reports.colTotal")}</th>
            <th scope="col">{t("reports.colConfirmed")}</th>
            <th scope="col">{t("reports.colNoAnswer")}</th>
            <th scope="col">{t("reports.colInvalidPhone")}</th>
            <th scope="col">{t("reports.colTechnical")}</th>
            <th scope="col">{t("reports.colConfirmRate")}</th>
          </tr>
        </thead>
        <tbody>
          {buckets.map((entry) => (
            <tr key={`${entry.bucket_start}|${entry.program}|row`}>
              <td>{formatBucketLabel(entry.bucket_start, bucket)}</td>
              <td>{entry.program}</td>
              <td>{formatNumber(entry.total)}</td>
              <td>{formatNumber(entry.confirmed)}</td>
              <td>{formatNumber(entry.no_answer)}</td>
              <td>{formatNumber(entry.invalid_phone)}</td>
              <td>{formatNumber(entry.technical)}</td>
              <td>{formatRate(entry.confirm_rate)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
