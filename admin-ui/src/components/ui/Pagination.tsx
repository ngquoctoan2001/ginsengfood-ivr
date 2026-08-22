import { formatNumber, t } from "@/lib/i18n";

import { LinkButton } from "./Button";
import styles from "./Pagination.module.css";

export interface PaginationProps {
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  /** Builds the URL for a page — the caller owns which filters travel with it. */
  readonly hrefFor: (page: number) => string;
  /** Accessible name for the navigation landmark. */
  readonly label: string;
}

/**
 * Page controls under a table.
 *
 * The range readout is the part that earns its space: with the page in the URL
 * an operator can arrive on page 7 from a shared link, and a bare
 * previous/next pair gives them no way to tell where in the set they landed.
 *
 * Both controls are links because each page is a real URL. At the ends of the
 * range they become plain spans rather than links to nowhere, so keyboard
 * focus never lands on a control that does nothing.
 */
export function Pagination({ page, pageSize, totalCount, hrefFor, label }: PaginationProps) {
  const lastPage = Math.max(1, Math.ceil(totalCount / pageSize));
  const first = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const last = Math.min(page * pageSize, totalCount);

  return (
    <nav className={styles.pager} aria-label={label}>
      {/* Range first, then the total it sits inside: "26–50 · Tổng số bản ghi:
          412". Written this way round because the operator's question is where
          am I, and the total is the context for the answer. */}
      <p className={styles.range}>
        <span className={styles.rangeStrong}>
          {`${formatNumber(first)}–${formatNumber(last)}`}
        </span>
        {` · ${t("calls.total")}: ${formatNumber(totalCount)}`}
      </p>

      <div className={styles.controls}>
        {page > 1 ? (
          <LinkButton href={hrefFor(page - 1)} variant="secondary" size="sm" icon={<ArrowLeft />}>
            {t("calls.previous")}
          </LinkButton>
        ) : (
          <span className={styles.disabled} aria-disabled="true">
            <ArrowLeft />
            {t("calls.previous")}
          </span>
        )}

        <span className={styles.position}>
          {`${t("calls.page")} ${formatNumber(page)}/${formatNumber(lastPage)}`}
        </span>

        {page < lastPage ? (
          <LinkButton href={hrefFor(page + 1)} variant="secondary" size="sm">
            {t("calls.next")}
          </LinkButton>
        ) : (
          <span className={styles.disabled} aria-disabled="true">
            {t("calls.next")}
          </span>
        )}
      </div>
    </nav>
  );
}

function ArrowLeft() {
  return (
    <svg
      viewBox="0 0 16 16"
      width="12"
      height="12"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M9.8 3.6 5.4 8l4.4 4.4" />
    </svg>
  );
}
