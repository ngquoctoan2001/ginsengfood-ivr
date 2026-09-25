# W-0355 — Ba lỗi lộ ra khi chạy hai worker, tìm thấy trong lượt soak của W-0037

Ngày 25/09/2026 · Claude, trong lượt *"rà soát tiếp việc cần làm"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Tìm ra thế nào

Lượt soak bốn giờ của W-0037 chạy hai worker tranh cùng một hàng việc suốt lượt; harness cố ý làm vậy.
Các chỉ số soak đo đều tốt: không task nào trễ hạn, không kết quả nào trùng. Nhưng log worker có lỗi lặp lại mà các
chỉ số đó không đo: normalizer ghi Error vì trùng khóa `PK_ivr_call_results`, và lượt ETL analytics hỏng, hoặc vì
trùng khóa `PK_fact_call_outcome`, hoặc vì một lệnh chạy quá 30 giây.

Lúc 12:15, trong phần log còn giữ của **cả hai** worker, không lượt ETL nào hoàn tất: worker-1 hỏng 5 lượt vì
timeout, worker-2 hỏng 5 lượt vì trùng khóa. Analytics trong soak đã ngừng tiến. Harness chỉ giữ phần log mới nhất,
nên số trong [soak-log-counts.json](soak-log-counts.json) là cận dưới. Các dòng `Scheduler dispatch failed` là lỗi giả
mà kịch bản soak cố ý bơm vào SIM giả, đúng thiết kế, không thuộc việc này.

## Ba lỗi

**1. Normalizer nhận lại việc worker kia vừa làm xong.** Câu lệnh nhận việc chỉ khóa dòng `raw_event`. Điều kiện
"attempt còn chờ chuẩn hoá" được đọc theo ảnh chụp dữ liệu lúc câu lệnh bắt đầu, mà worker chuẩn hoá xong thì đổi
dòng `attempt`, không đổi `raw_event`. Worker thứ hai bắt đầu câu lệnh ngay trước khi worker thứ nhất commit sẽ vẫn
thấy event còn chờ, giành khóa `raw_event` ngay khi được nhả, rồi ghi kết quả lần hai và vấp khóa chính.
Sửa: khóa luôn dòng `attempt` (`FOR UPDATE OF raw_event, attempt SKIP LOCKED`). Khi giành khóa một dòng đã bị sửa sau
ảnh chụp, PostgreSQL kiểm lại điều kiện trên bản đã commit và bỏ qua; dòng đang bị giữ thì `SKIP LOCKED` bỏ qua.

**2. Hai lượt ETL chạy cùng lúc.** Anti-join chọn "result chưa có fact" chỉ đúng một lần so với các lượt trước, không
so với lượt chạy bên cạnh. Hai worker khởi động cùng lúc và cùng chu kỳ 300 giây, nên lượt nào cũng đụng nhau.
Sửa: mỗi lượt giữ advisory lock phiên `ivr:analytics-etl:call_outcome` suốt lượt, vì một lượt gồm nhiều transaction.
Lượt thấy khóa bị giữ thì không làm gì, không ghi checkpoint, không đếm metric, chỉ ghi một dòng Debug
(`EventId 1205`). Khóa được trả tường minh khi lượt xong: đột biến `M4` cho thấy bỏ bước trả thì kết nối quay về
pool vẫn giữ khóa và lượt sau của worker kia bị bỏ.

**3. Câu đối chiếu job fact chạy theo bình phương dữ liệu.** Đây là lỗi hồi quy của chính W-0353 (`e95ba64`), phần
làm mới job fact theo nguồn mà `BI-DRIFT-06` kiểm. Số attempt được đếm riêng cho từng job ngay trong phép chiếu, nên
PostgreSQL chạy ba lần quét tuần tự bảng `ivr_call_attempts` cho **mỗi** job fact. Index duy nhất bắt đầu bằng mã job
là index một phần (`WHERE is_counted_customer_attempt IS TRUE`), và planner không suy ra được điều kiện đó từ cột
boolean trần mà EF viết. Câu này chạy hai lần mỗi lượt. Đo trên chính database của soak lúc 12:11
([drift-query-measurement.json](drift-query-measurement.json)): với 7.623 job fact, câu cũ mất **34.472 ms**, vượt
ngưỡng timeout 30 giây của lệnh; câu mới **19 ms**; hai câu trả về cùng 19 dòng, trùng từng cột.
Sửa: đếm attempt theo job **một lần** (`GROUP BY`) rồi left join.

