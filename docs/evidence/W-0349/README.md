# W-0349 — C2 xét được việc chỉ có gate hoặc chỉ có tài liệu

Ngày 23/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: ứng viên đã commit, lượt collector chạy ở bước tiếp theo.**

## Vì sao có việc này

Sau [W-0348](../W-0348/README.md), 132 việc chờ duyệt vẫn trượt C2. 101 việc trong số đó không khai test
hay phép kiểm nào, nên C2 không có gì để xét. Toàn yêu cầu làm tiếp phần này.

## C2 nhận thêm gì

`acceptance-tests.json` của một hồ sơ nay có thể khai:

- `gates`: gate trong full sweep có self-test kiểm chính thay đổi của việc đó. Hồ sơ phải nêu tên gate, gate
  phải có `argv` trong manifest, và chỉ được tính khi full sweep của đúng commit đạt.
- `scope: document`: việc không đổi code, test hay gate. Hồ sơ khai `deliverables` (mỗi tệp phải có tại
  commit) và `reason`.
- `scope: lab-run`: lượt chạy trên máy lab hoặc máy đích mà công cụ của nó không gate nào chạy. Khai như
  `document`.

Hai scope sau luôn ra `XEM`, kể cả khi Residual trống. Script chỉ xác nhận tệp có tại commit và full sweep
đạt; nội dung do Toàn đọc. Test mà hồ sơ trích vẫn phải xanh như trước.

Self-test của danh sách (`CT-ACCEPTANCE-C2-01`) có thêm 18 phép kiểm, tổng 76. Hai phép thử đột biến đều làm
self-test đỏ: bỏ luật `XEM` của tài liệu, và bỏ luật hồ sơ phải nêu tên gate.

## Khai báo cho 101 việc

Mỗi hồ sơ được đọc lại cùng lịch sử commit của nó, theo luật chặt: chỉ nhận test mà README hoặc commit của
chính việc đó gắn với nó, và không độn test gần giống. Mọi đề xuất được kiểm máy: TestId có trong traceability,
gate có `argv` trong sweep, tệp có trong git. Các ca chưa chắc được đọc lại trước khi ghi.

| Loại khai báo | Số việc |
| --- | ---: |
| Test .NET | 20 |
| Gate trong sweep | 34 |
| Test .NET và gate | 3 |
| Tài liệu | 33 |
| Chạy lab | 5 |
| UI đã ngừng, đề nghị `CANCELLED` | 1 |
| Chưa khai được | 5 |

- **32 test đã có nhưng chưa mang TestId** được gắn thêm TestId, không đổi tên hay thân test nào. Gồm 19
  test W-0016 viết cho P1-3, và các test mà W-0089, W-0090, W-0091, W-0094 dùng để chứng minh phát hiện của
  mình. Nhờ vậy khai báo của các việc đó phủ đủ mọi phát hiện, không chỉ phần đã có tên. Traceability tăng từ
  777 lên 809 dòng, và gate `generate-test-traceability.mjs` giữ nó khớp với cây.
- **23 việc chưa từng có gói bằng chứng** được dựng README từ nguồn sẵn có: tracker, commit, gói của việc
  khác. README chỉ tóm tắt và trỏ nguồn, không chạy lại gì.
- **74 README** có thêm mục "Khai báo phép kiểm C2 — W-0349". Bốn việc chỉ thêm tệp khai báo, vì README của
  chúng bị ghim hash: README của W-0128, W-0135, W-0141 bị năm validator external-decision ghim, còn README
  của W-0343 bị `verification.json` của chính nó ghim.
- Chi tiết từng việc và danh sách TestId mới nằm ở [declarations.json](declarations.json).

## Chưa khai được, và đề nghị đóng

- **W-0078**: bản ghim gói SSH.NET chỉ được kiểm gián tiếp, qua build coi cảnh báo là lỗi và job security_scan
  trên CI. Toàn quyết nhận theo sổ, hay thêm một phép kiểm trong sweep trước.
- **W-0094**: ba trong sáu thay đổi chưa có test nào; test duy nhất đã được gắn TestId nhưng chưa khai.
- **W-0168**: sửa `security-scan.sh`, script không chạy trong sweep, và không test nào phủ thay đổi.
- **W-0200**: mọi thay đổi đã bị rút khi merge, changelog API ghi `CANCELLED`. Đề nghị Toàn chuyển dòng
  này sang `CANCELLED`.
- **W-0205**: freeze và quét gitleaks là số đo tại thời điểm đó; các pin nó ghim lại đã bị việc sau thay,
  trừ một.
- **W-0097**: Admin UI đã bị gỡ khỏi repo. Khai báo đề nghị `CANCELLED` theo quyết định đã ghim, giống
  W-0027 và W-0028.

## Điều nên biết khi đọc

- Mục khai báo trong từng README có ghi chú khi README cũ nói khác hiện trạng, ví dụ W-0191, W-0203, W-0266,
  W-0273, W-0344.
- Bốn việc tài liệu có sản phẩm gốc đã bị việc sau xoá: W-0141, W-0226, W-0227, W-0281. Lý do trong khai
  báo ghi rõ phần còn lại.
- W-0192 là bản ghi phía main của phần W-0193 làm trên nhánh. Toàn có thể gộp hai dòng.
- Impact trước khi sửa: `collectTestEvidence`, `judge`, `render` đều LOW, 0 execution flow. Ở cấp lớp, hai
  lớp test `CallbackDeliveryTests` và `RetentionJobTests` báo HIGH, vì test khác dùng helper lồng của chúng.
  Các method được gắn TestId đều LOW, 0 caller, và thay đổi chỉ là thêm attribute.

## Kiểm chứng

- Self-test của danh sách: 76 phép kiểm C2. Self-test bằng chứng: 47 phép kiểm.
- Build hai project test sau khi gắn TestId: 0 lỗi.
- Chế độ worktree: 101/101 việc đạt C1. 95 việc có khai báo hợp lệ, chờ kết quả collector. 6 việc cố ý chưa
  đạt, như mục trên.
- PII scan, docs-selftest và gate-status đạt.

Chỉ Toàn chuyển các việc này sang `ACCEPTED`.
