#!/usr/bin/env node
// W-0043 / P7-1 §8. Checks the container images as artifacts, not the code inside them.
//
// These questions cannot be answered by any .NET test: whether the published image runs as root,
// whether its healthcheck actually reports healthy, whether the dev stack comes up with no route
// out, whether the scan gate still fails on a HIGH -- and whether a task put in at one end of the
// stack comes out the other as a callback Sales accepted. A green unit suite says nothing about
// any of them.
//
// IT-IMG-E2E-05 is the one that earned its place. The first four checks all passed on a stack that
// could not complete a single call: the worker image threw on the first Vietnamese number it
// formatted, the dial-token vault could not resolve across two processes, the callback payload was
// stored in a column that reorders JSON keys and so never matched its own hash, and the fake Sales
// answered a path IVR does not call. Four defects, every one of them invisible to a health check.
//
// Run: node deploy/ci/scripts/image-selftest.mjs [--skip-compose] [--skip-scan] [--skip-e2e]
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const skipCompose = process.argv.includes("--skip-compose");
const skipScan = process.argv.includes("--skip-scan");
const skipEndToEnd = process.argv.includes("--skip-e2e");

const TAG = process.env.IVR_IMAGE_TAG ?? "p7-1-selftest";
const IMAGES = [
  { name: "ivr-api", dockerfile: "deploy/docker/Dockerfile.api", context: ".", probe: "/health/live", port: 8080 },
  { name: "ivr-worker", dockerfile: "deploy/docker/Dockerfile.worker", context: ".", probe: null, port: null },
  { name: "ivr-admin-ui", dockerfile: "deploy/docker/Dockerfile.ui", context: "admin-ui", probe: "/login", port: 3000 },
];

function docker(args, options = {}) {
  return execFileSync("docker", args, {
    cwd: repositoryRoot,
    encoding: "utf8",
    stdio: options.inherit ? "inherit" : ["ignore", "pipe", "pipe"],
    maxBuffer: 64 * 1024 * 1024,
    ...options,
  });
}

// Portable blocking sleep. Shelling out to `sleep` or `timeout` is platform-specific and the
// Windows one needs a console it does not have when stdio is redirected.
function sleepSeconds(seconds) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, seconds * 1000);
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

// ---------------------------------------------------------------- IT-IMG-BUILD-01
function buildAndCheckUser() {
  for (const image of IMAGES) {
    docker(["build", "-f", image.dockerfile, "-t", `${image.name}:${TAG}`, image.context], { inherit: true });

    const user = docker(["inspect", "--format", "{{.Config.User}}", `${image.name}:${TAG}`]).trim();
    assert(user !== "", `${image.name} declares no USER, so it runs as root (P7-1 §11).`);
    assert(
      user !== "root" && user !== "0" && !user.startsWith("0:"),
      `${image.name} runs as ${user}.`,
    );

    // A safe default has to be IN the image. An orchestrator that forgets to set it must still get
    // a container that will not call a real customer.
    const env = JSON.parse(docker(["inspect", "--format", "{{json .Config.Env}}", `${image.name}:${TAG}`]));
    if (image.name !== "ivr-admin-ui") {
      assert(
        env.includes("REAL_CUSTOMER_CALL_ALLOWED=NO"),
        `${image.name} does not default REAL_CUSTOMER_CALL_ALLOWED to NO.`,
      );
      assert(
        env.includes("IVR_ADAPTER_MODE=MOCK"),
        `${image.name} does not default IVR_ADAPTER_MODE to MOCK.`,
      );
    }

    const size = Number(docker(["image", "inspect", "--format", "{{.Size}}", `${image.name}:${TAG}`]).trim());
    process.stdout.write(`  ${image.name}: USER=${user}, ${Math.round(size / 1048576)} MB\n`);
  }
  process.stdout.write("IT-IMG-BUILD-01 PASS — three images build, none runs as root\n");
}

