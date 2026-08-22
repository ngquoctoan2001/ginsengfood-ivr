import "server-only";

import { cookies } from "next/headers";
import type { NextResponse } from "next/server";
import { cache } from "react";

import { IVR_API_BASE_PATH } from "@/lib/api/client";
import { newCorrelationId } from "@/lib/api/correlation";
import { readConfig } from "@/lib/config/env";

import {
  SESSION_COOKIE_NAME,
  createSessionFromApi,
  type AdminSession,
} from "./session";

/**
 * Data Access Layer entry point: every server render, Server Action and Route
 * Handler resolves the actor through here. `cache` memoises it for the duration
 * of a single render pass so one request verifies the cookie once.
 */
export const readSession = cache(async (): Promise<AdminSession | null> => {
  const store = await cookies();
  const accessToken = store.get(SESSION_COOKIE_NAME)?.value;
  if (accessToken === undefined) {
    return null;
  }

  const correlationId = newCorrelationId();
  try {
    const response = await fetch(
      `${readConfig().apiBaseUrl}${IVR_API_BASE_PATH}/auth/session`,
      {
        headers: {
          Accept: "application/json",
          Authorization: `Bearer ${accessToken}`,
          "X-Correlation-Id": correlationId,
        },
        cache: "no-store",
        redirect: "error",
      },
    );
    if (!response.ok) {
      return null;
    }

    return createSessionFromApi(accessToken, await response.json());
  } catch {
    return null;
  }
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
    session.accessToken,
    cookieOptions(Math.max(0, session.expiresAt - Math.floor(Date.now() / 1000))),
  );

  return response;
}

export function clearSessionCookieOn(response: NextResponse): NextResponse {
  response.cookies.set(SESSION_COOKIE_NAME, "", cookieOptions(0));
  return response;
}
