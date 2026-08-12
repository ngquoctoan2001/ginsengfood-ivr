# BẢNG GÔM PHASE 8

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: PHASE_8_SRS_DEV_HANDOFF_READY
Phạm vi: Gộp toàn bộ nội dung IVR theo đúng thứ tự đọc của Phase 8 để dev bóc task và triển khai có kiểm soát.
Nguồn hiện hành: docs/documents/4. phase/phase-8/00-... đến 25-...; các nhãn IVR-00...IVR-21 trong nội dung là mã module SRS, không phải đường dẫn file hiện hành.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev triển khai Phase 8 IVR. Các cụm release gate, smoke, evidence, real customer call block hoặc `PRODUCTION_READY = NO` là guard bắt buộc sau triển khai, không phải lỗi dev-readiness.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES
READY_TO_HAND_TO_DEV: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## Mục lục nguồn gộp

- IVR-00: IVR-00-governance-source-of-truth-scope-boundary.md
- IVR-01: IVR-01-business-purpose-confirmation-use-case.md
- IVR-02: IVR-02-ownership-boundary-connected-systems.md
- IVR-03: IVR-03-eligibility-customer-trust-official-contact.md
- IVR-04: IVR-04-order-core-to-ivr-task-contract.md
- IVR-05: IVR-05-attempt-policy-scheduler-queue.md
- IVR-06: IVR-06-internal-sim-gateway-adapter.md
- IVR-07: IVR-07-result-normalization-order-core-callback.md
- IVR-08: IVR-08-admin-monitoring-evidence-audit-privacy.md
- IVR-09: IVR-09-test-matrix-smoke-release-gate.md

---

<!-- SOURCE_FILE: IVR-00-governance-source-of-truth-scope-boundary.md -->

# IVR-00 - Governance / Source of Truth / Scope Boundary

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa governance, nguồn sự thật và ranh giới phạm vi trước khi viết contract hoặc triển khai.

## 1. Mục tiêu

Tài liệu này xác định IVR là một hợp phần xác nhận đơn hàng bằng cuộc gọi tự động, không phải hệ thống bán hàng, không phải hệ thống chăm sóc khách hàng đại trà và không phải lớp quyết định trạng thái đơn hàng.

IVR chỉ được vận hành sau khi có Official Order đủ điều kiện từ Commerce Order Core. Kết quả IVR chỉ là tín hiệu đầu vào để Order Core revalidate và quyết định transition.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                              |
| ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------- |
| `docs/source-map.md`                                                                                         | Đường dẫn nguồn hợp lệ cho traceability.             |
| `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md`                                                   | Source-of-truth, owner, resolver, consumer boundary. |
| `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md`                                                   | Correlation, idempotency, evidence trace.            |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md`                                    | Evidence, smoke, completion gate.                    |
| `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md`                                           | Release/go-live gate.                                |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Pack source chính của IVR.                           |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Technical source chính của IVR.                      |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`                                                 | Baseline gap-closure tiền SRS.                       |

## 3. Thuật ngữ

| Thuật ngữ      | Định nghĩa                                                                 |
| -------------- | -------------------------------------------------------------------------- |
| IVR            | Hợp phần gọi tự động xác nhận đơn hàng bằng SIM Gateway nội bộ.            |
| Order Core     | Commerce Order Core / Order State Machine, chủ sở hữu trạng thái đơn hàng. |
| Official Order | Đơn chính thức đã được Commerce tạo từ Customer Confirmation hợp lệ.       |
| IVR task       | Yêu cầu nội bộ do Order Core tạo để IVR xem xét/gọi xác nhận.              |
| IVR result     | Kết quả đã normalize từ cuộc gọi IVR.                                      |
| Evidence       | Bằng chứng có trace, owner, trạng thái và audit.                           |
| Release gate   | Cổng kiểm soát trước khi cho phép gọi khách thật.                          |

## 4. Phạm vi

Trong phạm vi:

- Xác nhận ý chí đặt hàng bằng phím `1`.
- Ghi nhận yêu cầu hủy/không đặt bằng phím `0`.
- Ghi nhận no-answer theo attempt policy.
- Ghi nhận invalid phone theo policy.
- Ghi nhận technical exception.
- Gửi callback kết quả về Order Core.
- Ghi audit/evidence cho task, attempt, result, admin action và incident.

Ngoài phạm vi:

- Tạo Quote, Cart, Order Draft hoặc Official Order.
- Sửa giá, tồn kho, chương trình, quyền lợi thành viên, phí ship, payment, MISA.
- Tự xác nhận `PAID`, `DELIVERED`, `VERIFIED_REVENUE`.
- Tự hủy đơn.
- Tự gửi SMS/thông báo sau từng attempt hoặc sau no-answer max.
- Marketing, upsell, CRM, chăm sóc khách hàng đại trà.

## 5. Source-of-truth

| Nhóm sự thật                 | Owner                                   | IVR được làm                         | IVR bị cấm                             |
| ---------------------------- | --------------------------------------- | ------------------------------------ | -------------------------------------- |
| Trạng thái đơn               | Order Core                              | Đọc task và gửi result signal        | Tự transition đơn                      |
| Official Order               | Commerce Runtime                        | Chỉ xử lý order đã đủ điều kiện      | Gọi Quote/Cart/Order Draft             |
| Sale Lock/Recall/Suppression | Operational Core                        | Consume trạng thái block             | Override hoặc bỏ qua block             |
| Customer trust               | Customer Trust/Customer Memory Resolver | Consume quyết định trusted/untrusted | Hardcode trusted customer              |
| Official contact             | Customer/Commerce Profile               | Gọi contact được duyệt               | Đọc full profile hoặc full address     |
| Payment/MISA/Revenue         | Payment/MISA/Finance/Commerce           | Không xử lý trực tiếp                | Xác nhận thanh toán/kế toán/doanh thu  |
| Evidence/Audit               | Foundation/Evidence Registry            | Ghi evidence/audit theo policy       | Tự gọi PASS khi evidence chưa accepted |

## 6. Baseline governance

| Rule                                          | Giá trị                            |
| --------------------------------------------- | ---------------------------------- |
| `CALL_PURPOSE`                                | `ORDER_CONFIRMATION_ONLY`          |
| `IVR_DEPLOYMENT_MODEL`                        | `INTERNAL_SIM_GATEWAY_SERVER`      |
| `MAX_ATTEMPT_PER_ORDER`                       | `PROGRAM_BASED`                    |
| Golden Hour                                   | 2 cuộc trong 10 phút               |
| 24/7                                          | 3 cuộc trong 15 phút               |
| `IVR_RESULT_IS_INPUT_SIGNAL_ONLY`             | `YES`                              |
| `ORDER_STATE_MACHINE_IS_FINAL_DECISION_LAYER` | `YES`                              |
| `REAL_CUSTOMER_CALL_ALLOWED`                  | `NO` cho đến khi release gate pass |

## 7. Yêu cầu chức năng

| ID           | Yêu cầu                                                                                                   |
| ------------ | --------------------------------------------------------------------------------------------------------- |
| IVR00-FR-001 | Hệ thống phải chặn mọi task không đến từ Order Core hoặc không có correlation/idempotency hợp lệ.         |
| IVR00-FR-002 | Hệ thống phải từ chối Quote, Cart, Order Draft và mọi entity không phải Official Order.                   |
| IVR00-FR-003 | Hệ thống phải coi mọi kết quả IVR là signal, không phải quyết định trạng thái đơn.                        |
| IVR00-FR-004 | Hệ thống phải dừng hoặc hold IVR khi Sale Lock, Recall, Suppression, opt-out hoặc policy block xuất hiện. |
| IVR00-FR-005 | Hệ thống phải ghi evidence/audit cho task, attempt, result, callback, exception và admin action.          |
| IVR00-FR-006 | Hệ thống phải giữ production disabled cho đến khi IVR-09 release gate pass.                               |

## 8. Yêu cầu phi chức năng

- Idempotency bắt buộc cho task intake, attempt dispatch và callback.
- Correlation bắt buộc xuyên suốt Order Core, IVR, SIM Gateway Adapter, Evidence Registry.
- Fail-safe khi source-of-truth hoặc policy resolver không khả dụng.
- Không lộ PII trong log/UI ngoài phạm vi được duyệt.
- Không có hardcoded PASS/Release Ready/Production Ready.

## 9. Cổng chặn lỗi

Phase 8 FAIL nếu:

- IVR có đường tự update order.
- IVR có đường gọi Quote/Cart/Order Draft.
- IVR gọi khách trusted đại trà.
- IVR bỏ qua Sale Lock/Recall/Suppression.
- IVR trộn no-answer với invalid phone hoặc technical failure.
- IVR tự gửi notification.
- Tài liệu hoặc implementation nói production ready khi chưa có smoke/evidence/owner sign-off.

## 10. Tiêu chí chấp nhận

Tài liệu này đạt khi:

- Các ranh giới owner/source-of-truth đã rõ.
- Các rule baseline khớp PACK-09, TECH-09 và Phase 8 baseline.
- Không có quyết định nghiệp vụ nào bị giao cho implementer tự suy diễn.
- Mọi điểm chưa có nguồn được ghi `Owner Decision Required`.

## 11. Phân rã phạm vi triển khai chi tiết

| Nhóm phạm vi       | Trong Phase 8                                                      | Ngoài Phase 8                                   | Ghi chú triển khai                                |
| ------------------ | ------------------------------------------------------------------ | ----------------------------------------------- | ------------------------------------------------- |
| Order confirmation | Gọi xác nhận Official Order bằng IVR.                              | Tư vấn sản phẩm, upsell, CRM, chăm sóc sau bán. | Call script chỉ phục vụ xác nhận đơn.             |
| Order ownership    | Gửi tín hiệu cho Order Core.                                       | IVR tự confirm/cancel/expire order.             | Order Core là lớp quyết định cuối.                |
| Attempt policy     | Golden Hour 2 cuộc/10 phút; 24/7 3 cuộc/15 phút.                   | Tự thêm attempt, tự kéo dài window.             | Policy phải nằm trong resolver/config được duyệt. |
| Contact            | Dùng official contact projection.                                  | Đọc full profile/full address/CRM note.         | Ưu tiên `phone_ref`, `phone_masked`, dial token.  |
| Evidence           | Ghi audit/evidence cho task/attempt/result/admin.                  | Tự mark release PASS.                           | Evidence Registry/Foundation quyết định accepted. |
| Admin              | Monitor, pause/resume, disable/enable SIM, manual technical retry. | Admin force order state.                        | Admin action không bypass blocker.                |
| SIM Gateway        | Internal SIM Gateway Server.                                       | Cloud IVR/SIP/brandname làm mặc định.           | Provider khác là future owner decision.           |

## 12. Source-of-truth decision table

| Quyết định                          | Source-of-truth                 | IVR consume như thế nào                                 | Khi source không khả dụng                                                             |
| ----------------------------------- | ------------------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| Entity có phải Official Order không | Commerce Order Core             | Task payload + revalidation callback.                   | Không tạo/gọi task.                                                                   |
| Order còn nhận IVR result không     | Order Core                      | Callback ack/reject.                                    | Retry bounded hoặc admin review.                                                      |
| Khách có trusted skip không         | Customer Trust Resolver         | Snapshot trong task/eligibility.                        | Không tự hardcode; route review hoặc require IVR theo policy an toàn nếu owner duyệt. |
| Phone có hợp lệ không               | Official Contact/Phone Resolver | `phone_validation_status`.                              | Không dispatch nếu không có fallback approved.                                        |
| Sale Lock/Recall/Suppression        | Operational Core                | Pre-dispatch check và Order Core callback revalidation. | Không dispatch.                                                                       |
| Payment issue                       | Payment/Order Core              | Order Core revalidate khi callback.                     | Không confirm order.                                                                  |
| Evidence accepted                   | Evidence Registry               | Evidence refs.                                          | Không mở release/pass.                                                                |
| Production ready                    | Release Owner                   | Release gate evidence.                                  | `REAL_CUSTOMER_CALL_ALLOWED = NO`.                                                    |

## 13. Governance state gates

| Gate                                | Trạng thái mặc định | Điều kiện mở                                              |
| ----------------------------------- | ------------------- | --------------------------------------------------------- |
| `IVR_DOCS_APPROVED`                 | `NO`                | SRS/SDS được owner review.                                |
| `IVR_CONTRACT_APPROVED`             | `NO`                | Contract validator pass và owner accepted.                |
| `IVR_TASK_INTAKE_ENABLED`           | `NO`                | API/auth/idempotency/evidence test pass.                  |
| `IVR_SCHEDULER_ENABLED`             | `NO`                | Scheduler/concurrency/capacity test pass.                 |
| `IVR_SIM_INTERNAL_TEST_ENABLED`     | `NO`                | Internal-number smoke approved.                           |
| `REAL_CUSTOMER_CALL_ALLOWED`        | `NO`                | IVR-09 release gate pass, privacy/security approved.      |
| `DOWNSTREAM_IVR_DEPENDENCY_ALLOWED` | `NO`                | Order Core/Release owner approves downstream consumption. |

## 14. P0 governance rules

| P0           | MUST                                   | MUST NOT                                       | FAIL IF                                                      |
| ------------ | -------------------------------------- | ---------------------------------------------- | ------------------------------------------------------------ |
| IVR00-P0-001 | IVR chỉ xử lý Official Order.          | Không gọi Quote/Cart/Order Draft.              | Có task IVR từ entity chưa phải Official Order.              |
| IVR00-P0-002 | IVR result là input signal.            | Không tự transition order.                     | IVR/SIM có quyền update order state.                         |
| IVR00-P0-003 | Order Core revalidate callback.        | Không accept callback mù.                      | Key `1` làm order confirmed khi Sale Lock/Recall active.     |
| IVR00-P0-004 | Technical failure tách khỏi no-answer. | Không count lỗi kỹ thuật như khách không nghe. | SIM/server/DTMF lỗi tạo no-answer final.                     |
| IVR00-P0-005 | Evidence/audit bắt buộc.               | Không release bằng xác nhận miệng.             | PASS thiếu evidence packet.                                  |
| IVR00-P0-006 | Privacy safe by default.               | Không log raw phone/full profile.              | Raw phone xuất hiện trong app log/admin UI không được duyệt. |

## 15. Traceability requirement

Mỗi implementation task sau này phải có tối thiểu:

- Source document path trong `docs/source-map.md`.
- Requirement ID từ `IVR-00` đến `IVR-09` hoặc SDS `IVR-10` đến `IVR-20`.
- Contract path nếu có: schema/API/event/state-machine.
- Test case hoặc evidence item.
- Owner decision nếu rule chưa được khóa.

## 16. Implementation readiness checklist

| Checklist                  | Required before code |
| -------------------------- | -------------------- |
| Source path hợp lệ         | Có.                  |
| Requirement rõ             | Có.                  |
| Owner/source-of-truth rõ   | Có.                  |
| Boundary với Order Core rõ | Có.                  |
| Privacy impact rõ          | Có.                  |
| Evidence/audit rõ          | Có.                  |
| Fail-safe rõ               | Có.                  |
| Test expected rõ           | Có.                  |

## 17. Residual open decisions

Các quyết định sau không được implementer tự bịa:

- Ngưỡng trusted customer.
- Risk flags bắt buộc vẫn phải IVR.
- Permanent invalid phone criteria.
- Technical retry count/backoff.
- SIM Gateway production protocol.
- Recording policy.
- Retention duration.
- Pilot real customer scope.
- Notification template sau khi Order Core hủy/expire.

---

<!-- SOURCE_FILE: IVR-01-business-purpose-confirmation-use-case.md -->

# IVR-01 - Business Purpose / Confirmation Use Case

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa mục đích nghiệp vụ và các use case xác nhận đơn hàng.

## 1. Mục tiêu

IVR được xây dựng để giảm đơn ảo, đơn sai số điện thoại, đơn đặt nhầm và đơn không xác nhận được, nhưng không làm phiền khách hàng tin cậy một cách đại trà.

IVR chỉ xác nhận ý chí đặt hàng ở mức tối thiểu, không xác nhận thanh toán, xuất kho, giao hàng, doanh thu hoặc kế toán.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                                         |
| ------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Mục đích pack, call script, phím bấm, attempt policy.           |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Eligibility, outcome classification, privacy-safe confirmation. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`      | Official Order không đồng nghĩa Paid/Verified Revenue.          |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md`                                                      | Nội dung cuộc gọi, DTMF và rule chương trình.                   |

