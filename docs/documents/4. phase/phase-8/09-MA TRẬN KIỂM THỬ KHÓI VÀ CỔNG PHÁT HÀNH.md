# IVR-09 - Test Matrix / Smoke / Release Gate

> **HISTORICAL_BASELINE / SUPERSEDED OVERLAY — 2026-08-27:** Smoke/gate trusted-skip bên dưới đã
> bị `OD-18`/`W-0123` thay thế bằng authority tests: M3 quyết định, IVR thực thi. Các gate an toàn
> và release khác vẫn phải được đánh giá độc lập.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa test matrix, smoke, evidence và release gate cho real customer call.

## 1. Mục tiêu

IVR chỉ được gọi khách thật khi documentation, implementation, privacy, security, smoke, evidence, owner sign-off và release decision đều đạt. Tài liệu này xác định acceptance test ở mức SRS.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Evidence/smoke/completion gate. |
| `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` | Release/go-live control. |
| `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` | Completion/evidence gateway. |
| `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-EVIDENCE-RELEASE-GATEWAY-PRODUCTION-READINESS-CONTROL.md` | Global smoke/UAT/release readiness. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | IVR final smoke matrix. |

## 3. Cổng tài liệu

Trước khi triển khai:

- `IVR-00` đến `IVR-09` hoàn tất.
- `IVR-SRS-trace-matrix.md` hoàn tất.
- PACK-09, TECH-09 và Phase 8 baseline không mâu thuẫn attempt policy.
- Owner decision chưa có nguồn được đánh dấu rõ.
- Không có production claim.

## 4. Cổng sẵn sàng triển khai

Trước khi bật staging/prod-like:

- Order Core chỉ tạo task cho Official Order đủ điều kiện.
- IVR không có quyền update order.
- Order Core revalidate mọi callback.
- Internal SIM Gateway Adapter có health monitoring.
- Evidence/audit writer hoạt động.
- RBAC server-side cho admin actions.
- Kill switch/pause queue/disable SIM có audit.

## 5. Ma trận smoke bắt buộc

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| IVR-SMK-001 | Quote đi vào IVR | Bị reject, không tạo CallJob. |
| IVR-SMK-002 | Cart đi vào IVR | Bị reject, không tạo CallJob. |
| IVR-SMK-003 | Order Draft đi vào IVR | Bị reject, không tạo CallJob. |
| IVR-SMK-004 | Official Order khách mới | Eligible nếu không có blocker. |
| IVR-SMK-005 | Khách trusted không risk | Skip IVR. |
| IVR-SMK-006 | Khách trusted có risk mới | Route IVR hoặc human review theo policy. |
| IVR-SMK-007 | Phone invalid format | Không gọi, invalid phone outcome. |
| IVR-SMK-008 | Phone validation technical error | Technical exception, không tính attempt. |
| IVR-SMK-009 | Golden Hour attempt policy | Chỉ 2 customer attempts trong 10 phút. |
| IVR-SMK-010 | 24/7 attempt policy | Chỉ 3 customer attempts trong 15 phút. |
| IVR-SMK-011 | Khách bấm `1` | Callback `IVR_CONFIRMED`, Core revalidate. |
| IVR-SMK-012 | Khách bấm `0` | Callback `IVR_CUSTOMER_CANCELLED`, Core cancel. |
| IVR-SMK-013 | No-answer chưa max | Queue attempt kế tiếp đúng schedule. |
| IVR-SMK-014 | No-answer max | Callback final, IVR không notification. |
| IVR-SMK-015 | SIM failure | Technical exception, không tính attempt. |
| IVR-SMK-016 | DTMF capture error | Technical exception, không tính customer fault. |
| IVR-SMK-017 | Sale Lock trước dispatch | Không gọi, operational blocked. |
| IVR-SMK-018 | Recall trong lúc gọi | Core block/hold khi callback. |
| IVR-SMK-019 | Payment issue xuất hiện sau phím `1` | Core không auto-confirm. |
| IVR-SMK-020 | Callback duplicate | Idempotent, không duplicate transition. |
| IVR-SMK-021 | Callback stale order version | Core reject stale hoặc hold review. |
| IVR-SMK-022 | Evidence unavailable | Result không được dùng transition. |
| IVR-SMK-023 | Admin retry technical | Permission/reason/evidence bắt buộc. |
| IVR-SMK-024 | Admin override bypass blocker | Bị chặn. |
| IVR-SMK-025 | Queue pause/resume | Audit đầy đủ, re-check windows. |
| IVR-SMK-026 | Disable/enable SIM | Permission/health evidence. |
| IVR-SMK-027 | Privacy script | Không đọc full address/payment/member/health note. |
| IVR-SMK-028 | Recording disabled | Không recording khi chưa approved. |
| IVR-SMK-029 | Production flag off | Không gọi khách thật. |
| IVR-SMK-030 | Release gate incomplete | `REAL_CUSTOMER_CALL_ALLOWED` vẫn `NO`. |

