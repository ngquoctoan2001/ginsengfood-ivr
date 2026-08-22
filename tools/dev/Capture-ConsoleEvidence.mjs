#!/usr/bin/env node
// W-0102 captured the Phase 3 screens by hand and wrote up the result. What it did not leave
// behind was a way to do it again, so every later capture starts by reconstructing the harness
// from prose. This file is that harness.
//
// Option A, still: no browser dependency. Node built-ins only, so running this widens neither the
// lockfile, the gitleaks surface nor the PII scan surface -- the same reasoning W-0097 used to keep
// a third-party skill out of the tree.
//
// It captures three things a test cannot:
//
//   1. The enum values the LIVE API actually emits. `enum-coverage.test.ts` proves the dictionary
//      covers every enum the OpenAPI spec DECLARES. That is a different set. Open families --
//      technical_exception_type takes whatever code the SIM provider invents, order_state belongs
//      to Order Core (D-02) -- are absent from the spec by design, so no spec-driven test can see
//      them. They reach the operator's screen anyway, and when the dictionary has no entry the
//      screen falls back to the raw code with a warning marker (NT-4). This sweep is the only
//      thing that names those values before an operator meets one.
//   2. That the W-0105 session guard actually redirects, per route, from a running server.
//   3. The visible text of the screens reachable without a session.
//
// SAFETY. Read-only: every request is a GET. The mock permission headers it sends are rejected
// outright by the API unless IVR_EXECUTION_MODE=MOCK (MockPermissionHeaderGuardMiddleware), so
// this cannot authenticate against a lab or production instance -- it fails closed rather than
// capturing something it should not see. Nothing here can place a call.
//
// Usage:
//   node tools/dev/Capture-ConsoleEvidence.mjs
//   IVR_API_BASE=http://127.0.0.1:5015 IVR_UI_BASE=http://127.0.0.1:3007 node tools/dev/...

import { readFileSync, readdirSync, writeFileSync, mkdirSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");

const apiBase = process.env.IVR_API_BASE ?? "http://127.0.0.1:5015";
const uiBase = process.env.IVR_UI_BASE ?? "http://127.0.0.1:3007";
const outputPath = resolve(
  repositoryRoot,
  process.env.IVR_EVIDENCE_OUT ?? "docs/evidence/W-0107/live-enum-coverage.txt",
);

const ACTOR = "AGT-ADMIN-01";
const PERMISSIONS = [
  "IVR_QUEUE_VIEW",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
  "IVR_RESULT_REVIEW",
  "IVR_MANUAL_RETRY",
  "IVR_FLAG_READ",
  "IVR_RUNTIME_GATE_ADMIN",
  "IVR_SIM_DISABLE",
  "IVR_SIM_ENABLE",
].join(",");

const API_PATH = "/v1/ivr/order-confirmation";

/** Read-only surface. Mutations are deliberately absent: a capture must not change what it captures. */
const READ_ENDPOINTS = [
  "/dashboard",
  "/call-jobs",
  "/call-attempts",
  "/call-results",
  "/result-callbacks",
  "/eligibility-checks",
  "/technical-retries",
  "/admin-reviews",
  "/review-items",
  "/queue",
  "/scripts",
  "/sim-channels",
  "/integration-status",
  "/analytics/summary",
  "/analytics/breakdown",
  "/analytics/trend",
];

/** Every console route, so the guard sweep cannot quietly skip one. */
const CONSOLE_ROUTES = [
  "/",
  "/login",
  "/dashboard",
  "/calls",
  "/queue",
  "/reports",
  "/review",
  "/config",
  "/integration",
  "/seed",
  "/roles",
  "/accounts",
  "/profile",
];

/**
 * Which dictionary family each API field is rendered through.
 *
 * This is READ OUT OF THE SCREENS, not written down here. A hand-maintained table would only
 * record what the author believed the console does, and the whole point of a live capture is to
 * stop believing. `<EnumLabel family="deliveryStatus" value={callback.delivery_status} />` is the
 * fact; this parses it.
 *
 * A field no screen renders is reported separately rather than checked against a guessed family --
 * `closed_reason` has a dictionary entry but reaches no screen, and calling that a coverage gap
 * would be inventing one.
 */
function readRenderedFamilies(screenSources) {
  const pairs = new Map();

  for (const [file, source] of screenSources) {
    // Matches both orderings and both components, across the line breaks Prettier introduces:
    //   <EnumLabel family="jobStatus" value={item.status} />
    //   <EnumLabelList\n family="blockedReason"\n values={detail.blocked_reasons}\n />
    const usage =
      /<EnumLabel(?:List)?\b[\s\S]{0,240}?family="([A-Za-z]+)"[\s\S]{0,240}?values?=\{([^}]+)\}/gu;

    for (const match of source.matchAll(usage)) {
      const [, family, expression] = match;

      // `detail.eligibility_decision` -> eligibility_decision; `row.role` -> role. A template
      // literal (integration/page.tsx builds `DIAL_KILL_SWITCH_${key}`) has no field to bind, so
      // it is skipped rather than bound to something arbitrary.
      const field = /([A-Za-z_][A-Za-z0-9_]*)\s*$/u.exec(expression.trim())?.[1];
      if (field === undefined || expression.includes("`")) {
        continue;
      }

      const existing = pairs.get(field) ?? { families: new Set(), sites: [] };
      existing.families.add(family);
      existing.sites.push(`${file}`);
      pairs.set(field, existing);
    }
  }

  return pairs;
}

