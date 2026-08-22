# Data governance — `W-0053` · `P10-2`

Ngày: `2026-08-19` · Nguồn sự thật: `src/Ivr.Infrastructure/Governance/DataClassification.cs`

## 1. Tài liệu này không phải nguồn sự thật

Bảng phân loại nằm **trong code**. Tài liệu được kiểm **ngược lại** code — `DG-RETENTION-04` đỏ nếu
một bảng có trong model mà không có trong bảng phân loại, và đỏ nếu tài liệu này thiếu một dòng mà
code có.

Lý do: một bảng phân loại bảo trì tay trong Markdown mô tả schema **của ngày ai đó đọc lần cuối**.
`W-0055` vừa thêm 7 bảng; nếu phân loại chỉ nằm ở đây thì hôm nay đã có 7 bảng không lớp, không quy
tắc crypto, không quy tắc backup — và không gì đỏ.

## 2. Hai trục, cố ý không gộp

| Trục | Trả lời | Ở đâu |
| --- | --- | --- |
| **Protection class** | *trong lúc dữ liệu còn ở đây, cái gì phải đúng* | `DataProtectionClass` |
| **Retention class** | *khi nào nó biến mất* | `specs/database/05-retention-and-privacy.md` §2, P1-5 |

Một bảng có thể sống ngắn mà rất nhạy cảm, hoặc sống mãi mà vô hại. Gộp hai trục thành một nhãn là
cách một bản backup được mã hoá **vì nó cũ** chứ không phải **vì thứ nằm trong nó**.

| Protection class | Nghĩa | Nghĩa vụ |
| --- | --- | --- |
| `PiiDirect` | có trường gắn với khách: phone ref, phone masked, dial-token ciphertext, speech snapshot, response snapshot | crypto at-rest + in-transit bắt buộc; **không** được ghi ra artifact chưa mã hoá, kể cả tạm |
| `PiiDerived` | không có trường khách, nhưng mang khoá dẫn về khách trong hệ thống IVR **không sở hữu** | cùng nghĩa vụ crypto — bí danh không phải ẩn danh — nhưng **được** chia sẻ cho reader báo cáo |
| `AuditTrail` | append-only, ai làm gì | không bao giờ purge; nên là lớp **sống lâu hơn mọi bản sao khác** của cùng một sự kiện |
| `Operational` | trạng thái vận hành: lease, counter, checkpoint | mã hoá vì dùng chung volume, nhưng không phải lý do crypto tồn tại |
| `Configuration` | flag, policy, script version | **toàn vẹn** quan trọng hơn bí mật — một attempt policy bị sửa đổi tần suất gọi khách |

## 3. Phân loại đầy đủ

| Bảng | Protection | Retention class |
| --- | --- | --- |
| `ivr_confirmation_tasks` | `PiiDirect` | `task_metadata` + `speech_snapshot` (xem dưới) |
| `ivr_task_intake_outbox` | `Operational` | `task_metadata` |
| `ivr_attempt_policies` | `Configuration` | `active_config` |
| `ivr_call_jobs` | `PiiDerived` | `task_metadata` |
| `ivr_call_attempts` | `PiiDerived` | `attempt_metadata` |
| `ivr_raw_call_events` | `PiiDirect` | `raw_call_event` |
| `ivr_call_results` | `PiiDerived` | `result_metadata` |
| `ivr_result_callbacks` | `PiiDirect` | `callback_metadata` |
| `ivr_sim_channels` | `Operational` | `active_config` |
| `ivr_capacity_incidents` | `Operational` | `task_metadata` |
| `ivr_technical_exceptions` | `Operational` | `attempt_metadata` |
| `ivr_admin_actions` | `AuditTrail` | `audit_log` |
| `ivr_evidence_links` | `PiiDerived` | `evidence_link` |
| `ivr_idempotency_keys` | `PiiDirect` | `idempotency_key` |
| `ivr_audit_log` | `AuditTrail` | `audit_log` |
| `ivr_evidence` | `PiiDerived` | `evidence_link` |
| `ivr_feature_flags` | `Configuration` | `active_config` |
| `ivr_review_items` | `PiiDerived` | `review_item` |
| `ivr_retention_checkpoints` | `Operational` | `retention_control` |
| `ivr_script_versions` | `Configuration` | `active_config` |
| `ivr_script_approvals` | `AuditTrail` | `active_config` |
| `ivr_console_accounts` | `PiiDirect` | `staff_account` (`OWNER_DATA_REQUIRED`) |
| `ivr_console_sessions` | `PiiDerived` | `console_session` (`OWNER_DATA_REQUIRED`) |
| `fact_call_outcome` | `PiiDerived` | `analytics_derived` |
| `fact_call_job` | `PiiDerived` | `analytics_derived` |
| `dim_program` | `Operational` | `analytics_derived` |
| `dim_script_variant` | `Operational` | `analytics_derived` |
| `dim_result_type` | `Operational` | `analytics_derived` |
| `agg_kpi_daily` | `Operational` | `analytics_derived` |
| `etl_checkpoint` | `Operational` | `analytics_derived` |

Lý do từng bảng nằm trong code, cạnh chính mục phân loại — chỗ người sửa bảng sẽ nhìn thấy.

**Một bảng có thể chịu hai class trên hai đồng hồ khác nhau.** `ivr_confirmation_tasks` bị
`speech_snapshot` **redact các trường bên trong** từ rất sớm, rồi mãi sau mới bị `task_metadata`
xoá cả dòng. Mô hình hoá thành một giá trị đơn là một **defect** mà `COMP-RETENTION-04` bắt được:
`speech_snapshot` trông như một class **không ai phân loại** trong khi job vẫn chạy nó. Giờ có
`PreDeletionAnonymizeClass`, và cổng đòi **mọi** class job thực thi phải được một bảng nào đó khai.