// ---------------------------------------------------------------- IT-IMG-HEALTH-02
function checkHealthcheck() {
  const container = `ivr-selftest-health-${Date.now()}`;
  try {
    docker([
      "run", "-d", "--name", container, "-P",
      "-e", "IVR_INTERNAL_SERVICE_TOKEN=selftest-internal-token-not-a-real-secret",
      "-e", "ORDER_CORE_SERVICE_TOKEN=selftest-ordercore-token-not-a-real-secret",
      // Deliberately unreachable: liveness must not depend on a database, or a dependency outage
      // restarts every pod (P6-1 §1).
      "-e", "ConnectionStrings__IvrDb=Host=127.0.0.1;Port=1;Database=absent;Username=none;Password=none;Timeout=1",
      `ivr-api:${TAG}`,
    ]);

    let status = "";
    for (let attempt = 0; attempt < 40; attempt += 1) {
      status = docker(["inspect", "--format", "{{if .State.Health}}{{.State.Health.Status}}{{else}}none{{end}}", container]).trim();
      if (status === "healthy" || status === "unhealthy") break;
      sleepSeconds(2);
    }
    assert(status === "healthy", `ivr-api healthcheck reported '${status}', not healthy.`);
    process.stdout.write("IT-IMG-HEALTH-02 PASS — /health/live answers healthy with no database behind it\n");
  } finally {
    try { docker(["rm", "-f", container]); } catch { /* already gone */ }
  }
}

// ---------------------------------------------------------------- IT-IMG-COMPOSE-03
function checkCompose() {
  const compose = ["compose", "-f", "docker-compose.dev.yml"];
  try {
    docker([...compose, "up", "-d", "--build"], { inherit: true });

    // Bounded wait rather than a fixed sleep: the api healthcheck has a start period, and pinning
    // this to one duration would be flaky on a slower machine while proving nothing on a fast one.
    let services = [];
    for (let attempt = 0; attempt < 40; attempt += 1) {
      services = JSON.parse(`[${docker([...compose, "ps", "--format", "json"]).trim().split("\n").filter(Boolean).join(",")}]`);
      const waiting = services.find((service) => service.Service === "ivr-api");
      if (waiting?.Health === "healthy" || waiting?.Health === "unhealthy") break;
      sleepSeconds(3);
    }
    for (const required of ["ivr-api", "ivr-worker", "ivr-admin-ui", "fake-sales", "postgres"]) {
      const found = services.find((service) => service.Service === required);
      assert(found, `compose is missing ${required}.`);
      assert(found.State === "running", `${required} is ${found.State}, not running.`);
    }

    const api = services.find((service) => service.Service === "ivr-api");
    assert(api.Health === "healthy", `ivr-api is ${api.Health}, not healthy, inside compose.`);

    // The guarantee that matters: the network the fakes live on has no route out. Asserted by
    // trying, not by reading `internal: true` back out of the file we just wrote.
    const network = docker(["network", "ls", "--format", "{{.Name}}"])
      .split("\n").map((line) => line.trim()).find((line) => line.endsWith("_ivr-internal"));
    assert(network, "the internal network is missing from the stack.");

    let escaped = false;
    try {
      docker(["run", "--rm", "--network", network, "busybox:1.37.0-uclibc", "wget", "-q", "-T", "6", "-O-", "http://example.com"]);
      escaped = true;
    } catch { /* the refusal is the pass */ }
    assert(!escaped, `${network} reached the internet; the fakes are not isolated (P7-1 §6.5).`);

    process.stdout.write("IT-IMG-COMPOSE-03 PASS — stack healthy, fake-sales network has no egress\n");
  } finally {
    try { docker([...compose, "down", "-v"], { inherit: true }); } catch { /* best effort */ }
  }
}

// ---------------------------------------------------------------- IT-IMG-SCAN-04
function checkScan() {
  const trivy = ["run", "--rm", "-v", "/var/run/docker.sock:/var/run/docker.sock", "aquasec/trivy:0.58.1",
    "image", "--scanners", "vuln", "--severity", "HIGH,CRITICAL", "--exit-code", "1", "--quiet"];

  for (const image of IMAGES) {
    docker([...trivy, `${image.name}:${TAG}`], { inherit: true });
  }

  // Positive control. A scanner that never fails is indistinguishable from one that is broken, so
  // the gate is proven against an image with a known-vulnerable base before it is trusted.
  let caught = false;
  try {
    docker([...trivy, "alpine:3.10"], { stdio: ["ignore", "ignore", "ignore"] });
  } catch {
    caught = true;
  }
  assert(caught, "trivy passed a base with known HIGH/CRITICAL findings; the scan gate is not wired.");

  process.stdout.write("IT-IMG-SCAN-04 PASS — three images clean, and the gate still fails on a known-bad base\n");
}


// ------------------------------------------------------------------- IT-IMG-E2E-05
//
// Three tasks, all the way through: intake -> eligibility -> scheduler dispatch -> mock SIM ->
// normalization -> callback outbox -> fake Sales, across both programmes and the three DT-02
// branches that give Sales different instructions. Asserted at BOTH ends, because either one alone
// can be satisfied by a stack that does not work: the database row says IVR believes it delivered,
// the WireMock journal says Sales believes it received. Only the pair says a request crossed the
// gap between them.
//
// The overlay is what makes the stack able to dial at all. Everything the worker does ships
// disabled, which is right for `docker compose up` and useless for a smoke, so the E2E posture
// lives in docker-compose.e2e.yml where it can be read in one screen.

