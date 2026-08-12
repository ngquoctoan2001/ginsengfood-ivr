# MASTER-03 - TRACEABILITY ID

## Mục Đích

MASTER-03 khóa chuẩn định danh truy vết cho toàn hệ. Mọi quyết định nghiệp vụ, trạng thái vận hành, bằng chứng, smoke, audit và handoff phải có ID đủ rõ để truy ngược về source, owner và hành động phát sinh.

MASTER-03 không thiết kế schema chi tiết. File này chỉ khóa nguyên tắc đặt ID và quan hệ ID.

## Nguyên Tắc ID

1. ID nghiệp vụ phải ổn định và không phụ thuộc UI.
2. ID kỹ thuật tạm thời không được dùng làm canonical business ID.
3. ID public phải qua whitelist.
4. ID internal không được lộ ra khách hàng nếu chưa có public mapping.
5. Mỗi evidence phải link được về action, owner, source và timestamp.
6. Mỗi smoke phải link được về rule kiểm chứng.

## Nhóm ID Chuẩn

| Prefix | Nhóm | Ví dụ | Public default |
| --- | --- | --- | --- |
| MAS | MASTER governance | MAS-SOT-001 | Không |
| PK | Pack rule | PK01-OPR-001 | Không |
| PRD | Product / SKU | PRD-SKU-001 | Theo mapping |
| ING | Ingredient / UOM | ING-UOM-001 | Không |
| SRC | Source / Supplier | SRC-ZONE-001 | Theo whitelist |
| LOT | Raw material lot | LOT-RAW-001 | Không |
| QC | Quality control | QC-RAW-001 | Không |
| PO | Production order | PO-2026-0001 | Không |
| MI | Material issue | MI-2026-0001 | Không |
| BAT | Finished batch | BAT-2026-0001 | Theo public batch code |
| WH | Warehouse receipt | WHR-2026-0001 | Không |
| INV | Inventory ledger | INV-LEDGER-001 | Không |
| TRC | Traceability | TRC-BATCH-001 | Có nếu whitelist |
| QR | QR / print code | QR-BOX-001 | Có nếu valid |
| RCL | Recall / sale lock | RCL-2026-001 | Theo scope public |
| EVD | Evidence | EVD-SMK-001 | Không |
| SMK | Smoke case | SMK-OPR-001 | Không |

## Quan Hệ ID Bắt Buộc

```text
Product ID
  -> SKU ID
  -> Recipe ID / Formula Version ID
  -> Production Order ID
  -> Material Issue ID
  -> Batch ID
  -> Warehouse Receipt ID
  -> Inventory Ledger Entry ID
  -> Trace ID / QR ID
  -> Recall or Sale Lock ID if any
```

## Traceability Cho Operational Core

PACK-01 phải bảo đảm:

1. Raw material lot trace được về source, supplier hoặc company source.
2. Production Order trace được về SKU và formula version.
3. Material Issue trace được về lot và production order.
4. Batch trace được về material issue, process, QC và release.
5. Warehouse receipt trace được về released batch.
6. Inventory ledger trace được về receipt hoặc issue event.
7. QR trace được về batch, package level và print payload.
8. Recall / Sale Lock trace được về batch, SKU, lot hoặc trade item scope.

## Public Trace Boundary

| Dữ liệu | Public default | Ghi chú |
| --- | --- | --- |
| Public product name | Có thể public | Chỉ bản đã duyệt |
| Batch public code | Có thể public | Không dùng internal batch ID nếu chưa mapped |
| MFG/HSD | Có thể public | Theo template |
| Source public text | Có thể public | Wording đã duyệt |
| Internal lot code | Không public | Chỉ dùng nội bộ |
| QC internal note | Không public | Chỉ evidence nội bộ |
| Cost / MISA / accounting | Không public | Không thuộc trace public |
| Staff / audit actor | Không public | Chỉ audit nội bộ |

## ID Cho Evidence Và Smoke

Evidence ID phải chỉ ra:

1. Rule được kiểm chứng.
2. Pack hoặc phase liên quan.
3. Artifact hoặc log chứng minh.
4. Người hoặc hệ thống tạo evidence.
5. Thời điểm tạo.
6. Kết luận review.

Smoke ID phải gắn với rule. Ví dụ `SMK-OPR-RELEASE-001` kiểm Batch QC_PASS không tự thành RELEASED.

## Event Và Audit ID

| Event | ID cần có | Audit requirement |
| --- | --- | --- |
| Product activation | Product ID, actor ID, reason ID | Có |
| Formula version approval | Formula ID, version ID, owner decision ID | Có |
| Raw lot QC | Lot ID, QC ID, evidence ID | Có |
| Material issue | Issue ID, lot ID, production order ID | Có |
| Batch release | Batch ID, release decision ID | Có |
| Warehouse receipt | Receipt ID, batch ID, location ID | Có |
| Sale lock | Lock ID, scope ID, owner decision ID | Có |
| Reprint | Print ID, reprint reason ID, actor ID | Có |
| MISA sync | Handoff ID, sync attempt ID, reconcile ID | Có |

## ID Conflict Rules

1. Không tái sử dụng ID đã public.
2. Không sửa lịch sử ID trong ledger.
3. Không dùng QR void/failed làm public-valid.
4. Không tạo barcode thương mại thứ hai cho cùng trade item nếu registry đã active.
5. Reprint phải tạo log mới, không ghi đè print log cũ.
6. Temporary ID phải được đổi sang canonical ID trước khi dùng trong evidence.

## Done Gate Của MASTER-03

MASTER-03 đạt yêu cầu khi:

1. Prefix ID đủ dùng cho MASTER, PACK, PHASE và evidence.
2. Operational trace chain rõ từ source đến sellable.
3. Public trace boundary rõ.
4. Smoke và evidence có ID riêng.
5. Rule chống sửa lịch sử, public nhầm hoặc reuse ID đã được khóa.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-03 — Traceability Id Standard


## Global Id / Correlation / Idempotency / Evidence Trace / Cross-Pack Traceability Control


## PHẦN 1/4 — ID PRINCIPLES / TRACEABILITY MODEL / ID OWNERSHIP / ID BOUNDARY


## 1. MỤC ĐÍCH CỦA MASTER-03

MASTER-03 quy định chuẩn định danh truy vết xuyên hệ thống Ginsengfood.

Mục tiêu của MASTER-03 là bảo đảm mọi quyết định nghiệp vụ, mọi sự kiện runtime, mọi hành động customer-facing, mọi bằng chứng kiểm thử, mọi trạng thái release và mọi quan hệ phụ thuộc giữa các pack đều có ID rõ ràng, có owner rõ ràng, có đường truy vết rõ ràng và không bị đứt mạch khi dữ liệu đi qua nhiều pack khác nhau.

MASTER-03 không phải tài liệu thiết kế database, không phải tài liệu API, không phải tài liệu DTO, không phải tài liệu UI, không phải tài liệu worker và không phải tài liệu triển khai code.

MASTER-03 là tài liệu governance kỹ thuật dùng để khóa nguyên tắc định danh, quyền sở hữu ID, quan hệ trace, ranh giới phát sinh ID, tính bất biến, correlation, idempotency, evidence trace và audit relationship cho toàn bộ hệ thống Ginsengfood.


## 2. VAI TRÒ CỦA MASTER-03 TRONG BỘ MASTER

MASTER-03 đứng sau MASTER-00, MASTER-01 và MASTER-02.

MASTER-00 quy định cấu trúc governance tổng thể.

MASTER-01 quy định Source-of-Truth, Owner Core, Runtime Resolver, Consumer Boundary, Guard, Evidence và Release Gate.

MASTER-02 quy định dependency giữa các pack, các business domain P0, các quan hệ runtime, rule bị chặn-if-missing, suppression, fallback và no-hardcode.

MASTER-03 quy định chuẩn ID để các dependency đã khóa trong MASTER-02 có thể được truy vết, kiểm chứng, audit, smoke test và release một cách nhất quán.

MASTER-03 bắt buộc được dùng làm chuẩn khi các pack sau thiết kế chi tiết database, API, DTO, UI, worker, event, log, evidence, smoke test và release gate.


## 3. NGUYÊN TẮC CỐT LÕI CỦA TRACEABILITY ID


### 3.1. Mọi quyết định quan trọng phải có ID truy vết

Hệ thống Ginsengfood bắt buộc mọi quyết định nghiệp vụ quan trọng phải có ID truy vết.

Các quyết định liên quan đến khách hàng, tư vấn, giá, chương trình, mở bán, quote, order, thanh toán, vận chuyển, CRM, Diamond, affiliate, khiếu nại, privacy, moderation, evidence, smoke test và release gate không được tồn tại dưới dạng dữ liệu rời rạc không có định danh.

Nếu một quyết định có thể ảnh hưởng đến khách hàng, doanh thu, quyền lợi thành viên, hoa hồng Diamond, trạng thái đơn, trạng thái thanh toán, trạng thái vận chuyển, claim public, Gateway hoặc Go-live, quyết định đó bắt buộc phải có ID.


### 3.2. ID phải có Owner Core

Mỗi loại ID trong hệ thống Ginsengfood bắt buộc phải có Owner Core.

Owner Core là nơi duy nhất được quyền tạo, quản lý vòng đời, xác định trạng thái, khóa tính hợp lệ và cung cấp ID cho các Consumer Pack sử dụng.

Consumer Pack không được tự tạo ID thay cho Owner Core.

Consumer Pack không được suy luận ID từ dữ liệu text, tên khách, số điện thoại, nội dung comment, mã sản phẩm, giờ live, mã chương trình hoặc nội dung AI trả lời.

Consumer Pack chỉ được sử dụng ID đã được Owner Core hoặc Runtime Resolver hợp lệ cung cấp.


### 3.3. ID phải bất biến sau khi phát sinh

ID trong hệ thống Ginsengfood là định danh truy vết, không phải nhãn hiển thị tùy ý.

Sau khi ID được phát sinh hợp lệ, ID không được sửa, không được tái sử dụng, không được gán lại cho đối tượng khác và không được thay đổi ý nghĩa.

Nếu một đối tượng nghiệp vụ bị hủy, thay thế, rollback, void, reissue hoặc superseded, hệ thống phải tạo quan hệ trace giữa ID cũ và ID mới, không được ghi đè ID cũ.

Nguyên tắc bắt buộc:

Không đổi ID để sửa lỗi nghiệp vụ.

Không tái sử dụng ID đã void.

Không dùng lại ID của event cũ cho event mới.

Không xóa ID đã phát sinh nếu ID đó có liên quan đến audit, evidence, customer action, payment, order, complaint, moderation hoặc release gate.

Không làm mất quan hệ giữa ID gốc và ID thay thế.


### 3.4. ID không được chứa dữ liệu nhạy cảm

ID không được chứa thông tin cá nhân hoặc dữ liệu nhạy cảm.

Không được dùng số điện thoại, email, địa chỉ, tên khách hàng, mã số thuế, tài khoản ngân hàng, số CCCD, nội dung bệnh lý, nội dung khiếu nại, nội dung chuyển khoản hoặc thông tin nội bộ nhạy cảm làm ID chính.

Các dữ liệu nhạy cảm nếu cần liên kết phải được quản lý bởi Owner Core phù hợp và chỉ được tham chiếu thông qua ID hợp lệ.

Nguyên tắc này áp dụng cho toàn bộ:

Customer Identity.

CRM.

Payment.

Bank Transfer.

Accounting Review.

Shipping.

Complaint.

Privacy.

Health Guard.

Counterfeit.

Moderation.

Gateway.

Evidence.

Audit.


### 3.5. ID không được hardcode trong Consumer

Không pack nào được hardcode ID runtime.

AI Advisor, Gateway, CRM, Landing Page, Admin UI, MC AI, ADS, IVR, template nội dung, script live, content block và frontend không được hardcode các ID runtime như chương trình đang chạy, phiên Giờ Vàng, quote, order draft, payment reference, bank transfer queue, shipping tracking, handoff event, customer memory snapshot, member lifecycle event hoặc completion gate.

Consumer phải nhận ID từ Runtime Resolver, Owner Core hoặc approved runtime context.

Nếu thiếu ID bắt buộc, Consumer phải block, fallback hoặc handoff theo rule đã khóa; không được tự tạo ID thay thế.


### 3.6. ID phải phục vụ audit, không chỉ phục vụ hiển thị

ID trong Ginsengfood không chỉ dùng để hiển thị cho người dùng hoặc để nối dữ liệu kỹ thuật.

ID phải phục vụ audit nghiệp vụ.

Một ID hợp lệ phải cho phép truy ngược:

Ai hoặc core nào phát sinh ID.

ID phát sinh trong ngữ cảnh nào.

ID liên kết với domain nào.

ID liên kết với sự kiện nào.

ID liên kết với quyết định nào.

ID liên kết với evidence nào.

ID liên kết với audit nào.

ID liên kết với smoke nào.

ID liên kết với release gate nào nếu có.

ID có bị void, superseded, disputed, failed, bị chặn hoặc closed hay không.


## 4. MÔ HÌNH TRACEABILITY TỔNG THỂ


### 4.1. Traceability không phải chỉ là mã đơn hàng

Trong Ginsengfood, traceability không chỉ là truy xuất nguồn gốc sản phẩm hoặc mã đơn hàng.

Traceability là khả năng truy vết toàn bộ chuỗi:

Nguồn dữ liệu -> rule nghiệp vụ -> dependency -> runtime resolver -> guard -> quyết định -> hành động -> phản hồi khách hàng -> bằng chứng -> audit -> release gate.

Vì vậy, chuẩn ID của MASTER-03 áp dụng cho toàn bộ hệ thống, không chỉ áp dụng cho sản xuất, kho, order hoặc QR.


### 4.2. Chuỗi trace chuẩn

Mỗi nghiệp vụ P0 trong Ginsengfood phải được thiết kế theo chuỗi trace chuẩn sau:

Source TraceDữ liệu bắt nguồn từ Source-of-Truth nào.

Dependency TraceNghiệp vụ đó phụ thuộc vào domain, pack và Owner Core nào.

Runtime TraceRuntime Resolver nào đã resolve dữ liệu tại thời điểm thực thi.

Decision TraceGuard nào đã pass, block, suppress, fallback hoặc handoff.

Action TraceAction nào được thực hiện: gửi tư vấn, tạo quote, tạo order draft, handoff Messenger, tạo CRM job, tạo payment reference, tạo shipping tracking, tạo complaint case.

Evidence TraceBằng chứng nào chứng minh action hoặc dependency đã hoạt động đúng.

Audit TraceAudit nào ghi nhận trạng thái, người/cơ chế thực hiện, thời điểm, kết quả, lý do, quan hệ trước/sau.

Release TraceGate nào được xét, gate nào còn chưa đủ điều kiện, gate nào pass, gate nào block.


### 4.3. Mô hình quan hệ ID xuyên pack

MASTER-03 quy định mô hình quan hệ ID xuyên pack theo nguyên tắc:

Một nghiệp vụ không đứng một mình.

Ví dụ: báo giá cho khách không chỉ có quote_snapshot_id.

Một báo giá hợp lệ có thể cần liên kết với:

customer_identity_resolution_id

customer_memory_snapshot_id

program_activation_id

product_activation_id

golden_hour_session_id nếu thuộc Giờ Vàng

early_access_window_id nếu có quyền mua sớm

quote_snapshot_id

quote_cart_id

audit_id

evidence_id nếu dùng cho smoke/release

Tương tự, một đơn hàng hợp lệ không chỉ có order_code.

Một đơn hàng hợp lệ phải có trace tối thiểu từ:

order_confirmation_draft_id

customer_confirmation_id

order_code

payment_reference nếu có thanh toán

bank_transfer_queue_id nếu chuyển khoản

accounting_review_id nếu cần kế toán xác nhận

shipping_tracking_event_id nếu phát sinh vận chuyển


## 5. PHÂN LOẠI ID TRONG HỆ THỐNG GINSENGFOOD

MASTER-03 phân loại ID theo vai trò governance, không theo cấu trúc database.


### 5.1. Governance ID

Governance ID dùng để quản trị nguồn, dependency, smoke, evidence, sign-off và completion gate.

Nhóm này bao gồm:

source_id

dependency_id

domain_id

smoke_id

evidence_id

owner_signoff_id

completion_gate_id

Governance ID không được dùng thay cho ID nghiệp vụ runtime.

Ví dụ: smoke_id chỉ chứng minh một smoke case; không được dùng thay cho order_code, quote_snapshot_id hoặc payment_reference.


### 5.2. Customer Context ID

Customer Context ID dùng để truy vết nhận diện khách hàng, ngữ cảnh khách hàng, lịch sử mua, chăm sóc trước bán tiếp và snapshot phục vụ tư vấn.

Customer Context ID bắt buộc phải được resolve trước khi hệ thống cá nhân hóa giá, quyền lợi, CRM, Diamond, affiliate, chăm sóc sau mua hoặc đề xuất nâng hạng.

AI hoặc Consumer không được tự suy luận khách là khách mới, member, Diamond, buyer từ link Diamond hoặc khách có open case nếu chưa có Customer Context ID hợp lệ.


### 5.3. CRM / Message Trigger ID

CRM / Message Trigger ID dùng để truy vết chăm sóc vòng đời, message trigger, job gửi tin, suppression, dedup và immutable message text.

crm_message_job_id

message_trigger_id

dedup_key

idempotency_key

correlation_id

CRM không được gửi nếu thiếu trigger hợp lệ, thiếu customer context, vi phạm suppression, quiet hours, frequency cap hoặc có priority conflict.


### 5.4. Member Lifecycle ID

Member Lifecycle ID dùng để truy vết chu kỳ thành viên, duy trì hạng, ân hạng, hạ hạng, tranh chấp quyền lợi và lịch sử thay đổi quyền lợi.

member_lifecycle_event_id

member_grace_event_id

member_dispute_case_id

AI, CRM, Gateway hoặc Landing Page không được tự tính hạng, doanh số, số còn thiếu hoặc quyền lợi nếu thiếu Member Lifecycle ID và dữ liệu runtime hợp lệ từ Owner Core.


### 5.5. Diamond / Affiliate ID

Diamond / Affiliate ID dùng để truy vết referral link, bind, hoa hồng, eligibility, conflict giữa Ads/Diamond và tranh chấp commission.

diamond_referral_link_id

diamond_bind_id

commission_event_id

Diamond/Affiliate/khởi nghiệp là domain riêng, không được nhầm với đại lý, nhà phân phối hoặc mua sỉ.

Consumer không được tự gán commission, không được tự nhận bind, không được hardcode tỷ lệ hoa hồng và không được route sai sang domain đại lý.


### 5.6. Official Contact ID

Official Contact ID dùng để truy vết số điện thoại/chân dung kênh liên hệ chính thức được phép hiển thị theo từng intent.

official_contact_id

Không Consumer nào được tự gửi số điện thoại nếu chưa resolve qua Official Contact Registry.

Không được cung cấp số cá nhân lãnh đạo.


### 5.7. Gateway / Public Comment / Handoff ID

Gateway ID dùng để truy vết comment public, handoff Messenger, delivery log, moderation, trạng thái gửi thành công/thất bại và rule không lặp PII.

public_comment_event_id

messenger_handoff_event_id

handoff_delivery_log_id

live_moderation_event_id

Public comment/live không được báo giá, không chốt đơn, không lặp PII, không hướng dẫn payment và không tư vấn health detail dài.

Chỉ khi handoff thành công mới được nói đã gửi Messenger.

Nếu handoff thất bại, hệ thống không được nói đã gửi.


### 5.8. Recommendation ID

