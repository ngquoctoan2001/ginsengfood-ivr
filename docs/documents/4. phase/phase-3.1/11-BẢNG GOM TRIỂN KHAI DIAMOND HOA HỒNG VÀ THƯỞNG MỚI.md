# BẢNG GOM NGUỒN CHÍNH GIAI ĐOẠN 3.1 - DIAMOND / THÀNH VIÊN / CRM / AI ADVISOR

## 0. Vai trò nguồn chính

Tài liệu này là bản gom chính thức cho PHASE-03.1. Sau khi bộ PHASE-03.1 được hoàn tất, người đọc có thể dùng thư mục `phase-3.1` làm nguồn chính để triển khai mà không phải quay lại hỏi thêm tài liệu bổ sung nào khác.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev triển khai Phase 3.1 có kiểm soát. Trạng thái `CHƯA RELEASE READY`, yêu cầu owner sign-off, evidence accepted hoặc blocker carry-over là guard runtime đúng, không phải lỗi dev-readiness.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES
READY_TO_HAND_TO_DEV: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

GIAI ĐOẠN 03.1 là giai đoạn bổ sung nối tiếp PHASE-03 Commerce Runtime. Giai đoạn này không thay thế PHASE-03, nhưng khóa thêm các rule đã phát sinh sau test thực chiến và từ các bản bổ sung nghiệp vụ:

- Diamond, hoa hồng, link giới thiệu và buyer benefit.
- Chính sách thành viên, chu kỳ 12 tháng, duy trì hạng, ân hạng, hạ hạng.
- Giá niêm yết, chương trình 24/7, Giờ Vàng Tri Ân, quyền mua sớm và quota.
- CRM message, trigger, multi-channel, no duplicate, email rule.
- CRM 12 tháng, member lifecycle automation và outcome logger.
- AI Advisor rule sau test thực chiến, bao gồm core policy, product effectiveness, public/private, health, gift, office, overseas, vegan, inventory, return/refund, privacy, fake goods.
- Payment bank transfer/VietQR, Accounting Review Queue, payment reference, reconciliation.
- Shipping ETA/tracking fallback.
- IVR xác nhận khách mới/high-risk và fake order prevention.
- Smoke, evidence, blocker, owner decision và cross-phase handoff.

Trạng thái cuối của bộ tài liệu: `GIAI ĐOẠN 3.1 LÀ NGUỒN BÀN GIAO TRIỂN KHAI - CHƯA RELEASE READY`.

Không file nào trong PHASE-03.1 được phép tự gọi các trạng thái release/gateway positive khi chưa có owner sign-off và evidence accepted.

Canonical cleanup 2026-06-05: các rule giá/giờ/chính sách trong tài liệu này dùng `PRICE_POLICY_V2026_06_CANONICAL_001`, `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001` và `DIAMOND_COMMISSION_POLICY_V2026_06_CANONICAL_001`. Các giá trị nháp cũ trước cleanup không còn là runtime active.

## 1. Tài liệu nguồn đã gom

| STT | Tài liệu nguồn trong Phase 3.1 | Vai trò được gom vào bộ chính |
| --- | --- | --- |
| 1 | `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | Master Rule Lock V2.1: core policy, AI test note 001-063, resolver/guard, situation registry, done gate, gateway blocked. |
| 2 | `5. bo sung/2. BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU.md` | Supplementary situation locks 076-096: health, gift, office, overseas, 20 SKU, vegan, inventory, IVR, shipping, bank transfer, usage, storage, return/refund, order edit, VAT, privacy, fake goods. |
| 3 | `5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Message system: Golden Hour messages, Diamond referral, commission, Tết, birthday, guest/new/silver/gold/platinum/diamond content, care-after-purchase rules. |
| 4 | `5. bo sung/4. chính sách và hoa hồng thành viên.md` | Member policy, 24/7, Golden Hour, early access, Diamond buyer benefit, commission, non-stacking policy, QuoteSnapshot. |
| 5 | `5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | CRM 12 tháng, product triad, member lifecycle maintain/upgrade/grace/downgrade, quiet hours, frequency cap, retry/fail, P0 test matrix. |
| 6 | `5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Phụ lục message: trigger, immutable text, multi-channel priority, email rule, variables, data conditions, logging, bans. |

## 2. Quan hệ với các phase hiện có

