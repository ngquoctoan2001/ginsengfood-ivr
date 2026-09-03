# IR-02 — Ops-Core Requirements (Module 1/2)

Trạng thái: `SUPERSEDED` · `2026-08-26` · Thay bởi owner decision `OD-17`.

## IVR không còn yêu cầu gì từ ops-core

`OD-17` gỡ `sellable_status[]` khỏi toàn bộ IVR — contract, code, database và console. Hệ quả: **IVR không đọc tồn kho, thu hồi, sale-lock hay quality-hold từ bất kỳ nguồn nào**, và không có lối dữ liệu nào giữa IVR và `ginsengfood-ops-core`.

Bảng `IR-OPS-01…07` ở bản trước liệt kê những thứ ops-core cần build **để phục vụ IVR**. Không còn mục nào trong đó là yêu cầu của IVR.

## Điều đó không có nghĩa ops-core hết việc

Ops-core vẫn phải phục vụ **Module 3** — và giờ quan trọng hơn trước, vì nó là lưới an toàn duy nhất còn lại:

> `D-06`: Order Core revalidate realtime với ops khi nhận callback. Nếu có Sale Lock/Recall mới → Core **block/hold, không confirm dù khách bấm `1`**.

Trước `OD-17` có hai tầng chặn đơn không bán được: IVR chặn trước khi quay số, và Module 3 chặn lúc revalidate. Nay chỉ còn tầng thứ hai. Nếu Module 3 bỏ bước revalidate đó, **không còn gì chặn việc xác nhận một đơn không bán được** — IVR sẽ không phát hiện ra, vì nó không còn nhìn thấy dữ liệu đó nữa.

Yêu cầu ops-core ↔ Module 3 là việc giữa hai module đó. Ghi lại ở đây chỉ để người đọc IVR biết mắt xích nào đang gánh.

## Current verification W-0149 — 03/09/2026

Đối chiếu read-only snapshot Module 3 `PhucApu@a3aad246d986` không tìm thấy generic Target V1
callback consumer, ACK `BLOCKED_BY_CORE`/`REJECTED_STALE` hoặc IVR revoke path trong
`back-end/src/main` và tests. Kết quả này không chứng minh M3 không có artifact ở nơi khác; nó chứng
minh repo/snapshot hiện được cung cấp **chưa đủ để xác nhận D-06 đã chạy**.

Do đó current topology chỉ là `CURRENT_OPTION_A_BEHAVIOR_PRESENT / M3_D06_RUNTIME_NOT_FOUND`.
Không khôi phục IVR→Ops egress để vá khoảng trống. Owner/M3 phải hoặc giao D-06 runtime + shared E2E
cho phương án A, hoặc ký lifecycle command qua M3 theo
[M8-09 decision pack](../plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md).

## Ghi chú ranh giới vẫn đúng

- Ops-core **không biết `order_id`** (`DO-CORR-1`) — chỉ tra theo SKU / batch / QR. Fan-out là việc của Order Core.
- **do-not-call / opt-out KHÔNG thuộc ops** (`DO-CORR-2`) — thuộc CRM / business-platform (`DC-01`). Đây là blocker IVR **thực sự** đọc, và nó đến qua task từ Module 3, không phải qua ops.
- **Sale Lock ops-core hiện = recall-triggered** (`DO-CORR-3`).

## Lịch sử

Nội dung `IR-OPS-01…07` và các đối soát source ngày `2026-08-25` nằm trong lịch sử git của file này. Không khôi phục để dùng làm yêu cầu mới mà không có quyết định owner thay `OD-17`.

Liên quan: [06-module-3-api-handover.md §4.6](06-module-3-api-handover.md) · [decisions-log `OD-17`](../plan/ivr-orther/decisions-log.md)
