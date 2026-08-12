# API-05 — Order Core Contracts

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/04` (task), `/07` (callback); D-01..D-06, D-10, DO-02, DO-CORR-1, DC-01/IR-CRM-01.

## 1. `IvrConfirmationTaskV1` (Order Core → IVR, PUSH sync — D-03)
Transport: `POST /v1/ivr/order-confirmation/tasks` do **Order Core** gọi; `Idempotency-Key` + `X-Correlation-Id` + `X-Source-System=order-core` (DF-06). Core giữ retry bounded (D-03).

| Field | Type | Bắt buộc | Nguồn/ghi chú |
| --- | --- | --- | --- |
| `task_id`, `task_version` | string | Có | Order Core; version `v1` |
| `idempotency_key`, `correlation_id` | string | Có | Trace/dedupe |
| `created_at`, `expires_at` | datetime | Có | `expires_at` = `T0 + window` (D-10) |
| `order_id`, `order_code`, `order_code_short` | string | Có | Official Order (D-01) |
| `order_version` | string/int | **Target only** | IR-SALES-OC1; current Core chưa expose (DS-04), không dùng làm required field hiện hành |
| `order_state` | enum (đục) | Có | IVR không suy diễn (D-02) |
| `payment_method_snapshot` | enum | Có | Chỉ chấp nhận `COD` cho IVR (DS-01); IVR không xử lý payment |
| `is_ivr_callable` | bool | Optional | Convenience flag do Core derive từ `CONFIRMING+COD`; không là source-of-truth riêng |
| `program_code` | enum | Có | `GOLDEN_HOUR`/`TWENTY_FOUR_SEVEN` |
| `max_attempts` | int | Có | **=2** cả hai (D-10) |
| `confirmation_window_seconds` | int | Có | GH **300** / 24-7 **900** (D-10) |
| `attempt_schedule` | array | Có | GH `[T0, T0+150]` / 24-7 `[T0, T0+450]` (D-10); `T0` = lúc Core mở window/tạo task |
| `customer_ref`, `customer_trust_status`, `trusted_skip_allowed`, `risk_flags[]` | — | Có | Trust Resolver (D-12); IVR chỉ consume boolean source-backed (D-13) |
| `official_contact_id`, `phone_ref`, `phone_masked`, `phone_validation_status` | — | Có | D-05 |
| `dial_token` | string | Có điều kiện | TTL ≤ window, one-use/attempt (D-05); mapping token→số ở SIM adapter, không ở IVR |
| `call_script_template_id`, `call_script_version`, `allowed_script_variables` | — | Có | Approved script |
| `sellable_status[]` | array | Có | **Per-line SKU/batch** SellableStatus snapshot (DO-02); Order Core fan-out (DO-CORR-1); mỗi phần tử: `sku_id`, `batch_id?`, `decision`, `recall_hold`, `sale_lock`, `quality_hold`, `stock_available`, `trace_ready`, `captured_at` |
| `call_restriction` / `opt_out` | bool/object | ✅ DC-01; P1 IR-CRM-01 | **do-not-call từ CRM** (không phải ops) — `crm-ads-eligibility` PHONE_CALL usable now; rich fields/Core wiring pending |
| `evidence_policy_version`, `privacy_policy_version` | string | Có | Governance |

Invariants task khẳng định (hoặc validation tương đương server-side): `not_for_quote_cart_draft=true`, `no_direct_order_update=true`, `call_purpose=ORDER_CONFIRMATION_ONLY`, `input_signal_only=true`, `order_state=CONFIRMING`, `payment_method_snapshot=COD`, `program_code`↔`max_attempts=2`↔window khớp (D-10). `order_version` là target IR-SALES-OC1, không required trong current contract.

## 2. Callback contract current/target (IVR → Order Core — D-04)
Transport: outbound `POST {orderCore}/v1/orders/{order_id}/ivr-result-callbacks`; `Idempotency-Key` + `X-Correlation-Id`.

OpenAPI tách hai mức để không trộn thực tế hiện tại với target race-guard:
- **Current:** `IvrConfirmationResultCallbackCurrentV1` + `CallbackCoreResponseCurrent` — không gửi `order_version_seen_by_ivr`; Core hiện nhận khi `CONFIRMING+COD`, accept `200` hoặc reject invalid/stale-by-state bằng `422` (DS-03/DS-04).
- **Target:** `IvrConfirmationResultCallbackTargetV1` + `CallbackCoreResponseTarget` — thêm `order_version_seen_by_ivr` và semantic `CALLBACK_*` codes (`CALLBACK_REJECTED_STALE`...), cần IR-SALES-OC1/OC2.

| Field | Bắt buộc | Ghi chú |
| --- | --- | --- |
| `callback_id`, `task_id`, `order_id` | Có | Link |
| `call_job_id`, `attempt_id` | Có điều kiện | Nếu đã tạo |
| `order_version_seen_by_ivr` | **Target only** | Race guard (D-02/D-04); current Core chưa nhận field này (DS-04) |
| `program_code`, `attempt_policy_code`, `attempt_no`, `max_attempts` | Có | Context (D-10) |
| `result_type` | Có | taxonomy `functional/05` + DT-02 |
| `result_reason`, `dtmf_key` | — | — |
| `is_counted_customer_attempt`, `is_final_for_ivr` | Có | DT-02 |
| `recommended_core_action` | Có | **advisory only** — Core revalidate (D-04) |
| `technical_error_code` | Có điều kiện | Nếu technical |
| `evidence_ref`, `audit_ref` | Có | Bắt buộc trước final action |
| `privacy_policy_version`, `script_version`, `created_at` | Có | — |

### Order Core revalidate (D-04)
Đồng bộ tối thiểu P0: idempotency, order state + COD gate, blocker (Core gọi ops sellable gate realtime — DO-03), evidence. **Response trong 3–5s.** Transition nội bộ có thể async. `CALLBACK_ACCEPTED_FOR_REVALIDATION` **≠ order confirmed**. Target IR-SALES-OC1 bổ sung `order_version` revalidation.

> ⚠️ **Thực tế Order Core hiện tại (DS-03/DS-04):** Core nhận result **chỉ khi `order_status=CONFIRMING` + `payment_method_snapshot=COD`**; else non-timeout → **`422`**. **Chưa hiện thực** bộ `CALLBACK_*` codes ở bảng dưới và **chưa** nhận `order_version_seen_by_ivr` (race-guard). ⇒ Bảng response codes dưới đây là **target** (IVR muốn Core thêm — IR-SALES-OC1/OC2); implement hiện tại: **OpenAPI current = 200 nếu accept, 422 nếu invalid**. Transition thật xem `workflows/09` (DS-02): confirm→CONFIRMED, cancel→CANCELLED, expiry→EXPIRED; no-answer/technical **không** transition (order chờ timeout→EXPIRED).

| Core response | Ý nghĩa | IVR action |
| --- | --- | --- |
| `CALLBACK_ACCEPTED_FOR_REVALIDATION` | Core nhận signal | mark accepted |
| `CALLBACK_REJECTED_STALE` | version/state stale | mark stale, không retry như signal mới |
| `CALLBACK_BLOCKED_BY_CORE` | blocker (sellable/recall/sale-lock/do-not-call) | mark blocked + evidence |
| `CALLBACK_NEEDS_ADMIN_REVIEW` | cần review | mark review |
| `CALLBACK_TECHNICAL_RETRY_ALLOWED` | Core transient | retry bounded **cùng idempotency key** (D-04) |
| `CALLBACK_TECHNICAL_RETRY_BLOCKED` | không an toàn/expired | admin review |

### Race guard (P0)
Khách bấm `1` nhưng blocker (recall/sale-lock) xuất hiện trước khi Core accept → result raw vẫn `IVR_CONFIRMED`, nhưng Core **block/hold**, KHÔNG confirm (D-06). Evidence link cả signal lẫn blocker.

## 3. Transition Core theo result (D-02)
`IVR_CONFIRMED`→tiếp tục nếu revalidate pass · `IVR_CUSTOMER_CANCELLED`→Core cancel · `IVR_NO_ANSWER_FINAL`→Core cancel/hold · `IVR_CONFIRMATION_WINDOW_EXPIRED`→expire/cancel/hold · `IVR_TECHNICAL_EXCEPTION`→admin review/retry, **không** tính no-answer. IVR không tự transition.
