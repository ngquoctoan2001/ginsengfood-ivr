# 01-P1 - THIẾT KẾ KỸ THUẬT SẢN PHẨM SKU CÔNG THỨC

Trạng thái: `TECHNICAL_DESIGN_SOURCE_ABSORBED`  
Mode: `TECHNICAL DESIGN ONLY`  
Mục tiêu: thiết kế kỹ thuật đủ chi tiết để implement Phase 1 mà không cần mở 5 file nguồn cũ.

## 1. Entity boundary

| Entity | Vai trò | Rule không được vi phạm |
| --- | --- | --- |
| Product | Sản phẩm public/business | Không chứa runtime sellable/release state |
| SKU | Mã vận hành nội bộ của biến thể sản phẩm | Không public internal code nếu channel public không được duyệt |
| Ingredient/Material | Nguyên liệu/vật tư canonical | Không tạo trùng material vì khác usage_role |
| MaterialAlias | Tên cũ/tên viết khác/tên lỗi chính tả | Không dùng alias làm source chính |
| UOM | Đơn vị kg/g/l/ml/gói/hộp/thùng/lọ/hũ | Không scale nếu thiếu conversion/density bắt buộc |
| Recipe | Cấu trúc công thức theo SKU | Không dùng recipe không version |
| FormulaVersion | G0/pilot/G1/superseded | Không sửa đè version đã dùng |
| FormulaLine | Từng dòng nguyên liệu cụ thể | Không giữ dòng gom nhóm trong production snapshot |
| PackagingProfile | B1/B2/B3, output gói/hộp/thùng | Không hardcode 7.200/1.800/300 nếu owner đổi output profile |
| ActivationState | Trạng thái product/formula usable | Không đồng nghĩa sellable/released |

## 2. Data contract tối thiểu

Product/SKU phải có:

1. `product_id`, `sku_id`, `internal_sku_code`, `public_name`, `product_group`.
2. `classification`: `NON_VEGAN`, `SAVORY` hoặc `VEGAN_CLAIM_APPROVED`; không seed claim `VEGAN` nếu formula còn dòng động vật.
3. `formula_id`, `formula_version`, `formula_status`.
4. `activation_state`: `DRAFT`, `ACTIVE_BASELINE`, `ACTIVE`, `INACTIVE`, `BLOCKED`.
5. `public_claim_state`: `NOT_REVIEWED`, `APPROVED`, `SUPPRESSED`.
6. `evidence_refs`, `owner_approval_ref`, `created_by`, `updated_by`.

Formula line phải có:

1. `formula_line_id`, `formula_version_id`, `material_id`, `usage_role`.
2. `quantity`, `uom`, `ratio_to_anchor`, `anchor_material_id`.
3. `line_group`: `A1_QTTS`, `A2_BASE_NUTRITION`, `A2_BROTH`, `A3_SEASONING`, `B1_PACKAGING`, `B2_PACKAGING`, `B3_AUXILIARY`.
4. `is_required`, `is_vegan_compatible`, `preprocess_note`, `qc_policy_ref`.

## 3. Resolver bắt buộc

| Resolver | Input | Output | Fail-closed khi |
| --- | --- | --- | --- |
| SKUResolver | SKU code/id | SKU canonical | SKU thiếu, trùng, inactive |
| FormulaResolver | SKU + requested date | FormulaVersion G1/current | Thiếu version approved |
| BOMSnapshotResolver | FormulaVersion + planned quantity | Full BOM snapshot | Có dòng gom nhóm hoặc UOM thiếu |
| AnchorRiceScalingResolver | Anchor quantity | Calculation snapshot | Thiếu ratio/UOM/density |
| VeganFormulaGuard | SKU + formula lines + dietary claim | PASS/BLOCK | SKU/claim vegan có line động vật |
| PackagingProfileResolver | SKU + batch output | B1/B2/B3 requirements | Thiếu output profile |
| ActivationGuard | Product/SKU/formula/evidence | Active/blocked | Thiếu evidence/owner approval |

## 4. Source payload đã nhập vào thiết kế

Thiết kế này consume toàn bộ 5 nguồn cũ theo rule:

1. Công thức 20 SKU là source định lượng, nhưng dữ liệu canonical phải normalize UOM và line group.
2. Bộ khóa 5 bước là source rule về BOM map, operational config, formula version, trace và screen/action.
3. Danh mục phân nhóm là source material/packaging threshold và disposal/write-off.
4. Bảng phiếu lệnh sản xuất là source field cho production consumer.
5. Phiếu tự sinh là source click/tự hiện/nhập tay, print và accounting boundary.

## 5. Acceptance thiết kế

Thiết kế Phase 1 chỉ đạt khi:

1. Dev có thể implement Product/SKU/Material/UOM/Recipe/Formula/Activation mà không mở file nguồn cũ.
2. Phase 2 có thể tạo production dossier và BOM snapshot chỉ từ Phase 1 APIs/projections.
3. Không còn path nào cho production user chọn tay nguyên liệu ngoài snapshot.
4. Vegan guard fail-closed: claim thuần chay chỉ mở khi formula/broth sạch động vật, có owner approval và effective date.
5. Seed idempotent và owner decision được lưu như dữ liệu governance, không hardcode.

## 6. Data contract chi tiết

