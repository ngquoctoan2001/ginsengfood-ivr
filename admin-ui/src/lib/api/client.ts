import { authorizationHeaders } from "@/lib/auth/guard";
import "server-only";

import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

import {
  ACTOR_HEADER,
  CORRELATION_HEADER,
  IDEMPOTENCY_HEADER,
  newCorrelationId,
  newIdempotencyKey,
} from "./correlation";
import { IvrApiError, parseErrorEnvelope } from "./errors";

/** Admin/intake surface of Ivr.Api (`specs/api/03-admin-api.md`). */
export const IVR_API_BASE_PATH = "/v1/ivr/order-confirmation";

export interface IvrApiRequest {
  readonly method: "GET" | "POST" | "PATCH" | "DELETE";
  readonly path: string;
  readonly session: AdminSession | null;
  readonly config: AdminUiConfig;
  readonly body?: unknown;
  readonly correlationId?: string;
  readonly idempotencyKey?: string;
  /** Required by Ivr.Api for danger-tier operations; never inferred from an arbitrary body. */
  readonly actionReason?: string;
  readonly signal?: AbortSignal;
  readonly fetchImpl?: typeof fetch;
}

export interface IvrApiResponse<T> {
  readonly data: T;
  readonly correlationId: string;
}

/**
 * Single egress point from the admin UI to Ivr.Api.
 *
 * Everything the API requires is attached here rather than at each call site:
 * `X-Correlation-Id` on every request without exception, `X-Actor-Id` bound to
 * the authenticated session, and `Idempotency-Key` on every mutation.
 *
 * This module is server-only. The browser never learns the API base URL and
 * never holds a credential — it talks to this Next.js server, which is the only
 * thing that talks to Ivr.Api (specs/ui/08 §4).
 */
export async function callIvrApi<T>(request: IvrApiRequest): Promise<IvrApiResponse<T>> {
  const correlationId = request.correlationId ?? newCorrelationId();
  const fetchImpl = request.fetchImpl ?? fetch;
  const url = `${request.config.apiBaseUrl}${IVR_API_BASE_PATH}${request.path}`;

  const headers = buildHeaders(request, correlationId);
  const hasBody = request.body !== undefined;

  let response: Response;
  try {
    response = await fetchImpl(url, {
      method: request.method,
      headers,
      body: hasBody ? JSON.stringify(request.body) : undefined,
      signal: request.signal,
      cache: "no-store",
      redirect: "error",
    });
  } catch (cause) {
    // A transport failure is not a business outcome. Surface it as a typed
    // envelope so the UI renders one consistent error shape (API-06 §5).
    throw new IvrApiError({
      code: "IVR_INTERNAL_ERROR",
      message: cause instanceof Error ? cause.message : "Ivr.Api is unreachable.",
      status: 0,
      correlationId,
    });
  }

  const effectiveCorrelationId =
    response.headers.get(CORRELATION_HEADER) ?? correlationId;
  const payload = await readJson(response);

  if (!response.ok) {
    throw parseErrorEnvelope(payload, {
      status: response.status,
      correlationId: effectiveCorrelationId,
      message: `Ivr.Api returned HTTP ${response.status}.`,
    });
  }

  return { data: payload as T, correlationId: effectiveCorrelationId };
}

function buildHeaders(request: IvrApiRequest, correlationId: string): Headers {
  const headers = new Headers({
    Accept: "application/json",
    [CORRELATION_HEADER]: correlationId,
  });

  if (request.body !== undefined) {
    headers.set("Content-Type", "application/json");
    if (request.session !== null) {
      headers.set(IDEMPOTENCY_HEADER, request.idempotencyKey ?? newIdempotencyKey());
    }
  }

  if (request.session === null) {
    return headers;
  }

  // W-0128. Tier credential plus the acting operator. The danger tier also needs
  // X-Action-Reason; the call sites that use it set that header themselves.
  headers.set(ACTOR_HEADER, request.session.actorId);
  for (const [name, value] of Object.entries(
    authorizationHeaders(request.session, request.actionReason),
  )) {
    headers.set(name, value);
  }
  return headers;
}

async function readJson(response: Response): Promise<unknown> {
  const text = await response.text();
  if (text.trim() === "") {
    return undefined;
  }

  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}
