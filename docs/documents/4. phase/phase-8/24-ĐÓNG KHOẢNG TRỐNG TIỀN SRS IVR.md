# IVR Phase 8 - Đặc Tả Gap Closure Tiền SRS

> **HISTORICAL_BASELINE / SUPERSEDED OVERLAY — 2026-08-27:** Gap customer-trust/trusted-skip phía
> IVR bên dưới đã bị `OD-18`/`W-0123` thay thế. M3 là business decision owner; IVR không còn chờ
> trust/risk-evidence để quyết định call/skip.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Phiên bản: `V1.0 PRE-SRS GAP CLOSURE`  
Trạng thái: `DESIGN_BASELINE_FOR_SRS`  
Phạm vi: IVR Order Confirmation  
Production status: `NOT_READY`

## 1. Mục tiêu

Tài liệu này là baseline gap-closure trước khi viết SRS chi tiết cho Phase 8. Nội dung dùng để bảo đảm bộ SRS `IVR-00` đến `IVR-09` không bỏ sót các gap quan trọng đã được phát hiện khi rà PACK-09, TECH-09, các phase trước và tài liệu nền trong appendices/master/pack/tech.

Tài liệu này không thay thế PACK-09 hoặc TECH-09. Tài liệu này gom các rule đã có nguồn, tách rõ owner decision còn thiếu và khóa ranh giới để Phase 8 không triển khai sai vai trò IVR.

## 2. Nguồn nền

