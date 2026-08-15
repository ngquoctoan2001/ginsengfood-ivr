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
] as const;

export type IvrPermission = (typeof IVR_PERMISSIONS)[number];

const PERMISSION_LOOKUP: ReadonlySet<string> = new Set(IVR_PERMISSIONS);

export function isIvrPermission(value: string): value is IvrPermission {
  return PERMISSION_LOOKUP.has(value);
}

/**
 * Roles proposed by `specs/ui/08-role-permission-ui.md` §2 and seeded in
 * `seed/agents.sample.json`. `tests/unit/rbac-directory-drift.test.ts` fails if
 * the two drift apart.
 */
export const IVR_ROLES = ["OpsViewer", "Ops", "AdminIM"] as const;

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
