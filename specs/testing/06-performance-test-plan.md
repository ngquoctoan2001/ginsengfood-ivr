# TEST-06 — Performance and Capacity Test Plan

Trạng thái: `TARGET_V1_DRAFT`.

| Case | Model | Assert |
| --- | --- | --- |
| PT-01 | 1-channel lab simulation | one active call, deadline/cooldown, no duplicate |
| PT-02 | 32-channel eSIM target simulation | scheduler/outbox/DB throughput and fair deadlines |
| PT-03 | multiple policy versions/windows | capacity uses task config, no hard-coded timings |
| PT-04 | callback latency/throttling/outage | bounded backlog/retry, no idempotency drift |
| PT-05 | burst + insufficient capacity | incident/fail-safe, no silent expiry |
| PT-06 | worker crash/lease recovery | fencing/deadlines preserved |
| PT-07 | soak | memory/connection/outbox growth within accepted thresholds |

Simulation sizes software only. Production 32-eSIM readiness requires measured vendor/gateway capacity and failure evidence.