## 3. Mục tiêu nghiệp vụ

| ID           | Mục tiêu                                                                                    |
| ------------ | ------------------------------------------------------------------------------------------- |
| IVR01-BG-001 | Xác nhận khách còn muốn tiếp tục xử lý đơn hàng.                                            |
| IVR01-BG-002 | Giảm rủi ro đơn ảo, số điện thoại sai, khách đặt nhầm, khách không phản hồi.                |
| IVR01-BG-003 | Tách rõ khách xác nhận, khách hủy, khách không nghe, số điện thoại invalid và lỗi kỹ thuật. |
| IVR01-BG-004 | Tránh gọi đại trà khách trusted nếu không có tín hiệu rủi ro mới.                           |
| IVR01-BG-005 | Cung cấp evidence cho Order Core ra quyết định, không thay Order Core.                      |

## 4. Tác nhân

| Actor                  | Vai trò                                                       |
| ---------------------- | ------------------------------------------------------------- |
| Khách hàng             | Nhận cuộc gọi, bấm `1` để xác nhận hoặc `0` để hủy/không đặt. |
| Order Core             | Tạo task, nhận callback, revalidate và transition order.      |
| IVR Runtime            | Điều phối queue, gọi, capture DTMF, normalize result.         |
| IVR Operator           | Theo dõi queue, SIM health, lỗi kỹ thuật.                     |
| Admin/Incident Manager | Pause queue, disable SIM, review exception theo permission.   |
| Evidence/Audit Owner   | Kiểm tra evidence, audit, release readiness.                  |

## 5. Tình huống nghiệp vụ chính

### UC-01 - Khách bấm `1`

Điều kiện:

- Official Order đủ điều kiện IVR.
- Phone valid.
- Không có Sale Lock/Recall/Suppression/call restriction.
- Attempt còn trong window.

Kết quả:

- IVR ghi `IVR_CONFIRMED`.
- IVR gửi callback về Order Core.
- Order Core revalidate.
- Nếu vẫn hợp lệ, Order Core chuyển đơn sang bước xử lý tiếp theo theo state machine.

### UC-02 - Khách bấm `0`

Kết quả:

- IVR ghi `IVR_CUSTOMER_CANCELLED`.
- IVR gửi callback về Order Core.
- Order Core revalidate và thực hiện cancel theo state machine.
- IVR không tự cancel order.

### UC-03 - Khách không nghe

Kết quả:

- IVR ghi no-answer attempt nếu attempt hợp lệ.
- Nếu chưa đạt max attempt, scheduler chờ attempt tiếp theo theo chương trình.
- Nếu đạt max attempt, IVR gửi `IVR_NO_ANSWER_FINAL` về Order Core.
- Notification chỉ được gửi bởi Notification Owner sau khi Order Core đã ra quyết định.

### UC-04 - Số điện thoại invalid

Kết quả:

- IVR không gọi nếu phone validation xác định invalid trước dispatch.
- IVR gửi/ghi outcome invalid phone theo policy.
- Invalid phone không được ghi thành no-answer.

### UC-05 - Lỗi kỹ thuật

Kết quả:

- IVR ghi `IVR_TECHNICAL_EXCEPTION`.
- Attempt kỹ thuật không tính là customer attempt.
- Hệ thống route admin review hoặc technical retry trong giới hạn policy.
- Không tự hủy đơn.

## 6. Nội dung cuộc gọi

Call script V1.0 chỉ được dùng biến đã duyệt:

- `order_code_short`.
- `total_amount_display`.
- `customer_name_short` nếu có và policy cho phép.
- `program_name` nếu cần.

Không được đọc:

- Full address.
- Payment detail.
- Member tier.
- Diamond/referral information.
- Health note.
- AI consultation content.
- CRM content.
- Full order history.

## 7. Quy tắc DTMF

| Phím      | Ý nghĩa                           | Outcome                                            |
| --------- | --------------------------------- | -------------------------------------------------- |
| `1`       | Khách xác nhận tiếp tục xử lý đơn | `IVR_CONFIRMED`                                    |
| `0`       | Khách không đặt hoặc muốn hủy     | `IVR_CUSTOMER_CANCELLED`                           |
| Không bấm | Không có xác nhận hợp lệ          | `IVR_NO_ANSWER_ATTEMPT` hoặc `IVR_NO_ANSWER_FINAL` |
| Sai phím  | Input không hợp lệ                | Xử lý theo policy attempt                          |
| Lỗi DTMF  | Lỗi kỹ thuật                      | `IVR_TECHNICAL_EXCEPTION`                          |

`KEY_9_HUMAN_SUPPORT` chưa bật trong V1.0. Nếu mở sau này phải có owner decision, CSKH capacity và SRS update.

## 8. Quy tắc nghiệp vụ

| ID           | Rule                                                                                  |
| ------------ | ------------------------------------------------------------------------------------- |
| IVR01-BR-001 | IVR confirmation không đồng nghĩa `PAID`.                                             |
| IVR01-BR-002 | IVR confirmation không đồng nghĩa `DELIVERED`.                                        |
| IVR01-BR-003 | IVR confirmation không đồng nghĩa `VERIFIED_REVENUE`.                                 |
| IVR01-BR-004 | IVR không được dùng làm kênh marketing, upsell, CRM hoặc chăm sóc khách hàng đại trà. |
| IVR01-BR-005 | Không có evidence thì kết quả IVR không được dùng làm căn cứ transition.              |

## 9. Tiêu chí chấp nhận

- Use case trả lời đủ cho confirm, cancel, no-answer, invalid phone và technical failure.
- DTMF result tách rõ business outcome và technical exception.
- Không có câu nào cho phép IVR tự cập nhật order.
- Call script không chứa dữ liệu riêng tư ngoài phạm vi.

## 10. Business capability breakdown

| Capability                         | Người dùng/hệ thống hưởng lợi | Giá trị nghiệp vụ                      | Không được hiểu là                 |
| ---------------------------------- | ----------------------------- | -------------------------------------- | ---------------------------------- |
| Auto confirm order intent          | Order Core, vận hành đơn      | Giảm đơn ảo, đơn đặt nhầm, phone sai.  | Xác nhận thanh toán.               |
| Customer cancel by key `0`         | Khách hàng, Order Core        | Ghi nhận ý chí không đặt/hủy.          | IVR tự hủy đơn.                    |
| No-answer classification           | Order Core, Ops               | Có signal để xử lý đơn không xác nhận. | Notification tự động.              |
| Invalid phone classification       | Order Core, Admin             | Tách phone sai khỏi khách không nghe.  | Kết luận khách cố tình không nhận. |
| Technical exception classification | IVR Ops                       | Không làm sai attempt/no-answer.       | Lý do hủy đơn.                     |
| Admin monitoring                   | Ops/Admin                     | Điều hành queue/SIM/review an toàn.    | Quyền override order.              |

## 11. Use case catalog

| Use case ID  | Tên                               | Primary actor         | Trigger                                  | Final owner      |
| ------------ | --------------------------------- | --------------------- | ---------------------------------------- | ---------------- |
| IVR01-UC-001 | Xác nhận đơn bằng phím `1`        | Khách hàng            | Official Order cần IVR                   | Order Core       |
| IVR01-UC-002 | Hủy/không đặt bằng phím `0`       | Khách hàng            | Khách nghe call và bấm `0`               | Order Core       |
| IVR01-UC-003 | Không nghe trong Golden Hour      | Khách hàng không nghe | 2 attempt hợp lệ đều no-answer           | Order Core       |
| IVR01-UC-004 | Không nghe trong 24/7             | Khách hàng không nghe | 3 attempt hợp lệ đều no-answer           | Order Core       |
| IVR01-UC-005 | Trusted customer skip             | Order Core            | Trust policy cho phép skip               | Order Core       |
| IVR01-UC-006 | Invalid official phone            | Phone resolver        | Phone invalid/missing/not official       | Order Core/Admin |
| IVR01-UC-007 | Technical failure                 | IVR runtime/SIM       | SIM/server/DTMF/evidence lỗi             | IVR Ops/Admin    |
| IVR01-UC-008 | Operational blocker after confirm | Operational Core      | Sale Lock/Recall/payment issue xuất hiện | Order Core       |
| IVR01-UC-009 | Admin pause queue                 | Admin/Ops             | Capacity/security/incident               | IVR Ops          |
| IVR01-UC-010 | Manual technical retry            | Admin/Ops             | Technical exception retryable            | IVR Ops          |

## 12. Detailed scenario - customer confirms

Preconditions:

- Official Order đã tạo.
- Order state còn callable.
- Customer không được trusted skip.
- Official contact valid.
- Program policy resolved.
- Không có Sale Lock/Recall/Suppression/opt-out.

Main flow:

1. Order Core phát hành task.
2. IVR tạo CallJob và attempt schedule.
3. Scheduler dispatch attempt trong window.
4. SIM Adapter phát script đã duyệt.
5. Khách bấm `1`.
6. Result Normalizer tạo `IVR_CONFIRMED`.
7. IVR callback Order Core.
8. Order Core revalidate.
9. Nếu pass, Order Core transition đơn theo state machine.

Alternate flows:

| Điều kiện           | Kết quả                                     |
| ------------------- | ------------------------------------------- |
| Callback stale      | Order Core reject stale; order không đổi.   |
| Sale Lock xuất hiện | Order Core block/hold; order không confirm. |
| Evidence thiếu      | Route admin review/technical exception.     |
| Duplicate callback  | Idempotency trả kết quả cũ.                 |

## 13. Detailed scenario - customer cancels

Preconditions giống confirm.

Main flow:

1. Khách bấm `0`.
2. Result Normalizer tạo `IVR_CUSTOMER_CANCELLED`.
3. IVR callback Order Core.
4. Order Core revalidate.
5. Nếu chính sách cho phép, Order Core hủy đơn.
6. Notification owner gửi thông báo nếu có template/rule được duyệt.

Hard blocks:

- IVR/SIM không tự gửi thông báo hủy.
- Admin IVR không sửa result thành cancel.
- Technical exception không được chuyển thành cancel.

## 14. Detailed scenario - no-answer

Golden Hour:

- Attempt 1 tại `T0`.
- Attempt 2 tại `T0 + 5`.
- Final no-answer nếu attempt 2 hợp lệ không nghe hoặc hết window theo policy.

24/7:

- Attempt 1 tại `T0`.
- Attempt 2 tại `T0 + 5`.
- Attempt 3 tại `T0 + 10`.
- Final no-answer nếu attempt 3 hợp lệ không nghe hoặc hết window theo policy.

No-answer không được sinh khi:

- SIM lỗi.
- Server lỗi.
- DTMF capture lỗi.
- Evidence writer lỗi.
- Callback lỗi.
- Phone validation technical error.

## 15. Business rule matrix

| Rule                | Allowed                          | Blocked                       |
| ------------------- | -------------------------------- | ----------------------------- |
| Khách mới/untrusted | Có thể IVR nếu đủ điều kiện.     | Không skip nếu risk flag.     |
| Khách trusted       | Có thể skip theo trust decision. | Không hardcode trong IVR.     |
| Phone invalid       | Không gọi.                       | Không count no-answer.        |
| Key `1`             | Signal confirm.                  | Không force confirm order.    |
| Key `0`             | Signal cancel request.           | Không tự hủy order.           |
| No-answer max       | Signal final no-answer.          | Không tự notification.        |
| Technical failure   | Technical exception.             | Không count customer attempt. |

## 16. Business acceptance tests

| Test ID       | Given                    | When               | Then                                      |
| ------------- | ------------------------ | ------------------ | ----------------------------------------- |
| IVR01-BAT-001 | Official Order untrusted | Khách bấm `1`      | Callback confirm, Order Core revalidate.  |
| IVR01-BAT-002 | Official Order untrusted | Khách bấm `0`      | Callback cancel signal, Core quyết định.  |
| IVR01-BAT-003 | Golden Hour              | 2 no-answer hợp lệ | Final no-answer, không attempt 3.         |
| IVR01-BAT-004 | 24/7                     | 3 no-answer hợp lệ | Final no-answer, không attempt 4.         |
| IVR01-BAT-005 | Trusted skip             | Trust pass         | Không tạo call job.                       |
| IVR01-BAT-006 | Phone invalid            | Task created       | Không dispatch SIM.                       |
| IVR01-BAT-007 | SIM failure              | During attempt     | Technical exception, attempt not counted. |
| IVR01-BAT-008 | Sale Lock after key `1`  | Callback           | Core block/hold, không confirm.           |

---

<!-- SOURCE_FILE: IVR-02-ownership-boundary-connected-systems.md -->

# IVR-02 - Ownership Boundary / Connected Systems

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa hệ thống IVR kết nối với hệ thống nào, bằng gì và theo ranh giới ownership nào.

## 1. Mục tiêu

Tài liệu này mô tả kiến trúc kết nối cấp SRS cho IVR. Đây chưa phải OpenAPI/AsyncAPI/schema thật, nhưng khóa semantic interface để phase sau sinh contract.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                                |
| ------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------ |
| `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md`                                                   | Owner/resolver/consumer boundary.                      |
| `docs/documents/1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md`                                             | Cross-pack dependency.                                 |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Boundary Order Core / IVR / Operational Core.          |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`      | Commerce owner của Official Order và payment boundary. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Module boundary IVR.                                   |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`                                                 | Connected systems baseline.                            |

## 3. Hệ thống kết nối

| Hệ thống                                  | Kết nối bằng gì                    | Kết nối như thế nào                                                                                           | Owner                     | IVR được làm                       | IVR không được làm             |
| ----------------------------------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------- | ------------------------- | ---------------------------------- | ------------------------------ |
| Commerce Order Core / Order State Machine | Internal command/callback contract | Order Core tạo `IvrConfirmationTaskV1`; IVR callback `IvrConfirmationResultCallbackV1`; Order Core revalidate | Commerce/Order Owner      | Nhận task, gửi result signal       | Tự transition order            |
| Operational Core                          | Internal read/check contract       | IVR/Order Core kiểm Sale Lock, Recall, Suppression, availability, traceability blocker                        | Operational Owner         | Consume blocker status             | Bỏ qua hoặc override blocker   |
| Customer Trust / Customer Memory          | Internal resolver                  | Lấy trust decision, risk reason, trusted skip reason                                                          | Customer/CRM Owner        | Consume trusted/untrusted decision | Hardcode trusted customer      |
| Official Contact / Customer Profile       | Privacy-safe projection            | Nhận `official_contact_id`, `phone_ref`, `phone_masked`, token dial nếu được phép                             | Customer/Commerce Owner   | Gọi contact đã duyệt               | Đọc full profile/full address  |
| SIM Gateway Adapter                       | Internal hardware/API adapter      | Scheduler reserve SIM, adapter dial, capture call status/DTMF                                                 | IVR Infrastructure Owner  | Dial, capture, health check        | Gửi SMS hoặc update order      |
| Evidence Registry / Audit                 | Internal writer                    | Ghi evidence/audit refs cho task, attempt, result, admin action                                               | Foundation/Evidence Owner | Ghi evidence                       | Tự mark accepted/pass          |
| Admin Web/Ops Console                     | Internal admin API                 | Monitor queue, review exception, pause/resume, disable SIM theo RBAC                                          | Ops/Admin Owner           | Điều hành có audit                 | Override P0 blocker            |
| Notification Owner                        | Internal handoff sau Core decision | Order Core gửi yêu cầu notification sau khi hủy/expire/hold                                                   | Notification Owner        | Không gửi trực tiếp                | Tự gửi notice từ IVR/SIM       |
| AI Advisor/Facebook Gateway/Live/CRM      | No direct command                  | Chỉ consume trạng thái đã được Core công nhận nếu policy cho phép                                             | Channel/AI/CRM Owners     | Không trigger IVR                  | Tạo task/gọi IVR/public result |
| Payment/MISA/Finance                      | Không kết nối trực tiếp từ IVR     | Order Core/Finance xử lý riêng                                                                                | Finance/MISA Owner        | Không xử lý                        | Xác nhận payment/MISA/revenue  |

## 4. Luồng kết nối chuẩn

1. Commerce Order Core tạo Official Order đủ điều kiện.
2. Order Core quyết định có cần IVR theo policy và tạo `IvrConfirmationTaskV1`.
3. IVR intake task, kiểm idempotency và trace.
4. IVR re-check eligibility, phone, blocker, attempt policy.
5. IVR scheduler reserve SIM channel.
6. SIM Gateway Adapter gọi khách và capture DTMF.
7. IVR Result Normalizer chuẩn hóa result.
8. IVR ghi evidence/audit.
9. IVR callback về Order Core.
10. Order Core revalidate và quyết định transition.
11. Notification/Evidence/Release systems chỉ consume quyết định đã được owner công nhận.

