# W-0351 — Dọn các việc trích test UI đã gỡ hoặc ID không còn

Ngày 24/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: ứng viên đang được kiểm; lượt collector chạy ở bước tiếp theo.**

## Vì sao có việc này

Sau [W-0350](../W-0350/README.md), 29 việc vẫn trượt C2 vì trích TestId không còn chạy. Toàn hỏi: *“29 việc này
cần quyết gì không? ko thì clean đi”*. 11 việc trong số đó không cần quyết định mới:

- **8 việc trích test của admin-ui**, mà owner đã cho gỡ từ 05/09 (W-0253). W-0039 là việc UI thuần, nên khai
  báo đề nghị `CANCELLED`, giống W-0027 và W-0097. Bảy việc còn lại có phần backend với test đang xanh; chỉ phần
  test UI được retire.
- **3 việc trích ID không còn**: W-0115 ghi khoảng `01..04` nhưng mã `04` chưa từng tồn tại; W-0315 nhắc các
  test do chính nó gỡ khi owner chốt TTS chỉ VieNeu; W-0318 dùng ID làm ví dụ cho luật của bộ kiểm.

18 việc còn lại (17 việc cộng W-0116) trích test của gate cần Docker, K8s hoặc oasdiff. Máy này thiếu helm,
kubeconform và oasdiff, nên cách kiểm các việc đó chờ Toàn quyết.

## C2 nhận thêm gì

- **`mentions`**: ID mà hồ sơ nhắc nhưng không nhận là claim, như ví dụ, phát hiện về việc khác hay test do chính
  việc đó gỡ. Mỗi ID cần một lý do. ID đang có test sống hoặc gate sống thì không nhắc được, nên không thể giấu
  một test đỏ.
- **Retire kèm `surfaceRemoved`**: bỏ ID của một bề mặt đã gỡ theo quyết định đã ghim (ở đây là admin-ui), không
  cần test thay thế. Quyết định vẫn phải qua kiểm hash và trích đoạn như mọi retire khác.

Cả hai luôn ra `XEM`, và không làm một hồ sơ hết claim thành đạt: phần còn lại vẫn phải có test xanh, gate đạt,
hoặc khai là tài liệu. Self-test của danh sách (`CT-ACCEPTANCE-C2-01`) có thêm 8 phép kiểm, tổng 84. Hai phép thử
đột biến đều làm self-test đỏ: cho nhắc một ID đang sống, và bỏ luật `XEM` của ghi chú.

## Khai báo

| Việc | Khai báo |
| --- | --- |
| W-0039 | UI thuần, đề nghị `CANCELLED` theo quyết định gỡ admin-ui |
| W-0099, W-0100, W-0101, W-0110, W-0112, W-0113 | Retire phần test UI; phần backend giữ test đang có |
| W-0322 | Nhắc bốn ID của các việc nó rà: hai test UI đã gỡ, hai ID mà W-0019 đã retire |
| W-0115 | Nhắc mã `04` chưa từng tồn tại |
| W-0315 | Nhắc bảy test do chính nó gỡ |
| W-0318 | Nhắc ba ID ví dụ và một phát hiện về W-0115; gate `acceptance-batches.mjs` mà nó dựng |
| W-0348 | Việc tài liệu: phiếu duyệt một lượt |
| W-0121, W-0169, W-0171 | Dựng hồ sơ cho ba việc `CODE_DONE` chưa có gói; W-0169 kèm gate `docs-selftest.mjs`; chuyển `EVIDENCE_SUBMITTED` |

Lý do từng ID nằm trong `acceptance-tests.json` của mỗi việc.

## Kiểm chứng

- Self-test của danh sách: 84 phép kiểm C2. Self-test bằng chứng: 47 phép kiểm.
- Chế độ worktree: 14 việc có khai báo hợp lệ, chờ kết quả collector; W-0039 ra đúng dạng đề nghị `CANCELLED`.
- PII scan, docs-selftest và gate-status đạt.

Chỉ Toàn chuyển các việc này sang `ACCEPTED`.
