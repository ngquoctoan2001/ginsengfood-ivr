# W-0026 / P3-2 — Hồ sơ kết thúc phạm vi UI

Ngày: 21/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO.

**Quyết định owner: CANCELLED ngày21/09/2026, do deliverable UI đã bỏ khỏi IVR.**
Không đề nghị ACCEPTED cho sản phẩm UI hiện tại. Trạng thái work là CANCELLED; hồ sơ lịch sử được giữ.

## Căn cứ đã có

- Phần từng triển khai: Dashboard, call log, detail; [README/capture gốc](README.md) giữ nguyên lịch sử.
- Owner chuyển console về M3; [W-0253](../W-0253/README.md) được ghim tại
  commit `c0e660948b9ba5e330d9a60dfe08ae0610f137cc`. Commit này xoá source/tests/CI của admin-ui.
- [Prompt index](../../../prompt/00-index.md) ghi bốn prompt P3 chỉ còn để đối chiếu lịch sử.
- [acceptance-tests.json](acceptance-tests.json) ghim hash và trích đoạn quyết định; C2 trả lý do
  scope đã ngừng, không dùng test .NET xanh làm bằng chứng UI.
- Rà từng Residual: [W-0322](../W-0322/README.md); hồ sơ này ghi cách xử lý, không xoá câu cũ.

## Chuyển tiếp và người phụ trách

M3 chốt auto-refresh (build step bắt buộc cũ), CSV (tuỳ chọn), filter/detail và evidence boundary. Cost model local đã có nhưng metric chi phí chưa có đầu vào giá.

| Hành động | Phụ trách | Tình trạng |
| --- | --- | --- |
| Ghi quan hệ work cũ → quyết định bỏ UI và phạm vi chuyển tiếp | Codex | Đã lập hồ sơ này |
| Quyết định kết thúc W-0026 bằng CANCELLED hoặc lựa chọn terminal khác có lý do | Toàn, owner M8 | CANCELLED — Toàn duyệt “oke cả nhóm”, phiếu W-0337 tại8763dc1 |
| Chỉ định/xác nhận đầu mối M3 nhận yêu cầu | Toàn + owner M3 | Chưa có xác nhận người nhận trong repo |
| Nếu M3 triển khai: cung cấp commit, UI/E2E và nghiệm thu trong sản phẩm M3 | Đầu mối M3 được xác nhận | Chưa có bằng chứng; không là điều kiện phải dựng lại UI trong IVR |

Không gửi thông điệp ra ngoài ở lượt này. Không tái tạo admin-ui/CI UI, không mở gate production,
không suy việc hồ sơ đã hoàn thiện thành M3 đã nhận hay đã thực hiện.

## Ô quyết định dành cho Toàn

- Work: W-0026.
- Đề nghị: CANCELLED — scope đã retire bởi W-0253.
- Quyết định: CANCELLED theo phạm vi đã trình.
- Người duyệt/ngày/tham chiếu: Toàn,21/09/2026, phản hồi “oke cả nhóm” sau phiếu W-0337 tại8763dc1.

## Tiếp tục trình owner — W-0337, 21/09/2026

[Phiếu hiện hành](../W-0337/approval-request.md) tập hợp9 P2 và4 UI retired; kiểm lại pin và
bằng chứng tại aaba3d2. Phạm vi và hành động còn lại của hồ sơ này giữ nguyên. Quyết định PENDING;
chưa chuyển ACCEPTED/CANCELLED và không có thông điệp bàn giao M3 gửi trong lượt này.

## Ghi nhận sau quyết định

[Quyết định W-0337](../W-0337/approval-request.md#quyết-định-owner--21092026) chốt W-0026 bằng **CANCELLED**. Các câu PENDING
ở addendum trước là lịch sử. Phần chuyển tiếp vẫn có người/hành động như bảng; chưa có M3
xác nhận nhận hoặc hoàn tất, không có thông điệp gửi trong lượt này. REAL_CUSTOMER_CALL_ALLOWED=NO.
