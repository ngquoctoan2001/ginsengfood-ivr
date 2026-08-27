# IVR-19 - Test / Smoke / Release Plan

> **HISTORICAL_PLAN / SUPERSEDED OVERLAY — 2026-08-27:** Các test/release step trusted-skip bên
> dưới đã bị `OD-18`/`W-0123` thay thế; không dùng chúng làm acceptance hiện hành. Real-call và
> production gates vẫn fail-closed độc lập.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Kế hoạch test và release để triển khai IVR an toàn.

## 1. Mục tiêu

Tài liệu này định nghĩa test layers, smoke scenarios, evidence requirements và release gates cho Phase 8. Không được gọi khách thật production khi chưa pass đủ gate.

## 2. Test layers

| Layer | Mục tiêu | Ví dụ |
| --- | --- | --- |
| Static/contract validation | Contract hygiene. | `node scripts\validate-contracts.mjs`, stale checks. |
| Unit tests | Test service/function nhỏ. | Policy resolver, result normalizer, permission guard. |
| Integration tests | Test DB/API/service với fake dependencies. | Task intake creates CallJob. |
| Contract tests | Provider/consumer payload compatibility. | Task/callback/event fixtures. |
| Worker tests | Scheduler/queue/concurrency. | No duplicate dispatch, deadline ordering. |
| Adapter tests | SIM fake/internal adapter. | DTMF mapping, technical failure. |
| Admin tests | RBAC/admin actions. | Pause/resume, disable SIM, manual retry. |
| E2E dry-run | Full flow không gọi khách. | Order -> task -> fake call -> callback. |
| Internal-number smoke | Gọi số nội bộ được duyệt. | DTMF 1/0/no-answer. |
| UAT | Owner review bằng evidence. | Release packet. |

## 3. Unit test matrix

| Module | Scenarios |
| --- | --- |
| `AttemptPolicyResolver` | Golden Hour 2 attempts; 24/7 3 attempts; reject mismatch; no attempt outside window. |
| `IvrEligibilityService` | Official Order only; trusted skip; invalid phone; opt-out; Sale Lock/Recall; resolver unavailable fail-safe. |
| `ResultNormalizer` | DTMF 1; DTMF 0; no-answer attempt; no-answer final; invalid phone; technical exception; operational block. |
| `SimChannelManager` | One SIM one active call; disabled SIM not selected; release after call; health failure. |
| `ResultCallbackClient` | Idempotency; stale response; blocked response; retry bounded. |
| `IvrAdminActionService` | Permission required; reason required; no policy bypass; evidence required. |

## 4. Integration test matrix

| Scenario | Expected |
| --- | --- |
| Order Core sends valid Golden Hour task | Task accepted, CallJob created with 2 attempts. |
| Order Core sends valid 24/7 task | Task accepted, CallJob created with 3 attempts. |
| Duplicate task same idempotency | Returns existing task/job, no duplicate. |
| Program mismatch max attempts | Reject/conflict. |
| Quote/Cart/Draft | Reject/not created. |
| Phone invalid | No call attempt, invalid-phone final/review. |
| Operational blocker before dispatch | Attempt blocked, no SIM call. |
| Evidence write failure | Route technical exception/admin review. |
| Callback stale | No order transition, callback state stale. |

## 5. Scheduler/concurrency tests

Must pass:

- Golden Hour cannot create attempt 3.
- 24/7 cannot create attempt 4.
- Two workers cannot dispatch same attempt.
- Two workers cannot reserve same SIM.
- Queue pause stops new dispatch.
- Capacity incident holds queue.
- Technical retry does not increment customer attempt.
- Window expiry prevents late call.

## 6. Adapter smoke tests

Fake adapter:

- DTMF `1` returns `IVR_CONFIRMED`.
- DTMF `0` returns `IVR_CUSTOMER_CANCELLED`.
- No-answer returns non-final/final based on attempt number.
- SIM failure returns technical exception.
- DTMF capture error returns technical exception.

Internal-number adapter:

- Gọi số nội bộ test.
- Script đúng nội dung approved.
- Key `1` capture đúng.
- Key `0` capture đúng.
- Không bấm phím timeout đúng.
- Call log/evidence tạo đúng.

## 7. Security/privacy tests

- Admin không có permission không pause/resume được.
- Admin không có permission không disable/enable SIM được.
- Manual retry yêu cầu permission/reason/evidence.
- Raw phone không xuất hiện trong app logs.
- Admin UI chỉ hiển thị phone masked.
- Recording disabled by default.
- SIM credentials không expose qua API/UI/log.
- IVR/SIM service không có credential update order.

## 8. E2E dry-run scenarios

| ID | Scenario | Expected |
| --- | --- | --- |
| IVR-E2E-001 | Official Order untrusted, Golden Hour, key `1` | Callback accepted after Core revalidation. |
| IVR-E2E-002 | Official Order untrusted, Golden Hour, key `0` | Core cancels only after revalidation. |
| IVR-E2E-003 | Golden Hour no-answer twice | Final no-answer callback, no auto notification. |
| IVR-E2E-004 | 24/7 no-answer three times | Final no-answer callback, no attempt 4. |
| IVR-E2E-005 | Phone invalid | No SIM dispatch, invalid-phone final/review. |
| IVR-E2E-006 | SIM failure | Technical exception, no attempt count. |
| IVR-E2E-007 | Key `1` then Sale Lock before callback ack | Core blocks/holds, no auto confirm. |
| IVR-E2E-008 | Trusted customer skip | No IVR call job. |
| IVR-E2E-009 | Queue paused | Due attempts not dispatched. |
| IVR-E2E-010 | Admin manual retry | Retry technical only, no policy bypass. |

## 9. Evidence packet

Release evidence must include:

- Contract validation output.
- API test output.
- DB migration test output.
- Scheduler/concurrency test output.
- Adapter fake smoke evidence.
- Internal-number smoke evidence if used.
- Security/privacy test output.
- Admin RBAC test output.
- Race-condition test output.
- Evidence/audit samples.
- Owner sign-off.

## 10. Release gates

| Gate | Required |
| --- | --- |
| Contract gate | Validator pass 0 warning. |
| Build/test gate | Relevant backend/admin tests pass. |
| Security gate | RBAC, auth, secret, log redaction pass. |
| Privacy gate | Phone/recording/call log policy approved. |
| Evidence gate | Evidence packet accepted. |
| Operational gate | Queue pause/resume, SIM disable/enable, incident runbook tested. |
| Business gate | Owner approves real-call scope. |
| Production gate | `REAL_CUSTOMER_CALL_ALLOWED` can be enabled only after all gates pass. |

## 11. Release levels

| Level | Allowed |
| --- | --- |
| `L0_DOC_CONTRACT` | Docs/contracts only. |
| `L1_DRY_RUN_API` | API/DB/fake adapter, no real SIM. |
| `L2_INTERNAL_NUMBER_SMOKE` | Internal numbers only. |
| `L3_OWNER_APPROVED_PILOT` | Limited real customer pilot if approved. |
| `L4_PRODUCTION_ROLLOUT` | Production rollout after evidence and owner approval. |

## 12. Acceptance criteria

- Test plan covers all acceptance scenarios from SRS and contract pass.
- Release gates block real customer call until evidence pass.
- There is a clear dry-run/internal-number path before pilot.
- Race, idempotency, technical retry, capacity, RBAC, privacy are tested.
