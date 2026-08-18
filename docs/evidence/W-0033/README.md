# W-0033 — Evidence: V1 notification disabled boundary (`P4-5`)

Ngày: `2026-08-18` · Trạng thái đạt được: `DEFERRED_TARGET` giữ nguyên, phần no-op boundary đạt `TESTS_PASS`

## 1. Đã có sẵn gì, còn thiếu gì

`P4-5` §2 có 6 mục. Đối chiếu trước khi viết:

| Mục §2 | Hiện trạng |
| --- | --- |
| 1. Không có CRM notification client / customer-message template trong runtime V1 | **đã có** — `UT-ARCH-NO-CRM-EGRESS-06` (W-0031) chặn `SendSms`, `SendNotification`, `PublishNotification`… |
| 2. Nếu có interface thì bind sink vô hiệu | **không áp dụng** — không có interface nào để bind |
| 3. Config validation từ chối bật notification | **đã có** — `FeatureFlagGuardrails` immutable-off (P0-4) |
| 4. Test: mọi loại kết quả → 0 notification call/outbox row/egress | **thiếu** |
| 5. UI/docs hiển thị `V1_NOTIFICATION=DISABLED`, không phải "pending failure" | **thiếu hoàn toàn** |
| 6. Cập nhật W-0033 | slice này |

## 2. Lỗ hổng đáng kể nhất: console không nói gì cả

Grep toàn `admin-ui/src`: **không một chỗ nào** nhắc tới notification. Nghĩa là một operator nhìn console thấy khách không nhận được tin nhắn nào thì **không có cách nào phân biệt** đó là chính sách hay là lỗi gửi.

Đó đúng là thứ `P4-5` §2.5 cảnh báo: hiển thị `DISABLED`, **không** để nó trông như "pending failure".

Giờ trang integration có dòng "Thông báo tới khách (V1) — TẮT theo thiết kế, không phải lỗi".

**Quyết định: nêu như một bất biến, không đọc từ API.** Notification là immutable-off trong feature-flag guardrails — nó **không phải biến runtime**. Nối nó thành một giá trị đọc động sẽ ngụ ý một độ chính xác không tồn tại, và mở cửa cho việc một ngày nào đó nó hiện `ENABLED` vì một lỗi cấu hình mà đáng lẽ không thể xảy ra. Nêu như bất biến vừa đúng hơn vừa rẻ hơn — không cần đổi contract, không cần codegen.

## 3. Bảo đảm không nằm ở một cái cờ

Một cái cờ có thể bị lật bởi người sở hữu config. Một kiểu dữ liệu **không tồn tại** thì không ai gọi được.

| Test | Khẳng định |
| --- | --- |
| `UT-NOTIF-SURFACE-01` | Quét reflection toàn bộ assembly runtime: **không kiểu nào** mang tên chứa `notification`, `sms`, `zalo`, `email`, `push`, `messagetemplate`, `customermessage` — trừ chính khoá feature flag, thứ ghi lại quyết định |
| `UT-NOTIF-STORE-02` | Không `DbSet` nào có thể xếp hàng một tin nhắn khách. Và **nửa khẳng định dương**: hai hàng đợi outbound tồn tại đúng là `ResultCallbacks` + `TaskIntakeOutbox` — thêm cái thứ ba là test đỏ, không phải lọt qua im lặng |
| `UT-NOTIF-FLAG-03` | Bật qua admin mutation bị từ chối (`immutable-off`); **và** một snapshot đã bật đến từ lối khác (seed, restore, ghi thẳng) vẫn không khởi động được (`must remain disabled`) |
| `E2E` back-office (mở rộng) | Console hiển thị đúng câu "TẮT theo thiết kế", và **không** hiển thị bất kỳ chữ nào gợi ý lỗi gửi |

`UT-NOTIF-STORE-02` là test tôi thích nhất trong nhóm: khẳng định dương biến "không có bảng notification" thành "danh sách hàng đợi outbound là một tập đóng đã liệt kê". Kiểu khẳng định phủ định thuần tuý sẽ mục theo thời gian; kiểu này thì không.

## 4. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **340/340** (22 contract + 192 unit + 126 integration), +3 |
| `npm test` (admin-ui) | **181/181**, 17 file — thêm assertion vào e2e sẵn có, không thêm test mới |
| `npm run lint` / `typecheck` | 0 |
| `dotnet build -warnaserror` | 0 warning / 0 error |

## 5. Cái này KHÔNG chứng minh

- **Không mở phạm vi notification.** `W-0033` giữ `DEFERRED_TARGET`: V1 không gửi gì, và slice này chỉ chứng minh điều đó.
- **Không dựng consumer "để dành sau".** `P4-5` §3 cấm; không có interface, không có sink, không có template.
- **Không có cờ ẩn nào bật được delivery.** Đó chính là nội dung `UT-NOTIF-FLAG-03`.
- **Không coi notification là điều kiện phát hành.** Nó không xuất hiện trong bất kỳ release gate nào.
- Công việc CRM tương lai nằm **ngoài V1** cho tới khi có hợp đồng được duyệt riêng.
