# IR-01 — Sales Platform / Order Core Requirements

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-12`
Owner cung cấp: Sales Platform/Order Core (Java). Consumer: IVR (.NET).

## 1. Chức năng bắt buộc

| ID | Yêu cầu Sales phải cung cấp | Priority | IVR build trước bằng mock | Trạng thái |
| --- | --- | --- | --- | --- |
| `IR-SALES-TASK-01` | Push task vào `POST /v1/ivr/order-confirmation/tasks` cho `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`; chỉ khi `ivr_confirmation_required=true` | P0 | intake API + fake producer | `TARGET_DRAFT`; GH partial, COD producer chưa có |
| `IR-SALES-TASK-02` | Task có `order_version`, callable/window timestamps, policy version, eligibility evidence và call restriction | P0 | schema/validator/fixtures | `TARGET_DRAFT` |
| `IR-SALES-SPEECH-01` | Cấp `privacy_safe_order_summary`: tên ngắn, mã đơn ngắn, public item names + qty, tổng tiền, vùng giao rút gọn, program name, locale | P0 | renderer/TTS port + fake summaries | `NOT_IMPLEMENTED_UPSTREAM` |
| `IR-SALES-DIAL-01` | Cấp `dial_token` không lộ raw phone, TTL trong window, preferably one-use/attempt; có resolver tại telephony trust boundary | P0 | token-only persistence + fake resolver | `NOT_IMPLEMENTED_UPSTREAM` |
| `IR-SALES-CB-01` | Nhận `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks` với auth/idempotency/correlation/version/result/evidence | P0 | target client + WireMock | `TARGET_DRAFT` |
| `IR-SALES-CB-02` | ACK: 200 accepted/duplicate/blocked/review; 409 stale/conflict; 422 invalid; 429/5xx retryable | P0 | semantic mapping + retry/DLQ tests | `TARGET_DRAFT` |
| `IR-SALES-REV-01` | Revalidate idempotency, order id/version/state, program/payment, sellable/recall/lock và evidence trước transition | P0 | contract expectations | `TARGET_DRAFT` |
| `IR-SALES-TIMEOUT-01` | No-answer callback không hủy ngay; timeout worker revalidate rồi mới `EXPIRED`; technical exception không tính customer attempt | P0 | advisory result/no-transition tests | `TARGET_DRAFT` |
| `IR-SALES-AUTH-01` | Service auth: issuer/audience/scope/JWKS/TTL; mTLS yes/no; sandbox credential | P0 | mock JWT + auth handler abstraction | `OWNER_DECISION_REQUIRED` |
| `IR-SALES-OAS-01` | OpenAPI thật, examples, sandbox/base URL, compatibility/deprecation window và consumer-driven contract tests | P0 | pinned target OpenAPI + drift gate | `BLOCKED_EXTERNAL` |

## 2. Program invariant

| Program | Payment | Điều kiện |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | Official Order + callable state + `ivr_confirmation_required=true`; IVR xác nhận ý định, không xác nhận payment |
| `TWENTY_FOUR_SEVEN` | `COD` | Official Order + callable state + `ivr_confirmation_required=true` |

Legacy `24_7` chỉ được normalize tại `CURRENT_COMPAT` boundary. Bất kỳ tổ hợp khác phải fail-closed.

## 3. Target task fields

Required: `contract_version`, `task_id`, `order_id`, `order_code`, `order_version`, `program_code`, `payment_method_snapshot`, `ivr_confirmation_required`, `confirmation_window_started_at`, `confirmation_window_expires_at`, `attempt_policy_version`, `max_customer_attempts`, `attempt_offsets_seconds`, `phone_ref`, `phone_masked`, `dial_token`, `dial_token_expires_at`, `privacy_safe_order_summary`, `call_restriction`, `eligibility_snapshot`, `evidence_ref`.

Headers required: `Authorization`, `Idempotency-Key`, `X-Correlation-Id`.

## 4. Speech payload privacy

- `delivery_area_short` tuyệt đối không phải full address.
- `items[].public_name` phải là tên công khai phù hợp để đọc; Sales chịu trách nhiệm normalize.
- Có limit/collapse policy khi đơn nhiều dòng; IVR không âm thầm bỏ tổng tiền hay đổi nghĩa đơn.
- Fake fixtures phải có: 1 item, nhiều item/collapse, Unicode/tên khó đọc, total lớn, thiếu field, PII violation.

## 5. Callback target và current compatibility

Target path: `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`.

Current source chỉ có `POST /api/v1/internal/ivr/golden-hour/callbacks`. IVR được phép có `CurrentGoldenHourCallbackAdapter`, nhưng:

- không coi nó là Target V1;
- không route 24/7 COD qua đó nếu Sales chưa xác nhận;
- không dùng raw phone;
- phải có contract tests tách riêng và feature flag;
- loại bỏ/disable dễ dàng khi generic endpoint sẵn.

## 6. No-answer và notification

`NO_ANSWER_FINAL` → callback advisory `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`. Core timeout worker mới quyết định `EXPIRED` sau revalidation. V1 **không yêu cầu SMS/CRM notification**; IVR không gửi.

## 7. Acceptance evidence cần từ Sales

1. Owner ký program/payment/flag matrix và D-10 policy.
2. OpenAPI + examples + error/ACK table.
3. Test chứng minh idempotency, stale version, state changed, blocked stock và timeout race.
4. Sandbox URL + auth metadata/test credential.
5. Payload speech privacy review và sample fixtures.
6. Dial-token threat model + issue/resolve/TTL tests.
