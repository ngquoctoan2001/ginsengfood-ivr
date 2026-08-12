# TECH-03 - OPERATIONAL CORE PRODUCTION WAREHOUSE INVENTORY TRACEABILITY RECALL SALE LOCK

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## PHẦN 1/4 - OPERATIONAL CORE PRINCIPLES / SOURCE-OF-TRUTH / PRODUCTION-WAREHOUSE-TRACE BOUNDARY / NO-BYPASS RULE

## 1. MỤC TIÊU CỦA TECH-03

## 1.1. Vai trò của TECH-03

TECH-03 là tài liệu kỹ thuật nền cho tầng Operational Core của Ginsengfood.

Operational Core là lớp chịu trách nhiệm quản trị toàn bộ chuỗi vận hành thật từ:

Nguồn nguyên liệu -> nhập nguyên liệu -> QC -> sẵn sàng sản xuất -> lệnh sản xuất -> xuất nguyên liệu -> nhận nguyên liệu tại xưởng -> sản xuất -> đóng gói -> QC thành phẩm -> release batch -> nhập kho thành phẩm -> tồn kho -> traceability -> public trace -> recall -> stop-sale / sale lock.

TECH-03 không phải tài liệu thương mại, không phải tài liệu bán hàng, không phải tài liệu AI tư vấn, không phải tài liệu quảng cáo và không phải tài liệu kế toán chi tiết.

TECH-03 là lớp khóa kỹ thuật để đảm bảo rằng:

Không có sản phẩm nào được mở bán nếu chưa qua đúng chuỗi vận hành.

Không có batch nào được bán nếu chưa được release.

Không có tồn kho nào được tăng hoặc giảm tùy tiện.

Không có public trace nào được hiển thị nếu thiếu trace chain.

Không có Commerce, AI, Ads, Live, CRM nào được vượt qua sale lock, recall hoặc trạng thái sellable của Operational Core.

## 1.2. TECH-03 nằm ở đâu trong chuỗi TECH

TECH-03 nằm sau:

## TECH-00 - Technical Implementation Master Plan.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-02 - Product / SKU / Ingredient / Recipe / Formula / Product Activation.

TECH-03 nằm trước:

## TECH-04 - Commerce Runtime / Quote / Cart / Order / Payment / Shipping.

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway / Meta Live & Messenger.

## TECH-07 - Ads Measurement.

## TECH-08 - MC AI Live.

## TECH-09 - IVR.

## TECH-10 - Completion / Evidence / Gateway / Release Readiness hoặc release handoff tương ứng.

Nguyên tắc bắt buộc:

TECH-04 Commerce không được triển khai production nếu TECH-03 Operational Core chưa clear các gate bắt buộc liên quan đến sellable, warehouse, inventory, trace và sale lock.

## 2. PHẠM VI CỦA TECH-03

## 2.1. TECH-03 bao gồm

TECH-03 khóa các domain vận hành chính sau:

Source Origin.

Raw Material Intake.

Incoming QC.

Raw Lot Readiness.

Production Order.

Formula Snapshot trong Production Order.

Material Request.

Material Issue.

Material Receipt.

Production Process Execution.

Packaging / Printing / QR.

Batch QC.

Batch Release.

Warehouse Receipt.

Inventory Ledger.

Lot Balance.

Traceability.

Public Trace.

Recall.

Recovery / CAPA nếu scope có.

Sale Lock / Stop Sale.

Operational handoff sang Commerce, AI, Ads, Live, Public Trace, MISA nếu scope có.

## 2.2. TECH-03 không bao gồm

TECH-03 không thiết kế chi tiết:

API endpoint.

Database schema.

UI screen.

Component frontend.

Code backend.

Code worker.

Code integration.

Chi tiết quote/order/payment của Commerce.

Chi tiết nội dung tư vấn của AI Advisor.

Chi tiết campaign Ads.

Chi tiết live script.

Chi tiết IVR call flow.

Các phần trên chỉ được phép triển khai ở TECH sau hoặc DEV phase tương ứng, sau khi TECH-03 khóa đúng boundary.

## 3. NGUYÊN TẮC GREENFIELD CHO OPERATIONAL CORE

## 3.1. Tài liệu mới là nguồn đúng

Operational Core được xây theo hướng GREENFIELD TECHNICAL RESET.

Điều này có nghĩa:

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

TECH-00, TECH-01, TECH-02, TECH-03 và các TECH docs mới là SOURCE_OF_TRUTH.

Nếu code cũ khác tài liệu mới thì tài liệu mới thắng.

Dev không được lấy logic từ code cũ hoặc tài liệu cũ để ghi đè TECH-03.

Chỉ được dùng legacy để đối chiếu gap sau khi TECH-03 đã khóa.

## 3.2. Không code trước khi khóa mapping

Operational Core là tầng có nhiều rủi ro nhất vì liên quan đến:

Tồn kho nguyên liệu.

Tồn kho thành phẩm.

Lot.

Batch.

## QC.

Trace.

Recall.

Stop-sale.

Handoff sang bán hàng.

Vì vậy không được đi thẳng vào code khi chưa có:

Module map.

Dependency map.

State model.

P0 điểm chặn.

Evidence requirement.

Smoke matrix.

Owner review.

Release handoff.

Việc viết code rời rạc cho Operational Core sẽ tạo lỗi dây chuyền. Ví dụ chỉ viết “trừ kho nguyên liệu” mà không khóa material issue, material receipt, ledger append-only và lot readiness thì rất dễ dẫn đến trừ kho hai lần hoặc xuất nhầm nguyên liệu chưa đủ điều kiện sản xuất.

## 4. SOURCE-OF-TRUTH CỦA OPERATIONAL CORE

## 4.1. Source-of-truth cấp tài liệu

Operational Core phải tuân theo thứ tự nguồn đúng như sau:

Cấp

Nguồn

Vai trò

MASTER / PACK đã khóa

Business governance, release governance, source-of-truth cấp cao

## TECH-00

Kế hoạch triển khai kỹ thuật tổng thể

## TECH-01

Foundation, RBAC, audit, idempotency, evidence

## TECH-02

Product, SKU, ingredient, recipe, formula, activation

## TECH-03

Operational Core source-of-truth

TECH-04 trở đi

Downstream consumer của Operational Core

Code hiện tại

Chỉ là trạng thái hiện hữu để gap analysis

Tài liệu cũ

Chỉ là legacy reference

## 4.2. Source-of-truth cấp runtime

Trong runtime vận hành, mỗi loại dữ liệu phải có owner rõ ràng.

Loại dữ liệu

Owner đúng

Không được lấy từ

Product / SKU / Ingredient / Recipe

Product Domain - TECH-02

AI content, Commerce, Ads, file rời

Formula Snapshot cho sản xuất

Production Order trong Operational Core

Recipe live lookup sau khi PO đã tạo

Raw lot readiness

Operational Core

Commerce, AI, manual flag ngoài quy trình

Batch release

Operational Core

QC_PASS đơn lẻ, warehouse, Commerce

Finished goods stock

Warehouse Receipt + Inventory Ledger

Quote/order/payment

Sellable gate

Operational Core + Commerce Runtime sau handoff

Product active flag đơn lẻ

Stop-sale / recall lock

Operational Core

Campaign, Ads, AI, CRM

Public trace

Trace/Public Trace boundary

Nội bộ QC, supplier evidence, costing, MISA

Quote/order/payment

Commerce Runtime

Operational Core

## 5. VAI TRÒ CỦA OPERATIONAL CORE

## 5.1. Operational Core là lõi vận hành thật

Operational Core không phải nơi chỉ lưu danh sách batch hoặc kho.

Operational Core là nơi xác nhận thực tế:

Nguyên liệu có nguồn gốc hợp lệ hay chưa.

Nguyên liệu đã được nhập đúng chưa.

Nguyên liệu đã qua QC chưa.

Raw lot đã thật sự sẵn sàng sản xuất chưa.

Lệnh sản xuất dùng công thức nào.

Nguyên liệu nào đã được xuất cho sản xuất.

Xưởng đã nhận nguyên liệu nào.

Batch đã hoàn thành quy trình nào.

Thành phẩm đã qua QC chưa.

Batch đã được release chưa.

Kho đã nhận thành phẩm chưa.

Tồn kho có thật để bán chưa.

Trace chain có đầy đủ chưa.

Có recall hoặc stop-sale không.

## 5.2. Operational Core không thay Commerce

Operational Core không được tự quyết định:

Giá bán.

Chương trình giá.

Chiết khấu thành viên.

Quote.

Cart.

Order.

Thanh toán.

Phí ship.

## COD.

VAT hiển thị cho khách.

Chính sách giữ giá.

Các phần đó thuộc TECH-04 Commerce Runtime.

Operational Core chỉ cung cấp trạng thái vận hành thật để Commerce quyết định có được bán hay không.

Ví dụ:

Batch đã RELEASED.

Thành phẩm đã nhập kho.

Tồn kho khả dụng > 0.

Không bị recall.

Không bị sale lock.

Trace chain hợp lệ nếu public trace được bật.

SKU nằm trong release scope.

Commerce được quyền quote/order chỉ khi các điều kiện Operational hợp lệ.

## 5.3. Operational Core không thay AI Advisor

Operational Core không viết câu tư vấn cho khách.

AI Advisor không được tự tạo trạng thái vận hành.

Ranh giới đúng:

Operational Core cung cấp trạng thái sellable, trace, recall, batch/lot public-safe.

AI Advisor chỉ được dùng dữ liệu đã được resolver/runtime cung cấp.

Nếu Operational Runtime unavailable, AI không được bịa rằng sản phẩm còn hàng, đã release, có trace hoặc có thể bán.

Nếu sản phẩm bị stop-sale hoặc recall, AI phải dừng tư vấn bán/chốt đơn và đi theo template an toàn đã được khóa ở AI Advisor.

## 5.4. Operational Core không thay Ads / Live / CRM

Ads, Live, CRM có thể tạo nhu cầu bán hàng, nhưng không được quyết định hàng có được bán hay không.

Nguyên tắc:

Ads không được chạy sản phẩm bị stop-sale.

Live không được chốt sản phẩm không sellable.

CRM không được upsell sản phẩm bị recall hoặc không còn sellable.

AI Live không được giới thiệu sản phẩm nếu Operational Core báo bị chặn.

Facebook Gateway không được route quote/order vượt qua sale lock.

## 6. CHUỖI VẬN HÀNH CHUẨN CỦA OPERATIONAL CORE

## 6.1. Chuỗi tổng thể bắt buộc

Chuỗi Operational Core chuẩn:

Source Origin -> Raw Material Intake -> Incoming QC -> Raw Lot Readiness -> Production Order -> Formula Snapshot -> Material Request -> Material Issue -> Material Receipt -> Production Process Execution -> Packaging / Printing / QR -> Batch QC -> Batch Release -> Warehouse Receipt -> Inventory Ledger / Lot Balance -> Traceability -> Public Trace -> Recall / Recovery / CAPA -> Sale Lock / Stop Sale Handoff -> Commerce / AI / Ads / Live.

Không module downstream nào được bỏ qua chuỗi này để mở bán.

## 6.2. Nguyên tắc “không nhảy cóc trạng thái”

Mỗi trạng thái vận hành phải đi qua đúng bước.

Không được:

QC_PASS raw lot rồi tự động cho sản xuất.

Batch QC_PASS rồi tự động nhập kho.

Product Active rồi tự động sellable.

SKU Active rồi tự động mở bán.

Warehouse có batch chưa release.

Public trace khi trace chain thiếu.

Quote/order khi sale lock đang bật.

Ads/Live/CRM bán sản phẩm bị recall.

## 6.3. Chuỗi kiểm soát nguyên liệu

Nguyên liệu phải đi qua:

Xác định nguồn.

Nhập nguyên liệu.

Ghi nhận lot.

QC đầu vào.

Nếu QC đạt thì vẫn chưa được sản xuất ngay.

Phải có hành động mark ready hoặc readiness approval.

Chỉ raw lot ở trạng thái READY_FOR_PRODUCTION mới được dùng cho Material Issue.

Rule khóa:

RAW_LOT QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

## 6.4. Chuỗi kiểm soát sản xuất

Lệnh sản xuất phải có:

SKU hợp lệ từ TECH-02.

Recipe/formula version hợp lệ.

Formula snapshot.

Ingredient snapshot.

Quantity snapshot.

UOM snapshot.

Formula kind snapshot.

Production scope.

Approval nếu command có rủi ro.

Rule khóa:

Production Order phải dùng formula snapshot, không được phụ thuộc live recipe để diễn giải lịch sử sản xuất.

## 6.5. Chuỗi kiểm soát xuất nguyên liệu

Material Issue là checkpoint chính làm giảm tồn nguyên liệu.

Rule khóa:

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt tại xưởng chỉ xác nhận xưởng đã nhận, ghi nhận chênh lệch nếu có, không được trừ tồn thêm lần nữa.

Rule khóa:

Material Receipt không được giảm tồn lần hai.

## 6.6. Chuỗi kiểm soát batch

Batch phải đi qua:

Tạo từ Production Order hợp lệ.

Ghi nhận quá trình sản xuất.

Đóng gói nếu scope áp dụng.

QC thành phẩm.

Nếu QC_PASS thì vẫn chưa RELEASED.

Phải có hành động Batch Release riêng.

Chỉ batch RELEASED mới được kho nhận thành phẩm.

Rule khóa:

Batch QC_PASS không đồng nghĩa Batch RELEASED.

## 6.7. Chuỗi kiểm soát kho thành phẩm

Warehouse chỉ nhận thành phẩm từ batch RELEASED.

Rule khóa:

Warehouse chỉ nhận batch RELEASED.

Tồn kho thành phẩm chỉ tăng khi warehouse receipt confirmed.

Rule khóa:

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

## 6.8. Chuỗi kiểm soát bán hàng

Sản phẩm chỉ được xem xét bán khi có đủ:

Product/SKU active theo TECH-02.

Formula/version hợp lệ nếu liên quan sản xuất.

Batch RELEASED.

Warehouse receipt confirmed.

Finished goods stock khả dụng.

Không recall.

Không sale lock.

Không quality hold.

Trace chain hợp lệ nếu public trace được bật.

Commerce Runtime xác nhận đủ điều kiện quote/order.

Rule khóa:

Product Active hoặc SKU Active không đồng nghĩa Sellable.

## 7. RANH GIỚI GIỮA PRODUCT - OPERATIONAL - COMMERCE

## 7.1. Product Domain cung cấp gì cho Operational Core

Product Domain từ TECH-02 cung cấp:

Product.

## SKU.

Ingredient master.

Recipe.

Formula kind.

Formula version.

Recipe line group.

Trade item nếu scope có.

Product knowledge public/internal nếu downstream cần.

Operational Core không được tự tạo SKU, tự sửa recipe hoặc tự thay formula version ngoài Product governance.

## 7.2. Operational Core tạo gì từ Product Domain

Operational Core tạo dữ liệu vận hành thật:

Raw material lot.

Raw lot QC result.

Raw lot readiness.

Production Order.

Formula Snapshot.

Material Request.

Material Issue.

Material Receipt.

Batch.

Batch QC.

Batch Release.

Warehouse Receipt.

Inventory Ledger.

Lot Balance projection.

Trace chain.

Recall case.

Stop-sale state.

Operational sellable signal.

## 7.3. Commerce sử dụng gì từ Operational Core

Commerce Runtime chỉ được dùng sản phẩm để quote/order khi nhận được tín hiệu hợp lệ từ Operational Core:

SKU nằm trong scope bán.

Batch/finished goods có tồn khả dụng.

Không bị sale lock.

Không bị recall.

Không quality hold.

Trace/public trace policy không vi phạm nếu cần.

Operational sellable signal pass.

Commerce không được tự suy luận sellable chỉ từ Product Active.

## 8. NO-BYPASS RULE

## 8.1. Định nghĩa no-bypass

No-bypass nghĩa là không module nào được đi vòng qua owner domain.

Trong Operational Core, no-bypass áp dụng cho toàn bộ hành động rủi ro:

Không bypass raw lot readiness.

Không bypass material issue.

Không bypass batch release.

Không bypass warehouse receipt.

Không bypass inventory ledger.

Không bypass recall.

Không bypass sale lock.

Không bypass public trace whitelist.

Không bypass permission/audit/evidence.

## 8.2. No-bypass với Foundation

TECH-03 bắt buộc dựa trên TECH-01.

Mọi command rủi ro trong Operational Core phải có:

Permission theo action.

Backend enforcement.

Audit.

Idempotency nếu có side-effect.

Evidence nếu liên quan release/pass/gate.

Owner review nếu command ảnh hưởng production.

Không được chỉ dựa vào UI để khóa quyền.

Không được hardcode role để bỏ qua permission.

Không được cho admin toàn quyền sửa mọi trạng thái mà không có audit/reason.

## 8.3. No-bypass với Product

Operational Core không được:

Tự tạo SKU production ngoài Product Domain.

Tự sửa công thức để sản xuất.

Tự thay formula version trong lệnh đã tạo.

Tự đổi group nguyên liệu.

Tự dùng ingredient không thuộc recipe snapshot.

Tự đưa product chưa approved vào production scope.

Nếu Product Runtime unavailable hoặc Product Domain chưa clear, Operational production phải bị block ở các bước phụ thuộc.

## 8.4. No-bypass với Commerce

Commerce không được:

Quote sản phẩm chưa sellable.

Tạo order cho batch chưa released.

Tạo order khi không có tồn kho khả dụng.

Tạo order khi có sale lock.

Tạo order khi recall đang active.

Bán sản phẩm chỉ vì Product Active.

Bán sản phẩm dựa vào nội dung AI/Ads/Live nếu Operational Core báo bị chặn.

## 8.5. No-bypass với AI / Ads / Live / CRM

AI, Ads, Live, CRM không được:

Công bố sản phẩm còn hàng nếu Operational Runtime chưa xác nhận.

Đẩy sản phẩm bị stop-sale.

Gợi ý mua sản phẩm bị recall.

Nói public trace hợp lệ khi QR VOID/FAILED.

Hiển thị thông tin trace không nằm trong whitelist.

Dùng dữ liệu nội bộ để tư vấn công khai.

Ghi đè trạng thái sellable.

## 9. P0 OPERATIONAL RULES NỀN

## 9.1. P0-OP-001 - RAW LOT QC_PASS không đồng nghĩa READY_FOR_PRODUCTION

Raw lot sau khi QC_PASS vẫn chưa được issue cho sản xuất.

Raw lot chỉ được issue khi có trạng thái:

## READY_FOR_PRODUCTION

Điều kiện readiness phải có:

QC đạt.

Không hold.

Không reject.

Không recall.

Nguồn hợp lệ.

Lot balance đủ.

Evidence/record theo policy.

Fail Gate:

Nếu raw lot chưa READY_FOR_PRODUCTION mà vẫn được issue, Operational Core FAIL.

## 9.2. P0-OP-002 - Material Issue là điểm giảm tồn nguyên liệu chính

Tồn nguyên liệu chỉ được giảm tại Material Issue.

Material Issue phải:

Chọn đúng raw lot.

Kiểm tra raw lot readiness.

Kiểm tra balance.

Ghi ledger debit.

Có idempotency.

Có audit.

Không tạo debit trùng khi retry.

Fail Gate:

Nếu retry làm trừ tồn hai lần, Operational Core FAIL.

## 9.3. P0-OP-003 - Material Receipt không được giảm tồn lần hai

Material Receipt chỉ ghi nhận xưởng đã nhận nguyên liệu và chênh lệch nếu có.

Không được giảm tồn nguyên liệu lần hai tại Material Receipt.

Fail Gate:

Nếu Material Issue đã trừ tồn, sau đó Material Receipt tiếp tục trừ tồn, Operational Core FAIL.

## 9.4. P0-OP-004 - Production Order phải snapshot formula

Production Order phải snapshot tối thiểu:

## SKU.

Formula kind.

Formula version.

Ingredient.

Quantity.

## UOM.

Group.

Usage basis nếu có.

Approved recipe reference.

Created time.

Actor/owner.

Production scope.

Không được phụ thuộc live recipe để tái dựng lịch sử batch.

Fail Gate:

Nếu recipe thay đổi làm lịch sử Production Order thay đổi theo, Operational Core FAIL.

## 9.5. P0-OP-005 - Batch QC_PASS không đồng nghĩa RELEASED

Batch QC_PASS chỉ là kết quả QC.

Batch RELEASED là hành động riêng, có quyền, audit, evidence và owner review nếu policy yêu cầu.

Fail Gate:

Nếu QC_PASS tự động cho batch nhập kho hoặc mở bán, Operational Core FAIL.

## 9.6. P0-OP-006 - Warehouse chỉ nhận batch RELEASED

Warehouse Receipt chỉ chấp nhận batch ở trạng thái RELEASED.

Không nhận:

Batch draft.

Batch đang sản xuất.

Batch QC_WAITING.

Batch QC_PASS nhưng chưa RELEASED.

Batch HOLD.

Batch REJECTED.

Batch RECALLED.

Fail Gate:

Nếu warehouse nhận batch chưa RELEASED, Operational Core FAIL.

## 9.7. P0-OP-007 - Finished goods chỉ tăng tồn khi warehouse receipt confirmed

Tồn thành phẩm không tăng khi:

Batch vừa tạo.

Batch hoàn thành sản xuất.

Batch QC_PASS.

Batch RELEASED nhưng chưa nhập kho.

Packaging done nhưng chưa warehouse confirmed.

Tồn thành phẩm chỉ tăng khi:

Warehouse Receipt Confirmed

Fail Gate:

Nếu finished goods stock tăng trước warehouse receipt confirmed, Operational Core FAIL.

## 9.8. P0-OP-008 - Inventory Ledger append-only

Inventory Ledger phải append-only.

Không được:

Sửa dòng ledger cũ.

Xóa dòng ledger cũ.

Ghi đè movement.

Điều chỉnh tồn bằng cách sửa trực tiếp balance.

Mọi điều chỉnh phải là movement mới có reason, audit và permission.

Fail Gate:

Nếu ledger có thể sửa/xóa để thay đổi lịch sử tồn, Operational Core FAIL.

## 9.9. P0-OP-009 - Sale Lock / Recall thắng mọi downstream

Sale Lock và Recall phải thắng:

Commerce.

AI Advisor.

Ads.

Live.

## CRM.

Facebook Gateway.

## MC AI.

IVR nếu liên quan xác nhận đơn.

Public-facing sales flows.

Fail Gate:

Nếu sản phẩm bị recall hoặc sale lock mà vẫn quote/order/sell được, Operational Core FAIL.

## 9.10. P0-OP-010 - Public Trace whitelist-only

Public Trace chỉ được hiển thị dữ liệu đã whitelist.

Không public:

Supplier evidence nội bộ.

Supplier internal ID.

Personnel.

Costing.

QC defect.

Loss.

Internal note.

MISA/private data.

Evidence nội bộ.

Recall internal investigation nếu không public-safe.

Tài liệu kiểm nghiệm nội bộ chưa được public policy cho phép.

Fail Gate:

Nếu public trace lộ dữ liệu nội bộ, Operational Core FAIL.

## 9.11. P0-OP-011 - QR VOID/FAILED không public-valid

QR ở trạng thái VOID hoặc FAILED không được public-valid.

Nếu QR lifecycle có:

## GENERATED.

## QUEUED.

## PRINTED.

## FAILED.

## VOID.

## REPRINTED.

Thì public trace chỉ được phản hồi hợp lệ theo policy với QR còn valid.

Fail Gate:

Nếu QR VOID/FAILED vẫn mở public trace như bình thường, Operational Core FAIL.

## 9.12. P0-OP-012 - Không có trace chain thì không public trace

Public Trace yêu cầu trace chain đầy đủ theo scope.

Nếu thiếu link giữa:

Product/SKU.

Batch.

Raw lot.

Production Order.

Warehouse Receipt.

QR/package.

Release state.

Thì public trace phải fail-safe hoặc trả trạng thái an toàn theo policy.

Fail Gate:

Nếu thiếu trace chain mà vẫn public trace như hợp lệ, Operational Core FAIL.

## 9.13. P0-OP-013 - Batch chưa RELEASED không sellable

Batch chưa RELEASED không được đưa vào sellable stock.

Product Active hoặc SKU Active không đủ để bán.

Fail Gate:

Nếu Commerce nhận được sellable stock từ batch chưa RELEASED, Operational Core FAIL.

## 9.14. P0-OP-014 - Recall / Sale Lock Smoke bắt buộc trước production sale

Trước khi mở bán production, phải có smoke test chứng minh:

Recall active chặn Commerce.

Recall active chặn AI.

Recall active chặn Ads/Live nếu scope có.

Sale Lock chặn quote/order.

Public Trace xử lý an toàn.

Inventory không bị bán vượt lock.

Owner review accepted.

Fail Gate:

Nếu không có recall/sale lock smoke accepted, không được mở bán production.

## 10. BOUNDARY GIỮA INTERNAL TRACE VÀ PUBLIC TRACE

## 10.1. Internal Trace

Internal Trace phục vụ vận hành nội bộ.

Internal Trace có thể chứa:

Source Origin.

Raw lot.

QC result.

Production Order.

Material Issue.

Material Receipt.

Batch.

Packaging.

Warehouse Receipt.

Ledger movement.

Recall impact.

CAPA evidence.

Audit trail.

Internal investigation.

Owner decision.

Internal Trace chỉ dành cho role có quyền.

## 10.2. Public Trace

Public Trace phục vụ người tiêu dùng hoặc kênh public.

Public Trace chỉ được hiển thị dữ liệu an toàn, đã whitelist.

Public Trace không phải bản sao của Internal Trace.

Public Trace không được tự động expose mọi dữ liệu có trong Operational Core.

Nguyên tắc:

Internal có thể nhiều, Public phải ít, đúng, an toàn, có kiểm soát.

## 11. BOUNDARY GIỮA PACKAGING / QR / TRACE

## 11.1. Packaging level

Packaging có thể có nhiều cấp:

## PACKET.

## BOX.

## CARTON.

Rule đã khóa:

PACKET không có QR public trace standalone nếu packaging rule đang áp dụng như đã khóa.

BOX/CARTON mới có QR public trace nếu scope packaging áp dụng.

QR phải đi theo lifecycle hợp lệ.

QR phải liên kết với batch/trace chain.

QR VOID/FAILED không public-valid.

## 11.2. QR không thay Batch Release

QR được in không đồng nghĩa batch được release.

QR được generate không đồng nghĩa sản phẩm sellable.

QR public-valid không được vượt qua:

Batch Release.

Warehouse Receipt.

Sale Lock.

Recall.

Public Trace whitelist.

Fail Gate:

Nếu chỉ vì QR đã in mà sản phẩm được bán, Operational Core FAIL.

## 12. OPERATIONAL SELLABLE SIGNAL

## 12.1. Định nghĩa Operational Sellable Signal

Operational Sellable Signal là tín hiệu vận hành cho biết một SKU hoặc finished good có đủ điều kiện vận hành để Commerce xem xét bán.

Operational Sellable Signal không phải giá bán.

Operational Sellable Signal không phải quote.

Operational Sellable Signal không phải order.

Operational Sellable Signal là điều kiện đầu vào cho Commerce.

## 12.2. Điều kiện tối thiểu để pass Operational Sellable Signal

Một sản phẩm/SKU chỉ pass Operational Sellable Signal khi:

Product/SKU hợp lệ theo TECH-02.

Batch liên quan đã RELEASED.

Warehouse Receipt đã confirmed.

Inventory Ledger có credit hợp lệ.

Lot Balance / finished goods balance khả dụng.

Không có recall active.

Không có sale lock active.

Không có quality hold active.

Trace chain hợp lệ nếu policy yêu cầu.

Public trace policy không vi phạm nếu public trace bật.

Release scope cho phép kênh downstream sử dụng.

## 12.3. Operational Sellable Signal không đủ để bán

Dù Operational Sellable Signal pass, Commerce vẫn phải kiểm tra thêm:

Giá active.

Chương trình giá.

Member benefit.

Quote hold window.

Payment rule.

Shipping rule.

Order rule.