Recommendation ID dùng để truy vết gợi ý segment, gợi ý bộ ba sản phẩm, Product Effectiveness và logic tư vấn theo mùa/chức năng/bổ dưỡng/người thân.

segment_recommendation_id

product_triad_recommendation_id

Gợi ý sâu không được là nội dung tự do rời rạc.

Mỗi recommendation quan trọng phải trace được vì sao chọn sản phẩm đó, dựa trên segment nào, nhu cầu nào, customer context nào, product activation nào, health boundary nào và guard nào.


### 5.9. Program / Sellable / Activation ID

Program / Sellable / Activation ID dùng để truy vết trạng thái mở bán, chương trình 24/7, Giờ Vàng, sản phẩm active, board live, script MC AI và quyền mua sớm.

golden_hour_session_id

early_access_window_id

daily_live_product_board_id

mc_ai_script_brief_id

AI, Gateway, CRM, Landing, ADS và MC AI chỉ được nói/bán SKU đang active, sellable, có giá hợp lệ, đúng board, đúng chương trình và đúng kênh.

Không được nói/bán SKU hết hàng, bị hold, recall, sale lock, channel block, price inactive hoặc sellable_status không hợp lệ.


### 5.10. Quote / Cart / Order ID

Quote / Cart / Order ID dùng để truy vết báo giá, giỏ quote, bản nháp xác nhận đơn, xác nhận khách hàng và mã đơn chính thức.

Báo giá bắt buộc phải có QuoteSnapshot.

Tạo đơn bắt buộc đi qua OrderDraft.

Order Success chỉ hợp lệ khi có order_code.

IVR chỉ được xác nhận đơn theo phạm vi ORDER_CONFIRMATION_ONLY, không được tự đổi order state.


### 5.11. Payment / Accounting ID

Payment / Accounting ID dùng để truy vết thanh toán, chuyển khoản, hàng chờ đối soát, kế toán xác nhận và trạng thái không tự động PAID.

payment_reference

bank_transfer_queue_id

accounting_review_id

Chuyển khoản phải có payment_reference.

Ảnh giao dịch hoặc khách nói đã chuyển không tự động là PAID.

PAID chỉ hợp lệ khi Payment Core hoặc Accounting Review xác nhận.

Không Consumer nào được hardcode tài khoản ngân hàng hoặc tự tạo hướng dẫn thanh toán ngoài Bank Registry.


### 5.12. Shipping ID

Shipping ID dùng để truy vết trạng thái vận chuyển, tracking, ETA, COD, phí ship và event giao hàng.

shipping_tracking_event_id

ETA, COD, phí ship và tracking phải từ Shipping Core.

Không hỏi lại địa chỉ nếu order đã có shipping info hợp lệ.

Không bịa trạng thái vận chuyển.


### 5.13. Guard / Case ID

Guard / Case ID dùng để truy vết các tình huống ưu tiên cao hơn sales và CRM.

health_guard_case_id

complaint_case_id

privacy_case_id

counterfeit_case_id

Khi có complaint, refund, privacy, payment dispute, delivery issue, health review, counterfeit, member dispute hoặc commission dispute, các case ID này có quyền ưu tiên cao hơn sales, CRM và upsell.


### 5.14. Control ID

Control ID dùng để kiểm soát correlation, idempotency, dedup, audit và release.

Control ID không thay thế business ID.

Control ID giúp bảo đảm cùng một event không bị xử lý trùng, cùng một action không bị gửi lặp, cùng một customer flow không bị đứt trace, cùng một release gate không bị pass khi thiếu evidence.


## 6. NGUYÊN TẮC ID OWNERSHIP


### 6.1. Owner Core là nguồn tạo ID chính thức

Mỗi ID phải thuộc một Owner Core duy nhất.

Owner Core chịu trách nhiệm:

Quy định điều kiện phát sinh ID.

Tạo ID hợp lệ.

Đảm bảo ID không trùng.

Quản lý trạng thái ID.

Quản lý quan hệ ID cha/con.

Quản lý vòng đời ID.

Quản lý void/reissue/supersede nếu có.

Cung cấp ID cho Runtime Resolver hoặc Consumer.

Ghi audit khi ID được tạo, dùng, khóa, void hoặc liên kết evidence.


### 6.2. Consumer chỉ được tiêu thụ ID trong boundary

Consumer Pack chỉ được dùng ID trong phạm vi được cấp quyền.

Consumer có thể lưu reference, hiển thị trạng thái được phép, gọi resolver được phép hoặc ghi action theo ID được cấp.

Consumer không được:

Tự tạo ID canonical.

Tự sửa ID canonical.

Tự map ID bằng suy luận.

Tự merge ID giữa các domain.

Tự thay ID khi lỗi.

Tự dùng ID của domain này làm ID của domain khác.

Tự chuyển trạng thái nghiệp vụ chỉ vì có ID.

Tự pass release gate chỉ vì đã có tài liệu.


### 6.3. Runtime Resolver là cổng cấp ID theo ngữ cảnh

Runtime Resolver chịu trách nhiệm trả về ID đúng tại thời điểm thực thi.

Runtime Resolver không chỉ trả về dữ liệu; Runtime Resolver phải trả về định danh truy vết đủ để Consumer biết dữ liệu đó đến từ đâu và có hợp lệ hay không.

Ví dụ:

Khi AI cần báo giá, Runtime Resolver phải trả về quote_snapshot_id, không chỉ trả về con số giá.

Khi Gateway cần kéo Messenger, Runtime Resolver phải trả về hoặc liên kết messenger_handoff_event_id, không chỉ trả về câu trả lời public.

Khi CRM cần gửi tin, Runtime Resolver phải liên kết crm_message_job_id, message_trigger_id, dedup_key, idempotency_key, không chỉ trả về nội dung tin nhắn.

Khi thanh toán chuyển khoản, Runtime Resolver phải liên kết payment_reference, không chỉ trả về hướng dẫn thanh toán.

Khi xét Gateway PASS, hệ thống phải có evidence_id, smoke_id, owner_signoff_id, completion_gate_id, không chỉ có checklist chữ.


## 7. ID BOUNDARY GIỮA CÁC PACK


### 7.1. Không pack nào được tạo ID làm đứt traceability

Mọi pack trong Ginsengfood phải tuân thủ nguyên tắc không tự tạo ID ngoài boundary.

Nếu một pack cần ID nhưng không phải Owner Core, pack đó phải gọi đúng Runtime Resolver hoặc nhận ID từ context đã được Owner Core cấp.

Nếu thiếu ID bắt buộc, pack phải block, fallback hoặc handoff theo governance.

Pack không được tự tạo ID tạm rồi biến thành ID chính thức.


### 7.2. ID tạm không được thay ID chính thức

Trong một số giai đoạn triển khai, hệ thống có thể có technical temporary identifier để vận hành nội bộ.

MASTER-03 quy định rõ: technical temporary identifier không phải canonical business ID.

Temporary identifier không được dùng để:

Báo giá chính thức.

Xác nhận đơn chính thức.

Xác nhận thanh toán.

Xác nhận quyền lợi thành viên.

Xác nhận hoa hồng Diamond.

Xác nhận shipping.

Xác nhận Gateway PASS.

Xác nhận Go-live.

Xác nhận evidence PASS.

Khi canonical ID được Owner Core phát sinh, mọi trace nghiệp vụ phải quay về canonical ID.


### 7.3. Public ID và Internal ID phải có boundary

Một số ID có thể được hiển thị cho khách hàng, ví dụ order_code hoặc payment_reference theo chính sách Payment Core.

Một số ID chỉ dùng nội bộ, ví dụ audit_id, completion_gate_id, evidence_id, messenger_handoff_event_id, customer_memory_snapshot_id, commission_event_id.

MASTER-03 quy định không được tự ý public hóa ID nội bộ.

Consumer chỉ được hiển thị ID nếu ID đó có public_view_policy hợp lệ từ Owner Core hoặc Source-of-Truth tương ứng.


## 8. NGUYÊN TẮC ID GENERATION


### 8.1. ID chỉ phát sinh khi điều kiện nghiệp vụ hợp lệ

ID không được phát sinh tùy tiện.

Mỗi ID phải có điều kiện phát sinh rõ.

quote_snapshot_id chỉ phát sinh khi đủ điều kiện báo giá.

order_confirmation_draft_id chỉ phát sinh khi có quote/cart hợp lệ để lập bản nháp xác nhận.

customer_confirmation_id chỉ phát sinh khi khách có hành động xác nhận hợp lệ.

order_code chỉ phát sinh khi đủ điều kiện tạo đơn.

payment_reference chỉ phát sinh theo Payment Core / Bank Registry.

accounting_review_id chỉ phát sinh khi cần kế toán đối soát.

messenger_handoff_event_id chỉ phát sinh khi Gateway thực hiện handoff.

crm_message_job_id chỉ phát sinh khi CRM trigger hợp lệ và không bị suppression.

completion_gate_id chỉ phát sinh khi có gate cần xét.


### 8.2. Không tạo ID để hợp thức hóa dữ liệu sai

ID không được dùng để hợp thức hóa nghiệp vụ sai.

Nếu một nghiệp vụ bị thiếu điều kiện, hệ thống không được tạo ID để đi tiếp cho đủ form.

Không tạo quote_snapshot_id nếu chưa resolve được giá runtime.

Không tạo order_code nếu chưa có customer_confirmation_id.

Không tạo payment_reference ngoài Payment Core.

Không tạo commission_event_id nếu Diamond bind chưa hợp lệ.

Không tạo shipping_tracking_event_id nếu chưa có shipping event hợp lệ.

Không tạo owner_signoff_id nếu Owner chưa sign-off.

Không tạo trạng thái Gateway PASS nếu completion_gate_id còn thiếu evidence.


## 9. NGUYÊN TẮC ID IMMUTABILITY


### 9.1. ID không được ghi đè

Hệ thống Ginsengfood bắt buộc giữ nguyên ID sau khi phát sinh.

Không được ghi đè ID để sửa lỗi dữ liệu, sửa luồng, đổi trạng thái hoặc làm đẹp báo cáo.

Mọi thay đổi phải đi qua event mới, snapshot mới, audit mới hoặc relation mới.


### 9.2. Snapshot ID phải giữ đúng lịch sử

Các ID dạng snapshot như customer_memory_snapshot_id, quote_snapshot_id, mc_ai_script_brief_id phải phản ánh trạng thái tại thời điểm snapshot được tạo.

Không được cập nhật snapshot cũ để khớp với dữ liệu mới.

Nếu dữ liệu thay đổi, phải tạo snapshot mới hoặc version mới và liên kết trace với snapshot trước đó nếu cần.


### 9.3. Event ID phải phản ánh event thật

Các ID dạng event như public_comment_event_id, messenger_handoff_event_id, shipping_tracking_event_id, member_lifecycle_event_id, commission_event_id, live_moderation_event_id phải phản ánh event đã xảy ra.

Không được dùng một event ID cho nhiều event khác nhau.

Không được tạo event giả để bù log.

Không được xóa event có liên quan đến audit, dispute, complaint, payment, shipping hoặc release evidence.


## 10. NGUYÊN TẮC CORRELATION


### 10.1. Correlation dùng để nối luồng, không thay thế business ID

correlation_id dùng để nối các bước trong cùng một luồng xử lý.

correlation_id không thay thế ID nghiệp vụ.

Một luồng khách comment hỏi giá trên live rồi được handoff sang Messenger, tư vấn, báo quote, tạo order draft và xác nhận đơn có thể có cùng một correlation_id.

Nhưng trong luồng đó vẫn phải có các ID nghiệp vụ riêng:


### 10.2. Correlation bắt buộc cho luồng đa pack

Bất kỳ luồng nào đi qua từ hai pack trở lên phải có correlation.

Các luồng bắt buộc có correlation gồm:

Public comment -> Messenger handoff -> AI tư vấn.

AI tư vấn -> QuoteSnapshot -> OrderDraft.

OrderDraft -> CustomerConfirmation -> Order.

Order -> Payment -> Accounting Review.

Order -> Shipping -> Tracking.

CRM trigger -> CRM message job -> customer response.

Diamond link -> bind -> order -> commission.

Complaint -> guard -> case -> resolution.

Evidence -> smoke -> completion gate -> owner sign-off.


## 11. NGUYÊN TẮC IDEMPOTENCY VÀ DEDUP


### 11.1. Idempotency bảo vệ hệ thống khỏi xử lý lặp

idempotency_key dùng để bảo đảm một hành động giống nhau không bị xử lý nhiều lần ngoài ý muốn.

Các action bắt buộc kiểm soát idempotency:

Tạo quote.

Tạo order draft.

Xác nhận đơn.

Tạo order.

Tạo payment reference.

Gửi CRM message.

Handoff Messenger.

Ghi event shipping.

Ghi complaint case.

Ghi commission event.

Ghi evidence.

Ghi owner sign-off.

Xét completion gate.


### 11.2. Dedup bảo vệ khách hàng khỏi bị gửi trùng

dedup_key dùng để chống gửi trùng, xử lý trùng hoặc trả lời trùng.

Các domain bắt buộc có dedup:

Public comment.

Messenger handoff.

Live moderation.

Payment notification.

Shipping event.

IVR confirmation.

Complaint intake.

Evidence upload.

Dedup không được xóa audit.

Nếu một event bị dedup, hệ thống vẫn phải có dấu vết vì sao event đó không được xử lý lại.


## 12. NGUYÊN TẮC EVIDENCE TRACE


### 12.1. Evidence phải có ID riêng

Mọi bằng chứng phục vụ smoke, test, release, Gateway, security, App Review, UAT, production readiness hoặc owner sign-off phải có evidence_id.

Evidence không được chỉ là file rời, ảnh rời, log rời hoặc ghi chú rời.

Evidence phải liên kết được với:

owner_signoff_id nếu có


### 12.2. Không có evidence thì không PASS

MASTER-03 khóa nguyên tắc:

Không có ID evidence hợp lệ thì không được coi là PASS.

Không có trace từ evidence đến smoke thì không được coi là PASS.

Không có trace từ smoke đến completion gate thì không được coi là Gateway PASS.

Không có owner sign-off hợp lệ thì không được coi là release approved.

Tài liệu viết xong không đồng nghĩa production ready.


## 13. NGUYÊN TẮC AUDIT RELATIONSHIP


### 13.1. Audit là lớp quan hệ bắt buộc

audit_id là lớp quan hệ bắt buộc cho các nghiệp vụ có ảnh hưởng đến khách hàng, tiền, quyền lợi, claim, order, payment, shipping, dispute, moderation, evidence và release.

Audit phải ghi nhận được:

ID nào được tạo.

ID nào được dùng.

ID nào được liên kết.

ID nào bị block.

ID nào bị fallback.

ID nào bị void.

ID nào bị superseded.

ID nào pass/fail.

Actor/Core/Resolver/Guard nào tạo quyết định.

Thời điểm quyết định.

Lý do quyết định.

Evidence liên quan nếu có.


### 13.2. Audit không được thay thế nghiệp vụ

Audit ghi nhận nghiệp vụ, không thay thế nghiệp vụ.

Có audit log không có nghĩa là nghiệp vụ hợp lệ.

Có audit về việc khách nói đã chuyển khoản không có nghĩa là PAID.

Có audit về việc tạo order draft không có nghĩa là order success.

Có audit về handoff attempt không có nghĩa là handoff success.

Có audit về smoke run không có nghĩa là smoke pass.

Có audit về owner review không có nghĩa là owner sign-off pass.


## 14. RANH GIỚI MASTER-03

MASTER-03 chỉ khóa chuẩn định danh và traceability governance.

MASTER-03 không quy định:

Cấu trúc bảng database.

Kiểu dữ liệu vật lý.

Endpoint API.

DTO request/response.

Giao diện Admin UI.

Worker implementation.

Message broker implementation.

Code sinh ID.

Thuật toán cụ thể tạo ID.

Chi tiết lưu trữ log.

Chi tiết bảo mật hạ tầng.

Các tài liệu implementation sau phải bám MASTER-03, nhưng không được hiểu MASTER-03 là bản thiết kế code.


## 15. KẾT LUẬN PHẦN 1/4


## PHẦN 1/4 của MASTER-03 khóa nguyên tắc nền cho toàn bộ chuẩn ID của hệ thống Ginsengfood.

Từ thời điểm áp dụng MASTER-03:

Mọi ID phải có Owner Core.

Mọi ID quan trọng phải có trace.

Consumer không được tự tạo ID ngoài boundary.

Runtime Resolver phải cấp ID theo ngữ cảnh hợp lệ.

ID phải bất biến sau khi phát sinh.

Snapshot phải giữ đúng lịch sử.

Event phải phản ánh event thật.

Correlation phải nối luồng đa pack.

Idempotency phải chống xử lý lặp.

Dedup phải chống gửi trùng.

Evidence phải có ID riêng.

Audit phải ghi quan hệ quyết định.

Không evidence thì không PASS.

Không owner sign-off thì không release.

Không completion gate PASS thì không Gateway PASS.

Tài liệu hoàn thành không đồng nghĩa production ready.

MASTER-03 tiếp tục ở PHẦN 2/4 với ID Registry theo từng domain, Owner Core, Consumer Pack và Required Trace Links.


## PHẦN 2/4 — ID REGISTRY BY DOMAIN / OWNER CORE / CONSUMER PACK / REQUIRED TRACE LINKS


## PHẦN 2/4 thiết lập registry định danh theo từng business domain đã được khóa từ MASTER-02, bao gồm ID chính, Owner Core, Consumer Pack và các trace link bắt buộc. MASTER-03 tiếp tục giữ đúng phạm vi governance, không thay thế database schema, API, DTO hoặc implementation.


## 16. NGUYÊN TẮC ĐỌC ID REGISTRY


### 16.1. ID Registry là chuẩn governance bắt buộc

ID Registry trong MASTER-03 là chuẩn bắt buộc để các pack của Ginsengfood cùng hiểu:

ID nào là canonical ID.

ID đó thuộc business domain nào.

Owner Core nào được quyền phát sinh ID.

Consumer Pack nào được quyền tiêu thụ ID.

ID đó bắt buộc phải trace đến những ID nào khác.

Khi thiếu ID thì domain nào phải block, fallback, suppress hoặc handoff.

ID Registry không phải danh sách tên cột database.

ID Registry không phải danh sách endpoint API.

ID Registry không phải contract DTO.

ID Registry là chuẩn kiểm soát định danh xuyên hệ thống.


### 16.2. Mỗi ID phải có 5 lớp governance

Mỗi ID trong MASTER-03 bắt buộc được hiểu qua 5 lớp:

Lớp 1 — Business MeaningID đại diện cho đối tượng, sự kiện, quyết định hoặc bằng chứng nào.

Lớp 2 — Owner CoreCore nào được quyền tạo, quản lý, void, supersede, close hoặc sign-off ID.

Lớp 3 — Consumer PackPack nào được quyền dùng ID để đọc, hiển thị, gửi message, lập quote, tạo order draft, kiểm tra payment, shipping hoặc evidence.

Lớp 4 — Required Trace LinksID đó bắt buộc phải liên kết với những ID nào để không đứt traceability.

Lớp 5 — bị chặn-if-Missing RuleNếu thiếu ID hoặc ID không hợp lệ thì Consumer phải dừng, fallback, suppress hoặc handoff như thế nào.


### 16.3. Không có ID thì không có quyết định hợp lệ

Không có ID hợp lệ thì không có quyết định hợp lệ.

Không có trace link hợp lệ thì không có audit hợp lệ.

Không có evidence ID hợp lệ thì không có PASS hợp lệ.

Không có owner sign-off ID hợp lệ thì không có release hợp lệ.

