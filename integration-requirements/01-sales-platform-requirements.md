# IR-01 — Module 3 Requirements Register (mã ổn định)

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-09-03`
Owner: **Module 3** — `ginsengfood-business-platform` (Commerce/Order Core + Sales Extensions + CRM/Customer Identity)

> **File này là sổ đăng ký mã, không phải tài liệu bàn giao.**
> Nội dung chi tiết — endpoint, 22 field, payload mẫu, ACK taxonomy, checklist ký — nằm ở **[06-module-3-api-handover.md](06-module-3-api-handover.md)**. Đó là file gửi cho Module 3 và là **authority** khi hai file lệch nhau.
>
> IR-01 tồn tại vì mã `IR-SALES-*` được trích dẫn từ `seed/README.md`, `specs/api/06-error-codes.md`, `specs/data/00-index.md` và `plan/ivr-orther/production-blockers-plan.md`. Đổi hoặc xoá mã ở đây sẽ làm gãy các trích dẫn đó.

## 1. Sổ đăng ký

| ID | Yêu cầu | Prio | IVR chạy tạm bằng | Trạng thái | Chi tiết |
| --- | --- | --- | --- | --- | --- |
| `IR-SALES-TASK-01` | Sau khi Module 3 đã quyết định nghiệp vụ rằng đơn cần gọi, push task vào `POST /v1/ivr/order-confirmation/tasks` cho `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, với `ivr_confirmation_required=true` | P0 | intake API + fake producer | `TARGET_V1_DRAFT` — Giờ Vàng partial, **producer 24/7 COD chưa có** | [IR-06 §3.1–3.3](06-module-3-api-handover.md) |
| `IR-SALES-TASK-02` | Task mang `order_version`, window timestamps, policy version, eligibility evidence, call restriction | P0 | schema/validator/fixtures | `TARGET_V1_DRAFT` | [IR-06 §3.3](06-module-3-api-handover.md) |
| `IR-SALES-TASK-03` | **Ma trận `program × payment × order_state → callable` đã ký** + định nghĩa `ivr_confirmation_required` | P0 | ma trận IVR tự đặt (4 tầng enforce) | `OWNER_DECISION_REQUIRED` — ⚠️ nghi ngờ số 1 | [IR-06 §3.2](06-module-3-api-handover.md), [T-01](../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) |
| `IR-SALES-SPEECH-01` | `privacy_safe_order_summary`: tên ngắn, mã đơn ngắn, public item name + qty, tổng tiền, vùng giao rút gọn, program, locale | P0 | renderer/TTS port + fake summaries | `NOT_BUILT_UPSTREAM` | [IR-06 §3.5](06-module-3-api-handover.md) |
| `IR-SALES-DIAL-01` | `dial_token` không lộ raw phone, TTL phủ hết window, resolver tại telephony trust boundary | P0 | token-only persistence + fake resolver | `NOT_BUILT_UPSTREAM` — 4 phương án chưa chọn | [IR-06 §5](06-module-3-api-handover.md), [T-04](../docs/contracts/target-v1-closure-pack/T-04-dial-token.md) |
| `IR-SALES-CB-01` | Mở `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks` với auth/idempotency/correlation/version/result/evidence | P0 | target client + WireMock | `NOT_BUILT_UPSTREAM` — ⚠️ **endpoint generic chưa tồn tại**, 24/7 hiện không có lối trả kết quả | [IR-06 §4.1–4.2](06-module-3-api-handover.md) |
| `IR-SALES-CB-02` | ACK taxonomy: `200` accepted/duplicate/blocked/review · `409` stale/conflict · `422` invalid · `429`/`5xx` retryable | P0 | semantic mapping + retry/DLQ tests | `TARGET_V1_DRAFT` | [IR-06 §4.4](06-module-3-api-handover.md) |
| `IR-SALES-REV-01` | Revalidate idempotency, order id/version/state, program/payment, **blocker tồn kho/thu hồi (gọi ops)** và evidence trước transition | P0 | contract expectations | `TARGET_V1_DRAFT` — 🚨 xem cảnh báo §2 | [IR-06 §4.5](06-module-3-api-handover.md) |
| `IR-SALES-TIMEOUT-01` | No-answer không huỷ ngay; timeout worker revalidate rồi mới `EXPIRED`; technical exception không tính customer attempt | P0 | advisory result/no-transition tests | `TARGET_V1_DRAFT` | [IR-06 §4.3](06-module-3-api-handover.md), [T-06](../docs/contracts/target-v1-closure-pack/T-06-no-answer-timeout.md) |
| `IR-SALES-RISK-01` | `SUPERSEDED` bởi `OD-18`: Module 3 tự phân loại/ra quyết định; IVR không yêu cầu `risk_evidence_available` hay trust metadata để call/skip. `risk_flags` nếu gửi chỉ dùng scheduler/audit | — | không áp dụng | `SUPERSEDED` | [IR-06 §6](06-module-3-api-handover.md) |
| `IR-SALES-CRM-01` | **`DC-01`** — CRM Customer Identity sở hữu do-not-call; Module 3 hợp nhất vào `call_restriction`. Snapshot `PhucApu@a3aad246d986` có eligibility read trả `eligible/denyReason/suppressionMarkerId` và user-auth consent mutation, nhưng chưa có signed service proposal/read contract cho M3/IVR, detailed scope/effective fields hoặc ACK | P1 | `call_restriction` boolean trong task; IVR fail-closed | `READ_PRIMITIVE_PRESENT / SERVICE_CONTRACT_AND_PROPOSAL_NOT_SIGNED` | `decisions-log` `DC-01`; [W-0148](../docs/evidence/W-0148/README.md) |
| `IR-SALES-EVT-01` | **`DC-05`** — publish `ORDER_CONFIRMED` / `CANCELLED` / `EXPIRED` sau Core decision để CRM tự thông báo. IVR **không** gửi (`D-14`) | P2 | không có | `NOT_BUILT_UPSTREAM` | `decisions-log` `DC-05` |
| `IR-SALES-AUTH-01` | Service auth production: issuer / audience / scope / JWKS / TTL; mTLS yes-no; sandbox credential | P0 | mock JWT + auth abstraction | `OWNER_DECISION_REQUIRED` — Security/Platform, xem [IR-04](04-shared-auth-audit-requirements.md) | [IR-06 §7](06-module-3-api-handover.md) |
| `IR-SALES-OAS-01` | OpenAPI thật + examples + sandbox URL + compatibility/deprecation window + consumer-driven contract tests | P0 | pinned target OpenAPI + drift gate | `BLOCKED_EXTERNAL` | [T-08](../docs/contracts/target-v1-closure-pack/T-08-openapi-compat-cdc.md) |

