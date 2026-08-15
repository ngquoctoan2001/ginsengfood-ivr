import vi from "@/i18n/vi.json";

/**
 * DTS-03: the console ships in Vietnamese only. A single locale keeps `t()`
 * fully typed — a missing or misspelled key is a compile error, not a runtime
 * fallback string.
 */
export const LOCALE = "vi-VN";
export const TIME_ZONE = "Asia/Ho_Chi_Minh";

export type MessageKey = keyof typeof vi;

export function t(key: MessageKey): string {
  return vi[key];
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

export function formatNumber(value: number): string {
  return numberFormatter.format(value);
}

export function formatCurrencyVnd(value: number): string {
  return currencyFormatter.format(value);
}
