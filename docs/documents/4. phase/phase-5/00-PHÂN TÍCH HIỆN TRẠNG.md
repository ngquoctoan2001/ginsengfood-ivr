# P5 - Analysis Only

**Phase:** PHASE-05 - Facebook Channel Gateway / Meta Live / Messenger Delivery  
**Mode:** `ANALYSIS_ONLY`  
**Ngày viết lại:** 2026-06-05  
**Gateway:** `PRODUCTION_BLOCKED`  
**Release:** `NOT_RELEASE_READY`

## 1. Mục tiêu analysis

Phân tích current state trước khi thiết kế hoặc triển khai Facebook Channel Gateway. File này chỉ dùng để inspect tài liệu, code, API, DB, config, test, log, evidence và owner decision. Không sửa code, không tạo migration, không seed, không bật webhook production, không tạo Page Access Token thật trong tài liệu.

## 2. Kết luận nguồn bổ sung

Các tài liệu bổ sung đạt mức `PASS_AS_BUSINESS_OPERATING_BASELINE`: đủ làm nền chiến lược, tài chính, Page Registry, Ads/Live operating model và Gateway transition gate.

Chưa đủ để dev triển khai production Gateway ngay vì còn thiếu:

- Gateway API contract.
- Database object contract.
- Pixel / CAPI / Offline Conversion contract.
- App Review / permission checklist.
- Runbook vận hành và rollback.
- Evidence package pass cho AI Advisor Facebook Completion Report.

`01. FILE FACEBOOK ADS & LIVE COMMERCE OPERATING MODEL.md` hiện là file rỗng, không có nội dung để phân tích.

## 3. Entry gate

Chỉ bắt đầu analysis khi đã đọc:

- `docs/documents/4. phase/phase-4/BẢNG GÔM GIAI ĐOẠN 4.md`
- `docs/documents/4. phase/phase-4/10-P4-PHỤ LỤC KHÓA RUNTIME THỰC CHIẾN AI ADVISOR.md`
- `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/AI_ADVISOR_FACEBOOK_COMPLETION_REPORT.md`
- `docs/documents/4. phase/phase-5/TÀI LIỆU BỔ SUNG/TRÌNH TỰ TRIỂN KHAI TIẾP THEO.md`

Nếu AI Advisor Completion Report chưa `PASS`, ghi:

`P5-ENTRY-BLOCKED-BY-AI-ADVISOR-COMPLETION-EVIDENCE`

## 4. Scope phân tích

1. Meta Business/App/Page Registry current state.
2. Page role: Commerce Hub, live spoke, ads spoke, test page, CRM allowed.
3. Token/secret governance, app mode, app review status.
4. Webhook endpoint, verify token handshake, signature validation.
5. Raw webhook storage, normalized event, idempotency, dedup, retry, DLQ.
6. Live Session Registry and comment ingestion.
7. Public comment boundary and system-initiated Messenger handoff.
8. Messenger thread binding and source attribution preservation.
9. AI Advisor routing and Final Response Guard enforcement.
10. Ads campaign/adset/ad attribution, Pixel/CAPI/offline mapping.
11. ORDER_VERIFIED revenue measurement.
12. Diamond referral attribution and commission eligibility handoff.
13. Rate-limit, anti-spam, moderation, complaint split.
14. Suppression, opt-out, CRM quiet hour, frequency cap.
15. Evidence package, dashboard, pilot, scale gate, rollback.

## 5. Business locks phải đối chiếu

| Nhóm | Giá trị khóa |
| --- | --- |
| Revenue engine | Giờ Vàng Tri Ân là chính; 24/7 là CRM/reorder/reactivation. |
| Price wording | Không dùng "sale sốc/xả kho/flash sale"; dùng "Giờ Vàng Tri Ân". |
| Quote hold | Giờ Vàng 5 phút, 24/7 15 phút; hết hạn phải revalidate. |
| AOV | 1 hộp hợp lệ; 2-3 hộp là growth target; AI/Gateway không ép combo. |
| Ads cost | Tháng 1-2: 20% verified revenue; tháng 3-4: 17%; tháng 5-6: 15%. |
| ROAS | Chỉ tính từ ORDER_VERIFIED, không tính comment/inbox/quote/order_created. |
| Diamond | Commission chỉ sau valid referral bind và Order Verified. |

## 6. Output bắt buộc

Analysis report phải có:

- Current-state map: API, route, service, config, DB/table, worker, queue, test, log, evidence.
- Page Registry gap matrix.
- Gateway API gap matrix.
- Database object gap matrix.
- Pixel/CAPI/offline conversion gap matrix.
- App Review/permission gap matrix.
- Public/private risk register.
- Ads/ROAS measurement risk register.
- Diamond attribution risk register.
- P0 smoke matrix proposal.
- Owner decision register.

## 7. P0 blockers

