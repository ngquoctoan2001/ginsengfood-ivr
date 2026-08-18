# R-05 — Năng lực TTS và nguồn audio

External work `W-0008` · quyết định `OD-V1-19` · gate `LAB_REAL_SIM` · trạng thái `OPEN`

Owner: **Product** (chấp nhận phát âm), **Infra** (tích hợp, chi phí), **Privacy/Legal** (DPA, dữ liệu rời mạng).

Due: chốt **trước `P8-1`** — không có nguồn audio thì `PlayAsync` không có gì để phát. Ngày cam kết của owner: `<owner điền>`.

## 1. Khoảng trống chính xác là gì

`P2-9` / `W-0066` đã dựng **port + fake + khung adapter**. Chưa có adapter thật, chưa chọn nhà cung cấp, và **chưa có một giây audio thật nào** trong toàn dự án.

`P8-1` gọi `PlayAsync` mà không có nguồn audio. Đây không phải nợ kỹ thuật — nó là điều kiện chặn: lab không chạy được nếu không có gì để phát cho máy nghe.

Hai hướng, chọn một hoặc kết hợp:

| Hướng | Ưu | Nhược |
| --- | --- | --- |
| **TTS động** | Đọc được mọi đơn, mọi tên sản phẩm | Nội dung đơn rời khỏi mạng nội bộ → PDPA; chi phí theo ký tự; phát âm cần nghiệm thu |
| **Ghép file thu sẵn** | Không có dữ liệu rời mạng; chi phí một lần | Chỉ đọc được phần cố định; tên sản phẩm và số tiền vẫn cần TTS hoặc thu từng cái |

Câu thoại Target V1 bắt buộc đọc **tên sản phẩm, số lượng, số tiền, khu vực giao** (xem [T-03](../target-v1-closure-pack/T-03-speech-summary.md)). Cả bốn đều là biến. Nghĩa là **hướng ghép file thuần tuý không đủ**, trừ khi whitelist bị thu về bộ hẹp — mà đó lại là `OD-V1-15`, chưa chốt.

**Hai quyết định này ràng buộc nhau.** Chốt whitelist hẹp làm phương án audio rẻ và an toàn hơn nhiều; chốt whitelist rộng gần như buộc phải có TTS.

## 2. Ràng buộc kỹ thuật đã có trong code

Nguồn: [`docs/capacity-model.md`](../../capacity-model.md) — ngân sách MOCK mặc định, **fail-closed**:

| Ràng buộc | Giá trị | Cưỡng chế ở đâu |
| --- | --- | --- |
| Ký tự tối đa mỗi lần tổng hợp | 1.200 | từ chối **trước khi** gọi nhà cung cấp |
| Yêu cầu tối đa mỗi tiến trình / phút | 60 | ngân sách cửa sổ cố định |
| Ký tự tối đa mỗi tiến trình / phút | 72.000 | ngân sách cửa sổ cố định |
| Thời lượng audio tối đa | 120 giây | từ chối kết quả nếu vượt |
| Timeout nhà cung cấp | 5 giây | thành `IVR_TECHNICAL_EXCEPTION`, **không bao giờ** thành no-answer |
| TTL cache tối đa | 900 giây | còn bị chặn thêm bởi hạn xác nhận và retention lời thoại |

Định danh cache: `SHA-256` trên `(script_template_id, script_version, hash(privacy_safe_order_summary), voice_id, locale)` — **không** chứa nội dung tóm tắt, số liên lạc, khu vực hay văn bản đã render. Khởi động lại là mất cache; job retention `P1-5` gọi hook purge.

Định dạng MOCK: **mono 8 kHz, 16-bit linear PCM (`audio/L16`)**, ~16 kB/giây. Đây là **metadata mô phỏng** — MOCK không mở socket mạng và không đại diện cho codec thật nào. Codec thật do câu trả lời ở [R-01](R-01-vendor-requirements.md) §6 quyết định.

## 3. Câu hỏi cho nhà cung cấp TTS

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Giọng | Có giọng tiếng Việt nào; nam/nữ; vùng miền | `<vendor điền>` |
| Chất lượng | Nghe tự nhiên đến đâu; có mẫu để nghe thử không | `<vendor điền>` |
| Định dạng đầu ra | Codec, sample rate; có ra được đúng thứ gateway nhận không | `<vendor điền>` |
| Độ trễ | p50/p95/p99 cho một câu ~1.200 ký tự | `<vendor điền>` |
| SSML | Hỗ trợ không; điều khiển được tốc độ, ngắt nghỉ, nhấn không | `<vendor điền>` |
| Gợi ý phát âm | Nhận được `pronunciation_hints` dạng nào | `<vendor điền>` |
| Quota | Giới hạn yêu cầu/phút, ký tự/tháng, đồng thời | `<vendor điền>` |
| Vùng | Endpoint đặt ở đâu; có endpoint trong nước không | `<vendor điền>` |
| Tính cước | Đếm ký tự thế nào — dấu câu, SSML, khoảng trắng có tính không | `<vendor điền>` |
| Lưu trữ | Nhà cung cấp có giữ văn bản gửi lên không; bao lâu; dùng để huấn luyện không | `<vendor điền>` |
| DPA | Có hợp đồng xử lý dữ liệu không; mẫu ra sao | `<vendor điền>` |
| Xoá | Yêu cầu xoá được không; trong bao lâu | `<vendor điền>` |

