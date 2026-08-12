# 00-P1 - PHÂN TÍCH NGUỒN SỰ THẬT VÀ PHẠM VI

Trạng thái: `SOURCE_ABSORBED_CANONICAL`  
Mode: `ANALYSIS ONLY`  
Phase: `PHASE 1 - Product / SKU / Recipe / Formula / Activation`  
Mục tiêu: khóa nguồn sự thật Phase 1 sau khi hấp thụ đủ 5 file nguồn cũ. File này là điểm đọc đầu tiên của Phase 1; không dùng tài liệu tổng hợp trung gian.

## 1. Phạm vi chuẩn

Phase 1 chỉ quản trị nền Product/SKU/Ingredient/UOM/Recipe/BOM/Formula Version/Product Activation. Phase 1 tạo dữ liệu sạch để Phase 2 sản xuất và kho consume, nhưng không tạo raw lot, không xuất kho, không release batch, không mở bán.

Trong phạm vi:

1. 20 SKU canonical, tên sản phẩm, nhóm, phân loại và mã formula G1.
2. Ingredient/material canonical, alias, UOM, conversion, density policy nếu cần.
3. BOM/Recipe map theo SKU và Formula Version.
4. G0/pilot/G1/version governance.
5. Anchor rice scaling và output profile mẻ 400.
6. Material/packaging group A1/A2/A3/B1/B2/B3 và threshold policy.
7. Product Activation Guard và downstream consumption boundary.
8. Smoke/evidence để chứng minh dữ liệu đủ cho Phase 2.

Ngoài phạm vi:

1. Raw receipt, raw lot, incoming QC, readiness, material issue.
2. Production order runtime, batch execution, warehouse receipt.
3. QR/public trace runtime, recall execution, sale lock runtime.
4. Quote/order/payment/shipping/invoice/verified revenue/MISA sync thật.
5. AI/Facebook/Live/Ads/CRM runtime.

## 2. 5 file nguồn cũ đã được nhập

| Nguồn cũ | Nội dung đã đưa vào Phase 1 | File Phase 1 chính |
| --- | --- | --- |
| BỘ KHÓA 5 BƯỚC | BOM map, operational config, version formula, trace/recall boundary, màn hình/route/action | 00, 01, 03, 04, 06 |
| CÔNG THỨC VẬN HÀNH 20 SKU MỚI | Toàn bộ 20 SKU, formula G1, bảng định lượng từng nguyên liệu | 02, 03, 06 |
| DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ | A1/A2/A3, B1/B2/B3, threshold, dynamic resolver, disposal/write-off | 02, 05, 06 |
| BẢNG PHIẾU LỆNH SẢN XUẤT | Các field Phase 1 phải cung cấp cho lệnh sản xuất và phiếu tự sinh | 01, 03, 04, 06 |
| PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN | Click chọn/tự hiện/nhập tay, source dữ liệu công thức, print/accounting boundary | 01, 03, 04, 05, 06 |

## 3. Baseline 20 SKU

| SKU | Tên sản phẩm | Nhóm sản phẩm | Phân loại chuẩn | Formula G1 |
| --- | --- | --- | --- | --- |
| A1/CS/DM/HS | Cháo Sâm - Diêm mạch & Hạt sen | Cháo sâm theo mùa | Không thuần chay - khóa vegan claim do G1 có lòng trắng trứng | FML-A1-G1 |
| A2/CS/BASA | Cháo Sâm - Cá Basa | Cháo sâm theo mùa | Mặn | FML-A2-G1 |
| A3/CS/CAHOI | Cháo Sâm - Cá hồi | Cháo sâm theo mùa | Mặn | FML-A3-G1 |
| A4/CS/LUON | Cháo Sâm - Lươn đồng | Cháo sâm theo mùa | Mặn | FML-A4-G1 |
| A5/CS/CUU | Cháo Sâm - Thịt cừu & Táo tàu | Cháo sâm theo mùa | Mặn | FML-A5-G1 |
| B1/CS/RM/ĐX | Cháo Sâm - Rau má & Đậu xanh | Cháo sâm chức năng | Không thuần chay - khóa vegan claim do G1 có lòng trắng trứng | FML-B1-G1 |
| B2/CS/DHA | Cháo Sâm - DHA Não bộ | Cháo sâm chức năng | Mặn | FML-B2-G1 |
| B3/CS/CACOM | Cháo Sâm - Cá cơm & Vừng | Cháo sâm chức năng | Mặn | FML-B3-G1 |
| B4/CS/COLAGEN | Cháo Sâm - Thịt heo & Da heo | Cháo sâm chức năng | Mặn | FML-B4-G1 |
| B5/CS/SINHLUC | Cháo Sâm - Hàu biển | Cháo sâm chức năng | Mặn | FML-B5-G1 |
| B6/CS/GAAC | Cháo Sâm - Gà ác | Cháo sâm chức năng | Mặn | FML-B6-G1 |
| C1/CS/BAONGU | Cháo Sâm - Bào ngư | Cháo sâm bổ dưỡng | Mặn | FML-C1-G1 |
| C2/CS/DONGTRUNG | Cháo Sâm - Đông trùng hạ thảo | Cháo sâm bổ dưỡng | Không thuần chay - khóa vegan claim do G1 có lòng trắng trứng | FML-C2-G1 |
| C3/CS/NAMDONGCO | Cháo Sâm - Nấm đông cô | Cháo sâm bổ dưỡng | Không thuần chay - khóa vegan claim do G1 có lòng trắng trứng | FML-C3-G1 |
| C4/CS/CUABIEN | Cháo Sâm - Cua biển | Cháo sâm bổ dưỡng | Mặn | FML-C4-G1 |
| C5/CS/CANGU | Cháo Sâm - Cá ngừ | Cháo sâm bổ dưỡng | Mặn | FML-C5-G1 |
| C6/CS/TOM/RONGBIEN | Cháo Sâm - Tôm & Rong biển | Cháo sâm bổ dưỡng | Mặn | FML-C6-G1 |
| C7/CS/THITGA | Cháo Sâm - Thịt gà | Cháo sâm bổ dưỡng | Mặn | FML-C7-G1 |
| C8/CS/THITHEO | Cháo Sâm - Thịt heo | Cháo sâm bổ dưỡng | Mặn | FML-C8-G1 |
| C9/CS/THITBO | Cháo Sâm - Thịt bò | Cháo sâm bổ dưỡng | Mặn | FML-C9-G1 |

