# W-0365 — `K-54`, `K-55`, `K-56` của kế hoạch khắc phục `25/09`: ba phát hiện của lô `L7`

Ngày 26/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp tục K-54, K-55, K-56"* ·
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Lô `L7` (`W-0362`) tìm ra ba lỗi và ghi vào kế hoạch
[`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) thay vì sửa ngay:

- sweep hết hạn bỏ sót hai kiểu job, nên Module 3 không bao giờ nhận callback cho chúng;
- guard PII của audit đọc JSON như text, nên không thấy những gì bộ serialize escape;
- một sự kiện ARI sai kiểu làm chết vòng nhận sự kiện.

Lô được viết trên ba bản sao `git archive` của `main@b0d2515f`, ngoài cây chính, mỗi mục một bản sao và không chung file
production nào. Phiên lập kế hoạch làm `K-55`, hai sub-agent làm `K-54` và `K-56`. Phiên lập kế hoạch duyệt từng diff,
đóng dấu `W-0365` rồi land lên `main@d8c1a360` (sau `W-0364` của phiên kia) và chạy toàn bộ suite trên cây chính.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-54` | Sweep hết hạn (`PostgresSchedulerStore.CloseMissedDeadlinesAsync`) có thêm hai nhánh, khoá theo quyết định eligibility chứ không chỉ theo trạng thái: job eligibility giữ chờ người duyệt (`TASK_HELD_ADMIN_REVIEW`, `eligible=false`), và job còn chờ eligibility (`PENDING_ELIGIBILITY`) ở cả hai dạng lab và MOCK. Mọi hàng rào exactly-once của `K-52` giữ nguyên cho mọi nhánh. Kết cục theo bảng map và DT-06: `IVR_CONFIRMATION_WINDOW_EXPIRED`, không tính lượt, `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` vì chưa có lượt nào, một callback, một dòng audit ghi thêm quyết định eligibility lúc đóng; không mở incident dung lượng. `PostgresEligibilityRepository.PersistAsync` đọc job dưới khoá dòng và từ chối job đã đóng, để một lượt đánh giá chạy song song với sweep không trả job đã xong về cho scheduler; endpoint trả `409 IVR_POLICY_MISMATCH` thay vì `500` | `IT-SCH-ADMIN-HELD-01`, `IT-SCH-PENDING-ELIG-01`, `IT-ELIG-CLOSED-01` |
| `K-55` | `PiiGuard.EnsureSafeJsonText`: kiểm JSON trên text như đã viết (như trước) **và** trên từng chuỗi, từng tên thuộc tính sau khi giải mã. `JsonSerializer` mặc định escape dấu `+` và mọi chữ có dấu thành `\u…`, nên số dạng `+84` và các dấu địa chỉ có dấu lọt qua phép kiểm text, rồi cột `jsonb` lưu lại đúng ký tự đã giải mã. Áp cho ba cột JSON của mọi dòng audit được thêm (`PersistenceInvariantValidator`), cho `PostgresAuditLogger` (kiểm cả trước khi mở context) và `InMemoryAuditLogger`. Không nới guard nào: mọi thứ trước đây bị từ chối vẫn bị từ chối | `UT-PII-JSON-01`, `UT-PII-JSON-02`, `IT-DB-AUDIT-PII-11` |
| `K-56` | `ProcessEvent` kiểm kiểu của từng giá trị trước khi đọc: sự kiện không phải object, `type` không phải chuỗi, `channel` không phải object, `channel.id` không phải chuỗi thì bỏ qua như JSON hỏng; phím không phải chuỗi thì bỏ qua (một phím là kết cục duy nhất được thi hành mà không hỏi lại); `cause`, `cause_txt` sai kiểu thì coi như vắng, và cuộc gọi vẫn kết thúc. Phép bắt `JsonException` của vòng nhận sự kiện giữ hẹp có chủ ý. `PlayAsync` với cuộc bị phía IVR đóng vì mất luồng báo đúng việc đó: `NetworkError`, mã của luồng (`ASTERISK_EVENT_STREAM_LOST`…), cùng câu trả lời `GetDispositionAsync` cho cuộc đó; cuộc do Asterisk tự kết thúc giữ `ASTERISK_CHANNEL_ALREADY_ENDED` | `UT-AST-EVENTS-13`, `UT-AST-EVENTS-14` |

## Đo trước khi chặn (`K-55`)

Kế hoạch yêu cầu đo trước xem chữ tiếng Việt nào đang qua được mà sẽ bị từ chối. Toàn bộ suite (unit `887`, contract `24`,
chaos `8`, integration `434`) chạy một lượt trên bản sao với một bản đo của phép kiểm mới: ghi ra file thay vì từ chối.

- Không dòng audit nào bị phép kiểm mới từ chối.
- Thứ duy nhất bị escape trong JSON audit của cả suite là mốc thời gian có `+00:00` (65 lần), không phải dữ liệu khách.
  Không có chữ tiếng Việt nào trong JSON audit của suite.
- Soát tay các lối ghi audit mang chữ tự do: lý do và kết luận của admin, phần tử allowlist lab của feature flag. Cả
  hai đã được kiểm dưới dạng chuỗi gốc trước khi vào JSON, nên phép kiểm mới không từ chối thêm gì ở đó.

Bản đo không nằm trong commit; mã nguồn được khôi phục và kiểm lại không còn dấu vết trước khi land.

## Hành vi đổi

- **Module 3:** đơn bị eligibility giữ chờ người duyệt, và đơn eligibility chưa kịp trả lời, nay nhận callback
  `IVR_CONFIRMATION_WINDOW_EXPIRED` với `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` khi hết cửa sổ, thay vì không bao giờ
  nhận gì. Ba chỗ trong IR-06 nói khác (bảng từ vựng trên dây ghi "im lặng", và khối bảng map ghi cặp kết quả này chỉ vì
  lỗi phía IVR); file bị ghim hash nên không sửa ở lô này, ghi thành `K-59`.
- **Worker eligibility:** đánh giá một job sweep đã đóng nhận `409 IVR_POLICY_MISMATCH`, không ghi gì; job đó không
  bao giờ được đọc lại.
- **Triển khai bật scheduler mà tắt eligibility polling** (ví dụ `docker-compose.softphone.yml`): mọi task đã nhận nay
  đóng khi hết cửa sổ, có callback.
- **Task bị thu hồi** mà còn chờ eligibility hoặc bị giữ chờ duyệt nay đóng khi hết cửa sổ, như job đủ điều kiện bị thu
  hồi vẫn đóng từ trước. Không thêm vị từ `revoked_at` (`Q-06` PA2 giữ nguyên); gói `C13` phải phủ cả hai nhánh mới.
  Hôm nay không code nào ghi `revoked_at`, nên production không đổi.
- **Audit:** dòng có số `+84` hoặc dấu địa chỉ có dấu nằm trong JSON (dạng escape) bị từ chối; không lối ghi nào hôm nay
  tạo ra dòng như vậy.
- **ARI:** sự kiện sai kiểu bị bỏ qua thay vì làm chết vòng nhận sự kiện. Mất luồng trước khi phát nay ghi kênh không
  lành, cộng một lỗi vào chuỗi lỗi SIM, như nhánh bắt phím đã làm từ `K-47`; việc mất luồng có nên tính vào SIM hay
  không là `K-57`.

## Phát hiện mới

Ghi vào kế hoạch, không sửa ở đây:

- **`K-57`:** mất luồng ARI không phải lỗi của SIM, nhưng cả nhánh bắt phím (từ `K-47`) lẫn nhánh phát (`K-56`) đều cộng
  một lỗi vào chuỗi lỗi SIM. Một luồng sự kiện dùng chung cho mọi SIM của worker, nên một lần chập mạng cộng lỗi cho mọi
  SIM đang có cuộc gọi; ba lần trong 10 phút là `HEALTH_FAILED`, và kênh Asterisk không có lối bật lại. Cần một kết cục
  trung tính ở `FinalizeAsync` (không cộng, không xoá chuỗi lỗi). Cùng chỗ: raw event ghi audio `PLAYED` cho mọi lỗi sau
  khi quay không phải lỗi audio, kể cả khi phát bị từ chối.
- **`K-58`:** `AsteriskAriSimGateway.DisposeAsync` chờ bắt tay đóng WebSocket không có timeout, nên một đầu kia không trả
  lời làm treo lúc worker tắt; và ném lại mọi lỗi khác `WebSocketException` của vòng nhận sự kiện.
- **`K-59`:** IR-06 (tài liệu gửi Module 3) nói sai sau `K-52` và `K-54`: đơn bị giữ hoặc chưa được trả lời nay có
  callback khi hết cửa sổ. Sửa ở lần ghim lại IR-06 kế tiếp, kèm chuỗi ghim, và báo Module 3.
- **`K-60`:** eligibility chặn (`TASK_BLOCKED_OPERATIONAL`) đóng job mà không có callback, dù intake đã bảo Module 3 chờ
  callback; và eligibility có thể chạy sau khi cửa sổ đã hết rồi chặn vì hết cửa sổ, tranh với sweep của `K-54`: ai lấy
  khoá dòng trước quyết định Module 3 có nhận callback hay không. Có thể cần quyết định về hợp đồng.
- **`K-61`:** cảnh báo `IvrConfirmationDeadlineMissed` cộng mọi lý do, và runbook của nó bảo hiệu chỉnh lại mô hình dung
  lượng; sau `K-52` và `K-54`, các đơn hết cửa sổ không vì thiếu kênh cũng bật cảnh báo này.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi, trên cây chính sau khi land |
| Impact | Trước khi sửa: `PersistenceInvariantValidator.Validate` CRITICAL (285 symbol, 28 luồng), `AsteriskAriSimGateway.PlayAsync` HIGH (phần lớn do chỉ mục gộp sáu bản cài của `ISimGateway`), `PostgresSchedulerStore.CloseMissedDeadlinesAsync` HIGH (30 symbol, 10 trực tiếp), `PostgresEligibilityRepository.PersistAsync`, `EvaluateEligibilityAsync`, `ProcessEvent`, `PostgresAuditLogger.AppendToContextAsync` LOW; đã báo Toàn, và đã chạy toàn bộ suite, chaos gồm cả |
| Đột biến | `K-54`: 8/9 bị bắt; con còn sống là bỏ vị từ `eligibility_decision`, vị từ phòng thủ mà không dữ liệu thật nào phân biệt được. `K-55`: 7/7 (bước duyệt JSON đã giải mã; từng cột trong ba cột JSON; lối kiểm sớm của logger Postgres; logger in-memory). `K-56`: 12/12; thêm một phép thử kiểm chứng (nới rộng phép bắt lỗi) sống như dự đoán, là lý do giữ phép bắt hẹp |
| Test trên bản sao | `K-54`: integration `440/443`, unit `887/887`; `K-56`: unit `888/889`. Ca đỏ đều do môi trường bản sao: `OpenApiDocumentTests` thiếu `node_modules`, `ApiBehaviorMatrixTests` thiếu `.git`, và bảng traceability chưa sinh lại |
| Test trên cây chính sau khi land | `main@d8c1a360` (`W-0364` của phiên kia) cộng lô: unit `912/912`, contract `24/24`, integration `444/444`, chaos `8/8`, tổng `1388/1388`, build `0` cảnh báo; HEAD không đổi suốt lượt. Ba ca chỉ chạy được ở cây chính (`OpenApiDocumentTests`, `ApiBehaviorMatrixTests`) đều đạt |
| Test mới | 13 dòng traceability mới; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`923` → `936`) |
| Gate sweep | `GATE_SWEEP_PASS 45/45 run, 26 skipped by manifest` (Git Bash, trên cây đã land); trong đó `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 353 work item |
| Phạm vi | `gitnexus detect_changes` trước commit: 123 symbol trong 14 file đã theo dõi (gồm file kế hoạch đang sửa dở của phiên khác, không thuộc lô, và bảng traceability; nhiều symbol chỉ lệch dòng; file test mới chưa vào index), 27 luồng bị ảnh hưởng, rủi ro `critical`: mọi lối lưu có dòng audit (qua `ValidateAudit` và `AppendAsync` của logger), cùng `EvaluateEligibilityAsync` và luồng bắt phím; đúng với impact đã báo trước khi sửa, và đều nằm trong lượt test trên cây chính |

## Chưa làm

`K-57` tới `K-61` như trên. Sửa lời IR-06 thuộc `K-59`. `Q-22.2` của phiên kia sửa cùng hàm sweep và chờ lô này vào
`main`.
