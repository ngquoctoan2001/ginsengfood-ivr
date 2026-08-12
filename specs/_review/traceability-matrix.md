# REVIEW — Traceability Matrix

Trạng thái: `REVIEW` · Sinh bởi: `p14` · Nguồn: `phase-8/25` (ma trận truy vết), toàn bộ `specs/srs/*`.
Cột: **Requirement/Domain → Source doc → Spec file → Test → Evidence/Smoke → Decision**.

| Domain | Source (docs) | Spec file | Test | Smoke/Evidence | Decision |
| --- | --- | --- | --- | --- | --- |
| Scope/governance | phase-8/00,01; docx §0-2 | `01-context-and-scope`, `02-business-goals` | — | release gate | D-08 |
| Task intake | phase-8/04; docx §5,§6 | `functional/01`, `api/02/05`, `database/02` | UT-INTAKE, CT-TASK, IT-01..05 | M8-P0-001 | D-01/02/03 |
| Eligibility/blocker | phase-8/03; docx §7 | `functional/02`, `data/03` | UT-ELIG, IT-03/04 | M8-P0-008, SMK-016 | D-06/12, DO-01/CORR-2 |
| Attempt/scheduler | phase-8/05; docx §8,§12 | `functional/03`, `database/04` | UT-SCH, E2E-03/08, PT-* | M8-P0-002/003/006/009 | **D-10** |
| Call/DTMF | phase-8/06; docx §9,§10 | `functional/04`, `api/04` | UT-NORM, E2E-13/15 | M8-P0-004/005 | DT-01/02/05 |
| Result/callback | phase-8/07; docx §13,§14 | `functional/05`, `api/05` | UT-CB, IT-06..09, CT-CB | M8-P0-007/010, SMK-010 | D-02/04, DT-02 |
| Technical/capacity | docx §11,§15 | `functional/06`, `architecture/04/05` | UT-NORM-04, PT-*, IT-17 | M8-P0-007/009 | DT-02/04 |
| Admin/monitoring | phase-8/08; docx §16 | `functional/07`, `ui/*`, `api/03` | SEC-04..07 | M8-P0-011 | DF-01 |
| Security/privacy | phase-8/08,15; docx §17 | `data/05`, `testing/07` | SEC-08..15 | M8-P0 privacy | D-05, DT-05, DF-07 |
| Evidence/release gate | phase-8/09,19; MASTER-05; TECH-10 | `testing/08` | acceptance | M8-DONE/FAIL | DF-03 |
| Order Core contract | phase-8/04,07 | `api/05`, `integration-requirements/01` | CT-*, IT-06..09 | — | D-01..06 |
| Ops blocker | phase-2/06; TECH-03 | `data/03`, `integration-requirements/02` | CT-OPS, IT-10/11 | SMK-009 | DO-01..09 |
| Foundation | TECH-01; MASTER-03 | `api/01/07`, `integration-requirements/04` | SEC-01..03, CT-OAS | — | DF-01..07 |
| CRM do-not-call | (ops-core corr) | `data/02`, `api/05` | SCN-012/IT-15 | SMK-016 | ✅ DC-01; IR-CRM-01 P1 |

## Coverage
- **P0 rules (functional/00-index P0-IVR-001..010)** đều có: spec + test + smoke. 
- **Decisions D-01..14, DO-01..09, DF-01..07, DT-01..06** đều được phản ánh trong ≥1 spec file (cross-check `decisions-log.md`).
- **Requirement không có test:** không (mọi domain có ≥1 test); ✅ DG-03 đã trả DS-01..05, integration target còn IR-SALES-OC1/OC2/OC3.

## Dev prompt slice → spec/test (prompt/*, thêm round 2)
| Slice | Prompt | Spec chính | Test | Smoke |
| --- | --- | --- | --- | --- |
| Foundation | `prompt/01` | api/01,07, database/*, openapi | CT-OAS, SEC-01..03, UT idempotency | — |
| M8.2A Task intake | `prompt/02` | functional/01, api/02,05,06 | UT-INTAKE, IT-01..05, CT-TASK | M8-P0-001 |
| M8.2B Eligibility | `prompt/03` | functional/02, data/03 | UT-ELIG, IT-03/04, IT-12..17 | SMK-015/016, M8-P0-008 |
| M8.2C Scheduler | `prompt/04` | functional/03, database/04 | UT-SCH, PT-01..06 | M8-P0-002/003/009 |
| M8.2D SIM adapter | `prompt/05` | api/04, functional/04 | SEC-03/08/09/10 | disposition |
| M8.2E DTMF/Normalizer | `prompt/06` | functional/06,05 | UT-NORM-01..06, E2E-04/05 | M8-P0-007 |
| M8.2F Callback | `prompt/07` | api/05, functional/05, workflows/06 | IT-06..09, CT-CB, E2E-06 | M8-P0-010, SMK-010 |
| M8.2G Admin/UI | `prompt/08` | api/03, functional/07, ui/* | SEC-04..07 | M8-P0-011 |
| M8.2H Smoke/Evidence | `prompt/09` | testing/08,09, _review/* | toàn bộ testing | M8-DONE/FAIL |

## Gaps traceability
- DG-03 → ✅ resolved DS-01..05; OC1/OC2/OC3 là build/target, không còn là open decision.
- Q-C1 → ✅ resolved DC-01; test vẫn dùng seed/mock `call_restriction` cho dry-run, IR-CRM-01 là build P1.
- Task/callback race-guard → ✅ current/target đã phủ trong OpenAPI/spec/database/test/workflow: current task `CONFIRMING+COD` không require `order_version`, current callback `200`/`422`; target OC1/OC2 cho `order_version_seen_by_ivr` + semantic `CALLBACK_*`.
- (Round 2) Không phát sinh gap traceability mới; mọi slice có spec+test.
