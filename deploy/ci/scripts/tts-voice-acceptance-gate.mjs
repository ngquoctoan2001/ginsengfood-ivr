#!/usr/bin/env node
import { resolve } from "node:path";
import {
  createTestOnlyAcceptance,
  loadVoiceAcceptanceContext,
  loadVoiceAcceptanceSources,
  validateVoiceAcceptance,
  validateVoiceAcceptanceFile,
  verifyVoiceConfigBindings,
} from "./tts-voice-acceptance-lib.mjs";

const repoRoot = resolve(import.meta.dirname, "../../..");
const args = process.argv.slice(2);

if (args.includes("--selftest")) {
  const context = loadVoiceAcceptanceContext(repoRoot);
  const fixture = createTestOnlyAcceptance(context);
  validateVoiceAcceptance(fixture, context);
  let pendingTemplateRejected = false;
  try {
    validateVoiceAcceptanceFile(
      resolve(repoRoot, "docs/evidence/W-0122/voice-acceptance-manifest.template.json"),
      repoRoot,
    );
  } catch {
    pendingTemplateRejected = true;
  }
  if (!pendingTemplateRejected) throw new Error("pending Owner template was accepted");
  process.stdout.write("TTS_VOICE_ACCEPTANCE_PENDING_TEMPLATE_PASS rejected=YES\n");

  // The fixture below is built from the same declared authority it is then checked against, so on
  // its own it can only prove the manifest rules. These mutations prove the separate half: that a
  // voices.json which no longer describes the real tree is rejected instead of quietly becoming
  // the new expectation.
  const bindingMutations = [
    ["file-hash", value => { value.voiceConfig.dependency_lock_sha256 = "0".repeat(64); }],
    ["model-lock", value => { value.voiceConfig.model_lock_sha256 = "0".repeat(64); }],
    ["audition-script", value => { value.voiceConfig.audition_script_sha256 = "0".repeat(64); }],
    ["audition-source", value => { value.voiceConfig.source_commit = "0".repeat(40); }],
    ["listening-profile", value => { value.voiceConfig.listening_profile_id = "w0122-direct-wav"; }],
    ["codec-revision", value => { value.voiceConfig.codec_revision = "0".repeat(40); }],
  ];
  for (const [name, mutate] of bindingMutations) {
    const sources = loadVoiceAcceptanceSources(repoRoot);
    mutate(sources);
    let rejected = false;
    try {
      verifyVoiceConfigBindings(repoRoot, sources);
    } catch {
      rejected = true;
    }
    if (!rejected) throw new Error(`voice config binding mutation was not rejected: ${name}`);
    process.stdout.write(`TTS_VOICE_CONFIG_BINDING_PASS mutation=${name}\n`);
  }

  const mutations = [
    ["status", value => { value.status = "PENDING_OWNER_LISTENING"; }],
    ["stale", value => { value.stale_relisten_required = true; }],
    ["hash", value => { value.runtime_lock_sha256 = "0".repeat(64); }],
    ["route", value => { value.listening_route = "DIRECT_WAV"; }],
    ["incomplete", value => { value.all_11_candidates_listened = false; }],
    ["region", value => { value.selections.South = { ...value.selections.North }; }],
    ["unheard", value => { value.candidate_results[0].listened = false; }],
    ["verdict", value => { value.candidate_results[0].verdict = "NOT_SELECTED"; }],
    ["extra", value => { value.unreviewed = true; }],
  ];
  for (const [name, mutate] of mutations) {
    const candidate = structuredClone(fixture);
    mutate(candidate);
    let rejected = false;
    try {
      validateVoiceAcceptance(candidate, context);
    } catch {
      rejected = true;
    }
    if (!rejected) throw new Error(`acceptance mutation was not rejected: ${name}`);
    process.stdout.write(`TTS_VOICE_ACCEPTANCE_MUTATION_PASS mutation=${name}\n`);
  }
  process.stdout.write(
    `TTS_VOICE_ACCEPTANCE_SELFTEST_PASS fixture=TEST_ONLY authority=NONE`
      + ` bindings=${bindingMutations.length} mutations=${mutations.length}\n`,
  );
  process.exit(0);
}

const acceptanceIndex = args.indexOf("--acceptance");
if (acceptanceIndex < 0 || !args[acceptanceIndex + 1]) {
  throw new Error("use --selftest or --acceptance <owner-signed-manifest.json>");
}
const result = validateVoiceAcceptanceFile(args[acceptanceIndex + 1], repoRoot);
process.stdout.write(
  `TTS_VOICE_ACCEPTANCE_PASS manifest_sha256=${result.sha256}`
    + ` north=${result.selections.North.voice_id}`
    + ` central=${result.selections.Central.voice_id}`
    + ` south=${result.selections.South.voice_id}\n`,
);
