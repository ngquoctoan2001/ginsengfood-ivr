# REVIEW — Target V1 Traceability Matrix

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`.

| Requirement | Source/decision | Spec/contract | Prompt slices | Required evidence |
| --- | --- | --- | --- | --- |
| standalone .NET boundary | `TV1-03` | tech/architecture | P0/P1/P4/P7 | solution/build/deploy diagram |
| GH ONLINE + 24/7 COD | `TV1-01` | functional/01, API task OAS | P1-1/P2-1/P5 | schema + unit/contract/E2E both programs |
| speech reads order details safely | `TV1-08` | functional/04, task OAS | P1-3/P2-4/P2-7/P5 | render/audio snapshot + PII tests |
| versioned attempt policy | `TV1-02` | functional/03, database | P1-2/P1-3/P2-3/P5 | config/version/bounds/env-gate tests |
| dial-token/PII boundary | D-05/TV1-08 | data, API, telephony IR | P2-4/P4/P5/P8 | leak tests + resolver contract/lab evidence |
| target callback/ACK | `TV1-04/05` | API-05 + Sales callback OAS | P1-1/P2-6/P4-1/P5 | WireMock/CDC + retry/DLQ evidence |
| no-answer waits for timeout | `TV1-06` | functional/05, workflow/03 | P2-3/P2-6/P5 | sequence/state tests |
| no notification | `TV1-07` | scope/integration | P4-5/P5 | no-op and no-egress tests |
| mock-first build | `TV1-10` | acceptance/runbook | P0-P7 | fake providers + CI evidence |
| one real SIM lab | `TV1-09` | telephony IR/acceptance | P8 | allowlist/kill-switch/DTMF evidence |
| 32 eSIM target | `TV1-09` | deployment/capacity | P8/P10/P11 | procurement + measured load evidence |
| single progress ledger | owner request 2026-08-12 | prompt governance/tracker | every prompt | sequential Work IDs + evidence refs |

Any prompt/PR missing source → contract → test → evidence linkage is not complete.
