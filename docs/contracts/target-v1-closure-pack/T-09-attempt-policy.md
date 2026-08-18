# T-09 — `attempt_policy_version` cho production

External work `W-0007` · quyết định `OD-V1-16` (kèm `OD-V1-08`) · gate **production** · trạng thái `OPEN`

Owner: **Product / Order Core**.

Due: chốt **trước release gate `P9-1`** — MOCK/LAB chạy được bằng candidate, production thì không. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Code đang chạy candidate `mock-lab-v1`, và nó implement đúng `D-10`.** [`src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs:10`](../../../src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs):

| Program | Attempts | Offsets | Window | Approval |
| --- | --- | --- | --- | --- |
| Golden Hour | 2 | `0`, `150` giây | 5 phút | `CandidateMockLabOnly` |
| 24/7 | 2 | `0`, `450` giây | 15 phút | `CandidateMockLabOnly` |

Khớp `D-10` trong [`specs/04-glossary.md:23-25`](../../../specs/04-glossary.md): Giờ Vàng window 5′ (A1@T0, A2@T0+2:30), 24/7 window 15′ (A1@T0, A2@T0+7:30).

**Hai tài liệu business ghi con số khác.** Cả hai **không** mang banner `D-10`:

[`docs/documents/4. phase/phase-8/10-KIẾN TRÚC TRIỂN KHAI.md:121`](<../../../docs/documents/4. phase/phase-8/10-KIẾN TRÚC TRIỂN KHAI.md>) §8 Runtime invariants:

> Golden Hour: đúng 2 customer-counted attempts trong **10 phút**.
> 24/7: đúng **3** customer-counted attempts trong 15 phút.

[`docs/documents/4. phase/phase-8/16-YÊU CẦU PHI CHỨC NĂNG.md:26`](<../../../docs/documents/4. phase/phase-8/16-YÊU CẦU PHI CHỨC NĂNG.md>):

> Golden Hour | 2 customer-counted attempts trong **10 phút**, offsets `0`, `300` giây.
> 24/7 | **3** customer-counted attempts trong 15 phút, offsets `0`, `300`, `600` giây.

Dev Sales đã nêu xung đột này là `OWNER_DECISION_REQUIRED`.

## 2. Target delta — chính xác là gì

Bốn con số lệch nhau, không phải một:

| Thông số | `D-10` + code hiện tại | Business docs phase-8 | Lệch |
| --- | --- | --- | --- |
| GH window | **5 phút** | **10 phút** | gấp đôi |
| GH offset lần 2 | **150 giây** | **300 giây** | gấp đôi |
| 24/7 số attempt | **2** | **3** | thêm một lần gọi khách |
| 24/7 offsets | `0`, `450` | `0`, `300`, `600` | khác cả số lượng lẫn thời điểm |

Hai hệ quả cụ thể:

**(a) Nếu bản business đúng, IVR đang gọi thiếu một lần cho mọi đơn 24/7.** Không lỗi, không alert — chỉ là tỉ lệ xác nhận thấp hơn mức business kỳ vọng, và không ai truy được vì sao.

**(b) Nếu bản `D-10` đúng, hai tài liệu business đang mô tả một hệ thống không tồn tại.** Bất kỳ ai đọc chúng để viết test hay để báo cáo KPI đều sẽ lệch.

**(c) `attempt_policy_version` là field bắt buộc trên task, do Sales phát.** Nghĩa là Sales tuyên bố phiên bản policy, IVR tra trong registry. Nếu Sales phát một version IVR không có, intake fail-closed (đã có test: `CreateTask(policyVersion: "not-yet-present")` trong `TaskIntakePersistenceTests.cs`). Cần chốt: **ai là nguồn chân lý của bảng policy** — Sales phát cả tham số, hay chỉ phát version còn IVR giữ bảng? Hiện IVR giữ bảng.

**(d) `max_customer_attempts` và `attempt_offsets_seconds` cũng nằm trên task.** Tức là task mang **cả version lẫn tham số**. Nếu hai thứ mâu thuẫn nhau, ai thắng? Hiện chưa có quy tắc nào trên wire.

## 3. Sample payload

```json
{
  "attempt_policy_version": "mock-lab-v1",
  "max_customer_attempts": 2,
  "attempt_offsets_seconds": [0, 150],
  "confirmation_window_started_at": "2026-08-18T03:00:00Z",
  "confirmation_window_expires_at": "2026-08-18T03:05:00Z"
}
```

Nếu bản business phase-8 được chọn cho 24/7, payload thành:

```json
{
  "attempt_policy_version": "<version production đã ký>",
  "max_customer_attempts": 3,
  "attempt_offsets_seconds": [0, 300, 600],
  "confirmation_window_started_at": "2026-08-18T03:00:00Z",
  "confirmation_window_expires_at": "2026-08-18T03:15:00Z"
}
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `PolicyAndContactFailuresCreateNoJob` | [`tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs:81`](../../../tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs) | Version không có trong registry → không tạo job |
| `IT-ELIG-SCHED-09` | `tests/Ivr.IntegrationTests/EligibilityPersistenceTests.cs` | Lịch attempt đúng offsets, nằm trong window |
| `ck_ivr_call_attempts_technical_not_counted` | migration `20260812142435` | `DT-02` enforce ở database |
| **`CDC-POLICY-01`** *(Sales viết)* | producer phía Sales | `attempt_policy_version` phát ra luôn là version đã ký, và tham số kèm theo khớp bảng |

## 5. Mock fallback

`mock-lab-v1` được đánh dấu `CandidateMockLabOnly` ngay trong kiểu dữ liệu — không phải comment, mà là giá trị enum. Feature flag `MockLabAttemptPolicy` và seed bootstrap trong migration đều trỏ về nó. Chạy MOCK/LAB được; **production thì không**.

## 6. Closure artifact — owner điền

- [ ] **`attempt_policy_version` production đã ký**, kèm bảng đầy đủ: mỗi program → số attempt, offsets, window.
- [ ] **Giải quyết xung đột `D-10` vs phase-8**: chọn một bộ số. Nếu chọn bộ business, IVR sửa registry; nếu chọn `D-10`, **owner business** sửa hoặc gắn banner cho hai file phase-8 — **IVR không sửa `docs/documents/`**.
- [ ] **Chốt nguồn chân lý**: Sales phát tham số hay chỉ phát version.
- [ ] **Quy tắc khi version và tham số mâu thuẫn** trên cùng một task.

## 7. Rủi ro nếu để mở

Không chặn build, không chặn lab. Chặn **production** — và chặn cả khả năng nói chuyện về hiệu quả: chừng nào chưa chốt, mọi báo cáo tỉ lệ xác nhận đều đo trên một policy chưa ai duyệt, nên không so sánh được với kỳ vọng business.
