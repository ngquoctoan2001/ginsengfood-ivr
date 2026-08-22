import { NextResponse } from "next/server";

import { relativeRedirect } from "@/lib/auth/redirect-response";
import { isSameOrigin } from "@/lib/auth/same-origin";
import { createSessionFromApi } from "@/lib/auth/session";
import { applySessionCookie } from "@/lib/auth/session-cookie";
import { safeRedirectTarget } from "@/lib/auth/sign-in";
import { callIvrApi } from "@/lib/api/client";
import { IvrApiError } from "@/lib/api/errors";
import { readConfig } from "@/lib/config/env";

/**
 * A Route Handler rather than a Server Action so the login form works without
 * JavaScript and so the auth flow is reachable over plain HTTP — which is what
 * `E2E-UI-AUTH-05` drives.
 *
 * The browser submits credentials to this same-origin handler. It forwards them
 * once to Ivr.Api and stores only the returned opaque token in an httpOnly
 * cookie; the browser never receives an account directory or password hash.
 */
export async function POST(request: Request): Promise<NextResponse> {
  if (!isSameOrigin(request)) {
    return NextResponse.json({ error: "cross-site request rejected" }, { status: 403 });
  }

  const formData = await request.formData();
  const requestedRedirect = String(formData.get("next") ?? "") || null;
  try {
    const result = await callIvrApi<{
      access_token: string;
      session: unknown;
    }>({
      method: "POST",
      path: "/auth/sign-in",
      session: null,
      config: readConfig(),
      body: {
        username: String(formData.get("username") ?? ""),
        password: String(formData.get("password") ?? ""),
      },
    });
    const session = createSessionFromApi(result.data.access_token, result.data.session);
    if (session === null) {
      return relativeRedirect("/login?error=unavailable");
    }

    return applySessionCookie(
      relativeRedirect(safeRedirectTarget(requestedRedirect, "/dashboard")),
      session,
    );
  } catch (cause) {
    const error = cause instanceof IvrApiError && (cause.status === 401 || cause.status === 429)
      ? "invalidCredentials"
      : "unavailable";
    return relativeRedirect(`/login?error=${error}`);
  }
}