Không có completion gate ID hợp lệ thì không có Gateway PASS hợp lệ.


## 17. DOMAIN-01 — GOVERNANCE / SOURCE-OF-TRUTH


### 17.1. Mục tiêu định danh

Domain Governance / Source-of-Truth dùng để định danh nguồn sự thật, domain nghiệp vụ, quan hệ phụ thuộc, smoke test, evidence, audit, owner sign-off và completion gate.

Đây là lớp ID nền để toàn bộ hệ thống biết một rule, một dependency hoặc một gate đang dựa vào nguồn nào, domain nào và bằng chứng nào.


### 17.2. Canonical IDs

Domain này bao gồm các ID chính:


### 17.3. Owner Core

Owner Core của domain này gồm:

Master Governance Core.

Source-of-Truth Registry.

Dependency Registry.

Evidence Registry.

Smoke Registry.

Release Gate Core.

Owner Sign-off Core.

Audit Core.


### 17.4. Consumer Pack

Consumer Pack được quyền tiêu thụ ID trong domain này gồm:

AI Advisor Pack.

Gateway Pack.

CRM Pack.

Commerce Core Pack.

Payment Pack.

Shipping Pack.

Member Pack.

Diamond / Affiliate Pack.

Product Knowledge Pack.

Program / Sellable Pack.

Admin Governance Pack.

QA / Smoke / Evidence Pack.

Completion Report Pack.


### 17.5. Required Trace Links

Các trace link bắt buộc:

source_id phải liên kết với domain_id.

domain_id phải liên kết với dependency_id.

dependency_id phải liên kết với Consumer Pack đang dùng dependency đó.

smoke_id phải liên kết với dependency_id hoặc domain_id được smoke.

evidence_id phải liên kết với smoke_id, dependency_id, audit_id.

owner_signoff_id phải liên kết với evidence_id và completion_gate_id.

completion_gate_id phải liên kết với toàn bộ evidence_id, smoke_id, owner_signoff_id liên quan.

audit_id phải ghi nhận mọi thay đổi trạng thái của source, dependency, evidence, smoke và gate.


### 17.6. bị chặn-if-Missing Rule

Nếu thiếu source_id, Consumer không được xem dữ liệu đó là Source-of-Truth.

Nếu thiếu dependency_id, pack không được tuyên bố dependency đã được kiểm soát.

Nếu thiếu evidence_id, smoke không được tính PASS.

Nếu thiếu owner_signoff_id, release không được tính approved.

Nếu thiếu completion_gate_id, Gateway không được chuyển PASS.


## 18. DOMAIN-02 — PRODUCT KNOWLEDGE / CLAIM / BRAND / SCIENTIFIC PROOF


### 18.1. Mục tiêu định danh

Domain Product Knowledge / Claim / Brand / Scientific Proof dùng để truy vết tri thức sản phẩm, claim được phép dùng, brand wording, scientific proof và public-safe wording.

Domain này bảo đảm AI, CRM, Gateway, Landing Page, MC AI và content block không tự bịa claim, không tự thuốc hóa sản phẩm, không tự diễn giải sai công bố khoa học và không dùng nội dung ngoài Source-of-Truth.


### 18.2. Canonical IDs

Domain này sử dụng các ID liên quan:

Khi nội dung sản phẩm được dùng trong tư vấn hoặc recommendation, domain này phải liên kết thêm:

quote_snapshot_id nếu nội dung dẫn tới báo giá

order_confirmation_draft_id nếu nội dung dẫn tới xác nhận đơn


### 18.3. Owner Core

Product Knowledge Core.

Claim Whitelist Core.

Brand Voice Core.

Scientific Evidence Core.

Public Wording Guard.

Product Effectiveness Governance.


### 18.4. Consumer Pack

Consumer Pack gồm:

AI Advisor.

Landing Page.

ADS.

MC AI.

Content Block Pack.

Product Recommendation Pack.

Customer Care Pack.


### 18.5. Required Trace Links

Claim public phải trace về source_id.

Scientific proof phải trace về evidence_id.

Product Effectiveness phải trace về Product Knowledge Source và domain_id.

Nội dung tư vấn phải trace về correlation_id của phiên tư vấn.

Recommendation có dùng tri thức sản phẩm phải trace về product_triad_recommendation_id.

Nếu nội dung dẫn tới quote, phải trace tiếp đến quote_snapshot_id.

Nếu nội dung dẫn tới order draft, phải trace tiếp đến order_confirmation_draft_id.


### 18.6. bị chặn-if-Missing Rule

Nếu thiếu claim source, Consumer không được dùng claim.

Nếu thiếu scientific evidence approved, Consumer không được nói có bằng chứng khoa học theo hướng khẳng định.

Nếu thiếu public-safe wording, Consumer phải dùng fallback an toàn.

Nếu nội dung có dấu hiệu thuốc hóa, Consumer phải block hoặc chuyển Health Boundary Guard.


## 19. DOMAIN-03 — CUSTOMER IDENTITY / CUSTOMER MEMORY


### 19.1. Mục tiêu định danh

Domain Customer Identity / Customer Memory dùng để bảo đảm hệ thống biết đang nói chuyện với ai trước khi cá nhân hóa tư vấn, báo giá, quyền lợi thành viên, lịch sử mua, CRM, Diamond/referral hoặc chăm sóc sau mua.


### 19.2. Canonical IDs

Domain này bao gồm:

Khi liên quan đến member, Diamond, CRM, order hoặc complaint, phải liên kết thêm:


### 19.3. Owner Core

Owner Core gồm:

Customer Identity Core.

Customer Profile Core.

Customer Memory Core.

Customer Context Resolver.

Privacy Guard.


### 19.4. Consumer Pack

Order Pack.

Complaint Pack.


### 19.5. Required Trace Links

customer_identity_resolution_id phải liên kết với correlation_id.

customer_memory_snapshot_id phải liên kết với customer_identity_resolution_id.

Nếu khách cũ quay lại, memory snapshot phải trace được lần mua gần nhất, sản phẩm gần nhất và đối tượng sử dụng nếu có.

Nếu tư vấn cá nhân hóa, AI response phải trace về customer_memory_snapshot_id.

Nếu báo quyền lợi thành viên, phải trace thêm member_lifecycle_event_id.

Nếu khách đến từ link Diamond, phải trace thêm diamond_bind_id.

Nếu tạo quote, phải trace thêm quote_snapshot_id.

Nếu tạo order, phải trace thêm order_code.


### 19.6. bị chặn-if-Missing Rule

Nếu thiếu customer_identity_resolution_id, hệ thống không được cá nhân hóa quyền lợi, member tier, Diamond, CRM hoặc lịch sử mua.

Nếu thiếu customer_memory_snapshot_id, AI không được nhắc lịch sử mua cụ thể.

Nếu có privacy case mở, sales và CRM phải bị suppress.

Nếu customer context chưa resolve, quote/member/Diamond phải dừng hoặc yêu cầu xác minh trong boundary an toàn.


## 20. DOMAIN-04 — SEGMENT RECOMMENDATION / PRODUCT RECOMMENDATION


### 20.1. Mục tiêu định danh

Domain Segment Recommendation / Product Recommendation dùng để truy vết lý do hệ thống gợi ý sản phẩm theo phân khúc, nhu cầu, mùa, mục đích sử dụng, người dùng chính, người thân hoặc quà tặng.

Gợi ý sản phẩm trong Ginsengfood không được là nội dung cảm tính rời rạc. Mỗi gợi ý quan trọng phải trace được logic chọn sản phẩm.


### 20.2. Canonical IDs

health_guard_case_id nếu có health boundary


### 20.3. Owner Core

Segment Recommendation Core.

Product Recommendation Core.

Product Activation Core.

Health Boundary Guard.


### 20.4. Consumer Pack

Sales Proposal Pack.


### 20.5. Required Trace Links

segment_recommendation_id phải liên kết với customer context hoặc session context.

product_triad_recommendation_id phải liên kết với segment_recommendation_id.

Mỗi sản phẩm được đề xuất phải liên kết với product_activation_id.

Nếu có chương trình giá, phải liên kết với program_activation_id.

Nếu có khách cũ, phải liên kết với customer_memory_snapshot_id.

Nếu khách nêu bệnh hoặc kiêng cữ, phải liên kết với health_guard_case_id.

Nếu dẫn tới quote, phải liên kết với quote_snapshot_id.

Nếu dẫn tới order draft, phải liên kết với order_confirmation_draft_id.


### 20.6. bị chặn-if-Missing Rule

Nếu thiếu product_activation_id, sản phẩm không được đề xuất như sản phẩm bán được.

Nếu thiếu sellable/program context, AI không được gợi ý theo hướng chốt mua.

Nếu thiếu health guard trong tình huống khách nêu bệnh, AI không được tư vấn sâu.

Nếu thiếu customer context, AI chỉ được tư vấn chung, không được cá nhân hóa theo lịch sử mua, member, Diamond hoặc người thân.


## 21. DOMAIN-05 — CRM 12-MONTH / MESSAGE TRIGGER / SUPPRESSION


### 21.1. Mục tiêu định danh

Domain CRM 12-Month dùng để truy vết chăm sóc vòng đời khách hàng từ D0, D1, D3, D7, D14, D21, D30, M2 đến M12.

CRM trong Ginsengfood không phải spam khuyến mãi. CRM là chăm sóc có trigger, có suppression, có quiet hours, có frequency cap, có dedup, có nội dung immutable và có trace.


### 21.2. Canonical IDs

Khi CRM có đề xuất sản phẩm hoặc quote, phải liên kết thêm:


### 21.3. Owner Core

CRM Core.

Message Trigger Core.

Suppression Guard.

Frequency Cap Core.

Quiet Hours Guard.

Dedup Core.


### 21.4. Consumer Pack

CRM Runtime.

Gateway Messenger.


### 21.5. Required Trace Links

message_trigger_id phải liên kết với customer lifecycle stage.

crm_message_job_id phải liên kết với message_trigger_id.

crm_message_job_id phải liên kết với customer_identity_resolution_id.

Nếu dùng lịch sử mua, phải liên kết với customer_memory_snapshot_id.

Mỗi CRM job phải có dedup_key.

Mỗi CRM action phải có idempotency_key.

Nếu CRM bị suppress, audit phải ghi rõ suppression reason.

Nếu CRM có gợi ý sản phẩm, phải liên kết với product_triad_recommendation_id.

Nếu CRM dẫn tới quote, phải liên kết với quote_snapshot_id.


### 21.6. bị chặn-if-Missing Rule

Nếu thiếu message_trigger_id, CRM không được gửi.

Nếu thiếu crm_message_job_id, không được ghi nhận job gửi tin hợp lệ.

Nếu thiếu dedup_key, CRM không được gửi ra khách.

Nếu vi phạm suppression, quiet hours hoặc frequency cap, CRM phải dừng.

Nếu có complaint, privacy, payment dispute, delivery issue hoặc health review mở, CRM bán hàng phải bị suppress.


## 22. DOMAIN-06 — MEMBER LIFECYCLE / RIGHTS / DOWNGRADE / DISPUTE


### 22.1. Mục tiêu định danh

Domain Member Lifecycle dùng để truy vết chu kỳ thành viên, quyền lợi, duy trì hạng, ân hạng, hạ hạng, tranh chấp member và các thay đổi liên quan đến tier.

AI, CRM, Gateway và Landing Page không được tự tính hạng, doanh số, số còn thiếu hoặc quyền lợi thành viên.


### 22.2. Canonical IDs

Khi liên quan đến quote hoặc order, phải liên kết thêm:


### 22.3. Owner Core

Member Lifecycle Core.

Member Rights Core.

Member Tier Resolver.

Grace Period Core.

Member Dispute Core.


### 22.4. Consumer Pack

Quote Core.

Order Core.


### 22.5. Required Trace Links

member_lifecycle_event_id phải liên kết với customer_identity_resolution_id.

member_grace_event_id phải liên kết với member_lifecycle_event_id.

member_dispute_case_id phải liên kết với customer identity, member event và audit.

Quote có dùng quyền lợi member phải liên kết với member_lifecycle_event_id.

Order có phát sinh tích lũy hoặc quyền lợi member phải liên kết với order_code.

CRM có nội dung member phải liên kết với member lifecycle context hợp lệ.


### 22.6. bị chặn-if-Missing Rule

Nếu thiếu member_lifecycle_event_id, AI không được báo hạng, quyền lợi, doanh số còn thiếu hoặc trạng thái duy trì.

Nếu có member_dispute_case_id đang mở, CRM upsell và thông điệp nâng hạng phải bị suppress.

Nếu thiếu customer identity, không được resolve member tier.

Nếu thiếu audit, thay đổi trạng thái member không được coi là hợp lệ.


## 23. DOMAIN-07 — DIAMOND / AFFILIATE / ENTREPRENEURSHIP / DISTRIBUTOR BOUNDARY


### 23.1. Mục tiêu định danh

Domain Diamond / Affiliate / Entrepreneurship dùng để truy vết link Diamond, bind, eligibility, commission, conflict với Ads, self-purchase policy và tranh chấp hoa hồng.

Domain này tách biệt với đại lý, nhà phân phối, mua sỉ và hợp tác B2B.


### 23.2. Canonical IDs

Khi liên quan đến quote/order/payment, phải liên kết thêm:


### 23.3. Owner Core

Diamond Referral Core.

Affiliate Attribution Core.

Diamond Bind Core.

Commission Eligibility Core.

Payment / Accounting Review Core.

Dispute Core.


### 23.4. Consumer Pack

ADS Attribution Pack.

Accounting Pack.


### 23.5. Required Trace Links

diamond_referral_link_id phải liên kết với Diamond owner.

diamond_bind_id phải liên kết với referral link, buyer context và entry session.

commission_event_id phải liên kết với diamond_bind_id.

Commission chỉ được xét khi có order_code và payment/accounting eligibility hợp lệ.

Nếu quote có quyền lợi Diamond link, phải liên kết với quote_snapshot_id.

Nếu có conflict Ads/Diamond, audit phải ghi quyết định attribution.

Nếu có tranh chấp hoa hồng, phải mở dispute case hoặc liên kết audit tương ứng.


### 23.6. bị chặn-if-Missing Rule

Nếu thiếu diamond_bind_id, không được áp dụng Diamond attribution.

Nếu thiếu eligible order, không được tạo commission event hợp lệ.

Nếu thiếu payment/accounting confirmation, không được coi commission payable.

Nếu khách hỏi đại lý/nhà phân phối/mua sỉ, không được route nhầm sang Diamond/Affiliate.

Nếu thiếu member context, không được xác nhận khách đủ điều kiện Diamond.


## 24. DOMAIN-08 — PRICING / PROGRAM / 24/7 / GOLDEN HOUR / QUOTESNAPSHOT


### 24.1. Mục tiêu định danh

Domain Pricing / Program / QuoteSnapshot dùng để truy vết giá, chương trình 24/7, Giờ Vàng, quyền lợi member, quyền lợi Diamond link, thời điểm quote và giá trị báo cho khách.

AI không được báo giá nếu không có QuoteSnapshot hợp lệ.


### 24.2. Canonical IDs


### 24.3. Owner Core

Pricing Core.

Program Core.

Member Benefit Resolver.

Diamond Benefit Resolver.

Sellable Guard.


### 24.4. Consumer Pack

ADS / MC AI.


### 24.5. Required Trace Links

quote_snapshot_id phải liên kết với customer_identity_resolution_id nếu quote cá nhân hóa.

quote_snapshot_id phải liên kết với program_activation_id nếu có chương trình.

Nếu là Giờ Vàng, phải liên kết với golden_hour_session_id.

Nếu có quyền mua sớm, phải liên kết với early_access_window_id.

Nếu có member benefit, phải liên kết với member_lifecycle_event_id.

Nếu có Diamond link benefit, phải liên kết với diamond_bind_id.

quote_cart_id phải liên kết với quote_snapshot_id.

Quote dẫn tới order draft phải liên kết với order_confirmation_draft_id.


### 24.6. bị chặn-if-Missing Rule

Nếu thiếu quote_snapshot_id, AI không được báo giá.

Nếu thiếu program_activation_id, không được nói giá chương trình.

Nếu thiếu golden_hour_session_id, không được nói đang thuộc phiên Giờ Vàng.

Nếu thiếu product_activation_id, không được quote SKU đó.

Nếu thiếu customer identity trong quote cá nhân hóa, không được báo quyền lợi member hoặc Diamond.

Nếu quote hết hạn, phải tạo quote mới, không được dùng lại quote cũ.


## 25. DOMAIN-09 — PROGRAM / SELLABLE / PRODUCT ACTIVATION / PRODUCTION SIGNAL


### 25.1. Mục tiêu định danh

Domain Program / Sellable / Product Activation dùng để truy vết SKU nào được phép nói, được phép bán, được phép lên board, được phép vào chương trình và được phép kích hoạt trên kênh nào.

Quy tắc mở bán là domain P0 riêng.


### 25.2. Canonical IDs

Khi liên quan đến sản xuất/kho, phải trace đến production/stock signal theo boundary của Operational Core.


### 25.3. Owner Core

Sellable Gate Core.

Golden Hour Core.

Daily Live Board Core.

MC AI Brief Core.

Inventory / Operational Signal Core.


### 25.4. Consumer Pack

Live Board.


### 25.5. Required Trace Links

product_activation_id phải liên kết với sellable status.

program_activation_id phải liên kết với sản phẩm được phép tham gia chương trình.

golden_hour_session_id phải liên kết với program activation và quota/session rule.

early_access_window_id phải liên kết với member/Diamond eligibility nếu có.

daily_live_product_board_id phải liên kết với các sản phẩm active được phép nói trong live.

mc_ai_script_brief_id phải liên kết với board và sản phẩm được phép nói.

Quote phải liên kết với product/program activation đang hợp lệ.


### 25.6. bị chặn-if-Missing Rule

Nếu thiếu product_activation_id, sản phẩm không được bán.

Nếu sellable status không hợp lệ, AI/Gateway/CRM/Landing/ADS/MC AI không được nói theo hướng chốt mua.

Nếu thiếu daily_live_product_board_id, live không được tự chọn sản phẩm để đẩy.

Nếu thiếu mc_ai_script_brief_id, MC AI không được tự tạo script bán hàng theo SKU.

Nếu stock/hold/recall/sale lock/channel block xảy ra, product activation phải bị chặn hoặc dừng theo Owner Core.


## 26. DOMAIN-10 — PAYMENT / BANK TRANSFER / ACCOUNTING REVIEW


### 26.1. Mục tiêu định danh

Domain Payment / Bank Transfer / Accounting Review dùng để truy vết thanh toán, chuyển khoản, payment reference, hàng chờ đối soát và xác nhận kế toán.

Khách nói đã chuyển tiền hoặc gửi ảnh giao dịch không tự động tạo trạng thái PAID.


### 26.2. Canonical IDs

Khi có dispute, phải liên kết thêm:

privacy_case_id nếu liên quan dữ liệu nhạy cảm

payment dispute case theo boundary của Payment Core nếu được triển khai trong pack chi tiết


### 26.3. Owner Core

Payment Core.

Bank Registry Core.

Bank Transfer Queue Core.

Accounting Review Core.


### 26.4. Consumer Pack

CRM Care Pack.


### 26.5. Required Trace Links

payment_reference phải liên kết với order_code hoặc order draft/payment intent hợp lệ theo Payment Core.

bank_transfer_queue_id phải liên kết với payment_reference.

accounting_review_id phải liên kết với bank_transfer_queue_id nếu cần đối soát.

PAID status phải trace đến Payment Core hoặc Accounting Review.

