# M8-13 — External decision copy/paste dispatch-message kit

Work: `W-0153` · ngày lập: `2026-09-03`

Trạng thái:
**`LOCAL_MESSAGE_KIT_READY / RECIPIENT_IDENTITIES_REQUIRED /
EXTERNAL_DISPATCH_NOT_PERFORMED / EXTERNAL_APPROVAL_NOT_RECEIVED`**

Nguồn dispatch bất biến:

- M8-12: `plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md`
- M8-12 SHA-256: `311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d`
- Manifest: `docs/evidence/W-0170/artifact-sha256.txt`
- Manifest SHA-256: `945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92`

Người lập: **Codex — message preparation only**. W-0153 không biết danh tính người nhận thật,
không gửi message/ticket và không ký hoặc duyệt thay owner.

## 1. Pre-send bắt buộc

Trước khi copy bất kỳ message nào:

1. Điền `[RECIPIENT_NAME]`, `[RECIPIENT_ROLE]`, `[CHANNEL/TICKET]` và `[REQUESTED_RESPONSE_DATE]`;
   không gửi tới một mailbox chung nếu không xác định được owner có thẩm quyền.
2. Verify lại SHA-256 của M8-12 và manifest. Nếu lệch, dừng gửi, tạo manifest mới và cập nhật message.
3. Đính kèm đúng artifact liệt kê cho batch; không thay bằng screenshot hoặc bản export không có hash.
4. Không ghi `approved`, `accepted`, `delivered` hoặc `production-ready` trước khi có receipt/response.
5. Sau khi gửi, ghi receipt/hash ở ticket hoặc audit system-of-record và đưa metadata vào W-0170;
   không sửa file message kit đang được hash-pin. Sau khi nhận trả lời, đối chiếu đủ M8-12 §5.

## 2. Message D-01 — M3 contract/business/producer

**To:** `[RECIPIENT_NAME — M3 contract/business/producer owner]`

**CC:** `[Product/Order Core owner nếu khác người nhận]`

**Channel/ticket:** `[CHANNEL/TICKET]`

**Subject:** `ACTION REQUIRED — Module 8 S-01/S-02/S-04/S-05/S-07 contract decisions`

```text
Chào [RECIPIENT_NAME],

Module 8 gửi batch D-01 để xin quyết định và artifact thuộc thẩm quyền M3 cho các sheet:
- S-01: OD-18 business authority và retention field;
- S-02: program/task wire mapping và result consumer;
- S-04: golden_hour_session_id;
- S-05: generic callback/ACK/auth/sandbox;
- S-07: revoke/freshness strategy và D-06 evidence.

Nguồn điều khiển: M8-12 SHA-256
311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d.
Manifest SHA-256:
945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92.

Vui lòng trả lời từng sheet theo M8-12 §5, gồm signer identity, role/organization,
authority source, exact artifact hash, decision, scope/environment, approval timestamp,
effective/cutover, rollback, evidence reference và residual blocker.

Artifact cần giao tối thiểu: OD18-C1..C5; producer mapping/CDC; generic callback OAS/consumer;
golden_hour_session_id contract + producer SHA; và RVK-01..12 cùng D-06 evidence hoặc revoke contract.

Requested response date: [REQUESTED_RESPONSE_DATE — owner điền].

Cho tới khi đủ phản hồi: IVR không remove field/enum, không sửa session/revoke contract,
không bật callback delivery và không mở production gate.
```

**Attachments:** OD-18 form; M8-05; M8-06; M8-07; M8-09; M8-12; SHA-256 manifest.

## 3. Message D-02 — Security/Platform/Telephony

**To:** `[RECIPIENT_NAME — Security owner]`; `[RECIPIENT_NAME — Platform owner]`

**CC:** `[RECIPIENT_NAME — Telephony/vendor owner, bắt buộc cho S-08]`

**Channel/ticket:** `[CHANNEL/TICKET]`

**Subject:** `ACTION REQUIRED — Module 8 S-03/S-05/S-08 trust-boundary sign-off`

