# TODAY-01 — Decision / Sign-off Pack hiện hành cho Module 8

**Ngày chuẩn bị:** 29/08/2026

**Evidence snapshot:** main @ **0baed74cd384cd661aed068c263a92ef97ead1f4**

**Trạng thái triển khai TODAY-01:** **M8_OWNER_SIGNED / HANDOFF_READY / EXTERNAL_SIGNATURES_REQUIRED**

**Người ký phía Module 8:** **Tôi — Module 8 / Project Owner** · **2026-08-29**

**External dispatch:** **NOT_PERFORMED**

**External approval:** **NOT_RECEIVED**

> Pack này là đầu mối gửi và nhận phản hồi hiện hành. Chữ ký Module 8 được ghi tại §8; nó không thay chữ ký M3, Security, Platform, Legal/Privacy, CRM hoặc Telephony, không thay tracker và không cho phép bắt đầu code phụ thuộc trước khi đúng external owner ký.

## 1. Cảnh báo trước khi gửi

Không gửi nguyên trạng [Target Contract V1 Closure Pack cũ](../../docs/contracts/target-v1-closure-pack/README.md) như một nguồn hiện hành duy nhất:

- README của pack cũ vẫn ghim baseline **main@54ca239 / draft.18**; contract hiện đã lên **draft.22**.
- [T-01 cũ](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) còn nói ma trận program/payment chưa có nguồn business. Điều này đã lỗi thời: Flow 04/05 đã khóa **24_7 + COD** và **GOLDEN_HOUR + ONLINE**.
- [T-07 cũ](../../docs/contracts/target-v1-closure-pack/T-07-production-auth.md) còn mô tả admin identity trước W-0128. Hiện Module 3 sở hữu operator identity/UI; IVR chỉ nhận service credential và actor identity.
- Các ticket cũ vẫn hữu ích cho câu hỏi kỹ thuật, acceptance artifact và mock boundary, nhưng phải được đọc qua correction/routing trong pack này.

Nguồn điều khiển theo thứ tự:

1. [Control order](00-index.md).
2. [Decisions log](decisions-log.md).
3. [IR-06 — Module 3 API handover](../../integration-requirements/06-module-3-api-handover.md).
4. OpenAPI draft hiện hành và evidence W-0123/W-0128/W-0137.
5. Ticket closure cũ chỉ làm thư viện câu hỏi/evidence khi không mâu thuẫn các nguồn trên.

## 2. Phạm vi pack

Pack này route mười nhóm quyết định. Mỗi nhóm có owner, câu trả lời bắt buộc, artifact chấp nhận và stop rule.

| ID | Chủ đề | Owner trả lời | Trạng thái hiện tại |
|---|---|---|---|
| S-01 | M3 là business authority, IVR chỉ thực thi | M3 + M8; Privacy cho retention field | M8_OWNER_SIGNED / M3_PRIVACY_PENDING |
| S-02 | Program/task wire mapping | M3 + Product/Order Core | M8_POSITION_SIGNED / M3_PRODUCT_PENDING |
| S-03 | Admin identity/UI handoff W-0128 | M3 + Security/Platform | M8_HANDOFF_SIGNED / M3_SECURITY_PENDING |
| S-04 | Upstream session trace | M3 + M8 | M8_STOP_RULE_SIGNED / M3_DECISION_PENDING |
| S-05 | Generic callback, ACK, auth, sandbox và CDC | M3 + Security/Platform + M8 | M8_FAIL_CLOSED_SIGNED / BLOCKED_EXTERNAL |
| S-06 | Opt-out feedback loop | CRM/M3.1 + M3 + Legal/Privacy | M8_STOP_RULE_SIGNED / EXTERNAL_DECISION_PENDING |
| S-07 | Revoke/freshness lifecycle | Owner + M3 + M8 | OPTION_A_M8_OWNER_SIGNED / M3_ACK_REQUIRED |
| S-08 | Dial-token production path | M3/Sales + Security + Telephony | M8_STOP_RULE_SIGNED / EXTERNAL_DECISION_PENDING |
| S-09 | Attempt policy production | Product + Order Core | PRODUCTION_POLICY_NOT_APPROVED / PRODUCT_ORDER_CORE_REQUIRED |
| S-10 | Bản DOCX V0.3 lỗi thời | Module 8 Owner | OPTION_A_M8_OWNER_SIGNED / CONTROLLED_EXECUTION_QUEUED |