const COMPOSE_E2E = ["compose", "-f", "docker-compose.dev.yml", "-f", "docker-compose.e2e.yml"];
const ORDER_CORE_TOKEN = "dev-ordercore-token-not-a-real-secret";
const INTERNAL_TOKEN = "dev-internal-token-not-a-real-secret";

// curl runs from a container ON the internal network rather than from the host: fake-sales is not
// published, and reaching it from outside would measure a different topology than the one shipped.
function curlInternal(args) {
  const network = docker(["network", "ls", "--format", "{{.Name}}"])
    .split("\n").map((line) => line.trim()).find((line) => line.endsWith("_ivr-internal"));
  assert(network, "the internal network is missing from the stack.");
  return docker(["run", "--rm", "--network", network, "curlimages/curl:8.11.1", "-s", ...args]);
}

function psql(sql) {
  return docker([...COMPOSE_E2E, "exec", "-T", "postgres",
    "psql", "-U", "ivr", "-d", "ivr", "-tAc", sql]).trim();
}

function apiPost(routePath, body, headers) {
  const flags = Object.entries(headers).flatMap(([name, value]) => ["-H", `${name}: ${value}`]);
  return docker(["run", "--rm", "--network", "host", "curlimages/curl:8.11.1", "-s",
    "-X", "POST", "-H", "Content-Type: application/json", ...flags,
    "--data-binary", body,
    `http://127.0.0.1:${process.env.IVR_API_PORT ?? "58080"}${routePath}`]);
}

// Three cases, and the set is chosen rather than convenient. P7-1 section 8 asks for BOTH
// programmes, and the DT-02 taxonomy is the thing Sales acts on, so one row per branch that leads
// to a different instruction for Sales:
//
//   CONFIRM   Golden Hour / ONLINE     DTMF 1       -> confirm the order
//   CANCEL    24/7 / COD               DTMF 0       -> cancel at the customer's request
//   NOANSWER  Golden Hour / ONLINE     rings out    -> change NOTHING, wait for the timeout
//
// The third is the one worth the extra policy. TargetV1CallbackTransport refuses to send an
// IVR_NO_ANSWER_FINAL that asks for any state transition -- IVR does not get to expire an order,
// Sales' timeout worker owns that -- and until now that refusal had never run in a container.
const E2E_CASES = [
  {
    taskId: "TASK-E2E-CONFIRM",
    program: "GOLDEN_HOUR",
    payment: "ONLINE",
    policy: "mock-lab-v1",
    windowSeconds: 300,
    offsets: [0, 150],
    maxAttempts: 2,
    expectedResult: "IVR_CONFIRMED",
    expectedAction: "CORE_REVALIDATE_AND_CONFIRM_ORDER",
    expectedCounted: true,
  },
  {
    taskId: "TASK-E2E-CANCEL",
    program: "TWENTY_FOUR_SEVEN",
    payment: "COD",
    policy: "mock-lab-v1",
    windowSeconds: 900,
    offsets: [0, 450],
    maxAttempts: 2,
    expectedResult: "IVR_CUSTOMER_CANCELLED",
    expectedAction: "CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST",
    expectedCounted: true,
  },
  {
    taskId: "TASK-E2E-NOANSWER",
    program: "GOLDEN_HOUR",
    payment: "ONLINE",
    policy: "mock-e2e-single-v1",
    windowSeconds: 300,
    offsets: [0],
    maxAttempts: 1,
    expectedResult: "IVR_NO_ANSWER_FINAL",
    expectedAction: "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
    // A ring-out still spends a customer attempt. Counting it is what makes "two attempts" mean
    // two chances the customer had, rather than two times the line happened to work.
    expectedCounted: true,
  },
];

