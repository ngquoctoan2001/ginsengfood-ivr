# TECH-13 - CODEX COPILOT DEV PROMPT PACK PHASE EXECUTION HANDOFF IMPLEMENTATION REPORT TEMPLATE

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## CODING AGENT GOVERNANCE / PHASE EXECUTION PROMPTS / DEV HANDOFF CONTROL / IMPLEMENTATION REPORT STANDARD

## PHẦN 1/4 - DEV PROMPT PACK PRINCIPLES / AGENT BOUNDARY / NO-CODE-SNIPPET RULE / PHASE EXECUTION HANDOFF GOVERNANCE

## 1. MỤC TIÊU CỦA TECH-13

TECH-13 là tài liệu tạo bộ prompt/handoff giao dev, Codex, Copilot hoặc coding agent triển khai hệ thống Ginsengfood theo từng phase.

TECH-13 không thay thế TECH-00 -> TECH-12.

TECH-13 không viết code trong tài liệu này.

TECH-13 không thiết kế API chi tiết.

TECH-13 không thiết kế database chi tiết.

TECH-13 không thiết kế UI chi tiết.

TECH-13 có nhiệm vụ khóa:

Cách viết prompt giao dev/coding agent.

Cách chia prompt theo PHASE 0 -> PHASE 9.

Cách yêu cầu coding agent đọc source-of-truth trước khi sửa code.

Cách yêu cầu coding agent inspect codebase thật trước khi đề xuất sửa.

Cách cấm coding agent copy-paste code rời rạc.

Cách cấm coding agent hardcode policy.

Cách yêu cầu coding agent báo cáo theo format 14 mục.

Cách yêu cầu coding agent nộp evidence.

Cách yêu cầu coding agent chạy test/build.

Cách yêu cầu coding agent không gọi dev done là release ready.

Cách yêu cầu coding agent không tự mở Global Gateway.

Cách bàn giao từng phase sang TECH-11 và TECH-10.

TECH-13 là cầu nối giữa:

## TECH-12 - Phase Backlog Pack

và

Dev/Codex/Copilot triển khai trong codebase thật.

## 2. VAI TRÒ CỦA TECH-13 TRONG BỘ TECH

TECH-13 đứng sau:

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

## TECH-11 - Implementation Roadmap / Dev Phase Plan / Backlog Governance / Code Handoff Control.

## TECH-12 - Phase Backlog Pack / Dev Task Breakdown / Source-to-Backlog Matrix.

TECH-12 đã khóa backlog.

TECH-13 khóa prompt giao việc cho dev/coding agent.

Nói cách khác:

TECH-10 khóa release/go-live gate.

TECH-11 khóa cách triển khai theo phase.

TECH-12 khóa backlog/task theo phase.

TECH-13 khóa prompt/handoff để dev hoặc coding agent thực hiện từng phase.

## 3. TECH-13 KHÔNG PHẢI TÀI LIỆU CODE

TECH-13 không tạo code mẫu.

TECH-13 không tạo snippet để copy.

TECH-13 không yêu cầu dán code vào project.

TECH-13 không thay developer.

TECH-13 chỉ viết prompt chuẩn để giao việc cho dev hoặc coding agent.

Một prompt đúng không phải là:

“Viết code cho chức năng này.”

“Tạo API này.”

“Tạo bảng này.”

“Dán đoạn code này vào.”

“Làm nhanh cho chạy.”

“Fix lỗi này bằng mọi cách.”

“Copy code từ AI vào project.”

Một prompt đúng phải yêu cầu coding agent:

Đọc source-of-truth.

Inspect codebase thật.

Xác định current implementation state.

Phát hiện conflict giữa code cũ và TECH mới.

Đề xuất implementation plan.

Nêu file dự kiến sửa.

Nêu rủi ro.

Chỉ sửa trong phạm vi phase/backlog.

Chạy test/build.

Nộp evidence.

Báo cáo theo format 14 mục.

Không tự tuyên bố release ready.

## 4. NO-CODE-SNIPPET RULE

## 4.1. Không giao coding agent bằng snippet rời rạc

Không được yêu cầu coding agent:

Viết vài đoạn code rời.

Copy vào file bất kỳ.

Tự đoán kiến trúc.

Tự đoán database.

Tự đoán API.

Tự đoán UI.

Tự đoán policy.

Tự đoán state machine.

Tự đoán permission.

Tự đoán evidence.

Hệ thống Ginsengfood có nhiều domain liên kết chặt:

Foundation.

Product.

Operational.

Commerce.

AI Advisor.

Gateway.

Ads.

MC AI Live.

## IVR.

Global Release.

Vì vậy, không thể triển khai bằng code snippet rời rạc.

## 4.2. Coding agent phải làm trong codebase thật

Khi đến bước được phép code, coding agent phải:

Inspect cấu trúc project.

Đọc file hiện có.

Tìm pattern đang dùng.

Xác định module liên quan.

Xác định dependency.

Xác định test hiện có.

Xác định build command.

Xác định migration/seed impact nếu có.

Xác định docs/handoff cần update.

Chỉ sửa trong phạm vi được giao.

## 4.3. Không được dùng AI code thay thiết kế hệ thống

AI/coding agent có thể hỗ trợ triển khai.

Nhưng không được thay:

Kiến trúc sư hệ thống.

Developer.

## QA.

Owner nghiệp vụ.

Security reviewer.

Release owner.

Prompt phải luôn giữ nguyên tắc:

Owner quyết định muốn gì.

Developer quyết định làm như thế nào trong codebase thật.

AI/coding agent hỗ trợ triển khai có kiểm soát, không tự đổi nghiệp vụ.

## 5. AGENT BOUNDARY

## 5.1. Coding agent được phép làm gì

Coding agent được phép:

Đọc source-of-truth được giao.

Inspect codebase hiện tại.

Tìm file liên quan.

Phân tích khoảng cách giữa code hiện tại và TECH mới.

Đề xuất implementation plan.

Sửa code trong phạm vi được giao nếu được phép.

Thêm/chỉnh test nếu phù hợp.

Chạy build/test/lint nếu có lệnh.

Ghi nhận lỗi build/test.

Cập nhật docs/handoff nếu được yêu cầu.

Nộp implementation report.

Nêu rủi ro còn lại.

## 5.2. Coding agent không được làm gì

Coding agent không được:

Tự đổi business rule.

Tự đổi source-of-truth.

Tự dùng tài liệu cũ để phủ định TECH mới.

Tự dùng code cũ để phủ định TECH mới.

Tự hardcode policy.

Tự bỏ audit.

Tự bỏ evidence.

Tự bỏ smoke.

Tự bỏ permission.

Tự bỏ idempotency.

Tự bỏ state machine.

Tự bỏ privacy/security rule.

Tự sửa ngoài scope.

Tự tạo release decision.

Tự gọi release ready.

Tự gọi production ready.

Tự mở Global Gateway.

## 5.3. Khi agent gặp mâu thuẫn

Nếu agent phát hiện mâu thuẫn giữa:

TECH mới và tài liệu cũ.

TECH mới và code cũ.

Source-of-truth và implementation hiện tại.

Backlog và codebase.

Policy và config hiện có.

Test hiện có và requirement mới.

Agent phải:

Ghi rõ conflict.

Không tự chọn theo cảm tính.

Không âm thầm sửa lệch rule.

Không giữ code cũ nếu trái TECH mới.

Đề xuất điểm chặn.

Báo cáo trong phần “Rủi ro còn lại” hoặc “điểm chặn”.

## 6. PROMPT PACK PHẢI CHIA THEO PHASE

TECH-13 bắt buộc chia prompt theo PHASE 0 -> PHASE 9.

Không dùng một prompt làm toàn bộ hệ thống.

Không dùng một prompt “triển khai Ginsengfood”.

Không dùng một prompt “đọc toàn bộ TECH rồi code hết”.

Mỗi prompt phải đi theo một phase hoặc một nhóm backlog rất rõ.

Danh sách phase:

PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency

PHASE 1 - Product / SKU / Recipe / Product Activation

PHASE 2 - Operational Core

PHASE 3 - Commerce Runtime

PHASE 4 - AI Advisor Runtime

PHASE 5 - Facebook Gateway / Messenger / Channel Delivery

PHASE 6 - Ads Measurement

PHASE 7 - MC AI Live

PHASE 8 - IVR Order Confirmation

PHASE 9 - Global Smoke / UAT / Release Gateway

## 7. PHASE EXECUTION PROMPT STRUCTURE

Mỗi prompt giao dev/coding agent phải có cấu trúc chuẩn.

## 7.1. Header bắt buộc

Mỗi prompt phải bắt đầu bằng:

Tên dự án.

Phase.

Module/backlog.

Mục tiêu.

Source-of-truth.

Trạng thái gateway.

Cảnh báo không được gọi release ready.

Mẫu header:

DỰ ÁN: Ginsengfood - Greenfield Technical Implementation
PHASE: {{phase_id}} - {{phase_name}}
BACKLOG: {{backlog_id}} - {{backlog_name}}
SOURCE-OF-TRUTH: {{tech_refs}}
MỤC TIÊU: {{goal}}
TRẠNG THÁI: Implementation task only. Not Release Ready. Global Gateway remains bị chặn.

## 7.2. Source-of-truth bắt buộc

Prompt phải ghi rõ:

TECH liên quan.

Section liên quan.

Rule P0 liên quan.

Dependency liên quan.

Evidence/smoke liên quan.

Không được nói chung chung:

“Đọc tài liệu liên quan.”

Phải nói cụ thể:

“Đọc TECH-04 Commerce Runtime phần QuoteSnapshot, Order Draft, Payment Boundary và TECH-07 Verified Revenue nếu backlog liên quan Ads.”

## 7.3. Scope In bắt buộc

Prompt phải liệt kê rõ việc cần làm.

Ví dụ:

Inspect module hiện tại.

Xác định current implementation.

So sánh với TECH source-of-truth.

Đề xuất implementation plan.

Sửa đúng phạm vi nếu được phép.

Chạy test/build.

Nộp report.

## 7.4. Scope Out bắt buộc

Prompt phải liệt kê rõ việc không làm.

Ví dụ:

Không tự thiết kế lại domain ngoài scope.

Không hardcode policy.

Không tự sửa giá.

Không tự tạo order.

Không bypass audit/evidence.

Không mở Gateway.

Không gọi Production Ready.

## 7.5. Dependency bắt buộc

Prompt phải ghi:

Upstream dependency.

Downstream dependency.

Dependency nào đang required.

Dependency nào chỉ planning-only.

Dependency nào bị chặn.

## 7.6. P0 điểm chặn bắt buộc

Prompt phải nêu rõ lỗi cấm tuyệt đối.

Ví dụ với Commerce:

Không QuoteSnapshot mà vẫn final price.

Cart bị hiểu là order.

Order Draft bị hiểu là Official Order.

Ảnh chuyển khoản bị coi là PAID.

COD WAITING bị coi là Verified Revenue.

## 7.7. Evidence bắt buộc

Prompt phải yêu cầu evidence:

File đã sửa.

Log/test/build output.

Before/after nếu phù hợp.

Migration/seed result nếu có.

Audit/correlation nếu high-risk.

Smoke output nếu có.

## 7.8. Report format bắt buộc

Prompt phải yêu cầu dev báo cáo 14 mục:

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

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

## 8. PROMPT EXECUTION MODE

## 8.1. Mode 1 - Analysis Only

Dùng khi chưa cho phép sửa code.

Agent chỉ được:

Inspect repo.

Tìm file liên quan.

Đối chiếu source-of-truth.

Lập implementation plan.

Nêu rủi ro.

Không sửa file.

Prompt phải ghi rõ:

## MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

## 8.2. Mode 2 - Implement With Guard

Dùng khi đã qua Code Handoff Control.

Agent được phép sửa code trong phạm vi task.

Prompt phải ghi rõ:

## MODE: IMPLEMENT WITH GUARD - MODIFY ONLY FILES REQUIRED FOR THIS BACKLOG

Agent phải:

Không sửa ngoài scope.

Không đổi nghiệp vụ.

Không hardcode policy.

Không bỏ test.

Không bỏ evidence.

Báo cáo đầy đủ.

## 8.3. Mode 3 - Test / Smoke Only

Dùng khi cần QA/test/smoke.

Agent chỉ được:

Chạy test.

Chạy build.

Chạy smoke nếu có.

Thu thập evidence.

Không sửa code nếu chưa được phép.

Prompt phải ghi:

## MODE: TEST / SMOKE ONLY - DO NOT MODIFY BUSINESS LOGIC

## 8.4. Mode 4 - Fix After Review

Dùng khi có điểm chặn/smoke fail.

Agent chỉ được sửa phần liên quan điểm chặn.

Prompt phải ghi:

MODE: FIX AFTER REVIEW - FIX ONLY LISTED điểm chặn

Agent phải:

Nêu root cause.

Sửa đúng điểm chặn.

Chạy retest.

Cập nhật report.

Không mở rộng scope.

## 8.5. Mode 5 - Documentation / Handoff Update

Dùng khi cần cập nhật docs/handoff sau implementation.

Agent chỉ được:

Cập nhật Markdown/handoff.

Ghi rõ thay đổi.

Không sửa code logic.

Không thay source-of-truth.

Prompt phải ghi:

## MODE: DOCUMENTATION / HANDOFF UPDATE ONLY

## 9. PROMPT GUARDRAIL CHUNG

Mọi prompt TECH-13 phải có đoạn guardrail chung sau.

## 9.1. Guardrail về source-of-truth

Coding agent phải tuân thủ:

TECH source-of-truth mới thắng tài liệu cũ.

TECH source-of-truth mới thắng code cũ.

Legacy documents chỉ là tham chiếu.

Current implementation chỉ là hiện trạng.

Nếu conflict, báo điểm chặn.

Không tự chọn theo cảm tính.

## 9.2. Guardrail về không hardcode

Không hardcode:

Giá.

Khuyến mãi.

Member benefit.

Diamond benefit.

Shipping fee.

Payment account.

IVR attempt policy.

Notification content.

Ads scale rule.

Claim/public wording.

Sale Lock/Recall.

Verified Revenue.

Privacy rules.

Nếu runtime/config chưa có, báo điểm chặn.

## 9.3. Guardrail về evidence

Không được báo xong nếu thiếu:

File list.

Commands run.

Test/build result.

Evidence.

Risk note.

Handoff update.

## 9.4. Guardrail về release

Agent không được ghi:

Release Ready.

Production Ready.

Go-live Approved.

Scale Approved.

Global Gateway PASS.

Agent chỉ được ghi:

Implementation completed.

Report submitted.

Evidence provided.

Smoke suggested/passed nếu đã chạy.

Ready for review.

Not release-ready until TECH-10 gates pass.

## 10. PHASE PROMPT PACK OVERVIEW

TECH-13 sẽ tạo prompt pack cho 10 phase.

## 10.1. PHASE 0 Prompt Pack

Tên:

PROMPT-P0 - Foundation / RBAC / Audit / Evidence / Idempotency Execution Handoff

Dùng cho:

## RBAC.

Actor identity.

Audit trail.

Evidence registry.

Idempotency.

Correlation ID.

Admin override.

## 10.2. PHASE 1 Prompt Pack

Tên:

PROMPT-P1 - Product / SKU / Recipe / Product Activation Execution Handoff

Dùng cho:

Product Master.

SKU Master.

Recipe/Formula.

Activation Guard.

Product Active != Sellable.

Public product naming.

## 10.3. PHASE 2 Prompt Pack

Tên:

PROMPT-P2 - Operational Core Execution Handoff

Dùng cho:

Raw intake.

Raw QC.

Raw lot readiness.

Production order snapshot.

Material issue/receipt.

Batch release.

Warehouse.

Trace.

Recall/Sale Lock.

## 10.4. PHASE 3 Prompt Pack

Tên:

PROMPT-P3 - Commerce Runtime Execution Handoff

Dùng cho:

Sellable Gate.

QuoteSnapshot.

Cart.

Order Draft.

Official Order.

Payment.

Shipping/COD.

Verified Revenue.

Member/Diamond/runtime benefit.

## 10.5. PHASE 4 Prompt Pack

Tên:

PROMPT-P4 - AI Advisor Runtime Execution Handoff

Dùng cho:

Product consulting.

Customer memory.

Claim guard.

Quote-safe sales.

Order Draft handoff.

Final Response Guard.

Runtime unavailable fail-safe.

## 10.6. PHASE 5 Prompt Pack

Tên:

PROMPT-P5 - Facebook Gateway / Messenger / Channel Delivery Execution Handoff

Dùng cho:

Public/private routing.

Comment-to-Messenger.

Messenger context.

Final Response Guard delivery.

Typing indicator.

Rate limit.

Moderation.

Suppression.

## 10.7. PHASE 6 Prompt Pack

Tên:

PROMPT-P6 - Ads Measurement / Attribution / Verified Revenue / Scale Gate Execution Handoff

Dùng cho:

Event taxonomy.

Pixel/CAPI dedup.

Attribution.

Verified Revenue.

Exclusion rules.

Data Quality.

Scale Gate.

## 10.8. PHASE 7 Prompt Pack

Tên:

PROMPT-P7 - MC AI Live / Live Script Runtime / Live Sales Control Execution Handoff

Dùng cho:

Live session.

Script runtime.

Script guard.

Comment classifier.

Messenger coordination.

Troll/complaint split.

Ads-safe signal boundary.

## 10.9. PHASE 8 Prompt Pack

Tên:

PROMPT-P8 - IVR Order Confirmation Execution Handoff

Dùng cho:

IVR eligibility.

Customer trust.

Official Order only.

Phone validation.

Attempt policy.

Outcome classification.

Core Order handoff.

Notification handoff.

## 10.10. PHASE 9 Prompt Pack

Tên:

PROMPT-P9 - Global Smoke / UAT / Release Gateway Execution Handoff

Dùng cho:

Evidence intake.

Global smoke.

## UAT.

Owner sign-off.

P0 điểm chặn.

Cross-tech dependency.

Privacy/security.

Rollback/fallback.

Monitoring.

Release decision.

Global Gateway.

## 11. PROMPT TEMPLATE CHUNG CHO MỌI PHASE

Dưới đây là template chuẩn dùng cho mọi phase.

DỰ ÁN: Ginsengfood - Greenfield Technical Implementation

MODE: {{ANALYSIS_ONLY | IMPLEMENT_WITH_GUARD | TEST_SMOKE_ONLY | FIX_AFTER_REVIEW | DOCUMENTATION_HANDOFF_UPDATE_ONLY}}

PHASE: {{phase_id}} - {{phase_name}}

BACKLOG / TASK:
- Backlog ID: {{backlog_id}}
- Task ID: {{task_id_if_any}}
- Domain: {{domain}}
- Source-of-Truth: {{tech_refs}}

MỤC TIÊU:
{{goal}}

SOURCE-OF-TRUTH BẮT BUỘC:
1. {{tech_ref_1}}
2. {{tech_ref_2}}
3. {{tech_ref_3}}

NGUYÊN TẮC BẮT BUỘC:
- TECH source-of-truth mới thắng tài liệu cũ.
- Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.
- Không dùng tài liệu cũ để phủ định TECH mới.
- Không hardcode policy.
- Không copy-paste code rời rạc.
- Không thay đổi nghiệp vụ ngoài scope.
- Không gọi implementation done là release ready.
- Không mở Global Gateway.

SCOPE IN:
1. {{scope_in_1}}
2. {{scope_in_2}}
3. {{scope_in_3}}

SCOPE OUT:
1. {{scope_out_1}}
2. {{scope_out_2}}
3. {{scope_out_3}}

UPSTREAM DEPENDENCY:
1. {{upstream_1}}
2. {{upstream_2}}

DOWNSTREAM HANDOFF:
1. {{downstream_1}}
2. {{downstream_2}}

P0 điểm chặn:
1. {{p0_1}}
2. {{p0_2}}
3. {{p0_3}}

YÊU CẦU THỰC THI:
1. Inspect codebase thật trước khi sửa.
2. Xác định file liên quan.
3. Không sửa ngoài scope.
4. Nếu có conflict giữa code hiện tại và TECH mới, báo điểm chặn.
5. Nếu thiếu runtime/config/policy, báo điểm chặn.
6. Chạy test/build phù hợp nếu có.
7. Không bỏ qua lỗi build/test.

EVIDENCE REQUIRED:
1. {{evidence_1}}
2. {{evidence_2}}
3. {{evidence_3}}

