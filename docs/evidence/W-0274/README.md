# W-0274 — Sửa spec cho khớp code, và luật hoá ra cũng sai chứ không chỉ giá trị

Ngày: 2026-09-10 · Baseline: `main@fad7e87` · Trạng thái: **TESTS_PASS**.

Câu hỏi mở do `W-0272` sinh ra. Owner chốt ngày 2026-09-10: **sửa spec cho khớp code.**

## 1. Điều đáng chú ý: không chỉ danh sách giá trị sai

`W-0272` phát hiện spec khai `adapter_mode (MOCK/REAL)` trong khi code nhận
`MOCK/VENDOR/ASTERISK_ARI`. Cách sửa hiển nhiên là thay danh sách. **Làm vậy vẫn còn sai.**

Đọc code chặn enable (`InternalAdminApiService.cs:759`):

```csharp
if (!string.Equals(channel.AdapterMode, SimAdapters.Mock, StringComparison.Ordinal)
    || channel.Status is "QUARANTINED" or "HEALTH_FAILED"
    ...
```

Điều kiện là **`!= MOCK`** — fail-closed trên **mọi** giá trị khác, kể cả giá trị chưa tồn tại. Spec
viết *"Enable bị chặn khi `adapter_mode=REAL`"*, tức nêu **một giá trị cụ thể**.

Nếu tôi chỉ thay danh sách, spec sẽ thành *"chặn khi `=VENDOR` hoặc `=ASTERISK_ARI`"* — vẫn là liệt
kê, vẫn hỏng khi thêm adapter thứ tư, và vẫn mô tả sai cơ chế. Câu đúng là **"khác `MOCK`"**.

Nên spec giờ nói **luật**, không nói danh sách.

## 2. Tám chỗ, năm file

| File | Sửa gì |
| --- | --- |
| `specs/database/02-tables.md:174` | `(MOCK/REAL — DT-01)` → `(MOCK/VENDOR/ASTERISK_ARI — DT-01)` |
| `specs/database/06-migration-plan.md:26` | `adapter_mode=REAL` → `adapter_mode` khác `MOCK`, kèm giải thích `VENDOR` là SIM nhà cung cấp và `ASTERISK_ARI` là adapter lab `W-0104` |
| `specs/ui/01-dashboard.md:10` | bộ lọc `adapter_mode(MOCK/REAL)` → ba giá trị |
| `specs/ui/07-seed-mock-management.md` | 3 chỗ — danh sách giá trị, dòng mô tả màn hình, và ô quyền |
| `prompt/phase-2-core-runtime/P2-8-internal-admin-api.md` | 2 chỗ — luật chặn enable (§49) và mô tả `IT-API-SIM-09` (§78) |

## 3. Vì sao `specs/ui/` là phần quan trọng nhất

`admin-ui` đã bị xoá ở `W-0253`; **Module 3 dựng console**. Nghĩa là `specs/ui/*` không phải tài
liệu nội bộ — nó là **thứ Module 3 đọc để build**.

Ba chỗ vừa sửa đều nằm trên đường đó: bộ lọc dashboard, màn hình đổi adapter, và bảng phân quyền.
Để nguyên thì Module 3 dựng một **toggle hai trạng thái MOCK/REAL** cho một cột có **ba** giá trị, và
dựng một ô khoá theo tên `REAL` cho một luật thật ra là "khác MOCK".

Đây là sửa spec, nhưng hệ quả nằm ở phía Module 3.

## 4. Cố ý không đụng

**OpenAPI**: `specs/api/openapi/ivr-order-confirmation.v1.yaml` khai `adapter_mode: { type: string }`
— **không có `enum`**, nên nó không tuyên bố từ vựng nào và không mâu thuẫn với code. Thêm `enum` vào
đó là **đổi hợp đồng** với Module 3, không phải sửa spec cho khớp code; và file này đang là vùng
agent khác sửa dở (`draft.25`). Ghi lại là một lựa chọn còn mở, không tự làm.

**Baseline `draft.20`…`draft.25`**: bản đóng băng của hợp đồng đã phát hành — đúng như tên gọi, sai
lệch so với hiện tại là thiết kế.

**`specs/ui/05-integration-status.md:12`** (`SIM Gateway: MOCK_up / down`): đây là mô tả trạng thái
hiển thị, không phải tuyên bố từ vựng cột. Không sửa.

## 5. Kết quả

Không chạm mã nguồn. Không file nào vừa sửa bị pin — **không có cascade**.

Gate sweep: **37/39 run**, 2 FAIL — cả hai là pin `06-module-3-api-handover.md` của agent khác, đỏ
từ trước lượt này. `docs-selftest` xanh.

## 6. Còn lại

Câu hỏi `adapter_mode` đã đóng. Còn mở, sinh ra từ chính lượt này:

- **Có nên biến `adapter_mode` thành `enum` trong OpenAPI không?** Hiện là `string` tự do, nên hợp
  đồng không ràng buộc gì. Đóng lại được nhưng là breaking change với Module 3 — quyết định của
  owner cùng producer, và nên chờ agent kia commit xong `draft.25`.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
