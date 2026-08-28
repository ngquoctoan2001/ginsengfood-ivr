# W-0122 — Tự host VieNeu-TTS, loại phụ thuộc SaaS TTS ở runtime

| Metadata | Giá trị |
| --- | --- |
| Trạng thái tài liệu | `OWNER_APPROVED_FOR_EXECUTION` |
| Trạng thái triển khai | `IN_PROGRESS — LOCAL_IMPLEMENTATION_READY; RELEASE_BLOCKED_BY_EXTERNAL_GATES_AND_BASE_OS_CVES` |
| Ngày lập | `2026-08-27` |
| Ngày rà soát/sửa plan | `2026-08-27` |
| Baseline source đã đọc | `main@f291f44` |
| Owner duyệt triển khai | `2026-08-27` |
| Origin | `UNPLANNED` — owner requested (`2026-08-27`) |
| Prereq | `W-0106 ACCEPTED`; `W-0108 TESTS_PASS` — còn nghiệm thu audio thật bởi owner; `W-0119 ACCEPTED` — chỉ xác nhận toolchain/sine-tone handoff; `W-0120 ACCEPTED` |
| Liên quan | `OD-VOICE-01`, `OD-VOICE-04`, `OD-V1-19`, `G-LEGAL`, `G-PLATFORM`, `G-DIAL`, `G-ESIM32` |

> Owner đã cấp quyền triển khai ngày `2026-08-27`; tracker đã ghi `W-0122 IN_PROGRESS` và
> Activity `A-0359 START`. Quyền này không thay thế các gate Legal/Privacy, voice listening,
> target-hardware performance, telephony, production topology hoặc release approval.

Tài liệu nguồn:

- [Execution tracker](../../prompt/_execution/prompt-execution-tracker.md)
- [W-0106 regional voice routing plan](W-0106-regional-voice-routing-plan.md)
- [W-0108 evidence và residual acceptance](../../docs/evidence/W-0108/README.md)
- [Speech synthesis orchestration](../../src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs)
- [External configurable provider](../../src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs)
- [Static-file provider](../../src/Ivr.Infrastructure/Speech/StaticFileTtsProvider.cs)
- [Media retention hook](../../src/Ivr.Infrastructure/Speech/AudioCache.cs)
- [Worker dispatch gateway](../../src/Ivr.Infrastructure/Telephony/AsteriskSchedulerDispatchGateway.cs)
- [Softphone lab Compose](../../docker-compose.softphone.yml)
- [Accepted fixed-segment converter](../../deploy/lab/Convert-LabSegmentAudio.ps1)
- [Asterisk lab entrypoint](../../deploy/lab/asterisk/entrypoint.sh)
- [Worker container identity](../../deploy/docker/Dockerfile.worker)
- [Worker safe defaults](../../src/Ivr.Worker/appsettings.json)

---

## 1. Mục tiêu và kết luận

### 1.1 Mục tiêu

Đưa một bản VieNeu-TTS đã ghim và kiểm soát provenance vào chuỗi build/deploy của IVR để:

1. tự tổng hợp tiếng Việt trong hạ tầng do dự án kiểm soát;
2. loại phụ thuộc **SaaS TTS ở runtime** sau khi vượt đủ gate;
3. giữ nguyên hợp đồng provider trung lập vendor của .NET;
4. tạo audio raw PCM đúng chuẩn Asterisk và có vòng đời dữ liệu hữu hạn;
5. giữ đường rollback về cấu hình/provider đã được chấp nhận trước đó.

“Tự host” ở đây **không** có nghĩa là hết phụ thuộc bên thứ ba. Source, model, tokenizer, Python
packages và base image vẫn là third-party supply chain. Mục tiêu là loại cuộc gọi ra SaaS trong
luồng runtime; build production chỉ độc lập với public upstream sau khi source, weights và image
đã được mirror nội bộ, ghim hash/digest và kiểm tra license.

### 1.2 Kết luận rà soát

Phương án khả thi dưới dạng **candidate có điều kiện**, không phải phê duyệt production:

- Không cần sửa C# cho luồng chính. `ConfigurableExternalTtsProvider` đã hỗ trợ HTTP loopback,
  JSON request và raw `audio/L16` response.
- Kiến trúc hiện tại chỉ đọc catalog cho `SpeechSegmentKind.Fixed`. Vì vậy W-0122 chỉ batch 4
  đoạn văn cố định × 3 miền thành 12 file `.wav` PCM s16le/8 kHz/mono theo toolchain W-0119;
  các đoạn items, tổng tiền và khu vực giao hàng tiếp tục được tổng hợp động qua external provider.
- VieNeu chạy cạnh **worker**, vì worker thực hiện synthesis/dispatch. API không cần sidecar TTS.
- Lab dùng `ivr-tts` chung network namespace với `ivr-worker`, không publish cổng. Worker gọi
  `http://127.0.0.1:<port>/synthesize`.
- Audio động do worker ghi phải có volume dùng chung với Asterisk; nếu không, Asterisk nhận
  media reference nhưng không thấy file.
- Shim không có cache vô hạn. Cache và file media tiếp tục bị giới hạn bởi confirmation window,
  `CacheMaximumTtlSeconds` và `SpeechSnapshotRetentionSeconds`.
- Tuyên bố upstream về Apache-2.0 và quyền dùng preset voice là đầu vào cho Legal, **không phải
  kết luận pháp lý của dự án**. `G-LEGAL` vẫn mở.
- `StaticFileTtsProvider` tiếp tục là `LAB_REAL_SIM`-only. W-0122 không gỡ guard này và không cần
  dùng provider đó để chạy catalog cố định.

### 1.3 Những hướng bị loại khỏi W-0122