SMOKE REQUIRED:
1. {{smoke_1}}
2. {{smoke_2}}
3. {{smoke_3}}

REPORT FORMAT BẮT BUỘC:
1. Tóm tắt.
2. File đã sửa.
3. Nguồn yêu cầu.
4. Evidence đã dùng.
5. Lệnh đã chạy.
6. Kết quả test.
7. Kết quả backend build.
8. Kết quả frontend build.
9. Kết quả cleanup process.
10. Cập nhật Markdown.
11. Kết quả database migration/update nếu áp dụng.
12. Kết quả seed validation nếu áp dụng.
13. Rủi ro còn lại.
14. Cập nhật handoff.

KẾT LUẬN BẮT BUỘC:
- Chỉ báo cáo implementation status.
- Không được gọi Release Ready.
- Không được gọi Production Ready.
- Không được gọi Go-live Approved.
- Global Gateway vẫn bị chặn cho tới khi TECH-10 pass.

## 12. IMPLEMENTATION REPORT TEMPLATE CHUNG

Mọi phase sau khi dev/coding agent thực hiện phải báo cáo đúng mẫu sau.

## 12.1. Tóm tắt

Ghi rõ:

Phase.

Backlog.

Task.

Đã làm gì.

Chưa làm gì.

Trạng thái hiện tại.

## 12.2. File đã sửa

Ghi rõ:

Backend files.

Frontend files.

Config files.

Migration files.

Seed files.

Test files.

Docs files.

Nếu không áp dụng, ghi “Không áp dụng”.

## 12.3. Nguồn yêu cầu

Ghi rõ:

TECH number.

Section.

Requirement.

Backlog ID.

Task ID.

## 12.4. Evidence đã dùng

Ghi rõ:

Log.

Screenshot/export nếu có.

Test output.

Build output.

Migration output.

Seed validation.

Audit/correlation nếu có.

## 12.5. Lệnh đã chạy

Ghi rõ các lệnh đã chạy:

Build.

Test.

Lint.

Typecheck.

Migration.

Seed.

Smoke.

Cleanup.

## 12.6. Kết quả test

Ghi rõ:

Pass.

Fail.

Chưa chạy.

Lý do chưa chạy.

Retest cần thiết.

## 12.7. Kết quả backend build

Ghi:

Pass/fail.

Error.

Warning.

Impact.

## 12.8. Kết quả frontend build

Ghi:

Pass/fail.

Error.

Warning.

Impact.

## 12.9. Kết quả cleanup process

Ghi:

File rác đã xóa.

Code tạm đã bỏ.

Debug log tạm đã bỏ.

Duplicate đã xử lý.

Chưa cleanup gì thì nêu lý do.

## 12.10. Cập nhật Markdown

Ghi:

Docs cập nhật.

README cập nhật.

Handoff cập nhật.

Chưa cập nhật thì nêu lý do.

## 12.11. Database migration/update nếu áp dụng

Ghi:

Migration name.

Migration status.

Rollback note.

Data impact.

Risk.

## 12.12. Seed validation nếu áp dụng

Ghi:

Seed file.

Seed data.

Validation result.

Missing seed.

Conflict seed.

## 12.13. Rủi ro còn lại

Ghi:

Known issues.

Open điểm chặn.

Technical debt.

Dependency chưa pass.

Smoke chưa chạy.

Evidence thiếu.

Risk cần owner review.

## 12.14. Cập nhật handoff

Ghi:

Phase tiếp theo.

Backlog tiếp theo.

Điều kiện để tiếp tục.

điểm chặn cần xử lý.

Owner/QA cần review.

## 13. DEV PROMPT PACK P0 điểm chặn REGISTRY - PHẦN 1/4

Các lỗi sau là P0 điểm chặn:

Prompt yêu cầu viết code snippet rời rạc.

Prompt không có source-of-truth.

Prompt không có Scope Out.

Prompt không có dependency.

Prompt không có P0 điểm chặn.

Prompt không có evidence required.

Prompt không có smoke required.

Prompt không có report format.

Prompt cho phép hardcode policy.

Prompt cho phép coding agent tự đổi nghiệp vụ.

Prompt cho phép dùng tài liệu cũ thắng TECH mới.

Prompt cho phép dùng code cũ thắng TECH mới.

Prompt cho phép sửa ngoài scope.

Prompt cho phép bỏ test/build.

Prompt cho phép bỏ evidence.

Prompt cho phép gọi implementation done là release ready.

Prompt cho phép gọi production ready.

Prompt cho phép go-live.

Prompt cho phép scale.

Prompt cho phép mở Global Gateway.

## 14. EVIDENCE YÊU CẦU CHO PHẦN 1/4

## PHẦN 1/4 yêu cầu evidence ở cấp tài liệu gồm:

TECH-13 purpose accepted.

Agent Boundary accepted.

No-Code-Snippet Rule accepted.

Prompt Pack Phase Structure accepted.

Prompt Execution Mode accepted.

Prompt Guardrail accepted.

Prompt Template accepted.

Implementation Report Template accepted.

P0 điểm chặn Registry accepted.

Release Handoff sang PHẦN 2/4 accepted.

## 15. SMOKE ĐỊNH HƯỚNG - PHẦN 1/4

TECH13-P1-SMK-001 - Prompt Không Source Bị Reject

Given prompt không có TECH source-of-truth
When Prompt Review chạy
Then prompt bị reject.

TECH13-P1-SMK-002 - Prompt Thiếu Scope Out Bị Reject

Given prompt có Scope In nhưng thiếu Scope Out
When Prompt Review chạy
Then prompt không được dùng giao dev.

TECH13-P1-SMK-003 - Prompt Yêu Cầu Copy Code Bị Reject

Given prompt yêu cầu “viết code rồi dán vào project”
When Prompt Review chạy
Then prompt bị reject.

TECH13-P1-SMK-004 - Prompt Thiếu Evidence/Smoke Bị Reject

Given prompt không yêu cầu evidence/smoke
When Prompt Review chạy
Then prompt không được dùng.

TECH13-P1-SMK-005 - Prompt Cho Hardcode Policy Bị Reject

Given prompt cho phép hardcode giá/khuyến mãi/member/IVR policy
When Prompt Review chạy
Then prompt fail.

TECH13-P1-SMK-006 - Agent Không Được Gọi Release Ready

Given agent hoàn tất implementation
When agent report
Then agent không được gọi Release Ready, chỉ báo Ready for Review.

TECH13-P1-SMK-007 - Conflict Phải Báo điểm chặn

Given agent thấy code cũ khác TECH mới
When execution chạy
Then agent phải báo conflict/điểm chặn, không tự chọn code cũ.

TECH13-P1-SMK-008 - Report Thiếu 14 Mục Bị Reject

Given agent report thiếu file list/test/build/evidence/risk/handoff
When report review
Then report bị reject hoặc Need More Info.

## 16. DONE GATE - PHẦN 1/4

## PHẦN 1/4 đạt Documentation Complete ở cấp nguyên tắc khi:

Đã khóa mục tiêu TECH-13.

Đã khóa vai trò TECH-13 trong bộ TECH.

Đã khóa TECH-13 không phải tài liệu code.

Đã khóa No-Code-Snippet Rule.

Đã khóa Agent Boundary.

Đã khóa Prompt Pack chia theo phase.

Đã khóa Phase Execution Prompt Structure.

Đã khóa Prompt Execution Mode.

Đã khóa Prompt Guardrail chung.

Đã khóa Phase Prompt Pack Overview.

Đã khóa Prompt Template chung.

Đã khóa Implementation Report Template.

Đã khóa P0 điểm chặn Registry.

Đã có Evidence Requirement.

Đã có Smoke định hướng.

Đã có Release Handoff sang PHẦN 2/4.

## 17. FAIL GATE - PHẦN 1/4

## PHẦN 1/4 FAIL nếu:

TECH-13 bị hiểu là tài liệu code.

TECH-13 tạo code snippet để copy.

TECH-13 không chia phase.

Prompt không có source-of-truth.

Prompt không có Scope Out.

Prompt không có dependency.

Prompt không có P0 điểm chặn.

Prompt không có evidence.

Prompt không có smoke.

Prompt không có report format.

Prompt cho phép hardcode policy.

Prompt cho phép dev/coding agent tự đổi nghiệp vụ.

Prompt cho phép dùng code cũ thắng TECH mới.

Prompt cho phép dùng tài liệu cũ thắng TECH mới.

Prompt cho phép gọi Release Ready.

Prompt cho phép gọi Production Ready.

Prompt cho phép Go-live Approved.

Prompt cho phép mở Global Gateway.

Không có prompt execution mode.

Không có implementation report template.

## 18. RELEASE HANDOFF - SANG PHẦN 2/4

## PHẦN 1/4 bàn giao sang PHẦN 2/4 các nguyên tắc đã khóa:

TECH-13 là Dev Prompt Pack.

TECH-13 không viết code.

Prompt phải chia theo phase.

Prompt phải có source-of-truth.

Prompt phải có Scope In / Scope Out.

Prompt phải có dependency.

Prompt phải có P0 điểm chặn.

Prompt phải có evidence required.

Prompt phải có smoke required.

Prompt phải có report format 14 mục.

Prompt phải có execution mode.

Prompt phải cấm hardcode policy.

Prompt phải cấm copy-paste code rời rạc.

Prompt phải yêu cầu inspect codebase thật.

Prompt phải yêu cầu báo conflict nếu code cũ khác TECH mới.

Prompt không được gọi Release Ready / Production Ready / Go-live Approved.

Prompt không được mở Global Gateway.

## PHẦN 2/4 sẽ viết:

## DEV PROMPT MODULE CONTRACTS / PHASE PROMPT CONTRACTS / AGENT EXECUTION CONTROL

Các module cần khóa ở PHẦN 2/4:

Prompt Pack Orchestrator.

Phase Prompt Factory.

Source Injection Resolver.

Scope Injection Resolver.

Dependency Injection Resolver.

P0 điểm chặn Injection Resolver.

Evidence/Smoke Injection Resolver.

Execution Mode Resolver.

Agent Guardrail Resolver.

Codebase Inspection Resolver.

Conflict Detection Resolver.

Report Template Resolver.

Prompt Review Resolver.

Prompt Change Control Resolver.

Prompt-to-Dev Handoff Resolver.

## 19. TRẠNG THÁI SAU PHẦN 1/4

Sau PHẦN 1/4:

Chưa được gọi là Documentation Complete.

Chưa được gọi là Prompt Pack Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa nguyên tắc nền của TECH-13.

Các điểm cốt lõi đã được cố định:

TECH-13 là bộ prompt/handoff giao dev/Codex/Copilot theo phase.

TECH-13 không phải tài liệu code.

TECH-13 không tạo snippet để copy-paste.

Prompt phải chia theo PHASE 0 -> PHASE 9.

Mỗi prompt phải có source-of-truth rõ.

Mỗi prompt phải có Scope In / Scope Out.

Mỗi prompt phải có dependency.

Mỗi prompt phải có P0 điểm chặn.

Mỗi prompt phải có evidence required.

Mỗi prompt phải có smoke required.

Mỗi prompt phải có report format 14 mục.

Mỗi prompt phải có execution mode.

Coding agent phải inspect codebase thật.

Coding agent không được tự đổi nghiệp vụ.

Coding agent không được hardcode policy.

Coding agent không được dùng tài liệu cũ hoặc code cũ để phủ định TECH mới.

Coding agent phải báo conflict/điểm chặn nếu phát hiện mâu thuẫn.

Coding agent không được gọi implementation done là Release Ready.

Coding agent không được gọi Production Ready.

Coding agent không được mở Global Gateway.

Report thiếu file list/test/build/evidence/risk/handoff sẽ bị reject.

TECH-10 mới có quyền xét Release Ready, Go-live Approved và Global Gateway.

## PHẦN 1/4 sẵn sàng bàn giao sang:

## PHẦN 2/4 - DEV PROMPT MODULE CONTRACTS / PHASE PROMPT CONTRACTS / AGENT EXECUTION CONTROL.

## PHẦN 2/4 - DEV PROMPT MODULE CONTRACTS / PHASE PROMPT CONTRACTS / AGENT EXECUTION CONTROL

## 1. MỤC TIÊU PHẦN 2/4

## PHẦN 2/4 khóa các module contract để tạo, review, kiểm soát và bàn giao prompt cho dev/Codex/Copilot/coding agent.

PHẦN này xác định rõ:

Module nào điều phối toàn bộ prompt pack.

Module nào tạo prompt theo từng phase.

Module nào đưa source-of-truth vào prompt.

Module nào đưa Scope In / Scope Out vào prompt.

Module nào đưa dependency vào prompt.

Module nào đưa P0 điểm chặn vào prompt.

Module nào đưa evidence/smoke vào prompt.

Module nào xác định execution mode.

Module nào kiểm soát guardrail cho coding agent.

Module nào buộc agent inspect codebase thật.

Module nào phát hiện conflict giữa TECH mới và code cũ.

Module nào ép report template 14 mục.

Module nào review prompt trước khi giao dev.

Module nào kiểm soát thay đổi prompt.

Module nào bàn giao prompt sang dev/TECH-11.

## PHẦN 2/4 không viết code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế database chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa module contract cho prompt/handoff governance.

## 2. NGUYÊN TẮC MODULE CONTRACT CHUNG

Mọi module trong TECH-13 phải tuân thủ:

Không tạo prompt không source-of-truth.

Không tạo prompt thiếu Scope Out.

Không tạo prompt thiếu dependency.

Không tạo prompt thiếu P0 điểm chặn.

Không tạo prompt thiếu evidence required.

Không tạo prompt thiếu smoke required.

Không tạo prompt thiếu report format.

Không tạo prompt yêu cầu copy-paste code rời rạc.

Không tạo prompt yêu cầu agent hardcode policy.

Không tạo prompt cho phép agent tự đổi nghiệp vụ.

Không tạo prompt cho phép code cũ thắng TECH mới.

Không tạo prompt cho phép tài liệu cũ thắng TECH mới.

Không tạo prompt cho phép agent sửa ngoài scope.

Không tạo prompt cho phép bỏ test/build/evidence.

Không tạo prompt cho phép agent gọi Release Ready.

Không tạo prompt cho phép agent gọi Production Ready.

Không tạo prompt cho phép agent gọi Go-live Approved.

Không tạo prompt cho phép agent mở Global Gateway.

Không tạo prompt một lần cho toàn bộ hệ thống.

Không bỏ qua TECH-11 Code Handoff Control và TECH-10 Global Release Gateway.

## 3. MODULE CONTRACT 01 - PROMPT PACK ORCHESTRATOR

## 3.1. Mục tiêu

Prompt Pack Orchestrator điều phối toàn bộ bộ prompt TECH-13 theo PHASE 0 -> PHASE 9.

Module này đảm bảo mỗi prompt được tạo đúng phase, đúng backlog, đúng source, đúng boundary và đúng mode thực thi.

## 3.2. Scope In

Prompt Pack Orchestrator được phép:

Tạo danh sách prompt pack theo phase.

Gắn mỗi prompt với phase/backlog/task.

Kiểm tra prompt có đủ module contract bắt buộc chưa.

Tổng hợp trạng thái prompt: Draft / Need Review / Ready / bị chặn.

Điều phối Source Injection Resolver.

Điều phối Scope Injection Resolver.

Điều phối Dependency Injection Resolver.

Điều phối P0 điểm chặn Injection Resolver.

Điều phối Evidence/Smoke Injection Resolver.

Điều phối Execution Mode Resolver.

Điều phối Agent Guardrail Resolver.

Bàn giao prompt đã pass sang Prompt-to-Dev Handoff Resolver.

## 3.3. Scope Out

Prompt Pack Orchestrator không được:

Viết code.

Tạo code snippet.

Giao prompt không source.

Giao prompt một lần cho toàn bộ hệ thống.

Tự quyết định business rule.

Tự mở Dev Ready nếu TECH-11 chưa pass.

Tự mở Release Ready nếu TECH-10 chưa pass.

Tự mở Global Gateway.

Bỏ qua prompt review.

Bỏ qua change control.

## 3.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-12.

TECH-12 Phase Backlog Pack.

TECH-11 Dev Handoff Governance.

TECH-10 Global Release Governance.

Backlog-to-Handoff Package.

Phase Dependency Matrix.

## 3.5. Downstream Handoff

Bàn giao cho:

Phase Prompt Factory.

Prompt Review Resolver.

Prompt-to-Dev Handoff Resolver.

TECH-11 Dev Handoff Resolver.

## 3.6. P0 điểm chặn

FAIL nếu Prompt Pack Orchestrator tạo prompt tổng “triển khai toàn bộ Ginsengfood”.

FAIL nếu prompt thiếu source-of-truth vẫn Ready.

FAIL nếu prompt gọi dev/coding agent viết code copy-paste.

FAIL nếu prompt bypass TECH-11 hoặc TECH-10.

## 3.7. Evidence

Evidence tối thiểu:

Prompt pack list.

Phase mapping.

Backlog mapping.

Source mapping status.

Prompt review status.

bị chặn prompt list.

Ready prompt list.

Handoff record.

## 3.8. Smoke

## TECH13-MOD-SMK-001

Given prompt pack có prompt “triển khai toàn bộ hệ thống”
When Prompt Pack Orchestrator review
Then prompt phải bị reject vì không chia phase.

## 4. MODULE CONTRACT 02 - PHASE PROMPT FACTORY

## 4.1. Mục tiêu

Phase Prompt Factory tạo prompt draft cho từng phase/backlog/task theo template chuẩn TECH-13.

Module này không tạo code.

Module này chỉ tạo prompt/handoff để coding agent làm việc có kiểm soát.

## 4.2. Scope In

Phase Prompt Factory được phép tạo prompt có:

Project header.

Execution mode.

Phase.

Backlog ID.

Task ID nếu có.

Domain.

Source-of-truth.

Mục tiêu.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Report format 14 mục.

No hardcode policy guard.

No copy-paste code guard.

Release/Gateway warning.

## 4.3. Scope Out

Phase Prompt Factory không được:

Tạo prompt thiếu source.

Tạo prompt thiếu Scope Out.

Tạo prompt thiếu P0 điểm chặn.

Tạo prompt thiếu evidence/smoke.

Tạo prompt không có execution mode.

Tạo prompt yêu cầu agent tự đoán requirement.

Tạo prompt yêu cầu agent tự quyết business rule.

Tạo prompt viết code không cần inspect codebase.

Tạo prompt kết luận Release Ready.

Tạo prompt mở Gateway.

## 4.4. Upstream Dependency

Phụ thuộc:

TECH-12 Backlog Pack.

Prompt Pack Orchestrator.

Source Injection Resolver.

Scope Injection Resolver.

Dependency Injection Resolver.

Evidence/Smoke Injection Resolver.

## 4.5. Downstream Handoff

Bàn giao:

Draft phase prompt.

Missing prompt field list.

Prompt status.

Handoff sang Prompt Review Resolver.

## 4.6. P0 điểm chặn

FAIL nếu prompt factory tạo prompt không có execution mode.

FAIL nếu prompt factory tạo prompt không có report format.

FAIL nếu prompt factory tạo prompt có câu “hãy viết code hoàn chỉnh để copy vào”.

## 4.7. Evidence

Evidence tối thiểu:

Prompt ID.

Phase ID.

Backlog ID.

Required prompt field checklist.

Missing field list.

Draft status.

## 4.8. Smoke

## TECH13-MOD-SMK-002

Given phase prompt thiếu Scope Out và Report Format
When Phase Prompt Factory tạo draft
Then prompt không được Ready.

## 5. MODULE CONTRACT 03 - SOURCE INJECTION RESOLVER

## 5.1. Mục tiêu

Source Injection Resolver đưa source-of-truth vào prompt một cách cụ thể, không chung chung.

Module này đảm bảo coding agent biết phải đọc TECH nào, section nào, rule nào.

## 5.2. Scope In

Module được phép đưa vào prompt:

TECH number.

Section hoặc requirement group.

Backlog ID.

Task ID.

Rule P0 liên quan.

Dependency liên quan.

Source conflict note nếu có.

Legacy boundary.

Current implementation boundary.

Yêu cầu báo conflict nếu code cũ khác TECH mới.

## 5.3. Scope Out

Module không được:

Ghi “đọc tài liệu liên quan” chung chung.

Ghi source mơ hồ.

Dùng tài liệu cũ làm source thật.

Dùng code cũ làm source thật.

Bỏ qua conflict.

Cho agent tự chọn source.

