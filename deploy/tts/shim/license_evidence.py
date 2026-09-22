"""Verify published licensing evidence; this never grants release approval."""
from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from .model_lock import ModelLockError


class LicenseEvidenceError(ModelLockError):
    """A publisher declaration, license text or artifact binding is invalid."""


def _require(condition: Any, reason: str) -> None:
    if not condition:
        raise LicenseEvidenceError(reason)


def _read(root: Path, name: Any, digest: Any, size: Any = None) -> bytes:
    _require(isinstance(name, str) and re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]*", name), "license path invalid")
    _require(isinstance(digest, str) and re.fullmatch(r"[a-f0-9]{64}", digest), "license digest invalid")
    path = root / name
    _require(not path.is_symlink() and path.is_file(), "license document missing or linked")
    _require(path.stat().st_size <= 1024 * 1024, "license document too large")
    data = path.read_bytes()
    if size is not None:
        _require(type(size) is int and size > 0 and size == len(data), "license size mismatch")
    _require(hashlib.sha256(data).hexdigest() == digest, "license digest mismatch")
    return data


def verify_license_evidence(lock: dict[str, Any], root: Path) -> dict[str, int]:
    """Validate the exact evidence bound by the caller's trusted model lock, offline.

    CI additionally pins the manifest digest outside the lock. A model-card declaration
    and its referenced license replace the old filename-only requirement; neither this
    function nor a valid manifest satisfies Legal/Privacy or mirror approval.
    """
    try:
        binding = lock.get("license_evidence")
        _require(isinstance(binding, dict), "license evidence binding missing")
        _require(binding.get("manifest_path") == "LICENSES.json", "license manifest path invalid")
        raw = _read(root, binding["manifest_path"], binding.get("sha256"))
        manifest = json.loads(raw)
        _require(isinstance(manifest, dict) and type(manifest.get("schema_version")) is int
                 and manifest["schema_version"] == 1, "license manifest schema invalid")
        _require(manifest.get("production_approval") is False, "license evidence is not release approval")
        documents, models = manifest.get("documents"), manifest.get("models")
        _require(isinstance(documents, list) and documents and isinstance(models, list) and models,
                 "license evidence empty")
        by_id, contents, paths = {}, {}, set()
        for doc in documents:
            _require(isinstance(doc, dict), "license document invalid")
            ident = doc.get("id")
            _require(isinstance(ident, str) and ident and ident not in by_id, "license document id duplicate or missing")
            _require(doc.get("path") not in paths, "license document path duplicate")
            _require(isinstance(doc.get("source_url"), str) and doc["source_url"].startswith("https://"),
                     "license source URL invalid")
            _require(type(doc.get("size_bytes")) is int and doc["size_bytes"] > 0, "license size invalid")
            contents[ident] = _read(root, doc.get("path"), doc.get("sha256"), doc["size_bytes"])
            paths.add(doc["path"])
            by_id[ident] = doc

        artifacts = lock.get("artifacts")
        _require(isinstance(artifacts, list) and artifacts, "licensed artifacts empty")
        groups, artifact_keys = {}, set()
        for item in artifacts:
            key = (item["model_repo"], item["full_revision"])
            artifact_key = (*key, item["allowed_file_path"])
            _require(artifact_key not in artifact_keys, "licensed artifact duplicate")
            artifact_keys.add(artifact_key)
            groups.setdefault(key, []).append(item)
        seen_models, seen_ids, used_documents = set(), set(), set()
        for model in models:
            _require(isinstance(model, dict), "license model invalid")
            ident = model.get("id")
            _require(isinstance(ident, str) and ident and ident not in seen_ids, "license evidence id duplicate or missing")
            seen_ids.add(ident)
            key = (model.get("model_repo"), model.get("full_revision"))
            _require(key in groups and key not in seen_models, "license model revision mismatch or duplicate")
            seen_models.add(key)
            _require(model.get("kind") == "PUBLISHER_DECLARATION_WITH_REFERENCED_LICENSE"
                     and model.get("declared_spdx") == "Apache-2.0", "license evidence kind or SPDX invalid")
            declaration = by_id.get(model.get("declaration_document_id"))
            license_doc = by_id.get(model.get("license_document_id"))
            _require(declaration is not None and license_doc is not None, "license document reference missing")
            expected_url = f"https://huggingface.co/{key[0]}/resolve/{key[1]}/README.md"
            _require(declaration["source_url"] == expected_url, "publisher declaration source mismatch")
            _require(license_doc["source_url"] == "https://www.apache.org/licenses/LICENSE-2.0.txt",
                     "referenced license source mismatch")
            card_text = contents[declaration["id"]].decode("utf-8")
            _require(re.match(r"\A---\r?\n(?:(?!---).)*?^license: apache-2\.0\s*$", card_text, re.S | re.M),
                     "publisher license declaration missing")
            items = groups[key]
            _require(model.get("artifact_paths") == sorted(x["allowed_file_path"] for x in items),
                     "license artifact coverage mismatch")
            cards = [x for x in items if x["allowed_file_path"] == "README.md"]
            _require(len(cards) == 1 and cards[0]["sha256"] == declaration["sha256"]
                     and cards[0]["size_bytes"] == declaration["size_bytes"], "publisher card artifact binding mismatch")
            for item in items:
                _require(item.get("license_evidence_id") == ident and item.get("declared_spdx") == "Apache-2.0",
                         "artifact license evidence binding mismatch")
                # Honest absence of a standalone upstream LICENSE; no borrowed hash.
                _require("license_file_sha256" in item and item["license_file_sha256"] is None,
                         "publisher declaration cannot impersonate standalone license")
            if key[0] == "pnnbao-ump/VieNeu-TTS-v3-Turbo":
                preset = model.get("preset_assets")
                _require(isinstance(preset, dict) and preset.get("source_commit") == lock.get("source_commit")
                         and preset.get("voice_manifest_sha256") == lock.get("voice_manifest_sha256")
                         and isinstance(preset.get("voice_manifest_sha256"), str)
                         and re.fullmatch(r"[a-f0-9]{64}", preset["voice_manifest_sha256"])
                         and preset.get("commercial_generated_audio") is True
                         and preset.get("speaker_consent_basis") == "PUBLISHER_DECLARATION",
                         "preset rights provenance mismatch")
            else:
                _require(model.get("preset_assets") is None, "codec cannot assert preset consent")
            used_documents.update([declaration["id"], license_doc["id"]])
        _require(seen_models == set(groups), "license model coverage incomplete")
        supplementary = manifest.get("supplementary_document_ids")
        _require(isinstance(supplementary, list) and all(isinstance(x, str) for x in supplementary)
                 and len(set(supplementary)) == len(supplementary), "supplementary document list invalid")
        _require(set(supplementary).isdisjoint(used_documents)
                 and used_documents | set(supplementary) == set(by_id), "license document coverage mismatch")
        return {"documents": len(documents), "models": len(models), "artifacts": len(artifacts)}
    except LicenseEvidenceError:
        raise
    except (OSError, ValueError, KeyError, TypeError, UnicodeError) as error:
        raise LicenseEvidenceError("license evidence malformed or unavailable") from error
