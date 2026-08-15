import { NextResponse, type NextRequest } from "next/server";

import { SESSION_COOKIE_NAME } from "@/lib/auth/session";

/**
 * Optimistic authentication gate (Next.js 16 renamed Middleware to Proxy).
 *
 * This only checks that a session cookie is *present* so an unauthenticated
 * visitor is redirected without rendering. It deliberately does not verify the
 * signature or read permissions: the real check lives next to the data, in
 * `requireSession()` and in Ivr.Api itself. A forged cookie gets past this and
 * straight into a `redirect("/login")` one layer down.
 */
/**
 * Routes that must stay reachable without a session, or sign-in could never
 * establish one.
 */
const PUBLIC_PATHS: ReadonlySet<string> = new Set([
  "/login",
  "/api/auth/sign-in",
  "/api/auth/sign-out",
]);

export function proxy(request: NextRequest): NextResponse {
  const hasSessionCookie = request.cookies.has(SESSION_COOKIE_NAME);
  const { pathname, search } = request.nextUrl;
  const isLoginRoute = pathname === "/login";

  // `nextUrl.clone()` keeps the host the browser actually used, so the redirect
  // can never shift origin and strand the SameSite=Strict session cookie.
  if (!hasSessionCookie && !PUBLIC_PATHS.has(pathname)) {
    const target = request.nextUrl.clone();
    target.pathname = "/login";
    target.search = "";
    if (pathname !== "/") {
      target.searchParams.set("next", `${pathname}${search}`);
    }

    return NextResponse.redirect(target);
  }

  if (hasSessionCookie && isLoginRoute) {
    const target = request.nextUrl.clone();
    target.pathname = "/dashboard";
    target.search = "";
    return NextResponse.redirect(target);
  }

  return NextResponse.next();
}

export const config = {
  // Everything except Next.js internals and static assets.
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp|ico)$).*)"],
};
