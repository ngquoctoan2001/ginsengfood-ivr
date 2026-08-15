export const CORRELATION_HEADER = "X-Correlation-Id";
export const IDEMPOTENCY_HEADER = "Idempotency-Key";
export const ACTOR_HEADER = "X-Actor-Id";

const GROUP_COUNT = 6;
const GROUP_SIZE = 4;

/**
 * Trace identifiers accepted by `InternalRequestGuard` in Ivr.Api: at most 128
 * characters, `[A-Za-z0-9-_.:]` only, and `PiiGuard.IsSafeText` clean.
 *
 * The hex payload is emitted in four-character groups on purpose. PiiGuard
 * rejects any value containing a ten-digit run that reads as a Vietnamese MSISDN
 * (`0`/`84`/`+84` + nine digits, D-05), and an ungrouped random hex string hits
 * that pattern often enough to flake in production. Four-digit groups cannot.
 */
export function generateTraceId(prefix: string): string {
  const bytes = new Uint8Array(GROUP_COUNT * GROUP_SIZE / 2);
  crypto.getRandomValues(bytes);

  const hex = Array.from(bytes, (byte) => byte.toString(16).padStart(2, "0")).join("");
  const groups: string[] = [];
  for (let index = 0; index < hex.length; index += GROUP_SIZE) {
    groups.push(hex.slice(index, index + GROUP_SIZE));
  }

  return `${prefix}-${groups.join("-")}`;
}

export function newCorrelationId(): string {
  return generateTraceId("ui");
}

export function newIdempotencyKey(): string {
  return generateTraceId("uikey");
}