## 6. Gói bằng chứng

Release evidence tối thiểu:

- SRS trace matrix.
- Contract semantic review.
- Eligibility smoke evidence.
- Attempt policy smoke evidence.
- SIM Gateway internal-number smoke evidence.
- DTMF evidence.
- Callback/idempotency evidence.
- Race-condition evidence.
- Sale Lock/Recall/Suppression evidence.
- Admin RBAC evidence.
- Privacy review evidence.
- Security review evidence.
- Owner sign-off.
- Release decision audit.

## 7. Cổng release production

Required trước khi `REAL_CUSTOMER_CALL_ALLOWED = YES`:

- SRS approved.
- Implementation approved.
- Privacy/legal retention approved.
- Pilot scope approved.
- Kill switch tested.
- Queue pause tested.
- SIM disable tested.
- Duplicate callback tested.
- Stale callback tested.
- Evidence accepted.
- IVR Owner sign-off.
- Order Core Owner sign-off.
- Operational/Sale Lock Owner sign-off.
- Privacy Owner sign-off.
- Release Owner sign-off.

## 8. Cổng chặn lỗi

Release FAIL nếu:

- IVR gọi khách thật khi production flag chưa mở.
- IVR tự update order.
- IVR gọi Quote/Cart/Order Draft.
- Golden Hour có attempt thứ 3.
- 24/7 có attempt thứ 4.
- Technical failure bị tính là no-answer.
- Invalid phone bị tính là no-answer.
- Admin override bỏ qua Sale Lock/Recall/Suppression.
- Evidence chưa accepted nhưng gọi PASS.
- Privacy/retention chưa được duyệt nhưng bật recording hoặc production call.

## 9. Tiêu chí chấp nhận

- Test matrix bao phủ toàn bộ tình huống user yêu cầu.
- Release gate trả lời rõ khi nào được gọi khách thật.
- Không có smoke nào yêu cầu schema/API/code ở phase SRS này.
- Phase sau có thể sinh contract-tests từ smoke matrix này.

## 10. Expanded test catalog

| Group | Test IDs | Purpose |
| --- | --- | --- |
| Contract hygiene | IVR09-CON-* | Source-map, schema, API, event, fixture, stale wording. |
| Task intake | IVR09-TASK-* | Official Order only, idempotency, policy, contact, blocker. |
| Eligibility | IVR09-ELIG-* | Trusted skip, phone validation, opt-out, source unavailable. |
| Scheduler | IVR09-SCH-* | Program attempts, deadline, queue pause, capacity, concurrency. |
| SIM adapter | IVR09-SIM-* | DTMF, no-answer, technical failure, one SIM one call. |
| Result callback | IVR09-CB-* | Confirm/cancel/no-answer/invalid/technical/stale/block. |
| Admin | IVR09-ADM-* | RBAC, pause/resume, SIM disable/enable, manual retry. |
| Security/privacy | IVR09-SEC-* | Raw phone redaction, recording off, secrets hidden. |
| Release | IVR09-REL-* | Evidence packet, owner sign-off, config gate. |

## 11. Detailed smoke matrix

