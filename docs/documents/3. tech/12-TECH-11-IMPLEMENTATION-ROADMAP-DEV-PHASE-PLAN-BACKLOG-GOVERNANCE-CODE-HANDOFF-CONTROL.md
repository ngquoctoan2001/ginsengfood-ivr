# TECH-11 - IMPLEMENTATION ROADMAP DEV PHASE PLAN BACKLOG GOVERNANCE CODE HANDOFF CONTROL

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## DEV PHASE GOVERNANCE / SOURCE-OF-TRUTH TO IMPLEMENTATION / BACKLOG CONTROL / EVIDENCE-FIRST DELIVERY / NO-COPY-PASTE-CODE RULE

## PHẦN 1/4 - IMPLEMENTATION PRINCIPLES / PHASE BOUNDARY / NO-COPY-PASTE-CODE RULE / DEV HANDOFF GOVERNANCE

## 1. MỤC TIÊU CỦA TECH-11

TECH-11 là tài liệu chuyển tiếp từ bộ tài liệu kỹ thuật đã khóa sang kế hoạch triển khai dev thực tế.

TECH-11 không thay thế TECH-00 -> TECH-10.

TECH-11 không viết lại nghiệp vụ của từng TECH.

TECH-11 không viết code.

TECH-11 không thiết kế API chi tiết.

TECH-11 không thiết kế database chi tiết.

TECH-11 không thiết kế UI chi tiết.

TECH-11 có nhiệm vụ khóa:

Thứ tự triển khai dev.

Nguyên tắc chia phase.

Ranh giới từng phase.

Cách lập backlog.

Cách giao việc cho dev/Codex/Copilot.

Cách nhận báo cáo sau mỗi phase.

Cách kiểm soát evidence.

Cách kiểm soát smoke.

Cách tránh dev tự suy luận.

Cách tránh copy-paste code rời rạc từ AI.

Cách đảm bảo source-of-truth mới thắng tài liệu cũ và code cũ.

Cách đưa từng phase vào Global Gateway theo TECH-10.

Mục tiêu cuối cùng của TECH-11 là biến bộ TECH source-of-truth thành lộ trình triển khai có kiểm soát, không phải thành một tập code để copy dán.

## 2. VAI TRÒ CỦA TECH-11 TRONG BỘ TÀI LIỆU

TECH-11 đứng sau:

## TECH-00 - Global Technical Governance.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-02 - Product / SKU / Recipe / Product Activation.

## TECH-03 - Operational Core.

## TECH-04 - Commerce Runtime.

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway.

## TECH-07 - Ads Measurement.

## TECH-08 - MC AI Live.

## TECH-09 - IVR Order Confirmation.

## TECH-10 - Global Smoke / UAT / Evidence / Release Gateway.

TECH-11 có vai trò:

Tổng hợp thứ tự triển khai.

Chia phase dev.

Kiểm soát backlog.

Kiểm soát handoff.

Kiểm soát báo cáo dev.

Kiểm soát evidence theo từng phase.

Kiểm soát smoke theo từng phase.

Kiểm soát không nhảy phase.

Kiểm soát không code rời rạc.

Kiểm soát không gọi dev done là release ready.

## 3. NGUYÊN TẮC GREENFIELD IMPLEMENTATION

## 3.1. TECH mới là source-of-truth

Trong triển khai greenfield:

TECH-00 -> TECH-11 là source-of-truth.

Tài liệu cũ chỉ dùng tham chiếu.

Code cũ chỉ dùng để biết hiện trạng.

Nếu code cũ khác tài liệu mới, tài liệu mới thắng.

Nếu tài liệu cũ khác tài liệu mới, tài liệu mới thắng.

Nếu dev thấy mâu thuẫn, phải báo điểm chặn, không tự chọn theo cảm tính.

Không được chỉnh sửa tạm code cũ để hợp thức hóa logic cũ nếu logic mới đã khóa khác.

## 3.2. Không triển khai kiểu chỉnh sửa tạm lẻ

TECH-11 cấm cách làm:

Đụng đâu sửa đó.

Gặp lỗi nào chỉnh sửa tạm lỗi đó.

Thấy thiếu gì thêm tạm.

Copy code từ AI rồi dán vào.

Ghép các đoạn code rời rạc thành hệ thống.

Bỏ qua state machine.

Bỏ qua evidence.

Bỏ qua audit.

Bỏ qua smoke.

Bỏ qua source-of-truth upstream.

## 3.3. Greenfield không có nghĩa bỏ hết tư duy đã khóa

Greenfield không có nghĩa làm lại tùy ý.

Greenfield nghĩa là:

Không bị ràng buộc bởi tài liệu cũ sai.

Không bị ràng buộc bởi code cũ sai.

Triển khai theo source-of-truth mới.

Thiết kế lại đúng boundary.

Giữ đúng dependency.

Giữ đúng gate.

Giữ đúng evidence.

Giữ đúng smoke.

Giữ đúng release governance.

Không để dev tự suy luận nghiệp vụ.

## 4. NO-COPY-PASTE-CODE RULE

## 4.1. Không xây hệ thống bằng code AI rời rạc

Một hệ thống phần mềm thực tế không thể xây bằng cách:

Yêu cầu AI viết vài đoạn code.

Copy code.

Dán vào project.

Hy vọng hệ thống chạy đúng.

Sửa lỗi phát sinh bằng các đoạn code tiếp theo.

Ghép nhiều đoạn code không cùng kiến trúc.

Đây là cách làm sai bản chất phát triển phần mềm.

## 4.2. Vì sao copy-paste code từ AI không đủ

Code rời rạc từ AI thường không có đầy đủ:

Context toàn hệ thống.

Kiến trúc thật của project.

Database schema thật.

Domain model thật.

State machine thật.

Quy tắc nghiệp vụ thật.

Quyền truy cập thật.

Audit/evidence thật.

Idempotency thật.

Error handling thật.

Runtime dependency thật.

Security boundary thật.

Performance constraint thật.

Release/rollback thật.

Test/smoke thật.

Vì vậy, copy code có thể làm một chức năng nhỏ chạy thử, nhưng không tạo được hệ thống vận hành ổn định.

## 4.3. Ví dụ bắt buộc: chức năng đăng nhập

Người không kỹ thuật thường nghĩ đăng nhập chỉ là:

Form nhập số điện thoại/email.

Form nhập mật khẩu.

API login.

Trả về token.

Nhưng thực tế đăng nhập cần:

Thiết kế bảng user.

Thiết kế account status.

Mã hóa mật khẩu.

Chính sách reset mật khẩu.

Session/token.

Refresh token nếu có.

RBAC/permission.

Chống brute force.

Chống SQL injection.

Audit login.

Lock account.

Logout.

Token expiry.

Device/session management.

Error message an toàn.

Privacy/security review.

Test pass/fail.

Smoke và evidence.

Không có chuyện copy một đoạn code login là xong.

Hệ thống Ginsengfood còn phức tạp hơn nhiều vì có:

Sản phẩm.

Công thức.

Sản xuất.

Kho.

Truy xuất.

Recall.

Commerce.

Payment.

AI Advisor.

Facebook Gateway.

Ads.

MC AI Live.

## IVR.

Global Release Gateway.

Do đó, TECH-11 khóa nguyên tắc:

Không giao dev bằng các đoạn code rời rạc.

Chỉ giao dev bằng phase, backlog, requirement, boundary, evidence và smoke.

## 5. PHÂN VAI TRÒ ĐÚNG: OWNER MUỐN GÌ, DEV LÀM NHƯ THẾ NÀO

## 5.1. Vai trò của owner/người dùng

Owner/người dùng quyết định:

Hệ thống cần làm gì.

Quy trình vận hành thực tế ra sao.

Chức năng nào bắt buộc.

Quy tắc nghiệp vụ nào đúng.

Trường hợp nào phải chặn.

Trường hợp nào cần người duyệt.

Trạng thái nào là pass.

Trạng thái nào là fail.

Evidence nào phải có.

Smoke nào phải chạy.

Khi nào được release.

Khi nào không được release.

## 5.2. Vai trò của developer

Developer quyết định:

Thiết kế kỹ thuật chi tiết.

Cấu trúc code.

Cấu trúc module.

Database schema.

## API.

## UI.

Worker/job.

Integration.

Test automation.

Build/deploy.

Migration.

Performance.

Security implementation.

DevOps/monitoring.

Cách triển khai tối ưu trong codebase thật.

## 5.3. AI hỗ trợ gì

AI được dùng để hỗ trợ:

Viết tài liệu source-of-truth.

Chia phase triển khai.

Viết checklist cho dev.

Viết prompt giao Codex/Copilot.

Rà báo cáo dev.

Soát smoke/evidence.

Phát hiện thiếu boundary.

Phát hiện P0 điểm chặn.

Gợi ý thứ tự triển khai.

Chuẩn hóa báo cáo handoff.

AI không thay thế developer.

AI không tự tạo hệ thống hoàn chỉnh để copy-paste.

AI không được biến thành nguồn code rời rạc thiếu context.

## 6. IMPLEMENTATION PHASE BOUNDARY

## 6.1. Nguyên tắc chia phase

Mỗi phase phải có:

Mục tiêu rõ.

Source-of-truth rõ.

Module scope rõ.

Scope In rõ.

Scope Out rõ.

Upstream dependency rõ.

Downstream handoff rõ.

P0 điểm chặn rõ.

Evidence required rõ.

Smoke required rõ.

Done Gate rõ.

Fail Gate rõ.

Handoff rõ.

## 6.2. Không được nhảy phase

Dev không được triển khai phase downstream khi phase upstream critical chưa có evidence/smoke tối thiểu.

Ví dụ:

Không làm Commerce bán hàng khi Product/SKU/Operational Sellable Gate chưa rõ.

Không làm AI báo giá khi Commerce QuoteSnapshot chưa có.

Không làm Ads ROAS khi Verified Revenue chưa có.

Không làm MC AI Live khi Gateway/AI/Commerce chưa pass.

Không làm IVR thật khi Official Order/Core Order State chưa pass.

Không mở Global Gateway khi evidence/smoke/sign-off/release decision chưa đủ.

## 6.3. Có thể song song nhưng không được bypass

Một số việc có thể làm song song ở mức chuẩn bị:

Chuẩn bị tài liệu.

Chuẩn bị backlog.

Chuẩn bị mock.

Chuẩn bị test case.

Chuẩn bị UI wireframe.

Chuẩn bị environment.

Chuẩn bị seed data.

Nhưng không được release downstream nếu upstream chưa pass.

## 7. DEV PHASE REGISTRY - ĐỀ XUẤT KHÓA BAN ĐẦU

## 7.1. PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency

Mục tiêu:

Xây nền quyền.

Xây audit.

Xây evidence.

Xây idempotency.

Xây owner/action trace.

Xây trạng thái nền để các phase sau có điểm bám.

Không có PHASE 0 thì các phase sau không đủ kiểm soát.

## 7.2. PHASE 1 - Product / SKU / Recipe / Product Activation

Mục tiêu:

Product Master.

## SKU.

Recipe.

Formula kind/version.

Product Activation Guard.

Claim/product data approved.

Không cho Product Active bị hiểu là Sellable.

## 7.3. PHASE 2 - Operational Core

Mục tiêu:

Production.

Warehouse.

Inventory.

Traceability.

Recall.

Sale Lock.

Raw lot readiness.

Batch release.

Finished goods stock.

Public trace whitelist.

## 7.4. PHASE 3 - Commerce Runtime

Mục tiêu:

Sellable Gate.

QuoteSnapshot.

Cart.

Order Draft.

Official Order.

Payment boundary.

Shipping boundary.

Verified Revenue.

Member/Diamond/program runtime benefit.

## 7.5. PHASE 4 - AI Advisor Runtime

Mục tiêu:

Product consulting.

Customer memory.

Quote-safe sales.

Order Draft handoff.

Final Response Guard.

Claim guard.

Public/private answer control.

Không tự tính giá, không tự tạo official order, không tự xác nhận payment.

## 7.6. PHASE 5 - Facebook Gateway / Messenger / Channel Delivery

Mục tiêu:

Comment-to-Messenger.

Public/private routing.

Delivery guard.

Typing indicator/response pacing.

Rate limit.

Moderation.

Suppression.

Không gửi response chưa guard.

## 7.7. PHASE 6 - Ads Measurement

Mục tiêu:

Attribution.

Pixel/CAPI dedup.

## CPA.

## AOV.

## ROAS.

Verified Revenue.

Data Quality.

Scale Gate.

Không dùng quote/cart/order draft/unpaid order làm revenue.

## 7.8. PHASE 7 - MC AI Live

Mục tiêu:

Live script runtime.

Host intelligence.

Ads-safe live orchestration.

Live comment handling.

Messenger routing.

Troll/complaint separation.

Không dùng live signal làm ROAS.

Không tự báo giá/tạo order/payment/revenue.

## 7.9. PHASE 8 - IVR Order Confirmation

Mục tiêu:

IVR eligibility.

New/untrusted customer verification.

Official Order only.

Invalid phone.

No answer.

Customer cancel.

Customer need support.

Technical failure.

Notification Owner handoff.

Privacy-safe call result.

## 7.10. PHASE 9 - Global Smoke / UAT / Release Gateway

Mục tiêu:

Evidence accepted.

Smoke pass.

Cross-tech smoke.

## UAT.

Owner sign-off.

Release decision.

Global Gateway.

Post-go-live monitoring.

Scale approval control.

## 8. PHASE ENTRY GATE

Mỗi phase chỉ được bắt đầu khi có đủ:

Source-of-truth tương ứng.

Scope rõ.

Upstream dependency rõ.

Backlog rõ.

Dev owner rõ.

QA owner rõ.

Evidence requirement rõ.

Smoke requirement rõ.

P0 điểm chặn rõ.

Handoff từ phase trước nếu có.

Nếu thiếu một trong các điểm trên, phase chưa được coi là ready để dev triển khai.

## 9. PHASE EXIT GATE

Một phase chỉ được coi là hoàn tất ở cấp dev khi có đủ:

Dev implementation report.

File đã sửa.

Evidence đã nộp.

Build/test result.

Smoke result.

Known issues.

P0 điểm chặn status.

Migration/seed status nếu có.

Cleanup status.

Markdown/docs update nếu có.

Handoff sang phase tiếp theo.

Owner/QA review nếu required.

Dev nói “đã làm xong” không đủ để phase exit.

Phase exit không đồng nghĩa release ready.

## 10. BACKLOG GOVERNANCE PRINCIPLE

## 10.1. Backlog phải bám source-of-truth

Mọi backlog item phải gắn với:

TECH number.

Section.

Requirement.

Phase.

Owner.

Priority.

Dependency.

Evidence.

Smoke.

Done Gate.

## 10.2. Không có backlog mơ hồ

Không tạo backlog kiểu:

“Làm AI.”

“Làm kho.”

“Làm bán hàng.”

“Làm Ads.”

“Làm IVR.”

“Sửa cho chạy.”

“Tối ưu lại.”

“Làm giống tài liệu.”

Backlog phải cụ thể.

Ví dụ đúng:

“PHASE 3 - Implement QuoteSnapshot as only final price source for AI/Gateway/Live.”

“PHASE 8 - Ensure IVR only accepts Official Order, not Quote/Cart/Order Draft.”

“PHASE 6 - Exclude unpaid order, payment WAITING, COD WAITING from Verified Revenue.”

“PHASE 5 - Public comment price intent routes to Messenger; no private data in public reply.”

## 10.3. Backlog không được vượt phase

Backlog downstream không được move Ready nếu upstream điểm chặn chưa closed.

Ví dụ:

Ads scale backlog không Ready nếu Verified Revenue chưa pass.

AI quote backlog không Ready nếu Commerce QuoteSnapshot chưa pass.

IVR call backlog không Ready nếu Official Order/Core Order State chưa pass.

MC AI Live backlog không Ready nếu Gateway/AI/Commerce chưa pass.

## 11. DEV HANDOFF GOVERNANCE

## 11.1. Handoff cho dev phải có cấu trúc

Mỗi handoff cho dev phải có:

Tên phase.

Tên module.

Mục tiêu.

Source-of-truth.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Report format.

Không viết code rời rạc.

Không hardcode policy.

Không bypass source-of-truth.

## 11.2. Không giao việc kiểu “code giúp chức năng này”

Không giao dev/Codex kiểu:

“Viết code đăng nhập.”

“Viết code bán hàng.”

“Viết code AI.”

“Viết code IVR.”

“Viết code Ads.”

“Copy đoạn này vào dự án.”

“Làm nhanh cho chạy.”

Giao đúng phải là:

“Phân tích source-of-truth.”

“Đối chiếu codebase hiện tại.”

“Đề xuất implementation plan.”

“Chỉ ra file cần sửa.”

“Triển khai trong kiến trúc thật.”

“Chạy test/build.”

“Nộp evidence.”

“Báo cáo theo format.”

“Không tự thay đổi nghiệp vụ.”

## 11.3. Dev không được tự hardcode policy

Các chính sách không được hardcode tùy tiện:

Giá.

Khuyến mãi.

Member benefit.

Diamond benefit.

Golden Hour.

24/7.

Shipping.

Payment account.

Commission.

IVR attempts.

Notification content.

Privacy rules.

Sale Lock.

Recall.

Scale Gate.

Nếu policy chưa có runtime/config/source-of-truth, dev phải báo điểm chặn.

## 12. CODE HANDOFF CONTROL

## 12.1. Khi nào mới được yêu cầu code

Chỉ yêu cầu code khi đã có:

Phase rõ.

Requirement rõ.

Scope rõ.

Architecture context rõ.

Database impact rõ hoặc xác định chưa cần DB.

API impact rõ hoặc xác định chưa cần API.

UI impact rõ hoặc xác định chưa cần UI.

Test expectation rõ.

Evidence expectation rõ.

Rollback/fallback nếu high-risk.

## 12.2. Code phải đi trong codebase thật

Code không được viết như mảnh rời.

Code phải đi theo:

Project structure thật.

Naming convention thật.

Existing module pattern.

Existing dependency injection pattern.

Existing error handling.

Existing logging/audit pattern.

Existing test framework.

Existing build pipeline.

Existing security/RBAC model.

Existing environment config.

## 12.3. Code không được phá source-of-truth

Dev không được:

Tự đổi nghiệp vụ.

Tự bỏ gate.

Tự hardcode policy.

Tự bypass audit.

Tự bỏ evidence.

Tự bỏ smoke.

Tự gọi runtime unavailable là pass.

Tự dùng code cũ làm chuẩn nếu khác TECH mới.

Tự release khi chưa qua TECH-10.

## 13. IMPLEMENTATION REPORT FORMAT

Sau mỗi phase, dev phải báo cáo theo format cố định:

## 13.1. Tóm tắt

Nêu ngắn gọn:

Phase nào.

Module nào.

Đã làm gì.

Chưa làm gì.

Trạng thái hiện tại.

## 13.2. File đã sửa

Liệt kê:

File backend.

File frontend.

File config.

File migration.

File seed.

File docs.

File test.

Nếu không sửa loại file nào thì ghi rõ không áp dụng.

## 13.3. Nguồn yêu cầu

Phải trích:

TECH number.

Section.

Requirement.

Smoke ID nếu có.

## 13.4. Evidence đã dùng

Liệt kê evidence:

Log.

Screenshot.

Test output.

Build output.

Migration output.

Seed validation.

Audit/correlation id nếu có.

## 13.5. Lệnh đã chạy

Ghi rõ các lệnh:

Build.

Test.

Lint.

Migration.

Seed.

Typecheck.

Smoke script nếu có.

## 13.6. Kết quả test

Nêu:

Test pass.

Test fail.

Test chưa chạy.

Lý do chưa chạy.

Retest cần thiết.

## 13.7. Kết quả backend build

Ghi:

Pass/fail.

Lỗi nếu có.

Warning nếu có.

Impact.

## 13.8. Kết quả frontend build

Ghi:

Pass/fail.

Lỗi nếu có.

Warning nếu có.

Impact.

## 13.9. Kết quả cleanup process

Ghi:

File rác đã xóa.

Code thừa đã bỏ.

Duplicate đã xử lý.

Comment/debug tạm đã gỡ.

Chưa cleanup gì thì nêu rõ.

## 13.10. Cập nhật Markdown

Ghi:

Tài liệu nào đã cập nhật.

README nào đã cập nhật.

Handoff nào đã cập nhật.

Chưa cập nhật thì nêu lý do.

## 13.11. Database migration/update nếu áp dụng

Ghi:

Migration name.

Trạng thái chạy.

Rollback migration nếu có.

Data impact.

Seed impact.

## 13.12. Seed validation nếu áp dụng

Ghi:

Seed file.

Seed data.

Validation result.

Missing seed.

Conflict seed.

## 13.13. Rủi ro còn lại

Ghi rõ:

Known issues.

Open điểm chặn.

Risk.

Technical debt.

Dependency chưa pass.

