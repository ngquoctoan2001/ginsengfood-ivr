# KPI catalog — `W-0055` · `P10-4`

Ngày: `2026-08-19` · Nguồn: `analytics.fact_call_outcome`, `analytics.fact_call_job`,
`analytics.agg_kpi_daily`

Tài liệu này định nghĩa **công thức**. Con số cụ thể không nằm ở đây — chúng nằm trong warehouse, và
mọi lần đọc đều kèm `data_quality` nói rõ nguồn nào trả lời.

## 1. Điều phải đọc trước

Warehouse này là **một schema PostgreSQL** (`analytics`) trong **cùng database** với bảng vận hành.
Đó không phải kho dữ liệu riêng — chưa có cluster nào để dựng (`W-0063`). Phần **thật** hôm nay là
ranh giới quyền: một BI tool cấp `SELECT` trên `analytics` đọc được toàn bộ KPI mà **không** chạm
được bảng vận hành nào.

Pipeline **chỉ đọc**. Nó không ghi bảng vận hành, không đụng audit/evidence (D-14), không gọi ra
ngoài. Chiều của mũi tên là toàn bộ lập luận an toàn.

## 2. Hai hạt (grain), và vì sao phải có hai

| Bảng | Một dòng là | Trả lời được |
| --- | --- | --- |
| `fact_call_outcome` | một **kết quả** cuộc gọi | tỉ lệ theo taxonomy, thời gian tới kết quả, A/B |
| `fact_call_job` | một **job** gọi | tổng job, job đủ điều kiện, tỉ lệ sang lần 2 |

Một job **chưa có kết quả** không có dòng nào ở bảng thứ nhất. Nếu chỉ giữ hạt kết quả thì
`total_call_jobs` phải lấy từ bảng vận hành, và payload sẽ vừa nói `warehouse_backed=true` vừa lấy
một nửa số liệu ở nơi khác — đúng nửa mà không ai kiểm.

## 3. Ranh giới privacy (D-05)

**Hai loại định danh, xử lý khác nhau, và khác biệt đó là trọng tâm:**

- **ID nội bộ IVR** (`ivr_call_result_id`, `ivr_call_job_id`) giữ nguyên. Chúng định danh *việc*
  của IVR, không định danh người, và admin console đã hiện chúng. Băm cái này trong khi cái kia
  vẫn đọc được là **trông như** bảo vệ chứ không bảo vệ gì.
- **ID đơn của Sales** chỉ tồn tại dưới dạng `order_ref_hash` (SHA-256, hex thường). Đây là khoá
  nghiệp vụ dẫn về khách trong hệ thống IVR **không sở hữu**, nên là chỗ duy nhất mà một phép join
  ngược là lối tái định danh thật. Băm vẫn đếm được số đơn phân biệt — thứ duy nhất KPI cần.

`order_ref_hash` là **bí danh, không phải ẩn danh**: ai cầm mã đơn vẫn xác nhận được nó có trong tập
hay không. Ghi ra vì đọc nhầm thành "đã ẩn danh" sẽ dẫn tới kết luận sai về mức chia sẻ được.

**Không bao giờ có mặt:** số điện thoại dưới mọi dạng, dial token, `order_code`, id khách, trạng thái
trust, evidence/audit ref, kênh SIM, provider call id, hay bất kỳ trường văn bản tự do nào.

Ép bằng hai lớp, hỏng vì hai lý do khác nhau:

| Lớp | Kiểm gì | Hỏng khi |
| --- | --- | --- |
| cấu trúc | đọc **model EF**, đòi mọi cột trong schema `analytics` nằm trong allowlist đã rà | ai đó thêm cột |
| giá trị | `PiiGuard` chạy trên từng chuỗi thực sự được ghi | một cột hợp lệ chứa giá trị không hợp lệ |

Dòng bị lớp 2 từ chối được **bỏ và đếm**, không ghi. Số đếm nằm trên checkpoint nên một lần bỏ im
lặng không thể bị nhầm với nguồn rỗng.

