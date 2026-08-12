# 11 — Sales Platform API Needs (DRAFT)

⚠️ **Đây là bản nháp nhu cầu API cần từ team module 3/3.1 — CHƯA phải API spec final, và KHÔNG khẳng định các API này đã tồn tại.** Endpoint sơ bộ chỉ để trao đổi.

Nhãn scope: **[CORE]** = cần cho outbound confirmation (scope phase-8). **[INBOUND?]** = chỉ cần nếu owner mở scope inbound.

Với mỗi API: mục đích · priority · input · output · write? · idempotency? · mock? · câu hỏi cho sales.

---

## API-1 [CORE] Order Core tạo/đẩy IVR task
- Mục đích: Order Core gửi `IvrConfirmationTaskV1` để IVR xét/gọi xác nhận.
- Priority: **P0**. Chiều: sales→IVR (IVR nhận). Endpoint sơ bộ: `POST /v1/ivr/order-confirmation/tasks` (IVR expose, Order Core gọi).
- Input: order_id, order_code_short, order_version, order_state, program_code, max_attempts, attempt_schedule, customer_trust_status, trusted_skip_allowed, risk_flags, official_contact_id, phone_ref/masked/validation, script template/version, allowed_script_variables, sale_lock/recall/suppression snapshots, evidence/privacy policy version, idempotency_key, correlation_id.
- Output (IVR trả): intake decision (`TASK_ACCEPTED_CALL_JOB_CREATED`/`REJECTED_*`/`BLOCKED_OPERATIONAL`/`HELD_ADMIN_REVIEW`).
- Write? Không ghi order (chỉ tạo task IVR). Idempotency? **Có** (bắt buộc). Mock? **Có**.
- Câu hỏi: Order Core **push** (gọi API IVR) hay IVR **poll**? Transport chuẩn? Ai giữ retry?

## API-2 [CORE] IVR callback result về Order Core
- Mục đích: IVR gửi `IvrConfirmationResultCallbackV1` (signal) để Core revalidate & transition.
- Priority: **P0**. Chiều: IVR→sales. Endpoint sơ bộ: `POST /v1/orders/{order_id}/ivr-result-callbacks` (sales expose).
- Input: callback_id, task_id, order_id, order_version_seen_by_ivr, result_type, result_reason, dtmf_key, is_counted_customer_attempt, is_final_for_ivr, recommended_core_action, evidence_ref, audit_ref, idempotency_key, correlation_id.
- Output: `CALLBACK_ACCEPTED_FOR_REVALIDATION`/`REJECTED_STALE`/`BLOCKED_BY_CORE`/`NEEDS_ADMIN_REVIEW`/`TECHNICAL_RETRY_ALLOWED|BLOCKED`.
- Write? Không (Core tự transition). Idempotency? **Có**. Mock? **Có**.
- Câu hỏi: Core revalidate đồng bộ trong response hay async? Timeout & retry policy phía Core?

## API-3 [CORE] IVR-required decision
- Mục đích: Biết một order có cần IVR không (`IVRRequiredDecision`), để Core tạo task đúng lúc.
- Priority: **P1**. Endpoint sơ bộ: `GET /v1/orders/{order_id}/ivr-required` hoặc event `order.ivr-required`.
- Input: order_id (hoặc order_draft_id), customer_id, channel.
- Output: required (bool), risk_reasons[], attempt_policy, quota_release_policy.
- Write? Không. Idempotency? N/A (read). Mock? **Có**.
- Câu hỏi: Quyết định này ở phase-3.1 xảy ra **trước** order_code — nó có tạo task IVR trực tiếp hay chỉ set cờ để Order Core (phase-8) tạo task sau? (liên quan tension GAP-S3).

## API-4 [CORE] Official contact / phone projection + dial token
- Mục đích: Cấp số gọi privacy-safe cho IVR.
- Priority: **P0**. Thường **đi kèm trong task** (API-1), nhưng có thể cần resolve riêng.
- Input: official_contact_id / order_id.
- Output: phone_ref, phone_masked, phone_validation_status, dial_token (TTL ngắn) nếu policy cho phép.
- Write? Không. Idempotency? N/A. Mock? **Có**.
- Câu hỏi: Cơ chế dial token, TTL, ai giữ mapping token→số thật?