Smoke chưa chạy.

Evidence thiếu.

## 13.14. Cập nhật handoff

Ghi:

Phase tiếp theo.

Module tiếp theo.

Điều kiện để tiếp tục.

điểm chặn cần xử lý trước.

Owner cần review.

## 14. IMPLEMENTATION P0 điểm chặn REGISTRY - PHẦN 1/4

Các lỗi sau là P0 điểm chặn:

Dev triển khai bằng code copy-paste rời rạc.

Dev tự sửa nghiệp vụ khác source-of-truth.

Dev dùng tài liệu cũ để phủ định TECH mới.

Dev dùng code cũ để phủ định TECH mới.

Dev hardcode giá/khuyến mãi/member/Diamond/shipping/payment/IVR policy.

Dev bỏ audit/evidence cho high-risk action.

Dev bỏ smoke.

Dev nhảy phase downstream khi upstream bị chặn.

Dev gọi implementation done là release ready.

Dev báo xong nhưng không có file list.

Dev báo xong nhưng không có lệnh đã chạy.

Dev báo xong nhưng không có test/build result.

Dev báo xong nhưng không có evidence.

Dev tự mở Gateway.

Dev tự bỏ P0 điểm chặn.

Dev đóng điểm chặn không retest.

Dev không cập nhật handoff.

Dev triển khai policy không có config/runtime/source-of-truth.

Dev dùng local/dev evidence để tuyên bố production ready.

Dev không báo rủi ro còn lại.

## 15. EVIDENCE YÊU CẦU CHO PHẦN 1/4

## PHẦN 1/4 yêu cầu evidence ở cấp tài liệu gồm:

Implementation Principles đã khóa.

Greenfield Implementation Rule đã khóa.

No-Copy-Paste-Code Rule đã khóa.

Owner/Dev role boundary đã khóa.

Phase Boundary đã khóa.

Dev Phase Registry đã có.

Phase Entry Gate đã có.

Phase Exit Gate đã có.

Backlog Governance đã có.

Dev Handoff Governance đã có.

Code Handoff Control đã có.

Implementation Report Format đã có.

P0 điểm chặn Registry đã có.

Release handoff sang PHẦN 2/4 đã rõ.

## 16. SMOKE ĐỊNH HƯỚNG - PHẦN 1/4

TECH11-P1-SMK-001 - Không Copy-Paste Code

Given dev nhận yêu cầu triển khai
When dev đề xuất copy code rời rạc từ AI
Then handoff phải bị reject và yêu cầu quay lại phase/source-of-truth.

TECH11-P1-SMK-002 - Không Nhảy Phase

Given PHASE 3 Commerce cần Operational Sellable Gate
When PHASE 2 chưa pass dependency
Then PHASE 3 không được release-ready.

TECH11-P1-SMK-003 - Backlog Phải Gắn Source-of-Truth

Given backlog item không có TECH/section/requirement
When backlog review chạy
Then backlog không được đưa vào Ready.

TECH11-P1-SMK-004 - Dev Report Thiếu Evidence

Given dev báo đã xong nhưng không có evidence/test/build result
When phase exit review chạy
Then phase chưa được exit.

TECH11-P1-SMK-005 - Code Cũ Khác TECH Mới

Given current implementation khác TECH source-of-truth
When dev review
Then TECH mới thắng, code cũ chỉ là hiện trạng tham chiếu.

TECH11-P1-SMK-006 - Documentation Complete Không Phải Release Ready

Given phase tài liệu đã xong
When release status được xét
Then không được gọi Release Ready nếu chưa có evidence/smoke/sign-off.

## 17. DONE GATE - PHẦN 1/4

## PHẦN 1/4 đạt Documentation Complete ở cấp nguyên tắc khi:

Đã khóa mục tiêu TECH-11.

Đã khóa vai trò TECH-11 trong bộ TECH.

Đã khóa Greenfield Implementation Rule.

Đã khóa No-Copy-Paste-Code Rule.

Đã khóa phân vai Owner/Dev/AI.

Đã khóa Implementation Phase Boundary.

Đã khóa Dev Phase Registry ban đầu.

Đã khóa Phase Entry Gate.

Đã khóa Phase Exit Gate.

Đã khóa Backlog Governance.

Đã khóa Dev Handoff Governance.

Đã khóa Code Handoff Control.

Đã khóa Implementation Report Format.

Đã khóa P0 điểm chặn Registry.

Đã có evidence requirement.

Đã có smoke định hướng.

Đã có release handoff sang PHẦN 2/4.

## 18. FAIL GATE - PHẦN 1/4

## PHẦN 1/4 FAIL nếu:

Cho phép dev copy-paste code rời rạc từ AI.

Cho phép dev tự suy luận nghiệp vụ.

Cho phép dùng tài liệu cũ làm source-of-truth.

Cho phép code cũ thắng TECH mới.

Không có phase boundary.

Không có phase entry gate.

Không có phase exit gate.

Không có backlog governance.

Không có dev handoff governance.

Không có report format.

Không có evidence requirement.

Không có smoke requirement.

Không có P0 điểm chặn.

Gọi dev done là release ready.

Gọi documentation complete là production ready.

Cho phép nhảy phase khi upstream bị chặn.

## 19. RELEASE HANDOFF - SANG PHẦN 2/4

## PHẦN 1/4 bàn giao sang PHẦN 2/4 các nguyên tắc đã khóa:

TECH-11 là tài liệu kiểm soát implementation roadmap.

Không code trong TECH-11.

Không copy-paste code rời rạc.

Không để dev tự suy luận.

Source-of-truth mới thắng tài liệu cũ và code cũ.

Owner quyết định muốn gì.

Dev quyết định làm như thế nào.

AI hỗ trợ tài liệu/checklist/prompt/review, không thay dev.

Dev phase phải có entry/exit gate.

Backlog phải gắn TECH/requirement/evidence/smoke.

Handoff phải rõ Scope In/Scope Out/Dependency/P0/Evidence/Smoke.

Dev report phải theo format cố định.

Không phase nào release-ready nếu upstream bị chặn.

Global Gateway vẫn bị chặn nếu chưa đủ TECH-10 gate.

## PHẦN 2/4 sẽ viết:

## DEV PHASE MODULE CONTRACTS / BACKLOG CONTROL / IMPLEMENTATION REPORT CONTRACTS

Các module cần khóa ở PHẦN 2/4:

Implementation Roadmap Orchestrator.

Phase Entry Gate Resolver.

Phase Exit Gate Resolver.

Backlog Governance Resolver.

Source-of-Truth Mapping Resolver.

Dev Handoff Resolver.

Code Handoff Control Resolver.

Implementation Report Resolver.

Evidence Mapping Resolver.

Smoke Mapping Resolver.

Cross-Phase Dependency Validator.

Risk / điểm chặn / Retest Resolver.

Cleanup / Documentation Update Resolver.

Dev-to-QA Handoff Resolver.

Release Handoff Resolver.

## 20. TRẠNG THÁI SAU PHẦN 1/4

Sau PHẦN 1/4:

Chưa được gọi là Documentation Complete.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa nguyên tắc triển khai cho TECH-11.

Các điểm cốt lõi đã được cố định:

TECH-11 là tài liệu chuyển từ source-of-truth sang dev implementation roadmap.

TECH-11 không viết code.

TECH-11 không thay thế TECH-00 -> TECH-10.

Greenfield nghĩa là tài liệu mới thắng tài liệu cũ và code cũ.

Không triển khai bằng copy-paste code rời rạc từ AI.

Không giao việc cho dev kiểu “viết code cho chức năng này” khi chưa có phase/scope/evidence/smoke.

Owner quyết định muốn gì.

Dev quyết định làm như thế nào.

AI hỗ trợ tài liệu, checklist, prompt, review, không thay dev.

Triển khai phải chia phase.

Không được nhảy phase downstream khi upstream critical chưa pass.

Backlog phải gắn TECH/section/requirement/evidence/smoke.

Dev handoff phải có Scope In/Scope Out/Dependency/P0/Evidence/Smoke.

Code handoff chỉ được thực hiện trong codebase thật, theo kiến trúc thật.

Dev report phải theo format cố định.

Dev done không phải Release Ready.

Documentation Complete không phải Production Ready.

Global Gateway vẫn bị chặn nếu chưa đủ evidence/smoke/sign-off/release decision.

## PHẦN 1/4 sẵn sàng bàn giao sang:

## PHẦN 2/4 - DEV PHASE MODULE CONTRACTS / BACKLOG CONTROL / IMPLEMENTATION REPORT CONTRACTS.

## PHẦN 2/4 - DEV PHASE MODULE CONTRACTS / BACKLOG CONTROL / IMPLEMENTATION REPORT CONTRACTS

## 1. MỤC TIÊU PHẦN 2/4

## PHẦN 2/4 khóa các module quản trị triển khai dev cho hệ thống Ginsengfood.

Mục tiêu là để khi chuyển từ tài liệu TECH sang triển khai thực tế, đội dev không làm theo cảm tính, không nhảy phase, không copy-paste code rời rạc và không tự suy luận nghiệp vụ.

## PHẦN 2/4 xác định rõ:

Ai điều phối implementation roadmap.

Phase nào được bắt đầu.

Phase nào được kết thúc.

Backlog được tạo và kiểm soát thế nào.

Mỗi backlog item phải gắn source-of-truth ra sao.

Dev nhận handoff theo chuẩn nào.

Khi nào mới được phép đi vào code.

Dev báo cáo kết quả theo chuẩn nào.

Evidence mapping ra sao.

Smoke mapping ra sao.

Cross-phase dependency được kiểm tra thế nào.

điểm chặn và retest được xử lý ra sao.

Cleanup và documentation update được kiểm soát thế nào.

Dev bàn giao QA thế nào.

Phase bàn giao sang TECH-10 Release Gateway thế nào.

## PHẦN 2/4 không viết code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế database chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa module contract cho quá trình triển khai.

## 2. NGUYÊN TẮC MODULE CONTRACT CHUNG

Mọi module trong TECH-11 phải tuân thủ các nguyên tắc sau:

Không thay thế TECH-00 -> TECH-10.

Không tự tạo source-of-truth mới.

Không dùng tài liệu cũ làm nền triển khai.

Không dùng code cũ làm chuẩn nghiệp vụ nếu khác tài liệu mới.

Không cho dev nhảy phase khi upstream chưa pass.

Không cho backlog mơ hồ vào trạng thái Ready.

Không giao việc kiểu “viết code cho chạy”.

Không copy-paste code rời rạc từ AI.

Không hardcode policy.

Không bỏ qua evidence.

Không bỏ qua smoke.

Không bỏ qua build/test.

Không bỏ qua cleanup.

Không bỏ qua documentation update nếu có ảnh hưởng.

Không gọi dev done là Release Ready.

Không gọi implementation complete là Production Ready.

Không mở Global Gateway nếu chưa qua TECH-10.

Không đóng điểm chặn nếu chưa retest.

Không để QA/owner tự suy luận tiêu chí pass/fail.

Không để phase downstream release khi upstream bị chặn.

## 3. MODULE CONTRACT 01 - IMPLEMENTATION ROADMAP ORCHESTRATOR

## 3.1. Mục tiêu

Implementation Roadmap Orchestrator là module điều phối tổng thể kế hoạch triển khai dev theo phase.

Module này giữ trật tự triển khai từ PHASE 0 đến PHASE 9, đảm bảo dev không đi tắt, không nhảy phase và không triển khai downstream khi upstream chưa đủ điều kiện.

## 3.2. Scope In

Module này được phép:

Xác định danh sách phase triển khai.

Sắp xếp thứ tự phase.

Gắn mỗi phase với TECH source-of-truth.

Xác định upstream/downstream dependency.

Xác định phase nào có thể chuẩn bị song song.

Xác định phase nào bị chặn.

Kiểm tra Entry Gate của từng phase.

Kiểm tra Exit Gate của từng phase.

Tổng hợp trạng thái phase.

Bàn giao phase đã đủ điều kiện sang TECH-10 Release Governance.

## 3.3. Scope Out

Module này không được:

Tự viết code.

Tự sửa nghiệp vụ.

Tự bỏ qua dependency.

Tự mở phase downstream khi upstream bị chặn.

Tự gọi phase complete là release ready.

Tự mở Global Gateway.

Tự hardcode policy.

Tự thay owner sign-off.

Tự chấp nhận evidence chưa review.

Tự bỏ qua P0 điểm chặn.

## 3.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-10.

Phase Registry.

Backlog Governance Resolver.

Source-of-Truth Mapping Resolver.

Cross-Phase Dependency Validator.

P0 điểm chặn Registry.

TECH-10 Global Gateway.

## 3.5. Downstream Handoff

Bàn giao cho:

Phase Entry Gate Resolver.

Dev Handoff Resolver.

Phase Exit Gate Resolver.

Release Handoff Resolver.

TECH-10 Global Release Orchestrator.

## 3.6. P0 điểm chặn

FAIL nếu roadmap cho phép PHASE 3 Commerce release khi PHASE 2 Operational Sellable Gate chưa pass.

FAIL nếu roadmap cho phép PHASE 6 Ads scale khi Verified Revenue chưa pass.

FAIL nếu roadmap cho phép PHASE 8 IVR chạy thật khi Official Order/Core Order State chưa pass.

FAIL nếu roadmap gọi dev implementation done là Release Ready.

## 3.7. Evidence

Evidence tối thiểu:

Phase list.

Phase order.

Source-of-truth mapping.

Dependency map.

Entry gate status.

Exit gate status.

điểm chặn list.

Handoff record.

Audit/review note.

## 3.8. Smoke

## TECH11-MOD-SMK-001

Given PHASE 3 cần PHASE 2 pass
When PHASE 2 còn bị chặn
Then PHASE 3 không được chuyển Release Ready.

## 4. MODULE CONTRACT 02 - PHASE ENTRY GATE RESOLVER

## 4.1. Mục tiêu

Phase Entry Gate Resolver quyết định một phase đã đủ điều kiện để bắt đầu dev implementation hay chưa.

## 4.2. Scope In

Module này được phép kiểm tra:

Phase có source-of-truth chưa.

Scope phase có rõ chưa.

Scope In có rõ chưa.

Scope Out có rõ chưa.

Upstream dependency có rõ chưa.

Backlog đã rõ chưa.

Dev owner đã có chưa.

QA owner đã có chưa.

Evidence requirement đã có chưa.

Smoke requirement đã có chưa.

P0 điểm chặn đã có chưa.

Handoff từ phase trước đã có chưa.

## 4.3. Scope Out

Module này không được:

Cho phase bắt đầu khi chưa có source-of-truth.

Cho phase bắt đầu khi scope mơ hồ.

Cho phase bắt đầu khi upstream critical chưa pass.

Cho phase bắt đầu chỉ vì dev rảnh.

Cho phase bắt đầu bằng prompt code rời rạc.

Bỏ qua QA/evidence/smoke requirement.

Tự tạo backlog không gắn TECH.

Tự đổi phase order.

## 4.4. Upstream Dependency

Phụ thuộc:

TECH source-of-truth.

Implementation Roadmap.

Backlog Governance.

Cross-Phase Dependency Validator.

Dev/QA owner assignment.

Phase previous handoff nếu có.

## 4.5. Downstream Handoff

Bàn giao:

## PHASE_ENTRY_READY.

## PHASE_ENTRY_BLOCKED.

Missing requirement list.

Upstream điểm chặn list.

Handoff sang Dev Handoff Resolver.

## 4.6. P0 điểm chặn

FAIL nếu phase bắt đầu khi không có evidence/smoke requirement.

FAIL nếu phase bắt đầu khi backlog không gắn TECH/section/requirement.

FAIL nếu phase downstream bắt đầu implementation thật khi upstream critical bị chặn.

## 4.7. Evidence

Evidence tối thiểu:

Phase ID.

Source-of-truth reference.

Scope checklist.

Dependency checklist.

Backlog checklist.

Owner assignment.

Evidence checklist.

Smoke checklist.

Entry decision.

Reviewer.

## 4.8. Smoke

## TECH11-MOD-SMK-002

Given phase chưa có QA owner và smoke requirement
When Entry Gate kiểm tra
Then phase không được Ready.

## 5. MODULE CONTRACT 03 - PHASE EXIT GATE RESOLVER

## 5.1. Mục tiêu

Phase Exit Gate Resolver quyết định một phase đã hoàn tất ở cấp dev hay chưa.

Phase exit không đồng nghĩa Release Ready.

Phase exit chỉ có nghĩa phase đã có implementation report, test/build/evidence/smoke cơ bản và handoff đúng format.

## 5.2. Scope In

Module này được phép kiểm tra:

Dev implementation report.

File đã sửa.

Source-of-truth đã tham chiếu.

Evidence đã nộp.

Lệnh đã chạy.

Test result.

Backend build result.

Frontend build result.

Migration result nếu có.

Seed validation nếu có.

Cleanup status.

Markdown/docs update.

Smoke result.

Open điểm chặn.

Handoff sang phase tiếp theo.

## 5.3. Scope Out

Module này không được:

Cho phase exit chỉ bằng câu “đã xong”.

Cho phase exit khi không có file list.

Cho phase exit khi không có test/build result.

Cho phase exit khi không có evidence.

Cho phase exit khi P0 điểm chặn còn mở.

Gọi phase exit là Release Ready.

Gọi phase exit là Production Ready.

Tự bỏ qua retest khi có fail.

Tự mở phase downstream release.

## 5.4. Upstream Dependency

Phụ thuộc:

Dev Handoff.

Implementation Report.

Evidence Mapping.

Smoke Mapping.

Build/test output.

Risk/điểm chặn Resolver.

Cleanup/Documentation Update Resolver.

## 5.5. Downstream Handoff

Bàn giao:

## PHASE_EXIT_ACCEPTED.

## PHASE_EXIT_REJECTED.

## PHASE_EXIT_NEED_MORE_EVIDENCE.

## PHASE_EXIT_RETEST_REQUIRED.

Handoff sang QA hoặc phase tiếp theo.

Handoff sang TECH-10 nếu đủ điều kiện release review.

## 5.6. P0 điểm chặn

FAIL nếu dev report thiếu test/build result vẫn được phase exit.

FAIL nếu migration fail nhưng phase exit.

FAIL nếu P0 điểm chặn còn open nhưng phase exit.

FAIL nếu phase exit bị gọi là production ready.

## 5.7. Evidence

Evidence tối thiểu:

Implementation report.

File list.

Commands run.

Test output.

Build output.

Evidence links/logs.

Smoke output.

điểm chặn status.

Handoff note.

Reviewer decision.

## 5.8. Smoke

## TECH11-MOD-SMK-003

Given dev báo hoàn tất nhưng không có lệnh đã chạy
When Phase Exit Gate kiểm tra
Then phase exit bị reject.

## 6. MODULE CONTRACT 04 - BACKLOG GOVERNANCE RESOLVER

## 6.1. Mục tiêu

Backlog Governance Resolver kiểm soát mọi backlog item trước khi đưa vào triển khai.

Module này đảm bảo backlog không mơ hồ, không tự suy luận, không vượt phase và luôn gắn source-of-truth.

## 6.2. Scope In

Module này được phép kiểm tra:

Backlog item có TECH reference không.

Có section/requirement không.

Có phase không.

Có owner không.

Có priority không.

Có dependency không.

Có Scope In/Scope Out không.

Có evidence requirement không.

Có smoke requirement không.

Có Done Gate không.

Có Fail Gate không.

Có risk/P0 điểm chặn không.

Có handoff target không.

## 6.3. Scope Out

Module này không được:

Cho backlog mơ hồ vào Ready.

Cho backlog “làm cho chạy” vào Ready.

Cho backlog không gắn TECH vào Ready.

Cho backlog downstream Ready khi upstream bị chặn.

Cho backlog hardcode policy.

Cho backlog bỏ evidence/smoke.

Cho backlog không owner.

Tự đổi priority bỏ qua dependency.

## 6.4. Upstream Dependency

Phụ thuộc:

TECH source-of-truth.

Phase Registry.

Dependency Validator.

Owner Registry.

P0 điểm chặn Registry.

## 6.5. Downstream Handoff

Bàn giao:

## BACKLOG_READY.

## BACKLOG_BLOCKED.

## BACKLOG_NEEDS_CLARIFICATION.

## BACKLOG_REJECTED.

Handoff sang Dev Handoff Resolver.

## 6.6. P0 điểm chặn

FAIL nếu backlog “Làm AI Advisor” không chia requirement cụ thể mà vẫn Ready.

FAIL nếu backlog “Sửa cho chạy” vẫn đưa vào dev.

FAIL nếu backlog không có evidence/smoke vẫn Ready.

## 6.7. Evidence

Evidence tối thiểu:

Backlog ID.

TECH reference.

Requirement reference.

Phase.

Owner.

Dependency.

Evidence/smoke requirement.

Ready/bị chặn decision.

Review note.

## 6.8. Smoke

