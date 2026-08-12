# PROMPT P5-2 — Contract & E2E Test Suite

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-2` · **Phase** 5 — Quality Engineering |
| **Work ID** | `W-0036` (canonical tracker §5) |
| **Prereq** | `P2-1`..`P2-9`, `P3-1`..`P3-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · Pact/Spectral · Playwright |

## 1. ROLE
Bạn là **Senior Test Engineer (contract + E2E)**. Bạn khoá hợp đồng API (consumer-driven contract + OpenAPI) và kiểm luồng đầu-cuối qua UI + backend bằng Playwright. Bạn phát hiện lệch contract sớm và chứng minh luồng nghiệp vụ chạy đúng end-to-end (MOCK).

## 2. CONTEXT
IVR là consumer (task từ Order Core) và producer (callback về Core). Contract lệch = lỗi tích hợp đắt. E2E qua admin UI + API chứng minh happy path + các nhánh chính hoạt động trước khi deploy. Đây là "bằng chứng luồng" cho release gate.

## 3. SOURCE SPECS (đọc trước)
- `specs/testing/04-contract-test-plan.md`, `specs/testing/05-e2e-test-plan.md`
- `specs/api/openapi/ivr-order-confirmation.v1.yaml`, `specs/api/05-order-core-contracts.md`
- `plan/ivr-orther/decisions-log.md` §DS-03/04 (target vs live), §D-04

## 4. DECISIONS & CONSTRAINTS
- **DF-02:** OpenAPI validate + contract test trong CI.
- **Target/current split:** Target callback/order version/semantic ACK tests run against WireMock now and real Sales when available; Golden Hour current 200/422 tests stay in a compatibility suite. Never skip Target tests merely because provider is unavailable—run fake tests and mark real-provider evidence `BLOCKED_EXTERNAL`.
- **Consumer-driven:** pact cho task (Order Core producer) + callback (IVR producer).
- **E2E:** phủ SCN chính (confirm, cancel, no-answer→A2→final, technical, invalid-phone, race-block) qua UI+API MOCK.

## 5. INPUTS / DEPENDENCIES
- OpenAPI + generated client (P1-1); admin UI (P3-*); mock Order Core/ops/CRM; seed scenarios.

## 6. BUILD STEPS
1. **Contract tests** (`Ivr.ContractTests`): schema round-trip (task/callback), required/enum, ErrorEnvelope 15-code; drift-check (P1-1). Pact consumer/provider (task/callback) — target cases đánh `pending`.
2. **OpenAPI lint** trong CI (Spectral) — đã có P0-2, đảm bảo test lặp.
3. **E2E Playwright**: chạy stack MOCK (compose), thao tác admin UI (login, xem dashboard/log/detail) + kích luồng qua API; assert kết quả + evidence hiển thị.
4. **Scenario coverage** (testing/05): confirm/cancel/no-answer-final/technical/invalid-phone/race — mỗi cái 1 E2E.
5. Report + trace; target-pending cases liệt kê rõ (không giả pass).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `tests/Ivr.ContractTests/**` | Schema + pact (task/callback) |
| `tests/e2e/**` (Playwright) | Luồng UI+API |
| `deploy/ci/contract-e2e.gitlab-ci.yml` | Job contract + e2e; **phải được root `.gitlab-ci.yml` `include`**, `allow_failure: false`, và có test chứng minh job xuất hiện trong rendered pipeline |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-OAS-01..03` | contract | OpenAPI parse/ref/enum (D-10). |
| `CT-TASK-01..04` | contract | task schema + policy mismatch. |
| `CT-CB-01` | contract | `200 ACCEPTED` → `DELIVERED_ACCEPTED`, task đóng. |
| `CT-CB-02` | contract | `200 DUPLICATE_ACCEPTED` (resend cùng idempotency key) → `DELIVERED_ACCEPTED`, không tạo bản ghi trùng. |
| `CT-CB-03` | contract | `200 BLOCKED_BY_CORE` → `DELIVERED_BLOCKED`, **không retry**, hiển thị quyết định Core. |
| `CT-CB-04` | contract | `200 REVIEW_REQUIRED` → `DELIVERED_REVIEW`, vào hàng đợi admin. |
| `CT-CB-05` | contract | `409 REJECTED_STALE` → `REJECTED_STALE`, **không transport-retry**, audit/admin review. |
| `CT-CB-06` | contract | `409 IDEMPOTENCY_CONFLICT` → `IDEMPOTENCY_CONFLICT`, không retry. |
| `CT-CB-07` | contract | `422` → `INVALID_DEAD_LETTER`, không retry. |
| `CT-CB-08` | contract | `429/500/503/timeout` → `RETRY_PENDING` bounded, cùng idempotency key; hết bound → `RETRY_EXHAUSTED` + admin review. |
| `CT-CB-09` | contract | GH current-compat suite riêng (200/422), và **24/7 bị từ chối** trên route compat. |
| `E2E-CONFIRM-01` | e2e | confirm luồng → detail hiển thị CONFIRMED signal + callback 200. |
| `E2E-NOANSWER-02` | e2e | A1 no-answer→A2→final; order không bị IVR transition (DS-02). |
| `E2E-RACE-03` | e2e | phím 1 + blocker → blocked, không confirm. |

Trace: `specs/testing/04`, `05`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] target/current suites tách; [ ] both programs + speech/token/no-answer covered; [ ] fake vs real evidence labeled; [ ] no giả pass.
**Reviewer:** pact provider/consumer đúng vai; drift-check chặn; E2E deterministic.

## 10. EVIDENCE EXPECTED
Contract report, pact files, OpenAPI lint, Playwright report + trace video/screenshot, target-pending list.

## 11. FORBIDDEN
- ❌ Assert `CALLBACK_ACCEPTED_FOR_REVALIDATION`/`CALLBACK_*` (taxonomy D-04 cũ, đã bị Target V1 ACK thay thế) hoặc coi `order_version` là đã live ở Sales (DS-03/04 vẫn là current-compat). ❌ E2E gọi thật (MOCK). ❌ Giả pass target cases (phải pending rõ).

## 12. DEFINITION OF DONE
- [ ] Contract + E2E suite + drift-check; test §8 xanh CI; evidence §10 đủ.
