# IVR-18 - Deployment / Observability / Runbook

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế deployment, runtime config, monitoring, alerting và runbook vận hành cho IVR.

## 1. Mục tiêu

Tài liệu này mô tả cách đưa IVR từ contract/SDS sang môi trường dev, staging, pilot và production một cách an toàn. Mặc định Phase 8 không cho gọi khách thật cho đến khi release gate pass.

## 2. Environment strategy

| Environment | Mục tiêu | SIM mode | Real customer call |
| --- | --- | --- | --- |
| Local/dev | Unit/integration dev, fake adapter. | Fake/in-memory | No |
| Test/CI | Automated tests. | Fake deterministic | No |
| Staging | End-to-end với internal numbers. | Sandbox/internal SIM | No customer |
| Pilot | Owner-approved small scope nếu được duyệt. | Controlled SIM | Owner Decision Required |
| Production | Real customer call sau release gate. | Production SIM | Only after IVR-09 pass |

## 3. Runtime config

| Config | Default | Ghi chú |
| --- | --- | --- |
| `IVR_ENABLED` | `false` | Bật IVR runtime ở non-production trước. |
| `IVR_TASK_INTAKE_ENABLED` | `false` | Có thể bật trước dispatch. |
| `IVR_SCHEDULER_ENABLED` | `false` | Scheduler độc lập. |
| `IVR_SIM_GATEWAY_MODE` | `FAKE` | `FAKE`, `INTERNAL_TEST`, `PRODUCTION`. |
| `REAL_CUSTOMER_CALL_ALLOWED` | `false` | Hard gate. |
| `IVR_DRY_RUN_ONLY` | `true` | Cho phép tạo job/result giả lập. |
| `IVR_RECORDING_ENABLED` | `false` | Owner Decision Required. |
| `IVR_QUEUE_PAUSED` | `true` | Default safe. |
| `IVR_MAX_TECHNICAL_RETRY` | Owner Decision Required | Không tự bịa policy. |
| `IVR_LOG_RAW_PHONE_ALLOWED` | `false` | Không bật production. |

Config rules:

- Production deploy phải fail nếu `REAL_CUSTOMER_CALL_ALLOWED=true` nhưng release evidence chưa pass.
- `IVR_SIM_GATEWAY_MODE=PRODUCTION` chỉ hợp lệ khi real-call release gate pass.
- Config thay đổi phải audit.

## 4. Deployment stages

| Stage | Nội dung | Exit gate |
| --- | --- | --- |
| Stage 0 | Docs/SRS/SDS/contract complete. | Validator pass. |
| Stage 1 | DB migrations and repositories. | Migration tests pass. |
| Stage 2 | Internal APIs with fake adapter. | API/integration tests pass. |
| Stage 3 | Scheduler/queue dry-run. | Attempt policy tests pass. |
| Stage 4 | SIM adapter internal test numbers. | DTMF/call disposition evidence pass. |
| Stage 5 | Admin monitoring/RBAC. | Admin action tests pass. |
| Stage 6 | Smoke/UAT/evidence packet. | IVR-09 release gate pass. |
| Stage 7 | Owner-approved pilot. | Owner Decision Required. |

## 5. Observability dashboard

Dashboard tối thiểu:

- Total tasks by status.
- Active CallJobs.
- Due attempts by deadline bucket.
- Attempts dispatched by program.
- Golden Hour attempts over limit count.
- 24/7 attempts over limit count.
- Result distribution.
- Callback accepted/stale/blocked/review.
- Technical exceptions by type.
- SIM channel active/idle/disabled/failed.
- Capacity incidents open.
- Queue paused/resumed events.
- Admin actions by permission.

## 6. Alerts

| Alert | Condition | Severity | Runbook |
| --- | --- | --- | --- |
| Queue deadline risk | Due attempts likely miss window. | High | Capacity incident runbook. |
| SIM channel failure spike | SIM failures above baseline. | High | Disable affected SIM, investigate gateway. |
| Callback failure spike | Callback retries increasing. | High | Check Order Core/API/idempotency. |
| Evidence write failure | Evidence write errors. | Critical | Pause final callbacks, route review. |
| Technical counted as no-answer | Any detected invariant violation. | Critical | Pause queue immediately. |
| Real call flag mismatch | Production real call allowed without release evidence. | Critical | Disable IVR and rollback config. |
| Duplicate dispatch | Same attempt dispatched twice. | Critical | Pause scheduler, inspect locks. |

## 7. Runbook - Pause queue

Trigger:

- Capacity risk.
- SIM instability.
- Evidence/callback outage.
- Security/privacy issue.

Steps:

1. Admin with `IVR_QUEUE_PAUSE` submits pause action with reason.
2. System writes `IvrAdminAction`.
3. Scheduler stops dispatching new attempts.
4. Active calls finish or are handled by adapter policy.
5. Open capacity/technical incident if applicable.
6. Notify owner channel with incident id.

Exit:

- Root cause understood.
- Evidence attached.
- Resume action approved.

## 8. Runbook - Disable SIM

Steps:

1. Identify `sim_channel_id`.
2. Verify no active call or wait for safe release.
3. Admin with `IVR_SIM_DISABLE` submits action.
4. Channel becomes disabled and removed from scheduler pool.
5. Evidence links health failure.

Do not:

- Disable/enable from UI without server permission.
- Re-enable without health pass.

## 9. Runbook - Callback outage

Steps:

1. Detect callback errors or retry backlog.
2. Pause queue if backlog threatens unsafe behavior.
3. Keep results in `CALLBACK_RETRY_PENDING`.
4. Do not resend without idempotency key.
5. Escalate to Order Core owner.
6. After recovery, replay bounded callbacks.

Blocked:

- Do not manually update order from IVR DB.
- Do not mark callback accepted without Order Core ack.

## 10. Runbook - Privacy incident

Triggers:

- Raw phone in log.
- Unauthorized admin access.
- Recording enabled without decision.
- Evidence contains unnecessary PII.

Steps:

1. Pause real calls.
2. Preserve audit trail.
3. Disable affected view/log sink if needed.
4. Notify security/privacy owner.
5. Rotate secrets if SIM/adapter credentials exposed.
6. Create evidence packet for incident.
7. Resume only after owner approval.

## 11. Rollback strategy

Rollback levels:

| Level | Action |
| --- | --- |
| Config rollback | Disable `IVR_SCHEDULER_ENABLED` or `REAL_CUSTOMER_CALL_ALLOWED`. |
| Queue rollback | Pause queue and stop dispatch. |
| Adapter rollback | Switch SIM mode to fake/internal disabled. |
| API rollback | Disable task intake if unsafe. |
| DB rollback | Prefer forward-fix migration; do not delete audit/evidence. |

## 12. Acceptance criteria

- Có config gate rõ cho real customer call.
- Có dashboards/alerts/runbooks cho queue, SIM, callback, evidence, privacy.
- Có deployment stages và exit gates.
- Có rollback strategy không phá audit/evidence.
