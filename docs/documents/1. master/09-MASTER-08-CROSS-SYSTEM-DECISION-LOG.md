# MASTER-08 - CROSS-SYSTEM DECISION LOG

## Mục Đích

MASTER-08 khóa policy/decision register bắt buộc sau vòng bổ sung tài liệu Facebook Ads, Live Commerce, AI Advisor Facebook Completion, SKU/BOM/G1, phiếu vận hành, thành viên, Diamond, Finance và IVR.

Tài liệu này ghi rõ policy đã được chọn trong source canonical, phạm vi evidence còn thiếu và các chặn production còn hiệu lực. Các policy đã chọn không đồng nghĩa Gateway PASS, Release Ready hoặc Production Ready.

## Trạng Thái

| Nhóm | Trạng thái |
| --- | --- |
| Document status | DOCUMENTATION_ACTIVE |
| Direct implementation status | LIMITED_HANDOFF_ALLOWED_WITH_EVIDENCE_GATES |
| Gateway status | BLOCKED |
| Completion report status | PENDING_EVIDENCE |
| Production claim | NOT_ALLOWED |

## SYSTEM-WIDE GAP / IMPACT MATRIX

Ma trận này là bước đọc đầu tiên trước MASTER-09 và các addendum Phase 3/4/5. Mục tiêu không phải viết lại mọi file gốc cùng lúc, mà khóa rõ điểm thiếu, downstream bị ảnh hưởng và artifact canonical cần đọc trước khi dev triển khai.

| Gap ID | Gap / lệch hiện tại | Impact trực tiếp | Artifact khóa lại | Runtime stance |
| --- | --- | --- | --- | --- |
| SYS-GAP-001 | Source bổ sung phân tán giữa Phase 4, Phase 5 và `4. phase/phase-3.1/5. bo sung`, chưa có một ma trận impact toàn hệ. | Dev dễ triển khai từng mảng rời rạc, sai thứ tự source-of-truth. | MASTER-08 + MASTER-09. | Chỉ coi source bổ sung là baseline; implementation đi qua addendum canonical. |
| SYS-GAP-002 | Daily sales context chưa đủ contract: hôm nay bán gì, giá nào, tồn public nào, chương trình nào. | AI, CRM, Gateway, Live có nguy cơ bịa giá/tồn/chương trình. | P3 Addendum + P4 Practical Runtime Lock. | Không final price nếu thiếu QuoteSnapshot; không nói còn hàng nếu thiếu Sellable. |
| SYS-GAP-003 | CustomerAdvisoryContext chưa thành payload tối thiểu. | AI/CRM hỏi vòng, lộ lịch sử mua, hoặc giả lập member benefit. | P4 Addendum + CRM/Member Canonical. | Thiếu context thì fail-safe; public channel không lộ dữ liệu cá nhân. |
| SYS-GAP-004 | ProductRoleMatrix 20 sản phẩm chưa canonical. | Gợi ý 3 trụ, chay/mặn, trẻ em, người già, biếu tặng, du học sinh có thể bị model đoán. | P4 Addendum; Phase 1/Product source cần owner-approved matrix. | Không claim phù hợp nếu Product Knowledge chưa có role public-safe. |
| SYS-GAP-005 | Quote -> Order -> Payment -> Fulfillment -> Delivery chưa có state bridge xuyên Phase 3/4/5/CRM. | AI/Gateway/CRM có thể nói "đã đặt", "đã thanh toán", "đã xuất kho" sai trạng thái. | P3 Addendum + P4 Addendum + P5 Addendum. | Chỉ nói theo state do Commerce/Payment/Fulfillment/Delivery runtime trả về. |
| SYS-GAP-006 | CRM message files 3 và 6 có nội dung trùng/lệch, còn text customer-facing dùng từ nội bộ. | Render tin nhắn chăm sóc sai tone, lộ "hệ thống", lộ biến/khái niệm nội bộ. | CRM/Member Canonical. | Chưa qua Message Registry + Language Guard thì không auto-send. |
| SYS-GAP-007 | Member lifecycle, Diamond commission và Finance payout đã được tách khỏi buyer benefit trong canonical finance/CRM. | Sai doanh số duy trì hạng, sai hoa hồng, sai payable/accounting nếu runtime bỏ qua canonical. | Finance/Diamond Canonical + CRM/Member Canonical. | Commission chỉ là candidate đến khi verified revenue + selected policy ref + finance checkpoint pass. |
| SYS-GAP-008 | Phase 3.1 Diamond file bổ sung chỉ là workstream source; Finance/Diamond Canonical vẫn là source kiểm payout/MISA. | Dev không được lấy workstream Diamond làm quyền chi trả/payable cuối cùng. | Finance/Diamond Canonical. | Không dùng source phụ làm sole implementation spec nếu thiếu Finance/MISA evidence. |
| SYS-GAP-009 | Pack 09 IVR đã được cleanup khỏi legacy attempt/window fragment. | Scheduler/test phải dùng policy thống nhất, không quay lại legacy fixture. | IVR-21 Attempt Policy Cleanup Decision. | Canonical active: Golden Hour 10 phút/2 attempts; 24/7 15 phút/3 attempts; real call vẫn chặn đến khi evidence accepted. |
| SYS-GAP-010 | Evidence/smoke chưa có gate chung cho Customer-to-Cash-to-Care. | Từng phase có thể PASS cục bộ nhưng toàn hệ vẫn hở Gateway/CRM/Finance. | Evidence/Smoke Canonical. | Không gọi Global Smoke Pass, Completion PASS, Release Ready hoặc Production Ready nếu thiếu evidence accepted. |

## Phase Impact Matrix

| Phase / domain | Mức tác động | Việc cần làm |
| --- | --- | --- |
| Phase 1-2 Product / Ops | Indirect upstream lock | Không rewrite toàn bộ trong lane này; cần ProductRoleMatrix, formula/dietary approval và Sellable proof làm source cho Phase 3/4. |
| Phase 3 Commerce | Direct rewrite addendum | Khóa Customer-to-Cash: DailySalesContext, QuoteSnapshot, Order state, payment, shipping, verified revenue, commission handoff. |
| Phase 4 AI Advisor | Direct rewrite addendum | Giữ P4 practical runtime lock; AI chỉ consume Product/Ops/Commerce/CRM, không tự tính. |
| Phase 5 Gateway | Direct supplement addendum | Gateway là delivery/channel runtime; không tự quote/order/payment/CRM; khóa public/private, typing, dedup, moderation. |
| CRM / Member | New canonical | Chọn nguồn canonical cho lifecycle, message registry, quiet hours, frequency cap, suppression và language guard. |
| Finance / Diamond | New canonical | Tách buyer benefit, referral eligibility, commission candidate, payout checkpoint, MISA boundary. |
| Phase 8 IVR | Cleanup applied, evidence pending | Pack 09/Pack 07 đã đồng bộ attempt policy; giữ Phase 8 attempt policy làm canonical active cho implementation evidence. |
| Evidence / Smoke | New canonical | Gate toàn hệ, nối Phase 3/4/5/CRM/Finance/IVR trước khi Completion/Release. |

## Nguyên Tắc Áp Dụng

1. P0 policy đã chọn thì downstream phải dùng đúng policy ref trong register này, không được quay lại source baseline cũ.
2. Nếu runtime/evidence chưa có policy ref, AI/Gateway/CRM/Ads/Live/IVR không được hardcode runtime rule.
3. Quote, order, member benefit, Diamond commission, Ads/ROAS và MISA handoff chỉ được dùng policy version đã trace được vào QuoteSnapshot/evidence.
4. Completion Report hoặc tài liệu gate chỉ được PASS khi có evidence thật, không PASS bằng câu kết luận tài liệu.
5. Gateway production chỉ được mở khi tất cả P0 gate trong AI Advisor Facebook Completion Report PASS và owner sign-off PASS.

## Source Evidence Đã Rà

| Nhóm source | File nguồn |
| --- | --- |
| Financial baseline | `4. phase/phase-5/TÀI LIỆU BỔ SUNG/00. GINSENGFOOD — BUSINESS & ADS FINANCIAL BASELINE LOCK.md` |
| Pricing / Golden Hour / Diamond model mới | `4. phase/phase-5/TÀI LIỆU BỔ SUNG/GINSENGFOOD - MO HINH GIA - GIO VANG TRI AN - DOANH THU LIVE - ADS - DIAMOND.md` |
| Gateway completion gate | `4. phase/phase-5/TÀI LIỆU BỔ SUNG/AI_ADVISOR_FACEBOOK_COMPLETION_REPORT.md` |
| Facebook page list | `4. phase/phase-5/TÀI LIỆU BỔ SUNG/DANH-SACH-FACEBOOK-AI.md` |
| Facebook implementation sequence | `4. phase/phase-5/TÀI LIỆU BỔ SUNG/TRÌNH TỰ TRIỂN KHAI TIẾP THEO.md` |
| Member / 24/7 / Golden Hour policy cũ | `4. phase/phase-3.1/5. bo sung/4. chính sách và hoa hồng thành viên.md` |
| Product / formula / vegan source | `4. phase/phase-1/BỘ KHÓA 5 BƯỚC.md` và `4. phase/phase-1/CÔNG THỨC VẬN HÀNH 20 SKU MỚI.MD` |
| Product master formula governance | `2. pack/02-PACK-02-PRODUCT-MASTER-SKU-RECIPE-ACTIVATION.md` |
| IVR source | `2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` và `4. phase/phase-8/*` |

