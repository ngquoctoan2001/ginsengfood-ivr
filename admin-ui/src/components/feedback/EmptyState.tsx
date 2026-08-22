import type { ReactNode } from "react";

import { t } from "@/lib/i18n";

import styles from "./EmptyState.module.css";

export interface EmptyStateProps {
  readonly title?: string;
  readonly body?: string;
  /** A way out — usually a link that clears the filter that emptied the view. */
  readonly action?: ReactNode;
  /** Drops the border, for an empty state rendered inside a DataTable frame. */
  readonly inTable?: boolean;
}

/**
 * Nothing to show — and, where the caller can say so, what to do about it.
 *
 * "No data" and "your filter excluded everything" look identical to an
 * operator, and only one of them is fixable from the screen they are on. That
 * is what `action` is for.
 */
export function EmptyState({ title, body, action, inTable }: EmptyStateProps) {
  return (
    <div className={`${styles.empty} ${inTable === true ? styles.inTable : ""}`}>
      <span className={styles.icon}>
        <EmptyGlyph />
      </span>
      <p className={styles.title}>{title ?? t("state.emptyTitle")}</p>
      <p className={styles.body}>{body ?? t("state.emptyBody")}</p>
      {action === undefined ? null : <div className={styles.action}>{action}</div>}
    </div>
  );
}

/** Decorative: the title and body carry the message. */
function EmptyGlyph() {
  return (
    <svg
      viewBox="0 0 24 24"
      width="22"
      height="22"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <rect x="3.5" y="4.5" width="17" height="15" rx="2.2" />
      <path d="M3.5 9.2h17" />
      <path d="M8 13.4h8" />
      <path d="M8 16.4h5" />
    </svg>
  );
}
