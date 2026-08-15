import "server-only";

import { cookies } from "next/headers";
import type { NextResponse } from "next/server";
import { cache } from "react";

import { readConfig, readSessionSecret } from "@/lib/config/env";

import {
  SESSION_COOKIE_NAME,
  sealSession,
  unsealSession,
  type AdminSession,
} from "./session";

/**
 * Data Access Layer entry point: every server render, Server Action and Route
 * Handler resolves the actor through here. `cache` memoises it for the duration
 * of a single render pass so one request verifies the cookie once.
 */
export const readSession = cache(async (): Promise<AdminSession | null> => {
  const store = await cookies();
  return unsealSession(store.get(SESSION_COOKIE_NAME)?.value, readSessionSecret());
});

function cookieOptions(maxAge: number) {
  return {
    httpOnly: true,
    sameSite: "strict",
    secure: readConfig().isProductionRuntime,
    path: "/",
    maxAge,
  } as const;
}

/**
 * Writes the session onto an explicit response rather than the ambient cookie
 * store, so the Set-Cookie always travels with the redirect that established it.
 */
export function applySessionCookie(
  response: NextResponse,
  session: AdminSession,
): NextResponse {
  response.cookies.set(
    SESSION_COOKIE_NAME,
    sealSession(session, readSessionSecret()),
    cookieOptions(Math.max(0, session.expiresAt - Math.floor(Date.now() / 1000))),
  );

  return response;
}

export function clearSessionCookieOn(response: NextResponse): NextResponse {
  response.cookies.set(SESSION_COOKIE_NAME, "", cookieOptions(0));
  return response;
}
