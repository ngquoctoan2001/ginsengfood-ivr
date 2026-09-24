#!/usr/bin/env node
import { createHash } from "node:crypto";
import { readFileSync, statSync } from "node:fs";
import { resolve } from "node:path";
import { verifyLicenseEvidence } from "./tts-license-evidence.mjs";
import { runLicenseEvidenceSelftest } from "./tts-license-evidence-selftest.mjs";

const repoRoot = resolve(import.meta.dirname, "../../..");
const lockPath = resolve(repoRoot, "deploy/tts/models/MODELS.lock");
const lock = JSON.parse(readFileSync(lockPath, "utf8"));
const selftest = process.argv.includes("--selftest");
// W-0342: bind the W-0340 verified mirror and W-0341 publisher evidence. Changes require
// review and re-pinning the artifact set, license manifest and W-0126 lock/voice chain.
const expectedLicenseManifestSha256 = "cbcc81c26d262d84dfca4d2b8e7d372c6c3d6ae21795ec56672a04adfdbc2e6c";
const expectedArtifactSetSha256 = "09ecbec246276f75fda0c74a9ee395d0f9739fe50618c6c11c87830cab95ca8b";
const expectedRuntimeLockSha256 = "a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304";
const expectedVoiceConfigSha256 = "e6f95f9c1d794dbda981ef133de8db290840e6aa29d28515f6c3dcca4831b74f";
const expectedAcceptanceTemplateSha256 = "7099e55dc87ba3d7774b44646b328eb010234bbbf33a0aa581da75404655a3d0";
const artifactFingerprintFields = [
  "component", "runtime_required", "model_repo", "full_revision", "allowed_file_path",
  "bundle_path", "size_bytes", "sha256", "declared_spdx", "license_file_sha256",
  "voice_manifest_sha256", "dependency_lock_sha256", "internal_mirror_uri",
  "internal_mirror_digest", "license_evidence_id",
];

const allowedRepos = new Map([
  ["pnnbao-ump/VieNeu-TTS-v3-Turbo", "2da0efab622a1722125991736524f080b751ef5b"],
  ["OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX", "ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae"],
]);
const allowedArtifactKeys = new Set([
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:README.md",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/config.json",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/tokenizer.json",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/vieneu_acoustic_cached.onnx",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/vieneu_backbone_shared.data",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/vieneu_decode_step.onnx",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/vieneu_prefill.onnx",
  "pnnbao-ump/VieNeu-TTS-v3-Turbo:onnx_int8/vieneu_v3_heads.npz",
  "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX:README.md",
  "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX:codec_browser_onnx_meta.json",
  "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX:moss_audio_tokenizer_decode_full.onnx",
  "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX:moss_audio_tokenizer_decode_shared.data",
  "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX:moss_audio_tokenizer_decode_step.onnx",
]);

function hasLegalPrivacyApproval(gate) {
  return gate?.status === "PASS"
    && gate?.decision_authority === "LEGAL_PRIVACY"
    && typeof gate?.decided_by === "string"
    && gate.decided_by.trim().length > 0
    && typeof gate?.approval_reference === "string"
    && gate.approval_reference.trim().length > 0
    && typeof gate?.decided_on === "string"
    && /^\d{4}-\d{2}-\d{2}$/.test(gate.decided_on);
}

// The twin of hasLegalPrivacyApproval, and until now only one twin had been taught the
// 2a4f45d lesson. `legal_gate` cannot be opened by writing PASS: validate() throws unless a
// LEGAL_PRIVACY authority, a named signer, a reference and a date are all present, and the
// `legal-authority` mutation proves a MODULE_8_OWNER self-signature is refused.
// `internal_mirror_gate` was still the exact shape legal_gate had on 2026-08-28 -- a single
// status string with nothing behind it. Writing {"status":"PASS"} dropped INTERNAL_MIRROR
// from the blocker list while all 13 artifacts kept internal_mirror_uri: null.
//
// Two deliberate choices here.
//
// No authority token is prescribed. LEGAL_PRIVACY exists because the module owner must not be
// able to sign solely as MODULE_8_OWNER. W-0343 records an explicit company-authorized
// dual-role Legal/Privacy decision; the authority guard still rejects MODULE_8_OWNER. This gate is different: the lock's own reason says an
// "owner-approved internal artifact or OCI mirror URI/digest" is what is missing, so owner
// approval IS the right authority, and naming some other one would be inventing governance
// nobody has decided. What is demanded is the record -- who, when, against what reference.
//
// The artifact check is not new policy either. verify-model.py already refuses to release
// without an exact mirror on every artifact; it just does so under `--mode production`, and
// CI runs this Node gate instead, so CI never saw the rule. Folding it into what PASS *means*
// puts the same requirement in front of both readers.
//
// TWIN: deploy/tts/scripts/verify-model.py has_internal_mirror_approval. Change both together.
// They cannot share code across the language boundary, so they share cases instead:
// deploy/tts/tests/fixtures/release-approval-cases.json, replayed here by runReleaseApprovalCases
// under --selftest and against verify-model.py by test_release_approval_parity.py, which the
// container self-test runs inside the image (verify-model.py has shipped there since W-0342).
// Cases marked python_drift are where the Python copy is still looser; they stay marked until
// the next TTS candidate hoists that copy into shim/model_lock.py.
function hasExactInternalMirror(item) {
  return typeof item?.internal_mirror_uri === "string"
    && item.internal_mirror_uri.trim().length > 0
    && typeof item?.internal_mirror_digest === "string"
    && /^(sha256:)?[a-f0-9]{64}$/.test(item.internal_mirror_digest);
}

