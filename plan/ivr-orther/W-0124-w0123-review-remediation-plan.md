# W-0124 — Khắc phục phát hiện rà soát W-0123

Ngày lập: `2026-08-27`

Baseline: `main@ef09a062597f8f43dad41be751ace03ef5f5973f` + worktree W-0123 chưa commit

Origin: `UNPLANNED` — owner yêu cầu khắc phục toàn bộ phát hiện rà soát ngày `2026-08-27`

Trạng thái: `TESTS_PASS` cho local implementation · external gates của W-0123 **không** được đóng bởi work này

Prereq: W-0123 `TESTS_PASS`

> W-0124 **không** đảo bất kỳ quyết định nào của `OD-18`. Nó đóng sáu phát hiện của lượt rà soát
> W-0123: một khoảng trống đo lường, một CI gate đỏ sẵn, một guard test hẹp, ranh giới commit, và
> hai chỗ tài liệu/môi trường lệch.

---

## 1. Nguồn gốc

Lượt rà soát W-0123 xác minh lại toàn bộ số liệu evidence bằng cách chạy độc lập: .NET 801/801,
admin UI 223/223, 7 node gate, oasdiff pinned theo digest, GitNexus detect-changes. Không con số
nào sai.

Sáu phát hiện còn lại không phải lỗi trong cutover, mà là những chỗ W-0123 **đúng nhưng chưa đóng
được** — và mỗi chỗ đều đóng được ngay trong repo này, không cần bên ngoài.

## 2. Phát hiện và cách khắc phục

### F1 — Không đo được delta hành vi thật · `MEDIUM`

**Vấn đề.** `OD-18` là thay đổi hành vi một chiều: đơn trước kia *có thể* bị skip nay **sẽ bị
gọi**. W-0123 lập luận rằng hiện chưa ai bị skip, vì Module 3 chưa gửi
`trust.risk_evidence_available` (theo `W-0118`). Lập luận đúng, nhưng nó là **suy luận** — target
DB `ENV_BLOCKED`, không có cách nào đếm để xác nhận.

**Khắc phục.** Counter `ivr_legacy_skip_candidate_total`, ghi tại **intake**, đếm task mang đúng
hình dạng predicate đã nghỉ hưu: không có veto `trusted_skip_allowed=false`, `risk_flags` rỗng, và
`trust.risk_evidence_available=true`. Snapshot hỏng hoặc thiếu trả `false` — predicate cũ cũng đòi
bằng chứng dương, nên "không xác định được" chưa bao giờ là skip, và counter đoán mò sẽ thổi phồng
chính con số nó tồn tại để đo.

Ghi ở intake chứ không ở eligibility là có chủ ý: đọc trust metadata để **quyết định** là điều
`OD-18` cấm; đọc để **đếm producer đã gửi gì** là kiểm toán. F3 khoá ranh giới đó theo file.

Không gắn alert. Non-zero không phải lỗi phía IVR và không có hành động runtime nào để gọi ai dậy
lúc 2 giờ sáng — nó nghĩa là M3 vẫn gửi tín hiệu đã được yêu cầu ngừng gửi, và việc cần làm là một
trao đổi tích hợp theo `IR-06`. Nhưng mỗi increment là một cuộc gọi thật tới một khách hàng thật,
nên nó phải nằm trên dashboard chứ không nằm trong log.

### F2 — Hosted CI đỏ sẵn từ trước W-0123 · `MEDIUM`

**Vấn đề.** Job `api_contract_diff` (`allow_failure: false`) chạy oasdiff **cumulative**
`draft.2 → current --fail-on WARN`. `OD-17` đã remove `sellable_status` ở `draft.20` — một breaking
change **đã được owner duyệt**. Nên job exit 1 tại HEAD, và cũng exit 1 với W-0123: mọi push đều
fail, và không work item nào sau này có thể làm nó xanh.

W-0123 ghi nhận trung thực là `PREEXISTING_GATE_FAILURE` nhưng không mở việc sửa. Một gate không
bao giờ pass được thì sẽ thôi được đọc, và breaking change **chưa** duyệt tiếp theo sẽ đến như một
màu đỏ giữa những màu đỏ.

**Khắc phục.** Xoay baseline `draft.2 → draft.20`, theo đúng quy trình đã ghi trong
`docs/contracts/openapi-codegen.md` và đúng tiền lệ `1.0.0 → draft.2` trước đó:

- thêm `specs/api/openapi/baselines/ivr-order-confirmation.v1.0.0-draft.20.yaml` — copy byte-exact
  contract tại HEAD, sha256 `e41b0fb9…` khớp đúng hash đã pin trước khi W-0123 đổi;
- đóng băng cửa sổ cũ thành
  `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.2-to-v1.0.0-draft.20.md`, **giữ nguyên**
  phần ghi nhận removal `sellable_status`;
- repoint `changelog-baseline.json` và `deploy/ci/docs.gitlab-ci.yml`;
- regenerate changelog hiện hành và portal.

Xoay baseline là cách một breaking change **đã duyệt** thôi che breaking change **chưa duyệt** kế
tiếp. Xoá báo cáo mà nó xoay qua mới là cách đánh mất chính sự phê duyệt đó — nên báo cáo được đóng
băng, không xoá.

### F3 — Guard chống tái phát quá hẹp · `LOW`

