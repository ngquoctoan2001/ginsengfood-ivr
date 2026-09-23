# W-0299 — Trả `ProductionTargetV1FieldsApproved` về `NO` trong mọi appsettings

Ngày 16/09/2026. Baseline `90b914a`. Không đổi code runtime, không đổi wire, không phát hành
contract. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Nguồn: bản giao việc 16/09 mục `B6`. Nhận xét đúng, không cãi.

## Vấn đề

Cờ này quyết định một bản kịch bản đọc cho khách có được mang **các trường quyết định production**
hay không — tên món, số lượng, vùng giao rút gọn (`ScriptContentContracts.cs` `ProductionAllows`).
Nó ship sẵn `YES` trong **cả bốn** file cấu hình gốc:

```text
src/Ivr.Api/appsettings.json:18
src/Ivr.Api/appsettings.Development.json:12
src/Ivr.Worker/appsettings.json:13
src/Ivr.Worker/appsettings.Development.json:7
```

Nghĩa là **bề mặt lời thoại rộng nhất là mặc định** cho bất kỳ deployment nào không biết lối tắt
nó đi. Một mặc định nới quyền riêng tư là đặt ngược chiều: cái hẹp mới là cái nên sống sót khi có
người quên.

## Đã làm

Đổi cả bốn file về `"NO"`. Không đổi một dòng code nào.

Bật lại vẫn đúng một biến môi trường — `IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED`, đọc **trước** file
trong `ServiceCollectionExtensions.cs:76-83` — nên deployment nào có chữ ký đằng sau thì không mất gì.
Thứ nó không còn được là **bật mà không nói ra**.

Thêm `UT-FAILGATE-TARGETV1-DEFAULT-01` trong `FailGateTests`: đọc **file đã ship**, không phải object
đã bind, vì khuyết tật nằm ở thứ được ship và chỉ đọc thứ được ship mới bắt được nó quay lại. Test
cũng đỏ nếu khoá bị **xoá hẳn**, không chỉ khi bị đổi giá trị.

## Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **1064/1064** pass, 0 failed, 0 skipped, exit 0 |
| `gitnexus_impact` trên `ProductionAllows` | **LOW**, 1 caller (`Allows`), 0 execution flow |
| Mutation: đặt lại `YES` ở một file | Test **ĐỎ** đúng như phải thế |
| Khôi phục `NO` | Test **XANH** |
| `generate-test-traceability --check` | `TEST_TRACEABILITY_CURRENT=672` |
| `gate-status`, `docs-selftest`, `compliance-pack-selftest`, `ci-config-selftest` | PASS |

Test cũ không phải sửa một dòng: `FeatureFlagApiTests` và `ScriptContentTests` đều tự dựng
`ScriptContentOptions` riêng, không dựa vào mặc định của file.

## Vì sao đây là siết thật, không phải hình thức

Quét `deploy/` và mọi `docker-compose*.yml`: **không môi trường nào** đặt
`IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED`. Nên trước lượt này, mọi môi trường đều chạy với cờ
**bật**; sau lượt này, mọi môi trường chạy với cờ **tắt** cho tới khi có người cố ý bật.

Ảnh hưởng tới cuộc gọi khách hôm nay vẫn là **0**, vì `PRODUCTION_REAL` còn bị chặn độc lập ở
`IvrOptionsValidator.cs:51-53`, `DispatchGate.cs:65-73` và `SchedulerCapacity.cs:704`. Đây là dọn một
mặc định sai, không phải vá một lỗ đang chảy.

## Còn lại

`DR-12` — nội dung lời thoại đọc cho khách chạm quyền riêng tư của khách hàng — vẫn cần **một lần xác
nhận rủi ro ở cấp công ty**, chung cụm với `OD-V1-11`. Lượt này không thay cho chữ ký đó; nó chỉ làm
cho việc thiếu chữ ký không còn im lặng bật sẵn.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0299 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Trả cờ duyệt
field Target V1 về NO. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: xác nhận rủi ro cấp công ty (OD-V1-11). Mọi giới hạn trong cột
Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test,
sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
