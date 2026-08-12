# IVR-16 - Non-Functional Requirements

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế yêu cầu phi chức năng cho reliability, performance, capacity, idempotency, concurrency, observability, privacy và maintainability.

## 1. Mục tiêu

IVR là hệ thống tác động trực tiếp tới trải nghiệm khách hàng và trạng thái đơn hàng. NFR phải đảm bảo hệ thống fail-safe, không gọi sai khách, không gọi quá số lần, không làm sai trạng thái đơn, và có đủ evidence để release.

## 2. Reliability

| Requirement | Mô tả |
| --- | --- |
| Fail-safe by default | Nếu source owner không khả dụng, không dispatch call thật. |
| No direct order mutation | IVR không có đường ghi order state. |
| Bounded retry | Retry kỹ thuật phải có giới hạn và audit. |
| Idempotent commands | Task intake/callback/admin/retry phải idempotent. |
| Durable state | CallJob/Attempt/Result phải persisted trước khi side effect tiếp theo. |
| Worker recovery | Worker restart không làm mất attempt đang active hoặc dispatch duplicate. |

## 3. Timing và attempt policy

| Program | NFR timing |
| --- | --- |
| Golden Hour | 2 customer-counted attempts trong 10 phút, offsets `0`, `300` giây. |
| 24/7 | 3 customer-counted attempts trong 15 phút, offsets `0`, `300`, `600` giây. |

Rules:

- Không gọi attempt ngoài window.
- Không kéo dài window vì capacity/technical retry.
- Không gọi dồn cuối phiên.
- Scheduler ưu tiên deadline gần nhất.

## 4. Capacity baseline

Baseline từ PACK-09:

| SIM count | Capacity 5 phút | Capacity 15 phút |
| ---: | ---: | ---: |
| 12 | ~72 cuộc | ~216 cuộc |
| 24 | ~144 cuộc | ~432 cuộc |
| 32 | ~192 cuộc | ~576 cuộc |

NFR triển khai:

- `ONE_SIM_ONE_ACTIVE_CALL` là invariant.
- Capacity monitor phải phát hiện khi queue có nguy cơ miss deadline.
- Capacity incident không được biến thành no-answer.
- Capacity incident không được tự gia hạn chương trình thương mại.

## 5. Performance

| Operation | Requirement |
| --- | --- |
| Task intake | Phải nhanh, không dial SIM trong request thread. |
| Scheduler pick due attempts | Query được bằng index deadline/status. |
| SIM reserve | Atomic, không assign trùng SIM. |
| Result callback | Có retry bounded, không block scheduler indefinitely. |
| Admin queue view | Dùng projection/page, không scan toàn bộ call log. |

Không đặt số SLA production cụ thể nếu chưa có owner decision. Tuy nhiên implementation phải đo được latency và queue lag.

## 6. Concurrency

Race cần xử lý:

| Race | Guard |
| --- | --- |
| Duplicate task | Unique `task_id` và idempotency. |
| Duplicate callback | Unique `callback_id`/idempotency. |
| Hai worker dispatch cùng attempt | Attempt lock hoặc state compare-and-swap. |
| Hai worker reserve cùng SIM | Unique active reservation. |
| Order thay đổi khi IVR đang gọi | `order_version_seen_by_ivr` + Order Core revalidation. |
| Admin pause khi scheduler đang dispatch | Scheduler precheck ngay trước reserve/dial. |

## 7. Consistency

- Task/job/attempt/result/callback dùng eventual consistency với audit trail.
- Order state consistency thuộc Order Core.
- IVR callback không được coi là commit order state.
- Event publication nếu có chỉ phản ánh IVR signal, không phản ánh order final decision.

## 8. Observability

Metrics tối thiểu:

| Metric | Mục tiêu |
| --- | --- |
| `ivr_task_intake_total` | Theo accepted/rejected/blocked. |
| `ivr_call_job_active_total` | Active jobs. |
| `ivr_attempt_due_total` | Attempts đến hạn. |
| `ivr_attempt_dispatched_total` | Dispatch count. |
| `ivr_attempt_no_answer_total` | No-answer theo program/attempt number. |
| `ivr_result_status_total` | Result distribution. |
| `ivr_callback_latency_ms` | Latency callback. |
| `ivr_callback_rejected_total` | Stale/block/review. |
| `ivr_sim_channel_active_total` | Active SIMs. |
| `ivr_capacity_incident_total` | Capacity incidents. |
| `ivr_technical_exception_total` | Technical exceptions by type. |

Logs:

- Structured logs.
- Có `correlation_id`, `task_id`, `call_job_id`, `attempt_id` khi có.
- Redact raw phone/secrets.
- Không log full customer profile.

Tracing:

- Span cho task intake, scheduler, SIM dispatch, result normalization, callback, evidence write.

## 9. Security/privacy NFR

- Raw phone không xuất hiện trong logs.
- Admin access tới phone/call detail theo role.
- Recording disabled mặc định.
- Secrets không lưu trong DB/appsettings plaintext.
- Audit immutable theo foundation policy.
- Evidence refs không chứa dữ liệu nhạy cảm không cần thiết.

## 10. Maintainability

- Attempt policy đặt trong config/policy resolver, không hardcode rải rác.
- State transitions tập trung trong domain/service layer.
- SIM Adapter nằm sau interface để fake/internal/production adapter thay được.
- Error/result mapping có test table-driven.
- Admin UI không chứa business rule quyết định.

## 11. Compatibility

- v1 enum value đã publish không được xóa nếu chưa có breaking-change review.
- API path giữ `/v1`.
- Contract field mới ưu tiên optional/additive trừ khi SRS yêu cầu required.
- AsyncAPI broker/topic/retry/outbox vẫn future approved toolchain.

## 12. Release NFR gate

Không cho production real customer call nếu:

- Contract validator fail.
- Unit/integration/e2e/smoke fail.
- RBAC/security/privacy review chưa pass.
- Evidence packet chưa accepted.
- Queue pause/resume và SIM disable/enable chưa test.
- Technical failure còn có thể bị count là no-answer.
- Order Core revalidation chưa test race condition.

## 13. Acceptance criteria

- NFR bao phủ reliability, capacity, concurrency, observability, security, privacy.
- Có metrics/log/tracing tối thiểu.
- Có rule fail-safe rõ cho dependency outage.
- Có release NFR gate rõ.
