# W-0055 — Evidence: Analytics / BI pipeline (`P10-4`)

Ngày: `2026-08-19` · Trạng thái: **`TESTS_PASS`** — 4/4 test §8 xanh, mỗi cái có kiểm âm;
**không có kho dữ liệu riêng** (`W-0063`) và **không có BI tool nào từng kết nối**

## 1. Điều phải nói trước

Đây **không phải** một kho dữ liệu. Nó là một **schema PostgreSQL** (`analytics`) nằm trong **cùng
database** với bảng vận hành. Không có cluster nào để dựng (`W-0063` vẫn `BLOCKED_EXTERNAL`), và
dựng một datastore thứ hai mà không ai cấp được sẽ cho ra một pipeline chưa từng chạy — đúng thứ
`W-0041`/`W-0045` đã phải ghi ra ba lần.

Phần **thật hôm nay** là ranh giới quyền: một BI tool cấp `SELECT` trên `analytics` đọc được toàn bộ
KPI mà **không chạm được bảng vận hành nào**. Grant đó là thứ *cấp được*; chưa ai cấp.

Và `W-0098` đã tự khai khoảng trống này: mọi payload analytics ghi `warehouse_backed=false` kèm
`pipeline_work_id=W-0055`. Slice này đóng đúng khoảng trống nó tự chỉ ra.

## 2. Đặt ở đâu, và vì sao không phải project riêng

`P10-4` §7 gợi ý `src/Ivr.Analytics/**`. Không làm vậy, vì **DTS-04 chốt đúng ba deployable**
(`ivr-api`, `ivr-worker`, `ivr-admin-ui`). Thêm một project chạy được là thêm một deployable thứ tư
hoặc một thư viện chỉ Infrastructure/Worker dùng — mà cái thứ hai thì **thuộc về** Infrastructure.

Nên ETL nằm ở `src/Ivr.Infrastructure/Analytics/` và chạy trong worker. Lệch với §7 là **cố ý**, ghi
ra đây thay vì để người đọc tưởng quên.

## 3. Ranh giới privacy: hai loại ID, xử lý khác nhau

| Loại | Xử lý | Vì sao |
| --- | --- | --- |
| ID nội bộ IVR (`ivr_call_result_id`, `ivr_call_job_id`) | giữ nguyên | định danh **việc** của IVR, không định danh người; admin console đã hiện |
| ID đơn của Sales | chỉ `order_ref_hash` (SHA-256) | khoá nghiệp vụ dẫn về khách trong hệ thống IVR **không sở hữu** |

Băm một cái trong khi cái kia vẫn đọc được là **trông như** bảo vệ chứ không bảo vệ gì. Chỗ duy nhất
join ngược là lối tái định danh thật là mã đơn — nên đó là chỗ duy nhất được băm.

`order_ref_hash` là **bí danh, không phải ẩn danh**: ai cầm mã đơn vẫn xác nhận được nó có trong tập.
Ghi ra vì đọc nhầm thành "đã ẩn danh" sẽ dẫn tới kết luận sai về mức chia sẻ được.

Ép bằng **hai lớp hỏng vì hai lý do khác nhau**: lớp cấu trúc đọc **model EF** và đòi mọi cột nằm
trong allowlist đã rà; lớp giá trị chạy `PiiGuard` trên từng chuỗi thực sự được ghi. Dòng bị lớp 2
từ chối được **bỏ và đếm** — số đếm nằm trên checkpoint nên một lần bỏ im lặng không thể bị nhầm với
nguồn rỗng.

## 4. Cổng bắt lỗi của chính tôi ngay lần chạy đầu

`BI-PII-01` đỏ ngay lượt đầu tiên, với **hai cột do chính tôi vừa định nghĩa**:

- `fact_call_outcome.is_counted_customer_attempt` — chứa `customer`
- `agg_kpi_daily.invalid_phone_count` — chứa `phone`

Cả hai đều **đúng**: cái thứ nhất là boolean về kế toán attempt (DT-02), cái thứ hai là **số đếm**
một result type. Đây đúng lớp lỗi `W-0100` đã gặp — khi đó một rule substring gắn cờ nhầm
`invalid_phone` và cách xử lý là **bỏ khớp substring**.

Ở đây tôi chọn hẹp hơn có chủ ý: khớp substring là thứ bắt được `customer_id` trong một schema không
ai đọc lại, nên **luật giữ nguyên và ngoại lệ trở thành có tên, có khoá, có lý do** thay vì luật yếu
đi. Ngoại lệ khoá theo `table.column`, nên một cột khác mang cùng mảnh tên vẫn đỏ. Và một test riêng
đòi mỗi ngoại lệ phải trỏ tới **một cột có thật** kèm lý do đủ dài — một danh sách ngoại lệ sống lâu
hơn cột của nó là giấy phép thường trực cho bất cứ thứ gì được đặt tên đó sau này.

