# 08 — Target Specs Structure Proposal

**Chỉ đề xuất — CHƯA tạo thật ở giai đoạn này.** Các thư mục `specs/`, `integration-requirements/`, `seed/`, `prompt/` sẽ được tạo khi chạy prompt con tương ứng.

## 1. Cấu trúc `specs/` đề xuất

```txt
specs/
  srs/
    00-index.md
    01-context-and-scope.md
    02-business-goals.md
    03-stakeholders-and-actors.md
    04-glossary.md
    05-current-docs-review.md
    06-assumptions-and-open-questions.md

    functional/          # yêu cầu chức năng
    non-functional/      # NFR (reliability, capacity, security-nfr, observability)
    workflows/           # luồng + sequence + state machine
    api/                 # API specs + contracts + error codes + openapi
    database/            # ERD, tables, enums, indexes, retention, migration
    data/                # data ownership, mapping sales/ops, missing data, PII
    architecture/        # context, boundaries, integration, deployment, resilience
    modules/             # phân rã service block nội bộ IVR
    ui/                  # admin/ops console specs
    testing/             # strategy + các test plan + acceptance + smoke matrix
    diagrams/            # nguồn diagram dùng chung (mermaid)
    _review/             # (do p14) normalization-report, traceability-matrix, open-decisions-register

  decisions/             # ADR: mỗi quyết định/override baseline
```

## 2. Mục đích + nguồn + prompt + điều kiện hoàn thành từng nhóm

| Nhóm | Mục đích | Tài liệu nguồn chính | Prompt sinh | Điều kiện hoàn thành |
| --- | --- | --- | --- | --- |
| `srs/00-index` | Cổng vào specs | phase-8/26, plan/00 | p02/p14 | Index trỏ đủ mọi nhóm |
| `srs/01-context-and-scope` | Bối cảnh + IN/OUT scope | phase-8/00,/01 | p02 | Scope IN/OUT khớp phase-8/00 §4,§11 |
| `srs/02-business-goals` | Mục tiêu kinh doanh (anti-fake, giảm đơn ảo) | phase-8/01, phase-3.1/07 | p02 | Mỗi goal có nguồn |
| `srs/03-stakeholders-and-actors` | Actors system + human | phase-8/02 | p02 | Phân loại rõ |
| `srs/04-glossary` | Thuật ngữ | phase-8/00 §3, MASTER-03 | p02 | ≥20 thuật ngữ |
| `srs/05-current-docs-review` | Review + mapping docs cũ | toàn bộ phase-8/PACK/TECH | p01 | Mọi phase-8 doc có mapping |
| `srs/06-assumptions-and-open-questions` | Assumptions + open Qs | plan/15, phase-8/24,/25 | p02/p14 | Có owner + tác động |
| `functional/` | FR chi tiết theo domain | phase-8/00,03,04,05,06,07,13,14,22,23; TECH-09 | p03 | Giữ mã P0, có traceability |
| `non-functional/` | NFR | phase-8/16 | p08 (hoặc p03 phụ) | Có số capacity, reliability |
| `workflows/` | 8 luồng + state machines | phase-8/14,05,07,23 | p04 | Diagram render được |
| `api/` | API + contracts + error + openapi | phase-8/11,04,06,07; TECH-01 | p05 | Không endpoint update order; error map đủ |
| `database/` | ERD/tables/enums/indexes/retention/migration | phase-8/12,13; MASTER-03 | p07 | Constraint attempt/program; idempotency unique |
| `data/` | ownership + mapping + missing + PII | phase-8/02,04,08,12; MASTER-01,03 | p06 | Mọi trường task có ownership |
| `architecture/` | context/boundary/integration/deploy/resilience | phase-8/10,13,16,17,18; MASTER-04 | p08 | Failure matrix khớp phase-8/02 |
| `modules/` | service block nội bộ | phase-8/10,13 | p08 | Rõ ranh giới service |
| `ui/` | admin/ops console | phase-8/08,11; TECH-01 | p12 | Privacy-safe, permission map |
| `testing/` | strategy + plans + acceptance + smoke | phase-8/09,19; MASTER-05; TECH-10 | p11 | Map IVR-SMK; P0 negative đủ |
| `diagrams/` | mermaid dùng chung | (tổng hợp) | p04/p08 | Nguồn diagram tập trung |
| `_review/` | normalization + traceability + open-decisions | phase-8/25; AI-EVALUATION | p14 | Traceability đầy đủ |
| `decisions/` | ADR | (khi override) | mọi prompt khi có quyết định | Mỗi override có 1 ADR |

## 3. Cấu trúc `integration-requirements/` đề xuất (tạo khi chạy p09)

```txt
integration-requirements/
  00-index.md
  01-sales-platform-requirements.md
  02-ops-core-requirements.md
  03-telephony-sim-requirements.md
  04-shared-auth-audit-requirements.md
  05-open-contract-questions.md
```
Mục đích: tài liệu chính thức gửi team sales/ops/telephony. Nguồn: plan/10,11,12 + phase-8/17,02. Hoàn thành khi mỗi API-need có priority/owner/mock-note.

## 4. Cấu trúc `seed/` đề xuất (tạo khi chạy p10)

```txt
seed/
  README.md
  customers.sample.json      # giả lập projection sales
  orders.sample.json         # official orders đủ điều kiện IVR
  products.sample.json       # giả lập ops (gồm sale-lock/recall)
  inventory.sample.json
  ivr-tasks.sample.json      # IvrConfirmationTaskV1 mẫu
  call-scenarios.sample.json # confirm/cancel/no-answer/invalid/technical/race/trusted/capacity
  ivr-menu.sample.json       # script + phím 1/0
  agents.sample.json         # admin/ops actors + permission
  integration-status.sample.json # sales/ops/SIM up|down
```
Mục đích: chạy dry-run/smoke khi chưa có API thật. Nguồn: plan/13 + phase-8/09,12. Hoàn thành khi phủ smoke scenarios + không PII thật.

## 5. Cấu trúc `prompt/` (root, chính thức) đề xuất (tạo khi chạy p13)

```txt
prompt/
  00-index.md
  <dev prompt theo TECH-13, mỗi prompt gắn requirement ID + source + evidence>
```
Mục đích: dev handoff cho codex/copilot. Nguồn: TECH-13/11/12 + specs ổn định. **Chỉ tạo sau khi specs qua p14 ổn định.**

## 6. Lưu ý

- Cấu trúc trên là đề xuất; khi sinh thật có thể điều chỉnh theo convention repo, **không được làm mất semantic contract** của baseline.
- `specs/decisions/` (ADR) bắt buộc cho mọi lần lệch baseline.
