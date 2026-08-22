# Privacy Impact Assessment — `W-0052` · `P10-1`

Ngày dự thảo: `2026-08-19` · Trạng thái: **`DRAFT_UNSIGNED`** — chờ Legal/Privacy ký (`W-0009`)
· Phạm vi: IVR order confirmation, chế độ `MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 0. Tài liệu này chưa được ký

Không mục nào ở đây là kết luận pháp lý. Đây là **đánh giá kỹ thuật** do bên xây hệ thống viết, để
Legal/Privacy có thứ cụ thể mà phản đối. `P10-1` §11 cấm go-live khi chưa có PIA đã ký, và trạng thái
hôm nay là **chưa ký**.

## 1. Hoạt động xử lý

| | |
| --- | --- |
| Mục đích | xác nhận đơn hàng COD/Golden Hour qua cuộc gọi tự động, giảm đơn ảo |
| Chủ thể dữ liệu | khách đã đặt đơn trên hệ thống Sales |
| Loại dữ liệu | xem `docs/compliance/data-inventory.md` §3 — 16 trường |
| Cơ sở pháp lý đề xuất | **thực hiện hợp đồng**; audit theo **ghi chép pháp lý**. Không mục nào dựa trên đồng ý |
| Người nhận | Sales (callback kết quả). Không bên thứ ba nào khác |
| Chuyển ra nước ngoài | **không** |
| Thời hạn lưu | `docs/compliance/retention.md` — **chưa ai ký** |
| Quyết định tự động | có: điều kiện gọi/không gọi. **Không** có hệ quả pháp lý cho khách — kết quả IVR là **tín hiệu đầu vào**, không tự đổi trạng thái đơn (D-02) |

## 2. Rủi ro, mỗi rủi ro kèm biện pháp **đã dựng** hay **chưa**

| # | Rủi ro | Biện pháp | Trạng thái |
| --- | --- | --- | --- |
| R-01 | Số điện thoại rò ra log/response | `PiiGuard` chạy trên toàn response body và trên correlation id; fail-closed khi regex timeout (DO-06) | **đã dựng**, `UT-FND-PII-*` |
| R-02 | Số điện thoại rò qua database | IVR **không lưu số**; chỉ `phone_ref`, `phone_masked`, dial-token ciphertext (D-05). Check constraint từ chối `phone_masked` trông như số thật | **đã dựng** |
| R-03 | Ghi âm cuộc gọi không có cơ sở | Recording **OFF mặc định** (DT-05); `recording_ref` null | **đã dựng** |
| R-04 | Gọi người đã từ chối nhận cuộc gọi | do-not-call là **chặn cứng**, và ba trạng thái *bị chặn / không rõ / nguồn không trả lời* đều dừng dispatch | **đã dựng**, `COMP-DNC-03` |
| R-05 | Gọi ngoài khung giờ / quá số lần | attempt policy có phiên bản, `max_attempts` ≤ 10 ép ở database | **đã dựng**; **chính sách production chưa ký** (`W-0007`) |
| R-06 | Dữ liệu sống lâu hơn mục đích | retention job P1-5, dry-run mặc định | **cơ chế đã dựng**; **chu kỳ chưa ký** (DF-07) |
| R-07 | Bản backup giữ dữ liệu quá hạn | `prune.sh` theo tuổi; `retain_until` đi theo dump nên bản restore vẫn bị retention xử lý | **đã dựng**, `DG-RETENTION-04` |
| R-08 | Nghe lén đường truyền tới database | TLS ép **lúc render chart**; `Prefer` bị từ chối ở mọi env | **đã dựng**, `DG-CRYPTO-01` |
| R-09 | Dữ liệu cá nhân vào kho phân tích | hai lớp: allowlist cột đọc từ model EF + `PiiGuard` trên từng giá trị ghi | **đã dựng**, `BI-PII-01` |
| R-10 | Không đáp ứng được yêu cầu của chủ thể | `DsarService` + runbook; xoá redact đúng phạm vi, audit bất biến | **đã dựng**, `COMP-DSAR-02` |
| R-11 | Mã hoá at-rest của volume | — | **CHƯA** — thuộc storage class của cluster (`W-0063`) |
| R-12 | Khoá mã hoá backup nằm trong file, chưa rotate | — | **CHƯA** — cần KMS (`W-0063`), rotation nối `P7-5` |
| R-13 | Nội dung script đọc cho khách nghe | whitelist trường, không địa chỉ/thanh toán/sức khoẻ | **cơ chế đã dựng**; **whitelist chưa ký** (`OD-V1-15`) |
| R-14 | Không có ai giám sát độc lập việc mở real call | ladder ép ở chart + CI; không job nào set `REAL_CUSTOMER_CALL_ALLOWED` | **đã dựng**; **DF-03 chưa ký** |

## 3. Rủi ro tồn dư

Ba nhóm, và **không nhóm nào IVR tự đóng được**:

1. **Chưa ký** — chu kỳ retention (DF-07), whitelist script (`OD-V1-15`), attempt policy (`W-0007`),
   sign-off go-live (DF-03). Đây là các quyết định của chủ sở hữu/Legal, không phải việc kỹ thuật.
2. **Chưa có hạ tầng** — mã hoá volume, KMS, cluster, multi-AZ (`W-0063`).
3. **Chưa gọi khách thật lần nào.** Mọi đánh giá ở trên nói về hệ thống **chưa từng** gọi ai. Một PIA
   cho hệ thống đang chạy sẽ phải viết lại sau lần lab đầu tiên (`W-0008`).

## 4. Kết luận đề xuất

Với `REAL_CUSTOMER_CALL_ALLOWED=NO` và `MOCK`, **không có xử lý dữ liệu cá nhân của khách thật nào
đang diễn ra** — seed dùng dải test `84xxxxx…`. Rủi ro hiện tại là **rủi ro của thiết kế**, không phải
rủi ro đang xảy ra.

Trước lab (một SIM thật): cần R-13 và R-05 được ký. Quyết định về permission sửa allowlist
(`OD-V1-20`) đã có ngày 2026-08-22: `IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN` được cấp cho role
`Admin`. Rủi ro **chưa** hiện thực hoá: `FeatureFlagAdminService` kiểm `IRuntimeGateAuthorization` trước
mọi mutation, và bản đăng ký trong production (`PendingRuntimeGateAuthorization`) luôn trả `false`,
nên phiên `Admin` gọi `POST /feature-flags/{env}` nhận `409 IVR_OPERATIONAL_BLOCKED` — đổi kiểu từ
chối chứ không mở cổng. Cái thực sự mở là quyền **đọc** flag/kill-switch.

Điều đã đổi là **thứ tự khoá**: permission không còn là lớp ngoài cùng. Ngày nào
`PendingRuntimeGateAuthorization` được thay bằng một bản duyệt, mọi phiên `Admin` sẽ bật/tắt được
`realCustomerCallAllowed`/`globalDialKillSwitch` ngay lập tức, không cần thêm quyết định permission
nào nữa. Việc thay đó phải được coi là một thay đổi có tác động privacy, cần đánh giá lại PIA này,
và chỉ làm sau khi có chữ ký four-eyes của `OD-V1-20` — hiện vẫn trống.

Trước production: cần thêm R-06, R-11, R-12 và DF-03.

**Chữ ký**

| Vai trò | Tên | Ngày | Kết luận |
| --- | --- | --- | --- |
| Privacy | _(trống)_ | | |
| Legal | _(trống)_ | | |
| Chủ sở hữu IVR | _(trống)_ | | |

Ô trống là ô trống. Không điền hộ.
