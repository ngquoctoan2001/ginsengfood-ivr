#!/usr/bin/env node
// W-0044 / P7-2 §8. The Helm chart checked against a real Kubernetes API, not against itself.
//
// Rendering a chart proves the YAML parses. It does not prove the pods start, that readiness takes
// a pod out of rotation, or that a NetworkPolicy blocks anything -- and each of those failed for a
// different reason the first time this ran. So the script builds a throwaway cluster and deploys
// into it.
//
// Run: node deploy/ci/scripts/k8s-selftest.mjs [--keep]
import { execFileSync, execSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const keep = process.argv.includes("--keep");

const K3S = "rancher/k3s:v1.31.4-k3s1";
const HELM = "alpine/helm:3.16.3";
const KUBECONFORM = "ghcr.io/yannh/kubeconform:v0.6.7";
const BUSYBOX = "busybox:1.37.0-uclibc";
const CLUSTER = "ivr-k8s-selftest";
const HELMBOX = "ivr-helm-selftest";
const NS = "ivr-dev";
const ENVIRONMENTS = ["dev", "staging", "lab", "prod"];

function docker(args, options = {}) {
  return execFileSync("docker", args, {
    cwd: repositoryRoot,
    encoding: "utf8",
    stdio: options.inherit ? "inherit" : ["ignore", "pipe", "pipe"],
    maxBuffer: 64 * 1024 * 1024,
    ...options,
  });
}

const kubectl = (args, options = {}) => docker(["exec", CLUSTER, "kubectl", ...args], options);

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

function sleepSeconds(seconds) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, seconds * 1000);
}

function waitFor(description, seconds, predicate) {
  for (let attempt = 0; attempt * 3 < seconds; attempt += 1) {
    if (predicate()) return true;
    sleepSeconds(3);
  }
  throw new Error(`Timed out waiting for ${description}.`);
}

// ---------------------------------------------------------------- render
function render(environment) {
  return docker(["exec", HELMBOX, "helm", "template", "ivr", "/ivr", "-f", `/ivr/values-${environment}.yaml`]);
}

function renderFails(environment, overrides) {
  try {
    docker(["exec", HELMBOX, "helm", "template", "ivr", "/ivr", "-f", `/ivr/values-${environment}.yaml`,
      ...overrides.flatMap((override) => ["--set", override])]);
    return null;
  } catch (error) {
    return String(error.stderr ?? error.message);
  }
}

// ---------------------------------------------------------------- IT-K8S-LINT-01
function lintAndValidate() {
  for (const environment of ENVIRONMENTS) {
    const lint = docker(["exec", HELMBOX, "helm", "lint", "/ivr", "-f", `/ivr/values-${environment}.yaml`]);
    assert(lint.includes("0 chart(s) failed"), `helm lint failed for ${environment}: ${lint}`);

    const manifest = render(environment);
    const summary = execSync(
      `docker run -i --rm ${KUBECONFORM} -strict -summary -kubernetes-version 1.31.0 -`,
      { input: manifest, encoding: "utf8", cwd: repositoryRoot },
    );
    assert(/Invalid: 0, Errors: 0/.test(summary), `kubeconform rejected ${environment}: ${summary}`);
  }
  process.stdout.write("IT-K8S-LINT-01 PASS — helm lint and kubeconform clean for all four environments\n");
}

// ---------------------------------------------------------------- IT-K8S-GATE-02 (render half)
function gateRefusesToOpenTheLadder() {
  // The guard is asserted by trying to break it. A ladder rule nobody tested against a violation is
  // a comment.
  const cases = [
    { environment: "dev", overrides: ["governance.realCustomerCallAllowed=true"], expect: "Only lab and prod" },
    { environment: "lab", overrides: ["governance.executionMode=LAB_REAL_SIM"], expect: "labDestinationAllowlist" },
    { environment: "prod", overrides: ["governance.executionMode=PRODUCTION_REAL", "governance.killSwitchEnabled=false"], expect: "kill switch" },
  ];
  for (const testCase of cases) {
    const failure = renderFails(testCase.environment, testCase.overrides);
    assert(failure, `${testCase.environment} rendered with ${testCase.overrides.join(", ")}; the ladder guard did not fire.`);
    assert(failure.includes(testCase.expect), `${testCase.environment} failed for the wrong reason: ${failure}`);
  }

  // And every environment as shipped sits on the closed end.
  for (const environment of ENVIRONMENTS) {
    const manifest = render(environment);
    assert(/REAL_CUSTOMER_CALL_ALLOWED[\s\S]{0,80}?value: "NO"/.test(manifest),
      `${environment} does not render REAL_CUSTOMER_CALL_ALLOWED=NO.`);
    assert(/IVR_EXECUTION_MODE[\s\S]{0,80}?value: "MOCK"/.test(manifest),
      `${environment} does not render IVR_EXECUTION_MODE=MOCK.`);
  }
}

