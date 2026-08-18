// @vitest-environment node
import { spawn, type ChildProcess } from "node:child_process";
import { createServer as createHttpServer, type Server } from "node:http";
import { createServer as createNetServer } from "node:net";
import { fileURLToPath } from "node:url";

import { afterAll, beforeAll, describe, expect, it } from "vitest";

const projectRoot = fileURLToPath(new URL("../../", import.meta.url));
const nextBin = fileURLToPath(new URL("../../node_modules/next/dist/bin/next", import.meta.url));

/**
 * E2E-UI-REPORT-05.
 *
 * Drives the P3-4 reporting console end to end against a stub analytics API.
 *
 * On the "user without permission" half: every actor in `seed/agents.sample.json`
 * holds `IVR_QUEUE_VIEW`, so there is no seeded identity that lacks it. The
 * authority is the API anyway — so the denial is driven where it actually
 * happens, by having the stub answer `403 IVR_FORBIDDEN_CALLER` for one actor,
 * and the assertion is that the screen renders the refusal instead of numbers.
 * The signed-out path is covered separately by the redirect to `/login`.
 */

let apiServer: Server | undefined;
let devServer: ChildProcess | undefined;
let baseUrl = "";

const DENIED_ACTOR = "AGT-VIEWER-01";
const ALLOWED_ACTOR = "AGT-ADMIN-01";

const DATA_QUALITY = {
  generated_at: "2026-08-15T02:00:00Z",
  source: "OPERATIONAL_READ_MODEL",
  warehouse_backed: false,
  pipeline_work_id: "W-0055",
  latest_event_at: "2026-08-15T01:58:00Z",
  freshness_seconds: 120,
  status: "FRESH",
  min_bucket_size: 5,
  suppressed_bucket_count: 1,
  scanned_rows: 13,
  truncated: false,
};

const FILTER = { bucket: "DAY" };

const SUMMARY = {
  filter: FILTER,
  execution_mode: "MOCK",
  kpi: {
    total_results: 13,
    total_final_results: 13,
    total_call_jobs: 13,
    total_eligible_tasks: 13,
    confirm_rate: 0.4615,
    cancel_rate: 0,
    no_answer_rate: 0.3846,
    invalid_phone_rate: 0,
    technical_rate: 0.1538,
    operational_blocked_rate: 0,
    attempt_2_rate: 0.2308,
    avg_seconds_to_final: 135,
  },
  result_taxonomy: [
    { key: "IVR_CONFIRMED", total: 6, confirmed: 6, confirm_rate: 1, share: 0.4615 },
    { key: "IVR_NO_ANSWER_FINAL", total: 5, confirmed: 0, confirm_rate: 0, share: 0.3846 },
  ],
  data_quality: DATA_QUALITY,
};

const TREND = {
  filter: FILTER,
  buckets: [
    {
      bucket_start: "2026-08-14T00:00:00Z",
      program: "GOLDEN_HOUR",
      total: 11,
      confirmed: 6,
      cancelled: 0,
      no_answer: 5,
      invalid_phone: 0,
      technical: 0,
      operational_blocked: 0,
      confirm_rate: 0.5455,
    },
  ],
  data_quality: DATA_QUALITY,
};

const BREAKDOWN = {
  filter: FILTER,
  dimension: "RESULT_TYPE",
  rows: SUMMARY.result_taxonomy,
  data_quality: DATA_QUALITY,
};

const EXPORT = {
  filter: FILTER,
  dimension: "PROGRAM",
  reason: "weekly confirm-rate review",
  actor_id: ALLOWED_ACTOR,
  correlation_id: "corr-e2e-report",
  audit_ref: "8f1f1b0e-0000-4000-8000-000000000001",
  columns: ["dimension", "key", "total", "confirmed", "confirm_rate", "share"],
  rows: [["PROGRAM", "GOLDEN_HOUR", "11", "6", "0.5455", "0.8462"]],
  suppressed_row_count: 1,
  data_quality: DATA_QUALITY,
};

function findFreePort(): Promise<number> {
  return new Promise((resolve, reject) => {
    const probe = createNetServer();
    probe.once("error", reject);
    probe.listen(0, "127.0.0.1", () => {
      const address = probe.address();
      if (address === null || typeof address === "string") {
        probe.close(() => reject(new Error("could not resolve a free port")));
        return;
      }

      const { port } = address;
      probe.close(() => resolve(port));
    });
  });
}

