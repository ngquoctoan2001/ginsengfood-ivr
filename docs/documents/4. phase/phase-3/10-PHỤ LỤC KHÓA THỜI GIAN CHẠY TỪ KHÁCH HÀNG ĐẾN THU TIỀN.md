# P3 - Addendum Customer-to-Cash Runtime Lock

Ngày lập: 2026-06-05
Trạng thái: `ADDENDUM_REQUIRED_BEFORE_IMPLEMENTATION`
Global Gateway: `BLOCKED`

## 1. Mục đích

Addendum này là bản viết lại lớp thực thi Phase 3 theo các bản gôm bổ sung về AI Advisor, CRM/Member, Diamond, Finance, Gateway và IVR. Phase 3 là nơi sở hữu sự thật thương mại từ lúc khách có nhu cầu đến khi có doanh thu xác minh. Phase 4/5/CRM/Ads/Finance chỉ được consume kết quả Phase 3, không được tự tính thay.

## 2. Source-of-truth của Phase 3

| Nhóm | Owner truth | Consumer |
| --- | --- | --- |
| Sellable | Product + Operational + Inventory + Sale Lock/Recall | AI, Gateway, Live, CRM, Ads |
| Price / Program | Commerce Pricing Runtime | AI, Gateway, CRM, Live |
| Member / Diamond benefit trong báo giá | Commerce + Member policy version | AI, CRM, Quote renderer |
| QuoteSnapshot | Commerce Runtime | Order Draft, AI, Gateway, CRM |
| Order Draft / Confirmation | Commerce Runtime | Official Order |
| Payment | Payment Core | Verified Revenue, Finance |
| Shipping / ETA | Shipping/Delivery Runtime | AI, Gateway, CRM |
| Verified Revenue | Commerce + Payment/Order checkpoint | Ads, Diamond, Finance |

Không domain downstream nào được dùng file message, live script, AI prompt hoặc Gateway event để tự quyết định giá, quyền lợi, trạng thái đơn, doanh thu hoặc hoa hồng.

## 3. DailySalesContext contract

Phase 3 phải cung cấp một context public-safe cho câu hỏi "hôm nay bán gì", "còn gì", "giá nào", "chương trình nào":

```yaml
DailySalesContext:
  business_date: required
  channel: required
  active_programs: required
  program_time_window: required_if_program_active
  product_public_name: required
  public_category: required
  dietary_type: vegan | non_vegan | mixed | unknown
  target_roles: list
  listed_price_display: required_if_sellable
  program_price_display: required_if_program_eligible
  member_benefit_eligible: boolean
  diamond_benefit_eligible: boolean
  sellable_status: sellable | low_stock | sold_out | blocked | unknown
  quote_eligible: boolean
  policy_version_refs: required
```

Fail-closed bắt buộc:

| Thiếu dữ liệu | Response bắt buộc |
| --- | --- |
| Thiếu sellable status | Không nói còn hàng; route kiểm tra thêm. |
| Thiếu policy version | Không báo giá chương trình/member/Diamond. |
| Thiếu QuoteSnapshot | Không nói final total. |
| Product bị recall/sale lock/quality hold | Không bán và không gợi ý thay nếu chưa có substitute public-safe. |

## 4. QuoteSnapshot runtime lock

QuoteSnapshot là nguồn duy nhất cho giá cuối. QuoteSnapshot phải immutable và có expiry.

Required fields:

```yaml
QuoteSnapshot:
  quote_snapshot_id: required
  customer_context_id: required
  channel_context_id: required
  line_items:
    - product_public_name
    - quantity
    - listed_price
    - program_price
    - member_benefit_amount
    - diamond_benefit_amount
    - line_total
    - quote_line_reason
  shipping_fee: required_or_pending
  cod_fee: required_or_pending
  vat_display: required_or_not_applicable
  final_total: required_when_complete
  quote_created_at: required
  quote_expires_at: required
  policy_version_refs: required
  quote_status: active | expired | voided | converted
```

Không được:

- AI/Gateway/CRM tự cộng giá, chiết khấu, ship, COD hoặc VAT.
- Dùng listed price làm final price.
- Tạo Order Draft nếu QuoteSnapshot đã expired/voided.
- Cộng dồn member benefit và Golden Hour nếu policy priority chưa owner-approved.

## 5. Order state bridge

Phase 3 phải khóa state machine để downstream không nói sai:

| State | Chủ sở hữu | Downstream được nói gì |
| --- | --- | --- |
| `CUSTOMER_INTERESTED` | Channel/AI input | Chỉ tư vấn, chưa có báo giá. |
| `QUOTE_SNAPSHOT_CREATED` | Commerce | Có báo giá tạm tính theo hạn giữ giá. |
| `ORDER_DRAFT_RENDERED` | Commerce | Có đơn nháp để khách xác nhận. |
| `CUSTOMER_CONFIRMED` | Commerce | Khách đã xác nhận thông tin, chưa đồng nghĩa official order nếu chưa tạo order_code. |
| `ORDER_CODE_CREATED` | Commerce | Đơn đã ghi nhận chính thức với mã đơn. |
| `PAYMENT_PENDING` | Payment | Chờ thanh toán/xác nhận COD. |
| `PAYMENT_CONFIRMED` | Payment | Đã xác nhận thanh toán, chưa đồng nghĩa verified revenue nếu checkpoint chưa pass. |
| `READY_FOR_FULFILLMENT` | Order/Fulfillment | Đủ điều kiện chuyển xử lý kho/giao hàng. |
| `WAREHOUSE_RELEASED` | Fulfillment/Warehouse | Đã xuất kho khi runtime xác nhận. |
| `HANDED_TO_CARRIER` | Delivery | Đã bàn giao vận chuyển khi delivery runtime xác nhận. |
| `DELIVERED` | Delivery | Đã giao theo delivery runtime. |
| `ORDER_VERIFIED` | Commerce/Payment checkpoint | Được dùng cho Ads/ROAS/Commission/Finance. |

Forbidden claims:

- Không nói "đã đặt đơn" nếu chưa có `order_code`.
- Không nói "đã thanh toán" nếu Payment Core chưa confirmed.
- Không nói "đã xuất kho" nếu Fulfillment/Warehouse chưa released.
- Không nói "đang giao" nếu Delivery Runtime chưa có tracking/delivery state.
- Không tạo verified revenue từ Paid/Delivered/Invoice riêng lẻ nếu chưa có `ORDER_VERIFIED`.

## 6. Shipping / ETA lock

```yaml
ShippingResponsePolicy:
  SHIPPING_INFO_MISSING:
    action: ask_minimum_address_in_private_channel
  SHIPPING_INFO_AVAILABLE_NO_TRACKING:
    action: return_region_eta_fallback_if_policy_allows
  TRACKING_AVAILABLE:
    action: return_public_safe_tracking_status
  HANDED_TO_CARRIER:
    action: say_handed_to_carrier_only_if_runtime_confirms
  DELIVERY_EXCEPTION:
    action: route_delivery_support
```

Không được hỏi lại địa chỉ nếu đơn đã có shipping info hợp lệ. Không public mã đơn, địa chỉ, số điện thoại hoặc trạng thái thanh toán ở comment công khai.

## 7. Verified revenue, Diamond, ROAS và Finance handoff

| Output | Điều kiện tối thiểu | Consumer |
| --- | --- | --- |
| `VerifiedRevenueRecord` | `ORDER_VERIFIED` + payment/order checkpoint + no reversal blocker | Ads, Finance, Diamond |
| `CommissionEligibilitySnapshot` | Referral bind + buyer/order eligible + verified revenue + owner policy version | Diamond/Finance |
| `ROASHandoffRecord` | Verified revenue + attribution source + campaign/page context | Ads |
| `PayoutEligibilityRecord` | Commission eligible + finance checkpoint + tax/policy decision | Finance/MISA handoff |

Phase 3 không execute payout thật. Finance/Diamond canonical quyết định payable boundary.

## 8. P0 smoke bắt buộc

| Test ID | Scenario | Expected |
| --- | --- | --- |
| P3-ADD-P0-001 | Khách hỏi hôm nay bán gì | Chỉ trả sản phẩm public sellable, không lộ SKU/internal. |
| P3-ADD-P0-002 | Khách hỏi giá | Có QuoteSnapshot hoặc fail-safe, không tự tính. |
| P3-ADD-P0-003 | Chương trình hết hạn | Không dùng giá cũ. |
| P3-ADD-P0-004 | Product sale lock/recall | Block bán hàng trên AI/Gateway/CRM. |
| P3-ADD-P0-005 | Quote expired rồi xác nhận | Reject, không tạo order. |
| P3-ADD-P0-006 | Retry customer confirmation | Idempotent, không tạo duplicate order_code. |
| P3-ADD-P0-007 | Payment selected COD | Không set Paid. |
| P3-ADD-P0-008 | Paid chưa ORDER_VERIFIED | Không tạo verified revenue/commission/ROAS. |
| P3-ADD-P0-009 | Shipping info đã có | Không hỏi lại địa chỉ; trả ETA theo runtime. |
| P3-ADD-P0-010 | Diamond referral order | Chỉ tạo eligibility snapshot, không payout. |

## 9. Done gate

Phase 3 chỉ được coi là đủ cho Phase 4/5/CRM/Finance consume khi:

1. DailySalesContext có contract và P0 evidence.
2. QuoteSnapshot immutable, complete và expire đúng.
3. Order state bridge có proof từ quote đến `ORDER_VERIFIED`.
4. Shipping/ETA không bịa trạng thái.
5. Verified revenue không sinh từ signal chưa đủ.
6. Commission/ROAS/Finance chỉ nhận handoff sau verified checkpoint.
7. Gateway vẫn `BLOCKED`.
