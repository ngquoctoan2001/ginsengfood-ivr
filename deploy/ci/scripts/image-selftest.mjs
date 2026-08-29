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
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { verifyObservabilityRuntime } from "./observability-runtime-selftest.mjs";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const skipCompose = process.argv.includes("--skip-compose");
const skipScan = process.argv.includes("--skip-scan");
const skipEndToEnd = process.argv.includes("--skip-e2e");
const observabilityRuntime = process.argv.includes("--observability-runtime");

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
  const images = observabilityRuntime
    ? IMAGES.filter((image) => image.name !== "ivr-admin-ui")
    : IMAGES;
  for (const image of images) {
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
  process.stdout.write(
    `IT-IMG-BUILD-01 PASS — ${images.length} images build, none runs as root\n`,
  );
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
// Six tasks, all the way through: intake -> eligibility -> scheduler dispatch -> mock SIM ->
// normalization -> callback outbox -> fake Sales, across both programmes. Asserted at BOTH ends,
// because either one alone can be satisfied by a stack that does not work: the database row says
// IVR believes it delivered, the WireMock journal says Sales believes it received. Only the pair
// says a request crossed the gap between them.
//
// Three groups, and the split carries the meaning rather than tidying a list:
//
//   DIALLED    a call happened, the result is FINAL       -> a callback must arrive
//   SILENT     a call happened, the result is NOT final   -> nothing may reach Sales
//   CAPACITY   no call ever happened, the window closed   -> Sales is told to hold for review
//
// SILENT is the group nothing had ever checked. Only final results enter the outbox --
// ResultRepository asks before it builds one and CallbackOutboxSnapshotFactory throws if asked
// anyway -- and that single condition is the whole reason an intermediate outcome cannot move an
// order. Lose it and Sales hears "no answer" after the FIRST unanswered ring, on a task the
// customer still has a second chance to answer. Every assertion in the DIALLED group would stay
// green while that happened, because none of them can see a callback that should not exist. So the
// silent cases assert an absence, and they wait before believing it: "not yet" is not "never".
//
// CAPACITY is the only path that reaches a final result WITHOUT passing through normalization --
// the scheduler writes the row itself. It is driven the way an operator drives it, by pausing the
// queue through the admin API and letting a window close, rather than by inserting an incident row
// into the database. A fixture that faked the pause would also fake the thing under test, and this
// way the pause has to actually stop dispatch for the case to reach its result at all.
//
// The overlay is what makes the stack able to dial at all. Everything the worker does ships
// disabled, which is right for `docker compose up` and useless for a smoke, so the E2E posture
// lives in docker-compose.e2e.yml where it can be read in one screen.

const COMPOSE_E2E = [
  "compose",
  "-f",
  "docker-compose.dev.yml",
  "-f",
  "docker-compose.e2e.yml",
  ...(observabilityRuntime ? ["-f", "docker-compose.observability.yml"] : []),
];
const ORDER_CORE_TOKEN = "dev-ordercore-token-not-a-real-secret";
const INTERNAL_TOKEN = "dev-internal-token-not-a-real-secret";
const ADMIN_ACTOR = "e2e-smoke-operator";

// Twelve delivery polls at the overlay's 500ms interval. The silent cases assert that nothing
// arrives, and an assertion made the instant the result row appears would pass on a stack whose
// delivery loop is merely slow. Long enough that "nothing yet" means "nothing"; short enough that
// nobody deletes the check to save the time.
const SILENT_SETTLE_SECONDS = 6;

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

function apiRequest(method, routePath, body, headers) {
  const flags = Object.entries(headers).flatMap(([name, value]) => ["-H", `${name}: ${value}`]);
  const payload = body === null ? [] : ["--data-binary", body];
  return docker(["run", "--rm", "--network", "host", "curlimages/curl:8.11.1", "-s",
    "-X", method, "-H", "Content-Type: application/json", ...flags, ...payload,
    `http://127.0.0.1:${process.env.IVR_API_PORT ?? "58080"}${routePath}`]);
}

function apiPost(routePath, body, headers) {
  return apiRequest("POST", routePath, body, headers);
}

// The actor is named TWICE on purpose, and the duplication is the check rather than an artefact:
// the MOCK scheme mints the identity from X-Mock-Actor-Id, then InternalRequestGuard compares it
// against X-Actor-Id and refuses the call if they disagree, so a request cannot be attributed to
// whichever of two names the server happens to prefer.
function adminHeaders(permission, correlation, idempotencyKey) {
  const headers = {
    "X-Mock-Actor-Id": ADMIN_ACTOR,
    "X-Actor-Id": ADMIN_ACTOR,
    "X-Permissions": permission,
    "X-Correlation-Id": correlation,
  };
  if (idempotencyKey) {
    headers["Idempotency-Key"] = idempotencyKey;
  }
  return headers;
}

function queueProjection(correlation) {
  return JSON.parse(apiRequest(
    "GET",
    "/v1/ivr/order-confirmation/queue",
    null,
    adminHeaders("IVR_QUEUE_VIEW", correlation)));
}

// Four DIALLED cases, and the set is chosen rather than convenient. P7-1 section 8 asks for BOTH
// programmes, and the DT-02 taxonomy is the thing Sales acts on, so one row per branch that leads
// to a different instruction for Sales:
//
//   CONFIRM    Golden Hour / ONLINE   DTMF 1              -> confirm the order
//   CANCEL     24/7 / COD             DTMF 0              -> cancel at the customer's request
//   NOANSWER   Golden Hour / ONLINE   rings out           -> change NOTHING, wait for the timeout
//   BADNUMBER  24/7 / COD             cannot be reached   -> hold the order for a person to look
//
// The third is the one worth the extra policy. TargetV1CallbackTransport refuses to send an
// IVR_NO_ANSWER_FINAL that asks for any state transition -- IVR does not get to expire an order,
// Sales' timeout worker owns that -- and until now that refusal had never run in a container.
const E2E_DIALLED_CASES = [
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
  {
    // Sales said the number was valid and the network disagreed, which is the only way this result
    // can arise: eligibility refuses a task whose phone_validation_status is anything else, so a
    // number that fails at DIAL time is a disagreement between two systems rather than bad input.
    taskId: "TASK-E2E-BADNUMBER",
    program: "TWENTY_FOUR_SEVEN",
    payment: "COD",
    policy: "mock-e2e-single-v1",
    windowSeconds: 900,
    offsets: [0],
    maxAttempts: 1,
    expectedResult: "IVR_INVALID_PHONE_FINAL",
    expectedAction: "CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW",
    // NOT counted, and this is the same rule the capacity case rests on reached from the other
    // side: there the queue never dialled, here the dial found nothing to reach. Either way a
    // chance nobody could have taken is not a chance the customer spent.
    expectedCounted: false,
  },
];

// Two SILENT cases, one per programme. Both spend a customer attempt and neither is final, which
// is the pair of properties that makes them dangerous: they look like outcomes and they are not.
//
//   ATTEMPT   Golden Hour / ONLINE   rings out with an attempt left  -> IVR_NO_ANSWER_ATTEMPT
//   WRONGKEY  24/7 / COD             presses 7 with an attempt left  -> IVR_WRONG_INPUT
//
// Both ride a policy whose second attempt is twenty-five minutes out. Under mock-lab-v1 it would
// be 150 seconds, close enough to the smoke's own runtime that a slow machine could dial again
// mid-check, turn the result final and make the absence assertion fail for a reason that has
// nothing to do with what it tests. A flaky negative check is worse than none: it gets deleted.
// So the schedule moves out of reach instead of the clock being raced.
const E2E_SILENT_CASES = [
  {
    taskId: "TASK-E2E-ATTEMPT",
    program: "GOLDEN_HOUR",
    payment: "ONLINE",
    policy: "mock-e2e-silent-v1",
    windowSeconds: 1800,
    offsets: [0, 1500],
    maxAttempts: 2,
    expectedResult: "IVR_NO_ANSWER_ATTEMPT",
    expectedCounted: true,
  },
  {
    taskId: "TASK-E2E-WRONGKEY",
    program: "TWENTY_FOUR_SEVEN",
    payment: "COD",
    policy: "mock-e2e-silent-v1",
    windowSeconds: 1800,
    offsets: [0, 1500],
    maxAttempts: 2,
    expectedResult: "IVR_WRONG_INPUT",
    expectedCounted: true,
  },
];

// The CAPACITY case. Its window has thirty seconds left when it is accepted, and the queue is
// already paused, so the only way it can end is the way the scheduler's deadline sweep ends it.
// Nothing here shortens a window to force the outcome: the policy is the same one the NOANSWER
// case uses, and only the task's own start time is moved back.
const E2E_CAPACITY_CASE = {
  taskId: "TASK-E2E-CAPACITY",
  program: "GOLDEN_HOUR",
  payment: "ONLINE",
  policy: "mock-e2e-single-v1",
  windowSeconds: 300,
  startedSecondsAgo: 270,
  offsets: [0],
  maxAttempts: 1,
  expectedResult: "IVR_CAPACITY_EXCEPTION",
  expectedAction: "CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW",
  // Nobody was ever called, so nobody spent a chance. A capacity miss that counted against the
  // customer would quietly cost them an attempt for an outage that was ours.
  expectedCounted: false,
};

// The TECHNICAL case, and it is less a sixth taxonomy value than a set of promises about what
// happens when the fault is OURS. A technical failure is the one outcome the customer had no part
// in, so four separate things must hold, each recorded in a different table:
//
//   the customer's attempt is NOT spent          ivr_call_attempts
//   Sales is told NOTHING                        ivr_result_callbacks
//   the order's fate is NOT settled              ivr_call_jobs   (parked, never closed)
//   the SIM channel is NOT blamed                ivr_sim_channels
//
// None of the four had ever been checked outside a unit test, and they fail independently.
//
// It rides the ONE-attempt policy deliberately. Two dials happen while the policy allows the
// customer a single call: if either dial counted, that one chance would be gone -- spent entirely
// on our own audio stack failing, without the customer's phone ever ringing usefully.
const E2E_TECHNICAL_CASE = {
  taskId: "TASK-E2E-TECHNICAL",
  program: "GOLDEN_HOUR",
  payment: "ONLINE",
  policy: "mock-e2e-single-v1",
  windowSeconds: 300,
  offsets: [0],
  maxAttempts: 1,
  expectedResult: "IVR_TECHNICAL_EXCEPTION",
};

// Everything that must leave the WireMock journal empty.
const E2E_SILENT_TASKS = [
  ...E2E_SILENT_CASES.map((testCase) => testCase.taskId),
  E2E_TECHNICAL_CASE.taskId,
];

const E2E_DELIVERING_CASES = [...E2E_DIALLED_CASES, E2E_CAPACITY_CASE];

// The control for the capacity case's attempt count. CONFIRM answers on its first attempt and is
// therefore the one case whose attempt total is a known constant: exactly one.
const ATTEMPT_CONTROL_TASK = "TASK-E2E-CONFIRM";

function confirmationTask(testCase) {
  // The window length is not free: TaskIntakeService rejects the task unless
  // (expires - started) equals the stored policy's confirmation window exactly, so it is derived
  // from `started` rather than from now. Starting a minute in the past is what makes attempt
  // offset 0 already due when the scheduler first looks; the capacity case starts further back so
  // that the window runs out while the queue is paused.
  const startedAt = Date.now() - (testCase.startedSecondsAgo ?? 60) * 1000;
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

/** Intake plus eligibility. Returns the correlation id the rest of the case is tracked by. */
function admitCase(testCase, taskBody = confirmationTask(testCase)) {
  const suffix = testCase.taskId.replace("TASK-E2E-", "");
  const correlation = `corr-e2e-${suffix}`;

  const intake = JSON.parse(apiPost(
    "/v1/ivr/order-confirmation/tasks",
    JSON.stringify(taskBody),
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

  return correlation;
}

/** The latest result for a task as `type|final|counted`, or 'none'. */
function latestResult(taskId) {
  return psql(
    "SELECT COALESCE((SELECT r.result_type || '|' || r.is_final_for_ivr::text || '|' "
    + "|| r.is_counted_customer_attempt::text FROM ivr_call_results r "
    + "JOIN ivr_call_jobs j ON j.ivr_call_job_id = r.ivr_call_job_id "
    + `WHERE j.task_id = '${taskId}' ORDER BY r.created_at DESC LIMIT 1), 'none')`);
}

/** Waits until the task holds a final result AND that result has been accepted by Sales. */
function awaitDelivery(testCase, hint) {
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
    + `${testCase.expectedResult}|DELIVERED_ACCEPTED. ${hint}`);
}

function driveDeliveredCase(testCase) {
  const correlation = admitCase(testCase);
  awaitDelivery(
    testCase,
    "A capacity or technical result means the stack accepted the task and then could not "
    + "complete it.");
  return correlation;
}

function proveCollectorOutageDoesNotBreakBusinessFlow(
  testCase,
  taskBody,
  expectedCorrelation,
) {
  docker([...COMPOSE_E2E, "stop", "otel-lgtm"], { inherit: true });

  const liveStatus = docker([
    "run", "--rm", "--network", "host", "curlimages/curl:8.11.1",
    "--silent", "--output", "/dev/null", "--write-out", "%{http_code}",
    `http://127.0.0.1:${process.env.IVR_API_PORT ?? "58080"}/health/live`,
  ]).trim();
  assert(liveStatus === "200", `collector outage changed /health/live to HTTP ${liveStatus}.`);

  const replayCorrelation = admitCase(testCase, taskBody);
  assert(
    replayCorrelation === expectedCorrelation,
    "collector outage changed the idempotent replay correlation id.",
  );
  const journal = JSON.parse(curlInternal(["http://fake-sales:8080/__admin/requests?limit=100"]));
  const received = journal.requests
    .filter((entry) => entry.request.url.includes("/ivr-result-callbacks"))
    .map((entry) => ({ headers: entry.request.headers, body: JSON.parse(entry.request.body) }))
    .filter((entry) => entry.body.task_id === testCase.taskId);
  assert(received.length === 1, "collector outage prevented or duplicated the business callback.");
  assert(
    received[0].headers["X-Correlation-Id"] === expectedCorrelation,
    "collector outage changed the callback correlation id.",
  );

  process.stdout.write(
    "IT-OBS-RESILIENCE-12 PASS — LGTM stopped; liveness stayed 200 and the same MOCK task "
    + "replayed idempotently without duplicating its accepted callback\n",
  );
}

/** A case that must produce a result and then produce NOTHING for Sales. */
function driveSilentCase(testCase) {
  const correlation = admitCase(testCase);

  let outcome = "none";
  for (let attempt = 0; attempt < 60; attempt += 1) {
    outcome = latestResult(testCase.taskId);
    if (outcome !== "none") break;
    sleepSeconds(2);
  }
  assert(
    outcome === `${testCase.expectedResult}|false|${testCase.expectedCounted}`,
    `${testCase.taskId}: normalized to ${outcome}, expected `
    + `${testCase.expectedResult}|false|${testCase.expectedCounted}. A result that came back final `
    + "here means the taxonomy spent the customer's last chance on their first ring.");

  // The absence, given time to fail. Rechecked after the settle rather than only once, so that a
  // delivery loop which is merely late cannot pass as a delivery loop which correctly did nothing.
  sleepSeconds(SILENT_SETTLE_SECONDS);
  assert(
    psql(`SELECT COUNT(*) FROM ivr_result_callbacks WHERE task_id = '${testCase.taskId}'`) === "0",
    `${testCase.taskId}: a NON-final result reached the callback outbox. Sales would be told to `
    + "act on an outcome the customer has not finished producing.");
  assert(
    latestResult(testCase.taskId) === `${testCase.expectedResult}|false|${testCase.expectedCounted}`,
    `${testCase.taskId}: the result changed during the settle window, so the absence above was `
    + "measured against a different attempt than the one asserted.");
  assert(
    psql("SELECT COUNT(*) FROM ivr_call_jobs WHERE task_id = "
      + `'${testCase.taskId}' AND closed_at IS NULL`) === "1",
    `${testCase.taskId}: the job closed on a non-final result, so the customer lost the attempt `
    + "the policy still owed them.");

  return correlation;
}

/** A fault on OUR side: costs the customer nothing, retries once, then parks for a person. */
function driveTechnicalCase(testCase) {
  const correlation = admitCase(testCase);

  // Waits for the job to be PARKED rather than for a result. The state worth asserting is the one
  // after the retry budget is spent; stopping at the first result would measure the stack
  // mid-retry and call whatever it found the answer.
  let parked = "none";
  for (let attempt = 0; attempt < 60; attempt += 1) {
    parked = psql(
      "SELECT status || '|' || queue_status || '|' || (closed_at IS NULL)::text "
      + `FROM ivr_call_jobs WHERE task_id = '${testCase.taskId}'`);
    if (parked.startsWith("HELD_ADMIN_REVIEW")) break;
    sleepSeconds(2);
  }
  assert(
    parked === "HELD_ADMIN_REVIEW|HELD_TECHNICAL_REVIEW|true",
    `${testCase.taskId}: the job sits at ${parked}, expected `
    + "HELD_ADMIN_REVIEW|HELD_TECHNICAL_REVIEW|true. A CLOSED job would mean IVR settled what "
    + "happens to the order over a fault of its own.");

  // Two dials, ONE customer attempt, NONE of it spent. All three numbers in one query because the
  // claim is about how they relate: two rows that were two customer attempts would be a different
  // and much worse stack than two rows that were one attempt retried.
  assert(
    psql("SELECT COUNT(*) || '/' || COUNT(DISTINCT a.attempt_number) || '/' || "
      + "COUNT(*) FILTER (WHERE a.is_counted_customer_attempt) "
      + "FROM ivr_call_attempts a JOIN ivr_call_jobs j ON j.ivr_call_job_id = a.ivr_call_job_id "
      + `WHERE j.task_id = '${testCase.taskId}'`) === "2/1/0",
    `${testCase.taskId}: dials / customer attempts / counted should be 2/1/0. The retry belongs to `
    + "the SAME attempt, and a policy that promised the customer one call still owes them one.");

  // The budget, spent in order and then stopped. TechnicalRetryLimit is 1, so the counter runs
  // 1 then 2, and the second is the one that is over budget and parks the job.
  assert(
    psql("SELECT string_agg(a.technical_retry_count::text, ',' ORDER BY a.ended_at) "
      + "FROM ivr_call_attempts a JOIN ivr_call_jobs j ON j.ivr_call_job_id = a.ivr_call_job_id "
      + `WHERE j.task_id = '${testCase.taskId}'`) === "1,2",
    `${testCase.taskId}: the technical retry counter did not run 1 then 2, so the retry budget is `
    + "not being carried across dials and nothing bounds how many times this can repeat.");

  assert(
    psql("SELECT COUNT(*) || '/' || COUNT(*) FILTER (WHERE "
      + "r.result_type = 'IVR_TECHNICAL_EXCEPTION' AND r.is_final_for_ivr IS FALSE "
      + "AND r.is_counted_customer_attempt IS FALSE) "
      + "FROM ivr_call_results r JOIN ivr_call_jobs j ON j.ivr_call_job_id = r.ivr_call_job_id "
      + `WHERE j.task_id = '${testCase.taskId}'`) === "2/2",
    `${testCase.taskId}: both results must be non-final, uncounted IVR_TECHNICAL_EXCEPTION.`);

  // The channel carried the call; it did not cause the fault. Blaming it would let a bad hour in
  // our own audio stack quarantine the fleet one channel at a time, turning a software problem
  // into a capacity outage -- and DT-04 locks a channel after three.
  assert(
    psql("SELECT COUNT(*) || '/' || COUNT(*) FILTER (WHERE status = 'IDLE' AND fail_count = 0) "
      + "FROM ivr_sim_channels") === "1/1",
    `${testCase.taskId}: the SIM channel was charged for a fault in our own stack.`);

  // Same settle as the silent cases, and for the same reason -- plus one more: a retry budget that
  // is not carried would show up here as a third dial rather than as a wrong counter above.
  sleepSeconds(SILENT_SETTLE_SECONDS);
  assert(
    psql(`SELECT COUNT(*) FROM ivr_result_callbacks WHERE task_id = '${testCase.taskId}'`) === "0",
    `${testCase.taskId}: a technical fault reached the callback outbox. Sales would be asked to `
    + "act on an outcome that says nothing about the customer.");
  assert(
    psql("SELECT COUNT(*) FROM ivr_call_attempts a JOIN ivr_call_jobs j "
      + `ON j.ivr_call_job_id = a.ivr_call_job_id WHERE j.task_id = '${testCase.taskId}'`) === "2",
    `${testCase.taskId}: a third dial appeared after the retry budget was spent.`);

  return correlation;
}

/** Pause the queue, let a window close with nothing dialled, resume. */
function driveCapacityCase(testCase) {
  const correlation = `corr-e2e-${testCase.taskId.replace("TASK-E2E-", "")}`;
  const pause = JSON.parse(apiPost(
    "/v1/ivr/order-confirmation/queue:pause",
    JSON.stringify({
      reason: "E2E capacity drill - hold dispatch so a confirmation window can close undialled",
      evidence_ref: "evidence://compose/e2e-capacity",
    }),
    adminHeaders("IVR_QUEUE_PAUSE", correlation, "idem-e2e-pause")));
  assert(
    pause.status === "APPLIED",
    `queue:pause returned ${JSON.stringify(pause)}.`);
  // Accepted is not applied. The projection is read back because the whole case rests on dispatch
  // actually being held, and a pause that returned 200 without taking effect would instead produce
  // a confirmed call and a failure message pointing at the wrong component.
  assert(
    queueProjection(correlation).paused === true,
    "the queue reports itself running after queue:pause; dispatch was never held.");

  let released = false;
  try {
    admitCase(testCase);
    awaitDelivery(
      testCase,
      "A confirmed or cancelled result means the pause did not hold dispatch and the task was "
      + "dialled inside a window that was supposed to run out.");

    // The claim the result type makes: no call was placed. Without this the case would also pass
    // on a stack that dialled, failed, and happened to label the failure a capacity miss.
    //
    // Counted together with a case that certainly DID dial, in one query, because "no attempt
    // rows" is also what a query that cannot see attempt rows returns. Zero on its own is the
    // answer to both "nothing was dialled" and "this join is wrong", and only the pair separates
    // them.
    assert(
      psql("SELECT (SELECT COUNT(*) FROM ivr_call_attempts a JOIN ivr_call_jobs j "
        + `ON j.ivr_call_job_id = a.ivr_call_job_id WHERE j.task_id = '${testCase.taskId}') `
        + "|| '/' || (SELECT COUNT(*) FROM ivr_call_attempts a JOIN ivr_call_jobs j "
        + `ON j.ivr_call_job_id = a.ivr_call_job_id WHERE j.task_id = '${ATTEMPT_CONTROL_TASK}')`)
      === "0/1",
      `${testCase.taskId}: attempts for this task over attempts for ${ATTEMPT_CONTROL_TASK} should `
      + "be 0/1. A left digit above zero means the window did not close undialled; a right digit of "
      + "zero means the count cannot see attempts at all and the left digit proves nothing.");
    assert(
      psql("SELECT j.status || '|' || j.queue_status || '|' || COALESCE(i.shortage_reason, 'none') "
        + "FROM ivr_call_jobs j LEFT JOIN ivr_capacity_incidents i "
        + "ON i.capacity_incident_id = j.capacity_incident_id "
        + `WHERE j.task_id = '${testCase.taskId}'`)
      === "CAPACITY_MISSED|CLOSED_CAPACITY|NO_DISPATCH_BEFORE_DEADLINE",
      `${testCase.taskId}: the closed job does not carry the capacity incident that explains it.`);
  } finally {
    // Released on the way out of a failure as well, so a red case never leaves the queue held for
    // whatever runs next. Recorded rather than asserted HERE: this block also runs while an
    // exception is in flight, and throwing from it would replace the real error with a symptom.
    const resume = JSON.parse(apiPost(
      "/v1/ivr/order-confirmation/queue:resume",
      JSON.stringify({
        reason: "E2E capacity drill complete - release dispatch",
        evidence_ref: "evidence://compose/e2e-capacity",
      }),
      adminHeaders("IVR_QUEUE_RESUME", correlation, "idem-e2e-resume")));
    released = resume.status === "APPLIED" && queueProjection(correlation).paused === false;
  }

  // Outside the finally, so it can only be reached when nothing above already failed.
  assert(
    released,
    "the queue stayed paused after queue:resume; anything scheduled later would fail for the "
    + "wrong reason.");

  return correlation;
}

function checkEndToEnd() {
  try {
    const runtimeServices = observabilityRuntime
      ? ["otel-lgtm", "fake-sales", "ivr-api", "ivr-worker"]
      : [];
    docker([...COMPOSE_E2E, "up", "-d", "--build", ...runtimeServices], { inherit: true });
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
        input: fs.readFileSync(path.join(repositoryRoot, "deploy/docker/dev-seed/seed.sql"), "utf8"),
      });

    if (observabilityRuntime) {
      const testCase = E2E_DIALLED_CASES[0];
      const taskBody = confirmationTask(testCase);
      const correlation = admitCase(testCase, taskBody);
      awaitDelivery(testCase, "The single observability proof task did not complete.");
      const journal = JSON.parse(curlInternal(["http://fake-sales:8080/__admin/requests?limit=100"]));
      const received = journal.requests
        .filter((entry) => entry.request.url.includes("/ivr-result-callbacks"))
        .map((entry) => ({ headers: entry.request.headers, body: JSON.parse(entry.request.body) }))
        .filter((entry) => entry.body.task_id === testCase.taskId);
      assert(received.length === 1, `${testCase.taskId}: callback was not accepted exactly once.`);
      assert(
        received[0].body.result_type === testCase.expectedResult
        && received[0].body.recommended_core_action === testCase.expectedAction
        && received[0].headers["X-Correlation-Id"] === correlation,
        `${testCase.taskId}: callback taxonomy or correlation is wrong.`,
      );

      verifyObservabilityRuntime({
        docker,
        compose: COMPOSE_E2E,
        psql,
        sleepSeconds,
        repositoryRoot,
      });
      proveCollectorOutageDoesNotBreakBusinessFlow(testCase, taskBody, correlation);
      process.stdout.write(
        "IT-IMG-E2E-05 PASS — exactly one MOCK task crossed intake, eligibility, dispatch, "
        + "normalization and callback\n",
      );
      return;
    }

    // Sequential, not parallel. One SIM channel is seeded on purpose: with a pool, a scheduling
    // defect could hide behind a spare channel, and cases sharing one channel also prove the lease
    // is released between calls. The capacity case runs LAST because it holds the whole queue
    // still, and a paused queue would strand any case that came after it.
    const correlations = new Map();
    for (const testCase of E2E_DIALLED_CASES) {
      correlations.set(testCase.taskId, driveDeliveredCase(testCase));
    }
    for (const testCase of E2E_SILENT_CASES) {
      correlations.set(testCase.taskId, driveSilentCase(testCase));
    }
    correlations.set(E2E_TECHNICAL_CASE.taskId, driveTechnicalCase(E2E_TECHNICAL_CASE));
    correlations.set(E2E_CAPACITY_CASE.taskId, driveCapacityCase(E2E_CAPACITY_CASE));

    // The other end. IVR believing it delivered and Sales having received are two different
    // claims, and the point of this check is that they were not the same claim for a long time.
    const journal = JSON.parse(curlInternal(["http://fake-sales:8080/__admin/requests?limit=100"]));
    const delivered = journal.requests
      .filter((entry) => entry.request.url.includes("/ivr-result-callbacks"))
      .map((entry) => ({ headers: entry.request.headers, body: JSON.parse(entry.request.body) }));

    for (const testCase of E2E_DELIVERING_CASES) {
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

    // Asserted at the far end too, not only against our own outbox. An empty outbox proves IVR
    // sent nothing; an empty journal proves Sales heard nothing, and only the second one is the
    // promise being made to another team.
    for (const taskId of E2E_SILENT_TASKS) {
      assert(
        !delivered.some((entry) => entry.body.task_id === taskId),
        `${taskId}: fake Sales received a callback for a NON-final result.`);
    }

    // Each delivering case must have arrived exactly once, and nothing else may have arrived at
    // all. A retry that duplicated a signal would still satisfy every assertion above, and a
    // duplicate confirmation is not a harmless one.
    assert(
      delivered.length === E2E_DELIVERING_CASES.length,
      `fake Sales received ${delivered.length} callbacks for `
      + `${E2E_DELIVERING_CASES.length} tasks that should notify it.`);

    process.stdout.write(
      `IT-IMG-E2E-05 PASS — ${E2E_DELIVERING_CASES.length + E2E_SILENT_TASKS.length} tasks `
      + "crossed the whole stack on one SIM channel: "
      + `${E2E_DELIVERING_CASES.map((testCase) => testCase.expectedResult).join(", ")} each `
      + "ACCEPTED by fake Sales exactly once with its correlation id intact, and "
      + `${[...E2E_SILENT_CASES, E2E_TECHNICAL_CASE].map((testCase) => testCase.expectedResult)
        .join(", ")} reached Sales not at all\n`);
  } finally {
    try { docker([...COMPOSE_E2E, "down", "-v"], { inherit: true }); } catch { /* best effort */ }
  }
}

// ------------------------------------------------------------------- IT-IMG-SBOM-06
//
// An SBOM is worth having because it answers "is this image affected by CVE-X" without rebuilding
// it. A file that merely exists answers nothing, and an EMPTY one answers "no" to every question --
// which is the most dangerous shape a security artifact can take, because it looks like a clean
// bill of health.
//
// So the SBOM is checked by being USED. It is generated, asserted non-trivial, then handed back to
// the scanner: scanning the SBOM must reach the same verdict as scanning the image. And the whole
// path is proven against a base with known findings, because a pipeline that produces a green
// answer from an empty document is indistinguishable from one that works.
const SBOM_DIRECTORY = path.join(repositoryRoot, "artifacts", "sbom");

function sbomFor(image) {
  return docker([
    "run", "--rm", "-v", "/var/run/docker.sock:/var/run/docker.sock",
    "aquasec/trivy:0.58.1", "image", "--format", "cyclonedx", "--quiet", image,
  ]);
}

/** Scans an SBOM document. Copied into a container rather than bind-mounted: a mount would make
 *  this depend on the host's path layout, and the same choice is already made for the alerts. */
function scanSbom(document) {
  const container = docker([
    "create", "--entrypoint", "sleep", "aquasec/trivy:0.58.1", "300",
  ]).trim();
  try {
    docker(["start", container]);
    docker(["exec", "-i", container, "sh", "-c", "cat > /tmp/sbom.json"],
      { stdio: ["pipe", "pipe", "pipe"], input: document });
    docker(["exec", container, "trivy", "sbom", "--severity", "HIGH,CRITICAL",
      "--exit-code", "1", "--quiet", "/tmp/sbom.json"]);
    return true;
  } catch {
    return false;
  } finally {
    try { docker(["rm", "-f", container]); } catch { /* already gone */ }
  }
}

function checkSbom() {
  fs.mkdirSync(SBOM_DIRECTORY, { recursive: true });
  const counts = {};

  for (const image of IMAGES) {
    const reference = `${image.name}:${TAG}`;
    const document = sbomFor(reference);
    const parsed = JSON.parse(document);
    const components = parsed.components ?? [];

    // A floor rather than an exact number: the count moves whenever a base image is bumped, and a
    // check that had to be edited on every bump would be edited without being read. Zero, or a
    // handful, means the generator looked at the image and found nothing -- which for a .NET
    // runtime image is not possible and is the failure this floor exists to catch.
    //
    // Ten, not twenty. The smallest image here enumerates 24 components, and a floor four away
    // from the real value fails on the next base bump for a reason that has nothing to do with
    // what it is checking. The question is "did this enumerate anything", and ten answers it with
    // room to spare.
    assert(
      components.length >= 10,
      `${reference} produced an SBOM with ${components.length} components. An SBOM that enumerates `
      + "nothing answers 'not affected' to every CVE ever asked about it.");

    // It has to be an SBOM OF THIS IMAGE. A document naming something else would scan clean and
    // mean nothing.
    assert(
      JSON.stringify(parsed.metadata?.component ?? {}).includes(image.name),
      `${reference}'s SBOM does not name the image it claims to describe.`);

    fs.writeFileSync(path.join(SBOM_DIRECTORY, `${image.name}.cdx.json`), document, "utf8");
    counts[image.name] = components.length;

    // The verdict has to survive the round trip. If the SBOM lost the package data that the image
    // scan used, this is where it shows: same severities, same threshold, different input.
    assert(
      scanSbom(document),
      `${reference} scans clean as an image but not as an SBOM. The SBOM is not carrying what the `
      + "scanner needs, so nobody could answer a CVE question from it.");
  }

  // Positive control, and it is about the SBOM PATH rather than about the scanner -- IT-IMG-SCAN-04
  // already proved the scanner fails on a bad image. This proves a bad image still fails after
  // being turned into an SBOM and back, which is the step that could silently drop everything.
  const knownBad = sbomFor("alpine:3.10");
  assert(
    !scanSbom(knownBad),
    "an SBOM of a base with known HIGH/CRITICAL findings scanned clean. The SBOM path is dropping "
    + "the very data it exists to carry, and every green result above is vacuous.");

  process.stdout.write(
    `IT-IMG-SBOM-06 PASS — CycloneDX SBOMs written for ${IMAGES.length} images (`
    + `${IMAGES.map((image) => `${image.name}=${counts[image.name]}`).join(", ")} components), each `
    + "scans clean as an SBOM as well as an image, and a known-bad base still fails after the round "
    + `trip. Written to ${path.relative(repositoryRoot, SBOM_DIRECTORY)}\n`);
}

buildAndCheckUser();
checkHealthcheck();
if (!skipCompose) checkCompose();
if (!skipScan) checkScan();
if (!skipScan) checkSbom();
if (!skipEndToEnd) checkEndToEnd();
process.stdout.write("IMAGE_SELFTEST_PASS\n");