## 4. Định nghĩa KPI

Ký hiệu: `N` = `total_results` trong bucket; `J` = `total_call_jobs` trong phạm vi lọc.

| KPI | Công thức | Ghi chú |
| --- | --- | --- |
| `confirm_rate` | `confirmed_count / N` | `IVR_CONFIRMED` |
| `cancel_rate` | `cancelled_count / N` | `IVR_CUSTOMER_CANCELLED` |
| `no_answer_rate` | `no_answer_count / N` | gộp `IVR_NO_ANSWER_ATTEMPT` **và** `IVR_NO_ANSWER_FINAL` |
| `invalid_phone_rate` | `invalid_phone_count / N` | `IVR_INVALID_PHONE_FINAL` |
| `technical_rate` | `technical_count / N` | `IVR_TECHNICAL_EXCEPTION` |
| `operational_blocked_rate` | `null` trong current system | block xảy ra trước cuộc gọi nên không có dòng `ivr_call_results`; chỉ bật KPI khi có intake-block fact source riêng, không được diễn giải `null` thành `0` |
| `attempt_2_rate` | `#{job: counted_attempt_count ≥ 2} / J` | **chỉ** counted customer attempt (DT-02) |
| `avg_seconds_to_final` | `seconds_to_result_sum / seconds_to_result_count` | chỉ dòng `is_final` |
| `distinct_orders` | `count(distinct order_ref_hash)` | đơn, không phải kết quả |

**Lưu tổng và số đếm, không lưu trung bình.** Trung bình **không cộng được**: hai bucket trung bình
không ghép lại thành bucket thứ ba đúng, nên lưu trung bình sẽ làm hỏng lặng lẽ mọi phép roll-up của
BI tool. `BI-KPI-02` khẳng định điều này bằng một ví dụ số cụ thể.

**Bucket không có cuộc nào kết thúc trả `null`, không trả `0`.** `0` là một phép đo; "chưa có gì kết
thúc" thì không.

**Retry kỹ thuật không bao giờ nâng `attempt_2_rate`.** Một lần thử lại vì lỗi thiết bị không phải
lần thử thứ hai với khách (DT-02); tính vào sẽ báo chính sách attempt đang bị tiêu thụ nhanh hơn
thực tế.

## 5. Chiều phân tích

| Chiều | Cột | Dùng cho |
| --- | --- | --- |
| chương trình | `program_key` | `GOLDEN_HOUR` / `TWENTY_FOUR_SEVEN` |
| biến thể script | `script_variant_key` | so sánh A/B (P2-7) |
| taxonomy kết quả | `result_type_key` | DT-02 |
| ngày | `event_date` | bucket của `agg_kpi_daily` |
| giờ | `event_hour` | trend theo giờ, dẫn xuất **từ chính fact** |

`agg_kpi_daily` khoá theo **(ngày, chương trình, biến thể)**. Tách theo biến thể là thứ làm phép so
A/B đo được: gộp lại thì đúng khác biệt mà thí nghiệm sinh ra để tìm sẽ bị trung bình hoá mất.

## 6. Idempotency

**Không có watermark theo thời gian.** Thiết kế hiển nhiên là nhớ `created_at` cuối rồi đọc tiếp —
và thiết kế đó **mất dòng**: hai transaction có thể lấy timestamp theo thứ tự này rồi commit theo
thứ tự kia, nên một dòng có `created_at` đã nằm sau watermark vẫn có thể xuất hiện **sau khi**
watermark đi qua. Không ai đọc lại nó, không ai báo thiếu — KPI chỉ đơn giản là sai một lượng không
đo được.

Nên phép chọn là **anti-join theo khoá tự nhiên**: nạp mọi kết quả chưa có dòng fact. Thứ tự và thời
điểm commit hết quan trọng; replay là **exactly-once theo cấu trúc** chứ không theo quy ước.

**Aggregate được tính lại, không cộng dồn.** Mỗi bucket bị chạm được dựng lại từ fact. Cộng dồn
nhanh hơn và sẽ nhân đôi ngay lần đầu có gì đó chạy hai lượt.

