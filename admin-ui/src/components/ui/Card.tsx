import type { ReactNode } from "react";

import styles from "./Card.module.css";

export interface CardProps {
  /** Section heading. Omit for a card that is purely a container. */
  readonly title?: ReactNode;
  /** One line under the title, explaining what the panel is showing. */
  readonly description?: ReactNode;
  /** Controls that act on this section — a filter, an export, an admin action. */
  readonly actions?: ReactNode;
  readonly footer?: ReactNode;
  /**
   * Draws the gold hairline along the top edge. Reserved for the one card that
   * carries a screen's headline figures; on every card it stops signalling.
   */
  readonly accent?: boolean;
  /** Removes body padding, for a card whose child draws its own edges. */
  readonly flush?: boolean;
  /**
   * The heading level for `title`. Cards usually sit under the page's `h1`, so
   * `h2` is right; nest one level deeper and pass `h3`.
   */
  readonly headingLevel?: "h2" | "h3";
  readonly testId?: string;
  readonly children: ReactNode;
}

/**
 * A titled panel.
 *
 * The heading is rendered as a real `h2`/`h3` rather than a styled `div`,
 * because the section list is how a screen-reader user navigates a dashboard
 * with nine panels on it.
 */
export function Card({
  title,
  description,
  actions,
  footer,
  accent,
  flush,
  headingLevel = "h2",
  testId,
  children,
}: CardProps) {
  const Heading = headingLevel;
  const hasHeader = title !== undefined || actions !== undefined;

  return (
    <section
      className={`${styles.card} ${accent === true ? styles.accent : ""}`}
      data-testid={testId}
    >
      {hasHeader ? (
        <div className={styles.header}>
          <div className={styles.headingBlock}>
            {title === undefined ? null : <Heading className={styles.title}>{title}</Heading>}
            {description === undefined ? null : (
              <p className={styles.description}>{description}</p>
            )}
          </div>
          {actions === undefined ? null : <div className={styles.headerActions}>{actions}</div>}
        </div>
      ) : null}

      <div className={`${styles.body} ${flush === true ? styles.flush : ""}`}>{children}</div>

      {footer === undefined ? null : <div className={styles.footer}>{footer}</div>}
    </section>
  );
}

/** A page's run of cards, at one consistent vertical rhythm. */
export function CardStack({ children }: { readonly children: ReactNode }) {
  return <div className={styles.stack}>{children}</div>;
}
