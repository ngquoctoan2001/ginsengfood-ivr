# 04 - TIN NHẮN GIỜ VÀNG / CRM / ĐA KÊNH

## 1. Mục tiêu

File này khóa hệ thống tin nhắn cho PHASE-03.1:

- Tin nhắn Giờ Vàng.
- Tin nhắn theo hạng thành viên.
- Diamond referral/commission.
- Birthday, Tết, order success, tier congrats.
- Multi-channel priority.
- Email rule.
- No duplicate.
- Immutable message text.
- Delivery audit.

Đây là nguồn triển khai chính cho workstream P3.1-2C.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | Golden Hour, Guest/New/Silver/Gold/Platinum/Diamond, Diamond referral, commission, post-purchase care. |
| `5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Trigger, immutable text, email rule, channel priority, variables, data conditions, logs, bans. |
| `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | CRM immutable text, trigger-only, no duplicate, no overspam, cross-channel dedup, priority conflict. |
| `5. bo sung/5. CRM 12-MONTH + MEMBER LIFECYCLE AUTOMATION LOCK.md` | Quiet hours, frequency cap, retry/fail, suppression. |

## 3. Nguyên tắc chung

- Tin nhắn là lớp giao tiếp theo hành vi người dùng.
- Mỗi tin nhắn phải gắn với trigger cụ thể.
- Không tự tạo trigger ngoài danh sách approved.
- Không gửi sai hạng.
- Không gửi trùng nội dung trong cùng tháng dương lịch.
- Nội dung tin nhắn đã duyệt không được rewrite/paraphrase.
- Chỉ render biến runtime được phép.
- Không gửi khi thiếu dữ liệu bắt buộc.
- Không gửi khi opt-out/open case/suppression/fatigue/quiet hour fail.

## 4. Message family chính

| Family | Mục tiêu | Trigger chính |
| --- | --- | --- |
| Tết | Tri ân năm mới | `campaign_scheduler_event` |
| Birthday | Chúc mừng sinh nhật | `BIRTHDAY_REWARD` |
| Golden Hour trước phiên | Nhắc phiên sắp bắt đầu | `golden_hour_start_time - configured_minutes` |
| Golden Hour quyền vào sớm | Chỉ gửi cho người có quyền | `early_access_window_opened` hoặc equivalent |
| Golden Hour bắt đầu | Thông báo phiên bắt đầu | `golden_hour_session_started` |
| New activation | Khách có mua/chưa đăng ký hoặc chưa đạt hạng | `spent_total_update_event`, `user_visit_without_register` |
| Tier Congrats | Chúc mừng nâng hạng | `tier_change_event` |
| Sold out session | Hết hàng trong phiên | `sku_remaining_in_session = 0` |
| Order Success | Mua thành công | `order_status = delivered` hoặc equivalent owner-approved |
| Diamond Referral | Giới thiệu thành công | `DIAMOND_REFERRAL` |
| Diamond Commission | Hoa hồng phát sinh | `DIAMOND_COMMISSION` |

Không cho phép tạo trigger phụ không có owner approval.

## 5. Tin nhắn Giờ Vàng

### 5.1. Đối tượng

- Guest.
- Khách đã mua nhưng chưa đăng ký.
- New.
- Silver.
- Gold.
- Platinum.
- Diamond.
- Buyer từ link Diamond hợp lệ.

Runtime phải tự xác định quyền mua sớm. Template không tự suy đoán.

### 5.2. Trước phiên

Guest/khách chưa đăng ký:

- Chỉ thông báo giờ bắt đầu.
- Không hiển thị quyền mua sớm.
- Có thể mời đăng ký tài khoản theo policy.

Member:

- Hiển thị hạng hiện tại.
- Chỉ hiển thị quyền mua sớm nếu runtime xác nhận.
- Không dùng placeholder số phút nếu thiếu dữ liệu.

### 5.3. Quyền vào sớm

Chỉ gửi cho người có quyền. Nếu không có `early_access_eligible = true`, không gửi.

### 5.4. Bắt đầu phiên / đang diễn ra

Chỉ nhắc mua nếu:

- Phiên đang active.
- SKU còn sellable/quota.
- Customer không bị suppression.
- Channel delivery hợp lệ.

Không tạo khan hiếm giả.

## 6. Tin nhắn Diamond

### 6.1. DIAMOND_REFERRAL

Điều kiện:

- Trigger `DIAMOND_REFERRAL`.
- Member tier = Diamond.
- Referral/buyer event hợp lệ.
- Snapshot: diamond_snapshot, tier_snapshot, link_snapshot, policy_version_id.

Không gửi cho hạng khác.

### 6.2. DIAMOND_COMMISSION

Điều kiện:

- Trigger `DIAMOND_COMMISSION`.
- Commission được ghi nhận thành công.
- Commission percent/amount từ runtime.
- Snapshot: diamond_snapshot, link_snapshot, commission_decision, policy_version_id.

Không gửi khi commission đang pending/dispute/hold.

## 7. Tin nhắn Birthday và Tết

### 7.1. Birthday

- Trigger: `BIRTHDAY_REWARD`.
- Chỉ gửi 1 lần/năm.
- Cần full name hoặc wording fallback owner-approved.
- Có thể dùng email nếu email hợp lệ và email rule cho phép.

### 7.2. Tết

- Trigger: `campaign_scheduler_event`.
- Theo lịch âm đã owner/config xác nhận.
- Không tự tạo campaign date trong code.