| Nguồn | Vai trò |
| --- | --- |
| `docs/source-map.md` | Đường dẫn nguồn hợp lệ. |
| `docs/documents/0. appendices/` | Quy tắc form, auto-generated data, printing, MISA, material/packaging liên quan đến evidence và boundary. |
| `docs/documents/1. master/` | Source-of-truth, traceability, runtime guard, evidence, release gate. |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Nguồn pack chính của IVR. |
| `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` | Completion/evidence/release readiness. |
| `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md` | RBAC, audit, idempotency, evidence registry. |
| `docs/documents/3. tech/04-TECH-03-OPERATIONAL-CORE-PRODUCTION-WAREHOUSE-INVENTORY-TRACEABILITY-RECALL-SALE-LOCK.md` | Sale Lock, Recall, suppression và operational blocker. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Official Order, payment, shipping, verified revenue boundary. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Nguồn kỹ thuật chính của IVR. |
| `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-EVIDENCE-RELEASE-GATEWAY-PRODUCTION-READINESS-CONTROL.md` | Global smoke, UAT, evidence, release gate. |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md` | Baseline SIM Gateway, call script, attempt policy, capacity. |
| `docs/documents/4. phase/phase-8/ivr-order-confirmation.md` | Index/handoff Phase 8 và pack view sạch. |

## 3. Baseline không được thay đổi

| Rule | Giá trị khóa |
| --- | --- |
| Mục đích | `CALL_PURPOSE = ORDER_CONFIRMATION_ONLY` |
| Deployment | `IVR_DEPLOYMENT_MODEL = INTERNAL_SIM_GATEWAY_SERVER` |
| SIM capacity | `1 SIM = 1 ACTIVE_OUTBOUND_CALL` |
| Golden Hour | 2 cuộc trong 10 phút, lịch T0 và T0 + 5 phút |
| 24/7 | 3 cuộc trong 15 phút, lịch T0, T0 + 5 phút, T0 + 10 phút |
| Attempt policy | `MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED` |
| Kết quả IVR | Input signal only |
| Quyết định cuối | Commerce Order Core / Order State Machine |
| Operational blockers | Sale Lock / Recall / Suppression / opt-out / policy block thắng IVR |
| Production | Không gọi khách thật cho đến khi IVR-09 release gate pass |

## 4. Connected systems

| Hệ thống | Kết nối bằng gì | Cách kết nối | IVR được làm | IVR không được làm |
| --- | --- | --- | --- | --- |
| Commerce Order Core | Internal task/callback contract | Order Core tạo `IvrConfirmationTaskV1`; IVR callback `IvrConfirmationResultCallbackV1`; Core revalidate | Nhận task, gửi result signal | Tự transition order |
| Operational Core | Internal read/check contract | Check Sale Lock, Recall, Suppression, availability, traceability blocker | Consume blocker status | Override blocker |
| Customer Trust/Memory | Internal resolver | Lấy trust decision, risk reason, trusted skip reason | Consume decision | Hardcode trusted customer |
| Official Contact/Profile | Privacy-safe projection | Nhận `official_contact_id`, `phone_ref`, `phone_masked`, dial token nếu được duyệt | Gọi contact chính thức | Đọc full profile/full address |
| SIM Gateway Adapter | Internal hardware/API adapter | Reserve SIM, dial, phát script, capture call status/DTMF | Dial/capture/health check | Gửi SMS hoặc update order |
| Evidence/Audit | Internal writer | Ghi evidence/audit cho task, attempt, result, admin action | Ghi evidence | Tự accepted/pass evidence |
| Admin/Ops Console | Internal admin API + RBAC | Monitor, retry kỹ thuật, pause queue, disable SIM | Điều hành có permission/audit | Bypass P0 blocker |
| Notification Owner | Internal handoff sau Core decision | Chỉ sau khi Core hủy/hold/expire | Không gửi trực tiếp | IVR/SIM tự gửi notice |
| AI/Gateway/Live/CRM | No direct command | Chỉ consume trạng thái đã được Core công nhận nếu policy cho phép | Không trigger IVR | Tạo task/gọi IVR/public result |
| Payment/MISA/Finance | Không kết nối trực tiếp từ IVR | Do owner riêng xử lý | Không xử lý | Xác nhận payment/MISA/revenue |

## 5. Gap closure matrix

| Gap | Closure | SRS tương ứng |
| --- | --- | --- |
| Chưa có state machine chi tiết | Khóa CallJob, Attempt, Result, CapacityIncident, TechnicalException | `IVR-05`, `IVR-06`, `IVR-07` |
| Chưa khóa eligibility | Khóa khách cần IVR, trusted skip, fail-safe eligibility | `IVR-03` |
| Chưa có contract rõ | Khóa semantic contract task/callback | `IVR-04`, `IVR-07` |
| Chưa có official contact policy | Khóa projection, phone validation, invalid phone | `IVR-03`, `IVR-04` |
| Chưa có race-condition rule | Khóa revalidation trước dispatch và khi callback | `IVR-07` |
| Chưa tách technical retry với no-answer | Khóa counted/uncounted attempt và bounded retry | `IVR-05`, `IVR-06` |
| Chưa có permission matrix | Khóa RBAC cho admin action | `IVR-08` |
| Chưa có retention/privacy | Khóa PII, recording default off, retention owner decision | `IVR-08` |
| Chưa có production release gate | Khóa documentation/implementation/test/smoke/release gate | `IVR-09` |

## 6. Eligibility baseline

IVR được xét khi:

- Đơn là Official Order.
- Khách mới, untrusted, partially trusted hoặc có risk flag.
- Customer Memory chưa đủ dữ liệu để bỏ qua IVR.
- Official contact hợp lệ.
- Phone valid hoặc có fallback policy rõ.
- Không có Sale Lock/Recall/Suppression/call restriction.
- Program/window còn hợp lệ.

Trusted customer được skip khi:

- Resolver trả `TRUSTED`.
- Commerce/Customer Memory cho phép trusted checkout.
- Không có risk flag mới.
- Official contact ổn định.
- Không có operational/call restriction blocker.

Không được hardcode trusted customer theo tên, số điện thoại thô, lịch sử chat hoặc cảm tính.

## 7. Task contract baseline

`IvrConfirmationTaskV1` phải có tối thiểu:

- `task_id`, `task_version`, `idempotency_key`, `correlation_id`.
- `order_id`, `order_code_short`, `order_version`, `order_state`.
- `program_code`, `attempt_policy_code`, `max_attempts`, `attempt_schedule`.
- `customer_ref`, `customer_trust_status`, `trusted_skip_allowed`, `risk_flags`.
- `official_contact_id`, `phone_ref`, `phone_masked`, `phone_validation_status`.
- `call_script_template_id`, `call_script_version`, `allowed_script_variables`.
- `sale_lock_snapshot`, `recall_snapshot`, `suppression_snapshot`, `call_restriction_snapshot`.
- `evidence_policy_version`, `privacy_policy_version`.

Task phải bị reject/hold nếu không phải Official Order, sai program policy, thiếu contact/phone, có blocker, thiếu evidence/privacy policy hoặc script chưa duyệt.

## 8. Callback contract baseline

`IvrConfirmationResultCallbackV1` phải có tối thiểu:

- `callback_id`, `task_id`, `call_job_id`, `attempt_id`.
- `idempotency_key`, `correlation_id`.
- `order_id`, `order_version_seen_by_ivr`.
- `program_code`, `attempt_policy_code`, `attempt_no`, `max_attempts`.
- `result_type`, `result_reason`, `dtmf_key`.
- `is_counted_customer_attempt`, `is_final_for_ivr`.
- `recommended_core_action`.
- `technical_error_code` nếu có.
- `evidence_ref`, `audit_ref`, `privacy_policy_version`, `script_version`.

`recommended_core_action` chỉ là khuyến nghị. Order Core phải revalidate trước mọi transition.

## 9. State machine baseline

SRS phải có:

- CallJob state machine: intake, eligibility recheck, queued, SIM reserved, dialing, result, callback, closed/blocked/expired.
- Attempt state machine: planned, precheck, ready, dialing, ringing, answered, DTMF, no-answer, technical failure, blocked, expired.
- Result state machine: raw, normalized, evidence pending, ready callback, sent, Core accepted/rejected stale, review.
- CapacityIncident state machine: normal, degraded, at risk, incident open, paused, recovering, closed.
- TechnicalException state machine: detected, classified, retry pending/running, resolved, final review.

Mọi transition phải có audit/evidence. Transition ảnh hưởng order state phải qua Order Core.

## 10. Race-condition baseline

Nếu khách bấm `1` nhưng trong lúc callback hoặc trước khi Core accept có Sale Lock, Recall, Suppression, payment issue, order expiry hoặc order version change:

- IVR vẫn giữ raw result là `IVR_CONFIRMED`.
- Order Core không được auto-confirm.
- Core phải block hoặc hold admin review.
- Evidence phải link cả IVR result và blocker.

## 11. Privacy baseline

IVR không được đọc:

- Full address.
- Payment detail.
- Member tier.
- Diamond/referral information.
- Health note.
- AI consultation content.
- CRM private content.
- Full order history.

Default V1.0:

- `CALL_RECORDING_ENABLED = NO_UNTIL_OWNER_APPROVED`.
- `CALL_TRANSCRIPT_ENABLED = NO_UNTIL_OWNER_APPROVED`.
- Retention duration là `Owner Decision Required`.

## 12. Release baseline

Không được gọi khách thật cho đến khi:

- SRS `IVR-00` đến `IVR-09` hoàn tất.
- Implementation pass.
- Privacy/legal retention pass.
- Smoke matrix pass.
- Evidence accepted.
- IVR Owner, Order Core Owner, Operational Owner, Privacy Owner và Release Owner sign-off.
- Kill switch, pause queue, disable SIM, duplicate callback, stale callback đã test.

## 13. Owner Decision Required

Các điểm chưa được quyết định bởi nguồn nền:

- Ngưỡng Customer Trust để skip IVR.
- Risk flag buộc khách trusted vẫn phải IVR.
- Tiêu chí permanent invalid phone.
- Technical retry count/backoff.
- Mapping busy/rejected/unreachable/dropped theo SIM Gateway thực tế.
- Cho phép recording hay không.
- Retention duration cho call log/DTMF/evidence/SIM logs.
- Production SIM Gateway hardware/API protocol.
- Pilot scope.
- Notification template sau khi Order Core hủy đơn.

## 14. Acceptance của gap closure

Gap closure đạt khi:

- SRS dùng tài liệu này làm baseline.
- SRS không cho IVR update order trực tiếp.
- SRS không cho IVR gọi Quote/Cart/Order Draft.
- SRS giữ Golden Hour 2 cuộc và 24/7 3 cuộc.
- SRS tách confirm, cancel, no-answer, invalid phone, technical failure.
- SRS có state machine, task/callback contract, race revalidation, RBAC, privacy và release gate.