## 5. Yêu cầu chức năng

| ID           | Yêu cầu                                                                                             |
| ------------ | --------------------------------------------------------------------------------------------------- |
| IVR02-FR-001 | IVR chỉ nhận task từ Order Core hoặc service được Order Core ủy quyền rõ ràng.                      |
| IVR02-FR-002 | Mọi kết nối phải có `correlation_id`, `idempotency_key` hoặc trace tương đương.                     |
| IVR02-FR-003 | IVR phải re-check blocker trước attempt và Order Core phải re-check blocker khi nhận callback.      |
| IVR02-FR-004 | SIM Gateway Adapter không được có credential/quyền ghi order.                                       |
| IVR02-FR-005 | Admin action phải đi qua RBAC server-side và audit.                                                 |
| IVR02-FR-006 | Không hệ thống downstream nào được dùng raw IVR result để tuyên bố order đã paid/revenue/delivered. |

## 6. Tình huống lỗi

| Failure                               | Hành vi bắt buộc                                                      |
| ------------------------------------- | --------------------------------------------------------------------- |
| Order Core unavailable                | Không tạo hoặc không tiếp tục task mới; route fail-safe/admin review. |
| Operational blocker check unavailable | Không dispatch attempt.                                               |
| Customer Trust unavailable            | Không gọi trusted/customer nếu không có policy fallback.              |
| Evidence Registry unavailable         | Không dùng result để transition.                                      |
| SIM Gateway unavailable               | Technical exception, không tính customer attempt.                     |
| Callback failed                       | Retry kỹ thuật bounded, không gửi duplicate transition.               |

## 7. Tiêu chí chấp nhận

- SRS chỉ rõ từng hệ thống kết nối bằng gì và quyền gì.
- Không có hệ thống nào ngoài Order Core được quyết định trạng thái đơn.
- Không có kết nối IVR -> Payment/MISA/Revenue.
- Có fail-safe khi source owner không khả dụng.

## 8. Detailed integration responsibility matrix

| Integration       | Producer              | Consumer                        | Transport triển khai khuyến nghị   | Sync/async                                | Data classification                     |
| ----------------- | --------------------- | ------------------------------- | ---------------------------------- | ----------------------------------------- | --------------------------------------- |
| Order task        | Order Core            | IVR Internal API                | Internal REST/command API          | Sync command                              | Internal order data + phone projection. |
| Eligibility check | IVR/Order Core        | Trust/contact/blocker resolvers | Internal API/read model            | Sync trước dispatch                       | Sensitive decision data.                |
| Attempt dispatch  | Scheduler             | SIM Adapter                     | Internal worker command            | Async worker                              | Sensitive phone token/call metadata.    |
| Result callback   | IVR Runtime           | Order Core                      | Internal callback API              | Sync with retry                           | Order signal.                           |
| Event publication | IVR/Business Platform | Consumers                       | AsyncAPI future approved toolchain | Async                                     | Signal only, not order final.           |
| Admin action      | Admin Web             | IVR API                         | Admin API                          | Sync command                              | Audit/security sensitive.               |
| Evidence write    | IVR services          | Evidence Registry               | Internal writer/API                | Sync before final callback where required | Audit/evidence data.                    |

## 9. Ownership by lifecycle stage

| Stage                  | Owner                         | IVR role          | Required handoff           |
| ---------------------- | ----------------------------- | ----------------- | -------------------------- |
| Official Order created | Order Core                    | None until task.  | Task only if IVR required. |
| Task emitted           | Order Core                    | Consumer.         | `IvrConfirmationTaskV1`.   |
| Eligibility resolved   | Order Core + source resolvers | Validate/consume. | Eligibility decision.      |
| Queue scheduled        | IVR Runtime                   | Owner.            | CallJob/Attempt persisted. |
| SIM call active        | IVR Infrastructure            | Owner.            | Raw call status/DTMF.      |
| Result normalized      | IVR Runtime                   | Owner.            | `IvrCallResult`.           |
| Callback received      | Order Core                    | Decision owner.   | Ack/stale/block/review.    |
| Notification           | Notification Owner            | No direct role.   | Only after Core decision.  |

## 10. Connected system failure contracts

| System unavailable     | Before attempt                                      | During attempt                                                                    | During callback                    |
| ---------------------- | --------------------------------------------------- | --------------------------------------------------------------------------------- | ---------------------------------- |
| Order Core             | Do not create new task.                             | Continue active call only if already dispatched safely; callback retries bounded. | Retry bounded/admin review.        |
| Operational Core       | Do not dispatch.                                    | If active call already returned result, Core revalidates before action.           | Core blocks if cannot revalidate.  |
| Trust/contact resolver | Hold task/review.                                   | Do not switch contact mid-call.                                                   | Core decides with current sources. |
| Evidence Registry      | Do not final-callback if required evidence missing. | Technical exception if evidence write fails.                                      | Hold/admin review.                 |
| SIM Gateway            | Technical exception.                                | Technical exception; not no-answer.                                               | Not applicable.                    |
| Admin Web              | No operational impact.                              | No operational impact.                                                            | No operational impact.             |

## 11. Data flow details

### 11.1 Order Core to IVR

Data allowed:

- `task_id`, `order_id`, `order_code_short`, `order_version`.
- `program_code`, `attempt_policy_code`, schedule.
- `customer_ref`, trust decision, risk flags.
- `official_contact_id`, `phone_ref`, `phone_masked`, optional dial token.
- Blocker snapshots/refs.
- Evidence/privacy policy versions.

Data prohibited:

- Full customer profile.
- Full address.
- Health or sensitive note.
- Payment credentials/details.
- MISA accounting payload.
- AI consultation content.

### 11.2 IVR to SIM Adapter

Data allowed:

- `attempt_id`, `sim_channel_id`, script version.
- Dial token or approved dial phone access.
- Minimal script variables.

Data prohibited:

- Order write credential.
- Full order item history beyond script-approved fields.
- Admin credential.

### 11.3 IVR to Order Core callback

Data required:

- `callback_id`, `task_id`, `order_id`.
- `order_version_seen_by_ivr`.
- `result_status`, `result_reason`, `is_counted_customer_attempt`.
- `evidence_ref`, `audit_ref`.
- `idempotency_key`, `correlation_id`.

## 12. Boundary P0 tests

| Test                                         | Expected                          |
| -------------------------------------------- | --------------------------------- |
| SIM Adapter tries to update order            | Forbidden/no credential.          |
| Facebook Gateway tries to create IVR task    | Forbidden.                        |
| Admin tries to force order cancel from IVR   | Forbidden.                        |
| IVR callback without evidence                | Hold/reject/review.               |
| Operational Core unavailable before dispatch | No SIM call.                      |
| Payment/MISA integration requested from IVR  | Not implemented/blocked by scope. |

## 13. Implementation acceptance checklist

- All service identities are allowlisted.
- All internal APIs require auth.
- SIM Adapter has no Order Core write permission.
- Admin actions are RBAC protected server-side.
- Evidence/audit integration is present.
- Downstream systems consume only Core-approved state where required.

---

<!-- SOURCE_FILE: IVR-03-eligibility-customer-trust-official-contact.md -->

# IVR-03 - Eligibility Rule / Customer Trust / Official Contact

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa điều kiện đơn/khách nào cần IVR, khách trusted được skip thế nào và contact nào được phép gọi.

## 1. Mục tiêu

Eligibility là chốt chặn để IVR không gọi đại trà, không gọi sai đối tượng và không gọi khi nguồn sự thật chưa sẵn sàng. IVR chỉ được gọi Official Order đủ điều kiện từ Commerce Order Core.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                                                    |
| ------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Rule chương trình, boundary và call script.                                |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Eligibility, trusted customer exclusion, phone validation.                 |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`      | Official Order boundary.                                                   |
| `docs/documents/4. phase/phase-4/09-README-PHASE-04-HANDOFF-INDEX.md`                                        | Customer Memory/Context Resolver là consumer, không tự tạo business truth. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`                                                 | Gap closure về eligibility, official contact và phone validation.          |

## 3. Input bắt buộc

| Field                     | Owner                     | Bắt buộc | Ghi chú                                             |
| ------------------------- | ------------------------- | -------- | --------------------------------------------------- |
| `order_id`                | Order Core                | Có       | Official Order only.                                |
| `order_code_short`        | Order Core                | Có       | Biến được phép đọc trong script.                    |
| `order_version`           | Order Core                | Có       | Guard chống stale callback/race.                    |
| `order_state`             | Order Core                | Có       | Phải là trạng thái được phép IVR.                   |
| `program_code`            | Commerce Runtime          | Có       | `GOLDEN_HOUR` hoặc `TWENTY_FOUR_SEVEN`.             |
| `customer_ref`            | Customer/Commerce         | Có       | Không truyền full profile.                          |
| `customer_trust_status`   | Customer Trust Resolver   | Có       | `NEW`, `UNTRUSTED`, `PARTIALLY_TRUSTED`, `TRUSTED`. |
| `risk_flags`              | Commerce/Risk             | Có       | Chỉ dùng flag có source-backed policy.              |
| `official_contact_id`     | Customer/Commerce         | Có       | Contact được phép gọi.                              |
| `phone_ref`               | Customer/Commerce         | Có       | Tham chiếu bảo mật hoặc token.                      |
| `phone_masked`            | Customer/Commerce         | Có       | Hiển thị admin.                                     |
| `phone_validation_status` | Phone Validation Resolver | Có       | Không production nếu unknown.                       |
| `sale_lock_status`        | Operational Core          | Có       | Blocker.                                            |
| `recall_status`           | Operational Core          | Có       | Blocker.                                            |
| `suppression_status`      | Operational/Gateway       | Có       | Blocker.                                            |
| `call_restriction_status` | Compliance/Customer       | Có       | Opt-out/legal/call restriction.                     |

## 4. Eligibility decisions

| Decision                           | Ý nghĩa                                         | Hành động                             |
| ---------------------------------- | ----------------------------------------------- | ------------------------------------- |
| `ELIGIBLE_FOR_IVR`                 | Official Order đủ điều kiện IVR                 | Tạo CallJob.                          |
| `NOT_ELIGIBLE_NOT_OFFICIAL_ORDER`  | Không phải Official Order                       | Reject task, ghi evidence.            |
| `NOT_ELIGIBLE_TRUSTED_CUSTOMER`    | Khách trusted, không có risk mới                | Skip IVR, trả skip reason cho Core.   |
| `NOT_ELIGIBLE_POLICY_BLOCKED`      | Policy/source chưa sẵn sàng                     | Fail-safe, admin review.              |
| `NOT_ELIGIBLE_OPERATIONAL_BLOCKED` | Sale Lock/Recall/Suppression/availability block | Stop IVR, handoff Core.               |
| `NOT_ELIGIBLE_CALL_RESTRICTED`     | Opt-out/legal/call restriction                  | Stop IVR, handoff Core.               |
| `NOT_ELIGIBLE_PHONE_INVALID`       | Phone invalid trước attempt                     | Không gọi, trả invalid phone outcome. |
| `ROUTE_HUMAN_REVIEW`               | Ambiguous/risk cần người xử lý                  | Admin/CSKH review queue.              |

## 5. Khách cần IVR

Một đơn được xét IVR khi có ít nhất một điều kiện:

- Khách mới.
- Khách chưa đủ trust.
- Customer profile thiếu dữ liệu xác thực.
- Official contact mới hoặc bất thường.
- Đơn có risk flag theo policy.
- Customer Memory chưa đủ dữ liệu để bỏ qua IVR.
- Đơn thuộc chương trình yêu cầu xác nhận theo policy.

## 6. Trusted customer skip

Khách trusted được skip IVR chỉ khi tất cả điều kiện đúng:

- Customer Trust Resolver trả `TRUSTED`.
- Commerce/Customer Memory cho phép trusted checkout.
- Official contact ổn định và được duyệt.
- Không có risk flag mới.
- Không có Sale Lock/Recall/Suppression/call restriction.
- Order state/program/window còn hợp lệ.

IVR không được hardcode trusted customer bằng:

- Tên khách.
- Số điện thoại thô.
- Lịch sử chat.
- Admin note không có policy/evidence.
- Cảm tính nhân viên.

`Owner Decision Required`: ngưỡng trust cụ thể và danh sách risk flag buộc trusted customer vẫn phải IVR.

## 7. Official contact policy

IVR chỉ được gọi `official_contact_id` do Commerce/Customer Profile cung cấp. Projection sang IVR chỉ gồm dữ liệu tối thiểu:

- `official_contact_id`.
- `phone_ref`.
- `phone_masked`.
- `phone_e164_or_adapter_token` nếu security/privacy policy cho phép.
- `contact_source`.
- `contact_verified_at`.
- `contact_policy_version`.

Không được truyền full profile, full address, payment detail, health note, CRM note hoặc nội dung tư vấn riêng tư.

## 8. Phone validation

| State                                 | Ý nghĩa                          | Hành động                                  |
| ------------------------------------- | -------------------------------- | ------------------------------------------ |
| `PHONE_NOT_EVALUATED`                 | Chưa validate                    | Validate trước khi tạo CallJob.            |
| `PHONE_VALID`                         | Có thể gọi theo policy           | Tiếp tục.                                  |
| `PHONE_INVALID_FORMAT`                | Sai định dạng                    | Không gọi, invalid outcome.                |
| `PHONE_INVALID_UNREACHABLE_CONFIRMED` | Invalid theo rule owner-approved | Không gọi hoặc final invalid theo policy.  |
| `PHONE_UNKNOWN_NEEDS_REVIEW`          | Không đủ căn cứ                  | Human review hoặc policy fallback.         |
| `PHONE_VALIDATION_TECHNICAL_ERROR`    | Lỗi resolver                     | Technical exception, không tính lỗi khách. |

Invalid phone không được ghi là no-answer. Technical validation error không được ghi là invalid phone nếu chưa có rule owner-approved.

## 9. Fail-safe

Không được tạo CallJob nếu thiếu:

- Order Core status.
- Customer Trust Resolver.
- Phone Validation Resolver.
- Sale Lock/Recall/Suppression check.
- Attempt Policy Resolver.
- Evidence Registry.
- Privacy/Call Restriction check.

Fail-safe outcome: `ELIGIBILITY_POLICY_UNAVAILABLE` và `ADMIN_REVIEW_REQUIRED`.

## 10. Acceptance criteria

- SRS trả lời rõ khách mới/untrusted cần IVR như thế nào.
- SRS trả lời rõ khách trusted skip theo rule nào.
- Official contact được chọn, validate, mask và audit rõ.
- Invalid phone, no-answer và technical validation error không bị trộn.

## 11. Eligibility decision tree

```text
START
  -> Is entity Official Order?
       NO  -> BLOCK_QUOTE_CART_DRAFT
       YES -> Is order state IVR-callable?
                NO  -> BLOCK_POLICY
                YES -> Is Sale Lock/Recall/Suppression/opt-out active?
                         YES -> BLOCK_OPERATIONAL or BLOCK_OPT_OUT
                         NO  -> Is trusted skip allowed and no risk flag?
                                  YES -> SKIP_TRUSTED_CUSTOMER
                                  NO  -> Is official contact available?
                                           NO  -> BLOCK_INVALID_PHONE or OWNER_REVIEW_REQUIRED
                                           YES -> Validate phone
                                                    valid -> ELIGIBLE_FOR_IVR
                                                    invalid -> BLOCK_INVALID_PHONE
                                                    technical error -> OWNER_REVIEW_REQUIRED
```

## 12. Required eligibility inputs

