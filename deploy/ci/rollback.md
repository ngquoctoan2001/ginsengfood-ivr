# Rollback — `W-0045` · `P7-3` §8

> **Cluster rollback chưa chạy.** P0.3 có quy trình diễn tập hai binary trên bản sao PostgreSQL local,
> xem [W-0196](../../docs/evidence/W-0196/README.md) và [expand-contract](../../docs/database/expand-contract.md).
> Không có runner, registry hay credential cluster cho môi trường thật
> (`W-0061`, `W-0063` — cả hai `BLOCKED_EXTERNAL`). Đây là quy trình đã viết, **không phải** quy
> trình đã diễn tập. `P7-3` §10 nói rõ YAML và mô phỏng local không được gọi là deploy proof, và
> tài liệu này không tự nhận khác.

## 1. Ba lớp rollback, và lớp nào tự động

| Lớp | Kích hoạt | Ai làm |
| --- | --- | --- |
| `helm --atomic` | rollout không khoẻ trong `--timeout` | **tự động**, ngay trong lệnh upgrade |
| `after_script` của job | job thất bại **sau** khi upgrade trả về (ví dụ smoke đỏ) | **tự động**, trong CI |
| `rollback_prod` | quyết định của người sau khi đã deploy xong | **thủ công**, protected environment |

Hai lớp đầu tồn tại vì chúng bắt hai loại hỏng khác nhau. `--atomic` bắt "rollout không lên nổi".
`after_script` bắt trường hợp nguy hiểm hơn: **Kubernetes coi rollout là khoẻ trong khi dịch vụ trả
lời sai** — pod `Ready`, probe xanh, nhưng smoke sau deploy đỏ.

## 2. Rollback thủ công

```bash
helm history ivr --namespace ivr-prod --max 10
helm rollback ivr <REVISION> --namespace ivr-prod --wait --timeout 10m
kubectl -n ivr-prod rollout status deploy/ivr-ivr-api --timeout=5m
```

`<REVISION>` **phải nêu rõ**. `helm rollback` không tham số lùi đúng một bước, mà "một bước" là
tương đối với lịch sử tại thời điểm đó — nếu có hai lần deploy chồng nhau thì nó không lùi về chỗ
người vận hành đang nghĩ tới.

## 3. Migration **không** lùi cùng release

Đây là điều quan trọng nhất trong tài liệu này.

`helm rollback` đưa **manifest** về revision cũ. Nó **không** hoàn tác migration mà Job
`pre-upgrade` đã áp — schema đã đổi rồi. Lùi image về bản cũ trong khi schema đã tiến lên nghĩa là
chạy code cũ trên schema mới.

Vì vậy migration phải **tương thích lùi một bậc**: thêm cột nullable, không đổi tên và không xoá cột
trong cùng một release với code dùng nó. Đây là ràng buộc lên **cách viết migration**, không phải
lên rollback. Ghi ở đây vì đó là cái bẫy mà rollback tự động dễ làm người ta quên.

Bản đầu của tài liệu này (`W-0045`) kết đoạn trên bằng *"và nó chưa có test nào ép"*. Câu đó
**hết đúng từ `W-0046`**, khi `IT-MIGRATE-03` trong `progressive-selftest.mjs` bắt đầu quét mã
nguồn migration, và không ai quay lại sửa. Ghi lại ở đây thay vì lặng lẽ xoá: một tài liệu tự nhận
"chưa có gì canh" trong khi đã có là loại sai nguy hiểm hơn im lặng.

`W-0114` mở rộng phần đó. `UT-SCHEMA-BACKCOMPAT-01` đọc `Up` của **mọi** migration qua
`Migration.UpOperations` — mô hình thao tác có kiểu, không phải văn bản — và từ chối bảy dạng mà
release liền trước không sống nổi: xoá cột, xoá bảng, đổi tên cột, đổi tên bảng, thêm cột
`NOT NULL` không default, `AlterColumn` **thu hẹp** (siết nullable, rút ngắn, đổi kiểu), và thêm
ràng buộc mới (unique hoặc `CHECK`) lên cột **đã có từ trước**.

Ba dạng cuối là thứ `IT-MIGRATE-03` không thấy. Ngược lại, `IT-MIGRATE-03` chạy trong image node
không cần .NET nên đỏ sớm hơn và rẻ hơn — nên **cả hai đều giữ**. Khác biệt duy nhất về phán quyết
là `AlterColumn`: quét văn bản thấy lời gọi chứ không thấy tham số nên phải chặn mọi `AlterColumn`,
còn đọc thao tác thì phân biệt được nới rộng (an toàn theo chiều này) với thu hẹp.

Cổng có các miễn trừ constraint được giải thích trong test; hai miễn trừ drop bảng W0122 đã bị bỏ
bởi W-0196. Raw SQL cũng bị kiểm tra, kể cả SQL sinh qua helper. Hai migration SQL trước baseline
W0118 được ghim nguyên nội dung lịch sử, không được xem là upgrade rolling đã được chứng minh.
Release expand hiện hành không có miễn trừ drop bảng; cleanup là release riêng sau inventory
consumer, quan sát triển khai và đóng cửa sổ rollback. Xem runbook W-0196 phía trên.

