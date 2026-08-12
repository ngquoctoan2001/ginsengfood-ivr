# 10 - PHỤ LỤC KHÓA LIÊN GIAI ĐOẠN DIAMOND / CRM / AI

## 1. Vai trò phụ lục

Phụ lục này khóa quan hệ giữa PHASE-03.1 và các phase còn lại để tránh conflict.

PHASE-03.1 là nguồn bổ sung chính cho Diamond, member, CRM và AI Advisor policy, nhưng không thay owner truth của Product, Operational, Commerce, Gateway, Ads, Live, IVR hoặc Release.

## 2. Runtime chain

```text
Product Knowledge
  -> Operational Sellable / Inventory / Sale Lock
  -> Commerce QuoteSnapshot / Order / Payment / Shipping / Verified Revenue
  -> Phase 3.1 Member / Program / Diamond / CRM / AI Policy
  -> Downstream AI / CRM / Gateway / Live / Ads / IVR
  -> Smoke / Evidence / Owner Review / Release Gate
```

Nếu upstream chưa pass, downstream phải fail closed.

## 3. Source-to-workstream lock

| Source | Workstream chính | Workstream phụ |
| --- | --- | --- |
| Master Rule Lock V2.1 | File 06 | File 04, 05, 08 |
| Situation Lock 076-096 | File 06, 07 | File 08 |
| Tin nhắn chăm sóc | File 04 | File 05 |
| Chính sách thành viên/hoa hồng | File 02, 03 | File 04, 05 |
| CRM 12-Month | File 05 | File 06, 08 |
| Phụ lục hệ thống tin nhắn | File 04 | File 08 |

## 4. Downstream lock matrix

| Downstream | Được dùng từ Phase 3.1 | Không được làm |
| --- | --- | --- |
| AI Advisor | Member snapshot, quote-safe content, product triad, situation locks, suppression state. | Tự tính giá, tự order, tự payment, claim y khoa, public private data. |
| CRM | Trigger, template, lifecycle, quiet hours, frequency, member outcome. | Rewrite text, gửi sai hạng, gửi khi suppression fail. |
| Gateway | Delivery decision, public/private boundary, channel eligibility. | Public final quote cá nhân, hardcode payment account, send unguarded response. |
| Live | Golden Hour eligibility, quota, early access, abuse handling. | Tạo khan hiếm giả, bypass sale lock, kéo troll vào Messenger. |
| Ads | Verified Revenue, Diamond attribution nếu owner-approved. | Dùng quote/cart/draft/unpaid làm revenue. |
| IVR | IVR required decision, confirmation task. | Sales/CRM/marketing call, tự update order/payment. |
| Admin | Review queues, audit, evidence. | Hardcode policy, set PAID thiếu confirmation, sửa commission thiếu audit. |

## 5. Policy lock carry-over

### 5.1. Member

- Cycle 12 tháng.
- Maintain 50%.
- Grace 60 ngày.
- Downgrade safe.
- Runtime amount proactive.

### 5.2. Price / program

- Listed price active.
- 24/7 canonical 9%/5% rule.
- Golden Hour 2 phiên 12h00 - 12h45 và 20h00 - 20h45, stock threshold, quota.
- Early access theo hạng.
- QuoteSnapshot là source final price.

### 5.3. Diamond

- Buyer benefit 24/7 5%.
- Buyer early access 5 phút trong Giờ Vàng.
- Commission 24/7 15%.
- Commission Giờ Vàng 10%.
- Verified order/revenue required.
- Reversal/dispute required.

### 5.4. Messaging

- Trigger only.
- Immutable text.
- No duplicate monthly.
- Cross-channel dedup.
- Email rule.
- Delivery audit.

### 5.5. CRM

- D0-M12.
- Product triad + family add-on.
- Quiet hours.
- Frequency cap.
- Retry/fail.
- Suppression.
- Outcome log.

### 5.6. AI situation

- Customer context first.
- No "hệ thống".
- Product effectiveness.
- Health guard.
- Public/private.
- Return/refund/privacy/fake goods routing.

### 5.7. Payment/shipping/IVR