function startNext(port: number, apiPort: number): ChildProcess {
  return spawn(process.execPath, [nextBin, "start", "--port", String(port)], {
    cwd: projectRoot,
    env: {
      ...process.env,
      NODE_ENV: "production",
      IVR_EXECUTION_MODE: "MOCK",
      IVR_ENVIRONMENT_LABEL: "dev",
      REAL_CUSTOMER_CALL_ALLOWED: "NO",
      IVR_ADMIN_UI_SESSION_SECRET: "e2e-reports-secret-0123456789abcdef",
      IVR_API_BASE_URL: `http://127.0.0.1:${apiPort}`,
    },
    stdio: "ignore",
  });
}

async function waitFor(url: string): Promise<void> {
  const deadline = Date.now() + 60_000;
  while (Date.now() < deadline) {
    try {
      const probe = await fetch(url, { redirect: "manual" });
      if (probe.status > 0) {
        return;
      }
    } catch {
      // keep waiting
    }

    await new Promise((resolve) => setTimeout(resolve, 400));
  }

  throw new Error(`server did not become ready: ${url}`);
}

beforeAll(async () => {
  const apiPort = await findFreePort();
  apiServer = createHttpServer((request, response) => {
    const url = new URL(request.url ?? "/", "http://127.0.0.1");
    const base = "/v1/ivr/order-confirmation";

    if (request.headers["x-actor-id"] === DENIED_ACTOR) {
      response.writeHead(403, { "Content-Type": "application/json" });
      response.end(
        JSON.stringify({
          error: {
            code: "IVR_FORBIDDEN_CALLER",
            message: "caller is not permitted",
            correlationId: "corr-denied",
          },
        }),
      );
      return;
    }

    // A slice too small to be anonymous is refused, exactly as the API does.
    if (
      url.pathname === `${base}/analytics/export` &&
      url.searchParams.get("program") === "TWENTY_FOUR_SEVEN"
    ) {
      response.writeHead(422, { "Content-Type": "application/json" });
      response.end(
        JSON.stringify({
          error: {
            code: "IVR_PII_POLICY_VIOLATION",
            message: "slice too small",
            correlationId: "corr-suppressed",
          },
        }),
      );
      return;
    }

    const routes: Record<string, unknown> = {
      [`${base}/analytics/summary`]: SUMMARY,
      [`${base}/analytics/trend`]: TREND,
      [`${base}/analytics/breakdown`]: BREAKDOWN,
      [`${base}/analytics/export`]: EXPORT,
    };

    const body = routes[url.pathname];
    if (body === undefined) {
      response.writeHead(404, { "Content-Type": "application/json" });
      response.end(
        JSON.stringify({
          error: { code: "IVR_NOT_FOUND", message: "not found", correlationId: "" },
        }),
      );
      return;
    }

    response.writeHead(200, { "Content-Type": "application/json" });
    response.end(JSON.stringify(body));
  });
  await new Promise<void>((resolve) => apiServer!.listen(apiPort, "127.0.0.1", resolve));

  const port = await findFreePort();
  baseUrl = `http://127.0.0.1:${port}`;
  devServer = startNext(port, apiPort);
  await waitFor(`${baseUrl}/login`);
});

afterAll(async () => {
  devServer?.kill();
  await new Promise<void>((resolve) => {
    if (apiServer === undefined) {
      resolve();
      return;
    }

    apiServer.close(() => resolve());
  });
});

async function signedIn(actorId: string): Promise<string> {
  const response = await fetch(`${baseUrl}/api/auth/sign-in`, {
    method: "POST",
    redirect: "manual",
    body: new URLSearchParams({ actorId }),
  });
  const header = response.headers
    .getSetCookie()
    .find((cookie) => cookie.startsWith("ivr_admin_session="));
  if (header === undefined) {
    throw new Error("sign-in did not issue a session");
  }

  return header.split(";")[0];
}

function visit(path: string, cookie: string): Promise<Response> {
  return fetch(`${baseUrl}${path}`, { headers: { Cookie: cookie }, redirect: "manual" });
}

