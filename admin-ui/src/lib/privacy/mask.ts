/**
 * D-05 client-side guard.
 *
 * Ivr.Api already refuses to emit raw contact data, and `PiiGuard` blocks it at
 * the domain boundary. This is the last of the three layers: if a raw number
 * ever reaches a component — a contract regression, a hand-built fixture, a
 * mistyped field — the UI must refuse to paint it rather than trust upstream.
 *
 * Separators are stripped before matching. The server-side pattern only sees
 * unbroken digit runs, so `0912 341 234` slips past it; the UI has no reason to
 * inherit that blind spot.
 */

const SEPARATORS = /[\s.\-()]/g;
const RAW_MSISDN = /(?<![0-9])(?:\+?84|0)[0-9]{9}(?![0-9])/;
const MASK_CHARACTERS = /[*•xX]/;

export function looksLikeRawPhone(value: string): boolean {
  return RAW_MSISDN.test(value.replace(SEPARATORS, ""));
}

/**
 * A value is renderable as a masked phone only if it carries mask characters
 * *and* does not still contain a complete number. Both conditions matter:
 * `0912341234***` has mask characters and is still a leak.
 */
export function isMaskedPhone(value: string): boolean {
  const trimmed = value.trim();
  return (
    trimmed.length > 0 && MASK_CHARACTERS.test(trimmed) && !looksLikeRawPhone(trimmed)
  );
}
