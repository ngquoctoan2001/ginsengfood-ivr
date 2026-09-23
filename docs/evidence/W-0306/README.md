# W-0306 — Lô vận hành `4` mục, kiểm từ ngoài chứ không bằng `dotnet test`

**Ngày:** `2026-09-16` · **Baseline:** `main@e8dcf45` · **Loại:** compose + cấu hình + `1` file `.cs`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

Bốn mục, tất cả đến từ **smoke test `16/09`** chứ không từ test suite — đúng lớp lỗi mà
`dotnet test` xanh không bao giờ thấy. Cả bốn đã làm, và cả bốn được **kiểm bằng cách dựng một
stack sạch rồi đo**, không phải bằng cách đọc lại code.

| # | Việc | Kiểm bằng |
| --- | --- | --- |
| 1 | `libgssapi_krb5.so.2` trong ảnh migrate | log migrate trên stack sạch: `0` lần nhắc `krb5` |
| 2 | `dev-seed/seed.sql` vào luồng chuẩn compose | `docker compose up` **không seed tay**: `6/6` hàng policy có mặt |
| 3 | Hạ mức log EF của worker | đo `120` giây, hai worker song song: `8` dòng so với `113` |
| 4 | `B5` — môi trường đã chạy `W0122` bản drop | câu SQL chạy thật trên DB sạch, trả đúng `t` |

---

## 1. Mục `1` — `libgssapi_krb5.so.2`

### 1.1. Tái lập trước khi sửa

Chạy thẳng ảnh migrate hiện có với một postgres thật:

```
Cannot load library libgssapi_krb5.so.2
Error: libgssapi_krb5.so.2: cannot open shared object file: No such file or directory
```

Hai dòng đó ra **trước** câu lệnh đầu tiên, rồi `28` migration áp bình thường. Lần chạy đầu tôi thử
với host không tồn tại thì **không** thấy lỗi — DNS hỏng trước khi Npgsql kịp thương lượng. Chi
tiết đó là thứ quyết định: lỗi chỉ hiện khi có **một postgres thật trả lời**, nên không tái lập
được bằng cách chạy khan.

### 1.2. Nguyên nhân

Npgsql mặc định `gssencmode = Prefer`, nên **mọi** kết nối thử nạp thư viện Kerberos trước. Ảnh
runtime là `aspnet:10.0-noble-chiseled` và không mang nó. Npgsql ghi hai dòng rồi **fallback và
chạy tiếp** — không hỏng gì, nhưng mở đầu mọi log bằng chữ `Error`.

### 1.3. Sửa ở đâu, và vì sao không sửa ở ảnh

Có hai cách: thêm krb5 vào ảnh, hoặc tắt thương lượng. Chọn cách thứ hai:

- **không có gì trong hệ thống này** xác thực Postgres bằng Kerberos;
- database đi qua mạng riêng của compose/cluster;
- một thư viện không bao giờ gọi là **bề mặt tấn công**, không phải tính năng — thêm nó vào ảnh
  chiseled là đi ngược đúng lý do chiseled tồn tại;
- tắt đi còn bớt một vòng thương lượng mỗi lần mở kết nối.

Đặt ở **một chỗ** trong `AddIvrFoundation` thay vì `7` chuỗi kết nối phải cùng nhớ
(`4` file `appsettings`, `2` biến môi trường compose, cộng chuỗi mà người vận hành tự cấp). Đó mới
là thứ giải quyết được vế *"môi trường mới khó dựng"*: một môi trường mới **không phải biết gì cả**.

**Vẫn để người vận hành quyết:** nếu chuỗi kết nối **đã** nêu `gssencmode` (viết kiểu nào cũng
được), code **không đụng vào**. Mặc định dịch chuyển; lựa chọn không biến mất.

### 1.4. Một chi tiết làm test đỏ, và đáng ghi