## 4. Quy tắc xử lý conflict nguồn

Không để conflict nguồn lan sang implementation. Khi nguồn cũ có dòng dùng chung hoặc thuật ngữ chưa chuẩn, Phase 1 chuẩn hóa như sau:

1. Không SKU nào được claim `Thuần chay` nếu normalized formula còn dòng động vật. A1/CS/DM/HS, B1/CS/RM/ĐX, C2/CS/DONGTRUNG và C3/CS/NAMDONGCO hiện khóa vegan claim theo nguồn G1 vì có `lòng trắng trứng`; chỉ được mở lại claim khi có formula/broth version mới sạch động vật và owner approval.
2. `G0`/pilot là thử nghiệm, không dùng production thật. `G1` là baseline vận hành chỉ sau owner approval.
3. `Product Active` không đồng nghĩa `Sellable`.
4. `Formula Active` không đồng nghĩa `Batch Released`.
5. Buffer +5% Nhóm A và +7% Nhóm B là buffer MRP/kế hoạch, không sửa số lượng công thức sản xuất.
6. Material canonical là một bản ghi; usage_role phân biệt A1/A2/A3 thay vì tạo trùng material.
7. Sâm Savigin là `Company Source / strategic reserve`, không phải supplier purchase requirement mặc định.
8. Tất cả field có số liệu chưa owner-confirm phải chạy qua owner approval trước seed/runtime.

## 5. Done gate phân tích

Phase 1 Analysis đạt khi report sau này chứng minh đủ:

1. Đã inspect đầy đủ Product/SKU/Ingredient/UOM/Recipe/Formula/Activation hiện có.
2. Đã map đủ 5 file nguồn cũ vào file con Phase 1/2.
3. Đã ghi gap matrix cho SKU, material, UOM, formula, packaging, activation.
4. Đã ghi conflict đã normalize hoặc owner decision required.
5. Không gọi `Complete`, `Release Ready`, `Production Ready`, `Go-live`.

## 6. File con Phase 1 sau rename

| Thứ tự | File tiếng Việt | Vai trò |
| --- | --- | --- |
| 00 | `00-PHÂN TÍCH NGUỒN SỰ THẬT VÀ PHẠM VI.md` | Phân tích nguồn, phạm vi, conflict normalization |
| 01 | `01-THIẾT KẾ KỸ THUẬT SẢN PHẨM SKU CÔNG THỨC.md` | Thiết kế kỹ thuật Phase 1 |
| 02 | `02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md` | Product/SKU/material/UOM master |
| 03 | `03-CÔNG THỨC BOM PHIÊN BẢN G1.md` | Recipe/BOM/Formula G1 và nguồn công thức |
| 04 | `04-KHÓA KÍCH HOẠT SẢN PHẨM.md` | Activation guard và downstream boundary |
| 05 | `05-QUẢN TRỊ DỮ LIỆU KHỞI TẠO MỞ RỘNG SKU VÀ VẬT TƯ.md` | Seed/extension/material/packaging/threshold governance |
| 06 | `06-KIỂM THỬ KHÓI BẰNG CHỨNG VÀ NGHIỆM THU.md` | Smoke/evidence/coverage |
| 07 | `07-BẢNG PHIẾU LỆNH SẢN XUẤT.md` | Payload nguồn lệnh sản xuất đã nhập trực tiếp, dùng để đối chiếu Phase 2 |
| 08 | `08-BỘ KHÓA 5 BƯỚC.md` | Payload nguồn bộ khóa/rule vận hành đã nhập trực tiếp |
| 09 | `09-CÔNG THỨC VẬN HÀNH 20 SKU MỚI.md` | Payload nguồn công thức 20 SKU đã nhập trực tiếp |
| 10 | `10-DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VÀ VẬT TƯ.md` | Payload nguồn phân nhóm nguyên liệu/vật tư đã nhập trực tiếp |
| 11 | `11-PHIẾU TỰ SINH IN KẾ TOÁN VÀ HOẠCH TOÁN.md` | Payload nguồn phiếu tự sinh/in/kế toán/hoạch toán đã nhập trực tiếp |

