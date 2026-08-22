// @vitest-environment node
import { describe, expect, it } from "vitest";

import { createSessionFromApi, isValidActorId } from "@/lib/auth/session";

const NOW = 1_800_000_000;
const TOKEN = "a".repeat(43);
const ACCOUNT_ID = "11111111-1111-4111-8111-111111111111";

function payload(overrides: Record<string, unknown> = {}): Record<string, unknown> {
  return {
    account: {
      account_id: ACCOUNT_ID,
      username: "admin",
      display_name: "Quản trị viên",
      role: "Admin",
      status: "ACTIVE",
    },
    permissions: ["IVR_QUEUE_VIEW", "IVR_ACCOUNT_SELF_VIEW"],
    expires_at: new Date((NOW + 3600) * 1000).toISOString(),
    ...overrides,
  };
}

describe("opaque API session projection", () => {
  it("accepts a currently active, known role and permission set", () => {
    expect(createSessionFromApi(TOKEN, payload(), NOW)).toEqual({
      accessToken: TOKEN,
      accountId: ACCOUNT_ID,
      actorId: "admin",
      displayName: "Quản trị viên",
      role: "Admin",
      permissions: ["IVR_QUEUE_VIEW", "IVR_ACCOUNT_SELF_VIEW"],
      expiresAt: NOW + 3600,
    });
  });

  it.each([
    ["short token", "short", payload()],
    ["expired", TOKEN, payload({ expires_at: new Date((NOW - 1) * 1000).toISOString() })],
    ["unknown permission", TOKEN, payload({ permissions: ["IVR_MAKE_ME_ROOT"] })],
    ["inactive account", TOKEN, payload({ account: { ...payload().account as object, status: "DISABLED" } })],
    ["unknown role", TOKEN, payload({ account: { ...payload().account as object, role: "SuperAdmin" } })],
  ])("rejects %s", (_name, token, value) => {
    expect(createSessionFromApi(token as string, value, NOW)).toBeNull();
  });

  it("constrains usernames to the API policy", () => {
    expect(isValidActorId("admin")).toBe(true);
    expect(isValidActorId("ngquoctoan2001")).toBe(true);
    expect(isValidActorId("AGT-ADMIN-01")).toBe(false);
    expect(isValidActorId("agent with space")).toBe(false);
    expect(isValidActorId("-leading-dash")).toBe(false);
  });
});
