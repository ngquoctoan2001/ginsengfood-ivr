import styles from "./Meter.module.css";

export interface MeterProps {
  readonly label: string;
  /** The already-formatted figure, e.g. "95,5%". The UI never derives it. */
  readonly value: string;
  /** The same figure as a fraction of 1, for the bar's length. */
  readonly ratio: number;
  readonly tone?: "success" | "warning" | "danger";
  readonly testId?: string;
}

/**
 * A rate, drawn as well as written.
 *
 * The number stays the primary reading and the bar is a second channel for the
 * same value — nothing here asks the reader to judge a length. The track takes
 * `role="img"` with the formatted figure as its name, so a screen reader reads
 * the rate once rather than announcing a meaningless progress element after it.
 *
 * Every figure arrives pre-computed from the analytics API (P3-4 §4); the
 * ratio is only ever used to size the fill.
 */
export function Meter({ label, value, ratio, tone, testId }: MeterProps) {
  const clamped = Math.min(1, Math.max(0, Number.isFinite(ratio) ? ratio : 0));

  return (
    <div className={styles.meter} data-tone={tone} data-testid={testId}>
      <div className={styles.head}>
        <span className={styles.label}>{label}</span>
        <span className={styles.value}>{value}</span>
      </div>
      <div className={styles.track} role="img" aria-label={`${label}: ${value}`}>
        <div className={styles.fill} style={{ width: `${clamped * 100}%` }} />
      </div>
    </div>
  );
}
