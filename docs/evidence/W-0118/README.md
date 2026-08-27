# W-0118 — Không gọi IVR cho khách cũ (`OD-15`)

Ngày: `2026-08-25`

Baseline: `main@ee31c76`

Trạng thái: `TESTS_PASS`

## 1. Kết quả

- Owner chốt `OD-15`: **không gọi IVR cho khách cũ**. Quyết định supersede `OD-08` và phần trust-score của `D-12`.
- Điều kiện skip nay đọc **risk evidence**, không đọc trust score: `trust.risk_evidence_available=true` **và** `risk_flags` rỗng.
- Nghĩa vụ còn lại của Sales rút từ *"build một scoring engine"* xuống **đúng một field**.
- `trusted_skip_allowed` đổi từ opt-in bắt buộc sang **veto** (`bool` → `bool?`).
- Cờ chính sách `IVR_RETURNING_CUSTOMER_SKIP_ENABLED` mặc định **ON**, đặt `NO` để rollback không cần redeploy.
- Contract `1.0.0-draft.18 → draft.19`; shape không đổi, `oasdiff` no breaking.
- **Hành vi runtime chưa đổi** — xem §7.

## 2. Vì sao bỏ trust score khỏi predicate

Nhánh skip đã tồn tại đầy đủ từ `P4-3`: `TrustResolverEvidence`, decision `TASK_SKIPPED_TRUSTED_CUSTOMER`, DB check constraint, workflow spec. Nhưng nó **chưa từng chạy một lần nào trên dữ liệu thật**, vì hai lý do chồng lên nhau:

1. `EligibilityService` khoá cứng `const bool skipFeatureEnabled = false`.
2. Kể cả khi bật, predicate đòi `customer_trust_status=TRUSTED` + `resolver_available` — tức là đòi `CustomerTrustResolver`, thứ mà `DC-06` ghi rõ là **out-of-scope P3.2, chưa build**.

Rà source cho thấy điều kiện thứ hai là **thừa**: khách mới đã tự mang cờ `NEW_CUSTOMER` / `VERIFIED_ORDER_COUNT_0` trong `risk_flags` cấp task, đúng như `COD_FAIL_HISTORY` hay `SUSPICIOUS_DUPLICATE` (khớp `seed/customers.sample.json`). Một phép kiểm "list rỗng" vì vậy trả lời **cả hai vế** — *có phải khách cũ không* và *đơn có bất thường không* — mà không cần signal thứ hai nào từ Sales.

`trust.resolver_version` fallback về `source_version` cấp snapshot, y hệt cách `voice_restriction.source_version` đã làm từ `P4-3`. Vì `source_version` vốn đã bắt buộc ở tầng luật chạy trước, phần Sales còn phải thêm chỉ còn `trust.risk_evidence_available`.

## 3. Ranh giới phải giữ — và nó nằm ở đâu trong code

`risk_flags` rỗng có **hai** nguyên nhân không phân biệt được khi nhìn dữ liệu tĩnh:

| Nguyên nhân | Được phép skip? |
| --- | --- |
| Sales đã đánh giá và không thấy gì | ✅ |
| Sales chưa đánh giá bao giờ | ❌ **phải gọi** |

`risk_evidence_available` là thứ **duy nhất** tách được hai trường hợp đó. Nếu gộp, đúng những đơn Sales chưa kịp đánh giá sẽ bị bỏ qua xác minh — tức là đơn ảo lọt lưới, đúng thứ module này tồn tại để chặn.

Ranh giới này nằm ở `TrustResolverEvidence.CanSkip` ([`EligibilityRules.cs`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs)) và được khoá bằng `UT-ELIG-TRUST-18`.

Bất đối xứng với `voice_restriction` **giữ nguyên**: thiếu bằng chứng do-not-call thì **chặn gọi**; thiếu bằng chứng risk thì **vẫn gọi**. Cả hai đều fail-closed, đóng ngược chiều nhau vì thiệt hại khác nhau — một bên là cuộc gọi tới người đã từ chối, bên kia là đơn ảo không được xác minh.

## 4. Impact trước sửa

GitNexus index ở `main@ee31c76`, up-to-date.

| Ký hiệu | Risk | Liên đới |
| --- | --- | --- |
| `TrustResolverEvidence` (constructor) | **LOW** | 20 ký hiệu, 3 direct |
| `TrustResolverEvidence` (record) | **LOW** | 0 |

`detect_changes` sau khi sửa: **risk LOW, 0 affected process**, 22 file — toàn bộ nằm trong phạm vi eligibility + test + tài liệu, không ký hiệu nào ngoài phạm vi bị kéo theo.

## 5. Ba phép đo, không phải ba khẳng định

**(1) Bật cờ chính sách không skip ai — và test chứng minh điều đó.**
Toàn bộ 258 integration test vẫn đi qua nhánh gọi sau khi bật cờ, vì fixture không set `trust.risk_evidence_available`. Đây là tính chất cần giữ chứ không phải may mắn, nên `SkipPolicyOn` trong `EligibilityPersistenceTests` được nối bằng `IvrOptions` **mặc định thật** thay vì một stub tắt. Nếu ai đó sau này làm skip "rò rỉ" khi thiếu evidence, suite sẽ đỏ.

**(2) `IT-ELIG-TRUST-15` chạy ba nhánh trên cùng một payload.**
`risk_flags: []` **có** và **không có** risk evidence cho hai kết quả khác nhau; hàng thứ ba là khách mới với `NEW_CUSTOMER`. Nếu ba hàng này cho cùng một kết quả thì ranh giới §3 đã mất, và test bắt được.

