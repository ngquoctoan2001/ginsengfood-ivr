// @vitest-environment node
import { spawn, type ChildProcess } from "node:child_process";
import { createServer as createHttpServer, type Server } from "node:http";
import { createServer as createNetServer } from "node:net";
import { fileURLToPath } from "node:url";

import { afterAll, beforeAll, describe, expect, it } from "vitest";

const projectRoot = fileURLToPath(new URL("../../", import.meta.url));
const nextBin = fileURLToPath(new URL("../../node_modules/next/dist/bin/next", import.meta.url));

/**
 * E2E-UI-LOG-01 and E2E-UI-DETAIL-02.
 *
 * A real `next start` server is driven over HTTP. Ivr.Api is replaced by a stub
 * that speaks the same wire contract, so the screens are exercised end to end —
 * proxy, session, server render, API client, markup — without needing .NET and
 * PostgreSQL in the front-end job. The contract itself is held by the .NET
 * suite (`IT-ADMIN-READ-*`) and by `tests/unit/contract-drift.test.ts`.
 */

const JOB_ID = "JOB-E2E-GH";

let apiServer: Server | undefined;
let nextServer: ChildProcess | undefined;
let baseUrl = "";
let apiRequests: string[] = [];

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

function dashboardPayload(program: string | null) {
  const scopedToGoldenHour = program === "GOLDEN_HOUR";
  return {
    generated_at: "2026-08-15T02:00:00Z",
    execution_mode: "MOCK",
    sim_provider: "MOCK",
    real_customer_call_allowed: false,
    program_filter: program ?? undefined,
    queue: {
      paused: false,
      queued: scopedToGoldenHour ? 1 : 2,
      held_mock: 1,
      held_admin_review: 0,
      dispatching: 0,
      open_total: scopedToGoldenHour ? 1 : 3,
      closed_total: 4,
      near_expiry: 1,
    },
    results: {
      total: scopedToGoldenHour ? 1 : 4,
      by_result_type: { IVR_CONFIRMED: 2, IVR_NO_ANSWER_FINAL: 2 },
      confirm_rate: 0.5,
      cancel_rate: 0,
      no_answer_rate: 0.5,
      technical_exception_rate: 0.25,
    },
    attempts: {
      total: 6,
      counted_customer_attempts: 4,
      technical_retries: 1,
      active: 0,
    },
    sim: {
      total: 2,
      enabled: 1,
      idle: 1,
      active: 0,
      disabled: 1,
      health_failed: 0,
      quarantined: 0,
      adapter_mode: "MOCK",
    },
    open_incidents: [
      {
        capacity_incident_id: "INCIDENT-E2E-1",
        scope: "SCHEDULER_DEADLINE",
        status: "OPEN",
        hold_new_calls: false,
        shortage_reason: "MOCK_CAPACITY_SHORTAGE",
        missed_deadline_count: 2,
        opened_at: "2026-08-15T01:00:00Z",
      },
    ],
    missed_deadline_count: 2,
  };
}

const GOLDEN_HOUR_ROW = {
  ivr_call_job_id: JOB_ID,
  task_id: "TASK-E2E-GH",
  order_code_short: "GF-GH",
  phone_masked: "84xxxxx0065",
  program_type: "GOLDEN_HOUR",
  status: "DRY_RUN",
  queue_status: "QUEUED",
  attempt_count: 2,
  max_attempts: 2,
  result_type: "IVR_CONFIRMED",
  expires_at: "2026-08-15T02:05:00Z",
  created_at: "2026-08-15T01:55:00Z",
  near_expiry: true,
};

const TWENTY_FOUR_SEVEN_ROW = {
  ...GOLDEN_HOUR_ROW,
  ivr_call_job_id: "JOB-E2E-247",
  task_id: "TASK-E2E-247",
  order_code_short: "GF-247",
  phone_masked: "84xxxxx0247",
  program_type: "TWENTY_FOUR_SEVEN",
  queue_status: "HELD_MOCK",
  result_type: "IVR_NO_ANSWER_FINAL",
  near_expiry: false,
};

