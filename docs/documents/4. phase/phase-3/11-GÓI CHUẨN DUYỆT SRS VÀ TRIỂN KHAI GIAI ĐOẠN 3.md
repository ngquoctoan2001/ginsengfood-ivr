# 11 - GÓI CHUẨN DUYỆT / SRS / TRIỂN KHAI GIAI ĐOẠN 3

## 1. Vai trò tài liệu

Tài liệu này là cổng chuẩn duyệt trước khi đưa Giai đoạn 3 lên ChatGPT, sếp, dev lead hoặc đội triển khai để phân tích sang SRS.

Tài liệu này không thay thế các file Phase 3 chi tiết. Tài liệu này chỉ khóa:

- Thứ tự đọc.
- Ranh giới source-of-truth.
- Cách hiểu giữa Phase 3 và Phase 3.1 bổ sung.
- Checklist để ChatGPT review không bị hiểu sai.
- Cấu trúc SRS mà dev phải tạo trước khi code.
- Gate trước khi triển khai.
- Gate trước khi gọi completion, release hoặc production.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev/dev lead tạo SRS chi tiết, bóc task và triển khai có kiểm soát theo đúng thứ tự. Việc `SRS chi tiết` hoặc runtime implementation chưa có không phải lỗi của vòng review này nếu tài liệu đã khóa đủ nguồn, scope, dependency, gate và checklist để dev tạo SRS trước khi code.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES_FOR_SRS_AND_TASK_BREAKDOWN
READY_TO_HAND_TO_DEV: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## 2. Trạng thái chuẩn duyệt

| Hạng mục | Trạng thái |
| --- | --- |
| Bộ tài liệu Phase 3 gốc | Sẵn sàng làm nguồn phân tích SRS nếu đọc đúng thứ tự |
| Addendum Customer-to-Cash | Bắt buộc đọc trước khi triển khai |
| Phase 3.1 bổ sung | Bắt buộc đọc nếu triển khai member, Diamond, CRM, AI Advisor, messaging, payment/shipping/IVR bổ sung |
| SRS chi tiết | Chưa tạo trong Phase 3, dev phải lập từ bộ nguồn này |
| Implementation | Chưa thực hiện bởi tài liệu này |
| Evidence accepted | Chưa có nếu chưa chạy thật |
| Owner sign-off | Bắt buộc trước implementation |
| Gateway | BLOCKED |
| Release Ready | Không được gọi |
| Production Ready | Không được gọi |

Kết luận trạng thái được phép ghi:

```text
PHASE 3 DOCUMENTATION/SRS-HANDOFF READY
READY TO HAND TO DEV FOR SRS AND CONTROLLED TASK BREAKDOWN
IMPLEMENTATION NOT EXECUTED
EVIDENCE NOT ACCEPTED
OWNER SIGN-OFF REQUIRED
GATEWAY BLOCKED
NOT RELEASE READY
NOT PRODUCTION READY
```

Không được ghi khác trạng thái trên nếu chưa có SRS được duyệt, implementation thật, smoke thật, evidence accepted và owner sign-off.

## 3. Quy tắc không conflict giữa Phase 3 và Phase 3.1

Trong folder `phase-3`, tên `P3.1 - TECHNICAL DESIGN ONLY` là bước thiết kế kỹ thuật nội bộ của Phase 3 gốc.

Trong folder `phase-3.1`, `Giai đoạn 3.1` là phase bổ sung nối tiếp Phase 3, dùng để khóa thêm Diamond, member lifecycle, CRM, messaging, AI Advisor situation locks, payment/shipping/IVR bổ sung.

Hai khái niệm này không được hiểu là một.

| Tên gọi | Ý nghĩa đúng | Không được hiểu là |
| --- | --- | --- |
| `01-P3-1-TECHNICAL-DESIGN-ONLY.md` | Bước thiết kế kỹ thuật của Phase 3 Commerce Runtime | Phase bổ sung Diamond/CRM/AI |
| `docs/documents/4. phase/phase-3.1/` | Phase bổ sung nối tiếp Phase 3 | Thay thế Phase 3 |
| Phase 3 Commerce Runtime | Source-of-truth cho sellable, quote, order, payment, shipping, verified revenue | CRM/AI tự tính nghiệp vụ |
| Phase 3.1 bổ sung | Policy supplement cho member, Diamond, CRM, messaging, AI situation | Owner mới cho final price/order/payment/revenue |

