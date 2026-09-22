"""Real loopback HTTP lifecycle checks; synthetic audio only, no model or phone calls."""
from __future__ import annotations

from contextlib import contextmanager
import http.client
import json
import socket
import threading
import time
from types import SimpleNamespace
import unittest

import numpy as np

from shim.server import RuntimeState, TtsServer


BODY = json.dumps({"text": "Synthetic lab fixture", "voice_id": "fixture", "locale": "vi-VN",
                   "speaking_rate": 1.0, "output_format": "audio/L16", "sample_rate": 8000})


@contextmanager
def running_server(synthesize=None):
    state = RuntimeState()
    state.backend = SimpleNamespace(voice_ids=("fixture",), sample_rate=48000,
        synthesize=synthesize or (lambda *args: np.full(4800, 0.1, dtype=np.float32)))
    state.ready = True
    server = TtsServer(("127.0.0.1", 0), state)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    try:
        yield state, server.server_port
    finally:
        server.shutdown()
        server.server_close()
        thread.join(3)


class HttpLifecycleTests(unittest.TestCase):
    def test_busy_post_does_not_poison_the_next_request(self):
        with running_server() as (state, port):
            self.assertTrue(state.capacity.acquire(blocking=False))
            client = http.client.HTTPConnection("127.0.0.1", port, timeout=2)
            try:
                client.request("POST", "/synthesize", BODY, {"Content-Type": "application/json"})
                response = client.getresponse()
                self.assertEqual(503, response.status)
                self.assertEqual(b"", response.read())
                state.capacity.release()
                # A pooled client must be able to retry. The rejected JSON must not become
                # part of this request line, nor be echoed in the server's default HTML error.
                client.request("GET", "/health/ready")
                response = client.getresponse()
                self.assertEqual(200, response.status)
                self.assertEqual(b"", response.read())
            finally:
                client.close()

    def test_unsupported_content_type_does_not_poison_next_synthesis(self):
        with running_server() as (_, port):
            client = http.client.HTTPConnection("127.0.0.1", port, timeout=2)
            try:
                client.request("POST", "/synthesize", BODY, {"Content-Type": "text/plain"})
                response = client.getresponse()
                self.assertEqual(415, response.status)
                self.assertEqual(b"", response.read())
                client.request("POST", "/synthesize", BODY, {"Content-Type": "application/json"})
                response = client.getresponse()
                self.assertEqual(200, response.status)
                self.assertEqual(1600, len(response.read()))
            finally:
                client.close()

    def test_disconnected_client_releases_capacity_when_inference_finishes(self):
        entered = threading.Event()
        finish = threading.Event()
        def synthesize(*args):
            entered.set()
            if not finish.wait(3):
                raise RuntimeError("test inference did not finish")
            return np.full(4800, 0.1, dtype=np.float32)

        with running_server(synthesize) as (state, port):
            abandoned = http.client.HTTPConnection("127.0.0.1", port, timeout=0.05)
            retry = http.client.HTTPConnection("127.0.0.1", port, timeout=2)
            try:
                abandoned.request("POST", "/synthesize", BODY, {"Content-Type": "application/json"})
                self.assertTrue(entered.wait(1))
                with self.assertRaises(socket.timeout):
                    abandoned.getresponse()
                abandoned.close()
                retry.request("POST", "/synthesize", BODY, {"Content-Type": "application/json"})
                response = retry.getresponse()
                self.assertEqual(503, response.status)
                self.assertEqual(b"", response.read())
                finish.set()
                deadline = time.monotonic() + 2
                while not state.capacity.acquire(blocking=False):
                    if time.monotonic() >= deadline:
                        self.fail("capacity was not released after the abandoned inference")
                    time.sleep(0.01)
                state.capacity.release()
                retry.request("POST", "/synthesize", BODY, {"Content-Type": "application/json"})
                response = retry.getresponse()
                self.assertEqual(200, response.status)
                self.assertEqual(1600, len(response.read()))
            finally:
                finish.set()
                abandoned.close()
                retry.close()


if __name__ == "__main__":
    unittest.main()
