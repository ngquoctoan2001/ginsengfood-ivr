import enums from "@/i18n/enums.vi.json";

/**
 * W-0107. The data dictionary: API enum value -> Vietnamese label.
 *
 * Deliberately separate from `vi.json` and from `t()`, for two reasons.
 *
 * The first is type safety. `MessageKey = keyof typeof vi` is what makes a
 * misspelled interface key a compile error instead of a runtime blank. Folding
 * enum values into the same table would force every lookup through a dynamic
 * key — `t(`enum.${family}.${value}`)` — and switch that check off everywhere,
 * which is the exact property `lib/i18n/index.ts` says the single-locale design
 * exists to keep.
 *
 * The second is ownership. `vi.json` holds interface copy, written by whoever
 * builds the screen. This file holds operational vocabulary: the words an
 * operator reads to decide whether an order is dead or still running. Those need
 * a different reviewer, and keeping them in one file would mean the coverage
 * test in `enum-coverage.test.ts` could not tell a dead interface key from an
 * untranslated enum value.
 *
 * NT-1: the label never replaces the code. `EnumLabel` renders both — the label
 * for reading, the code for filters, CSV extracts and audit cross-reference.
 */
export type EnumFamily = keyof typeof enums;

export interface EnumLabelValue {
  /** Vietnamese label, or the raw code when the dictionary has no entry. */
  readonly label: string;
  /** Always the original code. Never translated (NT-1, NT-5). */
  readonly code: string;
  /** False when the dictionary had no entry — the UI marks it (NT-4). */
  readonly known: boolean;
}

const TABLES: Readonly<Record<string, Readonly<Record<string, string>>>> = enums;

/**
 * Resolves one enum value.
 *
 * Returns `null` for an absent value so callers can render their own em dash;
 * an absent value and an untranslated one are different facts and must not
 * collapse into the same cell.
 *
 * NT-4: an unknown value resolves to the code itself with `known: false`, never
 * to a blank. Some families are genuinely open — `technical_exception_type`
 * accepts any code the SIM provider invents, and `order_state` belongs to Order
 * Core (D-02) — so "I do not have a word for this" is a state the screen has to
 * be able to say out loud.
 */
export function tEnum(
  family: EnumFamily,
  value: string | null | undefined,
): EnumLabelValue | null {
  if (value === null || value === undefined || value === "") {
    return null;
  }

  const label = TABLES[family]?.[value];
  if (label === undefined) {
    reportUntranslated(family, value);
    return { label: value, code: value, known: false };
  }

  return { label, code: value, known: true };
}

/**
 * The dropdown options for a family, in dictionary order.
 *
 * Used by the call-log filters, which until W-0107 made the operator type
 * `IVR_CONFIRMED` by hand into a monospace box and returned an unexplained empty
 * table on a typo.
 */
export function enumOptions(
  family: EnumFamily,
): readonly { readonly value: string; readonly label: string }[] {
  return Object.entries(TABLES[family]).map(([value, label]) => ({ value, label }));
}

/** Test seam: what the coverage test asserts against. */
export function enumFamilyValues(family: EnumFamily): readonly string[] {
  return Object.keys(TABLES[family]);
}

/**
 * An untranslated value is a defect in the dictionary, not in the data, so it is
 * surfaced rather than swallowed.
 *
 * Development gets a console line naming the exact family and value to add.
 * Production increments a counter instead: the operator already sees the ⚠ on
 * screen, and shipping a console warning per table row would bury the browser
 * log during an incident — precisely when the screen matters most.
 *
 * The counter is read by the observability export (§6.3) so a value that starts
 * appearing in production is visible without anyone filing a report.
 */
const untranslated = new Map<string, number>();

function reportUntranslated(family: string, value: string): void {
  const key = `${family}.${value}`;
  untranslated.set(key, (untranslated.get(key) ?? 0) + 1);

  if (process.env.NODE_ENV === "development") {
    console.warn(`[i18n] gia tri enum chua dich: ${key}`);
  }
}

/** `{ "resultType.NEW_THING": 3 }` — for the metric export and for tests. */
export function untranslatedCounts(): Readonly<Record<string, number>> {
  return Object.fromEntries(untranslated);
}

/** Test seam only. Resets the counter between cases. */
export function resetUntranslatedCounts(): void {
  untranslated.clear();
}
