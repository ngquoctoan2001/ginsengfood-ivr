# W-0366 — Phần II của kế hoạch khắc phục `25/09`: `Q-28.1–2` (nhánh production của `DispatchGate`)

Ngày 26/09/2026 · Claude, lô Toàn chọn sau câu *"chuyển qua phần 2 hả"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

`Q-28` (Toàn chốt PA2 ngày 25/09 theo đề xuất M8): vault production dựng đích quay `sip:SỐ@HOST`, gateway đưa
nguyên đích đó vào `DispatchGate`, và dòng đầu của gate chạy `PiiGuard` nên ném lỗi; nếu lọt, gate còn đòi allowlist
lab, mà allowlist này không thể chứa số. Nghĩa là mở hết các khoá khác thì production vẫn không quay được ai, và nhánh
production chưa có test nào. PA2: nhánh production bỏ allowlist lab, gate nhận tham chiếu không chứa số, và có danh
sách pilot lưu vân tay HMAC của số, hai người duyệt; một bản duyệt có chữ ký chuyển pilot sang mở.

Ngày 26/09 Toàn chọn cách làm: danh sách pilot nằm trong cấu hình, một bản duyệt bốn mắt trong bảng duyệt hiện có
ghim đúng danh sách đó (không thêm API, Module 3 không phải làm gì). Lô làm trên bản sao tách riêng; `W-0364` (cùng
lô Toàn chọn) đã vào trước, và lô `K-54..K-56` của phiên khác (`W-0365`) vào giữa chừng nên thay đổi được gộp ba chiều
lên đó.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `Q-28.1` cổng | `DispatchGate` tách nhánh theo chế độ: MOCK từ chối, LAB giữ allowlist lab như cũ, production hỏi ba điều theo thứ tự: bản duyệt `PRODUCTION_CALL` của môi trường, môi trường đã được mở chưa (`PRODUCTION_CALL_OPEN`), nếu chưa thì đích có trong danh sách pilot đã duyệt không. Chế độ khác bị từ chối. Việc được gọi khách thật hay không đã do guardrail quyết trước (production bắt buộc `RealCustomerCallAllowed`), nên không kiểm lại | `UT-TRUNK-GATE-02`, `UT-TRUNK-GATE-03`, `UT-TRUNK-GATE-04` |
| `Q-28.1` danh sách pilot | Mỗi mục là vân tay HMAC-SHA256 của số quốc gia (cùng một số viết cách nào cũng ra một vân tay), viết bằng chữ `a`–`p` để `PiiGuard` không bao giờ đọc nhầm thành số điện thoại. Khoá là bí mật triển khai `Ivr:Telephony:SipTrunk:PilotFingerprintKey`; danh sách là `PilotDestinations`; validator từ chối trunk bật mà thiếu khoá, hoặc có mục không phải vân tay. `PostgresProductionPilotGate` chỉ nhận danh sách khi có bản duyệt `PRODUCTION_PILOT_LIST` còn hiệu lực cho đúng môi trường và đúng mã băm danh sách, do hai người khác nhau đề xuất và duyệt | `UT-TRUNK-OPT-09`, `UT-TRUNK-DI-07`, `IT-FLAG-PRODGATE-15` |
| `Q-28.1` mở production | Loại duyệt `PRODUCTION_CALL_OPEN`, theo môi trường; thu hồi thì môi trường quay về danh sách pilot | `IT-FLAG-PRODGATE-16` |
| `Q-28.1` migration | `20260926032500_ProductionPilotApprovalKinds` mở rộng CHECK loại duyệt; `PRODUCTION_PILOT_LIST` bắt buộc có người đề xuất, mã băm và môi trường (cùng CHECK bốn mắt có sẵn thì người đề xuất khác người duyệt); `PRODUCTION_CALL_OPEN` bắt buộc môi trường. Không seed dòng nào | `IT-FLAG-PRODGATE-15`, `IT-FLAG-PRODGATE-16` |
| `Q-28.1` thử lại thủ công | `RequireRetryAllowedAsync` chỉ hỏi allowlist lab khi ở `LAB_REAL_SIM`; trước đó nó chặn mọi lần thử lại ở MOCK khi `phone_ref` không có trong allowlist, và mọi lần ở production. `IT-API-RETRY-06` trước đây ghim đúng lỗi này ở dòng thứ hai; nay nó chỉ còn kiểm kill switch | `IT-API-RETRY-06`, `IT-API-RETRY-07` |
| `Q-28.2` | `DialAuthorization` có thêm `GateReference`: ở lab là alias, ở production là vân tay vault tính từ số. Gateway đưa `GateReference` cho cả hai lần hỏi cổng, nên số chỉ được đọc ở bước quay. Test integration đi từ vault (sổ resolve Postgres, audit thật) qua hai cổng production có bản duyệt, rồi quét mọi ô chữ của mọi bảng | `UT-TRUNK-GATE-01`, `UT-AST-GATE-04`, `IT-PHONE-CONTAIN-03` |
| Công cụ vận hành | `tools/ops/production-pilot-list.mjs` tính vân tay cho danh sách số (đọc khoá từ file, số từ stdin, không in lại số) và mã băm danh sách cho bản duyệt; `--hash` để người duyệt thứ hai tính lại từ cấu hình đã triển khai. Gate mới `production-pilot-selftest.mjs` giữ công cụ cùng đáp án với bản C# (cùng vector với `UT-TRUNK-GATE-01` và `IT-FLAG-PRODGATE-15`), đăng ký trong `gate-invocations.json` | `production-pilot-selftest.mjs` |
| Tài liệu | `docs/operations/production-dial-path.md`: dòng "`DispatchGate` từ chối mọi đích production" ghi đã sửa, bảng cấu hình trunk thêm hai khoá, mục mới về danh sách pilot và việc mở production (thứ tự gate hỏi, cách dựng danh sách, SQL của bản duyệt, đổi danh sách, mở và thu hồi). `ProviderPorts.cs` bị ghim hash ở `dial-token-production-bundle-validator.mjs` và template W-0183; ghim lại trong lô | `dial-token-production-bundle-validator.mjs`, `docs-selftest.mjs` |

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của lô trong bản sao, build lại project chứa test được nêu, chạy test, rồi ghi lại byte gốc
(không dùng `git checkout`). Mười hai phép đều đỏ đúng chỗ, ba lượt đối chứng đều xanh:

- cổng: production được phép ngay khi release được duyệt, bỏ qua danh sách pilot; allowlist lab được hỏi trước chế độ
  như thứ tự cũ; không bao giờ hỏi môi trường đã mở chưa;
- số: gateway đưa đích có số cho cổng; vault lấy đích SIP làm tham chiếu cho cổng; vân tay viết bằng hex, có chữ số;
- cấu hình: trunk bật mà khởi động được khi thiếu khoá; nhánh production không đăng ký cổng pilot;
- bản duyệt: cổng pilot không ghim mã băm danh sách; DB nhận bản duyệt danh sách không có người đề xuất;
- thử lại thủ công lại hỏi allowlist lab ở mọi chế độ;
- công cụ Node lệch một chữ so với bản C#: `production-pilot-selftest.mjs` đỏ. Lần chạy đầu, thông báo lỗi của gate
  trích nguyên một cách viết số thử, và lượt quét PII của hồ sơ từ chối nó; gate nay chỉ nêu thứ tự ca, và phép này được
  chạy lại để ghi đúng đầu ra mới.

Các phép chạy trước khi lô được gộp lên `W-0365`; bảng dưới kiểm lại sau khi gộp. Số liệu ở
[mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build cả solution trong bản sao đã gộp lên `892c8ed0` (`W-0365`), analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit, bản sao đã gộp | `919/919` |
| Bộ integration, bản sao đã gộp | `446/447`, 7 phút 8 giây. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git; trên `main` nó được chạy lại (dòng cuối bảng). Trước khi gộp, bản sao cũ cho `436/437` với cùng một test đỏ |
| Traceability, `generate-test-traceability.mjs --check` | sinh lại sau khi gộp, `947` dòng |
| Gate sweep, bản sao đã gộp | `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest; gồm `production-pilot-selftest.mjs` mới, `dial-token-production-bundle-validator.mjs` (ghim lại `ProviderPorts.cs`) và `docs-selftest.mjs` |
| Test Python của lab | `58` đạt, không đổi |
| Trên `main` sau khi đưa vào (`701148f4` cộng lô này), 11:08–11:13 | build 0 cảnh báo; unit `919/919`; `IT-API-MATRIX-38` cùng các test integration về bản duyệt runtime gate, chặn số điện thoại, admin nội bộ, feature flag, migration, tương thích schema và compliance `75/75`; `production-pilot-selftest.mjs`, `generate-test-traceability.mjs --check`, `dial-token-production-bundle-validator.mjs`, `docs-selftest.mjs`, `gate-status.mjs`, `acceptance-batches.mjs --self-test`, `ci-config-selftest.mjs` đạt. Quét PII: trên bản sao sạch `PII_SCAN_PASS` (776 file); trên `main` nó còn báo nhật ký soak cục bộ trong `ci-artifacts/`, thư mục git bỏ qua, không vào commit |

## Còn lại

- `Q-28.3` (dựng lời thoại trước khi resolve số) làm cùng SIP-04, như kế hoạch ghi.
- Chưa ai được báo phần cần người khác làm (kế hoạch §5): Platform cấp và giữ khoá vân tay, Sếp ký chuyển pilot sang
  mở. Gateway Asterisk vẫn đóng với `PRODUCTION_REAL` tới SIP-04 (`CB-13`); lô này không mở khoá nào.
- Helm chưa có khối cho hai khoá mới; khi trunk có cấu hình thật (SIP-01) thì thêm cùng các khoá trunk khác.