Cho agent tự quyết nếu TECH và code cũ khác nhau.

## 5.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-12.

Source-to-Backlog Matrix.

Backlog ID.

Task ID.

Source conflict registry nếu có.

## 5.5. Downstream Handoff

Bàn giao:

Source-injected prompt.

Source missing warning.

Source conflict warning.

Handoff sang Prompt Review Resolver.

## 5.6. P0 điểm chặn

FAIL nếu prompt không có TECH source.

FAIL nếu prompt cho phép dùng code cũ làm source-of-truth.

FAIL nếu prompt không yêu cầu báo conflict.

## 5.7. Evidence

Evidence tối thiểu:

Prompt ID.

TECH reference list.

Requirement mapping.

Conflict status.

Injection decision.

## 5.8. Smoke

## TECH13-MOD-SMK-003

Given prompt giao Commerce QuoteSnapshot nhưng không nhắc TECH-04
When Source Injection review
Then prompt rejected.

## 6. MODULE CONTRACT 04 - SCOPE INJECTION RESOLVER

## 6.1. Mục tiêu

Scope Injection Resolver đưa Scope In và Scope Out vào prompt.

Module này ngăn coding agent tự mở rộng phạm vi.

## 6.2. Scope In

Module được phép đưa vào prompt:

Việc cần inspect.

Việc cần phân tích.

Việc cần sửa nếu được phép.

Việc cần test.

Việc cần báo cáo.

Các file/khu vực có thể kiểm tra nếu biết.

Các việc được phép làm trong mode hiện tại.

## 6.3. Scope Out

Module phải đưa vào prompt các việc không được làm:

Không sửa ngoài phase.

Không sửa ngoài backlog.

Không đổi business rule.

Không hardcode policy.

Không bỏ audit/evidence.

Không tự tạo release decision.

Không mở Global Gateway.

Không gọi Release Ready.

Không gọi Production Ready.

Không sửa API/DB/UI nếu mode chưa cho phép hoặc impact chưa review.

## 6.4. Upstream Dependency

Phụ thuộc:

TECH-12 Backlog Scope.

TECH-11 Code Handoff Control.

Execution Mode Resolver.

Phase Prompt Factory.

## 6.5. Downstream Handoff

Bàn giao:

Scope-injected prompt.

Scope missing warning.

Scope conflict warning.

Handoff sang Prompt Review Resolver.

## 6.6. P0 điểm chặn

FAIL nếu prompt thiếu Scope Out.

FAIL nếu prompt cho phép agent sửa ngoài scope.

FAIL nếu prompt cho phép agent tự đổi business rule.

## 6.7. Evidence

Evidence tối thiểu:

Prompt ID.

Scope In list.

Scope Out list.

Reviewer decision.

## 6.8. Smoke

## TECH13-MOD-SMK-004

Given prompt có Scope In nhưng thiếu Scope Out
When Scope Injection review
Then prompt không Ready.

## 7. MODULE CONTRACT 05 - DEPENDENCY INJECTION RESOLVER

## 7.1. Mục tiêu

Dependency Injection Resolver đưa upstream/downstream dependency vào prompt.

Module này ngăn coding agent triển khai downstream khi upstream chưa pass.

## 7.2. Scope In

Module được phép đưa vào prompt:

Upstream phase dependency.

Upstream backlog dependency.

Downstream handoff.

Planning-only dependency nếu có.

bị chặn dependency nếu có.

Mock limitation nếu có.

Runtime dependency.

TECH-10 release dependency.

## 7.3. Scope Out

Module không được:

Bỏ dependency.

Cho downstream tự release.

Cho mock làm production truth.

Cho agent tự đánh dependency pass.

Cho agent triển khai khi upstream critical bị chặn.

Bỏ Sale Lock/Recall dependency.

Bỏ Verified Revenue dependency.

Bỏ QuoteSnapshot dependency.

Bỏ Official Order dependency.

Bỏ Final Response Guard dependency.

## 7.4. Upstream Dependency

Phụ thuộc:

TECH-12 Dependency Matrix.

TECH-11 Phase Control.

Backlog dependency status.

Prompt execution mode.

## 7.5. Downstream Handoff

Bàn giao:

Dependency-injected prompt.

Dependency bị chặn warning.

Planning-only note.

Handoff sang Prompt Review Resolver.

## 7.6. P0 điểm chặn

FAIL nếu prompt cho PHASE 6 Ads scale nhưng không nhắc Verified Revenue dependency.

FAIL nếu prompt cho PHASE 8 IVR nhưng không nhắc Official Order/Core Order dependency.

FAIL nếu prompt cho Gateway/Live nhưng không nhắc Final Response Guard.

## 7.7. Evidence

Evidence tối thiểu:

Prompt ID.

Dependency list.

Dependency status.

bị chặn/planning-only note.

Reviewer decision.

## 7.8. Smoke

## TECH13-MOD-SMK-005

Given prompt PHASE 8 IVR thiếu dependency Official Order
When Dependency Injection review
Then prompt rejected.

## 8. MODULE CONTRACT 06 - P0 điểm chặn INJECTION RESOLVER

## 8.1. Mục tiêu

P0 điểm chặn Injection Resolver đưa lỗi cấm tuyệt đối vào prompt.

Module này bảo đảm coding agent biết việc gì không được vi phạm.

## 8.2. Scope In

Module được phép đưa vào prompt:

P0 điểm chặn của phase.

P0 điểm chặn của backlog.

P0 điểm chặn của dependency.

P0 điểm chặn về source-of-truth.

P0 điểm chặn về hardcode.

P0 điểm chặn về privacy/security.

P0 điểm chặn về evidence/smoke.

P0 điểm chặn về release/gateway.

P0 điểm chặn về cross-phase downstream.

## 8.3. Scope Out

Module không được:

Bỏ P0 điểm chặn.

Ghi P0 điểm chặn chung chung.

Hạ P0 thành warning.

Cho agent tự bỏ qua P0.

Cho agent gọi P0 là “known issue” để tiếp tục.

Cho agent sửa workaround trái source-of-truth.

## 8.4. Upstream Dependency

Phụ thuộc:

TECH-00 -> TECH-12 P0 registry.

Phase P0 registry.

Backlog P0 registry.

Prompt Pack Orchestrator.

## 8.5. Downstream Handoff

Bàn giao:

P0-injected prompt.

Missing P0 warning.

Handoff sang Prompt Review Resolver.

## 8.6. P0 điểm chặn

FAIL nếu prompt không có P0 điểm chặn.

FAIL nếu prompt cho phép agent bỏ qua P0 điểm chặn.

FAIL nếu prompt gọi P0 điểm chặn là optional.

## 8.7. Evidence

Evidence tối thiểu:

Prompt ID.

P0 điểm chặn list.

Phase/backlog mapping.

Reviewer decision.

## 8.8. Smoke

## TECH13-MOD-SMK-006

Given prompt Commerce thiếu P0 “ảnh chuyển khoản không phải PAID”
When P0 Injection review
Then prompt không Ready.

## 9. MODULE CONTRACT 07 - EVIDENCE / SMOKE INJECTION RESOLVER

## 9.1. Mục tiêu

Evidence / Smoke Injection Resolver đưa yêu cầu evidence và smoke vào prompt.

Module này ngăn coding agent báo “đã xong” mà không có bằng chứng.

## 9.2. Scope In

Module được phép đưa vào prompt:

Evidence required.

Smoke required.

Test/build command expectation.

Migration/seed evidence nếu có.

Audit/correlation evidence nếu high-risk.

Privacy/security evidence nếu có dữ liệu nhạy cảm.

Runtime unavailable smoke nếu cần.

Dependency smoke nếu downstream.

Fail-case smoke nếu critical.

Report evidence section requirement.

## 9.3. Scope Out

Module không được:

Bỏ evidence.

Bỏ smoke.

Chấp nhận lời nói thay evidence.

Chỉ yêu cầu happy path smoke.

Bỏ P0 smoke.

Bỏ build/test output.

Bỏ migration/seed result khi có impact.

Bỏ privacy/security evidence.

Cho agent gọi done khi test chưa chạy.

Cho agent bỏ qua failed test.

## 9.4. Upstream Dependency

Phụ thuộc:

TECH-12 Evidence Matrix.

TECH-12 Smoke Matrix.

TECH-10 Evidence/Smoke Governance.

Phase/backlog risk level.

## 9.5. Downstream Handoff

Bàn giao:

Evidence/smoke-injected prompt.

Missing evidence warning.

Missing smoke warning.

Handoff sang Prompt Review Resolver.

## 9.6. P0 điểm chặn

FAIL nếu prompt không yêu cầu evidence.

FAIL nếu prompt không yêu cầu smoke.

FAIL nếu prompt không yêu cầu report test/build.

FAIL nếu high-risk task không yêu cầu audit/correlation evidence.

## 9.7. Evidence

Evidence tối thiểu:

Prompt ID.

Evidence list.

Smoke list.

Reviewer decision.

## 9.8. Smoke

## TECH13-MOD-SMK-007

Given prompt Ads Measurement thiếu smoke loại trừ quote/cart/draft/unpaid
When Evidence/Smoke Injection review
Then prompt rejected.

## 10. MODULE CONTRACT 08 - EXECUTION MODE RESOLVER

## 10.1. Mục tiêu

Execution Mode Resolver xác định coding agent được phép làm gì trong lần chạy prompt.

Module này ngăn agent tự sửa code khi prompt chỉ yêu cầu analysis hoặc test.

## 10.2. Execution Mode hợp lệ

Các mode hợp lệ:

ANALYSIS ONLY
Chỉ inspect, phân tích, lập plan. Không sửa file.

IMPLEMENT WITH GUARD
Được sửa file trong scope. Phải test/build/report.

TEST / SMOKE ONLY
Chỉ chạy test/smoke/build. Không sửa business logic.

FIX AFTER REVIEW
Chỉ sửa điểm chặn được liệt kê. Không mở rộng scope.

DOCUMENTATION / HANDOFF UPDATE ONLY
Chỉ cập nhật docs/handoff. Không sửa code logic.

## 10.3. Scope In

Module được phép:

Gắn execution mode vào prompt.

Ghi rõ quyền được làm.

Ghi rõ việc không được làm.

Chặn prompt thiếu mode.

Chặn mode sai với backlog status.

Chặn implement mode nếu TECH-11 Code Handoff chưa pass.

## 10.4. Scope Out

Module không được:

Cho prompt không có mode.

Cho implement mode khi chưa code handoff.

Cho fix mode sửa ngoài điểm chặn.

Cho test mode sửa business logic.

Cho docs mode sửa code.

Cho analysis mode sửa file.

Cho agent tự đổi mode.

## 10.5. Upstream Dependency

Phụ thuộc:

TECH-11 Code Handoff Control.

Backlog status.

Phase status.

Prompt purpose.

Handoff status.

## 10.6. Downstream Handoff

Bàn giao:

Mode-injected prompt.

Mode bị chặn warning.

Handoff sang Prompt Review Resolver.

## 10.7. P0 điểm chặn

FAIL nếu prompt không có execution mode.

FAIL nếu ANALYSIS ONLY nhưng agent được phép sửa file.

FAIL nếu IMPLEMENT WITH GUARD khi chưa qua Code Handoff Control.

## 10.8. Evidence

Evidence tối thiểu:

Prompt ID.

Execution mode.

Mode rationale.

Permission boundary.

Reviewer decision.

## 10.9. Smoke

## TECH13-MOD-SMK-008

Given prompt chưa qua TECH-11 Code Handoff Control
When Execution Mode Resolver chạy
Then không được chọn IMPLEMENT WITH GUARD.

## 11. MODULE CONTRACT 09 - AGENT GUARDRAIL RESOLVER

## 11.1. Mục tiêu

Agent Guardrail Resolver đưa các guardrail bắt buộc vào prompt để giới hạn hành vi của coding agent.

## 11.2. Scope In

Guardrail bắt buộc gồm:

TECH mới thắng tài liệu cũ.

TECH mới thắng code cũ.

Không hardcode policy.

Không copy-paste code rời rạc.

Không đổi business rule.

Không sửa ngoài scope.

Không bỏ audit/evidence.

Không bỏ smoke/test/build.

Không gọi Release Ready.

Không gọi Production Ready.

Không gọi Go-live Approved.

Không mở Global Gateway.

Nếu conflict thì báo điểm chặn.

Nếu thiếu runtime/config thì báo điểm chặn.

Nếu test/build fail thì báo thật.

## 11.3. Scope Out

Module không được:

Bỏ guardrail cho prompt “nhanh”.

Cho agent tự tắt guardrail.

Cho agent bypass vì “để build pass”.

Cho agent sửa rule để test pass.

Cho agent giấu failure.

Cho agent bỏ report risk.

## 11.4. Upstream Dependency

Phụ thuộc:

TECH-00 Governance.

TECH-10 Gateway.

TECH-11 Code Handoff.

TECH-12 Backlog Pack.

## 11.5. Downstream Handoff

Bàn giao:

Guardrail-injected prompt.

Missing guardrail warning.

Handoff sang Prompt Review Resolver.

## 11.6. P0 điểm chặn

FAIL nếu prompt không cấm hardcode.

FAIL nếu prompt không cấm release claim.

FAIL nếu prompt không yêu cầu báo conflict.

## 11.7. Evidence

Evidence tối thiểu:

Prompt ID.

Guardrail list.

Reviewer decision.

## 11.8. Smoke

## TECH13-MOD-SMK-009

Given prompt cho agent “fix cho build pass bằng mọi cách”
When Guardrail Resolver review
Then prompt rejected.

## 12. MODULE CONTRACT 10 - CODEBASE INSPECTION RESOLVER

## 12.1. Mục tiêu

Codebase Inspection Resolver buộc coding agent inspect codebase thật trước khi sửa.

Module này ngăn agent viết code tưởng tượng hoặc snippet không khớp project.

## 12.2. Scope In

Prompt phải yêu cầu agent:

Inspect cấu trúc repo.

Tìm file liên quan.

Tìm pattern hiện có.

Tìm module/domain liên quan.

Tìm test hiện có.

Tìm build command.

Tìm config/runtime liên quan.

Tìm migration/seed liên quan nếu có.

Xác định current implementation state.

Ghi rõ file dự kiến sửa trước hoặc trong report.

## 12.3. Scope Out

Module không được:

Cho agent viết code trước khi inspect.

Cho agent đoán file.

Cho agent tạo file lung tung.

Cho agent bỏ qua pattern project.

Cho agent bỏ qua test hiện có.

Cho agent bỏ qua config/runtime.

Cho agent sửa DB/API/UI ngầm không review.

## 12.4. Upstream Dependency

Phụ thuộc:

Execution Mode.

Dev Handoff.

Code Handoff Control.

Backlog/task scope.

## 12.5. Downstream Handoff

Bàn giao:

Inspection-injected prompt.

Inspection checklist.

Handoff sang Prompt Review Resolver.

## 12.6. P0 điểm chặn

FAIL nếu prompt implement không yêu cầu inspect codebase.

FAIL nếu prompt cho phép agent tạo code không theo project pattern.

FAIL nếu prompt không yêu cầu báo file đã sửa.

## 12.7. Evidence

Evidence tối thiểu:

Prompt ID.

Inspection checklist.

Required report fields.

Reviewer decision.

## 12.8. Smoke

## TECH13-MOD-SMK-010

Given prompt IMPLEMENT WITH GUARD nhưng không yêu cầu inspect codebase
When Codebase Inspection Resolver review
Then prompt rejected.

## 13. MODULE CONTRACT 11 - CONFLICT DETECTION RESOLVER

## 13.1. Mục tiêu

Conflict Detection Resolver đưa yêu cầu phát hiện và báo cáo mâu thuẫn vào prompt.

## 13.2. Scope In

Prompt phải yêu cầu agent phát hiện conflict giữa:

TECH mới và tài liệu cũ.

TECH mới và code cũ.

Backlog và codebase.

Source-of-truth và test hiện có.

Policy và config hiện tại.

Runtime rule và hardcode cũ.

State machine mới và implementation cũ.

Public/private boundary mới và behavior cũ.

Evidence/audit rule mới và code cũ.

Release gate mới và current pipeline.

## 13.3. Scope Out

Module không được:

Cho agent bỏ qua conflict.

Cho agent tự chọn code cũ.

Cho agent sửa âm thầm.

Cho agent hợp thức hóa legacy behavior.

Cho agent downgrade conflict thành note không hành động.

Cho agent tiếp tục code nếu conflict là P0 và chưa có decision.

## 13.4. Upstream Dependency

Phụ thuộc:

Source Injection.

Codebase Inspection.

Agent Guardrail.

Backlog P0 điểm chặn.

## 13.5. Downstream Handoff

Bàn giao:

Conflict detection prompt block.

Conflict report requirement.

Handoff sang Prompt Review Resolver.

## 13.6. P0 điểm chặn

FAIL nếu prompt không yêu cầu báo conflict.

FAIL nếu prompt cho phép giữ code cũ vì “đang chạy”.

FAIL nếu prompt không yêu cầu nêu điểm chặn khi TECH và code cũ mâu thuẫn.

## 13.7. Evidence

Evidence tối thiểu:

Prompt ID.

Conflict detection requirement.

Conflict reporting section.

Reviewer decision.

## 13.8. Smoke

## TECH13-MOD-SMK-011

Given prompt sửa AI Advisor nhưng không yêu cầu phát hiện AI tự tính giá cũ
When Conflict Detection Resolver review
Then prompt không Ready.

## 14. MODULE CONTRACT 12 - REPORT TEMPLATE RESOLVER

## 14.1. Mục tiêu

Report Template Resolver đưa format báo cáo 14 mục vào mọi prompt.

## 14.2. Scope In

Report bắt buộc gồm:

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

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

## 14.3. Scope Out

Module không được:

Bỏ report template.

Cho report rút gọn với task high-risk.

Cho agent chỉ ghi “done”.

Cho agent bỏ file list.

Cho agent bỏ lệnh đã chạy.

Cho agent bỏ test/build result.

Cho agent bỏ risk.

Cho agent bỏ handoff update.

## 14.4. Upstream Dependency

Phụ thuộc:

TECH-11 Implementation Report Format.

TECH-12 Report Requirement.

Phase/backlog risk level.

## 14.5. Downstream Handoff

Bàn giao:

Report-injected prompt.

Missing report warning.

Handoff sang Prompt Review Resolver.

## 14.6. P0 điểm chặn

FAIL nếu prompt không có report format.

FAIL nếu prompt cho agent báo cáo thiếu test/build/evidence.

FAIL nếu prompt không yêu cầu rủi ro còn lại.

## 14.7. Evidence

Evidence tối thiểu:

Prompt ID.

Report template attached.

Reviewer decision.

## 14.8. Smoke

## TECH13-MOD-SMK-012

Given prompt thiếu phần “Rủi ro còn lại”
When Report Template Resolver review
Then prompt rejected.

## 15. MODULE CONTRACT 13 - PROMPT REVIEW RESOLVER

## 15.1. Mục tiêu

Prompt Review Resolver kiểm tra prompt trước khi giao dev/coding agent.

Không prompt nào được dùng nếu chưa review.

## 15.2. Scope In

Module kiểm tra prompt có đủ:

Phase.

Backlog/task.

Execution mode.

Source-of-truth.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence required.

Smoke required.

Guardrail.

Codebase inspection requirement.

Conflict detection requirement.

Report format.

Release/Gateway warning.

## 15.3. Scope Out

Module không được:

Pass prompt thiếu field critical.

Pass prompt yêu cầu copy-paste code.

Pass prompt cho hardcode policy.

Pass prompt không mode.

Pass prompt không source.

Pass prompt không evidence/smoke.

Pass prompt không report.

Pass prompt cho phép release claim.

Pass prompt bypass TECH-11/TECH-10.

## 15.4. Upstream Dependency

Phụ thuộc:

All prompt injection resolvers.

Phase Prompt Factory.

Prompt Pack Orchestrator.

Backlog status.

## 15.5. Downstream Handoff

Bàn giao:

## PROMPT_READY.

## PROMPT_REJECTED.

## PROMPT_NEED_MORE_CONTEXT.

## PROMPT_BLOCKED.

Handoff sang Prompt-to-Dev Handoff Resolver.

## 15.6. P0 điểm chặn

FAIL nếu prompt chưa review đã giao dev.

FAIL nếu prompt thiếu source vẫn được dùng.

FAIL nếu prompt có release claim vẫn được dùng.

## 15.7. Evidence

