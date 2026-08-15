import { formatRate } from "@/lib/analytics/format";
import type { IvrAnalyticsBreakdownRow } from "@/lib/api/types";
import { formatNumber, t } from "@/lib/i18n";

import table from "@/components/data/DataTable.module.css";
import styles from "./BreakdownTable.module.css";

export interface BreakdownTableProps {
  readonly caption: string;
  readonly keyLabel: string;
  readonly rows: readonly IvrAnalyticsBreakdownRow[];
  readonly testId: string;
}

/**
 * Aggregate rows only. Every value is server-computed, and every row already
 * cleared the k-anonymity threshold, so there is nothing here to drill into
 * beyond the bucket itself — which is exactly the intent of D-05.
 */
export function BreakdownTable({ caption, keyLabel, rows, testId }: BreakdownTableProps) {
  if (rows.length === 0) {
    return (
      <p className={styles.empty} data-testid={`${testId}-empty`}>
        {t("reports.breakdownEmpty")}
      </p>
    );
  }

  return (
    <div className={table.scroll}>
      <table className={table.table} data-testid={testId}>
        <caption className={styles.caption}>{caption}</caption>
        <thead>
          <tr>
            <th scope="col">{keyLabel}</th>
            <th scope="col">{t("reports.colTotal")}</th>
            <th scope="col">{t("reports.colConfirmed")}</th>
            <th scope="col">{t("reports.colConfirmRate")}</th>
            <th scope="col">{t("reports.colShare")}</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.key}>
              <td className={table.mono}>{row.key}</td>
              <td>{formatNumber(row.total)}</td>
              <td>{formatNumber(row.confirmed)}</td>
              <td>{formatRate(row.confirm_rate)}</td>
              <td>{formatRate(row.share)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