**Vấn đề.** `UT-M3-AUTHORITY-02` assert bằng reflection rằng `EligibilitySnapshot.Trust` và
`TrustResolverEvidence` không còn tồn tại. Nó chỉ bắt được kiểu tái lập dựng lại đúng type cũ. Kiểu
tái lập rẻ hơn nhiều — đọc thẳng `RiskFlagsJson` hoặc cột trust vào decision — không đụng type nào
và lọt lưới; chỉ integration test (cần Docker) mới bắt.

**Khắc phục.** Hai guard không phụ thuộc tên type:

- `UT-M3-AUTHORITY-10` pin **từ vựng decision**: `EligibilityDecisions` phải khai báo đúng năm hằng
  (bốn outcome + `PENDING_ELIGIBILITY`), và `Evaluate` chạy trên ma trận hoán vị mọi trục domain
  còn nhìn thấy được không bao giờ trả giá trị ngoài bốn outcome. Thêm hằng thứ sáu là đỏ, bất kể
  đặt tên gì. Test tự chứng minh không rỗng: ma trận phải chạm cả nhánh eligible lẫn nhánh chặn.
- `UT-M3-AUTHORITY-11` bound **decision path theo file**: `EligibilityRules.cs` và
  `EligibilityService.cs` không được chứa `TrustedSkipAllowed`, `CustomerTrustStatus`,
  `trusted_skip_allowed`, `customer_trust_status`, `risk_evidence_available` hay
  `TASK_SKIPPED_TRUSTED_CUSTOMER`. `RiskFlagsJson` không cấm thẳng được — `OD-18` giữ nó làm
  scheduler priority và `SchedulerEligibilityCapacityProvider` nằm cùng file — nên guard bound
  **chỗ** được đọc: mọi dòng nhắc `RiskFlagsJson` phải đồng thời nhắc `RiskScore`.

### F4 — Thay đổi ngoài phạm vi đi chung diff · `LOW`

**Vấn đề.** Diff W-0123 mang theo ba thứ không thuộc `OD-18`: dead code UI đã chết sẵn từ HEAD, một
fixture E2E `READY_503` tự mâu thuẫn, và refresh số đếm GitNexus ở `AGENTS.md`/`CLAUDE.md`. Cả ba
đều đã được disclose và đều vô hại, nhưng sẽ nằm chung một commit với cutover authority.

**Khắc phục.** Ghi ranh giới commit rõ ràng trong evidence, kèm lệnh tách cụ thể. **Không revert**:
cả ba đang làm gate xanh, revert là phá gate để làm đẹp ranh giới.

### F5 — Hai chỗ chữ nghĩa lệch · `NIT`

- `specs/api/06-error-codes.md` §1b còn dòng `ACCEPTED*`/`SKIPPED` = 200, đọc như mapping active
  trong khi đoạn ngay trên đã ghi `LEGACY_READ`. → viết lại cho khớp.
- `specs/workflows/07-trusted-skip.md` giữ tên cũ dù nội dung đã là M3 authoritative. → **không đổi
  tên**: `workflows/07` đang được error-codes, workflow index và evidence/tracker lịch sử trỏ tới;
  đổi tên sẽ làm hỏng chính những bản ghi audit không được phép viết lại. Ghi lý do ngay đầu file,
  cùng đúng logic đã dùng để giữ enum `TASK_SKIPPED_TRUSTED_CUSTOMER`.

### F6 — Gate cục bộ không tái lập được · `NIT`

- `deploy/ci/scripts/*.sh` không có `eol=lf`, nên checkout Windows biến chúng thành CRLF và `sh` từ
  chối ngay dòng đầu `set -eu\r`; W-0123 phải normalize trong container mới chạy được. → thêm
  attribute (`deploy/lab/asterisk/*.sh` đã có sẵn tiền lệ) và renormalize worktree.
- Plan W-0123 §5 ghi verify bằng `pnpm --dir admin-ui`, nhưng CI cài bằng `npm ci`
  (`ui-qa.gitlab-ci.yml`) và lockfile được commit là `package-lock.json`. Gọi `pnpm` sẽ bootstrap
  lockfile/workspace riêng, dừng ở `ERR_PNPM_IGNORED_BUILDS`, và để lại hai file lạ trong worktree
  — đúng cái bẫy W-0123 đã phải dọn tay. → đổi lệnh trong plan sang `npm --prefix admin-ui`, và
  gitignore hai file bootstrap để sai lầm này tự giới hạn.

## 3. Ranh giới

- Không đảo `OD-18`, không mở lại trusted-skip phía IVR.
- Không sửa migration, baseline hay evidence lịch sử. Baseline `draft.2` được **giữ nguyên**, chỉ
  thôi là cửa sổ so sánh hiện hành.
- Không đóng hộ external gate của W-0123: M3 sign-off, target DB preflight, hosted CI run thật.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không đổi.

## 4. Acceptance

| Gate | Bằng chứng bắt buộc |
| --- | --- |
| F1 | Counter + `IT-M3-AUTHORITY-12` + mutation làm test đỏ + panel dashboard + runbook anchor |
| F2 | Job `api_contract_diff` chạy trọn vẹn exit `0` trong container pinned theo digest |
| F3 | `UT-M3-AUTHORITY-10/11` + hai mutation làm test đỏ |
| F4 | Ranh giới commit ghi rõ trong evidence W-0123 và W-0124 |
| F5 | Diff tài liệu + docs gate xanh |
| F6 | Script chạy được thẳng từ worktree không cần normalize; plan ghi lệnh `npm` |
| Regression | .NET full, admin UI, 7 node gate, oasdiff, traceability, GitNexus detect-changes |
