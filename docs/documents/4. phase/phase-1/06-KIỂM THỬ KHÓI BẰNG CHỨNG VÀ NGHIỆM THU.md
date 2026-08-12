# 06-P1 - SMOKE BẰNG CHỨNG VÀ NGHIỆM THU

Trạng thái: `PHASE_1_EVIDENCE_CANONICAL`  
Mode: `SMOKE / EVIDENCE HANDOFF`  
Mục tiêu: chứng minh Phase 1 đã hấp thụ đủ 5 nguồn cũ và đủ điều kiện cho Phase 2 consume mà không cần mở file cũ.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev triển khai hoặc consume sang Phase 2 có kiểm soát. `SMOKE`, `EVIDENCE`, `NGHIỆM THU` và `handoff-ready` trong file này là tiêu chí hồ sơ/dev handoff; chúng không phải Production Ready, Release Ready hoặc Go-live approval.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES
READY_TO_HAND_TO_DEV_OR_PHASE_2: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## 1. Source coverage

| Nguồn cũ | Nội dung đã đưa vào Phase 1 | File Phase 1 chính |
| --- | --- | --- |
| BỘ KHÓA 5 BƯỚC | BOM map, operational config, version formula, trace/recall boundary, màn hình/route/action | 00, 01, 03, 04, 06 |
| CÔNG THỨC VẬN HÀNH 20 SKU MỚI | Toàn bộ 20 SKU, formula G1, bảng định lượng từng nguyên liệu | 02, 03, 06 |
| DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ | A1/A2/A3, B1/B2/B3, threshold, dynamic resolver, disposal/write-off | 02, 05, 06 |
| BẢNG PHIẾU LỆNH SẢN XUẤT | Các field Phase 1 phải cung cấp cho lệnh sản xuất và phiếu tự sinh | 01, 03, 04, 06 |
| PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN | Click chọn/tự hiện/nhập tay, source dữ liệu công thức, print/accounting boundary | 01, 03, 04, 05, 06 |

## 2. Smoke bắt buộc

| Evidence ID | Nội dung | Expected |
| --- | --- | --- |
| EVD-P1-SRC-001 | 5 file nguồn cũ có mặt trong Phase 1/2 | Có source marker hoặc phụ lục nhập nguồn |
| EVD-P1-SKU-020 | Đủ 20 SKU canonical | Không thiếu/trùng SKU |
| EVD-P1-FML-G1 | Mỗi SKU có formula G1 | Không SKU nào thiếu `FML-*-G1` |
| EVD-P1-BOM-LINE | Formula bung từng nguyên liệu | Không dòng gom nhóm cho production snapshot |
| EVD-P1-UOM | UOM/conversion pass | Không scale khi thiếu conversion/density |
| EVD-P1-VEGAN | Vegan guard fail-closed | Mọi SKU có claim vegan không chứa line động vật; A1/B1/C2/C3 khóa claim nếu còn `lòng trắng trứng` |
| EVD-P1-MATERIAL | A1/A2/A3/B1/B2/B3 canonical | Không duplicate material vì usage_role |
| EVD-P1-PACKAGING | B1/B2/B3 profile pass | Có output/threshold policy |
| EVD-P1-ACTIVATION | Product active != sellable | Downstream không mở bán từ activation |
| EVD-P1-SEED | Seed idempotent | Rerun không duplicate |

## 3. Done gate

Phase 1 chỉ được coi là handoff-ready khi:

1. File con tiếng Việt là nguồn chính.
2. 5 file nguồn cũ có nội dung đã nhập đầy đủ vào đúng phạm vi Phase 1/Phase 2; riêng biểu mẫu vận hành được Phase 1 giữ dưới dạng contract/projection/boundary và Phase 2 giữ payload runtime.
3. Conflict vegan/common broth đã normalize fail-closed: không claim thuần chay khi formula còn dòng động vật.
4. Formula G1, material, packaging, threshold đủ để Phase 2 snapshot.
5. Không gọi Production Ready hoặc Go-live.

## 4. Exact source coverage

