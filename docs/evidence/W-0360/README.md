# W-0360 — Lô `L5` của kế hoạch khắc phục `25/09`: vận hành, quan sát, cấu hình

Ngày 25/09/2026 · Claude, trong lượt *"rà soát tiếp việc cần làm"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch khắc phục `25/09` (`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`) xếp lô `L5` gồm sáu mục `K-31`…`K-36`.
Toàn giao lô này cho phiên đang chờ soak bốn giờ của `W-0037`, trong khi một phiên khác làm `L4` (`W-0359`).
Lô được làm trên một bản sao tách riêng của `6b881f4` để không lẫn với phần `L4` đang sửa trong cùng cây, rồi mới
đưa vào `main`. Mỗi khẳng định của kế hoạch được đọc lại trên code trước khi sửa.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-31` | **Làm một phần.** `CallbackDispatcher`: một exception bất ngờ từ transport vẫn được thử lại như trước, nhưng nay ghi một dòng log `Error` (EventId 2430, khối riêng của class) nêu **loại** exception và mã callback, không bao giờ nêu message. Trước đây dấu vết duy nhất là số lần thử leo tới `RETRY_EXHAUSTED`. `PostgresRuntimeSafetyHealth`: kiểm audit store mà ném lỗi vẫn trả "không", nhưng nay được đếm trên `ivr_fail_closed_total` với lý do `AUDIT_STORE_UNREADABLE`, nên sự cố của chính phép kiểm tách được khỏi một bảng audit thật sự vắng. **Chưa làm:** ba khối `catch` ở `RuntimeGateApprovals.cs` biến truy vấn hỏng thành "không" im lặng. Phân tích tác động báo `AnyLiveForEnvironmentAsync` ở mức CRITICAL và `VerifyAsync` ở mức HIGH, nên theo luật của repo phần này chờ Toàn duyệt; câu hỏi đã gửi ngày 25/09. Phần `TryHangupAsync` ở hai gateway thuộc lô `L4` (`W-0359`) | `UT-CALLBACK-TRANSPORT-LOGGED-18`, `UT-OBS-FAILCLOSED-11` |
| `K-32` | `Retry-After` của Sales vẫn được tôn trọng, không thử lại sớm hơn nó (`UT-CALLBACK-RETRY-AFTER-09B` giữ nguyên), nhưng bị chặn trần ở 5 phút. Trước đây một phản hồi 429 kèm `Retry-After` một ngày treo kết quả cuối một ngày, và không cấu hình nào phía IVR rút ngắn được. Kế hoạch đề xuất chặn ở `MaxRetryDelay`; không làm vậy vì đó là trần backoff (5 giây mặc định), chặn 429 ở đó là thử lại trước lúc máy chủ yêu cầu, điều `UT-CALLBACK-RETRY-AFTER-09B` cấm. 5 phút là mức chờ dài nhất giữa hai lần thử mà bộ validator chấp nhận cho `MaxRetryDelayMilliseconds`. Transport chỉ đọc header; trần đặt ở dispatcher, nơi duy nhất dùng giá trị đó | `UT-CALLBACK-RETRY-AFTER-17` |
| `K-33` | Heartbeat của worker đọc registry ngay lúc khởi động, trước khi các job host kịp đăng ký, nên mọi worker mở log bằng cảnh báo giả "loops have stopped ticking: (none registered)". Nay registry rỗng trong 30 giây đầu là "chưa đăng ký", không phải "đã dừng"; quá 30 giây mà vẫn rỗng thì vẫn cảnh báo, vì đó đúng là lỗi `WorkerLiveness` cố ý gọi là Stalled. Dòng "N loops turning" nay chỉ đếm vòng đang bật. Phần báo cáo tách thành `ReportOnce` để test gọi thẳng: từ .NET 10 `BackgroundService` chạy `ExecuteAsync` trên thread pool, nên `StartAsync` không còn bảo đảm lượt báo cáo đầu đã xảy ra | `UT-WORKER-HEARTBEAT-01`, `UT-WORKER-HEARTBEAT-02` |
| `K-34` | Chart Helm: ngoài `MOCK`, API nay được đặt `Ivr__Speech__Tts__Provider=UNSELECTED`; trước đây nó giữ `FAKE` bake sẵn trong ảnh và **từ chối khởi động**. Bật `worker.eligibilityPolling.enabled` (mặc định tắt, như appsettings của worker) nay render đủ ba thứ vòng eligibility cần mà chart trước đó không diễn đạt được: origin của API, token dịch vụ nội bộ lấy từ secret (trước chỉ pod API có), và luồng mạng worker → API cổng 8080 xuyên qua NetworkPolicy default-deny | `IT-K8S-TTS-08`, `IT-K8S-ELIG-09` (`k8s-selftest.mjs`) |
| `K-35` | `appsettings.Development.json` chứa năm token dev đã công khai, có cả tầng Danger, và bị đóng vào ảnh API. Không bản publish nào đọc nó (ảnh đặt `ASPNETCORE_ENVIRONMENT=Production`, mọi compose truyền token qua biến môi trường), nên nay nó không được copy vào thư mục publish. `image-selftest.mjs` tạo container từ ảnh API và kiểm file đó không có | `IT-IMG-BUILD-01` (`image-selftest.mjs`) |
| `K-36` | Circuit callback chỉ sống trong worker; `/health/ready` của API không chạy delivery nên luôn báo `not_configured`. Body `/healthz` của worker nay có mục `callback_circuit` (readiness, open, số lỗi tạm thời liên tiếp, mở tới lúc nào), `null` khi worker tắt delivery. Chỉ nằm trong body, không đổi status code: restart không làm Sales trả lời, và circuit giữ trong bộ nhớ nên restart sẽ đóng nó và đẩy lô kế tiếp thẳng vào sự cố | `UT-WORKER-HEALTH-CIRCUIT-01`, `UT-WORKER-HEALTH-CIRCUIT-02` |

Tài liệu đi kèm: `README.md` (đoạn về `/health/ready`) và `docs/operations/production-dial-path.md` §3 (mẫu body
`/healthz` và bảng tín hiệu) nói chỗ đọc trạng thái circuit.

Trong lúc làm, `UT-OBS-TRACE-02` lộ ra là test không ổn định: nó bắt mọi span callback của cả tiến trình, nên khi thứ tự
test đổi thì bắt nhầm span của test khác chạy song song. Test nay khóa bộ thu và chỉ giữ span mang đúng mã callback của
nó; ba lượt chạy liền đều xanh.

## Kiểm bằng cách làm hỏng

Mỗi phép kiểm gỡ đúng một phần của bản sửa trong bản sao, build lại, chạy test được nêu, rồi ghi lại byte gốc (không bao
giờ dùng `git checkout`). Test được nêu phải đỏ, test hàng xóm phải còn xanh, và lượt đối chứng sau khi khôi phục phải
xanh. Mười một phép làm hỏng đều đỏ đúng chỗ, bốn lượt đối chứng đều xanh: `K-31` hai phép, `K-32` một, `K-33` hai,
`K-34` ba, `K-35` một, `K-36` hai. `K-34` chạy phần render của `k8s-selftest.mjs`, dừng trước khi dựng cluster. `K-35`
build ảnh API bằng `deploy/docker/Dockerfile.api` như `image-selftest.mjs`, rồi tìm file trong một container tạo từ ảnh;
lượt đối chứng kiểm cả `appsettings.json` có mặt, để chắc phép dò nhìn đúng chỗ. Số liệu ở
[mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Bộ unit trong bản sao, build với analyzer là lỗi | `839/839` |
| `docs-selftest.mjs` trong bản sao | `API_DOCS_SELFTEST_PASS` |
| `k8s-selftest.mjs` và `image-selftest.mjs` | Phần render của `k8s-selftest.mjs` đạt ở lượt đối chứng; ảnh API build từ bản sao đạt phép kiểm `K-35`. Hai gate đầy đủ (dựng cluster, build cả hai ảnh, compose, quét) là gate mở rộng, chạy trong lượt collector `--extended` sau soak |
| `acceptance-batches.mjs --self-test`: hai TestId mới của `k8s-selftest.mjs` giữ đúng runner mở rộng của nó | `ACCEPTANCE_BATCHES_SELFTEST_PASS`, 20 TestId giữ runner mở rộng (trước lô: 18) |
| Traceability, `generate-test-traceability.mjs --check` | sinh lại, `882` dòng, `TEST_TRACEABILITY_CURRENT` |
| Trên `main` sau khi đưa vào (`b7a0761` cộng lô này), 17:44–17:54 | build 0 cảnh báo; unit `839/839`, contract `24/24`, integration `412/412`, chaos `8/8`; `gate-status.mjs`, `generate-test-traceability.mjs --check`, `docs-selftest.mjs`, `acceptance-batches.mjs --self-test` đạt |

Chaos có kịch bản Sales sập, giữ kết quả để thử lại có giới hạn: đó là năm luồng mà `detect_changes` báo lô này chạm tới,
qua `CallbackDispatcher.RunBatchAsync`.

Soak của `W-0037` đang chạy trên `b7a0761`, bản trước lô này, nên không đo gì của `L5`.
