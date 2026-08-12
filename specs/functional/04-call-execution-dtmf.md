# FR — Call Execution & DTMF

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `docx` §9 (call script, biến được phép, DTMF), §10 (SIM gateway); `phase-8/06` (SIM adapter).

**Actor:** SIM Gateway Adapter + DTMF Capture Handler.
**Precondition:** SIM channel reserved cho attempt.
**Trigger:** Scheduler dispatch attempt.
**Postcondition:** Raw call event (status + DTMF) được capture, chuyển Result Normalizer.

## Call script
- CONFIRMED: `CALL_PURPOSE = ORDER_CONFIRMATION_ONLY`. Script ngắn chỉ để xác nhận đơn. Nguồn: docx §9.
- Mẫu chính thức (docx §9.1): *"Ginsengfood kính chào Mình. … Đơn hàng {{order_code_short}} … tổng thanh toán {{total_amount_display}}. Xác nhận bấm phím 1. Không đặt/hủy bấm phím 0. …"*

### Biến được phép trong script (docx §9)
| Biến | Trạng thái |
| --- | --- |
| `order_code_short` | ALLOWED |
| `total_amount_display` | ALLOWED |
| `customer_name_short` | OPTIONAL |
| `program_name` | OPTIONAL |
### Cấm đọc (docx §9)
`FULL_ADDRESS`, `MEMBER_TIER`/`DIAMOND_REFERRAL_INFO`, `PAYMENT_DETAIL`/`ORDER_HISTORY`, `AI_CONSULTATION_CONTENT`/`CRM_CONTENT`, `HEALTH_OR_SENSITIVE_NOTE`.

## DTMF key rule (docx §9)
| Phím | Ý nghĩa | Hành động |
| --- | --- | --- |
| `1` | Xác nhận đơn | → `IVR_CONFIRMED` (signal) |
| `0` | Không đặt/hủy | → `IVR_CUSTOMER_CANCELLED` (signal) |
| Không bấm | Không xác nhận hợp lệ | Xử lý theo attempt/window (no-answer) |
| Sai phím | Không input hợp lệ | `IVR_WRONG_INPUT` / `NO_VALID_INPUT` |
| Lỗi DTMF | Lỗi kỹ thuật | `IVR_TECHNICAL_EXCEPTION` (KHÔNG tính no-answer) |
| `9` | Human support | **NOT_ENABLED** giai đoạn đầu (AS-07) |

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-CALL-001 | Chỉ phát script đã approved (`call_script_template_id`+version) với biến được phép | phase-8/04; docx §9 | Script/biến chưa duyệt → reject |
| FR-IVR-CALL-002 | Chỉ dùng `phone_ref`/dial_token để gọi; không lộ raw phone | docx §9,§17 | Raw phone trong log → FAIL (P0-IVR-007) |
| FR-IVR-CALL-003 | Capture `dtmf_key` (1/0/không bấm/sai phím/lỗi) + call status (ringing/answered/completed/…) | docx §9,§10; phase-8/06 | Đủ raw event cho normalizer |
| FR-IVR-CALL-004 | Lỗi DTMF/audio/SIM = **technical exception**, KHÔNG phải no-answer | docx §9,§15 | Lỗi kỹ thuật → không cancel như no-answer (P0-IVR-004) |
| FR-IVR-CALL-005 | SIM adapter **không** có credential ghi order, không gửi SMS | phase-8/02 FR-004; docx §10 | Adapter thử ghi order → forbidden |
| FR-IVR-CALL-006 | Recording chỉ khi policy cho phép (mặc định OFF); nếu bật lưu `recording_ref` + retention + audit truy cập | docx §9,§17 | Recording OFF mặc định (AS-06) |
| FR-IVR-CALL-007 | Không đọc dữ liệu ngoài whitelist biến | docx §9 | Đọc field cấm → FAIL |

## Quyết định (2026-07-02)
- ✅ **OD-11 → DT-02 (LOCKED):** disposition mapping (xem `functional/06`); re-verify khi có SIM.
- ⏳ **OD-14 → DT-01 (PENDING procurement):** SIM chưa mua → thiết kế **adapter port** (dial/play/capture DTMF/disposition/health), dùng mock/dry-run; protocol điền khi mua.
- ✅ **OD-12 → DT-05 (LOCKED):** recording **OFF** mặc định; bật cần consent+legal.