Evidence tối thiểu:

Prompt ID.

Review checklist.

Missing field list.

Decision.

Reviewer.

## 15.8. Smoke

## TECH13-MOD-SMK-013

Given prompt thiếu execution mode và P0 điểm chặn
When Prompt Review Resolver chạy
Then PROMPT_REJECTED.

## 16. MODULE CONTRACT 14 - PROMPT CHANGE CONTROL RESOLVER

## 16.1. Mục tiêu

Prompt Change Control Resolver kiểm soát mọi thay đổi prompt sau khi đã Ready hoặc đã giao dev.

## 16.2. Scope In

Module được phép kiểm soát thay đổi:

Source-of-truth.

Scope.

Dependency.

Execution mode.

P0 điểm chặn.

Evidence.

Smoke.

Report format.

Guardrail.

Handoff target.

Phase/backlog mapping.

## 16.3. Scope Out

Module không được:

Sửa prompt âm thầm.

Xóa P0 điểm chặn.

Xóa evidence/smoke.

Thay IMPLEMENT WITH GUARD khi chưa code handoff.

Thay source sang legacy doc.

Bỏ report format.

Bỏ release warning.

Không thông báo dev/QA khi prompt thay đổi sau handoff.

## 16.4. Upstream Dependency

Phụ thuộc:

Prompt status.

Backlog change control.

Dev handoff status.

Reviewer/owner approval.

## 16.5. Downstream Handoff

Bàn giao:

Prompt change record.

Before/after prompt.

Impact list.

Re-review requirement.

Re-handoff requirement.

## 16.6. P0 điểm chặn

FAIL nếu prompt thay đổi sau handoff mà không có change record.

FAIL nếu change control xóa evidence/smoke.

FAIL nếu change control bỏ guardrail.

## 16.7. Evidence

Evidence tối thiểu:

Change request ID.

Prompt ID.

Before/after.

Impact.

Approver.

Re-handoff status.

## 16.8. Smoke

## TECH13-MOD-SMK-014

Given prompt đã giao dev nhưng Scope Out bị sửa
When Change Control chạy
Then phải tạo change record và re-handoff.

## 17. MODULE CONTRACT 15 - PROMPT-TO-DEV HANDOFF RESOLVER

## 17.1. Mục tiêu

Prompt-to-Dev Handoff Resolver bàn giao prompt đã review cho dev/coding agent.

Module này không được gọi prompt handoff là Dev Ready hoặc Release Ready nếu chưa qua các gate sau đó.

## 17.2. Scope In

Module được phép:

Nhận PROMPT_READY.

Kiểm tra backlog status.

Kiểm tra execution mode.

Kiểm tra handoff target.

Gửi prompt cho dev/coding agent.

Ghi handoff status.

Ghi điều kiện report.

Ghi điều kiện evidence/smoke.

Ghi release warning.

Ghi TECH-11/TECH-10 boundary.

## 17.3. Scope Out

Module không được:

Gửi prompt chưa review.

Gửi prompt bị chặn.

Gửi prompt thiếu source.

Gửi prompt thiếu P0 điểm chặn.

Gửi prompt thiếu evidence/smoke.

Gọi prompt handoff là implementation complete.

Gọi prompt handoff là release ready.

Gọi prompt handoff là production ready.

Mở Global Gateway.

## 17.4. Upstream Dependency

Phụ thuộc:

Prompt Review Resolver.

Prompt Change Control Resolver.

TECH-11 Dev Handoff Governance.

Backlog-to-Handoff Package.

## 17.5. Downstream Handoff

Bàn giao:

Prompt handoff package.

Dev/coding agent instruction.

Report requirement.

Evidence/smoke requirement.

Handoff status.

Next review step.

## 17.6. P0 điểm chặn

FAIL nếu prompt bị chặn vẫn giao dev.

FAIL nếu handoff không yêu cầu report 14 mục.

FAIL nếu handoff gọi Ready là Release Ready.

## 17.7. Evidence

Evidence tối thiểu:

Prompt handoff ID.

Prompt ID.

Dev/coding agent receiver.

Handoff timestamp.

Required report note.

Reviewer decision.

## 17.8. Smoke

## TECH13-MOD-SMK-015

Given PROMPT_READY được bàn giao dev
When Prompt-to-Dev Handoff chạy
Then trạng thái chỉ là PROMPT_HANDOFF_SENT, không Implementation Complete.

## 18. CROSS-MODULE DEPENDENCY MAP

## 18.1. Luồng chuẩn

Luồng chuẩn trong TECH-13:

Prompt Pack Orchestrator xác định phase/backlog.

Phase Prompt Factory tạo prompt draft.

Source Injection Resolver đưa source vào prompt.

Scope Injection Resolver đưa Scope In/Out.

Dependency Injection Resolver đưa dependency.

P0 điểm chặn Injection Resolver đưa P0 điểm chặn.

Evidence/Smoke Injection Resolver đưa evidence/smoke.

Execution Mode Resolver xác định mode.

Agent Guardrail Resolver đưa guardrail.

Codebase Inspection Resolver đưa inspection requirement.

Conflict Detection Resolver đưa conflict requirement.

Report Template Resolver đưa report 14 mục.

Prompt Review Resolver kiểm tra prompt.

Prompt Change Control Resolver kiểm soát thay đổi.

Prompt-to-Dev Handoff Resolver bàn giao dev.

## 18.2. No-bypass rule

Không module nào được bỏ qua:

Source injection.

Scope injection.

Dependency injection.

P0 điểm chặn injection.

Evidence/smoke injection.

Execution mode.

Guardrail.

Codebase inspection requirement.

Conflict detection.

Report template.

Prompt review.

Change control nếu sửa.

Handoff governance.

## 18.3. Dependency ưu tiên

Nếu có mâu thuẫn:

Source-of-truth thắng prompt draft.

Scope Out thắng agent tự mở rộng.

Dependency fail thắng handoff urgency.

P0 điểm chặn thắng convenience.

Evidence/smoke missing thắng dev speed.

Execution mode thắng agent hành vi.

TECH-11 thắng Dev Ready claim.

TECH-10 thắng Release Ready claim.

Global Gateway mặc định bị chặn.

## 19. MODULE P0 điểm chặn REGISTRY - PHẦN 2/4

Các lỗi sau là P0 điểm chặn cấp module:

Prompt Pack Orchestrator tạo prompt toàn hệ thống.

Phase Prompt Factory tạo prompt thiếu execution mode.

Source Injection Resolver bỏ TECH reference.

Scope Injection Resolver bỏ Scope Out.

Dependency Injection Resolver bỏ upstream critical.

P0 điểm chặn Injection Resolver bỏ P0.

Evidence/Smoke Injection Resolver bỏ evidence hoặc smoke.

Execution Mode Resolver cho implement khi chưa code handoff.

Agent Guardrail Resolver bỏ no-hardcode rule.

Codebase Inspection Resolver không yêu cầu inspect repo.

Conflict Detection Resolver không yêu cầu báo conflict.

Report Template Resolver bỏ report 14 mục.

Prompt Review Resolver pass prompt thiếu field critical.

Prompt Change Control sửa prompt âm thầm.

Prompt-to-Dev Handoff gửi prompt bị chặn.

Prompt cho phép copy-paste code rời rạc.

Prompt cho phép agent tự đổi nghiệp vụ.

Prompt cho phép agent gọi Release Ready.

Prompt cho phép agent mở Global Gateway.

Prompt bypass TECH-11/TECH-10.

## 20. EVIDENCE PACKAGE - PHẦN 2/4

## PHẦN 2/4 yêu cầu evidence thiết kế ở cấp module gồm:

Prompt Pack Orchestrator contract accepted.

Phase Prompt Factory contract accepted.

Source Injection Resolver contract accepted.

Scope Injection Resolver contract accepted.

Dependency Injection Resolver contract accepted.

P0 điểm chặn Injection Resolver contract accepted.

Evidence/Smoke Injection Resolver contract accepted.

Execution Mode Resolver contract accepted.

Agent Guardrail Resolver contract accepted.

Codebase Inspection Resolver contract accepted.

Conflict Detection Resolver contract accepted.

Report Template Resolver contract accepted.

Prompt Review Resolver contract accepted.

Prompt Change Control Resolver contract accepted.

Prompt-to-Dev Handoff Resolver contract accepted.

Cross-module dependency accepted.

Module P0 điểm chặn Registry accepted.

Smoke matrix định hướng accepted.

Release handoff checklist prepared.

## 21. SMOKE MATRIX ĐỊNH HƯỚNG - PHẦN 2/4

TECH13-P2-SMK-001 - Prompt Toàn Hệ Thống Bị Reject

Given prompt “triển khai toàn bộ Ginsengfood”
When Prompt Pack Orchestrator review
Then prompt rejected.

TECH13-P2-SMK-002 - Prompt Thiếu Mode Bị Reject

Given prompt không có execution mode
When Phase Prompt Factory review
Then prompt rejected.

TECH13-P2-SMK-003 - Prompt Thiếu Source Bị Reject

Given prompt không có TECH reference
When Source Injection Resolver chạy
Then prompt rejected.

TECH13-P2-SMK-004 - Prompt Thiếu Scope Out Bị Reject

Given prompt không có Scope Out
When Scope Injection Resolver chạy
Then prompt rejected.

TECH13-P2-SMK-005 - Prompt Thiếu Dependency Bị Reject

Given prompt PHASE 6 Ads không nhắc Verified Revenue dependency
When Dependency Injection Resolver chạy
Then prompt rejected.

TECH13-P2-SMK-006 - Prompt Thiếu P0 Bị Reject

Given prompt Commerce không có P0 Payment/Quote/Order boundary
When P0 Injection review
Then prompt rejected.

TECH13-P2-SMK-007 - Prompt Thiếu Evidence/Smoke Bị Reject

Given prompt không yêu cầu evidence/smoke
When Evidence/Smoke Injection review
Then prompt rejected.

TECH13-P2-SMK-008 - Prompt Sai Execution Mode Bị Reject

Given prompt chọn IMPLEMENT WITH GUARD khi chưa qua TECH-11 Code Handoff
When Execution Mode Resolver chạy
Then mode rejected.

TECH13-P2-SMK-009 - Prompt Không Inspect Repo Bị Reject

Given prompt implement nhưng không yêu cầu inspect codebase
When Codebase Inspection review
Then prompt rejected.

TECH13-P2-SMK-010 - Prompt Không Báo Conflict Bị Reject

Given prompt không yêu cầu phát hiện conflict TECH mới vs code cũ
When Conflict Detection review
Then prompt rejected.

TECH13-P2-SMK-011 - Prompt Thiếu Report Format Bị Reject

Given prompt không có report 14 mục
When Report Template Resolver chạy
Then prompt rejected.

TECH13-P2-SMK-012 - Prompt Change Không Audit Bị Reject

Given prompt đã handoff nhưng bị sửa âm thầm
When Change Control review
Then prompt invalid.

TECH13-P2-SMK-013 - Handoff Không Là Implementation Complete

Given PROMPT_READY đã gửi dev
When Prompt-to-Dev Handoff chạy
Then status chỉ là PROMPT_HANDOFF_SENT, không Implementation Complete.

## 22. DONE GATE - PHẦN 2/4

## PHẦN 2/4 đạt Documentation Complete ở cấp module contract khi:

Đã khóa Prompt Pack Orchestrator.

Đã khóa Phase Prompt Factory.

Đã khóa Source Injection Resolver.

Đã khóa Scope Injection Resolver.

Đã khóa Dependency Injection Resolver.

Đã khóa P0 điểm chặn Injection Resolver.

Đã khóa Evidence/Smoke Injection Resolver.

Đã khóa Execution Mode Resolver.

Đã khóa Agent Guardrail Resolver.

Đã khóa Codebase Inspection Resolver.

Đã khóa Conflict Detection Resolver.

Đã khóa Report Template Resolver.

Đã khóa Prompt Review Resolver.

Đã khóa Prompt Change Control Resolver.

Đã khóa Prompt-to-Dev Handoff Resolver.

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

Có module cho prompt thiếu source Ready.

Có module cho prompt thiếu Scope Out Ready.

Có module cho prompt thiếu execution mode Ready.

Có module cho prompt thiếu evidence/smoke Ready.

Có module cho prompt thiếu report format Ready.

Có module cho prompt không inspect repo Ready.

Có module cho prompt không báo conflict Ready.

Có module cho prompt hardcode policy.

Có module cho prompt copy-paste code rời rạc.

Có module cho prompt bypass TECH-11/TECH-10.

Có module cho prompt gọi Release Ready/Production Ready/Go-live Approved.

Có module tự mở Global Gateway.

## 24. RELEASE HANDOFF - SANG PHẦN 3/4

## PHẦN 2/4 bàn giao sang PHẦN 3/4 các nội dung đã khóa:

Danh sách module prompt governance đầy đủ.

Boundary từng module.

Dependency từng module.

Handoff từng module.

P0 điểm chặn từng module.

Evidence từng module.

Smoke từng module.

Cross-module dependency map.

Prompt Pack Orchestrator rule.

Phase Prompt Factory rule.

Source Injection rule.

Scope Injection rule.

Dependency Injection rule.

P0 điểm chặn Injection rule.

Evidence/Smoke Injection rule.

Execution Mode rule.

Agent Guardrail rule.

Codebase Inspection rule.

Conflict Detection rule.

Report Template rule.

Prompt Review rule.

Prompt Change Control rule.

Prompt-to-Dev Handoff rule.

## PHẦN 3/4 sẽ viết:

## PROMPT LIFECYCLE / AGENT EXECUTION STATE MACHINE / PHASE PROMPT FLOW / REPORT-TO-EVIDENCE CONTROL

Trong đó phải khóa rõ:

Prompt lifecycle.

Execution mode state model.

Prompt review state model.

Agent execution state model.

Conflict handling flow.

Codebase inspection flow.

Report submission flow.

Evidence/smoke handoff flow.

Fix-after-review flow.

Prompt change control flow.

Prompt-to-dev handoff flow.

TECH-11/TECH-10 alignment.

Fail-safe agent control.

## 25. TRẠNG THÁI SAU PHẦN 2/4

Sau PHẦN 2/4:

## PHẦN 1/4 đã khóa prompt pack principles.

## PHẦN 2/4 đã khóa module contracts.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Prompt Pack Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa đầy đủ module contracts cho TECH-13.

Các module trọng yếu đã được cố định:

Prompt Pack Orchestrator giữ toàn bộ prompt pack theo PHASE 0 -> PHASE 9.

Phase Prompt Factory tạo prompt draft đúng chuẩn.

Source Injection Resolver buộc prompt có TECH source rõ.

Scope Injection Resolver buộc có Scope In và Scope Out.

Dependency Injection Resolver chặn prompt bỏ upstream/downstream.

P0 điểm chặn Injection Resolver buộc prompt có lỗi cấm tuyệt đối.

Evidence/Smoke Injection Resolver buộc prompt yêu cầu bằng chứng và smoke.

Execution Mode Resolver kiểm soát agent được làm gì.

Agent Guardrail Resolver chặn hardcode, copy-paste, tự đổi nghiệp vụ.

Codebase Inspection Resolver buộc agent inspect repo thật.

Conflict Detection Resolver buộc agent báo mâu thuẫn TECH mới/code cũ.

Report Template Resolver buộc report 14 mục.

Prompt Review Resolver chặn prompt thiếu field critical.

Prompt Change Control Resolver kiểm soát thay đổi prompt sau khi Ready/handoff.

Prompt-to-Dev Handoff Resolver chỉ gửi prompt đã review, không gọi implementation complete hay release ready.

## PHẦN 2/4 sẵn sàng bàn giao sang:

## PHẦN 3/4 - PROMPT LIFECYCLE / AGENT EXECUTION STATE MACHINE / PHASE PROMPT FLOW / REPORT-TO-EVIDENCE CONTROL.

## PHẦN 3/4 - PROMPT LIFECYCLE / AGENT EXECUTION STATE MACHINE / PHASE PROMPT FLOW / REPORT-TO-EVIDENCE CONTROL

## 1. MỤC TIÊU PHẦN 3/4

## PHẦN 3/4 khóa vòng đời của prompt từ lúc được tạo theo phase/backlog cho đến khi được review, handoff cho dev/coding agent, agent thực thi, nộp report, evidence, smoke và bàn giao lại cho TECH-11/TECH-10 theo đúng gate.

PHẦN này xác định rõ:

Prompt đi qua những trạng thái nào.

Execution mode vận hành ra sao.

Prompt review vận hành thế nào.

Agent execution state machine hoạt động ra sao.

Agent phải inspect codebase thật trước khi sửa thế nào.

Agent phát hiện conflict giữa TECH mới và code cũ ra sao.

Agent nộp report theo 14 mục thế nào.

Report chuyển thành evidence ra sao.

Smoke/test/build output được kiểm soát thế nào.

Fix-after-review vận hành ra sao.

Prompt change control vận hành thế nào.

Prompt-to-dev handoff được kiểm soát thế nào.

TECH-13 liên kết với TECH-11 và TECH-10 thế nào.

Khi thiếu source, thiếu mode, thiếu evidence, smoke, report hoặc conflict thì fail-safe ra sao.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế database chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa prompt lifecycle, agent execution state machine, report-to-evidence control và handoff governance.

## 2. NGUYÊN TẮC LIFECYCLE CHUNG

Toàn bộ lifecycle của prompt trong TECH-13 phải tuân thủ các nguyên tắc sau:

Prompt không source-of-truth thì không được tạo thành prompt Ready.

Prompt thiếu Scope Out thì không được giao dev.

Prompt thiếu dependency thì không được giao dev.

Prompt thiếu P0 điểm chặn thì không được giao dev.

Prompt thiếu evidence/smoke thì không được giao dev.

Prompt thiếu execution mode thì không được giao dev.

Prompt thiếu report format thì không được giao dev.

Prompt thiếu guardrail thì không được giao dev.

Prompt không yêu cầu inspect codebase thật thì không được dùng cho implementation.

Prompt không yêu cầu báo conflict thì không được dùng.

Prompt cho phép hardcode policy thì phải bị reject.

Prompt cho phép copy-paste code rời rạc thì phải bị reject.

Prompt cho phép agent tự đổi nghiệp vụ thì phải bị reject.

Prompt cho phép agent gọi Release Ready / Production Ready / Go-live Approved thì phải bị reject.

Prompt handoff không phải Implementation Complete.

Agent report không phải Release Ready.

Agent build/test pass không phải Production Ready.

Evidence/smoke output phải đi qua TECH-11/TECH-10.

TECH-13 không mở Global Gateway.

Global Gateway vẫn bị chặn cho tới khi TECH-10 pass.

## 3. PROMPT LIFECYCLE STATE MODEL

## 3.1. Danh sách trạng thái prompt

Mỗi prompt trong TECH-13 có các trạng thái:

PROMPT_NOT_CREATED
Prompt chưa được tạo.

PROMPT_DRAFTING
Prompt đang được soạn theo phase/backlog.

PROMPT_NEED_SOURCE_INJECTION
Prompt chưa có source-of-truth rõ.

PROMPT_NEED_SCOPE_INJECTION
Prompt chưa có Scope In / Scope Out.

PROMPT_NEED_DEPENDENCY_INJECTION
Prompt chưa có dependency.

PROMPT_NEED_P0_INJECTION
Prompt chưa có P0 điểm chặn.

PROMPT_NEED_EVIDENCE_SMOKE_INJECTION
Prompt chưa có evidence/smoke.

PROMPT_NEED_EXECUTION_MODE
Prompt chưa có mode thực thi.

PROMPT_NEED_GUARDRAIL
Prompt chưa có guardrail.

PROMPT_NEED_REPORT_TEMPLATE
Prompt chưa có report format 14 mục.

PROMPT_REVIEW_WAITING
Prompt đang chờ review.

PROMPT_REJECTED
Prompt bị reject.

PROMPT_NEED_REWORK
Prompt cần sửa.

PROMPT_READY_FOR_HANDOFF
Prompt đủ điều kiện bàn giao dev/coding agent.

PROMPT_HANDOFF_SENT
Prompt đã gửi cho dev/coding agent.

PROMPT_EXECUTION_IN_PROGRESS
Agent đang thực thi.

PROMPT_EXECUTION_REPORTED
Agent đã nộp report.

PROMPT_REPORT_REVIEW_WAITING
Report đang được review.

PROMPT_REPORT_REJECTED
Report bị reject.

