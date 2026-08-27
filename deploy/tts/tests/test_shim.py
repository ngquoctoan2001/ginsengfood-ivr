from __future__ import annotations

import hashlib
import json
import math
import os
import tempfile
import unittest
from copy import deepcopy
from pathlib import Path
from unittest.mock import patch

import numpy as np

from shim.acceptance import VoiceAcceptanceError, validate_voice_acceptance
from shim.backend import BackendError, DeterministicTestBackend, _split_text
from shim.convert import ConversionError, float32_to_l16
from shim.model_lock import ModelLockError, verify_bundle
from shim.server import RequestError, _parse_request


class ModelLockTests(unittest.TestCase):
    def test_exact_bundle_passes_and_extra_file_fails(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            payload = b"locked-model-artifact"
            (root / "model.bin").write_bytes(payload)
            lock = {
                "schema_version": 1,
                "artifacts": [{
                    "bundle_path": "model.bin",
                    "size_bytes": len(payload),
                    "sha256": hashlib.sha256(payload).hexdigest(),
                }],
            }
            lock_path = root / "MODELS.lock"
            lock_path.write_text(json.dumps(lock), encoding="utf-8")
            verify_bundle(lock_path, root, reject_extra=False)
            (root / "extra.bin").write_bytes(b"not-allowlisted")
            with self.assertRaises(ModelLockError):
                verify_bundle(lock_path, root)

    def test_path_escape_and_digest_drift_fail(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            lock_path = root / "MODELS.lock"
            lock_path.write_text(json.dumps({
                "schema_version": 1,
                "artifacts": [{
                    "bundle_path": "../escape.bin",
                    "size_bytes": 1,
                    "sha256": "0" * 64,
                }],
            }), encoding="utf-8")
            with self.assertRaises(ModelLockError):
                verify_bundle(lock_path, root)

            payload = b"a"
            (root / "model.bin").write_bytes(payload)
            lock_path.write_text(json.dumps({
                "schema_version": 1,
                "artifacts": [{
                    "bundle_path": "model.bin",
                    "size_bytes": 1,
                    "sha256": "f" * 64,
                }],
            }), encoding="utf-8")
            with self.assertRaises(ModelLockError):
                verify_bundle(lock_path, root, reject_extra=False)


class ConversionTests(unittest.TestCase):
    def test_sine_is_headerless_l16_8khz(self) -> None:
        timeline = np.arange(12_000, dtype=np.float32) / 48_000
        source = 0.2 * np.sin(2 * math.pi * 440 * timeline)
        pcm = float32_to_l16(source)
        self.assertEqual(4_000, len(pcm))
        self.assertNotEqual(b"RIFF", pcm[:4])

    def test_stereo_is_downmixed(self) -> None:
        source = np.zeros((4_800, 2), dtype=np.float32)
        self.assertEqual(1_600, len(float32_to_l16(source)))

    def test_nonfinite_duration_and_contract_drift_fail(self) -> None:
        with self.assertRaises(ConversionError):
            float32_to_l16([0.0, float("nan")])
        with self.assertRaises(ConversionError):
            float32_to_l16(np.zeros(48_001, dtype=np.float32), max_duration_seconds=1)
        with self.assertRaises(ConversionError):
            float32_to_l16([0.0], target_rate=16_000)


class RequestContractTests(unittest.TestCase):
    def setUp(self) -> None:
        self.valid = {
            "text": "Xin chào.",
            "voice_id": "voice-1",
            "locale": "vi-VN",
            "speaking_rate": 1.0,
            "output_format": "audio/L16",
            "sample_rate": 8_000,
        }

    def parse(self, candidate: object) -> dict[str, object]:
        return _parse_request(
            json.dumps(candidate, ensure_ascii=False).encode(), 1_200, ("voice-1",)
        )

    def test_exact_contract_passes(self) -> None:
        parsed = self.parse(self.valid)
        self.assertEqual("Xin chào.", parsed["text"])

    def test_extra_field_and_invalid_values_fail(self) -> None:
        mutations = [
            {**self.valid, "extra": True},
            {**self.valid, "text": " "},
            {**self.valid, "voice_id": "unknown"},
            {**self.valid, "locale": "en-US"},
            {**self.valid, "output_format": "audio/wav"},
            {**self.valid, "sample_rate": True},
            {**self.valid, "speaking_rate": True},
            {**self.valid, "speaking_rate": 2.1},
        ]
        for candidate in mutations:
            with self.subTest(candidate=candidate), self.assertRaises(RequestError):
                self.parse(candidate)

    def test_invalid_utf8_and_oversized_text_fail(self) -> None:
        with self.assertRaises(RequestError):
            _parse_request(b"\xff", 1_200, ("voice-1",))
        with self.assertRaises(RequestError):
            self.parse({**self.valid, "text": "x" * 1_201})


class BackendGuardTests(unittest.TestCase):
    def test_deterministic_backend_is_mock_or_lab_only(self) -> None:
        with patch.dict(os.environ, {"IVR_EXECUTION_MODE": "MOCK"}, clear=False):
            backend = DeterministicTestBackend()
            backend.load()
            rendered = backend.synthesize("fixture", "test-north", 1.0)
            self.assertGreater(rendered.size, 0)
        with patch.dict(os.environ, {"IVR_EXECUTION_MODE": "PRODUCTION_REAL"}, clear=False):
            with self.assertRaises(BackendError):
                DeterministicTestBackend().load()

    def test_text_split_is_bounded_and_lossless_by_words(self) -> None:
        source = ("một hai ba bốn năm sáu bảy tám chín mười. " * 30).strip()
        chunks = _split_text(source, 80)
        self.assertGreater(len(chunks), 1)
        self.assertTrue(all(0 < len(chunk) <= 80 for chunk in chunks))
        self.assertEqual(source.split(), " ".join(chunks).split())


class VoiceAcceptanceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        source_path = Path(__file__).parents[1] / "shim" / "voices.json"
        runtime_path = Path("/opt/ivr-tts/shim/voices.json")
        voice_config_path = runtime_path if runtime_path.is_file() else source_path
        cls.voice_config = json.loads(
            voice_config_path.read_text(encoding="utf-8")
        )

    def fixture(self) -> dict[str, object]:
        selected = {
            region: next(
                item for item in self.voice_config["voices"] if item["region"] == region
            )
            for region in ("North", "Central", "South")
        }
        selected_ids = {item["voice_id"] for item in selected.values()}
        return {
            "schema_version": 1,
            "work_id": "W-0122",
            "status": "OWNER_ACCEPTED",
            "stale_relisten_required": False,
            "source_commit": self.voice_config["source_commit"],
            "model_artifacts": [
                {
                    "repo": "pnnbao-ump/VieNeu-TTS-v3-Turbo",
                    "revision": self.voice_config["model_revision"],
                },
                {
                    "repo": "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX",
                    "revision": self.voice_config["codec_revision"],
                },
            ],
            **{
                field: self.voice_config[field]
                for field in (
                    "voice_manifest_sha256",
                    "dependency_lock_sha256",
                    "runtime_lock_sha256",
                    "model_lock_sha256",
                    "audition_script_sha256",
                    "audition_manifest_sha256",
                    "audition_renderer_sha256",
                    "listening_profile_id",
                )
            },
            "listening_route": "ASTERISK_MICROSIP_8KHZ",
            "listener": "TEST_ONLY_OWNER_FIXTURE",
            "listened_at": "2026-08-27T00:00:00+07:00",
            "device_and_lab_route": "TEST_ONLY_ASTERISK_MICROSIP_8KHZ",
            "approval_reference": "TEST_ONLY_NO_AUTHORITY",
            "all_11_candidates_listened": True,
            "selections": {
                region: {
                    "voice_id": item["voice_id"],
                    "preset": item["preset"],
                    "speaking_rate": item["speaking_rate"],
                    "owner_notes": "TEST_ONLY",
                }
                for region, item in selected.items()
            },
            "candidate_results": [
                {
                    "voice_id": item["voice_id"],
                    "region": item["region"],
                    "listened": True,
                    "verdict": (
                        "SELECTED" if item["voice_id"] in selected_ids else "NOT_SELECTED"
                    ),
                    "notes": "TEST_ONLY",
                }
                for item in self.voice_config["voices"]
            ],
            "notes": "TEST_ONLY fixture; never an Owner approval.",
        }

    def test_exact_owner_acceptance_contract_passes(self) -> None:
        selections = validate_voice_acceptance(self.fixture(), self.voice_config)
        self.assertEqual({"North", "Central", "South"}, set(selections))

    def test_acceptance_mutations_fail_closed(self) -> None:
        mutations = {
            "status": lambda value: value.update(status="PENDING_OWNER_LISTENING"),
            "stale": lambda value: value.update(stale_relisten_required=True),
            "hash": lambda value: value.update(runtime_lock_sha256="0" * 64),
            "route": lambda value: value.update(listening_route="DIRECT_WAV"),
            "incomplete": lambda value: value.update(all_11_candidates_listened=False),
            "unheard": lambda value: value["candidate_results"][0].update(listened=False),
            "verdict": lambda value: value["candidate_results"][0].update(
                verdict="NOT_SELECTED"
            ),
            "extra": lambda value: value.update(unreviewed=True),
        }
        for name, mutate in mutations.items():
            candidate = deepcopy(self.fixture())
            mutate(candidate)
            with self.subTest(mutation=name), self.assertRaises(VoiceAcceptanceError):
                validate_voice_acceptance(candidate, self.voice_config)


if __name__ == "__main__":
    unittest.main()
