// @vitest-environment node
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

import { AA_LARGE_TEXT, AA_NORMAL_TEXT, contrastRatio } from "@/lib/design/contrast";

const css = readFileSync(
  fileURLToPath(new URL("../../src/app/globals.css", import.meta.url)),
  "utf8",
);

/**
 * Reads the token values for one theme out of `globals.css`.
 *
 * `light` is the bare `:root` block; `dark` is `:root` inside the
 * `prefers-color-scheme: dark` media query, layered over the light values the
 * way the cascade applies them.
 */
function readTheme(theme: "light" | "dark"): Record<string, string> {
  const darkStart = css.indexOf("@media (prefers-color-scheme: dark)");
  expect(darkStart).toBeGreaterThan(0);

  const lightBlock = css.slice(0, darkStart);
  const darkBlock = css.slice(darkStart, css.indexOf("\n}\n", darkStart));

  const collect = (source: string): Record<string, string> => {
    const tokens: Record<string, string> = {};
    for (const match of source.matchAll(/(--ivr-[a-z0-9-]+):\s*(#[0-9a-fA-F]{3,8})\s*;/g)) {
      tokens[match[1]] = match[2];
    }

    return tokens;
  };

  const light = collect(lightBlock);
  return theme === "light" ? light : { ...light, ...collect(darkBlock) };
}

/** Text pairs that must clear AA for normal text. */
const TEXT_PAIRS: readonly (readonly [string, string, string])[] = [
  ["body text on the page", "--ivr-text", "--ivr-surface-sunken"],
  ["body text on a card", "--ivr-text", "--ivr-surface"],
  ["body text on a raised surface", "--ivr-text", "--ivr-surface-raised"],
  ["muted text on a card", "--ivr-text-muted", "--ivr-surface"],
  ["muted text on the page", "--ivr-text-muted", "--ivr-surface-sunken"],
  ["muted text on a raised surface", "--ivr-text-muted", "--ivr-surface-raised"],
  ["subtle text on a card", "--ivr-text-subtle", "--ivr-surface"],
  ["link on a card", "--ivr-link", "--ivr-surface"],
  ["link on the page", "--ivr-link", "--ivr-surface-sunken"],
  ["primary button label", "--ivr-accent-contrast", "--ivr-accent"],
  ["success text on its surface", "--ivr-success", "--ivr-success-surface"],
  ["warning text on its surface", "--ivr-warning", "--ivr-warning-surface"],
  ["danger text on its surface", "--ivr-danger", "--ivr-danger-surface"],
  ["neutral text on its surface", "--ivr-neutral", "--ivr-neutral-surface"],
  ["success text on a card", "--ivr-success", "--ivr-surface"],
  ["warning text on a card", "--ivr-warning", "--ivr-surface"],
  ["danger text on a card", "--ivr-danger", "--ivr-surface"],
];

/** Non-text boundaries only need AA for large text / UI components. */
const BOUNDARY_PAIRS: readonly (readonly [string, string, string])[] = [
  ["strong border on a card", "--ivr-border-strong", "--ivr-surface"],
  ["focus ring on a card", "--ivr-focus", "--ivr-surface"],
  ["focus ring on the page", "--ivr-focus", "--ivr-surface-sunken"],
];

describe.each(["light", "dark"] as const)("%s theme contrast", (theme) => {
  const tokens = readTheme(theme);

  it("defines every token the pairs reference", () => {
    for (const [, foreground, background] of [...TEXT_PAIRS, ...BOUNDARY_PAIRS]) {
      expect(tokens[foreground], foreground).toBeDefined();
      expect(tokens[background], background).toBeDefined();
    }
  });

  it.each(TEXT_PAIRS)("%s clears AA for normal text", (_label, foreground, background) => {
    const ratio = contrastRatio(tokens[foreground], tokens[background]);
    expect(
      ratio,
      `${foreground} (${tokens[foreground]}) on ${background} (${tokens[background]}) = ${ratio.toFixed(2)}:1`,
    ).toBeGreaterThanOrEqual(AA_NORMAL_TEXT);
  });

  it.each(BOUNDARY_PAIRS)("%s clears AA for UI boundaries", (_label, foreground, background) => {
    const ratio = contrastRatio(tokens[foreground], tokens[background]);
    expect(
      ratio,
      `${foreground} (${tokens[foreground]}) on ${background} (${tokens[background]}) = ${ratio.toFixed(2)}:1`,
    ).toBeGreaterThanOrEqual(AA_LARGE_TEXT);
  });
});

describe("contrast helper", () => {
  it("computes the WCAG reference ratios", () => {
    expect(contrastRatio("#000000", "#ffffff")).toBeCloseTo(21, 5);
    expect(contrastRatio("#ffffff", "#ffffff")).toBeCloseTo(1, 5);
    // The skill's own example: #333 on white is roughly 12.6:1, #999 is 2.8:1.
    expect(contrastRatio("#333333", "#ffffff")).toBeGreaterThan(AA_NORMAL_TEXT);
    expect(contrastRatio("#999999", "#ffffff")).toBeLessThan(AA_NORMAL_TEXT);
  });

  it("accepts shorthand hex and rejects anything else", () => {
    expect(contrastRatio("#000", "#fff")).toBeCloseTo(21, 5);
    expect(() => contrastRatio("rebeccapurple", "#fff")).toThrow(/hex/);
  });
});