Channel scope.

Customer/order constraints.

Vì vậy:

Operational Sellable Signal = đủ điều kiện vận hành để xét bán.
Commerce Sellable Decision = đủ điều kiện thương mại để quote/order.

## 13. RUNTIME UNAVAILABLE RULE

## 13.1. Khi Operational Runtime unavailable

Nếu Operational Runtime không khả dụng, downstream phải xử lý fail-safe.

Không được:

AI nói sản phẩm còn hàng.

Commerce tạo quote/order production.

Ads scale campaign dựa vào tồn không xác nhận.

Live chốt đơn sản phẩm chưa xác minh sellable.

CRM gửi upsell sản phẩm không rõ trạng thái.

Public Trace trả dữ liệu đoán.

## 13.2. Nguyên tắc fail-safe

Khi không xác minh được dữ liệu vận hành:

Không bán.

Không quote production.

Không public trace như hợp lệ.

Không hiển thị thông tin nội bộ.

Không tự mở sale lock.

Không tự bỏ recall.

Không fallback sang dữ liệu cũ nếu không có policy cho phép.

## 14. P0 điểm chặn NỀN CỦA TECH-03

## 14.1. Danh sách P0 điểm chặn nền

Operational Core bị bị chặn nếu có bất kỳ lỗi nào sau:

Mã

P0 điểm chặn

Hậu quả

## OP-P0-001

TECH-01 Foundation chưa clear permission/audit/idempotency/evidence

Không được release Operational

## OP-P0-002

TECH-02 Product chưa clear SKU/recipe/formula

Không được tạo production order production

## OP-P0-003

Raw lot QC_PASS được issue trực tiếp

Chặn material issue

## OP-P0-004

Material issue không idempotent

Chặn production flow

## OP-P0-005

Material receipt trừ tồn lần hai

Chặn inventory release

## OP-P0-006

Production order không snapshot formula

Chặn production release

## OP-P0-007

Batch QC_PASS tự động sellable/released

Chặn batch release

## OP-P0-008

Warehouse nhận batch chưa RELEASED

Chặn warehouse release

## OP-P0-009

Ledger không append-only

Chặn inventory release

## OP-P0-010

Public trace lộ dữ liệu nội bộ

Chặn public trace

## OP-P0-011

Recall/sale lock không chặn downstream

Chặn go-live

## OP-P0-012

Không có smoke recall/sale lock

Chặn production sale

## OP-P0-013

Downstream vượt Operational Sellable Signal

Chặn downstream release

## 15. EVIDENCE NGUYÊN TẮC CHO PHẦN 1

## 15.1. Evidence bắt buộc ở cấp nguyên tắc

Để TECH-03 Operational Core được xem là đủ điều kiện đi tiếp sang module contract và implementation planning, cần có evidence sau:

Evidence

Mục đích

Trạng thái yêu cầu

OP-EVD-001 - Operational Boundary Approval

Owner xác nhận boundary Product/Operational/Commerce/AI/Ads/Live

## ACCEPTED

OP-EVD-002 - P0 Rule Approval

Owner xác nhận các P0 rule nền

## ACCEPTED

OP-EVD-003 - No-Bypass Approval

Xác nhận không module nào được bypass Operational

## ACCEPTED

OP-EVD-004 - Public Trace Data Boundary Approval

Xác nhận whitelist-only, không lộ dữ liệu nội bộ

## ACCEPTED

OP-EVD-005 - Sale Lock / Recall Priority Approval

Xác nhận sale lock/recall thắng downstream

## ACCEPTED

OP-EVD-006 - Runtime Unavailable Policy Approval

Xác nhận fail-safe khi runtime unavailable

## ACCEPTED

## 15.2. Evidence chưa accepted thì không PASS

Nguyên tắc dựa trên PACK-10:

Không có accepted evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có release decision thì không Go-live Approved.

Không dùng lời xác nhận miệng để PASS.

## 16. SMOKE NGUYÊN TẮC CHO PHẦN 1

## 16.1. Smoke cấp nguyên tắc

Smoke cấp nguyên tắc chưa phải test chi tiết module, nhưng phải chứng minh boundary đúng.

Smoke ID

Nội dung smoke

Expected Result

## OP-SMK-P1-001

Product Active nhưng chưa có batch released

Không sellable

## OP-SMK-P1-002

Raw lot QC_PASS nhưng chưa READY_FOR_PRODUCTION

Không issue được

## OP-SMK-P1-003

Material Issue retry cùng command

Không trừ kho hai lần

## OP-SMK-P1-004

Batch QC_PASS nhưng chưa RELEASED

Warehouse không nhận

## OP-SMK-P1-005

Batch RELEASED nhưng chưa warehouse receipt

Finished goods stock chưa tăng

## OP-SMK-P1-006

Warehouse receipt confirmed

Finished goods stock tăng bằng ledger credit

## OP-SMK-P1-007

Recall active

Commerce/AI/Ads/Live bị chặn

## OP-SMK-P1-008

Sale Lock active

Quote/order production bị chặn

## OP-SMK-P1-009

## QR VOID/FAILED

Public trace không valid

## OP-SMK-P1-010

Public Trace request dữ liệu ngoài whitelist

Bị chặn, không lộ dữ liệu

## OP-SMK-P1-011

Operational Runtime unavailable

Downstream fail-safe, không bịa sellable

## OP-SMK-P1-012

Trace chain thiếu

Public trace không pass như hợp lệ

## 17. DONE GATE CỦA PHẦN 1/4

## 17.1. Điều kiện Done Gate

## PHẦN 1/4 chỉ được xem là DONE khi:

Vai trò Operational Core đã được khóa.

Source-of-truth đã rõ.

Greenfield rule đã rõ.

Boundary Product - Operational - Commerce đã rõ.

Boundary Operational - AI - Ads - Live - CRM đã rõ.

Chuỗi raw material -> production -> batch -> warehouse -> trace -> recall -> sale lock đã rõ.

No-bypass rule đã rõ.

P0 Operational Rules nền đã rõ.

Public Trace whitelist-only đã rõ.

Sale Lock / Recall priority đã rõ.

Runtime unavailable fail-safe đã rõ.

Evidence requirement đã rõ.

Smoke nguyên tắc đã rõ.

Fail Gate đã rõ.

Không gọi documentation complete là production ready.

## 18. FAIL GATE CỦA PHẦN 1/4

## 18.1. Các điều kiện làm PHẦN 1 fail

## PHẦN 1/4 FAIL nếu có bất kỳ điểm nào sau:

Operational Core bị viết như module kho đơn giản.

Product Active bị hiểu nhầm là Sellable.

QC_PASS bị hiểu nhầm là RELEASED.

Raw lot QC_PASS bị hiểu nhầm là READY_FOR_PRODUCTION.

Material Receipt được phép trừ tồn lần hai.

Inventory Ledger được phép sửa/xóa.

Batch chưa RELEASED vẫn nhập kho.

Warehouse Receipt không phải điểm tăng tồn thành phẩm.

Recall/Sale Lock không thắng downstream.

AI/Ads/Live/CRM được phép vượt Operational Core.

Public Trace không theo whitelist-only.

QR VOID/FAILED vẫn public-valid.

Không có trace chain vẫn public trace.

Runtime unavailable vẫn cho bán.

Không có evidence nhưng vẫn PASS.

Không có smoke nhưng vẫn release ready.

## 19. TRẠNG THÁI SAU PHẦN 1/4

## 19.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-03 PHẦN 1/4

## WRITTEN_FOR_REVIEW

Operational Core Principles

## LOCKED_DRAFT

Source-of-Truth Boundary

## LOCKED_DRAFT

No-Bypass Rule

## LOCKED_DRAFT

P0 Rule Foundation

## LOCKED_DRAFT

Evidence Requirement

## DEFINED

Smoke Principle

## DEFINED

Production Ready

KHONG

Release Ready

KHONG

Go-live Approved

KHONG

## 20. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 khóa tư duy nền của TECH-03:

Operational Core là lớp xác nhận sự thật vận hành.

Operational Core không thay Product, không thay Commerce, không thay AI, không thay Ads, không thay Live, không thay CRM và không thay MISA.

Operational Core có nhiệm vụ bảo vệ chuỗi vận hành thật:

Nguyên liệu đúng -> QC đúng -> lot đúng -> sản xuất đúng -> batch đúng -> release đúng -> kho đúng -> tồn đúng -> trace đúng -> recall đúng -> sale lock đúng.

Từ đây trở đi, mọi module trong TECH-03 phải tuân theo nguyên tắc:

Không bypass. Không nhảy cóc. Không tự suy luận sellable. Không mở bán nếu chưa release. Không public trace nếu thiếu chain. Không downstream nào được vượt recall/sale lock.

## PHẦN 1/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 2/4 - OPERATIONAL MODULE CONTRACTS

## 21. MỤC TIÊU CỦA PHẦN 2/4

## 21.1. Vai trò của Operational Module Contracts

## PHẦN 2/4 khóa contract cho từng module trong Operational Core.

Contract ở đây không phải API contract, không phải DB schema, không phải UI design và không phải code.

Contract ở đây là cam kết kỹ thuật ở cấp module:

Module này chịu trách nhiệm gì.

Module này không chịu trách nhiệm gì.

Module này nhận dữ liệu từ đâu.

Module này tạo dữ liệu gì.

Module nào được dùng dữ liệu này.

Điều kiện nào làm module bị block.

Evidence nào bắt buộc.

Smoke nào bắt buộc.

Downstream nào không được bypass module này.

Mục tiêu là để khi chuyển sang DEV phase, đội kỹ thuật không tự suy luận sai phạm vi.

## 21.2. Nguyên tắc đọc PHẦN 2/4

Tất cả module contract trong PHẦN 2 phải tuân thủ PHẦN 1:

Không bypass.

Không nhảy cóc trạng thái.

Không dùng Product Active thay Sellable.

Không dùng QC_PASS thay RELEASED.

Không dùng Raw Lot QC_PASS thay READY_FOR_PRODUCTION.

Không trừ kho hai lần.

Không sửa ledger lịch sử.

Không public trace ngoài whitelist.

Không để Commerce, AI, Ads, Live, CRM vượt Sale Lock / Recall.

## 22. DANH SÁCH MODULE CONTRACT TRONG TECH-03

Operational Core trong TECH-03 gồm các module contract sau:

OP-M01 - Source Origin.

OP-M02 - Raw Material Intake.

OP-M03 - Incoming QC.

OP-M04 - Raw Lot Readiness.

OP-M05 - Production Order.

OP-M06 - Formula Snapshot in Production Order.

OP-M07 - Material Request.

OP-M08 - Material Issue.

OP-M09 - Material Receipt.

OP-M10 - Production Process Execution.

OP-M11 - Packaging / Printing / QR.

OP-M12 - Batch QC.

OP-M13 - Batch Release.

OP-M14 - Warehouse Receipt.

OP-M15 - Inventory Ledger / Lot Balance.

OP-M16 - Traceability.

OP-M17 - Public Trace.

OP-M18 - Recall.

OP-M19 - Recovery / CAPA.

OP-M20 - Sale Lock / Stop Sale.

OP-M21 - Operational Handoff.

## OP-M01 - SOURCE ORIGIN

## 23. OP-M01 - SOURCE ORIGIN CONTRACT

## 23.1. Mục tiêu

Source Origin quản trị nguồn gốc nguyên liệu trước khi nguyên liệu được nhập vào hệ thống vận hành.

Source Origin trả lời câu hỏi:

Nguyên liệu này đến từ nguồn nào?

Là nguồn công ty tự trồng hay nguồn mua ngoài?

Nguồn này đã được xác minh chưa?

Nguồn này có đủ điều kiện để tạo Raw Material Intake không?

## 23.2. Scope In

Source Origin bao gồm:

Vùng trồng công ty.

Nguồn nguyên liệu tự trồng.

Nhà cung cấp nếu scope supplier được bật.

Nguồn mua ngoài.

Thông tin xác minh nguồn.

Trạng thái nguồn.

Evidence nguồn theo policy.

Quy tắc nguồn nào được phép đi vào Raw Material Intake.

## 23.3. Scope Out

Source Origin không chịu trách nhiệm:

Tạo raw lot.

QC nguyên liệu.

Mark raw lot ready.

Tính tồn kho.

Tạo production order.

Quyết định bán hàng.

Public toàn bộ hồ sơ nguồn cho khách hàng.

Thay Product Domain trong việc định nghĩa ingredient master.

## 23.4. Upstream Dependency

Source Origin phụ thuộc:

Upstream

Điều kiện

TECH-01 Foundation

Permission, audit, evidence

TECH-02 Product / Ingredient Master

Ingredient phải tồn tại và hợp lệ

Supplier Management nếu scope có

Supplier phải được phép cung ứng ingredient tương ứng

Owner Policy

Quy định evidence xác minh nguồn

## 23.5. Downstream Consumer

Source Origin cung cấp dữ liệu cho:

Raw Material Intake.

Incoming QC.

Raw Lot Readiness.

Internal Traceability.

Public Trace nếu whitelist cho phép.

Recall impact analysis.

MISA nếu scope mapping cần nguồn.

## 23.6. P0 điểm chặn

Source Origin bị block nếu:

Ingredient không tồn tại trong Product Domain.

Nguồn chưa verified nhưng vẫn được dùng để nhập nguyên liệu.

Supplier không nằm trong allowlist nhưng vẫn được tạo intake.

Evidence bắt buộc thiếu hoặc chưa clean/accepted.

Public trace lấy trực tiếp dữ liệu nội bộ của source origin.

Source Origin bị sửa lịch sử mà không có audit.

## 23.7. Evidence Required

Source origin approval evidence.

Supplier allowlist evidence nếu purchased.

Company source verification evidence nếu self-grown.

File admission metadata theo `OD-UPLOAD-001` nếu có file đính kèm; không dùng malware scan lifecycle làm điều kiện nghiệp vụ.

Owner review record.

## 23.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M01-SMK-001

Source chưa verified tạo intake

Bị chặn

## OP-M01-SMK-002

Supplier không được phép cung ứng ingredient

Bị chặn

## OP-M01-SMK-003

Source verified hợp lệ

Được phép chuyển sang intake

## OP-M01-SMK-004

Public trace gọi source internal evidence

Không hiển thị

## OP-M02 - RAW MATERIAL INTAKE

## 24. OP-M02 - RAW MATERIAL INTAKE CONTRACT

## 24.1. Mục tiêu

Raw Material Intake ghi nhận nguyên liệu được đưa vào hệ thống vận hành.

Raw Material Intake là điểm bắt đầu hình thành raw lot vận hành.

Module này trả lời:

Nguyên liệu gì được nhập?

Từ nguồn nào?

Số lượng bao nhiêu?

Ngày nhập?

Ai nhập?

Evidence nào đi kèm?

Trạng thái ban đầu của lot là gì?

## 24.2. Scope In

Raw Material Intake bao gồm:

Phiếu nhập nguyên liệu.

Dòng nguyên liệu nhập.

Thông tin source/supplier.

Quantity/UOM.

Evidence metadata.

Raw lot tạo ban đầu.

Trạng thái ban đầu của raw lot.

Ghi nhận procurement type nếu scope có.

Chặn nhập nếu source không hợp lệ.

## 24.3. Scope Out

Raw Material Intake không chịu trách nhiệm:

Quyết định QC pass/fail.

Đánh dấu ready for production.

Trừ tồn.

Xuất nguyên liệu.

Tạo production order.

Tự động mở bán.

Tự động public trace.

## 24.4. Upstream Dependency

Upstream

Điều kiện

OP-M01 Source Origin

Source hợp lệ

TECH-02 Ingredient Master

Ingredient hợp lệ

TECH-01 Foundation

Permission, audit, evidence

Supplier Management nếu scope có

Supplier được phép cung ứng ingredient

## 24.5. Downstream Consumer

Raw Material Intake cung cấp cho:

Incoming QC.

Raw Lot Readiness.

Inventory Ledger nếu có ghi nhận nhập kho nguyên liệu theo policy.

Internal Trace.

Recall impact.

MISA nếu scope kế toán nguyên liệu cần.

## 24.6. P0 điểm chặn

Raw Material Intake bị block nếu:

Source chưa verified.

Ingredient không active/hợp lệ.

Supplier không được phép cung ứng.

Quantity/UOM thiếu hoặc sai.

Evidence bắt buộc thiếu.

Intake tạo raw lot ở trạng thái READY_FOR_PRODUCTION ngay lập tức.

Intake bypass Incoming QC.

## 24.7. Evidence Required

Intake record.

Source verification link.

Supplier confirmation nếu purchased.

Evidence metadata.

Actor/role/audit record.

Environment/version nếu dùng evidence cho release gate.

## 24.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M02-SMK-001

Intake với source chưa verified

Bị chặn

## OP-M02-SMK-002

Intake với ingredient không hợp lệ

Bị chặn

## OP-M02-SMK-003

Intake hợp lệ

Tạo raw lot trạng thái ban đầu, chưa ready

## OP-M02-SMK-004

Intake thiếu evidence bắt buộc

Bị chặn hoặc WAITING theo policy

## OP-M03 - INCOMING QC

## 25. OP-M03 - INCOMING QC CONTRACT

## 25.1. Mục tiêu

Incoming QC kiểm tra chất lượng nguyên liệu đầu vào.

Incoming QC trả lời:

Raw lot có đạt QC đầu vào không?

Có bị hold/reject không?

Có cần evidence QC không?

Kết quả QC có đủ để xét readiness không?

## 25.2. Scope In

Incoming QC bao gồm:

QC inspection cho raw lot.

QC result.

QC criteria theo policy.

QC evidence metadata.

QC_PASS / QC_FAIL / HOLD / REJECT state.

Reason nếu fail/hold/reject.

Audit người thực hiện.

## 25.3. Scope Out

Incoming QC không chịu trách nhiệm:

Tự động mark raw lot ready.

Tự động issue nguyên liệu.

Tự động tạo production order.

Tự động public QC details.

Tự động mở bán.

## 25.4. Upstream Dependency

Upstream

Điều kiện

OP-M02 Raw Material Intake

Raw lot đã được tạo

TECH-01 Foundation

QC permission, audit, evidence

QC Policy

Criteria hợp lệ

Evidence Registry

Evidence metadata nếu có

## 25.5. Downstream Consumer

Incoming QC cung cấp dữ liệu cho:

Raw Lot Readiness.

Internal Trace.

Recall.

CAPA nếu QC fail/defect.

Reporting.

Public Trace chỉ dùng dữ liệu đã whitelist, không lộ defect/internal note.

## 25.6. P0 điểm chặn

Incoming QC bị block nếu:

QC result được sửa không audit.

QC_PASS tự động chuyển raw lot thành READY_FOR_PRODUCTION.

QC_FAIL/HOLD vẫn cho issue.

QC defect/internal note bị public trace.

Evidence QC bắt buộc thiếu nhưng vẫn pass release gate.

Người không có permission thực hiện QC.

## 25.7. Evidence Required

QC inspection record.

QC criteria reference.

QC result.

QC evidence metadata.

Actor/audit.

Reviewer nếu policy yêu cầu.

## 25.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M03-SMK-001

Raw lot QC_PASS

Chưa READY_FOR_PRODUCTION

## OP-M03-SMK-002

Raw lot QC_FAIL

Không issue được

## OP-M03-SMK-003

## QC HOLD

Không issue được

## OP-M03-SMK-004

Public trace đòi defect note

Không hiển thị

## OP-M04 - RAW LOT READINESS

## 26. OP-M04 - RAW LOT READINESS CONTRACT

## 26.1. Mục tiêu

Raw Lot Readiness là module quyết định raw lot có đủ điều kiện đưa vào sản xuất hay chưa.

Đây là checkpoint bắt buộc giữa Incoming QC và Material Issue.

## 26.2. Scope In

Raw Lot Readiness bao gồm:

Kiểm tra QC_PASS.

Kiểm tra không HOLD.

Kiểm tra không REJECT.

Kiểm tra không recall/sale/quality restriction.

Kiểm tra source hợp lệ.

Kiểm tra balance khả dụng.

Mark READY_FOR_PRODUCTION.

Audit người duyệt.

Reason/evidence nếu policy yêu cầu.

## 26.3. Scope Out

Raw Lot Readiness không chịu trách nhiệm:

Tạo raw lot.

QC raw lot.

Xuất nguyên liệu.

Trừ tồn.

Tạo production order.

Quyết định bán thành phẩm.

## 26.4. Upstream Dependency

Upstream

Điều kiện

OP-M01 Source Origin

Source verified

OP-M02 Raw Intake

Raw lot tồn tại

OP-M03 Incoming QC

## QC_PASS

OP-M15 Inventory / Lot Balance

Có balance khả dụng

TECH-01 Foundation

Permission, audit, evidence

## 26.5. Downstream Consumer

Raw Lot Readiness cung cấp cho:

Material Request.

Material Issue.

Production Order validation nếu cần.

Internal Trace.

Recall impact.

## 26.6. P0 điểm chặn

Raw Lot Readiness bị block nếu:

QC chưa PASS.

Raw lot đang HOLD.

Raw lot REJECTED.

Source không verified.

Lot balance không đủ.

Evidence bắt buộc thiếu.

Người không có permission mark ready.

Raw lot bị recall/quality restriction.

Mark ready không audit.

## 26.7. Evidence Required

Readiness approval record.

QC_PASS reference.

Source verification reference.

Balance check record.

Actor/permission/audit.

Evidence nếu policy yêu cầu.

## 26.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M04-SMK-001

QC_PASS nhưng chưa mark ready

Không issue được

## OP-M04-SMK-002

QC_FAIL mark ready

Bị chặn

## OP-M04-SMK-003

HOLD mark ready

Bị chặn

## OP-M04-SMK-004

Ready lot issue

Được phép sang Material Issue nếu balance đủ

## OP-M05 - PRODUCTION ORDER

## 27. OP-M05 - PRODUCTION ORDER CONTRACT

## 27.1. Mục tiêu

Production Order là lệnh sản xuất chính thức cho một SKU/sản lượng/scope cụ thể.

Production Order trả lời:

Sản xuất SKU nào?

Công thức nào?

Formula kind/version nào?

Sản lượng kế hoạch bao nhiêu?

Nguyên liệu cần dùng là gì?

Lệnh này đã approved chưa?

Batch nào được tạo từ lệnh này?

## 27.2. Scope In

Production Order bao gồm:

SKU được sản xuất.

Production quantity.

Formula kind/version.

Formula snapshot reference.

Production scope.

Planned batch.

Approval state.

Owner/actor.

Audit.

Link đến material request.

Link đến batch sau này.

## 27.3. Scope Out

Production Order không chịu trách nhiệm:

Định nghĩa Product/SKU.

Tự sửa recipe.

Tự thay formula version.

Trừ tồn nguyên liệu.

Tăng tồn thành phẩm.

Mở bán.

Quote/order/payment.

## 27.4. Upstream Dependency

Upstream

Điều kiện

TECH-02 Product/SKU

SKU hợp lệ

TECH-02 Recipe/Formula

Formula approved

OP-M06 Formula Snapshot

Snapshot bắt buộc

TECH-01 Foundation

Permission, audit, approval

MRP/Demand nếu scope có

Sản lượng kế hoạch hợp lệ

## 27.5. Downstream Consumer

Production Order cung cấp cho:

Material Request.

Material Issue.

Material Receipt.

Production Process.

Batch QC.

Batch Release.

Traceability.

Inventory/warehouse.

MISA nếu scope có.

## 27.6. P0 điểm chặn

Production Order bị block nếu:

SKU không hợp lệ.

Recipe/formula chưa approved.

Không có formula snapshot.

Formula snapshot thiếu ingredient/quantity/UOM.

User không có quyền tạo/approve.

Production Order dùng live recipe thay snapshot.

Product Domain chưa clear.

Production quantity vượt policy mà không có approval.

## 27.7. Evidence Required

Production Order record.

Formula snapshot.

SKU/formula approval reference.

Owner approval.

Audit.

Evidence nếu dùng cho release gate.

## 27.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M05-SMK-001

Tạo PO với SKU chưa approved

Bị chặn

## OP-M05-SMK-002

Tạo PO không có formula snapshot

Bị chặn

## OP-M05-SMK-003

Recipe đổi sau khi PO tạo

PO snapshot không đổi

## OP-M05-SMK-004

PO hợp lệ

Cho phép tạo material request

## OP-M06 - FORMULA SNAPSHOT IN PRODUCTION ORDER

## 28. OP-M06 - FORMULA SNAPSHOT CONTRACT

## 28.1. Mục tiêu

Formula Snapshot bảo vệ lịch sử sản xuất.

Một Production Order đã tạo phải giữ nguyên công thức tại thời điểm tạo, không bị thay đổi bởi recipe live sau này.

## 28.2. Scope In

Formula Snapshot bao gồm:

## SKU.

Formula kind.

Formula version.

Ingredient.

Ingredient group.

Quantity.

## UOM.

Usage basis.

Approved recipe reference.

Snapshot timestamp.

Snapshot actor.

Production quantity base.

## 28.3. Scope Out

Formula Snapshot không chịu trách nhiệm:

Duyệt recipe gốc.

Tạo ingredient master.

Tự thay đổi công thức.

Tính giá thành chi tiết.

Tự động thay đổi Material Issue nếu PO đã approved mà không có change governance.

## 28.4. Upstream Dependency

Upstream

Điều kiện

TECH-02 Recipe

Recipe approved

TECH-02 Formula Version

Formula kind/version hợp lệ

OP-M05 Production Order

PO cần snapshot

TECH-01 Audit

Snapshot phải audit được

## 28.5. Downstream Consumer

Formula Snapshot cung cấp cho:

Material Request.

Material Issue.

Production Process.

Batch genealogy.

Traceability.

MISA/costing handoff nếu scope có.

Recall impact.

## 28.6. P0 điểm chặn

Formula Snapshot bị block nếu:

Thiếu formula kind/version.

Thiếu ingredient quantity/UOM.

Thiếu group theo 4 group đã khóa.

Snapshot bị ghi đè.

PO dùng live recipe thay snapshot.

Snapshot không audit được.

Formula không approved.

## 28.7. Evidence Required

Snapshot record.

Recipe approval reference.

Formula version reference.

Snapshot immutability evidence.

Audit.

## 28.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M06-SMK-001

Recipe live đổi sau PO

Snapshot giữ nguyên

## OP-M06-SMK-002

Snapshot thiếu UOM

PO bị chặn

## OP-M06-SMK-003

Snapshot thiếu group

PO bị chặn

## OP-M06-SMK-004

Truy xuất batch history

Ra đúng snapshot ban đầu

## OP-M07 - MATERIAL REQUEST

## 29. OP-M07 - MATERIAL REQUEST CONTRACT

## 29.1. Mục tiêu

Material Request là yêu cầu cấp nguyên liệu cho Production Order.

Module này chuyển nhu cầu từ Production Order thành yêu cầu nguyên liệu có kiểm soát.

## 29.2. Scope In

Material Request bao gồm:

Link đến Production Order.

Danh sách ingredient từ formula snapshot.

Quantity requested.

## UOM.

Required date/time.

Request status.

Approval nếu policy yêu cầu.

Check raw lot availability ở mức yêu cầu.

Audit.

## 29.3. Scope Out

Material Request không chịu trách nhiệm:

Trừ tồn.

Chọn final lot xuất nếu chưa đến bước issue.

Ghi ledger debit.

Xác nhận xưởng nhận.

Điều chỉnh công thức.

Tăng tồn thành phẩm.

## 29.4. Upstream Dependency

Upstream

Điều kiện

OP-M05 Production Order

PO hợp lệ

OP-M06 Formula Snapshot

Snapshot đầy đủ

OP-M04 Raw Lot Readiness

Có lot ready để xét

OP-M15 Lot Balance

Có tồn khả dụng

TECH-01 Foundation

Permission, audit

## 29.5. Downstream Consumer

Material Request cung cấp cho:

Material Issue.

Production Process planning.

Inventory allocation nếu scope có.

Traceability.

## 29.6. P0 điểm chặn

Material Request bị block nếu:

Production Order chưa hợp lệ.

Formula snapshot thiếu.

