**AI_ADVISOR_CHANNEL - MASTER RULE LOCK REPORT V2.1**

**BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE**

**Dự án:** Ginsengfood - AI TÍCH HỢP CHANNEL / AI_ADVISOR_CHANNEL  
**Mục đích:** Làm nền để viết lại từng FILE kỹ thuật sau khi test xong  
**Trạng thái:** FILE 04 = NOT_STARTED | Gateway = BLOCKED | Production Readiness = BLOCKED  
**Nguyên tắc:** Không vá miệng, không gọi production-ready, không mở Gateway khi chưa có evidence thật  
**Ngày lập:** 15/05/2026  
**Phiên bản:** V2.1 - CLEAN MASTER RULE LOCK

**0\. MỤC ĐÍCH VÀ PHẠM VI BẢN KHÓA**

Bản V2.1 tổng hợp toàn bộ ghi nhận sau test thực chiến AI Advisor Channel, kết hợp:

- AI-TEST-NOTE-001 đến AI-TEST-NOTE-063.
- Chính sách thành viên, giá, chương trình 24/7, Giờ Vàng Tri Ân, quyền mua sớm, Diamond, hoa hồng.
- Bổ sung tin nhắn chăm sóc khách hàng.
- CRM 12-MONTH AUTOMATION CONTENT BLOCK REGISTRY.
- MEMBER LIFECYCLE AUTOMATION 12-MONTH / UPGRADE / MAINTAIN / GRACE / DOWNGRADE LOCK.
- Các khóa bổ sung mới: Product Recommendation 3 trụ + chăm sóc người thân, proactive member amount care, priority conflict, cross-channel dedup, member dispute escalation, member lifecycle outcome logger.

Bản này dùng làm tài liệu nền để viết lại:

- FILE 01 - Parent Logic
- FILE 04 - Resolver / Guard / Runtime Contract
- FILE 07 - Common Content Blocks
- FILE 10 - Situation Registry
- FILE 09 - Test Matrix / Done Gate
- FILE 12 - Human-Level Sales / CRM / Affiliate Behavior
- Rà lại FILE 02/03/05/06/08/11/Appendix

Không phải bản triển khai Gateway. Gateway, Meta Gateway, MC AI và Production Readiness vẫn BLOCKED cho đến khi AI Advisor Channel pass thực chiến có evidence thật.

| **Hạng mục**                 | **Trạng thái khóa**                                              |
| ---------------------------- | ---------------------------------------------------------------- |
| AI Advisor Channel           | Đang test thực chiến, chưa pass Completion Report                |
| FILE 01                      | Cần viết lại sạch sau test                                       |
| FILE 04                      | NOT_STARTED                                                      |
| Gateway / Meta / MC AI       | BLOCKED                                                          |
| Production-ready             | KHÔNG ĐƯỢC GỌI                                                   |
| Nguồn giá/quyền lợi/hoa hồng | Ginsengfood Core / Runtime Core / QuoteSnapshot                  |
| CRM 12 tháng                 | Đã khóa nghiệp vụ, cần mapping vào FILE 07/09/10/12              |
| Member Lifecycle             | Đã khóa rule duy trì 50%, ân hạng 60 ngày, proactive amount care |
| Gateway Transition           | Chỉ sau Completion Report + owner sign-off                       |

**1\. NGUỒN TỔNG HỢP**

| **Nguồn**                                            | **Nội dung được dùng để khóa**                                                                               |
| ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Real Sales Test Impact Report / AI-TEST-NOTE-001→063 | Handoff, combo, quote, Giờ Vàng, Diamond, Sâm Savigin, moderation, affiliate, CRM                            |
| Chính sách và hoa hồng thành viên                    | Chu kỳ thành viên, hạng, quyền lợi, 24/7, Giờ Vàng, Diamond link, hoa hồng, QuoteSnapshot                    |
| Bổ sung tin nhắn chăm sóc khách hàng                 | CRM trigger, message theo hạng, Golden Hour, Diamond referral, commission, birthday, Tết, post-purchase care |
| CRM 12-MONTH + Member Lifecycle                      | Chăm sóc 12 tháng, gợi ý 3 trụ, duy trì/nâng hạng, ân hạng 60 ngày, hạ hạng an toàn                          |
| Đoạn test hiện tại                                   | Product effectiveness, customer context, no "hệ thống", proactive member care, runtime-first                 |

**2\. NGUYÊN TẮC TỐI THƯỢNG PHẢI KHÓA**

| **Mã**  | **Nguyên tắc**                                            | **Diễn giải**                                                                                                          |
| ------- | --------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| CORE-01 | AI Advisor phải biết khách là ai trước khi tư vấn         | Resolve khách mới, khách cũ, member tier, Diamond, buyer từ link Diamond, doanh số, lịch sử mua, chu kỳ, open case     |
| CORE-02 | Không dùng từ "hệ thống" với khách                        | Dùng "Em kiểm theo hồ sơ của Mình", "theo chính sách hiện hành", hoặc trả lời trực tiếp khi runtime đã có dữ liệu      |
| CORE-03 | AI không tự tính giá/quyền lợi/hoa hồng                   | Mọi giá, quyền lợi, hoa hồng, quota, quyền mua sớm phải lấy từ Runtime Core / QuoteSnapshot                            |
| CORE-04 | Mỗi lần giới thiệu sản phẩm phải có hiệu dụng Đông phương | Single SKU, combo, upsell, CRM, Live/Messenger, member care đều phải có hiệu dụng theo y thực đồng nguyên              |
| CORE-05 | Khách đã mua quay lại phải hỏi thăm trước                 | Trước khi bán tiếp, AI hỏi sản phẩm trước dùng có hợp không, có hài lòng không                                         |
| CORE-06 | Quote phải line-by-line và đầy đủ                         | Giá niêm yết, giá chương trình, quyền lợi member/Diamond, ship, COD, VAT, tổng, hạn giữ giá                            |
| CORE-07 | Order draft phải là form prefill                          | Khách cũ dùng thông tin cũ phải render OrderConfirmationDraft có nút \[Xác nhận đơn\]                                  |
| CORE-08 | Public comment chỉ là cổng kéo Messenger                  | Không báo giá, không xin PII, không chốt đơn public                                                                    |
| CORE-09 | Affiliate/Diamond tách khỏi đại lý/mua sỉ                 | Affiliate gắn Diamond; đại lý/nhà phân phối/mua sỉ đi 0889787878                                                       |
| CORE-10 | Live abuse phải hide/no reply/no Messenger                | Chửi thề, troll, brand attack, phá Live thì moderation, không kéo Messenger                                            |
| CORE-11 | CRM message text immutable                                | Tin nhắn CRM đã duyệt không rewrite/paraphrase; chỉ render biến đúng trigger/tier                                      |
| CORE-12 | Gateway vẫn BLOCKED                                       | Không mở Gateway nếu chưa có evidence: resolver trace, guard trace, QuoteSnapshot, OrderDraft, order_code, CRM job log |
| CORE-13 | CRM 12 tháng là chăm sóc vòng đời                         | Không phải spam bán hàng; phải dựa Customer Memory, Order History, Product Effectiveness, Member Lifecycle             |
| CORE-14 | Member lifecycle phải proactive                           | Khi runtime có dữ liệu, AI nói rõ hạng, tích lũy, số cần mua thêm, ngày còn lại                                        |
| CORE-15 | Duy trì hạng = 50% chuẩn hạng                             | Hết chu kỳ chưa đạt thì ân hạng 60 ngày                                                                                |
| CORE-16 | Product recommendation mặc định theo 3 trụ + người thân   | 1 theo mùa + 1 chức năng + 1 bổ dưỡng + 1 chăm sóc người thân theo giới/ngữ cảnh                                       |
| CORE-17 | Priority conflict thắng CRM/member sales                  | Khi complaint/refund/privacy/payment/health case mở thì dừng CRM bán hàng                                              |
| CORE-18 | Cross-channel dedup bắt buộc                              | Không gửi trùng cùng message family đa kênh trong 12 giờ                                                               |
| CORE-19 | Member dispute phải escalation                            | Sai hạng, sai doanh số, mất quyền lợi, hoa hồng, hạ hạng tranh chấp → open case/human review                           |
| CORE-20 | Member outcome phải log                                   | Maintain, grace, upgrade, downgrade, winback phải log để đo giữ hạng/nâng hạng                                         |