Ba dòng cuối là câu hỏi **Privacy/Legal**, không phải Infra. Gửi văn bản chứa tên sản phẩm khách đặt, số tiền và khu vực giao tới một dịch vụ bên ngoài là một hoạt động xử lý dữ liệu — kể cả khi không có tên và số điện thoại đi kèm.

**Dòng "dùng để huấn luyện" là dòng dễ bỏ sót nhất.** Nhiều dịch vụ mặc định được phép dùng input để cải thiện mô hình; phải tắt bằng hợp đồng, không bằng cấu hình.

## 4. Nghiệm thu phát âm tiếng Việt

Không đo bằng cảm nhận. Cần một bộ mẫu cố định, nghe bởi ít nhất 2 người, chấm đạt/không đạt từng dòng:

| # | Loại | Ví dụ cần đọc đúng | Đạt? |
| --- | --- | --- | --- |
| 1 | Tên sản phẩm thuần Việt | `<điền>` | `<điền>` |
| 2 | Tên sản phẩm có từ nước ngoài | `<điền>` | `<điền>` |
| 3 | Tên có dung tích/khối lượng (`500ml`, `1kg`) | `<điền>` | `<điền>` |
| 4 | Số tiền lớn | `560.000` đọc thành gì | `<điền>` |
| 5 | Số lượng + đơn vị | `2 hộp`, `10 gói` | `<điền>` |
| 6 | Khu vực giao có số | `Quận 7`, `Phường 12` | `<điền>` |
| 7 | Mã đơn rút gọn | đọc từng ký tự hay đọc thành số | `<điền>` |
| 8 | Câu hướng dẫn bấm phím | `bấm 1`, `bấm 0` — phải rõ ràng tuyệt đối | `<điền>` |

Dòng 8 quan trọng nhất và hay bị coi nhẹ: khách nghe nhầm hướng dẫn thì bấm nhầm phím, và hệ thống ghi nhận **ngược lại** ý khách. Không có cơ chế nào phát hiện việc đó.

Dòng 4 và 7 cần Product chốt **quy ước đọc**, không phải chọn nhà cung cấp: `560.000` đọc "năm trăm sáu mươi nghìn" hay "năm trăm sáu mươi ngàn"; mã đơn `0001` đọc "không không không một" hay "một".

## 5. Chi phí

Công thức đã có ở [`docs/capacity-model.md`](../../capacity-model.md), chưa tính được vì thiếu đơn giá:

```text
billable_characters = provider_characters_after_cache
monthly_tts_cost    = billable_characters / vendor_billing_unit × vendor_price_per_billing_unit
```

Đầu vào còn thiếu:

| Đầu vào | Nguồn | Giá trị |
| --- | --- | --- |
| Đơn giá và đơn vị tính cước | nhà cung cấp | `<điền>` |
| Cách tính dấu câu / SSML / gợi ý phát âm | nhà cung cấp | `<điền>` |
| Tỉ lệ cache hit đo được | lab/pilot | `<điền>` |
| Ký tự trung bình mỗi đơn | phụ thuộc `OD-V1-15` | `<điền>` |
| Số đơn mỗi tháng | Business | `<điền>` |

Tỉ lệ cache hit là biến có đòn bẩy lớn nhất: câu thoại chứa tên sản phẩm và số tiền của **từng đơn**, nên cache chỉ trúng khi hai đơn giống hệt nhau về nội dung đọc. Với danh mục sản phẩm hẹp và số tiền lặp lại, tỉ lệ trúng có thể cao; với danh mục rộng thì gần như bằng không. **Phải đo, không đoán.**

## 6. Closure artifact

`OD-V1-19` chỉ đóng khi có:

- [ ] **Quyết định nhà cung cấp** có chữ ký Product + Infra.
- [ ] **DPA / kết quả rà soát privacy** có chữ ký Privacy/Legal, trả lời rõ dòng "dùng để huấn luyện" ở §3.
- [ ] **Bộ nghiệm thu phát âm §4 đã chấm**, tối thiểu 2 người nghe, và Product chốt quy ước đọc cho dòng 4 và 7.
- [ ] **Mô hình chi phí §5 đã điền**, có tỉ lệ cache hit **đo được** chứ không giả định.
- [ ] **Xác nhận codec khớp** giữa đầu ra TTS và đầu vào gateway ([R-01](R-01-vendor-requirements.md) §6).

Cho tới lúc đó, mục TTS trong `docs/capacity-model.md` vẫn là mô hình kỹ thuật có biên, và phát âm, chi phí, năng lực nhiều kênh đều giữ `NOT_RUN`.