## 7. Nơi lưu nguồn gốc sau khi bỏ 5 file rời

| File nguồn cũ | File Phase 1/2 giữ payload nguyên văn | Rule canonical đã rút ra |
| --- | --- | --- |
| `BẢNG PHIẾU LỆNH SẢN XUẤT.md` | `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | Phase 1 phải cung cấp SKU/formula/material/UOM đủ để lệnh sản xuất tự hiện công thức |
| `BỘ KHÓA 5 BƯỚC.md` | `03-CÔNG THỨC BOM PHIÊN BẢN G1.md`, `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | BOM map, version, không chọn tay nguyên liệu, trace/recall boundary |
| `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` | `02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md`, `03-CÔNG THỨC BOM PHIÊN BẢN G1.md`, `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | 20 SKU G1, định lượng từng nguyên liệu, ratio, sơ chế |
| `DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ.md` | `02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md`, `05-QUẢN TRỊ DỮ LIỆU KHỞI TẠO MỞ RỘNG SKU VÀ VẬT TƯ.md`, `02-NGUỒN NGUYÊN LIỆU LÔ QC SẴN SÀNG SẢN XUẤT.md` | Material/packaging groups, thresholds, procurement suppression, disposal |
| `PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md` | `01-THIẾT KẾ KỸ THUẬT VẬN HÀNH LÕI.md`, `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | Ranh giới click chọn/tự hiện/nhập tay, print, kế toán/MISA handoff |

## 8. Luồng dữ liệu Phase 1 cấp cho Phase 2

```text
Product Master -> SKU Master -> Material Master -> UOM/Conversion
  -> Recipe Group -> Formula G1 Header -> Formula G1 Lines
  -> Packaging Profile -> Threshold Policy -> Activation Guard
  -> Phase 2 Production BOM Snapshot
```

Phase 1 không chạy raw lot, không issue, không batch, không release. Phase 1 chỉ tạo dữ liệu nguồn sạch và gate để Phase 2 consume.

## 9. Owner decision cần giữ lại

| Decision | Tác động | Nếu chưa có owner answer |
| --- | --- | --- |
| Formula G1 approval/effective date | Có được dùng làm operational baseline không | Giữ `OWNER_REVIEW_REQUIRED`, không active production |
| Vegan/dietary claim | Có được public claim thuần chay không | Suppress claim, fail-closed |
| Output profile mẻ 400 hoặc profile mới | Tính packaging và Phase 2 snapshot | Dùng mẻ 400 nguồn cũ làm baseline pilot, không hardcode production |
| UOM/density conversion | Scale công thức, MRP, issue | Block scale nếu thiếu conversion |
| Threshold policy owner override | Loại khỏi phiếu mua/thu mua | Dùng policy baseline nhưng không gọi final |
| Public SKU naming/channel exposure | Hiển thị ngoài hệ thống | Không public internal SKU code |

## 10. Conflict table Phase 1

| Conflict nguồn | Rule Phase 1 |
| --- | --- |
| G0 có trong tài liệu cũ | G0 chỉ là history/research/pilot baseline, không tạo production order/material issue/snapshot |
| G1 có dòng động vật nhưng SKU từng được gọi vegan | Khóa vegan claim, chỉ mở lại bằng formula version sạch động vật + owner approval |
| Material dùng nhiều vai trò | Một material canonical, nhiều `usage_roles`; không duplicate vì A1/A2/A3 |
| Tên nguyên liệu có alias/lỗi chính tả | Alias map về material canonical, không dùng alias làm source chính |
| Buffer +5%/+7% bị hiểu là công thức | Buffer chỉ là MRP/threshold planning, không sửa recipe quantity |
| Product active bị hiểu là sellable | Product active chỉ cho Phase 2 xem xét, không mở bán |
| Formula active bị hiểu là released | Formula active không đồng nghĩa batch QC/release |

## 11. Done gate Phase 1

Phase 1 chỉ handoff-ready khi:

1. 20 SKU có product/SKU/formula_id G1 không trùng.
2. Mọi material trong công thức có material canonical hoặc owner decision pending rõ.
3. Mọi UOM cần scale có conversion/density policy.
4. Formula G1 có header, line, group, ratio, UOM, source ref.
5. Packaging B1/B2/B3 có output/threshold policy.
6. Activation guard fail-closed, không sellable.
7. Evidence ID trong P1-06 có expected pass/block.
8. Không còn cần mở 5 file nguồn cũ để hiểu rule chính.

