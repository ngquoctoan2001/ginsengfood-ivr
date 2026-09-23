# W-0302 — Token hết hạn muộn hơn cửa sổ: `422` thay vì `500`

Ngày 16/09/2026. Baseline `2fb76fe`. OpenAPI `1.0.0-draft.27` → `1.0.0-draft.28`, **không breaking**.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

Nguồn: bản giao việc 16/09 mục `C6`, nửa sau. Phát hiện đúng — đây là lỗi thật trong code tôi viết.

## Vấn đề

`OD-V1-17` chốt `2026-09-09`: `dial_token_expires_at` **bằng đúng** `confirmation_window_expires_at`.
Cổng contact chỉ cưỡng chế **một** phía:

| Payload | Trước `W-0302` |
| --- | --- |
| Token hết hạn **sớm** hơn window end | `422 IVR_CONTACT_INVALID` + `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` ✅ |
| Token hết hạn **muộn** hơn window end | Qua cổng contact → `PersistenceInvariantValidator` ném `InvalidOperationException` trong transaction → `ErrorEnvelopeMiddleware` bắt `Exception` → **`500 IVR_INTERNAL_ERROR`** ❌ |

**`500` là mã producer được phép retry.** Nên Module 3 sẽ retry mãi một payload không bao giờ hợp lệ.

Test `IT-INTAKE-DB-03` **ghim đúng khuyết tật này bằng chữ**: *"The contact gate lets it through,
then the persistence invariant rejects it inside the transaction — which is why that clause was
replaced rather than implemented."*

## Đã làm

Đưa kiểm `> window.ExpiresAt` **lên intake**, cạnh kiểm `< window.ExpiresAt`. Hai luật cùng nói
`==`, nhưng **giữ hai reason code riêng** thay vì một `DIAL_TOKEN_EXPIRY_MISMATCH` — để câu trả lời
nêu **chiều nào** producer làm sai, khỏi phải tự đi so hai mốc thời gian.

**Guard ở persistence giữ nguyên.** Nó là lớp cuối chứ không phải lớp đầu; một guard chỉ nổ khi
thượng nguồn đã hỏng thì đáng giữ **chính vì** nó không bao giờ nên nổ.

| File | Thay đổi |
| --- | --- |
| `EligibilityRules.cs` | Thêm `DialTokenExpiresAfterWindow` |
| `TaskIntakeService.cs` | Guard mới cạnh guard cũ |
| `specs/api/openapi/ivr-order-confirmation.v1.yaml` | `draft.28` + mô tả ràng buộc bằng nhau trên `dial_token_expires_at` |
| `tests` | `IT-INTAKE-DB-03` (`+60s` nay `422`), `UT-INTAKE-REASON-TAXONOMY-13` (thêm ca), `CT-M3-AUTHORITY-08` (ghim version) |

### Vì sao ràng buộc nằm ở `description` chứ không ở schema

JSON Schema **không so sánh được hai giá trị anh em**. Đây là luật runtime; `description` là chỗ
duy nhất trên wire ghi lại được nó. `oasdiff` vì thế báo *"No changes to report, but the specs are
different"* — đúng, vì không có thay đổi cấu trúc nào.

## Phát hành contract — `5` pin đã dời

`sha256` từ `7739c425…` → `453bd331…`:

| # | Nơi ghim | Cách cập nhật |
| --- | --- | --- |
| 1 | `specs/api/openapi/contract-manifest.json` | `contract-freeze-verifier --write` |
| 2 | `deploy/ci/scripts/dial-token-production-bundle-validator.mjs` | sed |
| 3 | `docs/api/portal-manifest.json` | `build-api-docs.mjs` (**sinh**, không sửa tay) |
| 4 | `docs/contracts/openapi-contract-diff.md` | `openapi-contract-drift --accept-reviewed-draft` |
| 5 | `docs/contracts/target-v1-field-inventory.md` | `contract-freeze-verifier --write` |

Kèm: baseline `…draft.28.yaml`, changelog `27→28`, `docs/api-changelog.md`, và `6` con trỏ version
trong `IR-06`.

> **Baseline so sánh CI giữ `draft.27` có chủ đích.** Nếu dời lên `draft.28` thì `oasdiff` so
> `draft.28` với chính nó và lượt sau không còn gì để so. Bảng trong `api-changelog.md` vì thế đọc
> `Baseline draft.27 | Current draft.28`; xoay baseline là một lượt `chore` riêng, theo khuôn
> `W-0202`.

## Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **1105/1105** (24 contract + 756 unit + 8 chaos + 317 integration) |
| `oasdiff breaking` (image ghim `tufin/oasdiff:v1.26.1`) | **No breaking changes**, exit `0` |
| `gitnexus_impact` trên `ContactRejectionReason` | **HIGH** — 23 điểm chạm, 4 execution flow, 21 test. Đã báo owner |
| Mutation: gỡ guard mới | `UT-INTAKE-REASON-TAXONOMY-13` **ĐỎ** |
| Khôi phục | **XANH** |
| `openapi-contract-drift`, `contract-freeze-verifier`, `contract-freeze-selftest`, `ci-config-selftest`, `docs-selftest`, `validate-openapi` | PASS |

## Bài học — và một khuyết tật tôi tự gây ra ở lượt trước

`generate-test-traceability.mjs` đọc **cây làm việc**, không đọc `HEAD`.

Ở `W-0301` (`1651e8f`) tôi chạy nó trong lúc một phiên khác đang giữ `SipTrunkProductionDialTests.cs`
**chưa track**. Kết quả: commit của tôi mang `16` hàng `UT-TRUNK` trỏ tới một file `HEAD` **không
có** — `FailGateTests.TheTraceabilityTableMatchesTheSuiteItClaimsToDescribe` sẽ đỏ trên một checkout
sạch. Phiên kia phát hiện và vá ở `a2808ce`; tôi đã tự kiểm lại bằng `git show` và `git cat-file`
chứ không nhận nguyên lời.

Luật cũ của tôi — *"sinh chứ không gõ tay"* — vẫn đúng nhưng **chưa đủ**:

> **Sinh lại một file dẫn xuất chỉ an toàn khi cây nó đọc khớp với cây sắp được commit.**

Nay `--check` trả `695` và test khớp, vì test của phiên kia đã vào `HEAD`.

## Còn lại

Chốt `DTK-02`/`DTK-06` với Module 3 vẫn chờ phần đơn hàng bên đó. Nửa lỗi `500` thì không chờ ai, và
đã xong.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0302 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Token hết hạn
muộn hơn cửa sổ trả 422; draft.28. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: DTK-02, DTK-06 chờ phần đơn hàng của M3. Mọi giới hạn trong cột
Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test,
sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
