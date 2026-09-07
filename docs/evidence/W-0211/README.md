# W-0211 — Ba câu tài liệu nói sai về chính hệ thống này

Ngày: 2026-09-07 · Baseline: `main@02cbef1` · Trạng thái: **TESTS_PASS**, mục 0.4 **đóng hẳn**.

Khác ba mục trước của nhóm 0, mục này **không còn việc bên ngoài**. Cả ba lỗi đều thuộc tài liệu do
M8 sở hữu, không có quyết định nào phải chờ, và không có tập nào cần đổi.

## 1. `specs/database/03-enums-and-status.md` §4 — "bốn nơi đang khớp"

Mục đó ghi *"Result type — **11** giá trị"* rồi *"Bốn nơi phải khớp nhau và **đang khớp**"*, liệt kê
enum `IvrResultType`, `ck_ivr_call_results_result_type`, `ck_ivr_result_callbacks_result_status` và
OpenAPI.

Đếm lại bốn nơi đó tại `HEAD`:

| Nơi | Thực tế |
| --- | ---: |
| enum `IvrResultType` | **11** |
| `ck_ivr_call_results_result_type` | **9** |
| `ck_ivr_result_callbacks_result_status` | **6** |
| OpenAPI `ResultType` | **11** |

`11 / 9 / 6 / 11`. Không khớp — và **không được phép** khớp. `W-0172` thu hẹp runtime xuống 9 một
cách có chủ đích: `IVR_OPERATIONAL_BLOCKED` và `IVR_POLICY_BLOCKED` là quyết định **trước** cuộc
gọi, không phải kết quả của một cuộc gọi. Câu *"phải khớp nhau"* mô tả một thiết kế mà repo này cố ý
không có.

Đã thay bằng bảng bốn tập lồng nhau, mỗi tập kèm nơi thi hành và câu hỏi nó trả lời:

| Tập | Số | Câu hỏi |
| --- | ---: | --- |
| Vocabulary | 11 | tên nào tồn tại trong hợp đồng dùng chung |
| Runtime | 9 | một cuộc gọi có thể kết thúc bằng kết quả nào |
| Final callback | 6 | kết quả nào được rời IVR sang Sales |
| Counted attempt | 5 | kết quả nào đốt một lượt gọi của khách |

**Không thêm test.** `UT-RESULT-CONTRACT-01` đã khẳng định `11 / 9 / 6`, khẳng định phần bù đúng
bằng hai mã pre-call, và đã đỏ nếu một tập trôi — có từ `W-0172`. Việc cần làm không phải viết thêm
assertion mà là cho tài liệu **trỏ vào** assertion đã có. Giờ nó trỏ.

**OpenAPI không đụng gì**: `ResultType` ở đó vốn đã đúng, enum 11 giá trị kèm `description` giải
thích IVR không phát hai mã pre-call. Chỗ sai chỉ có một, là `specs/database`.

## 2. IR-06 vẫn chỉ M3 sang `draft.22`

Spec là `1.0.0-draft.23` (`ivr-order-confirmation.v1.yaml:4`), changelog `.22→.23` đã tồn tại. IR-06
vẫn ghi `draft.22` ở **5** chỗ: header trạng thái `L8`, bảng tài liệu `L25`, khối "lấy lại spec"
`§4A.7`, và checklist bàn giao `L1203`. Bảng tài liệu cũng chỉ có dòng changelog `.20→.22`.

M3 làm theo sẽ sinh client trễ một phiên bản và đọc changelog thiếu một bản.

Đã sửa cả 5 chỗ và thêm dòng `.22→.23`.

**Liên kết chéo đáng giá:** đọc bản `.22→.23` mới thấy chính nó là nơi các route feature-flag nhận

```text
minLength: 1 · maxLength: 128 · pattern: '^[A-Za-z0-9._:-]+$'
```

cho `x-correlation-id` — **đúng luật mà route intake vẫn chưa khai** và là nội dung của `W-0209`
(mục 0.2). Repo đã viết luật đó xuống một lần rồi, chỉ chưa áp cho intake. Hai mục nay trỏ lẫn nhau.

## 3. IR-06 §4A.7 nói bảng console "đã xoá"

Dòng đầu bảng *"Những thứ không còn tồn tại"* ghi *"Bảng tài khoản console và bảng vai trò trong DB
— đã xoá"*.

Đúng ở tầng M3 quan tâm: không endpoint, không auth, không màn hình. **Sai ở tầng DB.**
`W0122DropConsoleAccounts.Up()` nay rỗng (`W-0196`), và `P03PreserveConsoleCompatibility` chạy
`CREATE TABLE IF NOT EXISTS` cho `ivr_console_accounts` (`:23`) và `ivr_console_sessions` (`:50`).
Hai bảng **vẫn tồn tại**, cố ý, để cửa sổ rollback của helm còn dùng được.

Đã tách hai mệnh đề: dòng bảng nói về runtime/API, một ghi chú ngay dưới nói về DB. Người soi DB
thấy hai bảng này sẽ biết chúng không phải tàn dư bị bỏ quên.

## 4. Ghi chú cho gate

Cả ba lỗi **đều lọt qua `CONTRACT_FREEZE=PASS`**. Đó không phải lỗi của gate: freeze kiểm pins,
field inventory, draft state và ACK matrix — nó không đọc văn xuôi. Đây là ví dụ cụ thể cho câu cảnh
báo trong worklist: *FREEZE PASS không chứng minh mọi câu trong docs khớp code.*

## 5. Kiểm chứng

```text
dotnet test Ivr.sln                        894/894 PASS, 0 failed, 0 skipped
contract-freeze-verifier.mjs               CONTRACT_FREEZE=PASS
openapi-contract-drift.mjs                 OPENAPI_HASHES_PINNED=3 · HUMAN_DIFF_CURRENT=YES
generate-test-traceability.mjs --check     TEST_TRACEABILITY_CURRENT=553 (không đổi)
docs-selftest.mjs                          API_DOCS_SELFTEST_PASS
```

Không thêm/xoá test nên traceability giữ `553`. Không sửa runtime, không sửa OpenAPI.
`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`.

## 6. Còn lại

Không có. Mục 0.4 đóng.
