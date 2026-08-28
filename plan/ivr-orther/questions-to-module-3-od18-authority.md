# Phiếu sign-off — gửi Module 3 (`ginsengfood-business-platform`)

**Chủ đề:** `OD-18` — Module 3 quyết định gọi, IVR chỉ thực thi
**Người gửi:** Team IVR / Module 8 (IVR Order Confirmation)
**Ngày gửi:** `2026-08-27` · **Trạng thái:** ⏳ CHỜ TRẢ LỜI
**Ưu tiên:** P1 — chặn `ACCEPTED` của `W-0123`, **không** chặn hành vi runtime hiện tại

> Phiếu này thay thế [questions-to-module-3-od15-risk-evidence.md](questions-to-module-3-od15-risk-evidence.md)
> (`SUPERSEDED`). Toàn cảnh bàn giao hai chiều nằm ở
> [IR-06](../../integration-requirements/06-module-3-api-handover.md); đây là phiếu hẹp chỉ để
> đóng `OD18-C1..C4`.

---

## 1. Điều đã đổi phía IVR

Owner Module 8 khóa **`OD-18` (2026-08-27): Module 3 quyết định nghiệp vụ, IVR chỉ thực thi cuộc
gọi.** Đã triển khai xong ở `W-0123`, contract `1.0.0-draft.21`.

Cụ thể, IVR **đã gỡ** khả năng tự bỏ cuộc gọi:

| Trước (`OD-15`) | Nay (`OD-18`, draft.21) |
| --- | --- |
| IVR đọc `trust.risk_evidence_available` + `risk_flags` rỗng → `TASK_SKIPPED_TRUSTED_CUSTOMER` | Không còn nhánh nào. Runtime **không bao giờ** phát sinh decision này |
| `trusted_skip_allowed=false` là veto | Field `deprecated`, được nhận và lưu nhưng **bị bỏ qua** |
| `customer_trust_status` dùng cho audit | `deprecated`, `LEGACY_READ` |
| `risk_flags` tham gia quyết định gọi/bỏ | Chỉ còn audit + ưu tiên scheduler, **không** đảo quyết định |

**Hệ quả quan trọng nhất, xin đọc kỹ:** nếu Module 3 từng dựa vào IVR để bỏ qua khách cũ, thì kể
từ bản này **những đơn đó sẽ được gọi**. IVR không còn lọc hộ. Đơn nào không cần gọi thì Module 3
phải **không gửi task**.

Contract giữ tương thích để rolling deploy không vỡ: ba field cũ vẫn được chấp nhận (chỉ bị bỏ
qua), enum `TASK_SKIPPED_TRUSTED_CUSTOMER` vẫn còn trong wire để client sinh sẵn không lỗi.
`oasdiff` xác nhận `draft.20 → draft.21` **không có breaking change**.

## 2. Câu hỏi cần trả lời

Mỗi câu xin trả lời kèm **commit / phiên bản OpenAPI / ảnh chụp runtime** — không nhận trả lời
suy đoán. Điền thẳng vào cột "Trả lời của M3".

### `OD18-C1` — Hiện Module 3 có gửi hay đọc ba field này không? (P1)

| Field / giá trị | Chiều | Trả lời của M3 | Bằng chứng |
| --- | --- | --- | --- |
| `customer_trust_status` | M3 **gửi** trong `POST /tasks`? | ☐ Có ☐ Không | |
| `trusted_skip_allowed` | M3 **gửi**? | ☐ Có ☐ Không | |
| `eligibility_snapshot.trust.risk_evidence_available` | M3 **gửi**? | ☐ Có ☐ Không | |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | M3 **đọc** decision này từ response? | ☐ Có ☐ Không | |

Vì sao cần: đây là điều kiện để IVR biết có được **xoá hẳn** ba field khỏi contract hay phải giữ
một cửa sổ tương thích. Trả lời "Không" cho cả bốn ⇒ `OD18-C4` chọn remove; có bất kỳ "Có" nào ⇒
giữ deprecate.

### `OD18-C2` — Module 3 đã lọc đơn không cần gọi trước khi gửi task chưa? (P1)

☐ Đã lọc — mô tả tiêu chí: `_______________________`
☐ Chưa lọc, đang dựa vào IVR bỏ qua
☐ Không áp dụng — mọi đơn `CONFIRMING` đều cần gọi

Vì sao cần: đây là câu **duy nhất** quyết định `OD-18` có an toàn trên dữ liệu thật hay không. Nếu
chọn ô thứ hai thì lượng cuộc gọi sẽ tăng ngay khi bản này lên, và hai bên cần chốt kế hoạch trước
khi deploy chứ không phải sau.

### `OD18-C3` — `customer_trust_status` có còn cần lưu không? (P2)

☐ Giữ cho audit — nêu use case và thời hạn lưu: `_______________________`
☐ Bỏ khỏi payload theo nguyên tắc tối thiểu hóa dữ liệu

Cần chữ ký của **M3 + Privacy + M8**. IVR đang lưu cột này nhưng không dùng vào bất cứ quyết định
nào; giữ một trường phân loại khách hàng mà không có mục đích là rủi ro privacy chứ không phải tài
sản.

### `OD18-C4` — Xoá enum/field ngay ở draft kế tiếp, hay giữ một cửa sổ tương thích? (P2)

☐ Remove ngay ở `draft.22` (chỉ chọn được nếu `OD18-C1` toàn "Không")
☐ Giữ `deprecated` thêm `___` tuần rồi mới remove

Remove là **breaking change**: cần chạy `oasdiff` và consumer contract test hai phía trước khi
chốt.

### `OD18-C5` — `risk_flags` giữ nguyên hay thay bằng field ưu tiên tường minh? (P3)

☐ Giữ `risk_flags`, IVR tiếp tục dùng cho ưu tiên scheduler
☐ Thay bằng một field ưu tiên riêng — đề xuất tên/kiểu: `_______________________`

IVR hiện tính điểm ưu tiên từ `risk_flags` (`SchedulerCapacityMapper.RiskScore`). Điều này **không**
ảnh hưởng quyết định gọi/bỏ, nhưng dùng một trường rủi ro nghiệp vụ làm tín hiệu xếp hàng kỹ thuật
là một sự lẫn lộn đáng dọn khi có dịp.

## 3. Nếu Module 3 vẫn gửi hình dạng cũ thì sao?

Không có gì hỏng, và **hai bên đo được** thay vì phải tin nhau.

IVR `W-0124` thêm counter `ivr_legacy_skip_candidate_total`, đếm tại intake mỗi task mang đúng hình
dạng predicate đã nghỉ hưu: không veto, `risk_flags` rỗng, `risk_evidence_available=true`. Task đó
vẫn được gọi bình thường — counter chỉ ghi lại.

Nghĩa là sau khi deploy, con số này chính là **số đơn Module 3 vẫn đánh dấu theo cách cũ**. Bằng
`0` nghĩa là hai bên đã khớp. Khác `0` thì không phải sự cố IVR, mà là danh sách công việc còn lại
của phiếu này — và mỗi đơn trong đó là một cuộc gọi thật tới một khách hàng thật.

## 4. Điều IVR **không** yêu cầu nữa

Để tránh Module 3 làm thừa: phiếu `OD-15` cũ yêu cầu Module 3 gửi
`eligibility_snapshot.trust.risk_evidence_available` để bật trusted-skip. **Yêu cầu đó đã bị huỷ.**
Không cần build gì cho nó. Nếu đang làm dở thì dừng lại.

## 5. Ô ký

| Vai trò | Tên | Ngày | Kết luận |
| --- | --- | --- | --- |
| Module 3 — owner contract | | | ☐ Đồng ý `OD-18` ☐ Cần bàn thêm |
| Module 8 — IVR owner | | | |
| Privacy (chỉ cho `OD18-C3`) | | | |

Sau khi có chữ ký, IVR cập nhật `IR-06` §9 và chuyển `W-0123` từ `TESTS_PASS` sang đề nghị
`ACCEPTED`. Chưa có chữ ký thì `W-0123` giữ nguyên `TESTS_PASS`.
