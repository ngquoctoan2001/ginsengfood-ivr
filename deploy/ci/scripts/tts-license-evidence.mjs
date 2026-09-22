// Twin of shim/license_evidence.py. This verifies published evidence, not approval.
import { createHash } from "node:crypto";
import { lstatSync, readFileSync } from "node:fs";
import { join } from "node:path";

function requireEvidence(condition, reason) {
  if (!condition) throw new Error(reason);
}

function readDocument(root, name, digest, size) {
  requireEvidence(typeof name === "string" && /^[A-Za-z0-9][A-Za-z0-9_.-]*$/.test(name), "license path invalid");
  requireEvidence(typeof digest === "string" && /^[a-f0-9]{64}$/.test(digest), "license digest invalid");
  const path = join(root, name);
  const stat = lstatSync(path);
  requireEvidence(stat.isFile() && !stat.isSymbolicLink(), "license document missing or linked");
  requireEvidence(stat.size <= 1024 * 1024, "license document too large");
  const data = readFileSync(path);
  if (size !== undefined) requireEvidence(Number.isSafeInteger(size) && size > 0 && size === data.length, "license size mismatch");
  requireEvidence(createHash("sha256").update(data).digest("hex") === digest, "license digest mismatch");
  return data;
}

export function verifyLicenseEvidence(lock, root) {
  const binding = lock.license_evidence;
  requireEvidence(binding && typeof binding === "object" && !Array.isArray(binding), "license evidence binding missing");
  requireEvidence(binding.manifest_path === "LICENSES.json", "license manifest path invalid");
  const manifest = JSON.parse(readDocument(root, binding.manifest_path, binding.sha256).toString("utf8"));
  requireEvidence(manifest?.schema_version === 1, "license manifest schema invalid");
  requireEvidence(manifest.production_approval === false, "license evidence is not release approval");
  const { documents, models } = manifest;
  requireEvidence(Array.isArray(documents) && documents.length > 0 && Array.isArray(models) && models.length > 0, "license evidence empty");
  const byId = new Map(), contents = new Map(), paths = new Set();
  for (const doc of documents) {
    requireEvidence(doc && typeof doc === "object" && !Array.isArray(doc), "license document invalid");
    requireEvidence(typeof doc.id === "string" && doc.id.length > 0 && !byId.has(doc.id), "license document id duplicate or missing");
    requireEvidence(!paths.has(doc.path), "license document path duplicate");
    requireEvidence(typeof doc.source_url === "string" && doc.source_url.startsWith("https://"), "license source URL invalid");
    requireEvidence(Number.isSafeInteger(doc.size_bytes) && doc.size_bytes > 0, "license size invalid");
    contents.set(doc.id, readDocument(root, doc.path, doc.sha256, doc.size_bytes));
    paths.add(doc.path);
    byId.set(doc.id, doc);
  }
  requireEvidence(Array.isArray(lock.artifacts) && lock.artifacts.length > 0, "licensed artifacts empty");
  const groups = new Map(), artifactKeys = new Set();
  for (const item of lock.artifacts) {
    const key = JSON.stringify([item.model_repo, item.full_revision]);
    const artifactKey = JSON.stringify([item.model_repo, item.full_revision, item.allowed_file_path]);
    requireEvidence(!artifactKeys.has(artifactKey), "licensed artifact duplicate");
    artifactKeys.add(artifactKey);
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key).push(item);
  }
  const seenModels = new Set(), seenIds = new Set(), usedDocuments = new Set();
  for (const model of models) {
    requireEvidence(model && typeof model === "object" && !Array.isArray(model), "license model invalid");
    requireEvidence(typeof model.id === "string" && model.id.length > 0 && !seenIds.has(model.id), "license evidence id duplicate or missing");
    seenIds.add(model.id);
    const key = JSON.stringify([model.model_repo, model.full_revision]);
    requireEvidence(groups.has(key) && !seenModels.has(key), "license model revision mismatch or duplicate");
    seenModels.add(key);
    requireEvidence(model.kind === "PUBLISHER_DECLARATION_WITH_REFERENCED_LICENSE" && model.declared_spdx === "Apache-2.0", "license evidence kind or SPDX invalid");
    const declaration = byId.get(model.declaration_document_id), license = byId.get(model.license_document_id);
    requireEvidence(declaration && license, "license document reference missing");
    requireEvidence(declaration.source_url === `https://huggingface.co/${model.model_repo}/resolve/${model.full_revision}/README.md`, "publisher declaration source mismatch");
    requireEvidence(license.source_url === "https://www.apache.org/licenses/LICENSE-2.0.txt", "referenced license source mismatch");
    const cardText = contents.get(declaration.id).toString("utf8");
    requireEvidence(/^---\r?\n(?:(?!---)[\s\S])*?^license: apache-2\.0\s*$/m.test(cardText), "publisher license declaration missing");
    const items = groups.get(key);
    requireEvidence(JSON.stringify(model.artifact_paths) === JSON.stringify(items.map(x => x.allowed_file_path).sort()), "license artifact coverage mismatch");
    const cards = items.filter(x => x.allowed_file_path === "README.md");
    requireEvidence(cards.length === 1 && cards[0].sha256 === declaration.sha256 && cards[0].size_bytes === declaration.size_bytes, "publisher card artifact binding mismatch");
    for (const item of items) {
      requireEvidence(item.license_evidence_id === model.id && item.declared_spdx === "Apache-2.0", "artifact license evidence binding mismatch");
      requireEvidence(item.license_file_sha256 === null, "publisher declaration cannot impersonate standalone license");
    }
    if (model.model_repo === "pnnbao-ump/VieNeu-TTS-v3-Turbo") {
      const preset = model.preset_assets;
      requireEvidence(preset && preset.source_commit === lock.source_commit
        && preset.voice_manifest_sha256 === lock.voice_manifest_sha256
        && typeof preset.voice_manifest_sha256 === "string" && /^[a-f0-9]{64}$/.test(preset.voice_manifest_sha256)
        && preset.commercial_generated_audio === true && preset.speaker_consent_basis === "PUBLISHER_DECLARATION", "preset rights provenance mismatch");
    } else requireEvidence(model.preset_assets === null, "codec cannot assert preset consent");
    usedDocuments.add(declaration.id); usedDocuments.add(license.id);
  }
  requireEvidence(seenModels.size === groups.size, "license model coverage incomplete");
  const supplementary = manifest.supplementary_document_ids;
  requireEvidence(Array.isArray(supplementary) && supplementary.every(x => typeof x === "string")
    && new Set(supplementary).size === supplementary.length, "supplementary document list invalid");
  requireEvidence(supplementary.every(x => byId.has(x) && !usedDocuments.has(x))
    && usedDocuments.size + supplementary.length === byId.size, "license document coverage mismatch");
  return { documents: documents.length, models: models.length, artifacts: lock.artifacts.length };
}
