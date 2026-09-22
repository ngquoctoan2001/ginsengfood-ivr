#!/usr/bin/env python3
"""Verify and install only the versioned offline candidate, then return all probe evidence."""
import datetime as dt
import fcntl
import hashlib
import json
import os
from pathlib import Path
import shutil
import socket
import subprocess
import sys
import tarfile
import tempfile

ARCHIVE_SHA256 = "668f02f753b490cf29fe1d9ffd73f481f23db514deeb3ffa389923353008d3ff"
CATALOG_SHA256 = "39de4a043c02f4cf318c242eda5fc450315922f8ea52d54c77b625da78019b3a"

if socket.gethostname().split(".")[0] != "vps61":
    raise SystemExit("Wrong host: vps61 required")
os.umask(0o077)
task_home = Path.home()
archive = task_home / "vieneu-release-w0343.tar.gz"
parent = task_home / "ivr-artifact-mirror/releases"
parent.mkdir(parents=True, exist_ok=True)
lock = (parent / ".w0343-check.lock").open("a")
fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
digest = hashlib.sha256()
with archive.open("rb") as stream:
    for chunk in iter(lambda: stream.read(1048576), b""):
        digest.update(chunk)
if digest.hexdigest() != ARCHIVE_SHA256:
    raise SystemExit("Archive checksum mismatch")
release = parent / "vieneu-w0343"
if not release.exists():
    staging = Path(tempfile.mkdtemp(prefix=".w0343-stage-", dir=parent))
    with tarfile.open(archive, "r:gz") as tar:
        members = tar.getmembers()
        names = set()
        for member in members:
            name = member.name
            if (not member.isfile() or "\\" in name or name in names or
                    not name.startswith("vieneu-w0343/") or
                    any(part in ("", ".", "..") for part in name.split("/"))):
                raise ValueError("Unsafe archive member")
            names.add(name)
            target = (staging / name).resolve()
            target.relative_to(staging.resolve())
        for member in members:
            target = staging / member.name
            target.parent.mkdir(parents=True, exist_ok=True)
            with tar.extractfile(member) as source, target.open("xb") as destination:
                shutil.copyfileobj(source, destination)
    (staging / "vieneu-w0343").rename(release)
    staging.rmdir()
if release.is_symlink():
    raise ValueError("Symlink release is not allowed")
if hashlib.sha256((release / "catalog.json").read_bytes()).hexdigest() != CATALOG_SHA256:
    raise ValueError("Existing release differs from this handoff")
catalog = json.loads((release / "catalog.json").read_text(encoding="utf-8"))
checker = release / "run-release-check.py"
if hashlib.sha256(checker.read_bytes()).hexdigest() != catalog["files"]["run-release-check.py"]["sha256"]:
    raise ValueError("Release checker checksum mismatch")
stamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d-%H%M%S")
output = task_home / ("vieneu-release-w0343-result-" + stamp)
returned = task_home / "vieneu-release-w0343-result.tar.gz"
if returned.exists():
    raise SystemExit("A previous receipt exists; download and rename it before rerunning")
rc = 1
try:
    print("MIRROR_INSTALLED " + str(release), flush=True)
    result = subprocess.run([sys.executable, "-B", str(checker), "--catalog-sha256", CATALOG_SHA256,
                             "--expected-host", "vps61", "--output", str(output)], timeout=3000)
    rc = result.returncode
finally:
    output.mkdir(parents=True, exist_ok=True)
    (output / "handoff.json").write_text(json.dumps({
        "archive_sha256": ARCHIVE_SHA256, "catalog_sha256": CATALOG_SHA256,
        "probe_exit_code": rc, "host": socket.gethostname(), "production": "BLOCKED",
        "REAL_CUSTOMER_CALL_ALLOWED": "NO", "old_mirror_release_preserved":
        (parent / "vieneu-w0340/catalog.json").is_file()}, indent=2) + "\n", encoding="utf-8")
    shutil.copyfile(release / "catalog.json", output / "catalog.json")
    with tarfile.open(returned, "w:gz") as tar:
        tar.add(output, arcname="result")
    h = hashlib.sha256(returned.read_bytes()).hexdigest()
    Path(str(returned) + ".sha256").write_bytes((h + "  " + returned.name + "\n").encode("ascii"))
    print("RESULT_READY " + str(returned) + " exit=" + str(rc), flush=True)
sys.exit(rc)