- Payment reference.
- Accounting review.
- No proof-image PAID.
- Shipping fallback.
- IVR high-risk.
- No official order_code before IVR pass if required.

## 6. Owner decision register

| ID | Decision | Owner gợi ý |
| --- | --- | --- |
| P3_1-OD-001 | Member Core/CRM Lifecycle Core owner cuối cùng | Commerce/CRM owner |
| P3_1-OD-002 | Policy priority khi có member benefit và Diamond buyer benefit | Commerce owner |
| P3_1-OD-003 | Commission reversal policy | Finance/Commerce owner |
| P3_1-OD-004 | Message template registry canonical | CRM/Brand owner |
| P3_1-OD-005 | Company Bank Account Registry/config owner và masking rule | Finance/Security owner |
| P3_1-OD-006 | IVR high-risk threshold | Order/IVR owner |
| P3_1-OD-007 | International shipping active hay future governed extension | Shipping owner |
| P3_1-OD-008 | Product pillar canonical source cho 20 SKU | Product owner |
| P3_1-OD-009 | AI wording guard cho cụm cấm "hệ thống" | AI/Brand owner |
| P3_1-OD-010 | CRM grace exception frequency | CRM owner |

## 7. Cross-phase conflict rules

| Conflict | Cách xử lý |
| --- | --- |
| Phase 3.1 khác Phase 3 về final price | Phase 3 Commerce + QuoteSnapshot thắng. Phase 3.1 ghi blocker. |
| Phase 3.1 khác Product Knowledge về SKU/pillar | Product owner truth thắng. Phase 3.1 ghi blocker. |
| Phase 3.1 khác Operational về sellable/stock | Operational/Availability truth thắng. |
| AI muốn wording khác message registry | Message registry thắng nếu template locked. |
| Gateway muốn public giá | Public/private guard thắng. |
| CRM muốn gửi khi open case | Suppression thắng. |
| Ads muốn dùng order waiting | Verified Revenue rule thắng. |
| IVR muốn tự hủy đơn | Order State Machine thắng. |

## 8. Blocker carry-over

Các blocker phải carry sang downstream nếu chưa đóng:

- `P3_1-BLOCKED-MISSING-MEMBER-CORE`.
- `P3_1-BLOCKED-MISSING-QUOTE-SNAPSHOT`.
- `P3_1-BLOCKED-MISSING-DIAMOND-RUNTIME`.
- `P3_1-BLOCKED-MISSING-MESSAGE-REGISTRY`.
- `P3_1-BLOCKED-MISSING-CRM-SUPPRESSION`.
- `P3_1-BLOCKED-MISSING-PRODUCT-EFFECTIVENESS`.
- `P3_1-BLOCKED-MISSING-FINAL-GUARD`.
- `P3_1-BLOCKED-MISSING-PAYMENT-RECONCILIATION`.
- `P3_1-BLOCKED-MISSING-SHIPPING-ELIGIBILITY`.
- `P3_1-BLOCKED-MISSING-IVR-DECISION`.
- `P3_1-BLOCKED-MISSING-OWNER-DECISION`.

## 9. Release boundary

PHASE-03.1 không có quyền:

- Evidence accepted.
- Smoke accepted.
- Release ready.
- Production ready.
- Go-live approved.
- Gateway open.
- Scale approved.

Các quyền này thuộc release/evidence/owner gate hiện hành.

## 10. Review checklist

Trước khi sếp/owner ký:

- Tên file tiếng Việt.
- Source coverage đủ 6 nguồn.
- Workstream đủ 02-08.
- Không file tiếng Anh cũ làm nguồn chính.
- Không conflict Phase 1/2/3/4/5/8.
- Owner decision rõ.
- Blocker carry-over rõ.
- Release boundary rõ.

## 11. Done gate

File 10 DONE khi:

- Cross-phase chain rõ.
- Downstream lock rõ.
- Policy carry-over rõ.
- Owner decisions rõ.
- Conflict rules rõ.
- Blocker carry-over rõ.
- Release boundary rõ.
- Không release claim.

## 12. Final status

`PHỤ LỤC KHÓA LIÊN GIAI ĐOẠN 3.1 COMPLETE - NOT RELEASE READY - GATEWAY BLOCKED`
