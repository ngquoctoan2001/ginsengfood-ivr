# 07-P2 - SMOKE BẰNG CHỨNG VÀ NGHIỆM THU

Trạng thái: `PHASE_2_EVIDENCE_CANONICAL`  
Mode: `SMOKE / EVIDENCE HANDOFF`  
Mục tiêu: chứng minh Phase 2 đã hấp thụ đủ nguồn vận hành cũ và không còn cần mở 5 file nguồn riêng.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev triển khai hoặc consume sang Phase 3 có kiểm soát. `SMOKE`, `EVIDENCE`, `NGHIỆM THU` và `handoff-ready` trong file này là tiêu chí hồ sơ/dev handoff; chúng không phải Production Ready, Release Ready hoặc Go-live approval.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES
READY_TO_HAND_TO_DEV_OR_PHASE_3: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## 1. Source coverage

| Nguồn cũ | Nội dung đã đưa vào Phase 2 | File Phase 2 chính |
| --- | --- | --- |
| BẢNG PHIẾU LỆNH SẢN XUẤT | 12 phiếu/lệnh tối thiểu, field từng phiếu, xác nhận, evidence, kế toán | 00, 01, 02, 03, 04, 05, 07 |
| PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN | Hồ sơ sản xuất gốc, tự sinh phiếu, click/tự hiện/nhập, print, MISA boundary | 01, 03, 04, 05, 06, 07 |
| BỘ KHÓA 5 BƯỚC | Operational config, trace chain, recall, route/action, no manual ingredient | 01, 03, 04, 05, 06, 07 |
| DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ | Material/packaging group, thresholds, disposal/write-off, MRP suppression | 02, 04, 05, 07 |
| CÔNG THỨC VẬN HÀNH 20 SKU MỚI | Formula G1 mà Phase 2 snapshot/issue/trace consume | 03, 04, 05, 06, 07 |

## 2. Smoke bắt buộc

| Evidence ID | Nội dung | Expected |
| --- | --- | --- |
| EVD-P2-SRC-001 | 5 file nguồn cũ đã nhập vào file con Phase 1/2 | Nội dung nằm trực tiếp trong thân file con Phase 2 |
| EVD-P2-RAW-001 | Raw receipt tạo lot và QC | Lot có trạng thái đúng |
| EVD-P2-READY-001 | Only READY_FOR_PRODUCTION can issue | HOLD/REJECT/EXPIRED bị block |
| EVD-P2-PO-001 | Lệnh sản xuất tạo dossier | Có SKU/formula/snapshot |
| EVD-P2-BOM-001 | BOM snapshot không chọn tay | Full line tự hiện |
| EVD-P2-ISSUE-001 | Material issue debit ledger | Không double debit |
| EVD-P2-RECEIPT-001 | Workshop receipt không debit lần hai | Variance có reason |
| EVD-P2-QC-001 | QC_PASS không tự RELEASED | Release cần approval |
| EVD-P2-WH-001 | Warehouse chỉ nhận released batch | Finished goods ledger credit đúng |
| EVD-P2-TRACE-001 | Trace chain end-to-end | SKU/formula/lot/batch/warehouse nối được |
| EVD-P2-QR-001 | Public trace safe | Không lộ private/internal/MISA |
| EVD-P2-RECALL-001 | Sale lock chặn downstream | Không sellable/quote/order/promote |
| EVD-P2-MISA-001 | MISA boundary fail-closed | Không sync thật ngoài PACK-04 |

## 3. Done gate

Phase 2 handoff-ready khi:

1. 12 phiếu/lệnh tối thiểu có field và owner rõ.
2. Raw lot/readiness/material issue/batch/release/warehouse/trace chain pass.
3. Accounting/MISA boundary không vượt scope.
4. Không có Production Ready/Go-live claim khi chưa đủ evidence và owner sign-off.

## 4. Evidence map theo file con Phase 2

| File | Evidence bắt buộc | Ghi chú |
| --- | --- | --- |
| P2-00 | EVD-P2-SRC-001, EVD-P2-FLOW-001, EVD-P2-CONFLICT-001 | Source coverage và conflict rules |
| P2-01 | EVD-P2-DESIGN-001, EVD-P2-IDEMPOTENCY-001, EVD-P2-AUDIT-001 | Entity/command/event |
| P2-02 | EVD-P2-RAW-001, EVD-P2-READY-001, EVD-P2-THRESHOLD-001, EVD-P2-DISPOSAL-001 | Raw lot/readiness |
| P2-03 | EVD-P2-PO-001, EVD-P2-DOSSIER-001, EVD-P2-BOM-001, EVD-P2-AUTOFILL-001 | Dossier/snapshot |
| P2-04 | EVD-P2-REQUEST-001, EVD-P2-APPROVAL-001, EVD-P2-ISSUE-001, EVD-P2-LEDGER-001 | Cấp phát/ledger |
| P2-05 | EVD-P2-BATCH-001, EVD-P2-QC-001, EVD-P2-PACK-001, EVD-P2-WH-001 | Batch/release/kho |
| P2-06 | EVD-P2-TRACE-001, EVD-P2-QR-001, EVD-P2-RECALL-001, EVD-P2-SALELOCK-001 | Trace/QR/recall |
| P2-07 | EVD-P2-COVERAGE-001, EVD-P2-NO-GOLIVE-001 | Tổng nghiệm thu |

## 5. Checklist nghiệm thu chi tiết

1. `BẢNG PHIẾU LỆNH SẢN XUẤT.md` đã có đủ 12 phiếu/lệnh trong P2-00/P2-03/P2-07.
2. `PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md` đã được chuyển thành rule click/tự hiện/nhập tay ở P2-01 và print/accounting boundary ở P2-04/P2-05/P2-06.
3. `DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ.md` đã được đưa vào raw lot/readiness/threshold ở P2-02 và material issue/packaging ở P2-04/P2-05.
4. `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` được consume qua Production BOM Snapshot ở P2-03, không mở đường chọn tay nguyên liệu.
5. `BỘ KHÓA 5 BƯỚC.md` được dùng cho no-manual-ingredient, trace, recall, sale lock ở P2-03/P2-06.
6. Mọi gate fail-closed có expected block case.
7. Không có Bảng Gôm làm nguồn chính.
8. Không có claim Go-live/Production Ready nếu chưa có owner approval và evidence accepted.

## 6. Blocker phải giữ lại nếu chưa có owner input

| Blocker | Không được tự vượt |
| --- | --- |
| Owner chưa approve formula/packaging/output profile | Không release production-active |
| MISA mapping/credential chưa có | Không sync MISA thật |
| Public QR wording chưa duyệt | Không expose public trace |
| Recall/sale lock owner chưa duyệt | Không unlock |
| Evidence chỉ submitted chưa accepted | Không close phase |
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 07-P2-2F-SMOKE-EVIDENCE-REPORT.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P2.2F - SMOKE EVIDENCE REPORT

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-10 Evidence / Smoke / Completion Gateway.
- TECH-10 Global Smoke / UAT / Evidence.
- P2.2A den P2.2E reports va evidence.
- Warehouse chi tang ton sau batch release va receipt confirmed.

## Noi Dung Rewrite

## 34. KẾT LUẬN ĐIỀU PHỐI P2.2E

Từ thời điểm dùng bản V1.1 này, miền Traceability / QR / Recall / Sale Lock phải tuân thủ các khóa sau:

Traceability phải bám Production Dossier.

Internal genealogy phải nối raw lot -> material issue -> production dossier -> batch -> warehouse.

Trace projection không phải source-of-truth gốc.

Trace gap phải được flag.

QR phải linked batch và trace projection.

QR VOID/FAILED/bị chặn không public-valid.

Barcode/QR phải do Ginsengfood sinh theo payload/config.

Máy in chỉ nhận payload, in và trả trạng thái.

Máy in không được tự sinh batch/MFG/HSD/barcode/QR.

Print cấp 1 chỉ MFG/HSD.

Print cấp 2 batch/MFG/HSD/barcode/QR.

Reprint phải có lý do, quyền, log, audit.

Public Trace phải whitelist-only.

Public Trace không dùng Admin/Internal DTO.

Public Trace không lộ dữ liệu nội bộ.

Recall Case phải có impact analysis.

Hold/Sale Lock phải có scope/status/evidence/audit.

Sale Lock thắng Commerce/AI/Gateway/Ads/Live/CRM/IVR.

MISA không điều khiển trace/recall/sale lock.

Actor/permission/audit/idempotency/evidence là bắt buộc.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## 07-P2-2F-SMOKE-EVIDENCE-REPORT

## TÀI LIỆU SMOKE / EVIDENCE / VALIDATION REPORT CHO PHASE 2 - OPERATIONAL CORE

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Evidence Status mặc định: NOT ACCEPTED
Release Readiness: KHONG
Production Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: VALIDATION / SMOKE / EVIDENCE REPORT ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-02-P2-2F-SMOKE-EVIDENCE-REPORT

Đây là file kiểm chứng cuối của Phase 2 - Operational Core.

File này dùng để:

Gom toàn bộ smoke test cần có cho Phase 2.

Gom toàn bộ evidence cần nộp cho Phase 2.

Kiểm tra kết quả của P2.2A, P2.2B, P2.2C, P2.2D, P2.2E.

Kiểm tra các rule lõi từ nguồn tổng hợp.

Kiểm tra các rule operational core đã khóa.

Kiểm tra các boundary không được vi phạm.

Phân loại điểm chặn P0/P1/P2.

Phân loại evidence status.

Định nghĩa report format 15 mục.

Khóa điều kiện chuyển Owner Review.

Khóa điều kiện chưa được chuyển Phase 3 implementation.

Khóa Global Gate vẫn bị chặn.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 07-P2-2F-SMOKE-EVIDENCE-REPORT

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

## P2.2F

Loại tài liệu

Smoke / Evidence / Validation Report

Nguồn chính

README Phase 2 V1.1 + P2.1 + P2.2A-P2.2E

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Cho phép sửa feature code?

Không

Cho phép tạo migration mới?

Không

Cho phép tạo seed mới?

Không

Cho phép gọi PASS?

Không, nếu evidence chưa accepted

Cho phép tự chuyển Phase 3?

Không

Global Gate

bị chặn

Production Readiness

KHONG

Go-live Decision

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để làm bộ kiểm chứng cuối Phase 2 sau khi toàn bộ tài liệu P2.2A-P2.2E đã được viết lại theo nguồn tổng hợp.

Phase 2 là lõi vận hành sản xuất - kho - truy xuất - thu hồi. Nếu không có smoke/evidence đầy đủ, không được giao dev/Codex/Copilot tự tuyên bố hoàn tất, càng không được chuyển sang Phase 3 Commerce Runtime.

Mục tiêu chính:

Kiểm chứng Source / Raw / QC / Readiness.

Kiểm chứng Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Kiểm chứng Raw lot chỉ được issue khi READY_FOR_PRODUCTION.

Kiểm chứng Sâm Savigin là Company Source / strategic reserve.

Kiểm chứng Production Order tự hiện SKU + formula code + formula version.

Kiểm chứng Production Order có Formula / Recipe / BOM Snapshot.

Kiểm chứng Production Order sinh Production Dossier.

Kiểm chứng Production Dossier là root data cho các phiếu sau.

Kiểm chứng không chọn tay nguyên liệu ở Production Order và Material Request.

Kiểm chứng Material Issue là điểm giảm tồn nguyên liệu chính.

Kiểm chứng Material Receipt / Workshop Receipt không giảm tồn lần hai.

Kiểm chứng Raw Inventory Ledger append-only.

Kiểm chứng Batch QC_PASS không đồng nghĩa RELEASED.

Kiểm chứng Warehouse chỉ nhận Batch RELEASED.

Kiểm chứng Warehouse Receipt confirmed mới tạo Finished Goods Ledger Credit.

Kiểm chứng Warehouse Receipt không tự set Sellable.

Kiểm chứng Print cấp 1 / cấp 2 đúng rule.

Kiểm chứng máy in không tự sinh mã.

Kiểm chứng Reprint có reason / permission / log / audit.

Kiểm chứng Public Trace whitelist-only.

Kiểm chứng QR VOID / FAILED / bị chặn không public-valid.

Kiểm chứng Recall / Sale Lock chặn downstream.

Kiểm chứng MISA không điều khiển vận hành.

Kiểm chứng Disposal / Write-off không xóa tay tồn kho.

Kiểm chứng actor / permission / audit / idempotency / evidence trên toàn Phase 2.

Khóa Evidence Submitted chưa phải Evidence Accepted.

Khóa Phase 2 Done chưa phải Release Readiness / Production Readiness / Go-live Decision.

## 4. SOURCE-OF-TRUTH - THỨ BẬC NGUỒN SỰ THẬT

Tầng

Nguồn

Vai trò

Được dùng như thế nào

Không được làm

Tầng 0

MASTER Governance

Governance cao nhất

Giữ no-bypass, no-parallel-source, no-PASS-without-evidence

Không ghi đè bằng smoke local

Tầng 1

README Phase 2 V1.1

Điều phối Phase 2

Xác định scope, boundary, execution order

Không thay smoke detail

Tầng 2

P2.1 Technical Design V1.1

Thiết kế workstream

Cung cấp form registry, field behavior, matrix

Không tự cho phép PASS

Tầng 3

## P2.2A V1.1

Source / Raw / QC / Readiness

Smoke nhóm A

Không thay P2.2A bằng report này

Tầng 4

## P2.2B V2.0

Production Order / Dossier / Snapshot

Smoke nhóm B

Không sửa Production Order trong report

Tầng 5

## P2.2C V1.1

Material Request / Issue / Receipt / Ledger

Smoke nhóm C

Không sửa ledger trong report

Tầng 6

## P2.2D V2.0

Batch / QC / Release / Warehouse / Packaging

Smoke nhóm D/P

Không tự release hoặc warehouse

Tầng 7

## P2.2E V1.1

Trace / QR / Recall / Sale Lock

Smoke nhóm E

Không mở downstream

Tầng 8

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu smoke cho phiếu, dựa trên, MISA, print, disposal

Không code/seed trực tiếp

Tầng 9

Smoke result / log / screenshot / report

Evidence submitted

Dùng để Owner/Reviewer đánh giá

Không tự biến thành accepted evidence

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm smoke/evidence cho:

Source origin.

Raw material intake.

Raw material receipt.

Raw lot.

Incoming QC.

Raw lot readiness.

## READY_FOR_PRODUCTION.

Sâm Savigin Company Source.

Disposal raw material.

Production Order.

Formula Snapshot.

Production Dossier.

Workshop Handoff.

Material Request.

Material Approval.

Material Issue.

Raw Inventory Ledger Debit.

Material Receipt / Workshop Receipt.

No double debit.

Inventory Ledger append-only.

Accounting / MISA handoff boundary.

Production Batch.