## 12. Quy tắc đọc canonical cho Phase 1

Khi đưa bộ Phase 1 cho người khác hoặc AI/ChatGPT đọc, dùng quy tắc sau để tránh hiểu nhầm từ phần lịch sử:

1. Chỉ phần từ đầu file đến trước dòng `PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE` là canonical hiện hành.
2. Phụ lục cũ được giữ để không mất dấu rule/handoff cũ, nhưng không được dùng để phủ định phần canonical đã chuẩn hóa.
3. Nếu phụ lục cũ có từ như `Complete`, `Ready`, `Accepted`, `Go-live`, `Production Ready`, `Sellable Ready` thì phải hiểu đó là ngôn ngữ lịch sử hoặc ví dụ fail/done của prompt cũ; Phase 1 hiện hành vẫn không được tự gọi production-ready, go-live, sellable hoặc batch-released.
4. Nếu phụ lục cũ có tên file/nhãn cũ dạng không dấu, tên tiếng Việt ở mục 6 là tên file chính hiện hành.
5. `BẢNG PHIẾU LỆNH SẢN XUẤT.md` và `PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md` là nguồn yêu cầu cho projection/boundary của Phase 1; runtime phiếu, print, kế toán, MISA và payload vận hành đầy đủ thuộc Phase 2, nên không được kéo runtime đó vào Phase 1.
6. Verdict Phase 1 hiện hành: `SOURCE_ABSORBED_CANONICAL`, `HANDOFF_READY_FOR_PHASE_2_CONSUMPTION`, nhưng `NOT_PRODUCTION_READY`, `NOT_SELLABLE_READY`, `NOT_GO_LIVE`.
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 00-P1-ANALYSIS-ONLY.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P1 - ANALYSIS ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- MASTER registry va source-of-truth.
- PACK-02 Product Master / SKU / Recipe / Activation.
- TECH-02 Product / SKU / Ingredient / Recipe / Formula / Product Activation.
- PACK-10 Evidence / Smoke / Completion Gateway.

## Noi Dung Rewrite

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS HANDOFF

## MODE: ANALYSIS ONLY

## FILE

## 00-P1-ANALYSIS-ONLY.docx

## PHASE

## PHASE-01 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## MODE

## MODE: ANALYSIS ONLY

## PROMPT TIẾP THEO

## PROMPT-P1.1 - TECHNICAL DESIGN ONLY

Global Gate

bị chặn

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS HANDOFF

## MODE: ANALYSIS ONLY

## 1. HEADER

## 1.1. Tên prompt

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION ANALYSIS HANDOFF

## 1.2. Vị trí trong roadmap

PROMPT-P1 là prompt mở đầu cho:

PHASE 1 - Product / SKU / Recipe / Product Activation

PROMPT-P1 chỉ được bắt đầu sau khi PHASE 0 đã có:

Foundation Boundary.

Actor Context.

Correlation ID.

RBAC / Permission Enforcement.

Audit Trail / Append-only Event Log.

Evidence Registry.

Idempotency Foundation.

High-risk Command Guard / Admin Override Foundation.

PHASE 0 Smoke / Evidence / Implementation Report đã được submit cho owner/reviewer.

PROMPT-P1 không phải implementation prompt.

PROMPT-P1 là bước analysis only để phân tích domain Product / SKU / Recipe / Product Activation trước khi cho phép sửa code.

## 1.3. Mục tiêu

Mục tiêu của PROMPT-P1 là yêu cầu dev/Codex/Copilot:

Đọc source-of-truth.

Inspect codebase thật.

Xác định current implementation state của Product / SKU / Recipe / Product Activation.

Đối chiếu code hiện tại với TECH source-of-truth.

Tìm gap, conflict, điểm chặn.

Tìm rủi ro về Product Active / Sellable.

Tìm rủi ro về SKU / Recipe / Formula Version / BOM.

Tìm rủi ro về Product Activation Guard.

Tìm rủi ro khi mở rộng SKU sau này.

Lập báo cáo phân tích đủ 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Không gọi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

## 2. MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

## 2.1. Chế độ thực thi

## MODE: ANALYSIS ONLY

Agent chỉ được phép:

Đọc tài liệu.

Đọc report PHASE 0.

Inspect codebase thật.

Tìm kiếm file.

Đọc module Product / SKU / Recipe nếu có.

Đọc migration/schema hiện có nếu có.

Đọc seed hiện có nếu có.

Đọc test hiện có nếu có.

Đọc configuration hiện có nếu có.

Lập current implementation state.

Lập gap matrix.

Lập conflict matrix.

Lập P1 điểm chặn register.

Lập downstream impact.

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

Sửa activation logic.

Tự tạo API.

Tự tạo UI.

Tự đổi nghiệp vụ.

Hardcode policy.

