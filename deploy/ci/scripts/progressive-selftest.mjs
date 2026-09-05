#!/usr/bin/env node
// W-0046 / P7-4 §8. Progressive delivery checked as configuration, plus one check that is not.
//
// Three of these four read YAML: Argo Rollouts is not installed and no Prometheus receives IVR
// metrics (W-0063, BLOCKED_EXTERNAL), so no canary has ever run. P7-4 §10 asks for a canary run and
// an auto-rollback demo; neither exists, and this script does not pretend otherwise.
//
// IT-MIGRATE-03 is different. It reads the actual migrations and fails on anything that would
// break a two-version overlap — the gate P7-3 §5 identified as missing when it noted that
// `helm rollback` returns the manifest but never the schema.
//
// Run: node deploy/ci/scripts/progressive-selftest.mjs
import fs from "node:fs/promises";
import { createHash } from "node:crypto";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";
import { inspectExpandSource, verifyExpandGuard } from "./migration-expand-guard.mjs";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const ROLLOUTS = path.join(repositoryRoot, "deploy/rollouts");
const MIGRATIONS = path.join(repositoryRoot, "src/Ivr.Infrastructure/Persistence/Migrations");

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

const load = async (file) => YAML.parse(await fs.readFile(path.join(ROLLOUTS, file), "utf8"), { merge: true });

// ---------------------------------------------------------------- IT-CANARY-01
async function canaryIsGatedOnSlo() {
  const rollout = await load("api-canary.yaml");
  const canary = rollout.spec?.strategy?.canary;
  assert(canary, "api-canary.yaml declares no canary strategy.");

  const templates = (canary.analysis?.templates ?? []).map((entry) => entry.templateName);
  assert(templates.includes("ivr-api-slo"),
    "the api canary runs no SLO analysis, so every step would promote on time alone.");

  // Analysis must start before the last step. Analysis that begins at 100% has already shifted all
  // the traffic it was supposed to protect.
  const weights = (canary.steps ?? []).filter((step) => step.setWeight !== undefined).map((step) => step.setWeight);
  assert(weights.length >= 3, `the canary has ${weights.length} weight steps; it goes to full traffic too fast.`);
  assert(weights[0] <= 10, `the first canary step is ${weights[0]}%; P7-4 §2 asks for 5-10%.`);
  assert(canary.analysis.startingStep < (canary.steps ?? []).length - 1,
    "analysis starts on the last step, by which point all traffic has already moved.");

  // Every pause bounded. An indefinite pause waits for a human, and a canary nobody is watching
  // sits at 10% until somebody notices.
  for (const step of canary.steps ?? []) {
    if (step.pause !== undefined) {
      assert(step.pause?.duration, "a canary pause has no duration, so it waits for a human forever.");
    }
  }

  const analysis = await load("analysis-slo.yaml");
  const metrics = analysis.spec?.metrics ?? [];
  assert(metrics.length >= 3, `the SLO template has ${metrics.length} metrics; latency, fail-closed and delivery are all needed.`);
  for (const metric of metrics) {
    assert(metric.successCondition, `${metric.name} declares no successCondition, so it can never fail.`);
    assert(metric.failureLimit !== undefined,
      `${metric.name} has no failureLimit; without one a bad reading does not abort the rollout.`);
    assert(metric.provider?.prometheus?.query, `${metric.name} has no query.`);
  }

  // The gate must not be laxer than the paging threshold, or a canary can promote a version that
  // immediately pages whoever is on call.
  const latency = metrics.find((metric) => /p95/.test(metric.name));
  assert(latency && /<=\s*5\b/.test(latency.successCondition),
    "the canary latency gate is not the D-04 objective of 5s used by the P6-2 alert.");

  process.stdout.write("IT-CANARY-01 PASS — canary is SLO-gated, starts small, and every pause is bounded\n");
}

// ---------------------------------------------------------------- IT-BG-WORKER-02
async function workerIsBlueGreen() {
  const rollout = await load("worker-bluegreen.yaml");
  const strategy = rollout.spec?.strategy;
  assert(strategy?.blueGreen, "the worker uses a strategy other than blue-green.");
  assert(!strategy.canary,
    "the worker declares a canary; two scheduler versions would interleave attempts for the length of the analysis.");
  assert(strategy.blueGreen.activeService && strategy.blueGreen.previewService,
    "blue-green needs both an active and a preview service for the switch to be atomic.");

  // No auto-promotion: the worker has no HTTP surface to smoke (W-0043 §2), so promoting on "pods
  // are up" would assert health nobody measured.
  assert(strategy.blueGreen.autoPromotionEnabled === false,
    "the worker auto-promotes, but it exposes nothing that could be smoked first.");

  // The reason has to travel with the object. A future reader who does not know about the advisory
  // lock will otherwise "improve" this into a canary.
  const annotations = rollout.metadata?.annotations ?? {};
  assert(Object.values(annotations).some((value) => /advisory lock|double-dispatch/i.test(String(value))),
    "the worker rollout does not record why it is not a canary.");

  process.stdout.write("IT-BG-WORKER-02 PASS — worker switches blue-green, manually promoted, with the reason recorded\n");
}

