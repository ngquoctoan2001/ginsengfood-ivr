# M8-09 — Revoke / recall / freshness decision pack và handoff

Ngày: `2026-09-03`

IVR baseline: `main@b21ec676e490`

Cross-repo snapshot chỉ để đối chiếu, không phải shared contract đã ký:
`C:\Projects\ginsengfood-business-platform` · branch `PhucApu` ·
`a3aad246d986fbc273cf41aaa93eec6659669656`.

Trạng thái: **`EVIDENCE_SUBMITTED / CURRENT_OPTION_A_BEHAVIOR_PRESENT /
M8_POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND /
OPTION_B_NOT_IMPLEMENTED / CODE_NOT_AUTHORIZED`**

Người lập: **Codex — audit/handoff draft**. Tài liệu này không ký thay Project Owner, M3/Order
Core, Product, Security/Platform hoặc Ops owner.

## 1. Kết luận audit

`C10 + C11 + C13` là một bài toán lifecycle duy nhất, không phải ba thay đổi code độc lập.

1. **Behavior hiện tại tương ứng phương án A:** IVR validate snapshot khi intake, sau đó có thể tiếp
   tục attempt trong confirmation window. Callback mang `order_version_seen_by_ivr`; M3 được kỳ
   vọng revalidate state/version/blocker hiện thời và có thể ACK `BLOCKED_BY_CORE` hoặc
   `REJECTED_STALE`.
2. **Lưới an toàn A chưa được chứng minh end-to-end:** tại snapshot M3 nêu trên, exact search không
   thấy generic Target V1 callback consumer, ACK `BLOCKED_BY_CORE`/`REJECTED_STALE` hay D-06
   runtime path. Fixture IVR tự ghi ACK blocked không thay thế implementation phía M3.
3. **Phương án B chưa tồn tại:** current IVR chỉ có `POST /tasks`; không có revoke/update route,
   state, persistence field, scheduler/dispatch fencing hay shared ACK. Re-POST task không phải
   update: cùng idempotency/payload thì replay, body khác thì conflict.
4. **Freshness current chỉ là intake-time validity:** `captured_at` phải nằm trong confirmation
   window và không ở tương lai; chưa có `valid_until`, maximum age, source sequence/revision hoặc
   invalidation event giữa các attempt.

Vì vậy verdict đúng là:

**`CURRENT_OPTION_A_BEHAVIOR_PRESENT / PRODUCTION_SAFETY_NOT_PROVEN /
OPTION_B_REQUIRES_SIGNED_CONTRACT / NO_CODE_CHANGE`**.