## 2. 🚨 `IR-SALES-REV-01` nay gánh một mình

`OD-17` gỡ `sellable_status[]` khỏi IVR. Trước đó có hai tầng chặn đơn không bán được: IVR chặn trước khi quay số, và Module 3 chặn lúc revalidate. **Nay chỉ còn tầng thứ hai.**

Nếu Module 3 bỏ bước revalidate với ops khi nhận callback, không còn gì chặn việc xác nhận một đơn đã bị recall hoặc sale-lock — và IVR sẽ không phát hiện được, vì nó không còn nhìn thấy dữ liệu đó nữa.

`IR-SALES-REV-01` vì vậy đổi từ "một yêu cầu đúng đắn" thành **một yêu cầu chặn an toàn**.

## 3. Mã cũ đã bị bỏ

| Mã bị trích dẫn ở nơi khác | Thực tế |
| --- | --- |
| `IR-SALES-01` (`seed/README.md`) | → `IR-SALES-TASK-01` |
| `IR-SALES-OC1` (`specs/api/06`, `specs/data/00`) | → `IR-SALES-TASK-02` (`order_version` là stale guard) |
| `IR-CRM-01` (`seed/README.md`, `specs/architecture/05`) | → `IR-SALES-CRM-01` — CRM nằm **trong** Module 3, không phải owner riêng |
| `IR-OPS-01…07` | **Không còn** — `OD-17`, xem [IR-02](02-ops-core-requirements.md) |
| `IR-SALES-*-V1` (`production-blockers-plan.md`) | Hệ mã song song của file kế hoạch đó; không phải mã pack này |

## 4. Evidence Module 3 phải nộp để đóng

1. Ma trận program/payment/callable đã ký + định nghĩa `ivr_confirmation_required` (`IR-SALES-TASK-03`)
2. OpenAPI endpoint callback generic + bảng ACK (`IR-SALES-CB-01/02`)
3. Test chứng minh idempotency, stale version, state changed, timeout race (`IR-SALES-REV-01`, `IR-SALES-TIMEOUT-01`)
4. Sandbox URL + auth metadata + test credential (`IR-SALES-AUTH-01`)
5. Schema + fixtures `privacy_safe_order_summary` + privacy sign-off (`IR-SALES-SPEECH-01`)
6. Dial-token threat model + issue/resolve/TTL tests (`IR-SALES-DIAL-01`)
7. Signed `OPT-01..OPT-11`, CRM proposal/write/read/lifecycle/reversal contract, Legal approval và
   shared E2E chứng minh marker hiệu lực luôn thành `call_restriction=true` (`IR-SALES-CRM-01`)

Checklist đầy đủ có ô tick: [IR-06 §10](06-module-3-api-handover.md).
