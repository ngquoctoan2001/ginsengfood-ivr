# DR configuration — `W-0053` · `P10-2`

## Files

| File | Vai trò |
| --- | --- |
| `failover.sh` | promote standby, **giải phóng ràng buộc sync kế thừa**, và chứng minh node ghi được |
| `rebuild-standby.sh` | sau failover: dựng lại standby đồng bộ từ primary mới, đưa RPO về 0 |

Không có `primary.conf`/`standby.conf`: tham số replication đồng bộ là
`synchronous_standby_names`, đặt bằng `ALTER SYSTEM` trên primary — `failover.sh` gỡ
nó, `rebuild-standby.sh` đặt lại.

## Vì sao `failover.sh` là script chứ không phải danh sách gạch đầu dòng

Bước 2 là bước người ta bỏ sót, và bỏ sót nó tạo ra một database **qua mọi health
check** rồi **treo ở lệnh ghi đầu tiên**.

`pg_basebackup` chép `postgresql.auto.conf`, nên một standby dựng từ primary có
replication đồng bộ **kế thừa** `synchronous_standby_names`. Sau khi promote, giá
trị đó vẫn trỏ tới một standby **không còn tồn tại**: node rời recovery, nhận kết
nối, trả lời truy vấn đọc — và mọi `INSERT` chờ vô hạn trong `SyncRep`, **không
lỗi, không timeout**.

`DG-DR-03` khẳng định đúng thất bại đó **trước khi** gọi script, nên bước 2 không
thể lặng lẽ trở nên thừa.

## Vì sao không tự động

Promote khi primary chỉ **không liên lạc được** (chứ chưa chết) tạo **split-brain**,
và với replication đồng bộ nghĩa là hai node cùng tin mình đã nhận một commit.
Fencing/quorum chưa dựng (`W-0063`), nên người xác nhận primary đã chết; script làm
phần còn lại.

## Sau khi failover: `rebuild-standby.sh`

Dựng lại một standby **trước khi** đóng sự cố. Một primary đơn độc đã âm thầm đưa
RPO về khác 0, và chưa ai tuyên bố điều đó.

Chạy trên máy standby mới, với `PGDATA` rỗng. Thứ tự là cốt lõi: base backup, khởi
động standby, chờ primary thấy nó `streaming`, **rồi mới** đặt
`synchronous_standby_names` — đặt trước thì mọi lệnh ghi trên primary duy nhất treo
suốt thời gian base backup. Thất bại sau bước đó thì script trả giá trị về rỗng để
primary vẫn nhận ghi. `DG-DR-REBUILD-05` chạy chính script này và kiểm thứ tự đó
trên primary.
