# PROMPT P0-1 — Repo & Solution Bootstrap

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-1` |
| **Phase** | 0 — Foundation & Project Setup |
| **Prereq (blockedBy)** | — (prompt đầu tiên) |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 (C#) · PostgreSQL · Next.js · Docker |

## 1. ROLE
Bạn là **Senior .NET Platform Engineer** khởi tạo một service mới trong hệ sinh thái GinsengFood. Bạn dựng khung solution sạch, tách lớp rõ (Api/Worker/Domain/Infrastructure/Contracts), chuẩn hoá tooling, để mọi prompt sau build lên trên. Bạn ưu tiên convention-over-configuration, reproducible build, và ranh giới module chặt.

## 2. CONTEXT
IVR Order Confirmation là **service .NET độc lập** (KHÔNG thuộc `ginsengfood-business-platform` Java/Spring), giao tiếp với Order Core/CRM/Ops qua OpenAPI + webhook (DTS-01). Đây là bước 0: repo trống → solution biên dịch được với đầy đủ project rỗng đúng layout, DB Postgres chạy local, và Next.js admin app khởi tạo. Chưa có business logic — chỉ khung + tooling + "hello health".

## 3. SOURCE SPECS (đọc trước)
- `specs/tech/00-tech-stack.md` (layout, thành phần, ràng buộc)
- `prompt/README-governance.md` §3 (layout repo), §4 (coding standards)
- `specs/architecture/02-module-boundaries.md`, `specs/architecture/04-deployment-architecture.md`
- `plan/ivr-orther/decisions-log.md` §Tech Stack (DTS-01..05)

## 4. DECISIONS & CONSTRAINTS
- **DTS-01/02/03:** backend .NET 10, DB PostgreSQL, UI Next.js — cố định.
- **DTS-04:** 3 deployable: `ivr-api`, `ivr-worker`, `ivr-admin-ui` (chuẩn bị cho container hoá P7).
- **README-governance §2:** service KHÔNG share DB/entity với platform Java.
- **Coding standards §4:** nullable enabled, warnings-as-errors, analyzers, async-first.
- ORM = **EF Core** (default DTS parametrized) — chỉ cài package + DbContext rỗng ở bước này, migration ở P1-2.

## 5. INPUTS / DEPENDENCIES
- .NET 10 SDK, Node ≥ 20 (Next.js), Docker (Postgres local).
- Env mẫu: `IVR_ADAPTER_MODE=MOCK`, `ConnectionStrings__IvrDb`, `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- `NEED_CONFIRMATION`: tên repo/namespace gốc (default `Ivr`), package registry — không chặn.

## 6. BUILD STEPS
1. Tạo solution `Ivr.sln` + các project:
   - `src/Ivr.Api` (ASP.NET Core, target `net10.0`) — chỉ `GET /health/live|ready|startup` trả 200/JSON (readiness sẽ nối DB ở P0-3/P1-2).
   - `src/Ivr.Worker` (Worker Service, `BackgroundService` rỗng "IvrHeartbeat" log mỗi 30s).
   - `src/Ivr.Domain` (class library, không ref Infrastructure).
   - `src/Ivr.Infrastructure` (class library; ref Domain; thêm `IvrDbContext : DbContext` rỗng, EF Core + Npgsql).
   - `src/Ivr.Contracts` (class library rỗng — sẽ chứa DTO sinh từ OpenAPI ở P1-1).
   - `tests/Ivr.UnitTests`, `tests/Ivr.IntegrationTests`, `tests/Ivr.ContractTests` (xUnit).
2. Thiết lập **Directory.Build.props** ở gốc: `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<LangVersion>latest</LangVersion>`, `<ImplicitUsings>enable</ImplicitUsings>`.
3. Thêm `.editorconfig` (C# style rules) + analyzers (`Microsoft.CodeAnalysis.NetAnalyzers`, tuỳ chọn StyleCop). Cấu hình severity = warning→error cho rule quan trọng.
4. `admin-ui/`: khởi tạo **Next.js** (TypeScript, App Router, ESLint, strict). Trang `/` placeholder "IVR Admin — MOCK mode". Chưa auth (P3-1).
5. `docker-compose.dev.yml` ở gốc: service `postgres:16` (db `ivr`, user/pass dev), volume, healthcheck. Api đọc `ConnectionStrings__IvrDb`.
6. `README.md` gốc repo: cách chạy (`docker compose up postgres`, `dotnet run --project src/Ivr.Api`, `npm --prefix admin-ui run dev`), sơ đồ thành phần, nhắc governance (`REAL_CUSTOMER_CALL_ALLOWED=NO`).
7. Config qua `appsettings.json` + `appsettings.Development.json` + env override; bind `IvrOptions` (adapter mode, connection). KHÔNG hardcode secret.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `Ivr.sln`, `Directory.Build.props`, `.editorconfig` | Solution + build props chung |
| `src/Ivr.Api/**` | ASP.NET Core + health endpoints + Program.cs |
| `src/Ivr.Worker/**` | Worker + heartbeat |
| `src/Ivr.Domain/**`, `src/Ivr.Infrastructure/**`, `src/Ivr.Contracts/**` | Libs (rỗng đúng ref direction) |
| `tests/**` | 3 test project + 1 smoke test "solution builds & health returns 200" |
| `admin-ui/**` | Next.js app khởi tạo |
| `docker-compose.dev.yml`, `README.md` | Dev infra + hướng dẫn |

**Chuẩn output:** ref direction 1 chiều (Api/Worker→Infrastructure→Domain; Domain không ref ai). Không project nào ref ngược Domain→Infrastructure.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-BOOT-01` | unit | Solution build pass; `IvrOptions` bind đúng từ config. |
| `IT-BOOT-02` | integration | `GET /health/live` → 200; `/health/ready` → 200 (chưa nối DB) hoặc 503 nếu DB down (sau khi nối P1-2). |
| `UT-BOOT-03` | unit | Ref direction: test kiến trúc (VD NetArchTest) — `Ivr.Domain` không depend `Ivr.Infrastructure`. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] `dotnet build` + `dotnet test` xanh; `npm run build` (admin-ui) xanh.
- [ ] Warnings-as-errors bật; analyzers chạy.
- [ ] Không secret trong source; env-based config.

**Reviewer:** kiểm layout khớp governance §3; ref direction đúng; health endpoint tách live/ready/startup (chuẩn K8s probe sau này).

## 10. EVIDENCE EXPECTED
Build log (0 warning), test report (3 pass), `docker compose up postgres` + `dotnet run` log health 200, screenshot Next.js placeholder.

## 11. FORBIDDEN
- ❌ Business logic / entity `ivr_*` (thuộc P1).
- ❌ Nối tới Order Core/CRM/Ops thật.
- ❌ Secret hardcode; ❌ chung DB với platform Java.

## 12. DEFINITION OF DONE
- [ ] Solution + admin-ui build & test xanh trong CI (CI dựng ở P0-2 — tạm chạy local).
- [ ] Health endpoints hoạt động; compose Postgres chạy.
- [ ] Layout + ref direction đúng governance §3; evidence §10 đủ.
