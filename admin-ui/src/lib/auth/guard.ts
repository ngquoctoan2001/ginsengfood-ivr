import "server-only";

import { redirect } from "next/navigation";

import type { AdminSession } from "./session";
import { readSession } from "./session-cookie";

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
