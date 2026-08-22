import "server-only";

import { redirect } from "next/navigation";

import type { AdminSession } from "./session";
import { readSession } from "./session-cookie";
import { hasPermission, type IvrPermission } from "@/lib/rbac/permissions";

/**
 * Authorization checkpoint for server renders and Server Actions.
 *
 * `proxy.ts` performs the same check earlier for a faster redirect, but that is
 * an optimistic hint only — this is the check that actually gates data, and it
 * runs adjacent to every read (Next.js Data Access Layer pattern).
 */
export async function requireSession(): Promise<AdminSession> {
  const session = await readSession();
  if (session === null) {
    redirect("/login");
  }

  return session;
}

export async function requirePermission(required: IvrPermission): Promise<AdminSession> {
  const session = await requireSession();
  if (!hasPermission(session.permissions, required)) {
    redirect("/dashboard?error=forbidden");
  }

  return session;
}

export async function requireAdmin(): Promise<AdminSession> {
  const session = await requireSession();
  if (session.role !== "Admin") {
    redirect("/dashboard?error=forbidden");
  }

  return session;
}