PROMPT_REPORT_ACCEPTED_FOR_TECH11
Report được accepted ở cấp handoff về TECH-11.

PROMPT_SUPERSEDED
Prompt bị thay thế bởi prompt mới.

PROMPT_CANCELLED
Prompt bị hủy có lý do.

## 3.2. Transition hợp lệ

Các transition hợp lệ:

## PROMPT_NOT_CREATED -> PROMPT_DRAFTING.

## PROMPT_DRAFTING -> PROMPT_NEED_SOURCE_INJECTION.

## PROMPT_NEED_SOURCE_INJECTION -> PROMPT_NEED_SCOPE_INJECTION.

## PROMPT_NEED_SCOPE_INJECTION -> PROMPT_NEED_DEPENDENCY_INJECTION.

## PROMPT_NEED_DEPENDENCY_INJECTION -> PROMPT_NEED_P0_INJECTION.

## PROMPT_NEED_P0_INJECTION -> PROMPT_NEED_EVIDENCE_SMOKE_INJECTION.

## PROMPT_NEED_EVIDENCE_SMOKE_INJECTION -> PROMPT_NEED_EXECUTION_MODE.

## PROMPT_NEED_EXECUTION_MODE -> PROMPT_NEED_GUARDRAIL.

## PROMPT_NEED_GUARDRAIL -> PROMPT_NEED_REPORT_TEMPLATE.

## PROMPT_NEED_REPORT_TEMPLATE -> PROMPT_REVIEW_WAITING.

## PROMPT_REVIEW_WAITING -> PROMPT_REJECTED.

## PROMPT_REVIEW_WAITING -> PROMPT_NEED_REWORK.

## PROMPT_REVIEW_WAITING -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_READY_FOR_HANDOFF -> PROMPT_HANDOFF_SENT.

## PROMPT_HANDOFF_SENT -> PROMPT_EXECUTION_IN_PROGRESS.

## PROMPT_EXECUTION_IN_PROGRESS -> PROMPT_EXECUTION_REPORTED.

## PROMPT_EXECUTION_REPORTED -> PROMPT_REPORT_REVIEW_WAITING.

## PROMPT_REPORT_REVIEW_WAITING -> PROMPT_REPORT_REJECTED.

## PROMPT_REPORT_REVIEW_WAITING -> PROMPT_REPORT_ACCEPTED_FOR_TECH11.

Bất kỳ trạng thái nào -> PROMPT_SUPERSEDED nếu có prompt mới thay thế.

Bất kỳ trạng thái nào -> PROMPT_CANCELLED nếu owner quyết định hủy.

## 3.3. Transition bị cấm

Các transition bị cấm:

## PROMPT_NOT_CREATED -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_DRAFTING -> PROMPT_HANDOFF_SENT.

## PROMPT_NEED_SOURCE_INJECTION -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_NEED_SCOPE_INJECTION -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_NEED_P0_INJECTION -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_NEED_EVIDENCE_SMOKE_INJECTION -> PROMPT_READY_FOR_HANDOFF.

## PROMPT_NEED_EXECUTION_MODE -> PROMPT_HANDOFF_SENT.

## PROMPT_REJECTED -> PROMPT_HANDOFF_SENT.

PROMPT_HANDOFF_SENT -> Implementation Complete.

PROMPT_REPORT_ACCEPTED_FOR_TECH11 -> Release Ready.

PROMPT_REPORT_ACCEPTED_FOR_TECH11 -> Production Ready.

PROMPT_REPORT_ACCEPTED_FOR_TECH11 -> Go-live Approved.

## 3.4. P0 điểm chặn

FAIL nếu prompt chưa review đã giao dev.

FAIL nếu prompt thiếu source nhưng handoff.

FAIL nếu prompt thiếu evidence/smoke nhưng handoff.

FAIL nếu report accepted rồi bị gọi là Release Ready.

FAIL nếu prompt lifecycle bypass TECH-11 hoặc TECH-10.

## 4. EXECUTION MODE STATE MODEL

## 4.1. Danh sách trạng thái execution mode

Execution mode có các trạng thái:

MODE_NOT_SELECTED
Chưa chọn mode.

MODE_ANALYSIS_ONLY
Chỉ phân tích, không sửa file.

MODE_IMPLEMENT_WITH_GUARD
Được sửa file trong phạm vi đã duyệt.

MODE_TEST_SMOKE_ONLY
Chỉ test/smoke/build, không sửa business logic.

MODE_FIX_AFTER_REVIEW
Chỉ sửa điểm chặn đã được liệt kê.

MODE_DOCUMENTATION_HANDOFF_UPDATE_ONLY
Chỉ cập nhật tài liệu/handoff.

MODE_REJECTED
Mode không hợp lệ.

MODE_REVIEW_REQUIRED
Cần review lại mode.

## 4.2. Rule chọn mode

Mode được chọn theo trạng thái backlog/handoff:

Chưa qua Code Handoff Control -> chỉ được ANALYSIS ONLY.

Đã qua Code Handoff Control -> có thể IMPLEMENT WITH GUARD.

Đang cần test lại -> TEST / SMOKE ONLY.

Có điểm chặn đã review -> FIX AFTER REVIEW.

Chỉ cần cập nhật tài liệu/handoff -> DOCUMENTATION / HANDOFF UPDATE ONLY.

## 4.3. Transition bị cấm

MODE_ANALYSIS_ONLY -> Agent sửa file.

MODE_TEST_SMOKE_ONLY -> Agent sửa business logic.

MODE_DOCUMENTATION_HANDOFF_UPDATE_ONLY -> Agent sửa code logic.

MODE_FIX_AFTER_REVIEW -> Agent sửa ngoài điểm chặn.

MODE_IMPLEMENT_WITH_GUARD khi chưa qua TECH-11 Code Handoff Control.

## MODE_NOT_SELECTED -> PROMPT_HANDOFF_SENT.

## 4.4. P0 điểm chặn

FAIL nếu prompt không có mode.

FAIL nếu agent tự đổi mode.

FAIL nếu ANALYSIS ONLY nhưng agent sửa file.

FAIL nếu IMPLEMENT WITH GUARD khi chưa được phép.

## 5. PROMPT REVIEW STATE MODEL

## 5.1. Danh sách trạng thái review

Prompt review có các trạng thái:

## REVIEW_NOT_STARTED.

## REVIEW_CHECKING_SOURCE.

## REVIEW_CHECKING_SCOPE.

## REVIEW_CHECKING_DEPENDENCY.

## REVIEW_CHECKING_P0.

## REVIEW_CHECKING_EVIDENCE_SMOKE.

## REVIEW_CHECKING_MODE.

## REVIEW_CHECKING_GUARDRAIL.

## REVIEW_CHECKING_REPORT_TEMPLATE.

## REVIEW_PASS.

## REVIEW_FAIL.

## REVIEW_NEED_REWORK.

## REVIEW_BLOCKED_BY_BACKLOG.

## REVIEW_BLOCKED_BY_TECH11.

## 5.2. Review Pass Rule

Prompt chỉ REVIEW_PASS khi có:

Phase.

Backlog/task.

Execution mode.

Source-of-truth cụ thể.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Guardrail.

Codebase inspection requirement.

Conflict detection requirement.

Report format 14 mục.

Release/Gateway warning.

Không có prompt-level P0 điểm chặn.

## 5.3. Review Fail Rule

Prompt REVIEW_FAIL nếu:

Thiếu source.

Thiếu Scope Out.

Thiếu P0 điểm chặn.

Thiếu evidence/smoke.

Thiếu execution mode.

Thiếu report format.

Cho copy-paste code.

Cho hardcode policy.

Cho agent tự đổi nghiệp vụ.

Cho release claim.

Bypass TECH-11/TECH-10.

## 5.4. P0 điểm chặn

FAIL nếu prompt review pass khi thiếu field critical.

FAIL nếu reviewer pass prompt yêu cầu copy-paste code.

FAIL nếu prompt thiếu release warning nhưng vẫn gửi dev.

## 6. AGENT EXECUTION STATE MACHINE

## 6.1. Danh sách trạng thái agent execution

Agent execution có các trạng thái:

## AGENT_NOT_STARTED.

## AGENT_READING_PROMPT.

## AGENT_READING_SOURCE_OF_TRUTH.

## AGENT_INSPECTING_CODEBASE.

## AGENT_IDENTIFYING_CURRENT_STATE.

## AGENT_DETECTING_CONFLICTS.

## AGENT_PLANNING_IMPLEMENTATION.

## AGENT_WAITING_FOR_APPROVAL_IF_REQUIRED.

## AGENT_IMPLEMENTING_WITH_GUARD.

## AGENT_TESTING_BUILDING.

## AGENT_COLLECTING_EVIDENCE.

## AGENT_PREPARING_REPORT.

## AGENT_REPORT_SUBMITTED.

## AGENT_BLOCKED_BY_CONFLICT.

## AGENT_BLOCKED_BY_DEPENDENCY.

## AGENT_BLOCKED_BY_MISSING_RUNTIME_CONFIG.

## AGENT_BLOCKED_BY_TEST_FAILURE.

## AGENT_BLOCKED_BY_SCOPE.

## AGENT_COMPLETED_FOR_REVIEW.

## 6.2. Luồng execution chuẩn

Luồng chuẩn:

## AGENT_READING_PROMPT.

## AGENT_READING_SOURCE_OF_TRUTH.

## AGENT_INSPECTING_CODEBASE.

## AGENT_IDENTIFYING_CURRENT_STATE.

## AGENT_DETECTING_CONFLICTS.

## AGENT_PLANNING_IMPLEMENTATION.

Nếu mode cho phép: AGENT_IMPLEMENTING_WITH_GUARD.

## AGENT_TESTING_BUILDING.

## AGENT_COLLECTING_EVIDENCE.

## AGENT_PREPARING_REPORT.

## AGENT_REPORT_SUBMITTED.

## AGENT_COMPLETED_FOR_REVIEW.

## 6.3. Execution bị chặn khi

Agent phải dừng hoặc báo điểm chặn khi:

Source-of-truth không tìm thấy.

Codebase không có module liên quan.

Code cũ trái TECH mới.

Tài liệu cũ trái TECH mới.

Dependency upstream thiếu.

Runtime/config/policy thiếu.

Test/build fail.

Scope không rõ.

Cần DB/API/UI impact review nhưng chưa có.

Không thể tạo evidence.

Có risk hardcode policy.

Có risk sửa ngoài scope.

## 6.4. P0 điểm chặn

FAIL nếu agent sửa code trước khi đọc source.

FAIL nếu agent sửa code trước khi inspect codebase.

FAIL nếu agent gặp conflict nhưng vẫn tự xử lý im lặng.

FAIL nếu agent test fail nhưng báo complete.

## 7. CODEBASE INSPECTION FLOW

## 7.1. Mục tiêu

Codebase Inspection Flow đảm bảo agent không viết code tưởng tượng.

Agent phải hiểu project thật trước khi sửa.

## 7.2. Agent phải inspect

Agent phải inspect:

Project structure.

Module liên quan.

Existing patterns.

Naming conventions.

Current implementation state.

Tests hiện có.

Build/lint/typecheck command.

Config/runtime hiện có.

Migration/seed nếu có impact.

Docs/handoff hiện có nếu liên quan.

## 7.3. Inspection output bắt buộc

Agent phải ghi trong report:

File/khu vực đã inspect.

Current implementation summary.

Gaps với TECH source-of-truth.

Files modified nếu có.

Files không sửa nhưng liên quan.

Risk nếu không tìm thấy pattern/test/config.

## 7.4. P0 điểm chặn

FAIL nếu agent tạo file mới không theo project pattern.

FAIL nếu agent sửa file không liên quan scope.

FAIL nếu agent không báo file đã inspect.

FAIL nếu agent không báo current implementation state.

## 8. CONFLICT HANDLING FLOW

## 8.1. Các loại conflict

Agent phải phát hiện các loại conflict:

TECH mới vs tài liệu cũ.

TECH mới vs code cũ.

Backlog vs codebase.

Requirement vs test cũ.

Policy mới vs config cũ.

State machine mới vs implementation cũ.

Evidence/audit rule mới vs code không audit.

Public/private boundary mới vs behavior cũ.

Release gate mới vs current pipeline.

## 8.2. Xử lý conflict

Khi có conflict:

Không tự chọn code cũ.

Không dùng legacy behavior để làm chuẩn.

Không sửa âm thầm.

Ghi conflict trong report.

Phân loại severity.

Đề xuất điểm chặn nếu P0.

Đề xuất cần owner review nếu cần.

Chỉ tiếp tục phần không bị ảnh hưởng nếu scope cho phép.

## 8.3. Conflict report format

Conflict phải ghi:

Conflict ID.

TECH source.

Current code/file nếu có.

Conflict summary.

Impact.

## P0/P1/P2.

Suggested action.

Owner/reviewer required.

## 8.4. P0 điểm chặn

FAIL nếu conflict P0 bị ghi như warning nhẹ.

FAIL nếu code cũ thắng TECH mới.

FAIL nếu conflict không xuất hiện trong report.

FAIL nếu agent tự hợp thức hóa legacy behavior.

## 9. IMPLEMENTATION PLAN FLOW

## 9.1. Khi nào cần implementation plan

Agent phải lập plan trước khi sửa code nếu:

Mode là IMPLEMENT WITH GUARD.

Task có nhiều file.

Task có risk DB/API/UI.

Task ảnh hưởng high-risk command.

Task ảnh hưởng payment/order/revenue.

Task ảnh hưởng AI/Gateway/Ads/Live/IVR.

Task ảnh hưởng evidence/audit.

Task có source conflict.

## 9.2. Nội dung plan

Implementation plan phải có:

Requirement summary.

Files/modules expected.

Changes proposed.

Risk.

Test/build plan.

Evidence plan.

Rollback/undo note nếu phù hợp.

Out-of-scope note.

điểm chặn if any.

## 9.3. Plan không phải approval release

Implementation plan không phải:

Implementation Complete.

Evidence Accepted.

Smoke Pass.

Release Ready.

Production Ready.

Go-live Approved.

## 9.4. P0 điểm chặn

FAIL nếu agent sửa high-risk flow mà không có plan.

FAIL nếu plan bỏ test/build/evidence.

FAIL nếu plan không nêu risk khi có DB/API/UI impact.

## 10. REPORT SUBMISSION FLOW

## 10.1. Report nộp khi nào

Agent phải nộp report khi:

Hoàn tất analysis-only.

Hoàn tất implementation.

Hoàn tất test/smoke-only.

Hoàn tất fix-after-review.

Hoàn tất documentation/handoff update.

Bị bị chặn và không thể tiếp tục.

Có conflict P0.

Có test/build fail.

## 10.2. Report 14 mục bắt buộc

Report phải gồm:

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

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

## 10.3. Report khi bị bị chặn

Nếu bị bị chặn, report vẫn phải có:

Lý do bị bị chặn.

Source liên quan.

File/module liên quan nếu đã inspect.

Dependency thiếu.

Evidence thiếu.

Owner cần review.

Đề xuất next action.

## 10.4. P0 điểm chặn

FAIL nếu agent chỉ báo “done”.

FAIL nếu report thiếu file list.

FAIL nếu report thiếu test/build status.

FAIL nếu report giấu điểm chặn.

FAIL nếu report gọi Release Ready.

## 11. REPORT REVIEW STATE MODEL

## 11.1. Danh sách trạng thái report review

Report review có các trạng thái:

## REPORT_NOT_RECEIVED.

## REPORT_RECEIVED.

## REPORT_UNDER_REVIEW.

## REPORT_ACCEPTED_FOR_HANDOFF.

## REPORT_NEED_MORE_INFO.

## REPORT_REJECTED.

## REPORT_BLOCKED_BY_P0.

## REPORT_RETEST_REQUIRED.

## REPORT_SUPERSEDED.

## 11.2. Report accepted khi

Report accepted khi có:

14 mục đầy đủ hoặc ghi “không áp dụng” hợp lý.

Source requirement rõ.

File list rõ.

Commands run rõ.

Test/build result rõ.

Evidence rõ.

Risk rõ.

Handoff update rõ.

Không gọi release ready.

Không giấu điểm chặn.

## 11.3. Report rejected khi

Report rejected nếu:

Thiếu source.

Thiếu file list.

Thiếu lệnh chạy.

Thiếu test/build result.

Thiếu evidence.

Thiếu risk.

Thiếu handoff.

Có release claim.

Có điểm chặn nhưng ghi pass.

Có conflict nhưng không report.

## 11.4. P0 điểm chặn

FAIL nếu report accepted dù thiếu test/build/evidence.

FAIL nếu report accepted dù gọi Production Ready.

FAIL nếu report accepted dù có P0 điểm chặn open.

## 12. REPORT-TO-EVIDENCE CONTROL

## 12.1. Mục tiêu

Report-to-Evidence Control chuyển nội dung report thành evidence package cho TECH-11/TECH-10 review.

## 12.2. Report không tự thành evidence accepted

Agent report chỉ là submission.

Report chưa phải Evidence Accepted.

Evidence phải được review theo TECH-10.

## 12.3. Evidence từ report

Có thể trích từ report:

File list.

Commands run.

Test/build output.

Migration/seed output.

Smoke output.

Audit/correlation reference.

Screenshots/logs nếu có.

Risk/điểm chặn.

Handoff note.

## 12.4. Evidence bị reject nếu

Không map requirement.

Không rõ environment.

Không có expected/actual.

Không có reviewer.

Lộ dữ liệu nhạy cảm.

Chỉ là lời nói.

Không có output thực tế.

Không trace được smoke/test.

## 12.5. P0 điểm chặn

FAIL nếu report được coi là evidence accepted ngay.

FAIL nếu evidence chưa review mà Completion Decision.

FAIL nếu test/build output thiếu nhưng evidence accepted.

## 13. SMOKE / TEST / BUILD CONTROL

## 13.1. Agent phải chạy gì

Tùy scope, agent phải chạy hoặc báo rõ không chạy được:

Unit test.

Integration test.

Typecheck.

Lint.

Backend build.

Frontend build.

Migration test nếu có.

Seed validation nếu có.

Smoke script nếu có.

Manual smoke evidence nếu chưa có script.

## 13.2. Khi không chạy được

Nếu không chạy được test/build/smoke, agent phải báo:

Lệnh không chạy được.

Lý do.

Missing dependency.

Environment limitation.

Risk.

Suggested next action.

Không được ghi pass nếu chưa chạy.

## 13.3. Build/test fail

Nếu build/test fail:

Không được báo complete.

Phải ghi lỗi.

Phải phân loại lỗi thuộc scope hay ngoài scope.

Nếu mode cho phép fix thì đề xuất fix.

Nếu không thuộc scope thì báo điểm chặn.

Không được giấu fail.

## 13.4. P0 điểm chặn

FAIL nếu agent không chạy test nhưng ghi pass.

FAIL nếu build fail nhưng báo done.

FAIL nếu smoke fail nhưng báo ready.

FAIL nếu agent bỏ qua migration/seed failure.

## 14. FIX-AFTER-REVIEW FLOW

## 14.1. Khi nào dùng

Dùng FIX AFTER REVIEW khi:

Prompt trước đã chạy.

Report đã nộp.

Reviewer phát hiện điểm chặn.

điểm chặn đã được liệt kê.

Scope fix rõ.

Retest requirement rõ.

## 14.2. Rule của Fix After Review

Agent chỉ được:

Sửa điểm chặn được liệt kê.

Không mở rộng scope.

Không sửa business rule khác.

Không refactor ngoài scope.

Không hardcode để fix nhanh.

Không bỏ test.

Chạy retest.

Nộp fix report.

## 14.3. Fix report phải có

điểm chặn ID.

Root cause.

Files changed.

Fix summary.

Retest command.

Retest result.

Remaining risk.

Handoff update.

## 14.4. P0 điểm chặn

FAIL nếu fix mode sửa ngoài điểm chặn.

FAIL nếu fix không có retest.

FAIL nếu fix hardcode policy.

FAIL nếu fix che giấu root cause.

## 15. PROMPT CHANGE CONTROL FLOW

## 15.1. Khi nào cần change control

Cần change control khi:

Prompt đã Ready nhưng thay source.

Prompt đã Ready nhưng thay scope.

Prompt đã Ready nhưng thay dependency.

Prompt đã Ready nhưng thay execution mode.

Prompt đã Ready nhưng thay P0 điểm chặn.

Prompt đã Ready nhưng thay evidence/smoke.

Prompt đã handoff dev nhưng cần sửa.

