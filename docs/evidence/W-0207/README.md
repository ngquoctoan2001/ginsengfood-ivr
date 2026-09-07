# W-0207 — P2.2 Sandbox E2E: nửa IVR của ma trận, và một mâu thuẫn hợp đồng

Ngày: 2026-09-06. Trạng thái: **TESTS_PASS cho nửa IVR; `BLOCKED_EXTERNAL` cho nửa Module 3.**
Không tự ACCEPTED. Đây **không phải** shared-E2E report.

## 1. Vì sao lượt này không "chờ Module 3"

Module 3 chưa xong luồng bán hàng của họ nên không cấp được sandbox — chờ là thời gian chết chứ
không phải một phụ thuộc có lịch. Nên đảo chiều phụ thuộc: **IVR chứng minh trước nửa của mình**,
trên đúng ma trận mà M3 sẽ được mời đồng ký, ghim theo đúng contract hash đã đóng băng ở `W-0204`.
Khi M3 tới, phần thiếu chỉ còn là nửa của họ.

`W-0174` đã định nghĩa ma trận đó: 11 case `TV1-E2E-01..11` cộng một validator và một template có
chỗ trống. Trước lượt này **không có gì trong repo chứng minh nửa IVR của bất kỳ case nào** — 11 mã
case chỉ tồn tại trong chính gói W-0174.

## 2. Mâu thuẫn hợp đồng, tìm được mà không cần M3

Ma trận yêu cầu Module 3 trả:

| Case | Ma trận yêu cầu | Hợp đồng đã ghim cho phép |
|---|---|---|
| `TV1-E2E-03-EXACT-REPLAY` | `DUPLICATE_ACCEPTED` trên **HTTP 409** | `DUPLICATE_ACCEPTED` chỉ ở **200** |
| `TV1-E2E-06-CORE-BLOCKER` | `BLOCKED_BY_CORE`/`REVIEW_REQUIRED` trên **200 hoặc 409** | cả hai chỉ ở **200** |

`CallbackAck409` chỉ mang `REJECTED_STALE` và `IDEMPOTENCY_CONFLICT`. Đây không phải lỗi định dạng
nhỏ: `TargetV1CallbackTransport` đọc body 409 theo `CallbackAck409`, không parse được mã lạ, trả
`CALLBACK_ACK_INVALID`, và dispatcher biến nó thành **`INVALID_DEAD_LETTER` — terminal, không retry**.

Nếu M3 xây theo tờ ma trận đó thì **mọi lần replay chính xác sẽ bị dead-letter**, im lặng, và triệu
chứng duy nhất nhìn thấy được là callback ngừng tới.

Hệ quả được **chứng minh chứ không phát biểu**: `UT-CALLBACK-TARGET-ACK-CROSS-01` (6 trường hợp) gửi
từng mã ACK trên sai status và khẳng định kết quả là `Invalid` + `CALLBACK_ACK_INVALID` + đúng **một**
lần gửi.

Bên nào đúng: **hợp đồng**. `OD-V1-02` ký ngày `2026-09-05` đúng nguyên văn taxonomy đó
(200 `ACCEPTED`/`DUPLICATE_ACCEPTED`/`BLOCKED_BY_CORE`/`REVIEW_REQUIRED`, 409 `REJECTED_STALE`/
`IDEMPOTENCY_CONFLICT`). Tờ ma trận ghim `matrix_contract: M8-07-SECTION-6.2026-09-04` — sớm hơn
một ngày. README của W-0174 **không** ghi status cho hai case đó; con số `409` chỉ nằm trong code.
Đã sửa tờ ma trận về `[200]`.

### Gate để lớp lỗi này không quay lại

`FREEZE-06` trong `contract-freeze-verifier.mjs`:

1. Mã ACK mà một case yêu cầu phải được hợp đồng đã ghim cho phép **ở từng status case đó khai**.
2. Mọi mã ACK hợp đồng định nghĩa phải được **ít nhất một case** yêu cầu — lỗ hổng trong ma trận
   nghiệm thu là thứ được phát hiện ở production.
3. Hash callback OAS mà validator tự ghim phải bằng hash trong manifest. `FREEZE-02` không quét
   code, nên đây là bản sao pin duy nhất còn có thể trôi mà không ai thấy.

Selftest **14/14** (thêm 3 case cho `FREEZE-06`), trong đó có đúng khiếm khuyết vừa sửa.

## 3. Nửa IVR của ma trận: 11/11

Chạy `20 vòng, 330 task, 0 failure` — [`local-mock-e2e.json`](local-mock-e2e.json), khối
`shared_e2e_ivr_side`:

| Case | Chứng minh bởi | Quan sát |
|---|---|---|
| `TV1-E2E-01` Golden Hour accepted | `ACKOK` | `DELIVERED_ACCEPTED` |
| `TV1-E2E-02` 24/7 accepted qua generic endpoint | `ACK247` *(mới)* | `DELIVERED_ACCEPTED` |
| `TV1-E2E-03` Exact replay | `ACKDUP` | `DELIVERED_ACCEPTED` |
| `TV1-E2E-04` Same key, changed body | `ACKCONFLICT` | `IDEMPOTENCY_CONFLICT` |
| `TV1-E2E-05` Stale version/state | `ACKSTALE` | `REJECTED_STALE` |
| `TV1-E2E-06` Core blocker | `ACKBLOCK`+`ACKREVIEW` | `DELIVERED_BLOCKED`, `DELIVERED_REVIEW` |
| `TV1-E2E-07` Auth negative | `ACK401` *(mới)* | `AUTH_REJECTED` |
| `TV1-E2E-08` Invalid schema/result | `ACK422` | `INVALID_DEAD_LETTER` |
| `TV1-E2E-09` Rate limit | `ACK429` | `RETRY_EXHAUSTED`, `Retry-After` được tôn trọng |
| `TV1-E2E-10` M3 outage/timeout | pha `callback-outage` | 3/3 giao lại sau khi receiver lành, retry cao nhất 2/3 |
| `TV1-E2E-11` No-answer final | `NOANSWER` | `DELIVERED_ACCEPTED`, advisory, không hủy đơn |

