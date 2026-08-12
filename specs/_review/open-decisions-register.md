# REVIEW — Open Decisions Register

Trạng thái: `OPEN` · Cập nhật: `2026-08-12`. Không đóng bằng suy luận.

## P0 — real Sales integration/business acceptance

| ID | Decision/data | Owner | Current | Closure evidence |
| --- | --- | --- | --- | --- |
| `OD-V1-01` | program/payment/IVR-required/callable matrix | Sales Product/Core | target proposal only | signed matrix + producer tests |
| `OD-V1-02` | generic callback path and ACK taxonomy | Sales API/Core | GH-specific endpoint only | OpenAPI + contract tests |
| `OD-V1-03` | order version exposure/bump/stale behavior | Sales Core | version internal/partial | DTO + stale tests |
| `OD-V1-04` | speech-safe summary schema/content/item limits | Sales/Product/Privacy | not implemented | schema + samples + approval |
| `OD-V1-05` | dial-token issue/resolve/TTL/one-use | Sales/Security/Telephony | not established | API/threat model/tests |
| `OD-V1-06` | no-answer/timeout/revalidation semantics | Sales Product/Core | target proposal | sequence + runtime tests |
| `OD-V1-07` | production auth and mTLS | Security/Platform | dev mock JWT only | signed auth profile + tests |

## P0 — lab/production calls

| ID | Decision/data | Owner | Gate |
| --- | --- | --- | --- |
| `OD-V1-08` | final attempt policy/version | Product/Order Core | production; candidate only MOCK/LAB |
| `OD-V1-09` | 1 SIM lab protocol/DTMF/disposition/allowlist | Infra/vendor | LAB_REAL_SIM |
| `OD-V1-10` | 32 eSIM capacity/failover/caller-ID/cost | Infra/procurement | production |
| `OD-V1-11` | script/legal/do-not-call/retention | Legal/Privacy | customer calls |
| `OD-V1-12` | pilot/release authority/kill switch | Release owner | production |

## Explicit non-decisions

- V1 notification is disabled; no notification template/event is required.
- IVR remains a standalone .NET service; Sales remains Java and owns order truth.
- Current Golden Hour callback remains compatibility-only.
- `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` may be reached while all external rows remain open, but integration/production states must remain blocked.