### 3b. `W0122` — những database đã chạy bản **drop**, và cách tự xác định (`B5`, `W-0306`)

Đoạn trên nói *"hai miễn trừ drop bảng W0122 đã bị bỏ bởi W-0196"*. Đúng, nhưng chưa đủ cho người
đang cầm một database thật: `Up()` của `20260828040458_W0122DropConsoleAccounts` **đã từng xoá bảng
thật**, rồi được viết lại thành no-op. `Down()` cũng là no-op và **không dựng lại gì** — nó không
thể, dữ liệu đã mất. Vì vậy sự khác biệt này **vĩnh viễn**, và hai database cùng báo
`__EFMigrationsHistory` giống hệt nhau vẫn có thể khác schema.

| | |
| --- | --- |
| Bản **drop** sống trong repo | `ec3b5ca` (`2026-08-28`) → `c8dc3c4` (`2026-09-05`) |
| Hai bảng bị xoá | `ivr_console_sessions`, `ivr_console_accounts` |
| Nơi tạo chúng | `20260822120000_W0105ConsoleAccountAuth` |

**Đừng tra danh sách — hỏi chính database.** Một danh sách môi trường chép tay sẽ sai ngay lần
ai đó dựng thêm một stack mà không sửa tài liệu. Câu dưới trả lời dứt khoát **chỉ với database chưa áp
`P03`**; đọc đính chính `25/09` ngay sau bảng kết quả trước khi dùng nó:

```sql
SELECT to_regclass('public.ivr_console_accounts') IS NOT NULL
   AND to_regclass('public.ivr_console_sessions') IS NOT NULL AS console_tables_present;
```

| Kết quả | Nghĩa | Hành động |
| --- | --- | --- |
| `true` | **chỉ đọc được khi database chưa áp `P03`** (xem ô ngay dưới): khi đó database này **chưa bao giờ** chạy bản drop | không phải làm gì |
| `false` | đã chạy bản drop trong cửa sổ `8` ngày ở trên | **không tự dựng lại bảng**; xem ô dưới |

> **Sửa `25/09` (`W-0354`, mục `B5` trong danh sách của chief) — phép thử trên chỉ đúng với database chưa áp
> `P03`.** Bản trước ghi *"không migration nào sau đó tạo lại"*. Câu đó sai: `20260905120000_P03PreserveConsoleCompatibility`
> (cùng commit `c8dc3c4` viết lại `W0122`) chạy `CREATE TABLE IF NOT EXISTS` cho cả hai bảng. Một database đã
> drop trong cửa sổ rồi nâng lên từ `c8dc3c4` trở đi sẽ có lại hai bảng **rỗng**, và câu SQL trên báo `true` —
> đúng hành vi `IT-SCHEMA-EXPAND-07` giữ ở nhánh `dropAlreadyApplied`. Vì vậy phải hỏi thêm một câu trước:
>
> ```sql
> SELECT EXISTS (SELECT 1 FROM "__EFMigrationsHistory"
>                WHERE "MigrationId" = '20260905120000_P03PreserveConsoleCompatibility') AS p03_applied;
> ```
>
> | `p03_applied` | Đọc thế nào |
> | --- | --- |
> | `false` | Phép thử `to_regclass` ở trên dùng được, đọc theo bảng kết quả ngay trên |
> | `true` | Schema **không** phân biệt được. Chỉ một chiều còn đọc được: `ivr_console_accounts` **có hàng** ⇒ chưa từng drop, vì `P03` tạo bảng rỗng và từ `W-0128` không code nào ghi vào bảng này. **Không có hàng** ⇒ **không xác định được từ schema**. Khi đó dựa vào nhật ký triển khai hoặc backup từ trước `c8dc3c4`; không có thì ghi đúng câu "không xác định được từ schema", đừng đoán |
>
> *Sửa lại `25/09` (`W-0356`, mục `K-20`):* ở đây từng ghi database dựng mới từ `c8dc3c4` trở đi *"rơi vào dòng
> `true` với bảng rỗng, và như vậy là đúng"*. Đọc theo chính bảng trên thì không: database đó có
> `p03_applied = true` và bảng rỗng, tức rơi vào ô **không xác định được từ schema**. Nó chưa từng chạy bản
> drop, nhưng điều đó biết được từ lần migrate đầu tiên của nó (nhật ký triển khai: lần đầu đã ở `c8dc3c4` trở
> đi), không từ schema. Việc phân biệt chỉ quan trọng với database sống qua cửa sổ `8` ngày. Diễn tập trên
> database thật vẫn chờ có môi trường (`W-0063`).

**Môi trường đã biết, tính đến `2026-09-16`:**

