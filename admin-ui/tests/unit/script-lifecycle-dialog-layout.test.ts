import { readFileSync } from "node:fs";
import path from "node:path";

import { describe, expect, it } from "vitest";

const lifecycleActionsCss = readFileSync(
  path.resolve(
    __dirname,
    "../../src/app/(console)/config/ScriptLifecycleActions.module.css",
  ),
  "utf8",
);

describe("script lifecycle dialog layout", () => {
  it("resets the table cell nowrap inheritance before rendering dialogs", () => {
    expect(lifecycleActionsCss).toMatch(
      /\.actions\s*\{[\s\S]*?white-space:\s*normal;/,
    );
  });
});