- AI Advisor Completion Report chưa PASS nhưng Gateway production được mở.
- Gateway tự quote/order/CRM.
- Gateway public final price, PII, payment, order hoặc health-sensitive detail.
- Webhook thiếu signature validation hoặc idempotency.
- Page Access Token/App Secret/Verify Token xuất hiện trong tài liệu/evidence.
- ROAS tính từ quote/inbox/comment/order chưa verified.
- Diamond commission tính trước ORDER_VERIFIED.
- CRM gửi khi opt-out/open case/quiet hour/frequency cap.

## 8. Done gate

Analysis chỉ DONE khi có report rõ:

- `PASS_AS_BUSINESS_OPERATING_BASELINE` cho tài liệu nguồn.
- `PRODUCTION_BLOCKED` cho Gateway nếu Completion Report chưa PASS.
- Missing contracts cụ thể cho từng file P5.1 đến P5.2G.
- Không kết luận release-ready.

## 9. Final status tối đa

`P5_ANALYSIS_COMPLETED_WITH_GATEWAY_PRODUCTION_BLOCKED`

## 10. SRS hardening addendum - analysis baseline

Phần này biến file phân tích thành đầu vào SRS cho dev. Khi dev đọc Phase 5, mọi kết luận trong file này phải được hiểu là yêu cầu phân tích hiện trạng và phân loại blocker, không phải xác nhận implementation đã tồn tại.

### 10.1. Source hierarchy bắt buộc

| Tầng nguồn | Vai trò với Phase 5 | Cách dùng trong SRS |
| --- | --- | --- |
| Master source-of-truth / dependency / evidence / release control | Khóa ranh giới, completion vocabulary, release vocabulary. | Nếu Phase 5 xung đột với Master, Master thắng. |
| PACK-06 Facebook Channel Gateway | Source chính cho Page, webhook, Messenger, live comment, delivery, smoke. | Mọi requirement Gateway phải trace về PACK-06 hoặc TECH-06. |
| TECH-06 Facebook Gateway | Source kỹ thuật chính cho module, state, API, DB, delivery, evidence. | Dev triển khai theo TECH-06; Phase 5 chỉ chia nhỏ để bàn giao. |
| PACK-05 / TECH-05 AI Advisor | Upstream AI behavior và Final Response Guard. | Gateway chỉ gọi/tiêu thụ, không viết lại AI. |
| Phase 3 Commerce / canonical evidence | Quote, order, payment, verified revenue. | Gateway không tự tạo truth; chỉ giữ reference và trace. |
| Canonical CRM / Finance-Diamond / Evidence | Opt-out, CRM, commission, global smoke, production-ready wording. | Dùng để chặn false PASS và false revenue/commission. |
| Schemas / OpenAPI / state-machines | Contract surface hiện có. | Nếu thiếu schema chi tiết, ghi explicit gap thay vì tự bịa. |
| Tài liệu bổ sung Phase 5 | Business, ads, page list, AI report, triển khai thực chiến. | Chỉ là baseline/reference; phải normalize trước khi dùng. |

Không dùng file `BẢNG GÔM GIAI ĐOẠN 5.md` làm nguồn trong lượt bổ sung này theo phạm vi yêu cầu.

### 10.2. Analysis assertions phải giữ nguyên

| Assertion ID | Nội dung khóa | Hệ quả dev |
| --- | --- | --- |
| P5-ANL-A001 | Phase 5 là Facebook Channel Gateway, không phải AI Advisor. | Không tạo prompt engine, product reasoning, customer memory hoặc claim guard riêng trong Gateway. |
| P5-ANL-A002 | Gateway không phải Commerce Runtime. | Không tạo quote/order/payment/shipping/verified revenue. |
| P5-ANL-A003 | Gateway không phải Ads optimizer. | Không tự quyết campaign, budget, ROAS PASS, CPA PASS hoặc scale decision. |
| P5-ANL-A004 | Gateway không phải CRM worker. | Không tự gửi lifecycle/marketing CRM; chỉ deliver command hợp lệ nếu channel policy cho phép. |
| P5-ANL-A005 | Gateway không phải Finance/Diamond runtime. | Không tính hoa hồng, payout, payable hoặc finance checkpoint. |
| P5-ANL-A006 | Public comment không phải nơi chốt đơn. | Public chỉ safe ack hoặc chuyển private/human theo policy. |
| P5-ANL-A007 | Messenger private vẫn phải guard. | Private không được bypass QuoteSnapshot, Final Response Guard, suppression hoặc opt-out. |
| P5-ANL-A008 | Evidence thiếu thì status là blocked/pending. | Không dùng "đã viết tài liệu" để gọi PASS. |

### 10.3. Current-state review checklist cho dev

Dev phải hoàn tất checklist này trước khi chuyển sang thiết kế implementation:

| Checklist ID | Câu hỏi kiểm tra | Evidence tối thiểu | Nếu thiếu |
| --- | --- | --- | --- |
| P5-ANL-C001 | Có Page Registry chuẩn hóa từ mọi Page/Profile/Business/App ID chưa? | Bảng registry có object type, owner, role, permission, app review, production status. | `PAGE_REGISTRY_INCOMPLETE`. |
| P5-ANL-C002 | Webhook có verify token, signature validation và replay/idempotency chưa? | Contract hoặc test cho verify, signature, event key, duplicate. | `WEBHOOK_TRUST_INCOMPLETE`. |
| P5-ANL-C003 | Public/private boundary đã có policy matrix chưa? | Matrix surface x intent x allowed action. | `PUBLIC_PRIVATE_POLICY_INCOMPLETE`. |
| P5-ANL-C004 | Comment-to-Messenger có state model success/fail/blocked chưa? | State transitions và delivery log. | `HANDOFF_STATE_INCOMPLETE`. |
| P5-ANL-C005 | Messenger thread binding có customer/session mapping chưa? | Thread binding schema + stale/mismatch behavior. | `MESSENGER_BINDING_INCOMPLETE`. |
| P5-ANL-C006 | Gateway gọi AI Advisor theo contract nào? | Request/response envelope + Final Response Guard trace. | `AI_HANDOFF_CONTRACT_INCOMPLETE`. |
| P5-ANL-C007 | Delivery pacing/rate-limit/moderation có rule chưa? | Rule table, state, retry, dead-letter, smoke. | `DELIVERY_POLICY_INCOMPLETE`. |
| P5-ANL-C008 | Suppression/opt-out/open case/sale lock/recall có thắng delivery không? | Suppression decision envelope. | `SUPPRESSION_CHAIN_INCOMPLETE`. |
| P5-ANL-C009 | Ads/ROAS/Diamond chỉ preserve attribution không? | Data map chứng minh no revenue/commission calculation. | `ATTRIBUTION_BOUNDARY_INCOMPLETE`. |
| P5-ANL-C010 | Smoke/evidence có artifact schema chưa? | Evidence package template và pass/fail rule. | `EVIDENCE_MODEL_INCOMPLETE`. |

### 10.4. Conflict taxonomy

Mọi xung đột phát hiện khi đọc nguồn phải gán một loại:

| Conflict bucket | Ví dụ | Resolution rule |
| --- | --- | --- |
| `SOURCE_OF_TRUTH_CONFLICT` | Supplement nói Gateway có thể scale nhưng Master/Canonical chưa evidence. | Master/Canonical thắng; Phase 5 ghi blocked. |
| `ROLE_BOUNDARY_CONFLICT` | Gateway tự tính giá hoặc hoa hồng. | Owner runtime thắng; Gateway chỉ preserve reference. |
| `PUBLIC_PRIVATE_CONFLICT` | Public comment chứa PII, giá cuối hoặc payment. | Public-safe guard thắng; route private/human. |
| `EVIDENCE_STATUS_CONFLICT` | Completion report chữ PASS nhưng thiếu artifact thật. | Evidence gate thắng; status pending/blocked. |
| `PAGE_REGISTRY_CONFLICT` | Raw list trộn Page/Profile/Business/App ID. | Normalize trước; unknown quarantined. |
| `PRODUCTION_WORDING_CONFLICT` | Tài liệu gọi production-ready khi chưa App Review/smoke. | `PRODUCTION_BLOCKED`. |

### 10.5. Analysis output format

Mỗi report hiện trạng của Phase 5 phải có đúng cấu trúc:

```yaml
phase_5_analysis_report:
  scope: facebook_channel_gateway
  mode: analysis_only
  production_status: PRODUCTION_BLOCKED
  source_set:
    primary_pack: PACK-06
    primary_tech: TECH-06
    upstream_ai: PACK-05/TECH-05
    canonical_gates: CRM/Finance/Evidence
  findings:
    - finding_id: P5-ANL-Fxxx
      layer: source|api|db|event|ui|security|evidence|release
      severity: P0|P1|P2
      status: confirmed|needs_owner|blocked
      source_evidence: path-or-artifact
      required_followup: concrete action
  final_verdict: P5_ANALYSIS_COMPLETED_WITH_GATEWAY_PRODUCTION_BLOCKED
```

### 10.6. No-go claims

File phân tích không được tạo hoặc hàm ý các câu sau:

- `Gateway implementation complete`.
- `Gateway smoke pass`.
- `App Review pass`.
- `Webhook production verified`.
- `Messenger production verified`.
- `ROAS pass`.
- `Diamond commission pass`.
- `CRM delivery pass`.
- `Release ready`.
- `Production ready`.

Chỉ được dùng `blocked`, `pending evidence`, `analysis completed`, `design handoff ready` khi evidence tương ứng chưa có.