// ---------------------------------------------------------------- cluster
function startCluster() {
  docker(["rm", "-f", CLUSTER], { stdio: ["ignore", "ignore", "ignore"] });
  docker(["run", "-d", "--name", CLUSTER, "--privileged", K3S,
    "server", "--disable=traefik", "--disable=metrics-server", "--tls-san=127.0.0.1"]);
  waitFor("the cluster node to become Ready", 180, () => {
    try {
      return kubectl(["get", "nodes", "--no-headers"]).includes(" Ready ");
    } catch {
      return false;
    }
  });
}

function loadImages() {
  for (const image of ["ivr-api", "ivr-worker", "ivr-admin-ui", "ivr-migrate"]) {
    docker(["tag", `${image}:${process.env.IVR_IMAGE_TAG ?? "p7-1"}`, `${image}:0.1.0-dev`]);
    execSync(`docker save ${image}:0.1.0-dev | docker exec -i ${CLUSTER} ctr -n k8s.io images import -`,
      { cwd: repositoryRoot, stdio: ["ignore", "ignore", "inherit"] });
  }
}

function deploy() {
  try {
    kubectl(["create", "namespace", NS], { stdio: ["ignore", "ignore", "ignore"] });
  } catch { /* already exists on a re-run */ }
  const bootstrap = path.join(repositoryRoot, "deploy/helm/ivr/ci/bootstrap-dev.yaml");
  docker(["cp", bootstrap, `${CLUSTER}:/tmp/bootstrap.yaml`]);
  kubectl(["-n", NS, "apply", "-f", "/tmp/bootstrap.yaml"]);
  waitFor("postgres", 180, () =>
    kubectl(["-n", NS, "get", "pods", "-l", "app.kubernetes.io/name=postgres", "--no-headers"]).includes("1/1"));

  const manifest = path.join(os.tmpdir(), "ivr-k8s-selftest.yaml");
  fs.writeFileSync(manifest, render("dev"), "utf8");
  docker(["cp", manifest, `${CLUSTER}:/tmp/ivr-dev.yaml`]);
  kubectl(["-n", NS, "apply", "-f", "/tmp/ivr-dev.yaml"]);

  waitFor("the migration hook to complete", 240, () =>
    kubectl(["-n", NS, "get", "job", "ivr-ivr-migrate", "--no-headers"]).includes("1/1"));
  waitFor("api, worker and ui to become ready", 300, () => {
    const pods = kubectl(["-n", NS, "get", "pods", "--no-headers"]);
    return pods.split("\n").filter((line) => /1\/1\s+Running/.test(line)).length >= 4;
  });
}

