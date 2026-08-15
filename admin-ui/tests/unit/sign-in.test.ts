// @vitest-environment node
import { describe, expect, it } from "vitest";

import { resolveSignIn, safeRedirectTarget } from "@/lib/auth/sign-in";

describe("mock sign-in resolution", () => {
  it("resolves a seeded actor in MOCK mode", () => {
    const outcome = resolveSignIn("AGT-OPS-01", null, true);

    expect(outcome).toMatchObject({
      ok: true,
      redirectTo: "/dashboard",
      entry: { role: "Ops" },
    });
  });

  it("refuses every actor outside MOCK mode", () => {
    expect(resolveSignIn("AGT-ADMIN-01", null, false)).toEqual({
      ok: false,
      messageKey: "auth.signIn.unavailable",
    });
  });

  it("refuses an actor that is not in the directory", () => {
    expect(resolveSignIn("AGT-GHOST-99", null, true)).toEqual({
      ok: false,
      messageKey: "auth.signIn.invalidActor",
    });
  });

  it.each(["//evil.example", "/\\evil.example", "https://evil.example", "evil", ""])(
    "refuses the off-origin redirect target %j",
    (target) => {
      expect(safeRedirectTarget(target, "/dashboard")).toBe("/dashboard");
    },
  );

  it("preserves a same-origin redirect target", () => {
    expect(safeRedirectTarget("/calls?near_expiry=true", "/dashboard")).toBe("/calls?near_expiry=true");
  });
});
