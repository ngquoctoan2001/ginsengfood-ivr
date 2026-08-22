import type { ReactNode } from "react";

import { formatNumber, t } from "@/lib/i18n";

import { Button, LinkButton } from "./Button";
import styles from "./FilterBar.module.css";

export interface FilterBarProps {
  /** Accessible name for the search landmark, e.g. t("calls.filterSubmit"). */
  readonly label: string;
  /**
   * Where the reset link goes — the screen's own path with no query. Omit for a
   * bar that has nothing to clear.
   */
  readonly resetHref?: string;
  /**
   * How many filters are currently narrowing the view. Rendered as a count next
   * to the actions so a stale filter cannot quietly explain a short table.
   */
  readonly activeCount?: number;
  /** Extra controls in the footer, to the left of apply — an export, a preset. */
  readonly extraActions?: ReactNode;
  readonly submitLabel?: string;
  readonly testId?: string;
  readonly children: ReactNode;
}

/**
 * The filter row above a data screen.
 *
 * A plain GET form by design: filters live in the URL, so a filtered view is
 * shareable and reload-safe, and the screen keeps working with client
 * JavaScript switched off. Nothing here holds state.
 */
export function FilterBar({
  label,
  resetHref,
  activeCount,
  extraActions,
  submitLabel,
  testId,
  children,
}: FilterBarProps) {
  const count = activeCount ?? 0;

  return (
    <search>
      <form method="get" className={styles.bar} aria-label={label} data-testid={testId}>
        <div className={styles.fields}>{children}</div>

        <div className={styles.footer}>
          {activeCount === undefined ? (
            <span />
          ) : (
            <span className={styles.summary} data-testid="filter-active-count">
              <span className={`${styles.count} ${count === 0 ? styles.countZero : ""}`}>
                {formatNumber(count)}
              </span>
              <span>{t("filter.activeCount")}</span>
            </span>
          )}

          <div className={styles.actions}>
            {extraActions}
            {resetHref === undefined ? null : (
              <LinkButton href={resetHref} variant="ghost">
                {t("dashboard.filterReset")}
              </LinkButton>
            )}
            <Button type="submit" variant="primary">
              {submitLabel ?? t("dashboard.filterApply")}
            </Button>
          </div>
        </div>
      </form>
    </search>
  );
}

/**
 * Counts the filters that are actually narrowing a view.
 *
 * Anything blank, or explicitly "all", is not a filter — it is the absence of
 * one, and counting it would make an untouched bar claim to be filtering.
 *
 * The switch on `typeof` rather than a cast is deliberate. Callers pass a query
 * object, and a query object grows fields that are not filters — a page number,
 * a sort key. Those are not counted, and more to the point they cannot crash
 * the bar: a screen must not 500 because someone added a field upstream.
 */
export function countActiveFilters(
  values: Readonly<Record<string, string | boolean | number | undefined>>,
): number {
  return Object.values(values).filter((value) => {
    if (typeof value === "boolean") {
      return value;
    }

    return typeof value === "string" && value.trim() !== "";
  }).length;
}
