# PROMPT P5-1 — Unit & Integration Test Suite

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-1` · **Phase** 5 — Quality Engineering |
| **Prereq** | `P2-*` (core) |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · xUnit · Testcontainers |

## 1. ROLE
Bạn là **Senior Test Engineer (.NET)**. Bạn củng cố bộ test unit + integration cho toàn IVR core: chạy trên Postgres thật (Testcontainers), phủ happy + negative + fail-closed, đạt ngưỡng coverage, và trace 1-1 tới `specs/testing/*`. Bạn coi test là hợp đồng hành vi, không phải hình thức.

## 2. CONTEXT
Các slice Phase 2 đã có test cục bộ; Phase 5 hợp nhất thành **test suite chính thức** theo test plan, đảm bảo mọi FR/P0 có test backing (MASTER-05, không orphan claim). Đây là nền cho code-review gate (P5-4) và CD (P7-3).

## 3. SOURCE SPECS (đọc trước)
- `specs/testing/00-index.md`, `specs/testing/01-strategy.md`, `specs/testing/02-unit-test-plan.md`, `specs/testing/03-integration-test-plan.md`
- `specs/testing/08-acceptance-criteria.md` (fail gate), `specs/testing/09-smoke-matrix.md`
- `plan/ivr-orther/decisions-log.md` (mọi D/DS/DO/DT áp vào assert)

## 4. DECISIONS & CONSTRAINTS
- **Coverage:** core slice ≥ 80% (nâng từ nền P0-2); không loại trừ bừa.
- **Testcontainers Postgres:** integration chạy DB thật (migration P1-2), không in-memory giả.
- **Trace:** mỗi test map ID trong `testing/02/03`; mọi P0/FR có ≥1 test.
- **Target V1 asserts:** GH+ONLINE and 24/7+COD matrix; technical≠no-answer; policy is versioned/configurable. Candidate 2/5′/15′ is tested only as MOCK/LAB fixture, plus an alternate policy proving no hard-code.
- **Fail gate (testing/08):** test khẳng định IVR không transition order, không xử lý payment, technical≠no-answer, PII masked.

## 5. INPUTS / DEPENDENCIES
- Core P2-* + domain P1-3; seed/* làm fixture; Testcontainers.

## 6. BUILD STEPS
1. **Unit** (`Ivr.UnitTests`): domain policy (AttemptPolicy property, DispositionMapper matrix, EligibilityRules, PII guard), normalizer, callback logic, foundation (idempotency/correlation/error).
2. **Integration** (`Ivr.IntegrationTests`, Testcontainers Postgres): both intake paths→job, scheduler offsets/expire, speech/token guards, blocker snapshots, target callback semantic ACK/retry/DLQ plus GH current-compat isolation, fail-closed profiles.
3. **Fixture/builder**: test data builders từ seed; clock injectable cho thời gian.
4. **Coverage** đạt ngưỡng; report cobertura; map traceability (test↔spec ID) xuất bảng.
5. **Fail-gate assertions** (testing/08): test chuyên khẳng định 8 fail-gate không xảy ra.
6. Tối ưu tốc độ (parallel, container reuse) để CI nhanh.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `tests/Ivr.UnitTests/**` | Unit theo testing/02 |
| `tests/Ivr.IntegrationTests/**` | Integration theo testing/03 (Testcontainers) |
| `tests/_shared/Builders/**`, fixtures | Data builders từ seed |
| `docs/traceability-tests.md` (hoặc generated) | Bảng test↔spec |

## 8. TESTS TO WRITE (đại diện — phủ toàn plan)
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-*` (02) | unit | policy/normalizer/eligibility/foundation. |
| `IT-01..17` (03) | integration | intake/scheduler/callback/blocker/fail-closed profiles. |
| `IT-05a/05b` | integration | non-CONFIRMING/non-COD reject (DS-01). |
| `IT-FAILGATE-*` | integration | 8 fail-gate (testing/08) không xảy ra. |

Trace: toàn bộ `specs/testing/02`, `03`, `08`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] mọi P0/FR có test; [ ] coverage ≥ ngưỡng; [ ] Testcontainers Postgres thật; [ ] fail-gate assertions.
**Reviewer:** không orphan claim (spec không test → fail); không exclude bừa; test deterministic (không flaky do thời gian/thứ tự).

## 10. EVIDENCE EXPECTED
Test report (pass count), coverage report, traceability table (test↔spec), Testcontainers run log.

## 11. FORBIDDEN
- ❌ In-memory DB thay cho integration Postgres. ❌ Test giả pass (skip/hardcode). ❌ Loại trừ coverage tuỳ tiện. ❌ Flaky do thời gian (dùng clock inject).

## 12. DEFINITION OF DONE
- [ ] Suite unit+integration đạt coverage + trace; test §8 xanh CI; evidence §10 đủ.
