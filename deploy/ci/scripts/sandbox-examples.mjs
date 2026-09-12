#!/usr/bin/env node
/**
 * W-0282 / B2 — the example set Module 3 runs against the sandbox, from OUTSIDE the process.
 *
 * Why this exists when the API behaviour matrix already covers all 38 operations: the matrix runs
 * in-process against `Ivr.Api.Program` with a Testcontainers database. It proves our composition
 * root behaves. It cannot prove that a partner on another machine, holding only a credential and
 * a base URL, can get a task through to a delivered result — and on 2026-09-12 they could not.
 * Driving the stack from outside for the first time found a 404 developer surface, an unseeded
 * attempt policy and four worker jobs that ship disabled. None of those were visible from inside.
 *
 * So this speaks HTTP over a socket, with no reference to our assemblies, exactly as Module 3 will.
 *
 *   node deploy/ci/scripts/sandbox-examples.mjs
 *   node deploy/ci/scripts/sandbox-examples.mjs --base-url http://sandbox.internal:58080
 *
 * It requires a CLEAN sandbox, and says so rather than failing obliquely. MOCK telephony outcomes
 * are keyed by exact task id (docker-compose.sandbox.yml), so the scenarios have to use fixed ids;
 * a second run over the same volume replays those ids and is answered idempotently, which would
 * report "no result" for a stack that is working perfectly. Reset with:
 *
 *   docker compose -f docker-compose.dev.yml -f docker-compose.sandbox.yml down -v
 *
 * SAFETY. Nothing here can dial a person. The stack it drives is MOCK/MOCK/NO, and every phone
 * value in the fixtures is a masked reference. No real number, address or token is written by this
 * script or by anything it reads.
 */

import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { REPOSITORY_ROOT } from "./repository-path-lib.mjs";

const PREFIX = "/v1/ivr/order-confirmation";

/** Timestamps a rebase moves. Mirrors SeedCatalog.WindowFields field for field: the two describe
 *  the same operation, and a partner whose examples drifted from the seed loader's would be
 *  debugging a difference nobody meant to create. */
const WINDOW_FIELDS = [
  "created_at",
  "confirmation_window_started_at",
  "confirmation_window_expires_at",
  "dial_token_expires_at",
];

const args = new Map();
for (let i = 2; i < process.argv.length; i += 2) {
  args.set(process.argv[i].replace(/^--/, ""), process.argv[i + 1]);
}

const BASE_URL = (args.get("base-url") ?? process.env.IVR_SANDBOX_BASE_URL
  ?? "http://127.0.0.1:58080").replace(/\/$/, "");
const ORDER_CORE_TOKEN = process.env.ORDER_CORE_SERVICE_TOKEN
  ?? "dev-ordercore-token-not-a-real-secret";
const ADMIN_READ_TOKEN = process.env.IVR_ADMIN_READ_TOKEN
  ?? "dev-admin-read-token-not-a-real-secret";
const ADMIN_WRITE_TOKEN = process.env.IVR_ADMIN_WRITE_TOKEN
  ?? "dev-admin-write-token-not-a-real-secret";
const INTERNAL_TOKEN = process.env.IVR_INTERNAL_SERVICE_TOKEN
  ?? "dev-internal-token-not-a-real-secret";
const RESULT_TIMEOUT_MS = Number(args.get("result-timeout-ms") ?? 90_000);

/** Readiness polls, three seconds apart. Long enough for a cold start with migrations. */
const READY_ATTEMPTS = 20;

/** Attempts per request before a transport failure is reported as one. */
const TRANSPORT_ATTEMPTS = 4;

/**
 * The six telephony outcomes a client has to handle, and the task id that selects each one.
 *
 * `expectedResult` is what IVR normalises the mock disposition into — the value that reaches
 * Module 3 on the callback. `final` says whether the outcome ends the job: a non-final outcome is
 * one where an attempt remains, and Module 3 must receive NOTHING for it. That silence is a
 * contract term, so it is asserted rather than assumed.
 */