## TECH11-MOD-SMK-004

Given backlog không có TECH/section/requirement
When Backlog Governance kiểm tra
Then backlog không được Ready.

## 7. MODULE CONTRACT 05 - SOURCE-OF-TRUTH MAPPING RESOLVER

## 7.1. Mục tiêu

Source-of-Truth Mapping Resolver đảm bảo mọi việc dev làm đều map về đúng TECH source-of-truth.

Module này ngăn việc dev dùng tài liệu cũ hoặc code cũ để phủ định tài liệu mới.

## 7.2. Scope In

Module này được phép:

Map backlog item với TECH.

Map requirement với section.

Map phase với TECH.

Map smoke với requirement.

Map evidence với requirement.

Xác định source-of-truth active.

Đánh dấu tài liệu cũ là LEGACY_REFERENCE_ONLY.

Đánh dấu code cũ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Phát hiện mâu thuẫn giữa code cũ và TECH mới.

Yêu cầu điểm chặn khi source conflict.

## 7.3. Scope Out

Module này không được:

Cho tài liệu cũ thắng TECH mới.

Cho code cũ thắng TECH mới.

Tự sửa nghiệp vụ.

Tự chọn requirement theo cảm tính.

Cho requirement không rõ source đi vào dev.

Cho source conflict bị bỏ qua.

Dùng legacy reference như production truth.

## 7.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-11.

Legacy document registry.

Current implementation review.

Backlog Governance.

Evidence Mapping.

## 7.5. Downstream Handoff

Bàn giao:

Source mapping accepted.

Source conflict list.

Legacy reference note.

Current implementation deviation list.

Handoff sang Dev Handoff Resolver.

## 7.6. P0 điểm chặn

FAIL nếu dev dùng tài liệu cũ để bỏ rule mới.

FAIL nếu dev giữ code cũ sai logic vì “đang chạy”.

FAIL nếu source conflict không báo điểm chặn.

## 7.7. Evidence

Evidence tối thiểu:

Requirement ID.

TECH reference.

Section reference.

Legacy conflict nếu có.

Code deviation nếu có.

Decision.

Reviewer.

## 7.8. Smoke

## TECH11-MOD-SMK-005

Given code cũ cho phép AI tự tính giá nhưng TECH mới yêu cầu QuoteSnapshot
When mapping resolver kiểm tra
Then TECH mới thắng và code cũ bị ghi deviation.

## 8. MODULE CONTRACT 06 - DEV HANDOFF RESOLVER

## 8.1. Mục tiêu

Dev Handoff Resolver chuẩn hóa nội dung giao việc cho dev/Codex/Copilot.

Module này đảm bảo dev nhận đủ context trước khi triển khai.

## 8.2. Scope In

Một dev handoff hợp lệ phải có:

Phase.

Module.

Mục tiêu.

Source-of-truth.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Report format.

Không hardcode policy.

Không copy-paste code rời rạc.

Không thay đổi nghiệp vụ.

Không bỏ qua TECH-10 gate.

## 8.3. Scope Out

Module này không được:

Giao việc bằng một câu mơ hồ.

Giao việc kiểu “viết code cho chạy”.

Giao việc không có source-of-truth.

Giao việc không có scope.

Giao việc không có P0 điểm chặn.

Giao việc không có evidence/smoke.

Giao việc downstream khi upstream bị chặn.

Giao việc hardcode policy.

## 8.4. Upstream Dependency

Phụ thuộc:

Phase Entry Gate.

Backlog Governance.

Source-of-Truth Mapping.

Cross-Phase Dependency Validator.

Owner assignment.

## 8.5. Downstream Handoff

Bàn giao:

## DEV_HANDOFF_READY.

## DEV_HANDOFF_BLOCKED.

Dev task package.

Report requirement.

Evidence/smoke requirement.

## 8.6. P0 điểm chặn

FAIL nếu handoff không có source-of-truth.

FAIL nếu handoff không nói Scope Out.

FAIL nếu handoff không có P0 điểm chặn.

FAIL nếu handoff yêu cầu copy code rời rạc.

## 8.7. Evidence

Evidence tối thiểu:

Handoff document.

Phase ID.

Backlog IDs.

Source references.

Dependency check.

Dev owner.

QA owner.

Review decision.

## 8.8. Smoke

## TECH11-MOD-SMK-006

Given handoff chỉ ghi “làm Commerce”
When Dev Handoff Resolver kiểm tra
Then handoff bị reject.

## 9. MODULE CONTRACT 07 - CODE HANDOFF CONTROL RESOLVER

## 9.1. Mục tiêu

Code Handoff Control Resolver kiểm soát khi nào mới được chuyển từ requirement sang code implementation.

Module này không viết code, mà khóa điều kiện trước khi cho phép dev/coding agent thực hiện code trong codebase thật.

## 9.2. Scope In

Module này được phép kiểm tra:

Phase đã Entry Ready chưa.

Requirement đã rõ chưa.

Source-of-truth đã map chưa.

Architecture context đã có chưa.

Database impact đã xác định chưa.

API impact đã xác định chưa.

UI impact đã xác định chưa.

Test expectation đã rõ chưa.

Evidence expectation đã rõ chưa.

Rollback/fallback có cần không.

Dev đã hiểu không hardcode policy chưa.

Coding agent prompt đã đủ guard chưa.

## 9.3. Scope Out

Module này không được:

Cho code nếu requirement mơ hồ.

Cho code nếu source-of-truth chưa rõ.

Cho code nếu upstream dependency bị chặn.

Cho code bằng đoạn snippet rời rạc.

Cho copy-paste code AI không qua project structure.

Cho code hardcode policy.

Cho code bỏ audit/evidence.

Cho code không test/build expectation.

## 9.4. Upstream Dependency

Phụ thuộc:

Dev Handoff.

Source-of-Truth Mapping.

Phase Entry Gate.

Backlog Governance.

Cross-Phase Dependency.

TECH-01 audit/evidence principles.

## 9.5. Downstream Handoff

Bàn giao:

## CODE_HANDOFF_ALLOWED.

## CODE_HANDOFF_BLOCKED.

Code implementation constraints.

Required test/build/evidence.

Required implementation report.

## 9.6. P0 điểm chặn

FAIL nếu coding agent được yêu cầu “viết code dán vào” mà không có project context.

FAIL nếu dev tạo code bypass audit/evidence.

FAIL nếu code hardcode giá/khuyến mãi/IVR policy.

## 9.7. Evidence

Evidence tối thiểu:

Code handoff checklist.

Requirement mapping.

Architecture impact note.

Test expectation.

Evidence expectation.

Approval to implement.

Reviewer.

## 9.8. Smoke

## TECH11-MOD-SMK-007

Given requirement chưa xác định DB/API/UI impact
When Code Handoff kiểm tra
Then không cho phép chuyển sang code implementation.

## 10. MODULE CONTRACT 08 - IMPLEMENTATION REPORT RESOLVER

## 10.1. Mục tiêu

Implementation Report Resolver kiểm tra báo cáo sau khi dev triển khai.

Module này đảm bảo dev không báo cáo mơ hồ và không giấu rủi ro.

## 10.2. Scope In

Module này kiểm tra báo cáo có đủ:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown.

Database migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

## 10.3. Scope Out

Module này không được:

Chấp nhận báo cáo “đã xong”.

Chấp nhận thiếu file list.

Chấp nhận thiếu test/build.

Chấp nhận thiếu evidence.

Chấp nhận che giấu risk.

Chấp nhận bỏ qua migration/seed nếu có ảnh hưởng.

Gọi báo cáo dev là release ready.

Đóng phase khi báo cáo thiếu phần bắt buộc.

## 10.4. Upstream Dependency

Phụ thuộc:

Dev implementation output.

Test/build output.

Evidence Mapping.

Smoke Mapping.

Cleanup Resolver.

Risk/điểm chặn Resolver.

## 10.5. Downstream Handoff

Bàn giao:

## REPORT_ACCEPTED.

## REPORT_REJECTED.

## REPORT_NEED_MORE_INFO.

Report gap list.

Handoff sang Phase Exit Gate Resolver.

## 10.6. P0 điểm chặn

FAIL nếu báo cáo thiếu test/build result vẫn accepted.

FAIL nếu báo cáo thiếu rủi ro còn lại nhưng có known issue.

FAIL nếu báo cáo không có nguồn yêu cầu.

## 10.7. Evidence

Evidence tối thiểu:

Report document.

Required section checklist.

Missing section list.

Evidence links.

Test/build references.

Reviewer decision.

## 10.8. Smoke

## TECH11-MOD-SMK-008

Given report thiếu “Nguồn yêu cầu” và “Lệnh đã chạy”
When resolver kiểm tra
Then report không accepted.

## 11. MODULE CONTRACT 09 - EVIDENCE MAPPING RESOLVER

## 11.1. Mục tiêu

Evidence Mapping Resolver đảm bảo evidence của dev map đúng requirement, phase, smoke và environment.

## 11.2. Scope In

Module này được phép kiểm tra:

Evidence có TECH reference không.

Có phase không.

Có backlog item không.

Có requirement không.

Có environment không.

Có expected/actual không.

Có timestamp không.

Có reviewer không.

Có log/screenshot/export phù hợp không.

Có sensitive data violation không.

Có audit/correlation id nếu flow runtime không.

Có pass/fail rõ không.

## 11.3. Scope Out

Module này không được:

Accepted evidence không trace requirement.

Accepted evidence không environment.

Accepted evidence không reviewer.

Accepted evidence chứa secret.

Accepted evidence local để production ready.

Dùng lời nói thay evidence.

Dùng ảnh chụp không rõ context làm evidence chính.

## 11.4. Upstream Dependency

Phụ thuộc:

Implementation Report.

Test/build output.

Smoke Mapping.

TECH-10 Evidence Boundary.

Privacy/Security requirement.

## 11.5. Downstream Handoff

Bàn giao:

Evidence mapped.

Evidence rejected.

Evidence need more info.

Evidence gap list.

Handoff sang Phase Exit Gate và TECH-10.

## 11.6. P0 điểm chặn

FAIL nếu evidence không gắn requirement.

FAIL nếu evidence chứa dữ liệu nhạy cảm sai policy.

FAIL nếu evidence thiếu environment.

## 11.7. Evidence

Evidence tối thiểu:

Evidence ID.

Requirement mapping.

Environment.

Expected/actual.

Reviewer.

Decision.

Audit note.

## 11.8. Smoke

## TECH11-MOD-SMK-009

Given evidence là screenshot không rõ test case
When Evidence Mapping kiểm tra
Then evidence không được accepted.

## 12. MODULE CONTRACT 10 - SMOKE MAPPING RESOLVER

## 12.1. Mục tiêu

Smoke Mapping Resolver đảm bảo mỗi phase/backlog có smoke tương ứng và smoke có evidence.

## 12.2. Scope In

Module này được phép kiểm tra:

Smoke ID.

TECH reference.

Phase reference.

Backlog item.

Scenario.

Given/When/Then.

Must Not.

Expected result.

Actual result.

Evidence reference.

Pass/fail.

Retest status nếu có.

## 12.3. Scope Out

Module này không được:

Cho phase exit khi smoke bắt buộc chưa chạy.

Cho smoke pass thiếu evidence.

Cho smoke chỉ happy path nếu requirement có fail-case.

Bỏ qua runtime unavailable smoke.

Bỏ qua privacy smoke.

Bỏ qua cross-phase smoke.

Gọi unit test là smoke nếu không cover scenario.

## 12.4. Upstream Dependency

Phụ thuộc:

Backlog Governance.

Phase requirement.

Implementation output.

Evidence Mapping.

QA plan.

## 12.5. Downstream Handoff

Bàn giao:

Smoke mapped.

Smoke missing.

Smoke pass.

Smoke fail.

Retest required.

Handoff sang Phase Exit và TECH-10.

## 12.6. P0 điểm chặn

FAIL nếu smoke P0 chưa chạy mà phase exit.

FAIL nếu smoke fail nhưng report accepted như done.

FAIL nếu smoke không có evidence.

## 12.7. Evidence

Evidence tối thiểu:

Smoke ID.

Scenario.

Expected/actual.

Evidence.

Tester.

Reviewer.

Pass/fail.

Retest note.

## 12.8. Smoke

## TECH11-MOD-SMK-010

Given PHASE 8 IVR đã code nhưng chưa smoke invalid phone/no answer/customer cancel
When Smoke Mapping kiểm tra
Then phase chưa được exit.

## 13. MODULE CONTRACT 11 - CROSS-PHASE DEPENDENCY VALIDATOR

## 13.1. Mục tiêu

Cross-Phase Dependency Validator kiểm tra dependency giữa các phase implementation.

Module này đảm bảo phase downstream không vượt phase upstream.

## 13.2. Scope In

Module này kiểm tra các dependency chính:

PHASE 0 -> tất cả phase.

## PHASE 1 -> PHASE 2.

## PHASE 2 -> PHASE 3.

## PHASE 3 -> PHASE 4.

## PHASE 4 -> PHASE 5.

## PHASE 5 -> PHASE 7.

## PHASE 3 -> PHASE 6.

## PHASE 3 -> PHASE 8.

PHASE 9 -> release/go-live/scale.

Sale Lock / Recall -> toàn bộ downstream.

## 13.3. Scope Out

Module này không được:

Cho Commerce bán khi Operational chưa pass.

Cho AI quote khi Commerce QuoteSnapshot chưa pass.

Cho Ads scale khi Verified Revenue chưa pass.

Cho MC Live go-live khi Gateway chưa pass.

Cho IVR gọi thật khi Core Order State chưa pass.

Cho Release Ready khi TECH-10 gate chưa pass.

Cho downstream dùng mock như production truth.

## 13.4. Upstream Dependency

Phụ thuộc:

Phase status.

Evidence status.

Smoke status.

điểm chặn status.

Source-of-truth mapping.

## 13.5. Downstream Handoff

Bàn giao:

Dependency pass.

Dependency fail.

bị chặn phase list.

Required upstream fix.

Handoff sang Roadmap Orchestrator.

## 13.6. P0 điểm chặn

FAIL nếu phase downstream release khi upstream critical chưa exit.

FAIL nếu AI/Gateway/Ads/Live/IVR chạy thật với mock upstream.

FAIL nếu dependency fail bị ghi thành warning nhẹ.

## 13.7. Evidence

Evidence tối thiểu:

Dependency pair.

Upstream status.

Downstream status.

Validation result.

Block reason.

Reviewer.

## 13.8. Smoke

## TECH11-MOD-SMK-011

Given Verified Revenue chưa pass
When PHASE 6 Ads Scale backlog xét Ready
Then backlog bị bị chặn.

## 14. MODULE CONTRACT 12 - RISK / điểm chặn / RETEST RESOLVER

## 14.1. Mục tiêu

Risk / điểm chặn / Retest Resolver kiểm soát rủi ro, điểm chặn và retest trong quá trình triển khai.

## 14.2. Scope In

Module này được phép:

Ghi nhận điểm chặn.

Phân loại P0/P1/P2.

Gắn điểm chặn với phase.

Gắn điểm chặn với owner.

Gắn điểm chặn với evidence.

Gắn điểm chặn với smoke fail.

Theo dõi fix.

Yêu cầu retest.

Đóng điểm chặn khi đủ evidence.

Reopen điểm chặn nếu lỗi tái xuất hiện.

Block phase exit nếu P0 còn mở.

## 14.3. Scope Out

Module này không được:

Đóng điểm chặn bằng miệng.

Đóng điểm chặn không retest.

Ẩn điểm chặn khỏi report.

Hạ cấp P0 tùy tiện.

Cho phase exit khi P0 điểm chặn mở.

Cho release khi điểm chặn chưa closed.

Bỏ qua privacy/security điểm chặn.

Bỏ qua data/revenue/payment điểm chặn.

## 14.4. Upstream Dependency

Phụ thuộc:

Smoke Mapping.

Evidence Mapping.

Implementation Report.

QA feedback.

Owner feedback.

## 14.5. Downstream Handoff

Bàn giao:

Open điểm chặn list.

Resolved điểm chặn list.

Retest required list.

Phase block status.

Handoff sang Phase Exit Gate và TECH-10.

## 14.6. P0 điểm chặn

FAIL nếu P0 điểm chặn đóng không retest.

FAIL nếu điểm chặn mở nhưng phase exit.

FAIL nếu điểm chặn bị giấu khỏi handoff.

## 14.7. Evidence

Evidence tối thiểu:

điểm chặn ID.

Phase.

Description.

Severity.

Owner.

Fix evidence.

Retest evidence.

Closure decision.

Audit note.

## 14.8. Smoke

## TECH11-MOD-SMK-012

Given P0 điểm chặn đã fix nhưng chưa retest
When điểm chặn resolver kiểm tra
Then điểm chặn không được CLOSED.

## 15. MODULE CONTRACT 13 - CLEANUP / DOCUMENTATION UPDATE RESOLVER

## 15.1. Mục tiêu

Cleanup / Documentation Update Resolver đảm bảo sau triển khai không để lại code rác, file tạm, debug, duplicate, tài liệu lệch implementation hoặc handoff thiếu cập nhật.

## 15.2. Scope In

Module này kiểm tra:

File rác.

Code tạm.

Debug log tạm.

Duplicate logic.

Dead code.

Comment sai.

Unused config.

Unused seed.

Docs cần cập nhật.

README cần cập nhật.

Handoff cần cập nhật.

Migration/seed note cần cập nhật.

## 15.3. Scope Out

Module này không được:

Xóa audit/evidence.

Xóa migration cần thiết.

Xóa log kiểm thử cần lưu.

Sửa tài liệu source-of-truth tùy tiện.

Che giấu cleanup chưa làm.

Cho phase exit khi còn debug tạm nghiêm trọng.

Cho release khi docs/handoff sai lệch.

## 15.4. Upstream Dependency

Phụ thuộc:

Implementation output.

File list.

Test/build output.

Documentation ownership.

Handoff requirement.

## 15.5. Downstream Handoff

Bàn giao:

Cleanup pass/fail.

Documentation update status.

Remaining cleanup list.

Handoff update.

Phase Exit Gate.

## 15.6. P0 điểm chặn

FAIL nếu debug/test bypass còn trong production path.

FAIL nếu docs/handoff sai làm QA/dev tiếp theo hiểu sai.

FAIL nếu cleanup xóa evidence/audit.

## 15.7. Evidence

Evidence tối thiểu:

Cleanup checklist.

Removed file/list nếu có.

Docs updated list.

Remaining cleanup note.

Reviewer decision.

## 15.8. Smoke

## TECH11-MOD-SMK-013

Given implementation còn debug bypass trong flow payment/order
When cleanup resolver kiểm tra
Then phase exit bị block.

## 16. MODULE CONTRACT 14 - DEV-TO-QA HANDOFF RESOLVER

## 16.1. Mục tiêu

Dev-to-QA Handoff Resolver chuẩn hóa việc dev bàn giao cho QA sau implementation.

## 16.2. Scope In

Handoff sang QA phải có:

Phase.

Module.

Requirement list.

File đã sửa.

Test đã chạy.

Build result.

Smoke cần QA chạy.

Evidence đã có.

Known issues.

P0 điểm chặn status.

Migration/seed note.

Test data note.

Risk note.

Expected fail-case.

Retest requirement nếu có.

## 16.3. Scope Out

Module này không được:

Bàn giao QA bằng câu “test giúp”.

Bàn giao thiếu requirement.

Bàn giao thiếu file list.

Bàn giao thiếu known issues.

Che giấu test chưa chạy.

Che giấu build fail.

Không nêu migration/seed impact.

Không nêu smoke P0.

## 16.4. Upstream Dependency

Phụ thuộc:

Implementation Report.

Evidence Mapping.

Smoke Mapping.

Risk/điểm chặn Resolver.

Cleanup Resolver.

## 16.5. Downstream Handoff

Bàn giao:

## QA_HANDOFF_READY.

## QA_HANDOFF_BLOCKED.

QA test package.

Retest scope.

QA evidence requirement.

## 16.6. P0 điểm chặn

FAIL nếu dev bàn giao QA khi build fail mà không ghi rõ.

FAIL nếu QA handoff thiếu P0 smoke.

FAIL nếu QA không biết migration/seed impact.

## 16.7. Evidence

Evidence tối thiểu:

QA handoff document.

Requirement list.

File list.

Test/build result.

Smoke list.

Known issues.

Reviewer decision.

## 16.8. Smoke

## TECH11-MOD-SMK-014

Given QA handoff thiếu known issues và smoke list
When resolver kiểm tra
Then QA handoff không Ready.

## 17. MODULE CONTRACT 15 - RELEASE HANDOFF RESOLVER

## 17.1. Mục tiêu

Release Handoff Resolver bàn giao phase/module đã hoàn tất dev sang TECH-10 để xét evidence, smoke, UAT, owner sign-off và release decision.

Module này đảm bảo phase exit không bị hiểu nhầm là release ready.