**(3) Test cũ phải đỏ — và nó đã đỏ.**
`TrustResolverEvidenceNeverProducesASkipWhileTheFeatureStaysOwnerGated` mã hoá đúng chính sách bị thay, nên nó đỏ ngay lần chạy đầu sau khi sửa predicate. Đó là bằng chứng cổng cũ không rỗng. Đã viết lại thành `IT-ELIG-TRUST-14`, chứng minh chiều ngược lại **và** khẳng định `CallAttempts.CountAsync() == 0`.

**Bẫy tìm thấy khi viết test:** default `trustedSkipAllowed = false` trong hai test helper nay mang nghĩa **veto**. Để nguyên thì mọi test feature-on viết sau này sẽ bị veto ngầm và tác giả không hiểu vì sao không skip. Đã đổi cả hai sang `bool?` = `null`.

## 6. Đồng bộ nguồn sự thật

| Tài liệu | Trạng thái trước | Sau `W-0118` |
| --- | --- | --- |
| `specs/database/02-tables.md` | 🔴 `trusted_skip_allowed: null ⇒ false` — **sai** sau `OD-15` | `null` ⇒ không veto; ghi rõ khác `D-12` cũ |
| `specs/workflows/07-trusted-skip.md` | mô tả trust resolver, gắn nhãn "future/feature-gated" | viết lại theo `OD-15`, kèm bảng advisory và cách đo gap |
| `specs/04-glossary.md` | "Trusted skip … theo Customer Trust Resolver" | định nghĩa theo risk evidence |
| `specs/api/06-error-codes.md` | không có advisory code | thêm §2b — bảng 7 advisory |
| `specs/functional/02` `FR-IVR-ELIG-007` | "trust skip disabled unless …" | điều kiện `OD-15` |
| `specs/workflows/00-index.md`, `functional/00-index.md` | nhãn cũ | nhãn `OD-15` |
| `specs/api/evidence/eligibility-snapshot.v1.schema.json` | `trust` mô tả "skip stays DISABLED" | mô tả `OD-15`, thêm ví dụ payload skip |
| `seed/ivr-tasks.sample.json` | `_note` gắn `D-12` | `OD-15` + cảnh báo veto ở cấp file |
| OpenAPI 3 field + enum `decision` | không có `description` | có, `draft.19` |

Tài liệu bàn giao mới cho Module 3: [`integration-requirements/06-module-3-api-handover.md`](../../../integration-requirements/06-module-3-api-handover.md).

## 7. ⚠️ Hành vi runtime **chưa** đổi

Bật cờ chính sách không tự nó skip ai. Chừng nào Module 3 chưa gửi `trust.risk_evidence_available`, **mọi task đủ điều kiện vẫn được gọi** và mang advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE`.

Đó là thiết kế, không phải thiếu sót — và nó cho một cách đo tiến độ không cần ai báo cáo: **khi advisory này biến mất khỏi log eligibility, Sales đã bật field và skip đang chạy.**

## 8. Gate evidence

| Gate | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln` | PASS — 0 warning, 0 error |
| `dotnet test Ivr.sln` | PASS — **774/774** (unit `486`, integration `258`, contract `22`, chaos `8`) |
| Test mới | `UT-ELIG-TRUST-18/19`, `IT-ELIG-TRUST-14/15`; `UT-ELIG-TRUST-16` viết lại |
| `openapi:lint` (redocly) | PASS — 2 tài liệu, 0 lỗi |
| `openapi:validate` | PASS — target tasks 9; negative 12 + domain negative 13 |
| NSwag codegen | PASS — `OPENAPI_CODEGEN_COMPLETE=YES`; generated DTO **chỉ +12 dòng XML doc** |
| reviewed draft + drift | PASS — `OPENAPI_HASHES_PINNED=3`; `OPENAPI_HUMAN_DIFF_CURRENT=YES` |
| pinned `oasdiff v1.26.1@sha256:aae8…` | PASS — **no breaking changes**; operation `49` và schema `93` không đổi |
| portal/docs | PASS — 12 artifact; `API_DOCS_SELFTEST_PASS` |
| `ci-config-selftest` | PASS — `OPENAPI_CODEGEN_GATE_PASS` |
| traceability | PASS — `474` |
| GitNexus `detect_changes` | risk **LOW**, 0 affected process |

## 9. Residual — chưa đóng

| Mục | Trạng thái |
| --- | --- |
| Sales gửi `trust.risk_evidence_available` | `BLOCKED_EXTERNAL` — Module 3, xem [tài liệu bàn giao §6](../../../integration-requirements/06-module-3-api-handover.md) |
| Module 3 xác nhận **không** gửi `trusted_skip_allowed=false` mặc định | `OWNER_DATA_REQUIRED` — nếu họ đang gửi, **không đơn nào skip được** |
| Xác nhận tên mã `risk_flags` cuối cùng | `OWNER_DATA_REQUIRED` — `OD-15` dựa hoàn toàn vào tính đầy đủ của list này |
| Hosted GitLab CI | `NOT_RUN` — **mọi gate ở §8 chỉ chạy local**. Nguyên nhân (`remote.origin.pushurl` trỏ GitHub trong khi `remote.origin.url` fetch từ GitLab, nên GitLab không nhận commit) **đã sửa `2026-08-27` ở `W-0121`**; verdict vẫn `NOT_RUN` cho tới khi có một lượt push thật và pipeline chạy xanh |
| Owner/reviewer sign-off | Giữ `TESTS_PASS`, **không** `ACCEPTED` |
| Real call / SIM / carrier evidence | `NOT_RUN`; `REAL_CUSTOMER_CALL_ALLOWED=NO` |
