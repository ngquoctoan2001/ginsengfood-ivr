# T-01 — Ma trận program / payment / IVR-required / callable

External work `W-0002` · quyết định `OD-V1-01`, `OD-V1-13`, `OD-V1-14` · gate **real integration**

Trạng thái: **`M8_POSITION_SIGNED / M3_PRODUCT_ARTIFACT_PENDING`**

Rà soát hiện hành: `W-0145` · `2026-09-03` · baseline `main@b21ec676e490`

Owner phải trả artifact: **Module 3 / Product / Order Core**.

> **Correction W-0145:** bản cũ của ticket này nói business pair chưa được duyệt và yêu cầu
> quyết định lại Golden Hour COD/ONLINE. Kết luận đó đã lỗi thời. IR-06 ghi nhận nguồn Flow 04/05
> phía Module 3 đã khóa hai cặp business. Không dùng lại phần phân tích cũ để giao Module 8 sửa
> matrix hoặc nới schema.

## 1. Vị trí đã ký của Module 8

Module 8 nhận đúng hai tổ hợp sau:

| `program_code` trên wire IVR | `payment_method_snapshot` | Kết quả |
| --- | --- | --- |
| `GOLDEN_HOUR` | `ONLINE` | Được nhận nếu các gate task khác hợp lệ |
| `TWENTY_FOUR_SEVEN` | `COD` | Được nhận nếu các gate task khác hợp lệ |
| `GOLDEN_HOUR` | `COD` | Reject tại schema; không phải business pair được duyệt |
| `TWENTY_FOUR_SEVEN` | `ONLINE` | Reject tại schema; không phải business pair được duyệt |

Đây là receiver contract hiện hành. Module 8 **không** nhận alias `24_7`, không tự suy payment và
không tự quyết định đơn nào cần gọi.

Module 3 phải map tại producer/assembler:

| Giá trị phía Module 3 | Giá trị phải gửi sang IVR |
| --- | --- |
| `24_7` | `TWENTY_FOUR_SEVEN` |
| `PHONE_VALID` | `VALID` |
| `ELIGIBLE_FOR_IVR` | `ELIGIBLE` |

`ivr_confirmation_required` là assertion rằng Module 3 đã quyết định `CALL_REQUIRED`:

- đơn không cần gọi: **không gửi task**;
- task được gửi: field phải có giá trị `true`;
- thiếu field hoặc gửi `false`: schema từ chối; IVR không biến cờ này thành business-decision engine.

## 2. Evidence hiện hành

| Lớp | Evidence | Kết luận |
| --- | --- | --- |
| Business routing | [IR-06 §3.10–3.11](../../../integration-requirements/06-module-3-api-handover.md) ghi nhận Flow 04/05 phía M3 | `24_7 + COD`; `GOLDEN_HOUR + ONLINE`; còn thiếu producer artifact/chữ ký M3 |
| Wire schema | [OpenAPI IVR](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) | `oneOf` chỉ nhận hai cặp; `ivr_confirmation_required` chỉ nhận `true` |
| Runtime receiver | `ProgramPaymentPolicy` + `TargetV1TaskMapper` | Enforce cùng matrix; policy version/parameters phải khớp registry snapshot |
| Contract/runtime tests | `CT-INTAKE-OPENAPI-01`, `IT-INTAKE-DB-01/02`, intake tests | Chứng minh phía IVR trên local/test; không chứng minh producer M3 |

Nguồn Flow 04/05 nằm ở repository Module 3 và được IR-06 dẫn chiếu. Module 8 không thay chữ ký của
owner nguồn đó; M3 phải trả commit/source reference hiện hành trong artifact sign-off.

## 3. Module 3 / Product phải trả gì

Không trả lời bằng “OK”. Ticket chỉ đủ điều kiện đóng khi có đủ:

- [ ] Bảng matrix/wire mapping ở §1 có tên owner, vai trò và ngày ký.
- [ ] Commit assembler phía M3 chứng minh ba mapping chuỗi ở §1.
- [ ] Producer CDC chứng minh chỉ phát hai business pair và luôn gửi
  `ivr_confirmation_required=true` cho task `CALL_REQUIRED`.
- [ ] Test chứng minh producer rẽ nhánh theo HTTP/response shape và chỉ chờ callback sau
  `TASK_ACCEPTED_CALL_JOB_CREATED`.
- [ ] Mô tả thời điểm bump `order_version` và minimal eligibility snapshot đã ký.
- [ ] Production `attempt_policy_version` đã được Product/Order Core ký theo
  [T-09](T-09-attempt-policy.md); version và parameters khớp nhau.

Mục cuối **chưa được Module 8 phê duyệt**. `mock-lab-v1` chỉ là candidate MOCK/LAB và không được
đưa vào production bằng cách gộp chữ ký T-01 với chữ ký policy.

## 4. Stop rule

- Không nới IVR để nhận `24_7`, `PHONE_VALID`, `ELIGIBLE_FOR_IVR` hoặc business pair ngoài bảng.
- Không yêu cầu Product quyết lại business pair đã có nguồn; việc còn thiếu là artifact producer
  và chữ ký chịu trách nhiệm.
- Không triển khai real integration khi chưa có producer CDC, production policy và sandbox/auth.
- Không dùng local IVR tests để tuyên bố M3 đã tích hợp hoặc contract đã `ACCEPTED`.

## 5. Chữ ký

| Bên | Người ký | Ngày | Phạm vi |
| --- | --- | --- | --- |
| Module 8 / Project Owner | **Tôi — Module 8 / Project Owner** | **2026-09-03** | Receiver matrix, wire values và stop rule; không ký thay producer/policy owner |
| Module 3 contract owner | `<chưa nhận>` | `<chưa nhận>` | Producer mapping, lifecycle và CDC |
| Product / Order Core | `<chưa nhận>` | `<chưa nhận>` | Production attempt policy/version |

Vì hai chữ ký external còn trống, trạng thái đúng vẫn là
**`M8_POSITION_SIGNED / M3_PRODUCT_ARTIFACT_PENDING`**, không phải `SIGNED_OFF` toàn hệ thống.
