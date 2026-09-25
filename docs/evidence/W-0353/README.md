# W-0353 — Làm nốt phần việc IVR còn lại

Ngày 24/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** 15 việc đã đóng theo chỉ thị; W-0037 và W-0347 chờ lượt soak bốn giờ.

## Vì sao có việc này

Sau [W-0203](../W-0203/README.md) và [W-0352](../W-0352/README.md), Toàn trả lời: *“tiếp đi cho full luôn”*. Lúc đó
còn 20 việc mở phía IVR (14 `TESTS_PASS`, 5 `EVIDENCE_SUBMITTED`, 1 `CODE_DONE`). 9 việc `BLOCKED_EXTERNAL` và 2 việc
`DEFERRED_TARGET` chờ bên ngoài, nên không thuộc phạm vi này.

Việc này làm phần còn thiếu phía IVR của từng việc (test, code, tài liệu), rồi đóng những việc đã xong theo tiêu chí
của [phiếu W-0348](../W-0348/README.md): C1, C2 và C4 đạt, và phần còn lại chỉ gồm việc chờ bên ngoài, giới hạn đã
ghi, hoặc câu hỏi mà một việc sau đã trả lời.

## Phép kiểm cho gate chỉ chạy trong lượt mở rộng

Khai báo `gates` của một hồ sơ trước đây chỉ nhận gate của full sweep, nên ba việc mà phép kiểm là chính security scan
(W-0078, W-0168, W-0205) không khai được gì. Từ việc này, một gate mà full sweep bỏ qua nhưng có lượt mở rộng
(`extended` trong manifest) được xét qua lượt mở rộng của cùng gói bằng chứng. Gói không có lượt đó thì việc ra
`CHƯA KIỂM`. Phép thử đột biến cho gate mở rộng luôn đạt thì self-test đỏ.

## Lượt chạy đã ghi thay cho test trong suite

Có phép kiểm không phải test trong suite và không phải gate: một lượt soak bốn giờ không chạy lại được trong mỗi lượt
collector. Khai báo nay có thêm `runs`: mỗi mục là `{testId, file, reason}`. `file` là một tệp JSON trong cùng hồ sơ,
ghi `testId` trùng và `verdict: "PASS"`. Luật của mục này:

- TestId phải được hồ sơ nhắc tới, và không được là TestId đang sống trong suite hay trong registry gate. Test có
  trong suite thì phải chạy trong suite, không được thay bằng một tệp.
- `reason` ít nhất 20 ký tự; `file` phải nằm trong thư mục hồ sơ, đuôi `.json`, không có `..`.
- Việc có mục `runs` luôn ra `XEM` ở C2, vì không script nào tự đánh giá được một lượt chạy đã ghi.

Self-test của danh sách (`deploy/ci/scripts/acceptance-batches.mjs --self-test`) có 108 phép kiểm C2, trong đó có các ca
từ chối: TestId đang sống, TestId không được nhắc, tệp không có, tệp ngoài hồ sơ, lý do ngắn, verdict `FAIL`, và tệp ghi
TestId khác.

## Việc làm theo từng mục

| Việc | Đã làm | Phép kiểm |
| --- | --- | --- |
| W-0036 | Test cho bảy ID §8 của P5-2 | `CT-OAS-01..03`, `CT-TASK-01..04` (xem [hồ sơ W-0036](../W-0036/acceptance-addendum.md)) |
| W-0094 | Test cho ba thay đổi chưa có test | `UT-FAKE-HISTORY-10`, `UT-WORKER-RETENTION-05`, `UT-WORKER-CALLBACK-SCOPE-06` |
| W-0303 | Test ghim cổng Asterisk đang đóng | `UT-TRUNK-DI-05`, `UT-TRUNK-DI-06` |
| W-0249 | Seed test preflight bằng SQL ghim schema | `IT-RESULT-CONTRACT-PREFLIGHT-20` |
| W-0053 | `deploy/dr/rebuild-standby.sh` dựng lại standby đồng bộ sau failover; sửa phép kiểm sync của DG-DR-03 | `DG-DR-REBUILD-05` |
| W-0041 | Gauge và alert cho hàng đợi quay số bị dồn; panel backlog và panel burn-rate D-04 | `UT-OBS-METRIC-03`, `IT-SLO-BACKLOG-05`, `IT-OBS-BACKLOG-15` |
| W-0055 | Fact job so với nguồn thay vì tin `closed_at` bất biến; alert trên reconcile; sửa lỗi cộng dồn dòng bị từ chối | `BI-ALERT-05`, `BI-DRIFT-06`, `BI-QUALITY-04`, `IT-SLO-ANALYTICS-06` |
| W-0225 | Hai bản luật duyệt phát hành có chung 56 ca | `tts-provenance-gate.mjs` |
| W-0078, W-0168, W-0205 | Khai `security-scan.sh`, xét qua lượt mở rộng | `security-scan.sh` |
| W-0037 | Chế độ soak cho `tools/dev/Invoke-LocalMockE2E.mjs` | `PT-SOAK-02` qua `runs` |
| W-0169, W-0297 | Sinh lại bản đồ tài liệu bằng mapper chính thức; sửa 18 link | — |

