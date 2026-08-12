# Architecture SRS — Index

Trạng thái: `TARGET_V1_DRAFT`.

| File | Nội dung |
| --- | --- |
| [01-system-context.md](01-system-context.md) | Sales/IVR/Telephony/Foundation context |
| [02-module-boundaries.md](02-module-boundaries.md) | IVR-owned blocks and ports |
| [03-integration-architecture.md](03-integration-architecture.md) | task/callback/current-compat/auth |
| [04-deployment-architecture.md](04-deployment-architecture.md) | mock → one SIM lab → 32 eSIM target |
| [05-resilience.md](05-resilience.md) | failure/retry/circuit/fail-closed |
| [06-observability.md](06-observability.md) | redacted metrics/trace/alerts |
| [07-diagrams.md](07-diagrams.md) | summary diagrams |

IVR is a standalone .NET service. Sales Java owns order truth, aggregation of eligibility/blockers, revalidation and transitions. Telephony is provider-ported. V1 notification is disabled. Adapter availability never grants real-call permission.
