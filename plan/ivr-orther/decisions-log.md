# Decisions Log — IVR (lịch sử quyết định + Target V1 overlay)

> **Hiệu lực 2026-08-12:** các quyết định `TV1-*` trong [target-contract-v1-draft.md](target-contract-v1-draft.md) là lớp điều khiển mới cho plan/spec/prompt. Chúng **supersede** mọi câu cũ nói toàn hệ thống “COD-only”, D-10 đã khóa, callback cũ là target cuối, notification phải build trong V1, hoặc pilot mặc định 12 SIM. Các bảng `D-*`/`DS-*` bên dưới được giữ nguyên làm lịch sử/current-compat; không được dùng để phủ định `TV1-*`.

## Target V1 overlay — trạng thái DRAFT

| ID | Quyết định điều khiển | Trạng thái |
| --- | --- | --- |
| `TV1-01` | Scope gồm `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, đều cần `ivr_confirmation_required=true`. | `TARGET_DRAFT` |
| `TV1-02` | Attempt policy 2 lần/5′/15′ chỉ là candidate cho MOCK/LAB; phải policy-versioned/configurable và chờ owner chốt trước production. | `OWNER_DECISION_REQUIRED` |
| `TV1-03` | IVR là service .NET tách biệt; Sales Java sở hữu eligibility, order truth, revalidation và transition. | `ACCEPTED_FOR_PLANNING` |
| `TV1-04` | Callback target: `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`; endpoint Golden Hour hiện tại là `CURRENT_COMPAT`. | `TARGET_DRAFT` |
| `TV1-05` | Target callback dùng service auth + idempotency/correlation + order version và ACK semantic theo Target V1. | `TARGET_DRAFT` |
| `TV1-06` | `NO_ANSWER_FINAL` không hủy ngay; Core chờ timeout/revalidate. | `TARGET_DRAFT` |
| `TV1-07` | SMS/notification bị tắt trong V1; IVR không gửi. | `IN_SCOPE_LOCK` |
| `TV1-08` | `privacy_safe_order_summary` là P0 integration dependency để đọc tên, đơn, mặt hàng, tổng tiền và vùng giao rút gọn. | `TARGET_DRAFT` |
| `TV1-09` | Hiện test bằng 1 SIM thật + allowlist; tương lai target 32 eSIM channels, cấu hình động. | `OWNER_DIRECTION` |
| `TV1-10` | Ba mode bắt buộc: `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL`. | `ACCEPTED_FOR_IMPLEMENTATION` |
| `TV1-11` | Dev dùng mock JWT; production target short-lived service JWT, mTLS chờ Security/Platform quyết định. | `TARGET_DRAFT` |
| `TV1-12` | CI provider là GitLab CI; entrypoint `.gitlab-ci.yml`, Merge Request pipeline và GitLab merge/protected-branch gates. Không dùng GitHub Actions cho IVR. | `OWNER_CONFIRMED` |

Nguồn chi tiết và tiêu chí closure: [target-contract-v1-draft.md](target-contract-v1-draft.md).

Nguồn: trả lời chính thức từ **Module 3 (Commerce Order Core)** và **Module 3.1 (Sales Extensions)** ngày 2026-07-02 (xem `questions-to-module-3-and-3.1.md`).
Trạng thái các quyết định dưới đây: **LOCKED** (đã có owner trả lời) — nâng từ `ASSUMPTION`/`NEED_CONFIRMATION` lên `CONFIRMED`.
Còn treo: các câu Ops-Core (`questions-to-ops-core.md`) và Foundation/Telephony.

## Module 3 — Commerce Order Core

| ID | Quyết định (LOCKED) | Thay cho open question |
| --- | --- | --- |
| **D-01** | `order_code` cấp **khi tạo Official Order**; đơn vào `CONFIRMATION_REQUIRED` / `IVR_PENDING`; **fulfillment/downstream bị khóa** tới khi Core nhận & chấp nhận IVR signal. Câu "không sinh order_code trước IVR pass" ở phase-3.1/07 = **"không release/verify downstream trước IVR pass"** (không phải chặn cấp order_code). | Q1 / Q-F1 / tension GAP-S3 → **RESOLVED** |
| **D-02** | Core trả `order_state` (enum đục), `order_version`, `is_ivr_callable`; **IVR không tự suy diễn transition**. Transition do Core quyết: `IVR_CONFIRMED`→tiếp tục nếu revalidate pass; `IVR_CUSTOMER_CANCELLED`→Core cancel; `IVR_NO_ANSWER_FINAL`→Core cancel/hold theo policy; `IVR_CONFIRMATION_WINDOW_EXPIRED`→Core expire/cancel/hold; `IVR_TECHNICAL_EXCEPTION`→admin review/retry, **không tính no-answer**. | Q2 / Q-S1 → **RESOLVED** |
| **D-03** | Order Core **PUSH** sync command `POST /v1/ivr/order-confirmation/tasks`; bắt buộc `Idempotency-Key`, `X-Correlation-Id`; **Core giữ retry kỹ thuật bounded**. | Q3 / Q-S2 → **RESOLVED** |
| **D-04** | Core revalidate **P0 đồng bộ tối thiểu**: idempotency, version, state, blocker, evidence; **response trong 3–5s**. Transition nội bộ có thể **async**. `CALLBACK_ACCEPTED_FOR_REVALIDATION` **≠ order confirmed**. IVR chỉ retry callback khi timeout/5xx/`TECHNICAL_RETRY_ALLOWED`, **cùng idempotency key**. | Q4 → **RESOLVED** |
| **D-05** | `OfficialContactResolver` (Customer/Commerce) cấp `phone_ref`, `phone_masked`, `phone_validation_status`, `dial_token`. Mapping token→số thật nằm trong **token vault / SIM adapter boundary**, **KHÔNG lưu ở IVR**. `dial_token` TTL ≤ confirmation window, **khuyến nghị one-use theo attempt**. | Q5 / Q-S3 → **RESOLVED** |
| **D-06** | **Order Core** revalidate realtime với Operational Core khi nhận callback. Nếu có Sale Lock/Recall/Suppression mới → Core **block/hold, không confirm dù khách bấm 1**. (IVR không gọi ops trực tiếp.) | Q6 / Q-O1 (phần Core) → **RESOLVED** |
| **D-07** | Availability do **Commerce/Sellable Gate** tổng hợp; **IVR không gọi Operational Core lot-level**. | Q7 / Q-O2 (phần commerce) → **RESOLVED** |
| **D-08** | **Giữ outbound-only.** Inbound lookup payment/shipping/status → **future scope** (chưa làm). | Q8 / Q-B1 → **RESOLVED (outbound-only)** |

## Module 3.1 — Sales Extensions

| ID | Quyết định (LOCKED) | Thay cho open question |
| --- | --- | --- |
| **D-09** | `IVRRequiredDecision` chỉ **set cờ + risk_reasons/policy** trên draft/order context; **Order Core mới tạo IVR task**. Cơ chế: **event nội bộ `order.ivr_required_decisioned`**; có thể thêm `GET` read-model để debug. **IVR KHÔNG nhận trực tiếp từ 3.1.** | Q9 / Q-S4 → **RESOLVED** |
| **D-10** | **Attempt policy MỚI (rule PACK-09 V1.0):** `MAX_ATTEMPT = 2` cho **cả hai** chương trình. **Giờ Vàng**: window **5 phút**, A1@`T0`, A2@`T0+2:30`, expire `T0+5:00`. **24/7**: window **15 phút**, A1@`T0`, A2@`T0+7:30`, expire `T0+15:00`. **`T0` = thời điểm Order Core mở IVR confirmation window / tạo task** (KHÔNG phải timestamp thô lúc khách bấm đặt nếu task bị delay). | Q10 / OD-DR-01 / C-01 / C-02 → **RESOLVED (rule mới)** |
| **D-11** | IVR **chỉ gửi signal qua callback**; **QuotaReleaseGuard (Sales/Program)** thực hiện release quota sau khi Core accept result fail/expired. IVR **không** gọi API release riêng. | Q11 / Q-S5 → **RESOLVED** |
| **D-12** | **Không hardcode ngưỡng trust trong IVR.** Skip IVR **chỉ khi**: `Customer Trust Resolver = TRUSTED` **và** `trusted_skip_allowed=true` **và** contact ổn định **và** không blocker **và** không risk flag. **Risk flags buộc gọi/review:** COD fail, duplicate, high-risk address, suspicious phone, abnormal value, high-risk Golden Hour, contact mới đổi, **trust resolver unavailable**. | Q12 / Q-F3 → **RESOLVED** |
| **D-13** | Danh sách điều kiện "IVR required" theo phase-3.1/07 được xác nhận; **ngưỡng cụ thể thuộc Risk Policy/Resolver**. IVR **chỉ consume boolean/source-backed `risk_flags`**; **không tự định nghĩa "abnormal order value" bằng số tiền** trong scheduler. | Q13 → **RESOLVED** |
| **D-14** | IVR **chỉ ghi audit/evidence nội bộ**; **KHÔNG ghi CRM note**. Nếu CRM cần outcome → nhận **event sau Core decision** và CRM tự ghi theo policy. | Q14 / Q-D2 → **RESOLVED** |

## Hệ quả cập nhật (đã áp dụng vào plan/specs)

- Attempt policy (D-10): cập nhật `specs/srs/functional/03-scheduler-attempt-policy.md` (bỏ dòng nguồn ghi rule cũ 2/10 & 3/15), `06-assumptions` (AS-01 → CONFIRMED), `05-current-docs-review` (C-01/C-02/OD-DR-01 → RESOLVED), và các file plan 02/04/13 + prompts p03/p04/p07.
- order_code (D-01): tension trong `02-current-understanding` §12, `10-integration-gap-analysis` GAP-S3, `14-risk-register` R-02 → RESOLVED.
- Order state (D-02): `10` GAP-S2, `14` R-04 → RESOLVED; specs `functional/01`, `workflows/09` bám contract enum đục.
- Scope outbound-only (D-08): `01-context-and-scope`, `06-assumptions` AS-02 → CONFIRMED.

## Ops-Core (Module 1/2) — trả lời 2026-07-02 (owner cùng team)

> ⚠️ **3 đính chính nền tảng (ảnh hưởng thiết kế):**
> - **DO-CORR-1:** Ops-core **KHÔNG biết `order_id`** (đơn thuộc Order Core). Ops chỉ tra theo **SKU / batch / QR**. → **Order Core phải fan-out** order → từng dòng SKU/batch rồi hỏi ops-core.
> - **DO-CORR-2:** **"Suppression / do-not-call / opt-out" KHÔNG thuộc ops-core** — thuộc **CRM / business-platform (module 3.1)**. "Suppression" trong ops-core chỉ là procurement/MRP (FRM-05). → Blocker "khách từ chối gọi" phải lấy từ CRM, KHÔNG từ ops.
> - **DO-CORR-3:** **Sale Lock ops-core hiện = recall-triggered** (`op_sale_lock_registry.recall_case_id` là FK bắt buộc). Chưa có sale-lock thương mại độc lập; "khóa bán" hôm nay = do recall.

| ID | Quyết định (LOCKED) | Thay cho |
| --- | --- | --- |
| **DO-01** | **KHÔNG** có blocker-status theo `order_id`. Primitive gộp = **sellable gate**: `POST /api/v1/admin/availability/check` (perm `SellableCheck`), body `{skuId, batchId?, requestedQuantity?}` → `SellableStatus{Decision∈{SELLABLE,NOT_SELLABLE,BLOCKED,UNKNOWN}, StockAvailable, BatchReleased, WarehouseReceiptConfirmed, HsdValid, QualityHold, RecallHold, SaleLock, TraceReady}`. Scope **SKU(±batch)**. Read chi tiết: `GET /v1/sale-locks/{id}`, `GET /v1/recall-cases/{id}`, admin `GET /api/v1/admin/recall/cases/{id}/holds`. SLA đề xuất **p95<200ms** (cache per sku/batch); tùy chọn bọc GET low-latency + ETag/captured_at. Lock scope: `scope_type∈{BATCH,SKU,PRODUCT,QR}`+`scope_id(Guid)`, `lock_status∈{ACTIVE,RELEASED,CANCELLED}`. | QO1 |
| **DO-02** | Snapshot = mảng **SellableStatus per-line (SKU/batch)**; **Order Core fan-out** order→lines→gọi `availability/check`→nhúng mảng vào task. Ops-core sẽ **bổ sung `captured_at`** (+ optional `policy_version`/ETag). Snapshot chỉ **pre-dispatch**; chân lý = revalidate lúc callback. (op_sale_lock_registry có `locked_at/released_at`; recall V1 có `effective_from/to`, `evidence_refs`, `audit_refs`.) | QO2 |
| **DO-03** | **Order Core là caller** (IVR không gọi ops trực tiếp — API ops là admin/service-auth). Read rẻ/idempotent; ops cam kết sẵn sàng cho revalidate. Điều kiện: Order Core dùng **service credential** có perm `SellableCheck`/`RecallHoldView`. | QO3 |
| **DO-04** | Có **outbox → HTTP webhook** (at-least-once; header `X-Idempotency-Key=EventId`, `X-Correlation-Id`; consumer dedupe theo EventId). Event: `ops-core.sellable.sku-became-not-sellable.v1` / `...became-sellable.v1`. **Chưa** có typed `SaleLockActivated/RecallActivated`. **Chốt: revalidate-at-callback là cơ chế chính**; webhook chỉ tối ưu "hold sớm" (SKU-level, ~1s, không sub-second). | QO4 |
| **DO-05** | availability/inventory logic ở **ops-core** (auth-gated, lot/sku-level, internal); **Order Core/Commerce** gọi & cấp aggregate cho IVR. Ops **không** mở endpoint availability cho IVR. | QO5 |
| **DO-06** | **fail-closed.** Health: `/health/live`, `/health/ready` (503 nếu DB unhealthy), `/health/startup`, `/metrics`. Error envelope `{error:{code,message,details,correlationId}}`, mã ổn định: `SALE_LOCK_ACTIVE`, `RECALL_IMPACT_ACTIVE`, `SELLABLE_GATE_BLOCKED`, `INVENTORY_NOT_SELLABLE`, `QUALITY_HOLD`, `TRACE_GAP_DETECTED`; `RATE_LIMITED`(429), `INTERNAL_ERROR`(500). Core coi **non-2xx/timeout/ready=503 = "không xác thực được blocker" → không dispatch/block**. Ops đồng ý contract. | QO6 |
| **DO-07** | `sale_lock_id`=Guid, `recall_case_id`=Guid + `recall_no` (mã người-đọc); `scope_type`+`scope_id`(Guid). V1 response trả `evidence_refs[]`/`audit_refs[]`; recall link `BATCH_TO_RECALL`. IVR/Core ghi `sale_lock_id`/`recall_case_id`+scope+`correlationId` vào evidence (MASTER-03). | QO7 |
| **DO-08** | Public trace **có sẵn**: `GET /api/v1/public/trace/{qrCode}` (AllowAnonymous, no-store, rate-limited). Key = **`qrCode`** (KHÔNG phải batch_code). `traceStatus∈{VALID,NOT_PUBLIC,INVALID_QR}`; recall thể hiện qua `batch.releasePublicStatus∈{RELEASED,NOT_RELEASED,HELD,RECALLED}`. Whitelist 12 field (không MFG/EXP, không supplier/lot/cost/QC). Inbound theo QR; theo batch_code cần projection mới. | QO8 |
| **DO-09** | Không có public catalog/ingredient list; product/SKU/BOM đều gated (`ProductCatalogView`); `product_name` chỉ lộ qua public trace. Tên/thành phần call script lấy qua **Commerce catalog / PACK-05**. | QO9 |

**Việc ops-core cần làm nếu chốt (cùng owner):** (1) reuse `availability/check` hay build GET blocker gọn cho Core; (2) thêm `captured_at`/version vào response sellable+lock (DO-02); (3) mở service-auth cho Order Core (DO-03); (4) chốt SLA p95<200ms + hành vi fail-closed (DO-06).

## Hệ quả cập nhật thêm (ops-core)
- Blocker model: đổi từ "sale_lock/recall/suppression snapshot rời" → **SellableStatus per-line qua sellable gate** (DO-01/DO-02). Cập nhật `04-module-dependency-map` §3, `functional/02`, `06-assumptions` AS-04, `12-ops-core-api-needs`, `06-ops-core-analysis-plan`, `10-gap-analysis`, `04-glossary`.
- **Suppression relocation (DO-CORR-2):** blocker do-not-call/opt-out chuyển nguồn từ ops → **CRM/business-platform** ⇒ phát sinh **câu hỏi mới cho Module 3.1** (xem "Còn treo").
- Sale Lock = recall-triggered (DO-CORR-3): glossary + functional/02 ghi rõ.

## Order Core — Order State Contract (DG-03 trả lời từ source, 2026-07-02)

> Nguồn: đọc code thật `ginsengfood-business-platform` (OrderStatus, OrderStateMachineImpl, Order.java, ShipmentServiceImpl, GoldenHourIvrCallbackResult). **Đính chính nhiều giả định trước đó.**

| ID | Quyết định (thực tế source) | Đính chính |
| --- | --- | --- |
| **DS-01** | `order_status` thật: **`CONFIRMING`, `CONFIRMED`, `PACKED`, `SHIPPING`, `DELIVERED`, `FAILED`, `CANCELLED`, `EXPIRED`**. **IVR-callable = CHỈ `CONFIRMING` VÀ CHỈ khi `payment_method_snapshot=COD`.** Mọi state khác + mọi đơn non-COD = không callable. `is_ivr_callable` **không phải field** — là rule derive từ state machine. | ⚠️ Mock `CONFIRMATION_REQUIRED/IVR_PENDING` **SAI** → dùng **`CONFIRMING`**. 🆕 **SCOPE MỚI: IVR chỉ cho đơn COD.** |
| **DS-02** | Transition thật: `IVR_CONFIRMED`→`CONFIRMING→CONFIRMED` (COD; **không** set PAID/paid_at) · `IVR_CUSTOMER_CANCELLED`→`CONFIRMING→CANCELLED` (COD; release inventory) · `IVR_CONFIRMATION_WINDOW_EXPIRED`→`timeout: CONFIRMING→EXPIRED` khi qua `expires_at` · **`IVR_NO_ANSWER_FINAL` / `IVR_TECHNICAL_EXCEPTION` → KHÔNG có transition Order Core** (order chờ `timeout→EXPIRED`). `IVR_OPERATIONAL_BLOCKED`/`IVR_POLICY_BLOCKED` là pre-call decision, IVR không phát result callback; Sale Lock/Recall phát hiện khi Sales revalidate được ghi bằng ACK `BLOCKED_BY_CORE`, không viết lại kết quả khách. Không có state `HOLD`/`BLOCKED`. | ⚠️ Các `recommended_core_action` "cancel_no_answer/hold" của ta là **aspirational**; thực tế no-answer/technical **không** cancel order — order tự expire. Semantics blocked chốt tại DT-06. |
| **DS-03** | Core nhận IVR result **chỉ khi** order còn `CONFIRMING` + COD; else (đã rời CONFIRMING/terminal, hoặc CONFIRMING non-COD) → **reject `422`**. **KHÔNG có** `CALLBACK_REJECTED_STALE`, **KHÔNG có** check `order_version_seen_by_ivr` trong IVR/order transition API. | ⚠️ Callback response codes (D-04) là **target**, chưa hiện thực; reality = `422`. |
| **DS-04** | Có `orders.version` (JPA `@Version`, optimistic locking) — bump khi row `orders` được save. **NHƯNG chưa expose** trong `OrderDetailResponse`; callback DTO **không nhận** `order_version_seen_by_ivr` → race-guard như ta thiết kế = **GAP** (cần Core expose). Mutate bảng con/queue không bump `orders.version`. | ⚠️ Race-guard `order_version_seen_by_ivr` chưa dùng được — cần Core expose. |
| **DS-05** | Không có field `fulfillment_gated_by_ivr`. Gate thật = **`order_status`**: shipment cần `CONFIRMED` ⇒ `CONFIRMING` = trạng thái khóa fulfillment chờ IVR. Downstream (CRM/commission/reporting) gate khác: **`ORDER_VERIFIED`** = `DELIVERED` + `payment_status=PAID` + `verification_status∈{VERIFIED,TRUSTED}`. | ✅ D-01 (fulfillment gated) đúng bản chất — nhưng qua `order_status`, không field riêng. |

**Hệ quả (cần áp dụng):** (1) đổi order-state trong specs/seed sang enum thật + **COD-only**; (2) transition table theo DS-02 (no-answer/technical không cancel → order expire); (3) callback: chấp nhận model `422` hiện tại + ghi delta ta muốn Core thêm (order_version expose, richer codes, explicit no-answer cancel) vào integration-requirements; (4) `is_ivr_callable` = Core derive (CONFIRMING+COD) — Order Core chỉ tạo task khi thỏa.

## CRM / Customer Identity (Module 3.1) — trả lời từ source `ginsengfood-business-platform` (2026-07-02)

> Nguồn: đọc code thật tại `C:\Projects\ginsengfood-business-platform`. **Xác nhận DO-CORR-2**: do-not-call/consent thuộc **business-platform Customer Identity**, KHÔNG phải ops-core.

| ID | Quyết định (LOCKED / gap) | Thay cho |
| --- | --- | --- |
| **DC-01** | **do-not-call/opt-out** = business-platform **Customer Identity consent/suppression registry** (`consent_suppression_markers` theo `channel_type + contact_hash`). Endpoint gần nhất: `POST /api/v1/admin/customer-identity/crm-ads-eligibility` (`channelType=PHONE_CALL`, `category=TRANSACTIONAL|SERVICE`, `customerId|guestId`, `customerContactChannelId`, `policyVersionId`, `enqueueOutbox=false`). **Order Core gọi** (như sellable gate), nhúng vào task `call_restriction`. ⚠️ Response hiện chỉ `eligible/denyReason/suppressionMarkerId` — **cần bổ sung read contract/adapter** để trả `do_not_call/opt_out_scope/reason/effective_at`. Lỗi/không response → **fail-closed** (không gọi). | Q-C1 / QC1 → **RESOLVED (endpoint known) + build extension** |
| **DC-02** | Consent **channel-specific**: `SMS` và `PHONE_CALL` là enum tách; suppression lookup theo `channelType+contactHash`. **Opt-out SMS KHÔNG chặn IVR voice** — IVR đọc riêng `PHONE_CALL`. | QC2 → **RESOLVED** |
| **DC-03** | **IVR confirmation call = transactional → KHÔNG áp CRM marketing quiet-hours/frequency-cap**; chỉ tôn trọng `PHONE_CALL` do-not-call/suppression **tuyệt đối**. IVR **không** đi qua CRM automation rule (tránh `cooldownMinutes`/`quietPeriodJson`); gọi `crm-ads-eligibility` trực tiếp với `category=TRANSACTIONAL`. | QC3 → **RESOLVED (lock đề xuất bởi CRM owner)** |
| **DC-04** | `TRANSACTIONAL/SERVICE` **không cần marketing opt-in** nhưng phải pass service-contact eligibility + không opted-out/blocked/suppressed. Legal basis nằm ở registry Customer Identity/Consent (không encode trong code). | QC4 → **RESOLVED** |
| **DC-05** | ⚠️ **Chưa có CRM event sau Core decision cho IVR outcome.** Order Core có transition `ivr-confirm`/`ivr-reject`/`timeout`; event catalog có `ORDER_CONFIRMED/CANCELLED/EXPIRED` nhưng **impl hiện chỉ publish** `ORDER_CREATED/PAYMENT_SUCCESS/PAYMENT_FAILED/PAYMENT_RECONCILE_REQUIRED`; notification là **no-op**. **Cần implement event sau Core decision**; CRM giữ template; IVR/SIM **không** gửi (khớp D-14). | QC5 / OD-16 → **GAP build (Order Core/CRM)** |
| **DC-06** | ⚠️ **Chưa có `CustomerTrustResolver`/`trusted_skip_allowed`** cho pre-IVR skip (trust scoring engine out-of-scope P3.2). Gần nhất `VerificationStatus.TRUSTED` (post order-verification). → **Trusted-skip hiện KHÔNG khả dụng** ⇒ default **luôn gọi IVR** (không skip) tới khi có resolver — khớp D-12 fail-safe "trust resolver unavailable → require IVR". Cần resolver/API mới nếu muốn bật skip. <br>⬆️ **Hệ quả đã đổi bởi `OD-15` (2026-08-25):** skip không còn phụ thuộc `CustomerTrustResolver`. Gap "chưa có trust scoring engine" vẫn đúng nhưng **không còn chặn** — IVR đọc `risk_flags` + `trust.risk_evidence_available` thay thế. Yêu cầu còn lại với Sales rút xuống **đúng một field**. | QC6 / Q-F3 → **GAP không còn chặn (OD-15)** |

**Hệ quả (ops-core corr. đã đúng):** blocker "khách opt-out" = **business-platform Customer Identity** (DC-01), sellable/recall = ops (DO-*), trust/risk = business-platform (DC-06, chưa có resolver). 3 nguồn blocker riêng, đều do **Order Core** hợp nhất vào task + revalidate.

## Foundation (owner kiêm — chốt từ docs 2026-07-02)

| ID | Quyết định | Trạng thái | Nguồn |
| --- | --- | --- | --- |
| **DF-01** (QF2) | RBAC permission `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW`; tạo/quản ở Permission Core; enforce server-side; admin action có `reason`+audit. | ✅ LOCKED | phase-8/11 §5; TECH-01 |
| **DF-02** (QF3) | Sinh **OpenAPI 3.1** `openapi/business-platform/ivr-order-confirmation.v1.yaml`; validate CI (parse + contract validator). | ✅ LOCKED | phase-8/11 §1,§10 |
| **DF-03** (QF4) | Release gate theo phase-8/09 + MASTER-05/PACK-10/TECH-10; IVR nộp evidence packet (task/attempt/result/callback/admin/security/privacy/smoke); `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi pass. **Sign-off = Module 8 Owner (bạn) + security/privacy review.** | ✅ LOCKED (model); sign-off = owner | phase-8/09; MASTER-05 |
| **DF-04** (QF5) | Idempotency store + audit log dùng **foundation TECH-01** (append-only), không tự chế. | ✅ LOCKED (reuse) | TECH-01 |
| **DF-05** (QF6) | `X-Correlation-Id` xuyên suốt (MASTER-03); event/outbox **tái dùng pattern ops-core** (`HttpWebhookOutboxEventDispatcher`), không tạo broker mới; event KHÔNG thay callback. | ✅ LOCKED (reuse) | MASTER-03; DO-04 |
| **DF-06** (QF1) | Service token foundation; allowlist = **Order Core** cho `POST .../tasks`; SIM adapter **không** có order-write cred; Order Core service-cred có `SellableCheck`/`RecallHoldView` (nối DO-03). | ✅ LOCKED (owner tự cấp) | phase-8/02,/11; DO-03 |
| **DF-07** (QF7) | Retention từng loại (call log/DTMF/recording/audit/raw phone-token). Đề xuất: raw phone/dial_token TTL ≤ confirmation window; audit theo foundation; recording OFF nên chưa cần. | ⏳ PENDING (owner + Legal chốt số) | phase-8/08,/12 §11 |
| **OD-ACC-01** | Console chỉ có `Admin` và `Operator`. Operator có đúng `IVR_ACCOUNT_SELF_VIEW`, `IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`, `IVR_MANUAL_RETRY`; Admin có account CRUD/reset và các quyền vận hành đã duyệt. Username không tái sử dụng; bootstrap credential dùng local/lab, không production. | ✅ LOCKED — option B (owner 2026-08-22) | W-0105 §2, §5, §15 |
| **OD-V1-20** | Cấp `IVR_FLAG_READ` **và** `IVR_RUNTIME_GATE_ADMIN` cho role `Admin`; Operator không có cả hai. Hệ quả thực tế **hẹp hơn tên gọi**: Admin qua được tầng permission của `POST /v1/ivr/order-confirmation/feature-flags/{env}`, nhưng `FeatureFlagAdminService.MutateAsync` gọi `IRuntimeGateAuthorization.IsApprovedAsync()` trước tiên và bản duy nhất đăng ký ngoài test là `PendingRuntimeGateAuthorization` → luôn `false`. Vì vậy POST nay trả **`409 IVR_OPERATIONAL_BLOCKED`** thay vì `403 IVR_FORBIDDEN_CALLER` — đổi kiểu từ chối, **không** mở cổng. Thay đổi có hiệu lực thật là hai GET flag/kill-switch nay trả `200` cho Admin (`IVR_FLAG_READ`). Muốn thật sự đổi được cờ phải **thay `PendingRuntimeGateAuthorization`** — chưa có bản duyệt nào trong production code. Gap `G-A` của lab do đó **chưa đóng**. | ⚠️ ACCEPTED — owner module IVR duyệt 2026-08-22; **chữ ký thứ hai của four-eyes (Security/Platform + Release owner) CHƯA có** | W-0105 §2.3; `specs/ui/08` §2; `FeatureFlagEndpoint.cs` |
| **OD-15** | **Không gọi IVR cho khách cũ.** Bằng chứng "khách cũ" = Sales gửi `eligibility_snapshot.trust.risk_evidence_available=true` **và** `risk_flags` **rỗng** → `TASK_SKIPPED_TRUSTED_CUSTOMER`, không tạo CallJob. **Bỏ** yêu cầu `customer_trust_status=TRUSTED` và `resolver_available` khỏi predicate (D-12 cũ) — khách mới đã tự mang cờ `NEW_CUSTOMER`/`VERIFIED_ORDER_COUNT_0` nên một phép kiểm "list rỗng" trả lời cả hai vế, **không** phải chờ `CustomerTrustResolver` mà DC-06 ghi là chưa build. **Giữ nguyên** ngoại lệ D-12: bất kỳ risk flag nào (COD fail, nghi trùng, địa chỉ/phone rủi ro, giá trị bất thường, Giờ Vàng rủi ro) → **vẫn gọi**. `trusted_skip_allowed=false` = **veto** của Sales cho đơn đó; absent = im lặng, không veto. `trust.resolver_version` fallback về `source_version` cấp snapshot. **Fail-closed không đổi:** `risk_evidence_available` thiếu/false → list rỗng **không** được đọc là "không rủi ro" → vẫn gọi. Cờ chính sách `IVR_RETURNING_CUSTOMER_SKIP_ENABLED` mặc định **ON**, đặt `NO` để rollback. **Wire contract shape KHÔNG đổi:** ba field `customer_trust_status`/`trusted_skip_allowed`/`risk_flags` giữ nguyên kiểu và vẫn optional; chỉ *nghĩa* của `trusted_skip_allowed` đổi từ opt-in bắt buộc → **veto**. Nghĩa mới đã ghi thẳng vào OpenAPI: **`draft.18 → draft.19`** (owner duyệt cùng ngày) — thêm `description` cho ba field + enum `decision`, regenerate `IvrServerModels.g.cs` (chỉ thêm 12 dòng XML doc), cập nhật `contract-manifest.json`, contract-diff report, oasdiff changelog và docs portal. `oasdiff breaking` vs baseline `draft.2`: **no breaking changes**; số operation (49) và schema (93) không đổi, nên rolling deploy giữa service không bị ảnh hưởng. | ✅ LOCKED — owner 2026-08-25; supersede **OD-08** và phần trust-score của **D-12**; thay hệ quả của **DC-06** | `EligibilityRules.cs` `TrustResolverEvidence.CanSkip`; `specs/workflows/07-trusted-skip.md`; UT-ELIG-TRUST-16/18/19 |
| **OD-L10N-02b** | Không dịch đè telemetry `detail`. Giữ raw để grep log và thêm companion `detail_vi` optional cho `SIM_GATEWAY`/`ORDER_CORE`. Với `CAPACITY_INCIDENT`, thêm `hold_new_calls` optional để UI dịch hiệu ứng theo fact có kiểu thay vì parse nghĩa câu tiếng Anh. Cả hai field optional để UI draft.17 sống được với server draft.16 khi rolling deploy. | ✅ LOCKED — owner 2026-08-24; triển khai `W-0116`, OpenAPI `draft.17` | W-0107 §10–11; W-0116 evidence |

