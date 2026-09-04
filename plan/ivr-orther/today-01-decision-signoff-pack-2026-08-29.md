# TODAY-01 — Decision / Sign-off Pack hiện hành cho Module 8

**Ngày chuẩn bị:** 29/08/2026

**Evidence snapshot:** main @ **0baed74cd384cd661aed068c263a92ef97ead1f4**

**Trạng thái triển khai TODAY-01:** **M8_OWNER_SIGNED / HANDOFF_READY / EXTERNAL_SIGNATURES_REQUIRED**

**Người ký phía Module 8:** **Tôi — Module 8 / Project Owner** · **2026-08-29**

**External dispatch:** **NOT_PERFORMED**

**External approval:** **NOT_RECEIVED**

> Pack này là đầu mối gửi và nhận phản hồi hiện hành. Chữ ký Module 8 được ghi tại §8; nó không thay chữ ký M3, Security, Platform, Legal/Privacy, CRM hoặc Telephony, không thay tracker và không cho phép bắt đầu code phụ thuộc trước khi đúng external owner ký.
>
> **W-0152 follow-up:** provenance, exact hashes, dispatch batches và signature-intake template hiện
> được khóa tại [M8-12](m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md). M8-12 bổ
> sung `S-11` nhưng không mở rộng phạm vi chữ ký ngày 29/08/2026 từ `S-01..S-10` sang sheet mới.

## 1. Cảnh báo trước khi gửi

Không gửi nguyên trạng [Target Contract V1 Closure Pack cũ](../../docs/contracts/target-v1-closure-pack/README.md) như một nguồn hiện hành duy nhất:

- README của pack cũ vẫn ghim baseline **main@54ca239 / draft.18**; contract hiện đã lên **draft.22**.
- [T-01](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) đã được W-0145 sửa ngày
  03/09/2026: Flow 04/05 khóa **24_7 + COD** và **GOLDEN_HOUR + ONLINE**; phần còn thiếu là
  producer artifact/chữ ký M3 và production policy, không phải quyết định lại business pair.
- [T-07 cũ](../../docs/contracts/target-v1-closure-pack/T-07-production-auth.md) còn mô tả admin identity trước W-0128. Hiện Module 3 sở hữu operator identity/UI; IVR chỉ nhận service credential và actor identity.
- Các ticket cũ vẫn hữu ích cho câu hỏi kỹ thuật, acceptance artifact và mock boundary, nhưng phải được đọc qua correction/routing trong pack này.

Nguồn điều khiển theo thứ tự:

1. [Control order](00-index.md).
2. [Decisions log](decisions-log.md).
3. [IR-06 — Module 3 API handover](../../integration-requirements/06-module-3-api-handover.md).
4. OpenAPI draft hiện hành và evidence W-0123/W-0128/W-0137.
5. Ticket closure cũ chỉ làm thư viện câu hỏi/evidence khi không mâu thuẫn các nguồn trên.

## 2. Phạm vi pack

Pack này route mười một nhóm quyết định. Mỗi nhóm có owner, câu trả lời bắt buộc, artifact chấp nhận và stop rule.

| ID | Chủ đề | Owner trả lời | Trạng thái hiện tại |
|---|---|---|---|
| S-01 | M3 là business authority, IVR chỉ thực thi | M3 + M8; Privacy cho retention field | M8_OWNER_SIGNED / M3_PRIVACY_PENDING |
| S-02 | Program/task wire mapping | M3 + Product/Order Core | M8_CONTRACT_SIGNED_W0145 / M3_PRODUCT_PENDING |
| S-03 | Admin identity/UI handoff W-0128 | M3 + Security/Platform | M8_HANDOFF_SIGNED / M3_SECURITY_PENDING |
| S-04 | Upstream session trace | M3 + M8 | M8_POSITION_SIGNED_W0146 / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED |
| S-05 | Generic callback, ACK, auth, sandbox và CDC | M3 + Security/Platform + M8 | M8_LOCAL_CALLBACK_READY_W0147 / RETRY_AFTER_FIXED / SHARED_E2E_BLOCKED |
| S-06 | Opt-out feedback loop | Project Owner + CRM/M3.1 + M3 + Legal/Privacy + Product | M8_POSITION_SIGNED_W0148 / EXTERNAL_DECISION_PENDING / RUNTIME_NOT_AUTHORIZED |
| S-07 | Revoke/freshness lifecycle | Owner + M3 + M8 | W0149_CURRENT_OPTION_A_BEHAVIOR / OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND / CODE_NOT_AUTHORIZED |
| S-08 | Contact/dial-token production path | M3 + Security + Platform + Telephony | W0150_EVIDENCE_SUBMITTED / PRODUCTION_PATH_FAIL_CLOSED / CONTRACT_RUNTIME_MISMATCH_FOUND / EXTERNAL_DECISION_PENDING / CODE_NOT_AUTHORIZED |
| S-09 | Attempt policy production | Product + Order Core + M3; Platform/M8/Release ở dòng kỹ thuật | W0151_EVIDENCE_SUBMITTED / NUMERIC_CONFLICT_OPEN / M3_PRODUCER_NOT_FOUND / EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED |
| S-10 | Bản DOCX V0.3 lỗi thời | Module 8 Owner | OPTION_A_EXECUTED_W0141 / BYTES_PRESERVED / EXTERNAL_GATES_UNCHANGED |
| S-11 | Errata VoLTE và procurement acceptance | Module 8 Owner + Product + Infra/Procurement + Telephony/vendor | M8_ROUTING_PREPARED_W0152 / EXTERNAL_SIGNATURE_REQUIRED / PROCUREMENT_NOT_APPROVED |

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

