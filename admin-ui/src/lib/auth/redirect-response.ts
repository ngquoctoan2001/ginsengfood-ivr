import "server-only";

import { NextResponse } from "next/server";

/**
 * 303 redirect with a *relative* Location.
 *
 * `NextResponse.redirect` requires an absolute URL, which it builds from the
 * request's Host header. When the browser's host and Next's resolved host
 * disagree — `127.0.0.1` versus `localhost`, or any reverse-proxy setup — the
 * redirect lands on a different origin and the `SameSite=Strict` session cookie
 * is left behind, so the user bounces straight back to sign-in. A relative
 * Location is resolved by the browser against the URL it actually requested, so
 * the origin can never shift.
 */
export function relativeRedirect(location: string, init?: ResponseInit): NextResponse {
  return new NextResponse(null, {
    ...init,
    status: init?.status ?? 303,
    headers: { ...init?.headers, Location: location },
  });
}
