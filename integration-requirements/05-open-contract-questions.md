# IR-05 — Open Contract Questions

Trạng thái: `OPEN` · Cập nhật: `2026-08-26`

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
| `OQ-POLICY-01` | Chốt `attempt_policy_version` production: mỗi program → số attempt / offsets / window. ⚠️ `D-10` và tài liệu `phase-8` đang lệch **bốn con số** | Product / Order Core | production policy | owner decision ID + version, [T-09](../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md) |
| `OQ-AUTH-01` | JWT issuer / audience / scope / TTL / JWKS; mTLS có bắt buộc không; sandbox credential | Security / Platform | **mọi integration test thật** | auth profile + test credential, [T-07](../docs/contracts/target-v1-closure-pack/T-07-production-auth.md) |
| `OQ-TEL-01` | Protocol/SDK, DTMF mode, disposition codes, vị trí resolver, caller ID cho 1 SIM lab | Infra / vendor | lab | vendor docs + lab pass, [IR-03](03-telephony-sim-requirements.md) |
| `OQ-TEL-02` | 32 eSIM: concurrency, capacity, rate, cost, failover | Infra / procurement | production capacity | procurement + load evidence |
| `OQ-LEGAL-01` | Whitelist lời thoại (bộ hẹp hay bộ rộng), legal basis, do-not-call, retention. Recording giữ **OFF** | Legal / Privacy | **gọi khách thật** | signed review + PIA, [T-03](../docs/contracts/target-v1-closure-pack/T-03-speech-summary.md) |
| `OQ-REL-01` | Pilot scope, release authority, rollback/kill switch | Release owner | go-live | accepted go/no-go packet |

> `OQ-LEGAL-01` là câu duy nhất mà **rủi ro là pháp lý chứ không phải kỹ thuật**. Chạy thật với whitelist chưa duyệt nghĩa là mỗi cuộc gọi đọc thông tin đơn hàng cho một người chưa xác thực danh tính, trên kênh không ghi âm nên không chứng minh được đã đọc gì. Không rollback được sau khi đã gọi.

## 3. Câu hỏi đã đóng — đừng hỏi lại

| Câu cũ | Kết quả |
| --- | --- |
| `OQ-SALES-01…06` | Gom vào [IR-06 §10](06-module-3-api-handover.md) |
| Ops-core cần build gì cho IVR? | **Không còn** — `OD-17`, xem [IR-02](02-ops-core-requirements.md) |
| Trusted-skip cần `CustomerTrustResolver`? | **Không** — `OD-15` thay bằng một field `trust.risk_evidence_available` |

## 4. Được phép làm trước khi có câu trả lời

Toàn bộ phía IVR build qua interface + deterministic mock/fake. Khi thiếu field hoặc API lúc implement:

1. Cấp Work ID tuần tự trong `prompt/_execution/prompt-execution-tracker.md`
2. Thêm/cập nhật requirement ở [IR-01](01-sales-platform-requirements.md) hoặc file owner tương ứng
3. Đánh `BLOCKED_EXTERNAL`

**Không được tự bịa production behavior.**
