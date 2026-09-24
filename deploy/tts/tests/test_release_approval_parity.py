"""Parity between the two copies of the TTS release-approval rules.

deploy/ci/scripts/tts-provenance-gate.mjs (CI and the gate sweep) and scripts/verify-model.py
(shipped in the image, run by an operator with --mode production) carry the same three
predicates. They cannot share code across the language boundary, so they share cases:
fixtures/release-approval-cases.json. The Node gate replays every case under --selftest; this
module replays them against verify-model.py, locally and inside the image through
tts-container-selftest.mjs.

A case marked `python_drift` is one where verify-model.py disagrees with the Node gate today. Each
runs as its own expected failure, so the suite stays green while every drift stays listed by name.
verify-model.py is byte-pinned by the W-0343 candidate, so the fix waits for the next TTS
candidate. When it lands, delete the markers in the same change: a marked case that starts passing
is an unexpected success, and an unexpected success fails the run.
"""
from __future__ import annotations

import importlib.util
import json
import os
import re
import unittest
from pathlib import Path
from types import ModuleType

TTS_ROOT = Path(os.environ.get("IVR_TTS_TEST_ROOT", Path(__file__).resolve().parents[1]))
if not (TTS_ROOT / "scripts/verify-model.py").is_file():
    TTS_ROOT = Path("/opt/ivr-tts")

CASES = json.loads(
    (Path(__file__).parent / "fixtures/release-approval-cases.json").read_text(encoding="utf-8")
)["cases"]
PREDICATES = {
    "legal_privacy_approval": ("has_legal_privacy_approval", 1),
    "exact_internal_mirror": ("has_exact_internal_mirror", 1),
    "internal_mirror_approval": ("has_internal_mirror_approval", 2),
}


def load_verify_model() -> ModuleType:
    path = TTS_ROOT / "scripts/verify-model.py"
    spec = importlib.util.spec_from_file_location("verify_model_parity", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class ReleaseApprovalParityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.verify_model = load_verify_model()

    def verdict(self, case: dict) -> object:
        name, _ = PREDICATES[case["predicate"]]
        return getattr(self.verify_model, name)(*case["args"])

    def test_case_file_is_well_formed(self) -> None:
        ids = [case["id"] for case in CASES]
        self.assertEqual(len(ids), len(set(ids)), "duplicate case id")
        for case in CASES:
            with self.subTest(case=case["id"]):
                self.assertIn(case["predicate"], PREDICATES)
                self.assertEqual(PREDICATES[case["predicate"]][1], len(case["args"]))
                self.assertIsInstance(case["expected"], bool)
                if "python_drift" in case:
                    self.assertTrue(case["python_drift"].strip())
        # A rule that refuses everything passes every negative case, so each rule must also
        # accept one -- and here that has to hold among the cases Python already agrees on.
        for predicate in PREDICATES:
            with self.subTest(predicate=predicate):
                agreed = {c["expected"] for c in CASES if c["predicate"] == predicate and "python_drift" not in c}
                self.assertEqual({True, False}, agreed)

    def test_agreed_cases_give_the_node_verdict(self) -> None:
        for case in CASES:
            if "python_drift" not in case:
                with self.subTest(case=case["id"]):
                    self.assertIs(case["expected"], self.verdict(case))


def _drift_test(case: dict):
    @unittest.expectedFailure
    def test(self: ReleaseApprovalParityTests) -> None:
        self.assertIs(case["expected"], self.verdict(case))

    test.__doc__ = f"{case['id']}: {case['python_drift']}"
    return test


for _case in CASES:
    if "python_drift" in _case:
        _name = "test_python_drift_" + re.sub(r"[^0-9a-z]+", "_", _case["id"].lower()).strip("_")
        if hasattr(ReleaseApprovalParityTests, _name):
            raise RuntimeError(f"two drift cases map to {_name}")
        setattr(ReleaseApprovalParityTests, _name, _drift_test(_case))


if __name__ == "__main__":
    unittest.main()
