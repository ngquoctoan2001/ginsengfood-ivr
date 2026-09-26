# W-0370 — Phần II của kế hoạch khắc phục `25/09`: `Q-12.1` (tên hàng intake đã nhận thì phải gọi được)

Ngày 26/09/2026 · Claude, lô code độc lập Toàn giao sau câu *"chuyển qua phần 2 hả"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Intake kiểm tên hàng bằng guard sản phẩm (`W-0243`). Guard này bỏ bốn chữ nhập nhằng mà trong tên thực phẩm là nghĩa
thường, như món yến sào hay chất tạo ngọt. Lúc quay, bộ dựng lời thoại và bước kiểm trước khi đọc lại chạy guard toàn văn
lên cả câu. Vì thế một đơn như vậy được nhận, Module 3 được báo nhận, rồi mỗi lần quay đều bị từ chối lúc dựng lời thoại:
hai lần technical exception, rồi `WINDOW_EXPIRED`. Toàn chốt `Q-12` theo khuyến nghị ngày 26/09 (PA1): đoạn tên hàng
dùng guard sản phẩm, phần còn lại dùng guard toàn văn.

Khi viết test xuyên intake → quay mà kế hoạch đòi, lộ thêm một chỗ hỏng sớm hơn. Bộ kiểm lưu trữ
(`PersistenceInvariantValidator`) đọc bản tóm tắt đơn đã lưu bằng guard toàn văn, dưới dạng chuỗi. Lúc intake, chuỗi đó là
đầu ra của serializer, mọi chữ có dấu đã escape, nên guard không thấy gì. Postgres (jsonb) trả lại chữ thật, nên mọi lần
lưu lại chính task đó bị từ chối, bắt đầu từ lần lưu của eligibility (500). Một đơn như vậy chưa bao giờ tới được bước
quay.

Phân tích tác động trước khi sửa: `SpeechPrivacyGuard.EnsureSafe` ở mức HIGH, bộ dựng lời thoại MEDIUM,
`PersistenceInvariantValidator.ValidateTask` CRITICAL (mọi lần lưu task đều qua nó). Toàn duyệt cả ba trước khi sửa. Lô
làm trên bản sao tách riêng của `59a76487`.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| Kiểm lời thoại theo đoạn | `SpokenTextGuard` (Domain): đoạn `items_spoken` dùng guard sản phẩm; guard toàn văn đọc phần còn lại, với đoạn tên hàng thay bằng một chữ giữ chỗ; guard sản phẩm vẫn đọc cả câu. Vì vậy số điện thoại, dial token và bốn dấu chỉ có nghĩa địa chỉ vẫn bị chặn ở mọi chỗ, kể cả khi vắt qua ranh giới đoạn. Kịch bản không tách đoạn là một đoạn không có placeholder, nên vẫn dùng guard toàn văn cho tất cả | `UT-PII-PRODUCT-04`, `UT-PII-PRODUCT-05` |
| Hai chỗ kiểm lúc quay | Bộ dựng lời thoại và `SpeechPrivacyGuard` gọi chung hàm trên; bước kiểm trước khi đọc dùng các đoạn mà nó được giao. Ba tên hàng mẫu của `UT-PII-PRODUCT-01` nay dựng được lời thoại và qua bước kiểm cuối | `UT-PII-PRODUCT-06` |
| Ca cũ của `UT-RENDER-DATA-04` | Test này giữ nguyên mục đích: lời thoại bị guard từ chối thì là lỗi dựng lời thoại, không phải lỗi token. Trước đây nó dùng một tên hàng mà `Q-12` nay cho qua. Đầu vào mới là một số lượng có quá nhiều chữ số lẻ, bộ đọc số trả về dạng chữ số, và dãy chữ số đó trông như một số điện thoại | `UT-RENDER-DATA-04` |
| Bộ kiểm lưu trữ | Guard sản phẩm đọc cả chuỗi như đang lưu. Sau đó từng tên trường và giá trị được đọc sau khi giải mã: tên hàng và đơn vị dùng guard sản phẩm, mọi trường khác dùng guard toàn văn. Như vậy còn chặt hơn trước: một dấu địa chỉ ở trường không phải tên hàng mà bị escape nay cũng bị chặn | `IT-DB-TASK-SUMMARY-12` |
| Test xuyên suốt | Một đơn ba tên hàng mẫu, chạy thật trên Postgres: intake, eligibility qua internal API, quay MOCK với kịch bản đã duyệt, bộ dựng lời thoại thật và dịch vụ tổng hợp thật, rồi chuẩn hoá. Khách nghe đủ ba tên, kết quả `IVR_CONFIRMED`, callback nằm trong outbox | `IT-TEL-PRODUCT-NAME-12` |

