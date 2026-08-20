# DR topology — `W-0053` · `P10-2`

Ngày: `2026-08-19` · Drill: `deploy/ci/scripts/dr-selftest.mjs` · Trạng thái: **`PASS_SINGLE_HOST`**

## 1. Điều phải nói trước

`P10-2` §11 cấm "DR chỉ trên giấy", nên không mục nào ở đây đọc một file YAML rồi gọi đó là bằng
chứng. Bốn drill **chạy thật** trên PostgreSQL trong Docker.

Nhưng chúng chạy trên **một máy**. Một drill trên một host chứng minh được **cơ chế** — replication
đồng bộ giữ cam kết, promote hoạt động, standby nhận ghi sau khi lên — và **không** chứng minh được
**topology**: mất một AZ khác hẳn mất một container, vì AZ mang theo mạng, storage và control plane.

Ghi rõ ở đây và trong chính output của drill (`PASS_SINGLE_HOST`), thay vì để một dòng `PASS` gợi ý
nhiều hơn thứ đã đo.

## 2. RTO / RPO

| Chỉ số | Mục tiêu | Đo được | Điều kiện |
| --- | --- | --- | --- |
| **RPO** | `0` | **`0`** — dòng commit ngay trước `SIGKILL` sống sót | replication **đồng bộ** (`synchronous_standby_names`) |
| **RTO** (tới lượt ghi thành công đầu tiên) | ≤ 60s | **11,9s** | promote xong ở **0,7s**; 10s còn lại là bẫy ở §3b |

**RPO=0 là thứ được mua, không phải mặc định.** Replication **bất đồng bộ** có RPO > 0 theo bản
chất: primary xác nhận commit mà standby chưa nhận, nên giết primary là mất đúng commit đó. Đồng bộ
đổi lấy điều đó bằng **một vòng mạng cho mỗi lượt ghi**. Drill khẳng định đánh đổi này thay vì giả
định nó.

**RTO đo tới lượt ghi thành công đầu tiên, không phải tới lúc promote xong.** Khoảng cách giữa hai
mốc là toàn bộ nội dung §3b: promote hoàn tất ở 0,7s và database **vẫn không dùng được** cho tới
11,9s. Chưa tính: phát hiện sự cố, quyết định failover, chuyển endpoint, hâm nóng connection pool.
Con số thật lớn hơn, và lớn hơn bao nhiêu thì **chưa đo được** vì chưa có cluster.

## 3. Backup: mã hoá và toàn vẹn

Ba tính chất, mỗi cái vì một cách hỏng:

| Tính chất | Vì sao |
| --- | --- |
| dump **không bao giờ chạm đĩa** ở dạng rõ | `pg_dump` nối thẳng vào cipher; không có cửa sổ nào một bản sao chưa mã hoá của bảng `PiiDirect` nằm chờ dọn |
| **encrypt-then-MAC** | AES-CTR không MAC là **malleable**: lật một bit ciphertext lật đúng bit đó của plaintext, và restore sẽ áp SQL do kẻ tấn công chọn vào chính database nó định cứu |
| **verify trước decrypt** | verify sau khi giải mã nghĩa là SQL độc hại đã được sinh ra rồi mới bị chặn |
| hai subkey từ một master | dùng chung một khoá cho cả bí mật lẫn toàn vẹn là cách biến hai bảo đảm thành một |

Drill lật **một byte** giữa ciphertext và đòi restore **từ chối** với `RESTORE_REFUSED` — kèm một
tiền đề: md5 trước/sau phải khác nhau, nếu không phép kiểm từ chối kia là phép kiểm rỗng.

**Chưa dùng KMS.** Khoá master hiện đọc từ file. Vault/KMS là `W-0063`; rotation nối `P7-5`.

## 3b. Bẫy: standby đã promote **kế thừa** ràng buộc đồng bộ

Drill tìm ra một lỗi thật, và nó là loại lỗi tệ nhất — **trông như đã xong**.

`pg_basebackup` chép `postgresql.auto.conf`, nên standby dựng từ một primary có replication đồng bộ
**kế thừa `synchronous_standby_names`**. Sau promote, giá trị đó vẫn trỏ tới một standby **không còn
tồn tại**. Node:

- rời recovery (`pg_is_in_recovery() = f`)
- nhận kết nối
- **trả lời mọi truy vấn đọc**
- qua mọi health check

…và **mọi lệnh ghi chờ vô hạn** trong `IPC | SyncRep`.

**`statement_timeout` KHÔNG cứu được.** Phát hiện này đến từ lần chạy đầu của chính drill: nó treo.
Lý do là cuộc chờ xảy ra ở **COMMIT**, sau khi câu lệnh đã chạy xong — transaction đã bền vững tại
chỗ, thứ còn thiếu chỉ là **xác nhận** từ standby. Nên mô tả đúng trạng thái đó là *"đã commit tại
đây, chưa được xác nhận, người gọi không nhìn thấy"*, chứ không phải *"lệnh ghi thất bại"*.