Batch Process.

Freeze-dry QC.

Packaging Level 1.

Packaging Level 2.

Finished QC.

Batch Release.

Warehouse Receipt.

Finished Goods Ledger Credit.

No double credit.

No Sellable boundary.

Traceability / genealogy.

QR / Barcode.

Public Trace.

Reprint.

Recall.

Hold.

Sale Lock.

Downstream suppression.

MISA boundary.

Disposal/write-off.

Actor / Permission / Audit / Idempotency / Evidence.

Report 15 mục.

Final gateway lock.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không sửa feature code.

Không thêm feature mới.

Không tạo migration mới.

Không tạo seed mới.

Không sửa business logic.

Không sửa Commerce Runtime.

Không triển khai AI Advisor.

Không triển khai Facebook Gateway.

Không triển khai Ads/Live/CRM/IVR.

Không triển khai full MISA Integration.

Không tạo QuoteSnapshot.

Không tạo Cart.

Không tạo Official Order.

Không xử lý Payment.

Không xử lý Verified Revenue.

Không tự mở Phase 3.

Không gọi Completion Decision.

Không gọi Global Smoke Pass.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Không gọi Gateway PASS.

Tài liệu này chỉ là validation / smoke / evidence report specification.

## 7. EVIDENCE STATUS

Trạng thái

Ý nghĩa

Có được dùng để PASS không?

## NOT_SUBMITTED

Chưa nộp evidence

Không

## FOUND

Tìm thấy evidence thô

Không

## SUBMITTED

Đã nộp evidence

Không

## UNDER_REVIEW

Đang được Owner/Reviewer xem

Không

## NEEDS_REVIEW

Cần review

Không

## NEEDS_RETEST

Cần chạy lại smoke/test

Không

## MISSING

Thiếu evidence

Không

## REJECTED_BY_SMOKE

Smoke tự đánh fail

Không

## REJECTED_BY_REVIEW

Reviewer từ chối

Không

## ACCEPTED

Evidence đã được Owner/Reviewer chấp nhận

Có thể dùng cho gate liên quan

## SUPERSEDED

Evidence cũ bị thay thế

Không

bị chặn

Bị chặn vì thiếu dependency/điểm chặn

Không

Nguyên tắc:

Evidence Submitted chưa phải Evidence Accepted.

Smoke Passed cục bộ chưa phải Global Smoke Pass.

Phase 2 Validation Report chưa phải Release Readiness.

Phase 2 Done chưa phải Production Readiness.

## PHẦN 2/4 - SMOKE MATRIX PHASE 2

## 8. SMOKE GROUP A - SOURCE / RAW / QC / READINESS

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-A01

Tạo Raw Material hợp lệ từ master

Raw Material/Material reference được dùng hợp lệ

## EVD-P2-A-RAW-MATERIAL

## P2F-SMOKE-A02

Tạo Raw Material Receipt hợp lệ

Receipt được tạo

## EVD-P2-A-RAW-RECEIPT

## P2F-SMOKE-A03

Raw Receipt click chọn nguyên liệu từ master

Canonical material tự hiện

## EVD-P2-A-MASTER-SELECTION

## P2F-SMOKE-A04

Raw Receipt nhập material text tự do

Bị reject

## EVD-P2-A-NO-FREE-MATERIAL

## P2F-SMOKE-A05

Raw Receipt thiếu UOM/quantity

Bị reject

## EVD-P2-A-UOM-QUANTITY

## P2F-SMOKE-A06

Tạo Raw Lot từ receipt hợp lệ

Raw Lot CREATED/WAITING_QC

## EVD-P2-A-RAW-LOT

## P2F-SMOKE-A07

Raw Lot không linked receipt

Bị reject

## EVD-P2-A-RAW-LOT-LINK

## P2F-SMOKE-A08

Incoming QC PASS

QC PASS, lot chưa tự READY_FOR_PRODUCTION

EVD-P2-A-QC-PASS-NOT-READY

## P2F-SMOKE-A09

Incoming QC FAIL/HOLD/QUARANTINE

Lot không READY_FOR_PRODUCTION

EVD-P2-A-QC-FAIL-HOLD

## P2F-SMOKE-A10

Raw lot QC_PASS + đủ điều kiện readiness

Mark READY_FOR_PRODUCTION

## EVD-P2-A-READINESS

## P2F-SMOKE-A11

Raw lot HOLD/REJECTED/QUARANTINE mark READY

Bị reject

## EVD-P2-A-READINESS-BLOCK

## P2F-SMOKE-A12

Raw lot chưa READY_FOR_PRODUCTION issue eligibility

Không pass issue eligibility

## EVD-P2-A-ISSUE-ELIGIBILITY

## P2F-SMOKE-A13

Sâm Savigin intake

## EVD-P2-A-SAM-COMPANY-SOURCE

## P2F-SMOKE-A14

Sâm Savigin vào supplier purchase thường

Bị chặn

## EVD-P2-A-SAM-NO-SUPPLIER

## P2F-SMOKE-A15

Raw material rejected/expired/damaged

Kích hoạt disposal/write-off route

## EVD-P2-A-DISPOSAL

## P2F-SMOKE-A16

Xóa tay raw lot/tồn kho lỗi

Bị chặn

## EVD-P2-A-NO-MANUAL-DELETE

## P2F-SMOKE-A17

MISA mutate raw lot/QC/readiness

Bị chặn

## EVD-P2-A-MISA-BOUNDARY

## P2F-SMOKE-A18

Retry create receipt same key/payload

Không duplicate

## EVD-P2-A-IDEMPOTENCY

## P2F-SMOKE-A19

Same key different payload

Conflict

## EVD-P2-A-IDEMPOTENCY-CONFLICT

## P2F-SMOKE-A20

Command thiếu actor/permission

## DENY/BLOCK

## EVD-P2-A-RBAC-AUDIT

## 9. SMOKE GROUP B - PRODUCTION ORDER / SNAPSHOT / DOSSIER / WORKSHOP

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-B01

Chọn SKU hợp lệ

Formula code/version tự hiện

## EVD-P2-B-SKU-FORMULA

## P2F-SMOKE-B02

SKU thiếu formula active

Production Order bị reject/block

## EVD-P2-B-MISSING-FORMULA

## P2F-SMOKE-B03

Formula thiếu version

Bị reject

## EVD-P2-B-FORMULA-VERSION

## P2F-SMOKE-B04

Formula thiếu ingredient lines

Bị reject

## EVD-P2-B-FORMULA-LINES

## P2F-SMOKE-B05

Formula line chỉ có group mơ hồ

Bị reject

## EVD-P2-B-NO-GROUP-ONLY

## P2F-SMOKE-B06

Anchor-based formula có gạo anchor

Anchor tự hiện

## EVD-P2-B-ANCHOR

## P2F-SMOKE-B07

Nhập anchor quantity

Scaled formula lines được tính

## EVD-P2-B-SCALING

## P2F-SMOKE-B08

Tạo Production Order hợp lệ

Production Order được tạo

## EVD-P2-B-PRODUCTION-ORDER

## P2F-SMOKE-B09

Production Order snapshot

Có formula kind/version/ingredient/quantity/UOM

## EVD-P2-B-SNAPSHOT

## P2F-SMOKE-B10

Recipe/BOM sửa sau khi tạo order

Snapshot cũ không đổi

## EVD-P2-B-SNAPSHOT-IMMUTABLE

## P2F-SMOKE-B11

User chọn tay nguyên liệu trong order

Bị chặn

## EVD-P2-B-NO-MANUAL-INGREDIENT

## P2F-SMOKE-B12

User sửa ratio_to_rice trong order

Bị chặn

## EVD-P2-B-NO-RATIO-EDIT

## P2F-SMOKE-B13

Production Order created

Production Dossier tự sinh

## EVD-P2-B-DOSSIER

## P2F-SMOKE-B14

Production Dossier thiếu snapshot

Bị chặn

## EVD-P2-B-DOSSIER-SNAPSHOT

## P2F-SMOKE-B15

Workshop Handoff

Handoff package có dossier/snapshot

## EVD-P2-B-WORKSHOP-HANDOFF

## P2F-SMOKE-B16

Workshop Handoff

Không debit inventory

## EVD-P2-B-NO-DEBIT

## P2F-SMOKE-B17

Production Order created

Không tự tạo batch

## EVD-P2-B-NO-BATCH

## P2F-SMOKE-B18

Production Order active

Không tự set Sellable

## EVD-P2-B-NO-SELLABLE

## P2F-SMOKE-B19

Retry create order same key/payload

Không duplicate

## EVD-P2-B-IDEMPOTENCY

## P2F-SMOKE-B20

Same key different payload

Conflict

## EVD-P2-B-IDEMPOTENCY-CONFLICT

## 10. SMOKE GROUP C - MATERIAL REQUEST / ISSUE / RECEIPT / LEDGER

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-C01

Tạo Material Request từ Production Dossier

Request được tạo

## EVD-P2-C-MATERIAL-REQUEST

## P2F-SMOKE-C02

Material Request tự hiện nguyên liệu theo Formula Snapshot

Lines tự hiện

## EVD-P2-C-AUTO-LINES

## P2F-SMOKE-C03

User chọn tay nguyên liệu ở Material Request

Bị chặn

## EVD-P2-C-NO-MANUAL-INGREDIENT

## P2F-SMOKE-C04

Material Request thiếu Production Dossier

Bị reject

## EVD-P2-C-DOSSIER-REQUIRED

## P2F-SMOKE-C05

Material Request dùng formula hiện tại thay snapshot

Bị chặn

## EVD-P2-C-SNAPSHOT-ONLY

## P2F-SMOKE-C06

Material Approval linked Request

Approval được tạo

## EVD-P2-C-MATERIAL-APPROVAL

## P2F-SMOKE-C07

Approval thêm ingredient ngoài request

Bị chặn

## EVD-P2-C-APPROVAL-INHERITANCE

## P2F-SMOKE-C08

Approval adjustment thiếu reason

Bị reject

## EVD-P2-C-ADJUSTMENT-REASON

## P2F-SMOKE-C09

Issue raw lot READY_FOR_PRODUCTION

Issue được confirm

## EVD-P2-C-READY-ISSUE

## P2F-SMOKE-C10

Issue raw lot QC_PASS chưa READY

Bị reject

EVD-P2-C-QC-PASS-NOT-READY

## P2F-SMOKE-C11

Issue raw lot HOLD/REJECTED/QUARANTINE

Bị reject

## EVD-P2-C-HOLD-BLOCK

## P2F-SMOKE-C12

Issue sai ingredient so với approval

Bị reject

## EVD-P2-C-INGREDIENT-MATCH

## P2F-SMOKE-C13

Issue thành công

Tạo Raw Inventory Ledger Debit

## EVD-P2-C-RAW-LEDGER-DEBIT

## P2F-SMOKE-C14

Retry Issue same key/payload

Không double debit

## EVD-P2-C-NO-DOUBLE-DEBIT

## P2F-SMOKE-C15

Same key different payload

Conflict

## EVD-P2-C-IDEMPOTENCY-CONFLICT

## P2F-SMOKE-C16

Ledger update/delete attempt

Bị chặn

## EVD-P2-C-LEDGER-APPEND-ONLY

## P2F-SMOKE-C17

Workshop Receipt linked Issue

Receipt được tạo

## EVD-P2-C-WORKSHOP-RECEIPT

## P2F-SMOKE-C18

Workshop Receipt thành công

Không tạo raw ledger debit lần hai

## EVD-P2-C-NO-SECOND-DEBIT

## P2F-SMOKE-C19

Receipt quantity lệch issue

Cần variance reason

## EVD-P2-C-VARIANCE

## P2F-SMOKE-C20

Receipt quantity lớn hơn issue

Bị reject/block

## EVD-P2-C-OVER-RECEIPT-BLOCK

## P2F-SMOKE-C21

Phiếu kế toán xuất nguyên liệu

Có handoff boundary, MISA không điều khiển

## EVD-P2-C-MISA-HANDOFF

## P2F-SMOKE-C22

Disposal từ variance/hư hỏng

Có request/review/write-off flow

## EVD-P2-C-DISPOSAL

## P2F-SMOKE-C23

Material Issue confirmed

Không tự tạo batch/warehouse/sellable

## EVD-P2-C-NO-BATCH-WAREHOUSE-SELLABLE

## P2F-SMOKE-C24

Command thiếu actor/permission

## DENY/BLOCK

## EVD-P2-C-RBAC-AUDIT

## P2F-SMOKE-C25

Evidence required nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## EVD-P2-C-EVIDENCE-STATE

## 11. SMOKE GROUP D - BATCH / QC / RELEASE / WAREHOUSE / INVENTORY

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-D01

Tạo Batch từ Production Dossier hợp lệ

Batch được tạo

## EVD-P2-D-BATCH

## P2F-SMOKE-D02

Tạo Batch không có Production Dossier

Bị reject

## EVD-P2-D-DOSSIER-REQUIRED

## P2F-SMOKE-D03

Batch dựa trên SKU/formula snapshot

Dữ liệu tự hiện

## EVD-P2-D-BATCH-INHERITANCE

## P2F-SMOKE-D04

Batch nhập SKU/formula khác dossier

Bị chặn

## EVD-P2-D-NO-REENTER

## P2F-SMOKE-D05

Batch process sơ chế

Step linked batch

## EVD-P2-D-PREPROCESS

## P2F-SMOKE-D06

Batch process sấy thăng hoa

Step linked batch, chưa QC_PASS

## EVD-P2-D-FREEZE-DRY

## P2F-SMOKE-D07

QC sau sấy PASS

Chưa RELEASED

## EVD-P2-D-POST-DRY-QC

## P2F-SMOKE-D08

Batch QC PASS

Batch QC_PASSED_WAITING_RELEASE

EVD-P2-D-QC-PASS-NOT-RELEASED

## P2F-SMOKE-D09

Batch QC FAIL/HOLD/REJECT

Không release được

EVD-P2-D-QC-FAIL-HOLD

## P2F-SMOKE-D10

Release batch QC_PASS + guard pass

Batch RELEASED

## EVD-P2-D-BATCH-RELEASE

## P2F-SMOKE-D11

Warehouse Receipt batch QC_PASS chưa RELEASED

Bị reject

EVD-P2-D-WH-QC-PASS-BLOCK

## P2F-SMOKE-D12

Warehouse Receipt batch RELEASED

Receipt confirmed

## EVD-P2-D-WH-RELEASED

## P2F-SMOKE-D13

Warehouse Receipt confirmed

Tạo Finished Goods Ledger Credit

## EVD-P2-D-FG-LEDGER-CREDIT

## P2F-SMOKE-D14

Retry Warehouse Receipt same key/payload

Không double credit

## EVD-P2-D-NO-DOUBLE-CREDIT

## P2F-SMOKE-D15

Same key different payload

