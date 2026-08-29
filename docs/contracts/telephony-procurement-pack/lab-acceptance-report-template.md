# Biểu mẫu — Báo cáo nghiệm thu lab 1 SIM

> Bản mẫu. Khi chạy `P8-1`, sao chép thành `docs/evidence/W-0048/lab-acceptance-report.md` và điền. **Không sửa file mẫu này.**
>
> File điền xong nằm trong `docs/evidence/`, tức trong phạm vi quét PII. **Không ghi số điện thoại thật** — dùng nhãn `LAB-A`/`LAB-B`/`LAB-C`. Chạy `sh deploy/ci/scripts/scan-pii.sh docs/evidence` trước khi coi là xong.

## 0. Định danh phiên lab

| Trường | Giá trị |
| --- | --- |
| Ngày chạy | `<điền>` |
| Người chạy | `<điền>` |
| Người chứng kiến (bắt buộc, four-eyes) | `<điền>` |
| Nhà cung cấp / sản phẩm / phiên bản firmware | `<điền>` |
| `sim_channel_id` | `<điền>` |
| Commit IVR | `<điền>` |
| `IVR_ADAPTER_MODE` | `REAL` |
| `REAL_CUSTOMER_CALL_ALLOWED` | `NO` |
| `attempt_policy_version` đang dùng | `<điền>` (candidate — chưa phải production, xem `OD-V1-16`) |

## 1. Điều kiện tiên quyết — đủ 9 dòng mới được bắt đầu

| # | Hạng mục | PASS/FAIL | Bằng chứng |
| --- | --- | --- | --- |
| 1 | SIM đã kích hoạt, gói cước gọi ra | `<điền>` | `<điền>` |
| 2 | Gateway lắp xong, có kết nối | `<điền>` | `<điền>` |
| 3 | Credential nạp qua secret store, không nằm trong repo | `<điền>` | `<điền>` |
| 4 | Danh sách số test đã duyệt, tất cả do đội mình sở hữu | `<điền>` | `<điền>` |
| 5 | `labDestinationAllowlist` đã nạp đúng | `<điền>` | `<điền>` |
| 6 | `globalDialKillSwitch` đã kiểm chứng chặn thật | `<điền>` | `<điền>` |
| 7 | Four-eyes cho `OD-V1-20` đã ký (permission đã cấp cho `Admin` 2026-08-22) | `<điền>` | `<điền>` |
| 8 | Nguồn audio sẵn sàng (`OD-V1-19`) | `<điền>` | `<điền>` |
| 9 | Biểu mẫu này đã sao chép sẵn | `<điền>` | — |

Bất kỳ dòng nào `FAIL` → **không bật `REAL`**.

## 2. Kết quả 7 kịch bản đối chiếu mock

| ID | Kết quả IVR mong đợi | Trạng thái thô nhận được | Kết quả IVR thực tế | Tính lượt đúng? | PASS/FAIL |
| --- | --- | --- | --- | --- | --- |
| `SCN-001` | `IVR_CONFIRMED` | `<điền>` | `<điền>` | `<điền>` | `<điền>` |
| `SCN-002` | `IVR_CUSTOMER_CANCELLED` | `<điền>` | `<điền>` | `<điền>` | `<điền>` |
| `SCN-003` | `IVR_NO_ANSWER_FINAL` | `<điền>` | `<điền>` | `<điền>` | `<điền>` |
| `SCN-004` | `IVR_CONFIRMED` | `<điền>` | `<điền>` | `<điền>` | `<điền>` |
| `SCN-005` | `IVR_INVALID_PHONE_FINAL` | `<điền>` | `<điền>` | không tính | `<điền>` |
| `SCN-006` | `IVR_TECHNICAL_EXCEPTION` | `<điền>` | `<điền>` | không tính | `<điền>` |
| `SCN-007` | `IVR_CONFIRMATION_WINDOW_EXPIRED` | `<điền>` | `<điền>` | `<điền>` | `<điền>` |

## 3. Kết quả 15 kịch bản chỉ lab mới dựng được

| # | Kịch bản | Quan sát | PASS/FAIL | Cần sửa gì |
| --- | --- | --- | --- | --- |
| L-01 | Barge-in | `<điền>` | `<điền>` | `<điền>` |
| L-02 | Hộp thư thoại | `<điền>` | `<điền>` | `<điền>` |
| L-03 | Khách từ chối cuộc gọi | `<điền>` | `<điền>` | `<điền>` |
| L-04 | Phím sai (`5`, `9`, `#`) | `<điền>` | `<điền>` | `<điền>` |
| L-05 | Nhiều phím liên tiếp | `<điền>` | `<điền>` | `<điền>` |
| L-06 | Chất lượng thoại | `<điền>` | `<điền>` | `<điền>` |
| L-07 | Cooldown 5 giây | `<điền>` | `<điền>` | `<điền>` |
| L-08 | 3 lỗi → quarantine | `<điền>` | `<điền>` | `<điền>` |
| L-09 | Kill switch giữa cuộc gọi | `<điền>` | `<điền>` | `<điền>` |
| L-10 | Tắt kênh khi `busy=true` | `<điền>` | `<điền>` | `<điền>` |
| L-11 | Mất kết nối giữa cuộc | `<điền>` | `<điền>` | `<điền>` |
| L-12 | Caller ID hiển thị | `<điền>` | `<điền>` | `<điền>` |
| L-13 | Đối soát CDR ↔ `attempt_id` | `<điền>` | `<điền>` | `<điền>` |
| L-14 | `health()` báo recording tắt | `<điền>` | `<điền>` | `<điền>` |
| L-15 | Số ngoài allowlist bị chặn | `<điền>` | `<điền>` | `<điền>` |

## 4. Bảng ánh xạ disposition đã xác minh

Điền trạng thái thô **thực sự quan sát được**, không chép lại từ tài liệu nhà cung cấp.

| IVR disposition | Nhà cung cấp khai ([R-01](R-01-vendor-requirements.md) §7) | Quan sát thật | Khớp? |
| --- | --- | --- | --- |
| `Answered` | `<điền>` | `<điền>` | `<điền>` |
| `RingTimeout` | `<điền>` | `<điền>` | `<điền>` |
| `Busy` | `<điền>` | `<điền>` | `<điền>` |
| `Rejected` | `<điền>` | `<điền>` | `<điền>` |
| `Unreachable` | `<điền>` | `<điền>` | `<điền>` |
| `InvalidDestination` | `<điền>` | `<điền>` | `<điền>` |
| `Dropped` | `<điền>` | `<điền>` | `<điền>` |
| `NetworkError` | `<điền>` | `<điền>` | `<điền>` |
| `SimError` | `<điền>` | `<điền>` | `<điền>` |
| `AudioError` | `<điền>` | `<điền>` | `<điền>` |
| `DtmfError` | `<điền>` | `<điền>` | `<điền>` |

Trạng thái thô nào **không** ánh xạ được vào 11 giá trị trên: `<điền>`

## 5. Số đo

Không nộp một con số trung bình đứng một mình. Capacity cần phân biệt **channel occupancy**
(`started_at → ended_at`) với **full channel cycle** (`occupancy + cooldown`). Nếu trộn hai nghĩa,
model sẽ cộng cooldown hai lần hoặc bỏ sót nó.

| Chỉ số | N | p50 | p95 | p99 | Cách đo / nguồn |
| --- | ---: | ---: | ---: | ---: | --- |
| `dial` → đổ chuông | `<điền>` | `<điền>` | `<điền>` | `<điền>` | gateway event / CDR |
| Channel occupancy: `started_at` → `ended_at` | `<điền>` | `<điền>` | `<điền>` | `<điền>` | IVR attempt + provider CDR |
| Cooldown thực tế | `<điền>` | `<điền>` | `<điền>` | `<điền>` | `ended_at` → `cooldown_until` hoặc lúc kênh thật sự available |
| Full channel cycle | `<điền>` | `<điền>` | `<điền>` | `<điền>` | occupancy + cooldown |

Nếu số mẫu không đủ để một percentile có ý nghĩa, ghi **`INSUFFICIENT_SAMPLE`** và giữ `N`; không
nội suy cho đẹp. Giữ bảng dòng nguồn sau đây trong evidence PII-safe để reviewer tái tính được:

| Run | Attempt label | Programme | Carrier label | Scenario / disposition | `started_at` | `ended_at` | `cooldown_until` / available again | Occupancy (s) | Full cycle (s) | CDR correlation ref |
| --- | --- | --- | --- | --- | --- | --- | --- | ---: | ---: | --- |
| `<điền>` | `<không dùng số điện thoại>` | `<điền>` | `LAB-A/B/C` | `<điền>` | `<UTC>` | `<UTC>` | `<UTC>` | `<điền>` | `<điền>` | `<không chứa PII>` |

| Chỉ số phiên | Giá trị | Cách đo |
| --- | --- | --- |
| Tổng số attempt hợp lệ | `<điền>` | `<điền>` |
| Tỉ lệ bắt DTMF thành công | `<điền>` | `<điền>` |
| Tỉ lệ theo từng disposition | `<điền>` | `<điền>` |
| Số cuộc tối đa quan sát được trong 1 giờ trên 1 SIM | `<điền>` | `<điền>` |

Các dòng per-attempt và percentile là input timing cho `W-0142`/M8-01. **Không** suy năng lực
nhiều kênh bằng cách nhân kết quả 1 SIM; R-03 còn cần arrival profile, attempt policy, tỉ lệ
no-answer/retry và reserve/failure factor.

## 6. Sự cố trong phiên

| # | Xảy ra lúc | Mô tả | Có phải điều kiện dừng §7 của R-02 không | Xử lý |
| --- | --- | --- | --- | --- |
| 1 | `<điền>` | `<điền>` | `<điền>` | `<điền>` |

## 7. Kết luận

| Câu hỏi | Trả lời |
| --- | --- |
| Có cuộc gọi nào tới số ngoài allowlist không? | `<điền>` — nếu **có**, phiên lab này không hợp lệ |
| Có số điện thoại thô nào lọt vào log/evidence/DB không? | `<điền>` — nếu **có**, phiên lab này không hợp lệ |
| Recording có tắt suốt phiên không? | `<điền>` |
| Bao nhiêu dòng §2 và §3 `PASS`? | `<điền>` / 22 |
| Cần sửa code trước khi chạy lại không? | `<điền>` |

**Đề xuất trạng thái `G-LAB-SIM`:** `<PASS / FAIL / PARTIAL>`

Đề xuất này là **đầu vào cho owner**, không phải quyết định. Chỉ reviewer/owner chuyển `W-0048`/`W-0008` sang trạng thái mới.

## 8. Chữ ký

| Vai trò | Tên | Ngày |
| --- | --- | --- |
| Người chạy | `<điền>` | `<điền>` |
| Người chứng kiến | `<điền>` | `<điền>` |
| Owner nghiệm thu | `<điền>` | `<điền>` |