function confirmationTask(testCase) {
  // The window length is not free: TaskIntakeService rejects the task unless
  // (expires - started) equals the stored policy's confirmation window exactly, so it is derived
  // from `started` rather than from now. Starting a minute in the past is what makes attempt
  // offset 0 already due when the scheduler first looks.
  const startedAt = Date.now() - 60_000;
  const started = new Date(startedAt).toISOString().replace(/\.\d+Z$/, "Z");
  const expires = new Date(startedAt + testCase.windowSeconds * 1000)
    .toISOString().replace(/\.\d+Z$/, "Z");
  const suffix = testCase.taskId.replace("TASK-E2E-", "");
  return {
    contract_version: "ivr-order-confirmation.v1",
    task_id: testCase.taskId,
    correlation_id: `corr-e2e-${suffix}`,
    created_at: started,
    order_id: `ORDER-E2E-${suffix}`,
    order_code: `GF-E2E-${suffix}`,
    order_code_short: "E2E001",
    order_version: "17",
    order_state: "CONFIRMING",
    payment_method_snapshot: testCase.payment,
    ivr_confirmation_required: true,
    is_ivr_callable: true,
    program_code: testCase.program,
    confirmation_window_started_at: started,
    confirmation_window_expires_at: expires,
    attempt_policy_version: testCase.policy,
    max_customer_attempts: testCase.maxAttempts,
    attempt_offsets_seconds: testCase.offsets,
    // Reference and mask only, and the mask sits in the reserved test range. A dev stack is
    // exactly where a real number gets pasted once and then lives in git forever.
    phone_ref: `phone-ref-e2e-${suffix}`,
    phone_masked: "84xxxxx0001",
    phone_validation_status: "VALID",
    dial_token: `dial-token-e2e-${suffix}`,
    dial_token_expires_at: expires,
    privacy_safe_order_summary: {
      customer_display_name: "chị An",
      order_code_short: "E2E001",
      items: [{ public_name: "Nước hồng sâm", quantity: 2, unit_label: "hộp" }],
      total_amount: 560000,
      currency: "VND",
      delivery_area_short: "Phường Bến Nghé, Quận Một",
      program_display_name: testCase.program === "GOLDEN_HOUR" ? "Giờ Vàng" : "Hai mươi tư trên bảy",
      locale: "vi-VN",
    },
    call_restriction: false,
    sellable_status: [{
      sku_id: "SKU-E2E-1", decision: "SELLABLE", captured_at: started,
      recall_hold: false, sale_lock: false, quality_hold: false,
      stock_available: true, batch_released: true, trace_ready: true,
    }],
    eligibility_snapshot: {
      decision: "ELIGIBLE",
      source_version: "sales-eligibility-v1",
      captured_at: started,
      source_available: true,
      blockers: [],
      voice_restriction: { restricted: false, source_available: true, source_version: "sales-voice-v1" },
    },
    evidence_ref: `evidence://compose/e2e-${suffix}`,
  };
}

function driveCase(testCase) {
  const suffix = testCase.taskId.replace("TASK-E2E-", "");
  const correlation = `corr-e2e-${suffix}`;

  const intake = JSON.parse(apiPost(
    "/v1/ivr/order-confirmation/tasks",
    JSON.stringify(confirmationTask(testCase)),
    {
      "X-Source-System": "order-core",
      Authorization: `Bearer ${ORDER_CORE_TOKEN}`,
      "X-Correlation-Id": correlation,
      "Idempotency-Key": `idem-e2e-${suffix}`,
    }));
  // MOCK accepts DRY_RUN_ONLY by design (DTS-04): the mode never mints a live call job, and a
  // check that demanded ACCEPTED_CALL_JOB_CREATED here would be demanding the safety go away.
  assert(
    intake.decision === "TASK_ACCEPTED_DRY_RUN_ONLY",
    `${testCase.taskId}: intake returned ${JSON.stringify(intake)}.`);

  const eligibility = JSON.parse(apiPost(
    "/v1/ivr/order-confirmation/eligibility-checks",
    JSON.stringify({ task_id: testCase.taskId }),
    {
      "X-Source-System": "ivr-worker",
      "X-Service-Scope": "ivr.internal.write",
      Authorization: `Bearer ${INTERNAL_TOKEN}`,
      "X-Correlation-Id": correlation,
      "Idempotency-Key": `idem-elig-${suffix}`,
    }));
  assert(
    eligibility.decision === "ELIGIBLE_FOR_IVR",
    `${testCase.taskId}: eligibility returned ${JSON.stringify(eligibility)}.`);

  // Bounded wait on the terminal states rather than a fixed sleep: dispatch, normalization and
  // delivery are three separate poll loops and their combined latency is not a constant.
  let outcome = "";
  for (let attempt = 0; attempt < 90; attempt += 1) {
    outcome = psql(
      "SELECT COALESCE((SELECT r.result_type FROM ivr_call_results r "
      + "JOIN ivr_call_jobs j ON j.ivr_call_job_id = r.ivr_call_job_id "
      + `WHERE j.task_id = '${testCase.taskId}' AND r.is_final_for_ivr IS TRUE `
      + "ORDER BY r.created_at DESC LIMIT 1), 'none') || '|' || "
      + "COALESCE((SELECT c.delivery_status FROM ivr_result_callbacks c "
      + `WHERE c.task_id = '${testCase.taskId}' LIMIT 1), 'none')`);
    if (outcome.endsWith("|DELIVERED_ACCEPTED") || outcome.includes("DEAD_LETTER")) break;
    sleepSeconds(2);
  }
  assert(
    outcome === `${testCase.expectedResult}|DELIVERED_ACCEPTED`,
    `${testCase.taskId}: ended at ${outcome}, expected `
    + `${testCase.expectedResult}|DELIVERED_ACCEPTED. A capacity or technical result means the `
    + "stack accepted the task and then could not complete it.");

  return correlation;
}

