# T-01 — Ma trận program / payment / IVR-required / callable

External work `W-0002` · quyết định `OD-V1-01`, `OD-V1-13`, `OD-V1-14` · gate **real integration** · trạng thái `OPEN`

Owner: **Sales Product/Core** (ma trận + producer) và **Product/Business** (scope Golden Hour ONLINE, field `ivr_confirmation_required`).

Due: chốt **trước khi bắt đầu `P4-2`** — mọi slice adapter đều đọc ma trận này. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Business source hiện đọc được là COD-only.** [`plan/ivr-orther/decisions-log.md:91`](../../../plan/ivr-orther/decisions-log.md) `DS-01`, source-read từ Sales platform:

> IVR-callable = CHỈ `CONFIRMING` VÀ CHỈ khi `payment_method_snapshot=COD`. Mọi state khác + mọi đơn non-COD = không callable. `is_ivr_callable` **không phải field** — là rule derive từ state machine.

**IVR hiện enforce một ma trận khác, ở bốn nơi độc lập:**

| Nơi enforce | Vị trí | Luật |
| --- | --- | --- |
| OpenAPI (wire) | [`specs/api/openapi/ivr-order-confirmation.v1.yaml:839`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) `oneOf` | `(GOLDEN_HOUR ∧ ONLINE) ∨ (TWENTY_FOUR_SEVEN ∧ COD)` |
| Intake endpoint | [`src/Ivr.Api/Intake/TaskIntakeEndpoint.cs:209`](../../../src/Ivr.Api/Intake/TaskIntakeEndpoint.cs) | cùng luật, `422` nếu sai |
| Eligibility | [`src/Ivr.Domain/Policies/EligibilityRules.cs:139`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs) | `PROGRAM_PAYMENT_MATRIX_REJECTED` |
| Database | migration `20260812142435` — `ck_ivr_confirmation_tasks_matrix` | CHECK constraint, không ghi được row sai |

Bốn nơi này **nhất quán với nhau** và đều đang mã hoá một **đề xuất chưa được duyệt**.

## 2. Target delta — chính xác là gì

Ma trận IVR và `DS-01` lệch nhau theo **hai chiều ngược nhau**. Cả hai đều nguy hiểm:

| Cặp giá trị | `DS-01` (business) | IVR hiện tại | Hậu quả nếu chạy thật |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` + `COD` | **callable** | **từ chối `422`** | Sales đẩy task hợp lệ, IVR chặn hết. Một lớp đơn không bao giờ được gọi, im lặng. |
| `GOLDEN_HOUR` + `ONLINE` | **không callable** | **chấp nhận** | IVR gọi khách trên nhóm đơn business chưa cho phép. |
| `TWENTY_FOUR_SEVEN` + `COD` | callable | chấp nhận | khớp |
| `TWENTY_FOUR_SEVEN` + `ONLINE` | không callable | từ chối | khớp |

**`OD-V1-14` — `ivr_confirmation_required` không có nguồn business.** `grep -rl ivr_confirmation_required "docs/documents/"` → **0 file**. Nhưng OpenAPI khai `enum: [true]` và intake từ chối `422` nếu thiếu hoặc `false`. Nếu producer của Sales không set field này, **100% task bị từ chối** ngay ngày cắm thật.

`is_ivr_callable` cũng vậy: `DS-01` nói đây **không phải field**, OpenAPI khai nó là optional convenience flag. Không xung đột về hành vi, nhưng cần xác nhận Sales có phát nó không, và nếu có thì derive từ đâu.

## 3. Sample payload

Task tối thiểu hợp lệ theo contract hiện tại (đã lược field không liên quan ticket này):

```json
{
  "contract_version": "ivr-order-confirmation.v1",
  "task_id": "TASK-0001",
  "order_id": "ORDER-0001",
  "order_code": "GF-2026-0001",
  "order_version": "17",
  "order_state": "CONFIRMING",
  "program_code": "GOLDEN_HOUR",
  "payment_method_snapshot": "ONLINE",
  "ivr_confirmation_required": true,
  "is_ivr_callable": true
}
```

Đổi `payment_method_snapshot` thành `COD` mà giữ `program_code: GOLDEN_HOUR` → `422` ở cả bốn tầng bảng §1.

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `TaskIntakeServiceTests` theory 2 nhánh | [`tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs:24`](../../../tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs) | Hai cặp giá trị được duyệt đều tạo đúng 1 call job |
| `IT-INTAKE-DB-01/02` | `tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs` | Ghi bền + idempotency trên Postgres thật |
| `CT-INTAKE-OPENAPI-01` | `tests/Ivr.ContractTests/TaskIntakeContractTests.cs` | Payload khớp schema đã ghim |
| **`CDC-MATRIX-01`** *(Sales phải viết)* | producer phía Sales | Producer chỉ phát đúng những cặp giá trị trong ma trận đã ký, kèm `ivr_confirmation_required` |

Khi ma trận được ký khác với hiện tại, **cả bốn nơi ở §1 phải sửa cùng lúc** — sửa lệch một nơi là tạo lỗ hổng im lặng.

## 5. Mock fallback — IVR đang chạy bằng gì

Fake Sales producer phát cả hai cặp giá trị đã duyệt; toàn bộ Phase 2–3 chạy trên đó. Fixture MOCK **không** đóng gate.

## 6. Closure artifact — owner điền

Đóng `OD-V1-01`, `OD-V1-13`, `OD-V1-14` cần **cả ba**:

- [ ] **Ma trận đã ký**: bảng `program_code × payment_method_snapshot × order_state → IVR-callable`, có tên người duyệt và ngày. Trả lời dứt khoát: `GOLDEN_HOUR + COD` có callable không, và `GOLDEN_HOUR + ONLINE` có thuộc scope IVR V1 không.
- [ ] **Định nghĩa `ivr_confirmation_required`**: nguồn business, ai set, set khi nào, có bao giờ `false` không. Nếu field bị bỏ, phải nói rõ IVR gate bằng gì thay thế.
- [ ] **Producer test đã merge** phía Sales chứng minh producer chỉ phát đúng ma trận đã ký.

Đóng bằng "dev Sales nói vậy" là **không hợp lệ** — cần chữ ký owner Product/Business cho `OD-V1-13` và `OD-V1-14`.

## 7. Rủi ro nếu để mở

Đây là ticket **chặn cứng ngày cắm thật**. Hai chiều lệch ở §2 không gây lỗi ồn ào: chiều thứ nhất làm đơn biến mất khỏi hàng đợi mà không ai báo, chiều thứ hai gọi khách ngoài scope đã duyệt. Cả hai chỉ lộ ra khi đã chạy trên khách thật.