Conflict

## EVD-P2-D-IDEMPOTENCY-CONFLICT

## P2F-SMOKE-D16

Warehouse Receipt confirmed

Không tự set Sellable

## EVD-P2-D-NO-SELLABLE

## P2F-SMOKE-D17

Finished Goods Ledger update/delete attempt

Bị chặn

## EVD-P2-D-FG-LEDGER-APPEND-ONLY

## P2F-SMOKE-D18

Disposal batch/FG reject

Có request/review/write-off

## EVD-P2-D-DISPOSAL

## P2F-SMOKE-D19

Xóa tay batch/FG/packaging

Bị chặn

## EVD-P2-D-NO-MANUAL-DELETE

## P2F-SMOKE-D20

MISA mutate batch/warehouse/ledger

Bị chặn

## EVD-P2-D-MISA-BOUNDARY

## 12. SMOKE GROUP P - PACKAGING / PRINT LEVEL 1 / PRINT LEVEL 2

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-P01

Đóng gói cấp 1 linked batch

Packaging L1 được tạo

## EVD-P2-P-PACKAGING-L1

## P2F-SMOKE-P02

Đóng gói cấp 1

Chỉ payload MFG/HSD

## EVD-P2-P-PRINT-L1

## P2F-SMOKE-P03

Đóng gói cấp 2 linked batch

Packaging L2 được tạo

## EVD-P2-P-PACKAGING-L2

## P2F-SMOKE-P04

Đóng gói cấp 2

Payload batch/MFG/HSD/barcode/QR

## EVD-P2-P-PRINT-L2

## P2F-SMOKE-P05

Máy in nhận payload

Máy in chỉ in và trả trạng thái

## EVD-P2-P-PRINTER-PAYLOAD

## P2F-SMOKE-P06

Máy in tự sinh batch/MFG/HSD/barcode/QR

Bị chặn

## EVD-P2-P-NO-PRINTER-CODE

## P2F-SMOKE-P07

Print payload FAILED

QR/Public Trace không valid

## EVD-P2-P-PRINT-FAILED

## P2F-SMOKE-P08

Print payload VOID

QR/Public Trace không valid

## EVD-P2-P-PRINT-VOID

## P2F-SMOKE-P09

Reprint request hợp lệ

Có reason/permission/audit

## EVD-P2-P-REPRINT

## P2F-SMOKE-P10

Reprint không reason

Bị reject

## EVD-P2-P-REPRINT-REASON

## 13. SMOKE GROUP E - TRACEABILITY / QR / PUBLIC TRACE / RECALL / SALE LOCK

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-E01

Build internal genealogy

Raw lot -> issue -> dossier -> batch -> warehouse

## EVD-P2-E-GENEALOGY

## P2F-SMOKE-E02

Missing genealogy link

Trace gap flagged

## EVD-P2-E-TRACE-GAP

## P2F-SMOKE-E03

Build trace projection hợp lệ

Projection VALID nếu đủ dữ liệu

## EVD-P2-E-TRACE-PROJECTION

## P2F-SMOKE-E04

Trace projection thiếu dữ liệu

Projection PARTIAL/bị chặn

## EVD-P2-E-PROJECTION-BLOCK

## P2F-SMOKE-E05

Generate QR linked batch

QR được tạo và linked

## EVD-P2-E-QR-LINK

## P2F-SMOKE-E06

QR linked trace projection

Có trace_projection_ref

## EVD-P2-E-QR-TRACE

## P2F-SMOKE-E07

## QR VOID

Public Trace không valid

## EVD-P2-E-QR-VOID

## P2F-SMOKE-E08

## QR FAILED

Public Trace không valid

## EVD-P2-E-QR-FAILED

## P2F-SMOKE-E09

QR bị chặn

Public Trace không valid

EVD-P2-E-QR-bị chặn

## P2F-SMOKE-E10

Public Trace response

Whitelist-only

## EVD-P2-E-PUBLIC-WHITELIST

## P2F-SMOKE-E11

Public Trace private field attempt

Không lộ private/internal field

## EVD-P2-E-NO-PRIVATE-FIELD

## P2F-SMOKE-E12

Public Trace dùng Admin DTO

Bị chặn

## EVD-P2-E-NO-ADMIN-DTO

## P2F-SMOKE-E13

Recall Case OPEN

Case được tạo

## EVD-P2-E-RECALL-CASE

## P2F-SMOKE-E14

Recall Impact Analysis

Xác định impacted batch/QR/SKU

## EVD-P2-E-RECALL-IMPACT

## P2F-SMOKE-E15

Recall từ raw lot

Truy ra affected batch/QR nếu có

## EVD-P2-E-RAW-RECALL

## P2F-SMOKE-E16

Hold active

Target bị chặn theo scope

## EVD-P2-E-HOLD

## P2F-SMOKE-E17

Sale Lock active

Target bị suppression

## EVD-P2-E-SALE-LOCK

## P2F-SMOKE-E18

Sale Lock active downstream

Commerce/AI/Gateway/Ads/Live/CRM/IVR bị chặn ở boundary

## EVD-P2-E-DOWNSTREAM-SUPPRESSION

## P2F-SMOKE-E19

Sale Lock release thiếu approval

Bị reject

## EVD-P2-E-LOCK-RELEASE-GUARD

## P2F-SMOKE-E20

MISA mutate trace/recall/sale lock

Bị chặn

## EVD-P2-E-MISA-BOUNDARY

## 14. SMOKE GROUP X - CROSS-CUTTING CONTROL

Smoke ID

Case

Expected

Evidence

## P2F-SMOKE-X01

High-risk command thiếu actor

## DENY / BLOCK

## EVD-P2-X-ACTOR

## P2F-SMOKE-X02

High-risk command thiếu permission

## DENY

## EVD-P2-X-PERMISSION

## P2F-SMOKE-X03

Command require evidence nhưng SUBMITTED

Không pass nếu policy cần ACCEPTED

## EVD-P2-X-EVIDENCE-STATE

## P2F-SMOKE-X04

Same idempotency key + same payload

Không tạo side effect lần hai

## EVD-P2-X-IDEMPOTENCY-SAME

## P2F-SMOKE-X05

Same idempotency key + different payload

Conflict

## EVD-P2-X-IDEMPOTENCY-CONFLICT

## P2F-SMOKE-X06

Runtime dependency unavailable

Fail-safe, không suy đoán

## EVD-P2-X-FAILSAFE

## P2F-SMOKE-X07

Audit event for success/deny

Có actor/correlation/action/result

## EVD-P2-X-AUDIT

## P2F-SMOKE-X08

Evidence Submitted bị gọi Accepted

Bị chặn

## EVD-P2-X-NO-SUBMITTED-AS-ACCEPTED

## P2F-SMOKE-X09

Local smoke pass bị gọi Global Smoke Pass

Bị chặn

EVD-P2-X-NO-GLOBAL-PASS

## P2F-SMOKE-X10

Gateway status

Global Gate vẫn bị chặn

EVD-P2-X-GATEWAY-bị chặn

PHẦN 3/4 - EVIDENCE PACKAGE / điểm chặn REGISTER / RISK REGISTER

## 15. EVIDENCE PACKAGE TỐI THIỂU PHASE 2

Phase 2 cần gom tối thiểu các nhóm evidence sau:

P2 Analysis Report.

P2.1 Technical Design Report.

P2.2A Source / Raw / QC / Readiness evidence.

P2.2B Production Order / Production Dossier / Snapshot evidence.

P2.2C Material Request / Approval / Issue / Receipt / Ledger evidence.

P2.2D Batch / Release / Warehouse / Inventory / Packaging evidence.

P2.2E Traceability / QR / Recall / Sale Lock evidence.

P2.2F Smoke Result Matrix.

File changes summary.

Git diff summary nếu có.

Build result.

Test result.

Smoke result.

Migration validation nếu có.

Seed validation nếu có.

Raw Lot QC_PASS != READY_FOR_PRODUCTION evidence.

Raw Lot READY_FOR_PRODUCTION issue eligibility evidence.

Sâm Savigin Company Source evidence.

Production Order formula auto-display evidence.

Production Order snapshot evidence.

Production Dossier root evidence.

No manual ingredient selection evidence.

Material Request auto-lines evidence.

Material Issue no double debit evidence.

Material Receipt no second debit evidence.

Raw Inventory Ledger append-only evidence.

Batch QC_PASS != RELEASED evidence.

Warehouse only accepts RELEASED batch evidence.

Finished Goods Ledger Credit on warehouse confirmed evidence.

No double credit evidence.

No Sellable from warehouse evidence.

Print level 1 payload evidence.

Print level 2 payload evidence.

Printer no-code-generation evidence.

Reprint control evidence.

Public Trace whitelist-only evidence.

QR VOID/FAILED/bị chặn not public-valid evidence.

Recall Impact Analysis evidence.

Sale Lock downstream suppression evidence.

MISA boundary evidence.

Disposal/write-off evidence.

Actor/permission/audit/idempotency evidence.

Open điểm chặn register.

Risk register.

Gateway bị chặn evidence.

Owner review checklist.

Đề xuất tiếp theo.

## 16. EVIDENCE REGISTER TEMPLATE

Evidence ID

Smoke ID

Workstream

Nội dung evidence

Nguồn

Trạng thái

Reviewer

Ghi chú

## EVD-P2-A-RAW-RECEIPT

## P2F-SMOKE-A02

## P2.2A

Raw receipt hợp lệ

Log/screenshot/report

## SUBMITTED

Owner/Reviewer

Chưa accepted

## EVD-P2-B-SNAPSHOT

## P2F-SMOKE-B09

## P2.2B

Formula snapshot

Log/screenshot/report

## SUBMITTED

Owner/Reviewer

Chưa accepted

## EVD-P2-C-NO-DOUBLE-DEBIT

## P2F-SMOKE-C14

## P2.2C

Không double debit

Test/log

## SUBMITTED

Owner/Reviewer

Chưa accepted

## EVD-P2-D-NO-DOUBLE-CREDIT

## P2F-SMOKE-D14

## P2.2D

Không double credit

Test/log

## SUBMITTED

Owner/Reviewer

Chưa accepted

## EVD-P2-E-DOWNSTREAM-SUPPRESSION

## P2F-SMOKE-E18

## P2.2E

Sale Lock suppression

Test/report

## SUBMITTED

Owner/Reviewer

Chưa accepted

EVD-P2-X-GATEWAY-bị chặn

## P2F-SMOKE-X10

Cross-cutting

Gateway bị chặn

Config/report

## SUBMITTED

Owner/Reviewer

Chưa accepted

Trạng thái mặc định khi gom evidence là SUBMITTED hoặc NEEDS_REVIEW, không phải ACCEPTED.

## 17. EVIDENCE ACCEPTANCE RULES

Evidence chỉ được xem là ACCEPTED khi có đủ:

Evidence ID.

Smoke ID liên quan.

Workstream liên quan.

Mô tả case.

Dữ liệu đầu vào.

Kết quả đầu ra.

Log/screenshot/report phù hợp.

Môi trường test nếu có.

Người/system nộp evidence.

Thời điểm nộp.

Reviewer.

Review result.

Kết luận accepted/rejected/retest.

Nếu fail trước đó, có retest evidence.

Audit/correlation nếu command rủi ro.

Không hợp lệ nếu:

Chỉ nói “đã chạy”.

Không có log/screenshot/report.

Không biết thuộc smoke nào.

Không có reviewer.

Không có kết luận review.

Evidence còn SUBMITTED nhưng gọi ACCEPTED.

Evidence bị superseded nhưng vẫn dùng để pass.

Evidence không khớp version tài liệu/handoff.

## 18. điểm chặn REGISTER P0 TOÀN PHASE 2

Các lỗi sau là P0 điểm chặn:

Mã

điểm chặn

## P2-P0-001

Raw Lot QC_PASS bị dùng làm READY_FOR_PRODUCTION

## P2-P0-002

Raw lot chưa READY_FOR_PRODUCTION vẫn issue được

## P2-P0-003

Phiếu nhập nguyên liệu cho nhập material tự do không qua master

## P2-P0-004

Sâm Savigin bị xử lý như supplier material thường

## P2-P0-005

Production Order không snapshot formula/BOM

## P2-P0-006

Production Order cho chọn tay nguyên liệu

## P2-P0-007

Production Order không sinh Production Dossier

## P2-P0-008

Production Dossier không là root data cho phiếu sau

## P2-P0-009

Material Request cho chọn tay nguyên liệu

## P2-P0-010

Material Request không dựa trên Formula Snapshot

## P2-P0-011

Material Approval thêm nguyên liệu ngoài request

## P2-P0-012

Material Issue không tạo Raw Ledger Debit

## P2-P0-013

Material Issue retry tạo double debit

## P2-P0-014

Material Receipt / Workshop Receipt tạo debit lần hai

## P2-P0-015

Inventory Ledger update/delete được

## P2-P0-016

Batch không linked Production Dossier

## P2-P0-017

Batch QC_PASS bị dùng làm RELEASED

## P2-P0-018

Warehouse nhận Batch chưa RELEASED

## P2-P0-019

Finished Goods Ledger Credit tạo trước Warehouse Receipt confirmed

## P2-P0-020

Warehouse Receipt retry tạo double credit

## P2-P0-021

Warehouse Receipt tự set Sellable

## P2-P0-022

Print cấp 1 in sai rule MFG/HSD

## P2-P0-023

Print cấp 2 thiếu batch/MFG/HSD/barcode/QR

## P2-P0-024

Máy in tự sinh batch/MFG/HSD/barcode/QR

## P2-P0-025

Reprint không có reason/permission/log/audit

## P2-P0-026

Public Trace không whitelist-only

## P2-P0-027

Public Trace lộ private/internal field

## P2-P0-028

QR VOID/FAILED/bị chặn public-valid

## P2-P0-029

Recall Case không có Impact Analysis

## P2-P0-030

Sale Lock active nhưng downstream vẫn bán/chốt/thúc mua

## P2-P0-031

MISA điều khiển production/formula/batch/trace/recall/print

## P2-P0-032

Disposal/write-off bằng cách xóa tay tồn kho

## P2-P0-033

SUBMITTED evidence được xem là ACCEPTED

## P2-P0-034

Runtime unavailable nhưng hệ thống vẫn suy đoán

## P2-P0-035

Global Gate không còn bị chặn

## P2-P0-036

Smoke fail nhưng vẫn gọi done

## P2-P0-037

Test pass bị gọi Global Smoke Pass

## P2-P0-038

Phase 2 Done bị gọi Release Readiness

## P2-P0-039

Phase 2 Done bị gọi Production Readiness

## P2-P0-040

Phase 2 Done bị gọi Go-live Decision

## 19. điểm chặn REGISTER TEMPLATE

điểm chặn ID

Workstream

Mô tả

Mức độ

Evidence

