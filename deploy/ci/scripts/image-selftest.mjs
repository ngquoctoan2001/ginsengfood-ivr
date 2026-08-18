#!/usr/bin/env node
// W-0043 / P7-1 §8. Checks the container images as artifacts, not the code inside them.
//
// These four questions cannot be answered by any .NET test: whether the published image runs as
// root, whether its healthcheck actually reports healthy, whether the dev stack comes up with no
// route out, and whether the scan gate still fails on a HIGH. A green unit suite says nothing
// about any of them.
//
// Run: node deploy/ci/scripts/image-selftest.mjs [--skip-compose] [--skip-scan]
import { execFileSync } from "node:child_process";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const skipCompose = process.argv.includes("--skip-compose");
const skipScan = process.argv.includes("--skip-scan");

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

buildAndCheckUser();
checkHealthcheck();
if (!skipCompose) checkCompose();
if (!skipScan) checkScan();
process.stdout.write("IMAGE_SELFTEST_PASS\n");
