# 00-P2 - PHÂN TÍCH VẬN HÀNH LÕI

Trạng thái: `OPERATIONAL_ANALYSIS_SOURCE_ABSORBED`  
Mode: `ANALYSIS ONLY`  
Phase: `PHASE 2 - Operational Core`  
Mục tiêu: phân tích nguồn sự thật Phase 2 sau khi hấp thụ đủ phiếu/lệnh/tự sinh/print/MISA/trace từ 5 file cũ.

## 1. Phạm vi chuẩn

Phase 2 consume Phase 1 Product/SKU/Recipe/Formula/Material/Packaging để vận hành:

1. Raw receipt, raw lot, incoming QC, readiness.
2. Production order, production dossier, formula snapshot.
3. Material request, material approval, material issue, workshop receipt.
4. Batch execution, preprocessing, freeze-dry QC, packaging L1/L2.
5. Finished goods QC, release, warehouse receipt, inventory ledger.
6. Traceability, QR/public trace, recall, sale lock.
7. Accounting/MISA handoff boundary.

## 2. Source coverage

| Nguồn cũ | Nội dung đã đưa vào Phase 2 | File Phase 2 chính |
| --- | --- | --- |
| BẢNG PHIẾU LỆNH SẢN XUẤT | 12 phiếu/lệnh tối thiểu, field từng phiếu, xác nhận, evidence, kế toán | 00, 01, 02, 03, 04, 05, 07 |
| PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN | Hồ sơ sản xuất gốc, tự sinh phiếu, click/tự hiện/nhập, print, MISA boundary | 01, 03, 04, 05, 06, 07 |
| BỘ KHÓA 5 BƯỚC | Operational config, trace chain, recall, route/action, no manual ingredient | 01, 03, 04, 05, 06, 07 |
| DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ | Material/packaging group, thresholds, disposal/write-off, MRP suppression | 02, 04, 05, 07 |
| CÔNG THỨC VẬN HÀNH 20 SKU MỚI | Formula G1 mà Phase 2 snapshot/issue/trace consume | 03, 04, 05, 06, 07 |

## 2.1. Nơi lưu nguồn gốc sau khi bỏ 5 file rời

Các file dưới đây đã được hấp thụ vào Phase 2 theo đúng filename nguồn người dùng cung cấp. Từ sau bản này, Phase 2 là nơi tra cứu chính; phụ lục nguyên văn chỉ còn là payload bảo toàn ở cuối file phase tương ứng. Khi wording giữa phụ lục và phần canonical ở đầu file khác nhau, phần canonical ở đầu file thắng.

| File nguồn cũ | File Phase 2 giữ payload nguyên văn | Rule canonical đã rút ra |
| --- | --- | --- |
| `BẢNG PHIẾU LỆNH SẢN XUẤT.md` | `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | 12 phiếu/lệnh tối thiểu, field bắt buộc, xác nhận, evidence, kế toán |
| `PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md` | `01-THIẾT KẾ KỸ THUẬT VẬN HÀNH LÕI.md`, `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | Tự sinh phiếu, click/tự hiện/nhập tay, print, kế toán, MISA boundary |
| `DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ.md` | `02-NGUỒN NGUYÊN LIỆU LÔ QC SẴN SÀNG SẢN XUẤT.md` | Nhóm A/B, threshold, suppression, disposal/write-off, readiness |
| `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` | `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | Formula G1 được snapshot, không chọn tay nguyên liệu |
| `BỘ KHÓA 5 BƯỚC.md` | `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | BOM map, trace chain, recall/sale lock, operational screen/action |

## 3. 12 phiếu/lệnh tối thiểu

| STT | Phiếu/lệnh | Bắt buộc vì |
| --- | --- | --- |
| 1 | Phiếu đánh giá và nhập kho nguyên liệu đầu vào | Cửa vào nguyên liệu, giá nhập, lot, QC |
| 2 | Lệnh sản xuất | Sinh hồ sơ sản xuất gốc, chọn SKU ra công thức |
| 3 | Phiếu đề nghị cấp nguyên liệu | Cầu nối công thức và kho |
| 4 | Phiếu chấp thuận nguyên liệu được phép đưa vào sản xuất | Khóa lot dùng cho mẻ |
| 5 | Phiếu check-in/check-out nhân sự | Tính chi phí nhân công/trace |
| 6 | Phiếu theo dõi sơ chế | Theo dõi khay/kg/lít, hao hụt |
| 7 | Phiếu kiểm chất lượng sau sấy thăng hoa | Cổng QC trước đóng gói |
| 8 | Phiếu theo dõi đóng gói cấp 1 | Gói/lọ/hũ, in cấp 1 |
| 9 | Phiếu theo dõi đóng gói cấp 2 | Hộp/thùng, QR, barcode, batch |
| 10 | Phiếu QC thành phẩm | Chốt đạt/không đạt trước nhập kho |
| 11 | Lệnh nhập kho thành phẩm | Kích hoạt tồn thành phẩm |
| 12 | Phiếu kế toán xuất nguyên liệu | Chứng từ giá trị cho kế toán |

## 4. Boundary không được lẫn

1. `QC_PASS` nguyên liệu không tự là `READY_FOR_PRODUCTION`.
2. `QC_PASS` batch không tự là `RELEASED`.
3. `RELEASED` batch không tự là `SELLABLE`.
4. Warehouse không nhận batch chưa released.
5. Material issue là điểm giảm tồn nguyên liệu chính.
6. Workshop receipt không giảm tồn lần hai.
7. MISA handoff không sync thật nếu PACK-04 chưa mở.

## 5. Luồng vận hành chuẩn Phase 2

