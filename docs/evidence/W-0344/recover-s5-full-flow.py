#!/usr/bin/env python3
"""Resume the SSH handoff, never a partially executed test. No Docker mutations here."""
import argparse
import datetime as dt
import hashlib
import json
import os
from pathlib import Path
import re
import socket
import subprocess
import sys
import tarfile
import time

RUN = re.compile(r"/home/ssv/ivr-full-flow-w0344-\d{8}-\d{6}-[a-f0-9]{8}")


def digest(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1048576), b""):
            h.update(chunk)
    return h.hexdigest()


def save(path, value):
    temporary = path.with_suffix(path.suffix + ".tmp-" + str(os.getpid()))
    temporary.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    temporary.replace(path)


def validate(root, archive_pin, installer_pin):
    if not RUN.fullmatch(str(root)) or root.is_symlink() or not root.is_dir():
        raise ValueError("Unexpected run directory")
    if socket.gethostname().split(".")[0] != "vps61":
        raise ValueError("Expected vps61")
    for name, expected in (("ivr-full-flow-w0344.tar.gz", archive_pin),
                           ("install-s5-full-flow.sh", installer_pin)):
        if not re.fullmatch(r"[a-f0-9]{64}", expected):
            raise ValueError("Invalid pin")
        path = root / name
        if path.is_symlink() or digest(path) != expected:
            raise ValueError("Checksum mismatch: " + name)
    expected_sum = archive_pin + "  ivr-full-flow-w0344.tar.gz\n"
    if (root / "ivr-full-flow-w0344.tar.gz.sha256").read_text(encoding="ascii") != expected_sum:
        raise ValueError("Archive checksum file differs")


def active_jobs(root, proc=Path("/proc")):
    here, elsewhere = [], []
    names = ("install-s5-full-flow.sh", "full-flow-s5/launcher.py", "run-release-check.py", "run-worker-s5.py")
    for path in proc.iterdir():
        if not path.name.isdigit() or int(path.name) == os.getpid():
            continue
        try:
            command = (path / "cmdline").read_bytes().replace(b"\0", b" ").decode(errors="replace")
            cwd = (path / "cwd").resolve(strict=True)
            ours = "recover-s5-full-flow.py" in command and "--execute" in command
            if not ours and not any(name in command for name in names):
                continue
            (here if cwd == root or root in cwd.parents else elsewhere).append(int(path.name))
        except (OSError, RuntimeError):
            continue
    return here, elsewhere


def classify(root, active, other):
    if active:
        return "RUNNING"
    if (root / "result.tar.gz").is_file():
        return "RECEIPT_READY"
    if (root / "recovery-exit.json").is_file():
        return "FAILED_BEFORE_RECEIPT"
    if any((root / name).exists() for name in ("full-flow-s5", "result", "launcher.log", "recovery-start.json")):
        return "PARTIAL_REVIEW_REQUIRED"
    if other:
        return "OTHER_PROBE_RUNNING"
    return "NOT_STARTED"


def diagnostics(root, status, **extra):
    data = {"work_id": "W-0344", "state": status, "host": socket.gethostname(),
            "remote_directory": str(root), "REAL_CUSTOMER_CALL_ALLOWED": "NO", "production": "BLOCKED",
            "observed_utc": dt.datetime.now(dt.timezone.utc).isoformat(), **extra}
    save(root / "recovery-status.json", data)
    target = root / "recovery-diagnostic.tar.gz"
    # Explicit allowlist; never include compose.private.json, SQL or SIP credentials.
    with tarfile.open(target, "w:gz") as archive:
        for name in ("recovery-status.json", "recovery-start.json", "recovery-exit.json",
                     "recovery-detached.log", "launcher.log"):
            path = root / name
            if path.is_file() and not path.is_symlink():
                archive.add(path, arcname=name, recursive=False)
    Path(str(target) + ".sha256").write_text(digest(target) + "  " + target.name + "\n", encoding="ascii")
    if status == "RECEIPT_READY":
        path = root / "result.tar.gz"
        Path(str(path) + ".sha256").write_text(digest(path) + "  " + path.name + "\n", encoding="ascii")
    print("W0344_RECOVERY_STATE " + status, flush=True)


