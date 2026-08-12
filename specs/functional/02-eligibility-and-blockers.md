# FR — Eligibility and Blockers

Trạng thái: `TARGET_V1_DRAFT`.

Sales owns source aggregation. IVR validates the task snapshot before dispatch; Sales revalidates current truth on callback.

| ID | Requirement |
| --- | --- |
| `FR-IVR-ELIG-001` | Official order/version/callable state and required flag present |
| `FR-IVR-ELIG-002` | exact GH+ONLINE or 24/7+COD matrix |
| `FR-IVR-ELIG-003` | valid window and approved policy version |
| `FR-IVR-ELIG-004` | token/contact refs valid; no raw phone |
| `FR-IVR-ELIG-005` | `call_restriction=true/unknown` blocks; voice semantics separate from SMS |
| `FR-IVR-ELIG-006` | eligibility/sellable/recall/sale-lock/evidence blocked, stale, unknown or missing fails closed |
| `FR-IVR-ELIG-007` | trust skip disabled unless Sales supplies versioned resolver evidence and no risk/blocker |
| `FR-IVR-ELIG-008` | capacity cannot meet deadline → incident/hold, not silent accept |
| `FR-IVR-ELIG-009` | IVR cannot extend window, bypass blocker or directly override Sales decision |

IVR does not directly query Ops/CRM under Target V1. Any future direct integration requires a separate architecture/contract decision. Fake Sales provides deterministic eligibility states for development.
