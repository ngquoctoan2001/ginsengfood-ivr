# W-0036 — Gắn ID của P5-2 §8 vào test đang kiểm chúng, W-0347

Ngày: 23/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO · trạng thái hồ sơ: TESTS_PASS.

Mục 1 của README này đã đối chiếu từng ID §8 với test hiện có, theo assertion chứ không theo tên, và
cố ý không đổi tên test cũ. Bảng đó đúng, nhưng C2 không đọc được nó: C2 chỉ nhận một ID khi có test
mang đúng tag ấy và chạy đạt trong bộ kết quả của commit ứng viên.

W-0347 gắn **thêm** tag §8 vào đúng các test đó, và giữ nguyên tag cũ. Nó không đổi tên test nào.
Tiền lệ: bốn test trong repo đã mang hai TestId. Nhờ vậy bảng ánh xạ của mục 1 thành thứ C2 kiểm lại
được ở mỗi commit. Một ID chỉ được gắn khi các test cộng lại kiểm **đủ các vế** của định nghĩa. Vế nào
chưa có assertion thì W-0347 viết test cho vế đó.

| ID §8 | Định nghĩa rút gọn | Test mang tag |
| --- | --- | --- |
| `CT-CB-01` | 200 `ACCEPTED` → `DELIVERED_ACCEPTED`, task đóng | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08`, `E2E-FLOW-CONFIRM-01`. Test cuối nay kiểm thêm job đã đóng (`ClosedAt`). Mô hình hiện hành không có trạng thái "đóng" riêng cho task, vì D-02/DS-02 không cho IVR đổi task hay đơn |
| `CT-CB-02` | 200 `DUPLICATE_ACCEPTED` khi gửi lại cùng idempotency key → `DELIVERED_ACCEPTED`, không tạo bản ghi trùng | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08`, `UT-CALLBACK-RETRY-IDENTITY-02`, và **test mới** `AResendSalesAlreadyHasIsAcceptedOnceAndLeavesNoSecondRecord`: lease đầu mất ACK, lấy lại cùng id, key và hash; kết quả vẫn một bản ghi callback, không review |
| `CT-CB-03` | 200 `BLOCKED_BY_CORE` → `DELIVERED_BLOCKED`, không retry, hiện quyết định Core | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08`, `IT-ELIG-RACE-12` (mã Core được lưu, có review, đơn không đổi) |
| `CT-CB-04` | 200 `REVIEW_REQUIRED` → `DELIVERED_REVIEW`, vào hàng đợi admin | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08` (bật cờ review), `IT-CALLBACK-OUTBOX-06` (cờ review tạo một mục review admin xem được, trên Postgres thật) |
| `CT-CB-05` | 409 `REJECTED_STALE` → `REJECTED_STALE`, không retry, audit và review | Như trên. `IT-CALLBACK-OUTBOX-06` kiểm cả dòng audit của lần đổi trạng thái |
| `CT-CB-06` | 409 `IDEMPOTENCY_CONFLICT` → `IDEMPOTENCY_CONFLICT`, không retry | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08` |
| `CT-CB-07` | 422 → `INVALID_DEAD_LETTER`, không retry | `UT-CALLBACK-TARGET-ACK-01`, `UT-CALLBACK-STATE-08` |
| `CT-CB-08` | 429/500/503/timeout → `RETRY_PENDING` có giới hạn, cùng key; hết lượt → `RETRY_EXHAUSTED` + review | `UT-CALLBACK-TARGET-ACK-01` (429, 500, 503), `UT-CALLBACK-STATE-08`, `UT-CALLBACK-TIMEOUT-03`, `UT-CALLBACK-RETRY-IDENTITY-02`, `UT-CALLBACK-RETRY-EXHAUSTED-09` |
| `CT-CB-09` | Bộ compat Giờ Vàng riêng (200/422), 24/7 bị từ chối trên route compat | `UT-CALLBACK-GH-COMPAT-06` (200), `UT-CALLBACK-GH-ISOLATION-07` (24/7), và **test mới** `CurrentGoldenHourTransportTreatsA422AsARejectionNotARetry` (422 là từ chối, không retry) |
| `E2E-CONFIRM-01` | Luồng xác nhận → detail hiện tín hiệu CONFIRMED + callback 200 | `E2E-FLOW-CONFIRM-01` |
| `E2E-NOANSWER-02` | A1 không nghe → A2 → final; IVR không đổi đơn (DS-02) | **Test mới** `TwoUnansweredAttemptsEndInOneFinalNoAnswerAndTheOrderNeverMoves`, dùng hai lượt của Giờ Vàng. A1 được đếm nhưng chưa final, job quay lại hàng đợi, chưa báo Sales. A2 final, job đóng, đúng một callback. Đơn giữ nguyên trạng thái và version |
| `E2E-RACE-03` | Phím 1 + blocker → blocked, không confirm | `IT-ELIG-RACE-12` |

Hai phép thử đột biến cho thấy test mới không kiểm suông. Ép no-answer luôn là final thì test
`E2E-NOANSWER-02` đỏ ngay ở A1. Test lease của W-0015 cũng được thử theo cách tương tự.

## Còn chờ Toàn quyết

`CT-OAS-01..03` (OpenAPI parse/ref/enum) và `CT-TASK-01..04` (task schema + policy mismatch) được
prompt định nghĩa **theo nhóm**, không theo từng số. Mục 1 ánh xạ chúng sang `CT-API-OAS-10`, gate
`validate-openapi.mjs` và bộ `CT-INTAKE-*`/`IT-INTAKE-*`.

Chọn test nào gánh số nào là việc diễn giải yêu cầu gốc. "Policy mismatch" ở intake cũng không còn là
một nhánh lỗi riêng, vì các lỗi `PolicyMismatch` hiện chỉ có ở API admin. Vì vậy W-0347 không tự gán
bảy ID này, và W-0036 vẫn trượt C2 vì chúng cho tới khi Toàn quyết.

Chỉ Toàn chuyển W-0036 sang `ACCEPTED`.

## Bổ sung W-0353 — 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO. Mục "Còn chờ Toàn quyết" ở trên đã cũ. Ngày 24/09 Toàn yêu cầu làm
nốt phần việc IVR còn lại, nên W-0353 chọn test cho bảy ID theo bảng dưới. Không test nào bị đổi
tên; test cũ chỉ được gắn thêm tag, như W-0347 đã làm. Chỉ Toàn chuyển W-0036 sang `ACCEPTED`.

| ID §8 | Test mang tag |
| --- | --- |
| `CT-OAS-01` | Test mới `EveryOpenApiDocumentParsesAndValidates`: chạy `validate-openapi.mjs`, đòi exit 0, `OPENAPI_FILES_VALID=2` và một dòng `OPENAPI_PARSE_PASS` cho mỗi tài liệu |
| `CT-OAS-02` | Test mới `ADocumentWithADanglingReferenceIsRefused`: chạy `selftest-openapi.mjs` và đọc fixture của nó. Script đạt với bất kỳ lỗi nào, nên test còn đòi fixture vẫn mang một `$ref` trỏ vào schema không có |
| `CT-OAS-03` | Hai test mới trong `OpenApiEnumParityTests`: enum `ErrorCode` của spec trùng `IvrErrorCodes.All` và enum sinh ra; enum `ProgramCode` của spec trùng enum sinh ra, và mỗi chương trình có policy đã ký đúng D-10 (hai lượt, lượt hai ở giữa window) |
| `CT-TASK-01` | `IT-INTAKE-HAPPY-01`: task hợp lệ của cả hai cặp chương trình và thanh toán được nhận |
| `CT-TASK-02` | `IT-INTAKE-SCHEMA-03`, và test mới `EveryCanonicalSchemaNegativeFixtureIsRefusedOverTheWire`: gửi cả 16 fixture `schema_negative` của seed qua HTTP, mỗi cái phải nhận `400 IVR_MALFORMED_REQUEST`, không tạo call job, không ghi audit. Trước đây chưa test nào gửi nhóm fixture này tới endpoint |
| `CT-TASK-03` | `IT-INTAKE-HAPPY-01` (hai cặp đúng được nhận) và `IT-INTAKE-REASON-WIRE-15` (cặp sai bị từ chối 400) |
| `CT-TASK-04` | `UT-INTAKE-NOJOB-16` ở mức service, và test mới `PolicySnapshotMismatchIs409AndCreatesNothing` qua HTTP với ba ca: `NEG-DOMAIN-POLICY-02`, `NEG-DOMAIN-POLICY-03` và một task Giờ Vàng có window 900 giây thay vì 300. Cả ba nhận `409 IVR_POLICY_MISMATCH`, không tạo task, call job hay outbox |

**Sửa một câu sai ở trên.** Lỗi `PolicyMismatch` không chỉ có ở API admin. Intake trả
`409 IVR_POLICY_MISMATCH` khi snapshot policy của task lệch với policy IVR tra được: số lượt, offset
hoặc độ dài window (`TaskIntakeService.WirePolicyMatches`). `IT-INTAKE-NEGATIVE-18` đã kiểm nhánh
này qua HTTP từ trước, với hai fixture POLICY. Vậy "policy mismatch" ở intake là một nhánh lỗi riêng
có thật, và `CT-TASK-04` gắn vào chính nhánh đó.

Mỗi test mới đã được thử đột biến trong một bản clone rồi hoàn lại, và đều đỏ:

- Bỏ luật "không bắt đầu bằng chữ số" của `delivery_area_short`: `CT-TASK-02` đỏ đúng ở
  `NEG-SCHEMA-PII-02` (thành 422), trong khi `IT-INTAKE-SCHEMA-03` vẫn xanh.
- Bỏ phép so độ dài window: ca 900 giây đỏ (thành lỗi 500 ở tầng sau), hai ca fixture vẫn xanh.
  Chỉ ca này giữ luật độ dài window.
- Một `$ref` gãy trong spec callback: `CT-OAS-01` đỏ.
- Fixture gãy theo kiểu khác (bỏ `$ref`, đổi phiên bản `openapi`): script vẫn in PASS nhưng
  `CT-OAS-02` đỏ.
- Bớt một mã lỗi khỏi spec, thêm một chương trình vào spec, hoặc dời lượt hai của 24/7: `CT-OAS-03`
  đỏ.