## Telephony / Internal SIM Gateway (SIM **chưa mua** — làm trước bằng adapter port + mock)

| ID | Quyết định | Trạng thái | Nguồn |
| --- | --- | --- | --- |
| **DT-01** (QT1) | **Thiết kế adapter port** độc lập protocol: `dial / play_script / capture_dtmf / call_disposition / health`. Dev/test dùng **mock/dry-run**. Protocol phần cứng cụ thể điền **sau khi mua** SIM gateway. | ⏳ PENDING procurement (thiết kế port ✅ LOCKED) | phase-8/06; docx §10 |
| **DT-02** (QT2) | **Disposition mapping (LOCKED, IVR-owned):** answered+`1`/`0`→confirm/cancel (counted); answered no-key→`IVR_NO_ANSWER_ATTEMPT`/`WRONG_INPUT` (counted); ring timeout/không nghe→`NO_ANSWER` (counted); **busy→`NO_ANSWER`** (counted); **rejected (khách từ chối cuộc gọi)→`NO_ANSWER`** (counted; **KHÔNG** coi là cancel; flag review — có thể là opt-out signal tương lai); **unreachable/thuê bao không tồn tại/sai số→`IVR_INVALID_PHONE_FINAL`** (KHÔNG counted như no-answer; final riêng); SIM/audio/DTMF/network error/dropped→`IVR_TECHNICAL_EXCEPTION` (KHÔNG counted); capacity→`IVR_CAPACITY_EXCEPTION` (KHÔNG counted). ⚠️ **Re-verify với disposition code telco thật khi có SIM (DT-01).** | ✅ LOCKED (re-verify khi có SIM) | docx §13,§15; phase-8/07 |
| **DT-03** (QT3) | DTMF `1`/`0` qua RFC2833 hoặc in-band (tùy gateway); timeout sau khi phát script; sai/không bấm theo rule. | ⏳ PENDING gateway capability (đề xuất ✅) | docx §9 |
| **DT-04** (QT4) | `SIM_COOLDOWN_AFTER_CALL=5s`; `fail_count≥3/10′`→auto-disable+alert. **Số SIM: giả định lập kế hoạch pilot 12 → launch 24–32** (điền số thật khi mua). | ⏳ PENDING procurement (rule ✅ LOCKED) | docx §10,§11 |
| **DT-05** (QT5) | Call recording **OFF mặc định**; bật chỉ khi có consent+legal+retention. | ✅ LOCKED (OFF) | docx §17; phase-8/08 |
| **DT-06** (QT6) | Caller-ID/brandname hiển thị nhất quán, đáng tin. | ⏳ PENDING procurement/telco | docx §17 |

