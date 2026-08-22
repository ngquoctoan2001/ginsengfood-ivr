import { readFileSync, readdirSync, statSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

import vi from "../../src/i18n/vi.json";
import { formatCurrencyVnd, formatDateTime, formatNumber } from "../../src/lib/i18n";
import { formatRate } from "../../src/lib/analytics/format";

const sourceRoot = path.resolve(__dirname, "../../src");

function collectSources(directory: string): string[] {
  const found: string[] = [];
  for (const entry of readdirSync(directory)) {
    const full = path.join(directory, entry);
    if (statSync(full).isDirectory()) {
      found.push(...collectSources(full));
    } else if (full.endsWith(".ts") || full.endsWith(".tsx")) {
      found.push(full);
    }
  }

  return found;
}

const sources = collectSources(sourceRoot).map((file) => ({
  file: path.relative(sourceRoot, file).replaceAll("\\", "/"),
  text: readFileSync(file, "utf8"),
}));

/**
 * W-0039 / P5-5. Vietnamese diacritics. Any string literal containing one is operator-facing
 * prose, and operator-facing prose belongs in the message catalogue.
 */
const VIETNAMESE = /[àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ]/iu;

/**
 * `pii-patterns.txt` is written for `grep -E`, which understands POSIX bracket expressions.
 * JavaScript does not, and `new RegExp("[[:space:]]")` does not fail -- it quietly compiles to a
 * character class of the literal letters. A pattern JavaScript misreads that way would make the
 * check below green on the exact input it exists to catch, so an unrecognised class throws.
 */
const POSIX_CLASSES: Readonly<Record<string, string>> = {
  "[:space:]": "\\s",
  "[:digit:]": "\\d",
  "[:alpha:]": "a-zA-Z",
  "[:alnum:]": "a-zA-Z0-9",
  "[:upper:]": "A-Z",
  "[:lower:]": "a-z",
};

function toJavaScriptRegex(source: string): string {
  return source.replaceAll(/\[:[a-z]+:\]/gu, (match) => {
    const replacement = POSIX_CLASSES[match];
    if (replacement === undefined) {
      throw new Error(`pii-patterns.txt uses POSIX class ${match}, which this check cannot read.`);
    }

    return replacement;
  });
}

describe("UI-I18N-02 the console speaks Vietnamese through one catalogue", () => {
  it("keeps every operator-facing string in vi.json rather than in a component", () => {
    const offenders: string[] = [];
    for (const source of sources) {
      // The catalogue itself, and the test that reads it, are allowed to hold the prose.
      if (source.file === "i18n/vi.json") {
        continue;
      }

      for (const line of source.text.split("\n")) {
        const trimmed = line.trim();
        if (trimmed.startsWith("//") || trimmed.startsWith("*") || trimmed.startsWith("/*")) {
          continue;
        }

        for (const literal of trimmed.match(/"[^"\n]{2,}"/gu) ?? []) {
          if (VIETNAMESE.test(literal)) {
            offenders.push(`${source.file}: ${literal}`);
          }
        }
      }
    }

    expect(offenders).toEqual([]);
  });

  /**
   * W-0107 / UT-L10N-NOENG-05. The diacritic scan above has a blind spot big
   * enough to drive the whole of W-0107 through: it only catches Vietnamese
   * prose that escaped the catalogue. English prose hardcoded into a display
   * prop trips nothing at all, which is how `aria-label="Governance"` and a
   * bare `fail-closed` survived in a console that is otherwise fully localised.
   *
   * Scope is deliberately narrow — props that end up as visible or announced
   * text, and only values that read as prose (two or more words). `label="ID"`,
   * `variant="mono"` and every testId stay legal, so the check has no false
   * positives to train people to ignore.
   */
  it("keeps English prose out of props that reach the operator", () => {
    const DISPLAY_PROP =
      /(?:aria-label|placeholder|title|caption|subtitle|body|allLabel|keyLabel)=\{?"([^"\n]+)"/gu;

    // OD-L10N-03: shared vocabulary with the runbooks, the logs and the API.
    // Translating these would cost operators the ability to search for them.
    const TECHNICAL = new Set([
      "fail-closed",
      "correlation id",
      "idempotency key",
      "kill switch",
    ]);

    const offenders: string[] = [];
    for (const source of sources) {
      if (source.file === "i18n/vi.json") {
        continue;
      }

      for (const [index, line] of source.text.split("\n").entries()) {
        const trimmed = line.trim();
        if (trimmed.startsWith("//") || trimmed.startsWith("*") || trimmed.startsWith("/*")) {
          continue;
        }

        for (const match of line.matchAll(DISPLAY_PROP)) {
          const value = match[1];
          if (!/^[\x20-\x7E]+$/u.test(value)) {
            continue; // Non-ASCII: the diacritic check above owns this case.
          }

          const words = value.trim().split(/\s+/u);
          if (words.length < 2 || TECHNICAL.has(value.trim().toLowerCase())) {
            continue;
          }

          offenders.push(`${source.file}:${index + 1}: ${match[0]}`);
        }
      }
    }

    expect(offenders).toEqual([]);
  });

  /**
   * W-0102 wrote the rule down after the PII gate stopped a capture twice: console prose that can
   * reach an evidence file must avoid the address vocabulary in `deploy/ci/pii-patterns.txt`, even
   * where the word carries another sense. What it did not leave was anything that enforces it.
   *
   * The gap is in the timing. `scan-pii.sh` runs over `docs/evidence`, so a catalogue string only
   * becomes a pipeline failure once someone has copied it into an evidence file -- which is one
   * commit too late, and lands on whoever ran the capture rather than whoever wrote the words.
   * `nav.breadcrumbLabel` carried the word for exactly that reason: never captured, never caught,
   * while the entry beside it in `enums.vi.json` was captured and turned the pipeline red. This
   * checks the catalogues directly, where the words are written.
   *
   * The offending strings are described rather than quoted, here and below, for the reason the
   * gate itself demonstrates: a file that reproduces the pattern to complain about it is a file
   * the pattern matches.
   *
   * The patterns are read from the same file CI uses. A copy here would drift, and a drifted copy
   * of a privacy gate is worse than no copy -- it would report PASS in the one case that matters.
   */
  it("keeps the address vocabulary out of both catalogues (W-0102 rule)", () => {
    const patternFile = path.resolve(__dirname, "../../../deploy/ci/pii-patterns.txt");
    const patterns = readFileSync(patternFile, "utf8")
      .split("\n")
      .map((line) => line.trim())
      .filter((line) => line !== "" && !line.startsWith("#"))
      .map((source) => ({ source, expression: new RegExp(toJavaScriptRegex(source), "u") }));

    expect(patterns.length).toBeGreaterThan(0);

    const catalogues = ["src/i18n/vi.json", "src/i18n/enums.vi.json"];
    const offenders: string[] = [];

    for (const relative of catalogues) {
      const text = readFileSync(path.resolve(__dirname, "../..", relative), "utf8");
      for (const [index, line] of text.split("\n").entries()) {
        for (const pattern of patterns) {
          if (pattern.expression.test(line)) {
            // The line is not echoed: reproducing a matched address string here would put it in a
            // file the gate also scans, and the failure would then be unfixable without deleting
            // the report of it. The location and the pattern are enough to find it.
            offenders.push(`${relative}:${index + 1} matches /${pattern.source}/`);
          }
        }
      }
    }

    expect(offenders).toEqual([]);
  });

  it("resolves every catalogue key from somewhere, including dynamic families", () => {
    // A naive scan reports the error.* family as dead because ErrorAlert builds the key from a
    // response code, and messageKey values travel as data before anything calls t(). A checker
    // that cries wolf on those gets deleted, so it has to understand both shapes.
    const allText = sources.map((source) => source.text).join("\n");
    const unused = Object.keys(vi).filter((key) => {
      if (allText.includes(`t("${key}")`)) {
        return false;
      }

      // messageKey: "action.reasonRequired" — resolved later by the caller.
      if (allText.includes(`"${key}"`)) {
        return false;
      }

      // t(`error.${code}`) — the family is reached, not the individual key.
      const family = key.split(".")[0];
      return !allText.includes(`t(\`${family}.`);
    });

    expect(unused).toEqual([]);
  });

  it("formats dates, money and rates in Vietnamese notation", () => {
    // vi-VN writes thousands with a dot and decimals with a comma. A console that shows
    // 560,000 or 95.5% to Vietnamese operators is showing numbers in someone else's notation.
    expect(formatNumber(560_000)).toContain(".");
    expect(formatNumber(560_000)).not.toContain(",");
    expect(formatCurrencyVnd(560_000)).toContain("₫");
    expect(formatRate(0.955)).toBe("95,5%");
    expect(formatRate(0.955)).not.toContain(".");

    // Pinned time zone: server and browser must agree, and ops should never have to ask which
    // clock a timestamp came from.
    expect(formatDateTime("2026-08-18T00:30:00Z")).toContain("07:30");
    expect(formatDateTime("not-a-date")).toBe("—");
  });

  it("answers a value that is not a number with an em dash, never with NaN", () => {
    // The casts are the point of the test, not a way around the compiler. `no_answer`,
    // `invalid_phone` and `technical` are required and non-nullable in the contract
    // (IvrAnalyticsTrendBucket), so the only way the trend table is handed a non-number is
    // Ivr.Api breaking its own schema — and the numbers arrive as unchecked JSON, so the
    // signature saying `number` does not make it one. The screen should go blank in that
    // cell rather than print arithmetic on nothing at whoever is on shift.
    expect(formatNumber(undefined as unknown as number)).toBe("—");
    expect(formatNumber(Number.NaN)).toBe("—");
    expect(formatNumber(null as unknown as number)).toBe("—");
    expect(formatNumber(Number.POSITIVE_INFINITY)).toBe("—");

    // The guard must not eat the ordinary path, and least of all the zero a genuinely
    // empty bucket reports — a real 0 is an answer, not a missing value.
    expect(formatNumber(0)).toBe("0");
    expect(formatNumber(560_000)).toBe("560.000");
  });
});

