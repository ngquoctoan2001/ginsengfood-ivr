# W-0122 — Evidence index cho VieNeu-TTS self-hosted

Ngày: `2026-08-27`  
Baseline triển khai: `main@f291f44` (đọc); commit W-0122 đầu tiên là `e1b6b4b` trên `54d285d`  
Trạng thái: `IN_PROGRESS — RELEASE_BLOCKED`; `OD-VOICE-06` **đã đóng** `2026-08-28`

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
| Dependency lock | SHA-256 `f04d0713ee2e0041fae1234064fad0f22958be712f852fc6464e1beb3a724b4e` |
| Production runtime lock | 24 packages; SHA-256 `a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304` |
| Hardened local image | `sha256:4c76d318e24110267c908594031863cd7dbe3f31c92569c4e02b4de3ba9ba30d`; `126,109,579` bytes — `STALE_REBUILD_REQUIRED`, xem re-pin bên dưới |

Model cards của cả hai model repo khai báo `Apache-2.0`, nhưng hai revision đã khóa không chứa
file `LICENSE`. Chưa có ý kiến Legal về commercial use của đúng preset voice/training-data gap,
và chưa có internal mirror URI/digest. Vì vậy production provenance giữ `RELEASE_BLOCKED`.

### Re-pin `2026-08-28` — chuẩn hoá line ending

Hai pin ban đầu được tính trên working tree Windows nên mang byte CRLF, trong khi blob đã
commit là LF: `third_party/vieneu-tts/uv.lock` (`bc375e3d…` → `f04d0713…`) và
`third_party/vieneu-tts/LICENSE` (`1eb85fc9…` → `c71d239d…`). Hệ quả đã tái lập được: trên
GitLab runner Linux `tts-provenance-gate.mjs` đỏ ngay lệnh đầu của job với `dependency lock
drift`, và image build từ checkout Linux làm shim raise `dependency lock binding drift` nên
`/health/ready` giữ `503` vĩnh viễn. Toàn bộ path được hash nay đã ghim `text eol=lf` trong
`.gitattributes`; pin tính lại từ đúng byte đã commit, kéo theo `model_lock_sha256` →
`voices.json` → `expectedArtifactSetSha256`/`expectedVoiceConfigSha256`/
`expectedAcceptanceTemplateSha256`. Nội dung artifact không đổi, chỉ đường ghi line ending.
Đã chứng minh checkout sạch trên Windows và byte trên Linux cho cùng một hash ở cả 9 file
được hash.

`sha256:4c76d318…` là image build từ working tree CRLF cũ nên không còn tái lập được. Phải
build lại và chạy lại SBOM/Trivy trên digest mới trước khi trích dẫn lại con số đó.

### Cách tái lập `audition_script_sha256`

Pin này là hash của **text đã trim** mà renderer nạp cho model, không phải hash byte của file,
nên `sha256sum docs/evidence/W-0122/audition-script.txt` cho giá trị khác (`c0e7e237…`). Gate nay
tự dựng lại giá trị từ file bằng đúng normalisation của renderer, nên sửa nội dung kịch bản
audition sẽ làm gate đỏ — trước đây đây là mắt xích duy nhất không được kiểm với file thật.
Lệnh tái lập:

```powershell
node -e "const {createHash}=require('node:crypto');const {readFileSync}=require('node:fs');console.log(createHash('sha256').update(Buffer.from(readFileSync('docs/evidence/W-0122/audition-script.txt','utf8').trim(),'utf8')).digest('hex'))"
```

## Evidence tự động hiện có

