# PROMPT P2-8 — IVR Internal & Admin API Implementation

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-8` |
| **Work ID** | `W-0065` (canonical tracker §5) |
| **Phase** | 2 — Core runtime in MOCK |
| **Prereq (blockedBy)** | `P0-4` (flags/kill switch), `P2-1`, `P2-3`, `P2-5`, `P2-6` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL · GitLab CI |
| **Execution mode** | `MOCK` |

## 1. ROLE
Bạn là **Senior .NET Backend Engineer**. Bạn implement phần API còn thiếu của IVR: 6 internal lifecycle operation và 7 admin operation đã khai báo trong OpenAPI nhưng **chưa có prompt nào build**. Bạn ưu tiên authz chặt, idempotency, audit append-only và PII masking. Bạn KHÔNG mở thêm quyền cho IVR đối với order state.

## 2. CONTEXT
`specs/api/openapi/ivr-order-confirmation.v1.yaml` hiện khai báo 17 operation: 14 operation thuộc phạm vi P2-8 (intake + 6 lifecycle + 7 admin) và 3 operation feature-flag đã được triển khai ở P0-4. `P2-1` chỉ phủ `POST /tasks`. 13 operation P2-8 còn lại (`recordEligibility`, `createCallJob`, `getCallJob`, `recordAttempt`, `recordResult`, `recordResultCallback`, `getQueue`, `pauseQueue`, `resumeQueue`, `disableSim`, `enableSim`, `technicalRetry`, `adminReview`) không có build step ở bất kỳ prompt nào, trong khi `P0-3` định nghĩa permission cho chúng, `P3-1..P3-4` build UI gọi chúng và `P5-2` drive chúng qua Playwright. Slice này đóng lỗ hổng đó.

Đây là **IVR-internal/admin API**, không phải outbound Sales callback. Outbound callback là `P2-6` và dùng `order-core-ivr-callback.target-v1.yaml`. Hai surface không được trộn.

## 3. SOURCE SPECS (đọc trước khi code — bắt buộc)
- `specs/api/openapi/ivr-order-confirmation.v1.yaml` (17 operation tổng; 14 thuộc P2-8, `ErrorCode`, `IvrTaskIntakeResult`)
- `specs/api/02-internal-api.md`, `specs/api/03-admin-api.md`, `specs/api/06-error-codes.md`, `specs/api/07-idempotency-and-correlation.md`
- `specs/functional/07-admin-operations.md`, `specs/functional/06-technical-exception-capacity.md`
- `specs/database/02-tables.md` §7 §8 §9, `specs/database/04-indexes.md` §5
- `specs/ui/06-callback-request.md`, `specs/ui/08-role-permission-ui.md`
- `plan/ivr-orther/decisions-log.md` §DF-01 (permission set), §DF-04 (idempotency/audit), §D-02 (Core owns transition)
- `prompt/README-governance.md` §2, §4, §5

## 4. DECISIONS & CONSTRAINTS
- **DF-01 (LOCKED):** permission set hiện có `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW`. Mỗi admin endpoint map đúng **một** permission.
- **`OD-V1-20` (đã duyệt 2026-08-22):** quyền `IVR_RUNTIME_GATE_ADMIN` cho allowlist/kill-switch **đã cấp cho role `Admin`** (Operator không có). Endpoint liên quan (thuộc `P0-4`) vẫn fail-closed, nhưng ở tầng khác: `IRuntimeGateAuthorization` production luôn `false` → `409 IVR_OPERATIONAL_BLOCKED`. Prompt này không tự cấp quyền mới và **không** được thay `PendingRuntimeGateAuthorization`.
- **D-02:** không endpoint nào ở đây được đổi order state hoặc ghi sang Sales. `recordResultCallback` chỉ ghi **lifecycle nội bộ**, không phải lời gọi Sales.
- **DF-04:** mọi POST bắt buộc `Idempotency-Key` + `X-Correlation-Id`; replay cùng key+hash trả snapshot cũ; khác hash → `409 IVR_IDEMPOTENCY_CONFLICT`.
- **Audit:** mọi admin action ghi `ivr_admin_actions` + `ivr_audit_log` append-only với `reason`, `actor_id`, `before_state`, `after_state`, `no_policy_bypass=true`.
- **Không bypass:** `technicalRetry` không được reset customer attempt count, không vượt policy max, không bỏ qua blocker/kill switch/allowlist. `adminReview` không được sửa `result_type` thật.

## 5. INPUTS / DEPENDENCIES
- `REAL_AVAILABLE`: OpenAPI contract, DB schema (P1-2), domain model (P1-3), idempotency/audit store (P0-3), flags/kill switch (P0-4).
- `MOCK_REQUIRED`: fake Sales provider, mock SIM adapter cho các lifecycle endpoint.
- `OWNER_DECISION_REQUIRED`: `OD-V1-20` đã đóng phần permission (2026-08-22); còn treo chữ ký four-eyes Security/Platform + Release owner.
- `BLOCKED_EXTERNAL`: không có — slice này chạy trọn vẹn trong MOCK.

## 6. BUILD STEPS
1. **Internal lifecycle endpoints** (`src/Ivr.Api/Internal/`): `POST /eligibility-checks`, `POST /call-jobs`, `GET /call-jobs/{ivrCallJobId}`, `POST /call-attempts`, `POST /call-results`, `POST /result-callbacks`. Mỗi handler: validate contract → idempotency → persist → trả DTO sinh từ OpenAPI. `GET /call-jobs/{id}` trả **masked** view (không raw phone, không `dial_token`).
2. **Caller identity:** internal endpoint chỉ chấp nhận service identity của chính IVR worker/adapter (scope `ivr.internal.write`, xem `OD-V1-07` cho production profile). Từ chối admin session và caller ngoài allowlist service → `403 IVR_FORBIDDEN_CALLER`. Ghi rõ trong `specs/api/02-internal-api.md` rằng đây không phải public surface.
3. **Admin queue endpoints** (`src/Ivr.Api/Admin/`): `GET /queue` (perm `IVR_QUEUE_VIEW`), `POST /queue:pause` (`IVR_QUEUE_PAUSE`), `POST /queue:resume` (`IVR_QUEUE_RESUME`). Pause phải dừng dispatch mới nhưng **không** hủy call đang chạy; ghi `capacity_incident` nếu pause gây miss deadline.
4. **SIM channel admin**: `POST /sim-channels/{id}:disable` (`IVR_SIM_DISABLE`), `:enable` (`IVR_SIM_ENABLE`). Enable bị chặn khi `adapter_mode=REAL` mà chưa có gate (DT-01/DF-03). Bổ sung trạng thái `QUARANTINED` và không cho enable trực tiếp từ `QUARANTINED` khi chưa reconcile (xem `specs/database/04-indexes.md` §5).
5. **Technical retry**: `POST /technical-retries` (`IVR_MANUAL_RETRY`) — `is_counted_customer_attempt=false`, bounded theo `technical_retry_count`, kiểm lại blocker + kill switch + allowlist + mode trước khi cho phép.
6. **Admin review**: `POST /admin-reviews` (`IVR_RESULT_REVIEW`) — ghi annotation/resolution vào `ivr_review_items`, **không** thay đổi `result_type`/`final_result_status` gốc.
7. **Response typing**: mọi operation trả body có schema (không còn `200` rỗng); bổ sung `401`/`403`/`409`/`422`/`429`/`500` theo `ErrorEnvelope`. Cập nhật OpenAPI trong cùng work item nếu phải thêm schema.
8. **PII masking filter** áp cho toàn bộ response và log của cả 13 endpoint.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Api/Internal/**` | 6 internal lifecycle endpoint + handler |
| `src/Ivr.Api/Admin/**` | 7 admin endpoint + permission attribute |
| `src/Ivr.Api/Filters/PiiMaskingFilter.cs` | Masking response/log |
| `src/Ivr.Application/**` | command/query handler, idempotent wrapper |
| `specs/api/openapi/ivr-order-confirmation.v1.yaml` (cập nhật) | typed response cho 9 operation đang trả `200` rỗng; thêm 401/429/5xx |
| `tests/Ivr.IntegrationTests/Api/**` | test theo §8 |