function checkEndToEnd() {
  try {
    docker([...COMPOSE_E2E, "up", "-d", "--build"], { inherit: true });
    for (let attempt = 0; attempt < 60; attempt += 1) {
      const services = JSON.parse(`[${docker([...COMPOSE_E2E, "ps", "--format", "json"])
        .trim().split("\n").filter(Boolean).join(",")}]`);
      const api = services.find((service) => service.Service === "ivr-api");
      const sales = services.find((service) => service.Service === "fake-sales");
      if (api?.Health === "healthy" && sales?.Health === "healthy") break;
      sleepSeconds(3);
    }

    // The seed the dev stack was missing. Applied here rather than baked into the compose file, so
    // that `docker compose up` keeps meaning what it meant before this check existed.
    docker([...COMPOSE_E2E, "exec", "-T", "postgres", "psql", "-U", "ivr", "-d", "ivr",
      "-v", "ON_ERROR_STOP=1"],
      {
        // stdio has to be named: the docker() default is ["ignore", ...], which silently discards
        // `input` -- the seed appeared to apply and the stack then failed with
        // ATTEMPT_POLICY_NOT_FOUND several steps later.
        stdio: ["pipe", "pipe", "pipe"],
        input: readFileSync(path.join(repositoryRoot, "deploy/docker/dev-seed/seed.sql"), "utf8"),
      });

    // Sequential, not parallel. One SIM channel is seeded on purpose: with a pool, a scheduling
    // defect could hide behind a spare channel, and three cases sharing one channel also proves
    // the lease is released between calls.
    const correlations = new Map(E2E_CASES.map((testCase) => [testCase.taskId, driveCase(testCase)]));

    // The other end. IVR believing it delivered and Sales having received are two different
    // claims, and the point of this check is that they were not the same claim for a long time.
    const journal = JSON.parse(curlInternal(["http://fake-sales:8080/__admin/requests?limit=100"]));
    const delivered = journal.requests
      .filter((entry) => entry.request.url.includes("/ivr-result-callbacks"))
      .map((entry) => ({ headers: entry.request.headers, body: JSON.parse(entry.request.body) }));

    for (const testCase of E2E_CASES) {
      const received = delivered.find((entry) => entry.body.task_id === testCase.taskId);
      assert(received, `fake Sales never received a callback for ${testCase.taskId}.`);
      assert(
        received.body.result_type === testCase.expectedResult
        && received.body.recommended_core_action === testCase.expectedAction
        && received.body.is_final_for_ivr === true
        && received.body.is_counted_customer_attempt === testCase.expectedCounted,
        `${testCase.taskId}: the delivered callback carries the wrong taxonomy: `
        + `${JSON.stringify(received.body)}.`);
      assert(
        received.headers["X-Correlation-Id"] === correlations.get(testCase.taskId),
        `${testCase.taskId}: the callback lost its correlation id between intake and Sales.`);
    }

    // Each case must have arrived exactly once. A retry that duplicated a signal would still
    // satisfy every assertion above, and a duplicate confirmation is not a harmless one.
    assert(
      delivered.length === E2E_CASES.length,
      `fake Sales received ${delivered.length} callbacks for ${E2E_CASES.length} tasks.`);

    process.stdout.write(
      `IT-IMG-E2E-05 PASS — ${E2E_CASES.length} tasks crossed the whole stack on one SIM channel: `
      + `${E2E_CASES.map((testCase) => `${testCase.program.split("_")[0]}/${testCase.expectedResult}`)
        .join(", ")}, each ACCEPTED by fake Sales exactly once with its correlation id intact\n`);
  } finally {
    try { docker([...COMPOSE_E2E, "down", "-v"], { inherit: true }); } catch { /* best effort */ }
  }
}

buildAndCheckUser();
checkHealthcheck();
if (!skipCompose) checkCompose();
if (!skipScan) checkScan();
if (!skipEndToEnd) checkEndToEnd();
process.stdout.write("IMAGE_SELFTEST_PASS\n");
