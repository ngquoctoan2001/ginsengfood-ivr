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

Mọi tiêu chí tính được đều trong ngưỡng. Một tiêu chí **không tính được**: thời lượng vòng lõi. Console chỉ in vòng 1 và
mỗi vòng thứ mười; vòng 1 là vòng lõi duy nhất được in, các vòng thứ mười đều là vòng extended, nên quý cuối không có vòng
lõi nào để so. Số liệu từng vòng nằm trong tiến trình harness đã bị dừng.

**Sửa 25/09, sau `31a967c`:** bản đầu ghi 112 vòng được in và tất cả là vòng extended. Thật ra có 113 vòng được in, vòng 1
là vòng lõi (14 395 ms, ok): regex đọc console chỉ khớp dòng `extended` nên bỏ sót dòng `core`. Kết luận không đổi;
[partial-soak-2026-09-25.json](partial-soak-2026-09-25.json) đã sửa số và ghi lần sửa ở `corrections`.

**Soak không đo pipeline analytics.** Trong khi mọi chỉ số trên đạt, log hai worker cho thấy ETL analytics không hoàn
tất lượt nào trong phần log còn giữ lúc 12:15, và normalizer ghi lỗi trùng khóa khi hai worker đua. Ba lỗi đó được sửa
ở `W-0355` (`c2435b9`); lượt này chạy trên bản trước khi sửa.

**Còn lại:** một lượt đủ bốn giờ, trên bản đã có `W-0355`, vào lúc máy không build, không chạy CI hay collector.
Lượt này chạy khi máy còn build và chạy test ở nửa giữa; quý đầu được giữ không có việc nặng.

## 9. Hai lượt tiếp theo ngày `25/09`, cũng dở

| Lượt | Bản chạy | Thời gian | Dừng vì | Bản ghi |
| --- | --- | --- | --- | --- |
| 2 | `31a967c` | 13:41 → 15:25, phút 104, 84 mẫu | Tiến trình Claude Code khởi chạy lượt này thoát khi app desktop khởi động lại; harness, API và hai worker đi theo | [partial-soak-2026-09-25-run2.json](partial-soak-2026-09-25-run2.json) |
| 3 | `b7a0761` | 16:28 → 19:13, phút 165, 138 mẫu | Log kết thúc bằng `^C`. Lượt này khởi chạy qua WMI để không phụ thuộc phiên, và cửa sổ console của nó hiện trên desktop. Winlogon ghi máy khóa lúc 16:29:59, mở lúc 19:13:15; một bộ ghi CPU khởi chạy cùng cách cũng dừng trong cùng phút. Khả năng lớn nhất là hai cửa sổ bị đóng sau khi mở khóa: đó là suy luận, không phải quan sát | [partial-soak-2026-09-25-run3.json](partial-soak-2026-09-25-run3.json) |

Ở cả hai lượt, mọi tiêu chí tính được đều trong ngưỡng, còn thời lượng vòng lõi không tính được, cùng lý do như lượt 1.
Không lượt nào có verdict của harness, nên không có `pt-soak-02.json`, và `PT-SOAK-02` vẫn chưa được chứng minh.

**Quý đầu của lượt 3 bị tải.** Từ khoảng 16:30 tới 17:30, một phiên khác trên máy (ops-core) chạy chín agent build và
test .NET song song với Testcontainers: vòng 10 mất 41,9 giây, trước đó 21–34 giây. Bốn phép so quý (backlog callback,
độ trễ đóng cửa sổ, độ dư trước hạn, thời lượng vòng lõi) lấy quý đầu làm mốc, nên dễ đạt hơn; số ở bản ghi phải đọc
kèm điều đó. Bản ghi liệt kê các lần chồng lấn ở `overlaps`.