Nếu Phase 3 và Phase 3.1 có vẻ khác nhau:

1. Commerce Runtime trong Phase 3 thắng với final price, QuoteSnapshot, order, payment, shipping, verified revenue.
2. Phase 3.1 thắng với policy bổ sung nếu policy đó không thay owner truth Phase 3.
3. Nếu vẫn conflict, ghi blocker, không tự merge rule.

## 4. Thứ tự đọc bắt buộc trước khi đưa ChatGPT review

Đưa ChatGPT hoặc dev đọc theo đúng thứ tự này:

| Thứ tự | File | Mục đích |
| ---: | --- | --- |
| 1 | `09-README-PHASE-03-HANDOFF-INDEX.md` | Hiểu phạm vi, thứ tự và gateway lock Phase 3 |
| 2 | `10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md` | Khóa Customer-to-Cash, DailySalesContext, QuoteSnapshot, state bridge, verified revenue |
| 3 | `11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md` | Cổng chuẩn duyệt, SRS handoff và checklist không conflict |
| 4 | `00-P3-ANALYSIS-ONLY.md` | Phân tích hiện trạng codebase thật |
| 5 | `01-P3-1-TECHNICAL-DESIGN-ONLY.md` | Thiết kế kỹ thuật Phase 3 gốc |
| 6 | `02-P3-2A-SELLABLE-GATE-RUNTIME.md` | Sellable Gate Runtime |
| 7 | `03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md` | Pricing/program/member/QuoteSnapshot |
| 8 | `04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md` | Cart, Order Draft, Customer Confirmation |
| 9 | `05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md` | Official Order, order_code, state machine |
| 10 | `06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md` | Payment, shipping, invoice, tax |
| 11 | `07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md` | Verified revenue, commission, ROAS handoff |
| 12 | `08-P3-2G-SMOKE-EVIDENCE-REPORT.md` | Smoke, evidence, report |
| 13 | `../phase-3.1/TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md` | Bổ sung Diamond/member/CRM/AI nếu nằm trong scope triển khai |
| 14 | `../phase-3.1/09-HƯỚNG-DẪN-BÀN-GIAO-GIAI-ĐOẠN-3-1.md` | Handoff bổ sung Phase 3.1 |

Không được bắt đầu từ file 02-08 khi chưa đọc README, addendum, file chuẩn duyệt, analysis và design.

## 5. Source-of-truth không được phá

| Quyết định nghiệp vụ/kỹ thuật | Owner truth | Consumer được phép |
| --- | --- | --- |
| Product public name, SKU internal, claim, dietary | Product/Product Knowledge Core | Commerce, AI, CRM, Gateway |
| Sellable status | Sellable Gate Runtime + Operational/Inventory/Quality/Recall/Sale Lock | AI, CRM, Gateway, Live |
| Listed price | Pricing Runtime | QuoteSnapshot, AI, CRM, Gateway |
| Program discount | ProgramEligibilityResolver | QuoteSnapshot |
| Member benefit | MemberBenefitResolver + Member Core | QuoteSnapshot, CRM |
| Diamond buyer benefit | DiamondReferralResolver + policy version | QuoteSnapshot |
| Final total | QuoteSnapshot | Order Draft, customer renderer, AI/Gateway as consumer |
| Cart | Commerce Runtime | Order Draft |
| Order Draft | Commerce Runtime | Customer Confirmation |
| Official Order/order_code | Order Core | Payment, shipping, AI/Gateway as status consumer |
| Payment paid status | Payment Core | Order, verified revenue |
| Shipping/tracking/ETA | Shipping/Delivery Runtime | AI, CRM, Gateway |
| Verified revenue | Commerce + payment/order checkpoint | Ads, Diamond, Finance |
| Commission eligibility | Diamond/Finance handoff after verified revenue | Finance |
| Gateway opening | Release/Gateway owner decision | Không thuộc Phase 3 implementation |

Không consumer nào được tự tính thay owner truth.

