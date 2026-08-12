# 16 — Prompt Roadmap

Hệ thống prompt con để sinh specs dần. Chi tiết từng prompt: [prompts/](prompts/00-index.md).

> ✅ **HOÀN TẤT p01–p14 (2026-07-02).** Roadmap này ban đầu viết ở Giai đoạn 1 (thì tương lai); đã cập nhật theo thực tế. Quyết định P0 gate đã có định hướng (D-/DO-/DF-/DT- trong [decisions-log.md](../decisions-log.md)). Trạng thái chi tiết: [plan index](../00-index.md) và [open-decisions-register.md](../../../specs/_review/open-decisions-register.md).

## 1. Danh sách prompt con

| Prompt | Mục tiêu | Input chính | Output chính |
| --- | --- | --- | --- |
| p01 | Docs review + mapping cũ→mới | phase-8/PACK/TECH; plan/01,03,07 | `specs/srs/05-current-docs-review.md` + mapping/inventory |
| p02 | Context/scope/goals/actors/glossary/assumptions | phase-8/00,01,02,03; plan/02,15 | `specs/srs/01,02,03,04,06` |
| p03 | Functional SRS | phase-8/00,03-07,13,14,22,23; TECH-09 | `specs/srs/functional/*` |
| p04 | Workflows + state machines | phase-8/14,05,07,23 | `specs/srs/workflows/*` |
| p05 | API specs + contracts + error + openapi | phase-8/11,04,06,07; TECH-01; plan/11,12 | `specs/srs/api/*` |
| p06 | Data ownership/mapping/PII | MASTER-01,03; phase-8/02,04,08,12 | `specs/srs/data/*` |
| p07 | Database design | phase-8/12,13; MASTER-03; TECH-01 | `specs/srs/database/*` |
| p08 | Architecture + NFR + modules | phase-8/10,13,16,17,18; MASTER-04 | `specs/srs/architecture/*`, `non-functional/*`, `modules/*` |
| p09 | Integration requirements (sales/ops/telephony/shared) | plan/10,11,12; phase-8/17,02; phase-3.1/07 | `integration-requirements/*` |
| p10 | Seed data | plan/13; phase-8/09,12; specs/database,data | `seed/*` |
| p11 | Testing specs | phase-8/09,19; MASTER-05; TECH-10 | `specs/srs/testing/*` |
| p12 | UI specs | phase-8/08,11; TECH-01 | `specs/srs/ui/*` |
| p13 | Final prompt library (root) | specs ổn định; TECH-13/11/12 | `prompt/*` |
| p14 | Review/normalize | toàn bộ specs; AI-EVALUATION; phase-8/25 | `specs/srs/_review/*` |

## 2. Thứ tự chạy — ✅ đã thực hiện

Thực tế đã chạy: p01 → p02/p03/p04 → **p05** → **p06** → **p07** → **p08** → **p09** → **p10** → **p11** → **p12** → **p14 (round 1)** → **p13** → **p14 (round 2)**. Tất cả ✅.

| Prompt | Output thực tế | Trạng thái |
| --- | --- | --- |
| p01 | `specs/srs/05-current-docs-review.md` | ✅ |
| p02 | `specs/srs/01–04, 06` | ✅ |
| p03 | `specs/srs/functional/*` | ✅ |
| p04 | `specs/srs/workflows/*` | ✅ |
| p05 | `specs/srs/api/*` + `openapi/ivr-order-confirmation.v1.yaml` | ✅ |
| p06 | `specs/srs/data/*` | ✅ |
| p07 | `specs/srs/database/*` (11 bảng) | ✅ |
| p08 | `specs/srs/architecture/*` (NFR/modules gộp vào architecture) | ✅ |
| p09 | `integration-requirements/*` (root) | ✅ |
| p10 | `seed/*` (root, 9 JSON) | ✅ |
| p11 | `specs/srs/testing/*` | ✅ |
| p12 | `specs/srs/ui/*` | ✅ |
| p13 | `prompt/*` (root, 10 dev prompt) | ✅ |
| p14 | `specs/srs/_review/*` (2 vòng) | ✅ (lặp mỗi vòng) |

## 3. Điều kiện chạy (gate) — trạng thái đã thỏa