W-0146 đã đối chiếu current source: task contract/domain/task DB/job DB **không có** upstream
session. `capacity_incident.session_id` lại đang mang nhiều capacity scope ID nội bộ/synthetic và có
cả global admin incident `ProgramCode=ALL`; vì vậy không được map đè Golden Hour session vào cột đó.

**Đề xuất đã ký phía M8:** dùng đúng một upstream field `golden_hour_session_id`, vì đây là tên có
trong master traceability và không đánh tráo với internal capacity session.

Contract M3 phải đồng ký đủ:

1. `golden_hour_session_id`: opaque string `1..128`, case-sensitive, không control/edge whitespace,
   không PII; M3/Golden Hour Core phát và giữ nguyên qua retry/replay.
2. Golden Hour: required/non-null. 24/7: field phải absent; `null` cũng bị từ chối.
3. Một session có thể có nhiều task; field không unique theo task, không dùng làm idempotency key.
4. Task/job giữ nguyên; task-scoped GH incident dùng cột nullable riêng. Current
   `capacity_incident.session_id` vẫn là internal scope ID.
5. Retention theo owning row; không log/public export/UI nếu chưa có signed use case và permission.
6. Cutover hai pha store → enforce; không backfill từ task/order/correlation/internal ID.

**Artifact chấp nhận:**

- [M8-06 sign-off/handoff](m8-06-upstream-session-trace-signoff-2026-09-03.md) có chữ ký M8.
- M3 decision record có signer/date/scope và xác nhận source/namespace/program semantics.
- Producer commit/client revision, OpenAPI acceptance, additive migration/cutover plan và CDC.
- Shared E2E exact SHA cho GH, 24/7, replay/conflict và capacity incident.

**Stop rule:** M8 đã khóa đề xuất của mình nhưng shared contract vẫn chưa ký. Không thêm field, cột,
migration hoặc runtime propagation trước chữ ký M3; không gọi “M8 signed” là “hai bên signed”.

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

**Follow-up `W-0147` / 03/09/2026:** M8 đã trace toàn bộ local callback seam và sửa defect
`429` từng bỏ qua `Retry-After`. Local transport nay mang positive server delay sang dispatcher;
retry không xảy ra sớm hơn header, vẫn giữ cùng immutable key/body và bounded retry budget. Callback
unit `38/38` và Sales contract `20/20` pass tại lượt focused đầu tiên. Kết quả này không chứng minh
consumer M3, auth thật, network hay sandbox.

**Artifact chấp nhận:**

- OpenAPI phía M3 + consumer commit.
- Security auth profile + sandbox credential reference.
- Shared E2E: callback thật nhận ACCEPTED, duplicate, stale, blocked, auth failure và retryable failure.
- Shared E2E phải phủ cả 24/7, changed-body idempotency conflict, `422`, `429 Retry-After`,
  outage/circuit/recovery và ghim exact SHA hai repo.
- Gói bàn giao: [M8-07 — Target V1 shared callback](m8-07-target-v1-shared-callback-handoff-2026-09-03.md).

**Stop rule:** không bật Target V1 delivery và không gỡ fail-closed validator trước shared E2E.

### S-06 — Opt-out feedback loop

Current taxonomy không có IVR_OPT_OUT. Rejected chỉ là NO_ANSWER + review signal; nó **không tự động là opt-out**.