## 5. Quyết định thiết kế, mỗi cái vì một cách hỏng cụ thể

| Quyết định | Vì cách hỏng nào |
| --- | --- |
| **anti-join** theo khoá tự nhiên, **không watermark thời gian** | hai transaction lấy timestamp thứ tự này, commit thứ tự kia → dòng có `created_at` đã sau watermark vẫn xuất hiện sau khi watermark đi qua, và **không ai đọc lại, không ai báo thiếu** |
| aggregate **tính lại**, không cộng dồn | cộng dồn nhân đôi ngay lần đầu có gì chạy hai lượt — với pipeline mà hợp đồng là idempotency, đó là lỗi phải **bất khả** chứ không phải được test |
| checkpoint **không** tham gia tính đúng | xoá nó tốn một lượt chạy chậm, không mất một dòng fact |
| **hai hạt** (result + job) | job chưa có kết quả không có dòng ở hạt result; giữ một hạt thì `total_call_jobs` phải lấy từ bảng vận hành và payload vừa nói `warehouse_backed=true` vừa lấy một nửa ở nơi khác |
| job **mở** được refresh, job **đóng** thì không | attempt tích luỹ sau khi fact được ghi; insert-only sẽ đóng băng số attempt lần đầu nhìn thấy và `attempt_2_rate` thấp mãi mãi |
| lưu **tổng + số đếm**, không lưu trung bình | trung bình không cộng được; lưu trung bình làm hỏng lặng lẽ mọi roll-up của BI tool |
| bucket không có cuộc nào kết thúc trả `null` | `0` là một phép đo, "chưa có gì kết thúc" thì không |
| retention theo **phụ thuộc**, không theo chu kỳ thứ hai | hai chu kỳ đặt lệch nhau được; một phụ thuộc thì không |
| `source` **không** là enum, `warehouse_status` **là** enum | console *in* `source` và *rẽ nhánh* theo `warehouse_status`; enum ở chỗ thứ nhất chỉ tạo ra một client cũ không đọc nổi payload nêu nguồn nó chưa biết |
| ETL **bật mặc định** | khác CronJob retention của `W-0044`: cái đó **không hoàn tất được** nếu chưa sửa code, còn cái này không có phụ thuộc ngoài — nên hỏng kiểu "tắt sẵn" mới là kiểu cắn: warehouse rỗng, API lặng lẽ đọc vận hành, không đâu nói pipeline chưa chạy |

## 6. Warehouse thắng kể cả khi backlog

Warehouse phục vụ **bất cứ khi nào** nó có fact, **kể cả** khi reconcile báo `BACKLOG`. Rơi về đọc
vận hành lúc đó sẽ **đổi nguồn ngay giữa sự cố**: cùng một câu hỏi trả về hai đáp án cách nhau vài
phút mà payload không có gì giải thích.

Nên `data_quality` mang **hai khẳng định tách rời**: `source` (kho nào trả lời) và `warehouse_status`
(pipeline đã đủ chưa). Backlog là dữ liệu kém **ghi nhãn trung thực**; đổi nguồn im lặng là dữ liệu
kém **trông ổn**.

## 7. Kiểm chứng

| Test | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `BI-PII-01` | thêm cột `contact_phone_number` vào schema `analytics` | ❌ đỏ, **nêu đúng tên cột** |
| `BI-PII-01` | (chạy thật, không dựng) hai cột của chính tôi | ❌ đỏ lượt đầu → §4 |
| `BI-KPI-02` | kỳ vọng **đếm tay** từ fixture, không tính lại bằng chính biểu thức của `Fold` | xanh |
| `BI-IDEMP-03` | thay anti-join bằng watermark `CreatedAt > max(EventAt)` | ❌ đỏ **đúng một test**: dòng tới muộn biến mất |
| `BI-IDEMP-03` | bỏ pass refresh job (insert-only) | ❌ đỏ đúng một test |
| `BI-QUALITY-04` | retention hook không tính lại bucket | ❌ đỏ đúng một test |

Mỗi kiểm âm làm đỏ **đúng một** test — tức mỗi test đang khẳng định đúng thứ nó nói, không phải một
tập chồng lấn.