| Contract | Field bắt buộc | Rule |
| --- | --- | --- |
| `Product` | product_id, public_name, product_group, brand_line, lifecycle_state, public_claim_state | Không chứa sellable/release/inventory state |
| `Sku` | sku_id, internal_sku_code, product_id, sku_name_snapshot, group_code, classification_state, formula_ref | Internal SKU không tự public |
| `Material` | material_id, canonical_name, material_group, source_type, uom_base, qc_required, threshold_policy_code | Một material canonical có nhiều usage role |
| `MaterialAlias` | alias_id, alias_text, material_id, confidence, source_ref | Alias không được dùng làm material_id |
| `UomConversion` | from_uom, to_uom, factor, density_policy_ref, owner_approval_ref | Thiếu conversion thì block scale |
| `RecipeGroup` | recipe_group_id, sku_id, formula_family, anchor_material_id | Recipe phải versioned qua FormulaVersion |
| `FormulaVersion` | formula_version_id, formula_id, version_code, status, effective_from, owner_approval_ref | G0 không production; G1 cần approval để active |
| `FormulaLine` | formula_line_id, material_id, line_group, qty, uom, ratio_to_anchor, preprocess_note, is_required | Không dòng gom nhóm trong production snapshot |
| `PackagingProfile` | profile_id, sku_id, output_gói, output_hộp, output_thùng, B1/B2/B3 refs, loss_policy | Không hardcode nếu owner đổi output |
| `ActivationRecord` | activation_id, sku_id, formula_version_id, guard_result, blocked_reason, evidence_refs | Không đồng nghĩa sellable |

## 7. Command/event thiết kế

| Command | Event thành công | Block khi |
| --- | --- | --- |
| `UpsertProduct` | `ProductUpserted` | Tên public thiếu hoặc trùng không xử lý |
| `UpsertSku` | `SkuUpserted` | SKU code thiếu/trùng/sai group |
| `UpsertMaterial` | `MaterialUpserted` | Tên chưa canonical hoặc duplicate material |
| `MapMaterialAlias` | `MaterialAliasMapped` | Alias trỏ nhiều material không resolve |
| `DefineUomConversion` | `UomConversionDefined` | Factor/density thiếu approval |
| `CreateFormulaVersion` | `FormulaVersionCreated` | Không có formula_id/version/source_ref |
| `UpsertFormulaLine` | `FormulaLineUpserted` | material/UOM/qty thiếu |
| `BuildPackagingProfile` | `PackagingProfileBuilt` | thiếu output hoặc B1/B2/B3 |
| `EvaluateActivationGuard` | `ActivationGuardEvaluated` | thiếu evidence/owner approval/conflict |

## 8. Resolver và projection cho Phase 2

| Projection/API | Input | Output Phase 2 nhận |
| --- | --- | --- |
| `SkuProjection` | sku_id/code | SKU, product name, group, claim state |
| `FormulaProjection` | sku_id + effective date | Formula G1/current, status, owner ref |
| `BomLineProjection` | formula_version_id | Full material lines, qty, UOM, ratio |
| `PackagingProjection` | sku_id + planned qty | B1/B2/B3 material refs, output profile |
| `ThresholdProjection` | material_id + SKU/planning basket | threshold code, suppression rule |
| `ActivationProjection` | sku_id/formula_version | active/blocked + reason |

Phase 2 không đọc trực tiếp file nguồn cũ và không tự suy luận công thức từ tên SKU.

## 9. Rule bảo toàn dữ liệu nguồn

1. Mọi dòng công thức từ `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` phải có `source_ref`.
2. Mọi rule BOM/version/no-manual từ `BỘ KHÓA 5 BƯỚC.md` phải có evidence ID trong P1-06.
3. Mọi material/threshold từ `DANH MỤC...` phải có threshold policy rõ hoặc owner pending.
4. Mọi field production consumer từ `BẢNG PHIẾU...` chỉ là requirement của Phase 1 projection, không tạo runtime phiếu ở Phase 1.
5. Mọi rule tự sinh/print/accounting từ `PHIẾU TỰ SINH...` đi vào boundary, không mở MISA/print runtime ở Phase 1.
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 01-P1-1-TECHNICAL-DESIGN-ONLY.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P1.1 - TECHNICAL DESIGN ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- MASTER source-of-truth va traceability guard.
- PACK-02 Product Master / SKU / Recipe / Activation.
- TECH-02 Product technical boundary.
- TECH-11 Implementation Roadmap boundary.

## Noi Dung Rewrite

## PROMPT-P1.1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION TECHNICAL DESIGN HANDOFF

## MODE: TECHNICAL DESIGN ONLY

P1.1 sẽ chuyển gap analysis thành:

Workstream.

Task breakdown.

Dependency.

Evidence plan.

Smoke plan.

Implementation sequence.

P1.1 vẫn chưa nên sửa code nếu gap còn lớn.

## 19.2. Không tự chuyển mode

Agent không được tự chuyển từ:

## ANALYSIS ONLY

sang:

## IMPLEMENTATION MODE

Chỉ owner/dev lead mới được cho phép prompt tiếp theo.

## 20. PROMPT COPY GIAO DEV / CODEX / COPILOT

## 20.1. Nội dung prompt

## BẮT ĐẦU PROMPT

Bạn đang tiếp tục dự án Ginsengfood theo GREENFIELD TECH SOURCE-OF-TRUTH.

