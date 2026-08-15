import { isIvrErrorCode, type IvrErrorCode, type IvrErrorEnvelope } from "./types";

/**
 * A failed Ivr.Api call, normalised into the API-06 error envelope shape so the
 * UI always renders `code` + message + correlation id regardless of whether the
 * failure came from the API, the transport, or a malformed body.
 */
export class IvrApiError extends Error {
  readonly code: IvrErrorCode;
  readonly status: number;
  readonly correlationId: string;
  readonly details: Readonly<Record<string, string>>;

  constructor(init: {
    code: IvrErrorCode;
    message: string;
    status: number;
    correlationId: string;
    details?: Readonly<Record<string, string>>;
  }) {
    super(init.message);
    this.name = "IvrApiError";
    this.code = init.code;
    this.status = init.status;
    this.correlationId = init.correlationId;
    this.details = init.details ?? {};
  }

  /** Plain, serialisable shape safe to hand to a Client Component. */
  toEnvelope(): IvrErrorEnvelope["error"] {
    return {
      code: this.code,
      message: this.message,
      correlationId: this.correlationId,
      details: this.details,
    };
  }
}

function readDetails(value: unknown): Record<string, string> | undefined {
  if (typeof value !== "object" || value === null) {
    return undefined;
  }

  const details: Record<string, string> = {};
  for (const [key, detail] of Object.entries(value as Record<string, unknown>)) {
    if (typeof detail === "string") {
      details[key] = detail;
    }
  }

  return Object.keys(details).length > 0 ? details : undefined;
}

/**
 * Parse an error response body into an `IvrApiError`.
 *
 * An unrecognised or absent `code` degrades to `IVR_INTERNAL_ERROR` rather than
 * surfacing an arbitrary server string: API-06 §5 keeps the code catalogue
 * closed, and a UI that renders unknown codes invites contract drift.
 */
export function parseErrorEnvelope(
  body: unknown,
  fallback: { status: number; correlationId: string; message: string },
): IvrApiError {
  const envelope =
    typeof body === "object" && body !== null
      ? (body as { error?: Record<string, unknown> }).error
      : undefined;

  const code = isIvrErrorCode(envelope?.code) ? envelope.code : "IVR_INTERNAL_ERROR";
  const message =
    typeof envelope?.message === "string" && envelope.message.trim() !== ""
      ? envelope.message
      : fallback.message;
  const correlationId =
    typeof envelope?.correlationId === "string" && envelope.correlationId.trim() !== ""
      ? envelope.correlationId
      : fallback.correlationId;

  return new IvrApiError({
    code,
    message,
    status: fallback.status,
    correlationId,
    details: readDetails(envelope?.details),
  });
}
