# IVR-03 - Eligibility Rule / Customer Trust / Official Contact

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa điều kiện đơn/khách nào cần IVR, khách trusted được skip thế nào và contact nào được phép gọi.

## 1. Mục tiêu

Eligibility là chốt chặn để IVR không gọi đại trà, không gọi sai đối tượng và không gọi khi nguồn sự thật chưa sẵn sàng. IVR chỉ được gọi Official Order đủ điều kiện từ Commerce Order Core.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Rule chương trình, boundary và call script. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Eligibility, trusted customer exclusion, phone validation. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Official Order boundary. |
| `docs/documents/4. phase/phase-4/09-README-PHASE-04-HANDOFF-INDEX.md` | Customer Memory/Context Resolver là consumer, không tự tạo business truth. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | Gap closure về eligibility, official contact và phone validation. |

## 3. Input bắt buộc

| Field | Owner | Bắt buộc | Ghi chú |
| --- | --- | --- | --- |
| `order_id` | Order Core | Có | Official Order only. |
| `order_code_short` | Order Core | Có | Biến được phép đọc trong script. |
| `order_version` | Order Core | Có | Guard chống stale callback/race. |
| `order_state` | Order Core | Có | Phải là trạng thái được phép IVR. |
| `program_code` | Commerce Runtime | Có | `GOLDEN_HOUR` hoặc `TWENTY_FOUR_SEVEN`. |
| `customer_ref` | Customer/Commerce | Có | Không truyền full profile. |
| `customer_trust_status` | Customer Trust Resolver | Có | `NEW`, `UNTRUSTED`, `PARTIALLY_TRUSTED`, `TRUSTED`. |
| `risk_flags` | Commerce/Risk | Có | Chỉ dùng flag có source-backed policy. |
| `official_contact_id` | Customer/Commerce | Có | Contact được phép gọi. |
| `phone_ref` | Customer/Commerce | Có | Tham chiếu bảo mật hoặc token. |
| `phone_masked` | Customer/Commerce | Có | Hiển thị admin. |
| `phone_validation_status` | Phone Validation Resolver | Có | Không production nếu unknown. |
| `sale_lock_status` | Operational Core | Có | Blocker. |
| `recall_status` | Operational Core | Có | Blocker. |
| `suppression_status` | Operational/Gateway | Có | Blocker. |
| `call_restriction_status` | Compliance/Customer | Có | Opt-out/legal/call restriction. |

## 4. Eligibility decisions

| Decision | Ý nghĩa | Hành động |
| --- | --- | --- |
| `ELIGIBLE_FOR_IVR` | Official Order đủ điều kiện IVR | Tạo CallJob. |
| `NOT_ELIGIBLE_NOT_OFFICIAL_ORDER` | Không phải Official Order | Reject task, ghi evidence. |
| `NOT_ELIGIBLE_TRUSTED_CUSTOMER` | Khách trusted, không có risk mới | Skip IVR, trả skip reason cho Core. |
| `NOT_ELIGIBLE_POLICY_BLOCKED` | Policy/source chưa sẵn sàng | Fail-safe, admin review. |
| `NOT_ELIGIBLE_OPERATIONAL_BLOCKED` | Sale Lock/Recall/Suppression/availability block | Stop IVR, handoff Core. |
| `NOT_ELIGIBLE_CALL_RESTRICTED` | Opt-out/legal/call restriction | Stop IVR, handoff Core. |
| `NOT_ELIGIBLE_PHONE_INVALID` | Phone invalid trước attempt | Không gọi, trả invalid phone outcome. |
| `ROUTE_HUMAN_REVIEW` | Ambiguous/risk cần người xử lý | Admin/CSKH review queue. |

## 5. Khách cần IVR

Một đơn được xét IVR khi có ít nhất một điều kiện:

- Khách mới.
- Khách chưa đủ trust.
- Customer profile thiếu dữ liệu xác thực.
- Official contact mới hoặc bất thường.
- Đơn có risk flag theo policy.
- Customer Memory chưa đủ dữ liệu để bỏ qua IVR.
- Đơn thuộc chương trình yêu cầu xác nhận theo policy.

## 6. Trusted customer skip

Khách trusted được skip IVR chỉ khi tất cả điều kiện đúng:

- Customer Trust Resolver trả `TRUSTED`.
- Commerce/Customer Memory cho phép trusted checkout.
- Official contact ổn định và được duyệt.
- Không có risk flag mới.
- Không có Sale Lock/Recall/Suppression/call restriction.
- Order state/program/window còn hợp lệ.

