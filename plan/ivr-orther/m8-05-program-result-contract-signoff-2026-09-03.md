# M8-05 — Program/result contract sign-off

**Work ID:** `W-0145`

**Baseline kiểm tra:** `main@b21ec676e490`

**Trạng thái:** **`M8_OWNER_SIGNED / PROGRAM_CONTRACT_LOCKED / RESULT_CONTRACT_LOCKED / M3_PRODUCT_SIGNOFF_REQUIRED / PRODUCTION_POLICY_PENDING`**

**Người ký phía Module 8:** **Tôi — Module 8 / Project Owner** · **2026-09-03**

**External signature/artifact:** **NOT_RECEIVED**

> Module 8 ký phần contract mà Module 8 sở hữu. Chữ ký này không biến proposal thành production
> policy, không chứng minh Module 3 đã có producer/consumer và không cho phép gọi khách thật.

## 1. Kết luận bắt buộc

1. **Business authority:** Module 3 quyết định `CALL_REQUIRED`; Module 8 validate, thực thi và báo
   kết quả. Module 8 không phân loại khách/đơn để đảo quyết định của M3.
2. **Program matrix:** chỉ `GOLDEN_HOUR + ONLINE` và `TWENTY_FOUR_SEVEN + COD` được nhận.
3. **Wire mapping:** M3 map `24_7 → TWENTY_FOUR_SEVEN`, `PHONE_VALID → VALID` và
   `ELIGIBLE_FOR_IVR → ELIGIBLE` tại producer. IVR không nhận alias.
4. **Result contract:** giữ đúng 11 code hiện hành. Không thêm `IVR_OPT_OUT`, không đổi
   `REJECTED` thành opt-out và không phát minh alias result mới.
5. **Production policy:** chưa được ký. `mock-lab-v1` không phải production policy.

## 2. Program/task contract đã khóa phía Module 8

| M3 business value | IVR wire value | Trạng thái |
| --- | --- | --- |
| `GOLDEN_HOUR` + `ONLINE` | giữ nguyên | Receiver đã enforce |
| `24_7` + `COD` | `TWENTY_FOUR_SEVEN` + `COD` | M3 phải map trước khi gửi |
| `ivr_confirmation_required` | `true` | Assertion `CALL_REQUIRED`; thiếu/false bị reject |

M3 không được gửi task rồi chờ callback chỉ vì HTTP thành công. Chỉ
`TASK_ACCEPTED_CALL_JOB_CREATED` nghĩa là IVR đã nhận trách nhiệm gọi; mọi decision khác M3 phải
tự xử lý tiếp.

Nguồn kiểm soát: [T-01 hiện hành](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md)
và [IR-06 §3.10–3.11](../../integration-requirements/06-module-3-api-handover.md).

## 3. Result taxonomy hiện hành: 11 / 9 / 6 / 2

- **11 code trong contract dùng chung.** Đây là enum compatibility, không phải tuyên bố cả 11 đều
  được gửi như callback.
- **9 code có producer path trong IVR runtime.** Chúng có thể được persist thành call result.
- **6 code final được đưa vào callback outbox.** Ba code non-final chỉ ghi nhận tiến trình/lỗi.
- **2 code blocked không phải call result.** Chúng là quyết định pre-call/compatibility và mapper
  outbound phải fail-closed.