## 17.2. Scope In

Module này được phép:

Nhận phase exit result.

Kiểm tra evidence mapping.

Kiểm tra smoke mapping.

Kiểm tra QA handoff.

Kiểm tra open điểm chặn.

Kiểm tra cross-phase dependency.

Kiểm tra docs/handoff update.

Tạo release handoff package.

Gửi sang TECH-10 Global Release Governance.

Ghi rõ trạng thái chưa Release Ready nếu chưa qua TECH-10.

## 17.3. Scope Out

Module này không được:

Tự đánh Release Ready.

Tự đánh Production Ready.

Tự đánh Go-live Approved.

Tự mở Global Gateway.

Bỏ qua TECH-10.

Bỏ qua owner sign-off.

Bỏ qua UAT nếu required.

Bỏ qua smoke/evidence accepted.

Bỏ qua rollback/fallback/monitoring nếu production scope.

## 17.4. Upstream Dependency

Phụ thuộc:

Phase Exit Gate.

Implementation Report.

Evidence Mapping.

Smoke Mapping.

QA Handoff.

Cross-Phase Dependency Validator.

điểm chặn Resolver.

## TECH-10.

## 17.5. Downstream Handoff

Bàn giao:

Release handoff package.

Evidence list.

Smoke list.

Open risk list.

Dependency status.

Owner review needed.

TECH-10 Global Gateway review.

## 17.6. P0 điểm chặn

FAIL nếu release handoff gọi phase done là Release Ready.

FAIL nếu release handoff thiếu evidence/smoke.

FAIL nếu release handoff bỏ qua open điểm chặn.

FAIL nếu release handoff bypass TECH-10.

## 17.7. Evidence

Evidence tối thiểu:

Release handoff document.

Phase exit decision.

Evidence list.

Smoke list.

QA handoff.

điểm chặn status.

Dependency status.

Reviewer.

## 17.8. Smoke

## TECH11-MOD-SMK-015

Given phase exit accepted nhưng TECH-10 chưa review evidence/smoke
When Release Handoff chạy
Then status không được Release Ready, chỉ là Ready for TECH-10 Review.

## 18. CROSS-MODULE DEPENDENCY MAP

## 18.1. Luồng chuẩn

Luồng chuẩn của TECH-11:

Implementation Roadmap Orchestrator xác định phase.

Phase Entry Gate Resolver kiểm tra phase có được bắt đầu không.

Backlog Governance Resolver chuẩn hóa backlog.

Source-of-Truth Mapping Resolver map backlog với TECH.

Dev Handoff Resolver tạo handoff cho dev.

Code Handoff Control Resolver kiểm tra điều kiện trước khi coding.

Dev triển khai trong codebase thật.

Implementation Report Resolver kiểm tra báo cáo.

Evidence Mapping Resolver kiểm tra evidence.

Smoke Mapping Resolver kiểm tra smoke.

Risk / điểm chặn / Retest Resolver kiểm soát lỗi.

Cleanup / Documentation Update Resolver kiểm soát cleanup/docs.

Dev-to-QA Handoff Resolver bàn giao QA.

Phase Exit Gate Resolver xét phase exit.

Cross-Phase Dependency Validator cập nhật dependency.

Release Handoff Resolver bàn giao TECH-10.

## 18.2. No-bypass rule

Không module nào được bỏ qua:

Source-of-truth mapping.

Phase entry gate.

Backlog governance.

Dev handoff.

Code handoff control.

Implementation report.

Evidence mapping.

Smoke mapping.

điểm chặn/retest.

Phase exit gate.

Release handoff sang TECH-10.

## 18.3. Dependency ưu tiên

Nếu có mâu thuẫn:

Source-of-truth mới thắng code cũ/tài liệu cũ.

Upstream phase bị chặn thì downstream bị chặn.

P0 điểm chặn thắng phase exit.

Evidence missing thắng dev claim completed.

Smoke fail thắng dev report.

TECH-10 gate thắng mọi release claim.

Global Gateway mặc định bị chặn.

## 19. MODULE P0 điểm chặn REGISTRY - PHẦN 2/4

Các lỗi sau là P0 điểm chặn cấp module:

Roadmap cho phép nhảy phase.

Phase Entry cho phase bắt đầu khi thiếu source-of-truth.

Phase Entry cho phase bắt đầu khi upstream bị chặn.

Phase Exit accepted khi thiếu dev report.

Phase Exit accepted khi thiếu evidence/test/build.

Backlog Ready nhưng không gắn TECH/requirement.

Source-of-Truth Mapping cho code cũ thắng TECH mới.

Dev Handoff thiếu Scope Out/P0/Evidence/Smoke.

Code Handoff cho phép copy-paste code rời rạc.

Implementation Report accepted khi thiếu lệnh đã chạy.

Evidence Mapping accepted evidence không trace requirement.

Smoke Mapping pass khi chưa chạy smoke.

Cross-Phase Validator cho downstream release khi upstream bị chặn.

điểm chặn Resolver đóng P0 không retest.

Cleanup Resolver cho debug bypass vào production path.

Dev-to-QA Handoff thiếu known issues/smoke list.

Release Handoff gọi phase exit là Release Ready.

Module nào tự mở Global Gateway.

Module nào bỏ TECH-10.

Module nào hardcode policy nghiệp vụ.

## 20. EVIDENCE PACKAGE - PHẦN 2/4

## PHẦN 2/4 yêu cầu evidence thiết kế ở cấp module gồm:

Implementation Roadmap Orchestrator contract accepted.

Phase Entry Gate Resolver contract accepted.

Phase Exit Gate Resolver contract accepted.

Backlog Governance Resolver contract accepted.

Source-of-Truth Mapping Resolver contract accepted.

Dev Handoff Resolver contract accepted.

Code Handoff Control Resolver contract accepted.

Implementation Report Resolver contract accepted.

Evidence Mapping Resolver contract accepted.

Smoke Mapping Resolver contract accepted.

Cross-Phase Dependency Validator contract accepted.

Risk / điểm chặn / Retest Resolver contract accepted.

Cleanup / Documentation Update Resolver contract accepted.

Dev-to-QA Handoff Resolver contract accepted.

Release Handoff Resolver contract accepted.

Cross-module dependency accepted.

Module P0 điểm chặn Registry accepted.

Smoke matrix định hướng accepted.

Release handoff checklist prepared.

## 21. SMOKE MATRIX ĐỊNH HƯỚNG - PHẦN 2/4

TECH11-P2-SMK-001 - Phase Không Có Source Không Ready

Given phase chưa có source-of-truth
When Phase Entry Gate chạy
Then phase không được Ready.

TECH11-P2-SMK-002 - Backlog Mơ Hồ Bị Chặn

Given backlog “làm AI” không có requirement
When Backlog Governance kiểm tra
Then backlog bị reject.

TECH11-P2-SMK-003 - Code Cũ Không Thắng TECH Mới

Given implementation hiện tại khác TECH source-of-truth
When Source Mapping chạy
Then ghi deviation và TECH mới thắng.

TECH11-P2-SMK-004 - Dev Handoff Thiếu Scope Out

Given handoff thiếu Scope Out
When Dev Handoff Resolver kiểm tra
Then handoff không Ready.

TECH11-P2-SMK-005 - Code Handoff Không Đủ Context

Given yêu cầu code không có architecture/test/evidence expectation
When Code Handoff Control kiểm tra
Then code handoff bị chặn.

TECH11-P2-SMK-006 - Report Thiếu Build/Test

Given dev report thiếu build/test result
When Implementation Report Resolver kiểm tra
Then report rejected.

TECH11-P2-SMK-007 - Evidence Không Trace Requirement

Given evidence không gắn requirement
When Evidence Mapping kiểm tra
Then evidence rejected.

TECH11-P2-SMK-008 - Smoke Chưa Chạy

Given smoke P0 chưa chạy
When Phase Exit Gate kiểm tra
Then phase exit bị chặn.

TECH11-P2-SMK-009 - Dependency Upstream bị chặn

Given PHASE 2 bị chặn
When PHASE 3 release được xét
Then PHASE 3 bị chặn.

TECH11-P2-SMK-010 - điểm chặn Không Retest

Given P0 điểm chặn marked fixed nhưng chưa retest
When điểm chặn Resolver kiểm tra
Then điểm chặn chưa được CLOSED.

TECH11-P2-SMK-011 - Cleanup Fail

Given còn debug bypass trong payment/order flow
When Cleanup Resolver kiểm tra
Then phase exit bị chặn.

TECH11-P2-SMK-012 - Release Handoff Không Gọi Release Ready

Given phase exit accepted
When Release Handoff tạo package
Then status chỉ là Ready for TECH-10 Review, không Release Ready.

## 22. DONE GATE - PHẦN 2/4

## PHẦN 2/4 đạt Documentation Complete ở cấp module contract khi:

Đã khóa Implementation Roadmap Orchestrator.

Đã khóa Phase Entry Gate Resolver.

Đã khóa Phase Exit Gate Resolver.

Đã khóa Backlog Governance Resolver.

Đã khóa Source-of-Truth Mapping Resolver.

Đã khóa Dev Handoff Resolver.

Đã khóa Code Handoff Control Resolver.

Đã khóa Implementation Report Resolver.

Đã khóa Evidence Mapping Resolver.

Đã khóa Smoke Mapping Resolver.

Đã khóa Cross-Phase Dependency Validator.

Đã khóa Risk / điểm chặn / Retest Resolver.

Đã khóa Cleanup / Documentation Update Resolver.

Đã khóa Dev-to-QA Handoff Resolver.

Đã khóa Release Handoff Resolver.

Mỗi module có Scope In.

Mỗi module có Scope Out.

Mỗi module có Upstream Dependency.

Mỗi module có Downstream Handoff.

Mỗi module có P0 điểm chặn.

Mỗi module có Evidence.

Mỗi module có Smoke.

Có Cross-Module Dependency Map.

Có Module P0 điểm chặn Registry.

Có Evidence Package cấp phần.

Có Smoke Matrix định hướng.

Sẵn sàng chuyển sang PHẦN 3/4.

## 23. FAIL GATE - PHẦN 2/4

## PHẦN 2/4 FAIL nếu:

Thiếu module contract bắt buộc.

Có module chưa có Scope In.

Có module chưa có Scope Out.

Có module chưa có Upstream Dependency.

Có module chưa có Downstream Handoff.

Có module chưa có P0 điểm chặn.

Có module chưa có Evidence.

Có module chưa có Smoke.

Có module cho phép nhảy phase.

Có module cho phép backlog không gắn source-of-truth.

Có module cho phép code cũ thắng TECH mới.

Có module cho phép copy-paste code rời rạc.

Có module cho phép report thiếu build/test/evidence.

Có module cho phép smoke pass khi chưa chạy.

Có module cho phép đóng điểm chặn không retest.

Có module cho phép release handoff bỏ qua TECH-10.

Có module gọi phase exit là Release Ready.

Có module tự mở Global Gateway.

## 24. RELEASE HANDOFF - SANG PHẦN 3/4

## PHẦN 2/4 bàn giao sang PHẦN 3/4 các nội dung đã khóa:

Danh sách module implementation governance đầy đủ.

Boundary từng module.

Dependency từng module.

Handoff từng module.

P0 điểm chặn từng module.

Evidence từng module.

Smoke từng module.

Cross-module dependency map.

Phase entry/exit logic.

Backlog governance logic.

Source-of-truth mapping logic.

Dev handoff logic.

Code handoff control logic.

Implementation report logic.

Evidence/smoke mapping logic.

Cross-phase dependency logic.

Risk/điểm chặn/retest logic.

Cleanup/docs update logic.

Dev-to-QA handoff logic.

Release handoff logic.

## PHẦN 3/4 sẽ viết:

## IMPLEMENTATION LIFECYCLE / PHASE STATE MACHINE / BACKLOG-TO-CODE-TO-EVIDENCE FLOW / CROSS-PHASE CONTROL

Trong đó phải khóa rõ:

Phase lifecycle.

Backlog state model.

Source mapping state model.

Dev handoff state model.

Code handoff state model.

Implementation report state model.

Evidence mapping state model.

Smoke mapping state model.

điểm chặn/retest state model.

Cleanup/docs update state model.

Dev-to-QA handoff state model.

Release handoff state model.

Cross-phase dependency state model.

Fail-safe implementation control.

## 25. TRẠNG THÁI SAU PHẦN 2/4

Sau PHẦN 2/4:

## PHẦN 1/4 đã khóa implementation principles.

## PHẦN 2/4 đã khóa module contracts.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa đầy đủ module contracts cho TECH-11.

Các module trọng yếu đã được cố định:

Implementation Roadmap Orchestrator giữ thứ tự phase.

Phase Entry Gate Resolver chặn phase chưa đủ điều kiện.

Phase Exit Gate Resolver chặn phase báo xong thiếu evidence/test/build/smoke.

Backlog Governance Resolver chặn backlog mơ hồ.

Source-of-Truth Mapping Resolver đảm bảo TECH mới thắng tài liệu cũ và code cũ.

Dev Handoff Resolver chuẩn hóa giao việc cho dev.

Code Handoff Control Resolver chặn copy-paste code rời rạc.

Implementation Report Resolver kiểm tra báo cáo dev.

Evidence Mapping Resolver gắn evidence với requirement.

Smoke Mapping Resolver gắn smoke với phase/backlog.

Cross-Phase Dependency Validator chặn nhảy phase.

Risk / điểm chặn / Retest Resolver chặn đóng điểm chặn không retest.

Cleanup / Documentation Update Resolver chặn code rác/docs lệch.

Dev-to-QA Handoff Resolver chuẩn hóa bàn giao QA.

Release Handoff Resolver chỉ bàn giao sang TECH-10, không tự gọi Release Ready.

## PHẦN 2/4 sẵn sàng bàn giao sang:

## PHẦN 3/4 - IMPLEMENTATION LIFECYCLE / PHASE STATE MACHINE / BACKLOG-TO-CODE-TO-EVIDENCE FLOW / CROSS-PHASE CONTROL.

## PHẦN 3/4 - IMPLEMENTATION LIFECYCLE / PHASE STATE MACHINE / BACKLOG-TO-CODE-TO-EVIDENCE FLOW / CROSS-PHASE CONTROL

## 1. MỤC TIÊU PHẦN 3/4

## PHẦN 3/4 khóa vòng đời triển khai dev từ lúc một yêu cầu còn nằm trong tài liệu source-of-truth cho đến khi được đưa vào backlog, handoff cho dev, triển khai trong codebase thật, báo cáo kết quả, nộp evidence, chạy smoke, bàn giao QA và chuyển sang TECH-10 Release Governance.

PHẦN này xác định rõ:

Phase đi qua những trạng thái nào.

Backlog đi từ ý tưởng đến Ready như thế nào.

Source-of-truth được map vào backlog ra sao.

Dev handoff được tạo, review, accepted hoặc rejected thế nào.

Code handoff được phép khi nào.

Implementation report được kiểm tra thế nào.

Evidence được map với requirement ra sao.

Smoke được map với phase/backlog ra sao.

điểm chặn và retest được xử lý thế nào.

Cleanup và documentation update được kiểm soát thế nào.

Dev bàn giao QA thế nào.

Release handoff sang TECH-10 thế nào.

Cross-phase dependency chặn nhảy phase ra sao.

Khi thiếu runtime/evidence/smoke thì fail-safe như thế nào.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế database chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa implementation lifecycle, state machine, transition, điểm chặn và handoff control.

## 2. NGUYÊN TẮC LIFECYCLE CHUNG

Toàn bộ Implementation Lifecycle phải tuân thủ:

Source-of-truth mới thắng tài liệu cũ và code cũ.

Backlog không có TECH/section/requirement thì không Ready.

Phase không có Entry Gate thì không bắt đầu.

Phase không có Exit Gate thì không kết thúc.

Dev handoff không đủ Scope In/Scope Out/Dependency/P0/Evidence/Smoke thì không Ready.

Code handoff không đủ architecture/test/evidence expectation thì không được coding.

Dev report thiếu file list/test/build/evidence thì không accepted.

Smoke bắt buộc chưa chạy thì phase không exit.

Evidence không map requirement thì không accepted.

P0 điểm chặn chưa retest thì không closed.

Cleanup chưa đạt thì không phase exit.

QA handoff thiếu smoke/known issues thì không Ready.

Release handoff không được gọi phase exit là Release Ready.

TECH-10 mới xét Completion Decision / Release Ready / Release Approved / Go-live Approved.

Global Gateway mặc định bị chặn.

## 3. IMPLEMENTATION LIFECYCLE TỔNG THỂ

## 3.1. Luồng chuẩn

Luồng chuẩn của một yêu cầu triển khai:

Requirement được khóa trong TECH source-of-truth.

Source-of-Truth Mapping Resolver map requirement vào phase.

Backlog Governance Resolver tạo backlog item đúng chuẩn.

Phase Entry Gate Resolver kiểm tra phase có đủ điều kiện bắt đầu.

Dev Handoff Resolver tạo handoff cho dev.

Code Handoff Control Resolver kiểm tra có đủ điều kiện đi vào code.

Dev triển khai trong codebase thật.

Dev tạo Implementation Report.

Evidence Mapping Resolver kiểm tra evidence.

Smoke Mapping Resolver kiểm tra smoke.

Risk / điểm chặn / Retest Resolver xử lý lỗi.

Cleanup / Documentation Update Resolver kiểm tra dọn dẹp và cập nhật tài liệu.

Dev-to-QA Handoff Resolver bàn giao QA.

QA chạy smoke/retest/UAT-prep theo scope.

Phase Exit Gate Resolver xét phase exit.

Release Handoff Resolver bàn giao sang TECH-10.

TECH-10 xét evidence accepted, smoke pass, UAT, owner sign-off, release decision và Global Gateway.

## 3.2. Luồng cấm

Các luồng sau bị cấm:

Requirement mơ hồ -> Code.

Backlog không source -> Dev Ready.

Phase chưa Entry Ready -> Implementation.

Dev handoff thiếu Scope Out -> Coding.

Code snippet AI -> Copy vào project.

Dev done -> Release Ready.

Phase exit -> Production Ready.

Report thiếu evidence -> Accepted.

Smoke chưa chạy -> Phase Exit.

P0 điểm chặn fixed nhưng chưa retest -> Closed.

QA handoff thiếu known issues -> QA Ready.

Release handoff -> Go-live Approved.

TECH-10 chưa pass -> Global Gateway Open.

## 4. PHASE LIFECYCLE STATE MODEL

## 4.1. Danh sách trạng thái phase

Mỗi phase triển khai có các trạng thái:

PHASE_NOT_STARTED
Phase chưa bắt đầu.

PHASE_PLANNING
Đang lập scope, backlog, dependency, owner, evidence, smoke.

PHASE_ENTRY_REVIEW
Đang kiểm tra Entry Gate.

PHASE_ENTRY_BLOCKED
Phase bị chặn vì thiếu điều kiện bắt đầu.

PHASE_ENTRY_READY
Phase đủ điều kiện bắt đầu implementation.

PHASE_DEV_HANDOFF_READY
Handoff cho dev đã đủ.

PHASE_IMPLEMENTATION_IN_PROGRESS
Dev đang triển khai trong codebase thật.

PHASE_IMPLEMENTATION_REPORTED
Dev đã nộp implementation report.

PHASE_EVIDENCE_REVIEW
Evidence đang được review/map requirement.

PHASE_SMOKE_REVIEW
Smoke đang được review/map phase.

PHASE_QA_HANDOFF_READY
Đủ điều kiện bàn giao QA.

PHASE_QA_IN_PROGRESS
QA đang test/smoke/retest.

PHASE_EXIT_REVIEW
Đang kiểm tra Exit Gate.

PHASE_EXIT_BLOCKED
Phase chưa được exit vì thiếu evidence/smoke/report/cleanup hoặc còn điểm chặn.

PHASE_EXIT_ACCEPTED
Phase hoàn tất ở cấp dev/QA handoff.

PHASE_READY_FOR_TECH10_REVIEW
Phase sẵn sàng bàn giao sang TECH-10 Global Release Governance.

PHASE_RELEASE_REVIEW_IN_TECH10
TECH-10 đang review evidence/smoke/UAT/sign-off/release gate.

PHASE_RELEASE_BLOCKED
TECH-10 chặn release.

PHASE_RELEASE_READY_BY_TECH10
TECH-10 xác nhận Release Ready theo scope.

## 4.2. Transition hợp lệ

## PHASE_NOT_STARTED -> PHASE_PLANNING.

## PHASE_PLANNING -> PHASE_ENTRY_REVIEW.

## PHASE_ENTRY_REVIEW -> PHASE_ENTRY_BLOCKED.

## PHASE_ENTRY_REVIEW -> PHASE_ENTRY_READY.

## PHASE_ENTRY_READY -> PHASE_DEV_HANDOFF_READY.

## PHASE_DEV_HANDOFF_READY -> PHASE_IMPLEMENTATION_IN_PROGRESS.

