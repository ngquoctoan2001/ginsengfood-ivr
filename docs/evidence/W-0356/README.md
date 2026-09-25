# W-0356 — Lô `L1` của kế hoạch khắc phục `25/09`: vệ sinh test

Ngày 25/09/2026 · Claude, trong lượt *"rà soát tiếp việc cần làm"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch khắc phục `25/09` của Toàn (`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`, lập trên `f3e26a6`) xếp lô `L1` là
việc agent làm được mà không cần ai quyết: bốn mục `K-02`…`K-05`. Hai phiên chia lô qua tin nhắn: phiên này `L1` và
`L3`, phiên lập kế hoạch `L2` và việc push. Mỗi khẳng định của kế hoạch đã được đọc lại trên code trước khi sửa.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-02` | Một TestId đang gắn cho ba test khác nhau. Test "gauge khớp đúng thứ claim sẽ lấy" giữ `IT-OBS-BACKLOG-15`, vì hồ sơ W-0041 và chú thích code trích nó theo nghĩa đó. Test tính giờ chờ từ lúc mở khung giờ nhận `IT-OBS-BACKLOG-16`. Test "hàng đợi tạm dừng và đơn bị thu hồi không tính là dồn", tức test canh vị từ `revoked_at`, nhận `IT-OBS-BACKLOG-17`. Chú thích ở `SchedulerQueueBacklog.cs` nay trỏ cả hai test giữ hai mệnh đề WHERE khớp nhau | `IT-OBS-BACKLOG-15`, `IT-OBS-BACKLOG-16`, `IT-OBS-BACKLOG-17` |
| `K-03` | Mười test lõi chưa có TestId được gắn ID, nối số thứ tự của từng họ; không ID nào đã dùng ở đâu khác | `UT-INTAKE-PROGRAMS-17`, `UT-INTAKE-IDEMPOTENCY-18`, `UT-INTAKE-IDEMPOTENCY-19`, `UT-INTAKE-RESTRICTION-20`, `UT-INTAKE-PII-21`, `UT-INTAKE-PII-22`, `UT-INTAKE-NOJOB-23`, `UT-INTAKE-AUDIT-24`, `UT-RESULT-TAXONOMY-12`, `UT-DOMAIN-PRIVACY-05` |
| `K-04` | Profile `LocalMockE2E` của API ước dung lượng theo mặc định (1 kênh, 60 giây một cuộc) trong khi worker cùng profile quay 8 kênh, 5 giây. Eligibility chạy trong API và dùng đúng hai giá trị này (`MockSchedulerCapacityService`), nên chép đúng hai khóa `MockChannelCount` và `ExpectedCallDurationSeconds`; các khóa vòng lặp của worker (bật loop, poll, lease) không chép vì API không chạy scheduler. File được sửa lúc soak còn chạy vì API nạp profile với `reloadOnChange: false` và đọc dung lượng qua `IOptions`, nên tiến trình đang chạy không đổi | `UT-CALLWINDOW-PARITY-02` |
| `K-05` | Chỗ thứ tư và thứ năm cùng lỗi `C15`: hai host test và hai script `tools/dev` giữ khung giờ mặc định, nên sau 21:08 giờ Việt Nam intake từ chối task và test đỏ. Helper `WholeDayCallingWindow` mở cả ngày cho hai host test, như `docker-compose.e2e.yml`; hai script đặt cùng bốn biến cho API và worker; test parity phủ thêm hai script. Chú thích cũ "20:52:30" ở `CallingWindowTests` sửa thành mốc đúng với khung 21:08 | `IT-API-MATRIX-38`, `IT-DEV-SEED-03`, `IT-DEV-SEED-04`, `UT-CALLWINDOW-PARITY-03` |

## Kiểm `K-05` mà không phải chờ tới đêm

Thay cho đồng hồ giả, cho helper một khung giờ **đang đóng đúng lúc chạy** (00:00–01:00): với hai host này, ban ngày
chạy trong khung đóng tương đương ban đêm chạy trong khung mặc định. Đúng ba test kế hoạch nêu đỏ,
`IT-API-MATRIX-38`, `IT-DEV-SEED-03` và `IT-DEV-SEED-04`; mở lại cả ngày thì cả ba xanh. Đưa một script về 21:08
thì `UT-CALLWINDOW-PARITY-03` đỏ; đưa API về 1 kênh thì `UT-CALLWINDOW-PARITY-02` đỏ. Mọi file được khôi phục bằng
bản sao byte, không bằng `git checkout`. Số liệu ở [mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Bốn bộ test trên cây làm việc, 12:42–12:52, trước khi phiên kia bắt đầu sửa file nguồn | unit `815/815`, contract `24/24`, integration `412/412`, chaos `8/8` |
| Bộ unit sau `K-04`, trên bản sao tách riêng gồm `HEAD` `c2435b9` cộng đúng các file của `W-0356` và `W-0357` | `815/815` |
| Traceability | sinh lại, `871` dòng |

Bản sao tách riêng là để phần phiên kia đang sửa dở trong cùng cây (`TaskIntakeService.cs`) không lẫn vào kết quả.
Gói bằng chứng của collector tại commit chạy sau khi cả hai phiên commit và cây sạch.
