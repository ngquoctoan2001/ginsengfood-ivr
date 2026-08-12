**CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK**

**AI TÍCH HỢP CHANNEL / AI_ADVISOR_CHANNEL**

**Trạng thái:** DRAFT LOCK FOR OWNER REVIEW  
**Mục tiêu:** Khóa quy trình AI chăm sóc khách hàng 12 tháng, gợi ý sản phẩm theo cấu trúc chuẩn, chăm sóc thành viên, nâng hạng, duy trì hạng, ân hạng 60 ngày và hạ hạng an toàn.

**1\. Nguyên tắc lõi**

CRM không phải tin nhắn khuyến mãi. CRM là chăm sóc vòng đời khách hàng dựa trên dữ liệu thật: khách là ai, đã mua gì, mua cho ai, dùng có hợp không, đang ở hạng nào, còn thiếu bao nhiêu để duy trì/nâng hạng và đang còn bao nhiêu ngày trong chu kỳ/ân hạng. Tài liệu hiện tại đã khóa CRM phải dựa trên Customer Memory, Order History, Product Effectiveness Block, quyền lợi runtime và suppression policy; không được bịa lịch sử, hạng, quyền lợi hoặc dữ liệu mua hàng.

MUST:

- AI phải chủ động chăm sóc, không nói nặng kiểu "để Em kiểm tra rồi báo".
- Khi runtime đã có dữ liệu, AI phải nói rõ:
  - Mình đang ở hạng gì.
  - Đã tích lũy bao nhiêu.
  - Cần mua thêm khoảng bao nhiêu để duy trì hạng.
  - Cần mua thêm khoảng bao nhiêu để nâng lên hạng nào.
  - Còn bao nhiêu ngày trong chu kỳ.
  - Nếu đang ân hạng, còn bao nhiêu ngày ân hạng.
- Mọi gợi ý sản phẩm phải có hiệu dụng theo y thực đồng nguyên kết hợp kiến thức ẩm thực phương Đông.
- Mọi giá/quote phải đi qua QuoteSnapshot.
- Mọi quyền lợi thành viên phải đi qua RuntimeRightsResolver / MemberLifecycleResolver. FILE 07 hiện đã có block member upgrade/maintain yêu cầu resolver và guard tương ứng.

MUST NOT:

- Không bịa doanh số.
- Không bịa hạng.
- Không bịa ngày còn lại.
- Không tự tính nâng/duy trì/hạ hạng.
- Không nói hạ hạng theo kiểu gây áp lực.
- Không ép combo.
- Không dùng "đổi món để không ngán".
- Không gửi CRM nếu opt-out, open case, fatigue, quiet hour fail.

**2\. Product Recommendation Rule**

**3 Trụ + Chăm Sóc Người Thân**

Mỗi lần AI gợi ý sản phẩm trong CRM, member lifecycle, winback, quà sức khỏe hoặc chăm sóc gia đình, mặc định phải theo cấu trúc:

CRM_PRODUCT_RECOMMENDATION_DEFAULT:  
required:  
\- 01 sản phẩm theo mùa  
\- 01 sản phẩm chức năng  
\- 01 sản phẩm bổ dưỡng  
\- 01 sản phẩm chăm sóc người thân theo giới/ngữ cảnh gia đình  
<br/>every_item_must_include:  
\- product_public_name  
\- eastern_effectiveness_summary  
\- hero_ingredients  
\- ingredient_synergy_effect  
\- suitable_context  
\- safety_boundary

**2.1. Nếu người mua là nữ có gia đình**

AI gợi ý thêm sản phẩm chăm sóc:

- trẻ/con nếu có dữ liệu;
- ba mẹ/người lớn tuổi nếu có dữ liệu;
- chồng/phái mạnh nếu có ngữ cảnh gia đình.

FEMALE_FAMILY_BUYER_RULE:  
add_on_priority:  
\- child_care_product  
\- elderly_parent_product  
\- husband_male_product

Câu định hướng:

Dạ Em không chỉ gợi ý một dòng đơn lẻ, mà chọn theo 3 trụ của hệ Cháo Sâm Savigin: một dòng theo mùa, một dòng chức năng và một dòng bổ dưỡng. Vì Mình có ngữ cảnh gia đình, Em gợi ý thêm một dòng phù hợp để chăm sóc người thân trong nhà ạ.

**2.2. Nếu người mua là nam**

AI vẫn giữ:

- 01 sản phẩm theo mùa;
- 01 sản phẩm chức năng;
- 01 sản phẩm bổ dưỡng;

và gợi ý thêm sản phẩm cho vợ/người phụ nữ trong gia đình theo hướng dưỡng mô, đẹp da, bồi bổ nhẹ nhàng.

