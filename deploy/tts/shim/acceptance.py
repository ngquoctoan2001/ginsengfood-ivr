from __future__ import annotations

from datetime import datetime
from typing import Any


class VoiceAcceptanceError(RuntimeError):
    """The Owner voice-selection artifact is absent, stale or does not bind this candidate."""


REGIONS = ("North", "Central", "South")
TOP_LEVEL_KEYS = {
    "schema_version",
    "work_id",
    "status",
    "stale_relisten_required",
    "source_commit",
    "model_artifacts",
    "voice_manifest_sha256",
    "dependency_lock_sha256",
    "runtime_lock_sha256",
    "model_lock_sha256",
    "audition_script_sha256",
    "audition_manifest_sha256",
    "audition_renderer_sha256",
    "listening_profile_id",
    "listening_route",
    "listener",
    "listened_at",
    "device_and_lab_route",
    "approval_reference",
    "all_11_candidates_listened",
    "selections",
    "candidate_results",
    "notes",
}


def validate_voice_acceptance(
    candidate: dict[str, Any], voice_config: dict[str, Any]
) -> dict[str, dict[str, Any]]:
    _exact_keys(candidate, TOP_LEVEL_KEYS, "acceptance manifest")
    _require(candidate.get("schema_version") == 1, "acceptance schema drift")
    _require(candidate.get("work_id") == "W-0122", "acceptance work drift")
    _require(candidate.get("status") == "OWNER_ACCEPTED", "owner acceptance missing")
    _require(candidate.get("stale_relisten_required") is False, "owner acceptance stale")
    _require(candidate.get("source_commit") == voice_config.get("source_commit"), "source drift")

    expected_models = [
        {
            "repo": "pnnbao-ump/VieNeu-TTS-v3-Turbo",
            "revision": voice_config.get("model_revision"),
        },
        {
            "repo": "OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX",
            "revision": voice_config.get("codec_revision"),
        },
    ]
    _require(candidate.get("model_artifacts") == expected_models, "model artifact drift")
    bindings = {
        "voice_manifest_sha256": voice_config.get("voice_manifest_sha256"),
        "dependency_lock_sha256": voice_config.get("dependency_lock_sha256"),
        "runtime_lock_sha256": voice_config.get("runtime_lock_sha256"),
        "model_lock_sha256": voice_config.get("model_lock_sha256"),
        "audition_script_sha256": voice_config.get("audition_script_sha256"),
        "audition_manifest_sha256": voice_config.get("audition_manifest_sha256"),
        "audition_renderer_sha256": voice_config.get("audition_renderer_sha256"),
        "listening_profile_id": voice_config.get("listening_profile_id"),
    }
    for field, expected in bindings.items():
        _require(isinstance(expected, str) and candidate.get(field) == expected, f"{field} drift")

    _require(candidate.get("listening_route") == "ASTERISK_MICROSIP_8KHZ", "route drift")
    _required_text(candidate.get("listener"), "listener", 200)
    _required_text(candidate.get("device_and_lab_route"), "device route", 500)
    _required_text(candidate.get("approval_reference"), "approval reference", 500)
    _optional_text(candidate.get("notes"), "notes", 2000)
    listened_at = candidate.get("listened_at")
    _require(isinstance(listened_at, str), "listened_at missing")
    try:
        parsed = datetime.fromisoformat(listened_at.replace("Z", "+00:00"))
    except ValueError as error:
        raise VoiceAcceptanceError("listened_at invalid") from error
    _require(parsed.tzinfo is not None, "listened_at timezone missing")
    _require(candidate.get("all_11_candidates_listened") is True, "candidate listening incomplete")

    roster = voice_config.get("voices")
    _require(isinstance(roster, list) and len(roster) == 11, "voice roster drift")
    roster_by_id = {item.get("voice_id"): item for item in roster if isinstance(item, dict)}
    _require(len(roster_by_id) == 11, "voice roster duplicate")

    selections_raw = candidate.get("selections")
    _exact_keys(selections_raw, set(REGIONS), "selections")
    selections: dict[str, dict[str, Any]] = {}
    selected_ids: set[str] = set()
    for region in REGIONS:
        selection = selections_raw.get(region)
        _exact_keys(
            selection,
            {"voice_id", "preset", "speaking_rate", "owner_notes"},
            f"{region} selection",
        )
        voice_id = selection.get("voice_id")
        roster_item = roster_by_id.get(voice_id)
        _require(roster_item is not None and roster_item.get("region") == region, "selection region drift")
        _require(selection.get("preset") == roster_item.get("preset"), "selection preset drift")
        rate = selection.get("speaking_rate")
        roster_rate = roster_item.get("speaking_rate")
        _require(
            isinstance(rate, (int, float))
            and not isinstance(rate, bool)
            and isinstance(roster_rate, (int, float))
            and not isinstance(roster_rate, bool)
            and float(rate) == float(roster_rate),
            "selection speaking rate drift",
        )
        _optional_text(selection.get("owner_notes"), "owner notes", 1000)
        _require(voice_id not in selected_ids, "regional selections duplicate")
        selected_ids.add(voice_id)
        selections[region] = selection

    results = candidate.get("candidate_results")
    _require(isinstance(results, list) and len(results) == 11, "candidate results incomplete")
    result_ids: set[str] = set()
    for index, result in enumerate(results):
        _exact_keys(
            result,
            {"voice_id", "region", "listened", "verdict", "notes"},
            f"candidate result {index}",
        )
        roster_item = roster[index]
        voice_id = result.get("voice_id")
        _require(
            voice_id == roster_item.get("voice_id")
            and result.get("region") == roster_item.get("region"),
            "candidate result roster drift",
        )
        _require(voice_id not in result_ids, "candidate result duplicate")
        result_ids.add(voice_id)
        _require(result.get("listened") is True, "candidate not listened")
        if voice_id in selected_ids:
            _require(result.get("verdict") == "SELECTED", "selected verdict drift")
        else:
            _require(result.get("verdict") in {"NOT_SELECTED", "REJECTED"}, "candidate verdict drift")
        _optional_text(result.get("notes"), "candidate notes", 1000)
    _require(result_ids == set(roster_by_id), "candidate result set drift")
    return selections


def _exact_keys(value: Any, expected: set[str], label: str) -> None:
    _require(isinstance(value, dict), f"{label} must be object")
    _require(set(value) == expected, f"{label} keys drift")


def _required_text(value: Any, label: str, limit: int) -> None:
    _require(
        isinstance(value, str)
        and value == value.strip()
        and 0 < len(value) <= limit
        and not _has_control(value),
        f"{label} invalid",
    )


def _optional_text(value: Any, label: str, limit: int) -> None:
    _require(value is None or (isinstance(value, str) and len(value) <= limit and not _has_control(value)), f"{label} invalid")


def _has_control(value: str) -> bool:
    return any((ord(char) < 32 and char not in "\n\r\t") or ord(char) == 127 for char in value)


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise VoiceAcceptanceError(message)
