# 04-P1 - KHÓA KÍCH HOẠT SẢN PHẨM

Trạng thái: `ACTIVATION_GUARD_CANONICAL`  
Mode: `LIMITED IMPLEMENTATION HANDOFF`  
Mục tiêu: khóa điều kiện Product/SKU/Formula được kích hoạt mà không bị hiểu nhầm là được sản xuất, release hoặc bán.

## 1. State model

| State | Ý nghĩa | Không đồng nghĩa |
| --- | --- | --- |
| DRAFT | Đang tạo master data | Không được production/commerce consume |
| ACTIVE_BASELINE | Đủ baseline để review/seed | Không sellable |
| ACTIVE | Product/SKU/formula active nội bộ | Không batch released |
| BLOCKED | Bị chặn do thiếu evidence/conflict | Không downstream |
| INACTIVE | Ngừng dùng | Không sản xuất mới |

## 2. Guard bắt buộc

1. SKU tồn tại trong baseline 20 SKU.
2. Formula G1 approved hoặc có owner review state rõ.
3. BOM line đầy đủ từng nguyên liệu.
4. Material/UOM/packaging profile đủ.
5. Dietary claim guard pass nếu SKU/formula được gắn claim thuần chay.
6. Không có conflict source chưa normalize.
7. Evidence refs được ghi.

## 3. Downstream boundary

Phase 1 chỉ cho downstream biết SKU/formula có thể được Phase 2 xem xét. Phase 1 không cấp quyền:

1. Sản xuất thật.
2. Nhập kho thành phẩm.
3. Sellable/quote/order.
4. AI tư vấn bán.
5. Ads/Live/CRM promote.

## 4. Fail gate

Activation fail nếu:

1. SKU thiếu formula G1.
2. Formula có dòng gom nhóm.
3. SKU/formula đang mang claim thuần chay chứa line động vật.
4. Product active bị code dùng như sellable.
5. Formula active bị code dùng như released.
6. Seed tự mở vận hành hoặc production.

## 5. Activation guard input

| Input | Source | Required |
| --- | --- | --- |
| SKU baseline | P1-02 | SKU thuộc 20 SKU hoặc extension approved |
| Formula G1 | P1-03 | Header + lines + version + source refs |
| Material canonical | P1-02/P1-05 | Không alias, không duplicate usage role |
| UOM/conversion | P1-02 | Đủ cho scale/snapshot |
| Packaging profile | P1-02/P1-05 | B1/B2/B3 đủ nếu Phase 2 consume |
| Threshold policy | P1-05 | Có policy code hoặc owner pending rõ |
| Dietary claim result | P1-03 | Fail-closed nếu có line động vật |
| Owner approval | Governance/evidence | Bắt buộc trước production-active |
| Evidence refs | P1-06 | Submitted/accepted tách rõ |

## 6. Activation result model

| Field | Ý nghĩa |
| --- | --- |
| `guard_result` | PASS/BLOCK/WARN_OWNER_REVIEW |
| `activation_state` | DRAFT/ACTIVE_BASELINE/ACTIVE/BLOCKED/INACTIVE |
| `phase2_consumable` | true chỉ khi Phase 2 được phép đọc projection |
| `sellable_allowed` | Luôn false trong Phase 1 |
| `production_ready_claim` | Luôn false nếu chưa có owner sign-off/evidence accepted |
| `blocked_reason_codes` | Mã blocker cụ thể |
| `owner_decision_refs` | Ref approval/pending |
| `evaluated_at/evaluated_by` | Audit |

## 7. State transition

```text
DRAFT -> ACTIVE_BASELINE -> ACTIVE
```

Nhánh chặn:

```text
DRAFT/ACTIVE_BASELINE/ACTIVE -> BLOCKED -> ACTIVE_BASELINE hoặc INACTIVE
```

`ACTIVE_BASELINE` dùng cho review/seed/projection. `ACTIVE` chỉ là active nội bộ master data, không có nghĩa sản xuất/release/bán.

## 8. Block reason codes

