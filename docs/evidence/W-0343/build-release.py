#!/usr/bin/env python3
"""Build a self-contained, hash-bound handoff from already verified artifacts."""
import hashlib
import json
from pathlib import Path
import shutil
import tarfile

repo = Path(__file__).resolve().parents[3]
art = repo / ".artifacts/W-0343"
release = art / "vieneu-w0343"
if release.exists() and any(release.iterdir()):
    raise ValueError("Release directory is not empty; do not overwrite an existing handoff")
release.mkdir(parents=True, exist_ok=True)
base_old = repo / ".artifacts/W-0333/vieneu-s5"
kit_old = repo / ".artifacts/W-0338/vieneu-deadline-s5-final"


def digest(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1048576), b""):
            h.update(chunk)
    return h.hexdigest()


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def save(path, value):
    path.write_bytes((json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8"))


def copy(source, relative, expected=None):
    if expected is not None and digest(source) != expected:
        raise ValueError("Source changed: " + str(source))
    target = release / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, target)


verified = read(repo / "docs/evidence/W-0342/verification.json")
bindings = {x["path"]: "".join(x["sha256_hex_blocks"]) for x in verified["bindings"]}
for relative, expected in bindings.items():
    historical = repo / relative
    if digest(historical) != expected:
        historical = art / "baseline" / relative
    if not historical.is_file() or digest(historical) != expected:
        raise ValueError("W-0342 evidence binding changed: " + relative)
candidate = read(art / "candidate.json")
for relative, expected in candidate["source_bindings"].items():
    if digest(repo / relative) != expected:
        raise ValueError("Final candidate source binding changed: " + relative)
index = candidate["image_index"]
config = candidate["image_config"]
base = read(base_old / "manifest.json")
for name, expected in base["files"].items():
    if name not in {"tts-image.tar", "voice-acceptance-manifest.json"}:
        copy(base_old / name, "base/" + name, expected)
copy(art / "tts-image.tar", "base/tts-image.tar", candidate["archive_sha256"])
copy(repo / "docs/evidence/W-0343/voice-acceptance-manifest.json", "base/voice-acceptance-manifest.json")
base.update(work_id="W-0343", image_index=index, image_config=config,
            scope="S5_CANDIDATE_DEPLOYMENT_CHECK_NOT_PRODUCTION_APPROVAL")
base["files"] = {p.relative_to(release / "base").as_posix(): digest(p)
                 for p in sorted((release / "base").rglob("*")) if p.is_file()}
save(release / "base/manifest.json", base)
kit = read(kit_old / "manifest.json")
for name, expected in kit["files"].items():
    copy(kit_old / name, "kit/" + name, expected)
kit.update(work_id="W-0343", base_manifest_sha256=digest(release / "base/manifest.json"))
save(release / "kit/manifest.json", kit)
for name in ("run-release-check.py", "release-decision.md", "legal-approval.json"):
    copy(Path(__file__).parent / name, name)
for name in ("LICENSES.json", "Apache-2.0.txt", "vieneu-model-card.md", "moss-onnx-model-card.md", "MOSS-source-LICENSE", "README.md"):
    copy(repo / "deploy/tts/licenses" / name, "licenses/" + name)
for relative, source in {
    "metadata/MODELS.lock": repo / "deploy/tts/models/MODELS.lock",
    "metadata/voices.json": repo / "deploy/tts/shim/voices.json",
    "metadata/THIRD_PARTY_NOTICES.md": repo / "deploy/tts/THIRD_PARTY_NOTICES.md",
    "metadata/W0342-verification.json": repo / "docs/evidence/W-0342/verification.json",
    "metadata/final-candidate.json": art / "candidate.json",
    "metadata/W0341-source-verification.json": repo / "docs/evidence/W-0341/source-verification.json",
    "metadata/S2-owner-review.md": repo / "docs/evidence/W-0340/S2-owner-review.md",
    "security/tts-trivy.json": art / "scan/trivy.json",
    "security/db-metadata.json": repo / ".artifacts/W-0340/db-metadata.json",
}.items():
    copy(source, relative)
scan = read(release / "security/tts-trivy.json")
if scan["Metadata"]["ImageID"] != config or any(x.get("Vulnerabilities") for x in scan["Results"]):
    raise ValueError("Image scan mismatch or findings changed")
catalog = {"work_id": "W-0343", "release": "vieneu-w0343", "production": "BLOCKED",
    "REAL_CUSTOMER_CALL_ALLOWED": "NO", "candidate_approval": "S5_VALIDATION_AUTHORIZED",
    "legal_release_decision": "PASS_OWNER_AUTHORIZED_LEGAL_PRIVACY",
    "tts_image_index": index, "tts_image_config": config,
    "profile": {"tts_cpus": 2, "tts_memory_gib": 4, "capacity": 1, "ort_threads": 1,
                "segment_ms": 30000, "queue_ms": 90000, "preparation_ms": 120000, "queue_limit": 8},
    "worker_probe": {"reference": "W-0338", "original_manifest_sha256": digest(kit_old / "manifest.json"),
                     "all_payload_bytes_unchanged": True, "not_full_scheduler_or_SIP": True},
    "scan_reference": "W-0343 final image; same current database downloaded during W-0340",
    "files": {p.relative_to(release).as_posix(): {"bytes": p.stat().st_size, "sha256": digest(p)}
              for p in sorted(release.rglob("*")) if p.is_file()}}
save(release / "catalog.json", catalog)
lines = [digest(p) + "  " + p.relative_to(release).as_posix()
         for p in sorted(release.rglob("*"), key=lambda p: p.relative_to(release).as_posix()) if p.is_file()]
(release / "SHA256SUMS").write_bytes(("\n".join(lines) + "\n").encode("ascii"))
archive = art / "vieneu-release-w0343.tar.gz"
with tarfile.open(archive, "w:gz", compresslevel=4) as tar:
    for path in sorted(release.rglob("*"), key=lambda p: p.relative_to(release).as_posix()):
        if path.is_file():
            tar.add(path, arcname=release.name + "/" + path.relative_to(release).as_posix(), recursive=False)
summary = {"archive": archive.name, "archive_bytes": archive.stat().st_size,
           "archive_sha256": digest(archive), "catalog_sha256": digest(release / "catalog.json"),
           "files": len(catalog["files"]), "tts_image_index": index, "tts_image_config": config,
           "profile": catalog["profile"], "REAL_CUSTOMER_CALL_ALLOWED": "NO", "production": "BLOCKED"}
save(art / "package.json", summary)
print(json.dumps(summary, indent=2))
