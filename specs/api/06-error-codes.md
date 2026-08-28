# API-06 — Error Codes

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §9; DO-06 (ops error codes); error envelope `01-conventions` §3.

## 1. HTTP mapping (IVR API)
| HTTP | Dùng khi |
| --- | --- |
| `400` | Sai cú pháp/format field |
| `401` | Thiếu auth |
| `403` | Caller không có quyền / không thuộc allowlist (DF-06) |
| `404` | Resource không tồn tại |
| `409` | Conflict: idempotency, policy (D-10), state/capacity; target thêm `order_version` (IR-SALES-TASK-02) |
| `422` | Hợp lệ cú pháp nhưng vi phạm business contract |
| `429` | Rate limit (nếu hỗ trợ) |
| `500` | Lỗi hệ thống ngoài dự kiến; **không** dùng để che business reject |

## 1b. Response model — khi nào `200 + decision` vs `4xx + envelope`
W-0129 khóa theo runtime hiện hành, không suy từ prefix của decision:

- `TaskIntakeEndpoint.ValidateSchema` chạy trước service. `ivr_confirmation_required=false` và cặp
  program/payment không được phép trả `400 IVR_MALFORMED_REQUEST`; chúng không tạo
  `IvrTaskIntakeResult`.
- Service outcome có `FailureCode=null` mới được serialize thành `200 IvrTaskIntakeResult`.
  Accepted/dry-run và policy hold đi theo nhánh này. Một số nhánh phòng vệ dành cho caller nội bộ
  cũng có thể mang `TASK_REJECTED_*`, nhưng không vì thế mà chúng trở thành public wire promise.
- Outcome có `FailureCode` được endpoint chuyển thành `4xx` error envelope. Contact invalid hiện là
  `422 IVR_CONTACT_INVALID`; attempt-policy mismatch là `409 IVR_POLICY_MISMATCH`.
- M3 phải rẽ nhánh theo HTTP trước; chỉ đọc `decision`/`blocked_reasons` khi body thật sự là
  `IvrTaskIntakeResult`. Không được giả định mọi `TASK_REJECTED_*` đều trả `200`.

`TASK_SKIPPED_TRUSTED_CUSTOMER` chỉ còn `LEGACY_READ`; runtime `draft.22` không emit (`OD-18`).

## 1c. Danh mục `code` ổn định (error envelope 4xx/5xx)
| `code` | HTTP | Dùng khi | Decision tương ứng (nếu có) |
| --- | --- | --- | --- |
| `IVR_UNAUTHENTICATED` | 401 | Thiếu/invalid auth | — |
| `IVR_FORBIDDEN_CALLER` | 403 | Không thuộc allowlist / thiếu permission (DF-06/DF-01) | — |
| `IVR_MALFORMED_REQUEST` | 400 | Sai cú pháp/format | — |
| `IVR_MISSING_TRACE` | 422 | Thiếu `idempotency_key`/`correlation_id` | `TASK_REJECTED_INVALID_TRACE` |
| `IVR_IDEMPOTENCY_CONFLICT` | 409 | Same key, khác payload | — |
| `IVR_VERSION_CONFLICT` | 409 | target IR-SALES-TASK-02: `order_version` stale/mismatch | (callback target → `REJECTED_STALE`) |
| `IVR_NOT_OFFICIAL_ORDER` | 422 | Entity không phải Official Order | `TASK_REJECTED_NOT_OFFICIAL_ORDER` |
| `IVR_STATE_NOT_CALLABLE` | 422 | state không `CONFIRMING` hoặc `payment_method_snapshot` không `COD`; `is_ivr_callable=false` nếu được gửi | `TASK_REJECTED_STATE_NOT_CALLABLE` |
| `IVR_POLICY_MISMATCH` | 409 | program/`max_attempts`/window lệch (D-10) | `TASK_REJECTED_POLICY_MISMATCH` |
| `IVR_CONTACT_INVALID` | 422 | phone/contact không hợp lệ | `TASK_REJECTED_CONTACT_INVALID` |
| `IVR_SCRIPT_NOT_APPROVED` | 422 | script/version chưa duyệt | `TASK_REJECTED_SCRIPT_NOT_APPROVED` |
| `IVR_PII_POLICY_VIOLATION` | 422 | payload/text chứa PII không được phép như phone hoặc địa chỉ đường phố đầy đủ | — |
| `IVR_OPERATIONAL_BLOCKED` | 409 | blocker active (do-not-call, eligibility snapshot blocked) | `TASK_BLOCKED_OPERATIONAL` |
| `IVR_NOT_FOUND` | 404 | resource không tồn tại | — |
| `IVR_RATE_LIMITED` | 429 | rate limit (nếu hỗ trợ) | — |
| `IVR_INTERNAL_ERROR` | 500 | lỗi hệ thống (không che business) | — |

> `code` là **chuỗi ổn định** (không đổi nghĩa giữa version). Admin action lỗi RBAC dùng `IVR_FORBIDDEN_CALLER` (403).

## 2. Business intake outcome codes (200 body — phase-8/04 §12)
Đây là enum decision của domain/DTO, không phải cam kết rằng mọi giá trị đều xuất hiện trong body
`200` trên route intake. Wire mapping authoritative là §1b và OpenAPI `responses`; `4xx` chỉ có
error envelope. `TASK_SKIPPED_TRUSTED_CUSTOMER` được giữ `LEGACY_READ` cho client/row cũ, không
phải active outcome.

## 2a. W-0129 — rejection-reason taxonomy và compatibility

Các mã dưới đây chi tiết hóa `TaskIntakeOutcome.BlockedReasons` ở service boundary. Chúng không
đổi decision, không tạo job và không đổi HTTP contract:

| Nhóm | Điều kiện đầu tiên thất bại | Decision service | Reason service | Wire hiện hành |
| --- | --- | --- | --- | --- |
| Policy | `ivr_confirmation_required != true` | `TASK_REJECTED_POLICY_MISMATCH` | `IVR_CONFIRMATION_REQUIRED_NOT_TRUE` | schema chặn trước service → `400 IVR_MALFORMED_REQUEST` |
| Policy | cặp `program_code × payment_method_snapshot` sai | `TASK_REJECTED_POLICY_MISMATCH` | `PROGRAM_PAYMENT_MATRIX_REJECTED` | schema chặn trước service → `400 IVR_MALFORMED_REQUEST` |
| Contact | `phone_validation_status != VALID` | `TASK_REJECTED_CONTACT_INVALID` | `PHONE_VALIDATION_STATUS_NOT_VALID` | `422 IVR_CONTACT_INVALID` |
| Contact | `phone_masked` không có `x`, `X` hoặc `*` | `TASK_REJECTED_CONTACT_INVALID` | `PHONE_MASKED_NOT_MASKED` | `422 IVR_CONTACT_INVALID` |
| Contact | dial token đã hết hạn tại intake | `TASK_REJECTED_CONTACT_INVALID` | `DIAL_TOKEN_ALREADY_EXPIRED` | `422 IVR_CONTACT_INVALID` |
| Contact | dial token còn hạn nhưng hết trước confirmation window | `TASK_REJECTED_CONTACT_INVALID` | `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` | `422 IVR_CONTACT_INVALID` |
| Contact | `phone_ref` có hình dạng số điện thoại thô | `TASK_REJECTED_CONTACT_INVALID` | `PHONE_REF_LOOKS_LIKE_RAW_PHONE` | `422 IVR_CONTACT_INVALID` |
| Contact | `dial_token` có hình dạng số điện thoại thô | `TASK_REJECTED_CONTACT_INVALID` | `DIAL_TOKEN_LOOKS_LIKE_RAW_PHONE` | `422 IVR_CONTACT_INVALID` |
| Contact | opaque reference vi phạm privacy guard | `TASK_REJECTED_CONTACT_INVALID` | `CONTACT_FAILED_PRIVACY_GUARD` | `422 IVR_CONTACT_INVALID` |

Compatibility:

- `blocked_reasons` vẫn là open `string[]`; OpenAPI không có enum mới và `draft.22` không đổi.
- `CONTACT_OR_DIAL_TOKEN_INVALID` là mã tổng quát cũ, service hiện không emit. Consumer phải có
  unknown/fallback handling thay vì exhaustive switch trên reason string.
- `PROGRAM_PAYMENT_MATRIX_REJECTED` giữ đúng nghĩa cho matrix; required flag có mã nội bộ riêng.
- M3 **chưa nhìn thấy** chín mã chi tiết qua public intake route theo mapping hiện hành. Muốn đưa
  safe reason vào error envelope hoặc đổi reject thành `200 decision` là thay đổi contract riêng,
  cần owner/M3 ký; W-0129 không tự mở rộng quyền đó.

## 2b. Eligibility advisory codes cũ

Toàn bộ nhóm `TRUSTED_CUSTOMER_SKIP`, `RISK_FLAGS_PRESENT_REQUIRE_IVR`,
`TRUST_RISK_EVIDENCE_UNAVAILABLE`, `TRUST_SKIP_VETOED_BY_SALES`,
`TRUST_SKIP_DISABLED_REQUIRE_IVR`, `TRUST_RESOLVER_VERSION_MISSING` và
`TRUST_RESOLVER_UNAVAILABLE` là `LEGACY_READ` / `SUPERSEDED` bởi `OD-18`. Active runtime không
emit nhóm advisory này. `risk_flags` chỉ còn dùng cho audit/scheduler priority; xem
[workflows/07](../workflows/07-trusted-skip.md).

## 3. Result taxonomy (callback — functional/05 + DT-02)
`IVR_CONFIRMED` · `IVR_CUSTOMER_CANCELLED` · `IVR_NO_ANSWER_ATTEMPT` · `IVR_NO_ANSWER_FINAL` · `IVR_CONFIRMATION_WINDOW_EXPIRED` · `IVR_INVALID_PHONE_FINAL` · `IVR_WRONG_INPUT` · `IVR_TECHNICAL_EXCEPTION` · `IVR_CAPACITY_EXCEPTION` · `IVR_OPERATIONAL_BLOCKED` · `IVR_POLICY_BLOCKED`.

## 5. Nguyên tắc
- `500` không che business reject (dùng `409`/`422` cho business).
- P2-8 bật `ThrowOnBadRequest` cho minimal API để JSON body malformed/thiếu required body được middleware đổi thành typed `400 IVR_MALFORMED_REQUEST`, không trả body rỗng mặc định của framework.
- Mọi lỗi kỹ thuật cuộc gọi → `IVR_TECHNICAL_EXCEPTION`, **không** thành no-answer (DT-02; P0-IVR-004).
- Error envelope thống nhất `{error:{code,message,details,correlationId}}` (đồng bộ ops — DO-06).

## Báo cáo (error)
- HTTP mapping (8) + **response model rõ (200-decision vs 4xx-envelope)** + **danh mục 18 `code` ổn định** (§1c); intake taxonomy 12 (5 → 200 body, 7 → 4xx); result taxonomy 11; consume 8 mã ops-core (fail-closed).
- ✅ Cập nhật review 2026-07-02: bổ sung §1b (response model) + §1c (stable code catalog) — gỡ nhập nhằng reject 4xx vs 200 và khai báo `code` cho envelope.
