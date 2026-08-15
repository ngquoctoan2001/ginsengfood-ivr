import { StatusIcon } from "./StatusIcon";
import styles from "./MetricGrid.module.css";

export interface Metric {
  readonly label: string;
  readonly value: string;
  readonly tone?: "success" | "warning" | "danger";
  readonly testId?: string;
}

export interface MetricGridProps {
  readonly metrics: readonly Metric[];
}

/**
 * Read-only figure grid. Values arrive pre-computed and pre-formatted; nothing
 * here derives a KPI from raw rows (P3-2 §9 — the API owns the arithmetic).
 *
 * A toned figure also gets a glyph, so "this number needs attention" is not
 * signalled by colour alone.
 */
export function MetricGrid({ metrics }: MetricGridProps) {
  return (
    <dl className={styles.grid}>
      {metrics.map((metric) => (
        <div key={metric.label} className={styles.metric} data-tone={metric.tone}>
          <dt>{metric.label}</dt>
          <dd data-testid={metric.testId}>
            {metric.tone === undefined ? null : <StatusIcon tone={metric.tone} />}
            <span>{metric.value}</span>
          </dd>
        </div>
      ))}
    </dl>
  );
}
