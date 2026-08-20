# W-0053 — Evidence: Data governance, backup crypto & DR (`P10-2`)

Ngày: `2026-08-19` · Trạng thái: **`TESTS_PASS`** — 4/4 drill §8 **chạy thật** trên PostgreSQL;
**không multi-AZ**, **không KMS**, **không mã hoá volume at-rest** (`W-0063`)

## 1. Điều phải nói trước

`P10-2` §11 cấm "DR chỉ trên giấy". Không mục nào ở đây đọc một file YAML rồi gọi đó là bằng chứng:
bốn drill dựng PostgreSQL thật, mã hoá một bản dump thật, restore nó, giết primary bằng `SIGKILL`
rồi promote standby.

Nhưng chúng chạy trên **một máy**. Một drill trên một host chứng minh **cơ chế** và **không** chứng
minh **topology** — mất một AZ khác hẳn mất một container, vì AZ mang theo mạng, storage và control
plane. Drill in `PASS_SINGLE_HOST` chứ không in `PASS`, ở cả từng bước lẫn dòng tổng kết.

## 2. Một khoảng trống thật vừa đóng: TLS tới database

Trước slice này chart render connection string **không có `SSL Mode`**. Npgsql mặc định `Prefer`
nghĩa là: thử TLS, và **im lặng rơi về plaintext** nếu server không mời. Một server cấu hình sai cho
ra kết nối không mã hoá mà **không lỗi, không log, không cảnh báo**.

Ba luật giờ ép **lúc render**, mỗi luật có kiểm âm:

| Vi phạm | Thông báo | Kiểm âm |
| --- | --- | --- |
| `sslMode: Prefer` ở **bất kỳ** env nào | `Prefer falls back to plaintext in silence` | ❌ đỏ |
| `sslMode: Disable` ngoài `dev` | `Only dev may run without TLS to the database` | ❌ đỏ |
| `trustServerCertificate: true` ở `prod` | `not a machine in the middle` | ❌ đỏ |

`Prefer` bị từ chối **kể cả ở dev**: "mã hoá nếu tiện" không phải một chính sách, nó tạo ra kết nối
plaintext đúng vào điều kiện mà chính sách sinh ra để phủ. `Disable` ít nhất **thành thật** về việc
nó là gì, nên dev được dùng và không env nào khác được.

`DG-CRYPTO-01` còn có **đối chứng dương**: một PostgreSQL thứ hai không có chính sách, cùng client,
cùng cờ, **chấp nhận** kết nối plaintext. Không có nó thì "server từ chối plaintext" cũng nhất quán
với "client không làm được plaintext", và drill chứng minh không gì cả.

## 3. Phân loại dữ liệu nằm trong code

Bảng phân loại ở `src/Ivr.Infrastructure/Governance/DataClassification.cs`, tài liệu được kiểm
**ngược lại** nó. Một bảng phân loại bảo trì tay trong Markdown mô tả schema **của ngày ai đó đọc
lần cuối** — và `W-0055` vừa thêm 7 bảng sáng nay.

Hai trục **cố ý không gộp**: `DataProtectionClass` trả lời *trong lúc dữ liệu còn ở đây, cái gì phải
đúng*; retention class (P1-5) trả lời *khi nào nó biến mất*. Gộp lại là cách một bản backup được mã
hoá **vì nó cũ** chứ không phải **vì thứ nằm trong nó**.

Hai phân loại đáng nêu:

- **`ivr_idempotency_keys` là `PiiDirect`** dù không có trường khách nào của riêng nó. Nó lưu
  **response snapshot** — bất kỳ thứ gì endpoint đã trả. Đây là bảng duy nhất mà nội dung được
  định nghĩa bởi **bảng khác**, nên nó thừa hưởng lớp cao nhất mà bất kỳ endpoint nào có thể trả.
- **`ivr_script_approvals` là `AuditTrail`** trong khi `ivr_script_versions` là `Configuration`. Một
  bản ghi phê duyệt là bản ghi audit: xoá nó là xoá bằng chứng ai đã cho phép nói gì với khách.

Quyền cấp cho BI tool **suy ra từ phân loại**, không viết cạnh nó, nên không thể trôi. Test khẳng
định điều kiện làm quyền đó bảo vệ được: **không bảng nào trong danh sách là `PiiDirect`**.

## 4. Backup: ba tính chất, mỗi cái vì một cách hỏng

| Tính chất | Vì cách hỏng nào |
| --- | --- |
| dump **không bao giờ chạm đĩa** ở dạng rõ | `pg_dump` nối thẳng vào cipher — không cửa sổ nào một bản sao chưa mã hoá của bảng `PiiDirect` nằm chờ dọn |
| **encrypt-then-MAC** | AES-CTR không MAC là **malleable**: lật một bit ciphertext lật đúng bit đó của plaintext, và restore sẽ áp SQL **do kẻ tấn công chọn** vào chính database nó định cứu |
| **verify trước decrypt** | verify sau khi giải mã nghĩa là SQL độc hại đã được sinh ra rồi mới bị chặn |
| hai subkey từ một master | dùng chung một khoá cho cả bí mật lẫn toàn vẹn là cách biến hai bảo đảm thành một |
| **từ chối chạy** nếu thiếu `openssl` | thất bại phải chặn là: job không mã hoá được, ghi plaintext, rồi **báo thành công** |

Drill lật **một byte** giữa ciphertext và đòi `RESTORE_REFUSED` — kèm **tiền đề**: md5 trước/sau
phải khác nhau. Không có tiền đề đó, một lần tamper hỏng lặng lẽ sẽ làm phép kiểm từ chối thành phép
kiểm rỗng **vẫn xanh**.

