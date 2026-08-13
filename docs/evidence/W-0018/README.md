# W-0018 / P2-1 — Target V1 Task Intake

Status: `TESTS_PASS`. Đây là bằng chứng local `MOCK` và PostgreSQL disposable; không phải bằng chứng Sales thật, SIM/eSIM thật, LAB hay production.

> Current ownership note (`W-0019/P2-2`, 2026-08-13): restriction/sellable quyết định sau intake tại eligibility gate. Các reject sample/count bên dưới ghi lại đúng baseline commit P2-1; runtime hiện tại persist restricted/blocker snapshot ở `PENDING_ELIGIBILITY` rồi P2-2 ghi decision/reason/evidence atomically.

## Phạm vi đã hoàn thành

- `POST /v1/ivr/order-confirmation/tasks` nhận đúng Target V1, bắt buộc service-token/source allowlist, `Idempotency-Key` và `X-Correlation-Id`.
- Body tối đa 1 MiB, JSON/schema strict, từ chối field thừa và trả lỗi endpoint-specific `422 IVR_MALFORMED_REQUEST`.
- Chỉ chấp nhận hai matrix: `GOLDEN_HOUR + ONLINE` và `TWENTY_FOUR_SEVEN + COD`, cùng `ivr_confirmation_required=true`.
- Validation chạy theo thứ tự: identity/window → matrix → versioned attempt policy → contact reference/token → speech privacy → eligibility/restriction/evidence → exact approved script → execution mode.
- Speech snapshot bắt buộc tên hiển thị public-safe, mã đơn rút gọn, items, tổng tiền, tiền tệ, khu vực giao rút gọn và locale; raw phone hoặc full address bị fail-closed.
- Script phải resolve đúng `template/version/mode` từ P2-7; không fallback sang draft hoặc mode khác.
- Cùng key + cùng canonical payload replay đúng response ban đầu; đổi payload trả `IVR_IDEMPOTENCY_CONFLICT`. Concurrent duplicate hội tụ vào một atomic unit.
- PostgreSQL lưu task/job/intake-outbox/audit/idempotency trong một transaction; snapshot script/policy và outbox identity/payload được trigger bảo vệ immutable.
- `MOCK` tạo job `DRY_RUN` và outbox `HELD_MOCK`; không gọi real adapter. Non-MOCK vẫn bị giữ ở `HELD_ELIGIBILITY` cho các phase sau.
- Dial token chỉ đi qua opaque protector. MOCK dùng hash một chiều test-only; non-MOCK vẫn fail-closed khi chưa có protector thật.

## Response mẫu đã redacted

Accepted MOCK:

```json
{
  "decision": "TASK_ACCEPTED_DRY_RUN_ONLY",
  "ivr_call_job_id": "JOB-SYNTHETIC",
  "blocked_reasons": [],
  "evidence_ref": "evidence://mock/p2-1"
}
```

Operational reject:

```json
{
  "decision": "TASK_BLOCKED_OPERATIONAL",
  "ivr_call_job_id": null,
  "blocked_reasons": ["CALL_RESTRICTION_ACTIVE"],
  "evidence_ref": "evidence://mock/p2-1"
}
```

Schema failure trả `422` với stable code `IVR_MALFORMED_REQUEST`; missing trace trả `IVR_MISSING_TRACE`; changed-body replay trả `409 IVR_IDEMPOTENCY_CONFLICT`. Response/audit không chứa raw contact value hoặc full delivery address.

## Test và DB assertions

| Test ID / nhóm | Bằng chứng | Kết quả |
| --- | --- | --- |
| `UT-INTAKE-*` | hai program path, matrix/policy/contact/PII/script/window, persisted-metadata PII, duplicate/conflict, safe audit | `13/13 PASS` |
| API intake | happy paths, replay/conflict, strict schema, auth/source/trace, PII | `10/10 PASS` |
| `IT-INTAKE-DB-01` | 8 concurrent requests → task/job/outbox/idempotency/audit = `1/1/1/1/1`; protected token; immutable snapshot | `PASS` |
| `IT-INTAKE-DB-02` | restricted task → task/job/outbox = `0/0/0`, idempotency/audit = `1/1` | `PASS` |
| contract | OpenAPI route/headers/matrix/422 và canonical fixture classes | `2/2 PASS` |

Audit data chỉ gồm `decision`, `program_code`, `payment_method`, `execution_mode`, `payload_sha256`, `failure_code`, `occurred_at`; PII guard chạy trước khi persist.

## Final local gate — 2026-08-13

```text
dotnet restore Ivr.sln --locked-mode
PASS

dotnet format Ivr.sln --no-restore --verify-no-changes
PASS

dotnet build Ivr.sln -c Release --no-restore
PASS — 0 warnings / 0 errors

dotnet test Ivr.sln -c Release --no-build --no-restore
contract 21 + unit 80 + integration 43 = 144/144 PASS

merged coverage
TOTAL_LINE_COVERAGE=95.26% COVERED=18289 VALID=19200 REPORTS=3

dotnet ef migrations has-pending-model-changes
No changes have been made to the model since the last migration.

CI config, OpenAPI lint/parse/schema/hash/drift/negative and API docs
PASS — 2 OpenAPI files; 9 canonical tasks; 12 schema negatives rejected;
13 domain negatives schema-valid; 11 portal artifacts

admin-ui lint + production build
PASS

NuGet HIGH gate + admin-ui/deploy-ci npm HIGH audits
PASS — 0 findings

Docker Compose config
PASS

Gitleaks history scan at implementation commit `85c2b63`
PASS — 28 commits / 21.19 MB / no leaks

PII selftest + docs/evidence and ci-artifacts scan
PASS — CT-CI-06..06h; 140 text files; 2 binary files skipped

official Markdown map
PASS — 412 files / 375 resolved links / 0 unresolved
```

## Artifacts

- Runtime/API: `src/Ivr.Api/Intake/`, `src/Ivr.Infrastructure/Intake/`.
- Persistence: `20260813111817_P2_1_TaskIntake` migration, model snapshot, intake outbox mapping and retention ordering.
- Fixtures/specs: `seed/sales-target-v1.sample.json`, `seed/README.md`, database tables/indexes/privacy specs.
- Tests: `TaskIntakeServiceTests`, `TaskIntakeApiTests`, `TaskIntakePersistenceTests`, `TaskIntakeContractTests`.

## Residual gates

- Real Sales endpoint, service auth and production CDC are `NOT_RUN`; fake fixture không chứng minh Sales integration.
- LAB/PROD script approval, real opaque protector/key management, physical SIM/eSIM/modem và destination allowlist chưa có.
- P2-2 còn sở hữu eligibility/blocker orchestration trước khi một accepted intake có thể tiến sâu hơn.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`; không có real customer call và IVR không gửi SMS/customer notification.
- Hosted GitLab pipeline không được dùng để nâng trạng thái prompt này; protected `main` hiện vẫn chặn direct push.

## Commit/remote handoff

- Implementation commit: `85c2b63b6b386fcc7311a8c6c64385dacad5b31f` trên `main`.
- GitHub `main`: fast-forward thành công và đã xác minh remote ref trùng implementation commit.
- GitLab `main`: vẫn ở `5544395`; server từ chối direct push vì protected branch. Không hạ protection và không tạo branch/MR trái chỉ đạo owner.
