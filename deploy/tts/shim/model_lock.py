from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any


class ModelLockError(RuntimeError):
    """The on-disk artifact bundle does not match the exact allowlist."""


def load_lock(lock_path: Path) -> dict[str, Any]:
    try:
        data = json.loads(lock_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        raise ModelLockError("model lock unavailable") from error
    if data.get("schema_version") != 1 or not isinstance(data.get("artifacts"), list):
        raise ModelLockError("unsupported model lock")
    return data


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def verify_bundle(lock_path: Path, bundle_root: Path, *, reject_extra: bool = True) -> dict[str, Any]:
    lock = load_lock(lock_path)
    root = bundle_root.resolve(strict=True)
    expected: set[str] = set()

    for artifact in lock["artifacts"]:
        relative = artifact.get("bundle_path")
        if not isinstance(relative, str) or not relative or Path(relative).is_absolute():
            raise ModelLockError("invalid bundle path")
        normalized = Path(relative).as_posix()
        if normalized in expected:
            raise ModelLockError("duplicate bundle path")
        expected.add(normalized)

        candidate = (root / relative).resolve(strict=False)
        try:
            candidate.relative_to(root)
        except ValueError as error:
            raise ModelLockError("bundle path escapes root") from error
        if not candidate.is_file() or candidate.is_symlink():
            raise ModelLockError("required artifact missing")
        if candidate.stat().st_size != artifact.get("size_bytes"):
            raise ModelLockError("artifact size mismatch")
        if sha256_file(candidate) != artifact.get("sha256"):
            raise ModelLockError("artifact digest mismatch")

    if reject_extra:
        actual = {
            path.relative_to(root).as_posix()
            for path in root.rglob("*")
            if path.is_file()
        }
        if actual != expected:
            raise ModelLockError("artifact allowlist mismatch")

    return lock

