# W-0331 — Retry khi VieNeu bận, giữ nguyên timeout

Ngày `2026-09-21`. **REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**
Ba giọng và câu ghép vẫn OWNER_ACCEPTED; không yêu cầu nghe lại.
Kết quả máy local không đóng S5. Metadata/checksum: [local-results.json](local-results.json).

## Thay đổi và phạm vi

`ConfigurableExternalTtsProvider.RequestAudioAsync` trước đây ném lỗi ngay khi nhận503.
Nếu inference của request vừa timeout vẫn chạy, lần retry kỹ thuật của worker có thể
bị tiêu ngay vì sidecar chưa có chỗ. Nay client chờ 250ms, 500ms, rồi tối đa1000ms mỗi
lần; tối đa8 retry (9 HTTP request). Chỉ retry503. Mọi request, khoảng chờ và đọc body
dùng chung một deadline; caller cancellation vẫn dừng ngay. Không ghi body lỗi vào log.

Không đổi timeout production5s/lab10s, concurrency1, model, giọng, image TTS,
normalization, số lượt khách, schema hoặc chính sách gọi. Retry không làm inference nhanh
hơn và không hủy được inference đã chạy khi client ngắt kết nối.

Impact trước sửa: `RequestAudioAsync` LOW, 1 caller trực tiếp/11 symbol bị ảnh hưởng,
0 flow được index. Test mới và harness Python chưa có trong index; kiểm scope bằng source.
Detect-changes sau sửa là advisory trên checkout dùng chung, không chứng nhận toàn bộ WIP.

## Bằng chứng tách khỏi thay đổi task khác

Baseline `main@f2d35ce`. Candidate được xuất bằng `git archive` từ đúng baseline và chỉ
chồng một file production nêu trên cùng `TtsBusyRetryTests.cs`; traceability được sinh
lại trong candidate. Không tạo branch/worktree. Bản build đầu từ checkout dùng chung có
DSAR/audit WIP nên **không triển khai**, không dùng làm nguồn chứng nhận cuối.

Worker đã triển khai lab có image prefix `24409b682e9f`; TTS giữ prefix `79e9106ec140`.
Hash đầy đủ nằm trong JSON và `.artifacts/W-0331/candidate-bindings.json`.
Đây là evidence local của baseline cộng overlay, không phải clean-commit/hosted-CI proof.

| Kiểm chứng | Kết quả |
| --- | --- |
| Regression trước vá | 4 fail, 3 pass |
| Candidate sau vá: speech, normalization, traceability | 221/221 PASS; gồm 7 ca mới/5 TestId |
| Model thật, cố tình chiếm capacity rồi gọi provider C# | 5 lần503 →200; 4685ms; đúng1 file audio |
| Model thật, cùng tình huống, timeout500ms | Dừng505ms; 2 lần503; không có file audio |
| Audio probe | Hash khớp PCM đã duyệt, không phát tiếng |
| Worker image archive, scanner ghim, network none | 0 HIGH/0 CRITICAL; database CVE ngày20/09 |
| Preflight sau cập nhật worker | Readiness200; ghi/đọc media PASS; Asterisk không được ghi |
| Một cuộc warm North/MultiItem/phím1 | PASS: IVR_CONFIRMED; bảy media đúng; RTP1104 gói; khôi phục SIP |

Test kiểm cùng deadline cả đọc body, caller cancellation, request/body giống nhau,
không retry400/429/500, không echo body lỗi, và trần9 request.
Lượt test trên checkout dùng chung từng lỗi traceability khi task khác đang thêm TestId;
candidate riêng mới là kết quả221/221 ở trên. Không nhận toàn bộ shared checkout đã xanh.
Probe candidate lượt đầu hết thời gian chờ model khởi động; lượt sau đợi readiness trước
khi đo. Thay thời gian chờ công cụ không thay timeout xử lý đơn.

## Lab và lỗi còn giữ nguyên bằng chứng

Lượt smoke đầu sau khởi động lại gặp `TTS_TIMEOUT`10s trước dial, `counted=false`.
Harness cũ dừng ở kết quả kỹ thuật đầu tiên và khôi phục SIP quá sớm; attempt kế tiếp
thành `ASTERISK_DIAL_FAILED`, cũng không tính lượt khách. Không quy lỗi dial này cho TTS.
Job được giữ lại, không xóa/sửa lịch sử. Kết quả này **không PASS**.

Đã sửa `deploy/lab/run-headless-dtmf.py` giữ peer/route sau khi script intake trả về,
đợi job kết thúc hoặc `HELD_ADMIN_REVIEW` trước khi khôi phục. Lượt smoke warm tiếp theo
kiểm North/MultiItem/phím1; kết quả cụ thể, bảy media/hash, RTP, trace và checksum khôi phục
nằm trong JSON. Cuộc warm không phát sinh retry; không nhận nó là bằng chứng runtime cho
mọi nhánh retry của harness. Không thay sáu ca DTMF và no-input đã xác minh ở W-0329 bằng
một smoke này. Sau chạy: 0 active call/channel; peer riêng đã xóa; khách thật vẫn tắt.

Timeout10s vẫn tái hiện dù retry503 đã sửa. Một ca phục hồi trong4685ms không chứng minh
mọi đơn hoặc máy S5 đều đáp ứng5s/10s. Không nâng timeout để biến ca lỗi thành PASS.

## Việc tiếp theo

1. [W-0333 đã nhận và đối chiếu raw S5](../W-0333/s5-findings.md) từ vps61 với quota
   2 CPU/4 GiB. Sau disconnect, model còn bận 8871ms, dài hơn tổng backoff 6750ms ở trên.
   Cần tái hiện bằng client worker, kiểm đủ đơn/cold-warm/tải kéo dài và cách điều phối.
2. Dùng số đo S5 quyết định ngân sách tạo tiếng trước dial, cách giới hạn số request được
   nhận và cơ chế làm nóng model. Chưa có kết luận timeout production5s đạt.
3. Chốt mirror nội bộ và S2 bản quyền model/codec. Quét lại image sau mọi thay đổi image;
   không dùng scan worker để thay scan TTS hoặc ngược lại. Khách thật tiếp tục tắt.
