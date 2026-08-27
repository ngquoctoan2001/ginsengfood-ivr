#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

SCRIPT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPT_ROOT))

from shim.model_lock import ModelLockError, verify_bundle  # noqa: E402


def main() -> int:
    parser = argparse.ArgumentParser(description="Verify a W-0122 model bundle")
    parser.add_argument("--lock", type=Path, required=True)
    parser.add_argument("--bundle", type=Path, required=True)
    parser.add_argument("--mode", choices=("nonprod", "production"), default="production")
    args = parser.parse_args()

    lock = verify_bundle(args.lock, args.bundle)
    if args.mode == "production":
        if any(item.get("license_file_sha256") is None for item in lock["artifacts"]):
            raise ModelLockError("production requires license-file evidence")
        if any(
            not item.get("internal_mirror_uri") or not item.get("internal_mirror_digest")
            for item in lock["artifacts"]
        ):
            raise ModelLockError("production requires an exact internal mirror")
    blockers = []
    if lock.get("legal_gate", {}).get("status") != "PASS":
        blockers.append("LEGAL")
    if lock.get("internal_mirror_gate", {}).get("status") != "PASS":
        blockers.append("INTERNAL_MIRROR")
    print(
        "MODEL_VERIFY_PASS "
        f"mode={args.mode} files={len(lock['artifacts'])} release_blockers={','.join(blockers) or 'NONE'}"
    )
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:
        print(f"MODEL_VERIFY_FAILED reason={type(error).__name__}", file=sys.stderr)
        raise SystemExit(1)

