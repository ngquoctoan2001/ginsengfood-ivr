# W-0104 — Free Asterisk/softphone telephony preflight

Ngày: 2026-08-20

Baseline: `main@ce49f73`

Neural A/B change baseline: `main@3cd7613`

Trạng thái: `TESTS_PASS` — code, automated gates và hai disposition MicroSIP `1/0` đã đạt; owner không chấp nhận eSpeak hoặc neural A/B Edge. Voice C ElevenLabs `Trung Caha` đã được sinh đúng script v2, pin checksum/voice ID và chạy đủ `1/0`; owner chưa ghi quyết định chất lượng cuối nên chưa `ACCEPTED`.

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

Các task trên chỉ chứa dữ liệu fake, không chứa số điện thoại, credential hay dữ liệu khách thật. Một lượt đối chứng không bấm phím kết thúc `IVR_NO_ANSWER_FINAL`; hai lượt click chính control `1/0` của MicroSIP được ARI thu và normalizer tạo đúng hai final result tương ứng.

## 4. Sự cố đã tìm và sửa trong preflight

- Image tối giản ban đầu thiếu các file cấu hình Asterisk bắt buộc; bổ sung cấu hình module/logger/http/RTP/extensions tối thiểu.
- INI MicroSIP ghi UTF-8 BOM khiến section đầu không được đọc; launcher chuyển sang ASCII cho dữ liệu cấu hình chỉ chứa ASCII.
- Runner cuộc gọi ban đầu phụ thuộc secret compose trong shell mới; chuyển probe runtime sang container đã chạy để không ghi hoặc tái sử dụng secret.
- Cấu hình ban đầu trộn MicroSIP SIP-INFO với endpoint `auto_info`, đồng thời cửa sổ DTMF chỉ 15 giây nên probe UI không ổn định. Profile cuối khóa MicroSIP `RFC2833`, Asterisk `rfc4733`, tăng lab-only capture window lên 60 giây và gửi `BM_CLICK` tới đúng control Windows; cả `1` và `0` sau đó PASS end-to-end.

## 5. Ranh giới kết luận

W-0104 chứng minh được preflight telephony software miễn phí, không chứng minh modem/SIM/PSTN/carrier/caller ID hay capacity 32 eSIM. Sales endpoint/auth/payload thật cũng không được chạy. Những mục đó vẫn thuộc W-0048 và các external gate hiện hữu.

W-0104 đã đạt `TESTS_PASS`. Owner đã từ chối nghiệm thu audio hiện tại vì giọng tổng hợp máy móc/cũ; kết quả này không phủ nhận luồng gọi, playback hoặc DTMF đã PASS, nhưng chặn `ACCEPTED` cho trải nghiệm lời thoại. Phương án thay thế và tiêu chí A/B nằm ở [`voice-modernization-proposal.md`](voice-modernization-proposal.md).

Ngày 2026-08-22, hai file neural A/B đã được sinh bằng cùng script fake, chuẩn hóa PCM signed 16-bit/8 kHz/mono, ghim checksum và phát thành công qua media reference hiện hữu. Hai lượt MicroSIP đều được owner bắt máy và tạo `IVR_CONFIRMED`; kiểm tra checksum A/B đều PASS trước mỗi lần chuyển file. `edge-tts 7.2.8` chỉ là công cụ sinh mẫu dev, không phải provider production. Owner đã nghe đủ A/B nhưng chưa ghi lựa chọn cuối trong tracker, vì vậy trạng thái vẫn là `TESTS_PASS`.

Sau khi từ chối cả A/B, owner chọn candidate ElevenLabs `Trung Caha`. Code có immutable script `v2-test-approved` và migration MOCK tương ứng; script nhận diện Ginsengfood, không đọc mã đơn, dùng hướng dẫn phím “một/không”, đồng thời lab seed dùng Giang/cháo sâm/khu vực Phú Khương. Bản MP3 mới đúng 302 ký tự có voice ID `ueSxRO0nLF1bj93J2hVt`, được chuyển thành voice C PCM signed 16-bit/8 kHz/mono và image kiểm checksum trước khi phát. Hai disposition MicroSIP mới đều PASS. Chi tiết ở [`voice-modernization-proposal.md`](voice-modernization-proposal.md#7-candidate-elevenlabs-và-script-v2).

Chỉ chuyển `ACCEPTED` sau khi owner xác nhận rõ bản Trung Caha vừa nghe tự nhiên, âm lượng/tốc độ phù hợp và đúng số tiền/sản phẩm/khu vực/phím bấm. DTMF `1/0` đã PASS nhưng không tự thay thế quyết định UX. Hướng dẫn tái hiện đầy đủ ở `deploy/lab/README.md`.