```text
Raw Receipt -> Incoming QC -> Raw Lot Readiness
  -> Production Order -> Production Dossier -> Production BOM Snapshot
  -> Material Request -> Material Approval -> Material Issue -> Workshop Receipt
  -> Batch Execution -> Process QC -> Packaging L1 -> Packaging L2
  -> Finished QC -> Batch Release -> Finished Goods Receipt -> Inventory Ledger
  -> Internal Trace -> Public QR Projection -> Recall/Sale Lock
  -> Accounting Handoff
```

Không bước nào được nhảy cóc state. Nếu cần override, override phải có owner approval, reason, evidence, audit và không được tự gọi là pass.

## 6. File con Phase 2 sau rewrite

| Thứ tự | File tiếng Việt | Vai trò nguồn chính |
| --- | --- | --- |
| 00 | `00-PHÂN TÍCH VẬN HÀNH LÕI.md` | Phạm vi, source coverage, 12 phiếu/lệnh, boundary |
| 01 | `01-THIẾT KẾ KỸ THUẬT VẬN HÀNH LÕI.md` | Entity, command, event, self-generation, idempotency |
| 02 | `02-NGUỒN NGUYÊN LIỆU LÔ QC SẴN SÀNG SẢN XUẤT.md` | Raw receipt, raw lot, QC, readiness, disposal, threshold |
| 03 | `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | Production order, dossier gốc, BOM snapshot, field phiếu |
| 04 | `04-CẤP PHÁT NGUYÊN LIỆU NHẬN XƯỞNG VÀ SỔ KHO.md` | Material request, approval, issue, workshop receipt, ledger |
| 05 | `05-MẺ SẢN XUẤT QC PHÁT HÀNH KHO VÀ TỒN.md` | Batch execution, QC, release, packaging, warehouse receipt |
| 06 | `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | Trace chain, QR public trace, recall, sale lock |
| 07 | `07-KIỂM THỬ KHÓI BẰNG CHỨNG VÀ NGHIỆM THU.md` | Evidence, smoke, acceptance, blocker gate |

## 7. Quy tắc xử lý conflict nguồn

| Conflict nguồn | Rule Phase 2 |
| --- | --- |
| Phiếu cho phép nhập tay nguyên liệu sau khi đã chọn SKU | Cấm. Nguyên liệu phải tự hiện từ BOM snapshot; thay đổi đi qua Formula Version hoặc owner-approved deviation |
| QC pass nguyên liệu bị hiểu là sẵn sàng cấp phát | Cấm. Phải có readiness gate riêng |
| Production order bị hiểu là đã xuất kho | Cấm. Chỉ Material Issue mới debit tồn nguyên liệu |
| Workshop receipt bị hiểu là xuất kho lần hai | Cấm. Workshop receipt chỉ xác nhận nhận xưởng |
| Batch QC pass bị hiểu là release | Cấm. Release cần approval riêng |
| Release bị hiểu là sellable | Cấm. Commerce sellable gate là boundary sau warehouse/release/sale lock |
| Print/reprint không reason | Cấm. Reprint phải có reason, actor, bản in gốc, payload hash |
| MISA handoff bị hiểu là sync thật | Cấm khi chưa có PACK-04/owner credential/approval |

## 8. Done gate phân tích

Phase 2 Analysis chỉ đạt khi:

1. Đã map đủ 12 phiếu/lệnh vào file 02-07.
2. Đã xác định rõ dữ liệu nào click chọn, dữ liệu nào tự hiện, dữ liệu nào được nhập tay.
3. Đã khóa raw lot readiness trước material issue.
4. Đã khóa production dossier là root của toàn bộ phiếu sau.
5. Đã khóa accounting/MISA là handoff fail-closed, không sync thật.
6. Đã có smoke/evidence ID cho từng gate vận hành.
7. Không dùng `Production Ready`, `Go-live`, `Complete` nếu thiếu owner sign-off và evidence accepted.
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 00-P2-ANALYSIS-ONLY.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2 - ANALYSIS ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- MASTER registry va runtime guard.
- PACK-01 Operational Core.
- TECH-03 Operational Core.
- PHASE-01 Product / SKU / Recipe foundation.

## Noi Dung Rewrite

FILE: 00-P2-ANALYSIS-ONLY.docx

## PROMPT-P2 - OPERATIONAL CORE / PRODUCTION / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK ANALYSIS HANDOFF

## MODE: ANALYSIS ONLY

## 1. PHASE MARKER

## 1.1. Phase hiện tại

## PHASE-02 - OPERATIONAL CORE

## 1.2. Prompt hiện tại

## PROMPT-P2 - OPERATIONAL CORE / PRODUCTION / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK ANALYSIS HANDOFF

## 1.3. Mode

## MODE: ANALYSIS ONLY

## 1.4. Prompt liền trước

## PROMPT-P1.2E - PHASE 1 SMOKE / EVIDENCE / IMPLEMENTATION REPORT HANDOFF

## 1.5. Prompt tiếp theo

## PROMPT-P2.1 - OPERATIONAL CORE TECHNICAL DESIGN HANDOFF

## 1.6. Điều kiện bắt đầu PHASE 2

PHASE 2 chỉ được bắt đầu khi:

PHASE 1 đã có Analysis Report.

PHASE 1 đã có Technical Design Handoff.

PHASE 1 đã có các limited implementation report.

PHASE 1 đã có Smoke / Evidence / Implementation Report.

Product / SKU / Recipe / Formula / BOM foundation đã có.

Product Active không bị hiểu là Sellable.

Recipe Active không bị hiểu là Batch Released.

