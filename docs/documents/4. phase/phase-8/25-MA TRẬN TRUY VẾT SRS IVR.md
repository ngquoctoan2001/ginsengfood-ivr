# IVR-SRS Trace Matrix - Phase 8

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_TRACE_BASELINE`  
Phạm vi: Phase 8 - IVR Order Confirmation  
Ngôn ngữ tài liệu: Tiếng Việt có dấu, giữ nguyên mã kỹ thuật/enum/API field bằng ASCII.

## 1. Mục tiêu

Trace matrix này dùng để nối nguồn tài liệu nền với bộ SRS `IVR-00` đến `IVR-09`. Mỗi yêu cầu SRS của Phase 8 phải truy ngược được về ít nhất một nguồn nền hoặc được đánh dấu rõ là `Owner Decision Required`.

## 2. Nguồn nền bắt buộc

| Nhóm nguồn | Tài liệu | Vai trò trong Phase 8 |
| --- | --- | --- |
| Source map | `docs/source-map.md` | Danh sách đường dẫn nguồn hợp lệ, dùng cho traceability. |
| Appendices | `docs/documents/0. appendices/01-OPERATIONAL-FORMS.md` | Quy tắc form/evidence/audit khi có operational handoff hoặc admin review. |
| Appendices | `docs/documents/0. appendices/02-AUTO-GENERATED-FORM-RULES.md` | Nguyên tắc không nhập lại dữ liệu đã có source object, áp dụng cho IVR task/callback evidence. |
| Appendices | `docs/documents/0. appendices/03-PRINTING-CODE-RULES.md` | Ranh giới không liên quan trực tiếp; chỉ dùng làm ví dụ về audit/reprint boundary nếu future integration. |
| Appendices | `docs/documents/0. appendices/04-MISA-MAPPING-RULES.md` | IVR không ghi nhận kế toán, không xác nhận MISA, không tạo doanh thu xác thực. |
| Appendices | `docs/documents/0. appendices/05-MATERIAL-PACKAGING-TAXONOMY.md` | Operational blocker có thể đến từ material/packaging/sellable policy nhưng IVR chỉ consume trạng thái block. |
| Master | `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Source-of-truth, owner, resolver, consumer boundary, no-hardcode rule. |
| Master | `docs/documents/1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md` | Ranh giới phụ thuộc giữa Commerce, Operational Core, Gateway, IVR, Evidence. |
| Master | `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md` | Correlation, idempotency, audit/evidence trace. |
| Master | `docs/documents/1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md` | Runtime guard, suppression, fallback, owner review. |
| Master | `docs/documents/1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Evidence status, smoke, completion gate, owner sign-off. |
| Master | `docs/documents/1. master/08-MASTER-07-RELEASE-GO-LIVE-CONTROL.md` | Release/go-live, stop gate, rollback, production readiness. |
| Pack | `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Nguồn pack chính cho IVR, SIM Gateway, attempt policy, call script, boundary. |
| Pack | `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` | Release evidence, completion gateway, production readiness. |
| Tech | `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md` | RBAC, permission, audit, evidence registry, idempotency. |
| Tech | `docs/documents/3. tech/04-TECH-03-OPERATIONAL-CORE-PRODUCTION-WAREHOUSE-INVENTORY-TRACEABILITY-RECALL-SALE-LOCK.md` | Sale Lock, Recall, suppression, traceability blocker. |
| Tech | `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Quote/Cart/Order Draft/Official Order, payment, shipping, verified revenue boundary. |
| Tech | `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Nguồn kỹ thuật chính cho IVR eligibility, lifecycle, outcome, privacy, smoke. |
| Tech | `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-EVIDENCE-RELEASE-GATEWAY-PRODUCTION-READINESS-CONTROL.md` | Global smoke, UAT, evidence, release gateway. |
| Phase 8 | `docs/documents/4. phase/phase-8/ivr-input-baseline.md` | Baseline đầu vào: SIM Gateway, call script, attempt policy, capacity. |
| Phase 8 | `docs/documents/4. phase/phase-8/ivr-order-confirmation.md` | Index/handoff Phase 8 và pack view sạch. |
| Phase 8 | `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | Baseline gap-closure để viết SRS. |

## 3. Trace theo tài liệu SRS

| SRS | Chủ đề | Nguồn chính | Contract/state-machine tương lai |
| --- | --- | --- | --- |
| `IVR-00` | Governance, source-of-truth, scope boundary | MASTER-01/02/03/04/05/07, PACK-09, TECH-09 | Compatibility notes, source_documents policy |
| `IVR-01` | Business purpose, confirmation use case | PACK-09, TECH-09, TECH-04 | IVR purpose enum, result reason enum |
| `IVR-02` | Ownership boundary, connected systems | MASTER-02/03, TECH-04, TECH-09, TECH-10 | System boundary map, API responsibility matrix |
| `IVR-03` | Eligibility, customer trust, official contact | TECH-09, TECH-04, Phase 4 Customer Memory, MASTER-04 | Eligibility decision enum, phone validation enum |
| `IVR-04` | Order Core -> IVR task contract | PACK-09, TECH-09, MASTER-03/04, TECH-01 | `IvrConfirmationTaskV1` schema |
| `IVR-05` | Attempt policy, scheduler, queue | PACK-09, TECH-09, `ivr-input-baseline.md` | CallJob/Attempt state machines, scheduler config enum |
| `IVR-06` | Internal SIM Gateway adapter | PACK-09, TECH-09, `ivr-input-baseline.md`, TECH-01 | SIM channel schema, adapter error enum |
| `IVR-07` | Result normalization, callback to Order Core | TECH-09, TECH-04, MASTER-03/04 | `IvrConfirmationResultCallbackV1`, Result state machine |
| `IVR-08` | Admin monitoring, evidence, audit, privacy | TECH-01, MASTER-05, TECH-09, PACK-10 | Permission matrix, audit/evidence schemas |
| `IVR-09` | Test matrix, smoke, release gate | MASTER-05/07, TECH-10, PACK-10, TECH-09 | Contract tests, fixture map, release checklist |

## 4. Trace theo baseline rule

| Rule | SRS nơi khóa | Nguồn chính | Ghi chú |
| --- | --- | --- | --- |
| Golden Hour có 2 cuộc trong 10 phút | `IVR-05`, `IVR-09` | PACK-09, TECH-09, Phase 8 baseline | Không có attempt thứ 3. |
| 24/7 có 3 cuộc trong 15 phút | `IVR-05`, `IVR-09` | PACK-09, TECH-09, Phase 8 baseline | Không có attempt thứ 4. |
| IVR không gọi Quote/Cart/Order Draft | `IVR-00`, `IVR-03`, `IVR-09` | TECH-04, TECH-09, Phase 3 | Chỉ Official Order đủ điều kiện. |
| IVR không tự update order | `IVR-00`, `IVR-02`, `IVR-07` | PACK-09, TECH-09 | Order Core là lớp quyết định cuối. |
| Sale Lock/Recall/Suppression thắng IVR | `IVR-02`, `IVR-03`, `IVR-07`, `IVR-09` | TECH-03, TECH-04, TECH-09 | Revalidate trước dispatch và khi callback. |
| Invalid phone tách khỏi no-answer | `IVR-03`, `IVR-07`, `IVR-09` | TECH-09 | Không tính lỗi kỹ thuật là lỗi khách. |
| Technical retry không tính customer attempt | `IVR-05`, `IVR-06`, `IVR-07` | TECH-09, Phase 8 baseline | Không được kéo dài window thương mại. |
| Real customer call bị chặn đến release gate | `IVR-00`, `IVR-09` | PACK-09, TECH-10, MASTER-07 | `REAL_CUSTOMER_CALL_ALLOWED = NO` cho đến khi gate pass. |

## 5. Owner Decision Required

Các điểm sau chưa được nguồn nền quyết định đủ chi tiết. SRS phải giữ trạng thái `Owner Decision Required`, không tự bịa rule:

- Ngưỡng cụ thể để khách được xem là trusted checkout.
- Danh sách risk flag cụ thể buộc khách trusted vẫn phải đi IVR.
- Tiêu chí permanent invalid phone.
- Số lần technical retry và backoff.
- Mapping chi tiết tín hiệu SIM Gateway cho `BUSY`, `REJECTED`, `UNREACHABLE`, `CALL_DROPPED`.
- Có cho phép recording hay không.
- Retention duration cho call log, DTMF evidence, SIM log, admin audit.
- Protocol/API phần cứng SIM Gateway production.
- Phạm vi pilot khách thật nếu được duyệt.
- Template notification sau khi Order Core hủy đơn.

## 6. Acceptance của trace matrix

Trace matrix đạt khi:

- Mỗi file `IVR-00` đến `IVR-09` trỏ lại nguồn nền trong bảng này.
- Không có SRS rule quan trọng nào không có nguồn hoặc không được đánh dấu `Owner Decision Required`.
- Không dùng đường dẫn legacy cũ; mọi source path phải lấy theo `docs/source-map.md`.
- Không ghi production ready nếu chưa có evidence, smoke, owner sign-off và release decision.