Audit W-0148 làm rõ thêm:

- Inbound `call_restriction` đã chặn fail-closed; outbound feedback chưa được wire.
- Threshold `AbsoluteFloor=2`/`Default=3` và queue proposer chỉ là dormant local candidate.
- Không có counter/orchestrator/CRM delivery/ACK; proposal `PENDING_CRM` không có admin/ACK terminal
  transition và chưa có retention path đã ký.
- Snapshot CRM hiện có registry/read/user-consent primitive, nhưng không có signed service proposal
  contract cho M3/IVR.
- M8 đề xuất **explicit-only V1**; inference từ weak signal phải là contract V2 riêng. DTMF `0` vẫn
  là huỷ đơn và không được gán thêm nghĩa consent withdrawal.

CRM/M3.1, M3 và Legal/Privacy phải ký:

1. Sự kiện/action explicit nào được tính là yêu cầu opt-out; UX/script/version nào tạo nó.
2. Ai ghi vào consent/suppression registry.
3. Key/scope: customer/guest/contact channel/contact hash, tenant, channel, category và policy version.
4. Lifecycle pending/accepted/rejected/reversed/expired; effective_at, expiry/reversal, retention và audit.
5. Read contract để Order Core kiểm trước khi gửi task.
6. Writer event/API, idempotency, feedback/ACK, retry/DLQ/reconciliation từ CRM về M3/IVR.
7. Legal basis, minimization, DSAR/deletion/legal hold và hành vi fail-closed khi registry không trả lời.

**Artifact chấp nhận:**

- Consent/suppression contract đã ký.
- Legal basis/retention approval.
- CRM writer/read API hoặc event schema, test lifecycle/ACK/reversal và M3 producer CDC.
- Shared E2E exact SHA: explicit signal → CRM marker → `call_restriction=true` → IVR không dispatch.

- Gói trả lời exact `OPT-01..OPT-11`: [M8-08 opt-out/suppression decision pack](m8-08-opt-out-suppression-decision-pack-2026-09-03.md).
- Evidence audit: [W-0148](../../docs/evidence/W-0148/README.md).

**Stop rule:** không map Rejected/DTMF `0` thành opt-out, không tự thêm IVR_OPT_OUT và không wire
threshold `2/3`/`ContactReference`/proposal lifecycle tự suy đoán trong IVR. Nếu business muốn inferred
threshold, mở contract V2 riêng có chữ ký Product/CRM/Legal.

### S-07 — Revoke/freshness lifecycle

Hai hướng đã được trình Owner:

- **A — Chấp nhận stale-call trade-off:** IVR có thể tiếp tục attempt trong window; M3 revalidate khi callback và có quyền trả BLOCKED_BY_CORE.
- **B — Yêu cầu revoke/update:** M3 phát lệnh khi order bị hủy, recall, sale-lock hoặc không còn call-required.

**Correction W-0149 ngày 03/09/2026:** tài liệu nội bộ đã ghi lựa chọn **A** ngày 29/08, nhưng
approval reference/provenance độc lập chưa được cung cấp cho lượt audit này. Vì vậy chỉ được gọi là
`M8_POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED`, không phải shared acceptance. Behavior current
đúng là A; M3 vẫn phải ký và chứng minh D-06 revalidation là lớp an toàn bắt buộc.

Snapshot M3 `PhucApu@a3aad246d986` không có exact hit cho generic Target V1 callback consumer,
`BLOCKED_BY_CORE`, `REJECTED_STALE` hoặc IVR revoke path. Do đó phương án A **chưa có bằng chứng an
toàn end-to-end**; local fixture IVR không thay implementation phía M3.

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

**Decision/race matrix hiện hành:** [M8-09 — Revoke/freshness decision pack](m8-09-revoke-freshness-decision-pack-2026-09-03.md),
`RVK-01..RVK-12`. Không sửa scheduler/OpenAPI/DB cho tới khi strategy, authority và race behavior có
đúng signer.

### S-08 — Dial-token production path

**Phiếu kỹ thuật:** [T-04 — Dial-token](../../docs/contracts/target-v1-closure-pack/T-04-dial-token.md)

**Correction W-0150:** [M8-10 decision pack](m8-10-contact-dial-token-production-decision-pack-2026-09-03.md)
đã trace intake→protected persistence→scheduler→resolver→gateway và khóa `DTK-01..DTK-15`.
Production path hiện fail-closed; không có production resolver/protector/adapter/credential mount/egress.
OpenAPI/runtime lệch requiredness `phone_validation_status`; TTL current bị ép bằng window end;
MOCK/LAB reuse theo attempt và resolver output là opaque reference, không phải raw E.164.

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