Vì vậy `deploy/dr/failover.sh` tồn tại như một **script**, không phải một danh sách gạch đầu dòng:
bước 2 (giải phóng `synchronous_standby_names`) là bước người ta bỏ sót, và drill khẳng định thất
bại đó **trước khi** gọi script, nên bước 2 không thể lặng lẽ trở nên thừa.

Script **idempotent**: nếu ai đó đã `pg_ctl promote` bằng tay rồi mới đi tìm runbook — đúng chuỗi
hành động thực tế — nó vẫn chạy được thay vì từ chối đúng lúc cần nhất.

## 4. Backup và retention (DF-07)

Hai nửa, và nửa thứ hai là nửa hay bị quên:

1. **Catalogue bị prune theo tuổi.** Dry-run mặc định. Xoá kèm cả file anh em (`.iv`, `.hmac`,
   `.meta`) — một `.meta` mồ côi đọc như một bản backup vẫn còn.
2. **Bản restore vẫn mang dấu retention.** Prune catalogue **không** nói gì về dữ liệu bên trong một
   bản backup còn trong hạn: restore nó mang về những dòng đã hết hạn từ lâu. Thứ làm điều đó an
   toàn là `retain_until` **đi theo trong dump**, nên retention job vẫn thấy chúng là hết hạn thay vì
   coi lần restore là một khởi đầu mới.

Đường rò rỉ thật là **một lần restore hợp lệ mang dữ liệu hết hạn quay lại production** — không phải
một bản backup quên xoá.

## 5. Topology mục tiêu (chưa dựng)

```text
                    ┌──────────── AZ-a ────────────┐
   app (K8s)  ───►  │  postgres primary            │
                    │  synchronous_standby_names   │
                    └──────────────┬───────────────┘
                                   │ streaming, synchronous
                    ┌──────────────▼───────────────┐
                    │  postgres standby   (AZ-b)   │
                    └──────────────────────────────┘
                                   │ WAL archive
                    ┌──────────────▼───────────────┐
                    │  object store (AZ-c / region)│
                    │  encrypted artefacts + WAL   │
                    └──────────────────────────────┘
```

| Thành phần | Trạng thái |
| --- | --- |
| primary + standby đồng bộ | **cơ chế đã drill** (RPO=0 đo được), topology `NOT_RUN` |
| đặt hai bậc ở **hai AZ khác nhau** | `BLOCKED_EXTERNAL` (`W-0063`) |
| WAL archive / PITR | **chưa làm** — drill hiện là logical dump, không phải PITR |
| object store cho artefact | `BLOCKED_EXTERNAL` (`W-0063`) |
| mã hoá volume at-rest | `BLOCKED_EXTERNAL` — thuộc storage class |
| multi-region | chưa yêu cầu; chưa quyết |

## 6. Runbook failover (rút gọn)

Bổ trợ `P9-2`, không thay thế.

1. **Xác nhận primary thật sự mất**, không phải phân vùng mạng. Promote trong lúc primary còn sống
   tạo **split-brain**, và với replication đồng bộ thì hậu quả là hai bản ghi khác nhau cùng nhận
   commit.
2. Chạy `deploy/dr/failover.sh` với `PGDATA` của standby. Script promote (hoặc bỏ qua nếu ai đó đã
   promote), **giải phóng `synchronous_standby_names` kế thừa**, rồi **ghi một dòng probe** để
   chứng minh node thật sự phục vụ được — chứ không chỉ đã rời recovery.
3. Đổi endpoint (Service/DNS) sang bậc đã promote.
4. **Dựng lại một standby mới** trước khi coi sự cố là đã đóng — chạy một primary không standby
   nghĩa là RPO quay về khác 0 mà không ai tuyên bố điều đó.
5. Ghi Activity vào tracker; failover là sự kiện governance, không phải thao tác vận hành lặng lẽ.

**Không có bước tự động nào.** Failover tự động cần một cơ chế chống split-brain (fencing/quorum) mà
chưa ai dựng; một script promote tự động khi chưa có fencing là cách tạo ra sự cố thứ hai.

## 7. Cái này KHÔNG chứng minh

- **Không multi-AZ.** Một host, hai container. Drill in `PASS_SINGLE_HOST` chứ không in `PASS`.
- **Không PITR.** Drill dùng logical dump; WAL archive + `recovery_target_time` chưa dựng.
- **Không có mã hoá volume at-rest** (`W-0063`).
- **Không có KMS.** Khoá master từ file, chưa rotate lần nào qua `P7-5`.
- **RTO chưa gồm phát hiện, quyết định và chuyển traffic** — chỉ đo từ `SIGKILL` tới lượt ghi
  thành công đầu tiên trên node đã promote.
- **Chưa drill trên dữ liệu quy mô production**; kích thước dump ảnh hưởng trực tiếp tới RTO của
  restore và con số hiện tại không suy rộng được.
- **Chưa có fencing.** Runbook yêu cầu người xác nhận primary đã chết; không cơ chế nào ép điều đó.
- **Chưa dựng lại standby sau failover trong drill.** Bước 4 của runbook là bước duy nhất chưa
  có phép kiểm nào, và nó chính là bước đưa RPO về lại 0.