function hasInternalMirrorApproval(gate, artifacts) {
  return gate?.status === "PASS"
    && typeof gate?.decided_by === "string"
    && gate.decided_by.trim().length > 0
    && typeof gate?.approval_reference === "string"
    && gate.approval_reference.trim().length > 0
    && typeof gate?.decided_on === "string"
    && /^\d{4}-\d{2}-\d{2}$/.test(gate.decided_on)
    // A mirror gate that passes while the mirrors are null is decorative.
    && Array.isArray(artifacts)
    && artifacts.length > 0
    && artifacts.every(hasExactInternalMirror);
}

function validate(candidate) {
  if (candidate.schema_version !== 1 || !Array.isArray(candidate.artifacts)) {
    throw new Error("invalid lock schema");
  }
  if (candidate.license_evidence?.sha256 !== expectedLicenseManifestSha256) {
    throw new Error("license evidence manifest pin drift");
  }
  verifyLicenseEvidence(candidate, resolve(repoRoot, "deploy/tts/licenses"));
  if (candidate.legal_gate?.status === "PASS" && !hasLegalPrivacyApproval(candidate.legal_gate)) {
    throw new Error("legal approval authority invalid");
  }
  if (candidate.internal_mirror_gate?.status === "PASS"
    && !hasInternalMirrorApproval(candidate.internal_mirror_gate, candidate.artifacts)) {
    throw new Error("internal mirror approval invalid");
  }
  if (candidate.source_commit !== "36c4b501b0634a8f59805e6b529a058fbd30190b") {
    throw new Error("source revision drift");
  }
  const seenPaths = new Set();
  const seenArtifacts = new Set();
  for (const item of candidate.artifacts) {
    const fields = [
      "model_repo", "full_revision", "allowed_file_path", "bundle_path", "size_bytes",
      "sha256", "declared_spdx", "license_file_sha256", "voice_manifest_sha256",
      "dependency_lock_sha256", "internal_mirror_uri", "internal_mirror_digest", "license_evidence_id",
    ];
    for (const field of fields) {
      if (!(field in item)) throw new Error(`missing field ${field}`);
    }
    if (!allowedRepos.has(item.model_repo) || allowedRepos.get(item.model_repo) !== item.full_revision) {
      throw new Error("repository or revision not allowlisted");
    }
    const artifactKey = `${item.model_repo}:${item.allowed_file_path}`;
    if (!allowedArtifactKeys.has(artifactKey) || seenArtifacts.has(artifactKey)) {
      throw new Error("artifact not in exact allowlist");
    }
    seenArtifacts.add(artifactKey);
    if (item.model_repo.includes("0.3B-q4-gguf") || item.declared_spdx !== "Apache-2.0") {
      throw new Error("license allowlist rejected");
    }
    if (!Number.isSafeInteger(item.size_bytes) || item.size_bytes <= 0) {
      throw new Error("invalid artifact size");
    }
    if (!/^[a-f0-9]{64}$/.test(item.sha256)) throw new Error("invalid artifact digest");
    if (seenPaths.has(item.bundle_path)) throw new Error("duplicate artifact path");
    seenPaths.add(item.bundle_path);
  }
  if (seenArtifacts.size !== allowedArtifactKeys.size) throw new Error("allowlisted artifact missing");
  const artifactSet = candidate.artifacts.map(item => Object.fromEntries(
    artifactFingerprintFields.map(field => [field, item[field]]),
  ));
  const artifactSetSha256 = createHash("sha256")
    .update(JSON.stringify(artifactSet))
    .digest("hex");
  if (artifactSetSha256 !== expectedArtifactSetSha256) {
    throw new Error("artifact provenance fingerprint drift");
  }

  const voiceHash = sha256(resolve(repoRoot, "third_party/vieneu-tts/src/vieneu/assets/voices_v3_turbo.json"));
  const dependencyHash = sha256(resolve(repoRoot, "third_party/vieneu-tts/uv.lock"));
  const runtimeLockHash = sha256(resolve(repoRoot, "deploy/tts/runtime-requirements.lock"));
  const licenseHash = sha256(resolve(repoRoot, "third_party/vieneu-tts/LICENSE"));
  if (voiceHash !== candidate.voice_manifest_sha256) throw new Error("voice manifest drift");
  if (dependencyHash !== candidate.dependency_lock_sha256) throw new Error("dependency lock drift");
  if (runtimeLockHash !== expectedRuntimeLockSha256) throw new Error("runtime lock drift");
  if (licenseHash !== "c71d239df91726fc519c6eb72d318ec65820627232b2f796219e87dcf35d0ab4") {
    throw new Error("source license drift");
  }
  for (const item of candidate.artifacts) {
    if (item.voice_manifest_sha256 !== voiceHash || item.dependency_lock_sha256 !== dependencyHash) {
      throw new Error("artifact provenance binding drift");
    }
  }
}

