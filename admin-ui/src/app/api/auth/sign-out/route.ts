import { NextResponse } from "next/server";

import { relativeRedirect } from "@/lib/auth/redirect-response";
import { isSameOrigin } from "@/lib/auth/same-origin";
import { clearSessionCookieOn } from "@/lib/auth/session-cookie";

export async function POST(request: Request): Promise<NextResponse> {
  if (!isSameOrigin(request)) {
    return NextResponse.json({ error: "cross-site request rejected" }, { status: 403 });
  }

  return clearSessionCookieOn(relativeRedirect("/login"));
}