Ingredient không thuộc snapshot.

Quantity request vượt snapshot mà không có approval.

Không có raw lot ready nhưng vẫn approve request như khả dụng.

Request không audit.

## 29.7. Evidence Required

Material request record.

PO reference.

Formula snapshot reference.

Approval record nếu có.

Availability check record.

Audit.

## 29.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M07-SMK-001

Request từ PO không có snapshot

Bị chặn

## OP-M07-SMK-002

Request ingredient ngoài snapshot

Bị chặn

## OP-M07-SMK-003

Request hợp lệ

Cho phép sang issue

## OP-M07-SMK-004

Không có lot ready

Request không được xem là fulfillable

## OP-M08 - MATERIAL ISSUE

## 30. OP-M08 - MATERIAL ISSUE CONTRACT

## 30.1. Mục tiêu

Material Issue là điểm xuất nguyên liệu chính thức cho sản xuất.

Đây là checkpoint giảm tồn nguyên liệu chính.

## 30.2. Scope In

Material Issue bao gồm:

Link đến Material Request.

Link đến Production Order.

Chọn raw lot cụ thể.

Kiểm tra READY_FOR_PRODUCTION.

Kiểm tra lot balance.

Quantity issued.

Ledger debit.

Idempotency key.

Audit.

Variance nếu có policy.

## 30.3. Scope Out

Material Issue không chịu trách nhiệm:

Xưởng xác nhận đã nhận nguyên liệu.

QC nguyên liệu.

Tăng tồn thành phẩm.

Release batch.

Mở bán.

Sửa recipe.

Sửa ledger lịch sử.

## 30.4. Upstream Dependency

Upstream

Điều kiện

OP-M07 Material Request

Request hợp lệ

OP-M04 Raw Lot Readiness

Lot READY_FOR_PRODUCTION

OP-M15 Lot Balance

Balance đủ

TECH-01 Idempotency

Command không được trừ trùng

TECH-01 Audit

Audit bắt buộc

## 30.5. Downstream Consumer

Material Issue cung cấp cho:

Material Receipt.

Production Process.

Inventory Ledger.

Traceability.

Recall impact.

MISA nếu scope có.

## 30.6. P0 điểm chặn

Material Issue bị block nếu:

Raw lot chưa READY_FOR_PRODUCTION.

Lot balance không đủ.

Material Request chưa approved.

Ingredient không thuộc formula snapshot.

Issue không idempotent.

Retry tạo ledger debit trùng.

Issue không audit.

Issue trực tiếp không qua request nếu policy yêu cầu request.

Issue từ raw lot HOLD/REJECT/RECALL.

## 30.7. Evidence Required

Material issue record.

Raw lot readiness reference.

Lot balance check.

Ledger debit record.

Idempotency evidence.

Audit.

## 30.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M08-SMK-001

Issue lot QC_PASS chưa ready

Bị chặn

## OP-M08-SMK-002

Issue lot ready đủ balance

Ledger debit tạo đúng

## OP-M08-SMK-003

Retry issue cùng idempotency key

Không trừ kho lần hai

## OP-M08-SMK-004

Issue ingredient ngoài snapshot

Bị chặn

## OP-M09 - MATERIAL RECEIPT

## 31. OP-M09 - MATERIAL RECEIPT CONTRACT

## 31.1. Mục tiêu

Material Receipt xác nhận xưởng đã nhận nguyên liệu được xuất.

Material Receipt không phải điểm giảm tồn nguyên liệu chính.

## 31.2. Scope In

Material Receipt bao gồm:

Link đến Material Issue.

Quantity received.

Variance ghi nhận nếu có.

Receipt confirmation.

Actor/receiver.

Timestamp.

Evidence nếu policy yêu cầu.

Audit.

## 31.3. Scope Out

Material Receipt không chịu trách nhiệm:

Trừ tồn nguyên liệu lần hai.

Tự sửa Material Issue.

Tự thay đổi formula snapshot.

Tăng tồn thành phẩm.

Release batch.

Mở bán.

## 31.4. Upstream Dependency

Upstream

Điều kiện

OP-M08 Material Issue

Issue đã ghi nhận

TECH-01 Foundation

Permission, audit, evidence

Variance Policy

Cách xử lý chênh lệch nếu có

## 31.5. Downstream Consumer

Material Receipt cung cấp cho:

Production Process Execution.

Traceability.

Variance review.

MISA nếu scope có.

Internal reporting.

## 31.6. P0 điểm chặn

Material Receipt bị block nếu:

Không có Material Issue.

Receipt tự trừ tồn lần hai.

Receipt sửa ledger debit cũ.

Variance không có reason.

Người nhận không có permission.

Receipt không audit.

## 31.7. Evidence Required

Receipt confirmation record.

Material issue reference.

Variance record nếu có.

Actor/audit.

Evidence nếu policy yêu cầu.

## 31.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M09-SMK-001

Receipt không có issue

Bị chặn

## OP-M09-SMK-002

Receipt sau issue

Không trừ tồn lần hai

## OP-M09-SMK-003

Receipt có variance

Ghi variance, không sửa ledger cũ

## OP-M09-SMK-004

Receipt không permission

Bị chặn

## OP-M10 - PRODUCTION PROCESS EXECUTION

## 32. OP-M10 - PRODUCTION PROCESS EXECUTION CONTRACT

## 32.1. Mục tiêu

Production Process Execution ghi nhận các bước sản xuất thực tế của batch.

Module này chứng minh batch đã đi qua quy trình vận hành theo scope.

## 32.2. Scope In

Production Process Execution bao gồm:

Link đến Production Order.

Link đến Material Receipt.

Batch creation/progress.

Process steps.

Start/end time.

Operator/role.

Process status.

Hold/halt/cancel nếu có.

Evidence nếu policy yêu cầu.

Audit.

## 32.3. Scope Out

Production Process Execution không chịu trách nhiệm:

Tạo recipe.

Thay formula snapshot.

QC thành phẩm cuối.

Release batch.

Nhập kho.

Tăng tồn.

Mở bán.

Quote/order.

## 32.4. Upstream Dependency

Upstream

Điều kiện

OP-M05 Production Order

PO hợp lệ

OP-M09 Material Receipt

Xưởng đã nhận nguyên liệu

TECH-01 Foundation

Permission, audit

Process Policy

Các bước sản xuất theo scope

## 32.5. Downstream Consumer

Production Process Execution cung cấp cho:

Packaging/Printing/QR.

Batch QC.

Batch Release.

Traceability.

Recall impact.

Reporting.

## 32.6. P0 điểm chặn

Production Process bị block nếu:

Không có Production Order.

Không có Material Receipt nếu policy yêu cầu.

Process bypass nguyên liệu đã issue.

Process hoàn tất nhưng không audit.

Batch tạo không link PO.

Process bị HOLD/HALT nhưng vẫn cho đi QC/release.

Process tự tăng tồn thành phẩm.

## 32.7. Evidence Required

Process execution record.

PO reference.

Material receipt reference.

Batch record.

Operator/audit.

Process evidence nếu policy yêu cầu.

## 32.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M10-SMK-001

Process không có PO

Bị chặn

## OP-M10-SMK-002

Process từ PO hợp lệ

Tạo/tiến hành batch đúng link

## OP-M10-SMK-003

Batch HOLD

Không cho release

## OP-M10-SMK-004

Process completed

Chưa tăng tồn thành phẩm

## OP-M11 - PACKAGING / PRINTING / QR

## 33. OP-M11 - PACKAGING / PRINTING / QR CONTRACT

## 33.1. Mục tiêu

Packaging / Printing / QR quản trị đóng gói, in thông tin lô và tạo QR theo cấp bao bì.

Module này không tự quyết định batch được bán.

## 33.2. Scope In

Packaging / Printing / QR bao gồm:

Packaging job.

Package level: PACKET / BOX / CARTON.

Batch link.

Print content theo approved template.

Lot/batch/date/expiry nếu scope áp dụng.

QR generation.

QR lifecycle.

QR print status.

Reprint/void/fail handling.

Audit.

Device/printer evidence nếu scope có.

## 33.3. Scope Out

Packaging / QR không chịu trách nhiệm:

Release batch.

Nhập kho.

Tăng tồn.

Mở bán.

Public trace nếu thiếu trace chain.

Quote/order.

Tự tạo GTIN/GS1 ngoài Product/Trade Item config.

## 33.4. Upstream Dependency

Upstream

Điều kiện

OP-M10 Production Process

Batch/process đủ điều kiện đóng gói

TECH-02 Product/Trade Item nếu scope có

Package/trade item config hợp lệ

OP-M16 Traceability

Trace link cần thiết

TECH-01 Foundation

Permission, audit, evidence

Device/Printer Policy

Nếu có máy in thật

## 33.5. Downstream Consumer

Packaging / QR cung cấp cho:

Batch QC nếu QC sau đóng gói.

Batch Release.

Warehouse Receipt.

Traceability.

Public Trace.

Recall.

Commerce/AI nếu cần hiển thị QR/public trace status theo policy.

## 33.6. P0 điểm chặn

Packaging / QR bị block nếu:

Batch không hợp lệ.

QR không link batch/trace chain.

QR VOID/FAILED vẫn public-valid.

PACKET tạo QR public trace standalone trái rule đã khóa.

BOX/CARTON QR không theo scope.

In sai batch/NSX/HSD/lot.

Reprint không audit.

QR lifecycle bị ghi đè.

QR/print job bị xem là điều kiện mở bán dù batch chưa release/warehouse chưa nhận.

## 33.7. Evidence Required

Packaging job record.

Print template approval.

QR lifecycle record.

Device/printer callback evidence nếu scope có.

Reprint/void reason nếu có.

Audit.

## 33.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M11-SMK-001

QR generated nhưng batch chưa release

Không sellable

## OP-M11-SMK-002

## QR FAILED

Public trace không valid

## OP-M11-SMK-003

## QR VOID

Public trace không valid

## OP-M11-SMK-004

PACKET public trace standalone

Bị chặn nếu rule packaging áp dụng

## OP-M11-SMK-005

BOX/CARTON QR valid + trace chain đủ

Public trace xét theo whitelist

## OP-M12 - BATCH QC

## 34. OP-M12 - BATCH QC CONTRACT

## 34.1. Mục tiêu

Batch QC kiểm tra chất lượng thành phẩm/batch trước khi release.

Batch QC trả lời:

Batch có đạt QC không?

Batch có bị hold/reject không?

Batch có đủ điều kiện để owner xem xét release không?

## 34.2. Scope In

Batch QC bao gồm:

QC inspection cho batch.

QC criteria theo policy.

QC result.

## QC_PASS / QC_FAIL / HOLD / REJECT.

Evidence metadata.

Reason nếu fail/hold/reject.

Audit.

## 34.3. Scope Out

Batch QC không chịu trách nhiệm:

Tự động release batch.

Tự động nhập kho.

Tự động tăng tồn thành phẩm.

Tự động mở bán.

Public defect/internal note.

Quote/order.

## 34.4. Upstream Dependency

Upstream

Điều kiện

OP-M10 Production Process

Batch đã sản xuất theo scope

OP-M11 Packaging nếu QC sau đóng gói

Packaging job hợp lệ

TECH-01 Foundation

Permission, audit, evidence

QC Policy

Criteria hợp lệ

## 34.5. Downstream Consumer

Batch QC cung cấp cho:

Batch Release.

Warehouse Receipt gating.

Traceability.

Recall.

## CAPA.

Reporting.

## 34.6. P0 điểm chặn

Batch QC bị block nếu:

Batch không có production history.

QC_PASS tự động release.

QC_PASS tự động nhập kho.

QC_FAIL vẫn cho release.

QC internal note public ra ngoài.

QC evidence bắt buộc thiếu.

QC result sửa không audit.

## 34.7. Evidence Required

Batch QC record.

QC criteria reference.

QC result.

Evidence metadata.

Actor/audit.

Reviewer nếu policy yêu cầu.

## 34.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M12-SMK-001

Batch QC_PASS

Chưa RELEASED

## OP-M12-SMK-002

Batch QC_FAIL

Không release

## OP-M12-SMK-003

Batch HOLD

Không warehouse receipt

## OP-M12-SMK-004

Public trace gọi QC defect

Không hiển thị

## OP-M13 - BATCH RELEASE

## 35. OP-M13 - BATCH RELEASE CONTRACT

## 35.1. Mục tiêu

Batch Release là hành động chính thức cho phép batch rời trạng thái QC đạt sang trạng thái được phép nhập kho thành phẩm.

Batch Release là checkpoint bắt buộc trước Warehouse Receipt.

## 35.2. Scope In

Batch Release bao gồm:

Kiểm tra Batch QC_PASS.

Kiểm tra không HOLD.

Kiểm tra không REJECT.

Kiểm tra không recall/quality lock.

Release decision.

Release actor/owner.

Release timestamp.

Release evidence.

Audit.

## 35.3. Scope Out

Batch Release không chịu trách nhiệm:

Tăng tồn thành phẩm.

Tạo quote/order.

Tự động public trace.

Tự động chạy Ads/Live.

Tự động mở bán nếu chưa warehouse receipt.

Sửa QC result.

## 35.4. Upstream Dependency

Upstream

Điều kiện

OP-M12 Batch QC

## QC_PASS

OP-M10 Production Process

Batch hoàn tất

OP-M11 Packaging nếu scope có

Packaging/QR hợp lệ

TECH-01 Foundation

Permission, audit, evidence

## 35.5. Downstream Consumer

Batch Release cung cấp cho:

Warehouse Receipt.

Inventory Ledger.

Traceability.

Public Trace nếu chain đủ.

Commerce sellable evaluation.

AI/Ads/Live sale gating qua Operational Handoff.

## 35.6. P0 điểm chặn

Batch Release bị block nếu:

Batch chưa QC_PASS.

Batch HOLD/REJECT.

Batch có active quality issue.

Batch có recall restriction.

Release không audit.

Release thiếu evidence bắt buộc.

Người release không có permission.

Batch Release tự tăng tồn thành phẩm.

## 35.7. Evidence Required

Batch release decision record.

QC_PASS reference.

Packaging/QR status nếu scope có.

Owner approval.

Audit.

Evidence package nếu production release.

## 35.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M13-SMK-001

QC_PASS nhưng chưa release

Warehouse không nhận

## OP-M13-SMK-002

QC_FAIL release

Bị chặn

## OP-M13-SMK-003

HOLD release

Bị chặn

## OP-M13-SMK-004

Release hợp lệ

Cho phép warehouse receipt

## OP-M14 - WAREHOUSE RECEIPT

## 36. OP-M14 - WAREHOUSE RECEIPT CONTRACT

## 36.1. Mục tiêu

Warehouse Receipt xác nhận kho thành phẩm đã nhận batch released.

Đây là checkpoint tăng tồn thành phẩm.

## 36.2. Scope In

Warehouse Receipt bao gồm:

Link đến batch RELEASED.

Quantity received.

Package/trade item level nếu scope có.

Warehouse/location.

Receipt confirmation.

Ledger credit.

Actor/warehouse keeper.

Audit.

Variance nếu có.

## 36.3. Scope Out

Warehouse Receipt không chịu trách nhiệm:

Release batch.

QC batch.

Sửa production order.

Sửa formula snapshot.

Quote/order/payment.

Quyết định giá bán.

Public trace ngoài whitelist.

## 36.4. Upstream Dependency

Upstream

Điều kiện

OP-M13 Batch Release

Batch RELEASED

OP-M11 Packaging nếu scope có

Packaging/QR hợp lệ

TECH-01 Foundation

Permission, audit, idempotency

Warehouse Policy

Warehouse/location hợp lệ

## 36.5. Downstream Consumer

Warehouse Receipt cung cấp cho:

Inventory Ledger.

Lot Balance / Finished Goods Balance.

Commerce sellable evaluation.

AI/Ads/Live sale gating.

Traceability.

Recall.

MISA nếu scope có.

## 36.6. P0 điểm chặn

Warehouse Receipt bị block nếu:

Batch chưa RELEASED.

Batch QC_PASS nhưng chưa RELEASED.

Batch HOLD/REJECT/RECALL.

Warehouse/location không hợp lệ.

Receipt không idempotent.

Retry tạo credit trùng.

Warehouse receipt không audit.

Tồn thành phẩm tăng trước warehouse receipt confirmed.

## 36.7. Evidence Required

Warehouse receipt record.

Batch release reference.

Ledger credit record.

Idempotency evidence.

Warehouse/location confirmation.

Audit.

## 36.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M14-SMK-001

Receipt batch QC_PASS chưa release

Bị chặn

## OP-M14-SMK-002

Receipt batch RELEASED

Tạo ledger credit

## OP-M14-SMK-003

Retry receipt

Không tăng tồn hai lần

## OP-M14-SMK-004

Receipt confirmed

Finished goods balance tăng đúng

## OP-M15 - INVENTORY LEDGER / LOT BALANCE

## 37. OP-M15 - INVENTORY LEDGER / LOT BALANCE CONTRACT

## 37.1. Mục tiêu

Inventory Ledger là sổ cái tồn kho append-only.

Lot Balance là projection từ ledger, dùng để đọc nhanh số lượng khả dụng.

## 37.2. Scope In

Inventory Ledger / Lot Balance bao gồm:

Raw material debit/credit theo policy.

Finished goods credit/debit theo policy.

Movement type.

Quantity/UOM.

Source document.

Lot/batch reference.

Idempotency reference.

Balance projection.

Adjustment movement có reason.

Audit.

## 37.3. Scope Out

Inventory Ledger không chịu trách nhiệm:

Tự quyết định QC.

Tự mark raw lot ready.

Tự release batch.

Tự quote/order.

Tự public trace.

Sửa dòng lịch sử để điều chỉnh tồn.

## 37.4. Upstream Dependency

Upstream

Điều kiện

OP-M02 Raw Intake

Nếu nhập nguyên liệu tạo credit theo policy

OP-M08 Material Issue

Raw material debit

OP-M14 Warehouse Receipt

Finished goods credit

Commerce sau này

Finished goods debit khi order fulfillment nếu scope TECH-04

TECH-01 Foundation

Idempotency, audit

## 37.5. Downstream Consumer

Inventory Ledger / Lot Balance cung cấp cho:

Raw Lot Readiness.

Material Issue.

Warehouse.

Commerce sellable evaluation.

AI/Ads/Live stock-safe signal.

Traceability.

Recall impact.

MISA nếu scope có.

Reporting.

## 37.6. P0 điểm chặn

Inventory bị block nếu:

Ledger không append-only.

Balance sửa trực tiếp không movement.

Retry tạo debit/credit trùng.

Material Receipt trừ tồn lần hai.

Warehouse Receipt tăng tồn trùng.

Balance âm không có policy xử lý.

Stock khả dụng không trừ sale lock/hold/recall.

Downstream dùng balance không qua sellable/sale lock filter.

## 37.7. Evidence Required

Ledger movement records.

Source document reference.

Idempotency evidence.

Balance projection evidence.

Adjustment approval nếu có.

Audit.

## 37.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M15-SMK-001

Material Issue

Ledger debit đúng

## OP-M15-SMK-002

Material Receipt

Không tạo debit lần hai

## OP-M15-SMK-003

Warehouse Receipt

Ledger credit đúng

## OP-M15-SMK-004

Retry debit/credit

Không trùng movement

## OP-M15-SMK-005

Adjustment

Tạo movement mới, không sửa dòng cũ

## OP-M16 - TRACEABILITY

## 38. OP-M16 - TRACEABILITY CONTRACT

## 38.1. Mục tiêu

Traceability xây dựng chuỗi truy xuất nội bộ từ nguyên liệu đến thành phẩm.

Traceability trả lời:

Batch này dùng raw lot nào?

Raw lot đến từ nguồn nào?

Batch được sản xuất từ Production Order nào?

Batch đã qua QC/release/kho chưa?

QR nào gắn với batch/package nào?

Nếu recall thì ảnh hưởng đến những gì?

## 38.2. Scope In

Traceability bao gồm:

Product/SKU link.

Formula snapshot link.

Source origin link.

Raw lot link.

Material issue link.

Material receipt link.

Production order link.

Batch link.

Packaging/QR link.

Batch QC/release link.

Warehouse receipt link.

Inventory movement link.

Recall impact link.

Internal trace graph.

## 38.3. Scope Out

Traceability không chịu trách nhiệm:

Tạo dữ liệu gốc thay module owner.

Public toàn bộ internal trace.

Tự release batch.

Tự mở bán.

Tự quote/order.

Sửa ledger/QC/release history.

## 38.4. Upstream Dependency

Upstream

Điều kiện

OP-M01 đến OP-M15

Các node trace phải có owner

TECH-02 Product

Product/SKU/recipe source

TECH-01 Foundation

Audit/evidence

## OP-M11 QR

QR/package link nếu scope có

## 38.5. Downstream Consumer

Traceability cung cấp cho:

Public Trace.

Recall.

## CAPA.

Commerce safety gate nếu cần.

AI/public response nếu whitelist.

MISA/reconcile nếu scope có.

Owner audit/review.

## 38.6. P0 điểm chặn

Traceability bị block nếu:

Trace chain thiếu node bắt buộc.

Batch không link Production Order.

Production Order không link formula snapshot.

Material Issue không link raw lot.

Warehouse receipt không link batch.

QR không link batch/package.

Public Trace dùng trace chain thiếu.

Trace tự sửa dữ liệu lịch sử.

## 38.7. Evidence Required

Trace chain evidence.

Node linkage evidence.

Missing chain report nếu có.

Internal trace review.

Audit.

## 38.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M16-SMK-001

Batch đầy đủ chain

Internal trace pass

## OP-M16-SMK-002

Thiếu raw lot link

Trace fail

## OP-M16-SMK-003

Thiếu warehouse receipt

Trace không đủ sellable/public

## OP-M16-SMK-004

Recall impact query

Trả đúng affected lots/batches/SKU

## OP-M17 - PUBLIC TRACE

## 39. OP-M17 - PUBLIC TRACE CONTRACT

## 39.1. Mục tiêu

Public Trace cung cấp thông tin truy xuất an toàn cho người dùng cuối theo whitelist.

Public Trace không phải Internal Trace.

## 39.2. Scope In

Public Trace bao gồm:

QR lookup.

Public whitelist DTO/view.

Product public info.

Batch public-safe info.

Source public-safe info nếu policy cho phép.

Manufacture/expiry nếu policy cho phép.

Public trace status.

Safe error response.

QR valid/invalid handling.

## 39.3. Scope Out

Public Trace không chịu trách nhiệm:

Hiển thị toàn bộ internal trace.

Hiển thị supplier evidence nội bộ.

Hiển thị personnel.

Hiển thị costing.

Hiển thị QC defect/loss/internal note.

Hiển thị MISA/private data.

Sửa trace chain.

Quyết định sellable.

Quote/order.

## 39.4. Upstream Dependency

Upstream

Điều kiện

OP-M16 Traceability

Trace chain đủ

## OP-M11 QR

QR valid

OP-M13 Batch Release

Batch release status nếu policy yêu cầu

OP-M14 Warehouse Receipt

Warehouse confirmed nếu public policy yêu cầu

Public Trace Policy

Whitelist field

TECH-01 Security/Audit

Public boundary kiểm soát

## 39.5. Downstream Consumer

Public Trace phục vụ:

Người tiêu dùng.

AI Advisor nếu cần gửi link public trace.

Facebook/Messenger nếu khách hỏi truy xuất.

Customer support.

Recall communication nếu policy cho phép.

## 39.6. P0 điểm chặn

Public Trace bị block nếu:

Trace chain thiếu.

## QR VOID/FAILED.

QR không link batch/package.

Public DTO chứa internal field.

Batch chưa release nhưng public hiển thị như hợp lệ nếu policy cấm.

Recall active nhưng public trace không xử lý an toàn.

Public trace policy unavailable nhưng vẫn trả dữ liệu rộng.

## 39.7. Evidence Required

Public whitelist approval.

Public trace response review.

Security review.

QR lifecycle evidence.

Trace chain evidence.

Negative test evidence.

## 39.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M17-SMK-001

QR valid + chain đủ

Trả public whitelist

## OP-M17-SMK-002

## QR VOID

Không public-valid

## OP-M17-SMK-003

## QR FAILED

Không public-valid

## OP-M17-SMK-004

Chain thiếu

Fail-safe

## OP-M17-SMK-005

Request internal field

Không trả dữ liệu

## OP-M18 - RECALL

## 40. OP-M18 - RECALL CONTRACT

## 40.1. Mục tiêu

Recall quản trị sự cố thu hồi, ảnh hưởng đến lot/batch/SKU/order/channel nếu scope có.

Recall phải thắng toàn bộ downstream bán hàng.

## 40.2. Scope In

Recall bao gồm:

Recall case.

Affected lot/batch/SKU.

Impact analysis.

Hold/stop-sale trigger.

Recall status.

Owner decision.

Internal investigation.

Customer/channel impact nếu scope có.

Link đến CAPA nếu có.

Audit/evidence.

## 40.3. Scope Out

Recall không chịu trách nhiệm:

Tự hoàn tiền.

Tự tạo order mới.

Tự viết nội dung marketing.

Tự public toàn bộ hồ sơ điều tra.

Tự sửa tồn kho lịch sử.

Tự xóa trace chain.

## 40.4. Upstream Dependency

Upstream

Điều kiện

OP-M16 Traceability

Impact analysis

OP-M15 Inventory

Stock/balance affected

OP-M20 Sale Lock

Stop-sale enforcement

TECH-01 Foundation

Permission, audit, evidence

Public Policy

Nếu có public communication

## 40.5. Downstream Consumer

Recall cung cấp cho:

Sale Lock / Stop Sale.

Commerce.

AI Advisor.

Ads.

Live.

## CRM.

Public Trace nếu policy có.

MISA nếu scope có.

Owner reporting.

## 40.6. P0 điểm chặn

Recall bị block nếu:

Recall không kích hoạt sale lock khi cần.

Recall active nhưng Commerce vẫn quote/order.

Recall active nhưng AI/Ads/Live vẫn bán.

Recall impact không dựa trace chain.

Recall close thiếu evidence.

Recall close thiếu owner review.

Public trace lộ internal investigation.

Recall bị xóa/sửa lịch sử không audit.

## 40.7. Evidence Required

Recall case record.

Impact analysis evidence.

Sale lock trigger evidence.

Owner decision.

Customer/channel action evidence nếu scope có.

CAPA link nếu scope có.

Close approval evidence.

Audit.

## 40.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M18-SMK-001

Recall batch active

Commerce bị chặn

## OP-M18-SMK-002

Recall SKU active

AI/Ads/Live bị chặn

## OP-M18-SMK-003

Recall impact query

Ra đúng affected chain

## OP-M18-SMK-004

Recall close thiếu evidence

Bị chặn

## OP-M18-SMK-005

Public trace recall case

Chỉ hiển thị theo policy an toàn

## OP-M19 - RECOVERY / CAPA

## 41. OP-M19 - RECOVERY / CAPA CONTRACT

## 41.1. Mục tiêu

Recovery / CAPA quản trị hành động khắc phục và phòng ngừa sau sự cố.

CAPA không được dùng để bỏ qua recall/sale lock nếu chưa có owner approval.

## 41.2. Scope In

Recovery / CAPA bao gồm:

CAPA case.

Root cause record.

Corrective action.

Preventive action.

Evidence.

Responsible owner.

Due date/status.

Effectiveness review.

Link recall/quality incident nếu có.

Close approval.

Audit.

## 41.3. Scope Out

CAPA không chịu trách nhiệm:

Tự mở lại bán hàng.

Tự remove recall.

Tự sửa QC result.

Tự sửa ledger.

Tự public internal investigation.

Tự tạo order/customer message.

## 41.4. Upstream Dependency

Upstream

Điều kiện

OP-M18 Recall

Nếu CAPA từ recall

OP-M12 Batch QC

Nếu CAPA từ QC issue

OP-M03 Incoming QC

Nếu CAPA từ raw material issue

TECH-01 Foundation

Evidence, audit

Owner Policy

Close approval

## 41.5. Downstream Consumer

CAPA cung cấp cho:

Recall close.