## 5. Phát hiện lớn nhất: standby đã promote **kế thừa** ràng buộc đồng bộ

Lần chạy đầu của drill **treo**. Truy ra thì đó không phải lỗi của drill mà là một cái bẫy thật, và
là loại tệ nhất — **trông như đã xong**.

`pg_basebackup` chép `postgresql.auto.conf`, nên standby dựng từ primary có replication đồng bộ
**kế thừa `synchronous_standby_names`**. Sau promote, giá trị đó trỏ tới một standby **không còn tồn
tại**. Node rời recovery, nhận kết nối, **trả lời mọi truy vấn đọc**, qua mọi health check — và
**mọi lệnh ghi chờ vô hạn** trong `IPC | SyncRep`.

**`statement_timeout` KHÔNG cứu được**, và đó là nửa thứ hai của phát hiện. Cuộc chờ xảy ra ở
**COMMIT**, sau khi câu lệnh đã chạy xong: transaction đã bền vững tại chỗ, thứ còn thiếu chỉ là
**xác nhận**. Mô tả đúng trạng thái là *"đã commit tại đây, chưa được xác nhận, người gọi không nhìn
thấy"* — không phải *"lệnh ghi thất bại"*.

Hệ quả: `deploy/dr/failover.sh` là **script**, không phải gạch đầu dòng. Drill khẳng định thất bại
đó **trước khi** gọi script, nên bước giải phóng sync không thể lặng lẽ trở nên thừa. Script
**idempotent** vì chuỗi hành động thật là: promote bằng tay → thấy ghi treo → *rồi mới* đi tìm
runbook.

## 6. Kiểm chứng

| Drill | Kết quả đo |
| --- | --- |
| `DG-CRYPTO-01` | server từ chối plaintext (`no pg_hba.conf entry`), `pg_stat_ssl.ssl = t` cho phiên `require`, **đối chứng dương** chấp nhận plaintext; 4/4 env render đúng `SSL Mode`; 3/3 kiểm âm chart đỏ |
| `DG-BACKUP-02` | mã hoá trong luồng, **0** file `.sql` rõ trên đĩa, tamper 1 byte → `RESTORE_REFUSED`, restore 2 bảng **khớp số dòng** (backup 0,3s / restore 0,3s) |
| `DG-DR-03` | **RPO = 0** — dòng commit ngay trước `SIGKILL` sống sót; promote **0,7s**, lượt ghi thành công đầu tiên **11,9s** (budget 60s); giữa hai mốc, một lượt ghi treo **10s** trong `SyncRep` cho tới khi client bỏ cuộc |
| `DG-RETENTION-04` | prune dry-run báo mà không xoá, prune thật xoá **cả file anh em**, giữ bản trong hạn; bản restore vẫn mang `retain_until` trên **2** dòng hết hạn |

| Lệnh | Kết quả |
| --- | --- |
| `dr-selftest.mjs` | `DR_SELFTEST_PASS_SINGLE_HOST=DG-DR-03` |
| `helm lint` 4 env | 0 chart(s) failed |
| kubeconform (bật backup CronJob) | 14 resource, 0 invalid |
| backup guard | 3/3 kiểm âm đỏ: thiếu `image` / `existingSecret` / `destination` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` — **13 fragment** root-included; kiểm âm gỡ include DR → đỏ đúng thông báo |
| `dotnet test Ivr.sln -c Release` | **419/419** — 22 contract + 241 unit + 5 chaos + 151 integration, 0 fail |
| `scan-pii.sh` | `PII_SCAN_PASS files=282` |

## 7. Backup CronJob tắt ở **cả 4 env**, và không phải vì thận trọng

Job cần ba thứ chưa tồn tại: một image mang **cả** `pg_dump` lẫn `openssl`, một đích lưu **bền ngoài
cluster**, và master key từ secret store (`W-0063`). Bật sẵn sẽ để lại một CronJob **hỏng vĩnh viễn**
trong mọi namespace — đúng cách người vận hành học thói quen bỏ qua Job failed, bài học `W-0044` đã
trả giá một lần.

Nhưng chart **từ chối render** nếu ai bật nó mà thiếu bất kỳ thứ nào trong ba, thay vì render một
placeholder tình cờ hợp lệ. Và script thì **thật**: `deploy/backup/*.sh` chạy trong `DG-BACKUP-02`
mỗi lượt pipeline. Cái thiếu là **chỗ để đặt kết quả**.

## 8. Cái này KHÔNG chứng minh

- **Không multi-AZ.** Một host, hai container. Mất AZ mang theo mạng, storage và control plane —
  không cái nào bị drill chạm tới.
- **Không mã hoá volume at-rest.** Thuộc storage class của cluster (`W-0063`).
- **Không KMS.** Khoá master từ file; chưa rotate lần nào qua `P7-5`.
- **Không PITR.** Drill dùng logical dump; WAL archive + `recovery_target_time` chưa dựng.
- **RTO chưa gồm** phát hiện sự cố, quyết định failover, chuyển endpoint, hâm nóng pool.
- **Chưa drill trên dữ liệu quy mô production.** Kích thước dump ảnh hưởng trực tiếp tới RTO của
  restore; con số 0,3s **không suy rộng được**.
- **Chưa có fencing.** Runbook đòi người xác nhận primary đã chết; không cơ chế nào ép điều đó, nên
  promote khi chỉ mất liên lạc vẫn tạo split-brain được.
- **Bước "dựng lại standby" chưa có phép kiểm nào** — và đó chính là bước đưa RPO về lại 0 sau
  failover.
- **Chưa lượt backup nào chạy trên dữ liệu production**, và chưa lượt nào chạy trong cluster.