## Ảnh hưởng

- **Dữ liệu vận hành không sai.** Khóa chính chặn mọi bản ghi trùng; lượt hỏng không để lại gì dở dang.
- **Lỗi 3 ảnh hưởng production kể cả khi chỉ một worker.** Thời gian tăng theo bình phương số job, nên ở quy mô thật
  ETL sẽ không bao giờ hoàn tất: báo cáo đứng yên và alert `IvrAnalyticsEtlNotCompleting` kêu.
- **Lỗi 1 và 2 chỉ lộ khi chạy từ hai worker.** Production hiện chạy một worker (`deploy/helm/ivr/values-prod.yaml`:
  `replicas: 1`, HPA tắt, `maxReplicas: 2` đã chuẩn bị). Hậu quả là log Error, heartbeat báo vòng analytics hỏng, và
  metric đếm `FAILED`.
- **Không đổi contract.** `Skipped` là field nội bộ; lượt bị bỏ giữ trạng thái `NOT_RUN` mà API báo cáo đã có; tập
  nhãn metric không đổi; không có migration.

## Phép kiểm

| Test | Kiểm gì |
| --- | --- |
| `IT-NORM-CONCURRENCY-06` | Giữ cửa sổ đua mở thay vì trông may: một transaction đóng vai worker A đã đổi attempt nhưng chưa commit, worker B chạy vào đúng lúc đó; B phải bỏ qua attempt cả khi A đang giữ lẫn sau khi A commit. `IT-NORM-CONCURRENCY-05` đua hai normalizer thật, nhưng chỉ bắt được lỗi khi may trúng khe hẹp |
| `BI-CONCURRENT-07` | Một phiên giữ khóa như worker thứ hai: lượt ETL bị bỏ, không nạp fact, không ghi checkpoint, không đếm metric; nhả khóa thì lượt sau nạp đủ; lượt xong thì trả khóa |
| `BI-DRIFT-08` | Kế hoạch thực thi của câu đối chiếu không có `SubPlan` và chỉ đọc `ivr_call_attempts` một lần. Kế hoạch không phụ thuộc cỡ dữ liệu như thời gian đo; `BI-DRIFT-06` vẫn kiểm câu này tìm đúng dòng |

Năm đột biến, mỗi cái gỡ một phần bản sửa, đều làm test tương ứng đỏ, rồi xanh lại khi khôi phục
([mutation-results.json](mutation-results.json)). Với `M5`, đưa lại phép đếm từng job: `BI-DRIFT-08` đỏ còn
`BI-DRIFT-06` vẫn xanh, tức test mới bắt đúng chi phí chứ không bắt ngữ nghĩa.

## Kiểm chứng

Trên cây làm việc: build `0` cảnh báo; unit `813/813`; contract `24/24`; integration `411/411` trước khi sửa câu
đối chiếu, rồi toàn bộ `AnalyticsPipelineTests` `16/16` sau khi sửa (lớp duy nhất gọi ETL); chaos `8/8`. Gói collector
tại commit chạy sau khi soak kết thúc.

## Việc cho Toàn

W-0055 được ghi `ACCEPTED` sáng `25/09` (`A-1008`) dựa trên `BI-DRIFT-06`, trong khi lỗi 3 nằm ở chính phần đó của
W-0353. Test chạy trên vài chục dòng nên không lộ. Bản sửa ở đây; giữ hay mở lại nghiệm thu W-0055 là Toàn quyết.

**Toàn quyết `25/09`: giữ nghiệm thu W-0055.** Bản sửa đi cùng `W-0355`; không mở lại.
