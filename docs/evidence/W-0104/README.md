# W-0104 — Free Asterisk/softphone telephony preflight

Ngày: 2026-08-20

Baseline: `main@ce49f73`

Neural A/B change baseline: `main@3cd7613`

Trạng thái: `ACCEPTED` — owner chấp nhận voice C ElevenLabs `Trung Caha` và lời chào trung tính “Xin chào Quý khách” ngày 2026-08-22. Immutable script `v3-test-approved`, migration, PCM 8 kHz và cả hai disposition MicroSIP `1/0` đã được kiểm lại end-to-end. Acceptance này chỉ áp dụng cho software lab bằng dữ liệu fake; không mở quyền gọi khách thật.

## 1. Phạm vi đã triển khai

- Asterisk 22.10.1 LTS Docker profile có base/source checksum đã ghim, ARI/Stasis, PJSIP endpoint `LAB-A`, RTP local và audio tiếng Việt sinh trong image.
- `AsteriskAriSimGateway`, ARI event pump, health/originate/playback/DTMF/disposition/hangup; Basic auth không nằm trong URL.
- `AsteriskSchedulerDispatchGateway` nối lease của scheduler vào `DispatchGate` trước mọi thao tác telephony.
- `STATIC_FILE` speech provider trả media reference an toàn `sound:ivr-lab-order-confirmation`; runtime không gọi dịch vụ TTS ngoài. Lượt tạo voice C chỉ gửi script fake đã duyệt tới ElevenLabs web app.
- Lab dial-token vault một lần, chỉ phân giải alias `LAB-A`; không có raw phone number.
- Idempotent channel provisioner `SIM-ASTERISK-001`, fake policy/feature-flag seed, fake order/task runner và MicroSIP portable launcher.
- MicroSIP archive/executable đều được kiểm SHA-256 đã ghim trước khi chạy.
- Cấu hình fail-closed ngoài lab, recording tắt và `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 2. Automated evidence

| Gate | Kết quả |
| --- | --- |
| Focused W-0104 unit tests | `5/5 PASS` |
| Unit suite | `262/262 PASS` |
| Integration suite | `177/177 PASS` |
| Contract suite | `22/22 PASS` |
| Chaos suite | `6/6 PASS` |
| Traceability | `336` test IDs, generated table synchronized |
| `dotnet format Ivr.sln --verify-no-changes --no-restore` | `PASS` |
| Docker Compose merged config | `PASS` |
| Asterisk image build + health check | `PASS` |
| PII scan | `PASS` — 292 files |
| Gitleaks staged diff | `PASS` — no leaks |
| GitNexus staged change scope | `CRITICAL` breadth — 38 files, 154 symbols, 22 flows; reviewed against full regression and runtime evidence |

Focused tests khóa các điểm: DI chỉ bật đúng profile lab, gate chặn thì gateway nhận zero call, static media ref, token one-use/alias-only và từ chối production/raw destination/recording.

## 3. Runtime evidence đã quan sát

| Check | Kết quả |
| --- | --- |
| Asterisk boot + 12 module `res_ari*` | `PASS` |
| MicroSIP contact `LAB-A` trạng thái `Avail` | `PASS` |
| ARI originate làm MicroSIP đổ chuông | `PASS` |
| Không bắt máy được chuẩn hóa thành `IVR_NO_ANSWER_FINAL` | `PASS` |
| MicroSIP bắt máy; attempt có `raw_call_status=ANSWERED` | `PASS` |
| Playback được ghi `audio_status=PLAYED`; RTP PCMU đi tới Windows client | `PASS` |
| DTMF `1` -> `IVR_CONFIRMED` | `PASS` — `TASK-LAB-20260820110825` |
| DTMF `0` -> `IVR_CUSTOMER_CANCELLED` | `PASS` — `TASK-LAB-20260820110858` |
| Neural A — `vi-VN-HoaiMyNeural`, PCM 8 kHz mono | `PASS` — `TASK-LAB-20260822013752` → `IVR_CONFIRMED` |
| Neural B — `vi-VN-NamMinhNeural`, PCM 8 kHz mono | `PASS` — `TASK-LAB-20260822013829` → `IVR_CONFIRMED` |
| Voice C — ElevenLabs `Trung Caha`, PCM 8 kHz mono, phím `1` | `PASS` — `TASK-LAB-20260822033915` → `IVR_CONFIRMED` |
| Voice C — ElevenLabs `Trung Caha`, PCM 8 kHz mono, phím `0` | `PASS` — `TASK-LAB-20260822034006` → `IVR_CUSTOMER_CANCELLED` |
| Voice C v3 — lời chào “Quý khách”, phím `1` | `PASS` — `TASK-LAB-20260822042001` → `IVR_CONFIRMED|true|true` |
| Voice C v3 — lời chào “Quý khách”, phím `0` | `PASS` — `TASK-LAB-20260822042024` → `IVR_CUSTOMER_CANCELLED|true|true` |

Các task trên chỉ chứa dữ liệu fake, không chứa số điện thoại, credential hay dữ liệu khách thật. Một lượt đối chứng không bấm phím kết thúc `IVR_NO_ANSWER_FINAL`; hai lượt click chính control `1/0` của MicroSIP được ARI thu và normalizer tạo đúng hai final result tương ứng.

## 4. Sự cố đã tìm và sửa trong preflight

- Image tối giản ban đầu thiếu các file cấu hình Asterisk bắt buộc; bổ sung cấu hình module/logger/http/RTP/extensions tối thiểu.
- INI MicroSIP ghi UTF-8 BOM khiến section đầu không được đọc; launcher chuyển sang ASCII cho dữ liệu cấu hình chỉ chứa ASCII.
- Runner cuộc gọi ban đầu phụ thuộc secret compose trong shell mới; chuyển probe runtime sang container đã chạy để không ghi hoặc tái sử dụng secret.
- Cấu hình ban đầu trộn MicroSIP SIP-INFO với endpoint `auto_info`, đồng thời cửa sổ DTMF chỉ 15 giây nên probe UI không ổn định. Profile cuối khóa MicroSIP `RFC2833`, Asterisk `rfc4733`, tăng lab-only capture window lên 60 giây và gửi `BM_CLICK` tới đúng control Windows; cả `1` và `0` sau đó PASS end-to-end.

## 5. Ranh giới kết luận

W-0104 chứng minh được preflight telephony software miễn phí, không chứng minh modem/SIM/PSTN/carrier/caller ID hay capacity 32 eSIM. Sales endpoint/auth/payload thật cũng không được chạy. Những mục đó vẫn thuộc W-0048 và các external gate hiện hữu.

Owner đã từ chối eSpeak và cả hai mẫu neural A/B Edge vì giọng máy móc/cũ. Kết quả này không phủ nhận luồng gọi, playback hoặc DTMF đã PASS; nó là lý do voice C ElevenLabs được bổ sung và kiểm lại. Lịch sử lựa chọn cùng tiêu chí audio nằm ở [`voice-modernization-proposal.md`](voice-modernization-proposal.md).

Ngày 2026-08-22, hai file neural A/B đã được sinh bằng cùng script fake, chuẩn hóa PCM signed 16-bit/8 kHz/mono, ghim checksum và phát thành công qua media reference hiện hữu. Hai lượt MicroSIP đều được owner bắt máy và tạo `IVR_CONFIRMED`; kiểm tra checksum A/B đều PASS trước mỗi lần chuyển file. `edge-tts 7.2.8` chỉ là công cụ sinh mẫu dev, không phải provider production. Ở checkpoint A/B owner chưa chọn variant nên trạng thái khi đó giữ `TESTS_PASS`; quyết định sau cùng là voice C như phần dưới.

Sau khi từ chối cả A/B, owner chọn ElevenLabs `Trung Caha`. Bản v2 đã chứng minh voice và hai disposition; sau đó owner đổi lời mở đầu thành “Xin chào Quý khách”. Code vì vậy giữ v1/v2 để replay và thêm immutable `v3-test-approved`, không render tên khách, không đọc mã đơn, vẫn chỉ đọc sản phẩm/tổng tiền/`delivery_area_short` và hướng dẫn phím “một/không”. MP3 v3 dài 16,770563 giây, SHA-256 `6f89c520236049d57d6e2147cd5b503a43106f7ee5b52afa2dab484abb691217`; PCM signed 16-bit/8 kHz/mono dài 16,770625 giây, SHA-256 `38a6cb92ef59e70d457d08cd048470443d910f1389dcfdf7fd5eea32a780818a`. Image kiểm checksum trước khi phát và hai task v3 ở bảng trên đều PASS. Chi tiết ở [`voice-modernization-proposal.md`](voice-modernization-proposal.md#7-voice-c-elevenlabs-và-script-v3).

Owner đã ghi rõ `W-0104 ACCEPTED` sau khi nghe voice C; lượt v3 tiếp theo xác nhận lời chào trung tính và cả hai DTMF vẫn đúng. `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên. Modem/SIM/PSTN/carrier/caller ID, capacity 32 eSIM, Sales API thật, quyền/licensing production của ElevenLabs và Privacy/Legal vẫn là các gate độc lập chưa đạt. Hướng dẫn tái hiện đầy đủ ở `deploy/lab/README.md`.
