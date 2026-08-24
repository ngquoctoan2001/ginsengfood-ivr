# W-0116 — Companion Việt hóa cho telemetry tích hợp

Ngày: `2026-08-24`

Baseline: `main@220ebfc9373f3b8a3fd641d71b7d7c6610eeefb6`

Trạng thái: `TESTS_PASS`

Plan: [`remaining-work-plan-2026-08-22.md` §A10](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

Owner chốt `OD-L10N-02b`: thêm `detail_vi` và nâng contract lên
`1.0.0-draft.17`. Phương án triển khai không thay văn bản chẩn đoán gốc và không suy
trạng thái nghiệp vụ từ câu tiếng Anh.

---

## 1. Contract và hành vi

| Shape | Draft.17 | Quy tắc tương thích |
| --- | --- | --- |
| `IvrDependencyStatus.detail` | giữ nguyên, required | raw để grep log/trace |
| `IvrDependencyStatus.detail_vi` | thêm, optional | chỉ phát cho `SIM_GATEWAY` và `ORDER_CORE`; UI fallback draft.16 |
| `IvrFailClosedEvent.effect` | giữ nguyên, required | raw evidence không đổi |
| `IvrFailClosedEvent.hold_new_calls` | thêm, optional | chỉ phát cho `CAPACITY_INCIDENT`; không có ở `REVIEW_ITEM`/draft.16 |

`detail_vi` không phải bản dịch của key/value dùng để truy vết. Nó là companion dễ đọc:

- raw: `provider=MOCK; channels 1/1 enabled`
- companion: `Nhà cung cấp=MOCK; 1/1 kênh đang bật`

Màn tích hợp hiện hai cột cạnh nhau: chi tiết Việt và chi tiết gốc đối chiếu log. Khi gặp
server draft.16 không có `detail_vi`, UI quay về từ điển W-0107 hoặc raw hiện có. Với sự cố
năng lực, UI dùng boolean `hold_new_calls`; nếu field chưa có, nó giữ nguyên `effect` raw.

---

## 2. Impact đã chạy trước sửa

| Symbol | GitNexus |
| --- | --- |
| `BuildDependencies`, `BuildFailClosedEvents` | LOW — 1 caller trực tiếp, 1 endpoint gián tiếp, 0 flow |
| `DependencyDetail`, `FailClosedEventEffect` | LOW — mỗi hàm 1 cell caller |
| record backend, `AdminConfigApiTests` | LOW |
| 2 interface TypeScript | HIGH — 52 liên đới/24 trực tiếp, do toàn bộ type nằm chung `types.ts`; 0 flow |
| 2 generated DTO C# | HIGH — 95 liên đới/20 trực tiếp, generated file dùng chung; 0 flow |

Các HIGH đã được cảnh báo trước sửa. Giảm thiểu: chỉ thêm field optional, không xoá/đổi type
cũ, có fallback draft.16, full regression cả .NET và UI.

---

## 3. Contract governance

- `openapi:validate`: PASS, 2 OpenAPI + negative schema fixtures.
- Codegen `NSwag.ConsoleCore 14.7.1`: PASS; generated server DTO được sinh lại.
- `openapi:accept-reviewed-draft`: `OPENAPI_REVIEWED_DRAFT_BASELINE_UPDATED=YES`.
- `openapi:drift`: `OPENAPI_HASHES_PINNED=3`, human diff current.
- Portal: sinh lại 12 artifact; `API_DOCS_SELFTEST_PASS`.
- Pinned `oasdiff v1.26.1`: **No breaking changes** từ draft.2 tới draft.17.
- `CT-DOC-02`: PASS; container trên checkout Windows cần pipe bỏ CRLF, không sửa script.

Codegen cũng đóng một drift tích lũy ở HEAD: generated server file chưa mang đầy đủ DTO
của draft.13–16 dù authoritative OpenAPI và manifest đã tiến tới draft.16. W-0116 giữ toàn bộ
output xác định của generator; build Release và full test chứng minh output này tương thích.

### Residual lint tách riêng

`openapi:lint` vẫn FAIL 14 lỗi baseline không thuộc W-0116: 13 cách viết `nullable` kiểu
OAS 3.0 trong tài liệu đang khai 3.1 và route `/scripts/` có trailing slash. Diff W-0116 không
chạm các dòng đó. Không sửa lẫn vào contract localization; đây vẫn là residual gate cần work
item riêng.

> Follow-up `W-0117` (`2026-08-24`) đã đóng residual này: lint còn `0` lỗi,
> contract lên `draft.18`, và alias runtime `/scripts/` vẫn được test để không breaking.
> Kết quả 14 lỗi ở trên vẫn được giữ như ảnh chụp đúng tại thời điểm W-0116 kết thúc.

---

## 4. Kết quả kiểm chứng

| Gate | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -c Release --no-restore` | PASS — 0 warning, 0 error |
| .NET full Release | **769/769** = unit 484 + integration 255 + contract 22 + chaos 8 |
| `IT-ADMIN-CONFIG-03` focused | PASS trên PostgreSQL Testcontainers |
| admin-ui | lint + typecheck + **222/222** + production build |
| back-office integration screen | **9/9**; khóa companion, raw và capacity effect tiếng Việt |
| `UT-UI-CONTRACT-06` | **16/16**; khóa hai field optional trong spec và TypeScript |

---

## 5. Không tuyên bố

- Không đổi luồng gọi, provider, permission hay runtime flag.
- Không chạy SIM/carrier, không gọi khách thật, không tạo evidence production.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.
- Hosted CI/deploy/UAT: `NOT_RUN`; `TESTS_PASS` không tự nâng thành `ACCEPTED`.
