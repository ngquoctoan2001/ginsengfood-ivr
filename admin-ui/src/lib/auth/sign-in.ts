import type { MessageKey } from "@/lib/i18n";

import { findDirectoryEntry, type DirectoryEntry } from "./directory";

export type SignInOutcome =
  | { readonly ok: true; readonly entry: DirectoryEntry; readonly redirectTo: string }
  | { readonly ok: false; readonly messageKey: MessageKey };

export const SIGN_IN_ERROR_KEYS = {
  unavailable: "auth.signIn.unavailable",
  invalidActor: "auth.signIn.invalidActor",
} as const satisfies Record<string, MessageKey>;

export type SignInErrorCode = keyof typeof SIGN_IN_ERROR_KEYS;

export function isSignInErrorCode(value: unknown): value is SignInErrorCode {
  return typeof value === "string" && value in SIGN_IN_ERROR_KEYS;
}

/**
 * Accept only same-origin, absolute-path redirect targets.
 *
 * `//evil.example` and `/\evil.example` are both read as protocol-relative URLs
 * by browsers, so a naive "starts with /" check is an open redirect.
 */
export function safeRedirectTarget(value: string | null, fallback: string): string {
  if (value === null || !value.startsWith("/") || value.startsWith("//") || value.startsWith("/\\")) {
    return fallback;
  }

  return value;
}

/**
 * Resolve a sign-in request against the MOCK directory.
 *
 * `isMockMode` is passed in rather than read here so this stays a pure decision
 * that the route handler and its tests can both exercise.
 */
export function resolveSignIn(
  actorId: string,
  requestedRedirect: string | null,
  isMockMode: boolean,
): SignInOutcome {
  if (!isMockMode) {
    return { ok: false, messageKey: SIGN_IN_ERROR_KEYS.unavailable };
  }

  const entry = findDirectoryEntry(actorId);
  if (entry === undefined) {
    return { ok: false, messageKey: SIGN_IN_ERROR_KEYS.invalidActor };
  }

  return { ok: true, entry, redirectTo: safeRedirectTarget(requestedRedirect, "/dashboard") };
}