// ---------------------------------------------------------------- IT-MIGRATE-03
async function migrationsSurviveTwoVersions() {
  // The real check, and the one P7-3 §5 said was missing. During a canary or a rollback, the old
  // code runs against the new schema. Anything the old code cannot tolerate belongs in a later
  // release (expand-contract), not this one.
  //
  // W-0114 added UT-SCHEMA-BACKCOMPAT-01, which checks the same property against EF's typed
  // operation model and covers three shapes this cannot see (unique constraints, unique indexes,
  // and CHECK constraints over pre-existing columns). This one stays because it is the cheap half:
  // it runs in a node image with no .NET toolchain, so a destructive Up() fails in `validate`
  // rather than after a Release build. Reading text is also why every AlterColumn is flagged here
  // and only narrowing ones are flagged there -- text sees the call, not its arguments.
  const files = (await fs.readdir(MIGRATIONS))
    .filter((name) => name.endsWith(".cs") && !name.includes("Designer") && !name.includes("Snapshot"));
  assert(files.length > 0, "no migrations found; the check would be vacuous.");

  verifyExpandGuard();
  const violations = [];
  const baseline = JSON.parse(await fs.readFile(path.join(repositoryRoot, "deploy/ci/migration-expand-baseline.json"), "utf8"));
  assert(Object.keys(baseline.legacySqlSourceSha256).length === 2, "historical baseline must remain a bounded two-file inventory");
  for (const [id, expected] of Object.entries(baseline.legacySqlSourceSha256)) {
    assert(id < baseline.supportedLegacySchema, `${id}: cannot exempt current expand DDL`);
    const source = (await fs.readFile(path.join(MIGRATIONS, `${id}.cs`), "utf8")).replaceAll("\r\n", "\n");
    assert(createHash("sha256").update(source).digest("hex").toUpperCase() === expected,
      `${id}: historical SQL changed; its baseline pin is no longer valid`);
  }

  for (const file of files) {
    const text = await fs.readFile(path.join(MIGRATIONS, file), "utf8");
    const upStart = text.indexOf("protected override void Up(");
    const downStart = text.indexOf("protected override void Down(");
    assert(upStart >= 0, `${file} has no Up method.`);
    // Only Up matters. Down drops what Up created — that is what a down migration IS, and a check
    // that flagged it would fire on every migration ever written and be switched off within a week.
    const up = downStart > upStart ? text.slice(upStart, downStart) : text.slice(upStart);

    const pinnedLegacySql = baseline.legacySqlSourceSha256[file.replace(/\.cs$/, "")];
    violations.push(...inspectExpandSource(up)
      .filter((reason) => !(pinnedLegacySql && reason === "destructive or dynamic raw SQL in expand phase"))
      .map((reason) => `${file}: ${reason}`));
  }

  assert(violations.length === 0, `migrations are not expand-contract safe:\n  ${violations.join("\n  ")}`);
  process.stdout.write(
    `IT-MIGRATE-03 PASS — ${files.length} migrations, expand DDL guarded; 2 byte-pinned legacy SQL migrations outside the supported overlap window\n`);
}

// ---------------------------------------------------------------- IT-FLAG-RAMP-04
async function releaseIsSeparateFromDeploy() {
  // Deploy ≠ release. If a rollout could turn a feature on, the two would be the same event again
  // and the feature flag would only be documentation.
  const offenders = [];
  for (const file of await fs.readdir(ROLLOUTS)) {
    const text = await fs.readFile(path.join(ROLLOUTS, file), "utf8");
    text.split("\n").forEach((line, index) => {
      if (line.trim().startsWith("#")) return;
      if (/FeatureFlag|IVR_FEATURE_|featureFlags/i.test(line)) {
        offenders.push(`${file}:${index + 1}: ${line.trim()}`);
      }
      const real = /REAL_CUSTOMER_CALL_ALLOWED[\s\S]{0,40}?["']?(YES|true|1|on)["']?\s*$/i.exec(line);
      if (real) {
        offenders.push(`${file}:${index + 1}: opens real calling from a rollout`);
      }
    });
  }
  assert(offenders.length === 0,
    `rollout configuration couples release to deploy:\n  ${offenders.join("\n  ")}`);

  // And the floor travels with the canary: a canary carrying a different governance posture than
  // stable would make the ladder depend on which pod answered the request.
  for (const file of ["api-canary.yaml", "worker-bluegreen.yaml"]) {
    const rollout = await load(file);
    const env = rollout.spec?.template?.spec?.containers?.[0]?.env ?? [];
    const flag = env.find((entry) => entry.name === "REAL_CUSTOMER_CALL_ALLOWED");
    assert(flag?.value === "NO", `${file} does not pin REAL_CUSTOMER_CALL_ALLOWED=NO on the new version.`);
    const mode = env.find((entry) => entry.name === "IVR_EXECUTION_MODE");
    assert(mode?.value === "MOCK", `${file} does not pin IVR_EXECUTION_MODE=MOCK on the new version.`);
  }

  process.stdout.write("IT-FLAG-RAMP-04 PASS — no rollout touches a feature flag, and both pin the governance floor\n");
}

await canaryIsGatedOnSlo();
await workerIsBlueGreen();
await migrationsSurviveTwoVersions();
await releaseIsSeparateFromDeploy();

process.stdout.write(
  "PROGRESSIVE_SELFTEST_PASS (IT-MIGRATE-03 reads the real migrations; the other three are "
  + "configuration only — Argo Rollouts is not installed and no Prometheus receives IVR metrics. "
  + "See docs/evidence/W-0046.)\n");