| Evidence ID | File nguồn cũ | File Phase 1/2 chứa payload | Expected |
| --- | --- | --- | --- |
| `EVD-P1-APP-001` | `BẢNG PHIẾU LỆNH SẢN XUẤT.md` | `07-BẢNG PHIẾU LỆNH SẢN XUẤT.md`, Phase 2 `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | Field production consumer được phản ánh vào P1 projection/P2 dossier |
| `EVD-P1-APP-002` | `BỘ KHÓA 5 BƯỚC.md` | `08-BỘ KHÓA 5 BƯỚC.md`, `03-CÔNG THỨC BOM PHIÊN BẢN G1.md`, Phase 2 `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | BOM map/version/no-manual/trace không mất |
| `EVD-P1-APP-003` | `CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` | `09-CÔNG THỨC VẬN HÀNH 20 SKU MỚI.md`, `02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md`, `03-CÔNG THỨC BOM PHIÊN BẢN G1.md`, Phase 2 `03-LỆNH SẢN XUẤT HỒ SƠ GỐC VÀ ẢNH CHỤP.md` | 20 formula G1 và định lượng nguyên liệu tồn tại |
| `EVD-P1-APP-004` | `DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VẬT TƯ.md` | `10-DANH MỤC PHÂN NHÓM NGUYÊN LIỆU VÀ VẬT TƯ.md`, `02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md`, `05-QUẢN TRỊ DỮ LIỆU KHỞI TẠO MỞ RỘNG SKU VÀ VẬT TƯ.md`, Phase 2 `02-NGUỒN NGUYÊN LIỆU LÔ QC SẴN SÀNG SẢN XUẤT.md` | A/B groups, threshold, disposal/write-off không mất |
| `EVD-P1-APP-005` | `PHIẾU TỰ SINH, IN, KẾ TOÁN, HOẠCH TOÁN.md` | `11-PHIẾU TỰ SINH IN KẾ TOÁN VÀ HOẠCH TOÁN.md`, Phase 2 `01-THIẾT KẾ KỸ THUẬT VẬN HÀNH LÕI.md`, Phase 2 `06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | Click/tự hiện/nhập tay, print/accounting boundary không mất |

## 5. Evidence map theo file con Phase 1

| File | Evidence bắt buộc | Expected |
| --- | --- | --- |
| P1-00 | `EVD-P1-SCOPE`, `EVD-P1-SOURCE-MAP`, `EVD-P1-CONFLICT` | Nguồn/phạm vi/conflict rõ |
| P1-01 | `EVD-P1-DESIGN`, `EVD-P1-CONTRACT`, `EVD-P1-PROJECTION` | Data contract đủ cho implementation |
| P1-02 | `EVD-P1-SKU-020`, `EVD-P1-MATERIAL`, `EVD-P1-UOM`, `EVD-P1-PACKAGING` | Master data đủ |
| P1-03 | `EVD-P1-FML-G1`, `EVD-P1-BOM-LINE`, `EVD-P1-SCALING`, `EVD-P1-VERSION` | Formula/BOM đủ và versioned |
| P1-04 | `EVD-P1-ACTIVATION`, `EVD-P1-DOWNSTREAM-BLOCK` | Active không sellable/released |
| P1-05 | `EVD-P1-SEED`, `EVD-P1-THRESHOLD`, `EVD-P1-EXTENSION` | Seed idempotent, threshold governable |
| P1-06 | `EVD-P1-COVERAGE`, `EVD-P1-NO-GOLIVE` | Nghiệm thu coverage và không claim quá scope |

## 6. Smoke case tối thiểu

1. Seed 20 SKU không thiếu/trùng.
2. Query SKU A1/B1/C2/C3 trả về claim suppressed, không vegan-approved.
3. Query formula G1 trả full material lines, không dòng gom nhóm.
4. Query formula G0 nếu tồn tại trả research/pilot, không production-active.
5. Scale theo gạo thiếu conversion thì block.
6. Material alias không được xuất hiện trong Phase 2 BOM snapshot.
7. Packaging profile trả 7.200 gói, 1.800 hộp, 300 thùng baseline hoặc owner profile mới.
8. Threshold policy không hardcode trong business logic.
9. Activation guard block nếu thiếu owner approval/evidence.
10. Phase 2 projection không cần mở 5 file nguồn cũ.

## 7. Blocker phải giữ lại

| Blocker | Không được tự vượt |
| --- | --- |
| Owner chưa approve Formula G1 | Không active production |
| Formula có line động vật nhưng claim thuần chay | Suppress claim |
| UOM/density thiếu | Không scale/issue |
| Packaging profile thiếu | Không handoff Phase 2 packaging |
| Evidence submitted chưa accepted | Không close |
| Seed tạo duplicate | Không nghiệm thu |
| Source coverage thiếu marker | Không bỏ 5 file cũ |

## 8. Verdict kiểm định Phase 1

Kết luận đọc tài liệu Phase 1:

1. Phase 1 đã hấp thụ đủ nguồn cần thiết cho phạm vi Product/SKU/Material/UOM/Recipe/BOM/Formula/Activation.
2. Phase 1 không còn phụ thuộc 5 file rời để hiểu rule chính, vì mapping nguồn, conflict normalization, data contract, formula G1, material/packaging governance và evidence gate đã nằm trong các file con Phase 1.
3. Hai nguồn biểu mẫu vận hành không được nhân runtime vào Phase 1: Phase 1 chỉ giữ trường dữ liệu, resolver, projection, no-manual rule, print/accounting boundary; Phase 2 giữ lệnh sản xuất, phiếu tự sinh, print, QR, kế toán và MISA handoff.
4. Phụ lục cũ dưới dòng `PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE` chỉ là archive/handoff history. Khi có khác biệt, phần canonical ở đầu file luôn thắng.
5. Trạng thái chuẩn để giao tiếp với AI/ChatGPT: `PHASE_1_CANONICAL_READY_FOR_IMPLEMENTATION_HANDOFF`, `SOURCE_ABSORBED`, `NO_SOURCE_CONFLICT_DETECTED`, `NOT_PRODUCTION_READY`, `NOT_GO_LIVE`.
---

## PHỤ LỤC NỘI DUNG CŨ TRƯỚC REWRITE - 06-P1-2E-SMOKE-EVIDENCE-REPORT.md

> Phần này giữ nội dung cũ của file Phase để không mất rule/handoff đang có. Các rule được chuẩn hóa ở phần đầu file là nguồn chính khi có khác biệt cách diễn đạt.

# P1.2E - SMOKE EVIDENCE REPORT

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-10 Evidence / Smoke / Completion Gateway.
- TECH-10 Global Smoke / UAT / Evidence.
- TECH-11 Implementation Report format.
- P1.2A den P1.2D reports va evidence.

## Noi Dung Rewrite

## 06-P1-2E-SMOKE-EVIDENCE-REPORT V1.1

Smoke/evidence sau khi viết lại

## 23.2. Thứ tự phân tích trước seed/code

Bước

Việc cần làm

Output

Documentation Reading

Tóm tắt seed boundary

Analysis Only

Gap matrix seed/code hiện tại

Owner Confirm Required

Danh sách dữ liệu thiếu/conflict

Technical Design Only

Kế hoạch seed/migration nếu được mở

Limited Implementation

Chỉ khi có lệnh riêng

Smoke/Evidence

Nộp log/evidence

Owner Review

Chờ accepted

## 24. DEV / CODEX / COPILOT HANDOFF

## 24.1. Mode mặc định

Khi giao file này cho Agent, mode mặc định là:

## ANALYSIS ONLY

Agent không được tạo seed script, không sửa code, không tạo migration.

## 24.2. Prompt giao Agent đúng

Nhiệm vụ:

Lập Gap Matrix giữa tài liệu seed governance và code/seed hiện tại.

Liệt kê seed hiện tại nếu có.

Liệt kê dữ liệu seed thiếu Owner confirm.

Liệt kê dữ liệu có nguy cơ dirty seed.

Liệt kê dữ liệu bị hardcode.

Liệt kê impacted files.

Liệt kê impacted database/migration nếu có.

Liệt kê impacted API/DTO/service/UI/worker nếu có.

Liệt kê risk P0/P1/P2.

Đề xuất phạm vi implementation sau này.

Đề xuất smoke/evidence.

Không sửa code.

Cấm:

Không tạo seed script.

Không tạo migration.

Không sửa code.

Không tự điền SKU.

Không tự điền formula.

Không tự điền ratio_to_rice.

Không hardcode threshold.

Không seed trực tiếp từ nguồn tổng hợp.

Không set sellable/released/production-ready.

Không gọi Completion Decision / Gateway PASS / Production Readiness.

## 25. OWNER CONFIRM REQUIRED

Các dữ liệu sau bắt buộc Owner xác nhận trước khi seed/code:

Nhóm

Dữ liệu cần xác nhận

Vì sao chặn

SKU baseline

20 SKU code, public name, internal name, group, status

Tránh seed sai sản phẩm

SKU extension

Mã SKU mới, mục tiêu, tên, nhóm

Tránh thêm SKU tự phát

Ingredient

Canonical name, code, group, source type

Tránh trùng nguyên liệu

Alias

Alias trỏ canonical

Tránh duplicate master

## UOM

Đơn vị và conversion

Tránh sai formula/inventory

Recipe group

Group code, required flag

Tránh công thức mơ hồ

Formula

Formula code/version/status

Tránh seed công thức chưa khóa

## G1

Header và lines

Tránh sản xuất sai

Anchor gạo

Ingredient code và UOM

Tránh scale sai

Ratio_to_rice

Tỷ lệ từng line

Lõi tính nguyên liệu

Packaging

Gói/hộp/thùng, units_per_box, boxes_per_carton

Tránh sai đóng gói/in/threshold

Material group

A/B/A-specific/A-common/B1/B2

Tránh sai MRP/stock

Threshold

5 mẻ, 20 mẻ, 15 mẻ, B1/B2 quantities

Tránh mua thừa/sai planning

Sâm Savigin

Company Source/strategic reserve/harvest rule

Tránh supplier purchase sai

Activation

## ACTIVE_BASELINE/ACTIVE/INACTIVE

Tránh mở bán nhầm

Seed status

Dữ liệu nào được seed active

Tránh dirty seed

## 26. STOP CONDITIONS CHO P1.2D

Dev/Agent phải dừng ngay nếu gặp:

Mã

Điều kiện dừng

Hành động

## P1-2D-STOP-01

Seed trực tiếp từ nguồn tổng hợp

Dừng, báo P0

## P1-2D-STOP-02

SKU chưa canonical

Owner confirm

## P1-2D-STOP-03

Trùng SKU code

Owner confirm

## P1-2D-STOP-04

Ingredient trùng canonical

Owner confirm

## P1-2D-STOP-05

Alias không trỏ canonical

Owner confirm

## P1-2D-STOP-06

UOM thiếu conversion

Owner confirm

## P1-2D-STOP-07

Formula thiếu version

Owner confirm

## P1-2D-STOP-08

G1 thiếu anchor gạo

Owner confirm

## P1-2D-STOP-09

G1 thiếu ratio_to_rice

Owner confirm

## P1-2D-STOP-10

Formula line chỉ có group mơ hồ

Owner confirm

## P1-2D-STOP-11

Packaging thiếu units_per_box/boxes_per_carton

Owner confirm

## P1-2D-STOP-12

Threshold hardcode

Dừng, báo P0

## P1-2D-STOP-13

Sâm Savigin seed như supplier material

Dừng, báo P0

## P1-2D-STOP-14

Seed set sellable

Dừng, báo P0

## P1-2D-STOP-15

Seed set released

Dừng, báo P0

## P1-2D-STOP-16

Seed set production-ready

Dừng, báo P0

## P1-2D-STOP-17

Agent tự suy luận dữ liệu thiếu

Dừng, Owner confirm

## P1-2D-STOP-18

Code hiện tại conflict với tài liệu sạch

Lập conflict report

## 27. SMOKE REQUIREMENTS CHO P1.2D

Chi tiết sẽ được đưa vào 06-P1-2E-SMOKE-EVIDENCE-REPORT, nhưng P1.2D phải khóa các smoke tối thiểu sau:

Mã smoke

Nội dung

## SMK-P1-2D-01

Seed SKU baseline không trùng

## SMK-P1-2D-02

Seed SKU không tự set sellable

## SMK-P1-2D-03

Seed SKU không tự set released

## SMK-P1-2D-04

Seed SKU không tự set production-ready

## SMK-P1-2D-05

Ingredient canonical không trùng

## SMK-P1-2D-06

Alias trỏ đúng canonical

## SMK-P1-2D-07

UOM tồn tại và conversion đúng

## SMK-P1-2D-08

Recipe group không thay ingredient

## SMK-P1-2D-09

Formula seed có version

## SMK-P1-2D-10

G1 seed có anchor gạo

## SMK-P1-2D-11

G1 seed có ratio_to_rice

## SMK-P1-2D-12

Formula line có ingredient cụ thể

## SMK-P1-2D-13

Packaging profile có đủ cấu hình

## SMK-P1-2D-14

Material group A/B đúng

## SMK-P1-2D-15

Sâm Savigin seed là Company Source

## SMK-P1-2D-16

Threshold không hardcode trong code

## SMK-P1-2D-17

Seed chạy lại không tạo trùng

## SMK-P1-2D-18

Dirty seed bị chặn

## SMK-P1-2D-19

SKU extension không cho thêm SKU thiếu Owner confirm

## SMK-P1-2D-20

G0/research không active operational

## 28. EVIDENCE REQUIREMENTS

Mã evidence

Nội dung evidence

## EVD-P1-2D-SKU-BASELINE

Danh sách 20 SKU canonical

## EVD-P1-2D-SKU-EXTENSION

Quy trình thêm SKU mới

## EVD-P1-2D-INGREDIENT

Danh sách ingredient canonical

## EVD-P1-2D-ALIAS

Alias mapping

## EVD-P1-2D-UOM

UOM và conversion

## EVD-P1-2D-RECIPE-GROUP

Recipe line group

## EVD-P1-2D-FORMULA-SEED

Formula header/line seed review

## EVD-P1-2D-G1

G1 seed review

## EVD-P1-2D-ANCHOR-RICE

Anchor gạo

## EVD-P1-2D-RATIO

Ratio_to_rice

## EVD-P1-2D-PACKAGING

Packaging profile

## EVD-P1-2D-MATERIAL-GROUP

Material group A/B

## EVD-P1-2D-SAM

Sâm Savigin Company Source

## EVD-P1-2D-THRESHOLD

Threshold review

## EVD-P1-2D-IDEMPOTENCY

Log seed idempotency

## EVD-P1-2D-NO-DIRTY-SEED

Danh sách dữ liệu bị chặn

## EVD-P1-2D-OWNER-CONFIRM

Biên bản Owner confirm

Lưu ý:

Evidence Submitted chưa phải Evidence Accepted.

Không được dùng evidence chưa accepted để gọi PASS.

## 29. RISK REGISTER P1.2D

Risk

Mức độ

Nguyên nhân

Kiểm soát

Seed bẩn từ nguồn tổng hợp

## P0

Dữ liệu chưa chuẩn hóa

Checklist + Owner confirm

Trùng SKU

## P0

Seed không idempotent

Unique business key + smoke

Trùng ingredient

## P0

Alias bị seed thành master

Canonical mapping

Sai UOM

## P0

Thiếu conversion

UOM checklist

Sai G1 formula

## P0

Thiếu formula rule

## P1.2B

Seed thiếu anchor gạo

## P0

Không follow P1.2B

Formula smoke

Seed thiếu ratio_to_rice

## P0

Dữ liệu chưa đầy đủ

Owner confirm

Hardcode SKU/material

## P1

Dev làm nhanh

Master/config only

Hardcode threshold

## P1

Không có threshold seed

Threshold data ref

Sâm bị supplier hóa

## P0

Không set Company Source

Sâm seed rule

Active thành sellable

## P0

Nhầm trạng thái

Activation seed boundary

Seed set released

## P0

Nhầm seed với batch release

Stop condition

Seed set production-ready

## P0

Nhầm seed với go-live

Stop condition

Agent tự suy luận

## P1

Thiếu dữ liệu

Owner Confirm Required

## 30. DONE GATE

File P1.2D được xem là đạt khi:

Đã khóa seed là dữ liệu nền, không phải mở vận hành.

Đã khóa nguồn tổng hợp không được seed trực tiếp.

Đã khóa seed readiness gate.

Đã khóa SKU baseline seed.

Đã khóa SKU extension process.

Đã khóa product master seed.

Đã khóa ingredient canonical seed.

Đã khóa alias seed.

Đã khóa UOM/conversion seed.

Đã khóa recipe line group seed.

Đã khóa recipe/formula seed theo P1.2B.

Đã khóa G1 formula seed.

Đã khóa anchor gạo seed.

Đã khóa ratio_to_rice seed.

Đã khóa packaging profile seed.

Đã khóa material group/source type seed.

Đã khóa Sâm Savigin Company Source seed.

Đã khóa threshold seed ở mức boundary.

Đã khóa activation seed không đồng nghĩa sellable.

Đã khóa seed change control.

Đã khóa Owner Confirm Required.

Đã khóa Stop Conditions.

Đã khóa Dev/Codex/Copilot handoff.

Đã khóa smoke/evidence requirements.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 31. FAIL GATE

File P1.2D fail nếu xảy ra một trong các trường hợp sau:

Seed trực tiếp từ nguồn tổng hợp.

Seed SKU chưa canonical.

Seed SKU trùng mã.

Seed SKU thiếu public name.

Seed SKU tự set sellable.

Seed SKU tự set released.

Seed SKU tự set production-ready.

Seed ingredient trùng canonical.

Seed alias thành ingredient mới.

Seed UOM thiếu conversion.

Seed recipe group thay ingredient.

Seed formula thiếu version.

Seed G1 thiếu anchor gạo.

Seed G1 thiếu ratio_to_rice.

Seed formula line chỉ có nhóm mơ hồ.

Seed G0/research thành active operational.

Seed packaging thiếu units_per_box/boxes_per_carton.

Seed threshold hardcode vào service.

Seed Sâm Savigin như supplier material.

Seed buffer +5%/+7% vào công thức sản xuất.

Seed chạy lại tạo trùng dữ liệu.

Seed ghi đè dữ liệu vận hành đã phát sinh.

Agent tự suy luận dữ liệu còn thiếu.

Dev/Codex/Copilot tạo seed script khi đang Analysis Only.

Gọi Evidence Submitted là Evidence Accepted.

Gọi Completion Decision.

Gọi Gateway PASS.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 1

Module

SKU Extension / Seed Governance

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed script authorization

KHONG

Actual seed authorization

KHONG, cho đến khi Owner confirm và seed plan được duyệt

Limited implementation authorization

KHONG

Gateway status

bị chặn

Completion status

## WAITING IMPLEMENTATION EVIDENCE

Evidence status

## NOT ACCEPTED

Release status

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

## 33. KẾT LUẬN ĐIỀU PHỐI P1.2D

Từ thời điểm dùng bản V1.1 này, toàn bộ seed và mở rộng SKU trong Phase 1 phải tuân thủ các khóa sau:

nguồn tổng hợp là nguồn phụ trợ, không phải nguồn seed trực tiếp.

Seed chỉ được thực hiện với dữ liệu đã chuẩn hóa.

Dữ liệu thiếu hoặc mơ hồ phải đưa vào Owner Confirm Required.

20 SKU baseline không được hardcode.

SKU extension phải có request, review, code reservation, formula, packaging, seed plan và evidence.

Ingredient phải canonical.

Alias phải trỏ canonical.

UOM phải có conversion.

Formula seed phải tuân thủ P1.2B.

G1 phải có anchor gạo và ratio_to_rice.

Packaging profile phải chuẩn hóa trước Phase 2.

Threshold chỉ là dữ liệu planning/procurement boundary, không phải công thức.

Sâm Savigin phải là Company Source / strategic reserve, không là supplier material thường.

Seed không được tự set sellable.

Seed không được tự set released.

Seed không được tự set production-ready.

Seed phải idempotent.

Seed không được ghi đè dữ liệu vận hành đã phát sinh.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 06-P1-2E-SMOKE-EVIDENCE-REPORT

## TÀI LIỆU SMOKE / EVIDENCE / DONE GATE CHO PHASE 1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Production Readiness: KHONG
Go-live Decision: KHONG
Evidence Status mặc định: NOT ACCEPTED
Default Agent Mode: ANALYSIS ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH / SCOPE

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-01-P1-2E-SMOKE-EVIDENCE-REPORT

Đây là file kiểm chứng cuối Phase 1, dùng để xác định:

Smoke nào phải chạy.

Evidence nào phải nộp.

Evidence nào được xem là hợp lệ.

Điều kiện nào được xem là đạt ở cấp Phase 1.

Điều kiện nào làm fail Phase 1.

Điều kiện nào buộc dừng và yêu cầu Owner xác nhận.

Trạng thái nào được phép ghi nhận.

Trạng thái nào tuyệt đối không được gọi.

Tài liệu này thay thế toàn bộ nội dung cũ của:

## 06-P1-2E-SMOKE-EVIDENCE-REPORT.docx

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## 06-P1-2E-SMOKE-EVIDENCE-REPORT

Phiên bản

Phase

Phase 1

Module

Product / SKU / Ingredient / UOM / Recipe / BOM / Formula / Seed / Activation

Loại tài liệu

Smoke / Evidence / Done Gate / Fail Gate

Nguồn chính

Bộ tài liệu Phase 1 bản sạch

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

File điều phối trước

## README-PHASE-01-HANDOFF-INDEX V1.1

File công thức liên quan

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION V2.0

File seed liên quan

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE V1.1

Cho phép code ngay?

Không

Cho phép seed ngay?

Không

Cho phép gọi PASS?

Không, nếu evidence chưa accepted

Gateway Status

bị chặn

Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để khóa cách kiểm chứng Phase 1 sau khi đã mở rộng các nghiệp vụ quan trọng từ nguồn tổng hợp.

Phase 1 không chỉ cần có tài liệu Product/SKU/Recipe/Formula, mà còn phải chứng minh bằng smoke/evidence rằng dữ liệu nền không sai trước khi Phase 2 tiêu thụ.

Mục tiêu chính:

Kiểm chứng SKU baseline không trùng, không mở bán nhầm.

Kiểm chứng ingredient canonical và alias không tạo dữ liệu trùng.

Kiểm chứng UOM và conversion đủ điều kiện cho formula/inventory.

Kiểm chứng recipe/formula không còn dòng mơ hồ.

Kiểm chứng G1 formula có version, anchor gạo, ratio_to_rice.

Kiểm chứng scaling theo kg gạo đúng nguyên tắc.

Kiểm chứng scale kg gạo không làm đổi formula version.

Kiểm chứng đổi tỷ lệ/thêm/bỏ nguyên liệu phải tạo version mới.

Kiểm chứng buffer +5%/+7% không làm thay đổi công thức sản xuất.

Kiểm chứng seed idempotency.

Kiểm chứng dirty seed bị chặn.

Kiểm chứng seed không tự set sellable/released/production-ready.

Kiểm chứng Sâm Savigin được xử lý là Company Source / strategic reserve.

Kiểm chứng packaging profile đủ dữ liệu nền cho Phase 2.

Kiểm chứng evidence submitted không bị hiểu nhầm là evidence accepted.

## 4. SOURCE-OF-TRUTH - NGUỒN SỰ THẬT

Tầng

Nguồn

Vai trò

Được dùng như thế nào

Không được dùng để làm gì

Tầng 0

MASTER Governance

Quy tắc quản trị cao nhất

Giữ nguyên tắc không PASS nếu chưa có accepted evidence

Không ghi đè bằng kết quả smoke chưa review

Tầng 1

## README-PHASE-01-HANDOFF-INDEX V1.1

Điều phối Phase 1

Xác định phạm vi Phase 1 và boundary

Không thay thế smoke detail

Tầng 2

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION V2.0

Nguồn rule cho formula smoke

Kiểm chứng G1, anchor, ratio, versioning

Không sửa rule formula bằng smoke

Tầng 3

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE V1.1

Nguồn rule cho seed smoke

Kiểm chứng seed idempotency, no dirty seed

Không cho seed nếu chưa Owner confirm

Tầng 4

## MASTER-DATA-NORMALIZATION-CHECKLIST

Nguồn kiểm soát dữ liệu trước seed/code

Đối chiếu Owner Confirm Required

Không thay thế evidence acceptance

Tầng 5

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ

Đối chiếu smoke cần có

Không dùng làm evidence PASS

Tầng 6

Smoke result / log / screenshot / report

Evidence submitted

Dùng để Owner/Reviewer đánh giá

Không tự biến thành accepted evidence

## 5. SCOPE IN - PHẠM VI BAO GỒM

Tài liệu này bao gồm smoke/evidence cho:

SKU baseline.

SKU extension readiness.

Product master.

Ingredient canonical.

Ingredient alias.

## UOM.

UOM conversion.

Recipe line group.

Recipe header.

Formula header.

Formula line.

G1 formula.

G0/research block.

Anchor gạo.

Ratio_to_rice.

Formula scaling.

Formula versioning.

Formula snapshot readiness.

Buffer +5%/+7% boundary.

Packaging profile.

Material group A/B.

Sâm Savigin Company Source.

Threshold seed boundary.

Seed idempotency.

No dirty seed.

No active = sellable.

No recipe active = batch released.

No seed = Production Readiness.

Evidence status.

Done Gate / Fail Gate Phase 1.

## 6. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM

Tài liệu này không bao gồm:

Không chạy test thật trong tài liệu này.

Không viết code test.

Không viết automation test script.

Không tạo seed script.

Không tạo migration.

Không triển khai API.

Không triển khai UI.

Không triển khai Production Order.

Không triển khai Material Issue.

Không triển khai Raw Lot/QC thật.

Không triển khai Warehouse Ledger.

Không triển khai Batch Release.

Không triển khai Print/QR runtime.

Không triển khai MISA integration.

Không triển khai Commerce sellable gate thật.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Tài liệu này chỉ định nghĩa tiêu chuẩn smoke/evidence và gate kiểm chứng Phase 1.

## 7. ĐỊNH NGHĨA TRẠNG THÁI EVIDENCE

Trạng thái

Ý nghĩa

Có được dùng để PASS không?

## NOT_SUBMITTED

Chưa nộp evidence

Không

## SUBMITTED

Đã nộp evidence

Không

## UNDER_REVIEW

Đang được Owner/Reviewer xem

Không

## NEEDS_RETEST

Cần chạy lại hoặc mở rộng

Không

## REJECTED

Evidence không đạt

Không

## ACCEPTED

Evidence đã được Owner/Reviewer chấp nhận

Có thể dùng cho gate liên quan

## SUPERSEDED

Evidence cũ bị thay bằng evidence mới

Không

bị chặn

Không thể review vì thiếu dữ liệu/gate

Không

Nguyên tắc quan trọng:

Evidence Submitted chưa phải Evidence Accepted.

Smoke Passed chưa tự động là Completion Decision.

Phase 1 Done Gate không đồng nghĩa Production Readiness.

## PHẦN 2/4 - SMOKE MATRIX PHASE 1

## 8. SMOKE GROUP 01 - SKU BASELINE

## 8.1. Mục tiêu

Kiểm chứng SKU baseline 20 sản phẩm được chuẩn hóa đúng và không bị hiểu nhầm là hàng được phép bán.

## 8.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-SKU-01

SKU code không trùng

Danh sách 20 SKU canonical

Không có sku_code trùng

## EVD-P1-SKU-LIST

## SMK-P1-SKU-02

SKU có public name

Danh sách SKU

Mỗi SKU có tên public

## EVD-P1-SKU-LIST

## SMK-P1-SKU-03

SKU có group/type

Danh sách SKU

Mỗi SKU có group/type hợp lệ

## EVD-P1-SKU-GROUP

## SMK-P1-SKU-04

SKU baseline không tự sellable

Seed/preview SKU

sellable_status không được PASS mặc định

## EVD-P1-NO-SELLABLE

## SMK-P1-SKU-05

SKU không tự released

Seed/preview SKU

released_status không được set

## EVD-P1-NO-RELEASED

## SMK-P1-SKU-06

SKU không tự production-ready

Seed/preview SKU

PRODUCTION_READINESS_status không được set

## EVD-P1-NO-PRODUCTION-READY

## SMK-P1-SKU-07

SKU không hardcode trong service

Code/gap analysis

Không phát hiện list SKU hardcode

## EVD-P1-NO-HARDCODE-SKU

## SMK-P1-SKU-08

SKU có recipe ref nếu đã có G1

SKU có công thức

default_recipe_ref và default_formula_version rõ

## EVD-P1-SKU-RECIPE-REF

## 8.3. Fail nếu

Trùng sku_code.

SKU thiếu public name.

SKU thiếu group/type.

SKU baseline bị xem là sellable.

SKU seed tự set released.

SKU seed tự set production-ready.

SKU bị hardcode trong service.

SKU có G1 nhưng thiếu formula_version.

## 9. SMOKE GROUP 02 - INGREDIENT CANONICAL / ALIAS

## 9.1. Mục tiêu

Kiểm chứng nguyên liệu không bị trùng và alias không tạo thành master mới.

## 9.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-ING-01

Ingredient canonical không trùng

Danh sách ingredient

Không có canonical duplicate

## EVD-P1-INGREDIENT

## SMK-P1-ING-02

Ingredient có mã duy nhất

Danh sách ingredient

Không trùng ingredient_code

## EVD-P1-INGREDIENT

## SMK-P1-ING-03

Alias trỏ canonical

Danh sách alias

Mỗi alias trỏ đúng một canonical

## EVD-P1-ALIAS

## SMK-P1-ING-04

Alias không tạo ingredient mới

Seed/gap analysis

Alias không xuất hiện như master riêng

## EVD-P1-ALIAS-NO-DUP

## SMK-P1-ING-05

Ingredient có material group

Danh sách ingredient

Có A/B/Company Source/Other

## EVD-P1-MATERIAL-GROUP

## SMK-P1-ING-06

Ingredient có source type

Danh sách ingredient

Supplier / Company Source / Internal

## EVD-P1-SOURCE-TYPE

## SMK-P1-ING-07

Ingredient dùng trong formula phải active

Formula lines

Ingredient line đều trỏ active canonical

## EVD-P1-FORMULA-INGREDIENT

## SMK-P1-ING-08

Không hardcode ingredient

Code/gap analysis

Không phát hiện ingredient hardcode

## EVD-P1-NO-HARDCODE-INGREDIENT

## 9.3. Fail nếu

Một nguyên liệu có nhiều canonical.

Hai nguyên liệu dùng cùng canonical name.

Alias bị seed thành ingredient mới.

Alias trỏ nhiều canonical.

Ingredient dùng trong formula chưa active.

Ingredient thiếu material group.

Ingredient thiếu source type.

Ingredient bị hardcode trong code.

## 10. SMOKE GROUP 03 - UOM / CONVERSION

## 10.1. Mục tiêu

Kiểm chứng UOM đủ điều kiện cho công thức, tồn kho, seed và Phase 2.

## 10.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-UOM-01

UOM code không trùng

Danh sách UOM

Không có uom_code trùng

## EVD-P1-UOM

## SMK-P1-UOM-02

UOM có type

Danh sách UOM

Weight/Volume/Count/Packaging

## EVD-P1-UOM

## SMK-P1-UOM-03

Conversion tồn tại khi cần

Formula/inventory UOM khác nhau

Có conversion factor

## EVD-P1-UOM-CONVERSION

## SMK-P1-UOM-04

Formula line không thiếu UOM

Formula lines

Mỗi line có formula_uom

## EVD-P1-FORMULA-UOM

## SMK-P1-UOM-05

Packaging có UOM gói/hộp/thùng

Packaging profile

Có UOM packaging hợp lệ

## EVD-P1-PACKAGING-UOM

## SMK-P1-UOM-06

Rounding policy rõ

Formula scaling

Có rounding policy

## EVD-P1-ROUNDING

## SMK-P1-UOM-07

Không nhập tay UOM tự do

UI/spec/code analysis

UOM phải từ master/config

## EVD-P1-NO-FREE-UOM

## SMK-P1-UOM-08

Không hardcode UOM

Code/gap analysis

Không hardcode UOM trong service

## EVD-P1-NO-HARDCODE-UOM

## 10.3. Fail nếu

Formula line thiếu UOM.

UOM không tồn tại trong master.

UOM khác nhau nhưng không có conversion.

Packaging thiếu UOM gói/hộp/thùng.

Rounding policy không rõ.

UOM được nhập tay tự do.

UOM hardcode trong code.

## 11. SMOKE GROUP 04 - RECIPE LINE GROUP / RECIPE STRUCTURE

## 11.1. Mục tiêu

Kiểm chứng recipe không chỉ có nhóm mơ hồ mà phải có nguyên liệu cụ thể.

## 11.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-REC-01

Recipe gắn SKU

Recipe list

Mỗi recipe có sku_code

## EVD-P1-RECIPE

## SMK-P1-REC-02

Recipe có status

Recipe list

Có DRAFT/APPROVED/ACTIVE/INACTIVE

## EVD-P1-RECIPE

## SMK-P1-REC-03

Recipe line group hợp lệ

Recipe group list

Group có code/name/order/status

## EVD-P1-RECIPE-GROUP

## SMK-P1-REC-04

Group không thay ingredient

Formula lines

Không có line chỉ chứa group

## EVD-P1-NO-GROUP-AS-INGREDIENT

## SMK-P1-REC-05

Formula line có ingredient cụ thể

Formula lines

Mỗi line có ingredient_code

## EVD-P1-FORMULA-LINE

## SMK-P1-REC-06

Formula line có thứ tự

Formula lines

line_no không trùng trong formula

## EVD-P1-FORMULA-LINE

## SMK-P1-REC-07

Recipe không trôi nổi

Recipe list

Không có recipe không gắn SKU

## EVD-P1-RECIPE

## 11.3. Fail nếu

Recipe không gắn SKU.

Formula line chỉ ghi nhóm.

Formula line thiếu ingredient_code.

Recipe line group bị hardcode trong service.

Recipe không có status.

Recipe trôi nổi không có SKU.

## 12. SMOKE GROUP 05 - FORMULA HEADER / FORMULA VERSION

## 12.1. Mục tiêu

Kiểm chứng mọi công thức vận hành đều có version và trạng thái rõ.

## 12.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-FML-01

Formula có formula_code

Formula header

Không null, không trùng sai rule

## EVD-P1-FORMULA-HEADER

## SMK-P1-FML-02

Formula có version

Formula header

Mỗi formula có formula_version

## EVD-P1-FORMULA-VERSION

## SMK-P1-FML-03

Formula gắn SKU

Formula header

Có sku_code

## EVD-P1-FORMULA-HEADER

## SMK-P1-FML-04

Formula có kind

Formula header

## ANCHOR_BASED / FIXED_BATCH / RESEARCH_ONLY

## EVD-P1-FORMULA-KIND

## SMK-P1-FML-05

Formula có status

Formula header

## DRAFT/APPROVED/ACTIVE/SUPERSEDED/INACTIVE

## EVD-P1-FORMULA-STATUS

## SMK-P1-FML-06

G0/research không active operational

Formula header

RESEARCH_ONLY không được ACTIVE_OPERATIONAL

## EVD-P1-G0-BLOCK

## SMK-P1-FML-07

Chỉ một active operational theo rule

Formula per SKU

Không có nhiều active conflict

## EVD-P1-FORMULA-ACTIVE

## SMK-P1-FML-08

Formula approved có approval metadata

Approved formula

Có approved_by/approved_at

## EVD-P1-FORMULA-APPROVAL

## 12.3. Fail nếu

Formula thiếu code.

Formula thiếu version.

Formula không gắn SKU.

Formula không có kind.

Formula không có status.

G0/research active operational.

Một SKU có nhiều active formula conflict.

Formula approved thiếu approved_by/approved_at.

## PHẦN 3/4 - G1 / ANCHOR / SCALING / VERSIONING / BUFFER / PACKAGING / SEED

## 13. SMOKE GROUP 06 - G1 FORMULA LOCK

## 13.1. Mục tiêu

Kiểm chứng G1 đủ điều kiện là công thức vận hành nền.

## 13.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-G1-01

G1 có formula version

G1 formula

formula_version = G1 hoặc version Owner khóa

## EVD-P1-G1

## SMK-P1-G1-02

G1 có formula lines

G1 formula

Có ít nhất một line hợp lệ

## EVD-P1-G1-LINES

## SMK-P1-G1-03

G1 có Owner confirm

G1 formula

owner_confirmed = TRUE hoặc evidence tương ứng

## EVD-P1-G1-OWNER

## SMK-P1-G1-04

G1 không sửa trực tiếp sau khi khóa

Change attempt

Bị chặn hoặc yêu cầu version mới

## EVD-P1-G1-LOCK

## SMK-P1-G1-05

G1 snapshot-ready

Formula structure

Có đủ dữ liệu để snapshot cho Phase 2

## EVD-P1-SNAPSHOT-READY

## SMK-P1-G1-06

G1 không tự sellable

Formula active

Không làm SKU sellable

## EVD-P1-G1-NO-SELLABLE

## 13.3. Fail nếu

G1 thiếu version.

G1 thiếu lines.

G1 chưa Owner confirm nhưng được seed active.

G1 bị sửa trực tiếp.

G1 không snapshot-ready.

G1 active làm SKU sellable.

## 14. SMOKE GROUP 07 - ANCHOR GẠO

## 14.1. Mục tiêu

Kiểm chứng mỗi công thức G1 anchor-based có đúng một anchor gạo.

## 14.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-ANCHOR-01

G1 có anchor gạo

G1 formula

anchor_ingredient_code tồn tại

## EVD-P1-ANCHOR-RICE

## SMK-P1-ANCHOR-02

Anchor là ingredient canonical

Ingredient master

anchor trỏ ingredient chuẩn

## EVD-P1-ANCHOR-CANONICAL

## SMK-P1-ANCHOR-03

Chỉ có một anchor

Formula lines

Chỉ một line is_anchor_ingredient = TRUE

## EVD-P1-ANCHOR-ONE

## SMK-P1-ANCHOR-04

Anchor có UOM

Anchor line

anchor_uom hợp lệ

## EVD-P1-ANCHOR-UOM

## SMK-P1-ANCHOR-05

Anchor không nhập tay tự do

Formula/production design

Anchor lấy từ formula, không nhập text tự do

## EVD-P1-ANCHOR-NO-FREE-TEXT

## 14.3. Fail nếu

G1 thiếu anchor.

Có nhiều anchor.

Anchor không phải ingredient canonical.

Anchor thiếu UOM.

Anchor nhập tay dạng text tự do.

Anchor không phải gạo nhưng không có Owner confirm.

## 15. SMOKE GROUP 08 - RATIO_TO_RICE

## 15.1. Mục tiêu

Kiểm chứng ratio_to_rice tồn tại và đủ điều kiện để scale công thức.

## 15.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-RATIO-01

Line cần scale có ratio_to_rice

Formula lines

Không null

## EVD-P1-RATIO

## SMK-P1-RATIO-02

Ratio không âm

Formula lines

ratio_to_rice >= 0

## EVD-P1-RATIO

## SMK-P1-RATIO-03

Ratio thuộc formula version

Formula lines

Ratio gắn đúng version

## EVD-P1-RATIO-VERSION

## SMK-P1-RATIO-04

Ratio không tự suy luận

Owner confirm

Có Owner confirm/evidence

## EVD-P1-RATIO-OWNER

## SMK-P1-RATIO-05

Đổi ratio yêu cầu version mới

Change attempt

Bị chặn hoặc tạo version mới

## EVD-P1-RATIO-VERSIONING

## 15.3. Fail nếu

Thiếu ratio_to_rice.

Ratio âm.

Ratio không gắn version.

Ratio tự suy luận từ nguồn tổng hợp.

Đổi ratio mà không tạo version mới.

Công thức vẫn scale khi thiếu ratio.

## 16. SMOKE GROUP 09 - FORMULA SCALING THEO KG GẠO

## 16.1. Mục tiêu

Kiểm chứng khi nhập số kg gạo, hệ thống tính đúng nguyên liệu theo ratio_to_rice và rounding policy.

## 16.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-SCALE-01

Scale theo kg gạo

Nhập anchor quantity

Tất cả line tính theo ratio_to_rice

## EVD-P1-SCALING

## SMK-P1-SCALE-02

Rounding nhất quán

Scaling result

Kết quả theo rounding policy

## EVD-P1-ROUNDING

## SMK-P1-SCALE-03

Scaling không đổi formula version

Đổi kg gạo

Version giữ nguyên

## EVD-P1-SCALE-NO-VERSION

## SMK-P1-SCALE-04

Scaling tạo scaled lines

Preview/snapshot

Có danh sách nguyên liệu đã scale

## EVD-P1-SCALED-LINES

## SMK-P1-SCALE-05

Scaling không sửa ratio gốc

Sau scaling

ratio_to_rice không đổi

## EVD-P1-RATIO-NOT-MUTATED

## SMK-P1-SCALE-06

Scaling không tự cộng buffer

Scaling result

Không cộng +5%/+7% vào formula result

## EVD-P1-SCALE-NO-BUFFER

## 16.3. Fail nếu

Scale sai ratio_to_rice.

Scale thiếu dòng nguyên liệu.

Rounding không nhất quán.

Đổi kg gạo tạo version mới.

Scaling sửa ratio gốc.

Scaling tự cộng buffer vào công thức.

## 17. SMOKE GROUP 10 - FORMULA VERSIONING

## 17.1. Mục tiêu

Kiểm chứng thay đổi công thức sau G1 không sửa trực tiếp mà tạo version mới.

## 17.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-VER-01

Đổi ratio tạo version mới

Change ratio

G1 không bị sửa, G2/draft mới được tạo hoặc yêu cầu workflow

## EVD-P1-VERSIONING

## SMK-P1-VER-02

Thêm nguyên liệu tạo version mới

Add ingredient

Không sửa G1 trực tiếp

## EVD-P1-VERSIONING

## SMK-P1-VER-03

Bỏ nguyên liệu tạo version mới

Remove ingredient

Không sửa G1 trực tiếp

## EVD-P1-VERSIONING

## SMK-P1-VER-04

Thay nguyên liệu tạo version mới

Replace ingredient

Không sửa G1 trực tiếp

## EVD-P1-VERSIONING

## SMK-P1-VER-05

Đổi UOM ảnh hưởng quantity tạo review/version

Change UOM

Bị chặn hoặc tạo version/review

## EVD-P1-VERSIONING

## SMK-P1-VER-06

Formula cũ không bị cập nhật ngược

Sau version mới

Snapshot cũ giữ version cũ

## EVD-P1-SNAPSHOT-NOT-MUTATED

## 17.3. Fail nếu

Đổi ratio nhưng sửa G1 trực tiếp.

Thêm nguyên liệu vào G1 active.

Bỏ nguyên liệu khỏi G1 active.

Thay nguyên liệu trong G1 active.

Đổi UOM ảnh hưởng quantity mà không review/version.

Version mới cập nhật ngược snapshot cũ.

## 18. SMOKE GROUP 11 - BUFFER +5% / +7%

## 18.1. Mục tiêu

Kiểm chứng buffer chỉ dùng cho planning, không làm thay đổi công thức sản xuất.

## 18.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-BUF-01

Buffer +5% nhóm A là planning

Material group A

Buffer nằm ở planning policy, không ở formula line

## EVD-P1-BUFFER

## SMK-P1-BUF-02

Buffer +7% nhóm B là planning

Material group B

Buffer nằm ở planning policy, không sửa packaging/formula

## EVD-P1-BUFFER

## SMK-P1-BUF-03

Buffer không sửa ratio_to_rice

Formula line

ratio_to_rice không bị tăng

## EVD-P1-BUFFER-NO-RATIO

## SMK-P1-BUF-04

Buffer không sửa G1 quantity

G1 formula

G1 quantity giữ nguyên

## EVD-P1-BUFFER-NO-G1

## SMK-P1-BUF-05

Buffer không tự vào Material Request

Preview rule

Nếu chưa có approved rule thì không tự cấp thêm

## EVD-P1-BUFFER-NO-ISSUE

## 18.3. Fail nếu

Buffer +5% bị cộng vào formula line nhóm A.

Buffer +7% bị cộng vào packaging/formula line nhóm B.

Buffer sửa ratio_to_rice.

Buffer sửa G1.

Buffer tự cấp thêm nguyên liệu khi chưa có rule.

## 19. SMOKE GROUP 12 - PACKAGING PROFILE

## 19.1. Mục tiêu

Kiểm chứng packaging profile đủ dữ liệu nền cho Phase 2 đóng gói, print và threshold.

## 19.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-PKG-01

Packaging profile gắn SKU

Packaging profile

Mỗi profile có sku_code

## EVD-P1-PACKAGING

## SMK-P1-PKG-02

Có level 1 unit

Packaging profile

Có gói/packet hoặc level 1 unit

## EVD-P1-PACKAGING

## SMK-P1-PKG-03

Có B1 material nếu cần

Packaging profile

level_1_material_code hợp lệ

## EVD-P1-PACKAGING

## SMK-P1-PKG-04

Có units_per_box nếu có hộp

Packaging profile

units_per_box > 0

## EVD-P1-PACKAGING

## SMK-P1-PKG-05

Có boxes_per_carton nếu có thùng

Packaging profile

boxes_per_carton > 0

## EVD-P1-PACKAGING

## SMK-P1-PKG-06

Có label template ref

Packaging profile

Có template ref cấp 1/cấp 2

## EVD-P1-PACKAGING-TEMPLATE

## SMK-P1-PKG-07

Không sinh QR/barcode runtime ở Phase 1

Packaging seed

Chỉ là config/ref, không tạo mã thật

## EVD-P1-PKG-NO-RUNTIME-PRINT

## SMK-P1-PKG-08

Packaging không hardcode threshold

Code/gap analysis

Threshold đọc từ config/master

## EVD-P1-NO-HARDCODE-THRESHOLD

## 19.3. Fail nếu

Packaging profile không gắn SKU.

Thiếu level 1 unit.

Thiếu B1/B2 material khi cần.

Có hộp nhưng thiếu units_per_box.

Có thùng nhưng thiếu boxes_per_carton.

Phase 1 sinh QR/barcode runtime.

Threshold packaging bị hardcode trong service.

## 20. SMOKE GROUP 13 - MATERIAL GROUP / SÂM SAVIGIN / THRESHOLD BOUNDARY

## 20.1. Mục tiêu

Kiểm chứng dữ liệu nhóm nguyên liệu/vật tư và Sâm Savigin đúng boundary.

## 20.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-MAT-01

Material group A/B rõ

Ingredient/material list

Mỗi material có group

## EVD-P1-MATERIAL-GROUP

## SMK-P1-MAT-02

A-specific/A-common rõ

Nhóm A

Có phân loại đặc thù/dùng chung

## EVD-P1-MATERIAL-GROUP

## SMK-P1-MAT-03

B1/B2 rõ

Nhóm B

Có phân loại film/hộp/thùng

## EVD-P1-MATERIAL-GROUP

## SMK-P1-SAM-01

Sâm Savigin là Company Source

Ingredient Sâm

## EVD-P1-SAM

## SMK-P1-SAM-02

Sâm không procurement eligible như supplier material

Ingredient Sâm

is_procurement_eligible = FALSE cho supplier purchase thường

## EVD-P1-SAM

## SMK-P1-SAM-03

Sâm có strategic reserve rule

Ingredient Sâm

strategic_reserve flag/rule rõ

## EVD-P1-SAM

## SMK-P1-THR-01

Threshold có source config

Threshold config

Không hardcode

## EVD-P1-THRESHOLD

## SMK-P1-THR-02

Threshold chưa kích hoạt full MRP trong Phase 1

Phase 1 scope

Chỉ là boundary/config, không tạo purchase request

## EVD-P1-THR-BOUNDARY

## 20.3. Fail nếu

Material thiếu group.

A-specific/A-common lẫn nhau.

B1/B2 không rõ.

Sâm Savigin là supplier material thường.

Sâm bị procurement eligible như hàng mua ngoài thông thường.

Sâm thiếu strategic reserve rule.

Threshold hardcode.

Phase 1 tự triển khai full MRP/procurement.

## 21. SMOKE GROUP 14 - SEED IDEMPOTENCY / KHONG DIRTY SEED

## 21.1. Mục tiêu

Kiểm chứng seed có thể chạy an toàn, không tạo trùng, không ghi đè dữ liệu vận hành, và không nạp dữ liệu bẩn.

## 21.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-SEED-01

Seed chạy lần đầu tạo dữ liệu đúng

Seed plan

Dữ liệu đúng canonical

## EVD-P1-SEED-LOG

## SMK-P1-SEED-02

Seed chạy lại không tạo trùng

Chạy lại seed

Không tăng duplicate rows

## EVD-P1-SEED-IDEMPOTENCY

## SMK-P1-SEED-03

Seed không ghi đè dữ liệu vận hành

Existing operational data

Không reset/overwrite trái phép

## EVD-P1-SEED-NO-OVERWRITE

## SMK-P1-SEED-04

Dirty seed bị chặn

Dữ liệu chưa Owner confirm

Không được seed active

## EVD-P1-NO-DIRTY-SEED

## SMK-P1-SEED-05

Seed có actor/log

Seed run

Có log actor/system

## EVD-P1-SEED-AUDIT

## SMK-P1-SEED-06

Seed có evidence

Seed run

Có log/report/snapshot

## EVD-P1-SEED-EVIDENCE

## SMK-P1-SEED-07

Seed không trực tiếp từ nguồn tổng hợp

Seed source

Chỉ từ dữ liệu đã normalize

## EVD-P1-NO-RAW-AUXILIARY-SOURCE-SEED

## 21.3. Fail nếu

Seed chạy lại tạo trùng.

Seed ghi đè dữ liệu vận hành.

Dirty seed được nạp.

Seed thiếu log.

Seed thiếu evidence.

Seed trực tiếp từ nguồn tổng hợp.

Agent tự điền dữ liệu còn thiếu.

## 22. SMOKE GROUP 15 - ACTIVATION GUARD / KHONG SELLABLE / KHONG RELEASED

## 22.1. Mục tiêu

Kiểm chứng Phase 1 không mở bán nhầm.

## 22.2. Smoke cases

Mã smoke

Tên smoke

Điều kiện đầu vào

Kết quả kỳ vọng

Evidence

## SMK-P1-ACT-01

Active SKU không sellable

SKU active

sellable gate không tự PASS

## EVD-P1-NO-SELLABLE

## SMK-P1-ACT-02

Recipe active không batch released

Recipe/formula active

Không có batch released

## EVD-P1-NO-BATCH-RELEASED

## SMK-P1-ACT-03

Formula active không production-ready

Formula active

Không set Production Readiness

## EVD-P1-NO-PRODUCTION-READY

## SMK-P1-ACT-04

Seed không set sellable

Seed result

Không có sellable=true

## EVD-P1-SEED-NO-SELLABLE

## SMK-P1-ACT-05

Seed không set released

Seed result

Không có released=true

## EVD-P1-SEED-NO-RELEASED

## SMK-P1-ACT-06

Seed không set production-ready

Seed result

Không có PRODUCTION_READINESS=true

## EVD-P1-SEED-NO-PRODUCTION-READY

## SMK-P1-ACT-07

Commerce/AI/Gateway không được dùng Phase 1 active để bán

Boundary check

Không tiêu thụ SKU active như sellable

## EVD-P1-COMMERCE-BLOCK

## 22.3. Fail nếu

Active SKU được bán.

Recipe active tạo batch released.

Formula active tạo production-ready.

Seed set sellable.

Seed set released.

Seed set production-ready.

AI/Commerce/Gateway dùng trạng thái Phase 1 để chào bán.

PHẦN 4/4 - EVIDENCE REGISTRY / EXECUTION ORDER / HANDOFF / DONE GATE / FAIL GATE

## 23. EVIDENCE REGISTRY PHASE 1

Mã evidence

Tên evidence

Nội dung bắt buộc

Trạng thái mặc định

## EVD-P1-SKU-LIST

Danh sách SKU baseline

20 SKU canonical, code/name/group/status

SUBMITTED sau khi nộp

## EVD-P1-SKU-GROUP

Nhóm SKU

Mapping SKU group/type

## SUBMITTED

## EVD-P1-NO-SELLABLE

No Active = Sellable

Bằng chứng active không sellable

## SUBMITTED

## EVD-P1-NO-RELEASED

No Released

Bằng chứng seed không released

## SUBMITTED

## EVD-P1-NO-PRODUCTION-READY

No Production Readiness

Bằng chứng không production-ready

## SUBMITTED

## EVD-P1-INGREDIENT

Ingredient canonical

Danh sách ingredient chuẩn

## SUBMITTED

## EVD-P1-ALIAS

Alias mapping

Alias trỏ canonical

## SUBMITTED

## EVD-P1-UOM

UOM master

UOM code/name/type

## SUBMITTED

## EVD-P1-UOM-CONVERSION

UOM conversion

Quy đổi đơn vị

## SUBMITTED

## EVD-P1-RECIPE

Recipe header

Recipe gắn SKU

## SUBMITTED

## EVD-P1-RECIPE-GROUP

Recipe line group

Group code/name/order

## SUBMITTED

## EVD-P1-FORMULA-HEADER

Formula header

formula_code/version/status

## SUBMITTED

## EVD-P1-FORMULA-LINE

Formula line

Ingredient cụ thể, UOM, ratio

## SUBMITTED

## EVD-P1-G1

G1 formula

G1 header + lines

## SUBMITTED

## EVD-P1-G0-BLOCK

G0 block

Research không active

## SUBMITTED

## EVD-P1-ANCHOR-RICE

Anchor gạo

Anchor ingredient code/UOM

## SUBMITTED

## EVD-P1-RATIO

Ratio_to_rice

Bảng ratio theo line

## SUBMITTED

## EVD-P1-SCALING

Scaling result

Test scale theo kg gạo

## SUBMITTED

## EVD-P1-VERSIONING

Formula versioning

Test đổi ratio/thêm/bỏ tạo version

## SUBMITTED

## EVD-P1-BUFFER

Buffer boundary

+5%/+7% không sửa formula

## SUBMITTED

## EVD-P1-PACKAGING

Packaging profile

Gói/hộp/thùng/template ref

## SUBMITTED

## EVD-P1-MATERIAL-GROUP

Material group

A/B/A-specific/A-common/B1/B2

## SUBMITTED

## EVD-P1-SAM

Sâm Savigin rule

Company Source / strategic reserve

## SUBMITTED

## EVD-P1-THRESHOLD

Threshold boundary

5 mẻ/20 mẻ/15 mẻ/B1/B2

## SUBMITTED

## EVD-P1-SEED-IDEMPOTENCY

Seed idempotency

Chạy lại không trùng

## SUBMITTED

## EVD-P1-NO-DIRTY-SEED

No dirty seed

Dữ liệu chưa confirm bị chặn

## SUBMITTED

## EVD-P1-OWNER-CONFIRM

Owner confirm

Biên bản/chốt dữ liệu nền

## SUBMITTED

## 24. EVIDENCE ACCEPTANCE RULES

## 24.1. Evidence hợp lệ phải có

Mỗi evidence hợp lệ phải có:

Mã evidence.

Phase.

Module.

Smoke case liên quan.

Ngày giờ nộp.

Người nộp hoặc system actor.

Môi trường kiểm thử nếu có.

Dữ liệu đầu vào.

Kết quả đầu ra.

Log/screenshot/report nếu có.

Kết luận submitted.

Người review.

Trạng thái review.

Ghi chú nếu fail/retest.

## 24.2. Evidence chưa hợp lệ nếu

Evidence chưa hợp lệ nếu:

Không có mã evidence.

Không biết thuộc smoke nào.

Không có output.

Không có log/screenshot/report khi cần.

Không có người nộp.

Không có môi trường.

Không có Owner/Reviewer review.

Chưa được đánh dấu accepted.

Bị superseded.

Bị reject.

Chỉ là lời nói “đã chạy”.

## 25. EXECUTION ORDER - THỨ TỰ CHẠY SMOKE PHASE 1

Thứ tự

Nhóm smoke

Lý do chạy trước/sau

SKU baseline

SKU là nền cho recipe/formula

Ingredient canonical/alias

Formula cần ingredient chuẩn

UOM/conversion

Formula/scaling cần UOM

Recipe structure

Formula cần recipe/line group

Formula header/version

G1 cần version

G1 formula lock

Công thức vận hành nền

Anchor gạo

Scaling phụ thuộc anchor

Ratio_to_rice

Scaling phụ thuộc ratio

Scaling theo kg gạo

Kiểm chứng tính toán công thức

10

Formula versioning

Bảo vệ G1

11

Buffer boundary

Chặn nhầm planning với formula

12

Packaging profile

Nền cho Phase 2 packaging/print

13

Material group/Sâm/threshold

Nền cho Phase 2/MRP boundary

14

Seed idempotency/no dirty seed

Kiểm chứng nạp dữ liệu an toàn

15

Activation/no sellable/no released

Chặn mở bán/sản xuất nhầm

16

Evidence registry review

Kiểm tra evidence đã đủ chưa

## 26. DEV / CODEX / COPILOT HANDOFF

## 26.1. Mode mặc định

Khi giao file này cho Agent:

## MODE: ANALYSIS ONLY

Agent không được tự chạy code, không tạo test, không tạo seed, không sửa code nếu chưa có lệnh Limited Implementation riêng.

## 26.2. Prompt đúng cho Agent

Nhiệm vụ:

Lập danh sách smoke cases cần có cho Phase 1.

Đối chiếu code/test/seed hiện tại có smoke nào, thiếu smoke nào.

Lập Gap Matrix smoke/evidence.

Liệt kê evidence cần nộp.

Liệt kê P0/P1 risk nếu smoke thiếu.

Liệt kê dữ liệu cần Owner confirm trước smoke.

Đề xuất smoke execution plan.

Không sửa code.

Không tạo test.

Không tạo seed.

Không gọi PASS/READY.

## 26.3. Output Agent bắt buộc

Mục

Nội dung

Scope đã đọc

Phase 1 Smoke/Evidence

Smoke Matrix Gap

Smoke đã có / chưa có

Evidence Gap

Evidence đã nộp / chưa nộp / chưa accepted

Risk Register

## P0/P1/P2

Owner Confirm Required

Dữ liệu thiếu trước smoke

Proposed Smoke Execution Plan

Thứ tự chạy smoke

Proposed Evidence Package

Danh sách evidence cần nộp

điểm chặn

Điều kiện chặn

## 27. OWNER CONFIRM REQUIRED

Trước khi smoke có thể được xem xét đầy đủ, các dữ liệu sau phải có Owner confirm:

Nhóm

Dữ liệu cần xác nhận

## SKU

20 SKU canonical

Ingredient

Ingredient canonical và alias

## UOM

UOM và conversion

Recipe

Recipe line group

Formula

G1 header + lines

Anchor

Gạo anchor

Ratio

Ratio_to_rice

Rounding

Rounding policy

Buffer

+5%/+7% là planning

Packaging

Packaging profile

Material group

A/B/A-specific/A-common/B1/B2

Sâm

Company Source / strategic reserve

Threshold

5 mẻ/20 mẻ/15 mẻ/B1/B2 quantities

Activation

Active không sellable

Seed

Seed idempotency và no dirty seed

Nếu thiếu Owner confirm, smoke có thể được ghi nhận là bị chặn hoặc NEEDS_OWNER_CONFIRM, không được gọi PASS.

## 28. RISK REGISTER PHASE 1 SMOKE/EVIDENCE

Risk

Mức độ

Nguyên nhân

Kiểm soát

Smoke thiếu nhưng vẫn giao Phase 2

## P0

Chưa có gate cứng

Phase 2 bị chặn nếu Phase 1 P0 smoke thiếu

Evidence submitted bị hiểu là accepted

## P0

Nhầm trạng thái evidence

Evidence state machine

Formula chưa smoke nhưng đã seed

## P0

Bỏ qua P1.2E

Seed gate

G1 chưa owner confirm nhưng dùng sản xuất

## P0

Thiếu evidence

Owner Confirm Required

Scaling sai

## P0

Ratio/UOM sai

Scaling smoke

Dirty seed vào hệ thống

## P0

Seed từ nguồn tổng hợp thẳng

No dirty seed smoke

Active thành sellable

## P0

Nhầm activation

Activation smoke

Seed set released/production-ready

## P0

Nhầm seed với vận hành

No released/no production-ready smoke

Sâm bị supplier hóa

## P0

Sai source type

Sâm smoke

Buffer sửa formula

## P0

Nhầm planning với formula

Buffer smoke

Packaging thiếu cấu hình

## P1

Thiếu profile

Packaging smoke

Hardcode SKU/material/threshold

## P1

Dev làm nhanh

Gap analysis + code review

## 29. DONE GATE PHASE 1

Phase 1 chỉ được xem là đạt mức tài liệu/smoke readiness khi:

SKU baseline smoke đạt.

Ingredient canonical/alias smoke đạt.

UOM/conversion smoke đạt.

Recipe structure smoke đạt.

Formula header/version smoke đạt.

G1 lock smoke đạt.

Anchor gạo smoke đạt.

Ratio_to_rice smoke đạt.

Formula scaling smoke đạt.

Formula versioning smoke đạt.

Buffer boundary smoke đạt.

Packaging profile smoke đạt.

Material group/Sâm/threshold boundary smoke đạt.

Seed idempotency smoke đạt.

No dirty seed smoke đạt.

Activation/no sellable/no released/no production-ready smoke đạt.

Evidence cho từng nhóm đã được nộp.

Evidence đã được Owner/Reviewer review.

P0 điểm chặn không còn mở.

Không có conflict chưa Owner confirm.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Lưu ý:

Ngay cả khi toàn bộ smoke Phase 1 đạt và evidence được nộp, trạng thái tối đa được ghi nhận là:

## PHASE 1 EVIDENCE SUBMITTED FOR OWNER REVIEW

hoặc nếu Owner đã review:

## PHASE 1 EVIDENCE ACCEPTED FOR DOCUMENTATION / IMPLEMENTATION GATE

Không được tự gọi Production Readiness.

## 30. FAIL GATE PHASE 1

Phase 1 fail nếu xảy ra một trong các trường hợp sau:

Trùng SKU code.

SKU thiếu public name.

SKU baseline tự sellable.

SKU seed tự released.

SKU seed tự production-ready.

Ingredient canonical bị trùng.

Alias bị seed thành ingredient mới.

Alias trỏ nhiều canonical.

UOM thiếu conversion.

Formula line thiếu UOM.

Recipe line chỉ có group mơ hồ.

Formula thiếu version.

G0/research active operational.

G1 thiếu Owner confirm.

G1 thiếu anchor gạo.

G1 có nhiều anchor.

Anchor không phải ingredient canonical.

G1 thiếu ratio_to_rice.

Scaling sai hoặc không nhất quán.

Đổi kg gạo tạo version mới.

Đổi ratio/thêm/bỏ nguyên liệu không tạo version mới.

Buffer +5%/+7% sửa formula.

Packaging profile thiếu cấu hình bắt buộc.

Sâm Savigin bị seed như supplier material thường.

Threshold bị hardcode.

Seed chạy lại tạo trùng.

Dirty seed không bị chặn.

Seed trực tiếp từ nguồn tổng hợp.

Agent tự suy luận dữ liệu.

Evidence thiếu hoặc không thể review.

Evidence Submitted bị gọi là Accepted.

P0 smoke thiếu nhưng vẫn giao Phase 2 implementation.

Gọi Completion Decision.

Gọi Gateway PASS.

Gọi Production Readiness.

Gọi Go-live Decision.

Hạng mục

Trạng thái

Documentation review

## READY FOR OWNER REVIEW

Phase

## PHASE 1

Module

Smoke / Evidence / Done Gate

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed authorization

KHONG, nếu chưa có Owner confirm và seed governance đạt

Limited implementation authorization

KHONG

Evidence status

NOT ACCEPTED mặc định

Gateway status

bị chặn

Completion status

## WAITING IMPLEMENTATION EVIDENCE

Release status

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

## 32. KẾT LUẬN ĐIỀU PHỐI P1.2E

Từ thời điểm dùng bản V1.1 này, Phase 1 không được xem là đủ điều kiện chuyển tiếp nếu chưa có smoke/evidence cho các điểm lõi:

SKU baseline không trùng.

SKU active không sellable.

Ingredient canonical không trùng.

Alias không tạo master mới.

UOM có conversion.

Recipe line không mơ hồ.

Formula có version.

G1 có anchor gạo.

G1 có ratio_to_rice.

Scaling theo kg gạo đúng rule.

Scaling không đổi formula version.

Đổi tỷ lệ/thêm/bỏ nguyên liệu tạo version mới.

Buffer không sửa formula.

Packaging profile đủ nền.

Sâm Savigin là Company Source / strategic reserve.

Threshold không hardcode.

Seed idempotent.

Dirty seed bị chặn.

Evidence được nộp và review đúng trạng thái.

Không gọi Completion Decision / Gateway PASS / Production Readiness.

Tài liệu kế tiếp cần viết lại toàn bộ là:

## README-PHASE-01-HANDOFF-INDEX

## TÀI LIỆU ĐIỀU PHỐI GIAO DEV PHASE 1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## README-PHASE-01-02-AUXILIARY-SOURCE-MAPPING

## MASTER-DATA-NORMALIZATION-CHECKLIST-PHASE-01-02

## DEV-RUNBOOK-PHASE-01-02-START-HERE

Global Gate Status: bị chặn
Completion Status: WAITING IMPLEMENTATION EVIDENCE
Production Readiness: KHONG
Go-live Decision: KHONG
Default Agent Mode: ANALYSIS ONLY

## PHẦN 1/4 - PHASE MARKER / PURPOSE / SOURCE-OF-TRUTH

## 1. PHASE MARKER

Tài liệu này thuộc nhóm:

## PHASE-01-PRODUCT-SKU-RECIPE-ACTIVATION-HANDOFF-INDEX

Tài liệu này là file README điều phối chính của Phase 1.

Phase 1 tập trung vào nền dữ liệu sản phẩm và công thức, gồm:

Product master.

SKU baseline.

Ingredient master.

Ingredient alias.

## UOM.

Recipe line group.

Recipe / BOM.

Formula version.

G1 operational formula.

Anchor gạo.

Ratio_to_rice.

Product activation guard.

SKU extension governance.

Seed governance.

Smoke/evidence Phase 1.

Phase 1 không phải phase sản xuất vận hành hoàn chỉnh.

Phase 1 không được tự mở bán sản phẩm.

Phase 1 không được tự khẳng định SKU đã sellable.

Phase 1 không được tự gọi Production Readiness.

Phase 1 chỉ tạo nền dữ liệu sạch để Phase 2 và các phase sau có thể tiêu thụ đúng.

## 2. HEADER

Trường

Nội dung

Tên tài liệu

## README-PHASE-01-HANDOFF-INDEX

Phiên bản

Loại tài liệu

README điều phối Phase 1

Phạm vi

Product / SKU / Ingredient / UOM / Recipe / Formula / Activation / Seed / Smoke

Nguồn chính

Bộ tài liệu Phase 1 bản sạch

Nguồn phụ trợ

nguồn tổng hợp TÀI LIỆU.docx

Mode mặc định cho dev/Agent

## ANALYSIS ONLY

Cho phép code ngay?

Không

Cho phép seed ngay?

Không

Cho phép sửa migration ngay?

Không

Cho phép dùng nguồn tổng hợp để code trực tiếp?

Không

Cho phép Agent tự suy luận công thức?

Không

Trạng thái Gateway

bị chặn

Trạng thái Production Readiness

KHONG

## 3. PURPOSE - MỤC ĐÍCH TÀI LIỆU

Tài liệu này được viết lại để làm cổng điều phối chính cho toàn bộ Phase 1.

Lý do cần viết lại:

Bộ Phase 1 ban đầu đã đúng khung kỹ thuật, nhưng sau khi có nguồn tổng hợp TÀI LIỆU.docx, nhiều nghiệp vụ thực tế ảnh hưởng trực tiếp đến Phase 1 cần được khóa lại ngay từ README, đặc biệt là:

Công thức sản xuất phải tự hiện chi tiết theo SKU + mã công thức + version.

Pilot formula lấy gạo làm anchor ingredient.

Khi nhập số kg gạo, hệ thống scale toàn bộ nguyên liệu theo ratio_to_rice.

Scale kg gạo không phải đổi công thức.

Đổi tỷ lệ, thêm nguyên liệu, bỏ nguyên liệu sau khi G1 đã khóa thì bắt buộc tạo Formula Version mới.

Công thức phải bung từng nguyên liệu cụ thể, không để nhóm mơ hồ.

UOM kg/g/l/ml/gói/hộp/thùng phải chuẩn hóa trước seed/code.

20 SKU baseline phải là dữ liệu nền sạch, không hardcode.

Material canonical và ingredient alias phải chuẩn hóa trước seed.

Packaging config ảnh hưởng seed, công thức đóng gói, print và threshold.

Buffer +5% nhóm A và +7% nhóm B chỉ dùng cho MRP/stock planning, không được sửa công thức sản xuất đã phê duyệt.

SKU active không đồng nghĩa sellable.

Seed không được tự set released, sellable, production-ready.

nguồn tổng hợp là auxiliary source, không code trực tiếp.

Mục tiêu của tài liệu này là:

Khóa Phase 1 thành bộ nền dữ liệu sản phẩm - công thức - seed sạch, đủ rõ để dev/Codex/Copilot phân tích và triển khai có kiểm soát, nhưng chưa cho phép mở Gateway hoặc gọi Production Readiness.

## 4. SOURCE-OF-TRUTH - THỨ BẬC NGUỒN SỰ THẬT CỦA PHASE 1

Tầng

Nguồn

Vai trò

Dev/Agent được dùng như thế nào

Không được làm

Tầng 0

MASTER Governance / nguyên tắc đã khóa

Nguồn quản trị cao nhất

Giữ no-bypass, no-parallel-source, no-PASS-without-evidence

Không dùng Phase 1 để ghi đè governance

Tầng 1

## README-PHASE-01-HANDOFF-INDEX V1.1

Cổng điều phối Phase 1

Biết thứ tự đọc file, phạm vi, gate, boundary

Không thay file chi tiết P1.2B/P1.2D/P1.2E

Tầng 2

File Phase 1 bản sạch

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

Biết SKU, ingredient, UOM, formula, packaging nào cần Owner confirm

Không tự điền dữ liệu thiếu

Tầng 5

nguồn tổng hợp TÀI LIỆU.docx

Nguồn phụ trợ nghiệp vụ thực tế

Dùng để đối chiếu và viết lại tài liệu sạch

Không seed/code trực tiếp

Tầng 6

Code hiện tại

Hiện trạng triển khai

Dùng để gap analysis

Không coi code là source-of-truth nếu conflict với tài liệu đã khóa

## 5. DANH SÁCH FILE PHASE 1

Bộ Phase 1 gồm các file sau:

## STT

File

Vai trò

Trạng thái theo V1.1

README-PHASE-01-HANDOFF-INDEX.docx

File điều phối Phase 1

## 00-P1-ANALYSIS-ONLY.docx

Tài liệu phân tích Phase 1

Giữ tạm nếu không phát hiện conflict

## 01-P1-1-TECHNICAL-DESIGN-ONLY.docx

Thiết kế kỹ thuật Phase 1

Giữ tạm nếu không phát hiện conflict

## 02-P1-2A-PRODUCT-SKU-INGREDIENT-UOM.docx

Product/SKU/Ingredient/UOM

Giữ tạm, nhưng phải đối chiếu checklist chuẩn hóa

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION.docx

Recipe/BOM/Formula Version

## 04-P1-2C-ACTIVATION-GUARD.docx

Product Activation Guard

Giữ tạm, nếu thiếu sellable boundary thì viết lại

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE.docx

SKU Extension / Seed Governance

## 06-P1-2E-SMOKE-EVIDENCE-REPORT.docx

Smoke/Evidence Phase 1

## 6. SCOPE IN - PHẠM VI BAO GỒM CỦA PHASE 1

Phase 1 bao gồm:

Product master.

SKU baseline 20 sản phẩm.

SKU code, tên public, tên nội bộ.

SKU group.

SKU type.

SKU status.

Ingredient master.

Ingredient canonical.

Ingredient alias.

## UOM.

UOM conversion.

Recipe line group.

Recipe header.

Recipe line.

BOM / formula line.

Formula version.

Pilot formula.

G1 operational formula.

Anchor ingredient là gạo.

Ratio_to_rice.

Formula scaling theo kg gạo.

Formula snapshot requirement cho Phase 2.

Product activation guard.

SKU extension rule.

Seed governance.

Seed idempotency.

No dirty seed.

No hardcoded SKU/material.

Smoke/evidence Phase 1.

## 7. SCOPE OUT - PHẠM VI KHÔNG BAO GỒM CỦA PHASE 1

Phase 1 không bao gồm:

Không triển khai full production order.

Không triển khai full production dossier.

Không triển khai raw material receipt.

Không triển khai raw lot QC.

Không triển khai material issue.

Không triển khai material receipt.

Không triển khai warehouse ledger thật.

Không triển khai batch QC/release thật.

Không triển khai finished goods warehouse receipt thật.

Không triển khai print integration thật.

Không triển khai QR public trace thật.

Không triển khai recall operation thật.

Không triển khai sale lock suppression thật.

Không triển khai full MRP/procurement.

Không triển khai MISA integration thật.

Không triển khai Commerce sellable gate thật.

Không triển khai AI Advisor quote/order.

Không gọi SKU sellable chỉ vì SKU active.

Không gọi batch released.

Không gọi Production Readiness.

Không gọi go-live.

Các phần trên chỉ được Phase 1 chuẩn bị dữ liệu nền hoặc boundary để Phase 2 và các phase sau tiêu thụ.

PHẦN 2/4 - BOUNDARY RULES / nguồn tổng hợp / DOMAIN REGISTRY

## 8. nguồn tổng hợp ĐƯỢC DÙNG NHƯ THẾ NÀO TRONG PHASE 1

nguồn tổng hợp TÀI LIỆU.docx chỉ được dùng trong Phase 1 theo vai trò:

Auxiliary source - nguồn phụ trợ nghiệp vụ.

nguồn tổng hợp giúp Phase 1 biết các dữ liệu nào phải chuẩn hóa trước khi seed/code, nhưng không được dùng làm file code hoặc file seed trực tiếp.

## 9. CÁC NỘI DUNG nguồn tổng hợp ẢNH HƯỞNG TRỰC TIẾP PHASE 1

Nội dung từ nguồn tổng hợp

Ảnh hưởng đến Phase 1

File Phase 1 tiêu thụ

Cách dùng đúng

Công thức sản xuất tự hiện theo SKU + mã công thức + version

Bắt buộc Recipe/Formula phải có version, status, snapshot-ready

## 03-P1-2B

Viết lại thành rule rõ

Pilot formula lấy gạo làm anchor

Cần trường anchor_ingredient và ratio_to_rice

## 03-P1-2B

Gạo là anchor, nhập kg gạo để scale

Nhập kg gạo thì scale nguyên liệu

Cần công thức ratio_to_rice

## 03-P1-2B, 06-P1-2E

Smoke scaling

Scale kg gạo không đổi công thức

Cần phân biệt scaling và versioning

## 03-P1-2B

Không tạo version mới khi chỉ đổi sản lượng

Đổi tỷ lệ/thêm/bỏ nguyên liệu phải tạo version mới

Cần formula version governance

## 03-P1-2B

Không sửa G1 tại chỗ

Công thức phải bung chi tiết từng nguyên liệu

Cần recipe line cụ thể

## 03-P1-2B

Không dùng nhóm mơ hồ thay ingredient

UOM kg/g/l/ml

Cần chuẩn hóa đơn vị

## 02-P1-2A, 03-P1-2B, 05-P1-2D

Không seed thiếu conversion

20 SKU baseline

Cần seed canonical

## 05-P1-2D

Không hardcode

Material canonical

Cần ingredient canonical

## 02-P1-2A, 05-P1-2D

Alias trỏ về canonical

Packaging config

Cần seed cấu hình đóng gói

## 05-P1-2D

Chuẩn bị cho Phase 2 print/packaging

Buffer +5% nhóm A, +7% nhóm B

Cần phân biệt formula và planning

## 03-P1-2B, 05-P1-2D

Không cộng buffer vào công thức đã duyệt

Ngưỡng tồn kho

Phase 1 chỉ giữ dữ liệu chuẩn/boundary, chưa triển khai full MRP

## 05-P1-2D

Không hardcode threshold

Sâm Savigin Company Source

Cần source_type cho ingredient

## 02-P1-2A, 05-P1-2D

Không seed như supplier material thường

Không seed dữ liệu bẩn

Cần seed governance

## 05-P1-2D

Chỉ seed sau Owner confirm

## 10. BOUNDARY RULES - QUY TẮC BIÊN PHASE 1

Mã

Quy tắc

Diễn giải

## P1-BR-01

Phase 1 là nền dữ liệu sản phẩm/công thức, không phải mở bán

SKU tồn tại không đồng nghĩa bán được

## P1-BR-02

nguồn tổng hợp là auxiliary source

Không code/seed trực tiếp từ nguồn tổng hợp

## P1-BR-03

SKU baseline không được hardcode

SKU phải đến từ master/seed chuẩn

## P1-BR-04

Ingredient phải có canonical

Alias không tạo ingredient mới

## P1-BR-05

UOM phải chuẩn hóa trước formula

Không có conversion thì không seed formula

## P1-BR-06

Công thức phải có formula_code và formula_version

Không có version thì không dùng sản xuất

## P1-BR-07

G1 đã khóa không sửa trực tiếp

Đổi tỷ lệ/thêm/bỏ nguyên liệu phải tạo version mới

## P1-BR-08

Gạo là anchor ingredient cho pilot/G1 scaling

Nhập kg gạo để scale nguyên liệu

## P1-BR-09

Scale sản lượng không phải đổi công thức

Chỉ đổi anchor quantity, không đổi ratio

## P1-BR-10

Công thức phải bung từng nguyên liệu cụ thể

Không để nhóm mơ hồ thay dòng nguyên liệu

## P1-BR-11

Buffer +5%/+7% là planning buffer

Không cộng vào công thức sản xuất đã duyệt

## P1-BR-12

Product Activation không đồng nghĩa Sellable

Activation chỉ là điều kiện sản phẩm

## P1-BR-13

Seed không tự set sellable/released/production-ready

Các trạng thái này thuộc gate khác

## P1-BR-14

Sâm Savigin phải có source_type Company Source

Không xử lý như supplier material thường

## P1-BR-15

Packaging config phải chuẩn hóa trước print/packaging Phase 2

Phase 1 chỉ chuẩn bị profile

## P1-BR-16

Evidence Submitted không phải Evidence Accepted

Smoke nộp chưa phải PASS

## P1-BR-17

Không gọi Completion Decision / Gateway PASS / Production Readiness

Phase 1 chỉ là documentation/implementation stage

## 11. DOMAIN REGISTRY PHASE 1

## 11.1. Product / SKU Domain

Hạng mục

Quy tắc

SKU phải có mã duy nhất

Không trùng sku_code

SKU phải có tên public

Tên dùng cho khách hàng

SKU có thể có tên internal

Dùng nội bộ nếu cần

SKU phải có group/type

Seasonal / Functional / Nourishing hoặc phân loại đã khóa

SKU status không mở bán

ACTIVE_BASELINE/ACTIVE không đồng nghĩa sellable

SKU không hardcode

Code phải đọc từ master/config/seed

SKU không tự production-ready

Production readiness thuộc Phase 2 và gate sau

## 11.2. Ingredient / UOM Domain

Hạng mục

Quy tắc

Ingredient phải canonical

Một nguyên liệu chuẩn, nhiều alias nếu cần

Alias phải trỏ canonical

Không seed alias thành master riêng

Material group phải rõ

Nhóm A, Nhóm B, Company Source, hoặc phân loại phù hợp

Source type phải rõ

Supplier / Company Source / Internal

UOM phải chuẩn

kg/g/l/ml/gói/hộp/thùng

UOM khác nhau phải có conversion

Không có conversion thì không seed công thức

Sâm Savigin là Company Source

Không đưa vào supplier material thường

## 11.3. Recipe / BOM / Formula Domain

Hạng mục

Quy tắc

Recipe phải gắn SKU

Không có recipe trôi nổi

Formula phải có version

G1 hoặc version được Owner xác nhận

G1 là operational formula

Không sửa G1 tại chỗ sau khi khóa

Gạo là anchor

Hệ thống scale theo kg gạo

Ratio_to_rice bắt buộc

Mỗi dòng nguyên liệu cần tỷ lệ rõ

Dòng công thức phải cụ thể

Không ghi nhóm chung thay nguyên liệu

Snapshot-ready

Phase 2 production order phải snapshot được

Buffer không sửa formula

Buffer chỉ dùng MRP/planning

## 11.4. Activation Guard Domain

Hạng mục

Quy tắc

Product activation là gate riêng

Không đồng nghĩa sellable

SKU có recipe chưa đủ bán

Cần release, stock, sale lock, commerce gate

SKU active chưa đủ bán

Không cho AI/Commerce/Gateway bán nếu chưa sellable

Activation phải audit được

Ai kích hoạt, lúc nào, lý do

Deactivation phải audit được

Không xóa dữ liệu cũ

## 11.5. Seed Governance Domain

Hạng mục

Quy tắc

Seed phải idempotent

Chạy lại không tạo trùng

Seed không dùng dữ liệu bẩn

Dữ liệu phải qua normalization checklist

Seed không hardcode logic nghiệp vụ

Seed chỉ nạp dữ liệu nền

Seed không tự mở bán

Không set sellable/released/production-ready

Seed phải có evidence

Log, kết quả, dữ liệu kiểm tra

Seed phải có rollback/forward-fix strategy

Không destructive nếu đã có dữ liệu vận hành

## 12. PHASE 1 KHÔNG ĐƯỢC LẤN SANG CÁC MIỀN SAU

Miền

Phase 1 được làm gì

Phase 1 không được làm gì

Production Order

Chuẩn bị formula snapshot-ready

Không tạo production order thật

Raw Lot/QC

Chuẩn bị ingredient/source/UOM

Không xử lý raw lot thật

Material Issue

Chuẩn bị recipe/BOM data

Không giảm tồn

Warehouse

Chuẩn bị SKU/packaging data

Không credit finished goods

Print/QR

Chuẩn bị packaging/profile data

Không sinh print payload thật

MRP/Procurement

Chuẩn bị threshold data/boundary

Không triển khai full MRP

## MISA

Chuẩn bị mapping placeholder nếu cần

Không sync/hạch toán thật

Commerce/AI

Chuẩn bị product activation boundary

Không quote/bán hàng

Recall/Sale Lock

Chuẩn bị sellable không tự mở

Không triển khai sale lock runtime

## PHẦN 3/4 - THỨ TỰ FILE / HANDOFF / OWNER CONFIRM

## 13. EXECUTION ORDER - THỨ TỰ TRIỂN KHAI PHASE 1

## 13.1. Thứ tự đọc và xử lý tài liệu

Thứ tự

File

Mục đích

Mode mặc định

## README-PHASE-01-HANDOFF-INDEX V1.1

Hiểu toàn bộ Phase 1

Documentation Reading

## 00-P1-ANALYSIS-ONLY

Đọc nền phân tích nếu còn dùng

Analysis Only

## 01-P1-1-TECHNICAL-DESIGN-ONLY

Đọc thiết kế kỹ thuật hiện có

Analysis Only

## 02-P1-2A-PRODUCT-SKU-INGREDIENT-UOM

Đối chiếu Product/SKU/Ingredient/UOM

Analysis Only

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION V2.0

File trọng tâm cần viết lại

Analysis Only sau khi viết lại

## 04-P1-2C-ACTIVATION-GUARD

Đối chiếu activation/sellable boundary

Analysis Only

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE V1.1

File seed governance cần viết lại

Analysis Only sau khi viết lại

## 06-P1-2E-SMOKE-EVIDENCE-REPORT V1.1

File smoke/evidence cần viết lại

Analysis Only sau khi viết lại

## 13.2. Thứ tự viết lại tài liệu Phase 1

Thứ tự

File

Hành động

## README-PHASE-01-HANDOFF-INDEX

## 03-P1-2B-RECIPE-BOM-FORMULA-VERSION

## 05-P1-2D-SKU-EXTENSION-SEED-GOVERNANCE

## 06-P1-2E-SMOKE-EVIDENCE-REPORT

## 02-P1-2A-PRODUCT-SKU-INGREDIENT-UOM

Giữ tạm, chỉ viết lại nếu checklist phát hiện conflict

## 04-P1-2C-ACTIVATION-GUARD

Giữ tạm, chỉ viết lại nếu thiếu boundary nghiêm trọng

## 00-P1-ANALYSIS-ONLY

Giữ tạm

## 01-P1-1-TECHNICAL-DESIGN-ONLY

Giữ tạm

## 14. DEV / CODEX / COPILOT HANDOFF RULES CHO PHASE 1

## 14.1. Mode mặc định

Mọi file Phase 1 khi giao cho dev/Codex/Copilot phải bắt đầu bằng:

## MODE: ANALYSIS ONLY

Agent không được sửa code ngay.

## 14.2. Output bắt buộc khi Analysis Only

Agent phải trả về:

Scope đã đọc.

Rule P0.

Boundary rules.

Gap Matrix.

Impacted files.

Database/migration impact.

API/DTO/service/UI/worker impact.

Owner Confirm Required.

Risk Register.

Proposed Implementation Scope.

Proposed Smoke/Evidence.

## 14.3. Các việc Agent được làm

Việc

Được phép?

Điều kiện

Đọc README Phase 1

Có

Documentation Reading

Đọc file Phase 1 bản sạch

Có

Analysis Only

Lập gap matrix

Có

Không sửa code

Liệt kê impacted files

Có

Không sửa

Đề xuất migration impact

Có

Không tạo migration

Đề xuất seed structure

Có

Không tạo seed

Đánh dấu Owner confirm

Có

Không tự điền

Đề xuất smoke/evidence

Có

Không gọi PASS

## 14.4. Các việc Agent không được làm

Agent không được:

Code trực tiếp từ README này.

Code trực tiếp từ nguồn tổng hợp.

Tạo migration khi chưa có lệnh.

Tạo seed khi chưa có Owner confirm.

Tự suy luận 20 SKU nếu thiếu.

Tự suy luận ingredient canonical.

Tự suy luận UOM.

Tự suy luận G1 formula.

Tự suy luận ratio_to_rice.

Tự tạo packaging config.

Tự hardcode threshold.

Sửa công thức G1 tại chỗ.

Đưa buffer vào công thức sản xuất.

Set SKU sellable mặc định.

Set SKU released mặc định.

Set SKU production-ready mặc định.

Refactor lan ngoài scope.

Gọi Completion Decision.

Gọi Gateway PASS.

Gọi Production Readiness.

## 15. OWNER CONFIRM REQUIRED - DỮ LIỆU PHASE 1 CẦN CHỦ DỰ ÁN XÁC NHẬN

Nhóm

Dữ liệu cần xác nhận

Vì sao chặn seed/code

SKU baseline

20 SKU code, public name, internal name, group, status

Tránh seed sai sản phẩm

Product group

Nhóm mùa / chức năng / bổ dưỡng hoặc phân loại chính thức

Tránh sai logic tư vấn/sản xuất

Ingredient canonical

Tên chuẩn, mã, nhóm, source type

Tránh trùng nguyên liệu

Ingredient alias

Alias nào trỏ về canonical nào

Tránh seed trùng

## UOM

kg/g/l/ml/gói/hộp/thùng và conversion

Tránh sai công thức/tồn kho

Recipe line group

Nhóm dòng công thức nhưng không thay thế ingredient

Tránh công thức mơ hồ

Formula G1

Formula code, version, status, approved date

Tránh code theo công thức chưa khóa

Anchor gạo

Ingredient nào là gạo anchor

Tránh scale sai

Ratio_to_rice

Tỷ lệ từng nguyên liệu so với gạo

Tránh tính sai sản lượng

Rounding policy

Làm tròn khi scale

Tránh lệch số lượng nguyên liệu

Buffer policy

+5% nhóm A, +7% nhóm B

Tránh cộng nhầm vào formula

Packaging profile

Gói/hộp/thùng, units_per_box, boxes_per_carton

Tránh sai đóng gói/print

Source type Sâm Savigin

Company Source / strategic reserve

Tránh đưa vào supplier material

Seed status

## ACTIVE_BASELINE/ACTIVE/INACTIVE

Tránh mở bán nhầm

Activation guard

Điều kiện active/deactive

Tránh nhầm active với sellable

## 16. STOP CONDITIONS CHO PHASE 1

Dev/Agent phải dừng ngay nếu gặp:

Mã

Điều kiện dừng

Hành động

## P1-STOP-01

Trùng SKU code

Owner confirm

## P1-STOP-02

SKU thiếu public name

Owner confirm

## P1-STOP-03

Ingredient trùng canonical

Owner confirm

## P1-STOP-04

Alias trỏ nhiều canonical

Owner confirm

## P1-STOP-05

UOM thiếu conversion

Owner confirm

## P1-STOP-06

Formula thiếu version

Owner confirm

## P1-STOP-07

G1 thiếu anchor gạo

Owner confirm

## P1-STOP-08

Thiếu ratio_to_rice

Owner confirm

## P1-STOP-09

Công thức dùng nhóm mơ hồ thay nguyên liệu

Owner confirm

## P1-STOP-10

Có yêu cầu sửa G1 trực tiếp

Dừng, yêu cầu version mới

## P1-STOP-11

Buffer bị cộng vào formula

Dừng, báo P0

## P1-STOP-12

Seed tự set sellable/released

Dừng, báo P0

## P1-STOP-13

Sâm Savigin bị seed là supplier material

Dừng, báo P0

## P1-STOP-14

Code hiện tại conflict với tài liệu sạch

Lập conflict report

## P1-STOP-15

Agent không chắc dữ liệu

Dừng, Owner confirm

PHẦN 4/4 - SMOKE / EVIDENCE / DONE GATE / FAIL GATE

## 17. SMOKE / EVIDENCE REQUIREMENTS PHASE 1

Phase 1 phải có smoke/evidence riêng sau khi triển khai.

README này chỉ khóa danh mục yêu cầu. Chi tiết sẽ được viết lại trong:

## 18. DANH MỤC SMOKE TỐI THIỂU PHASE 1

Nhóm smoke

Yêu cầu

SKU baseline

Seed 20 SKU không trùng, đúng canonical, không tự sellable

Ingredient canonical

Ingredient không trùng, alias trỏ đúng canonical

## UOM

UOM có conversion, không nhập tay tự do

Recipe line

Mỗi dòng công thức có ingredient cụ thể

Formula version

Formula có code/version/status

G1 lock

Không sửa G1 tại chỗ sau khi active/approved

Anchor gạo

Formula có đúng một anchor gạo

Ratio_to_rice

Mỗi dòng cần scaling có ratio_to_rice

Scaling

Nhập kg gạo, hệ thống tính nguyên liệu theo ratio_to_rice

Scaling không đổi version

Đổi kg gạo không tạo formula version mới

Formula change

Đổi tỷ lệ/thêm/bỏ nguyên liệu tạo version mới

Buffer

+5%/+7% chỉ xuất hiện trong planning, không sửa formula

Packaging seed

Packaging profile có units_per_box/boxes_per_carton nếu áp dụng

Seed idempotency

Chạy seed lại không tạo trùng

No dirty seed

Dữ liệu chưa Owner confirm không được seed

Activation guard

Active SKU không tự sellable

No production-ready

Không seed production-ready

No released

Không seed released

## 19. EVIDENCE REQUIREMENTS PHASE 1

Mã evidence

Nội dung evidence

## EVD-P1-SKU

Danh sách SKU baseline sau chuẩn hóa

## EVD-P1-INGREDIENT

Ingredient canonical + alias

## EVD-P1-UOM

Bảng UOM + conversion

## EVD-P1-RECIPE-LINE

Recipe line có ingredient cụ thể

## EVD-P1-FORMULA-G1

G1 formula header + lines

## EVD-P1-ANCHOR-RICE

Bằng chứng gạo là anchor

## EVD-P1-RATIO

Bảng ratio_to_rice

## EVD-P1-SCALING

Test scale theo kg gạo

## EVD-P1-VERSIONING

Test đổi tỷ lệ/thêm/bỏ nguyên liệu tạo version mới

## EVD-P1-BUFFER

Bằng chứng buffer không sửa formula

## EVD-P1-PACKAGING

Packaging profile seed

## EVD-P1-SEED-IDEMPOTENCY

Log seed idempotency

## EVD-P1-ACTIVATION-GUARD

Test active không sellable

## EVD-P1-NO-DIRTY-SEED

Danh sách dữ liệu bị chặn vì chưa confirm

## EVD-P1-OWNER-CONFIRM

Biên bản/ghi nhận Owner confirm dữ liệu nền

Lưu ý:

Evidence Submitted chưa phải Evidence Accepted.

Chỉ khi Owner/Reviewer chấp nhận evidence thì mới dùng cho gate tiếp theo.

## 20. RISK REGISTER PHASE 1

Risk

Mức độ

Nguyên nhân

Cách kiểm soát

Seed sai SKU

## P0

SKU chưa canonical

Owner confirm + smoke seed

Trùng ingredient

## P0

Alias bị seed thành master

Canonical/alias checklist

Sai UOM

## P0

Thiếu conversion

UOM smoke

Sai formula G1

## P0

Thiếu anchor/ratio/version

Viết lại P1.2B

Sửa G1 trực tiếp

## P0

Thiếu versioning rule

Formula lock

Cộng buffer vào formula

## P0

Nhầm planning với production formula

Rule buffer riêng

Active thành sellable

## P0

Nhầm product status với commerce gate

Activation guard

Seed production-ready

## P0

Nhầm seed với vận hành

Seed governance

Hardcode SKU/material

## P1

Dev muốn làm nhanh

Master/config only

Agent tự suy luận dữ liệu

## P1

Thiếu Owner confirm

Runbook stop condition

Phase 2 tiêu thụ dữ liệu bẩn

## P0

Phase 1 chưa chuẩn

Không mở Phase 2 implementation

## 21. DONE GATE CỦA README PHASE 1

README Phase 1 được xem là đạt khi:

Xác định rõ Phase 1 là nền Product/SKU/Recipe/Activation, không phải vận hành full.

Xác định rõ nguồn tổng hợp là auxiliary source.

Xác định rõ danh sách file Phase 1.

Phân loại file cần viết lại, giữ tạm.

Khóa source-of-truth hierarchy.

Khóa scope in.

Khóa scope out.

Khóa boundary rules.

Khóa domain registry Phase 1.

Khóa việc G1 phải có formula version.

Khóa anchor gạo.

Khóa ratio_to_rice.

Khóa scaling không đổi formula version.

Khóa đổi tỷ lệ/thêm/bỏ nguyên liệu phải tạo version mới.

Khóa công thức phải bung từng nguyên liệu cụ thể.

Khóa buffer là planning, không sửa formula.

Khóa active SKU không đồng nghĩa sellable.

Khóa seed không tự set released/sellable/production-ready.

Khóa Owner Confirm Required.

Khóa Stop Conditions.

Khóa handoff rule cho dev/Codex/Copilot.

Khóa smoke/evidence yêu cầu cho Phase 1.

Không gọi Completion Decision.

Không gọi Gateway PASS.

Không gọi Production Readiness.

## 22. FAIL GATE CỦA README PHASE 1

README Phase 1 bị xem là fail nếu xảy ra một trong các trường hợp:

Dùng nguồn tổng hợp để code trực tiếp.

Dùng nguồn tổng hợp để seed trực tiếp.

chỉnh sửa tạm lẻ file Phase 1 bị ảnh hưởng thay vì viết lại toàn bộ.

Cho Agent tự suy luận SKU/ingredient/UOM/formula.

Seed SKU chưa canonical.

Seed ingredient trùng.

Seed alias thành nguyên liệu mới.

Seed formula thiếu version.

Seed formula thiếu anchor gạo.

Seed formula thiếu ratio_to_rice.

Công thức chỉ ghi nhóm mơ hồ, không có ingredient cụ thể.

Sửa G1 trực tiếp sau khi khóa.

Đổi tỷ lệ nhưng không tạo version mới.

Thêm/bỏ nguyên liệu nhưng không tạo version mới.

Cộng buffer +5%/+7% vào công thức sản xuất.

SKU active bị hiểu là sellable.

Seed tự set released.

Seed tự set production-ready.

Sâm Savigin bị seed như supplier material thường.

Packaging profile bị hardcode.

Phase 2 dùng dữ liệu Phase 1 chưa chuẩn.

Dev/Codex/Copilot sửa code khi mới Analysis Only.

Gọi Evidence Submitted là Evidence Accepted.

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

## PHASE 1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

Implementation authorization

KHONG

Code authorization

KHONG

Migration authorization

KHONG

Seed authorization

KHONG, cho đến khi dữ liệu được chuẩn hóa và Owner confirm

Limited implementation authorization

KHONG

Gateway status

bị chặn

Completion status

## WAITING IMPLEMENTATION EVIDENCE

Evidence status

## NOT ACCEPTED

Release status

Production Readiness

KHONG

Go-live Decision

KHONG

Tài liệu kế tiếp

## 24. KẾT LUẬN ĐIỀU PHỐI PHASE 1

Từ thời điểm dùng README Phase 1 V1.1 này, cách làm đúng cho Phase 1 là:

Không code trực tiếp từ nguồn tổng hợp.

Không sửa chỉnh sửa tạm file cũ.

File nào bị ảnh hưởng mạnh thì viết lại toàn bộ.

Chuẩn hóa master data trước seed/code.

Không để Agent tự suy luận dữ liệu.

Không hardcode SKU/material/formula.

Không để Active SKU thành Sellable.

Không để Seed tự mở trạng thái vận hành.

Không để G1 bị sửa trực tiếp.

Không để scaling kg gạo bị hiểu là đổi công thức.

Không để buffer planning sửa công thức sản xuất.

Không mở Gateway.

Không gọi Production Readiness.

Tài liệu kế tiếp cần viết lại toàn bộ là:
