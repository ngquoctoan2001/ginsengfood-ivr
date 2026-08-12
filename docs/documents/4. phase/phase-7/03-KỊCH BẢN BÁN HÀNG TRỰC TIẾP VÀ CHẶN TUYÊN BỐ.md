# P7.2B - Live Script Runtime / Claim Guard

**BACKLOG:** LIVE-BLG-002 / LIVE-BLG-003  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tao/giai quyet script live tu approved source va bat buoc qua Script Guard / Claim Guard truoc khi host/operator dung.

## Entry Gate

- LIVE-BLG-001 pass.
- AIA-BLG-002 Product Knowledge / Claim Guard pass.
- Approved script/product knowledge source available.

## Implementation Scope

- Script runtime resolver.
- Product story generation from approved public-safe data.
- Claim Guard enforcement.
- Script version/evidence.
- Guard fail -> no use/human review.

## Not Allowed

- Script tu noi san pham nhu thuoc/chua benh.
- Claim chua approved van vao script.
- Script hardcode gia/discount/ton kho.
- Script bypass guard vi dang live.

## Acceptance Criteria

- Script co source/version.
- Script pass Claim Guard truoc khi hien thi/dung.
- Guard fail co reason va no delivery/use.
- Product story khong lo recipe/BOM/supplier/private data.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2B-SMK-001 | Approved product story | Script pass |
| P7.2B-SMK-002 | Medical treatment claim | Guard fail |
| P7.2B-SMK-003 | Internal formula in script | Guard fail |
| P7.2B-SMK-004 | Hardcoded price | Guard fail/block |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Live Script Runtime / Claim Guard

### 9.1. Purpose lock

`LIVE-BLG-002/003` chỉ cho phép tạo cue/script từ source approved và chỉ khi qua guard. Script/cue là gợi ý cho host/operator hoặc response layer đã qua Gateway; không phải câu trả lời production tự động.

### 9.2. HostCue contract

Mọi script/cue phải map được về `schemas/live/host-cue.schema.json`:

```yaml
host_cue_required_fields:
  host_cue_id: required
  version: v1
  live_plan_id: required
  live_session_id: required
  cue_type: required
  cue_text: required
  sku_id: conditional
  quote_snapshot_id: conditional_if_final_price
  product_recommendation_id: optional
  claim_guard_result_id: required_if_product_or_claim
  public_safe: required
  allowed_to_present: required
  no_fake_urgency: true
```

Nếu `allowed_to_present=false`, cue không được hiển thị cho host dưới dạng usable script; chỉ được hiển thị trong review/internal evidence.

### 9.3. Allowed cue types for SRS

| Cue type | Public allowed | Required guard |
|---|---:|---|
| `OPENING` | Yes | Privacy + fake urgency guard. |
| `BRAND_STORY` | Yes | Claim guard + product story source. |
| `PRODUCT_SPOTLIGHT` | Conditional | Product scope + claim guard + sellable guard. |
| `PRODUCT_COMPARISON` | Conditional | Claim guard + no medical promise + no unapproved comparison. |
| `OBJECTION_HANDLING` | Conditional | Claim guard + public/private boundary. |
| `CTA_PUBLIC_SAFE` | Yes | Gateway boundary + no final price/order closing. |
| `PRIVATE_HANDOFF` | Yes, ack only | Gateway handoff eligibility. |
| `SUPPRESSION_NOTICE` | Yes, safe wording | Suppression/risk guard. |
| `CLOSING` | Yes | No fake urgency/scarcity. |

Nếu official enum chưa có, SRS phải ghi là `CONTRACT_TAXONOMY_PENDING`, không tự tạo enum trong code mà không cập nhật contract.

### 9.4. Claim guard truth table

