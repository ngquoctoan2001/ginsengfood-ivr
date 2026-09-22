# W-0122 third-party provenance notice

This file is an inventory and attribution aid, not a Legal conclusion. Production distribution
remains blocked until Legal/Privacy approves the exact source, model, codec, voice and retention
set recorded in `models/MODELS.lock` and the Owner-signed voice manifest.

| Component | Exact source | Evidence carried by this repository | Current gate |
| --- | --- | --- | --- |
| VieNeu-TTS source 3.3.0 | `pnnbao97/VieNeu-TTS@36c4b501b0634a8f59805e6b529a058fbd30190b` | `third_party/vieneu-tts/LICENSE`, `UPSTREAM.md`, frozen `uv.lock` | Source file declares Apache-2.0; Legal review still required for distribution/use context |
| VieNeu-TTS v3 Turbo weights/tokenizer | `pnnbao-ump/VieNeu-TTS-v3-Turbo@2da0efab622a1722125991736524f080b751ef5b` | Exact paths/sizes/SHA-256 plus pinned model card in `MODELS.lock` bundle | Model card declares Apache-2.0; pinned revision has no LICENSE file; `RELEASE_BLOCKED` |
| MOSS Audio Tokenizer Nano ONNX | `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX@ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae` | Exact decoder paths/sizes/SHA-256 plus pinned model card in `MODELS.lock` bundle | Model card declares Apache-2.0; pinned revision has no LICENSE file; `RELEASE_BLOCKED` |
| Python/runtime packages | Upstream dependency universe in vendored `uv.lock`; production subset in `runtime-requirements.lock` (SHA-256 `a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304`) | Hash-locked 24-package Linux/amd64 inference set; upstream web UI/API, training and voice-cloning packages excluded; provenance gate binds both locks | Vulnerability/license disposition required by release policy |
| Python base images | Chainguard Python: builder `sha256:073219a510343c070f74fd1fd7100b9e599cf68eb1536be8ebd4c0b170ede589`; runtime `sha256:3402da0629d26501855f13d490b7fa1b525e4a5db3a10af324277e5507fcb8e6`, both at `cgr.dev/chainguard/python` | Both immutable digests pinned in `Dockerfile.tts`; Python 3.14.7 in the tested image; package inventory in the Trivy JSON | W-0323 selected this base after container/model checks and an exact-image scan with 0 HIGH/0 CRITICAL. This does not close model licensing or production gates. Prior Debian scan remains historical in W-0317/W-0321 |
| uv build tool image | `ghcr.io/astral-sh/uv:0.8.14@sha256:d97bc3f40af096399f67e8e69e10b7735f3dbc6fed300391637ecb00f37af981` | Digest-pinned in `Dockerfile.tts` | Build-only supply-chain input |

The pinned VieNeu source tree contains no upstream `NOTICE`. Neither pinned model repository
contains a `LICENSE` file. Absence is recorded explicitly; it must not be replaced with an inferred
license file copied from another revision.

Generated SBOM/vulnerability reports live under ignored `artifacts/sbom/`; their hashes and scan
outcomes are recorded in `docs/evidence/W-0122/README.md` so large, time-sensitive reports are not
mistaken for stable source artifacts.

The current selected-image scan after the HTTP lifecycle fix is under
`.artifacts/W-0326/scan/trivy.json`, with its hash, archive-to-image binding and scanner/database
provenance in `docs/evidence/W-0326/local-results.json`. The scan uses an exported archive with
network disabled and no Docker socket. W-0323's scan remains historical evidence for that image.
