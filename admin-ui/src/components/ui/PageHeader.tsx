import type { ReactNode } from "react";

import { Breadcrumb, type Crumb } from "./Breadcrumb";
import styles from "./PageHeader.module.css";

export interface PageHeaderMeta {
  readonly label: string;
  readonly value: string;
  /** Renders the value in the monospace face — ids, correlation ids, codes. */
  readonly mono?: boolean;
  readonly testId?: string;
}

export interface PageHeaderProps {
  readonly title: string;
  readonly subtitle?: string;
  /** Small label above the title, e.g. the section a screen belongs to. */
  readonly eyebrow?: string;
  readonly breadcrumb?: {
    readonly items: readonly Crumb[];
    readonly label: string;
  };
  /** Page-level controls: an export, a refresh, a link to a sibling screen. */
  readonly actions?: ReactNode;
  /** Facts about the screen itself, such as when the figures were computed. */
  readonly meta?: readonly PageHeaderMeta[];
}

/**
 * The opening block of a screen: where you are, what this is, and what you can
 * do to it.
 *
 * There is exactly one `h1` per screen and it lives here, so the page's
 * accessible name and its visible title cannot drift apart.
 */
export function PageHeader({
  title,
  subtitle,
  eyebrow,
  breadcrumb,
  actions,
  meta,
}: PageHeaderProps) {
  return (
    <header className={styles.header}>
      {breadcrumb === undefined ? null : (
        <Breadcrumb items={breadcrumb.items} label={breadcrumb.label} />
      )}

      <div className={styles.top}>
        <div className={styles.headingBlock}>
          {eyebrow === undefined ? null : <span className={styles.eyebrow}>{eyebrow}</span>}
          <h1 className={styles.title}>{title}</h1>
          {subtitle === undefined ? null : <p className={styles.subtitle}>{subtitle}</p>}
        </div>
        {actions === undefined ? null : <div className={styles.actions}>{actions}</div>}
      </div>

      {meta === undefined || meta.length === 0 ? null : (
        <dl className={styles.meta}>
          {meta.map((item) => (
            <div key={item.label} className={styles.metaItem}>
              <dt className={styles.metaKey}>{item.label}</dt>
              <dd
                className={`${styles.metaValue} ${item.mono === true ? styles.metaMono : ""}`}
                data-testid={item.testId}
              >
                {item.value}
              </dd>
            </div>
          ))}
        </dl>
      )}
    </header>
  );
}
