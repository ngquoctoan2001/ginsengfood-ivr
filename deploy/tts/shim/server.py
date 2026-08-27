from __future__ import annotations

import json
import os
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from typing import Any

from .backend import BackendError, create_backend
from .convert import ConversionError, float32_to_l16


_REQUEST_FIELDS = {
    "text",
    "voice_id",
    "locale",
    "speaking_rate",
    "output_format",
    "sample_rate",
}


class RuntimeState:
    def __init__(self) -> None:
        self.backend: Any = None
        self.ready = False
        self.max_text_chars = int(os.environ.get("VIE_NEU_MAX_TEXT_CHARS", "1200"))
        self.max_body_bytes = int(os.environ.get("VIE_NEU_MAX_BODY_BYTES", "16384"))
        self.max_duration_seconds = float(os.environ.get("VIE_NEU_MAX_DURATION_SECONDS", "120"))
        self.capacity = threading.BoundedSemaphore(
            int(os.environ.get("VIE_NEU_MAX_CONCURRENCY", "1"))
        )

    def initialize(self) -> None:
        try:
            backend = create_backend()
            backend.load()
            first_voice = backend.voice_ids[0]
            smoke = backend.synthesize("Xin chào.", first_voice, 1.0)
            pcm = float32_to_l16(smoke, source_rate=backend.sample_rate, max_duration_seconds=10)
            if len(pcm) < 1600:
                raise BackendError("startup smoke too short")
            self.backend = backend
            self.ready = True
            _metric("startup", "ready")
        except (BackendError, ConversionError, OSError, ValueError):
            self.backend = None
            self.ready = False
            _metric("startup", "not_ready")


class TtsServer(ThreadingHTTPServer):
    daemon_threads = True
    request_queue_size = 8
    allow_reuse_address = False

    def __init__(self, address: tuple[str, int], state: RuntimeState) -> None:
        self.state = state
        super().__init__(address, TtsHandler)

    def handle_error(self, _request: object, _client_address: object) -> None:
        # socketserver's default prints a traceback. Even though request text is not present in
        # the current stack, production logs must remain structurally privacy-safe on every error.
        _metric("server", "handler_error")


class TtsHandler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"
    server_version = ""
    sys_version = ""

    @property
    def state(self) -> RuntimeState:
        return self.server.state  # type: ignore[attr-defined,no-any-return]

    def log_message(self, _format: str, *args: object) -> None:
        return

    def do_GET(self) -> None:  # noqa: N802
        if self.path == "/health/live":
            self._empty(200)
        elif self.path == "/health/ready":
            self._empty(200 if self.state.ready else 503)
        else:
            self._empty(404)

    def do_POST(self) -> None:  # noqa: N802
        started = time.monotonic()
        if self.path != "/synthesize":
            self._empty(404)
            return
        if not self.state.ready or self.state.backend is None:
            self._empty(503)
            return
        if self.headers.get("Content-Type") != "application/json":
            self._empty(415)
            return

        length_header = self.headers.get("Content-Length")
        if length_header is None or not length_header.isdecimal():
            self._empty(411)
            return
        length = int(length_header)
        if length <= 0 or length > self.state.max_body_bytes:
            self._empty(413)
            return
        if not self.state.capacity.acquire(blocking=False):
            self._empty(503)
            _metric("request", "overloaded")
            return

        status = "internal_error"
        try:
            raw = self.rfile.read(length)
            request = _parse_request(raw, self.state.max_text_chars, self.state.backend.voice_ids)
            audio = self.state.backend.synthesize(
                request["text"], request["voice_id"], request["speaking_rate"]
            )
            pcm = float32_to_l16(
                audio,
                source_rate=self.state.backend.sample_rate,
                max_duration_seconds=self.state.max_duration_seconds,
            )
            self.send_response_only(200)
            self.send_header("Content-Type", "audio/L16")
            self.send_header("Content-Length", str(len(pcm)))
            self.send_header("Cache-Control", "no-store")
            self.send_header("X-Content-Type-Options", "nosniff")
            self.end_headers()
            self.wfile.write(pcm)
            status = "ok"
        except (BrokenPipeError, ConnectionResetError):
            status = "client_disconnected"
        except RequestError as error:
            status = error.metric_code
            self._empty(error.http_status)
        except (BackendError, ConversionError, OSError, ValueError):
            self._empty(500)
        finally:
            self.state.capacity.release()
            elapsed_ms = max(0, int((time.monotonic() - started) * 1000))
            _metric("request", status, elapsed_ms)

    do_PUT = do_DELETE = do_PATCH = do_OPTIONS = do_HEAD = lambda self: self._empty(405)

    def _empty(self, status: int) -> None:
        try:
            self.send_response_only(status)
            self.send_header("Content-Length", "0")
            self.send_header("Cache-Control", "no-store")
            self.send_header("X-Content-Type-Options", "nosniff")
            self.end_headers()
        except (BrokenPipeError, ConnectionResetError):
            return


class RequestError(ValueError):
    def __init__(self, http_status: int, metric_code: str) -> None:
        super().__init__(metric_code)
        self.http_status = http_status
        self.metric_code = metric_code


def _parse_request(raw: bytes, max_text_chars: int, voice_ids: tuple[str, ...]) -> dict[str, Any]:
    try:
        parsed = json.loads(raw.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as error:
        raise RequestError(400, "invalid_json") from error
    if not isinstance(parsed, dict) or set(parsed) != _REQUEST_FIELDS:
        raise RequestError(422, "invalid_schema")
    text = parsed.get("text")
    voice_id = parsed.get("voice_id")
    rate = parsed.get("speaking_rate")
    sample_rate = parsed.get("sample_rate")
    if not isinstance(text, str) or not text.strip() or len(text) > max_text_chars:
        raise RequestError(422, "invalid_text")
    if not isinstance(voice_id, str) or voice_id not in voice_ids:
        raise RequestError(422, "invalid_voice")
    if isinstance(rate, bool) or not isinstance(rate, (int, float)) or not 0.5 <= rate <= 2.0:
        raise RequestError(422, "invalid_rate")
    if parsed.get("locale") != "vi-VN" or parsed.get("output_format") != "audio/L16":
        raise RequestError(422, "invalid_contract")
    if isinstance(sample_rate, bool) or sample_rate != 8000:
        raise RequestError(422, "invalid_contract")
    parsed["text"] = text.strip()
    parsed["speaking_rate"] = float(rate)
    return parsed


def _metric(event: str, status: str, elapsed_ms: int | None = None) -> None:
    bucket = "na" if elapsed_ms is None else _latency_bucket(elapsed_ms)
    print(f"tts_event={event} status={status} latency_bucket={bucket}", flush=True)


def _latency_bucket(elapsed_ms: int) -> str:
    for bound in (100, 250, 500, 1000, 2500, 5000, 10000, 30000):
        if elapsed_ms <= bound:
            return f"le_{bound}ms"
    return "gt_30000ms"


def main() -> None:
    host = os.environ.get("VIE_NEU_HOST", "127.0.0.1")
    port = int(os.environ.get("VIE_NEU_PORT", "8090"))
    state = RuntimeState()
    state.initialize()
    TtsServer((host, port), state).serve_forever(poll_interval=0.5)


if __name__ == "__main__":
    main()