Trạng thái

Hành động

## P2-P0-001

## P2.2A

QC_PASS bị dùng làm READY

## P0

EVD liên quan

OPEN/bị chặn

Quay lại P2.2A

## P2-P0-013

## P2.2C

Retry tạo double debit

## P0

EVD liên quan

OPEN/bị chặn

Quay lại P2.2C

## P2-P0-018

## P2.2D

Warehouse nhận batch chưa release

## P0

EVD liên quan

OPEN/bị chặn

Quay lại P2.2D

## P2-P0-030

## P2.2E

Sale Lock không chặn downstream

## P0

EVD liên quan

OPEN/bị chặn

Quay lại P2.2E

Không được chuyển Phase 3 nếu P0 còn OPEN.

## 20. RISK REGISTER PHASE 2

Risk

Mức độ

Nguyên nhân

Cách kiểm soát

Phase 2 bị dùng như Commerce Runtime

## P0

Nhầm operational truth với sellable truth

No Sellable boundary

Raw lot chưa READY vẫn issue

## P0

Sai readiness guard

P2.2A/P2.2C smoke

Công thức snapshot sai

## P0

Dùng formula động

P2.2B snapshot smoke

Double debit

## P0

Issue/receipt cùng debit

Ledger/idempotency smoke

Double credit

## P0

Retry warehouse receipt

FG ledger/idempotency smoke

Batch QC_PASS tự RELEASED

## P0

Gộp QC và release

Batch release smoke

Print sai cấp

## P0

Không khóa payload level 1/2

Print smoke

Public Trace lộ nội bộ

## P0

DTO sai

Public whitelist smoke

Sale Lock bị bypass

## P0

Downstream không tiêu thụ lock

Suppression smoke

MISA mutate vận hành

## P0

Sai integration boundary

MISA boundary evidence

Disposal xóa tay tồn kho

## P0

Thiếu write-off flow

Disposal smoke

Evidence submitted bị gọi accepted

## P0

Sai evidence governance

Evidence state rules

Agent sửa lan khi validation

## P1

Mode không khóa

P2.2F no-feature-change rule

Report thiếu mục

## P1

Không theo format 15 mục

Report gate

PHẦN 4/4 - REPORT FORMAT / EXECUTION ORDER / DONE GATE / FAIL GATE

## 21. EXECUTION ORDER P2.2F

P2.2F chỉ được chạy sau khi có report/evidence của:

## P2.2A.

## P2.2B.

## P2.2C.

## P2.2D.

## P2.2E.

Thứ tự xử lý P2.2F:

Thứ tự

Việc cần làm

Output

Đọc README Phase 2 V1.1

Hiểu phase boundary

Đọc P2.1 Technical Design V1.1

Hiểu workstream matrix

Đọc P2.2A report/evidence

Validate Source/Raw/QC/Readiness

Đọc P2.2B report/evidence

Validate Production/Snapshot/Dossier

Đọc P2.2C report/evidence

Validate Material Issue/Receipt/Ledger

Đọc P2.2D report/evidence

Validate Batch/QC/Release/Warehouse

Đọc P2.2E report/evidence

Validate Trace/QR/Recall/Sale Lock

Lập Smoke Result Matrix

PASS/FAIL/bị chặn/NOT_RUN

Lập Evidence Register

Evidence status

10

Lập Open điểm chặn Register

## P0/P1/P2

11

Lập Risk Register

Rủi ro còn lại

12

Lập Missing Evidence List

Evidence còn thiếu

13

Lập Owner Review Checklist

Gói review

14

Lập Report 15 mục

Handoff

15

Đề xuất tiếp theo

## PLAN_ONLY / RETEST / OWNER_DECISION / P3_ANALYSIS_ALLOWED

## 22. SMOKE RESULT MATRIX TEMPLATE

Smoke ID

Workstream

Case

Expected

Actual

Result

Evidence ID

Ghi chú

## P2F-SMOKE-A08

## P2.2A

QC PASS chưa READY

QC_PASS nhưng chưa READY

## TBD

NOT_RUN / PASS / FAIL / bị chặn

EVD-P2-A-QC-PASS-NOT-READY

## P2F-SMOKE-C14

## P2.2C

Retry Issue không double debit

Không double debit

## TBD

NOT_RUN / PASS / FAIL / bị chặn

## EVD-P2-C-NO-DOUBLE-DEBIT

## P2F-SMOKE-D14

## P2.2D

Retry Warehouse không double credit

Không double credit

## TBD

NOT_RUN / PASS / FAIL / bị chặn

## EVD-P2-D-NO-DOUBLE-CREDIT

## P2F-SMOKE-E18

## P2.2E

Sale Lock downstream

Downstream bị chặn

## TBD

NOT_RUN / PASS / FAIL / bị chặn

## EVD-P2-E-DOWNSTREAM-SUPPRESSION

Result hợp lệ:

PASS.

FAIL.

bị chặn.

## NOT_RUN.

## NEEDS_RETEST.

NOT_APPLICABLE nếu có lý do rõ.

Không dùng:

## GLOBAL_PASS.

## COMPLETION_PASS.

## PRODUCTION_READINESS.

## GO_LIVE_DECISION_ACCEPTED.

## 23. REPORT FORMAT 15 MỤC BẮT BUỘC

Mọi report Phase 2, đặc biệt P2.2F, phải có đủ 15 mục:

## 1. Tóm tắt

Tóm tắt phạm vi validation, workstream đã kiểm, trạng thái tổng quát.

## 2. File đã sửa

Danh sách file đã sửa nếu có.
Với P2.2F, mặc định không sửa feature code.
Nếu có sửa tài liệu/report, phải ghi rõ.

## 3. Nguồn yêu cầu

Liệt kê tài liệu nguồn:

README Phase 2 V1.1.

P2.1 Technical Design V1.1.

## P2.2A-P2.2E.

nguồn tổng hợp auxiliary source.

Owner instruction nếu có.

## 4. Evidence đã dùng

Liệt kê evidence ID, nguồn evidence, trạng thái evidence.

## 5. Lệnh đã chạy

Liệt kê command build/test/smoke nếu có.
Nếu chưa chạy, ghi rõ NOT_RUN và lý do.

## 6. Kết quả test

Unit/integration/e2e nếu có.
Nếu chưa có, ghi rõ thiếu.

## 7. Kết quả backend build

Backend build result.
Nếu không áp dụng, ghi NOT_APPLICABLE.

## 8. Kết quả frontend build

Frontend build result.
Nếu không áp dụng, ghi NOT_APPLICABLE.

## 9. Kết quả cleanup process

Lint/format/cleanup nếu có.
Nếu không áp dụng, ghi NOT_APPLICABLE.

## 10. Cập nhật Markdown/tài liệu

Tài liệu nào đã cập nhật hoặc chưa cập nhật.

## 11. Kết quả database migration/update nếu áp dụng

Migration đã chạy/chưa chạy.
Nếu không áp dụng, ghi NOT_APPLICABLE.

## 12. Kết quả seed validation nếu áp dụng

Seed validation result.
Nếu không có seed, ghi NOT_APPLICABLE.

## 13. Rủi ro còn lại

P0/P1/P2 còn mở.
Không được bỏ trống nếu còn điểm chặn.

## 14. Cập nhật handoff

Trạng thái bàn giao, workstream nào cần quay lại.

## 15. Đề xuất tiếp theo

Chọn một trong các hướng:

## RETEST_REQUIRED.

## OWNER_CONFIRM_REQUIRED.

## FIX_P2_2A.

## FIX_P2_2B.

## FIX_P2_2C.

## FIX_P2_2D.

## FIX_P2_2E.

## P3_ANALYSIS_PROMPT_ALLOWED.

## PLAN_ONLY.

## STOP_BLOCKED.

Nếu thiếu mục 15, report fail.

## 24. DEV / CODEX / COPILOT HANDOFF

## 24.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY

Agent không được:

Sửa feature code.

Tạo migration mới.

Tạo seed mới.

Tạo API mới.

Tạo UI mới.

Tạo worker mới.

Mở Gateway.

Tự chuyển Phase 3.

## 24.2. Prompt đúng cho Agent

## MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY.

Nhiệm vụ:

Đọc reports/evidence của P2.2A-P2.2E.

Lập Smoke Result Matrix.

Lập Evidence Register.

Lập Missing Evidence List.

Lập Open điểm chặn Register.

Lập Risk Register.

Kiểm tra report 15 mục.

Kiểm tra Global Gate vẫn bị chặn.

Đề xuất bước tiếp theo ở mục 15.

Không sửa feature code.

Không tạo migration.

Không tạo seed.

Không gọi Completion Decision / Gateway PASS / Release Readiness / Production Readiness.

## 25. OWNER REVIEW CHECKLIST

Trước khi Owner Review, phải có:

Checklist

Trạng thái

P2.2A report

## REQUIRED

P2.2A evidence

## REQUIRED

P2.2B report

## REQUIRED

P2.2B evidence

## REQUIRED

P2.2C report

## REQUIRED

P2.2C evidence

## REQUIRED

P2.2D report

## REQUIRED

P2.2D evidence

## REQUIRED

P2.2E report

## REQUIRED

Smoke Result Matrix

## REQUIRED

Evidence Register

## REQUIRED

Open điểm chặn Register

## REQUIRED

Risk Register

## REQUIRED

Gateway bị chặn evidence

## REQUIRED

Missing Evidence List

REQUIRED nếu có thiếu

Retest Plan

REQUIRED nếu có fail

Owner Confirm List

REQUIRED nếu còn decision

Đề xuất tiếp theo

## REQUIRED

## 26. DONE GATE P2.2F

P2.2F được xem là đạt ở mức Validation Report Ready for Owner Review khi:

Đã gom đủ reports P2.2A-P2.2E.

Đã lập Smoke Result Matrix.

Đã lập Evidence Register.

Đã lập Missing Evidence List.

Đã lập Open điểm chặn Register.

Đã lập Risk Register.

Đã kiểm tra Source / Raw / QC / Readiness smoke.

Đã kiểm tra Production / Snapshot / Dossier smoke.

Đã kiểm tra Material Issue / Receipt / Ledger smoke.

Đã kiểm tra Batch / QC / Release / Warehouse smoke.

Đã kiểm tra Packaging / Print smoke.

Đã kiểm tra Trace / QR / Public Trace smoke.

Đã kiểm tra Recall / Sale Lock / Downstream suppression smoke.

Đã kiểm tra MISA boundary smoke.

Đã kiểm tra Disposal/write-off smoke.

Đã kiểm tra actor/permission/audit/idempotency/evidence smoke.

P0 điểm chặn không bị bỏ qua.

Evidence status không bị gọi sai.

Report có đủ 15 mục.

Global Gate vẫn bị chặn.

Không gọi Completion Decision.

Không gọi Global Smoke Pass.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Có đề xuất tiếp theo rõ ràng.

Trạng thái tối đa được phép ghi:

## PHASE 2 VALIDATION REPORT SUBMITTED FOR OWNER REVIEW

hoặc:

## PHASE 2 EVIDENCE PACKAGE READY FOR OWNER REVIEW

Không được ghi:

PHASE 2 COMPLETE / Completion Decision / Production Readiness / Go-live Decision

## 27. FAIL GATE P2.2F

P2.2F fail nếu xảy ra một trong các trường hợp:

Thiếu report P2.2A.

Thiếu report P2.2B.

Thiếu report P2.2C.

Thiếu report P2.2D.

Thiếu report P2.2E.

Thiếu Smoke Result Matrix.

Thiếu Evidence Register.

Thiếu Open điểm chặn Register.

Thiếu Risk Register.

Thiếu mục “Đề xuất tiếp theo”.

Report không đủ 15 mục.

P0 điểm chặn còn OPEN nhưng vẫn đề xuất chuyển Phase 3.

Evidence SUBMITTED bị gọi ACCEPTED.

Smoke local pass bị gọi Global Smoke Pass.

P2.2F sửa feature code ngoài scope.

P2.2F tạo migration mới.

P2.2F tạo seed mới.

Raw Lot QC_PASS bị dùng làm READY mà không flag P0.

Material Issue double debit không flag P0.

Material Receipt second debit không flag P0.

Batch QC_PASS = RELEASED không flag P0.

Warehouse nhận batch chưa RELEASED không flag P0.

Print cấp 1/cấp 2 sai rule không flag P0.

Public Trace lộ nội bộ không flag P0.

QR VOID/FAILED public-valid không flag P0.

Sale Lock downstream bypass không flag P0.

MISA mutate vận hành không flag P0.

Disposal xóa tay tồn kho không flag P0.

Global Gate không còn bị chặn.

Gọi Completion Decision.

Gọi Gateway PASS.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 2 - OPERATIONAL CORE

Workstream

P2.2F - Smoke / Evidence / Validation Report

Implementation authorization by this document

KHONG

Code authorization by this document

KHONG

Migration authorization by this document

KHONG

Seed authorization by this document

KHONG

Feature change authorization

KHONG

Evidence status mặc định

## NOT ACCEPTED

Gateway status

bị chặn

Global Smoke Pass

KHONG

Completion Decision

KHONG

Release Readiness

KHONG

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

Phase 2 Owner Review / hoặc PROMPT-P3 - COMMERCE RUNTIME ANALYSIS HANDOFF nếu Owner cho phép

## 29. KẾT LUẬN ĐIỀU PHỐI P2.2F

Từ thời điểm dùng bản V1.1 này, Phase 2 chỉ được xem là có thể đưa vào Owner Review khi:

Đã có đủ smoke matrix.

Đã có đủ evidence register.

Đã có điểm chặn register.

Đã có risk register.

Đã có report 15 mục.

Đã phân loại evidence đúng trạng thái.

Không gọi Submitted là Accepted.

Không gọi smoke local là Global Smoke Pass.

Không gọi Phase 2 Done là Release Readiness.

Không gọi Phase 2 Done là Production Readiness.

Không gọi Phase 2 Done là Go-live Decision.

Global Gate vẫn bị chặn.

Sau tài liệu này, nếu Owner/dev lead cho phép, bước tiếp theo chỉ được là:

## PROMPT-P3 - COMMERCE RUNTIME ANALYSIS HANDOFF

và Phase 3 vẫn phải bắt đầu bằng:

## ANALYSIS ONLY

Không được tự động chuyển sang code Phase 3.

## README-PHASE-02-HANDOFF-INDEX

## TÀI LIỆU ĐIỀU PHỐI GIAO DEV PHASE 2 - OPERATIONAL CORE

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Production Readiness: KHONG
Release Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: ANALYSIS ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-02-OPERATIONAL-CORE-HANDOFF-INDEX

Đây là file README điều phối chính của Phase 2.

Phase 2 tập trung vào lõi vận hành sản xuất - kho - truy xuất - thu hồi của Ginsengfood.