## 6. P0 fail gate toàn Phase 3

Phase 3 bị FAIL nếu xảy ra một trong các lỗi sau:

- Product Active, SKU Active hoặc Recipe Active bị dùng thay Sellable.
- Warehouse Receipt tự set Sellable.
- Recall, sale lock, quality hold hoặc channel suppression không chặn bán.
- AI/Gateway/CRM/Live tự tính giá, discount, member benefit, Diamond benefit, shipping, VAT hoặc COD fee.
- Final price trả ra khi không có QuoteSnapshot.
- QuoteSnapshot mutable hoặc không có expiry/freeze window.
- Cart bị xem là Order.
- Order Draft bị xem là Official Order.
- Customer Confirmation bị bỏ qua.
- Official Order không có order_code.
- Retry/concurrency tạo duplicate order_code.
- Payment selected bị set Paid.
- COD selected bị set Paid.
- Bank transfer selected bị set Paid.
- Paid, Delivered hoặc Invoice tự tạo Verified Revenue.
- Verified Revenue sinh khi chưa có `ORDER_VERIFIED` hoặc checkpoint tương đương đã duyệt.
- Commission final trước Verified Revenue.
- ROAS dùng unverified value.
- Payout executed trong Phase 3 khi chưa có Finance/Reward approval riêng.
- Evidence Submitted bị gọi là Evidence Accepted.
- Local smoke bị gọi là Global Smoke Pass.
- Gateway bị mở.
- Release Ready/Production Ready/Go-live bị gọi trước owner/release decision.

## 7. Điều kiện để ChatGPT được phép duyệt tài liệu

ChatGPT chỉ được đánh giá `APPROVE_FOR_SRS_HANDOFF` nếu tất cả điều kiện sau đạt:

| Gate | Điều kiện pass |
| --- | --- |
| Read order | Có đủ thứ tự đọc từ README, addendum, file chuẩn duyệt, analysis, design, implementation prompts, smoke report |
| Source boundary | Owner truth từng domain rõ |
| Phase 3 vs Phase 3.1 | Không nhầm `P3.1 Technical Design` với phase bổ sung `phase-3.1` |
| Scope | Không claim Phase 3 thay Phase 1/2/4/5/6/7/8/9 |
| SRS handoff | Có cấu trúc SRS bắt buộc trước khi code |
| P0 blocker | P0 fail gate đã được liệt kê |
| Evidence | Chỉ gọi submitted/required, không gọi accepted nếu chưa review |
| Gateway | Vẫn blocked |
| Release | Không gọi release/production ready |
| Dev protocol | Dev phải inspect codebase thật và tạo SRS trước implementation |

Nếu thiếu bất kỳ gate nào, ChatGPT phải trả `NEEDS_REVISION`, không được approve.

## 8. Prompt đưa ChatGPT review

Khi đưa bộ Phase 3 lên ChatGPT, dùng prompt sau:

```text
Bạn là reviewer kỹ thuật cấp SRS/implementation-readiness.

Hãy review bộ tài liệu Phase 3 Commerce Runtime theo đúng thứ tự:
1. 09-README-PHASE-03-HANDOFF-INDEX.md
2. 10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md
3. 11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md
4. 00-P3-ANALYSIS-ONLY.md
5. 01-P3-1-TECHNICAL-DESIGN-ONLY.md
6. 02 đến 08 của Phase 3
7. Nếu scope liên quan Diamond/member/CRM/AI, đọc thêm phase-3.1 file tổng và hướng dẫn bàn giao.

Mục tiêu review:
- Có đủ để dev phân tích sang SRS không?
- Có conflict source-of-truth không?
- Có nhầm Phase 3 với Phase 3.1 không?
- Có claim quá mức release/production/gateway không?
- Có thiếu P0 fail gate, evidence gate, owner sign-off hoặc SRS output không?
- Có rule nào mơ hồ khiến dev code sai tiền, sai đơn, sai payment, sai revenue, sai commission không?

Chỉ được trả một trong hai trạng thái:
- APPROVE_FOR_SRS_HANDOFF
- NEEDS_REVISION

Không được trả Release Ready, Production Ready hoặc Gateway Open.

Nếu APPROVE_FOR_SRS_HANDOFF, hãy liệt kê các điều kiện bắt buộc dev phải giữ khi viết SRS.
Nếu NEEDS_REVISION, hãy chỉ rõ file, section, lý do và cách sửa.
```

