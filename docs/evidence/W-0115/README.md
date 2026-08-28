# W-0115 — Khóa tập giá trị enum ở tầng PostgreSQL

Ngày: `2026-08-24`  
Baseline: `main@fd7d7373f92aa3d89045cf0bf74c39bf71a69d34`  
Trạng thái: `TESTS_PASS`  
Plan: [`remaining-work-plan-2026-08-22.md` §A9](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

W-0115 thêm **16 CHECK cho cột enum đóng trên 8 bảng**, cộng một bất biến kết quả. Migration
chạy preflight trước DDL và từ chối toàn bộ transaction nếu thấy một giá trị cũ không hợp lệ.
Không đổi API, OpenAPI hay luồng gọi; `REAL_CUSTOMER_CALL_ALLOWED` vẫn là `NO`.

---

## 1. Phạm vi được chốt theo cột, không theo tên bảng

Kế hoạch W-0107 dùng cách nói “16/22 bảng còn thiếu”. Đếm bảng không đủ để quyết định: một bảng
có thể chứa ba họ đóng và hai họ mở. Audit W-0115 đọc từng write-site và chốt theo cột. Baseline
thực tế đã có **7** CHECK enum, không còn là 6 như ghi chú W-0107: W-0113 đã thêm
`ivr_call_attempts.voice_region` sau khi ghi chú đó được viết.

| Bảng trong model (22) | Quyết định W-0115 | Lý do |
| --- | --- | --- |
| `ivr_confirmation_tasks` | thêm 1 | `eligibility_decision` là tập đóng; `order_state` do Core sở hữu nên giữ mở |
| `ivr_attempt_policies` | không thêm | version/program là khóa cấu hình có thể mở rộng, không phải lifecycle của một dòng |
| `ivr_call_jobs` | thêm 3 | `status`, `queue_status`, `eligibility_decision` đều do IVR ghi từ tập hữu hạn |
| `ivr_call_attempts` | thêm 2 | `status`, `result_status` đóng; `disposition` và mã kỹ thuật từ provider giữ mở |
| `ivr_task_intake_outbox` | giữ 1 CHECK cũ | `status` đã khóa |
| `ivr_raw_call_events` | không thêm | raw disposition/mã kỹ thuật là dữ liệu biên provider, phải giữ mở |
| `ivr_call_results` | thêm 2 + 1 bất biến | result/action đóng; reason giữ mở; hai cột kết quả cuối phải bằng nhau |
| `ivr_result_callbacks` | thêm 3 | result, state và delivery lifecycle đều đóng |
| `ivr_sim_channels` | thêm 1 | `execution_mode` đã khóa; W-0115 khóa thêm `status` |
| `ivr_capacity_incidents` | thêm 2 | status và scope đều do service sở hữu |
| `ivr_technical_exceptions` | không thêm | `exception_type` nhận taxonomy mở từ provider |
| `ivr_admin_actions` | không thêm | action/reason là audit vocabulary mở |
| `ivr_evidence_links` | không thêm | owner/evidence reference là định danh mở |
| `ivr_idempotency_keys` | không thêm | scope mở theo use case |
| `ivr_audit_log` | không thêm | action, actor và target là audit vocabulary mở |
| `ivr_evidence` | không thêm | evidence kind/work id là định danh mở |
| `ivr_review_items` | thêm 2 | source đóng; status là hợp của review lifecycle và CRM proposal lifecycle |
| `ivr_retention_checkpoints` | không thêm | data class/segment và trạng thái job được phép tiến hóa độc lập với console enum |
| `ivr_script_versions` | giữ 1 CHECK cũ | status đã khóa |
| `ivr_script_approvals` | giữ 1 CHECK cũ | approval type đã khóa |

Ba vùng cố ý không khóa đúng yêu cầu A9: `order_state`, `review.reason`/`resolution`, và mọi mã
provider/technical. Không có tập giá trị nào được nới chỉ để hợp thức hóa fixture cũ.

---

## 2. 17 ràng buộc mới

| Bảng | Cột / bất biến | Số giá trị |
| --- | --- | ---: |
| `ivr_confirmation_tasks` | `eligibility_decision` nullable | 6 |
| `ivr_call_jobs` | `status` | 29 |
| `ivr_call_jobs` | `queue_status` | 13 |
| `ivr_call_jobs` | `eligibility_decision` | 6 |
| `ivr_call_attempts` | `status` | 10 |
| `ivr_call_attempts` | `result_status` nullable | 11 |
| `ivr_call_results` | `result_type` | 11 |
| `ivr_call_results` | `recommended_core_action` dạng lưu DB | 7 |
| `ivr_call_results` | `final_result_status = result_type` | bất biến |
| `ivr_result_callbacks` | `result_status` | 11 |
| `ivr_result_callbacks` | `result_state` | 1 |
| `ivr_result_callbacks` | `delivery_status` | 11 |
| `ivr_sim_channels` | `status` | 8 |
| `ivr_capacity_incidents` | `status` | 2 |
| `ivr_capacity_incidents` | `scope` | 3 |
| `ivr_review_items` | `source_type` | 4 |
| `ivr_review_items` | `status` | 4 |

`recommended_core_action` trong DB không có tiền tố `CORE_`. Tiền tố chỉ xuất hiện ở contract
Sales sau bước map. Full integration đã bắt được nhầm lẫn tầng này trong bản fixture đầu tiên;
fixture được sửa về dạng domain, không sửa CHECK.

`review_items.status` là hợp có chủ ý của hai producer:

- review thông thường: `OPEN`, `RESOLVED`;
- đề xuất opt-out gửi CRM: `PENDING_CRM`, `ACCEPTED_BY_CRM`.

Test opt-out thật đã phát hiện hai giá trị CRM trong lượt full regression đầu. CHECK cuối cùng giữ
nguyên nghĩa nghiệp vụ này và từ điển console có đủ cả bốn nhãn.

---

## 3. Migration fail-closed trước khi sửa schema

Migration [`20260824021636_W0115ClosedEnumChecks.cs`](../../../src/Ivr.Infrastructure/Persistence/Migrations/20260824021636_W0115ClosedEnumChecks.cs)
chạy một khối `DO` trước mọi `AddCheckConstraint`:

1. quét đủ 16 cột và bất biến kết quả;
2. gom `DISTINCT` giá trị sai theo đúng `table.column`;
3. ném SQLSTATE `23514` với tiền tố `W-0115 enum preflight blocked`;
4. chỉ thêm 17 constraint nếu không có vi phạm.

Migration không tắt transaction. Vì vậy preflight đỏ thì không có trạng thái nửa áp dụng.

`IT-DBENUM-MIGRATE-05` dựng schema N-1 hai lần và chứng minh cả hai chiều:

- seed source/status rác ⇒ migration dừng bằng `23514`, nêu cả field/value, không ghi migration id;
- seed opt-out `PENDING_CRM` ⇒ migration thành công, đủ 17 tên constraint; sau đó
  `ACCEPTED_BY_CRM` vẫn hợp lệ và `LEGACY_OPEN` bị chính CHECK PostgreSQL từ chối.

---

## 4. Dữ liệu hiện có

### DB dev local — PASS read-only

Quét `DISTINCT` ngày `2026-08-24` trả 17/17 field không có giá trị ngoài tập. Các nhóm có dữ liệu:

| Nhóm | Giá trị quan sát |
| --- | --- |
| task/job eligibility | `ELIGIBLE_FOR_IVR` |
| job | `RESULT_READY_FOR_CALLBACK`, `HELD_CALLBACK` |
| attempt | `NORMALIZED_FINAL`; result gồm confirmed/cancelled/no-answer-final |
| result | ba result type tương ứng; ba core action canonical; final luôn bằng type |
| callback | `PENDING_CORE_REVALIDATION`, `DELIVERED_ACCEPTED`; ba result status |
| SIM | `IDLE` |
| capacity/review | bảng rỗng tại thời điểm quét |

### Production — `OWNER_DATA_REQUIRED`

Môi trường hiện tại không có credential DB production (`PRODUCTION_DB_CREDENTIAL=NOT_CONFIGURED`).
Do đó kết quả local và integration **không** được trình bày như bằng chứng production. Preflight
migration là cổng fail-closed cuối cùng, nhưng owner DBA vẫn phải chạy cùng truy vấn read-only trên
production/staging trước cửa sổ triển khai.

---

## 5. Rolling deploy và miễn trừ W-0114

W-0114 đúng khi đánh dấu mọi CHECK trên cột cũ là không tương thích mặc định. W-0115 không tắt
cổng. [`ReviewedExemptions`](../../../tests/Ivr.UnitTests/Persistence/RollingDeploySchemaCompatibilityTests.cs)
chứa đúng 17 khóa `{migration}::AddCheckConstraint::{table.column}` và lý do không rỗng:

- 16 tập enum được đối chiếu với writer của release N-1 và có preflight dữ liệu;
- bất biến result có lý do riêng: hai writer N-1 gán hai cột từ cùng normalized result.

`UT-SCHEMA-BACKCOMPAT-01` vẫn kiểm mọi migration khác như trước. Miễn trừ hết khớp nếu id
migration, operation hoặc cột đổi; nó không che được thao tác thứ hai.

---

## 6. Guard chống trôi

`IT-L10N-DBENUM-04` giờ dựng lại cả chuỗi SQL C# bị nối nhiều dòng, đọc cả dạng nullable
`IS NULL OR ... IN (...)`, rồi yêu cầu tập tên constraint khớp chính xác với
`FamilyByConstraint`. Guard không còn có thể xanh vì parser bỏ sót một CHECK mới.

Từ điển [`enums.vi.json`](../../../admin-ui/src/i18n/enums.vi.json) bổ sung nhãn cho
`CREATED`, `DRY_RUN`, `PENDING_CRM`, `ACCEPTED_BY_CRM`; mọi giá trị constraint còn lại đã có nhãn.

---

## 7. Kết quả kiểm chứng

| Gate | Kết quả |
| --- | --- |
| `IT-L10N-DBENUM-04` | PASS |
| `UT-SCHEMA-BACKCOMPAT-01..04` + enum guard | 5/5 PASS |
| `IT-DBENUM-MIGRATE-05` | PASS |
| focused integration sau chuẩn hóa fixture | 104/104 PASS |
| `Ivr.UnitTests` | 484/484 PASS |
| `Ivr.IntegrationTests` | 255/255 PASS (+1) |
| `Ivr.ContractTests` | 22/22 PASS |
| `Ivr.ChaosTests` | 8/8 PASS |
| **Tổng .NET Release** | **769/769 PASS** |
| `dotnet build` | PASS, 0 warning / 0 error |
| `dotnet format --verify-no-changes` | PASS |
| admin-ui | lint + typecheck + 221/221 + build PASS |
| traceability | 471 test có tag |
| gate-status | 11 gate, 114 work item, 21 quyết định mở |
| CI config / docs / progressive / OpenAPI drift | PASS |
| `progressive-selftest.mjs` | PASS, 13 migration |

Lượt full solution cuối chạy trên candidate Release đã format và sau khi traceability được sinh
lại. Lượt test bị ngắt và lượt chạy trên binary cũ không được tính vào bằng chứng chốt.

---

## 8. Ngoài phạm vi và cổng còn lại

- Không đổi OpenAPI; contract vẫn `1.0.0-draft.16`.
- A10 không được triển khai. Telemetry `key=value` giữ nguyên để grep; nếu owner muốn thêm
  `detail_vi` thì đó là decision + contract `draft.17` riêng.
- Không gọi SIM/carrier, không tạo real-customer-call evidence; `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- Production distinct-value scan: `OWNER_DATA_REQUIRED`.
- Không push; hai remote vẫn chờ owner xác nhận.