## PHASE_IMPLEMENTATION_IN_PROGRESS -> PHASE_IMPLEMENTATION_REPORTED.

## PHASE_IMPLEMENTATION_REPORTED -> PHASE_EVIDENCE_REVIEW.

## PHASE_EVIDENCE_REVIEW -> PHASE_SMOKE_REVIEW.

## PHASE_SMOKE_REVIEW -> PHASE_QA_HANDOFF_READY.

## PHASE_QA_HANDOFF_READY -> PHASE_QA_IN_PROGRESS.

## PHASE_QA_IN_PROGRESS -> PHASE_EXIT_REVIEW.

## PHASE_EXIT_REVIEW -> PHASE_EXIT_BLOCKED.

## PHASE_EXIT_REVIEW -> PHASE_EXIT_ACCEPTED.

## PHASE_EXIT_ACCEPTED -> PHASE_READY_FOR_TECH10_REVIEW.

## PHASE_READY_FOR_TECH10_REVIEW -> PHASE_RELEASE_REVIEW_IN_TECH10.

## PHASE_RELEASE_REVIEW_IN_TECH10 -> PHASE_RELEASE_BLOCKED.

## PHASE_RELEASE_REVIEW_IN_TECH10 -> PHASE_RELEASE_READY_BY_TECH10.

## 4.3. Transition bị cấm

## PHASE_NOT_STARTED -> PHASE_IMPLEMENTATION_IN_PROGRESS.

PHASE_PLANNING -> PHASE_IMPLEMENTATION_IN_PROGRESS nếu chưa Entry Ready.

## PHASE_ENTRY_BLOCKED -> PHASE_IMPLEMENTATION_IN_PROGRESS.

PHASE_DEV_HANDOFF_READY -> PHASE_EXIT_ACCEPTED nếu chưa implementation.

PHASE_IMPLEMENTATION_REPORTED -> PHASE_EXIT_ACCEPTED nếu chưa evidence/smoke review.

PHASE_EXIT_ACCEPTED -> PHASE_RELEASE_READY_BY_TECH10 nếu TECH-10 chưa review.

PHASE_EXIT_ACCEPTED -> Production Ready.

PHASE_READY_FOR_TECH10_REVIEW -> Go-live Approved.

PHASE_RELEASE_BLOCKED -> Go-live Approved.

PHASE_RELEASE_READY_BY_TECH10 -> Scale Approved nếu chưa qua TECH-10 scale gate.

## 4.4. P0 điểm chặn

FAIL nếu phase nhảy từ planning sang implementation mà chưa Entry Gate.

FAIL nếu phase exit khi thiếu report/evidence/smoke.

FAIL nếu phase exit bị gọi là Release Ready.

FAIL nếu TECH-10 chưa review nhưng phase được tuyên bố production ready.

## 5. BACKLOG STATE MODEL

## 5.1. Danh sách trạng thái backlog

Backlog item có các trạng thái:

BACKLOG_IDEA
Ý tưởng/yêu cầu thô.

BACKLOG_NEED_SOURCE_MAPPING
Cần map vào TECH source-of-truth.

BACKLOG_SOURCE_MAPPED
Đã map nguồn.

BACKLOG_NEED_SCOPE_REVIEW
Cần kiểm tra Scope In/Scope Out.

BACKLOG_NEED_DEPENDENCY_REVIEW
Cần kiểm tra dependency.

BACKLOG_NEED_EVIDENCE_SMOKE
Cần mở rộng evidence/smoke.

BACKLOG_BLOCKED
Bị chặn.

BACKLOG_READY_FOR_HANDOFF
Sẵn sàng handoff cho dev.

BACKLOG_IN_IMPLEMENTATION
Dev đang triển khai.

BACKLOG_IMPLEMENTED_WAITING_REPORT
Đã triển khai, chờ report.

BACKLOG_REPORTED_WAITING_EVIDENCE
Có report, chờ evidence.

BACKLOG_EVIDENCE_ACCEPTED
Evidence accepted ở cấp phase.

BACKLOG_SMOKE_PASS
Smoke pass ở cấp phase.

BACKLOG_READY_FOR_QA
Sẵn sàng QA.

BACKLOG_QA_PASS
QA pass.

BACKLOG_DONE_FOR_PHASE
Hoàn tất trong phase.

BACKLOG_READY_FOR_TECH10
Sẵn sàng đưa vào TECH-10 review.

## 5.2. Backlog Ready Rule

Backlog chỉ được Ready khi có:

TECH reference.

Section/requirement.

Phase.

Owner.

Scope In.

Scope Out.

Dependency.

Evidence requirement.

Smoke requirement.

P0 điểm chặn.

Done Gate.

Fail Gate.

Handoff target.

## 5.3. Backlog bị cấm Ready khi

Backlog không được Ready nếu:

Chỉ ghi “làm AI”.

Chỉ ghi “làm kho”.

Chỉ ghi “sửa cho chạy”.

Không có TECH source.

Không có Scope Out.

Không có dependency.

Không có evidence/smoke.

Upstream phase đang bị chặn.

Có source conflict chưa xử lý.

Có P0 điểm chặn chưa closed.

## 5.4. P0 điểm chặn

FAIL nếu backlog mơ hồ vẫn Ready.

FAIL nếu backlog không map source vẫn đưa dev.

FAIL nếu backlog downstream Ready khi upstream bị chặn.

FAIL nếu backlog không evidence/smoke vẫn được phase exit.

## 6. SOURCE-OF-TRUTH MAPPING STATE MODEL

## 6.1. Danh sách trạng thái

Source mapping có các trạng thái:

## SOURCE_NOT_MAPPED.

## SOURCE_MAPPING_IN_PROGRESS.

## SOURCE_MAPPED_TO_TECH.

## SOURCE_CONFLICT_DETECTED.

## SOURCE_CONFLICT_REVIEW_REQUIRED.

## SOURCE_CONFLICT_RESOLVED_TECH_WINS.

## SOURCE_CONFLICT_RESOLVED_OWNER_DECISION.

## SOURCE_MAPPING_REJECTED.

## SOURCE_MAPPING_ACCEPTED.

## 6.2. Source mapping rule

Một source mapping hợp lệ phải có:

TECH number.

Section.

Requirement text hoặc requirement ID.

Phase.

Backlog item.

Legacy reference status nếu có.

Current implementation deviation nếu có.

Decision.

Reviewer.

Audit note.

## 6.3. Conflict rule

Khi có mâu thuẫn:

TECH mới thắng tài liệu cũ.

TECH mới thắng code cũ.

Không tự chọn theo cảm tính.

Không silently ignore conflict.

Phải ghi deviation/điểm chặn.

Nếu cần owner decision thì route owner.

## 6.4. Transition bị cấm

SOURCE_CONFLICT_DETECTED -> SOURCE_MAPPING_ACCEPTED nếu chưa review.

## SOURCE_NOT_MAPPED -> DEV_HANDOFF_READY.

## SOURCE_MAPPING_REJECTED -> CODE_HANDOFF_ALLOWED.

Current implementation wins over TECH source nếu chưa owner decision.

## 6.5. P0 điểm chặn

FAIL nếu code cũ thắng TECH mới.

FAIL nếu tài liệu cũ thắng TECH mới.

FAIL nếu source conflict không được ghi điểm chặn.

FAIL nếu requirement không source vẫn coding.

## 7. DEV HANDOFF STATE MODEL

## 7.1. Danh sách trạng thái

Dev handoff có các trạng thái:

## HANDOFF_NOT_CREATED.

## HANDOFF_DRAFTING.

## HANDOFF_WAITING_REVIEW.

## HANDOFF_REJECTED.

## HANDOFF_NEED_MORE_CONTEXT.

## HANDOFF_READY_FOR_DEV.

## HANDOFF_ACCEPTED_BY_DEV.

## HANDOFF_BLOCKED_BY_DEPENDENCY.

## HANDOFF_SUPERSEDED.

## 7.2. Handoff Ready Rule

Handoff chỉ Ready khi có:

Phase.

Module.

Mục tiêu.

Source-of-truth.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Report format.

Hardcode policy warning.

No-copy-paste-code rule.

TECH-10 release gate note.

## 7.3. Transition bị cấm

HANDOFF_DRAFTING -> HANDOFF_READY_FOR_DEV nếu thiếu source.

HANDOFF_WAITING_REVIEW -> HANDOFF_ACCEPTED_BY_DEV nếu chưa review.

## HANDOFF_REJECTED -> CODE_HANDOFF_ALLOWED.

## HANDOFF_BLOCKED_BY_DEPENDENCY -> IMPLEMENTATION_IN_PROGRESS.

## HANDOFF_READY_FOR_DEV -> RELEASE_READY.

## 7.4. P0 điểm chặn

FAIL nếu handoff chỉ ghi “làm module này”.

FAIL nếu handoff thiếu Scope Out.

FAIL nếu handoff không cảnh báo hardcode policy.

FAIL nếu handoff cho copy-paste code.

## 8. CODE HANDOFF STATE MODEL

## 8.1. Danh sách trạng thái

Code handoff có các trạng thái:

## CODE_HANDOFF_NOT_ALLOWED.

## CODE_HANDOFF_CONTEXT_REVIEW.

## CODE_HANDOFF_NEED_ARCHITECTURE_CONTEXT.

## CODE_HANDOFF_NEED_DB_IMPACT_REVIEW.

## CODE_HANDOFF_NEED_API_IMPACT_REVIEW.

## CODE_HANDOFF_NEED_UI_IMPACT_REVIEW.

## CODE_HANDOFF_NEED_TEST_EXPECTATION.

## CODE_HANDOFF_NEED_EVIDENCE_EXPECTATION.

## CODE_HANDOFF_BLOCKED_BY_SOURCE_CONFLICT.

## CODE_HANDOFF_BLOCKED_BY_DEPENDENCY.

## CODE_HANDOFF_ALLOWED_IN_CODEBASE.

## CODE_HANDOFF_REVOKED.

## 8.2. Code Handoff Allowed Rule

Code handoff chỉ allowed khi có:

Requirement rõ.

Source mapping accepted.

Phase Entry Ready.

Dev handoff accepted.

Architecture context đủ.

DB impact đã xác định hoặc xác nhận không áp dụng.

API impact đã xác định hoặc xác nhận không áp dụng.

UI impact đã xác định hoặc xác nhận không áp dụng.

Test expectation rõ.

Evidence expectation rõ.

Không có upstream điểm chặn.

Không có source conflict mở.

Không yêu cầu copy-paste code snippet rời rạc.

## 8.3. Transition bị cấm

## CODE_HANDOFF_NOT_ALLOWED -> IMPLEMENTATION_IN_PROGRESS.

## CODE_HANDOFF_NEED_ARCHITECTURE_CONTEXT -> CODE_HANDOFF_ALLOWED.

## CODE_HANDOFF_BLOCKED_BY_SOURCE_CONFLICT -> CODING.

CODE_HANDOFF_ALLOWED_IN_CODEBASE -> Production Ready.

Code handoff allowed -> Hardcode policy.

## 8.4. P0 điểm chặn

FAIL nếu coding agent được yêu cầu viết code không có project context.

FAIL nếu code handoff bỏ qua DB/API/UI impact.

FAIL nếu code handoff không có test/evidence expectation.

FAIL nếu dev copy code rời rạc vào codebase.

## 9. IMPLEMENTATION REPORT STATE MODEL

## 9.1. Danh sách trạng thái

Implementation report có các trạng thái:

## REPORT_NOT_SUBMITTED.

## REPORT_DRAFTING.

## REPORT_SUBMITTED.

## REPORT_UNDER_REVIEW.

## REPORT_NEED_MORE_INFO.

## REPORT_REJECTED.

## REPORT_ACCEPTED_FOR_PHASE_REVIEW.

## REPORT_ACCEPTED_WITH_RISKS.

## REPORT_SUPERSEDED_BY_REWORK.

## 9.2. Report Accepted Rule

Report chỉ accepted khi có đủ:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Backend build result.

Frontend build result.

Cleanup result.

Markdown/docs update.

Migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Handoff update.

## 9.3. Report bị reject khi

Report bị reject nếu:

Chỉ ghi “đã xong”.

Không có file list.

Không có source requirement.

Không có lệnh đã chạy.

Không có test/build result.

Không có evidence.

Không nêu known risk.

Không nói migration/seed khi có impact.

Không có handoff update.

Che giấu fail/warning/điểm chặn.

## 9.4. P0 điểm chặn

FAIL nếu report thiếu test/build vẫn accepted.

FAIL nếu report không có source requirement.

FAIL nếu report không có risk nhưng thực tế còn known issue.

FAIL nếu report accepted rồi chuyển thẳng Release Ready.

## 10. EVIDENCE MAPPING STATE MODEL

## 10.1. Danh sách trạng thái

Evidence mapping có các trạng thái:

## EVIDENCE_NOT_SUBMITTED.

## EVIDENCE_SUBMITTED.

## EVIDENCE_MAPPING_IN_PROGRESS.

## EVIDENCE_MISSING_REQUIREMENT_LINK.

## EVIDENCE_MISSING_ENVIRONMENT.

## EVIDENCE_MISSING_EXPECTED_ACTUAL.

## EVIDENCE_PRIVACY_REVIEW_REQUIRED.

## EVIDENCE_REJECTED.

## EVIDENCE_NEED_MORE_INFO.

## EVIDENCE_MAPPED_ACCEPTED_FOR_PHASE.

## EVIDENCE_READY_FOR_TECH10_REVIEW.

## EVIDENCE_REVOKED.

## 10.2. Evidence Mapping Rule

Evidence phải có:

Evidence ID.

Phase.

Backlog item.

TECH reference.

Requirement.

Environment.

Expected result.

Actual result.

Pass/fail.

Timestamp.

Reviewer/tester.

Sensitive data check.

Correlation/audit id nếu là runtime flow.

## 10.3. Transition bị cấm

EVIDENCE_SUBMITTED -> EVIDENCE_MAPPED_ACCEPTED nếu chưa review.

EVIDENCE_MISSING_REQUIREMENT_LINK -> Phase Exit.

EVIDENCE_PRIVACY_REVIEW_REQUIRED -> Accepted nếu chưa privacy pass.

EVIDENCE_REJECTED -> Release Handoff.

EVIDENCE_REVOKED -> Gateway pass.

## 10.4. P0 điểm chặn

FAIL nếu evidence không gắn requirement.

FAIL nếu evidence thiếu environment.

FAIL nếu evidence lộ dữ liệu nhạy cảm.

FAIL nếu evidence local dùng để tuyên bố production ready.

## 11. SMOKE MAPPING STATE MODEL

## 11.1. Danh sách trạng thái

Smoke mapping có các trạng thái:

## SMOKE_NOT_DEFINED.

## SMOKE_DEFINED_FOR_BACKLOG.

## SMOKE_READY_TO_RUN.

## SMOKE_RUNNING.

## SMOKE_PASS_FOR_PHASE.

## SMOKE_FAIL_FOR_PHASE.

## SMOKE_BLOCKED_BY_DEPENDENCY.

## SMOKE_RETEST_REQUIRED.

## SMOKE_RETEST_PASS.

## SMOKE_READY_FOR_QA_REVIEW.

## SMOKE_READY_FOR_TECH10_REVIEW.

## 11.2. Smoke Mapping Rule

Smoke phải map với:

Smoke ID.

Phase.

Backlog.

TECH requirement.

Scenario.

Given/When/Then.

Must Not.

Expected result.

Actual result.

Evidence.

Pass/fail.

Retest nếu có.

## 11.3. Smoke bị reject khi

Smoke bị reject nếu:

Không có Smoke ID.

Không có requirement.

Không có actual result.

Không có evidence.

Chỉ test happy path trong khi requirement có fail-case.

Không test P0 điểm chặn.

Không test runtime unavailable nếu requirement yêu cầu.

Không test privacy/suppression nếu liên quan.

## 11.4. P0 điểm chặn

FAIL nếu smoke P0 chưa chạy nhưng phase exit.

FAIL nếu smoke fail nhưng handoff release.

FAIL nếu smoke không có evidence.

FAIL nếu smoke không test fail-case quan trọng.

## 12. điểm chặn / RETEST STATE MODEL

## 12.1. Danh sách trạng thái

điểm chặn/retest có các trạng thái:

## STOP_POINT_NOT_REPORTED.

## STOP_POINT_OPEN.

## STOP_POINT_ASSIGNED.

## STOP_POINT_FIX_IN_PROGRESS.

## STOP_POINT_FIX_SUBMITTED.

## STOP_POINT_RETEST_REQUIRED.

## STOP_POINT_RETEST_RUNNING.

## STOP_POINT_RETEST_FAILED.

## STOP_POINT_RETEST_PASS.

## STOP_POINT_WAITING_ACCEPTANCE.

## STOP_POINT_CLOSED.

## STOP_POINT_REOPENED.

## STOP_POINT_ESCALATED_TO_OWNER.

## 12.2. điểm chặn Closure Rule

điểm chặn chỉ được closed khi có:

Fix description.

File impact.

Evidence.

Retest evidence.

Owner/QA acceptance nếu P0.

Regression note nếu cần.

Handoff update.

Audit note.

## 12.3. Transition bị cấm

## STOP_POINT_OPEN -> STOP_POINT_CLOSED.

STOP_POINT_FIX_SUBMITTED -> STOP_POINT_CLOSED nếu chưa retest.

## STOP_POINT_RETEST_FAILED -> PHASE_EXIT_ACCEPTED.

STOP_POINT_REOPENED -> Release Handoff.

STOP_POINT_ESCALATED_TO_OWNER -> Release Ready nếu chưa decision.

## 12.4. P0 điểm chặn

FAIL nếu đóng điểm chặn bằng miệng.

FAIL nếu đóng điểm chặn không retest.

FAIL nếu P0 điểm chặn mở nhưng phase exit.

FAIL nếu điểm chặn không xuất hiện trong implementation report.

## 13. CLEANUP / DOCUMENTATION UPDATE STATE MODEL

## 13.1. Danh sách trạng thái

Cleanup/docs update có các trạng thái:

## CLEANUP_NOT_STARTED.

## CLEANUP_IN_PROGRESS.

## CLEANUP_PASS.

## CLEANUP_FAIL.

## DOC_UPDATE_NOT_REQUIRED.

## DOC_UPDATE_REQUIRED.

## DOC_UPDATE_IN_PROGRESS.

## DOC_UPDATE_PASS.

## DOC_UPDATE_FAIL.

## HANDOFF_UPDATE_REQUIRED.

## HANDOFF_UPDATE_PASS.

## 13.2. Cleanup Pass Rule

Cleanup pass khi:

Không còn debug bypass.

Không còn code tạm nguy hiểm.

Không còn duplicate logic critical.

Không còn unused config nguy hiểm.

Không xóa audit/evidence.

Không xóa migration cần thiết.

Không làm docs/handoff lệch thực tế.

Known cleanup còn lại được ghi rõ nếu không critical.

## 13.3. Documentation Update Rule

Docs/handoff phải update khi:

Có thay đổi behavior.

Có thay đổi config.

Có migration/seed impact.

Có thay đổi smoke.

Có known issue.

Có handoff sang phase sau.

Có risk mới.

## 13.4. P0 điểm chặn

FAIL nếu debug bypass còn trong production path.

FAIL nếu docs sai làm QA/dev hiểu nhầm.

FAIL nếu cleanup xóa evidence/audit.

FAIL nếu migration/seed impact không được note.

## 14. DEV-TO-QA HANDOFF STATE MODEL

## 14.1. Danh sách trạng thái

Dev-to-QA handoff có các trạng thái:

## QA_HANDOFF_NOT_CREATED.

## QA_HANDOFF_DRAFTING.

## QA_HANDOFF_WAITING_REVIEW.

## QA_HANDOFF_REJECTED.

## QA_HANDOFF_NEED_MORE_INFO.

## QA_HANDOFF_READY.

## QA_ACCEPTED_HANDOFF.

## QA_TESTING_IN_PROGRESS.

## QA_FEEDBACK_RETURNED.

## QA_HANDOFF_COMPLETED.

## 14.2. QA Handoff Ready Rule

QA handoff Ready khi có:

Phase.

Module.

Requirement list.

File list.

Test/build result.

Smoke list.

Evidence list.

Known issues.

P0 điểm chặn status.

Migration/seed note.

Test data note.

Expected fail-case.

Retest scope nếu có.

## 14.3. Transition bị cấm

QA_HANDOFF_DRAFTING -> QA_HANDOFF_READY nếu thiếu smoke list.

## QA_HANDOFF_REJECTED -> QA_TESTING_IN_PROGRESS.

QA_HANDOFF_READY -> Release Handoff nếu QA chưa accepted khi QA required.

QA_FEEDBACK_RETURNED -> PHASE_EXIT_ACCEPTED nếu có P0 issue mở.

## 14.4. P0 điểm chặn

