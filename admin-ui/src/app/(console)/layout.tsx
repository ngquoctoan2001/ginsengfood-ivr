import type { ReactNode } from "react";

import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import { AppShell } from "@/components/shell/AppShell";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";

/**
 * Every route in this group runs behind an authenticated session. The session
 * is resolved here, once, and published to Client Components purely so the UI
 * can hide controls — Ivr.Api still authorizes each call (DF-01).
 */
export default async function ConsoleLayout({ children }: { children: ReactNode }) {
  const session = await requireSession();
  const config = readConfig();

  return (
    <PermissionProvider
      actorId={session.actorId}
      role={session.role}
      permissions={session.permissions}
    >
      <AppShell
        actorId={session.actorId}
        role={session.role}
        permissionCount={session.permissions.length}
        environmentLabel={config.environmentLabel}
        executionMode={config.executionMode}
        isMockMode={config.isMockMode}
        realCustomerCallAllowed={config.realCustomerCallAllowed}
      >
        {children}
      </AppShell>
    </PermissionProvider>
  );
}
