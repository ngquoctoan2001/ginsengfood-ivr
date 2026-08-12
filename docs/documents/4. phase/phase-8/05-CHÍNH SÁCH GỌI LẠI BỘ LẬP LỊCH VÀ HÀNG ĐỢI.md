# IVR-05 - Attempt Policy / Scheduler / Queue

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa attempt policy, scheduler, queue và state machine cho CallJob/Attempt.

## 1. Mục tiêu

Scheduler phải gọi đúng số lần, đúng window, đúng ưu tiên deadline và không kéo dài hiệu lực thương mại. Technical retry phải tách khỏi customer attempt.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Program-based call rule, capacity, scheduler. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Attempt lifecycle, no-answer, technical failure. |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md` | SIM capacity, attempt timing, queue baseline. |

## 3. Chính sách attempt theo chương trình

| Program | Window | Max customer attempt | Schedule | Final no-answer |
| --- | --- | --- | --- | --- |
| `GOLDEN_HOUR` | 10 phút | 2 | T0, T0 + 5 phút | Sau attempt 2 hợp lệ không nghe |
| `TWENTY_FOUR_SEVEN` | 15 phút | 3 | T0, T0 + 5 phút, T0 + 10 phút | Sau attempt 3 hợp lệ không nghe |

Không được áp một max attempt chung cho mọi chương trình. Không được gọi attempt ngoài window.

## 4. Attempt được tính và không được tính

Counted customer attempt khi:

- Phone valid.
- Attempt dispatch trong program window.
- Call đạt trạng thái customer-reachable theo adapter/policy.
- Khách nghe, từ chối, không nghe sau valid ring, hoặc không nhập DTMF theo policy.

Không tính là customer attempt khi:

- SIM Gateway failure.
- Server error.
- DTMF capture error.
- Audio playback error.
- Scheduler error trước dispatch.
- Callback error.
- Policy source unavailable.
- Evidence write failure.
- Phone validation technical error.

## 5. Scheduler rules

| Rule | Yêu cầu |
| --- | --- |
| `DEADLINE_AWARE_ROLLING_QUEUE` | Queue phải ưu tiên job gần hết window. |
| `BATCH_AFTER_SESSION_CALLING = PROHIBITED` | Không dồn toàn bộ cuộc gọi cuối phiên. |
| `ONE_SIM_ONE_ACTIVE_CALL` | Một SIM chỉ có một outbound call active. |
| `NO_WINDOW_EXTENSION` | Capacity incident không được kéo dài Golden Hour/24/7 validity. |
| `NO_INFINITE_RETRY` | Không có retry vô hạn. |

## 6. CallJob state machine

| State | Ý nghĩa | Next states |
| --- | --- | --- |
| `CALL_JOB_NOT_CREATED` | Chưa có job | `CALL_JOB_CREATED`, `CALL_JOB_REJECTED` |
| `CALL_JOB_CREATED` | Job accepted | `ELIGIBILITY_RECHECK_PENDING`, `CALL_JOB_CANCELLED` |
| `ELIGIBILITY_RECHECK_PENDING` | Kiểm lại source trước queue | `QUEUED`, `CALL_JOB_BLOCKED`, `ADMIN_REVIEW_REQUIRED` |
| `QUEUED` | Chờ scheduler | `SIM_CHANNEL_RESERVED`, `CALL_JOB_EXPIRED`, `QUEUE_PAUSED` |
| `QUEUE_PAUSED` | Queue bị pause | `QUEUED`, `CALL_JOB_EXPIRED`, `ADMIN_REVIEW_REQUIRED` |
| `SIM_CHANNEL_RESERVED` | Đã giữ SIM | `DIALING`, `SIM_CHANNEL_FAILED`, `CALL_JOB_CANCELLED` |
| `DIALING` | Đang gọi | `ATTEMPT_RESULT_RECEIVED`, `SIM_CHANNEL_FAILED`, `CALL_JOB_CANCELLED` |
| `ATTEMPT_RESULT_RECEIVED` | Có result attempt | `WAITING_NEXT_ATTEMPT`, `RESULT_CALLBACK_PENDING` |
| `WAITING_NEXT_ATTEMPT` | Chờ attempt sau | `QUEUED`, `CALL_JOB_EXPIRED`, `CALL_JOB_BLOCKED` |
| `RESULT_CALLBACK_PENDING` | Chờ callback Core | `RESULT_CALLBACK_SENT`, `CALLBACK_FAILED` |
| `RESULT_CALLBACK_SENT` | Đã gửi callback | `CORE_ACK_RECEIVED`, `CALLBACK_FAILED` |
| `CORE_ACK_RECEIVED` | Core nhận callback | `CALL_JOB_CLOSED` |
| `CALLBACK_FAILED` | Lỗi callback | `RESULT_CALLBACK_PENDING`, `ADMIN_REVIEW_REQUIRED` |
| `CALL_JOB_BLOCKED` | Bị blocker/policy chặn | `RESULT_CALLBACK_PENDING`, `ADMIN_REVIEW_REQUIRED` |
| `CALL_JOB_EXPIRED` | Hết window | `RESULT_CALLBACK_PENDING` |
| `CALL_JOB_REJECTED` | Reject trước job | Terminal |
| `CALL_JOB_CANCELLED` | Core/admin cancel trước khi xong | Terminal |
| `CALL_JOB_CLOSED` | IVR side hoàn tất | Terminal |

## 7. Attempt state machine

| State | Ý nghĩa | Counted? |
| --- | --- | --- |
| `ATTEMPT_PLANNED` | Attempt được lên lịch | Không |
| `ATTEMPT_PRECHECK_PENDING` | Kiểm lại blocker/source | Không |
| `ATTEMPT_READY` | Sẵn sàng reserve SIM | Không |
| `SIM_RESERVED` | Đã giữ SIM | Không |
| `DIALING` | Bắt đầu dial | Tùy result |
| `RINGING` | Valid ring/connect | Có nếu no-answer/reject theo policy |
| `ANSWERED` | Thiết bị/khách nghe | Có |
| `DTMF_WAITING` | Chờ phím | Có |
| `DTMF_1_CONFIRMED` | Bấm `1` | Có, final |
| `DTMF_0_CANCELLED` | Bấm `0` | Có, final |
| `DTMF_INVALID` | Sai phím | Có nếu policy tính |
| `DTMF_TIMEOUT` | Không nhập | Có |
| `NO_ANSWER` | Không nghe | Có |
| `CUSTOMER_REJECTED` | Từ chối cuộc gọi | `Owner Decision Required` |
| `UNREACHABLE` | Không liên lạc được | `Owner Decision Required` |
| `CALL_DROPPED` | Rớt cuộc gọi | `Owner Decision Required` |
| `SIM_FAILURE` | Lỗi SIM/channel | Không |
| `CALL_TECHNICAL_FAILURE` | Lỗi kỹ thuật | Không |
| `ATTEMPT_BLOCKED` | Block trước/trong dispatch | Không |
| `ATTEMPT_EXPIRED` | Hết window | Không |

## 8. Capacity incident

| State | Ý nghĩa |
| --- | --- |
| `CAPACITY_NORMAL` | Queue trong SLA. |
| `CAPACITY_DEGRADED` | Có nguy cơ trễ. |
| `CAPACITY_AT_RISK` | Một số attempt có thể miss deadline. |
| `CAPACITY_INCIDENT_OPEN` | Incident xác nhận. |
| `CAPACITY_ADMIN_PAUSED` | Admin pause queue. |
| `CAPACITY_RECOVERING` | Đang phục hồi backlog. |
| `CAPACITY_INCIDENT_CLOSED` | Đã đóng incident với evidence. |

Capacity incident không được kéo dài window. Nếu miss window, IVR trả expiry/block signal và Order Core quyết định.

## 9. Tiêu chí chấp nhận

- Golden Hour không có attempt thứ 3.
- 24/7 không có attempt thứ 4.
- Technical retry không tăng customer attempt count.
- Queue pause/resume có permission, reason, audit.
- Capacity miss không làm kéo dài giá/chương trình.

## 10. Attempt schedule generation

| Program | `T0` source | Generated customer attempts | Expiry |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` | Time Order Core says IVR confirmation starts. | Attempt 1 at `T0`; attempt 2 at `T0 + 5 minutes`. | `T0 + 10 minutes`. |
| `TWENTY_FOUR_SEVEN` | Time Order Core says IVR confirmation starts. | Attempt 1 at `T0`; attempt 2 at `T0 + 5 minutes`; attempt 3 at `T0 + 10 minutes`. | `T0 + 15 minutes`. |

