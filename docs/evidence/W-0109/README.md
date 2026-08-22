# W-0109 — Vòng đời kịch bản: API, ràng buộc quyền và màn hình

Ngày: `2026-08-23`
Baseline: `main@790931f`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A2](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

> **Đây là đảo ngược một quyết định đã ghi, theo yêu cầu owner.** `W-0096` cố ý để màn
> `/config` read-only. Xem §1. `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.

---

## 1. Quyết định bị đảo, và vì sao

`AdminConfigReadService` ghi nguyên văn: *"Script lifecycle transitions stay in
`IScriptContentManager` and are not exposed: approval is an owner action governed by
`OD-V1-15`, not a console button."*

Bản rà soát ban đầu gọi hiện trạng là "cỗ máy lifecycle không có công tắc" — **đúng về hiện
trạng, sai về nguyên nhân**. Có lý do, và nó được viết ra.

Lý do đảo: lối duy nhất còn lại để một chữ ký Pháp chế vào hệ thống là **sửa tay dữ liệu**.
Sửa tay thì mất bản ghi audit, mất luật `creator ≠ approver`, và mất luôn ý nghĩa của chính
cái cổng mà chữ ký đó phục vụ. Mở qua khuôn admin mutation đặt chữ ký **trở lại bên trong**
các kiểm soát, thay vì bỏ nó ra ngoài.

Owner đã được báo trước khi code và trả lời tiếp tục.

---

## 2. `OD-SCRIPT-01` không cần quyết định mới

Kế hoạch xếp đây là câu hỏi chặn: Content approver và Privacy/Legal approver phải là hai người
khác nhau, nhưng hệ thống chỉ có 2 role. Đọc source cho thấy **câu trả lời đã nằm sẵn trong
code**: four-eyes ràng buộc theo `ActorId`, không theo role, và được thi hành ở **hai** chỗ độc
lập — `EnsureApprovalAllowed` lúc ghi, `ScriptApprovalPolicy.ProductionAllows` lúc đọc.

Không cần role thứ ba. Hệ quả vận hành phải nói thẳng, và §7 nói.

---

## 3. Phạm vi đã triển khai

### 3.1 Quyền

Bảy quyền console `IVR_SCRIPT_*`, ánh xạ 1-1 sang `ivr.script.*` của domain. Không gộp thành
một quyền "quản lý kịch bản": gộp lại thì không bao giờ tách được Pháp chế ra role riêng mà
không phải migration.

Cả bảy vào [`ConsoleSessionOnly`](../../../src/Ivr.Api/Auth/IvrPermissions.cs) cùng nhóm quyền
account, và bốn route mutation ghim vào console session scheme. Seam quyền MOCK
(`X-Permissions`) mint bất cứ quyền nào được yêu cầu, MOCK là chế độ mặc định, và một trong
các quyền này ký duyệt lời thoại đọc cho khách. Một header mint được
`IVR_SCRIPT_APPROVE_CONTENT` là một header duyệt được lời thoại production **không cần
credential nào**.

### 3.2 API — 5 operation

| Route | Quyền | Ghi chú |
| --- | --- | --- |
| `GET /scripts/{templateId}/{version}` | `IVR_QUEUE_VIEW` | mọi trạng thái, gồm bản nháp |
| `POST /scripts/` | `IVR_SCRIPT_EDIT` | version bất biến sau khi tạo |
| `POST /scripts/{…}:submit` | `IVR_SCRIPT_REVIEW` | |
| `POST /scripts/{…}:approve` | `IVR_SCRIPT_APPROVE_*` theo `approval_type` | |
| `POST /scripts/{…}:retire` | `IVR_SCRIPT_RETIRE` | không xoá |

[`ScriptLifecycleApiService`](../../../src/Ivr.Api/Application/ScriptLifecycleApiService.cs)
**không giữ luật nào của riêng nó**. Ai được duyệt, theo thứ tự nào, và version có nói được
hay không — tất cả ở lại trong `IScriptContentManager` và `ScriptApprovalPolicy`, cũng chính là
thứ worker hỏi lúc quay số. Một bản sao thứ hai của các luật đó ở tầng API sẽ là câu trả lời
thứ hai cho "kịch bản này đã duyệt chưa", và hai bản sẽ trôi khỏi nhau.

### 3.3 `403` với `409` — phân biệt *ai* với *trạng thái*

`ScriptApproverConflictException` (kế thừa `InvalidOperationException`, nên mọi `catch` cũ
không đổi hành vi) tách hai loại từ chối:

- **403** — sai người: là người tạo, hoặc là tài khoản đã ký nửa còn lại. Bấm lại không sửa được.
- **409** — sai trạng thái: duyệt bản nháp, trùng loại duyệt, thu hồi bản nháp.

Trả `409` cho vế đầu sẽ đẩy người vận hành đi bấm lại, trong khi việc cần làm là **đi tìm
đồng nghiệp**.

### 3.4 Gộp bản sao four-eyes

`EnsureApprovalAllowed` tồn tại **hai bản byte-identical** trong registry in-memory và registry
PostgreSQL. Gộp về `ScriptApprovalPolicy`. Hai bản của một luật quyết định ai được duyệt lời
thoại là một luật sẽ có ngày thi hành ở MOCK mà không thi hành ở production — và chênh lệch đó
chỉ lộ ra dưới dạng một approval lẽ ra không được phép.

### 3.5 Audit có before **và** after

Cả hai registry thêm `previous_status`. Một dòng audit nói version đang `APPROVED` không cho
biết cú bấm đó **là** cú duyệt hay là một no-op sau đó — mà "ai đưa nó ra khỏi review" mới là
câu một buổi soát sign-off thực sự hỏi.

Đồng thời sửa một bất nhất sẵn có: registry in-memory ghi `Draft`, PostgreSQL ghi `DRAFT`. Giờ
cả hai dùng dạng lưu trữ.

### 3.6 Màn hình

`/config` có action bar theo trạng thái, dùng lại `AdminActionDialog` (bắt buộc có `reason`,
ẩn nút theo quyền). Hai chốt cứng của `specs/ui/04` **không** đổi: không ô nào thêm biến ngoài
whitelist, không ô nào bật `KEY_9` — cả hai nằm trong `ValidateTemplate`, chạy server-side.

Nút bị ẩn theo trạng thái chỉ là tiện lợi. Server vẫn từ chối transition sai, nên một trang cũ
không thể duyệt một version đã thu hồi chỉ vì nó render trước lúc thu hồi.

### 3.7 Contract

OpenAPI `draft.12 → draft.13`: +5 path, +5 schema (`IvrScriptVersionDetail`,
`IvrScriptDraftRequest`, `IvrScriptTransitionRequest`, `IvrScriptApprovalRequest`,
`IvrScriptActionResult`), +2 parameter. Re-pin `contract-manifest.json`, sinh lại human diff và
portal API docs.

`IvrScriptApproval` và `IvrScriptVersion` **đã tồn tại** cho catalog; bản đầu của thay đổi này
định nghĩa trùng và YAML parser bắt được. Bản detail đổi tên thành `IvrScriptVersionDetail` —
projection khác, tên khác.

---

## 4. Đối chiếu tiêu chí nghiệm thu

| Tiêu chí (plan §A2) | Test | Kết quả |
| --- | --- | --- |
| Creator submit rồi tự approve ⇒ `403` | `IT-SCRIPT-FOUREYES-02` | ✅ |
| Content approver = Privacy/Legal approver ⇒ bị chặn | `IT-SCRIPT-FOUREYES-02` | ✅ `403` |
| Approve khi `ProductionTargetV1FieldsApproved=false` ⇒ **không** production-ready | `IT-SCRIPT-PRODGATE-03` | ✅ `production_blocked_reason` nêu đích danh `OD-V1-15` |
| Retired version ⇒ fail-closed mọi mode | `IT-SCRIPT-RETIRED-04` | ✅ + duyệt lại ⇒ `409` |
| Mọi action có bản ghi `ivr_audit_log` với before/after | `IT-SCRIPT-AUDIT-05` | ✅ đọc từ bảng thật, `DRAFT → IN_REVIEW` |

Thêm ngoài yêu cầu: `IT-SCRIPT-LIFECYCLE-01` (chuỗi chuyển trạng thái đầy đủ) và `IT-SCRIPT-OPERATOR-06`
(Operator bị từ chối cả bốn transition).

---

## 5. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **449 / 449** |
| `Ivr.IntegrationTests` | **234 / 234** (+6 mới) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **6 / 6** |
| **Tổng .NET** | **711 / 711** |
| admin-ui | lint + `tsc` + **210 / 210** + build |
| `dotnet build Ivr.sln` | 0 warning / 0 error |
| `dotnet format Ivr.sln` | PASS |
| Traceability | **427** tagged test |

### Guard của luồng khác bắt được lỗi của luồng này

`UT-L10N-COVER-03` (W-0107) đỏ vì các enum mới trong OpenAPI chưa có nhãn tiếng Việt. Đây là
lỗi thật, không phải phiền toái: `Enum.ToString()` cho `Approved`/`Mock`/`MockTest`, còn từ điển
dùng `APPROVED`/`MOCK`/`MOCK_TEST`, nên console sẽ hiện huy hiệu cảnh báo cạnh một mã lạ thay vì
nhãn. Đã sửa bằng bảng dạng wire tường minh trong service, và bốn họ enum được nối vào bảng phủ.

---

## 6. Ba test bị viết lại, và vì sao

Không test nào bị nới lỏng; cả ba đổi sang khẳng định thứ mạnh hơn hoặc chính xác hơn.

| Test | Trước | Sau |
| --- | --- | --- |
| `IT-ADMIN-CONFIG-05` | `POST /scripts` ⇒ `405` (không có mutation surface) | `/integration-status` và `/review-items` vẫn `405`; `/scripts` ⇒ **`401` qua seam MOCK** — mạnh hơn `405` vì nó chứng minh seam không chạm tới được |
| `UT-UI-SCRIPT-01` | copy nói "không có nút" | copy phải nêu **lý do**, **nhật ký**, và **hai tài khoản khác nhau**; và mỗi nửa duyệt là một nhãn riêng, cấm nhãn `config.approve` gộp |
| `UT-SCRIPT-LIFECYCLE-02` | `Assert.ThrowsAsync<InvalidOperationException>` | `ScriptApproverConflictException` — kiểu cụ thể, để phân biệt 403/409 không thể bị xoá mà không có test nào đỏ |

---

## 7. Những gì bản này **không** chứng minh

- **Ma trận role không phân biệt được Pháp chế với Admin.** Cả bảy quyền nằm trên `Admin`. Kiểm
  soát còn hiệu lực là theo **tài khoản**, và hệ quả là một sign-off production cần **ba tài
  khoản khác nhau** (người tạo + hai người duyệt). Deployment ít hơn thế **không đạt** được
  approval production — fail-closed đúng, không phải lỗi. Muốn phân biệt ở mức role thì cần
  role thứ ba, là work item khác.
- **Không đóng `G-LEGAL`.** Bản này dựng **lối vào** cho chữ ký. Chữ ký thật vẫn do Legal/Privacy
  ký, và `OD-V1-15` vẫn mở — `IT-SCRIPT-PRODGATE-03` chứng minh console không nói vòng qua được.
- **Chưa chụp ảnh màn hình.** Cần stack có dữ liệu.
- **Chưa có idempotency key trên 4 route mới.** Các admin mutation khác đòi `Idempotency-Key`;
  các transition này dựa vào tính bất biến của chính lifecycle (duyệt trùng loại ⇒ `409`) chứ
  chưa qua `IIdempotencyStore`. Ghi ở đây thay vì để người sau đọc bảng §3.2 rồi tưởng là có.

---

## 8. Lệnh tái lập

```bash
dotnet build Ivr.sln --nologo
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj --nologo
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj --nologo
dotnet test tests/Ivr.ContractTests/Ivr.ContractTests.csproj --nologo
node deploy/ci/scripts/openapi-contract-drift.mjs
node deploy/ci/scripts/docs-selftest.mjs
npm --prefix admin-ui run lint && npm --prefix admin-ui test
```
