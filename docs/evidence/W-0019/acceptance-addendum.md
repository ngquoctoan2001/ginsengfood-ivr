# W-0019 — Đối chiếu test lịch sử, W-0324

Ngày: 21/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO · trạng thái hồ sơ: EVIDENCE_SUBMITTED.

| Test lịch sử | Quyết định và commit bỏ test | Kiểm chứng thay thế hiện hành |
| --- | --- | --- |
| `UT-ELIG-BLOCK-01` | OD-17 bỏ sellable predicate riêng của IVR; M3 giữ quyết định nghiệp vụ, IVR kiểm snapshot và chặn nguồn thiếu/stale. Commit `55ea48bbddfa4a0856b47ab0c68da1221c0ba2d2` | `UT-ELIG-FAILCLOSED-04`, `UT-ELIG-EVIDENCE-10`, `UT-ELIG-EVIDENCE-11`, `UT-ELIG-EVIDENCE-12` |
| `UT-ELIG-TRUST-03` | OD-18 bỏ nhánh IVR tự chọn trust-skip; M3 quyết định gọi, trust/risk metadata không được đổi quyết định đó. Commit `6760ba614e47e1631384640209cc4acee944a8cf` | `UT-M3-AUTHORITY-01`, `UT-M3-AUTHORITY-02`, `UT-M3-AUTHORITY-10`, `UT-M3-AUTHORITY-11`, `IT-M3-AUTHORITY-05`, `IT-M3-AUTHORITY-06`, `IT-M3-AUTHORITY-07` |

[OD-17](../../../plan/ivr-orther/decisions-log.md) và [OD-18/W-0123](../W-0123/README.md)
đã có từ trước lượt này. SHA đầy đủ, hash văn bản và trích đoạn được lưu ở
[acceptance-tests.json](acceptance-tests.json). C2 kiểm commit nằm trong lịch sử của candidate,
văn bản/hash/trích đoạn khớp, ID cũ không còn active, mọi ID thay thế tồn tại và chạy Passed.
Một test thay thế thiếu/đỏ, pin sai hoặc nguồn quyết định không đọc được đều làm C2 từ chối.

Đây là thay đổi yêu cầu được duyệt, không phải đổi tên test một-một: sellable/trust predicate đã
ngừng thuộc IVR. Các kiểm snapshot/authority thay thế giữ ranh giới hiện hành.
Sáu ID còn sống trong README gốc tiếp tục bắt buộc; shell CT-CI-06 được kiểm qua full sweep
của selftest-pii.sh, không giả thành kết quả .NET.

## Phần còn lại

Capacity scheduler đã có ở W-0020. Không gọi Ops/CRM trực tiếp vẫn đúng. Tích hợp producer M3,
credential, SIM thật và shared E2E vẫn ngoài scope MOCK. Không đề nghị phục hồi trust-skip.
Codex phụ trách addendum/kiểm C2; Toàn xét nghiệm thu; đầu mối M3 chưa xác nhận nhận bàn giao.

Bằng chứng chạy mới và quyết định trình duyệt ở [W-0324](../W-0324/README.md).
Chưa có chữ ký ACCEPTED; W-0019 là hồ sơ bổ sung riêng, không ghép vào tám việc owner yêu cầu trình.

## Tiếp tục trình owner — W-0337, 21/09/2026

[Phiếu hiện hành](../W-0337/approval-request.md) tập hợp9 P2 và4 UI retired; kiểm lại pin và
bằng chứng tại aaba3d2. Phạm vi và hành động còn lại của hồ sơ này giữ nguyên. Quyết định PENDING;
chưa chuyển ACCEPTED/CANCELLED và không có thông điệp bàn giao M3 gửi trong lượt này.
