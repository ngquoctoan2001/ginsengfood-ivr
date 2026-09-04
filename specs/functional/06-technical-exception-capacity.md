# FR — Technical Exception & Capacity

Trạng thái: `TARGET_V1_DRAFT` · Disposition mapping is provisional until verified with the real gateway.
Nguồn: `docx` §15 (technical error boundary), §11 (capacity), §12 (scheduler); `phase-8/16` (NFR).

**Actor:** Technical Exception Handler + Capacity Incident Monitor.
**Precondition:** Đang gọi/scheduling.
**Trigger:** Lỗi kỹ thuật hoặc quá tải.
**Postcondition:** Route đúng (không nhầm no-answer); mở incident khi cần.

## Technical error boundary (docx §15) — P0
CONFIRMED: **Lỗi kỹ thuật tuyệt đối không được tính là khách không nghe.** Phải vào `IVR_TECHNICAL_EXCEPTION` + admin review hoặc technical retry bounded.

| Lỗi kỹ thuật | KHÔNG xử lý như | Route đúng |
| --- | --- | --- |
| `SIM_GATEWAY_ERROR` | CUSTOMER_NO_ANSWER | `IVR_TECHNICAL_EXCEPTION` |
| `SERVER_ERROR` | CUSTOMER_NO_ANSWER | `IVR_TECHNICAL_EXCEPTION` |
| `DTMF_CAPTURE_ERROR` | CUSTOMER_NO_ANSWER | `IVR_TECHNICAL_EXCEPTION` |
| `AUDIO_PLAYBACK_ERROR` | CUSTOMER_NO_ANSWER | `IVR_TECHNICAL_EXCEPTION` |
| `SIM_CHANNEL_FAILURE` | CUSTOMER_NO_ANSWER | Disable SIM + admin alert |
| `INTERNAL_CALLBACK_ERROR` | CUSTOMER_NO_ANSWER | Retry callback bounded + admin review |
| `SCHEDULER_ERROR` | CUSTOMER_NO_ANSWER | Capacity/technical incident |

> **Trạng thái thực thi (đối soát code `2026-09-04`, `W-0171`).** Cột "Route đúng" **đã được thực
> thi và cưỡng chế ở tầng DB**; bảy nhãn ở cột trái thì **chưa**.
>
> - `exception_type` và `technical_exception_type` là **`string` tự do** ở cả DB lẫn OpenAPI
>   (`{ type: string }`), không có CHECK constraint. Runtime ghi mã gốc của gateway — ví dụ
>   `ASTERISK_DIAL_TIMEOUT` (`src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs:136`) —
>   nên **không giá trị nào trong bảy nhãn trên từng xuất hiện trong dữ liệu**.
> - Hệ quả: đừng viết consumer lọc theo `exception_type = 'SIM_GATEWAY_ERROR'`. Muốn biết "đây có
>   phải lỗi kỹ thuật không", đọc `result_type`/`result_status` — đó mới là enum đóng (§DB-03 §4).
> - Đóng khoảng trống này cần một owner decision: hoặc khóa `exception_type` thành enum bảy giá
>   trị rồi map mã gateway vào, hoặc chấp nhận mã gateway là giá trị chuẩn và bỏ bảy nhãn khỏi
>   đặc tả. Chưa quyết thì giữ nguyên bảng trên làm **yêu cầu**, không phải mô tả hiện trạng.

## Call disposition mapping (candidate; re-verify with 1 real SIM/gateway)
Ánh xạ tín hiệu SIM/telco → result. Nguồn: docx §13,§15; quyết định DT-02 (`plan/ivr-orther/decisions-log.md`). ⚠️ SIM gateway **chưa mua** → khi mua phải re-verify disposition code thật (DT-01).