Không còn điểm chặn nghiêm trọng từ Product / SKU / Recipe.

Owner/dev lead cho phép viết PROMPT-P2 Analysis Only.

Global Gate vẫn bị chặn.

## 1.7. Điều kiện chuyển sang P2.1

Chỉ được chuyển sang P2.1 Technical Design Only khi PROMPT-P2 đạt đủ:

Đã inspect codebase thật.

Đã phân tích Source / Raw Material / Raw Lot / QC / Readiness.

Đã phân tích Production Order / Formula Snapshot.

Đã phân tích Material Request / Material Issue / Material Receipt.

Đã phân tích Batch / QC / Release.

Đã phân tích Warehouse / Inventory Ledger.

Đã phân tích Traceability / QR / Recall / Sale Lock.

Đã có current implementation state matrix.

Đã có gap matrix.

Đã có conflict matrix.

Đã có điểm chặn register.

Đã có downstream impact.

Đã có smoke required.

Đã có report 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Global Gate vẫn bị chặn.

## 2. HEADER

## 2.1. Tên prompt

## PROMPT-P2 - OPERATIONAL CORE / PRODUCTION / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK ANALYSIS HANDOFF

## 2.2. Vị trí trong roadmap

PROMPT-P2 là prompt mở đầu cho:

PHASE 2 - Operational Core

PHASE 2 là lớp vận hành lõi, quản lý chuỗi:

Source / Raw Material -> Raw Lot -> QC -> READY_FOR_PRODUCTION -> Production Order -> Material Issue -> Material Receipt -> Batch -> QC / Release -> Warehouse Receipt -> Inventory Ledger -> Traceability -> QR / Public Trace -> Recall / Sale Lock

PROMPT-P2 không phải implementation prompt.

PROMPT-P2 chỉ dùng để phân tích codebase thật, tìm gap, conflict, điểm chặn và xác định mức độ sẵn sàng của Operational Core trước khi viết technical design.

## 2.3. Mục tiêu

Mục tiêu của PROMPT-P2 là yêu cầu dev/Codex/Copilot:

Đọc source-of-truth.

Đọc PHASE 1 report.

Inspect codebase thật.

Xác định current implementation state của Operational Core.

Đối chiếu code hiện tại với TECH source-of-truth.

Tìm gap, conflict, điểm chặn.

Tìm rủi ro về Raw Lot QC_PASS vs READY_FOR_PRODUCTION.

Tìm rủi ro về Material Issue / Material Receipt / Inventory Ledger.

Tìm rủi ro về Production Order formula snapshot.

Tìm rủi ro về Batch QC_PASS vs Batch RELEASED.

Tìm rủi ro về Warehouse Receipt / Finished Goods Inventory.

Tìm rủi ro về Traceability / QR / Recall / Sale Lock.

Lập báo cáo phân tích đủ 14 mục.

Không sửa file.

Không tạo code.

Không tạo migration.

Không gọi PHASE 2 Complete / Release Readiness / Production Readiness / Go-live Decision.

## 3. MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

## 3.1. Chế độ thực thi

## MODE: ANALYSIS ONLY

Agent chỉ được phép:

Đọc tài liệu.

Đọc report PHASE 0 và PHASE 1.

Inspect codebase thật.

Tìm kiếm file.

Đọc module Source / Raw Material nếu có.

Đọc module QC nếu có.

Đọc module Production nếu có.

Đọc module Material Issue / Receipt nếu có.

Đọc module Batch nếu có.

Đọc module Warehouse / Inventory nếu có.

Đọc module Traceability / QR nếu có.

Đọc module Recall / Sale Lock nếu có.

Đọc migration/schema hiện có nếu có.

Đọc seed hiện có nếu có.

Đọc test hiện có nếu có.

Lập current implementation state.

Lập gap matrix.

Lập conflict matrix.

Lập P2 điểm chặn register.

Lập downstream impact.

Lập smoke required.

Lập report 14 mục.

## 3.2. Các hành động bị cấm

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

Sửa source/raw/lot data.

Sửa QC data.

Sửa production order.

Sửa material issue / receipt.

Sửa inventory ledger.

Sửa warehouse data.

Sửa traceability data.

Sửa QR / recall / sale lock logic.

Tạo API.

Tạo UI.

Tự đổi nghiệp vụ.

Hardcode policy.

Gọi Raw Lot QC_PASS là READY_FOR_PRODUCTION.

Gọi Batch QC_PASS là RELEASED.

Gọi Warehouse Receipt trước khi batch RELEASED.

Gọi Finished Goods available khi chưa warehouse receipt confirmed.

Gọi PHASE 2 Complete.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Mở Global Gate.

## 4. SOURCE-OF-TRUTH BẮT BUỘC

## 4.1. Source chính

Agent phải đọc và ưu tiên:

TECH-01 - Foundation / RBAC / Audit / Evidence / Idempotency

TECH-10 - Global Smoke / UAT / Evidence / Release Gateway

TECH-11 - Implementation Roadmap / Dev Phase Plan

TECH-12 - Phase Backlog Pack

TECH-13 - Codex / Copilot Dev Prompt Pack

PHASE 0 Validation Report

PHASE 1 Smoke / Evidence / Implementation Report

TECH Operational Core nếu đã có trong bộ TECH

Source / Raw / Production / Warehouse / Inventory / Traceability / Recall source-of-truth mới do owner cung cấp nếu có

## 4.2. Quy tắc ưu tiên

Agent phải áp dụng:

TECH mới thắng code cũ.

TECH mới thắng tài liệu cũ.

Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

PHASE 0 / PHASE 1 evidence submitted chưa phải evidence accepted.

