# M8-08 — Opt-out / suppression decision pack và handoff

Ngày: `2026-09-03`

IVR baseline: `main@b21ec676e490`

Cross-repo snapshot chỉ để đối chiếu, không phải shared contract đã ký:
`C:\Projects\ginsengfood-business-platform` · branch `PhucApu` · `a3aad246d986`.

Trạng thái: **`M8_POSITION_SIGNED / CURRENT_LOOP_NOT_WIRED / EXPLICIT_ONLY_V1_PROPOSED / CRM_M3_LEGAL_SIGNOFF_REQUIRED / RUNTIME_NOT_AUTHORIZED`**

Người lập: **Codex — audit/handoff draft**. Không có chữ ký nào trong tài liệu này được dùng thay
Project Owner, CRM/M3, Legal/Privacy, Product hoặc Security.

## 1. Kết luận audit

C9 không phải một việc “thêm `IVR_OPT_OUT`”. Nó gồm hai chiều độc lập:

1. **Inbound restriction:** CRM/Customer Identity sở hữu registry; Module 3 hợp nhất quyết định vào
   task qua `call_restriction`. IVR đã chặn fail-closed khi restricted, unknown hoặc source
   unavailable.
2. **Outbound feedback:** IVR quan sát tín hiệu trong cuộc gọi rồi đề xuất cho owner của registry.
   Chiều này **chưa khép vòng**: code hiện chỉ có capture, pure threshold policy và queue-only
   proposer rời nhau; không có orchestration/runtime caller, delivery contract hoặc ACK.

Vì vậy verdict đúng là:

**`INBOUND_BLOCK_PRESENT / OUTBOUND_PRIMITIVES_TESTED_BUT_DORMANT / EXTERNAL_DECISION_REQUIRED`**.

`W-0034` giữ `DEFERRED_TARGET`; local test của các primitive không được nâng thành CRM integration.

## 2. Bằng chứng current

| Câu hỏi | Bằng chứng current | Kết luận |
| --- | --- | --- |
| `Rejected` hiện nghĩa gì? | `DispositionMapper.cs`: `Rejected → NO_ANSWER + REJECTED_REVIEW_REQUIRED`; `ResultRepository.cs` tạo `IVR_CALL_RESULT/OPEN` review item | Weak review signal, không phải huỷ đơn và không phải explicit opt-out |
| Có `IVR_OPT_OUT` trong taxonomy không? | Current `IvrResultType`, OpenAPI và generated model không có mã này | Không thêm mã mới trong M8-08 |
| Inbound do-not-call có chặn không? | `TaskIntakeService` reject khi `call_restriction=true`; `EligibilityRules` block restricted/unknown/unavailable | Có, fail-closed; M3 vẫn phải cung cấp source/evidence thật |
| Policy threshold có chạy runtime không? | `OptOutSuppressionPolicy.Decide` chỉ có caller trong unit/integration test; GitNexus không thấy production process | Không; `Default=3`, `AbsoluteFloor=2` chỉ là local candidate chưa được external owner ký |
| Proposer có chạy runtime không? | `QueueOnlySuppressionProposer` chỉ được tạo trực tiếp trong integration test; không DI registration/caller trong `src/` | Không |
| Có bộ đếm theo contact không? | Rejected review row dùng `SourceId=resultId`; schema không có `signal_count` hoặc stable CRM contact key | Không |
| Queue có gửi CRM/nhận ACK không? | Proposer chỉ ghi `ivr_review_items` trạng thái `PENDING_CRM`; không HTTP client/outbox worker/consumer | Không |
| Admin có xác nhận proposal được không? | `ReviewAsync` chỉ nhận item trạng thái `OPEN`; proposer tạo thẳng `PENDING_CRM` | Không có operational transition cho proposal |
| `ACCEPTED_BY_CRM` được ghi ở đâu? | Chỉ là constant/check-constraint; không có writer production | Không có ACK path |
| Idempotency hiện đủ chưa? | ID là `OPTOUT-{ContactReference}` và existing row làm mọi đề xuất sau trả cùng ID | Chưa: có thể gộp sai các signal/policy/reversal khác thời điểm; contract owner phải chốt key |
| Retention hiện đủ chưa? | Review item mặc định `LEGAL_DECISION_PENDING`; retention chỉ anonymize item đã có `resolved_at`; `PENDING_CRM` hiện không có đường resolve | Chưa; có nguy cơ giữ vô hạn nếu đưa vào runtime |
| CRM current có gì? | Snapshot business-platform có registry `consent_suppression_markers`, user-auth consent revoke/upsert và read eligibility trả `eligible/denyReason/suppressionMarkerId`; không có service proposal endpoint cho IVR/M3 | Registry/read primitive có thật, nhưng writer/proposal auth/schema/ACK chưa phải shared contract |

## 3. Factual corrections so với mô tả C9 cũ

- **Không tạo bảng signal mới ngay.** Current schema đã có review queue; data model chỉ được chọn sau
  khi có identity key, retention và replay semantics đã ký.
- **Không gọi threshold policy trong normalization ngay.** Normalization không có aggregate count và
  policy `2/3` chưa có CRM/M3/Legal/Product approval.
- **Không coi hai lần rejected là opt-out.** Hai lần rejected vẫn là hai weak observations. Chúng có
  thể mở manual review theo policy tương lai, nhưng không chứng minh khách đã rút quyền liên hệ.
- **Không thêm `IVR_OPT_OUT`.** Inbound restriction là pre-call veto; outbound observation/proposal là
  event/workflow khác result taxonomy của một cuộc gọi.
- **Không gọi trực tiếp CRM từ IVR V1.** `UT-ARCH-NO-CRM-EGRESS-06` đang khóa boundary. Nếu muốn đổi
  boundary, M3/CRM/Security phải ký và cập nhật architecture/egress contract trước code.

## 4. Position M8 đề xuất để owner ký

1. `Rejected` giữ nguyên `NO_ANSWER` counted + review; không suy ra customer consent mutation.
2. DTMF `1` là xác nhận đơn, DTMF `0` là yêu cầu huỷ đơn. Không tái dùng hai phím này cho opt-out.
3. Current V1 không có explicit opt-out signal. Muốn thêm phải có wording/script, signal source,
   proof và Legal/Privacy approval riêng.
4. IVR không sở hữu do-not-call registry và không ghi trạng thái `SUPPRESSED` local.
5. Inbound `call_restriction` tiếp tục fail-closed và chỉ M3/CRM có quyền cung cấp/refresh.
6. Weak signal có thể được lưu để review; không được tự động biến thành effective suppression.
7. Threshold `Default=3`/floor `2` được phân loại **`TEST_ONLY_CANDIDATE`**, không phải production
   policy. Không wire trước external signatures.
8. Không dùng raw phone, `phone_masked`, `customer_id` hoặc `phone_ref` chưa được owner xác nhận làm
   cross-system suppression key.
9. Không mở CRM egress, migration, worker hoặc admin mutation trong W-0148.
10. M8 đề xuất **explicit-only V1**: chỉ customer action có wording/script/proof đã được Product và
    Legal/Privacy ký mới được tạo proposal. Threshold inference từ weak signal là contract V2 riêng.

## 5. Decision matrix cần CRM/M3/Legal trả lại

| ID | Quyết định bắt buộc | Đề xuất M8 | Owner phải ký | Artifact chấp nhận |
| --- | --- | --- | --- | --- |
| `OPT-01` | Explicit opt-out signal là gì? | Chỉ một customer action được script/UI mô tả rõ và có proof; `Rejected` là `WEAK_REVIEW_SIGNAL` | CRM/M3.1 + Product + Legal/Privacy | Signal taxonomy + wording/script version + examples/negative cases |
| `OPT-02` | Weak-signal threshold dùng làm gì? | Nếu giữ candidate `3`, nó chỉ mở/propose manual review; không tự ghi registry. Mọi số khác phải có policy version | Product + CRM + Legal/Privacy | Signed policy table: window, count, dedupe, reset, false-positive handling |
| `OPT-03` | Stable identity/key | CRM phát `customer_contact_channel_id` và/hoặc contact hash; IVR chỉ mang opaque reference, không tự hash bằng thuật toán riêng | CRM/M3.1 + M3 + Privacy | Field semantics, namespace, stability, rotation/link/merge behavior |
| `OPT-04` | Route outbound | Ưu tiên M3/Order Core relay để giữ một integration seam; phương án CRM pull queue chỉ hợp lệ nếu CRM nhận ownership | M3 + CRM + Security/Platform | Signed topology, service identity, endpoint/event/OAS và network path |
| `OPT-05` | Idempotency | Key theo proposal/signal + policy version; không dùng duy nhất contact reference | CRM + M3 | Replay/same-body/changed-body contract và retention của key |
| `OPT-06` | Proposal/ACK lifecycle | Cần accepted, duplicate, rejected-invalid, retryable, terminal-rejected và correlation/evidence reference; tên wire do CRM/M3 chốt | CRM + M3 | Authoritative OAS/event schema + CDC + retry/DLQ contract |
| `OPT-07` | Effective suppression writer | Chỉ CRM Customer Identity ghi registry. M3/IVR không đổi consent trực tiếp nếu chưa có delegated authority | CRM/M3.1 + Legal/Privacy | Writer authorization, audit actor, proof requirements và negative authorization tests |
| `OPT-08` | Reversal/expiry | Reactivation chỉ bởi CRM với proof mới hơn suppression; expiry/merge/unlink phải được định nghĩa | CRM + Legal/Privacy | State machine + effective timestamps + reversal/appeal procedure |
| `OPT-09` | Retention/DSAR | Proposal, weak observations, audit và idempotency có chu kỳ riêng; `PENDING_CRM` không được vô hạn | Legal/Privacy + CRM + M8 | Signed retention table, anonymization rule, legal hold và deletion tests |
| `OPT-10` | Inbound freshness | M3 query/revalidate CRM trước tạo task và quyết định cách thu hồi task nếu restriction xuất hiện giữa window | M3 + CRM | Producer/revalidation/CDC + liên kết với M8-09 revoke/freshness |
| `OPT-11` | Admin authority | Ai được confirm/reject weak-signal review và action đó tạo proposal hay chỉ annotation | CRM/Product + Security | Permission, dual-control nếu cần, audit/evidence và UI/API test |

## 6. Semantic lifecycle đề xuất — chưa phải wire contract

```text
REJECTED call
  -> WEAK_REVIEW_SIGNAL
  -> review only
  -> [signed threshold/admin policy]
  -> PROPOSAL_READY
  -> M3/CRM delivery boundary
  -> CRM_ACCEPTED | CRM_DUPLICATE | CRM_REJECTED | RETRY_PENDING

explicit customer opt-out
  -> EXPLICIT_SIGNAL_WITH_PROOF
  -> [signed writer/authorization contract]
  -> CRM-owned consent/suppression mutation
  -> future M3 task carries call_restriction=true
```

Các tên trên chỉ mô tả semantics để owner trả lời; không được copy thành enum/endpoint trước khi
authoritative contract được ký.

## 7. Những gì từng owner phải giao

### CRM / Module 3.1

- Authoritative proposal/write/read contract và service authentication.
- Stable contact-channel identity, contact hash ownership và channel/category semantics.
- Registry state machine, proof, expiry/reversal và ACK/idempotency behavior.
- CDC/contract test cho accepted, duplicate, invalid, retryable, reversal và active lookup.

### Module 3 / Order Core

- Chọn/implement relay hoặc signed alternative; IVR không tự mở CRM egress.
- Hợp nhất CRM read vào `call_restriction`, provenance/evaluated-at/source-version và fail-closed.
- Producer test cho `call_restriction=true/false/unknown/unavailable`.
- Quyết định freshness/revoke khi registry đổi giữa hai attempt.

### Legal / Privacy

- Wording/proof nào cấu thành explicit opt-out cho transactional `PHONE_CALL`.
- Phân biệt weak rejection, order cancellation và consent withdrawal.
- Retention/DSAR/legal hold cho observation, proposal, ACK và audit.
- Quyền admin, appeal/reversal và dữ liệu được phép đi qua boundary.

### Product / Module 8 Owner

- Threshold/window chỉ để review hay được phép tạo proposal.
- False-positive budget, manual review SLA và behavior khi không có người xử lý.
- Xác nhận current DTMF `0/1` không đổi nghĩa trong M8-08.

## 8. Kế hoạch implementation sau chữ ký

1. Freeze authoritative CRM/M3 schema và CDC trước.
2. Impact-analyze tối thiểu `ResultRepository.NormalizeNextAsync`, `ReviewAsync`,
   `OptOutSuppressionPolicy.Decide`, persistence entity/index và worker composition.
3. Viết contract tests cho identity, idempotency, ACK/retry và forbidden raw phone trước runtime.
4. Chọn store dựa trên signed retention/query pattern; không mặc định thêm bảng.
5. Nối capture → aggregate → policy → proposal bằng một orchestrator có audit, giữ normalization
   result bất biến.
6. Nối outbound theo topology đã ký; delivery lỗi giữ durable retry, không suppress local.
7. Nối CRM ACK và terminal lifecycle; sửa admin transition theo permission đã ký.
8. Chạy shared E2E: weak signal không mutate, explicit signal có proof, duplicate, changed-body,
   outage/recovery, reversal và task mới bị M3 chặn bằng `call_restriction`.

## 9. Gate mở code

Code chỉ được mở khi đủ đồng thời:

- `OPT-01..OPT-11` có signer/name/date/scope và approval reference;
- authoritative OAS/event schema + service auth + reachable sandbox;
- retention/legal basis đã ký;
- M3 producer/relay plan + CDC;
- test matrix và rollback/cutover được hai bên chấp nhận.

Thiếu bất kỳ mục nào: **`RUNTIME_NOT_AUTHORIZED`**.

## 10. Approval record

| Role | Signer / authority / date / approval reference | Trạng thái |
| --- | --- | --- |
| Module 8 / Project Owner | Chưa cung cấp | `NOT_RECEIVED` |
| CRM / Module 3.1 | Chưa cung cấp | `NOT_RECEIVED` |
| Module 3 / Order Core | Chưa cung cấp | `NOT_RECEIVED` |
| Legal / Privacy | Chưa cung cấp | `NOT_RECEIVED` |
| Product | Chưa cung cấp | `NOT_RECEIVED` |
| Security / Platform | Chưa cung cấp | `NOT_RECEIVED` |

Cho tới khi bảng trên có provenance thật, nội dung §4 là **đề xuất M8 đã ghi nhận**, không phải quyết
định production đã được phê duyệt. `REAL_CUSTOMER_CALL_ALLOWED=NO` và các external gate không đổi.
