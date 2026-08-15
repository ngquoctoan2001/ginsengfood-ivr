import { formatNumber, LOCALE } from "@/lib/i18n";

/** Rates arrive as server-computed fractions; the console only formats them. */
export function formatRate(rate: number): string {
  return `${(rate * 100).toFixed(1)}%`;
}

/** Seconds-to-final reads better as m/s than as a four-digit second count. */
export function formatDuration(seconds: number | undefined): string {
  if (seconds === undefined) {
    return "—";
  }

  const whole = Math.round(seconds);
  if (whole < 60) {
    return `${formatNumber(whole)}s`;
  }

  const minutes = Math.floor(whole / 60);
  const rest = whole % 60;
  return rest === 0 ? `${formatNumber(minutes)}m` : `${formatNumber(minutes)}m ${rest}s`;
}

/** Freshness is a lag, so it is shown as an age rather than a timestamp. */
export function formatFreshness(seconds: number | undefined): string {
  return seconds === undefined ? "—" : formatDuration(seconds);
}

/** A bucket label; the hour bucket needs the hour, the day bucket does not. */
export function formatBucketLabel(bucketStart: string, bucket: "DAY" | "HOUR"): string {
  const parsed = new Date(bucketStart);
  if (Number.isNaN(parsed.getTime())) {
    return "—";
  }

  return new Intl.DateTimeFormat(LOCALE, {
    timeZone: "UTC",
    day: "2-digit",
    month: "2-digit",
    ...(bucket === "HOUR" ? { hour: "2-digit", minute: "2-digit", hour12: false } : {}),
  }).format(parsed);
}

/**
 * Renders the server's sanitized extract as CSV.
 *
 * Two rules, both deliberate:
 *
 * - Nothing is computed here. The rows are exactly what the API returned, so a
 *   value that never left the server cannot appear in the file.
 * - A cell starting with `=`, `+`, `-` or `@` is prefixed with an apostrophe.
 *   The extract is opened in a spreadsheet, and a leading `=` there is a formula,
 *   not a label.
 */
export function toCsv(
  columns: readonly string[],
  rows: readonly (readonly string[])[],
): string {
  const lines = [columns.map(escapeCell).join(",")];
  for (const row of rows) {
    lines.push(row.map(escapeCell).join(","));
  }

  return `${lines.join("\r\n")}\r\n`;
}

function escapeCell(value: string): string {
  const neutralized = /^[=+\-@]/.test(value) ? `'${value}` : value;
  return /[",\r\n]/.test(neutralized)
    ? `"${neutralized.replaceAll('"', '""')}"`
    : neutralized;
}
