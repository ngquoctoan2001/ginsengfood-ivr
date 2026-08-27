# W-0122 — Evidence index cho VieNeu-TTS self-hosted

Ngày: `2026-08-27`  
Baseline triển khai: `main@f291f44`  
Trạng thái: `IN_PROGRESS — RELEASE_BLOCKED`

> Evidence trong thư mục này tách rõ local/nonprod proof với Owner/Legal/Infra/telephony
> acceptance. Test xanh không cấp quyền production hoặc gọi khách thật.
> `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## Artifact đã đóng băng

| Thành phần | Pin |
| --- | --- |
| VieNeu source | commit `36c4b501b0634a8f59805e6b529a058fbd30190b`; tree `16632c30c2484aa4f86c8cde68a074192bd52736` |
| VieNeu model | `pnnbao-ump/VieNeu-TTS-v3-Turbo@2da0efab622a1722125991736524f080b751ef5b` |
| MOSS codec transitively required | `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX@ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae` |
| Runtime allowlist | 13 exact files trong `deploy/tts/models/MODELS.lock` |
| Voice manifest | SHA-256 `574e6acf03823c4cafdc43f106731ce5fce6de30228fe383831b8b9064ee0bd8` |
| Dependency lock | SHA-256 `bc375e3d5a64bcef007133781703a5689b8bba226f108437b812a97c00cbcec9` |
| Production runtime lock | 24 packages; SHA-256 `a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304` |
| Hardened local image | `sha256:4c76d318e24110267c908594031863cd7dbe3f31c92569c4e02b4de3ba9ba30d`; `126,109,579` bytes |

Model cards của cả hai model repo khai báo `Apache-2.0`, nhưng hai revision đã khóa không chứa
file `LICENSE`. Chưa có ý kiến Legal về commercial use của đúng preset voice/training-data gap,
và chưa có internal mirror URI/digest. Vì vậy production provenance giữ `RELEASE_BLOCKED`.

## Evidence tự động hiện có

| Gate | Kết quả hiện tại |
| --- | --- |
| Source/model exact pin | `PASS` ở nonprod; 13/13 size + SHA-256 khớp |
| Production model verifier | `EXPECTED_FAIL` — thiếu license-file evidence và internal mirror |
| Provenance mutations | `PASS` — revision/path/hash/license/extra artifact/voice-config/acceptance-template drift đều bị từ chối |
| Converter regression | `PASS` — MP3 cũ bitexact 12/12; WAV 12/12 PCM s16le/8 kHz/mono; unknown/missing source bị từ chối |
| Owner-manifest gate | `PASS` fail-closed — 11 mutation + pending template bị từ chối; production thiếu/pending manifest hoặc bật audition đều readiness `503` |
| Container contract | `12/12 PASS` — non-root `1654:1654`, read-only, no network, no exposed port, drop caps |
| Minimal runtime content | `PASS` — không có `uv`, web UI, upstream deploy/docs/examples, training, tests hoặc reference samples; chỉ venv + source runtime + locks/license |
| Real ONNX smoke | `PASS` nonprod — ready; request `200`, raw `audio/L16` 8 kHz, 20,480 bytes; không phải voice acceptance |
| Voice audition render | `FILES_READY` — 11/11 WAV PCM s16le/8 kHz/mono, tracked manifest SHA-256 `0cfbeacf6a60403c974354fc205e12591c12304f5b68a0abcac5d40afb8326cf`; owner chưa nghe/ký |
| Asterisk/MicroSIP audition harness | `RUNTIME_PASS` — 11/11 checksum/decode, `12201` playback pass, catch-all hangup pass; Owner listening vẫn `NOT_RUN` |
| Compose/media permissions | `PASS` local — loopback/no port/shared volume; UID 1654 write, Asterisk read-only/write-denied |
| Helm candidate | `PASS` local render — mặc định tắt; positive TEST_ONLY fixture và negative prod/lab guards pass |
| SBOM/vulnerability | `RELEASE_BLOCKED` — SPDX 152 entries; Trivy `13 HIGH`, `3 CRITICAL`, `0 fixable`; toàn bộ finding còn lại thuộc Debian 13.6 |
| Fixed catalog 12 file | `BLOCKED_BY_OD-VOICE-06` |
| 6 MicroSIP calls | `NOT_RUN` |
| Retention + rollback drill | `NOT_RUN` |
| Target-hardware performance | `ENV_BLOCKED` |
| Legal/Internal mirror/production topology | `OWNER_DATA_REQUIRED` / `OWNER_DECISION_REQUIRED` |

## Tài liệu và artifact liên quan

- `audition-script.txt`: một script versioned dùng giống nhau cho 11 candidate.
- `voice-acceptance-manifest.template.json`: mẫu fail-closed; không phải phê duyệt.
- `deploy/ci/scripts/tts-voice-acceptance-gate.mjs`: validator Owner artifact; template pending và fixture `TEST_ONLY` không có authority.
- `lab-runbook.md`: lệnh dựng lab, readiness, permission probe, call/retention/rollback procedure.
- `voice-audition-runbook.md`: profile Compose cô lập và mapping `12200`/`12201`–`12211` để Owner nghe đúng tuyến 8 kHz.
- `security-performance.md`: image digest, runtime lock, container/ONNX proof, SBOM, exact residual CVE và performance boundary.
- `deploy/tts/models/MODELS.lock`: allowlist/hashes và release blockers machine-readable.
- `artifacts/w-0122-models/`: bundle local đã xác minh, bị Git ignore.
- `artifacts/w-0122-voice-audition/`: 11 WAV + manifest local, bị Git ignore.

## Gate còn cần con người/hạ tầng

1. Owner nghe đủ 11 file qua Asterisk/MicroSIP 8 kHz và ký đúng một giọng Bắc/Trung/Nam.
2. Legal/Privacy phê duyệt bằng văn bản exact source/model/codec/preset/retention set.
3. Infra tạo internal mirror, ghim digest, cung cấp target CPU/RAM và chấp nhận SBOM/vulnerability disposition.
4. Platform + Telephony chốt `OD-VOICE-08` production media sink/topology.
5. Owner thực hiện 2 đơn × 3 miền, nghe nội dung/giọng/mối nối; sau đó mới chạy retention và rollback drill.

Không mục nào ở trên được suy ra từ local smoke hoặc file metadata.