Gọi Product Active là Sellable.

Gọi PHASE 1 Complete.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Mở Global Gate.

## 3. SOURCE-OF-TRUTH BẮT BUỘC

## 3.1. Source chính

Agent phải đọc và ưu tiên:

TECH-01 - Foundation / RBAC / Audit / Evidence / Idempotency

TECH-10 - Global Smoke / UAT / Evidence / Release Gateway

TECH-11 - Implementation Roadmap / Dev Phase Plan

TECH-12 - Phase Backlog Pack

TECH-13 - Codex / Copilot Dev Prompt Pack

PROMPT-P0.2G PHASE 0 Validation / Smoke / Evidence Report

TECH liên quan PHASE 1 - Product / SKU / Recipe / Product Activation nếu đã có trong bộ TECH

Các tài liệu Product / SKU / Recipe mới thuộc TECH source-of-truth nếu đã được owner cung cấp.

## 3.2. Quy tắc ưu tiên

Agent phải áp dụng:

TECH mới thắng code cũ.

TECH mới thắng tài liệu cũ.

Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

PHASE 0 evidence submitted chưa phải evidence accepted.

Không dùng legacy làm nền nghiệp vụ.

Không để code cũ thắng TECH mới.

Nếu code hiện tại khác TECH mới thì báo conflict, không sửa.

Nếu tài liệu cũ khác TECH mới thì báo legacy conflict, không dùng làm nền.

Nếu nghiệp vụ chưa có source-of-truth thì không tự suy luận.

## 4. OBJECTIVE

## 4.1. Mục tiêu phân tích PHASE 1

Agent phải phân tích các domain sau:

Product Master.

SKU Master.

Product Category / Group.

Ingredient Master.

Recipe Master.

Formula Version.

Formula Kind.

BOM / Ingredient Quantity / UOM.

Product Activation.

Product Active Guard.

Product Active vs Sellable boundary.

Recipe Snapshot Readiness.

SKU Expansion / Extension Registry.

Product Data Governance.

Product Seed / Master Data nếu có.

Product / SKU / Recipe smoke hiện có nếu có.

## 4.2. Mục tiêu không phải

PROMPT-P1 không nhằm:

Viết code Product module.

Viết code SKU module.

Viết code Recipe module.

Viết API.

Viết DB schema.

Viết UI.

Tạo migration.

Tạo seed.

Sửa dữ liệu sản phẩm.

Kích hoạt sản phẩm.

Mở bán sản phẩm.

Gọi product là sellable.

Release.

Go-live.

## 5. REQUIRED INPUT

## 5.1. Đầu vào bắt buộc

Agent cần có:

TECH source-of-truth.

PHASE 0 Validation Report.

Codebase thật.

Quyền đọc source code.

Quyền inspect repo.

Quyền đọc migration/test/seed hiện có nếu có.

Danh sách file/module Product / SKU / Recipe hiện có nếu có.

## 5.2. Nếu thiếu đầu vào

Nếu thiếu PHASE 0 report, agent phải báo:

bị chặn - PHASE 0 VALIDATION REPORT REQUIRED

Nếu thiếu codebase thật, agent phải báo:

bị chặn - CODEBASE INSPECTION REQUIRED

Nếu thiếu source-of-truth PHASE 1, agent phải báo:

bị chặn - PHASE 1 SOURCE-OF-TRUTH REQUIRED

Agent không được tự dựng nghiệp vụ Product / SKU / Recipe.

## 6. SCOPE IN

## 6.1. Product Master Analysis

Agent phải phân tích:

Product entity/model hiện có không.

Product public name.

Product internal name.

Product group/category.

Product status.

Product active flag.

Product lifecycle.

Product public/private field boundary.

Product owner/source-of-truth.

Product có liên kết SKU không.

Product có liên kết recipe không.

Product active có bị hiểu sai thành sellable không.

Product có audit/actor/correlation không nếu có command.

Product có seed/master data không.

## 6.2. SKU Master Analysis

Agent phải phân tích:

SKU entity/model hiện có không.

SKU code.

SKU public name.

SKU internal code.

SKU group.

SKU type.

SKU status.

SKU active.

SKU versioning nếu có.

SKU public/private field boundary.

SKU có liên kết Product không.

SKU có liên kết Recipe/BOM không.

SKU có liên kết packaging/trade item không nếu có.

SKU có bị dùng sai làm sellable gate không.

SKU seed hiện có có đầy đủ/đúng source-of-truth không.

## 6.3. Ingredient Master Analysis

Agent phải phân tích:

Ingredient master hiện có không.

Ingredient code/name.

Ingredient group.

Ingredient UOM.

Ingredient category.

Ingredient active status.

Ingredient source type.

Ingredient có phân nhóm nguyên liệu/vật tư không.

Ingredient có public/private boundary không.

Ingredient có liên kết recipe/BOM không.

Ingredient có bị hardcode trong recipe không.

Ingredient có audit/versioning nếu thay đổi không.