Bản đầu dùng `NpgsqlConnectionStringBuilder.ContainsKey("gssencmode")` để biết người vận hành đã
nêu hay chưa. **Sai** — `UT-BOOT-GSS-02` đỏ ngay ở hai case `Prefer`/`Require`. Npgsql chuẩn hoá
alias, nên `gssencmode` và `GSS Encryption Mode` là **cùng một thiết lập** nhưng chỉ một cách viết
trả lời `ContainsKey`. Đọc key bằng `DbConnectionStringBuilder` chung rồi bỏ khoảng trắng thì nhận
cả hai — và đó là cách **người vận hành** nhìn thấy nó.

### 1.5. Migrate bundle phải sửa riêng

`efbundle` là một executable độc lập: nó **không** nạp `AddIvrFoundation`, nên mặc định của ứng
dụng không với tới được. Vì vậy `docker-compose.dev.yml` nêu thẳng
`GSS Encryption Mode=Disable` trong `--connection` — và **chỉ** ở đó, kèm comment nói vì sao chỗ
này không dùng chung được với hai service kia.

---

## 2. Mục `2` — `seed.sql` vào luồng chuẩn

### 2.1. Vấn đề thật

`deploy/docker/dev-seed/seed.sql` có từ `W-0043`, nhưng **không gì trong luồng chuẩn chạy nó** —
chỉ `image-selftest.mjs` chạy, bằng cách tự đọc file rồi pipe vào. Nên `docker compose up` trên
volume sạch cho ra một stack mà task **được nhận rồi không bao giờ được gọi**: `ivr_attempt_policies`
không có hàng `mock-lab-v1` để `TaskIntakeService` đối chiếu snapshot trên wire.

Mọi môi trường mới phải **tự biết** mà seed tay. Smoke `16/09` đã phải làm đúng thế.

### 2.2. Cách làm

Thêm service một lần `ivr-dev-seed`, chạy **sau** `ivr-migrate` (hàng cần schema), dùng
`postgres:16` có sẵn `psql`, mount file read-only, `ON_ERROR_STOP=1`.

**Không** nhét vào migration bundle: schema không phải data, và một migration đi seed là đúng cái
giá `W-0196` đã trả. `api` và `worker` chờ seed xong, nên `docker compose up` cho ra stack mà e2e
lái được ngay.

### 2.3. Kiểm

Dựng project mới hoàn toàn (`-p w0306`, volume mới), **không seed tay một lệnh nào**:

```
ivr-dev-seed exit code: 0
BEGIN / INSERT 0 2 / INSERT 0 2 / INSERT 0 2 / DO / COMMIT

mock-e2e-silent-v1 / GOLDEN_HOUR      mock-e2e-single-v1 / GOLDEN_HOUR      mock-lab-v1 / GOLDEN_HOUR
mock-e2e-silent-v1 / TWENTY_FOUR_SEVEN  mock-e2e-single-v1 / TWENTY_FOUR_SEVEN  mock-lab-v1 / TWENTY_FOUR_SEVEN
```

`6/6` hàng có mặt. Đây là bằng chứng mà `dotnet test` không thể cho.

---

## 3. Mục `3` — mức log EF

`Microsoft.EntityFrameworkCore.Database.Command` ở `Information` ghi **mọi** câu SQL. Hạ xuống
`Warning` ở `appsettings.json` của **cả** worker và api (api ít ồn hơn chỉ vì nó truy vấn ít hơn,
không phải vì nó được cấu hình khác).

`Warning` chứ không phải `None`: câu lệnh **thất bại** vẫn ghi ở `Error`, nên vẫn thấy khi hỏng.

### 3.1. Đo, không ước

Hai worker chạy song song trên cùng database, `120` giây, khác đúng một biến môi trường:

| | dòng log | `Executed DbCommand` |
| --- | ---: | ---: |
| mặc định `W-0306` (`Warning`) | **`8`** | **`0`** |
| ép về `Information` | `113` | `10` |

