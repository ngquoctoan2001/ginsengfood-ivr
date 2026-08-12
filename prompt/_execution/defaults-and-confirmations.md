# Defaults and Confirmations — IVR Execution

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`.

`DEFAULT_FOR_DEV` chỉ cho phép tiếp tục code/mock. `OWNER_DECISION_REQUIRED` không được biến thành production default.

Xác nhận bởi IVR dev ngày `2026-08-12`: repository/namespace/source root, GitLab CI, evidence root, PostgreSQL outbox, IVR-owned schema, committed OpenAPI-generated code và local MOCK như bảng dưới. `C:\Users\Administrator\Desktop\ivr` được hiểu là root của repository độc lập; không tạo nested Git repository. Nếu source root này cần đổi, phải ghi `SCOPE_CHANGE` vào tracker trước P0-1.

## Implementation defaults

| Item | Dev default | Gate | Status |
| --- | --- | --- | --- |
| service/repo | standalone `ginsengfood-ivr`, namespace `Ivr`, source tại repository hiện tại `C:\Users\Administrator\Desktop\ivr` | P0 | `CONFIRMED_2026-08-12` |
| backend/data/UI | .NET 10, PostgreSQL/EF Core, Next.js strict | implementation | accepted for plan |
| database ownership/outbox | IVR-owned PostgreSQL schema; Postgres-backed outbox/worker loop | P1 | `CONFIRMED_2026-08-12` |
| codegen | generate/commit DTO/client + CI drift check | P1 | `CONFIRMED_2026-08-12` |
| local runtime | Docker Compose API/Worker/UI/Postgres/fake Sales/mock SIM/mock JWT | P0/P7 | `CONFIRMED_2026-08-12` |
| auth dev | mock JWT issuer/test keys | dev only | `CONFIRMED_2026-08-12` |
| auth prod | short-lived service JWT | before P4 real | owner decision |
| mTLS | supported optional transport | before P4 real | owner decision |
| CI | GitLab CI; root entrypoint `.gitlab-ci.yml`; Merge Request pipelines | P0-2 | `CONFIRMED_2026-08-12`, supersedes prior GitHub choice |
| evidence root | `docs/evidence/<W-XXXX>/` (mỗi Work ID một thư mục) | all work | `CONFIRMED_2026-08-12` |
| package registry | public NuGet/npm for bootstrap; private registry only when explicitly required | before private package use | default for dev; private registry open |
| secrets/APM | platform standard; no prod secret in repo | before related phase | open — gom vào `W-0063` `G-PLATFORM` |
| execution mode key | canonical `IVR_EXECUTION_MODE` (env) ↔ `executionMode` (config); `IVR_ADAPTER_MODE`/`EXECUTION_MODE` là alias lịch sử | P0-1/P0-4 | `CONFIRMED_2026-08-12` (governance §6) |

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

- Before P0: repository/name/source root/CI/evidence root are confirmed; P0-1 must still inspect the current working tree and avoid conflicting source.
- **Before P0 (mới, W-0062): `BASELINE_FREEZE_REQUIRED`.** Toàn bộ tài liệu điều khiển (governance, tracker, defaults, specs, OpenAPI, seed, prompt) hiện **uncommitted** trên `main` tại `HEAD=b3a93aa`. Owner phải review và commit/freeze baseline này trước khi W-0010 bắt đầu; nếu không, `git checkout .` sẽ xoá cả quyết định A-0004/A-0005 lẫn toàn bộ remediation. Remediation **không tự commit**.
- **Before P0-2: `G-GITLAB` (W-0061)** phải có GitLab project + runner, nếu không P0-2 chỉ đạt local/config evidence và hosted evidence giữ `NOT_RUN`/`BLOCKED_EXTERNAL`.
- Before P1: codegen/outbox/schema ownership are confirmed.
- Before P4 real: Sales OpenAPI/base URL/sandbox, auth/JWT/mTLS and provider credentials.
- Before LAB_REAL_SIM: vendor protocol, 1 SIM, allowlist, kill switch and test plan.
- Before PRODUCTION_REAL: attempt policy, 32 eSIM capacity, script/privacy/legal/retention, staging/pilot evidence and release sign-off.

All open choices must also exist in the canonical tracker with owner/evidence.