| Input                       | Owner                  | Required | Use                                 |
| --------------------------- | ---------------------- | -------- | ----------------------------------- |
| `entity_type`               | Order Core             | Có       | Chặn Quote/Cart/Draft.              |
| `official_order_id`         | Order Core             | Có       | Link order.                         |
| `order_state`               | Order Core             | Có       | Chỉ trạng thái callable.            |
| `order_version`             | Order Core             | Có       | Race guard.                         |
| `customer_ref`              | Customer/Commerce      | Có       | Trust lookup.                       |
| `customer_trust_status`     | Trust Resolver         | Có       | Trusted skip.                       |
| `risk_flags`                | Risk/Order Core        | Có       | Trusted vẫn phải IVR nếu có risk.   |
| `official_contact_id`       | Customer/Commerce      | Có       | Contact được phép gọi.              |
| `phone_ref`                 | Customer/Commerce      | Có       | Không dùng raw phone nếu không cần. |
| `phone_masked`              | Customer/Commerce      | Có       | Admin display.                      |
| `phone_validation_status`   | Phone Resolver         | Có       | Valid/invalid/inconclusive.         |
| `sale_lock_snapshot`        | Operational Core       | Có       | Blocker.                            |
| `recall_snapshot`           | Operational Core       | Có       | Blocker.                            |
| `suppression_snapshot`      | Operational/Compliance | Có       | Blocker.                            |
| `call_restriction_snapshot` | Compliance/Customer    | Có       | Opt-out/legal block.                |

## 13. Trusted customer skip rules

Trusted skip chỉ được áp dụng khi tất cả điều kiện sau đúng:

- Trust resolver trả decision rõ ràng.
- Không có risk flags bắt buộc IVR.
- Order state vẫn hợp lệ.
- Không có Sale Lock/Recall/Suppression/opt-out.
- Contact/payment/order snapshot không có thay đổi làm tăng rủi ro.
- Policy version được ghi lại.

Trusted skip không được áp dụng khi:

| Điều kiện                       | Hành vi                                                      |
| ------------------------------- | ------------------------------------------------------------ |
| New customer                    | Require IVR nếu đủ điều kiện.                                |
| Trust resolver unavailable      | `OWNER_REVIEW_REQUIRED` hoặc fallback owner-approved.        |
| Phone mới đổi gần đây           | Owner Decision Required; mặc định không skip nếu risk.       |
| High value order                | Owner Decision Required; mặc định require IVR nếu risk flag. |
| Prior cancellation/no-show risk | Require IVR nếu risk flag.                                   |

## 14. Official contact selection

Priority:

1. Contact được Order Core đánh dấu là official contact cho order.
2. Contact từ customer profile projection được owner duyệt.
3. Không tự chọn contact khác nếu official contact invalid.

Rules:

- Không gọi số từ comment/chat/free text.
- Không gọi số từ CRM note.
- Không gọi số từ lịch sử cũ nếu không được resolver trả về.
- Không gọi full address/contact list.
- Nếu có nhiều official contact, resolver phải trả priority rõ.

## 15. Phone validation taxonomy

| Status                             | Meaning                               | IVR action                             |
| ---------------------------------- | ------------------------------------- | -------------------------------------- |
| `PHONE_VALID`                      | Có thể gọi theo policy.               | Eligible tiếp.                         |
| `PHONE_INVALID_FORMAT`             | Format sai.                           | Block invalid phone.                   |
| `PHONE_MISSING`                    | Không có phone.                       | Block/review.                          |
| `PHONE_NOT_OFFICIAL_CONTACT`       | Phone không thuộc contact chính thức. | Block.                                 |
| `PHONE_OPTED_OUT`                  | Opt-out/suppressed.                   | Block opt-out.                         |
| `PHONE_VALIDATION_INCONCLUSIVE`    | Không đủ kết luận.                    | Review/fail-safe.                      |
| `PHONE_VALIDATION_TECHNICAL_ERROR` | Resolver lỗi kỹ thuật.                | Technical/review, không invalid final. |

## 16. Eligibility output contract

| Field                     | Required         | Note                |
| ------------------------- | ---------------- | ------------------- |
| `eligibility_decision_id` | Có               | Trace decision.     |
| `official_order_id`       | Có               | Link order.         |
| `decision`                | Có               | Enum decision.      |
| `trusted_customer_skip`   | Có               | True/false.         |
| `blocked_reasons`         | Có               | Empty nếu eligible. |
| `policy_version`          | Có               | Source policy.      |
| `evaluated_at`            | Có               | Timestamp.          |
| `source_refs`             | Có               | Sources used.       |
| `evidence_refs`           | Có nếu persisted | Evidence.           |
| `audit_refs`              | Có               | Audit.              |

## 17. Edge cases

| Edge case                                               | Required behavior                                             |
| ------------------------------------------------------- | ------------------------------------------------------------- |
| Order changed contact after task created                | Revalidate before dispatch; if mismatch, block/recreate task. |
| Phone valid at intake but opt-out before attempt        | Block attempt, no call.                                       |
| Trusted at intake but risk flag appears before dispatch | Require/block/review per policy, no blind skip.               |
| Recall appears after key `1`                            | Core blocks callback.                                         |
| Phone resolver timeout                                  | No call unless approved fallback.                             |
| Customer has multiple phones                            | Use official selected contact only.                           |

## 18. Eligibility P0 tests

| Test ID      | Scenario                         | Expected                             |
| ------------ | -------------------------------- | ------------------------------------ |
| IVR03-P0-001 | Quote task                       | `BLOCK_QUOTE_CART_DRAFT`.            |
| IVR03-P0-002 | Trusted no risk                  | `SKIP_TRUSTED_CUSTOMER`.             |
| IVR03-P0-003 | Trusted with risk                | Not skipped; require IVR/review.     |
| IVR03-P0-004 | Invalid phone                    | No SIM dispatch.                     |
| IVR03-P0-005 | Phone validation technical error | Not invalid final; review/technical. |
| IVR03-P0-006 | Sale Lock active                 | Block operational.                   |
| IVR03-P0-007 | Opt-out active                   | Block opt-out.                       |

---

<!-- SOURCE_FILE: IVR-04-order-core-to-ivr-task-contract.md -->

# IVR-04 - Order Core -> IVR Task Contract

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa semantic contract cho task mà Order Core gửi sang IVR.

## 1. Mục tiêu

`IvrConfirmationTaskV1` là contract nội bộ để Order Core yêu cầu IVR xét/gọi xác nhận đơn. Tài liệu này chưa tạo JSON Schema/OpenAPI thật, nhưng khóa field, owner, validation và reject rule để phase sau sinh contract.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                        |
| ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------- |
| `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md`                                                   | Correlation, idempotency, trace.               |
| `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md`                   | Foundation audit/evidence/idempotency.         |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Pack boundary và program-based attempt policy. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Module contract và lifecycle.                  |

## 3. Tổng quan contract

| Thuộc tính     | Giá trị                                                        |
| -------------- | -------------------------------------------------------------- |
| Contract name  | `IvrConfirmationTaskV1`                                        |
| Direction      | Order Core -> IVR Runtime                                      |
| Producer       | Commerce Order Core / Order State Machine                      |
| Consumer       | IVR Runtime Orchestrator                                       |
| Transport      | Internal service contract, implementation decision ở phase sau |
| Idempotency    | Bắt buộc                                                       |
| Correlation    | Bắt buộc                                                       |
| Production use | Chỉ sau IVR-09 release gate                                    |

## 4. Trường dữ liệu bắt buộc

| Field                         | Type semantic  | Required     | Owner               | Ghi chú                                   |
| ----------------------------- | -------------- | ------------ | ------------------- | ----------------------------------------- |
| `task_id`                     | string         | Có           | Order Core          | ID duy nhất của task.                     |
| `task_version`                | string         | Có           | Order Core          | `v1`.                                     |
| `idempotency_key`             | string         | Có           | Order Core          | Chống duplicate task.                     |
| `correlation_id`              | string         | Có           | Order Core          | Trace xuyên hệ thống.                     |
| `created_at`                  | datetime       | Có           | Order Core          | Thời điểm tạo task.                       |
| `expires_at`                  | datetime       | Có           | Order Core          | Không vượt program window.                |
| `order_id`                    | string         | Có           | Order Core          | Official Order id.                        |
| `order_code_short`            | string         | Có           | Order Core          | Biến script được phép đọc.                |
| `order_version`               | string/integer | Có           | Order Core          | Race-condition guard.                     |
| `order_state`                 | enum           | Có           | Order Core          | Phải là IVR-callable state.               |
| `program_code`                | enum           | Có           | Commerce Runtime    | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN`.       |
| `attempt_policy_code`         | enum           | Có           | Policy              | Program-based policy.                     |
| `max_attempts`                | integer        | Có           | Policy              | 2 cho Golden Hour, 3 cho 24/7.            |
| `attempt_schedule`            | array          | Có           | Policy              | T0/T0+5/T0+10 tùy chương trình.           |
| `customer_ref`                | string         | Có           | Customer/Commerce   | Không phải full profile.                  |
| `customer_trust_status`       | enum           | Có           | Trust Resolver      | Snapshot trust.                           |
| `trusted_skip_allowed`        | boolean        | Có           | Trust Resolver      | Nếu true và không có risk, IVR không gọi. |
| `risk_flags`                  | array          | Có           | Commerce/Risk       | Chỉ source-backed.                        |
| `official_contact_id`         | string         | Có           | Customer/Commerce   | Contact được gọi.                         |
| `phone_ref`                   | string         | Có           | Customer/Commerce   | Secure reference.                         |
| `phone_masked`                | string         | Có           | Customer/Commerce   | Admin display.                            |
| `phone_e164_or_adapter_token` | string         | Có điều kiện | Customer/Commerce   | Chỉ nếu privacy/security cho phép.        |
| `phone_validation_status`     | enum           | Có           | Phone Resolver      | Không unknown ở production.               |
| `call_script_template_id`     | string         | Có           | IVR Owner           | Script đã duyệt.                          |
| `call_script_version`         | string         | Có           | IVR Owner           | Version đã duyệt.                         |
| `allowed_script_variables`    | object         | Có           | Order Core          | Chỉ biến được duyệt.                      |
| `sale_lock_snapshot`          | object/ref     | Có           | Operational Core    | Blocking snapshot/ref.                    |
| `recall_snapshot`             | object/ref     | Có           | Operational Core    | Blocking snapshot/ref.                    |
| `suppression_snapshot`        | object/ref     | Có           | Operational/Gateway | Blocking snapshot/ref.                    |
| `call_restriction_snapshot`   | object/ref     | Có           | Compliance/Customer | Opt-out/legal restriction.                |
| `evidence_policy_version`     | string         | Có           | Evidence Owner      | Bắt buộc.                                 |
| `privacy_policy_version`      | string         | Có           | Privacy Owner       | Bắt buộc.                                 |

## 5. Validation tại task intake

IVR phải reject hoặc hold task nếu:

- Task thiếu `idempotency_key` hoặc `correlation_id`.
- `order_id` không phải Official Order.
- `order_state` không phải IVR-callable.
- `expires_at` đã qua.
- `program_code` không được hỗ trợ.
- `max_attempts` không khớp chương trình.
- Thiếu official contact hoặc phone projection.
- Phone invalid/unknown mà không có fallback policy.
- Sale Lock/Recall/Suppression/call restriction active.
- Evidence/privacy policy version thiếu.
- Script template/version chưa approved.

## 6. Output sau intake

| Intake result                      | Ý nghĩa                                        |
| ---------------------------------- | ---------------------------------------------- |
| `TASK_ACCEPTED_CALL_JOB_CREATED`   | Task hợp lệ, tạo CallJob.                      |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | Không phải Official Order.                     |
| `TASK_REJECTED_POLICY_MISMATCH`    | Attempt policy hoặc program mismatch.          |
| `TASK_REJECTED_CONTACT_INVALID`    | Contact/phone không hợp lệ.                    |
| `TASK_BLOCKED_OPERATIONAL`         | Sale Lock/Recall/Suppression/call restriction. |
| `TASK_HELD_ADMIN_REVIEW`           | Cần owner/admin review.                        |

## 7. Audit/evidence

Mỗi task intake phải ghi:

- `task_id`.
- `order_id`.
- `order_version`.
- `program_code`.
- `decision`.
- `reject_or_block_reason`.
- `correlation_id`.
- `idempotency_key`.
- `policy_version`.
- `evidence_ref`.
- `audit_ref`.

## 8. Tiêu chí chấp nhận

- Contract mô tả đủ dữ liệu Order Core phải gửi.
- Contract không chứa dữ liệu riêng tư vượt policy.
- Reject rule rõ, không để implementer tự suy luận.
- Sẵn sàng để phase sau sinh schema `IvrConfirmationTaskV1`.

## 9. Field-level contract expansion

| Field group    | Fields                                                                                  | Validation                                       | Reject/Hold reason                                                      |
| -------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------- |
| Identity       | `task_id`, `task_version`, `idempotency_key`, `correlation_id`                          | Non-empty, unique/idempotent, version `v1`.      | `TASK_REJECTED_INVALID_TRACE`.                                          |
| Order          | `order_id`, `order_code_short`, `order_version`, `order_state`                          | Official Order, callable state, current version. | `TASK_REJECTED_NOT_OFFICIAL_ORDER`, `TASK_REJECTED_STATE_NOT_CALLABLE`. |
| Program policy | `program_code`, `attempt_policy_code`, `max_attempts`, `attempt_schedule`, `expires_at` | Golden Hour 2/10; 24/7 3/15.                     | `TASK_REJECTED_POLICY_MISMATCH`.                                        |
| Customer trust | `customer_ref`, `customer_trust_status`, `trusted_skip_allowed`, `risk_flags`           | Resolver-backed, no hardcode.                    | `TASK_HELD_TRUST_POLICY_UNAVAILABLE`.                                   |
| Contact        | `official_contact_id`, `phone_ref`, `phone_masked`, optional dial token                 | Official contact only, phone valid.              | `TASK_REJECTED_CONTACT_INVALID`.                                        |
| Script         | `call_script_template_id`, `call_script_version`, `allowed_script_variables`            | Approved script and approved variables only.     | `TASK_REJECTED_SCRIPT_NOT_APPROVED`.                                    |
| Blockers       | Sale Lock, Recall, Suppression, call restriction snapshots                              | No active blocker.                               | `TASK_BLOCKED_OPERATIONAL`.                                             |
| Governance     | `evidence_policy_version`, `privacy_policy_version`                                     | Present and approved for target environment.     | `TASK_HELD_POLICY_MISSING`.                                             |

## 10. Required request invariants

Task payload must assert or imply:

- `not_for_quote_cart_draft = true`.
- `no_direct_order_update = true`.
- `call_purpose = ORDER_CONFIRMATION_ONLY`.
- `input_signal_only = true`.
- `program_code` and `max_attempts` match.
- `expires_at` does not exceed program window.
- `order_version` is included for callback race guard.

If implementation does not include these exact boolean fields, equivalent server-side validation and audit evidence are required.

## 11. Intake processing sequence

```text
receive task
  -> authenticate caller
  -> validate schema/required fields
  -> open idempotency scope
  -> reject duplicate conflict or return existing decision
  -> validate Official Order and order state
  -> resolve attempt policy
  -> validate official contact/phone
  -> check trusted skip/risk flags
  -> check operational blockers
  -> validate script/privacy/evidence policy
  -> create task record
  -> create CallJob if eligible
  -> write audit/evidence
  -> return intake decision
