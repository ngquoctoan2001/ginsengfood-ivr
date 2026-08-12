# P4 - MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI

**Phase:** PHASE-04 - AI Advisor Runtime / AI Channel Readiness
**Ngày khóa:** 2026-06-05
**Trạng thái tài liệu:** `DEV_SRS_HANDOFF_READY`
**Implementation status:** `IMPLEMENTATION_ALLOWED_ONLY_AFTER_SRS_ACCEPTED_AND_OWNER_GATES`
**Gateway:** `PRODUCTION_BLOCKED`
**Release:** `NOT_RELEASE_READY`

## 1. Khóa chống conflict Phase 4

Tài liệu này là cầu nối từ Phase 4 canonical rewrite sang SRS và implementation backlog. Nó không override `12-BẢNG GOM GIAI ĐOẠN 4.md`; nếu có khác biệt về owner, contract, gate, evidence hoặc readiness, áp dụng `12-BẢNG GOM GIAI ĐOẠN 4.md` và xử lý fail-closed.

Các cụm `DEV_SRS_HANDOFF_READY`, `SRS_READY`, `implementation backlog`, `local-ready`, `DONE` hoặc `PASS cục bộ` chỉ có nghĩa là tài liệu đã đủ để dev phân tích sang SRS. Chúng không phải `GATEWAY_PASS`, không phải `COMPLETION_PASS`, không phải `RELEASE_READY`, không phải `PRODUCTION_READY` và không phải Go-live.

## 2. Mục tiêu bàn giao

Dev nhận Phase 4 phải tạo được SRS và implementation plan mà không tự suy diễn nghiệp vụ.

Output tối thiểu của dev sau khi đọc Phase 4:

1. SRS Phase 4 có requirement ID rõ.
2. Requirement traceability matrix nối từ source doc -> SRS ID -> implementation work package -> test/evidence.
3. Current-state gap report từ codebase thật.
4. Technical design hoặc delta design nếu codebase đã có module tương ứng.
5. Implementation backlog có thứ tự, dependency, owner, acceptance criteria.
6. Evidence/smoke plan cho từng P0 gate.
7. Blocker register cho owner decisions còn pending.

Không được bắt đầu code nếu chưa có SRS được review hoặc chưa chứng minh module hiện hữu map được sang SRS.

## 3. Phạm vi SRS

SRS Phase 4 phải mô tả AI Advisor như lớp runtime consumer/orchestrator/renderer.

Phase 4 sở hữu:

- AIAdvisorRequestEnvelope.
- ChannelContext usage.
- DailySalesContext consumption.
- CustomerAdvisoryContext consumption.
- ProductRoleMatrix consumption.
- Recommendation decision tree.
- QuoteSnapshotConsumerView rendering.
- OrderRuntimeConsumerView rendering.
- PaymentPublicResponsePolicy.
- ShippingETAResponsePolicy.
- CRMMemberContext rendering lock.
- DiamondReferralContext rendering lock.
- FinalResponseGuard.
- TypingDelayPolicy.
- DecisionEnvelope.
- Phase4EvidenceItem.

Phase 4 không sở hữu:

- Product/SKU/formula truth.
- Inventory/sellable/recall/sale-lock truth.
- Pricing/discount/member benefit calculation.
- Order/payment/shipping truth.
- CRM scheduler/worker truth.
- Diamond payout/Finance/MISA truth.
- Gateway production, Meta app, Page token, webhook delivery.
- IVR real-call runtime.
- Ads optimizer/ROAS truth.

## 4. SRS readiness gate

Phase 4 được xem là đủ chuẩn để bàn giao dev phân tích sang SRS khi toàn bộ điều kiện sau đúng:

| Gate | Điều kiện |
| --- | --- |
| SRS-GATE-001 | File 00-08 có khối nguồn bắt buộc, conflict lock và trạng thái evidence ở đầu file; khối này được đọc trước nội dung triển khai của từng shard. |
| SRS-GATE-002 | File 00-08 được đọc theo đúng mode; không dùng file ngoài hoặc suy diễn thủ công để override lock đã nhập. |
| SRS-GATE-003 | Mọi source truth upstream/downstream được map vào SRS bằng owner rõ ràng. |
| SRS-GATE-004 | Mọi requirement có acceptance criteria và smoke/evidence tương ứng. |
| SRS-GATE-005 | Mọi side-effect command có idempotency/audit/correlation yêu cầu rõ. |
| SRS-GATE-006 | Mọi public/private rule có negative test. |
| SRS-GATE-007 | Mọi pending owner decision được carry thành blocker, không được tự quyết. |
| SRS-GATE-008 | Không có release/production/Gateway claim. |

