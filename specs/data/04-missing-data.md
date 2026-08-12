# DATA-04 — Missing External Data and Contracts

Trạng thái: `OPEN` · Cập nhật: `2026-08-12`.

| ID | Thiếu | Owner | Mock fallback | Chặn thật |
| --- | --- | --- | --- | --- |
| `DG-V1-01` | callable state + producer matrix cho GH ONLINE/24-7 COD | Sales/Product | fake producer | integration |
| `DG-V1-02` | `order_version` exposure/bump semantics | Sales Core | fixture versions | race safety |
| `DG-V1-03` | speech-safe short name/items/total/short area schema/examples | Sales/Product/Privacy | fake summaries | business acceptance |
| `DG-V1-04` | dial-token issue/resolve/TTL | Sales/Security/Telephony | fake resolver | real call |
| `DG-V1-05` | callback path/ACK/idempotency/revalidation | Sales API/Core | WireMock target | integration |
| `DG-V1-06` | owner-approved attempt policy version | Product | candidate mock-lab-v1 | production |
| `DG-V1-07` | auth metadata/credential/mTLS decision | Security/Platform | mock JWT | integration |
| `DG-V1-08` | vendor DTMF/disposition/protocol | Telephony | simulator | lab |
| `DG-V1-09` | 32 eSIM capacity/cost/caller-ID/failover | Infra/procurement | load model | production |
| `DG-V1-10` | retention/script/legal approvals | Legal/Privacy | configurable defaults, recording off | customer calls |

V1 notification template is not missing data because notification is deliberately disabled.
