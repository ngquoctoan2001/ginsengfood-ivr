# W-0037 — Evidence: Performance, load and security/privacy tests (`P5-3`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (phần đo được), ba mục `NOT_RUN` nêu rõ ở §5

Báo cáo số liệu: **[`docs/perf-security-report.md`](../../perf-security-report.md)**

## 1. Đo bất biến dưới tranh chấp, không đo throughput

`P5-3` §7 gợi ý k6/NBomber. Tôi không dùng, và ghi rõ lý do: **nút thắt của IVR là lease kênh trong database**, không phải HTTP throughput. Bắn HTTP vào một endpoint không phải giới hạn sẽ cho ra một con số đẹp về đúng thứ không quan trọng.

Cái đo thay vào đó: worker đua thật trên Postgres thật, đẩy quá năng lực có chủ ý, rồi hỏi ba câu mà **sai được ngay cả khi latency đẹp**:

1. `ONE_SIM_ONE_ACTIVE_CALL` có giữ dưới tranh chấp không?
2. Quá tải có làm mất task không?
3. Quá tải có tiêu lượt gọi của khách không?

`PT-CAP-01` chạy hai hình dạng — 1 kênh/8 job (lab sắp tới) và 4 kênh/24 job — và trả lời: giữ, không, không.

Assertion đáng giá nhất là mỗi kênh `RESERVED` trỏ tới đúng **một** `active_call_job_id` duy nhất. Một kênh mang hai cuộc là hai khách hàng nghe đơn của nhau.

## 2. Fail-closed dưới tải

`PT-FAILCLOSED-03`: 12 task đánh giá **đồng thời** với nguồn capacity ném timeout → **0/12** dispatch, cả 12 mang `CAPACITY_SOURCE_UNAVAILABLE`, 0 attempt, 12 job còn nguyên.

`DO-06`. Chế độ hỏng đáng sợ là chế độ **dễ dãi**. Chạy đồng thời là ca đáng thử nhất vì một cuộc đua là nơi "không biết" dễ bị làm tròn thành "ổn" nhất.

## 3. Cổng PII quét file — không ai quét database

`SEC-PII-04` là lỗ hổng thật lớn nhất slice này tìm ra.

`scan-pii.sh` quét **file trong repo**. Không có gì quét thứ service **thật sự ghi xuống** — payload audit, evidence ref, reason của review item dựng từ field khách hàng. Đó là nơi một rò rỉ sẽ đáp xuống lúc chạy, và trước slice này nó không được kiểm.

Giờ sau một lượt tải, mọi dòng trong 5 bảng được đưa **ngược lại** qua chính `PiiGuard` mà runtime dùng để tự kiểm. Kiểm từ ngoài vào, trên cái đã landed — không phải kiểm ý định.

## 4. Hai lỗi fixture mà tải mới lộ ra

**Idempotency key hard-code.** Seeder của `EligibilityPersistenceTests` gán `IdempotencyKey = "order-core:TASK-ELIG-CAP-05:idem-cap-05"` cố định. Ổn khi mỗi test seed một task; **vỡ unique index ngay khi một test tải seed 12 task**. Sửa thành derive từ `taskId`.

Đây là loại lỗi chỉ tải mới tìm được: một fixture đúng ở n=1 và sai ở n>1.

**Concat trên cột json.** Bản đầu của `SEC-PII-04` ghép chuỗi **trong câu query**, mà `DataJson` là cột `json` → PostgreSQL từ chối `22P02`. Sửa bằng cách materialize trước rồi ghép trong bộ nhớ. Cũng là lỗi chỉ lộ khi chạm database thật, không phải in-memory.

## 5. Ba mục KHÔNG làm — và vì sao không giả

| Mục | Trạng thái | Lý do |
| --- | --- | --- |
| **Rate limiting** (`SEC-AUTHZ-05` nửa sau) | **CHƯA CÓ** | Chỉ tồn tại ánh xạ `IVR_RATE_LIMITED → 429`; **không middleware nào phát ra nó**. Không tự thêm: ngưỡng rate-limit cho một internal API là **quyết định vận hành** — ai gọi, bao nhiêu, vượt thì sao. Đặt một con số tuỳ tiện rồi viết test cho nó là tạo bằng chứng về một chính sách chưa ai duyệt |
| **Soak 4–8h** (`PT-SOAK-02`) | **NOT_RUN** | Không chạy được trong phiên này. Không thay bằng một lượt ngắn rồi gọi là soak: leak bộ nhớ và trôi deadline là hiện tượng **theo thời gian**, một lượt 30 giây không nói gì về chúng |
| **Ngưỡng latency D-04 (3–5s)** | **không tuyên bố** | Đó là ngưỡng đầu-cuối **có Sales thật trong vòng lặp**. Với fake provider, con số đo được là độ trễ của chính fake. Báo cáo không ghi một con số latency nào |

