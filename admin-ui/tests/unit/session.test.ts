// @vitest-environment node
import { describe, expect, it } from "vitest";

import {
  createSession,
  isValidActorId,
  sealSession,
  SESSION_TTL_SECONDS,
  unsealSession,
} from "@/lib/auth/session";

const SECRET = "unit-test-session-secret-0123456789abcdef";
const OTHER_SECRET = "a-different-session-secret-0123456789abcd";
const NOW = 1_800_000_000;

describe("admin session cookie", () => {
  it("round-trips a sealed session", () => {
    const session = createSession("AGT-ADMIN-01", "AdminIM", ["IVR_QUEUE_VIEW"], NOW);
    const restored = unsealSession(sealSession(session, SECRET), SECRET, NOW);

    expect(restored).toEqual(session);
    expect(session.expiresAt - session.issuedAt).toBe(SESSION_TTL_SECONDS);
  });

  it("rejects a payload signed with a different key", () => {
    const token = sealSession(
      createSession("AGT-ADMIN-01", "AdminIM", ["IVR_QUEUE_VIEW"], NOW),
      OTHER_SECRET,
    );

    expect(unsealSession(token, SECRET, NOW)).toBeNull();
  });

  it("rejects a payload edited to widen permissions", () => {
    const token = sealSession(
      createSession("AGT-VIEWER-01", "OpsViewer", ["IVR_QUEUE_VIEW"], NOW),
      SECRET,
    );

    const [payload, signature] = token.split(".");
    const decoded = JSON.parse(Buffer.from(payload, "base64url").toString("utf8"));
    decoded.permissions = ["IVR_QUEUE_VIEW", "IVR_QUEUE_PAUSE", "IVR_RUNTIME_GATE_ADMIN"];
    const forged = `${Buffer.from(JSON.stringify(decoded), "utf8").toString("base64url")}.${signature}`;

    expect(unsealSession(forged, SECRET, NOW)).toBeNull();
  });

  it("rejects an expired session", () => {
    const token = sealSession(
      createSession("AGT-ADMIN-01", "AdminIM", ["IVR_QUEUE_VIEW"], NOW),
      SECRET,
    );

    expect(unsealSession(token, SECRET, NOW + SESSION_TTL_SECONDS + 1)).toBeNull();
  });

  it.each([undefined, "", "not-a-token", "a.b", "."])(
    "rejects the malformed token %j",
    (token) => {
      expect(unsealSession(token, SECRET, NOW)).toBeNull();
    },
  );

  it("rejects a session carrying an unknown role or permission", () => {
    for (const mutate of [
      (value: Record<string, unknown>) => {
        value.role = "SuperAdmin";
      },
      (value: Record<string, unknown>) => {
        value.permissions = ["IVR_MAKE_ME_ROOT"];
      },
      (value: Record<string, unknown>) => {
        value.actorId = "actor with spaces";
      },
    ]) {
      const session: Record<string, unknown> = {
        ...createSession("AGT-ADMIN-01", "AdminIM", ["IVR_QUEUE_VIEW"], NOW),
      };
      mutate(session);

      const payload = Buffer.from(JSON.stringify(session), "utf8").toString("base64url");
      const token = sealSession(session as never, SECRET);
      // Re-sign the mutated payload so only the content, not the signature, is at issue.
      const resigned = `${payload}.${token.split(".")[1]}`;

      expect(unsealSession(resigned, SECRET, NOW)).toBeNull();
    }
  });

  it("constrains actor ids to values PiiGuard accepts", () => {
    expect(isValidActorId("AGT-ADMIN-01")).toBe(true);
    expect(isValidActorId("agent.ops:01")).toBe(true);
    expect(isValidActorId("")).toBe(false);
    expect(isValidActorId("agent with space")).toBe(false);
    expect(isValidActorId("-leading-dash")).toBe(false);
    expect(isValidActorId("a".repeat(65))).toBe(false);
  });
});