Chuỗi vận hành chính của Phase 2:

Source / Raw Material -> Raw Material Receipt -> Raw Lot -> Incoming QC -> READY_FOR_PRODUCTION -> Production Order -> Production Dossier -> Formula Snapshot -> Material Request -> Material Approval -> Material Issue -> Material Receipt / Workshop Receipt -> Batch -> Batch Process -> Batch QC -> Batch Release -> Finished Goods Warehouse Receipt -> Inventory Ledger -> Traceability -> QR / Public Trace -> Recall / Hold / Sale Lock

Phase 2 là nơi tạo operational truth cho các phase sau.

Phase 2 không phải Commerce Runtime.

Phase 2 không quyết định Sellable.

Phase 2 không tạo giá bán.

Phase 2 không tạo QuoteSnapshot.

Phase 2 không tạo Cart / Order / Payment.

Phase 2 không xử lý verified revenue.

Phase 2 chỉ cung cấp sự thật vận hành để Phase 3 Commerce Runtime và các kênh AI/Gateway/Ads/Live/CRM/IVR tiêu thụ đúng.

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## README-PHASE-02-HANDOFF-INDEX

Phiên bản

Phase

## PHASE 2 - OPERATIONAL CORE

Loại tài liệu

README / Handoff Index / Execution Order / Gateway Lock

Nguồn chính

Bộ tài liệu Phase 2 bản sạch

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Tài liệu điều phối trước

## README-PHASE-01-02-AUXILIARY-SOURCE-MAPPING

Checklist liên quan

## MASTER-DATA-NORMALIZATION-CHECKLIST-PHASE-01-02

Runbook liên quan

## DEV-RUNBOOK-PHASE-01-02-START-HERE

Cho phép code ngay?

Không

Cho phép migration ngay?

Không

Cho phép seed ngay?

Không

Cho phép copy-paste code từ AI?

Không

Mode mặc định cho Agent

## ANALYSIS ONLY

Global Gate

bị chặn

Production Readiness

KHONG

Go-live Decision

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để làm cổng điều phối chính cho toàn bộ Phase 2 - Operational Core.

Bản README cũ đã khóa nhiều nguyên tắc quan trọng như:

Không nhảy phase.

Không bỏ qua Analysis.

Không bỏ qua Technical Design.

Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt không giảm tồn lần hai.

Batch QC_PASS không đồng nghĩa RELEASED.

Warehouse chỉ nhận batch RELEASED.

Public Trace whitelist-only.

QR VOID/FAILED không public-valid.

Recall/Sale Lock thắng downstream.

Global Gate vẫn bị chặn.

Tuy nhiên, sau khi có nguồn tổng hợp TÀI LIỆU.docx, Phase 2 cần được viết lại để bám sát hơn các nghiệp vụ vận hành thực tế, đặc biệt:

12 phiếu/lệnh vận hành tối thiểu.

Quy tắc tự sinh phiếu.

Quy tắc dựa trên dữ liệu giữa các phiếu.

Ranh giới click chọn / tự hiện / nhập tay.

Công thức sản xuất tự hiện theo SKU + mã công thức + version.

Không cho người dùng chọn tay nguyên liệu ở lệnh sản xuất và phiếu cấp nguyên liệu.

Phiếu nhập nguyên liệu đầu vào là nơi được click chọn nguyên liệu từ master.

Lệnh sản xuất sinh hồ sơ sản xuất gốc.

Hồ sơ sản xuất gốc là nguồn dữ liệu duy nhất cho chuỗi phiếu sau.

Phiếu đề nghị cấp nguyên liệu tự hiện nguyên liệu theo công thức.

Phiếu chấp thuận nguyên liệu đưa vào sản xuất dựa trên từ phiếu đề nghị.

Phiếu kế toán xuất nguyên liệu cho sản xuất có ngõ MISA/kế toán.

Batch / sơ chế / sấy / đóng gói cấp 1 / đóng gói cấp 2 / QC thành phẩm / nhập kho thành phẩm phải tự dựa trên dữ liệu.

In mã sản phẩm chia 2 cấp: cấp 1 chỉ in MFG/HSD; cấp 2 in batch/MFG/HSD/barcode/QR.

Máy in chỉ nhận payload, in và trả trạng thái.

Máy in không được tự sinh mã sản xuất, mã lô, ngày sản xuất, hạn dùng, barcode, QR.

Reprint phải có lý do, quyền, log và audit.

Ginsengfood tự tính vận hành/giá thành nội bộ; MISA hạch toán chính thức.

MISA không điều khiển lệnh sản xuất, công thức, batch, traceability, recall, print logic.

Disposal/write-off không được xóa tay tồn kho.

Mục tiêu của README này:

Khóa Phase 2 thành một chuỗi vận hành có kiểm soát, đủ rõ để dev/Codex/Copilot phân tích và triển khai theo từng file sạch, nhưng chưa cho phép gọi Completion Decision, Release Readiness, Production Readiness hoặc Go-live Decision.

## 4. SOURCE-OF-TRUTH - THỨ BẬC NGUỒN SỰ THẬT CỦA PHASE 2

Tầng

Nguồn

Vai trò

Dev/Agent được dùng như thế nào

Không được làm

Tầng 0

MASTER Governance / nguyên tắc đã khóa

Nguồn quản trị cao nhất

Giữ no-bypass, no-parallel-source, no-PASS-without-evidence

Không dùng Phase 2 để ghi đè governance

Tầng 1

## README-PHASE-02-HANDOFF-INDEX V1.1

Cổng điều phối Phase 2

Biết phạm vi, thứ tự file, gate, boundary

Không thay thế file chi tiết P2.2A-P2.2F

Tầng 2

File Phase 2 bản sạch

Nguồn giao dev theo từng domain

Dùng từng file để Analysis Only / Technical Design / Limited Implementation khi được mở

Không dùng file cũ nếu đã có bản sạch mới

Tầng 3

## README-PHASE-01-02-AUXILIARY-SOURCE-MAPPING

Điều phối nguồn tổng hợp vào Phase 1/2

Xác định nguồn tổng hợp ảnh hưởng file nào

Không code trực tiếp

Tầng 4

## MASTER-DATA-NORMALIZATION-CHECKLIST-PHASE-01-02

Kiểm soát dữ liệu trước seed/code

Biết dữ liệu Product/SKU/Recipe/Packaging/Threshold nào cần Owner confirm

Không tự điền dữ liệu thiếu

Tầng 5

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ thực tế

Dùng để đối chiếu và viết lại tài liệu sạch

Không code/seed trực tiếp

Tầng 6

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không coi code là source-of-truth nếu conflict với tài liệu đã khóa

## 5. DANH SÁCH FILE PHASE 2

Bộ Phase 2 gồm các file sau:

## STT

File

Vai trò

Trạng thái theo V1.1

README-PHASE-02-HANDOFF-INDEX.docx

File điều phối Phase 2

## 00-P2-ANALYSIS-ONLY.docx

Phân tích hiện trạng Operational Core

Giữ tạm nếu không phát hiện conflict

## 01-P2-1-TECHNICAL-DESIGN-ONLY.docx

Thiết kế kỹ thuật Phase 2

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS.docx

Source / Raw Lot / QC / Readiness

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx

Production Order / Production Dossier / Formula Snapshot / Workshop

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx

Material Request / Approval / Issue / Receipt / Ledger

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx

Batch / QC / Release / Warehouse / Inventory / Packaging

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx

Traceability / QR / Public Trace / Recall / Sale Lock

## 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx

Smoke / Evidence / Report Phase 2

## 6. SCOPE IN - PHẠM VI BAO GỒM CỦA PHASE 2

Phase 2 bao gồm:

Source origin.

Raw material master consumption.

Phiếu nhập nguyên liệu đầu vào.

Raw material receipt.

Raw lot.

Incoming QC.

Raw lot readiness.

READY_FOR_PRODUCTION gate.

Sâm Savigin Company Source / harvest / strategic reserve boundary.

Production order.

Production dossier / hồ sơ sản xuất gốc.

Formula / recipe / BOM snapshot.

Workshop handoff.

Material request / phiếu đề nghị cấp nguyên liệu.

Material approval / phiếu chấp thuận nguyên liệu đưa vào sản xuất.

Accounting material issue handoff / phiếu kế toán xuất nguyên liệu cho sản xuất.

Material issue.

Raw inventory ledger debit.

Material receipt / workshop receipt.

No second debit boundary.

Production batch.

Batch process state.

Check-in/check-out nhân sự sản xuất nếu thuộc scope vận hành.

Phiếu sơ chế.

Phiếu QC sau sấy thăng hoa.

Phiếu đóng gói cấp 1.

Phiếu đóng gói cấp 2.

Phiếu QC thành phẩm.

Batch QC.

Batch release.

Finished goods warehouse receipt.

Finished goods ledger credit.

Inventory balance projection.

Print cấp 1.

Print cấp 2.

Print payload.

Reprint control.

Traceability / genealogy.

## QR.

Public Trace whitelist.

Recall.

Hold / Sale Lock.

Downstream suppression boundary.

Disposal / write-off.

Operational audit / evidence / idempotency / actor / permission.

Phase 2 smoke/evidence/report.

## 7. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM CỦA PHASE 2

Phase 2 không bao gồm:

Không triển khai Commerce Runtime.

Không quyết định Sellable.

Không tạo giá bán.

Không tạo QuoteSnapshot.

Không tạo Cart.

Không tạo Official Order.

Không xử lý Payment.

Không xử lý Verified Revenue.

Không xử lý Commission.

Không triển khai AI Advisor runtime.

Không triển khai Facebook Gateway runtime.

Không triển khai Ads Measurement.

Không triển khai MC AI Live.

Không triển khai IVR.

Không triển khai full MISA integration.

Không để MISA điều khiển vận hành.

Không triển khai full MRP/procurement nếu chưa mở phase riêng.

Không tạo phiếu thu mua đầy đủ nếu chưa có MRP/Procurement phase.

Không tự mở Gateway.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Phase 2 chỉ tạo operational truth và boundary để các phase sau tiêu thụ.

PHẦN 2/4 - nguồn tổng hợp MAPPING / BOUNDARY RULES / OPERATIONAL FORM REGISTRY

## 8. nguồn tổng hợp ĐƯỢC DÙNG NHƯ THẾ NÀO TRONG PHASE 2

nguồn tổng hợp TÀI LIỆU.docx chỉ được dùng trong Phase 2 theo vai trò:

Auxiliary source - nguồn phụ trợ nghiệp vụ vận hành.

nguồn tổng hợp không được dùng để code trực tiếp.

nguồn tổng hợp không được dùng để seed trực tiếp.

nguồn tổng hợp không được giao cho Agent để tự suy luận DB/API/UI.

Nội dung từ nguồn tổng hợp phải được chuẩn hóa vào các file Phase 2 bản sạch theo đúng thứ tự.

## 9. MAPPING nguồn tổng hợp VÀO FILE PHASE 2

Nội dung từ nguồn tổng hợp

Ảnh hưởng đến Phase 2

File tiêu thụ chính

Cách dùng đúng

12 phiếu/lệnh vận hành tối thiểu

Cần mapping form/command/states/evidence

README Phase 2, 01-P2-1, 07-P2-2F

Đưa vào Operational Form Registry

Tự sinh phiếu

Chuỗi phiếu không tạo tay rời rạc

## 01-P2-1, 03-P2-2B, 04-P2-2C, 05-P2-2D

Khóa trigger/state machine

dựa trên dữ liệu giữa phiếu

Phiếu sau không nhập lại dữ liệu nguồn

## 03-P2-2B, 04-P2-2C, 05-P2-2D

Khóa inherited fields

Click chọn / tự hiện / nhập tay

UI/API phải phân rõ quyền nhập dữ liệu

## 01-P2-1, 02-P2-2A, 03-P2-2B, 04-P2-2C, 05-P2-2D

Khóa field behavior

Công thức tự hiện theo SKU + version

Production Order phải lấy formula snapshot

## 03-P2-2B

Không chọn tay nguyên liệu

Phiếu nhập nguyên liệu đầu vào được click chọn nguyên liệu master

Raw receipt là điểm hợp lệ để chọn material

## 02-P2-2A

Không cho nhập material tự do

Lệnh sản xuất sinh hồ sơ sản xuất gốc

Production dossier là root data

## 03-P2-2B

Phiếu sau dựa trên dossier

Phiếu đề nghị cấp nguyên liệu tự hiện theo công thức

Material request không chọn tay nguyên liệu

## 04-P2-2C

Lấy từ production dossier snapshot

Phiếu chấp thuận nguyên liệu dựa trên từ phiếu đề nghị

Approval không thêm nguyên liệu ngoài đề nghị

## 04-P2-2C

Khóa approval source

Material Issue là điểm giảm tồn

Ledger debit chỉ ở issue

## 04-P2-2C

Chống double debit

Material Receipt / Workshop Receipt không giảm tồn lần hai

Receipt chỉ xác nhận xưởng nhận

## 04-P2-2C

Không tạo debit lần hai

Phiếu kế toán xuất nguyên liệu có ngõ MISA

Có handoff kế toán/sync boundary

## 04-P2-2C, future MISA pack

MISA không điều khiển vận hành

Sơ chế/sấy/đóng gói/QC/nhập kho dựa trên dữ liệu

Downstream forms không nhập rời

## 05-P2-2D

dựa trên production dossier/batch

Print cấp 1/cấp 2

Tách payload in

## 05-P2-2D, 06-P2-2E

Cấp 1 MFG/HSD, cấp 2 batch/MFG/HSD/barcode/QR

Máy in chỉ nhận payload

Printer không sinh mã

## 06-P2-2E

Ginsengfood sinh dữ liệu in

Reprint có lý do/quyền/log/audit

Reprint là action rủi ro

## 06-P2-2E

Kiểm soát traceability

MISA boundary

MISA hạch toán chính thức, không điều khiển vận hành

README Phase 2, 01-P2-1, 04-P2-2C

Có integration layer riêng

Ngưỡng tồn kho / loại khỏi phiếu thu mua

Phase 2 chỉ giữ boundary, không full MRP

README Phase 2, future MRP pack

Không triển khai nhầm procurement full

Sâm Savigin strategic reserve

Company Source, không supplier purchase

## 02-P2-2A

Harvest/intake/QC/readiness/reserve

Disposal/write-off

Hủy phải qua request-review-execute-ledger-evidence

## 02-P2-2A, 04-P2-2C, 05-P2-2D

Không xóa tay tồn kho

## 10. OPERATIONAL FORM / COMMAND REGISTRY PHASE 2

nguồn tổng hợp có nói nhóm 12 phiếu/lệnh tối thiểu. Để giao dev an toàn, README này khóa theo nhóm vận hành tối thiểu như sau. Khi viết lại file chi tiết, Owner có thể chốt tên/mã phiếu cuối cùng, nhưng không được bỏ các nhóm kiểm soát này.