## 9. SRS bắt buộc dev phải tạo

Trước khi implementation, dev phải tạo SRS từ bộ Phase 3. SRS không được chỉ là bản tóm tắt. SRS phải đủ để triển khai, review, test và audit.

SRS tối thiểu gồm 16 phần:

1. Mục tiêu và phạm vi Phase 3.
2. Out of scope và release/gateway lock.
3. Glossary và naming convention.
4. Source-of-truth matrix.
5. Actor/role/channel matrix.
6. Domain model.
7. State machine.
8. Business rules theo workstream.
9. API/DTO contract.
10. Database/entity/migration proposal.
11. Resolver/service/guard design.
12. Event/outbox/handoff contract.
13. RBAC/audit/security/privacy requirements.
14. Validation/test/smoke matrix.
15. Evidence package.
16. Open decisions, blockers, risk register và sign-off.

Nếu SRS thiếu một trong 16 phần, không được bắt đầu code.

## 10. SRS mapping theo workstream

| Workstream | File nguồn | SRS section bắt buộc |
| --- | --- | --- |
| P3 Analysis | `00-P3-ANALYSIS-ONLY.md` | Current-state, gap, conflict, blocker, evidence required |
| P3 Technical Design | `01-P3-1-TECHNICAL-DESIGN-ONLY.md` | Architecture, owner boundary, API/DTO/entity/service/test plan |
| P3.2A Sellable | `02-P3-2A-SELLABLE-GATE-RUNTIME.md` | Sellable decision, guard chain, block reason, fail-safe |
| P3.2B Quote | `03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md` | Pricing, program, member, Diamond, QuoteSnapshot, expiry |
| P3.2C Draft | `04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md` | Cart, draft, customer confirmation, idempotency |
| P3.2D Order | `05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md` | Official order, order_code, state machine, duplicate guard |
| P3.2E Payment/Shipping | `06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md` | Payment selection vs confirmation, shipping, VAT/tax |
| P3.2F Revenue | `07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md` | ORDER_VERIFIED, verified revenue, commission/ROAS handoff |
| P3.2G Evidence | `08-P3-2G-SMOKE-EVIDENCE-REPORT.md` | Smoke matrix, evidence package, final handoff report |
| Addendum | `10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md` | DailySalesContext, state bridge, public-safe response, P0 addendum smoke |
| Phase 3.1 Supplement | `../phase-3.1/TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md` | Member/Diamond/CRM/AI supplemental policy, only if in implementation scope |

## 11. SRS domain model bắt buộc

SRS phải có domain model tối thiểu:

| Domain object | Purpose | Không được |
| --- | --- | --- |
| SellableDecision | Quyết định bán được hay không | Không thay bằng Product Active |
| ProgramEligibilityDecision | Quyết định chương trình áp dụng | Không hardcode trong AI/Gateway |
| MemberBenefitDecision | Quyết định quyền lợi member | Không cộng dồn nếu policy priority chưa duyệt |
| DiamondReferralSnapshot | Snapshot referral/buyer benefit | Không tạo commission final |
| QuoteSnapshot | Nguồn final price immutable | Không mutable, không thiếu expiry |
| Cart | Giỏ hàng | Không xem là Order |
| OrderDraft | Đơn nháp | Không có order_code |
| CustomerConfirmation | Xác nhận khách hàng | Không bỏ qua khi tạo official order |
| OfficialOrder | Đơn chính thức | Không paid/revenue ngay khi tạo |
| OrderStateTransition | Lịch sử state append-only | Không overwrite mất audit |
| PaymentSelection | Phương thức thanh toán đã chọn | Không set Paid |
| PaymentConfirmation | Payment Core xác nhận | Không tự tạo verified revenue |
| ShippingQuote/ShippingState | Phí/trạng thái giao hàng | Không bịa ETA/tracking |
| TaxSnapshot | VAT/tax policy | Không hardcode nếu policy chưa duyệt |
| VerifiedRevenueRecord | Doanh thu xác minh | Không tạo khi chưa ORDER_VERIFIED |
| CommissionEligibilitySnapshot | Điều kiện hoa hồng | Không payout |
| ROASHandoffRecord | Handoff Ads/ROAS | Không dùng unverified value |

