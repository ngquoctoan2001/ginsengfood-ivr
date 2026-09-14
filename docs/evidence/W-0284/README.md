# W-0284 — Đối soát bàn giao và sổ tiến độ

Ngày 2026-09-14. Baseline `main@a0790e3`. Phạm vi tài liệu và metadata nguồn.

- IR-06/07 cùng chỉ tới contract draft.27; IR-08 ghi đúng OD-V1-07 đã ký nhưng deployment JWT còn thiếu.
- Mặc định trong IR-07 chỉ có hiệu lực trong phiếu M3 trả lại và ký. Chưa nhận phản hồi vẫn pending. Không điền hộ chữ ký hay gửi lại phiếu.
- [13 dòng phục hồi](recovered-work-items.json) giữ baseline, hash README và trạng thái gốc: 11 TESTS_PASS, W-0271 ACCEPTED từ quyết định owner đã ghi, W-0273 BLOCKED_EXTERNAL. Các kết quả kiểm thử trong hồ sơ đó là lịch sử, không phải rerun ngày 14/09.
- Báo cáo tuần giữ số liệu kỳ 12/09 và đúng 100 dòng, thêm đính chính ngày 14/09: historical hosted CI có thật; 5 kiểm tra image không phải full E2E; pilot khách thật phải sau go/no-go và cấp quyền; worker tự xét kỹ thuật đã có W-0283.
- Không tạo hàng loạt README để lấp số 56: planned prompt có yêu cầu pack, unplanned có thể có evidence trong tracker. Xem inventory được kiểm trên đĩa.

Follow-up W-0286: `evidence_sha256` chia nhóm 8 ký tự, bỏ dấu `-` để khôi phục hash chuẩn. Ba tiêu đề kỹ thuật trong inventory được thay bằng tham chiếu tracker tại baseline vì scanner nhầm thuật ngữ thành địa chỉ; tiêu đề gốc vẫn nguyên trong tracker. Không sửa rule quét PII hay thay trạng thái lịch sử.

## OpenAPI kiểm chứng thật

Chạy container `tufin/oasdiff:v1.26.1@sha256:aae8cfcf7d18d3b0ebce6bdf407623bf8788ca318c7a0440627aaf583ed3e9f4` và script `generate-oasdiff-changelog.sh` có sẵn. Sinh hai changelog lưu trữ [25→26](../../api/changelog/ivr-order-confirmation.v1.0.0-draft.25-to-v1.0.0-draft.26.md), [26→27](../../api/changelog/ivr-order-confirmation.v1.0.0-draft.26-to-v1.0.0-draft.27.md), exit 0.

`oasdiff breaking` 25→27 `--format markdown --fail-on WARN`: **exit 1, 10 WARN** về enum response của `/sim-channels` (6) và `/dashboard` (4). Đây là cảnh báo tương thích phải công bố, không phải kết quả PASS bị bỏ qua. Baseline CI draft.27 giữ nguyên theo quyết định owner ở W-0279; không thay đổi OpenAPI hay runtime.

## Kiểm tra và giới hạn

Contract-freeze PASS; docs-selftest PASS 17 trang; bốn validator handover PASS sau khi re-pin nguồn và mẫu chưa ký; gate-status PASS (272 work, 44 ACCEPTED, 19 BLOCKED_EXTERNAL, 11 gate, 2 quyết định mở, nấc 0). GitNexus impact bốn SOURCE_PINS: LOW, 0 caller/0 process. Các hash đổi chỉ chứng minh tài liệu nguồn, không mang nghĩa duyệt external evidence.

Nhãn queue pause vẫn chờ quyết định owner; chưa sửa semantics. M3 thật, JWT deployed, SIM, staging và production chưa được chứng minh bởi task này. `REAL_CUSTOMER_CALL_ALLOWED=NO`.
