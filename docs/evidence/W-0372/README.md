# W-0372 — `K-63`…`K-65` của kế hoạch khắc phục `25/09`: ba phát hiện của `W-0369`

Ngày 26/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp k63 k64 65"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

`W-0369` ghi ba phát hiện vào kế hoạch
[`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) thay vì sửa ngay:

- lớp HTTP của adapter ARI không có timeout, và mã HTTP lỗi được đọc lẫn lộn;
- panel trễ hạn cộng mọi lý do, và eligibility hỏng một phần không có cảnh báo nào;
- F05 và IR-06 còn nói sai về `IVR_NO_ANSWER_FINAL` và về callback của task dry-run.

Lô được viết trên ba bản sao `git archive` của `main@dd90dd15`, ba nhóm không chung file nào, mỗi nhóm một sub-agent. Phiên
lập kế hoạch duyệt từng diff, đóng dấu `W-0372` rồi land lên `main@977e05fa`. IR-06 và IR-07 được gộp ba chiều với `W-0371`
của phiên kia, lô sửa cùng hai file đó trong lúc lô này đang viết; hash IR-06 được ghim lại trên bản đã gộp.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-63` | Mỗi lệnh REST gửi ARI, và việc mở luồng sự kiện, chỉ chờ tối đa 10 giây (hằng `AriRequestTimeout`). Trước đó lệnh REST chờ 100 giây mặc định của `HttpClient`, còn luồng sự kiện không có hạn, nên một Asterisk im lặng (mất gói, hoặc bị treo) giữ mỗi dispatch tới 100 giây rồi lỗi rơi vào nhánh chung của dispatch gateway và bị tính vào SIM. Nay quá hạn là `ASTERISK_HTTP_TIMEOUT` (hoặc `ASTERISK_EVENT_STREAM_UNAVAILABLE` với luồng sự kiện), không nói gì về SIM, cùng lý do `K-62`; worker tự huỷ thì vẫn là huỷ. Lệnh quay số quá hạn kéo theo một `DELETE` kênh theo id IVR tự đặt, vì một Asterisk chậm mà còn sống có thể đã tạo kênh và đang đổ chuông máy khách. Mã HTTP lỗi đọc theo từng thao tác: 401/403 là `ASTERISK_HTTP_UNAUTHORIZED`, không nói gì về SIM; 500 khi quay số ("Allocation failed" của ARI) tính vào SIM; 404/409/412 khi phát là kênh đã kết thúc sau khi khách bắt máy, ghi như `ASTERISK_CHANNEL_ALREADY_ENDED`; mọi mã khác, kể cả 503, không nói gì về SIM. Trước đây mọi mã trừ 503 đều báo kênh lành và xoá chuỗi lỗi của SIM. Bảng đầy đủ ở spec SIM adapter | `UT-AST-HTTP-01`, `UT-AST-HTTP-02`, `UT-AST-HTTP-03`, `UT-AST-HTTP-04`, `UT-AST-HTTP-05` |
| `K-64` | Sweep hết hạn đếm job mà eligibility chưa xét (`PENDING_ELIGIBILITY`) dưới lý do riêng `ELIGIBILITY_NOT_EVALUATED_BEFORE_DEADLINE`, như `Q-22.2` đã làm cho giờ gọi; kết quả và callback gửi Module 3 không đổi. Cảnh báo `IvrConfirmationWindowsExpiringUnevaluated` (ticket, ngưỡng 0, `for: 5m`) đọc thẳng lý do này và thay `IvrConfirmationWindowsExpiringUndialled` của `K-61`: luật cũ nhận ra đơn chưa được xét qua hình dạng ("cửa sổ cứ hết mà không có cuộc nào được quay"), nên chỉ kêu khi vòng eligibility dừng hẳn, sau khoảng một giờ, và không bao giờ kêu khi vòng bỏ sót một phần. Theo promtool, luật mới kêu ở phút 21 khi vòng dừng ở phút 10, và ở phút 26 khi vòng bỏ sót một đơn trong mười. Panel trễ hạn tách theo lý do và chương trình; chỉ `NO_DISPATCH_BEFORE_DEADLINE` nói về mô hình dung lượng. `capacity-selftest.mjs` (CAP-ALERT-04) đọc lý do mới từ code và vẫn đỏ khi có lý do chưa ai quyết | `IT-SCH-UNEVALUATED-01`, `UT-DASH-REASON-06`, `IT-SLO-CAPACITY-04` (promtool, bảy ca mới hoặc sửa); `capacity-selftest.mjs` |
| `K-65` | F05: câu về `IVR_NO_ANSWER_FINAL` viết lại theo chốt `C7` (action chỉ là nhãn, Module 3 không làm theo và không chờ timeout); đoạn về kết quả hết cửa sổ thêm ngoại lệ `IVR_CAPACITY_EXCEPTION` (`K-52`) và thôi gọi action là "advisory" (đính chính `C22`). IR-06 §3.9: task `DRY_RUN` trên sandbox MOCK **có** callback, giống hệt callback thật, không field nào báo giả lập, nên chỉ được trỏ sandbox vào đầu nhận thử; task bị giữ ngay ở intake (`TASK_HELD_*`) không tạo job, không bao giờ có callback, và phải gửi lại với `Idempotency-Key` mới sau khi sửa nguyên nhân. IR-07 thêm đính chính cùng ý cho `M3-02`, vốn bảo Module 3 "chờ người xử lý phía IVR". Hash IR-06 ghim lại ở bốn validator và ba template | self-test của `d06-revalidation-evidence-validator.mjs`, `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`, `upstream-session-signoff-validator.mjs`; `contract-freeze-verifier.mjs` |

## Quyết định

- **Timeout của `K-63`:** 10 giây, là hằng chứ không phải cấu hình: validator chỉ nhận Asterisk cục bộ, và Asterisk cục bộ trả
  lời các lệnh này ngay; chờ đổ chuông là việc của luồng sự kiện, không nằm trong lệnh. Một dispatch gặp Asterisk im lặng lỗi
  sau 10 giây, hoặc 20 giây nếu phải gác kênh sau lệnh quay số.
- **500 khi quay số tính vào SIM:** cùng cách adapter đã tính mã huỷ cuộc gọi phía nhà mạng. Mọi SIM quay qua cùng một
  endpoint, nên một endpoint chết lần lượt tính vào mọi SIM quay tới nó; một Asterisk hết bộ nhớ cũng trả "Allocation failed"
  và cũng bị tính. Mỗi ô của bảng là một giá trị trong code, đổi lại được.
- **Job dry-run chưa được xét cũng nhận lý do mới:** trên MOCK, vòng eligibility xét job dry-run y như job lab, và sandbox là
  nơi diễn tập vòng này.
- **Mức cảnh báo mới:** ticket, như luật nó thay. Khi vòng eligibility dừng hẳn, mọi đơn sau đó mất cho tới khi có người xử
  lý; câu hỏi nâng lên page đã nêu ở `W-0369` vẫn là của owner.
- **`K-65` sửa thêm IR-07**, ngoài hai file kế hoạch nêu, vì `M3-02` bảo Module 3 chờ phía IVR xử lý task `TASK_HELD_*`, trong
  khi phía IVR không còn gì để xử lý: chờ là chờ mãi.

## Hành vi đổi

- **Adapter ARI:** Asterisk im lặng làm dispatch lỗi sau 10 giây, không còn 100 giây, và không tính vào SIM. Mã HTTP lỗi không
  còn xoá chuỗi lỗi của SIM.
- **Giám sát:** `ivr_missed_deadline_total` có lý do thứ tư; `IvrConfirmationWindowsExpiringUndialled` được thay bằng
  `IvrConfirmationWindowsExpiringUnevaluated`, runbook `docs/slo.md` §9a viết lại; panel tách theo lý do.
- **Đính chính `W-0369`:** câu "cảnh báo mới kêu ở mọi nơi có task vào mà vòng eligibility tắt" chỉ đúng ở nơi scheduler chạy.
  Chart Helm không bật scheduler lẫn vòng eligibility (`values.yaml`, `worker.eligibilityPolling.enabled: false`, không file
  values nào ghi đè), và sweep chỉ chạy trong scheduler, nên theo mặc định không luật nào trong hai luật kêu.
- **Tài liệu gửi Module 3:** IR-06 §3.9 và IR-07 đổi; Module 3 cần được báo. Phiên này không gửi gì.

## Phát hiện mới

Ghi vào kế hoạch, không sửa ở đây:

- **`K-66`:** tài liệu gửi Module 3 còn lệch ở IR-08 (dòng 111 lặp lỗi `K-59` đã sửa ở IR-06; §6 không nói `task_id` không có
  kịch bản riêng thì về `IVR_CONFIRMED`), IR-06 §3.11 (decision lệch chỉ được nhận là `TASK_ACCEPTED_CALL_JOB_CREATED` ở LAB)
  và IR-07 (nói hai trong năm decision `200` không tạo cuộc gọi; thật ra là ba).
- **`K-67`:** `attempt_policy_version` dài hơn 120 ký tự làm intake trả `500`, và Module 3 sẽ gửi lại mãi.
- **`K-68`:** sandbox MOCK đóng job dry-run không kịp quay bằng kết quả hết cửa sổ, production đóng bằng
  `IVR_CAPACITY_EXCEPTION`; Module 3 không thử được callback thiếu dung lượng trên sandbox.
- **`K-69`:** lệnh gác máy lỗi sau khi đã có kết quả làm lượt gọi bị ghi là lỗi kỹ thuật, kể cả khi khách đã bấm `1`; khách bị
  gọi lại.
- **`K-70`:** kênh mồ côi: lệnh `DELETE` sau quay số quá hạn có thể tới trước lệnh quay số chậm, và worker tự huỷ giữa lúc quay
  thì không gác kênh; khách bắt máy sẽ gặp một cuộc gọi im lặng.
- **`K-71`:** nhánh chung của dispatch gateway vẫn tính vào SIM các lỗi không phải của SIM (database, worker đang tắt), và một
  trả lời lỗi có charset lạ rơi vào nhánh "policy hoặc token".
- **`K-72`:** khách bị tính lượt cho lỗi phía IVR: luồng sự kiện im lặng sau khi kết nối không bị phát hiện, và lần phát hỏng về
  sau không được ghi nhận.
- **`K-73`:** các test double coi `ASTERISK_PLAYBACK_FAILED` là lỗi âm thanh, adapter thật báo lỗi mạng.
- **`K-74`:** luật ngưỡng 0 trên counter bỏ sót lần tăng đầu tiên sau mỗi lần worker khởi động lại.
- **`K-75`:** `docs/operations/production-dial-path.md` và mô tả metric trong `IvrTelemetry.cs` còn đọc mọi lần trễ hạn là
  thiếu dung lượng.
- **`K-76`:** sweep không đọc `revoked_at`; khi có endpoint thu hồi (`Q-20`), job bị thu hồi sẽ đóng sai lý do.
- **`K-77`:** `generate-test-traceability.mjs` chỉ tìm tên method trong 12 dòng sau TestId, nên theory có bảng `InlineData`
  dài ra dòng trống tên method. `UT-AST-HTTP-04` gặp lỗi này lúc land và đã được xử lý bằng cách đặt `[Trait]` sát khai
  báo method; ba dòng cũ của lô khác vẫn trống.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi, trên cây chính sau khi land |
| Impact | Trước khi sửa: `AsteriskAriSimGateway.SendAsync`, `EnsureSuccessAsync` và `HangupAsync` HIGH (35 symbol, hai vòng dispatch); `DialAsync` MEDIUM; `PlayAsync`, `CheckHealthAsync`, `EnsureEventPumpAsync` LOW; `PostgresSchedulerStore.CloseMissedDeadlinesAsync` HIGH (35 symbol, 13 trực tiếp), lớp `PostgresSchedulerStore` CRITICAL ở mức lớp (chỉ thêm một hằng); đã báo Toàn |
| Đột biến | `K-63`: 18/18 bị bắt. `K-64`: 16/16. `K-65`: thêm một byte vào IR-06 làm cả bốn self-test đỏ |
| Test trên bản sao | `K-63`: unit Telephony `150/150`, chaos `8/8`, unit `959/960` (ca đỏ là bảng traceability chưa sinh lại). `K-64`: integration liên quan `124/124`, unit `928/929` (cùng lý do), promtool `check rules` 10 luật và 6 file thử đạt. `K-65`: 7 self-test đạt |
| Test trên cây chính sau khi land | `main@977e05fa` cộng lô: unit `980/980`, contract `24/24`, integration `464/464`, chaos `8/8`, tổng `1476/1476`, build `0` cảnh báo; HEAD không đổi suốt lượt |
| Test mới | 7 dòng traceability mới: `UT-AST-HTTP-01`, `UT-AST-HTTP-02`, `UT-AST-HTTP-03`, `UT-AST-HTTP-04`, `UT-AST-HTTP-05`, `IT-SCH-UNEVALUATED-01`, `UT-DASH-REASON-06`; `IT-SLO-CAPACITY-04` là test sẵn có, chạy promtool trên file thử đã sửa; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`972` → `979`). Lúc land, `[Trait]` của `UT-AST-HTTP-04` được dời xuống sát khai báo method, vì bộ sinh bảng chỉ tìm tên method trong 12 dòng (`K-77`) |
| Gate sweep | `GATE_SWEEP_PASS 46/46 run, 26 skipped by manifest` (Git Bash, trên cây đã land, HEAD không đổi suốt lượt); trong đó `capacity-selftest.mjs` (`CAPACITY_SELFTEST_PASS_UNCALIBRATED`), bốn validator ghim IR-06 (`W0178_SELFTEST_PASS`, `W0183_SELFTEST_PASS`, `W0187_OPTOUT_BUNDLE_SELFTEST_PASS`, `W0181_SELFTEST_PASS`), `contract-freeze-verifier.mjs`, `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 360 work item |
| Phạm vi | `gitnexus detect_changes` trước commit, sau khi reindex: 182 symbol trong 29 file đã theo dõi (gồm file kế hoạch đang sửa dở của phiên khác, `CLAUDE.md` và `AGENTS.md` của lần reindex, tracker, sổ trạng thái, bảng traceability, IR-06, IR-07, spec, validator và template). Symbol đổi trong code là các method của `AsteriskAriSimGateway` và `PostgresSchedulerStore.CloseMissedDeadlinesAsync`, đúng phạm vi. Công cụ báo 0 luồng và rủi ro `low`, thấp hơn impact đo trước khi sửa (HIGH, hai vòng dispatch); lấy mức HIGH, và hai vòng đó đều nằm trong lượt test trên cây chính |

## Chưa làm

`K-66`…`K-77` như trên. Báo Module 3 về IR-06 và IR-07 là việc của Toàn; `Q-37` vẫn chờ chief.