| Phase liên quan | Quan hệ với Phase 3.1 | Nguyên tắc không conflict |
| --- | --- | --- |
| PHASE-01 Product/SKU | Cung cấp product public name, pillar, dietary, claim/product data. | Phase 3.1 không tự tạo SKU, không đổi pillar, không public SKU nội bộ. |
| PHASE-02 Operational Core | Cung cấp sellable, stock, quality hold, recall hold, sale lock, inventory ledger. | Phase 3.1 không bán SKU non-sellable, không public tồn kho cụ thể, không bypass sale lock/recall. |
| PHASE-03 Commerce Runtime | Owner của QuoteSnapshot, Cart, Order Draft, Official Order, Payment, Shipping, Verified Revenue. | Phase 3.1 bổ sung policy, không thay owner truth của Commerce. |
| PHASE-04 AI Advisor | Consumer của member/quote/order/product/situation decisions. | AI không tự tính giá, quyền lợi, hoa hồng, shipping, payment hoặc order state. |
| PHASE-05 Gateway | Consumer/delivery layer cho Messenger/Live/public-private. | Gateway không public final quote cá nhân, không gửi unguarded AI response, không hardcode payment account. |
| PHASE-06 Ads | Consumer của Verified Revenue và attribution-safe signal. | Ads không dùng quote/cart/draft/unpaid/COD waiting làm revenue. |
| PHASE-07 MC AI Live | Consumer của Golden Hour, public/private, abuse moderation. | Live không tự hứa giá/tồn/payment/order, không tạo khan hiếm giả. |
| PHASE-08 IVR | Consumer của official order/confirmation task. | IVR chỉ xác nhận đơn, không sales/CRM/marketing, không tự cập nhật order state. |
| PHASE-09 Release/Evidence | Review evidence/smoke/owner sign-off. | Phase 3.1 không tự quyết release. |

## 3. Bộ file tiếng Việt chính thức

| Thứ tự | File chính thức | Mục đích |
| --- | --- | --- |
| 00 | `00-PHAN-TICH-HIEN-TRANG-GIAI-DOAN-3-1.md` | Đọc codebase/tài liệu, lập current-state/gap/conflict/blocker trước khi thiết kế. |
| 01 | `01-THIẾT-KẾ-KỸ-THUẬT-GIAI-ĐOẠN-3-1.md` | Khóa kiến trúc, owner truth, resolver/guard, contract, DB/API/UI/job/evidence. |
| 02 | `02-THÀNH-VIÊN-GIÁ-CHƯƠNG-TRÌNH-24-7-GIỜ-VÀNG.md` | Member tier, benefit, 24/7, Golden Hour, early access, QuoteSnapshot. |
| 03 | `03-DIAMOND-GIỚI-THIỆU-HOA-HỒNG-ĐỐI-SOÁT.md` | Diamond referral, buyer benefit, commission, reversal, dispute, audit. |
| 04 | `04-TIN-NHẮN-GIỜ-VÀNG-CRM-ĐA-KÊNH.md` | Message trigger, immutable text, channel priority, no duplicate, email rule. |
| 05 | `05-CRM-12-THÁNG-VÒNG-ĐỜI-THÀNH-VIÊN.md` | CRM timeline, member lifecycle automation, quiet hour, frequency cap, outcome logger. |
| 06 | `06-AI-ADVISOR-CHÍNH-SÁCH-LÕI-TÌNH-HUỐNG-THỰC-CHIẾN.md` | AI core policy, AI-TEST-NOTE 001-063 và 076-096, situation registry, content guards. |
| 07 | `07-THANH-TOÁN-VẬN-CHUYỂN-IVR-ĐƠN-HÀNG.md` | Bank transfer, VietQR, reconciliation, shipping fallback, IVR, order edit/refund/VAT. |
| 08 | `08-KIỂM-THỬ-BẰNG-CHỨNG-BÁO-CÁO-GIAI-ĐOẠN-3-1.md` | Ma trận kiểm thử, checklist bằng chứng, báo cáo 14 mục, fail gate. |
| 09 | `09-HƯỚNG-DẪN-BÀN-GIAO-GIAI-ĐOẠN-3-1.md` | Thứ tự chạy, dependency, bàn giao, final status. |
| 10 | `10-PHỤ-LỤC-KHÓA-LIÊN-GIAI-ĐOẠN-DIAMOND-CRM-AI.md` | Chuỗi runtime liên giai đoạn, downstream lock, owner decisions, blocker carry-over. |

## 4. Chuỗi runtime bắt buộc

```text
Product / Operational truth
  -> Sellable / Availability
  -> Commerce QuoteSnapshot / Cart / Order Draft / Official Order / Payment / Shipping / Verified Revenue
  -> Phase 3.1 Member / Program / Diamond / CRM / AI situation policy
  -> AI Advisor / CRM / Gateway / Live / Ads / IVR consumers
  -> Smoke / Evidence / Owner review / Release gate
```

