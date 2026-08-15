import { NextResponse } from "next/server";

import { relativeRedirect } from "@/lib/auth/redirect-response";
import { isSameOrigin } from "@/lib/auth/same-origin";
import { createSession } from "@/lib/auth/session";
import { applySessionCookie } from "@/lib/auth/session-cookie";
import { resolveSignIn, safeRedirectTarget } from "@/lib/auth/sign-in";
import { readConfig } from "@/lib/config/env";

/**
 * MOCK-mode sign-in endpoint.
 *
 * A Route Handler rather than a Server Action so the login form works without
 * JavaScript and so the auth flow is reachable over plain HTTP — which is what
 * `E2E-UI-AUTH-05` drives.
 *
 * Outside `IVR_EXECUTION_MODE=MOCK` this always refuses: real identities come
 * from platform SSO/JWT, gate G-AUTH (W-0006), still BLOCKED_EXTERNAL.
 */
export async function POST(request: Request): Promise<NextResponse> {
  if (!isSameOrigin(request)) {
    return NextResponse.json({ error: "cross-site request rejected" }, { status: 403 });
  }

  const formData = await request.formData();
  const requestedRedirect = String(formData.get("next") ?? "") || null;
  const outcome = resolveSignIn(
    String(formData.get("actorId") ?? ""),
    requestedRedirect,
    readConfig().isMockMode,
  );

  if (!outcome.ok) {
    const reason =
      outcome.messageKey === "auth.signIn.unavailable" ? "unavailable" : "invalidActor";
    return relativeRedirect(`/login?error=${reason}`);
  }

  return applySessionCookie(
    relativeRedirect(safeRedirectTarget(outcome.redirectTo, "/dashboard")),
    createSession(outcome.entry.actorId, outcome.entry.role, outcome.entry.permissions),
  );
}