FAIL nếu QA handoff thiếu known issues.

FAIL nếu QA handoff che giấu build/test fail.

FAIL nếu QA handoff thiếu migration/seed impact.

FAIL nếu QA không biết smoke P0 cần chạy.

## 15. RELEASE HANDOFF STATE MODEL

## 15.1. Danh sách trạng thái

Release handoff có các trạng thái:

## RELEASE_HANDOFF_NOT_READY.

## RELEASE_HANDOFF_PREPARING.

## RELEASE_HANDOFF_WAITING_REVIEW.

## RELEASE_HANDOFF_REJECTED.

## RELEASE_HANDOFF_NEED_MORE_EVIDENCE.

## RELEASE_HANDOFF_READY_FOR_TECH10.

## RELEASE_HANDOFF_ACCEPTED_BY_TECH10.

## RELEASE_HANDOFF_BLOCKED_BY_TECH10.

## RELEASE_HANDOFF_SUPERSEDED_BY_REWORK.

## 15.2. Release Handoff Ready Rule

Release handoff chỉ Ready for TECH-10 khi có:

Phase exit accepted.

Implementation report accepted.

Evidence mapped.

Smoke mapped.

QA handoff completed nếu required.

P0 điểm chặn closed.

Cleanup/docs update pass.

Cross-phase dependency pass.

Known risk list.

TECH-10 review package.

## 15.3. Transition bị cấm

RELEASE_HANDOFF_READY_FOR_TECH10 -> Release Ready.

RELEASE_HANDOFF_READY_FOR_TECH10 -> Production Ready.

RELEASE_HANDOFF_READY_FOR_TECH10 -> Go-live Approved.

RELEASE_HANDOFF_NEED_MORE_EVIDENCE -> TECH10 accepted.

RELEASE_HANDOFF_BLOCKED_BY_TECH10 -> Gateway Open.

## 15.4. P0 điểm chặn

FAIL nếu release handoff gọi là Release Ready.

FAIL nếu release handoff thiếu evidence/smoke.

FAIL nếu release handoff có điểm chặn mở.

FAIL nếu release handoff bypass TECH-10.

## 16. CROSS-PHASE DEPENDENCY STATE MODEL

## 16.1. Danh sách trạng thái

Cross-phase dependency có các trạng thái:

## DEPENDENCY_NOT_CHECKED.

## DEPENDENCY_CHECKING.

## DEPENDENCY_PASS.

## DEPENDENCY_FAIL.

## DEPENDENCY_BLOCKED_BY_UPSTREAM.

## DEPENDENCY_BLOCKED_BY_DOWNSTREAM_SCOPE.

## DEPENDENCY_NEED_OWNER_DECISION.

## DEPENDENCY_RETEST_REQUIRED.

## DEPENDENCY_ACCEPTED_FOR_HANDOFF.

## 16.2. Dependency chain bắt buộc

Các dependency phase tối thiểu:

PHASE 0 -> toàn bộ phase sau.

## PHASE 1 -> PHASE 2.

## PHASE 2 -> PHASE 3.

## PHASE 3 -> PHASE 4.

## PHASE 4 -> PHASE 5.

## PHASE 5 -> PHASE 7.

## PHASE 3 -> PHASE 6.

## PHASE 3 -> PHASE 8.

PHASE 9 -> release/go-live/scale.

Sale Lock / Recall -> Commerce/AI/Gateway/Ads/Live/IVR.

Evidence/Audit -> high-risk action ở mọi phase.

Verified Revenue -> Ads/Scale.

QuoteSnapshot -> AI/Gateway/Live báo giá.

Official Order/Core Order State -> IVR.

Final Response Guard -> Gateway/Live response.

## 16.3. Transition bị cấm

DEPENDENCY_FAIL -> downstream release.

DEPENDENCY_BLOCKED_BY_UPSTREAM -> phase exit downstream.

DEPENDENCY_NOT_CHECKED -> release handoff.

DEPENDENCY_NEED_OWNER_DECISION -> code handoff nếu risk critical.

Mock upstream -> production truth.

## 16.4. P0 điểm chặn

FAIL nếu downstream release khi upstream fail.

FAIL nếu mock được dùng như production truth.

FAIL nếu dependency fail bị downgrade thành warning nhẹ.

FAIL nếu Sale Lock/Recall không block downstream.

## 17. FAIL-SAFE IMPLEMENTATION CONTROL

## 17.1. Khi source-of-truth không rõ

Nếu source-of-truth không rõ:

Không tạo backlog Ready.

Không handoff dev.

Không code.

Không phase entry.

Route owner review.

## 17.2. Khi dependency không rõ

Nếu dependency không rõ:

Không release downstream.

Có thể chuẩn bị tài liệu/test nhưng không go-live.

Route dependency review.

Ghi điểm chặn.

## 17.3. Khi evidence không đủ

Nếu evidence không đủ:

Không phase exit.

Không QA pass.

Không release handoff accepted.

Không Completion Decision ở TECH-10.

Ghi evidence gap.

## 17.4. Khi smoke fail

Nếu smoke fail:

Mở điểm chặn.

Yêu cầu fix.

Yêu cầu retest.

Không phase exit.

Không release handoff.

## 17.5. Khi report thiếu

Nếu report thiếu:

Report rejected.

Phase exit bị chặn.

Dev mở rộng report.

Không tự suy đoán phần thiếu.

## 17.6. Khi release governance unavailable

Nếu TECH-10/release governance chưa sẵn sàng:

Không gọi Release Ready.

Không mở Gateway.

Chỉ ghi Ready for TECH-10 Review.

Global Gateway vẫn bị chặn.

## 18. BACKLOG-TO-CODE-TO-EVIDENCE FLOW

## 18.1. Flow chuẩn

Flow chuẩn:

## BACKLOG_IDEA.

## BACKLOG_NEED_SOURCE_MAPPING.

## BACKLOG_SOURCE_MAPPED.

## BACKLOG_NEED_SCOPE_REVIEW.

## BACKLOG_NEED_DEPENDENCY_REVIEW.

## BACKLOG_NEED_EVIDENCE_SMOKE.

## BACKLOG_READY_FOR_HANDOFF.

## HANDOFF_READY_FOR_DEV.

## CODE_HANDOFF_ALLOWED_IN_CODEBASE.

## BACKLOG_IN_IMPLEMENTATION.

## REPORT_SUBMITTED.

## REPORT_ACCEPTED_FOR_PHASE_REVIEW.

## EVIDENCE_MAPPED_ACCEPTED_FOR_PHASE.

## SMOKE_PASS_FOR_PHASE.

## QA_HANDOFF_READY.

## QA_HANDOFF_COMPLETED.

## PHASE_EXIT_ACCEPTED.

## RELEASE_HANDOFF_READY_FOR_TECH10.

## 18.2. Flow khi source conflict

## SOURCE_CONFLICT_DETECTED.

## SOURCE_CONFLICT_REVIEW_REQUIRED.

Route owner/source review.

Nếu TECH mới thắng: SOURCE_CONFLICT_RESOLVED_TECH_WINS.

Nếu cần owner decision: SOURCE_CONFLICT_RESOLVED_OWNER_DECISION.

Update backlog.

Handoff lại.

Không coding khi conflict còn mở.

## 18.3. Flow khi dev report fail

## REPORT_SUBMITTED.

## REPORT_UNDER_REVIEW.

REPORT_REJECTED hoặc REPORT_NEED_MORE_INFO.

Dev mở rộng report.

Evidence/smoke chưa được accepted.

Phase exit bị chặn.

## 18.4. Flow khi smoke fail

## SMOKE_RUNNING.

## SMOKE_FAIL_FOR_PHASE.

## STOP_POINT_OPEN.

## STOP_POINT_ASSIGNED.

Fix.

Retest.

## SMOKE_RETEST_PASS.

điểm chặn WAITING acceptance.

điểm chặn closed.

Phase exit review lại.

## 18.5. Flow khi QA phát hiện lỗi

## QA_TESTING_IN_PROGRESS.

## QA_FEEDBACK_RETURNED.

Nếu P0: STOP_POINT_OPEN.

Nếu thiếu evidence: EVIDENCE_NEED_MORE_INFO.

Nếu thiếu docs: DOC_UPDATE_REQUIRED.

Fix/retest.

QA accepted lại.

Phase exit review.

## 19. PHASE-SPECIFIC LIFECYCLE CONTROL

## 19.1. PHASE 0 - Foundation

PHASE 0 phải exit trước khi high-risk phase sử dụng audit/evidence/RBAC.

P0 Fail nếu phase sau dùng high-risk command mà audit/evidence foundation chưa pass.

## 19.2. PHASE 1 - Product / SKU / Recipe

PHASE 1 phải khóa Product/SKU/Recipe/Activation trước Operational/Commerce.

P0 Fail nếu Product Active bị dùng như Sellable.

## 19.3. PHASE 2 - Operational Core

PHASE 2 phải khóa Sellable Gate, Inventory, Recall, Sale Lock.

P0 Fail nếu Commerce bán khi Operational bị chặn.

## 19.4. PHASE 3 - Commerce Runtime

PHASE 3 phải khóa QuoteSnapshot, Order, Payment, Verified Revenue.

P0 Fail nếu AI/Ads/IVR dùng Commerce khi Commerce chưa pass.

## 19.5. PHASE 4 - AI Advisor

PHASE 4 phải phụ thuộc Product/Commerce/Claim/Final Response Guard.

P0 Fail nếu AI tự tính giá/tạo order/xác nhận payment.

## 19.6. PHASE 5 - Facebook Gateway

PHASE 5 phải phụ thuộc AI Final Response Guard và Commerce public/private boundary.

P0 Fail nếu Gateway gửi response chưa guard hoặc public dữ liệu private.

## 19.7. PHASE 6 - Ads Measurement

PHASE 6 phải phụ thuộc Verified Revenue, Data Quality, Dedup/Idempotency.

P0 Fail nếu Ads dùng fake revenue hoặc scale khi data quality fail.

## 19.8. PHASE 7 - MC AI Live

PHASE 7 phải phụ thuộc Gateway/AI/Commerce/Ads-safe control.

P0 Fail nếu MC AI Live dùng live signal làm revenue/ROAS.

## 19.9. PHASE 8 - IVR

PHASE 8 phải phụ thuộc Official Order/Core Order State/Notification Owner.

P0 Fail nếu IVR gọi Quote/Cart/Order Draft hoặc tự gửi notification.

## 19.10. PHASE 9 - Global Release

PHASE 9 phải phụ thuộc evidence/smoke/UAT/sign-off/release decision.

P0 Fail nếu Global Gateway mở khi thiếu gate.

## 20. IMPLEMENTATION COMMAND BOUNDARY

## 20.1. Command được phép ở cấp governance

TECH-11 cho phép các command nghiệp vụ quản trị triển khai như:

Create Backlog Item.

Map Source-of-Truth.

Review Phase Entry.

Create Dev Handoff.

Approve Code Handoff.

Submit Implementation Report.

Map Evidence.

Map Smoke.

Open điểm chặn.

Submit Fix.

Require Retest.

Close điểm chặn.

Submit QA Handoff.

Review Phase Exit.

Prepare Release Handoff.

Submit To TECH-10 Review.

## 20.2. Command bị cấm ở cấp TECH-11

TECH-11 không được phát sinh command:

Create Production Code Snippet.

Directly Patch Production.

Override Business Rule.

Hardcode Runtime Policy.

Mark Release Ready.

Mark Production Ready.

Mark Go-live Approved.

Open Global Gateway.

Skip Evidence.

Skip Smoke.

Skip Owner Sign-off.

Close P0 Without Retest.

Bypass TECH-10.

## 20.3. P0 điểm chặn

FAIL nếu TECH-11 được dùng để mở release trực tiếp.

FAIL nếu TECH-11 sinh code copy-paste.

FAIL nếu TECH-11 bỏ qua TECH-10 Global Gateway.

## 21. IMPLEMENTATION QUERY BOUNDARY

## 21.1. Query được phép

TECH-11 được phép truy vấn/đối chiếu:

TECH source-of-truth.

Backlog status.

Phase status.

Handoff package.

Implementation report.

Evidence mapping.

Smoke mapping.

điểm chặn status.

Dependency status.

Cleanup/docs update status.

QA handoff status.

Release handoff status.

TECH-10 gate status.

## 21.2. Query bị hạn chế

TECH-11 không được dùng các dữ liệu sau để tự quyết định release:

Dev nói đã xong.

Demo chạy một luồng.

Dashboard nhìn đẹp.

Local test không đủ scope.

Mock upstream như production.

Evidence chưa accepted.

Smoke chưa pass.

Owner chưa sign-off.

Release decision chưa có.

## 21.3. P0 điểm chặn

FAIL nếu query từ dev claim thay evidence.

FAIL nếu local/demo được dùng làm production readiness.

FAIL nếu mock được coi là production truth.

## 22. GLOBAL GATEWAY ALIGNMENT

## 22.1. TECH-11 không mở Gateway

TECH-11 chỉ bàn giao sang TECH-10.

TECH-11 không được mở Global Gateway.

TECH-11 không được quyết định Go-live Approved.

TECH-11 không được quyết định Scale Approved.

TECH-11 chỉ được tạo trạng thái:

Ready for TECH-10 Review

## 22.2. Khi nào sang TECH-10

Chỉ sang TECH-10 khi có:

Phase exit accepted.

Evidence mapped.

Smoke mapped.

QA handoff completed nếu required.

Open P0 điểm chặn = none.

Dependency pass.

Cleanup/docs update pass.

Release handoff package complete.

## 22.3. Sau khi sang TECH-10

TECH-10 sẽ xét:

Evidence Accepted.

Global Smoke Pass.

UAT Pass nếu required.

Owner Sign-off.

Production Readiness.

Release Decision.

Global Gateway State.

Post-Go-Live Monitoring.

Scale Approval.

## 22.4. P0 điểm chặn

FAIL nếu TECH-11 gọi Ready for TECH-10 Review là Release Ready.

FAIL nếu TECH-11 bypass TECH-10.

FAIL nếu TECH-11 mở Gateway.

## 23. P0 điểm chặn REGISTRY - PHẦN 3/4

Các lỗi sau là P0 điểm chặn cấp lifecycle/state machine:

Phase không Entry Ready nhưng bắt đầu implementation.

Backlog không source nhưng Ready.

Source conflict chưa giải quyết nhưng code.

Dev handoff thiếu Scope Out/P0/Evidence/Smoke.

Code handoff thiếu architecture/test/evidence context.

Dev copy-paste code rời rạc.

Implementation report thiếu test/build/evidence.

Evidence thiếu requirement/environment nhưng accepted.

Smoke chưa chạy nhưng phase exit.

Smoke fail nhưng phase exit.

P0 điểm chặn chưa retest nhưng closed.

Cleanup fail nhưng phase exit.

QA handoff thiếu known issues/smoke list.

Release handoff thiếu evidence/smoke.

Release handoff gọi phase done là Release Ready.

Cross-phase dependency fail nhưng downstream release.

Mock upstream được dùng như production truth.

Code cũ thắng TECH mới.

Tài liệu cũ thắng TECH mới.

TECH-11 bypass TECH-10.

TECH-11 tự mở Global Gateway.

Dev hardcode policy.

Dev tự bỏ source-of-truth.

Dev tự đóng điểm chặn không evidence.

Global Gateway mở khi chưa đủ TECH-10 gate.

## 24. SMOKE ĐỊNH HƯỚNG - PHẦN 3/4

TECH11-P3-SMK-001 - Phase Entry bị chặn

Given phase thiếu source-of-truth hoặc dependency chưa pass
When Phase Entry Gate chạy
Then phase = PHASE_ENTRY_BLOCKED.

TECH11-P3-SMK-002 - Backlog Ready bị chặn

Given backlog không có TECH/section/requirement
When Backlog Governance chạy
Then backlog không được BACKLOG_READY_FOR_HANDOFF.

TECH11-P3-SMK-003 - Source Conflict

Given code cũ khác TECH mới
When Source Mapping chạy
Then SOURCE_CONFLICT_DETECTED và TECH mới thắng sau review.

TECH11-P3-SMK-004 - Handoff Rejected

Given dev handoff thiếu Scope Out và P0 điểm chặn
When Dev Handoff review
Then HANDOFF_REJECTED.

TECH11-P3-SMK-005 - Code Handoff bị chặn

Given requirement chưa rõ DB/API/UI impact
When Code Handoff Control chạy
Then CODE_HANDOFF_NOT_ALLOWED.

TECH11-P3-SMK-006 - Report Rejected

Given implementation report thiếu lệnh đã chạy và test/build result
When Report Resolver chạy
Then REPORT_REJECTED.

TECH11-P3-SMK-007 - Evidence Mapping Rejected

Given evidence không có requirement/environment
When Evidence Mapping chạy
Then EVIDENCE_REJECTED.

TECH11-P3-SMK-008 - Smoke Fail

Given smoke P0 fail
When Smoke Mapping chạy
Then STOP_POINT_OPEN và phase không exit.

TECH11-P3-SMK-009 - điểm chặn Closure bị chặn

Given điểm chặn marked fixed nhưng chưa retest
When điểm chặn Resolver chạy
Then điểm chặn không được CLOSED.

TECH11-P3-SMK-010 - QA Handoff Rejected

Given QA handoff thiếu known issues và smoke list
When QA Handoff Resolver chạy
Then QA_HANDOFF_REJECTED.

TECH11-P3-SMK-011 - Release Handoff Not Release Ready

Given phase exit accepted
When Release Handoff chạy
Then status = RELEASE_HANDOFF_READY_FOR_TECH10, không phải Release Ready.

TECH11-P3-SMK-012 - Cross-Phase Block

Given PHASE 3 muốn release nhưng PHASE 2 Sellable Gate bị chặn
When Cross-Phase Dependency Validator chạy
Then PHASE 3 bị chặn.

TECH11-P3-SMK-013 - TECH-10 Required

Given phase hoàn tất dev/QA
When chưa qua TECH-10
Then không được Go-live Approved.

TECH11-P3-SMK-014 - Global Gateway bị chặn

Given chưa có accepted evidence/smoke pass/sign-off/release decision
When Gateway status xét
Then Global Gate bị chặn.

## 25. EVIDENCE PACKAGE - PHẦN 3/4

## PHẦN 3/4 yêu cầu evidence thiết kế gồm:

Phase Lifecycle State Model accepted.

Backlog State Model accepted.

Source-of-Truth Mapping State Model accepted.

Dev Handoff State Model accepted.

Code Handoff State Model accepted.

Implementation Report State Model accepted.

Evidence Mapping State Model accepted.

Smoke Mapping State Model accepted.

điểm chặn / Retest State Model accepted.

Cleanup / Documentation Update State Model accepted.

Dev-to-QA Handoff State Model accepted.

Release Handoff State Model accepted.

Cross-Phase Dependency State Model accepted.

Fail-safe Implementation Control accepted.

Backlog-to-Code-to-Evidence Flow accepted.

Phase-Specific Lifecycle Control accepted.

Command Boundary accepted.

Query Boundary accepted.

Global Gateway Alignment accepted.

P0 điểm chặn Registry accepted.

Smoke định hướng accepted.

## 26. DONE GATE - PHẦN 3/4

## PHẦN 3/4 đạt Documentation Complete ở cấp lifecycle/state machine khi:

Đã khóa Implementation Lifecycle tổng thể.

Đã khóa Phase Lifecycle State Model.

Đã khóa Backlog State Model.

Đã khóa Source-of-Truth Mapping State Model.

Đã khóa Dev Handoff State Model.

Đã khóa Code Handoff State Model.

Đã khóa Implementation Report State Model.

Đã khóa Evidence Mapping State Model.

Đã khóa Smoke Mapping State Model.

Đã khóa điểm chặn / Retest State Model.

Đã khóa Cleanup / Documentation Update State Model.

Đã khóa Dev-to-QA Handoff State Model.

Đã khóa Release Handoff State Model.

Đã khóa Cross-Phase Dependency State Model.

Đã khóa Fail-safe Implementation Control.

Đã khóa Backlog-to-Code-to-Evidence Flow.

Đã khóa Phase-Specific Lifecycle Control.

Đã khóa Implementation Command Boundary.

Đã khóa Implementation Query Boundary.

Đã khóa Global Gateway Alignment.

Đã có P0 điểm chặn Registry.

Đã có Smoke định hướng.

Đã có Evidence Package.

Đã sẵn sàng chuyển sang PHẦN 4/4.

## 27. FAIL GATE - PHẦN 3/4

## PHẦN 3/4 FAIL nếu:

Thiếu Phase Lifecycle State Model.

Thiếu Backlog State Model.

Thiếu Source Mapping State Model.

Thiếu Dev Handoff State Model.

Thiếu Code Handoff State Model.

Thiếu Implementation Report State Model.

Thiếu Evidence Mapping State Model.

Thiếu Smoke Mapping State Model.

Thiếu điểm chặn/Retest State Model.