MALE_BUYER_RULE:  
add_on_priority:  
\- wife_female_care_product  
\- beauty_skin_tissue_product  
\- gentle_nourishing_product

Câu định hướng:

Dạ với Mình, Em vẫn chọn theo 3 trụ: một dòng theo mùa, một dòng chức năng và một dòng bổ dưỡng. Ngoài ra, nếu Mình muốn chọn thêm cho vợ/người phụ nữ trong gia đình, Em sẽ gợi ý dòng thiên về dưỡng mô, chăm sóc sắc vóc và bồi bổ nhẹ nhàng theo hướng thực dưỡng ạ.

**3\. CRM 12-Month Automation Timeline**

| **Mốc** | **Tên automation**        | **Mục tiêu**                             |
| ------- | ------------------------- | ---------------------------------------- |
| D0      | Post Purchase Welcome     | Cảm ơn, xác nhận chăm sóc                |
| D1      | Usage Guide               | Hướng dẫn dùng                           |
| D3      | First Experience Check    | Hỏi trải nghiệm đầu                      |
| D7      | Taste/Fit Check           | Hỏi hợp khẩu vị                          |
| D14     | Reorder Soft Suggestion   | Gợi ý duy trì nếu khách tích cực         |
| D21     | Next Product Suggestion   | Gợi ý sản phẩm kế tiếp                   |
| D30     | First Month Review        | Tổng kết tháng đầu                       |
| M2      | Next Need Care            | Gợi ý theo nhu cầu mới                   |
| M3      | Family/Gift/Seasonal Care | Gia đình/quà/theo mùa                    |
| M4      | Product Education         | Giáo dục hệ sản phẩm                     |
| M5      | Growth Order Care         | Gợi ý phương án nhiều dòng nếu trust tốt |
| M6      | Mid-cycle Review          | Hỏi lại trải nghiệm và tiến độ member    |
| M7      | Member Lifecycle Care     | Duy trì/nâng hạng                        |
| M8      | Gift/Family Care          | Quà sức khỏe/gia đình                    |
| M9      | Maintain/Upgrade Reminder | Nhắc chủ động số còn thiếu               |
| M10     | Retention Event Care      | Sự kiện/retention                        |
| M11     | Pre-cycle / Winback       | Chăm trước hết chu kỳ                    |
| M12     | Annual Review             | Tổng kết 12 tháng                        |

**4\. Member Lifecycle Rule**

MEMBER_LIFECYCLE_CORE:  
cycle_length: "12 months"  
maintain_rule: "Đạt từ 50% chuẩn hạng hiện tại"  
grace_period: "60 days"  
upgrade_rule: "Đạt chuẩn hạng tiếp theo theo runtime"  
downgrade_rule: "Hết 60 ngày ân hạng vẫn chưa đạt duy trì thì hạ hạng theo runtime policy"

MUST:

- Duy trì hạng = đạt từ 50% chuẩn hạng hiện tại.
- Nếu chưa đạt duy trì khi hết chu kỳ, khách được ân hạng 60 ngày.
- Trong 60 ngày ân hạng, nếu đạt lại 50% chuẩn hạng hiện tại thì duy trì hạng.
- Hết ân hạng 60 ngày mà vẫn chưa đạt thì hạ hạng theo runtime.
- AI phải nói chủ động số tiền còn thiếu nếu runtime đã có.

**5\. Member Runtime Variables**

MEMBER_PROACTIVE_AMOUNT_VARIABLES:  
required:  
\- member_tier_display  
\- accumulated_valid_revenue_display  
\- amount_to_maintain_tier_display  
\- amount_to_next_tier_display  
\- next_member_tier_display  
\- days_to_cycle_end_display  
\- grace_status  
\- days_left_in_grace_display

Không có dữ liệu runtime thì không được bịa.

**6\. Member Lifecycle Content Blocks**

**6.1. Duy trì hạng - chủ động**

Dạ hiện Mình đang là {{member_tier_display}} ạ.  
Doanh số tích lũy hợp lệ hiện tại của Mình là {{accumulated_valid_revenue_display}}.

Để duy trì hạng {{member_tier_display}} trong chu kỳ này, Mình cần thêm đơn hàng khoảng {{amount_to_maintain_tier_display}} nữa. Chu kỳ hiện tại còn {{days_to_cycle_end_display}}.

Nếu Mình muốn, Em gợi ý phương án sản phẩm vừa dùng thật sự phù hợp cho Mình/gia đình, vừa giúp Mình duy trì quyền lợi một cách tự nhiên ạ.

**6.2. Nâng hạng - chủ động**

Dạ hiện Mình đang là {{member_tier_display}}.  
Nếu Mình muốn nâng lên {{next_member_tier_display}}, Mình cần thêm đơn hàng khoảng {{amount_to_next_tier_display}} nữa trong chu kỳ hiện tại.

