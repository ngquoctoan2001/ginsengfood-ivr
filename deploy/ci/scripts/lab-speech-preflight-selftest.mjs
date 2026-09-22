#!/usr/bin/env node
import assert from "node:assert/strict";
import { createHash, randomUUID } from "node:crypto";
import { spawnSync } from "node:child_process";
import { resolve } from "node:path";
import { checkSpeechPreflight } from "../../lab/check-speech-preflight.mjs";
import { createSpeechLabProfile } from "../../lab/speech-lab-profile.mjs";

const profileImage = 'sha256:' + 'a'.repeat(64);
for (const mode of ['segmented', 'whole']) {
  const profile = createSpeechLabProfile(mode, profileImage);
  const worker = profile.services['ivr-worker'].environment;
  assert.equal(worker.REAL_CUSTOMER_CALL_ALLOWED, 'NO');
  assert.equal(worker.Ivr__Speech__Tts__Segmentation__Enabled, String(mode === 'segmented'));
  assert.equal(worker.Ivr__Speech__Tts__TimeoutMilliseconds, mode === 'segmented' ? '30000' : '60000');
  assert.equal(worker.Ivr__Speech__Tts__PreparationTimeoutMilliseconds, '120000');
  assert.equal(worker.Ivr__Speech__Tts__PreparationQueueTimeoutMilliseconds, '90000');
  assert.ok(Number(worker.Ivr__Scheduler__LeaseDurationSeconds) > 120 + 120 + 30);
  assert.equal(profile.services['ivr-tts'].cpus, 2);
  assert.equal(profile.services['ivr-tts'].mem_limit, '4g');
  assert.equal(Object.keys(worker).filter(x => x.endsWith('__TextHash')).length, 12);
  assert.equal(profile.services['ivr-tts'].environment.VIE_NEU_ORT_THREADS, '1');
}
assert.throws(() => createSpeechLabProfile('production', profileImage), /INVALID_MODE/);
assert.throws(() => createSpeechLabProfile('segmented', 'ivr-tts:latest'), /IMAGE_NOT_PINNED/);
console.log('LAB_SPEECH_PROFILE_SELFTEST_PASS valid=2 refusal=2');

const root = resolve(import.meta.dirname, "../../..");
const governance = ["IVR_EXECUTION_MODE=LAB_REAL_SIM", "REAL_CUSTOMER_CALL_ALLOWED=NO", "VIE_NEU_BACKEND=vieneu-onnx"];
function fixture(mutate = () => {}, failure = "") {
  let clock = 0;
  let readyCalls = 0;
  let digest = "";
  const calls = [];
  const infos = Object.fromEntries(["ivr-worker", "ivr-tts", "asterisk"].map(service => [service, {
    id: `test-${service}`, running: true, user: "1654:1654", image: "sha256:test-only",
    project: "ginsengfood-ivr-dev", service, network: "container:test-ivr-worker", governance: [...governance],
    mounts: [{ Type: "volume", Name: "test-only-media", RW: service !== "asterisk",
      Destination: service === "asterisk" ? "/var/lib/asterisk/sounds/generated" : "/var/lib/ivr/speech" }],
  }]));
  mutate(infos);
  const run = async args => {
    calls.push(args);
    if (args[0] === "inspect") {
      const service = Object.keys(infos).find(key => args.at(-1) === `ginsengfood-ivr-dev-${key}-1`);
      return JSON.stringify(infos[service]);
    }
    if (args[0] === "rm") return "";
    if (args[0] === "run") {
      assert.match(args[args.indexOf("--mount") + 1], /,volume-nocopy$/, "never overwrite empty-volume ownership");
      const writing = args.includes("-c") && args[args.indexOf("-c") + 1].includes("p.open('xb')");
      if (writing) {
        assert.ok(readyCalls >= 3, "volume touched before readiness");
        assert.ok(args.includes("1654:1654"));
        assert.ok(args.includes("none"));
        if (failure === "write") throw new Error("WRITE_REFUSED");
        digest = createHash("sha256").update(args.at(-1)).digest("hex");
      } else if (failure === "cleanup") throw new Error("CLEANUP_REFUSED");
      return "";
    }
    if (args.includes("python")) {
      readyCalls++;
      if (failure === "timeout" || readyCalls < 3) throw new Error("COLD_START");
      return "VIENEU_READY";
    }
    if (args.includes("sha256sum")) return `${failure === "digest" ? "bad" : digest}  probe`;
    if (args.includes("sh")) {
      if (failure === "read-only") throw new Error("WRITE_WAS_ALLOWED");
      return "READ_ONLY_CONFIRMED";
    }
    throw new Error("UNEXPECTED_COMMAND");
  };
  return { calls, options: { run, timeoutSeconds: 4, now: () => clock, pause: async ms => { clock += ms; } } };
}

