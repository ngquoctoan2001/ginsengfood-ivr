# So sánh quy trình gọi IVR: đơn Giờ Vàng vs đơn 24/7

Ngày dựng: `2026-09-22` · Baseline: `main@02e32e3` · Phạm vi: Module 8 (IVR), đọc từ code + seed + spec
đang nằm trong repo, không phải từ tài liệu business cũ.

> **Đọc một dòng:** hai chương trình dùng **cùng một quy trình gọi**, cùng kịch bản, cùng phím bấm,
> cùng khung giờ, cùng bộ mã kết quả. Khác nhau **chỉ ở bốn chỗ**: cặp payment bắt buộc, độ dài cửa
> sổ xác nhận (5 phút vs 15 phút), khoảng cách giữa hai cuộc gọi (2:30 vs 7:30), và endpoint callback
> cũ. Số lần gọi khách **bằng nhau: 2 lần**.

---

## 1. Bảng so sánh chính

| # | Hạng mục | Giờ Vàng | 24/7 |
|---|---|---|---|
| 1 | Mã trên wire | `GOLDEN_HOUR` | `TWENTY_FOUR_SEVEN` — **không** nhận alias `24_7` |
| 2 | Payment bắt buộc đi kèm | `ONLINE` | `COD` |
| 3 | Cặp sai thì sao | `GOLDEN_HOUR`+`COD` → schema từ chối ngay | `TWENTY_FOUR_SEVEN`+`ONLINE` → schema từ chối ngay |
| 4 | Số lần gọi khách tối đa | **2** | **2** |
| 5 | Mốc gọi (offsets) | `[0, 150]` giây → A1 `T0`, A2 `T0+2:30` | `[0, 450]` giây → A1 `T0`, A2 `T0+7:30` |
| 6 | Độ dài cửa sổ xác nhận | **5 phút** (300s) | **15 phút** (900s) |
| 7 | Hết hạn | `T0+5:00` | `T0+15:00` |
| 8 | Nghỉ giữa 2 cuộc gọi | 2 phút 30 giây | 7 phút 30 giây |
| 9 | Đệm còn lại sau A2 | 2 phút 30 giây | 7 phút 30 giây |
| 10 | TTL `dial_token` | phải **bằng đúng** `T0+5:00` | phải **bằng đúng** `T0+15:00` |
| 11 | Sales chờ kết quả cuối lâu nhất | ~5 phút | ~15 phút |
| 12 | Ưu tiên hàng đợi khi nghẽn kênh | hạng **0** (trước) | hạng **1** (sau) |
| 13 | Callback endpoint cũ (`CURRENT_GOLDEN_HOUR_COMPAT`) | được phép | **bị chặn** — ném `InvalidOperationException` |
| 14 | Bộ số hiện đăng ký ở DB | `mock-lab-v1` (MOCK) · `lab-softphone-v1` (LAB) | `mock-lab-v1` (MOCK) · `lab-softphone-v1` (LAB) |

`T0` = thời điểm **Order Core mở confirmation window / tạo task**, **không phải** lúc khách bấm đặt
hàng. Nếu task bị đẩy sang IVR trễ, `T0` vẫn là mốc Order Core khai trên wire (`D-10`).

---

## 2. Dòng thời gian cụ thể — cùng đặt `T0 = 09:00:00`

| Mốc | Giờ Vàng | 24/7 |
|---|---|---|
| `T0` | **09:00:00** — cuộc gọi 1 | **09:00:00** — cuộc gọi 1 |
| Đổ chuông tối đa | 30 giây | 30 giây |
| Phát TTS xong, chờ phím | 15 giây | 15 giây |
| Khách bấm `1` / `0` | kết thúc **ngay**, gửi callback | kết thúc **ngay**, gửi callback |
| Không nghe máy → | `IVR_NO_ANSWER_ATTEMPT` (chưa final, **không** gửi callback) | `IVR_NO_ANSWER_ATTEMPT` (chưa final, **không** gửi callback) |
| Cuộc gọi 2 | **09:02:30** | **09:07:30** |
| Vẫn không nghe máy → | `IVR_NO_ANSWER_FINAL` → gửi callback | `IVR_NO_ANSWER_FINAL` → gửi callback |
| Hết hạn cửa sổ | **09:05:00** | **09:15:00** |
| Hết hạn mà chưa có kết quả final | `IVR_CONFIRMATION_WINDOW_EXPIRED` | `IVR_CONFIRMATION_WINDOW_EXPIRED` |
| Hết hạn mà **chưa gọi được lần nào** | `IVR_CAPACITY_EXCEPTION` + mở capacity incident | `IVR_CAPACITY_EXCEPTION` + mở capacity incident |