describe("UI-A11Y-01 structural accessibility of the console source", () => {
  it("gives every boolean cell a text alternative, not a bare glyph", () => {
    // A cell rendering only ✓ has no accessible name: a screen reader announces nothing useful,
    // and WCAG 1.4.1 is the same objection as colour-alone in a different costume.
    const offenders: string[] = [];
    for (const source of sources) {
      if (!source.file.endsWith(".tsx")) {
        continue;
      }

      for (const [index, line] of source.text.split("\n").entries()) {
        if (!line.includes("✓")) {
          continue;
        }

        // BooleanCell is the component that supplies the accessible name; it is where the
        // glyph is allowed to live, and it always pairs it with sr-only text.
        if (source.file === "components/data/BooleanCell.tsx") {
          continue;
        }

        // Acceptable when the glyph already carries a name on the same element.
        if (line.includes("aria-label") || line.includes("sr-only")) {
          continue;
        }

        offenders.push(`${source.file}:${index + 1}`);
      }
    }

    expect(offenders).toEqual([]);
  });

  it("never hardcodes a language other than Vietnamese on the document", () => {
    const layout = sources.find((source) => source.file === "app/layout.tsx");
    expect(layout).toBeDefined();
    expect(layout!.text).toContain('lang="vi"');
  });
});