Payment instruction customer-facing phải trace đến Bank Registry hợp lệ.

Audit phải ghi nhận mọi xác nhận, từ chối, chưa đủ điều kiện hoặc exception.


### 26.6. bị chặn-if-Missing Rule

Nếu thiếu payment_reference, không được hướng dẫn chuyển khoản chính thức.

Nếu thiếu Bank Registry hợp lệ, không được hiển thị tài khoản ngân hàng.

Nếu chỉ có ảnh giao dịch hoặc lời khách nói, không được chuyển PAID.

Nếu thiếu accounting_review_id trong trường hợp cần kế toán xác nhận, trạng thái phải giữ chưa đủ điều kiện review.

Nếu payment dispute mở, CRM/sales phải bị suppress theo Priority Conflict.


## 27. DOMAIN-11 — SHIPPING / TRACKING / ETA / COD


### 27.1. Mục tiêu định danh

Domain Shipping / Tracking / ETA / COD dùng để truy vết phí ship, ETA, COD, tracking và các event vận chuyển.

AI, CRM, Gateway hoặc CSKH không được bịa trạng thái vận chuyển.


### 27.2. Canonical IDs

Nếu có delivery issue, phải liên kết thêm case ID theo domain complaint hoặc delivery issue trong pack chi tiết.


### 27.3. Owner Core

Shipping Core.

Shipping Eligibility Core.

Tracking Core.

COD Core.


### 27.4. Consumer Pack

CRM Care.


### 27.5. Required Trace Links

shipping_tracking_event_id phải liên kết với order_code.

ETA phải trace đến Shipping Core.

COD status phải trace đến COD Core.

Phí ship phải trace đến Shipping Core.

Customer-facing shipping reply phải trace đến shipping event mới nhất.

Nếu order đã có shipping info, Consumer phải trace về shipping info hiện có, không hỏi lại địa chỉ.


### 27.6. bị chặn-if-Missing Rule

Nếu thiếu shipping event, không được nói trạng thái giao hàng cụ thể.

Nếu thiếu ETA từ Shipping Core, không được tự đoán ETA.

Nếu thiếu phí ship runtime, không được bịa phí ship.

Nếu có delivery issue mở, CRM bán hàng phải suppress.


## 28. DOMAIN-12 — ORDER / ORDERDRAFT / CUSTOMERCONFIRMATION / IVR


### 28.1. Mục tiêu định danh

Domain Order dùng để truy vết chuỗi từ quote, cart, order draft, customer confirmation đến order code chính thức.

Order trong Ginsengfood không được tạo trực tiếp từ nội dung chat nếu chưa có OrderDraft và CustomerConfirmation.


### 28.2. Canonical IDs

Nếu thanh toán/vận chuyển phát sinh, phải liên kết thêm:

Nếu IVR xác nhận đơn, phải liên kết theo boundary PACK-09 IVR_ORDER_CONFIRMATION.


### 28.3. Owner Core

Cart Core.

Order Draft Core.

Customer Confirmation Core.

IVR Order Confirmation Core.


### 28.4. Consumer Pack

IVR Pack.


### 28.5. Required Trace Links

order_confirmation_draft_id phải liên kết với quote_cart_id.

customer_confirmation_id phải liên kết với order_confirmation_draft_id.

order_code chỉ phát sinh sau customer_confirmation_id.

Payment phải liên kết với order_code.

Shipping phải liên kết với order_code.

IVR confirmation chỉ được liên kết với order confirmation purpose, không tự đổi order state.

Audit phải ghi nhận mọi trạng thái draft, confirmed, created, failed, cancelled, void hoặc updated.


### 28.6. bị chặn-if-Missing Rule

Nếu thiếu quote_snapshot_id, không được lập order draft.

Nếu thiếu order_confirmation_draft_id, không được coi là khách đã có bản xác nhận đơn.

Nếu thiếu customer_confirmation_id, không được tạo order chính thức.

Nếu thiếu order_code, không được nói đặt hàng thành công.

Nếu IVR vượt phạm vi xác nhận đơn, phải block theo IVR governance.


## 29. DOMAIN-13 — GATEWAY / PUBLIC COMMENT / MESSENGER HANDOFF / MODERATION


### 29.1. Mục tiêu định danh

Domain Gateway dùng để truy vết comment public, live comment, Messenger handoff, delivery log, moderation, duplicate protection và trạng thái handoff thành công/thất bại.

Public comment/live không được báo giá, chốt đơn, lặp PII, gửi payment instruction hoặc tư vấn health detail dài.


### 29.2. Canonical IDs

Khi handoff dẫn tới tư vấn/quote/order, phải liên kết thêm:


### 29.3. Owner Core

Gateway Core.

Public Comment Core.

Messenger Handoff Core.

Handoff Delivery Log Core.

Live Moderation Core.

Idempotency Core.


### 29.4. Consumer Pack

Messenger Runtime.

Moderation Pack.


### 29.5. Required Trace Links

public_comment_event_id phải liên kết với page/live/comment context.

messenger_handoff_event_id phải liên kết với public_comment_event_id.

handoff_delivery_log_id phải ghi nhận handoff success/fail.

live_moderation_event_id phải liên kết với comment/event bị moderation.

dedup_key phải chống trả lời trùng comment.

idempotency_key phải chống handoff lặp.

Nếu handoff success, private flow phải dùng cùng correlation_id.

Nếu handoff fail, public fallback phải trace đến delivery log fail.


### 29.6. bị chặn-if-Missing Rule

Nếu thiếu public_comment_event_id, không được xử lý comment như event hợp lệ.

Nếu thiếu messenger_handoff_event_id, không được nói đã handoff.

Nếu handoff_delivery_log_id không ghi success, không được nói đã gửi Messenger.

Nếu thiếu dedup/idempotency, không được auto-reply public ở quy mô live.

Nếu comment chứa PII, payment, health, giá hoặc mua hàng, public reply phải bị giới hạn theo Gateway Guard.


## 30. DOMAIN-14 — OFFICIAL CONTACT / PHONE NUMBER REGISTRY


### 30.1. Mục tiêu định danh

Domain Official Contact dùng để truy vết số điện thoại hoặc kênh liên hệ chính thức được phép hiển thị theo intent.


### 30.2. Canonical IDs


### 30.3. Owner Core

Official Contact Registry.

Contact Policy Core.

Intent Contact Resolver.


### 30.4. Consumer Pack

Admin UI.

Partnership / Distributor Pack.


### 30.5. Required Trace Links

official_contact_id phải liên kết với contact source.

Contact được trả lời phải liên kết với intent.

Public response có số điện thoại phải trace đến official_contact_id.

Audit phải ghi nhận intent nào đã dùng contact nào.

Nếu intent là tham quan/gặp công ty/lãnh đạo, contact phải trace đúng registry.

Nếu intent là đại lý/nhà phân phối/hợp tác/mua số lượng nhiều, contact phải trace đúng registry.


### 30.6. bị chặn-if-Missing Rule

Nếu thiếu official_contact_id, không được gửi số điện thoại.

Nếu intent chưa rõ, không được tự chọn số.

Không được hardcode số điện thoại trong content block, AI response, CRM template, Gateway hoặc Landing Page.


## 31. DOMAIN-15 — HEALTH BOUNDARY / COMPLAINT / PRIORITY CONFLICT


### 31.1. Mục tiêu định danh

Domain Health Boundary / Complaint / Priority Conflict dùng để truy vết tình huống sức khỏe, khiếu nại, privacy, hàng giả, tranh chấp và các case có quyền ưu tiên cao hơn sales/CRM.


### 31.2. Canonical IDs

Nếu liên quan order/payment/shipping, phải liên kết thêm:


### 31.3. Owner Core

Complaint Core.

Privacy Core.

Counterfeit / Anti-fraud Core.

Payment Dispute Core nếu có.

Shipping Issue Core nếu có.

Customer Care Core.


### 31.4. Consumer Pack

Customer Care.

Diamond Pack.


### 31.5. Required Trace Links

health_guard_case_id phải liên kết với customer/session context nếu khách nêu tình trạng sức khỏe.

complaint_case_id phải liên kết với customer, order hoặc product context nếu có.

privacy_case_id phải liên kết với dữ liệu hoặc hành động cần bảo vệ.

counterfeit_case_id phải liên kết với product/order/evidence context nếu có.

Case mở phải liên kết với suppression rule.

Sales/CRM bị suppress phải ghi audit reason.

Nếu case đóng, closure phải có audit và evidence nếu cần.


### 31.6. bị chặn-if-Missing Rule

Nếu khách nêu bệnh mà thiếu health guard, AI không được tư vấn sâu.

Nếu có complaint mở, CRM bán hàng phải suppress.

Nếu có privacy case mở, mọi outbound không cần thiết phải dừng.

Nếu có counterfeit case mở, sales phải dừng hoặc chuyển đúng luồng kiểm tra.

Nếu có payment dispute hoặc delivery issue, upsell phải bị suppress.


## 32. DOMAIN-16 — EVIDENCE / COMPLETION REPORT / GATEWAY GATE / SECURITY


### 32.1. Mục tiêu định danh

Domain Evidence / Completion Report / Gateway Gate / Security dùng để truy vết bằng chứng, smoke test, security evidence, App Review evidence, UAT evidence, owner sign-off và completion gate.

Domain này bảo đảm không có trạng thái Gateway PASS, production ready, release approved hoặc go-live approved khi chưa đủ bằng chứng.


### 32.2. Canonical IDs


### 32.3. Owner Core

Smoke Test Registry.

Security Evidence Core.

Completion Report Core.

Gateway Gate Core.

Release Control Core.


### 32.4. Consumer Pack

QA / Smoke Pack.

Security Pack.

Release Control Pack.

All P0 Domain Owner Cores.


### 32.5. Required Trace Links

evidence_id phải liên kết với smoke_id.

smoke_id phải liên kết với dependency_id hoặc domain_id.

completion_gate_id phải liên kết với toàn bộ P0 evidence cần thiết.

owner_signoff_id phải liên kết với completion_gate_id.

Security evidence phải liên kết với security gate.

Gateway evidence phải liên kết với Gateway gate.

Completion report phải trace được trạng thái PASS/chưa đủ điều kiện/bị chặn của từng P0 domain.

Audit phải ghi nhận mọi thay đổi trạng thái gate.


### 32.6. bị chặn-if-Missing Rule

Nếu thiếu evidence_id, smoke không PASS.

Nếu thiếu smoke_id, evidence không đủ ngữ cảnh.

Nếu thiếu owner_signoff_id, release không approved.

Nếu thiếu completion_gate_id, Gateway không được PASS.


## 33. CROSS-DOMAIN TRACE LINK MATRIX


### 33.1. Luồng Public Comment -> Messenger -> Quote -> Order

Trace bắt buộc:

customer_memory_snapshot_id nếu có

bị chặn rule:

Nếu handoff không success, không được nói đã gửi Messenger.

Nếu thiếu quote snapshot, không được báo giá.

Nếu thiếu customer confirmation, không được tạo order.


### 33.2. Luồng CRM chăm sóc -> Gợi ý sản phẩm -> Quote

quote_snapshot_id nếu báo giá

Nếu có suppression, CRM không được gửi.

Nếu thiếu dedup, không được outbound.

Nếu thiếu product activation, không được chốt mua.


### 33.3. Luồng Diamond Link -> Bind -> Order -> Commission

Nếu thiếu bind, không có Diamond attribution.

Nếu thiếu eligible order, không tạo commission.

Nếu payment/accounting chưa xác nhận, commission không payable.


### 33.4. Luồng Chuyển khoản -> Kế toán đối soát -> Paid

Nếu thiếu payment reference, không hướng dẫn chuyển khoản.

Nếu chưa qua Payment Core hoặc Accounting Review, không chuyển PAID.

Ảnh giao dịch không thay thế accounting review.


### 33.5. Luồng Shipping -> Tracking -> CSKH

crm_message_job_id nếu có chăm sóc sau giao hàng

complaint_case_id nếu có sự cố

Nếu thiếu shipping event, không nói trạng thái cụ thể.

Nếu có delivery issue, CRM bán hàng phải suppress.


### 33.6. Luồng Complaint / Health / Privacy -> Suppression

health_guard_case_id hoặc complaint_case_id hoặc privacy_case_id hoặc counterfeit_case_id

order_code nếu liên quan đơn hàng

payment_reference nếu liên quan thanh toán

shipping_tracking_event_id nếu liên quan vận chuyển

crm_message_job_id nếu có suppression CRM

Case ưu tiên cao hơn sales.

CRM, upsell, quote hoặc chốt đơn phải bị suppress nếu conflict chưa đóng.


### 33.7. Luồng Evidence -> Smoke -> Completion Gate -> Owner Sign-off

Không evidence thì không smoke PASS.

Không smoke PASS thì không completion gate PASS.

Không owner sign-off thì không release approved.


## 34. QUY TẮC CONSUMER KHI TIÊU THỤ ID


### 34.1. AI Advisor

AI Advisor được tiêu thụ ID để tư vấn, giải thích, gợi ý sản phẩm, báo quote, lập order draft hoặc chăm sóc khách hàng.

AI Advisor không được tự tạo:

AI Advisor phải dừng hoặc fallback nếu thiếu ID bắt buộc.


### 34.2. Gateway

Gateway được tiêu thụ ID để xử lý public comment, handoff Messenger, moderation và dedup.

Gateway không được tự báo giá, tự chốt đơn, tự gửi payment instruction hoặc tự public thông tin nhạy cảm.

Gateway bắt buộc dùng:


### 34.3. CRM

CRM được tiêu thụ ID để chăm sóc vòng đời, nhắc dùng sản phẩm, hỏi trải nghiệm, gợi ý mua lại, gợi ý combo hoặc chăm sóc thành viên.

CRM không được gửi nếu thiếu:

CRM phải suppress nếu có complaint, privacy, payment dispute, delivery issue, health review hoặc case ưu tiên chưa đóng.


### 34.4. Landing Page / ADS / MC AI

Landing Page, ADS và MC AI chỉ được dùng sản phẩm, giá, chương trình, board và script đã active.

Các Consumer này không được tự chọn SKU để bán nếu thiếu:

daily_live_product_board_id nếu thuộc live

mc_ai_script_brief_id nếu thuộc MC AI

quote_snapshot_id nếu báo giá cá nhân hóa


### 34.5. Payment / Accounting

Payment và Accounting được quyền xử lý:

Các Consumer khác không được tự xác nhận PAID.

Không được hardcode tài khoản ngân hàng ngoài Bank Registry.


### 34.6. Shipping

Shipping Core là nguồn chính thức của:

ETA

COD

Phí ship

Tracking status

AI/CRM/Gateway chỉ được phản hồi theo dữ liệu Shipping Core, không tự đoán trạng thái giao hàng.


## 35. KẾT LUẬN PHẦN 2/4


## PHẦN 2/4 của MASTER-03 đã khóa ID Registry theo 16 business domain chính.

Mỗi domain có canonical ID rõ ràng.

Mỗi ID có Owner Core rõ ràng.

Consumer Pack chỉ được tiêu thụ ID trong boundary.

Required Trace Links là bắt buộc.

Thiếu ID thì không được suy luận.

Thiếu trace link thì không được pass audit.

Thiếu evidence thì không được PASS.

Thiếu owner sign-off thì không được release.

Thiếu completion gate PASS thì không được Gateway PASS.

Không pack nào được tự tạo ID làm đứt traceability.

MASTER-03 tiếp tục ở PHẦN 3/4 với Correlation, Idempotency, Immutability, Audit và Evidence Trace Control.


## PHẦN 3/4 — CORRELATION / IDEMPOTENCY / IMMUTABILITY / AUDIT / EVIDENCE TRACE CONTROL


## PHẦN 3/4 khóa lớp kiểm soát vận hành của ID khi dữ liệu đi qua nhiều pack, nhiều resolver, nhiều guard, nhiều action và nhiều gate. Trọng tâm của phần này là bảo đảm không đứt traceability, không xử lý trùng, không ghi đè lịch sử, không mất audit và không pass evidence/gateway khi chưa đủ bằng chứng hợp lệ.


## 36. MỤC TIÊU KIỂM SOÁT CỦA PHẦN 3/4


## PHẦN 3/4 quy định cách hệ thống Ginsengfood kiểm soát 5 lớp bắt buộc của Traceability ID:

Correlation ControlBảo đảm các bước trong cùng một luồng nghiệp vụ được nối với nhau xuyên pack.

Idempotency ControlBảo đảm một hành động quan trọng không bị thực hiện lặp ngoài ý muốn.

Dedup ControlBảo đảm khách hàng không bị nhận trùng tin, comment không bị trả lời trùng, event không bị xử lý trùng.

Immutability ControlBảo đảm ID, snapshot, event, quote, evidence, audit và message lịch sử không bị ghi đè.

Audit / Evidence Trace ControlBảo đảm mọi quyết định quan trọng đều có audit, mọi trạng thái PASS đều có evidence, mọi release đều có owner sign-off và completion gate hợp lệ.


## 37. CORRELATION CONTROL


### 37.1. Định nghĩa correlation trong Ginsengfood

correlation_id là ID dùng để nối toàn bộ các bước phát sinh trong cùng một luồng xử lý nghiệp vụ.

correlation_id không phải ID khách hàng.

correlation_id không phải ID đơn hàng.

correlation_id không phải ID quote.

correlation_id không phải ID payment.

correlation_id không thay thế bất kỳ canonical business ID nào.

Vai trò của correlation_id là giữ mạch trace xuyên suốt khi một hành trình đi qua nhiều pack, nhiều domain, nhiều event và nhiều quyết định.


### 37.2. Nguyên tắc bắt buộc của correlation

Hệ thống Ginsengfood bắt buộc áp dụng các nguyên tắc sau:

Mỗi luồng nghiệp vụ đa pack phải có correlation_id.

Một correlation_id có thể liên kết nhiều business ID.

Một business ID quan trọng phải biết nó thuộc correlation_id nào nếu phát sinh trong luồng đa pack.

correlation_id không được dùng để tạo quyền nghiệp vụ.

correlation_id không được dùng để thay thế kiểm tra Owner Core.

correlation_id không được dùng để pass guard.

correlation_id chỉ nối luồng, không quyết định nghiệp vụ.


### 37.3. Khi nào bắt buộc có correlation_id

Các luồng sau bắt buộc phải có correlation_id:

Messenger tư vấn -> QuoteSnapshot -> QuoteCart.

QuoteCart -> OrderConfirmationDraft -> CustomerConfirmation -> OrderCode.

Order -> PaymentReference -> BankTransferQueue -> AccountingReview.

Order -> ShippingTrackingEvent -> CSKH.

CRM trigger -> CRM message job -> khách phản hồi -> tư vấn tiếp.

Diamond referral link -> bind -> quote -> order -> commission.

Member lifecycle -> quyền lợi -> quote -> order.

Health guard / complaint / privacy / counterfeit -> suppression -> resolution.

Live comment -> moderation -> handoff/private reply/fallback.

Product activation -> live board -> MC AI script -> quote/order.


### 37.4. Correlation không được tự tạo bởi Consumer ngoài boundary

Consumer Pack không được tự tạo correlation_id để hợp thức hóa một luồng đã thiếu trace.

Nếu Consumer nhận được event không có correlation_id trong luồng bắt buộc phải có correlation, Consumer phải:

Yêu cầu Runtime Resolver cung cấp correlation hợp lệ.

Hoặc block action.

Hoặc fallback an toàn.

Hoặc handoff đúng boundary.

Hoặc ghi audit exception nếu event không đủ điều kiện xử lý.

Consumer không được tự ghép chuỗi bằng cách suy luận từ số điện thoại, tên khách, comment text, giờ live, order text hoặc nội dung hội thoại.


