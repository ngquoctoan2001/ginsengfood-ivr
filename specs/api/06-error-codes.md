# API-06 — Error Codes

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §9; DO-06 (ops error codes); error envelope `01-conventions` §3.

## 1. HTTP mapping (IVR API)
| HTTP | Dùng khi |
| --- | --- |
| `400` | Sai cú pháp/format field |
| `401` | Thiếu auth |
| `403` | Caller không có quyền / không thuộc allowlist (DF-06) |
| `404` | Resource không tồn tại |
| `409` | Conflict: idempotency, policy (D-10), state/capacity; target thêm `order_version` (IR-SALES-OC1) |
| `422` | Hợp lệ cú pháp nhưng vi phạm business contract |
| `429` | Rate limit (nếu hỗ trợ) |
| `500` | Lỗi hệ thống ngoài dự kiến; **không** dùng để che business reject |

## 1b. Response model — khi nào `200 + decision` vs `4xx + envelope`
Bám phase-8/11 §6. Quy tắc chốt để implementer không làm lệch:
- **`200` + body `IvrTaskIntakeResult.decision`** cho **kết quả nghiệp vụ thành công/soft**: `TASK_ACCEPTED_CALL_JOB_CREATED`, `TASK_ACCEPTED_DRY_RUN_ONLY`, `TASK_SKIPPED_TRUSTED_CUSTOMER`, `TASK_HELD_ADMIN_REVIEW`, `TASK_HELD_POLICY_MISSING` (Order Core consume như signal, không phải lỗi giao thức).
- **`4xx` + error envelope** (`code` = lý do) cho **reject cứng / protocol**: auth, allowlist, malformed, thiếu idempotency/correlation, duplicate conflict, không phải Official Order, state không callable, policy mismatch, contact invalid, script chưa duyệt, operational blocked.
- `HELD` = soft (200/202); `REJECTED`/`BLOCKED` = 4xx. `ACCEPTED*`/`SKIPPED` = 200.
- `NEED_CONFIRMATION` (nhẹ): nếu repo có convention "envelope 200 cho mọi outcome" thì có thể để tất cả trong 200-decision; mặc định theo phase-8/11 (reject = 4xx). Không chặn — chọn 1 và giữ nhất quán ở OpenAPI.

## 1c. Danh mục `code` ổn định (error envelope 4xx/5xx)
| `code` | HTTP | Dùng khi | Decision tương ứng (nếu có) |
| --- | --- | --- | --- |
| `IVR_UNAUTHENTICATED` | 401 | Thiếu/invalid auth | — |
| `IVR_FORBIDDEN_CALLER` | 403 | Không thuộc allowlist / thiếu permission (DF-06/DF-01) | — |
| `IVR_MALFORMED_REQUEST` | 400 | Sai cú pháp/format | — |
| `IVR_MISSING_TRACE` | 422 | Thiếu `idempotency_key`/`correlation_id` | `TASK_REJECTED_INVALID_TRACE` |
| `IVR_IDEMPOTENCY_CONFLICT` | 409 | Same key, khác payload | — |
| `IVR_VERSION_CONFLICT` | 409 | target IR-SALES-OC1: `order_version` stale/mismatch | (callback target → `REJECTED_STALE`) |
| `IVR_NOT_OFFICIAL_ORDER` | 422 | Entity không phải Official Order | `TASK_REJECTED_NOT_OFFICIAL_ORDER` |
| `IVR_STATE_NOT_CALLABLE` | 422 | state không `CONFIRMING` hoặc `payment_method_snapshot` không `COD`; `is_ivr_callable=false` nếu được gửi | `TASK_REJECTED_STATE_NOT_CALLABLE` |
| `IVR_POLICY_MISMATCH` | 409 | program/`max_attempts`/window lệch (D-10) | `TASK_REJECTED_POLICY_MISMATCH` |
| `IVR_CONTACT_INVALID` | 422 | phone/contact không hợp lệ | `TASK_REJECTED_CONTACT_INVALID` |
| `IVR_SCRIPT_NOT_APPROVED` | 422 | script/version chưa duyệt | `TASK_REJECTED_SCRIPT_NOT_APPROVED` |
| `IVR_PII_POLICY_VIOLATION` | 422 | payload/text chứa PII không được phép như phone hoặc địa chỉ đường phố đầy đủ | — |
| `IVR_OPERATIONAL_BLOCKED` | 409 | blocker active (sellable/recall/sale-lock/do-not-call) | `TASK_BLOCKED_OPERATIONAL` |
| `IVR_NOT_FOUND` | 404 | resource không tồn tại | — |
| `IVR_RATE_LIMITED` | 429 | rate limit (nếu hỗ trợ) | — |
| `IVR_INTERNAL_ERROR` | 500 | lỗi hệ thống (không che business) | — |