| Input condition | Guard result | Required action |
|---|---|---|
| Claim approved and public-safe | `PASS` | Cue allowed if all other guards pass. |
| Claim source missing | `FAIL_SOURCE_MISSING` | Block cue; create risk/evidence. |
| Medical treatment/chữa bệnh/thay thuốc | `FAIL_MEDICAL_CLAIM` | Block cue; no public output. |
| Internal formula/BOM/supplier/private proof | `FAIL_PRIVATE_DATA` | Block cue; mask evidence. |
| Hardcoded final price | `FAIL_PRICE_BOUNDARY` | Block cue unless `quote_snapshot_id` valid and policy permits. |
| Fake scarcity/urgency/social proof | `FAIL_FAKE_URGENCY` | Block cue; create `LiveRiskCue`. |
| Product outside live plan scope | `FAIL_PRODUCT_SCOPE` | Block cue. |
| SKU sale lock/recall/not sellable | `FAIL_SELLABLE` | Block sales cue; create risk cue. |

### 9.5. Script source policy

Script/cue source must be one of:

1. Approved product knowledge.
2. Approved live plan/run-of-show.
3. Approved MC AI script brief.
4. Public-safe product scope line.
5. Owner-approved operational note for the live session.

Not allowed:

- Raw recipe/BOM/formula/supplier.
- Internal ads/campaign/ROAS text.
- Owner chat note without approval.
- Screenshot-only evidence.
- Past live script reused without revalidation.
- Any wording that implies guaranteed cure, treatment, medical result, fake scarcity or fake inventory.

### 9.6. Runtime unavailable behavior

If any critical source is unavailable:

| Unavailable source | Required behavior |
|---|---|
| Product/claim approval | No product story cue. |
| Commerce QuoteSnapshot | No final price cue. |
| Gateway public/private guard | No public/private response suggestion. |
| Sale lock/recall resolver | No sales CTA. |
| Evidence registry | No completion/pass claim. |

### 9.7. LiveRiskCue creation

When guard fails with risk, create or require `LiveRiskCue`:

```yaml
live_risk_cue:
  risk_type:
    enum_hint:
      - CLAIM_UNAPPROVED
      - MEDICAL_TREATMENT_CLAIM
      - PRIVATE_DATA_LEAK
      - FAKE_URGENCY
      - FAKE_SCARCITY
      - PRICE_BOUNDARY_BREACH
      - SALE_LOCK_OR_RECALL
      - PUBLIC_ORDER_CLOSING
  suppress_sales: true
  handoff_required: false_or_policy_based
```

### 9.8. Acceptance matrix

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2B-AC-001 | Cue source | Every cue has source refs and live plan refs. |
| P7-2B-AC-002 | Claim guard | Product/claim cue cannot be allowed without `claim_guard_result_id`. |
| P7-2B-AC-003 | No medical claim | Treatment/cure/medicine replacement wording is blocked. |
| P7-2B-AC-004 | No private leak | Formula/BOM/supplier/private proof does not appear in cue. |
| P7-2B-AC-005 | No fake urgency | `no_fake_urgency=true`; fake scarcity/social proof creates risk cue. |
| P7-2B-AC-006 | No self-price | Final price wording requires `quote_snapshot_id`; otherwise blocked. |
| P7-2B-AC-007 | No auto-send | HostCue creation never sends to customer directly. |

### 9.9. Evidence required

| Evidence ID | Artifact |
|---|---|
| P7-2B-EVD-001 | Approved product story cue with source refs and guard pass. |
| P7-2B-EVD-002 | Medical claim blocked with guard trace. |
| P7-2B-EVD-003 | Internal formula/private data blocked. |
| P7-2B-EVD-004 | Hardcoded price blocked. |
| P7-2B-EVD-005 | Fake urgency/scarcity blocked. |
| P7-2B-EVD-006 | Out-of-scope SKU blocked. |

### 9.10. SRS no-go

SRS fails if it allows any of:

- "Host có thể nói giá hôm nay..." without QuoteSnapshot.
- "Sản phẩm chữa/điều trị/thay thuốc".
- "Chỉ còn vài suất" without approved real inventory/evidence and policy.
- "MC AI tự trả lời khách".
- "Script guard optional vì đang live".