**Stop rule:** không sửa OpenAPI/TTL/persistence, viết production resolver/protector/adapter, mở secret
hoặc egress trước khi M3/Security/Platform/Telephony ký đủ decision/artifact chịu trách nhiệm.

### S-09 — Attempt policy production

**Phiếu kỹ thuật:** [T-09 — Attempt policy](../../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md)

**Decision matrix:** [M8-11 — ATP-01..ATP-15](m8-11-attempt-policy-production-decision-pack-2026-09-03.md)

Owner phải ký:

- Authority/source supersession và production version mới theo canonical two-program bundle/hash.
- Window, customer attempts, offsets, exact T0/clock-skew, counted/terminal taxonomy cho từng program.
- Technical retry/backoff/manual retry; timezone/quiet-hours/holiday/window crossing.
- Giữ hay đổi wire version+snapshot. Current IVR đã exact-compare và trả `409` khi mismatch.
- M3 producer/distribution/CDC; registry four-eyes/lifecycle; cutover/in-flight và pre-dial coherence.
- Capacity/dial-token coupling, audit/retention, rollout/rollback và cách sửa các business source xung đột.

**Artifact chấp nhận:**

- Signed `ATP-01..ATP-15` có owner/date/reference.
- Canonical policy bundle đủ hai program + SHA-256; không dùng tên `mock-lab-v1`.
- M3 producer SHA/OpenAPI/schema/CDC + sandbox/shared tests.
- Registry lifecycle/four-eyes/database validation + pre-dial coherence design đã ký.
- Cutover/rollback + capacity/telephony/dial-token recalibration evidence.

**Stop rule:** không promote/rename/flip approval `mock-lab-v1`; không sửa scheduler, registry,
OpenAPI, DB, seed, config hoặc feature gate trước đủ chữ ký Product/Order Core/M3 và owner kỹ thuật.

**W-0151 correction:** current wire đã có exact mismatch behavior; task/job đã lưu immutable
snapshot. Gaps còn lại là production numbers/authority, M3 producer, registry governance,
technical-retry/temporal policy và runtime active-policy coherence. Module 8 chưa phê duyệt bất kỳ
production policy number nào.

### S-10 — Bản DOCX V0.3

Nguồn bằng chứng: [W-0137](../../docs/evidence/W-0137/README.md), [OD-20](decisions-log.md) và bản [V0.3 Markdown hiện hành](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md).

Ba phương án đã được trình Owner:

- **A — Thu hồi:** đổi tên _SUPERSEDED hoặc xóa theo quy trình được duyệt; Markdown là bản duy nhất.
- **B — Tái sinh:** sinh DOCX từ Markdown và chỉ định owner/gate chống lệch.
- **C — Giữ nguyên và chấp nhận rủi ro:** không khuyến nghị.

**Lựa chọn của Module 8 Owner ngày 29/08/2026:** **A — Thu hồi**. W-0141 đã thực thi controlled
rename sang `_SUPERSEDED`, giữ nguyên `45.101` byte và SHA-256; xem §8.1. Không có nội dung Word
bị sửa hoặc xóa và external gate không thay đổi.

**Artifact chấp nhận:**

- Lựa chọn A/B/C, tên người duyệt và ngày.
- Nếu B: owner sinh lại, công cụ/quy trình và gate parity.

**Stop rule:** không gửi artifact `_SUPERSEDED` như tài liệu hiện hành; không suy controlled
withdrawal thành release/external approval.

### S-11 — Errata VoLTE và procurement acceptance

Nguồn bằng chứng:

- [W-0135 — factual correction](../../docs/evidence/W-0135/README.md).
- [Errata 21](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md).
- [R-00 — voice gateway RFQ](../../docs/contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md).
- [R-06 — tờ trình mua thiết bị](../../docs/contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md).

**Fact đã kiểm:** mốc 2G toàn quốc `15/09/2026` giữ nguyên; 3G là tháng `09/2028`, không phải
`30/09/2026`. Yêu cầu VoLTE vẫn cần cho horizon vận hành dài hạn sau 09/2028, nhưng không được diễn
giải là thiết bị CSFB “chết sau một tháng”.

**Owner phải trả lời và giao artifact:**