describe("E2E-UI-REPORT-05 reporting console", () => {
  it("renders KPI, trend, breakdown and the freshness banner", async () => {
    const cookie = await signedIn(ALLOWED_ACTOR);
    const html = await (await visit("/reports", cookie)).text();

    expect(html).toContain("Báo cáo &amp; phân tích");
    // KPI values are the ones the API computed, formatted by the console.
    expect(html).toContain("46,2%");
    expect(html).toContain("23,1%");
    expect(html).toContain("2m 15s");
    // Trend and breakdown.
    expect(html).toContain("GOLDEN_HOUR");
    expect(html).toContain("54,6%");
    expect(html).toContain("IVR_NO_ANSWER_FINAL");
    // Freshness plus the honest source statement and the suppression count.
    expect(html).toContain("Độ tươi dữ liệu");
    expect(html).toContain("CHƯA có pipeline P10-4");
    expect(html).toContain("W-0055");
    expect(html).toContain("k=5");
  });

  it("carries the filter into the URL and back into the request", async () => {
    const cookie = await signedIn(ALLOWED_ACTOR);
    const response = await visit(
      "/reports?program=GOLDEN_HOUR&bucket=HOUR&dimension=PROGRAM&from=2026-08-01&to=2026-08-14",
      cookie,
    );
    const html = await response.text();

    expect(response.status).toBe(200);
    expect(html).toContain('value="GOLDEN_HOUR"');
    expect(html).toContain('value="2026-08-01"');
    expect(html).toContain('value="2026-08-14"');
  });

  it("exports a sanitized CSV and states the audit reference", async () => {
    const cookie = await signedIn(ALLOWED_ACTOR);
    const response = await visit(
      "/reports/export?dimension=PROGRAM&reason=weekly%20confirm-rate%20review",
      cookie,
    );
    const csv = await response.text();

    expect(response.status).toBe(200);
    expect(response.headers.get("content-type")).toContain("text/csv");
    expect(response.headers.get("content-disposition")).toContain("ivr-analytics-program.csv");
    expect(response.headers.get("x-ivr-audit-ref")).toBe(EXPORT.audit_ref);
    expect(response.headers.get("x-ivr-suppressed-rows")).toBe("1");
    expect(csv).toContain("dimension,key,total,confirmed,confirm_rate,share");
    expect(csv).toContain("PROGRAM,GOLDEN_HOUR,11,6,0.5455,0.8462");
    // Aggregates only: nothing that could name a customer.
    expect(csv).not.toMatch(/\d{9,}/);
  });

  it("refuses an export without a usable reason and one that would re-identify", async () => {
    const cookie = await signedIn(ALLOWED_ACTOR);

    const noReason = await visit("/reports/export?dimension=PROGRAM", cookie);
    expect(noReason.status).toBe(400);

    const tooSmall = await visit(
      "/reports/export?program=TWENTY_FOUR_SEVEN&reason=isolate%20the%20small%20cohort",
      cookie,
    );
    expect(tooSmall.status).toBe(422);
    expect(await tooSmall.text()).toContain("IVR_PII_POLICY_VIOLATION");
  });

  it("shows the refusal rather than numbers when the API denies the caller", async () => {
    const cookie = await signedIn(DENIED_ACTOR);
    const html = await (await visit("/reports", cookie)).text();

    expect(html).toContain("IVR_FORBIDDEN_CALLER");
    expect(html).not.toContain("46,2%");
    expect(html).not.toContain("54,6%");
  });

  it("sends a signed-out visitor to login instead of the report", async () => {
    const response = await fetch(`${baseUrl}/reports`, { redirect: "manual" });

    expect([302, 307]).toContain(response.status);
    expect(response.headers.get("location")).toContain("/login");
  });

  it("offers no control that could dispatch a call or change an order", async () => {
    const cookie = await signedIn(ALLOWED_ACTOR);
    const html = await (await visit("/reports", cookie)).text();
    const markup = html.replace(/<script[\s\S]*?<\/script>/gi, "");

    const controlPattern =
      /<(?:button|a)[^>]*>(?:(?!<\/(?:button|a)>)[\s\S])*?(?:gọi lại|xác nhận đơn|huỷ đơn|tạm dừng|tiếp tục hàng đợi)/gi;
    expect(markup.match(controlPattern)).toBeNull();
  });
});
