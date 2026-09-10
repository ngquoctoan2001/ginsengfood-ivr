# W-0266 — Owner tự làm quorum cho ba mục, và ba thứ việc đó không mở ra

Ngày: 2026-09-10 · Baseline: `main@f730809` · Trạng thái: **TESTS_PASS**.

`W-0254` phát hiện ba mục trong sổ quyết định treo **không** vì thiếu nội dung, mà vì chờ một quorum
gồm những vai không tồn tại trong tổ chức này. `IR-06 §9a` nêu hai đường; owner chọn đường thứ nhất:
**tự làm quorum cho cả ba**, và **không** mua ý kiến pháp lý ngoài cho V1.

Sổ quyết định: **5 mục mở → 2**.

## 1. Đóng gì

| Mục | Nội dung đã ký từ trước | Trạng thái mới |
| --- | --- | --- |
| `OD-V1-11` | Ghi âm TẮT vĩnh viễn ở V1; metadata cuộc gọi 90 ngày; DTMF 180 ngày; audit admin 1 năm | ✅ `CLOSED` 2026-09-10 |
| `OD-V1-21` | Cấu hình GitLab: project/runner/registry/protected branch/hosted MR pipeline | ✅ `CLOSED` 2026-09-10 |
| `OD-V1-23` | Opt-out **explicit-only** ở V1; không suy ra từ số lần khách từ chối | ✅ `CLOSED` 2026-09-10 |

Còn mở, và **đúng** khi còn mở: `OD-V1-09` (giao thức lab đã ký, chờ SIM/nhà mạng thật) và
`OD-V1-10` (cố ý chưa ký — con số **32 eSIM** là giả định chưa đo; ký bây giờ là ký một điều chưa
biết).

## 2. Đóng quyết định **không** mở ra mọi thứ

Đây là chỗ dễ đọc nhầm nhất, nên nó được ghi thẳng vào cả ba dòng register, vào `IR-06 §9a` và vào
`IR-07` Phần A. Sáu tháng sau, ai đọc `CLOSED` mà không thấy dòng này sẽ tưởng duyệt script
production đã chạy được.

**Hai trong ba mục vướng ràng buộc kỹ thuật mà chữ ký không gỡ được:**

| Mục | Đóng cái gì | **Không** đóng cái gì |
| --- | --- | --- |
| `OD-V1-11` | Chính sách ghi âm, thời hạn lưu | `PRODUCTION_REAL` vẫn cần **ba actor id khác nhau** — `ScriptContentContracts.EnsureApprovalAllowed`: người tạo không được duyệt, `CONTENT` ≠ `PRIVACY_LEGAL`. Một người thì **code vẫn từ chối**. Duyệt script cho production **vẫn chặn**, và đó là sự thật vận hành, không phải chữ ký còn thiếu |
| `OD-V1-21` | Quyết định cấu hình GitLab | Bằng chứng four-eyes cần **Premium/Ultimate cộng một reviewer thứ hai** — không phụ thuộc chữ ký của ai. Ghi nhận là **giới hạn được chấp nhận**, không phải điều kiện đã thoả |

Muốn mở `OD-V1-11` thật sự thì hoặc có người thứ hai/ba, hoặc ký một `OD` mới **đổi chính luật
ba-actor** — và luật ấy tồn tại có lý do, nên đổi nó là một quyết định riêng chứ không phải thủ tục.

## 3. `OD-V1-23` đóng sạch, nhưng phơi ra một khoảng trống thật

Khác hai mục trên: về mặt quyết định nó đóng được hoàn toàn. Nhưng đọc kỹ nội dung đã ký thì
**V1 hiện không có tín hiệu opt-out tường minh nào**:

- `DTMF-0` là **phím hủy đơn** — lời thoại khóa cứng nói *"bấm phím 0 để hủy"* (`TargetV1SpeechPolicy`),
  và `M8-08 §4.2` ghi rõ **không tái dùng hai phím này cho opt-out**.
- Phím 9 (`handoff`) **ngoài scope**, và `TargetV1SpeechPolicy.ValidateTemplate` **từ chối** mọi
  template chứa chữ "phím 9".

Nên hệ quả thực tế của "explicit-only" ở V1 là: **không có opt-out**. Đã ghi thành `A-13` trong
`IR-07` để dev M3 **không dựng luồng trông chờ nó**. Thêm một tín hiệu opt-out là một `OD` mới —
cần wording, nguồn tín hiệu và bằng chứng — chứ không phải một dòng code.

Hằng số `2/3` (suy opt-out từ nhiều lần từ chối) vì vậy vẫn là **khoảng trống**, không phải quy tắc.

## 4. Không xoá lịch sử

Mỗi dòng register giữ **nguyên văn trạng thái cũ** ngay sau phần đóng
(*"Nguyên văn trạng thái cũ: ⏳ `CONTENT_SIGNED / APPROVER_QUORUM_UNRESOLVED` 2026-09-05 — …"*).
Đọc lại vẫn thấy nó từng treo vì cái gì, và vì sao. Đóng một dòng không phải xoá lý do nó từng mở.

## 5. Kiểm chứng

```text
gate-status.mjs            GATE_STATUS_PASS — 2 open decisions  (từ 5)
dotnet test Ivr.sln        982/982 PASS, 0 failed, 0 skipped
CONTRACT_FREEZE=PASS
API_DOCS_SELFTEST_PASS
d06 / dial-token           W0178_SELFTEST_PASS · W0183_SELFTEST_PASS sau khi re-pin IR-06
pipe count register        6/7/7 — giữ nguyên, bảng không vỡ
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | Gửi `IR-07` cho dev M3 — nay không còn dòng nào chờ người không tồn tại | **owner** |
| — | Re-pin `IR-06` trong `opt-out` + `upstream` validator sau khi luồng kia hạ cánh | tôi |
| — | Nếu muốn mở `PRODUCTION_REAL` script approval: người thứ hai/ba, hoặc `OD` mới đổi luật ba-actor | owner |
