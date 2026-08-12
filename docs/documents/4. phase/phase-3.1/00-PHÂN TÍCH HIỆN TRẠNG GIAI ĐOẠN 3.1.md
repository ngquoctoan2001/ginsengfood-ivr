# 00 - PHÂN TÍCH HIỆN TRẠNG GIAI ĐOẠN 3.1

## 1. Mode

`CHỈ PHÂN TÍCH`.

Không sửa code, không tạo migration, không seed, không format file, không đổi contract, không mở Gateway, không gọi Production Ready.

## 2. Mục tiêu phân tích

Mục tiêu của bước 00 là đọc codebase thật và toàn bộ tài liệu Phase 3.1 để trả lời:

1. Codebase hiện có những module nào liên quan member, price, program, Golden Hour, Diamond, CRM, AI Advisor, payment, shipping, IVR.
2. Những rule nào trong 6 tài liệu nguồn đã có chỗ bám.
3. Những rule nào chưa có owner runtime.
4. Những rule nào conflict với Phase 1, Phase 2, Phase 3, Phase 4, Phase 5, Phase 8 hoặc TECH/Master.
5. Những file nào có thể sửa ở từng workstream nếu đi vào implementation.
6. Những migration/API/DTO/event/UI/job/test nào có thể cần.
7. Những blocker P0 nào phải được owner xử lý trước khi code.

Analysis không được tự điền khoảng trống bằng suy luận. Nếu thiếu dữ liệu runtime hoặc chưa thấy module, ghi blocker.

## 3. Entry gate

Chỉ bắt đầu khi đã đọc:

- `TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md`
- 6 tài liệu nguồn trong `5. bo sung/`
- `docs/documents/4. phase/phase-3/09-README-PHASE-03-HANDOFF-INDEX.md`
- `docs/documents/4. phase/phase-3/10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md` nếu có
- Phase 4/5/8 liên quan nếu analysis chạm AI/Gateway/IVR

Nếu thiếu Phase 3 Commerce boundary, ghi:

`P3_1-ENTRY-BLOCKED-BY-PHASE-03-COMMERCE-RUNTIME`

## 4. Source-of-truth bắt buộc

| Nhóm | Source cần đọc | Không được bỏ |
| --- | --- | --- |
| Master Rule Lock | `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | CORE-01 đến CORE-20, AI-TEST-NOTE 001-063, resolver/guard, situation registry. |
| Situation bổ sung | `5. bo sung/2. BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU.md` | AI-TEST-NOTE 076-096, đặc biệt payment/shipping/IVR/privacy/fake goods. |
| Tin nhắn 1 | `5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Golden Hour, Diamond referral, commission, tier message, post-purchase care. |
| Chính sách | `5. bo sung/4. chính sách và hoa hồng thành viên.md` | Member, 24/7, Golden Hour, Diamond benefit, commission, non-stacking, QuoteSnapshot. |
| CRM lifecycle | `5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | Timeline D0-M12, maintain/grace/downgrade, quiet hours, frequency cap. |
| Tin nhắn 2 | `5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Trigger, immutable text, email rule, channel priority, variables, logs, bans. |

## 5. Phạm vi phân tích chi tiết

### 5.1. Member và policy

Phải tìm:

- Entity/table/model member profile.
- Tier field và tier history nếu có.
- Revenue accumulator trong chu kỳ.
- Valid revenue source.
- Cycle start/end.
- Grace status.
- Benefit policy/config.
- Upgrade/maintain/downgrade job nếu có.
- Audit log cho tier change.

Phải ghi gap nếu code chỉ có customer/account mà chưa có Member Core.

### 5.2. Price, 24/7, Golden Hour

Phải tìm:

- Listed price source.
- Program price resolver.
- QuoteSnapshot.
- Program 24/7 scheduler/rule.
- Golden Hour session entity.
- Golden Hour quota.
- Early access resolver.
- Stock/sellable dependency.
- No-stacking/policy priority.

Phải ghi blocker nếu báo giá hiện tại không có QuoteSnapshot hoặc final price đang hardcode.

