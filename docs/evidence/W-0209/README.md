# W-0209 — Header intake: `1-128`, bảng chữ cái đóng

Ngày: 2026-09-07 · Baseline: `main@fb6a613` · Trạng thái: **TESTS_PASS** cho tài liệu và phần ghim;
**OWNER_DECISION_REQUIRED** cho một chỗ lệch trong chính IVR.

## 1. Vấn đề — ba con số sai, không phải một

Codex nêu một vế: tài liệu bàn giao ghi `Idempotency-Key: <8-200 chars>` còn
`TaskIntakeEndpoint.RequiredHeader` chặn ở `128`. Đọc kỹ ra ba:

| | IR-06 nói | Code thi hành |
| --- | --- | --- |
| Dài tối đa | `200` | **`128`** |
| Dài tối thiểu (`Idempotency-Key`) | `8` | **`1`** — không có sàn |
| Bảng chữ cái | *không nói gì* | **chỉ `[A-Za-z0-9._:-]`** |

Vế thứ ba không có trong báo cáo nào trước đây, và nó là vế dễ làm hỏng M3 nhất. `+`, `/`, `=`, dấu
cách, `{}` đều bị từ chối — nghĩa là **một key base64 sẽ bị chặn**. Sinh key idempotency ngẫu nhiên
bằng base64 là việc hoàn toàn bình thường; M3 không có cách nào đoán được điều này từ tài liệu, và
OpenAPI cũng không nói (`{ type: string }` trần).

Cùng luật đó áp cho `X-Correlation-Id`, ở cả `RequiredHeader` lẫn `CorrelationMiddleware` — hai nơi
viết cùng một điều kiện.

## 2. Phát hiện thêm — cùng một header, hai luật trong cùng một API

| Route | Guard | Max | Bảng chữ cái |
| --- | --- | --- | --- |
| Intake `/tasks` | `TaskIntakeEndpoint.RequiredHeader` | `128` | **có ràng** `[A-Za-z0-9._:-]` |
| Admin · internal · script | `InternalServiceOptions.RequireIdempotencyKey` | `128` | **không ràng** (chỉ `PiiGuard.IsSafeText`) |

Một key base64 bị route intake từ chối nhưng được route admin nhận. Cả hai dùng chung một
`$ref: '#/components/parameters/IdempotencyKey'` trong OpenAPI, nên schema không thể mô tả đúng cả
hai. **Chưa quyết** nên siết admin hay nới intake — ghi lại, không tự chọn.

## 3. Đã làm gì

**Không sửa một dòng runtime nào.**

### 3.1. Ghim hành vi — `IT-INTAKE-HEADER-07`

`tests/Ivr.IntegrationTests/TaskIntakeApiTests.cs`, chạy qua HTTP thật:

| Gửi | Kết quả |
| --- | --- |
| `Idempotency-Key` 128 ký tự | `200 OK` |
| `Idempotency-Key` 129 ký tự | `400` + `IVR_MALFORMED_REQUEST` |
| `Idempotency-Key` = `sB3+xQ/9dGVzdA==` | `400` + `IVR_MALFORMED_REQUEST` |
| `X-Correlation-Id` 129 ký tự | `400` + `IVR_MALFORMED_REQUEST` |

Cuối test khẳng định chỉ **một** job vào store — tức ba case kia dừng trước mọi side effect.

Case `X-Correlation-Id` dài đáng chú ý: `CorrelationMiddleware` có nhánh "mint id mới thay vì tin id
vào", nên hoàn toàn có thể tưởng request sẽ đi tiếp với id sinh mới. Trên route này thì **không** —
nó tới `RequiredHeader` và bị từ chối. Đó là lý do phải chạy chứ không suy.

### 3.2. Sửa tài liệu

`integration-requirements/06-module-3-api-handover.md`:

- **`§3.1.1` mới** — bảng ba con số sai, mã lỗi cho từng chiều (`422 IVR_MISSING_TRACE` khi thiếu,
  `400 IVR_MALFORMED_REQUEST` khi sai cú pháp), cảnh báo base64, và ghi chỗ lệch intake↔admin.
- `§3.1` block `http` + bảng header — `<8-200 chars>` → `<1-128 ký tự, chỉ [A-Za-z0-9._:-]>`.
- `§4.1` (chiều IVR → M3) — thay khoảng bịa `<8-200 chars>` bằng **giá trị IVR thật sự phát**:
  `ivr-result:RESULT-<raw_event_id>`, dài 50 ký tự với `raw_event_id` GUID-32 hiện hành. Chiều này
  IVR là bên gửi nên một khoảng là vô nghĩa; M3 cần biết cái gì sẽ tới.

### 3.3. Cố ý chưa làm — OpenAPI

`CorrelationId` và `IdempotencyKey` vẫn là `{ type: string }` trần, dù **cùng file đã có** schema
đúng ở `GeneratedCorrelationId`: `minLength: 1, maxLength: 128, pattern: '^[A-Za-z0-9._:-]+$'`.

Hai lý do hoãn, không phải một:

1. Sửa bump hash đã ghim → cần re-pin có review (`W-0204`). Gộp cùng lượt với `W-0208` là một
   re-pin cho hai mục, thay vì hai.
2. Lệch intake↔admin (§2) **chưa được quyết**. Viết pattern vào `IdempotencyKey` dùng chung bây giờ
   sẽ mô tả sai route admin. Quyết trước, viết sau.

## 4. Kiểm chứng

```text
dotnet test Ivr.sln                     893/893 PASS, 0 failed, 0 skipped
                                        (Unit 593 · Integration 267→268 · Contract 24 · Chaos 8)
dotnet test --filter IT-INTAKE-HEADER-07  1/1 PASS
contract-freeze-verifier.mjs            CONTRACT_FREEZE=PASS
openapi-contract-drift.mjs              OPENAPI_HASHES_PINNED=3 · HUMAN_DIFF_CURRENT=YES
generate-test-traceability.mjs --check  TEST_TRACEABILITY_CURRENT=552  (551→552)
docs-selftest.mjs                       API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`.

## 5. Còn lại

1. **Quyết lệch intake↔admin**: siết `RequireIdempotencyKey` theo bảng chữ cái của intake, hay bỏ
   ràng buộc bảng chữ cái ở intake. Ảnh hưởng tới việc OpenAPI mô tả được bằng một `$ref` hay phải
   tách hai.
2. **Re-pin OpenAPI** cùng lượt với `W-0208`: `CorrelationId` lấy đúng schema của
   `GeneratedCorrelationId`; `IdempotencyKey` theo kết quả bước 1.
3. Nếu `raw_event_id` đổi định dạng, con số `50` ở `§4.1` phải đổi theo.
