# PROMPT P11-2 — Cross-Team Contract Closure Pack

## 0. Meta
| | |
| --- | --- |
| **ID** | `P11-2` · **Phase** 11 — External Production Closure |
| **Prereq** | Có thể chạy song song từ `P1-1`; feed vào `P4-*`, `P5-2`, `P9-1` |
| **Governance** | Target contracts default OFF bằng feature flag tới khi provider accepted |
| **Stack** | Contract package + tickets + pact/OpenAPI acceptance |

## 1. ROLE
Bạn là **Cross-Team Contract Delivery Lead**. Bạn đóng gói các gap Order Core/CRM/Ops thành ticket đủ rõ để team khác build, kèm OpenAPI/pact/test acceptance, và cập nhật IVR flags khi contract thật được accepted. Bạn không được biến target thành live khi provider chưa giao.

## 2. CONTEXT
Specs đã tách current/target: current đủ để COD production fail-safe, nhưng production đầy đủ cần OC1/OC2/OC3, DC-05/06, IR-CRM-01, DO-02. Phase 4 đã chuẩn bị IVR-side bằng feature flags; prompt này bảo đảm phần bên ngoài có owner, ticket, acceptance, pact và rollout plan.

## 3. SOURCE SPECS (đọc trước)
- `integration-requirements/01-sales-platform-requirements.md`
- `integration-requirements/02-ops-core-requirements.md`
- `integration-requirements/05-open-contract-questions.md`
- `specs/api/05-order-core-contracts.md`, `specs/api/08-external-api-needs.md`
- `specs/_review/open-decisions-register.md`
- `plan/ivr-orther/production-blockers-plan.md` §B
- `prompt/phase-4-integration/P4-1-order-core-wiring.md`, `P4-2`, `P4-3`, `P4-5`, `P4-6`

## 4. DECISIONS & CONSTRAINTS
- **Current must remain valid:** task `CONFIRMING+COD`, callback `200/422`, no `order_version` required.
- **OC1 target:** provider exposes `order_version` and accepts `order_version_seen_by_ivr`.
- **OC2 target:** provider returns semantic callback codes.
- **OC3 optional:** explicit no-answer/technical transition; until then order expires by timeout.
- **DC-05:** Core/CRM event after decision; IVR does not send customer notification.
- **DC-06:** trust resolver; until live, default require-IVR.
- **IR-CRM-01:** rich do-not-call fields; until live, `eligible=false` is enough for basic block.
- **DO-02:** sellable snapshot freshness fields/ETag improve safety; fail-closed if absent/unknown.

## 5. INPUTS / DEPENDENCIES
- Provider repo/API owners for Order Core, CRM, Ops-Core.
- Existing OpenAPI/pact tooling from `P1-1`/`P5-2`.
- Feature flags from `P0-4`: `orderVersionRaceGuard`, `richCallbackCodes`, `postDecisionNotify`, `trustResolver`, `richDoNotCall`, `sellableFreshness`.

## 6. BUILD STEPS
1. Tạo contract pack cho Order Core OC1/OC2/OC3: request/response examples, OpenAPI delta, negative cases, provider acceptance tests, rollout flag mapping.
2. Tạo contract pack cho CRM/DC: do-not-call rich response, post-decision event, trust resolver, opt-out feedback loop; ghi rõ IVR không tự notify.
3. Tạo contract pack cho Ops DO-02: `captured_at`, ETag/freshness, error semantics, fail-closed behavior.
4. Tạo tickets theo từng item với owner, priority, acceptance, sample payload, pact link, IVR flag cần bật.
5. Tạo pact/provider tests hoặc JSON fixtures để provider chạy; current cases must pass now, target cases pending until provider delivers.
6. Khi provider giao: chạy `P5-2` contract tests, cập nhật feature flag rollout plan, cập nhật `open-decisions-register` status từ `⏳` sang accepted only with evidence.
7. Nếu provider chưa giao trước release: ghi explicit production limitation + flag OFF trong P9 dossier.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/cross-team/ivr-order-core-contract-pack.md` | OC1/OC2/OC3 payloads + acceptance |
| `docs/cross-team/ivr-crm-contract-pack.md` | DC-05/DC-06/IR-CRM-01 contract pack |
| `docs/cross-team/ivr-ops-freshness-contract-pack.md` | DO-02 freshness/ETag contract pack |
| `docs/cross-team/ivr-cross-team-ticket-board.md` | Ticket list, owner, status, flag, evidence |
| `tests/contracts/provider-fixtures/**` | Pact/JSON fixtures for provider acceptance |
| `specs/decisions/IR-*-accepted.md` | Decision record only after provider tests pass |

## 8. TESTS / VERIFICATION TO RUN
| Test ID | Loại | Assert |
| --- | --- | --- |
| `XT-OC1-01` | contract | OC1 provider accepts version guard payload; mismatch rejects stale when flag on. |
| `XT-OC2-02` | contract | OC2 provider returns semantic callback codes; current 200/422 still supported. |
| `XT-CRM-03` | contract | CRM rich DNC response maps `eligible/do_not_call/opt_out_scope/reason/effective_at`. |
| `XT-EVENT-04` | contract | Core/CRM post-decision event idempotent and deduped. |
| `XT-OPS-05` | contract | Ops sellable freshness fields present; stale/unknown fail-closed. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] every gap has ticket+owner+acceptance; [ ] provider fixtures runnable; [ ] target flags default OFF; [ ] current production path remains valid.

**Reviewer:** provider owners confirm ticket scope; IVR owner confirms release limitation if not delivered; contract tests prove accepted before flags on.

## 10. EVIDENCE EXPECTED
Ticket links, provider OpenAPI diff, pact reports, sample payloads, flag rollout note, accepted decision records or explicit deferred limitation.

## 11. FORBIDDEN
- ❌ Flip target flag from ticket text alone. ❌ Break current 200/422 path. ❌ Require OC1/OC2 for COD go-live unless owner changes gate. ❌ Let target tests fake-pass without provider evidence.

## 12. DEFINITION OF DONE
- [ ] Cross-team packs + tickets + fixtures exist; each target item is either accepted with passing provider evidence or explicitly deferred with flag OFF and release limitation documented.