IVR không được hardcode trusted customer bằng:

- Tên khách.
- Số điện thoại thô.
- Lịch sử chat.
- Admin note không có policy/evidence.
- Cảm tính nhân viên.

`Owner Decision Required`: ngưỡng trust cụ thể và danh sách risk flag buộc trusted customer vẫn phải IVR.

## 7. Official contact policy

IVR chỉ được gọi `official_contact_id` do Commerce/Customer Profile cung cấp. Projection sang IVR chỉ gồm dữ liệu tối thiểu:

- `official_contact_id`.
- `phone_ref`.
- `phone_masked`.
- `phone_e164_or_adapter_token` nếu security/privacy policy cho phép.
- `contact_source`.
- `contact_verified_at`.
- `contact_policy_version`.

Không được truyền full profile, full address, payment detail, health note, CRM note hoặc nội dung tư vấn riêng tư.

## 8. Phone validation

| State | Ý nghĩa | Hành động |
| --- | --- | --- |
| `PHONE_NOT_EVALUATED` | Chưa validate | Validate trước khi tạo CallJob. |
| `PHONE_VALID` | Có thể gọi theo policy | Tiếp tục. |
| `PHONE_INVALID_FORMAT` | Sai định dạng | Không gọi, invalid outcome. |
| `PHONE_INVALID_UNREACHABLE_CONFIRMED` | Invalid theo rule owner-approved | Không gọi hoặc final invalid theo policy. |
| `PHONE_UNKNOWN_NEEDS_REVIEW` | Không đủ căn cứ | Human review hoặc policy fallback. |
| `PHONE_VALIDATION_TECHNICAL_ERROR` | Lỗi resolver | Technical exception, không tính lỗi khách. |

Invalid phone không được ghi là no-answer. Technical validation error không được ghi là invalid phone nếu chưa có rule owner-approved.

## 9. Fail-safe

Không được tạo CallJob nếu thiếu:

- Order Core status.
- Customer Trust Resolver.
- Phone Validation Resolver.
- Sale Lock/Recall/Suppression check.
- Attempt Policy Resolver.
- Evidence Registry.
- Privacy/Call Restriction check.

Fail-safe outcome: `ELIGIBILITY_POLICY_UNAVAILABLE` và `ADMIN_REVIEW_REQUIRED`.

## 10. Acceptance criteria

- SRS trả lời rõ khách mới/untrusted cần IVR như thế nào.
- SRS trả lời rõ khách trusted skip theo rule nào.
- Official contact được chọn, validate, mask và audit rõ.
- Invalid phone, no-answer và technical validation error không bị trộn.

## 11. Eligibility decision tree

```text
START
  -> Is entity Official Order?
       NO  -> BLOCK_QUOTE_CART_DRAFT
       YES -> Is order state IVR-callable?
                NO  -> BLOCK_POLICY
                YES -> Is Sale Lock/Recall/Suppression/opt-out active?
                         YES -> BLOCK_OPERATIONAL or BLOCK_OPT_OUT
                         NO  -> Is trusted skip allowed and no risk flag?
                                  YES -> SKIP_TRUSTED_CUSTOMER
                                  NO  -> Is official contact available?
                                           NO  -> BLOCK_INVALID_PHONE or OWNER_REVIEW_REQUIRED
                                           YES -> Validate phone
                                                    valid -> ELIGIBLE_FOR_IVR
                                                    invalid -> BLOCK_INVALID_PHONE
                                                    technical error -> OWNER_REVIEW_REQUIRED
```

## 12. Required eligibility inputs

| Input | Owner | Required | Use |
| --- | --- | --- | --- |
| `entity_type` | Order Core | Có | Chặn Quote/Cart/Draft. |
| `official_order_id` | Order Core | Có | Link order. |
| `order_state` | Order Core | Có | Chỉ trạng thái callable. |
| `order_version` | Order Core | Có | Race guard. |
| `customer_ref` | Customer/Commerce | Có | Trust lookup. |
| `customer_trust_status` | Trust Resolver | Có | Trusted skip. |
| `risk_flags` | Risk/Order Core | Có | Trusted vẫn phải IVR nếu có risk. |
| `official_contact_id` | Customer/Commerce | Có | Contact được phép gọi. |
| `phone_ref` | Customer/Commerce | Có | Không dùng raw phone nếu không cần. |
| `phone_masked` | Customer/Commerce | Có | Admin display. |
| `phone_validation_status` | Phone Resolver | Có | Valid/invalid/inconclusive. |
| `sale_lock_snapshot` | Operational Core | Có | Blocker. |
| `recall_snapshot` | Operational Core | Có | Blocker. |
| `suppression_snapshot` | Operational/Compliance | Có | Blocker. |
| `call_restriction_snapshot` | Compliance/Customer | Có | Opt-out/legal block. |

