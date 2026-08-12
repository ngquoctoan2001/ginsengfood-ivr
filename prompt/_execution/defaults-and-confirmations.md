# Defaults and Confirmations — IVR Execution

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`.

`DEFAULT_FOR_DEV` chỉ cho phép tiếp tục code/mock. `OWNER_DECISION_REQUIRED` không được biến thành production default.

## Implementation defaults

| Item | Dev default | Gate | Status |
| --- | --- | --- | --- |
| service/repo | standalone `ginsengfood-ivr`, namespace `Ivr` | confirm before P0 | proposed |
| backend/data/UI | .NET 10, PostgreSQL/EF Core, Next.js strict | implementation | accepted for plan |
| outbox | Postgres-backed outbox/worker loop | confirm before P1 | proposed |
| codegen | generate/commit DTO/client + CI drift check | confirm before P1 | proposed |
| local runtime | Docker Compose API/Worker/UI/Postgres/fake Sales/mock SIM/mock JWT | P0/P7 | default for dev |
| auth dev | mock JWT issuer/test keys | dev only | default for dev |
| auth prod | short-lived service JWT | before P4 real | owner decision |
| mTLS | supported optional transport | before P4 real | owner decision |
| CI/registry/secrets/APM | platform standard; no prod secret in repo | before related phase | open |

## Product/contract truth

| Item | Current plan | Status/Gate |
| --- | --- | --- |
| program scope | GH+ONLINE and 24/7+COD, both `ivr_confirmation_required=true` | Target V1 draft; Sales/Product sign-off needed |
| attempt policy | candidate max 2; GH 300/[0,150], 24/7 900/[0,450] | MOCK/LAB only; owner decision before PROD |
| order transition | IVR never transitions | locked invariant |
| callback target | `/api/v1/internal/orders/{orderId}/ivr-result-callbacks` + semantic ACK | draft; Sales must implement/sign |
| callback current | `/api/v1/internal/ivr/golden-hour/callbacks` | compatibility-only |
| speech payload | short name/code, public items+qty, total, short area, program | P0 upstream dependency |
| dial token | opaque, TTL in window, resolver at trust boundary | P0 upstream dependency |
| no-answer | no immediate cancel; wait for Core timeout/revalidation | target draft |
| notification | disabled/no-op; IVR sends no SMS | V1 invariant |
| recording | OFF | default/owner+legal gate to change |

## Modes and SIM

| Mode/item | Default | Gate |
| --- | --- | --- |
| dev/test | `MOCK`; no real network call | always |
| lab | `LAB_REAL_SIM`; **1 real SIM**, destination allowlist, kill switch | protocol/test SIM/approved test numbers |
| production | `PRODUCTION_REAL`; target **32 eSIM channels** | Sales/auth/policy/legal/security/capacity/release accepted |
| channel count | config; never hard-code | all |
| DTMF/disposition | simulator mapping until real lab verified | P8 |

## Must-decide gates

- Before P0: repo/name/CI/evidence root and confirm no conflicting existing source.
- Before P1: codegen/outbox/schema ownership.
- Before P4 real: Sales OpenAPI/base URL/sandbox, auth/JWT/mTLS and provider credentials.
- Before LAB_REAL_SIM: vendor protocol, 1 SIM, allowlist, kill switch and test plan.
- Before PRODUCTION_REAL: attempt policy, 32 eSIM capacity, script/privacy/legal/retention, staging/pilot evidence and release sign-off.

All open choices must also exist in the canonical tracker with owner/evidence.
