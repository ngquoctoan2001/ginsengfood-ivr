#!/usr/bin/env python3
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path
from typing import Any

SCRIPT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPT_ROOT))

from shim.model_lock import ModelLockError, verify_bundle  # noqa: E402


def has_legal_privacy_approval(gate: Any) -> bool:
    return (
        isinstance(gate, dict)
        and gate.get("status") == "PASS"
        and gate.get("decision_authority") == "LEGAL_PRIVACY"
        and isinstance(gate.get("decided_by"), str)
        and bool(gate["decided_by"].strip())
        and isinstance(gate.get("approval_reference"), str)
        and bool(gate["approval_reference"].strip())
        and isinstance(gate.get("decided_on"), str)
        and len(gate["decided_on"]) == 10
        and gate["decided_on"][4:5] == "-"
        and gate["decided_on"][7:8] == "-"
    )


_MIRROR_DIGEST = re.compile(r"^(sha256:)?[a-f0-9]{64}$")


def has_exact_internal_mirror(item: Any) -> bool:
    if not isinstance(item, dict):
        return False
    uri = item.get("internal_mirror_uri")
    digest = item.get("internal_mirror_digest")
    return (
        isinstance(uri, str)
        and bool(uri.strip())
        and isinstance(digest, str)
        and _MIRROR_DIGEST.match(digest) is not None
    )


def has_internal_mirror_approval(gate: Any, artifacts: Any) -> bool:
    # Twin of has_legal_privacy_approval, and the same rule the Node gate enforces. Kept
    # byte-for-byte equivalent on purpose: this rule already lived in two places once, one
    # of them looser, which is how the mirror gate stayed openable by a bare status string
    # while every internal_mirror_uri was null. See tts-provenance-gate.mjs for why no
    # authority token is prescribed.
    return (
        isinstance(gate, dict)
        and gate.get("status") == "PASS"
        and isinstance(gate.get("decided_by"), str)
        and bool(gate["decided_by"].strip())
        and isinstance(gate.get("approval_reference"), str)
        and bool(gate["approval_reference"].strip())
        and isinstance(gate.get("decided_on"), str)
        and len(gate["decided_on"]) == 10
        and gate["decided_on"][4:5] == "-"
        and gate["decided_on"][7:8] == "-"
        and isinstance(artifacts, list)
        and len(artifacts) > 0
        and all(has_exact_internal_mirror(item) for item in artifacts)
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Verify a W-0122 model bundle")
    parser.add_argument("--lock", type=Path, required=True)
    parser.add_argument("--bundle", type=Path, required=True)
    parser.add_argument("--mode", choices=("nonprod", "production"), default="production")
    args = parser.parse_args()

    lock = verify_bundle(args.lock, args.bundle)
    blockers = []
    if not has_legal_privacy_approval(lock.get("legal_gate")):
        blockers.append("LEGAL")
    if not has_internal_mirror_approval(lock.get("internal_mirror_gate"), lock.get("artifacts")):
        blockers.append("INTERNAL_MIRROR")

    if args.mode == "production":
        if any(item.get("license_file_sha256") is None for item in lock["artifacts"]):
            raise ModelLockError("production requires license-file evidence")
        if any(not has_exact_internal_mirror(item) for item in lock["artifacts"]):
            raise ModelLockError("production requires an exact internal mirror")
        if blockers:
            raise ModelLockError("production release approval unavailable")
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