/**
 * Candidates adjudicated as not-defects, with the reason and the file that settles it.
 *
 * The binding above is keyed on the bare field name, so two unrelated payload fields that happen
 * to share a name collide. Rather than weaken the check until the collision stops appearing, the
 * one verified case is suppressed by name with its justification attached -- which keeps the exit
 * code meaningful: non-zero means a gap nobody has looked at yet.
 *
 * A line here is a claim about the code and goes stale like any other. Each carries the file that
 * would have to change for it to stop being true.
 */
const ADJUDICATED = [
  {
    field: "source",
    value: "ANALYTICS_WAREHOUSE",
    reason:
      "data_quality.source is interpolated into a sentence by FreshnessBanner and never passes " +
      "through EnumLabel; it collides with the integration screen's event.source",
    settledBy: "admin-ui/src/components/reports/FreshnessBanner.tsx",
  },
];

/** Enum-shaped: SCREAMING_SNAKE, or one of the three region words voice_region uses. */
const ENUM_SHAPE = /^[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)+$/u;
const REGION_SHAPE = /^(?:North|Central|South)$/u;

let correlationCounter = 0;
function nextCorrelationId() {
  correlationCounter += 1;
  const suffix = String(correlationCounter).padStart(4, "0");
  return `corr-cap0-0000-0000-0000-0000-0000-0000-${suffix}`;
}

async function apiGet(path) {
  const response = await fetch(`${apiBase}${API_PATH}${path}`, {
    headers: {
      Accept: "application/json",
      "X-Correlation-Id": nextCorrelationId(),
      "X-Permissions": PERMISSIONS,
      "X-Mock-Actor-Id": ACTOR,
      "X-Actor-Id": ACTOR,
    },
    redirect: "manual",
  });

  const text = await response.text();
  let body;
  try {
    body = text.trim() === "" ? undefined : JSON.parse(text);
  } catch {
    body = undefined;
  }

  return { status: response.status, body };
}

/**
 * Walks a response and records every enum-shaped value against the field name that carried it.
 *
 * Keyed on the bare field name because that is what the screens bind to -- `value={item.status}`
 * is written against `status`, not against a path. One name can reach several families (`status`
 * is rendered as jobStatus, simStatus, scriptStatus, reviewStatus and accountStatus on five
 * different screens), which is handled at check time rather than by guessing here.
 */
function harvest(node, fieldName, path, sink) {
  if (Array.isArray(node)) {
    for (const item of node) {
      harvest(item, fieldName, `${path}[]`, sink);
    }
    return;
  }

  if (node !== null && typeof node === "object") {
    for (const [key, value] of Object.entries(node)) {
      harvest(value, key, `${path}.${key}`, sink);
    }
    return;
  }

  if (typeof node !== "string" || fieldName === null) {
    return;
  }

  if (!ENUM_SHAPE.test(node) && !REGION_SHAPE.test(node)) {
    return;
  }

  // The full path travels with the value, because a bare field name collides. `source` is
  // `fail_closed_events[].source` on the integration screen and something else entirely under
  // analytics -- without the path, one of those two would be reported as the other's gap.
  const byValue = sink.get(fieldName) ?? new Map();
  const paths = byValue.get(node) ?? new Set();
  paths.add(path);
  byValue.set(node, paths);
  sink.set(fieldName, byValue);
}

