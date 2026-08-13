# W-0019 / P2-2 — Eligibility and Blockers

Status: `TESTS_PASS`. Evidence này chỉ chứng minh rule/runtime local `MOCK` và transaction trên PostgreSQL disposable; không phải bằng chứng Sales/CRM/Ops, SIM/eSIM, LAB hoặc production.

## Phạm vi đã hoàn thành

- Task intake tạo call job với `PENDING_ELIGIBILITY`; P2-2 là cổng duy nhất đánh dấu `ELIGIBLE_FOR_IVR` trước scheduler.
- Rule thuần chạy đúng thứ tự: official/state → program/payment → per-line sellable → `PHONE_CALL` restriction → contact/token → window → capacity → trust → eligible.
- Sellable thiếu, stale, `UNKNOWN`, field con không xác định hoặc trạng thái blocker đều fail-closed với stable reason và evidence link.
- `call_restriction=true` chặn voice. SMS opt-out được tách semantics và không chặn voice trong rule P2-2.
- Trust-skip path vẫn tồn tại trong rule nhưng application service hard-code flag `false`; khách `TRUSTED` vẫn phải qua IVR và nhận advisory `TRUST_SKIP_DISABLED_REQUIRE_IVR`.
- Capacity provider chỉ được hỏi sau khi các gate trước đã qua. Provider lỗi/thiếu fail-closed; không đáp ứng deadline tạo `IVR_CAPACITY_EXCEPTION` và capacity incident.
- PostgreSQL cập nhật task, job, intake outbox, reason, evidence link, audit và capacity incident trong một transaction có advisory lock.
- Capacity exception không tạo `CallAttempt`, không được tính no-answer/customer attempt và không dispatch.
- `MOCK` eligible vẫn giữ job `DRY_RUN`, queue/outbox `HELD_MOCK`; `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi.
- Không có HTTP client hoặc lời gọi trực tiếp Ops/CRM trong eligibility service/repository; service chỉ đọc snapshot đã lưu từ Sales/Order Core.

## Decision samples đã redacted

Block sellable:

```json
{
  "eligible": false,
  "decision": "TASK_BLOCKED_OPERATIONAL",
  "reasons": [
    {
      "code": "INVENTORY_NOT_SELLABLE",
      "signal": "sellable_status[0]",
      "evidence_ref": "evidence://synthetic/p2-2#eligibility/sellable/0"
    }
  ],
  "is_counted_customer_attempt": false
}
```

Capacity exception:

```json
{
  "eligible": false,
  "decision": "IVR_CAPACITY_EXCEPTION",
  "reasons": [
    {
      "code": "CAPACITY_DEADLINE_UNAVAILABLE",
      "signal": "capacity.deadline",
      "evidence_ref": "evidence://synthetic/p2-2/capacity-shortage"
    }
  ],
  "is_counted_customer_attempt": false
}
```

## Tests

| Test ID | Bằng chứng | Kết quả |
| --- | --- | --- |
| `UT-ELIG-BLOCK-01` | một line `NOT_SELLABLE` block; toàn bộ `SELLABLE` eligible | `PASS` |
| `UT-ELIG-DNC-02` | phone-call restriction block; SMS opt-out only không block voice | `PASS` |
| `UT-ELIG-TRUST-03` | trusted + allowed vẫn require IVR khi skip flag off | `PASS` |
| `UT-ELIG-FAILCLOSED-04` | thiếu sellable/restriction và sellable unknown đều hold | `PASS` |
| `IT-ELIG-CAP-05` | incident + evidence + audit + held job; `CallAttempt=0` | `PASS` |
| `IT-ELIG-MOCK-06` | eligible MOCK vẫn `DRY_RUN/HELD_MOCK`; attempt/incident `0/0` | `PASS` |
| `IT-ELIG-DNC-07` | stored phone restriction block trước capacity; evidence có, attempt/incident `0/0` | `PASS` |
| `IT-ELIG-FAILCLOSED-08` | capacity response thiếu evidence → admin hold, không attempt/incident | `PASS` |

Focused result: unit `4/4`, PostgreSQL integration `4/4`.

## Local gates — 2026-08-13

```text
dotnet format Ivr.sln --no-restore --verify-no-changes
PASS

dotnet build Ivr.sln -c Release --no-restore -p:RunAnalyzers=true
PASS — 0 warnings / 0 errors

dotnet test Ivr.sln -c Release --no-build --no-restore --collect:"XPlat Code Coverage"
contract 21 + unit 84 + integration 47 = 152/152 PASS

merged coverage
TOTAL_LINE_COVERAGE=94.71% COVERED=18870 VALID=19925 REPORTS=3

dotnet ef migrations has-pending-model-changes
No changes have been made to the model since the last migration.

CI config, OpenAPI lint/parse/schema/hash/drift/negative and API docs
PASS — 2 OpenAPI files; 9 canonical tasks; 12 schema negatives rejected;
13 domain negatives schema-valid; 11 portal artifacts

admin-ui lint + production build; NuGet/npm HIGH; Docker Compose
PASS — 0 vulnerability finding

Gitleaks 8.30.0 Windows working tree + implementation-commit Git history
PASS — SHA256 verified; 30 commits / 21.31 MB / no leaks

PII selftest + docs/evidence and ci-artifacts scan
PASS — CT-CI-06..06h; 154 text files; 2 binary files skipped

official Markdown map
PASS — 413 files / 375 resolved links / 0 unresolved
```

## Artifacts

- `src/Ivr.Domain/Policies/EligibilityRules.cs`
- `src/Ivr.Api/Application/EligibilityService.cs`
- `src/Ivr.Infrastructure/Repositories/EligibilityRepository.cs`
- `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs`
- `src/Ivr.Infrastructure/Intake/TaskIntakeStores.cs`
- `tests/Ivr.UnitTests/Policies/EligibilityRulesTests.cs`
- `tests/Ivr.IntegrationTests/EligibilityPersistenceTests.cs`

## Residual gates

- Sales/Order Core Target V1 approval, real service auth and real snapshot values remain `BLOCKED_EXTERNAL`/`NOT_RUN`.
- No direct Ops/CRM integration is needed by IVR Target V1; Sales owns aggregation. P4-2/P4-3 still own contract/provider validation when real systems are connected.
- Default non-MOCK capacity provider deliberately fails closed until P2-3 supplies real scheduler/channel capacity.
- Physical SIM/eSIM, modem, carrier, destination allowlist and customer call are `NOT_RUN`.
- Trust skip cannot be enabled until a versioned resolver/evidence decision is approved.
- Protected GitLab `main` may still reject owner-mandated direct push; this does not change local test verdict.

## Commit/remote handoff

- Implementation commit: `6e0f9d3e0fc294256a298ff5d65e92fcf0dcd21f` trên `main`.
- GitHub `main`: fast-forward thành công và remote ref đã xác minh trùng implementation commit.
- GitLab `main`: vẫn ở `5544395`; pre-receive hook từ chối direct push do protected branch. Không hạ protection và không tạo branch/MR trái chỉ đạo owner.
