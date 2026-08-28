#!/usr/bin/env node
// W-0122 / W-0126 F6. render-fixed-speech.mjs is what turns the Owner's three accepted voices into
// the twelve fixed catalog files. Nothing invoked it - not a job, not an npm script - so its guards
// were unexercised code. This drives the real script: every input it must refuse, plus proof that a
// valid one gets all the way past validation to the synthesis call.
import { spawnSync } from "node:child_process";
import { mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import {
  createTestOnlyAcceptance,
  loadVoiceAcceptanceContext,
} from "./tts-voice-acceptance-lib.mjs";

const repoRoot = resolve(import.meta.dirname, "../../..");
const renderer = resolve(repoRoot, "deploy/ci/scripts/render-fixed-speech.mjs");
const scratch = mkdtempSync(join(tmpdir(), "ivr-fixed-render-"));
const pendingTemplate = resolve(repoRoot, "docs/evidence/W-0122/voice-acceptance-manifest.template.json");

// A closed loopback port: enough to reach the synthesis call without a TTS, never a real endpoint.
const closedEndpoint = "http://127.0.0.1:9/";
const acceptancePath = join(scratch, "voice-acceptance-manifest.json");
writeFileSync(
  acceptancePath,
  JSON.stringify(createTestOnlyAcceptance(loadVoiceAcceptanceContext(repoRoot)), null, 2) + "\n",
);

const refusals = [
  ["no-arguments", [], "--endpoint, --output and --acceptance are required"],
  ["odd-arguments", ["--endpoint"], "arguments must be --key value pairs"],
  [
    "https-endpoint",
    ["--endpoint", "https://127.0.0.1/", "--output", scratch, "--acceptance", acceptancePath],
    "render endpoint must be HTTP loopback",
  ],
  [
    "remote-endpoint",
    ["--endpoint", "http://example.com/", "--output", scratch, "--acceptance", acceptancePath],
    "render endpoint must be HTTP loopback",
  ],
  [
    "pending-owner-template",
    ["--endpoint", closedEndpoint, "--output", scratch, "--acceptance", pendingTemplate],
    "owner acceptance status missing",
  ],
  [
    "absent-acceptance",
    ["--endpoint", closedEndpoint, "--output", scratch, "--acceptance", join(scratch, "absent.json")],
    "ENOENT",
  ],
];

for (const [name, args, expected] of refusals) {
  const result = run(args);
  const output = result.stdout + result.stderr;
  if (result.status === 0 || !output.includes(expected)) {
    throw new Error(`renderer did not fail closed on ${name}: expected "${expected}"`);
  }
  process.stdout.write(`TTS_FIXED_RENDER_REFUSAL_PASS case=${name}\n`);
}

// The guards must not be refusing everything: an accepted manifest and a loopback endpoint have to
// reach the synthesis call, which is the only thing missing here.
const reached = run([
  "--endpoint", closedEndpoint,
  "--output", join(scratch, "render"),
  "--acceptance", acceptancePath,
]);
const reachedOutput = reached.stdout + reached.stderr;
if (reached.status === 0 || !reachedOutput.includes("fetch failed")) {
  throw new Error("an accepted manifest on a loopback endpoint did not reach synthesis");
}
process.stdout.write("TTS_FIXED_RENDER_REACHES_SYNTHESIS_PASS endpoint=LOOPBACK_CLOSED\n");
process.stdout.write(`TTS_FIXED_RENDER_SELFTEST_PASS refusals=${refusals.length}\n`);

function run(args) {
  const result = spawnSync(process.execPath, [renderer, ...args], {
    cwd: repoRoot, encoding: "utf8", maxBuffer: 8 * 1024 * 1024,
  });
  return { status: result.status, stdout: result.stdout || "", stderr: result.stderr || "" };
}