Chênh lệch thực tế: Sales biết kết quả cuối của đơn **Giờ Vàng nhanh hơn đơn 24/7 đúng 5 phút** trong
kịch bản khách không nghe máy, và **nhanh hơn 10 phút** trong kịch bản hết hạn cửa sổ.

---

## 3. Những phần **giống hệt nhau** (đừng thiết kế khác đi)

| Hạng mục | Giá trị dùng chung cho cả hai |
|---|---|
| Đổ chuông tối đa | 30 giây (`DialTimeoutSeconds`) |
| Chờ phím DTMF | 15 giây (`DtmfTimeoutSeconds`) |
| Phím hợp lệ | `1` = xác nhận · `0` = hủy · phím khác = `IVR_WRONG_INPUT` |
| Kịch bản thoại | `SCRIPT-ORDER-CONFIRM` `v3-test-approved` — **không đọc tên chương trình**, dù `program_display_name` có trên wire |
| Giọng đọc | VieNeu TTS, một engine duy nhất, không thu âm người |
| Ghi âm cuộc gọi | **Tắt** — health check từ chối dial nếu ARI không xác nhận đã tắt |
| Khung giờ được phép gọi | **08:00 – 21:08** giờ VN (`+07:00`), không phân biệt chương trình |
| Retry kỹ thuật | `TechnicalRetryLimit = 1`, là config scheduler, **không** thuộc attempt policy |
| Bộ mã kết quả | 11 giá trị `IVR_*` dùng chung |
| Khi nào gửi callback | **Chỉ khi kết quả là final**; kết quả giữa chừng không gửi |
| SLA Sales revalidate | 3–5 giây (`D-04`) |
| Cooldown kênh sau cuộc gọi | 2 giây |
| Thời lượng gọi dự tính (tính capacity) | 60 giây |
| Nhịp quét scheduler | 1 giây |

Hệ quả đáng lưu ý của dòng "retry kỹ thuật": giới hạn là **1 lần cho cả hai**, nhưng retry chỉ được
phép nếu vẫn còn nằm trong cửa sổ. Cửa sổ Giờ Vàng 5 phút nên chỗ để retry chật hơn 24/7 rất nhiều —
cùng một sự cố mạng, đơn 24/7 thường retry được, đơn Giờ Vàng thì không.

---

## 4. Biên khung giờ 08:00 – 21:08 tác động khác nhau

Intake từ chối thẳng nếu **mọi** lần gọi theo lịch đều rơi ngoài giờ, với lý do
`CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` — thay vì nhận đơn rồi để nó chết lặng lẽ thành
"khách không xác nhận".

| Tình huống theo `T0` | Giờ Vàng | 24/7 |
|---|---|---|
| `T0` trước mốc này → **từ chối ngay** (buổi sáng) | trước **07:57:30** | trước **07:52:30** |
| `T0` trong vùng này → nhận nhưng **chỉ gọi được 1 lần** (buổi sáng) | 07:57:30 → 07:59:59 | 07:52:30 → 07:59:59 |
| `T0` bình thường, đủ cả 2 cuộc | 08:00:00 → 21:05:29 | 08:00:00 → 21:00:29 |
| `T0` trong vùng này → nhận nhưng **chỉ gọi được 1 lần** (buổi tối) | 21:05:30 → 21:07:59 | 21:00:30 → 21:07:59 |
| `T0` từ **21:08:00** trở đi | **từ chối ngay** | **từ chối ngay** |