**Lỗi quay số trong log worker là lỗi được tiêm.** Trong lượt 3, worker ghi hơn 3 000 cảnh báo "Consecutive dispatch
failures" vào Application log của Windows. Lỗi gốc của mỗi lần là `MockSimOperationException: The fake SIM adapter
injected an audio error`, tức lỗi harness cố ý tiêm ở vòng extended, và backoff là phản ứng đúng của scheduler.

**Lượt 4** bắt đầu 19:35 trên `d989c31` (có cả `W-0359` và `W-0360`), khởi chạy qua WMI với cửa sổ ẩn. Kết quả sẽ
ghi ở mục sau.

## 10. Lượt thứ tư: `PT-SOAK-02` đạt

Lượt thứ tư chạy trên `d989c31` (đã có `W-0355` tới `W-0360`, trừ phần `RuntimeGateApprovals.cs` của `K-31`), từ 19:35:24
tới 23:35:29, đủ 240 phút, hai worker, `MOCK` toàn phần, `REAL_CUSTOMER_CALL_ALLOWED=NO`. Harness kết luận **`PASS`**:
1092/1092 vòng, 12 337 task nhận và đóng đủ, không lỗi, cả 18 tiêu chí đạt. Bản ghi của harness:
[pt-soak-02.json](pt-soak-02.json); phần E2E đi kèm: [local-mock-e2e.json](local-mock-e2e.json).

| Tiêu chí | Quý đầu → quý cuối | Ngưỡng |
| --- | --- | --- |
| Working set API · worker-1 · worker-2 | 171 → 169 MB · 163 → 189 MB · 158 → 169 MB | ≤ 1,5 lần |
| Handle API · worker-1 · worker-2 | 609 → 648 · 481 → 482 · 472 → 476 | ≤ 1,5 lần + 100 |
| Mỗi tiến trình một PID suốt lượt, không mất mẫu | 1 · 1 · 1 | không restart |
| Kết nối database, đỉnh | 35 → 15 | ≤ 1,5 lần + 5 |
| Backlog callback giữa các vòng, đỉnh | 1 → 1 | + 5 |
| Task quá hạn chưa có kết quả; quay số sau hạn; kết quả khách sau hạn | 0; 0; 0 | 0 |
| Cửa sổ hết hạn được đóng: số; trung vị độ trễ (tệ nhất) | 1 293; 0,238 → 0,054 giây (5,17 giây) | + 30 giây |
| Độ dư trước hạn, trung vị (thấp nhất) | 129,9 → 130,1 giây (112,8 giây) | − 30 giây |
| Thời lượng vòng lõi, trung vị (983 vòng lõi) | 12 684 → 8 595 ms | ≤ 2 lần |

**Nguồn gốc.** Lượt chạy từ một bản `git archive` của `d989c31` nằm ngoài mọi repo git, nên harness ghi `commit` rỗng,
và `tracked_tree_clean: true` của nó ở đây không có nghĩa. Sau lượt chạy, thư mục được so từng file với một bản
`git archive` mới của cùng commit: trùng khớp, trừ `bin/`, `obj/`, `ci-artifacts/` (log của harness), một file đánh dấu
và hai file harness ghi vào `docs/evidence/W-0037`. Commit và tree ghi ở [pt-soak-02-supplement.json](pt-soak-02-supplement.json).

**Quý đầu bị tải, quý cuối thì không.** CPU trung bình mỗi 15 phút: 81–99% từ 19:45 tới 21:15, 61–81% tới 22:00, rồi
14–24% từ 22:30 tới hết lượt. Nguồn tải: ba test host integration của ops-core, lượt test integration của phiên làm lô
`L6` trên cây `main`, và mười phút build cùng test của chính phiên này ở nửa giữa. Bốn phép so lấy quý đầu làm mốc
(backlog, độ trễ đóng cửa sổ, độ dư trước hạn, thời lượng vòng lõi) vì vậy dễ đạt hơn. Để bù, bản bổ sung tính lại các
phép so có trong mẫu, cùng ngưỡng, giữa quý cuối và khoảng yên duy nhất trước khi tải bắt đầu (19:36–19:48, 11 mẫu): tất
cả đạt, không chỉ số nào tụt. Khoảng đó ngắn và nằm ngay sau khởi động, nên đây là phép kiểm bổ sung, không thay được một
lượt trên máy yên. Vòng lõi không tính lại được từ mẫu; vòng lõi duy nhất đo trước khi tải, vòng 1, mất 12 126 ms, vẫn
chậm hơn trung vị quý cuối.

**Cách khởi chạy.** Qua WMI với cửa sổ ẩn, để cả việc khởi động lại tiến trình Claude Code lẫn việc đóng cửa sổ đều
không dừng được lượt này.

**Không chứng minh**, theo `not_claimed` của harness: ngưỡng là của harness, không phải mức dịch vụ đã duyệt; không có số
latency cho `D-04`; chỉ `MOCK`, không SIM, không nhà mạng, không khách thật (dung lượng 32 kênh thật là `W-0008`); một
máy, một database, API và worker là tiến trình cục bộ chứ không chạy trong cluster.

**Còn lại:** nếu Toàn cần một lượt mà cả quý đầu lẫn quý cuối đều trên máy yên, cần một khoảng bốn giờ không có phiên
nào khác build.
