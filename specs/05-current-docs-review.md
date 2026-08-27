# SRS-05 — Current Docs Review (IVR Order Confirmation)

> **HISTORICAL REVIEW / SUPERSEDED:** kết luận COD-only/D-10 khóa trong file này đã bị Target V1
> Round 5 ngày 2026-08-12 supersede; các mô tả trusted-skip phía IVR tiếp tục bị `OD-18`/`W-0123`
> thay thế ngày 2026-08-27. Xem `plan/ivr-orther/target-contract-v1-draft.md`. Nội dung dưới đây
> chỉ để truy vết, không phải implementation source.

Trạng thái: `DOCS_REVIEW — DRAFT` (chưa phải specs nghiệp vụ; đây là bước rà soát nguồn)
Sinh bởi: prompt `plan/ivr-orther/prompts/p01-generate-docs-review.md`
Ngày: 2026-07-02 · Ngôn ngữ: Tiếng Việt (thuật ngữ kỹ thuật giữ tiếng Anh)
Module: IVR Order Confirmation (working name `ivr-orther`; slug đề xuất `ivr-order-confirmation`) — PACK-09 / TECH-09 / phase-8 / Module 8.

> Đây là file specs **duy nhất** được tạo ở bước p01. Mapping docs→specs, inventory final, và danh sách file tham chiếu bị thiếu nằm trong chính file này (§7, §8, §9). Chưa tạo bất kỳ file specs nào khác.

## 1. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/4. phase/phase-8/00..26` | Bộ SRS/SDS baseline IVR (markdown) |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Pack source IVR |
| `docs/documents/3. tech/10-TECH-09-...ANTI-FAKE-ORDER-CONTROL.md` | Technical source IVR |
| `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.2_CLEAN_FINAL.docx` | **Bản consolidated V0.2 mới nhất** (mã `GFD-M8-IVR-ORDER-CONFIRMATION-TECHDESC-002`) |
| `docs/documents/1. master/01,02,04,06` | Governance / source-of-truth / traceability-id / evidence-gate |
| `docs/documents/00-AI-EVALUATION-DEV-READINESS.md` | Chuẩn đánh giá dev-readiness |
| `plan/ivr-orther/01-reading-inventory.md`, `03-ivr-related-findings.md`, `07-source-of-truth-build-plan.md` | Đầu vào từ plan |

## 2. Phương pháp

- Đọc chi tiết phần lõi (phase-8: 00, 02, 04, 07, 11, 12) + trích xuất & đọc toàn văn `.docx` V0.2 (819 đoạn, ~28.6K ký tự) + tổng hợp các nhóm còn lại qua khảo sát tài liệu.
- Đánh giá mỗi tài liệu theo: **độ chín** (`analysis` / `SRS_BASELINE` / `SDS_BASELINE` / `consolidated`), **độ tin cậy**, **điểm mâu thuẫn**.
- Nhãn: `CONFIRMED / ASSUMPTION / NEED_CONFIRMATION / TODO / GAP / RISK`.

## 3. Tổng quan độ chín theo nhóm

| Nhóm | Độ chín | Độ tin cậy | Ghi chú |
| --- | --- | --- | --- |
| phase-8/00–09 | `SRS_BASELINE` | Cao | Governance, scope, contract, eligibility, scheduler, adapter, normalizer, monitoring, test/release |
| phase-8/10–20 | `SDS_BASELINE` | Cao (nhưng lệch với docx) | Deployment, API, DB, service, workflow, security, NFR, integration, ops |
| phase-8/22–26 | baseline/traceability | Cao | Input baseline, order-confirmation UC, gap closure, trace matrix, rollup |
| phase-8/21 | decision | TB | Dọn dẹp chính sách gọi lại (liên quan attempt) |
| `.docx` V0.2 | **`consolidated` (mới nhất)** | **Cao — nhưng LÀ NGUỒN GÂY MÂU THUẪN** | Chọn rule attempt mới của PACK-09; mô hình object đơn giản hơn; ID scheme khác |
| PACK-09 / TECH-09 | pack/tech source | Cao | Nguồn gốc; PACK-09 "Input Baseline V1.0" là nơi khóa rule attempt mới |
| MASTER 00–09 | governance | Cao | Bắt buộc tuân |

## 4. Review chi tiết tài liệu IVR (phase-8)

