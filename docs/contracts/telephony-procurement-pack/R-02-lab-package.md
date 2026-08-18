# R-02 — Gói lab 1 SIM thật

External work `W-0008` · quyết định `OD-V1-09` (kèm `OD-V1-20`) · gate `G-LAB-SIM` · trạng thái `OPEN`

Owner: **Infra** (thiết bị, kết nối), **Security** (allowlist, kill switch, bí mật).

Due: sẵn sàng **trước `P8-1`**. Ngày cam kết của owner: `<owner điền>`.

## 1. Mục tiêu và giới hạn

Lab tồn tại để trả lời **một** câu hỏi: hành vi thật của thiết bị/nhà mạng có khớp với những gì `MockTelephonyDispatchGateway` đang giả định không.

Nó **không** trả lời: hệ thống chịu được bao nhiêu kênh, chi phí bao nhiêu, khách hàng phản ứng thế nào. Ba câu đó thuộc [R-03](R-03-esim32-package.md) và giai đoạn pilot.

**Không cuộc gọi nào tới khách thật.** `REAL_CUSTOMER_CALL_ALLOWED=NO` trong suốt giai đoạn lab.

## 2. Danh mục cần có trước khi bật `IVR_ADAPTER_MODE=REAL`

| # | Hạng mục | Ai chuẩn bị | Trạng thái |
| --- | --- | --- | --- |
| 1 | 1 SIM thật, đã kích hoạt, gói cước cho phép gọi ra | Infra | `<điền>` |
| 2 | Thiết bị gateway đã lắp, có kết nối mạng tới môi trường lab | Infra | `<điền>` |
| 3 | Credential nhà cung cấp, nạp qua secret store chứ không nằm trong repo | Security | `<điền>` |
| 4 | Danh sách số test đã duyệt, tất cả do đội mình sở hữu | Infra + Security | `<điền>` |
| 5 | `labDestinationAllowlist` đã nạp đúng danh sách ở mục 4 | Security | `<điền>` |
| 6 | `globalDialKillSwitch` đã kiểm chứng là **chặn được thật** | Security | `<điền>` |
| 7 | Quyền chỉnh allowlist/kill switch — **hiện chưa tồn tại**, xem §5 | Security/Platform | `BLOCKED` |
| 8 | Nguồn audio: TTS đã chọn hoặc file thu sẵn — xem [R-05](R-05-tts-audio-capability.md) | Product + Infra | `<điền>` |
| 9 | Biểu mẫu nghiệm thu đã in/sao chép sẵn | Infra | [template](lab-acceptance-report-template.md) |

Mục 7 và 8 hiện đang chặn. Không bật `REAL` khi bất kỳ dòng nào chưa `PASS`.

## 3. Số test — quy tắc

- **Chỉ số do đội mình sở hữu và cầm được máy.** Không mượn số người quen, không dùng số trong dữ liệu khách hàng, kể cả dữ liệu đã ẩn.
- **Tối thiểu 3 số** trên **ít nhất 2 nhà mạng** khác nhau — hành vi DTMF và mã trạng thái khác nhau giữa các mạng, và đó chính là thứ cần đo.
- **Một số cố ý để sai/không tồn tại** để kiểm nhánh `InvalidDestination`.
- Ghi vào `labDestinationAllowlist`. Mọi số ngoài danh sách bị IVR chặn **trước khi** chạm nhà cung cấp — đây là lớp bảo vệ của mình, không phụ thuộc thiết bị.
- **Không viết số thật vào tài liệu, evidence hay commit.** Dùng nhãn `LAB-A`, `LAB-B`, `LAB-C`; bảng ánh xạ nhãn→số giữ ngoài repo. Cổng PII của CI sẽ chặn nếu vi phạm.

| Nhãn | Nhà mạng | Mục đích | Ai giữ máy |
| --- | --- | --- | --- |
| `LAB-A` | `<điền>` | luồng chính: nghe máy, bấm `1` | `<điền>` |
| `LAB-B` | `<điền>` | khác mạng: bấm `0`, từ chối, bận | `<điền>` |
| `LAB-C` | `<điền>` | số không tồn tại / sai định dạng | — |

## 4. Kết nối và bí mật

| Hạng mục | Yêu cầu |
| --- | --- |
| Cô lập | Lab tách khỏi môi trường có dữ liệu khách hàng. Không dùng chung credential với bất kỳ môi trường nào khác. |
| Chiều kết nối | Ghi rõ IVR chủ động gọi ra hay nhà cung cấp gọi vào; nếu có callback vào thì phải xác thực và nằm trong allowlist IP. |
| Bí mật | Qua secret store (`W-0063`, đang `BLOCKED_EXTERNAL`). Trong lúc chờ, dùng biến môi trường ngoài repo và ghi rõ đây là tạm thời. |
| Nhật ký | Không log số quay, không log token, không log nội dung thoại. `RenderedSpeech.ToString()` đã trả `[REDACTED_RENDERED_SPEECH]` — đừng vòng qua nó. |
| Thu hồi | Trước khi bắt đầu, xác nhận thu hồi được credential trong bao lâu. Sau khi kết thúc lab, thu hồi ngay. |

## 5. Khoảng trống về quyền — cần biết trước khi lên lịch lab

`OD-V1-20`: bộ permission `DF-01` (LOCKED, 7 quyền) **không có quyền nào** cho phép sửa `labDestinationAllowlist` hay `globalDialKillSwitch`.

Nghĩa là hôm nay, hai control an toàn quan trọng nhất của lab **không có ai được phép bấm** qua console. Cần Security/Platform + Release owner duyệt permission mới **kèm cơ chế bốn mắt**, rồi mới lên lịch lab. Đây là điều kiện tiên quyết, không phải việc dọn dẹp sau.

## 6. Checklist kịch bản — phải chạy hết

### 6a. Bảy kịch bản mock đã có, chạy lại trên SIM thật

Nguồn: `seed/call-scenarios.sample.json`. Mục đích là **so sánh**, nên phải chạy đúng cùng luồng:

| ID | Kịch bản | Kết quả IVR mong đợi | Tính lượt? | Thực tế |
| --- | --- | --- | --- | --- |
| `SCN-001` | nghe máy, bấm `1` | `IVR_CONFIRMED` | có | `<điền>` |
| `SCN-002` | nghe máy, bấm `0` | `IVR_CUSTOMER_CANCELLED` | có | `<điền>` |
| `SCN-003` | đổ chuông hết giờ ×2 | `IVR_NO_ANSWER_FINAL` | có | `<điền>` |
| `SCN-004` | bận → lần 2 nghe máy bấm `1` | `IVR_CONFIRMED` | có | `<điền>` |
| `SCN-005` | số không tồn tại | `IVR_INVALID_PHONE_FINAL` | **không** | `<điền>` |
| `SCN-006` | lỗi SIM | `IVR_TECHNICAL_EXCEPTION` | **không** | `<điền>` |
| `SCN-007` | hết window trước khi xong | `IVR_CONFIRMATION_WINDOW_EXPIRED` | có | `<điền>` |

### 6b. Kịch bản mock KHÔNG dựng được — đây mới là lý do phải có lab

| # | Kịch bản | Đo cái gì | Thực tế |
| --- | --- | --- | --- |
| L-01 | Khách bấm phím **trong lúc** đang phát thoại (barge-in) | Có bắt được không; nếu không, thời lượng cuộc gọi tăng bao nhiêu | `<điền>` |
| L-02 | Vào hộp thư thoại | Nhà cung cấp báo `Answered` hay trạng thái riêng? Nếu báo `Answered`, IVR sẽ đọc thoại cho hộp thư và chờ phím vô ích | `<điền>` |
| L-03 | Khách bấm nút từ chối | Trạng thái thô là gì; **phải** ánh xạ thành `Rejected` → `IVR_NO_ANSWER`, không phải cancel | `<điền>` |
| L-04 | Bấm phím sai (`5`, `9`, `#`) | Trả về gì; `KEY_9` phải ra `IVR_WRONG_INPUT`, không kích hoạt luồng nào | `<điền>` |
| L-05 | Bấm nhiều phím liên tiếp | Lấy phím đầu hay phím cuối; có ổn định không | `<điền>` |
| L-06 | Chất lượng thoại thật trên mạng di động | Khách nghe rõ không: mã đơn, số tiền, tên sản phẩm | `<điền>` |
| L-07 | Cooldown 5 giây giữa hai cuộc trên cùng SIM | Có đủ không, hay thiết bị cần lâu hơn | `<điền>` |
| L-08 | 3 lần lỗi liên tiếp → quarantine | `fail_count` tăng đúng; kênh bị cách ly; alert phát ra | `<điền>` |
| L-09 | **Kill switch trong lúc đang có cuộc gọi** | Cuộc đang chạy xử lý ra sao; cuộc tiếp theo **phải** bị chặn | `<điền>` |
| L-10 | Tắt kênh trong lúc `busy=true` | Tắt có hiệu lực sau khi cuộc kết thúc, đúng như mô tả trong `IvrSimChannel` | `<điền>` |
| L-11 | Rút mạng / mất kết nối giữa cuộc | Ra `Dropped` hay `NetworkError`; IVR có treo không | `<điền>` |
| L-12 | Caller ID hiển thị trên máy khách | Đúng số/tên mong đợi không; có bị gắn nhãn spam không | `<điền>` |
| L-13 | Đối soát CDR | `provider_call_reference` nối được sang `attempt_id` không | `<điền>` |
| L-14 | Đọc ngược trạng thái recording qua `health()` | Trả về `recording_disabled: true` — nếu không, dừng lab | `<điền>` |
| L-15 | Số ngoài allowlist | IVR chặn **trước khi** chạm nhà cung cấp; không có cuộc gọi nào phát sinh | `<điền>` |

L-02 và L-09 là hai kịch bản mà kết quả có thể **buộc sửa code** chứ không chỉ sửa cấu hình. Nên chạy sớm trong lịch lab.

## 7. Quy tắc dừng

Dừng lab ngay và báo owner nếu:

- một cuộc gọi phát sinh tới số **không** nằm trong allowlist;
- `health()` báo recording **không** ở trạng thái tắt;
- kill switch bật mà vẫn có cuộc gọi mới đi ra;
- có bất kỳ số điện thoại thô nào xuất hiện trong log, evidence hay database của IVR.

Bốn điều này không phải "kết quả cần ghi nhận" — chúng là lỗi chặn, phải sửa trước khi chạy tiếp.

## 8. Closure artifact

`OD-V1-09` và phần lab của `W-0008` chỉ đóng khi có:

- [ ] **Biểu mẫu nghiệm thu đã điền đủ** — [lab-acceptance-report-template.md](lab-acceptance-report-template.md) — với kết quả thật cho cả 7 dòng §6a và 15 dòng §6b.
- [ ] **Bảng ánh xạ disposition đã xác minh trên thiết bị thật**, đối chiếu với bảng nhà cung cấp khai ở [R-01](R-01-vendor-requirements.md) §7. Lệch chỗ nào ghi chỗ đó.
- [ ] **Bằng chứng kill switch và allowlist chặn thật** (L-09, L-15).
- [ ] **Permission `OD-V1-20` đã duyệt** kèm four-eyes.

Lắp xong thiết bị **không** đóng gate. Gọi thành công một cuộc **không** đóng gate. Chỉ biểu mẫu §8 điền đủ mới đóng.
