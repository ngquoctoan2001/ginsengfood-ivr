# IR-05 — Open Contract Questions

Trạng thái: `OPEN` · Cập nhật: `2026-09-03`

Chỉ liệt kê thứ **chưa có lời đáp**. Không đóng bằng suy luận, và **không đóng chỉ bằng code IVR** — mock chạy xanh không biến `BLOCKED_EXTERNAL` thành `VERIFIED`.

## 1. Module 3 — hỏi qua tài liệu bàn giao, không hỏi ở đây

Toàn bộ câu hỏi cho Module 3 đã được gom thành checklist có ô tick trong **[06-module-3-api-handover.md §10](06-module-3-api-handover.md)**. Giữ một bản sao ở đây chỉ tạo ra hai danh sách lệch nhau — đó chính là lỗi bản trước mắc phải.

Ba câu **chặn cứng**, trích lại để thấy mức độ:

| Câu | Vì sao chặn |
| --- | --- |
| Ma trận `program × payment × order_state → callable` là gì? | Sai ma trận ⇒ **100% task bị `422`**, im lặng, không alert nào bắt được |
| Bao giờ có endpoint callback generic? | Chưa có ⇒ chương trình **24/7 không có lối trả kết quả nào** |
| `ivr_confirmation_required` do ai set, có bao giờ `false`? | Producer không set ⇒ không đơn nào được gọi |

## 2. Câu hỏi cho owner **ngoài** Module 3

| ID | Câu hỏi | Owner | Chặn | Evidence để đóng |
| --- | --- | --- | --- | --- |
| `OQ-POLICY-01` | Ký `ATP-01..15`: authority/source supersession; immutable two-program version+bundle hash; attempts/offsets/window/T0; counted/terminal + technical retry/backoff; quiet-hours/timezone; wire mismatch; M3 producer/distribution; registry four-eyes/lifecycle; cutover/in-flight; pre-dial coherence; capacity/token/audit/rollback. ⚠️ `D-10` và phase-8 đang lệch **bốn nhóm số**; current wire đã exact-compare và trả `409`, nhưng chưa có policy production | Product + Order Core + M3; Platform/M8/Release ở dòng kỹ thuật | **mọi production policy, producer và scheduler/registry promotion** | signed [M8-11 decision pack](../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md) + canonical bundle/hash + M3 producer SHA/OpenAPI/CDC/shared tests; [T-09](../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md) |
| `OQ-AUTH-01` | JWT issuer / audience / scope / TTL / JWKS; mTLS có bắt buộc không; sandbox credential | Security / Platform | **mọi integration test thật** | auth profile + test credential, [T-07](../docs/contracts/target-v1-closure-pack/T-07-production-auth.md) |
| `OQ-TEL-01` | Protocol/SDK, DTMF mode, disposition codes, vị trí resolver, caller ID cho 1 SIM lab | Infra / vendor | lab | vendor docs + lab pass, [IR-03](03-telephony-sim-requirements.md) |
| `OQ-TEL-02` | 32 eSIM: concurrency, capacity, rate, cost, failover | Infra / procurement | production capacity | procurement + load evidence |
| `OQ-DIALTOKEN-01` | Ký contact producer/requiredness, issuer, scalar-vs-reissue model, TTL, resolver output/topology, custody, replay/idempotency, failure/audit/retention và rollout theo `DTK-01..15` | M3 + Security + Platform + Telephony + M8 | **mọi production resolver/vault/adapter và real call** | signed [M8-10 decision pack](../plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md) + producer/resolve/vendor artifacts + shared E2E |
| `OQ-LEGAL-01` | Whitelist lời thoại (bộ hẹp hay bộ rộng), legal basis, do-not-call, retention. Recording giữ **OFF** | Legal / Privacy | **gọi khách thật** | signed review + PIA, [T-03](../docs/contracts/target-v1-closure-pack/T-03-speech-summary.md) |
| `OQ-OPTOUT-01` | Chốt explicit-only V1: signal/UX/script, subject key/scope/category, registry writer/read, lifecycle/ACK/reject/reversal, idempotency/retry, retention/DSAR và M3 `call_restriction`. Nếu muốn suy từ nhiều `Rejected`, phải ký contract V2 riêng; hằng số `2/3` current không có authority. | Product + CRM/M3 + Legal/Privacy | **mọi opt-out code và feedback E2E** | signed `OPT-01..11` + schema/producer/consumer SHA + [M8-08](../plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md) |
| `OQ-REL-01` | Pilot scope, release authority, rollback/kill switch | Release owner | go-live | accepted go/no-go packet |

> `OQ-LEGAL-01` là câu duy nhất mà **rủi ro là pháp lý chứ không phải kỹ thuật**. Chạy thật với whitelist chưa duyệt nghĩa là mỗi cuộc gọi đọc thông tin đơn hàng cho một người chưa xác thực danh tính, trên kênh không ghi âm nên không chứng minh được đã đọc gì. Không rollback được sau khi đã gọi.

## 3. Câu hỏi đã đóng — đừng hỏi lại

| Câu cũ | Kết quả |
| --- | --- |
| `OQ-SALES-01…06` | Gom vào [IR-06 §10](06-module-3-api-handover.md) |
| Ops-core cần build gì cho IVR? | **Không còn** — `OD-17`, xem [IR-02](02-ops-core-requirements.md) |
| Trusted-skip cần `CustomerTrustResolver` hoặc risk-evidence field? | **Không** — toàn bộ placement `OD-15` đã `SUPERSEDED` bởi `OD-18`; M3 quyết định đơn cần gọi, IVR chỉ thực thi |

## 4. Được phép làm trước khi có câu trả lời

Toàn bộ phía IVR build qua interface + deterministic mock/fake. Khi thiếu field hoặc API lúc implement:

1. Cấp Work ID tuần tự trong `prompt/_execution/prompt-execution-tracker.md`
2. Thêm/cập nhật requirement ở [IR-01](01-sales-platform-requirements.md) hoặc file owner tương ứng
3. Đánh `BLOCKED_EXTERNAL`

**Không được tự bịa production behavior.**
