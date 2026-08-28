/**
 * W-0122. The permission names this console used to gate on, kept only as a map
 * to the three tiers that replaced them.
 *
 * IVR no longer authorises per permission. It authorises per tier — read, write,
 * danger — because the surface has one caller and the question that matters at
 * the call site is how much damage an operation can do, not which of nineteen
 * names it was filed under.
 *
 * This map exists so screens can keep naming the operation they perform and let
 * one place decide which credential that needs. Module 3 will have its own
 * permission model; what it needs from this file is the right-hand column.
 */
import type { AdminScope } from "@/lib/auth/guard";

export type IvrPermission =
  | "IVR_QUEUE_VIEW"
  | "IVR_FLAG_READ"
  | "IVR_RESULT_REVIEW"
  | "IVR_DEV_TOOLING"
  | "IVR_SCRIPT_EDIT"
  | "IVR_SCRIPT_REVIEW"
  | "IVR_SCRIPT_APPROVE_MOCK"
  | "IVR_SCRIPT_APPROVE_LAB"
  | "IVR_SCRIPT_APPROVE_CONTENT"
  | "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL"
  | "IVR_SCRIPT_RETIRE"
  | "IVR_RUNTIME_GATE_ADMIN"
  | "IVR_CALL_TERMINATE"
  | "IVR_SIM_DISABLE"
  | "IVR_SIM_ENABLE"
  | "IVR_MANUAL_RETRY"
  | "IVR_QUEUE_PAUSE"
  | "IVR_QUEUE_RESUME";

const SCOPE_OF: Record<IvrPermission, AdminScope> = {
  IVR_QUEUE_VIEW: "read",
  IVR_FLAG_READ: "read",
  IVR_RESULT_REVIEW: "write",
  IVR_DEV_TOOLING: "write",
  IVR_SCRIPT_EDIT: "write",
  IVR_SCRIPT_REVIEW: "write",
  IVR_SCRIPT_APPROVE_MOCK: "write",
  IVR_SCRIPT_APPROVE_LAB: "write",
  IVR_SCRIPT_APPROVE_CONTENT: "write",
  IVR_SCRIPT_APPROVE_PRIVACY_LEGAL: "write",
  IVR_SCRIPT_RETIRE: "write",
  IVR_RUNTIME_GATE_ADMIN: "danger",
  IVR_CALL_TERMINATE: "danger",
  IVR_SIM_DISABLE: "danger",
  IVR_SIM_ENABLE: "danger",
  IVR_MANUAL_RETRY: "danger",
  IVR_QUEUE_PAUSE: "danger",
  IVR_QUEUE_RESUME: "danger",
};

export function scopeFor(permission: IvrPermission): AdminScope {
  return SCOPE_OF[permission];
}

/** Legacy role name, kept so old imports resolve while the console is read as a sample. */
export type IvrRole = "admin" | "operator";

/**
 * Client-side rendering gate: does the viewer hold this permission?
 *
 * The API no longer consults it — authorisation there is by credential tier — but
 * the pattern is kept because Module 3's console needs exactly this: showing a
 * viewer only the actions their own role allows. It decides what is drawn, never
 * what is permitted; the server decides that, and will refuse an action this
 * function happened to reveal.
 */
export function hasPermission(
  held: readonly string[] | undefined,
  required: IvrPermission,
): boolean {
  return held?.includes(required) ?? false;
}

/** The full list, kept so component tests can enumerate the surface. */
export const IVR_PERMISSIONS: readonly IvrPermission[] = Object.keys(
  SCOPE_OF,
) as IvrPermission[];

export const IVR_ROLES: readonly IvrRole[] = ["admin", "operator"];
