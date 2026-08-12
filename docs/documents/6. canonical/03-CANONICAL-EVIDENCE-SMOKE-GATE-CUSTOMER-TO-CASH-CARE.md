# Canonical Evidence / Smoke Gate - Customer-to-Cash-to-Care

Ngày lập: 2026-06-05
Trạng thái: `CANONICAL_GATE_REQUIRED`
Completion status: `PENDING_EVIDENCE`

## 1. Mục đích

Gate này nối Phase 3 Commerce, Phase 4 AI Advisor, Phase 5 Gateway, CRM/Member, Finance/Diamond và IVR. Mỗi phase có thể có smoke cục bộ, nhưng chỉ gate này mới cho phép gọi `GLOBAL_SMOKE_PASS`, `COMPLETION_PASS`, `RELEASE_READY` hoặc `PRODUCTION_READY`.

## 2. Status vocabulary

| Status | Ý nghĩa |
| --- | --- |
| `EVIDENCE_NOT_SUBMITTED` | Chưa có proof. |
| `EVIDENCE_SUBMITTED` | Có proof dev gửi, chưa accepted. |
| `EVIDENCE_ACCEPTED` | Reviewer/owner accepted. |
| `LOCAL_SMOKE_PASS` | Một lane pass cục bộ. |
| `GLOBAL_SMOKE_PASS` | Tất cả P0 cross-system pass và evidence accepted. |
| `COMPLETION_PASS` | Completion report pass sau evidence review. |
| `RELEASE_READY` | Release review pass. |
| `PRODUCTION_READY` | Go-live owner decision pass. |

Không được dùng status cao hơn khi status thấp chưa pass.

## 3. Cross-system smoke matrix

| Gate ID | Lane | P0 proof bắt buộc |
| --- | --- | --- |
| ESM-P0-001 | Product/Sellable | Product active không đồng nghĩa sellable; recall/sale lock/quality hold chặn mọi channel. |
| ESM-P0-002 | DailySalesContext | Hôm nay bán gì/giá gì/tồn gì trả từ runtime, không bịa. |
| ESM-P0-003 | QuoteSnapshot | Final price chỉ từ QuoteSnapshot, immutable, có expiry. |
| ESM-P0-004 | Order | Order Draft không phải Official Order; order_code chỉ sau customer confirmation hợp lệ. |
| ESM-P0-005 | Payment | Selected không phải Paid; Paid chưa chắc verified revenue. |
| ESM-P0-006 | Shipping/ETA | Không nói xuất kho/giao hàng/tracking nếu runtime chưa xác nhận. |
| ESM-P0-007 | AI Advisor | AI không tự tính giá/tồn/claim/payment/order; language guard pass. |
| ESM-P0-008 | Gateway | Public/private boundary pass; no PII/final quote/payment/order leak. |
| ESM-P0-009 | CRM/Member | Opt-out, quiet hours, frequency cap, open case suppression pass. |
| ESM-P0-010 | Diamond/Finance | Commission chỉ eligible sau verified revenue + policy + finance checkpoint; no payout fake. |
| ESM-P0-011 | Ads/ROAS | ROAS chỉ dùng verified revenue, không dùng lead/comment/inbox. |
| ESM-P0-012 | IVR | Attempt policy canonical; technical retry không tăng customer attempt; no real call nếu release gate fail. |
| ESM-P0-013 | Idempotency | Webhook/comment/confirmation/payment/callback retry không tạo duplicate. |
| ESM-P0-014 | Evidence | Correlation id, resolver trace, guard trace, state transition và owner decision ref đầy đủ. |
| ESM-P0-015 | Gender/age add-on | Female self/male self/elderly calcium add-ons đúng context, có ProductRoleMatrix proof và no treatment claim. |
| ESM-P0-016 | Bulk/distributor | Bulk buyer/corporate gift/distributor intent split pass; no self discount, no false distributor routing. |
| ESM-P0-017 | Order success care | order_code_created render approved success/care template; no paid/shipped wording ngoài runtime. |
| ESM-P0-018 | Runtime providers | DailySalesContext, ProductRoleMatrix, QuoteSnapshot, OrderDraft, ShippingState, CRMJobLog và guard trace có provider evidence thật. |

## 4. Evidence packet tối thiểu

```yaml
EvidencePacket:
  evidence_id: required
  lane: required
  related_gate_ids: list
  source_docs: list
  command_or_test_ref: required
  fixture_ref: required
  runtime_trace:
    correlation_id: required
    idempotency_key: required
    resolver_trace_id: required
    guard_trace_id: required
    decision_ids: list
  runtime_provider_refs:
    daily_sales_context_ref: required_if_lane_ai_sales
    product_role_matrix_ref: required_if_lane_ai_product
    quote_snapshot_ref: required_if_quote
    order_draft_ref: required_if_order
    shipping_state_ref: required_if_shipping
    crm_job_log_ref: required_if_crm
    final_response_guard_ref: required_if_customer_output
  source_line_refs: list
  message_registry_refs: list
  normalized_message_hashes: required_if_crm_message
  result:
    status: pass | fail | blocked | skipped_with_reason
    reviewer_status: pending | accepted | rejected
  screenshots_or_logs: required_if_applicable
```

Evidence không được là câu kết luận tài liệu. Phải có log, fixture, trace hoặc proof tương ứng.

## 5. Blocking rules

| Nếu còn | Không được gọi |
| --- | --- |
| Policy ref / selected option thiếu trong runtime evidence | Global Smoke Pass, Release Ready, Production Ready |
| Gateway P0 evidence pending | Gateway PASS, Completion PASS |
| QuoteSnapshot P0 fail | AI/Gateway/CRM quote flow PASS |
| CRM language guard fail | CRM auto-send PASS |
| Finance approval hoặc MISA mapping active missing | Payout/MISA Production Ready |
| IVR attempt smoke/evidence missing | IVR implementation ready / real customer call |
| Gender/age add-on proof missing | AI Product Consultation PASS |
| Bulk/distributor split proof missing | Bulk quote / distributor routing PASS |
| Order success template registry/evidence missing | Order success / post-purchase care PASS |
| Runtime provider refs missing | Global Smoke Pass, P4 Evidence Submitted |

## 6. Done gate toàn hệ

Toàn hệ chỉ đạt khi:

1. Mọi P0 gate ESM-P0-001 đến ESM-P0-018 có evidence accepted.
2. Mọi policy refs trong MASTER-08 có selected option và evidence trace hoặc blocker fail-closed rõ.
3. Gateway Completion Report chuyển từ PENDING_EVIDENCE sang accepted bằng proof thật.
4. Không có source conflict chưa phân loại trong Phase 3/4/5/CRM/Finance/IVR.
5. Release/Go-live quyết định tách riêng theo MASTER-07.
