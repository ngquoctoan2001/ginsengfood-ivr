import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";

const REGIONS = ["North", "Central", "South"];
const LISTENING_PROFILE_ID = "w0122-asterisk-microsip-8khz-v1";
const TOP_LEVEL_KEYS = [
  "schema_version", "work_id", "status", "stale_relisten_required", "source_commit",
  "model_artifacts", "voice_manifest_sha256", "dependency_lock_sha256",
  "runtime_lock_sha256", "model_lock_sha256", "audition_script_sha256",
  "audition_manifest_sha256", "audition_renderer_sha256", "listening_profile_id",
  "listening_route", "listener",
  "listened_at", "device_and_lab_route", "approval_reference",
  "all_11_candidates_listened", "selections", "candidate_results", "notes",
];

export function loadVoiceAcceptanceContext(repoRoot) {
  const voiceConfig = readJson(resolve(repoRoot, "deploy/tts/shim/voices.json"));
  const modelLockPath = resolve(repoRoot, "deploy/tts/models/MODELS.lock");
  const auditionManifestPath = resolve(repoRoot, "docs/evidence/W-0122/audition-manifest.json");
  const auditionManifest = readJson(auditionManifestPath);
  const modelArtifacts = [...new Map(
    readJson(modelLockPath).artifacts.map(item => [
      item.model_repo,
      { repo: item.model_repo, revision: item.full_revision },
    ]),
  ).values()];
  return {
    voiceConfig,
    roster: voiceConfig.voices,
    modelArtifacts,
    expected: {
      source_commit: voiceConfig.source_commit,
      voice_manifest_sha256: sha256File(resolve(repoRoot, "third_party/vieneu-tts/src/vieneu/assets/voices_v3_turbo.json")),
      dependency_lock_sha256: sha256File(resolve(repoRoot, "third_party/vieneu-tts/uv.lock")),
      runtime_lock_sha256: sha256File(resolve(repoRoot, "deploy/tts/runtime-requirements.lock")),
      model_lock_sha256: sha256File(modelLockPath),
      audition_script_sha256: auditionManifest.script_sha256,
      audition_manifest_sha256: sha256File(auditionManifestPath),
      audition_renderer_sha256: sha256File(
        resolve(repoRoot, "deploy/ci/scripts/render-voice-audition.mjs"),
      ),
      listening_profile_id: LISTENING_PROFILE_ID,
    },
  };
}

export function validateVoiceAcceptance(candidate, context) {
  exactKeys(candidate, TOP_LEVEL_KEYS, "acceptance manifest");
  assert(candidate.schema_version === 1 && candidate.work_id === "W-0122", "acceptance identity drift");
  assert(candidate.status === "OWNER_ACCEPTED", "owner acceptance status missing");
  assert(candidate.stale_relisten_required === false, "voice acceptance is stale");
  assert(candidate.source_commit === context.expected.source_commit, "source commit drift");
  assert(JSON.stringify(candidate.model_artifacts) === JSON.stringify(context.modelArtifacts), "model artifact set drift");
  for (const field of [
    "voice_manifest_sha256", "dependency_lock_sha256", "runtime_lock_sha256",
    "model_lock_sha256", "audition_script_sha256", "audition_manifest_sha256",
    "audition_renderer_sha256",
    "listening_profile_id",
  ]) {
    assert(candidate[field] === context.expected[field], `${field} drift`);
  }
  assert(candidate.listening_route === "ASTERISK_MICROSIP_8KHZ", "listening route drift");
  requiredText(candidate.listener, "listener", 200);
  requiredText(candidate.device_and_lab_route, "device_and_lab_route", 500);
  requiredText(candidate.approval_reference, "approval_reference", 500);
  optionalText(candidate.notes, "notes", 2000);
  assert(
    typeof candidate.listened_at === "string"
      && /(?:Z|[+-]\d{2}:\d{2})$/.test(candidate.listened_at)
      && Number.isFinite(Date.parse(candidate.listened_at)),
    "listened_at must be RFC3339 with timezone",
  );
  assert(candidate.all_11_candidates_listened === true, "all 11 candidates were not heard");
  exactKeys(candidate.selections, REGIONS, "selections");

  const rosterById = new Map(context.roster.map(item => [item.voice_id, item]));
  assert(rosterById.size === 11, "candidate roster drift");
  const selections = {};
  const selectedIds = new Set();
  for (const region of REGIONS) {
    const selection = candidate.selections[region];
    exactKeys(selection, ["voice_id", "preset", "speaking_rate", "owner_notes"], `${region} selection`);
    const rosterItem = rosterById.get(selection.voice_id);
    assert(rosterItem?.region === region, `${region} selection outside candidate region`);
    assert(selection.preset === rosterItem.preset, `${region} preset drift`);
    assert(
      typeof selection.speaking_rate === "number"
        && Number.isFinite(selection.speaking_rate)
        && selection.speaking_rate === rosterItem.speaking_rate,
      `${region} speaking rate drift`,
    );
    optionalText(selection.owner_notes, `${region} owner_notes`, 1000);
    assert(!selectedIds.has(selection.voice_id), "regional selections must be distinct");
    selectedIds.add(selection.voice_id);
    selections[region] = selection;
  }

  assert(Array.isArray(candidate.candidate_results) && candidate.candidate_results.length === 11, "candidate_results must be 11");
  const resultIds = new Set();
  for (const [index, result] of candidate.candidate_results.entries()) {
    exactKeys(result, ["voice_id", "region", "listened", "verdict", "notes"], `candidate_results[${index}]`);
    const rosterItem = context.roster[index];
    assert(result.voice_id === rosterItem.voice_id && result.region === rosterItem.region, "candidate result roster/order drift");
    assert(!resultIds.has(result.voice_id), "duplicate candidate result");
    resultIds.add(result.voice_id);
    assert(result.listened === true, `${result.voice_id} was not listened`);
    const selected = selectedIds.has(result.voice_id);
    assert(
      selected ? result.verdict === "SELECTED" : ["NOT_SELECTED", "REJECTED"].includes(result.verdict),
      `${result.voice_id} verdict does not match selections`,
    );
    optionalText(result.notes, `${result.voice_id} notes`, 1000);
  }
  assert(resultIds.size === rosterById.size, "candidate result set drift");
  return selections;
}