Hai điều cần nhớ:

1. **Con số 21:08 sinh ra từ chương trình 24/7, không phải Giờ Vàng.** Chốt gốc là 21:00; cộng thêm
   450 giây (khoảng cách A1→A2 của 24/7) ra 21:07:30, làm tròn lên phút thành 21:08. Trước khi sửa
   (`W-0220`), đơn 24/7 vào lúc 20:53 trở đi **âm thầm mất cuộc gọi thứ hai** và Sales nhận
   `IVR_CONFIRMATION_WINDOW_EXPIRED` thay vì `IVR_NO_ANSWER_FINAL`.
2. **Vùng "chỉ gọi được 1 lần" của 24/7 rộng gấp 3 lần Giờ Vàng** ở cả hai đầu ngày (7 phút 30 so với
   2 phút 30), đúng bằng tỉ lệ giữa hai khoảng cách attempt.

Đơn đặt ban đêm (ví dụ 23:00) bị từ chối tại intake cho **cả hai** chương trình. IVR không tự dời
sang sáng hôm sau: cửa sổ xác nhận và `dial_token` đều do Module 3 cấp sẵn, nên quyết định giữ đơn
đến sáng hay cấp cửa sổ mới là việc của Module 3.

---

## 5. Khi kênh SIM bị nghẽn — ai được gọi trước

Thứ tự tranh kênh (SQL claim, `FOR UPDATE ... SKIP LOCKED`):

| Ưu tiên | Tiêu chí | Ghi chú |
|---|---|---|
| 1 | `expires_at` gần nhất trước | Giờ Vàng gần như luôn thắng ngay tại đây: cửa sổ 5 phút so với 15 phút |
| 2 | Chương trình: `GOLDEN_HOUR` = 0, `TWENTY_FOUR_SEVEN` = 1 | chỉ có tác dụng khi hai đơn **trùng hạn chót** |
| 3 | Attempt offset nhỏ hơn trước | A1 trước A2 |
| 4 | Số `risk_flags` nhiều hơn trước | đơn rủi ro cao được ưu tiên |
| 5 | `created_at`, rồi `job_id` | phá hòa tất định |

Thực tế: **đơn Giờ Vàng luôn chen lên trước đơn 24/7** trong cùng một đợt nghẽn, vì hạn chót của nó
gần hơn 10 phút. Tiêu chí số 2 chỉ là lưới an toàn.

Mặt trái: đơn Giờ Vàng cũng là đơn **dễ rơi vào `IVR_CAPACITY_EXCEPTION` nhất** nếu thiếu kênh, vì nó
chỉ có 5 phút để tìm được một khe trống. Gate capacity tại intake mô phỏng với thời lượng gọi 60 giây
và từ chối trước nếu không kịp hạn chót.

---

## 6. Trạng thái thật hôm nay — đọc trước khi dùng bảng trên để nghiệm thu

