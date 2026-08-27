# IVR-13 - Function / Service Design

> **HISTORICAL_BASELINE / SUPERSEDED OVERLAY — 2026-08-27:** Các function/service trusted-skip
> bên dưới đã bị `OD-18`/`W-0123` thay thế và không còn trong active runtime. Giữ phần dưới làm
> lịch sử thiết kế.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế service/function để triển khai IVR theo từng module có owner rõ.

## 1. Mục tiêu

Tài liệu này chia hệ thống IVR thành các service/function triển khai. Mỗi service phải có trách nhiệm rõ, input/output rõ, lỗi rõ, transaction boundary rõ và test rõ. Không service nào được tự chuyển trạng thái Official Order.

## 2. Service map

| Service/function | Vai trò | Input chính | Output chính |
| --- | --- | --- | --- |
| `OrderCoreIvrTaskPublisher` | Order Core tạo task IVR. | Official Order, policy, contact projection. | `IvrConfirmationTaskV1`. |
| `IvrTaskIntakeService` | Nhận/validate task, idempotency. | Task payload. | Accepted/rejected/blocked decision, CallJob. |
| `IvrEligibilityService` | Kiểm eligibility/trusted/phone/block. | Task, resolver snapshots. | `IvrEligibilityDecision`. |
| `AttemptPolicyResolver` | Resolve Golden Hour/24/7 schedule. | Program, T0, policy version. | Attempt schedule. |
| `CallJobService` | Quản lý CallJob state. | Task, eligibility, scheduler signal. | CallJob state transitions. |
| `DeadlineAwareScheduler` | Chọn job/attempt cần gọi. | Queue, SIM capacity, deadlines. | Dispatch command. |
| `SimChannelManager` | Reserve/release/health SIM. | Attempt dispatch request. | SIM reservation or capacity incident. |
| `SimGatewayAdapter` | Dial, play script, capture call status/DTMF. | SIM reservation, script, dial token. | Raw call outcome. |
| `DtmfCaptureHandler` | Chuẩn hóa key DTMF. | Raw DTMF event. | DTMF semantic. |
| `ResultNormalizer` | Map raw outcome sang result status. | Attempt outcome, policy. | `IvrCallResult`. |
| `ResultCallbackClient` | Callback Order Core. | Result callback payload. | Core ack/reject/block. |
| `TechnicalExceptionService` | Xử lý lỗi kỹ thuật và retry. | Exception event. | Retry decision/admin review. |
| `CapacityIncidentService` | Mở/đóng incident queue/SIM. | Health/capacity metrics. | Capacity state/admin action. |
| `IvrAdminActionService` | Pause/resume/disable/enable/manual retry/review. | Admin request. | Admin action with audit. |
| `IvrEvidenceAuditWriter` | Ghi evidence/audit refs. | Domain action. | Evidence/audit refs. |
| `IvrReleaseGateService` | Chặn real call theo release gate. | Runtime config, evidence gate. | Allow/block dispatch. |

## 3. `OrderCoreIvrTaskPublisher`

Trách nhiệm:

- Chỉ tạo task cho Official Order.
- Đọc policy program, customer trust, official contact, blocker snapshot.
- Tạo `task_id`, `correlation_id`, `idempotency_key`, `order_version`.
- Không tạo task nếu trusted skip hợp lệ.
- Không tạo task nếu Quote/Cart/Order Draft.

Pseudo-flow:

```text
if entity_type != OFFICIAL_ORDER:
  do not create IVR task
if trusted_skip_allowed and no risk flags:
  record skip decision
  do not create IVR task
if sale_lock or recall or suppression or opt_out:
  record blocked decision
  do not create IVR task
create IvrConfirmationTaskV1
send task to IVR internal API
```

Tests:

- Official Order mới/untrusted tạo task.
- Trusted customer skip không tạo task.
- Quote/Cart/Draft không tạo task.
- Sale Lock/Recall active không tạo task.

## 4. `IvrTaskIntakeService`

Trách nhiệm:

- Validate schema, auth, idempotency, correlation.
- Reject hoặc hold task vi phạm contract.
- Tạo CallJob khi hợp lệ.
- Ghi audit/evidence cho intake.

Transaction boundary:

1. Start transaction.
2. Lock idempotency key.
3. Insert task nếu chưa có.
4. Run intake validation.
5. Insert CallJob hoặc reject decision.
6. Write audit/evidence refs.
7. Commit.

Không được:

- Dispatch SIM trong cùng request.
- Bỏ qua evidence nếu task được accept.
- Override `max_attempts` từ payload nếu không qua policy resolver.

## 5. `IvrEligibilityService`

Trách nhiệm:

- Kiểm Official Order còn callable.
- Kiểm trusted skip.
- Kiểm official contact và phone validation.
- Kiểm Sale Lock/Recall/Suppression/opt-out/payment issue/policy block.

Decision output:

| Decision | Ý nghĩa |
| --- | --- |
| `ELIGIBLE_FOR_IVR` | Được queue gọi. |
| `SKIP_TRUSTED_CUSTOMER` | Không cần IVR. |
| `BLOCK_QUOTE_CART_DRAFT` | Entity không đúng loại. |
| `BLOCK_INVALID_PHONE` | Phone không hợp lệ. |
| `BLOCK_OPT_OUT` | Opt-out/suppression. |
| `BLOCK_OPERATIONAL` | Sale Lock/Recall/payment/availability. |
| `OWNER_REVIEW_REQUIRED` | Không đủ nguồn để quyết định an toàn. |

