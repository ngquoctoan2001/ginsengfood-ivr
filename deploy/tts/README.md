# W-0122 VieNeu-TTS adapter

The image exposes only loopback HTTP inside the worker network namespace. It returns headerless
signed 16-bit little-endian mono PCM at 8 kHz (`audio/L16`). It does not persist text or audio and
has no SaaS fallback.

Model weights are intentionally absent from Git and from the image build context. Use
`scripts/fetch-model-nonprod.py` only with its explicit non-production acknowledgement, then verify
the exact bundle with `scripts/verify-model.py`. Production must replace public fetch with the
owner-approved internal mirror recorded in `models/MODELS.lock`.

`shim/voices.json` lists the exact 11 female audition candidates but grants none of them production
authority. Production readiness requires a separately mounted Owner manifest that proves all 11
were heard through the pinned Asterisk/MicroSIP 8 kHz route, binds every candidate hash, selects
exactly one distinct voice per region and matches the three configured routing IDs.

Tracked evidence and owner templates are under `docs/evidence/W-0122/`. Local weight bundles,
audition WAVs and generated SBOMs are intentionally Git-ignored; only exact manifests/hashes are
tracked. `THIRD_PARTY_NOTICES.md` records the known source/model/codec/base-image attribution and
the unresolved Legal gates.

The release image installs the hash-locked 24-package inference subset from
`runtime-requirements.lock`; the full vendored upstream `uv.lock` remains provenance evidence but
is not installed. The image sets `HF_HUB_OFFLINE=1`. The current local security scan and residual
release blockers are recorded in `docs/evidence/W-0122/security-performance.md`.

W-0323 selects digest-pinned Chainguard Python builder/runtime images in `Dockerfile.tts`.
The final image is tested with the real ONNX model; its exact image ID and current scan are in
[W-0323](../../docs/evidence/W-0323/README.md). The earlier Debian scan is historical.
Changing the base does not grant model licensing or production authority.

After Owner feedback in W-0323, the VieNeu backend shortens outer silence longer than 250 ms,
retaining a 60 ms guard. Detection uses 10 ms RMS windows in the 8 kHz telephone band, so
inaudible high-frequency noise cannot hide a pause. Internal pauses and short natural boundaries
are preserved. This runs before dialing; no order speech is generated during playback.
The fixed, already-auditioned catalog stays unchanged. The Owner accepted the final voices and
seams in W-0323; that listening decision does not require another audition.

W-0326 closes empty-response HTTP connections after early request refusals. This prevents unread
JSON from corrupting the next request on a pooled connection after an overload/invalid request.
Successful PCM responses still use the existing contract. An inference already running can remain
busy after its client times out; this change does not cancel ONNX inference or certify a timeout
budget. Three fresh-process measurements and unchanged-audio proof are in
[W-0326](../../docs/evidence/W-0326/README.md).

`VIE_NEU_ORT_THREADS` accepts `0` (the existing upstream automatic default) or `1..8`.
An invalid value keeps readiness closed. The explicit speech lab profile uses `1` and bounds
`OPENBLAS_NUM_THREADS`, `OMP_NUM_THREADS` and `MKL_NUM_THREADS` to `1`, avoiding CPU oversubscription
measured in W-0321. This changes scheduling, not model weights; remeasure on the target host.

`tests/measure_lab_orders.py` is an opt-in diagnostic for synthetic, worker-rendered segmented
orders. Mount the verified model/Owner voice manifest, renderer JSON, fixed WAV catalog and an
output directory; run with `--texts <json> --fixed <directory> --out <directory> [--repeat 1..5]`.
It requires `LAB_REAL_SIM` and `REAL_CUSTOMER_CALL_ALLOWED=NO`, uses the real HTTP/backend path,
and records per-request latency, audio duration and PCM hashes. Its diagnostic HTTP timeout is
60 seconds so it can report requests that exceed the worker's 5-second default; completion
does not imply that performance budget passed. W-0323 contains the measured limits and lab override.

CI entry points:

```text
node deploy/ci/scripts/tts-provenance-gate.mjs --selftest
node deploy/ci/scripts/tts-audition-selftest.mjs
node deploy/ci/scripts/tts-voice-acceptance-gate.mjs --selftest
node deploy/ci/scripts/tts-container-selftest.mjs
node deploy/ci/scripts/tts-helm-selftest.mjs
```

Worker speech preparation (W-0335): the external provider path prepares one complete order at
a time per worker, with FIFO admission. `Ivr:Speech:Tts:PreparationQueueLimit` defaults to 8
waiting orders and `PreparationQueueTimeoutMilliseconds` to 30000. Queue waiting is separate
from the existing per-segment timeout; caller cancellation and the order's confirmation deadline
cover both waiting and synthesis. Full/expired queues fail before dialing. This is process-local
admission, not a distributed queue for multiple workers sharing one model endpoint.
HTTP 503 retries keep their original deadline and capped 1-second backoff, without the former
nine-request ceiling. Inference already running in ONNX may continue after a client disconnect.
See [S5 worker probe](../../docs/evidence/W-0335/README.md); production calls remain disabled.

W-0338 adds `PreparationTimeoutMilliseconds` (default 120000, allowed 10–120000) across the
entire external preparation, including FIFO wait, busy retries and all segments. Caller
cancellation and an earlier order expiry still stop preparation; a provider returning after
cancellation cannot return a playlist. Total expiry is `TTS_PREPARATION_TIMEOUT`, distinct
from queue, segment and order expiry. The explicit lab profile uses 30s/segment, 90s queue,
120s total and a 360s channel lease; it does not alter the production per-segment default.
The offline probe's `--final-profile` selects the same deadlines for all cases. See
[W-0338 evidence and limits](../../docs/evidence/W-0338/README.md).