Em có thể gợi ý theo 3 trụ: một dòng theo mùa, một dòng chức năng, một dòng bổ dưỡng, và thêm một dòng chăm sóc người thân trong gia đình nếu phù hợp ạ.

**6.3. Ân hạng 60 ngày**

Dạ hiện Mình đang trong thời gian ân hạng của hạng {{member_tier_display}}.  
Thời gian ân hạng còn {{days_left_in_grace_display}}.

Để duy trì hạng hiện tại, Mình cần thêm đơn hàng khoảng {{amount_to_maintain_tier_display}} nữa. Em sẽ gợi ý phương án sản phẩm có giá trị sử dụng thật, không chỉ vì điểm/hạng ạ.

**6.4. Hạ hạng an toàn**

Dạ theo dữ liệu hiện tại, thời gian ân hạng 60 ngày của Mình đã kết thúc và điều kiện duy trì hạng {{previous_member_tier_display}} chưa được hoàn tất.

Quyền lợi thành viên của Mình sẽ được cập nhật theo hạng {{new_member_tier_display}} theo chính sách hiện hành. Khi Mình mua hàng trong chu kỳ mới, doanh số hợp lệ sẽ tiếp tục được ghi nhận để Mình có thể duy trì hoặc nâng hạng lại ạ.

Em vẫn sẽ tư vấn sản phẩm theo nhu cầu thật của Mình/gia đình, không để việc thay đổi hạng làm trải nghiệm của Mình bị nặng nề ạ.

**7\. Automation Event Registry**

CRM_MEMBER_AUTOMATION_EVENT_REGISTRY:  
CRM-D0-001:  
trigger: "order_code_created"  
content: "Post Purchase Welcome"  
guard:  
\- CRMEligibilityGuard  
\- ConsentPreferenceGuard  
\- FinalGuard  
<br/>CRM-D7-001:  
trigger: "D+7 after delivered"  
content: "Taste/Fit Check"  
<br/>CRM-D14-001:  
trigger: "D+14 after delivered + positive_or_neutral_feedback"  
content: "Reorder Soft Suggestion"  
<br/>CRM-D21-001:  
trigger: "D+21 after delivered"  
content: "Next Product Suggestion with Product Effectiveness"  
<br/>CRM-M04-001:  
trigger: "Month 4"  
content: "Product Triad + Family Add-on Recommendation"  
<br/>MEM-M06-001:  
trigger: "Month 6 from member cycle start"  
branch:  
if_progress_gte_50_percent: "Maintain OK"  
if_progress_lt_50_percent: "Maintain Risk"  
<br/>MEM-M09-001:  
trigger: "Month 9"  
content: "Proactive Maintain / Upgrade Amount"  
<br/>MEM-M11-001:  
trigger: "30 days before cycle end"  
content: "Pre-cycle End Care"  
<br/>MEM-M12-001:  
trigger: "cycle_end_date"  
branch:  
if_upgrade_eligible: "Upgrade Success"  
if_maintain_eligible: "Maintain Success"  
if_not_maintain: "Grace Start"  
<br/>MEM-GRACE-D01:  
trigger: "first day of grace period"  
content: "Grace Start"  
<br/>MEM-GRACE-D15:  
trigger: "15 days after grace start"  
content: "Grace Care"  
<br/>MEM-GRACE-D30:  
trigger: "30 days after grace start"  
content: "Grace Progress"  
<br/>MEM-GRACE-D45:  
trigger: "45 days after grace start"  
content: "Grace Reminder"  
<br/>MEM-GRACE-D55:  
trigger: "55 days after grace start"  
content: "Final Grace Reminder"  
<br/>MEM-GRACE-D60:  
trigger: "60 days after grace start"  
branch:  
if_maintain_eligible: "Maintain Success"  
if_not_maintain: "Downgrade Safe"

**8\. Quiet Hours / Frequency Cap**

CRM_MEMBER_QUIET_HOURS:  
timezone: Asia/Ho_Chi_Minh  
allowed_send_window:  
\- "08:30-11:30"  
\- "14:00-17:30"  
\- "19:00-20:45"  
<br/>CRM_MEMBER_FREQUENCY_CAP:  
max_per_day: 1  
max_per_7_days: 2  
max_per_30_days: 5  
<br/>grace_period_exception:  
max_per_60_days: 5  
allowed_milestones:  
\- D1  
\- D15  
\- D30  
\- D45  
\- D55

**9\. Retry / Fail Policy**

