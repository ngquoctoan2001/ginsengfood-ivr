# Canonical CRM / Member Lifecycle Runtime

Ngày lập: 2026-06-05
Trạng thái: `CANONICAL_RUNTIME_LOCK`
Gateway: `BLOCKED`

## 1. Mục đích

Tài liệu này chọn lớp canonical cho CRM 12 tháng, chăm sóc sau mua, tin nhắn thành viên và member lifecycle từ các bản bổ sung. Các file trong `docs/documents/4. phase/phase-3.1/5. bo sung/`, đặc biệt file 3 và file 6, là source baseline/reference, không được render trực tiếp ra khách nếu chưa qua registry và guard trong tài liệu này.

## 2. CRM không phải tin nhắn khuyến mãi

CRM chỉ được gửi khi có dữ liệu thật và trigger thật:

| CRM nhóm | Trigger hợp lệ | Suppression bắt buộc |
| --- | --- | --- |
| Post-purchase care | Paid/delivered/usage window theo policy | opt-out, open complaint/refund/privacy/payment case |
| Product education | Product purchased + allowed content | fatigue, quiet hours, child/health-sensitive guard |
| Reorder / winback | Expected consumption window + verified purchase | frequency cap, no fake scarcity |
| Member lifecycle | Member cycle/runtime variables | missing runtime, dispute, downgrade tone guard |
| Diamond / referral care | Diamond status + referral event | finance/commission pending wording guard |

Không được bịa lịch sử mua, hạng thành viên, doanh số, số tiền còn thiếu, ngày còn lại hoặc hoa hồng.

## 3. Customer / Member context tối thiểu

```yaml
CustomerMemberContext:
  customer_id: required_if_known
  customer_identity_status: guest | known | verified
  verified_purchase_status: boolean
  last_purchase_public_summary: optional_public_safe
  recipient_profile: optional
  member_status: non_member | member | diamond | suspended | unknown
  member_tier_display: optional
  cycle_start_date: optional
  cycle_end_date: optional
  days_to_cycle_end: optional
  accumulated_valid_revenue_display: optional
  amount_to_next_tier_display: optional
  amount_to_maintain_tier_display: optional
  grace_status: none | active | recovered | expired | unknown
  opt_out_status: required
  open_case_status: required
  quiet_hour_status: required
  frequency_cap_status: required
```

Nếu thiếu biến runtime thì message phải dùng fallback mềm, không được thay bằng câu khẳng định.

## 4. Member lifecycle rule

| Stage | Rule canonical | Customer-facing stance |
| --- | --- | --- |
| New member | Bắt đầu chu kỳ 12 tháng khi đủ điều kiện và policy version active. | Chúc mừng, giải thích quyền lợi bằng ngôn ngữ dễ hiểu. |
| Maintain tracking | Cần tối thiểu 50% chuẩn hạng trong chu kỳ theo policy. | Nhắc mềm, không dọa mất hạng. |
| Upgrade | Đủ chuẩn hạng tiếp theo theo verified valid revenue. | Chúc mừng khi runtime confirmed. |
| Grace | Hết chu kỳ chưa đạt nhưng policy cho ân hạng 60 ngày. | Nói còn bao nhiêu ngày và còn thiếu bao nhiêu nếu có runtime. |
| Downgrade | Hết grace chưa đạt. | Không làm khách xấu hổ; route chăm sóc lại. |
| Dispute/open case | Có khiếu nại/quyền riêng tư/payment/refund. | Dừng bán hàng, route human/support. |

Member lifecycle chỉ dựa trên verified revenue hợp lệ, không dựa comment, lead, inbox, order draft hoặc unpaid order.

## 5. Canonical message registry schema

Mỗi message phải có:

```yaml
MessageRegistryItem:
  message_code: required
  canonical_source: required
  trigger: required
  lifecycle_stage: optional
  eligible_segment: required
  required_variables: list
  fallback_if_missing_variable: required
  channel_scope: public | private | notification
  quiet_hour_policy: required
  frequency_cap: required
  suppression_guards: list
  language_guard_status: required
  forbidden_terms_scan_status: required
  approved_text_hash: required
  normalized_customer_text: required
  raw_source_line_refs: required_if_from_file_3_or_6
  evidence_required: required
```

Không message nào được gửi nếu thiếu `message_code`, `trigger`, `required_variables` và `suppression_guards`.

## 6. Customer-facing language guard

Các từ/khái niệm sau không được xuất hiện trong message gửi khách:

| Cấm | Cách nói thay thế |
| --- | --- |
| hệ thống | bên Em, Ginsengfood ghi nhận, theo chính sách hiện hành |
| SKU | tên sản phẩm công khai |
| runtime / resolver / guard / core / gateway | không nói với khách |
| QuoteSnapshot | báo giá của Mình |
| MISA / audit / internal | không nói với khách |
| cost / supplier | không nói với khách |

File bổ sung 3 và 6 hiện có nhiều đoạn dùng "hệ thống". Các đoạn đó phải đổi trước khi đưa vào registry.

## 7. Message source decision

| Source | Vai trò sau cleanup |
| --- | --- |
| `4. phase/phase-3.1/5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Source baseline cho tone chăm sóc và các tình huống đời thường; không render trực tiếp. |
| `4. phase/phase-3.1/5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Appendix/reference cho message variants và biến cần chuẩn hóa. |
| Canonical registry trong tài liệu này | Nguồn dev dùng để render, validate và test. |

Nếu file 3 và file 6 khác nhau, registry canonical thắng. Dev không được tự chọn raw text; chỉ được render `message_code` đã có `canonical_source`, `approved_text_hash`, suppression guards và language guard pass. Raw variant chưa map vào registry bị coi là reference-only, không phải blocker source mới.

## 7.1. Order success / post-purchase care registry lock

Message code bắt buộc: `TPL-OFFICIAL-ORDER-SUCCESS-CARE`.

Trigger bắt buộc: `order_code_created`.

Required variables: `order_code`, `payment_or_shipping_public_next_step`, `post_purchase_care_allowed`.

Suppression guards: opt-out, open complaint/refund/privacy/payment/quality case, quiet hours nếu gửi chủ động sau hội thoại, frequency cap và language guard.

Customer-facing rule: Chỉ được chúc mừng đơn hàng thành công sau khi runtime trả `order_code`. Không được nói đã thanh toán, đã xuất kho hoặc đang giao nếu `payment_state_public` / `shipping_state_public` chưa xác nhận. Nếu thiếu biến hoặc guard fail thì không render message này.

Raw text từ file 3/6 có từ "hệ thống" phải được chuẩn hóa sang `normalized_customer_text`, có `approved_text_hash` và scan pass trước khi đưa vào registry. Không render trực tiếp raw paragraph.

## 8. P0 smoke bắt buộc

| Test ID | Scenario | Expected |
| --- | --- | --- |
| CRM-MEM-CAN-P0-001 | Khách opt-out | Không gửi CRM. |
| CRM-MEM-CAN-P0-002 | Quiet hour 22:00 | Dời sang khung hợp lệ. |
| CRM-MEM-CAN-P0-003 | Missing member runtime | Không bịa hạng/doanh số/ngày còn lại. |
| CRM-MEM-CAN-P0-004 | Open complaint/refund/privacy | Suppress sales CRM, route support. |
| CRM-MEM-CAN-P0-005 | Downgrade | Tone an toàn, không làm khách xấu hổ. |
| CRM-MEM-CAN-P0-006 | Duplicate same-month trigger | Cross-channel dedup. |
| CRM-MEM-CAN-P0-007 | Message có "hệ thống" hoặc "SKU" | Language guard block. |
| CRM-MEM-CAN-P0-008 | Diamond commission pending | Không nói hoa hồng đã ghi nhận nếu Finance chưa pass. |
| CRM-MEM-CAN-P0-009 | Raw file 3/6 còn từ "hệ thống" | Không render; chỉ cho qua khi có normalized_customer_text + approved_text_hash + language guard pass. |
| CRM-MEM-CAN-P0-010 | order_code_created | Render `TPL-OFFICIAL-ORDER-SUCCESS-CARE` nếu đủ biến/guard; không nói paid/shipped ngoài runtime. |

## 9. Done gate

CRM/Member chỉ được gọi canonical khi:

1. Có message registry với code/trigger/variables/suppression.
2. File 3/6 đã được map thành canonical hoặc reference-only; không còn nhánh source cạnh tranh.
3. Quiet hours, frequency cap, opt-out, fatigue và open-case suppression có evidence.
4. Member lifecycle dùng verified valid revenue.
5. Customer-facing language guard pass.
6. Gateway vẫn `BLOCKED` nếu chưa có Completion evidence accepted.
7. `TPL-OFFICIAL-ORDER-SUCCESS-CARE` có registry item, trigger `order_code_created`, required variables và suppression guards đầy đủ trước khi dùng sau mua.