Không dùng legacy làm nền nghiệp vụ.

Không để code cũ thắng TECH mới.

Nếu code hiện tại khác TECH mới thì báo conflict, không sửa.

Nếu tài liệu cũ khác TECH mới thì báo legacy conflict, không dùng làm nền.

Nếu nghiệp vụ Operational Core chưa có source-of-truth thì không tự suy luận.

## 5. OBJECTIVE

## 5.1. Mục tiêu phân tích PHASE 2

Agent phải phân tích các domain sau:

Source Origin / Supplier / Growing Source nếu có.

Raw Material Master.

Raw Material Receipt.

Raw Lot.

Incoming QC.

Raw Lot Readiness.

Production Demand / Production Order.

Formula Snapshot / Recipe Snapshot.

Material Request.

Material Issue.

Material Receipt / Workshop Receipt.

Production Batch.

Batch Process State.

Batch QC.

Batch Release.

Warehouse Receipt.

Inventory Ledger.

Inventory Balance Projection.

Packaging / QR / Print readiness nếu có liên quan.

Traceability / Genealogy.

Public Trace boundary.

Recall / Hold / Sale Lock.

Operational Evidence / Audit / Idempotency.

Operational Smoke hiện có nếu có.

## 5.2. Mục tiêu không phải

PROMPT-P2 không nhằm:

Viết code Operational Core.

Tạo schema DB.

Tạo API.

Tạo UI.

Tạo migration.

Tạo seed.

Tạo production order.

Issue material.

Receipt material.

Release batch.

Confirm warehouse receipt.

Tạo QR.

Public trace dữ liệu.

Mở bán sản phẩm.

Release.

Go-live.

## 6. SCOPE IN

## 6.1. Source Origin / Raw Material Analysis

Agent phải phân tích:

Source origin entity/model hiện có không.

Supplier/source zone hiện có không.

Raw material master hiện có không.

Raw material group/category hiện có không.

Raw material UOM hiện có không.

Raw material receipt flow hiện có không.

Raw material evidence/COA/source verification nếu có không.

Raw material có audit/actor/correlation không nếu có command.

Raw material có public/private boundary không.

Raw material có liên kết raw lot không.

## 6.2. Raw Lot Analysis

Agent phải phân tích:

Raw lot entity/model hiện có không.

Raw lot code.

Raw material linkage.

Source/receipt linkage.

Lot quantity/UOM.

Lot status.

Lot QC status.

Lot readiness status.

Lot hold/reject/quarantine status nếu có.

Lot có audit/state history không.

Lot có inventory/ledger linkage không.

Lot có traceability linkage không.

Lot có thể bị dùng sai khi chỉ QC_PASS chưa READY_FOR_PRODUCTION không.

Nguyên tắc bắt buộc:

RAW_LOT QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

## 6.3. Incoming QC / Raw Lot Readiness Analysis

Agent phải phân tích:

Incoming QC flow hiện có không.

QC result có PASS/FAIL/HOLD không.

QC result có actor/audit/correlation không.

QC result có evidence không.

QC result có idempotency không nếu command có side effect.

Có bước mark READY_FOR_PRODUCTION riêng không.

READY_FOR_PRODUCTION có guard không.

Raw lot chỉ được issue khi READY_FOR_PRODUCTION không.

Code có chỗ nào dùng QC_PASS thay READY_FOR_PRODUCTION không.

Có test chứng minh QC_PASS không đủ để issue không.

Nguyên tắc bắt buộc:

Raw lot chỉ được issue khi READY_FOR_PRODUCTION.

## 6.4. Production Order / Formula Snapshot Analysis

Agent phải phân tích:

Production order entity/model hiện có không.

Production order code/status.

Production order linked SKU.

Production order linked recipe.

Production order linked formula version.

Production order có snapshot formula kind không.

Production order có snapshot ingredient/quantity/UOM không.

Production order có snapshot recipe version không.

Production order có audit/actor/correlation không.

Production order có idempotency không.

Production order có approval/guard không.

Production order có bị sửa theo recipe mới sau khi đã tạo không.

Nguyên tắc bắt buộc:

Production Order phải snapshot formula kind / version / ingredient / quantity / UOM.

## 6.5. Material Request / Material Issue Analysis

Agent phải phân tích:

Material request entity/model hiện có không.

Material issue entity/model hiện có không.

Material issue linked production order không.

Material issue linked raw lot không.

Material issue kiểm raw lot READY_FOR_PRODUCTION không.

Material issue có quantity/UOM không.

Material issue có idempotency không.

Material issue có audit không.

Material issue có inventory ledger debit không.

Material issue có thể bị retry tạo double debit không.

Material issue có test duplicate retry không.

Material issue có state machine không.

Nguyên tắc bắt buộc:

Material Issue là điểm giảm tồn nguyên liệu chính.

## 6.6. Material Receipt / Workshop Receipt Analysis

Agent phải phân tích:

Material receipt/workshop receipt entity/model hiện có không.

Material receipt linked production order không.

Material receipt linked material issue không.

Material receipt xác nhận xưởng nhận nguyên liệu không.

Material receipt có variance không.

Material receipt có audit không.

Material receipt có idempotency không.

Material receipt có inventory ledger debit lần hai không.

Material receipt có bị dùng sai để giảm tồn nguyên liệu không.

Material receipt có test chứng minh không double debit không.

Nguyên tắc bắt buộc:

Material Receipt không được giảm tồn lần hai.

## 6.7. Production Batch / Process State Analysis

Agent phải phân tích:

Production batch entity/model hiện có không.

Batch linked production order không.

Batch linked material receipt không.

Batch code/status.