| Code | Khi nào |
| --- | --- |
| `SKU_MISSING` | SKU không tồn tại trong baseline/extension |
| `FORMULA_G1_MISSING` | Thiếu formula G1 |
| `FORMULA_NOT_APPROVED` | Formula chưa owner-approved |
| `FORMULA_LINE_GROUPED` | Có dòng gom nhóm không material cụ thể |
| `MATERIAL_NOT_CANONICAL` | Formula line trỏ alias/chưa canonical |
| `UOM_CONVERSION_MISSING` | Scale cần conversion nhưng chưa có |
| `DIETARY_CLAIM_BLOCKED` | Claim thuần chay có line động vật |
| `PACKAGING_PROFILE_MISSING` | Thiếu B1/B2/B3/output profile |
| `THRESHOLD_POLICY_MISSING` | Thiếu threshold/material policy |
| `EVIDENCE_MISSING` | Thiếu evidence refs |
| `DOWNSTREAM_SCOPE_VIOLATION` | Code dùng activation như sellable/released |

## 9. Projection boundary

Phase 1 chỉ export:

1. Product/SKU master projection.
2. Formula/BOM projection.
3. Material/UOM projection.
4. Packaging/threshold projection.
5. Activation guard projection.

Phase 1 không export raw lot, batch, release, finished goods, sale lock hoặc MISA synced state.
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 04-P1-2C-ACTIVATION-GUARD.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P1.2C - ACTIVATION GUARD

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-02 Product Activation Guard.
- PACK-01 Operational Sellable boundary.
- TECH-02 Product Activation.
- TECH-03 Operational Core boundary.

## Noi Dung Rewrite

## PROMPT-P1.2C - PRODUCT / SKU / RECIPE ACTIVATION GUARD IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION

## FILE

## 04-P1-2C-ACTIVATION-GUARD.docx

## PHASE

## PHASE-01 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## MODE

## MODE: LIMITED IMPLEMENTATION

## PROMPT TIẾP THEO

## PROMPT-P1.2D - SKU EXTENSION REGISTRY / SEED GOVERNANCE IMPLEMENTATION HANDOFF

Global Gate

bị chặn

## PROMPT-P1.2C - PRODUCT / SKU / RECIPE ACTIVATION GUARD IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION

## 1. PHASE MARKER

## 1.1. Phase hiện tại

## PHASE-01 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## 1.2. Prompt hiện tại

## PROMPT-P1.2C - PRODUCT / SKU / RECIPE ACTIVATION GUARD IMPLEMENTATION HANDOFF

## 1.3. Mode

## MODE: LIMITED IMPLEMENTATION

## 1.4. Prompt tiếp theo

## PROMPT-P1.2D - SKU EXTENSION REGISTRY / SEED GOVERNANCE IMPLEMENTATION HANDOFF

## 1.5. Điều kiện chuyển phase

Chưa được chuyển sang PHASE 2 cho đến khi hoàn tất toàn bộ P1.2E - PHASE 1 Smoke / Evidence / Implementation Report và owner/dev lead cho phép viết PROMPT-P2 - Operational Core Analysis Only.

## 2. HEADER

Tên prompt: PROMPT-P1.2C - PRODUCT / SKU / RECIPE ACTIVATION GUARD IMPLEMENTATION HANDOFF.

Prompt này là bước limited implementation thứ ba của PHASE 1, sau P1.2A và P1.2B.

Mục tiêu là triển khai giới hạn Product Activation Guard, SKU Activation Guard, Recipe Activation Guard và Active != Sellable enforcement.

Không triển khai SKU Extension Registry, Seed Governance, Sellable Gate, Commerce Runtime, Operational Core, AI Advisor, Gateway, Ads, MC AI Live, IVR hoặc Release Gateway.

## 3. SOURCE-OF-TRUTH BẮT BUỘC

Agent phải đọc PROMPT-P1 Analysis Report, PROMPT-P1.1 Technical Design Handoff, các Implementation Report liền trước trong PHASE 1 và PHASE 0 Validation Report.

Agent phải đọc TECH-01, TECH-10, TECH-11, TECH-12, TECH-13 và mọi source-of-truth Product / SKU / Recipe / Activation / Seed có liên quan.

