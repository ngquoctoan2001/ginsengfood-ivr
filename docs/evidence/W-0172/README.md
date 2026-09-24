# W-0172 — C1+C2 program/result contract invariant hardening

Ngày: `2026-09-04`
Baseline: `main@c213bf7663708dfca7184bf443e66d6552e2daea`
Phạm vi: local domain, callback outbox, PostgreSQL schema/migration và tests.
Trạng thái: `TESTS_PASS_LOCAL / EXTERNAL_SIGNOFF_PENDING`.

## 1. Finding được xác nhận

Program matrix đã fail-closed ở intake/OpenAPI: chỉ `GOLDEN_HOUR + ONLINE` và
`TWENTY_FOUR_SEVEN + COD`, đều bắt buộc `ivr_confirmation_required=true`. Hai runtime writer hiện
tại cũng đang sinh đúng taxonomy.

Gap thật nằm ở invariant dùng chung:

- shared wire enum có 11 value nhưng chỉ 9 value là runtime call result;
- `CallResultSnapshot.Create` trước W-0172 vẫn cho hai pre-call value trở thành snapshot và chưa
  khóa exact counted/finality/action cho mọi result;
- outbox factory chỉ tin cờ `IsFinal`, nên một `NormalizedResult` được dựng trực tiếp có thể giả
  final;
- DB cho đủ 11 value trong attempt/result/callback; không khóa exact `5 counted / 4 not-counted`,
  `6 final / 3 non-final`, hoặc `6 callback`.

Không tìm thấy bằng chứng hai writer hiện tại đã ghi row sai. Đây là hardening ngăn writer mới hoặc
direct entity write phá contract.

## 2. Khắc phục

- `ResultContractPolicy` là authority local cho `11 shared / 9 runtime / 6 final callback /
  3 non-final / 2 pre-call-only`, exact counted/finality và core-action recommendation.
- `CallResultSnapshot.Create` từ chối hai pre-call value và mọi semantic tuple không canonical.
- `CallbackOutboxSnapshotFactory` chỉ nhận sáu final result, không chỉ dựa vào `IsFinal`.
- EF model khóa attempt/result/callback bằng exact CHECK constraints.
- Migration `20260904090000_W0172ProgramResultContractInvariants` preflight toàn bộ ba bảng, nêu ID
  row vi phạm trước khi thay constraint; `Down` phục hồi đúng shape trước W-0172.
- Rolling-deploy gate chỉ có exemption exact-key cho migration này; không có wildcard/broad bypass.
- Negative PostgreSQL tests phủ pre-call value, finality mismatch, non-final callback và migration
  preflight.

## 3. Impact review

| Symbol/gate | GitNexus risk | Kết quả |
| --- | --- | --- |
| `CallResultSnapshot.Create` | LOW | shared domain guard; caller hiện tại đã audit |
| `CallbackOutboxSnapshotFactory.Create` | LOW | hai production caller hiện tại conform |
| `ConfigureAttempt/ConfigureResult/ConfigureCallback` | LOW | 1 caller trực tiếp mỗi method, 0 process |
| `IvrDbContextModelSnapshot.BuildModel` | LOW | 0 upstream |
| `RollingDeploySchemaCompatibilityTests` | **CRITICAL** | 183 impacted, 86 direct, 0 process; đã cảnh báo trước edit; chỉ thêm bảy exact exemption có preflight |

## 4. Verification hiện tại

| Check | Kết quả |
| --- | --- |
| Baseline focused unit | **PASS `15/15`** |
| Baseline contract | **PASS `24/24`** |
| W-0172 focused unit | **PASS `26/26`** |
| Full unit | **PASS `510/510`**, 0 fail/skip |
| Full contract | **PASS `24/24`**, 0 fail/skip |
| Release build `-warnaserror` | **PASS**, 0 warning/0 error |
| `dotnet format --verify-no-changes` | **PASS** |
| EF pending-model check | **PASS** — no change since latest migration snapshot |
| Traceability generator/gate | **PASS `485`**; full unit rerun green |
| Focused PostgreSQL `IT-RESULT-CONTRACT-*` | **PASS `7/7`**, 0 fail/skip |
| Fixture regression sau taxonomy hardening | **PASS `24/24`**, 0 fail/skip |
| Full PostgreSQL integration | **PASS `239/239`**, 0 fail/skip |

Docker engine đã sẵn sàng ở lượt xác minh tiếp theo. Lượt focused đầu tiên chạy thật đạt `5/7`; hai
case pre-call bị DB chặn hợp lệ ở constraint `action_matches_type` nhưng assertion chỉ chấp nhận ba
constraint khác. Sau khi sửa assertion, focused đạt `7/7`.

Lượt full đầu tiên đạt `215/239`: 24 test dùng ba fixture cũ có tuple trái taxonomy mới
(`IVR_TECHNICAL_EXCEPTION` bị gắn final, `IVR_NO_ANSWER_FINAL` bị gắn not-counted). Chỉ fixture và
expectation aggregate được sửa; không nới production constraint. Rerun ba nhóm đạt `24/24`, sau đó
full integration trên build biệt lập dưới repository root đạt `239/239` trong `9m33s`.

## 5. Boundary và phần còn lại

- W-0172 không đổi OpenAPI/wire enum, producer decision, scheduler policy hoặc callback ACK.
- W-0161 historical `236/236` vẫn chỉ thuộc exact tree trước đó; W-0172 có bằng chứng current riêng
  `7/7` focused và `239/239` full integration.
- M3 assembler/CDC, generic callback consumer, Product result/attempt policy and shared E2E remain
  external and `NOT_RECEIVED/NOT_RUN`.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Next action: giữ invariant và migration trong exact release candidate, rồi M3/Product cung cấp
assembler/CDC, generic callback consumer, policy approval và shared-E2E evidence. Không còn local
code/test action bắt buộc cho W-0172 nếu không xuất hiện finding mới.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-RESULT-CONTRACT-01`, `UT-RESULT-CONTRACT-02`, `UT-RESULT-CONTRACT-03`, `IT-RESULT-CONTRACT-PRECALL-17`, `IT-RESULT-CONTRACT-FINALITY-18`, `IT-RESULT-CONTRACT-ACTION-19`, `IT-RESULT-CONTRACT-OUTBOX-19`, `IT-RESULT-CONTRACT-PREFLIGHT-20`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0172 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
