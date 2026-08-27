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

CI entry points:

```text
node deploy/ci/scripts/tts-provenance-gate.mjs --selftest
node deploy/ci/scripts/tts-audition-selftest.mjs
node deploy/ci/scripts/tts-voice-acceptance-gate.mjs --selftest
node deploy/ci/scripts/tts-container-selftest.mjs
node deploy/ci/scripts/tts-helm-selftest.mjs
```
