# W-0104 — Free Asterisk/softphone telephony preflight

Ngày: 2026-08-20

Baseline: `main@ce49f73`

Trạng thái: `TESTS_PASS` — code, automated gates và hai disposition MicroSIP `1/0` đã đạt; owner review ngày `2026-08-21` **không chấp nhận chất lượng giọng eSpeak**, nên chưa `ACCEPTED`.

## 1. Phạm vi đã triển khai

- Asterisk 22.10.1 LTS Docker profile có base/source checksum đã ghim, ARI/Stasis, PJSIP endpoint `LAB-A`, RTP local và audio tiếng Việt sinh trong image.
- `AsteriskAriSimGateway`, ARI event pump, health/originate/playback/DTMF/disposition/hangup; Basic auth không nằm trong URL.
- `AsteriskSchedulerDispatchGateway` nối lease của scheduler vào `DispatchGate` trước mọi thao tác telephony.
- `STATIC_FILE` speech provider trả media reference an toàn `sound:ivr-lab-order-confirmation`; không gửi/nắm giữ nội dung lời thoại ở dịch vụ ngoài.
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

Các task trên chỉ chứa dữ liệu fake, không chứa số điện thoại, credential hay dữ liệu khách thật. Một lượt đối chứng không bấm phím kết thúc `IVR_NO_ANSWER_FINAL`; hai lượt click chính control `1/0` của MicroSIP được ARI thu và normalizer tạo đúng hai final result tương ứng.

## 4. Sự cố đã tìm và sửa trong preflight

- Image tối giản ban đầu thiếu các file cấu hình Asterisk bắt buộc; bổ sung cấu hình module/logger/http/RTP/extensions tối thiểu.
- INI MicroSIP ghi UTF-8 BOM khiến section đầu không được đọc; launcher chuyển sang ASCII cho dữ liệu cấu hình chỉ chứa ASCII.
- Runner cuộc gọi ban đầu phụ thuộc secret compose trong shell mới; chuyển probe runtime sang container đã chạy để không ghi hoặc tái sử dụng secret.
- Cấu hình ban đầu trộn MicroSIP SIP-INFO với endpoint `auto_info`, đồng thời cửa sổ DTMF chỉ 15 giây nên probe UI không ổn định. Profile cuối khóa MicroSIP `RFC2833`, Asterisk `rfc4733`, tăng lab-only capture window lên 60 giây và gửi `BM_CLICK` tới đúng control Windows; cả `1` và `0` sau đó PASS end-to-end.

## 5. Ranh giới kết luận

W-0104 chứng minh được preflight telephony software miễn phí, không chứng minh modem/SIM/PSTN/carrier/caller ID hay capacity 32 eSIM. Sales endpoint/auth/payload thật cũng không được chạy. Những mục đó vẫn thuộc W-0048 và các external gate hiện hữu.

W-0104 đã đạt `TESTS_PASS`. Owner đã từ chối nghiệm thu audio hiện tại vì giọng tổng hợp máy móc/cũ; kết quả này không phủ nhận luồng gọi, playback hoặc DTMF đã PASS, nhưng chặn `ACCEPTED` cho trải nghiệm lời thoại. Phương án thay thế và tiêu chí A/B nằm ở [`voice-modernization-proposal.md`](voice-modernization-proposal.md).

Chỉ chuyển `ACCEPTED` sau khi owner nghe lại ít nhất hai neural voice, chọn một voice/version và xác nhận lời thoại rõ, tự nhiên, đúng số tiền/sản phẩm/khu vực/phím bấm. Hướng dẫn tái hiện đầy đủ ở `deploy/lab/README.md`.
