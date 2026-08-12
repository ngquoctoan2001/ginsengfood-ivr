# IVR-15 - Security / Privacy / Audit

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế bảo mật, quyền hạn, privacy, retention, evidence và audit cho IVR.

## 1. Mục tiêu

IVR xử lý dữ liệu nhạy cảm: số điện thoại, đơn hàng, DTMF, call log, admin action và trạng thái xác nhận/hủy. Vì vậy Phase 8 phải khóa security/privacy/audit trước khi có real customer call.

## 2. Security principles

- Deny by default.
- Service-to-service allowlist cho internal API.
- Admin RBAC server-side, không tin UI-only guard.
- Không lộ raw phone/full profile trong log hoặc UI.
- Secrets/SIM credentials chỉ ở adapter/infrastructure boundary.
- Evidence immutable sau khi accepted.
- Mọi action rủi ro có actor, reason, audit, correlation.
- Real customer call bị feature flag và release gate chặn.

## 3. Authentication

| Actor/caller | Auth model | Ghi chú |
| --- | --- | --- |
| Order Core | Service-to-service token/mTLS/API key nội bộ theo repo standard | Chỉ được gọi task/callback liên quan Order Core. |
| IVR Worker | Internal service identity | Ghi attempt/result/callback. |
| SIM Adapter | Internal service identity giới hạn | Không có quyền Order Core. |
| Admin Web | User session/JWT theo admin auth | Server kiểm RBAC. |
| Evidence Writer | Internal service identity | Chỉ ghi evidence/audit theo policy. |

## 4. Authorization matrix

| Capability | Permission | Actor hợp lệ | Evidence required |
| --- | --- | --- | --- |
| View queue/job/attempt/result | `IVR_QUEUE_VIEW` | Admin/Ops/Support được phân quyền | Không bắt buộc evidence mới, nhưng audit access nên có nếu xem PII. |
| Pause queue | `IVR_QUEUE_PAUSE` | Ops lead/admin | Có reason + audit. |
| Resume queue | `IVR_QUEUE_RESUME` | Ops lead/admin | Có reason + incident resolved evidence. |
| Disable SIM | `IVR_SIM_DISABLE` | IVR ops/admin | Có health/failure evidence. |
| Enable SIM | `IVR_SIM_ENABLE` | IVR ops/admin | Có health pass evidence. |
| Manual technical retry | `IVR_MANUAL_RETRY` | IVR ops/admin | Có technical exception evidence. |
| Result review | `IVR_RESULT_REVIEW` | Authorized reviewer | Có review note/audit. |
| Override order state | Không có trong IVR | Không ai | Phải qua Order Core, không qua IVR. |

## 5. Data classification

| Data | Classification | Rule |
| --- | --- | --- |
| `official_order_id`, `order_code` | Internal business data | Log được nhưng không public. |
| `phone_ref` | Sensitive reference | Không expose ngoài internal/admin. |
| `phone_masked` | Admin-safe projection | Có thể hiển thị nếu role phù hợp. |
| Raw phone | Restricted PII | Không lưu nếu chưa có owner decision. |
| DTMF key | Sensitive operational evidence | Chỉ lưu semantic key/result, không public. |
| Call recording | Restricted/high-risk | Owner Decision Required, mặc định disabled. |
| SIM provider logs | Sensitive infrastructure data | Sanitize trước khi đưa vào evidence. |
| Admin action reason | Audit data | Immutable, không chứa PII không cần thiết. |

## 6. Privacy rules

- Admin UI chỉ hiển thị `phone_masked`, không hiển thị raw phone.
- Dial token nếu có phải TTL ngắn và chỉ adapter đọc được.
- Không đọc full address, full customer profile, health note, CRM note vào IVR script.
- Call script chỉ dùng biến được duyệt: `order_code_short`, `total_amount_display`, `customer_name_short` nếu policy cho phép, `program_name` nếu cần.
- Recording mặc định `OFF` đến khi có privacy/legal/owner decision.
- DTMF evidence chỉ lưu key semantic và timestamp, không lưu audio nếu chưa duyệt.

## 7. Audit events bắt buộc

| Action | Audit fields tối thiểu |
| --- | --- |
| Task intake | `task_id`, `order_id`, `order_version`, `decision`, `idempotency_key`, `correlation_id`. |
| Eligibility decision | `decision`, `blocked_reasons`, `policy_version`, `source_refs`. |
| Attempt dispatch | `attempt_id`, `call_job_id`, `sim_channel_id`, `scheduled_at`, `actor=system`. |
| SIM reserve/release | `sim_channel_id`, `attempt_id`, `state_before`, `state_after`. |
| DTMF captured | `attempt_id`, `dtmf_key`, `result_status`, `timestamp`. |
| Result callback | `callback_id`, `order_id`, `order_version_seen_by_ivr`, `result_status`. |
| Core ack/reject | `callback_id`, `core_response_code`, `block_reason`. |
| Admin action | `actor_id`, `permission`, `target_type`, `target_id`, `reason`, `evidence_ref`. |
| Technical exception | `exception_type`, `customer_attempt_counted=false`, `retry_allowed`. |
| Capacity incident | `capacity_incident_id`, `scope`, `status`, `hold_new_calls`. |

## 8. Evidence requirements

Evidence packet cho IVR release hoặc incident phải có:

- Task sample.
- CallJob state trace.
- Attempt trace.
- DTMF/call disposition evidence.
- Result normalization evidence.
- Callback request/ack evidence.
- Admin action audit nếu có.
- Capacity/technical incident evidence nếu có.
- Security/privacy review evidence.
- Smoke/UAT result evidence.

Không được:

- Ghi `PASS` nếu evidence thiếu hoặc chưa accepted.
- Dùng screenshot UI thay cho audit log nếu audit log là source bắt buộc.
- Dùng real customer call làm smoke trước khi release gate cho phép.

## 9. Threat model tối thiểu

| Threat | Mitigation |
| --- | --- |
| Unauthorized task creation | Service allowlist, auth, idempotency, source system validation. |
| IVR direct order mutation | Không cấp credential/order write API cho IVR/SIM. |
| Duplicate callback confirms order twice | Idempotency + Order Core state/version revalidation. |
| Raw phone leakage | Token/ref/masked phone, log redaction, role-based UI. |
| Admin abuse manual retry | RBAC, reason, evidence, rate/attempt guard. |
| SIM credential leak | Secret manager/infrastructure boundary, no UI exposure. |
| Technical failure counted as no-answer | Separate technical exception state and tests. |
| Race condition confirm during Sale Lock | Order Core revalidation before transition. |

## 10. Retention baseline

Các thời hạn retention cụ thể là `Owner Decision Required`. Cho đến khi có quyết định:

- Không bật recording production.
- Không lưu raw phone lâu dài.
- Không expose call log raw cho role không cần thiết.
- Giữ audit/evidence đủ phục vụ release và dispute review theo foundation policy.

## 11. Release security gate

Trước real customer call:

- Security review pass.
- Privacy review pass.
- RBAC tests pass.
- Log redaction tests pass.
- Idempotency/race tests pass.
- Evidence registry integration pass.
- SIM credential handling reviewed.
- Admin action audit reviewed.

## 12. Acceptance criteria

- Có auth/authz rõ cho internal và admin API.
- Có permission matrix cho mọi admin action.
- Có privacy rule cho phone, DTMF, recording, call log.
- Có audit/evidence fields tối thiểu.
- Có threat model và mitigation.
- Có release security gate chặn production.
