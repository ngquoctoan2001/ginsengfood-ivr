# IR-01 — Sales Platform Requirements (Order Core M3 + Sales Extensions M3.1)

Trạng thái: `REQUIREMENTS` · Nguồn: D-01..D-14; `api/05-order-core-contracts`, `data/02`; `phase-8/04`,`/07`; `phase-3.1/07`.
✅ Contract đã được Order Core/Sales chốt (2026-07-02). Đây là **việc cần build/expose**.

## Order Core (Module 3)
| ID | Yêu cầu | Prio | I/O | sync/async | idempotency | mock? | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IR-SALES-01 | **Gọi task intake**: Order Core gọi `POST /v1/ivr/order-confirmation/tasks` với `IvrConfirmationTaskV1` (đủ field D-10/D-02/D-05 + **fan-out `sellable_status[]` per-line**) | P0 | out: task | sync (push) | có | có | Order Core | ✅ contract D-03/DO-02 |
| IR-SALES-02 | **Expose callback intake current** `POST /v1/orders/{id}/ivr-result-callbacks` + revalidate P0 đồng bộ (idempotency/state/COD/blocker/evidence) trong **3–5s**; response hiện là HTTP `200`/`422`, semantic `core_response_code` là target OC2 | P0 | in: callback; out: `200`/`422` current | sync + retry | có | có | Order Core | ✅ D-04/DS-03; version/code deltas ở OC1/OC2 |
| IR-SALES-03 | **order_code lifecycle**: cấp khi tạo Official Order; đơn `CONFIRMATION_REQUIRED/IVR_PENDING`; fulfillment/downstream **khóa** tới khi Core chấp nhận IVR signal | P0 | — | — | — | — | Order Core | ✅ D-01 |
| IR-SALES-04 | **Bàn giao order-state contract current**: `order_state`(enum đục) + COD gate (`payment_method_snapshot=COD`) + optional derived `is_ivr_callable`; **danh sách state IVR-callable + bảng transition per result** (`IVR_CONFIRMED`/`CANCELLED`/`NO_ANSWER_FINAL`/`WINDOW_EXPIRED`/`TECHNICAL`) | P0 | out: enum + transition table | — | — | — | Order Core | ✅ D-02 + **DG-03 resolved (DS-01..05)**; `order_version` nằm ở OC1 target |
| IR-SALES-05 | **Fan-out blocker**: Order Core tách order → dòng SKU/batch → gọi ops sellable gate → nhúng snapshot + **revalidate realtime** khi callback (Core là caller, không IVR) | P0 | — | sync | — | có | Order Core | ✅ DO-03/DO-CORR-1 |

## Order Core deltas — IVR muốn Core bổ sung (từ DS-01..05)
| ID | Yêu cầu | Prio | Trạng thái |
| --- | --- | --- | --- |
| IR-SALES-OC1 | **Expose `order_version`** trong `OrderDetailResponse` + task; callback DTO nhận `order_version_seen_by_ivr` → bật **race-guard** (nay chưa có — DS-04) | P1 | ⏳ GAP (JPA @Version có nội bộ) |
| IR-SALES-OC2 | **Richer callback response codes** (thay vì chỉ `422`): `ACCEPTED/STALE/BLOCKED/REVIEW/RETRY_*` (D-04) — nay Core reject `422` cho invalid, không có `CALLBACK_REJECTED_STALE` (DS-03) | P2 | ⏳ GAP (dùng 422 tạm) |
| IR-SALES-OC3 | (tuỳ chọn) **Explicit transition cho no-answer/technical**: nay `IVR_NO_ANSWER_FINAL`/`TECHNICAL` chỉ set `ivr_call_queue`, order chờ `timeout→EXPIRED` (DS-02). Nếu muốn hủy sớm thay vì đợi expire → Core thêm transition | P2 | ⏳ (hiện: order expire qua timeout) |
| IR-SALES-OC4 | **`is_ivr_callable` / COD gate**: Core chỉ tạo task khi `order_status=CONFIRMING` + `payment_method_snapshot=COD` (DS-01) — xác nhận Core enforce, IVR reject nếu lệch | P0 | ✅ rule rõ (DS-01); IVR intake enforce |

