# W-0293 — Trình bày evidence tương thích PII gate

Ngày 14/09/2026. **TESTS_PASS (local)**. Baseline `6bf954d2-ba8fd82e-b638b589-2c11df77-9c88eb42` (bỏ dấu nối).

Full scan `docs/evidence` exit 1: **121 dòng tại 45 file**. Đã đọc từng dòng: phần lớn là từ kỹ thuật bị pattern địa chỉ khớp; hai hồ sơ W-0242/W-0243 lặp marker, tên thực phẩm và fixture âm; W-0260 lặp fixture phone. Đây không phải bằng chứng phát hiện dữ liệu khách thật.

[Danh sách dòng đã rà](reviewed-lines.json) ghi file và số dòng tại baseline, không sao chép giá trị bị scanner chặn. Sửa prose theo nghĩa kỹ thuật; thay ví dụ bằng mô tả/fixture ID, dẫn bản lịch sử bất biến và mã test gốc. Tài liệu Phase-8 số 22 vẫn được định danh bằng số và thư mục, không đổi tên nguồn. Các số liệu, trạng thái, quyết định owner và hash chứng thực giữ nguyên.

Không sửa `pii-patterns.txt`, scanner, test nguồn, allowlist hoặc cấu hình job. Job privacy vẫn quét toàn bộ evidence và artifact tải từ các job phụ thuộc. Quét local chưa chứng minh artifact của pipeline mới đã sạch.

Kiểm chứng: [full evidence scan](pii-scan.log) exit 0; [PII selftest](pii-selftest.log) 8 marker PASS, gồm fixture âm/SQL/file không đuôi và lỗi thiếu target/không có text. Các dòng `PII_SCAN_FAIL` trong selftest là fixture bắt buộc bị từ chối. Gate-status được sinh lại từ tracker; diff/links và GitNexus staged kiểm trước commit. Không tìm thấy tham chiếu heading tới các heading vừa đổi. **Đính chính W-0295:** phạm vi tìm pin ban đầu chưa đủ; CI phát hiện active pins W-0181/W-0183 trên W-0146/W-0150. [W-0295](../W-0295/README.md) rà đủ theo 90 hash byte LF/CRLF của 45 file, đồng bộ pin sống và giữ attestation lịch sử.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).