**3\. OPEN ISSUE REGISTER - AI-TEST-NOTE-001 ĐẾN 063 + BỔ SUNG V2.1**

| **Note** | **Nội dung khóa**                                                                                                                               |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| 001      | Comment mơ hồ/ký hiệu/emoji phải kéo Messenger nếu policy cho phép                                                                              |
| 002      | Sau handoff thành công, các câu tiếp theo mặc định Messenger/private                                                                            |
| 003      | Hỏi giá nhiều SKU trong Messenger phải báo theo runtime, không hỏi vòng                                                                         |
| 004      | Active proposal/combo không được tụt về 1-2 SKU                                                                                                 |
| 005      | Tư vấn phải giữ y thực đồng nguyên, Quân-Thần-Tá-Sứ, nguyên liệu đặc sản, Sâm Savigin                                                           |
| 006      | Khách hỏi "cháo này/thành phần" không được nhảy sai SKU                                                                                         |
| 007      | "Ngon không" phải dùng trục Ngon như Mẹ nấu - thương ngay từ vị đầu tiên                                                                        |
| 008      | Quote phải có giá niêm yết + giá chương trình + ưu đãi + thành tiền                                                                             |
| 009      | Multi-SKU quote phải line-by-line                                                                                                               |
| 010      | Khách "mua/đồng ý/chốt" phải render bản tạm tính/order draft                                                                                    |
| 011      | Không hỏi quá nhiều kiểu y khoa/kiêng cữ nếu khách chưa nêu rủi ro                                                                              |
| 012      | Shared-user recommendation phải dùng combo 3 trụ hoặc Growth Combo                                                                              |
| 013      | Combo gia đình/4 dòng phải linh hoạt theo thành phần người dùng                                                                                 |
| 014      | Order draft phải có thông tin nhận hàng, thanh toán, xác nhận đơn                                                                               |
| 015      | Đơn đã có địa chỉ thì khách hỏi giao hàng phải trả ETA, không hỏi lại                                                                           |
| 016      | Order draft phải có phí ship, VAT, tổng cuối; ETA trả ngắn                                                                                      |
| 017      | Live/Giờ Vàng không cộng giảm giá thành viên; thành viên chỉ có quyền mua sớm nếu runtime xác nhận                                              |
| 018      | Khách chọn 2 sản phẩm phải tư vấn thêm 1 để miễn phí ship nếu policy cho phép                                                                   |
| 019      | Không nói bấm xác nhận đơn khi chưa có form/order draft đầy đủ                                                                                  |
| 020      | Khách sửa giỏ/thêm sản phẩm phải revalidate quote và render lại order draft                                                                     |
| 021      | VAT phải rõ; chốt Giờ Vàng có câu giữ đơn/giữ giá 5 phút                                                                                        |
| 022      | Tư vấn phải có hiệu dụng theo ẩm thực phương Đông                                                                                               |
| 023      | Khách đã chọn SKU cho con thì không hỏi lại tuổi nếu không có rủi ro mới                                                                        |
| 024      | Free ship upsell phải mở nhiều lựa chọn, không ép một dòng                                                                                      |
| 025      | Diamond 24/7 buyer benefit wording: vào link giới thiệu được giảm thêm 5%                                                                       |
| 026      | Giảm 5% từ link giới thiệu hiển thị ở tổng đơn, không lặp từng sản phẩm                                                                         |
| 027      | Sâm Savigin Value Answer Block và Scientific Evidence Lock                                                                                      |
| 027A     | Owner-approved Sâm Savigin Micro Q&A wording                                                                                                    |
| 028      | ASK_SAVIGIN_RESEARCHER: nêu TS. Phù Tường Nguyên Dũng và các nhà khoa học viên iCHM                                                             |
| 029      | Không bán củ Sâm Savigin nguyên liệu riêng lẻ                                                                                                   |
| 030      | Không bán giống/cây giống Sâm Savigin                                                                                                           |
| 031      | ASK_COMPANY_ADDRESS: đường Võ Văn Phẩm, ấp Bình Thành, Phường Bến Tre, Tỉnh Vĩnh Long                                                           |
| 032      | ASK_SAVIGIN_GROWING_LOCATION: Bảo tàng Dược liệu vùng cát biển Thạnh Hải, Vĩnh Long                                                             |
| 033      | ASK_VISIT_REQUEST: gọi 0817639939                                                                                                               |
| 034      | Đại lý/nhà phân phối/hợp tác/mua nhiều: gọi 0889787878                                                                                          |
| 035      | Liên hệ/gặp/trao đổi với sếp/công ty: gọi 0817639939                                                                                            |
| 036      | Xin số cá nhân lãnh đạo: không cung cấp, chuyển 0817639939                                                                                      |
| 037      | Diamond quay lại Messenger: nhắc sản phẩm mua gần nhất, hỏi có hợp không                                                                        |
| 038      | Khách đã mua phải hỏi thăm trước khi bán tiếp                                                                                                   |
| 039      | Combo/proposal phải có hiệu dụng Đông phương từng SKU                                                                                           |
| 040      | Giới thiệu sản phẩm nào cũng phải nói hiệu dụng Đông phương                                                                                     |
| 041      | Giá hôm nay khác hôm trước: giải thích theo chương trình/ngày, không lan man                                                                    |
| 042      | Khách Diamond/member quote phải có quyền lợi hạng nếu runtime eligible                                                                          |
| 043      | Quote/member rights đi qua CustomerIdentityResolver → MembershipTierResolver → ProgramEligibilityResolver → MemberBenefitResolver → QuoteEngine |
| 044      | Khách cũ dùng thông tin cũ: render OrderConfirmationDraft prefill                                                                               |
| 045      | Comment "mã/mả cho bọn bây": no reply/no Messenger/moderation                                                                                   |
| 046      | "Bọn lừa đảo": brand-risk; malicious thì moderation, khiếu nại thật thì CSKH                                                                    |
| 047      | Live abuse: hide/no reply/no Messenger/Purchase Block nếu đủ policy                                                                             |
| 048      | Profanity variant: nhận diện biến thể chửi thề/lách chữ                                                                                         |
| 049      | Affiliate tách đại lý/nhà phân phối/mua sỉ                                                                                                      |
| 050      | Member/price/commission policy đọc từ Core                                                                                                      |
| 051      | Affiliate/Diamond policy dùng biến Core                                                                                                         |
| 052      | No "hệ thống" customer-facing                                                                                                                   |
| 053      | Diamond hỏi affiliate: trả lời trực tiếp đủ điều kiện và hoa hồng theo biến runtime                                                             |
| 054      | MESSAGE_TEXT_IMMUTABLE_LOCK                                                                                                                     |
| 055      | MESSAGE_TRIGGER_ONLY_LOCK                                                                                                                       |
| 056      | NO_DUPLICATE_MESSAGE_IN_SAME_MONTH_LOCK                                                                                                         |
| 057      | CRM_BY_CUSTOMER_STAGE_LOCK                                                                                                                      |
| 058      | DIAMOND_COMMISSION_RUNTIME_LOCK                                                                                                                 |
| 059      | CUSTOMER_EMOTION_CRM_LOCK                                                                                                                       |
| 060      | NO_OVERSPAM_CRM_LOCK                                                                                                                            |
| 061      | GOLDEN_HOUR_MESSAGE_RUNTIME_LOCK                                                                                                                |
| 062      | POST_PURCHASE_CARE_LOCK                                                                                                                         |
| 063      | CRM_TEXT_DO_NOT_REWRITE_LOCK                                                                                                                    |
| 064      | CRM 12 tháng phải có lịch D0→M12                                                                                                                |
| 065      | Member lifecycle: duy trì 50%, ân hạng 60 ngày                                                                                                  |
| 066      | AI phải proactive amount-based care                                                                                                             |
| 067      | Product recommendation mặc định = 1 theo mùa + 1 chức năng + 1 bổ dưỡng + 1 người thân                                                          |
| 068      | Nữ có gia đình: add-on cho con/ba mẹ/chồng nếu context có                                                                                       |
| 069      | Nam: add-on cho vợ/nữ/dưỡng mô đẹp da                                                                                                           |
| 070      | Priority conflict: complaint/refund/privacy/payment/health thắng CRM/member sales                                                               |
| 071      | Cross-channel dedup 12 giờ                                                                                                                      |
| 072      | Member dispute escalation                                                                                                                       |
| 073      | Member lifecycle outcome logger                                                                                                                 |
| 074      | Member CRM không gửi nếu opt-out/open case/fatigue/quiet hour fail                                                                              |
| 075      | Hạ hạng phải nói nhẹ, không làm khách xấu hổ                                                                                                    |