### 5.3. Diamond

Phải tìm:

- Referral link model.
- Referral bind.
- Buyer attribution.
- Diamond tier eligibility.
- Commission table/job.
- Verified Revenue dependency.
- Commission reversal/refund handling.
- Dispute/case routing.
- Diamond message trigger.

Phải ghi blocker nếu commission có thể tính từ order chưa verified.

### 5.4. CRM messaging

Phải tìm:

- Message template registry.
- Trigger/event registry.
- Delivery channel config.
- Dedup log.
- Frequency cap.
- Quiet hours.
- Opt-out/suppression.
- Delivery audit.
- Email delivery rule.

Phải ghi blocker nếu message text nằm rải rác trong code và có nguy cơ bị rewrite.

### 5.5. CRM lifecycle

Phải tìm:

- Customer Memory.
- Order History.
- Product Effectiveness/Recommendation service.
- CRM jobs D0-D30/M2-M12.
- Member lifecycle jobs M6/M9/M11/M12/Grace.
- Outcome log.
- Open case suppression.

Phải ghi blocker nếu CRM chỉ là marketing blast không có lifecycle guard.

### 5.6. AI Advisor situation

Phải tìm:

- CustomerIdentityResolver.
- CustomerMemoryResolver.
- ProductKnowledgeResolver.
- ClaimGuard/FinalResponseGuard.
- HealthSafetyGuard.
- Public/private boundary.
- Product pillar/dietary resolver.
- Complaint/refund/privacy/fake goods router.
- No "hệ thống" customer-facing control.

Phải ghi blocker nếu AI có thể tự tính giá hoặc tự hứa payment/shipping.

### 5.7. Payment, shipping, IVR, order

Phải tìm:

- Payment method tagging.
- Bank transfer payment reference.
- Company Bank Account Registry.
- Accounting Review Queue.
- Manual reconciliation.
- Bank webhook/API reconciliation nếu có.
- Shipping resolver/tracking/fallback ETA.
- IVR required decision.
- Order state machine.
- Order edit/cancel/refund/VAT handling.

Phải ghi blocker nếu ảnh chuyển khoản có thể set PAID.

## 6. Output report bắt buộc

Analysis report phải có đúng 14 mục:

1. Tóm tắt hiện trạng.
2. File/module/API/schema/test/log/evidence đã đọc.
3. Source-to-code map theo từng tài liệu nguồn.
4. Workstream map P3.1-2A đến P3.1-2G.
5. Gap matrix.
6. Conflict matrix.
7. Runtime owner matrix.
8. API/DTO/event impact.
9. Database/migration impact.
10. UI/Admin/CRM/worker impact.
11. P0 blocker register.
12. Smoke đề xuất.
13. Evidence hiện có hoặc missing evidence.
14. Kết luận có được chuyển sang thiết kế kỹ thuật không.

## 7. P0 blocker analysis

Analysis FAIL nếu bỏ sót:

- AI/CRM/Gateway tự tính giá, benefit, commission.
- Quote không qua QuoteSnapshot.
- Member revenue tính từ order không hợp lệ.
- Diamond commission tính trước Verified Revenue.
- CRM gửi sai hạng, gửi trùng, gửi khi opt-out/open case.
- Message text mutable.
- Bank transfer không có payment reference.
- PAID thiếu reconciliation.
- Shipping ETA bịa.
- IVR required nhưng vẫn sinh official order_code.
- Health/privacy/refund/open case không suppress sales.
- Public surface lộ PII.

## 8. Done gate

File 00 DONE khi:

- Analysis đủ 14 mục.
- Mọi nguồn Phase 3.1 đã được map.
- Mọi gap và conflict được ghi rõ.
- Mọi blocker P0 có ID.
- Không có implementation.
- Kết luận rõ `CAN_MOVE_TO_TECHNICAL_DESIGN` hoặc `BLOCKED`.

## 9. Final status

Trạng thái tối đa:

`GIAI ĐOẠN 3.1 PHÂN TÍCH HOÀN TẤT - CHƯA IMPLEMENT - GATEWAY BLOCKED`
