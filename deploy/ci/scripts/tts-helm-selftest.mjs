#!/usr/bin/env node
import { randomUUID } from "node:crypto";
import { spawnSync } from "node:child_process";
import { resolve } from "node:path";

const chartRoot = resolve(import.meta.dirname, "../../helm/ivr");
const helmImage = "alpine/helm:3.16.3";
const helmContainer = `ivr-tts-helm-${randomUUID().replaceAll("-", "").slice(0, 12)}`;
const N1_REFUSAL = "worker.tts.enabled is refused: N1, the Tech Lead's transition rule of 24/09";

try {
  docker([
    "create", "--name", helmContainer, "--entrypoint", "/bin/sh", helmImage,
    "-c", "while :; do sleep 60; done",
  ]);
  docker(["start", helmContainer]);
  docker(["cp", `${chartRoot}/.`, `${helmContainer}:/ivr`]);

  for (const environment of ["dev", "lab", "staging", "prod"]) {
    helm(["lint", "/ivr", "-f", `/ivr/values-${environment}.yaml`]);
  }

  // W-0363 / K-50. N1, the Tech Lead's transition rule of 24/09: production does not synthesize
  // speech at call time. The complete W-0122 candidate used to render here as the positive case;
  // it is now refused, and by that rule rather than by anything it lacks. The cases below still
  // reach their own guards, which run first, so an incomplete candidate names what is missing.
  expectRenderFailure(
    ["template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml", "-f", "/ivr/ci/w0122-render-fixture.yaml"],
    N1_REFUSAL,
  );

expectRenderFailure(
  ["template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml", "--set", "worker.tts.enabled=true"],
  "requires governance.executionMode=PRODUCTION_REAL",
);
expectRenderFailure(
  ["template", "ivr", "/ivr", "-f", "/ivr/values-lab.yaml", "--set", "worker.tts.enabled=true"],
  "lab must use the explicit Compose overlay",
);
expectRenderFailure(
  [
    "template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml",
    "-f", "/ivr/ci/w0122-render-fixture.yaml",
    "--set", "worker.tts.voiceAcceptance.existingConfigMap=",
  ],
  "worker.tts.voiceAcceptance requires a valid existingConfigMap",
);
// W-0122 4.6. A longer per-request timeout is not capacity: it has to be measured, and the
// three sequential dynamic segments still have to fit the pre-dial window with headroom.
expectRenderFailure(
  [
    "template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml",
    "-f", "/ivr/ci/w0122-render-fixture.yaml",
    "--set", "worker.tts.timeoutMilliseconds=30000",
  ],
  "above the accepted worker baseline of 5000",
);
expectRenderFailure(
  [
    "template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml",
    "-f", "/ivr/ci/w0122-render-fixture.yaml",
    "--set", "worker.tts.timeoutMilliseconds=30000",
    "--set", "worker.tts.approvals.performanceRef=TEST_ONLY_MEASUREMENT",
  ],
  "leaves less than the 20 percent headroom",
);
// The budget guard is still a budget, not a blanket refusal: a measured raise that fits passes
// it. It used to render; under N1 it now reaches the transition-rule refusal instead.
expectRenderFailure(
  [
    "template", "ivr", "/ivr", "-f", "/ivr/values-prod.yaml",
    "-f", "/ivr/ci/w0122-render-fixture.yaml",
    "--set", "worker.tts.timeoutMilliseconds=16000",
    "--set", "worker.tts.approvals.performanceRef=TEST_ONLY_MEASUREMENT",
  ],
  N1_REFUSAL,
);

  process.stdout.write(
    "TTS_HELM_SELFTEST_PASS defaults=4 fail_closed=YES acceptance_configmap=REQUIRED predial_budget=ENFORCED prod_candidate=REFUSED_N1\n",
  );
} finally {
  spawnSync("docker", ["rm", "--force", helmContainer], { stdio: "ignore" });
}

function helm(arguments_, expectSuccess = true) {
  return docker(["exec", helmContainer, "helm", ...arguments_], expectSuccess);
}

function docker(arguments_, expectSuccess = true) {
  const result = spawnSync("docker", arguments_, {
    encoding: "utf8", maxBuffer: 16 * 1024 * 1024,
  });
  if (expectSuccess && result.status !== 0) {
    throw new Error(`docker/helm command failed (${result.status})`);
  }
  return { status: result.status, stdout: result.stdout || "", stderr: result.stderr || "" };
}

function expectRenderFailure(arguments_, expectedMessage) {
  const result = helm(arguments_, false);
  const output = result.stdout + result.stderr;
  if (result.status === 0 || !output.includes(expectedMessage)) {
    throw new Error(`expected fail-closed Helm guard: ${expectedMessage}`);
  }
}