function validateSupportingFiles(
  expectedVoiceConfig = expectedVoiceConfigSha256,
  expectedAcceptanceTemplate = expectedAcceptanceTemplateSha256,
) {
  const voiceConfigHash = sha256(resolve(repoRoot, "deploy/tts/shim/voices.json"));
  const acceptanceTemplateHash = sha256(
    resolve(repoRoot, "docs/evidence/W-0122/voice-acceptance-manifest.template.json"),
  );
  if (voiceConfigHash !== expectedVoiceConfig) throw new Error("voice config drift");
  if (acceptanceTemplateHash !== expectedAcceptanceTemplate) {
    throw new Error("acceptance template drift");
  }
}

function sha256(path) {
  if (!statSync(path).isFile()) throw new Error("required source artifact missing");
  return createHash("sha256").update(readFileSync(path)).digest("hex");
}

function expectFailure(name, mutate) {
  const candidate = structuredClone(lock);
  mutate(candidate);
  try {
    validate(candidate);
  } catch {
    process.stdout.write(`TTS_PROVENANCE_MUTATION_PASS mutation=${name}\n`);
    return;
  }
  throw new Error(`mutation was not rejected: ${name}`);
}

// W-0225 follow-up. Every case must get its `expected` verdict from these predicates; the file's
// _note says who else replays it. Mismatches are collected so one run names all of them.
function runReleaseApprovalCases() {
  const predicates = new Map([
    ["legal_privacy_approval", hasLegalPrivacyApproval],
    ["exact_internal_mirror", hasExactInternalMirror],
    ["internal_mirror_approval", hasInternalMirrorApproval],
  ]);
  const { cases } = JSON.parse(readFileSync(
    resolve(repoRoot, "deploy/tts/tests/fixtures/release-approval-cases.json"),
    "utf8",
  ));
  if (!Array.isArray(cases) || cases.length === 0) throw new Error("release approval cases missing");
  const ids = new Set();
  const verdicts = new Set();
  const mismatches = [];
  for (const test of cases) {
    const predicate = predicates.get(test?.predicate);
    const wellFormed = typeof test?.id === "string"
      && !ids.has(test.id)
      && typeof predicate === "function"
      && Array.isArray(test.args)
      && test.args.length === predicate.length
      && typeof test.expected === "boolean"
      && (!("python_drift" in test)
        || (typeof test.python_drift === "string" && test.python_drift.trim().length > 0));
    if (!wellFormed) throw new Error(`release approval case malformed: ${test?.id}`);
    ids.add(test.id);
    verdicts.add(`${test.predicate}:${test.expected}`);
    const verdict = predicate(...test.args);
    if (verdict !== test.expected) mismatches.push(`${test.id} expected=${test.expected} got=${verdict}`);
  }
  // A rule that refuses everything passes every negative case, so each rule must also accept one.
  for (const name of predicates.keys()) {
    if (!verdicts.has(`${name}:true`) || !verdicts.has(`${name}:false`)) {
      throw new Error(`release approval cases need an accepted and a refused case for ${name}`);
    }
  }
  if (mismatches.length > 0) throw new Error(`release approval case mismatch: ${mismatches.join("; ")}`);
  const drift = cases.filter(test => "python_drift" in test).length;
  process.stdout.write(`TTS_RELEASE_APPROVAL_CASES_PASS cases=${cases.length} python_drift=${drift}\n`);
}