| Gate | Kết quả hiện tại |
| --- | --- |
| Source/model exact pin | `PASS` ở nonprod; 13/13 size + SHA-256 khớp |
| Production model verifier | `EXPECTED_FAIL` — thiếu license-file evidence và internal mirror |
| Provenance mutations | `PASS` — revision/path/hash/license/extra artifact/voice-config/acceptance-template drift đều bị từ chối |
| Converter regression | `PASS` — MP3 cũ bitexact 12/12; WAV 12/12 PCM s16le/8 kHz/mono; unknown/missing source bị từ chối |
| Owner-manifest gate | `PASS` fail-closed — pending template + 9 acceptance mutation + 7 voices.json binding mutation bị từ chối (gồm sửa thẳng `audition-script.txt`); production thiếu/pending manifest hoặc bật audition đều readiness `503` |
| Container contract | `PASS` — non-root `1654:1654`, read-only, no network, no exposed port, drop caps; chạy `15/15` shim test bên trong image — cùng bộ test ở dòng `Shim unit/negative` của `security-performance.md`, không phải hai lượt đếm |
| Minimal runtime content | `PASS` — không có `uv`, web UI, upstream deploy/docs/examples, training, tests hoặc reference samples; chỉ venv + source runtime + locks/license |
| Real ONNX smoke | `PASS` nonprod — ready; request `200`, raw `audio/L16` 8 kHz, 20,480 bytes; không phải voice acceptance |
| Voice audition render | `FILES_READY` — 11/11 WAV PCM s16le/8 kHz/mono, tracked manifest SHA-256 `0cfbeacf6a60403c974354fc205e12591c12304f5b68a0abcac5d40afb8326cf`; owner chưa nghe/ký |
| Owner audition profile | `READY` — probe `2026-08-28` dựng thật profile cô lập: verifier `11/11`, Asterisk healthy, dialplan `12200`/`12201`–`12211` load đủ, catch-all `Hangup`; `W0122_AUDITION_PROFILE_READY`. Owner chỉ còn mở MicroSIP và gọi |
| Asterisk/MicroSIP audition harness | `RUNTIME_PASS` — 11/11 checksum/decode, `12201` playback pass, catch-all hangup pass; Owner listening vẫn `NOT_RUN` |
| Compose/media permissions | `PASS` local — loopback/no port/shared volume; UID 1654 write, Asterisk read-only/write-denied |
| Converter regression | `PASS` — `Convert-LabSegmentAudio.ps1` chạy trong container PowerShell ghim digest: roster `4×3` khớp `speech-segments.json`, 7 input sai đều fail closed |
| Fixed-render guards | `PASS` — `render-fixed-speech.mjs` từ chối 6 input sai (non-loopback, pending template, thiếu manifest) và đi tới synthesis với manifest hợp lệ |
| Readiness path thật | `PASS` — 3 test drive `VieNeuBackend.load()` với bundle tổng hợp: 5 lock binding drift và 3 guard audition/acceptance đều fail closed |
| Helm candidate | `PASS` local render — mặc định tắt; positive TEST_ONLY fixture và negative prod/lab guards pass |
| Pre-dial budget guard | `PASS` — `timeoutMilliseconds` trả về baseline `5000`; nâng lên `30000` bị từ chối (thiếu `approvals.performanceRef`, và vẫn thủng budget kể cả khi có); `16000` có measurement ref thì render |
| SBOM/vulnerability | `RELEASE_BLOCKED` — SPDX 152 entries; Trivy `13 HIGH`, `3 CRITICAL`, `0 fixable`; toàn bộ finding còn lại thuộc Debian 13.6 |
| Owner voice acceptance | `OWNER_ACCEPTED` `2026-08-28` — Bắc `Ngọc Linh`, Trung `Ngọc Trân`, Nam `Mỹ Duyên`; manifest SHA-256 `90927e16…`; shim chuyển từ `503` sang `/health/ready=200` |
| Fixed catalog 12 file | `FILES_READY` — render `12/12` bằng ba giọng đã ký, convert PCM s16le/8 kHz/mono, `sha256sum --check --strict` `18/18 OK` trong image, entrypoint báo `installed 12 fixed speech segments`, 12 media reference đối chiếu khớp file/hash/TextHash. **Owner chưa nghe đoạn và mối nối** |
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
- `deploy/lab/New-W0122VoiceAcceptance.ps1`: sinh manifest Owner từ ba lựa chọn rồi chạy gate; từ chối nếu thiếu người nghe, tuyến nghe, approval reference, hoặc thiếu khẳng định đã nghe đủ 11 giọng.
- `audition-environment.md`: pin của lần render, cơ sở determinism và rủi ro evidence Phase 1 chỉ có một bản.
- `security-performance.md`: image digest, runtime lock, container/ONNX proof, SBOM, exact residual CVE và performance boundary.
- `deploy/tts/models/MODELS.lock`: allowlist/hashes và release blockers machine-readable.
- `artifacts/w-0122-models/`: bundle local đã xác minh, bị Git ignore.
- `artifacts/w-0122-voice-audition/`: 11 WAV + manifest local, bị Git ignore.

## Hai lỗi phát hiện khi chạy thật Phase 3 (`2026-08-28`)

1. **Converter ghi CRLF.** `Convert-LabSegmentAudio.ps1` dùng `Set-Content`, nối dòng bằng CRLF trên Windows. Entrypoint chạy `sha256sum --check --strict` trong container Linux, ở đó `` cuối dòng thành một phần tên file ⇒ **cả 18 dòng fail**. Cùng họ với F1: `.gitattributes` giữ bản commit ở LF nên `git status` vẫn sạch, nhưng `docker build` đọc working tree. Sửa ba lớp: ghi LF tường minh; converter tự đọc lại byte vừa ghi và throw nếu có CR (git normalise lúc commit nên CI không bao giờ thấy bản CRLF — chỗ duy nhất bắt được là trên máy đã ghi); thêm assertion vào `lab-converter-selftest.mjs`.
2. **Không build lại được image lab.** `asterisk-22.10.1.tar.gz` đã bị dời sang `old-releases/`; URL ghim trả `404`, `old-releases/` trả `200`. Image chỉ còn tồn tại nhờ bản build cũ trên máy này. Đã thêm fallback hai đường, giữ nguyên `ASTERISK_SHA256` nên provenance không đổi. Đây đúng kịch bản mà gate internal mirror của W-0122 đang lo, nhưng xảy ra ở một dependency chưa ai để ý.

## Gate còn cần con người/hạ tầng

Mỗi gate dưới đây nay có **một** hành động cụ thể và một phiếu để điền, thay vì một mô tả.

1. **Owner** nghe đủ 11 file qua Asterisk/MicroSIP 8 kHz rồi ký đúng một giọng Bắc/Trung/Nam.
   Profile đã được probe ngày `2026-08-28`; ký bằng [`New-W0122VoiceAcceptance.ps1`](../../../deploy/lab/New-W0122VoiceAcceptance.ps1), không sửa JSON tay.
2. **Legal/Privacy** — [`questions-to-legal-od-voice-07.md`](../../../plan/ivr-orther/questions-to-legal-od-voice-07.md).
3. **Security/Release** — 16 finding không có bản vá: [`questions-to-security-w0122-cve-disposition.md`](../../../plan/ivr-orther/questions-to-security-w0122-cve-disposition.md).
4. **Platform/Infra/Telephony** — internal mirror, target hardware, `OD-VOICE-08`: [`questions-to-platform-w0122-infrastructure.md`](../../../plan/ivr-orther/questions-to-platform-w0122-infrastructure.md).
5. Owner thực hiện 2 đơn × 3 miền, nghe nội dung/giọng/mối nối; sau đó mới chạy retention và rollback drill.

Không mục nào ở trên được suy ra từ local smoke hoặc file metadata.