```

## 12. Intake result taxonomy

| Result                             | Creates CallJob? | Dispatch allowed?                      | Notes                             |
| ---------------------------------- | ---------------- | -------------------------------------- | --------------------------------- |
| `TASK_ACCEPTED_CALL_JOB_CREATED`   | Yes              | Only if release/scheduler gate allows. | Normal path.                      |
| `TASK_ACCEPTED_DRY_RUN_ONLY`       | Yes              | No real SIM.                           | For test/staging.                 |
| `TASK_SKIPPED_TRUSTED_CUSTOMER`    | No               | No                                     | Order Core owns continuation.     |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | No               | No                                     | Quote/Cart/Draft.                 |
| `TASK_REJECTED_POLICY_MISMATCH`    | No               | No                                     | Program/max/window mismatch.      |
| `TASK_REJECTED_CONTACT_INVALID`    | No               | No                                     | Invalid phone/contact.            |
| `TASK_BLOCKED_OPERATIONAL`         | No               | No                                     | Sale Lock/Recall/Suppression/etc. |
| `TASK_HELD_ADMIN_REVIEW`           | No or held       | No                                     | Missing source/ambiguous policy.  |

## 13. Idempotency rules

| Scenario                        | Required behavior                                                       |
| ------------------------------- | ----------------------------------------------------------------------- |
| Same key, same payload          | Return existing intake result.                                          |
| Same key, different payload     | Reject conflict.                                                        |
| Same `task_id`, different key   | Reject conflict unless exact duplicate mapped.                          |
| Retry after transient IVR error | Safe retry using same key.                                              |
| Retry after task rejected       | Return same rejection, do not create job later unless new task/version. |

## 14. API/database mapping

| Contract field         | API payload                      | DB column/entity                 |
| ---------------------- | -------------------------------- | -------------------------------- |
| `task_id`              | `task_id`                        | `ivr_confirmation_tasks.task_id` |
| `official_order_id`    | `official_order_id` / `order_id` | `official_order_id`              |
| `order_version`        | `order_version`                  | `order_version`                  |
| `program_code`         | `program_type`                   | `program_type`                   |
| `max_attempts`         | `max_attempts`                   | `max_attempts`                   |
| `attempt_schedule`     | `attempt_schedule`               | `attempt_schedule_json`          |
| `phone_ref`            | `phone_ref`                      | `phone_ref`                      |
| `phone_masked`         | `phone_masked`                   | `phone_masked`                   |
| `eligibility_decision` | `eligibility_decision`           | `eligibility_decision`           |
| `evidence_ref`         | `evidence_refs`                  | `evidence_refs_json`             |
| `audit_ref`            | `audit_refs`                     | `audit_refs_json`                |

## 15. Negative task scenarios

| Scenario                                         | Expected                                   |
| ------------------------------------------------ | ------------------------------------------ |
| Task missing correlation                         | Reject.                                    |
| Task missing idempotency                         | Reject.                                    |
| Program `GOLDEN_HOUR` with `max_attempts = 3`    | Reject policy mismatch.                    |
| Program `TWENTY_FOUR_SEVEN` with only 2 attempts | Reject policy mismatch.                    |
| Phone invalid                                    | Reject or hold, no CallJob dispatch.       |
| Release flag disabled                            | Accept dry-run/hold, no real SIM dispatch. |
| Script version not approved                      | Reject/hold.                               |

## 16. Task acceptance tests

| Test ID        | Given                                   | Then                     |
| -------------- | --------------------------------------- | ------------------------ |
| IVR04-TASK-001 | Valid Golden Hour task                  | CallJob with 2 attempts. |
| IVR04-TASK-002 | Valid 24/7 task                         | CallJob with 3 attempts. |
| IVR04-TASK-003 | Duplicate idempotency same payload      | Existing result.         |
| IVR04-TASK-004 | Duplicate idempotency different payload | Conflict.                |
| IVR04-TASK-005 | Quote task                              | Reject.                  |
| IVR04-TASK-006 | Active Sale Lock                        | Block.                   |
| IVR04-TASK-007 | Missing evidence policy                 | Hold/review.             |

---

<!-- SOURCE_FILE: IVR-05-attempt-policy-scheduler-queue.md -->

# IVR-05 - Attempt Policy / Scheduler / Queue

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa attempt policy, scheduler, queue và state machine cho CallJob/Attempt.

## 1. Mục tiêu

Scheduler phải gọi đúng số lần, đúng window, đúng ưu tiên deadline và không kéo dài hiệu lực thương mại. Technical retry phải tách khỏi customer attempt.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                          |
| ------------------------------------------------------------------------------------------------------------ | ------------------------------------------------ |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Program-based call rule, capacity, scheduler.    |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Attempt lifecycle, no-answer, technical failure. |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md`                                                      | SIM capacity, attempt timing, queue baseline.    |

## 3. Chính sách attempt theo chương trình

| Program             | Window  | Max customer attempt | Schedule                      | Final no-answer                 |
| ------------------- | ------- | -------------------- | ----------------------------- | ------------------------------- |
| `GOLDEN_HOUR`       | 10 phút | 2                    | T0, T0 + 5 phút               | Sau attempt 2 hợp lệ không nghe |
| `TWENTY_FOUR_SEVEN` | 15 phút | 3                    | T0, T0 + 5 phút, T0 + 10 phút | Sau attempt 3 hợp lệ không nghe |

Không được áp một max attempt chung cho mọi chương trình. Không được gọi attempt ngoài window.

## 4. Attempt được tính và không được tính

Counted customer attempt khi:

- Phone valid.
- Attempt dispatch trong program window.
- Call đạt trạng thái customer-reachable theo adapter/policy.
- Khách nghe, từ chối, không nghe sau valid ring, hoặc không nhập DTMF theo policy.

Không tính là customer attempt khi:

- SIM Gateway failure.
- Server error.
- DTMF capture error.
- Audio playback error.
- Scheduler error trước dispatch.
- Callback error.
- Policy source unavailable.
- Evidence write failure.
- Phone validation technical error.

## 5. Scheduler rules

| Rule                                       | Yêu cầu                                                         |
| ------------------------------------------ | --------------------------------------------------------------- |
| `DEADLINE_AWARE_ROLLING_QUEUE`             | Queue phải ưu tiên job gần hết window.                          |
| `BATCH_AFTER_SESSION_CALLING = PROHIBITED` | Không dồn toàn bộ cuộc gọi cuối phiên.                          |
| `ONE_SIM_ONE_ACTIVE_CALL`                  | Một SIM chỉ có một outbound call active.                        |
| `NO_WINDOW_EXTENSION`                      | Capacity incident không được kéo dài Golden Hour/24/7 validity. |
| `NO_INFINITE_RETRY`                        | Không có retry vô hạn.                                          |

## 6. CallJob state machine

| State                         | Ý nghĩa                          | Next states                                                           |
| ----------------------------- | -------------------------------- | --------------------------------------------------------------------- |
| `CALL_JOB_NOT_CREATED`        | Chưa có job                      | `CALL_JOB_CREATED`, `CALL_JOB_REJECTED`                               |
| `CALL_JOB_CREATED`            | Job accepted                     | `ELIGIBILITY_RECHECK_PENDING`, `CALL_JOB_CANCELLED`                   |
| `ELIGIBILITY_RECHECK_PENDING` | Kiểm lại source trước queue      | `QUEUED`, `CALL_JOB_BLOCKED`, `ADMIN_REVIEW_REQUIRED`                 |
| `QUEUED`                      | Chờ scheduler                    | `SIM_CHANNEL_RESERVED`, `CALL_JOB_EXPIRED`, `QUEUE_PAUSED`            |
| `QUEUE_PAUSED`                | Queue bị pause                   | `QUEUED`, `CALL_JOB_EXPIRED`, `ADMIN_REVIEW_REQUIRED`                 |
| `SIM_CHANNEL_RESERVED`        | Đã giữ SIM                       | `DIALING`, `SIM_CHANNEL_FAILED`, `CALL_JOB_CANCELLED`                 |
| `DIALING`                     | Đang gọi                         | `ATTEMPT_RESULT_RECEIVED`, `SIM_CHANNEL_FAILED`, `CALL_JOB_CANCELLED` |
| `ATTEMPT_RESULT_RECEIVED`     | Có result attempt                | `WAITING_NEXT_ATTEMPT`, `RESULT_CALLBACK_PENDING`                     |
| `WAITING_NEXT_ATTEMPT`        | Chờ attempt sau                  | `QUEUED`, `CALL_JOB_EXPIRED`, `CALL_JOB_BLOCKED`                      |
| `RESULT_CALLBACK_PENDING`     | Chờ callback Core                | `RESULT_CALLBACK_SENT`, `CALLBACK_FAILED`                             |
| `RESULT_CALLBACK_SENT`        | Đã gửi callback                  | `CORE_ACK_RECEIVED`, `CALLBACK_FAILED`                                |
| `CORE_ACK_RECEIVED`           | Core nhận callback               | `CALL_JOB_CLOSED`                                                     |
| `CALLBACK_FAILED`             | Lỗi callback                     | `RESULT_CALLBACK_PENDING`, `ADMIN_REVIEW_REQUIRED`                    |
| `CALL_JOB_BLOCKED`            | Bị blocker/policy chặn           | `RESULT_CALLBACK_PENDING`, `ADMIN_REVIEW_REQUIRED`                    |
| `CALL_JOB_EXPIRED`            | Hết window                       | `RESULT_CALLBACK_PENDING`                                             |
| `CALL_JOB_REJECTED`           | Reject trước job                 | Terminal                                                              |
| `CALL_JOB_CANCELLED`          | Core/admin cancel trước khi xong | Terminal                                                              |
| `CALL_JOB_CLOSED`             | IVR side hoàn tất                | Terminal                                                              |

## 7. Attempt state machine

| State                      | Ý nghĩa                    | Counted?                            |
| -------------------------- | -------------------------- | ----------------------------------- |
| `ATTEMPT_PLANNED`          | Attempt được lên lịch      | Không                               |
| `ATTEMPT_PRECHECK_PENDING` | Kiểm lại blocker/source    | Không                               |
| `ATTEMPT_READY`            | Sẵn sàng reserve SIM       | Không                               |
| `SIM_RESERVED`             | Đã giữ SIM                 | Không                               |
| `DIALING`                  | Bắt đầu dial               | Tùy result                          |
| `RINGING`                  | Valid ring/connect         | Có nếu no-answer/reject theo policy |
| `ANSWERED`                 | Thiết bị/khách nghe        | Có                                  |
| `DTMF_WAITING`             | Chờ phím                   | Có                                  |
| `DTMF_1_CONFIRMED`         | Bấm `1`                    | Có, final                           |
| `DTMF_0_CANCELLED`         | Bấm `0`                    | Có, final                           |
| `DTMF_INVALID`             | Sai phím                   | Có nếu policy tính                  |
| `DTMF_TIMEOUT`             | Không nhập                 | Có                                  |
| `NO_ANSWER`                | Không nghe                 | Có                                  |
| `CUSTOMER_REJECTED`        | Từ chối cuộc gọi           | `Owner Decision Required`           |
| `UNREACHABLE`              | Không liên lạc được        | `Owner Decision Required`           |
| `CALL_DROPPED`             | Rớt cuộc gọi               | `Owner Decision Required`           |
| `SIM_FAILURE`              | Lỗi SIM/channel            | Không                               |
| `CALL_TECHNICAL_FAILURE`   | Lỗi kỹ thuật               | Không                               |
| `ATTEMPT_BLOCKED`          | Block trước/trong dispatch | Không                               |
| `ATTEMPT_EXPIRED`          | Hết window                 | Không                               |

## 8. Capacity incident

| State                      | Ý nghĩa                              |
| -------------------------- | ------------------------------------ |
| `CAPACITY_NORMAL`          | Queue trong SLA.                     |
| `CAPACITY_DEGRADED`        | Có nguy cơ trễ.                      |
| `CAPACITY_AT_RISK`         | Một số attempt có thể miss deadline. |
| `CAPACITY_INCIDENT_OPEN`   | Incident xác nhận.                   |
| `CAPACITY_ADMIN_PAUSED`    | Admin pause queue.                   |
| `CAPACITY_RECOVERING`      | Đang phục hồi backlog.               |
| `CAPACITY_INCIDENT_CLOSED` | Đã đóng incident với evidence.       |

Capacity incident không được kéo dài window. Nếu miss window, IVR trả expiry/block signal và Order Core quyết định.

## 9. Tiêu chí chấp nhận

- Golden Hour không có attempt thứ 3.
- 24/7 không có attempt thứ 4.
- Technical retry không tăng customer attempt count.
- Queue pause/resume có permission, reason, audit.
- Capacity miss không làm kéo dài giá/chương trình.

## 10. Attempt schedule generation

| Program             | `T0` source                                   | Generated customer attempts                                                       | Expiry             |
| ------------------- | --------------------------------------------- | --------------------------------------------------------------------------------- | ------------------ |
| `GOLDEN_HOUR`       | Time Order Core says IVR confirmation starts. | Attempt 1 at `T0`; attempt 2 at `T0 + 5 minutes`.                                 | `T0 + 10 minutes`. |
| `TWENTY_FOUR_SEVEN` | Time Order Core says IVR confirmation starts. | Attempt 1 at `T0`; attempt 2 at `T0 + 5 minutes`; attempt 3 at `T0 + 10 minutes`. | `T0 + 15 minutes`. |

Schedule must be deterministic from:

- `program_type`.
- `T0`.
- `policy_version`.
- `order_version`.

Schedule must not be regenerated with different offsets for the same task unless Order Core emits a new task/version.

## 11. Scheduler query model

Scheduler should query attempts by:

| Filter                                                                 | Reason                 |
| ---------------------------------------------------------------------- | ---------------------- |
| `status in (ATTEMPT_PLANNED, ATTEMPT_PRECHECK_PENDING, ATTEMPT_READY)` | Only due work.         |
| `scheduled_at <= now`                                                  | Attempt due.           |
| `scheduled_window_expires_at > now`                                    | Not expired.           |
| `queue_status = ACTIVE`                                                | Queue not paused/held. |
| `capacity_status != CAPACITY_HELD`                                     | Capacity safe.         |
| `real_call_allowed` or dry-run mode                                    | Release gate.          |

Ordering:

1. Earliest `scheduled_window_expires_at`.
2. Earliest `scheduled_at`.
3. Higher business priority only if source-backed.
4. Stable tie-breaker by `task_id`/`attempt_id`.

## 12. Pre-dispatch checks

Right before SIM reserve, scheduler must re-check:

- Queue not paused.
- Job not closed/cancelled.
- Attempt not already dispatched.
- Window not expired.
- Official contact still callable.
- Sale Lock/Recall/Suppression/opt-out not active.
- SIM capacity available.
- Evidence writer available if required.
- Release gate allows selected SIM mode.

If any check fails, mark attempt as `ATTEMPT_BLOCKED`, `ATTEMPT_EXPIRED`, `CAPACITY_HELD`, or `ADMIN_REVIEW_REQUIRED` as appropriate.

## 13. Technical retry model

Technical retry is allowed only when:

- Failure is classified as technical.
- No customer outcome was reached.
- Retry does not violate privacy/call restriction.
- Retry does not bypass operational blocker.
- Retry is within owner-approved retry count/backoff.

Technical retry must record:

| Field                            | Required    |
| -------------------------------- | ----------- |
| `technical_exception_id`         | Yes         |
| `original_attempt_id`            | Yes         |
| `customer_attempt_counted=false` | Yes         |
| `retry_reason`                   | Yes         |
| `admin_action_id` if manual      | Conditional |
| `evidence_ref`                   | Yes         |
| `audit_ref`                      | Yes         |

## 14. Capacity handling

Capacity incident should open when:

- Due attempts cannot be dispatched before expiry.
- SIM healthy count drops below operational threshold.
- Callback/evidence outage makes final processing unsafe.
- Admin pauses queue for safety.

Capacity incident must not:

- Count as no-answer.
- Extend commercial window.
- Create additional customer attempts.
- Allow batch-after-session calling.

## 15. Concurrency controls

| Risk                                                     | Guard                                       |
| -------------------------------------------------------- | ------------------------------------------- |
| Two workers dispatch same attempt                        | Attempt lock/compare-and-swap state update. |
| Two attempts reserve same SIM                            | Unique active SIM reservation.              |
| Queue paused while worker is dispatching                 | Pre-dispatch check after lock.              |
| Callback closes job while scheduler selects next attempt | Job state re-read after lock.               |
| Window expires while SIM reserve waits                   | Expiry check after reserve attempt.         |

## 16. Scheduler P0 tests

| Test ID      | Scenario                           | Expected                                      |
| ------------ | ---------------------------------- | --------------------------------------------- |
| IVR05-P0-001 | Golden Hour schedule               | Exactly 2 attempts.                           |
| IVR05-P0-002 | 24/7 schedule                      | Exactly 3 attempts.                           |
| IVR05-P0-003 | Golden Hour worker tries attempt 3 | Blocked/test fail.                            |
| IVR05-P0-004 | 24/7 worker tries attempt 4        | Blocked/test fail.                            |
| IVR05-P0-005 | SIM failure                        | Technical exception, no customer count.       |
| IVR05-P0-006 | Queue paused                       | No dispatch.                                  |
| IVR05-P0-007 | Capacity miss                      | Incident/expired signal, no window extension. |

## 17. Queue operational metrics

| Metric                           | Use                        |
| -------------------------------- | -------------------------- |
| `due_attempts_total`             | Backlog by deadline.       |
| `missed_window_total`            | Capacity/rule health.      |
| `dispatch_latency_ms`            | Scheduler performance.     |
| `sim_reservation_conflict_total` | Concurrency defects.       |
| `technical_retry_total`          | Adapter/runtime stability. |
| `customer_attempt_count_total`   | Policy validation.         |

---

<!-- SOURCE_FILE: IVR-06-internal-sim-gateway-adapter.md -->

# IVR-06 - Internal SIM Gateway Adapter

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa ranh giới kỹ thuật của Internal SIM Gateway Adapter.

