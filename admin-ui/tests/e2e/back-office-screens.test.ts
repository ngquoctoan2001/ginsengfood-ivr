// @vitest-environment node
import { spawn, type ChildProcess } from "node:child_process";
import { createServer as createHttpServer, type Server } from "node:http";
import { createServer as createNetServer } from "node:net";
import { fileURLToPath } from "node:url";

import { afterAll, beforeAll, describe, expect, it } from "vitest";

import {
  ADMIN_USERNAME,
  OPERATOR_USERNAME,
  handleConsoleAuthStub,
  signInBody,
} from "./console-auth-stub";

const projectRoot = fileURLToPath(new URL("../../", import.meta.url));
const nextBin = fileURLToPath(new URL("../../node_modules/next/dist/bin/next", import.meta.url));

/**
 * E2E-UI-REVIEW-05.
 *
 * The P3-3 prompt's §6.4 and its `E2E-UI-REPLAY-05` describe a callback replay
 * control, but `specs/ui/06` — the source spec the prompt itself lists first —
 * defines UI-06 as the human review queue with `POST /admin-reviews` and
 * `POST /technical-retries`, and no replay operation exists anywhere in the API.
 * This suite follows the spec: it asserts the review queue works end to end and
 * that no resend or replay control is offered.
 *
 * It also covers the production guard, which needs a second server started with
 * a production environment label.
 */

interface CapturedFlagMutation {
  readonly actorHeader: string;
  readonly idempotencyKey: string;
  readonly body: Record<string, unknown>;
}

const flagMutations: CapturedFlagMutation[] = [];

const FLAG_SNAPSHOT = {
  environment: "dev",
  revision: 41,
  executionMode: "MOCK",
  salesProvider: "FAKE_TARGET_V1",
  simProvider: "MOCK",
  attemptPolicyVersion: "mock-lab-v1",
  realCustomerCallAllowed: false,
  labDestinationAllowlist: ["LAB-A"],
  globalDialKillSwitch: true,
  v1NotificationEnabled: false,
  recordingEnabled: false,
} as const;

const FLAG_READ_RESULT = {
  snapshot: FLAG_SNAPSHOT,
  providerReadable: true,
  fromCache: false,
} as const;

const KILL_SWITCH_ENGAGED = {
  providerReadable: true,
  revision: 41,
  globalDialKillSwitch: true,
  realCallsEnabled: false,
} as const;

const FLAG_MUTATION_RESULT = {
  snapshot: { ...FLAG_SNAPSHOT, revision: 42 },
  approvedBy: null,
  increasedRiskKeys: [],
} as const;

let apiServer: Server | undefined;
let devServer: ChildProcess | undefined;
let prodServer: ChildProcess | undefined;
let devUrl = "";
let prodUrl = "";

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

const SCRIPT_CATALOG = {
  generated_at: "2026-08-15T02:00:00Z",
  execution_mode: "MOCK",
  production_target_v1_fields_approved: false,
  allowed_input_fields: ["customer_display_name", "order_code_short"],
  prohibited_variables: ["FULL_ADDRESS", "PAYMENT_DETAIL", "HEALTH"],
  dtmf_map: [
    { key: "1", meaning: "CONFIRM", enabled: true },
    { key: "0", meaning: "CANCEL", enabled: true },
    { key: "9", meaning: "NOT_ENABLED", enabled: false },
  ],
  required_approval_types: ["MOCK_TEST", "LAB", "CONTENT", "PRIVACY_LEGAL"],
  versions: [
    {
      template_id: "SCRIPT-ORDER-CONFIRM",
      version: "v1-approved",
      status: "APPROVED",
      template_hash: "a".repeat(64),
      allowed_input_fields: ["order_code_short"],
      approvals: [
        {
          approval_type: "MOCK_TEST",
          actor_id: "approver-mock",
          reason: "ok",
          correlation_id: "corr-1",
          approved_at: "2026-08-15T01:00:00Z",
        },
        {
          approval_type: "LAB",
          actor_id: "approver-lab",
          reason: "lab ok",
          correlation_id: "corr-2",
          approved_at: "2026-08-15T01:10:00Z",
        },
      ],
      missing_approvals: ["CONTENT", "PRIVACY_LEGAL"],
      template_valid: true,
      uses_production_decision_fields: true,
      created_by: "author-01",
      created_at: "2026-08-15T00:00:00Z",
    },
    {
      template_id: "SCRIPT-ORDER-CONFIRM",
      version: "v2-draft",
      status: "DRAFT",
      template_hash: "b".repeat(64),
      allowed_input_fields: [],
      approvals: [],
      missing_approvals: ["MOCK_TEST", "LAB", "CONTENT", "PRIVACY_LEGAL"],
      template_valid: false,
      uses_production_decision_fields: false,
      created_by: "author-02",
      created_at: "2026-08-15T00:30:00Z",
    },
  ],
};