### 37.5. Correlation cha/con

Một luồng lớn có thể phát sinh nhiều luồng con.

Một khách comment trong live hỏi giá, sau đó vào Messenger, nhận tư vấn, được lập quote, chọn chuyển khoản, phát sinh kế toán đối soát và shipping.

Luồng chính có thể có một correlation_id xuyên suốt.

Trong luồng đó có thể có các correlation con hoặc sub-flow reference cho:

Handoff flow.

Quote flow.

Order flow.

Payment flow.

Shipping flow.

CRM follow-up flow.

Complaint flow nếu có sự cố.

MASTER-03 quy định mọi correlation con phải trace được về correlation cha.

Không được để luồng con tách rời làm mất mối liên hệ với customer journey gốc.


### 37.6. Correlation trong public comment và Messenger handoff

Đối với Gateway / Public Comment / Messenger Handoff, correlation bắt buộc bảo đảm:

public_comment_event_id liên kết với messenger_handoff_event_id.

messenger_handoff_event_id liên kết với handoff_delivery_log_id.

Nếu handoff thành công, private Messenger flow tiếp tục dùng cùng correlation hoặc correlation con có trace về correlation gốc.

Nếu handoff thất bại, fallback public phải trace được về handoff fail.

AI không được nói “đã gửi Messenger” nếu handoff_delivery_log_id không ghi success.

Live comment trùng phải được dedup nhưng vẫn giữ audit về sự kiện bị bỏ qua.


### 37.7. Correlation trong Quote và Order

Đối với Quote / Cart / Order, correlation bắt buộc bảo đảm:

quote_snapshot_id liên kết với customer/session context.

quote_cart_id liên kết với quote_snapshot_id.

order_confirmation_draft_id liên kết với quote_cart_id.

customer_confirmation_id liên kết với order_confirmation_draft_id.

order_code liên kết với customer_confirmation_id.

Payment và shipping nếu phát sinh phải trace về order_code.

Nếu order không tạo được, failure audit phải giữ correlation.

Không có correlation hợp lệ thì không được coi luồng quote -> order là đầy đủ trace.


### 37.8. Correlation trong Payment và Accounting Review

Đối với Payment / Bank Transfer / Accounting Review, correlation bắt buộc bảo đảm:

payment_reference trace về order hoặc payment intent hợp lệ.

bank_transfer_queue_id trace về payment_reference.

accounting_review_id trace về bank_transfer_queue_id.

PAID status trace về Payment Core hoặc Accounting Review.

Ảnh giao dịch hoặc lời khách nói đã chuyển tiền chỉ được ghi nhận như evidence/claim của khách, không thay thế Accounting Review.

Nếu phát sinh tranh chấp thanh toán, payment dispute phải cùng correlation hoặc có trace về correlation gốc.


### 37.9. Correlation trong CRM

Đối với CRM 12 tháng, correlation bắt buộc bảo đảm:

message_trigger_id liên kết với stage chăm sóc.

crm_message_job_id liên kết với message_trigger_id.

Tin CRM được gửi ra phải liên kết với customer context.

Nếu khách phản hồi CRM, phản hồi đó phải nối lại với CRM job gốc.

Nếu CRM dẫn tới tư vấn, quote hoặc order, correlation phải nối tiếp sang flow mới.

Nếu CRM bị suppress, audit phải ghi rõ suppression reason trong cùng trace.

CRM không được tạo cuộc hội thoại bán hàng rời rạc không liên kết với trigger gốc.


### 37.10. Correlation trong Diamond / Affiliate

Đối với Diamond / Affiliate, correlation bắt buộc bảo đảm:

diamond_referral_link_id liên kết với Diamond owner.

diamond_bind_id liên kết với buyer/session hợp lệ.

Quote phát sinh từ link Diamond phải trace về diamond_bind_id.

Order phát sinh từ link Diamond phải trace về diamond_bind_id.

commission_event_id chỉ được xét khi có order/payment/accounting eligibility hợp lệ.

Nếu có conflict Ads/Diamond, audit phải ghi decision theo correlation.

Nếu có self-purchase policy hoặc commission dispute, trace không được đứt khỏi bind và order gốc.


### 37.11. Correlation trong Evidence / Smoke / Completion Gate

Đối với Evidence / Smoke / Completion Gate, correlation bắt buộc bảo đảm:

evidence_id liên kết với smoke_id.

smoke_id liên kết với dependency_id hoặc domain_id.

completion_gate_id liên kết với evidence package.

owner_signoff_id liên kết với completion gate.

Mọi trạng thái PASS/chưa đủ điều kiện/bị chặn phải có audit.

Không có correlation giữa evidence và gate thì không được xem là release evidence hợp lệ.


### 37.12. bị chặn-if-Missing Correlation

Nếu một luồng bắt buộc có correlation nhưng không có correlation_id, hệ thống phải áp dụng:

Không báo giá.

Không tạo order draft.

Không xác nhận order success.

Không xác nhận PAID.

Không gửi CRM outbound.

Không ghi commission payable.

Không nói handoff success.

Không pass smoke.

Không pass completion gate.

Không release approved.


## 38. IDEMPOTENCY CONTROL


### 38.1. Định nghĩa idempotency trong Ginsengfood

idempotency_key dùng để bảo đảm một hành động nghiệp vụ quan trọng không bị thực hiện lặp nhiều lần do retry, double click, webhook gửi lại, người dùng bấm nhiều lần, hệ thống timeout, network lỗi hoặc worker xử lý lại.

Idempotency không phải dedup nội dung.

Idempotency kiểm soát hành động có tác động trạng thái.

Dedup kiểm soát sự trùng lặp event/message/response.


### 38.2. Nguyên tắc idempotency bắt buộc

Hệ thống Ginsengfood bắt buộc áp dụng idempotency cho mọi hành động có thể gây ra hậu quả nghiệp vụ, tài chính, customer-facing hoặc release.

Các hành động này không được chạy lặp nếu đã có kết quả hợp lệ trong cùng phạm vi idempotency.

Nếu nhận lại cùng một request/action với cùng idempotency_key, hệ thống phải trả về trạng thái đã xử lý hoặc block xử lý lặp, không được tạo thêm bản ghi nghiệp vụ mới.


### 38.3. Các action bắt buộc có idempotency_key

Các action sau bắt buộc có idempotency_key:

Tạo quote_snapshot_id.

Tạo quote_cart_id.

Tạo order_confirmation_draft_id.

Ghi customer_confirmation_id.

Tạo order_code.

Tạo payment_reference.

Đưa giao dịch vào bank_transfer_queue_id.

Ghi accounting_review_id.

Tạo shipping_tracking_event_id.

Tạo crm_message_job_id.

Thực hiện Messenger handoff.

Ghi handoff_delivery_log_id.

Ghi live_moderation_event_id.

Ghi complaint_case_id.

Ghi privacy_case_id.

Ghi counterfeit_case_id.

Ghi health_guard_case_id.

Tạo diamond_bind_id.

Tạo commission_event_id.

Ghi evidence_id.

Ghi owner_signoff_id.

Xét completion_gate_id.


### 38.4. Idempotency trong Quote

Đối với Quote, idempotency bảo đảm:

Một lần yêu cầu quote hợp lệ không tạo nhiều QuoteSnapshot trùng ngoài ý muốn.

Nếu khách bấm lại hoặc hệ thống retry, không tạo nhiều giá trị quote mâu thuẫn trong cùng phạm vi xử lý.

Nếu quote hết hạn hoặc điều kiện giá thay đổi, quote mới phải là quote mới có trace rõ, không ghi đè quote cũ.

Quote cũ không được cập nhật để giống quote mới.


### 38.5. Idempotency trong OrderDraft và CustomerConfirmation

Đối với OrderDraft và CustomerConfirmation, idempotency bảo đảm:

Một lần khách xác nhận không tạo nhiều đơn.

Một nút xác nhận bị bấm nhiều lần không sinh nhiều order_code.

Một IVR confirmation retry không tạo nhiều trạng thái xác nhận.

Một draft đã void hoặc superseded không được dùng lại để tạo order nếu không còn hợp lệ.

CustomerConfirmation phải trace về draft hợp lệ tại thời điểm xác nhận.


### 38.6. Idempotency trong Payment

Đối với Payment, idempotency bảo đảm:

Một order không bị tạo nhiều payment_reference ngoài rule Payment Core.

Một callback/retry không làm trạng thái thanh toán bị ghi nhận nhiều lần.

Một giao dịch chờ kế toán không bị tạo nhiều bank_transfer_queue_id trùng.

Một kết quả Accounting Review không bị ghi lặp thành nhiều lần xác nhận PAID.

Payment dispute không bị mở trùng nếu cùng sự kiện tranh chấp.


### 38.7. Idempotency trong Gateway và Handoff

Đối với Gateway, idempotency bảo đảm:

Một public comment không bị handoff nhiều lần ngoài rule.

Một comment không bị auto-reply nhiều lần do webhook retry.

Một handoff attempt không bị ghi thành nhiều success giả.

Một handoff failure không bị biến thành success nếu delivery log không xác nhận.

Một moderation event không bị tạo trùng gây sai dashboard hoặc sai xử lý.


### 38.8. Idempotency trong CRM

Đối với CRM, idempotency bảo đảm:

Một trigger không tạo nhiều CRM job ngoài rule.

Một CRM job không gửi nhiều message giống nhau ngoài chủ đích.

Một message đã bị suppress không được retry gửi nếu suppression vẫn còn hiệu lực.

Một khách đã opt-out không bị gửi lại vì worker retry.

Một message text immutable không bị sửa sau khi job đã phát hành.


### 38.9. Idempotency trong Diamond / Commission

Đối với Diamond / Commission, idempotency bảo đảm:

Một referral session không bị bind nhiều lần sai rule.

Một order không tạo nhiều commission event cho cùng một quyền lợi.

Một commission không bị payable nhiều lần.

Một conflict Ads/Diamond không bị xử lý thành nhiều quyết định mâu thuẫn.

Một self-purchase case không bị bỏ qua do event lặp.


### 38.10. Idempotency trong Evidence / Completion Gate

Đối với Evidence và Completion Gate, idempotency bảo đảm:

Một evidence upload không bị tạo nhiều evidence_id trùng làm sai trạng thái gate.

Một smoke run không bị ghi nhiều PASS nếu thực tế chỉ có một evidence hợp lệ.

Một owner sign-off không bị ghi trùng.

Một completion gate không bị chuyển PASS nhiều lần từ cùng một evidence không đầy đủ.


### 38.11. bị chặn-if-Missing Idempotency

Nếu một action bắt buộc có idempotency_key nhưng thiếu idempotency_key, hệ thống phải:

Không tạo order.

Không gửi CRM.

Không handoff tự động ở quy mô live.

Không tạo payment reference.

Không ghi commission.

Không ghi owner sign-off.

Không xét completion gate.

Ghi audit exception nếu action bị chặn.


## 39. DEDUP CONTROL


### 39.1. Định nghĩa dedup trong Ginsengfood

dedup_key dùng để phát hiện và kiểm soát các event, message hoặc phản hồi bị trùng.

Dedup bảo vệ khách hàng và hệ thống khỏi:

Nhận nhiều tin CRM giống nhau.

Bị trả lời public nhiều lần trên cùng comment.

Bị handoff nhiều lần không cần thiết.

Bị gọi IVR xác nhận lặp.

Bị ghi duplicate complaint.

Bị xử lý duplicate payment notification.

Bị tính duplicate commission.

Dedup không được xóa trace.

Một event bị dedup vẫn phải có audit để biết vì sao không xử lý lại.


### 39.2. Dedup khác idempotency

MASTER-03 quy định rõ:

dedup_key dùng để nhận diện trùng event/message/response.

idempotency_key dùng để bảo vệ action có tác động trạng thái.

Một nghiệp vụ quan trọng có thể cần cả hai.

Một comment live hỏi giá có thể cần:

dedup_key để không trả lời trùng comment.

idempotency_key để không handoff nhiều lần.

correlation_id để nối comment -> Messenger -> quote.

audit_id để ghi nhận quyết định public/private.


### 39.3. Dedup trong Public Comment / Live

Đối với Public Comment / Live, dedup bắt buộc bảo đảm:

Một comment không bị trả lời nhiều lần do webhook retry.

Một khách spam cùng nội dung không làm AI trả lời vô hạn.

Một intent hỏi giá/mua không bị tạo nhiều handoff không cần thiết.

Một comment chứa PII không bị lặp lại public.

Một moderation flag không bị nhân bản làm sai dashboard.

Nếu thiếu dedup ở live quy mô lớn, Gateway không được bật auto-reply production.


### 39.4. Dedup trong Messenger Handoff

Đối với Messenger Handoff, dedup bắt buộc bảo đảm:

Một handoff không được gửi trùng.

Một private reply không được gửi nhiều lần cho cùng event.

Một delivery success không được ghi trùng.

Một delivery failure không được retry vô hạn ngoài rule.

Handoff success và handoff failure không được tồn tại mâu thuẫn cho cùng attempt mà không có audit giải thích.


### 39.5. Dedup trong CRM

Đối với CRM, dedup bắt buộc bảo đảm:

Một khách không nhận cùng một tin chăm sóc nhiều lần trong cùng trigger.

Một lifecycle stage không tạo nhiều job trùng.

Một job đã suppress không tạo lại message nếu chưa có trigger mới.

Một message đã gửi không bị gửi lại vì retry.

Một khách có complaint/payment dispute/delivery issue không nhận tin bán hàng do job cũ chạy lại.


### 39.6. Dedup trong Payment và Shipping

Đối với Payment, dedup bắt buộc bảo đảm:

Một giao dịch hoặc một payment signal không bị ghi trùng.

Một payment reference không bị đối soát trùng.

Một accounting review không bị tạo nhiều kết luận cho cùng một dữ kiện nếu chưa có evidence mới.

Đối với Shipping, dedup bắt buộc bảo đảm:

Một tracking event không bị ghi trùng.

Một trạng thái giao hàng không bị gửi lại nhiều lần cho khách.

Một delivery issue không bị tạo nhiều complaint case ngoài rule.


### 39.7. Dedup trong Evidence

Đối với Evidence, dedup bắt buộc bảo đảm:

Một bằng chứng không được tính nhiều lần cho nhiều gate nếu không có mapping rõ.

Một ảnh/log/file không được nhân bản để làm giả coverage.

Một smoke evidence không được dùng lại sai domain nếu không có trace link.

Một evidence không đủ điều kiện không được retry thành PASS.


### 39.8. Dedup không được làm mất audit

Khi hệ thống dedup một event, audit phải ghi nhận:

Event nào bị dedup.

Dedup dựa trên dedup_key nào.

Event nào là event gốc.

Action nào đã bị bỏ qua.

Có ảnh hưởng customer-facing hay không.

Có liên quan payment/order/complaint/evidence hay không.

Dedup không được xóa lịch sử.

Dedup không được che lỗi.

Dedup không được biến event sai thành event đúng.


## 40. IMMUTABILITY CONTROL


### 40.1. Định nghĩa immutability trong Ginsengfood

Immutability là nguyên tắc bảo đảm ID và dữ liệu lịch sử quan trọng không bị ghi đè sau khi phát sinh.

Trong Ginsengfood, các đối tượng sau bắt buộc áp dụng immutability:

Canonical ID.

Snapshot.

Event.

Quote.

Order confirmation draft history.

Customer confirmation.

Payment reference.

Accounting review result.

CRM message text đã phát hành.

Handoff delivery log.

Complaint/privacy/counterfeit case event.

Smoke result.

Owner sign-off.

Completion gate state history.


### 40.2. Canonical ID bất biến

Các ID canonical không được sửa sau khi phát sinh:

Nếu cần thay đổi nghiệp vụ, phải tạo event mới, snapshot mới, version mới hoặc relation supersede/void.


### 40.3. Snapshot bất biến

Snapshot phản ánh trạng thái tại thời điểm snapshot được tạo.

Các snapshot như customer_memory_snapshot_id, quote_snapshot_id, mc_ai_script_brief_id không được cập nhật ngược để khớp với dữ liệu mới.

Khách thay đổi hạng member sau quote thì quote cũ vẫn giữ quyền lợi tại thời điểm quote.

Giá chương trình thay đổi thì quote cũ không được sửa.

Memory mới phát sinh thì memory snapshot cũ không được ghi đè.

MC AI script brief thay đổi thì brief cũ vẫn phải giữ lịch sử.

Snapshot mới phải có ID mới hoặc version mới và trace rõ với snapshot trước nếu có liên quan.


### 40.4. Event bất biến

Event phản ánh sự kiện đã xảy ra.

Các event như public comment, handoff attempt, delivery log, shipping event, commission event, member lifecycle event, CRM job event, moderation event không được sửa để làm thay đổi sự thật lịch sử.

Nếu event sai, hệ thống phải ghi correction event, void event hoặc supersede relation.

Không được sửa event cũ để xóa lỗi.

Không được xóa event cũ để làm đẹp dashboard.

Không được đổi event failure thành success nếu không có event success mới hợp lệ.


### 40.5. Quote bất biến

quote_snapshot_id là bằng chứng giá tại thời điểm báo cho khách.

Quote đã phát hành không được sửa các dữ kiện chính:

Sản phẩm.

Giá niêm yết tại thời điểm quote.

Chương trình áp dụng.

Quyền lợi member nếu có.

Quyền lợi Diamond link nếu có.

Tổng tiền.

Thời điểm tạo quote.

Thời hạn giữ quote.

Điều kiện quote.

Customer context tại thời điểm quote.

Nếu cần báo lại giá, tạo quote mới.

Không được cập nhật quote cũ để tránh tranh chấp.


### 40.6. OrderDraft và CustomerConfirmation bất biến

order_confirmation_draft_id phải giữ lịch sử bản nháp khách đã thấy.

customer_confirmation_id phải giữ bằng chứng khách xác nhận.

Không được sửa bản draft đã được khách xác nhận mà không tạo version/supersede rõ.

Không được tạo order_code nếu không trace được confirmation hợp lệ.

Nếu khách sửa thông tin nhận hàng, sản phẩm, số lượng, payment method hoặc shipping method, phải tạo draft mới hoặc revision có trace rõ.


### 40.7. Payment và Accounting bất biến

payment_reference không được sửa tùy tiện sau khi phát sinh.

bank_transfer_queue_id không được chuyển thành PAID nếu chưa có Payment Core hoặc Accounting Review xác nhận.

accounting_review_id phải giữ kết quả review và evidence liên quan.

Nếu review sai, phải có correction/review event mới; không được sửa âm thầm kết quả cũ.

Ảnh giao dịch, lời khách nói đã chuyển, ghi chú CSKH không thay thế accounting result.


### 40.8. CRM message text bất biến

Message text đã được phát hành cho khách phải immutable.

CRM không được sửa lại nội dung đã gửi để làm thay đổi lịch sử.

Nếu cần gửi nội dung đính chính, phải tạo message mới, trigger mới hoặc correction flow mới.

CRM message đã phát hành phải trace được:

customer context

nội dung đã gửi

thời điểm gửi

suppression/pass decision

dedup/idempotency result

audit


### 40.9. Evidence bất biến

Evidence phục vụ smoke, security, UAT, App Review, Gateway, Completion Report hoặc Owner Sign-off không được sửa sau khi được đưa vào evidence package.

Nếu evidence sai, thiếu hoặc lỗi, phải tạo evidence mới hoặc đánh dấu evidence cũ là void/superseded bằng audit.

Không được thay file evidence cũ bằng file mới nhưng giữ nguyên evidence_id.

