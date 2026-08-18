# W-0058 — Evidence: Sales/Auth Target Contract Closure Pack (`P11-2`)

Ngày: `2026-08-18` · Trạng thái đạt được: `EVIDENCE_SUBMITTED` (mức tối đa của một gói tài liệu, theo DoD `P11-2`)

## 1. Artifact

| File | Nội dung |
| --- | --- |
| `docs/contracts/target-v1-closure-pack/README.md` | Index, bảng 9 ticket, cách trả lời, baseline đã ghim, hai ranh giới không thương lượng |
| `docs/contracts/target-v1-closure-pack/T-01-program-matrix.md` | Ma trận program/payment/IVR-required/callable |
| `…/T-02-task-data-order-version.md` | `order_version`, `order_state`, eligibility/restriction evidence |
| `…/T-03-speech-summary.md` | Privacy-safe order summary + whitelist biến lời thoại |
| `…/T-04-dial-token.md` | Dial-token: issue/resolve/TTL/one-use/audit |
| `…/T-05-callback-ack.md` | Generic callback target + ACK taxonomy |
| `…/T-06-no-answer-timeout.md` | No-answer, wait-for-timeout, race lúc revalidation |
| `…/T-07-production-auth.md` | Production JWT/JWKS/mTLS + sandbox credential |
| `…/T-08-openapi-compat-cdc.md` | OpenAPI compatibility/deprecation + sở hữu CDC |
| `…/T-09-attempt-policy.md` | `attempt_policy_version` production |

Tổng 10 file. Không sửa code, không sửa contract, không sửa `docs/documents/`, không sửa `specs/_review/`.

## 2. Phủ DoD

`P11-2` §6 yêu cầu `OD-V1-01..07` và `OD-V1-13..18` đều có ticket. Đối chiếu:

| Quyết định | Ticket |
| --- | --- |
| `OD-V1-01` ma trận program/payment | T-01 |
| `OD-V1-02` callback path + ACK taxonomy | T-05 |
| `OD-V1-03` order version | T-02 |
| `OD-V1-04` speech-safe summary schema | T-03 |
| `OD-V1-05` dial-token issue/resolve/TTL | T-04 |
| `OD-V1-06` no-answer/timeout/revalidation | T-06 |
| `OD-V1-07` production auth + mTLS | T-07 |
| `OD-V1-13` Golden Hour ONLINE có thuộc scope | T-01 |
| `OD-V1-14` `ivr_confirmation_required` không có nguồn | T-01 |
| `OD-V1-15` speech variable whitelist | T-03 |
| `OD-V1-16` attempt policy delta vs business source | T-09 |
| `OD-V1-17` dial-token reuse semantics | T-04 |
| `OD-V1-18` vị trí resolve token → E.164 | T-04 |

13/13 phủ đủ. T-08 không gắn `OD-V1` nào vì nó là ticket process — nhưng nó sở hữu 11 test `CDC-*` rải khắp T-01…T-07, vốn trước đây không có chủ.

Mỗi ticket có đủ 9 mục DoD: current evidence, target delta, sample payload, acceptance test, owner, due, gate, mock fallback, closure artifact.

## 3. Cách gói này được dựng

Mọi mục "current evidence" đều **đọc từ nguồn trong repo tại thời điểm viết**, không chép lại từ tài liệu tóm tắt. Ví dụ các khẳng định đã được xác minh trực tiếp:

| Khẳng định | Cách xác minh |
| --- | --- |
| Không có JWT/JWKS/mTLS trong code | grep `Jwks`, `JwtBearer`, `JsonWebKey`, `IssuerSigningKey`, `ClientCertificate` trên `src/` và `tests/` → 0 kết quả |
| Ma trận program bị enforce ở 4 nơi | đọc OpenAPI `oneOf`, `TaskIntakeEndpoint.cs:209`, `EligibilityRules.cs:139`, CHECK constraint trong migration |
| `ivr_confirmation_required` không có nguồn business | `grep -rl` trên `docs/documents/` → 0 file |
| Delta attempt policy | đọc thẳng hai file business phase-8 và `CandidateAttemptPolicies` trong code, đối chiếu 4 tham số |
| Current GH callback khác Target V1 | đọc fixture compat đã ghim tại commit Sales `a3aad246…` và so từng field |
| Token so sánh hằng thời gian | đọc `OrderCoreAllowlistMiddleware.cs:55` — `CryptographicOperations.FixedTimeEquals` |

## 4. Phát hiện đáng chú ý trong lúc soạn

Ba thứ không có trong `specs/_review/open-decisions-register.md` và chỉ lộ ra khi đọc code:

1. **Ma trận lệch theo hai chiều, không phải một.** Register chỉ ghi `OD-V1-13` là "Golden Hour ONLINE có thuộc scope không". Đọc kỹ `DS-01` thì cặp giá trị `GOLDEN_HOUR + COD` cũng lệch — theo `DS-01` là callable, nhưng IVR từ chối `422`. Chiều này chưa được ghi ở đâu cả, và nó nguy hiểm hơn: một lớp đơn bị chặn im lặng.

