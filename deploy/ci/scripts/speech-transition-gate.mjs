#!/usr/bin/env node
// W-0363 / K-50. N1, the Tech Lead's transition rule of 24/09: production does not synthesize
// speech at call time; it plays audio rendered ahead of time, by VieNeu offline. This gate reads the
// chart files that decide what a production release would do, without docker or helm, and fails if
// any of them would let production synthesize again:
//
//   1. values.yaml and values-prod.yaml leave worker.tts.enabled off;
//   2. values-prod.yaml names no EXTERNAL_CONFIGURABLE provider;
//   3. the worker template wires no loopback synthesis endpoint;
//   4. the TTS guard in _helpers.tpl carries the N1 refusal, so a complete W-0122 candidate is refused;
//   5. values-prod-tts.draft.yaml is marked SUPERSEDED.
//
// tts-helm-selftest.mjs proves the refusal renders; this gate proves the files still say it, and runs
// in the offline sweep. --self-test also breaks each check on a copy and requires it to fail.
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const chart = path.join(root, "deploy/helm/ivr");
const FILES = [
  "values.yaml",
  "values-prod.yaml",
  "templates/deployment-worker.yaml",
  "templates/_helpers.tpl",
  "values-prod-tts.draft.yaml",
];
const N1_REFUSAL = "worker.tts.enabled is refused: N1";

export function problems(files) {
  const found = [];
  for (const name of ["values.yaml", "values-prod.yaml"]) {
    if (YAML.parse(files[name])?.worker?.tts?.enabled === true) {
      found.push(`${name} turns worker.tts on`);
    }
  }
  if (files["values-prod.yaml"].includes("EXTERNAL_CONFIGURABLE")) {
    found.push("values-prod.yaml names the EXTERNAL_CONFIGURABLE provider");
  }
  if (/127\.0\.0\.1[^\n]*\/synthesize/u.test(files["templates/deployment-worker.yaml"])) {
    found.push("the worker template wires a loopback synthesis endpoint");
  }
  if (!files["templates/_helpers.tpl"].includes(N1_REFUSAL)) {
    found.push("the TTS guard in _helpers.tpl does not carry the N1 refusal");
  }
  if (!/^# SUPERSEDED\b/u.test(files["values-prod-tts.draft.yaml"])) {
    found.push("values-prod-tts.draft.yaml is not marked SUPERSEDED");
  }
  return found;
}

const files = Object.fromEntries(FILES.map((file) => [file, fs.readFileSync(path.join(chart, file), "utf8")]));
const found = problems(files);
if (found.length > 0) {
  process.stderr.write(`SPEECH_TRANSITION_GATE_FAIL\n${found.map((problem) => `  - ${problem}`).join("\n")}\n`);
  process.exit(1);
}

let selfTest = "";
if (process.argv.includes("--self-test")) {
  const enableTts = (text) => {
    const values = YAML.parse(text);
    values.worker.tts.enabled = true;
    return YAML.stringify(values);
  };
  const breakers = [
    ["values-prod.yaml", enableTts],
    ["values.yaml", enableTts],
    ["values-prod.yaml", (text) => `${text}\n# Ivr__Speech__Tts__Provider: EXTERNAL_CONFIGURABLE\n`],
    ["templates/deployment-worker.yaml",
      (text) => `${text}\n            - name: Ivr__Speech__Tts__External__Endpoint\n              value: "http://127.0.0.1:8090/synthesize"\n`],
    ["templates/_helpers.tpl", (text) => text.replaceAll(N1_REFUSAL, "worker.tts.enabled is checked")],
    ["values-prod-tts.draft.yaml", (text) => text.replace(/^# SUPERSEDED[^\n]*\n/u, "")],
  ];
  for (const [file, breakIt] of breakers) {
    const broken = problems({ ...files, [file]: breakIt(files[file]) });
    if (broken.length !== 1) {
      process.stderr.write(`SPEECH_TRANSITION_GATE_SELFTEST_FAIL breaking ${file} found ${broken.length} problems\n`);
      process.exit(1);
    }
  }
  selfTest = ` self_test=${breakers.length}/${breakers.length}`;
}

process.stdout.write(`SPEECH_TRANSITION_GATE_PASS checks=5 prod_tts=OFF loopback_endpoint=NONE n1_refusal=PRESENT draft=SUPERSEDED${selfTest}\n`);
