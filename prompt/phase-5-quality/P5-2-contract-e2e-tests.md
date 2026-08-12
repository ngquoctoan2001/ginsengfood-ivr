# PROMPT P5-2 — Contract & E2E Test Suite

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-2` · **Phase** 5 — Quality Engineering |
| **Prereq** | `P2-*`, `P3-*` |
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
- **DS-03/04:** contract test callback assert theo **reality (200/422)**; các case `CALLBACK_*`/order_version = **target**, đánh dấu skip/pending tới khi OC1/OC2.
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
| `deploy/ci/*` (mở rộng) | Job contract + e2e |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-OAS-01..03` | contract | OpenAPI parse/ref/enum (D-10). |
| `CT-TASK-01..04` | contract | task schema + policy mismatch. |
| `CT-CB-01..04` | contract | callback 200/422 (DS-03); target cases pending. |
| `E2E-CONFIRM-01` | e2e | confirm luồng → detail hiển thị CONFIRMED signal + callback 200. |
| `E2E-NOANSWER-02` | e2e | A1 no-answer→A2→final; order không bị IVR transition (DS-02). |
| `E2E-RACE-03` | e2e | phím 1 + blocker → blocked, không confirm. |

Trace: `specs/testing/04`, `05`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] contract khớp reality (200/422); [ ] target cases pending rõ; [ ] E2E phủ SCN chính; [ ] no giả pass.
**Reviewer:** pact provider/consumer đúng vai; drift-check chặn; E2E deterministic.

## 10. EVIDENCE EXPECTED
Contract report, pact files, OpenAPI lint, Playwright report + trace video/screenshot, target-pending list.

## 11. FORBIDDEN
- ❌ Assert `CALLBACK_*`/order_version như đã live (DS-03/04). ❌ E2E gọi thật (MOCK). ❌ Giả pass target cases (phải pending rõ).

## 12. DEFINITION OF DONE
- [ ] Contract + E2E suite + drift-check; test §8 xanh CI; evidence §10 đủ.
