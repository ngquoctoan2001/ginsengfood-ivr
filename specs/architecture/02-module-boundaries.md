# ARCH-02 — IVR Module Boundaries

Trạng thái: `TARGET_V1_DRAFT`.

| Block | Owns | Must not do |
| --- | --- | --- |
| Task Intake | auth/idempotency/schema/program/privacy validation | infer Sales transitions |
| Eligibility Snapshot Evaluator | validate Sales-provided restriction/evidence freshness | query/override Sales truth silently |
| Policy Registry/Scheduler | versioned policy, queue/deadline, attempts | hard-code candidate as production |
| Channel Manager | leases/fencing/health/quarantine/config count | infer call permission from health |
| Dial Token Resolver port | resolve opaque token at trust boundary | persist/log raw phone |
| Speech Renderer | approved Vietnamese order summary | render full address/free sensitive text |
| SIM Gateway port | dial/play/DTMF/disposition/health | order write/notification/recording |
| Result Normalizer | canonical result/count/final | pass raw provider payload to Sales |
| Callback/Outbox | Target ACK/retry + current GH compat | treat ACK as order state |
| Admin/UI | masked monitoring/config/review/audit | force result/order/bypass policy |

Providers/modes: fake Sales and mock SIM in MOCK; vendor adapter in LAB/PROD; Target/current Sales clients separated by anti-corruption layer.
