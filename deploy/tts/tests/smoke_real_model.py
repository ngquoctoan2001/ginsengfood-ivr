"""W-0320: opt-in, offline real-model HTTP smoke; not part of model-free unit tests.

Run inside a pinned TTS image with the verified bundle and Owner manifest mounted read-only.
Only synthetic speech is used. Metadata is printed; this is not listening or production approval.
"""
from __future__ import annotations

import hashlib
import json
import platform
import threading
import time
import urllib.request

from shim.server import RuntimeState, TtsServer


def main() -> None:
    started = time.monotonic()
    state = RuntimeState()
    state.initialize()
    load_ms = round((time.monotonic() - started) * 1000)
    assert state.ready and state.backend.__class__.__name__ == "VieNeuBackend"
    assert len(state.backend.voice_ids) == 3
    server = TtsServer(("127.0.0.1", 0), state)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    base = f"http://127.0.0.1:{server.server_port}"
    cases = []
    try:
        with urllib.request.urlopen(base + "/health/ready", timeout=5) as response:
            assert response.status == 200
        texts = {
            "amount": "Năm trăm sáu mươi nghìn đồng.",
            "fake-order": "Xin chào Quý khách. Đơn hàng thử nghiệm có hai hộp cháo sâm. Tổng tiền năm trăm sáu mươi nghìn đồng.",
        }
        for voice in state.backend.voice_ids:
            for label, text in texts.items():
                for iteration in (1, 2):
                    body = json.dumps({
                        "text": text, "voice_id": voice, "locale": "vi-VN", "speaking_rate": 1.0,
                        "output_format": "audio/L16", "sample_rate": 8000,
                    }, ensure_ascii=False).encode("utf-8")
                    request = urllib.request.Request(base + "/synthesize", data=body,
                                                     headers={"Content-Type": "application/json"})
                    request_started = time.monotonic()
                    with urllib.request.urlopen(request, timeout=120) as response:
                        pcm = response.read()
                        assert response.status == 200 and response.headers["Content-Type"] == "audio/L16"
                    assert len(pcm) >= 1600 and len(pcm) % 2 == 0 and not pcm.startswith(b"RIFF")
                    assert any(pcm), "silent PCM"
                    result = {
                        "voice_id": voice, "case": label, "iteration": iteration,
                        "elapsed_ms": round((time.monotonic() - request_started) * 1000),
                        "pcm_bytes": len(pcm), "audio_ms": len(pcm) // 16,
                        "sha256": hashlib.sha256(pcm).hexdigest(),
                    }
                    cases.append(result)
                    print("REAL_MODEL_CASE " + json.dumps(result), flush=True)
    finally:
        server.shutdown()
        server.server_close()
        thread.join(timeout=5)
    print("REAL_MODEL_SMOKE_PASS " + json.dumps({
        "python": platform.python_version(), "load_and_startup_smoke_ms": load_ms,
        "cases": cases, "real_customer_call_allowed": "NO", "listening_acceptance": "NOT_RUN",
    }), flush=True)


if __name__ == "__main__":
    main()