2. **`eligibility_snapshot` bắt buộc nhưng không có type; `sellable_status[]` có type đầy đủ nhưng optional.** Ngược nhau. IVR chỉ kiểm `eligibility_snapshot` "là object".

3. **`order_state` khai là opaque, code so literal `"CONFIRMING"`.** Nếu Sales đổi tên state, IVR chặn toàn bộ task mới mà không có tín hiệu nào phía Sales.

Cả ba đã thành mục delta trong T-01 và T-02.

## 5. Cũng đã kiểm chứng: một nghi ngờ hoá ra sai

`seed/ivr-tasks.sample.json` có shape khác hẳn `IvrConfirmationTaskV1` — thiếu 12 field bắt buộc. Trông như doc drift. Nhưng `seed/README.md` đã ghi rõ file này là `LEGACY`, "KHÔNG phải `IvrConfirmationTaskV1`, KHÔNG đẩy vào `POST /tasks`", giữ để đọc lịch sử. Không phải finding; không ghi vào gói.

## 6. Kiểm chứng cơ học

| Lệnh | Kết quả |
| --- | --- |
| Kiểm link nội bộ toàn gói (10 file) | 0 link hỏng |
| `dotnet build Ivr.sln -warnaserror` | 0 warning / 0 error |
| `npm test` (admin-ui) | 17 file / 181 test pass |
| `sh deploy/ci/scripts/scan-pii.sh docs/evidence` | xem §8 |

Build và test chạy để xác nhận gói tài liệu này **không đụng gì tới code** — trạng thái trước và sau như nhau.

## 7. Cái này KHÔNG chứng minh

- **Không đóng `W-0002`…`W-0007`.** Cả sáu giữ `BLOCKED_EXTERNAL`. Gói này chỉ tạo lối đi để đóng.
- **Không phê duyệt** Golden Hour ONLINE, `ivr_confirmation_required`, attempt policy, whitelist lời thoại, dial-token semantics hay production auth profile.
- **Không gửi cho ai.** IVR soạn; owner IVR quyết định gửi.
- **Không sửa** business source, `specs/_review/`, `_archive/` hay `_legacy-mock/`.
- **Không có phản hồi nào từ Sales/Security/Privacy.** Mọi mục "closure artifact" còn trống.

## 8. Ghi chú về cổng PII

File evidence này nằm trong phạm vi quét của `scan-pii.sh`. Theo bài học `A-0190`, nội dung ở đây tránh các từ khoá địa chỉ mà pattern coi là PII, và không viết giá trị token dưới dạng `khoá: giá trị`. Bản thân các ticket nằm ở `docs/contracts/` — ngoài phạm vi quét — nên chúng được viết tự nhiên hơn, nhưng vẫn chỉ dùng giá trị mờ dạng `<...>` cho token và dải test cho số liên lạc.

**Bản nháp đầu dính gate, và lại là false positive cùng loại với `A-0190`.** Một danh từ chỉ đơn vị dân cư trong pattern địa chỉ cũng là nửa đầu của một từ ghép rất thông dụng nghĩa "combination" — mô tả đúng cái bảng 4 dòng ở T-01. Pattern khớp vì sau nó là dấu cách.

Xử lý giống `A-0190`: **đổi cách diễn đạt, không nới pattern.** `W-0076` chọn literal byte alternation để pattern độc lập locale; nới nó ra là đánh đổi một tính chất đã chứng minh lấy tiện lợi hình thức. Từ đã đổi ở cả evidence lẫn T-01 để gói không thành mìn nếu sau này phạm vi quét mở rộng sang `docs/contracts/`.

Đây là lần thứ hai cùng một lớp lỗi xuất hiện. Điểm chung: **pattern địa chỉ tiếng Việt bắt trúng những từ mà nghĩa hành chính chỉ là một trong nhiều nghĩa.** Ai viết tài liệu tiếng Việt trong `docs/evidence/` nên chạy `sh deploy/ci/scripts/scan-pii.sh docs/evidence` trước khi coi là xong — rẻ hơn nhiều so với để CI bắt.

## 9. Việc kế tiếp

| Việc | Ai | Ghi chú |
| --- | --- | --- |
| Quyết định gửi gói cho Sales/Product/Privacy/Security | **owner IVR** | IVR không tự gửi |
| `P11-1` / `W-0057` — gói telephony RFQ + 1 SIM lab | chưa làm | sở hữu `W-0008`; prompt ghi "start at project beginning" |
| `P11-3` / `W-0059` — gói legal/retention | chưa làm | prereq `W-0052`, `W-0053` (P10-1, P10-2) chưa chạy |
| `P4-2` → `P4-3` → `P4-4` → `P4-1` | có thể chạy song song | tất cả đều mock-only cho tới khi gói này có câu trả lời |