Nhiệm vụ hiện tại:

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS HANDOFF

## MODE: ANALYSIS ONLY

Bạn chỉ được phân tích.

Bạn không được sửa file.

Bạn không được tạo code.

Bạn không được tạo migration.

Bạn không được tạo seed.

Bạn không được update database.

Bạn không được tự đổi nghiệp vụ.

Bạn không được hardcode policy.

Bạn không được gọi Product Active là Sellable.

Bạn không được mở Global Gate.

Bạn không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

A. Source-of-truth bắt buộc

Bạn phải đọc:

## TECH-01.

## TECH-10.

## TECH-11.

## TECH-12.

## TECH-13.

PHASE 0 Validation Report.

TECH Product / SKU / Recipe / Product Activation nếu có.

Quy tắc:

TECH mới thắng code cũ.

TECH mới thắng tài liệu cũ.

Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Legacy chỉ là LEGACY_REFERENCE_ONLY.

Nếu conflict thì báo cáo, không sửa.

B. Scope In

Bạn phải phân tích:

Product Master.

SKU Master.

Ingredient Master.

Recipe Master.

Formula Version.

Formula Kind.

BOM / Ingredient Quantity / UOM.

Product Activation.

Product Active vs Sellable.

SKU Extension Registry.

Product/SKU/Recipe seed hiện có.

Product/SKU/Recipe test hiện có.

Downstream impact sang PHASE 2 -> PHASE 9.

C. Scope Out

Bạn không được:

Sửa code.

Tạo code.

Tạo migration.

Tạo seed.

Tạo API.

Tạo UI.

Kích hoạt Product/SKU.

Sửa Recipe/BOM.

Tạo Sellable Gate.

Sửa Commerce / Operational Core / AI Advisor / Gateway.

Gọi PHASE 1 Complete.

Mở Global Gate.

D. Phân tích bắt buộc Product Active vs Sellable

Bạn phải kiểm tra:

Product Active có bị dùng làm điều kiện bán không.

SKU Active có bị dùng làm sellable không.

Recipe Active có bị dùng làm sellable không.

Có Sellable Gate riêng không.

Có rủi ro Commerce/AI/Gateway dùng sai Product Active không.

Nguyên tắc bắt buộc:

Product Active không đồng nghĩa Sellable.

E. Current implementation state

Dùng một trong các trạng thái:

## NOT_FOUND.

## PARTIAL.

## IMPLEMENTED_BUT_CONFLICT.

## IMPLEMENTED_NEEDS_VALIDATION.

## UNKNOWN_BLOCKED.

Không dùng PASS / COMPLETE / READY.

F. Báo cáo bắt buộc

Bạn phải lập:

Current Implementation State Matrix.

Gap Analysis Matrix.

Conflict Detection Matrix.

P1 điểm chặn Register.

Downstream Impact.

Smoke Required.

Report 14 mục.

G. Report format 14 mục

Bạn phải báo cáo đúng 14 mục:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown.

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

H. Done Gate

Chỉ được coi là analysis done nếu:

Đọc source-of-truth.

Inspect codebase thật.

Phân tích Product/SKU/Ingredient/Recipe/BOM/Formula/Activation.

Phân tích Product Active vs Sellable.

Có gap matrix.

Có conflict matrix.

Có điểm chặn register.

Có downstream impact.

Có smoke required.

Report đủ 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Global Gate vẫn bị chặn.

I. Fail Gate

Phải fail nếu:

Sửa file.

Tạo code.

Tạo migration.

Tạo seed.

Gọi Product Active là Sellable.

Không phân tích Product Active vs Sellable.

Không inspect codebase.

Không lập gap/conflict/điểm chặn.

Gọi Release Readiness / Production Readiness / Go-live Decision.

Mở Global Gate.

J. Trạng thái cuối bắt buộc

Kết thúc report phải ghi:

## KẾT THÚC PROMPT

## 21.1. Trạng thái tài liệu

## 21.2. Trạng thái thực thi

## ANALYSIS HANDOFF ONLY

## 21.3. Phạm vi

## PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS ONLY

## 21.4. Trạng thái PHASE 1

## 21.5. Trạng thái implementation

## 21.6. Trạng thái Completion

## 21.7. Trạng thái release

## 21.8. Trạng thái production

## 21.9. Trạng thái go-live

## 21.10. Trạng thái Global Gate

Global Gate: bị chặn

## PROMPT-P1.1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION TECHNICAL DESIGN HANDOFF

## MODE: TECHNICAL DESIGN ONLY

## FILE

## 01-P1-1-TECHNICAL-DESIGN-ONLY.docx

## PHASE

## PHASE-01 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## MODE

## MODE: TECHNICAL DESIGN ONLY

## PROMPT TIẾP THEO

## PROMPT-P1.2A - LIMITED IMPLEMENTATION

Global Gate

bị chặn

## PROMPT-P1.1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION TECHNICAL DESIGN HANDOFF

## MODE: TECHNICAL DESIGN ONLY - DO NOT MODIFY FILES

## 1. HEADER

## 1.1. Tên prompt

## PROMPT-P1.1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION TECHNICAL DESIGN HANDOFF

## 1.2. Vị trí trong roadmap

PROMPT-P1.1 là bước tiếp theo sau:

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS HANDOFF - MODE: ANALYSIS ONLY

PROMPT-P1.1 thuộc:

PHASE 1 - Product / SKU / Recipe / Product Activation

PROMPT-P1.1 không phải implementation prompt.

PROMPT-P1.1 chỉ dùng để chuyển kết quả analysis của PROMPT-P1 thành:

Technical design handoff.

Workstream breakdown.

Task breakdown.

Dependency matrix.

Evidence plan.

Smoke plan.

điểm chặn handling.

Implementation sequence đề xuất.

Handoff cho prompt implementation sau.

## 1.3. Mục tiêu

Mục tiêu của PROMPT-P1.1 là yêu cầu dev/Codex/Copilot:

Đọc PROMPT-P1 Analysis Report.

Đọc TECH source-of-truth.

Chuyển gap của Product / SKU / Recipe / Product Activation thành workstream.

Chuyển điểm chặn thành task kỹ thuật.

Xác định boundary giữa Product Active và Sellable.

Xác định boundary giữa Recipe Active và Production-ready.

Xác định boundary giữa SKU Active và Commerce Sellable.

Xác định thiết kế cần có cho SKU extension.

Xác định evidence cần có.

Xác định smoke cần có.

Xác định thứ tự implementation an toàn.

Không sửa code.

Không tạo migration.

Không tạo seed.

Không tự đổi nghiệp vụ.

Không gọi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

## 2. MODE: TECHNICAL DESIGN ONLY - DO NOT MODIFY FILES

## 2.1. Chế độ thực thi

## MODE: TECHNICAL DESIGN ONLY

Agent chỉ được phép:

Đọc PROMPT-P1 Analysis Report.

Đọc TECH source-of-truth.

Đọc codebase nếu cần đối chiếu lại.

Tổng hợp gap thành workstream.

Tổng hợp điểm chặn thành task.

Thiết kế boundary kỹ thuật ở mức handoff.

Lập implementation sequence.

Lập evidence plan.

Lập smoke plan.

Lập report 14 mục.

## 2.2. Các hành động bị cấm

Agent không được:

Sửa file.

Tạo file.

Xóa file.

Rename file.

Format file.

Refactor code.

Tạo code.

Tạo migration.

Tạo seed.

Update database.

Sửa product data.

Sửa SKU data.

Sửa recipe/BOM.

Tạo API.

Tạo UI.

Tự quyết nghiệp vụ.

Hardcode policy.

Gọi Product Active là Sellable.

Gọi Recipe Active là Released / Production-ready nếu chưa đủ gate.

Gọi SKU Active là Sellable.

Gọi PHASE 1 Complete.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Mở Global Gate.

## 3. SOURCE-OF-TRUTH BẮT BUỘC

## 3.1. Source chính

Agent phải đọc và ưu tiên:

PROMPT-P1 Analysis Report

TECH-01 - Foundation / RBAC / Audit / Evidence / Idempotency

TECH-10 - Global Smoke / UAT / Evidence / Release Gateway

TECH-11 - Implementation Roadmap / Dev Phase Plan

TECH-12 - Phase Backlog Pack

TECH-13 - Codex / Copilot Dev Prompt Pack

PHASE 0 Validation Report

TECH Product / SKU / Recipe / Product Activation nếu đã có trong bộ TECH

Product / SKU / Recipe source-of-truth mới do owner cung cấp nếu có.

## 3.2. Quy tắc ưu tiên

Agent phải áp dụng:

TECH mới thắng code cũ.

TECH mới thắng tài liệu cũ.

PROMPT-P1 Analysis Report là đầu vào thiết kế, không phải release evidence.

Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Không dùng legacy làm nền nghiệp vụ.

Không để code cũ thắng TECH mới.

Nếu code khác TECH mới thì đưa vào conflict handling, không tự sửa.

Nếu nghiệp vụ chưa có source-of-truth thì đưa vào owner decision required, không tự suy luận.

## 4. REQUIRED INPUT

## 4.1. Đầu vào bắt buộc

Agent cần có:

PROMPT-P1 Analysis Report.

P1 Current Implementation State Matrix.

P1 Gap Analysis Matrix.

P1 Conflict Detection Matrix.

P1 điểm chặn Register.

P1 Downstream Impact.

P1 Smoke Required.

PHASE 0 Validation Report.

TECH source-of-truth.

## 4.2. Nếu thiếu đầu vào

Nếu thiếu PROMPT-P1 Analysis Report, agent phải dừng và báo:

bị chặn - PROMPT-P1 ANALYSIS REPORT REQUIRED

Nếu thiếu gap/conflict/điểm chặn matrix, agent phải báo:

bị chặn - PROMPT-P1 ANALYSIS REPORT INCOMPLETE

Nếu thiếu source-of-truth PHASE 1, agent phải báo:

bị chặn - PHASE 1 SOURCE-OF-TRUTH REQUIRED

Agent không được tự dựng nghiệp vụ Product / SKU / Recipe.

## 5. OBJECTIVE

## 5.1. Mục tiêu chính

Agent phải lập technical design handoff cho:

Product Master.

SKU Master.

Ingredient Master.

Recipe Master.

Formula Version.

Formula Kind.

BOM / Ingredient Quantity / UOM.

Product Activation.

Product Active vs Sellable Boundary.

Recipe Active vs Production-ready Boundary.

SKU Active vs Sellable Boundary.

SKU Extension Registry.

Product / SKU / Recipe Seed Governance.