| Doc | Độ chín | Tin cậy | Điểm chính / lưu ý mâu thuẫn |
| --- | --- | --- | --- |
| 00 Governance/Scope | SRS_BASELINE | Cao | Khóa scope IN/OUT, source-of-truth, P0, governance gates. **Attempt policy ghi 2/10 & 3/15** (bản cũ) |
| 01 Business purpose/UC | SRS_BASELINE | Cao | Mục đích anti-fake, phím 1/0, trusted skip |
| 02 Ownership/Connected | SRS_BASELINE | Cao | Ownership matrix, data allowed/prohibited, failure contracts — nhất quán với docx |
| 03 Eligibility/Trust/Contact | SRS_BASELINE | Cao | phone_ref/masked, trusted skip, invalid phone |
| 04 Task contract | SRS_BASELINE | Cao | `IvrConfirmationTaskV1`; **max_attempts theo program (2/3)** (bản cũ) |
| 05 Attempt policy/Scheduler/Queue | SRS_BASELINE | TB→Cao | **Golden Hour 2/10, 24/7 3/15, spacing 300s** — MÂU THUẪN với docx |
| 06 SIM adapter | SDS-ish | Cao | DTMF, disposition, no order write |
| 07 Result/Callback | SRS_BASELINE | Cao | Result taxonomy md (khác naming docx); revalidation; race matrix |
| 08 Monitoring/Audit/Privacy | SRS_BASELINE | Cao | RBAC, evidence, recording OFF, retention |
| 09 Test matrix/Release gate | SRS_BASELINE | Cao | IVR-SMK-001..030 (khác ID docx M8-P0-*) |
| 10 Deployment arch | SDS_BASELINE | Cao | Internal SIM gateway |
| 11 API design | SDS_BASELINE | Cao | `/v1/ivr/order-confirmation/*`; tách bảng attempts/results/callbacks (khác object docx) |
| 12 Database design | SDS_BASELINE | Cao | 9–10 bảng `ivr_*` số nhiều; **constraint 24/7 max=3, window=900** — MÂU THUẪN với docx |
| 13 Function/Service | SDS_BASELINE | Cao | Services intake/scheduler/normalizer |
| 14 Workflow orchestration | SDS_BASELINE | Cao | 8 luồng |
| 15 Security/Privacy/Audit | SDS_BASELINE | Cao | Threat model, PII redaction |
| 16 NFR | SDS_BASELINE | Cao | Capacity SIM 12/24/32 |
| 17 Integration design | SDS_BASELINE | Cao | Con trỏ integration-requirements |
| 18 Observability/Runbook | SDS_BASELINE | TB | Ops runbook |
| 19 Smoke/Release plan | SDS_BASELINE | Cao | Smoke/release |
| 20 Task/backlog plan | plan | TB | Không phải specs nghiệp vụ → feed roadmap |
| 21 Callback/attempt cleanup decision | decision | TB | Ảnh hưởng attempt/callback |
| 22 IVR input baseline | baseline | Cao | Đường cơ sở đầu vào (khả năng đồng bộ PACK-09 V1.0) |
| 23 Order confirmation via IVR | SRS_BASELINE | Cao | UC xác nhận |
| 24 Gap closure (tiền SRS) | baseline | Cao | Danh sách `Owner Decision Required` |
| 25 Traceability matrix | traceability | Cao | IVR-00..09 → source |
| 26 Rollup phase-8 | rollup | Cao | Bảng gom |

## 5. Review bản `.docx` V0.2 (bản consolidated mới nhất) — quan trọng

