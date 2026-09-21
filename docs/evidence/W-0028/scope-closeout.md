# W-0028 / P3-4 — Hồ sơ kết thúc phạm vi UI

Ngày: 21/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO.

**Đề nghị: CANCELLED do deliverable UI đã bỏ khỏi IVR; quyết định ghi trạng thái: PENDING_TOAN.**
Không đề nghị ACCEPTED cho sản phẩm UI hiện tại. Trạng thái hồ sơ là EVIDENCE_SUBMITTED.

## Căn cứ đã có

- Phần từng triển khai: Reporting UI; [README/capture gốc](README.md) giữ nguyên lịch sử.
- Owner chuyển console về M3; [W-0253](../W-0253/README.md) được ghim tại
  commit `c0e660948b9ba5e330d9a60dfe08ae0610f137cc`. Commit này xoá source/tests/CI của admin-ui.
- [Prompt index](../../../prompt/00-index.md) ghi bốn prompt P3 chỉ còn để đối chiếu lịch sử.
- [acceptance-tests.json](acceptance-tests.json) ghim hash và trích đoạn quyết định; C2 trả lý do
  scope đã ngừng, không dùng test .NET xanh làm bằng chứng UI.
- Rà từng Residual: [W-0322](../W-0322/README.md); hồ sơ này ghi cách xử lý, không xoá câu cũ.

## Chuyển tiếp và người phụ trách

BI pipeline/backend vẫn còn: M3 kiểm source/freshness/export/RBAC/a11y trên UI của họ. Bốn test BI không thay năm test UI đã bỏ; không ghép việc deploy ETL/đầu vào cost vào nghiệm thu UI lịch sử.

| Hành động | Phụ trách | Tình trạng |
| --- | --- | --- |
| Ghi quan hệ work cũ → quyết định bỏ UI và phạm vi chuyển tiếp | Codex | Đã lập hồ sơ này |
| Quyết định kết thúc W-0028 bằng CANCELLED hoặc lựa chọn terminal khác có lý do | Toàn, owner M8 | PENDING; chưa ghi chữ ký hộ |
| Chỉ định/xác nhận đầu mối M3 nhận yêu cầu | Toàn + owner M3 | Chưa có xác nhận người nhận trong repo |
| Nếu M3 triển khai: cung cấp commit, UI/E2E và nghiệm thu trong sản phẩm M3 | Đầu mối M3 được xác nhận | Chưa có bằng chứng; không là điều kiện phải dựng lại UI trong IVR |

Không gửi thông điệp ra ngoài ở lượt này. Không tái tạo admin-ui/CI UI, không mở gate production,
không suy việc hồ sơ đã hoàn thiện thành M3 đã nhận hay đã thực hiện.

## Ô quyết định dành cho Toàn

- Work: W-0028.
- Đề nghị: CANCELLED — scope đã retire bởi W-0253.
- Quyết định: PENDING.
- Người duyệt/ngày/tham chiếu: chưa có.