**Cổng PII chặn chính tài liệu này một lần**, và là **dương tính giả**: một từ tiếng Việt vừa nghĩa
"phố" vừa nghĩa "lối". Cùng lớp lỗi `A-0190`, và cách xử lý giữ nguyên: **sửa từ ngữ của mình, không
nới pattern** — `W-0076` chọn literal byte alternation để độc lập locale, nới ra là đánh đổi một tính
chất đã chứng minh lấy một tiện lợi hình thức.

`BI-KPI-02` có một khẳng định đáng nêu riêng: trung bình của hai trung bình (`240` và `60` → `150`)
**khác** trung bình ghép từ tổng (`1740/8 = 217.5`). Test đòi cả hai, nên nếu ai đó "đơn giản hoá"
sang lưu trung bình thì nó đỏ kèm con số chứng minh vì sao.

| Lệnh | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -c Release` | 0 warning / 0 error |
| `dotnet test Ivr.sln -c Release` | **413/413** — 22 contract + 235 unit + 5 chaos + 151 integration, 0 fail |
| `oasdiff breaking` (container ghim digest) | **No breaking changes** |
| `selftest-oasdiff.sh` (`CT-DOC-02`) | `PASS` |
| `openapi-contract-drift.mjs` | `OPENAPI_HASHES_PINNED=3` sau khi rà |
| `generate-test-traceability.mjs` | `TEST_TRACEABILITY_WRITTEN=288` |
| `progressive-selftest.mjs` | `PROGRESSIVE_SELFTEST_PASS` — `IT-MIGRATE-03` đọc 6 migration, 0 thao tác phá |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` (sau khi dựng lại portal) |
| `ci-config-selftest.mjs` | `CI_CONFIG_SELFTEST_PASS` |
| `scan-pii.sh` | `PII_SCAN_PASS files=281` |
| admin-ui lint / tsc / vitest | 0 / 0 / 179 xanh (18 file) |

## 8. Migration expand-contract

Migration `P10_4_AnalyticsWarehouse` chỉ có `EnsureSchema` + 7 `CreateTable` trong `Up()`. Không
`DropColumn`/`DropTable`/`Rename*`/`AlterColumn`, không `AddColumn` nào — nên cổng `IT-MIGRATE-03`
(`W-0046`) không có gì để phản đối, và **không phải vì cổng dễ**: một schema mới hoàn toàn là hình
dạng expand-contract an toàn nhất có thể.

## 9. Blast radius đã cảnh báo trước khi sửa

`impact({target:"IvrDbContext", direction:"upstream"})` trả **`CRITICAL`** — 18 symbol, 8 execution
flow, 5 module. Thay đổi ở đó là **thuần cộng thêm** (6 `DbSet` mới + một lời gọi `Apply`), nhưng
CRITICAL nghĩa là một model không hợp lệ sẽ giết **mọi** flow lúc boot, nên toàn bộ regression là
phép kiểm, không phải hình thức.

**Chỉ số index đã cũ.** `impact({target:"AnalyticsReadService"})` trả `Target not found`: symbol đó
do `W-0098` thêm sau lần index gần nhất. Nên con số 18 là **sàn**, không phải chính xác, dù tool tự
ghi `epistemic: exact`. Phần blast radius của `AnalyticsReadService` được xác định bằng grep:
4 endpoint handler + 1 đăng ký DI + 1 file test.

## 10. Cái này KHÔNG chứng minh

- **Không có kho dữ liệu riêng** (`W-0063`). Là schema trong cùng database.
- **Không có BI tool nào từng kết nối.** Grant `SELECT` trên `analytics` là thứ *cấp được*, chưa ai cấp.
- **Chưa đo trên khối lượng thật.** Anti-join quét toàn bộ nguồn mỗi lượt; nguồn bị chặn bởi retention
  (DF-07) nhưng chưa lượt nào chạy trên dữ liệu quy mô production, và chưa có số đo chi phí quét.
- **`fact_call_job` giả định `closed_at` là bất biến.** Reconcile so **số dòng**, mà một dòng job mở
  đã cũ có đúng số dòng với nội dung sai. `BI-IDEMP-03` phủ phép refresh, **không** phủ tiền đề đó.
- **Chưa có alert nào** trên `reconcile_status`. Giá trị đã có; panel và luật thì chưa (`W-0041`).
- **Chưa có ETL chạy trong container/K8s.** Host đã đăng ký trong worker, nhưng bằng chứng là test
  gọi thẳng job — chưa lượt nào qua `AnalyticsEtlJobHost` trên cluster.
- **`CT-DOC-02` chạy trong container ghim digest**, không phải trên máy: `oasdiff` không cài local, và
  `sh` local vấp CRLF của working tree. Trong GitLab CI checkout là LF nên không gặp — nhưng điều đó
  là **suy luận**, không phải quan sát.