Ba mục này ở phần "chưa làm", không phải "đã đạt".

## 6. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **361/361** (22 contract + 205 unit + 134 integration), +4 |
| `dotnet test --filter "TestId~PT-"` | 3/3 (2 hình dạng capacity + fail-closed) |
| `dotnet test --filter "TestId~SEC-PII-04"` | 1/1 |
| `test:traceability` | `TEST_TRACEABILITY_WRITTEN=239` |
| `dotnet build -warnaserror` | 0 warning / 0 error |

Tất cả chạy trên PostgreSQL thật qua Testcontainers.

## 7. Cái này KHÔNG chứng minh

- **Không phải bằng chứng năng lực.** Mô phỏng 4 kênh chứng minh **bất biến**, không chứng minh 32 kênh chạy được. Năng lực thật là `W-0008`/`G-ESIM32`, cần SIM thật — xem `telephony-procurement-pack` R-03 §6.
- **Không có soak, không có rate limit, không có ngưỡng latency** (§5).
- **Không có OWASP ZAP.** Cần một service đang chạy trong pipeline; thuộc `P7-3`.
- **Không có PII thật, không có khách thật.** `MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

## 8. `PT-SOAK-02` — lượt ngày `25/09`, dừng ở phút 199

*Thêm `25/09`. Mục `5` ở trên ghi soak `NOT_RUN` và rate limiting "CHƯA CÓ"; hai dòng đó giữ nguyên như lúc viết.
Rate limiting có từ `W-0282` (`5cc4b17`): `Ivr:ServiceQuota`, tắt mặc định vì chưa đo năng lực thật, bật ở sandbox,
và `SEC-AUTHZ-05` có test.*

`W-0353` thêm chế độ soak cho `tools/dev/Invoke-LocalMockE2E.mjs`. Lượt này chạy nó trên máy dev từ 10:06:

```text
node tools/dev/Invoke-LocalMockE2E.mjs --duration-minutes 240 --sample-seconds 60 --skip-faults --evidence-dir docs/evidence/W-0037
```

Hai worker tranh cùng một hàng việc, `MOCK` toàn phần, `REAL_CUSTOMER_CALL_ALLOWED=NO`. **Lượt dừng lúc khoảng 13:25,
ở phút 199 trên 240**, sau mẫu 176 và vòng 1120; hệ thống báo task bị dừng từ phía người dùng, log không có lỗi nào của
stack trước đó. Harness chưa kịp tính verdict nên **không có `pt-soak-02.json`, và lượt này không phải bằng chứng
`PT-SOAK-02` đạt**: vừa thiếu mốc bốn giờ, vừa thiếu verdict của chính harness.

Để lượt dở không mất, [partial-soak-2026-09-25.json](partial-soak-2026-09-25.json) tính lại từ các mẫu đã ghi đúng
những tiêu chí harness dùng, cùng ngưỡng, quý đầu so với quý cuối của 199 phút:

| Tiêu chí | Quý đầu → quý cuối | Ngưỡng |
| --- | --- | --- |
| Working set API · worker-1 · worker-2 | 187,9 → 163,6 MB · 172,0 → 179,7 MB · 174,2 → 178,0 MB | ≤ 1,5 lần |
| Handle API · worker-1 · worker-2 | 694 → 658 · 513 → 529 · 515 → 527 | ≤ 1,5 lần + 100 |
| Mỗi tiến trình một PID suốt lượt | 1 · 1 · 1 | không restart |
| Kết nối database, đỉnh | 36 → 17 | ≤ 1,5 lần + 5 |
| Task quá hạn chưa có kết quả; quay số sau hạn; kết quả khách sau hạn | 0; 0; 0 | 0 |
| Độ trễ đóng cửa sổ, trung vị (tệ nhất) | 0,086 → 0,068 giây (0,45 giây) | + 30 giây |
| Độ dư trước hạn, trung vị | 130,6 → 131,0 giây | − 30 giây |

Mọi tiêu chí tính được đều trong ngưỡng. Một tiêu chí **không tính được**: thời lượng vòng lõi, vì console chỉ in mỗi
vòng thứ mười và các vòng được in ở đây đều là vòng extended; số liệu từng vòng nằm trong tiến trình harness đã bị dừng.

**Soak không đo pipeline analytics.** Trong khi mọi chỉ số trên đạt, log hai worker cho thấy ETL analytics không hoàn
tất lượt nào trong phần log còn giữ lúc 12:15, và normalizer ghi lỗi trùng khóa khi hai worker đua. Ba lỗi đó được sửa
ở `W-0355` (`c2435b9`); lượt này chạy trên bản trước khi sửa.

**Còn lại:** một lượt đủ bốn giờ, trên bản đã có `W-0355`, vào lúc máy không build, không chạy CI hay collector.
Lượt này chạy khi máy còn build và chạy test ở nửa giữa; quý đầu được giữ không có việc nặng.