Quality improvement.

Owner review.

Release readiness.

Evidence package.

Internal reporting.

## 41.6. P0 điểm chặn

CAPA bị block nếu:

Close không có evidence.

Close không có effectiveness review nếu policy yêu cầu.

CAPA tự remove sale lock.

CAPA public internal investigation.

CAPA không link recall/incident gốc.

CAPA không audit.

## 41.7. Evidence Required

CAPA case.

Root cause evidence.

Action evidence.

Effectiveness review.

Owner close approval.

Audit.

## 41.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M19-SMK-001

CAPA từ recall

Link đúng recall case

## OP-M19-SMK-002

CAPA close thiếu evidence

Bị chặn

## OP-M19-SMK-003

CAPA close hợp lệ

Cho phép owner review recall close

## OP-M19-SMK-004

CAPA tự remove sale lock

Bị chặn

## OP-M20 - SALE LOCK / STOP SALE

## 42. OP-M20 - SALE LOCK / STOP SALE CONTRACT

## 42.1. Mục tiêu

Sale Lock / Stop Sale là lớp chặn bán hàng ở cấp vận hành.

Module này phải thắng Commerce, AI, Ads, Live, CRM và các kênh downstream.

## 42.2. Scope In

Sale Lock / Stop Sale bao gồm:

Lock theo SKU.

Lock theo batch.

Lock theo lot nếu ảnh hưởng.

Lock theo product.

Lock theo channel nếu scope có.

Lock reason.

Lock source.

Lock status.

Effective time.

Owner decision.

Unlock rule.

Audit.

## 42.3. Scope Out

Sale Lock không chịu trách nhiệm:

Tính giá.

Tạo quote.

Tạo order.

Hoàn tiền.

Chạy nội dung CRM.

Tự sửa inventory.

Tự sửa product master.

## 42.4. Upstream Dependency

Upstream

Điều kiện

OP-M18 Recall

Recall trigger

OP-M12 Batch QC

Quality hold

OP-M13 Batch Release

Release issue

OP-M15 Inventory

Stock restriction

Owner Policy

Manual stop-sale

TECH-01 Foundation

Permission, audit

## 42.5. Downstream Consumer

Sale Lock / Stop Sale bắt buộc được dùng bởi:

Commerce Runtime.

AI Advisor.

Facebook Gateway.

Ads Measurement/Activation.

MC AI Live.

## CRM.

IVR nếu liên quan order confirmation.

Public Trace nếu policy có.

## 42.6. P0 điểm chặn

Sale Lock bị block hoặc FAIL nếu:

Lock không chặn quote/order.

Lock không chặn AI chốt sản phẩm.

Lock không chặn Ads/Live campaign.

Recall active nhưng không tạo lock.

Unlock không có permission/owner approval.

Unlock không audit.

Downstream dùng cache cũ để bán sản phẩm đang lock.

Không có smoke chứng minh lock thắng downstream.

## 42.7. Evidence Required

Sale lock record.

Lock reason.

Trigger source.

Owner decision.

Downstream block evidence.

Unlock approval nếu có.

Audit.

## 42.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M20-SMK-001

Sale lock SKU

Commerce không quote/order

## OP-M20-SMK-002

Sale lock batch

Không bán stock từ batch đó

## OP-M20-SMK-003

Sale lock active

AI không chốt sản phẩm

## OP-M20-SMK-004

Sale lock active

Ads/Live bị chặn

## OP-M20-SMK-005

Unlock thiếu approval

Bị chặn

## OP-M21 - OPERATIONAL HANDOFF

## 43. OP-M21 - OPERATIONAL HANDOFF CONTRACT

## 43.1. Mục tiêu

Operational Handoff là lớp bàn giao dữ liệu vận hành đã được kiểm soát sang downstream.

Downstream bao gồm:

Commerce.

AI Advisor.

Facebook Gateway.

Ads.

MC AI Live.

## CRM.

Public Trace.

MISA nếu scope có.

IVR nếu liên quan order confirmation.

## 43.2. Scope In

Operational Handoff bao gồm:

Operational Sellable Signal.

Stock availability safe view.

Sale lock status.

Recall status.

Batch release status.

Warehouse receipt status.

Public trace status.

QR valid status.

Channel scope.

Runtime unavailable status.

Downstream block reason.

## 43.3. Scope Out

Operational Handoff không chịu trách nhiệm:

Tính giá bán.

Chọn chương trình giá.

Tính member discount.

Tạo quote.

Tạo order.

Tạo payment.

Viết câu tư vấn.

Chạy campaign.

Gửi IVR.

## 43.4. Upstream Dependency

Upstream

Điều kiện

OP-M13 Batch Release

Release status

OP-M14 Warehouse Receipt

Finished goods stock

OP-M15 Inventory

Balance khả dụng

OP-M16 Traceability

Trace status

OP-M17 Public Trace

Public-safe trace status

OP-M18 Recall

Recall status

OP-M20 Sale Lock

Sale lock status

TECH-01 Foundation

Audit/evidence where needed

## 43.5. Downstream Consumer

Operational Handoff là nguồn bắt buộc cho:

Downstream

Dữ liệu được dùng

Commerce

Sellable, stock, lock, recall

AI Advisor

Có được tư vấn/chốt sản phẩm hay không

Facebook Gateway

Có được chuyển quote/order hay không

Ads

Có được scale/activate sản phẩm hay không

MC AI Live

Có được nói/chốt sản phẩm live hay không

## CRM

Có được upsell/repurchase sản phẩm hay không

Public Trace

Có được phản hồi trace public hay không

## IVR

Có tiếp tục xác nhận order hay bị chặn do lock/recall

## MISA

Đồng bộ vận hành nếu scope có

## 43.6. P0 điểm chặn

Operational Handoff FAIL nếu:

Commerce bán sản phẩm không pass Operational Sellable Signal.

AI chốt sản phẩm bị lock.

Ads chạy sản phẩm recall.

Live chốt hàng không có stock khả dụng.

CRM upsell sản phẩm stop-sale.

Public Trace dùng dữ liệu ngoài whitelist.

MISA nhận dữ liệu không qua integration boundary nếu scope có.

Runtime unavailable nhưng downstream vẫn xem là pass.

## 43.7. Evidence Required

Handoff contract approval.

Downstream mapping evidence.

Blocking reason mapping.

Sellable signal evidence.

Recall/sale lock propagation evidence.

Runtime unavailable fail-safe evidence.

Owner review.

## 43.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## OP-M21-SMK-001

Sellable pass

Commerce được xét quote

## OP-M21-SMK-002

Sale lock active

Commerce/AI/Ads/Live bị chặn

## OP-M21-SMK-003

Recall active

All sales downstream bị chặn

## OP-M21-SMK-004

Runtime unavailable

Downstream fail-safe

## OP-M21-SMK-005

Public trace policy fail

Không public dữ liệu

## 44. MA TRẬN PHỤ THUỘC MODULE OPERATIONAL CORE

## 44.1. Dependency Matrix cấp module

Module

Phụ thuộc chính

Chặn downstream nếu fail

OP-M01 Source Origin

TECH-02 Ingredient, supplier/source policy

Raw Intake

OP-M02 Raw Intake

Source Origin

Incoming QC, Raw Lot

OP-M03 Incoming QC

Raw Lot

Raw Lot Readiness

OP-M04 Raw Lot Readiness

QC_PASS + source + balance

Material Issue

OP-M05 Production Order

SKU/Recipe/Formula

Material Request, Batch

OP-M06 Formula Snapshot

Approved Recipe

Material Request, Trace

OP-M07 Material Request

PO + Snapshot

Material Issue

OP-M08 Material Issue

Ready lot + balance

Production Process

OP-M09 Material Receipt

Material Issue

Production Execution

OP-M10 Production Process

PO + Receipt

Packaging, Batch QC

OP-M11 Packaging/QR

Batch/process

Public Trace, Warehouse support

OP-M12 Batch QC

Batch

Batch Release

OP-M13 Batch Release

Batch QC_PASS

Warehouse Receipt

OP-M14 Warehouse Receipt

Batch RELEASED

Inventory stock

OP-M15 Inventory Ledger

Issue/Receipt movements

Sellable/stock

OP-M16 Traceability

All operational links

Public Trace, Recall

OP-M17 Public Trace

Trace + whitelist + QR valid

Public-facing trace

OP-M18 Recall

Trace/impact

Sale Lock

## OP-M19 CAPA

Recall/QC incident

Recall close

OP-M20 Sale Lock

Recall/quality/owner lock

Commerce/AI/Ads/Live

OP-M21 Handoff

Release/stock/trace/lock

Downstream runtime

## 45. CROSS-MODULE P0 điểm chặn SUMMARY

## 45.1. P0 điểm chặn theo chuỗi vận hành

Chuỗi

P0 điểm chặn

Module bị ảnh hưởng

Source -> Intake

Source chưa verified

## OP-M02

Intake -> QC

Raw lot không hợp lệ

## OP-M03

QC -> Readiness

QC_PASS bị hiểu là ready

## OP-M04

Readiness -> Issue

Lot chưa ready vẫn issue

## OP-M08

Issue -> Receipt

Issue không idempotent

## OP-M09/OP-M15

Receipt -> Process

Receipt thiếu link issue

## OP-M10

PO -> Snapshot

PO không snapshot formula

## OP-M07/OP-M16

Batch -> QC

Batch không link PO/process

## OP-M12

QC -> Release

QC_PASS tự động release

## OP-M13

Release -> Warehouse

Batch chưa RELEASED nhập kho

## OP-M14

Warehouse -> Inventory

Credit trùng hoặc trước receipt

## OP-M15

Inventory -> Sellable

Stock không lọc lock/recall

## OP-M21

Trace -> Public

Chain thiếu vẫn public

## OP-M17

Recall -> Sale Lock

Recall không chặn downstream

## OP-M20

Sale Lock -> Commerce/AI

Downstream bypass lock

## OP-M21

## 46. EVIDENCE PACKAGE CHO PHẦN 2/4

## 46.1. Evidence cấp module

Mỗi module trong TECH-03 phải có evidence tối thiểu:

Module owner approval.

Scope in/out approval.

Upstream/downstream mapping.

P0 điểm chặn mapping.

Smoke evidence.

Audit/permission evidence nếu module có command rủi ro.

Idempotency evidence nếu module tạo side-effect.

Public/internal boundary evidence nếu module có public output.

Runtime unavailable/fail-safe evidence nếu module phục vụ downstream.

Owner review accepted.

## 46.2. Evidence không được dùng để PASS nếu chưa accepted

Evidence chỉ được dùng cho Completion Decision khi có trạng thái:

## ACCEPTED

Không dùng:

Draft evidence.

Screenshot không reviewer.

Log không environment.

Test chưa pass.

Owner nói miệng.

Dev tự xác nhận.

Evidence không version.

Evidence không trace đến requirement.

## 47. SMOKE MATRIX CHO PHẦN 2/4

## 47.1. Smoke tổng hợp module contract

Smoke ID

Module

Nội dung

Expected Result

## OP-P2-SMK-001

Source Origin

Source chưa verified

Intake bị chặn

## OP-P2-SMK-002

Raw Intake

Intake hợp lệ

Raw lot tạo nhưng chưa ready

## OP-P2-SMK-003

Incoming QC

Raw lot QC_PASS

Chưa ready

## OP-P2-SMK-004

Raw Lot Readiness

Lot ready hợp lệ

Được xét issue

## OP-P2-SMK-005

Production Order

PO không snapshot

Bị chặn

## OP-P2-SMK-006

Formula Snapshot

Recipe đổi sau PO

Snapshot không đổi

## OP-P2-SMK-007

Material Request

Ingredient ngoài snapshot

Bị chặn

## OP-P2-SMK-008

Material Issue

Lot chưa ready

Bị chặn

## OP-P2-SMK-009

Material Issue

Retry issue

Không trừ kho hai lần

## OP-P2-SMK-010

Material Receipt

Receipt sau issue

Không trừ kho lần hai

## OP-P2-SMK-011

Production Process

Batch không link PO

Bị chặn

## OP-P2-SMK-012

Packaging/QR

## QR FAILED/VOID

Không public-valid

## OP-P2-SMK-013

Batch QC

## QC_PASS

Chưa RELEASED

## OP-P2-SMK-014

Batch Release

QC_FAIL release

Bị chặn

## OP-P2-SMK-015

Warehouse Receipt

Batch chưa release

Bị chặn

## OP-P2-SMK-016

Inventory Ledger

Adjustment

Movement mới, không sửa dòng cũ

## OP-P2-SMK-017

Traceability

Missing chain

Trace fail

## OP-P2-SMK-018

Public Trace

Request internal data

Không trả

## OP-P2-SMK-019

Recall

Recall active

Downstream bị chặn

## OP-P2-SMK-020

## CAPA

Close thiếu evidence

Bị chặn

## OP-P2-SMK-021

Sale Lock

Lock active

Commerce/AI/Ads/Live bị chặn

## OP-P2-SMK-022

Handoff

Runtime unavailable

Fail-safe

## 48. DONE GATE CỦA PHẦN 2/4

## 48.1. Điều kiện Done Gate

## PHẦN 2/4 chỉ được xem là DONE khi:

Tất cả module OP-M01 đến OP-M21 đã có contract.

Mỗi module có scope in rõ.

Mỗi module có scope out rõ.

Mỗi module có upstream dependency rõ.

Mỗi module có downstream consumer rõ.

Mỗi module có P0 điểm chặn rõ.

Mỗi module có evidence requirement rõ.

Mỗi module có smoke requirement rõ.

Cross-module dependency matrix đã có.

Cross-module P0 điểm chặn summary đã có.

Không module nào được phép bypass module owner khác.

Không module nào tự mở bán.

Không module nào tự sửa Product Domain.

Không module nào tự thay Commerce.

Public Trace đã tách khỏi Internal Trace.

Recall/Sale Lock đã được khóa là quyền ưu tiên cao nhất với downstream.

Handoff sang Commerce/AI/Ads/Live/CRM/MISA/Public Trace đã có boundary rõ.

Không gọi Documentation Complete là Production Ready.

## 49. FAIL GATE CỦA PHẦN 2/4

## 49.1. Điều kiện làm PHẦN 2 fail

## PHẦN 2/4 FAIL nếu có bất kỳ điểm nào sau:

Module contract thiếu scope out.

Module contract thiếu upstream/downstream.

Module contract không có P0 điểm chặn.

Source Origin bị bypass khi intake.

Raw Intake tạo lot ready ngay.

Incoming QC tự mark ready.

Raw Lot Readiness không kiểm QC/source/balance.

Production Order không snapshot formula.

Formula Snapshot bị ghi đè.

Material Issue không idempotent.

Material Receipt trừ tồn lần hai.

Production Process tạo batch không link PO.

Packaging/QR tự mở bán.

Batch QC_PASS tự release.

Warehouse nhận batch chưa RELEASED.

Inventory Ledger sửa/xóa dòng cũ.

Trace thiếu chain vẫn public.

Public Trace lộ dữ liệu nội bộ.

Recall không chặn downstream.

CAPA tự mở lock.

Sale Lock không thắng Commerce/AI/Ads/Live.

Operational Handoff cho downstream bán khi runtime unavailable.

## 50. TRẠNG THÁI SAU PHẦN 2/4

## 50.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-03 PHẦN 2/4

## WRITTEN_FOR_REVIEW

Operational Module Contracts

## LOCKED_DRAFT

Module Scope In/Out

## DEFINED

Upstream/Downstream Mapping

## DEFINED

P0 điểm chặn Mapping

## DEFINED

Module Evidence Requirement

## DEFINED

Module Smoke Requirement

## DEFINED

Cross-Module Dependency Matrix

## DEFINED

Production Ready

KHONG

Release Ready

KHONG

Go-live Approved

KHONG

## 51. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa contract cho từng module trong Operational Core.

Từ đây trở đi, đội kỹ thuật không được hiểu Operational Core là một module kho đơn giản.

Operational Core là chuỗi module liên kết chặt chẽ:

Source -> Raw Intake -> QC -> Readiness -> Production Order -> Formula Snapshot -> Material Request -> Material Issue -> Material Receipt -> Production Process -> Packaging/QR -> Batch QC -> Batch Release -> Warehouse Receipt -> Inventory Ledger -> Traceability -> Public Trace -> Recall/CAPA -> Sale Lock -> Operational Handoff.

Mỗi module có owner, boundary, dependency, điểm chặn, evidence và smoke riêng.

Không module nào được tự ý thay vai trò của module khác.

Không downstream nào được vượt Operational Core để bán hàng.

## PHẦN 2/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 3/4 - OPERATIONAL LIFECYCLE / STATE MACHINE / COMMAND-QUERY BOUNDARY / INVENTORY-LEDGER-TRACE HANDOFF

## 52. MỤC TIÊU CỦA PHẦN 3/4

## 52.1. Vai trò của PHẦN 3/4

## PHẦN 3/4 khóa cách các module Operational Core vận hành theo vòng đời trạng thái.

PHẦN 2 đã khóa contract từng module.

PHẦN 3 khóa tiếp:

State model của từng đối tượng vận hành chính.

Luồng chuyển trạng thái hợp lệ.

Luồng chuyển trạng thái bị cấm.

Command nào được phép thay đổi trạng thái.

Query nào chỉ được đọc dữ liệu.

Inventory Ledger ghi nhận movement như thế nào.

Trace chain nhận dữ liệu từ đâu.

Handoff sang downstream như Commerce, AI, Ads, Live, CRM, Public Trace, MISA.

Runtime unavailable thì chặn ở đâu.

Khi nào phải fail-safe.

Khi nào phải block release.

## 52.2. Nguyên tắc quan trọng

Trong TECH-03, state machine không phải phần phụ.

State machine là lõi kiểm soát để tránh:

Nhảy cóc trạng thái.

Bán hàng chưa release.

Trừ kho hai lần.

Public trace sai.

AI/Commerce/Ads/Live vượt sale lock.

Dữ liệu tồn kho bị sửa tay.

Recall không chặn được downstream.

MISA nhận dữ liệu chưa đủ điều kiện.

## 53. NGUYÊN TẮC STATE MACHINE CỦA OPERATIONAL CORE

## 53.1. Mọi trạng thái phải có owner

Mỗi state phải có owner module rõ ràng.

Không state nào được nhiều module cùng ghi không kiểm soát.

Ví dụ:

State

Owner đúng

## RAW_LOT_QC_PASS

Incoming QC

## READY_FOR_PRODUCTION

Raw Lot Readiness

## PRODUCTION_ORDER_APPROVED

Production Order

## MATERIAL_ISSUED

Material Issue

## MATERIAL_RECEIVED

Material Receipt

## BATCH_QC_PASS

Batch QC

## BATCH_RELEASED

Batch Release

## WAREHOUSE_RECEIPT_CONFIRMED

Warehouse Receipt

## LEDGER_POSTED

Inventory Ledger

## PUBLIC_TRACE_VALID

Public Trace Policy + Trace Chain

## RECALL_ACTIVE

Recall

## SALE_LOCK_ACTIVE

Sale Lock

## 53.2. Không module nào được tự ghi state của module khác

Quy tắc:

QC không được tự mark ready.

Readiness không được tự issue.

Issue không được tự receipt.

Receipt không được tự complete production.

Production không được tự QC_PASS.

QC_PASS không được tự release.

Release không được tự warehouse receipt.

Warehouse receipt không được tự quote/order.

Trace không được tự public toàn bộ dữ liệu.

CAPA không được tự unlock sale lock.

Commerce không được tự bỏ recall/sale lock.

## 53.3. State transition phải có reason, actor, timestamp và audit

Mọi command làm thay đổi state phải ghi nhận tối thiểu:

State trước.

State sau.

Actor.

Permission context.

Timestamp.

Reason nếu là hold/reject/recall/lock/unlock/adjustment.

Source document.

Correlation reference nếu có.

Evidence nếu state ảnh hưởng release hoặc production gate.

## 53.4. State không được sửa ngược lịch sử

Không được sửa trực tiếp state lịch sử để làm dữ liệu “đẹp”.

Nếu cần điều chỉnh, phải dùng command điều chỉnh hợp lệ:

Void.

Cancel.

Supersede.

Adjustment movement.

Reopen with reason nếu policy cho phép.

New review decision.

New evidence record.

Owner approval.

Không được xóa dấu vết vận hành.

## 54. OPERATIONAL LIFECYCLE TỔNG THỂ

## 54.1. Chuỗi lifecycle chuẩn

Lifecycle tổng thể của Operational Core:

SOURCE_VERIFIED -> RAW_LOT_CREATED -> INCOMING_QC_WAITING -> RAW_LOT_QC_PASS -> READY_FOR_PRODUCTION -> PRODUCTION_ORDER_CREATED -> FORMULA_SNAPSHOT_LOCKED -> MATERIAL_REQUESTED -> MATERIAL_ISSUED -> MATERIAL_RECEIVED -> PRODUCTION_IN_PROGRESS -> PRODUCTION_COMPLETED -> PACKAGING_COMPLETED -> BATCH_QC_PASS -> BATCH_RELEASED -> WAREHOUSE_RECEIPT_CONFIRMED -> LEDGER_CREDIT_POSTED -> TRACE_CHAIN_COMPLETED -> OPERATIONAL_SELLABLE_PASS -> COMMERCE_SELLABLE_EVALUATION.

Lifecycle này có thể bị chặn bởi:

## HOLD.

## REJECT.

## CANCEL.

## QC_FAIL.

## RECALL_ACTIVE.

## SALE_LOCK_ACTIVE.

## QUALITY_LOCK.

## TRACE_MISSING.

## PUBLIC_TRACE_BLOCKED.

## RUNTIME_UNAVAILABLE.

## OWNER_REVIEW_REQUIRED.

## EVIDENCE_NOT_ACCEPTED.

## 54.2. Lifecycle không đồng nghĩa mở bán

Ngay cả khi Operational lifecycle đã pass, việc bán hàng vẫn phải qua TECH-04 Commerce.

Operational Core chỉ trả lời:

Có đủ điều kiện vận hành để xét bán hay không.

Commerce mới trả lời:

Có đủ điều kiện thương mại để quote/order hay không.

## 55. QUY ƯỚC STATE MODEL TRONG TECH-03

## 55.1. Nhóm trạng thái chuẩn

Mỗi object trong Operational Core nên dùng nhóm state rõ:

Nhóm state

Ý nghĩa

## DRAFT

Mới tạo, chưa đủ điều kiện xử lý

## WAITING_REVIEW

Chờ kiểm tra / chờ duyệt

## APPROVED

Đã được duyệt theo scope

## IN_PROGRESS

Đang thực hiện

## PASSED

Đạt một kiểm tra cụ thể

## FAILED

Không đạt

## HOLD

Tạm giữ, không được đi tiếp

## READY

Sẵn sàng cho bước tiếp theo

## COMPLETED

Hoàn tất quy trình tương ứng

## RELEASED

Được release chính thức

## CONFIRMED

Xác nhận chính thức

## CANCELLED

Hủy hợp lệ

## VOID

Vô hiệu hóa hợp lệ

## LOCKED

Bị khóa

## RECALLED

Thu hồi

Đã đóng sau review

## 55.2. State có tên giống nhau nhưng không đồng nghĩa

Các state có thể cùng chữ “PASS” nhưng ý nghĩa khác nhau.

Ví dụ:

State

Không đồng nghĩa

## RAW_LOT_QC_PASS

Không đồng nghĩa READY_FOR_PRODUCTION

## BATCH_QC_PASS

Không đồng nghĩa BATCH_RELEASED

## PRODUCT_ACTIVE

Không đồng nghĩa SELLABLE

## SKU_ACTIVE

Không đồng nghĩa SELLABLE

## QR_PRINTED

Không đồng nghĩa PUBLIC_TRACE_VALID

## WAREHOUSE_RECEIPT_CONFIRMED

Không đồng nghĩa COMMERCE_ORDERABLE

## TRACE_CHAIN_COMPLETED

Không đồng nghĩa PUBLIC_TRACE_PUBLICABLE nếu whitelist chưa pass

## 56. RAW LOT STATE MODEL

## 56.1. Mục tiêu

Raw Lot State Model kiểm soát vòng đời nguyên liệu từ lúc được tạo sau intake đến khi đủ điều kiện xuất cho sản xuất.

## 56.2. Trạng thái raw lot chuẩn

State

Ý nghĩa

## RAW_LOT_CREATED

Raw lot được tạo sau intake

## INCOMING_QC_WAITING

Chờ QC đầu vào

## INCOMING_QC_IN_PROGRESS

Đang QC

## RAW_LOT_QC_PASS

QC đầu vào đạt

## RAW_LOT_QC_FAIL

QC đầu vào không đạt

## RAW_LOT_HOLD

Tạm giữ

## RAW_LOT_REJECTED

Loại bỏ

## READY_REVIEW_WAITING

Chờ xét sẵn sàng sản xuất

## READY_FOR_PRODUCTION

Đủ điều kiện xuất cho sản xuất

## RAW_LOT_PARTIALLY_ISSUED

Đã xuất một phần

## RAW_LOT_FULLY_ISSUED

Đã xuất hết

## RAW_LOT_EXPIRED

Hết hạn dùng nội bộ nếu policy có

## RAW_LOT_RECALLED

Bị recall/quality restriction

## RAW_LOT_CLOSED

Đóng vòng đời lot

## 56.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## RAW_LOT_CREATED

## INCOMING_QC_WAITING

Intake hợp lệ

## INCOMING_QC_WAITING

## INCOMING_QC_IN_PROGRESS

QC bắt đầu

## INCOMING_QC_IN_PROGRESS

## RAW_LOT_QC_PASS

QC đạt

## INCOMING_QC_IN_PROGRESS

## RAW_LOT_QC_FAIL

QC không đạt

## RAW_LOT_QC_PASS

## READY_REVIEW_WAITING

Có yêu cầu xét readiness

## READY_REVIEW_WAITING

## READY_FOR_PRODUCTION

Source pass, QC pass, không hold, balance đủ

## READY_FOR_PRODUCTION

## RAW_LOT_PARTIALLY_ISSUED

Material Issue một phần

## READY_FOR_PRODUCTION

## RAW_LOT_FULLY_ISSUED

Material Issue hết lot

## RAW_LOT_PARTIALLY_ISSUED

## RAW_LOT_FULLY_ISSUED

Issue hết balance khả dụng

Bất kỳ active state

## RAW_LOT_HOLD

Quality/owner hold

## RAW_LOT_HOLD

## READY_REVIEW_WAITING

Hold được giải quyết có evidence

Bất kỳ active state

## RAW_LOT_RECALLED

Recall/quality restriction

## RAW_LOT_QC_FAIL

## RAW_LOT_REJECTED

Reject decision

## RAW_LOT_REJECTED

## RAW_LOT_CLOSED

Close lot

## 56.4. Chuyển trạng thái bị cấm

Không được:

## RAW_LOT_CREATED -> READY_FOR_PRODUCTION.

RAW_LOT_QC_PASS -> MATERIAL_ISSUED nếu chưa READY_FOR_PRODUCTION.

## RAW_LOT_QC_FAIL -> READY_FOR_PRODUCTION.

## RAW_LOT_HOLD -> MATERIAL_ISSUED.

## RAW_LOT_REJECTED -> MATERIAL_ISSUED.

## RAW_LOT_RECALLED -> MATERIAL_ISSUED.

READY_FOR_PRODUCTION -> QC result mới mà không có reopen governance.

RAW_LOT_FULLY_ISSUED -> MATERIAL_ISSUED thêm.

## 56.5. P0 điểm chặn

Raw Lot lifecycle FAIL nếu:

QC_PASS tự động thành READY_FOR_PRODUCTION.

HOLD/REJECT/RECALL vẫn issue được.

Không có audit khi mark ready.

Lot balance âm.

Issue vượt balance.

Raw lot chưa verified source vẫn ready được.

## 56.6. Evidence bắt buộc

Intake evidence.

Source verification.

Incoming QC evidence.

Readiness approval.

Balance check.

Hold/release reason nếu có.

Audit state transition.

## 57. PRODUCTION ORDER STATE MODEL

## 57.1. Mục tiêu