Thiếu Cleanup/Docs Update State Model.

Thiếu QA Handoff State Model.

Thiếu Release Handoff State Model.

Cho phép coding khi source conflict còn mở.

Cho phép copy-paste code rời rạc.

Cho phase exit khi thiếu report/evidence/smoke.

Cho đóng điểm chặn không retest.

Cho release handoff bypass TECH-10.

Gọi Ready for TECH-10 Review là Release Ready.

Gọi phase exit là Production Ready.

Không khóa Global Gate bị chặn khi thiếu TECH-10 gate.

## 28. RELEASE HANDOFF - SANG PHẦN 4/4

## PHẦN 3/4 bàn giao sang PHẦN 4/4 các nội dung đã khóa:

Implementation Lifecycle.

Phase Lifecycle State Model.

Backlog State Model.

Source-of-Truth Mapping State Model.

Dev Handoff State Model.

Code Handoff State Model.

Implementation Report State Model.

Evidence Mapping State Model.

Smoke Mapping State Model.

điểm chặn / Retest State Model.

Cleanup / Documentation Update State Model.

Dev-to-QA Handoff State Model.

Release Handoff State Model.

Cross-Phase Dependency State Model.

Fail-safe Implementation Control.

Backlog-to-Code-to-Evidence Flow.

Phase-Specific Lifecycle Control.

Command Boundary.

Query Boundary.

Global Gateway Alignment.

P0 điểm chặn Registry.

Smoke định hướng.

Evidence Package.

## PHẦN 4/4 sẽ viết:

FINAL IMPLEMENTATION SMOKE MATRIX / EVIDENCE PACKAGE / PHASE DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-11 FINAL CONCLUSION

Trong đó phải khóa rõ:

Final smoke matrix cho implementation roadmap.

Smoke theo từng phase.

Smoke backlog/source/handoff/code/report/evidence/smoke/QA/release.

Evidence package cuối cùng.

Owner review points.

Dev report review.

QA handoff review.

Release handoff review.

Final Done Gate.

Final Fail Gate.

TECH-11 Final Conclusion.

## 29. TRẠNG THÁI SAU PHẦN 3/4

Sau PHẦN 3/4:

## PHẦN 1/4 đã khóa implementation principles.

## PHẦN 2/4 đã khóa module contracts.

## PHẦN 3/4 đã khóa lifecycle, state machine, backlog-to-code-to-evidence flow và cross-phase control.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời triển khai dev cho TECH-11.

Các điểm trọng yếu đã được cố định:

Phase phải đi qua Entry Gate trước khi implementation.

Backlog phải map TECH/section/requirement mới được Ready.

Source conflict phải được review, TECH mới thắng tài liệu cũ và code cũ.

Dev handoff phải có Scope In/Scope Out/Dependency/P0/Evidence/Smoke.

Code handoff chỉ được allowed khi có đủ context thật của codebase.

Không copy-paste code rời rạc từ AI.

Implementation report phải có đủ file list, source requirement, evidence, lệnh chạy, test/build, cleanup, docs, migration/seed nếu áp dụng, risk và handoff.

Evidence phải map requirement/environment/expected/actual/reviewer.

Smoke phải map phase/backlog/requirement và có evidence.

P0 điểm chặn không được closed nếu chưa retest.

Cleanup/docs update là điều kiện phase exit.

Dev-to-QA handoff phải có known issues, smoke list, evidence, migration/seed note.

Release handoff chỉ là Ready for TECH-10 Review.

TECH-11 không tự gọi Release Ready, Production Ready hoặc Go-live Approved.

Cross-phase dependency chặn downstream khi upstream chưa pass.

Mock upstream không được dùng như production truth.

Runtime/evidence/smoke/source unavailable thì fail-safe, không tự suy đoán.

Global Gateway vẫn bị chặn cho tới khi TECH-10 đủ evidence, smoke, UAT/sign-off và release decision.

## PHẦN 3/4 sẵn sàng bàn giao sang:

## PHẦN 4/4 - FINAL IMPLEMENTATION SMOKE MATRIX / EVIDENCE PACKAGE / PHASE DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-11 FINAL CONCLUSION.

## PHẦN 4/4 - FINAL IMPLEMENTATION SMOKE MATRIX / EVIDENCE PACKAGE / PHASE DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-11 FINAL CONCLUSION

## 1. MỤC TIÊU PHẦN 4/4

## PHẦN 4/4 khóa lớp kiểm soát cuối cùng cho TECH-11.

PHẦN này xác định rõ:

Final smoke matrix cho implementation roadmap.

Smoke theo từng phase triển khai.

Smoke backlog/source/handoff/code/report/evidence/smoke/QA/release.

Evidence package cuối cùng.

Owner review points.

Dev report review.

QA handoff review.

Release handoff review.

Final Done Gate.

Final Fail Gate.

Kết luận cuối cùng của TECH-11.

## PHẦN 4/4 không viết code.

## PHẦN 4/4 không thiết kế API chi tiết.

## PHẦN 4/4 không thiết kế database chi tiết.

## PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa chuẩn kiểm soát triển khai từ tài liệu sang dev, từ dev sang QA, từ QA sang TECH-10 Global Release Governance.

## 2. FINAL IMPLEMENTATION PRINCIPLE

TECH-11 khẳng định nguyên tắc cuối cùng:

Implementation roadmap không phải implementation thật.

Dev completed không phải Release Ready.

Phase exit không phải Production Ready.

Ready for TECH-10 Review không phải Go-live Approved.

Một phase chỉ được coi là hoàn tất ở cấp implementation khi có đủ:

Source-of-truth mapping.

Backlog chuẩn.

Phase Entry Gate pass.

Dev handoff pass.

Code handoff control pass.

Implementation report pass.

Evidence mapping pass.

Smoke mapping pass.

điểm chặn/retest pass.

Cleanup/docs update pass.

Dev-to-QA handoff pass.

Phase Exit Gate pass.

Release handoff package sẵn sàng cho TECH-10.

Tuy nhiên, để release thật vẫn phải qua TECH-10:

Evidence accepted.

Global smoke pass.

UAT pass nếu required.

Owner sign-off.

No open P0 điểm chặn.

Cross-tech dependency pass.

Privacy/security pass.

Rollback/fallback ready.

Monitoring/alert ready.

Release decision.

Global Gateway approval.

Nếu chưa qua TECH-10:

Global Gate bị chặn

## 3. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM A: SOURCE-OF-TRUTH / GREENFIELD CONTROL

TECH11-FSMK-001 - TECH mới thắng tài liệu cũ

Given tài liệu cũ có rule khác TECH source-of-truth mới
When Source-of-Truth Mapping Resolver kiểm tra
Then TECH mới thắng
Must Not dùng tài liệu cũ để override TECH mới.

P0 Fail If: legacy document được dùng làm nguồn thật để phủ định TECH mới.

TECH11-FSMK-002 - TECH mới thắng code cũ

Given current implementation đang chạy khác TECH source-of-truth mới
When dev đối chiếu
Then code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY
Must Not dùng code cũ làm chuẩn nghiệp vụ.

P0 Fail If: code cũ thắng TECH mới vì lý do “đang chạy”.

TECH11-FSMK-003 - Source conflict phải mở điểm chặn

Given backlog hoặc code có mâu thuẫn source-of-truth
When Source Mapping phát hiện conflict
Then phải mở điểm chặn hoặc route owner decision
Must Not silently ignore conflict.

P0 Fail If: source conflict vẫn được đưa vào coding.

TECH11-FSMK-004 - Requirement không source không được Ready

Given requirement không có TECH/section/requirement mapping
When Backlog Governance kiểm tra
Then backlog không được Ready.

P0 Fail If: requirement không nguồn vẫn được giao dev.

## 4. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM B: PHASE ENTRY / PHASE BOUNDARY

TECH11-FSMK-005 - Phase thiếu source-of-truth bị chặn

Given phase chưa map source-of-truth
When Phase Entry Gate Resolver kiểm tra
Then PHASE_ENTRY_BLOCKED.

P0 Fail If: phase bắt đầu implementation khi source chưa rõ.

TECH11-FSMK-006 - Phase thiếu Scope In / Scope Out bị chặn

Given phase có mục tiêu nhưng chưa rõ Scope In / Scope Out
When Phase Entry Gate chạy
Then phase không được Entry Ready.

P0 Fail If: dev bắt đầu làm khi chưa rõ làm gì và không làm gì.

TECH11-FSMK-007 - Phase thiếu evidence/smoke requirement bị chặn

Given phase chưa có evidence requirement và smoke requirement
When Entry Gate kiểm tra
Then phase không được Ready.

P0 Fail If: phase bắt đầu mà không biết sẽ chứng minh pass bằng gì.

TECH11-FSMK-008 - Upstream bị chặn thì downstream bị chặn

Given upstream phase critical chưa pass
When downstream phase xin bắt đầu release-level implementation
Then downstream bị bị chặn.

P0 Fail If: phase downstream chạy thật khi upstream còn bị chặn.

## 5. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM C: BACKLOG GOVERNANCE

TECH11-FSMK-009 - Backlog mơ hồ bị reject

Given backlog ghi “làm AI”, “làm bán hàng”, “sửa cho chạy”
When Backlog Governance Resolver kiểm tra
Then backlog bị reject.

P0 Fail If: backlog mơ hồ vẫn được đưa vào Ready.

TECH11-FSMK-010 - Backlog phải gắn TECH / section / requirement

Given backlog không có TECH number, section, requirement
When review backlog
Then backlog không Ready.

P0 Fail If: dev nhận việc không biết nguồn yêu cầu ở đâu.

TECH11-FSMK-011 - Backlog phải có dependency

Given backlog thiếu upstream/downstream dependency
When backlog review
Then backlog bị Need Dependency Review.

P0 Fail If: backlog downstream không biết phụ thuộc upstream nào.

TECH11-FSMK-012 - Backlog phải có Done Gate / Fail Gate

Given backlog không có Done Gate / Fail Gate
When Backlog Governance kiểm tra
Then backlog không được Ready.

P0 Fail If: dev không biết khi nào xong, khi nào fail.

## 6. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM D: DEV HANDOFF / CODE HANDOFF CONTROL

TECH11-FSMK-013 - Dev handoff thiếu source bị reject

Given dev handoff không có source-of-truth reference
When Dev Handoff Resolver kiểm tra
Then handoff rejected.

P0 Fail If: dev triển khai theo yêu cầu không nguồn.

TECH11-FSMK-014 - Dev handoff thiếu Scope Out bị reject

Given handoff có Scope In nhưng thiếu Scope Out
When Dev Handoff review
Then handoff không Ready.

P0 Fail If: dev tự suy luận phần không được làm.

TECH11-FSMK-015 - Dev handoff thiếu P0 điểm chặn bị reject

Given handoff không nêu P0 điểm chặn
When Dev Handoff Resolver kiểm tra
Then handoff rejected.

P0 Fail If: dev không biết các lỗi cấm tuyệt đối.

TECH11-FSMK-016 - Code handoff thiếu architecture context bị chặn

Given yêu cầu đi vào code nhưng chưa rõ architecture context
When Code Handoff Control Resolver chạy
Then CODE_HANDOFF_BLOCKED.

P0 Fail If: coding agent viết code không hiểu kiến trúc project thật.

TECH11-FSMK-017 - Code handoff thiếu DB/API/UI impact bị chặn

Given requirement có thể ảnh hưởng DB/API/UI nhưng chưa review impact
When Code Handoff kiểm tra
Then không allowed coding.

P0 Fail If: dev sửa DB/API/UI ngầm mà không khai báo impact.

TECH11-FSMK-018 - Cấm copy-paste code rời rạc

Given coding request yêu cầu AI sinh code snippet để dán vào project
When Code Handoff Control kiểm tra
Then request bị reject và yêu cầu quay lại source/architecture/context.

P0 Fail If: dev copy code rời rạc vào codebase thật.

## 7. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM E: IMPLEMENTATION REPORT

TECH11-FSMK-019 - Report thiếu file list bị reject

Given dev báo xong nhưng không liệt kê file đã sửa
When Implementation Report Resolver kiểm tra
Then report rejected.

P0 Fail If: không biết dev đã sửa gì.

TECH11-FSMK-020 - Report thiếu nguồn yêu cầu bị reject

Given dev report không ghi TECH/section/requirement
When report review
Then report rejected.

P0 Fail If: không trace được code về source-of-truth.

TECH11-FSMK-021 - Report thiếu lệnh đã chạy bị reject

Given dev report không có build/test/lint/migration/seed command nếu áp dụng
When report review
Then report rejected hoặc Need More Info.

P0 Fail If: dev nói đã test nhưng không có lệnh đã chạy.

TECH11-FSMK-022 - Report thiếu test/build result bị reject

Given report không ghi backend build, frontend build, test result
When report review
Then không accepted.

P0 Fail If: implementation pass bằng lời nói.

TECH11-FSMK-023 - Report che giấu risk bị reject

Given dev report ghi “không có rủi ro” nhưng thực tế còn known issue/open điểm chặn
When reviewer kiểm tra
Then report rejected.

P0 Fail If: rủi ro bị giấu khỏi handoff.

## 8. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM F: EVIDENCE / SMOKE MAPPING

TECH11-FSMK-024 - Evidence không trace requirement bị reject

Given evidence không gắn TECH/requirement/backlog/phase
When Evidence Mapping Resolver kiểm tra
Then evidence rejected.

P0 Fail If: evidence không trace vẫn accepted.

TECH11-FSMK-025 - Evidence thiếu environment bị reject

Given evidence không ghi local/dev/staging/production-like
When evidence review
Then evidence không accepted.

P0 Fail If: evidence không environment vẫn dùng để phase exit.

TECH11-FSMK-026 - Evidence lộ dữ liệu nhạy cảm bị reject

Given evidence chứa secret/token/payment/private data/health note sai policy
When review evidence
Then evidence rejected và mở privacy/security điểm chặn nếu cần.

P0 Fail If: evidence lộ dữ liệu vẫn accepted.

TECH11-FSMK-027 - Smoke chưa chạy thì phase không exit

Given smoke bắt buộc chưa chạy
When Phase Exit Gate kiểm tra
Then PHASE_EXIT_BLOCKED.

P0 Fail If: phase exit khi smoke chưa chạy.

TECH11-FSMK-028 - Smoke fail phải mở điểm chặn

Given smoke P0 fail
When Smoke Mapping Resolver ghi nhận
Then mở P0 điểm chặn và phase exit bị chặn.

P0 Fail If: smoke fail nhưng phase vẫn accepted.

TECH11-FSMK-029 - Smoke phải có fail-case

Given requirement có các luồng chặn nhưng smoke chỉ test happy path
When Smoke Mapping review
Then smoke chưa đủ.

P0 Fail If: smoke không test fail-case nhưng được pass.

## 9. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM G: điểm chặn / RETEST / CLEANUP

TECH11-FSMK-030 - P0 điểm chặn không được đóng nếu chưa retest

Given dev đã fix P0 điểm chặn nhưng chưa có retest evidence
When điểm chặn Resolver kiểm tra
Then điểm chặn không được CLOSED.

P0 Fail If: điểm chặn đóng bằng lời nói.

TECH11-FSMK-031 - điểm chặn reopened kéo phase về bị chặn

Given điểm chặn từng closed nhưng lỗi tái xuất hiện
When điểm chặn reopened
Then phase/release handoff bị bị chặn lại.

P0 Fail If: điểm chặn reopened nhưng phase vẫn Ready.

TECH11-FSMK-032 - Cleanup fail chặn phase exit

Given còn debug bypass, code tạm nguy hiểm hoặc duplicate critical logic
When Cleanup Resolver kiểm tra
Then phase exit bị chặn.

P0 Fail If: debug bypass còn trong production path.

TECH11-FSMK-033 - Cleanup không được xóa audit/evidence

Given cleanup process xóa log/evidence/audit cần lưu
When review cleanup
Then cleanup fail.

P0 Fail If: cleanup làm mất bằng chứng kiểm tra.

TECH11-FSMK-034 - Documentation update bắt buộc khi behavior thay đổi

Given implementation làm thay đổi behavior/config/smoke/handoff
When Documentation Update Resolver kiểm tra
Then docs/handoff phải cập nhật.

P0 Fail If: tài liệu/handoff sai lệch làm phase sau hiểu sai.

## 10. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM H: DEV-TO-QA HANDOFF

TECH11-FSMK-035 - QA handoff thiếu requirement list bị reject

Given QA handoff không có requirement list
When Dev-to-QA Handoff Resolver kiểm tra
Then handoff rejected.

P0 Fail If: QA không biết test theo yêu cầu nào.

TECH11-FSMK-036 - QA handoff thiếu smoke list bị reject

Given QA handoff không có smoke list
When handoff review
Then QA handoff không Ready.

P0 Fail If: QA không biết smoke P0 cần chạy.

TECH11-FSMK-037 - QA handoff thiếu known issues bị reject

Given implementation có known issues nhưng QA handoff không ghi
When review
Then handoff rejected.

P0 Fail If: known issues bị giấu khỏi QA.

TECH11-FSMK-038 - QA handoff thiếu migration/seed note bị reject nếu có impact

Given implementation có migration/seed impact
When QA handoff không ghi note
Then handoff không Ready.

P0 Fail If: QA test sai dữ liệu vì thiếu seed/migration context.

## 11. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM I: CROSS-PHASE DEPENDENCY

TECH11-FSMK-039 - PHASE 0 chặn toàn bộ high-risk phase

Given Foundation/RBAC/Audit/Evidence chưa pass
When phase high-risk muốn release-level implementation
Then downstream bị chặn.

P0 Fail If: high-risk command không có audit/evidence foundation.

TECH11-FSMK-040 - PHASE 2 chặn PHASE 3 nếu Operational bị chặn

Given Operational Sellable Gate chưa pass hoặc Sale Lock/Recall chưa đúng
When Commerce muốn release
Then PHASE 3 bị chặn.

P0 Fail If: Commerce bán khi Operational bị chặn.

TECH11-FSMK-041 - PHASE 3 chặn AI/Ads/IVR nếu Commerce chưa pass

Given QuoteSnapshot/Official Order/Payment/Verified Revenue chưa pass
When AI/Ads/IVR muốn dùng Commerce
Then downstream bị chặn.

P0 Fail If: AI báo giá, Ads đo revenue hoặc IVR gọi khi Commerce boundary chưa pass.

TECH11-FSMK-042 - PHASE 5 chặn MC AI Live nếu Gateway chưa pass

Given Gateway public/private routing hoặc Final Response Guard chưa pass
When MC AI Live muốn go-live
Then PHASE 7 bị chặn.

P0 Fail If: MC AI Live phát live response khi Gateway/Guard chưa pass.

TECH11-FSMK-043 - Mock upstream không phải production truth

Given downstream đang dùng mock upstream
When xét release handoff
Then không được coi là production-ready dependency.

P0 Fail If: mock được dùng như source-of-truth production.

## 12. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM J: RELEASE HANDOFF / TECH-10 ALIGNMENT

TECH11-FSMK-044 - Phase exit không phải Release Ready

Given phase exit accepted
When Release Handoff Resolver tạo package
Then status chỉ là Ready for TECH-10 Review.

P0 Fail If: phase exit được gọi là Release Ready.

TECH11-FSMK-045 - Release handoff thiếu evidence/smoke bị reject

Given release handoff package thiếu evidence hoặc smoke mapping
When resolver kiểm tra
Then handoff rejected hoặc Need More Evidence.

P0 Fail If: thiếu evidence/smoke vẫn gửi TECH-10 như hoàn chỉnh.

TECH11-FSMK-046 - Release handoff không được bypass TECH-10

Given phase đã hoàn tất dev/QA
When cần xét release
Then phải bàn giao TECH-10, không tự release.

P0 Fail If: TECH-11 tự mở release/gateway.

TECH11-FSMK-047 - TECH-10 bị chặn thì phase không release

Given TECH-10 Global Gateway vẫn bị chặn
When release status được xét
Then phase không được Go-live Approved.

P0 Fail If: go-live khi TECH-10 còn bị chặn.

TECH11-FSMK-048 - Documentation Complete không phải Dev Ready

Given TECH-11 đã viết xong tài liệu
When xét implementation status
Then chỉ là Documentation Complete, chưa Dev Ready nếu chưa có backlog/handoff/evidence/smoke thật.

P0 Fail If: tài liệu hoàn chỉnh được hiểu là dev đã sẵn sàng triển khai ngay không cần phase planning.

## 13. FINAL IMPLEMENTATION SMOKE MATRIX - NHÓM K: NO-COPY-PASTE / HUMAN ROLE BOUNDARY

TECH11-FSMK-049 - Owner quyết định muốn gì

Given requirement nghiệp vụ chưa rõ
When dev/coding agent muốn tự quyết
Then phải route owner clarification hoặc điểm chặn
Must Not dev tự quyết nghiệp vụ.

P0 Fail If: dev tự thay business rule.

