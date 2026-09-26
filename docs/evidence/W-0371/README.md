# W-0371 — Phần II của kế hoạch khắc phục `25/09`: `Q-16` và `Q-19.1`, contract `1.0.0-draft.34`

Ngày 26/09/2026 · Claude, lô code độc lập Toàn giao sau câu *"chuyển qua phần 2 hả"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu. Kế hoạch ghi `Q-16` cần chief duyệt; chief chưa được báo.

## Vì sao có việc này

**`Q-16` (PA2, Toàn chốt 26/09).** Có một lớp lỗi mà intake nhận task rồi lúc quay mới từ chối (`B16`): tổng tiền quá
lớn để đọc thành lời, số lượng mà chữ số trông như số điện thoại, lời thoại quá giới hạn độ dài. Module 3 được báo
nhận, rồi mọi lần quay đều hỏng tới hết cửa sổ mà không ai được gọi. PA2 cho intake dựng thử lời thoại ngay khi đã có
kịch bản đã duyệt, và từ chối với một reason mới, dùng decision và mã lỗi có sẵn. Toàn chọn mã: như lỗi tóm tắt đơn hiện
có, tức giữ để duyệt, `422 IVR_PII_POLICY_VIOLATION`. Việc này làm sau `Q-12` (`W-0370`), để tên hàng intake đã nhận
không bị chặn ngay tại cửa.

**`Q-19.1` (PA2, Toàn chốt 26/09).** Endpoint phát lại callback đã chết không giới hạn tuổi. Một người tầng danger có thể
phát lại một `IVR_CONFIRMED` sau nhiều ngày, khi Module 3 đã không còn giữ idempotency key của nó. PA2 chặn cứng theo
tuổi: mặc định `7` ngày theo đề xuất `M3-10`, cấu hình `1–30`. Cùng lượt, endpoint nhận thêm `AUTH_REJECTED`: khi IVR
xác thực callback, một credential sai làm mọi kết quả rơi vào trạng thái này, và trước đây chỉ gỡ được bằng `UPDATE`.

Hai việc đổi mô tả contract, nên đi chung một lần nâng lên `draft.34` và một lượt sửa IR-06 (luật 5 và 6 của kế hoạch).
Lượt này cũng sửa summary cũ của `POST /feature-flags/{environment}` mà `K-15` để lại cho lần nâng contract kế tiếp.

Phân tích tác động trước khi sửa: `TaskIntakeService.IntakeAsync` ở mức CRITICAL (38 chỗ gọi, gồm endpoint intake), Toàn
duyệt; hàm dựng của `TaskIntakeService` MEDIUM; `ReplayCallbackAsync` và hàm dựng của `InternalAdminApiService` LOW. Lô
làm trên bản sao tách riêng của `0e6bb73e`.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| Intake dựng thử lời thoại | `TaskIntakeService` nhận bộ dựng lời thoại của lúc quay qua một tham số tuỳ chọn; service do container dựng luôn có nó (`RendersBeforeAccepting`). Sau khi đã có kịch bản đã duyệt, intake dựng thử; hai loại từ chối của bộ dựng lời thoại (giá trị bộ đọc số không đọc được, kịch bản bị chính sách từ chối) thành `TASK_HELD_ADMIN_REVIEW`, `IVR_PII_POLICY_VIOLATION`, reason `SPEECH_SUMMARY_NOT_RENDERABLE`. Lỗi khác của bộ dựng lời thoại không bị coi là phán quyết về đơn, nên vẫn ném ra | `UT-INTAKE-RENDER-01`, `UT-INTAKE-RENDER-02` |
| Qua HTTP | Tổng tiền vượt ngưỡng đọc được nhận `422`, không tạo job; tên hàng mà `Q-12` cho đọc vẫn nhận `200` | `IT-INTAKE-RENDER-01` |
| Giới hạn tuổi phát lại | `CallbackReplayOptions` (Infrastructure, API bind và kiểm lúc khởi động): `MaxAgeDays` mặc định `7`, ngoài `1–30` thì không khởi động. Callback cũ hơn giới hạn nhận `409 IVR_VERSION_CONFLICT`, không ghi gì | `UT-CB-REPLAY-AGE-01`, `IT-API-DEADLETTER-15`, `IT-API-DEADLETTER-16` |
| Phát lại `AUTH_REJECTED` | Trạng thái này được đưa lại hàng đợi gửi như hai trạng thái chết kia | `IT-API-DEADLETTER-17` |
| Contract `draft.34` | Chỉ đổi mô tả: `total_amount` và `quantity` ghi giới hạn đọc được, endpoint phát lại ghi trạng thái mới và giới hạn tuổi, summary của `POST /feature-flags/{environment}` bỏ chữ cũ. `oasdiff` không báo breaking cho `33→34` lẫn `27→34`. Đủ chuỗi ghim: baseline `draft.34`, changelog sinh bằng image `oasdiff` đã ghim, `contract-manifest.json`, model sinh lại bằng NSwag và hash của nó, field inventory, portal, và hash spec trong validator dial-token cùng template `W-0183` | `CT-M3-AUTHORITY-08` (bộ contract, khẳng định phiên bản spec), các gate nêu ở phần kiểm chứng |
| IR-06, IR-07, IR-08, bảng mã lỗi | IR-06: mọi con trỏ phiên bản hiện hành sang `draft.34`, thêm dòng vào bảng từ chối và vào dòng `total_amount`, đoạn phát lại §4A.3 nay có giới hạn tuổi. IR-07: sửa tại chỗ dòng `M3-10` · `D-5` và `E-8`, thêm mục đính chính `26/09` cho `draft.34`. IR-08: hai chỗ nhắc phiên bản. Bảng mã lỗi thêm một dòng. IR-06 được ghim lại ở bốn validator và ba template | các gate nêu ở phần kiểm chứng |

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của lô trong bản sao, rồi ghi lại byte gốc (không dùng `git checkout`). Phép sửa code build
lại project chứa test được nêu và chạy test; phép sửa contract đổi một con trỏ hay một chỗ ghim rồi chạy gate được nêu.
Mười một phép đều đỏ đúng chỗ, ba lượt đối chứng đều xanh:

- intake: không bao giờ dựng thử; giá trị bộ đọc số không đọc được thoát ra thành exception; kịch bản bị chính sách từ
  chối thoát ra thành exception; mọi lỗi của bộ dựng lời thoại đều bị coi là từ chối; lời từ chối mang reason của guard
  tóm tắt thay vì reason riêng;
- phát lại: không có giới hạn tuổi; `AUTH_REJECTED` vẫn bị từ chối; giới hạn mặc định là `30` ngày; giới hạn `0` ngày
  vẫn qua kiểm;
- contract: IR-06 vẫn bảo Module 3 sinh client từ `draft.33` (`contract-freeze-verifier.mjs` đỏ); validator `d06` giữ
  hash IR-06 cũ (`d06-revalidation-evidence-validator.mjs` đỏ).

Số liệu ở [mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build cả solution trong bản sao (`0e6bb73e` cộng lô này), analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit, bản sao | `947/947` |
| Bộ contract, bản sao | `24/24`, gồm `CT-M3-AUTHORITY-08` khẳng định `version: 1.0.0-draft.34` |
| Bộ integration, bản sao | `462/463`, 9 phút 26 giây. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git; trên `main` nó được chạy lại |
| Contract, bản sao | `oasdiff breaking` (image đã ghim) không có dòng nào cho `27→34` lẫn `33→34`; `contract-freeze-verifier.mjs` (`CONTRACT_FREEZE=PASS`, `intake=1.0.0-draft.34`), `openapi-contract-drift.mjs`, `docs-selftest.mjs`, `openapi-lint-gate.mjs` đạt |
| Chuỗi ghim, bản sao | IR-06 ghim lại ở bốn validator và ba template; hash spec ở validator dial-token và template `W-0183`; `TaskIntakeService.cs`, cũng bị ghim ở dial-token, opt-out, `W-0183` và `W-0187`, được ghim lại sau khi lượt sweep đầu báo lệch. `d06-revalidation-evidence-validator.mjs`, `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`, `upstream-session-signoff-validator.mjs` đạt self-test |
| Gate sweep, bản sao | lượt đầu `44/46` (hai gate báo `TaskIntakeService.cs` lệch ghim), sau khi ghim lại `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest |
| Trên `main` sau khi đưa vào (`f64021cd` cộng lô này), 15:15–15:31 | build 0 cảnh báo; unit `947/947`; contract `24/24`; integration `463/463` ngay lượt đầu, 10 phút 34 giây, có ghi `trx`, gồm `IT-API-MATRIX-38`; `generate-test-traceability.mjs` sinh lại `972` dòng; `gate-status.mjs` viết lại; `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest, gồm quét PII hồ sơ |

## Còn lại

- Chief chưa duyệt `Q-16` như kế hoạch ghi, và Module 3 chưa nhận IR-07 bản này (`CB-01`).
- Sandbox chưa có ví dụ cho ca `422` mới; nếu Module 3 cần diễn tập thì thêm vào `sandbox-examples.mjs` cùng `Q-10`.
