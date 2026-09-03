# Target Contract V1 — Closure Pack

Trạng thái: `OPEN_EXTERNAL` · Work `W-0058` (prompt `P11-2`) · Tạo `2026-08-18` · baseline lịch sử `main@54ca239` (`draft.18`)

Correction hiện hành: `W-0145` · `2026-09-03` · T-01 và producer/result semantics đã được đối
chiếu lại trên `main@b21ec676e490`; đọc cùng
[M8-05 sign-off](../../../plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md).

Dial-token correction: `W-0150` · `2026-09-03` · T-04 đã được đối chiếu lại về contact requiredness,
TTL equality, scalar/reuse semantics, opaque resolver output và production fail-closed; đọc cùng
[M8-10 decision pack](../../../plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md).

Attempt-policy correction: `W-0151` · `2026-09-03` · T-09 đã được đối chiếu lại về exact wire
mismatch `409`, immutable task/job snapshot, seed scope, registry lifecycle/four-eyes gap,
technical-retry config và pre-dial flag drift; đọc cùng
[M8-11 decision pack](../../../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md).

Gói review duy nhất để Sales / Product / Privacy / Security trả lời bằng **code, OpenAPI và test** — không trả lời bằng ý kiến.

> **Mock không đóng được dòng nào trong gói này.** IVR đã build xong toàn bộ Phase 0–3 sau fake provider. Điều đó chứng minh IVR sẵn sàng nhận, **không** chứng minh hợp đồng đã chốt. Mọi row `W-0002…W-0007` giữ `BLOCKED_EXTERNAL` cho tới khi có artifact thật.

## 1. Vì sao có gói này

IVR là service standalone; Sales giữ chân lý về đơn hàng (D-02: IVR **không bao giờ** ghi order state). Nghĩa là mọi thứ IVR cần đều phải do Sales/Security cấp qua hợp đồng. Hiện có **13 quyết định mở** chặn 6 external work item. Gói này biến 13 quyết định đó thành 9 ticket, mỗi ticket có đúng một artifact đóng.

## 2. Bảng ticket

| Ticket | Chủ đề | External W | OD-V1 | Owner | Due (chặn cái gì) | Gate | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [T-01](T-01-program-matrix.md) | Ma trận program/payment/IVR-required/callable | `W-0002` | `01`, `13`, `14` | Sales Product/Core + Product/Business | trước `P4-2` | real integration | `M8_SIGNED / M3_PRODUCT_PENDING` |
| [T-02](T-02-task-data-order-version.md) | Task data: `order_version`, eligibility/restriction evidence | `W-0002` | `03` | Sales Core | trước `P4-2` | real integration | `OPEN` |
| [T-03](T-03-speech-summary.md) | Privacy-safe order summary + whitelist biến lời thoại | `W-0003` | `04`, `15` | Sales/Product + Privacy/Legal | whitelist trước script production; nội dung trước `P8-2` | business acceptance | `OPEN` |
| [T-04](T-04-dial-token.md) | Dial-token: issue/resolve/TTL/one-use/audit | `W-0004` | `05`, `17`, `18` | M3/Security/Platform/Telephony | trước `P8-1` | real call · `LAB_REAL_SIM` | `W0150_EVIDENCE_SUBMITTED / OPEN_EXTERNAL` |
| [T-05](T-05-callback-ack.md) | Generic callback target + ACK taxonomy + idempotency/version | `W-0005` | `02` | Sales API/Core | trước `P4-1` | real integration | `OPEN` |
| [T-06](T-06-no-answer-timeout.md) | No-answer / wait-for-timeout / revalidation race | `W-0005` | `06` | Sales Product/Core | trước `P8-2` | real integration | `OPEN` |
| [T-07](T-07-production-auth.md) | Production JWT issuer/audience/scope/TTL/JWKS + mTLS | `W-0006` | `07` | Security/Platform | trước `P4-4` | real integration | `OPEN` |
| [T-08](T-08-openapi-compat-cdc.md) | OpenAPI compatibility/deprecation + sở hữu CDC | `W-0002`,`W-0005` | — (process) | Sales API + IVR | trước `P9-1` | real integration | `OPEN` |
| [T-09](T-09-attempt-policy.md) | `attempt_policy_version` production | `W-0007` | `16` (+`08`) | Product + Order Core + M3 | trước `P9-1` | production | `W0151_EVIDENCE_SUBMITTED / OPEN_EXTERNAL` |

Cột **Due** là hạn suy ra từ phụ thuộc kỹ thuật — nó nói ticket này chặn việc gì, không phải ngày trên lịch. Ngày cam kết thật do owner điền vào từng ticket.