## 1. Mục tiêu

Internal SIM Gateway Adapter là lớp duy nhất được phép giao tiếp với SIM Gateway phần cứng/API. Adapter chỉ dial, phát script, capture call status/DTMF và báo lỗi kỹ thuật. Adapter không biết business state và không được cập nhật order.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                      |
| ------------------------------------------------------------------------------------------------------------ | -------------------------------------------- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | Deployment model và SIM capacity.            |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md`                                                      | SIM Gateway, call execution, DTMF capture.   |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Technical failure không phải customer fault. |

## 3. Trách nhiệm của adapter

Adapter được phép:

- Quản lý SIM channel pool.
- Reserve/release SIM channel.
- Dial official contact token/phone theo task hợp lệ.
- Phát approved call script.
- Capture call status.
- Capture DTMF.
- Báo technical error code.
- Ghi SIM health metrics.
- Trả raw adapter result về IVR Runtime.

Adapter không được:

- Tạo/cập nhật/hủy order.
- Gửi SMS hoặc notification.
- Tự retry ngoài scheduler policy.
- Tự xác định khách trusted/untrusted.
- Tự bỏ qua blocker.
- Ghi evidence accepted/pass.

## 4. SIM channel model

| Rule                       | Yêu cầu                                     |
| -------------------------- | ------------------------------------------- |
| `ONE_SIM_ONE_ACTIVE_CALL`  | Một SIM chỉ xử lý một outbound call active. |
| `CHANNEL_HEALTH_REQUIRED`  | Chỉ SIM healthy mới được reserve.           |
| `CHANNEL_RELEASE_REQUIRED` | Mọi call kết thúc/lỗi phải release channel. |
| `IN_FLIGHT_AUDIT_REQUIRED` | Mọi in-flight call có audit/correlation.    |

## 5. Dữ liệu vào của adapter

Adapter chỉ nhận từ IVR Runtime:

- `call_job_id`.
- `attempt_id`.
- `correlation_id`.
- `sim_channel_id` nếu đã reserve.
- `phone_adapter_token` hoặc dialing material được privacy/security duyệt.
- `call_script_template_id`.
- `call_script_version`.
- `allowed_script_variables`.
- `attempt_deadline`.

## 6. Dữ liệu ra của adapter

Adapter trả:

- `adapter_result_id`.
- `attempt_id`.
- `sim_channel_id`.
- `call_started_at`.
- `call_ended_at`.
- `call_duration_seconds`.
- `raw_call_status`.
- `raw_dtmf_key`.
- `technical_error_code` nếu có.
- `adapter_trace_ref`.

Raw result chưa được dùng trực tiếp cho Order Core. Phải qua Result Normalizer ở `IVR-07`.

## 7. Phân loại lỗi kỹ thuật

| Error                     | Ý nghĩa                  | Counted customer attempt? |
| ------------------------- | ------------------------ | ------------------------- |
| `SIM_GATEWAY_ERROR`       | Gateway/hardware lỗi     | Không                     |
| `SIM_CHANNEL_FAILURE`     | SIM lỗi hoặc mất channel | Không                     |
| `SERVER_ERROR`            | Server/adaptor lỗi       | Không                     |
| `AUDIO_PLAYBACK_ERROR`    | Không phát được script   | Không                     |
| `DTMF_CAPTURE_ERROR`      | Không capture được phím  | Không                     |
| `SCHEDULER_ERROR`         | Lỗi trước dispatch       | Không                     |
| `INTERNAL_CALLBACK_ERROR` | Lỗi callback Core        | Không                     |
| `EVIDENCE_WRITE_ERROR`    | Không ghi được evidence  | Không                     |

## 8. Retry kỹ thuật

Technical retry phải:

- Có giới hạn.
- Không kéo dài program window.
- Không tăng customer attempt count.
- Không gọi duplicate callback.
- Có evidence/incident metrics.

`Owner Decision Required`:

- `MAX_TECHNICAL_RETRY_PER_ATTEMPT`.
- `TECHNICAL_RETRY_BACKOFF_SECONDS`.
- Mapping `BUSY`, `REJECTED`, `UNREACHABLE`, `CALL_DROPPED` theo tín hiệu SIM Gateway thực tế.
- Production SIM Gateway hardware/API protocol.

## 9. Giám sát sức khỏe hệ thống

Adapter phải expose trạng thái:

- SIM healthy/degraded/disabled.
- Active call count.
- Channel reserve failure.
- DTMF capture failure rate.
- Audio playback failure rate.
- Callback failure rate.
- Capacity risk.

Disable/enable SIM phải đi qua permission trong `IVR-08`.

## 10. Tiêu chí chấp nhận

- Adapter không có quyền update order.
- Adapter không gửi notification.
- Technical failure được tách khỏi no-answer.
- SIM health và channel lifecycle có audit.
- Các owner decision kỹ thuật còn thiếu được đánh dấu rõ.

## 11. Adapter interface detail

| Operation            | Input                                        | Output                               | Side effect              |
| -------------------- | -------------------------------------------- | ------------------------------------ | ------------------------ |
| `ReserveSimChannel`  | Attempt context, queue, program.             | SIM reservation or capacity failure. | Marks SIM reserved.      |
| `ReleaseSimChannel`  | Reservation id, final status.                | Release ack.                         | Marks SIM idle/degraded. |
| `DialOutboundCall`   | SIM reservation, dial token, script version. | Provider/call id.                    | Starts call.             |
| `PlayScript`         | Approved script template and variables.      | Playback status.                     | Audio playback.          |
| `CaptureDtmf`        | Active call id.                              | DTMF key/status.                     | Captures customer input. |
| `GetCallDisposition` | Active call id.                              | Raw call outcome.                    | None.                    |
| `HealthCheck`        | SIM channel id.                              | Healthy/degraded/failed.             | Updates health evidence. |

## 12. SIM channel lifecycle

```text
ENABLED_IDLE
  -> RESERVED
  -> ACTIVE_CALL
  -> RELEASING
  -> ENABLED_IDLE

Any state -> DISABLED_BY_ADMIN
Any active state -> HEALTH_FAILED
HEALTH_FAILED -> DISABLED_BY_ADMIN or ENABLED_IDLE after review
```

Rules:

- `RESERVED` must have TTL.
- `ACTIVE_CALL` must have exactly one attempt.
- `DISABLED_BY_ADMIN` cannot be selected by scheduler.
- `HEALTH_FAILED` cannot auto-enable without health pass/review.

## 13. Raw outcome taxonomy

| Raw adapter outcome           | Normalized category      | Counted customer attempt |
| ----------------------------- | ------------------------ | ------------------------ |
| Answered + DTMF `1`           | `IVR_CONFIRMED`          | Yes                      |
| Answered + DTMF `0`           | `IVR_CUSTOMER_CANCELLED` | Yes                      |
| Ring timeout under policy     | `NO_ANSWER`              | Yes                      |
| Invalid key                   | `DTMF_INVALID`           | Owner policy             |
| DTMF timeout after answer     | `DTMF_TIMEOUT`           | Owner policy             |
| Busy                          | `BUSY`                   | Owner Decision Required  |
| Rejected                      | `REJECTED`               | Owner Decision Required  |
| Unreachable                   | `UNREACHABLE`            | Owner Decision Required  |
| SIM gateway error             | `SIM_GATEWAY_ERROR`      | No                       |
| Audio playback error          | `AUDIO_PLAYBACK_ERROR`   | No                       |
| DTMF capture error            | `DTMF_CAPTURE_ERROR`     | No                       |
| Callback/evidence write error | Technical exception      | No                       |

## 14. Script delivery rules

Adapter may only play:

- Approved script template.
- Approved script version.
- Approved variables.

Adapter must not play:

- Full customer address.
- Full customer profile.
- Payment detail.
- Health/sensitive notes.
- CRM/AI consultation content.
- Marketing/upsell message.

## 15. Adapter security controls

| Control          | Requirement                                                     |
| ---------------- | --------------------------------------------------------------- |
| Credential scope | SIM credential only, no Order Core write credential.            |
| Secret storage   | Secret manager/config boundary, no UI/log exposure.             |
| Logging          | No raw phone unless explicitly approved; redact tokens.         |
| Network          | Internal network only.                                          |
| Admin            | Disable/enable through IVR admin API, not adapter local button. |
| Recording        | Disabled unless privacy/legal owner approves.                   |

## 16. Adapter failure handling

| Failure                   | Required output                            |
| ------------------------- | ------------------------------------------ |
| Cannot reserve SIM        | Capacity incident or technical exception.  |
| Dial fails before ring    | Technical exception, not no-answer.        |
| Audio fails               | Technical exception, not customer attempt. |
| DTMF capture fails        | Technical exception.                       |
| Call drops after answered | `CALL_DROPPED`, owner decision for count.  |
| SIM hangs active          | Health failed, admin review.               |

## 17. Adapter smoke tests

| Test ID       | Scenario              | Expected                          |
| ------------- | --------------------- | --------------------------------- |
| IVR06-SMK-001 | Fake adapter DTMF `1` | `IVR_CONFIRMED`.                  |
| IVR06-SMK-002 | Fake adapter DTMF `0` | `IVR_CUSTOMER_CANCELLED`.         |
| IVR06-SMK-003 | Fake no-answer        | `NO_ANSWER`.                      |
| IVR06-SMK-004 | SIM gateway error     | Technical exception, not counted. |
| IVR06-SMK-005 | Disabled SIM selected | Scheduler refuses.                |
| IVR06-SMK-006 | Two calls same SIM    | Second reserve fails.             |
| IVR06-SMK-007 | Raw phone in logs     | Test fails.                       |

---

<!-- SOURCE_FILE: IVR-07-result-normalization-order-core-callback.md -->

# IVR-07 - Result Normalization / IVR -> Order Core Callback

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa result taxonomy, callback contract và race-condition revalidation.

## 1. Mục tiêu

IVR Result Normalizer chuyển raw SIM/DTMF result thành outcome chuẩn và gửi callback về Order Core. Order Core luôn revalidate trước khi transition. IVR không được ép trạng thái đơn.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                       |
| ------------------------------------------------------------------------------------------------------------ | --------------------------------------------- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`                                                | IVR result là input signal.                   |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Outcome classification và callback boundary.  |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`      | Order Core/Payment/Verified Revenue boundary. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`                                                 | Race condition và callback contract baseline. |

## 3. Tổng quan contract

| Thuộc tính    | Giá trị                                   |
| ------------- | ----------------------------------------- |
| Contract name | `IvrConfirmationResultCallbackV1`         |
| Direction     | IVR Runtime -> Order Core                 |
| Producer      | IVR Runtime Orchestrator                  |
| Consumer      | Commerce Order Core / Order State Machine |
| Role          | Result signal only                        |
| Idempotency   | Bắt buộc                                  |
| Correlation   | Bắt buộc                                  |

## 4. Trường dữ liệu bắt buộc

| Field                         | Required     | Ghi chú                           |
| ----------------------------- | ------------ | --------------------------------- |
| `callback_id`                 | Có           | Unique callback.                  |
| `task_id`                     | Có           | Link task gốc.                    |
| `call_job_id`                 | Có điều kiện | Có nếu đã tạo CallJob.            |
| `attempt_id`                  | Có điều kiện | Có với attempt-level result.      |
| `idempotency_key`             | Có           | Chống duplicate transition.       |
| `correlation_id`              | Có           | Trace.                            |
| `order_id`                    | Có           | Order gốc.                        |
| `order_version_seen_by_ivr`   | Có           | Race guard.                       |
| `program_code`                | Có           | Context chương trình.             |
| `attempt_policy_code`         | Có           | Context policy.                   |
| `attempt_no`                  | Có điều kiện | Required cho attempt result.      |
| `max_attempts`                | Có           | 2 hoặc 3 theo chương trình.       |
| `result_type`                 | Có           | Outcome chuẩn.                    |
| `result_reason`               | Có           | Lý do chi tiết.                   |
| `dtmf_key`                    | Có điều kiện | `1`, `0`, invalid hoặc null.      |
| `is_counted_customer_attempt` | Có           | False cho technical failure.      |
| `is_final_for_ivr`            | Có           | IVR có dừng không.                |
| `recommended_core_action`     | Có           | Recommendation only.              |
| `technical_error_code`        | Có điều kiện | Required nếu technical exception. |
| `evidence_ref`                | Có           | Evidence Registry reference.      |
| `audit_ref`                   | Có           | Audit reference.                  |
| `privacy_policy_version`      | Có           | Privacy context.                  |
| `script_version`              | Có           | Script đã phát.                   |
| `created_at`                  | Có           | Callback timestamp.               |

## 5. Loại kết quả

| Result type                       | Ý nghĩa                                       | Counted?    | Final?             |
| --------------------------------- | --------------------------------------------- | ----------- | ------------------ |
| `IVR_CONFIRMED`                   | Khách bấm `1`                                 | Có          | Có                 |
| `IVR_CUSTOMER_CANCELLED`          | Khách bấm `0`                                 | Có          | Có                 |
| `IVR_NO_ANSWER_ATTEMPT`           | Attempt hợp lệ nhưng không nghe/không input   | Có          | Không nếu chưa max |
| `IVR_NO_ANSWER_FINAL`             | No-answer sau max attempt                     | Có          | Có                 |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết window trước confirmation hợp lệ          | Tùy attempt | Có                 |
| `INVALID_PHONE_FINAL`             | Phone invalid theo policy                     | Không       | Có                 |
| `IVR_TECHNICAL_EXCEPTION`         | Lỗi kỹ thuật                                  | Không       | Tùy policy         |
| `IVR_POLICY_BLOCKED`              | Policy/source unavailable hoặc blocked        | Không       | Có hoặc review     |
| `IVR_OPERATIONAL_BLOCKED`         | Sale Lock/Recall/Suppression/call restriction | Không       | Có                 |
| `IVR_CUSTOMER_NEEDS_SUPPORT`      | Khách cần hỗ trợ nếu future key enabled       | Có          | Có/review          |

## 6. Hành động khuyến nghị cho Core

`recommended_core_action` không phải command. Order Core phải revalidate.

Allowed values:

- `CORE_REVALIDATE_AND_CONFIRM_ORDER`.
- `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST`.
- `CORE_REVALIDATE_AND_CANCEL_NO_ANSWER`.
- `CORE_REVALIDATE_AND_EXPIRE_CONFIRMATION`.
- `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`.
- `CORE_IGNORE_STALE_CALLBACK`.
- `CORE_BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT`.

## 7. Result state machine

| State                           | Ý nghĩa                            | Next states                                                                           |
| ------------------------------- | ---------------------------------- | ------------------------------------------------------------------------------------- |
| `RESULT_NOT_NORMALIZED`         | Chỉ có raw adapter result          | `RESULT_NORMALIZED`, `RESULT_REJECTED_NEEDS_REVIEW`                                   |
| `RESULT_NORMALIZED`             | Outcome đã phân loại               | `RESULT_EVIDENCE_PENDING`, `RESULT_REJECTED_NEEDS_REVIEW`                             |
| `RESULT_EVIDENCE_PENDING`       | Chờ ghi evidence                   | `RESULT_READY_FOR_CALLBACK`, `RESULT_EVIDENCE_FAILED`                                 |
| `RESULT_READY_FOR_CALLBACK`     | Payload callback sẵn sàng          | `RESULT_SENT_TO_CORE`                                                                 |
| `RESULT_SENT_TO_CORE`           | Đã gửi Core                        | `RESULT_CORE_ACCEPTED`, `RESULT_CORE_REJECTED_STALE`, `RESULT_CALLBACK_RETRY_PENDING` |
| `RESULT_CORE_ACCEPTED`          | Core nhận hợp lệ                   | Terminal cho IVR                                                                      |
| `RESULT_CORE_REJECTED_STALE`    | Core reject do stale/version/state | Terminal cho IVR                                                                      |
| `RESULT_CALLBACK_RETRY_PENDING` | Retry callback kỹ thuật            | `RESULT_SENT_TO_CORE`, `RESULT_ADMIN_REVIEW_REQUIRED`                                 |
| `RESULT_EVIDENCE_FAILED`        | Không ghi evidence                 | `RESULT_ADMIN_REVIEW_REQUIRED`                                                        |
| `RESULT_ADMIN_REVIEW_REQUIRED`  | Cần người review                   | Terminal đến khi admin xử lý                                                          |

## 8. Revalidation khi có race condition