Bất biến kèm theo: **0 final trùng, 0 callback trùng, 0 kết quả không-phải-khách bị đếm là customer
attempt**; sổ giao nhận lấy từ journal bên nhận — **314 callback id, 26 lần giao lại, 0 lần không
nhất quán**.

## 4. Một khẳng định của tôi sai, đã sửa

Pha outage ban đầu đòi **cả bốn** callback phải phục hồi. Nó đỏ — và hệ thống không sai. Một outage
dài hơn ngân sách retry thì **đúng ra phải** dead-letter; `MaxRetries=3` là chính sách, không phải
tai nạn. Bài test cũ đòi một thứ hệ thống không hứa.

Tách làm hai, vì đó là hai khẳng định khác nhau:

- **4a-1 — tiến trình nhận biến mất** (connection refused, lỗi transport thật). Cái được bảo đảm ở
  đây là **ngân sách có trần**: mọi message về trạng thái terminal, không cái nào vượt `MaxRetries`,
  không mất, không nhân đôi. Lượt này: 4 phục hồi, 0 dead-letter.
- **4a-2 — receiver sống nhưng trả 503**, gỡ lỗi đi **bên trong** ngân sách. Đây mới là chỗ phục hồi
  được bảo đảm, nên đây mới là chỗ khẳng định nó, và đây là `TV1-E2E-10`. Lượt này: 3/3, retry cao
  nhất 2/3.

## 5. Bề mặt admin (gạch 5) — nửa IVR đã có, nửa BFF thì không

Không dựng gì mới: bề mặt đã được phủ bởi test đang chạy, và liệt kê ở đây để nửa còn thiếu nhìn
thấy được.

| Yêu cầu | Bằng chứng phía IVR |
|---|---|
| read/write/danger tier | `IT-API-AUTHZ-01`, `IT-API-AUTHZ-02` |
| danger có actor + reason | `AdminScopeGuard` bắt buộc `X-Actor-Id` + `X-Action-Reason`; `IT-API-AUDIT-04` |
| four-eyes khi áp dụng | `PostgresFourEyesApprovalVerifier` (`W-0195`) trên runtime gate và script lifecycle; `RuntimeGateApprovalTests` |
| idempotency/replay trên mutation | `IT-API-IDEMP-03` |
| danger thật sự nguy hiểm | `IT-API-TERMINATE-09/10`, `IT-API-QUEUE-08`, `IT-API-SIM-09`, `IT-API-RETRY-06` |

**BFF của Module 3 gọi những API này thì chưa có gì chứng minh** — cần M3.

## 6. Vì sao P2.2 **chưa** đạt exit

Exit đòi "hai chiều producer/callback và bề mặt admin đều có sandbox evidence trên cùng contract
hash". Lượt này giao **một nửa** của cả ba, trên đúng contract hash `W-0204` đã ghim. Nửa kia —
producer của M3 phát task call-ready, M3 revalidate rồi đổi trạng thái đơn, BFF của M3 gọi admin
API — **không quan sát được từ phía này bằng bất kỳ cách nào**, và `shared_e2e_ivr_side` ghi đúng
câu đó trong chính artifact.

Chữ ký thật cần `target-v1-shared-e2e-report-validator.mjs` với artifact M3 thật và **năm chữ ký**
(`M8_OWNER`, `M3_OWNER`, `SECURITY`, `PLATFORM`, `RELEASE_OWNER`). Template vẫn `NOT_READY`.

## 7. Tái lập

```bash
npm --prefix deploy/ci run contract:freeze
npm --prefix deploy/ci run test:contract-freeze
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj --filter TestId=UT-CALLBACK-TARGET-ACK-CROSS-01
pnpm e2e:mock -- --rounds 20 --workers 2 --extended-every 2 --evidence-dir docs/evidence/W-0207
node deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs --check-template docs/evidence/W-0174/shared-e2e-report.template.json
```

## 8. Giới hạn

- Local/MOCK, working-tree candidate. Không có M3, không có vendor, không có khách thật.
- `shared_e2e_ivr_side` **không phải** shared-E2E report và không thể trở thành nó bằng cách thêm
  chữ ký: mỗi case còn một nửa chỉ M3 quan sát được.
- 20 vòng nói về tính đúng đắn lặp lại của ma trận hợp đồng, **không** thay bằng chứng 100 vòng của
  `W-0203` và không nói gì về soak.
- Rotation token có overlap, rate limit thật, DNS/TLS outage: mới có ở mức fake/transport. Bằng
  chứng thật cần credential sandbox (`OQ-AUTH-01`).
- Chưa ai ký. `TESTS_PASS`, không phải `ACCEPTED`.