## P0 Policy Decision Register

| Decision ID | Domain | Policy đã chọn | Runtime stance hiện hành | Downstream / evidence bị chặn |
| --- | --- | --- | --- | --- |
| PRICE-POLICY-DECISION-001 | 24/7 pricing | `PRICE_POLICY_V2026_06_CANONICAL_001`: mở bán lần đầu 9%; dưới 50 hộp/ngày 9%; từ 50 hộp/ngày trở lên 5%. | ProgramPriceResolver phải dùng policy ref này; nếu runtime thiếu `price_policy_version` thì trả `POLICY_PENDING`, không tự fallback legacy pricing branch. | Commerce quote, AI báo giá, CRM reorder, Gateway quote, Ads financial model, Diamond buyer benefit cần evidence policy ref. |
| GOLDEN-HOUR-TIME-DECISION-001 | Golden Hour schedule | `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`: phiên 12h00-12h45 và 20h00-20h45; early access Diamond 30 phút, Platinum 20 phút, Gold 10 phút, Silver 5 phút, buyer link Diamond 5 phút. | GoldenHourResolver phải dùng policy ref này; không schedule theo legacy schedule branch. | Ads schedule, Live board, CRM reminder, member early access, Gateway context, IVR timing assumptions, QuoteSnapshot expiry cần evidence. |
| VEGAN-FORMULA-DECISION-001 | Vegan / dietary claim | `DIETARY_POLICY_V2026_06_CANONICAL_001`: A1/B1/C2/C3 không được public claim vegan khi formula/BOM có thành phần động vật; chỉ mở lại với plant-only formula + evidence accepted. | SKU có ingredient/formula/broth animal component phải bị `DIETARY_CLAIM_BLOCKED`. | Product Master, AI recommendation, claim guard, label/packaging, Live script, Ads claim, public wording cần proof. |
| FORMULA-VERSION-DECISION-001 | G0/G1 active formula | `FORMULA_POLICY_V2026_06_CANONICAL_001`: G0 là history/research/pilot baseline; G1 là operational/pilot candidate; production-active chỉ khi có owner approval, effective date và evidence accepted. | Không tạo production order từ G0; không coi G1 là production active nếu thiếu approval/evidence. | Phase 1 Product Master, Phase 2 Production Dossier, BOM snapshot, Material Issue, MRP, costing, MISA boundary. |
| FB-PAGE-REGISTRY-CLEAN-001 | Facebook Page Registry | `FB_PAGE_REGISTRY_LOCK_V2026_06_CANONICAL_001`: raw Page/Profile/Business/App list là reference-only; chỉ normalized Page Registry mới là source active; thiếu App ID/mapping thì `production_allowed=false`. | Không dùng raw table để bật webhook/token/outbound/CRM/Gateway production. | Meta App setup, webhook subscription, Messenger handoff, source attribution, CRM suppression, Ads/Live attribution cần registry evidence. |
| GATEWAY-PRODUCTION-GATE-001 | Facebook Gateway | `GATEWAY_PRODUCTION_GATE_V2026_06_CANONICAL_001`: dev task breakdown/handoff được phép khi policy refs sạch; production vẫn blocked đến khi Completion Report PASS bằng evidence và release sign-off. | Không bật quote/order/live auto-reply production; không gọi Gateway PASS từ wording tài liệu. | Facebook Gateway, Messenger quote/order, Live auto-reply, Ads scale, owner release cần evidence accepted. |

## P1 / Cross-Phase Decision Register

| Decision ID | Domain | Policy đã chọn | Runtime stance hiện hành |
| --- | --- | --- | --- |
| IVR-ATTEMPT-POLICY-DECISION-001 | IVR | `IVR_ATTEMPT_POLICY_V2026_06_CANONICAL_001`: Golden Hour T0/T0+5, expiry T0+10; 24/7 T0/T0+5/T0+10, expiry T0+15. | Real customer call vẫn chặn nếu thiếu smoke/evidence/security/release sign-off. |
| MEMBER-BENEFIT-GOLDEN-HOUR-DECISION-001 | Member benefit | `GOLDEN_HOUR_MEMBER_BENEFIT_POLICY_V2026_06_CANONICAL_001`: Giờ Vàng chỉ có early access; không cộng member discount/Diamond buyer discount trừ khi QuoteSnapshot có policy exception rõ. | QuoteSnapshot phải ghi applied/denied benefit và reason code. |
| DIAMOND-SELF-PURCHASE-DECISION-001 | Diamond | `DIAMOND_SELF_PURCHASE_POLICY_V2026_06_CANONICAL_001`: self-purchase không eligible commission, không payable. | CommissionDecision phải trả ineligible/rejected cho self-purchase. |
| MISA-PAYOUT-DECISION-001 | Finance / MISA | `MISA_PAYOUT_POLICY_V2026_06_CANONICAL_001`: PIT 10%; payout/MISA handoff chỉ sau Finance payable approval + MISA mapping active + evidence accepted. | Không sync payout/commission official nếu thiếu Finance approval, mapping active hoặc accounting evidence. |