Các gate câu hỏi P0 khi chạy prompt **đều đã có định hướng/đã khóa** (không còn treo như lúc lập plan):
| Gate câu hỏi (lúc lập plan) | Trạng thái hiện tại |
| --- | --- |
| Q-B1 (scope) | ✅ D-08 (outbound-only) |
| Q-S1 (order state) | ✅ D-02 + DG-03 resolved by DS-01..05 (`CONFIRMING+COD`; target OC1 for version) |
| Q-S2/S3 (transport/dial token) | ✅ D-03/D-05 |
| Q-A1/A2 (OpenAPI/allowlist) | ✅ DF-02/DF-06 |
| Q-DB1 (RDBMS/convention) | ✅ theo repo (DF-04/05) |
| Q-T1/T2 (SIM protocol/disposition) | ⏳ DT-01 (mua SIM) / ✅ DT-02 (mapping) |
| Q-U1 (admin platform) | ✅ theo phase-8/08 + DF-01 |
| Q-F1 (order_code) | ✅ D-01 (đã giải quyết tension) |
| p13 gate (specs ổn định + P0 đóng) | ✅ p01–p12+p14 xong; P0 còn lại là procurement/legal/CRM (không chặn specs) |

## 3b. Chú ý output khác plan gốc
- `non-functional/` và `modules/` (đề xuất riêng ở `08-target-specs-structure-proposal`) → **gộp vào `architecture/*`** (NFR ở `architecture/04-06`, module boundaries ở `architecture/02`). Không tách riêng — chấp nhận.
- `specs/decisions/` (ADR) chưa tạo — quyết định hiện gom ở `plan/ivr-orther/decisions-log.md`. Tạo `specs/decisions/*` khi có override baseline chính thức.

## 4. Checklist sau khi chạy mỗi prompt

- [ ] Output đúng file được phép tạo (không tạo file ngoài phạm vi).
- [ ] Có bảng "Nguồn tham chiếu" (path docs).
- [ ] Dùng nhãn CONFIRMED/ASSUMPTION/NEED_CONFIRMATION/TODO/GAP/RISK.
- [ ] Không suy diễn quyết định treo (ghi `Owner Decision Required`).
- [ ] Không vi phạm P0 boundary (IVR không update order/payment/notification; technical≠no-answer).
- [ ] Báo cáo cuối theo template trong file prompt.

## 5. File được phép / chưa được phép tạo theo từng prompt

| Prompt | ĐƯỢC tạo | CHƯA được tạo |
| --- | --- | --- |
| p01 | `specs/srs/05-*` + mapping | phần specs còn lại |
| p02 | `specs/srs/01,02,03,04,06` | functional/api/db… |
| p03 | `specs/srs/functional/*` | api/db/workflow |
| p04 | `specs/srs/workflows/*` | api/db |
| p05 | `specs/srs/api/*` (+openapi nếu duyệt) | db/seed |
| p06 | `specs/srs/data/*` | db/seed |
| p07 | `specs/srs/database/*` | seed thật |
| p08 | `specs/srs/architecture/*`,`non-functional/*`,`modules/*` | — |
| p09 | `integration-requirements/*` | — |
| p10 | `seed/*` | prod seed |
| p11 | `specs/srs/testing/*` | — |
| p12 | `specs/srs/ui/*` | frontend code |
| p13 | `prompt/*` (root) | production code |
| p14 | `specs/srs/_review/*`, cập nhật `06` | — |

## 6. Nguyên tắc dừng — trạng thái

- ✅ Các Owner Decision P0 lúc lập plan **đã đóng**: Q-F1 (order_code → D-01), Q-S1/DG-03 (order state → D-02 + DS-01..05), Q-S2/S3 (→ D-03/05), Q-A1/A2 (→ DF-02/06), Q-C1 (→ DC-01).
- ⏳ **P0 còn mở (chặn gọi khách thật, KHÔNG chặn tiếp tục code dry-run):** **DT-01** (mua SIM gateway), **DF-03** (release sign-off), **DF-07** (legal retention). **Q-C1/DC-01** và **DG-03/DS-01..05** đã resolved; OC1/OC2/OC3 là target build. Xem [open-decisions-register.md](../../../specs/_review/open-decisions-register.md).
- Không prompt/impl nào được tuyên bố "production-ready" hay bật gọi khách thật; `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi release gate pass.

## 7. Bước tiếp theo (hết prompt sinh specs)
- Hết p01–p14. Việc thật tiếp theo: **(a)** triển khai code theo [prompt/](../../../prompt/00-index.md) ở dry-run/MOCK; hoặc **(b)** đóng hard blockers còn mở: mua SIM, DF-03 sign-off, DF-07 retention/legal, và các target build OC1/OC2/OC3/IR-CRM-01 khi provider sẵn sàng.
- Chạy lại **p14** sau bất kỳ thay đổi specs nào.