Phủ đủ `OD-V1-01..07` và `OD-V1-13..18` theo DoD của `P11-2`. Nguồn của bảng quyết định: [`specs/_review/open-decisions-register.md`](../../../specs/_review/open-decisions-register.md) (chỉ đọc — gói này **không** sửa file đó).

## 3. Cách trả lời một ticket

Mỗi ticket có 8 mục cố định. Owner chỉ cần điền mục **Closure artifact**:

| Mục | Ai điền |
| --- | --- |
| Current evidence | IVR đã điền — source-read, có file:line |
| Target delta | IVR đã điền — chênh lệch chính xác |
| Sample payload | IVR đã điền — hợp lệ theo OpenAPI hiện tại |
| Acceptance test | IVR đã điền — test phải xanh khi đóng |
| Owner | IVR đã điền |
| Due | IVR đã điền phần "chặn cái gì"; **owner điền ngày cam kết** |
| Gate | IVR đã điền |
| Mock fallback | IVR đã điền — IVR đang chạy bằng gì trong lúc chờ |
| **Closure artifact** | **Owner điền** — artifact chính xác, không phải câu trả lời văn xuôi |

**Ticket "done" mà không có code/OpenAPI/test đã merge thì vẫn là `BLOCKED_EXTERNAL`.** Đề xuất kỹ thuật của dev Sales không phải chữ ký owner.

## 4. Baseline đã ghim

Mọi so sánh trong gói này chạy trên baseline đã ghim ở [`specs/api/openapi/contract-manifest.json`](../../../specs/api/openapi/contract-manifest.json):

| Thứ | Giá trị |
| --- | --- |
| Sales current baseline | `ginsengfood-business-platform` @ `a3aad246d986fbc273cf41aaa93eec6659669656` |
| IVR internal API (TARGET_DRAFT) | `sha256:b59a644e5bcaca3ad33b2b91523e14ec65196027b4a37a6b3c73d6842e8676b9` |
| Sales callback target (TARGET_DRAFT) | `sha256:af0cb5cc3f47aaa4c8e232418c216b228fd996e316fe129a7cbf1d4636659697` |
| Current Golden Hour compat fixture | `sha256:ad2f655070b14d0cdfb0540893f7d7ea83354dda56c4b403ae47f56a3f6a494d` |

Nếu Sales trả lời trên một commit khác, ghi commit đó vào ticket — **không** so sánh chéo hai baseline rồi kết luận.

## 5. Hai ranh giới không thương lượng trong mọi ticket

**D-02 — IVR không bao giờ chuyển trạng thái đơn.** `recommended_core_action` trong callback là **advisory**. Sales revalidate rồi tự quyết. Không ticket nào được đề nghị IVR ghi order state.

**D-05 — IVR không bao giờ nhận số điện thoại thô.** Task mang `phone_ref`, `phone_masked`, `dial_token`. Bất kỳ đề xuất nào đưa E.164 vào payload đều là thay đổi kiến trúc, phải mở quyết định riêng chứ không giải quyết trong ticket.

## 6. Cái gói này KHÔNG làm

- Không đóng `W-0002…W-0007`. Chỉ tạo con đường để đóng.
- M8 đã ký receiver matrix `GOLDEN_HOUR+ONLINE` / `TWENTY_FOUR_SEVEN+COD` và semantics
  `ivr_confirmation_required=true` tại W-0145; gói này **không** ký thay producer M3 hoặc đóng
  artifact/CDC external. Attempt policy (`OD-V1-16`), whitelist lời thoại (`OD-V1-15`) và
  dial-token semantics (`OD-V1-17`) vẫn chưa được external owner phê duyệt.
- Không sửa business source trong `docs/documents/`. Chỗ nào business source mâu thuẫn, ticket ghi mâu thuẫn và để owner business sửa.
- Không gửi gì ra ngoài. IVR soạn gói; owner IVR quyết định gửi cho ai và khi nào.

## 7. Liên quan

- Gói telephony/SIM: `P11-1` / `W-0057` (sở hữu `W-0008`) — chưa làm.
- Gói legal/retention: `P11-3` / `W-0059` (sở hữu `W-0009`) — chưa làm.
- Bảng readiness tổng: `P11-4` / `W-0060` — chưa làm; **soi** ledger chứ không thay thế.
- Sổ tiến độ duy nhất: [`prompt/_execution/prompt-execution-tracker.md`](../../../prompt/_execution/prompt-execution-tracker.md).
