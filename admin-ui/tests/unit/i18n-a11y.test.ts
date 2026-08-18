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