Product / SKU / Recipe Smoke Plan.

Handoff sang implementation prompt sau.

## 5.2. Mục tiêu không phải

PROMPT-P1.1 không nhằm:

Viết code.

Sửa code.

Tạo API.

Tạo DB schema cuối cùng.

Tạo migration.

Tạo seed.

Tạo UI.

Tạo test code.

Kích hoạt sản phẩm.

Mở bán sản phẩm.

Gọi PHASE 1 hoàn tất.

Release.

Go-live.

## 6. WORKSTREAM STRUCTURE

Agent phải chia PHASE 1 thành các workstream sau.

## 6.1. Workstream 1A - Product Master Boundary

Mục tiêu

Thiết kế boundary cho Product Master để bảo đảm:

Product có master data rõ.

Product có public/private field boundary.

Product có lifecycle/status rõ.

Product có liên kết SKU đúng.

Product active không bị dùng sai làm sellable.

Product command sau này phải dùng Actor / RBAC / Audit / Evidence / Idempotency từ PHASE 0.

Output bắt buộc

Product Master Design Boundary.

Product Field Boundary.

Product Status Boundary.

Product Public/Private Boundary.

Product Downstream Usage Rule.

Product Task List.

Product Evidence Plan.

Product Smoke Plan.

## 6.2. Workstream 1B - SKU Master Boundary

Mục tiêu

Thiết kế boundary cho SKU Master để bảo đảm:

SKU có mã định danh rõ.

SKU có public name và internal code tách bạch.

SKU có status/lifecycle.

SKU có liên kết Product.

SKU có liên kết Recipe/BOM/Formula Version nếu cần.

SKU Active không đồng nghĩa Sellable.

SKU có thể mở rộng 40-50 SKU mà không phá logic.

Output bắt buộc

SKU Master Design Boundary.

SKU Identity Rule.

SKU Status Rule.

SKU Public/Private Boundary.

SKU Extension Rule.

SKU Task List.

SKU Evidence Plan.

SKU Smoke Plan.

## 6.3. Workstream 1C - Ingredient Master / UOM Boundary

Mục tiêu

Thiết kế boundary cho Ingredient Master và UOM để bảo đảm:

Ingredient có master data rõ.

Ingredient có UOM rõ.

Ingredient có group/category.

Ingredient không bị hardcode trong recipe.

Ingredient có thể dùng lại trong nhiều recipe/BOM.

Ingredient active không tự động làm recipe active.

UOM conversion nếu có phải có boundary rõ.

Output bắt buộc

Ingredient Master Boundary.

Ingredient Group Boundary.

UOM Rule.

Ingredient Public/Private Boundary.

Ingredient Task List.

Ingredient Evidence Plan.

Ingredient Smoke Plan.

## 6.4. Workstream 1D - Recipe Master / BOM Boundary

Mục tiêu

Thiết kế boundary cho Recipe/BOM để bảo đảm:

Recipe có version.

Recipe có status.

Recipe có ingredient lines.

BOM có quantity/UOM đầy đủ.

Recipe/BOM không bị sửa đè lịch sử.

Recipe/BOM có approval/activation guard.

Recipe/BOM có snapshot readiness cho PHASE 2 Production.

Recipe active không tự động nghĩa là batch production/released.

Output bắt buộc

Recipe Master Boundary.

BOM Line Boundary.

Quantity/UOM Boundary.

Recipe Status Rule.

Recipe Version Rule.

BOM Snapshot Readiness Rule.

Recipe Task List.

Recipe Evidence Plan.

Recipe Smoke Plan.

## 6.5. Workstream 1E - Formula Version / Formula Kind Boundary

Mục tiêu

Thiết kế boundary cho Formula Version / Formula Kind để bảo đảm:

Formula version có lifecycle rõ.

Formula kind có ý nghĩa rõ.

Pilot formula và production formula không bị lẫn.

Formula đã dùng trong production không bị sửa đè.

Formula active có guard.

Formula snapshot sẵn sàng cho production order.

Formula mở rộng được khi có version G2/G3 hoặc SKU mới.

Output bắt buộc

Formula Version Boundary.

Formula Kind Boundary.

Pilot vs Production Formula Boundary.

Formula Version Immutability Rule.

Formula Snapshot Rule.

Formula Task List.

Formula Evidence Plan.

Formula Smoke Plan.

## 6.6. Workstream 1F - Product Activation Guard

Mục tiêu

Thiết kế Product Activation Guard để bảo đảm:

Product/SKU/Recipe activation có guard.

Activation cần actor.

Activation cần permission.

Activation cần audit.

Activation cần evidence nếu policy yêu cầu.

Activation cần idempotency nếu là command có side effect.

Activation cần reason nếu thay đổi trạng thái quan trọng.

Activation không đồng nghĩa sellable.

Activation không bypass Sellable Gate của Commerce.

Output bắt buộc

Product Activation Guard Boundary.

SKU Activation Guard Boundary.

Recipe Activation Guard Boundary.

Activation Required Checks.

Activation Evidence Plan.

Activation Smoke Plan.

Activation Task List.

## 6.7. Workstream 1G - Active vs Sellable Boundary

Mục tiêu

Thiết kế boundary bắt buộc để tránh lỗi nghiêm trọng:

Product Active không đồng nghĩa Sellable.
SKU Active không đồng nghĩa Sellable.
Recipe Active không đồng nghĩa Sellable.

Sellable phải thuộc Commerce Runtime / Sellable Gate ở PHASE 3, có phụ thuộc vào:

Product/SKU active.

Listed price active.

Stock available.

Finished goods released.

Warehouse receipt confirmed.

No recall.

No sale lock.

No quality hold.

No channel suppression.

Runtime policy.

Output bắt buộc

Active vs Sellable Boundary Design.

Product Active Usage Rule.

SKU Active Usage Rule.

Recipe Active Usage Rule.

Commerce Sellable Dependency Handoff.

AI/Gateway/Ads/Live Usage Warning.

Smoke Plan.

## 6.8. Workstream 1H - SKU Extension Registry

Mục tiêu

Thiết kế cách mở rộng SKU để sau này thêm 40-50 SKU không phá hệ thống.

Phải bảo đảm:

Không hardcode danh sách SKU trong logic.

SKU mới có registry/manifest.

SKU mới có Product link.

SKU mới có Recipe/BOM/Formula Version.

SKU mới có activation guard.

SKU mới có seed/evidence nếu cần.

SKU mới không sửa đè SKU cũ.

SKU mới không tự sellable.

SKU mới có public/private boundary.

SKU mới có downstream handoff sang Product Knowledge / AI Advisor / Commerce nếu cần.

Output bắt buộc

SKU Extension Registry Boundary.

SKU Add Flow Design.

SKU Version/Activation Rule.

SKU Extension Evidence Plan.

SKU Extension Smoke Plan.

SKU Extension Task List.

## 6.9. Workstream 1I - Seed / Master Data Governance

Mục tiêu

Thiết kế governance cho seed/master data Product / SKU / Recipe.

Phải bảo đảm:

Seed idempotent.

Seed không phá dữ liệu đã có.

Seed không hardcode sai nghiệp vụ.

Seed không tự active sellable.

Seed có source-of-truth.

Seed có validation.

Seed có audit/evidence nếu áp dụng.

Seed hỗ trợ mở rộng SKU.

Seed không chứa secret.

Seed không chứa production-only data nếu chưa được phép.

Output bắt buộc

Product Seed Governance.

SKU Seed Governance.

Recipe Seed Governance.

Seed Validation Plan.

Seed Evidence Plan.

Seed Smoke Plan.

Seed Task List.

## 7. TECHNICAL DESIGN MATRICES

Agent phải tạo các ma trận sau.

## 7.1. Workstream Matrix

## 7.2. Task Breakdown Matrix

Mỗi task phải có đủ:

Task ID.

Phase.

Workstream.

Source-of-truth.

Requirement summary.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P1 điểm chặn liên quan.

Evidence required.

Smoke required.

Owner.

Report requirement.

Done Gate.

Fail Gate.

Initial status.

## 7.3. Dependency Matrix

## 7.4. Evidence Plan Matrix

## 7.5. Smoke Plan Matrix

## 8. BOUNDARY RULES BẮT BUỘC

## 8.1. Product Active Boundary

Product Active chỉ có nghĩa:

Product đã được định nghĩa hợp lệ trong Product Master.

Product có thể được dùng làm dữ liệu nền.

Product có thể được đưa vào các phase sau nếu đủ điều kiện.

Product có thể được liên kết SKU/Recipe theo rule.

Product Active không có nghĩa:

Được bán.

Có tồn kho.

Có giá đang active.

Có batch released.

Có warehouse receipt.

Không bị recall.

Không bị sale lock.

Được AI tư vấn chốt đơn.

Được Gateway public bán.

Được Ads scale.

## 8.2. SKU Active Boundary

SKU Active chỉ có nghĩa:

SKU đã được định nghĩa hợp lệ.

SKU thuộc Product hợp lệ.

SKU có thể được tham chiếu bởi recipe/BOM/commerce sau này.

SKU có lifecycle rõ.

SKU Active không có nghĩa:

SKU sellable.

SKU có tồn kho.

SKU có giá active.

SKU có batch released.

SKU được phép chốt đơn.

SKU được phép public trong mọi channel.

## 8.3. Recipe Active Boundary

Recipe Active chỉ có nghĩa:

Recipe/BOM đã hợp lệ theo rule nội bộ.

Recipe có version.

Recipe có ingredient/UOM đầy đủ.

Recipe có thể được dùng làm nguồn cho production snapshot nếu production guard cho phép.

Recipe Active không có nghĩa:

Batch đã sản xuất.

Batch QC_PASS.

Batch RELEASED.

Finished goods đã nhập kho.

SKU sellable.

Product được chốt đơn.

## 8.4. Sellable Boundary Handoff

Sellable thuộc PHASE 3 Commerce Runtime / Sellable Gate.

Sellable phải phụ thuộc tối thiểu:

Product/SKU active.

Listed price active.

Stock available.

Finished goods released.

Warehouse receipt confirmed.

No quality hold.

No recall.

No sale lock.

No channel suppression.

Runtime policy available.

PHASE 1 chỉ tạo Product/SKU/Recipe foundation.

PHASE 1 không được tự quyết Sellable.

## 9. P1 điểm chặn HANDLING

Agent phải chuyển điểm chặn từ P1 Analysis Report thành bảng xử lý.

Status được phép:

## DRAFT.

## BLOCKED_BY_OWNER_DECISION.