## 3. Decision sheets

### S-01 — M3 business authority

**Phiếu trả lời chính:** [questions-to-module-3-od18-authority.md](questions-to-module-3-od18-authority.md)

M3 phải trả lời đủ **OD18-C1..C5**, đặc biệt:

- M3 có còn gửi/đọc các field và enum trusted-skip cũ không.
- M3 đã lọc đơn không cần gọi trước khi gửi task hay chưa.
- Đơn không cần gọi có được bảo đảm **không gửi** sang IVR hay không.
- Có cần giữ customer trust metadata cho audit không; nếu có, mục đích và retention là gì.
- Giữ risk flags cho scheduler hay thay bằng một field priority tường minh.

**Artifact chấp nhận:**

- Phiếu đã điền đủ, có commit/OpenAPI/runtime evidence.
- Chữ ký M3 contract owner, M8 owner và Privacy nếu giữ customer trust metadata.

**Stop rule:** W-0123 giữ **TESTS_PASS / BLOCKED_EXTERNAL** cho tới khi đủ chữ ký M3/M8 và Privacy ở phần có retention metadata. Không dựng lại trusted-skip runtime.

### S-02 — Program/task wire mapping

Business pair **không hỏi lại**:

- **24_7 + COD**
- **GOLDEN_HOUR + ONLINE**

M3 phải ký và triển khai đúng wire mapping:

| Nguồn M3 | Giá trị IVR |
|---|---|
| 24_7 | TWENTY_FOUR_SEVEN |
| PHONE_VALID | VALID |
| ELIGIBLE_FOR_IVR | ELIGIBLE |

M3/Product còn phải trả lời:

- Producer set ivr_confirmation_required ở bước nào, điều kiện nào làm nó thành true và có bao giờ gửi false không.
- Producer xử lý từng decision rejection/blocking như thế nào; không chỉ rẽ nhánh theo HTTP status.
- order_version bump khi nào.
- Minimal eligibility snapshot nào được ký làm evidence.

**Artifact chấp nhận:**

- Bảng wire mapping đã ký.
- OpenAPI/assembler commit phía M3.
- Producer CDC chứng minh ba phép mapping và hai business pair.

**Stop rule:** không nới matrix IVR và không yêu cầu Product quyết lại business pair đã có nguồn.

### S-03 — Admin identity/UI handoff W-0128

Nguồn bằng chứng: [W-0128](../../docs/evidence/W-0128/README.md) và IR-06 §4A.

M3/Security phải trả lời:

1. Vai trò/claim M3 nào map sang tầng **read**, **write**, **danger**.
2. Vai trò lạ có deny-by-default không; ai được cấp **danger**.
3. Secret-store path, rotation owner/schedule và previous-token retirement cho từng tier.
4. X-Actor-Id lấy từ authenticated subject nào và định dạng opaque ID ra sao.
5. UI/BFF bảo đảm token không tới browser bằng cách nào.
6. M3 đã regenerate client từ OpenAPI **draft.22** chưa.
7. Positive/negative shared E2E cho từng tier chạy ở môi trường nào.
8. M3 có nhận ownership dựng UI quản trị không. Nếu không, phải chỉ định owner khác bằng văn bản; không được để M3 và M8 cùng build một UI.

**Artifact chấp nhận:**

- Role/claim → tier matrix đã ký.
- Secret/rotation/NetworkPolicy design.
- Commit client đã regenerate.
- Shared E2E report từng tier.

**Stop rule:** W-0128 vẫn là **TESTS_PASS_LOCAL / M3_AND_PRODUCTION_BLOCKED** cho tới khi đủ artifact.

### S-04 — Upstream session trace

Không có field upstream session nào trong task contract hiện hành. Capacity incident đang dùng session ID nội bộ/synthetic nên chưa đối soát được với phiên Golden Hour của M3.

M3 và M8 phải ký đủ:

1. Tên field chính xác; không dùng đồng thời session_id và golden_hour_session_id.
2. Kiểu, format, max length và quy tắc validation.
3. Required hay optional cho Golden Hour và 24/7.
4. Ai phát, thời điểm phát, uniqueness và stability qua retry/replay.
5. Quan hệ với task_id, correlation ID và capacity incident.
6. Retention/redaction và quyền đọc.
7. Hành vi với producer cũ chưa có field.