Production Order State Model kiểm soát lệnh sản xuất từ lúc tạo đến khi hoàn tất hoặc hủy.

## 57.2. Trạng thái Production Order chuẩn

State

Ý nghĩa

## PO_DRAFT

Lệnh nháp

## PO_WAITING_REVIEW

Chờ review

## PO_APPROVED

Được duyệt

## FORMULA_SNAPSHOT_LOCKED

Công thức đã snapshot

## MATERIAL_REQUEST_WAITING

Chờ tạo yêu cầu cấp nguyên liệu

## MATERIAL_REQUESTED

Đã yêu cầu cấp nguyên liệu

## MATERIAL_ISSUE_IN_PROGRESS

Đang xuất nguyên liệu

## MATERIAL_RECEIPT_WAITING

Chờ xưởng nhận

## MATERIAL_RECEIVED

Xưởng đã nhận nguyên liệu

## PRODUCTION_IN_PROGRESS

Đang sản xuất

## PRODUCTION_COMPLETED

Hoàn tất sản xuất

## PO_HOLD

Tạm dừng

## PO_CANCELLED

Hủy

## PO_CLOSED

Đóng lệnh

## 57.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PO_DRAFT

## PO_WAITING_REVIEW

Đủ thông tin cơ bản

## PO_WAITING_REVIEW

## PO_APPROVED

Owner/role duyệt

## PO_APPROVED

## FORMULA_SNAPSHOT_LOCKED

Snapshot hợp lệ

## FORMULA_SNAPSHOT_LOCKED

## MATERIAL_REQUEST_WAITING

Sẵn sàng yêu cầu nguyên liệu

## MATERIAL_REQUEST_WAITING

## MATERIAL_REQUESTED

Material Request hợp lệ

## MATERIAL_REQUESTED

## MATERIAL_ISSUE_IN_PROGRESS

Bắt đầu issue

## MATERIAL_ISSUE_IN_PROGRESS

## MATERIAL_RECEIPT_WAITING

Issue hoàn tất theo scope

## MATERIAL_RECEIPT_WAITING

## MATERIAL_RECEIVED

Xưởng xác nhận nhận nguyên liệu

## MATERIAL_RECEIVED

## PRODUCTION_IN_PROGRESS

Bắt đầu sản xuất

## PRODUCTION_IN_PROGRESS

## PRODUCTION_COMPLETED

Hoàn tất production execution

## PRODUCTION_COMPLETED

## PO_CLOSED

Batch flow đã hoàn tất theo policy

Bất kỳ active state

## PO_HOLD

Owner/quality hold

## PO_HOLD

State trước đó hoặc review state

Có reason/evidence gỡ hold

## PO_DRAFT/PO_WAITING_REVIEW

## PO_CANCELLED

Hủy trước production

## PO_CANCELLED

## PO_CLOSED

Đóng hồ sơ

## 57.4. Chuyển trạng thái bị cấm

Không được:

## PO_DRAFT -> MATERIAL_REQUESTED.

PO_APPROVED nhưng không snapshot mà vẫn material request.

FORMULA_SNAPSHOT_LOCKED bị ghi đè sau khi issue.

MATERIAL_REQUESTED -> PRODUCTION_IN_PROGRESS nếu chưa issue/receipt.

PO_HOLD -> tiếp tục issue/production.

PO_CANCELLED -> issue/production.

PRODUCTION_COMPLETED -> tự tăng tồn thành phẩm.

## 57.5. P0 điểm chặn

Production Order lifecycle FAIL nếu:

Không có formula snapshot.

Snapshot thay đổi theo live recipe.

PO dùng SKU/recipe chưa approved.

PO đang hold vẫn sản xuất.

PO cancelled vẫn issue được.

Production completed tự làm finished goods stock tăng.

## 57.6. Evidence bắt buộc

PO approval.

Formula snapshot.

Material request link.

Material issue link.

Material receipt link.

Production process record.

Hold/cancel evidence nếu có.

Audit.

## 58. FORMULA SNAPSHOT STATE MODEL

## 58.1. Mục tiêu

Formula Snapshot State Model đảm bảo công thức sản xuất của một Production Order là bất biến sau khi được khóa.

## 58.2. Trạng thái Formula Snapshot chuẩn

State

Ý nghĩa

## SNAPSHOT_WAITING

Chưa snapshot

## SNAPSHOT_CREATED

Snapshot được tạo

## SNAPSHOT_VALIDATED

Snapshot kiểm tra đủ field

## SNAPSHOT_LOCKED

Snapshot đã khóa

## SNAPSHOT_SUPERSEDED

Được thay thế theo governance trước khi issue

## SNAPSHOT_VOID

Vô hiệu hóa do PO hủy hoặc lỗi hợp lệ

## 58.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## SNAPSHOT_WAITING

## SNAPSHOT_CREATED

PO approved, recipe approved

## SNAPSHOT_CREATED

## SNAPSHOT_VALIDATED

Đủ formula kind/version/ingredient/quantity/UOM/group

## SNAPSHOT_VALIDATED

## SNAPSHOT_LOCKED

Owner/system lock

## SNAPSHOT_CREATED/VALIDATED

## SNAPSHOT_VOID

PO hủy trước khi lock

## SNAPSHOT_LOCKED

## SNAPSHOT_SUPERSEDED

Chỉ khi chưa issue và có change governance

## SNAPSHOT_SUPERSEDED

## SNAPSHOT_LOCKED

Snapshot mới được khóa

## 58.4. Chuyển trạng thái bị cấm

Không được:

SNAPSHOT_LOCKED bị sửa trực tiếp.

SNAPSHOT_LOCKED thay đổi sau Material Issue.

Snapshot thiếu 4 group vẫn lock.

Snapshot không có formula version vẫn production.

Snapshot bị xóa để tạo lại không audit.

## 58.5. P0 điểm chặn

Formula Snapshot FAIL nếu:

Không bất biến.

Thiếu ingredient/quantity/UOM.

Thiếu formula kind/version.

Không trace được recipe approved.

Recipe live change làm PO history đổi.

## 59. MATERIAL MOVEMENT STATE MODEL

## 59.1. Mục tiêu

Material Movement State Model kiểm soát yêu cầu, xuất và nhận nguyên liệu.

Material Movement gồm 3 lớp:

Material Request.

Material Issue.

Material Receipt.

## 59.2. Material Request states

State

Ý nghĩa

## REQUEST_DRAFT

Yêu cầu nháp

## REQUEST_WAITING_REVIEW

Chờ duyệt

## REQUEST_APPROVED

Được duyệt

## REQUEST_PARTIALLY_FULFILLED

Cấp một phần

## REQUEST_FULFILLED

Cấp đủ

## REQUEST_CANCELLED

Hủy

## REQUEST_HOLD

Tạm giữ

## 59.3. Material Issue states

State

Ý nghĩa

## ISSUE_WAITING

Chờ xuất

## ISSUE_VALIDATED

Đã kiểm tra lot/balance/readiness

## ISSUE_POSTED

Đã ghi xuất và tạo ledger debit

## ISSUE_PARTIAL

Xuất một phần

## ISSUE_VOID

Vô hiệu hóa theo policy

## ISSUE_FAILED

Xuất thất bại

## ISSUE_CANCELLED

Hủy trước khi posted

## 59.4. Material Receipt states

State

Ý nghĩa

## RECEIPT_WAITING

Chờ xưởng nhận

## RECEIPT_CONFIRMED

Xưởng xác nhận đã nhận

## RECEIPT_VARIANCE_RECORDED

Có chênh lệch được ghi nhận

## RECEIPT_REJECTED

Xưởng từ chối nhận theo policy

## RECEIPT_CLOSED

Đóng nhận nguyên liệu

## 59.5. Chuyển trạng thái hợp lệ

Flow

Điều kiện

## REQUEST_APPROVED -> ISSUE_VALIDATED

Có raw lot READY_FOR_PRODUCTION và balance đủ

## ISSUE_VALIDATED -> ISSUE_POSTED

Command idempotent, ledger debit tạo đúng

## ISSUE_POSTED -> RECEIPT_WAITING

Chờ xưởng nhận

## RECEIPT_WAITING -> RECEIPT_CONFIRMED

Xưởng xác nhận

## RECEIPT_CONFIRMED -> RECEIPT_VARIANCE_RECORDED

Có chênh lệch cần ghi nhận

## REQUEST_PARTIALLY_FULFILLED -> REQUEST_FULFILLED

Đủ số lượng theo request

## 59.6. Chuyển trạng thái bị cấm

Không được:

Request ingredient ngoài formula snapshot.

Issue raw lot chưa READY_FOR_PRODUCTION.

Issue không có balance.

Issue không ledger debit.

Retry issue tạo debit trùng.

Receipt trừ tồn lần hai.

Receipt sửa ledger debit cũ.

Receipt không có issue.

Receipt từ issue failed/void.

## 59.7. P0 điểm chặn

Material Movement FAIL nếu:

Issue không idempotent.

Material Receipt giảm tồn lần hai.

Request vượt snapshot không approval.

Issue lot HOLD/REJECT/RECALL.

Ledger debit không link issue.

Receipt variance không có reason.

## 60. PRODUCTION PROCESS STATE MODEL

## 60.1. Mục tiêu

Production Process State Model kiểm soát batch trong quá trình sản xuất.

## 60.2. Trạng thái production process chuẩn

State

Ý nghĩa

## PROCESS_NOT_STARTED

Chưa bắt đầu

## PROCESS_READY

Đủ điều kiện bắt đầu

## PROCESS_IN_PROGRESS

Đang sản xuất

## PROCESS_PAUSED

Tạm dừng

## PROCESS_HOLD

Bị hold

## PROCESS_COMPLETED

Hoàn tất sản xuất

## PROCESS_FAILED

Sản xuất lỗi

## PROCESS_CANCELLED

Hủy

## PROCESS_CLOSED

Đóng quy trình

## 60.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PROCESS_NOT_STARTED

## PROCESS_READY

PO approved, material receipt confirmed

## PROCESS_READY

## PROCESS_IN_PROGRESS

Bắt đầu sản xuất

## PROCESS_IN_PROGRESS

## PROCESS_PAUSED

Tạm dừng hợp lệ

## PROCESS_PAUSED

## PROCESS_IN_PROGRESS

Tiếp tục có reason

## PROCESS_IN_PROGRESS

## PROCESS_COMPLETED

Hoàn tất các bước

## PROCESS_IN_PROGRESS

## PROCESS_HOLD

Quality/owner hold

## PROCESS_HOLD

## PROCESS_IN_PROGRESS

Gỡ hold có evidence

## PROCESS_IN_PROGRESS

## PROCESS_FAILED

Lỗi không thể tiếp tục

## PROCESS_COMPLETED

## PROCESS_CLOSED

Đóng sau batch flow

## 60.4. Chuyển trạng thái bị cấm

Không được:

## PROCESS_NOT_STARTED -> PROCESS_COMPLETED.

PROCESS_READY nếu chưa receipt nguyên liệu.

PROCESS_HOLD -> Batch QC/Release.

PROCESS_FAILED -> Release.

PROCESS_COMPLETED -> Warehouse stock tăng trực tiếp.

Process tạo batch không link PO.

## 60.5. P0 điểm chặn

Production Process FAIL nếu:

Batch không link Production Order.

Process chạy khi material chưa nhận.

Process hold vẫn đi QC/release.

Process complete tự tăng tồn.

Process không audit.

## 61. PACKAGING / QR STATE MODEL

## 61.1. Mục tiêu

Packaging / QR State Model kiểm soát đóng gói, in ấn và vòng đời QR.

## 61.2. Packaging states

State

Ý nghĩa

## PACKAGING_NOT_STARTED

Chưa đóng gói

## PACKAGING_READY

Đủ điều kiện đóng gói

## PACKAGING_IN_PROGRESS

Đang đóng gói

## PACKAGING_COMPLETED

Hoàn tất đóng gói

## PACKAGING_HOLD

Tạm giữ

## PACKAGING_FAILED

Lỗi đóng gói

## PACKAGING_CLOSED

Đóng job

## 61.3. QR states

State

Ý nghĩa

## QR_NOT_REQUIRED

Scope không yêu cầu QR

## QR_WAITING

Chờ tạo

## QR_GENERATED

QR đã tạo

## QR_PRINT_QUEUED

Đưa vào hàng đợi in

## QR_PRINTED

Đã in

## QR_PRINT_FAILED

In thất bại

## QR_VOID

QR vô hiệu hóa

## QR_REPRINT_REQUESTED

Yêu cầu in lại

## QR_REPRINTED

Đã in lại

## QR_PUBLIC_VALID

Đủ điều kiện public trace theo policy

## QR_PUBLIC_BLOCKED

Bị chặn public trace

## 61.4. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PACKAGING_READY

## PACKAGING_IN_PROGRESS

Batch/process đủ điều kiện

## PACKAGING_IN_PROGRESS

## PACKAGING_COMPLETED

Đóng gói hoàn tất

## QR_WAITING

## QR_GENERATED

Có batch/package link

## QR_GENERATED

## QR_PRINT_QUEUED

Đưa in

## QR_PRINT_QUEUED

## QR_PRINTED

In thành công

## QR_PRINT_QUEUED

## QR_PRINT_FAILED

In lỗi

## QR_PRINTED

## QR_REPRINT_REQUESTED

Có reason

## QR_REPRINT_REQUESTED

## QR_REPRINTED

In lại thành công

## QR_PRINTED/QR_REPRINTED

## QR_PUBLIC_VALID

Trace chain đủ, whitelist pass, QR còn valid

## QR_PRINT_FAILED

## QR_PUBLIC_BLOCKED

Không public-valid

## QR_VOID

## QR_PUBLIC_BLOCKED

Không public-valid

## 61.5. Chuyển trạng thái bị cấm

Không được:

QR_GENERATED -> QR_PUBLIC_VALID nếu chưa có trace chain.

## QR_PRINT_FAILED -> QR_PUBLIC_VALID.

## QR_VOID -> QR_PUBLIC_VALID.

PACKET QR public trace standalone nếu rule packaging đang cấm.

QR_PUBLIC_VALID vượt qua Batch Release/Recall/Sale Lock policy.

QR reprint không reason/audit.

QR link sai batch/package.

## 61.6. P0 điểm chặn

Packaging/QR FAIL nếu:

QR VOID/FAILED vẫn public-valid.

QR không link batch.

In sai lot/NSX/HSD.

QR public trace ngoài whitelist.

QR được dùng như điều kiện sellable thay Batch Release/Warehouse Receipt.

## 62. BATCH STATE MODEL

## 62.1. Mục tiêu

Batch State Model kiểm soát thành phẩm từ production đến QC, release và warehouse.

## 62.2. Trạng thái batch chuẩn

State

Ý nghĩa

## BATCH_CREATED

Batch được tạo từ Production Order

## BATCH_IN_PRODUCTION

Đang sản xuất

## BATCH_PRODUCTION_COMPLETED

Sản xuất hoàn tất

## BATCH_PACKAGING_WAITING

Chờ đóng gói nếu scope có

## BATCH_PACKAGED

Đã đóng gói

## BATCH_QC_WAITING

Chờ QC thành phẩm

## BATCH_QC_IN_PROGRESS

Đang QC

## BATCH_QC_PASS

QC thành phẩm đạt

## BATCH_QC_FAIL

QC thành phẩm không đạt

## BATCH_HOLD

Tạm giữ

## BATCH_REJECTED

Loại bỏ

## BATCH_RELEASE_WAITING

Chờ release

## BATCH_RELEASED

Được release chính thức

## BATCH_WAREHOUSE_RECEIPT_WAITING

Chờ kho nhận

## BATCH_RECEIVED_BY_WAREHOUSE

Kho đã nhận

## BATCH_RECALLED

Bị recall

## BATCH_CLOSED

Đóng vòng đời batch

## 62.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## BATCH_CREATED

## BATCH_IN_PRODUCTION

Process bắt đầu

## BATCH_IN_PRODUCTION

## BATCH_PRODUCTION_COMPLETED

Process completed

## BATCH_PRODUCTION_COMPLETED

## BATCH_PACKAGING_WAITING

Packaging scope có

## BATCH_PACKAGING_WAITING

## BATCH_PACKAGED

Packaging completed

## BATCH_PRODUCTION_COMPLETED/BATCH_PACKAGED

## BATCH_QC_WAITING

Đủ điều kiện QC

## BATCH_QC_WAITING

## BATCH_QC_IN_PROGRESS

QC bắt đầu

## BATCH_QC_IN_PROGRESS

## BATCH_QC_PASS

QC đạt

## BATCH_QC_IN_PROGRESS

## BATCH_QC_FAIL

QC không đạt

## BATCH_QC_PASS

## BATCH_RELEASE_WAITING

Chờ release

## BATCH_RELEASE_WAITING

## BATCH_RELEASED

Release approved

## BATCH_RELEASED

## BATCH_WAREHOUSE_RECEIPT_WAITING

Chờ kho nhận

## BATCH_WAREHOUSE_RECEIPT_WAITING

## BATCH_RECEIVED_BY_WAREHOUSE

Warehouse receipt confirmed

Bất kỳ active state

## BATCH_HOLD

Quality/owner hold

Bất kỳ active state

## BATCH_RECALLED

Recall active

## BATCH_QC_FAIL

## BATCH_REJECTED

Reject decision

## BATCH_RECEIVED_BY_WAREHOUSE

## BATCH_CLOSED

Close theo policy

## 62.4. Chuyển trạng thái bị cấm

Không được:

## BATCH_CREATED -> BATCH_RELEASED.

BATCH_QC_PASS -> BATCH_RECEIVED_BY_WAREHOUSE nếu chưa BATCH_RELEASED.

BATCH_QC_PASS -> Sellable.

BATCH_RELEASED -> Finished goods stock nếu chưa warehouse receipt.

BATCH_HOLD -> Warehouse Receipt.

BATCH_RECALLED -> Sellable.

BATCH_QC_FAIL -> Release.

BATCH_REJECTED -> Warehouse Receipt.

## 62.5. P0 điểm chặn

Batch lifecycle FAIL nếu:

QC_PASS bị hiểu là RELEASED.

Batch chưa RELEASED được warehouse nhận.

Batch HOLD/RECALL vẫn sellable.

Batch không link PO/process.

Batch release không audit/evidence.

Warehouse receipt thiếu batch release reference.

## 63. WAREHOUSE RECEIPT STATE MODEL

## 63.1. Mục tiêu

Warehouse Receipt State Model kiểm soát điểm tăng tồn thành phẩm.

## 63.2. Trạng thái warehouse receipt chuẩn

State

Ý nghĩa

## WH_RECEIPT_DRAFT

Phiếu nhận kho nháp

## WH_RECEIPT_WAITING

Chờ nhận

## WH_RECEIPT_VALIDATED

Đã kiểm batch RELEASED và warehouse/location

## WH_RECEIPT_CONFIRMED

Kho xác nhận nhận

## WH_RECEIPT_VARIANCE_RECORDED

Có chênh lệch

## WH_RECEIPT_POSTED

Ledger credit đã ghi

## WH_RECEIPT_VOID

Vô hiệu hóa theo policy

## WH_RECEIPT_CLOSED

Đóng phiếu

## 63.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## WH_RECEIPT_DRAFT

## WH_RECEIPT_WAITING

Đủ dữ liệu

## WH_RECEIPT_WAITING

## WH_RECEIPT_VALIDATED

Batch RELEASED, warehouse hợp lệ

## WH_RECEIPT_VALIDATED

## WH_RECEIPT_CONFIRMED

Kho xác nhận

## WH_RECEIPT_CONFIRMED

## WH_RECEIPT_POSTED

Ledger credit tạo đúng

## WH_RECEIPT_CONFIRMED

## WH_RECEIPT_VARIANCE_RECORDED

Có chênh lệch

## WH_RECEIPT_POSTED

## WH_RECEIPT_CLOSED

Đóng receipt

## 63.4. Chuyển trạng thái bị cấm

Không được:

WH_RECEIPT_WAITING -> CONFIRMED nếu batch chưa RELEASED.

WH_RECEIPT_CONFIRMED tạo nhiều ledger credit do retry.

WH_RECEIPT_POSTED sửa dòng credit cũ.

WH_RECEIPT_VOID xóa ledger cũ.

Warehouse receipt cho batch QC_PASS nhưng chưa release.

Warehouse receipt cho batch recall/hold/reject.

## 63.5. P0 điểm chặn

Warehouse Receipt FAIL nếu:

Batch chưa RELEASED vẫn nhận.

Finished goods stock tăng trước receipt confirmed.

Retry receipt làm tăng tồn hai lần.

Receipt không audit.

Warehouse/location không hợp lệ.

Ledger credit không link receipt.

## 64. INVENTORY LEDGER STATE MODEL

## 64.1. Mục tiêu

Inventory Ledger State Model kiểm soát sổ cái tồn kho theo nguyên tắc append-only.

## 64.2. Trạng thái ledger movement chuẩn

State

Ý nghĩa

## LEDGER_MOVEMENT_WAITING

Movement đang chờ ghi

## LEDGER_MOVEMENT_VALIDATED

Đã validate source document

## LEDGER_MOVEMENT_POSTED

Đã ghi ledger

## LEDGER_MOVEMENT_REVERSED

Có movement đảo hợp lệ

## LEDGER_MOVEMENT_ADJUSTED

Có adjustment movement mới

## LEDGER_MOVEMENT_VOIDED

Void theo policy, không xóa lịch sử

## LEDGER_MOVEMENT_BLOCKED

Bị chặn do source invalid

## 64.3. Movement type chuẩn

Movement Type

Nguồn

## RAW_MATERIAL_IN

Raw Material Intake nếu policy ghi tồn nguyên liệu tại intake

## RAW_MATERIAL_ISSUE

Material Issue

## RAW_MATERIAL_ADJUSTMENT

Adjustment có approval

## FINISHED_GOODS_RECEIPT

Warehouse Receipt

## FINISHED_GOODS_HOLD

Hold/lock allocation nếu policy có

## FINISHED_GOODS_RELEASE_FROM_HOLD

Gỡ hold theo policy

## FINISHED_GOODS_ADJUSTMENT

Adjustment có approval

## FINISHED_GOODS_SALE_ALLOCATION

Commerce/Fulfillment nếu TECH-04 scope có

## FINISHED_GOODS_SALE_DEBIT

Commerce/Fulfillment nếu TECH-04 scope có

## RECALL_HOLD

Recall/Sale Lock

## WRITE_OFF

Write-off có owner approval

## 64.4. Quy tắc ledger

Inventory Ledger bắt buộc:

Append-only.

Mỗi movement có source document.

Mỗi movement có reason nếu adjustment/reversal/write-off.

Mỗi movement có idempotency reference nếu đến từ command side-effect.

Balance chỉ là projection.

Không sửa balance trực tiếp.

Không xóa movement cũ.

Không cho downstream đọc stock nếu projection chưa nhất quán.

## 64.5. Chuyển trạng thái bị cấm

Không được:

POSTED movement bị sửa quantity.

POSTED movement bị xóa.

Balance thay đổi không có movement.

Material Receipt tạo debit lần hai.

Warehouse Receipt retry tạo credit trùng.

Adjustment không reason/approval.

Recall hold không ảnh hưởng available stock nếu policy yêu cầu chặn bán.

## 64.6. P0 điểm chặn

Inventory Ledger FAIL nếu:

Không append-only.

Retry không idempotent.

Có thể sửa/xóa lịch sử.

Balance âm không policy.

Available stock không trừ hold/recall/sale lock.

Commerce/AI dùng stock không qua Operational Sellable Signal.

## 65. TRACEABILITY STATE MODEL

## 65.1. Mục tiêu

Traceability State Model kiểm soát chuỗi truy xuất nội bộ từ Product/SKU đến nguyên liệu, batch, warehouse, QR và recall.

## 65.2. Trạng thái trace chain chuẩn

State

Ý nghĩa

## TRACE_NOT_STARTED

Chưa hình thành chain

## TRACE_PARTIAL

Có một phần link

## TRACE_WAITING_COMPLETION

Chờ hoàn tất link

## TRACE_CHAIN_COMPLETED

Chain nội bộ đủ theo scope

## TRACE_CHAIN_VALIDATED

Chain đã validate

## TRACE_CHAIN_BROKEN

Chain thiếu hoặc lỗi

## TRACE_CHAIN_LOCKED

Chain đã khóa cho release/evidence

## TRACE_RECALL_LINKED

Chain có recall/impact

## TRACE_CLOSED

Đóng trace lifecycle

## 65.3. Node trace bắt buộc theo scope

Trace chain phải có các node sau nếu scope tương ứng được bật:

Node

Nguồn owner

Product/SKU

TECH-02 Product

Recipe/Formula Snapshot

## TECH-02 + OP-M06

Source Origin

## OP-M01

Raw Lot

## OP-M02/OP-M03/OP-M04

Material Issue

## OP-M08

Material Receipt

## OP-M09

Production Order

## OP-M05

Production Process

## OP-M10

Batch

## OP-M10/OP-M12/OP-M13

Packaging/QR

## OP-M11

Warehouse Receipt

## OP-M14

Inventory Ledger

## OP-M15

Recall/Sale Lock

## OP-M18/OP-M20

## 65.4. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## TRACE_NOT_STARTED

## TRACE_PARTIAL

Có Product/SKU/PO hoặc batch link

## TRACE_PARTIAL

## TRACE_WAITING_COMPLETION

Đang chờ thêm raw lot/material/warehouse link

## TRACE_WAITING_COMPLETION

## TRACE_CHAIN_COMPLETED

Đủ node theo scope

## TRACE_CHAIN_COMPLETED

## TRACE_CHAIN_VALIDATED

Validate link pass

## TRACE_CHAIN_VALIDATED

## TRACE_CHAIN_LOCKED

Dùng cho evidence/release/public policy

## TRACE_CHAIN_VALIDATED

## TRACE_RECALL_LINKED

Recall case active

## TRACE_CHAIN_BROKEN

## TRACE_WAITING_COMPLETION

Được sửa bằng mở rộng link hợp lệ

## TRACE_CHAIN_LOCKED

## TRACE_CLOSED

Đóng lifecycle theo policy

## 65.5. Chuyển trạng thái bị cấm

Không được:

TRACE_PARTIAL -> Public Trace valid.

TRACE_CHAIN_BROKEN -> Public Trace valid.

Trace tự tạo node thay owner module.

Trace sửa ledger/QC/release để làm chain đúng.

Public Trace dùng Internal Trace chưa whitelist.

Trace thiếu warehouse receipt mà vẫn sellable.

## 65.6. P0 điểm chặn

Traceability FAIL nếu:

Batch không trace được raw lot.

Batch không trace được Production Order.

PO không trace được formula snapshot.

QR không trace được batch/package.

Recall impact không dựa trace chain.

Public trace pass khi trace chain thiếu.

## 66. PUBLIC TRACE STATE MODEL

## 66.1. Mục tiêu

Public Trace State Model kiểm soát phản hồi truy xuất công khai theo whitelist.

## 66.2. Trạng thái public trace chuẩn

State

Ý nghĩa

## PUBLIC_TRACE_NOT_AVAILABLE

Chưa có public trace

## PUBLIC_TRACE_WAITING

Chờ đủ điều kiện

## PUBLIC_TRACE_VALIDATION_WAITING

Chờ validate QR/chain/whitelist

## PUBLIC_TRACE_VALID

Đủ điều kiện trả public whitelist

## PUBLIC_TRACE_BLOCKED

Bị chặn

## PUBLIC_TRACE_INVALID_QR

QR không hợp lệ

## PUBLIC_TRACE_CHAIN_MISSING

Thiếu trace chain

## PUBLIC_TRACE_RECALL_SAFE_MODE

Có recall, phản hồi theo policy an toàn

## PUBLIC_TRACE_CLOSED

Đóng public trace

## 66.3. Điều kiện PUBLIC_TRACE_VALID

Public Trace chỉ valid khi:

QR hợp lệ.

QR không VOID.

QR không FAILED.

QR link đúng batch/package.

Trace chain đủ theo scope.

Public whitelist tồn tại.