## Quyết định Toàn ghi ngày 26/09 cùng việc này

- **Tên khách giữ guard toàn văn.** Tên không dấu trùng một trong các chữ địa chỉ vẫn bị intake trả 422; tên có dấu không
  bị ảnh hưởng. Đây là quyết định còn mở, chờ Privacy xem xét.
- **Tên hàng viết không dấu vẫn bị intake từ chối**, như `K-27` (`UT-INTAKE-PII-23` giữ nguyên). Intake chặn ngay tại
  cửa, nên không có ca "nhận rồi hỏng", và Module 3 không thấy gì đổi.

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của lô trong bản sao, build lại project chứa test được nêu, chạy test, rồi ghi lại byte gốc
(không dùng `git checkout`). Mười một phép đều đỏ đúng chỗ, hai lượt đối chứng đều xanh:

- lời thoại: bộ dựng lời thoại, hoặc bước kiểm trước khi đọc, quay lại guard toàn văn trên cả câu; đoạn tên hàng không
  được thay bằng chữ giữ chỗ; cả câu không còn được đọc bằng guard sản phẩm; phần ngoài tên hàng thoát guard toàn văn;
  mọi placeholder bị coi là tên hàng;
- lưu trữ: bản tóm tắt lại bị đọc như chuỗi bằng guard toàn văn; cả chuỗi không còn được đọc bằng guard sản phẩm; giá
  trị không được đọc sau khi giải mã; tên hàng bị giữ ở guard toàn văn; tên hàng thoát guard hoàn toàn.

Để phép "cả chuỗi không còn được đọc bằng guard sản phẩm" có ca riêng, `IT-DB-TASK-SUMMARY-12` có thêm một con số trông
như số điện thoại nằm trong trường số, chỗ mà việc đọc từng trường không nhìn tới. Số liệu ở
[mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build cả solution trong bản sao (`59a76487` cộng lô này), analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit, bản sao | `932/932` |
| Bộ integration, bản sao | `456/457`, 22 phút 55 giây vì máy đang chạy cùng lúc build và test của dự án khác và một job CI. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git; trên `main` nó được chạy lại |
| Traceability, `generate-test-traceability.mjs` | bản sao `959` dòng; trên `main` sinh lại sau khi đưa vào |
| Gate sweep, bản sao | `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest |
| Quét gitleaks mô phỏng (luật generic-api-key, entropy từ 3,5) trên các dòng lô thêm | 0 dòng đáng ngờ; lô không thêm migration hay khoá thử nào |
| Trên `main` sau khi đưa vào (`dd90dd15` cộng lô này, tức sau `W-0369`), 14:12–14:29 | build 0 cảnh báo; unit `936/936`; integration `459/459` ngay lượt đầu, 12 phút 10 giây, gồm `IT-API-MATRIX-38`, `IT-TEL-PRODUCT-NAME-12` (lối eligibility mà `W-0369` vừa sửa) và `IT-DB-TASK-SUMMARY-12`, có ghi `trx`; `generate-test-traceability.mjs` sinh lại `965` dòng; `gate-status.mjs` viết lại; `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest, gồm quét PII hồ sơ |

## Còn lại

- `Q-16` (render thử tại intake) làm tiếp theo, trên nền lô này.
- Tên khách: chờ Privacy (xem trên).
