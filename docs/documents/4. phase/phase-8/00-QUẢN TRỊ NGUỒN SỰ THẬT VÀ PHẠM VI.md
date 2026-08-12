# IVR-00 - Governance / Source of Truth / Scope Boundary

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa governance, nguồn sự thật và ranh giới phạm vi trước khi viết contract hoặc triển khai.

## 1. Mục tiêu

Tài liệu này xác định IVR là một hợp phần xác nhận đơn hàng bằng cuộc gọi tự động, không phải hệ thống bán hàng, không phải hệ thống chăm sóc khách hàng đại trà và không phải lớp quyết định trạng thái đơn hàng.

IVR chỉ được vận hành sau khi có Official Order đủ điều kiện từ Commerce Order Core. Kết quả IVR chỉ là tín hiệu đầu vào để Order Core revalidate và quyết định transition.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/source-map.md` | Đường dẫn nguồn hợp lệ cho traceability. |
| `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Source-of-truth, owner, resolver, consumer boundary. |
| `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md` | Correlation, idempotency, evidence trace. |
| `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Evidence, smoke, completion gate. |
| `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` | Release/go-live gate. |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Pack source chính của IVR. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Technical source chính của IVR. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | Baseline gap-closure tiền SRS. |

## 3. Thuật ngữ

| Thuật ngữ | Định nghĩa |
| --- | --- |
| IVR | Hợp phần gọi tự động xác nhận đơn hàng bằng SIM Gateway nội bộ. |
| Order Core | Commerce Order Core / Order State Machine, chủ sở hữu trạng thái đơn hàng. |
| Official Order | Đơn chính thức đã được Commerce tạo từ Customer Confirmation hợp lệ. |
| IVR task | Yêu cầu nội bộ do Order Core tạo để IVR xem xét/gọi xác nhận. |
| IVR result | Kết quả đã normalize từ cuộc gọi IVR. |
| Evidence | Bằng chứng có trace, owner, trạng thái và audit. |
| Release gate | Cổng kiểm soát trước khi cho phép gọi khách thật. |

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

| Nhóm sự thật | Owner | IVR được làm | IVR bị cấm |
| --- | --- | --- | --- |
| Trạng thái đơn | Order Core | Đọc task và gửi result signal | Tự transition đơn |
| Official Order | Commerce Runtime | Chỉ xử lý order đã đủ điều kiện | Gọi Quote/Cart/Order Draft |
| Sale Lock/Recall/Suppression | Operational Core | Consume trạng thái block | Override hoặc bỏ qua block |
| Customer trust | Customer Trust/Customer Memory Resolver | Consume quyết định trusted/untrusted | Hardcode trusted customer |
| Official contact | Customer/Commerce Profile | Gọi contact được duyệt | Đọc full profile hoặc full address |
| Payment/MISA/Revenue | Payment/MISA/Finance/Commerce | Không xử lý trực tiếp | Xác nhận thanh toán/kế toán/doanh thu |
| Evidence/Audit | Foundation/Evidence Registry | Ghi evidence/audit theo policy | Tự gọi PASS khi evidence chưa accepted |

## 6. Baseline governance

| Rule | Giá trị |
| --- | --- |
| `CALL_PURPOSE` | `ORDER_CONFIRMATION_ONLY` |
| `IVR_DEPLOYMENT_MODEL` | `INTERNAL_SIM_GATEWAY_SERVER` |
| `MAX_ATTEMPT_PER_ORDER` | `PROGRAM_BASED` |
| Golden Hour | 2 cuộc trong 10 phút |
| 24/7 | 3 cuộc trong 15 phút |
| `IVR_RESULT_IS_INPUT_SIGNAL_ONLY` | `YES` |
| `ORDER_STATE_MACHINE_IS_FINAL_DECISION_LAYER` | `YES` |
| `REAL_CUSTOMER_CALL_ALLOWED` | `NO` cho đến khi release gate pass |

## 7. Yêu cầu chức năng

| ID | Yêu cầu |
| --- | --- |
| IVR00-FR-001 | Hệ thống phải chặn mọi task không đến từ Order Core hoặc không có correlation/idempotency hợp lệ. |
| IVR00-FR-002 | Hệ thống phải từ chối Quote, Cart, Order Draft và mọi entity không phải Official Order. |
| IVR00-FR-003 | Hệ thống phải coi mọi kết quả IVR là signal, không phải quyết định trạng thái đơn. |
| IVR00-FR-004 | Hệ thống phải dừng hoặc hold IVR khi Sale Lock, Recall, Suppression, opt-out hoặc policy block xuất hiện. |
| IVR00-FR-005 | Hệ thống phải ghi evidence/audit cho task, attempt, result, callback, exception và admin action. |
| IVR00-FR-006 | Hệ thống phải giữ production disabled cho đến khi IVR-09 release gate pass. |

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

| Nhóm phạm vi | Trong Phase 8 | Ngoài Phase 8 | Ghi chú triển khai |
| --- | --- | --- | --- |
| Order confirmation | Gọi xác nhận Official Order bằng IVR. | Tư vấn sản phẩm, upsell, CRM, chăm sóc sau bán. | Call script chỉ phục vụ xác nhận đơn. |
| Order ownership | Gửi tín hiệu cho Order Core. | IVR tự confirm/cancel/expire order. | Order Core là lớp quyết định cuối. |
| Attempt policy | Golden Hour 2 cuộc/10 phút; 24/7 3 cuộc/15 phút. | Tự thêm attempt, tự kéo dài window. | Policy phải nằm trong resolver/config được duyệt. |
| Contact | Dùng official contact projection. | Đọc full profile/full address/CRM note. | Ưu tiên `phone_ref`, `phone_masked`, dial token. |
| Evidence | Ghi audit/evidence cho task/attempt/result/admin. | Tự mark release PASS. | Evidence Registry/Foundation quyết định accepted. |
| Admin | Monitor, pause/resume, disable/enable SIM, manual technical retry. | Admin force order state. | Admin action không bypass blocker. |
| SIM Gateway | Internal SIM Gateway Server. | Cloud IVR/SIP/brandname làm mặc định. | Provider khác là future owner decision. |

## 12. Source-of-truth decision table

| Quyết định | Source-of-truth | IVR consume như thế nào | Khi source không khả dụng |
| --- | --- | --- | --- |
| Entity có phải Official Order không | Commerce Order Core | Task payload + revalidation callback. | Không tạo/gọi task. |
| Order còn nhận IVR result không | Order Core | Callback ack/reject. | Retry bounded hoặc admin review. |
| Khách có trusted skip không | Customer Trust Resolver | Snapshot trong task/eligibility. | Không tự hardcode; route review hoặc require IVR theo policy an toàn nếu owner duyệt. |
| Phone có hợp lệ không | Official Contact/Phone Resolver | `phone_validation_status`. | Không dispatch nếu không có fallback approved. |
| Sale Lock/Recall/Suppression | Operational Core | Pre-dispatch check và Order Core callback revalidation. | Không dispatch. |
| Payment issue | Payment/Order Core | Order Core revalidate khi callback. | Không confirm order. |
| Evidence accepted | Evidence Registry | Evidence refs. | Không mở release/pass. |
| Production ready | Release Owner | Release gate evidence. | `REAL_CUSTOMER_CALL_ALLOWED = NO`. |

## 13. Governance state gates

| Gate | Trạng thái mặc định | Điều kiện mở |
| --- | --- | --- |
| `IVR_DOCS_APPROVED` | `NO` | SRS/SDS được owner review. |
| `IVR_CONTRACT_APPROVED` | `NO` | Contract validator pass và owner accepted. |
| `IVR_TASK_INTAKE_ENABLED` | `NO` | API/auth/idempotency/evidence test pass. |
| `IVR_SCHEDULER_ENABLED` | `NO` | Scheduler/concurrency/capacity test pass. |
| `IVR_SIM_INTERNAL_TEST_ENABLED` | `NO` | Internal-number smoke approved. |
| `REAL_CUSTOMER_CALL_ALLOWED` | `NO` | IVR-09 release gate pass, privacy/security approved. |
| `DOWNSTREAM_IVR_DEPENDENCY_ALLOWED` | `NO` | Order Core/Release owner approves downstream consumption. |

## 14. P0 governance rules

| P0 | MUST | MUST NOT | FAIL IF |
| --- | --- | --- | --- |
| IVR00-P0-001 | IVR chỉ xử lý Official Order. | Không gọi Quote/Cart/Order Draft. | Có task IVR từ entity chưa phải Official Order. |
| IVR00-P0-002 | IVR result là input signal. | Không tự transition order. | IVR/SIM có quyền update order state. |
| IVR00-P0-003 | Order Core revalidate callback. | Không accept callback mù. | Key `1` làm order confirmed khi Sale Lock/Recall active. |
| IVR00-P0-004 | Technical failure tách khỏi no-answer. | Không count lỗi kỹ thuật như khách không nghe. | SIM/server/DTMF lỗi tạo no-answer final. |
| IVR00-P0-005 | Evidence/audit bắt buộc. | Không release bằng xác nhận miệng. | PASS thiếu evidence packet. |
| IVR00-P0-006 | Privacy safe by default. | Không log raw phone/full profile. | Raw phone xuất hiện trong app log/admin UI không được duyệt. |

## 15. Traceability requirement

Mỗi implementation task sau này phải có tối thiểu:

- Source document path trong `docs/source-map.md`.
- Requirement ID từ `IVR-00` đến `IVR-09` hoặc SDS `IVR-10` đến `IVR-20`.
- Contract path nếu có: schema/API/event/state-machine.
- Test case hoặc evidence item.
- Owner decision nếu rule chưa được khóa.

## 16. Implementation readiness checklist

| Checklist | Required before code |
| --- | --- |
| Source path hợp lệ | Có. |
| Requirement rõ | Có. |
| Owner/source-of-truth rõ | Có. |
| Boundary với Order Core rõ | Có. |
| Privacy impact rõ | Có. |
| Evidence/audit rõ | Có. |
| Fail-safe rõ | Có. |
| Test expected rõ | Có. |

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
