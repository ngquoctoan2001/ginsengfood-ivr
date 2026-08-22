import type { MessageKey } from "@/lib/i18n";

export const SIGN_IN_ERROR_KEYS = {
  unavailable: "auth.signIn.unavailable",
  invalidCredentials: "auth.signIn.invalidCredentials",
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