Batch process state.

Batch process step nếu có.

Batch output quantity nếu có.

Batch loss/variance nếu có.

Batch audit/state history không.

Batch traceability linkage không.

Batch có public/private boundary không.

## 6.8. Batch QC / Batch Release Analysis

Agent phải phân tích:

Batch QC flow hiện có không.

Batch QC result có PASS/FAIL/HOLD/REJECT không.

Batch QC có actor/audit/correlation không.

Batch QC có evidence không.

Batch QC có idempotency không.

Batch Release flow riêng không.

Batch Release có approval/guard không.

Batch Release có audit/evidence không.

Batch QC_PASS có bị dùng thay RELEASED không.

Warehouse có nhận batch QC_PASS chưa RELEASED không.

Nguyên tắc bắt buộc:

Batch QC_PASS không đồng nghĩa Batch RELEASED.

## 6.9. Warehouse Receipt / Finished Goods Inventory Analysis

Agent phải phân tích:

Warehouse receipt entity/model hiện có không.

Warehouse receipt chỉ nhận batch RELEASED không.

Warehouse receipt có idempotency không.

Warehouse receipt có audit không.

Warehouse receipt có ledger credit thành phẩm không.

Inventory balance projection có từ ledger không.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed không.

Warehouse receipt retry có tạo double credit không.

Có test duplicate warehouse receipt không.

Nguyên tắc bắt buộc:

Warehouse chỉ nhận batch RELEASED.
Finished goods chỉ tăng tồn khi warehouse receipt confirmed.
Inventory Ledger append-only.

## 6.10. Inventory Ledger Analysis

Agent phải phân tích:

Inventory ledger entity/model hiện có không.

Ledger append-only không.

Ledger có debit/credit không.

Ledger có raw material ledger không.

Ledger có finished goods ledger không.

Ledger có balance projection không.

Ledger có audit/correlation không.

Ledger có idempotency linkage không.

Ledger có thể update/delete không.

Ledger có test append-only không.

Ledger có double debit/double credit risk không.

Nguyên tắc bắt buộc:

Inventory Ledger append-only.

## 6.11. Traceability / QR / Public Trace Analysis

Agent phải phân tích:

Traceability entity/model hiện có không.

Genealogy raw lot -> production order -> batch -> warehouse receipt có không.

QR entity/model hiện có không.

QR status có GENERATED / PRINTED / VOID / FAILED không nếu có.

QR VOID/FAILED có bị public-valid không.

Public Trace có whitelist DTO không.

Public Trace có lọc private field không.

Public Trace có supplier/cost/formula/internal QC leak risk không.

Trace gap detection có không.

Trace có recall impact support không.

Nguyên tắc bắt buộc:

Public Trace whitelist-only.
QR VOID/FAILED không public-valid.

## 6.12. Recall / Hold / Sale Lock Analysis

Agent phải phân tích:

Recall case entity/model hiện có không.

Hold flow hiện có không.

Sale Lock flow hiện có không.

Recall impact analysis có không.

Recall linked traceability không.

Recall có audit/evidence không.

Recall có notification handoff nếu có không.

Sale Lock có thắng Commerce không.

Sale Lock có thắng AI / Ads / Live / Gateway / CRM / IVR không.

Có test stop-sale không.

Có risk system vẫn bán hàng khi recall active không.

Nguyên tắc bắt buộc:

Recall / Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

## 7. SCOPE OUT

PROMPT-P2 không được:

Sửa Source / Raw code.

Sửa QC code.

Sửa Production code.

Sửa Inventory code.

Sửa Warehouse code.

Sửa Traceability code.

Sửa Recall code.

Tạo migration.

Tạo seed.

Tạo API.

Tạo UI.

Tạo production order.

Issue material.

Receipt material.

Release batch.

Confirm warehouse receipt.

Tạo QR.

Public trace dữ liệu.

Kích hoạt recall/sale lock.

Tạo Sellable Gate.

Sửa Commerce.

Sửa AI Advisor.

Sửa Gateway / Ads / MC AI Live / IVR.

Mở Global Gate.

Gọi PHASE 2 Complete.

Gọi Release Readiness / Production Readiness / Go-live Decision.

Nếu phát hiện cần sửa, chỉ ghi gap/handoff.

## 8. CURRENT IMPLEMENTATION STATE

## 8.1. Trạng thái được phép dùng

Agent chỉ được dùng các trạng thái:

Status

Ý nghĩa

## NOT_FOUND

Chưa thấy implementation

## PARTIAL

Có một phần nhưng chưa đủ TECH

## IMPLEMENTED_BUT_CONFLICT

Có implementation nhưng khác TECH

## IMPLEMENTED_NEEDS_VALIDATION

Có implementation tương đối nhưng thiếu evidence/smoke

## UNKNOWN_BLOCKED

Không đủ quyền/thiếu file/không inspect được

Không được dùng:

PASS.

## COMPLETE.

## OPERATIONAL READY.

Release Readiness.

Production Readiness.

Go-live Decision.

## 8.2. Matrix bắt buộc

Domain

Current State

Evidence Found

Gap

P2 điểm chặn

Downstream Impact

Source Origin

Raw Material Master

Raw Material Receipt

Raw Lot

Incoming QC

Raw Lot Readiness

Production Order

Formula Snapshot

Material Request

Material Issue

Material Receipt

Production Batch

Batch QC

Batch Release

Warehouse Receipt

Inventory Ledger

Traceability

QR / Public Trace

Recall / Sale Lock

## 9. P2 điểm chặn

Agent phải đánh dấu P2 điểm chặn nếu phát hiện:

Raw Lot QC_PASS đang bị dùng làm READY_FOR_PRODUCTION.

Raw lot chưa READY_FOR_PRODUCTION vẫn được issue.

Material Issue không giảm tồn nguyên liệu.

Material Receipt giảm tồn nguyên liệu lần hai.

Inventory Ledger không append-only.

Material Issue retry có thể tạo double debit.

Warehouse Receipt retry có thể tạo double credit.

Production Order không snapshot formula kind/version/ingredient/quantity/UOM.

Recipe thay đổi làm Production Order cũ bị thay đổi.

Batch QC_PASS đang bị dùng làm RELEASED.

Warehouse nhận batch chưa RELEASED.

Finished goods tăng tồn trước warehouse receipt confirmed.

QR VOID/FAILED vẫn public-valid.

Public Trace không whitelist-only.

Public Trace lộ private/internal field.

Recall / Sale Lock không chặn Commerce.

Recall / Sale Lock không chặn AI/Gateway/Ads/Live/CRM/IVR.

Không có audit cho high-risk operational command.

Không có idempotency cho issue/receipt/release/warehouse.

Không có evidence cho QC/release/recall close nếu TECH yêu cầu.

Code cũ khác TECH mới nhưng chưa được ghi conflict.

## 10. GAP ANALYSIS MATRIX

Agent phải lập bảng:

Gap ID

Domain

TECH Requirement

Current State

Gap Description

Severity

P2 điểm chặn

Downstream Impact

Evidence Found

Smoke Required

Recommended Next Action

## P2-GAP-001

Raw Lot Readiness

## P2-GAP-002

Production Order Snapshot

## P2-GAP-003

Material Issue

## P2-GAP-004

Material Receipt

## P2-GAP-005

Inventory Ledger

## P2-GAP-006

Batch QC / Release

## P2-GAP-007

Warehouse Receipt

## P2-GAP-008

Traceability / QR

## P2-GAP-009

Recall / Sale Lock

## P2-GAP-010

Audit / Evidence / Idempotency

## 11. CONFLICT DETECTION MATRIX

Agent phải lập bảng:

Conflict ID

Area

TECH Source-of-truth

Current Implementation / Legacy State

Conflict Description

Risk

P2 điểm chặn

Recommended Handling

## P2-CONFLICT-001

QC_PASS vs READY_FOR_PRODUCTION

## P2-CONFLICT-002

Material Issue / Inventory Debit

## P2-CONFLICT-003

Material Receipt Double Debit

## P2-CONFLICT-004

Production Snapshot

## P2-CONFLICT-005

QC_PASS vs RELEASED

## P2-CONFLICT-006

Warehouse Receipt

## P2-CONFLICT-007

Public Trace

## P2-CONFLICT-008

Recall / Sale Lock

## 12. DOWNSTREAM IMPACT

Agent phải đánh giá PHASE 2 ảnh hưởng đến:

PHASE 3 - Commerce Runtime
Vì Commerce Sellable Gate phải dựa vào warehouse/finished goods/release/sale lock truth, không dùng Product Active thay sellable.

PHASE 4 - AI Advisor Runtime
Vì AI chỉ được tư vấn/chốt theo sellable runtime, không được bán sản phẩm bị hold/recall/out of stock.

PHASE 5 - Facebook Gateway
Vì Gateway không được public thông tin nội bộ và phải tôn trọng recall/sale lock.

PHASE 6 - Ads Measurement
Vì Ads không được scale SKU bị recall/sale lock/suppression hoặc không có verified operational truth.

PHASE 7 - MC AI Live
Vì Live không được giới thiệu/chốt SKU không sellable hoặc bị sale lock.

PHASE 8 - IVR
Vì IVR chỉ xác nhận Official Order hợp lệ, không xác nhận đơn có sản phẩm bị stop-sale.

PHASE 9 - Global Smoke / Release Gateway
Vì Operational Core thiếu evidence/smoke thì không Release Readiness.

## 13. EVIDENCE REQUIRED

Agent phải gom evidence analysis gồm:

Source-of-truth đã đọc.

File/module Source / Raw đã inspect.

File/module QC đã inspect.

File/module Production đã inspect.

File/module Material Issue / Receipt đã inspect.

File/module Batch / Release đã inspect.

File/module Warehouse / Inventory đã inspect.

File/module Traceability / QR đã inspect.

File/module Recall / Sale Lock đã inspect.

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

Evidence trong PROMPT-P2 là Evidence Submitted, chưa phải Evidence Accepted.

## 14. SMOKE REQUIRED - CHỈ ĐỀ XUẤT, CHƯA VIẾT TEST

Agent phải đề xuất smoke cho phase implementation sau:

Smoke ID

Scenario

Expected Result

## P2-SMOKE-001

Raw lot QC_PASS nhưng chưa READY_FOR_PRODUCTION

Không được issue

## P2-SMOKE-002

Raw lot READY_FOR_PRODUCTION

Được phép issue nếu đủ guard

## P2-SMOKE-003

Material Issue retry cùng idempotency key

Không double debit

## P2-SMOKE-004

Material Receipt sau issue

Không giảm tồn lần hai

## P2-SMOKE-005

Production Order tạo từ recipe

Snapshot formula/version/ingredient/quantity/UOM

## P2-SMOKE-006

Batch QC_PASS nhưng chưa RELEASED

Warehouse không được nhận

## P2-SMOKE-007

Batch RELEASED

Warehouse receipt được phép nếu đủ guard

## P2-SMOKE-008

Warehouse Receipt retry

Không double credit

## P2-SMOKE-009

Inventory Ledger

Append-only, không update/delete

## P2-SMOKE-010

