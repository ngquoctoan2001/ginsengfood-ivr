# FR — Task Intake

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/04` (Order Core→IVR task contract), `docx` §5 (Entry Gate), §6 (Runtime Object Contract); `MASTER-03` (correlation/idempotency).

**Actor:** Order Core (producer) → IVR Runtime (consumer).
**Precondition:** Order là Official Order đủ điều kiện; task có correlation + idempotency.
**Trigger:** Order Core gửi `IvrConfirmationTaskV1`.
**Postcondition:** Tạo CallJob nếu hợp lệ, hoặc trả reject/hold; ghi audit/evidence.

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-INTAKE-001 | Chỉ nhận task từ **Order Core hoặc service được ủy quyền** (allowlist), authenticated | phase-8/02 FR-001, /11; docx §5 | Task từ caller lạ → `403` |
| FR-IVR-INTAKE-002 | Bắt buộc `correlation_id` + `idempotency_key`; thiếu → reject | phase-8/04 §5; MASTER-03 | Thiếu → `422/reject` |
| FR-IVR-INTAKE-003 | Xác thực entity là **Official Order** ở state **`CONFIRMING`** + **`payment_method_snapshot=COD`** (DS-01); có `order_id`, `order_code` | phase-8/04; docx §5; **DS-01** | Không phải CONFIRMING/không COD → reject (P0-IVR-001) |
| FR-IVR-INTAKE-004 | Kiểm `expires_at` chưa qua và không vượt program window | phase-8/04 §5 | Hết hạn → reject/stale |
| FR-IVR-INTAKE-005 | Kiểm `program_code ∈ {GOLDEN_HOUR, TWENTY_FOUR_SEVEN}` và attempt policy khớp program | phase-8/04; docx §8 | Mismatch → `409` policy mismatch |
| FR-IVR-INTAKE-006 | Nhận & lưu **snapshot** trường được phép (order, program, trust, contact, blocker, script, evidence/privacy policy version) — chỉ dữ liệu privacy-safe | phase-8/04 §4, /02 §11 | Có trường cấm (full address/payment) → reject |
| FR-IVR-INTAKE-007 | Idempotency: same key+payload → trả kết quả cũ; same key+payload khác → conflict; retry an toàn cùng key | phase-8/04 §13 | Duplicate → không tạo job mới |
| FR-IVR-INTAKE-008 | Trả **intake decision** chuẩn (taxonomy dưới) và ghi audit/evidence | phase-8/04 §6,§7 | Có audit record cho mọi intake |
| FR-IVR-INTAKE-009 | Assert invariants: `not_for_quote_cart_draft`, `no_direct_order_update`, `call_purpose=ORDER_CONFIRMATION_ONLY`, `input_signal_only` (hoặc validation tương đương server-side) | phase-8/04 §10 | Thiếu invariant → guard chặn |

## Intake decision taxonomy (phase-8/04 §6,§12)
`TASK_ACCEPTED_CALL_JOB_CREATED` · `TASK_ACCEPTED_DRY_RUN_ONLY` (test/staging, no real SIM) · `TASK_SKIPPED_TRUSTED_CUSTOMER` · `TASK_REJECTED_NOT_OFFICIAL_ORDER` · `TASK_REJECTED_STATE_NOT_CALLABLE` · `TASK_REJECTED_POLICY_MISMATCH` · `TASK_REJECTED_CONTACT_INVALID` · `TASK_REJECTED_SCRIPT_NOT_APPROVED` · `TASK_REJECTED_INVALID_TRACE` · `TASK_BLOCKED_OPERATIONAL` · `TASK_HELD_ADMIN_REVIEW` · `TASK_HELD_POLICY_MISSING`.

## Quyết định đã khóa (Module 3, 2026-07-02)
- ✅ **D-03 (Q-S2):** Order Core **PUSH** sync command `POST /v1/ivr/order-confirmation/tasks`; bắt buộc `Idempotency-Key` + `X-Correlation-Id`; Core giữ retry kỹ thuật bounded.
- ✅ **D-01 + DS-01 (Q-F1):** Task IVR chạy trên order ở state **`CONFIRMING`** (đã có `order_code`); fulfillment khóa (shipment cần `CONFIRMED`) tới khi IVR confirm. 🆕 **CHỈ đơn COD** (`payment_method_snapshot=COD`) — DS-01.
- ✅ **D-02 + DS-01 (Q-S1):** `is_ivr_callable` = **Order Core derive** (`order_status=CONFIRMING && payment_method=COD`), không phải field bắt buộc/source-of-truth riêng; nếu gửi chỉ là convenience flag. IVR không suy diễn transition. ⚠️ **DS-04:** `order_version` chưa expose trong current contract → race-guard là target IR-SALES-OC1.