| Tín hiệu SIM/telco | Result | Counted? | Ghi chú |
| --- | --- | --- | --- |
| answered + DTMF `1`/`0` | `IVR_CONFIRMED`/`IVR_CUSTOMER_CANCELLED` | có | final |
| answered, hết window không bấm | `IVR_NO_ANSWER_ATTEMPT`/`IVR_WRONG_INPUT` | có | — |
| ring timeout / không nghe | `IVR_NO_ANSWER` | có | — |
| busy (máy bận) | `IVR_NO_ANSWER` | có | line bận = chưa tới khách |
| rejected (khách chủ động từ chối cuộc gọi) | `IVR_NO_ANSWER` | có | **KHÔNG** coi là cancel; flag review (opt-out signal tương lai) |
| unreachable / thuê bao không tồn tại / sai số | `IVR_INVALID_PHONE_FINAL` | **không** | final riêng (anti-fake wrong-number), không trộn no-answer |
| no dial tone / SIM / audio / DTMF / network error / dropped mid-call | `IVR_TECHNICAL_EXCEPTION` | **không** | (bảng trên) |
| capacity không xử lý kịp | `IVR_CAPACITY_EXCEPTION` | **không** | review |

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-TECH-001 | Mọi lỗi kỹ thuật → `IVR_TECHNICAL_EXCEPTION` với `is_counted_customer_attempt=false` | docx §15; phase-8/12 §6 | Lỗi không tăng attempt (P0-IVR-004) |
| FR-IVR-TECH-002 | Technical retry là kỹ thuật, có giới hạn (count/backoff — `Owner Decision Required` OD-10); không reset customer attempt | phase-8/07 §14; docx §15 | Retry bounded |
| FR-IVR-TECH-003 | `SIM_CHANNEL_FAILURE` → quarantine/disable + alert theo versioned config; candidate `fail_count ≥3/10′` chỉ MOCK/LAB | docx §10,§12 | SIM lỗi tự cô lập |
| FR-IVR-TECH-004 | `INTERNAL_CALLBACK_ERROR` → retry callback bounded cùng idempotency; hết retry → admin review | phase-8/07 §14 | Không duplicate transition |
| FR-IVR-CAP-001 | Mở `capacity_incident` khi pending/expired/missed-deadline vượt ngưỡng; **không im lặng để đơn hết hạn** | docx §11,§12 | Miss deadline không log → FAIL (P0) |
| FR-IVR-CAP-002 | Không nhận call job vượt capacity nếu chắc chắn miss deadline (Capacity Gate) | docx §7,§11 | Vượt capacity → incident + alert |
| FR-IVR-CAP-003 | Capacity incident chứa internal capacity-scope `session_id`, `program_code`, `active_sim_count`, `pending/expired/missed_deadline_count`, `shortage_reason`. Không map đè upstream Golden Hour session vào `session_id`; W-0146 đề xuất cột nullable riêng `golden_hour_session_id` sau chữ ký M3 | docx §6 + W-0146 | Incident đủ trường và giữ tách hai identity |
| FR-IVR-CAP-004 | Result khi capacity không xử lý kịp: `IVR_CAPACITY_EXCEPTION` (không tính no-answer) → Core/review | phase-8/07 §10 | Capacity → review |

### Đối soát code (`2026-09-04`, `W-0171`)

| ID | Trạng thái | Bằng chứng trong code |
| --- | --- | --- |
| FR-IVR-TECH-001 | ✅ thực thi, **cưỡng chế ở DB** | `ck_ivr_call_attempts_non_customer_not_counted` và `ck_ivr_call_results_non_customer_not_counted`: `IVR_TECHNICAL_EXCEPTION`/`IVR_CAPACITY_EXCEPTION`/`IVR_OPERATIONAL_BLOCKED`/`IVR_POLICY_BLOCKED` **không thể** có `is_counted_customer_attempt = true` |
| FR-IVR-TECH-002 | ✅ thực thi; ngưỡng vẫn chờ `OD-10` | `delivery_status` có `RETRY_PENDING`/`RETRY_EXHAUSTED`; `CallbackOutboxRepository.cs:214` |
| FR-IVR-TECH-003 | ✅ thực thi đúng candidate | `SimChannelFailurePolicy.cs`: `AutoDisableThreshold = 3`, `FailureWindow = 10 phút` — khớp candidate `≥3/10′` (`W-0144`) |
| FR-IVR-TECH-004 | ✅ thực thi | `CallbackDispatcher.cs:217` chuyển `RETRY_EXHAUSTED`; idempotency giữ ở outbox |
| Bảy nhãn `*_ERROR`/`*_FAILURE` | ❌ **chưa thực thi** | Xem cảnh báo ở §Technical error boundary |

Nói cách khác: **yêu cầu hành vi đã đạt, chỉ từ vựng nhãn là chưa.**

## Owner Decision
- Mapping disposition chỉ được khóa sau lab với **1 SIM thật**.
- Channel pool hiện tại: 1 SIM lab; production target **32 eSIM**, cấu hình động và cần capacity evidence.
- ⏳ OD-10 (technical retry count/backoff) — còn treo (đề xuất bounded; owner chốt số).