export function validateVoiceAcceptanceFile(path, repoRoot) {
  const raw = readFileSync(resolve(path));
  const acceptance = JSON.parse(raw.toString("utf8"));
  const context = loadVoiceAcceptanceContext(repoRoot);
  const selections = validateVoiceAcceptance(acceptance, context);
  return { acceptance, context, selections, raw, sha256: sha256(raw) };
}

export function createTestOnlyAcceptance(context) {
  const selectedByRegion = {
    North: context.roster.find(item => item.region === "North"),
    Central: context.roster.find(item => item.region === "Central"),
    South: context.roster.find(item => item.region === "South"),
  };
  const selectedIds = new Set(Object.values(selectedByRegion).map(item => item.voice_id));
  return {
    schema_version: 1,
    work_id: "W-0122",
    status: "OWNER_ACCEPTED",
    stale_relisten_required: false,
    source_commit: context.expected.source_commit,
    model_artifacts: context.modelArtifacts,
    voice_manifest_sha256: context.expected.voice_manifest_sha256,
    dependency_lock_sha256: context.expected.dependency_lock_sha256,
    runtime_lock_sha256: context.expected.runtime_lock_sha256,
    model_lock_sha256: context.expected.model_lock_sha256,
    audition_script_sha256: context.expected.audition_script_sha256,
    audition_manifest_sha256: context.expected.audition_manifest_sha256,
    audition_renderer_sha256: context.expected.audition_renderer_sha256,
    listening_profile_id: context.expected.listening_profile_id,
    listening_route: "ASTERISK_MICROSIP_8KHZ",
    listener: "TEST_ONLY_OWNER_FIXTURE",
    listened_at: "2026-08-27T00:00:00+07:00",
    device_and_lab_route: "TEST_ONLY_ASTERISK_MICROSIP_8KHZ",
    approval_reference: "TEST_ONLY_NO_AUTHORITY",
    all_11_candidates_listened: true,
    selections: Object.fromEntries(REGIONS.map(region => {
      const item = selectedByRegion[region];
      return [region, {
        voice_id: item.voice_id,
        preset: item.preset,
        speaking_rate: item.speaking_rate,
        owner_notes: "TEST_ONLY",
      }];
    })),
    candidate_results: context.roster.map(item => ({
      voice_id: item.voice_id,
      region: item.region,
      listened: true,
      verdict: selectedIds.has(item.voice_id) ? "SELECTED" : "NOT_SELECTED",
      notes: "TEST_ONLY",
    })),
    notes: "TEST_ONLY fixture; never an Owner approval.",
  };
}

function exactKeys(value, expected, label) {
  assert(value && typeof value === "object" && !Array.isArray(value), `${label} must be an object`);
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  assert(JSON.stringify(actual) === JSON.stringify(wanted), `${label} keys drift`);
}

function requiredText(value, label, limit) {
  assert(typeof value === "string" && value === value.trim() && value.length > 0 && value.length <= limit, `${label} invalid`);
  assert(!/[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f]/.test(value), `${label} contains control characters`);
}

function optionalText(value, label, limit) {
  if (value === null) return;
  assert(typeof value === "string" && value.length <= limit, `${label} invalid`);
  assert(!/[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f]/.test(value), `${label} contains control characters`);
}

function readJson(path) {
  return JSON.parse(readFileSync(path, "utf8"));
}

function sha256File(path) {
  return sha256(readFileSync(path));
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function assert(condition, message) {
  if (!condition) throw new Error(message);
}
