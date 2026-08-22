import vi from "@/i18n/vi.json";

/**
 * DTS-03: the console ships in Vietnamese only. A single locale keeps `t()`
 * fully typed — a missing or misspelled key is a compile error, not a runtime
 * fallback string.
 */
export const LOCALE = "vi-VN";
export const TIME_ZONE = "Asia/Ho_Chi_Minh";

export type MessageKey = keyof typeof vi;

/**
 * W-0107. `params` is optional, so every existing call site keeps compiling
 * unchanged — the 70 direct callers the impact graph reports did not have to be
 * touched to add interpolation.
 *
 * A placeholder with no matching parameter is left standing as `{name}` rather
 * than replaced with an empty string. A visible `{count}` on screen says "this
 * message was built wrong"; a silent blank says nothing, and reads to whoever is
 * on shift as a value that is genuinely absent.
 *
 * No plural rules: Vietnamese does not inflect for number, so a plural layer
 * would add machinery without answering any question the language asks.
 */
export function t(
  key: MessageKey,
  params?: Readonly<Record<string, string | number>>,
): string {
  const template = vi[key];
  if (params === undefined) {
    return template;
  }

  return template.replaceAll(/\{(\w+)\}/gu, (whole, name: string) =>
    Object.hasOwn(params, name) ? String(params[name]) : whole,
  );
}

const dateTimeFormatter = new Intl.DateTimeFormat(LOCALE, {
  dateStyle: "short",
  timeStyle: "medium",
  timeZone: TIME_ZONE,
});

const numberFormatter = new Intl.NumberFormat(LOCALE);

const currencyFormatter = new Intl.NumberFormat(LOCALE, {
  style: "currency",
  currency: "VND",
  maximumFractionDigits: 0,
});

/**
 * The time zone is pinned rather than inherited from the host. Server and
 * browser must format identically or React reports a hydration mismatch, and
 * ops reading a timestamp should never have to ask which clock it came from.
 */
export function formatDateTime(value: string | Date): string {
  const parsed = typeof value === "string" ? new Date(value) : value;
  return Number.isNaN(parsed.getTime()) ? "—" : dateTimeFormatter.format(parsed);
}

/**
 * Counts and totals, in vi-VN notation.
 *
 * A value that is not a finite number renders as an em dash rather than as
 * `NaN`.
 *
 * The parameter stays `number` on purpose, and that is the difference from
 * formatRate. There, `number | null | undefined` describes the domain: a rate
 * is contractually nullable (DT-06), so a caller legitimately holds an absent
 * one. Nothing formatted here is nullable — queue counts, `no_answer`,
 * `invalid_phone`, `technical` are all required non-nullable integers — so
 * widening would state something false about the data and switch off the
 * compile-time check at every call site, turning a real "you are passing a
 * possibly-undefined value" error into a silent dash.
 *
 * The guard is for what types cannot reach instead: these numbers arrive as
 * unchecked JSON, and if Ivr.Api omits a required field the value is
 * `undefined` at runtime whatever the signature claims. An operations screen
 * should answer a broken contract with a blank cell, not by doing arithmetic on
 * nothing and printing the result at whoever is on shift.
 *
 * `Number.isFinite` rather than a null check because it does not coerce, so one
 * guard covers absent, null, NaN and Infinity. This mirrors formatDateTime
 * above, and formatRate / formatDuration in lib/analytics/format.ts.
 */
export function formatNumber(value: number): string {
  if (!Number.isFinite(value)) {
    return "—";
  }

  return numberFormatter.format(value);
}

export function formatCurrencyVnd(value: number): string {
  return currencyFormatter.format(value);
}