## STT

Nhóm phiếu/lệnh

Nguồn dữ liệu chính

Tự sinh / click chọn / nhập tay

File chi tiết

Phiếu tiếp nhận/đánh giá nguyên liệu đầu vào

Material master / source

Click chọn nguyên liệu từ master; nhập số lượng/chứng từ/evidence

## 02-P2-2A

Phiếu nhập nguyên liệu đầu vào / Raw Receipt

Phiếu tiếp nhận + material master

Tự tạo receipt/lot theo rule

## 02-P2-2A

Phiếu QC nguyên liệu đầu vào

Raw receipt / raw lot

dựa trên lot/material; nhập kết quả QC/evidence

## 02-P2-2A

Lệnh sản xuất

SKU + formula active

Chọn SKU; công thức/version tự hiện; không chọn tay nguyên liệu

## 03-P2-2B

Hồ sơ sản xuất gốc

Lệnh sản xuất

Tự sinh từ production order

## 03-P2-2B

Phiếu đề nghị cấp nguyên liệu

Hồ sơ sản xuất gốc + formula snapshot

Tự hiện nguyên liệu theo snapshot; không chọn tay

## 04-P2-2C

Phiếu chấp thuận nguyên liệu đưa vào sản xuất

Phiếu đề nghị cấp nguyên liệu

dựa trên đề nghị; duyệt/từ chối/điều chỉnh theo rule

## 04-P2-2C

Phiếu kế toán xuất nguyên liệu cho sản xuất

Material issue / accounting handoff

Tự sinh dữ liệu kế toán/handoff; MISA chỉ nhận sync

## 04-P2-2C

Material Issue

Approval + raw lot READY_FOR_PRODUCTION

Ghi debit nguyên liệu; không tạo batch

## 04-P2-2C

10

Material Receipt / Workshop Receipt

Material issue

Xưởng xác nhận nhận; không debit lần hai

## 04-P2-2C

11

Phiếu sơ chế / sấy / QC sau sấy

Production dossier / batch

dựa trên SKU/batch/material; nhập kết quả công đoạn

## 05-P2-2D

12

Phiếu đóng gói cấp 1

Batch + packaging profile

dựa trên batch/MFG/HSD; print cấp 1 chỉ MFG/HSD

## 05-P2-2D

13

Phiếu đóng gói cấp 2

Batch + packaging profile + QR/barcode config

dựa trên batch/MFG/HSD; print cấp 2 batch/MFG/HSD/barcode/QR

## 05-P2-2D, 06-P2-2E

14

Phiếu QC thành phẩm

Batch/packaging output

dựa trên batch; nhập QC thành phẩm/evidence

## 05-P2-2D

15

Lệnh nhập kho thành phẩm

Batch RELEASED

Chỉ nhập kho batch đã release

## 05-P2-2D

16

Phiếu disposal/write-off

Lot/batch/material/FG

Request -> review -> execute -> ledger write-off -> evidence

## 02-P2-2A, 04-P2-2C, 05-P2-2D

Ghi chú:

Nếu Owner quyết định giữ đúng số lượng 12 biểu mẫu UI, các nhóm trên có thể được gom lại trong UI.

Nhưng về mặt hệ thống, không được bỏ các checkpoint nghiệp vụ.

Không được vì giảm số lượng biểu mẫu mà làm mất audit/evidence/state.

## 11. FIELD BEHAVIOR - CLICK CHỌN / TỰ HIỆN / NHẬP TAY

Miền

Được click chọn

Phải tự hiện

Được nhập tay

Raw Receipt

Material từ master, source/supplier nếu có

Material canonical, UOM, source type

Quantity, chứng từ, evidence, ghi chú hợp lệ

Incoming QC

Raw lot cần QC

Material, lot, receipt, quantity/UOM

Kết quả QC, chỉ tiêu QC, evidence

Production Order

SKU cần sản xuất

Formula code, formula version, ingredient lines, UOM, ratio/quantity

Anchor quantity/sản lượng kế hoạch, ngày kế hoạch, ghi chú

Production Dossier

Không tạo tay rời

Tự sinh từ Production Order

Chỉ cập nhật field được phép theo state

Material Request

Không chọn tay nguyên liệu

Nguyên liệu tự hiện từ formula snapshot

Request note/variance reason nếu rule cho phép

Material Approval

Không thêm nguyên liệu ngoài request

dựa trên request lines

Approve/reject/adjust theo rule và audit

Material Issue

Raw lot READY_FOR_PRODUCTION theo request

Material, lot, UOM, requested quantity

Issued quantity trong giới hạn, reason nếu lệch

Workshop Receipt

Issue đã thực hiện

Material issue lines

Received quantity, variance reason

Batch/QC/Release

Batch từ production dossier

SKU, formula snapshot, material genealogy

QC result, release decision, evidence

Packaging

Batch + packaging profile

MFG/HSD, packaging config, print payload

Actual quantity, reject/loss reason

Print

Print job từ Ginsengfood payload

Payload in

Không nhập tay mã in

Reprint

Original print job

Payload snapshot

Reason, approval, disposition

Disposal

Lot/batch/item

Item snapshot, current balance

Reason, evidence, review decision

## 12. BOUNDARY RULES - QUY TẮC BIÊN PHASE 2

Mã

Quy tắc

Diễn giải

## P2-BR-01

Phase 2 tạo operational truth, không tạo sellable truth

Sellable thuộc Phase 3 Commerce Runtime

## P2-BR-02

nguồn tổng hợp là auxiliary source

Không code trực tiếp từ nguồn tổng hợp

## P2-BR-03

Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION

QC_PASS chỉ là một điều kiện

## P2-BR-04

Raw lot chỉ được issue khi READY_FOR_PRODUCTION

Không cấp nguyên liệu từ lot chưa READY

## P2-BR-05

Production Order phải snapshot formula/recipe/BOM

Snapshot không phụ thuộc động vào recipe hiện tại

## P2-BR-06

Production Order sinh Production Dossier

Hồ sơ sản xuất gốc là root cho phiếu sau

## P2-BR-07

Không chọn tay nguyên liệu ở Production Order

Nguyên liệu phải tự hiện từ formula/version

## P2-BR-08

Không chọn tay nguyên liệu ở Material Request

Material Request phải tự hiện từ Production Dossier

## P2-BR-09

Material Issue là điểm giảm tồn nguyên liệu chính

Raw ledger debit chỉ tại issue

## P2-BR-10

Material Receipt / Workshop Receipt không giảm tồn lần hai

Chỉ xác nhận xưởng nhận

## P2-BR-11

Inventory Ledger append-only

Không update/delete ledger bằng business flow

## P2-BR-12

Batch QC_PASS không đồng nghĩa RELEASED

Release là gate riêng

## P2-BR-13

Warehouse chỉ nhận Batch RELEASED

Không nhập kho batch chưa release

## P2-BR-14

Finished goods tăng tồn khi Warehouse Receipt confirmed

Ledger credit chỉ khi receipt confirmed

## P2-BR-15

Warehouse Receipt không tự set Sellable

Sellable thuộc Phase 3

## P2-BR-16

Public Trace whitelist-only

Không lộ field nội bộ

## P2-BR-17

QR VOID/FAILED không public-valid

QR lỗi/void không được truy xuất public hợp lệ

## P2-BR-18

Recall/Sale Lock thắng downstream

Chặn Commerce/AI/Gateway/Ads/Live/CRM/IVR

## P2-BR-19

Print payload do Ginsengfood sinh

Máy in không sinh mã

## P2-BR-20

Print cấp 1 chỉ in MFG/HSD

Không tự thêm QR/barcode nếu rule chưa khóa

## P2-BR-21

Print cấp 2 in batch/MFG/HSD/barcode/QR

Payload do hệ thống sinh

## P2-BR-22

Reprint phải có reason/permission/log/audit

Không reprint tự do

## P2-BR-23

MISA không điều khiển vận hành

MISA chỉ hạch toán chính thức qua integration layer

## P2-BR-24

Sâm Savigin là Company Source / strategic reserve

Không là supplier material thường

## P2-BR-25

Disposal/write-off không xóa tay tồn kho

Phải qua request-review-execute-ledger-evidence

## P2-BR-26

Evidence Submitted chưa phải Evidence Accepted

Không gọi PASS khi chỉ mới nộp evidence

## P2-BR-27

Global Gate vẫn bị chặn

Phase 2 không tự mở Gateway

## PHẦN 3/4 - FILE INDEX / EXECUTION ORDER / REPORT FORMAT / DOWNSTREAM IMPACT

## 13. EXECUTION ORDER - THỨ TỰ TRIỂN KHAI PHASE 2

## 13.1. Thứ tự tài liệu bắt buộc

Thứ tự

File

Mode

Mục đích

Có được sửa code không?

README-PHASE-02-HANDOFF-INDEX.docx

Handoff Index

Điều phối toàn Phase 2

Không

## 00-P2-ANALYSIS-ONLY.docx

## ANALYSIS ONLY

Phân tích hiện trạng Operational Core

Không

## 01-P2-1-TECHNICAL-DESIGN-ONLY.docx

## TECHNICAL DESIGN ONLY

Thiết kế workstream/boundary/task/smoke/evidence

Không

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS.docx

LIMITED IMPLEMENTATION nếu được mở

Source / Raw / Lot / QC / Readiness

Có giới hạn

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx

LIMITED IMPLEMENTATION nếu được mở

Production Order / Production Dossier / Snapshot / Workshop

Có giới hạn

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx

LIMITED IMPLEMENTATION nếu được mở

Material Request / Approval / Issue / Receipt / Ledger

Có giới hạn

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx

LIMITED IMPLEMENTATION nếu được mở

Batch / QC / Release / Warehouse / Inventory / Packaging

Có giới hạn

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx

LIMITED IMPLEMENTATION nếu được mở

Traceability / QR / Public Trace / Recall / Sale Lock

Có giới hạn

## 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx

## VALIDATION / SMOKE / EVIDENCE ONLY

Smoke/evidence/report Phase 2

Không thêm feature

## 13.2. Quy tắc không nhảy bước

Không được:

Nhảy từ README sang implementation.

Bỏ qua 00-P2-ANALYSIS-ONLY.

Bỏ qua 01-P2-1-TECHNICAL-DESIGN-ONLY.

Chạy P2.2A nếu chưa có Analysis Report.

Chạy P2.2B nếu P2.2A chưa có report hợp lệ.

Chạy P2.2C nếu P2.2B chưa có report hợp lệ.

Chạy P2.2D nếu P2.2C chưa có report hợp lệ.

Chạy P2.2E nếu P2.2D chưa có report hợp lệ.

Chạy P2.2F nếu P2.2A-P2.2E chưa có report/evidence đủ.

Chuyển Phase 3 nếu P2.2F chưa có smoke/evidence/report và Owner/dev lead chưa cho phép.

## 14. FILE INDEX CHI TIẾT

## 14.1. 00-P2-ANALYSIS-ONLY.docx

Hạng mục

Nội dung

Mode

## ANALYSIS ONLY

Mục tiêu

Phân tích hiện trạng Operational Core, không sửa code

Scope chính

Source, Raw Material, Raw Receipt, Raw Lot, QC, Readiness, Production Order, Formula Snapshot, Material Request, Material Issue, Material Receipt, Batch, QC, Release, Warehouse, Inventory, Traceability, QR, Recall, Sale Lock, MISA boundary, Print boundary, Disposal boundary

Done Gate

Có current-state matrix, gap matrix, conflict matrix, điểm chặn register, impacted files, smoke required, owner confirm list

Không được

Không code, không migration, không seed, không tạo production order, không issue material, không release batch, không mở Gateway

## 14.2. 01-P2-1-TECHNICAL-DESIGN-ONLY.docx

Hạng mục

Nội dung

Mode

## TECHNICAL DESIGN ONLY

Mục tiêu

Chuyển analysis thành workstream, task breakdown, dependency, evidence plan, smoke plan và implementation sequence

Workstream

2A Source/Raw/QC/Readiness; 2B Production/Dossier/Snapshot; 2C Material Issue/Receipt/Ledger; 2D Batch/QC/Release/Warehouse/Inventory; 2E Trace/QR/Recall/Sale Lock; 2F Smoke/Evidence

mở rộng từ nguồn tổng hợp

Operational Form Registry, auto-generated forms, inherited fields, click/tự hiện/nhập tay, MISA boundary, print levels, disposal/write-off

Done Gate

Có Workstream Matrix, Form Matrix, Field Behavior Matrix, Dependency Matrix, Smoke/Evidence Plan, Stop Condition Matrix

Không được

Không code, không migration, không seed, không gọi QC_PASS là READY, không gọi Batch QC_PASS là RELEASED

## 14.3. 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS.docx

Hạng mục

Nội dung

Mode

LIMITED IMPLEMENTATION nếu được mở

Mục tiêu

Source / Raw Material / Raw Receipt / Raw Lot / Incoming QC / Readiness

Scope chính

Source origin, raw receipt, raw lot, incoming QC, readiness gate, issue eligibility

mở rộng từ nguồn tổng hợp

Phiếu nhập nguyên liệu đầu vào là nơi được click chọn nguyên liệu từ master; Sâm Savigin là Company Source / strategic reserve; disposal cho nguyên liệu reject/expired/damaged

Done Gate

Raw lot QC_PASS chưa tự READY; raw lot chỉ issue khi READY_FOR_PRODUCTION; Sâm không vào supplier purchase thường; disposal không xóa tay

Không được

Không Production Order đầy đủ, không Material Issue đầy đủ, không Inventory Ledger đầy đủ, không MISA sync thật, không Gateway

## 14.4. 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx

Hạng mục

Nội dung

Mode

LIMITED IMPLEMENTATION nếu được mở

Mục tiêu

Production Order / Production Dossier / Formula Snapshot / Workshop Handoff

Scope chính

Production order, production dossier, formula/recipe/BOM snapshot, workshop handoff

mở rộng từ nguồn tổng hợp

Chọn SKU thì công thức/mã công thức/version tự hiện; không chọn tay nguyên liệu; lệnh sản xuất sinh hồ sơ sản xuất gốc

Done Gate

Production dossier được tạo; snapshot có SKU/formula/version/ingredient/quantity/UOM; snapshot cũ không đổi khi recipe thay đổi

Không được

Không Material Issue, không debit inventory, không Batch/Warehouse, không Sellable, không AI/Gateway tự phát production order

## 14.5. 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx

Hạng mục

Nội dung

Mode

LIMITED IMPLEMENTATION nếu được mở

Mục tiêu

Material Request / Material Approval / Material Issue / Material Receipt / Raw Inventory Ledger

Scope chính

Phiếu đề nghị cấp nguyên liệu, chấp thuận nguyên liệu, issue, receipt, ledger debit