function callJobsPayload(search: URLSearchParams) {
  const orderCode = search.get("order_code");
  const queueStatus = search.get("queue_status");
  const rows = [GOLDEN_HOUR_ROW, TWENTY_FOUR_SEVEN_ROW].filter((row) => {
    if (orderCode === "GF-ORDER-GH-FULL" && row.ivr_call_job_id !== JOB_ID) {
      return false;
    }

    return !(queueStatus !== null && row.queue_status !== queueStatus);
  });

  return { page: 1, page_size: 25, total_count: rows.length, items: rows };
}

const DETAIL_PAYLOAD = {
  ivr_call_job_id: JOB_ID,
  task_id: "TASK-E2E-GH",
  order_code_short: "GF-GH",
  phone_masked: "84xxxxx0065",
  program_type: "GOLDEN_HOUR",
  order_state: "CONFIRMING",
  order_version_snapshot: "7",
  status: "DRY_RUN",
  queue_status: "QUEUED",
  eligible: true,
  eligibility_decision: "ELIGIBLE_FOR_IVR",
  blocked_reasons: ["DO_NOT_CALL"],
  call_restriction: false,
  max_attempts: 2,
  attempt_policy_code: "mock-lab-v1",
  script_version: "SCRIPT-ORDER-CONFIRM:v1-test-approved",
  privacy_policy_version: "privacy-v1",
  t0_at: "2026-08-15T01:55:00Z",
  expires_at: "2026-08-15T02:05:00Z",
  created_at: "2026-08-15T01:55:00Z",
  attempts: [
    {
      ivr_call_attempt_id: "ATTEMPT-E2E-1",
      attempt_number: 1,
      scheduled_at: "2026-08-15T01:55:00Z",
      status: "TECHNICAL_FAILED",
      result_status: "IVR_TECHNICAL_EXCEPTION",
      is_counted_customer_attempt: false,
      technical_retry_count: 1,
      technical_exception_type: "MOCK_ADAPTER_FAULT",
      sim_channel_id: "SIM-MOCK-001",
      policy_version: "mock-lab-v1",
      script_version: "v1-test-approved",
    },
    {
      ivr_call_attempt_id: "ATTEMPT-E2E-2",
      attempt_number: 2,
      scheduled_at: "2026-08-15T01:57:30Z",
      status: "COMPLETED",
      result_status: "IVR_CONFIRMED",
      disposition: "CONFIRMED",
      dtmf_key: "1",
      is_counted_customer_attempt: true,
      technical_retry_count: 0,
      sim_channel_id: "SIM-MOCK-001",
      policy_version: "mock-lab-v1",
      script_version: "v1-test-approved",
    },
  ],
  results: [
    {
      ivr_call_result_id: "RESULT-E2E-1",
      result_type: "IVR_CONFIRMED",
      dtmf_key: "1",
      is_counted_customer_attempt: true,
      is_final_for_ivr: true,
      recommended_core_action: "CORE_REVALIDATE_AND_CONTINUE",
      human_review_required: false,
      created_at: "2026-08-15T01:58:00Z",
    },
  ],
  callbacks: [
    {
      callback_id: "CALLBACK-E2E-1",
      ivr_call_result_id: "RESULT-E2E-1",
      result_state: "DELIVERED",
      delivery_status: "ACKNOWLEDGED",
      core_http_status: 422,
      core_response_code: "REJECTED_STALE",
      retry_count: 1,
      requires_core_revalidation: true,
      created_at: "2026-08-15T01:58:10Z",
    },
  ],
  technical_exceptions: [
    {
      technical_exception_id: "TECH-E2E-1",
      ivr_call_attempt_id: "ATTEMPT-E2E-1",
      exception_type: "MOCK_ADAPTER_FAULT",
      customer_attempt_counted: false,
      technical_retry_allowed: true,
      technical_retry_count: 1,
      created_at: "2026-08-15T01:56:00Z",
    },
  ],
  review_items: [
    {
      review_item_id: "REVIEW-E2E-1",
      source_type: "IVR_CALL_RESULT",
      source_id: "RESULT-E2E-1",
      reason: "verify confirmed evidence",
      status: "OPEN",
      created_at: "2026-08-15T01:58:20Z",
    },
  ],
  evidence_refs: ["evidence://ivr/e2e/task", "evidence://ivr/e2e/result"],
  audit_refs: ["audit://ivr/e2e/result"],
  correlation_id: "corr-e2e-gh",
  input_signal_only: true,
  no_direct_order_update: true,
};