// ---------------------------------------------------------------- IT-K8S-GATE-02 (running half)
function gateFromRunningPods() {
  // §8 is explicit: read the EFFECTIVE config of a running pod, not the values file. A values file
  // can be correct while the pod runs something else -- which is exactly what happened here once,
  // when $(IVR_DB_PASSWORD) reached the pod as literal text.
  for (const component of ["api", "worker"]) {
    const env = kubectl(["-n", NS, "get", "pod", "-l", `app.kubernetes.io/component=${component}`,
      "-o", 'jsonpath={range .items[0].spec.containers[0].env[*]}{.name}={.value}{"\\n"}{end}']);
    assert(/REAL_CUSTOMER_CALL_ALLOWED=NO/.test(env), `${component} pod does not carry REAL_CUSTOMER_CALL_ALLOWED=NO.`);
    assert(/IVR_EXECUTION_MODE=MOCK/.test(env), `${component} pod does not carry IVR_EXECUTION_MODE=MOCK.`);
    assert(/IVR_KILL_SWITCH_ENABLED=true/.test(env), `${component} pod has the kill switch disabled.`);
    assert(!/\$\(/.test(env.split("\n").filter((line) => !line.startsWith("ConnectionStrings")).join("\n")),
      `${component} pod carries an unexpanded $(...) reference.`);
  }
  process.stdout.write("IT-K8S-GATE-02 PASS — running api and worker pods carry MOCK, no real calling, kill switch on\n");
}

// ---------------------------------------------------------------- IT-K8S-PROBE-03
function readinessRemovesFromRotation() {
  const endpointsBefore = kubectl(["-n", NS, "get", "endpoints", "ivr-ivr-api", "-o", "jsonpath={.subsets[*].addresses[*].ip}"]);
  assert(endpointsBefore.trim() !== "", "the api Service had no endpoints before the fault.");

  kubectl(["-n", NS, "scale", "deploy", "ivr-postgres", "--replicas=0"]);
  waitFor("the api pod to leave rotation", 180, () =>
    kubectl(["-n", NS, "get", "endpoints", "ivr-ivr-api", "-o", "jsonpath={.subsets[*].addresses[*].ip}"]).trim() === "");

  // Out of rotation, NOT restarted. Liveness must not follow readiness, or one dependency outage
  // restarts every pod at once (P6-1 §1).
  const restarts = kubectl(["-n", NS, "get", "pods", "-l", "app.kubernetes.io/component=api",
    "-o", "jsonpath={.items[0].status.containerStatuses[0].restartCount}"]).trim();
  assert(restarts === "0", `the api pod restarted ${restarts} times during a database outage; liveness is following readiness.`);

  kubectl(["-n", NS, "scale", "deploy", "ivr-postgres", "--replicas=1"]);
  waitFor("the api pod to return to rotation", 240, () =>
    kubectl(["-n", NS, "get", "endpoints", "ivr-ivr-api", "-o", "jsonpath={.subsets[*].addresses[*].ip}"]).trim() !== "");

  process.stdout.write("IT-K8S-PROBE-03 PASS — readiness 503 removed the pod from rotation without restarting it\n");
}

// ---------------------------------------------------------------- IT-K8S-NETPOL-04
function networkPolicy() {
  // Structural half: always checked, and deterministic.
  const manifest = render("dev");
  assert(/kind: NetworkPolicy[\s\S]*?name: ivr-ivr-default-deny/.test(manifest), "the chart has no default-deny policy.");
  assert(!/cidr:\s*0\.0\.0\.0\/0/.test(manifest), "an egress rule opens 0.0.0.0/0, which is not least privilege.");
  const policies = kubectl(["-n", NS, "get", "networkpolicy", "--no-headers"]).trim().split("\n").filter(Boolean);
  assert(policies.length >= 3, `expected three policies in the cluster, found ${policies.length}.`);

  // Behavioural half: only meaningful if this cluster ENFORCES policy at all. A positive control
  // settles that -- without it, an unenforcing cluster would report the same green as a correct
  // policy, which is the worst possible outcome for a security control.
  const enforced = probeEnforcement();
  if (!enforced) {
    process.stdout.write(
      "IT-K8S-NETPOL-04 NOT_PROVEN — policies are present and default-deny, but this cluster does\n"
      + "  not enforce NetworkPolicy (positive control reached the internet through a deny-all).\n"
      + "  The behavioural half needs a policy-enforcing CNI. Recorded, not counted as a pass.\n");
    return false;
  }

  const escaped = reachesInternetAsIvrPod();
  assert(!escaped, "a pod matching the chart's selector reached the internet despite the egress allowlist.");
  process.stdout.write("IT-K8S-NETPOL-04 PASS — egress outside the allowlist is blocked\n");
  return true;
}

function probeEnforcement() {
  const denyAll = [
    "apiVersion: networking.k8s.io/v1",
    "kind: NetworkPolicy",
    "metadata:",
    "  name: netpol-capability-probe",
    "spec:",
    "  podSelector:",
    "    matchLabels:",
    "      netpol-probe: capability",
    "  policyTypes:",
    "    - Egress",
    "",
  ].join("\n");
  const file = path.join(os.tmpdir(), "netpol-capability-probe.yaml");
  fs.writeFileSync(file, denyAll, "utf8");
  docker(["cp", file, `${CLUSTER}:/tmp/netpol-probe.yaml`]);
  kubectl(["-n", NS, "apply", "-f", "/tmp/netpol-probe.yaml"]);
  try {
    kubectl(["-n", NS, "run", "capability-probe", "--rm", "-i", "--restart=Never",
      "--labels=netpol-probe=capability", `--image=${BUSYBOX}`,
      "--command", "--", "wget", "-q", "-T", "6", "-O-", "http://example.com"]);
    return false; // reached the internet through a deny-all: not enforced
  } catch {
    return true;
  } finally {
    try {
      kubectl(["-n", NS, "delete", "networkpolicy", "netpol-capability-probe"], { stdio: ["ignore", "ignore", "ignore"] });
    } catch { /* best effort */ }
  }
}

function reachesInternetAsIvrPod() {
  try {
    kubectl(["-n", NS, "run", "netpol-probe", "--rm", "-i", "--restart=Never",
      "--labels=app.kubernetes.io/name=ivr,app.kubernetes.io/instance=ivr", `--image=${BUSYBOX}`,
      "--command", "--", "wget", "-q", "-T", "6", "-O-", "http://example.com"]);
    return true;
  } catch {
    return false;
  }
}

// ---------------------------------------------------------------- IT-K8S-RETENTION-05
function retentionCronJob() {
  const spec = JSON.parse(kubectl(["-n", NS, "get", "cronjob", "ivr-ivr-retention", "-o", "json"]));
  assert(spec.spec.schedule === "30 2 * * *", `unexpected retention schedule ${spec.spec.schedule}.`);
  // Forbid, not Allow: two retention passes deleting the same rows concurrently is a data-loss
  // shape, not a throughput problem.
  assert(spec.spec.concurrencyPolicy === "Forbid", "the retention CronJob allows concurrent runs.");

  const env = spec.spec.jobTemplate.spec.template.spec.containers[0].env;
  const dryRun = env.find((entry) => entry.name === "Ivr__Retention__DryRun");
  assert(dryRun?.value === "true", "the retention job does not default to a dry run (DF-07).");
  // W-0047. Without this the pod never exits and the Job is recorded as failed, which is exactly
  // what this check missed the first time it ran.
  const runOnce = env.find((entry) => entry.name === "Ivr__Retention__RunOnce");
  assert(runOnce?.value === "true", "the retention job does not run once, so its pod would hang.");
  assert(!env.some((entry) => entry.name.startsWith("IVR_RETENTION_")),
    "the retention job still declares an invented env name the application ignores.");
  assert(env.some((entry) => entry.name === "REAL_CUSTOMER_CALL_ALLOWED" && entry.value === "NO"),
    "the retention job does not inherit the governance floor.");

  // Triggered for real rather than trusted: a CronJob whose pod cannot start is indistinguishable
  // from one that has simply not fired yet.
  try {
    kubectl(["-n", NS, "delete", "job", "retention-selftest"], { stdio: ["ignore", "ignore", "ignore"] });
  } catch { /* nothing to delete on a first run */ }
  kubectl(["-n", NS, "create", "job", "retention-selftest", "--from=cronjob/ivr-ivr-retention"]);
  waitFor("the retention job to finish", 240, () => {
    const job = JSON.parse(kubectl(["-n", NS, "get", "job", "retention-selftest", "-o", "json"]));
    return Boolean(job.status?.succeeded) || Boolean(job.status?.failed);
  });
  const job = JSON.parse(kubectl(["-n", NS, "get", "job", "retention-selftest", "-o", "json"]));
  assert(job.status?.succeeded === 1, `the retention job did not succeed: ${JSON.stringify(job.status)}`);

  process.stdout.write("IT-K8S-RETENTION-05 PASS - CronJob is dry-run by default, Forbid, and one run completed\n");
  return true;
}

// ---------------------------------------------------------------- run
let netpolProven = false;
try {
  docker(["rm", "-f", HELMBOX], { stdio: ["ignore", "ignore", "ignore"] });
  docker(["run", "-d", "--name", HELMBOX, "--entrypoint", "sh", HELM, "-c", "sleep 3600"]);
  sleepSeconds(2);
  docker(["cp", path.join(repositoryRoot, "deploy/helm/ivr"), `${HELMBOX}:/ivr`]);

  lintAndValidate();
  gateRefusesToOpenTheLadder();

  startCluster();
  loadImages();
  deploy();

  gateFromRunningPods();
  readinessRemovesFromRotation();
  netpolProven = networkPolicy();
  const retentionProven = retentionCronJob();

  // Named individually rather than collapsed into one flag. "Something is unproven" tells the
  // next reader to go hunting; naming which one tells them what to fix.
  const unproven = [
    netpolProven ? null : "NETPOL_ENFORCEMENT",
    retentionProven ? null : "RETENTION_EXECUTION",
  ].filter(Boolean);
  process.stdout.write(unproven.length === 0
    ? "K8S_SELFTEST_PASS\n"
    : "K8S_SELFTEST_PASS_WITH_NOT_PROVEN=" + unproven.join(",") + "\n");
} finally {
  if (!keep) {
    for (const container of [HELMBOX, CLUSTER]) {
      try { docker(["rm", "-f", container]); } catch { /* already gone */ }
    }
  }
}