## API-5 [CORE] Golden Hour quota release
- Mục đích: Khi IVR fail/timeout trong Golden Hour, giải phóng quota theo policy.
- Priority: **P1**. Endpoint sơ bộ: `POST /v1/programs/golden-hour/quota-release` hoặc event.
- Input: order_id, ivr_result_type, correlation_id.
- Write? **Có** (thuộc sales; IVR chỉ báo, sales thực thi). Idempotency? **Có**. Mock? **Có**.
- Câu hỏi: IVR chỉ gửi signal (qua callback) và sales tự release, hay cần API riêng?

## API-6 [INBOUND?] Lookup customer by phone
- Mục đích: (nếu inbound) nhận diện khách gọi vào theo số.
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/customers?phone={e164}`.
- Output: customer_id, member_tier, order_history_summary (masked).
- Write? Không. Mock? Có. Câu hỏi: Có mở inbound không? PII/consent?

## API-7 [INBOUND?] Lookup orders by customer/phone + order detail + status
- Mục đích: (nếu inbound) khách hỏi trạng thái đơn.
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/orders?customer_phone=...`, `GET /v1/orders/{id}`, `GET /v1/orders/{id}/status`.
- Output: order_code, status, payment_status, shipping_status/ETA (masked).
- Write? Không. Mock? Có. Câu hỏi: Trường nào được lộ cho IVR/khách?

## API-8 [INBOUND?] Payment/shipping status & payment reference
- Mục đích: (nếu inbound) đọc trạng thái thanh toán/giao hàng; cấp payment reference cho chuyển khoản.
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/orders/{id}/payment`, `GET /v1/orders/{id}/shipping-eta`, `GET /v1/orders/{id}/payment-reference`.
- Write? Không. Mock? Có. Câu hỏi: Mask bank info thế nào?

## API-9 [INBOUND?] Ghi call note / support ticket vào hồ sơ khách
- Mục đích: (nếu inbound hoặc muốn đồng bộ CRM) ghi kết quả cuộc gọi.
- Priority: **P2**. Endpoint sơ bộ: `POST /v1/customers/{id}/call-notes` hoặc `POST /v1/orders/{id}/call-notes`.
- Input: note_text/outcome, channel=IVR, correlation_id.
- Write? **Có**. Idempotency? **Có**. Mock? Có.
- Câu hỏi: Owner CRM có muốn IVR ghi note không? (phase-8 nói IVR không CRM đại trà — cần làm rõ ranh giới "ghi audit outcome" vs "CRM").

## API-10 [INBOUND?] Product catalog/price/promotion (tư vấn bán hàng)
- Mục đích: (nếu inbound tư vấn) — **ngoài scope phase-8**.
- Priority: **P2/OUT**. Câu hỏi: Có nằm trong IVR không, hay thuộc AI Advisor (PACK-05)?

---

## Bảng ưu tiên tổng hợp

| API | Scope | Priority | Write | Idempotency | Mock |
| --- | --- | --- | --- | --- | --- |
| API-1 Task intake | CORE | P0 | no | yes | yes |
| API-2 Result callback | CORE | P0 | no | yes | yes |
| API-3 IVR-required | CORE | P1 | no | n/a | yes |
| API-4 Contact/phone token | CORE | P0 | no | n/a | yes |
| API-5 Quota release | CORE | P1 | yes | yes | yes |
| API-6 Customer by phone | INBOUND? | P2 | no | n/a | yes |
| API-7 Orders by phone/detail/status | INBOUND? | P2 | no | n/a | yes |
| API-8 Payment/shipping/reference | INBOUND? | P2 | no | n/a | yes |
| API-9 Call note | INBOUND? | P2 | yes | yes | yes |
| API-10 Catalog/price | OUT? | P2 | no | n/a | yes |

> Chuyển bản draft này thành `integration-requirements/01-sales-platform-requirements.md` khi chạy p09.
