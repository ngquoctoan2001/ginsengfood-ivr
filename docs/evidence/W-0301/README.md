# W-0301 — `RUNTIME_GATE_ADMIN` phải nêu môi trường nó mở

Ngày 16/09/2026. Baseline `641d294`. `REAL_CUSTOMER_CALL_ALLOWED=NO`, không đổi wire, không phát
hành contract.

Nguồn: bản giao việc 16/09 mục `B4`.

## Vấn đề

`W0195` cho bảng `ivr_runtime_gate_approvals` một cột `environment`, rồi seed một hàng
`RUNTIME_GATE_ADMIN` với cột đó **`NULL`**, approver `'ivr-owner'`.
`PostgresRuntimeGateAuthorization` chỉ hỏi *"có hàng admin nào còn sống không"*, nên **một hàng đó
mở quản trị runtime-gate ở mọi môi trường chạm tới database này** — trong khi cột có thể nói khác
thì nằm đó không ai đọc.

## Tôi tự lật lập luận của chính mình

Vị trí cũ, viết trong `RuntimeGateApprovalKinds`, là *"administration is coarse on purpose"* —
quyết định theo môi trường nằm ở hàng four-eyes của từng thay đổi. Lập luận đó **đọc thì xuôi và sai
ở đúng một chỗ**: một người duyệt điền `environment = 'lab'` sẽ tin rằng mình đã giới hạn, và thực
tế đã mở luôn production.

**Một cột có người điền mà không ai đọc thì tệ hơn không có cột** — nó mời gọi một lời hứa hệ thống
chưa từng đưa ra.

`SIP-04` đã sửa đúng lớp lỗi này cho `PRODUCTION_CALL`. Áp luật cho một kind mà không cho kind kia
để lại nửa yếu hơn canh đúng cái kind mở **mọi** thay đổi feature flag.

## Ràng buộc schema quyết định hình dạng bản sửa

`trg_ivr_runtime_gate_approvals_append_only` từ chối **cả hai** lối:

```text
OLD.environment IS DISTINCT FROM NEW.environment  →  'only revocation may change'
TG_OP = 'DELETE'                                  →  'append-only; revoke instead of deleting'
```

⇒ Hàng seed **chỉ revoke được**. Đó chính là ý nghĩa của một sổ phê duyệt append-only, nên bản sửa
tôn trọng nó: **revoke hàng seed, không seed thay thế**. Quyền quay lại khi có người chèn hàng scoped
qua thủ tục mang `approval_reference` thật — đúng hình dạng `PRODUCTION_CALL` đang dùng.

### `CHECK` phải khác `PRODUCTION_CALL` một chỗ, và đây là lý do

```sql
CHECK (approval_kind <> 'RUNTIME_GATE_ADMIN'
       OR environment IS NOT NULL
       OR revoked_at IS NOT NULL)
```

Vế `revoked_at IS NOT NULL` là **bắt buộc chứ không phải nới lỏng**: hàng vừa revoke ở trên giữ
`environment = NULL` **vĩnh viễn**, vì trigger không cho ai đổi. Miễn trừ hàng đã revoke không mất gì
— một phê duyệt đã revoke không mở môi trường nào dù cột nó ghi gì — và bất biến cần giữ thì nguyên
vẹn: *một phê duyệt admin **còn sống** phải nêu môi trường*.

## Hệ quả vận hành — nói rõ

**Quản trị runtime-gate nay đóng ở mọi môi trường** cho tới khi có hàng scoped. Mọi thay đổi feature
flag **tăng rủi ro** sẽ bị từ chối ở mọi nơi.

Đó là chiều đúng và an toàn: `FeatureFlagAdminService` vốn cho phép **giảm rủi ro vô điều kiện** mà
không hỏi cổng này — nên kill switch vẫn bật được và vẫn dừng được cuộc gọi **mà không cần phê duyệt
nào**.

## Đã làm

| File | Thay đổi |
| --- | --- |
| `FeatureFlagContracts.cs` | `IRuntimeGateAuthorization.IsApprovedAsync` nhận `environment` |
| `RuntimeGateApprovals.cs` | `PostgresRuntimeGateAuthorization` → `AnyLiveForEnvironmentAsync`; viết lại doc-comment đang nói ngược |
| `RuntimeGateDefaults.cs` | `PendingRuntimeGateAuthorization` theo chữ ký mới |
| `FeatureFlagAdminService.cs` | Truyền `command.Environment` — giá trị đã có sẵn tại chỗ gọi |
| `Migrations/20260916073006_…` | Revoke hàng seed + `CHECK` |
| 2 test double + `RuntimeGateApprovalTests.cs` | Chữ ký mới; viết lại `IT-GATE-APPROVAL-01`, `-03`, `-10`; thêm `-13` |

## Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **1066/1066** pass (24 + 717 + 8 + 317) |
| `gitnexus_impact` trên `IRuntimeGateAuthorization` | **HIGH** — 20 điểm chạm, 4 implementation, 13 test. Đã báo owner trước khi sửa theo `CLAUDE.md` |
| Nhóm `IT-GATE-APPROVAL-*` | **13/13** |
| Mutation: trả reader về `AnyLiveAsync` | `IT-GATE-APPROVAL-10` **ĐỎ** |
| Khôi phục | **XANH** |
| `progressive-selftest`, `migration-expand-guard`, `docs-selftest`, `ci-config-selftest` | PASS |

> ⚠️ **Ghi trung thực:** một lượt chạy trọn bộ có **1 test integration đỏ**, và nó **không tái hiện**
> ở lượt chạy sạch ngay sau (`317/317`). Tôi không xác định được ca nào vì lượt sau xanh. Nghi là
> nhiễu container/timing, **chưa chứng minh được**.

### Test viết lại

| Test | Trước | Sau |
| --- | --- | --- |
| `IT-GATE-APPROVAL-01` | *"administration is approved because the signature is a row"* | Schema ship **không cấp ở môi trường nào**; quét cả `5` môi trường, không nêu tên một cái |
| `IT-GATE-APPROVAL-03` | Revoke hàng seed | Cấp hàng scoped trước rồi revoke — nếu không thì chỉ chứng minh cổng đóng vẫn đóng |
| `IT-GATE-APPROVAL-10` | Khẳng định cột *"recorded, never read"* và ai điền `lab` là **nhầm** | Cột **được đọc**; grant ở `lab` không mở `prod`/`staging` |
| `IT-GATE-APPROVAL-13` *(mới)* | — | Database từ chối một phê duyệt admin **còn sống** không nêu môi trường |

## Còn lại

`OD-V1-20` vẫn chưa có người ngoài M8 xác nhận. Lượt này không thay cho chữ ký đó; nó chỉ làm cho
việc thiếu chữ ký **không còn tự cấp quyền quản trị cho mọi môi trường**.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0301 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm:
RUNTIME_GATE_ADMIN phải nêu môi trường. Claude ghi nhận quyết định của owner, không tự cấp phê
duyệt.

Còn mở, không thuộc nghiệm thu này: OD-V1-20 chờ người ngoài M8 xác nhận. Mọi giới hạn trong cột
Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test,
sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