Schedule must be deterministic from:

- `program_type`.
- `T0`.
- `policy_version`.
- `order_version`.

Schedule must not be regenerated with different offsets for the same task unless Order Core emits a new task/version.

## 11. Scheduler query model

Scheduler should query attempts by:

| Filter | Reason |
| --- | --- |
| `status in (ATTEMPT_PLANNED, ATTEMPT_PRECHECK_PENDING, ATTEMPT_READY)` | Only due work. |
| `scheduled_at <= now` | Attempt due. |
| `scheduled_window_expires_at > now` | Not expired. |
| `queue_status = ACTIVE` | Queue not paused/held. |
| `capacity_status != CAPACITY_HELD` | Capacity safe. |
| `real_call_allowed` or dry-run mode | Release gate. |

Ordering:

1. Earliest `scheduled_window_expires_at`.
2. Earliest `scheduled_at`.
3. Higher business priority only if source-backed.
4. Stable tie-breaker by `task_id`/`attempt_id`.

## 12. Pre-dispatch checks

Right before SIM reserve, scheduler must re-check:

- Queue not paused.
- Job not closed/cancelled.
- Attempt not already dispatched.
- Window not expired.
- Official contact still callable.
- Sale Lock/Recall/Suppression/opt-out not active.
- SIM capacity available.
- Evidence writer available if required.
- Release gate allows selected SIM mode.