Nếu thiếu bất kỳ gate nào, dev chỉ được viết gap report, không được implement.

## 5. Requirement traceability matrix

| SRS ID | Requirement | Source Phase 4 | Upstream/downstream owner | Acceptance criteria | Evidence / smoke |
| --- | --- | --- | --- | --- | --- |
| P4-SRS-FR-001 | AI Advisor phải nhận request qua `AIAdvisorRequestEnvelope` có channel, actor, intent, runtime refs và idempotency/correlation. | Khối nguồn đã nhập 00-08; 02-P4-2A | Phase 4 owns envelope; upstream provides refs. | Missing required field -> fail-closed/handoff; no fake context. | P4-GATE-001, P4-GATE-015, P4-SMK-022 |
| P4-SRS-FR-002 | AI phải consume `DailySalesContext` để trả lời "hôm nay bán gì", chương trình, stock public state. | Khối nguồn đã nhập 00-08; 02-P4-2A | Phase 1/2/3 owners. | Không có context -> không nói sản phẩm/offer cụ thể. | P4-GATE-002, P4-SMK-001, P4-SMK-018 |
| P4-SRS-FR-003 | AI phải consume `CustomerAdvisoryContext` để phân biệt khách mới/cũ/member/Diamond/open case/opt-out. | Khối nguồn đã nhập 00-08; 03-P4-2B | CRM/Member canonical, Phase 3.1. | Public không lộ memory/PII; thiếu/conflict -> hỏi tối thiểu hoặc handoff. | P4-GATE-003, P4-SMK-008, P4-SMK-009, P4-SMK-014, P4-SMK-015 |
| P4-SRS-FR-004 | AI phải dùng `ProductRoleMatrix` khi tư vấn sản phẩm/combo. | Khối nguồn đã nhập 00-08; 04-P4-2C | Product public view from Phase 1/2. | Không có role matrix/public-safe proof -> không recommend. | P4-GATE-004, P4-SMK-006, P4-SMK-007 |
| P4-SRS-FR-005 | AI không được tự quyết sellable, sale lock, recall, not-sellable. | Khối nguồn đã nhập 00-08; 02/04/05-P4 | Phase 2/3 owners. | Any lock/recall/not-sellable -> stop sale; alternative only if allowed. | P4-GATE-001, P4-GATE-004, P4-SMK-006 |
| P4-SRS-FR-006 | Final price chỉ render từ `QuoteSnapshotConsumerView`. | Khối nguồn đã nhập 00-08; 05-P4-2D | Phase 3 Commerce Runtime. | Missing/expired/conflict quote -> revalidate/fail-safe; no hardcode. | P4-GATE-005, P4-SMK-002, P4-SMK-003, P4-SMK-005 |
| P4-SRS-FR-007 | Order official chỉ được nói khi `OrderRuntimeConsumerView` có order_code. | Khối nguồn đã nhập 00-08; 05-P4-2D | Phase 3 Order Runtime. | Order draft/customer confirm không đồng nghĩa official order. | P4-GATE-006 |
| P4-SRS-FR-008 | AI không được nói paid nếu Payment Core/Accounting chưa confirm. | Khối nguồn đã nhập 00-08; 05-P4-2D | Payment Core/Accounting. | Bank image/customer claim/COD selected không thành paid. | P4-GATE-007, P4-SMK-012 |
| P4-SRS-FR-009 | Shipping/ETA/tracking chỉ render từ runtime/fallback policy. | Khối nguồn đã nhập 00-08; 05-P4-2D | Shipping/Delivery Runtime. | Thiếu ETA -> hỏi private hoặc fallback safe; không cam kết bịa. | P4-GATE-008, P4-SMK-013 |
| P4-SRS-FR-010 | CRM/member outbound candidate phải obey suppression, quiet hours, frequency cap, open case, opt-out. | Khối nguồn đã nhập 00-08; 03-P4-2B; 07-P4-2F | CRM/Member canonical. | Any suppression fail -> no CRM send; human/support route if needed. | P4-GATE-009, P4-SMK-014, P4-SMK-015 |
| P4-SRS-FR-011 | Diamond/commission/payout wording phải theo Finance/Diamond state. | Khối nguồn đã nhập 00-08; 03/07-P4 | Finance/Diamond canonical. | Không nói ghi nhận/chi trả nếu state chưa pass. | P4-GATE-010, P4-SMK-011 |
| P4-SRS-FR-012 | Public channel không được leak final quote, PII, payment, order, member tier, commission, health-sensitive detail. | Khối nguồn đã nhập 00-08; 06-P4-2E | Phase 4 guard + Channel policy. | Public comment/live chỉ safe ack/general info/private handoff. | P4-GATE-011, P4-SMK-003, P4-SMK-008 |
| P4-SRS-FR-013 | Customer-facing output phải qua language guard và không dùng thuật ngữ nội bộ như "hệ thống". | Khối nguồn đã nhập 00-08; 06-P4-2E | Phase 4 FinalResponseGuard. | Banned term -> rewrite/block; exact-lock templates preserved. | P4-GATE-012, P4-SMK-016 |
| P4-SRS-FR-014 | AI phải chạy FinalResponseGuard trước mọi render/outbound/handoff candidate. | Khối nguồn đã nhập 00-08; 06-P4-2E | Phase 4 owns guard. | Guard status PASS/REWRITE/BLOCK/HANDOFF captured in trace. | P4-GATE-011, P4-GATE-012, P4-GATE-015 |
| P4-SRS-FR-015 | Typing/pacing phải có delay policy, max cap, cancel/recompute khi có message mới. | Khối nguồn đã nhập 00-08; 07-P4-2F | Phase 4 + Channel runtime. | New message cancels stale response; no duplicate send. | P4-GATE-013, P4-SMK-021 |
| P4-SRS-FR-016 | Gateway chỉ nhận handoff intent/safe candidate; production vẫn blocked. | Khối nguồn đã nhập 00-08; 07-P4-2F | Phase 5 Gateway. | No send if P5 blocked; no Page production assumption. | P4-GATE-014, P4-SMK-020 |
| P4-SRS-FR-017 | DecisionEnvelope phải được tạo cho mọi AI decision có outcome. | Khối nguồn đã nhập 00-08; 02/08-P4 | Phase 4 owns decision trace. | Contains source refs, resolver trace, guard trace, output surface. | P4-GATE-015 |
| P4-SRS-FR-018 | Evidence item phải có artifact/log/fixture/trace, không chỉ narrative. | Khối nguồn đã nhập 00-08; 08-P4-2G | Evidence canonical. | PII masked, secret scan status, owner review status. | P4-GATE-015, P4-SMK-022 |
| P4-SRS-FR-019 | Conflict pricing/GH/member/Diamond/vegan/page/IVR/message baseline phải fail-closed. | Khối nguồn đã nhập 00-08; 09/10-P4 | Multiple owners. | No self-choice; carry owner decision/blocker. | P4-SMK-004, P4-SMK-007, P4-SMK-011 |
| P4-SRS-FR-020 | Admin preview không được tạo outbound command. | Khối nguồn đã nhập 00-08; 07-P4-2F | Admin/Channel runtime. | Preview is internal only; no send/CRM/order side effect. | P4-SMK-020 |
| P4-SRS-FR-021 | Gender/age add-on rule phải phân biệt nữ tự dùng, nam tự dùng và người già/canxi theo ProductRoleMatrix proof. | Khối nguồn đã nhập 00-08; 04-P4 | Product public view + HealthSafetyGuard. | Không có context/proof -> hỏi một câu hoặc bỏ qua; no treatment/hormone/sexual-health claim. | P4-GATE-016, P4-SMK-023, P4-2C-SMK-011, P4-2C-SMK-012, P4-2C-SMK-013 |
| P4-SRS-FR-022 | Bulk buyer/corporate gift/distributor intent phải được tách trước quote/router. | Khối nguồn đã nhập 00-08; 04-P4; Phase 3.1 supplement 2 | BulkBuyIntentResolver, DistributorIntentGuard, BulkQuoteResolver, human support. | Corporate gift không bị gom thành đại lý; distributor/mua sỉ route policy/human; no self discount. | P4-GATE-017, P4-SMK-024, P4-2C-SMK-014 |
| P4-SRS-FR-023 | Order success care message phải dùng approved template sau khi runtime trả order_code. | Khối nguồn đã nhập 00-08; 05-P4; CRM canonical registry đã nhập | Commerce Runtime + CRM/Member canonical. | Có order_code mới chúc mừng đơn thành công; không nói paid/shipped nếu runtime chưa xác nhận. | P4-GATE-018, P4-SMK-025, P4-2D-SMK-013 |
| P4-SRS-FR-024 | Runtime provider evidence pack phải có provider thật cho sales/product/quote/order/shipping/CRM/guard trước khi gọi PASS. | Khối nguồn đã nhập 00-08; 08-P4; evidence lock đã nhập | DailySalesContext, ProductRoleMatrix, QuoteSnapshot, OrderDraft, ShippingState, CRMJobLog, guard trace owners. | Thiếu provider trace -> PENDING_EVIDENCE/BLOCKED; không nâng pass bằng narrative. | P4-GATE-019, P4-SMK-026, P4-EVD-009 |