Không có đường tắt:

```text
AI / CRM / Gateway / Live -> tự tính giá, tự tính quyền lợi, tự tính hoa hồng, tự xác nhận payment, tự tạo official order
```

## 5. Rule khóa toàn Phase 3.1

### 5.1. Member

- Chu kỳ thành viên = 12 tháng kể từ ngày được công nhận hạng.
- Doanh số hợp lệ chỉ tính từ order đã được Order Runtime xác nhận thành công.
- Không tính quote, cart, order draft, cancel, refund, test, duplicate, fraud, unpaid, COD waiting chưa verified.
- Silver: từ 1.000.000đ đến 2.999.999đ trong chu kỳ.
- Gold: từ 3.000.000đ đến 4.999.999đ trong chu kỳ.
- Platinum: từ 5.000.000đ đến 7.999.999đ trong chu kỳ.
- Diamond: từ 8.000.000đ trở lên trong chu kỳ.
- Quyền lợi: Silver 5%, Gold 8%, Platinum 12%, Diamond 20%.
- Duy trì hạng = đạt từ 50% chuẩn hạng hiện tại.
- Nếu hết chu kỳ chưa đạt duy trì, vào ân hạng 60 ngày.
- Hết ân hạng vẫn chưa đạt thì hạ hạng an toàn theo runtime.

### 5.2. Giá và chương trình

- Giá niêm yết chỉ hợp lệ khi `listed_price_status = ACTIVE`.
- Không mở bán SKU thiếu giá niêm yết active.
- Chương trình 24/7:
  - SKU lần đầu mở bán: giảm 9%.
  - Số lượng bán thành công ngày trước < 50: giảm 9% ngày sau.
  - Số lượng bán thành công ngày trước >= 50: giảm 5% ngày sau.
- Giờ Vàng:
  - Tối đa 2 phiên/ngày.
  - Phiên 1: 12h00 - 12h45.
  - Phiên 2: 20h00 - 20h45.
  - Stock < 300: không mở.
  - 300 <= stock < 500: mở 1 phiên tối.
  - Stock >= 500: mở 2 phiên.
  - Quota 2.000 sản phẩm/phiên.
- Quyền mua sớm:
  - Silver: 5 phút.
  - Gold: 10 phút.
  - Platinum: 20 phút.
  - Diamond: 30 phút.
  - Buyer từ link Diamond hợp lệ: 5 phút.

### 5.3. Diamond

- Buyer qua link Diamond hợp lệ được giảm 5% trong 24/7 nếu Runtime Core cho phép.
- Buyer qua link Diamond hợp lệ được mua sớm 5 phút trong Giờ Vàng nếu Runtime Core cho phép.
- Giờ Vàng không cộng thêm member discount hoặc Diamond buyer discount ngoài early access nếu QuoteSnapshot không có policy exception.
- Commission Diamond:
  - 24/7: 15%.
  - Giờ Vàng: 10%.
- Self-purchase không eligible commission.
- Chỉ tính commission khi order verified, không cancel, không refund, không test, không duplicate, không fraud.
- Nếu order bị refund/cancel sau commission, phải có reversal/hold policy.
- Diamond affiliate tách khỏi đại lý, nhà phân phối, mua sỉ.

### 5.4. CRM và tin nhắn

- Tin nhắn là trigger-driven, không tự tạo trigger phụ.
- Text đã duyệt là immutable, chỉ render biến.
- Không gửi sai hạng.
- Không gửi trùng cùng message family trong cùng tháng dương lịch.
- Cross-channel dedup bắt buộc.
- Chỉ gửi trên channel có lịch sử tương tác/xác thực.
- Email bắt buộc cho ORDER_SUCCESS nếu user có email hợp lệ.
- Email được phép cho BIRTHDAY_REWARD và TIER_CONGRATS.
- Không gửi email cho SKU sold out hoặc quảng bá tràn lan.
- Không gửi CRM nếu opt-out, open case, privacy request, refund/payment/health case, fatigue cap fail hoặc quiet hour fail.

### 5.5. CRM 12 tháng và vòng đời