mở rộng từ nguồn tổng hợp

Material Request tự hiện nguyên liệu theo công thức; Material Approval dựa trên request; Material Issue là điểm giảm tồn; Receipt không giảm tồn lần hai; phiếu kế toán xuất nguyên liệu có ngõ MISA

Done Gate

Issue linked production dossier; raw lot READY; ledger debit một lần; receipt không debit; ledger append-only

Không được

Không issue lot chưa READY, không double debit, không ledger update/delete, không batch/warehouse/full MISA

## 14.6. 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx

Hạng mục

Nội dung

Mode

LIMITED IMPLEMENTATION nếu được mở

Mục tiêu

Batch / Process / QC / Release / Warehouse / Finished Goods Inventory / Packaging

Scope chính

Batch, sơ chế, sấy, QC sau sấy, đóng gói cấp 1/2, QC thành phẩm, release, warehouse receipt, FG ledger

mở rộng từ nguồn tổng hợp

Batch/sơ chế/sấy/đóng gói/QC/nhập kho phải dựa trên production dossier; print cấp 1/cấp 2 boundary; warehouse không tự sellable

Done Gate

Batch QC_PASS chưa RELEASED; warehouse chỉ nhận RELEASED; FG ledger credit khi warehouse confirmed; packaging/print boundary rõ

Không được

Không hardcode QC_PASS = RELEASED, không warehouse batch chưa release, không tự set Sellable, không QR/Recall full nếu chưa tới P2.2E

## 14.7. 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx

Hạng mục

Nội dung

Mode

LIMITED IMPLEMENTATION nếu được mở

Mục tiêu

Traceability / QR / Public Trace / Recall / Hold / Sale Lock

Scope chính

Genealogy, trace projection, QR status, public trace whitelist, recall, impact analysis, sale lock, downstream suppression

mở rộng từ nguồn tổng hợp

Ginsengfood sinh print payload; máy in không sinh mã; print cấp 1/cấp 2; reprint reason/permission/log/audit; sale lock thắng mọi kênh

Done Gate

QR VOID/FAILED không public-valid; Public Trace whitelist-only; Recall/Sale Lock chặn downstream; reprint controlled

Không được

Không public internal trace, không lộ private field, không để downstream bypass, không mở Commerce/AI/Gateway

## 14.8. 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx

Hạng mục

Nội dung

Mode

## VALIDATION / SMOKE / EVIDENCE REPORT ONLY

Mục tiêu

Chạy smoke, gom evidence, lập Phase 2 report

Scope chính

Validate P2.2A-P2.2E, build/test/smoke, migration/seed validation nếu có, evidence register, điểm chặn register, risk register

mở rộng từ nguồn tổng hợp

Smoke cho auto-generated forms, inherited data, no manual ingredient, no double debit, print level, MISA boundary, disposal/write-off

Done Gate

Có smoke matrix, evidence register, open điểm chặn register, risk register, git diff/status nếu có, Gateway vẫn bị chặn

Không được

Không sửa feature code, không tạo migration mới, không tự chuyển Phase 3, không gọi Gateway PASS

## 15. REPORT FORMAT BẮT BUỘC - 15 MỤC

Từ V1.1, mọi report trong Phase 2 phải có 15 mục, mở rộng mục “Đề xuất tiếp theo” sau “Cập nhật handoff”.

## STT

Mục report

Nội dung

Tóm tắt

Tóm tắt việc đã làm

File đã sửa

Danh sách file thay đổi hoặc xác nhận không sửa

Nguồn yêu cầu

Tài liệu/handoff/prompt nguồn

Evidence đã dùng

Evidence đầu vào

Lệnh đã chạy

Build/test/smoke/command đã chạy

Kết quả test

Unit/integration test nếu có

Kết quả backend build

Backend build status

Kết quả frontend build

Frontend build status nếu có

Kết quả cleanup process

Cleanup/lint/format nếu có

10

Cập nhật Markdown/tài liệu

Tài liệu nào đã cập nhật hoặc không cập nhật

11

Kết quả database migration/update nếu áp dụng

Migration/update result

12

Kết quả seed validation nếu áp dụng

Seed validation result

13

Rủi ro còn lại

Risk/P0/P1/P2 còn mở

14

Cập nhật handoff

Trạng thái bàn giao

15

Đề xuất tiếp theo

PLAN_ONLY / IMPLEMENT_ONE_PHASE / test evidence / owner decision / stop

Nếu report thiếu 15 mục, không được chuyển prompt tiếp theo.

## 16. DOWNSTREAM IMPACT

## 16.1. Ảnh hưởng sang Phase 3 - Commerce Runtime

Phase 3 cần Phase 2 để biết:

Finished goods có thật không.

Batch đã RELEASED chưa.

Warehouse Receipt đã confirmed chưa.

Finished Goods Ledger đã credit chưa.

SKU/batch có recall không.

SKU/batch có sale lock không.

Public trace/QR có hợp lệ không.

Commerce Sellable Gate không được dùng Product Active thay Sellable.

Commerce Sellable Gate phải dựa vào operational truth từ Phase 2.

## 16.2. Ảnh hưởng sang Phase 4 - AI Advisor Runtime

AI Advisor không được tư vấn/chốt sản phẩm nếu:

Không có finished goods truth.

Batch chưa RELEASED.

Warehouse chưa receipt confirmed.

SKU/batch bị recall.

SKU/batch bị sale lock.

SKU không sellable theo Commerce Runtime.

## 16.3. Ảnh hưởng sang Facebook Gateway / Ads / MC AI Live / CRM / IVR

Các kênh downstream không được:

Chốt đơn SKU bị sale lock.

Đẩy quảng cáo SKU bị recall.

Giới thiệu SKU chưa có sellable truth.

Public trace private field.

Bỏ qua suppression boundary.

Xác nhận đơn chứa sản phẩm stop-sale.

Mở Global Gate khi Operational Core chưa có evidence.

## 17. PHASE 2 SMOKE MATRIX TỔNG HỢP

Smoke ID

Nhóm

Case

Expected

## P2F-SMOKE-A01

Source / Raw

Tạo Raw Material hợp lệ

Raw Material được tạo

## P2F-SMOKE-A02

Raw Receipt

Tạo Raw Material Receipt hợp lệ

Receipt được tạo

## P2F-SMOKE-A03

Raw Receipt

Chọn nguyên liệu từ master

Material canonical tự hiện

## P2F-SMOKE-A04

Raw Lot

Tạo Raw Lot từ receipt hợp lệ

Raw Lot CREATED/WAITING_QC

## P2F-SMOKE-A05

Raw Lot

Raw Lot thiếu UOM/quantity

Bị reject

## P2F-SMOKE-A06

Incoming QC

Incoming QC PASS

QC PASS, raw lot chưa tự READY_FOR_PRODUCTION

## P2F-SMOKE-A07

Incoming QC

Incoming QC FAIL/HOLD

Raw lot không READY_FOR_PRODUCTION

## P2F-SMOKE-A08

Readiness

Raw lot QC_PASS + đủ điều kiện

Mark READY_FOR_PRODUCTION

## P2F-SMOKE-A09

Readiness

Raw lot HOLD/REJECTED/QUARANTINE

Không READY_FOR_PRODUCTION

## P2F-SMOKE-A10

Sâm Savigin

Sâm là Company Source

Không vào supplier purchase thường

## P2F-SMOKE-A11

Disposal

Raw material reject/expired/damaged

Đi qua disposal/write-off flow

## P2F-SMOKE-B01

Production

Tạo Production Order từ SKU hợp lệ

Production Order được tạo

## P2F-SMOKE-B02

Production

Chọn SKU

Formula code/version tự hiện

## P2F-SMOKE-B03

Production

Production Order thiếu SKU

Bị reject

## P2F-SMOKE-B04

Production

Production Order thiếu Recipe/Formula

Bị reject

## P2F-SMOKE-B05

Snapshot

Production Order snapshot

Có formula kind/version/ingredient/quantity/UOM

## P2F-SMOKE-B06

Snapshot

Recipe/BOM sửa sau khi tạo order

Snapshot cũ không đổi

## P2F-SMOKE-B07

Production Dossier

Production Order created

Production Dossier tự sinh

## P2F-SMOKE-B08

No Manual Ingredient

Production Order

Không cho chọn tay nguyên liệu

## P2F-SMOKE-C01

Material Request

Request linked Production Dossier

Request tự sinh/tự hiện nguyên liệu

## P2F-SMOKE-C02

Material Request

User chọn tay nguyên liệu

Bị chặn

## P2F-SMOKE-C03

Material Approval

Approval linked request

dựa trên request lines

## P2F-SMOKE-C04

Material Issue

Issue raw lot READY_FOR_PRODUCTION

Issue được tạo

## P2F-SMOKE-C05

Material Issue

Issue raw lot QC_PASS chưa READY

Bị reject

## P2F-SMOKE-C06

Material Issue

Issue raw lot HOLD/REJECTED/QUARANTINE

Bị reject

## P2F-SMOKE-C07

Ledger

Material Issue thành công

Tạo raw ledger debit

## P2F-SMOKE-C08

Idempotency

Retry Material Issue same key/payload

Không double debit

## P2F-SMOKE-C09

Material Receipt

Receipt linked Issue

Receipt được tạo

## P2F-SMOKE-C10

Material Receipt

Receipt thành công

Không tạo raw ledger debit lần hai

## P2F-SMOKE-C11

Accounting Handoff

Phiếu kế toán xuất nguyên liệu

Có handoff/sync boundary, MISA không điều khiển

## P2F-SMOKE-C12

Ledger

Ledger update/delete attempt

Không được phép

## P2F-SMOKE-D01

Batch

Tạo Batch linked Production Dossier

Batch được tạo

## P2F-SMOKE-D02

Batch Process

Sơ chế/sấy/đóng gói

dựa trên SKU/batch/formula

## P2F-SMOKE-D03

Batch QC

Batch QC PASS

QC PASS, batch chưa tự RELEASED

## P2F-SMOKE-D04

Batch QC

Batch QC FAIL/HOLD/REJECTED

Không release được

## P2F-SMOKE-D05

Batch Release

Release khi QC_PASS + guard pass

Batch RELEASED

## P2F-SMOKE-D06

Warehouse

Warehouse Receipt Batch QC_PASS chưa RELEASED

Bị reject

## P2F-SMOKE-D07

Warehouse

Warehouse Receipt Batch RELEASED

Receipt được confirm

## P2F-SMOKE-D08

FG Ledger

Warehouse Receipt confirmed

Tạo Finished Goods Ledger credit

## P2F-SMOKE-D09

Idempotency

Retry Warehouse Receipt same key/payload

Không double credit

## P2F-SMOKE-D10

Commerce Boundary

Warehouse Receipt confirmed

Không tự set Sellable

## P2F-SMOKE-P01

Print Level 1

Print cấp 1

Chỉ MFG/HSD

## P2F-SMOKE-P02

Print Level 2

Print cấp 2

Batch/MFG/HSD/barcode/QR

## P2F-SMOKE-P03

Printer Boundary

Máy in nhận payload

Máy in không tự sinh mã

## P2F-SMOKE-P04

Reprint

Reprint request

Có reason/permission/log/audit

## P2F-SMOKE-E01

Traceability

Build internal genealogy

Raw lot -> production -> batch -> warehouse

## P2F-SMOKE-E02

Traceability

Missing genealogy link

Trace gap flagged

## P2F-SMOKE-E03

## QR

## QR GENERATED/PRINTED

QR có status hợp lệ

## P2F-SMOKE-E04

## QR

## QR VOID

Public Trace không valid

## P2F-SMOKE-E05

## QR

## QR FAILED

Public Trace không valid

## P2F-SMOKE-E06

Public Trace

Public Trace response

Whitelist-only

## P2F-SMOKE-E07

Public Trace

Private field attempt

Không lộ private/internal field

## P2F-SMOKE-E08

Recall

Recall Case OPEN

Recall case được tạo

## P2F-SMOKE-E09

Recall

Recall impact analysis

Xác định impacted batch/QR/SKU

## P2F-SMOKE-E10

Sale Lock

Sale Lock active

Target bị suppression

## P2F-SMOKE-E11

Downstream

Sale Lock active downstream hook

Commerce/AI/Gateway/Ads/Live/CRM/IVR bị chặn ở boundary

## P2F-SMOKE-X01

Cross-cutting

High-risk command thiếu actor

## DENY / BLOCK

## P2F-SMOKE-X02

Cross-cutting

High-risk command thiếu permission

## DENY

## P2F-SMOKE-X03

Evidence

Command require evidence nhưng SUBMITTED

Không pass

## P2F-SMOKE-X04

Idempotency

Same key same payload

Không tạo side effect lần hai

## P2F-SMOKE-X05

Idempotency

Same key different payload

Conflict

## P2F-SMOKE-X06

Runtime

Runtime dependency unavailable

Fail-safe, không suy đoán

## P2F-SMOKE-X07

Audit

Audit event for success/deny

Có actor/correlation/action/result

Không gọi bất kỳ smoke nào ở đây là Global Smoke Pass.

## 18. EVIDENCE PACKAGE TỐI THIỂU

Phase 2 cần gom evidence package gồm:

P2 Analysis Report.

P2.1 Technical Design Report.

P2.2A Source / Raw / QC / Readiness evidence.

P2.2B Production Order / Production Dossier / Snapshot evidence.

P2.2C Material Request / Issue / Receipt / Ledger evidence.

P2.2D Batch / Release / Warehouse / Inventory / Packaging evidence.

P2.2E Traceability / QR / Recall / Sale Lock evidence.

P2.2F Smoke Result Matrix.

File changes summary.

Git diff summary nếu có.

Build result.

Test result.

Smoke result.

Migration validation nếu có.

Seed validation nếu có.

Raw Lot QC_PASS != READY_FOR_PRODUCTION evidence.

Production Order snapshot evidence.

Production Dossier root evidence.

No manual ingredient selection evidence.

Material Issue no double debit evidence.

Material Receipt no second debit evidence.

Batch QC_PASS != RELEASED evidence.

Warehouse only accepts RELEASED batch evidence.

No double credit evidence.

Inventory Ledger append-only evidence.

Print level 1 payload evidence.

Print level 2 payload evidence.

Printer no-code-generation evidence.

Reprint control evidence.

Public Trace whitelist-only evidence.

QR VOID/FAILED not public-valid evidence.

Recall / Sale Lock downstream suppression evidence.

MISA boundary evidence.

Disposal/write-off evidence.

Open điểm chặn register.

Risk register.

Gateway bị chặn evidence.

Owner review checklist.

Evidence status được phép ghi:

## FOUND.

## SUBMITTED.

## MISSING.

## NEEDS_REVIEW.

## REJECTED_BY_SMOKE.

bị chặn.

## SUPERSEDED.

Không tự ghi:

## ACCEPTED.