## 12. API/DTO contract bắt buộc trong SRS

SRS phải mô tả API/DTO ở mức hợp đồng, không cần code, nhưng phải đủ:

- Endpoint hoặc command/query name.
- Actor/channel được gọi.
- Request fields.
- Response fields public-safe/internal.
- Validation rules.
- Error/fail-safe codes.
- Idempotency key nếu có write action.
- Audit/log requirement.
- Policy version refs.
- Security/RBAC requirement.
- Test/smoke mapping.

Không API nào được trả final price nếu thiếu QuoteSnapshot.

Không API nào được trả paid nếu Payment Core chưa confirmed.

Không API nào được trả verified revenue nếu chưa có `ORDER_VERIFIED`.

## 13. Database/migration proposal bắt buộc trong SRS

SRS phải mô tả DB/migration proposal theo nguyên tắc:

- Additive-first nếu có thể.
- Không phá schema hiện hữu nếu chưa có migration plan.
- Có unique constraint cho order_code và idempotency boundary.
- Có immutable snapshot hoặc append-only event khi rule yêu cầu.
- Có audit fields.
- Có policy_version_refs.
- Có reversal/void/cancel strategy nếu domain cần.
- Có seed/validation nếu policy/config cần.

Không được tạo migration theo suy đoán nếu chưa inspect database thật.

## 14. State machine bắt buộc

SRS phải khóa state bridge tối thiểu:

```text
CUSTOMER_INTERESTED
-> QUOTE_SNAPSHOT_CREATED
-> ORDER_DRAFT_RENDERED
-> CUSTOMER_CONFIRMED
-> ORDER_CODE_CREATED
-> PAYMENT_PENDING
-> PAYMENT_CONFIRMED
-> READY_FOR_FULFILLMENT
-> WAREHOUSE_RELEASED
-> HANDED_TO_CARRIER
-> DELIVERED
-> ORDER_VERIFIED
```

State transition phải có:

- Owner.
- Allowed previous states.
- Allowed next states.
- Validation.
- Audit/event.
- Public wording allowed.
- Downstream consumers.
- P0 fail case.

## 15. Evidence package bắt buộc

Evidence package tối thiểu:

| Nhóm | Evidence |
| --- | --- |
| Analysis | Current-state/gap/conflict/blocker report |
| Design | Architecture/API/DB/service/test design |
| Sellable | Sellable PASS/FAIL/block reason trace |
| Quote | QuoteSnapshot immutable/expiry/final total trace |
| Draft | Cart/Draft/Confirmation idempotency trace |
| Order | order_code unique/stable/concurrency trace |
| Payment | Payment selected vs paid separation trace |
| Shipping | Shipping fee/ETA/tracking fallback trace |
| Tax | VAT/tax snapshot policy trace |
| Revenue | ORDER_VERIFIED -> VerifiedRevenueRecord trace |
| Commission | Eligibility snapshot after verified revenue |
| ROAS | Verified revenue handoff only |
| RBAC | Permission matrix proof |
| Audit | Audit/event log proof |
| Runtime unavailable | Fail-closed proof |
| Public/private | No PII/no internal SKU/no payment leak proof |
| Gateway | Gateway remains blocked proof |

Evidence package chỉ được gọi `SUBMITTED` cho đến khi owner/reviewer accepted.

## 16. Owner decision register