## Decision Packet Chuẩn

Mỗi owner decision phải được ghi lại tối thiểu:

| Field | Bắt buộc | Ghi chú |
| --- | --- | --- |
| decision_id | Yes | Dùng ID trong register này. |
| selected_option | Yes | Không dùng mô tả mơ hồ. |
| policy_version | Yes | Ví dụ `PRICE_POLICY_V2026_06_OWNER_001`. |
| effective_from | Yes | Thời điểm áp dụng cho runtime. |
| owner | Yes | Người hoặc nhóm có quyền chốt. |
| sign_off_evidence | Yes | Link/ảnh/chữ ký/evidence packet. |
| affected_docs | Yes | Danh sách file phải cập nhật. |
| affected_runtime | Yes | Resolver/guard/smoke/evidence liên quan. |
| migration_note | Conditional | Bắt buộc nếu thay đổi rule đang tồn tại. |

## Fail-Closed Runtime Rules

| Nếu thiếu | Runtime phải làm |
| --- | --- |
| Price policy version | Không trả final price; không tạo QuoteSnapshot final. |
| Golden Hour policy version | Không nói Giờ Vàng đang active; không gửi reminder/early access theo giờ cứng. |
| Vegan formula proof | Không claim vegan; không gợi ý cho khách ăn chay. |
| Active formula approval | Không tạo production order/material issue/BOM snapshot production. |
| Page registry normalized | Không bật webhook, token, outbound, CRM hoặc production_allowed. |
| Completion Report PASS | Không mở Gateway production. |
| Release sign-off | Không gọi Production Ready/Go-live. |

## Work Queue Sau Decision Log

| Thứ tự | Artifact cần viết / sửa | Điều kiện bắt đầu |
| --- | --- | --- |
| 1 | MASTER-09 Cross-Phase Runtime Lock Addendum | Có SYSTEM-WIDE GAP / IMPACT MATRIX để làm blocker map. |
| 2 | Phase 3 Commerce / Customer-to-Cash Addendum | Dùng `PRICE_POLICY_V2026_06_CANONICAL_001` và `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`; cần runtime/evidence trace. |
| 3 | Phase 4 AI Advisor Practical Runtime Addendum | Dùng `DIETARY_POLICY_V2026_06_CANONICAL_001` và fail-closed dietary guard; consume Phase 3, không tự tính. |
| 4 | Phase 5 Gateway / Messaging Delivery Addendum | Dùng `FB_PAGE_REGISTRY_LOCK_V2026_06_CANONICAL_001` và Gateway P0 evidence plan. |
| 5 | CRM / Member Lifecycle Canonical | Cần thống nhất message registry, lifecycle rule, quiet hours, suppression và language guard. |
| 6 | Finance / Diamond Canonical Registry | Dùng Diamond self-purchase và MISA payout policy refs; cần verified revenue + Finance/MISA evidence. |
| 7 | IVR Attempt Policy Cleanup | Pack 09 cleanup đã áp dụng; không mở real call nếu evidence/release gate chưa pass. |
| 8 | Evidence / Smoke Matrix toàn hệ | Cần tất cả P0 decision IDs map vào smoke/evidence. |

## Done Gate Của MASTER-08

MASTER-08 đạt yêu cầu khi:

1. Mọi conflict P0 có selected policy ref hoặc fail-closed gate rõ.
2. Mọi downstream/evidence còn bị chặn được chỉ rõ.
3. Runtime stance trước release sign-off là fail-closed.
4. Không có nội dung nào tự tuyên bố Gateway PASS hoặc Production Ready.
5. Có policy/evidence packet chuẩn để trace vào implementation.

## Final Lock

Tại thời điểm viết MASTER-08:

| Scope | Kết luận |
| --- | --- |
| Business / technical foundation | CONFLICTS_RESOLVED_FOR_DOCUMENTATION |
| Direct full implementation | LIMITED_HANDOFF_ALLOWED_WITH_EVIDENCE_GATES |
| Gateway production | BLOCKED |
| Next required work | Runtime evidence + Completion Report review + release sign-off |