Mỗi test mới đều được thử đột biến: sửa tạm mã nguồn để làm hỏng đúng hành vi test giữ, xác nhận test đỏ, rồi khôi
phục. Chi tiết ở hồ sơ của từng việc.

## Ghi chú quy trình

Phần W-0036, W-0041/W-0055, W-0053, W-0225 và bản đồ tài liệu do năm agent làm song song trong các bản clone riêng
(push bị tắt), mỗi agent nộp một patch. Patch được đọc lại, áp bằng `git apply`, rồi build và chạy test trên cây chính.
Agent W-0053 đã tự kéo image `koalaman/shellcheck:stable` về Docker để chạy shellcheck, rồi xoá đi. Image này không
nằm trong danh sách Toàn đã duyệt tải; ghi ở đây để Toàn biết.

## Kiểm chứng

Gói bằng chứng của collector tại `b92942e` (`e95ba64` là phần làm thêm, `b92942e` là bản đồ tài liệu), trên
cây sạch, có lượt gate mở rộng:

| Phần | Kết quả |
| --- | --- |
| Test | contract 24/24, unit 796/796, chaos 8/8, integration 405/405; 0 đỏ |
| Full sweep | `GATE_SWEEP_PASS 43/43` |
| Lượt mở rộng | `EXTENDED_SWEEP_PASS 5/5`: hai lượt image, K8s, security scan, oasdiff |

Collector phải chạy bốn lần mới ra gói bằng chứng. Ba lượt đầu không hỏng vì code:

- Lượt 1: Docker Desktop bị tắt lúc 14:59 ngày 24/09 để nén ổ dữ liệu của Docker (việc dọn đĩa C, Toàn đồng ý). Test
  và full sweep đạt; ba gate mở rộng không kết nối được Docker.
- Lượt 2: gate image thứ hai đứt mạng ("unexpected EOF") khi đang tải lại `grafana/otel-lgtm:0.30.0`, vì việc dọn đĩa
  đã xoá image. Image được kéo riêng rồi chạy lại.
- Lượt 3: treo ở gate k8s sau `IT-K8S-LINT-01` cho tới khi Docker khởi động lại sáng 25/09.

Collector từ chối ghi gói bằng chứng khi lượt mở rộng đỏ, nên không lượt hỏng nào được dùng làm bằng chứng.

Hai lượt soak thử (3 và 5 phút) chạy trước để kiểm công cụ, không phải bằng chứng PT-SOAK-02. Lượt đầu cho thấy phép đo
hạn chót ban đầu sai: nó đếm cả kết quả `IVR_CONFIRMATION_WINDOW_EXPIRED` mà scheduler ghi khi đóng task hết hạn, tức
hạn chót được thi hành chứ không bị lỡ. Phép đo được tách làm bốn trước lượt thật (chi tiết ở hồ sơ W-0037).

## Kết quả

Theo [danh sách tại `b92942e`](../../release/acceptance-batches.md), 15 việc lên `XEM` và được chuyển sang `ACCEPTED`
ở activity `A-1004`..`A-1018`; lý do từng việc trong [closeout.json](closeout.json). Tổng `ACCEPTED` nay là 288/341, Nấc 1 là 46/54.

| Việc | Từ |
| --- | --- |
| W-0011 | `TESTS_PASS` |
| W-0036 | `TESTS_PASS` |
| W-0041 | `TESTS_PASS` |
| W-0053 | `TESTS_PASS` |
| W-0055 | `TESTS_PASS` |
| W-0078 | `TESTS_PASS` |
| W-0094 | `TESTS_PASS` |
| W-0108 | `TESTS_PASS` |
| W-0168 | `TESTS_PASS` |
| W-0169 | `EVIDENCE_SUBMITTED` |
| W-0205 | `TESTS_PASS` |
| W-0225 | `TESTS_PASS` |
| W-0249 | `TESTS_PASS` |
| W-0297 | `EVIDENCE_SUBMITTED` |
| W-0303 | `TESTS_PASS` |

Giữ lại năm việc, cũng ghi lý do trong closeout.json: W-0037, W-0347, W-0121, W-0171, W-0335.

## Còn lại

- **W-0037 và W-0347:** chờ lượt soak `PT-SOAK-02` bốn giờ. Chạy sau khi đóng lượt này, trên máy không làm việc khác.
- **W-0121:** cần một pipeline hosted xanh. Pipeline `2877405205` có 2 job đỏ; project GitLab là private nên máy này
  không đọc được tên job. Toàn xem trên GitLab.
- **W-0171 vế (b):** taxonomy 7 nhãn technical-exception cần owner quyết: khoá `exception_type` thành enum bảy giá trị
  rồi ánh xạ mã gateway vào, hay nhận mã gateway làm giá trị chuẩn và bỏ bảy nhãn khỏi đặc tả.
- **W-0335:** chờ S5 và quyết định ngân sách cuối; S2 và mirror còn mở.