Không được dùng evidence không trace được smoke.

Không được dùng evidence không trace được domain/dependency.


### 40.10. Owner Sign-off bất biến

owner_signoff_id là bằng chứng Owner đã phê duyệt một gate, một release hoặc một phạm vi.

Owner sign-off không được sửa sau khi phát sinh.

Nếu Owner rút lại phê duyệt hoặc yêu cầu kiểm tra lại, phải tạo event mới hoặc trạng thái revocation/supersede có audit.

Không được sửa sign-off cũ để đổi thời điểm, đổi phạm vi, đổi người phê duyệt hoặc đổi kết luận.


## 41. AUDIT RELATIONSHIP CONTROL


### 41.1. Vai trò của audit

Audit là lớp chứng minh quyết định đã xảy ra và xảy ra theo rule nào.

Audit không chỉ là log kỹ thuật.

Audit trong Ginsengfood phải ghi nhận quan hệ giữa:

ID.

Actor/Core/Resolver/Guard.

Thời điểm.

Trạng thái trước.

Trạng thái sau.

Decision.

Reason.

Correlation.

Exception.

Suppression.

Fallback.

Handoff.

Completion gate.


### 41.2. Các loại audit bắt buộc

MASTER-03 quy định các nhóm audit bắt buộc:

Decision AuditGhi lại guard/resolver đã quyết định pass, block, suppress, fallback hoặc handoff.

State AuditGhi lại thay đổi trạng thái của quote, order, payment, shipping, member, commission, complaint, evidence, gate.

Action AuditGhi lại action customer-facing như gửi CRM, trả lời Messenger, handoff, tạo order draft, xác nhận đơn.

Evidence AuditGhi lại evidence nào được gắn vào smoke/gate, evidence pass/fail/void/superseded.

Release AuditGhi lại trạng thái chưa đủ điều kiện/PASS/bị chặn của completion gate, owner sign-off và release control.

Exception AuditGhi lại trường hợp thiếu ID, thiếu evidence, thiếu source, thiếu resolver, conflict, retry, dedup, suppression hoặc policy block.


### 41.3. Audit bắt buộc cho P0 Domain

Các domain P0 sau bắt buộc có audit:

Customer Identity / Customer Memory.

CRM 12-Month / Suppression.

Member Lifecycle / Rights / Dispute.

Diamond / Affiliate / Commission.

Product Recommendation.

Pricing / Program / QuoteSnapshot.

Sellable / Product Activation / Golden Hour.

Payment / Bank Transfer / Accounting Review.

Shipping / Tracking / COD / ETA.

Order / OrderDraft / CustomerConfirmation / IVR.

Gateway / Public Comment / Messenger Handoff / Moderation.

Official Contact.

Health Boundary / Complaint / Privacy / Counterfeit.

Evidence / Completion Report / Gateway Gate / Security.


### 41.4. Audit không được thay thế business state

Audit chỉ ghi nhận quyết định hoặc hành động.

Audit không tự tạo trạng thái nghiệp vụ hợp lệ.

Có audit khách nói đã chuyển khoản không có nghĩa là PAID.

Có audit tạo order draft không có nghĩa là order success.

Có audit handoff attempt không có nghĩa là handoff success.

Có audit smoke run không có nghĩa là smoke PASS.

Có audit owner review không có nghĩa là owner sign-off PASS.

Có audit CRM planned không có nghĩa là CRM sent.

Business state chỉ hợp lệ khi Owner Core tương ứng xác nhận.


### 41.5. Audit phải ghi lý do block

Khi hệ thống block một action, audit phải ghi reason.

Các reason bắt buộc audit gồm:

Missing source.

Missing dependency.

Missing customer identity.

Missing quote snapshot.

Missing product activation.

Missing program activation.

Missing payment reference.

Missing accounting review.

Missing shipping event.

Missing evidence.

Missing owner sign-off.

Handoff failed.

Suppression active.

Complaint open.

Privacy case open.

Payment dispute open.

Delivery issue open.

Health boundary required.

Counterfeit case open.

Member dispute open.

Commission dispute open.

Completion gate chưa đủ điều kiện.


### 41.6. Audit phải ghi quyết định fallback

Khi hệ thống không thể thực hiện action chính và phải fallback, audit phải ghi:

Action chính là gì.

Vì sao action chính không được thực hiện.

Fallback nào được dùng.

Fallback có customer-facing hay không.

Fallback có ảnh hưởng sales/order/payment/shipping không.

Fallback có cần human handoff không.

Fallback có cần evidence không.

Gateway không handoff Messenger thành công thì không được nói đã gửi Messenger. Hệ thống chỉ được dùng fallback an toàn và audit lý do handoff fail.


### 41.7. Audit phải ghi suppression

Khi CRM, sales, quote, upsell hoặc outbound bị suppress, audit phải ghi:

Suppression source.

Suppression reason.

Case ID gây suppression nếu có.

Customer context.

Action bị chặn.

Thời điểm suppress.

Điều kiện để mở lại nếu có.

Pack bị ảnh hưởng.

Suppression không được âm thầm bỏ qua vì sẽ làm sai dashboard chăm sóc và sai đánh giá vận hành.


### 41.8. Audit phải bảo vệ dữ liệu nhạy cảm

Audit không được public hóa dữ liệu nhạy cảm.

Audit có thể lưu quan hệ ID và metadata cần thiết nhưng không được làm lộ:

Số điện thoại.

Email.

Địa chỉ.

Tài khoản ngân hàng.

Thông tin sức khỏe chi tiết.

Nội dung khiếu nại nhạy cảm.

Nội dung riêng tư của khách.

Thông tin nội bộ lãnh đạo.

Secret/token/key.

Dữ liệu payment nhạy cảm.

Nếu audit cần liên kết dữ liệu nhạy cảm, phải dùng ID tham chiếu và boundary của Owner Core tương ứng.


## 42. EVIDENCE TRACE CONTROL


### 42.1. Vai trò của evidence_id

evidence_id là định danh bằng chứng dùng để chứng minh một dependency, smoke, gate hoặc release condition đã được kiểm tra và có kết quả.

Evidence không phải ghi chú rời.

Evidence không phải ảnh chụp không định danh.

Evidence không phải checklist chữ không trace.

Evidence không phải lời khẳng định “đã test”.

Evidence phải có ID và trace link.


### 42.2. Evidence phải liên kết với smoke_id

Mỗi evidence dùng cho smoke phải liên kết với smoke_id.

Một smoke_id có thể có nhiều evidence_id.

Một evidence_id có thể hỗ trợ nhiều smoke chỉ khi có mapping rõ, không được tự động dùng lại.

Nếu evidence không liên kết smoke, smoke không được PASS.

Nếu smoke không liên kết domain/dependency, smoke không được dùng cho completion gate.


### 42.3. Evidence phải liên kết với domain_id và dependency_id

Evidence phải chứng minh đúng domain hoặc dependency được yêu cầu.

Evidence payment không được dùng để pass shipping.

Evidence handoff không được dùng để pass QuoteSnapshot.

Evidence quote không được dùng để pass Accounting Review.

Evidence CRM sent không được dùng để pass suppression.

Evidence smoke nội bộ không được dùng để pass Gateway nếu thiếu production-like proof theo gate.

Evidence sai domain không được tính PASS.


### 42.4. Evidence phải có trạng thái

Evidence phải có trạng thái governance rõ:

DRAFT

SUBMITTED

UNDER_REVIEW

ACCEPTED

REJECTED

VOID

SUPERSEDED

Chỉ evidence ở trạng thái ACCEPTED mới được dùng để xét PASS.

Evidence ở trạng thái DRAFT, SUBMITTED, UNDER_REVIEW, REJECTED, VOID hoặc SUPERSEDED không được tính PASS.


### 42.5. Evidence phải có phạm vi

Evidence phải ghi rõ phạm vi chứng minh.

Phạm vi evidence có thể là:

Unit-level smoke.

Pack-level smoke.

Cross-pack smoke.

E2E smoke.

Security smoke.

App Review evidence.

UAT evidence.

Production readiness evidence.

Gateway evidence.

Owner sign-off evidence.

Evidence không được dùng vượt phạm vi nếu không được Owner Core chấp thuận.


### 42.6. Evidence phải có thời điểm

Evidence phải có thời điểm tạo, thời điểm review và thời điểm accepted nếu được chấp nhận.

Evidence quá cũ so với trạng thái hệ thống hiện tại không được tự động giữ PASS nếu dependency, source, program, payment, gateway, security hoặc release condition đã thay đổi.

Nếu hệ thống thay đổi phạm vi P0, phải xét lại evidence liên quan.


### 42.7. Evidence phải có quan hệ với audit_id

Mỗi evidence quan trọng phải liên kết với audit_id.

Audit phải ghi:

Evidence được tạo từ đâu.

Evidence gắn vào smoke nào.

Evidence được review bởi ai/core nào.

Evidence được accepted/rejected vì sao.

Evidence có bị void/superseded không.

Evidence đang hỗ trợ gate nào.

Evidence có đủ để PASS hay không.


### 42.8. Evidence không được thay thế owner sign-off

Evidence chứng minh hệ thống hoạt động theo smoke.

Owner sign-off chứng minh chủ sở hữu đã phê duyệt cho gate hoặc release.

Có evidence không có nghĩa là release approved.

Có smoke pass không có nghĩa là owner sign-off pass.

Có owner review không có nghĩa là owner sign-off pass.

Release chỉ được approved khi có owner_signoff_id hợp lệ trong phạm vi gate tương ứng.


### 42.9. Evidence không được thay thế completion gate

Evidence là đầu vào của completion gate.

Completion gate là quyết định governance tổng hợp.

Một gate chỉ được PASS khi:

Đủ evidence bắt buộc.

Evidence được accepted.

Smoke liên quan PASS.

P0 dependency không còn missing.

Security/App Review/UAT/E2E nếu thuộc phạm vi đã PASS.

Owner sign-off hợp lệ.

Audit đầy đủ.

Không được chuyển Gateway PASS chỉ vì có một phần evidence.


## 43. EVIDENCE PACKAGE CONTROL


### 43.1. Evidence Package là gì

Evidence Package là tập hợp các evidence_id phục vụ một smoke, một gate, một release hoặc một Completion Report.

Evidence Package không phải file zip rời rạc.

Evidence Package phải có trace đến:

owner_signoff_id nếu cần


### 43.2. Evidence Package tối thiểu cho Gateway Gate

Gateway Gate tối thiểu phải có evidence cho:

Public comment event handling.

Messenger handoff success/failure.

Dedup public comment.

Idempotency handoff.

PII protection.

No public price quote.

No public payment instruction.

Health/complaint/private handoff.

Moderation event.

Customer context private flow.

QuoteSnapshot private flow.

OrderDraft private flow nếu Gateway hỗ trợ order flow.

Suppression conflict.

Security/App permission nếu thuộc phạm vi Gateway.

Thiếu bất kỳ P0 evidence bắt buộc nào thì Gateway Gate không PASS.


### 43.3. Evidence Package tối thiểu cho Quote / Order Gate

Quote / Order Gate tối thiểu phải có evidence cho:

Customer identity resolution.

Product activation.

Program activation.

QuoteSnapshot.

QuoteCart.

OrderConfirmationDraft.

CustomerConfirmation.

OrderCode.

Quote expiry.

Quote refresh.

Idempotency order confirmation.

Duplicate confirm prevention.

Payment reference nếu có payment.

Shipping info nếu có shipping.

Audit trail.

Thiếu QuoteSnapshot hoặc CustomerConfirmation evidence thì không được PASS.


### 43.4. Evidence Package tối thiểu cho Payment Gate

Payment Gate tối thiểu phải có evidence cho:

Bank Registry.

Bank transfer queue.

Accounting review.

chưa đủ điều kiện review.

Rejected review.

Paid confirmation.

Duplicate payment signal handling.

Customer claim “đã chuyển khoản” không tự PAID.

No hardcoded bank account.

Thiếu Accounting Review evidence trong luồng cần đối soát thì không được PASS.


### 43.5. Evidence Package tối thiểu cho CRM Gate

CRM Gate tối thiểu phải có evidence cho:

Message trigger.

CRM message job.

Customer identity.

Customer memory snapshot nếu dùng history.

Message text immutable.

Quiet hours.

Frequency cap.

Dedup.

Idempotency.

Opt-out.

Complaint/payment/delivery/health conflict suppression.

Product recommendation trace nếu CRM có gợi ý sản phẩm.

Quote trace nếu CRM dẫn tới quote.

Thiếu suppression evidence thì CRM không được production ready.


### 43.6. Evidence Package tối thiểu cho Diamond / Commission Gate

Diamond / Commission Gate tối thiểu phải có evidence cho:

Diamond referral link.

Bind eligibility.

Buyer context.

Ads/Diamond conflict.

Self-purchase policy.

Order eligibility.

Payment/accounting eligibility.

Commission event.

Commission dispute.

No hardcoded commission.

Thiếu Diamond bind hoặc eligible order evidence thì commission không PASS.


### 43.7. Evidence Package tối thiểu cho Shipping Gate

Shipping Gate tối thiểu phải có evidence cho:

Shipping eligibility.

Shipping info.

Shipping fee.

ETA.

COD if applicable.

Tracking event.

Delivery issue.

No fabricated shipping status.

No repeated address request when order has shipping info.

Thiếu Shipping Core evidence thì AI/CRM/Gateway không được trả lời trạng thái vận chuyển cụ thể.


## 44. RELEASE / GATE AUDIT CONTROL


### 44.1. Gate không được PASS nếu thiếu ID

Mọi gate quan trọng phải kiểm tra ID trước khi xét PASS.

Gate không được PASS nếu thiếu:

owner_signoff_id nếu gate yêu cầu owner approval

Gate không được dựa vào checklist text không có ID.


### 44.2. Gate không được PASS nếu thiếu trace link

Gate không được PASS nếu ID tồn tại nhưng trace link bị thiếu.

Có evidence_id nhưng không liên kết smoke_id thì không PASS.

Có smoke_id nhưng không liên kết dependency_id thì không PASS.

Có owner_signoff_id nhưng không liên kết completion_gate_id thì không release approved.

Có completion_gate_id nhưng thiếu P0 evidence thì không Gateway PASS.

Có audit nhưng không có evidence thì không PASS.


### 44.3. Gate không được PASS nếu evidence chưa accepted

Evidence ở trạng thái DRAFT, SUBMITTED, UNDER_REVIEW, REJECTED, VOID hoặc SUPERSEDED không được dùng để PASS gate.

Gate chỉ xét PASS với evidence accepted, đúng scope, đúng domain, đúng dependency và còn hiệu lực.


### 44.4. Gate không được PASS nếu có conflict đang mở

Các conflict sau nếu đang mở thì gate liên quan không được PASS:

Health review open.

Security issue open.

Gateway policy issue open.

App Review điểm chặn open.

P0 dependency missing.


### 44.5. Completion Report không thay thế Completion Gate

Completion Report là tài liệu tổng hợp trạng thái.

Completion Gate là quyết định governance có ID.

Không được xem Completion Report là PASS nếu completion_gate_id chưa PASS.

Không được xem Production Ready nếu E2E smoke, security evidence, owner sign-off hoặc P0 gate còn thiếu.


## 45. NO-HARDCODE TRACE CONTROL


### 45.1. Không hardcode ID runtime

Không pack nào được hardcode các ID runtime:

ID runtime phải đến từ Owner Core, Runtime Resolver hoặc context hợp lệ.


### 45.2. Không hardcode dữ liệu thay cho ID

Không được hardcode:

Giá.

Chương trình.

Hạng member.

Quyền lợi member.

Hoa hồng Diamond.

Số điện thoại liên hệ.

Tồn kho.

Trạng thái vận chuyển.

Trạng thái payment.

Trạng thái Gateway.

Trạng thái release.

Claim khoa học.

Link bằng chứng.

Sản phẩm active trong live.

Các dữ liệu này phải trace về Source-of-Truth hoặc Runtime Resolver tương ứng.


### 45.3. Hardcode violation phải tạo audit exception

Nếu phát hiện Consumer dùng dữ liệu hardcode thay vì ID/resolver/source hợp lệ, hệ thống phải ghi audit exception.

Các vi phạm no-hardcode phải được xem là điểm chặn với domain liên quan.

Hardcode tài khoản ngân hàng là điểm chặn Payment Gate.

Hardcode số điện thoại là điểm chặn Official Contact Gate.

Hardcode giá là điểm chặn Quote Gate.

Hardcode hoa hồng là điểm chặn Diamond Gate.

Hardcode product active là điểm chặn Program/Sellable Gate.

Hardcode Gateway PASS là điểm chặn Completion Gate.


## 46. PRIORITY CONFLICT TRACE CONTROL


### 46.1. Priority Conflict phải có case ID

Khi có conflict ưu tiên cao hơn sales/CRM, hệ thống phải có case ID tương ứng:

payment dispute ID nếu được định nghĩa trong Payment Pack

delivery issue ID nếu được định nghĩa trong Shipping/Complaint Pack

commission dispute ID nếu được định nghĩa trong Diamond Pack

Không có case ID thì không được suppress âm thầm.


### 46.2. Priority Conflict phải suppress đúng luồng

Khi conflict mở, các luồng sau phải bị chặn nếu thuộc phạm vi ảnh hưởng:

CRM bán hàng.

Upsell.

Quote mới.

Chốt đơn mới.

Diamond promotion.

Member upgrade message.

Referral/affiliate message.

Product recommendation theo hướng bán.

Public reply có thể làm leo thang khiếu nại.

Payment reminder nếu đang có tranh chấp payment.

Suppression phải trace về case ID.


### 46.3. Priority Conflict không được xóa lịch sử sales

Khi conflict mở, hệ thống không được xóa lịch sử sales, CRM, quote hoặc order.

Hệ thống chỉ được thêm trạng thái suppression, hold, review hoặc case relation.

Lịch sử cũ vẫn giữ nguyên để phục vụ audit, CSKH, kế toán, pháp lý, QA hoặc owner review.


## 47. FAILURE / FALLBACK TRACE CONTROL


### 47.1. Failure phải có audit

Mọi failure quan trọng phải có audit.

Các failure bắt buộc audit gồm:

Resolver failed.

Guard failed.

Quote failed.

Order draft failed.

Customer confirmation failed.

Payment reference failed.

Accounting review failed.

Shipping lookup failed.

CRM suppression.

Evidence rejected.

Smoke failed.

Completion gate bị chặn.

Owner sign-off rejected.

Security điểm chặn.

Gateway policy điểm chặn.


### 47.2. Fallback phải trace được về failure gốc

Fallback không được xuất hiện rời rạc.

Mỗi fallback phải trace về:

Failure gốc.

Guard hoặc resolver đã block.

Action chính bị dừng.

Fallback được chọn.

Customer-facing message nếu có.

Nếu Messenger handoff fail, public fallback phải trace về handoff_delivery_log_id fail.

Nếu Shipping Core không có ETA, câu trả lời không được bịa ETA và phải trace về shipping lookup failed.

Nếu Payment Core chưa xác nhận, AI không được nói đã thanh toán thành công và phải trace về accounting chưa đủ điều kiện/review.


### 47.3. Fallback không được nâng trạng thái nghiệp vụ

Fallback chỉ là phản hồi an toàn hoặc hướng xử lý thay thế.

Fallback không được:

Tạo order success.

Tạo PAID.

Tạo commission payable.

Tạo shipping delivered.

Tạo Gateway PASS.

Tạo release approved.

Tạo owner sign-off.

Tạo evidence accepted.

Tạo member upgrade.

Tạo product active.


## 48. DATA RETENTION / TRACE HISTORY PRINCIPLE