const happy = fixture();
assert.match(await checkSpeechPreflight(happy.options), /^LAB_SPEECH_PREFLIGHT_PASS/);
assert.equal(happy.calls.filter(args => args[0] === "run").length, 2, "probe and cleanup both ran");
let refusals = 0;
for (const [name, mutate, message] of [
  ["stopped", x => { x["ivr-tts"].running = false; }, "CONTAINER_NOT_READY"],
  ["wrong-project", x => { x.asterisk.project = "another-project"; }, "CONTAINER_NOT_READY"],
  ["real-calls", x => { x["ivr-worker"].governance = ["IVR_EXECUTION_MODE=LAB_REAL_SIM"]; }, "GOVERNANCE_REFUSED"],
  ["production-mode", x => { x["ivr-tts"].governance = ["REAL_CUSTOMER_CALL_ALLOWED=NO"]; }, "GOVERNANCE_REFUSED"],
  ["test-backend", x => { x["ivr-tts"].governance.pop(); }, "SIDECAR_REFUSED"],
  ["wrong-network", x => { x["ivr-tts"].network = "bridge"; }, "SIDECAR_REFUSED"],
  ["wrong-uid", x => { x["ivr-worker"].user = "0"; }, "WORKER_USER_REFUSED"],
  ["other-volume", x => { x.asterisk.mounts[0].Name = "other"; }, "SHARED_VOLUME_REFUSED"],
  ["worker-read-only", x => { x["ivr-worker"].mounts[0].RW = false; }, "SHARED_VOLUME_REFUSED"],
  ["asterisk-writable", x => { x.asterisk.mounts[0].RW = true; }, "SHARED_VOLUME_REFUSED"],
  ["missing-mount", x => { x.asterisk.mounts = []; }, "SHARED_VOLUME_REFUSED"],
]) {
  const test = fixture(mutate);
  await assert.rejects(checkSpeechPreflight(test.options), new RegExp(message));
  assert.equal(test.calls.filter(args => args[0] !== "inspect").length, 0);
  console.log(`LAB_SPEECH_REFUSAL_PASS case=${name}`);
  refusals++;
}
for (const [name, message] of [["timeout", "READINESS_TIMEOUT"], ["write", "WRITE_REFUSED"],
  ["digest", "MEDIA_DIGEST_MISMATCH"], ["read-only", "WRITE_WAS_ALLOWED"], ["cleanup", "PROBE_CLEANUP_FAILED"]]) {
  const test = fixture(undefined, name);
  await assert.rejects(checkSpeechPreflight(test.options), new RegExp(message));
  const helpers = test.calls.filter(args => args[0] === "run");
  assert.equal(helpers.length, name === "timeout" ? 0 : 2, "no premature writes; failures still clean up");
  console.log(`LAB_SPEECH_REFUSAL_PASS case=${name}`);
  refusals++;
}
await assert.rejects(checkSpeechPreflight({ timeoutSeconds: 0 }), /INVALID_TIMEOUT/);
refusals++;

const args = process.argv.slice(2);
const labRoot = resolve(root, "deploy/lab");
const testScript = resolve(labRoot, "tests/SpeechPreflight.Tests.ps1");
function run(command, argv) {
  const result = spawnSync(command, argv, { encoding: "utf8", windowsHide: true, timeout: 60_000 });
  if (result.error || result.status !== 0) throw new Error(result.error?.message || result.stderr || result.stdout);
  return result.stdout;
}
let output;
if (args.length === 2 && args[0] === "--powershell") {
  output = run(args[1], ["-NoProfile", "-File", testScript, "-LabRoot", labRoot]);
} else {
  assert.ok(args.length === 0 || (args.length === 1 && args[0] === "--self-test"));
  const name = `ivr-preflight-test-${randomUUID().slice(0, 12)}`;
  const image = "mcr.microsoft.com/powershell@sha256:91cdd71ef0cbf76143501321d33613e1b0711d19236dd05a9fd1195da304df93";
  try {
    run("docker", ["create", "--name", name, "--network", "none", "--entrypoint", "pwsh", image,
      "-NoProfile", "-Command", "while ($true) { Start-Sleep -Seconds 60 }"]);
    run("docker", ["start", name]);
    run("docker", ["cp", labRoot, `${name}:/lab`]);
    output = run("docker", ["exec", name, "pwsh", "-NoProfile", "-File", "/lab/tests/SpeechPreflight.Tests.ps1", "-LabRoot", "/lab"]);
  } finally { spawnSync("docker", ["rm", "--force", name], { windowsHide: true, timeout: 10_000 }); }
}
assert.equal((output.match(/LAB_SPEECH_ENTRY_REFUSAL_PASS/g) || []).length, 2);
assert.equal((output.match(/LAB_SPEECH_ENTRY_CONTINUE_PASS/g) || []).length, 2);
assert.equal((output.match(/LAB_ORDER_VARIANT_PASS/g) || []).length, 12);
process.stdout.write(output);
console.log(`LAB_SPEECH_PREFLIGHT_SELFTEST_PASS valid=1 refusal=${refusals} entry_guards=4 orders=12`);