Không có field nội bộ trong response.

Recall/sale lock được xử lý theo policy.

Batch/release/warehouse điều kiện public pass theo policy.

Security/public trace review pass.

## 66.4. Chuyển trạng thái bị cấm

Không được:

## QR_VOID -> PUBLIC_TRACE_VALID.

## QR_FAILED -> PUBLIC_TRACE_VALID.

## TRACE_CHAIN_MISSING -> PUBLIC_TRACE_VALID.

PUBLIC_TRACE_VALID trả supplier evidence nội bộ.

PUBLIC_TRACE_VALID trả costing/MISA/personnel/QC defect/internal note.

PACKET QR public standalone nếu rule packaging cấm.

Public trace dùng dữ liệu raw internal view.

## 66.5. P0 điểm chặn

Public Trace FAIL nếu:

Lộ dữ liệu nội bộ.

QR invalid vẫn trả hợp lệ.

Trace thiếu vẫn trả hợp lệ.

Recall active nhưng không safe-mode.

Public whitelist chưa approved.

Public Trace runtime unavailable nhưng vẫn bịa dữ liệu.

## 67. RECALL STATE MODEL

## 67.1. Mục tiêu

Recall State Model kiểm soát vòng đời sự cố thu hồi và đảm bảo recall thắng mọi downstream bán hàng.

## 67.2. Trạng thái recall chuẩn

State

Ý nghĩa

## RECALL_DRAFT

Hồ sơ recall nháp

## RECALL_WAITING_REVIEW

Chờ review

## RECALL_ACTIVE

Recall có hiệu lực

## RECALL_IMPACT_ANALYSIS_IN_PROGRESS

Đang xác định phạm vi ảnh hưởng

## RECALL_IMPACT_CONFIRMED

Đã xác nhận phạm vi ảnh hưởng

## RECALL_SALE_LOCK_APPLIED

Đã áp sale lock

## RECALL_CUSTOMER_ACTION_WAITING

Chờ xử lý khách/kênh nếu scope có

## RECALL_CAPA_REQUIRED

Cần CAPA

## RECALL_CAPA_IN_PROGRESS

Đang xử lý CAPA

## RECALL_READY_FOR_CLOSE_REVIEW

Chờ review đóng

## RECALL_CLOSED

Đóng recall

## RECALL_REOPENED

Mở lại recall

## 67.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## RECALL_DRAFT

## RECALL_WAITING_REVIEW

Đủ thông tin ban đầu

## RECALL_WAITING_REVIEW

## RECALL_ACTIVE

Owner/quality decision

## RECALL_ACTIVE

## RECALL_IMPACT_ANALYSIS_IN_PROGRESS

Bắt đầu phân tích trace

## RECALL_IMPACT_ANALYSIS_IN_PROGRESS

## RECALL_IMPACT_CONFIRMED

Impact chain xác định

## RECALL_IMPACT_CONFIRMED

## RECALL_SALE_LOCK_APPLIED

Stop-sale được áp

## RECALL_SALE_LOCK_APPLIED

## RECALL_CUSTOMER_ACTION_WAITING

Nếu có customer/channel scope

## RECALL_SALE_LOCK_APPLIED

## RECALL_CAPA_REQUIRED

Nếu cần CAPA

## RECALL_CAPA_REQUIRED

## RECALL_CAPA_IN_PROGRESS

CAPA mở

## RECALL_CAPA_IN_PROGRESS

## RECALL_READY_FOR_CLOSE_REVIEW

CAPA evidence đủ

## RECALL_READY_FOR_CLOSE_REVIEW

## RECALL_CLOSED

Owner close approval

## RECALL_CLOSED

## RECALL_REOPENED

Có lý do/evidence mở lại

## 67.4. Chuyển trạng thái bị cấm

Không được:

RECALL_ACTIVE mà không sale lock nếu policy yêu cầu.

RECALL_ACTIVE nhưng Commerce vẫn quote/order.

RECALL_ACTIVE nhưng AI/Ads/Live/CRM vẫn bán.

RECALL_CLOSED khi chưa có impact analysis.

RECALL_CLOSED khi chưa có evidence.

RECALL_CLOSED khi chưa owner approval.

RECALL_CLOSED tự remove sale lock nếu unlock policy chưa pass.

Public trace lộ internal investigation.

## 67.5. P0 điểm chặn

Recall FAIL nếu:

Recall không chặn downstream.

Impact analysis không dựa trace chain.

Sale lock không propagate.

Close thiếu evidence.

Close thiếu owner decision.

Public trace lộ nội dung điều tra nội bộ.

## 68. CAPA STATE MODEL

## 68.1. Mục tiêu

CAPA State Model kiểm soát hành động khắc phục/phòng ngừa sau QC issue hoặc recall.

## 68.2. Trạng thái CAPA chuẩn

State

Ý nghĩa

## CAPA_DRAFT

CAPA nháp

## CAPA_WAITING_REVIEW

Chờ review

## CAPA_OPEN

CAPA đang mở

## CAPA_ROOT_CAUSE_IN_PROGRESS

Đang phân tích nguyên nhân

## CAPA_ACTION_IN_PROGRESS

Đang thực hiện hành động

## CAPA_EFFECTIVENESS_REVIEW_WAITING

Chờ đánh giá hiệu lực

## CAPA_READY_FOR_CLOSE

Sẵn sàng đóng

## CAPA_CLOSED

Đã đóng

## CAPA_REOPENED

Mở lại

## 68.3. Chuyển trạng thái bị cấm

Không được:

CAPA_CLOSED thiếu root cause.

CAPA_CLOSED thiếu action evidence.

CAPA_CLOSED thiếu effectiveness review nếu policy yêu cầu.

CAPA tự remove recall.

CAPA tự unlock sale lock.

CAPA public internal investigation.

## 68.4. P0 điểm chặn

CAPA FAIL nếu:

Dùng CAPA để bypass recall.

Close không evidence.

Không link incident/recall gốc.

Không audit.

Không owner approval.

## 69. SALE LOCK / STOP SALE STATE MODEL

## 69.1. Mục tiêu

Sale Lock / Stop Sale State Model kiểm soát quyền được bán ở cấp vận hành.

## 69.2. Trạng thái sale lock chuẩn

State

Ý nghĩa

## SALE_LOCK_NONE

Không có lock

## SALE_LOCK_WAITING

Chờ áp lock

## SALE_LOCK_ACTIVE

Lock đang hiệu lực

## SALE_LOCK_PROPAGATED

Lock đã propagate downstream

## SALE_LOCK_PARTIAL_PROPAGATION

Một số downstream chưa nhận

## SALE_LOCK_RELEASE_REVIEW_WAITING

Chờ review gỡ lock

## SALE_LOCK_RELEASED

Lock được gỡ hợp lệ

## SALE_LOCK_REOPENED

Lock mở lại

## SALE_LOCK_CLOSED

Đóng hồ sơ lock

## 69.3. Phạm vi lock

Sale Lock có thể áp theo:

Product.

## SKU.

Batch.

Raw lot nếu ảnh hưởng.

Warehouse/location nếu scope có.

Channel.

Campaign.

Public trace mode.

Customer/order pipeline nếu scope có.

## 69.4. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## SALE_LOCK_NONE

## SALE_LOCK_WAITING

Trigger từ recall/quality/owner

## SALE_LOCK_WAITING

## SALE_LOCK_ACTIVE

Lock approved/trigger confirmed

## SALE_LOCK_ACTIVE

## SALE_LOCK_PROPAGATED

Downstream nhận lock

## SALE_LOCK_ACTIVE

## SALE_LOCK_PARTIAL_PROPAGATION

Có downstream chưa confirm

## SALE_LOCK_PROPAGATED

## SALE_LOCK_RELEASE_REVIEW_WAITING

Có yêu cầu gỡ lock

## SALE_LOCK_RELEASE_REVIEW_WAITING

## SALE_LOCK_RELEASED

Owner approval + evidence

## SALE_LOCK_RELEASED

## SALE_LOCK_CLOSED

Đóng lock

## SALE_LOCK_RELEASED

## SALE_LOCK_REOPENED

Có trigger mới

## 69.5. Chuyển trạng thái bị cấm

Không được:

SALE_LOCK_ACTIVE nhưng Commerce quote/order.

SALE_LOCK_ACTIVE nhưng AI chốt sản phẩm.

SALE_LOCK_ACTIVE nhưng Ads/Live chạy bán.

SALE_LOCK_ACTIVE nhưng CRM upsell.

SALE_LOCK_RELEASED thiếu owner approval.

SALE_LOCK_PROPAGATED giả khi downstream chưa xác nhận.

Dùng cache cũ để vượt sale lock.

## 69.6. P0 điểm chặn

Sale Lock FAIL nếu:

Không propagate downstream.

Không có blocking reason.

Unlock thiếu evidence.

Runtime unavailable nhưng downstream assume unlocked.

Recall active nhưng lock không active.

## 70. COMMAND BOUNDARY

## 70.1. Định nghĩa command

Command là hành động làm thay đổi trạng thái hoặc tạo side-effect trong Operational Core.

Command có thể:

Tạo record vận hành.

Chuyển state.

Ghi ledger.

Tạo evidence.

Áp hold/recall/sale lock.

Release batch.

Confirm warehouse receipt.

Gỡ lock.

Close recall/CAPA.

Command phải có kiểm soát permission, audit, idempotency nếu có side-effect, evidence nếu ảnh hưởng release.

## 70.2. Nhóm command rủi ro cao

Các command sau là rủi ro cao:

Command Group

Ví dụ

Source/Intake

Approve source, create raw intake

## QC

Submit QC result, change QC state

Readiness

Mark raw lot READY_FOR_PRODUCTION

Production

Approve Production Order, lock formula snapshot

Material

Issue material, confirm receipt

Inventory

Post ledger, adjustment, write-off

Batch

Submit batch QC, release batch

Warehouse

Confirm warehouse receipt

## QR

Void QR, reprint QR

Trace

Lock trace chain

Public Trace

Approve public whitelist

Recall

Activate recall, close recall

## CAPA

Close CAPA

Sale Lock

Apply lock, release lock

Handoff

Publish sellable signal

## 70.3. Command bắt buộc có permission

Không command rủi ro nào được chạy chỉ dựa vào UI.

Permission phải enforce ở backend/application layer theo TECH-01.

Không được:

Hardcode role.

Cho admin override không audit.

Bỏ qua permission vì là nội bộ.

Dùng frontend hidden button làm cơ chế bảo mật.

## 70.4. Command bắt buộc có audit

Audit phải ghi:

Actor.

Action.

Object.

State before.

State after.

Reason nếu có.

Timestamp.

Environment.

Correlation reference nếu có.

Evidence reference nếu có.

## 70.5. Command bắt buộc idempotent

Các command có side-effect tồn kho hoặc trạng thái quan trọng phải idempotent:

Material Issue.

Warehouse Receipt.

Ledger posting.

QR generation/reprint/void nếu có side-effect.

Sale Lock propagation.

Recall activation.

MISA handoff nếu scope có.

Downstream operational handoff nếu có event/outbox.

Retry không được tạo:

Trừ kho hai lần.

Tăng tồn hai lần.

QR trùng không kiểm soát.

Recall case trùng.

Sale lock trùng gây sai trạng thái.

MISA sync trùng nếu scope có.

## 71. QUERY BOUNDARY

## 71.1. Định nghĩa query

Query là hành động đọc dữ liệu, không thay đổi state, không tạo side-effect.

Query có thể phục vụ:

Admin/internal review.

Production operator.

Warehouse.

Quality.

Commerce.

AI Advisor.

Ads/Live.

Public Trace.

MISA reconcile.

Evidence review.

## 71.2. Query không được thay state

Query không được:

Mark ready.

Issue material.

Release batch.

Confirm warehouse receipt.

Post ledger.

Apply recall.

Unlock sale lock.

Tạo quote/order.

Public dữ liệu ngoài whitelist.

Nếu query làm thay đổi trạng thái, đó là command và phải đi qua command boundary.

## 71.3. Internal query và public query phải tách biệt

Internal query có thể xem dữ liệu vận hành theo quyền.

Public query chỉ được xem dữ liệu whitelist.

Không được dùng một query internal rồi “ẩn field ở frontend” để public.

Public Trace phải là public-safe view riêng, được whitelist ở server/application boundary.

## 71.4. Downstream query phải fail-safe

Commerce/AI/Ads/Live khi query Operational Core phải nhận được một trong các kết quả rõ:

PASS.

bị chặn.

## UNAVAILABLE.

## NEEDS_REVIEW.

## NOT_IN_SCOPE.

## TRACE_MISSING.

## LOCKED.

## RECALLED.

## NOT_RELEASED.

## NO_STOCK.

## PUBLIC_TRACE_BLOCKED.

Không được trả dữ liệu mơ hồ khiến downstream tự đoán.

## 72. COMMAND-QUERY SEPARATION RULE

## 72.1. Nguyên tắc tách command-query

Operational Core phải tách rõ:

Command: thay đổi sự thật vận hành.

Query: đọc sự thật vận hành.

Không được dùng query để ghi nhận state.

Không được dùng command để trả public data ngoài boundary.

## 72.2. Ví dụ đúng

Nhu cầu

Loại đúng

Xem raw lot đã ready chưa

Query

Mark raw lot ready

Command

Xem batch QC status

Query

Submit batch QC result

Command

Xem batch có sellable không

Query qua sellable evaluation

Release batch

Command

Xem public trace

Public query

Approve public trace whitelist

Command

Xem sale lock status

Query

Apply sale lock

Command

Gỡ sale lock

Command

## 72.3. P0 điểm chặn

Command-query boundary FAIL nếu:

Query có side-effect.

Public query dùng internal query.

Command thiếu permission/audit.

Command tồn kho thiếu idempotency.

Downstream query nhận dữ liệu raw và tự tính sellable.

AI/Commerce dùng query không qua sellable/lock filter.

## 73. INVENTORY-LEDGER HANDOFF

## 73.1. Mục tiêu

Inventory-Ledger Handoff khóa cách các movement vận hành đi vào ledger và balance projection.

## 73.2. Source document được phép tạo ledger movement

Ledger Movement

Source document

Raw material in

Raw Intake nếu policy ghi nhận tồn tại intake

Raw material issue

Material Issue

Raw material adjustment

Adjustment command có approval

Finished goods receipt

Warehouse Receipt

Finished goods hold

Recall/Sale Lock/Hold command nếu policy giữ stock

Finished goods release from hold

Unlock command có approval

Finished goods write-off

Write-off command có approval

Finished goods sale allocation/debit

Commerce/Fulfillment ở TECH-04 nếu scope có

## 73.3. Ledger không nhận dữ liệu trực tiếp từ downstream

Không được cho:

AI ghi stock.

Ads ghi stock.

Live ghi stock.

CRM ghi stock.

Public Trace ghi stock.

MISA ghi ngược stock Operational nếu chưa có integration governance.

Manual script sửa balance production.

## 73.4. Balance projection

Lot Balance / Finished Goods Balance chỉ được tính từ ledger movement hợp lệ.

Balance projection phải tách:

Total balance.

Available balance.

Held balance.

Recalled balance.

Locked balance.

Allocated balance nếu Commerce scope có.

Damaged/write-off balance nếu scope có.

## 73.5. Available stock không đồng nghĩa sellable

Available stock chưa đủ để bán.

Cần thêm:

Batch released.

Warehouse receipt confirmed.

No recall.

No sale lock.

No quality hold.

Channel scope.

Commerce runtime pass.

## 73.6. P0 điểm chặn

Inventory-Ledger Handoff FAIL nếu:

Balance sửa trực tiếp.

Movement không source document.

Movement không audit.

Retry tạo trùng movement.

Available stock không trừ hold/recall/lock.

Downstream bán từ total balance thay vì sellable/available safe view.

## 74. TRACE HANDOFF

## 74.1. Mục tiêu

Trace Handoff khóa cách dữ liệu từ các module Operational đi vào trace chain.

## 74.2. Trace không tự tạo dữ liệu gốc

Trace chỉ liên kết dữ liệu từ owner module.

Trace không được tự tạo:

Source Origin.

Raw Lot.

QC result.

Production Order.

Formula Snapshot.

Material Issue.

Batch Release.

Warehouse Receipt.

Ledger Movement.

Sale Lock.

Recall.

Trace chỉ được link và validate chain.

## 74.3. Trace Handoff theo từng bước

Handoff

Dữ liệu đi vào trace

Product -> Trace

Product/SKU/Trade item

Formula Snapshot -> Trace

Formula kind/version/ingredient snapshot

Source Origin -> Trace

Source reference

Raw Intake -> Trace

Raw lot

Incoming QC -> Trace

QC status internal

Readiness -> Trace

Ready status

Material Issue -> Trace

Raw lot issued to PO/batch

Material Receipt -> Trace

Xưởng nhận nguyên liệu

Production Process -> Trace

Batch/process record

Packaging/QR -> Trace

Package/QR link

Batch QC/Release -> Trace

QC/release status

Warehouse Receipt -> Trace

Finished goods receipt

Inventory Ledger -> Trace

Movement references

Recall/Sale Lock -> Trace

Impact/lock state

## 74.4. Trace completeness rule

Trace chain được xem là complete khi có đủ node theo scope và không có broken link.

Nếu thiếu node bắt buộc, trace phải là:

TRACE_CHAIN_BROKEN hoặc TRACE_WAITING_COMPLETION

Không được trả:

## TRACE_CHAIN_COMPLETED

## 74.5. P0 điểm chặn

Trace Handoff FAIL nếu:

Trace complete giả.

Missing raw lot vẫn public trace.

Missing warehouse receipt vẫn sellable.

QR không link batch vẫn public trace.

Recall impact không query được affected chain.

Trace sửa dữ liệu gốc để lấp chain.

## 75. PUBLIC TRACE HANDOFF

## 75.1. Mục tiêu

Public Trace Handoff chuyển dữ liệu internal trace thành public-safe response.

## 75.2. Public Trace chỉ nhận dữ liệu đã được lọc

Public Trace chỉ được nhận:

Product public name.

Public batch/lot display nếu policy cho phép.

Manufacture/expiry nếu policy cho phép.

Source public-safe text nếu policy cho phép.

Trace status public-safe.

QR status public-safe.

Recall/safety public-safe message nếu policy có.

Link public product page nếu scope có.

## 75.3. Public Trace không nhận dữ liệu nội bộ

Public Trace không được nhận:

Supplier internal evidence.

Supplier internal ID.

Personnel.

Costing.

QC defect.

Loss.

Internal note.

MISA/private data.

Full audit trail.

CAPA internal investigation.

Recall internal decision notes.

Formula detail.

Ingredient quantity/internal recipe.

## 75.4. Public Trace safe response

Nếu không đủ điều kiện public trace, response phải an toàn:

Không xác nhận giả.

Không lộ nội bộ.

Không nói sản phẩm lỗi nếu policy chưa cho phép.

Không hiển thị stack/log/internal field.

Có thể trả trạng thái không khả dụng hoặc hướng dẫn liên hệ kênh chính thức theo policy.

## 75.5. P0 điểm chặn

Public Trace Handoff FAIL nếu:

Internal field lọt public.

Không có whitelist.

QR VOID/FAILED vẫn valid.

Chain thiếu vẫn valid.

Public trace dùng dữ liệu cũ khi runtime unavailable.

Public trace không safe-mode khi recall active.

## 76. OPERATIONAL HANDOFF SANG COMMERCE

## 76.1. Mục tiêu

Operational Core bàn giao trạng thái vận hành cho Commerce để Commerce xét quote/order.

## 76.2. Dữ liệu Operational được phép bàn giao cho Commerce

Commerce được nhận:

SKU operational scope.

Batch/finished goods release status.

Warehouse receipt status.

Available sellable-safe stock.

Sale lock status.

Recall status.

Quality hold status.

Trace/public trace eligibility nếu cần.

Blocking reason.

Runtime status.

## 76.3. Operational Core không bàn giao cho Commerce

Operational Core không quyết định:

Listed price.

Program price.

Member discount.

Diamond referral benefit.

Quote hold window.

Payment method.

Shipping fee.

COD policy.

VAT display.

Order confirmation.

Paid status.

## 76.4. Commerce phải chặn khi Operational không pass

Commerce phải BLOCK nếu nhận:

## NOT_RELEASED.

## NO_WAREHOUSE_RECEIPT.

## NO_STOCK.

## STOCK_HELD.

## SALE_LOCK_ACTIVE.

## RECALL_ACTIVE.

## QUALITY_HOLD.

TRACE_MISSING nếu policy yêu cầu trace.

PUBLIC_TRACE_BLOCKED nếu kênh yêu cầu public trace.

## RUNTIME_UNAVAILABLE.

## NOT_IN_RELEASE_SCOPE.

## 76.5. P0 điểm chặn

Commerce Handoff FAIL nếu:

Commerce quote/order khi Operational bị chặn.

Commerce dùng Product Active thay Operational Sellable Signal.

Commerce dùng stock không trừ recall/lock/hold.

Commerce bán khi Operational Runtime unavailable.

Commerce tự bỏ sale lock.

## 77. OPERATIONAL HANDOFF SANG AI ADVISOR

## 77.1. Mục tiêu

AI Advisor chỉ được tư vấn/chốt sản phẩm dựa trên trạng thái vận hành đã được Operational Core và Commerce Runtime xác nhận.

## 77.2. AI được nhận gì từ Operational Core

AI được nhận qua runtime/resolver:

Sản phẩm có thể tư vấn bán hay không.

Sản phẩm bị lock/recall hay không.

Có được chốt quote/order hay không sau Commerce.

Public trace link/status nếu public-safe.

Blocking reason public-safe.

Alternative suggestion eligibility nếu sản phẩm không sellable.

## 77.3. AI không được nhận gì

AI không được nhận hoặc public:

Số lượng tồn kho chi tiết nếu policy không cho public.

Supplier evidence nội bộ.

QC defect/loss/internal note.

Costing.

Formula detail.

Personnel.

MISA data.

Full recall internal investigation.

## 77.4. AI fail-safe

Nếu Operational Runtime unavailable:

AI không được nói còn hàng.

AI không được chốt đơn.

AI không được public trace như hợp lệ.

AI không được bịa trạng thái release.

AI phải dùng câu an toàn theo AI Advisor policy.

## 77.5. P0 điểm chặn

AI Handoff FAIL nếu:

AI bán sản phẩm bị sale lock.

AI bán sản phẩm recall.

AI chốt sản phẩm chưa release.

AI dùng thông tin tồn kho nội bộ để public.

AI tự tạo sellable status.

AI bịa public trace khi runtime lỗi.

## 78. OPERATIONAL HANDOFF SANG ADS / LIVE / CRM

## 78.1. Mục tiêu

Ads, Live và CRM chỉ được kích hoạt bán hàng khi Operational Core cho phép.

## 78.2. Ads

Ads không được:

Scale campaign cho SKU bị stop-sale.

Đẩy sản phẩm recall.

Dùng stock cũ để tối ưu bán.

Chạy hero SKU không sellable.

Bỏ qua release scope.

Ads phải nhận lock/sellable signal từ Operational/Commerce handoff.

## 78.3. Live / MC AI Live

Live không được:

Chốt SKU chưa sellable.

Nói còn hàng nếu Operational chưa xác nhận.

Bán sản phẩm đang recall.

Vượt sale lock.

Public trace sai.

Live có thể giới thiệu/giá/chốt chỉ khi Commerce runtime và Operational signal đều pass theo scope.

## 78.4. CRM

CRM không được:

Upsell sản phẩm stop-sale.

Gửi repurchase sản phẩm recall.

Tạo chiến dịch chăm sóc bán lại khi stock không sellable.

Dùng public trace không hợp lệ.

CRM phải nhận suppression signal khi sale lock/recall active.

## 78.5. P0 điểm chặn

Ads/Live/CRM Handoff FAIL nếu:

Campaign bán sản phẩm locked.

Live chốt hàng bị recall.

CRM upsell SKU không sellable.

Runtime unavailable nhưng vẫn gửi bán hàng.

Sale lock không propagate suppression.

## 79. OPERATIONAL HANDOFF SANG MISA

## 79.1. Mục tiêu

Operational Core có thể bàn giao dữ liệu vận hành sang MISA nếu scope tích hợp được bật.

TECH-03 không thiết kế chi tiết MISA API.

TECH-03 chỉ khóa boundary dữ liệu vận hành nào được phép đi sang MISA và trạng thái nào chưa được phép.

## 79.2. Dữ liệu có thể bàn giao sang MISA nếu scope có

Raw material intake.

Warehouse movement.

Material issue.

Finished goods receipt.

Inventory movement.

Production completion reference.

Batch/lot reference public/internal theo mapping.

Adjustment/write-off nếu có approval.

Reconcile status.

Missing mapping status.

## 79.3. Dữ liệu không được gửi sai sang MISA

Không được gửi:

Movement chưa posted.

Warehouse receipt chưa confirmed.

Batch chưa RELEASED nếu MISA receipt yêu cầu release.

Ledger movement trùng.

Mapping thiếu nhưng vẫn coi là synced.

Internal evidence file không thuộc scope.

Public trace data thay cho accounting/warehouse data.

## 79.4. MISA missing mapping

Nếu thiếu mapping:

Không được bỏ qua.

Không được tự tạo mapping production nếu chưa owner approved.

Phải giữ trạng thái WAITING/reconcile.

Không được làm sai tồn Operational.

Không được xem là sync success.

## 79.5. P0 điểm chặn

MISA Handoff FAIL nếu:

Gửi dữ liệu chưa đủ điều kiện.

Trùng movement do retry.

Missing mapping bị xem là success.

MISA sync làm thay đổi ngược Operational truth không governance.

Không có reconcile evidence.

## 80. RUNTIME UNAVAILABLE / BLOCKING RULES

## 80.1. Nguyên tắc chung

Nếu bất kỳ runtime quan trọng nào không khả dụng, downstream phải fail-safe.

Không được suy đoán.

Không được fallback sang dữ liệu cũ nếu không có policy rõ.

Không được dùng cache để vượt recall/sale lock.

## 80.2. Các runtime quan trọng

Runtime

Nếu unavailable thì

Product Runtime

Không tạo PO mới, không mở downstream product scope

Operational Runtime

Không quote/order production, AI không nói còn hàng

Inventory Runtime

Không sellable, không issue nếu không xác minh balance

Trace Runtime

Không public trace valid

Public Trace Runtime

Không trả public trace hợp lệ

Recall/Sale Lock Runtime

Downstream phải assume bị chặn theo fail-safe policy

Commerce Runtime

Không quote/order

Evidence Runtime

Không Completion Decision / Release Ready

MISA Runtime

Sync WAITING/reconcile, không coi là success

## 80.3. Fail-safe theo từng downstream

Downstream

Khi Operational unavailable

Commerce

Không quote/order production

AI Advisor

Không chốt, không nói còn hàng, dùng câu an toàn

Facebook Gateway

Không đẩy quote/order

Ads

Không scale/activate sản phẩm cần sellable

Live

Không chốt sản phẩm đó

## CRM

Không gửi upsell/repurchase sản phẩm đó

Public Trace

Không public valid nếu chain/runtime thiếu

## MISA

WAITING/reconcile, không success giả

## IVR

Không xác nhận tiếp order nếu order bị lock/recall hoặc runtime không xác minh

## 80.4. Blocking reason chuẩn

Operational Core phải trả blocking reason rõ cho downstream, ví dụ:

Blocking reason

Ý nghĩa

## PRODUCT_NOT_APPROVED

Product/SKU chưa hợp lệ

## FORMULA_NOT_APPROVED

Formula chưa approved

## RAW_LOT_NOT_READY

Raw lot chưa ready

## MATERIAL_NOT_ISSUED

Chưa xuất nguyên liệu

## BATCH_NOT_RELEASED

Batch chưa release

## WAREHOUSE_RECEIPT_MISSING

Chưa nhập kho thành phẩm

## NO_AVAILABLE_STOCK

Không có tồn khả dụng

## QUALITY_HOLD_ACTIVE

Đang hold chất lượng

## RECALL_ACTIVE

Đang recall

## SALE_LOCK_ACTIVE

