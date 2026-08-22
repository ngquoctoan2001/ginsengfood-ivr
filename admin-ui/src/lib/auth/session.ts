import "server-only";

import {
  isIvrPermission,
  isIvrRole,
  type IvrPermission,
  type IvrRole,
} from "@/lib/rbac/permissions";

/**
 * Server-side projection returned by Ivr.Api for an opaque bearer session.
 * Only the raw token is stored in the httpOnly cookie; account and permission
 * claims are re-resolved from the API on every request.
 */
export interface AdminSession {
  readonly accessToken: string;
  readonly accountId: string;
  readonly actorId: string;
  readonly displayName: string;
  readonly role: IvrRole;
  readonly permissions: readonly IvrPermission[];
  readonly expiresAt: number;
}

export const SESSION_COOKIE_NAME = "ivr_admin_session";
export const SESSION_TTL_SECONDS = 8 * 60 * 60;

const ACTOR_ID_PATTERN = /^[a-z][a-z0-9._-]{2,63}$/;
const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export function isValidActorId(value: string): boolean {
  return ACTOR_ID_PATTERN.test(value);
}

/** Validate the API session payload before publishing it to the UI. */
export function createSessionFromApi(
  accessToken: string,
  payload: unknown,
  nowSeconds: number = Math.floor(Date.now() / 1000),
): AdminSession | null {
  if (accessToken.length < 32 || accessToken.length > 128) {
    return null;
  }
  if (typeof payload !== "object" || payload === null) {
    return null;
  }

  const candidate = payload as Record<string, unknown>;
  const account = candidate.account;
  if (typeof account !== "object" || account === null) {
    return null;
  }

  const accountView = account as Record<string, unknown>;
  const accountId = accountView.account_id;
  const actorId = accountView.username;
  const displayName = accountView.display_name;
  const role = accountView.role;
  const status = accountView.status;
  const permissions = candidate.permissions;
  const expiresAtValue = candidate.expires_at;
  const expiresAt = typeof expiresAtValue === "string"
    ? Math.floor(Date.parse(expiresAtValue) / 1000)
    : Number.NaN;

  if (
    typeof accountId !== "string" ||
    !UUID_PATTERN.test(accountId) ||
    typeof actorId !== "string" ||
    !isValidActorId(actorId) ||
    typeof displayName !== "string" ||
    displayName.trim() === "" ||
    typeof role !== "string" ||
    !isIvrRole(role) ||
    status !== "ACTIVE" ||
    !Array.isArray(permissions) ||
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

  return {
    accessToken,
    accountId,
    actorId,
    displayName,
    role,
    permissions: granted,
    expiresAt,
  };
}
