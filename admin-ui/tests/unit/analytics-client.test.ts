// @vitest-environment node
import { describe, expect, it } from "vitest";

import {
  exportAnalytics,
  getAnalyticsBreakdown,
  getAnalyticsSummary,
  getAnalyticsTrend,
} from "@/lib/analytics/client";
import { IvrApiError } from "@/lib/api/errors";
import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

const CONFIG: AdminUiConfig = {
  apiBaseUrl: "http://127.0.0.1:5005",
  executionMode: "MOCK",
  isMockMode: true,
  realCustomerCallAllowed: false,
  environmentLabel: "dev",
  isProductionRuntime: false,
  isNonProductionEnvironment: true,
};

const SESSION: AdminSession = {
  actorId: "AGT-ADMIN-01",
  role: "AdminIM",
  permissions: ["IVR_QUEUE_VIEW"],
  issuedAt: 0,
  expiresAt: 4_102_444_800,
};

function recordingFetch(body: unknown, status = 200): {
  fetchImpl: typeof fetch;
  urls: string[];
} {
  const urls: string[] = [];
  const fetchImpl = (async (url: string | URL | Request) => {
    urls.push(String(url));
    return new Response(JSON.stringify(body), {
      status,
      headers: { "Content-Type": "application/json", "X-Correlation-Id": "api-2222" },
    });
  }) as unknown as typeof fetch;

  return { fetchImpl, urls };
}

function queryOf(url: string): URLSearchParams {
  return new URL(url).searchParams;
}

/**
 * UT-UI-REPORT-02 — the filter the operator sets in the URL is the filter the
 * analytics API receives, with the calendar-day inputs widened to the instants
 * the contract expects.
 */
describe("UT-UI-REPORT-02 filter round-trip", () => {
  it("maps every filter field onto its contract query parameter", async () => {
    const { fetchImpl, urls } = recordingFetch({});

    await getAnalyticsSummary(
      { session: SESSION, config: CONFIG, fetchImpl },
      {
        program: "GOLDEN_HOUR",
        resultType: "IVR_CONFIRMED",
        scriptVariant: "SCRIPT-ORDER-CONFIRM:vA",
        bucket: "HOUR",
        from: "2026-08-01",
        to: "2026-08-14",
      },
    );

    const query = queryOf(urls[0]);
    expect(urls[0]).toContain("/analytics/summary?");
    expect(query.get("program")).toBe("GOLDEN_HOUR");
    expect(query.get("result_type")).toBe("IVR_CONFIRMED");
    expect(query.get("script_variant")).toBe("SCRIPT-ORDER-CONFIRM:vA");
    expect(query.get("bucket")).toBe("HOUR");
    // A day input must cover the whole day, not midnight.
    expect(query.get("from")).toBe("2026-08-01T00:00:00Z");
    expect(query.get("to")).toBe("2026-08-14T23:59:59Z");
  });

  it("omits empty filters instead of sending blank parameters", async () => {
    const { fetchImpl, urls } = recordingFetch({});

    await getAnalyticsTrend(
      { session: SESSION, config: CONFIG, fetchImpl },
      { program: "", resultType: "", from: "", to: "", bucket: "DAY" },
    );

    const query = queryOf(urls[0]);
    expect([...query.keys()]).toEqual(["bucket"]);
  });

  it("sends the selected dimension on breakdown and export", async () => {
    const { fetchImpl, urls } = recordingFetch({});

    await getAnalyticsBreakdown(
      { session: SESSION, config: CONFIG, fetchImpl },
      "SCRIPT_VARIANT",
      { program: "TWENTY_FOUR_SEVEN" },
    );
    await exportAnalytics(
      { session: SESSION, config: CONFIG, fetchImpl },
      "PROGRAM",
      "weekly confirm-rate review",
      { program: "TWENTY_FOUR_SEVEN" },
    );

    expect(queryOf(urls[0]).get("dimension")).toBe("SCRIPT_VARIANT");
    expect(queryOf(urls[1]).get("dimension")).toBe("PROGRAM");
    expect(queryOf(urls[1]).get("reason")).toBe("weekly confirm-rate review");
  });

  it("never sends a caller-supplied anonymity threshold", async () => {
    const { fetchImpl, urls } = recordingFetch({});

    await getAnalyticsBreakdown(
      { session: SESSION, config: CONFIG, fetchImpl },
      "PROGRAM",
      { program: "GOLDEN_HOUR" },
    );

    expect(urls[0]).not.toContain("min_bucket_size");
  });

  it("surfaces a refused export as an error rather than an empty extract", async () => {
    const { fetchImpl } = recordingFetch(
      {
        error: {
          code: "IVR_PII_POLICY_VIOLATION",
          message: "Lát cắt quá nhỏ.",
          correlationId: "api-2222",
        },
      },
      422,
    );

    await expect(
      exportAnalytics(
        { session: SESSION, config: CONFIG, fetchImpl },
        "PROGRAM",
        "isolate the small cohort",
      ),
    ).rejects.toBeInstanceOf(IvrApiError);
  });
});
