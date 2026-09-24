# W-0205 — GĐ 3.1–3.2: đóng băng bằng chứng trên một SHA + chạy lại security wrapper

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Clone sạch và chạy toàn bộ bộ test offline trên đúng một SHA: lần đầu `9f13bea`, rồi đóng băng lại trên `98e94dc` với Unit 587/587, Contract 24/24, Chaos 8/8, Integration 266/266. Sau đó chạy `security-scan.sh`: gitleaks báo 55 phát hiện trên toàn lịch sử, từng dòng được mở ra kiểm, và cả 55 cùng 3 phát hiện của lượt hai đều là dương tính giả. Chúng được ghim theo commit/path/rule/dòng trong `.gitleaksignore` thay vì nới rule (`433a42d`, `1bebc42`). Cùng lượt re-pin năm validator offline bị drift hash nguồn, ghim `eol=lf` cho `integration-requirements/05-open-contract-questions.md` (`98e94dc`) và soạn phiếu `docs/release/gd3-acceptance-review-pack.md` cho owner. Kết quả freeze và scan (`SECURITY_SCAN_PASS`, gitleaks 194 commit `no leaks found`) là số đo tại thời điểm đó, sweep không chạy lại được.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0205; §9 completion record 'Work ID: W-0205'; activity A-0583..A-0587
- 433a42d (55 fingerprint gitleaks dương tính giả)
- 98e94dc (re-pin năm validator offline, ghim eol=lf trong .gitattributes)
- 1bebc42 (đóng băng lại trên 98e94dc; 3 fingerprint lượt hai; phiếu review)
- .gitleaksignore (khối 55 fingerprint mang tiêu đề 'W-0203 reviewed false positives…' và khối 'W-0205, second pass')
- .gitattributes (dòng chú thích W-0205)
- docs/release/gd3-acceptance-review-pack.md

## Phép kiểm C2

Trạng thái khai báo:

- Chưa khai phép kiểm. Kết quả freeze và quét gitleaks là số đo tại thời điểm đó; security-scan.sh không chạy trong sweep, và các pin lượt này ghim lại đã bị việc sau thay, trừ một. Chưa khai phép kiểm; Toàn quyết nhận theo hồ sơ hay chạy lại.

## Khai báo phép kiểm C2 — W-0353, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Việc này đóng băng bằng chứng trên một SHA và chạy lại lớp quét security. Con số của lần đó là số đo
tại thời điểm ấy; phép kiểm còn sống là chính `deploy/ci/scripts/security-scan.sh`, chạy trong lượt
mở rộng của collector từ W-0352. Từ W-0353 khai báo `gates` được xét qua lượt đó, nên [khai
báo](acceptance-tests.json) nay ghi `security-scan.sh`. Kết quả và phạm vi lịch sử ở trên giữ
nguyên.
