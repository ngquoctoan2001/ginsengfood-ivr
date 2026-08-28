# Progressive delivery — `W-0046` · `P7-4`

> **Chưa lần nào chạy.** Argo Rollouts chưa cài, và **không có Prometheus nào nhận metric của IVR**
> (`W-0063`, `BLOCKED_EXTERNAL`). Đây là chiến lược đã viết và cấu hình đã kiểm hình dạng, **không
> phải** một canary đã diễn ra. `P7-4` §10 đòi "canary run + auto-rollback demo"; cả hai `NOT_RUN`.

## 1. Hai chiến lược, vì hai thứ khác nhau

| Deployable | Chiến lược | Lý do |
| --- | --- | --- |
| `ivr-api` | **canary** 10% → 50% → 100%, gate theo SLO | stateless, theo request; hai phiên bản phục vụ hai request khác nhau là trạng thái bình thường |
| `ivr-worker` | **blue-green**, đổi service nguyên tử | xem §3 |
| `admin-ui` | `NOT_DEPLOYED_BY_IVR` | W-0128: reference local; Module 3 sở hữu UI/identity/deployment |

## 2. Canary API — cổng SLO chính là ngưỡng paging

| Chỉ số | Ngưỡng | Nguồn |
| --- | --- | --- |
| callback revalidate p95 | ≤ **5s** | D-04 |
| fail-closed ratio | ≤ **20%** | DO-06, đề xuất |
| callback retry exhausted | **0** | ARCH-06 §4 |

Ngưỡng latency **cố ý bằng đúng** ngưỡng alert của `P6-2`. Một cổng canary **lỏng hơn** ngưỡng
paging sẽ promote một phiên bản rồi lập tức đánh thức người trực — canary khi đó không giảm rủi ro,
nó chỉ dời rủi ro sang ca đêm.

Bất đối xứng có chủ đích: **năm** lần đọc tốt mới promote, **một** lần đọc xấu là abort. Promote là
chiều nguy hiểm.

Mọi `pause` đều có `duration`. Pause vô hạn nghĩa là chờ người, và một canary không ai nhìn sẽ nằm ở
10% cho tới khi có người tình cờ nhận ra.

## 3. Vì sao worker **không** canary — và lý do không phải là tính đúng đắn

Canary nghĩa là hai phiên bản chạy song song **suốt thời gian analysis** — mười, hai mươi phút. Với
API stateless đó là bình thường. Với worker, đó là hai phiên bản scheduler cùng tranh các call job
đến hạn trong suốt cửa sổ đó.

Advisory lock và lease fencing (P2-3) làm việc đó **an toàn**: `IT-SCH-CLAIM-01` chứng minh hai
worker cùng claim một job tạo ra **đúng một** attempt và **một** channel lease. Nên rủi ro ở đây
**không phải** tính đúng đắn.

Rủi ro là: một cửa sổ chồng lấn dài làm mọi lỗi lập lịch của phiên bản mới **dễ bị quan sát đúng lúc
khó quy trách nhất** — hai phiên bản sinh attempt xen kẽ nhau là trạng thái tệ nhất để debug.

Blue-green thu cửa sổ đó về **một lần đổi service**. Bất biến giữ cho nó an toàn vẫn là cùng cái
lock; lựa chọn ở đây là **một trạng thái khó hiểu được phép tồn tại bao lâu**, không phải liệu tính
đúng đắn có giữ hay không.

`autoPromotionEnabled: false` vì worker **không mở socket nào** (`W-0043` §2) — không có gì để smoke,
và promote tự động dựa trên "pod đã lên" là khẳng định một sức khoẻ không ai đo.

## 4. Migration expand-contract — cổng thật duy nhất của slice này (`W-0046`)

`P7-3` §5 đã nêu cái bẫy: `helm rollback` đưa **manifest** về revision cũ nhưng **không** hoàn tác
migration. Lùi image cũ trên schema mới nghĩa là chạy **code cũ trên schema mới**. Canary còn làm nó
thường trực: trong suốt cửa sổ, code cũ và code mới **cùng** chạy trên **một** schema.