const SCENARIOS = [
  { taskId: "TASK-M3-CONFIRM", base: "golden-hour-online-confirmable",
    expectedResult: "IVR_CONFIRMED", final: true,
    note: "Customer answered and pressed 1." },
  { taskId: "TASK-M3-CANCEL", base: "golden-hour-online-cancel",
    expectedResult: "IVR_CUSTOMER_CANCELLED", final: true,
    note: "Customer answered and pressed 0." },
  // Rang out on the FIRST of two permitted attempts, so this is the deliberately quiet case.
  // mock-lab-v1 allows two attempts at +0s and +150s; the second ring-out is what produces
  // IVR_NO_ANSWER_FINAL and the only one Module 3 ever hears about. Watch that happen with
  // --result-timeout-ms 200000. The fast case is the more useful one to rehearse: a partner who
  // treats silence as failure will cancel orders IVR is still working on.
  { taskId: "TASK-M3-NOANSWER", base: "golden-hour-online-no-answer",
    expectedResult: "IVR_NO_ANSWER_ATTEMPT", final: false,
    note: "Rang out with an attempt still to come, so nothing is sent yet." },
  { taskId: "TASK-M3-BADNUMBER", base: "twenty-four-seven-cod-confirmable",
    expectedResult: "IVR_INVALID_PHONE_FINAL", final: true,
    note: "The network could not reach the destination at all." },
  { taskId: "TASK-M3-WRONGKEY", base: "twenty-four-seven-cod-cancel",
    expectedResult: "IVR_WRONG_INPUT", final: false,
    note: "Answered, pressed a key the menu does not offer. An attempt remains, so nothing is sent." },
  { taskId: "TASK-M3-TECHNICAL", base: "twenty-four-seven-cod-technical",
    expectedResult: "IVR_TECHNICAL_EXCEPTION", final: false,
    note: "Our own audio failed on a healthy channel. An attempt remains, so nothing is sent." },
];

function iso(date) {
  return `${date.toISOString().slice(0, 19)}Z`;
}

/** SeedCatalog.Rebase, in JavaScript: anchor on the window start, shift all four fields by that
 *  task's own offset so the gaps between them survive. */
function rebase(body, to) {
  const anchor = Date.parse(body.confirmation_window_started_at);
  if (Number.isNaN(anchor)) {
    return body;
  }

  const offset = to.getTime() - anchor;
  for (const field of WINDOW_FIELDS) {
    if (typeof body[field] === "string") {
      body[field] = iso(new Date(Date.parse(body[field]) + offset));
    }
  }

  // The eligibility evidence moves with the window it describes, or the task is held as
  // ELIGIBILITY_SNAPSHOT_STALE. Same rule as SeedCatalog.Rebase, one level down.
  const capturedAt = body.eligibility_snapshot?.captured_at;
  if (typeof capturedAt === "string") {
    body.eligibility_snapshot.captured_at = iso(new Date(Date.parse(capturedAt) + offset));
  }

  return body;
}

function orderCoreHeaders(correlationId, idempotencyKey) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${ORDER_CORE_TOKEN}`,
    "X-Source-System": "order-core",
    "X-Correlation-Id": correlationId,
    "Idempotency-Key": idempotencyKey,
  };
}

function adminHeaders(tier, correlationId, idempotencyKey) {
  const token = tier === "read" ? ADMIN_READ_TOKEN : ADMIN_WRITE_TOKEN;
  const headers = {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
    "X-Service-Scope": tier === "read" ? "ivr.admin.read" : "ivr.admin.write",
    "X-Actor-Id": "module-3-sandbox-rehearsal",
    "X-Correlation-Id": correlationId,
  };
  if (idempotencyKey) {
    headers["Idempotency-Key"] = idempotencyKey;
  }

  return headers;
}

/**
 * Headers for the INTERNAL lifecycle surface. Not Module 3's credential and not Module 3's to
 * hold: X-Source-System is literally `ivr-worker`, so this speaks as IVR's own worker.
 *
 * See recordEligibility() for why the harness has to speak as the worker at all.
 */
function internalHeaders(correlationId, idempotencyKey) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${INTERNAL_TOKEN}`,
    "X-Source-System": "ivr-worker",
    "X-Service-Scope": "ivr.internal.write",
    "X-Correlation-Id": correlationId,
    "Idempotency-Key": idempotencyKey,
  };
}

/**
 * One HTTP call, with a bounded retry on TRANSPORT failures only.
 *
 * Not defensive padding. This run makes hundreds of requests over a minute or more, and a Node
 * keep-alive socket that the server recycles rejects the in-flight request with a bare
 * `fetch failed` -- which is ordinary, and which killed a whole clean run on 2026-09-12 while the
 * API stayed up, healthy and with zero restarts. The first version of this message then announced
 * that the sandbox was unreachable, which was false and pointed the reader at the wrong thing. A
 * message that misdiagnoses confidently is worse than a stack trace.
 *
 * HTTP statuses are NEVER retried. A 409, a 422 or a 429 is an answer, and several of them are the
 * answers this script exists to assert.
 */
async function call(method, path, { headers = {}, body } = {}) {
  let lastFailure = null;
  let response = null;
  for (let attempt = 0; attempt < TRANSPORT_ATTEMPTS && response === null; attempt += 1) {
    if (attempt > 0) {
      await new Promise((resolve) => setTimeout(resolve, 250 * attempt));
    }

    response = await fetch(`${BASE_URL}${PREFIX}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    }).catch((error) => {
      lastFailure = error;
      return null;
    });
  }

  if (response === null) {
    throw new GuidanceError(
      `${BASE_URL} did not answer ${method} ${path} after ${TRANSPORT_ATTEMPTS} attempts`
      + ` (${lastFailure?.cause?.code ?? lastFailure?.message ?? "transport failure"}).\n\n`
      + "Check the sandbox is up and that the API is not restarting:\n"
      + "  docker compose -f docker-compose.dev.yml -f docker-compose.sandbox.yml ps\n"
      + "  docker compose -f docker-compose.dev.yml -f docker-compose.sandbox.yml logs --tail 50 ivr-api");
  }

  const text = await response.text();
  let parsed = null;
  try {
    parsed = text.length > 0 ? JSON.parse(text) : null;
  } catch {
    parsed = null;
  }

  return { status: response.status, body: parsed, raw: text };
}

/** A failure the reader is meant to ACT on, not debug: printed as a sentence, with no stack. */
class GuidanceError extends Error {}

const observations = [];
const failures = [];

function record(id, expected, actual, note) {
  const pass = JSON.stringify(expected) === JSON.stringify(actual);
  observations.push({ id, expected, actual, pass, note });
  if (!pass) {
    failures.push(`${id}: expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
  }

  const mark = pass ? "PASS" : "FAIL";
  console.log(`  ${mark}  ${id}  ${note ?? ""}`);
  if (!pass) {
    console.log(`        expected ${JSON.stringify(expected)}`);
    console.log(`        actual   ${JSON.stringify(actual)}`);
  }

  return pass;
}

/** The correlation id a scenario runs under. Used by the run AND by the clean-sandbox guard, so
 *  the guard cannot go looking for something the run never wrote. */
function correlationFor(taskId) {
  return `corr-${taskId.toLowerCase()}`;
}

function loadFixture() {
  const path = join(REPOSITORY_ROOT, "seed", "sales-target-v1.sample.json");
  return JSON.parse(readFileSync(path, "utf8"));
}

/**
 * Builds a task Module 3 owns: the fixture's shape, its own identifiers, and a window that is open
 * now.
 *
 * Both of the things done here are things a partner copying an example out of IR-06 gets wrong on
 * the first try, and each fails with a message that points somewhere else. The fixtures carry
 * absolute August-2026 windows, so a verbatim copy is refused IVR_STATE_NOT_CALLABLE — which reads
 * as "your order is in the wrong state". And `correlation_id` in the body has to equal the
 * X-Correlation-Id header, or the answer is IVR_MISSING_TRACE.
 */
function taskFrom(fixture, baseScenario, { taskId, correlationId }) {
  const source = fixture.tasks.find((task) => task.scenario === baseScenario);
  if (!source) {
    throw new Error(`seed fixture has no task scenario '${baseScenario}'.`);
  }

  const body = rebase(structuredClone(source.body), new Date());
  const suffix = taskId.replace(/^TASK-/, "");
  body.task_id = taskId;
  body.order_id = `ORDER-${suffix}`;
  body.order_code = `GF-2026-${suffix}`;
  body.correlation_id = correlationId;
  return body;
}

async function preflight() {
  console.log(`\nSandbox: ${BASE_URL}`);
  // /health/ready, not /health/live: a sandbox that is alive but cannot reach its database would
  // answer every example with a 500, and the run should say so in one line rather than in sixty.
  //
  // And it WAITS rather than asking once. `pnpm sandbox:up && pnpm sandbox:examples` is the natural
  // thing to type, but `up` returns when the containers start, not when the API has finished its
  // migrations -- so asking once turned a stack that was merely still booting into a raw
  // ECONNREFUSED stack trace. Waiting costs a few seconds and removes the race.
  for (let attempt = 0; attempt < READY_ATTEMPTS; attempt += 1) {
    const health = await fetch(`${BASE_URL}/health/ready`).catch(() => null);
    if (health?.ok) {
      console.log(attempt === 0 ? "  ready: OK" : `  ready: OK (after ${attempt * 3}s)`);
      return;
    }

    await new Promise((resolve) => setTimeout(resolve, 3000));
  }

  throw new GuidanceError(
    `${BASE_URL} did not answer /health/ready within ${READY_ATTEMPTS * 3}s. Start the sandbox with:\n`
    + "  pnpm sandbox:up\n\n"
    + "If it is already running, check the API log:\n"
    + "  docker compose -f docker-compose.dev.yml -f docker-compose.sandbox.yml logs ivr-api");
}

/**
 * Refuses to run against a sandbox that has already run the example set.
 *
 * Without this the second run is answered `409 IVR_IDEMPOTENCY_CONFLICT` for every scenario, and
 * that is the system being RIGHT: the examples use fixed task ids because MOCK telephony outcomes
 * are keyed by task id, so a rerun replays the same Idempotency-Key while the window has been
 * rebased to a new now -- same key, different payload, correctly refused. But it prints as nine red
 * lines that read exactly like a broken module, which is the shape of trap this whole work item
 * exists to remove. The header of this file documented the precondition; documenting it was not
 * enough, because the first person to hit it was the owner, two minutes after being handed the
 * command.
 *
 * Only blocks when it is CERTAIN. A probe that cannot be read (wrong tier token, route missing)
 * says nothing and lets prepare() produce its clearer message instead.
 */
async function assertCleanSandbox() {
  const correlation = correlationFor(SCENARIOS[0].taskId);
  const probe = await call(
    "GET",
    `/call-jobs?correlation_id=${encodeURIComponent(correlation)}&page_size=1`,
    { headers: adminHeaders("read", "corr-sandbox-clean-check") });

  if (probe.status !== 200 || typeof probe.body?.total_count !== "number") {
    return;
  }

  if (probe.body.total_count === 0) {
    console.log("  clean: no previous example run on this volume");
    return;
  }

  throw new GuidanceError(
    [
      "This sandbox has already run the example set.",
      "",
      "Every scenario would be answered 409 IVR_IDEMPOTENCY_CONFLICT, which is CORRECT: the",
      "examples use fixed task ids (MOCK outcomes are keyed by task id), so a rerun replays the",
      "same Idempotency-Key with a freshly rebased window -- the same key carrying a different",
      "payload, which the contract refuses on purpose.",
      "",
      "The example set needs a clean volume. Reset and run again:",
      "",
      "  pnpm sandbox:reset && pnpm sandbox:up && pnpm sandbox:examples",
    ].join("\n"));
}

/**
 * Loads the seed fixtures, which is also what writes the attempt policy. Without it every task a
 * partner pushes is answered TASK_HELD_POLICY_MISSING, and a 404 here means the stack is running
 * with the developer surface unmapped — the exact failure docker-compose.sandbox.yml exists to fix.
 */
async function prepare() {
  const result = await call("POST", "/dev/seed:load", {
    headers: adminHeaders("write", "corr-sandbox-prepare", "idem-sandbox-prepare"),
    body: { reason: "Module 3 sandbox example run", rebase_windows: true },
  });

  if (result.status === 404) {
    throw new GuidanceError(
      "/dev/seed:load answered 404, so the developer surface is not mapped. The stack is running "
      + "as Production. Start it with the sandbox overlay:\n"
      + "  docker compose -f docker-compose.dev.yml -f docker-compose.sandbox.yml up -d --build");
  }

  if (result.status !== 200) {
    throw new Error(`/dev/seed:load answered ${result.status}: ${result.raw.slice(0, 300)}`);
  }

  console.log(`  prepared: ${result.body.task_count} fixtures, attempt policy present`);
}

async function admitScenarios(fixture) {
  console.log("\nTask intake, one per telephony outcome");
  const admitted = [];
  for (const scenario of SCENARIOS) {
    const correlationId = correlationFor(scenario.taskId);
    const body = taskFrom(fixture, scenario.base, {
      taskId: scenario.taskId,
      correlationId,
    });
    const result = await call("POST", "/tasks", {
      headers: orderCoreHeaders(correlationId, `idem-${scenario.taskId.toLowerCase()}`),
      body,
    });

    const decision = result.status === 200 ? result.body?.decision : `HTTP ${result.status}`;
    const ok = record(
      scenario.taskId,
      { status: 200, accepted: true },
      { status: result.status, accepted: typeof result.body?.ivr_call_job_id === "string" },
      scenario.note);
    if (ok) {
      admitted.push({ ...scenario, jobId: result.body.ivr_call_job_id, correlationId, decision });
    }
  }

  return admitted;
}

/**
 * Records eligibility so the job can leave HELD_MOCK and be claimed for dialling.
 *
 * THIS STEP IS A STAND-IN, and the gap it stands in for is the largest thing B2 found. Intake in
 * MOCK parks a job at DRY_RUN / HELD_MOCK, and nothing moves it until something posts
 * /eligibility-checks. Nothing does: the worker registers ten hosted services and not one of them
 * is an eligibility loop, and the only callers of that endpoint in the repository are this script
 * and the image self-test. So a task Module 3 pushes is accepted, and then sits there.
 *
 * The endpoint is tagged `internal` and its source header names `ivr-worker`, so the design intent
 * is plainly that IVR's own worker calls it — Module 3 must NOT be handed this credential. Who
 * owns that loop, and what it revalidates (D-06), is an owner decision and not something to invent
 * inside a sandbox harness. Until it is answered, the sandbox makes the round trip demonstrable by
 * doing the step explicitly, and says so here rather than letting a green run imply the loop exists.
 */
async function recordEligibility(admitted) {
  console.log("\nEligibility (stand-in for a loop IVR does not have — see W-0282)");
  const eligible = [];
  for (const scenario of admitted) {
    const suffix = scenario.taskId.toLowerCase();
    const result = await call("POST", "/eligibility-checks", {
      headers: internalHeaders(`corr-elig-${suffix}`, `idem-elig-${suffix}`),
      body: { task_id: scenario.taskId },
    });
    const ok = record(
      `${scenario.taskId}/eligibility`,
      { status: 200, decision: "ELIGIBLE_FOR_IVR" },
      { status: result.status, decision: result.body?.decision ?? null },
      (result.body?.blocked_reasons ?? []).join(", "));
    if (ok) {
      eligible.push(scenario);
    }
  }

  return eligible;
}

/**
 * Waits for each admitted job to reach a result, reading IVR's own admin surface rather than the
 * fake receiver's journal. Two reasons: it is the surface Module 3 is entitled to use, and the
 * fake receiver sits on an internal network with a proved no-egress guarantee that publishing its
 * journal would weaken.
 */
async function awaitResults(admitted) {
  console.log("\nRound trip: dialled in MOCK, normalised, delivered");
  const deadline = Date.now() + RESULT_TIMEOUT_MS;
  const pending = new Map(admitted.map((entry) => [entry.taskId, entry]));
  const seen = new Map();
  let pollIntervalMs = 1000;

  while (pending.size > 0 && Date.now() < deadline) {
    for (const [taskId, entry] of [...pending]) {
      const detail = await call("GET", `/call-jobs/${entry.jobId}/detail`, {
        headers: adminHeaders("read", `corr-detail-${taskId.toLowerCase()}`),
      });
      if (detail.status !== 200) {
        continue;
      }

      const results = detail.body?.results ?? [];
      if (results.length === 0) {
        continue;
      }

      seen.set(taskId, {
        resultType: results.at(-1)?.result_type ?? null,
        recommendedCoreAction: results.at(-1)?.recommended_core_action ?? null,
        callbacks: (detail.body?.callbacks ?? []).length,
      });
      pending.delete(taskId);
    }

    if (pending.size > 0) {
      // Backs off rather than sweeping every second. Six jobs polled once a second is 540 calls in
      // 90 seconds, and the admin read tier's own ceiling is 600 a minute -- so the long-wait
      // variant this guide recommends for IVR_NO_ANSWER_FINAL (--result-timeout-ms 200000) would
      // have spent 1200 and been refused by our own quota. The ceiling was right; the caller was
      // greedy.
      await new Promise((resolve) => setTimeout(resolve, pollIntervalMs));
      pollIntervalMs = Math.min(pollIntervalMs + 500, 5000);
    }
  }

  for (const scenario of admitted) {
    const observed = seen.get(scenario.taskId);
    record(
      `${scenario.taskId}/result`,
      { result_type: scenario.expectedResult, reached_module_3: scenario.final },
      {
        result_type: observed?.resultType ?? "NONE",
        reached_module_3: (observed?.callbacks ?? 0) > 0,
      },
      scenario.final
        ? "final outcome; a callback is delivered"
        : "not final; Module 3 must receive nothing");
  }
}

/** The refusals a client author has to write code for. Each one is a different decision on their
 *  side, which is why they are worth rehearsing rather than reading about. */
async function negativeExamples(fixture) {
  console.log("\nRefusals a client has to handle");

  const wrongPair = taskFrom(fixture, "golden-hour-online-confirmable", {
    taskId: "TASK-M3-NEG-PAIR",
    correlationId: "corr-m3-neg-pair",
  });
  wrongPair.payment_method_snapshot = "COD";
  const pairResult = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-pair", "idem-m3-neg-pair"),
    body: wrongPair,
  });
  record(
    "NEG-PAIR",
    { status: 400, code: "IVR_MALFORMED_REQUEST" },
    { status: pairResult.status, code: pairResult.body?.error?.code ?? null },
    "GOLDEN_HOUR with COD is not a supported program/payment pair");

  const expired = taskFrom(fixture, "golden-hour-online-confirmable", {
    taskId: "TASK-M3-NEG-WINDOW",
    correlationId: "corr-m3-neg-window",
  });
  const longAgo = new Date(Date.now() - 48 * 3600 * 1000);
  rebase(expired, longAgo);
  const expiredResult = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-window", "idem-m3-neg-window"),
    body: expired,
  });
  record(
    "NEG-WINDOW",
    { status: 422, code: "IVR_STATE_NOT_CALLABLE" },
    { status: expiredResult.status, code: expiredResult.body?.error?.code ?? null },
    "A window that already closed. This is what a verbatim copy of a doc example returns.");

  const mismatch = taskFrom(fixture, "golden-hour-online-confirmable", {
    taskId: "TASK-M3-NEG-TRACE",
    correlationId: "corr-m3-neg-trace-body",
  });
  const traceResult = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-trace-header", "idem-m3-neg-trace"),
    body: mismatch,
  });
  record(
    "NEG-TRACE",
    { status: 422, code: "IVR_MISSING_TRACE" },
    { status: traceResult.status, code: traceResult.body?.error?.code ?? null },
    "body correlation_id must equal the X-Correlation-Id header");

  const replayBody = taskFrom(fixture, "golden-hour-online-confirmable", {
    taskId: "TASK-M3-NEG-REPLAY",
    correlationId: "corr-m3-neg-replay",
  });
  const first = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-replay", "idem-m3-neg-replay"),
    body: replayBody,
  });
  const replay = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-replay", "idem-m3-neg-replay"),
    body: replayBody,
  });
  record(
    "REPLAY-SAME",
    { status: 200, same_job: true },
    {
      status: replay.status,
      same_job: Boolean(first.body?.ivr_call_job_id)
        && first.body?.ivr_call_job_id === replay.body?.ivr_call_job_id,
    },
    "Same key, same body: the original decision comes back. Retrying is safe.");

  const conflicting = structuredClone(replayBody);
  conflicting.order_code = "GF-2026-M3-DIFFERENT";
  const conflict = await call("POST", "/tasks", {
    headers: orderCoreHeaders("corr-m3-neg-replay", "idem-m3-neg-replay"),
    body: conflicting,
  });
  record(
    "REPLAY-CONFLICT",
    { status: 409, code: "IVR_IDEMPOTENCY_CONFLICT" },
    { status: conflict.status, code: conflict.body?.error?.code ?? null },
    "Same key, different body: refused. Use a new key for a new command.");
}

