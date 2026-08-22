import { tEnum, type EnumFamily } from "@/lib/i18n/enum";

import styles from "./EnumLabel.module.css";

export interface EnumLabelProps {
  readonly family: EnumFamily;
  readonly value: string | null | undefined;
  /**
   * Renders the raw code on its own line under the label.
   *
   * On in the detail screen, where there is width and the reader is already
   * cross-referencing evidence; off in list tables, where a second line per cell
   * would double the row height of every row to serve the minority of glances
   * that need the code. The code is still on the `title` and in the accessible
   * name either way, so nothing is lost — only moved.
   */
  readonly showCode?: boolean;
  /** Rendered when the value is absent. Absent and untranslated are not the same. */
  readonly fallback?: string;
  readonly testId?: string;
}

/**
 * One enum value, rendered as a Vietnamese label that still carries its code.
 *
 * W-0107 / NT-1. Before this, eight screens printed `IVR_NO_ANSWER_FINAL` and
 * `TASK_HELD_ADMIN_REVIEW` straight from the API into table cells. Translating
 * the value in place was not an option: the same codes are what the filters
 * match on, what the CSV extract contains, and what audit entries are
 * cross-referenced by (NT-5). So the label is added and the code is kept.
 *
 * `data-enum-code` is what E2E assertions match on. Tests that asserted on the
 * raw code keep asserting on the code rather than on Vietnamese prose, so
 * rewording the dictionary later cannot turn a green suite red.
 */
export function EnumLabel({
  family,
  value,
  showCode = false,
  fallback = "—",
  testId,
}: EnumLabelProps) {
  const resolved = tEnum(family, value);

  if (resolved === null) {
    return <>{fallback}</>;
  }

  return (
    <span
      className={styles.wrapper}
      title={resolved.code}
      data-enum-code={resolved.code}
      data-enum-known={resolved.known ? "true" : "false"}
      data-testid={testId}
    >
      <span className={styles.label}>
        {resolved.known ? null : <span aria-hidden="true">⚠ </span>}
        {resolved.label}
      </span>
      {showCode && resolved.known ? <span className={styles.code}>{resolved.code}</span> : null}
      {/*
        The code travels in the accessible name even when it is not painted. A
        screen-reader user reconciling a row against an audit entry needs it as
        much as a sighted one, and `title` alone is not reliably announced.
      */}
      <span className="sr-only"> ({resolved.code})</span>
    </span>
  );
}

/**
 * A list of enum values — `blocked_reasons`, `missing_approvals`, the approval
 * types on a script version.
 *
 * Separated so a caller cannot accidentally join the raw array with `", "` and
 * lose the per-value tooltip, which is what the detail screen did before.
 */
export function EnumLabelList({
  family,
  values,
  fallback = "—",
}: {
  readonly family: EnumFamily;
  readonly values: readonly string[];
  readonly fallback?: string;
}) {
  if (values.length === 0) {
    return <>{fallback}</>;
  }

  return (
    <span className={styles.list}>
      {values.map((value, index) => (
        <span key={value}>
          {index === 0 ? null : ", "}
          <EnumLabel family={family} value={value} />
        </span>
      ))}
    </span>
  );
}