## 8. Message theo tier

| Tier | Nội dung được phép | Guard |
| --- | --- | --- |
| Guest | Giới thiệu sản phẩm, mời đăng ký, không hứa quyền lợi cá nhân. | Không bịa tên/hạng/doanh số. |
| New | Chăm sóc sau mua, mời đăng ký/kích hoạt hạng nếu runtime có doanh số. | Không bịa spent_total/amount_needed. |
| Silver | Nhắc quyền lợi hạng, đường lên Gold/Platinum/Diamond nếu runtime có amount. | Không dùng benefit ngoài runtime. |
| Gold | Nhắc nâng Platinum/Diamond nếu đủ dữ liệu. | Không ép mua. |
| Platinum | Nhắc Diamond/nâng hạng nếu phù hợp. | Không hứa commission khi chưa Diamond. |
| Diamond | Nhắc link, tài nguyên bán hàng, commission, Golden Hour nếu runtime eligible. | Không gửi sai platform rule. |

## 9. Channel priority

Thứ tự ưu tiên:

1. Zalo.
2. Messenger.
3. SMS.
4. Instagram.
5. Email.

Chỉ gửi trên channel khách đã từng tương tác hoặc xác thực. Không gửi đồng loạt tất cả channel. Cross-channel dedup bắt buộc.

## 10. Email rule

Email bắt buộc nếu:

- Event `ORDER_SUCCESS`.
- User có email hợp lệ.

Email được phép nếu:

- `BIRTHDAY_REWARD`.
- `TIER_CONGRATS`.

Email không gửi cho:

- SKU sold out.
- Quảng bá thường.
- Message Giờ Vàng nếu policy không cho phép.
- Nội dung không có email approval.

Không tồn tại rule email thứ hai song song.

## 11. Biến được phép

Template registry phải khai báo biến trước khi render:

- `full_name`.
- `spent_total`.
- `tier_name`.
- `current_tier_display_name`.
- `amount_needed`.
- `discount_percent`.
- `current_tier_discount_percent`.
- `early_minutes`.
- `current_tier_early_access_minutes`.
- `golden_hour_start_time`.
- `golden_hour_landing_link`.
- `quota_per_sku`.
- `voucher_amount`.
- `cash_amount`.
- `referral_link`.
- `commission_percent`.
- `policy_member_link`.
- `policy_diamond_link`.

Thiếu biến bắt buộc thì không gửi hoặc route fallback owner-approved. Không để placeholder trống ra khách.

## 12. No duplicate và frequency

- Không gửi trùng cùng message family trong cùng tháng dương lịch theo múi giờ Việt Nam.
- Không gửi quá giới hạn quiet/frequency của CRM lifecycle.
- Không retry khi opt-out/open case/FinalGuard fail/ClaimGuard fail/missing runtime.

## 13. Delivery log bắt buộc

Mỗi lần xét gửi phải lưu:

- customer_id hoặc user_id.
- message_family.
- template_id.
- trigger_id.
- channel.
- decision allow/deny.
- deny_reason.
- sent_at hoặc scheduled_at.
- status.
- error_code nếu có.
- correlation_id.
- policy_version.

## 14. Contract tối thiểu

| Contract | Fields |
| --- | --- |
| MessageTemplate | template_id, family, approved_text, approved_hash, variables, locale, status, policy_version |
| MessageTrigger | trigger_id, event_name, family, eligibility_rules, schedule_rules |
| DeliveryEligibilityDecision | customer_id, channel, trigger_id, allow_send, deny_reason, dedup_key |
| DeliveryLog | delivery_id, template_id, channel, status, sent_at, error_code, correlation_id |
| MessageSuppressionState | customer_id, opt_out, open_case, quiet_hour, fatigue, duplicate_status |

## 15. P0 blocker

- Gửi sai hạng.
- Rewrite/paraphrase text đã khóa.
- Tự tạo trigger phụ.
- Gửi trùng trong tháng.
- Gửi trên channel chưa xác thực.
- Email gửi tràn lan.
- Placeholder rỗng ra khách.
- Gửi khi opt-out/open case.
- Lộ PII/private data public.

## 16. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2C-001 | Guest trước Giờ Vàng | Không hiển thị early access. |
| P3_1-2C-002 | Silver có early access | Gửi đúng số phút nếu runtime confirmed. |
| P3_1-2C-003 | Silver thiếu runtime minute | Không gửi text có placeholder trống. |
| P3_1-2C-004 | Diamond commission pending | Không gửi DIAMOND_COMMISSION. |
| P3_1-2C-005 | Same family đã gửi tháng này | Suppress duplicate. |
| P3_1-2C-006 | ORDER_SUCCESS có email | Email required. |
| P3_1-2C-007 | SKU_SOLD_OUT có email | Không email. |
| P3_1-2C-008 | Opt-out | Không gửi mọi CRM marketing. |

## 17. Evidence bắt buộc

- Template registry snapshot.
- Trigger registry snapshot.
- Delivery decision log.
- Dedup log.
- Quiet hour/frequency/suppression log.
- Email rule evidence.
- Smoke output.

## 18. Done gate

Workstream 04 DONE khi:

- Tất cả message family trong source đã map.
- Text immutable có registry.
- Trigger-only enforce.
- Channel/email/no duplicate rõ.
- Delivery audit có evidence.
- Không release claim.

## 19. Final status

`P3.1-2C HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`