Prompt bị superseded bởi prompt mới.

## 15.2. Change record phải có

Change ID.

Prompt ID.

Before.

After.

Reason.

Impact.

Owner/reviewer.

Re-review required.

Re-handoff required.

Dev/QA notification.

## 15.3. Change bị cấm

Không được:

Sửa prompt âm thầm.

Xóa P0 điểm chặn.

Xóa evidence/smoke.

Chuyển mode sang implement khi chưa được phép.

Bỏ report format.

Bỏ guardrail.

Bỏ release warning.

## 15.4. P0 điểm chặn

FAIL nếu prompt đã giao dev bị sửa âm thầm.

FAIL nếu prompt change làm mất source/evidence/smoke.

FAIL nếu dev không được thông báo prompt change.

## 16. PROMPT-TO-DEV HANDOFF FLOW

## 16.1. Điều kiện handoff

Prompt chỉ được handoff khi:

Prompt Review Pass.

Execution Mode hợp lệ.

Source injected.

Scope injected.

Dependency injected.

P0 điểm chặn injected.

Evidence/smoke injected.

Guardrail injected.

Report template attached.

No open prompt điểm chặn.

## 16.2. Handoff package phải có

Prompt ID.

Phase.

Backlog/task.

Execution mode.

Source-of-truth.

Scope In/Out.

Dependency.

P0 điểm chặn.

Evidence/smoke.

Report template.

Receiver.

Handoff timestamp.

Next review step.

## 16.3. Handoff không có nghĩa là

Prompt handoff không có nghĩa:

Dev Ready hoàn toàn.

Implementation Complete.

Evidence Accepted.

Smoke Pass.

Release Ready.

Production Ready.

Go-live Approved.

Global Gateway Open.

## 16.4. P0 điểm chặn

FAIL nếu handoff gửi prompt bị chặn.

FAIL nếu handoff thiếu report template.

FAIL nếu handoff gọi prompt sent là implementation complete.

## 17. TECH-11 / TECH-10 ALIGNMENT

## 17.1. Liên kết với TECH-11

TECH-13 chỉ được giao prompt dựa trên:

TECH-12 Backlog Pack.

TECH-11 Dev Handoff Governance.

TECH-11 Code Handoff Control.

TECH-11 Implementation Report Format.

TECH-11 Phase Exit Gate.

Nếu chưa qua TECH-11 Code Handoff Control, không dùng IMPLEMENT WITH GUARD.

## 17.2. Liên kết với TECH-10

TECH-13 không xét:

Completion Decision.

Release Ready.

Release Approved.

Go-live Approved.

Scale Approved.

Global Gateway PASS.

Các trạng thái này thuộc TECH-10.

## 17.3. Handoff về TECH-10

Chỉ sau khi:

Prompt đã handoff.

Agent thực thi.

Report accepted.

Evidence mapped.

Smoke mapped.

QA/retest nếu có.

Phase exit accepted theo TECH-11.

thì mới bàn giao sang TECH-10 review.

## 17.4. P0 điểm chặn

FAIL nếu TECH-13 bypass TECH-11.

FAIL nếu TECH-13 bypass TECH-10.

FAIL nếu prompt report tự gọi Release Ready.

## 18. FAIL-SAFE AGENT CONTROL

## 18.1. Khi source thiếu

Nếu source thiếu:

Agent không được sửa code.

Report điểm chặn.

Prompt status cần rework.

Route source review.

## 18.2. Khi dependency thiếu

Nếu dependency thiếu:

Agent không được release-level implement.

Có thể analysis-only nếu mode cho phép.

Report dependency điểm chặn.

Chờ upstream pass.

## 18.3. Khi codebase không khớp

Nếu codebase không có pattern/module như prompt kỳ vọng:

Không tự tạo kiến trúc tùy ý.

Report gap.

Đề xuất implementation plan.

Chờ review nếu impact lớn.

## 18.4. Khi runtime/config thiếu

Nếu thiếu runtime/config/policy:

Không hardcode.

Report điểm chặn.

Đề xuất config/runtime need.

Không tự tạo policy tạm.

## 18.5. Khi test/build fail

Nếu test/build fail:

Không báo complete.

Báo lỗi.

Nếu trong scope, fix theo mode.

Nếu ngoài scope, báo điểm chặn.

Không giấu fail.

## 18.6. Khi agent không chắc

Nếu agent không chắc:

Không tự đoán.

Ghi uncertainty.

Report risk.

Đề xuất owner/dev review.

Không sửa business rule.

## 19. P0 điểm chặn REGISTRY - PHẦN 3/4

Các lỗi sau là P0 điểm chặn cấp lifecycle/state machine:

Prompt thiếu source nhưng handoff.

Prompt thiếu execution mode nhưng handoff.

Prompt thiếu Scope Out nhưng handoff.

Prompt thiếu P0 điểm chặn nhưng handoff.

Prompt thiếu evidence/smoke nhưng handoff.

Prompt thiếu report format nhưng handoff.

Prompt chưa review nhưng handoff.

Prompt rejected nhưng handoff.

Agent sửa file trong ANALYSIS ONLY.

Agent sửa business logic trong TEST / SMOKE ONLY.

Agent sửa ngoài điểm chặn trong FIX AFTER REVIEW.

Agent sửa code trước khi inspect repo.

Agent không báo conflict TECH mới vs code cũ.

Agent dùng code cũ thắng TECH mới.

Agent hardcode policy.

Agent copy-paste code rời rạc.

Agent bỏ test/build.

Agent test fail nhưng báo complete.

Agent report thiếu 14 mục.

Report accepted nhưng thiếu evidence.

Report gọi Release Ready.

Prompt change không audit.

Prompt handoff gọi Implementation Complete.

TECH-13 bypass TECH-11.

TECH-13 bypass TECH-10.

TECH-13 mở Global Gateway.

## 20. SMOKE ĐỊNH HƯỚNG - PHẦN 3/4

TECH13-P3-SMK-001 - Prompt Lifecycle Blocks Missing Source

Given prompt draft chưa có source
When Prompt Lifecycle chạy
Then prompt chuyển PROMPT_NEED_SOURCE_INJECTION, không handoff.

TECH13-P3-SMK-002 - Mode Blocks Wrong Action

Given MODE_ANALYSIS_ONLY
When agent cố sửa file
Then execution fail và report violation.

TECH13-P3-SMK-003 - Review Blocks Missing Report Template

Given prompt thiếu report format 14 mục
When Prompt Review chạy
Then PROMPT_REJECTED.

TECH13-P3-SMK-004 - Agent Must Inspect Codebase

Given IMPLEMENT_WITH_GUARD prompt
When agent bắt đầu
Then phải inspect codebase trước khi sửa.

TECH13-P3-SMK-005 - Conflict Must Be Reported

Given code cũ khác TECH mới
When agent phát hiện
Then report conflict/điểm chặn, không im lặng sửa lệch rule.

TECH13-P3-SMK-006 - Build Fail Not Complete

Given build/test fail
When agent report
Then không được ghi completed/pass.

TECH13-P3-SMK-007 - Report To Evidence Not Auto Accepted

Given agent report có test output
When evidence review chưa accepted
Then không gọi Completion Decision.

TECH13-P3-SMK-008 - Fix Mode Limited

Given FIX_AFTER_REVIEW
When agent sửa ngoài điểm chặn list
Then execution violation.

TECH13-P3-SMK-009 - Prompt Change Requires Re-handoff

Given prompt đã gửi dev nhưng thay scope
When Change Control chạy
Then phải re-review và re-handoff.

TECH13-P3-SMK-010 - Handoff Not Release Ready

Given prompt handoff sent and agent report accepted
When release status xét
Then không Release Ready nếu chưa qua TECH-10.

TECH13-P3-SMK-011 - Runtime Missing Blocks Hardcode

Given runtime/config thiếu
When agent triển khai
Then agent phải báo điểm chặn, không hardcode.

TECH13-P3-SMK-012 - Global Gateway Still bị chặn

Given prompt execution completed
When chưa có TECH-10 evidence/smoke/sign-off/release decision
Then Global Gateway vẫn bị chặn.

## 21. EVIDENCE PACKAGE - PHẦN 3/4

## PHẦN 3/4 yêu cầu evidence thiết kế gồm:

Prompt Lifecycle State Model accepted.

Execution Mode State Model accepted.

Prompt Review State Model accepted.

Agent Execution State Machine accepted.

Codebase Inspection Flow accepted.

Conflict Handling Flow accepted.

Implementation Plan Flow accepted.

Report Submission Flow accepted.

Report Review State Model accepted.

Report-to-Evidence Control accepted.

Smoke/Test/Build Control accepted.

Fix-after-Review Flow accepted.

Prompt Change Control Flow accepted.

Prompt-to-Dev Handoff Flow accepted.

TECH-11 / TECH-10 Alignment accepted.

Fail-safe Agent Control accepted.

P0 điểm chặn Registry accepted.

Smoke định hướng accepted.

## 22. DONE GATE - PHẦN 3/4

## PHẦN 3/4 đạt Documentation Complete ở cấp lifecycle/state machine khi:

Đã khóa Prompt Lifecycle State Model.

Đã khóa Execution Mode State Model.

Đã khóa Prompt Review State Model.

Đã khóa Agent Execution State Machine.

Đã khóa Codebase Inspection Flow.

Đã khóa Conflict Handling Flow.

Đã khóa Implementation Plan Flow.

Đã khóa Report Submission Flow.

Đã khóa Report Review State Model.

Đã khóa Report-to-Evidence Control.

Đã khóa Smoke/Test/Build Control.

Đã khóa Fix-after-Review Flow.

Đã khóa Prompt Change Control Flow.

Đã khóa Prompt-to-Dev Handoff Flow.

Đã khóa TECH-11 / TECH-10 Alignment.

Đã khóa Fail-safe Agent Control.

Đã có P0 điểm chặn Registry.

Đã có Smoke định hướng.

Đã có Evidence Package.

Đã sẵn sàng chuyển sang PHẦN 4/4.

## 23. FAIL GATE - PHẦN 3/4

## PHẦN 3/4 FAIL nếu:

Thiếu Prompt Lifecycle State Model.

Thiếu Execution Mode State Model.

Thiếu Prompt Review State Model.

Thiếu Agent Execution State Machine.

Thiếu Codebase Inspection Flow.

Thiếu Conflict Handling Flow.

Thiếu Report Submission Flow.

Thiếu Report-to-Evidence Control.

Thiếu Smoke/Test/Build Control.

Thiếu Fix-after-Review Flow.

Thiếu Prompt Change Control Flow.

Thiếu TECH-11/TECH-10 Alignment.

Cho prompt thiếu source handoff.

Cho prompt thiếu mode handoff.

Cho agent sửa code trong Analysis Only.

Cho agent bỏ inspect codebase.

Cho agent không báo conflict.

Cho report thiếu 14 mục accepted.

Cho report tự thành Evidence Accepted.

Cho implementation report gọi Release Ready.

Cho TECH-13 mở Global Gateway.

## 24. RELEASE HANDOFF - SANG PHẦN 4/4

## PHẦN 3/4 bàn giao sang PHẦN 4/4 các nội dung đã khóa:

Prompt Lifecycle.

Execution Mode State Model.

Prompt Review State Model.

Agent Execution State Machine.

Codebase Inspection Flow.

Conflict Handling Flow.

Implementation Plan Flow.

Report Submission Flow.

Report Review State Model.

Report-to-Evidence Control.

Smoke/Test/Build Control.

Fix-after-Review Flow.

Prompt Change Control Flow.

Prompt-to-Dev Handoff Flow.

TECH-11 / TECH-10 Alignment.

Fail-safe Agent Control.

P0 điểm chặn Registry.

Smoke định hướng.

Evidence Package.

## PHẦN 4/4 sẽ viết:

FINAL PHASE PROMPT PACK / EXECUTION HANDOFF MATRIX / REPORT TEMPLATE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-13 FINAL CONCLUSION

Trong đó phải khóa rõ:

Final prompt pack PHASE 0 -> PHASE 9.

Execution handoff matrix.

Mode matrix.

Evidence/smoke/report matrix.

Prompt review matrix.

Agent execution matrix.

Final implementation report template.

Final prompt smoke matrix.

Final evidence package.

Owner/reviewer points.

Final Done Gate.

Final Fail Gate.

TECH-13 Final Conclusion.

## 25. TRẠNG THÁI SAU PHẦN 3/4

Sau PHẦN 3/4:

## PHẦN 1/4 đã khóa prompt pack principles.

## PHẦN 2/4 đã khóa module contracts.

## PHẦN 3/4 đã khóa prompt lifecycle, agent execution state machine, phase prompt flow và report-to-evidence control.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Prompt Pack Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời prompt và agent execution cho TECH-13.

Các điểm trọng yếu đã được cố định:

Prompt phải đi từ draft -> source injection -> scope injection -> dependency -> P0 -> evidence/smoke -> mode -> guardrail -> report template -> review -> handoff.

Prompt thiếu source không được handoff.

Prompt thiếu Scope Out không được handoff.

Prompt thiếu execution mode không được handoff.

Prompt thiếu evidence/smoke/report không được handoff.

Agent không được sửa file trong ANALYSIS ONLY.

Agent không được sửa business logic trong TEST / SMOKE ONLY.

Agent chỉ được sửa điểm chặn đã liệt kê trong FIX AFTER REVIEW.

Agent phải đọc source-of-truth.

Agent phải inspect codebase thật.

Agent phải phát hiện và báo conflict giữa TECH mới và code cũ.

Agent không được hardcode policy.

Agent không được copy-paste code rời rạc.

Agent không được tự đổi nghiệp vụ.

Agent không được giấu build/test fail.

Agent report phải đủ 14 mục.

Agent report không tự thành Evidence Accepted.

Agent report không được gọi Release Ready.

Prompt change sau handoff phải có change record và re-handoff.

TECH-13 chỉ bàn giao report/evidence về TECH-11/TECH-10, không tự xét release.

TECH-13 không mở Global Gateway.

Global Gateway vẫn bị chặn cho tới khi TECH-10 có accepted evidence, smoke pass, owner sign-off và release decision.

## PHẦN 3/4 sẵn sàng bàn giao sang:

## PHẦN 4/4 - FINAL PHASE PROMPT PACK / EXECUTION HANDOFF MATRIX / REPORT TEMPLATE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-13 FINAL CONCLUSION.

## PHẦN 4/4 - FINAL PHASE PROMPT PACK / EXECUTION HANDOFF MATRIX / REPORT TEMPLATE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-13 FINAL CONCLUSION

## 1. MỤC TIÊU PHẦN 4/4

## PHẦN 4/4 khóa bản cuối của Dev Prompt Pack dùng để giao việc cho dev/Codex/Copilot/coding agent theo từng phase.

PHẦN này xác định rõ:

Final Prompt Pack cho PHASE 0 -> PHASE 9.

Execution Handoff Matrix.

Execution Mode Matrix.

Evidence / Smoke / Report Matrix.

Prompt Review Matrix.

Agent Execution Matrix.

Final Implementation Report Template.

Final Prompt Smoke Matrix.

Final Evidence Package.

Owner / Reviewer Points.

Final Done Gate.

Final Fail Gate.

Release Handoff.

TECH-13 Final Conclusion.

## PHẦN 4/4 không viết code.

## PHẦN 4/4 không thiết kế API chi tiết.

## PHẦN 4/4 không thiết kế database chi tiết.

## PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa prompt/handoff chuẩn để dev triển khai trong codebase thật.

## 2. FINAL PROMPT PACK PRINCIPLE

TECH-13 khẳng định nguyên tắc cuối cùng:

Prompt không phải code.

Prompt handoff không phải implementation complete.

Agent report không phải evidence accepted.

Build/test pass không phải release ready.

TECH-13 không mở Global Gateway.

Một prompt chỉ hợp lệ khi có đủ:

Phase.

Backlog/task.

Execution mode.

Source-of-truth.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Guardrail.

Codebase inspection requirement.

Conflict detection requirement.

Report format 14 mục.

Release/Gateway warning.

Nếu thiếu bất kỳ điểm cốt lõi nào:

Prompt không READY.

## 3. FINAL PHASE PROMPT PACK - PHASE 0

## PROMPT-P0 - FOUNDATION / RBAC / AUDIT / EVIDENCE / IDEMPOTENCY EXECUTION HANDOFF

## 3.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 0 - Foundation.

PHASE 0 là nền bắt buộc cho toàn bộ hệ thống.

Không có PHASE 0, các phase sau không được triển khai high-risk flow ở mức release.

## 3.2. Source-of-Truth bắt buộc

## TECH-00 - Global Technical Governance.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-10 - Global Evidence / Release Gateway.

## TECH-11 - Implementation Roadmap.

## TECH-12 - PHASE 0 Backlog Matrix.

## 3.3. Execution Mode mặc định

Bắt đầu bằng:

## MODE: ANALYSIS ONLY

Chỉ chuyển sang:

## MODE: IMPLEMENT WITH GUARD

khi TECH-11 Code Handoff Control đã pass.

## 3.4. Scope In

Inspect codebase foundation hiện tại.

Xác định RBAC/permission hiện có.

Xác định actor identity hiện có.

Xác định audit/evidence hiện có.

Xác định idempotency hiện có.

Xác định correlation/trace context hiện có.

Phát hiện gap với TECH-01.

Đề xuất implementation plan.

Nếu được phép implement, chỉ sửa đúng scope PHASE 0.

Chạy test/build phù hợp.

Nộp report 14 mục.

## 3.5. Scope Out

Không triển khai Product.

Không triển khai Operational.

Không triển khai Commerce.

Không triển khai AI.

Không triển khai Gateway.

Không triển khai Ads.

Không triển khai Live.

Không triển khai IVR.

Không hardcode permission.

Không bỏ audit/evidence cho high-risk command.

Không gọi Foundation pass là Release Ready.

Không mở Global Gateway.

## 3.6. P0 điểm chặn

High-risk command không audit.

Admin override không reason/evidence/audit.

Evidence không map requirement.

Permission không chặn action sai quyền.

Idempotency không chặn double action.

Correlation ID thiếu ở flow cần trace.

Dev report thiếu test/build/evidence.

Implementation done bị gọi là Production Ready.

## 3.7. Evidence Required

File/module đã inspect.

Current implementation summary.

Permission matrix hoặc permission gap.

Audit/evidence gap.

Idempotency gap.

Test/build output.

Risk/điểm chặn list.

Report 14 mục.

## 3.8. Smoke Required

User thiếu quyền -> action bị chặn.

High-risk action -> audit created.

Missing evidence -> gate bị chặn.

Duplicate command -> không double action.

Admin override thiếu reason -> bị chặn.

## 3.9. Handoff

Kết quả PHASE 0 chỉ được ghi:

Ready for TECH-11 Review

Không được ghi:

Release Ready.

Production Ready.

Go-live Approved.

Global Gateway PASS.

## 4. FINAL PHASE PROMPT PACK - PHASE 1

## PROMPT-P1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION EXECUTION HANDOFF

## 4.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 1 - Product / SKU / Recipe / Product Activation.

PHASE 1 tạo nền sản phẩm trước Operational, Commerce, AI, Gateway và Live.

## 4.2. Source-of-Truth bắt buộc

## TECH-02 - Product / SKU / Recipe / Product Activation.

## TECH-03 - Operational Core dependency.

## TECH-04 - Commerce Sellable dependency.

## TECH-05 - AI Product Knowledge dependency.

## TECH-12 - PHASE 1 Backlog Matrix.

## 4.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD sau khi PHASE 0 và TECH-11 Code Handoff Control pass.

## 4.4. Scope In

Inspect product/SKU/recipe hiện tại.

Xác định product master structure.

Xác định SKU master structure.

Xác định recipe/formula/version hiện có.

Xác định product activation hiện có.

Xác định public product name handling.

Xác định internal SKU exposure risk.

So sánh với TECH-02.

Báo gap/conflict.

Nếu được phép implement, sửa đúng PHASE 1.

Chạy test/build.

Nộp report 14 mục.

## 4.5. Scope Out

Không tự quyết Sellable.

Không bán hàng.

Không tạo quote.

Không tạo order.

Không triển khai warehouse.

Không triển khai AI response.

Không public internal SKU code.

Không dùng product knowledge chưa approved.

Không hardcode product rule.

Không gọi Product Active là Sellable.

## 4.6. P0 điểm chặn

Product Active bị hiểu là Sellable.

Internal SKU code public ra khách.

Product knowledge chưa approved nhưng downstream dùng.