TECH mới thắng code cũ. TECH mới thắng tài liệu cũ. Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY. Legacy chỉ là LEGACY_REFERENCE_ONLY.

Nếu phát hiện conflict ngoài scope prompt hiện tại thì báo cáo trong handoff, không tự sửa lan.

## 4. SCOPE IN

Product Activation Guard foundation: kiểm Product tồn tại, không archived/bị chặn, required fields đầy đủ, actor hợp lệ, permission hợp lệ, reason/evidence/idempotency/audit nếu policy yêu cầu.

SKU Activation Guard foundation: kiểm SKU tồn tại, linked Product hợp lệ, parent Product không bị chặn/archived, SKU code unique, actor/permission/reason/evidence/idempotency/audit nếu policy yêu cầu.

Recipe Activation Guard foundation: kiểm Recipe tồn tại, linked SKU hợp lệ, formula version/kind hợp lệ, BOM lines đầy đủ Ingredient/Quantity/UOM, branch rule của formula kind, snapshot readiness.

Active vs Sellable enforcement: Product Active, SKU Active, Recipe Active đều không được xem là Sellable, không được xem là Batch Released hoặc Finished Goods Available.

## 5. SCOPE OUT

Không triển khai SKU Extension Registry đầy đủ.

Không triển khai Seed Governance đầy đủ.

Không triển khai Sellable Gate / Commerce Runtime.

Không triển khai Production Order / Operational Core.

Không sửa AI Advisor / Facebook Gateway / Ads / MC AI Live / IVR / Release Gateway.

## 6. ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file liên quan trực tiếp tới Product/SKU/Recipe activation guard/service/policy, status transition validation, activation command foundation nếu đã có pattern, permission/audit/evidence/idempotency hook, reason validation, test/smoke activation guard.

Không đoán file path. Nếu boundary không rõ, báo bị chặn - FILE BOUNDARY UNCLEAR.

## 7. IMPLEMENTATION REQUIREMENTS

Activation Guard Context tối thiểu: targetType, targetId, currentStatus, targetStatus, actorId, actorType, correlationId, requiredPermission, reasonCode, evidenceRefs, idempotencyKey, validationResult, guardResult, safe metadata.

Guard Result tối thiểu: ALLOW, DENY, BLOCK, NEEDS_PERMISSION, NEEDS_EVIDENCE, EVIDENCE_NOT_ACCEPTED, NEEDS_IDEMPOTENCY_KEY, NEEDS_REASON, INVALID_STATE, INVALID_MASTER_DATA, RUNTIME_UNAVAILABLE, UNKNOWN.

Không dùng UNKNOWN để allow.

Failure reason tối thiểu: ACTOR_MISSING, ACTOR_UNKNOWN, PERMISSION_MISSING, PERMISSION_DENIED, PRODUCT_NOT_FOUND, SKU_PRODUCT_INVALID, RECIPE_VERSION_MISSING, BOM_LINE_MISSING, UOM_INVALID, QUANTITY_INVALID, EVIDENCE_NOT_ACCEPTED, IDEMPOTENCY_CONFLICT, ACTIVE_NOT_SELLABLE_BOUNDARY_VIOLATION.

## 8. DATABASE / MIGRATION / SEED RULE

Mặc định không cần migration lớn nếu Product/SKU/Recipe status đã có từ P1.2A/P1.2B.

Chỉ tạo migration nếu P1.1 đã xác định cần, migration chỉ phục vụ Activation Guard foundation và không tạo Sellable Gate / Commerce / Operational table.

Không update database thật. Không chạy migration production. Không seed production data.

Seed/dev fixture nếu có không được tự set sellable, released hoặc production-ready; không chứa secret; phải idempotent nếu có seed validation.

## 9. TEST / SMOKE REQUIREMENTS