## 6.4. Recipe Master Analysis

Agent phải phân tích:

Recipe entity/model hiện có không.

Recipe code/name.

Recipe linked SKU.

Recipe version.

Recipe status.

Recipe active flag.

Recipe ingredient lines.

Recipe quantity.

Recipe UOM.

Recipe formula kind.

Recipe approval status.

Recipe effective date.

Recipe snapshot readiness.

Recipe có bị sửa đè lịch sử không.

Recipe có audit/versioning không.

Recipe có guard khi active không.

## 6.5. Formula Version / Formula Kind Analysis

Agent phải phân tích:

Có formula version không.

Có formula kind không.

Formula kind có bị hardcode không.

Có phân biệt pilot formula và production formula không.

Có lưu version lịch sử không.

Có cho sửa đè version đã dùng trong production không.

Có snapshot khi dùng vào production order không.

Có guard không cho recipe chưa approved/active vào production không.

Có liên kết BOM không.

Có test cho formula version không.

## 6.6. BOM / Ingredient Quantity / UOM Analysis

Agent phải phân tích:

BOM có tồn tại không.

BOM có linked SKU/recipe không.

BOM có line item không.

Ingredient quantity có UOM không.

UOM conversion có được kiểm soát không.

BOM có version không.

BOM có status không.

BOM có approval không.

BOM có audit không.

BOM có snapshot readiness không.

BOM có bị tính từ dữ liệu hardcode không.

BOM có mở rộng SKU mới được không.

## 6.7. Product Activation Analysis

Agent phải phân tích:

Product activation flow hiện có không.

SKU activation flow hiện có không.

Recipe activation flow hiện có không.

Product activation có guard không.

Product active có cần recipe active không.

Product active có cần SKU active không.

Product active có cần BOM/version không.

Product active có cần approval không.

Product active có audit không.

Product active có evidence không.

Product active có idempotency không nếu command có side effect.

Product active có permission không.

Product active có bị hiểu nhầm là sellable không.

## 6.8. Product Active vs Sellable Boundary Analysis

Agent phải kiểm tra bắt buộc:

Product Active có đang bị dùng làm điều kiện mở bán không.

SKU Active có đang bị dùng làm sellable không.

Recipe Active có đang bị dùng làm sellable không.

Có Sellable Gate riêng không.

Có Commerce Runtime kiểm sellable không.

Có inventory/warehouse/release/sale lock/recall dependency không.

Product Active có bị public là “đang bán” không.

Có rủi ro AI/Gateway/Commerce dùng Product Active sai không.

Có test chứng minh Product Active không đồng nghĩa Sellable không.

Nguyên tắc bắt buộc:

Product Active không đồng nghĩa Sellable.

## 6.9. SKU Expansion / Extension Registry Analysis

Agent phải phân tích:

Hệ thống có hỗ trợ thêm SKU mới không.

Có extension registry không.

Có cơ chế thêm SKU mà không sửa đè SKU cũ không.

Có recipe/BOM version cho SKU mới không.

Có activation guard cho SKU mới không.

Có audit/evidence/approval cho SKU mới không.

Có test thêm SKU mới không.

Có rủi ro hardcode danh sách 20 SKU không.

Có rủi ro mở rộng 40-50 SKU làm vỡ logic không.

## 7. SCOPE OUT

PROMPT-P1 không được:

Sửa Product code.

Sửa SKU code.

Sửa Recipe code.

Sửa BOM code.

Tạo migration.

Tạo seed.

Tạo API.

Tạo UI.

Tạo Product Activation Guard implementation.

Tạo Sellable Gate.

Sửa Commerce.

Sửa Operational Core.

Sửa AI Advisor.

Sửa Gateway.

Sửa Ads / MC AI Live / IVR.

Mở Global Gate.

Gọi Product Ready.

Gọi Sellable Ready.

Gọi PHASE 1 Complete.

Gọi Release Readiness / Production Readiness / Go-live Decision.

Nếu phát hiện cần sửa, chỉ ghi gap/handoff.

## 8. CURRENT IMPLEMENTATION STATE

## 8.1. Trạng thái được phép dùng

Agent chỉ được dùng các trạng thái:

Không được dùng:

PASS.

## COMPLETE.

## PRODUCT READY.

## SELLABLE READY.

Release Readiness.

Production Readiness.

Go-live Decision.

## 8.2. Matrix bắt buộc

## 9. P1 điểm chặn

Agent phải đánh dấu P1 điểm chặn nếu phát hiện:

Product Active đang bị dùng làm Sellable.

SKU Active đang bị dùng làm Sellable.

Recipe Active đang bị dùng làm Sellable.

Không có Product/SKU/Recipe source-of-truth.

Không có recipe version.

Không có formula version.

Không có BOM line rõ ràng.

Không có UOM cho ingredient quantity.

Recipe có thể sửa đè lịch sử.

Recipe/BOM đã dùng vẫn bị sửa trực tiếp.