Recipe/formula không version.

Product activation không evidence/audit.

Code cũ khác TECH-02 nhưng vẫn được giữ làm chuẩn.

## 4.7. Evidence Required

Product/SKU/recipe current state.

Activation rule evidence.

Public/internal field boundary evidence.

Formula/version evidence.

Test/build output.

Report 14 mục.

## 4.8. Smoke Required

Product Active != Sellable.

Internal SKU not public.

Unapproved product knowledge bị chặn.

Formula version required before operational usage.

## 5. FINAL PHASE PROMPT PACK - PHASE 2

## PROMPT-P2 - OPERATIONAL CORE EXECUTION HANDOFF

## 5.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 2 - Operational Core.

Operational Core là nền cho Sellable, Inventory, Traceability, Recall và Sale Lock.

## 5.2. Source-of-Truth bắt buộc

## TECH-03 - Operational Core.

## TECH-02 - Product/SKU/Recipe dependency.

## TECH-01 - Audit/Evidence/Idempotency dependency.

## TECH-04 - Commerce dependency.

## TECH-12 - PHASE 2 Backlog Matrix.

## 5.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi PHASE 0 và PHASE 1 dependency pass.

## 5.4. Scope In

Inspect operational modules hiện tại.

Xác định raw intake/QC/readiness flow.

Xác định production order snapshot flow.

Xác định material issue/receipt.

Xác định batch execution/release.

Xác định warehouse/inventory ledger.

Xác định trace/public trace.

Xác định recall/sale lock.

So sánh với TECH-03.

Báo conflict/gap.

Nếu được phép implement, sửa đúng scope PHASE 2.

Chạy test/build.

Nộp report 14 mục.

## 5.5. Scope Out

Không tạo Commerce quote/order.

Không tự bán SKU.

Không quyết định giá.

Không triển khai AI.

Không triển khai Ads.

Không triển khai IVR.

Không public dữ liệu trace ngoài whitelist.

Không mutate ledger history.

Không bypass audit/evidence.

Không gọi QC_PASS là RELEASED.

## 5.6. P0 điểm chặn

## RAW_LOT QC_PASS = READY_FOR_PRODUCTION.

Raw lot chưa ready vẫn issue.

Material Receipt giảm tồn lần hai.

Batch QC_PASS = RELEASED.

Warehouse nhận batch chưa RELEASED.

Inventory ledger không append-only.

Public trace không whitelist-only.

Recall/Sale Lock không chặn downstream.

## 5.7. Evidence Required

Raw lot readiness evidence.

Material issue/receipt evidence.

Batch release evidence.

Warehouse receipt evidence.

Inventory ledger evidence.

Public trace whitelist evidence.

Recall/Sale Lock evidence.

Test/build output.

Report 14 mục.

## 5.8. Smoke Required

## QC_PASS != READY_FOR_PRODUCTION.

READY_FOR_PRODUCTION mới được issue.

Issue decrements raw stock once.

## QC_PASS != RELEASED.

RELEASED mới warehouse receipt.

Recall/Sale Lock blocks Commerce/AI/Ads/Live/IVR.

## 6. FINAL PHASE PROMPT PACK - PHASE 3

## PROMPT-P3 - COMMERCE RUNTIME EXECUTION HANDOFF

## 6.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 3 - Commerce Runtime.

Commerce là nguồn sự thật cho Sellable Gate, QuoteSnapshot, Cart, Order Draft, Official Order, Payment, Shipping và Verified Revenue.

## 6.2. Source-of-Truth bắt buộc

## TECH-04 - Commerce Runtime.

## TECH-03 - Operational Sellable/Sale Lock/Recall dependency.

## TECH-07 - Verified Revenue dependency.

## TECH-09 - Official Order / IVR dependency.

## TECH-12 - PHASE 3 Backlog Matrix.

## 6.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi PHASE 0, PHASE 1 và PHASE 2 relevant dependency pass.

## 6.4. Scope In

Inspect Commerce modules hiện tại.

Xác định Sellable Gate.

Xác định QuoteSnapshot.

Xác định Cart.

Xác định Order Draft.

Xác định Customer Confirmation.

Xác định Official Order.

Xác định Payment boundary.

Xác định COD/Shipping boundary.

Xác định Verified Revenue.

Xác định Member/Diamond/runtime benefit.

So sánh với TECH-04.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 3.

Chạy test/build.

Nộp report 14 mục.

## 6.5. Scope Out

Không tự bypass Operational.

Không bán SKU not sellable.

Không hardcode giá.

Không hardcode payment account.

Không dùng cart làm order.

Không dùng order draft làm official order.

Không coi ảnh chuyển khoản là PAID.

Không coi COD WAITING là Verified Revenue.

Không tạo Ads ROAS.

Không mở Gateway.

## 6.6. P0 điểm chặn

Operational bị chặn nhưng Commerce vẫn bán.

Không QuoteSnapshot mà vẫn final price.

Cart bị hiểu là Order.

Order Draft bị hiểu là Official Order.

CustomerConfirmation chưa CONFIRMED mà tạo official order.

Ảnh chuyển khoản được coi là PAID.

COD WAITING được coi là Verified Revenue.

Verified Revenue không đến từ Commerce.

Payment account hardcode.

## 6.7. Evidence Required

Sellable decision evidence.

QuoteSnapshot evidence.

Cart/order draft/official order evidence.

Payment confirmation evidence.

COD/shipping boundary evidence.

Verified Revenue evidence.

Runtime benefit evidence.

Test/build output.

Report 14 mục.

## 6.8. Smoke Required

Operational bị chặn -> Commerce bị chặn.

QuoteSnapshot is only final price source.

Cart != Order.

Order Draft != Official Order.

Image transfer != PAID.

COD WAITING != Verified Revenue.

Verified Revenue comes from Commerce only.

## 7. FINAL PHASE PROMPT PACK - PHASE 4

## PROMPT-P4 - AI ADVISOR RUNTIME EXECUTION HANDOFF

## 7.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 4 - AI Advisor Runtime.

AI Advisor chỉ là lớp tư vấn, diễn giải, gợi ý và hỗ trợ chốt đơn có kiểm soát.

AI không phải source-of-truth nghiệp vụ.

## 7.2. Source-of-Truth bắt buộc

## TECH-05 - AI Advisor Runtime.

## TECH-02 - Product Knowledge.

## TECH-04 - QuoteSnapshot / Order Draft / Payment dependency.

## TECH-06 - Gateway delivery dependency.

## TECH-12 - PHASE 4 Backlog Matrix.

## 7.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi Product/Commerce dependencies phù hợp.

## 7.4. Scope In

Inspect AI Advisor modules hiện tại.

Xác định product knowledge resolver.

Xác định claim guard.

Xác định customer memory.

Xác định quote consumption.

Xác định order draft handoff.

Xác định final response guard.

Xác định public/private boundary.

Xác định runtime unavailable behavior.

So sánh với TECH-05.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 4.

Chạy test/build.

Nộp report 14 mục.

## 7.5. Scope Out

Không tự tính giá.

Không tự tạo official order.

Không tự xác nhận payment.

Không tự xác nhận Verified Revenue.

Không bán SKU not sellable/Sale Lock/Recall.

Không dùng claim chưa approved.

Không public dữ liệu private.

Không nói chữa bệnh/điều trị/thay thuốc.

Không dùng mã SKU nội bộ với khách.

Không mở Gateway.

## 7.6. P0 điểm chặn

AI tự tính giá.

AI tự tạo official order.

AI tự xác nhận payment.

AI tự xác nhận Verified Revenue.

AI bán SKU not sellable/Sale Lock/Recall.

AI dùng claim chưa approved.

AI public dữ liệu private.

AI nói claim y khoa bị cấm.

Runtime unavailable nhưng AI suy đoán.

## 7.7. Evidence Required

Product knowledge evidence.

Claim guard evidence.

QuoteSnapshot consumption evidence.

Order draft handoff evidence.

Final response guard evidence.

Privacy boundary evidence.

Runtime unavailable fail-safe evidence.

Test/build output.

Report 14 mục.

## 7.8. Smoke Required

AI no self-price.

AI no self-order.

AI no payment confirmation.

Unapproved claim bị chặn.

Private data not public.

Runtime unavailable -> fail-safe.

## 8. FINAL PHASE PROMPT PACK - PHASE 5

## PROMPT-P5 - FACEBOOK GATEWAY / MESSENGER / CHANNEL DELIVERY EXECUTION HANDOFF

## 8.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 5 - Facebook Gateway / Messenger / Channel Delivery.

Gateway là lớp delivery/channel, không phải AI, không phải Commerce, không phải Payment.

## 8.2. Source-of-Truth bắt buộc

## TECH-06 - Facebook Gateway.

## TECH-05 - Final Response Guard.

## TECH-04 - Commerce private quote boundary.

## TECH-08 - Live dependency.

## TECH-12 - PHASE 5 Backlog Matrix.

## 8.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi AI Final Response Guard dependency pass.

## 8.4. Scope In

Inspect Gateway/channel modules hiện tại.

Xác định page/channel context.

Xác định public/private routing.

Xác định comment-to-Messenger handoff.

Xác định Messenger private context.

Xác định Final Response Guard enforcement.

Xác định typing indicator/pacing.

Xác định rate limit/anti-spam.

Xác định moderation/troll/complaint split.

Xác định suppression/opt-out.

So sánh với TECH-06.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 5.

Chạy test/build.

Nộp report 14 mục.

## 8.5. Scope Out

Không tự tạo AI response.

Không tự tính giá.

Không tự tạo order.

Không tự xác nhận payment.

Không public dữ liệu private.

Không gửi response chưa qua guard.

Không kéo troll/malicious vào Messenger sai policy.

Không xử complaint thật như troll.

Không giả mạo người thật.

Không mở Gateway production nếu TECH-10 chưa pass.

## 8.6. P0 điểm chặn

Gateway public dữ liệu private.

Gateway public giá cá nhân/order info sai rule.

Gateway gửi response chưa qua Final Response Guard.

Public price/buy/deep consult không route Messenger.

Troll/malicious bị kéo Messenger sai policy.

Complaint thật bị xử như troll.

Gateway giả mạo người thật.

## 8.7. Evidence Required

Public/private routing evidence.

Messenger handoff evidence.

Final Response Guard enforcement evidence.

Pacing evidence.

Rate-limit evidence.

Moderation evidence.

Suppression evidence.

Test/build output.

Report 14 mục.

## 8.8. Smoke Required

Public comment no private data.

Price/buy intent routes Messenger.

Unguarded response bị chặn.

Troll not pulled to Messenger.

Complaint routed CSKH/Quality.

Suppressed user not messaged.

## 9. FINAL PHASE PROMPT PACK - PHASE 6

## PROMPT-P6 - ADS MEASUREMENT / ATTRIBUTION / VERIFIED REVENUE / SCALE GATE EXECUTION HANDOFF

## 9.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 6 - Ads Measurement.

Ads chỉ dùng Verified Revenue từ Commerce.

Ads không dùng quote, cart, order draft, unpaid order, payment WAITING hoặc COD WAITING làm revenue.

## 9.2. Source-of-Truth bắt buộc

## TECH-07 - Ads Measurement.

## TECH-04 - Verified Revenue.

## TECH-01 - Idempotency / Evidence.

## TECH-10 - Scale Gate / Global Gateway.

## TECH-12 - PHASE 6 Backlog Matrix.

## 9.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi Verified Revenue dependency rõ và TECH-11 Code Handoff Control pass.

## 9.4. Scope In

Inspect ads/event/attribution modules hiện tại.

Xác định event taxonomy.

Xác định Pixel/CAPI dedup.

Xác định attribution source.

Xác định Verified Revenue consumption.

Xác định exclusion rules.

Xác định CPA/AOV/ROAS computation.

Xác định Data Quality gate.

Xác định Scale Gate.

Xác định Sale Lock/Recall suppression.

So sánh với TECH-07.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 6.

Chạy test/build.

Nộp report 14 mục.

## 9.5. Scope Out

Không dùng quote làm revenue.

Không dùng cart làm revenue.

Không dùng order draft làm revenue.

Không dùng unpaid order làm revenue.

Không dùng payment WAITING làm revenue.

Không dùng COD WAITING làm revenue.

Không double count Pixel/CAPI.

Không scale khi Data Quality fail.

Không scale khi Sale Lock/Recall/Suppression active.

Không gọi ROAS đẹp là Scale Approved.

## 9.6. P0 điểm chặn

Fake revenue.

Double count revenue.

Verified Revenue không từ Commerce.

Ads scale khi Data Quality fail.

Ads scale khi Sale Lock/Recall active.

Ads scale khi chưa owner approval.

Ads scale khi Global Gateway bị chặn.

## 9.7. Evidence Required

Event taxonomy evidence.

Dedup/idempotency evidence.

Attribution evidence.

Verified Revenue evidence.

Exclusion rule evidence.

Data Quality evidence.

Scale Gate evidence.

Test/build output.

Report 14 mục.

## 9.8. Smoke Required

Quote/cart/draft/unpaid excluded.

Payment WAITING != revenue.

COD WAITING != revenue.

Verified Revenue only.

Dedup prevents double count.

Data Quality fail blocks scale.

Sale Lock/Recall blocks scale.

## 10. FINAL PHASE PROMPT PACK - PHASE 7

## PROMPT-P7 - MC AI LIVE / LIVE SCRIPT RUNTIME / LIVE SALES CONTROL EXECUTION HANDOFF

## 10.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 7 - MC AI Live.

MC AI Live là lớp hỗ trợ live script và live orchestration, không phải source-of-truth nghiệp vụ.

## 10.2. Source-of-Truth bắt buộc

## TECH-08 - MC AI Live.

## TECH-06 - Gateway / Messenger.

## TECH-05 - AI Advisor / Claim Guard.

## TECH-04 - Commerce price/order boundary.

## TECH-07 - Ads-safe signal boundary.

## TECH-12 - PHASE 7 Backlog Matrix.

## 10.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi Gateway/AI/Commerce dependencies pass.

## 10.4. Scope In

Inspect live modules hiện tại.

Xác định live session context.

Xác định script runtime.

Xác định script guard/claim guard.

Xác định live comment classifier.

Xác định Messenger coordination.

Xác định product/price boundary.

Xác định troll/abuse handling.

Xác định complaint route.

Xác định Ads-safe live signal boundary.

So sánh với TECH-08.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 7.

Chạy test/build.

Nộp report 14 mục.

## 10.5. Scope Out

Không tự báo giá.

Không tự tạo order.

Không tự xác nhận payment.

Không tự xác nhận revenue.

Không dùng live signal làm ROAS.

Không dùng claim chưa approved.

Không kéo troll/malicious vào Messenger sai policy.

Không xử complaint thật như troll.

Không bỏ Gateway rate/suppression.

Không gọi Live Ready nếu chưa TECH-10 pass.

## 10.6. P0 điểm chặn

MC AI Live tự báo giá.

MC AI Live tự tạo order.

MC AI Live tự xác nhận payment/revenue.

Live signal dùng làm ROAS.

Script chưa guard vẫn dùng.

Claim chưa approved vẫn nói.

Troll/malicious kéo Messenger sai policy.

Complaint thật xử như troll.

## 10.7. Evidence Required

Live session context evidence.

Script guard evidence.

Comment classification evidence.

Messenger coordination evidence.

Price/order boundary evidence.

Troll/complaint split evidence.

Live signal boundary evidence.

Test/build output.

Report 14 mục.

## 10.8. Smoke Required

Script guard blocks unsafe script.

Live no self-price.

Live no self-order.

Live signal not revenue.

Troll not pulled Messenger.

Complaint routed correctly.

## 11. FINAL PHASE PROMPT PACK - PHASE 8

## PROMPT-P8 - IVR ORDER CONFIRMATION EXECUTION HANDOFF

## 11.1. Mục tiêu

Giao dev/coding agent triển khai hoặc phân tích PHASE 8 - IVR Order Confirmation.

IVR chỉ xử lý Official Order đủ điều kiện theo Commerce.

IVR không gọi đại trà, không xác nhận payment, không xác nhận revenue.

## 11.2. Source-of-Truth bắt buộc

## TECH-09 - IVR Order Confirmation.

## TECH-04 - Official Order / Core Order State Machine.

## TECH-05 - Customer Memory dependency nếu liên quan trust.

## TECH-10 - Evidence / Privacy / Release Gate.

## TECH-12 - PHASE 8 Backlog Matrix.

## 11.3. Execution Mode mặc định

## MODE: ANALYSIS ONLY

Chỉ chuyển IMPLEMENT WITH GUARD khi Official Order/Core Order State dependency pass.

## 11.4. Scope In

Inspect IVR modules hiện tại.

Xác định IVR eligibility.

Xác định customer trust exclusion.

Xác định Official Order only boundary.

Xác định phone validation.

Xác định attempt policy.

Xác định outcome classification.

Xác định confirm/cancel/need support.

Xác định no answer/max attempts.

Xác định Core Order State handoff.

Xác định Notification Owner handoff.

Xác định privacy-safe call result.

So sánh với TECH-09.

Báo conflict/gap.

Nếu được phép implement, sửa đúng PHASE 8.

Chạy test/build.

Nộp report 14 mục.

## 11.5. Scope Out

Không gọi mọi khách.

Không gọi Quote.

Không gọi Cart.

Không gọi Order Draft.

Không tự tạo order.

Không tự hủy order.

Không tự gửi notification.

Không xác nhận PAID.

Không xác nhận Verified Revenue.

Không đọc full address/payment/member/Diamond/health note sai policy.

Không hardcode attempt policy nếu runtime/config chưa có.

Không mở Global Gateway.

## 11.6. P0 điểm chặn

IVR gọi đại trà.

IVR gọi Quote/Cart/Order Draft.

IVR tự hủy đơn.

IVR tự gửi notification.

IVR confirmation = PAID.

Invalid phone bị ghi no answer.

Customer cancel bị ghi no answer.

Technical failure bị quy lỗi khách.

Notification gửi trước Core cancellation.

## 11.7. Evidence Required

Eligibility evidence.

Customer trust evidence.

Official Order boundary evidence.

Phone validation evidence.

Attempt policy evidence.

Outcome classification evidence.

Core Order handoff evidence.

Notification handoff evidence.

Privacy evidence.

Test/build output.

Report 14 mục.

## 11.8. Smoke Required

Quote/cart/draft cannot enter IVR.

Trusted customer not called đại trà.

Technical failure not customer fault.

Confirm != PAID.

No answer max only handoff Core.

Notification only after Core cancellation.

## 12. FINAL PHASE PROMPT PACK - PHASE 9

## PROMPT-P9 - GLOBAL SMOKE / UAT / RELEASE GATEWAY EXECUTION HANDOFF

## 12.1. Mục tiêu

Giao dev/coding agent hoặc QA/release agent triển khai hoặc phân tích PHASE 9 - Global Smoke / UAT / Release Gateway.

PHASE 9 kiểm soát evidence, smoke, UAT, sign-off, điểm chặn, release decision, Global Gateway và post-go-live.

## 12.2. Source-of-Truth bắt buộc

## TECH-10 - Global Smoke / UAT / Evidence / Release Gateway.

## TECH-11 - Implementation Roadmap / Dev Phase Control.

## TECH-12 - Phase Backlog Pack.

TECH-00 -> TECH-09 - Domain source-of-truth.

## 12.3. Execution Mode mặc định

MODE: ANALYSIS ONLY hoặc TEST / SMOKE ONLY

Chỉ dùng IMPLEMENT WITH GUARD nếu có release tooling/backlog rõ và TECH-11 Code Handoff Control pass.

## 12.4. Scope In

Inspect release/evidence/smoke tooling hiện tại nếu có.

Xác định documentation completion registry.

Xác định evidence intake/acceptance.

Xác định global smoke matrix.

Xác định UAT session control.

Xác định owner sign-off control.

Xác định P0 điểm chặn registry.

Xác định cross-tech dependency validator.

Xác định privacy/security release review.

Xác định rollback/fallback/monitoring readiness.

Xác định release decision/global gateway state.

Xác định post-go-live monitoring.

So sánh với TECH-10.

Báo gap/conflict.

Chạy smoke/test nếu mode cho phép.

Nộp report 14 mục.

## 12.5. Scope Out

Không tự đánh Documentation Complete là Production Ready.

Không tự accepted evidence.

Không tự pass smoke.

Không tự sign-off owner.

Không tự release decision.

Không tự Go-live Approved.

