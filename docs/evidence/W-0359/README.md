# W-0359 — Lô `L4` của kế hoạch khắc phục `25/09`: intake và dựng lời thoại

Ngày 25/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"còn luồng này chúng ta tiếp tục l4
được chứ?"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch [`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) xếp lô `L4`
là năm lỗi ở intake và ở bước dựng lời thoại (`K-26…K-30`), đều là chỗ một đơn sai dữ liệu bị xử lý sai: thành `500`
để Module 3 gửi lại mãi, thành lỗi token để người trực đi tìm nhầm chỗ, hoặc làm cách ly một SIM không có lỗi gì.
Hai phiên chia việc qua tin nhắn: phiên kia làm `L5` (`W-0360`); phần `TryHangupAsync` của `K-31` nằm trong đúng hai
gateway `K-29` sửa, nên lô này làm luôn phần đó.

Lô được viết trên một bản sao `git archive` của `main@6b881f4`, ngoài cây chính, vì soak `W-0037` chạy tới 17:41 và
collector của phiên kia cần cây chính sạch. Build và test chạy sau đó.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-26` | `SpellQuantity` lấy chữ số lẻ bằng phép tính `decimal` từng bước, không cắt chuỗi đã làm tròn 9 chữ nữa. Trước đây một số lượng mười chữ số lẻ ném `IndexOutOfRangeException`, loại lỗi gateway đọc là hỏng kênh, nên cách ly SIM. Nay số lượng cần quá 3 chữ số lẻ luôn bị từ chối bằng `ArgumentOutOfRangeException`. `FormatQuantity` giữ **đủ** mọi chữ số khi phải đọc dạng số (trước làm tròn: `0,0005` đọc thành "0", `2,9999999999` đọc thành "3"), và bắt mọi loại lỗi speller có thể ném cho một giá trị | `UT-SPELL-QTY-01`, `UT-SPELL-QTY-02`, `UT-SPELL-QTY-03`, `UT-SPELL-QTY-04` |
| `K-27` | Guard toàn văn trên bản JSON của `privacy_safe_order_summary` (đúng bản IVR lưu) chạy trong khối kiểm privacy đã có, nên một tên hàng không dấu mà guard sản phẩm `W-0243` cho qua nhận `422 IVR_PII_POLICY_VIOLATION` (reason `PRIVACY_SAFE_SPEECH_REJECTED`) thay cho `500` ở bước lưu. Không đổi chính sách guard: nhận hay không những tên như vậy là `Q-12` | `UT-INTAKE-PII-23` |
| `K-28` | `400 IVR_MALFORMED_REQUEST` của intake có `details.field`: path của field sai (ví dụ `privacy_safe_order_summary.items[1].quantity`), không bao giờ kèm giá trị, không bao giờ nêu tên field lạ hay key do producer đặt. `message`, status, `code` giữ nguyên; `details` vốn khai `string → string` nên contract không đổi | `IT-INTAKE-SCHEMA-04`, `IT-INTAKE-SCHEMA-05`, `IT-INTAKE-SCHEMA-06` |
| `K-29` | Kịch bản chưa duyệt, placeholder không điền được, bản thoại quá giới hạn độ dài, guard toàn văn trên bản thoại: nay là `SpeechRenderPolicyRejectedException`, ghi mã `SPEECH_RENDER_POLICY_REJECTED` thay cho `*_POLICY_OR_TOKEN_REJECTED`. Disposition và sức khoẻ kênh giữ như cũ, chỉ đổi mã. Chú thích quá tay trên `SpeechRenderRejectedException` sửa lại cho đúng phạm vi. Mã kỹ thuật là chuỗi tự do trong OpenAPI nên không cần bump `draft.34` | `UT-RENDER-DATA-03` (sửa kỳ vọng), `UT-RENDER-DATA-04` |
| `K-30` | Test bù | `UT-AST-RENDER-DATA-10`, `IT-INTAKE-AMOUNT-17`, `IT-TEL-RENDER-DATA-10` |
| `K-31` (phần gateway) | `TryHangupAsync` ở hai gateway vẫn nuốt lỗi, nhưng đếm vào `ivr_fail_closed_total` (`ASTERISK_HANGUP_FAILED`, `MOCK_HANGUP_FAILED`) và ghi một dòng cảnh báo có **tên loại** lỗi, không có nội dung lỗi. Logger là tham số tuỳ chọn như `FeatureFlagPlatform`; EventId `2410`, `2420` | `UT-TEL-HANGUP-01`, `UT-AST-HANGUP-01` |
| Tài liệu | `specs/api/06-error-codes.md`: dòng Privacy mới, `details.field`; hai anchor dòng trong `docs/operations/production-dial-path.md` | `ci-config-selftest.mjs` |
| Ghim hash | `TaskIntakeService.cs` ở 4 nơi | `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`: `--self-test` + `--check-template` |

## Hành vi đổi trên dây, và phát hiện mới

- **Production:** một task có summary như ở `K-27` trước đây dừng ở `200 TASK_HELD_ADMIN_REVIEW` (vì
  `REAL_CUSTOMER_CALL_ALLOWED=NO`), `500` chỉ xảy ra ở MOCK và lab. Nay mọi môi trường nhận `422`, như mọi phép kiểm
  privacy khác đã làm; lỗi privacy cũng thắng `409 call_restriction` và `422` kịch bản chưa duyệt.
- Cùng hướng `500` → `422` còn có vài ca biên: tên hoặc địa bàn gửi dấu dạng tách rời (combining), key của
  `pronunciation_hints` trông như token, tổng tiền mà bản JSON trông như một số điện thoại.
- **Tên khách không dấu** có một trong các từ địa danh mơ hồ vẫn bị từ chối `422` ở guard của chính field đó (không
  phải lỗi mới của lô này). Điều đó đi ngược tinh thần `W-0105` (không bắt ai đổi họ); là câu hỏi chính sách cùng loại
  với `Q-12`, ghi vào kế hoạch, không sửa ở đây.
- **Số lượng mười chữ số lẻ** nay không cách ly SIM nữa, nhưng dạng số đầy đủ của nó có một dãy chữ số mà guard toàn
  văn đọc như số điện thoại, nên đơn đó bị từ chối theo nhánh `K-29` (kênh healthy). Đúng hướng: đó là lỗi dữ liệu.
- Một lần thất bại báo "kênh healthy" (như lời thoại bị từ chối) gọi `SimChannelFailurePolicy.RecordHealthy`,
  xoá chuỗi lỗi SIM trước đó của kênh. Kế hoạch đã có việc này là `K-45`; lô này không đổi hành vi đó.
- **Mới:** `PiiSafeLogRecordProcessor` chỉ xuất thuộc tính trong allowlist, và `ExceptionType` không có trong đó, nên
  tên loại lỗi của dòng log `K-31` (và của `FeatureFlagPlatform`) không tới OTLP; dòng log vẫn có `ReasonCode` và
  `AttemptId`. Ghi vào kế hoạch là `K-53`.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi (`dotnet build Ivr.sln`, trên bản sao `git archive` của `6b881f4` cộng lô này) |
| Test trên bản sao | unit `853/853`, contract `24/24`, chaos `8/8`, integration `423/426`. Ba ca đỏ đều do bản sao không có `deploy/ci/node_modules` và `.git`: hai ca `OpenApiDocumentTests` đạt khi nối `node_modules`; ca `ApiBehaviorMatrixTests` gọi `git ls-files`, chạy lại trong cây chính |
| Test trên cây chính sau khi land | build `0` cảnh báo; unit `860/860`, contract `24/24`, integration `426/426`, chaos `8/8` trên cây chính `2d91e1a` cộng lô này (đã gồm `W-0360`); `ApiBehaviorMatrixTests` đạt ở đây |
| Test mới | 14 TestId mới cộng `UT-RENDER-DATA-03` đổi kỳ vọng; bảng traceability sinh lại bằng `generate-test-traceability.mjs` (`875` → `889` trên bản sao; `896` trên cây chính cùng `W-0360`) |
| Ghim hash | `TaskIntakeService.cs` → `bb0662e3…`; `dial-token-production-bundle-validator.mjs` và `opt-out-suppression-bundle-validator.mjs` đạt `--self-test` và `--check-template` |
| Gate sweep | `GATE_SWEEP_PASS 44/44 run, 26 skipped by manifest`, exit `0` (`node deploy/ci/scripts/gate-sweep.mjs` từ Git Bash, 18:08–18:13 ngày 25/09, sau build và test) |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_WRITTEN gates=11 work=348 decisions=12`, rồi `GATE_STATUS_PASS` |
| Phạm vi | `gitnexus detect_changes` trước commit: 24 file, 133 symbol, 33 luồng, xếp mức `critical` vì chạm luồng intake `HandleAsync` và `DispatchAsync` của hai gateway; không có file ngoài danh sách của lô. Đối trọng là toàn bộ test trên cây chính và gate sweep ở trên |

## Chưa làm

Phần còn lại của `K-31` (callback, runtime gate) thuộc `L5` (`W-0360`). `IR-06` chưa nhắc `details.field`: tài liệu
đó ghim hash ở 7 nơi, để lần ghim lại kế tiếp. Chính sách guard tên hàng và tên khách là `Q-12`.