## 13. Trusted customer skip rules

Trusted skip chỉ được áp dụng khi tất cả điều kiện sau đúng:

- Trust resolver trả decision rõ ràng.
- Không có risk flags bắt buộc IVR.
- Order state vẫn hợp lệ.
- Không có Sale Lock/Recall/Suppression/opt-out.
- Contact/payment/order snapshot không có thay đổi làm tăng rủi ro.
- Policy version được ghi lại.

Trusted skip không được áp dụng khi:

| Điều kiện | Hành vi |
| --- | --- |
| New customer | Require IVR nếu đủ điều kiện. |
| Trust resolver unavailable | `OWNER_REVIEW_REQUIRED` hoặc fallback owner-approved. |
| Phone mới đổi gần đây | Owner Decision Required; mặc định không skip nếu risk. |
| High value order | Owner Decision Required; mặc định require IVR nếu risk flag. |
| Prior cancellation/no-show risk | Require IVR nếu risk flag. |

## 14. Official contact selection

Priority:

1. Contact được Order Core đánh dấu là official contact cho order.
2. Contact từ customer profile projection được owner duyệt.
3. Không tự chọn contact khác nếu official contact invalid.

Rules:

- Không gọi số từ comment/chat/free text.
- Không gọi số từ CRM note.
- Không gọi số từ lịch sử cũ nếu không được resolver trả về.
- Không gọi full address/contact list.
- Nếu có nhiều official contact, resolver phải trả priority rõ.

## 15. Phone validation taxonomy

| Status | Meaning | IVR action |
| --- | --- | --- |
| `PHONE_VALID` | Có thể gọi theo policy. | Eligible tiếp. |
| `PHONE_INVALID_FORMAT` | Format sai. | Block invalid phone. |
| `PHONE_MISSING` | Không có phone. | Block/review. |
| `PHONE_NOT_OFFICIAL_CONTACT` | Phone không thuộc contact chính thức. | Block. |
| `PHONE_OPTED_OUT` | Opt-out/suppressed. | Block opt-out. |
| `PHONE_VALIDATION_INCONCLUSIVE` | Không đủ kết luận. | Review/fail-safe. |
| `PHONE_VALIDATION_TECHNICAL_ERROR` | Resolver lỗi kỹ thuật. | Technical/review, không invalid final. |

## 16. Eligibility output contract

| Field | Required | Note |
| --- | --- | --- |
| `eligibility_decision_id` | Có | Trace decision. |
| `official_order_id` | Có | Link order. |
| `decision` | Có | Enum decision. |
| `trusted_customer_skip` | Có | True/false. |
| `blocked_reasons` | Có | Empty nếu eligible. |
| `policy_version` | Có | Source policy. |
| `evaluated_at` | Có | Timestamp. |
| `source_refs` | Có | Sources used. |
| `evidence_refs` | Có nếu persisted | Evidence. |
| `audit_refs` | Có | Audit. |

## 17. Edge cases

| Edge case | Required behavior |
| --- | --- |
| Order changed contact after task created | Revalidate before dispatch; if mismatch, block/recreate task. |
| Phone valid at intake but opt-out before attempt | Block attempt, no call. |
| Trusted at intake but risk flag appears before dispatch | Require/block/review per policy, no blind skip. |
| Recall appears after key `1` | Core blocks callback. |
| Phone resolver timeout | No call unless approved fallback. |
| Customer has multiple phones | Use official selected contact only. |

## 18. Eligibility P0 tests

| Test ID | Scenario | Expected |
| --- | --- | --- |
| IVR03-P0-001 | Quote task | `BLOCK_QUOTE_CART_DRAFT`. |
| IVR03-P0-002 | Trusted no risk | `SKIP_TRUSTED_CUSTOMER`. |
| IVR03-P0-003 | Trusted with risk | Not skipped; require IVR/review. |
| IVR03-P0-004 | Invalid phone | No SIM dispatch. |
| IVR03-P0-005 | Phone validation technical error | Not invalid final; review/technical. |
| IVR03-P0-006 | Sale Lock active | Block operational. |
| IVR03-P0-007 | Opt-out active | Block opt-out. |