/** Every .tsx under the console, so a screen cannot be missed by naming it in a list. */
function readScreenSources() {
  const roots = [
    resolve(repositoryRoot, "admin-ui/src/app"),
    resolve(repositoryRoot, "admin-ui/src/components"),
  ];
  const found = [];

  const walk = (directory) => {
    for (const entry of readdirSync(directory, { withFileTypes: true })) {
      const full = resolve(directory, entry.name);
      if (entry.isDirectory()) {
        walk(full);
      } else if (entry.name.endsWith(".tsx")) {
        // Forward slashes: this path is quoted in evidence read on Linux CI as well as Windows.
        const relative = full.slice(repositoryRoot.length + 1).replaceAll("\\", "/");
        found.push([relative, readFileSync(full, "utf8")]);
      }
    }
  };

  for (const root of roots) {
    walk(root);
  }

  return found;
}

async function main() {
  const dictionary = JSON.parse(
    readFileSync(resolve(repositoryRoot, "admin-ui/src/i18n/enums.vi.json"), "utf8"),
  );
  const familyNames = Object.keys(dictionary);
  const dictionaryValueCount = familyNames.reduce(
    (total, family) => total + Object.keys(dictionary[family]).length,
    0,
  );

  const lines = [];
  const say = (text = "") => {
    lines.push(text);
    console.log(text);
  };

  say("W-0107 / W-0105 / W-0106 — live console capture (option A: no browser dependency)");
  say("");
  say("Captured from the running stack, not a stub:");
  say(`  Ivr.Api    ${apiBase}`);
  say(`  admin-ui   ${uiBase}`);
  say("");
  say("Every request below is a GET. The mock permission headers are refused by the API outside");
  say("IVR_EXECUTION_MODE=MOCK, so this harness cannot authenticate against lab or production.");
  say("");

  // ---- A. Preflight -------------------------------------------------------------------------
  say("=== A. Preflight — what the API says about its own governance state ===");
  const dashboard = await apiGet("/dashboard");
  if (dashboard.status !== 200) {
    say(`  FAILED: /dashboard -> ${dashboard.status}. Is the API up in MOCK mode?`);
    writeOut(lines);
    process.exitCode = 1;
    return;
  }

  say(`  execution_mode              ${dashboard.body.execution_mode}`);
  say(`  sim_provider                ${dashboard.body.sim_provider}`);
  say(`  real_customer_call_allowed  ${dashboard.body.real_customer_call_allowed}`);
  if (dashboard.body.real_customer_call_allowed !== false) {
    say("  REFUSING TO CONTINUE: real customer calls are not disabled.");
    writeOut(lines);
    process.exitCode = 1;
    return;
  }
  say("");

  // ---- B. API sweep -------------------------------------------------------------------------
  say("=== B. Read-endpoint sweep ===");
  const harvested = new Map();
  const responses = new Map();
  for (const path of READ_ENDPOINTS) {
    const result = await apiGet(path);
    responses.set(path, result);
    say(`  GET ${path.padEnd(24)} -> ${result.status}`);
    if (result.status === 200 && result.body !== undefined) {
      harvest(result.body, null, `GET ${path}`, harvested);
    }
  }

  // Detail is per-job and carries fields the list does not: voice_region, sellable decisions,
  // per-attempt disposition. Sampling every job would make the capture length depend on the
  // fixture, so it takes a bounded slice and says how many it took.
  const jobList = responses.get("/call-jobs")?.body;
  const jobItems = Array.isArray(jobList?.items) ? jobList.items : [];
  const sampled = jobItems.slice(0, 12);
  say("");
  say(`  Job detail sampled: ${sampled.length} of ${jobItems.length} returned by /call-jobs`);
  for (const job of sampled) {
    const id = job.ivr_call_job_id;
    if (typeof id !== "string") {
      continue;
    }
    const detail = await apiGet(`/call-jobs/${id}/detail`);
    if (detail.status === 200 && detail.body !== undefined) {
      harvest(detail.body, null, "GET /call-jobs/{id}/detail", harvested);
    }
  }
  say("");

  // ---- C/D. Dictionary cross-check ----------------------------------------------------------
  const rendered = readRenderedFamilies(readScreenSources());
  say("=== C. Live values, checked against the family each screen actually renders them through ===");
  say(`  Dictionary:    ${familyNames.length} families / ${dictionaryValueCount} values`);
  say(`  Render map:    ${rendered.size} fields bound to a family by an EnumLabel call site`);
  say("");

  const gaps = [];
  const adjudicated = [];
  const notRendered = [];
  let checkedValues = 0;

  for (const field of [...harvested.keys()].sort()) {
    const byValue = harvested.get(field);
    const values = [...byValue.keys()].sort();
    const binding = rendered.get(field);

    if (binding === undefined) {
      notRendered.push({ field, values });
      continue;
    }

    const families = [...binding.families].sort();
    say(`  ${field}  ->  ${families.join(" | ")}`);

    for (const value of values) {
      checkedValues += 1;
      const paths = [...byValue.get(value)].sort();
      const labelling = families.filter((family) => dictionary[family]?.[value] !== undefined);

      if (labelling.length === 0) {
        const settled = ADJUDICATED.find(
          (entry) => entry.field === field && entry.value === value,
        );
        if (settled !== undefined) {
          adjudicated.push({ field, value, settled });
          say(`      ${value.padEnd(38)} -- adjudicated: not a gap`);
          continue;
        }

        gaps.push({ field, families, value, paths, sites: binding.sites });
        say(`      ${value.padEnd(38)} !! CANDIDATE GAP`);
        for (const location of paths) {
          say(`      ${" ".repeat(38)}    at ${location}`);
        }
        continue;
      }

      say(`      ${value.padEnd(38)} ${dictionary[labelling[0]][value]}  (${labelling[0]})`);
    }
  }

  say("");
  say("=== D. Result ===");
  say(`  live enum values checked                    ${checkedValues}`);
  say(`  values that reach a screen with no label    ${gaps.length}`);
  for (const gap of gaps) {
    say(`      ${gap.field} = ${gap.value}`);
    say(`          seen at   ${gap.paths.join(", ")}`);
    say(`          bound to  ${gap.families.join(" | ")}; none has an entry`);
    say(`          binding   ${[...new Set(gap.sites)].join(", ")}`);
  }
  say("");
  say("  CANDIDATE, not verdict. The binding above is keyed on the bare field name, because that");
  say("  is what a screen writes: `value={event.source}`. Two different payload fields can share");
  say("  one name -- `data_quality.source` is printed inside a sentence by FreshnessBanner and");
  say("  never passes through EnumLabel at all, yet it collides with the integration screen's");
  say("  `event.source`. Each line above must be adjudicated against its binding file before it is");
  say("  called a defect. The adjudication belongs in the evidence pack, not in this output.");
  say("");
  say(`  candidates already adjudicated              ${adjudicated.length}`);
  for (const item of adjudicated) {
    say(`      ${item.field} = ${item.value}`);
    say(`          ${item.settled.reason}`);
    say(`          settled by ${item.settled.settledBy}`);
  }
  say("");
  say(`  values on fields no screen renders          ${notRendered.length} field(s)`);
  for (const item of notRendered) {
    say(`      ${item.field}: ${item.values.join(", ")}`);
  }
  say("  Those are not gaps. A field the console never displays needs no Vietnamese label; listing");
  say("  it as missing would manufacture work and hide the real findings above.");
  say("");
  say("  A gap is not a crash. `tEnum` (NT-4) renders the raw code and marks it, because an open");
  say("  family is a state the screen has to be able to say out loud. It is listed here so the");
  say("  dictionary owner sees it before an operator does.");
  say("");

  // ---- E. Session guard ---------------------------------------------------------------------
  say("=== E. W-0105 session guard — every console route, no session ===");
  for (const route of CONSOLE_ROUTES) {
    const response = await fetch(`${uiBase}${route}`, { redirect: "manual" });
    const location = response.headers.get("location") ?? "";
    say(`  GET ${route.padEnd(12)} -> ${response.status}${location === "" ? "" : `  ${location}`}`);
  }
  say("");

  // ---- F. Login screen ----------------------------------------------------------------------
  say("=== F. Login screen — visible text ===");
  say("  <script> blocks and the RSC flight payload are stripped, so this records what an");
  say("  operator sees rather than the data behind it.");
  const login = await fetch(`${uiBase}/login`);
  for (const line of visibleText(await login.text())) {
    say(`  ${line}`);
  }

  writeOut(lines);
  console.log(`\nWritten: ${outputPath}`);

  // Non-zero exit, so a capture wired into a pipeline fails rather than filing a green report with
  // the gaps buried in its body.
  if (gaps.length > 0) {
    console.log(`${gaps.length} value(s) reach a screen with no Vietnamese label.`);
    process.exitCode = 1;
  }
}

/** Strips markup to the text an operator would read. */
function visibleText(html) {
  return html
    .replace(/<script[\s\S]*?<\/script>/giu, " ")
    .replace(/<style[\s\S]*?<\/style>/giu, " ")
    .replace(/<[^>]+>/gu, "\n")
    .replace(/&#x27;/gu, "'")
    .replace(/&quot;/gu, '"')
    .replace(/&amp;/gu, "&")
    .replace(/&lt;/gu, "<")
    .replace(/&gt;/gu, ">")
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line !== "");
}

function writeOut(lines) {
  mkdirSync(dirname(outputPath), { recursive: true });
  writeFileSync(outputPath, `${lines.join("\n")}\n`, "utf8");
}

await main();
