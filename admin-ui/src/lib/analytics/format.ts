import { formatNumber, LOCALE } from "@/lib/i18n";

/**
 * W-0039 / P5-5. One decimal place, but through Intl rather than toFixed: vi-VN writes the
 * decimal separator as a comma, and a console that shows `95.5%` to Vietnamese operators is
 * showing them a number in someone else's notation.
 */
const percentFormatter = new Intl.NumberFormat(LOCALE, {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

/**
 * Rates arrive as server-computed fractions; the console only formats them.
 *
 * A missing rate renders as an em dash, never as `NaN%` and never as `0%`.
 *
 * Both halves of that matter. `operational_blocked_rate` is contractually
 * required *and* nullable (DT-06): a pre-call block produces no call result, so
 * the rate is null until an intake-block fact source exists, and showing `0%`
 * would claim no block occurred when the truth is that none was recorded.
 * `NaN%` is the other failure — it is not a number an operator can act on, and
 * a KPI tile is the last place to leak one. Guarding on `Number.isFinite`
 * rather than on null alone also covers a rate that arrives absent or
 * unparsable, which is a contract violation the screen should survive rather
 * than repeat back as arithmetic.
 *
 * This mirrors formatDuration and formatFreshness, which already answer a
 * missing value with the same em dash.
 */
export function formatRate(rate: number | null | undefined): string {
  if (rate === null || rate === undefined || !Number.isFinite(rate)) {
    return "—";
  }

  return `${percentFormatter.format(rate * 100)}%`;
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