| `result_type` | IVR persist | Counted | Final | Callback | Semantics/action phía Core |
| --- | --- | --- | --- | --- | --- |
| `IVR_CONFIRMED` | Có | Có | Có | Có | Revalidate rồi cân nhắc confirm; IVR không đổi order state |
| `IVR_CUSTOMER_CANCELLED` | Có | Có | Có | Có | Revalidate rồi xử lý yêu cầu cancel; không tự hủy |
| `IVR_NO_ANSWER_ATTEMPT` | Có | Có | Không | Không | Còn lượt; giữ state/chờ policy |
| `IVR_NO_ANSWER_FINAL` | Có | Có | Có | Có | Hết lượt; `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Có | Không | Có | Có | Scheduler IVR đóng window; expire nếu đã có counted attempt, nếu chưa thì hold admin review |
| `IVR_INVALID_PHONE_FINAL` | Có | Không | Có | Có | Revalidate và hold admin review |
| `IVR_WRONG_INPUT` | Có | Có | Không | Không | Còn lượt; tới final attempt được normalize thành `IVR_NO_ANSWER_FINAL` |
| `IVR_TECHNICAL_EXCEPTION` | Có | Không | Không | Không | Bounded technical retry hoặc hold review; không giả thành customer attempt |
| `IVR_CAPACITY_EXCEPTION` | Có | Không | Có | Có | Revalidate và hold admin review; không đổ lỗi cho khách |
| `IVR_OPERATIONAL_BLOCKED` | **Không** | Không | N/A | **Không** | Pre-call decision/error; không tạo result |
| `IVR_POLICY_BLOCKED` | **Không** | Không | N/A | **Không** | Pre-call decision; không tạo result |

Correction bắt buộc của W-0145: tài liệu cũ nói `IVR_CONFIRMATION_WINDOW_EXPIRED` do timeout worker
Sales sở hữu và IVR chỉ sinh tám result. Điều đó không còn đúng sau runtime change `f291f449`:
`PostgresSchedulerStore.CloseMissedDeadlinesAsync` tự persist result final và tạo callback snapshot.
Sales/Order Core vẫn sở hữu **revalidation và order-state transition** khi nhận callback; IVR chỉ
phát signal/advisory.

## 4. Module 3 / Product phải ký và giao artifact

### 4.1. Task producer

- Commit assembler chứa ba mapping chuỗi ở §1.
- CDC cho hai business pair, `ivr_confirmation_required=true`, response-shape branching và
  `TASK_ACCEPTED_CALL_JOB_CREATED` responsibility boundary.
- Quy tắc bump `order_version` và minimal eligibility snapshot.

### 4.2. Result consumer

- OpenAPI + implementation endpoint generic
  `/api/v1/internal/orders/{orderId}/ivr-result-callbacks` cho cả hai program.
- Consumer chấp nhận đúng taxonomy 11 code nhưng không kỳ vọng hai blocked code từ callback.
- ACK/idempotency/revalidation semantics; tôn trọng `is_counted_customer_attempt` và
  `is_final_for_ivr` thay vì tự suy từ số callback.
- Shared CDC/E2E cho `ACCEPTED`, duplicate, stale, blocked, auth failure và retryable failure.
- Bảng compatibility 11 → 4 của đường Golden Hour cũ, ghi rõ mất mát; không được dùng compat để
  giả rằng 24/7 đã có callback path.

### 4.3. Production attempt policy

- Product/Order Core ký version, window, attempt count và offsets cho từng program.
- Chọn một nguồn chân lý và xử lý dứt điểm xung đột `D-10` với phase-8 business docs.
- Producer CDC chứng minh version và parameters luôn khớp.

Phần này thuộc [T-09](../../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md) và
M8-11. Cho tới khi có chữ ký, `G-POLICY` giữ `BLOCKED_EXTERNAL`.

## 5. Phản hồi bị từ chối

- “OK”, “đồng ý”, “dev tự map” nhưng không có tên owner, commit/OpenAPI và test.
- Yêu cầu IVR nhận thêm alias để che lỗi assembler phía M3.
- Tự thêm result code hoặc đổi semantics để khớp UI/CRM chưa có contract.
- Gọi `mock-lab-v1` là production policy.
- Dùng unit/local/WireMock test phía IVR để tuyên bố shared integration đã xong.
- Gộp “M8 đã ký” thành “M3/Product/Security đã duyệt”.

## 6. Exit status

Phần local của M8-05 hoàn tất khi docs/contract/test gates của W-0145 pass. Trạng thái bàn giao sau
đó là:

**`M8_OWNER_SIGNED / M3_PRODUCT_SIGNOFF_REQUIRED / PRODUCTION_POLICY_PENDING / SHARED_E2E_NOT_RUN`**

Không nâng `ACCEPTED`, không đóng `G-CONTRACT`/`G-POLICY`, không bật Target callback hoặc
`REAL_CUSTOMER_CALL_ALLOWED` nếu chưa có external artifact.

## 7. Chữ ký

| Bên | Người ký | Ngày | Phạm vi |
| --- | --- | --- | --- |
| Module 8 / Project Owner | **Tôi — Module 8 / Project Owner** | **2026-09-03** | Program receiver, 11-result semantics, stop rule và handoff |
| Module 3 contract/business owner | `<chưa nhận>` | `<chưa nhận>` | Task producer, wire mapping, callback consumer, revalidation/CDC |
| Product / Order Core | `<chưa nhận>` | `<chưa nhận>` | Production attempt policy/version |
| Security / Platform | `<chưa nhận>` | `<chưa nhận>` | Auth, sandbox credential, network/custody evidence |