## Customer/Commerce (contact)
| ID | Yêu cầu | Prio | Ai build | Trạng thái |
| --- | --- | --- | --- | --- |
| IR-SALES-06 | **OfficialContactResolver**: cấp `phone_ref`/`phone_masked`/`phone_validation_status`/`dial_token` (TTL ≤ window, one-use/attempt); mapping token→số ở SIM vault, không ở IVR | P0 | Customer/Commerce | ✅ D-05 |

## Sales Extensions (Module 3.1)
| ID | Yêu cầu | Prio | I/O | sync/async | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- |
| IR-SALES-07 | **IVRRequiredDecision**: set cờ + `risk_reasons`/policy trên draft/order; phát **event `order.ivr_required_decisioned`** (Order Core mới tạo task; IVR không nhận trực tiếp) | P1 | out: event | async | Sales 3.1 | ✅ D-09 |
| IR-SALES-08 | **QuotaReleaseGuard**: nhận signal fail/expired (qua Core accept) → release quota Giờ Vàng; IVR không gọi API riêng | P1 | in: signal | async | Sales 3.1 | ✅ D-11 |
| IR-SALES-09 | **Customer Trust Resolver**: cấp `customer_trust_status`/`trusted_skip_allowed`/`risk_flags` (boolean source-backed); ngưỡng ở Risk Policy | P1 | out: trust fields | sync | Customer Trust (3.1/CRM) | ✅ D-12/D-13 |
| IR-SALES-10 | **CRM nhận outcome**: subscribe event sau Core decision; IVR không ghi CRM | P1 | in: event | async | CRM | ✅ D-14 |

## CRM / Customer Identity (Module 3.1) — build items (từ DC-01..06)
| ID | Yêu cầu | Prio | I/O | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- |
| IR-CRM-01 | **do-not-call read-contract**: bổ sung response `crm-ads-eligibility` (PHONE_CALL) trả `do_not_call/opt_out_scope/reason/effective_at` (nay chỉ `eligible/denyReason/suppressionMarkerId`); Order Core gọi & nhúng `call_restriction`; fail-closed | **P1** | in `{channelType=PHONE_CALL,category=TRANSACTIONAL,customerId|guestId,customerContactChannelId,policyVersionId}` → out eligibility | CRM/Customer Identity | ✅ DC-01 source resolved; ⏳ build extension/Core wiring |
| IR-CRM-02 | **Quiet/cap exemption**: IVR confirmation không áp CRM marketing quiet/frequency-cap; không đi qua automation rule; chỉ tôn trọng PHONE_CALL suppression | P1 | — | CRM | ✅ DC-03 (lock) |
| IR-CRM-03 | **Event sau Core decision**: implement publish `ORDER_CONFIRMED/CANCELLED/EXPIRED` (transition `ivr-confirm/ivr-reject/timeout`) + CRM notification template; IVR/SIM không gửi | P1 | out: event | Order Core + CRM | ⏳ DC-05 (chưa publish; notification no-op) |
| IR-CRM-04 | **CustomerTrustResolver**: build resolver/API trả `customer_trust_status/trusted_skip_allowed/risk_flags` cho Core (bật trusted-skip). Hiện chưa có → default require-IVR | P2 | out: trust | CRM/business-platform | ⏳ DC-06 (out-of-scope P3.2) |

## Deadline mong muốn
- P0 (IR-SALES-01..06): trước khi bật integration test thật; riêng IR-SALES-04 đã có DS-01..05.
- ✅ **DG-03 đã trả lời** (DS-01..05). Delta build còn lại: **IR-SALES-OC1** (race-guard P1) + **OC2/OC3** (P2 target).
