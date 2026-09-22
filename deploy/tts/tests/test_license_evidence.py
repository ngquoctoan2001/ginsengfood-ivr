from __future__ import annotations

import hashlib
import importlib.util
import io
import json
import os
import shutil
import tempfile
import unittest
from contextlib import redirect_stdout
from copy import deepcopy
from pathlib import Path
from unittest.mock import patch

from shim.license_evidence import LicenseEvidenceError, verify_license_evidence
from shim.model_lock import ModelLockError

TTS_ROOT = Path(os.environ.get("IVR_TTS_TEST_ROOT", Path(__file__).resolve().parents[1]))
if not (TTS_ROOT / "licenses").is_dir():
    TTS_ROOT = Path("/opt/ivr-tts")


class LicenseEvidenceTests(unittest.TestCase):
    def setUp(self) -> None:
        self.root = TTS_ROOT / "licenses"
        self.lock = json.loads((TTS_ROOT / "models/MODELS.lock").read_text(encoding="utf-8"))

    def test_exact_published_evidence_without_standalone_license_passes(self) -> None:
        self.assertTrue(all(x["license_file_sha256"] is None for x in self.lock["artifacts"]))
        self.assertEqual({"documents": 4, "models": 2, "artifacts": 13}, verify_license_evidence(self.lock, self.root))

    def test_shared_negative_cases_fail_even_with_rebound_manifest_hash(self) -> None:
        cases = json.loads((Path(__file__).parent / "fixtures/license-evidence-cases.json").read_text(encoding="utf-8"))
        for test in cases:
            with self.subTest(case=test["id"]), tempfile.TemporaryDirectory() as directory:
                root = Path(directory) / "licenses"
                shutil.copytree(self.root, root)
                candidate = deepcopy(self.lock)
                manifest = json.loads((root / "LICENSES.json").read_text(encoding="utf-8"))
                if "file" in test:
                    if test.get("remove"):
                        (root / test["file"]).unlink()
                    else:
                        (root / test["file"]).write_text(test["value"], encoding="utf-8")
                else:
                    target = candidate if test["target"] == "lock" else manifest
                    for key in test["path"][:-1]:
                        target = target[key]
                    last = test["path"][-1]
                    if test.get("remove"):
                        del target[last]
                    else:
                        target[last] = test["value"]
                    if test["target"] == "manifest":
                        data = (json.dumps(manifest, ensure_ascii=False, indent=2) + "\n").encode()
                        (root / "LICENSES.json").write_bytes(data)
                        candidate["license_evidence"]["sha256"] = hashlib.sha256(data).hexdigest()
                with self.assertRaises(LicenseEvidenceError):
                    verify_license_evidence(candidate, root)

    def test_cli_evidence_does_not_substitute_release_authority(self) -> None:
        path = TTS_ROOT / "scripts/verify-model.py"
        spec = importlib.util.spec_from_file_location("verify_model_test", path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        candidate = deepcopy(self.lock)
        candidate["internal_mirror_gate"] = {"status": "PASS", "decided_by": "TEST_ONLY", "decided_on": "2026-09-22", "approval_reference": "TEST_ONLY"}
        for item in candidate["artifacts"]:
            item["internal_mirror_uri"] = "file:///TEST_ONLY/" + item["bundle_path"]
            item["internal_mirror_digest"] = "sha256:" + item["sha256"]
        argv = [str(path), "--lock", "TEST_ONLY", "--bundle", "TEST_ONLY", "--license-root", str(self.root), "--mode", "production"]
        for authority, passes in [(None, False), ("MODULE_8_OWNER", False), ("LEGAL_PRIVACY", True)]:
            with self.subTest(authority=authority):
                candidate["legal_gate"] = {"status": "PASS", "decision_authority": authority, "decided_by": "TEST_ONLY", "decided_on": "2026-09-22", "approval_reference": "TEST_ONLY"}
                with patch.object(module, "verify_bundle", return_value=candidate), patch("sys.argv", argv), redirect_stdout(io.StringIO()):
                    if passes:
                        self.assertEqual(0, module.main())
                    else:
                        with self.assertRaisesRegex(ModelLockError, "production release approval unavailable"):
                            module.main()

    def test_license_document_symlink_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / "licenses"
            shutil.copytree(self.root, root)
            path = root / "Apache-2.0.txt"
            outside = Path(directory) / "outside-license.txt"
            outside.write_bytes(path.read_bytes())
            path.unlink()
            try:
                path.symlink_to(outside)
            except OSError as error:
                self.skipTest(f"symlink unavailable: {error}")
            with self.assertRaisesRegex(LicenseEvidenceError, "missing or linked"):
                verify_license_evidence(self.lock, root)


if __name__ == "__main__":
    unittest.main()