Nên ràng buộc nằm ở **cách viết migration**, và giờ có cổng ép:

| Cấm trong `Up()` | Vì sao |
| --- | --- |
| `DropColumn`, `DropTable` | phiên bản cũ vẫn `SELECT` cột đó |
| `RenameColumn`, `RenameTable` | tương đương xoá rồi thêm, với phiên bản cũ |
| `AlterColumn` | thu hẹp kiểu làm hỏng ghi của phiên bản cũ |
| cột `nullable: false` **không** `defaultValue` | schema nhận, rồi `INSERT` của phiên bản cũ — vốn không biết cột đó — hỏng lúc chạy |

`Down()` **không** bị kiểm: xoá thứ `Up()` vừa tạo chính là định nghĩa của down migration, và một
cổng bắt cả nó sẽ đỏ với **mọi** migration từng viết rồi bị tắt trong một tuần.

Trạng thái lúc `W-0046` viết cổng: 5 migration, 42 `AddColumn` trong `Up()`, **không** thao tác phá
nào, **không** cột NOT NULL nào thiếu default. Cổng đang xanh vì code đang đúng, không phải vì cổng
dễ — và vẫn xanh ở 12 migration.

`W-0114` bổ sung `UT-SCHEMA-BACKCOMPAT-01`: cùng tính chất, nhưng đọc `Migration.UpOperations` thay
vì quét văn bản. Nó thấy thêm ba dạng bảng trên không có (`AddUniqueConstraint`, unique index,
`AddCheckConstraint` lên cột đã có từ trước) và phân biệt `AlterColumn` **nới rộng** — an toàn theo
chiều này — với **thu hẹp**. Cổng ở đây vẫn giữ vì nó chạy trong image node không cần .NET nên đỏ
sớm hơn; xem `deploy/ci/rollback.md` §3.

## 5. Deploy ≠ release

Bật tính năng đi qua feature flag (P0-4), **không** qua phần trăm rollout. Nếu một rollout bật được
tính năng thì hai việc lại là một sự kiện, và feature flag chỉ còn là tài liệu.

`IT-FLAG-RAMP-04` đỏ nếu bất kỳ file rollout nào chạm tới feature flag.

Và **sàn governance đi cùng canary**: cả hai rollout ghim `IVR_EXECUTION_MODE=MOCK` và
`REAL_CUSTOMER_CALL_ALLOWED=NO` trên phiên bản mới. Một canary mang tư thế governance khác bản
stable sẽ làm ladder **phụ thuộc vào việc pod nào trả lời request** — mở real call vẫn phải qua cổng
riêng của `P9-1`.

## 6. Cái này KHÔNG chứng minh

- **Chưa canary nào chạy**, chưa auto-rollback nào diễn ra. Argo Rollouts chưa cài; không Prometheus
  nào nhận metric (`W-0063`).
- **Ngưỡng fail-closed 20% là đề xuất**, chưa có baseline production để hiệu chỉnh.
- **Chưa diễn tập blue-green switch** trên cluster thật.
- ~~**Chưa cổng nào ép chiều ngược lại** (code mới gặp schema cũ).~~ **Đã đóng `2026-08-19`**
  (`W-0046` residual): `/health/ready` giờ đọc `GetPendingMigrationsAsync` và trả `schema_behind`
  → 503. Chart chạy migration bằng pre-upgrade hook nên đường hạnh phúc vốn được **thứ tự** che;
  cái không được che là deploy bỏ hook, hook bị tắt, hoặc ai đó trỏ vào database chưa migrate —
  cả ba cho ra một pod **kết nối được**, báo Healthy, nhận traffic, rồi hỏng ở truy vấn đầu tiên
  vào một bảng không tồn tại. `IT-OBS-HEALTH-04` có kiểm âm: tắt phép kiểm → đỏ.
- **Chưa chứng minh hai phiên bản chạy song thật.** `IT-MIGRATE-03` chứng minh migration **cho phép**
  điều đó; nó không chứng minh đã có ai chạy hai phiên bản cùng lúc.
