"""Deployment boundary checks: valid catalog hashes must not admit unsafe profiles."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location("release_check", Path(__file__).with_name("run-release-check.py"))
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class ReleaseGuards(unittest.TestCase):
    def test_rehashed_unsafe_catalogs_fail(self):
        catalog = {"release": "vieneu-w0343", "production": "BLOCKED", "REAL_CUSTOMER_CALL_ALLOWED": "NO",
                   "profile": copy.deepcopy(check.PROFILE), "files": {}}
        mutations = [("REAL_CUSTOMER_CALL_ALLOWED", "YES"), ("production", "APPROVED")]
        mutations += [("profile." + key, value + 1) for key, value in check.PROFILE.items()]
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            def materialize(value):
                check.write(root / "catalog.json", value)
                (root / "SHA256SUMS").write_bytes((check.sha(root / "catalog.json") + "  catalog.json\n").encode())
                return check.sha(root / "catalog.json")
            check.verify_release(root, materialize(catalog))
            for field, value in mutations:
                changed = copy.deepcopy(catalog)
                parts = field.split(".")
                if len(parts) == 2:
                    changed[parts[0]][parts[1]] = value
                else:
                    changed[field] = value
                with self.subTest(field=field), self.assertRaisesRegex(ValueError, "scope/profile"):
                    check.verify_release(root, materialize(changed))
            pinned = materialize(catalog)
            (root / "unexpected.py").write_text("payload", encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "file set"):
                check.verify_release(root, pinned)

    def test_actual_container_must_remain_isolated(self):
        info = {"Image": "sha256:test", "Config": {"User": "1000:1000", "Env": [
            "REAL_CUSTOMER_CALL_ALLOWED=NO", "VIE_NEU_MAX_CONCURRENCY=1", "VIE_NEU_ORT_THREADS=1"]},
            "HostConfig": {"NanoCpus": 2000000000, "Memory": 4294967296, "MemorySwap": 4294967296,
                           "NetworkMode": "none", "ReadonlyRootfs": True, "Privileged": False,
                           "PortBindings": {}}}
        with patch.object(check.subprocess, "check_output", return_value=json.dumps([info])):
            check.inspect_probe(["docker"], "only-test", "sha256:test")
        for field, value in [("NetworkMode", "bridge"), ("NanoCpus", 3000000000),
                             ("Memory", 8589934592), ("MemorySwap", -1), ("Privileged", True),
                             ("ReadonlyRootfs", False), ("PortBindings", {"8090/tcp": []})]:
            changed = copy.deepcopy(info)
            changed["HostConfig"][field] = value
            with self.subTest(field=field), patch.object(check.subprocess, "check_output", return_value=json.dumps([changed])):
                with self.assertRaisesRegex(ValueError, "guard mismatch"):
                    check.inspect_probe(["docker"], "only-test", "sha256:test")
        for field, value in [("User", "0:0"), ("Env", [])]:
            changed = copy.deepcopy(info)
            changed["Config"][field] = value
            with self.subTest(field=field), patch.object(check.subprocess, "check_output", return_value=json.dumps([changed])):
                with self.assertRaisesRegex(ValueError, "guard mismatch"):
                    check.inspect_probe(["docker"], "only-test", "sha256:test")
        with patch.object(check.subprocess, "check_output", return_value=json.dumps([info])):
            with self.assertRaisesRegex(ValueError, "guard mismatch"):
                check.inspect_probe(["docker"], "only-test", "sha256:another-image")


if __name__ == "__main__":
    unittest.main()