If any check fails, mark attempt as `ATTEMPT_BLOCKED`, `ATTEMPT_EXPIRED`, `CAPACITY_HELD`, or `ADMIN_REVIEW_REQUIRED` as appropriate.

## 13. Technical retry model

Technical retry is allowed only when:

- Failure is classified as technical.
- No customer outcome was reached.
- Retry does not violate privacy/call restriction.
- Retry does not bypass operational blocker.
- Retry is within owner-approved retry count/backoff.

Technical retry must record:

| Field | Required |
| --- | --- |
| `technical_exception_id` | Yes |
| `original_attempt_id` | Yes |
| `customer_attempt_counted=false` | Yes |
| `retry_reason` | Yes |
| `admin_action_id` if manual | Conditional |
| `evidence_ref` | Yes |
| `audit_ref` | Yes |

## 14. Capacity handling

Capacity incident should open when:

- Due attempts cannot be dispatched before expiry.
- SIM healthy count drops below operational threshold.
- Callback/evidence outage makes final processing unsafe.
- Admin pauses queue for safety.

Capacity incident must not:

- Count as no-answer.
- Extend commercial window.
- Create additional customer attempts.
- Allow batch-after-session calling.

## 15. Concurrency controls

| Risk | Guard |
| --- | --- |
| Two workers dispatch same attempt | Attempt lock/compare-and-swap state update. |
| Two attempts reserve same SIM | Unique active SIM reservation. |
| Queue paused while worker is dispatching | Pre-dispatch check after lock. |
| Callback closes job while scheduler selects next attempt | Job state re-read after lock. |
| Window expires while SIM reserve waits | Expiry check after reserve attempt. |

## 16. Scheduler P0 tests

| Test ID | Scenario | Expected |
| --- | --- | --- |
| IVR05-P0-001 | Golden Hour schedule | Exactly 2 attempts. |
| IVR05-P0-002 | 24/7 schedule | Exactly 3 attempts. |
| IVR05-P0-003 | Golden Hour worker tries attempt 3 | Blocked/test fail. |
| IVR05-P0-004 | 24/7 worker tries attempt 4 | Blocked/test fail. |
| IVR05-P0-005 | SIM failure | Technical exception, no customer count. |
| IVR05-P0-006 | Queue paused | No dispatch. |
| IVR05-P0-007 | Capacity miss | Incident/expired signal, no window extension. |

## 17. Queue operational metrics

| Metric | Use |
| --- | --- |
| `due_attempts_total` | Backlog by deadline. |
| `missed_window_total` | Capacity/rule health. |
| `dispatch_latency_ms` | Scheduler performance. |
| `sim_reservation_conflict_total` | Concurrency defects. |
| `technical_retry_total` | Adapter/runtime stability. |
| `customer_attempt_count_total` | Policy validation. |
