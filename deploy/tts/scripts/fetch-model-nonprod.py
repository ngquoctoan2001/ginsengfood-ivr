#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import os
import sys
import urllib.parse
import urllib.request
from pathlib import Path

ACK = "I_UNDERSTAND_PUBLIC_FETCH_IS_NONPROD"


def main() -> int:
    parser = argparse.ArgumentParser(description="Fetch the exact W-0122 public artifacts in nonprod")
    parser.add_argument("--lock", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--nonprod-ack", required=True)
    args = parser.parse_args()

    if args.nonprod_ack != ACK:
        parser.error(f"--nonprod-ack must equal {ACK}")
    if os.environ.get("IVR_EXECUTION_MODE", "MOCK") not in {"MOCK", "LAB_REAL_SIM"}:
        parser.error("public fetch is forbidden outside MOCK/LAB_REAL_SIM")

    lock = json.loads(args.lock.read_text(encoding="utf-8"))
    root = args.output.resolve()
    if root == Path(root.anchor) or root == Path.home().resolve():
        parser.error("refusing broad output directory")
    root.mkdir(parents=True, exist_ok=True)

    for artifact in lock["artifacts"]:
        destination = (root / artifact["bundle_path"]).resolve()
        try:
            destination.relative_to(root)
        except ValueError:
            parser.error("bundle path escapes output root")
        destination.parent.mkdir(parents=True, exist_ok=True)

        if destination.is_file() and verify_file(destination, artifact):
            print(f"MODEL_FETCH_SKIP path={artifact['bundle_path']}")
            continue

        quoted_path = "/".join(
            urllib.parse.quote(part, safe="") for part in artifact["allowed_file_path"].split("/")
        )
        url = (
            f"https://huggingface.co/{artifact['model_repo']}/resolve/"
            f"{artifact['full_revision']}/{quoted_path}?download=true"
        )
        partial = destination.with_name(destination.name + ".partial")
        request = urllib.request.Request(url, headers={"User-Agent": "ginsengfood-ivr-w0122/1"})
        digest = hashlib.sha256()
        count = 0
        try:
            with urllib.request.urlopen(request, timeout=60) as response, partial.open("wb") as output:
                while True:
                    chunk = response.read(1024 * 1024)
                    if not chunk:
                        break
                    output.write(chunk)
                    digest.update(chunk)
                    count += len(chunk)
            if count != artifact["size_bytes"] or digest.hexdigest() != artifact["sha256"]:
                raise RuntimeError("downloaded artifact does not match lock")
            partial.replace(destination)
        finally:
            if partial.exists():
                partial.unlink()
        print(f"MODEL_FETCH_OK path={artifact['bundle_path']} bytes={count}")

    expected = {item["bundle_path"] for item in lock["artifacts"]}
    actual = {
        path.relative_to(root).as_posix()
        for path in root.rglob("*")
        if path.is_file()
    }
    if actual != expected:
        raise RuntimeError("output contains missing or extra artifacts")
    print(f"MODEL_FETCH_COMPLETE files={len(expected)} public_fetch=NONPROD_ONLY")
    return 0


def verify_file(path: Path, artifact: dict[str, object]) -> bool:
    if path.stat().st_size != artifact["size_bytes"]:
        return False
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest() == artifact["sha256"]


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:
        print(f"MODEL_FETCH_FAILED reason={type(error).__name__}", file=sys.stderr)
        raise SystemExit(1)

