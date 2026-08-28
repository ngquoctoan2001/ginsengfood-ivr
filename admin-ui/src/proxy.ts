import { NextResponse, type NextRequest } from "next/server";

/**
 * W-0122. What is left of the console's request proxy after sign-in was retired.
 *
 * This used to redirect anyone without a session cookie to `/login`. There is no
 * session cookie and no `/login`: Module 3 owns operator identity, and this
 * console is now a reference implementation for that team to read rather than a
 * service anyone signs into. A redirect to a page that no longer exists would
 * only produce a loop.
 *
 * The check it performed has not disappeared, it moved to where it always
 * belonged. Every screen calls the API with one of three service credentials,
 * and the API refuses the request when the credential is missing or does not
 * cover the endpoint. That is the check that actually gates data; this one was
 * a faster hint in front of it.
 *
 * Whoever rebuilds these screens in Module 3's console should put their own
 * session check here and keep sending the tier credential onward — the API will
 * not accept the session in its place.
 */
export function proxy(_request: NextRequest): NextResponse {
  return NextResponse.next();
}