| Môi trường | Database có sống qua cửa sổ không | Ghi chú |
| --- | --- | --- |
| Cluster thật | **không tồn tại** | `W-0061`/`W-0063` `BLOCKED_EXTERNAL`; không có runner/registry/credential — xem đầu tài liệu này |
| CI | **không** | Testcontainers và `image-selftest` dựng database rỗng mỗi lần chạy; không có volume nào sống qua đêm |
| `docker compose` local | **có thể** | chỉ khi volume `ivr-postgres-data` được tạo và migrate trong cửa sổ đó và chưa `down -v` lần nào kể từ đó |
| Máy smoke `audit-0907` | **có thể** | volume dài ngày, migrate nhiều đợt, cả sau `c8dc3c4` — hỏi `p03_applied` trước, rồi đọc theo bảng hai chiều ở đính chính `25/09` |

Hai dòng cuối ghi *"có thể"* chứ không ghi có/không, và đó là có chủ ý: trạng thái của chúng phụ
thuộc vào việc ai đó có `down -v` hay không, mà việc đó không được ghi lại ở đâu cả. **Chép một
câu trả lời vào đây sẽ là bịa.** Hai câu SQL mất vài giây; khi chúng không trả lời được thì ghi đúng như vậy.

> **Nếu gặp `false`: đừng dựng lại hai bảng đó.** Không có code nào trong bản ship đọc chúng —
> `W-0128` đã gỡ toàn bộ hệ thống tài khoản console khỏi code, database và tài liệu, và Module 3 sở
> hữu identity nhân viên. Hai bảng giữ lại **chỉ** để replica cũ và cửa sổ rollback sống được, đúng
> như comment trong `Up()` nói. Một database thiếu chúng vẫn chạy đúng với bản ship hiện tại; cái
> nó **không** làm được là quay về release trước `W-0128`. Đó là về **thu hẹp cửa sổ rollback**, không
> phải lỗi đang chạy — và cách đúng là ghi nhận nó, không phải `CREATE TABLE` một cấu trúc rỗng
> để một truy vấn nào đó đọc ra `0` hàng và tưởng đó là sự thật.
>
> **Ghi kết quả vào nhật ký triển khai trước khi nâng cấp** *(`W-0356`, mục `K-20`)*. Một database báo `false`
> là database chưa áp `P03`. Lần migrate kế tiếp sẽ tự chạy `P03`, tạo lại hai bảng rỗng, và từ đó câu SQL báo
> `true`: dấu vết duy nhất của bản drop mất ở chính lần nâng cấp đó.

## 3a. Chiều còn lại — code mới trên schema cũ

Chiều ngược lại xảy ra ở khoảng giữa lúc Job `pre-upgrade` bắt đầu và lúc nó xong, và ở mọi lần
deploy bỏ hook hoặc trỏ nhầm database chưa migrate. Ở đó binary mới **không thể** đọc schema cũ —
EF gọi tên cột chưa tồn tại nên câu đọc đầu tiên là `42703 undefined_column`. Điều phải giữ không
phải "vẫn chạy" mà là **hỏng đúng một dạng mà rolling deploy sống được**:

| Phải | Vì sao |
| --- | --- |
| `/health/ready` trả `503` `schema_behind` | pod báo Healthy ở đây sẽ nhận traffic rồi trả lỗi cho người gọi |
| `/health/live` và `/health/startup` vẫn `200` | liveness đỏ làm **crash-loop** mọi replica; `--atomic` cần rollout **đứng**, không cần nó giãy |
| readiness tự xanh lại khi migration xong, **không cần restart** | API không được báo là hook đã xong; nếu readiness chốt cứng thì rollout đứng vì một schema đã đúng |

`IT-SCHEMA-NEWCODE-01` dựng schema tiến từ rỗng đến migration liền trước — không lùi bằng `Down`,
vì schema mà deploy thật gặp là schema do release trước dựng lên — rồi khởi động **đúng entry point
của bản ship** trên đó. `IT-SCHEMA-NEWCODE-02` khoá tiền đề: đúng một bậc, bậc đó là migration mới
nhất, và migration đó có thao tác thật.

## 4. Evidence không bị rollback

`ci-artifacts/cd/` giữ digest và effective values của lần deploy **đã xảy ra**, kể cả khi nó bị lùi.
Rollback là một sự kiện mới, không phải một cái tẩy: hồ sơ phải trả lời được "lúc đó cái gì đang
chạy", chứ không chỉ "bây giờ cái gì đang chạy".

## 5. Sau khi rollback

1. Ghi lại revision đã lùi về và lý do.
2. Kiểm `helm get values ivr -n <ns>` xác nhận `realCustomerCallAllowed: false` vẫn giữ — rollback
   đưa về **cấu hình cũ**, và cấu hình cũ cũng phải nằm ở đáy ladder.
3. Kiểm alert `IvrDownstreamFailClosedSpike` và `IvrCallbackRevalidateLatencyBreach` (`W-0041`) đã
   tắt chưa; nếu chưa thì rollback chưa giải quyết được nguyên nhân.
