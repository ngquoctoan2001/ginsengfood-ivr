/**
 * DF-01 permission vocabulary. Mirrors `IvrPermissions` in
 * `src/Ivr.Api/Auth/IvrPermissions.cs` and `specs/ui/08-role-permission-ui.md` §1.
 *
 * The client copy exists only to hide actions the actor cannot perform.
 * Authorization itself is decided server-side by Ivr.Api on every request —
 * a forged client permission changes nothing.
 */
export const IVR_PERMISSIONS = [
  "IVR_QUEUE_VIEW",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
  "IVR_SIM_ENABLE",
  "IVR_SIM_DISABLE",
  "IVR_MANUAL_RETRY",
  "IVR_RESULT_REVIEW",
  "IVR_FLAG_READ",
  "IVR_RUNTIME_GATE_ADMIN",
  "IVR_ACCOUNT_VIEW",
  "IVR_ACCOUNT_MANAGE",
  "IVR_ACCOUNT_PASSWORD_RESET",
  "IVR_ACCOUNT_SELF_VIEW",

  // W-0109 script lifecycle. All seven sit on Admin today, so hiding buttons by
  // permission is not what stops a bad approval — the server does, and it refuses
  // Content and Privacy/Legal from the same account no matter what this list says.
  "IVR_SCRIPT_EDIT",
  "IVR_SCRIPT_REVIEW",
  "IVR_SCRIPT_APPROVE_MOCK",
  "IVR_SCRIPT_APPROVE_LAB",
  "IVR_SCRIPT_APPROVE_CONTENT",
  "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
  "IVR_SCRIPT_RETIRE",

  // W-0111. On Operator as well as Admin — cutting a live call is the risk-reducing
  // direction, and it cannot start anything.
  "IVR_CALL_TERMINATE",
] as const;

export type IvrPermission = (typeof IVR_PERMISSIONS)[number];

const PERMISSION_LOOKUP: ReadonlySet<string> = new Set(IVR_PERMISSIONS);

export function isIvrPermission(value: string): value is IvrPermission {
  return PERMISSION_LOOKUP.has(value);
}

/**
 * W-0105 locks the console to exactly two roles. Ivr.Api remains the
 * authorization source; this union only validates the session projection.
 */
export const IVR_ROLES = ["Admin", "Operator"] as const;

export type IvrRole = (typeof IVR_ROLES)[number];

export function isIvrRole(value: string): value is IvrRole {
  return (IVR_ROLES as readonly string[]).includes(value);
}

export function hasPermission(
  granted: readonly string[],
  required: IvrPermission,
): boolean {
  return granted.includes(required);
}

export function hasEveryPermission(
  granted: readonly string[],
  required: readonly IvrPermission[],
): boolean {
  return required.every((permission) => hasPermission(granted, permission));
}
