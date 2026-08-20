# W-0059 — Evidence: Legal retention & DF-03 sign-off pack (`P11-3`)

Ngày: `2026-08-19` · Trạng thái: **`EVIDENCE_SUBMITTED`** — hồ sơ đầy đủ về cấu trúc, và
**tuyên bố rõ rằng chưa ai ký**. `P9-1` phải đọc nó là **NO-GO**.

## 1. Điều phải nói trước

Slice này **không** đóng cổng nào. Nó chuẩn bị hồ sơ để chủ sở hữu/Legal/Security **có thể** ký, và
`P11-3` §1 cấm đưa tư vấn pháp lý giả — nên mọi chỗ cần chữ ký mang nhãn `LEGAL_SIGNOFF_REQUIRED` và
mọi ô tên để **trống**.

`specs/decisions/DF-03-signoff.md` **không tồn tại**, và đó là trạng thái đúng.

## 2. Đầy đủ về cấu trúc ≠ được duyệt

Đây là phân biệt mà cả slice xoay quanh, và cũng là câu cuối cùng `compliance-pack-selftest.mjs` in
ra. Một hồ sơ có đủ mọi mục, mọi bảng kín, mọi tham chiếu giải được — **vẫn** là một hồ sơ chưa ai
đồng ý. `MASTER-05` nói cùng điều đó ở phía evidence: **đã nộp không phải đã được chấp nhận**.

Cổng ép phân biệt này ở hai chỗ:

- `LEGAL-RET-01` chấp nhận một chu kỳ **chỉ khi** nó là số đã ký **hoặc** là
  `LEGAL_SIGNOFF_REQUIRED`. Một con số xuất hiện mà không có chữ ký làm cổng đỏ.
- `SIGNOFF-DF03-04` cho phép `DF-03-signoff.md` **không tồn tại**, nhưng nếu nó tồn tại thì ba ô
  phê duyệt phải có tên. Một bản ghi sign-off không có người ký **trông y hệt** một bản có — chỉ
  khác ở ô tên, và đó là chỗ cổng nhìn.

## 3. Hai danh mục, hai ống kính, cố ý không gộp

| Tài liệu | Ống kính | Người đọc |
| --- | --- | --- |
| `docs/compliance/data-inventory.md` (P10-1) | **trường**: mục đích, cơ sở pháp lý, hành vi khi xoá | cơ quan quản lý, người trả lời DSAR |
| `docs/compliance/ivr-data-inventory.md` (P11-3) | **class**: ai sở hữu, mức nhạy cảm, lưu ở đâu, xoá bằng gì | người ký DF-07 |

Người ký retention không cần biết `phone_ref` là gì. Họ cần biết **ai chịu trách nhiệm**, dữ liệu
**ở đâu**, và **cơ chế nào thực sự xoá** — vì đó là thứ họ đang ký. Gộp hai ống kính vào một bảng
làm cả hai người đọc phải lọc bỏ nửa còn lại.

## 4. Phương án, không phải một con số

`ivr-retention-options.md` đưa **hai phương án** cho mỗi class kèm cột **"mất gì nếu chọn A"**.
Người ký cần ba thứ mà một bảng số trống không cho: các lựa chọn có thật, cái mất đi ở mỗi lựa chọn,
và ràng buộc kỹ thuật giới hạn lựa chọn. Đưa một con số duy nhất là bắt họ hoặc gật hoặc tự nghĩ lại
từ đầu.

Bốn ràng buộc **không thương lượng được** ghi ngay trên bảng: `speech_snapshot` phải ngắn hơn
`task_metadata`, chu kỳ con ≤ chu kỳ cha, tuổi backup ≤ chu kỳ dài nhất, và `analytics_derived`
không có chu kỳ riêng.

## 5. DT-05: bốn điều kiện mở lại, không phải ba

Bật ghi âm **không phải một thay đổi cấu hình** — nó là một hoạt động xử lý riêng với mục đích, cơ
sở, rủi ro và chu kỳ khác. Bốn điều kiện, và cổng kiểm đủ cả bốn:

1. cơ sở pháp lý **riêng** cho việc ghi âm,
2. cơ chế thu thập và lưu đồng ý — IVR **không có**, nên một cơ sở "đồng ý" hiện là không có cơ sở,
3. **thông báo cho người bị ghi âm**, bằng chính cuộc gọi đó,
4. chu kỳ lưu riêng, ký theo DF-07.

Điều kiện 3 là điều kiện hay bị bỏ: một quyết định liệt kê "xin đồng ý" rồi dừng đã bỏ qua phần
người ta phải **được cho biết**.

Và điều kiện thứ năm mang tính kỹ thuật: **PIA phải viết lại** — bản hiện tại đánh giá một hệ thống
không ghi âm.

## 6. Kiểm chứng

| Check | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `LEGAL-RET-01` | gỡ cơ chế xoá của `raw_call_event` | ❌ đỏ, **nêu tên class** |
| `LEGAL-PII-02` | (không dựng) đòi đủ 4 điều kiện DT-05 + biên giới token D-05 trên 5 tài liệu | ✅ |
| `LEGAL-DSAR-03` | (không dựng) đòi 3 giới hạn trong DB + giới hạn backup, trong **cả hai** tài liệu | ✅ |
| `SIGNOFF-DF03-04` | tạo `DF-03-signoff.md` với ô tên `_(trống)_` | ❌ đỏ: *"a sign-off record without a signer is a sign-off nobody gave"* |
| `GATE-EVID-05` | đổi một tham chiếu thành `W-9999` | ❌ đỏ, nêu đúng `W-9999` |

| Lệnh | Kết quả |
| --- | --- |
| `compliance-pack-selftest.mjs` | `COMPLIANCE_PACK_SELFTEST_PASS` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` — `compliance_pack_selftest` root-included, `allow_failure: false` |
| `scan-pii.sh` | xem §8 |

## 7. `GATE-EVID-05` kiểm một thứ dễ bỏ sót

Ngoài việc mọi tham chiếu `docs/evidence/W-XXXX` phải giải được, nó đòi **hàng P8 phải hiện ra là
trống**. Một danh sách evidence lặng lẽ bỏ qua phase không có evidence **đọc như đã phủ đủ** — và
P8 là phase quan trọng nhất, vì nó là phase duy nhất có cuộc gọi thật.

## 8. Cái này KHÔNG chứng minh

- **Không chữ ký nào.** DF-07, DT-05 và DF-03 đều `LEGAL_SIGNOFF_REQUIRED` / chưa tồn tại.
- **Cổng chỉ kiểm cấu trúc.** Nó biết một ô tên trống; nó **không** biết người ký có đọc gì không,
  có thẩm quyền không, hay con số họ ký có hợp lý không.
- **`DATA_CLASSES` trong script là một danh sách viết tay.** `COMP-RETENTION-04` khẳng định phía C#
  khớp với governance map, nhưng **không cổng nào** khẳng định danh sách trong `.mjs` khớp với
  `RetentionDataClasses` — một class thêm vào cả hai phía C# mà quên thêm vào script sẽ không ai đỏ.
  Đây là khoảng trống thật, ghi ra thay vì giấu.
- **13/13 điều kiện tiên quyết của DF-03 chưa đạt**, và không mục nào IVR tự đóng được.
- **Hàng evidence P8 trống** — chưa có cuộc gọi thật nào.
