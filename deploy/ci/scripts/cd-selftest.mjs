#!/usr/bin/env node
// W-0045 / P7-3 §8. The delivery pipeline checked as configuration.
//
// These five checks read the CI YAML, and that is the honest ceiling: there is no runner, no
// registry and no cluster credential (W-0061 / W-0063, both BLOCKED_EXTERNAL), so no pipeline in
// this repository has ever executed. P7-3 §10 says YAML is not deploy proof, and this script does
// not claim it is -- what it proves is that the SHAPE of the pipeline cannot express the things
// the governance forbids.
//
// That distinction matters most for IT-CD-REAL-03. Nobody can promise a future pipeline run will
// not open real calling; what can be proven today is that no job in the repository sets the flag
// at all, so opening it would have to be a visible edit rather than a variable someone flips.
//
// Run: node deploy/ci/scripts/cd-selftest.mjs
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const CI_DIR = path.join(repositoryRoot, "deploy/ci");

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

// merge: true so `<<: *anchor` is resolved the way GitLab resolves it. Without it a job inheriting
// allow_failure from a base looks like it declares nothing, and the checker reports a violation
// that does not exist -- or worse, misses one that does because it only read the base.
const PARSE = { merge: true };

async function readFragment(name) {
  return YAML.parse(await fs.readFile(path.join(CI_DIR, name), "utf8"), PARSE);
}

async function allCiText() {
  const files = [path.join(repositoryRoot, ".gitlab-ci.yml")];
  for (const entry of await fs.readdir(CI_DIR)) {
    if (entry.endsWith(".yml") || entry.endsWith(".yaml")) {
      files.push(path.join(CI_DIR, entry));
    }
  }
  const texts = [];
  for (const file of files) {
    texts.push({ file: path.relative(repositoryRoot, file), text: await fs.readFile(file, "utf8") });
  }
  return texts;
}

const isJob = (value) => value && typeof value === "object" && !Array.isArray(value)
  && ("script" in value || "trigger" in value);

async function jobsIn(name) {
  const fragment = await readFragment(name);
  return Object.entries(fragment).filter(([key, value]) => !key.startsWith(".") && isJob(value));
}

// ---------------------------------------------------------------- IT-CD-DEV-01
async function devPipelineIsGatedOnTheScan() {
  const cd = await readFragment("cd.gitlab-ci.yml");
  const deploy = cd.deploy_dev;
  assert(deploy, "cd.gitlab-ci.yml has no deploy_dev job.");
  assert(deploy.allow_failure === false, "deploy_dev may fail; a deploy gate that may fail is not a gate.");

  // needs, not stage order. Stage order only says "later"; needs says "unreachable if that job is
  // red", which is what stops a deploy racing a failed scan.
  const needs = (deploy.needs ?? []).map((entry) => (typeof entry === "string" ? entry : entry.job));
  assert(needs.includes("publish_images"),
    "deploy_dev does not need publish_images, so a failed image scan would not block it.");

  const publish = cd.publish_images;
  assert(publish, "cd.gitlab-ci.yml has no publish_images job.");
  const script = JSON.stringify(publish.script);
  assert(/trivy/.test(script), "publish_images does not scan the images it publishes.");
  assert(/--exit-code 1/.test(script), "the image scan does not fail the job on a finding.");
  // Scanning after the push would mean the bad image already exists and can be pulled.
  assert(script.indexOf("trivy") < script.indexOf("docker push"),
    "publish_images scans after pushing; the vulnerable image would already be in the registry.");
  assert(/RepoDigests/.test(script),
    "publish_images does not record an image digest, so evidence could only cite a movable tag.");

  const smoke = JSON.stringify(deploy.script);
  assert(/health\/ready/.test(smoke), "deploy_dev runs no post-deploy smoke against readiness.");

  process.stdout.write("IT-CD-DEV-01 PASS — dev deploy needs a scanned, digest-recorded publish, and smokes after\n");
}

// ---------------------------------------------------------------- IT-CD-GATE-02
async function promotionsRequireAHuman() {
  const promote = await readFragment("promote.gitlab-ci.yml");
  for (const name of ["promote_lab", "promote_prod"]) {
    const job = promote[name];
    assert(job, `promote.gitlab-ci.yml has no ${name} job.`);
    assert(job.allow_failure === false, `${name} may fail; P7-3 §11 forbids that on a promotion gate.`);

    // Manual either on the job or on every rule that can reach it. A single automatic rule is
    // enough to promote without a human, so all of them are checked rather than the first.
    const rules = job.rules ?? [];
    const manual = job.when === "manual"
      || (rules.length > 0 && rules.every((rule) => rule.when === "manual"));
    assert(manual, `${name} can start without a human pressing it.`);

    assert(job.environment?.name, `${name} declares no environment, so GitLab cannot protect it.`);
  }

  assert(promote.promote_prod.environment.name === "production",
    "promote_prod does not target the production environment, so protection rules would not apply.");
  process.stdout.write("IT-CD-GATE-02 PASS — lab and prod promotions are manual, fail-closed and environment-scoped\n");
}

