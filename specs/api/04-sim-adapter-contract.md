# API-04 — SIM Adapter Contract (Internal — Adapter Port)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/06` (SIM adapter), `/12` (raw_call_event); DT-01 (adapter port), DT-02 (disposition), DT-05 (recording OFF).

⚠️ **SIM Gateway CHƯA MUA (sẽ mua).** Contract này định nghĩa **adapter port** độc lập protocol để dev/test chạy **mock/dry-run**; protocol phần cứng cụ thể (AT command / SIP-to-SIM / vendor API) điền khi mua (DT-01). Đây là contract **nội bộ**, KHÔNG public.

## 1. Adapter port — operations
| Operation | Input | Output |
| --- | --- | --- |
| `dial(attempt)` | `attempt_id`, `sim_channel_id`, `dial_token` (D-05), `script_template_id`+`script_version`, `allowed_script_variables` | `adapter_result_id`, `call_started_at` |
| `play_script` | script + biến được phép (`order_code_short`, `total_amount_display`, opt `customer_name_short`/`program_name`) | ack |
| `capture_dtmf` | timeout sau script | `raw_dtmf` (`1`/`0`/none/invalid), `dtmf_error?` |
| `report_disposition` | — | `raw_call_status`, `call_ended_at`, `call_duration`, `technical_error_code?` |
| `health` | `sim_channel_id` | `status`, `last_health_check_at`, `cooldown_until` |

## 2. Ràng buộc (P0)
- Adapter **KHÔNG** có credential ghi order, **không** gửi SMS (phase-8/02 FR-004; P0-IVR-005).
- Chỉ dùng `dial_token`/`phone_ref`; **không** nhận/lưu raw phone (D-05; P0-IVR-007). Token TTL ≤ window, one-use/attempt (D-05).
- `ONE_SIM_ONE_ACTIVE_CALL`; cooldown 5s; `fail_count≥3/10′` → disable+alert (DT-04).
- Recording **OFF** mặc định (DT-05); nếu bật, chỉ lưu `recording_ref` + retention (DF-07 PENDING).

## 3. Disposition mapping (DT-02 — LOCKED; re-verify khi có SIM)
Adapter trả `raw_call_status` → Result Normalizer ánh xạ (chi tiết `functional/06`):

| raw_call_status | → result | counted? |
| --- | --- | --- |
| answered + dtmf `1`/`0` | `IVR_CONFIRMED`/`IVR_CUSTOMER_CANCELLED` | có |
| answered, no key | `IVR_NO_ANSWER_ATTEMPT`/`IVR_WRONG_INPUT` | có |
| ring timeout / no-answer / **busy** / **rejected** | `IVR_NO_ANSWER` | có |
| unreachable / thuê bao không tồn tại / sai số | `IVR_INVALID_PHONE_FINAL` | không |
| no-dial-tone / SIM / audio / DTMF / network error / dropped | `IVR_TECHNICAL_EXCEPTION` | không |

> `rejected` (khách từ chối cuộc gọi) = `NO_ANSWER`, **KHÔNG** = cancel; gắn cờ review (opt-out signal tương lai). ⚠️ Danh sách `raw_call_status` thật phụ thuộc gateway → re-verify (DT-01).

## 4. Mock/dry-run mode
- `IVR_ADAPTER_MODE = MOCK | REAL`. MOCK phát các `raw_call_status` mô phỏng theo `seed/call-scenarios.sample.json` (p10) để chạy smoke mà không gọi thật. REAL chỉ bật sau khi mua SIM + release gate pass.

## Báo cáo (adapter)
- Adapter port 5 operation; disposition DT-02 locked; SIM protocol PENDING mua (DT-01) → mock. `NEED_CONFIRMATION`: telephony webhook provider **không dùng** ở mô hình internal SIM (chỉ xét nếu đổi provider — future).
