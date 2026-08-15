import type { ReactNode } from "react";

import { StatusIcon, type StatusTone } from "./StatusIcon";
import styles from "./StatusBadge.module.css";

export interface StatusBadgeProps {
  readonly tone: StatusTone;
  readonly children: ReactNode;
  /** Renders the label in the monospace face — for codes and enum values. */
  readonly mono?: boolean;
  readonly testId?: string;
}

/**
 * The single status chip used across the console: glyph + word + colour.
 *
 * Consolidating these matters beyond looks. Before this there were four
 * different ad-hoc badge treatments, two of which signalled state with
 * background colour and nothing else — unreadable in greyscale or with
 * colour-blindness.
 */
export function StatusBadge({ tone, children, mono, testId }: StatusBadgeProps) {
  return (
    <span
      className={`${styles.badge} ${mono === true ? styles.mono : ""}`}
      data-tone={tone}
      data-testid={testId}
    >
      <StatusIcon tone={tone} />
      <span>{children}</span>
    </span>
  );
}
