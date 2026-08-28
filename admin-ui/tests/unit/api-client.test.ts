// @vitest-environment node
import { describe, expect, it } from "vitest";

import { callIvrApi, IVR_API_BASE_PATH } from "@/lib/api/client";
import { newCorrelationId, newIdempotencyKey } from "@/lib/api/correlation";
import { IvrApiError } from "@/lib/api/errors";
import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

const MOCK_CONFIG: AdminUiConfig = {
  apiBaseUrl: "http://127.0.0.1:5005",
  executionMode: "MOCK",
  isMockMode: true,
  realCustomerCallAllowed: false,
  environmentLabel: "dev",
  isProductionRuntime: false,
  isNonProductionEnvironment: true,
};

const SESSION: AdminSession = {
  actorId: "admin",
  displayName: "Quản trị viên",
  scope: "read" as const,
  role: "admin",
  permissions: ["IVR_QUEUE_VIEW", "IVR_QUEUE_PAUSE"],
};

interface Capture {
  url: string;
  init: RequestInit;
  headers: Headers;
}

function recordingFetch(response: Response): { fetchImpl: typeof fetch; calls: Capture[] } {
  const calls: Capture[] = [];
  const fetchImpl = (async (url: string | URL | Request, init?: RequestInit) => {
    calls.push({
      url: String(url),
      init: init ?? {},
      headers: new Headers(init?.headers),
    });
    return response.clone();
  }) as unknown as typeof fetch;

  return { fetchImpl, calls };
}

function jsonResponse(body: unknown, status = 200, correlationId = "api-1111"): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json", "X-Correlation-Id": correlationId },
  });
}

/** UT-UI-CORR-03 — every outbound request carries X-Correlation-Id. */
describe("UT-UI-CORR-03 correlation propagation", () => {
  it("attaches X-Correlation-Id to a GET", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({ paused: false }));

    await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: MOCK_CONFIG,
      fetchImpl,
    });

    expect(calls).toHaveLength(1);
    expect(calls[0].headers.get("X-Correlation-Id")).toBeTruthy();
    expect(calls[0].url).toBe(`${MOCK_CONFIG.apiBaseUrl}${IVR_API_BASE_PATH}/queue`);
  });

  it("attaches X-Correlation-Id and Idempotency-Key to a mutation", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({ admin_action_id: "A1" }));

    await callIvrApi({
      method: "POST",
      path: "/queue:pause",
      body: { reason: "capacity incident" },
      session: SESSION,
      config: MOCK_CONFIG,
      fetchImpl,
    });

    const { headers, init } = calls[0];
    expect(headers.get("X-Correlation-Id")).toBeTruthy();
    expect(headers.get("Idempotency-Key")).toBeTruthy();
    expect(headers.get("Content-Type")).toBe("application/json");
    expect(init.body).toBe(JSON.stringify({ reason: "capacity incident" }));
  });

  it("attaches X-Correlation-Id even without a session", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({}));

    await callIvrApi({
      method: "GET",
      path: "/queue",
      session: null,
      config: MOCK_CONFIG,
      fetchImpl,
    });

    expect(calls[0].headers.get("X-Correlation-Id")).toBeTruthy();
    expect(calls[0].headers.get("X-Actor-Id")).toBeNull();
    expect(calls[0].headers.get("X-Permissions")).toBeNull();
  });

  it("honours a caller-supplied correlation id", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({}));

    await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: MOCK_CONFIG,
      correlationId: "ui-fixed-0001",
      fetchImpl,
    });

    expect(calls[0].headers.get("X-Correlation-Id")).toBe("ui-fixed-0001");
  });

  it("generates ids that satisfy the Ivr.Api trace validator", () => {
    // Mirrors InternalRequestGuard.RequireCorrelation + PiiGuard's MSISDN rule.
    const charset = /^[A-Za-z0-9\-_.:]+$/;
    const msisdn = /(?<![0-9])(?:\+?84|0)[0-9]{9}(?![0-9])/;

    for (let index = 0; index < 2_000; index += 1) {
      for (const id of [newCorrelationId(), newIdempotencyKey()]) {
        expect(id.length).toBeLessThanOrEqual(128);
        expect(charset.test(id)).toBe(true);
        expect(msisdn.test(id)).toBe(false);
      }
    }
  });
});

describe("Ivr.Api call contract", () => {
  it("binds X-Actor-Id and the opaque bearer token to the authenticated session", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({}));

    await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: MOCK_CONFIG,
      fetchImpl,
    });

    const { headers } = calls[0];
    expect(headers.get("X-Actor-Id")).toBe("admin");
    expect(headers.get("X-Actor-Id")).toBe(SESSION.actorId);
    expect(headers.get("X-Service-Scope")).toBe("ivr.admin.read");
    expect(headers.get("X-Mock-Actor-Id")).toBeNull();
    expect(headers.get("X-Permissions")).toBeNull();
  });

  it("uses the same bearer session outside MOCK mode", async () => {
    const { fetchImpl, calls } = recordingFetch(jsonResponse({}));
    const labConfig: AdminUiConfig = {
      ...MOCK_CONFIG,
      executionMode: "LAB_REAL_SIM",
      isMockMode: false,
    };

    await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: labConfig,
      fetchImpl,
    });

    expect(calls).toHaveLength(1);
    expect(calls[0].headers.get("X-Actor-Id")).toBe(SESSION.actorId);
    expect(calls[0].headers.get("X-Service-Scope")).toBe("ivr.admin.read");
  });

  it("turns an error response into a typed envelope with the server correlation id", async () => {
    const { fetchImpl } = recordingFetch(
      jsonResponse(
        {
          error: {
            code: "IVR_FORBIDDEN_CALLER",
            message: "Missing permission.",
            correlationId: "api-9999",
          },
        },
        403,
        "api-9999",
      ),
    );

    const failure = await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: MOCK_CONFIG,
      fetchImpl,
    }).catch((error: unknown) => error);

    expect(failure).toBeInstanceOf(IvrApiError);
    expect(failure).toMatchObject({
      code: "IVR_FORBIDDEN_CALLER",
      status: 403,
      correlationId: "api-9999",
    });
  });

  it("turns a transport failure into IVR_INTERNAL_ERROR rather than leaking it", async () => {
    const fetchImpl = (async () => {
      throw new TypeError("fetch failed");
    }) as unknown as typeof fetch;

    const failure = await callIvrApi({
      method: "GET",
      path: "/queue",
      session: SESSION,
      config: MOCK_CONFIG,
      fetchImpl,
    }).catch((error: unknown) => error);

    expect(failure).toBeInstanceOf(IvrApiError);
    expect(failure).toMatchObject({ code: "IVR_INTERNAL_ERROR", status: 0 });
  });
});
