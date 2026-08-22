import { EnumLabel } from "@/components/data/EnumLabel";
import { EmptyState } from "@/components/feedback/EmptyState";
import { DataTable, type Column } from "@/components/ui";
import { formatRate } from "@/lib/analytics/format";
import type { IvrAnalyticsBreakdownRow } from "@/lib/api/types";
import { formatNumber, t } from "@/lib/i18n";
import type { EnumFamily } from "@/lib/i18n/enum";

export interface BreakdownTableProps {
  readonly caption: string;
  readonly keyLabel: string;
  readonly rows: readonly IvrAnalyticsBreakdownRow[];
  readonly testId: string;
  /**
   * Which dictionary `row.key` belongs to — it changes with the dimension being
   * broken down: result types on one panel, programs on the next.
   *
   * Absent for SCRIPT_VARIANT, whose values are script version identifiers
   * (`v3-test-approved`) rather than an enum. Rendering those through a
   * dictionary would mark every one of them untranslated, which would be the
   * warning glyph crying wolf on data that is working exactly as intended.
   */
  readonly family?: EnumFamily;
}

/**
 * Aggregate rows only. Every value is server-computed, and every row already
 * cleared the k-anonymity threshold, so there is nothing here to drill into
 * beyond the bucket itself — which is exactly the intent of D-05.
 */
export function BreakdownTable({
  caption,
  keyLabel,
  rows,
  testId,
  family,
}: BreakdownTableProps) {
  return (
    <DataTable
      label={caption}
      caption={caption}
      testId={testId}
      columns={columnsFor(keyLabel, family)}
      rows={rows}
      rowKey={(row) => row.key}
      density="compact"
      zebra
      empty={
        <div data-testid={`${testId}-empty`}>
          <EmptyState inTable body={t("reports.breakdownEmpty")} />
        </div>
      }
    />
  );
}

/**
 * Built per call because the first column's header is the dimension being
 * broken down — result type on one panel, script variant on the next.
 */
function columnsFor(
  keyLabel: string,
  family: EnumFamily | undefined,
): readonly Column<IvrAnalyticsBreakdownRow>[] {
  return [
    {
      key: "key",
      header: keyLabel,
      variant: family === undefined ? "mono" : undefined,
      cell: (row) =>
        family === undefined ? row.key : <EnumLabel family={family} value={row.key} />,
    },
    {
      key: "total",
      header: t("reports.colTotal"),
      variant: "numeric",
      cell: (row) => formatNumber(row.total),
    },
    {
      key: "confirmed",
      header: t("reports.colConfirmed"),
      variant: "numeric",
      cell: (row) => formatNumber(row.confirmed),
    },
    {
      key: "confirmRate",
      header: t("reports.colConfirmRate"),
      variant: "numeric",
      cell: (row) => formatRate(row.confirm_rate),
    },
    {
      key: "share",
      header: t("reports.colShare"),
      variant: "numeric",
      cell: (row) => formatRate(row.share),
    },
  ];
}