## 6. Non-functional requirements

| SRS ID | Requirement | Acceptance criteria | Evidence |
| --- | --- | --- | --- |
| P4-SRS-NFR-001 | Privacy/PII safety | Public output contains no phone/address/order/member/payment/private memory. | Guard trace + negative tests |
| P4-SRS-NFR-002 | Secret safety | Evidence/output contains no token, webhook secret, app secret, bank raw config unless approved/masked. | Secret scan status |
| P4-SRS-NFR-003 | Idempotency | Quote/order/handoff side effects use idempotency key or owner-approved equivalent. | Command trace |
| P4-SRS-NFR-004 | Auditability | Every decision has correlation id, actor, source docs, resolver trace, guard trace. | DecisionEnvelope |
| P4-SRS-NFR-005 | Fail-closed behavior | Missing/stale/conflict source truth stops sale or routes handoff, never guesses. | Runtime conflict smoke |
| P4-SRS-NFR-006 | Observability | Logs distinguish resolver failure, guard block, handoff failure, duplicate event, typing cancel. | Log sample |
| P4-SRS-NFR-007 | Performance/UX pacing | Typing delay has cap/cancel/recompute; no stale duplicate response. | P4-SMK-021 evidence |
| P4-SRS-NFR-008 | Localization/tone | Customer-facing Vietnamese follows Ginsengfood tone and banned-word registry. | Language guard smoke |
| P4-SRS-NFR-009 | Testability | Each P0 gate has automated or documented smoke evidence. | P4-GATE/P4-SMK matrix |
| P4-SRS-NFR-010 | Maintainability | Contract adapters are centralized; no hardcode spread across prompt/controller/frontend. | Static audit/report |
| P4-SRS-NFR-011 | Evidence traceability | Provider evidence names real source/runtime/log refs for DailySalesContext, ProductRoleMatrix, QuoteSnapshot, OrderDraft, ShippingState, CRMJobLog and guard trace. | P4-EVD-009 + canonical evidence packet |