Smoke tối thiểu: Product activation hợp lệ; Product activation thiếu permission bị deny; Product Active không Sellable; SKU activation linked Product hợp lệ; SKU thiếu Product hợp lệ bị deny; SKU Active không Sellable; Recipe activation có BOM đầy đủ; Recipe thiếu BOM/UOM bị deny; Recipe Active không Sellable/Batch Released; Evidence SUBMITTED không pass nếu require ACCEPTED; retry same idempotency key/payload không tạo side effect lần hai nếu hook có; runtime unavailable fail-safe; activation success/deny có audit event nếu hook hỗ trợ.

Không viết test cho Sellable Gate, Commerce, Production Order, Inventory, Warehouse, QC/Release, AI Advisor, Gateway, Ads/Live/IVR, Release Gateway.

## 10. SECURITY / GOVERNANCE GUARDRAILS

Không hardcode Product Active = Sellable; SKU Active = Sellable; Recipe Active = Sellable; Recipe Active = Batch Released.

Không cho UNKNOWN actor activation. Không cho missing permission activation. Không cho SUBMITTED evidence pass ACCEPTED requirement. Không bỏ qua idempotency conflict.

Không mở Global Gate. Không gọi Activation Guard là Sellable Ready hoặc PHASE 1 Complete.

## 11. EXECUTION STEPS

Đọc PROMPT-P1, P1.1, P1.2A, P1.2B, PHASE 0 Validation Report và TECH source-of-truth.

Inspect codebase thật, xác định file thuộc scope và file không được sửa.

Triển khai giới hạn Product/SKU/Recipe Activation Guard foundation và Active != Sellable enforcement.

Thêm/chỉnh test/smoke trong scope, chạy build/test/lint, migration/seed validation nếu có, git status/git diff.

## 12. REQUIRED REPORT FORMAT - 14 MỤC

Agent phải báo cáo đúng 14 mục: 1. Tóm tắt; 2. File đã sửa; 3. Nguồn yêu cầu; 4. Evidence đã dùng; 5. Lệnh đã chạy; 6. Kết quả test; 7. Kết quả backend build; 8. Kết quả frontend build; 9. Kết quả cleanup process; 10. Cập nhật Markdown; 11. Kết quả database migration/update nếu áp dụng; 12. Kết quả seed validation nếu áp dụng; 13. Rủi ro còn lại; 14. Cập nhật handoff.

Mọi evidence trong report chỉ là Evidence Submitted, not Evidence Accepted.

Không gọi test pass là Global Smoke Pass. Không gọi prompt done là Release Readiness / Production Readiness / Go-live Decision.

## 13. DONE GATE

Đúng scope P1.2C; có Product/SKU/Recipe Activation Guard foundation; có Actor/RBAC/Audit/Evidence/Idempotency linkage nếu foundation hỗ trợ; Product/SKU/Recipe Active != Sellable; Recipe Active != Batch Released; không triển khai SKU Extension / Seed Governance / Sellable Gate / Commerce / Operational Core; có test/build hoặc lý do không chạy; report đủ 14 mục; Global Gate bị chặn.

## 14. FAIL GATE

Fail nếu sửa ngoài scope, hardcode Active=Sellable, cho UNKNOWN actor activation, missing permission vẫn activation được, SUBMITTED evidence pass ACCEPTED requirement, bỏ qua idempotency conflict, tự triển khai Sellable Gate/Commerce/Operational Core, mở Gateway hoặc gọi Release Readiness / Production Readiness / Go-live Decision.

## 15. DOWNSTREAM HANDOFF

Nếu P1.2C đạt Done Gate, prompt tiếp theo là PROMPT-P1.2D - SKU Extension Registry / Seed Governance Implementation Handoff.

Không tự chuyển prompt. Chỉ owner/dev lead mới quyết định prompt tiếp theo.

## 16. PROMPT COPY GIAO DEV / CODEX / COPILOT

BẮT ĐẦU PROMPT: Bạn được phép sửa code chỉ trong phạm vi Product / SKU / Recipe Activation Guard foundation. Không sửa lan, không tự đổi nghiệp vụ, không hardcode policy, không gọi Active là Sellable, không mở Global Gate, không gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision. Hãy đọc source-of-truth, kiểm scope, triển khai giới hạn, chạy test/build, báo cáo 14 mục. KẾT THÚC PROMPT.