Fail-safe:

- Nếu resolver không khả dụng và không có fallback owner-approved, không dispatch call.

## 6. `AttemptPolicyResolver`

Input:

- `program_type`.
- `T0`.
- `order_version`.
- `policy_version`.

Output:

| Program | Attempts | Offsets | Window |
| --- | ---: | --- | --- |
| `GOLDEN_HOUR` | 2 | `0`, `300` seconds | `600` seconds |
| `TWENTY_FOUR_SEVEN` | 3 | `0`, `300`, `600` seconds | `900` seconds |

Rules:

- Không tạo attempt ngoài window.
- Không tạo attempt thứ 3 cho Golden Hour.
- Không tạo attempt thứ 4 cho 24/7.
- Technical retry có counter riêng, không nằm trong attempt schedule.

## 7. `DeadlineAwareScheduler`

Trách nhiệm:

- Query CallJob/Attempt đủ điều kiện.
- Ưu tiên job gần hết window.
- Kiểm queue pause/capacity/release gate trước dispatch.
- Reserve SIM qua `SimChannelManager`.

Scheduler loop:

```text
while worker_running:
  load due attempts ordered by deadline
  for each attempt:
    if release gate blocks real calls:
      keep in dry-run or hold state
    if queue paused or capacity held:
      skip and record reason
    if eligibility precheck fails:
      mark attempt blocked
    else reserve SIM and dispatch
```

Concurrency guard:

- Dùng DB lock/distributed lock để không dispatch cùng attempt hai lần.
- Dùng unique active reservation trên `sim_channel_id`.

## 8. `SimChannelManager`

Trách nhiệm:

- Đảm bảo `ONE_SIM_ONE_ACTIVE_CALL`.
- Health check SIM.
- Reserve/release SIM.
- Disable/enable SIM theo admin action.

State:

- `ENABLED_IDLE`.
- `RESERVED`.
- `ACTIVE_CALL`.
- `DISABLED`.
- `HEALTH_FAILED`.
- `COOLDOWN`.

Không được:

- Dùng SIM disabled.
- Assign trùng SIM.
- Tự enable SIM không qua health/admin rule.

## 9. `SimGatewayAdapter`

Trách nhiệm:

- Nhận dispatch command.
- Dial bằng phone token/ref đã được phép.
- Play approved script.
- Capture call status và DTMF.
- Trả raw outcome cho Result Normalizer.

Adapter output tối thiểu:

| Field | Ghi chú |
| --- | --- |
| `provider_call_id` | Nếu gateway cung cấp. |
| `sim_channel_id` | SIM đã dùng. |
| `started_at`, `ended_at` | Timing. |
| `raw_call_status` | Answered/no answer/busy/failure. |
| `raw_dtmf_key` | `1`, `0`, invalid, none. |
| `technical_error_code` | Nếu lỗi kỹ thuật. |
| `evidence_ref` | Call log/evidence. |

Không được:

- Gọi lại khách ngoài scheduler.
- Gửi SMS/notification.
- Ghi order state.
- Log raw phone nếu không có policy.

## 10. `ResultNormalizer`

Trách nhiệm:

- Map raw outcome sang result enum.
- Tách no-answer/invalid-phone/technical/capacity/operational block.
- Xác định `is_counted_customer_attempt`.
- Tạo `IvrCallResult`.

Mapping cốt lõi:

| Raw outcome | Normalized result | Counted |
| --- | --- | --- |
| DTMF `1` | `IVR_CONFIRMED` | Có |
| DTMF `0` | `IVR_CUSTOMER_CANCELLED` | Có |
| Valid no-answer chưa max | `IVR_NO_ANSWER_ATTEMPT` | Có |
| Valid no-answer max | `IVR_NO_ANSWER_FINAL` | Có |
| Phone invalid | `INVALID_PHONE_FINAL` | Không |
| SIM/server/DTMF technical error | `IVR_TECHNICAL_EXCEPTION` | Không |
| Sale Lock/Recall during callback | `IVR_OPERATIONAL_BLOCKED` | Không |

## 11. `ResultCallbackClient`

Trách nhiệm:

- Build `IvrConfirmationResultCallbackV1`.
- Send to Order Core.
- Retry bounded khi lỗi kỹ thuật.
- Ghi callback ack/reject/block.

Không được:

- Retry vô hạn.
- Gửi callback thiếu evidence/audit.
- Coi HTTP 200 là order confirmed nếu body không có ack semantic.

## 12. `IvrAdminActionService`

Trách nhiệm:

- Enforce RBAC server-side.
- Validate reason/evidence.
- Ghi `IvrAdminAction`.
- Thực thi pause/resume/disable/enable/manual technical retry/review.

Permission matrix:

| Action | Permission |
| --- | --- |
| View queue | `IVR_QUEUE_VIEW` |
| Pause queue | `IVR_QUEUE_PAUSE` |
| Resume queue | `IVR_QUEUE_RESUME` |
| Disable SIM | `IVR_SIM_DISABLE` |
| Enable SIM | `IVR_SIM_ENABLE` |
| Manual technical retry | `IVR_MANUAL_RETRY` |
| Result review | `IVR_RESULT_REVIEW` |

## 13. Acceptance criteria

- Mỗi service có owner, input, output, failure rule.
- Không service nào trực tiếp update order state.
- Scheduler và SIM manager có concurrency guard.
- Technical retry tách khỏi customer attempt.
- Admin action có permission/audit/evidence.
- Result callback có idempotency và race-condition handling.