| ID | Decision cần chốt | Owner gợi ý | Trạng thái mặc định |
| --- | --- | --- | --- |
| P3-OD-001 | P2 exit gate có cho bắt đầu Phase 3 không | Owner/dev lead | REQUIRED |
| P3-OD-002 | Boundary implementation sau P3 technical design | Dev lead/architect | REQUIRED |
| P3-OD-003 | Policy priority giữa program/member/Diamond | Commerce/CRM owner | REQUIRED nếu có nhiều benefit |
| P3-OD-004 | QuoteSnapshot expiry/freeze window | Commerce owner | REQUIRED |
| P3-OD-005 | Official order state names và public wording | Commerce/Support owner | REQUIRED |
| P3-OD-006 | Payment Core confirmation source | Finance/Payment owner | REQUIRED |
| P3-OD-007 | Shipping fee/ETA fallback policy | Shipping owner | REQUIRED |
| P3-OD-008 | ORDER_VERIFIED checkpoint | Commerce/Finance owner | REQUIRED |
| P3-OD-009 | Commission/ROAS handoff boundary | Finance/Ads owner | REQUIRED |
| P3-OD-010 | Gateway/release decision path | Release owner | REQUIRED sau evidence |

Nếu owner decision còn `REQUIRED`, SRS được phép ghi blocker nhưng không được code phần phụ thuộc decision đó.

## 17. Dev implementation protocol

Dev phải làm theo thứ tự:

1. Đọc toàn bộ file theo mục 4.
2. Tạo SRS theo mục 9.
3. Gắn từng requirement với file nguồn.
4. Gắn từng API/entity/service/test với requirement.
5. Rà P0 fail gate.
6. Rà owner decision register.
7. Xin review SRS.
8. Chỉ khi SRS được duyệt mới bắt đầu implementation từng workstream.
9. Mỗi workstream phải có report 14 mục theo README.
10. Sau P3.2G mới gom evidence package.

Không được nhảy thẳng từ tài liệu sang code.

## 18. Checklist trước khi giao dev

| Checklist | Pass/Fail |
| --- | --- |
| Dev đã biết Phase 3 là Commerce Runtime source-of-truth |  |
| Dev đã biết Phase 3.1 là supplement, không thay Phase 3 |  |
| Dev đã biết `P3.1 Technical Design` không phải folder `phase-3.1` |  |
| Dev đã có thứ tự đọc |  |
| Dev đã có SRS template 16 phần |  |
| Dev đã có P0 fail gate |  |
| Dev đã có owner decision register |  |
| Dev đã biết Gateway blocked |  |
| Dev đã biết không gọi Release/Production Ready |  |
| Dev đã biết evidence submitted khác evidence accepted |  |

Không đủ pass thì chưa giao implementation.

## 19. Checklist trước khi sếp duyệt

| Câu hỏi duyệt | Expected |
| --- | --- |
| Phase 3 có đủ để dev lập SRS không? | Có, nếu đọc đúng thứ tự |
| Phase 3 có tự gọi Production Ready không? | Không |
| Phase 3 có mở Gateway không? | Không |
| Phase 3 có khóa owner truth không? | Có |
| Phase 3 có chặn sai tiền/sai đơn/sai payment/sai revenue không? | Có P0 fail gate |
| Phase 3 có xử lý Phase 3.1 bổ sung không? | Có, với vai trò supplement |
| Phase 3 có cho code ngay không? | Không, phải có SRS + owner sign-off |
| Phase 3 có đủ cho ChatGPT review không? | Có, với prompt mục 8 |

## 20. Kết luận chuẩn duyệt

Phase 3 được phép đưa ChatGPT/sếp/dev review với mục tiêu:

```text
APPROVE_FOR_SRS_HANDOFF
```

Phase 3 chưa được phép gọi:

```text
FORBIDDEN_IMPLEMENTATION_COMPLETE
FORBIDDEN_EVIDENCE_ACCEPTED
FORBIDDEN_GATEWAY_OPEN
FORBIDDEN_RELEASE_READY
FORBIDDEN_PRODUCTION_READY
FORBIDDEN_GO_LIVE_APPROVED
```

Done gate của file này:

- Thứ tự đọc rõ.
- Phase 3 vs Phase 3.1 rõ.
- SRS output rõ.
- P0 fail gate rõ.
- Owner decision rõ.
- Evidence/release/gateway lock rõ.

Trạng thái cuối:

```text
GÓI CHUẨN DUYỆT GIAI ĐOẠN 3 COMPLETE
READY FOR CHATGPT REVIEW AS SRS-HANDOFF SOURCE
NOT READY FOR IMPLEMENTATION WITHOUT SRS SIGN-OFF
GATEWAY BLOCKED
```
