# W-0295 — Đồng bộ pin sống sau sửa tài liệu

Ngày 14/09/2026. **TESTS_PASS (local)**. Baseline `c089a96a-f73c18aa-6cef68c5-6c0540be-517b3807` (bỏ dấu nối).

[Hosted sweep 16478174676](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478174676), pipeline 2846002747: **37/39 PASS**, hai lỗi pin trên W-0146 và W-0150 do sửa prose tại W-0293. Các gate khác, gồm policy selftest chứa regression JUnit mới, đã PASS. Nhận định ban đầu W-0293 rằng không tìm thấy pin đã thiếu phạm vi; sửa tại đây và Activity A-0619.

[Rà 9 tham chiếu](pin-reference-review.json) theo 90 SHA-256 LF/CRLF của 45 file tại baseline 6bf954d: chỉ có hai pin active trong validator W-0181/W-0183. Các tham chiếu khác là attested manifest và bảng lịch sử W-0154/W-0181/m8-14; giữ nguyên. Cập nhật hai constant pin sống, không bỏ kiểm hash. Hai selftest nay kiểm cả template trên đĩa và template sinh trong bộ kiểm.

[Cập nhật template](template-update.log) xác nhận chỉ `source` đổi, mọi quyết định/sign-off/safety giữ nguyên: W-0181 hai hash, W-0183 năm hash. Hai hash thuộc sửa prose; năm hash còn lại đã cũ từ trước so với validator. Trạng thái vẫn PENDING_M3_SIGNOFF / CONTACT_DIAL_TOKEN_DECISIONS_NOT_RECEIVED. Không sửa `attested-sha256.txt` hoặc ghi chữ ký mới.

[Full sweep local sau sửa pin](gate-sweep-local.log): **39/39 PASS, 22 skip theo manifest**, exit 0. Kiểm PII tiếp theo phát hiện chữ số trong hash source resolver-port trùng pattern phone. Export template nay dùng Unicode escape để chia cụm số dài trong SHA-256 đã kiểm với source; [JSON roundtrip](template-encoding.log) xác nhận giá trị sau đọc không đổi. Không xử lý bundle bên ngoài hoặc thay scanner. Sau sửa export đã chạy lại W-0183, CI-config và full evidence PII: 380 file PASS.

[W-0183 selftest cuối](dial-token-selftest.log): 3 template (trên đĩa, sinh trong bộ kiểm, JSON roundtrip), 4 model, 64 refusal. [W-0181 selftest](upstream-selftest.log): 2 template, 1 valid, 32 refusal. [Hai template cũ](stale-template-refusal.log) đều exit 1 đúng lỗi source pin. Git diff của hai attested manifest rỗng. GitNexus LOW: mỗi constant 0 caller index; mỗi runSelfTest có 1 caller main, main có 1 file caller, 0 process runtime. Hosted cần candidate sau commit; không gọi pipeline c089a96 là PASS.
