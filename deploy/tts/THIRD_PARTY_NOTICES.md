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
| Python base image | `python:3.12.14-slim-trixie@sha256:7a8b475003c4fe15a2cd4e55e5cfc2f3560bdc9333d624f24cdd6d4340fd7a17`, with exact Debian security updates `libssl3t64`, `openssl`, `openssl-provider-legacy` `3.5.7-1~deb13u2` | Base digest and security package versions pinned in `Dockerfile.tts`; included in image SBOM | Release scan/disposition required; unfixed base-OS findings remain release-blocking |
| uv build tool image | `ghcr.io/astral-sh/uv:0.8.14@sha256:d97bc3f40af096399f67e8e69e10b7735f3dbc6fed300391637ecb00f37af981` | Digest-pinned in `Dockerfile.tts` | Build-only supply-chain input |

The pinned VieNeu source tree contains no upstream `NOTICE`. Neither pinned model repository
contains a `LICENSE` file. Absence is recorded explicitly; it must not be replaced with an inferred
license file copied from another revision.

Generated SBOM/vulnerability reports live under ignored `artifacts/sbom/`; their hashes and scan
outcomes are recorded in `docs/evidence/W-0122/README.md` so large, time-sensitive reports are not
mistaken for stable source artifacts.