**4\. CÁC NHÓM QUY TẮC PHẢI ĐƯA VÀO TÀI LIỆU**

**4.1. Public Comment / Messenger Handoff**

MUST:

- Comment mơ hồ/ký hiệu/emoji nếu không vi phạm policy phải kéo Messenger.
- Public chỉ trả lời ngắn, không báo giá, không hỏi PII.
- Sau handoff thành công, mặc định private/Messenger.
- Handoff phải giữ context: page, live, comment, product hint, attribution nếu có.

MUST NOT:

- Không tư vấn sâu ở public.
- Không chốt đơn public.
- Không xin số điện thoại/địa chỉ public.
- Không báo giá public.
- Không lặp CTA kéo Messenger nếu đã ở Messenger.

FAIL IF:

- AI báo giá ở comment.
- AI xin địa chỉ/số điện thoại public.
- Sau handoff vẫn xử lý như public.
- Handoff mất context live/ads/referral.

DONE GATE:

- Test ".", emoji, "ib", "giá", "chốt", "quan tâm".
- Handoff test phải có public reply + private context restored.

**4.2. Customer Context / Returning Customer Care**

MUST:

- AI resolve customer context trước khi trả lời.
- Khách đã mua phải hỏi thăm trải nghiệm trước khi bán tiếp.
- Nếu runtime có last_purchase, AI nhắc đúng sản phẩm mua gần nhất.
- Diamond/member phải được nhận diện đúng.
- Nếu khách cũ hỏi lại, AI không tư vấn như khách mới.

MUST NOT:

- Không bán ngay khi khách cũ chỉ mới chào.
- Không hỏi lại dữ liệu đã có.
- Không nói "để hệ thống kiểm tra".
- Không hỏi y khoa dài nếu khách không nêu rủi ro.

FAIL IF:

- Khách cũ quay lại mà AI bán ngay.
- AI không nhận ra Diamond/member khi context có.
- AI bịa lịch sử mua.
- AI hỏi lại sản phẩm đã mua dù runtime có.

DONE GATE:

- Test khách cũ / Diamond / member gần hết chu kỳ / khách đã mua Bào ngư nói "chào em".

**4.3. Product Effectiveness / Đông phương Lock**

MUST:

- Mọi sản phẩm được giới thiệu phải có hiệu dụng theo ẩm thực phương Đông/y thực đồng nguyên.
- Mỗi SKU trong combo phải có hiệu dụng riêng.
- Giữ brand soul: Sâm Savigin, y thực đồng nguyên, Ngon như Mẹ nấu, thực dưỡng, công nghệ sấy thăng hoa.
- CRM/member/gift/family recommendation phải có Product Effectiveness Block.

MUST NOT:

- Không chỉ nói "ngon/dễ ăn/phù hợp/bổ dưỡng".
- Không trả lời như catalog.
- Không dùng claim điều trị.
- Không dùng mã SKU nội bộ với khách.

FAIL IF:

- AI giới thiệu SKU mà thiếu hiệu dụng như thanh nhiệt/kiện tỳ/dưỡng khí/dưỡng âm/bổ huyết/bổ khoáng/phục hồi thể lực đúng SKU.
- Combo thiếu hiệu dụng từng item.
- CRM gợi ý mua lại thiếu lý do thực dưỡng.

DONE GATE:

- Test combo cả nhà mùa hè.
- Test người già.
- Test trẻ em.
- Test mua quà.
- Test chồng/phái mạnh.
- Test vợ/dưỡng mô đẹp da.

**4.4. Product Recommendation 3 Trụ + Chăm Sóc Người Thân**

MUST:

- Mặc định deep recommendation trong CRM/member/proposal phải có:
  - 01 sản phẩm theo mùa.
  - 01 sản phẩm chức năng.
  - 01 sản phẩm bổ dưỡng.
  - 01 sản phẩm chăm sóc người thân theo giới/ngữ cảnh gia đình.
- Mỗi sản phẩm phải có Product Effectiveness.
- Nếu người mua là nữ có gia đình, ưu tiên add-on cho con/ba mẹ/chồng nếu context có.
- Nếu người mua là nam, ưu tiên add-on cho vợ/người phụ nữ trong gia đình theo hướng dưỡng mô, đẹp da, bồi bổ nhẹ nhàng.

MUST NOT:

- Không ép combo.
- Không gọi đây là combo bắt buộc.
- Không gợi ý sản phẩm cho trẻ nếu không có ngữ cảnh an toàn.
- Không gợi ý phái mạnh nhạy cảm trên public.
- Không báo giá nếu chưa có QuoteSnapshot.

FAIL IF:

- AI chỉ gợi ý 1 sản phẩm khi context yêu cầu deep recommendation.
- Thiếu 1 trong 3 trụ.
- Thiếu family add-on khi có ngữ cảnh gia đình.
- Gợi ý trái dị ứng/ăn chay/health/sellable.

DONE GATE:

- Test nữ có gia đình mua cho cả nhà.
- Test nam mua cho gia đình/vợ.
- Test mẹ mua cho con.
- Test mua cho ba mẹ.
- Test chồng/phái mạnh.
- Test khách từ chối combo → fallback 1 sản phẩm.

**4.5. Quote / Price / Member Rights**

MUST:

- Mọi báo giá lấy từ QuoteSnapshot.
- Quote line-by-line: giá niêm yết, giá chương trình, quyền lợi member/Diamond nếu eligible, ship, COD, VAT, tổng.
- Member/Diamond benefit phải hiển thị nếu runtime eligible.
- VAT hiển thị rõ: {{vat_display}} hoặc "Không áp dụng".
- Quote phải có hạn giữ giá.
- Giờ Vàng giữ giá theo runtime/QuoteLock.
- Không quote nếu thiếu Core data.

MUST NOT:

- Không hardcode giá/quyền lợi/hoa hồng.
- Không bỏ quyền lợi Diamond/member khi runtime eligible.
- Không dùng "VAT nếu áp dụng".
- Không public final price trên live/comment.
- Không tự cộng nhiều quyền lợi nếu policy không cho phép.

FAIL IF:

- Diamond quote không có quyền lợi hạng.
- AI tự tính.
- Quote không có final_total.
- Quote thiếu line-by-line.
- Quote thiếu applied benefit policy.

DONE GATE:

- Test Diamond 24/7.
- Test Giờ Vàng không cộng hạng nếu policy không cho.
- Test buyer từ link Diamond.
- Test multi-SKU quote.
- Test hết hạn quote → revalidate.

**4.6. Order Draft / Confirmation Form**

MUST:

- Sau "mua/đồng ý/chốt" phải render OrderConfirmationDraft.
- Khách cũ dùng thông tin cũ phải prefill form đầy đủ.
- Form có 3 phần: thông tin nhận hàng, phương thức thanh toán, xác nhận đơn.
- Có nút:
  - \[Xác nhận đơn\]
  - \[Sửa thông tin nhận hàng\]
  - \[Đổi phương thức thanh toán\]
- Nếu khách sửa giỏ/thêm/bớt SKU, phải revalidate quote và render lại order draft.
- Chỉ nói order success khi có order_code.

MUST NOT:

- Không chỉ tóm tắt text.
- Không báo thành công nếu chưa có order_code.
- Không cho xác nhận nếu quote thiếu quyền lợi.
- Không tạo order từ public comment.

FAIL IF:

- Không có trường nhận hàng đã điền.
- Không có nút xác nhận.
- Không re-render sau khi đổi giỏ.
- Không có order_code mà nói đơn đã ghi nhận.

DONE GATE:

- Test khách cũ chọn "thông tin cũ nhé".
- Test thêm/bỏ SKU.
- Test expired quote.
- Test order_code success.

**4.7. Member Lifecycle / Upgrade / Maintain / Grace / Downgrade**

MUST:

- Chu kỳ thành viên = 12 tháng.
- Duy trì hạng = đạt từ 50% doanh số chuẩn của hạng hiện tại.
- Ân hạng = 60 ngày.
- Trong ân hạng, nếu đạt 50% thì duy trì hạng.
- Hết 60 ngày không đạt thì hạ hạng theo runtime.
- Nếu đủ nâng hạng trong chu kỳ/ân hạng, nâng hạng theo runtime policy.
- AI phải chủ động nói:
  - Mình đang ở hạng gì.
  - Đã tích lũy bao nhiêu.
  - Cần mua thêm khoảng bao nhiêu để duy trì hạng.
  - Cần mua thêm khoảng bao nhiêu để nâng lên hạng nào.
  - Còn bao nhiêu ngày trong chu kỳ.
  - Nếu đang ân hạng, còn bao nhiêu ngày ân hạng.

MUST NOT:

- Không bịa hạng.
- Không bịa doanh số.
- Không bịa số ngày còn lại.
- Không tự tính nâng/duy trì/hạ.
- Không dọa khách bị hạ hạng.
- Không nói "để Em kiểm tra" nếu runtime đã có dữ liệu.

FAIL IF:

- AI nói thiếu số cần mua thêm khi runtime có.
- AI nói sai hạng.
- AI nói hạ hạng gây áp lực.
- AI gửi member CRM khi opt-out/open case/fatigue fail.
- AI quote sản phẩm member care mà thiếu QuoteSnapshot.

DONE GATE:

- Test member đủ maintain.
- Test member chưa đủ maintain.
- Test grace start.
- Test grace D30.
- Test grace recovered.
- Test downgrade safe.
- Test dispute doanh số/hạng.

**4.8. CRM Messaging / Emotional Care**

MUST:

- Tin nhắn gắn đúng trigger.
- Không gửi sai hạng.
- Không gửi trùng cùng tháng.
- Không rewrite câu chữ đã duyệt.
- Tần suất có fatigue guard/quiet hours.
- CRM phải tạo cảm giác được quan tâm, trân trọng, đồng hành.
- Sau mua phải hỏi ngon/hợp/phục vụ/mua tiếp.
- Diamond CRM nhắc link, đăng bài, tài nguyên, commission theo đúng trigger.

MUST NOT:

- Không tự tạo trigger phụ.
- Không đổi brand soul.
- Không gửi quyền Diamond cho hạng khác.
- Không gửi commission khi chưa ghi nhận thành công.
- Không gửi CRM bán hàng khi khách đang có open case.

FAIL IF:

- Sai tier.
- Trùng message tháng.
- Hardcode quyền vào sớm/hoa hồng/quota.
- Tin nhắn bị rewrite sai câu chữ đã duyệt.
- CRM spam quá dày.

DONE GATE:

- Test birthday.
- Test Tết.
- Test Golden Hour T-10/T-5.
- Test tier_change.
- Test delivered order.
- Test Diamond referral/commission.
- Test opt-out.
- Test open case.

**4.9. Priority Conflict / Suppression**

MUST:

- Complaint/refund/privacy/payment/delivery/health review thắng CRM/member sales.
- Nếu khách đang khiếu nại hoặc tranh chấp, dừng tin nhắn duy trì/nâng hạng.
- Ưu tiên xử lý case trước bán hàng.

MUST NOT:

- Không gửi "mua thêm để duy trì hạng" khi khách đang khiếu nại.
- Không gửi Diamond commission promo khi khách đang dispute.
- Không upsell trong cùng luồng complaint.

FAIL IF:

- CRM gửi khi open_case_status = QUALITY / COUNTERFEIT / PRIVACY / PAYMENT.
- AI nhắc nâng hạng khi khách đang refund.
- AI bán tiếp khi health-sensitive chưa resolve.

DONE GATE:

- Test complaint open.
- Test payment dispute.
- Test counterfeit case.
- Test refund pending.
- Test health unresolved.

**4.10. Cross-channel Dedup**

MUST:

- Không gửi cùng message family đa kênh trong 12 giờ.
- Không gửi cùng trigger nhiều lần.
- Không gửi lại message đã gửi trong tháng nếu message lock quy định không trùng.

CROSS_CHANNEL_DEDUP_RULE:  
dedup_window: "12 hours"  
channels:  
\- Messenger  
\- SMS  
\- Email  
\- Zalo_if_any  
\- Web_push_if_any

MUST NOT:

- Không gửi Messenger + SMS + Email cùng nội dung trong 12 giờ.
- Không retry bằng nội dung khác nếu FinalGuard fail.

FAIL IF:

- Một khách nhận cùng nội dung qua 2 kênh trong 12 giờ.
- Cùng trigger tạo nhiều CRM job.

DONE GATE:

- Test Messenger sent → SMS suppressed.
- Test retry provider timeout.
- Test duplicate trigger.

**4.11. Member Dispute Escalation**

MUST:

- Nếu khách phản ứng về sai hạng, sai doanh số, thiếu giảm giá, thiếu hoa hồng, link Diamond, hạ hạng, ân hạng, đơn không được tính → mở case.
- AI không tranh luận.
- AI không tự sửa hạng/quyền lợi.
- AI phản hồi nhẹ và chuyển kiểm tra nội bộ.

Câu an toàn:

Dạ Em ghi nhận phản hồi của Mình ạ. Phần doanh số/hạng/quyền lợi cần đối chiếu đúng theo hồ sơ đơn hàng hợp lệ, nên Em sẽ chuyển nội bộ kiểm tra để tránh trả lời sai quyền lợi của Mình ạ.

MUST NOT:

- Không khẳng định khách sai.
- Không nói "hệ thống ghi nhận vậy".
- Không tự bù quyền lợi.
- Không tiếp tục upsell.

FAIL IF:

- AI tranh luận với khách.
- AI tự sửa hạng.
- AI tiếp tục bán trong dispute.

DONE GATE:

- Test wrong tier dispute.
- Test missing commission.
- Test downgrade complaint.
- Test order not counted.

**4.12. Member Lifecycle Outcome Logger**

MUST:

- Ghi outcome cho toàn bộ member lifecycle.
- Outcome dùng cho dashboard/BI, không public.

MEMBER_LIFECYCLE_OUTCOME:  
\- MAINTAIN_SUCCESS  
\- MAINTAIN_RISK  
\- GRACE_STARTED  
\- GRACE_RECOVERED  
\- DOWNGRADED  
\- UPGRADE_SUCCESS  
\- MEMBER_RETAINED  
\- MEMBER_WINBACK  
\- MEMBER_DISPUTE_OPENED  
\- MEMBER_CRM_SUPPRESSED

MUST NOT:

- Không dùng outcome để ép khách.
- Không public internal outcome.
- Không dùng outcome để tự sửa chính sách.

FAIL IF:

- Không log maintain/grace/downgrade.
- Không đo được tỷ lệ cứu ân hạng.
- Không biết sản phẩm nào hỗ trợ giữ hạng tốt nhất.

DONE GATE:

- Test outcome log after maintain.
- Test outcome log after grace start.
- Test outcome log after downgrade.
- Test CRM suppressed outcome.

**4.13. Affiliate / Diamond / Distributor Separation**

MUST:

- Affiliate/khởi nghiệp gắn với Diamond và customer context.
- Khách đã là Diamond trả lời trực tiếp đủ điều kiện và chính sách hoa hồng theo biến runtime.
- Khách mới: nói cần kích hoạt thành viên, trải nghiệm sản phẩm, tích lũy đến Diamond.
- Member chưa Diamond: nói hạng hiện tại, doanh số tích lũy, còn thiếu để Diamond.
- Đại lý/nhà phân phối/mua sỉ: 0889787878.
- Gặp công ty/lãnh đạo/tham quan: 0817639939.

MUST NOT:

- Không nhầm affiliate với đại lý.
- Không dùng từ "hệ thống".
- Không nói Diamond còn phải đạt Diamond.
- Không hardcode % nếu runtime variable chưa resolve.

FAIL IF:

- AI đưa số 0889787878 cho affiliate.
- AI nói khách Diamond chưa đủ điều kiện Diamond.
- AI hardcode hoa hồng khi không có biến.

DONE GATE:

- Test khách mới hỏi affiliate.
- Test Gold hỏi affiliate.
- Test Diamond hỏi chính sách.
- Test mua sỉ/đại lý.
- Test xin số cá nhân lãnh đạo.

**4.14. Live Moderation / Profanity / Blacklist**

MUST:

- Comment chửi thề/xúc phạm/troll/phá Live phải đi moderation.
- Không public reply.
- Không Messenger.
- Không nhắn riêng.
- Ẩn bình luận theo policy.
- Purchase Block nếu malicious được xác nhận.
- Có normalization/fuzzy matching cho tiếng Việt có dấu/không dấu/sai chính tả/ký tự chen giữa.

MUST NOT:

- Không chỉ hardcode vài câu.
- Không để lọt biến thể lách chữ.
- Không auto blacklist khi là khiếu nại thật có mã đơn/mã lô/bằng chứng.

FAIL IF:

- AI đôi co.
- AI kéo Messenger troll.
- AI cho account blacklist tạo quote/order.
- AI chặn nhầm complaint thật.

DONE GATE:

- Test "bọn lừa đảo".
- Test "đụ má", không dấu, tách ký tự.
- Test khiếu nại thật có mã đơn.
- Test troll burst.

**5\. LOCKED RESPONSE OWNER-APPROVED**