## BLOCKED_BY_SOURCE_OF_TRUTH.

## BLOCKED_BY_DEPENDENCY.

## READY_FOR_REVIEW.

## APPROVED_FOR_IMPLEMENTATION_PROMPT.

Không dùng:

## DONE.

PASS.

Release Readiness.

Production Readiness.

Go-live Decision.

## 10. IMPLEMENTATION SEQUENCE ĐỀ XUẤT

Agent phải đề xuất implementation sequence theo thứ tự an toàn:

## 10.1. Thứ tự khuyến nghị

Product Master Boundary.

SKU Master Boundary.

Ingredient / UOM Boundary.

Recipe / BOM Boundary.

Formula Version / Formula Kind Boundary.

Product / SKU / Recipe Status Lifecycle.

Product Activation Guard.

Active vs Sellable Boundary Enforcement.

SKU Extension Registry.

Seed / Master Data Governance.

PHASE 1 Smoke / Evidence / Report.

## 10.2. Không được đảo thứ tự nguy hiểm

Không được làm Product Activation trước khi:

Product Master rõ.

SKU Master rõ.

Recipe/BOM rõ.

Formula Version rõ.

Active vs Sellable boundary rõ.

PHASE 0 guard foundation dùng được.

Không được làm Sellable Gate trong PHASE 1.

Sellable Gate thuộc PHASE 3.

## 11. PROMPT IMPLEMENTATION SAU P1.1

Nếu P1.1 đạt Done Gate, các prompt tiếp theo nên chia nhỏ:

## 11.1. PROMPT-P1.2A

## PRODUCT MASTER / SKU MASTER / INGREDIENT UOM FOUNDATION IMPLEMENTATION HANDOFF
MODE: LIMITED IMPLEMENTATION

Phạm vi:

Product Master.

SKU Master.

Ingredient Master.

UOM foundation.

Không làm Recipe/BOM full.

Không làm Activation Guard full.

Không làm Sellable Gate.

## 11.2. PROMPT-P1.2B

## RECIPE / BOM / FORMULA VERSION IMPLEMENTATION HANDOFF
MODE: LIMITED IMPLEMENTATION

Phạm vi:

Recipe Master.

BOM lines.

Formula Version.

Formula Kind.

Snapshot readiness foundation.

Không làm production order.

Không làm sellable.

## 11.3. PROMPT-P1.2C

## PRODUCT / SKU / RECIPE ACTIVATION GUARD IMPLEMENTATION HANDOFF
MODE: LIMITED IMPLEMENTATION

Phạm vi:

Product activation guard.

SKU activation guard.

Recipe activation guard.

Actor/RBAC/Audit/Evidence/Idempotency linkage.

Active != Sellable enforcement.

Không làm Commerce Sellable Gate.

## 11.4. PROMPT-P1.2D

## SKU EXTENSION REGISTRY / SEED GOVERNANCE IMPLEMENTATION HANDOFF
MODE: LIMITED IMPLEMENTATION

Phạm vi:

SKU extension registry.

Add SKU flow foundation.

Seed idempotency.

Seed validation.

No hardcode SKU.

Không sửa AI Advisor/Product Knowledge full.

## 11.5. PROMPT-P1.2E

## PHASE 1 SMOKE / EVIDENCE / IMPLEMENTATION REPORT HANDOFF
MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY

Phạm vi:

Smoke PHASE 1.

Evidence register.

Open điểm chặn.

Handoff sang PHASE 2.

Không gọi Release Readiness.

## 12. SECURITY / GOVERNANCE GUARDRAILS

Agent phải giữ các guardrail sau:

Không hardcode danh sách SKU vào logic nghiệp vụ.

Không hardcode Product Active = Sellable.

Không hardcode Recipe Active = Production-ready.

Không hardcode SKU Active = Sellable.

Không dùng UI hidden field làm security.

Không bỏ qua RBAC PHASE 0.

Không bỏ qua Audit PHASE 0.

Không bỏ qua Evidence PHASE 0.

Không bỏ qua Idempotency PHASE 0.

Không bỏ qua High-risk Guard PHASE 0.

Không public private field.

Không tạo seed production chứa secret.

Không gọi smoke cục bộ là Global Smoke Pass.

Không gọi Documentation Complete là Production Readiness.

Không mở Global Gate.

## 13. REQUIRED REPORT FORMAT - 14 MỤC

Agent phải báo cáo đúng 14 mục.

## 13.1. Mục 1 - Tóm tắt

Ghi rõ:

Prompt đã chạy: PROMPT-P1.1.

Mode: TECHNICAL DESIGN ONLY.

Đầu vào đã dùng.

Workstream đã tạo.

Task breakdown đã tạo.

điểm chặn chính.

Implementation sequence đề xuất.

Trạng thái cuối.

Không được ghi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

## 13.2. Mục 2 - File đã sửa

Bắt buộc ghi:

Không sửa file.

Không tạo file.

Không tạo code.

Không tạo migration.

Không tạo seed.

Không update database.

Git status/git diff nếu có.

## 13.3. Mục 3 - Nguồn yêu cầu

Liệt kê:

PROMPT-P1 Analysis Report.

## TECH-01.

## TECH-10.

## TECH-11.

## TECH-12.

## TECH-13.

PHASE 0 Validation Report.

