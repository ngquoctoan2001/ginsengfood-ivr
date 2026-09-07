#!/usr/bin/env node
/**
 * W-0203 / P1.2. The full worker pipeline, driven end to end against deterministic fakes, in a
 * loop, with faults injected on purpose.
 *
 * What this is for
 * ----------------
 * `Invoke-LocalE2E.ps1` (W-0193) proves the pipeline can carry five tasks once. That is a
 * different claim from the one P1.2 has to make, which is that the pipeline carries traffic
 * REPEATEDLY without losing or duplicating a business outcome, and that every retry inside it
 * stops. A single pass can show neither: nothing that happens once can be called stable, and an
 * unbounded retry looks exactly like a bounded one until you watch it end.
 *
 * So this harness runs the same stack for N rounds, asserts the whole outcome ledger after every
 * round, and - before the loop starts - breaks the stack on purpose in the ways the plan names:
 * a worker killed mid-call, a lease left to expire, the kill switch thrown between claim and
 * dial, the callback receiver taken away and given back, and two workers competing for the same
 * work throughout.
 *
 * Safety
 * ------
 * IVR_EXECUTION_MODE stays MOCK, SIM_PROVIDER stays MOCK and REAL_CUSTOMER_CALL_ALLOWED stays NO
 * for the whole run. MockSchedulerDispatchGateway.IsReady requires all three, so this harness
 * cannot reach a vendor even if one were configured. The only safety lifted is the MOCK kill
 * switch, and it is lifted against a fake gateway every one of whose destinations is the same
 * allowlisted fake string.
 *
 * Usage
 * -----
 *   node tools/dev/Invoke-LocalMockE2E.mjs [--rounds 100] [--workers 2] [--skip-faults]
 *                                          [--keep-running] [--policy mock-lab-v1]
 */

import { spawn, spawnSync } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const HERE = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(HERE, '../..');

// ---------------------------------------------------------------------------------------------
// Options
// ---------------------------------------------------------------------------------------------

function readArgs(argv) {
  const options = {
    rounds: 100,
    workers: 2,
    skipFaults: false,
    keepRunning: false,
    policy: 'mock-lab-v1',
    apiPort: 5015,
    salesPort: 18085,
    extendedEvery: 10,
  };
  for (let index = 0; index < argv.length; index += 1) {
    const flag = argv[index];
    const value = argv[index + 1];
    switch (flag) {
      case '--rounds': options.rounds = Number(value); index += 1; break;
      case '--workers': options.workers = Number(value); index += 1; break;
      case '--policy': options.policy = value; index += 1; break;
      case '--api-port': options.apiPort = Number(value); index += 1; break;
      case '--sales-port': options.salesPort = Number(value); index += 1; break;
      case '--extended-every': options.extendedEvery = Number(value); index += 1; break;
      case '--skip-faults': options.skipFaults = true; break;
      case '--keep-running': options.keepRunning = true; break;
      default:
        if (flag.startsWith('--')) throw new Error(`Unknown flag ${flag}.`);
    }
  }
  if (!Number.isInteger(options.rounds) || options.rounds < 1) {
    throw new Error('--rounds must be a positive integer.');
  }
  if (!Number.isInteger(options.workers) || options.workers < 1 || options.workers > 8) {
    throw new Error('--workers must be between 1 and 8.');
  }
  return options;
}

const OPTIONS = readArgs(process.argv.slice(2));

const POSTGRES_CONTAINER = 'ginsengfood-ivr-dev-postgres-1';
const SALES_CONTAINER = 'ivr-e2e-mock-sales';
const API_URL = `http://127.0.0.1:${OPTIONS.apiPort}`;
const API_BASE = `${API_URL}/v1/ivr/order-confirmation`;
const SALES_URL = `http://127.0.0.1:${OPTIONS.salesPort}`;
const LOG_DIR = join(ROOT, 'ci-artifacts', 'local-mock-e2e');
const EVIDENCE_DIR = join(ROOT, 'docs', 'evidence', 'W-0203');

const ORDER_CORE_TOKEN = 'dev-ordercore-token-not-a-real-secret';
const INTERNAL_TOKEN = 'dev-internal-token-not-a-real-secret';
const DANGER_TOKEN = 'dev-admin-danger-token-not-a-real-secret';

const RUN_ID = new Date().toISOString().replace(/[^0-9]/g, '').slice(8, 14);
/** Every task this run creates carries this, so one run's rows can be told from another's. */
const RUN_MARK = `-${RUN_ID}-`;

// ---------------------------------------------------------------------------------------------
// The scenario matrix. This table IS the specification under test.
//
// `dispatches` is how many times the scheduler may dial a task before it settles, and it is
// stated rather than derived because it is the number that says whether a retry is bounded. A
// scenario whose observed dispatch count exceeds it has an unbounded retry, and that is a
// failure even when the final result looks right.
// ---------------------------------------------------------------------------------------------

const GOLDEN_HOUR = { program: 'GOLDEN_HOUR', payment: 'ONLINE', windowSeconds: 300, secondOffset: 150 };
const TWENTY_FOUR_SEVEN = { program: 'TWENTY_FOUR_SEVEN', payment: 'COD', windowSeconds: 900, secondOffset: 450 };

