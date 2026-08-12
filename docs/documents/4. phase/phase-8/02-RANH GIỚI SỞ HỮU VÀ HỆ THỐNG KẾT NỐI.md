# IVR-02 - Ownership Boundary / Connected Systems

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa hệ thống IVR kết nối với hệ thống nào, bằng gì và theo ranh giới ownership nào.

## 1. Mục tiêu

Tài liệu này mô tả kiến trúc kết nối cấp SRS cho IVR. Đây chưa phải OpenAPI/AsyncAPI/schema thật, nhưng khóa semantic interface để phase sau sinh contract.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Owner/resolver/consumer boundary. |
| `docs/documents/1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md` | Cross-pack dependency. |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Boundary Order Core / IVR / Operational Core. |
| `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Commerce owner của Official Order và payment boundary. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Module boundary IVR. |
| `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` | Connected systems baseline. |

## 3. Hệ thống kết nối

| Hệ thống | Kết nối bằng gì | Kết nối như thế nào | Owner | IVR được làm | IVR không được làm |
| --- | --- | --- | --- | --- | --- |
| Commerce Order Core / Order State Machine | Internal command/callback contract | Order Core tạo `IvrConfirmationTaskV1`; IVR callback `IvrConfirmationResultCallbackV1`; Order Core revalidate | Commerce/Order Owner | Nhận task, gửi result signal | Tự transition order |
| Operational Core | Internal read/check contract | IVR/Order Core kiểm Sale Lock, Recall, Suppression, availability, traceability blocker | Operational Owner | Consume blocker status | Bỏ qua hoặc override blocker |
| Customer Trust / Customer Memory | Internal resolver | Lấy trust decision, risk reason, trusted skip reason | Customer/CRM Owner | Consume trusted/untrusted decision | Hardcode trusted customer |
| Official Contact / Customer Profile | Privacy-safe projection | Nhận `official_contact_id`, `phone_ref`, `phone_masked`, token dial nếu được phép | Customer/Commerce Owner | Gọi contact đã duyệt | Đọc full profile/full address |
| SIM Gateway Adapter | Internal hardware/API adapter | Scheduler reserve SIM, adapter dial, capture call status/DTMF | IVR Infrastructure Owner | Dial, capture, health check | Gửi SMS hoặc update order |
| Evidence Registry / Audit | Internal writer | Ghi evidence/audit refs cho task, attempt, result, admin action | Foundation/Evidence Owner | Ghi evidence | Tự mark accepted/pass |
| Admin Web/Ops Console | Internal admin API | Monitor queue, review exception, pause/resume, disable SIM theo RBAC | Ops/Admin Owner | Điều hành có audit | Override P0 blocker |
| Notification Owner | Internal handoff sau Core decision | Order Core gửi yêu cầu notification sau khi hủy/expire/hold | Notification Owner | Không gửi trực tiếp | Tự gửi notice từ IVR/SIM |
| AI Advisor/Facebook Gateway/Live/CRM | No direct command | Chỉ consume trạng thái đã được Core công nhận nếu policy cho phép | Channel/AI/CRM Owners | Không trigger IVR | Tạo task/gọi IVR/public result |
| Payment/MISA/Finance | Không kết nối trực tiếp từ IVR | Order Core/Finance xử lý riêng | Finance/MISA Owner | Không xử lý | Xác nhận payment/MISA/revenue |

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

| ID | Yêu cầu |
| --- | --- |
| IVR02-FR-001 | IVR chỉ nhận task từ Order Core hoặc service được Order Core ủy quyền rõ ràng. |
| IVR02-FR-002 | Mọi kết nối phải có `correlation_id`, `idempotency_key` hoặc trace tương đương. |
| IVR02-FR-003 | IVR phải re-check blocker trước attempt và Order Core phải re-check blocker khi nhận callback. |
| IVR02-FR-004 | SIM Gateway Adapter không được có credential/quyền ghi order. |
| IVR02-FR-005 | Admin action phải đi qua RBAC server-side và audit. |
| IVR02-FR-006 | Không hệ thống downstream nào được dùng raw IVR result để tuyên bố order đã paid/revenue/delivered. |

## 6. Tình huống lỗi

| Failure | Hành vi bắt buộc |
| --- | --- |
| Order Core unavailable | Không tạo hoặc không tiếp tục task mới; route fail-safe/admin review. |
| Operational blocker check unavailable | Không dispatch attempt. |
| Customer Trust unavailable | Không gọi trusted/customer nếu không có policy fallback. |
| Evidence Registry unavailable | Không dùng result để transition. |
| SIM Gateway unavailable | Technical exception, không tính customer attempt. |
| Callback failed | Retry kỹ thuật bounded, không gửi duplicate transition. |

## 7. Tiêu chí chấp nhận

- SRS chỉ rõ từng hệ thống kết nối bằng gì và quyền gì.
- Không có hệ thống nào ngoài Order Core được quyết định trạng thái đơn.
- Không có kết nối IVR -> Payment/MISA/Revenue.
- Có fail-safe khi source owner không khả dụng.

## 8. Detailed integration responsibility matrix

| Integration | Producer | Consumer | Transport triển khai khuyến nghị | Sync/async | Data classification |
| --- | --- | --- | --- | --- | --- |
| Order task | Order Core | IVR Internal API | Internal REST/command API | Sync command | Internal order data + phone projection. |
| Eligibility check | IVR/Order Core | Trust/contact/blocker resolvers | Internal API/read model | Sync trước dispatch | Sensitive decision data. |
| Attempt dispatch | Scheduler | SIM Adapter | Internal worker command | Async worker | Sensitive phone token/call metadata. |
| Result callback | IVR Runtime | Order Core | Internal callback API | Sync with retry | Order signal. |
| Event publication | IVR/Business Platform | Consumers | AsyncAPI future approved toolchain | Async | Signal only, not order final. |
| Admin action | Admin Web | IVR API | Admin API | Sync command | Audit/security sensitive. |
| Evidence write | IVR services | Evidence Registry | Internal writer/API | Sync before final callback where required | Audit/evidence data. |

## 9. Ownership by lifecycle stage

| Stage | Owner | IVR role | Required handoff |
| --- | --- | --- | --- |
| Official Order created | Order Core | None until task. | Task only if IVR required. |
| Task emitted | Order Core | Consumer. | `IvrConfirmationTaskV1`. |
| Eligibility resolved | Order Core + source resolvers | Validate/consume. | Eligibility decision. |
| Queue scheduled | IVR Runtime | Owner. | CallJob/Attempt persisted. |
| SIM call active | IVR Infrastructure | Owner. | Raw call status/DTMF. |
| Result normalized | IVR Runtime | Owner. | `IvrCallResult`. |
| Callback received | Order Core | Decision owner. | Ack/stale/block/review. |
| Notification | Notification Owner | No direct role. | Only after Core decision. |

## 10. Connected system failure contracts

| System unavailable | Before attempt | During attempt | During callback |
| --- | --- | --- | --- |
| Order Core | Do not create new task. | Continue active call only if already dispatched safely; callback retries bounded. | Retry bounded/admin review. |
| Operational Core | Do not dispatch. | If active call already returned result, Core revalidates before action. | Core blocks if cannot revalidate. |
| Trust/contact resolver | Hold task/review. | Do not switch contact mid-call. | Core decides with current sources. |
| Evidence Registry | Do not final-callback if required evidence missing. | Technical exception if evidence write fails. | Hold/admin review. |
| SIM Gateway | Technical exception. | Technical exception; not no-answer. | Not applicable. |
| Admin Web | No operational impact. | No operational impact. | No operational impact. |

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

| Test | Expected |
| --- | --- |
| SIM Adapter tries to update order | Forbidden/no credential. |
| Facebook Gateway tries to create IVR task | Forbidden. |
| Admin tries to force order cancel from IVR | Forbidden. |
| IVR callback without evidence | Hold/reject/review. |
| Operational Core unavailable before dispatch | No SIM call. |
| Payment/MISA integration requested from IVR | Not implemented/blocked by scope. |

## 13. Implementation acceptance checklist

- All service identities are allowlisted.
- All internal APIs require auth.
- SIM Adapter has no Order Core write permission.
- Admin actions are RBAC protected server-side.
- Evidence/audit integration is present.
- Downstream systems consume only Core-approved state where required.