CRM_MEMBER_RETRY_FAIL_POLICY:  
max_retry: 2  
<br/>retry_if:  
\- provider_timeout  
\- temporary_rate_limit  
\- transient_network_error  
<br/>do_not_retry_if:  
\- opt_out  
\- open_case  
\- message_fatigue_exceeded  
\- quiet_hour_without_next_window  
\- ClaimGuard_FAIL  
\- FinalGuard_FAIL  
\- missing_runtime_member_data  
\- missing_ProductEffectivenessBlock  
<br/>fail_action:  
\- mark_CRM_EVENT_FAILED  
\- log_fail_reason_code  
\- no_unsafe_fallback

**10\. P0 Test Matrix**

| **Test ID**    | **Nhánh**             | **Điều kiện**                | **Kết quả**                                    |
| -------------- | --------------------- | ---------------------------- | ---------------------------------------------- |
| CRM-MEM-P0-001 | Runtime member        | Có đủ runtime                | AI nói hạng, tích lũy, còn thiếu, ngày còn lại |
| CRM-MEM-P0-002 | Missing runtime       | Thiếu dữ liệu                | Không bịa                                      |
| CRM-MEM-P0-003 | Maintain              | Đạt từ 50% chuẩn hạng        | Duy trì hạng                                   |
| CRM-MEM-P0-004 | Maintain risk         | Chưa đạt 50%                 | Nhắc mềm, không dọa                            |
| CRM-MEM-P0-005 | Upgrade               | Đạt chuẩn hạng tiếp theo     | Nói đủ điều kiện nâng hạng                     |
| CRM-MEM-P0-006 | Grace start           | Hết chu kỳ chưa đạt          | Ân hạng 60 ngày                                |
| CRM-MEM-P0-007 | Grace amount          | Đang ân hạng                 | Nói còn bao nhiêu ngày + còn thiếu bao nhiêu   |
| CRM-MEM-P0-008 | Grace success         | Đạt 50% trong ân hạn         | Duy trì hạng                                   |
| CRM-MEM-P0-009 | Downgrade             | Hết 60 ngày chưa đạt         | Hạ hạng an toàn                                |
| CRM-MEM-P0-010 | Product triad         | CRM gợi ý sâu                | Có 1 theo mùa + 1 chức năng + 1 bổ dưỡng       |
| CRM-MEM-P0-011 | Female family buyer   | Nữ có gia đình               | Có add-on trẻ/ba mẹ/chồng nếu context có       |
| CRM-MEM-P0-012 | Male buyer            | Nam                          | Có add-on cho vợ/nữ/dưỡng mô đẹp da            |
| CRM-MEM-P0-013 | Product Effectiveness | Gợi ý sản phẩm               | Có hiệu dụng từng dòng                         |
| CRM-MEM-P0-014 | Quote                 | Khách hỏi giá                | Phải có QuoteSnapshot                          |
| CRM-MEM-P0-015 | Opt-out               | Khách từ chối nhận tin       | Không gửi                                      |
| CRM-MEM-P0-016 | Open case             | Có khiếu nại/payment/privacy | Không gửi bán hàng                             |
| CRM-MEM-P0-017 | Quiet hour            | Trigger 22:00                | Dời sang khung hợp lệ                          |
| CRM-MEM-P0-018 | Frequency cap         | Vượt giới hạn                | Suppress                                       |
| CRM-MEM-P0-019 | Internal leak         | Có raw revenue/internal ID   | FinalGuard chặn                                |
| CRM-MEM-P0-020 | Tone downgrade        | Hạ hạng                      | Không làm khách xấu hổ                         |

**11\. Done Gate**

Block này đạt khi:

- Đã gộp CRM 12 tháng và Member Lifecycle thành một flow thống nhất.
- AI chủ động nói hạng, tích lũy, số cần mua thêm, hạng tiếp theo, ngày còn lại.
- Duy trì hạng = đạt từ 50% chuẩn hạng hiện tại.
- Ân hạng = 60 ngày.
- Hạ hạng chỉ sau khi hết ân hạng và runtime xác nhận.
- Gợi ý sản phẩm mặc định theo 3 trụ + chăm sóc người thân.
- Mọi gợi ý sản phẩm có Product Effectiveness.
- Mọi quote có QuoteSnapshot.
- Có automation event registry.
- Có quiet hours, frequency cap, retry/fail.
- Có test P0.
- Không bịa dữ liệu.
- Không ép mua.
- Không làm khách thấy bị dọa khi gần hạ hạng.

**Kết luận khóa:** phần này nên đưa vào bản tổng hợp sau test thực chiến, sau đó mapping vào **FILE 07 / FILE 09 / FILE 10 / FILE 12**, đồng thời FILE 04 phải triển khai rõ MemberLifecycleResolver, RuntimeRightsResolver, CRMProductRecommendationResolver, ProductTriadFamilyAddOnResolver, CRMEligibilityGuard, MessageFatigueGuard, FinalGuard.
