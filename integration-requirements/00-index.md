# Integration Requirements — Index (IVR Order Confirmation)

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-27`

## Mục đích

Contract pack gửi các team ngoài để IVR (.NET, Module 8) tích hợp được. Mỗi file một owner.

## Ai sở hữu cái gì

| File | Owner thật | Nội dung | Trạng thái |
| --- | --- | --- | --- |
| **[06-module-3-api-handover.md](06-module-3-api-handover.md)** | **Module 3** — `ginsengfood-business-platform` | 📤 **Tài liệu bàn giao, gửi thẳng cho Module 3.** Hai chiều push, 22 field bắt buộc, payload mẫu, ACK taxonomy, checklist ký | `TARGET_V1_DRAFT` |
| [01-sales-platform-requirements.md](01-sales-platform-requirements.md) | Module 3 | **Sổ đăng ký ID** `IR-SALES-*` — mã ổn định để file khác trích dẫn. Nội dung chi tiết ở IR-06 | `TARGET_V1_DRAFT` |
| [02-ops-core-requirements.md](02-ops-core-requirements.md) | — | **`SUPERSEDED`** bởi `OD-17`: IVR không còn yêu cầu gì từ ops-core | `SUPERSEDED` |
| [03-telephony-sim-requirements.md](03-telephony-sim-requirements.md) | Telephony / Infra | mock → 1 SIM lab → 32 eSIM target | `TARGET_V1_DRAFT` |
| [04-shared-auth-audit-requirements.md](04-shared-auth-audit-requirements.md) | Security / Platform / Legal | auth, RBAC, audit, retention, release gate | `TARGET_V1_DRAFT` |
| [05-open-contract-questions.md](05-open-contract-questions.md) | nhiều owner | Câu hỏi **chưa có lời đáp**, kèm evidence để đóng | `OPEN` |

> **Chỉ có hai owner ngoài Module 3:** Telephony/Infra (IR-03) và Security/Platform/Legal (IR-04). Mọi thứ khác — Order Core, Sales Extensions, CRM/Customer Identity — đều nằm trong **cùng một repository `ginsengfood-business-platform`**. Xem [IR-06 §0](06-module-3-api-handover.md) về cách đánh số cũ "Module 3 / 3.1".

## Quy tắc ưu tiên khi hai file mâu thuẫn

1. `plan/ivr-orther/decisions-log.md` (`TV1-*`, `OD-*`, `D-*`) — **cao nhất**
2. `specs/api/openapi/ivr-order-confirmation.v1.yaml` — hợp đồng máy đọc được
3. **`06-module-3-api-handover.md`** — authority cho mọi thứ Module 3 nợ IVR
4. `01`, `03`, `04`, `05` — sổ đăng ký ID và câu hỏi mở

Nếu `01` và `06` lệch nhau, **`06` thắng** và `01` phải sửa. `01` tồn tại vì mã `IR-SALES-*` được trích dẫn từ `seed/README.md` và `specs/**`; xoá nó sẽ làm gãy các trích dẫn đó.

## Từ vựng trạng thái

Chỉ dùng đúng bảy nhãn này. Nhãn lạ (`REQUIREMENTS_DRAFT`, `NOT_IMPLEMENTED_UPSTREAM`…) đã bị chuẩn hoá về đây.

| Nhãn | Nghĩa |
| --- | --- |
| `TARGET_V1_DRAFT` | Hai bên build song song được, nhưng chưa khoá |
| `CURRENT_COMPAT` | API/nguồn hiện có, chỉ dùng tạm sau adapter, không phải Target V1 |
| `OWNER_DECISION_REQUIRED` | Implementer **không** được tự chọn cho production |
| `BLOCKED_EXTERNAL` | Không chặn code sau mock, nhưng chặn integration/vận hành thật |
| `NOT_BUILT_UPSTREAM` | Bên ngoài đã trả lời nhưng **chưa build**; IVR chạy bằng mock |
| `SUPERSEDED` | Đã bị quyết định owner thay thế; giữ để tra cứu, không dùng làm yêu cầu |
| `OPEN` | Chưa ai trả lời |

## Mock-first rule

Mọi external dependency phải có port + deterministic fake provider + failure scenarios. Điều này cho phép đạt `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`, nhưng **không** biến `BLOCKED_EXTERNAL` thành `VERIFIED`.

Ba mode: `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL`. `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới release gate.

## Thay đổi gần đây

| Ngày | Quyết định | Ảnh hưởng tới pack |
| --- | --- | --- |
| `2026-08-27` | `OD-18` — M3 quyết định, IVR chỉ thực thi | M3 chỉ gửi task đã quyết định cần gọi; IVR không cần field trust/risk-evidence để phân loại khách; trust wire fields chỉ còn `LEGACY_READ` |
| `2026-08-26` | `OD-17` — gỡ `sellable_status[]` khỏi IVR | `02` thành `SUPERSEDED`; IVR không còn lối dữ liệu nào tới ops-core; `D-06` (Module 3 revalidate với ops lúc callback) nay là **lưới an toàn duy nhất** |
| `2026-08-25` | `OD-15` — không gọi khách cũ | `SUPERSEDED` bởi `OD-18`; không còn yêu cầu Module 3 gửi risk-evidence cho IVR |
