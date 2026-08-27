from __future__ import annotations

import hashlib
import json
import math
import os
from pathlib import Path
from typing import Any

from .acceptance import VoiceAcceptanceError, validate_voice_acceptance
from .model_lock import ModelLockError, sha256_file, verify_bundle


class BackendError(RuntimeError):
    """Privacy-safe backend failure; messages must never include customer text."""


class VieNeuBackend:
    sample_rate = 48_000

    def __init__(self) -> None:
        self.bundle_root = Path(os.environ.get("VIE_NEU_BUNDLE_ROOT", "/models"))
        self.lock_path = Path(
            os.environ.get("VIE_NEU_MODEL_LOCK", "/opt/ivr-tts/models/MODELS.lock")
        )
        self.voice_config_path = Path(
            os.environ.get("VIE_NEU_VOICE_CONFIG", "/opt/ivr-tts/shim/voices.json")
        )
        self.upstream_voice_path = Path(
            os.environ.get(
                "VIE_NEU_UPSTREAM_VOICE_MANIFEST",
                "/opt/vieneu/src/vieneu/assets/voices_v3_turbo.json",
            )
        )
        self.acceptance_path = Path(
            os.environ.get(
                "VIE_NEU_VOICE_ACCEPTANCE_MANIFEST",
                "/run/ivr-tts/voice-acceptance-manifest.json",
            )
        )
        self.runtime_lock_path = Path(
            os.environ.get(
                "VIE_NEU_RUNTIME_LOCK",
                "/opt/ivr-runtime/runtime-requirements.lock",
            )
        )
        self.dependency_lock_path = Path(
            os.environ.get("VIE_NEU_DEPENDENCY_LOCK", "/opt/vieneu/uv.lock")
        )
        self._engine: Any = None
        self._voices: dict[str, dict[str, Any]] = {}

    @property
    def voice_ids(self) -> tuple[str, ...]:
        return tuple(sorted(self._voices))

    def load(self) -> None:
        try:
            lock = verify_bundle(self.lock_path, self.bundle_root)
            voice_config = json.loads(self.voice_config_path.read_text(encoding="utf-8"))
            upstream_voices = json.loads(self.upstream_voice_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError, ModelLockError) as error:
            raise BackendError("locked artifacts unavailable") from error

        expected_manifest_hash = lock.get("voice_manifest_sha256")
        if sha256_file(self.upstream_voice_path) != expected_manifest_hash:
            raise BackendError("voice manifest drift")
        if voice_config.get("voice_manifest_sha256") != expected_manifest_hash:
            raise BackendError("voice allowlist drift")
        if sha256_file(self.lock_path) != voice_config.get("model_lock_sha256"):
            raise BackendError("model lock binding drift")
        if sha256_file(self.runtime_lock_path) != voice_config.get("runtime_lock_sha256"):
            raise BackendError("runtime lock binding drift")
        if sha256_file(self.dependency_lock_path) != voice_config.get("dependency_lock_sha256"):
            raise BackendError("dependency lock binding drift")

        allowed_env = os.environ.get("VIE_NEU_ALLOWED_VOICE_IDS", "")
        audition_mode = os.environ.get("VIE_NEU_AUDITION_MODE", "0") == "1"
        requested = {value.strip() for value in allowed_env.split(",") if value.strip()}
        configured = {
            item["voice_id"]: item
            for item in voice_config.get("voices", [])
            if isinstance(item, dict) and isinstance(item.get("voice_id"), str)
        }
        if audition_mode:
            if os.environ.get("IVR_EXECUTION_MODE", "") not in {"MOCK", "LAB_REAL_SIM"}:
                raise BackendError("audition mode forbidden")
            permitted = {
                key for key, value in configured.items() if value.get("audition_enabled") is True
            }
        else:
            try:
                acceptance = json.loads(self.acceptance_path.read_text(encoding="utf-8"))
                selections = validate_voice_acceptance(acceptance, voice_config)
            except (OSError, json.JSONDecodeError, VoiceAcceptanceError) as error:
                raise BackendError("voice acceptance missing or invalid") from error
            permitted = {selection["voice_id"] for selection in selections.values()}
            if len(permitted) != 3 or requested != permitted:
                raise BackendError("accepted voice routing drift")

        presets = upstream_voices.get("presets", {})
        selected: dict[str, dict[str, Any]] = {}
        for voice_id in permitted:
            item = configured.get(voice_id)
            preset = presets.get(item.get("preset") if item else None)
            if not item or not isinstance(preset, dict):
                raise BackendError("voice allowlist entry unavailable")
            selected[voice_id] = {**item, "preset_data": preset}
        if not selected:
            raise BackendError("voice allowlist empty")

        try:
            from vieneu._v3_turbo_engine.onnx_runtime_lite import OnnxV3LiteEngine

            self._engine = OnnxV3LiteEngine(
                checkpoint_path=str(self.bundle_root / "vieneu"),
                onnx_dir=str(self.bundle_root / "vieneu" / "onnx_int8"),
                codec_dir=str(self.bundle_root / "moss-codec"),
            )
        except Exception as error:
            raise BackendError("model load failed") from error
        self._voices = selected

    def synthesize(self, text: str, voice_id: str, speaking_rate: float) -> Any:
        if self._engine is None or voice_id not in self._voices:
            raise BackendError("backend unavailable")
        voice = self._voices[voice_id]
        if not math.isclose(speaking_rate, float(voice["speaking_rate"]), abs_tol=0.000001):
            raise BackendError("speaking rate not approved")

        try:
            import numpy as np

            preset = voice["preset_data"]
            chunks = _split_text(text, 240)
            rendered = [
                self._engine.infer(
                    text=chunk,
                    speaker_emb=np.asarray(preset["speaker_emb"], dtype=np.float32),
                    ref_codes=np.asarray(preset["codes"], dtype=np.int64),
                    temperature=0.0,
                    top_k=1,
                    top_p=1.0,
                    repetition_penalty=1.2,
                    frame_cap=True,
                )
                for chunk in chunks
            ]
            gap = np.zeros(int(self.sample_rate * 0.12), dtype=np.float32)
            output: list[Any] = []
            for index, item in enumerate(rendered):
                if index:
                    output.append(gap)
                output.append(np.asarray(item, dtype=np.float32))
            return np.concatenate(output)
        except Exception as error:
            raise BackendError("synthesis failed") from error