**Hệ quả:** SIM chưa mua **không chặn** specs — dùng adapter port (DT-01) + mock để chạy p05–p08 và dry-run smoke. `REAL_CUSTOMER_CALL_ALLOWED=NO` vẫn giữ. Cập nhật `functional/04`, `functional/06` (disposition DT-02), `01-context` (adapter-port/pending).

## Còn treo (thật sự cần người ngoài / mua sắm / legal)
- **CRM / Module 3.1 (vòng 2):** ✅ đã trả lời (DC-01..06). **Việc build còn lại:** (a) DC-01 bổ sung read-contract cho `crm-ads-eligibility` (trả `do_not_call/opt_out_scope/reason/effective_at`); (b) DC-05 implement event `ORDER_CONFIRMED/CANCELLED/EXPIRED` sau Core decision + CRM notification; (c) ~~DC-06 build `CustomerTrustResolver`~~ → **thay bằng `OD-15`**: chỉ cần Sales gửi **một field** `eligibility_snapshot.trust.risk_evidence_available=true` cùng `risk_flags` đầy đủ; không phải build trust scoring engine. Chừng nào chưa gửi, IVR **vẫn gọi tất cả** và log advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` — dùng chính advisory này để đo lúc nào gap đóng. → [questions-to-module-3-od15-risk-evidence.md](questions-to-module-3-od15-risk-evidence.md).
- **SIM procurement:** DT-01 protocol, DT-04 số SIM thật, DT-06 caller-ID — điền khi mua gateway.
- **Legal:** DF-07 retention, DT-05 recording (nếu muốn bật).
- **Release sign-off:** DF-03 — owner (bạn) + security/privacy review khi tới release gate.
- **Four-eyes cho `OD-V1-20`:** quyền runtime-gate đã cấp cho Admin trong code từ 2026-08-22 theo quyết định owner module IVR, nhưng chữ ký thứ hai (Security/Platform + Release owner) vẫn trống. Lab acceptance report và `release-compliance-checklist` S-07 chỉ được đánh ✅ khi có chữ ký đó.

## Tech Stack (DTS) — chốt 2026-07-03 (Owner)

> Quyết định nền tảng công nghệ cho **service IVR** (tách biệt `ginsengfood-business-platform`). Chi phối toàn bộ bộ prompt A–Z và deployment.

| ID | Quyết định (LOCKED) | Ghi chú / hệ quả |
| --- | --- | --- |
| **DTS-01** | **Backend = .NET 10 (C# / ASP.NET Core)**. Service IVR độc lập, không dùng chung codebase Java/Spring của Order Core/CRM/Ops. | Giao tiếp cross-platform qua **contract** (OpenAPI 3.1 REST + webhook outbox), không share DB/entity. Đọc `orders`/consent/sellable **chỉ qua API** của platform. |
| **DTS-02** | **Database = PostgreSQL**. Bảng `ivr_*` (DB riêng của IVR), migration bằng **EF Core** (hoặc Dapper+FluentMigrator — chốt ở Phase 1). | Constraint D-10 (max_attempts=2, window/spacing), unique idempotency/task/callback, index scheduler-deadline. **KHÔNG** cùng DB với Order Core. |
| **DTS-03** | **Admin/Monitoring UI = Next.js** (React, TypeScript). Gọi API IVR .NET; auth/RBAC `IVR_*`; i18n **vi**. | UI **không** bypass Order Core (D-02); chỉ đọc/giám sát + admin action có audit (DF-01). PII masked (D-05). |
| **DTS-04** | **Deploy = Docker + Kubernetes** (Helm). Thành phần: `ivr-api` (intake/callback), `ivr-worker` (scheduler/dispatch/SIM), `ivr-admin-ui`. | **HPA** scale worker theo SIM concurrency; secrets qua K8s Secret/Vault; NetworkPolicy; retention job = CronJob. Governance ladder → env promotion (DF-03). |
| **DTS-05** | **Quan sát & CI/CD:** OpenTelemetry (log/metric/trace), health `/health/live|ready|startup` (fail-closed 503, khớp DO-06), pipeline build→test→scan→push→deploy staged. | `REAL_CUSTOMER_CALL_ALLOWED=NO` map thành env gate; chỉ prod-gate mới bật SIM `REAL`. |

**Đã xác nhận 2026-08-12:** ORM/migration = EF Core; outbox = PostgreSQL-backed; CI provider = GitLab CI, không dùng GitHub Actions. **Còn cần owner/platform xác nhận trước phase liên quan:** secret store (K8s Secret vs HashiCorp Vault vs cloud KMS), container registry và GitLab Runner/Kubernetes credentials. Không được biến các mục còn mở thành production evidence.