const INTEGRATION_STATUS = {
  generated_at: "2026-08-15T02:00:00Z",
  execution_mode: "MOCK",
  sales_provider: "FAKE_TARGET_V1",
  sim_provider: "MOCK",
  real_customer_call_allowed: false,
  global_dial_kill_switch: true,
  attempt_policy_version: "mock-lab-v1",
  flag_revision: 3,
  dependency_probing_available: false,
  dependencies: [
    {
      dependency: "SIM_GATEWAY",
      state: "UP",
      detail: "provider=MOCK; channels 1/1 enabled",
      detail_vi: "Nhà cung cấp=MOCK; 1/1 kênh đang bật",
      fail_closed_effect: "SIM down maps to IVR_TECHNICAL_EXCEPTION.",
      observed: true,
      captured_at: "2026-08-15T01:59:00Z",
    },
    {
      dependency: "ORDER_CORE",
      state: "NOT_WIRED",
      detail: "No provider endpoint configured.",
      detail_vi: "Nhà cung cấp=chưa cấu hình; endpoint thật vẫn BLOCKED_EXTERNAL",
      fail_closed_effect: "Order Core down means no new task.",
      observed: false,
    },
  ],
  recent_fail_closed_events: [
    {
      source: "CAPACITY_INCIDENT",
      reference: "INCIDENT-E2E-CFG",
      effect: "SCHEDULER_DEADLINE: open, dispatch not held",
      hold_new_calls: false,
      correlation_id: "corr-cfg-incident",
      occurred_at: "2026-08-15T01:00:00Z",
    },
  ],
};

const REVIEW_QUEUE = {
  page: 1,
  page_size: 25,
  total_count: 1,
  items: [
    {
      review_item_id: "REVIEW-E2E-CFG",
      source_type: "IVR_CALL_RESULT",
      source_id: "RESULT-E2E-CFG",
      reason: "verify confirmed evidence",
      status: "OPEN",
      correlation_id: "corr-e2e-cfg",
      ivr_call_job_id: "JOB-E2E-CFG",
      order_code_short: "GF-CFG",
      result_type: "IVR_CONFIRMED",
      created_at: "2026-08-15T01:30:00Z",
    },
  ],
};

