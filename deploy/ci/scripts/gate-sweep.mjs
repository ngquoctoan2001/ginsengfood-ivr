#!/usr/bin/env node

// W-0251. Runs every offline gate that has no CI job of its own, and refuses to let a new one be
// added without saying how it is run.
//
// Why this exists. `external-decision-response-validator` and `external-decision-closure-validator`
// were red from `W-0217` until `W-0250` found them - roughly thirty work items. Not because anyone
// ignored them: nothing ran them. Seventeen scripts under this directory are reachable from no CI
// job, no npm script and no other script, so they only ever execute when somebody sweeps the
// directory by hand. And a hand sweep reads them wrong, because most of these scripts print a usage
// line and exit non-zero when invoked with no arguments - `W-0221` recorded that lesson after a
// sweep took four usage lines for four passes.
//
// So the sweep needs two things a `for f in *.mjs` loop cannot have: the argv each gate actually
// wants, and the token it prints when it truly passed. Exit code alone is not enough - a script
// that prints usage and exits 0 would read as a pass.
//
// The third property is the one that keeps this honest over time: every file in this directory must
// appear in the manifest, as either a runnable entry or an explicit `sweepable: false` with a
// reason. A gate added without an entry fails this sweep. That is the check that would have caught
// the seventeen orphans on the day the first one landed.

import { spawnSync } from "node:child_process";
import { readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPTS_DIR = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPTS_DIR, "..", "..", "..");
const MANIFEST_PATH = path.join(SCRIPTS_DIR, "..", "gate-invocations.json");
const RUNNABLE_EXTENSIONS = new Set([".mjs", ".sh", ".py", ".ps1"]);
const DEFAULT_TIMEOUT_MS = 180_000;
const LOCK_PATH = path.join(SCRIPTS_DIR, "..", ".gate-sweep.lock");

/** This file is the sweep, so it cannot be one of the gates it sweeps. */
const SELF = path.basename(fileURLToPath(import.meta.url));

function loadManifest() {
  const manifest = JSON.parse(readFileSync(MANIFEST_PATH, "utf8"));
  if (!manifest.gates || typeof manifest.gates !== "object") {
    throw new Error("gate-invocations.json must carry a `gates` object");
  }

  return manifest.gates;
}

function listGateFiles() {
  return readdirSync(SCRIPTS_DIR)
    .filter((name) => RUNNABLE_EXTENSIONS.has(path.extname(name)))
    .filter((name) => name !== SELF)
    .sort();
}

/** Neither side may drift: an unlisted gate is invisible, a listed ghost is a stale entry. */
function checkCoverage(gates, files) {
  const problems = [];
  const listed = new Set(Object.keys(gates));
  for (const name of files) {
    if (!listed.has(name)) {
      problems.push(`${name}: present in scripts/ but absent from gate-invocations.json`);
    }
  }

  const present = new Set(files);
  for (const name of listed) {
    if (!present.has(name)) {
      problems.push(`${name}: listed in gate-invocations.json but no such file`);
    }
  }

  return problems;
}

// Two sweeps at once produce a red that means nothing. `dr-selftest` builds docker containers under
// fixed names - ivr-dr-primary, ivr-dr-standby, the ivr-dr-selftest network - so a second run
// collides with the first and dies in seconds, and the sweep reports a broken gate when the truth is
// that it was run twice. That is the same class of wrong answer this whole file exists to remove, so
// the second run is refused by name instead. A lock left behind by a crash is not a trap: the owner
// pid is recorded, and a lock whose owner is gone is taken over rather than obeyed.
function acquireLock() {
  try {
    writeFileSync(LOCK_PATH, JSON.stringify({ pid: process.pid, startedAt: new Date().toISOString() }), {
      encoding: "utf8",
      flag: "wx",
    });
    return true;
  } catch (error) {
    if (error.code !== "EEXIST") throw error;
  }

  let owner;
  try {
    owner = JSON.parse(readFileSync(LOCK_PATH, "utf8"));
  } catch {
    owner = null;
  }

  if (owner?.pid && isAlive(owner.pid)) {
    process.stdout.write(
      `GATE_SWEEP_BUSY — pid ${owner.pid} has been sweeping since ${owner.startedAt}; ` +
        "run one sweep at a time, or use --only <gate>\n",
    );
    return false;
  }

  process.stdout.write(
    `  ---- took over a lock left by pid ${owner?.pid ?? "unknown"}, which is no longer running\n`,
  );
  writeFileSync(LOCK_PATH, JSON.stringify({ pid: process.pid, startedAt: new Date().toISOString() }), "utf8");
  return true;
}