**Checkpoint không tham gia tính đúng.** Xoá nó tốn một lượt chạy chậm, không mất một dòng fact.

## 7. Reconcile và data quality

Mỗi lượt so `source_row_count` với `fact_row_count`, sau khi trừ dòng mồ côi (kết quả mất job) và
dòng bị lớp privacy từ chối.

| `reconcile_status` | Nghĩa |
| --- | --- |
| `COMPLETE` | fact khớp nguồn |
| `BACKLOG` | trần batch chặn lại; còn dòng chờ. Bình thường, không phải lỗi |
| `MISMATCH` | lệch mà **không** có backlog và **không** có dòng bị từ chối để giải thích |
| `NOT_RUN` | chưa lượt nào |

`MISMATCH` là trạng thái mà một pipeline dùng watermark sẽ rơi vào **trong im lặng**; ở đây nó là một
giá trị mà cổng data-quality đỏ được.

Freshness đo theo **sự kiện mới nhất đã nạp**, không theo thời điểm chạy: một pipeline chạy đúng lịch
trên một nguồn đã ngừng sinh dữ liệu **không** phải là tươi.

## 8. Serve cho P3-4

`data_quality` mang **hai** khẳng định tách rời, và tách là có chủ ý:

- `source` — kho nào trả lời: `ANALYTICS_WAREHOUSE` hay `OPERATIONAL_READ_MODEL`.
- `warehouse_status` — pipeline đã đủ chưa: `NOT_RUN` / `COMPLETE` / `BACKLOG` / `MISMATCH`.

Warehouse thắng **bất cứ khi nào** nó có fact, **kể cả** khi reconcile báo backlog. Rơi về đọc vận
hành lúc đó sẽ đổi nguồn ngay giữa sự cố, nên cùng một câu hỏi trả về hai đáp án cách nhau vài phút
mà payload không có gì giải thích. Backlog được ghi nhãn là dữ liệu kém nhưng **trung thực**; đổi
nguồn im lặng là dữ liệu kém mà **trông ổn**.

Ngưỡng k-anonymity `min_bucket_size=5` vẫn áp **sau** khi đọc, ở tầng service, không đổi theo nguồn.

## 9. Retention (DF-07)

Fact tồn tại **chỉ khi** kết quả gốc còn tồn tại. Đây không phải một chu kỳ thứ hai để cấu hình mà
là một **phụ thuộc**: chu kỳ của warehouse bằng chu kỳ của nguồn một cách tự động, và không có cách
nào đặt hai bên lệch nhau vì chỉ có một bên.

Chạy như purge hook **sau** khi retention job xoá xong các lớp vận hành. Tôn trọng dry-run. Sau khi
xoá, bucket bị ảnh hưởng được **tính lại** — bỏ qua bước đó sẽ giữ nguyên đúng con số mà lượt
retention sinh ra để xoá.

## 10. Cái này KHÔNG chứng minh

- **Không có kho dữ liệu riêng.** Là schema trong cùng database (`W-0063`).
- **Không có BI tool nào từng kết nối.** Grant `SELECT` trên `analytics` là thứ *có thể* cấp, chưa ai
  cấp.
- **Chưa đo trên khối lượng thật.** Anti-join quét toàn bộ nguồn mỗi lượt; nguồn bị chặn bởi retention
  nhưng chưa lượt nào chạy trên dữ liệu quy mô production.
- **`fact_call_job` giả định `closed_at` là bất biến.** Reconcile so **số dòng**, mà một dòng job mở
  đã cũ có đúng số dòng với nội dung sai. Nếu `closed_at` từng được đặt trong khi job còn đổi được
  thì pass refresh sẽ cũ đi trong im lặng — `BI-IDEMP-03` phủ phép refresh, **không** phủ tiền đề đó.
- **Chưa có alert nào** trên `reconcile_status`. Giá trị đã có, panel/luật thì chưa (`W-0041` phạm vi
  khác).