class DeterministicTestBackend:
    """Network-free contract backend; guarded so it cannot become a production fallback."""

    sample_rate = 48_000

    def __init__(self) -> None:
        self._loaded = False
        self._voice_ids = ("test-north", "test-central", "test-south")

    @property
    def voice_ids(self) -> tuple[str, ...]:
        return self._voice_ids if self._loaded else ()

    def load(self) -> None:
        if os.environ.get("IVR_EXECUTION_MODE", "") not in {"MOCK", "LAB_REAL_SIM"}:
            raise BackendError("test backend forbidden")
        self._loaded = True

    def synthesize(self, text: str, voice_id: str, speaking_rate: float) -> Any:
        if not self._loaded or voice_id not in self._voice_ids or not math.isclose(speaking_rate, 1.0):
            raise BackendError("test backend request rejected")
        try:
            import numpy as np
        except ImportError as error:
            raise BackendError("test dependency unavailable") from error
        digest = hashlib.sha256((voice_id + "\0" + text).encode("utf-8")).digest()
        frequency = 220 + digest[0]
        count = self.sample_rate // 4
        timeline = np.arange(count, dtype=np.float32) / self.sample_rate
        return (0.15 * np.sin(2 * np.pi * frequency * timeline)).astype(np.float32)


def create_backend() -> VieNeuBackend | DeterministicTestBackend:
    mode = os.environ.get("VIE_NEU_BACKEND", "vieneu-onnx")
    if mode == "vieneu-onnx":
        return VieNeuBackend()
    if mode == "deterministic-test":
        return DeterministicTestBackend()
    raise BackendError("unknown backend")


def _split_text(text: str, limit: int) -> list[str]:
    if len(text) <= limit:
        return [text]
    chunks: list[str] = []
    remaining = text
    while len(remaining) > limit:
        boundary = max(
            remaining.rfind(mark, 0, limit + 1)
            for mark in (". ", ", ", "; ", ": ", " ")
        )
        if boundary < limit // 2:
            boundary = limit
        else:
            boundary += 1
        chunks.append(remaining[:boundary].strip())
        remaining = remaining[boundary:].strip()
    if remaining:
        chunks.append(remaining)
    if any(not chunk for chunk in chunks):
        raise BackendError("text chunking failed")
    return chunks