### 48.1. Trace history phải đủ để kiểm toán

Hệ thống phải giữ trace history đủ để trả lời các câu hỏi kiểm toán:

Vì sao AI tư vấn sản phẩm này?

Vì sao AI báo giá này?

Vì sao khách được quyền lợi này?

Vì sao order được tạo?

Vì sao payment chưa PAID?

Vì sao CRM không gửi?

Vì sao comment không trả lời public?

Vì sao handoff thất bại?

Vì sao commission không payable?

Vì sao shipping chưa có ETA?

Vì sao evidence bị rejected?

Vì sao Gateway chưa PASS?


### 48.2. Trace history không được phụ thuộc trí nhớ hội thoại

Trace history không được phụ thuộc vào nội dung chat rời rạc.

Hội thoại có thể là một nguồn context, nhưng quyết định nghiệp vụ phải trace bằng ID.

Không được nói “vì trong chat có nhắc” nếu không có ID, snapshot, event, audit hoặc evidence tương ứng.


### 48.3. Trace history phải phục vụ tranh chấp

Các domain dễ phát sinh tranh chấp phải giữ trace đặc biệt rõ:

Diamond/referral/commission.

Bank transfer.

COD.

Order confirmation.

Health boundary.

Gateway handoff.

CRM opt-out.

Khi có tranh chấp, hệ thống phải truy được ID gốc, quyết định gốc, evidence gốc và audit gốc.


## 49. CONTROL MATRIX CHO 5 LỚP ID


### 49.1. Correlation Matrix

Bắt buộc với:

Luồng đa pack.

Public -> Private.

Quote -> Order.

Order -> Payment.

Order -> Shipping.

CRM -> Response.

Diamond -> Commission.

Evidence -> Gate.

Không có correlation thì không được coi trace hoàn chỉnh.


### 49.2. Idempotency Matrix

Action tạo dữ liệu mới.

Action đổi trạng thái.

Action gửi customer-facing.

Action liên quan tiền.

Action liên quan order.

Action liên quan gate/release.

Action có thể bị retry.

Không có idempotency thì không chạy production ở action P0.


### 49.3. Dedup Matrix

Comment.

Webhook.

Payment signal.

Không có dedup thì không bật automation ở quy mô live/CRM/payment/shipping.


### 49.4. Immutability Matrix

ID canonical.

Draft đã xác nhận.

Confirmation.

CRM text đã gửi.

Sign-off.

Completion gate history.

Không có immutability thì không đủ điều kiện audit/release.


### 49.5. Evidence / Audit Matrix

P0 dependency.

Smoke.

Security.

Order.

Diamond.

Health/Complaint/Privacy.

Completion Report.

Release.

Không có evidence/audit thì không PASS.


## 50. KẾT LUẬN PHẦN 3/4


## PHẦN 3/4 của MASTER-03 khóa lớp kiểm soát vận hành của Traceability ID.

Luồng đa pack bắt buộc có correlation_id.

Action P0 bắt buộc có idempotency_key.

Event/message dễ trùng bắt buộc có dedup_key.

Canonical ID không được sửa.

Snapshot không được ghi đè.

Event không được sửa để thay đổi lịch sử.

Quote đã phát hành không được cập nhật ngược.

CustomerConfirmation không được tạo giả.

PaymentReference không tự đồng nghĩa PAID.

AccountingReview không được bypass.

CRM message đã gửi phải immutable.

Evidence phải có evidence_id.

Smoke phải trace được evidence.

Gate phải trace được smoke/evidence/sign-off.

Audit phải ghi decision, state, action, exception, suppression và fallback.

Fallback không được nâng trạng thái nghiệp vụ.

Completion Report không thay thế Completion Gate.

MASTER-03 tiếp tục ở PHẦN 4/4 với Final ID Done Gate, Smoke Mapping, Release Control và MASTER-03 Final Conclusion.


## PHẦN 4/4 — FINAL ID DONE GATE / SMOKE MAPPING / RELEASE CONTROL / MASTER-03 FINAL CONCLUSION


## 51. MỤC TIÊU CỦA FINAL ID DONE GATE

Final ID Done Gate của MASTER-03 quy định điều kiện tối thiểu để chuẩn Traceability ID được xem là hoàn chỉnh ở tầng governance.

Final ID Done Gate không xác nhận hệ thống đã production ready.

Final ID Done Gate không xác nhận Gateway PASS.

Final ID Done Gate không xác nhận API, database, worker, UI hoặc runtime đã triển khai xong.

Final ID Done Gate chỉ xác nhận rằng chuẩn định danh xuyên hệ thống đã đủ nguyên tắc để các pack sau thiết kế implementation không làm đứt traceability, không tự tạo ID ngoài boundary, không hardcode runtime data, không bỏ qua audit và không pass gate khi thiếu evidence.


## 52. PHẠM VI CỦA FINAL ID DONE GATE

Final ID Done Gate áp dụng cho toàn bộ các nhóm ID đã được khóa trong MASTER-03:

Governance ID.

Customer Context ID.

CRM / Message Trigger ID.

Member Lifecycle ID.

Diamond / Affiliate ID.

Official Contact ID.

Gateway / Handoff / Moderation ID.

Recommendation ID.

Program / Sellable / Activation ID.

Quote / Cart / Order ID.

Payment / Accounting ID.

Shipping ID.

Guard / Case ID.

Control ID.

Evidence / Smoke / Gate ID.

Audit ID.

Final ID Done Gate bắt buộc kiểm tra cả ID ownership, ID generation, ID immutability, correlation, idempotency, dedup, evidence trace, audit relationship và release control.


## 53. NGUYÊN TẮC FINAL ID DONE GATE


### 53.1. Done Gate không dựa trên việc tài liệu đã viết xong

MASTER-03 quy định rõ: tài liệu hoàn thành không đồng nghĩa hệ thống đã sẵn sàng chạy production.

Vì vậy, Final ID Done Gate chỉ xác nhận chuẩn ID đã đủ để làm nền cho implementation và kiểm thử sau này.


### 53.2. Done Gate phải dựa trên traceability

Một ID Done Gate chỉ được xem là đạt khi có thể trả lời được các câu hỏi sau:

ID nào được tạo?

ID đó thuộc domain nào?

Owner Core nào sở hữu ID đó?

Consumer Pack nào được quyền dùng ID đó?

ID đó liên kết với những ID nào khác?

ID đó có immutable không?

ID đó có audit không?

ID đó có thể đi qua correlation không?

ID đó có cần idempotency không?

ID đó có cần dedup không?

ID đó có evidence trace không?

ID đó có ảnh hưởng release gate không?

Nếu không trả lời được các câu hỏi trên, ID chưa đạt chuẩn governance.


### 53.3. Done Gate phải chặn mọi ID không có owner

Không ID nào được phép tồn tại trong hệ thống Ginsengfood nếu không có Owner Core rõ ràng.

Nếu một pack cần một ID nhưng không xác định được Owner Core, ID đó phải được đưa về trạng thái bị chặn tại governance level.

Consumer không được tự nhận quyền tạo ID.

Consumer không được tự map ID từ dữ liệu text.

Consumer không được dùng ID tạm làm ID chính thức.

Consumer không được tự merge ID giữa các domain.


### 53.4. Done Gate phải chặn mọi ID thiếu trace link

ID tồn tại nhưng không có trace link không được coi là đạt.

Có quote_snapshot_id nhưng không trace được product/program/customer context thì không đạt.

Có order_code nhưng không trace được customer_confirmation_id thì không đạt.

Có payment_reference nhưng không trace được Payment Core hoặc Bank Registry thì không đạt.

Có evidence_id nhưng không trace được smoke_id thì không đạt.

Có owner_signoff_id nhưng không trace được completion_gate_id thì không đạt.

Có completion_gate_id nhưng thiếu P0 evidence thì không đạt.


### 53.5. Done Gate phải chặn mọi ID có nguy cơ ghi đè lịch sử

Các ID có khả năng bị sửa ngược, tái sử dụng, void không audit hoặc supersede không trace đều không đạt.

Snapshot, event, quote, order confirmation, payment reference, accounting review, CRM message đã gửi, evidence, owner sign-off và completion gate history phải giữ tính bất biến.

Nếu cần sửa nghiệp vụ, phải tạo event mới, snapshot mới, revision mới hoặc supersede relation mới.

Không được sửa ID cũ để làm thay đổi lịch sử.


## 54. FINAL ID DONE GATE CHECKLIST


### 54.1. Gate 01 — ID Ownership Gate

Điều kiện PASS:

Owner Core có quyền tạo, quản lý vòng đời, void, supersede và audit ID.

Consumer Pack chỉ dùng ID trong boundary.

Không có ID canonical nào do Consumer tự tạo ngoài quyền.

Không có ID runtime nào hardcode trong content, UI, AI, CRM, Gateway, Landing Page, MC AI hoặc template.

Trạng thái bị chặn nếu:

ID thiếu Owner Core.

ID được tạo bởi Consumer không đúng boundary.

ID dùng sai domain.

ID tạm được dùng như ID chính thức.

ID được suy luận từ text, số điện thoại, email, tên khách, comment hoặc nội dung hội thoại.


### 54.2. Gate 02 — ID Generation Gate

Mỗi ID có điều kiện phát sinh rõ.

ID chỉ phát sinh khi đủ điều kiện nghiệp vụ.

ID không được tạo để hợp thức hóa dữ liệu thiếu.

ID không được tạo để bypass guard.

ID không được tạo để pass gate khi thiếu evidence.

Tạo quote_snapshot_id khi chưa đủ giá/runtime.

Tạo order_code khi chưa có customer_confirmation_id.

Tạo payment_reference ngoài Payment Core / Bank Registry.

Tạo commission_event_id khi chưa có Diamond bind/eligible order.

Tạo shipping_tracking_event_id khi chưa có shipping event thật.

Tạo owner_signoff_id khi Owner chưa phê duyệt.

Tạo completion_gate_id PASS khi P0 evidence còn thiếu.


### 54.3. Gate 03 — ID Immutability Gate

Canonical ID không bị sửa sau khi phát sinh.

Snapshot không bị cập nhật ngược.

Event không bị sửa để thay đổi lịch sử.

Quote đã phát hành không bị ghi đè.

Order draft đã xác nhận không bị sửa âm thầm.

Customer confirmation không được tạo giả.

Payment/accounting result không bị đổi ngầm.

Evidence không bị thay nội dung dưới cùng evidence_id.

Owner sign-off không bị sửa lịch sử.

ID bị tái sử dụng.

ID bị ghi đè.

Snapshot cũ bị cập nhật theo dữ liệu mới.

Event failure bị sửa thành success không có event mới.

Evidence cũ bị thay file nhưng giữ nguyên ID.

Sign-off cũ bị sửa người phê duyệt, thời điểm hoặc phạm vi.


### 54.4. Gate 04 — Correlation Gate

Mọi luồng đa pack có correlation_id.

Correlation nối được public -> private, quote -> order, order -> payment, order -> shipping, CRM -> response, Diamond -> commission, evidence -> gate.

Correlation không thay thế business ID.

Correlation cha/con có trace rõ nếu có sub-flow.

Failure/fallback vẫn giữ correlation.

Luồng đa pack không có correlation_id.

Public comment không nối được Messenger handoff.

Quote không nối được OrderDraft.

Order không nối được Payment/Shipping.

Evidence không nối được Smoke/Gate.

Consumer tự ghép correlation bằng suy luận ngoài boundary.


### 54.5. Gate 05 — Idempotency Gate

Các action P0 có idempotency_key.

Retry không tạo duplicate quote/order/payment/CRM/handoff/commission/evidence.

Double click hoặc webhook retry không tạo trạng thái lặp.

Idempotency result có audit.

Tạo nhiều order từ một customer confirmation.

Tạo nhiều payment reference ngoài rule.

Gửi CRM lặp vì retry.

Handoff Messenger lặp không kiểm soát.

Ghi commission lặp.

Xét completion gate nhiều lần từ cùng evidence thiếu điều kiện.


### 54.6. Gate 06 — Dedup Gate

Event/message dễ trùng có dedup_key.

Public comment không trả lời trùng.

CRM không gửi trùng trigger.

Handoff không gửi trùng.

Payment/shipping signal không ghi trùng.

Evidence không bị tính trùng.

Dedup vẫn giữ audit.

Live auto-reply chưa có dedup.

CRM chưa có dedup.

Webhook retry tạo nhiều event giống nhau.

Payment signal bị đối soát trùng.

Evidence bị dùng lặp sai domain.

Dedup xóa mất audit.


### 54.7. Gate 07 — Audit Relationship Gate

Các decision P0 có audit.

Các state change P0 có audit.

Các action customer-facing có audit.

Block/suppression/fallback có audit reason.

Evidence/gate/sign-off có audit.

Audit không chứa dữ liệu nhạy cảm ngoài boundary.

Guard block nhưng không ghi reason.

CRM suppress nhưng không ghi case/source.

Handoff fail nhưng không ghi delivery log/audit.

Payment chưa đủ điều kiện nhưng không trace lý do.

Completion gate đổi trạng thái không audit.

Audit chứa PII/payment/health/private data không đúng boundary.


### 54.8. Gate 08 — Evidence Trace Gate

Mỗi evidence có evidence_id.

Evidence trace được smoke_id.

Smoke trace được domain_id hoặc dependency_id.

Evidence có trạng thái governance.

Evidence accepted mới được dùng để PASS.

Evidence có audit.

Evidence có phạm vi rõ.

Evidence không có ID.

Evidence không liên kết smoke.

Evidence sai domain.

Evidence chưa accepted.

Evidence bị void/superseded nhưng vẫn dùng PASS.

Evidence không trace được completion gate.


### 54.9. Gate 09 — No-Hardcode Runtime ID Gate

Không hardcode ID runtime.

Không hardcode giá/chương trình/quyền lợi/hoa hồng/tài khoản ngân hàng/số điện thoại/tồn kho/trạng thái vận chuyển/trạng thái payment/trạng thái release.

Consumer lấy dữ liệu qua Owner Core, Runtime Resolver hoặc Source-of-Truth hợp lệ.

Hardcode violation có audit exception.

AI hardcode giá.

Gateway hardcode trạng thái handoff.

CRM hardcode quyền lợi member.

Landing Page hardcode tài khoản ngân hàng.

MC AI hardcode sản phẩm active.

ADS hardcode chương trình đang chạy.

Completion Report hardcode trạng thái PASS.


### 54.10. Gate 10 — Release Trace Gate

Gate có completion_gate_id.

Gate trace được toàn bộ P0 evidence.

Gate trace được smoke.

Gate trace được owner sign-off nếu cần.

Gate không có conflict mở.

Gate có audit đầy đủ.

Missing P0 evidence.

Missing E2E smoke.

Missing security evidence.

Gateway policy issue còn mở.

Production Ready chưa đủ điều kiện.


## 55. MASTER-03 SMOKE MAPPING PRINCIPLES


### 55.1. Smoke Mapping của MASTER-03 là governance smoke

Smoke Mapping trong MASTER-03 không phải test case implementation chi tiết.

Smoke Mapping của MASTER-03 là bản đồ kiểm tra tối thiểu để xác nhận chuẩn ID có thể được áp dụng nhất quán trong các pack sau.

Các smoke case trong MASTER-03 phải được các pack implementation cụ thể hóa thành test case, E2E test, evidence package và release gate tương ứng.


### 55.2. Smoke ID Naming Principle

MASTER-03 quy định nhóm smoke cho Traceability ID dùng tiền tố:

M03-ID-SMK-xxx

Trong đó:

M03 là MASTER-03.

ID là Traceability ID Standard.

SMK là smoke mapping.

xxx là số thứ tự smoke.

Smoke ID không thay thế evidence ID.

Mỗi smoke phải có evidence riêng hoặc evidence mapping rõ.


### 55.3. Smoke Mapping phải bám 16 business domain

Smoke Mapping của MASTER-03 phải bao phủ tối thiểu 16 business domain:

Governance / Source-of-Truth.

Product Knowledge / Claim / Brand / Scientific Proof.

Segment Recommendation / Product Recommendation.

CRM 12-Month / Message Trigger / Suppression.

Member Lifecycle / Rights / Downgrade / Dispute.

Diamond / Affiliate / Entrepreneurship / Distributor Boundary.

Pricing / Program / 24/7 / Golden Hour / QuoteSnapshot.

Program / Sellable / Product Activation / Production Signal.

Shipping / Tracking / ETA / COD.

Official Contact / Phone Number Registry.

Health Boundary / Complaint / Priority Conflict.

Không domain P0 nào được thiếu smoke mapping.


## 56. MASTER-03 SMOKE MATRIX


### 56.1. M03-ID-SMK-001 — Source / Domain / Dependency Trace

Mục tiêu:

Kiểm tra mỗi Source-of-Truth, domain và dependency đều có ID và trace link đúng.

ID bắt buộc:

Source trace được domain.

Domain trace được dependency.

Dependency trace được Owner Core và Consumer Pack.

Missing dependency bị block.

Audit ghi đầy đủ trạng thái.

bị chặn nếu:

Consumer dùng dữ liệu không có source_id.

Dependency không có owner.

Dependency thiếu audit.


### 56.2. M03-ID-SMK-002 — Customer Context First Trace

Kiểm tra hệ thống không cá nhân hóa tư vấn, quyền lợi, quote, CRM hoặc Diamond nếu chưa resolve customer context.

customer_memory_snapshot_id nếu dùng memory

Khách mới, khách cũ, member, Diamond, buyer từ link Diamond, open case đều được resolve đúng.

Khách cũ có memory snapshot khi nhắc lịch sử mua.

Missing customer context thì không cá nhân hóa.

Audit ghi quyết định pass/block.

AI tự suy luận khách là member/Diamond.

AI nhắc lịch sử mua khi thiếu memory snapshot.

CRM gửi cá nhân hóa khi thiếu customer identity.


### 56.3. M03-ID-SMK-003 — Care Before Next Sale Trace

Kiểm tra khách cũ quay lại được hỏi thăm trải nghiệm trước khi bán tiếp.

Hệ thống biết lần mua gần nhất nếu dữ liệu có.

AI hỏi trải nghiệm trước khi tư vấn tiếp.

Nếu không có memory snapshot, AI không nhắc sản phẩm cụ thể.

Audit ghi luồng chăm sóc trước bán.

AI nhảy thẳng vào quote/combo khi khách cũ chỉ quay lại chào.

AI bịa sản phẩm mua gần nhất.

AI dùng memory không có snapshot.


### 56.4. M03-ID-SMK-004 — CRM Trigger / Suppression / Dedup Trace

Kiểm tra CRM 12 tháng có trigger, job, suppression, dedup, idempotency và audit.

customer_memory_snapshot_id nếu dùng history

CRM chỉ gửi khi có trigger hợp lệ.

CRM không gửi trùng.

CRM suppress đúng quiet hours, frequency cap, opt-out và priority conflict.

Message text đã gửi immutable.

Audit ghi pass/suppress.

CRM gửi khi thiếu trigger.

CRM gửi khi complaint/payment/delivery/health case mở.

CRM gửi trùng do retry.

CRM sửa lại message đã gửi.


### 56.5. M03-ID-SMK-005 — Member Lifecycle Trace

Kiểm tra hạng, quyền lợi, duy trì, ân hạng, hạ hạng và dispute member có trace.

member_grace_event_id nếu có ân hạng

member_dispute_case_id nếu có tranh chấp

AI không tự tính hạng/doanh số/quyền lợi.

Member benefit trong quote trace được member event.

Hạ hạng không làm khách xấu hổ.

Dispute mở thì suppress CRM upsell/nâng hạng.