Không có Product Activation Guard.

Product/SKU activation không kiểm recipe/BOM/approval.

Không có audit cho activation command.

Không có actor/correlation cho product command.

Không có evidence/approval cho product activation nếu TECH yêu cầu.

Hardcode danh sách SKU làm không mở rộng được.

Không có cơ chế thêm SKU mới.

Product/SKU public/private field bị lẫn.

AI/Gateway/Commerce có thể dùng Product Active để tư vấn/chốt bán.

Code cũ khác TECH mới nhưng chưa được ghi conflict.

## 10. GAP ANALYSIS MATRIX

Agent phải lập bảng:

## 11. CONFLICT DETECTION MATRIX

Agent phải lập bảng:

## 12. DOWNSTREAM IMPACT

Agent phải đánh giá PHASE 1 ảnh hưởng đến:

PHASE 2 - Operational Core
Vì Production Order cần SKU/Recipe/BOM/Formula Version đúng và có snapshot readiness.

PHASE 3 - Commerce Runtime
Vì Product Active không được dùng thay Sellable Gate.

PHASE 4 - AI Advisor Runtime
Vì AI chỉ được tư vấn/chốt theo public product data và sellable runtime đúng.

PHASE 5 - Facebook Gateway
Vì Gateway không được public sai SKU/private field.

PHASE 6 - Ads Measurement
Vì Ads không được dùng sản phẩm inactive/suppressed sai trạng thái.

PHASE 7 - MC AI Live
Vì Live script không được giới thiệu SKU chưa active/chưa sellable.

PHASE 8 - IVR
Vì IVR không liên quan Product activation trực tiếp nhưng order confirmation phụ thuộc order hợp lệ.

PHASE 9 - Global Smoke / Release Gateway
Vì Product/SKU/Recipe thiếu evidence/smoke thì không Release Readiness.

## 13. EVIDENCE REQUIRED

Agent phải gom evidence analysis gồm:

Source-of-truth đã đọc.

File/module Product đã inspect.

File/module SKU đã inspect.

File/module Recipe đã inspect.

File/module Ingredient/BOM đã inspect.

Migration/schema hiện có nếu có.

Seed hiện có nếu có.

Test hiện có nếu có.

Current implementation state matrix.

Gap matrix.

Conflict matrix.

điểm chặn register.

Downstream impact.

Git status/git diff chứng minh không sửa file nếu có git.

Ghi rõ:

Evidence trong PROMPT-P1 là Evidence Submitted, chưa phải Evidence Accepted.

## 14. SMOKE REQUIRED - CHỈ ĐỀ XUẤT, CHƯA VIẾT TEST

Agent phải đề xuất smoke cho phase implementation sau:

Không gọi smoke đề xuất này là Global Smoke Pass.

## 15. EXECUTION STEPS

Agent phải làm theo thứ tự:

Đọc TECH source-of-truth.

Đọc PHASE 0 validation report.

Inspect codebase thật.

Tìm Product module.

Tìm SKU module.

Tìm Ingredient module.

Tìm Recipe/BOM module.

Tìm Product Activation logic nếu có.

Tìm Product/SKU seed nếu có.

Tìm Product/SKU/Recipe tests nếu có.

Tìm các chỗ Product Active đang được dùng.

Tìm các chỗ Sellable đang được dùng.

Tìm chỗ AI/Gateway/Commerce có thể đọc Product Active sai.

Lập current implementation state.

Lập gap matrix.

Lập conflict matrix.

Lập điểm chặn register.

Lập downstream impact.

Lập smoke required.

Báo cáo đủ 14 mục.

Không sửa file.

## 16. REQUIRED REPORT FORMAT - 14 MỤC

Agent phải báo cáo đúng 14 mục.

## 16.1. Mục 1 - Tóm tắt

Ghi rõ:

Prompt đã chạy: PROMPT-P1.

Mode: ANALYSIS ONLY.

Scope đã phân tích.

Current implementation state tổng quan.

P1 điểm chặn chính.

Gap chính.

Downstream impact chính.

Trạng thái cuối.

Không được ghi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

## 16.2. Mục 2 - File đã sửa

Bắt buộc ghi:

Không sửa file.

Không tạo file.

Không tạo code.

Không tạo migration.

Không tạo seed.

Không update database.

Git status/git diff nếu có.

## 16.3. Mục 3 - Nguồn yêu cầu

Liệt kê:

## TECH-01.

## TECH-10.

## TECH-11.

## TECH-12.

## TECH-13.

PHASE 0 Validation Report.

TECH Product / SKU / Recipe nếu có.

Legacy docs nếu có đọc, ghi rõ LEGACY_REFERENCE_ONLY.

## 16.4. Mục 4 - Evidence đã dùng

Liệt kê:

Bắt buộc ghi:

Evidence Submitted, not Evidence Accepted.

## 16.5. Mục 5 - Lệnh đã chạy

Liệt kê:

Lệnh inspect.

Lệnh search.

