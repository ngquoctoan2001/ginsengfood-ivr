#!/usr/bin/env node
import { createHash } from "node:crypto";
import { readFileSync, statSync } from "node:fs";
import { resolve } from "node:path";

const repoRoot = resolve(import.meta.dirname, "../../..");
const lockPath = resolve(repoRoot, "deploy/tts/models/MODELS.lock");
const lock = JSON.parse(readFileSync(lockPath, "utf8"));
const selftest = process.argv.includes("--selftest");
const expectedArtifactSetSha256 = "84eeb006ab695c875707cdb3ed56409468ab8394ddf25bbf11409e08539bfd94";
const expectedRuntimeLockSha256 = "a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304";
const expectedVoiceConfigSha256 = "9a76fdabca3ad58994caa1b59c0c76f3a98facb22f11e2f9ec9210a9371ccae2";
const expectedAcceptanceTemplateSha256 = "c70230fa2b3c20007c78286e150b5b83f718c3b3c1da2e354dc98887eb7951f6";
const artifactFingerprintFields = [
  "component", "runtime_required", "model_repo", "full_revision", "allowed_file_path",
  "bundle_path", "size_bytes", "sha256", "declared_spdx", "license_file_sha256",
  "voice_manifest_sha256", "dependency_lock_sha256", "internal_mirror_uri",
  "internal_mirror_digest",
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

function validate(candidate) {
  if (candidate.schema_version !== 1 || !Array.isArray(candidate.artifacts)) {
    throw new Error("invalid lock schema");
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
      "dependency_lock_sha256", "internal_mirror_uri", "internal_mirror_digest",
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

validate(lock);
validateSupportingFiles();
if (selftest) {
  expectFailure("revision", value => { value.artifacts[0].full_revision = "main"; });
  expectFailure("path", value => { value.artifacts[0].bundle_path = value.artifacts[1].bundle_path; });
  expectFailure("hash", value => { value.artifacts[0].sha256 = "0".repeat(64); });
  expectFailure("license", value => { value.artifacts[0].declared_spdx = "CC-BY-NC-4.0"; });
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
}

const blockers = [];
if (lock.legal_gate?.status !== "PASS") blockers.push("LEGAL");
if (lock.internal_mirror_gate?.status !== "PASS") blockers.push("INTERNAL_MIRROR");
process.stdout.write(
  `TTS_PROVENANCE_STRUCTURE_PASS artifacts=${lock.artifacts.length} release_blockers=${blockers.join(",") || "NONE"}\n`,
);