/**
 * Last, because it deliberately spends the account's whole window. The ceiling exists so a client
 * meets a 429 here, cheaply, rather than discovering in production that it does not handle one.
 */
async function quotaExample(fixture) {
  console.log("\nService-account ceiling");
  let refused = null;
  for (let attempt = 0; attempt < 200 && refused === null; attempt += 1) {
    const body = taskFrom(fixture, "golden-hour-online-confirmable", {
      taskId: `TASK-M3-QUOTA-${attempt}`,
      correlationId: `corr-m3-quota-${attempt}`,
    });
    const result = await call("POST", "/tasks", {
      headers: orderCoreHeaders(`corr-m3-quota-${attempt}`, `idem-m3-quota-${attempt}`),
      body,
    });
    if (result.status === 429) {
      refused = result;
    }
  }

  if (refused === null) {
    record("QUOTA", { status: 429 }, { status: "never refused" },
      "The ceiling did not engage. Is Ivr__ServiceQuota__Enabled set on this stack?");
    return;
  }

  const details = refused.body?.error?.details ?? {};
  record(
    "QUOTA",
    { status: 429, code: "IVR_RATE_LIMITED", has_backoff: true },
    {
      status: refused.status,
      code: refused.body?.error?.code ?? null,
      has_backoff: Number(details.retry_after_seconds) >= 1
        && Number(details.requests_per_window) >= 1,
    },
    `retry_after_seconds=${details.retry_after_seconds}, `
    + `requests_per_window=${details.requests_per_window}`);
}

async function main() {
  await preflight();
  await assertCleanSandbox();
  await prepare();
  const fixture = loadFixture();
  const admitted = await admitScenarios(fixture);
  const eligible = await recordEligibility(admitted);
  await awaitResults(eligible);
  await negativeExamples(fixture);
  await quotaExample(fixture);

  const report = {
    schema_version: "ivr.sandbox-examples.v1",
    generated_at: new Date().toISOString(),
    base_url: BASE_URL,
    safety: "MOCK/MOCK/NO; synthetic fixtures; no real destination is reachable",
    examples: observations.length,
    failures: failures.length,
    observations,
  };
  const output = join(REPOSITORY_ROOT, ".artifacts", "sandbox", "module-3-examples.json");
  mkdirSync(dirname(output), { recursive: true });
  writeFileSync(output, `${JSON.stringify(report, null, 2)}\n`, "utf8");

  console.log(`\n${observations.length - failures.length}/${observations.length} examples behaved `
    + `as documented. Report: ${output}`);
  if (failures.length > 0) {
    console.error(`\n${failures.length} did not:`);
    for (const failure of failures) {
      console.error(`  - ${failure}`);
    }

    process.exitCode = 1;
  }
}

try {
  await main();
} catch (error) {
  // A guidance failure is a sentence to read, not a stack to decode. Anything genuinely unexpected
  // still shows its stack, because that is when a stack is the useful thing.
  if (error instanceof GuidanceError) {
    console.error(`\n${error.message}\n`);
    process.exitCode = 1;
  } else {
    throw error;
  }
}
