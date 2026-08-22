import Link from "next/link";

import styles from "./Breadcrumb.module.css";

export interface Crumb {
  readonly label: string;
  /** Omitted on the last crumb — the page you are already on. */
  readonly href?: string;
}

export interface BreadcrumbProps {
  readonly items: readonly Crumb[];
  /** Accessible name for the landmark, e.g. t("nav.breadcrumbLabel"). */
  readonly label: string;
}

/**
 * The trail is a `nav` with an ordered list inside, which is what assistive
 * technology expects; the current page is marked with `aria-current` rather
 * than by being the last item, so it survives a wrapped layout.
 */
export function Breadcrumb({ items, label }: BreadcrumbProps) {
  return (
    <nav className={styles.nav} aria-label={label}>
      <ol className={styles.list}>
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          return (
            <li key={`${item.label}-${index}`} className={styles.item}>
              {item.href === undefined || isLast ? (
                <span className={styles.current} aria-current={isLast ? "page" : undefined}>
                  {item.label}
                </span>
              ) : (
                <Link href={item.href} className={styles.link}>
                  {item.label}
                </Link>
              )}
              {isLast ? null : <Separator />}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

/** Decorative: the list structure already conveys the nesting. */
function Separator() {
  return (
    <svg
      className={styles.separator}
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
      <path d="M6.2 3.6 10.6 8l-4.4 4.4" />
    </svg>
  );
}