AI/CRM tự tính số còn thiếu.

Quote áp quyền lợi member không có member lifecycle event.

Member dispute mở nhưng vẫn gửi upsell.


### 56.6. M03-ID-SMK-006 — Diamond / Affiliate / Commission Trace

Kiểm tra Diamond referral, bind, order eligibility, payment eligibility và commission có trace đầy đủ.

accounting_review_id nếu cần

Diamond link trace được Diamond owner.

Buyer bind hợp lệ trước khi áp attribution.

Commission chỉ xét trên eligible order.

Payment/accounting chưa xác nhận thì commission chưa payable.

Ads/Diamond conflict có audit.

Thiếu diamond_bind_id.

Tạo commission khi chưa có eligible order.

Hardcode hoa hồng.

Route nhầm Diamond sang đại lý/nhà phân phối.


### 56.7. M03-ID-SMK-007 — Product Recommendation / Product Effectiveness Trace

Kiểm tra gợi ý sản phẩm có trace về segment, product triad, product activation, health boundary và customer context.

customer_identity_resolution_id nếu cá nhân hóa

customer_memory_snapshot_id nếu dùng lịch sử

program_activation_id nếu có chương trình

health_guard_case_id nếu khách nêu bệnh

Gợi ý sâu theo 3 trụ: mùa, chức năng, bổ dưỡng.

Thêm sản phẩm chăm sóc người thân nếu có ngữ cảnh.

Mỗi item có Product Effectiveness.

Không ép combo.

Sản phẩm đề xuất phải active/sellable nếu tư vấn theo hướng mua.

Gợi ý SKU không active.

Tư vấn health sâu không có health guard.

Bịa hiệu dụng ngoài Product Knowledge.

Ép combo khi khách không phù hợp.


### 56.8. M03-ID-SMK-008 — Product Activation / Sellable / Golden Hour Trace

Kiểm tra sản phẩm chỉ được nói/bán khi active, sellable, có price, đúng board, đúng chương trình và đúng kênh.

daily_live_product_board_id nếu live

mc_ai_script_brief_id nếu MC AI

SKU sellable mới được nói/bán.

Stock/hold/recall/sale lock/channel block được kiểm soát.

Giờ Vàng chỉ mở theo điều kiện tồn và quota.

Board live chỉ chứa sản phẩm active.

MC AI chỉ nói sản phẩm trong brief hợp lệ.

AI bán SKU hết hàng.

Gateway/CRM/Landing/ADS nói SKU không active.

MC AI tự chọn SKU ngoài board.

Golden Hour thiếu session ID.

Không có product activation nhưng vẫn quote.


### 56.9. M03-ID-SMK-009 — QuoteSnapshot / QuoteCart Trace

Kiểm tra báo giá luôn đi qua QuoteSnapshot và QuoteCart.

member_lifecycle_event_id nếu có member benefit

diamond_bind_id nếu có Diamond link benefit

golden_hour_session_id nếu có Giờ Vàng

AI không báo giá nếu thiếu QuoteSnapshot.

Quote trace được sản phẩm, chương trình, khách hàng, quyền lợi và thời điểm.

Quote hết hạn thì tạo quote mới.

Quote cũ không bị cập nhật ngược.

QuoteCart liên kết đúng QuoteSnapshot.

Báo giá không có quote_snapshot_id.

Quote dùng SKU không sellable.

Quote dùng quyền lợi member/Diamond không có context.

Quote cũ bị sửa.


### 56.10. M03-ID-SMK-010 — OrderDraft / CustomerConfirmation / OrderCode Trace

Kiểm tra order chỉ được tạo sau OrderDraft và CustomerConfirmation.

OrderDraft trace được QuoteCart.

CustomerConfirmation trace được OrderDraft.

OrderCode chỉ phát sinh sau CustomerConfirmation.

Double confirm không tạo nhiều order.

Draft sửa phải có revision/supersede trace.

IVR chỉ xác nhận đơn trong phạm vi ORDER_CONFIRMATION_ONLY.

Tạo order không có CustomerConfirmation.

Nói đặt hàng thành công khi thiếu order_code.

IVR tự đổi order state.

Double click tạo nhiều order.


### 56.11. M03-ID-SMK-011 — Payment / Bank Transfer / Accounting Review Trace

Kiểm tra thanh toán, chuyển khoản, đối soát và PAID status không bị bypass.

Chuyển khoản có payment reference.

Bank transfer đi qua queue nếu cần đối soát.

Accounting Review xác nhận trước khi PAID trong luồng cần review.

Ảnh giao dịch/lời khách nói không tự PAID.

Không hardcode tài khoản ngân hàng.

Payment dispute suppress sales/CRM nếu mở.

Hướng dẫn chuyển khoản không có payment_reference.

Hiển thị tài khoản ngân hàng ngoài registry.

Khách gửi ảnh là chuyển PAID.

Không có Accounting Review trong luồng cần đối soát.


### 56.12. M03-ID-SMK-012 — Shipping / Tracking / ETA / COD Trace

Kiểm tra phí ship, ETA, COD, tracking và event vận chuyển đều từ Shipping Core.

Shipping event trace được order.

ETA trace về Shipping Core.

COD trace về COD Core.

Phí ship trace về Shipping Core.

AI/CRM/Gateway không bịa trạng thái vận chuyển.

Không hỏi lại địa chỉ nếu order đã có shipping info.

Nói trạng thái giao hàng khi thiếu shipping event.

Tự đoán ETA.

Bịa phí ship.

Delivery issue mở nhưng CRM bán hàng vẫn gửi.


### 56.13. M03-ID-SMK-013 — Gateway / Public Comment / Messenger Handoff Trace

Kiểm tra comment public, handoff Messenger, delivery log, moderation, dedup và idempotency.

live_moderation_event_id nếu có moderation

Public comment không báo giá, không chốt đơn, không lặp PII, không payment instruction.

Hỏi giá/mua/PII/payment/health/Diamond phải handoff private nếu policy cho phép.

Handoff success mới nói đã gửi Messenger.

Handoff fail không nói đã gửi.

Không kêu khách tự inbox nếu có thể chủ động handoff.

Duplicate comment không trả lời trùng.

Public reply báo giá.

Public reply hướng dẫn chuyển khoản.

Handoff fail nhưng nói đã gửi Messenger.

Thiếu dedup/idempotency ở live scale.

Lặp PII ngoài public.


### 56.14. M03-ID-SMK-014 — Official Contact Registry Trace

Kiểm tra số điện thoại/kênh liên hệ chỉ được trả lời qua Official Contact Registry.

Intent tham quan/gặp công ty/lãnh đạo resolve đúng contact.

Intent đại lý/nhà phân phối/hợp tác/mua số lượng nhiều resolve đúng contact.

Không cung cấp số cá nhân lãnh đạo.

Không hardcode số điện thoại trong AI/CRM/Gateway/Landing/template.

Audit ghi intent và contact được dùng.

Gửi số điện thoại không có official_contact_id.

Intent chưa rõ nhưng tự chọn số.

Hardcode số trong content block.

Cung cấp số cá nhân lãnh đạo.


### 56.15. M03-ID-SMK-015 — Health / Complaint / Privacy / Counterfeit Priority Trace

Kiểm tra case ưu tiên cao hơn sales/CRM được trace và suppress đúng.

member_dispute_case_id nếu có

order_code nếu liên quan order

payment_reference nếu liên quan payment

shipping_tracking_event_id nếu liên quan shipping

Khách không nêu bệnh thì AI không tự hỏi bệnh.

Khách nêu bệnh thì Health Boundary Guard kích hoạt.

Complaint/privacy/counterfeit/payment/delivery/member/commission dispute suppress sales/CRM.

Case đóng phải có audit/evidence nếu cần.

Không thuốc hóa sản phẩm.

Tư vấn health sâu không có guard.

Complaint mở nhưng vẫn upsell.

Privacy case mở nhưng vẫn outbound.

Counterfeit case mở nhưng vẫn chốt bán.


### 56.16. M03-ID-SMK-016 — Evidence / Completion Gate / Owner Sign-off Trace

Kiểm tra evidence, smoke, completion gate và owner sign-off có trace đầy đủ.

Evidence trace được smoke.

Smoke trace được dependency/domain.

Completion gate trace được P0 evidence package.

Owner sign-off trace được completion gate.

Evidence accepted mới được tính PASS.

Completion Report không tự thay thế Completion Gate.

Gateway chỉ xem xét khi all P0 gates PASS, evidence complete, E2E smoke PASS và owner sign-off PASS.

Evidence thiếu ID.

Evidence không trace smoke.

Gate thiếu P0 evidence.

Owner chưa sign-off.

Gọi Gateway PASS chỉ vì tài liệu viết xong.


## 57. CROSS-SMOKE E2E TRACE MAP


### 57.1. E2E-01 — Public Live Comment đến Messenger Quote

Luồng kiểm tra:

Public comment hỏi giá -> Gateway nhận event -> handoff Messenger -> delivery success -> private AI tư vấn -> customer context -> QuoteSnapshot.

PASS khi:

Public không báo giá.

Private flow giữ trace.

Quote có QuoteSnapshot hợp lệ.


### 57.2. E2E-02 — Quote đến Order Success

QuoteSnapshot -> QuoteCart -> OrderConfirmationDraft -> CustomerConfirmation -> OrderCode.

Không tạo order nếu thiếu confirmation.

Order success chỉ khi có order_code.


### 57.3. E2E-03 — Order đến Payment Accounting Review

OrderCode -> PaymentReference -> BankTransferQueue -> AccountingReview -> Payment status.

Ảnh giao dịch không tự PAID.

Accounting Review quyết định payment status nếu cần đối soát.


### 57.4. E2E-04 — Order đến Shipping Tracking

OrderCode -> Shipping eligibility -> Shipping event -> ETA/COD/tracking -> CSKH.

Shipping status từ Shipping Core.

Không bịa ETA.

Không hỏi lại địa chỉ nếu đã có shipping info.

Delivery issue suppress CRM sales.


### 57.5. E2E-05 — CRM Care đến Product Recommendation

MessageTrigger -> CRMJob -> CustomerMemorySnapshot -> ProductTriadRecommendation -> Quote nếu khách có nhu cầu.

CRM không spam.

Có suppression.

Có Product Effectiveness.

Không gửi trùng.

Quote chỉ tạo khi đủ điều kiện.


### 57.6. E2E-06 — Diamond Link đến Commission

DiamondReferralLink -> DiamondBind -> Quote -> Order -> Payment/Accounting -> CommissionEvent.

No payment/accounting confirmation = commission not payable.


### 57.7. E2E-07 — Complaint / Privacy / Health Suppression

Customer raises issue -> Case ID created -> Suppression -> CRM/Sales bị chặn -> Resolution audit.

complaint_case_id hoặc privacy_case_id hoặc health_guard_case_id hoặc counterfeit_case_id

crm_message_job_id nếu có CRM bị suppress

quote_snapshot_id nếu quote bị block

CRM/upsell bị suppress.

Audit ghi reason.

Closure có audit/evidence nếu cần.


### 57.8. E2E-08 — Evidence đến Gateway Gate

P0 smoke -> Evidence accepted -> CompletionGate -> OwnerSignoff -> Gateway review.

Evidence accepted.

Smoke PASS.

P0 gate PASS.

Owner sign-off PASS.

Completion gate PASS.

Gateway chỉ được xem xét sau khi all P0 gates PASS.


## 58. RELEASE CONTROL


### 58.1. MASTER-03 Governance Release

MASTER-03 có thể đạt governance release khi:

16 domain ID registry đã được khóa.

Owner Core/Consumer Pack boundary rõ.

Required Trace Links rõ.

Correlation/idempotency/dedup/immutability rõ.

Audit/evidence trace rõ.

Final ID Done Gate rõ.

Smoke mapping rõ.

Release control rõ.

Không đi vào code/API/DB/UI/worker chi tiết.

Không hardcode runtime data.

Không cho Consumer tự suy luận rule kinh doanh.


### 58.2. MASTER-03 không mở Gateway

MASTER-03 Governance Release không mở Gateway.

Gateway vẫn chỉ được xét khi:

All P0 gates PASS.

Evidence package complete.

E2E smoke PASS.

Security/App Review evidence PASS nếu thuộc phạm vi.

Completion Gate PASS.

Nếu các điều kiện trên chưa đủ, Gateway vẫn bị chặn.


### 58.3. MASTER-03 không xác nhận Production Ready

MASTER-03 không xác nhận Production Ready.

Production Ready chỉ được xét khi các pack implementation đã có:

Database/API/DTO/UI/worker chi tiết theo tài liệu implementation.

Runtime Resolver hoạt động.

Guard hoạt động.

Audit hoạt động.

Evidence Registry hoạt động.

Smoke/E2E test có evidence accepted.

Security review PASS.


### 58.4. MASTER-03 không xác nhận Release Approved

MASTER-03 không tự tạo Release Approved.

Release Approved chỉ hợp lệ khi có:

completion_gate_id PASS.

owner_signoff_id hợp lệ.

P0 evidence accepted.

Security/App Review/UAT PASS nếu thuộc phạm vi.

Không có điểm chặn đang mở.


### 58.5. MASTER-03 không xác nhận Go-live Approved

Go-live Approved là quyết định sau cùng, không phát sinh chỉ vì tài liệu hoàn thành.

Go-live Approved chỉ được xét khi:

Gateway PASS nếu thuộc phạm vi.

Security/App Review/Meta permission nếu thuộc phạm vi đã PASS.

Payment/Shipping/Order/CRM/Gateway P0 gates PASS.

Owner phê duyệt go-live.

Rollback plan và incident handling đã sẵn sàng nếu thuộc phạm vi release.


## 59. RELEASE STATE MODEL


### 59.1. Các trạng thái release dùng cho MASTER-03

MASTER-03 sử dụng các trạng thái governance sau:

READY_FOR_REVIEW

GOVERNANCE_PASS

IMPLEMENTATION_REQUIRED

bị chặn


## Release_Review_Ready


### 59.2. Ý nghĩa trạng thái GOVERNANCE_PASS

GOVERNANCE_PASS nghĩa là tài liệu đã đủ chuẩn governance để làm nền cho các pack implementation.

GOVERNANCE_PASS không có nghĩa:

Đã có code.

Đã có database.

Đã có API.

Đã có UI.

Đã có worker.

Đã có runtime.

Đã có test evidence.

Đã có Gateway PASS.

Đã production ready.

Đã release approved.

Đã go-live approved.


### 59.3. Ý nghĩa trạng thái IMPLEMENTATION_REQUIRED

IMPLEMENTATION_REQUIRED nghĩa là các pack sau phải dùng MASTER-03 để thiết kế chi tiết:

Database schema.

API contract.

DTO.

Runtime Resolver.

Guard.

Worker.

Audit log.

Evidence registry.

Smoke test.

E2E test.

Release gate.

MASTER-03 không thay thế các tài liệu này.


### 59.5. Ý nghĩa trạng thái bị chặn

bị chặn nghĩa là có điều kiện P0 chưa đạt hoặc có conflict đang mở.

Các nguyên nhân bị chặn gồm:

Missing Owner Core.

Missing ID.

Missing trace link.

Missing audit.

Missing smoke.

Hardcode violation.

Payment điểm chặn.

Shipping điểm chặn.

Gateway điểm chặn.

Complaint/privacy/health/payment/delivery/member/commission dispute đang mở.

E2E smoke failed.


## 60. FINAL RELEASE STATUS FOR MASTER-03

Sau khi PHẦN 1/4, PHẦN 2/4, PHẦN 3/4 và PHẦN 4/4 được thống nhất, trạng thái governance của MASTER-03 như sau:


## 61. MASTER-03 FINAL DONE GATE SUMMARY

MASTER-03 được xem là đạt Final ID Done Gate ở tầng governance khi toàn bộ điều kiện sau được khóa:

ID Ownership rõ ràng.

ID Generation có điều kiện nghiệp vụ.

ID Immutability bắt buộc.

Snapshot không ghi đè.

Event không sửa lịch sử.

Correlation nối luồng đa pack.

Idempotency chặn xử lý lặp.

Dedup chặn gửi/xử lý trùng.

Audit ghi decision, state, action, fallback, suppression, exception.

Evidence có ID, scope, trạng thái, audit và trace.

Smoke Mapping bao phủ 16 business domain.

Release Control tách governance pass khỏi production ready.

Completion Gate không pass khi thiếu evidence.

Owner Sign-off bắt buộc cho release.

Không Consumer nào tự tạo ID làm đứt traceability.

Không gọi Gateway PASS chỉ vì tài liệu đã hoàn thành.


## 62. MASTER-03 FINAL CONCLUSION

MASTER-03 quy định chuẩn Traceability ID toàn cục cho hệ thống Ginsengfood.

Từ thời điểm MASTER-03 được áp dụng, toàn bộ hệ thống Ginsengfood bắt buộc vận hành theo nguyên tắc:

Mọi ID quan trọng phải có Owner Core.

Mọi ID quan trọng phải có trace link.

Mọi ID quan trọng phải có audit.

Mọi luồng đa pack phải có correlation.

Mọi action P0 phải có idempotency.

Mọi event/message dễ trùng phải có dedup.

Mọi snapshot phải giữ đúng lịch sử.

Mọi quote phải bất biến sau khi phát hành.

Mọi order phải đi qua OrderDraft và CustomerConfirmation.

Mọi payment phải đi qua Payment Core / Bank Registry / Accounting Review.

Mọi shipping reply phải dựa trên Shipping Core.

Mọi CRM phải có trigger, suppression, dedup và audit.

Mọi Diamond commission phải trace từ link, bind, order, payment và eligibility.

Mọi public comment nhạy cảm phải đi qua Gateway Guard và handoff private nếu policy cho phép.

Mọi contact chính thức phải đi qua Official Contact Registry.

Mọi health/complaint/privacy/counterfeit case phải ưu tiên hơn sales và CRM.

Mọi evidence phải có ID.

Mọi smoke phải trace được evidence.

Mọi completion gate phải trace được P0 evidence.

Mọi release phải có owner sign-off.

Mọi trạng thái PASS phải có bằng chứng accepted.

Mọi dữ liệu runtime phải đến từ Source-of-Truth, Owner Core hoặc Runtime Resolver.

Không pack nào được hardcode runtime data.

Không pack nào được tự suy luận rule kinh doanh ngoài boundary.

MASTER-03 hoàn tất vai trò khóa chuẩn định danh xuyên hệ thống để các MASTER, PACK và implementation documents sau có thể triển khai nhất quán, kiểm thử được, audit được, release được và không phá vỡ Source-of-Truth của Ginsengfood.


## 63. MASTER-03 HANDOFF NOTE

MASTER-03 được dùng làm governance reference cho các tài liệu tiếp theo.

Khi các pack sau thiết kế database, API, DTO, UI, worker, event, log, resolver, guard, audit, smoke test hoặc evidence package, bắt buộc đối chiếu MASTER-03 để bảo đảm:

ID đúng owner.

ID đúng domain.

ID đúng trace link.

ID đúng immutability.

ID đúng correlation.

ID đúng idempotency.

ID đúng dedup.

ID đúng audit.

ID đúng evidence.

ID đúng release gate.

Không tạo ID ngoài boundary.

MASTER-03 không yêu cầu sửa lại MASTER-00, MASTER-01 hoặc MASTER-02.

MASTER-03 dựa trên governance từ MASTER-00, Source-of-Truth từ MASTER-01 và dependency model từ MASTER-02, đồng thời trở thành chuẩn ID bắt buộc cho toàn bộ hệ thống tài liệu kỹ thuật Ginsengfood phía sau.