Lệnh git status/git diff nếu có.

Lệnh build/test nếu có chạy ở chế độ không sửa file.

Nếu không chạy lệnh nào, ghi lý do.

## 16.6. Mục 6 - Kết quả test

Ghi rõ:

Test Product/SKU/Recipe hiện có.

Test đã chạy nếu có.

Test pass/fail nếu có.

Test gap.

Smoke required cho phase sau.

Không gọi test pass là Global Smoke Pass.

## 16.7. Mục 7 - Kết quả backend build

Ghi rõ:

Có chạy backend build không.

Kết quả nếu có.

Nếu không chạy, lý do.

## 16.8. Mục 8 - Kết quả frontend build

Ghi rõ:

Có chạy frontend build không.

Có cần chạy không.

Nếu không chạy, lý do.

## 16.9. Mục 9 - Kết quả cleanup process

Ghi rõ:

Có process nào mở không.

Có server/test runner cần dừng không.

Có file tạm không.

Có artifact phát sinh không.

Đã cleanup chưa.

## 16.10. Mục 10 - Cập nhật Markdown

Bắt buộc ghi:

Không cập nhật Markdown.

Không sửa tài liệu trong repo.

Chỉ trả report trong response.

Nếu có cập nhật Markdown ngoài ý muốn thì FAIL.

## 16.11. Mục 11 - Kết quả database migration/update nếu áp dụng

Bắt buộc ghi:

Không tạo migration.

Không chạy migration.

Không update database.

Chỉ đọc migration/schema hiện có nếu cần.

## 16.12. Mục 12 - Kết quả seed validation nếu áp dụng

Ghi rõ:

Có seed Product/SKU/Recipe không.

Seed có đúng source-of-truth không.

Seed có hardcode sai không.

Seed có hỗ trợ mở rộng SKU không.

Không sửa seed.

## 16.13. Mục 13 - Rủi ro còn lại

Phân nhóm:

Product Master risk.

SKU Master risk.

Ingredient Master risk.

Recipe/BOM risk.

Formula Version risk.

Product Activation risk.

Active vs Sellable risk.

SKU Extension risk.

Seed/Test risk.

Downstream PHASE 2/3/4 risk.

Release Gateway risk.

Global Gate risk.

## 16.14. Mục 14 - Cập nhật handoff

Phải ghi:

Gap cần xử lý ở PHASE 1 Technical Design.

điểm chặn cần clear trước implementation.

Evidence cần chuẩn bị.

Smoke cần viết/chạy.

Prompt tiếp theo đề xuất.

Điều kiện để được chuyển sang P1.1.

Trạng thái cuối.

Bắt buộc ghi:

## 17. DONE GATE

PROMPT-P1 chỉ được xem là analysis done khi đủ:

Đã đọc source-of-truth.

Đã đọc PHASE 0 report.

Đã inspect codebase thật.

Đã phân tích Product Master.

Đã phân tích SKU Master.

Đã phân tích Ingredient Master.

Đã phân tích Recipe/BOM.

Đã phân tích Formula Version / Formula Kind.

Đã phân tích Product Activation.

Đã phân tích Product Active vs Sellable.

Đã phân tích SKU Extension Registry.

Đã có current implementation state matrix.

Đã có gap matrix.

Đã có conflict matrix.

Đã có điểm chặn register.

Đã có downstream impact.

Đã có smoke required.

Đã report đủ 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Không gọi PHASE 1 Complete / Release Readiness / Production Readiness / Go-live Decision.

Global Gate vẫn bị chặn.

Trạng thái tối đa được phép:

## PHASE 1 ANALYSIS REPORT COMPLETED

Không được gọi:

PHASE 1 Complete.

Implementation Complete.

Completion Decision.

Release Readiness.

Production Readiness.

Go-live Decision.

Gateway PASS.

## 18. FAIL GATE

PROMPT-P1 phải fail nếu:

Agent sửa file.

Agent tạo code.

Agent tạo migration.

Agent tạo seed.

Agent update database.

Agent không inspect codebase.

Agent không đọc source-of-truth.

Agent không phân tích Product Active vs Sellable.

Agent gọi Product Active là Sellable.

Agent bỏ qua Recipe/BOM/Formula Version.

Agent không lập gap matrix.

Agent không lập conflict matrix.

Agent không lập điểm chặn register.

Agent không report đủ 14 mục.

Agent tự đổi nghiệp vụ.

Agent hardcode Product/SKU/Recipe policy.

Agent gọi Analysis là Implementation Complete.

Agent gọi Release Readiness / Production Readiness / Go-live Decision.

Agent mở Global Gate.

Nếu fail, agent phải ghi:

PROMPT-P1 FAIL GATE - ANALYSIS NOT ACCEPTED

## 19. DOWNSTREAM HANDOFF

## 19.1. Prompt tiếp theo nếu P1 analysis đạt Done Gate

Nếu PROMPT-P1 đạt Done Gate, prompt tiếp theo nên là:
