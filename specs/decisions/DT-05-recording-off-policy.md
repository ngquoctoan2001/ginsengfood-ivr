# DT-05 — Recording OFF policy

Trạng thái: **`LEGAL_SIGNOFF_REQUIRED`** · Ngày dự thảo: `2026-08-19` · Work: `W-0059` / `P11-3`

## 1. Quyết định

**Ghi âm cuộc gọi TẮT.** `ivr_raw_call_events.recording_ref` là `NULL` trong mọi môi trường, mọi chế
độ, mọi lúc.

Đây không phải một mặc định đợi ai đó bật. Nó là **trạng thái duy nhất** hôm nay có cơ sở pháp lý,
vì cơ sở pháp lý cho cuộc gọi là **thực hiện hợp đồng** — và thực hiện hợp đồng cho phép **gọi**,
không cho phép **ghi lại** cuộc gọi đó.

## 2. Vì sao TẮT không phải là "chưa làm"

Ghi âm là một hoạt động xử lý **riêng biệt** với cuộc gọi:

- mục đích khác (bằng chứng/đào tạo, không phải xác nhận đơn),
- cơ sở pháp lý khác (cần đồng ý, hoặc một cơ sở khác Legal xác lập),
- rủi ro khác (nội dung lời nói là dữ liệu chưa cấu trúc, không whitelist được như
  `privacy_safe_order_summary_json`),
- chu kỳ lưu khác.

Nên bật nó **không phải một thay đổi cấu hình**. Nó mở lại toàn bộ PIA.

## 3. Điều kiện mở lại — cả bốn, không phải ba

Nếu có lúc nào cần ghi âm, **tất cả** phải có trước khi một byte audio được lưu:

1. **Cơ sở pháp lý riêng cho việc ghi âm**, do Legal xác lập. Cơ sở của cuộc gọi **không** bao trùm.
2. **Cơ chế thu thập và lưu đồng ý**, nếu cơ sở là đồng ý — IVR hiện **không có** cơ chế nào, và một
   cơ sở "đồng ý" mà không thu thập được là không có cơ sở.
3. **Thông báo cho người bị ghi âm** trước khi ghi, bằng chính cuộc gọi đó.
4. **Chu kỳ lưu riêng cho bản ghi**, ký theo DF-07, cùng cơ chế xoá đã kiểm.

Và một điều kiện thứ năm mang tính kỹ thuật: **PIA phải viết lại**. `docs/compliance/pia.md` hiện
đánh giá một hệ thống không ghi âm; nó không nói gì về rủi ro của hệ thống có ghi âm.

## 4. Cái gì đang ép điều này

| Cơ chế | Ở đâu |
| --- | --- |
| `recording_ref` mặc định `NULL` | `specs/database/05-retention-and-privacy.md` §1 |
| Danh mục dữ liệu ghi rõ "không có bản ghi nào để xoá ngay từ đầu" | `docs/compliance/data-inventory.md` §3 |
| Readback bảo vệ chiều sâu | `W-0094` |

**Không có cổng CI nào ép `recording_ref IS NULL` trên dữ liệu chạy thật**, vì chưa có dữ liệu chạy
thật. Ghi ra đây thay vì để bảng trên trông như đã phủ hết.

## 5. Chữ ký

| Vai trò | Tên | Ngày | Kết luận |
| --- | --- | --- | --- |
| Legal | _(trống)_ | | |
| Privacy | _(trống)_ | | |
| Chủ sở hữu IVR | _(trống)_ | | |

Cho tới khi ba ô trên có tên, tài liệu này là **dự thảo**. Trạng thái `LEGAL_SIGNOFF_REQUIRED` ở đầu
trang là thứ `LEGAL-PII-02` đọc.