function isAlive(pid) {
  try {
    process.kill(pid, 0);
    return true;
  } catch (error) {
    return error.code === "EPERM";
  }
}

function runGate(name, entry) {
  const target = path.join(SCRIPTS_DIR, name);
  const extension = path.extname(name);
  const interpreter = extension === ".mjs" ? "node" : extension === ".sh" ? "sh" : "python";
  const started = Date.now();
  const result = spawnSync(interpreter, [target, ...entry.argv], {
    cwd: REPOSITORY_ROOT,
    encoding: "utf8",
    timeout: entry.timeoutMs ?? DEFAULT_TIMEOUT_MS,
    windowsHide: true,
  });
  const elapsed = Date.now() - started;
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`;

  if (result.error) {
    return { ok: false, elapsed, detail: `could not start: ${result.error.message}` };
  }

  if (result.status !== 0) {
    // The last line of a crashed node process is its version banner, which says nothing. Prefer
    // the line that names the problem; fall back to the tail only when nothing looks like one.
    const printed = output.trim().split(/\r?\n/u).filter((line) => line.trim());
    const diagnostic =
      printed.findLast((line) => /error|fail|refus|denied|conflict|not found/iu.test(line)) ??
      printed.at(-1) ??
      "";
    return {
      ok: false,
      elapsed,
      detail: `exit ${result.status}: ${diagnostic.trim().slice(0, 140)}`,
    };
  }

  // A zero exit is not the verdict. Several of these scripts print usage and stop; some print a
  // report whose own verdict field says FAIL. The token is what the gate says when it passed.
  if (!output.includes(entry.expect)) {
    return { ok: false, elapsed, detail: `exit 0 but never printed ${entry.expect}` };
  }

  return { ok: true, elapsed, detail: entry.expect };
}

export function runGateSweep({ listOnly = false, only = null } = {}) {
  const gates = loadManifest();
  const files = listGateFiles();
  const problems = checkCoverage(gates, files);

  // `--only` narrows what is executed, never what is checked for coverage: a sweep that stopped
  // noticing an unlisted gate while debugging one gate would be the wrong tool to reach for.
  const runnable = files
    .filter((name) => gates[name]?.argv)
    .filter((name) => !only || name === only || name === `${only}.mjs`);
  const skipped = files.filter((name) => gates[name] && !gates[name].argv);

  if (listOnly) {
    for (const name of files) {
      const entry = gates[name];
      const how = entry?.argv
        ? `run: ${entry.argv.join(" ") || "<no arguments>"} -> ${entry.expect}`
        : `skip: ${entry?.reason ?? "UNLISTED"}`;
      process.stdout.write(`  ${name.padEnd(52)} ${how}\n`);
    }
    process.stdout.write(`GATE_SWEEP_LIST ${runnable.length} runnable, ${skipped.length} skipped\n`);
    return problems.length === 0 ? 0 : 1;
  }

  // Only the executing path needs the lock; --list touches nothing.
  if (!acquireLock()) return 1;

  let passed = 0;
  try {
  for (const name of runnable) {
    const outcome = runGate(name, gates[name]);
    const seconds = (outcome.elapsed / 1000).toFixed(1);
    if (outcome.ok) {
      passed += 1;
      process.stdout.write(`  ok   ${name.padEnd(52)} ${seconds}s  ${outcome.detail}\n`);
    } else {
      problems.push(`${name}: ${outcome.detail}`);
      process.stdout.write(`  FAIL ${name.padEnd(52)} ${seconds}s  ${outcome.detail}\n`);
    }
  }

  } finally {
    rmSync(LOCK_PATH, { force: true });
  }

  for (const problem of problems.filter((item) => !item.includes(": exit "))) {
    process.stdout.write(`  ---- ${problem}\n`);
  }

  process.stdout.write(
    `GATE_SWEEP_${problems.length === 0 ? "PASS" : "FAIL"} ` +
      `${passed}/${runnable.length} run, ${skipped.length} skipped by manifest\n`,
  );
  return problems.length === 0 ? 0 : 1;
}

const invokedDirectly =
  process.argv[1] && path.resolve(process.argv[1]) === path.resolve(fileURLToPath(import.meta.url));
if (invokedDirectly) {
  const onlyIndex = process.argv.indexOf("--only");
  process.exit(
    runGateSweep({
      listOnly: process.argv.includes("--list"),
      only: onlyIndex === -1 ? null : process.argv[onlyIndex + 1],
    }),
  );
}
