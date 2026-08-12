# 01. FILE FACEBOOK ADS & LIVE COMMERCE OPERATING MODEL

**Scope:** `BUSINESS_OPERATING_MODEL_REFERENCE`
**Phase:** `PHASE_5_FACEBOOK_CHANNEL_GATEWAY`
**Authority:** `REFERENCE_ONLY__NOT_RUNTIME_TRUTH`
**Gateway:** `PRODUCTION_BLOCKED`
**Ngày bổ sung:** 2026-06-05

## 1. Mục đích

File này lấp lỗ trống của tài liệu bổ sung Phase 5. Nội dung dùng để dev hiểu business operating model của Facebook Ads / Live Commerce trước khi tách SRS cho Facebook Channel Gateway.

File này không phải:

- Source of truth về giá cuối.
- Source of truth về order.
- Source of truth về payment.
- Source of truth về verified revenue.
- Source of truth về ROAS chính thức.
- Source of truth về Diamond commission.
- Source of truth để bật production Gateway.

Mọi runtime truth vẫn thuộc Commerce, Ads Measurement, Finance/Diamond, CRM, Master Evidence và release owner tương ứng.

## 2. Operating surfaces

| Surface | Vai trò business | Gateway được làm | Gateway bị cấm |
| --- | --- | --- | --- |
| Facebook public post comment | Tạo tín hiệu nhu cầu và câu hỏi ban đầu. | Safe acknowledgement, classify intent, route private/human. | Public final price, payment, order, PII, health-sensitive detail. |
| Facebook live comment | Tín hiệu mua/hỏi giá/hỏi sản phẩm trong live. | Preserve live context, classify, handoff Messenger/human. | Chốt đơn công khai hoặc spam reply giống nhau. |
| Messenger private | Hội thoại tư vấn/chốt theo guard. | Bind thread, call AI Advisor, deliver guarded response. | Tự tính giá, tự tạo order, bypass guard. |
| Ads click/message source | Attribution entry. | Store campaign/source refs if redacted and policy allows. | Tự gọi ROAS/CPA/AOV PASS. |
| Comment-to-Messenger handoff | Chuyển khách từ public sang private. | Gửi handoff khi permission/policy pass. | Nói đã gửi khi handoff failed/blocked. |
| Human CSKH/sales route | Xử lý complaint, refund, health-sensitive, policy unknown. | Create human handoff with context and pause automation. | Tiếp tục automation sales khi human takeover active. |

## 3. Funnel model tham khảo

```text
Ad impression/click/message
-> Public comment or Messenger entry
-> Gateway normalize + classify
-> Safe public ack or private handoff
-> Messenger private context
-> AI Advisor guarded advice
-> Commerce quote/cart/order handoff
-> Customer confirmation
-> Official order/payment/fulfillment
-> ORDER_VERIFIED
-> Ads/ROAS measurement
-> CRM/Member/Finance/Diamond downstream if eligible
```

Phase 5 chỉ sở hữu các bước từ `Gateway normalize + classify` đến `delivery/evidence` của kênh Facebook/Messenger. Các bước commerce/revenue/commission là downstream owner.

## 4. Business metrics reference

| Metric | Business meaning | Runtime owner | Phase 5 handling |
| --- | --- | --- | --- |
| Comment volume | Mức tương tác public/live. | Ads/Live/Gateway evidence. | Count as raw funnel signal only. |
| Messenger handoff rate | Tỷ lệ public chuyển được private. | Gateway evidence. | Measure sent/failed/blocked; not revenue. |
| Response latency | Độ trễ phản hồi kênh. | Gateway delivery. | Measure with pacing/rate-limit context. |
| Quote request | Khách có nhu cầu báo giá. | AI/Commerce. | Preserve intent; no final price. |
| Order draft | Khách đủ điều kiện lập draft. | Commerce. | Handoff only. |
| ORDER_VERIFIED | Doanh thu xác minh. | Commerce / Ads Measurement. | Consume as downstream evidence only. |
| ROAS/CPA/AOV | Hiệu quả ads. | Ads Measurement. | Cannot compute PASS from comment/inbox. |
| Diamond commission | Hoa hồng giới thiệu. | Finance/Diamond. | Preserve referral; no payout calculation. |

## 5. Public wording constraints

Public/live wording must:

- Ngắn, an toàn, không lộ thông tin riêng.
- Không dùng "sale sốc", "xả kho", "flash sale" nếu business lock yêu cầu dùng "Giờ Vàng Tri Ân".
- Không trả giá cuối nếu thiếu QuoteSnapshot từ Commerce.
- Không nói "đã tạo đơn", "đã thanh toán", "đã xác nhận doanh thu" nếu thiếu owner runtime.
- Không hứa tác dụng điều trị hoặc kết luận y khoa.
- Không tranh luận dài với troll/complaint.
- Không kêu khách tự nhắn thay cho handoff thật nếu hệ thống có thể handoff.

## 6. Live commerce guardrails

| Guard | Requirement |
| --- | --- |
| Product scope | Live chỉ nói sản phẩm thuộc live board/script/product scope đã duyệt. |
| Price | Public live không nói final quote; chỉ safe ack hoặc private handoff. |
| Scarcity/urgency | Không tạo khan hiếm giả hoặc gấp giả. |
| PII | Không đọc/lặp số điện thoại/địa chỉ public. |
| Complaint | Route CSKH/human, suppress sales. |
| Spam | Không trả nhiều comment giống nhau theo kiểu spam. |
| Measurement | Live comment không phải verified revenue. |

## 7. Ads attribution guardrails

Gateway được preserve:

```yaml
AdsAttributionReference:
  source_platform: facebook
  page_registry_id: required
  campaign_ref: optional_redacted
  adset_ref: optional_redacted
  ad_ref: optional_redacted
  click_ref: optional_redacted
  message_ref: optional_redacted
  live_session_id: optional
  normalized_event_id: required
  customer_channel_ref: redacted
```

Gateway không được:

- Tạo campaign budget decision.
- Kết luận ad winner/loser.
- Tính CPA/ROAS từ comment, inbox, quote hoặc order draft.
- Gửi offline conversion nếu thiếu verified revenue owner.
- Đẩy Pixel/CAPI production nếu thiếu owner/App/permission/evidence.

## 8. SRS extraction for Phase 5

| SRS group | Requirement extracted |
| --- | --- |
| Page Registry | Every surface must resolve to page role and production status. |
| Webhook | Every inbound event must be signed, normalized and idempotent. |
| Public/private | Public cannot leak private/commercial/health-sensitive details. |
| Messenger | Thread must be bound before private response. |
| AI Guard | No AI response delivery without guard trace. |
| Delivery | Pacing, retry, rate-limit and evidence are mandatory. |
| Suppression | Opt-out/open case/sale lock/recall wins. |
| Attribution | Preserve source; do not compute revenue/commission. |
| Evidence | No smoke/release claim without artifact package. |

## 9. Final status

`REFERENCE_MODEL_READY_FOR_SRS_EXTRACTION`

`NOT_RUNTIME_TRUTH`

`GATEWAY_PRODUCTION_BLOCKED`
