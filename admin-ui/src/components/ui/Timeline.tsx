import type { ReactNode } from "react";

import type { StatusTone } from "@/components/data/StatusIcon";

import styles from "./Timeline.module.css";

export interface TimelineProps {
  readonly children: ReactNode;
}

/**
 * An ordered run of events.
 *
 * An `ol`, because "attempt 2 came after attempt 1" is a fact about the data
 * rather than a layout choice: assistive technology announces the position in
 * the sequence, which three stacked panels would not.
 */
export function Timeline({ children }: TimelineProps) {
  return <ol className={styles.timeline}>{children}</ol>;
}

export interface TimelineItemProps {
  /** The event's headline — what happened, and when or in what order. */
  readonly title: ReactNode;
  /** Secondary line beside the title: a timestamp, a policy version. */
  readonly meta?: ReactNode;
  /**
   * Tints the node on the rail so a failed attempt can be found by scanning.
   * The written status in the title is what actually reports the state.
   */
  readonly tone?: StatusTone;
  readonly children?: ReactNode;
}

export function TimelineItem({ title, meta, tone, children }: TimelineItemProps) {
  return (
    <li className={styles.item} data-tone={tone}>
      <p className={styles.head}>
        {title}
        {meta === undefined ? null : <span className={styles.meta}>{meta}</span>}
      </p>
      {children}
    </li>
  );
}