Đang stop-sale

## TRACE_CHAIN_MISSING

Thiếu trace chain

## PUBLIC_TRACE_BLOCKED

Không đủ điều kiện public trace

## RUNTIME_UNAVAILABLE

Runtime không khả dụng

## OWNER_REVIEW_REQUIRED

Cần owner review

## EVIDENCE_NOT_ACCEPTED

Evidence chưa accepted

## NOT_IN_CHANNEL_SCOPE

Không thuộc scope kênh

## 81. CROSS-LIFECYCLE P0 điểm chặn MATRIX

## 81.1. Ma trận điểm chặn theo lifecycle

Lifecycle

P0 điểm chặn

Downstream bị chặn

Raw Lot

QC_PASS tự ready

Material Issue

Raw Lot

HOLD/REJECT/RECALL vẫn issue

Production

Production Order

Không snapshot formula

Material Request / Trace

Formula Snapshot

Snapshot bị ghi đè

Production history / Trace

Material Issue

Retry trừ tồn hai lần

Inventory / Production

Material Receipt

Trừ tồn lần hai

Inventory

Production Process

Batch không link PO

Batch QC / Trace

Packaging/QR

QR VOID/FAILED public-valid

Public Trace

Batch QC

QC_PASS tự RELEASED

Warehouse

Batch Release

Release thiếu evidence

Warehouse / Sellable

Warehouse Receipt

Nhận batch chưa RELEASED

Inventory

Inventory Ledger

Không append-only

Stock / MISA / Evidence

Trace

Chain thiếu vẫn complete

Public Trace / Recall

Public Trace

Lộ internal data

Public release

Recall

Không chặn downstream

Commerce / AI / Ads / Live

Sale Lock

Unlock thiếu approval

Commerce / AI / Ads / Live

Handoff

Runtime unavailable vẫn pass

Tất cả downstream

## 82. EVIDENCE CHO PHẦN 3/4

## 82.1. Evidence bắt buộc theo state machine

Mỗi state machine quan trọng phải có evidence:

Evidence ID

Nội dung

## OP-EVD-P3-001

Raw Lot State Transition Evidence

## OP-EVD-P3-002

Production Order State Transition Evidence

## OP-EVD-P3-003

Formula Snapshot Immutability Evidence

## OP-EVD-P3-004

Material Issue Idempotency Evidence

## OP-EVD-P3-005

Material Receipt No-Double-Decrement Evidence

## OP-EVD-P3-006

Batch QC vs Batch Release Separation Evidence

## OP-EVD-P3-007

Warehouse Receipt Release Gate Evidence

## OP-EVD-P3-008

Inventory Ledger Append-Only Evidence

## OP-EVD-P3-009

Trace Chain Completeness Evidence

## OP-EVD-P3-010

Public Trace Whitelist Evidence

## OP-EVD-P3-011

Recall Propagation Evidence

## OP-EVD-P3-012

Sale Lock Propagation Evidence

## OP-EVD-P3-013

Runtime Unavailable Fail-Safe Evidence

## OP-EVD-P3-014

Downstream Handoff Blocking Evidence

## OP-EVD-P3-015

MISA WAITING/Reconcile Evidence nếu scope có

## 82.2. Evidence metadata tối thiểu

Evidence phải có:

Evidence ID.

Requirement/source rule.

Module.

Object/state.

Environment.

Version/build reference nếu có.

Test/smoke reference.

Actor/reviewer.

Status.

Timestamp.

Result.

Link/correlation nếu có.

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

## 83. SMOKE MATRIX CHO PHẦN 3/4

## 83.1. Smoke lifecycle/state machine

Smoke ID

Lifecycle

Nội dung

Expected Result

## OP-P3-SMK-001

Raw Lot

QC_PASS chưa ready

Không issue được

## OP-P3-SMK-002

Raw Lot

READY_FOR_PRODUCTION + balance đủ

Issue được

## OP-P3-SMK-003

Raw Lot

## HOLD/REJECT/RECALL

Issue bị chặn

## OP-P3-SMK-004

## PO

PO không snapshot

Material Request bị chặn

## OP-P3-SMK-005

Snapshot

Recipe đổi sau PO

Snapshot không đổi

## OP-P3-SMK-006

Material Issue

Retry issue

Không trừ tồn hai lần

## OP-P3-SMK-007

Material Receipt

Receipt sau issue

Không giảm tồn lần hai

## OP-P3-SMK-008

Process

Process complete

Không tự tăng finished goods

## OP-P3-SMK-009

## QR

## QR VOID/FAILED

Không public-valid

## OP-P3-SMK-010

Batch

QC_PASS chưa RELEASED

Warehouse không nhận

## OP-P3-SMK-011

Batch

Batch RELEASED

Warehouse được xét nhận

## OP-P3-SMK-012

Warehouse

Receipt confirmed

Ledger credit posted

## OP-P3-SMK-013

Ledger

Adjustment

Movement mới, không sửa dòng cũ

## OP-P3-SMK-014

Trace

Missing chain

Trace không complete

## OP-P3-SMK-015

Public Trace

Request internal data

Không trả

## OP-P3-SMK-016

Recall

Recall active

Sale lock applied

## OP-P3-SMK-017

Sale Lock

Lock active

Commerce/AI/Ads/Live bị chặn

## OP-P3-SMK-018

Runtime

Operational unavailable

Downstream fail-safe

## OP-P3-SMK-019

## MISA

Missing mapping

WAITING/reconcile, không success

## OP-P3-SMK-020

Handoff

Product active nhưng batch chưa release

Not sellable

## 84. DONE GATE CỦA PHẦN 3/4

## 84.1. Điều kiện Done Gate

## PHẦN 3/4 chỉ được xem là DONE khi:

Raw Lot State Model đã khóa.

Production Order State Model đã khóa.

Formula Snapshot State Model đã khóa.

Material Movement State Model đã khóa.

Production Process State Model đã khóa.

Packaging / QR State Model đã khóa.

Batch State Model đã khóa.

Warehouse Receipt State Model đã khóa.

Inventory Ledger State Model đã khóa.

Traceability State Model đã khóa.

Public Trace State Model đã khóa.

Recall State Model đã khóa.

CAPA State Model đã khóa nếu scope có.

Sale Lock State Model đã khóa.

Command Boundary đã rõ.

Query Boundary đã rõ.

Command-query separation đã rõ.

Inventory-Ledger Handoff đã rõ.

Trace Handoff đã rõ.

Public Trace Handoff đã rõ.

Handoff sang Commerce/AI/Ads/Live/CRM/MISA đã rõ.

Runtime unavailable fail-safe đã rõ.

Blocking reason chuẩn đã rõ.

P0 điểm chặn matrix đã rõ.

Evidence requirement đã rõ.

Smoke matrix đã rõ.

Không gọi Documentation Complete là Production Ready.

## 85. FAIL GATE CỦA PHẦN 3/4

## 85.1. Điều kiện làm PHẦN 3 fail

## PHẦN 3/4 FAIL nếu có bất kỳ điểm nào sau:

State model cho raw lot cho phép QC_PASS tự ready.

State model cho batch cho phép QC_PASS tự released.

State model cho warehouse cho phép nhận batch chưa RELEASED.

Formula snapshot bị sửa theo live recipe.

Material Issue không idempotent.

Material Receipt trừ tồn lần hai.

Inventory Ledger không append-only.

Balance được sửa trực tiếp không movement.

Trace chain thiếu vẫn complete.

Public Trace dùng internal trace raw.

QR VOID/FAILED vẫn public-valid.

Recall active nhưng không sale lock.

Sale Lock active nhưng downstream vẫn quote/order/sell.

Command không permission/audit.

Query có side-effect.

Public query dùng internal query.

Runtime unavailable vẫn pass.

MISA missing mapping vẫn success.

Không có evidence accepted nhưng vẫn PASS.

Không có smoke nhưng vẫn Release Ready.

## 86. TRẠNG THÁI SAU PHẦN 3/4

## 86.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-03 PHẦN 3/4

## WRITTEN_FOR_REVIEW

Operational Lifecycle

## LOCKED_DRAFT

State Machine Model

## DEFINED

Command Boundary

## DEFINED

Query Boundary

## DEFINED

Inventory-Ledger Handoff

## DEFINED

Trace Handoff

## DEFINED

Public Trace Handoff

## DEFINED

Downstream Handoff

## DEFINED

Runtime Unavailable Rule

## DEFINED

P0 điểm chặn Matrix

## DEFINED

Evidence Requirement

## DEFINED

Smoke Matrix

## DEFINED

Production Ready

KHONG

Release Ready

KHONG

Go-live Approved

KHONG

## 87. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời vận hành của Operational Core.

Từ đây trở đi, Operational Core không được triển khai như các bảng dữ liệu rời rạc hoặc các thao tác thủ công thiếu trạng thái.

Mọi đối tượng vận hành đều phải đi qua state machine rõ:

Raw Lot -> Production Order -> Formula Snapshot -> Material Movement -> Production Process -> Packaging/QR -> Batch QC -> Batch Release -> Warehouse Receipt -> Inventory Ledger -> Trace -> Public Trace -> Recall/CAPA -> Sale Lock -> Downstream Handoff.

Nguyên tắc cuối cùng của PHẦN 3/4:

Command phải có quyền, audit, idempotency và evidence khi cần. Query chỉ được đọc, không được thay đổi state. Ledger phải append-only. Trace phải đủ chain. Public Trace phải whitelist-only. Recall/Sale Lock phải thắng mọi downstream. Runtime unavailable thì fail-safe, không suy đoán.

## PHẦN 3/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 4/4 - FINAL OPERATIONAL SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-03 FINAL CONCLUSION

## 88. MỤC TIÊU CỦA PHẦN 4/4

## 88.1. Vai trò của PHẦN 4/4

## PHẦN 4/4 là phần khóa cuối của TECH-03.

PHẦN này không viết thêm module mới.

PHẦN này dùng để khóa:

Smoke matrix tổng thể Operational Core.

Evidence package bắt buộc.

Owner review points.

Security / Public Trace review.

Downstream readiness rule.

Done Gate toàn TECH-03.

Fail Gate toàn TECH-03.

Release handoff sang TECH-04 Commerce và các downstream khác.

Kết luận cuối cùng của Operational Core.

## 88.2. Nguyên tắc khóa cuối

TECH-03 chỉ được xem là DOCUMENTATION_COMPLETE khi đủ 4 phần.

Nhưng:

DOCUMENTATION_COMPLETE không phải Production Ready.

TECH-03 hoàn tất tài liệu không có nghĩa là:

Đã implement.

Đã test.

Đã smoke pass.

Đã có accepted evidence.

Đã release ready.

Đã production ready.

Đã go-live approved.

Nguyên tắc dựa trên:

Không có accepted evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có release decision thì không Go-live Approved.

Không được dùng lời xác nhận miệng để PASS.

Không downstream nào được phụ thuộc production vào Operational Core nếu TECH-03 chưa clear implementation/evidence/smoke.

## 89. FINAL OPERATIONAL SMOKE MATRIX - TỔNG THỂ

## 89.1. Mục tiêu của Final Operational Smoke Matrix

Final Operational Smoke Matrix dùng để chứng minh chuỗi Operational Core hoạt động đúng từ đầu đến cuối.

Smoke không chỉ kiểm tra từng màn hình hoặc từng thao tác rời rạc.

Smoke phải chứng minh được các rule P0 sau:

Raw lot QC_PASS không tự động READY_FOR_PRODUCTION.

Raw lot chỉ được issue khi READY_FOR_PRODUCTION.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt không được giảm tồn lần hai.

Production Order phải snapshot formula.

Batch QC_PASS không đồng nghĩa RELEASED.

Warehouse chỉ nhận batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Ledger append-only.

Trace chain đầy đủ mới được trace.

Public Trace chỉ whitelist-only.

QR VOID/FAILED không public-valid.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM.

Runtime unavailable thì downstream fail-safe.

## 90. SMOKE GROUP A - FOUNDATION / PERMISSION / AUDIT / IDEMPOTENCY

## 90.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-A001

Command rủi ro yêu cầu permission

User không có quyền mark ready/release/issue

Bị chặn

CO

## OP-SMK-A002

Backend enforce permission

Ẩn UI nhưng gọi command trực tiếp

Bị chặn ở backend/application layer

CO

## OP-SMK-A003

Command rủi ro có audit

Material Issue / Batch Release / Sale Lock

Audit ghi actor, state before/after, timestamp

CO

## OP-SMK-A004

Idempotency cho Material Issue

Retry cùng command

Không trừ kho hai lần

CO

## OP-SMK-A005

Idempotency cho Warehouse Receipt

Retry receipt

Không tăng tồn hai lần

CO

## OP-SMK-A006

Evidence có metadata

Evidence thiếu environment/reviewer/status

Không được dùng để PASS

CO

## OP-SMK-A007

Audit append-only

Thử sửa/xóa audit cũ

Bị chặn

CO

## 90.2. Fail Gate

Nhóm A FAIL nếu:

Permission chỉ khóa ở UI.

Command rủi ro thiếu audit.

Command side-effect không idempotent.

Evidence thiếu metadata nhưng vẫn được dùng để PASS.

Audit có thể sửa/xóa.

Nếu nhóm A FAIL, toàn bộ TECH-03 implementation bị bị chặn.

## 91. SMOKE GROUP B - SOURCE ORIGIN / RAW INTAKE / INCOMING QC / RAW LOT READINESS

## 91.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-B001

Source chưa verified

Tạo raw intake từ source chưa verified

Bị chặn

CO

## OP-SMK-B002

Ingredient không hợp lệ

Intake ingredient không có trong Product Domain

Bị chặn

CO

## OP-SMK-B003

Raw Intake hợp lệ

Source verified + ingredient hợp lệ

Tạo raw lot, chưa ready

CO

## OP-SMK-B004

Raw lot QC_PASS

Submit QC_PASS

Raw lot chưa READY_FOR_PRODUCTION

CO

## OP-SMK-B005

Raw lot QC_FAIL

Submit QC_FAIL

Không issue được

CO

## OP-SMK-B006

Raw lot HOLD

Mark hold

Không issue được

CO

## OP-SMK-B007

Mark READY_FOR_PRODUCTION hợp lệ

QC_PASS + source pass + balance đủ

Ready thành công, có audit

CO

## OP-SMK-B008

Mark ready khi QC_FAIL

QC_FAIL nhưng cố mark ready

Bị chặn

CO

## OP-SMK-B009

Issue lot QC_PASS chưa ready

Lot QC_PASS nhưng chưa ready

Bị chặn

CO

## OP-SMK-B010

Issue lot READY_FOR_PRODUCTION

Lot ready + balance đủ

Được phép issue

CO

## 91.2. Fail Gate

Nhóm B FAIL nếu:

Raw lot QC_PASS tự động ready.

Raw lot chưa ready vẫn issue được.

HOLD/REJECT/RECALL vẫn issue được.

Source chưa verified vẫn intake/ready được.

Mark ready thiếu audit/evidence.

## 92. SMOKE GROUP C - PRODUCTION ORDER / FORMULA SNAPSHOT

## 92.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-C001

Tạo PO với SKU chưa approved

SKU không hợp lệ

Bị chặn

CO

## OP-SMK-C002

Tạo PO với formula chưa approved

Formula draft/WAITING

Bị chặn

CO

## OP-SMK-C003

PO không có formula snapshot

Tạo PO thiếu snapshot

Bị chặn

CO

## OP-SMK-C004

Snapshot đủ dữ liệu

Formula kind/version/ingredient/quantity/UOM/group

Snapshot locked

CO

## OP-SMK-C005

Recipe live đổi sau PO

Đổi recipe sau snapshot

PO snapshot không đổi

CO

## OP-SMK-C006

Snapshot thiếu group

Thiếu 1 trong 4 group

Không lock được

CO

## OP-SMK-C007

Snapshot bị sửa sau Material Issue

Cố sửa snapshot đã issue

Bị chặn

CO

## OP-SMK-C008

Production history lookup

Xem lại PO cũ

Hiển thị đúng snapshot tại thời điểm tạo

CO

## 92.2. Fail Gate

Nhóm C FAIL nếu:

PO không snapshot formula.

Snapshot bị ghi đè bởi live recipe.

Snapshot thiếu ingredient/quantity/UOM/group vẫn sản xuất.

Product/SKU/Recipe chưa approved vẫn tạo PO.

Lịch sử sản xuất thay đổi khi công thức hiện tại thay đổi.

## 93. SMOKE GROUP D - MATERIAL REQUEST / ISSUE / RECEIPT

## 93.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-D001

Request từ PO chưa snapshot

PO thiếu snapshot

Bị chặn

CO

## OP-SMK-D002

Request ingredient ngoài snapshot

Thêm nguyên liệu không thuộc snapshot

Bị chặn

CO

## OP-SMK-D003

Request vượt quantity snapshot

Vượt định mức không approval

Bị chặn hoặc WAITING review

CO

## OP-SMK-D004

Issue lot chưa ready

Raw lot QC_PASS nhưng chưa ready

Bị chặn

CO

## OP-SMK-D005

Issue lot ready đủ balance

Lot ready + balance đủ

Tạo ledger debit đúng

CO

## OP-SMK-D006

Issue vượt balance

Quantity issue > balance

Bị chặn

CO

## OP-SMK-D007

Retry issue

Retry cùng idempotency key

Không trừ kho hai lần

CO

## OP-SMK-D008

Receipt sau issue

Confirm material receipt

Không trừ tồn lần hai

CO

## OP-SMK-D009

Receipt không có issue

Receipt orphan

Bị chặn

CO

## OP-SMK-D010

Receipt variance

Có chênh lệch nhận

Ghi variance, không sửa ledger cũ

CO

## 93.2. Fail Gate

Nhóm D FAIL nếu:

Issue lot chưa ready.

Issue không tạo ledger debit.

Retry issue trừ kho hai lần.

Material Receipt trừ tồn lần hai.

Receipt sửa ledger debit cũ.

Request/Issue dùng nguyên liệu ngoài formula snapshot.

## 94. SMOKE GROUP E - PRODUCTION PROCESS / PACKAGING / PRINTING / QR

## 94.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-E001

Process không có PO

Tạo process không link PO

Bị chặn

CO

## OP-SMK-E002

Process chưa nhận nguyên liệu

Chưa material receipt

Không cho start nếu policy yêu cầu

CO

## OP-SMK-E003

Process completed

Hoàn tất production process

Không tự tăng tồn thành phẩm

CO

## OP-SMK-E004

Batch link PO

Tạo batch từ process

Batch link đúng PO/snapshot

CO

## OP-SMK-E005

Packaging không có batch

Tạo packaging job không batch

Bị chặn

CO

## OP-SMK-E006

QR generated

QR link đúng batch/package

QR chưa public-valid nếu chưa đủ chain

CO

## OP-SMK-E007

## QR PRINT_FAILED

QR in lỗi

Không public-valid

CO

## OP-SMK-E008

## QR VOID

QR bị void

Không public-valid

CO

## OP-SMK-E009

QR reprint

Reprint có reason/audit

Reprint hợp lệ, QR lifecycle rõ

CO

## OP-SMK-E010

PACKET public trace standalone

Packet QR standalone

Bị chặn nếu rule packaging cấm

CO

## OP-SMK-E011

## BOX/CARTON QR

QR cấp BOX/CARTON đủ chain

Được xét public trace theo whitelist

CO

## 94.2. Fail Gate

Nhóm E FAIL nếu:

Production completed tự tăng tồn thành phẩm.

Batch không link Production Order.

QR không link batch/package.

QR VOID/FAILED vẫn public-valid.

QR được dùng thay Batch Release/Warehouse Receipt để mở bán.

PACKET có public trace standalone trái rule packaging đã khóa.

Reprint không audit/reason.

## 95. SMOKE GROUP F - BATCH QC / BATCH RELEASE / WAREHOUSE RECEIPT

## 95.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-F001

Batch QC_PASS

Submit QC_PASS

Batch chưa RELEASED

CO

## OP-SMK-F002

Batch QC_FAIL

Submit QC_FAIL

Không release được

CO

## OP-SMK-F003

Batch HOLD

Batch đang hold

Không release/warehouse receipt

CO

## OP-SMK-F004

Release QC_PASS batch

QC_PASS + không hold + evidence đủ

Batch RELEASED

CO

## OP-SMK-F005

Release QC_FAIL batch

## QC_FAIL

Bị chặn

CO

## OP-SMK-F006

Warehouse receipt QC_PASS chưa release

Batch QC_PASS nhưng chưa released

Bị chặn

CO

## OP-SMK-F007

Warehouse receipt RELEASED batch

Batch RELEASED

Receipt confirmed, ledger credit posted

CO

## OP-SMK-F008

Retry warehouse receipt

Retry cùng command

Không tăng tồn hai lần

CO

## OP-SMK-F009

Finished goods stock trước receipt

Batch released nhưng chưa receipt

Stock chưa tăng

CO

## OP-SMK-F010

Finished goods stock sau receipt

Receipt confirmed

Stock tăng theo ledger credit

CO

## 95.2. Fail Gate

Nhóm F FAIL nếu:

Batch QC_PASS tự động RELEASED.

Batch chưa RELEASED vẫn warehouse receipt được.

Finished goods stock tăng trước warehouse receipt confirmed.

Retry warehouse receipt tăng tồn hai lần.

Batch HOLD/REJECT/RECALL vẫn nhập kho.

## 96. SMOKE GROUP G - INVENTORY LEDGER / LOT BALANCE

## 96.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-G001

Ledger append-only

Thử sửa movement cũ

Bị chặn

CO

## OP-SMK-G002

Xóa movement cũ

Thử xóa ledger

Bị chặn

CO

## OP-SMK-G003

Adjustment

Điều chỉnh tồn

Tạo movement mới, có reason/approval

CO

## OP-SMK-G004

Balance projection

Tính từ movement

Balance đúng theo ledger

CO

## OP-SMK-G005

Available stock

Có hold/recall/lock

Available stock loại trừ phần bị chặn

CO

## OP-SMK-G006

Negative balance

Issue vượt balance

Bị chặn hoặc xử lý theo policy, không âm tự do

CO

## OP-SMK-G007

Downstream dùng total stock

Commerce/AI đọc total không qua sellable

Bị chặn

CO

## OP-SMK-G008

Write-off

Write-off có approval

Tạo movement mới, không sửa lịch sử

CO

## 96.2. Fail Gate

Nhóm G FAIL nếu:

Ledger không append-only.

Balance sửa trực tiếp.

Movement thiếu source document.

Adjustment/write-off thiếu reason/approval.

Available stock không loại trừ hold/recall/sale lock.

Downstream dùng total stock để bán.

## 97. SMOKE GROUP H - TRACEABILITY / PUBLIC TRACE

## 97.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-H001

Internal trace đủ chain

Product -> Raw lot -> PO -> Batch -> Warehouse

Trace complete

CO

## OP-SMK-H002

Thiếu raw lot link

Batch không link raw lot

Trace không complete

CO

## OP-SMK-H003

Thiếu PO snapshot

Batch không link snapshot

Trace broken

CO

## OP-SMK-H004

Thiếu warehouse receipt

Batch chưa nhập kho

Không đủ sellable/public nếu policy yêu cầu

CO

## OP-SMK-H005

QR không link batch

QR orphan

Public trace bị chặn

CO

## OP-SMK-H006

## QR VOID

Scan QR VOID

Không public-valid

CO

## OP-SMK-H007

## QR FAILED

Scan QR FAILED

Không public-valid

CO

## OP-SMK-H008

Public whitelist

Public trace valid

Chỉ trả field whitelist

CO

## OP-SMK-H009

Request internal data

Supplier evidence/QC defect/costing/personnel

Không trả dữ liệu

CO

## OP-SMK-H010

Public trace runtime unavailable

Scan QR khi runtime lỗi

Fail-safe, không bịa dữ liệu

CO

## OP-SMK-H011

Recall active public trace

Batch recall

Safe-mode theo policy

CO

## 97.2. Fail Gate

Nhóm H FAIL nếu:

Trace chain thiếu nhưng vẫn complete.

QR VOID/FAILED vẫn public-valid.

Public Trace lộ supplier evidence, personnel, costing, QC defect/loss/internal note, MISA/private data.

Public Trace dùng internal trace raw.

Runtime unavailable vẫn trả như hợp lệ.

Không có whitelist approved vẫn public dữ liệu.

## 98. SMOKE GROUP I - RECALL / CAPA / SALE LOCK / STOP SALE

## 98.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-I001

Recall theo batch

Recall batch cụ thể

Batch bị sale lock

CO

## OP-SMK-I002

Recall theo SKU

Recall SKU

SKU bị block downstream

CO

## OP-SMK-I003

Recall impact analysis

Trace từ raw lot/batch

Xác định đúng affected product/SKU/batch/stock

CO

## OP-SMK-I004

Recall active chặn Commerce

Commerce quote/order

Bị chặn

CO

## OP-SMK-I005

Recall active chặn AI

AI tư vấn/chốt

Bị chặn hoặc chuyển safe response

CO

## OP-SMK-I006

Recall active chặn Ads/Live

Campaign/live selling

Bị chặn

CO

## OP-SMK-I007

Sale Lock active

Quote/order/chốt đơn

Bị chặn

CO

## OP-SMK-I008

Unlock thiếu approval

Gỡ sale lock không evidence

Bị chặn

CO

## OP-SMK-I009

CAPA close thiếu evidence

Close CAPA

Bị chặn

CO

## OP-SMK-I010

Recall close thiếu impact/evidence

Close recall

Bị chặn

CO

## OP-SMK-I011

CAPA tự remove sale lock

CAPA close tự unlock

Bị chặn

CO

## 98.2. Fail Gate

Nhóm I FAIL nếu:

Recall active không tạo hoặc không kích hoạt Sale Lock.

Recall không chặn Commerce/AI/Ads/Live/CRM.

Sale Lock active nhưng downstream vẫn bán.

Unlock thiếu owner approval.

Recall close thiếu evidence.

CAPA tự gỡ lock.

Public trace lộ internal recall investigation.

## 99. SMOKE GROUP J - OPERATIONAL HANDOFF SANG DOWNSTREAM

## 99.1. Smoke Matrix

Smoke ID

Downstream

Dữ liệu kiểm tra

Expected Result

## P0

## OP-SMK-J001

Commerce

Product Active nhưng chưa batch released

Không quote/order

CO

## OP-SMK-J002

Commerce

Batch released nhưng chưa warehouse receipt

Không quote/order

CO

## OP-SMK-J003

Commerce

Stock available nhưng sale lock active

Không quote/order

CO

## OP-SMK-J004

AI Advisor

SKU bị recall

Không chốt bán

CO

## OP-SMK-J005

AI Advisor

Operational runtime unavailable

Không nói còn hàng, không chốt

CO

## OP-SMK-J006

Ads

SKU stop-sale

Không activate/scale bán

CO

## OP-SMK-J007

Live / MC AI

Live đang bán SKU locked

Bị chặn

CO

## OP-SMK-J008

## CRM

Repurchase SKU recall

Bị suppression

CO

## OP-SMK-J009

Public Trace

QR valid nhưng whitelist thiếu

Không public field

CO

## OP-SMK-J010

## MISA

Missing mapping

WAITING/reconcile, không success giả

CO

## OP-SMK-J011

IVR nếu scope có

Order bị sale lock trước xác nhận

Không tiếp tục xác nhận như bình thường

CO

## 99.2. Fail Gate

Nhóm J FAIL nếu:

Commerce bán sản phẩm Operational chưa pass.

AI tự bịa sellable/còn hàng.

Ads/Live chạy sản phẩm bị stop-sale.

CRM upsell SKU bị recall.

MISA missing mapping nhưng xem là sync success.

IVR tiếp tục xác nhận order đang bị lock/recall nếu scope có.

## 100. SMOKE GROUP K - RUNTIME UNAVAILABLE / FAIL-SAFE

## 100.1. Smoke Matrix

Smoke ID

Runtime lỗi

Expected Result

## P0

## OP-SMK-K001

Product Runtime unavailable

Không tạo PO mới, downstream block

CO

## OP-SMK-K002