/** Scenarios run on every round: the ordinary shape of traffic. */
const CORE_MATRIX = [
  { code: 'CONFIRM', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
  { code: 'CANCEL', plan: GOLDEN_HOUR, result: 'IVR_CUSTOMER_CANCELLED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
  { code: 'NOANSWER', plan: GOLDEN_HOUR, result: 'IVR_NO_ANSWER_FINAL', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 2 },
  { code: 'BUSY', plan: TWENTY_FOUR_SEVEN, result: 'IVR_NO_ANSWER_FINAL', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 2 },
  { code: 'NOINPUT', plan: GOLDEN_HOUR, result: 'IVR_NO_ANSWER_FINAL', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 2 },
  { code: 'WRONGKEY', plan: TWENTY_FOUR_SEVEN, result: 'IVR_NO_ANSWER_FINAL', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 2 },
  { code: 'BADNUMBER', plan: TWENTY_FOUR_SEVEN, result: 'IVR_INVALID_PHONE_FINAL', counted: false, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
  // A technical exception is never a customer attempt, never final, and therefore never a
  // callback. It stops after TechnicalRetryLimit + 1 dials and the job is parked for review.
  { code: 'AUDIOERR', plan: GOLDEN_HOUR, result: null, counted: null, delivery: null, retries: null, dispatches: 2, heldStatus: 'HELD_ADMIN_REVIEW' },
  { code: 'ACKOK', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
  { code: 'ACKDUP', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
];

/** Scenarios run every `--extended-every` rounds: the rest of the ACK and disposition taxonomy. */
const EXTENDED_MATRIX = [
  { code: 'REJECTED', plan: GOLDEN_HOUR, result: 'IVR_NO_ANSWER_FINAL', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 2 },
  { code: 'UNREACH', plan: TWENTY_FOUR_SEVEN, result: 'IVR_INVALID_PHONE_FINAL', counted: false, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 },
  { code: 'DTMFERR', plan: GOLDEN_HOUR, result: null, counted: null, delivery: null, retries: null, dispatches: 2, heldStatus: 'HELD_ADMIN_REVIEW' },
  { code: 'DROPPED', plan: GOLDEN_HOUR, result: null, counted: null, delivery: null, retries: null, dispatches: 2, heldStatus: 'HELD_ADMIN_REVIEW' },
  { code: 'ACKBLOCK', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_BLOCKED', retries: 0, dispatches: 1 },
  { code: 'ACKREVIEW', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_REVIEW', retries: 0, dispatches: 1 },
  { code: 'ACKSTALE', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'REJECTED_STALE', retries: 0, dispatches: 1 },
  { code: 'ACKCONFLICT', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'IDEMPOTENCY_CONFLICT', retries: 0, dispatches: 1 },
  { code: 'ACK422', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'INVALID_DEAD_LETTER', retries: 0, dispatches: 1 },
  { code: 'ACK429', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'RETRY_EXHAUSTED', retries: 3, dispatches: 1 },
  { code: 'ACK500', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'RETRY_EXHAUSTED', retries: 3, dispatches: 1 },
];

// ---------------------------------------------------------------------------------------------
// Small helpers
// ---------------------------------------------------------------------------------------------

const started = [];
const failures = [];
const timeline = [];
let stopping = false;

const sleep = (ms) => new Promise((done) => setTimeout(done, ms));

function step(text) {
  process.stdout.write(`\n== ${text}\n`);
}

function note(text) {
  process.stdout.write(`   ${text}\n`);
}

function fail(text) {
  failures.push(text);
  process.stdout.write(`   FAIL ${text}\n`);
}

function psql(sql) {
  const result = spawnSync(
    'docker',
    ['exec', POSTGRES_CONTAINER, 'psql', '-U', 'ivr', '-d', 'ivr', '-t', '-A', '-F', '|', '-c', sql],
    { encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 },
  );
  if (result.status !== 0) {
    throw new Error(`psql failed: ${result.stderr || result.stdout}`);
  }
  return result.stdout.split('\n').map((line) => line.trim()).filter((line) => line.length > 0);
}

function psqlRows(sql) {
  return psql(sql).map((line) => line.split('|'));
}

function scalar(sql) {
  const rows = psql(sql);
  return rows.length > 0 ? rows[0] : '';
}

async function waitFor(description, predicate, { timeoutMs = 120_000, intervalMs = 250 } = {}) {
  const deadline = Date.now() + timeoutMs;
  for (;;) {
    const answer = await predicate();
    if (answer && answer.done) return answer;
    if (Date.now() > deadline) {
      throw new Error(`Timed out waiting for ${description}: ${JSON.stringify(answer)}`);
    }
    await sleep(intervalMs);
  }
}

// ---------------------------------------------------------------------------------------------
// Process and container control
// ---------------------------------------------------------------------------------------------

function baseEnvironment() {
  return {
    ...process.env,
    ASPNETCORE_ENVIRONMENT: 'Development',
    DOTNET_ENVIRONMENT: 'Development',
    IVR_CONFIG_PROFILE: 'LocalMockE2E',
    // Restated here even though the profile carries them. A safety posture asserted in only one
    // place is a safety posture one edit away from being lost.
    IVR_EXECUTION_MODE: 'MOCK',
    IVR_ADAPTER_MODE: 'MOCK',
    SIM_PROVIDER: 'MOCK',
    SALES_PROVIDER: 'FAKE_TARGET_V1',
    REAL_CUSTOMER_CALL_ALLOWED: 'NO',
    ConnectionStrings__IvrDb: 'Host=127.0.0.1;Port=55433;Database=ivr;Username=ivr',
    Ivr__CallbackDelivery__TargetBaseUrl: SALES_URL,
  };
}

function launch(name, exePath, cwd, extraEnvironment = {}) {
  const child = spawn(exePath, [], {
    cwd,
    env: { ...baseEnvironment(), ...extraEnvironment },
    stdio: ['ignore', 'pipe', 'pipe'],
    windowsHide: true,
  });
  const logPath = join(LOG_DIR, `${name}.log`);
  const chunks = [];
  const capture = (chunk) => {
    chunks.push(chunk.toString());
    if (chunks.length > 6000) chunks.splice(0, 3000);
    try { writeFileSync(logPath, chunks.join('')); } catch { /* the log is a convenience */ }
  };
  child.stdout.on('data', capture);
  child.stderr.on('data', capture);
  child.on('error', (error) => fail(`${name} failed to start: ${error.message}`));
  const record = { kind: 'process', name, child, logPath, exePath, cwd, extraEnvironment };
  started.push(record);
  return record;
}

function killProcess(record, { hard = true } = {}) {
  if (!record.child || record.child.exitCode !== null) return;
  if (process.platform === 'win32') {
    const args = ['/T', '/PID', String(record.child.pid)];
    if (hard) args.unshift('/F');
    spawnSync('taskkill', args, { stdio: 'ignore' });
  } else {
    record.child.kill(hard ? 'SIGKILL' : 'SIGTERM');
  }
}

function forget(record) {
  const index = started.indexOf(record);
  if (index >= 0) started.splice(index, 1);
}

function docker(args, { allowFailure = false } = {}) {
  const result = spawnSync('docker', args, { encoding: 'utf8' });
  if (result.status !== 0 && !allowFailure) {
    throw new Error(`docker ${args.join(' ')} failed: ${result.stderr || result.stdout}`);
  }
  return (result.stdout || '').trim();
}

function startFakeSales() {
  docker(['rm', '-f', SALES_CONTAINER], { allowFailure: true });
  docker([
    'run', '-d', '--name', SALES_CONTAINER,
    '-p', `127.0.0.1:${OPTIONS.salesPort}:8080`,
    '-v', `${join(ROOT, 'deploy', 'docker', 'fake-sales-e2e')}:/home/wiremock/mappings:ro`,
    'wiremock/wiremock:3.9.1',
    '--max-request-journal-entries', '60000',
  ]);
  if (!started.some((item) => item.kind === 'container' && item.name === SALES_CONTAINER)) {
    started.push({ kind: 'container', name: SALES_CONTAINER });
  }
}

function stopEverything() {
  if (stopping) return;
  stopping = true;
  for (const item of [...started]) {
    if (item.kind === 'process') killProcess(item);
    else docker(['rm', '-f', item.name], { allowFailure: true });
  }
}

// ---------------------------------------------------------------------------------------------
// Intake
// ---------------------------------------------------------------------------------------------

function taskBody(scenario, taskId, now) {
  const plan = scenario.plan;
  // Back-dated so that BOTH attempt offsets are already due. The policy schedules its second
  // customer attempt 150s (Golden Hour) or 450s (24/7) after the window opens; a rehearsal that
  // waited for that in real time would take eight minutes per no-answer task and would therefore
  // never be run. Moving the window START rather than the clock keeps every other rule honest:
  // the deadline still closes at start + duration, the policy snapshot is untouched, and intake
  // still checks the declared offsets against the registered policy byte for byte.
  const start = new Date(now.getTime() - (plan.secondOffset + 10) * 1000);
  const end = new Date(start.getTime() + plan.windowSeconds * 1000);
  const programName = plan.program === 'GOLDEN_HOUR' ? 'Gio Vang' : 'Ban hang 24/7';
  return {
    contract_version: 'ivr-order-confirmation.v1',
    task_id: taskId,
    correlation_id: `corr-${taskId}`,
    created_at: start.toISOString(),
    order_id: `ORD-${taskId}`,
    order_code: 'GF-E2E-001',
    order_code_short: 'E2E001',
    order_version: '17',
    order_state: 'CONFIRMING',
    payment_method_snapshot: plan.payment,
    ivr_confirmation_required: true,
    is_ivr_callable: true,
    program_code: plan.program,
    confirmation_window_started_at: start.toISOString(),
    confirmation_window_expires_at: end.toISOString(),
    attempt_policy_version: OPTIONS.policy,
    max_customer_attempts: 2,
    attempt_offsets_seconds: [0, plan.secondOffset],
    phone_ref: `phone-ref-${taskId}`,
    phone_masked: '84xxxxx0001',
    phone_validation_status: 'VALID',
    dial_token: `dial-token-${taskId}`,
    dial_token_expires_at: end.toISOString(),
    privacy_safe_order_summary: {
      customer_display_name: 'chi An',
      order_code_short: 'E2E001',
      items: [{ public_name: 'Nuoc hong sam', quantity: 2, unit_label: 'hop' }],
      total_amount: 560000,
      currency: 'VND',
      delivery_area_short: 'Phuong Ben Nghe, Quan Mot',
      program_display_name: programName,
      locale: 'vi-VN',
    },
    call_restriction: false,
    eligibility_snapshot: {
      decision: 'ELIGIBLE',
      source_version: 'sales-eligibility-v1',
      captured_at: start.toISOString(),
      source_available: true,
      blockers: [],
      voice_restriction: { restricted: false, source_available: true, source_version: 'sales-voice-v1' },
    },
    evidence_ref: `evidence://local-mock-e2e/${taskId}`,
  };
}

async function postJson(url, headers, body) {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...headers },
    body: JSON.stringify(body),
  });
  const text = await response.text();
  let parsed = null;
  try { parsed = text ? JSON.parse(text) : null; } catch { parsed = null; }
  return { status: response.status, body: parsed, raw: text };
}

/**
 * Finding F-1, and the reason this counter exists.
 *
 * `POST /eligibility-checks` runs inside PostgresIdempotencyStore.ExecuteAsync, which opens a
 * SERIALIZABLE transaction, reads ivr_idempotency_keys and inserts into it. Concurrent requests
 * carrying DIFFERENT idempotency keys therefore form a dangerous structure under SSI, and all but
 * one abort at COMMIT with SQLSTATE 40001. The API turns that into an opaque HTTP 500
 * (IVR_INTERNAL_ERROR), so a caller cannot tell a retryable conflict from a broken service.
 *
 * Ten concurrent admissions reproduced it on the first round of the first run: nine of ten failed.
 * Every P1.1 API test admits sequentially, which is why it had never been seen.
 *
 * The harness retries rather than admitting serially, because admitting serially would hide the
 * defect instead of measuring it. Every retry is counted, and the count is published in the
 * evidence: as long as it is above zero, the workaround is load-bearing and the defect is open.
 */
const conflictRetries = { intake: 0, eligibility: 0, exhausted: 0 };
const MAX_CONFLICT_RETRIES = 12;
/**
 * How many admissions the loop runs at once. Two, not ten, and that is a concession to F-1
 * rather than a design choice: at ten the endpoint fails so persistently that even a dozen
 * retries cannot get a round in. The unbounded case is still measured, once, deliberately, by
 * concurrencyProbe() - so the number below is a workaround with a receipt attached, not a quietly
 * chosen level of load that happens to pass.
 */
const ADMIT_CONCURRENCY = 2;

async function mapLimit(items, limit, worker) {
  const results = new Array(items.length);
  let next = 0;
  const runners = Array.from({ length: Math.min(limit, items.length) }, async () => {
    for (;;) {
      const index = next;
      next += 1;
      if (index >= items.length) return;
      results[index] = await worker(items[index], index);
    }
  });
  await Promise.all(runners);
  return results;
}

function isRetryableApiConflict(response) {
  return response.status === 500 && response.body?.error?.code === 'IVR_INTERNAL_ERROR';
}

async function postWithConflictRetry(counter, url, headers, body) {
  let attempt = 0;
  for (;;) {
    const response = await postJson(url, headers, body);
    if (!isRetryableApiConflict(response)) return response;
    if (attempt >= MAX_CONFLICT_RETRIES) { conflictRetries.exhausted += 1; return response; }
    attempt += 1;
    conflictRetries[counter] += 1;
    // Jittered, because an unjittered backoff makes the contenders retry in step and collide
    // again on exactly the same schedule.
    await sleep(40 * attempt + Math.floor(Math.random() * 120));
  }
}

async function admit(scenario, taskId) {
  const intake = await postWithConflictRetry('intake', `${API_BASE}/tasks`, {
    Authorization: `Bearer ${ORDER_CORE_TOKEN}`,
    'X-Source-System': 'order-core',
    'X-Correlation-Id': `corr-${taskId}`,
    'Idempotency-Key': `idem-${taskId}`,
  }, taskBody(scenario, taskId, new Date()));
  if (intake.status >= 300) {
    throw new Error(`intake ${taskId} returned ${intake.status}: ${intake.raw.slice(0, 400)}`);
  }
  const eligibility = await postWithConflictRetry('eligibility', `${API_BASE}/eligibility-checks`, {
    Authorization: `Bearer ${INTERNAL_TOKEN}`,
    'X-Source-System': 'ivr-worker',
    'X-Service-Scope': 'ivr.internal.write',
    'X-Correlation-Id': `corr-${taskId}`,
    'Idempotency-Key': `elig-${taskId}`,
  }, { task_id: taskId });
  if (eligibility.body?.decision !== 'ELIGIBLE_FOR_IVR') {
    throw new Error(`eligibility ${taskId} returned ${eligibility.status} ${JSON.stringify(eligibility.body)}`);
  }
  return taskId;
}

function taskIdFor(code, round, index) {
  return `E2E-${code}-${RUN_ID}-${String(round).padStart(4, '0')}${index}`;
}

async function admitRound(matrix, round) {
  return mapLimit(matrix, ADMIT_CONCURRENCY, async (scenario, index) => {
    const taskId = taskIdFor(scenario.code, round, index);
    await admit(scenario, taskId);
    return { scenario, taskId };
  });
}

/**
 * F-1, measured rather than asserted. Ten admissions at once, with the client retry switched off,
 * and the answer recorded exactly as it comes back. This runs once per rehearsal: it is the
 * receipt behind ADMIT_CONCURRENCY, and if it ever reports zero failures the workaround can go.
 */
async function concurrencyProbe() {
  step('Concurrency probe: 10 simultaneous admissions, no client retry');
  const scenario = { code: 'CONFIRM', plan: GOLDEN_HOUR };
  const ids = Array.from({ length: 10 }, (_, index) => `E2E-PROBE-${RUN_ID}-P${index}`);
  const intakeOutcomes = await Promise.all(ids.map((taskId) => postJson(`${API_BASE}/tasks`, {
    Authorization: `Bearer ${ORDER_CORE_TOKEN}`,
    'X-Source-System': 'order-core',
    'X-Correlation-Id': `corr-${taskId}`,
    'Idempotency-Key': `idem-${taskId}`,
  }, taskBody(scenario, taskId, new Date()))));
  const eligibilityOutcomes = await Promise.all(ids.map((taskId) => postJson(
    `${API_BASE}/eligibility-checks`, {
      Authorization: `Bearer ${INTERNAL_TOKEN}`,
      'X-Source-System': 'ivr-worker',
      'X-Service-Scope': 'ivr.internal.write',
      'X-Correlation-Id': `corr-${taskId}`,
      'Idempotency-Key': `elig-${taskId}`,
    }, { task_id: taskId })));
  const probe = {
    concurrent_requests: ids.length,
    intake_500: intakeOutcomes.filter((item) => item.status === 500).length,
    intake_ok: intakeOutcomes.filter((item) => item.status < 300).length,
    eligibility_500: eligibilityOutcomes.filter((item) => item.status === 500).length,
    eligibility_ok: eligibilityOutcomes.filter(
      (item) => item.body?.decision === 'ELIGIBLE_FOR_IVR').length,
  };
  note(`intake ${probe.intake_ok}/${ids.length} ok, `
    + `eligibility ${probe.eligibility_ok}/${ids.length} ok `
    + `(${probe.eligibility_500} answered HTTP 500)`);
  // Whatever the probe found, the tasks it created must not be left half-admitted: they would
  // otherwise sit in the database as jobs nothing ever finishes and pollute the next run.
  for (const taskId of ids) {
    for (let attempt = 0; attempt < 6; attempt += 1) {
      const retry = await postJson(`${API_BASE}/eligibility-checks`, {
        Authorization: `Bearer ${INTERNAL_TOKEN}`,
        'X-Source-System': 'ivr-worker',
        'X-Service-Scope': 'ivr.internal.write',
        'X-Correlation-Id': `corr-${taskId}`,
        'Idempotency-Key': `elig-${taskId}`,
      }, { task_id: taskId });
      if (retry.body?.decision === 'ELIGIBLE_FOR_IVR') break;
      await sleep(150);
    }
  }
  return probe;
}

// ---------------------------------------------------------------------------------------------
// Observation
// ---------------------------------------------------------------------------------------------

/**
 * One query per round rather than one per task. The point is not speed: a per-task query would
 * read each task at a slightly different instant, and "no duplicates" asserted against a moving
 * database is not an assertion at all.
 */
function observe(taskIds) {
  const list = taskIds.map((id) => `'${id}'`).join(',');
  const sql = `
    SELECT t.task_id,
           COALESCE(f.final_count, 0),
           COALESCE(f.result_type, ''),
           COALESCE(f.is_counted::text, ''),
           COALESCE(a.dispatch_count, 0),
           COALESCE(c.callback_count, 0),
           COALESCE(c.delivery_status, ''),
           COALESCE(c.retry_count, -1),
           COALESCE(j.job_status, ''),
           COALESCE(r.result_count, 0)
    FROM (SELECT unnest(ARRAY[${list}]) AS task_id) t
    LEFT JOIN (SELECT task_id, COUNT(*) AS final_count, MAX(result_type) AS result_type,
                      BOOL_OR(is_counted_customer_attempt) AS is_counted
               FROM ivr_call_results WHERE is_final_for_ivr IS TRUE AND task_id IN (${list})
               GROUP BY task_id) f ON f.task_id = t.task_id
    LEFT JOIN (SELECT task_id, COUNT(*) AS result_count FROM ivr_call_results
               WHERE task_id IN (${list}) GROUP BY task_id) r ON r.task_id = t.task_id
    LEFT JOIN (SELECT task_id, COUNT(*) AS dispatch_count FROM ivr_call_attempts
               WHERE task_id IN (${list}) GROUP BY task_id) a ON a.task_id = t.task_id
    LEFT JOIN (SELECT task_id, COUNT(*) AS callback_count, MAX(delivery_status) AS delivery_status,
                      MAX(retry_count) AS retry_count
               FROM ivr_result_callbacks WHERE task_id IN (${list})
               GROUP BY task_id) c ON c.task_id = t.task_id
    LEFT JOIN (SELECT task_id, MAX(status) AS job_status FROM ivr_call_jobs
               WHERE task_id IN (${list}) GROUP BY task_id) j ON j.task_id = t.task_id`;
  const byTask = new Map();
  for (const row of psqlRows(sql.replace(/\s+/g, ' '))) {
    byTask.set(row[0], {
      taskId: row[0],
      finalCount: Number(row[1]),
      resultType: row[2],
      counted: row[3] === 't' || row[3] === 'true',
      dispatches: Number(row[4]),
      callbackCount: Number(row[5]),
      delivery: row[6],
      retryCount: Number(row[7]),
      jobStatus: row[8],
      resultCount: Number(row[9]),
    });
  }
  return byTask;
}

const TERMINAL_DELIVERY = new Set([
  'DELIVERED_ACCEPTED', 'DELIVERED_BLOCKED', 'DELIVERED_REVIEW', 'REJECTED_STALE',
  'IDEMPOTENCY_CONFLICT', 'INVALID_DEAD_LETTER', 'AUTH_REJECTED', 'RETRY_EXHAUSTED',
]);

function isSettled(scenario, seen) {
  if (!seen) return false;
  if (scenario.result === null) {
    // A technical-exception scenario settles when it has stopped being dialled and its job has
    // been parked. "Stopped" is the assertion; a job still eligible for another dial is not done.
    return seen.resultCount >= scenario.dispatches && seen.jobStatus === scenario.heldStatus;
  }
  if (seen.finalCount < 1) return false;
  if (scenario.delivery === null) return true;
  return seen.callbackCount >= 1 && TERMINAL_DELIVERY.has(seen.delivery);
}

function checkRound(admitted, byTask, label) {
  let ok = true;
  for (const { scenario, taskId } of admitted) {
    const seen = byTask.get(taskId);
    const where = `${label} ${scenario.code}`;
    if (!seen) { fail(`${where}: no row for ${taskId}.`); ok = false; continue; }

    if (scenario.result === null) {
      if (seen.finalCount !== 0) { fail(`${where}: expected no final result, saw ${seen.finalCount}.`); ok = false; }
      if (seen.callbackCount !== 0) { fail(`${where}: a technical exception produced ${seen.callbackCount} callback(s).`); ok = false; }
      if (seen.jobStatus !== scenario.heldStatus) { fail(`${where}: job is "${seen.jobStatus}", expected ${scenario.heldStatus}.`); ok = false; }
    } else {
      if (seen.finalCount !== 1) { fail(`${where}: ${seen.finalCount} final results, expected exactly 1.`); ok = false; }
      if (seen.resultType !== scenario.result) { fail(`${where}: result ${seen.resultType}, expected ${scenario.result}.`); ok = false; }
      if (seen.counted !== scenario.counted) { fail(`${where}: counted=${seen.counted}, expected ${scenario.counted}.`); ok = false; }
      if (seen.callbackCount !== 1) { fail(`${where}: ${seen.callbackCount} callback rows, expected exactly 1.`); ok = false; }
      if (seen.delivery !== scenario.delivery) { fail(`${where}: delivery ${seen.delivery}, expected ${scenario.delivery}.`); ok = false; }
      if (scenario.retries !== null && seen.retryCount !== scenario.retries) {
        fail(`${where}: retry_count ${seen.retryCount}, expected ${scenario.retries}.`); ok = false;
      }
    }

    // The bounded-retry assertion. Not "did it finish" but "did it stop where policy said stop".
    if (seen.dispatches > scenario.dispatches) {
      fail(`${where}: ${seen.dispatches} dispatches, policy bounds it at ${scenario.dispatches}.`);
      ok = false;
    }
  }
  return ok;
}

/**
 * Invariants worth asserting separately from any one scenario, because a plausible-looking bug
 * breaks them silently: no outcome that was not a customer answering may be stored as a customer
 * attempt, no task may hold two final results, and no task may hold two callback rows.
 */
function checkGlobalInvariants() {
  const like = `'E2E-%${RUN_MARK}%'`;
  const miscounted = Number(scalar(
    `SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE ${like} `
    + `AND is_counted_customer_attempt IS TRUE AND result_type IN `
    + `('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_INVALID_PHONE_FINAL',`
    + `'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')`));
  const duplicateFinals = Number(scalar(
    `SELECT COUNT(*) FROM (SELECT task_id FROM ivr_call_results WHERE task_id LIKE ${like} `
    + `AND is_final_for_ivr IS TRUE GROUP BY task_id HAVING COUNT(*) > 1) d`));
  const duplicateCallbacks = Number(scalar(
    `SELECT COUNT(*) FROM (SELECT task_id FROM ivr_result_callbacks WHERE task_id LIKE ${like} `
    + `GROUP BY task_id HAVING COUNT(*) > 1) d`));
  if (miscounted !== 0) fail(`${miscounted} non-customer result(s) are stored as counted customer attempts.`);
  if (duplicateFinals !== 0) fail(`${duplicateFinals} task(s) hold more than one final result.`);
  if (duplicateCallbacks !== 0) fail(`${duplicateCallbacks} task(s) hold more than one callback row.`);
  return { miscounted, duplicateFinals, duplicateCallbacks };
}

// ---------------------------------------------------------------------------------------------
// Bring the stack up
// ---------------------------------------------------------------------------------------------

function preflight() {
  step('Preflight');
  const running = docker(['ps', '--filter', `name=${POSTGRES_CONTAINER}`, '--format', '{{.Names}}']);
  if (!running.includes(POSTGRES_CONTAINER)) {
    throw new Error(`${POSTGRES_CONTAINER} is not running. Start it with: pnpm db:up`);
  }
  const migrations = Number(scalar('SELECT COUNT(*) FROM "__EFMigrationsHistory"'));
  if (migrations === 0) throw new Error('No migrations are applied. Run: pnpm db:migrate');
  const policies = psql(
    `SELECT program_type FROM ivr_attempt_policies WHERE policy_version = '${OPTIONS.policy}'`);
  if (policies.length < 2) {
    throw new Error(
      `Attempt policy ${OPTIONS.policy} is not registered for both programs. Run: pnpm db:migrate`);
  }
  note(`postgres ok, ${migrations} migrations, policy ${OPTIONS.policy} present`);
  mkdirSync(LOG_DIR, { recursive: true });
  mkdirSync(EVIDENCE_DIR, { recursive: true });
}

async function waitForApi() {
  await waitFor('the API to become ready', async () => {
    try {
      const response = await fetch(`${API_URL}/health/ready`);
      return { done: response.status === 200, status: response.status };
    } catch (error) {
      return { done: false, error: String(error.message ?? error) };
    }
  }, { timeoutMs: 90_000, intervalMs: 500 });
}

async function waitForSales() {
  await waitFor('the fake Sales stub to answer', async () => {
    try {
      const response = await fetch(`${SALES_URL}/__admin/health`);
      return { done: response.ok };
    } catch { return { done: false }; }
  }, { timeoutMs: 60_000, intervalMs: 500 });
}

function startWorker(index, extraEnvironment = {}) {
  return launch(
    `worker-${index}`,
    join(ROOT, 'src', 'Ivr.Worker', 'bin', 'Release', 'net10.0', 'Ivr.Worker.exe'),
    join(ROOT, 'src', 'Ivr.Worker'),
    extraEnvironment,
  );
}

async function startStack() {
  step('Fake Sales, API and workers');
  startFakeSales();
  await waitForSales();
  launch(
    'api',
    join(ROOT, 'src', 'Ivr.Api', 'bin', 'Release', 'net10.0', 'Ivr.Api.exe'),
    join(ROOT, 'src', 'Ivr.Api'),
    { ASPNETCORE_URLS: API_URL },
  );
  await waitForApi();
  const workers = [];
  for (let index = 1; index <= OPTIONS.workers; index += 1) {
    workers.push(startWorker(index));
  }
  await sleep(4000);
  for (const worker of workers) {
    if (worker.child.exitCode !== null) {
      throw new Error(`${worker.name} exited during startup; see ${worker.logPath}`);
    }
  }
  note(`api ${API_URL}, ${workers.length} worker(s), fake Sales ${SALES_URL}`);
  note(`logs: ${LOG_DIR}`);
  return workers;
}

// ---------------------------------------------------------------------------------------------
// Fault injection
// ---------------------------------------------------------------------------------------------

/**
 * F1/F2. A worker is killed while it is holding a call, and the lease it held is left to expire.
 *
 * The assertion is deliberately NOT "the task finishes anyway". A crash during a call leaves the
 * system unable to say whether the customer heard anything, and the designed answer to that is to
 * stop rather than to guess: the lease is reclaimed, the channel is handed back, and the job is
 * parked for a human. Re-dialling would be the bug. So what is checked is that recovery happens,
 * that it happens without a second dial, and that the surviving worker never stops working.
 */
async function faultCrashAndLease(workers) {
  step('Fault 1/2: worker killed mid-call, lease left to expire');

  // Exactly one worker runs for the abandonment, and that is not a simplification - it is the
  // difference between a test and a coin toss. With two workers up, "an attempt is in flight"
  // does not say WHICH process holds it, so killing one of them abandons a call only when the
  // guess is right. The first two-worker run of this phase guessed wrong: the call belonged to
  // the survivor, finished normally, and the assertions failed against a system that had done
  // nothing wrong. The replacements come back immediately afterwards, and the "queue kept
  // moving" check below is what they are there to prove.
  for (const worker of workers) { killProcess(worker); forget(worker); }
  await sleep(1500);
  const victim = startWorker(91);
  await sleep(3500);
  if (victim.child.exitCode !== null) {
    throw new Error(`the crash-test worker exited during startup; see ${victim.logPath}`);
  }

  const cohort = [];
  const slow = { code: 'SLOW', plan: GOLDEN_HOUR, result: null, counted: null, delivery: null, retries: null, dispatches: 1 };
  for (let index = 0; index < 4; index += 1) {
    const taskId = `E2E-SLOW-${RUN_ID}-F1${index}`;
    await admit(slow, taskId);
    cohort.push(taskId);
  }
  const inFlight = await waitFor('a call to be in flight', async () => {
    const active = Number(scalar(
      `SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' `
      + `AND status IN ('DIALING','ACTIVE_CALL','LEASED_PENDING_DISPATCH')`));
    return { done: active > 0, active };
  }, { timeoutMs: 60_000, intervalMs: 200 });
  note(`${inFlight.active} attempt(s) in flight; killing ${victim.name} hard`);
  killProcess(victim, { hard: true });
  forget(victim);

  // The replacements come up straight away, and they have to: lease recovery is work that a
  // RUNNING scheduler loop does, so a fleet with nothing running recovers nothing. Waiting for
  // quarantine before restarting would have been waiting for a machine that was switched off.
  const survivors = [];
  for (let index = 1; index <= OPTIONS.workers; index += 1) survivors.push(startWorker(index));

  // LeaseDurationSeconds is 30 and RecoveryQuarantineSeconds is 5, so recovery is a real wait
  // rather than an instant. That wait IS the test: a lease that is reclaimed immediately would
  // mean the lease was never protecting anything.
  const recovered = await waitFor('the abandoned lease to be quarantined and released', async () => {
    const rows = psqlRows(
      `SELECT COALESCE(status,''), COUNT(*) FROM ivr_call_attempts `
      + `WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' GROUP BY status`);
    const byStatus = Object.fromEntries(rows.map((row) => [row[0], Number(row[1])]));
    const stuck = (byStatus.DIALING ?? 0) + (byStatus.ACTIVE_CALL ?? 0)
      + (byStatus.LEASED_PENDING_DISPATCH ?? 0);
    return { done: stuck === 0, byStatus };
  }, { timeoutMs: 120_000, intervalMs: 500 });
  note(`attempt states after recovery: ${JSON.stringify(recovered.byStatus)}`);

  const recoveryRequired = Number(scalar(
    `SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' `
    + `AND status = 'RECOVERY_REQUIRED'`));
  if (recoveryRequired === 0) {
    fail('the killed worker left no attempt in RECOVERY_REQUIRED; nothing was recovered.');
  }
  const held = psqlRows(
    `SELECT status, queue_status, COUNT(*) FROM ivr_call_jobs `
    + `WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' GROUP BY status, queue_status`);
  const heldForReview = held
    .filter((row) => row[0] === 'HELD_ADMIN_REVIEW' && row[1] === 'HELD_LEASE_RECOVERY')
    .reduce((total, row) => total + Number(row[2]), 0);
  if (heldForReview === 0) {
    fail('no job was parked as HELD_ADMIN_REVIEW/HELD_LEASE_RECOVERY after the crash.');
  }
  // No second dial for a recovered attempt: re-dialling a customer whose call state is unknown is
  // exactly the duplicate this whole mechanism exists to prevent.
  const redialled = Number(scalar(
    `SELECT COUNT(*) FROM (SELECT task_id FROM ivr_call_attempts `
    + `WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' GROUP BY task_id HAVING COUNT(*) > 1) d`));
  if (redialled !== 0) {
    fail(`${redialled} recovered task(s) were dialled again after the crash.`);
  }
  const channelsIdle = await waitFor('quarantined channels to return to service', async () => {
    const quarantined = Number(scalar(
      "SELECT COUNT(*) FROM ivr_sim_channels WHERE status = 'QUARANTINED'"));
    return { done: quarantined === 0, quarantined };
  }, { timeoutMs: 90_000, intervalMs: 500 });

  // A crash must cost exactly the calls that were in flight. The rest of the cohort has to keep
  // moving, because the failure mode worth fearing is not "one task was lost" but "the queue
  // stopped and nobody noticed".
  const drained = await waitFor('the rest of the cohort to keep moving after the crash', async () => {
    const finals = Number(scalar(
      `SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F1%' `
      + `AND is_final_for_ivr IS TRUE`));
    return { done: finals + recoveryRequired >= cohort.length, finals };
  }, { timeoutMs: 180_000, intervalMs: 500 });
  note(`after recovery the queue kept moving: ${drained.finals} of the remaining `
    + `${cohort.length - recoveryRequired} task(s) finished`);

  timeline.push({
    phase: 'crash-and-lease-recovery',
    cohort: cohort.length,
    inFlightAtKill: inFlight.active,
    recoveryRequired,
    heldForReview,
    redialled,
    channelsQuarantinedAfterRelease: channelsIdle.quarantined,
    finishedAfterRecovery: drained.finals,
  });
  note(`recovery_required=${recoveryRequired} held=${heldForReview} redialled=${redialled}`);
  return survivors;
}

/**
 * F3a. The kill switch, engaged.
 *
 * Two facts about this system had to be learned by running it, and both are worth writing down.
 *
 * First, `Ivr:Telephony:Mock:KillSwitchEngaged=true` together with `Enabled=true` does not start:
 * MockTelephonyOptionsValidator refuses the combination outright. So the engaged posture is not
 * "armed but held" - it is "not armed", by construction, checked before the process is allowed to
 * exist. That is a stronger guarantee than a runtime flag and it is why both settings are used
 * together below.
 *
 * Second, the switch stops the NEXT call, not calls already under way; ending those is a separate
 * press with a separate route, which F3b exercises. IvrAdminEndpoints says so in as many words,
 * and the pair is the reason "kill switch between claim and dial" needs two tests rather than one.
 */
async function faultKillSwitch(workers) {
  step('Fault 3a: kill switch engaged - nothing is claimed and nothing is dialled');
  for (const worker of workers) { killProcess(worker); forget(worker); }
  await sleep(1500);
  const engaged = startWorker(90, {
    Ivr__Telephony__Mock__Enabled: 'false',
    Ivr__Telephony__Mock__KillSwitchEngaged: 'true',
  });
  await sleep(4000);
  if (engaged.child.exitCode !== null) {
    throw new Error(`the kill-switch worker exited during startup; see ${engaged.logPath}`);
  }

  const cohort = [];
  const scenario = { code: 'KILL', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: 0, dispatches: 1 };
  for (let index = 0; index < 3; index += 1) {
    const taskId = `E2E-KILL-${RUN_ID}-F3${index}`;
    await admit(scenario, taskId);
    cohort.push({ scenario, taskId });
  }
  // Ten seconds is a hundred scheduler polls at the profile's 100 ms interval. If a dial were
  // going to happen it would have happened many times over.
  await sleep(10_000);
  const dialled = Number(scalar(
    `SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE 'E2E-KILL-${RUN_ID}-F3%'`));
  if (dialled !== 0) {
    fail(`the kill switch was engaged and ${dialled} attempt(s) were still dialled.`);
  }
  const results = Number(scalar(
    `SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE 'E2E-KILL-${RUN_ID}-F3%'`));
  if (results !== 0) {
    fail(`the kill switch was engaged and ${results} result(s) were still produced.`);
  }
  note(`kill switch engaged: ${dialled} attempts, ${results} results after 100 scheduler polls`);

  killProcess(engaged); forget(engaged);
  await sleep(1500);
  const restored = [];
  for (let index = 1; index <= OPTIONS.workers; index += 1) restored.push(startWorker(index));
  await sleep(3000);
  await waitFor('the withheld tasks to complete once the switch is released', async () => {
    const seen = observe(cohort.map((item) => item.taskId));
    const settled = cohort.filter((item) => isSettled(item.scenario, seen.get(item.taskId))).length;
    return { done: settled === cohort.length, settled, of: cohort.length };
  }, { timeoutMs: 120_000, intervalMs: 500 });
  checkRound(cohort, observe(cohort.map((item) => item.taskId)), 'kill-switch-release');
  timeline.push({
    phase: 'kill-switch',
    withheldTasks: cohort.length,
    attemptsWhileEngaged: dialled,
    resultsWhileEngaged: results,
    completedAfterRelease: cohort.length,
    note: 'Enabled=false is part of the engaged posture: MockTelephonyOptionsValidator refuses '
      + 'to start a worker whose telephony is armed while the switch is engaged.',
  });
  note('kill switch released: every withheld task completed exactly once');
  return restored;
}

/**
 * F3b. The other press: ending calls that are already under way.
 *
 * A conversation in progress is not stopped by the kill switch - that stops the next call - so the
 * operator route POST /call-jobs:terminate-all exists, and MockSchedulerDispatchGateway polls for
 * the request while it is waiting for a keypress. What must come out of it is a technical
 * termination, NOT a customer outcome: an operator cutting a call has not learned anything about
 * what the customer wanted, and recording it as if they had would be the worst kind of quiet
 * corruption. So the assertion is on the result type, not merely on the call ending.
 */
async function faultTerminateInFlight() {
  step('Fault 3b: an operator ends calls that are already under way');
  const slow = { code: 'SLOW', plan: GOLDEN_HOUR };
  const cohort = [];
  for (let index = 0; index < 2; index += 1) {
    const taskId = `E2E-SLOW-${RUN_ID}-F6${index}`;
    await admit(slow, taskId);
    cohort.push(taskId);
  }
  await waitFor('a call to be under way', async () => {
    const active = Number(scalar(
      `SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F6%' `
      + `AND ended_at IS NULL AND started_at IS NOT NULL AND provider_call_id IS NOT NULL`));
    return { done: active > 0, active };
  }, { timeoutMs: 60_000, intervalMs: 200 });

  const response = await postJson(`${API_BASE}/call-jobs:terminate-all`, {
    Authorization: `Bearer ${DANGER_TOKEN}`,
    'X-Service-Scope': 'ivr.admin.danger',
    'X-Actor-Id': 'local-mock-e2e-operator',
    'X-Action-Reason': 'W-0203 rehearsal: end calls already under way',
    'X-Correlation-Id': `corr-terminate-${RUN_ID}`,
    'Idempotency-Key': `terminate-${RUN_ID}`,
  }, { reason: 'W-0203 rehearsal: end calls already under way' });
  if (response.status !== 200) {
    fail(`terminate-all returned ${response.status}: ${response.raw.slice(0, 300)}`);
    return;
  }

  const ended = await waitFor('the cut calls to be recorded', async () => {
    const rows = psqlRows(
      `SELECT COALESCE(result_type,''), COUNT(*) FROM ivr_call_results `
      + `WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F6%' GROUP BY result_type`);
    const byType = Object.fromEntries(rows.map((row) => [row[0], Number(row[1])]));
    return { done: Object.keys(byType).length > 0, byType };
  }, { timeoutMs: 90_000, intervalMs: 500 });

  const confirmed = ended.byType.IVR_CONFIRMED ?? 0;
  const technical = ended.byType.IVR_TECHNICAL_EXCEPTION ?? 0;
  if (confirmed > 0) {
    fail(`${confirmed} operator-terminated call(s) were recorded as a customer confirmation.`);
  }
  if (technical === 0) {
    fail('an operator-terminated call produced no technical result.');
  }
  const counted = Number(scalar(
    `SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE 'E2E-SLOW-${RUN_ID}-F6%' `
    + `AND is_counted_customer_attempt IS TRUE`));
  if (counted !== 0) {
    fail(`${counted} operator-terminated call(s) were counted as customer attempts.`);
  }
  note(`terminate-all: ${JSON.stringify(ended.byType)}, counted customer attempts ${counted}`);
  timeline.push({
    phase: 'terminate-in-flight',
    cohort: cohort.length,
    resultTypes: ended.byType,
    countedCustomerAttempts: counted,
  });
}

/**
 * F4a. The callback receiver disappears and comes back.
 *
 * A transport failure is transient by definition, so the outbox must hold the message, back off,
 * and deliver it when the receiver returns - without inventing a second business outcome and
 * without retrying for ever.
 */
async function faultCallbackOutage() {
  step('Fault 4a: callback receiver outage and recovery');
  const scenario = { code: 'ACKOK', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'DELIVERED_ACCEPTED', retries: null, dispatches: 1 };
  docker(['stop', SALES_CONTAINER]);
  const cohort = [];
  for (let index = 0; index < 4; index += 1) {
    const taskId = `E2E-ACKOK-${RUN_ID}-F4${index}`;
    await admit(scenario, taskId);
    cohort.push({ scenario, taskId });
  }
  const pending = await waitFor('deliveries to start failing while the receiver is down', async () => {
    const rows = psqlRows(
      `SELECT delivery_status, COUNT(*) FROM ivr_result_callbacks `
      + `WHERE task_id LIKE 'E2E-ACKOK-${RUN_ID}-F4%' GROUP BY delivery_status`);
    const byStatus = Object.fromEntries(rows.map((row) => [row[0], Number(row[1])]));
    return { done: (byStatus.RETRY_PENDING ?? 0) > 0, byStatus };
  }, { timeoutMs: 90_000, intervalMs: 250 });
  note(`during the outage: ${JSON.stringify(pending.byStatus)}`);

  docker(['start', SALES_CONTAINER]);
  await waitForSales();
  const recovered = await waitFor('every held callback to deliver once the receiver returns', async () => {
    const seen = observe(cohort.map((item) => item.taskId));
    const delivered = cohort.filter(
      (item) => seen.get(item.taskId)?.delivery === 'DELIVERED_ACCEPTED').length;
    return { done: delivered === cohort.length, delivered, of: cohort.length };
  }, { timeoutMs: 180_000, intervalMs: 500 });
  const seen = observe(cohort.map((item) => item.taskId));
  for (const item of cohort) {
    const row = seen.get(item.taskId);
    if (row.callbackCount !== 1) fail(`outage recovery ${item.taskId}: ${row.callbackCount} callback rows.`);
    if (row.finalCount !== 1) fail(`outage recovery ${item.taskId}: ${row.finalCount} final results.`);
    if (row.retryCount > 3) fail(`outage recovery ${item.taskId}: retry_count ${row.retryCount} exceeds MaxRetries.`);
  }
  timeline.push({
    phase: 'callback-outage',
    cohort: cohort.length,
    statusesDuringOutage: pending.byStatus,
    deliveredAfterRecovery: recovered.delivered,
    maxRetryCount: Math.max(...cohort.map((item) => seen.get(item.taskId).retryCount)),
  });
  note(`recovered ${recovered.delivered}/${cohort.length}, highest retry_count `
    + `${Math.max(...cohort.map((item) => seen.get(item.taskId).retryCount))} of 3 allowed`);
}

/**
 * F4b. Dead letter, then replay.
 *
 * A stub that always answers 500 drives the cohort to RETRY_EXHAUSTED, which is the dead-letter
 * state: bounded, visible and countable rather than a message that quietly keeps trying. The stub
 * is then removed and the rows are re-queued the way an operator would have to re-queue them
 * today - by hand, in SQL, because no admin replay endpoint exists yet. That gap is the finding,
 * and it is recorded in the evidence rather than papered over.
 */
async function faultDeadLetterAndReplay() {
  step('Fault 4b: dead letter and replay');
  const stub = await fetch(`${SALES_URL}/__admin/mappings`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      priority: 1,
      request: {
        method: 'POST',
        urlPathPattern: `/api/v1/internal/orders/ORD-E2E-DLQ-${RUN_ID}-[^/]+/ivr-result-callbacks`,
      },
      response: { status: 500, jsonBody: { error: 'INJECTED_OUTAGE' } },
    }),
  }).then((response) => response.json());

  const scenario = { code: 'DLQ', plan: GOLDEN_HOUR, result: 'IVR_CONFIRMED', counted: true, delivery: 'RETRY_EXHAUSTED', retries: 3, dispatches: 1 };
  const cohort = [];
  for (let index = 0; index < 3; index += 1) {
    const taskId = `E2E-DLQ-${RUN_ID}-F5${index}`;
    await admit(scenario, taskId);
    cohort.push({ scenario, taskId });
  }
  const exhausted = await waitFor('the cohort to reach the dead-letter state', async () => {
    const seen = observe(cohort.map((item) => item.taskId));
    const dead = cohort.filter(
      (item) => seen.get(item.taskId)?.delivery === 'RETRY_EXHAUSTED').length;
    return { done: dead === cohort.length, dead, of: cohort.length };
  }, { timeoutMs: 180_000, intervalMs: 500 });
  const beforeReplay = observe(cohort.map((item) => item.taskId));
  for (const item of cohort) {
    const row = beforeReplay.get(item.taskId);
    if (row.retryCount !== 3) {
      fail(`dead letter ${item.taskId}: retry_count ${row.retryCount}, MaxRetries is 3.`);
    }
  }
  note(`${exhausted.dead} callback(s) dead-lettered at retry_count 3`);

  await fetch(`${SALES_URL}/__admin/mappings/${stub.id}`, { method: 'DELETE' });
  const replayed = Number(scalar(
    `WITH replayed AS (UPDATE ivr_result_callbacks SET delivery_status = 'RETRY_PENDING', `
    + `retry_count = 0, next_retry_at = now(), lease_token = NULL, lease_expires_at = NULL `
    + `WHERE task_id LIKE 'E2E-DLQ-${RUN_ID}-F5%' AND delivery_status = 'RETRY_EXHAUSTED' `
    + `RETURNING 1) SELECT COUNT(*) FROM replayed`));
  note(`replayed ${replayed} dead-lettered callback(s)`);
  const delivered = await waitFor('the replayed cohort to deliver', async () => {
    const seen = observe(cohort.map((item) => item.taskId));
    const ok = cohort.filter(
      (item) => seen.get(item.taskId)?.delivery === 'DELIVERED_ACCEPTED').length;
    return { done: ok === cohort.length, ok, of: cohort.length };
  }, { timeoutMs: 120_000, intervalMs: 500 });
  const afterReplay = observe(cohort.map((item) => item.taskId));
  for (const item of cohort) {
    const row = afterReplay.get(item.taskId);
    if (row.callbackCount !== 1) fail(`replay ${item.taskId}: ${row.callbackCount} callback rows.`);
    if (row.finalCount !== 1) fail(`replay ${item.taskId}: ${row.finalCount} final results.`);
  }
  timeline.push({
    phase: 'dead-letter-and-replay',
    cohort: cohort.length,
    deadLettered: exhausted.dead,
    retryCountAtDeath: 3,
    replayedRows: replayed,
    deliveredAfterReplay: delivered.ok,
    replayPath: 'manual SQL re-queue; no admin replay endpoint exists yet (finding)',
  });
}

// ---------------------------------------------------------------------------------------------
// The steady loop
// ---------------------------------------------------------------------------------------------

async function runRounds() {
  step(`Steady loop: ${OPTIONS.rounds} round(s), ${OPTIONS.workers} worker(s)`);
  const perRound = [];
  let admittedTotal = 0;
  let settledTotal = 0;
  for (let round = 1; round <= OPTIONS.rounds; round += 1) {
    const extended = OPTIONS.extendedEvery > 0 && round % OPTIONS.extendedEvery === 0;
    const matrix = extended ? [...CORE_MATRIX, ...EXTENDED_MATRIX] : CORE_MATRIX;
    const startedAt = Date.now();
    const admitted = await admitRound(matrix, round);
    admittedTotal += admitted.length;
    const ids = admitted.map((item) => item.taskId);
    let last;
    await waitFor(`round ${round} to settle`, async () => {
      const seen = observe(ids);
      const settled = admitted.filter((item) => isSettled(item.scenario, seen.get(item.taskId)));
      last = seen;
      return { done: settled.length === admitted.length, settled: settled.length, of: admitted.length };
    }, { timeoutMs: 240_000, intervalMs: 400 });
    const ok = checkRound(admitted, last, `round ${round}`);
    settledTotal += admitted.length;
    const elapsed = Date.now() - startedAt;
    perRound.push({ round, extended, tasks: admitted.length, milliseconds: elapsed, ok });
    if (round === 1 || extended || round % 10 === 0 || !ok) {
      note(`round ${String(round).padStart(4)} ${extended ? 'extended' : 'core    '} `
        + `${String(admitted.length).padStart(2)} tasks ${String(elapsed).padStart(6)} ms `
        + `${ok ? 'ok' : 'FAIL'}`);
    }
    if (!ok) break;
  }
  return { perRound, admittedTotal, settledTotal };
}

// ---------------------------------------------------------------------------------------------
// Retention
// ---------------------------------------------------------------------------------------------

/**
 * The retention pass runs once per worker start, which is how a CronJob invokes it. So the proof
 * is: back-date one finished cohort past the configured period, restart a worker, and ask for
 * those rows back - while a second, untouched cohort must still be there afterwards. Without the
 * second cohort the test would pass just as well against a job that deleted everything.
 */
async function retentionProof(workers) {
  step('Retention: a back-dated cohort is purged, live rows are not');
  const purgePattern = `E2E-CONFIRM-${RUN_ID}-%`;
  const keepPattern = `E2E-CANCEL-${RUN_ID}-%`;
  const before = {
    purge: Number(scalar(`SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${purgePattern}'`)),
    keep: Number(scalar(`SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${keepPattern}'`)),
  };
  if (before.purge === 0 || before.keep === 0) {
    fail('the retention proof has no cohort to work with; the steady loop produced nothing.');
    return { before, after: before, skipped: true };
  }

  // ivr_task_intake_outbox.created_at is immutable by trigger - it is append-only intake
  // evidence - so the harness CANNOT age it. That is not a gap in the test, it is the shape of
  // the test: with the outbox row still inside its period, ivr_call_jobs is dependency-blocked
  // and ivr_confirmation_tasks behind it, so this run proves both halves of the contract at once.
  // What is past its period is deleted; what still has dependent evidence is refused. A proof
  // that only showed deletion would pass equally well against a job that deleted everything.
  const backdate = "now() - interval '400 days'";
  psql(`UPDATE ivr_result_callbacks SET created_at = ${backdate} WHERE task_id LIKE '${purgePattern}'`);
  psql(`UPDATE ivr_call_results SET created_at = ${backdate} WHERE task_id LIKE '${purgePattern}'`);
  psql(`UPDATE ivr_raw_call_events SET received_at = ${backdate} WHERE ivr_call_attempt_id IN `
    + `(SELECT ivr_call_attempt_id FROM ivr_call_attempts WHERE task_id LIKE '${purgePattern}')`);
  psql(`UPDATE ivr_technical_exceptions SET created_at = ${backdate} WHERE ivr_call_attempt_id IN `
    + `(SELECT ivr_call_attempt_id FROM ivr_call_attempts WHERE task_id LIKE '${purgePattern}')`);
  psql(`UPDATE ivr_call_attempts SET scheduled_at = ${backdate} WHERE task_id LIKE '${purgePattern}'`);
  psql(`UPDATE ivr_confirmation_tasks SET created_at = ${backdate} WHERE task_id LIKE '${purgePattern}'`);

  const restarted = [];
  for (const worker of workers) { killProcess(worker); forget(worker); }
  await sleep(1500);
  for (let index = 1; index <= OPTIONS.workers; index += 1) restarted.push(startWorker(index));

  const after = await waitFor('the back-dated cohort to be purged', async () => {
    const callbacks = Number(scalar(
      `SELECT COUNT(*) FROM ivr_result_callbacks WHERE task_id LIKE '${purgePattern}'`));
    const results = Number(scalar(
      `SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE '${purgePattern}'`));
    const attempts = Number(scalar(
      `SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE '${purgePattern}'`));
    return { done: callbacks === 0 && results === 0 && attempts === 0, callbacks, results, attempts };
  }, { timeoutMs: 150_000, intervalMs: 1000 });

  const redacted = Number(scalar(
    `SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${purgePattern}' `
    + `AND phone_ref = 'redacted' AND phone_validation_status = 'REDACTED'`));
  const dependencyHeld = {
    jobs: Number(scalar(`SELECT COUNT(*) FROM ivr_call_jobs WHERE task_id LIKE '${purgePattern}'`)),
    tasks: Number(scalar(`SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${purgePattern}'`)),
    outbox: Number(scalar(`SELECT COUNT(*) FROM ivr_task_intake_outbox WHERE task_id LIKE '${purgePattern}'`)),
  };
  const survived = {
    callbacks: Number(scalar(`SELECT COUNT(*) FROM ivr_result_callbacks WHERE task_id LIKE '${keepPattern}'`)),
    results: Number(scalar(`SELECT COUNT(*) FROM ivr_call_results WHERE task_id LIKE '${keepPattern}'`)),
    attempts: Number(scalar(`SELECT COUNT(*) FROM ivr_call_attempts WHERE task_id LIKE '${keepPattern}'`)),
    tasks: Number(scalar(`SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${keepPattern}'`)),
    unredacted: Number(scalar(
      `SELECT COUNT(*) FROM ivr_confirmation_tasks WHERE task_id LIKE '${keepPattern}' `
      + `AND phone_ref <> 'redacted'`)),
  };

  if (redacted !== before.purge) {
    fail(`retention redacted ${redacted} of ${before.purge} aged speech snapshot(s).`);
  }
  if (dependencyHeld.tasks !== before.purge || dependencyHeld.jobs === 0) {
    fail('retention deleted a task or job whose intake-outbox evidence is still inside its period.');
  }
  if (survived.tasks !== before.keep || survived.unredacted !== before.keep) {
    fail(`retention touched ${before.keep - survived.unredacted} in-period task(s).`);
  }
  if (survived.callbacks === 0 || survived.results === 0 || survived.attempts === 0) {
    fail('retention removed in-period callbacks, results or attempts.');
  }
  note(`aged cohort: callbacks/results/attempts deleted, ${redacted} speech snapshot(s) redacted, `
    + `${dependencyHeld.jobs} job(s) correctly held by in-period outbox evidence`);
  note(`in-period cohort untouched: ${survived.tasks} task(s), ${survived.callbacks} callback(s)`);
  const outcome = { before, after, redacted, dependencyHeld, survived };
  timeline.push({ phase: 'retention', ...outcome });
  return { ...outcome, workers: restarted };
}

// ---------------------------------------------------------------------------------------------
// Delivery ledger, taken from the receiver rather than from us
// ---------------------------------------------------------------------------------------------

/**
 * The database can only say what this system believes it sent. The receiver's journal says what
 * actually arrived, which is the only place a duplicate delivery would be visible at all.
 *
 * A redelivery is allowed - the outbox is at-least-once by design. What is not allowed is a
 * redelivery that the receiver could not recognise as the same message, so the assertion is that
 * every request carrying one callback id also carries one idempotency key and one payload.
 */
async function deliveryLedger() {
  step('Delivery ledger from the receiver journal');
  const journal = await fetch(`${SALES_URL}/__admin/requests?limit=60000`)
    .then((response) => response.json());
  const byCallback = new Map();
  for (const entry of journal.requests ?? []) {
    const url = entry.request?.url ?? '';
    if (!url.includes('/ivr-result-callbacks')) continue;
    let body = null;
    try { body = JSON.parse(entry.request.body); } catch { continue; }
    const callbackId = body.callback_id;
    if (!callbackId || !String(body.task_id ?? '').includes(RUN_MARK)) continue;
    const headers = entry.request.headers ?? {};
    const key = headers['Idempotency-Key'] ?? headers['idempotency-key'] ?? '';
    const seen = byCallback.get(callbackId) ?? { deliveries: 0, keys: new Set(), payloads: new Set() };
    seen.deliveries += 1;
    seen.keys.add(String(key));
    seen.payloads.add(entry.request.body);
    byCallback.set(callbackId, seen);
  }
  let redelivered = 0;
  let inconsistent = 0;
  for (const [callbackId, seen] of byCallback) {
    if (seen.deliveries > 1) redelivered += 1;
    if (seen.keys.size > 1 || seen.payloads.size > 1) {
      inconsistent += 1;
      fail(`callback ${callbackId} was delivered ${seen.deliveries} times with `
        + `${seen.keys.size} idempotency key(s) and ${seen.payloads.size} distinct payload(s).`);
    }
  }
  note(`${byCallback.size} callback id(s) delivered, ${redelivered} redelivered at least once, `
    + `${inconsistent} inconsistent`);
  return { callbackIds: byCallback.size, redelivered, inconsistent };
}

// ---------------------------------------------------------------------------------------------
// Evidence
// ---------------------------------------------------------------------------------------------

function writeEvidence(summary) {
  const path = join(EVIDENCE_DIR, 'local-mock-e2e.json');
  writeFileSync(path, `${JSON.stringify(summary, null, 2)}\n`);
  note(`evidence: ${path}`);
  return path;
}

// ---------------------------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------------------------

async function main() {
  const startedAt = new Date();
  preflight();
  let workers = await startStack();
  const probe = await concurrencyProbe();
  let faults = null;
  if (!OPTIONS.skipFaults) {
    workers = await faultCrashAndLease(workers);
    workers = await faultKillSwitch(workers);
    await faultTerminateInFlight();
    await faultCallbackOutage();
    await faultDeadLetterAndReplay();
    faults = timeline.filter((item) => item.phase !== 'retention');
  } else {
    note('fault injection skipped by --skip-faults');
  }

  const loop = await runRounds();
  const invariants = checkGlobalInvariants();
  const ledger = await deliveryLedger();
  const retention = await retentionProof(workers);
  if (retention.workers) workers = retention.workers;

  const rounds = loop.perRound;
  const completedRounds = rounds.filter((item) => item.ok).length;
  const durations = rounds.map((item) => item.milliseconds).sort((a, b) => a - b);
  const summary = {
    work_id: 'W-0203',
    plan_item: 'P1.2 Full worker pipeline',
    profile: 'LocalMockE2E',
    generated_at: new Date().toISOString(),
    started_at: startedAt.toISOString(),
    run_id: RUN_ID,
    safety: {
      execution_mode: 'MOCK',
      sim_provider: 'MOCK',
      real_customer_call_allowed: 'NO',
      destination_allowlist: ['mock-destination-allowlisted'],
    },
    configuration: {
      rounds_requested: OPTIONS.rounds,
      workers: OPTIONS.workers,
      extended_every: OPTIONS.extendedEvery,
      attempt_policy_version: OPTIONS.policy,
      api: API_URL,
      fake_sales: SALES_URL,
    },
    coverage: {
      core_scenarios: CORE_MATRIX.map((row) => row.code),
      extended_scenarios: EXTENDED_MATRIX.map((row) => row.code),
      faults: OPTIONS.skipFaults
        ? []
        : ['worker-crash-mid-call', 'lease-expiry', 'kill-switch-engaged',
           'operator-terminate-in-flight', 'callback-outage', 'dead-letter-and-replay',
           'two-worker-concurrency'],
    },
    loop: {
      rounds_completed: completedRounds,
      tasks_admitted: loop.admittedTotal,
      tasks_settled: loop.settledTotal,
      round_milliseconds: durations.length === 0 ? null : {
        min: durations[0],
        median: durations[Math.floor(durations.length / 2)],
        max: durations[durations.length - 1],
      },
    },
    invariants,
    open_findings: [
      {
        id: 'F-1',
        severity: 'HIGH',
        title: 'Concurrent writes with distinct idempotency keys fail with HTTP 500',
        detail:
          'PostgresIdempotencyStore.ExecuteAsync runs SERIALIZABLE, reads ivr_idempotency_keys and '
          + 'inserts into it, so concurrent requests with different keys abort at COMMIT with '
          + 'SQLSTATE 40001. The API maps that to IVR_INTERNAL_ERROR / HTTP 500 rather than a '
          + 'retryable answer. Reproduced by ten concurrent POST /eligibility-checks.',
        harness_workaround:
          `bounded client retry on 500 plus an admission concurrency of ${ADMIT_CONCURRENCY}`,
        measured: probe,
        conflict_retries: conflictRetries,
        fix_scope:
          'MutationReplayFilter wraps whole endpoint invocations in the same Serializable '
          + 'envelope, so a blanket transaction retry would re-run HTTP handlers. Each call site '
          + 'has to be shown retry-safe first; that is its own work item, not a patch here.',
      },
    ],
    delivery_ledger: ledger,
    faults,
    retention: {
      before: retention.before,
      after: retention.after,
      speech_snapshots_redacted: retention.redacted,
      dependency_held: retention.dependencyHeld,
      in_period_survivors: retention.survived,
    },
    failures,
    verdict: failures.length === 0 && completedRounds === OPTIONS.rounds ? 'PASS' : 'FAIL',
  };
  writeEvidence(summary);

  step('Result');
  note(`rounds ${completedRounds}/${OPTIONS.rounds}, tasks ${loop.admittedTotal}, `
    + `failures ${failures.length}`);
  if (summary.verdict === 'PASS') {
    process.stdout.write('\nLocalMockE2E PASSED.\n');
  } else {
    process.stdout.write('\nLocalMockE2E FAILED:\n');
    for (const item of failures.slice(0, 40)) process.stdout.write(`  - ${item}\n`);
    if (failures.length > 40) process.stdout.write(`  ... ${failures.length - 40} more\n`);
  }
  return summary.verdict === 'PASS' ? 0 : 1;
}

process.on('SIGINT', () => { stopEverything(); process.exit(130); });

let exitCode = 1;
try {
  exitCode = await main();
} catch (error) {
  process.stdout.write(`\nLocalMockE2E ABORTED: ${error?.stack ?? error}\n`);
  exitCode = 2;
} finally {
  if (!OPTIONS.keepRunning) {
    stopEverything();
  } else {
    note(`left running: ${API_URL}, fake Sales ${SALES_URL}. `
      + `Stop with: docker rm -f ${SALES_CONTAINER}; then close the Ivr.* processes.`);
  }
}
process.exit(exitCode);
