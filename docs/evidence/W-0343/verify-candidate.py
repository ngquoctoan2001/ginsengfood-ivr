"""Bind the owner-approved metadata, exact archive contents and scanner ImageID."""
import datetime as dt
import hashlib
import io
import json
from pathlib import Path
import tarfile

repo = Path(__file__).resolve().parents[3]
art = repo / ".artifacts/W-0343"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


lockpath = repo / "deploy/tts/models/MODELS.lock"
lock = read(lockpath)
old = read(art / "baseline/deploy/tts/models/MODELS.lock")
assert {k: v for k, v in lock.items() if k not in {"status", "legal_gate"}} == {
    k: v for k, v in old.items() if k not in {"status", "legal_gate"}}
approval = repo / "docs/evidence/W-0343/legal-approval.json"
assert lock["legal_gate"]["approval_sha256"] == sha(approval)
assert lock["legal_gate"]["decision_authority"] == "LEGAL_PRIVACY"
assert read(approval)["statement_verbatim"] == "Tôi có thẩm quyền cho phép kiêm nhiệm Legal/Privacy và chấp thuận phiếu này"
assert read(approval)["production"] == "BLOCKED" and read(approval)["REAL_CUSTOMER_CALL_ALLOWED"] == "NO"
voicepath = repo / "deploy/tts/shim/voices.json"
voices = read(voicepath)
oldvoices = read(art / "baseline/deploy/tts/shim/voices.json")
assert {k: v for k, v in voices.items() if k != "model_lock_sha256"} == {
    k: v for k, v in oldvoices.items() if k != "model_lock_sha256"}
assert voices["model_lock_sha256"] == sha(lockpath)
acceptance = repo / "docs/evidence/W-0343/voice-acceptance-manifest.json"
accepted = read(acceptance)
original = read(repo / "docs/evidence/W-0122/voice-acceptance-manifest.json")
assert {k: v for k, v in accepted.items() if k not in {"model_lock_sha256", "notes"}} == {
    k: v for k, v in original.items() if k not in {"model_lock_sha256", "notes"}}
assert accepted["model_lock_sha256"] == sha(lockpath)
image = read(art / "image-inspect.json")[0]["Id"]
sources = {"opt/ivr-tts/models/MODELS.lock": lockpath,
           "opt/ivr-tts/scripts/verify-model.py": repo / "deploy/tts/scripts/verify-model.py"}
for path in (repo / "deploy/tts/shim").glob("*.py"):
    sources["opt/ivr-tts/shim/" + path.name] = path
sources["opt/ivr-tts/shim/voices.json"] = voicepath
for path in (repo / "deploy/tts/licenses").iterdir():
    if path.is_file():
        sources["opt/ivr-tts/licenses/" + path.name] = path
found = {}
with tarfile.open(art / "tts-image.tar") as archive:
    descriptor = json.load(archive.extractfile("index.json"))["manifests"][0]
    assert descriptor["digest"] == image
    while True:
        raw = archive.extractfile("blobs/sha256/" + descriptor["digest"][7:]).read()
        assert hashlib.sha256(raw).hexdigest() == descriptor["digest"][7:]
        node = json.loads(raw)
        if "manifests" not in node:
            config = node["config"]["digest"]
            break
        descriptor = node["manifests"][0]
    assert hashlib.sha256(archive.extractfile("blobs/sha256/" + config[7:]).read()).hexdigest() == config[7:]
    manifest = json.load(archive.extractfile("manifest.json"))[0]
    assert manifest["Config"] == "blobs/sha256/" + config[7:]
    for layer in manifest["Layers"][-4:]:
        with tarfile.open(fileobj=io.BytesIO(archive.extractfile(layer).read())) as contents:
            for member in contents:
                name = member.name.removeprefix("./")
                if member.isfile() and name in sources:
                    found[name] = hashlib.sha256(contents.extractfile(member).read()).hexdigest()
assert found == {name: sha(path) for name, path in sources.items()}
scan = read(art / "scan/trivy.json")
db = read(repo / ".artifacts/W-0340/db-metadata.json")
assert dt.datetime.fromisoformat(db["NextUpdate"].replace("Z", "+00:00")) > dt.datetime.now(dt.timezone.utc)
assert scan["Metadata"]["ImageID"] == config
assert sum(len(x.get("Vulnerabilities", [])) for x in scan["Results"]) == 0
bindings = [*sources.values(), approval, acceptance, repo / "docs/evidence/W-0122/voice-acceptance-manifest.template.json"]
candidate = {"image_index": image, "image_config": config, "archive_sha256": sha(art / "tts-image.tar"),
    "source_bindings": {p.relative_to(repo).as_posix(): sha(p) for p in bindings},
    "image_files_exact": len(found), "model_artifacts_unchanged": 13, "listening_decision_preserved": True,
    "legal_gate": "PASS_OWNER_AUTHORIZED_LEGAL_PRIVACY", "production": "BLOCKED", "REAL_CUSTOMER_CALL_ALLOWED": "NO",
    "scan": {"vulnerabilities": 0, "metadata_image_id_matches_config": True, "database": db,
             "downloaded_this_turn": False, "report_sha256": sha(art / "scan/trivy.json")}}
(art / "candidate.json").write_text(json.dumps(candidate, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")
print("FINAL_CANDIDATE_VERIFIED", image, "CVE=0 image_files=" + str(len(found)))
