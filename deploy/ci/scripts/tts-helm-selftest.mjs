#!/usr/bin/env node
import { randomUUID } from "node:crypto";
import { spawnSync } from "node:child_process";
import { resolve } from "node:path";

const chartRoot = resolve(import.meta.dirname, "../../helm/ivr");
const helmImage = "alpine/helm:3.16.3";
const helmContainer = `ivr-tts-helm-${randomUUID().replaceAll("-", "").slice(0, 12)}`;

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

  const rendered = helm([
    "template", "ivr", "/ivr",
    "-f", "/ivr/values-prod.yaml",
    "-f", "/ivr/ci/w0122-render-fixture.yaml",
  ]).stdout;

const required = [
  /name: vieneu-tts/,
  /registry\.invalid\/ivr\/vieneu-tts@sha256:0{64}/,
  /Ivr__Speech__Tts__External__Endpoint[\s\S]*http:\/\/127\.0\.0\.1:8090\/synthesize/,
  /Ivr__Speech__Tts__Segmentation__Enabled/,
  /claimName: test-only-vieneu-models/,
  /claimName: test-only-speech-media/,
  /VIE_NEU_VOICE_ACCEPTANCE_MANIFEST/,
  /name: test-only-voice-acceptance/,
  /subPath: voice-acceptance-manifest\.json/,
  /readOnlyRootFilesystem: true/,
  /runAsUser: 1654/,
  /capabilities:[\s\S]*drop:[\s\S]*- ALL/,
];
  for (const pattern of required) {
    if (!pattern.test(rendered)) throw new Error(`positive TTS render missing ${pattern}`);
  }
  if ((rendered.match(/FixedSegments__\d+__TextHash/g) || []).length !== 12) {
    throw new Error("positive TTS render does not contain exactly 12 fixed-segment hashes");
  }
  if (/containerPort: 8090|targetPort: 8090/.test(rendered)) {
    throw new Error("TTS port was exposed outside the worker Pod");
  }

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

  process.stdout.write(
    "TTS_HELM_SELFTEST_PASS defaults=4 fail_closed=YES acceptance_configmap=REQUIRED test_fixture=YES fixed_catalog=12 port_exposed=NO\n",
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