Order Core phải revalidate khi nhận callback:

- Callback idempotency.
- `task_id`, `order_id`, `order_version_seen_by_ivr`.
- Order state hiện tại còn nhận IVR outcome không.
- Order amount/items/contact có thay đổi làm confirmation cũ không hợp lệ không.
- Sale Lock/Recall/Suppression/payment/availability hiện tại.
- Evidence/audit refs tồn tại.
- Result type khớp attempt policy.

Nếu khách bấm `1` nhưng Sale Lock/Recall/payment issue xuất hiện trước khi Core accept:

- IVR result vẫn là `IVR_CONFIRMED` như raw customer signal.
- Core action là block hoặc hold admin review.
- Order không được tiếp tục chỉ vì phím `1`.
- Evidence phải link cả IVR signal và blocker.

## 9. Tiêu chí chấp nhận

- Callback đủ dữ liệu để Core revalidate.
- Result taxonomy tách confirm/cancel/no-answer/invalid/technical/block.
- Stale callback không ép order transition.
- No-answer max không tự notification.

## 10. Result normalization mapping table

| Input từ adapter/service              | Điều kiện                                 | Normalized result                      | Counted? | Final?         |
| ------------------------------------- | ----------------------------------------- | -------------------------------------- | -------- | -------------- |
| DTMF `1`                              | Answered, script valid, evidence recorded | `IVR_CONFIRMED`                        | Yes      | Yes            |
| DTMF `0`                              | Answered, script valid, evidence recorded | `IVR_CUSTOMER_CANCELLED`               | Yes      | Yes            |
| No answer                             | Attempt valid, not max                    | `IVR_NO_ANSWER_ATTEMPT`                | Yes      | No             |
| No answer                             | Attempt valid, max reached                | `IVR_NO_ANSWER_FINAL`                  | Yes      | Yes            |
| Window expired                        | No accepted final signal                  | `IVR_CONFIRMATION_WINDOW_EXPIRED`      | Depends  | Yes            |
| Phone invalid                         | Validation confirms invalid               | `INVALID_PHONE_FINAL`                  | No       | Yes            |
| SIM/server/audio/DTMF technical error | Technical source                          | `IVR_TECHNICAL_EXCEPTION`              | No       | Depends/review |
| Capacity hold                         | Queue/SIM capacity unsafe                 | `IVR_CAPACITY_EXCEPTION`               | No       | Review         |
| Sale Lock/Recall/payment issue        | Core revalidation                         | `IVR_OPERATIONAL_BLOCKED`              | No       | Yes/review     |
| Ambiguous raw status                  | No source-backed mapping                  | `IVR_CUSTOMER_NEEDS_SUPPORT` or review | Depends  | Review         |

## 11. Callback payload validation

Callback must include:

| Field                         | Validation                            |
| ----------------------------- | ------------------------------------- |
| `callback_id`                 | Unique/idempotent.                    |
| `task_id`                     | Existing task.                        |
| `call_job_id`                 | Existing job if job was created.      |
| `attempt_id`                  | Required for attempt-level outcome.   |
| `order_id`                    | Matches task.                         |
| `order_version_seen_by_ivr`   | Required race guard.                  |
| `program_code`                | Matches task.                         |
| `attempt_no`                  | Within program policy.                |
| `result_type`                 | Known result taxonomy.                |
| `is_counted_customer_attempt` | False for technical/invalid/capacity. |
| `evidence_ref`                | Required before final action.         |
| `audit_ref`                   | Required.                             |
| `privacy_policy_version`      | Required.                             |

## 12. Order Core callback response contract

| Core response                        | Meaning                                            | IVR action                                                     |
| ------------------------------------ | -------------------------------------------------- | -------------------------------------------------------------- |
| `CALLBACK_ACCEPTED_FOR_REVALIDATION` | Core accepted signal.                              | Mark callback accepted; wait/read final order state if needed. |
| `CALLBACK_REJECTED_STALE`            | Task/order version stale.                          | Mark stale; do not retry as new business signal.               |
| `CALLBACK_BLOCKED_BY_CORE`           | Sale Lock/Recall/payment/suppression/policy block. | Mark blocked; evidence link.                                   |
| `CALLBACK_NEEDS_ADMIN_REVIEW`        | Core requires owner/admin review.                  | Mark review required.                                          |
| `CALLBACK_TECHNICAL_RETRY_ALLOWED`   | Core unavailable/transient.                        | Retry bounded with same idempotency key.                       |
| `CALLBACK_TECHNICAL_RETRY_BLOCKED`   | Retry unsafe or expired.                           | Admin review.                                                  |

## 13. Race condition matrix

| Race                                          | Detection                            | Required behavior                               |
| --------------------------------------------- | ------------------------------------ | ----------------------------------------------- |
| Order version changed after task              | `order_version_seen_by_ivr` mismatch | Reject stale or re-evaluate by Core.            |
| Customer presses `1`, Sale Lock appears       | Core blocker check                   | Do not confirm; block/hold.                     |
| Customer presses `0`, order already cancelled | Order state check                    | Idempotent no-op or stale.                      |
| No-answer final, payment issue appears        | Core revalidation                    | Core decides hold/cancel; IVR no direct action. |
| Duplicate callback                            | Idempotency                          | Return previous ack.                            |
| Evidence missing                              | Evidence check                       | Reject/hold/review.                             |

## 14. Callback retry policy

Retry is technical only. It must not:

- Create a new result.
- Increment customer attempt.
- Change result status.
- Bypass stale/order state guard.
- Retry forever.

Retry requires:

- Same `callback_id` or same idempotency scope.
- Retry counter.
- Last error.
- Next retry time.
- Admin escalation after max retry.

Exact retry count/backoff remains `Owner Decision Required`.

## 15. Result event publication rules

Events may be published for signal visibility:

- `ivr-confirmation-requested`.
- `ivr-confirmed`.
- `ivr-customer-cancelled`.
- `ivr-no-answer-final`.
- `ivr-invalid-phone-final`.
- `ivr-technical-exception`.
- `ivr-operational-blocked`.
- `ivr-capacity-incident-opened`.

Events must not:

- Represent final order state unless event name belongs to Order Core.
- Replace callback.
- Feed verified revenue.
- Trigger notification without Core decision.

## 16. Result normalization P0 tests

| Test ID      | Scenario                        | Expected                               |
| ------------ | ------------------------------- | -------------------------------------- |
| IVR07-P0-001 | DTMF `1`                        | Confirm signal only, Core revalidates. |
| IVR07-P0-002 | DTMF `0`                        | Cancel signal only, Core decides.      |
| IVR07-P0-003 | SIM error                       | Technical exception, not no-answer.    |
| IVR07-P0-004 | Invalid phone                   | Invalid final, not no-answer.          |
| IVR07-P0-005 | Stale callback                  | No order transition.                   |
| IVR07-P0-006 | Missing evidence                | Callback held/rejected.                |
| IVR07-P0-007 | Operational block after key `1` | Core block, no auto confirm.           |

---

<!-- SOURCE_FILE: IVR-08-admin-monitoring-evidence-audit-privacy.md -->

# IVR-08 - Admin Monitoring / Evidence / Audit / Privacy

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa yêu cầu Admin Monitoring, RBAC, evidence, audit, privacy và retention cho IVR.

## 1. Mục tiêu

Admin/Ops có thể quan sát và xử lý vận hành IVR, nhưng mọi hành động rủi ro phải đi qua permission, reason, evidence và audit. Privacy là gate bắt buộc vì IVR xử lý số điện thoại, call log và DTMF.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                        | Vai trò                                         |
| ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------- |
| `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md`                   | RBAC, audit, evidence registry, idempotency.    |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md`                                    | Evidence status, smoke mapping, owner sign-off. |
| `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md`                                           | Completion/evidence gateway.                    |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | IVR privacy/evidence/admin override.            |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md`                                                 | Permission matrix, retention/privacy baseline.  |

## 3. Phạm vi giám sát admin

Admin/Ops UI được phép hiển thị:

- Queue status.
- CallJob status.
- Attempt status.
- SIM channel health.
- Capacity incident.
- Technical exception.
- Result callback status.
- Evidence/audit refs.
- Masked phone/contact.
- Program policy và attempt count.

Admin/Ops UI không được hiển thị mặc định:

- Full phone number nếu chưa có permission.
- Full address.
- Payment detail.
- Member/Diamond/referral details.
- Health note.
- CRM/private consultation content.
- Raw recording/transcript nếu chưa được privacy/legal approve.

## 4. Ma trận quyền

| Action                 | Permission                 | Role tối thiểu                   | Evidence required                 | Hard block                                 |
| ---------------------- | -------------------------- | -------------------------------- | --------------------------------- | ------------------------------------------ |
| Xem queue              | `ivr.queue.read`           | Ops Viewer                       | Access audit                      | Không lộ PII thô.                          |
| Xem call detail        | `ivr.call.read`            | IVR Operator                     | Access audit                      | Không xem recording nếu chưa approved.     |
| Retry lỗi kỹ thuật     | `ivr.call.retry_technical` | IVR Operator                     | Reason, exception id              | Không kéo dài window.                      |
| Retry customer attempt | `ivr.call.retry_customer`  | Admin                            | Owner reason, Core approval       | Không vượt max attempt/call restriction.   |
| Pause queue            | `ivr.queue.pause`          | Admin/Incident Manager           | Reason, scope, duration           | Không silent miss SLA.                     |
| Resume queue           | `ivr.queue.resume`         | Admin/Incident Manager           | Before/after metrics              | Re-check windows.                          |
| Disable SIM            | `ivr.sim.disable`          | IVR Operator                     | SIM id, health reason             | Không mất in-flight audit.                 |
| Enable SIM             | `ivr.sim.enable`           | Admin                            | Health evidence                   | Không enable SIM failing health.           |
| Sửa script/template    | `ivr.script.update`        | IVR Owner/Admin                  | Approval, version, privacy review | Không dùng script tự sinh chưa duyệt.      |
| Review outcome         | `ivr.outcome.review`       | Admin/Auditor                    | Reason, before/after              | Không force paid/revenue.                  |
| Override outcome       | `ivr.outcome.override`     | Restricted Admin + Core approval | Dual evidence, Core transition    | Không bypass Sale Lock/Recall/Suppression. |
| Export evidence        | `ivr.evidence.export`      | Auditor/Admin                    | Export audit, purpose             | Không dùng marketing.                      |
| Production enable      | `ivr.production.enable`    | Release Owner                    | Full release evidence             | Không gọi khách thật nếu gate chưa pass.   |

## 5. Evidence bắt buộc

| Evidence                      | Khi nào ghi         | Nội dung tối thiểu                           |
| ----------------------------- | ------------------- | -------------------------------------------- |
| `IVR_TASK_INTAKE_EVIDENCE`    | Khi nhận task       | task id, order id, decision, policy version. |
| `IVR_ELIGIBILITY_EVIDENCE`    | Khi xét eligibility | trust, risk, blocker, contact, phone status. |
| `IVR_ATTEMPT_EVIDENCE`        | Mỗi attempt         | attempt no, schedule, SIM channel, result.   |
| `IVR_DTMF_EVIDENCE`           | Khi capture DTMF    | key normalized, timestamp, script version.   |
| `IVR_TECH_EXCEPTION_EVIDENCE` | Khi lỗi kỹ thuật    | error code, adapter trace, not-counted flag. |
| `IVR_CALLBACK_EVIDENCE`       | Khi callback Core   | callback id, idempotency, result type.       |
| `IVR_ADMIN_ACTION_EVIDENCE`   | Admin action        | actor, permission, reason, before/after.     |
| `IVR_RELEASE_EVIDENCE`        | Release gate        | smoke result, owner sign-off, blockers.      |

Evidence phải có trạng thái. Evidence `DRAFT` hoặc `SUBMITTED` không đủ để gọi PASS nếu gate yêu cầu `ACCEPTED`.

## 6. Audit requirements

Audit bắt buộc cho:

- Task intake/reject.
- Eligibility decision.
- Attempt dispatch.
- SIM channel reserve/release.
- DTMF capture.
- Result normalization.
- Callback sent/accepted/rejected.
- Technical retry.
- Queue pause/resume.
- SIM disable/enable.
- Admin review/override.
- Script version change.
- Evidence export.
- Production enable/disable.

Audit tối thiểu gồm:

- `actor_type`.
- `actor_id` hoặc system actor.
- `permission`.
- `action`.
- `target_ref`.
- `reason`.
- `before_state`.
- `after_state`.
- `correlation_id`.
- `evidence_ref`.
- timestamp.

## 7. Chính sách privacy

| Data                  | Classification              | Default rule                             |
| --------------------- | --------------------------- | ---------------------------------------- |
| Full phone number     | PII                         | Dùng token/ref nếu có thể; mask trên UI. |
| `phone_masked`        | Operational PII             | Chỉ role được phép xem.                  |
| `official_contact_id` | Internal reference          | Được lưu evidence.                       |
| Call status           | Operational evidence        | Lưu theo retention policy.               |
| DTMF key              | Order confirmation evidence | Không public.                            |
| Call recording        | Sensitive evidence          | Disabled until approved.                 |
| Transcript            | Sensitive evidence          | Not required V1.0.                       |
| SIM channel id        | Technical evidence          | Được dùng troubleshooting.               |
| Admin comment         | Operational evidence        | Required cho manual action.              |

## 8. Recording và retention

Baseline V1.0:

- `CALL_RECORDING_ENABLED = NO_UNTIL_OWNER_APPROVED`.
- `CALL_TRANSCRIPT_ENABLED = NO_UNTIL_OWNER_APPROVED`.
- Retention duration là `Owner Decision Required`.

Nếu sau này bật recording, SRS/contract phải bổ sung:

- Legal/consent basis.
- Recording start/stop rule.
- Retention duration.
- Access roles.
- Export restriction.
- Deletion/anonymization rule.
- Incident handling nếu rò rỉ.

## 9. Cổng chặn lỗi

IVR-08 FAIL nếu:

- Admin action không có permission server-side.
- Manual retry vượt policy/window.
- Override bypass Sale Lock/Recall/Suppression.
- Evidence thiếu audit hoặc sai scope.
- UI/log lộ PII ngoài policy.
- Recording bật khi chưa có privacy/legal approval.
- Evidence bị dùng cho marketing.

## 10. Tiêu chí chấp nhận

- Permission matrix đủ cho review, retry, pause queue, disable SIM, evidence export, production enable.
- Evidence/audit model đủ trace.
- Privacy/retention ghi rõ điều đã khóa và điều cần owner decision.
- Không có đường admin override trực tiếp sang paid/revenue/order completed.

## 11. Admin screen inventory

| Screen               | Purpose                                 | Minimum fields                                            | Actions                    |
| -------------------- | --------------------------------------- | --------------------------------------------------------- | -------------------------- |
| IVR Dashboard        | Tổng quan sức khỏe queue/SIM/result.    | Active jobs, due attempts, capacity, exceptions.          | View only.                 |
| Call Jobs            | Danh sách job.                          | Job id, order id masked/short, program, status, deadline. | Open detail.               |
| Call Job Detail      | Trace task/job/attempt/result/callback. | Timeline, evidence refs, audit refs, blocker refs.        | Review if permission.      |
| SIM Channels         | SIM health/capacity.                    | SIM id, enabled, status, active call, last health.        | Disable/enable.            |
| Capacity Incidents   | Incident queue/SIM.                     | Incident id, scope, status, reason, owner.                | Pause/resume/escalate.     |
| Technical Exceptions | Lỗi kỹ thuật.                           | Type, attempt, counted=false, retry allowed.              | Manual technical retry.    |
| Result Review        | Ambiguous/stale/blocked results.        | Result, callback ack, blocker, evidence.                  | Add review note.           |
| Evidence Viewer      | Evidence/audit refs.                    | Evidence id, status, source, owner.                       | Export only if permission. |

## 12. Admin action validation

| Action          | Required permission              | Required reason | Evidence required            | Hard blocks                                            |
| --------------- | -------------------------------- | --------------- | ---------------------------- | ------------------------------------------------------ |
| Pause queue     | `IVR_QUEUE_PAUSE`                | Yes             | Audit required               | None, safe action.                                     |
| Resume queue    | `IVR_QUEUE_RESUME`               | Yes             | Incident resolved evidence   | Cannot resume if release/security block active.        |
| Disable SIM     | `IVR_SIM_DISABLE`                | Yes             | Health/failure evidence      | Cannot drop active call unsafely.                      |
| Enable SIM      | `IVR_SIM_ENABLE`                 | Yes             | Health pass evidence         | Cannot enable failed/unreviewed SIM.                   |
| Manual retry    | `IVR_MANUAL_RETRY`               | Yes             | Technical exception evidence | Cannot count customer attempt; cannot bypass blockers. |
| Result review   | `IVR_RESULT_REVIEW`              | Yes             | Review evidence              | Cannot force order transition.                         |
| Evidence export | `IVR_EVIDENCE_EXPORT` if defined | Yes             | Audit access                 | Cannot export raw PII without policy.                  |

