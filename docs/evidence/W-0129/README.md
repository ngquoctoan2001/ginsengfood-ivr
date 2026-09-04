# W-0129 — Intake rejection-reason traceability evidence

Ngày: `2026-08-28`  
Baseline: `main@b4d8903` trên candidate W-0128 chưa commit  
Implementation được nhận diện lại: test tại `3e36b46`, runtime tại `c7c0f70`  
Trạng thái: `TESTS_PASS_LOCAL`  
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu và giới hạn

W-0129 sửa attribution `W-0120` bị tái sử dụng, ghi taxonomy/compatibility và tạo test evidence
cho refinement rejection reason. Work này không được tự đổi decision, HTTP status, persistence,
dispatch hay OpenAPI shape.

`W-0120` vẫn là Work ID hợp lệ của lỗi globalization-invariant đã `ACCEPTED`; không sửa lịch sử,
plan hoặc evidence thật của W-0120.

## 2. Phát hiện khi đối chiếu runtime

1. Bốn comment production/test gắn nhầm `W-0120`; implementation thật nằm ở commit
   `c7c0f70` và chưa có tracker/evidence riêng.
2. Mô tả gốc “mọi rejection trả HTTP 200, `blocked_reasons` là tín hiệu duy nhất” không đúng với
   public route:
   - `ivr_confirmation_required=false` và matrix sai bị `TaskIntakeEndpoint.ValidateSchema` chặn
     trước service → `400 IVR_MALFORMED_REQUEST`;
   - contact invalid tạo service outcome có reason chi tiết nhưng endpoint chuyển thành
     `422 IVR_CONTACT_INVALID`; public error envelope hiện không mang reason đó.
3. `DIAL_TOKEN_ALREADY_EXPIRED` không thể đạt tới: token `<= now` luôn đồng thời `< window expiry`,
   nhưng predicate “expires before window” chạy trước. W-0129 đảo đúng precedence để reason “đã
   hết hạn” có nghĩa, giữ nguyên `TASK_REJECTED_CONTACT_INVALID`, `IVR_CONTACT_INVALID`, không tạo
   job và không dispatch.

## 3. Taxonomy và compatibility

Taxonomy chín reason cùng visibility matrix nằm ở
[`specs/api/06-error-codes.md` §2a](../../../specs/api/06-error-codes.md). IR-06 §3.9–3.11 được sửa
theo đúng response shape runtime.

Compatibility được khóa như sau:

- `blocked_reasons` vẫn là open `string[]`; OpenAPI `draft.22` và generated DTO không đổi;
- hai business-approved pair vẫn accepted như trước;
- mọi refined failure giữ decision, failure code, persistence và dispatch outcome cũ;
- public route vẫn trả `400` cho required-flag/matrix schema violation và `422` cho contact;
- M3 chưa được phép branch trên chín reason nội bộ. Wire exposure là owner/M3 contract decision
  riêng, không được suy ra từ W-0129.

## 4. Test evidence

| Test ID | Chứng minh | Kết quả |
| --- | --- | --- |
| `UT-INTAKE-REASON-TAXONOMY-13` | đủ 9 reason; đúng decision/failure code; không tạo job | `9/9 PASS` |
| `UT-INTAKE-REASON-COMPAT-14` | hai pair được duyệt vẫn accepted | `2/2 PASS` |
| `IT-INTAKE-REASON-WIRE-15` | required flag/matrix vẫn 400; contact vẫn 422 | `3/3 PASS` |

## 5. Full local verification

| Gate | Kết quả |
| --- | --- |
| Build / format | `0 warning / 0 error`; `dotnet format --verify-no-changes` PASS |
| .NET unit | `490/490 PASS` |
| .NET integration | `232/232 PASS` |
| .NET contract | `24/24 PASS` |
| .NET chaos | `8/8 PASS` |
| Traceability | regenerate/check `465` tagged test PASS; đủ ba Test ID W-0129 |
| OpenAPI | lint, validate, negative selftest và drift PASS; current contract/hash không đổi bởi W-0129 |
| API docs | `14` artifact; docs selftest, boundary và links PASS |
| Markdown map | `595` file; `664` link resolved; `200` unresolved giữ nguyên baseline, nên W-0129 không thêm unresolved link |
| PII scope | evidence W-0129 `1 file PASS`; added lines trong API-06/IR-06 có `0` match |
| GitNexus final audit | shared dirty worktree: `84 file / 484 symbol / 29 process / CRITICAL`; aggregate chủ yếu gồm W-0128 và concurrent WIP, không được gán riêng cho W-0129 |

Full PII scan toàn repository không được dùng để nâng verdict vì baseline W-0122/W-0124 đã có
finding cũ. Scan gộp các tài liệu lịch sử đang sửa vẫn đỏ ở nội dung có sẵn; kiểm trực tiếp added
lines API-06/IR-06 cho `0` match và evidence W-0129 đạt `PII_SCAN_PASS files=1`.

GitNexus impact trước sửa đã báo `ContactRejectionReason` mức `HIGH` (`19` symbol, `3` process)
và đã được cảnh báo trước khi đổi precedence. Final `detect-changes` là số aggregate của toàn
shared worktree, gồm W-0128 và procurement/TTS WIP; vì vậy không dùng mức `CRITICAL` đó để tuyên
bố riêng W-0129 có 29 flow. Full regression phía trên là kiểm chứng hành vi cho bounded delta này.

## 6. Residual gate

Nếu M3 cần reason theo field trên public API, owner hai bên phải chọn và ký một trong hai hướng:

1. thêm safe reason vào `error.details` nhưng giữ `4xx`; hoặc
2. đổi các reject tương ứng sang `200 IvrTaskIntakeResult`.

Cả hai đều là contract change, cần OpenAPI/CDC/client rollout riêng. W-0129 không chọn thay owner.

`TESTS_PASS_LOCAL` không có nghĩa M3 đã tích hợp hoặc production ready. Hosted CI, shared M3→M8
E2E và owner decision về wire exposure vẫn `NOT_RUN / OWNER_DECISION_REQUIRED`.