TECH11-FSMK-050 - Dev quyết định làm như thế nào trong codebase thật

Given requirement đã rõ
When triển khai kỹ thuật
Then dev thiết kế theo architecture/codebase thật
Must Not owner/AI ép copy snippet không context.

P0 Fail If: code implementation bị ép theo snippet rời rạc không khớp codebase.

TECH11-FSMK-051 - AI không thay developer

Given AI tạo gợi ý triển khai
When dev nhận task
Then dev phải phân tích trong project thật, test/build/evidence
Must Not coi AI output là production code hoàn chỉnh.

P0 Fail If: AI-generated code được dán thẳng không review/test/context.

TECH11-FSMK-052 - Không hardcode runtime policy

Given giá, khuyến mãi, member benefit, Diamond, shipping, payment, IVR attempts, notification policy
When dev triển khai
Then phải lấy từ runtime/config/source-of-truth phù hợp
Must Not hardcode nếu policy chưa khóa runtime.

P0 Fail If: hardcode policy quan trọng trong code.

## 14. FINAL EVIDENCE PACKAGE - TECH-11

TECH-11 yêu cầu final evidence package cho implementation governance gồm các nhóm sau.

## 14.1. Documentation Evidence

TECH-11 PHẦN 1/4 accepted.

TECH-11 PHẦN 2/4 accepted.

TECH-11 PHẦN 3/4 accepted.

TECH-11 PHẦN 4/4 accepted.

Implementation Principles accepted.

No-Copy-Paste-Code Rule accepted.

Phase Boundary accepted.

Dev Handoff Governance accepted.

Code Handoff Control accepted.

Final Smoke Matrix accepted.

## 14.2. Phase Planning Evidence

Phase ID.

Phase name.

Phase source-of-truth.

Phase scope.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

Dev owner.

QA owner.

Evidence requirement.

Smoke requirement.

P0 điểm chặn list.

Entry Gate decision.

Exit Gate decision.

## 14.3. Backlog Evidence

Backlog ID.

Phase mapping.

TECH reference.

Section/requirement.

Owner.

Priority.

Dependency.

Scope In/Scope Out.

Evidence required.

Smoke required.

Done Gate.

Fail Gate.

Handoff target.

Ready/bị chặn decision.

## 14.4. Source Mapping Evidence

Requirement ID.

TECH number.

Section.

Active source-of-truth.

Legacy reference status.

Current implementation status.

Conflict/deviation nếu có.

Decision.

Reviewer.

Audit note.

## 14.5. Dev Handoff Evidence

Handoff ID.

Phase.

Module.

Source-of-truth.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence required.

Smoke required.

Report format.

No-hardcode warning.

No-copy-paste-code warning.

Dev acceptance.

## 14.6. Code Handoff Evidence

Code handoff ID.

Requirement mapping.

Architecture context note.

DB impact note.

API impact note.

UI impact note.

Test expectation.

Evidence expectation.

Dependency check.

Source conflict check.

Code handoff decision.

Reviewer.

## 14.7. Implementation Report Evidence

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Backend build result.

Frontend build result.

Cleanup result.

Markdown/docs update.

Database migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Handoff update.

Reviewer decision.

## 14.8. Evidence Mapping Evidence

Evidence ID.

Phase.

Backlog.

TECH reference.

Requirement.

Environment.

Expected result.

Actual result.

Pass/fail.

Timestamp.

Reviewer/tester.

Correlation/audit ID nếu áp dụng.

Sensitive data check.

Decision.

## 14.9. Smoke Mapping Evidence

Smoke ID.

Phase.

Backlog.

TECH requirement.

Scenario.

Given/When/Then.

Must Not.

Expected result.

Actual result.

Evidence.

Pass/fail.

Retest status.

Reviewer.

## 14.10. điểm chặn / Retest Evidence

điểm chặn ID.

Phase.

Backlog.

Severity.

Description.

Owner.

Detection evidence.

Fix evidence.

Retest evidence.

Acceptance decision.

Closure status.

Reopen status nếu có.

## 14.11. Cleanup / Documentation Update Evidence

Cleanup checklist.

File rác/code tạm/debug bypass status.

Duplicate/dead code status.

Docs update list.

README/handoff update list.

Migration/seed note.

Remaining cleanup note.

Reviewer decision.

## 14.12. Dev-to-QA Handoff Evidence

QA handoff ID.

Phase.

Module.

Requirement list.

File list.

Test/build result.

Smoke list.

Evidence list.

Known issues.

P0 điểm chặn status.

Migration/seed note.

Test data note.

Retest scope.

QA acceptance.

## 14.13. Release Handoff Evidence

Release handoff ID.

Phase exit decision.

Implementation report accepted.

Evidence mapping list.

Smoke mapping list.

QA handoff status.

Cleanup/docs update status.

Cross-phase dependency status.

Open risk list.

P0 điểm chặn status.

Ready for TECH-10 Review status.

TECH-10 handoff note.

## 15. OWNER REVIEW POINTS

## 15.1. Implementation Roadmap Owner

Implementation Roadmap Owner phải xác nhận:

Phase order đúng.

Không nhảy phase.

Upstream/downstream dependency rõ.

Phase Registry đầy đủ.

Entry Gate rõ.

Exit Gate rõ.

Release handoff sang TECH-10 rõ.

Không gọi phase exit là Release Ready.

## 15.2. Product / Operational / Commerce Owner

Các owner domain phải xác nhận:

Phase liên quan đúng source-of-truth.

Không dùng tài liệu cũ làm chuẩn.

Không dùng code cũ trái TECH mới.

Backlog domain đúng requirement.

Smoke domain đủ pass-case/fail-case.

P0 điểm chặn domain được ghi nhận.

Handoff sang phase sau đúng dependency.

## 15.3. Dev Lead Owner

Dev Lead phải xác nhận:

Dev handoff đủ context.

Code handoff không phải snippet rời rạc.

Implementation đi trong codebase thật.

Không hardcode policy.

Build/test command rõ.

File impact rõ.

Migration/seed impact rõ.

Known risks được báo cáo.

## 15.4. QA Owner

QA Owner phải xác nhận:

QA handoff đủ requirement.

Smoke list đầy đủ.

Known issues rõ.

P0 điểm chặn rõ.

Retest scope rõ.

Evidence đủ để test.

Không pass bằng cảm tính.

## 15.5. Evidence / Audit Owner

Evidence/Audit Owner phải xác nhận:

Evidence map requirement.

Evidence rõ environment.

Evidence có expected/actual.

Evidence có reviewer.

Evidence không lộ dữ liệu nhạy cảm.

High-risk action có audit/correlation nếu áp dụng.

Evidence đủ để sang TECH-10 review.

## 15.6. Release Owner

Release Owner phải xác nhận:

TECH-11 chỉ tạo Ready for TECH-10 Review.

TECH-11 không tự Release Ready.

TECH-11 không tự Go-live Approved.

TECH-11 không mở Global Gateway.

Release handoff package đủ.

TECH-10 sẽ xét evidence/smoke/UAT/sign-off/release decision.

Global Gateway vẫn bị chặn nếu chưa đủ TECH-10 gate.

## 16. FINAL REVIEW GATES

## 16.1. Phase Entry Review Gate

PASS khi:

Source-of-truth rõ.

Scope rõ.

Dependency rõ.

Backlog rõ.

Owner rõ.

Evidence requirement rõ.

Smoke requirement rõ.

P0 điểm chặn rõ.

Handoff từ phase trước rõ nếu có.

FAIL nếu phase bắt đầu bằng yêu cầu mơ hồ hoặc thiếu upstream dependency.

## 16.2. Backlog Review Gate

PASS khi:

Backlog có TECH reference.

Có section/requirement.

Có phase.

Có owner.

Có dependency.

Có Scope In/Scope Out.

Có evidence/smoke.

Có Done/Fail Gate.

FAIL nếu backlog mơ hồ hoặc không source.

## 16.3. Dev Handoff Review Gate

PASS khi:

Handoff đủ phase/module/source.

Scope In/Scope Out rõ.

Dependency rõ.

P0 điểm chặn rõ.

Evidence/smoke rõ.

Report format rõ.

No-copy-paste-code rule rõ.

No-hardcode-policy rule rõ.

FAIL nếu dev phải tự suy luận.

## 16.4. Code Handoff Review Gate

PASS khi:

Requirement rõ.

Source mapping accepted.

Architecture context đủ.

DB/API/UI impact đã review.

Test/evidence expectation rõ.

Dependency pass.

Không source conflict.

Không snippet rời rạc.

FAIL nếu yêu cầu coding không đủ context.

## 16.5. Implementation Report Review Gate

PASS khi report đủ 14 mục:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Backend build.

Frontend build.

Cleanup.

Markdown/docs.

Migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Handoff.

FAIL nếu thiếu test/build/evidence/source/risk.

## 16.6. Evidence Review Gate

PASS khi:

Evidence map requirement.

Có environment.

Có expected/actual.

Có timestamp.

Có reviewer.

Không lộ dữ liệu cấm.

Có audit/correlation nếu cần.

FAIL nếu evidence không trace được requirement.

## 16.7. Smoke Review Gate

PASS khi:

Smoke có ID.

Map phase/backlog/requirement.

Có scenario.

Có Given/When/Then.

Có Must Not.

Có expected/actual.

Có evidence.

Pass/fail rõ.

Fail-case/P0 case đủ nếu requirement yêu cầu.

FAIL nếu smoke chỉ happy path hoặc thiếu evidence.

## 16.8. QA Handoff Review Gate

PASS khi:

Requirement list có.

File list có.

Test/build result có.

Smoke list có.

Evidence list có.

Known issues có.

P0 điểm chặn status có.

Migration/seed note có nếu áp dụng.

FAIL nếu QA không đủ dữ liệu để test.

## 16.9. Phase Exit Review Gate

PASS khi:

Implementation report accepted.

Evidence mapped.

Smoke pass.

điểm chặn closed.

Cleanup/docs update pass.

QA handoff completed nếu required.

Dependency pass.

Handoff sang TECH-10 ready.

FAIL nếu thiếu bất kỳ điều kiện critical nào.

## 16.10. Release Handoff Review Gate

PASS khi:

Phase exit accepted.

Evidence/smoke package đầy đủ.

QA handoff đầy đủ.

No open P0 điểm chặn.

Dependency pass.

Known risk list có.

Status ghi rõ Ready for TECH-10 Review.

FAIL nếu gọi là Release Ready hoặc bypass TECH-10.

## 17. FINAL DONE GATE - TECH-11

TECH-11 đạt DOCUMENTATION COMPLETE khi:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Implementation Principles đã khóa.

Phase Boundary đã khóa.

No-Copy-Paste-Code Rule đã khóa.

Dev Handoff Governance đã khóa.

Code Handoff Control đã khóa.

Backlog Governance đã khóa.

Source-of-Truth Mapping đã khóa.

Phase Entry Gate đã khóa.

Phase Exit Gate đã khóa.

Implementation Report Format đã khóa.

Evidence Mapping đã khóa.

Smoke Mapping đã khóa.

Cross-Phase Dependency đã khóa.

Risk/điểm chặn/Retest đã khóa.

Cleanup/Documentation Update đã khóa.

Dev-to-QA Handoff đã khóa.

Release Handoff sang TECH-10 đã khóa.

Lifecycle/State Machine đã khóa.

Final Smoke Matrix đã khóa.

Final Evidence Package đã khóa.

Owner Review Points đã khóa.

Final Review Gates đã khóa.

Final Done Gate đã khóa.

Final Fail Gate đã khóa.

Final Conclusion đã khóa.

## 18. FINAL FAIL GATE - TECH-11

TECH-11 FAIL nếu còn bất kỳ điểm nào sau:

Cho phép copy-paste code rời rạc từ AI.

Cho phép dev tự suy luận nghiệp vụ.

Cho tài liệu cũ thắng TECH mới.

Cho code cũ thắng TECH mới.

Cho phase bắt đầu khi thiếu source-of-truth.

Cho phase bắt đầu khi upstream critical bị chặn.

Cho backlog mơ hồ vào Ready.

Cho backlog không evidence/smoke vào Ready.

Cho handoff thiếu Scope Out.

Cho handoff thiếu P0 điểm chặn.

Cho code handoff thiếu architecture context.

Cho code handoff thiếu DB/API/UI impact review nếu có ảnh hưởng.

Cho implementation report thiếu file list.

Cho implementation report thiếu nguồn yêu cầu.

Cho implementation report thiếu lệnh đã chạy.

Cho implementation report thiếu test/build result.

Cho implementation report thiếu evidence.

Cho evidence không trace requirement accepted.

Cho evidence thiếu environment accepted.

Cho evidence lộ dữ liệu nhạy cảm accepted.

Cho smoke chưa chạy mà phase exit.

Cho smoke fail mà phase exit.

Cho P0 điểm chặn closed khi chưa retest.

Cho cleanup fail mà phase exit.

Cho QA handoff thiếu known issues/smoke list.

Cho release handoff thiếu evidence/smoke.

Cho release handoff gọi phase exit là Release Ready.

Cho TECH-11 bypass TECH-10.

Cho TECH-11 mở Global Gateway.

Cho phase done là Production Ready.

Cho mock upstream làm production truth.

Cho dev hardcode policy runtime.

Cho release khi TECH-10 chưa pass.

Không có final smoke matrix.

Không có final evidence package.

## 19. RELEASE HANDOFF - SAU TECH-11

## 19.1. Trạng thái sau khi hoàn tất TECH-11

Sau khi hoàn tất PHẦN 4/4:

Nhưng:

Global Gate bị chặn

Lý do:

TECH-11 mới là tài liệu roadmap/governance.

Chưa có backlog thật.

Chưa có dev implementation thật.

Chưa có evidence thật.

Chưa có smoke thật.

Chưa có QA handoff thật.

Chưa có TECH-10 release review thật.

## 19.2. Điều kiện nâng lên Implementation Ready

Muốn nâng lên Implementation Ready cho một phase cần:

Phase scope rõ.

Source-of-truth mapping accepted.

Backlog chuẩn.

Phase Entry Gate pass.

Dev owner assigned.

QA owner assigned.

Evidence/smoke requirement rõ.

P0 điểm chặn list rõ.

Dev handoff ready.

Nếu chưa có các điều kiện trên:

## 19.3. Điều kiện nâng lên Dev Ready

Muốn nâng lên Dev Ready cho một backlog cần:

Backlog Ready for Handoff.

Dev handoff accepted.

Code handoff allowed.

Architecture context rõ.

DB/API/UI impact rõ hoặc không áp dụng.

Test/evidence expectation rõ.

Không source conflict.

Không upstream điểm chặn.

Nếu chưa đủ:

## 19.4. Điều kiện nâng lên Phase Exit Accepted

Muốn phase exit accepted cần:

Implementation report accepted.

Evidence mapped.

Smoke pass.

QA handoff completed nếu required.

Cleanup/docs update pass.

P0 điểm chặn closed.

Cross-phase dependency pass.

Release handoff package prepared.

Nếu chưa đủ:

## 19.5. Điều kiện nâng lên Ready for TECH-10 Review

Muốn Ready for TECH-10 Review cần:

Phase Exit Accepted.

Evidence package đủ.

Smoke package đủ.

QA handoff đủ.

Known risks rõ.

Dependency pass.

No open P0 điểm chặn.

Release handoff package hoàn chỉnh.

Nếu chưa đủ:

## 19.6. Điều kiện nâng lên Release Ready

Release Ready không thuộc quyền quyết định của TECH-11.

Release Ready chỉ được xét bởi TECH-10 khi có:

Evidence accepted.

Global smoke pass.

UAT pass nếu required.

Owner sign-off.

No open P0 điểm chặn.

Dependency pass.

Privacy/security pass.

Rollback/fallback ready nếu production scope.

Monitoring/alert ready nếu go-live scope.

Release decision phù hợp.

Nếu chưa qua TECH-10:

## 20. HANDOFF SANG CÁC BƯỚC TIẾP THEO

TECH-11 bàn giao sang các bước thực tế như sau.

## 20.1. Bước tiếp theo 1 - Lập Phase Backlog Pack

Sau TECH-11, bước hợp lý tiếp theo là viết:

Mục tiêu:

Biến PHASE 0 -> PHASE 9 thành backlog cụ thể.

Mỗi backlog gắn TECH/section/requirement.

Mỗi backlog có dependency.

Mỗi backlog có evidence/smoke.

Mỗi backlog có Done Gate/Fail Gate.

Không để dev tự chia việc.

## 20.2. Bước tiếp theo 2 - Viết Prompt giao Codex/Copilot theo từng phase

Sau khi có backlog, mới viết prompt giao coding agent.

Prompt phải theo từng phase, không một prompt làm hết.

Mỗi prompt phải có:

Phase.

Module.

Source-of-truth.

Scope In.

Scope Out.

Files to inspect.

Rules not to violate.

Evidence required.

Test/build commands expected.

Report format.

No hardcode.

No source-of-truth change.

No release claim.

## 20.3. Bước tiếp theo 3 - Dev thực hiện PHASE 0 trước

Phase đầu tiên nên là:

PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency

Không nên nhảy vào AI/Gateway/Ads/Live/IVR trước.

Vì nếu không có foundation:

Không audit được.

Không evidence được.

Không permission được.

Không idempotency được.

Không kiểm soát high-risk actions được.

Các phase sau dễ sai và khó debug.

## 20.4. Bước tiếp theo 4 - Sau mỗi phase phải có implementation report

Dev phải báo cáo đúng format đã khóa:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown.

Database migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

Thiếu bất kỳ phần quan trọng nào thì report không accepted.

Hạng mục

Trạng thái sau TECH-11

TECH-11 Documentation

Implementation Ready

KHONG

Dev Ready

KHONG

Phase Ready

KHONG

Phase Exit Accepted

KHONG

Ready for TECH-10 Review

KHONG

Release Ready

KHONG

Production Ready

KHONG

Go-live Approved

KHONG

Scale Approved

KHONG

Global Gateway

bị chặn

Backlog Package

## REQUIRED

Dev Handoff Package

## REQUIRED

Code Handoff Approval

## REQUIRED PER BACKLOG

Implementation Report

## REQUIRED AFTER DEV

Evidence Mapping

## REQUIRED

Smoke Mapping

## REQUIRED

QA Handoff

## REQUIRED IF SCOPE APPLIES

TECH-10 Review

## REQUIRED BEFORE RELEASE

## TECH-11 FINAL CONCLUSION

TECH-11 đã khóa lớp Implementation Roadmap / Dev Phase Plan / Backlog Governance / Code Handoff Control cho hệ thống Ginsengfood.

TECH-11 không thay thế TECH-00 -> TECH-10.

TECH-11 không viết code.

TECH-11 không thiết kế API/DB/UI chi tiết.

TECH-11 không cho phép xây hệ thống bằng copy-paste code rời rạc từ AI.

Các nguyên tắc cuối cùng đã khóa:

TECH source-of-truth mới thắng tài liệu cũ.

TECH source-of-truth mới thắng code cũ.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Không triển khai kiểu chỉnh sửa tạm lẻ.

Không copy-paste code rời rạc.

Không để dev tự suy luận nghiệp vụ.

Owner quyết định muốn gì.

Dev quyết định làm như thế nào trong codebase thật.

AI hỗ trợ tài liệu, checklist, prompt, review, không thay developer.

Phase phải có Entry Gate.

Phase phải có Exit Gate.

Backlog phải gắn TECH/section/requirement.

Backlog phải có Scope In/Scope Out/Dependency/Evidence/Smoke.

Dev handoff phải đầy đủ source, boundary, P0, evidence, smoke.

Code handoff chỉ allowed khi đủ architecture context, DB/API/UI impact, test expectation và evidence expectation.

Dev report phải theo format cố định 14 mục.

Evidence phải map requirement/environment/expected/actual/reviewer.

Smoke phải map phase/backlog/requirement và có evidence.

P0 điểm chặn không được closed nếu chưa retest.

Cleanup/docs update là điều kiện phase exit.

Dev-to-QA handoff phải có known issues, smoke list, evidence, migration/seed note.

Cross-phase dependency chặn downstream khi upstream chưa pass.

Mock upstream không được dùng như production truth.

Release handoff chỉ là Ready for TECH-10 Review.

TECH-11 không tự gọi Release Ready.

TECH-11 không tự gọi Production Ready.

TECH-11 không tự Go-live Approved.

TECH-11 không tự mở Global Gateway.

TECH-10 mới xét evidence accepted, global smoke pass, UAT, owner sign-off, release decision và Global Gateway.

Trạng thái cuối cùng của tài liệu:

Nhưng trạng thái triển khai vẫn là:

Bước tiếp theo hợp lý sau TECH-11 là viết:

để chuyển PHASE 0 -> PHASE 9 thành backlog cụ thể cho dev triển khai theo đúng source-of-truth, đúng dependency, đúng evidence và đúng smoke.
