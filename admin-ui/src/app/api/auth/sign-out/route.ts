import { NextResponse } from "next/server";

import { relativeRedirect } from "@/lib/auth/redirect-response";
import { isSameOrigin } from "@/lib/auth/same-origin";
import { clearSessionCookieOn } from "@/lib/auth/session-cookie";
import { readSession } from "@/lib/auth/session-cookie";
import { callIvrApi } from "@/lib/api/client";
import { readConfig } from "@/lib/config/env";

export async function POST(request: Request): Promise<NextResponse> {
  if (!isSameOrigin(request)) {
    return NextResponse.json({ error: "cross-site request rejected" }, { status: 403 });
  }

  const session = await readSession();
  if (session !== null) {
    try {
      await callIvrApi({
        method: "POST",
        path: "/auth/sign-out",
        session,
        config: readConfig(),
      });
    } catch {
      // Local logout still clears the browser credential if Ivr.Api is down.
    }
  }

  return clearSessionCookieOn(relativeRedirect("/login"));
}