Cho tới khi đúng owner ký chiến lược và M3 giao runtime evidence, `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 2. Bằng chứng current

| Câu hỏi | Bằng chứng current | Kết luận |
| --- | --- | --- |
| IVR nhận task bằng route nào? | `TaskIntakeEndpoint.cs:18,32` chỉ map `POST /v1/ivr/order-confirmation/tasks` | Không có revoke/update command |
| Có thể re-POST để đổi blocker/version không? | `TaskIntakeStores.cs:241-313` khóa task/idempotency scope; same payload replay, changed payload conflict | Không được tái dùng intake POST làm update |
| Freshness được kiểm lúc nào? | `EligibilityRules.cs:239-247,296-395`; evidence schema yêu cầu `captured_at` trong window và không ở tương lai | Intake/evaluation-time only; không có mid-window invalidation |
| Scheduler claim dựa trên gì? | `PostgresSchedulerStore.cs:90-140` kiểm eligible, due/deadline, final/active attempt và global pause | Không đọc current order/recall/sale-lock/revoke state |
| Sau claim có recheck trước dial không? | `SchedulerRuntime.cs:51-113` claim rồi dispatch; `PostgresTelephonyDispatchStore.cs:127-168,432-449` chỉ kiểm lease/token/fencing kỹ thuật | Có khoảng race claim → dial chưa có business fence |
| Admin pause/terminate có thay revoke được không? | `InternalAdminApiService` pause chặn new claims toàn cục; terminate ghi request cho live attempt | Không; đây là control vận hành, không phải M3 business command |
| DB có revoke lifecycle không? | `ConfirmationTaskEntity` không có revoke/version-invalidation; `CallJobEntity` chỉ có status/queue/closed reason | Chưa có persisted authority/state/fence |
| Có result `IVR_TASK_CANCELLED_BY_BLOCK` không? | `CallResult.cs` và DB check constraints chỉ có current 11 result | Không; tên trong worklist cũ là đề xuất chưa ký |
| Callback có semantic blocked/stale không? | Target ACK có `BLOCKED_BY_CORE` và `REJECTED_STALE`; recommended core action có block/ignore stale | Có ACK semantics, nhưng đó là kết quả M3 xử lý callback, không phải revoke command ACK |
| Test race hiện chứng minh gì? | `IT-ELIG-RACE-12` tự hoàn tất callback `DELIVERED_BLOCKED`; IVR result vẫn `IVR_CONFIRMED` | Chứng minh IVR bookkeeping, không chứng minh M3 runtime revalidation |
| Direct Ops lookup có được phép không? | `OD-17`, FR eligibility và `UT-ARCH-NO-OPS-EGRESS-05` cấm IVR direct Ops egress | Source of truth phải đi qua M3; không thêm Ops client vào scheduler |
| M3 current đã có D-06 consumer chưa? | Snapshot `PhucApu@a3aad246d986`; exact search `ivr-result-callbacks`, Target ACK/revoke term trong `back-end/src/main` + tests = `0` | `M3_D06_RUNTIME_NOT_FOUND`; cần owner M3 cung cấp artifact khác nếu có |

## 3. Sửa các giả định trong mô tả cũ

- **Không tự tạo** `POST /tasks/{task_id}:revoke` hoặc `IVR_TASK_CANCELLED_BY_BLOCK`. Chúng chưa nằm
  trong shared OpenAPI/CDC và chưa có signer.
- **Không chỉ thêm điều kiện vào scheduler SQL.** Cách đó không đóng được race sau khi row đã claim
  nhưng trước khi telephony dial, hoặc khi cuộc gọi đã dialing/ringing/active.
- **Không dùng admin terminate làm business revoke.** Permission, audit actor, scope và behavior
  khác nhau; admin API còn không xử lý queued task.
- **Không gọi trực tiếp Ops/CRM từ dispatch.** `OD-17` và architecture test giữ topology một cửa
  qua M3.
- **Không đổi final result đã quan sát.** Nếu khách đã bấm `1`, IVR result vẫn phản ánh sự kiện cuộc
  gọi; M3 ACK blocked/stale phản ánh quyết định business sau revalidation.
- **Không gọi fixture `IT-ELIG-RACE-12` là shared E2E.** Test đó mô phỏng ACK ngay trong IVR store.

## 4. Hai chiến lược cần owner quyết định

### A — Chấp nhận stale-call, M3 bắt buộc revalidate callback

Đây là behavior current và khớp trade-off ghi trong `OD-17`. Không cần IVR revoke command, nhưng
chỉ an toàn khi M3 thật sự thực hiện D-06 cho **mọi callback**, gồm version/state/recall/sale-lock/
quality hold/evidence freshness, trước khi đổi order state.

Điều kiện chấp nhận A:

- Project Owner/M3 ký rõ stale-call UX và chi phí cuộc gọi được chấp nhận.
- M3 giao generic callback route/OAS, service auth, idempotency và code/CDC của D-06.
- Shared E2E chứng minh `IVR_CONFIRMED` trên order vừa recall/sale-lock/version-changed không được
  confirm; ACK về IVR là blocked/stale và retry không đảo quyết định.
- M3 outage/revalidation unavailable phải fail closed; không được ACK accepted trước khi recheck.

### B — M3 phát revoke/update để IVR ngừng attempt

Chỉ chọn B sau khi ký đủ `RVK-01..RVK-12`. Implementation phải là lifecycle command riêng, lưu
authority/version/audit và có fencing xuyên từ scheduler claim tới telephony execution. B vẫn không
thay D-06: một event có thể trễ hoặc mất, nên M3 phải revalidate callback.

## 5. Decision matrix `RVK-01..RVK-12`

| ID | Quyết định bắt buộc | Đề xuất M8 | Owner phải ký | Artifact chấp nhận |
| --- | --- | --- | --- | --- |
| `RVK-01` | Chọn A, B hay hybrid | Giữ A làm current compatibility; chỉ thêm B nếu business cần giảm stale call. D-06 luôn bắt buộc | Project Owner + M3 + Product | Signed strategy/trade-off, scope và effective date |
| `RVK-02` | Trigger nào được revoke? | order cancelled, recall, sale-lock, quality hold, no-longer-call-required và voice restriction phải có taxonomy/source rõ | M3 + Ops/CRM owner + Product | Source-of-truth/state table + examples |
| `RVK-03` | Authority/topology | M3 phát command sau khi hợp nhất source; IVR không query Ops/CRM | M3 + Security/Architecture | Signed sequence diagram, service identity và network path |
| `RVK-04` | Command/auth | Command riêng; không tái dùng intake POST hoặc admin terminate | M3 + M8 + Security/Platform | Authoritative OpenAPI/event schema, auth/permission và negative tests |
| `RVK-05` | Identity/version | Task/order ID, observed order version, source revision/effective time, reason, correlation và command ID | M3 + M8 | Field semantics, limits, required/optional matrix và CDC |
| `RVK-06` | Idempotency/order | Same command+body duplicate; changed body conflict; quy tắc out-of-order theo monotonic source revision | M3 + M8 | Replay/conflict/out-of-order contract + retention window |
| `RVK-07` | State transition | Chốt behavior queued, leased, dialing/ringing, active, normalized/final và expired | Product + M3 + M8 + Telephony | Signed transition/race table |
| `RVK-08` | Claim→dial fencing | Persist generation/revision; kiểm cùng generation khi claim và ngay trước dial; stale lease không được thắng | M8 + Telephony/Platform | DB invariant, concurrency design và tests |
| `RVK-09` | Active-call behavior | Mặc định không tự cắt active call; nếu cần cắt phải có explicit reason/authority/script UX | Product + Legal + Telephony + M3 | Active-call policy, operator UX, audit và rollback |
| `RVK-10` | ACK/result semantics | Revoke command cần ACK riêng; không thêm IVR result code nếu chưa có signed requirement. Final call result đã có không bị rewrite | M3 + M8 + Product | ACK taxonomy + callback interaction examples |
| `RVK-11` | Audit/retention/metrics | Lưu command actor/source revision/effective/received/applied outcome, không raw PII; retention theo Legal | M8 + M3 + Legal/Security | Audit schema, retention/DSAR, metrics/alerts và access control |
| `RVK-12` | Rollout/failure policy | Contract/store first, dual-version compatibility, producer later; M3 unavailable/revoke delayed không được bypass D-06 | M3 + M8 + Platform/Release | Cutover/rollback plan, feature flag, failure drills và shared E2E |

## 6. Race matrix bắt buộc nếu chọn B

| Trạng thái khi command tới | Current behavior | Behavior phải được ký trước code |
| --- | --- | --- |
| Task accepted, job chưa eligible | Không có revoke state | Persist revoke; eligibility không được mở job |
| Eligible/queued, chưa claim | Scheduler vẫn có thể claim | Atomic exclude theo current generation/revoke state |
| `LEASED_PENDING_DISPATCH` | Lease hợp lệ vẫn đi dispatch | Invalidate generation/reservation; stale worker không được dial |
| Renderer/token lookup đang chạy | Không business recheck | Recheck/fence ngay trước dial bằng cùng generation |
| Dialing/ringing | Không policy revoke | Chốt cancel attempt hay để kết thúc; counted-attempt semantics rõ |
| `ACTIVE_CALL` | Chỉ admin danger terminate thủ công | Chốt keep/cut; nếu cut phải dùng business-authorized path riêng và audit |
| Customer đã trả lời, normalization chưa commit | Không revoke interaction | Customer evidence không được rewrite; M3 vẫn revalidate result |
| Final result/callback pending | Scheduler không tạo attempt mới | Revoke ACK không đổi final result; delivery/callback vẫn theo immutable payload |
| Callback đã M3 xử lý | Không có revoke command | Duplicate/out-of-order command trả terminal ACK, không đảo order transition |
| Window expired/job closed | Command chưa tồn tại | Idempotent terminal outcome; không reopen job |

## 7. Freshness contract còn thiếu

Nếu Owner yêu cầu recheck trước mỗi attempt nhưng không chọn event revoke, M3 vẫn phải định nghĩa một
signed read contract. Contract đó tối thiểu phải có source revision/version, evaluated/effective time,
`valid_until` hoặc maximum age, source unavailable semantics, timeout/retry budget, auth và cache rule.

M8 **không đề xuất direct synchronous Ops lookup**. Hai topology hợp lệ để owner cân nhắc là:

1. M3 push revoke/update theo B; hoặc
2. IVR gọi M3-owned business-decision endpoint trước attempt.

Cả hai đều cần xử lý TOCTOU và không thay callback D-06. Current evidence schema không đủ để tự suy ra
hai topology này.

## 8. Artifact bắt buộc để đóng phương án A

- M3 generic Target V1 callback endpoint/OAS và code pointer exact SHA.
- D-06 implementation/CDC: idempotency, version/state, recall/sale-lock/quality hold, source outage.
- Auth/custody và reachable sandbox từ Security/Platform.
- Shared E2E trên exact IVR/M3 SHA cho accepted, duplicate, version stale, recall/sale-lock, outage,
  retry và recovery.
- Owner approval có signer/authority/date/scope/reference cho stale-call trade-off.

Thiếu các artifact trên: **`M3_D06_RUNTIME_NOT_FOUND / PRODUCTION_SAFETY_NOT_PROVEN`**.

## 9. Kế hoạch implementation nếu B được ký

1. Freeze authoritative command schema, state/race matrix và rollback plan.
2. Impact-analyze tối thiểu intake/task/job entities, persistence configuration, scheduler claim,
   scheduler runtime, telephony dispatch store/gateway, normalization/callback và admin read surface.
3. Thêm persistence generation/revoke/audit bằng migration additive, giữ rolling compatibility.
4. Implement idempotent command + authorization trước; chưa bật producer.
5. Thêm atomic claim filter và pre-dial generation fence; viết concurrency tests làm đỏ trước.
6. Implement signed dialing/active-call behavior, không tái dùng admin permission ngầm.
7. Giữ result immutable; nối command ACK/audit/metrics riêng.
8. Chạy DB integration, race/lease tests, contract compatibility, shared E2E và rollback drill.

## 10. Approval record

| Role | Signer / authority / date / approval reference | Trạng thái |
| --- | --- | --- |
| Module 8 / Project Owner | Claim lựa chọn A có trong pack ngày 29/08, nhưng provenance độc lập chưa được cung cấp cho lượt audit này | `POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED` |
| Module 3 / Order Core | Chưa cung cấp | `NOT_RECEIVED` |
| Product | Chưa cung cấp | `NOT_RECEIVED` |
| Ops/CRM source owner | Chưa cung cấp | `NOT_RECEIVED` |
| Security / Platform / Telephony | Chưa cung cấp | `NOT_RECEIVED` |
| Legal / Privacy, nếu cắt active call | Chưa cung cấp | `NOT_RECEIVED` |

## 11. Stop rule

- Không sửa scheduler, telephony, DB, OpenAPI hoặc result enum trước signed strategy/contract.
- Không tự công nhận dòng “Owner chọn A” trong tài liệu nội bộ là external/M3 approval.
- Không dùng selected green tests để gọi D-06 production-ready.
- Không bật Target V1 delivery hoặc cuộc gọi khách thật.

`W-0149` đóng ở mức **decision/evidence handoff**, không phải runtime completion.