beforeAll(async () => {
  const apiPort = await findFreePort();
  apiServer = createHttpServer((request, response) => {
    const url = new URL(request.url ?? "/", "http://127.0.0.1");
    apiRequests.push(`${request.method} ${url.pathname}${url.search}`);

    const base = "/v1/ivr/order-confirmation";
    let body: unknown;
    if (url.pathname === `${base}/dashboard`) {
      body = dashboardPayload(url.searchParams.get("program"));
    } else if (url.pathname === `${base}/call-jobs`) {
      body = callJobsPayload(url.searchParams);
    } else if (url.pathname === `${base}/call-jobs/${JOB_ID}/detail`) {
      body = DETAIL_PAYLOAD;
    } else {
      response.writeHead(404, { "Content-Type": "application/json" });
      response.end(
        JSON.stringify({
          error: {
            code: "IVR_NOT_FOUND",
            message: "not found",
            correlationId: request.headers["x-correlation-id"] ?? "",
          },
        }),
      );
      return;
    }

    response.writeHead(200, {
      "Content-Type": "application/json",
      "X-Correlation-Id": String(request.headers["x-correlation-id"] ?? ""),
    });
    response.end(JSON.stringify(body));
  });
  await new Promise<void>((resolve) => apiServer!.listen(apiPort, "127.0.0.1", resolve));

  const env: NodeJS.ProcessEnv = {
    ...process.env,
    NODE_ENV: "production",
    IVR_EXECUTION_MODE: "MOCK",
    IVR_ENVIRONMENT_LABEL: "test",
    REAL_CUSTOMER_CALL_ALLOWED: "NO",
    IVR_ADMIN_UI_SESSION_SECRET: "e2e-screen-secret-0123456789abcdefghij",
    IVR_API_BASE_URL: `http://127.0.0.1:${apiPort}`,
  };

  // The app is built once by tests/e2e/global-setup.ts.
  const port = await findFreePort();
  baseUrl = `http://127.0.0.1:${port}`;
  nextServer = spawn(process.execPath, [nextBin, "start", "--port", String(port)], {
    cwd: projectRoot,
    env,
    stdio: "ignore",
  });

  const deadline = Date.now() + 60_000;
  while (Date.now() < deadline) {
    try {
      const probe = await fetch(`${baseUrl}/login`, { redirect: "manual" });
      if (probe.status > 0) {
        break;
      }
    } catch {
      // keep waiting
    }

    await new Promise((resolve) => setTimeout(resolve, 400));
  }
});

afterAll(async () => {
  nextServer?.kill();
  await new Promise<void>((resolve) => {
    if (apiServer === undefined) {
      resolve();
      return;
    }

    apiServer.close(() => resolve());
  });
});

