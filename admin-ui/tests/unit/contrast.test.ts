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
 * Reads the token values out of `globals.css`.
 *
 * The console is light-only, so there is a single `:root` block and a single
 * theme to check. The pattern requires the `--ivr-name: #hex;` form, which is
 * why hex values quoted in the file's prose are not picked up as tokens.
 */
function readTokens(): Record<string, string> {
  const tokens: Record<string, string> = {};
  for (const match of css.matchAll(/(--ivr-[a-z0-9-]+):\s*(#[0-9a-fA-F]{3,8})\s*;/g)) {
    tokens[match[1]] = match[2];
  }

  return tokens;
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

  /*
   * Brand gold. Rules 1 and 2 in globals.css confine gold to dark grounds and to
   * fills that carry dark ink, so the pairs that exist here are the pairs that
   * are legal: gold ink on light surfaces, dark ink on a gold fill, and brand
   * gold on the navy shell. There is deliberately no
   * "--ivr-brand-gold on --ivr-surface" pair and no white-on-gold pair — those
   * are 2.26:1 and must never be written.
   */
  ["gold ink on a card", "--ivr-gold-ink", "--ivr-surface"],
  ["gold ink on the page", "--ivr-gold-ink", "--ivr-surface-sunken"],
  ["label on a gold fill", "--ivr-on-gold", "--ivr-brand-gold"],
  // The primary button is gold, so its hover state carries a label too.
  ["primary button label on hover", "--ivr-accent-contrast", "--ivr-accent-hover"],
  ["shell text on the shell", "--ivr-chrome-text", "--ivr-chrome"],
  ["shell muted text on the shell", "--ivr-chrome-text-muted", "--ivr-chrome"],
  ["shell muted text on a raised shell surface", "--ivr-chrome-text-muted", "--ivr-chrome-raised"],
  ["brand gold on the shell", "--ivr-brand-gold", "--ivr-chrome"],
  ["brand gold on a raised shell surface", "--ivr-brand-gold", "--ivr-chrome-raised"],
];

/** Non-text boundaries only need AA for large text / UI components. */
const BOUNDARY_PAIRS: readonly (readonly [string, string, string])[] = [
  ["strong border on a card", "--ivr-border-strong", "--ivr-surface"],
  ["strong border on the page", "--ivr-border-strong", "--ivr-surface-sunken"],
  ["focus ring on a card", "--ivr-focus", "--ivr-surface"],
  ["focus ring on the page", "--ivr-focus", "--ivr-surface-sunken"],
  // AppShell re-points --ivr-border-strong to this inside the navy header, so a
  // bordered control placed in that band keeps a visible boundary there too.
  ["strong border on the shell", "--ivr-chrome-border-strong", "--ivr-chrome"],
  ["the gold rule on the shell", "--ivr-chrome-border", "--ivr-chrome"],
  // A chart bar is a graphical object under WCAG 1.4.11, and it sits in a track
  // painted --ivr-surface-sunken. This pair is what stops the dark-mode bar from
  // being navy-on-navy.
  ["chart fill against its track", "--ivr-data-fill", "--ivr-surface-sunken"],
];

describe("theme contrast", () => {
  const tokens = readTokens();

  /*
   * The console is pinned to light. If a dark block is ever reintroduced, these
   * pairs would silently check only the light values and a dark regression would
   * ship unnoticed — so re-adding one has to come with restoring the per-theme
   * layering this file used to do.
   */
  it("stays a single-theme stylesheet", () => {
    expect(css).not.toContain("prefers-color-scheme: dark");
    expect(css).toContain("color-scheme: light;");
  });

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