```text
Chào các owner Security/Platform/Telephony,

Module 8 gửi batch D-02 cho:
- S-03: operator identity/UI handoff;
- S-05: generic callback auth, network/TLS, sandbox và ACK;
- S-08: contact/dial-token issuer, resolver, TTL/reissue, custody, vendor API, egress và audit.

M8-12 SHA-256:
311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d.
Manifest SHA-256:
945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92.

Vui lòng trả lời từng sheet theo M8-12 §5 và DTK-01..DTK-15. Mỗi phản hồi phải có
authority source, exact hash, scope/environment, cutover/rollback và evidence thực tế.

Requested response date: [REQUESTED_RESPONSE_DATE — owner điền].

Cho tới khi đủ artifact: callback delivery giữ disabled; IVR không code production adapter/vault,
không mount secret, mở egress, đưa raw E.164 vào IVR hoặc bật PRODUCTION_REAL.
```

**Attachments:** W-0128 evidence; M8-07; M8-10; T-04; M8-12; SHA-256 manifest.

## 4. Message D-03 — CRM/M3.1, M3, Legal/Privacy, Product

**To:** `[RECIPIENT_NAME — CRM/M3.1 owner]`; `[RECIPIENT_NAME — M3 owner]`;
`[RECIPIENT_NAME — Legal/Privacy owner]`; `[RECIPIENT_NAME — Product owner]`

**CC:** `[Security/Platform owner nếu transport/store được chọn]`

**Channel/ticket:** `[CHANNEL/TICKET]`

**Subject:** `ACTION REQUIRED — Module 8 S-06 opt-out/suppression contract`

```text
Chào các owner CRM/M3/Legal/Product,

Module 8 gửi batch D-03 cho S-06 opt-out/suppression. Current inbound call_restriction đã
fail-closed; outbound feedback chưa được wire. IVR không coi Rejected là opt-out.

M8-12 SHA-256:
311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d.
Manifest SHA-256:
945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92.

Vui lòng trả OPT-01..OPT-11: explicit signal, subject/scope, threshold nếu có, registry/store owner,
write/read/ACK/reversal lifecycle, retention/legal basis, auth/audit và shared-test artifact.
Phản hồi phải theo M8-12 §5, gắn exact hash và authority source.

Requested response date: [REQUESTED_RESPONSE_DATE — owner điền].

Cho tới khi đủ chữ ký: IVR không thêm IVR_OPT_OUT, không suy consent mutation từ disposition và
không sửa OpenAPI/DB/runtime.
```

**Attachments:** M8-08; M8-12; SHA-256 manifest.

## 5. Message D-04 — Product/Order Core/M3 attempt policy

**To:** `[RECIPIENT_NAME — Product owner]`; `[RECIPIENT_NAME — Order Core owner]`;
`[RECIPIENT_NAME — M3 producer owner]`

**CC:** `[Platform owner]`; `[Module 8 technical owner]`; `[Release owner]`

**Channel/ticket:** `[CHANNEL/TICKET]`

**Subject:** `ACTION REQUIRED — Module 8 S-09 production attempt-policy bundle`

```text
Chào Product/Order Core/M3,

Module 8 gửi batch D-04 cho S-09. Current sources còn xung đột số production; mock-lab-v1 chỉ là
candidate. Wire đã exact-compare snapshot và fail 409 khi mismatch, nhưng chưa có canonical
production bundle hoặc M3 producer evidence.

M8-12 SHA-256:
311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d.
Manifest SHA-256:
945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92.

Vui lòng trả ATP-01..ATP-15 và giao canonical two-program bundle + SHA-256, signer provenance,
M3 producer SHA/OpenAPI/CDC, registry lifecycle/four-eyes, effective/cutover/rollback, pre-dial
coherence và shared-test evidence theo M8-12 §5.

Requested response date: [REQUESTED_RESPONSE_DATE — owner điền].

Cho tới khi đủ artifact: không promote/rename mock-lab-v1, không chọn production numbers và không
sửa scheduler/registry/config/seed.
```

**Attachments:** M8-11; T-09; M8-12; SHA-256 manifest.

## 6. Message D-05 — VoLTE/procurement

**To:** `[RECIPIENT_NAME — Module 8 Owner]`; `[RECIPIENT_NAME — Product owner]`;
`[RECIPIENT_NAME — Infra/Procurement owner]`; `[RECIPIENT_NAME — Telephony/vendor owner]`

