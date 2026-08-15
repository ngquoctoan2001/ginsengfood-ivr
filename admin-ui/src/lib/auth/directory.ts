import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

export interface DirectoryEntry {
  readonly actorId: string;
  readonly role: IvrRole;
  readonly permissions: readonly IvrPermission[];
}

/**
 * MOCK-mode sign-in directory. Mirrors `seed/agents.sample.json`, which
 * `specs/ui/08-role-permission-ui.md` §2 designates as the role source;
 * `tests/unit/rbac-directory-drift.test.ts` fails if the two diverge.
 *
 * Deliberately absent: `IVR_FLAG_READ` and `IVR_RUNTIME_GATE_ADMIN`. Runtime
 * gate administration stays ungranted until the owner approves `OD-V1-20`
 * (specs/api/03-admin-api.md §"Runtime-gate controls"), so no seeded role can
 * reach the feature-flag mutation endpoint from this UI.
 *
 * This directory is unreachable outside `IVR_EXECUTION_MODE=MOCK`. Real
 * identities come from platform SSO/JWT, which is BLOCKED_EXTERNAL (G-AUTH,
 * W-0006) — see `signIn` in `./actions.ts`.
 */
export const MOCK_DIRECTORY: readonly DirectoryEntry[] = [
  {
    actorId: "AGT-VIEWER-01",
    role: "OpsViewer",
    permissions: ["IVR_QUEUE_VIEW"],
  },
  {
    actorId: "AGT-OPS-01",
    role: "Ops",
    permissions: ["IVR_QUEUE_VIEW", "IVR_MANUAL_RETRY", "IVR_SIM_DISABLE"],
  },
  {
    actorId: "AGT-ADMIN-01",
    role: "AdminIM",
    permissions: [
      "IVR_QUEUE_VIEW",
      "IVR_QUEUE_PAUSE",
      "IVR_QUEUE_RESUME",
      "IVR_SIM_ENABLE",
      "IVR_SIM_DISABLE",
      "IVR_RESULT_REVIEW",
    ],
  },
];

export function findDirectoryEntry(actorId: string): DirectoryEntry | undefined {
  return MOCK_DIRECTORY.find((entry) => entry.actorId === actorId);
}