- CONFIRMED (metadata): mã `GFD-M8-IVR-ORDER-CONFIRMATION-TECHDESC-002`; version **V0.2 "Clean Final for Owner/Tech Lead/Dev Review"**; `created 2026-06-06`, `modified 2026-06-08`, revision 3; tạo bằng `python-docx`. Trạng thái: `READY FOR OWNER/TECH LEAD/DEV REVIEW`, `IVR_GATE=BLOCKED`, `PRODUCTION_READY=NO`, `REAL_CUSTOMER_CALL_ALLOWED=NO`, `IVR IMPLEMENTATION STATUS=NOT_STARTED`.
- CONFIRMED: docx là **bản chưng cất/hợp nhất** của toàn bộ phase-8 (có đủ 0–25 mục: scope, source-of-truth, roles, architecture, entry gate, object contract, eligibility, attempt policy, call script, SIM gateway, capacity, scheduler, normalization, callback, technical boundary, admin, security, module connections, roadmap slice M8.2A–H, P0 smoke, evidence plan, P0 rules, done/fail gate, open decision register, kết luận).
- CONFIRMED (điểm mới quan trọng nhất): docx **tự công bố chọn rule attempt MỚI của PACK-09 IVR Input Baseline V1.0** và ghi rõ nó **khác** baseline cũ. Trích: *"Phase 8 SRS baseline cũ có chỗ ghi Golden Hour 2 cuộc/10 phút và 24/7 3 cuộc/15 phút. … PACK-09 V1.0 khóa mới: Giờ Vàng 5 phút, 2 cuộc, cách 2:30; 24/7 15 phút, 2 cuộc, cách 7:30. Bản V0.2 này chọn rule mới làm rule triển khai."*
- CONFIRMED (xác nhận ánh xạ module↔phase): docx §1 & §18 liệt kê tường minh **Module 3=Phase 3 Commerce, Module 4=Phase 4 AI Advisor, Module 5=Phase 5 Facebook Gateway, Module 6=Phase 6 Ads, Module 7=Phase 7 MC AI Live, Module 8=Phase 8 IVR** → nâng ánh xạ "module N = phase N" từ `ASSUMPTION` lên **`CONFIRMED`** (ít nhất cho module 3–8; suy ra module 1=phase-1, 2=phase-2, 3.1=phase-3.1).
- CONFIRMED (call script chính thức): docx §9.1 có mẫu lời gọi thật (biến `order_code_short`, `total_amount_display`; `customer_name_short`/`program_name` optional). KEY_9 human support = `NOT_ENABLED`.

## 6. Mâu thuẫn phát hiện (CONTRADICTIONS) ⚠️

| # | Chủ đề | phase-8 markdown | `.docx` V0.2 (mới hơn) | Mức | Xử lý |
| --- | --- | --- | --- | --- | --- |
| C-01 | **Attempt policy** | Golden Hour **2 cuộc/10 phút**; 24/7 **3 cuộc/15 phút**; spacing ~300s; window 600/900 | Golden Hour **5 phút, 2 cuộc, cách 2:30**; 24/7 **15 phút, 2 cuộc, cách 7:30**; **MAX_ATTEMPT=2 cả hai**; interval = ½ window | **P0 — nghiêm trọng** | ✅ **RESOLVED — D-10 (Module 3.1, 2026-07-02): chọn RULE MỚI.** `T0`=lúc Core mở window/tạo task |
| C-02 | **DB constraint 24/7 max_attempts** | `12` §4: 24/7 `max_attempts=3, window=900` | `MAX_ATTEMPT_PER_ORDER=2` cho mọi program | **P0** | ✅ **RESOLVED — D-10: 24/7 `max_attempts=2`** (window 900s giữ). Cần sửa constraint DB `12` khi sinh database |
| C-03 | **Result taxonomy naming** | `IVR_NO_ANSWER_ATTEMPT`, `INVALID_PHONE_FINAL`, `IVR_POLICY_BLOCKED`, `IVR_OPERATIONAL_BLOCKED`, `IVR_CAPACITY_EXCEPTION`, `IVR_CUSTOMER_NEEDS_SUPPORT` | `ATTEMPT_1_NO_ANSWER`, `IVR_INVALID_PHONE_FINAL`, `IVR_WRONG_INPUT`, `IVR_OPT_OUT` (bỏ policy/operational/capacity/needs-support tường minh) | P1 | Chuẩn hóa một bảng result-code hợp nhất (p03/p06); giữ superset, map alias |
| C-04 | **Data object model** | 9–10 bảng số nhiều: `ivr_confirmation_tasks`, `ivr_call_jobs`, **`ivr_call_attempts`**, `ivr_call_results`, `ivr_result_callbacks`, `ivr_sim_channels`, `ivr_capacity_incidents`, **`ivr_technical_exceptions`**, **`ivr_admin_actions`** | Object số ít, gọn hơn: `ivr_task`, `ivr_call_job`, `sim_channel`, **`ivr_raw_call_event`**, `ivr_result`, `order_core_callback`, `capacity_incident`, `ivr_audit_evidence` (gộp technical/admin vào audit; thêm raw_call_event) | P1 | Hợp nhất ở p07; giữ mức chi tiết của md (attempts riêng, technical/admin riêng) + bổ sung `raw_call_event` từ docx |
| C-05 | **ID / mã yêu cầu** | `IVRxx-FR-*`, `IVRxx-P0-*`, `IVR-SMK-001..030`, `IVR-00..09/10..20` | `M8.2A–H` (slice), `M8-SCH-*`, `M8-P0-*`, `M8-DONE-*`, `M8-FAIL-*`, `M8-OD-*`, `P0-01..09` | P1 | Chọn 1 scheme chính + bảng ánh xạ chéo (p14 traceability). Đề xuất: giữ `IVR-*` cho SRS, `M8.*` cho slice/backlog |
| C-06 | **SIM cooldown** | (nêu chung) | `M8-SCH-005: SIM_COOLDOWN_AFTER_CALL=5s`; fail_count≥3/10 phút → disable | P2 | Lấy số cụ thể của docx |
| C-07 | **Capacity hệ số** | SIM 12/24/32 baseline | + `AVG_CALL_DURATION=35s`, `CONSERVATIVE_CYCLE=50s`, roadmap 12→24/32→64→96 | P2 | Lấy số docx (chi tiết hơn) |
| C-08 | **Invalid phone xử lý** | `INVALID_PHONE_FINAL` (thiên cancel/final) | docx M8-OD-004: mặc định **admin review** hoặc Core policy | P1 | `Owner Decision Required` |