## QR VOID/FAILED

Public Trace không valid

## P2-SMOKE-011

Public Trace response

Whitelist-only, không lộ private field

## P2-SMOKE-012

Recall/Sale Lock active

Commerce/AI/Gateway/Ads/Live/IVR bị chặn

## P2-SMOKE-013

Runtime unavailable

Fail-safe, không suy đoán

## P2-SMOKE-014

Evidence SUBMITTED dùng cho release/recall close

Không pass nếu cần ACCEPTED

Không gọi smoke đề xuất này là Global Smoke Pass.

## 15. EXECUTION STEPS

Agent phải làm theo thứ tự:

Đọc TECH source-of-truth.

Đọc PHASE 0 validation report.

Đọc PHASE 1 smoke/evidence report.

Inspect codebase thật.

Tìm Source / Raw Material module.

Tìm Raw Lot / QC / Readiness module.

Tìm Production Order module.

Tìm Material Request / Issue / Receipt module.

Tìm Batch / QC / Release module.

Tìm Warehouse / Inventory module.

Tìm Traceability / QR module.

Tìm Recall / Sale Lock module.

Tìm audit/evidence/idempotency usage trong operational commands.

Tìm các chỗ QC_PASS đang được dùng.

Tìm các chỗ READY_FOR_PRODUCTION đang được dùng.

Tìm các chỗ RELEASED đang được dùng.

Tìm các chỗ Warehouse Receipt đang được dùng.

Tìm các chỗ Public Trace đang expose dữ liệu.

Tìm các chỗ Recall/Sale Lock đang chặn downstream.

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

Prompt đã chạy: PROMPT-P2.

Mode: ANALYSIS ONLY.

Scope đã phân tích.

Current implementation state tổng quan.

P2 điểm chặn chính.

Gap chính.

Downstream impact chính.

Trạng thái cuối.

Không được ghi PHASE 2 Complete / Release Readiness / Production Readiness / Go-live Decision.

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

PHASE 1 Smoke / Evidence Report.

TECH Operational Core nếu có.

Legacy docs nếu có đọc, ghi rõ LEGACY_REFERENCE_ONLY.

## 16.4. Mục 4 - Evidence đã dùng

Liệt kê:

Evidence ID

Type

Source

Domain

Status

Limitation

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

Test Operational Core hiện có.

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

Có seed Source / Raw / Production / Inventory không.

Seed có đúng source-of-truth không.

Seed có hardcode sai không.

Seed có tạo trạng thái production/release sai không.

Seed có hỗ trợ smoke không.

Không sửa seed.

## 16.13. Mục 13 - Rủi ro còn lại

Phân nhóm:

Raw Lot Readiness risk.

Production Order Snapshot risk.

Material Issue risk.

Material Receipt risk.

Inventory Ledger risk.

Batch QC / Release risk.

Warehouse Receipt risk.

Traceability / Public Trace risk.

Recall / Sale Lock risk.

Audit / Evidence / Idempotency risk.

Downstream PHASE 3/4/5 risk.

Release Gateway risk.

Global Gate risk.

## 16.14. Mục 14 - Cập nhật handoff

Phải ghi:

Gap cần xử lý ở PHASE 2 Technical Design.

điểm chặn cần clear trước implementation.

Evidence cần chuẩn bị.

Smoke cần viết/chạy.

Prompt tiếp theo đề xuất.

Điều kiện để được chuyển sang P2.1.

Trạng thái cuối.

Bắt buộc ghi:

## 17. DONE GATE

PROMPT-P2 chỉ được xem là analysis done khi đủ:

Đã đọc source-of-truth.

Đã đọc PHASE 0 report.

Đã đọc PHASE 1 report.

Đã inspect codebase thật.

Đã phân tích Source / Raw Material.

Đã phân tích Raw Lot / QC / Readiness.

Đã phân tích Production Order / Formula Snapshot.

Đã phân tích Material Request / Issue / Receipt.

Đã phân tích Batch / QC / Release.

Đã phân tích Warehouse / Inventory Ledger.

Đã phân tích Traceability / QR / Public Trace.

Đã phân tích Recall / Sale Lock.

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

Không gọi PHASE 2 Complete / Release Readiness / Production Readiness / Go-live Decision.

Global Gate vẫn bị chặn.

Trạng thái tối đa được phép:

## PHASE 2 ANALYSIS REPORT COMPLETED

Không được gọi:

PHASE 2 Complete.

Implementation Complete.

Completion Decision.

Release Readiness.

Production Readiness.

Go-live Decision.

Gateway PASS.

## 18. FAIL GATE

PROMPT-P2 phải fail nếu:

Agent sửa file.

Agent tạo code.

Agent tạo migration.

Agent tạo seed.

Agent update database.

Agent không inspect codebase.

Agent không đọc source-of-truth.

Agent không phân tích QC_PASS vs READY_FOR_PRODUCTION.

Agent gọi Raw Lot QC_PASS là READY_FOR_PRODUCTION.

Agent bỏ qua Material Issue / Material Receipt double debit risk.

Agent bỏ qua Inventory Ledger append-only.

Agent gọi Batch QC_PASS là RELEASED.

Agent bỏ qua Warehouse Receipt gate.

Agent bỏ qua Public Trace whitelist-only.

Agent bỏ qua Recall / Sale Lock.

Agent không lập gap matrix.

Agent không lập conflict matrix.

Agent không lập điểm chặn register.

Agent không report đủ 14 mục.

Agent tự đổi nghiệp vụ.

Agent hardcode Operational Core policy.

Agent gọi Analysis là Implementation Complete.

Agent gọi Release Readiness / Production Readiness / Go-live Decision.