**Channel/ticket:** `[CHANNEL/TICKET]`

**Subject:** `ACTION REQUIRED — Module 8 S-11 VoLTE and procurement acceptance`

```text
Chào các owner Module 8/Product/Infra/Procurement/Telephony,

Module 8 gửi batch D-05 cho S-11. Fact đã hiệu chỉnh: 2G toàn quốc 15/09/2026; 3G tháng 09/2028.
VoLTE là yêu cầu cho horizon dài hạn sau 09/2028, không phải claim thiết bị CSFB chết sau một tháng.

M8-12 SHA-256:
311e2feda84bac04a51d2177c095ec15757763293753b0537320fbe0d25c119d.
Manifest SHA-256:
945663bb3d3b12a18c506f8bad47e1cddda44b1f5cb47ce36ba43578c26a5e92.

Vui lòng giao exact model/SKU, vendor datasheet/capability statement chứng minh VoLTE, support
lifecycle, báo giá 1 và 4 kênh, approved channel count, target-carrier acceptance plan/result,
procurement approval và owner-controlled update cho source spec §13.2. Phản hồi theo M8-12 §5,
gắn exact hash, authority source và rollback/rejection path.

Requested response date: [REQUESTED_RESPONSE_DATE — owner điền].

Không mua/duyệt thiết bị 2G/WCDMA/CSFB-only cho horizon sau 09/2028; RFQ, đề xuất 4 kênh và local
fact correction không phải model/procurement approval.
```

**Attachments:** W-0135 evidence; Module 8 V0.3 Markdown/Errata 21; R-00; R-06; M8-12;
SHA-256 manifest.

## 7. Receipt field reference — không sửa trực tiếp file hash-bound

Bảng dưới là baseline trước dispatch, không phải ledger để ghi đè. Receipt thật phải được giữ ở
ticket/audit system-of-record; W-0170 bind reference/hash và kiểm quorum mà không làm drift message kit.

| Dispatch | Actual recipient identity | Role / authority source | Channel + message/ticket ID | Sent timestamp with timezone | M8-12 hash verified | Manifest hash verified | Delivery state | Response artifact/reference |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `D-01` | `CHƯA_GỬI` | `CHƯA_CÓ` | `CHƯA_CÓ` | `CHƯA_CÓ` | `NOT_RUN` | `NOT_RUN` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-02` | `CHƯA_GỬI` | `CHƯA_CÓ` | `CHƯA_CÓ` | `CHƯA_CÓ` | `NOT_RUN` | `NOT_RUN` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-03` | `CHƯA_GỬI` | `CHƯA_CÓ` | `CHƯA_CÓ` | `CHƯA_CÓ` | `NOT_RUN` | `NOT_RUN` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-04` | `CHƯA_GỬI` | `CHƯA_CÓ` | `CHƯA_CÓ` | `CHƯA_CÓ` | `NOT_RUN` | `NOT_RUN` | `NOT_PERFORMED` | `NOT_RECEIVED` |
| `D-05` | `CHƯA_GỬI` | `CHƯA_CÓ` | `CHƯA_CÓ` | `CHƯA_CÓ` | `NOT_RUN` | `NOT_RUN` | `NOT_PERFORMED` | `NOT_RECEIVED` |

Một URL/message ID không tự chứng minh người nhận có authority hoặc đã đọc attachment. Chỉ chuyển
`DELIVERED` khi channel có receipt phù hợp; chỉ chuyển decision sang approved khi response record
đủ M8-12 §5 và exact hash khớp.

## 8. Exit và bước kế tiếp

W-0153 hoàn tất phía local khi 5 message, receipt template, tracker/readiness, target worklist và
Markdown map khớp nhau. Trạng thái tối đa là `EVIDENCE_SUBMITTED / LOCAL_MESSAGE_KIT_READY`.

**Bước kế tiếp:** Module 8 Owner/chief auditor điền danh tính/role/channel/due date thật, verify hai
hash, gửi từng batch, lưu receipt ngoài artifact hash-bound và chạy W-0170. Nếu chưa có recipient có authority, giữ
`EXTERNAL_DISPATCH_NOT_PERFORMED`; không gửi đại tới nhóm chung để tạo cảm giác đã handoff.
