# IVR Order Confirmation — Plan Index (canonical)

Trạng thái: `LIVING` · Module: **IVR Order Confirmation** (PACK-09 / TECH-09 / phase-8 / "Module 8").
Cập nhật 2026-07-03: đã **clean** — scaffolding Giai đoạn 1 chuyển vào `_archive/`; giữ lại tài liệu canonical (living) dưới đây. Giai đoạn hiện tại đã chuyển từ *plan/spec* sang **bộ prompt triển khai A–Z (zero→production)**.

## 1. Canonical (đang dùng)
| File | Vai trò |
| --- | --- |
| [decisions-log.md](decisions-log.md) | **Bản ghi quyết định đã KHÓA** (D-01..14, DS-01..05, DO-*, DF-*, DT-*, DC-*, DTS-*). Nguồn chân lý. |
| [14-risk-register.md](14-risk-register.md) | Risk register (living). |
| [questions-to-module-3-and-3.1.md](questions-to-module-3-and-3.1.md) | Handoff Q&A — Sales/Commerce 3 & 3.1 (đã trả lời → D-01..14). |
| [questions-to-ops-core.md](questions-to-ops-core.md) | Handoff Q&A — Ops-Core 1/2 (đã trả lời → DO-*). |
| [questions-to-crm-3.1-followup.md](questions-to-crm-3.1-followup.md) | Handoff Q&A — CRM/Customer Identity (đã trả lời → DC-*). |
| [questions-to-telephony-and-foundation.md](questions-to-telephony-and-foundation.md) | Handoff Q&A — Telephony/SIM & Foundation (đã trả lời → DT-*/DF-*). |
| [questions-to-order-core-state.md](questions-to-order-core-state.md) | Handoff Q&A — Order Core state (DG-03 → DS-01..05, đã trả lời). |

## 2. Deliverable chính (ngoài plan/)
| Nơi | Nội dung |
| --- | --- |
| `specs/` (78 file) | SRS/SDS đã distill+normalize (functional, workflows, api+OpenAPI, data, database, architecture, testing, ui, _review). |
| `integration-requirements/` (6 file) | Yêu cầu tích hợp gửi Sales/Ops/Telephony/Foundation + open contract questions. |
| `seed/` (10 file) | Mock data (orders CONFIRMING+COD, tasks, inventory sellable, scenarios…). |
| **`prompt/`** | ⭐ **Bộ prompt triển khai A–Z (zero→production)** — xem [`prompt/00-index.md`](../../prompt/00-index.md). |

## 3. Tech stack (DTS — 2026-07-03)
Backend **.NET 10 (C#)** · DB **PostgreSQL** · Admin UI **Next.js** · Deploy **Docker + Kubernetes**. Chi tiết: `decisions-log.md` §DTS + `specs/tech/00-tech-stack.md`.
Lưu ý: Order Core / CRM / Ops thuộc `ginsengfood-business-platform` (**Java/Spring**) — IVR là **service .NET tách biệt**, nói chuyện qua contract (OpenAPI/webhook), không chia sẻ codebase.

## 4. Trạng thái open-decisions
Bản chuẩn: [`specs/_review/open-decisions-register.md`](../../specs/_review/open-decisions-register.md).
P0 chặn gọi khách thật còn lại: **mua SIM (DT-01)**, **release sign-off (DF-03)**, **Legal retention (DF-07)**.
Build items (không chặn dry-run): IR-SALES-OC1/OC2/OC3, DC-05/DC-06, IR-CRM-01.

## 5. Lịch sử (đã archive)
`_archive/`: reading-inventory, current-understanding, findings, module-dependency-map, các *-analysis-plan, source-of-truth-build-plan, target-specs-structure-proposal, specs-generation-sequence, integration-gap-analysis, api-needs-drafts, seed-strategy, 15-open-questions (→ open-decisions-register), 16-prompt-roadmap (spec-gen cũ, → prompt/00-index mới), và `prompts/` (p01–p14 spec-generation, đã chạy xong).

## 6. Nhãn
`CONFIRMED` · `ASSUMPTION` · `NEED_CONFIRMATION` · `TODO` · `GAP` · `RISK` · `LOCKED`.
