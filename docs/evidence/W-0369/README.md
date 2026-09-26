# W-0369 — `K-58`…`K-62` của kế hoạch khắc phục `25/09`: năm phát hiện của `W-0365` và `W-0367`

Ngày 26/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp tục K-58 69 60 61 62"* (`69` đọc là `K-59`:
kế hoạch không có `K-69`) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

`W-0365` (`K-54`…`K-56`) và `W-0367` (`K-57`) ghi năm phát hiện vào kế hoạch
[`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) thay vì sửa ngay:

- tắt adapter ARI có thể treo worker;
- IR-06 nói sai sau `K-52` và `K-54`;
- eligibility chặn không phát callback và tranh với sweep hết hạn;
- cảnh báo trễ hạn cộng mọi lý do;
- Asterisk không truy cập được vẫn tính lỗi vào SIM.

Lô được viết trên bốn bản sao `git archive` của `main@4ae8a5ca`, bốn nhóm không chung file nào. Phiên lập kế hoạch làm
`K-60` và soạn quyết định `Q-37`. Ba sub-agent làm `K-58` cùng `K-62` (chung file adapter ARI), `K-59` và `K-61`. Phiên lập
kế hoạch duyệt từng diff, đóng dấu `W-0369` rồi land lên `main@59a76487`.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-58` | `AsteriskAriSimGateway.DisposeAsync` đóng luồng sự kiện trong giới hạn 5 giây (hằng `EventStreamCloseTimeout`), quá hạn thì huỷ socket; chờ vòng nhận sự kiện mà không để lỗi nào của nó thoát ra, vì dispose là việc cố gắng hết sức và container DI dừng ở lỗi đầu tiên, làm worker thoát. Cuộc gọi đang mở kết thúc như trước: `ASTERISK_EVENT_STREAM_CLOSED` nếu Asterisk trả lời frame đóng, `ASTERISK_EVENT_STREAM_LOST` nếu phải huỷ; không tính lượt, không nói gì về SIM | `UT-AST-DISPOSE-01`, `UT-AST-DISPOSE-02` |
| `K-62` | Phương án khuyến nghị PA1: Asterisk không truy cập được không còn tính vào SIM. `ASTERISK_EVENT_STREAM_UNAVAILABLE`, `ASTERISK_HTTP_UNAVAILABLE`, và ở dispatch gateway `ASTERISK_CHANNEL_HEALTH_NOT_READY`, báo sức khoẻ kênh là `null`. Mã thứ ba thêm ngoài danh sách của kế hoạch: lượt kiểm sức khoẻ trước khi quay chỉ ping Asterisk, cùng một câu trả lời cho mọi kênh, nên "Asterisk sập ở lần quay sau" đi vào đúng mã này. Cầu dao là backoff sẵn có của `SchedulerDispatchPump`: nhân đôi tới 30 giây, xoá khi có một cuộc thành công. Spec SIM adapter thêm một câu | `UT-AST-UNAVAILABLE-01`, `UT-AST-UNAVAILABLE-02`; `UT-AST-HEALTH-01` đổi mã ví dụ của lỗi sau khi phát sang `ASTERISK_NETWORK_FAILURE`, mã adapter thật sự báo là lỗi kênh |
| `K-59` | IR-06 §3.11, khối C22 ở §4.3 và §10 nói đúng hành vi sau `K-52`, `K-54`, `K-60`; IR-07 thêm mục "Đính chính bổ sung `2026-09-26`". Câu cũ "`ELIGIBLE_FOR_IVR` → `200 TASK_HELD_ADMIN_REVIEW`" đã sai từ trước `K-54`: intake không đọc giá trị này, việc giữ xảy ra ở bước eligibility sau intake. Hash IR-06 ghim lại ở bốn validator và ba template; không đổi con trỏ phiên bản nào | self-test của `d06-revalidation-evidence-validator.mjs`, `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`, `upstream-session-signoff-validator.mjs`; `contract-freeze-verifier.mjs` |
| `K-60` | Lượt xét eligibility tới một job đã hết cửa sổ bị từ chối dưới khoá dòng: không ghi gì, endpoint trả `409 IVR_POLICY_MISMATCH`. Job luôn do sweep hết hạn đóng, với `IVR_CONFIRMATION_WINDOW_EXPIRED` và callback; trước đây luật eligibility chặn nó im lặng hoặc sweep đóng nó, tuỳ bên nào lấy dòng trước. Phần hợp đồng, task bị chặn sau intake không có callback, thành `Q-37` chờ chief | `IT-ELIG-EXPIRED-01` |
| `K-61` | `IvrConfirmationDeadlineMissed` chỉ đếm lý do `NO_DISPATCH_BEFORE_DEADLINE`, nên runbook "hiệu chỉnh mô hình dung lượng" đúng trở lại. Cảnh báo mới `IvrConfirmationWindowsExpiringUndialled` (ticket): cửa sổ cứ hết mà nửa giờ không có cuộc nào được quay, tức vòng eligibility ngừng trả lời; trước đây không cảnh báo nào bắt được việc này. Runbook `docs/slo.md` §9 và §9a mới; `capacity-selftest.mjs` (CAP-ALERT-04) đọc lại nhãn và từng lý do từ code C#, nên một lý do mới chưa được cảnh báo nào nhận thì gate đỏ | `IT-SLO-CAPACITY-04` (promtool, năm ca mới); `capacity-selftest.mjs` |

## Quyết định

- **`K-62`:** ba phương án. PA1: không tính vào SIM, dựa vào backoff của pump. PA2: PA1 cộng một lượt hỏi sức khoẻ adapter
  trước khi nhận job khi đang backoff, để không đốt job nào lúc Asterisk sập. PA3: giữ việc tính lỗi, thêm lối bật lại
  kênh Asterisk. Làm PA1 theo khuyến nghị đã ghi ở `K-62`; mỗi mã là một giá trị trong code, đổi lại được. Cái giá: khi
  Asterisk sập, mỗi cửa sổ backoff (15–30 giây ở trần) có một job lỗi kỹ thuật không tính lượt và dùng một lượt thử kỹ
  thuật.
- **`Q-37`** (phần hợp đồng của `K-60`, chief quyết, báo M3): task đã nhận mà eligibility chặn thì Module 3 có nhận
  callback không. Ba phương án và khuyến nghị PA2 (để sweep đóng như `K-54`) nằm trong kế hoạch.
- **Mức cảnh báo của `K-61`:** cảnh báo mới ở mức ticket như chỉ dẫn. Một lần vòng eligibility ngừng làm mọi đơn không được
  gọi tới khi có người xử lý, nên owner có thể muốn nâng lên page.

## Hành vi đổi

- **Tắt worker** không còn treo vì Asterisk không trả lời frame đóng, và không còn ném lỗi của vòng nhận sự kiện.
- **Kênh SIM:** Asterisk không truy cập được không còn cộng lỗi, không còn đưa SIM tới `HEALTH_FAILED`.
- **Eligibility:** lượt xét trễ nhận `409`; job hết cửa sổ luôn có callback.
- **Cảnh báo:** cảnh báo dung lượng thôi kêu cho đơn hết cửa sổ vì lý do khác. Cảnh báo mới kêu ở mọi nơi có task vào mà
  vòng eligibility tắt; mặc định trong các file values Helm là tắt.
- **Tài liệu gửi Module 3:** IR-06 và IR-07 đổi. Module 3 cần được báo, gồm callback mới cho task bị giữ hoặc chưa được
  xét, và việc cặp `IVR_CONFIRMATION_WINDOW_EXPIRED` + `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` không còn chỉ nghĩa là lỗi
  phía IVR. Phiên này không gửi gì.

## Phát hiện mới

Ghi vào kế hoạch, không sửa ở đây:

- **`K-63`:** lớp HTTP của adapter ARI. Asterisk bị chặn gói (không từ chối) làm lời gọi hết hạn sau 100 giây, vì client
  không đặt timeout; lỗi đó rơi vào nhánh chung `ASTERISK_DISPATCH_TECHNICAL_FAILURE` và vẫn tính vào SIM. Trả lời HTTP
  không phải 2xx: `503` tính vào SIM, mọi mã khác (kể cả `401`, `403`) coi là kênh lành và xoá chuỗi lỗi.
- **`K-64`:** giám sát. Panel trễ hạn trên dashboard vẫn cộng mọi lý do và vẫn ghi rằng lớn hơn 0 là mô hình sai; eligibility
  hỏng một phần (có đơn được xét, có đơn không) chưa có cảnh báo, vì counter không có lý do riêng cho đơn chưa được xét.
- **`K-65`:** tài liệu lệch tìm thấy lúc làm `K-59`: F05 dòng 46 còn câu về timeout `IVR_NO_ANSWER_FINAL` từ trước C7; IR-06
  nói MOCK không có callback cho khách thật, nhưng sandbox MOCK có phát callback cho job dry-run, nay gồm cả job chưa được
  xét.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi, trên cây chính sau khi land |
| Impact | Trước khi sửa: `AsteriskAriOperationException` HIGH, `AsteriskAriSimGateway.PlayAsync` HIGH (qua `ISimGateway`), `AsteriskSchedulerDispatchGateway.DispatchAsync` MEDIUM, `PostgresEligibilityRepository.PersistAsync` và `InternalAdminApiService.EvaluateEligibilityAsync` LOW; đã báo Toàn |
| Đột biến | `K-58`: 3 bị bắt (đóng không giới hạn; bắt lỗi thu hẹp, hai test). `K-62`: 3/3. `K-60`: 2/2. `K-61`: 10/10 (selftest và promtool). `K-59`: thêm một byte vào IR-06 làm cả bốn self-test đỏ |
| Test trên bản sao | `K-58`/`K-62`: unit `927/928` (ca đỏ là bảng traceability chưa sinh lại), telephony `118/118`, `MockTelephonyPersistenceTests` `20/20`, chaos `8/8`. `K-60`: integration liên quan `164/164`. `K-61`: promtool `check rules` 10 luật và 6 file thử đạt |
| Test trên cây chính sau khi land | `main@59a76487` cộng lô: unit `928/928`, contract `24/24`, integration `457/457`, chaos `8/8`, tổng `1417/1417`, build `0` cảnh báo; HEAD không đổi suốt lượt |
| Test mới | 6 dòng traceability mới: `IT-ELIG-EXPIRED-01` (hai fact), `UT-AST-DISPOSE-01`, `UT-AST-DISPOSE-02`, `UT-AST-UNAVAILABLE-01`, `UT-AST-UNAVAILABLE-02`; `IT-SLO-CAPACITY-04` là test sẵn có, chạy promtool trên file thử có thêm năm ca; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`954` → `960`) |
| Gate sweep | `GATE_SWEEP_PASS 46/46 run, 26 skipped by manifest` (Git Bash, trên cây đã land, HEAD không đổi suốt lượt); trong đó `capacity-selftest.mjs` (`CAPACITY_SELFTEST_PASS_UNCALIBRATED`), bốn validator ghim IR-06 (`W0178_SELFTEST_PASS`, `W0183_SELFTEST_PASS`, `W0187_OPTOUT_BUNDLE_SELFTEST_PASS`, `W0181_SELFTEST_PASS`), `contract-freeze-verifier.mjs`, `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 357 work item |
| Phạm vi | `gitnexus detect_changes` trước commit: 139 symbol trong 26 file đã theo dõi (gồm file kế hoạch đang sửa dở của phiên khác, không thuộc lô; tracker, sổ trạng thái, bảng traceability, IR-06, IR-07, spec, validator và template), 13 luồng bị ảnh hưởng, rủi ro `high`: 10 là vòng dispatch Asterisk (`DispatchAsync`), 3 là `EvaluateEligibilityAsync`; đúng với impact đã báo trước khi sửa, và đều nằm trong lượt test trên cây chính. Các method test và helper cạnh chỗ chèn dòng hiện là "touched" dù không đổi |

## Chưa làm

`Q-37` chờ chief; `K-63`, `K-64`, `K-65` như trên. Báo Module 3 về IR-06 và IR-07 là việc của Toàn.