> ✅ Cập nhật 2026-07-02: C-01 đã được chốt (**D-10 = rule mới**). Các file plan/specs trích attempt policy đã/đang được đồng bộ về rule mới (xem `plan/ivr-orther/decisions-log.md` §"Hệ quả cập nhật"). DB constraint 24/7 sẽ đặt `max_attempts=2` khi sinh database (p07).

## 7. Mapping docs cũ → specs mới

Ánh xạ mỗi tài liệu nguồn tới (các) file specs dự kiến (theo cấu trúc `plan/ivr-orther/08-target-specs-structure-proposal.md`). Ký hiệu file specs viết tắt dưới `specs/srs/`.

| Nguồn | → File specs mới | Prompt |
| --- | --- | --- |
| phase-8/00 Governance/Scope | `01-context-and-scope`, `02-business-goals`, `functional/` (P0), `decisions/` | p02, p03 |
| phase-8/01 Business/UC | `02-business-goals`, `01-context-and-scope`, `functional/01,04` | p02, p03 |
| phase-8/02 Ownership/Connected | `architecture/02,03`, `data/01-data-ownership`, `03-stakeholders-and-actors` | p06, p08, p02 |
| phase-8/03 Eligibility/Trust/Contact | `functional/02-eligibility-and-blockers`, `data/05-pii-policy` | p03, p06 |
| phase-8/04 Task contract | `api/05-order-core-contracts`, `functional/01-task-intake`, `database/02-tables` | p05, p03, p07 |
| phase-8/05 Attempt/Scheduler/Queue | `functional/03-scheduler-attempt-policy`, `workflows/03`, `architecture/04` | p03, p04, p08 |
| phase-8/06 SIM adapter | `api/04-sim-adapter-contract`, `functional/04-call-execution-dtmf`, `architecture/02` | p05, p03, p08 |
| phase-8/07 Result/Callback | `functional/05`, `api/05`, `workflows/05`, `database/03-enums-and-status` | p03, p05, p04, p07 |
| phase-8/08 Monitoring/Audit/Privacy | `ui/`, `functional/08-evidence-audit-privacy`, `data/05`, `database/05-retention-and-privacy` | p12, p03, p06, p07 |
| phase-8/09 Test matrix/Release | `testing/09-smoke-matrix`, `testing/08-acceptance-criteria` | p11 |
| phase-8/10 Deployment arch | `architecture/01-system-context`, `architecture/04-deployment-architecture` | p08 |
| phase-8/11 API design | `api/02-internal-api`, `api/03-admin-api`, `api/06-error-codes`, `api/07-idempotency-and-correlation` (+ openapi) | p05 |
| phase-8/12 Database design | `database/01-erd`, `database/02-tables`, `database/03-enums`, `database/04-indexes` | p07 |
| phase-8/13 Function/Service | `architecture/02-module-boundaries`, `modules/` | p08 |
| phase-8/14 Workflow orchestration | `workflows/01..08` | p04 |
| phase-8/15 Security/Privacy/Audit | `non-functional/` (security), `data/05`, `ui/08-role-permission-ui` | p08, p06, p12 |
| phase-8/16 NFR | `non-functional/*` | p08 |
| phase-8/17 Integration design | `integration-requirements/*`, `architecture/03-integration-architecture` | p09, p08 |
| phase-8/18 Observability/Runbook | `architecture/06-observability` | p08 |
| phase-8/19 Smoke/Release plan | `testing/01-strategy`, `testing/09-smoke-matrix` | p11 |
| phase-8/20 Task/backlog plan | (không phải specs) → `prompt/` roadmap, `16-prompt-roadmap` | p13 |
| phase-8/21 Callback/attempt cleanup | `decisions/` (ADR), `functional/03` | p03 |
| phase-8/22 IVR input baseline | `01-context-and-scope`, `functional/03` (attempt) | p02, p03 |
| phase-8/23 Order confirmation UC | `functional/01,04,05`, `workflows/01` | p03, p04 |
| phase-8/24 Gap closure | `06-assumptions-and-open-questions`, `_review/open-decisions-register` | p02, p14 |
| phase-8/25 Traceability matrix | `_review/traceability-matrix` | p14 |
| phase-8/26 Rollup | `00-index` | p02/p14 |
| **`.docx` V0.2** | **cross-cutting**: nguồn đối chiếu cho `functional/03` (attempt), `database/*`, `api/*`, `testing/*`, `decisions/` (mọi mâu thuẫn C-01..C-08) | p01/p03/p07/p14 |
| PACK-09 | source cho `01-context`, `functional/03` (attempt V1.0), `decisions/` | p02, p03 |
| TECH-09 | source cho `functional/*`, `api/*`, classification | p03, p05 |

