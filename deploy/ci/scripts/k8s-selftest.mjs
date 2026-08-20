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

function writeAndApply(name, content) {
  const local = path.join(os.tmpdir(), name);
  fs.writeFileSync(local, content, "utf8");
  docker(["cp", local, `${CLUSTER}:/tmp/${name}`]);
  kubectl(["-n", NS, "apply", "-f", `/tmp/${name}`]);
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

  // The migrate Job goes in FIRST and finishes before anything else is applied.
  //
  // `helm template` emits hook resources inline, so applying the whole render at once creates the
  // Job and the Deployments in the same breath and Helm's `pre-install` ordering is lost. The
  // worker then raced the schema and died with `relation "ivr_sim_channels" does not exist` --
  // SIGSEGV, restart, second attempt fine. Nobody noticed because Kubernetes healed it, and the
  // suite only ever asserted the end state.
  //
  // That crash was an artefact of THIS harness, not of the chart. Splitting the apply is what makes
  // the harness deploy in the order a real `helm install` does, so the cluster under test is the
  // one the chart describes.
  const rendered = render("dev");
  const documents = rendered.split(/\r?\n---/);
  const migrateJob = documents.filter((document) =>
    /kind: Job/.test(document) && /-migrate/.test(document));
  assert(migrateJob.length === 1, `expected one migrate Job in the render, found ${migrateJob.length}.`);

  writeAndApply("ivr-migrate.yaml", migrateJob.join("\n---"));
  waitFor("the migration hook to complete", 240, () =>
    kubectl(["-n", NS, "get", "job", "ivr-ivr-migrate", "--no-headers"]).includes("1/1"));

  writeAndApply("ivr-dev.yaml", documents.filter((document) => !migrateJob.includes(document)).join("\n---"));
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

// ---------------------------------------------------------------- IT-K8S-WORKER-06
//
// The worker liveness probe, measured on the cluster rather than assumed.
//
// Two claims here cannot be read off a manifest. First, whether the kubelet can reach a port on a
// pod the default-deny NetworkPolicy covers: probe traffic comes from the node, not from a pod,
// and whether that is exempt is a property of the CNI rather than of the chart. Second, whether a
// database outage restarts the worker -- with PostgreSQL down every loop fails on every pass, and
// a probe that restarted on that would add a restart storm to an outage at the worst moment.
function workerLivenessProbe() {
  // Structural half. Liveness only: nothing routes traffic to the worker, so a readinessProbe
  // would be one more thing to be wrong about and nothing to be right about.
  const manifest = render("dev");
  const workerDocument = manifest
    .split("\n---")
    .find((document) => /kind: Deployment/.test(document) && /component: worker/.test(document));
  assert(workerDocument, "the chart renders no worker Deployment.");
  // Keys, not prose. A substring search matched the comment that explains WHY there is no
  // readinessProbe and failed the check the comment was describing -- the same shape of mistake as
  // a PII scan that flags the sentence documenting the rule. Comment lines are dropped first, and
  // the key has to appear as a key.
  const workerKeys = workerDocument
    .split(/\r?\n/)
    .filter((line) => !line.trimStart().startsWith("#"))
    .join("\n");
  assert(
    /^\s*livenessProbe:/m.test(workerKeys),
    "the worker deployment declares no livenessProbe.");
  assert(
    !/^\s*readinessProbe:/m.test(workerKeys),
    "the worker declares a readinessProbe; nothing routes traffic to it, so readiness has no "
    + "meaning here.");

  // Behavioural half, and the reason for the check. A probe the kubelet never ran would leave
  // restartCount at 0 and the pod Ready -- indistinguishable from a probe that ran and passed --
  // so the claim comes from the pod reporting the probe's own container state.
  waitFor("the worker pod to become Ready with its probe passing", 240, () =>
    kubectl(["-n", NS, "get", "pod", "-l", "app.kubernetes.io/component=worker",
      "-o", 'jsonpath={.items[0].status.conditions[?(@.type=="Ready")].status}']).trim() === "True");

  // The claim is about the PROBE, so the measurement has to be about the probe. Asserting zero
  // restarts measures every reason a container can die -- and the first run of this check failed on
  // one that had nothing to do with the probe: the worker raced the schema and segfaulted. That was
  // worth finding and worth fixing, but it is not what this assertion is for, and a check that
  // fails for the wrong reason gets read as noise.
  assert(
    workerProbeFailures() === 0,
    `the kubelet reported ${workerProbeFailures()} liveness failures against a healthy worker; the `
    + "probe is failing a working worker, which is worse than having no probe.");

  // And the opposite direction, which is the half that makes the first one mean something. The
  // kubelet reaches the port; another POD must not. Both facts are about the same port and they
  // pull opposite ways, so measuring only one would leave either "the probe never ran" or "the
  // health port is an open door in the namespace" indistinguishable from a pass.
  const workerIp = kubectl(["-n", NS, "get", "pod", "-l", "app.kubernetes.io/component=worker",
    "-o", "jsonpath={.items[0].status.podIP}"]).trim();
  assert(workerIp !== "", "the worker pod has no IP.");
  startProbePod("worker-health-probe", "role=netpol-probe");
  let reachedHealthPort = false;
  try {
    kubectl(["-n", NS, "exec", "worker-health-probe", "--",
      "wget", "-q", "-T", "6", "-O-", `http://${workerIp}:8081/healthz`]);
    reachedHealthPort = true;
  } catch { /* the refusal is the pass */ }
  deleteProbePod("worker-health-probe");
  assert(
    !reachedHealthPort,
    "a pod in the namespace reached the worker's health port. The endpoint reports which loops are "
    + "running and how they last failed; default-deny is what keeps that from being readable by "
    + "anything that lands in the namespace.");

  // The outage. Long enough for several probe periods (15s each, failureThreshold 3) so that a
  // probe which treated "failing" as "stopped" would have restarted the pod by now.
  const restartsBeforeOutage = workerRestartCount();
  kubectl(["-n", NS, "scale", "deploy", "ivr-postgres", "--replicas=0"]);
  sleepSeconds(90);
  const restartsDuringOutage = workerRestartCount();
  kubectl(["-n", NS, "scale", "deploy", "ivr-postgres", "--replicas=1"]);

  assert(
    workerProbeFailures() === 0,
    "the kubelet reported liveness failures during a database outage. A loop that is turning and "
    + "failing is not a restart signal: restarting does not repair a dependency, it adds a restart "
    + "storm to the outage.");
  assert(
    restartsDuringOutage === restartsBeforeOutage,
    `the worker restart count moved from ${restartsBeforeOutage} to ${restartsDuringOutage} during `
    + "a database outage.");

  process.stdout.write(
    "IT-K8S-WORKER-06 PASS — the kubelet reaches the worker probe through default-deny, and 90s of "
    + "database outage produced 0 restarts\n");
}

/**
 * How many times the kubelet has reported the liveness probe failing on the worker pod.
 *
 * Read from events rather than from restartCount, because restartCount counts every death and the
 * question here is narrower: did the PROBE kill it. Kubernetes records a probe failure as an
 * Unhealthy event naming which probe, so the two can be told apart.
 */
function workerProbeFailures() {
  const events = kubectl(["-n", NS, "get", "events", "--field-selector", "reason=Unhealthy",
    "-o", 'jsonpath={range .items[*]}{.involvedObject.name}|{.message}{"\\n"}{end}']);
  return events
    .split(/\r?\n/)
    .filter((line) => line.includes("worker") && /Liveness probe failed/i.test(line))
    .length;
}

/** Whether a pod labelled as the console can reach the API service. */
function consoleReachesApi() {
  startProbePod(
    "console-probe",
    "app.kubernetes.io/name=ivr,app.kubernetes.io/instance=ivr,app.kubernetes.io/component=ui");
  try {
    return podReachesApi("console-probe");
  } finally {
    deleteProbePod("console-probe");
  }
}

function consoleReachesApiFailure() {
  return "a pod labelled as the console could not reach the API. Both ends of a NetworkPolicy have "
    + "to permit a hop: the ingress rule names the console, and the egress allowlist has to name "
    + "the API back. The console renders server-side, so without this it cannot load a page.";
}

function unlabelledPodReachesApi() {
  startProbePod("stranger-probe", "role=netpol-probe");
  try {
    return podReachesApi("stranger-probe");
  } finally {
    deleteProbePod("stranger-probe");
  }
}

/** A GET the API answers with anything at all. 000/no output means the connection never landed. */
function podReachesApi(podName) {
  const output = kubectlSoft(["-n", NS, "exec", podName, "--", "sh", "-c",
    "wget -q -T 6 -S -O /dev/null http://ivr-ivr-api:8080/health/live 2>&1"]);
  return /HTTP\/1\.[01] \d{3}/.test(output);
}

function workerRestartCount() {
  return kubectl(["-n", NS, "get", "pods", "-l", "app.kubernetes.io/component=worker",
    "-o", "jsonpath={.items[0].status.containerStatuses[0].restartCount}"]).trim();
}

/**
 * kubectl that returns output instead of throwing on a non-zero exit.
 *
 * The rotation drill needs it: a 403 is the measurement, and wget reports a 403 by exiting
 * non-zero. Losing that as an exception would turn the observation into a crash.
 */
function kubectlSoft(args) {
  try {
    return kubectl(args);
  } catch (error) {
    return String(error.stdout ?? "") + String(error.stderr ?? "");
  }
}

// ---------------------------------------------------------------- IT-K8S-ROTATE-07
//
// The half of W-0047 that one container could not answer.
//
// The original drill proved the MIDDLEWARE across the retirement boundary: one process, both
// tokens configured, old accepted until T and refused after. What it could not touch is FLEET
// behaviour during a rolling restart, when pods carrying the old configuration and pods carrying
// the new one are both serving. That is where a rotation actually hurts, and it is invisible to
// any test with one replica.
//
// Two claims, and they point opposite ways -- which is the point. Measuring only the first would
// read as "rotation is seamless", and it is seamless for exactly one of the two clients:
//
//   OLD token   never rejected, throughout.  Old pods hold it as current, new pods hold it as
//               previous, so every pod in every state of the rollout accepts it.
//   NEW token   rejected until the rollout finishes.  A pod that has not restarted has never
//               heard of it, and no amount of overlap can fix that.
//
// The second is why the runbook ordering is what it is: roll the credential out FIRST, let the
// fleet converge, and only then switch callers. Doing it the other way round fails every caller
// for the length of a deploy.
function rotationAcrossARollingRestart() {
  const oldToken = "dev-ordercore-token-not-a-real-secret";
  const newToken = "dev-ordercore-token-rotated-not-a-real-secret";

  // Two replicas, because one replica has no "during" to observe: it is either old or new.
  kubectl(["-n", NS, "scale", "deploy", "ivr-ivr-api", "--replicas=2"]);
  waitFor("both api replicas to be ready", 240, () =>
    kubectl(["-n", NS, "get", "deploy", "ivr-ivr-api",
      "-o", "jsonpath={.status.readyReplicas}"]).trim() === "2");

  // Labelled as the console, because that is the caller being simulated -- and because the policy
  // only lets the console reach the API. A pod with arbitrary labels would be refused by the
  // egress allowlist and every probe would read as a rejection, which is indistinguishable from
  // the auth refusal this drill measures.
  startProbePod(
    "rotation-probe",
    "app.kubernetes.io/name=ivr,app.kubernetes.io/instance=ivr,app.kubernetes.io/component=ui");
  try {
    // Baseline: the old token works before anything moves. Without this the "never rejected"
    // claim below could be satisfied by a probe that never reached the service at all.
    assert(
      probeToken("rotation-probe", oldToken) !== 403,
      "the old token was already rejected before the rotation started; the drill would prove "
      + "nothing about the rollout.");
    assert(
      probeToken("rotation-probe", newToken) === 403,
      "the new token was already accepted before the rotation; the fleet is not in the state this "
      + "drill claims to start from.");

    // Roll the credential out. The overlap is what the chart could not express until now: the new
    // token becomes current and the old one stays valid until a fixed instant.
    // Computed here, not in the container: busybox date has no relative -d, and the same
    // limitation already cost a retention drill once. The pods run UTC, so an ISO instant needs no
    // translation on the way in.
    const retiresAt = new Date(Date.now() + 3_600_000).toISOString().replace(/\.\d+Z$/, "Z");
    kubectl(["-n", NS, "set", "env", "deploy/ivr-ivr-api",
      `ORDER_CORE_SERVICE_TOKEN=${newToken}`,
      `ORDER_CORE_SERVICE_TOKEN_PREVIOUS=${oldToken}`,
      `ORDER_CORE_SERVICE_TOKEN_PREVIOUS_RETIRES_AT=${retiresAt}`]);

    // Probe both tokens continuously WHILE the rollout runs. Sampling before and after would miss
    // the only interval this check exists to observe.
    let oldRejections = 0;
    let newRejections = 0;
    let samples = 0;
    let rolloutComplete = false;
    for (let attempt = 0; attempt < 60 && !rolloutComplete; attempt += 1) {
      if (probeToken("rotation-probe", oldToken) === 403) oldRejections += 1;
      if (probeToken("rotation-probe", newToken) === 403) newRejections += 1;
      samples += 1;
      rolloutComplete = kubectlSoft(["-n", NS, "rollout", "status", "deploy/ivr-ivr-api",
        "--timeout=2s"]).includes("successfully rolled out");
    }
    assert(rolloutComplete, `the api rollout did not finish within ${samples} probe rounds.`);
    assert(samples >= 3, `only ${samples} samples were taken; the rollout was too fast to observe.`);

    // Claim one: the overlap held. This is what the whole dual-key mechanism is for, and it is the
    // half a single-container drill could already suggest but not prove for a fleet.
    assert(
      oldRejections === 0,
      `the old token was rejected ${oldRejections} times out of ${samples} during the rollout. The `
      + "overlap did not hold, so callers still using the outgoing credential saw failures.");

    // Claim two: and it did NOT cover the new token. Asserted as a positive expectation rather
    // than tolerated, because a run where the new token never failed would mean the rollout was
    // over before probing began -- and then claim one measured nothing either.
    assert(
      newRejections > 0,
      "the new token was never rejected during the rollout, which means no old pod was still "
      + "serving when probing started. Both claims here are about the overlap WINDOW, and this run "
      + "did not observe one.");

    // After convergence both are accepted: new as current, old as previous until the instant above.
    assert(
      probeToken("rotation-probe", newToken) !== 403
      && probeToken("rotation-probe", oldToken) !== 403,
      "after the rollout the fleet does not accept both halves of the overlap.");

    process.stdout.write(
      `IT-K8S-ROTATE-07 PASS — across a 2-replica rolling restart the OLD token was rejected `
      + `0/${samples} times and the NEW token ${newRejections}/${samples}: the overlap protects `
      + "callers that have not switched yet, and cannot protect callers that switched early\n");
  } finally {
    deleteProbePod("rotation-probe");
    kubectlSoft(["-n", NS, "scale", "deploy", "ivr-ivr-api", "--replicas=1"]);
  }
}

/**
 * One request through the api SERVICE, so it lands on whichever replica the round robin picks --
 * probing a pod IP would pin the drill to one pod and measure the thing it is trying to avoid.
 *
 * 403 is the auth refusal. Anything else (400 for a body the schema rejects) means the request got
 * PAST auth, which is the signal: the drill is about who is let in, not about what they sent.
 */
function probeToken(podName, token) {
  // 2>&1 inside the container, not outside: wget reports the status line on STDERR, and
  // execFileSync hands back stdout alone. Without the redirect every probe parsed as "no status"
  // and the drill read a working fleet as a broken one.
  const output = kubectlSoft(["-n", NS, "exec", podName, "--", "sh", "-c",
    "wget -q -T 5 -S -O /dev/null --post-data '{}'"
    + " --header 'Content-Type: application/json'"
    + " --header 'X-Source-System: order-core'"
    + ` --header 'Authorization: Bearer ${token}'`
    + " --header 'X-Correlation-Id: rotation-drill'"
    + " --header 'Idempotency-Key: rotation-drill'"
    + " http://ivr-ivr-api:8080/v1/ivr/order-confirmation/tasks 2>&1"]);
  const status = /HTTP\/1\.[01] (\d{3})/.exec(output);
  return status ? Number(status[1]) : 0;
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
  const { enforced, reachedBefore } = probeEnforcement();
  assert(
    reachedBefore,
    "the capability probe could not reach the internet even before the deny-all converged, so a "
    + "block here would prove nothing: the pod has no route, and geography is not a policy.");
  if (!enforced) {
    process.stdout.write(
      "IT-K8S-NETPOL-04 NOT_PROVEN — policies are present and default-deny, but this cluster does\n"
      + "  not enforce NetworkPolicy: a deny-all was applied to a running pod and the pod was still\n"
      + "  reaching the internet a minute later. Needs a policy-enforcing CNI. Recorded, not a pass.\n");
    return false;
  }

  const escaped = reachesInternetAsIvrPod();
  assert(!escaped, "a pod matching the chart's selector reached the internet despite the egress allowlist.");

  // A policy suite that only checks its REFUSALS always passes, and this one did: every case above
  // asserts something is blocked. The console calling the API is the hop that has to work, both
  // ends have to agree for it to, and only the ingress end ever did -- so on any enforcing cluster
  // the console could not load a page. Measured now, in both directions.
  assert(consoleReachesApi(), consoleReachesApiFailure());
  assert(
    !unlabelledPodReachesApi(),
    "a pod with no console labels reached the API. The ingress policy names the console "
    + "specifically; if anything in the namespace can call it, that rule is decoration.");

  process.stdout.write(
    "IT-K8S-NETPOL-04 PASS — egress outside the allowlist is blocked, the console reaches the API, "
    + "and a pod that is not the console does not\n");
  return true;
}

/**
 * A probe pod that stays alive, so the network call is separated from pod creation.
 *
 * The original probes ran `kubectl run --rm -i -- wget`: start a pod and immediately call out.
 * kube-router installs per-pod iptables rules AFTER the pod appears, so a pod that races out of
 * the gate wins -- and that race is what made this cluster look like one that does not enforce
 * NetworkPolicy at all. It does.
 */
function startProbePod(name, labels) {
  deleteProbePod(name);
  kubectl(["-n", NS, "run", name, "--restart=Never", `--labels=${labels}`, `--image=${BUSYBOX}`,
    "--command", "--", "sleep", "600"]);
  kubectl(["-n", NS, "wait", "--for=condition=Ready", `pod/${name}`, "--timeout=120s"]);
}

function deleteProbePod(name) {
  try {
    kubectl(["-n", NS, "delete", "pod", name, "--ignore-not-found", "--now"],
      { stdio: ["ignore", "ignore", "ignore"] });
  } catch { /* best effort */ }
}

/** Whether the pod can reach the internet right now. */
function podReachesInternet(name) {
  try {
    kubectl(["-n", NS, "exec", name, "--", "wget", "-q", "-T", "6", "-O-", "http://example.com"]);
    return true;
  } catch {
    return false;
  }
}

/** Polls until the pod stops reaching the internet, or the deadline passes. */
function waitUntilBlocked(name, seconds) {
  for (let attempt = 0; attempt * 3 < seconds; attempt += 1) {
    if (!podReachesInternet(name)) {
      return true;
    }
    sleepSeconds(3);
  }
  return false;
}

/**
 * Does this cluster enforce NetworkPolicy at all?
 *
 * Order matters, and getting it wrong is what made the first version of this useless. The pod
 * starts with NO policy against it and its reachability is measured first. Only then is the
 * deny-all applied. Measuring only after cannot tell "the policy blocked it" from "it never had a
 * route" -- and a pod with no route is blocked by geography, not by policy.
 */
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

  try {
    startProbePod("capability-probe", "netpol-probe=capability");
    const reachedBefore = podReachesInternet("capability-probe");

    kubectl(["-n", NS, "apply", "-f", "/tmp/netpol-probe.yaml"]);

    // Then wait. "Not enforced" and "not enforced YET" look identical at t=0, and reading the
    // first as the second is what recorded this cluster as non-enforcing across four slices.
    return { enforced: waitUntilBlocked("capability-probe", 60), reachedBefore };
  } finally {
    deleteProbePod("capability-probe");
    try {
      kubectl(["-n", NS, "delete", "networkpolicy", "netpol-capability-probe"],
        { stdio: ["ignore", "ignore", "ignore"] });
    } catch { /* best effort */ }
  }
}

/**
 * Can a pod matching the chart's selector reach the internet?
 *
 * Same convergence wait, for the opposite reason. Here "blocked" is the pass, so a race that
 * blocked the pod by accident would hand out a PASS nobody earned.
 */
function reachesInternetAsIvrPod() {
  try {
    startProbePod("netpol-probe", "app.kubernetes.io/name=ivr,app.kubernetes.io/instance=ivr");
    return !waitUntilBlocked("netpol-probe", 60);
  } finally {
    deleteProbePod("netpol-probe");
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
  workerLivenessProbe();
  rotationAcrossARollingRestart();
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