## 7. Implementation work packages

| WP ID | Scope | Input docs | Output | Blocker |
| --- | --- | --- | --- | --- |
| P4-WP-00 | Current-state analysis | 00-P4 và khối nguồn đã nhập trong 00-08 | Code/doc/source map, gap register, conflict register | Missing repo/module access |
| P4-WP-01 | SRS and RTM | This file, 09/10-P4 và khối nguồn đã nhập trong 00-08 | SRS, requirement traceability matrix, owner decision register | Owner decision unknown |
| P4-WP-02 | Technical design/delta design | 01-P4, 02-P4-2A | DTO/API/event/state design mapped to existing code | Architecture mismatch |
| P4-WP-03 | Runtime consumer contracts | 02-P4-2A | Envelope/adapters/fail-closed behavior | Missing upstream provider |
| P4-WP-04 | Customer context resolver | 03-P4-2B | CustomerAdvisoryContext + privacy/suppression logic | CRM/member source missing |
| P4-WP-05 | Product recommendation orchestrator | 04-P4-2C | ProductRoleMatrix consumption, recommendation decision tree | Product public proof missing |
| P4-WP-06 | Quote/order/payment/shipping renderer | 05-P4-2D | QuoteSnapshot/order/payment/shipping public-safe renderers | Commerce runtime missing |
| P4-WP-07 | Final Response Guard | 06-P4-2E | Public/private/language/claim/PII guard | Policy registry missing |
| P4-WP-08 | Channel/CRM/Gateway handoff | 07-P4-2F | Handoff intent, typing/pacing, Gateway-blocked bridge | Phase 5 blocked |
| P4-WP-09 | Smoke/evidence package | 08-P4-2G | P0 evidence pack, owner review request | Evidence artifact missing |
| P4-WP-10 | Gender/age add-on hardening | Khối nguồn đã nhập 00-08; 04-P4 | Female/male/elderly add-on resolver + negative claim tests | ProductRoleMatrix proof missing |
| P4-WP-11 | Bulk/distributor hardening | Phase 3.1 supplement 2; 04-P4-2C | Bulk/distributor intent split, corporate gift flow, bulk quote handoff | Bulk policy/human route missing |
| P4-WP-12 | Order success + evidence providers | 05-P4-2D; 06 canonical; 08-P4-2G | Approved order success template and provider evidence pack | CRM registry/provider trace missing |