| Test ID | Setup | Action | Expected |
| --- | --- | --- | --- |
| IVR09-TASK-001 | Valid Official Order, Golden Hour | Create task | CallJob has 2 attempts. |
| IVR09-TASK-002 | Valid Official Order, 24/7 | Create task | CallJob has 3 attempts. |
| IVR09-TASK-003 | Quote/Cart/Draft | Try create task | Rejected, no CallJob. |
| IVR09-TASK-004 | Duplicate idempotency same payload | Retry task | Same result, no duplicate. |
| IVR09-TASK-005 | Duplicate idempotency different payload | Retry task | Conflict. |
| IVR09-ELIG-001 | Trusted customer no risk | Evaluate | Skip IVR. |
| IVR09-ELIG-002 | Trusted customer with risk | Evaluate | Require IVR or review. |
| IVR09-ELIG-003 | Invalid phone | Evaluate | No SIM dispatch. |
| IVR09-ELIG-004 | Phone validation technical error | Evaluate | Review/technical, not invalid final. |
| IVR09-SCH-001 | Golden Hour | Run scheduler | No attempt 3. |
| IVR09-SCH-002 | 24/7 | Run scheduler | No attempt 4. |
| IVR09-SCH-003 | Queue paused | Due attempts exist | No dispatch. |
| IVR09-SCH-004 | Two workers | Same due attempt | One dispatch only. |
| IVR09-SIM-001 | Fake adapter key `1` | Call | `IVR_CONFIRMED`. |
| IVR09-SIM-002 | Fake adapter key `0` | Call | `IVR_CUSTOMER_CANCELLED`. |
| IVR09-SIM-003 | Fake adapter no-answer | Call | Counted no-answer. |
| IVR09-SIM-004 | Fake adapter SIM failure | Call | Technical exception, not counted. |
| IVR09-CB-001 | Key `1`, no blockers | Callback | Core accepts/revalidates. |
| IVR09-CB-002 | Key `1`, Sale Lock appears | Callback | Core blocks/holds. |
| IVR09-CB-003 | No-answer final | Callback | No auto notification. |
| IVR09-CB-004 | Stale order version | Callback | Rejected stale. |
| IVR09-ADM-001 | No permission | Pause queue | 403. |
| IVR09-ADM-002 | Permission + reason | Pause queue | Queue paused, audit created. |
| IVR09-ADM-003 | Manual retry technical exception | Retry | Not counted as customer attempt. |
| IVR09-SEC-001 | Generate logs | Inspect | No raw phone/secrets. |
| IVR09-REL-001 | Missing evidence | Release check | Fail. |

## 12. Evidence checklist for release

| Evidence item | Required |
| --- | --- |
| Contract validator output | Yes |
| API/integration test output | Yes |
| DB migration test output | Yes |
| Scheduler/concurrency test output | Yes |
| SIM fake smoke output | Yes |
| Internal-number smoke output | Before pilot |
| Admin RBAC output | Yes |
| Security/privacy review | Yes |
| Evidence/audit samples | Yes |
| Race-condition test | Yes |
| Owner sign-off | Yes |
| Rollback runbook drill | Before production |

## 13. Release decision tree

```text
Are docs/contracts approved?
  NO -> stop
Are backend/admin/worker tests passing?
  NO -> stop
Are security/privacy gates passing?
  NO -> stop
Is evidence packet accepted?
  NO -> stop
Is internal-number smoke passing?
  NO -> stop
Has owner approved pilot scope?
  NO -> keep REAL_CUSTOMER_CALL_ALLOWED=false
YES -> allow limited pilot only
```

## 14. Production hard blocks

Production cannot enable real customer calls if any condition is true:

- `REAL_CUSTOMER_CALL_ALLOWED` is false.
- Release evidence missing.
- Privacy/recording decision missing.
- SIM adapter not smoke-tested.
- Scheduler can violate attempt count.
- Technical failure can become no-answer.
- Admin can bypass blocker.
- Order Core revalidation not tested.
- Rollback path not tested.

## 15. Release report template

Release report must include:

- Scope and environment.
- Commit/build identifier.
- Config flags.
- Test outputs.
- Evidence packet links.
- Known owner decisions.
- P0 blockers status.
- Rollback plan.
- Owner approval.

## 16. Final acceptance

Phase 8 can move from documentation/contract to implementation only when:

- `IVR-00` to `IVR-09` expanded and reviewed.
- `IVR-10` to `IVR-20` reviewed for SDS/backlog.
- Contract pass remains valid.
- Implementation backlog starts at database/API/fake adapter, not production SIM.
