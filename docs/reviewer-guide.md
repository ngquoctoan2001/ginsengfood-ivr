# Reviewer guide — cái máy không bắt được

Work `W-0038` (prompt `P5-4` §6.5) · Dùng cùng [`review-checklist.md`](review-checklist.md)

Checklist nói **kiểm gì**. File này nói **nhìn thế nào**, cho những chỗ một cổng tự động sẽ cho qua.

---

## 1. Test có đang chứng minh điều nó tuyên bố không

Câu hỏi đắt nhất trong review, và là câu duy nhất không tự động hoá được.

Ba lần trong dự án này một test đã **xanh vì lý do sai**:

| Ca | Test nói gì | Thực tế |
| --- | --- | --- |
| `UT-ELIG-DNC-02` (trước W-0031) | "SMS opt-out không chặn cuộc gọi thoại" | Field `SmsOptOut` **không rule nào đọc**, và `Map` truyền cứng `false`. Xanh vì một field chết |
| Helper `Evidence(capturedAt: null)` (W-0030) | ca "không có dấu thời gian" | Rơi vào nhánh `?? mặc định hợp lệ` nên vẫn có dấu thời gian tươi |
| `UT-TRACE-01` bản đầu (W-0035) | mọi test ID có trong bảng | Khớp "có nhắc tới ID" — mà header generator nhắc một ID trong văn xuôi |

**Cách nhìn:** với mỗi assertion, hỏi *"nếu code sai theo đúng cách test này muốn bắt, test có đỏ không?"*
Nếu không chắc, yêu cầu tác giả **phá code tạm** và cho thấy nó đỏ. Đó là chi phí một phút, đổi lấy một cổng thật.

## 2. Race và tính đồng thời

Máy thấy `Task.WhenAll`; nó không thấy **cái gì được bảo vệ**.

- Lease/fencing: một lease cũ có thể "sống lại" và chiếm tài nguyên sau khi lease đã chuyển không?
- `ONE_SIM_ONE_ACTIVE_CALL`: có đường nào hai job cùng chiếm một kênh không? Đây là bất biến mà hỏng thì **hai khách nghe đơn của nhau**.
- Blocker phát sinh **sau** khi khách bấm phím: kết quả đã quan sát có bị ghi đè không? (Không được — xem `IT-ELIG-RACE-12`.)
- Test có chạy **song song thật** không, hay tuần tự rồi gọi là concurrency?

## 3. Idempotency key

- Key có **derive từ dữ liệu** không, hay hard-code? Một key cố định đúng ở n=1 và vỡ ở n>1 — đúng lỗi `W-0037` tìm ra trong fixture.
- Retry có dùng **đúng key và đúng body** không? Retry với key mới là gửi trùng.
- Cùng key + body khác nhau → `IDEMPOTENCY_CONFLICT`, không phải chấp nhận im lặng.

## 4. Độ tươi của snapshot

Mọi bằng chứng Sales gửi đều có tuổi. Hỏi:

- Có kiểm `captured_at` nằm trong cửa sổ xác nhận không?
- **Thiếu** dấu thời gian có được xử như hỏng không, hay lặng lẽ coi là tươi?
- Dấu thời gian ở **tương lai** có bị từ chối không? Đó là lỗi đồng hồ hoặc lỗi producer, không phải sự thật mới hơn.

## 5. Ánh xạ taxonomy

Chỗ dễ sai nhất và tốn nhất:

- `rejected` (khách bấm từ chối) → **`NO_ANSWER` có tính lượt**, tuyệt đối **không phải** cancel. Đọc nhầm là huỷ đơn khách chưa bao giờ yêu cầu huỷ.
- Lỗi kỹ thuật → `TECHNICAL_EXCEPTION`, **không tính lượt**. Đọc nhầm là tiêu lượt gọi của khách vào bug của mình.
- Trên đường compat, 11 result type gập vào 4 — người xét phải xác nhận **cái gì bị mất** là chấp nhận được.
- Trạng thái lưu trong DB và giá trị trên wire có thể khác chính tả (`NO_STATE_CHANGE_...` vs `CORE_NO_STATE_CHANGE_...`). Assertion phải khớp đúng tầng nó đang kiểm.

## 6. Fail-closed đóng về chiều nào

Không phải mọi fail-closed đều đóng cùng hướng. Hai ví dụ trong repo này đóng **ngược nhau**:

| | Voice restriction | Trust skip |
| --- | --- | --- |
| Thiếu bằng chứng | **CHẶN** | **VẪN GỌI** |
| Vì sao | không biết có được gọi không thì không được gọi | không biết có được bỏ qua không thì không được bỏ qua |

**Cách nhìn:** với mỗi nhánh fail-closed mới, hỏi *"thiệt hại của hai chiều là gì?"* rồi kiểm hướng đóng có khớp câu trả lời không. Gộp hai loại vào một cờ là âm thầm chọn một thiệt hại.

## 7. Placeholder giả dạng tín hiệu

Một dòng `NOT_WIRED` trung thực tốt hơn một dòng `UP` không ai kiểm.

- Card/metric mới có **thật sự quan sát** cái nó khai không, hay chỉ trả một mặc định vui vẻ?
- `observed` có đúng nghĩa "cái này đến từ một quan sát" không?
- Có tín hiệu nào của **chính IVR** đang được trình bày như sức khoẻ của bên thứ ba không? (Circuit breaker của mình không phải sức khoẻ của Sales.)

## 8. Evidence có nói cả phần chưa làm không

Một file evidence chỉ liệt kê thành công là một file evidence chưa xong.

- Có mục "cái này KHÔNG chứng minh" không?
- Mục `NOT_RUN` / `BLOCKED_EXTERNAL` có nêu **lý do**, không chỉ nhãn?
- Có chỗ nào mock evidence được trình bày như đã đóng gate ngoài không?

---

## Khi nào nên chặn merge

Chặn nếu bất kỳ điều nào đúng:

1. Một bất biến governance ở §1 của checklist bị vi phạm.
2. Một test cũ bị **nới** mà không giải thích được vì sao fixture sai chứ không phải rule sai.
3. Evidence tuyên bố nhiều hơn cái đã chạy.
4. Một cổng bị tắt, đặt `allow_failure`, hoặc hạ ngưỡng để cho xanh.
5. Bạn không trả lời được câu hỏi §1 cho một assertion mới.
