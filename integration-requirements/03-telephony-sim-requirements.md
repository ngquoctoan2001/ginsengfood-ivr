# IR-03 — Telephony / SIM Gateway Requirements

Trạng thái: `REQUIREMENTS_DRAFT` · Cập nhật: `2026-08-12`.

## 1. Lộ trình đã được owner chỉ đạo

| Giai đoạn | Channel | Phạm vi |
| --- | --- | --- |
| Dev | mock | không gọi thật |
| Lab hiện tại | **1 SIM thật** | chỉ số trong allowlist, không gọi khách |
| Production target | **32 eSIM channels** | sau capacity/security/legal/release acceptance |

Số channel, concurrency, cooldown và rate limits phải cấu hình động; code/domain/database không được hard-code 1, 12 hay 32.

## 2. Yêu cầu adapter/vendor

| ID | Yêu cầu | Gate | Mock trước? | Trạng thái |
| --- | --- | --- | --- | --- |
| `IR-TEL-01` | Port `dial`, `play`, `capture_dtmf`, `hangup`, `disposition`, `health`; adapter vendor cô lập | code | có | port target |
| `IR-TEL-02` | Protocol/SDK/API, auth, timeout, webhook/poll semantics và version support | lab | có | `BLOCKED_EXTERNAL` |
| `IR-TEL-03` | Resolve `dial_token` tại trust boundary; IVR không persist/log raw phone | lab | fake resolver | `BLOCKED_EXTERNAL` |
| `IR-TEL-04` | DTMF 1/0 và invalid/no-input; xác định RFC2833/in-band/vendor event | lab | có | `BLOCKED_EXTERNAL` |
| `IR-TEL-05` | Disposition truth table: answered, busy, rejected, unreachable, invalid number, dropped, network/SIM/audio/DTMF error | lab | có | cần real verification |
| `IR-TEL-06` | One active call/channel, lease/fencing, cooldown, health, quarantine/auto-disable/alert | code+lab | có | target |
| `IR-TEL-07` | Lab destination allowlist + global kill switch; `REAL_CUSTOMER_CALL_ALLOWED=NO` | lab | có | hard gate |
| `IR-TEL-08` | 32 eSIM provisioning, measured concurrency/throughput, failover, cost/rate/caller ID | production | simulator | future procurement |
| `IR-TEL-09` | Recording OFF; nếu thay đổi cần consent/legal/retention riêng | all | n/a | locked off |

## 3. Lab acceptance với 1 SIM thật

- test allowlisted number answer + key 1;
- answer + key 0;
- no input/invalid key;
- busy/reject/unreachable nếu tái tạo được;
- adapter timeout/network failure và recovery;
- kill switch ngăn dispatch mới;
- không quá một active call trên channel;
- log/evidence không lộ raw phone/full address/audio;
- callback vẫn chạy qua fake/sandbox Sales theo mode cấu hình.

Lab pass không phải production proof và không mở gọi khách.

## 4. Thông tin cần vendor/Infra trả

Model/provider, protocol docs/SDK, endpoint topology, credential flow, dial-token resolver placement, DTMF mode, disposition codes, call concurrency/channel, rate limits, health/reconnect behavior, eSIM lifecycle, caller ID, cost, test SIM và số allowlist.
