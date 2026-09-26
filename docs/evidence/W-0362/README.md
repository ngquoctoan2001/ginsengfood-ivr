# W-0362 — Lô `L7` của kế hoạch khắc phục `25/09`: lối quay số

Ngày 26/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp tục l7"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch [`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) xếp lô `L7`
là tám lỗi trên lối quay số, và phần lớn là điều kiện trước `SIP-04` (`CB-13`): gateway viết cứng chế độ, lease bị bỏ
treo khi không nạp được ngữ cảnh, gate chỉ hỏi một lần trước bước chuẩn bị giọng, chuỗi lỗi SIM bị xoá bởi lỗi không
liên quan tới SIM, store ghi thẳng dòng audit không qua guard, mất luồng sự kiện ARI thì cuộc gọi bị ghi là lỗi của
khách, job bị giữ vì hết dung lượng không bao giờ đóng, và tên loại lỗi không tới OTLP.

Lô được viết trên các bản sao `git archive` của `main@20f05ed0`, ngoài cây chính. Bốn nhóm không chung file chạy song
song: phiên lập kế hoạch làm `K-42…K-45`, ba sub-agent làm `K-47`, `K-52`, và `K-46` cùng `K-53`, mỗi nhóm trên bản sao
riêng, chỉ chạy test của mình. Phiên lập kế hoạch duyệt từng diff, gộp vào một bản sao rồi chạy toàn bộ suite.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-42` | Gateway MOCK lấy chế độ từ deployment (`SchedulerExecutionContext.ToDomainMode()`) như gateway Asterisk đã làm từ `W-0354`; test kiến trúc cấm viết chế độ vào bất kỳ `*DispatchGateway.cs` nào. `SpeechSynthesisService` coi là production khi tham số **hoặc** chế độ deployment là `PRODUCTION_REAL`, nên một gateway truyền sai chế độ không bỏ qua được bản ghi phê duyệt whitelist. Kịch bản chỉ được duyệt cho MOCK bị từ chối khi lab dispatch, qua renderer và registry thật, không quay số. Việc tuỳ chọn gom ba bản đổi chuỗi sang enum không làm: hai bản ở tầng API so khớp chính xác, bản của scheduler không phân biệt hoa thường, gom lại là đổi hành vi | `ARCH-DISPATCH-MODE-01`, `UT-TTS-WHITELIST-08`, `UT-AST-SCRIPT-01` |
| `K-43` | `LoadAsync` nằm trong `try` ở cả hai gateway; lỗi nạp ngữ cảnh (task bị thu hồi, dữ liệu lưu không đọc được, DB lỗi) thành `DispatchContextUnavailableException`, mã `DISPATCH_CONTEXT_UNAVAILABLE`, kênh lành. Trước đây lease bị bỏ treo: kênh `RESERVED` tới khi lease hết hạn, rồi lease recovery cách ly kênh và đếm một lỗi `DT-04` cho một SIM chưa hề được dùng. Kết cục riêng cho task bị thu hồi vẫn là việc của `C13` | `UT-AST-LOAD-FAIL-01`, `IT-TEL-LOAD-FAIL-01` |
| `K-44` | Gateway Asterisk hỏi lại `DispatchGate` ngay trước `DialAsync`. Lần hỏi đầu nằm trước bước dựng và tổng hợp giọng, có thể kéo dài tới trần timeout TTS; kill switch bật trong khoảng đó nay chặn được chính cuộc gọi này. `docs/operations/production-dial-path.md` ghi hàng này là đã sửa | `UT-AST-GATE-03` |
| `K-45` | Kết cục lành chỉ xoá chuỗi lỗi SIM khi kênh đã thực sự mang một cuộc gọi (có session). Lỗi dừng trước khi quay (render, dữ liệu, token, gate, không nạp được ngữ cảnh) để nguyên chuỗi lỗi, không còn xoá mất hai lỗi SIM thật ngay trước đó. Test cũ về xoá chuỗi lỗi nay đi qua một session thật | `IT-TEL-HEALTH-UNTOUCHED-09`, `IT-TEL-HEALTH-RESET-08` (sửa dữ liệu vào) |
| `K-46` | `PersistenceInvariantValidator` kiểm mọi dòng `AuditLogEntity` được thêm, như `PostgresAuditLogger` kiểm một sự kiện: text của actor, action, đích, lý do, correlation, JSON dữ liệu, và tên từng khoá dữ liệu. Phép kiểm tìm ra một vi phạm có sẵn: dispatch store ghi khoá `recording`, tên mà `PiiGuard` từ chối, nên khoá đổi thành `recording_mode` (giá trị vẫn là `DISABLED`, bằng chứng `DT-05`). Không nới guard | `IT-DB-AUDIT-PII-10` |
| `K-47` | Mất luồng sự kiện ARI mà không có frame đóng nay kết thúc mọi cuộc gọi còn mở bằng `ASTERISK_EVENT_STREAM_LOST`, lỗi mạng, không tính lượt. Theo đó, `FailOpenCalls` bỏ qua cuộc đã có kết cục (Asterisk đã kết thúc, hoặc đã bắt được phím), để khách đã bấm 1 không bị gọi lại; và `HangupAsync` vẫn gửi lệnh DELETE cho cuộc bị đóng phía IVR vì mất luồng, vì kênh có thể vẫn còn khách | `UT-AST-EVENTS-11`, `UT-AST-EVENTS-12` |
| `K-52` | Sweep hết hạn đóng cả job `CAPACITY_HELD` khi hết cửa sổ: một kết quả `IVR_CAPACITY_EXCEPTION`, một callback, như nhánh capacity-miss; dùng lại incident mà eligibility đã mở, để một lần thiếu dung lượng không bị đếm hai lần vào bảng định cỡ SIM. `SchedulerQueueBacklog.cs` không đổi: file đó sao điều kiện của bước nhận job, không phải của sweep, và một job bị giữ không bao giờ là backlog | `IT-SCH-CAPACITY-HELD-01` |
| `K-53` | `ExceptionType` vào allowlist của `PiiSafeLogRecordProcessor`; giá trị vẫn qua bước lọc PII như mọi thuộc tính khác | `UT-OBS-LOG-14` |

## Hành vi đổi

- **Module 3:** một đơn bị giữ vì hết dung lượng nay nhận callback `IVR_CAPACITY_EXCEPTION` khi hết cửa sổ, thay vì
  không bao giờ nhận gì. Đơn đó cũng bật cảnh báo trễ hạn, vào bộ đếm trễ hạn, và rời khỏi số đếm "bị chặn" trên
  dashboard.
- **Task bị thu hồi lúc dispatch:** ghi ngay là lỗi kỹ thuật không tính lượt `DISPATCH_CONTEXT_UNAVAILABLE`, kênh trả
  về ngay, thay cho lease recovery và cách ly kênh.
- **Audit:** dòng audit của dispatch dùng khoá `recording_mode` thay cho `recording`. Ai đọc bằng chứng `DT-05` từ
  audit phải đọc khoá mới; evidence cũ của `W-0021` mô tả khoá cũ, đúng với lúc đó.
- **Kill switch** nay chặn cả cuộc gọi đang chuẩn bị giọng.
- **Mất luồng ARI:** cuộc gọi còn mở kết thúc ngay là lỗi mạng không tính lượt và được cúp máy; cuộc đã có kết cục giữ
  nguyên kết cục đó.

## Phát hiện mới

Ghi vào kế hoạch, không sửa ở đây:

- **`K-54`:** eligibility còn một kiểu giữ khác, `TASK_HELD_ADMIN_REVIEW` với `eligible=false`, mà sweep cũng không
  bao giờ đóng; và một job còn chờ eligibility khi cửa sổ đã hết cũng không bao giờ đóng.
- **`K-55`:** phép kiểm text trên JSON audit bỏ sót số dạng `+84` và dấu địa chỉ tiếng Việt, vì bộ serialize mặc định
  escape ký tự `+` và chữ có dấu thành `\u…`. Áp cho cả `PostgresAuditLogger` lẫn phép kiểm mới; sửa bằng cách lọc
  giá trị đã giải mã, và có thể bắt đầu từ chối chữ tiếng Việt đang qua được, nên tách riêng.
- **`K-56`:** ARI: một sự kiện sai kiểu làm chết vòng nhận sự kiện (`ProcessEvent` chỉ bắt `JsonException`); mất luồng
  giữa lúc quay và lúc phát thì bị ghi là `ASTERISK_CHANNEL_ALREADY_ENDED`, không nêu tên việc mất luồng.
- Hai tài liệu có số dòng cũ trỏ vào gateway nhưng bị ghim hash nên không sửa ở lô này: `docs/lab/one-sim-lab-plan.md`
  (validator B3) và `R-00-voice-gateway-rfq.md` (bốn chỗ ghim). Số dòng của `specs/functional/06-technical-exception-capacity.md`
  đã lệch từ trước và được sửa.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi, trên bản sao đã gộp |
| Impact | Trước khi sửa: `PostgresTelephonyDispatchStore.FinalizeAsync` CRITICAL, `PersistenceInvariantValidator.Validate` CRITICAL, `MockSchedulerDispatchGateway.DispatchAsync` HIGH, `PostgresSchedulerStore.CloseMissedDeadlinesAsync` HIGH, `AsteriskAriSimGateway.HangupAsync` HIGH; đã báo Toàn, và đã chạy lại chaos cùng toàn bộ test telephony, scheduler, persistence |
| Đột biến | `K-42…K-45`: 7/7 bị bắt. `K-47`: 5/5. `K-52`: 2/2. `K-46`, `K-53`: 3/3 |
| Test trên bản sao | unit `883/883`, contract `24/24`, chaos `8/8`, integration `433/434`. Ca đỏ duy nhất là `ApiBehaviorMatrixTests`, gọi `git ls-files` mà bản sao không có `.git` |
| Test trên cây chính sau khi land | `main@8efa3ee2` (W-0363, lô L8) cộng lô: unit `887/887`, contract `24/24`, integration `434/434`, chaos `8/8`, tổng `1353/1353`, build `0` cảnh báo; HEAD không đổi suốt lượt. Hai lô cùng sửa `SpeechSynthesisService.cs`: gộp ba chiều không xung đột, rồi bức tường sinh giọng lúc gọi của `K-49` dùng chung câu trả lời production của `K-42` thay vì tự tính lại |
| Test mới | 12 dòng traceability mới; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`910` → `922`) |
| Gate sweep | `GATE_SWEEP_PASS 45/45 run, 26 skipped by manifest` (Git Bash, trên cây đã land); trong đó `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 351 work item |
| Phạm vi | `gitnexus detect_changes` trước commit: 184 symbol trong 22 file đã theo dõi (file mới chưa vào index), 64 luồng bị ảnh hưởng, rủi ro `critical`: phần lớn là mọi lối lưu có thêm dòng audit, đi qua phép kiểm mới của `K-46`, cùng hai luồng dispatch; đúng với impact đã báo trước khi sửa, và cả hai đều nằm trong lượt test trên cây chính |

## Chưa làm

Kết cục riêng cho task bị thu hồi thuộc `C13`. `K-54`, `K-55`, `K-56` như trên.
