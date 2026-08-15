import "server-only";

/**
 * Reject cross-site form posts to the auth endpoints.
 *
 * The session cookie is `SameSite=Strict`, which stops a third-party page from
 * *using* an existing session — but a sign-in POST needs no cookie, so without
 * this check another origin could plant a session of its choosing. Server
 * Actions get this check from the framework; Route Handlers do not.
 */
export function isSameOrigin(request: Request): boolean {
  const secFetchSite = request.headers.get("Sec-Fetch-Site");
  if (secFetchSite !== null) {
    return secFetchSite === "same-origin" || secFetchSite === "none";
  }

  const origin = request.headers.get("Origin");
  if (origin === null) {
    // No Origin and no Sec-Fetch-Site: a non-browser client (curl, the HTTP
    // e2e suite). There is no cross-site risk to mitigate for those.
    return true;
  }

  try {
    return new URL(origin).origin === new URL(request.url).origin;
  } catch {
    return false;
  }
}