**Chuẩn output:** tuân `prompt/README-governance.md` §4; mọi public API có XML doc; không magic number; log có `correlationId`.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-API-AUTHZ-01` | integration | Mỗi admin endpoint thiếu đúng permission → `403 IVR_FORBIDDEN_CALLER`; đủ permission → pass. |
| `IT-API-AUTHZ-02` | integration | Admin session gọi internal endpoint → 403; service identity sai scope → 403. |
| `IT-API-IDEMP-03` | integration | Replay cùng key+hash → snapshot cũ; khác hash → `409 IVR_IDEMPOTENCY_CONFLICT`. |
| `IT-API-AUDIT-04` | integration | Mọi admin action ghi `ivr_admin_actions` + `ivr_audit_log` với reason/actor/before/after; audit không thể UPDATE/DELETE. |
| `IT-API-PII-05` | integration | Response và log của 13 endpoint không chứa raw phone, full address hoặc `dial_token`. |
| `IT-API-RETRY-06` | integration | `technical-retries` không tăng customer attempt, không vượt bound, bị chặn khi kill switch ON hoặc đích ngoài allowlist. |
| `IT-API-REVIEW-07` | integration | `admin-reviews` không đổi `result_type` gốc; `no_policy_bypass=true` được ghi. |
| `IT-API-QUEUE-08` | integration | pause dừng dispatch mới, không hủy call đang chạy; resume khôi phục; cả hai được audit. |
| `IT-API-SIM-09` | integration | enable bị chặn khi `adapter_mode=REAL` chưa qua gate; `QUARANTINED` không enable trực tiếp. |
| `CT-API-OAS-10` | contract | Mọi operation khớp OpenAPI (status code, schema, header bắt buộc). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 13 operation có handler, permission, idempotency, audit và test.
- [ ] Không endpoint nào ghi order state hoặc gọi Sales.
- [ ] Không endpoint nào bypass blocker/kill switch/allowlist/policy max.
- [ ] OpenAPI và implementation không drift (CT-API-OAS-10 xanh).

**Reviewer (GitLab MR):** kiểm permission mapping 1-1 với DF-01; kiểm audit append-only; kiểm masking; kiểm internal vs admin surface không lẫn.

## 10. EVIDENCE EXPECTED
Ghi vào `docs/evidence/W-0065/`: test report 10 nhóm test, sample 403 cho từng permission, sample 409 idempotency conflict, dump `ivr_audit_log` đã redact, OpenAPI diff, coverage report.

## 11. FORBIDDEN
- ❌ IVR transition/ghi order state (D-02).
- ❌ Cấp permission mới ngoài DF-01 + `OD-V1-20` khi chưa có quyết định owner tương ứng.
- ❌ Endpoint admin bypass blocker, kill switch, allowlist hoặc policy max.
- ❌ Trả raw phone/full address/`dial_token` trong response hoặc log.
- ❌ Trộn internal record DTO với outbound Sales callback DTO.
- ❌ Gọi khách thật hoặc bật real adapter.

## 12. DEFINITION OF DONE
- [ ] Build + test + lint pass trong GitLab pipeline (hosted evidence có thể `NOT_RUN` nếu `W-0061` chưa đóng — ghi trung thực).
- [ ] 10 nhóm test §8 xanh; evidence §10 đầy đủ trong `docs/evidence/W-0065/`.
- [ ] OpenAPI cập nhật cho 9 operation trả `200` rỗng; drift check xanh.
- [ ] Cập nhật `specs/api/02-internal-api.md`/`03-admin-api.md` nếu contract đổi.
- [ ] Đạt tối đa `TESTS_PASS` (mock-only). Không tuyên bố integration-verified.

## 13. TRACKER UPDATE (bắt buộc)
- Before: `W-0065` → `IN_PROGRESS` + baseline/prereq.
- During: checkpoint; mọi dependency phát sinh lấy Work ID kế tiếp.
- After: files, commands/results, evidence links, residual gates; chỉ reviewer/owner chuyển `ACCEPTED`.