async function signedInCookie(actorId = "AGT-ADMIN-01"): Promise<string> {
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

function getHtml(path: string, cookie: string): Promise<Response> {
  return fetch(`${baseUrl}${path}`, { headers: { Cookie: cookie }, redirect: "manual" });
}

describe("E2E-UI-LOG-01 call log", () => {
  it("renders masked rows and never the full order code", async () => {
    const cookie = await signedInCookie();
    const response = await getHtml("/calls", cookie);
    const html = await response.text();

    expect(response.status).toBe(200);
    expect(html).toContain("Nhật ký cuộc gọi");
    expect(html).toContain("84xxxxx0065");
    expect(html).toContain("84xxxxx0247");
    expect(html).toContain("GF-GH");
    expect(html).toContain("GF-247");
    expect(html).toContain("Sắp hết hạn");
    // Masked values render as-is; a raw number would have been redacted.
    expect(html).not.toContain("[đã ẩn]");
    expect(html).not.toMatch(/(?<![0-9])0\d{9}(?![0-9])/);
  });

  it("passes order_code and queue_status through to the API and narrows the result", async () => {
    const cookie = await signedInCookie();
    apiRequests = [];

    const byOrderCode = await getHtml("/calls?order_code=GF-ORDER-GH-FULL", cookie);
    const orderCodeHtml = await byOrderCode.text();
    expect(orderCodeHtml).toContain("GF-GH");
    expect(orderCodeHtml).not.toContain("GF-247");
    // The table shows `order_code_short`, never the full code the API was
    // queried with. (The filter input echoes the operator's own text back, which
    // is why this asserts on the table cells rather than the whole document.)
    const cells = [...orderCodeHtml.matchAll(/<td[^>]*>([\s\S]*?)<\/td>/g)].map(
      (match) => match[1],
    );
    expect(cells.length).toBeGreaterThan(0);
    expect(cells.some((cell) => cell.includes("GF-GH"))).toBe(true);
    expect(cells.some((cell) => cell.includes("GF-ORDER-GH-FULL"))).toBe(false);

    const byQueueStatus = await getHtml("/calls?queue_status=HELD_MOCK", cookie);
    const queueStatusHtml = await byQueueStatus.text();
    expect(queueStatusHtml).toContain("GF-247");
    expect(queueStatusHtml).not.toContain("GF-GH");

    expect(apiRequests.some((entry) => entry.includes("order_code=GF-ORDER-GH-FULL"))).toBe(true);
    expect(apiRequests.some((entry) => entry.includes("queue_status=HELD_MOCK"))).toBe(true);
  });

  it("shows the API-computed dashboard figures and honours the program filter", async () => {
    const cookie = await signedInCookie();
    apiRequests = [];

    const all = await getHtml("/dashboard", cookie);
    const allHtml = await all.text();
    expect(allHtml).toContain("Tổng quan vận hành IVR");
    expect(allHtml).toContain("50.0%");
    expect(allHtml).toContain("SCHEDULER_DEADLINE");

    const filtered = await getHtml("/dashboard?program=GOLDEN_HOUR", cookie);
    expect(await filtered.text()).toContain("Tổng quan vận hành IVR");
    expect(apiRequests.some((entry) => entry.includes("program=GOLDEN_HOUR"))).toBe(true);
  });
});

describe("E2E-UI-DETAIL-02 call detail", () => {
  it("renders the attempt timeline, result, Core callback code and evidence links", async () => {
    const cookie = await signedInCookie();
    const response = await getHtml(`/calls/${JOB_ID}`, cookie);
    const html = await response.text();

    expect(response.status).toBe(200);

    // Both attempts, in order, with DTMF shown as business semantics.
    expect(html).toContain("ATTEMPT-E2E-1");
    expect(html).toContain("ATTEMPT-E2E-2");
    expect(html).toContain("MOCK_ADAPTER_FAULT");
    expect(html).toContain("1 — xác nhận");

    // Result plus its advisory framing.
    expect(html).toContain("IVR_CONFIRMED");
    expect(html).toContain("CORE_REVALIDATE_AND_CONTINUE");
    expect(html).toContain("Order Core mới quyết định trạng thái đơn");

    // Core callback outcome is reported truthfully, including a 422.
    expect(html).toContain("422");
    expect(html).toContain("REJECTED_STALE");

    // Evidence, audit and correlation.
    expect(html).toContain("evidence://ivr/e2e/result");
    expect(html).toContain("audit://ivr/e2e/result");
    expect(html).toContain("corr-e2e-gh");

    // Opaque Core order state is displayed, never a control.
    expect(html).toContain("CONFIRMING");
    expect(html).toContain("Giao diện này không chuyển trạng thái đơn hàng");
  });

  it("renders a typed envelope instead of a crash when the job is unknown", async () => {
    const cookie = await signedInCookie();
    const response = await getHtml("/calls/JOB-DOES-NOT-EXIST", cookie);
    const html = await response.text();

    expect(response.status).toBe(200);
    expect(html).toContain("IVR_NOT_FOUND");
    expect(html).toContain("Không tìm thấy tài nguyên.");
  });

  it("hides both admin actions from a viewer and offers them to an admin", async () => {
    const viewerHtml = await (await getHtml(`/calls/${JOB_ID}`, await signedInCookie("AGT-VIEWER-01"))).text();
    expect(viewerHtml).not.toContain("Yêu cầu gọi lại kỹ thuật");
    expect(viewerHtml).not.toContain("Ghi kết luận duyệt");

    const adminHtml = await (await getHtml(`/calls/${JOB_ID}`, await signedInCookie())).text();
    expect(adminHtml).toContain("Ghi kết luận duyệt");
  });
});
