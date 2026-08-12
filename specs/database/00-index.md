# Database SRS — Index

Trạng thái: `TARGET_V1_DRAFT`.

| File | Nội dung |
| --- | --- |
| [01-erd.md](01-erd.md) | entity relationships |
| [02-tables.md](02-tables.md) | fields/constraints including policy and speech snapshots |
| [03-enums-and-status.md](03-enums-and-status.md) | program/mode/job/attempt/result/callback/SIM |
| [04-indexes.md](04-indexes.md) | idempotency, deadlines, leases, callback outbox |
| [05-retention-and-privacy.md](05-retention-and-privacy.md) | PII/retention/recording-off |
| [06-migration-plan.md](06-migration-plan.md) | forward/rollback/data gates |

Core invariants: IVR does not own order state; target tasks require order version; no raw phone/full address/recording; speech summary is privacy-safe snapshot; policy values are versioned and not hard-coded; program/payment matrix is GH+ONLINE or 24/7+COD; execution mode and provider are explicit.
