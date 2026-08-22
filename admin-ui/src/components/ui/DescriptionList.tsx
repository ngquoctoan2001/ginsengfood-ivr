import type { ReactNode } from "react";

import styles from "./DescriptionList.module.css";

export interface DescriptionItem {
  readonly label: string;
  /**
   * A node, not a string, so a value can bring its own accessible name — see
   * BooleanCell, where the glyph and the word travel together.
   */
  readonly value: ReactNode;
  /** Renders the value in the monospace face: ids, codes, versions. */
  readonly mono?: boolean;
  readonly testId?: string;
}

export interface DescriptionListProps {
  readonly items: readonly DescriptionItem[];
  /**
   * `grid` fills the width with as many pairs as fit; `rows` stacks one pair per
   * line with the value flushed right, for a narrow aside.
   */
  readonly layout?: "grid" | "wide" | "rows";
}

/**
 * Label/value pairs.
 *
 * A real `dl`: these are pairs, not a matrix, and assistive technology
 * announces them as term and definition without the row/column arithmetic a
 * table would imply. Each pair is wrapped in a `div`, which is the only
 * grouping HTML allows inside a `dl` and is what makes the grid possible.
 */
export function DescriptionList({ items, layout = "grid" }: DescriptionListProps) {
  const layoutClass = layout === "wide" ? styles.two : layout === "rows" ? styles.rows : "";

  return (
    <dl className={`${styles.list} ${layoutClass}`}>
      {items.map((item) => (
        <div key={item.label} className={styles.pair}>
          <dt className={styles.term}>{item.label}</dt>
          <dd
            className={`${styles.value} ${item.mono === true ? styles.mono : ""}`}
            data-testid={item.testId}
          >
            {item.value}
          </dd>
        </div>
      ))}
    </dl>
  );
}