- Module 8 Owner/Product xác nhận horizon, traffic/attempt assumptions và số kênh; không dùng con
  số chưa ký từ candidate policy.
- Infra/Procurement xác nhận model/SKU, báo giá, support lifecycle và điều kiện nghiệm thu.
- Telephony/vendor giao datasheet hoặc capability statement chứng minh VoLTE cho exact model/SKU,
  cùng kết quả acceptance test trên carrier/target environment.
- Owner của source spec phát hành controlled update cho `§13.2`; W-0152 không tự sửa nguồn bị khóa.

**Artifact chấp nhận:** signature record theo M8-12, exact model/SKU + vendor evidence, quote/channels
được duyệt, acceptance plan/result, procurement approval và reference tới controlled source update.

**Stop rule:** không mua/duyệt thiết bị 2G/WCDMA/CSFB-only cho horizon sau 09/2028; không gọi RFQ,
đề xuất 4 kênh hoặc local fact correction là model/procurement đã được duyệt.

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

1. Route batch `D-01`: **S-01, S-02, S-04, S-05, S-07** cho M3 contract/business/producer owner.
2. Route batch `D-02`: **S-03, S-05, S-08** cho Security/Platform; S-08 thêm Telephony.
3. Route batch `D-03`: **S-06** cho CRM/M3.1 + Legal/Privacy + M3 + Product.
4. Route batch `D-04`: **S-09 / ATP-01..15** cho Product, Order Core và M3; route
   Platform/M8/Release cho các dòng kỹ thuật. Chưa mở code trước đủ chữ ký và artifact.
5. **S-10** đã thực thi tại W-0141; không dispatch lại để xin quyết định.
6. Route batch `D-05`: **S-11** cho Module 8 Owner + Product + Infra/Procurement +
   Telephony/vendor, kèm W-0135/Errata 21/R-00/R-06 và exact hash manifest.
7. Dùng [M8-12](m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md) làm ledger
   dispatch/provenance; chữ ký §8 không thay external owner và không bao phủ S-11.

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
| S-04 | Chọn đề xuất `golden_hour_session_id`; giữ internal capacity `session_id` tách biệt; không mở code trước chữ ký M3. |
| S-05 | M8 local callback + `Retry-After` fix đã ký; giữ Target V1 delivery fail-closed tới khi M3/Security/Platform giao đủ artifact và shared E2E pass. |
| S-06 | Không suy Rejected thành opt-out; chờ CRM/M3/Legal contract. |
| S-07 | Position A đã được ghi, nhưng W-0149 yêu cầu approval provenance; D-06 phía M3 bắt buộc và runtime evidence hiện chưa tìm thấy. |
| S-08 | W-0150 đã nộp evidence/decision matrix; không tự chọn contact producer, token model/TTL/custody/trust boundary hoặc viết production path trước external signatures. |
| S-09 | W-0151 đã nộp audit/ATP-01..15; không phê duyệt `mock-lab-v1`, không chọn production numbers hoặc sửa runtime trước Product/Order Core/M3 và technical-owner artifacts. |
| S-10 | Chọn phương án A — thu hồi DOCX V0.3 lỗi thời; W-0141 đã controlled-rename, giữ nguyên byte/hash và external gates. |

| Vai trò | Người ký | Ngày | Phạm vi |
|---|---|---|---|
| Module 8 / Project Owner | **Tôi — người dùng xác nhận trực tiếp** | **2026-08-29** | Pack + vị trí S-01..S-10 phía Module 8; không ký thay external owner |

### 8.1. Controlled execution follow-up — W-0141

S-10/OD-20 đã được thực thi sau khi W-0140 đồng bộ tracker: file
`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx` được đổi tên thành
`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx`. Rename giữ nguyên `45.101` byte và
SHA-256 `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c`; không xóa hoặc sửa nội
dung Word. Evidence: [`docs/evidence/W-0141/README.md`](../../docs/evidence/W-0141/README.md).

### 8.2. Program/result contract follow-up — W-0145

M8-05 đã được đối chiếu lại với current OpenAPI/runtime/test và ký phía Module 8 ngày 03/09/2026:

- Program receiver giữ đúng `GOLDEN_HOUR + ONLINE` và `TWENTY_FOUR_SEVEN + COD`; M3 phải map
  `24_7`, `PHONE_VALID`, `ELIGIBLE_FOR_IVR` tại producer.
- Result contract giữ 11 code; runtime IVR có 9 producer path, 6 final callback và 2 blocked code
  chỉ dùng cho compatibility/pre-call.