`113` gồm ~`26` dòng khởi động, nên tỉ lệ ổn định là khoảng **`11×`**.

⚠️ **Nói rõ giới hạn của phép đo này:** stack đang **rảnh**, không có smoke chạy. Con số `46k`
dòng/`10` phút trong bản `16/09` là lúc **có tải**. Cái chuyển được giữa hai hoàn cảnh là **tỉ lệ**,
không phải con số tuyệt đối — và tôi không suy ra con số tuyệt đối sau khi sửa.

---

## 4. Mục `4` (`B5`) — runbook rollback

`deploy/ci/rollback.md` §3 đã nói *"hai miễn trừ drop bảng W0122 đã bị bỏ bởi W-0196"*, nhưng chưa
nói được điều người cầm một database thật cần biết.

### 4.1. Sự thật

| | |
| --- | --- |
| Bản **drop** sống trong repo | `ec3b5ca` (`2026-08-28`) → `c8dc3c4` (`2026-09-05`) |
| Hai bảng bị xoá | `ivr_console_sessions`, `ivr_console_accounts` |
| Nơi tạo chúng | `20260822120000_W0105ConsoleAccountAuth` |
| `Down()` | no-op — **không dựng lại được**, dữ liệu đã mất |

Hai database cùng khai `__EFMigrationsHistory` giống hệt nhau vẫn có thể khác schema. Đó là lý do
mục này tồn tại.

### 4.2. Không chép danh sách — đưa câu hỏi

Kế hoạch ghi *"thêm **danh sách môi trường** đã chạy `W0122` bản drop"*. Tôi **không** làm đúng chữ
đó, và đây là lý do:

- **không có cluster thật** (`W-0061`/`W-0063` `BLOCKED_EXTERNAL`, ghi ngay đầu runbook);
- CI dựng database rỗng mỗi lần, không volume nào sống qua đêm;
- còn lại là các stack `docker compose` local và máy smoke — trạng thái của chúng phụ thuộc vào
  việc **ai đó có `down -v` hay không**, và việc đó không được ghi ở đâu cả.

Một danh sách chép tay sẽ sai ngay lần tiếp theo có người dựng thêm một stack. Nên runbook đưa
**câu SQL trả lời dứt khoát**, cộng bảng môi trường đã biết với hai dòng ghi thẳng là *"có thể"*
thay vì đoán:

```sql
SELECT to_regclass('public.ivr_console_accounts') IS NOT NULL
   AND to_regclass('public.ivr_console_sessions') IS NOT NULL AS console_tables_present;
```

`false` ⇒ đã chạy bản drop. Phép thử đứng được vì **không lối nào khác** làm hai bảng đó biến
mất: chỉ `W0122` bản cũ drop chúng, và không migration nào sau đó tạo lại.

### 4.3. Kiểm

Chạy trên database sạch vừa dựng ở mục `2`: trả **`t`** — đúng như tài liệu nói một database mới
phải cho. Nếu `W0105` không còn tạo hai bảng đó thì phép thử vô nghĩa và câu này sẽ ra `f`.

### 4.4. Và một lời khuyên ngược

Runbook nói rõ: gặp `false` thì **đừng dựng lại hai bảng**. `W-0128` đã gỡ toàn bộ hệ thống tài
khoản console khỏi code, và không gì trong bản ship đọc chúng. Thiếu chúng **không** phải lỗi đang
chạy — nó là **cửa sổ rollback bị thu hẹp**, và cách đúng là ghi nhận, không phải `CREATE TABLE`
một cấu trúc rỗng để một truy vấn nào đó đọc ra `0` hàng rồi tưởng đó là sự thật.

---

## 5. Kiểm chứng