Product / SKU / Recipe TECH source-of-truth nếu có.

Legacy docs nếu có đọc, ghi rõ LEGACY_REFERENCE_ONLY.

## 13.4. Mục 4 - Evidence đã dùng

Liệt kê:

Bắt buộc ghi:

Evidence Submitted, not Evidence Accepted.

## 13.5. Mục 5 - Lệnh đã chạy

Liệt kê:

Lệnh inspect nếu có.

Lệnh search nếu có.

Lệnh git status/git diff nếu có.

Nếu không chạy lệnh, ghi lý do.

Không chạy lệnh phá hủy.

## 13.6. Mục 6 - Kết quả test

Ghi rõ:

Không viết test mới.

Không sửa test.

Có đọc/chạy test hiện có hay không.

Smoke cần chạy ở prompt implementation sau.

Không gọi test cục bộ là Global Smoke Pass.

## 13.7. Mục 7 - Kết quả backend build

Ghi rõ:

Backend build có chạy không.

Nếu không chạy, lý do.

Không sửa code để build pass.

## 13.8. Mục 8 - Kết quả frontend build

Ghi rõ:

Frontend build có chạy không.

Nếu không chạy, lý do.

Không sửa frontend.

## 13.9. Mục 9 - Kết quả cleanup process

Ghi rõ:

Có process nào mở không.

Có server/test runner cần dừng không.

Có file tạm không.

Có artifact phát sinh không.

Có cần cleanup không.

## 13.10. Mục 10 - Cập nhật Markdown

Bắt buộc ghi:

Không cập nhật Markdown.

Không sửa tài liệu trong repo.

Chỉ trả handoff trong response.

Nếu có cập nhật Markdown ngoài ý muốn thì FAIL.

## 13.11. Mục 11 - Kết quả database migration/update nếu áp dụng

Bắt buộc ghi:

Không tạo migration.

Không chạy migration.

Không update database.

Chỉ thiết kế migration need nếu cần cho prompt sau.

## 13.12. Mục 12 - Kết quả seed validation nếu áp dụng

Ghi rõ:

Không tạo seed.

Không sửa seed.

Chỉ thiết kế seed governance.

Seed validation sẽ thuộc prompt implementation sau.

## 13.13. Mục 13 - Rủi ro còn lại

Phân nhóm:

Product Master risk.

SKU Master risk.

Ingredient/UOM risk.

Recipe/BOM risk.

Formula Version risk.

Activation Guard risk.

Active vs Sellable risk.

SKU Extension risk.

Seed Governance risk.

Downstream PHASE 2/3/4 risk.

Release Gateway risk.

Global Gate risk.

## 13.14. Mục 14 - Cập nhật handoff

Phải ghi:

Workstream summary.

Task breakdown summary.

điểm chặn handling.

Evidence plan.

Smoke plan.

Implementation sequence.

Prompt tiếp theo đề xuất.

Điều kiện để được chuyển sang P1.2A.

Trạng thái cuối.

Bắt buộc ghi:

## 14. DONE GATE

PROMPT-P1.1 chỉ được xem là done khi đủ:

Đã đọc PROMPT-P1 Analysis Report.

Đã đọc TECH source-of-truth.

Đã tạo Workstream Matrix.

Đã tạo Task Breakdown Matrix.

Đã tạo Dependency Matrix.

Đã tạo Evidence Plan Matrix.

Đã tạo Smoke Plan Matrix.

Đã khóa Product Active != Sellable.

Đã khóa SKU Active != Sellable.

Đã khóa Recipe Active != Sellable / Production Released.

Đã có SKU Extension Registry design.

Đã có Seed Governance design.

Đã có implementation sequence.

Đã có prompt tiếp theo đề xuất.

Đã report đủ 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Không tạo seed.

Không gọi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

Global Gate vẫn bị chặn.

Trạng thái tối đa được phép:

## PHASE 1 TECHNICAL DESIGN HANDOFF COMPLETED

Không được gọi:

PHASE 1 Complete.

Implementation Complete.

Completion Decision.

Release Readiness.

Production Readiness.

Go-live Decision.

Gateway PASS.

## 15. FAIL GATE

PROMPT-P1.1 phải fail nếu:

Agent sửa file.

Agent tạo code.

Agent tạo migration.

Agent tạo seed.

Agent update database.

Agent không dùng PROMPT-P1 Analysis Report.

Agent không lập workstream.

Agent không lập task breakdown.

Agent không lập dependency matrix.

Agent không lập evidence plan.

Agent không lập smoke plan.

Agent gọi Product Active là Sellable.

Agent gọi SKU Active là Sellable.

Agent gọi Recipe Active là Sellable / Released.

Agent bỏ qua SKU Extension Registry.

Agent hardcode nghiệp vụ Product/SKU/Recipe.

Agent gọi Technical Design là Implementation Complete.

Agent gọi Release Readiness / Production Readiness / Go-live Decision.

Agent mở Global Gate.

Agent không report đủ 14 mục.

Nếu fail, agent phải ghi:

PROMPT-P1.1 FAIL GATE - TECHNICAL DESIGN HANDOFF NOT ACCEPTED

## 16. DOWNSTREAM HANDOFF

## 16.1. Prompt tiếp theo nếu P1.1 đạt Done Gate

Nếu PROMPT-P1.1 đạt Done Gate, prompt tiếp theo nên là:
