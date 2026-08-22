// @vitest-environment node
import { describe, expect, it } from "vitest";

import { safeRedirectTarget, SIGN_IN_ERROR_KEYS } from "@/lib/auth/sign-in";

describe("credential sign-in routing", () => {
  it.each(["//evil.example", "/\\evil.example", "https://evil.example", "evil", ""])(
    "refuses the off-origin redirect target %j",
    (target) => {
      expect(safeRedirectTarget(target, "/dashboard")).toBe("/dashboard");
    },
  );

  it("preserves a same-origin redirect target", () => {
    expect(safeRedirectTarget("/calls?near_expiry=true", "/dashboard")).toBe("/calls?near_expiry=true");
  });

  it("keeps generic credential and availability failures separate", () => {
    expect(SIGN_IN_ERROR_KEYS).toEqual({
      unavailable: "auth.signIn.unavailable",
      invalidCredentials: "auth.signIn.invalidCredentials",
    });
  });
});
