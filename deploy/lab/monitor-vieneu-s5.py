"""Run the pinned shim and record resource samples without changing its inference path."""
import json
import os
from pathlib import Path
import resource
import signal
import threading
import time
from shim.server import RuntimeState, TtsServer

assert os.environ['REAL_CUSTOMER_CALL_ALLOWED'] == 'NO'
assert os.environ['VIE_NEU_MAX_CONCURRENCY'] == '1'
stop = threading.Event()
signal.signal(signal.SIGTERM, lambda *_: stop.set())
signal.signal(signal.SIGINT, lambda *_: stop.set())
started = time.monotonic()
state = RuntimeState()
state.initialize()
assert state.ready and type(state.backend).__name__ == 'VieNeuBackend'
result = {'startup_ms': round((time.monotonic() - started) * 1000), 'capacity': 1,
          'ort_threads': state.backend._engine.ort_intra_op_threads, 'samples': [],
          'REAL_CUSTOMER_CALL_ALLOWED': 'NO'}
server = TtsServer(('127.0.0.1', 8090), state)
threading.Thread(target=server.serve_forever, daemon=True).start()
while True:
    sample = {'elapsed_ms': round((time.monotonic() - started) * 1000),
              'peak_rss_kib': resource.getrusage(resource.RUSAGE_SELF).ru_maxrss}
    for field in ('cpu.max', 'cpu.stat', 'memory.max', 'memory.current', 'memory.events'):
        path = Path('/sys/fs/cgroup') / field
        sample[field] = path.read_text().strip()
    result['samples'].append(sample)
    Path('/out/model.json').write_text(json.dumps(result, indent=2) + '\n')
    if stop.wait(5):
        break
server.shutdown()
