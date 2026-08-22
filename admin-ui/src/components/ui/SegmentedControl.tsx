import styles from "./SegmentedControl.module.css";

export interface SegmentOption {
  readonly value: string;
  readonly label: string;
}

export interface SegmentedControlProps {
  readonly name: string;
  /** Accessible name for the group — the question the options answer. */
  readonly label: string;
  readonly options: readonly SegmentOption[];
  readonly value: string;
}

/**
 * A small, mutually exclusive choice rendered as a pill row.
 *
 * Real radio inputs under the pills, not buttons with handlers: that keeps
 * arrow-key navigation, the radiogroup semantics and the plain-form submission
 * the console's GET filters depend on, with no client JavaScript. The visible
 * selection is drawn from `:checked`, so the control and its state cannot
 * disagree.
 *
 * Two to four options. Past that a select reads better and wraps better.
 */
export function SegmentedControl({ name, label, options, value }: SegmentedControlProps) {
  return (
    <div className={styles.group} role="radiogroup" aria-label={label}>
      {options.map((option) => (
        <label key={option.value} className={styles.option}>
          <input
            type="radio"
            className={styles.input}
            name={name}
            value={option.value}
            defaultChecked={option.value === value}
          />
          <span>{option.label}</span>
        </label>
      ))}
    </div>
  );
}