validate(lock);
validateSupportingFiles();
if (selftest) {
  runLicenseEvidenceSelftest(repoRoot);
  expectFailure("license-evidence-manifest-pin", value => { value.license_evidence.sha256 = "0".repeat(64); });
  expectFailure("revision", value => { value.artifacts[0].full_revision = "main"; });
  expectFailure("path", value => { value.artifacts[0].bundle_path = value.artifacts[1].bundle_path; });
  expectFailure("hash", value => { value.artifacts[0].sha256 = "0".repeat(64); });
  expectFailure("license", value => { value.artifacts[0].declared_spdx = "CC-BY-NC-4.0"; });
  expectFailure("legal-authority", value => {
    value.legal_gate = {
      status: "PASS",
      decided_on: "2026-08-29",
      decided_by: "Owner module IVR",
      decision_authority: "MODULE_8_OWNER",
      approval_reference: "owner-only-is-not-legal-approval",
    };
  });
  // The hole this gate had until now: three words, and INTERNAL_MIRROR left the blocker list
  // while every internal_mirror_uri in the lock stayed null.
  expectFailure("mirror-bare-pass", value => { value.internal_mirror_gate = { status: "PASS" }; });
  // And the subtler half: a complete decision record still is not a mirror.
  expectFailure("mirror-without-artifacts", value => {
    for (const item of value.artifacts) {
      item.internal_mirror_uri = null;
      item.internal_mirror_digest = null;
    }
    value.internal_mirror_gate = {
      status: "PASS",
      decided_on: "2026-09-08",
      decided_by: "TEST_ONLY signer",
      decision_authority: "TEST_ONLY",
      approval_reference: "TEST_ONLY:record-without-mirrors",
    };
  });
  expectFailure("extra", value => { value.artifacts.push({ ...value.artifacts[0], bundle_path: "extra.bin" }); });
  try {
    validateSupportingFiles("0".repeat(64), expectedAcceptanceTemplateSha256);
    throw new Error("voice config mutation was not rejected");
  } catch (error) {
    if (error.message === "voice config mutation was not rejected") throw error;
    process.stdout.write("TTS_PROVENANCE_MUTATION_PASS mutation=voice-config\n");
  }
  try {
    validateSupportingFiles(expectedVoiceConfigSha256, "0".repeat(64));
    throw new Error("acceptance template mutation was not rejected");
  } catch (error) {
    if (error.message === "acceptance template mutation was not rejected") throw error;
    process.stdout.write("TTS_PROVENANCE_MUTATION_PASS mutation=acceptance-template\n");
  }
  if (!hasLegalPrivacyApproval({
    ...lock.legal_gate,
    status: "PASS",
    decided_on: "2026-08-29",
    decided_by: "Legal/Privacy test fixture",
    decision_authority: "LEGAL_PRIVACY",
    approval_reference: "OD-VOICE-07:test-only-positive-fixture",
  })) {
    throw new Error("legal/privacy authority fixture was rejected");
  }
  // Positive counterpart: a fully supplied mirror IS accepted, so the rule above is a gate and
  // not a wall. Whoever fills the real values will also have to re-pin -- see the note on
  // expectedArtifactSetSha256.
  if (!hasInternalMirrorApproval(
    {
      status: "PASS",
      decided_on: "2026-09-08",
      decided_by: "Platform test fixture",
      decision_authority: "TEST_ONLY",
      approval_reference: "TEST_ONLY:mirror-positive-fixture",
    },
    lock.artifacts.map(item => ({
      ...item,
      internal_mirror_uri: "oci://registry.invalid/ivr/tts",
      internal_mirror_digest: `sha256:${"0".repeat(64)}`,
    })),
  )) {
    throw new Error("internal mirror positive fixture was rejected");
  }
  runReleaseApprovalCases();
}

const blockers = [];
if (!hasLegalPrivacyApproval(lock.legal_gate)) blockers.push("LEGAL");
if (!hasInternalMirrorApproval(lock.internal_mirror_gate, lock.artifacts)) {
  blockers.push("INTERNAL_MIRROR");
}
process.stdout.write(
  `TTS_PROVENANCE_STRUCTURE_PASS artifacts=${lock.artifacts.length} license_evidence=PASS release_blockers=${blockers.join(",") || "NONE"}\n`,
);