## 13. Audit trail detail

Every admin action must record:

- `admin_action_id`.
- `actor_id`.
- `permission`.
- `target_type`.
- `target_id`.
- `reason`.
- `state_before`.
- `state_after`.
- `correlation_id`.
- `evidence_ref`.
- `created_at`.

Audit record must be immutable or append-only according to foundation policy.

## 14. Privacy display rules

| Data          | Admin list                      | Admin detail                                   | Evidence export                |
| ------------- | ------------------------------- | ---------------------------------------------- | ------------------------------ |
| Phone         | Masked only.                    | Masked only unless explicit permission/policy. | Owner Decision Required.       |
| Order id/code | Short/id allowed.               | Allowed internal.                              | Allowed internal.              |
| Customer name | Short display only if approved. | Minimal projection.                            | Owner Decision Required.       |
| Address       | Hidden.                         | Hidden by default.                             | Not exported from IVR.         |
| DTMF key      | Result semantic allowed.        | Allowed to authorized reviewer.                | Allowed as evidence if policy. |
| Recording     | Hidden/disabled.                | Disabled unless approved.                      | Owner Decision Required.       |
| SIM logs      | Sanitized.                      | Sanitized.                                     | Sanitized only.                |

## 15. Evidence packet model

For each final result, evidence packet should link:

- Task intake decision.
- Eligibility decision.
- Attempt schedule.
- Attempt execution/call disposition.
- DTMF/result normalization.
- Callback request.
- Core callback ack/reject/block.
- Admin actions if any.
- Technical/capacity incidents if any.

Evidence packet must not include:

- Unredacted phone without approval.
- Secrets/SIM credentials.
- Full customer profile.
- Marketing notes.

## 16. Privacy and retention decisions

| Decision                | Default until approved    | Needed for              |
| ----------------------- | ------------------------- | ----------------------- |
| Recording enabled?      | No                        | Any audio storage.      |
| Raw phone retention     | No long-term storage      | Production SIM dialing. |
| DTMF evidence retention | Minimal semantic evidence | Dispute/review.         |
| Call log retention      | Owner Decision Required   | Ops/debug.              |
| Admin audit retention   | Foundation policy         | Compliance.             |
| Evidence export scope   | Restricted                | Release/dispute.        |

## 17. Admin P0 tests

| Test ID      | Scenario                                    | Expected                            |
| ------------ | ------------------------------------------- | ----------------------------------- |
| IVR08-P0-001 | User without permission pauses queue        | 403.                                |
| IVR08-P0-002 | Admin manual retry on no-answer             | Blocked unless technical exception. |
| IVR08-P0-003 | Admin enable failed SIM without health pass | Blocked.                            |
| IVR08-P0-004 | Admin tries force order cancel              | No route/403.                       |
| IVR08-P0-005 | Admin UI shows raw phone                    | Test fail.                          |
| IVR08-P0-006 | Evidence export includes secret             | Test fail.                          |
| IVR08-P0-007 | Recording enabled without approval          | Release fail.                       |

---

<!-- SOURCE_FILE: IVR-09-test-matrix-smoke-release-gate.md -->

# IVR-09 - Test Matrix / Smoke / Release Gate

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa test matrix, smoke, evidence và release gate cho real customer call.

## 1. Mục tiêu

IVR chỉ được gọi khách thật khi documentation, implementation, privacy, security, smoke, evidence, owner sign-off và release decision đều đạt. Tài liệu này xác định acceptance test ở mức SRS.

## 2. Nguồn tham chiếu

| Nguồn                                                                                                         | Vai trò                             |
| ------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md`                                     | Evidence/smoke/completion gate.     |
| `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md`                                            | Release/go-live control.            |
| `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md`                                            | Completion/evidence gateway.        |
| `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-EVIDENCE-RELEASE-GATEWAY-PRODUCTION-READINESS-CONTROL.md` | Global smoke/UAT/release readiness. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md`  | IVR final smoke matrix.             |

## 3. Cổng tài liệu

Trước khi triển khai:

- `IVR-00` đến `IVR-09` hoàn tất.
- `IVR-SRS-trace-matrix.md` hoàn tất.
- PACK-09, TECH-09 và Phase 8 baseline không mâu thuẫn attempt policy.
- Owner decision chưa có nguồn được đánh dấu rõ.
- Không có production claim.

## 4. Cổng sẵn sàng triển khai

Trước khi bật staging/prod-like:

- Order Core chỉ tạo task cho Official Order đủ điều kiện.
- IVR không có quyền update order.
- Order Core revalidate mọi callback.
- Internal SIM Gateway Adapter có health monitoring.
- Evidence/audit writer hoạt động.
- RBAC server-side cho admin actions.
- Kill switch/pause queue/disable SIM có audit.

## 5. Ma trận smoke bắt buộc

| Smoke ID    | Scenario                             | Expected                                           |
| ----------- | ------------------------------------ | -------------------------------------------------- |
| IVR-SMK-001 | Quote đi vào IVR                     | Bị reject, không tạo CallJob.                      |
| IVR-SMK-002 | Cart đi vào IVR                      | Bị reject, không tạo CallJob.                      |
| IVR-SMK-003 | Order Draft đi vào IVR               | Bị reject, không tạo CallJob.                      |
| IVR-SMK-004 | Official Order khách mới             | Eligible nếu không có blocker.                     |
| IVR-SMK-005 | Khách trusted không risk             | Skip IVR.                                          |
| IVR-SMK-006 | Khách trusted có risk mới            | Route IVR hoặc human review theo policy.           |
| IVR-SMK-007 | Phone invalid format                 | Không gọi, invalid phone outcome.                  |
| IVR-SMK-008 | Phone validation technical error     | Technical exception, không tính attempt.           |
| IVR-SMK-009 | Golden Hour attempt policy           | Chỉ 2 customer attempts trong 10 phút.             |
| IVR-SMK-010 | 24/7 attempt policy                  | Chỉ 3 customer attempts trong 15 phút.             |
| IVR-SMK-011 | Khách bấm `1`                        | Callback `IVR_CONFIRMED`, Core revalidate.         |
| IVR-SMK-012 | Khách bấm `0`                        | Callback `IVR_CUSTOMER_CANCELLED`, Core cancel.    |
| IVR-SMK-013 | No-answer chưa max                   | Queue attempt kế tiếp đúng schedule.               |
| IVR-SMK-014 | No-answer max                        | Callback final, IVR không notification.            |
| IVR-SMK-015 | SIM failure                          | Technical exception, không tính attempt.           |
| IVR-SMK-016 | DTMF capture error                   | Technical exception, không tính customer fault.    |
| IVR-SMK-017 | Sale Lock trước dispatch             | Không gọi, operational blocked.                    |
| IVR-SMK-018 | Recall trong lúc gọi                 | Core block/hold khi callback.                      |
| IVR-SMK-019 | Payment issue xuất hiện sau phím `1` | Core không auto-confirm.                           |
| IVR-SMK-020 | Callback duplicate                   | Idempotent, không duplicate transition.            |
| IVR-SMK-021 | Callback stale order version         | Core reject stale hoặc hold review.                |
| IVR-SMK-022 | Evidence unavailable                 | Result không được dùng transition.                 |
| IVR-SMK-023 | Admin retry technical                | Permission/reason/evidence bắt buộc.               |
| IVR-SMK-024 | Admin override bypass blocker        | Bị chặn.                                           |
| IVR-SMK-025 | Queue pause/resume                   | Audit đầy đủ, re-check windows.                    |
| IVR-SMK-026 | Disable/enable SIM                   | Permission/health evidence.                        |
| IVR-SMK-027 | Privacy script                       | Không đọc full address/payment/member/health note. |
| IVR-SMK-028 | Recording disabled                   | Không recording khi chưa approved.                 |
| IVR-SMK-029 | Production flag off                  | Không gọi khách thật.                              |
| IVR-SMK-030 | Release gate incomplete              | `REAL_CUSTOMER_CALL_ALLOWED` vẫn `NO`.             |

## 6. Gói bằng chứng

Release evidence tối thiểu:

- SRS trace matrix.
- Contract semantic review.
- Eligibility smoke evidence.
- Attempt policy smoke evidence.
- SIM Gateway internal-number smoke evidence.
- DTMF evidence.
- Callback/idempotency evidence.
- Race-condition evidence.
- Sale Lock/Recall/Suppression evidence.
- Admin RBAC evidence.
- Privacy review evidence.
- Security review evidence.
- Owner sign-off.
- Release decision audit.

## 7. Cổng release production

Required trước khi `REAL_CUSTOMER_CALL_ALLOWED = YES`:

- SRS approved.
- Implementation approved.
- Privacy/legal retention approved.
- Pilot scope approved.
- Kill switch tested.
- Queue pause tested.
- SIM disable tested.
- Duplicate callback tested.
- Stale callback tested.
- Evidence accepted.
- IVR Owner sign-off.
- Order Core Owner sign-off.
- Operational/Sale Lock Owner sign-off.
- Privacy Owner sign-off.
- Release Owner sign-off.

## 8. Cổng chặn lỗi

Release FAIL nếu:

- IVR gọi khách thật khi production flag chưa mở.
- IVR tự update order.
- IVR gọi Quote/Cart/Order Draft.
- Golden Hour có attempt thứ 3.
- 24/7 có attempt thứ 4.
- Technical failure bị tính là no-answer.
- Invalid phone bị tính là no-answer.
- Admin override bỏ qua Sale Lock/Recall/Suppression.
- Evidence chưa accepted nhưng gọi PASS.
- Privacy/retention chưa được duyệt nhưng bật recording hoặc production call.

## 9. Tiêu chí chấp nhận

- Test matrix bao phủ toàn bộ tình huống user yêu cầu.
- Release gate trả lời rõ khi nào được gọi khách thật.
- Không có smoke nào yêu cầu schema/API/code ở phase SRS này.
- Phase sau có thể sinh contract-tests từ smoke matrix này.

## 10. Expanded test catalog

| Group            | Test IDs      | Purpose                                                         |
| ---------------- | ------------- | --------------------------------------------------------------- |
| Contract hygiene | IVR09-CON-\*  | Source-map, schema, API, event, fixture, stale wording.         |
| Task intake      | IVR09-TASK-\* | Official Order only, idempotency, policy, contact, blocker.     |
| Eligibility      | IVR09-ELIG-\* | Trusted skip, phone validation, opt-out, source unavailable.    |
| Scheduler        | IVR09-SCH-\*  | Program attempts, deadline, queue pause, capacity, concurrency. |
| SIM adapter      | IVR09-SIM-\*  | DTMF, no-answer, technical failure, one SIM one call.           |
| Result callback  | IVR09-CB-\*   | Confirm/cancel/no-answer/invalid/technical/stale/block.         |
| Admin            | IVR09-ADM-\*  | RBAC, pause/resume, SIM disable/enable, manual retry.           |
| Security/privacy | IVR09-SEC-\*  | Raw phone redaction, recording off, secrets hidden.             |
| Release          | IVR09-REL-\*  | Evidence packet, owner sign-off, config gate.                   |

## 11. Detailed smoke matrix

| Test ID        | Setup                                   | Action             | Expected                             |
| -------------- | --------------------------------------- | ------------------ | ------------------------------------ |
| IVR09-TASK-001 | Valid Official Order, Golden Hour       | Create task        | CallJob has 2 attempts.              |
| IVR09-TASK-002 | Valid Official Order, 24/7              | Create task        | CallJob has 3 attempts.              |
| IVR09-TASK-003 | Quote/Cart/Draft                        | Try create task    | Rejected, no CallJob.                |
| IVR09-TASK-004 | Duplicate idempotency same payload      | Retry task         | Same result, no duplicate.           |
| IVR09-TASK-005 | Duplicate idempotency different payload | Retry task         | Conflict.                            |
| IVR09-ELIG-001 | Trusted customer no risk                | Evaluate           | Skip IVR.                            |
| IVR09-ELIG-002 | Trusted customer with risk              | Evaluate           | Require IVR or review.               |
| IVR09-ELIG-003 | Invalid phone                           | Evaluate           | No SIM dispatch.                     |
| IVR09-ELIG-004 | Phone validation technical error        | Evaluate           | Review/technical, not invalid final. |
| IVR09-SCH-001  | Golden Hour                             | Run scheduler      | No attempt 3.                        |
| IVR09-SCH-002  | 24/7                                    | Run scheduler      | No attempt 4.                        |
| IVR09-SCH-003  | Queue paused                            | Due attempts exist | No dispatch.                         |
| IVR09-SCH-004  | Two workers                             | Same due attempt   | One dispatch only.                   |
| IVR09-SIM-001  | Fake adapter key `1`                    | Call               | `IVR_CONFIRMED`.                     |
| IVR09-SIM-002  | Fake adapter key `0`                    | Call               | `IVR_CUSTOMER_CANCELLED`.            |
| IVR09-SIM-003  | Fake adapter no-answer                  | Call               | Counted no-answer.                   |
| IVR09-SIM-004  | Fake adapter SIM failure                | Call               | Technical exception, not counted.    |
| IVR09-CB-001   | Key `1`, no blockers                    | Callback           | Core accepts/revalidates.            |
| IVR09-CB-002   | Key `1`, Sale Lock appears              | Callback           | Core blocks/holds.                   |
| IVR09-CB-003   | No-answer final                         | Callback           | No auto notification.                |
| IVR09-CB-004   | Stale order version                     | Callback           | Rejected stale.                      |
| IVR09-ADM-001  | No permission                           | Pause queue        | 403.                                 |
| IVR09-ADM-002  | Permission + reason                     | Pause queue        | Queue paused, audit created.         |
| IVR09-ADM-003  | Manual retry technical exception        | Retry              | Not counted as customer attempt.     |
| IVR09-SEC-001  | Generate logs                           | Inspect            | No raw phone/secrets.                |
| IVR09-REL-001  | Missing evidence                        | Release check      | Fail.                                |

## 12. Evidence checklist for release

| Evidence item                     | Required          |
| --------------------------------- | ----------------- |
| Contract validator output         | Yes               |
| API/integration test output       | Yes               |
| DB migration test output          | Yes               |
| Scheduler/concurrency test output | Yes               |
| SIM fake smoke output             | Yes               |
| Internal-number smoke output      | Before pilot      |
| Admin RBAC output                 | Yes               |
| Security/privacy review           | Yes               |
| Evidence/audit samples            | Yes               |
| Race-condition test               | Yes               |
| Owner sign-off                    | Yes               |
| Rollback runbook drill            | Before production |

## 13. Release decision tree

```text
Are docs/contracts approved?
  NO -> stop
Are backend/admin/worker tests passing?
  NO -> stop
Are security/privacy gates passing?
  NO -> stop
Is evidence packet accepted?
  NO -> stop
Is internal-number smoke passing?
  NO -> stop
Has owner approved pilot scope?
  NO -> keep REAL_CUSTOMER_CALL_ALLOWED=false
YES -> allow limited pilot only
```

## 14. Production hard blocks

Production cannot enable real customer calls if any condition is true:

- `REAL_CUSTOMER_CALL_ALLOWED` is false.
- Release evidence missing.
- Privacy/recording decision missing.
- SIM adapter not smoke-tested.
- Scheduler can violate attempt count.
- Technical failure can become no-answer.
- Admin can bypass blocker.
- Order Core revalidation not tested.
- Rollback path not tested.

## 15. Release report template

Release report must include:

- Scope and environment.
- Commit/build identifier.
- Config flags.
- Test outputs.
- Evidence packet links.
- Known owner decisions.
- P0 blockers status.
- Rollback plan.
- Owner approval.

## 16. Final acceptance

Phase 8 can move from documentation/contract to implementation only when:

- `IVR-00` to `IVR-09` expanded and reviewed.
- `IVR-10` to `IVR-20` reviewed for SDS/backlog.
- Contract pass remains valid.
- Implementation backlog starts at database/API/fake adapter, not production SIM.