```
dotnet build                 0 Warning(s)  0 Error(s)
Ivr.UnitTests                760/760  (+4: UT-BOOT-GSS-01, UT-BOOT-GSS-02 ×3 case)
generate-test-traceability   697 (sinh lại; --check CURRENT)
docs-selftest                API_DOCS_SELFTEST_PASS
compliance-pack-selftest     PASS      ci-config-selftest     PASS
review-gate-selftest         PASS      progressive-selftest   PASS
gate-status                  GATE_STATUS_PASS — 11 gates, 6 open decisions
quét pin toàn cây            44 path ghim hash · 0 lệch · 0 file thiếu
gitnexus_impact              AddIvrFoundation: LOW, 0 impacted, 0 process
gitnexus_detect_changes      risk medium, 2 process (cả hai là chính AddIvrFoundation)
image-selftest               IMAGE_SELFTEST_PASS  (IVR_POSTGRES_PORT=55633, xem §5.2)
```

`image-selftest` là bằng chứng mạnh nhất cho mục `2`: log của nó cho thấy `ivr-dev-seed` được dựng, chạy và dọn **bên trong stack của chính gate**, nghĩa là service mới đi qua được toàn bộ luồng e2e chứ không chỉ lần `compose up` thủ công của tôi.

### 5.1. Mutation cả hai chiều

| Mutation | Kết quả |
| --- | --- |
| Bỏ `WithoutGssNegotiation(...)` khỏi `UseNpgsql` | `UT-BOOT-GSS-01` **đỏ** |
| `ContainsKey("gssencmode")` (bản sai đầu tiên) | `UT-BOOT-GSS-02` **đỏ** ở `Prefer`/`Require` |
| Ép `EF Database.Command=Information` trên worker thật | `10` câu SQL xuất hiện trong `120` giây |

### 5.2. `image-selftest` — vì sao lần đầu đỏ, và nó **không** phải lỗi của lượt này

Lần chạy đầu đỏ ở `docker compose up`. Nguyên nhân là **xung đột cổng**: `image-selftest.mjs` dùng
mặc định `55433`, và một stack `ginsengfood-ivr-dev` **có sẵn từ trước lượt này** đang giữ cổng đó.
Cùng lý do đã làm lần `docker compose up` đầu của tôi đỏ. Chạy lại với `IVR_POSTGRES_PORT=55633`.

Ghi ra đây vì đây đúng là chỗ dễ đổ lỗi nhầm: một gate đỏ ngay sau khi mình sửa compose **trông**
như hậu quả của thay đổi, và nó không phải.

---

## 6. Đính chính bản ghi của chính tôi

`docs/evidence/W-0304/README.md` §7.3 viết *"`0` pin lệch trên **`183`** [path] ghim hash"*.
**`183` sai** — đó là số path *ứng viên chuẩn hoá CRLF*, không phải số pin thật. Số đúng,
đo lại bằng chính script ở lượt này, là **`44`**. Kết luận `0` lệch không đổi, nhưng con số lớn
hơn thực tế `4` lần làm phạm vi kiểm tra nghe rộng hơn nó thật. Đã sửa bằng một dòng đính chính
**hiện trên mặt tài liệu**, không phải bằng cách lặng lẽ đổi số.

---

## 7. Còn lại

| Việc | Vì sao chưa |
| --- | --- |
| `image-selftest` cố định cổng `55433`/`58080` | là nợ riêng: nó làm gate không chạy được song song với một stack dev đang bật. Không sửa trong lượt này vì ngoài phạm vi `4` mục, nhưng đáng một Work ID |
| Trần kênh là **ba con số ở ba nơi**, không gate nào so | phát hiện của phiên PD-03; cần owner quyết |
| Kênh non-`MOCK` không enable lại được qua API | như trên — endpoint thuộc `AdminPolicies.Danger` |

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0306 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Lô vận hành 4
mục từ smoke 16/09. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: nợ cổng image-selftest (cố định cổng) đã ghi riêng. Mọi giới hạn
trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442`
(1200/1200 test, sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới.
REAL_CUSTOMER_CALL_ALLOWED=NO.
