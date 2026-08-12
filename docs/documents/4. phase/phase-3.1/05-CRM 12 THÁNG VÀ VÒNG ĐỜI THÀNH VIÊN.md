# 05 - CRM 12 THÁNG / VÒNG ĐỜI THÀNH VIÊN

## 1. Mục tiêu

File này khóa CRM 12 tháng và Member Lifecycle Automation cho PHASE-03.1.

CRM trong Phase 3.1 không phải spam khuyến mãi. CRM là chăm sóc vòng đời dựa trên dữ liệu thật: khách là ai, đã mua gì, dùng có hợp không, hạng nào, còn thiếu bao nhiêu để duy trì/nâng hạng, còn bao nhiêu ngày trong chu kỳ hoặc ân hạng.

Đây là nguồn triển khai chính cho workstream P3.1-2D.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | Timeline D0-M12, product triad, member lifecycle, variables, quiet hours, frequency cap, retry/fail, P0 tests. |
| `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | CRM là vòng đời, member proactive, priority conflict, outcome logger. |
| `5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Chăm sóc sau mua, New/Silver/Gold/Platinum/Diamond care. |
| `5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Trigger, channel, no duplicate, logs, bans. |

## 3. Nguyên tắc lõi

CRM phải:

- Chủ động chăm sóc.
- Không bịa dữ liệu.
- Không ép mua.
- Không làm khách bị dọa khi gần hạ hạng.
- Không dùng "đổi món để không ngán".
- Không gửi nếu suppression fail.
- Mọi gợi ý sản phẩm phải có Product Effectiveness.
- Mọi quote phải có QuoteSnapshot.
- Mọi quyền lợi thành viên phải qua runtime.

Khi runtime có dữ liệu, AI/CRM phải nói rõ:

- Hạng hiện tại.
- Doanh số tích lũy hợp lệ.
- Số cần mua thêm để duy trì.
- Số cần mua thêm để nâng hạng.
- Hạng tiếp theo.
- Ngày còn lại trong chu kỳ.
- Nếu đang ân hạng, số ngày còn lại trong ân hạng.

## 4. Product recommendation mặc định

CRM deep recommendation dùng cấu trúc:

1. 1 sản phẩm theo mùa.
2. 1 sản phẩm chức năng.
3. 1 sản phẩm bổ dưỡng.
4. 1 sản phẩm chăm sóc người thân theo giới/ngữ cảnh gia đình nếu phù hợp.

Mỗi item phải có:

- `product_public_name`.
- `eastern_effectiveness_summary`.
- `hero_ingredients`.
- `ingredient_synergy_effect`.
- `suitable_context`.
- `safety_boundary`.

### 4.1. Người mua là nữ có gia đình

Ưu tiên add-on:

- Sản phẩm cho trẻ/con nếu có dữ liệu.
- Sản phẩm cho ba mẹ/người lớn tuổi.
- Sản phẩm cho chồng/phái mạnh nếu có ngữ cảnh.

### 4.2. Người mua là nam

Giữ 3 trụ và add-on cho:

- Vợ/người phụ nữ trong gia đình.
- Dòng dưỡng mô/đẹp da/chăm sóc sắc vóc nếu Product Knowledge cho phép.
- Dòng bổ dưỡng nhẹ nhàng.

Không gợi ý sản phẩm nếu thiếu Product Knowledge hoặc sản phẩm non-sellable.

## 5. Timeline CRM 12 tháng

| Mốc | Automation | Mục tiêu | Guard bắt buộc |
| --- | --- | --- | --- |
| D0 | Post Purchase Welcome | Cảm ơn, xác nhận chăm sóc | CRMEligibility, Consent, FinalGuard |
| D1 | Usage Guide | Hướng dẫn dùng | ProductUsageData, ClaimGuard |
| D3 | First Experience Check | Hỏi trải nghiệm đầu | No sales if complaint |
| D7 | Taste/Fit Check | Hỏi hợp khẩu vị | TasteFeedback, QualitySignal |
| D14 | Reorder Soft Suggestion | Gợi ý mềm nếu feedback tích cực/trung tính | QuoteSnapshot if price |
| D21 | Next Product Suggestion | Gợi ý sản phẩm kế tiếp | ProductEffectiveness |
| D30 | First Month Review | Tổng kết tháng đầu | CustomerMemory |
| M2 | Next Need Care | Chăm nhu cầu mới | Need resolver |
| M3 | Family/Gift/Seasonal Care | Gia đình/quà/theo mùa | Gift/Family guard |
| M4 | Product Education | Giáo dục hệ sản phẩm | Claim whitelist |
| M5 | Growth Order Care | Gợi ý nhiều dòng nếu trust tốt | No pressure |
| M6 | Mid-cycle Review | Hỏi trải nghiệm và tiến độ member | MemberLifecycle |
| M7 | Member Lifecycle Care | Duy trì/nâng hạng | Runtime amount |
| M8 | Gift/Family Care | Gia đình/quà | Gift guard |
| M9 | Maintain/Upgrade Reminder | Nhắc số còn thiếu | No threat tone |
| M10 | Retention Event Care | Retention event | Suppression |
| M11 | Pre-cycle / Winback | Chăm trước hết chu kỳ | Lifecycle |
| M12 | Annual Review | Tổng kết 12 tháng | Outcome logger |

## 6. Member lifecycle event registry

| Event | Trigger | Branch / Output |
| --- | --- | --- |
| CRM-D0-001 | `order_code_created` | Post Purchase Welcome |
| CRM-D7-001 | D+7 sau delivered | Taste/Fit Check |
| CRM-D14-001 | D+14 + feedback positive/neutral | Reorder Soft Suggestion |
| CRM-D21-001 | D+21 sau delivered | Next Product Suggestion |
| CRM-M04-001 | Month 4 | Product triad + family add-on |
| MEM-M06-001 | Month 6 từ cycle_start | Maintain OK/Risk |
| MEM-M09-001 | Month 9 | Proactive maintain/upgrade amount |
| MEM-M11-001 | 30 ngày trước cycle_end | Pre-cycle end care |
| MEM-M12-001 | cycle_end_date | Upgrade/Maintain/Grace |
| MEM-GRACE-D01 | Ngày đầu grace | Grace Start |
| MEM-GRACE-D15 | Grace +15 | Grace Care |
| MEM-GRACE-D30 | Grace +30 | Grace Progress |
| MEM-GRACE-D45 | Grace +45 | Grace Reminder |
| MEM-GRACE-D55 | Grace +55 | Final Grace Reminder |
| MEM-GRACE-D60 | Grace +60 | Maintain success hoặc downgrade safe |

## 7. Member lifecycle rule

```text
cycle_length = 12 months
maintain_rule = đạt từ 50% chuẩn hạng hiện tại
grace_period = 60 days
upgrade_rule = đạt chuẩn hạng tiếp theo theo runtime
downgrade_rule = hết 60 ngày ân hạng vẫn chưa đạt duy trì thì hạ hạng theo runtime policy
```

Không được:

- Hạ hạng trước khi hết grace.
- Hứa giữ hạng khi runtime không xác nhận.
- Bịa amount hoặc days left.
- Dùng wording gây áp lực.

## 8. Biến runtime bắt buộc

- `member_tier_display`.
- `accumulated_valid_revenue_display`.
- `amount_to_maintain_tier_display`.
- `amount_to_next_tier_display`.
- `next_member_tier_display`.
- `days_to_cycle_end_display`.
- `grace_status`.
- `days_left_in_grace_display`.
- `previous_member_tier_display`.
- `new_member_tier_display`.

Không có biến thì không gửi content branch đó.

## 9. Quiet hours

Timezone: `Asia/Ho_Chi_Minh`.

Khung gửi:

- 08:30 - 11:30.
- 14:00 - 17:30.
- 19:00 - 20:45.

Trigger ngoài khung phải dời sang khung hợp lệ nếu policy cho phép. Không gửi lúc 22:00.

## 10. Frequency cap

| Window | Max |
| --- | ---: |
| Per day | 1 |
| Per 7 days | 2 |
| Per 30 days | 5 |

Grace exception:

- Tối đa 5 message trong 60 ngày grace.
- Chỉ tại D1, D15, D30, D45, D55.

## 11. Retry/fail policy

Retry tối đa 2 lần nếu:

- Provider timeout.
- Temporary rate limit.
- Transient network error.

Không retry nếu:

- Opt-out.
- Open case.
- Fatigue exceeded.
- Quiet hour without next window.
- ClaimGuard fail.
- FinalGuard fail.
- Missing runtime member data.
- Missing ProductEffectivenessBlock.

Fail action:

- Mark `CRM_EVENT_FAILED`.
- Log reason code.
- No unsafe fallback.

## 12. Priority conflict suppression

CRM sales/member message phải dừng nếu có:

- Complaint open.
- Refund open.
- Payment dispute.
- Privacy request.
- Health-sensitive unresolved.
- Product quality issue.
- Counterfeit suspicion.
- Recall/sale lock.

Open case thắng sales CRM.

## 13. Outcome logger

Phải log:

- Maintain OK.
- Maintain Risk.
- Grace Start.
- Grace Recovered.
- Upgrade Success.
- Downgrade Safe.
- Winback.
- Suppressed by open case.
- Failed by missing runtime.

Outcome log dùng để đo giữ hạng/nâng hạng và không được thay thế bằng delivery log đơn thuần.

## 14. Contract tối thiểu

| Contract | Fields |
| --- | --- |
| CRMCustomerLifecycleState | customer_id, stage, last_order, last_feedback, current_member_state, suppression_state |
| CRMMemberLifecycleEvent | event_id, customer_id, member_id, milestone, branch, scheduled_at, status |
| CRMProductRecommendationDecision | customer_id, context, selected_items, pillar_map, effect_summary, safety_boundary |
| CRMDeliveryEligibilityDecision | customer_id, event_id, allow_send, deny_reason, quiet_hour, frequency, dedup |
| MemberLifecycleOutcomeLog | customer_id, member_id, outcome, source_event, result, created_at |

## 15. P0 blocker

- CRM thiếu Customer Memory/Order History nhưng vẫn personalize.
- CRM bịa doanh số/hạng/ngày còn lại.
- Downgrade trước hết grace.
- Gợi ý sản phẩm thiếu Product Effectiveness.
- CRM quote thiếu QuoteSnapshot.
- Open case không suppress sales.
- Quiet hours/frequency cap bị bỏ qua.
- Outcome không log.

## 16. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2D-001 | Có đủ member runtime | Nói hạng, doanh số, số còn thiếu, ngày còn lại. |
| P3_1-2D-002 | Missing runtime | Không bịa, không gửi branch cần biến. |
| P3_1-2D-003 | M6 maintain risk | Nhắc mềm, không dọa. |
| P3_1-2D-004 | Cycle end chưa đạt | Grace 60 ngày. |
| P3_1-2D-005 | Grace D60 chưa đạt | Downgrade safe. |
| P3_1-2D-006 | CRM deep recommendation | Có 3 trụ + add-on nếu context. |
| P3_1-2D-007 | Trigger 22:00 | Dời lịch. |
| P3_1-2D-008 | Open refund case | Suppress sales CRM. |
| P3_1-2D-009 | Fatigue exceeded | Suppress. |
| P3_1-2D-010 | ProductEffectiveness missing | Không gửi recommendation. |

## 17. Evidence bắt buộc

- Customer lifecycle state.
- Member lifecycle snapshot.
- CRM event decision.
- Recommendation decision.
- Delivery eligibility.
- Quiet/frequency/suppression log.
- Outcome log.
- Smoke output.

## 18. Done gate

Workstream 05 DONE khi:

- CRM 12 tháng đầy đủ D0-M12.
- Member lifecycle maintain/grace/downgrade đầy đủ.
- Product triad đầy đủ.
- Quiet/frequency/retry/fail/suppression rõ.
- Outcome log có evidence.
- Không release claim.

## 19. Final status

`P3.1-2D HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`