| Hướng | Lý do loại |
| --- | --- |
| Batch từ số, 34 tỉnh/thành, SKU rồi lắp ghép runtime | Source hiện tại không có token assembly cho các segment động; tạo các file này sẽ thành artifact không được đọc |
| Cache audio địa chỉ/phường xã không thời hạn trong shim | Nội dung có thể là dữ liệu đơn hàng/cá nhân; trái retention hiện hành và shim không biết segment kind |
| Gỡ guard production của `StaticFileTtsProvider` | Không cần cho hybrid hiện tại và làm đổi trust boundary không có lợi ích |
| TTS Deployment riêng qua HTTP nội bộ | Không phải loopback nên bị validator từ chối; dùng HTTPS riêng sẽ thêm vận hành certificate |
| Gắn sidecar vào cả API và worker | Chỉ worker thực hiện synthesis/dispatch; gắn ở API tốn model/RAM và tăng bề mặt lỗi |
| Tải model public Hugging Face trong production build/runtime | Không tái lập và không sống sót khi upstream thay đổi/mất; phải dùng internal mirror |

Nếu sau này muốn token catalog cho số/tỉnh/SKU, phải mở work item riêng để thiết kế segmentation,
assembly, prosody, retention và test. Đây là thay đổi C#; phải chạy GitNexus impact trước khi sửa.

---

## 2. Baseline đã xác minh

### 2.1 Trạng thái work item và evidence

- `W-0106`: `ACCEPTED`.
- `W-0108`: `TESTS_PASS`, chưa có owner acceptance cho audio thật. W-0122 kế thừa residual này,
  không được ghi `ACCEPTED` chỉ từ sine tone, unit test hoặc file render thành công.
- `W-0119`: `ACCEPTED` cho toolchain/handoff bằng sine tone; không chứng minh chất giọng VieNeu.
- `W-0120`: `ACCEPTED`.
- `W-0122`: đúng next ID nhưng vẫn `NOT_STARTED` cho tới khi owner phê duyệt và tracker ghi
  `START` trong một bước riêng.

### 2.2 Hợp đồng provider hiện có

| Mục | Baseline bắt buộc |
| --- | --- |
| Request | `POST` JSON theo template, có text, voice ID, locale, speaking rate, output format, sample rate |
| Endpoint | HTTPS tuyệt đối, hoặc HTTP trên loopback |
| Response | raw PCM signed 16-bit little-endian, mono, `audio/L16`; không WAV/MP3/header |
| Asterisk | Audio động raw 8 kHz dùng `.sln`; fixed catalog giữ `.wav` PCM s16le/8 kHz theo toolchain đã nghiệm thu |
| File | content-addressed từ SHA-256 audio |
| Lỗi | fail closed; không đọc/log response body chứa dữ liệu nhạy cảm |
| Media reference | provider ghi file rồi trả prefix `sound:` đã cấu hình |

VieNeu phát 48 kHz float32 nên shim chuyển đổi là bắt buộc. Shim chỉ làm adapter protocol/audio,
không thay đổi domain/API contract của IVR.

### 2.3 Hành vi segmentation và retention hiện có

`SpeechSynthesisService` chỉ lấy catalog khi segment vừa là `Fixed` vừa bật catalog. Các segment
còn lại gọi provider runtime. Với kịch bản hiện tại:

| Nội dung | Kind/đường chạy | Cách W-0122 xử lý |
| --- | --- | --- |
| Lời chào/xác nhận cố định 1 | `Fixed` | Catalog `.wav` PCM 8 kHz |
| Câu dẫn trước danh sách món | `Fixed` | Catalog `.wav` PCM 8 kHz |
| Câu dẫn trước tổng tiền | `Fixed` | Catalog `.wav` PCM 8 kHz |
| Câu kết/thao tác phím | `Fixed` | Catalog `.wav` PCM 8 kHz |
| Items/SKU/số lượng | Dynamic | VieNeu qua external provider |
| Tổng tiền | Dynamic | VieNeu qua external provider |
| Khu vực giao hàng | Dynamic | VieNeu qua external provider |

Cache expiry là giá trị sớm nhất giữa confirmation window, `CacheMaximumTtlSeconds` và
`SpeechSnapshotRetentionSeconds` (baseline appsettings: 900 giây). `SpeechMediaFileRetentionHook`
xóa các file `.sln*` quá hạn. W-0122 không mở rộng TTL và shim không giữ bản audio/text thứ hai.

### 2.4 Candidate upstream cần được đóng băng trước audition

| Thành phần | Candidate quan sát ngày `2026-08-27` |
| --- | --- |
| Source repo | `pnnbao97/VieNeu-TTS` |
| Source commit đầy đủ | `36c4b501b0634a8f59805e6b529a058fbd30190b` |
| Model repo | `pnnbao-ump/VieNeu-TTS-v3-Turbo` |
| Model revision đầy đủ | `2da0efab622a1722125991736524f080b751ef5b` |
| Codec model transitively required | `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX` |
| Codec revision đầy đủ | `ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae` |
| Model card | SDK `v3.3.0`; roster phụ thuộc đúng SDK/source revision |
| Tuyên bố license upstream | Source có file Apache-2.0; hai model card khai báo Apache-2.0 |
| Khoảng trống đã xác minh | Hai model revision không chứa file `LICENSE`; training data không công bố/gated; quyền exact preset và distribution chưa có Legal acceptance |

Nguồn upstream để Phase 0 đối chiếu, không được dùng như floating dependency:

- [VieNeu-TTS-v3-Turbo model card](https://huggingface.co/pnnbao-ump/VieNeu-TTS-v3-Turbo)
- [MOSS Audio Tokenizer Nano ONNX model card](https://huggingface.co/OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX)
- [Voice manifest tại candidate source commit](https://github.com/pnnbao97/VieNeu-TTS/blob/36c4b501b0634a8f59805e6b529a058fbd30190b/src/vieneu/assets/voices_v3_turbo.json)
- [Upstream LICENSE tại candidate source commit](https://github.com/pnnbao97/VieNeu-TTS/blob/36c4b501b0634a8f59805e6b529a058fbd30190b/LICENSE)
- [Biến thể 0.3B-q4-gguf có license NC — phải bị từ chối](https://huggingface.co/pnnbao-ump/VieNeu-TTS-0.3B-q4-gguf)
- [Hugging Face revision pinning](https://huggingface.co/docs/huggingface_hub/main/guides/download)

Phase 0 đã xác nhận VieNeu ONNX gọi MOSS codec ở runtime; đây là dependency transitively required
không được liệt kê ở bản plan đầu. `MODELS.lock` hiện khóa 13 file của cả hai repo theo exact
path/size/SHA-256. Việc ghi SHA trong Markdown vẫn không thay thế gate machine-readable, và việc
model card khai báo license không thay thế file license/ý kiến Legal.

### 2.5 Checkpoint triển khai ngày 2026-08-27

| Hạng mục | Trạng thái/evidence |
| --- | --- |
| Source + dependency lock | Vendored exact commit/tree; `UPSTREAM.md`; source LICENSE hash đã khóa; production runtime lock 24 packages SHA-256 `a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304` |
| Model bundle | 13/13 artifact nonprod khớp path/size/SHA-256; production verifier fail đúng vì thiếu license-file evidence/internal mirror |
| Shim/container | Unit/negative `12/12`, contract container và real ONNX request pass; non-root/read-only/no-port/no-network selftest pass; thiếu/pending acceptance hoặc production audition đều readiness `503`; builder tách khỏi runtime, 9 nhóm tool/UI/training/test/sample bị loại; local image `sha256:4c76d318e24110267c908594031863cd7dbe3f31c92569c4e02b4de3ba9ba30d` (`126,109,579` bytes) — `STALE_REBUILD_REQUIRED` sau khi chuẩn hoá line ending ngày `2026-08-28` |
| Audition | 11/11 WAV PCM s16le/8 kHz/mono + tracked manifest; isolated Asterisk profile checksum/decode/route/deny probes pass; `PENDING_OWNER_MICROSIP_LISTENING` |
| Voice acceptance authority | Node/Python strict validator đọc chung `voices.json` làm declared authority + Compose bind + Helm ConfigMap mount; pending template, 9 acceptance mutation và 7 binding mutation fail closed; renderer/source/model/lock/audition/profile/results đều hash-bound; Owner artifact vẫn `NOT_RUN` |
| Converter | MP3 regression bitexact 12/12; WAV 12/12; unknown/missing source fail closed |
| Compose/media | Config/topology pass; UID 1654 write, Asterisk read-only/write-denied probe pass |
| Helm | Candidate sidecar/PVC/catalog wiring mặc định tắt; 4 default lint pass; positive `TEST_ONLY` fixture và negative guards pass |
| SBOM/vulnerability | SPDX 152 package entries; Trivy candidate cuối `13 HIGH`, `3 CRITICAL`, `0 fixable`, chỉ còn Debian 13.6; Python target `0 HIGH/CRITICAL`; `RELEASE_BLOCKED` chờ base fix/Security disposition |
| Fixed catalog/calls/retention/rollback | Chờ Owner chọn 3 voices; chưa được phép suy từ audition render |

### 2.6 Roster nữ tại candidate voice manifest

Candidate manifest có 20 preset voices, trong đó **11 giọng nữ**:

| Miền | Giọng nữ | Số lượng |
| --- | --- | ---: |
| Bắc | Trúc Ly; Ngọc Linh; Đoan Trang; Mai Anh; Quỳnh Anh; Ngọc Huyền | 6 |
| Trung | Ngọc Trân | 1 |
| Nam | Thục Đoan; Thùy Dung; Mỹ Duyên; Kim Thanh | 4 |

`Thục Đoan` được manifest candidate đánh dấu là nữ. Metadata chỉ dùng để lập danh sách audition;
owner phải nghe đúng tuyến 8 kHz. Nếu source/model/voice manifest/dependency lock thay đổi, toàn bộ
audition cũ mất hiệu lực và phải nghe lại.

---

## 3. Kiến trúc đã chọn

### 3.1 D1 — Hybrid đúng với source hiện tại

- `Segmentation.Enabled=true`.
- `Segmentation.FixedSegments=Catalog` cho đúng 4 đoạn fixed × 3 miền = 12 file `.wav` PCM
  s16le/8 kHz/mono do `Convert-LabSegmentAudio.ps1` sinh và manifest ghim.
- `Tts.Provider=EXTERNAL_CONFIGURABLE` cho items, tổng tiền và khu vực giao hàng.
- `StaticFileTtsProvider` không tham gia đường catalog và giữ nguyên lab-only guard.
- Không sửa domain, OpenAPI, database hay C# trong phạm vi W-0122 đã chọn.

Kiến trúc này giữ mối nối fixed ↔ dynamic trong playlist hiện hành. Acceptance phải nghe cả
độ tự nhiên của từng đoạn và prosody/âm lượng ở mối nối, không chỉ kiểm file tồn tại.

### 3.2 D2 — TTS sidecar cạnh worker

Worker là process dùng `AsteriskSchedulerDispatchGateway` và gọi speech synthesis. Vì thế:

- Lab: `ivr-tts` là Compose service nhưng dùng `network_mode: "service:ivr-worker"`; không có
  `ports:`. Worker gọi loopback.
- Kubernetes: container VieNeu nằm trong cùng Pod với worker, dùng loopback theo mô hình
  container cùng Pod. Không thêm vào API Deployment.
- Không tạo dependency cycle trong Compose. Runbook khởi động model, chờ `/health/ready`, rồi
  mới cho phép thực hiện lab call.

Tham chiếu networking: [Docker Compose networking](https://docs.docker.com/compose/how-tos/networking/)
và [Kubernetes networking](https://kubernetes.io/docs/concepts/services-networking/).

### 3.3 D3 — Media path dùng chung với Asterisk

Lab thêm named volume, ví dụ `ivr-speech-media`:

| Container | Mount | Quyền |
| --- | --- | --- |
| `ivr-worker` | `/var/lib/ivr/speech` | read-write |
| `asterisk` | `/var/lib/asterisk/sounds/generated` | read-only |

`read-write` trong Compose chỉ là mount mode, **không** cấp quyền POSIX. Worker image chạy UID
`1654`, nên lab phải thêm service one-shot `ivr-speech-media-init` dùng một utility image ghim
digest để tạo mount root với owner/group `1654:1654`, mode `0750`, rồi thoát. `ivr-worker` và
`asterisk` chỉ được start sau khi init service `service_completed_successfully`; Asterisk được cấp
đúng shared group chỉ để đọc. Không dùng `chmod 777` và không chạy worker dưới root.

Worker override:

```text
IVR_EXECUTION_MODE=LAB_REAL_SIM
Ivr__Speech__Tts__Provider=EXTERNAL_CONFIGURABLE
Ivr__Speech__Tts__OutputFormat=audio/L16
Ivr__Speech__Tts__SampleRate=8000
Ivr__Speech__Tts__External__Endpoint=http://127.0.0.1:<port>/synthesize
Ivr__Speech__Tts__External__MediaOutputDirectory=/var/lib/ivr/speech
Ivr__Speech__Tts__External__MediaReferencePrefix=sound:generated/
Ivr__Speech__Tts__RegionalVoices__Enabled=true
Ivr__Speech__Tts__Segmentation__Enabled=true
Ivr__Speech__Tts__Segmentation__FixedSegments=Catalog
```

Cặp `OutputFormat`/`SampleRate` phải override ngay trên worker; nếu để kế thừa anchor hiện tại thì
`audio/wav` làm external-provider validator từ chối startup. Cấu hình lab sinh từ acceptance
manifest còn phải điền exact `RequestBodyTemplate`, ba
`RegionalVoices.<Region>.VoiceId`/speaking rate và bốn `FixedSegments` entry của mỗi miền. Không
hard-code voice/model floating ngoài manifest đã ký.

Asterisk chỉ mount subdirectory `generated`, không mount đè toàn bộ sounds directory và không
che các audio tĩnh đã bake sẵn. Provider ghi `<digest>.sln`, nên prefix phải tạo đúng reference
`sound:generated/<digest>`; không thêm tiền tố filename mà provider không ghi ra disk. Test bắt
buộc chứng minh vòng tròn worker ghi → Asterisk đọc/phát → retention xóa.

Gate permission trước synthesis:

1. inspect xác nhận worker chạy UID `1654`, init volume có owner/group/mode đúng và Asterisk mount
   là read-only;
2. worker tạo được probe file trong `/var/lib/ivr/speech`, Asterisk đọc được cùng bytes tại
   `/var/lib/asterisk/sounds/generated`, nhưng Asterisk không ghi được;
3. xóa probe trước call và không ghi tên/text/audio đơn hàng ra log;
4. production dùng `runAsUser`/`runAsGroup`/`fsGroup` hoặc cơ chế CSI tương đương đã được Platform
   duyệt; không suy quyền lab sang Kubernetes.

Production chưa có Asterisk trong Helm worker Pod. Vì vậy media-sink topology là decision gate
`OD-VOICE-08`; không được copy lab volume sang production rồi gọi là hoàn tất.

### 3.4 D4 — Vendor source và mirror artifact, không floating fetch

Proposed tree sau khi plan được duyệt:

```text
third_party/vieneu-tts/
  LICENSE
  NOTICE                         # nếu upstream có
  UPSTREAM.md                    # URL, full commit, lấy ngày nào, local tree hash
  uv.lock                        # vendored dependency lock
  ...                            # source tại đúng commit
deploy/tts/
  Dockerfile.tts                 # base image ghim digest, install frozen lock
  shim/
    server.py
    convert.py
    voices.json
  models/
    MODELS.lock                  # metadata/allowlist; weights không vào git
  scripts/
    fetch-model-nonprod.*        # chỉ Phase 0, explicit nonprod flag
    verify-model.*
deploy/ci/scripts/
  tts-provenance-gate.*
  tts-container-selftest.*
  render-fixed-speech.*
```

Production image/build lấy weights từ internal artifact/OCI mirror đã ghim digest, không truy cập
public Hugging Face. Chỉ Phase 0 được tải candidate public khi bật cờ nonprod tường minh và vẫn
phải kiểm revision, path, size, SHA-256 trước khi dùng.

---

## 4. Thiết kế chi tiết và gates

### 4.1 Shim contract

```http
GET /health/live
GET /health/ready

POST /synthesize
Content-Type: application/json

{"text":"...","voice_id":"...","locale":"vi-VN","speaking_rate":1.0,
 "output_format":"audio/L16","sample_rate":8000}

200 Content-Type: audio/L16
<raw PCM s16le mono, no header>
```

Quy tắc fail-closed:

1. `/health/live` chỉ chứng minh process sống. `/health/ready` chỉ trả ready sau khi load đúng
   model revision, đúng voice manifest và deterministic startup smoke thành công.
2. Chỉ chấp nhận exact method/path/content type/schema, voice ID allowlist, `vi-VN`, format
   `audio/L16`, sample rate 8000 và speaking-rate trong range đã duyệt.
3. 4xx/5xx trả body rỗng. Không log request body, text, synthesized audio, traceback hay response
   body. Metrics chỉ dùng code, latency, queue depth và kích thước bucket hóa.
4. Bounded concurrency và bounded queue. Khi đầy hoặc chưa ready: fail closed; không tự động gọi
   SaaS/fallback provider và không treo vô hạn.
5. Shim không cache/persist text hoặc audio. Cache/file retention của .NET là authority duy nhất.

### 4.2 Chuyển đổi audio

#### Audio động từ shim

- Convert 48 kHz float32 → mono signed 16-bit little-endian 8 kHz với low-pass/resampler có kiểm
  soát aliasing; không decimate thô.
- Output không có WAV header; byte count chẵn; duration/size nằm trong limit provider.
- Kiểm bằng metadata parser và `ffmpeg`/`ffprobe` null sink; so sánh sample count/duration với
  expected tolerance.
- Loudness/prosody phải nghe trên MicroSIP qua tuyến Asterisk thật của lab. Không tự coi
  `loudnorm` hay waveform test là UX acceptance.
- Ghim tool name, version và command line trong artifact manifest để render có thể tái lập.

#### Fixed catalog

- Giữ nguyên **output contract** đã được W-0119 nghiệm thu:
  `Convert-LabSegmentAudio.ps1` sinh `ivr-seg-<region>-<text-hash-prefix>.wav` chứa PCM
  s16le/8 kHz/mono.
- Converter hiện hard-code input `.mp3`, còn VieNeu sinh source `.wav`. W-0122 mở rộng converter
  bằng explicit source manifest/extension để nhận `.wav`, không auto-guess; regression phải chứng
  minh đường `.mp3` W-0119 vẫn bitexact và mọi input lạ/thiếu bị fail-closed.
- Script phải cập nhật `SHA256SUMS`, `segments-manifest.txt`, `segments-compose-env.yml` và
  `segments-appsettings.json`; không chép tay text hash, duration hoặc media reference.
- Rebuild `ivr-asterisk-lab` sau khi 12 file được sinh. Boot phải pass toàn bộ `sha256sum --check`
  và entrypoint phải báo đã cài đúng 12 `ivr-seg-*.wav` vào Asterisk sounds directory.
- Catalog media reference phải trùng exact tên đã cài, không kèm extension. Asterisk/MicroSIP
  playback là gate cuối; metadata/decoder pass không thay thế việc nghe.

### 4.3 Voice acceptance manifest

Owner chốt đúng một preset cho mỗi miền sau khi nghe đủ 11 candidate. Đây là manifest **chọn
voice**, dùng để mở Phase 3; nó phải gắn với:

- source commit đầy đủ;
- model repo + full revision;
- hash voice manifest;
- hash `uv.lock`/dependency lock;
- hash production runtime lock và `MODELS.lock`;
- voice ID/tên preset cho từng miền;
- hash renderer audition, script text và audition manifest chứa exact size/SHA-256 của đủ 11 WAV;
- listening profile/tuyến Asterisk/MicroSIP 8 kHz đã khóa;
- người nghe, ngày nghe, thiết bị/tuyến lab, kết quả từng giọng.

Manifest chọn voice **không** chứa fixed-catalog hash vì 12 file đó chỉ được tạo sau khi Owner chọn
voice. Phase 3 sinh `source-manifest.json`, converter manifests/SHA256SUMS và Owner seam/call evidence
riêng để đóng acceptance catalog; không dùng manifest chọn voice để suy rằng catalog đã được nghe.

Runtime production/lab non-audition chỉ ready khi manifest riêng có `OWNER_ACCEPTED`, đủ 11 kết quả,
đúng ba voice khác nhau theo miền và khớp exact ba routing ID. Template pending/fixture `TEST_ONLY`,
manifest thiếu, extra field hoặc bất kỳ pin/hash trên đổi đều fail closed; bản đã ký phải chuyển
`STALE_RELISTEN_REQUIRED` và được nghe lại.

### 4.4 `MODELS.lock` và provenance gate

Mỗi artifact entry phải chứa tối thiểu:

```text
model_repo
full_revision
allowed_file_path
size_bytes
sha256
declared_spdx
license_file_sha256
voice_manifest_sha256
dependency_lock_sha256
internal_mirror_uri
internal_mirror_digest
```

Gate dùng **exact allowlist**, không dùng substring blacklist. Mọi repo, revision, path, size,
hash, license, voice manifest hoặc dependency-lock drift đều fail. Biến thể 0.3B-q4-gguf/NC phải
bị chặn như defense-in-depth, nhưng blacklist đó không thay thế allowlist.

Supply-chain outputs bắt buộc:

- giữ upstream `LICENSE` và `NOTICE` nếu có;
- third-party notice của image/package;
- SBOM cho source, Python dependencies, model/tokenizer artifacts và base image;
- vulnerability scan theo release policy;
- negative mutation tests: đổi revision/path/hash/license/extra file đều làm gate đỏ;
- reproducible/frozen dependency install; không resolve dependency mới trong production build.

### 4.5 Legal/privacy gate

Upstream model card/manifest là **tuyên bố của upstream**. Trước production, Legal/Privacy phải
đánh giá bằng văn bản:

- license source, weights, tokenizer và mọi artifact transitively required;
- quyền commercial use của đúng preset voices đã chọn;
- khoảng trống training data gated/không công bố;
- nghĩa vụ attribution/NOTICE/distribution;
- retention và xử lý text/audio đơn hàng;
- nếu dùng voice cloning sau này: consent, quyền giọng nói và vòng đời dữ liệu riêng.

Không đóng `G-LEGAL` từ CI xanh hoặc model card. Nếu không có phê duyệt, trạng thái production là
`RELEASE_BLOCKED`.

### 4.6 Runtime, performance và security gate

| Nhóm | Tiêu chí |
| --- | --- |
| Startup | Đo cold-start, model-load và warm-up; readiness chỉ mở sau deterministic smoke |
| Per-request latency | Trên CPU/memory target, p95 mỗi dynamic segment ≤ 80% `.NET TimeoutMilliseconds` |
| End-to-end latency | Đo p95 toàn bộ cold/warm `SpeechSynthesisService.SynthesizeAsync`; cold path gồm 3 dynamic requests tuần tự và phải còn ≥20% headroom trước lease deadline/pre-dial budget đã duyệt |
| Request budget | Với expected calls/minute và measured cache-miss ratio, `3 × cold calls/minute` cùng tổng ký tự phải nằm dưới `MaxRequestsPerMinute`/`MaxCharactersPerMinute` với ≥20% headroom |
| Memory | Peak RSS tối thiểu 25% dưới container memory limit |
| Load | Expected concurrency không 5xx, OOM, corrupt/truncated PCM; overload vượt capacity fail nhanh/có mã ổn định |
| Isolation | non-root, read-only root filesystem, drop Linux capabilities, seccomp, writable mount chỉ cho model/cache/media thật sự cần |
| Network | Không publish TTS port; production không public model fetch; egress theo allowlist/policy |
| Privacy | Không log text/body/audio/traceback; retention purge có evidence |
| Rollback | Rollback về previous image/config/provider là thao tác tường minh; không silent automatic SaaS fallback |

Không đặt trước capacity số học khi chưa đo trên target hardware. Infra phê duyệt measurement và
resource request/limit; test laptop không đại diện production.

`TimeoutMilliseconds` là timeout **mỗi provider request**, không phải timeout của cả playlist.
Vì ba dynamic segments được tổng hợp tuần tự trước khi dial, báo cáo performance phải tách cold
cache, warm cache, queue wait, inference time, conversion/write time và tổng pre-dial elapsed.
Không đạt end-to-end/lease headroom thì giảm concurrency hoặc tăng capacity; không chỉ tăng timeout.

Quy tắc này nay được Helm enforce ở render time thay vì chỉ nằm trong plan. Candidate ban đầu đặt
`worker.tts.timeoutMilliseconds=30000`, gấp 6 lần baseline `5000` đã nghiệm thu của worker và không
được ghi ở đâu; giá trị đã trả về `5000`. Chart tính `dynamicSegmentsPerCall × timeoutMilliseconds`
so với `preDialBudgetMilliseconds` (lease `120s` trừ `ExpectedCallDurationSeconds` `60s`) và fail
closed nếu còn dưới 20% headroom. Mọi giá trị vượt baseline còn phải có
`worker.tts.approvals.performanceRef` trỏ tới measurement trên target hardware — nâng timeout là
quyết định có bằng chứng, không phải một dòng values.

### 4.7 Procedure chứng minh retention trong lab

Retention mặc định của worker là `Enabled=false`, `DryRun=true`; vì vậy chỉ ghi “chờ purge” không
tạo được evidence. Phase 4 dùng procedure tách biệt sau khi toàn bộ call đã kết thúc:

1. khóa scheduler/dial và xác nhận không có playback đang chạy;
2. snapshot tên/hash/mtime của dynamic `.sln`, 12 fixed `.wav` và các audio baseline;
3. dùng utility container ghim digest trên cùng media volume để backdate **một file dynamic đã phát
   xong** quá `SpeechSnapshotRetentionSeconds`; giữ một dynamic file khác còn mới;
4. chạy one-shot worker với `Ivr__Retention__RunOnce=true`, `Enabled=true`, `DryRun=false`, cùng
   `MediaOutputDirectory`, trên **disposable lab DB**; không giảm production retention để làm test;
5. chứng minh file dynamic quá hạn bị xóa, file dynamic còn mới vẫn tồn tại, 12 fixed `.wav` và
   baseline audio không đổi hash; audit/report ghi `dryRun=false` nhưng không chứa text/audio;
6. trả retention config về safe default, bật lại scheduler sau khi đối chiếu volume sạch.

Nếu không có disposable DB hoặc không khóa được dispatch, gate giữ `NOT_RUN`; không chạy purge thật
trên DB/volume đang phục vụ call.

---

## 5. Kế hoạch thực hiện sau khi owner duyệt

### Phase 0 — Đóng băng provenance trước khi nghe

| # | Việc | Đầu ra/gate |
| --- | --- | --- |
| 0.1 | Ghi `START` cho W-0122 theo tracker trong thay đổi riêng | Work được cấp quyền thực thi |
| 0.2 | Fetch source/model candidate bằng full revision trong môi trường nonprod | Không dùng branch/tag floating |
| 0.3 | Kiểm toàn bộ artifact path/size/SHA-256, license/NOTICE, voice manifest và dependency lock | Draft `MODELS.lock`, `UPSTREAM.md`, provenance report |
| 0.4 | Tạo internal mirror và ghim digest | Production không phụ thuộc public upstream |
| 0.5 | Mở Legal intake với đúng artifact set | `OD-VOICE-07` mở; chưa tuyên bố clean |

Nếu artifact/license/hash khác candidate ghi ở §2.4: dừng, cập nhật plan/decision intake và xin duyệt
lại trước audition. Không “chọn bản gần giống”.

### Phase 1 — Audition 11 giọng nữ, owner chốt 3 miền

| # | Việc | Đầu ra/gate |
| --- | --- | --- |
| 1.1 | Render cùng một kịch bản versioned bằng đủ 11 giọng nữ candidate | 11 WAV nguồn + manifest/hash |
| 1.2 | Chuyển tất cả sang lab `.wav` PCM s16le/8 kHz/mono bằng tool/command đã ghim | Decoder/metadata validation + null-sink pass |
| 1.3 | Phát qua đúng Asterisk/MicroSIP lab, owner nghe và chấm | 11 kết quả nghe, không suy từ metadata |
| 1.4 | Owner chọn Bắc/Trung/Nam và ký acceptance manifest | Đóng `OD-VOICE-06` hoặc dừng |

`Ngọc Trân` là candidate nữ miền Trung duy nhất. Nếu không đạt, dừng W-0122 tại gate này; không
vendor shim/container để rồi mới phát hiện thiếu giọng. Voice clone chỉ có thể là work item khác
sau consent và Legal review; `OD-VOICE-04` vẫn mở.

### Phase 2 — Vendor, shim, container và supply-chain gates

| # | Việc | Đầu ra/gate |
| --- | --- | --- |
| 2.1 | Vendor đúng source commit, `uv.lock`, LICENSE/NOTICE và provenance metadata | Tree hash khớp lock |
| 2.2 | Xây shim contract/health/conversion, bounded queue và privacy-safe telemetry | Unit/contract/negative tests |
| 2.3 | Xây image từ base digest, frozen lock và internal model mirror | Reproducible image digest |
| 2.4 | Chạy exact-allowlist provenance/license gate, mutation tests | Drift/extra artifact fail closed |
| 2.5 | Sinh third-party notice, SBOM; chạy vulnerability/container security scan | Không còn policy blocker chưa disposition |
| 2.6 | Đo startup, per-request p95, RSS và overload trên target CPU | Per-request timeout/RSS/concurrency gate hoặc `ENV_BLOCKED` |
| 2.7 | Đo cold/warm full-playlist p95, queue wait, ba dynamic requests/call và request/character budgets | End-to-end lease/pre-dial headroom ≥20% hoặc `ENV_BLOCKED` |

### Phase 3 — Render catalog 12 đoạn fixed

| # | Việc | Đầu ra/gate |
| --- | --- | --- |
| 3.1 | Đọc đúng fixed segments từ manifest/script versioned hiện hành | Input hash ghi lại |
| 3.2 | Trước khi sửa converter, chạy GitNexus impact theo repo rule; thêm explicit `.wav` source manifest/extension và regression `.mp3` | Existing W-0119 MP3 path bitexact; WAV path + unknown/missing input tests pass |
| 3.3 | Render 4 fixed segments × 3 giọng đã duyệt | 12 source WAV đúng region/ordinal + hashes |
| 3.4 | Chạy `Convert-LabSegmentAudio.ps1` với explicit WAV input | 12 `.wav` PCM s16le/8 kHz/mono + `SHA256SUMS` + manifests + generated config |
| 3.5 | Rebuild Asterisk image; boot và kiểm checksum/install count | `sha256sum --check` pass; entrypoint cài đúng 12 `ivr-seg-*.wav` |
| 3.6 | Đối chiếu 12 catalog media references với 12 file thực trong Asterisk | Không thiếu/thừa/sai extension/hash/duration |
| 3.7 | Owner nghe cả đoạn và mối nối mẫu fixed ↔ dynamic | Owner audio approval; không chỉ CI pass |

Không mở rộng Phase 3 sang số, tỉnh/thành hoặc SKU trong W-0122.

### Phase 4 — Lab integration và real-audio acceptance

| # | Việc | Đầu ra/gate |
| --- | --- | --- |
| 4.1 | Thêm `ivr-tts` chung network namespace với worker, không publish port | `docker compose config` pass; endpoint loopback |
| 4.2 | Thêm init service ghim digest + named media volume theo §3.3 | Owner/group/mode đúng; init hoàn tất trước worker/Asterisk |
| 4.3 | Chạy permission probe bằng runtime identities | Worker UID 1654 ghi được; Asterisk chỉ đọc; Asterisk write bị từ chối |
| 4.4 | Worker override explicit `audio/L16`/8 kHz, dùng external provider; API giữ safe baseline; bật fixed catalog | External validator pass; không sửa C# hoặc weaken non-lab defaults |
| 4.5 | Chờ live/ready và deterministic smoke trước call | Model/voice/hash đúng manifest |
| 4.6 | Thực hiện **6 MicroSIP calls: 2 đơn khác nhau × 3 miền** | Hai playlist hash khác nhau; đủ ba route |
| 4.7 | Owner nghe nội dung, giọng vùng, số/tiền/địa chỉ, âm lượng và mọi mối nối | Kết quả từng call có owner signoff |
| 4.8 | Chứng minh media round-trip | Worker write → Asterisk read/play đúng bytes/reference |
| 4.9 | Chạy retention procedure §4.7 sau khi khóa dispatch | Expired dynamic xóa; fresh dynamic/fixed/baseline giữ nguyên |
| 4.10 | Chạy rollback drill về previous image/provider config | Không silent fallback; thời gian và kết quả ghi lại |

Phase 4 cung cấp evidence còn thiếu cho `W-0108`; việc đổi trạng thái/accept W-0108 phải theo quy
trình tracker riêng. Sáu call lab không cấp quyền gọi khách thật.

### Phase 5 — Production readiness, chưa tự động deploy

| # | Việc | Gate bắt buộc |
| --- | --- | --- |
| 5.1 | Chốt worker-Pod sidecar resources/security/egress/health | `G-PLATFORM`, perf/security acceptance |
| 5.2 | Chốt production media sink để telephony consumer đọc được file | `OD-VOICE-08`, `G-DIAL` |
| 5.3 | Chốt internal mirror availability/backup/restore | Infra/supply-chain approval |
| 5.4 | Legal/Privacy phê duyệt đúng locked artifact/voice set | `OD-VOICE-07`, `G-LEGAL` |
| 5.5 | E-SIM32/telephony acceptance và release rehearsal | `G-ESIM32`, release owner |

Chỉ các topology hợp lệ cho `OD-VOICE-08` mới được xem xét: co-locate Asterisk/media consumer
trong worker Pod với shared `emptyDir`, hoặc một shared RWX/object/telephony-adapter solution đã
được platform/telephony owner duyệt. Helm hiện chỉ có API/worker; local filesystem riêng của worker
không phải production media sink.

---

## 6. Acceptance matrix

| Gate | Bằng chứng bắt buộc | Trạng thái tại thời điểm lập plan |
| --- | --- | --- |
| Plan authority | Owner duyệt plan; tracker ghi `START` | `PASS` |
| Provenance | Full pins, exact paths/sizes/hashes, frozen deps, internal mirror digest | `NONPROD_PASS — INTERNAL_MIRROR_REQUIRED` |
| Legal/privacy | Ý kiến bằng văn bản trên đúng locked artifact/voices/retention | `OWNER_DATA_REQUIRED` |
| Voice quality | Owner nghe 11 candidate qua 8 kHz và ký 3 lựa chọn; strict manifest gate khớp exact candidate | `FILES_READY; RUNTIME_GATE_PASS — OWNER_LISTENING_REQUIRED` |
| Shim correctness | Contract, explicit `audio/L16`/8 kHz override, raw PCM, health, no-log, overload/negative tests | `LOCAL_PASS` |
| Supply chain/security | SBOM, notice, vuln scan, non-root/read-only/drop caps/seccomp | `LOCAL_HARDENING_PASS; RELEASE_BLOCKED — 13 HIGH/3 CRITICAL BASE_OS_UNFIXED` |
| Performance | Target hardware per-request và cold/warm full-playlist p95; lease/request/character headroom; RSS/concurrency | `LOCAL_OBSERVATION_ONLY — ENV_BLOCKED` |
| Media permissions | Init owner/group/mode; worker UID 1654 write; Asterisk read-only; production fsGroup/CSI decision | `LAB_PASS — PRODUCTION_DECISION_REQUIRED` |
| Fixed catalog | 12 `.wav` PCM 8 kHz + checksums/manifests + rebuilt Asterisk + install count + owner listening | `BLOCKED_BY_OD-VOICE-06` |
| Lab real audio | 2 đơn × 3 miền, content/region/seam approval | `NOT_RUN` |
| Media lifecycle | Worker write → Asterisk play; controlled one-shot purge xóa expired dynamic nhưng giữ fresh/fixed/baseline | `NOT_RUN` |
| Rollback | Previous image/config/provider restored deliberately | `NOT_RUN` |
| Production topology | Sidecar + media sink + telephony routing được owner duyệt | `HELM_CANDIDATE_FAIL_CLOSED — OWNER_DECISION_REQUIRED` |
| Real customer call | Release/telephony/legal gates và quyền riêng | `NO` |

Không được tổng hợp các test xanh cục bộ thành `ACCEPTED`, `PRODUCTION_READY` hoặc
`REAL_CUSTOMER_CALL_ALLOWED=YES`.

---

## 7. Rủi ro và biện pháp

| ID | Rủi ro | Mức | Biện pháp/gate |
| --- | --- | --- | --- |
| R1 | Chỉ một candidate nữ miền Trung | Cao | Audition trước implementation; không đạt thì dừng |
| R2 | Training data gated/không công bố | Cao | `G-LEGAL`/`OD-VOICE-07`; không suy diễn từ Apache declaration |
| R3 | Nhầm model/license/revision hoặc thêm artifact ngoài lock | Cao | Exact allowlist + hash/size/license mutation tests |
| R4 | Volume là RW nhưng worker UID 1654 không có quyền POSIX, hoặc Asterisk không đọc được | Cao | Pinned init service, exact owner/group/mode, runtime-identity permission probe, production fsGroup/CSI gate |
| R5 | Rò text/audio qua shim logs/errors/cache | Cao | Empty error body, no body/text logging, no shim persistence, retention evidence |
| R6 | Python/ONNX image tăng CVE và tài nguyên | Trung bình | Frozen minimal image, SBOM/scan, target perf/RSS gate |
| R7 | Ba dynamic requests tuần tự làm cold playlist vượt lease/pre-dial budget dù từng request chưa timeout | Trung bình | Readiness warm-up, bounded queue, per-request + full-playlist cold/warm p95 và request-budget headroom ≥20% |
| R8 | PCM hợp lệ về byte nhưng nghe rè/to nhỏ/mối nối gãy | Trung bình | Resampler validation, null sink và owner nghe đúng tuyến 8 kHz |
| R9 | Internal mirror thiếu/không khôi phục được | Trung bình | Digest pin, availability/backup/restore drill trước production |
| R10 | Silent fallback che lỗi và tái tạo SaaS dependency | Trung bình | Fail closed; rollback/fallback chỉ bằng thao tác cấu hình có audit |
| R11 | Retention evidence vô hiệu vì `Enabled=false`/`DryRun=true`, hoặc purge nhầm khi call đang chạy | Cao | Khóa dispatch, disposable DB, backdate một dynamic file, one-shot `DryRun=false`, đối chiếu fresh/fixed/baseline |

---

## 8. Ranh giới bắt buộc

- `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi trong W-0122.
- Không clone giọng người thật/preset của vendor khác. Voice cloning cần consent và work item/Legal
  review riêng.
- Không đóng `OD-VOICE-04`; W-0122 chỉ dùng preset voices candidate nếu được chấp nhận.
- Không nhúng Python vào .NET process.
- Không đổi OpenAPI, domain, DB schema hay RBAC.
- Không sửa/gỡ `StaticFileTtsProvider` lab-only guard.
- Không cache vô hạn, không tăng retention và không lưu text/audio trong shim.
- Không đổi fixed catalog sang `.sln` trong W-0122; giữ pipeline `.wav` đã nghiệm thu. Raw `.sln`
  chỉ dành cho audio động từ external provider.
- Không dùng `chmod 777`, không chạy worker dưới root và không coi mount `rw` là bằng chứng quyền.
- Không chạy retention purge khi scheduler/playback còn hoạt động, trên DB không disposable hoặc
  chỉ với `DryRun=true` rồi gọi là đã chứng minh xóa.
- Không xóa ElevenLabs/previous provider artifacts/config trước khi lab acceptance và rollback drill
  hoàn tất. Sau đó việc gỡ SaaS dependency vẫn là release decision riêng.
- Không production deploy từ plan approval. Phase 5 gates phải đóng bằng evidence thật.
- Nếu implementation phát sinh nhu cầu sửa C#, dừng scope, ghi lý do và chạy GitNexus upstream
  impact trước khi owner duyệt mở rộng.

---

## 9. Open decisions

| ID | Quyết định | Owner | Điều kiện đóng |
| --- | --- | --- | --- |
| `OD-VOICE-06` | Chọn 3 preset voices Bắc/Trung/Nam | Owner | Nghe đủ 11 giọng qua MicroSIP 8 kHz trên exact locked candidate; ký manifest |
| `OD-VOICE-07` | Chấp nhận hay từ chối legal/privacy risk của exact artifact/voice set | Legal/Privacy + Owner | Ý kiến bằng văn bản gắn full pins/hashes và retention design |
| `OD-VOICE-08` | Production media-sink/topology cho worker sidecar → telephony consumer | Platform + Telephony + Release owner | Kiến trúc, threat model, durability/retention và E2E evidence được duyệt |

`OD-VOICE-08` **không** còn là câu hỏi gỡ `StaticFileTtsProvider` guard; guard đó giữ nguyên.

---

## 10. Gate tiếp theo sau local implementation

1. Owner chạy `deploy/lab/Start-W0122VoiceAudition.ps1 -SkipBuild`, gọi `12200`/`12201`–`12211`,
   nghe 11 WAV đã khóa qua Asterisk/MicroSIP 8 kHz, điền manifest và chọn đúng một giọng
   Bắc/Trung/Nam; nếu Ngọc Trân không đạt thì dừng.
2. Sau lựa chọn đó mới render/convert/install 12 fixed segments và thực hiện 2 đơn × 3 miền.
3. Legal/Privacy đánh giá exact VieNeu + MOSS + preset set; Infra tạo internal mirror/digest và
   cung cấp target hardware; Security/Release owner xử lý/disposition 13 HIGH + 3 CRITICAL base-OS
   chưa có fixed version; Platform/Telephony chốt `OD-VOICE-08`.
4. Chỉ sau real-audio calls mới chạy retention/rollback drill và cân nhắc release rehearsal.

Không sửa C#, không gọi W-0122 `ACCEPTED`/`PRODUCTION_READY`, và không đổi
`REAL_CUSTOMER_CALL_ALLOWED=NO` từ các local test trên.
