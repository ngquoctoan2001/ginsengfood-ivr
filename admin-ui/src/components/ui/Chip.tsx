import type { ReactNode } from "react";

import styles from "./Chip.module.css";

/**
 * Emphasis, not state.
 *
 * Anything that reports an outcome — up, down, blocked, approved — belongs in
 * StatusBadge, which pairs its colour with a glyph and a word (globals.css
 * rule 4). A chip carries only a name, so its tone can never be the sole
 * carrier of meaning.
 */
export type ChipTone = "neutral" | "success" | "danger" | "info" | "accent";

export interface ChipProps {
  readonly tone?: ChipTone;
  /** Sets the label in the sans face — for chips holding prose, not identifiers. */
  readonly prose?: boolean;
  readonly testId?: string;
  readonly children: ReactNode;
}

export function Chip({ tone = "neutral", prose, testId, children }: ChipProps) {
  const toneClass = tone === "neutral" ? "" : styles[tone];

  return (
    <span
      className={`${styles.chip} ${toneClass} ${prose === true ? styles.prose : ""}`}
      data-testid={testId}
    >
      {children}
    </span>
  );
}

export interface ChipListProps {
  readonly items: readonly {
    readonly key: string;
    readonly label: string;
    readonly tone?: ChipTone;
    readonly testId?: string;
  }[];
  /** Accessible name for the list, e.g. which set these tokens belong to. */
  readonly label?: string;
  readonly prose?: boolean;
}

/**
 * A wrapped run of chips as a real list, so a screen reader announces how many
 * there are before reading them out.
 */
export function ChipList({ items, label, prose }: ChipListProps) {
  return (
    <ul className={styles.list} aria-label={label}>
      {items.map((item) => (
        <li key={item.key}>
          <Chip tone={item.tone} prose={prose} testId={item.testId}>
            {item.label}
          </Chip>
        </li>
      ))}
    </ul>
  );
}