Không tự Scale Approved.

Không mở Global Gateway.

Không bỏ rollback/fallback.

Không bỏ monitoring/alert.

Không bỏ P0 điểm chặn.

## 12.6. P0 điểm chặn

Documentation Complete = Production Ready.

Evidence submitted = Evidence accepted.

Smoke chưa pass nhưng Release Ready.

Owner chưa sign-off nhưng Release Approved.

Không release decision nhưng Go-live Approved.

Global Gateway mở khi còn điểm chặn.

Post-go-live incident open nhưng scale.

Verified Revenue/Data Quality fail nhưng scale.

## 12.7. Evidence Required

Documentation registry evidence.

Evidence intake evidence.

Smoke matrix evidence.

UAT evidence.

Owner sign-off evidence.

P0 điểm chặn evidence.

Dependency evidence.

Privacy/security evidence.

Rollback/fallback evidence.

Monitoring evidence.

Release decision evidence.

Gateway state evidence.

Report 14 mục.

## 12.8. Smoke Required

Documentation Complete != Production Ready.

Evidence not accepted -> no Completion Decision.

Smoke fail -> no Release Ready.

Missing owner sign-off -> no Release Approved.

Missing release decision -> no Go-live Approved.

P0 điểm chặn open -> Gateway bị chặn.

Incident open -> no scale.

## 13. FINAL EXECUTION HANDOFF MATRIX

Prompt

Phase

Default Mode

Required Upstream

Main Output

Not Allowed

## PROMPT-P0

Foundation

## ANALYSIS ONLY

## TECH-01, TECH-11, TECH-12

Foundation gap/report

Release claim

## PROMPT-P1

Product/SKU/Recipe

## ANALYSIS ONLY

## PHASE 0

Product backlog/report

Product Active = Sellable

## PROMPT-P2

Operational

## ANALYSIS ONLY

## PHASE 0/1

Operational report/evidence

Commerce selling

## PROMPT-P3

Commerce

## ANALYSIS ONLY

## PHASE 0/1/2

Commerce report/evidence

Self-price/payment/revenue bypass

## PROMPT-P4

AI Advisor

## ANALYSIS ONLY

## PHASE 1/3

AI report/evidence

AI self-price/order/payment

## PROMPT-P5

Gateway

## ANALYSIS ONLY

## PHASE 4

Gateway report/evidence

Public private data

## PROMPT-P6

Ads

## ANALYSIS ONLY

PHASE 3 Verified Revenue

Ads report/evidence

Fake revenue/scale

## PROMPT-P7

MC AI Live

## ANALYSIS ONLY

## PHASE 4/5/3/6

Live report/evidence

Live signal as ROAS

## PROMPT-P8

## IVR

## ANALYSIS ONLY

PHASE 3 Official Order

IVR report/evidence

IVR self-cancel/SMS/payment

## PROMPT-P9

Release Gateway

## ANALYSIS / TEST ONLY

## TECH-10

Release governance report

Open Gateway without decision

## 14. FINAL EXECUTION MODE MATRIX

Mode

Được làm

Không được làm

Dùng khi

## ANALYSIS ONLY

Inspect, map gap, plan, report

Sửa file

Chưa qua Code Handoff Control

## IMPLEMENT WITH GUARD

Sửa file trong scope

Sửa ngoài scope, hardcode, release claim

Đã qua TECH-11 Code Handoff

## TEST / SMOKE ONLY

Chạy test/build/smoke, thu evidence

Sửa business logic

QA/test/smoke

## FIX AFTER REVIEW

Sửa điểm chặn được liệt kê

Mở rộng scope

Sau review fail

## DOCUMENTATION / HANDOFF UPDATE ONLY

Cập nhật docs/handoff

Sửa code logic

Sau implementation hoặc handoff update

## 15. FINAL EVIDENCE / SMOKE / REPORT MATRIX

Thành phần

Bắt buộc trong prompt

Bắt buộc trong report

Source-of-truth

Có TECH/section/requirement

Có nguồn yêu cầu

Scope In/Out

Có

Nêu đã làm/chưa làm

Dependency

Có

Nêu dependency pass/fail

P0 điểm chặn

Có

Nêu điểm chặn/risk còn lại

Evidence

Có danh sách yêu cầu

Có evidence đã dùng

Smoke

Có danh sách smoke

Có smoke/test result

Test/build

Có expectation

Có lệnh và kết quả

Migration/seed

Có nếu áp dụng

Có kết quả nếu áp dụng

Cleanup

Có expectation

Có cleanup result

Handoff

Có downstream

Có handoff update

Release warning

Có

Không được claim release

## 16. FINAL PROMPT REVIEW MATRIX

Prompt chỉ được PROMPT_READY_FOR_HANDOFF khi pass toàn bộ review sau:

Review

PASS khi

FAIL nếu

Source Review

Có TECH source rõ

Source mơ hồ

Scope Review

Có Scope In/Out

Thiếu Scope Out

Dependency Review

Upstream/downstream rõ

Bỏ dependency

P0 Review

P0 cụ thể

P0 chung chung/thiếu

Evidence Review

Evidence required rõ

Không evidence

Smoke Review

Smoke required rõ

Không smoke

Mode Review

Mode hợp lệ

Không mode/sai mode

Guardrail Review

Cấm hardcode/copy/release claim

Cho agent tự đổi rule

Inspection Review

Yêu cầu inspect codebase

Viết code tưởng tượng

Conflict Review

Yêu cầu báo conflict

Bỏ conflict

Report Review

Có 14 mục

Report thiếu

Gateway Review

Có warning TECH-10

Cho mở Gateway

## 17. FINAL AGENT EXECUTION MATRIX

Agent thực thi đúng khi:

Đọc prompt.

Đọc source-of-truth.

Inspect codebase thật.

Xác định current state.

Phát hiện conflict.

Lập plan nếu cần.

Implement chỉ khi mode cho phép.

Chạy test/build.

Thu evidence.

Nộp report 14 mục.

Không release claim.

Không mở Gateway.

Agent fail nếu:

Sửa file trước khi inspect.

Không đọc source.

Không báo conflict.

Sửa ngoài scope.

Hardcode policy.

Copy-paste code rời rạc.

Bỏ test/build.

Giấu fail.

Report thiếu.

Gọi Production Ready.

## 18. FINAL IMPLEMENTATION REPORT TEMPLATE

Mọi report dev/coding agent phải theo đúng 14 mục sau.

## 18.1. Tóm tắt

Ghi:

Phase.

Prompt ID.

Backlog/task.

Mode.

Đã làm gì.

Chưa làm gì.

Trạng thái hiện tại.

## 18.2. File đã sửa

Ghi:

Backend files.

Frontend files.

Config files.

Migration files.

Seed files.

Test files.

Docs files.

Nếu không áp dụng, ghi rõ Không áp dụng.

## 18.3. Nguồn yêu cầu

Ghi:

TECH number.

Section.

Requirement.

Backlog ID.

Task ID.

Prompt ID.

## 18.4. Evidence đã dùng

Ghi:

Log.

Screenshot/export nếu có.

Test output.

Build output.

Migration output.

Seed validation.

Audit/correlation nếu có.

## 18.5. Lệnh đã chạy

Ghi:

Build.

Test.

Lint.

Typecheck.

Migration.

Seed.

Smoke.

Cleanup.

## 18.6. Kết quả test

Ghi:

Pass.

Fail.

Chưa chạy.

Lý do chưa chạy.

Retest cần thiết.

## 18.7. Kết quả backend build

Ghi:

Pass/fail.

Error.

Warning.

Impact.

## 18.8. Kết quả frontend build

Ghi:

Pass/fail.

Error.

Warning.

Impact.

## 18.9. Kết quả cleanup process

Ghi:

File rác đã xóa.

Code tạm đã bỏ.

Debug log tạm đã bỏ.

Duplicate đã xử lý.

Chưa cleanup gì thì nêu lý do.

## 18.10. Cập nhật Markdown

Ghi:

Docs cập nhật.

README cập nhật.

Handoff cập nhật.

Chưa cập nhật thì nêu lý do.

## 18.11. Kết quả database migration/update nếu áp dụng

Ghi:

Migration name.

Migration status.

Rollback note.

Data impact.

Risk.

## 18.12. Kết quả seed validation nếu áp dụng

Ghi:

Seed file.

Seed data.

Validation result.

Missing seed.

Conflict seed.

## 18.13. Rủi ro còn lại

Ghi:

Known issues.

Open điểm chặn.

Technical debt.

Dependency chưa pass.

Smoke chưa chạy.

Evidence thiếu.

Owner review cần thiết.

## 18.14. Cập nhật handoff

Ghi:

Phase tiếp theo.

Backlog tiếp theo.

Điều kiện để tiếp tục.

điểm chặn cần xử lý.

Owner/QA cần review.

## 19. FINAL PROMPT SMOKE MATRIX

TECH13-FSMK-001 - Prompt Không Source Bị Reject

Given prompt không có TECH source
When review chạy
Then PROMPT_REJECTED.

TECH13-FSMK-002 - Prompt Thiếu Scope Out Bị Reject

Given prompt thiếu Scope Out
When review chạy
Then PROMPT_REJECTED.

TECH13-FSMK-003 - Prompt Thiếu Mode Bị Reject

Given prompt không có execution mode
When review chạy
Then PROMPT_REJECTED.

TECH13-FSMK-004 - Prompt Cho Copy Code Bị Reject

Given prompt yêu cầu copy-paste code rời rạc
When review chạy
Then PROMPT_REJECTED.

TECH13-FSMK-005 - Prompt Cho Hardcode Bị Reject

Given prompt cho hardcode price/payment/IVR/notification policy
When review chạy
Then PROMPT_REJECTED.

TECH13-FSMK-006 - Agent Không Inspect Repo

Given prompt implement nhưng agent không inspect repo
When report review
Then report rejected.

TECH13-FSMK-007 - Agent Không Báo Conflict

Given code cũ khác TECH mới
When agent report
Then conflict phải được báo, nếu không report rejected.

TECH13-FSMK-008 - Agent Test Fail Nhưng Báo Done

Given build/test fail
When agent report done
Then report rejected.

TECH13-FSMK-009 - Report Thiếu 14 Mục

Given report thiếu file list/evidence/test/build/risk/handoff
When review
Then report rejected hoặc Need More Info.

TECH13-FSMK-010 - Report Gọi Release Ready

Given report ghi Release Ready/Production Ready
When review
Then report rejected.

TECH13-FSMK-011 - Prompt Handoff Không Phải Implementation Complete

Given prompt đã gửi dev
When trạng thái cập nhật
Then chỉ là PROMPT_HANDOFF_SENT.

TECH13-FSMK-012 - TECH-10 Required

Given implementation report accepted
When chưa qua TECH-10
Then không Release Ready.

## 20. FINAL EVIDENCE PACKAGE - TECH-13

TECH-13 yêu cầu final evidence package gồm:

## PHẦN 1/4 accepted.

## PHẦN 2/4 accepted.

## PHẦN 3/4 accepted.

## PHẦN 4/4 accepted.

Prompt Pack Principles accepted.

No-Code-Snippet Rule accepted.

Agent Boundary accepted.

Execution Mode Matrix accepted.

Phase Prompt Pack PHASE 0 -> PHASE 9 accepted.

Report Template accepted.

Prompt Review Matrix accepted.

Agent Execution Matrix accepted.

Prompt Smoke Matrix accepted.

P0 điểm chặn Registry accepted.

Release Handoff accepted.

## 21. OWNER / REVIEWER POINTS

## 21.1. Implementation Roadmap Owner

Phải xác nhận:

Prompt chia đúng PHASE 0 -> PHASE 9.

Prompt không gộp toàn hệ thống.

Prompt đúng backlog TECH-12.

Prompt đúng TECH-11 handoff governance.

Prompt không claim release.

## 21.2. Dev Lead

Phải xác nhận:

Prompt đủ context để inspect repo.

Prompt không ép snippet rời rạc.

Prompt có mode rõ.

Prompt có report format.

Prompt có test/build expectation.

Prompt không hardcode policy.

## 21.3. QA Owner

Phải xác nhận:

Prompt có smoke required.

Prompt có test/build output requirement.

Prompt có fail-case/P0 smoke.

Prompt có report đủ cho QA review.

Prompt không giấu risk.

## 21.4. Evidence / Audit Owner

Phải xác nhận:

Prompt có evidence required.

Prompt có audit/correlation nếu high-risk.

Prompt không coi report là evidence accepted.

Prompt không bỏ TECH-10 evidence review.

## 21.5. Release Owner

Phải xác nhận:

TECH-13 không gọi Release Ready.

TECH-13 không gọi Production Ready.

TECH-13 không gọi Go-live Approved.

TECH-13 không mở Global Gateway.

TECH-10 vẫn là release authority.

## 22. FINAL DONE GATE - TECH-13

TECH-13 đạt DOCUMENTATION COMPLETE khi:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Dev Prompt Pack Principles đã khóa.

Agent Boundary đã khóa.

No-Code-Snippet Rule đã khóa.

Prompt Execution Mode đã khóa.

Prompt Guardrail đã khóa.

Prompt Lifecycle đã khóa.

Agent Execution State Machine đã khóa.

Report-to-Evidence Control đã khóa.

Final Phase Prompt Pack đã khóa.

Execution Handoff Matrix đã khóa.

Mode Matrix đã khóa.

Evidence/Smoke/Report Matrix đã khóa.

Prompt Review Matrix đã khóa.

Agent Execution Matrix đã khóa.

Implementation Report Template đã khóa.

Prompt Smoke Matrix đã khóa.

Evidence Package đã khóa.

Owner/Reviewer Points đã khóa.

Final Done Gate đã khóa.

Final Fail Gate đã khóa.

Release Handoff đã khóa.

TECH-13 Final Conclusion đã khóa.

## 23. FINAL FAIL GATE - TECH-13

TECH-13 FAIL nếu còn bất kỳ điểm nào sau:

Prompt không có source-of-truth.

Prompt không có Scope Out.

Prompt không có dependency.

Prompt không có P0 điểm chặn.

Prompt không có evidence required.

Prompt không có smoke required.

Prompt không có execution mode.

Prompt không có report format.

Prompt không yêu cầu inspect codebase.

Prompt không yêu cầu báo conflict.

Prompt yêu cầu copy-paste code rời rạc.

Prompt cho hardcode policy.

Prompt cho agent tự đổi nghiệp vụ.

Prompt cho code cũ thắng TECH mới.

Prompt cho tài liệu cũ thắng TECH mới.

Prompt cho agent sửa ngoài scope.

Prompt cho agent bỏ test/build.

Prompt cho agent bỏ evidence.

Prompt cho agent gọi Release Ready.

Prompt cho agent gọi Production Ready.

Prompt cho agent gọi Go-live Approved.

Prompt cho agent mở Global Gateway.

Prompt không chia theo phase.

Prompt gộp toàn hệ thống.

Report thiếu 14 mục.

Report gọi done khi build/test fail.

Report giấu conflict.

Report tự thành Evidence Accepted.

TECH-13 bypass TECH-11.

TECH-13 bypass TECH-10.

Không có final prompt pack PHASE 0 -> PHASE 9.

## 24. RELEASE HANDOFF - SAU TECH-13

## 24.1. Trạng thái sau khi hoàn tất TECH-13

Sau khi hoàn tất PHẦN 4/4:

Nhưng:

Prompt Pack Ready = DOCUMENTED, NOT EXECUTED

Global Gate bị chặn

Lý do:

TECH-13 mới là tài liệu prompt/handoff.

Chưa có prompt thực tế được gửi dev.

Chưa có dev/coding agent execution.

Chưa có implementation report thật.

Chưa có evidence accepted.

Chưa có smoke pass.

Chưa có QA/UAT.

Chưa có owner sign-off.

Chưa có release decision.

## 24.2. Điều kiện để Prompt Pack Ready thành Dev Ready

Muốn Dev Ready phải có:

Prompt cụ thể theo phase/backlog.

Prompt Review Pass.

TECH-11 Dev Handoff accepted.

Execution mode hợp lệ.

Source/scope/dependency/P0/evidence/smoke đầy đủ.

Dev/coding agent receiver rõ.

Prompt handoff sent.

Nếu chưa:

## 24.3. Điều kiện để Dev Ready thành Implementation Complete

Muốn Implementation Complete phải có:

Agent/dev thực thi trong codebase thật.

Report 14 mục.

File list.

Source trace.

Commands run.

Test/build result.

Evidence.

Risk note.

Handoff update.

Nếu chưa:

## 24.4. Điều kiện để Implementation Complete thành Ready for TECH-10 Review

Muốn Ready for TECH-10 Review phải qua TECH-11:

Implementation report accepted.

Evidence mapped.

Smoke mapped.

QA handoff completed nếu required.

P0 điểm chặn closed.

Cleanup/docs update pass.

Phase exit accepted.

Release handoff package complete.

Nếu chưa:

## 24.5. Điều kiện để Release Ready

Release Ready chỉ do TECH-10 xét.

TECH-13 không có quyền xét Release Ready.

TECH-10 chỉ xét khi có:

Evidence accepted.

Global smoke pass.

UAT pass nếu required.

Owner sign-off.

No open P0 điểm chặn.

Cross-tech dependency pass.

Privacy/security pass.

Rollback/fallback ready nếu production.

Monitoring/alert ready nếu go-live.

Release decision.

Nếu chưa qua TECH-10:

Hạng mục

Trạng thái sau TECH-13

TECH-13 Documentation

Prompt Pack

## DOCUMENTED

Prompt Pack Ready

## DOCUMENTED, NOT EXECUTED

Prompt Handoff Sent

KHONG

Dev Ready

KHONG

Implementation Ready

KHONG

Implementation Complete

KHONG

Evidence Accepted

KHONG

Smoke Pass

KHONG

QA/UAT Pass

KHONG

Owner Sign-off

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

TECH-11 Review

## REQUIRED

TECH-10 Review

## REQUIRED BEFORE RELEASE

## TECH-13 FINAL CONCLUSION

TECH-13 đã khóa lớp Codex / Copilot Dev Prompt Pack / Phase Execution Handoff / Implementation Report Template cho hệ thống Ginsengfood.

TECH-13 không thay thế TECH-00 -> TECH-12.

TECH-13 không viết code.

TECH-13 không tạo snippet copy-paste.

TECH-13 không cho phép coding agent tự suy luận nghiệp vụ.

Các nguyên tắc cuối cùng đã khóa:

Prompt phải chia theo PHASE 0 -> PHASE 9.

Không dùng một prompt cho toàn bộ hệ thống.

Prompt phải có source-of-truth rõ.

Prompt phải có Scope In.

Prompt phải có Scope Out.

Prompt phải có dependency.

Prompt phải có P0 điểm chặn.

Prompt phải có evidence required.

Prompt phải có smoke required.

Prompt phải có execution mode.

Prompt phải có guardrail.

Prompt phải yêu cầu inspect codebase thật.

Prompt phải yêu cầu phát hiện conflict TECH mới vs code cũ.

Prompt phải yêu cầu report 14 mục.

Prompt không được yêu cầu copy-paste code rời rạc.

Prompt không được cho hardcode policy.

Prompt không được cho agent tự đổi business rule.

Prompt không được cho code cũ thắng TECH mới.

Prompt không được cho tài liệu cũ thắng TECH mới.

Prompt không được cho agent gọi Release Ready.

Prompt không được cho agent gọi Production Ready.

Prompt không được cho agent gọi Go-live Approved.

Prompt không được mở Global Gateway.

Agent không được sửa file trong ANALYSIS ONLY.

Agent không được sửa business logic trong TEST / SMOKE ONLY.

Agent không được sửa ngoài điểm chặn trong FIX AFTER REVIEW.

Agent không được sửa code trước khi inspect repo.

Agent không được giấu build/test fail.

Agent report không tự thành Evidence Accepted.

TECH-13 chỉ bàn giao về TECH-11/TECH-10, không tự xét release.

TECH-10 mới xét Evidence Accepted, Smoke Pass, UAT, Owner Sign-off, Release Decision và Global Gateway.

Trạng thái cuối cùng của tài liệu:

Nhưng trạng thái triển khai vẫn là:

Bước tiếp theo đúng là bắt đầu sử dụng TECH-13 để tạo PROMPT-P0 - Foundation / RBAC / Audit / Evidence / Idempotency Execution Handoff cho dev/Codex/Copilot, nhưng chỉ ở MODE: ANALYSIS ONLY trước khi cho phép sửa code.