Hai bảng đáng chú ý:

- **`ivr_idempotency_keys` là `PiiDirect`** dù bản thân nó không có trường khách nào. Nó lưu
  **response snapshot**, tức bất kỳ thứ gì endpoint đã trả về. Đây là bảng duy nhất mà nội dung
  được định nghĩa bởi **bảng khác**, nên nó thừa hưởng lớp cao nhất mà bất kỳ endpoint nào có thể trả.
- **`ivr_script_approvals` là `AuditTrail`** trong khi `ivr_script_versions` là `Configuration`. Một
  bản ghi phê duyệt là bản ghi audit: xoá nó là xoá bằng chứng ai đã cho phép nói gì với khách.

## 4. Quyền cấp cho reader báo cáo

Danh sách bảng một BI tool được cấp **suy ra từ phân loại**, không viết cạnh nó — nên nó không thể
trôi khỏi phân loại. Điều kiện làm quyền đó bảo vệ được: **không bảng nào trong danh sách là
`PiiDirect`**, và test khẳng định đúng điều đó.

Hệ quả cụ thể: `GRANT USAGE ON SCHEMA analytics` + `GRANT SELECT ON ALL TABLES IN SCHEMA analytics`,
**không** grant nào trên schema `public`. Đây là thứ *cấp được*; **chưa ai cấp** (`W-0063`).

## 5. Crypto

| Lớp | At-rest | In-transit | Trạng thái |
| --- | --- | --- | --- |
| Database | mã hoá volume | TLS tới PostgreSQL | volume: **`NOT_RUN`** — thuộc hạ tầng (`W-0063`); TLS: **ép ở chart**, xem §6 |
| Backup | encrypt-then-MAC, xem `docs/dr-topology.md` §3 | n/a | **chạy thật** trong `DG-BACKUP-02` |
| Dial token | ciphertext trong DB, ánh xạ ở vault của SIM adapter (D-05) | n/a | `TESTS_PASS` từ `W-0012`/`W-0047` |
| Khoá ký JWT | — | — | **không rotate được** — `MockOidcIssuer` sinh RSA theo tiến trình (`W-0006`) |

**Mã hoá volume không chứng minh được ở đây.** Nó là thuộc tính của storage class trong cluster, và
cluster chưa có (`W-0063`). Ghi ra thay vì để bảng trống trông như đã xong.

## 6. TLS tới PostgreSQL: một khoảng trống thật vừa đóng

Trước slice này, chart render connection string **không có `SSL Mode`**. Npgsql mặc định `Prefer`,
nghĩa là: thử TLS, và **im lặng rơi về plaintext** nếu server không mời. Một server cấu hình sai sẽ
cho ra kết nối không mã hoá mà **không lỗi, không log, không cảnh báo** — đúng loại hỏng mà một dòng
trong tài liệu bảo mật không bắt được.

Giờ `database.sslMode` là giá trị trong values, và `_helpers.tpl` **hỏng lúc render** nếu:

| Vi phạm | Vì sao |
| --- | --- |
| `sslMode: Disable` ngoài `dev` | plaintext ra ngoài máy dev là mất in-transit protection |
| `sslMode: Prefer` ở bất kỳ env nào | `Prefer` là "mã hoá nếu tiện" — không phải một chính sách |
| `trustServerCertificate: true` ở `prod` | mã hoá mà không xác thực chỉ chặn nghe lén thụ động, không chặn kẻ đứng giữa |

`dev` được phép `Disable` vì bootstrap Postgres của cluster thử không có chứng chỉ; ghi thẳng trong
values kèm lý do, chứ không để mặc định lặng lẽ.

## 7. Backup và retention (DF-07)

Backup **cũng** phải tuân retention. Một bản backup giữ 90 ngày trong khi `result_metadata` giữ 30
ngày nghĩa là chu kỳ thật của dữ liệu đó là 90, và con số 30 chỉ là mô tả.

Hai luật, ép ở hai chỗ khác nhau:

1. **Tuổi tối đa của backup ≤ chu kỳ retention dài nhất đã cấu hình.** Kiểm trong
   `DG-RETENTION-04` (nửa drill): tạo catalogue có bản quá tuổi, chạy prune, đòi nó biến mất.
2. **Sau khi restore, dữ liệu quá hạn vẫn phải bị xoá.** Restore đưa về **trạng thái cũ**, gồm cả
   những dòng lẽ ra đã hết hạn từ lâu. Nên retention job phải chạy **sau** mỗi lần restore, và
   `retain_until` trong bản restore là thứ làm điều đó xảy ra tự động.

Điểm thứ hai là chỗ dễ quên nhất: người ta kiểm "backup có bị prune không" rồi dừng, trong khi đường
rò rỉ thật là **một lần restore hợp lệ mang dữ liệu hết hạn quay lại production**.

## 8. Cái này KHÔNG chứng minh

- **Mã hoá volume at-rest**: thuộc storage class của cluster (`W-0063`), `NOT_RUN`.
- **KMS**: khoá backup hiện lấy từ biến môi trường/file. Vault/KMS là `W-0063`; rotation nối `P7-5`.
- **Multi-AZ**: xem `docs/dr-topology.md` §5. Drill failover chạy thật nhưng trên **một máy**.
- **Không có backup nào từng chạy trên dữ liệu production.** Drill dùng schema thật và seed test.
