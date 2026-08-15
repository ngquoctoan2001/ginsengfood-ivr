import "server-only";

import { createHmac, timingSafeEqual } from "node:crypto";

import {
  isIvrPermission,
  isIvrRole,
  type IvrPermission,
  type IvrRole,
} from "@/lib/rbac/permissions";

/**
 * The authenticated admin actor, as carried by the httpOnly session cookie.
 *
 * `permissions` is a *cached copy* of what the identity provider granted. It
 * drives which controls the UI renders; Ivr.Api independently re-derives the
 * actor's permissions on every call (DF-01), so a tampered cookie cannot widen
 * access — and the HMAC below makes tampering detectable anyway.
 */
export interface AdminSession {
  readonly actorId: string;
  readonly role: IvrRole;
  readonly permissions: readonly IvrPermission[];
  readonly issuedAt: number;
  readonly expiresAt: number;
}

export const SESSION_COOKIE_NAME = "ivr_admin_session";
export const SESSION_TTL_SECONDS = 8 * 60 * 60;

/** `X-Actor-Id` must survive `PiiGuard.EnsureSafeText`; keep actor ids opaque. */
const ACTOR_ID_PATTERN = /^[A-Za-z0-9][A-Za-z0-9._:-]{0,63}$/;

function base64UrlEncode(value: Buffer): string {
  return value.toString("base64url");
}

function sign(payload: string, secret: string): string {
  return base64UrlEncode(createHmac("sha256", secret).update(payload).digest());
}

export function isValidActorId(value: string): boolean {
  return ACTOR_ID_PATTERN.test(value);
}

/** Serialise + HMAC-SHA256 sign a session into a cookie value. */
export function sealSession(session: AdminSession, secret: string): string {
  const payload = base64UrlEncode(Buffer.from(JSON.stringify(session), "utf8"));
  return `${payload}.${sign(payload, secret)}`;
}

/**
 * Verify and decode a cookie value. Returns `null` for anything that is not a
 * currently valid, correctly signed session — a bad signature, an expired
 * window, an unknown role and an unknown permission are all treated the same
 * way: no session. Fail-closed, never partially trusted.
 */
export function unsealSession(
  token: string | undefined,
  secret: string,
  nowSeconds: number = Math.floor(Date.now() / 1000),
): AdminSession | null {
  if (token === undefined) {
    return null;
  }

  const separator = token.lastIndexOf(".");
  if (separator <= 0) {
    return null;
  }

  const payload = token.slice(0, separator);
  const suppliedSignature = Buffer.from(token.slice(separator + 1), "utf8");
  const expectedSignature = Buffer.from(sign(payload, secret), "utf8");
  if (
    suppliedSignature.length !== expectedSignature.length ||
    !timingSafeEqual(suppliedSignature, expectedSignature)
  ) {
    return null;
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(Buffer.from(payload, "base64url").toString("utf8"));
  } catch {
    return null;
  }

  return toSession(parsed, nowSeconds);
}

function toSession(parsed: unknown, nowSeconds: number): AdminSession | null {
  if (typeof parsed !== "object" || parsed === null) {
    return null;
  }

  const candidate = parsed as Record<string, unknown>;
  const { actorId, role, permissions, issuedAt, expiresAt } = candidate;

  if (
    typeof actorId !== "string" ||
    !isValidActorId(actorId) ||
    typeof role !== "string" ||
    !isIvrRole(role) ||
    !Array.isArray(permissions) ||
    typeof issuedAt !== "number" ||
    typeof expiresAt !== "number" ||
    !Number.isFinite(issuedAt) ||
    !Number.isFinite(expiresAt) ||
    expiresAt <= nowSeconds
  ) {
    return null;
  }

  const granted: IvrPermission[] = [];
  for (const permission of permissions) {
    if (typeof permission !== "string" || !isIvrPermission(permission)) {
      return null;
    }

    granted.push(permission);
  }

  return { actorId, role, permissions: granted, issuedAt, expiresAt };
}

export function createSession(
  actorId: string,
  role: IvrRole,
  permissions: readonly IvrPermission[],
  nowSeconds: number = Math.floor(Date.now() / 1000),
): AdminSession {
  return {
    actorId,
    role,
    permissions: [...permissions],
    issuedAt: nowSeconds,
    expiresAt: nowSeconds + SESSION_TTL_SECONDS,
  };
}
