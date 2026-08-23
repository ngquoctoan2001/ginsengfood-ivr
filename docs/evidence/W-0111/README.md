# W-0111 — Cắt ngang cuộc gọi đang diễn ra

Ngày: `2026-08-23`
Baseline: `main@83979d6`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A4](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

> `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả bốn môi trường. Bản này thêm một chốt **dừng**,
> không mở thêm gì.

---

## 1. Vấn đề đã đóng

`readiness-board.md` §6 ghi: *"cắt ngang cuộc đang gọi | **không có cơ chế nào**"*.

Kill switch và queue pause đều dừng cuộc **sắp** gọi. Nếu phát hiện script sai, giọng sai, hay
gọi nhầm số **trong lúc đang nói chuyện với khách**, không có gì dừng được — phải đợi khách cúp
máy. Với `REAL_CUSTOMER_CALL_ALLOWED=NO` đây là rủi ro lý thuyết; ngày bật cờ đó lên nó thành
rủi ro thật, và lúc đó mới làm là muộn.

---

## 2. Ràng buộc kiến trúc quyết định thiết kế

`Ivr.Api` **không** đăng ký `ISimGateway`. Telephony thuộc worker. Nên API **không thể** gọi
thẳng nhà mạng để cúp máy.

⇒ Cắt ngang phải qua cơ sở dữ liệu: API **ghi yêu cầu**, worker **đọc và cắt**.

Hệ quả phải nói thẳng: độ trễ bằng chu kỳ poll của vòng lặp dispatch —
`TerminationPollMilliseconds`, mặc định **500 ms**, sàn cứng **200 ms** trong code để cấu hình
sai không thành busy-loop. Đó là lý do phản hồi API nói **"đã yêu cầu"**, không nói "đã cắt".

Không có cách nào nhanh hơn mà không đưa telephony vào tiến trình API.

---

## 3. Phạm vi đã triển khai

### 3.1 Lược đồ

Migration `W0111CallTerminationRequest` — 3 cột nullable trên `ivr_call_attempts`
(`termination_requested_at/_by/_reason`), một partial index, và một check constraint buộc **cả
ba cùng có hoặc cùng không**. Một dòng có timestamp mà không có actor là một thao tác vận hành
không hỏi được ai — đúng thứ một lệnh cắt có audit không bao giờ được phép trở thành.

Thuần bổ sung: code cũ bỏ qua cột mới, nên rolling deploy an toàn.

### 3.2 Vòng lặp dispatch

Cả hai gateway (`MockTelephonyDispatchGateway`, `AsteriskSchedulerDispatchGateway`) đua lệnh chờ
phím với một vòng poll:

- Có yêu cầu ⇒ **cúp máy trước**, ném `CallTerminatedException` sau. Kết thúc kênh mới là thứ
  thật sự làm khách ngừng nghe; unwind vòng lặp chỉ là cách ghi lại.
- Kiểm **lần nữa** sau khi lệnh chờ phím trả về. Một cú cúp máy do người vận hành có thể làm
  lệnh chờ phím kết thúc *bình thường*, và thiếu bước này vòng lặp sẽ ghi một lệnh cắt thành
  kết quả của khách — khách không bấm gì, mà "không bấm gì" là một sự kiện rất khác với
  "ta đã ngừng nói chuyện với họ".
- Ngoại lệ rơi vào **lối thất bại sẵn có**, vốn đã trả lease, trả kênh và ghi technical
  exception.

### 3.3 Ngữ nghĩa

| | |
| --- | --- |
| Kết quả | `IVR_TECHNICAL_EXCEPTION`, mã `CALL_TERMINATED_BY_OPERATOR` |
| Tính vào số lần gọi của khách | **Không** |
| Phím khách bấm sau khi có lệnh cắt | **Không ghi** |
| Kênh SIM | Về `IDLE`, **không** vào cooldown |

Kênh không bị phạt là có chủ đích: cuộc bị cắt là do **ta**, không phải thiết bị hỏng, và đưa
kênh vào cooldown sẽ lấy mất năng lực gọi như một tác dụng phụ của một chốt an toàn.

Ràng buộc `ck_ivr_call_attempts_technical_not_counted` sẵn có ở CSDL cũng ép "technical ⇒ không
tính" — nên bất biến này được giữ ở hai lớp, không chỉ trong code.

### 3.4 API

| Route | Quyền |
| --- | --- |
| `POST /call-jobs/{ivrCallJobId}:terminate` | `IVR_CALL_TERMINATE` |
| `POST /call-jobs:terminate-all` | `IVR_CALL_TERMINATE` |

`IVR_CALL_TERMINATE` cấp cho **cả Admin và Operator**. Đây là chiều giảm rủi ro và không khởi
động được gì; bắt operator đi tìm admin trong lúc khách đang nghe nhầm kịch bản là một chốt tốn
hơn cái nó bảo vệ.

`409` khi không có cuộc nào đang chạy, và **không ghi gì**. Một thao tác an toàn bị từ chối mà
vẫn để lại bản ghi dở dang còn tệ hơn một thao tác không để lại gì.

### 3.5 Cắt hàng loạt — tách riêng, không gộp vào kill switch

Kế hoạch nói "bật kill switch ⇒ **tùy chọn** cắt mọi cuộc đang chạy, mặc định không tự cắt".
Đã làm thành **một route và một nút riêng**, đứng cạnh kill switch trên `/flags`.

Kill switch chặn cuộc **tiếp theo**; cắt hàng loạt ngắt giữa chừng những cuộc **đang diễn ra**
với người thật. Gộp lại nghĩa là một người vận hành với tay tới nút dừng thường ngày và im lặng
cắt ngang mọi khách hàng giữa câu.

### 3.6 Màn hình

Nút trên `/calls/[id]`, chỉ hiện khi có attempt đã bắt đầu và chưa kết thúc. Ẩn theo trạng thái
chỉ để dễ đọc — server vẫn trả `409` nếu cuộc đã kết thúc giữa lúc render và lúc bấm, nên một
trang cũ không cắt được cuộc đã xong.

---

## 4. Đối chiếu tiêu chí nghiệm thu

| Tiêu chí (plan §A4) | Test | Kết quả |
| --- | --- | --- |
| Cắt cuộc đang chạy ⇒ lease giải phóng, channel `IDLE`, không kẹt fencing token | `IT-TEL-TERMINATE-04` | ✅ |
| Kết quả là technical exception, `customer_attempt_counted=false` | `IT-TEL-TERMINATE-04`, `CHAOS-TERMINATE-08` | ✅ + ràng buộc CSDL |
| Cắt cuộc đã kết thúc ⇒ `409`, không tạo bản ghi rác | `IT-API-TERMINATE-09` | ✅ assert cả audit count lẫn `ivr_admin_actions` rỗng |
| Chaos: cắt đúng lúc chuyển trạng thái ⇒ không mất/ghi trùng kết quả | `CHAOS-TERMINATE-07` | ✅ |

Thêm ngoài yêu cầu: `IT-API-TERMINATE-10` (job lạ ⇒ `404`, sai quyền ⇒ `403`),
`UT-UI-CALLDETAIL-*` (nút chỉ hiện khi có cuộc đang chạy **và** có quyền — cả hai vế, vì mỗi vế
một mình sẽ xanh vì lý do sai).

### Về cửa sổ tranh chấp

`CHAOS-TERMINATE-07` mô hình hoá đúng cái không khoá được: API đọc "cuộc này đang chạy", worker
kết thúc nó một khoảnh khắc sau, yêu cầu rơi vào một attempt vừa xong. Console và worker là hai
tiến trình, và điện thoại của khách không đợi bên nào cả.

Cái phải sống sót là **bản ghi**: đúng một raw event, đúng một kết quả, không ghi đè im lặng
lên điều thật sự đã xảy ra trên kênh thoại — vì kết quả đó là thứ Order Core được báo, và một bản
ghi trùng hay bị viết lại là một khách hàng có trạng thái đơn không khớp với cuộc gọi của họ.

Yêu cầu đến muộn được **giữ lại trên dòng** nhưng vô hiệu. Có chủ đích: người vận hành đã bấm
nút xứng đáng thấy rằng họ đã bấm, kể cả khi nó đến muộn.

---

## 5. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **449 / 449** |
| `Ivr.IntegrationTests` | **238 / 238** (+3 mới) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **8 / 8** (+2 mới) |
| **Tổng .NET** | **717 / 717** |
| admin-ui | lint + `tsc` + **218 / 218** (+1 mới) + build |
| Traceability | **433** tagged test |

OpenAPI `draft.13 → draft.14`: +2 path, re-pin manifest, sinh lại portal.

---

## 6. Những gì bản này **không** làm

- **Không cắt tức thì.** Độ trễ = chu kỳ poll (mặc định 500 ms). Xem §2 — đây là ràng buộc kiến
  trúc, không phải lựa chọn.
- **Không cắt được cuộc mà worker đã chết giữa chừng.** Yêu cầu nằm đó; lease hết hạn theo cơ
  chế recovery sẵn có. Cắt ngang cần một vòng lặp còn sống để đọc nó.
- **Không có e2e chạy console và API thật cùng lúc.** Lớp console và lớp API được chứng minh
  riêng, như W-0110.
- **Chưa nghe thử trên cuộc gọi thật.** Toàn bộ chạy trên MOCK và trên `PostgresTelephonyDispatchStore`
  thật; chưa có SIM.
- **Chưa cập nhật `readiness-board.md` §6.** Dòng "không có cơ chế nào" do
  `gate-status.mjs` sinh từ tracker, và nội dung §6 là văn bản viết tay trong generator — sửa nó
  là một việc riêng, không phải của W-0111.