| **Intent**                                  | **Locked response**                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ASK_SAVIGIN_IDENTITY                        | Dạ Sâm Savigin / Vietnam Ocean Ginseng là kết quả nghiên cứu khoa học đã được công bố khoa học quốc tế trên tạp chí Natural Product Research ngày 11 tháng 3 năm 2026. Sâm Savigin cho thấy có đến 158 hoạt chất quý có hàm lượng cao trong đó nổi bật với nhóm triterpenoid, saponin và sterol. Đây là loài sâm được trồng trên cát biển tại Việt Nam và duy nhất trên thế giới ạ.                                                                                                                                                                                                                                                                                                                                           |
| ASK_SCIENTIFIC_EVIDENCE                     | Dạ có ạ. Em gửi Mình link công bố khoa học chính thức để Mình xem trực tiếp ạ: {{approved_science_publication_link}}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ASK_SAVIGIN_COMPARE_OTHER_GINSENG           | Dạ Sâm Hàn là Panax ginseng, nổi bật với nhóm ginsenosides/panaxosides, thường được dùng theo dạng củ, hồng sâm, cao, trà hoặc viên bổ sung. Sâm Mỹ là Panax quinquefolius, cũng thuộc nhóm Panax, dùng nhiều trong thực phẩm bổ sung/dược liệu. Sâm Ngọc Linh là Panax vietnamensis, đặc hữu Việt Nam, nổi bật với nhóm saponin đặc trưng, đặc biệt majonoside R2. Sâm Savigin với nhóm hoạt chất phong phú và khác biệt rất riêng: trong đó nổi bật với 158 hoạt chất quý hàm lượng cao với nền phytochemical nổi bật với triterpenoid, sterol, saponin, phenolics, flavonoids và đã có công bố khoa học quốc tế ạ.                                                                                                         |
| ASK_SAVIGIN_RESEARCHER                      | Dạ Sâm Savigin / Vietnam Ocean Ginseng do TS. Phù Tường Nguyên Dũng và các nhà khoa học viên iCHM nghiên cứu ạ. Đây là kết quả nghiên cứu dược liệu vùng cát biển tại Việt Nam, đã được công bố khoa học quốc tế trên tạp chí Natural Product Research ngày 11 tháng 3 năm 2026 ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ASK_RAW_SAVIGIN_TUBER_SALE                  | Dạ hiện bên Em không bán củ nguyên liệu riêng lẻ vì trồng trên cát biển sản lượng thấp ạ. Sâm Savigin / Vietnam Ocean Ginseng hiện được chuẩn hóa làm nguyên liệu trung tâm trong Cháo Sâm Savigin, để bảo đảm đúng công thức, hàm lượng, quy trình từ đó giúp tạo nên những sản phẩm thực dưỡng tốt cho sức khỏe ạ.                                                                                                                                                                                                                                                                                                                                                                                                          |
| ASK_SAVIGIN_SEEDLING_SALE                   | Dạ hiện bên Em không bán giống / cây giống Sâm Savigin ra ngoài ạ. Sâm Savigin / Vietnam Ocean Ginseng được trồng trên cát biển bằng quy trình kỹ thuật riêng, điều kiện môi trường riêng, do vậy nhằm đảm bảo chất lượng, tránh hàng giả, hàng kém chất lượng bên Em không bán giống ra ngoài mà tập trung nghiên cứu ra sản phẩm cụ thể phục vụ cộng đồng ạ.                                                                                                                                                                                                                                                                                                                                                                |
| ASK_COMPANY_ADDRESS                         | Dạ địa chỉ là đường Võ Văn Phẩm, ấp Bình Thành, Phường Bến Tre, Tỉnh Vĩnh Long ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| ASK_SAVIGIN_GROWING_LOCATION                | Dạ Sâm Savigin / Vietnam Ocean Ginseng được trồng tại Bảo tàng Dược liệu vùng cát biển Thạnh Hải, Vĩnh Long, Việt Nam ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ASK_VISIT_REQUEST                           | Mình vui lòng gọi số điện thoại 0817639939 để sắp lịch và đón tiếp Mình chu đáo ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ASK_CONTACT_COMPANY_EXECUTIVE               | Mình vui lòng gọi số điện thoại 0817639939 để sắp lịch trao đổi và được công ty hỗ trợ Mình chu đáo ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ASK_AGENCY_DISTRIBUTOR_PARTNERSHIP_BULK_BUY | Dạ Mình vui lòng gọi số điện thoại 0889787878 để công ty trao đổi trực tiếp về chính sách đại lý / nhà phân phối / hợp tác kinh doanh / mua số lượng nhiều và hỗ trợ Mình chu đáo ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ASK_EXECUTIVE_PERSONAL_PHONE                | Dạ Em không cung cấp số điện thoại cá nhân của TS. Phù Tường Nguyên Dũng / Giám đốc / Chủ tịch / Viện trưởng ạ. Mình vui lòng gọi số 0817639939 để công ty sắp lịch kết nối và hỗ trợ Mình chu đáo ạ.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ASK_AFFILIATE_DIAMOND_MEMBER                | Dạ Mình đang là Diamond, Mình đủ điều kiện trở thành nhà khởi nghiệp cùng SSAVIGROUP ạ. Chính sách khởi nghiệp như sau: với các đơn giới thiệu hợp lệ qua link Diamond của Mình, quyền lợi hoa hồng Diamond hiện áp dụng theo chương trình: 24/7 là {{diamond_commission_24_7_percent}}, Giờ Vàng Tri Ân là {{diamond_commission_golden_hour_percent}} ạ. Điều kiện này được đặt ra vì SSAVIGROUP muốn nhà đồng hành là người đã có trải nghiệm sản phẩm, hiểu sản phẩm và thật sự muốn đồng hành khởi nghiệp cùng SSAVIGROUP ạ.                                                                                                                                                                                              |
| ASK_AFFILIATE_NEW_CUSTOMER                  | Dạ chương trình affiliate / nhà đồng hành khởi nghiệp của SSAVIGROUP gắn với hạng Diamond ạ. Hiện Mình chưa có doanh số mua hàng hợp lệ, nên bước đầu tiên là kích hoạt thành viên và trải nghiệm sản phẩm. Khi Mình mua sản phẩm, doanh số hợp lệ sẽ được tích lũy theo hạng thành viên; khi đạt Diamond, Mình sẽ đủ điều kiện tham gia chương trình affiliate / nhà đồng hành khởi nghiệp cùng SSAVIGROUP ạ. Điều kiện này giúp nhà đồng hành có thời gian trải nghiệm sản phẩm, hiểu sản phẩm và xác định có thật sự muốn khởi nghiệp cùng SSAVIGROUP không ạ.                                                                                                                                                             |
| ASK_AFFILIATE_MEMBER_NOT_DIAMOND            | Dạ Mình hiện đang là {{member_tier_display}}. Doanh số tích lũy hợp lệ của Mình là {{lifetime_revenue_display}}. Để đạt Diamond nhà đồng hành khởi nghiệp cùng SSAVIGROUP, Mình còn cần thêm {{amount_to_diamond_display}} doanh số hợp lệ. Khi Mình đạt Diamond, Mình sẽ đủ điều kiện tham gia affiliate / nhà đồng hành khởi nghiệp theo chính sách hiện hành. Với các đơn giới thiệu hợp lệ, quyền lợi hoa hồng Diamond hiện áp dụng theo chương trình: 24/7 là {{diamond_commission_24_7_percent}} và Giờ Vàng Tri Ân là {{diamond_commission_golden_hour_percent}} ạ. Điều kiện này giúp nhà đồng hành có thời gian trải nghiệm sản phẩm, hiểu sản phẩm và xác định có thật sự muốn khởi nghiệp cùng SSAVIGROUP không ạ. |

**6\. BIẾN RUNTIME / CORE CONTRACT CẦN ĐỌC**

| **Nhóm biến**            | **Biến bắt buộc**                                                                                                                                                                                                                                                                           |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Customer / Member        | customer_id, member_id, customer_identity_status, member_status, member_tier, is_member, is_diamond_member, buyer_from_diamond_link, referral_bind_status, referral_owner_tier                                                                                                              |
| Membership Cycle         | member_cycle_duration_months, cycle_start_date, cycle_end_date, membership_cycle_status, days_to_cycle_end, grace_period_status, grace_period_days, days_left_in_grace                                                                                                                      |
| Revenue / Tier Progress  | eligible_revenue_in_cycle, lifetime_revenue, current_tier_min_revenue, next_tier, amount_to_next_tier, amount_to_diamond, amount_to_maintain_current_tier, maintain_required_revenue, accumulated_valid_revenue_display                                                                     |
| Member Benefits          | silver_member_discount_percent, gold_member_discount_percent, platinum_member_discount_percent, diamond_member_discount_percent, allow_member_benefit, member_discount_amount, member_discount_display                                                                                      |
| 24/7 Program             | current_program, program_type, initial_24_7_discount_percent, successful_sales_count_previous_day, program_discount_percent, program_price, allow_referral_benefit                                                                                                                          |
| Diamond Referral         | diamond_referral_link_id, diamond_referral_bind_status, diamond_referral_eligible, buyer_from_diamond_link_24_7_discount_percent, buyer_referral_discount_amount, buyer_referral_discount_display                                                                                           |
| Golden Hour              | golden_hour_session_id, golden_hour_session_status, golden_hour_session_start_time, golden_hour_session_end_time, golden_hour_session_quota, session_sku_quota, sellable_stock, golden_hour_enabled                                                                                         |
| Early Access             | silver_early_access_minutes, gold_early_access_minutes, platinum_early_access_minutes, diamond_early_access_minutes, buyer_from_diamond_link_early_access_minutes, early_access_start_time, early_access_eligible                                                                           |
| Diamond Commission       | diamond_commission_24_7_percent, diamond_commission_golden_hour_percent, commission_eligible, commission_rate, commission_amount, commission_status, commission_source_order_id                                                                                                             |
| Sellable / SKU           | sku_id, sku_public_name, sellable_status, listed_price_status, listed_price, stock_available, quality_hold_status, recall_hold_status, sale_lock_status, channel_block_status                                                                                                               |
| QuoteSnapshot            | quote_snapshot_id, quote_created_at, quote_expires_at, quote_hold_minutes, program_name, program_discount_percent, listed_price, program_price, line_total, subtotal_after_program_price, shipping_fee, cod_fee, vat_display, final_total, applied_benefit_policy, policy_priority_decision |
| CRM / Messaging          | message_trigger_id, message_template_id, customer_stage, current_tier_display_name, last_message_sent_at, same_month_duplicate_status, fatigue_status, quiet_hour_status                                                                                                                    |
| Product Recommendation   | seasonal_product_public_name, functional_product_public_name, nourishing_product_public_name, family_addon_product_public_name, eastern_effectiveness_summary, hero_ingredients, ingredient_synergy_effect, suitable_context                                                                |
| Member Lifecycle Outcome | member_lifecycle_outcome, maintain_status, upgrade_status, grace_status, downgrade_status, dispute_status                                                                                                                                                                                   |

