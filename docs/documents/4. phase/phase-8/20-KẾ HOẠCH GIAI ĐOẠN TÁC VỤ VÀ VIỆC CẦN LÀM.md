# IVR-20 - Phase / Task / TODO Plan

> **HISTORICAL_PLAN / SUPERSEDED OVERLAY — 2026-08-27:** Work item trusted-skip phía IVR bên dưới
> đã bị `OD-18`/`W-0123` thay thế. Giữ nội dung cũ để truy vết, không dùng làm backlog hiện hành.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `IMPLEMENTATION_BACKLOG_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Backlog triển khai chi tiết từ SRS/SDS/contract sang API, DB, service, admin, adapter, test, release.

## 1. Mục tiêu

Tài liệu này chia Phase 8 thành các slice triển khai có thứ tự, dependency, deliverable và done gate. Đây là tài liệu điều phối dev, không phải bằng chứng release.

## 2. Definition of Ready

Một task được đưa vào dev khi:

- Có source/SRS/SDS/contract tham chiếu.
- Có input/output rõ.
- Có owner và dependency rõ.
- Có test expected rõ.
- Không yêu cầu Owner Decision chưa được chốt, trừ khi task chỉ tạo guard/placeholder an toàn.

## 3. Definition of Done

Một task được xem là done khi:

- Code/test/docs liên quan được cập nhật.
- Unit/integration tests pass.
- Không bypass source-of-truth.
- Có audit/evidence nếu task rủi ro.
- Feature flag/release gate không bị mở sai.
- Contract validator vẫn pass nếu có chỉnh contract.

## 4. Phase slices

| Slice | Tên | Mục tiêu | Dependency | Gate |
| --- | --- | --- | --- | --- |
| P8.0 | Baseline docs/contracts | SRS/SDS/contract đầy đủ. | None | Validator pass. |
| P8.1 | Database foundation | Tạo bảng IVR. | P8.0 | Migration/test pass. |
| P8.2 | Internal API shell | Task/callback/admin endpoints với auth/idempotency. | P8.1 | API tests pass. |
| P8.3 | Order Core task publisher | Tạo task đúng rule. | P8.2 | Official Order scenarios pass. |
| P8.4 | Eligibility and policy | Trusted skip, phone validation, blocker check, attempt resolver. | P8.3 | Policy tests pass. |
| P8.5 | Scheduler/queue | Deadline-aware scheduler, CallJob/Attempt state. | P8.4 | Concurrency tests pass. |
| P8.6 | SIM adapter fake/internal | Fake adapter, internal-number adapter boundary. | P8.5 | Adapter smoke pass. |
| P8.7 | Result callback/revalidation | Normalize result, callback, Order Core ack handling. | P8.6 | Race tests pass. |
| P8.8 | Admin monitoring/actions | Queue/SIM/incident/retry/review UI/API. | P8.5 | RBAC tests pass. |
| P8.9 | Evidence/security/privacy | Evidence packet, audit, privacy, retention, logs. | P8.2-P8.8 | Security/privacy gate pass. |
| P8.10 | Smoke/UAT/release | Dry-run, internal number smoke, release packet. | P8.9 | IVR-09 gate pass. |
| P8.11 | Pilot/production | Owner-approved pilot/rollout. | P8.10 | Owner Decision Required. |

## 5. Detailed tasks

### P8.0 - Baseline docs/contracts

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T001 | Chốt SRS `IVR-00` đến `IVR-09`. | SRS docs. | Review accepted. |
| P8-T002 | Chốt SDS `IVR-10` đến `IVR-20`. | SDS/backlog docs. | Review accepted. |
| P8-T003 | Chốt contract schemas/enums/API/events/state machines. | Contract files. | `node scripts\validate-contracts.mjs` pass. |
| P8-T004 | Tạo issue/task tracker theo slice. | Backlog tickets. | Mỗi ticket có source link. |

### P8.1 - Database foundation

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T010 | Thiết kế migration IVR tables. | Migration draft. | DBA/backend review. |
| P8-T011 | Tạo repositories/read models. | Repository classes. | Unit tests pass. |
| P8-T012 | Tạo idempotency storage integration. | Idempotency guard. | Duplicate tests pass. |
| P8-T013 | Tạo scheduler indexes. | DB indexes. | Query plan acceptable. |
| P8-T014 | Tạo seed SIM channels disabled/non-prod. | Seed config. | Production safe defaults. |

### P8.2 - Internal API shell

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T020 | Implement task intake endpoint. | API route/controller. | Contract/request tests. |
| P8-T021 | Implement eligibility endpoint/projection. | API/service. | Validation tests. |
| P8-T022 | Implement call job/attempt/result endpoints. | API routes. | Auth/idempotency tests. |
| P8-T023 | Implement result callback endpoint/client boundary. | API/client. | Callback tests. |
| P8-T024 | Implement admin API shell. | Queue/SIM/retry/review routes. | RBAC tests. |

### P8.3 - Order Core task publisher

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T030 | Add Official Order IVR eligibility hook. | Domain/service hook. | Quote/Cart/Draft blocked. |
| P8-T031 | Build `IvrConfirmationTaskV1`. | Task builder. | Snapshot tests. |
| P8-T032 | Add trusted customer skip. | Trust integration. | Trusted skip tests. |
| P8-T033 | Add blocker snapshot integration. | Operational Core read/check. | Sale Lock/Recall tests. |
| P8-T034 | Add task delivery retry/hold. | Delivery service. | IVR unavailable tests. |

### P8.4 - Eligibility and policy

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T040 | Implement phone validation policy. | Phone validation service. | Invalid/unknown/technical tests. |
| P8-T041 | Implement official contact resolver boundary. | Projection integration. | No full profile tests. |
| P8-T042 | Implement attempt policy resolver. | Program schedule resolver. | 2/3 attempts tests. |
| P8-T043 | Implement pre-dispatch revalidation. | Scheduler guard. | Blocker race tests. |
| P8-T044 | Implement eligibility audit/evidence. | Audit writer. | Evidence tests. |

### P8.5 - Scheduler/queue

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T050 | Implement CallJob state machine. | Domain service. | Transition tests. |
| P8-T051 | Implement Attempt state machine. | Domain service. | Transition tests. |
| P8-T052 | Implement deadline-aware scheduler. | Worker. | Ordering/window tests. |
| P8-T053 | Implement worker locking. | Lock/transaction guard. | Duplicate dispatch test. |
| P8-T054 | Implement capacity incident detection. | Monitor/service. | Incident tests. |

### P8.6 - SIM adapter fake/internal

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T060 | Define SIM adapter interface. | Interface/contract. | Code review. |
| P8-T061 | Implement fake deterministic adapter. | Fake adapter. | Unit tests. |
| P8-T062 | Implement internal-number adapter mode. | Internal adapter. | Internal smoke only. |
| P8-T063 | Implement SIM channel manager. | Reserve/release/health. | One SIM one call tests. |
| P8-T064 | Implement DTMF capture mapping. | Handler. | Key 1/0/invalid tests. |

### P8.7 - Result callback/revalidation

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T070 | Implement result normalizer. | Normalization service. | Mapping tests. |
| P8-T071 | Implement result persistence. | Result repository/service. | Required fields tests. |
| P8-T072 | Implement callback client. | Order Core callback client. | Idempotency/retry tests. |
| P8-T073 | Implement Order Core callback handler/revalidation. | Core handler. | Race tests. |
| P8-T074 | Implement stale/block/review ack handling. | Callback state update. | Stale/block tests. |

### P8.8 - Admin monitoring/actions

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T080 | Queue dashboard API/UI. | Admin page/API. | Permission tests. |
| P8-T081 | CallJob detail API/UI. | Admin detail. | Masked phone only. |
| P8-T082 | SIM channel admin API/UI. | Enable/disable flow. | RBAC/audit tests. |
| P8-T083 | Capacity incident page. | Incident workflow. | Pause/resume tests. |
| P8-T084 | Manual technical retry action. | Admin action. | No customer count tests. |
| P8-T085 | Result review action. | Review workflow. | No order override tests. |

### P8.9 - Evidence/security/privacy

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T090 | Evidence writer integration. | Evidence refs. | Evidence tests. |
| P8-T091 | Audit writer integration. | Audit records. | Audit tests. |
| P8-T092 | Log redaction. | Redaction rules. | Raw phone absent tests. |
| P8-T093 | Secret handling review. | Secret config. | Security review. |
| P8-T094 | Privacy retention policy decision. | Owner decision record. | Approved before production. |
| P8-T095 | Recording policy decision. | Owner decision record. | Default disabled. |

### P8.10 - Smoke/UAT/release

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T100 | Dry-run E2E suite. | E2E tests. | All pass. |
| P8-T101 | Internal-number smoke. | Smoke evidence. | Owner accepted. |
| P8-T102 | Security/privacy smoke. | Evidence packet. | Security pass. |
| P8-T103 | Admin operations smoke. | Queue/SIM/retry evidence. | Ops pass. |
| P8-T104 | Race-condition smoke. | Race evidence. | Order Core pass. |
| P8-T105 | Release packet assembly. | Evidence package. | Release owner accepted. |

### P8.11 - Pilot/production

| Task ID | Task | Deliverable | Done gate |
| --- | --- | --- | --- |
| P8-T110 | Define pilot scope. | Owner decision. | Approved. |
| P8-T111 | Enable limited real-call flag. | Config change. | Audited. |
| P8-T112 | Monitor pilot metrics. | Pilot report. | No P0 violation. |
| P8-T113 | Rollback drill. | Rollback evidence. | Ops accepted. |
| P8-T114 | Production rollout decision. | Release decision. | Owner approved. |

## 6. P0 blockers

Phase 8 must stop immediately if:

- IVR/SIM can directly update order state.
- Real customer call flag is enabled before release gate.
- Technical failure is counted as no-answer.
- Golden Hour creates attempt 3.
- 24/7 creates attempt 4.
- Admin action bypasses Sale Lock/Recall/Suppression/opt-out.
- Raw phone appears in logs/UI without approved policy.
- Evidence/audit is missing for high-risk transitions.

## 7. Owner Decision Required backlog

| Decision | Needed before |
| --- | --- |
| Trusted customer threshold and risk flags | P8.3/P8.4 production use. |
| Permanent invalid phone criteria | P8.4. |
| Technical retry count/backoff | P8.6/P8.7. |
| SIM Gateway hardware/API protocol | P8.6 internal adapter. |
| Recording allowed or prohibited | P8.9/P8.10. |
| Retention duration by data class | P8.9/P8.10. |
| Pilot real customer scope | P8.11. |
| Notification template after Core cancellation | Notification owner phase. |

## 8. Suggested implementation order

1. Finish docs/contracts review.
2. Implement DB foundation.
3. Implement API shell with fake adapter only.
4. Implement Order Core task publisher.
5. Implement eligibility/policy/scheduler.
6. Implement fake SIM flow and result callback.
7. Implement admin monitoring/actions.
8. Implement evidence/security/privacy gates.
9. Run dry-run and internal-number smoke.
10. Request owner decision for pilot.

## 9. Acceptance criteria

- Backlog covers API, DB, services, workflow, admin, security, privacy, test, release.
- Every task has deliverable and done gate.
- P0 blockers and owner decisions are explicit.
- Implementation order does not require real customer call before smoke/release evidence.