Completion Decision.

Release Readiness.

Production Readiness.

Go-live Decision.

GATEWAY PASS.

## 19. điểm chặn P0 TOÀN PHASE 2

Các lỗi sau là P0 điểm chặn:

Raw Lot QC_PASS bị dùng làm READY_FOR_PRODUCTION.

Raw lot chưa READY_FOR_PRODUCTION vẫn issue được.

Phiếu nhập nguyên liệu cho nhập material tự do không qua master.

Sâm Savigin bị xử lý như supplier material thường.

Production Order không snapshot formula/BOM.

Production Order cho chọn tay nguyên liệu.

Production Order không sinh Production Dossier.

Production Dossier không là root data cho phiếu sau.

Material Request cho chọn tay nguyên liệu.

Material Request không dựa trên formula snapshot.

Material Approval thêm nguyên liệu ngoài request.

Material Issue không tạo raw ledger debit.

Material Issue retry tạo double debit.

Material Receipt tạo debit lần hai.

Inventory Ledger update/delete được.

Batch QC_PASS bị dùng làm RELEASED.

Warehouse nhận Batch chưa RELEASED.

Finished Goods Ledger credit tạo trước warehouse receipt confirmed.

Warehouse Receipt retry tạo double credit.

Warehouse Receipt tự set Sellable.

Print cấp 1 in sai rule MFG/HSD.

Print cấp 2 thiếu batch/MFG/HSD/barcode/QR.

Máy in tự sinh batch/MFG/HSD/barcode/QR.

Reprint không có reason/permission/log/audit.

Public Trace không whitelist-only.

QR VOID/FAILED public-valid.

Recall/Sale Lock không chặn Commerce.

Recall/Sale Lock không chặn AI/Gateway/Ads/Live/CRM/IVR.

MISA điều khiển production/formula/batch/trace/recall/print.

Disposal bằng cách xóa tay tồn kho.

SUBMITTED evidence được xem là ACCEPTED.

Runtime unavailable nhưng hệ thống vẫn suy đoán.

Global Gate không còn bị chặn.

Smoke fail nhưng vẫn gọi done.

Evidence Submitted bị gọi Evidence Accepted.

Test pass bị gọi Global Smoke Pass.

Phase 2 Done bị gọi Release Readiness.

Phase 2 Done bị gọi Production Readiness.

Phase 2 Done bị gọi Go-live Decision.

## 20. DEV EXECUTION PROTOCOL

Khi giao dev/Codex/Copilot, bắt buộc chạy theo protocol sau:

Step 1 - Đọc README này

Dev phải đọc README này để hiểu:

Scope Phase 2.

File order.

Gate.

Boundary.

P0 điểm chặn.

MISA/print/disposal/sale lock boundary.

Global Gate vẫn bị chặn.

Step 2 - Chạy 00-P2-ANALYSIS-ONLY.docx

Chỉ phân tích.

Không sửa file.

Không code.

Không migration.

Không seed.

Step 3 - Chạy 01-P2-1-TECHNICAL-DESIGN-ONLY.docx

Chỉ thiết kế.

Không sửa file.

Không code.

Không migration.

Không seed.

Step 4 - Owner/dev lead duyệt boundary

Không có approval thì không được implementation.

Các boundary phải được duyệt:

Operational form registry.

Field behavior click/tự hiện/nhập tay.

Production dossier.

Formula snapshot.

Material issue/receipt/ledger.

Batch/release/warehouse.

Print level 1/2.

Trace/QR/public trace.

Recall/sale lock.

MISA boundary.

Disposal/write-off.

Actor/permission/audit/idempotency/evidence.

Step 5 - Chạy từng prompt implementation có giới hạn

Chạy tuần tự:

P2.2A - Source / Raw / Lot / QC / Readiness.

P2.2B - Production Order / Production Dossier / Snapshot / Workshop.

P2.2C - Material Request / Approval / Issue / Receipt / Ledger.

P2.2D - Batch / QC / Release / Warehouse / Inventory / Packaging.

P2.2E - Traceability / QR / Recall / Sale Lock.

Mỗi prompt phải có report 15 mục.

Step 6 - Chạy P2.2F

Chỉ smoke/evidence/report.

Không sửa feature code.

Không tạo feature mới.

Không tạo migration mới.

Không tạo seed mới.

Step 7 - Owner Review

Sau P2.2F mới sang Owner Review.

Evidence Submitted chưa phải Evidence Accepted.

Step 8 - Cho phép Phase 3 nếu đủ điều kiện

Chỉ sau Owner Review, mới được bắt đầu:

## PROMPT-P3 - COMMERCE RUNTIME ANALYSIS HANDOFF

Phase 3 vẫn phải bắt đầu bằng:

## ANALYSIS ONLY

## 21. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối dưới đây để giao dev/Codex/Copilot khi bắt đầu Phase 2.

## PHASE-02 OPERATIONAL CORE - DEV EXECUTION HANDOFF

Bạn đang thực hiện PHASE-02 - Operational Core cho dự án Ginsengfood.

Không được hiểu nhiệm vụ này là copy-paste code từ AI.

Bạn phải inspect codebase thật, database thật, API thật, migration thật, seed thật, test thật và triển khai theo architecture thật.

## GLOBAL LOCK

Global Gate: bị chặn.

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Documentation Complete chưa phải Production Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision nếu chưa có accepted evidence, smoke, owner review và release decision.

## EXECUTION ORDER

Chạy đúng thứ tự:

README-PHASE-02-HANDOFF-INDEX.docx.

## 00-P2-ANALYSIS-ONLY.docx.

## 01-P2-1-TECHNICAL-DESIGN-ONLY.docx.

## 02-P2-2A-SOURCE-RAW-LOT-QC-READINESS.docx.

## 03-P2-2B-PRODUCTION-ORDER-SNAPSHOT-WORKSHOP.docx.

## 04-P2-2C-MATERIAL-ISSUE-RECEIPT-LEDGER.docx.

## 05-P2-2D-BATCH-QC-RELEASE-WAREHOUSE-INVENTORY.docx.

## 06-P2-2E-TRACEABILITY-QR-RECALL-SALE-LOCK.docx.

## 07-P2-2F-SMOKE-EVIDENCE-REPORT.docx.

Không được nhảy phase.

Không được bỏ qua Analysis/Design.

Không được implementation nếu chưa có approved boundary.

## CORE RULES

Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Raw lot chỉ được issue khi READY_FOR_PRODUCTION.

Phiếu nhập nguyên liệu đầu vào là nơi được click chọn nguyên liệu từ master.

Production Order phải snapshot formula kind / version / ingredient / quantity / UOM.

Production Order phải sinh Production Dossier.

Production Dossier là nguồn dữ liệu gốc cho chuỗi phiếu sau.

Không được chọn tay nguyên liệu ở Production Order.

Không được chọn tay nguyên liệu ở Material Request.

Material Request phải tự hiện nguyên liệu theo formula snapshot.

Material Approval phải dựa trên Material Request.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt không được giảm tồn lần hai.

Inventory Ledger append-only.

Batch QC_PASS không đồng nghĩa Batch RELEASED.

Warehouse chỉ nhận Batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Warehouse Receipt không tự set Sellable.

Finished Goods Inventory không tự Sellable.

Print cấp 1 chỉ in MFG/HSD.

Print cấp 2 in batch/MFG/HSD/barcode/QR.

Máy in chỉ nhận payload, không tự sinh mã.

Reprint phải có lý do, quyền, log, audit.

QR VOID/FAILED không public-valid.

Public Trace whitelist-only.

Recall / Sale Lock thắng Commerce / AI / Ads / Live / CRM / Gateway / IVR.

MISA không điều khiển production/formula/batch/trace/recall/print.

Disposal/write-off không được xóa tay tồn kho.

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Global Gate vẫn bị chặn.

## REPORT FORMAT

Mọi report phải có 15 mục:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown/tài liệu.

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

Đề xuất tiếp theo.

## FINAL RULE

Nếu phát hiện điểm chặn, không chỉnh sửa tạm lén, không bỏ qua, không sửa seed/test để xanh giả.

Phải báo điểm chặn và quay lại đúng prompt liên quan:

Lỗi Source / Raw / QC / Readiness -> quay lại P2.2A.

Lỗi Production Order / Snapshot / Production Dossier -> quay lại P2.2B.

Lỗi Material Issue / Receipt / Ledger -> quay lại P2.2C.

Lỗi Batch / Warehouse / Inventory / Packaging -> quay lại P2.2D.

Lỗi Traceability / QR / Recall / Sale Lock / Print -> quay lại P2.2E.

Lỗi Smoke/Evidence -> xử lý theo P2.2F và Owner Review.

Lỗi Actor/RBAC/Audit/Evidence/Idempotency -> quay lại Phase 0/Foundation liên quan.

Lỗi Product/SKU/Recipe dependency -> quay lại Phase 1 liên quan.

## 22. RELEASE / GATEWAY LOCK

Phase 2 không được gọi các trạng thái sau:

Phase 2 Complete.

Completion Decision.

Global Smoke Pass.

Release Readiness.

Production Readiness.

Go-live Decision.

Gateway PASS.

Phase 2 chỉ có thể kết luận tối đa:

PHASE 2 Analysis Report Completed.

PHASE 2 Technical Design Handoff Completed.

P2.2A Limited Implementation Report Only.

P2.2B Limited Implementation Report Only.

P2.2C Limited Implementation Report Only.

P2.2D Limited Implementation Report Only.

P2.2E Limited Implementation Report Only.

PHASE 2 Validation Report Only.

Evidence Submitted for Review.

Ready for Owner Review.

P3 Analysis Prompt Draft Allowed nếu Owner/dev lead cho phép.

## 23. DONE GATE CỦA README PHASE 2

README Phase 2 được xem là đạt khi:

Xác định rõ Phase 2 là Operational Core, không phải Commerce Runtime.

Xác định rõ Phase 2 tạo operational truth, không tự quyết sellable.

Xác định rõ nguồn tổng hợp là auxiliary source.

Xác định rõ source-of-truth hierarchy.

Xác định rõ scope in.

Xác định rõ scope out.

Xác định rõ danh sách file Phase 2.

Xác định rõ file nào phải viết lại toàn bộ.

Khóa Operational Form Registry.

Khóa tự sinh phiếu.

Khóa dựa trên dữ liệu giữa các phiếu.

Khóa click chọn / tự hiện / nhập tay.

Khóa Production Dossier là nguồn dữ liệu gốc.

Khóa Formula Snapshot.

Khóa No Manual Ingredient Selection.

Khóa Raw Lot QC_PASS != READY_FOR_PRODUCTION.

Khóa Material Issue là điểm giảm tồn.

Khóa Material Receipt không giảm tồn lần hai.

Khóa Inventory Ledger append-only.

Khóa Batch QC_PASS != RELEASED.

Khóa Warehouse chỉ nhận Batch RELEASED.

Khóa Warehouse Receipt không tự set Sellable.

Khóa Print cấp 1/cấp 2.

Khóa Printer không tự sinh mã.

Khóa Reprint có reason/permission/log/audit.

Khóa Public Trace whitelist-only.

Khóa QR VOID/FAILED không public-valid.

Khóa Recall/Sale Lock thắng downstream.

Khóa MISA boundary.

Khóa Sâm Savigin Company Source / strategic reserve.

Khóa Disposal/write-off không xóa tay.

Khóa report format 15 mục.

Khóa smoke/evidence requirements.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 24. FAIL GATE CỦA README PHASE 2

README Phase 2 fail nếu xảy ra một trong các trường hợp:

Dùng nguồn tổng hợp để code trực tiếp.

Dùng nguồn tổng hợp để seed trực tiếp.

chỉnh sửa tạm lẻ file Phase 2 bị ảnh hưởng thay vì viết lại toàn bộ.

Nhảy từ README sang implementation.

Bỏ qua Analysis Only.

Bỏ qua Technical Design Only.

Cho Agent tự suy luận nghiệp vụ còn thiếu.

Raw Lot QC_PASS bị hiểu là READY_FOR_PRODUCTION.

Raw lot chưa READY vẫn issue được.

Production Order không snapshot formula/BOM.

Production Order cho chọn tay nguyên liệu.

Material Request cho chọn tay nguyên liệu.

Material Issue không phải điểm debit chính.

Material Receipt debit lần hai.

Inventory Ledger update/delete được.

Batch QC_PASS bị hiểu là RELEASED.

Warehouse nhận batch chưa RELEASED.

Warehouse Receipt tự set Sellable.

Print cấp 1/cấp 2 sai rule.

Máy in tự sinh mã.

Reprint không audit.

Public Trace lộ field nội bộ.

QR VOID/FAILED public-valid.

Recall/Sale Lock bị downstream bypass.

MISA điều khiển vận hành.

Disposal/write-off bằng cách xóa tay tồn kho.

Evidence Submitted bị gọi Evidence Accepted.

Smoke local bị gọi Global Smoke Pass.

Phase 2 Done bị gọi Release Readiness.

Phase 2 Done bị gọi Production Readiness.

Phase 2 Done bị gọi Go-live Decision.

Global Gate không còn bị chặn.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 2 - OPERATIONAL CORE

File type

README / Handoff Index / Execution Order / Gateway Lock

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed authorization

KHONG

Limited implementation authorization

KHONG

Evidence status

NOT ACCEPTED mặc định

Gateway status

bị chặn

Global Smoke Pass

KHONG

Completion Decision

KHONG

Release Readiness

KHONG

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

## 26. KẾT LUẬN ĐIỀU PHỐI PHASE 2

Từ thời điểm dùng README Phase 2 V1.1 này, cách làm đúng cho Phase 2 là:

Không code trực tiếp từ nguồn tổng hợp.

Không sửa chỉnh sửa tạm file cũ.

File nào bị ảnh hưởng mạnh thì viết lại toàn bộ.

Không nhảy từ README sang implementation.

Không bỏ qua Analysis Only.

Không bỏ qua Technical Design Only.

Chuỗi phiếu/lệnh phải tự sinh và dựa trên dữ liệu đúng.

Production Dossier là nguồn dữ liệu gốc.

Công thức phải snapshot theo SKU + formula code + version.

Không chọn tay nguyên liệu ở Production Order và Material Request.

Material Issue là điểm giảm tồn nguyên liệu.

Material Receipt không giảm tồn lần hai.

Batch QC_PASS không đồng nghĩa RELEASED.

Warehouse chỉ nhận batch RELEASED.

Print payload do Ginsengfood sinh.

Máy in không tự sinh mã.

MISA không điều khiển vận hành.

Disposal/write-off không xóa tay tồn kho.

Recall/Sale Lock thắng downstream.

Evidence Submitted chưa phải Evidence Accepted.

Global Gate vẫn bị chặn.

Tài liệu kế tiếp cần viết lại toàn bộ là:
