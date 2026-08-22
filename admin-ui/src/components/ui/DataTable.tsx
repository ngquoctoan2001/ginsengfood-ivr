import type { ReactNode } from "react";

import styles from "./DataTable.module.css";

/** How a column's cells are typeset. */
export type ColumnVariant = "mono" | "wrap" | "numeric";

export interface Column<Row> {
  /** Stable key for the column — also the React key for its cells. */
  readonly key: string;
  readonly header: ReactNode;
  readonly cell: (row: Row) => ReactNode;
  /**
   * `mono` for identifiers and enum values, `wrap` for prose that may run long,
   * `numeric` for figures that should align on the right.
   */
  readonly variant?: ColumnVariant;
  readonly headerTestId?: string;
}

export interface DataTableProps<Row> {
  readonly columns: readonly Column<Row>[];
  readonly rows: readonly Row[];
  readonly rowKey: (row: Row) => string;
  /** Sits under the table — what the reader is looking at, or a caveat. */
  readonly caption?: ReactNode;
  /** Accessible name for the scroll region, so keyboard users know what pane they are in. */
  readonly label: string;
  /** Shown in place of the body when there are no rows. */
  readonly empty?: ReactNode;
  /**
   * Pins the first column while the rest scrolls horizontally. Worth it once a
   * table is wide enough to scroll and its first column is the row's identity.
   */
  readonly pinFirstColumn?: boolean;
  /** Bands alternate rows — helps on wide tables, noise on narrow ones. */
  readonly zebra?: boolean;
  readonly density?: "compact" | "comfortable";
  readonly testId?: string;
}

/**
 * The console's data table.
 *
 * Column-driven rather than markup-driven: six screens previously hand-rolled
 * `thead`/`tbody` around a shared stylesheet, and they had drifted into three
 * row heights and two header treatments. Passing `columns` also means the
 * header and its cells cannot fall out of step — a column is one object, not a
 * `th` in one place and a `td` twenty lines below it.
 *
 * The scroll container carries `tabIndex={0}` and a label: a pane that scrolls
 * but cannot be reached from the keyboard fails WCAG 2.1.1.
 */
export function DataTable<Row>({
  columns,
  rows,
  rowKey,
  caption,
  label,
  empty,
  pinFirstColumn,
  zebra,
  density = "comfortable",
  testId,
}: DataTableProps<Row>) {
  const tableClasses = [
    styles.table,
    density === "compact" ? styles.compact : "",
    zebra === true ? styles.zebra : "",
    pinFirstColumn === true ? styles.pinned : "",
  ]
    .filter((name) => name !== "")
    .join(" ");

  return (
    <div className={styles.frame}>
      <div className={styles.scroll} tabIndex={0} role="region" aria-label={label}>
        <table className={tableClasses} data-testid={testId}>
          {caption === undefined ? null : <caption className={styles.caption}>{caption}</caption>}
          <thead>
            <tr>
              {columns.map((column) => (
                <th key={column.key} scope="col" data-testid={column.headerTestId}>
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.length === 0 ? (
              <tr>
                <td className={styles.emptyCell} colSpan={columns.length}>
                  {empty}
                </td>
              </tr>
            ) : (
              rows.map((row) => (
                <tr key={rowKey(row)}>
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={column.variant === undefined ? undefined : styles[column.variant]}
                    >
                      {column.cell(row)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