**Artifact chấp nhận:**

- Decision record có chữ ký M3 + M8.
- OpenAPI diff và migration plan additive.
- Producer/consumer CDC.

**Stop rule:** không thêm field, cột hoặc migration trước chữ ký; không chọn tên thay owner.

### S-05 — Generic callback, auth, sandbox và CDC

Nguồn câu hỏi:

- [T-05 — Generic callback/ACK](../../docs/contracts/target-v1-closure-pack/T-05-callback-ack.md)
- [T-07 — Production service auth](../../docs/contracts/target-v1-closure-pack/T-07-production-auth.md), chỉ dùng phần service-to-service sau correction W-0128
- [T-08 — OpenAPI/CDC ownership](../../docs/contracts/target-v1-closure-pack/T-08-openapi-compat-cdc.md)

M3/Security/Platform phải cung cấp:

- Generic consumer phủ Golden Hour + 24/7.
- ACK taxonomy, idempotency boundary, key retention và revalidation rules.
- Auth profile: issuer, JWKS, audience, scope, TTL, rotation và mTLS decision.
- Reachable sandbox, credential retrieval và network path.
- OpenAPI publication/versioning/deprecation policy.
- CDC ownership: ai viết, chạy ở pipeline nào, đỏ thì ai chặn merge/sửa.

**Artifact chấp nhận:**

- OpenAPI phía M3 + consumer commit.
- Security auth profile + sandbox credential reference.
- Shared E2E: callback thật nhận ACCEPTED, duplicate, stale, blocked, auth failure và retryable failure.

**Stop rule:** không bật Target V1 delivery và không gỡ fail-closed validator trước shared E2E.

### S-06 — Opt-out feedback loop

Current taxonomy không có IVR_OPT_OUT. Rejected chỉ là NO_ANSWER + review signal; nó **không tự động là opt-out**.

CRM/M3.1, M3 và Legal/Privacy phải ký:

1. Sự kiện explicit nào được tính là yêu cầu opt-out.
2. Ai ghi vào consent/suppression registry.
3. Key/scope: customer, contact hash, channel, category và policy version.
4. effective_at, expiry/reversal, retention và audit.
5. Read contract để Order Core kiểm trước khi gửi task.
6. Feedback/ACK từ CRM về M3/IVR.
7. Hành vi fail-closed khi registry không trả lời.

**Artifact chấp nhận:**

- Consent/suppression contract đã ký.
- Legal basis/retention approval.
- CRM writer/read API test và M3 producer test.

**Stop rule:** không map Rejected thành opt-out, không tự thêm IVR_OPT_OUT và không persist threshold tự suy đoán trong IVR.

### S-07 — Revoke/freshness lifecycle

Hai hướng đã được trình Owner:

- **A — Chấp nhận stale-call trade-off:** IVR có thể tiếp tục attempt trong window; M3 revalidate khi callback và có quyền trả BLOCKED_BY_CORE.
- **B — Yêu cầu revoke/update:** M3 phát lệnh khi order bị hủy, recall, sale-lock hoặc không còn call-required.

**Lựa chọn của Module 8 Owner ngày 29/08/2026:** **A**, tái xác nhận trade-off đã khóa trong OD-17. M3 vẫn phải ký rằng D-06 revalidation là lớp an toàn bắt buộc; lựa chọn này không cho phép M3 bỏ revalidation.

Nếu chọn B, M3 + M8 phải định nghĩa trước khi code:

- Command path và body.
- Task identity/order version/correlation.
- ACK taxonomy.
- Idempotency và replay.
- State transition hợp lệ.
- Race/fencing với queued, dialing và active call.
- Có dừng active call không, và ai có quyền quyết định.
- Callback/audit/evidence sau revoke.
- Backward compatibility và rollout order.

**Artifact chấp nhận:**

- Owner trade-off decision.
- Nếu chọn B: OpenAPI + state table + race matrix + CDC.

**Stop rule:** không tự đặt route hoặc result code; không dùng admin call termination thay cho business revoke.

### S-08 — Dial-token production path