## 8. Dev analysis checklist before SRS

Dev phải inspect codebase thật và trả lời được:

1. Module nào hiện tạo AI prompt/context?
2. Module nào render câu trả lời cuối?
3. Module nào đang tự tính giá/discount/benefit/stock/sellable/order/payment?
4. Module nào đang gọi hoặc giả định Gateway/Messenger/CRM outbound?
5. DTO/API/event nào hiện có thể map sang `AIAdvisorRequestEnvelope`?
6. Có QuoteSnapshot/OrderRuntimeConsumerView thật chưa?
7. Có CustomerAdvisoryContext hoặc nguồn CRM/member tương đương chưa?
8. Có ProductRoleMatrix hoặc product public view chưa?
9. Có FinalResponseGuard tập trung chưa hay guard đang rải rác?
10. Có evidence/log/correlation/idempotency cho side effects chưa?

Nếu câu trả lời là "chưa rõ", SRS phải ghi `UNKNOWN` và mở gap; không được đoán.

## 9. SRS document structure bắt buộc

SRS Phase 4 phải có tối thiểu các mục:

1. Scope / out-of-scope.
2. Source-of-truth and owner map.
3. Glossary and status vocabulary.
4. User/channel scenarios.
5. Functional requirements `P4-SRS-FR-*`.
6. Non-functional requirements `P4-SRS-NFR-*`.
7. Data contracts and DTO/API/event mapping.
8. State/fail-closed rules.
9. Public/private/language/claim safety.
10. Privacy/security/PII/secret rules.
11. Idempotency/audit/correlation requirements.
12. Test, smoke, evidence matrix.
13. Owner decision/blocker register.
14. Implementation work packages.
15. Release/readiness exclusions.

## 10. Acceptance criteria cho SRS

SRS không đạt nếu:

- Thiếu SRS ID cho requirement quan trọng.
- Requirement không có acceptance criteria.
- Requirement không map được về source file hoặc P0/smoke.
- Có wording cho phép AI tự tính truth.
- Có wording mở Gateway/production/release.
- Có wording cho phép public leak.
- Có owner pending nhưng không carry thành blocker.
- Có evidence gate nhưng không nói artifact cần gì.

SRS đạt để sang implementation backlog khi:

- 100% P4-SRS-FR-001 đến P4-SRS-FR-024 được map.
- 100% P4-SRS-NFR-001 đến P4-SRS-NFR-011 được map.
- Mọi `UNKNOWN` có owner/blocker.
- Work package có thứ tự và dependency.
- Smoke/evidence plan bám P4-GATE-001 đến P4-GATE-019 và P4-SMK-001 đến P4-SMK-026.

## 11. Implementation guardrails

Dev implementation không được:

- Sửa upstream owner truth ngoài phạm vi Phase 4.
- Tạo bảng/migration mới nếu SRS/design chưa yêu cầu.
- Hardcode giá, discount, benefit, stock, sellable, GH schedule, commission, payout.
- Dùng raw message baseline file 3/6 thay canonical registry/guard.
- Render public final quote/order/payment/member/private info.
- Gửi Gateway/Messenger/CRM production outbound khi P5 blocked.
- Gọi release/production/pass nếu evidence chưa accepted.

Dev implementation phải:

- Làm từng WP nhỏ.
- Chạy test/smoke theo WP.
- Cập nhật evidence cùng lúc với code.
- Ghi rõ blocker thay vì bypass.
- Giữ traceability từ code/test về SRS ID.

## 12. Report format bắt buộc sau implementation

Mỗi WP report phải có đúng 14 mục:

1. Tóm tắt phạm vi.
2. File đã sửa.
3. SRS ID đã xử lý.
4. Source docs đã dùng.
5. Runtime owner đã consume.
6. Contract/API/DTO/event đã map.
7. Evidence artifact.
8. Lệnh đã chạy.
9. Kết quả test/smoke.
10. P0 gate status.
11. Privacy/secret scan status.
12. Owner decisions còn pending.
13. Rủi ro còn lại.
14. Handoff tiếp theo.

## 13. Final status

```text
PHASE_4_DEV_SRS_HANDOFF_READY
SRS_REQUIRED_BEFORE_CODE_IMPLEMENTATION
IMPLEMENTATION_ALLOWED_ONLY_AFTER_SRS_ACCEPTED_AND_OWNER_GATES
GATEWAY_PRODUCTION_BLOCKED
NOT_RELEASE_READY
NOT_PRODUCTION_READY
```