**7\. RESOLVER / GUARD / ENGINE CẦN KHÓA TRONG FILE 04**

| **Nhóm**               | **Resolver / Guard / Engine**                                                                                                                                                                                                                       |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Identity / Context     | CustomerIdentityResolver, MembershipTierResolver, CustomerMemoryResolver, LastPurchaseResolver, MemberPolicyContextGuard, IdentityResolutionGuard                                                                                                   |
| Member Lifecycle       | MemberLifecycleResolver, RuntimeRightsResolver, MemberMaintainResolver, MemberUpgradeResolver, MemberGracePeriodResolver, MemberDowngradeResolver, MemberDisputeEscalationResolver, MemberLifecycleOutcomeLogger                                    |
| Sales / Proposal       | ActiveProposalContextResolver, SeasonalRuntimeResolver, SharedUserFitResolver, FourLineComboResolver, ProductTriadFamilyAddOnResolver, ProductEffectivenessResolver, ProposalContinuityGuard, SalesMomentumGuard, QuestionBudgetGuard, OverAskGuard |
| Price / Quote          | ProgramPriceResolver, MemberBenefitResolver, DiamondReferralResolver, RuntimeRightsResolver, QuoteEngine, QuoteRightsGuard, QuoteCompletenessGuard, QuoteCartChangeGuard                                                                            |
| Order                  | OrderDraftCompletenessGuard, OrderDraftPrefillFormGuard, CustomerConfirmationEligibilityGuard, ConfirmationButtonEnableGuard, OrderCodeSuccessGuard                                                                                                 |
| Science / Claim        | ScientificEvidenceResolver, SaviginScienceClaimGuard, ContentExactMatchGuard, NoExtraSalesExpansionGuard                                                                                                                                            |
| Contact Routing        | OfficialContactRoutingGuard, ExecutivePrivacyGuard                                                                                                                                                                                                  |
| Moderation             | LiveAbuseClassifier, ProfanityNormalizer, BrandAttackClassifier, TrollSpamBurstGuard, PurchaseBlockGuard, ComplaintEvidenceGuard                                                                                                                    |
| CRM                    | MessageTriggerResolver, TierMessageEligibilityGuard, MonthlyDuplicateGuard, CrossChannelDedupGuard, MessageFatigueGuard, QuietHoursGuard, MessageTextImmutableGuard, DiamondCommissionMessageGuard, CRMOpenCaseSuppressionGuard                     |
| Priority / Suppression | PriorityConflictResolver, OpenCaseSuppressionGuard, RefundDisputeGuard, PrivacyRequestGuard, HealthReviewSuppressionGuard                                                                                                                           |
| Audit / Evidence       | DecisionEnvelopeLogger, ResolverTraceLogger, GuardTraceLogger, CRMJobLog, QuoteSnapshotEvidenceLogger, OrderDraftEvidenceLogger                                                                                                                     |

**8\. SITUATION REGISTRY CẦN BỔ SUNG**

SIT-LIVE-LOW-INTENT-ENGAGEMENT  
SIT-HANDOFF-PRIVATE-CONTEXT-RESTORED  
SIT-STO-MULTI-SKU-PRICE-INQUIRY  
SIT-GROWTH-TASTE-REASSURANCE  
SIT-ADV-ANSWER-FIRST  
SIT-HS-PREGNANCY-CONDITIONAL-ADVISORY  
SIT-GROWTH-SHARED-USER  
SIT-COMBO-FAMILY-DYNAMIC  
SIT-FREESHIP-UPSELL-THIRD-ITEM  
SIT-STO-CART-UPDATED-BEFORE-CONFIRMATION  
SIT-DIAMOND-24_7-BUYER-BENEFIT  
SIT-DIAMOND-24_7-QUOTE-SUMMARY  
SIT-SCIENCE-SAM-SAVIGIN-VALUE  
SIT-TRACE-SCIENCE-EVIDENCE-LINK  
SIT-SAVIGIN-IDENTITY  
SIT-SAVIGIN-SCIENCE-EVIDENCE-LINK  
SIT-SAVIGIN-COMPARE-PANAX  
SIT-SAVIGIN-RESEARCHER  
SIT-SAVIGIN-RAW-TUBER-SALE  
SIT-SAVIGIN-SEEDLING-SALE  
SIT-COMPANY-ADDRESS  
SIT-SAVIGIN-GROWING-LOCATION  
SIT-VISIT-REQUEST  
SIT-CONTACT-COMPANY-EXECUTIVE  
SIT-AGENCY-DISTRIBUTOR-BULK  
SIT-EXECUTIVE-PERSONAL-PHONE-REQUEST  
SIT-RETURNING-CUSTOMER-CARE-BEFORE-SALE  
SIT-DIAMOND-NEAR-CYCLE-HELLO  
SIT-DYNAMIC-PRICE-DIFFERENCE  
SIT-ORDER-DRAFT-PREFILL-FORM  
SIT-LIVE-ABUSE-HIDE-NO-REPLY  
SIT-PROFANITY-ABUSE-VARIANT  
SIT-AFFILIATE-NEW-CUSTOMER  
SIT-AFFILIATE-MEMBER-PROGRESS  
SIT-AFFILIATE-DIAMOND-DIRECT  
SIT-CRM-GOLDEN-HOUR-MESSAGE  
SIT-CRM-DIAMOND-REFERRAL-SUCCESS  
SIT-CRM-DIAMOND-COMMISSION  
SIT-CRM-POST-PURCHASE-CARE  
SIT-CRM-MEMBER-LIFECYCLE-MAINTAIN  
SIT-CRM-MEMBER-LIFECYCLE-UPGRADE  
SIT-CRM-MEMBER-GRACE-START  
SIT-CRM-MEMBER-GRACE-RECOVERED  
SIT-CRM-MEMBER-DOWNGRADE-SAFE  
SIT-CRM-PRODUCT-TRIAD-FAMILY-ADDON  
SIT-MEMBER-DISPUTE-ESCALATION  
SIT-CRM-CROSS-CHANNEL-DEDUP  
SIT-CRM-PRIORITY-CONFLICT-SUPPRESSION  
SIT-CRM-MEMBER-OUTCOME-LOGGING

