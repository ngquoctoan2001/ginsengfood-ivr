# W-0324 — Sửa C2, bổ sung năm hồ sơ và trình tám việc P2

Ngày: 21/09/2026 · Codex · REAL_CUSTOMER_CALL_ALLOWED=NO.

Theo yêu cầu owner: sửa phát hiện C2 trong [W-0322](../W-0322/README.md), hoàn thiện hồ sơ
W-0019/W-0025/W-0026/W-0027/W-0028 và chuẩn bị [phiếu duyệt tám việc P2](approval-request.md).
Không tự chuyển ACCEPTED/CANCELLED. Baseline trước sửa: `a7f7ad3c572fb71de46b90074e557abb6d994064`.

## Thay đổi công cụ

- Nhận diện TestId của cả nhóm đã xoá; không bỏ qua ID lồng trong nhóm eligibility.
- Đọc README, các Markdown đính kèm trong cùng gói và TestId bắt buộc trong prompt.
  Thiếu file, không có TestId hoặc range không hợp lệ đều bị từ chối.
- Đối chiếu mọi định nghĩa trùng TestId và class/method trong source tại commit đang xét;
  kết quả của method trùng tên nhưng thuộc class khác không được dùng thay.
- File `acceptance-tests.json` khai báo test bắt buộc, test lịch sử đã thay thế hoặc scope UI
  đã ngừng. Pin quyết định gồm commit đầy đủ, path, SHA-256 và trích đoạn khớp văn bản;
  commit phải là ancestor của candidate. Test thay thế phải hiện hữu và Passed.
- Pin kiểm được nguồn quyết định, không tạo chữ ký owner. Người duyệt vẫn đọc nội dung và
  quyết định phạm vi; một văn bản bất kỳ không tự trở thành quyết định có thẩm quyền.
- Bốn UI đã retire được báo rõ cần owner quyết định kết thúc, không đạt C2 cho phần mềm UI.
- Test shell có bảng ánh xạ cố định trong `acceptance-test-plan.mjs`; chỉ đạt khi gate tương ứng
  có trong manifest và full sweep cùng commit được xác minh. Hồ sơ không tự gán ID mất cho gate xanh.
- Giữ toàn bộ kiểm SHA/tree/hash, cây sạch, đủ solution/TRX và đủ gate của W-0319.

## Kiểm thử hồi quy

| TestId của gate | Lệnh bắt buộc | Kết quả trước commit |
| --- | --- | --- |
| CT-ACCEPTANCE-C2-01 | `node deploy/ci/scripts/acceptance-batches.mjs --self-test` | PASS, 25 trường hợp C2 mới |
| CT-ACCEPTANCE-EVIDENCE-01 | `node deploy/ci/scripts/acceptance-evidence-selftest.mjs` | PASS, 47 kiểm tra provenance/CLI |

Đã chạy trường hợp tiền tố bị xoá trên code cũ và thấy đỏ trước sửa. Các kiểm tra mới gồm
prompt UI nhưng chỉ có backend, README chỉ liên kết test report, test rỗng, trùng TestId,
class sai, file mất, vòng liên kết, retirement thiếu quyết định/test thay thế và scope UI đã ngừng.
CLI dùng Git fixture thật kiểm pin hợp lệ, hash sai, trích đoạn sai, commit không tồn tại và path không an toàn.
Các fixture được ghi rõ là tổng hợp; không dùng làm bằng chứng nghiệm thu IVR.

Hai helper mới được manifest phân loại thư viện; assertions chạy trong hai gate bắt buộc bên trên.
Không thêm gate rỗng hay bỏ qua một phép kiểm mới.

## Năm hồ sơ đã bổ sung

| Việc | Hồ sơ | Việc còn thuộc người khác |
| --- | --- | --- |
| W-0019 | [Addendum](../W-0019/acceptance-addendum.md), pin quyết định và 11 test thay thế cho hai ID lịch sử | Toàn xét riêng sau kết quả mới; producer/auth/SIM thật thuộc gói tích hợp |
| W-0025 | [Closeout foundation/identity](../W-0025/scope-closeout.md) | Toàn quyết định CANCELLED; chỉ định đầu mối M3 nếu bàn giao |
| W-0026 | [Closeout dashboard](../W-0026/scope-closeout.md) | Toàn quyết định CANCELLED; M3 chốt auto-refresh/CSV; owner cấp đầu vào cost ở work riêng |
| W-0027 | [Closeout config/roles](../W-0027/scope-closeout.md) | Toàn quyết định CANCELLED; M3 chốt config/permission/E2E; không suy replay đã làm |
| W-0028 | [Closeout reporting](../W-0028/scope-closeout.md) | Toàn quyết định CANCELLED; M3 cung cấp UI/RBAC/a11y nếu triển khai; BI/ETL vẫn riêng |

README gốc chỉ thêm mục liên kết có ngày; không sửa lịch sử chạy hay chữ ký. W-0023 được bổ sung
[ma trận 22 TestId callback](../W-0023/acceptance-test-matrix.md) vì README trước đó không ghi ID.
W-0066 được xét đủ chín ID trong test-report đã có. W-0019 là hồ sơ bổ sung riêng, không đổi danh sách tám việc.

## Kiểm chứng tại commit cố định

PENDING — sau commit implementation, chạy collector trên checkout detached sạch: toàn solution,
sau đó full gate sweep, rồi sinh danh sách nghiệm thu từ bundle đó. Kết quả trước commit không
được gắn nhãn là kết quả của candidate. WIP VieNeu/lab của các work khác được giữ riêng.

## Phạm vi ảnh hưởng

GitNexus impact trước sửa: LOW, caller nằm trong công cụ nghiệm thu; không có runtime flow bị ảnh hưởng.
Kiểm staged: LOW, 27 file, 50 symbol, 0 process. Đã kiểm 62 liên kết nội bộ và mirror 309 work
của snapshot chỉ chứa thay đổi này; cây chung có 312 vì giữ WIP của các work khác.
Không thay API, nghiệp vụ gọi điện, migration hay UI. Hosted CI, M3 shared E2E, SIM/lab/production
không được chứng nhận bởi gói local này. Chỉ Toàn có quyền quyết định nghiệm thu theo tracker §1.