| # | Sự thật | Ảnh hưởng |
|---|---|---|
| 1 | Bộ số production `gh-247-prod-v1` **đã bị xóa khỏi database** bởi migration `20260916065842_UnapproveUnsignedProductionAttemptPolicy` (`W-0300`/`B3`) | Các con số 5 phút/15 phút, `[0,150]`/`[0,450]` **vẫn nằm trong code** (`SignedProductionAttemptPolicies`) nhưng **không resolve được từ DB**. Lý do xóa: một migration không phải là một quyết định — nó mang approval tới mọi máy chạy schema |
| 2 | DB hiện chỉ có `mock-lab-v1` (MOCK) và `lab-softphone-v1` (LAB_REAL_SIM) | Muốn chạy đúng bộ số production phải **insert lại qua bước vận hành có tham chiếu approval thật**, không phải qua migration |
| 3 | `lab-softphone-v1` ép **cả hai** chương trình về **1 attempt / cửa sổ 300s** | Lab **không** tái hiện được khác biệt thời gian giữa Giờ Vàng và 24/7. Đừng dùng kết quả lab để nghiệm thu bảng §2 |
| 4 | Kịch bản full-flow S5 (`deploy/lab/full-flow-s5/cases.py`) chỉ chạy `GOLDEN_HOUR` | Luồng **24/7 chưa từng được chạy end-to-end trên lab** — kể cả ở mức 1 attempt |
| 5 | `PRODUCTION_REAL` vẫn fail-closed ở 3 chỗ độc lập (`IvrOptionsValidator`, `DispatchGate`, `SchedulerCapacity`) và chưa có dispatch gateway production | Chưa có cuộc gọi thật nào chạy trên bộ số nào |
| 6 | Tài liệu phase-8 cũ ghi Giờ Vàng `2/[0,300]/600s` và 24/7 `3/[0,300,600]/900s` | **Đã bị `D-10` thay thế**, chỉ còn giá trị lịch sử. Bảng §1 mới là bộ số đang chạy |
| 7 | `T-01` vẫn ở trạng thái `M8_POSITION_SIGNED / M3_PRODUCT_ARTIFACT_PENDING` | Ma trận program/payment là chữ ký một phía của Module 8; Module 3 chưa trả producer CDC + chữ ký |

---

## 7. Nguồn tra cứu

| Nội dung | File |
|---|---|
| Enum chương trình, ma trận program/payment, snapshot policy | [`src/Ivr.Domain/Confirmation/AttemptPolicy.cs`](../../src/Ivr.Domain/Confirmation/AttemptPolicy.cs) |
| Bộ số candidate + bộ số production đã ký | [`src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs`](../../src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs) |
| Khung giờ 08:00–21:08 và lý do có 8 phút | [`src/Ivr.Infrastructure/Scheduling/CallingWindow.cs`](../../src/Ivr.Infrastructure/Scheduling/CallingWindow.cs) |
| Gate intake, `dial_token` phải bằng đúng cuối cửa sổ, chặn task cả đêm | [`src/Ivr.Infrastructure/Intake/TaskIntakeService.cs`](../../src/Ivr.Infrastructure/Intake/TaskIntakeService.cs) |
| Thứ tự tranh kênh, sweep hết hạn, capacity incident | [`src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs`](../../src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs) |
| Ưu tiên chương trình trong hàng đợi | [`src/Ivr.Domain/Scheduling/DeadlineScheduler.cs`](../../src/Ivr.Domain/Scheduling/DeadlineScheduler.cs) |
| Đổ chuông 30s, DTMF 15s, ghi âm tắt | [`src/Ivr.Infrastructure/Telephony/AsteriskAriOptions.cs`](../../src/Ivr.Infrastructure/Telephony/AsteriskAriOptions.cs) |
| Ánh xạ phím bấm → mã kết quả, khi nào là final | [`src/Ivr.Domain/Confirmation/DispositionMapper.cs`](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs) |
| Chặn 24/7 đi vào endpoint Giờ Vàng cũ | [`src/Ivr.Contracts/Sales/SalesContractSelection.cs`](../../src/Ivr.Contracts/Sales/SalesContractSelection.cs) |
| Ràng buộc cặp program/payment trên wire | [`specs/api/openapi/ivr-order-confirmation.v1.yaml`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml) |
| Quyết định `D-10` (bộ số hiện hành) | [`plan/ivr-orther/decisions-log.md`](decisions-log.md) |
| Ma trận program/payment đã ký một phía | [`docs/contracts/target-v1-closure-pack/T-01-program-matrix.md`](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) |
| Spec scheduler & attempt policy | [`specs/functional/03-scheduler-attempt-policy.md`](../../specs/functional/03-scheduler-attempt-policy.md) |
| Lý do xóa bộ số production khỏi DB | [`src/Ivr.Infrastructure/Persistence/Migrations/20260916065842_UnapproveUnsignedProductionAttemptPolicy.cs`](../../src/Ivr.Infrastructure/Persistence/Migrations/20260916065842_UnapproveUnsignedProductionAttemptPolicy.cs) |