**Phiếu kỹ thuật:** [T-04 — Dial-token](../../docs/contracts/target-v1-closure-pack/T-04-dial-token.md)

M3/Sales, Security và Telephony phải ký:

- Một token dùng lại, token per-attempt, token bundle hay endpoint reissue.
- Vault/resolver nằm ở đâu và ai vận hành.
- TTL/replay/rotation/audit semantics.
- Gateway nhận opaque token hay bắt buộc E.164.
- Trust boundary bảo đảm IVR DB/log không giữ raw phone.

**Artifact chấp nhận:**

- Contract issue/resolve/reissue.
- Threat model + vendor capability statement.
- TTL/replay/audit tests.

**Stop rule:** không viết production resolver trước khi chọn model/trust boundary.

### S-09 — Attempt policy production

**Phiếu kỹ thuật:** [T-09 — Attempt policy](../../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md)

Owner phải ký:

- Production policy version.
- Window, số attempt và offsets cho từng program.
- Nguồn chân lý: M3 phát tham số hay chỉ phát version.
- Quy tắc fail-closed khi version và tham số mâu thuẫn.
- Cách xử lý xung đột D-10 với business documents.

**Artifact chấp nhận:**

- Policy table có owner/date/version.
- Producer CDC.
- Registry/test update sau quyết định.

**Stop rule:** mock-lab-v1 chỉ dùng MOCK/LAB, không được promote thành production policy.

**Vị trí đã ký của Module 8 Owner:** chưa phê duyệt bất kỳ production policy number nào. Giữ fail-closed cho tới khi Product/Order Core ký bảng policy và giải quyết xung đột nguồn.

### S-10 — Bản DOCX V0.3

Nguồn bằng chứng: [W-0137](../../docs/evidence/W-0137/README.md), [OD-20](decisions-log.md) và bản [V0.3 Markdown hiện hành](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md).

Ba phương án đã được trình Owner:

- **A — Thu hồi:** đổi tên _SUPERSEDED hoặc xóa theo quy trình được duyệt; Markdown là bản duy nhất.
- **B — Tái sinh:** sinh DOCX từ Markdown và chỉ định owner/gate chống lệch.
- **C — Giữ nguyên và chấp nhận rủi ro:** không khuyến nghị.

**Lựa chọn của Module 8 Owner ngày 29/08/2026:** **A — Thu hồi**. Thực thi đổi tên/xóa và đồng bộ decisions-log/gate mirror được xếp vào controlled execution sau khi W-0139 ngừng sửa tracker; task này chưa di chuyển hoặc xóa file DOCX.

**Artifact chấp nhận:**

- Lựa chọn A/B/C, tên người duyệt và ngày.
- Nếu B: owner sinh lại, công cụ/quy trình và gate parity.

**Stop rule:** không tự xóa/sinh lại DOCX trước quyết định Owner.

## 4. Mẫu phản hồi bắt buộc

Mỗi owner trả lại đúng cấu trúc:

| Field | Giá trị |
|---|---|
| Decision ID | S-xx / OD-xx / T-xx |
| Chọn phương án | Giá trị dứt khoát, không ghi “tùy dev” |
| Contract delta | Field/path/enum/semantics thay đổi hoặc NONE |
| Artifact | Commit/OpenAPI/test/evidence link |
| Owner | Tên + vai trò |
| Approval date | YYYY-MM-DD |
| Effective environment | LAB / STAGING / PRODUCTION |
| Rollout/rollback | Thứ tự triển khai và đường lùi |
| Residual blocker | NONE hoặc mã blocker còn lại |

Các phản hồi sau **không hợp lệ**:

- “Đồng ý”, “OK”, “dev tự chọn” nhưng không có artifact.
- Chỉ có proposal của developer, không có owner ký.
- Local/mock test được dùng thay cho shared E2E hoặc production approval.
- Một quyết định mới tự thêm field/enum/route mà không có OpenAPI/CDC.
- Im lặng hoặc không phản hồi được hiểu là approval.

## 5. Routing và thứ tự nhận phản hồi

