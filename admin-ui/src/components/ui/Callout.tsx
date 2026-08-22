import type { ReactNode } from "react";

import { StatusIcon } from "@/components/data/StatusIcon";

import styles from "./Callout.module.css";

/**
 * `info` states a policy, `locked` states a capability that is closed on
 * purpose, and the other four mirror the status tones.
 *
 * `locked` is not `danger`: nothing is wrong when a guard holds, and painting a
 * working guard red teaches operators to read red as noise.
 */
export type CalloutTone = "info" | "success" | "warning" | "danger" | "neutral" | "locked";

export interface CalloutProps {
  readonly tone?: CalloutTone;
  /** Optional headline. Without one the body carries the whole message. */
  readonly title?: string;
  /** Marks the callout as a live region — for a state the operator must notice. */
  readonly role?: "alert" | "status";
  readonly testId?: string;
  readonly children: ReactNode;
}

/**
 * A standing statement about the screen: a policy, a constraint, a caveat.
 *
 * Every callout carries a glyph and reads its meaning from its words, so the
 * tint is only recognition speed — colour never carries the message on its own
 * (globals.css rule 4).
 */
export function Callout({ tone = "info", title, role, testId, children }: CalloutProps) {
  return (
    <div className={styles.callout} data-tone={tone} role={role} data-testid={testId}>
      <span className={styles.icon}>
        <CalloutIcon tone={tone} />
      </span>
      <div className={styles.content}>
        {title === undefined ? null : <p className={styles.title}>{title}</p>}
        <p className={styles.body}>{children}</p>
      </div>
    </div>
  );
}

/** A run of related callouts at one consistent gap. */
export function CalloutStack({ children }: { readonly children: ReactNode }) {
  return <div className={styles.stack}>{children}</div>;
}

/**
 * The four status tones reuse StatusIcon rather than redrawing it, so a badge
 * and a callout reporting the same state show the same glyph. `info` and
 * `locked` are not operational states and are drawn here.
 */
function CalloutIcon({ tone }: { readonly tone: CalloutTone }) {
  if (tone === "success" || tone === "warning" || tone === "danger" || tone === "neutral") {
    return <StatusIcon tone={tone} />;
  }

  return (
    <svg
      viewBox="0 0 16 16"
      width="14"
      height="14"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {tone === "locked" ? (
        <>
          <rect x="3.2" y="7" width="9.6" height="6.6" rx="1.4" />
          <path d="M5.6 7V5.2a2.4 2.4 0 0 1 4.8 0V7" />
        </>
      ) : (
        <>
          <circle cx="8" cy="8" r="6.25" />
          <path d="M8 7.4v3.4M8 5.1h.01" />
        </>
      )}
    </svg>
  );
}