Agent mở Global Gate.

Nếu fail, agent phải ghi:

PROMPT-P2 FAIL GATE - ANALYSIS NOT ACCEPTED

## 19. DOWNSTREAM HANDOFF

## 19.1. Prompt tiếp theo nếu P2 analysis đạt Done Gate

Nếu PROMPT-P2 đạt Done Gate, prompt tiếp theo nên là:

## PROMPT-P2.1 - OPERATIONAL CORE TECHNICAL DESIGN HANDOFF

## MODE: TECHNICAL DESIGN ONLY

P2.1 sẽ chuyển gap analysis thành:

Workstream.

Task breakdown.

Dependency matrix.

Evidence plan.

Smoke plan.

Implementation sequence.

P2.1 vẫn chưa nên sửa code nếu gap còn lớn.

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

## PHASE HIỆN TẠI:

## PHASE-02 - OPERATIONAL CORE

## PROMPT HIỆN TẠI:

## PROMPT-P2 - OPERATIONAL CORE / PRODUCTION / WAREHOUSE / INVENTORY / TRACEABILITY / RECALL / SALE LOCK ANALYSIS HANDOFF

## MODE:

## ANALYSIS ONLY

## PROMPT TIẾP THEO:

## PROMPT-P2.1 - OPERATIONAL CORE TECHNICAL DESIGN HANDOFF

Bạn chỉ được phân tích.

Bạn không được sửa file.

Bạn không được tạo code.

Bạn không được tạo migration.

Bạn không được tạo seed.

Bạn không được update database.

Bạn không được tự đổi nghiệp vụ.

Bạn không được hardcode policy.

Bạn không được gọi Raw Lot QC_PASS là READY_FOR_PRODUCTION.

Bạn không được gọi Batch QC_PASS là RELEASED.

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

PHASE 1 Smoke / Evidence Report.

TECH Operational Core nếu có.

Quy tắc:

TECH mới thắng code cũ.

TECH mới thắng tài liệu cũ.

Code hiện tại chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Legacy chỉ là LEGACY_REFERENCE_ONLY.

Nếu conflict thì báo cáo, không sửa.

B. Scope In

Bạn phải phân tích:

Source Origin.

Raw Material.

Raw Material Receipt.

Raw Lot.

Incoming QC.

Raw Lot Readiness.

Production Order.

Formula Snapshot.

Material Request.

Material Issue.

Material Receipt.

Production Batch.

Batch QC.

Batch Release.

Warehouse Receipt.

Inventory Ledger.

Inventory Balance Projection.

Traceability / QR / Public Trace.

Recall / Hold / Sale Lock.

Operational audit/evidence/idempotency.

C. Scope Out

Bạn không được:

Sửa code.

Tạo code.

Tạo migration.

Tạo seed.

Tạo API.

Tạo UI.

Tạo production order.

Issue material.

Receipt material.

Release batch.

Confirm warehouse receipt.

Public trace dữ liệu.

Kích hoạt recall/sale lock.

Sửa Commerce / AI Advisor / Gateway.

Gọi PHASE 2 Complete.

Mở Global Gate.

D. Phân tích bắt buộc

Bạn phải kiểm tra:

RAW_LOT QC_PASS có bị dùng thay READY_FOR_PRODUCTION không.

Raw lot chưa READY_FOR_PRODUCTION có bị issue không.

Material Issue có phải điểm giảm tồn nguyên liệu chính không.

Material Receipt có giảm tồn lần hai không.

Production Order có snapshot formula kind/version/ingredient/quantity/UOM không.

Batch QC_PASS có bị dùng thay RELEASED không.

Warehouse chỉ nhận batch RELEASED không.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed không.

Inventory Ledger có append-only không.

Public Trace có whitelist-only không.

QR VOID/FAILED có bị public-valid không.

Recall / Sale Lock có thắng Commerce / AI / Ads / Live / CRM / Gateway / IVR không.

E. Báo cáo bắt buộc

Bạn phải lập:

Current Implementation State Matrix.

Gap Analysis Matrix.

Conflict Detection Matrix.

P2 điểm chặn Register.

Downstream Impact.

Smoke Required.

Report 14 mục.

F. Report format 14 mục

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

G. Done Gate

Chỉ được coi là analysis done nếu:

Đọc source-of-truth.

Inspect codebase thật.

Phân tích đầy đủ Source / Raw / QC / Production / Material / Batch / Warehouse / Inventory / Traceability / Recall.

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

H. Fail Gate

Phải fail nếu:

Sửa file.

Tạo code.

Tạo migration.

Tạo seed.

Gọi QC_PASS là READY_FOR_PRODUCTION.

Gọi QC_PASS là RELEASED.

Không phân tích Inventory Ledger append-only.

Không phân tích Recall / Sale Lock.

Không inspect codebase.

Không lập gap/conflict/điểm chặn.

Gọi Release Readiness / Production Readiness / Go-live Decision.

Mở Global Gate.

I. Trạng thái cuối bắt buộc

Kết thúc report phải ghi:

## KẾT THÚC PROMPT

## 21.1. Trạng thái tài liệu

## 21.2. Trạng thái thực thi

## ANALYSIS HANDOFF ONLY

## 21.3. Phạm vi

## OPERATIONAL CORE ANALYSIS ONLY

## 21.4. Trạng thái PHASE 2

## 21.5. Trạng thái implementation

## 21.6. Trạng thái Completion

## 21.7. Trạng thái release

## 21.8. Trạng thái production

## 21.9. Trạng thái go-live

## 21.10. Trạng thái Global Gate

Global Gate: bị chặn