1. Gửi **S-01, S-02, S-03, S-04, S-05, S-07** cho M3 contract/business owner.
2. Gửi **S-03, S-05, S-08** cho Security/Platform; S-08 thêm Telephony.
3. Gửi **S-06** cho CRM/M3.1 + Legal/Privacy + M3.
4. Gửi **S-09** cho Product/Order Core để họ ký production policy; Module 8 Owner đã ký giữ fail-closed.
5. **S-10** đã được Module 8 Owner chọn phương án A; chờ controlled ledger/file execution sau W-0139.
6. Module 8 Owner đã ký boundary ở §8; không ký thay external owner.

Không đặt ngày cam kết thay owner. Mỗi owner phải tự điền due date trong phản hồi.

## 6. Mẫu tin nhắn handoff

**Tiêu đề:** Module 8 — Technical Review & Decision Sign-off Required

> Module 8 đã đối chiếu worklist với code và contract hiện hành. Các phần local đã hoàn tất không được giao làm lại. Các mục trong pack này cần đúng owner quyết định hoặc cung cấp artifact trước khi IVR được phép triển khai tiếp.
>
> Vui lòng trả lời theo mẫu ở §4, kèm commit/OpenAPI/test/evidence. “OK” bằng văn xuôi không được tính là sign-off. Mọi mục chưa có chữ ký tiếp tục giữ OWNER_SIGNOFF_REQUIRED hoặc BLOCKED_EXTERNAL.

## 7. Exit criteria của TODAY-01

TODAY-01 được coi là **triển khai xong ở phía IVR** khi:

- Pack hiện hành đã được tạo và link từ worklist.
- Mọi câu hỏi có đúng owner, artifact và stop rule.
- Không còn yêu cầu Product quyết lại business pair đã có nguồn.
- Không có semantics session/revoke/opt-out bị IVR tự phát minh.
- Handoff ghi rõ external dispatch/approval chưa diễn ra.
- Chữ ký và phạm vi chữ ký của Module 8 Owner được ghi rõ, không lẫn với external approval.

TODAY-01 **không đồng nghĩa các decision đã đóng**. Sau handoff:

- Trạng thái task: **M8_OWNER_SIGNED / HANDOFF_READY / EXTERNAL_SIGNATURES_REQUIRED**.
- Việc tiếp theo thuộc owner nhận phiếu.
- IVR chỉ resume nhánh code tương ứng khi nhận đủ artifact/chữ ký.

## 8. Xác nhận của Module 8 / Project Owner

> **Tôi là người ký phía Module 8.** Tôi phê duyệt pack, routing, stop rule và các vị trí Module 8 dưới đây. Chữ ký này không đại diện và không thay thế chữ ký của bất kỳ external owner nào.

| Nội dung | Vị trí đã ký của Module 8 Owner |
|---|---|
| S-01 | Đồng ý OD-18: M3 quyết định call/no-call; IVR validate, execute và report. |
| S-02 | Giữ business pair đã có nguồn; M3 phải ký và implement wire mapping. |
| S-03 | Đồng ý bàn giao operator identity/UI sang M3; M3/Security phải nhận và ký contract §4A. |
| S-04 | Không tự chọn session field; chờ contract hai phía. |
| S-05 | Giữ Target V1 delivery fail-closed tới shared E2E. |
| S-06 | Không suy Rejected thành opt-out; chờ CRM/M3/Legal contract. |
| S-07 | Chọn phương án A; giữ D-06 revalidation phía M3 là bắt buộc. |
| S-08 | Không tự chọn token model/trust boundary. |
| S-09 | Không phê duyệt mock-lab-v1 cho production; chờ Product/Order Core. |
| S-10 | Chọn phương án A — thu hồi DOCX V0.3 lỗi thời; controlled execution còn chờ đồng bộ ledger sau W-0139. |

| Vai trò | Người ký | Ngày | Phạm vi |
|---|---|---|---|
| Module 8 / Project Owner | **Tôi — người dùng xác nhận trực tiếp** | **2026-08-29** | Pack + vị trí S-01..S-10 phía Module 8; không ký thay external owner |

### 8.1. Controlled execution follow-up — W-0141

S-10/OD-20 đã được thực thi sau khi W-0140 đồng bộ tracker: file
`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx` được đổi tên thành
`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx`. Rename giữ nguyên `45.101` byte và
SHA-256 `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c`; không xóa hoặc sửa nội
dung Word. Evidence: [`docs/evidence/W-0141/README.md`](../../docs/evidence/W-0141/README.md).
