# TEST-09 — Target V1 Smoke Matrix

Trạng thái: `TARGET_V1_DRAFT`.

| ID | Smoke | PASS | Negative/block |
| --- | --- | --- | --- |
| `SMK-V1-001` | GH ONLINE intake | job created in MOCK | GH+COD rejected |
| `SMK-V1-002` | 24/7 COD intake | job created in MOCK | 24/7+ONLINE rejected |
| `SMK-V1-003` | required flag | true accepted | false/missing rejected |
| `SMK-V1-004` | speech | items/total/short area rendered | full address/missing items blocked |
| `SMK-V1-005` | dial token | valid fake token dials mock | expired/raw phone blocked |
| `SMK-V1-006` | policy | candidate mock policy schedules | unknown/unapproved PROD blocked |
| `SMK-V1-007` | DTMF | 1/0 normalized | invalid/error classified correctly |
| `SMK-V1-008` | no-answer | wait-for-timeout callback | no direct cancel/notification |
| `SMK-V1-009` | target ACK | accepted/duplicate/blocked/review | stale/conflict/422/retry map correctly |
| `SMK-V1-010` | current compat | GH callback isolated | 24/7 routing blocked |
| `SMK-V1-011` | idempotency/race | identical replay stable | changed payload conflicts |
| `SMK-V1-012` | PII | masked logs/UI/evidence | raw phone/address scan fails build |
| `SMK-V1-013` | MOCK mode | zero real egress | real provider activation rejected |
| `SMK-V1-014` | LAB mode | allowlisted test number only | non-allowlisted blocked/kill switch works |
| `SMK-V1-015` | capacity | 1/32-channel simulations produce metrics | overload creates incident |
| `SMK-V1-016` | release truth | mock/lab evidence labelled | no premature production-ready state |

Each smoke records contract/policy/provider/mode versions and evidence link in the canonical tracker.