## 8. Inventory final (trạng thái đọc)

| Path | Nhóm | Rel | Đọc | Trạng thái |
| --- | --- | --- | --- | --- |
| phase-8/00,02,04,07,11,12 | IVR | H | **Chi tiết (đọc kỹ)** | Đã dùng làm nền phân tích |
| phase-8/01,03,05,06,08,09,10,13–26 | IVR | H/M | Scan (qua khảo sát tổng hợp) | Đủ để mapping; đọc kỹ thêm ở p03–p11 |
| `MODULE_8_...V0.2.docx` | IVR | H | **Chi tiết (trích xuất toàn văn)** | Đã đọc; phát hiện mâu thuẫn C-01..C-08 |
| PACK-09, TECH-09 | IVR | H | Scan + resolved by D-10 | PACK-09 "Input Baseline V1.0" đã được chốt qua D-10; rule cũ chỉ còn lịch sử |
| MASTER 00–09, TECH-00/01/10/11 | Shared | H | Scan | Convention nền tảng |
| phase-3, phase-3.1, TECH-04/05, PACK-05 | Sales | H | Scan | Cho integration |
| phase-1, phase-2, TECH-02/03, PACK-01 | Ops | H | Scan | Cho blocker/recall |

Chi tiết đầy đủ 179 file: xem `plan/ivr-orther/01-reading-inventory.md` (không lặp lại toàn bộ ở đây để tránh trùng).

## 9. File tham chiếu bị thiếu (GAP)

| File được trích | Trích ở đâu | Trạng thái | Xử lý đề xuất |
| --- | --- | --- | --- |
| `docs/source-map.md` | phase-8/00 §2, /15 | **KHÔNG tồn tại** | Tạo `source-map` (có thể đặt trong `specs/`) ánh xạ requirement→source path |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | phase-8/00,/02,/07 | **KHÔNG tồn tại (lệch tên)** | Nội dung tương ứng = `24-ĐÓNG KHOẢNG TRỐNG TIỀN SRS IVR.md`; chuẩn hóa tham chiếu |
| `openapi/business-platform/ivr-order-confirmation.v1.yaml` | phase-8/11 §1 | **KHÔNG tồn tại** | Sinh ở p05 vào `specs/srs/api/openapi/` (nếu owner duyệt — Q-A1) |
| `events/business-platform/ivr/*` | phase-8/12 §10 | **KHÔNG tồn tại** | Chỉ khi bật event bus (toolchain chưa duyệt) |
| `docs/documents/2. pack/09` "Input Baseline V1.0" phần số attempt | phase-8/05, docx §1 | Có PACK-09; đã được D-10 chốt thành rule triển khai | Không còn TODO; xem `plan/ivr-orther/decisions-log.md` D-10 |

## 10. Nguồn nào đủ chín làm source / cần bổ sung

- ĐỦ CHÍN làm nền chưng cất trực tiếp: phase-8/00,01,02,03,04,07,08,09 (SRS) + 10,11,12,13,14,15,16 (SDS) + docx V0.2 (đối chiếu).
- ĐÃ CHỐT SAU REVIEW: C-01/C-02 (attempt policy) đã resolved bằng D-10; prompt/spec hiện hành dùng `max_attempts=2`, GH 300/150, 24-7 900/450.
- CẦN BỔ SUNG/ĐỌC KỸ: phase-8/17 (integration), TECH-09 (classification), C-05 (ID scheme) nếu cần chuẩn hóa sâu hơn.
- KHÔNG dùng làm source nghiệp vụ: phase-8/20 (backlog), phase-4/.codex-doc-memory (metadata công cụ).

