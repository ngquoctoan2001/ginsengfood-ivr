# W-0079 — P0-2 CI semantic fail-closed remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Trước W-0079, self-test âm CT-CI-02/03 coi mọi exit khác 0 là PASS. W-0079 thay nó bằng deploy/ci/scripts/selftest-dotnet-policy.sh. Script này đòi thấy đúng test cố tình fail CtCi02DeliberatelyFails và đúng báo cáo coverage thấp, và đòi path gõ sai phải fail closed. Ivr.CiPolicy kiểm schema và severity của JSON NuGet vulnerability, dùng 6 fixture (clean, High, hai dạng rỗng, malformed, severity lạ); self-test in ra CT-CI-09. ci-config-selftest.mjs xác nhận job build_test_dotnet gọi self-test này. Bằng chứng trên GitLab hosted vẫn NOT_RUN dưới W-0061.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- deploy/ci/scripts/selftest-dotnet-policy.sh
- deploy/ci/tools/Ivr.CiPolicy/Program.cs
- deploy/ci/fixtures/vulnerabilities
- deploy/ci/scripts/ci-config-selftest.mjs
- deploy/ci/ci.gitlab-ci.yml
- prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `selftest-dotnet-policy.sh`, `ci-config-selftest.mjs`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0079 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