- Correction: `IVR_CONFIRMATION_WINDOW_EXPIRED` hiện do scheduler IVR persist + enqueue callback;
  Sales/Order Core vẫn sở hữu revalidation và order-state transition.
- `mock-lab-v1` vẫn không phải production policy. Product/Order Core chưa giao policy/version đã ký.

Gói ký/handoff hiện hành:
[M8-05 — Program/result contract sign-off](m8-05-program-result-contract-signoff-2026-09-03.md).
External M3/Product/Security artifact và shared E2E vẫn `NOT_RECEIVED / NOT_RUN`; không nâng
`ACCEPTED` và không đóng `G-CONTRACT`/`G-POLICY`.

### 8.3. Upstream session trace follow-up — W-0146

M8-06 đã khóa đề xuất phía Module 8 ngày 03/09/2026:

- Dùng `golden_hour_session_id`, required/non-null cho Golden Hour và absent cho 24/7.
- Không dùng `session_id`/`source_session_id` làm alias trên wire.
- Không đổi nghĩa `capacity_incident.session_id`; nếu triển khai thì thêm cột upstream nullable riêng
  cho task/job/task-scoped incident.
- Store phase phải đi trước enforce phase; required-field cutover là breaking với producer cũ.
- Không backfill/synthesis từ task ID, order ID, correlation ID hoặc internal capacity ID.

Gói ký/handoff hiện hành:
[M8-06 — Upstream session trace sign-off](m8-06-upstream-session-trace-signoff-2026-09-03.md).
Chữ ký **Tôi — Module 8 / Project Owner** khóa đề xuất/stop rule phía M8. M3 signer, producer
commit, CDC, OpenAPI acceptance và shared E2E vẫn `NOT_RECEIVED / NOT_RUN`; code giữ
`NOT_AUTHORIZED`.

### 8.4. Target V1 shared callback follow-up — W-0147

M8-07 đã hoàn tất phần có thể làm độc lập tại repo IVR ngày 03/09/2026:

- Trace final result → immutable outbox → dispatcher → Target transport → ACK/retry/circuit →
  audit/review; không phát hiện consumer generic nào vì endpoint đích thuộc M3.
- Sửa defect `429`: runtime trước đây bỏ qua `Retry-After`; nay retry schedule dùng delay lớn hơn
  giữa local backoff và positive server delay.
- Giữ nguyên payload/hash/idempotency identity, bounded retry, terminal ACK behavior và fail-start
  guard cho real `TARGET_V1`.
- Local focused proof ban đầu: callback unit `38/38`, Sales contract `20/20`.

Gói ký/handoff hiện hành:
[M8-07 — Target V1 shared callback readiness](m8-07-target-v1-shared-callback-handoff-2026-09-03.md).
Chữ ký **Tôi — Module 8 / Project Owner** chỉ xác nhận local behavior, fix và stop rule. M3 consumer,
Security auth/custody, Platform sandbox/network và shared E2E vẫn `NOT_RECEIVED / NOT_RUN`; Target V1
delivery tiếp tục `DISABLED`.

### 8.5. Opt-out feedback-loop boundary follow-up — W-0148

M8-08 đã đối chiếu current source và sửa claim cũ ngày 03/09/2026:

- `Rejected` là `NO_ANSWER + review`; DTMF `0` là huỷ đơn. Cả hai không phải explicit opt-out.
- `OptOutSuppressionPolicy` và `QueueOnlySuppressionProposer` không có runtime caller/DI wiring;
  integration test tự khởi tạo proposer. Không có signal count store, CRM sender/ACK/reversal.
- Proposal `PENDING_CRM` không đi qua current admin mutation chỉ nhận item `OPEN`; vì vậy không được
  gọi generic review queue là proposal approval workflow.
- M8 ký đề xuất explicit-only V1 và từ chối wire threshold inference `2/3` khi Product/CRM/Legal
  chưa ký signal/window/key/dedupe/false-positive/reversal contract.

Gói ký/handoff hiện hành:
[M8-08 — Opt-out/suppression decision pack](m8-08-opt-out-suppression-decision-pack-2026-09-03.md).
Chữ ký **Tôi — Module 8 / Project Owner** khóa current-truth correction, explicit-only proposal và
stop rule phía M8. Product/CRM/M3/Legal/Security/Platform artifact cùng shared E2E vẫn
`NOT_RECEIVED / NOT_RUN`; code giữ `RUNTIME_NOT_AUTHORIZED`.