**9\. REWRITE IMPACT MATRIX THEO FILE**

| **File**           | **Nội dung cần khóa khi viết lại**                                                                                                                                                               |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| FILE 00            | Source of Truth, Brand Soul, Gateway BLOCKED, owner-approved science claim, no production-ready, CRM/member lifecycle là native capability                                                       |
| FILE 01            | Parent Logic: customer context first, public→Messenger, returning care, product effectiveness, quote/order gates, affiliate separation, moderation boundary, member lifecycle, priority conflict |
| FILE 02            | SKU Knowledge: role trong combo, hiệu dụng Đông phương từng SKU, taste profile, family/child/gift/elderly/seasonal roles, triad roles                                                            |
| FILE 03            | Data Contract: product_effectiveness_block, combo_role, seasonal_runtime_role, quote_line_reason, science_evidence_view, CRM message references, member lifecycle variables                      |
| FILE 04            | Resolver/Guard/Runtime Contract: toàn bộ resolver/guard mục 7, đặc biệt MemberLifecycleResolver, ProductTriadFamilyAddOnResolver, CrossChannelDedupGuard, MemberDisputeEscalationResolver        |
| FILE 05            | Implementation Layer: DTO/state cho QuoteSnapshot, OrderDraft, prefill form, member benefit, VAT/ship/COD, CRM job, message log, member lifecycle job/outcome                                    |
| FILE 06            | Claim/Safety: Sâm Savigin claim, no treatment, executive privacy, no secret planting process, moderation/abuse, health-sensitive guard, CRM/member wording safety                                |
| FILE 07            | Common Content: locked responses, returning care, quote/order templates, Golden Hour, affiliate templates, CRM content blocks, member lifecycle templates                                        |
| FILE 08            | SKU Sales Content: mỗi SKU có hiệu dụng Đông phương, taste, combo role, quote reason, order confirmation reason, CRM recommendation reason                                                       |
| FILE 09            | Test Matrix: exact-match locked response, quote/order, Diamond, affiliate, moderation, CRM trigger, member lifecycle, no "hệ thống", evidence requirements                                       |
| FILE 10            | Situation Registry: toàn bộ SIT ở mục 8, map trigger→resolver→guard→action→content→test                                                                                                          |
| FILE 11 / Appendix | Open Issue Register, Completion Report PENDING_EVIDENCE, Gateway BLOCKED, CRM Runtime Appendix, Core Runtime Integration Appendix                                                                |
| FILE 12            | Human-Level Sales: không hỏi vòng, chăm sóc trước bán, giữ lực chốt, combo linh hoạt, Diamond/affiliate/CRM emotional behavior, member care chủ động                                             |

**10\. DONE GATE TRƯỚC KHI VIẾT LẠI / MỞ GATEWAY**

Pass các nhóm test sau:

- Public comment mơ hồ → Messenger.
- Abuse/troll → hide/no reply/no Messenger.
- Customer context: khách mới, khách cũ, member, Diamond, buyer link Diamond, gần hết chu kỳ.
- Product effectiveness: mọi single/combo/CRM recommendation đều có hiệu dụng Đông phương đúng SKU.
- Sâm Savigin locked responses exact-match và không lan man.
- Quote: line-by-line, QuoteSnapshot, member benefit, VAT, ship, final_total, quote expiry.
- OrderDraft: form prefill, nút xác nhận, sửa thông tin, đổi thanh toán, order_code success only.
- Affiliate: khách mới/member/Diamond trả đúng trạng thái; không nhầm đại lý/mua sỉ.
- CRM: đúng trigger, đúng tier, không rewrite text, không gửi trùng tháng, fatigue guard.
- Member lifecycle: chủ động nói hạng, tích lũy, số còn thiếu, ngày còn lại, grace days.
- Maintain/grace/downgrade: duy trì 50%, ân hạng 60 ngày, hạ hạng an toàn.
- Product triad: 1 theo mùa + 1 chức năng + 1 bổ dưỡng + 1 người thân.
- Priority conflict: open case/refund/privacy/payment/health suppress CRM/member sales.
- Cross-channel dedup: không gửi trùng đa kênh trong 12 giờ.
- Member dispute: open case/human review, không tranh luận.
- Outcome logger: maintain/grace/upgrade/downgrade/winback có log.

Evidence bắt buộc:

- DecisionEnvelope.
- Resolver trace.
- Guard trace.
- QuoteSnapshot.
- OrderDraft.
- order_code.
- CRM job log.
- Member lifecycle job log.
- Outcome log.
- Suppression log.
- Cross-channel dedup log.

Gateway chỉ được mở sau Completion Report có evidence thật và owner sign-off.

**11\. THỨ TỰ LÀM TIẾP**

| **Ưu tiên** | **Việc cần làm**                                                   |
| ----------- | ------------------------------------------------------------------ |
| 1           | Owner rà bản V2.1 này                                              |
| 2           | Nếu đạt, viết lại FILE 01 bản sạch theo Master Rule Lock V2.1      |
| 3           | Viết FILE 04 Resolver / Guard / Runtime Contract                   |
| 4           | Viết FILE 07 Common Content Blocks                                 |
| 5           | Viết FILE 10 Situation Registry                                    |
| 6           | Viết FILE 09 Test Matrix / Done Gate                               |
| 7           | Viết FILE 12 Human-Level Sales / CRM / Affiliate / Member Behavior |
| 8           | Rà FILE 02/03/05/06/08/11/Appendix                                 |
| 9           | Chạy Completion Report có evidence thật                            |
| 10          | Gateway vẫn BLOCKED cho đến khi pass và owner sign-off             |

**12\. KẾT LUẬN KHÓA V2.1**

Bản V2.1 là **Master Rule Lock nền** để viết lại tài liệu AI TÍCH HỢP CHANNEL sau test thực chiến.

Bản này đã khóa:

- Customer context first.
- No "hệ thống" customer-facing.
- Runtime-first pricing/member/commission.
- QuoteSnapshot là nguồn báo giá duy nhất.
- OrderConfirmationDraft dạng form prefill.
- Product Effectiveness bắt buộc.
- Product Recommendation 3 trụ + chăm sóc người thân.
- CRM 12 tháng.
- Member Lifecycle 12 tháng.
- Duy trì hạng 50%.
- Ân hạng 60 ngày.
- Proactive amount-based member care.
- Hạ hạng an toàn.
- CRM immutable text / trigger only / no duplicate.
- Priority conflict suppression.
- Cross-channel dedup 12 giờ.
- Member dispute escalation.
- Member lifecycle outcome logger.
- Affiliate/Diamond tách đại lý/mua sỉ.
- Live abuse hide/no reply/no Messenger.
- Gateway BLOCKED.
- FILE 04 NOT_STARTED.
- Production Readiness BLOCKED.

**Trạng thái cuối:**  
AI Advisor Channel chưa được gọi production-ready.  
Gateway chưa được mở.  
FILE 04 phải được viết trước khi triển khai runtime.