// ---------------------------------------------------------------- IT-CD-REAL-03
async function nothingOpensRealCalling() {
  // The ladder (README-governance §6) makes this flag `false (immutable)` for dev, staging and lab,
  // and false for prod until a DF-03 sign-off. So the correct number of pipeline jobs that set it
  // is zero -- not "one, carefully".
  const offenders = [];
  for (const { file, text } of await allCiText()) {
    text.split("\n").forEach((line, index) => {
      if (line.trim().startsWith("#")) return;
      const match = /REAL_CUSTOMER_CALL_ALLOWED\s*[:=]\s*["']?([A-Za-z]+)["']?/.exec(line);
      if (match && /^(yes|true|1|on)$/i.test(match[1])) {
        offenders.push(`${file}:${index + 1}: ${line.trim()}`);
      }
    });
  }
  assert(offenders.length === 0,
    `CI configuration opens real calling:\n  ${offenders.join("\n  ")}`);

  // And the promotion jobs refuse a non-MOCK execution mode rather than passing it through.
  const promote = await readFragment("promote.gitlab-ci.yml");
  for (const name of ["promote_lab", "promote_prod"]) {
    const script = JSON.stringify(promote[name].script);
    assert(/IVR_EXECUTION_MODE/.test(script) && /exit 1/.test(script),
      `${name} does not refuse a non-MOCK execution mode.`);
  }
  process.stdout.write("IT-CD-REAL-03 PASS — no job sets REAL_CUSTOMER_CALL_ALLOWED, and promotions refuse non-MOCK\n");
}

// ---------------------------------------------------------------- IT-CD-ROLLBACK-04
async function failedDeploysRollBack() {
  for (const fragment of ["cd.gitlab-ci.yml", "promote.gitlab-ci.yml"]) {
    const text = await fs.readFile(path.join(CI_DIR, fragment), "utf8");
    const parsed = YAML.parse(text, PARSE);
    for (const [name, job] of Object.entries(parsed)) {
      if (name.startsWith(".") || !isJob(job) || !job.environment) continue;
      // The job's OWN resolved wiring, anchors included. An earlier draft fell back to "the string
      // appears somewhere in the file", which would have passed on a comment -- the same
      // match-the-prose mistake that made a K8s check fail on its own documentation.
      const wiring = JSON.stringify(job.after_script ?? "") + JSON.stringify(job.script ?? "");
      assert(/helm rollback/.test(wiring),
        `${name} deploys to ${job.environment.name} with no rollback path.`);
      if (name !== "rollback_prod") {
        assert(/--atomic/.test(JSON.stringify(job.script)),
          `${name} does not use --atomic, so a partly-applied release could survive a failure.`);
      }
    }
  }
  process.stdout.write("IT-CD-ROLLBACK-04 PASS — every environment job is atomic and has a rollback path\n");
}

// ---------------------------------------------------------------- IT-CD-CONCURRENCY-05
async function deploymentsAreSerialised() {
  for (const fragment of ["cd.gitlab-ci.yml", "promote.gitlab-ci.yml"]) {
    for (const [name, job] of await jobsIn(fragment)) {
      if (!job.environment) continue;
      assert(job.resource_group,
        `${name} touches ${job.environment.name} without a resource_group; two pipelines could deploy at once.`);
      // The rollback shares the group of the deploy it undoes, or it can race an in-flight upgrade.
      assert(job.resource_group === job.environment.name
        || (job.environment.name === "production" && job.resource_group === "production"),
        `${name} uses resource_group ${job.resource_group} for environment ${job.environment.name}; they must match.`);
    }
  }
  process.stdout.write("IT-CD-CONCURRENCY-05 PASS — every environment job is serialised by resource_group\n");
}

await devPipelineIsGatedOnTheScan();
await promotionsRequireAHuman();
await nothingOpensRealCalling();
await failedDeploysRollBack();
await deploymentsAreSerialised();

process.stdout.write(
  "CD_SELFTEST_PASS (configuration only — no pipeline in this repository has ever run: "
  + "no runner, registry or cluster credential exists. See docs/evidence/W-0045.)\n");
