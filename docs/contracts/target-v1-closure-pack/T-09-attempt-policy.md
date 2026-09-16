# T-09 — `attempt_policy_version` cho production

External work `W-0007` · `OD-V1-08` + `OD-V1-16` · correction `W-0151` · gate
**production** · trạng thái `W0151_EVIDENCE_SUBMITTED / OPEN_EXTERNAL`

Owner quyết định bắt buộc: **Product + Order Core + Module 3**.

Decision sheet chi tiết:
[M8-11 attempt-policy production decision pack](../../../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md).

Due: trước release gate `P9-1`. Ngày cam kết: `<owner điền>`.

## 1. Current evidence

### Candidate, không phải production

`src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs` khai báo `mock-lab-v1`:

| Program | Attempts | Offsets | Window | Approval |
| --- | ---: | --- | ---: | --- |
| Golden Hour | 2 | `[0,150]` giây | 300 giây | `CandidateMockLabOnly` |
| 24/7 | 2 | `[0,450]` giây | 900 giây | `CandidateMockLabOnly` |

Phạm vi seed cần đọc chính xác:

- dev seed đăng ký `mock-lab-v1` chỉ cho `MOCK`, `approved=false`;
- dev UI loader có thể đăng ký candidate cho `MOCK` + `LAB_REAL_SIM`;
- lab seed mặc định không dùng `mock-lab-v1`, mà dùng `lab-softphone-v1` với một attempt.

Không artifact nào ở trên là production approval.

### Hai bộ số xung đột

`D-10` và candidate dùng bộ số ở bảng trên. Hai tài liệu phase-8 business không có banner
supersession lại ghi:

- Golden Hour: 2 attempts, offsets `[0,300]`, window 600 giây;
- 24/7: 3 attempts, offsets `[0,300,600]`, window 900 giây.

| Delta | Candidate / `D-10` | Phase-8 business |
| --- | --- | --- |
| GH offset lần 2 | 150s | 300s |
| GH window | 300s | 600s |
| 24/7 attempts | 2 | 3 |
| 24/7 offsets | `[0,450]` | `[0,300,600]` |

Chưa có chữ ký Product/Order Core/M3 chọn bộ nào. Module 8 không tự sửa business source.

## 2. Wire rule hiện có — correction của W-0151

Task bắt buộc mang **cả version lẫn snapshot**:

- `attempt_policy_version`;
- `max_customer_attempts`;
- `attempt_offsets_seconds`;
- `confirmation_window_started_at` và `confirmation_window_expires_at`.

Current intake không tin payload một chiều. Nó:

1. resolve registry theo `(version, program, execution_mode)`;
2. so exact max attempts, ordered offsets và window duration;
3. trả `409 IVR_POLICY_MISMATCH`, không tạo job, nếu bất kỳ giá trị nào lệch.

Vì vậy claim cũ “hiện chưa có quy tắc khi version và tham số mâu thuẫn” là **sai và đã được
thu hồi**. Câu hỏi còn mở không phải mismatch behavior hiện tại, mà là Product/M3 có ký giữ
contract `version + snapshot + exact 409` cho production hay không.

Khi task được accepted, IVR ghi immutable snapshot vào task/job và scheduler dùng schedule đã lưu.
Registry đổi sau đó không rewrite job đang chạy.

## 3. Registry và runtime gap

Current registry đủ làm primitive nhưng chưa đủ production governance:

- mỗi `(version, program)` là một row; chưa có atomic two-program bundle;
- writer có audit hash nhưng không có approval reference, signer authority hoặc four-eyes verifier;
- không có effective/retire/active state hay environment rollout record;
- update bị chặn, nhưng delete protection/lifecycle chưa được chứng minh;
- DB chỉ CHECK max bounds và positive window; full offset invariants nằm ở EF/domain path;
- không tìm thấy governed production registration route; caller hiện thấy là dev seed loader.

Feature flag pre-dial từ chối exact literal `mock-lab-v1`/`UNAPPROVED`, nhưng không resolve registry
và không so flag version với policy snapshot của job. Do đó intake gate và pre-dial gate đang là hai
nguồn có thể drift; production coherence phải được ký trước khi sửa code.

## 4. Counting/retry/temporal gap

Scheduler dùng counted attempts để chọn offset kế tiếp. Technical exception không đốt customer
attempt; `TechnicalRetryLimit` mặc định `1` là config riêng và không nằm trong versioned policy.
Retry có thể requeue cùng attempt number mà chưa có signed production backoff.

Wire/registry cũng không có timezone, quiet-hours, holiday hoặc window-crossing rule. Product và M3
phải quyết định producer tránh giờ cấm, hay IVR defer/truncate/reject; Module 8 không tự suy.

## 5. M3 producer evidence

Audit snapshot `ginsengfood-business-platform` `PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656`
không tìm thấy Target V1 producer/schema cho version, max attempts, offsets và confirmation window.
Chỉ kết luận `M3_ATTEMPT_POLICY_PRODUCER_NOT_FOUND` trên snapshot đã ghim; M3 cần giao producer SHA,
OpenAPI/schema, CDC và sandbox payload thật.

## 6. Owner phải trả lời

Mọi dòng `ATP-01..ATP-15` trong decision pack phải có signer/date/reference. Tối thiểu:

- authority và nguồn nào supersede nguồn nào;
- một version production **mới**, canonical bundle/hash đủ hai program;
- exact attempts/offsets/window/T0/clock skew;
- counted/terminal taxonomy và technical retry/backoff/manual retry;
- timezone/quiet-hours/holiday/window-crossing;
- giữ hay đổi wire `version + snapshot`, source of truth và mismatch behavior;
- producer/distribution ordering; registry four-eyes/lifecycle/database validation;
- effective/cutover/retire/in-flight behavior;
- coherent pre-dial rule giữa active policy và job snapshot;
- capacity, dial-token, rollout, rollback, audit và retention.

## 7. Acceptance evidence để đóng T-09

- [ ] Product + Order Core + M3 ký `ATP-01..ATP-15` đúng phạm vi.
- [ ] Signed two-program policy bundle có canonical SHA-256 và production version mới.
- [ ] M3 producer commit/OpenAPI/schema/CDC phát version + snapshot exact.
- [ ] Owner business sửa hoặc supersede các tài liệu số xung đột.
- [ ] Registry governance, four-eyes, DB validation và pre-dial coherence implementation đã review.
- [ ] Shared tests phủ unknown/disallowed/mismatch/partial/drift/cutover/rollback/retry/window.
- [ ] Capacity/telephony/dial-token recalibration theo signed policy đã hoàn tất.
- [ ] Production release packet được đúng authority chấp nhận.

## 8. Mock fallback và stop rule

MOCK/dev/lab có thể tiếp tục dùng candidate đúng environment seed. Production không được:

- promote, rename hoặc flip approval cho `mock-lab-v1`;
- chọn số từ một nguồn bất kỳ rồi sửa scheduler/registry/OpenAPI/DB/seed/config;
- gọi local/mock test là owner approval;
- bật real-customer call trước shared evidence và release sign-off.

W-0151 chỉ nộp audit và decision pack. Production policy vẫn `NOT_APPROVED` và
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 9. Offline intake hardening — W-0180

W-0180 thêm [production-bundle validator](../../evidence/W-0180/README.md) để kiểm machine-readable
bundle khi Product, Order Core và M3 gửi quyết định thật. PASS của validator chỉ cho phép mở review
implementation riêng; nó không tự đóng checklist §7, không chọn strategy ATP-11 và không cho phép
promote `mock-lab-v1` hoặc bật production.