- CRM là chăm sóc vòng đời, không phải spam bán hàng.
- Dựa trên Customer Memory, Order History, Product Effectiveness, Member Runtime và Suppression Policy.
- AI/CRM phải nói chủ động hạng, tích lũy, số còn thiếu, ngày còn lại nếu runtime có dữ liệu.
- Không có runtime data thì không bịa.
- Gợi ý sản phẩm mặc định theo 3 trụ:
  - 1 sản phẩm theo mùa.
  - 1 sản phẩm chức năng.
  - 1 sản phẩm bổ dưỡng.
  - 1 sản phẩm chăm sóc người thân nếu context phù hợp.
- Outcome maintain, grace, upgrade, downgrade, winback phải log.

### 5.6. AI Advisor

- Customer context first.
- Không dùng từ "hệ thống" với khách.
- Không tự tính giá/quyền lợi/hoa hồng.
- Không quote nếu thiếu QuoteSnapshot.
- Không order nếu thiếu Order Draft/Confirmation hợp lệ.
- Mỗi gợi ý sản phẩm phải có hiệu dụng Đông phương/y thực đồng nguyên.
- Khách đã mua quay lại phải hỏi thăm trước khi bán tiếp.
- Public comment không báo giá/chốt đơn/PII.
- Health-sensitive phải qua HealthSafetyGuard.
- Complaint/refund/privacy/open case phải suppress sales.
- Abuse/troll malicious phải hide/no reply/no Messenger nếu policy xác định.

### 5.7. Payment, shipping, IVR

- Bank transfer phải có `payment_reference` duy nhất.
- Chọn bank transfer phải gắn `BANK_TRANSFER_PENDING` và `WAITING_BANK_TRANSFER`.
- Không PAID từ ảnh chuyển khoản hoặc lời khách nói.
- PAID chỉ khi Payment Core hoặc Accounting Review xác nhận.
- Shipping realtime nếu có tracking; nếu không có tracking dùng fallback theo vùng.
- Không hứa shipping quốc tế nếu chưa có owner-approved extension.
- Khách mới/high-risk phải qua IVR nếu resolver yêu cầu.
- Nếu IVR required, không sinh official order_code trước khi IVR pass.
- Giờ Vàng phải release quota khi IVR fail/timeout theo policy.

## 6. Workstream chính

| Workstream | Nội dung đầy đủ |
| --- | --- |
| P3.1-2A | Member, price, 24/7, Golden Hour, early access, QuoteSnapshot policy, no stacking. |
| P3.1-2B | Diamond link, referral bind, buyer benefit, commission, reversal, dispute, audit. |
| P3.1-2C | Message templates, trigger registry, tier messages, Golden Hour messages, channel/email/dedup. |
| P3.1-2D | CRM 12-month timeline, member lifecycle, product triad, quiet hours, frequency, retry/fail, outcome log. |
| P3.1-2E | AI Advisor core policy, situation registry, health/gift/office/overseas/20 SKU/vegan/inventory/return/privacy/fake goods. |
| P3.1-2F | Bank transfer, VietQR, payment reconciliation, shipping ETA, IVR, order edit, VAT, return/refund. |
| P3.1-2G | Smoke, evidence, report 14 mục, P0 fail gate, owner review. |

## 7. P0 fail gate toàn Phase 3.1

Phase 3.1 FAIL nếu xảy ra một trong các điểm sau:

- Có rule nguồn trong 6 tài liệu bổ sung không được map vào file chính.
- File tiếng Anh được dùng làm file chính thay vì file tiếng Việt.
- Một workstream thiếu Scope In, Scope Out, P0 blocker, smoke hoặc evidence.
- AI/CRM/Gateway tự tính giá, benefit, commission, shipping hoặc payment.
- Quote không qua QuoteSnapshot.
- Commission không qua Verified Revenue.
- Bank transfer không có payment reference.
- Payment PAID khi thiếu confirmation.
- CRM gửi khi opt-out/open case/fatigue/quiet hour fail.
- Message text bị rewrite trái lock.
- Health-sensitive vẫn quote/order khi guard chưa pass.
- Public surface lộ dữ liệu private.
- Downstream được gọi release-ready khi chưa có evidence accepted và owner sign-off.

## 8. Trạng thái cuối bắt buộc

Sau khi hoàn tất tài liệu:

```text
Documentation: BỘ NGUỒN GIAI ĐOẠN 3.1 COMPLETE
Implementation: NOT EXECUTED BY THIS DOCUMENT
Smoke: REQUIRED DURING IMPLEMENTATION
Evidence: REQUIRED DURING IMPLEMENTATION
Owner sign-off: REQUIRED BEFORE RELEASE
Release: NOT RELEASE READY
Gateway: BLOCKED
```

Không được viết khác trạng thái trên nếu chưa có implementation thật, smoke thật, evidence accepted và owner sign-off.