## 11. Khuyến nghị source-of-truth & open decisions phát sinh

- Thứ tự ưu tiên khi mâu thuẫn (theo `plan/ivr-orther/07` §6): **MASTER > PACK/TECH > docx V0.2 (mới hơn) > phase-8 md > suy luận**. Với C-01/C-02, owner đã chốt D-10: **PACK-09 V1.0 rule mới là nguồn attempt-policy triển khai**.
- Open decisions mới phát sinh từ bước này (thêm vào `06-assumptions-and-open-questions` / `_review/open-decisions-register` ở p02/p14):
  - ✅ **OD-DR-01 (P0) → RESOLVED — D-10 (Module 3.1, 2026-07-02):** chọn rule mới PACK-09 V1.0 (2 cuộc cả hai program; GH 5′/2:30; 24/7 15′/7:30; `T0`=lúc Core mở window).
  - **OD-DR-02 (P1):** Chọn ID scheme chính (`IVR-*` vs `M8-*`) + bảng ánh xạ chéo.
  - **OD-DR-03 (P1):** Chọn data-object model (md chi tiết vs docx gọn) — đề xuất giữ md + bổ sung `ivr_raw_call_event`.
  - **OD-DR-04 (P1):** Chuẩn hóa result taxonomy (superset + alias) — C-03.
  - **OD-DR-05 (P1):** Invalid phone → cancel hay admin review (C-08 / M8-OD-004).
  - **OD-DR-06 (P1):** Chọn version chuẩn giữa docx V0.2 và phase-8 md làm "baseline canonical" (đề xuất: docx V0.2 là bản consolidated mới nhất → làm khung, đối chiếu chi tiết SDS từ md).

## 12. Báo cáo cuối (theo template p01)

1. **Số docs đã review:** 29 tài liệu IVR trực tiếp (phase-8 00–26 = 27, + PACK-09, + TECH-09) + docx V0.2 + các MASTER/shared liên quan; tổng kho 179 `.md`.
2. **Số mapping đã lập:** 27 phase-8 docs + docx + PACK-09 + TECH-09 = **30 dòng mapping** docs→specs (§7).
3. **Số file tham chiếu bị thiếu:** **5** (§9): `source-map.md`, `ivr-pre-srs-gap-closure.md` (lệch tên → doc 24), `openapi/...v1.yaml`, `events/.../ivr/*`, phần "Input Baseline V1.0" cần đọc kỹ.
4. **Danh sách mâu thuẫn:** **8** (C-01..C-08); nghiêm trọng nhất là C-01/C-02 (attempt policy: docx/PACK-09 V1.0 = 2 cuộc cả hai program vs md = 2/10 & 3/15).
5. **Docs đủ chín / cần bổ sung:** phase-8/00–16 + docx đủ chín làm nền; cần chốt C-01/C-05 và đọc kỹ PACK-09 V1.0 trước khi sinh functional/database/testing.

## 13. Bước tiếp theo (cập nhật 2026-07-02)
- ✅ OD-DR-01 (attempt policy) đã được chốt → **D-10** (Module 3.1). p02/p03/p04 đã chạy.
- ✅ Module 3/3.1 đã trả lời 14 câu hỏi → **D-01..D-14** (`plan/ivr-orther/decisions-log.md`); đã đồng bộ vào specs + plan.
- ⏳ Còn chờ để tiếp tục p05–p07 đầy đủ: **Ops-Core** (QO1–QO3 blocker API), **Telephony** (SIM protocol/disposition — Q-T1/T2), **Foundation** (allowlist/OpenAPI/release gate — Q-A1/A2/K1).
- ✅ **p05 đã chạy** → `specs/srs/api/*` + OpenAPI 3.1. Bước tiếp: **p06 (data mapping)** → **p07 (database, đặt constraint 24/7 `max_attempts=2` theo D-10)** → **p08 (architecture, adapter port DT-01)**.
- Chặn gọi thật còn lại: **mua SIM** (DT-01/04) và **release sign-off** (DF-03). **Q-C1/DC-01** đã có nguồn; **DG-03/DS-01..05** đã trả lời, các phần còn lại là build/target P1/P2.
