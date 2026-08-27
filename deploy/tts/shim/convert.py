from __future__ import annotations

from collections.abc import Sequence


class ConversionError(RuntimeError):
    """Audio cannot be converted to the IVR raw PCM contract."""


def float32_to_l16(
    audio: Sequence[float],
    *,
    source_rate: int = 48_000,
    target_rate: int = 8_000,
    max_duration_seconds: float = 120.0,
) -> bytes:
    """Anti-aliased float PCM to headerless signed little-endian 16-bit mono PCM."""
    try:
        import numpy as np
        import soxr
    except ImportError as error:
        raise ConversionError("resampler unavailable") from error

    if source_rate <= 0 or target_rate != 8_000 or max_duration_seconds <= 0:
        raise ConversionError("invalid conversion contract")

    samples = np.asarray(audio, dtype=np.float32)
    if samples.ndim == 2:
        samples = samples.mean(axis=1, dtype=np.float32)
    if samples.ndim != 1 or samples.size == 0:
        raise ConversionError("audio must be non-empty mono PCM")
    if not np.isfinite(samples).all():
        raise ConversionError("audio contains non-finite samples")
    if samples.size > int(source_rate * max_duration_seconds):
        raise ConversionError("audio exceeds duration limit")

    converted = soxr.resample(samples, source_rate, target_rate, quality="HQ")
    if converted.size == 0 or converted.size > int(target_rate * max_duration_seconds):
        raise ConversionError("resampled audio exceeds limits")
    converted = np.clip(converted, -1.0, 1.0)
    pcm = np.rint(converted * 32767.0).astype("<i2", copy=False).tobytes()
    if not pcm or len(pcm) % 2:
        raise ConversionError("invalid PCM byte count")
    return pcm