Operational Runtime unavailable

Commerce không quote/order, AI không chốt

CO

## OP-SMK-K003

Inventory Runtime unavailable

Không sellable, không issue nếu không xác minh balance

CO

## OP-SMK-K004

Trace Runtime unavailable

Không public trace valid

CO

## OP-SMK-K005

Recall/Sale Lock Runtime unavailable

Downstream fail-safe, không assume unlocked

CO

## OP-SMK-K006

Evidence Runtime unavailable

Không Completion Decision / Release Ready

CO

## OP-SMK-K007

MISA Runtime unavailable

Sync WAITING/reconcile, không success

CO

## OP-SMK-K008

Public Trace Runtime unavailable

Safe response, không lộ nội bộ

CO

## 100.2. Fail Gate

Nhóm K FAIL nếu:

Runtime unavailable nhưng downstream vẫn cho bán.

Cache cũ được dùng để vượt sale lock/recall.

AI nói còn hàng khi không xác minh được.

Commerce tạo quote/order khi Operational unavailable.

Public trace trả hợp lệ khi trace runtime lỗi.

Evidence runtime lỗi nhưng vẫn PASS.

## 101. FINAL OPERATIONAL EVIDENCE PACKAGE

## 101.1. Mục tiêu của Evidence Package

Evidence Package là bộ chứng cứ bắt buộc để chứng minh TECH-03 đã được triển khai, kiểm thử, smoke và review đúng.

Không có Evidence Package ACCEPTED thì không được gọi:

Completion Decision.

Release Ready.

Production Ready.

Go-live Approved.

## 102. EVIDENCE PACKAGE STRUCTURE

## 102.1. Nhóm evidence bắt buộc

Evidence Group

Nội dung

Bắt buộc

## OP-EVD-G01

Foundation Permission / Audit / Idempotency Evidence

CO

## OP-EVD-G02

Product Dependency / Formula Snapshot Evidence

CO

## OP-EVD-G03

Source Origin / Raw Intake Evidence

CO

## OP-EVD-G04

Incoming QC / Raw Lot Readiness Evidence

CO

## OP-EVD-G05

Production Order / Snapshot Evidence

CO

## OP-EVD-G06

Material Request / Issue / Receipt Evidence

CO

## OP-EVD-G07

Production Process / Batch Evidence

CO

## OP-EVD-G08

Packaging / QR Lifecycle Evidence

CO

## OP-EVD-G09

Batch QC / Batch Release Evidence

CO

## OP-EVD-G10

Warehouse Receipt / Inventory Ledger Evidence

CO

## OP-EVD-G11

Traceability Evidence

CO

## OP-EVD-G12

Public Trace Whitelist / Security Evidence

CO

## OP-EVD-G13

Recall / CAPA / Sale Lock Evidence

CO

## OP-EVD-G14

Downstream Handoff Evidence

CO

## OP-EVD-G15

MISA Handoff Evidence nếu scope có

## CONDITIONAL

## OP-EVD-G16

Runtime Unavailable / Fail-Safe Evidence

CO

## OP-EVD-G17

Final Smoke Report

CO

## OP-EVD-G18

Owner Review / Sign-off Record

CO

## OP-EVD-G19

Release Handoff Record

CO

## 103. EVIDENCE METADATA STANDARD

## 103.1. Metadata tối thiểu

Mỗi evidence record phải có:

Evidence ID.

Requirement ID hoặc source rule.

TECH / module / smoke mapping.

Environment.

Build/version nếu có.

Test/smoke ID.

Actor.

Reviewer.

Review status.

Timestamp.

Result.

Attachment/log/screenshot/report link nếu có.

Audit/correlation ID nếu có.

Owner decision nếu dùng cho gate.

Notes về rủi ro còn lại nếu có.

## 103.2. Evidence status chuẩn

Status

Ý nghĩa

Được dùng để PASS

## DRAFT

Evidence nháp

KHONG

## SUBMITTED

Đã nộp review

KHONG

## NEEDS_CLARIFICATION

Cần làm rõ

KHONG

## REJECTED

Bị từ chối

KHONG

## ACCEPTED

Đã được reviewer/owner chấp nhận

CO

## SUPERSEDED

Bị thay thế bởi evidence mới

KHONG

## EXPIRED

Không còn hợp lệ

KHONG

## 103.3. Rule khóa

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

Không dùng:

Lời xác nhận miệng.

Screenshot rời không mapping.

Log không environment.

Test không reviewer.

Smoke chưa pass.

Evidence không liên kết requirement.

Evidence không có timestamp.

Evidence không có owner/reviewer khi cần.

## 104. OWNER REVIEW POINTS

## 104.1. Mục tiêu

Owner Review Points xác định các điểm bắt buộc cần người có thẩm quyền duyệt.

Operational Core có nhiều command ảnh hưởng trực tiếp đến sản xuất, tồn kho, truy xuất, recall và bán hàng. Vì vậy owner review không được bỏ qua.

## 105. OWNER REVIEW MATRIX

Review Point

Nội dung review

Owner đề xuất

Bắt buộc

## OP-ORP-001

Operational boundary Product/Operational/Commerce

Business/Product/Technical Owner

CO

## OP-ORP-002

Raw material readiness policy

Production/QC Owner

CO

## OP-ORP-003

QC criteria / threshold policy

QC Owner

CO

## OP-ORP-004

Formula snapshot policy

Product/Production Owner

CO

## OP-ORP-005

Material issue / receipt rule

Warehouse/Production Owner

CO

## OP-ORP-006

Ledger append-only / adjustment policy

Inventory/Finance Owner

CO

## OP-ORP-007

Batch release rule

QC/Production Owner

CO

## OP-ORP-008

Warehouse receipt gate

Warehouse Owner

CO

## OP-ORP-009

Packaging / QR level rule

Production/Trace Owner

CO

## OP-ORP-010

Public Trace whitelist

Security/Trace/Business Owner

CO

## OP-ORP-011

Recall / CAPA close rule

QC/Business Owner

CO

## OP-ORP-012

Sale Lock priority

Business/Commerce/Operational Owner

CO

## OP-ORP-013

Downstream handoff blocking reason

Technical/Runtime Owner

CO

## OP-ORP-014

MISA handoff rule nếu scope có

Finance/MISA Owner

## CONDITIONAL

## OP-ORP-015

Final Operational smoke acceptance

Release Owner

CO

## OP-ORP-016

TECH-03 release handoff

Technical Owner

CO

## 106. SECURITY / PUBLIC TRACE REVIEW

## 106.1. Mục tiêu

Security / Public Trace Review đảm bảo dữ liệu public không làm lộ thông tin nội bộ, dữ liệu nhạy cảm, dữ liệu vận hành hoặc dữ liệu kế toán.

## 106.2. Public Trace whitelist bắt buộc review

Public Trace chỉ được phép hiển thị field đã được whitelist.

Whitelist phải được review cho từng nhóm dữ liệu:

Nhóm dữ liệu

Public mặc định

Product public name

Có thể public nếu approved

Product public description

Có thể public nếu approved

Batch display code

Chỉ public nếu policy cho phép

## NSX / HSD

Chỉ public nếu policy cho phép

Source public-safe text

Chỉ public nếu approved

QR valid status

Có thể public-safe

Trace safe status

Có thể public-safe

Recall/safety message

Chỉ theo policy

Supplier evidence

Không public

Supplier internal ID

Không public

Personnel

Không public

Costing

Không public

QC defect/loss/internal note

Không public

MISA/private data

Không public

Formula detail / ingredient quantity

Không public

Internal audit trail

Không public

CAPA internal investigation

Không public

Recall internal decision note

Không public

## 106.3. Security Review Smoke

Smoke ID

Nội dung

Expected Result

## OP-SEC-SMK-001

Public trace request supplier evidence

Không trả

## OP-SEC-SMK-002

Public trace request costing

Không trả

## OP-SEC-SMK-003

Public trace request personnel

Không trả

## OP-SEC-SMK-004

Public trace request QC defect/internal note

Không trả

## OP-SEC-SMK-005

Public trace QR VOID/FAILED

Không valid

## OP-SEC-SMK-006

Public trace thiếu chain

Fail-safe

## OP-SEC-SMK-007

Public trace runtime lỗi

Safe response

## OP-SEC-SMK-008

Recall active public view

Safe-mode theo policy

## 106.4. Fail Gate

Security/Public Trace FAIL nếu:

Public DTO/view chứa field nội bộ.

Public trace dùng internal query raw.

Whitelist chưa approved.

QR invalid vẫn public-valid.

Trace thiếu vẫn public-valid.

Recall active nhưng public trace không safe-mode.

Public response lộ lỗi kỹ thuật, stack, ID nội bộ hoặc dữ liệu riêng tư.

## 107. DOWNSTREAM READINESS RULE

## 107.1. Mục tiêu

Downstream Readiness Rule xác định khi nào các hệ thống phía sau được phép phụ thuộc vào Operational Core.

Downstream bao gồm:

TECH-04 Commerce Runtime.

AI Advisor.

Facebook Gateway.

Ads.

MC AI Live.

## CRM.

Public Trace.

MISA nếu scope có.

IVR nếu scope có.

## 108. DOWNSTREAM DEPENDENCY MATRIX

Downstream

Chỉ được đi tiếp khi

Nếu Operational chưa clear

Commerce

Sellable signal, stock, sale lock, recall smoke pass

Không quote/order production

AI Advisor

Operational + Commerce resolver pass

Không chốt sản phẩm, không nói còn hàng

Facebook Gateway

Commerce/AI handoff pass

Không đẩy quote/order

Ads

Sellable/sale lock/recall status pass

Không scale/activate bán

MC AI Live

Live SKU sellable + no lock

Không chốt live

## CRM

Repurchase/upsell SKU không bị lock/recall

Suppression

Public Trace

QR valid + chain đủ + whitelist approved

Safe response

## MISA

Mapping đủ + movement eligible

WAITING/reconcile

## IVR

Order không bị lock/recall + Commerce valid

Không xác nhận tiếp như bình thường

## 109. DOWNSTREAM BLOCKING REASONS

## 109.1. Blocking reason bắt buộc

Operational Core phải bàn giao blocking reason rõ.

Blocking Reason

Ý nghĩa

## PRODUCT_NOT_APPROVED

Product/SKU chưa hợp lệ

## FORMULA_NOT_APPROVED

Công thức chưa approved

## FORMULA_SNAPSHOT_MISSING

PO thiếu snapshot

## RAW_LOT_NOT_READY

Raw lot chưa ready

## RAW_LOT_HOLD

Raw lot đang hold

## MATERIAL_NOT_ISSUED

Chưa issue nguyên liệu

## MATERIAL_RECEIPT_MISSING

Xưởng chưa receipt

## PRODUCTION_NOT_COMPLETED

Sản xuất chưa hoàn tất

## BATCH_QC_WAITING

Batch chưa QC

## BATCH_QC_FAILED

Batch QC fail

## BATCH_NOT_RELEASED

Batch chưa released

## WAREHOUSE_RECEIPT_MISSING

Thành phẩm chưa nhập kho

## INVENTORY_LEDGER_NOT_POSTED

Ledger chưa ghi

## NO_AVAILABLE_STOCK

Không có tồn khả dụng

## QUALITY_HOLD_ACTIVE

Hold chất lượng

## RECALL_ACTIVE

Recall active

## SALE_LOCK_ACTIVE

Sale lock active

## TRACE_CHAIN_MISSING

Thiếu trace chain

## PUBLIC_TRACE_BLOCKED

Public trace bị chặn

## QR_INVALID

QR invalid/void/failed

## RUNTIME_UNAVAILABLE

Runtime không khả dụng

## EVIDENCE_NOT_ACCEPTED

Evidence chưa accepted

## OWNER_REVIEW_REQUIRED

Cần owner review

## NOT_IN_CHANNEL_SCOPE

Không thuộc scope kênh

## 110. RELEASE HANDOFF PACKAGE

## 110.1. Mục tiêu

Release Handoff Package là bộ hồ sơ bàn giao TECH-03 sang các TECH downstream và DEV implementation wave.

Handoff không phải “cho phép go-live”.

Handoff chỉ có nghĩa:

Tài liệu đã đủ rõ để dev lập implementation plan.

Các boundary đã khóa.

Các P0 điểm chặn đã khóa.

Các smoke/evidence/gate đã rõ.

Downstream biết phải phụ thuộc vào gì.

## 111. RELEASE HANDOFF CONTENT

Release Handoff Package của TECH-03 phải có:

Module contract list OP-M01 -> OP-M21.

State machine list.

Command-query boundary.

Inventory ledger rule.

Trace/public trace rule.

Recall/sale lock priority rule.

Downstream handoff rule.

Blocking reason registry.

Smoke matrix OP-SMK-A -> OP-SMK-K.

Evidence package OP-EVD-G01 -> OP-EVD-G19.

Owner review matrix.

Security/public trace review checklist.

P0 điểm chặn registry.

Done Gate / Fail Gate.

Gap report template cho implementation.

Handoff note sang TECH-04 Commerce.

Handoff note sang AI/Ads/Live/Public Trace/MISA.

Statement rõ: Documentation Complete không phải Production Ready.

## 112. HANDOFF SANG TECH-04 COMMERCE

## 112.1. Commerce chỉ được dùng Operational Core như nguồn sellable gate

TECH-04 Commerce phải dựa trên từ TECH-03:

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Batch chưa RELEASED không sellable.

Warehouse chưa receipt thì chưa có finished goods stock.

Sale Lock / Recall thắng Commerce.

Runtime unavailable thì không quote/order production.

Stock dùng cho quote/order phải là sellable-safe stock, không phải total stock thô.

## 112.2. Commerce không được tự tạo Operational truth

TECH-04 Commerce không được:

Tự đánh dấu batch released.

Tự tăng tồn.

Tự bỏ recall.

Tự gỡ sale lock.

Tự public trace.

Tự dùng product active để bán.

Tự dùng tồn kho chưa qua Operational handoff.

## 112.3. Điều kiện để TECH-04 được đi vào production implementation

TECH-04 chỉ được production implementation khi:

TECH-03 module map đã clear.

Operational sellable signal đã xác định.

Inventory/stock handoff đã xác định.

Sale lock/recall handoff đã xác định.

Blocking reason registry đã xác định.

Smoke requirement cho Commerce dependency đã xác định.

P0 dependency chưa clear phải được ghi trong gap report.

## 113. HANDOFF SANG AI ADVISOR

## 113.1. AI Advisor chỉ được tư vấn/chốt dựa trên runtime đã pass

AI Advisor phải dựa trên:

Không tư vấn/chốt sản phẩm hết hàng hoặc không sellable.

Không public số lượng tồn kho nếu policy không cho phép.

Không tự bịa release/trace/stock.

Không bán sản phẩm sale lock/recall.

Không public dữ liệu trace nội bộ.

Nếu runtime unavailable thì dùng safe response.

## 113.2. AI phải nhận blocking reason public-safe

AI không được nhận dữ liệu nội bộ để tự diễn giải.

AI chỉ nhận:

Có thể bán / không thể bán.

Blocking reason public-safe.

Có thể gợi ý thay thế hay không.

Public trace link/status nếu approved.

Không được public nội bộ tồn kho, QC, supplier, costing.

## 114. HANDOFF SANG ADS / LIVE / CRM

## 114.1. Ads

Ads phải bị chặn nếu:

SKU sale lock.

SKU recall.

SKU không sellable.

Operational runtime unavailable.

Không thuộc channel scope.

## 114.2. Live / MC AI Live

Live chỉ được nói/chốt sản phẩm khi:

Operational pass.

Commerce pass.

Không recall.

Không sale lock.

Không quality hold.

Giá/quote do Commerce Runtime quyết định.

## 114.3. CRM

CRM phải suppression nếu:

SKU recall.

SKU sale lock.

SKU không sellable.

Runtime không xác minh được.

Public trace policy không cho phép nội dung truy xuất.

## 115. HANDOFF SANG PUBLIC TRACE

## 115.1. Public Trace chỉ mở khi đủ điều kiện

Public Trace chỉ được mở nếu:

QR valid.

QR không VOID/FAILED.

Trace chain đủ.

Public whitelist approved.

Security review pass.

Recall/sale lock public policy rõ.

Runtime public trace available.

## 115.2. Public Trace không phải Internal Trace

Public Trace không được public:

Supplier evidence nội bộ.

Personnel.

Costing.

QC defect/loss/internal note.

MISA/private data.

Formula detail.

Internal audit.

CAPA/recall internal investigation.

## 116. HANDOFF SANG MISA NẾU SCOPE CÓ

## 116.1. MISA chỉ nhận dữ liệu đủ điều kiện

MISA handoff chỉ được thực hiện với:

Movement có source document.

Ledger posted.

Warehouse receipt confirmed.

Mapping hợp lệ.

Idempotency/reconcile có kiểm soát.

Missing mapping giữ WAITING/reconcile.

## 116.2. MISA không thay Operational truth

MISA không được ghi ngược để thay đổi Operational truth nếu chưa có governance riêng.

Nếu MISA khác Operational:

Không tự sửa Operational.

Ghi reconcile issue.

Owner review.

Evidence.

Adjustment nếu được duyệt.

## 117. TECH-03 FINAL DONE GATE

## 117.1. Điều kiện Done Gate toàn TECH-03

TECH-03 chỉ được xem là DOCUMENTATION_COMPLETE khi đủ các điều kiện sau:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Operational Core Principles đã khóa.

Source-of-truth đã khóa.

Product / Operational / Commerce boundary đã khóa.

No-bypass rule đã khóa.

OP-M01 -> OP-M21 module contracts đã khóa.

State machine cho raw lot, PO, snapshot, material, process, QR, batch, warehouse, ledger, trace, public trace, recall, CAPA, sale lock đã khóa.

Command-query boundary đã khóa.

Inventory Ledger append-only rule đã khóa.

Trace/Public Trace boundary đã khóa.

Recall/Sale Lock priority đã khóa.

Downstream handoff đã khóa.

Runtime unavailable fail-safe đã khóa.

Final Smoke Matrix đã có.

Evidence Package đã có.

Owner Review Matrix đã có.

Security/Public Trace Review đã có.

Release Handoff Package đã có.

Không có đoạn nào gọi Documentation Complete là Production Ready.

Không có đoạn nào cho phép downstream vượt Operational Core.

Không có đoạn nào cho phép Product Active/SKU Active tự trở thành Sellable.

Không có đoạn nào cho phép QC_PASS tự trở thành RELEASED.

Không có đoạn nào cho phép Public Trace ngoài whitelist.

## 118. TECH-03 FINAL FAIL GATE

## 118.1. Điều kiện làm TECH-03 FAIL

TECH-03 FAIL nếu có bất kỳ điểm nào sau:

Operational Core bị hiểu là module kho đơn giản.

Product Active/SKU Active bị hiểu là Sellable.

Raw lot QC_PASS tự động READY_FOR_PRODUCTION.

Raw lot chưa READY_FOR_PRODUCTION vẫn issue được.

Material Issue không phải điểm giảm tồn chính.

Material Receipt trừ tồn lần hai.

Production Order không snapshot formula.

Formula Snapshot bị ghi đè bởi recipe live.

Batch QC_PASS tự động RELEASED.

Warehouse nhận batch chưa RELEASED.

Finished goods stock tăng trước warehouse receipt confirmed.

Inventory Ledger không append-only.

Balance được sửa trực tiếp không movement.

QR VOID/FAILED vẫn public-valid.

PACKET có QR public trace standalone trái rule packaging đã khóa.

Trace chain thiếu vẫn public trace.

Public Trace lộ dữ liệu nội bộ.

Recall active nhưng không chặn Commerce/AI/Ads/Live/CRM.

Sale Lock active nhưng downstream vẫn bán.

Runtime unavailable nhưng downstream vẫn pass.

Evidence chưa accepted nhưng dùng để PASS.

Không có smoke pass nhưng gọi Release Ready.

Không có owner sign-off nhưng gọi Release Approved.

Không có release decision nhưng gọi Go-live Approved.

Downstream được phép phụ thuộc production vào Operational Core chưa clear.

## 119. P0 điểm chặn REGISTRY - FINAL

## 119.1. Danh sách P0 điểm chặn cuối

## P0 ID

điểm chặn

Gate bị chặn

## OP-P0-FINAL-001

Foundation permission/audit/idempotency chưa clear

Operational Release

## OP-P0-FINAL-002

Product/SKU/Recipe/Formula dependency chưa clear

Production Order

## OP-P0-FINAL-003

Raw lot QC_PASS tự ready

Material Issue

## OP-P0-FINAL-004

Lot chưa ready vẫn issue

Production

## OP-P0-FINAL-005

Material Issue retry trừ kho hai lần

Inventory

## OP-P0-FINAL-006

Material Receipt trừ tồn lần hai

Inventory

## OP-P0-FINAL-007

PO không snapshot formula

Production / Trace

## OP-P0-FINAL-008

Snapshot bị ghi đè

Historical correctness

## OP-P0-FINAL-009

Batch QC_PASS tự released

Warehouse

## OP-P0-FINAL-010

Warehouse nhận batch chưa RELEASED

Finished goods stock

## OP-P0-FINAL-011

Finished goods stock tăng trước receipt

Commerce dependency

## OP-P0-FINAL-012

Ledger không append-only

Inventory / MISA / Evidence

## OP-P0-FINAL-013

QR VOID/FAILED public-valid

Public Trace

## OP-P0-FINAL-014

Public trace lộ internal data

Security / Public release

## OP-P0-FINAL-015

Trace chain thiếu vẫn pass

Public Trace / Recall

## OP-P0-FINAL-016

Recall không chặn downstream

Commerce / AI / Ads / Live

## OP-P0-FINAL-017

Sale Lock không thắng downstream

Commerce / AI / Ads / Live / CRM

## OP-P0-FINAL-018

Runtime unavailable nhưng vẫn bán

Production go-live

## OP-P0-FINAL-019

Evidence chưa accepted nhưng dùng PASS

Completion / Release

## OP-P0-FINAL-020

Smoke recall/sale lock thiếu

Production sale

## OP-P0-FINAL-021

MISA missing mapping success giả nếu scope có

MISA release

## OP-P0-FINAL-022

Downstream bypass Operational Handoff

Global Gateway

## 120.1. Trạng thái sau khi hoàn tất TECH-03

Hạng mục

Trạng thái

## TECH-03 PHẦN 1/4

## WRITTEN_FOR_REVIEW

## TECH-03 PHẦN 2/4

## WRITTEN_FOR_REVIEW

## TECH-03 PHẦN 3/4

## WRITTEN_FOR_REVIEW

## TECH-03 PHẦN 4/4

## WRITTEN_FOR_REVIEW

## TECH-03 FULL DOCUMENT

## DOCUMENTATION_COMPLETE_AFTER_OWNER_REVIEW

## OPERATIONAL_TECHNICAL_SPEC

## LOCKED_DRAFT

## OPERATIONAL_MODULE_CONTRACTS

## DEFINED

## OPERATIONAL_STATE_MACHINE

## DEFINED

## OPERATIONAL_SMOKE_MATRIX

## DEFINED

## OPERATIONAL_EVIDENCE_PACKAGE

## DEFINED

## OPERATIONAL_RELEASE_HANDOFF

## DEFINED

## IMPLEMENTATION_STATUS

## WAITING

## TEST_STATUS

## WAITING

## SMOKE_STATUS

## WAITING

## EVIDENCE_STATUS

## WAITING_ACCEPTED_EVIDENCE

## OWNER_SIGN_OFF

## WAITING

## COMPLETION_PASS

KHONG

## RELEASE_READY

KHONG

## RELEASE_DECISION_ACCEPTED

KHONG

## PRODUCTION_READINESS

KHONG

## GO_LIVE_DECISION_ACCEPTED

KHONG

bị chặn

## 121. DOWNSTREAM STATUS SAU TECH-03

## 121.1. TECH-04 Commerce

Hạng mục

Trạng thái

TECH-04 có thể viết tiếp tài liệu

CO

TECH-04 có thể code production ngay

KHONG

TECH-04 phải phụ thuộc Operational Sellable Signal

CO

TECH-04 được tự tính Operational Sellable

KHONG

TECH-04 được vượt Recall/Sale Lock

KHONG

## 121.2. AI / Ads / Live / CRM

Hạng mục

Trạng thái

Có thể viết tiếp tài liệu downstream

CO

Được chốt bán nếu Operational chưa clear

KHONG

Được nói còn hàng khi runtime unavailable

KHONG

Được public tồn kho chi tiết

KHONG

Được vượt sale lock/recall

KHONG

Được dùng Public Trace ngoài whitelist

KHONG

## 121.3. Public Trace

Hạng mục

Trạng thái

Public Trace boundary đã xác định

CO

Public Trace có thể dùng Internal Trace raw

KHONG

QR VOID/FAILED public-valid

KHONG

Trace chain thiếu vẫn valid

KHONG

Whitelist/security review bắt buộc

CO

## 121.4. MISA nếu scope có

Hạng mục

Trạng thái

MISA handoff boundary đã xác định

CO

MISA API/DB chi tiết đã thiết kế

KHONG

Missing mapping được xem là success

KHONG

Reconcile evidence bắt buộc

CO

MISA được thay Operational truth

KHONG

## 122. IMPLEMENTATION READINESS NOTE

## 122.1. TECH-03 chưa cho phép code sâu ngay

TECH-03 là tài liệu kỹ thuật nền.

Sau TECH-03, chưa nên nhảy vào code sâu nếu chưa có:

TECH-04 Commerce.

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

TECH-07 Ads.

TECH-08 MC AI Live.

TECH-09 IVR nếu scope triển khai.

TECH-10 Completion/Evidence/Release.

Mapping repository/module.

Gap report.

Implementation wave plan.

Test plan chi tiết.

Seed/config plan.

Environment plan.

Owner sign-off.

## 122.2. Việc code phải đi theo wave

Operational Core thuộc:

Wave 2 - Operational Core

Wave 2 chỉ nên bắt đầu sau:

Wave 0 Foundation clear.

Wave 1 Product/SKU/Recipe clear.

TECH-03 accepted for implementation planning.

Module map OP-M01 -> OP-M21 rõ.

P0 điểm chặn registry rõ.

Evidence/smoke plan rõ.

## 123. TECH-03 FINAL RELEASE HANDOFF STATEMENT

## 123.1. Statement bàn giao

TECH-03 bàn giao cho các tài liệu tiếp theo với thông điệp:

Operational Core là nguồn sự thật vận hành của sản xuất, kho, tồn, trace, recall và sale lock.

Mọi downstream muốn bán, tư vấn, chạy ads, live, CRM, public trace hoặc đồng bộ MISA phải tôn trọng Operational Core.

Không downstream nào được tự tạo sự thật vận hành.

## 124. TECH-03 FINAL CONCLUSION

TECH-03 đã khóa đầy đủ tầng Operational Core cho Ginsengfood theo hướng greenfield.

Tài liệu đã xác định rõ:

Operational Core đi sau TECH-02 Product.

Operational Core đi trước TECH-04 Commerce.

Product Active/SKU Active không đồng nghĩa Sellable.

QC_PASS không đồng nghĩa RELEASED.

Raw lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt không được giảm tồn lần hai.

Production Order phải snapshot formula.

Batch QC_PASS không đồng nghĩa Batch RELEASED.

Warehouse chỉ nhận batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Inventory Ledger phải append-only.

Trace chain phải đầy đủ.

Public Trace phải whitelist-only.

QR VOID/FAILED không public-valid.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM.

Runtime unavailable thì fail-safe.

Evidence chưa ACCEPTED thì không PASS.

Smoke chưa PASS thì không Release Ready.

Owner chưa sign-off thì không Release Approved.

Chưa có release decision thì không Go-live Approved.

Kết luận cuối cùng:

TECH-03 hoàn tất ở cấp tài liệu kỹ thuật nền, đủ để chuyển sang review và làm cơ sở viết TECH-04 Commerce Runtime.

Nhưng TECH-03 hiện vẫn là:

Operational Core chỉ được production-ready khi đã có implementation thật, test thật, smoke thật, evidence accepted, owner sign-off và release decision hợp lệ.