def start_detached(root, args):
    # The caller holds flock. This exclusive marker closes the startup race before fork.
    marker = root / "recovery-start.json"
    with marker.open("x", encoding="utf-8") as stream:
        json.dump({"started_utc": dt.datetime.now(dt.timezone.utc).isoformat(), "archive_sha256": args.archive_sha256,
                   "installer_sha256": args.installer_sha256, "REAL_CUSTOMER_CALL_ALLOWED": "NO"}, stream)
    command = [sys.executable, "-u", "-B", str(root / "recover-s5-full-flow.py"),
               "--root", str(root), "--archive-sha256", args.archive_sha256,
               "--installer-sha256", args.installer_sha256, "--execute"]
    with (root / "recovery-detached.log").open("ab", buffering=0) as log:
        process = subprocess.Popen(command, cwd=root, stdin=subprocess.DEVNULL, stdout=log,
                                   stderr=subprocess.STDOUT, start_new_session=True, close_fds=True)
    return process.pid


def execute(root, args):
    import fcntl
    with (root / ".recovery-job.lock").open("a") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        code = 1
        try:
            validate(root, args.archive_sha256, args.installer_sha256)
            print("W0344_DETACHED_START", flush=True)
            code = subprocess.run(["bash", "./install-s5-full-flow.sh"], cwd=root,
                                  stdin=subprocess.DEVNULL).returncode
        finally:
            save(root / "recovery-exit.json", {"exit_code": code,
                 "finished_utc": dt.datetime.now(dt.timezone.utc).isoformat()})


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--archive-sha256", required=True)
    parser.add_argument("--installer-sha256", required=True)
    parser.add_argument("--execute", action="store_true")
    args = parser.parse_args()
    root = args.root.absolute()
    if args.execute:
        execute(root, args)
        return 0
    # Only write diagnostics inside the caller's exact, existing, non-linked run directory.
    if not RUN.fullmatch(str(root)) or root.is_symlink() or not root.is_dir():
        raise ValueError("Unexpected run directory")
    try:
        validate(root, args.archive_sha256, args.installer_sha256)
    except (ValueError, OSError) as error:
        diagnostics(root, "PREFLIGHT_FAILED", error=str(error))
        return 13
    import fcntl
    with (root / ".recovery-controller.lock").open("a") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        active, other = active_jobs(root)
        status = classify(root, active, other)
        if status == "NOT_STARTED":
            # A Docker workload left by another diagnostic must also block a second load test.
            names = subprocess.check_output(["docker", "ps", "--format", "{{.Names}}"], text=True).splitlines()
            probes = [n for n in names if n.startswith(("ivr-w0335-", "ivr-w0343-", "ivr-w0344-"))]
            if probes:
                diagnostics(root, "OTHER_PROBE_RUNNING", probe_names=probes)
                return 12
            pid = start_detached(root, args)
            diagnostics(root, "STARTED_DETACHED", pid=pid)
    deadline = time.monotonic() + 3600
    shown = 0
    while True:
        active, other = active_jobs(root)
        status = classify(root, active, other)
        diagnostics(root, status, active_pids=active)
        log = root / "recovery-detached.log"
        if log.is_file():
            lines = log.read_text(encoding="utf-8", errors="replace").splitlines()
            for line in lines[shown:]:
                print(line, flush=True)
            shown = len(lines)
        if status != "RUNNING":
            return 0 if status == "RECEIPT_READY" else 10
        if time.monotonic() >= deadline:
            return 11
        time.sleep(20)


if __name__ == "__main__":
    raise SystemExit(main())