function startNext(port: number, apiPort: number, environmentLabel: string): ChildProcess {
  return spawn(process.execPath, [nextBin, "start", "--port", String(port)], {
    cwd: projectRoot,
    env: {
      ...process.env,
      NODE_ENV: "production",
      IVR_EXECUTION_MODE: "MOCK",
      IVR_ENVIRONMENT_LABEL: environmentLabel,
      REAL_CUSTOMER_CALL_ALLOWED: "NO",
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
  apiServer = createHttpServer(async (request, response) => {
    if (await handleConsoleAuthStub(request, response)) return;
    const url = new URL(request.url ?? "/", "http://127.0.0.1");
    const base = "/v1/ivr/order-confirmation";

    // W-0110. The mutation is captured rather than route-mapped, because what a
    // console test can actually prove about an audited change is what the
    // console sent: the actor header, and a reason. Whether the server writes
    // the audit row is IT-FLAG-EMERGENCY-10's job, against a real database.
    if (url.pathname === `${base}/feature-flags/dev` && request.method === "POST") {
      const chunks: Buffer[] = [];
      for await (const chunk of request) {
        chunks.push(chunk as Buffer);
      }

      flagMutations.push({
        actorHeader: String(request.headers["x-actor-id"] ?? ""),
        idempotencyKey: String(request.headers["idempotency-key"] ?? ""),
        body: JSON.parse(Buffer.concat(chunks).toString("utf8")) as Record<string, unknown>,
      });
      response.writeHead(200, { "Content-Type": "application/json" });
      response.end(JSON.stringify(FLAG_MUTATION_RESULT));
      return;
    }

    const routes: Record<string, unknown> = {
      [`${base}/scripts`]: SCRIPT_CATALOG,
      [`${base}/integration-status`]: INTEGRATION_STATUS,
      [`${base}/review-items`]: REVIEW_QUEUE,
      [`${base}/feature-flags/dev`]: FLAG_READ_RESULT,
      [`${base}/feature-flags/dev/kill-switch`]: KILL_SWITCH_ENGAGED,
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

  const devPort = await findFreePort();
  const prodPort = await findFreePort();
  devUrl = `http://127.0.0.1:${devPort}`;
  prodUrl = `http://127.0.0.1:${prodPort}`;
  devServer = startNext(devPort, apiPort, "dev");
  prodServer = startNext(prodPort, apiPort, "production");

  await waitFor(`${devUrl}/login`);
  await waitFor(`${prodUrl}/login`);
});

afterAll(async () => {
  devServer?.kill();
  prodServer?.kill();
  await new Promise<void>((resolve) => {
    if (apiServer === undefined) {
      resolve();
      return;
    }

    apiServer.close(() => resolve());
  });
});

async function signedIn(baseUrl: string, actorId = ADMIN_USERNAME): Promise<string> {
  const response = await fetch(`${baseUrl}/api/auth/sign-in`, {
    method: "POST",
    redirect: "manual",
    body: signInBody(actorId),
  });
  const header = response.headers
    .getSetCookie()
    .find((cookie) => cookie.startsWith("ivr_admin_session="));
  if (header === undefined) {
    throw new Error("sign-in did not issue a session");
  }

  return header.split(";")[0];
}

function page(baseUrl: string, path: string, cookie: string): Promise<Response> {
  return fetch(`${baseUrl}${path}`, { headers: { Cookie: cookie }, redirect: "manual" });
}

describe("E2E-UI-REVIEW-05 review queue and back-office screens", () => {
  it("lists review items and links each one into its call detail", async () => {
    const cookie = await signedIn(devUrl);
    const html = await (await page(devUrl, "/review", cookie)).text();

    expect(html).toContain("Hàng đợi chờ duyệt");
    expect(html).toContain("REVIEW-E2E-CFG");
    expect(html).toContain("GF-CFG");
    expect(html).toContain("IVR_CONFIRMED");
    expect(html).toContain("/calls/JOB-E2E-CFG");
  });

  it("offers no replay or resend control anywhere in the console", async () => {
    const cookie = await signedIn(devUrl);
    const paths = ["/review", "/config", "/integration", "/seed", "/roles"];

    // Assert on rendered controls, not prose and not the RSC flight payload:
    // the review screen legitimately *says* that no replay exists, and the
    // serialized payload repeats every string as data.
    const controlPattern =
      /<(?:button|a)[^>]*>(?:(?!<\/(?:button|a)>)[\s\S])*?(?:gửi lại|replay|resend)/gi;

    for (const path of paths) {
      const html = await (await page(devUrl, path, cookie)).text();
      const markup = html.replace(/<script[\s\S]*?<\/script>/gi, "");
      expect(markup.match(controlPattern), path).toBeNull();
    }

    // And the review screen says so explicitly.
    const reviewHtml = await (await page(devUrl, "/review", cookie)).text();
    expect(reviewHtml).toContain("Không có chức năng gửi lại callback từ giao diện");
  });

  it("shows script approval state, the KEY_9 lock and the OD-V1-15 lock", async () => {
    const cookie = await signedIn(devUrl);
    const html = await (await page(devUrl, "/config", cookie)).text();

    expect(html).toContain("v1-approved");
    expect(html).toContain("v2-draft");
    expect(html).toContain("Trạng thái vòng đời");
    expect(html).toContain("Đã phê duyệt");
    expect(html).toContain("Còn thiếu để duyệt đủ");
    expect(html).not.toContain("Đã duyệt đủ");
    expect(html).not.toContain("Chưa duyệt đủ");
    // W-0107. The approval types render as Vietnamese labels now, so the
    // assertion matches on `data-enum-code` rather than on the prose: rewording
    // the dictionary is not supposed to be able to turn this test red.
    for (const approval of ["MOCK_TEST", "LAB", "CONTENT", "PRIVACY_LEGAL"]) {
      expect(html).toContain(`data-enum-code="${approval}"`);
    }

    expect(html).toContain("Kiểm thử mô phỏng");
    expect(html).toContain("Template không còn hợp lệ");
    expect(html).toContain('data-enum-code="NOT_ENABLED"');
    expect(html).toContain("ProductionTargetV1FieldsApproved=NO");
    // Read-only: no lifecycle control is rendered.
    expect(html).not.toMatch(/<button[^>]*>\s*(Phê duyệt|Gửi duyệt|Thu hồi)/);
  });

  it("labels a 503 dependency fail-closed and an unprobed one as not observed", async () => {
    const cookie = await signedIn(devUrl);
    const html = await (await page(devUrl, "/integration", cookie)).text();

    expect(html).toContain("READY_503");
    expect(html).toContain("fail-closed");
    expect(html).toContain("NOT_WIRED");
    expect(html).toContain("Chưa có thăm dò");
    expect(html).toContain("W-0040");
    expect(html).toContain("Nhà cung cấp=MOCK; 1/1 kênh đang bật");
    expect(html).toContain("provider=MOCK; channels 1/1 enabled");
    expect(html).toContain("Trễ hạn điều phối");
    expect(html).toContain("sự cố đang mở; chưa giữ dispatch");
    // The kill switch is reported as engaged, not hidden.
    expect(html).toContain("ĐANG BẬT");

    // W-0033 / P4-5 §2.5. The console must say V1 notification is off BY DESIGN. Without this
    // line an operator seeing no customer message has no way to tell policy from failure.
    expect(html).toContain("Thông báo tới khách (V1)");
    expect(html).toContain("TẮT theo thiết kế");
    expect(html).not.toContain("thông báo lỗi gửi");
  });

  it("locks seed and mock management in a production environment", async () => {
    const devHtml = await (await page(devUrl, "/seed", await signedIn(devUrl))).text();
    expect(devHtml).toContain("Chế độ thực thi");
    expect(devHtml).toContain("MOCK");
    expect(devHtml).toContain("STATUS-all-up");
    expect(devHtml).not.toContain("bị khoá hoàn toàn");

    const prodHtml = await (await page(prodUrl, "/seed", await signedIn(prodUrl))).text();
    expect(prodHtml).toContain("bị khoá hoàn toàn");
    expect(prodHtml).not.toContain("STATUS-all-up");
    // No adapter figure and therefore no way to read or change the mode here.
    expect(prodHtml).not.toContain("STATUS-order-core-down");
  });

  /**
   * E2E-UI-FLAGS-06 (W-0110).
   *
   * OD-V1-20 moved the P0 constraint from "no role holds this permission" to
   * "every press needs four-eyes, a matching actor and an audit record". These
   * assert the console half of that: the screen renders the effective state,
   * separates the two directions, and sends an actor header the server checks
   * against the subject. The server half — that the audit row is written, and
   * that a mismatched actor is refused — is IT-FLAG-EMERGENCY-10 and
   * UT-FLAG-AUTHZ-05, against a real database.
   */
  it("shows the effective gate state and separates risk-reducing from risk-raising", async () => {
    const cookie = await signedIn(devUrl);
    const html = await (await page(devUrl, "/flags", cookie)).text();

    expect(html).toContain("Cổng vận hành");
    expect(html).toContain('data-testid="kill-switch-state"');
    expect(html).toContain("ĐANG BẬT");

    // Both directions are offered, and the labels say which is which. A screen
    // that listed them together would be the same screen `curl` already was.
    expect(html).toContain("Giảm rủi ro");
    expect(html).toContain("Tăng rủi ro");
    expect(html).toContain("Bật kill switch");
    expect(html).toContain("Nhả kill switch");

    // The allowlist entry is shown, because an operator deciding whether to
    // widen it has to see what is already there.
    expect(html).toContain("LAB-A");
  });

  it("refuses the whole screen to an operator rather than only hiding the buttons", async () => {
    const cookie = await signedIn(devUrl, OPERATOR_USERNAME);
    const response = await page(devUrl, "/flags", cookie);
    const html = await response.text();

    // No gate state at all, not a page with the controls removed: a rendered
    // kill-switch state is itself operational information.
    expect(html).not.toContain('data-testid="kill-switch-state"');
    expect(html).not.toContain("Bật kill switch");
    expect(html).not.toContain("LAB-A");
  });

  it("blocks risk-raising in a production environment and says where it moved", async () => {
    const cookie = await signedIn(prodUrl);
    const html = await (await page(prodUrl, "/flags", cookie)).text();

    expect(html).toContain('data-testid="flags-production-blocked"');
    expect(html).toContain("deployment");
    // Risk reduction stays available in production. An incident is exactly when
    // someone needs to stop dialling, and that is the direction that is safe.
    expect(html).toContain("Bật kill switch");
    expect(html).not.toContain("Nhả kill switch");
  });

  it("presents roles as a reference matrix with no assignment control", async () => {
    const cookie = await signedIn(devUrl);
    const html = await (await page(devUrl, "/roles", cookie)).text();

    expect(html).toContain("Ivr.Api");
    expect(html).toContain("Quản trị viên");
    expect(html).toContain("Nhân viên vận hành");
    expect(html).toContain("IVR_RUNTIME_GATE_ADMIN");
    // OD-V1-20 approved 2026-08-22: the runtime-gate row now names Admin as its holder, and
    // no permission is left ungranted. The matrix stays read-only regardless.
    expect(html).not.toContain('data-testid="ungranted-IVR_RUNTIME_GATE_ADMIN"');
    expect(html).not.toContain('data-testid="ungranted-IVR_FLAG_READ"');
    expect(html).not.toMatch(/<button[^>]*>\s*(Gán|Thu hồi)/);
  });
});