> `code` là **chuỗi ổn định** (không đổi nghĩa giữa version). Admin action lỗi RBAC dùng `IVR_FORBIDDEN_CALLER` (403).

## 2. Business intake outcome codes (200 body — phase-8/04 §12)
Đây là **toàn bộ taxonomy** intake. Theo §1b: **200 body** = `TASK_ACCEPTED_CALL_JOB_CREATED` · `TASK_ACCEPTED_DRY_RUN_ONLY` · `TASK_SKIPPED_TRUSTED_CUSTOMER` · `TASK_HELD_ADMIN_REVIEW` · `TASK_HELD_POLICY_MISSING`. **4xx envelope** (map sang `code` ở §1c) = `TASK_REJECTED_NOT_OFFICIAL_ORDER` · `TASK_REJECTED_STATE_NOT_CALLABLE` · `TASK_REJECTED_POLICY_MISMATCH` · `TASK_REJECTED_CONTACT_INVALID` · `TASK_REJECTED_SCRIPT_NOT_APPROVED` · `TASK_REJECTED_INVALID_TRACE` · `TASK_BLOCKED_OPERATIONAL`.

## 3. Result taxonomy (callback — functional/05 + DT-02)
`IVR_CONFIRMED` · `IVR_CUSTOMER_CANCELLED` · `IVR_NO_ANSWER_ATTEMPT` · `IVR_NO_ANSWER_FINAL` · `IVR_CONFIRMATION_WINDOW_EXPIRED` · `IVR_INVALID_PHONE_FINAL` · `IVR_WRONG_INPUT` · `IVR_TECHNICAL_EXCEPTION` · `IVR_CAPACITY_EXCEPTION` · `IVR_OPERATIONAL_BLOCKED`.

## 4. Consume ops-core error codes (khi Order Core revalidate blocker — DO-06)
Order Core gọi ops sellable gate; các mã ổn định IVR/Core phải hiểu để **fail-closed**:

| Ops-core code (DO-06) | Ý nghĩa | Xử lý |
| --- | --- | --- |
| `SALE_LOCK_ACTIVE` | Sale-lock (recall-triggered — DO-CORR-3) | block/hold, không confirm |
| `RECALL_IMPACT_ACTIVE` | Recall | block/hold |
| `SELLABLE_GATE_BLOCKED` | Gate chặn (gộp) | block/hold |
| `INVENTORY_NOT_SELLABLE` | Không đủ điều kiện bán | block/hold |
| `QUALITY_HOLD` | Giữ chất lượng | block/hold |
| `TRACE_GAP_DETECTED` | Thiếu trace | review |
| `RATE_LIMITED` (429) | Quá tải ops | retry bounded/fail-closed |
| `INTERNAL_ERROR` (500) / timeout / `/health/ready=503` | Ops không xác thực được | **fail-closed**: không dispatch/không confirm |

## 5. Nguyên tắc
- `500` không che business reject (dùng `409`/`422` cho business).
- Mọi lỗi kỹ thuật cuộc gọi → `IVR_TECHNICAL_EXCEPTION`, **không** thành no-answer (DT-02; P0-IVR-004).
- Error envelope thống nhất `{error:{code,message,details,correlationId}}` (đồng bộ ops — DO-06).

## Báo cáo (error)
- HTTP mapping (8) + **response model rõ (200-decision vs 4xx-envelope)** + **danh mục 16 `code` ổn định** (§1c); intake taxonomy 12 (5 → 200 body, 7 → 4xx); result taxonomy 10; consume 8 mã ops-core (fail-closed).
- ✅ Cập nhật review 2026-07-02: bổ sung §1b (response model) + §1c (stable code catalog) — gỡ nhập nhằng reject 4xx vs 200 và khai báo `code` cho envelope.
