import "server-only";

/**
 * W-0122. What is left of the console's authorization layer after the account
 * system was retired.
 *
 * This console is no longer a deployed service. Module 3 owns operator identity
 * and builds the console its staff actually use; what remains here is a reference
 * implementation they can read while building it, so the screens still render
 * against a running API but nobody signs in to them.
 *
 * The tiers are the real contract: a call reaches the API with one of three
 * service credentials, and the danger tier additionally carries the acting
 * operator and a reason. Whoever rebuilds these screens should carry that shape
 * across rather than the shape of this shim.
 */
export type AdminScope = "read" | "write" | "danger";

export interface AdminSession {
  readonly scope: AdminScope;
  readonly actorId: string;
  /** Display fields the shell still renders. Fixed values — nobody signs in here. */
  readonly displayName: string;
  readonly role: "admin" | "operator";
  readonly permissions: readonly string[];
}

const ALL_PERMISSIONS: readonly string[] = [
  "IVR_QUEUE_VIEW",
  "IVR_FLAG_READ",
  "IVR_RESULT_REVIEW",
  "IVR_DEV_TOOLING",
  "IVR_SCRIPT_EDIT",
  "IVR_SCRIPT_REVIEW",
  "IVR_SCRIPT_APPROVE_MOCK",
  "IVR_SCRIPT_APPROVE_LAB",
  "IVR_SCRIPT_APPROVE_CONTENT",
  "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
  "IVR_SCRIPT_RETIRE",
  "IVR_RUNTIME_GATE_ADMIN",
  "IVR_CALL_TERMINATE",
  "IVR_SIM_DISABLE",
  "IVR_SIM_ENABLE",
  "IVR_MANUAL_RETRY",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
];

const TOKENS: Record<AdminScope, string | undefined> = {
  read: process.env.IVR_ADMIN_READ_TOKEN,
  write: process.env.IVR_ADMIN_WRITE_TOKEN,
  danger: process.env.IVR_ADMIN_DANGER_TOKEN,
};

const SCOPE_HEADER_VALUE: Record<AdminScope, string> = {
  read: "ivr.admin.read",
  write: "ivr.admin.write",
  danger: "ivr.admin.danger",
};

/**
 * The service identity a screen calls the API with.
 *
 * `actorId` is a placeholder here because this console has no signed-in user.
 * In Module 3's console it must be the real operator: it is the only thing that
 * answers "who pressed this" once the audit row is written.
 */
export async function requireScope(scope: AdminScope): Promise<AdminSession> {
  return {
    scope,
    actorId: process.env.IVR_ADMIN_ACTOR_ID ?? "ivr-console-reference",
    displayName: "IVR service",
    role: scope === "read" ? "operator" : "admin",
    // Every permission: this console has no user to restrict, and a reference
    // implementation that renders half its screens teaches half the pattern.
    // Module 3's console fills this from its own role model.
    permissions: ALL_PERMISSIONS,
  };
}

/** Headers every guarded API call needs, including the danger-tier evidence. */
export function authorizationHeaders(
  session: AdminSession,
  reason?: string,
): Record<string, string> {
  const headers: Record<string, string> = {
    Authorization: `Bearer ${TOKENS[session.scope] ?? ""}`,
    "X-Service-Scope": SCOPE_HEADER_VALUE[session.scope],
    "X-Actor-Id": session.actorId,
  };
  if (session.scope === "danger" && reason !== undefined) {
    headers["X-Action-Reason"] = reason;
  }

  return headers;
}

/**
 * Screens that used to be Admin-only now need the write tier. Kept under the old
 * name so the reference implementation reads the way it did, with one place
 * showing what "admin" translated to.
 */
export async function requireAdmin(): Promise<AdminSession> {
  return requireScope("write");
}
