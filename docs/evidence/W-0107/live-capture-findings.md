# W-0107 — Rà soát bằng stack thật: 3 lỗi từ điển mà test không thấy được

> **HISTORICAL_EVIDENCE — overlay 2026-08-27:** Capture này thuộc baseline 2026-08-22.
> `TASK_SKIPPED_TRUSTED_CUSTOMER` trong inventory bên dưới chỉ còn `LEGACY_READ` sau
> `OD-18`/W-0123; không dùng dấu ✅ tại đây để suy ra runtime hiện hành còn emit.

| | |
| --- | --- |
| Nguồn | Chạy `tools/dev/Capture-ConsoleEvidence.mjs` trên stack đang chạy |
| Ngày | 2026-08-22 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=false` (API tự khai, §A của bản capture) |
| Dữ liệu thô | [`live-enum-coverage.txt`](live-enum-coverage.txt) |
| Trạng thái | `FINDINGS_FIXED` — cả 3 lỗi đã sửa và kiểm chứng lại trên stack thật (xem §6) |

## 1. Vì sao test xanh mà màn hình vẫn sai

`enum-coverage.test.ts` chứng minh từ điển phủ **mọi enum mà OpenAPI khai báo**. Đó là một
tập khác với **mọi giá trị mà API thật trả về**.

Ba field dưới đây được spec khai là `{ type: string }` — chuỗi mở, **không có `enum:`**:

| Field | Dòng trong spec |
| --- | --- |
| `eligibility_decision` | `specs/api/openapi/ivr-order-confirmation.v1.yaml:1827` |
| `delivery_status` | `…:1498`, `…:1739` |
| `attempts[].status` | không khai enum |

Không có gì để liệt kê ⇒ test không có gì để kiểm ⇒ test xanh. Nhưng runtime vẫn đổ giá trị
vào đó, `EnumLabel` vẫn tra từ điển, và khi tra trượt thì `tEnum` (NT-4) trả về **mã thô kèm
dấu cảnh báo**. Đúng thứ W-0107 sinh ra để xoá bỏ.

Kiểm tra chiều còn lại — *từ điển có khớp thực tế không* — chưa từng tồn tại. Bản capture này
là lần đầu chạy nó.

## 2. Bốn ứng viên, ba lỗi thật

Bộ capture chỉ nêu **ứng viên**: nó ràng buộc field theo tên trần (`value={event.source}`), mà
tên trần thì đụng nhau. Mỗi dòng dưới đây được phán quyết bằng cách đọc file ràng buộc.

### 2.1 · `attempts[].status` — nặng nhất, sai cả họ

| | |
| --- | --- |
| Render tại | [`calls/[ivrCallJobId]/page.tsx:185`](../../../admin-ui/src/app/(console)/calls/[ivrCallJobId]/page.tsx) — `<EnumLabel family="jobStatus" value={attempt.status} />` |
| Nguồn giá trị | `ResultRepository.ApplyAttemptOutcome` — [`ResultRepository.cs:310`](../../../src/Ivr.Infrastructure/Repositories/ResultRepository.cs) |
| Giá trị thật | 10 giá trị — xem bảng dưới |
| Họ đang dùng | `jobStatus` — 27 giá trị, phủ được 6/10 |
| Phán quyết | **LỖI THẬT.** Không phải thiếu một nhãn — dùng nhầm họ. |

Trạng thái *lần gọi* đang được render qua từ điển trạng thái *job*.

**Đính chính so với bản đầu của tài liệu này:** hai bộ phân loại **không** rời nhau hoàn toàn.
Chúng trùng nhau ở 6 trạng thái giai đoạn quay số — và **đó chính là lý do lỗi sống sót lâu**.
Suốt thời gian cuộc gọi còn chạy, dòng thời gian hiện nhãn đúng; chỉ khi chuẩn hoá xong nó mới
rơi sang 4 mã `NORMALIZED_*` mà `jobStatus` không có. Người kiểm thử nhìn một cuộc gọi đang chạy
sẽ thấy mọi thứ bình thường.

| Giá trị | `jobStatus` phủ? | Nguồn |
| --- | --- | --- |
| `LEASED_PENDING_DISPATCH` | ✅ | `PostgresSchedulerStore.cs:184` (lúc tạo dòng) |
| `DIALING` | ✅ | `AdminReadService.cs:62` liệt là trạng thái đang hoạt động |
| `ACTIVE_CALL` | ✅ | `PostgresTelephonyDispatchStore.cs:116` |
| `PROVIDER_EVENT_PENDING_NORMALIZATION` | ✅ | `PostgresTelephonyDispatchStore.cs:235` |
| `TECHNICAL_RETRY_QUEUED` | ✅ | `InternalAdminApiService.cs:420` |
| `RECOVERY_REQUIRED` | ✅ | `PostgresSchedulerStore.cs:295` |
| `NORMALIZED_ATTEMPT_COMPLETE` | ❌ | `ResultRepository.cs:310` |
| `NORMALIZED_FINAL` | ❌ | `ResultRepository.cs:310` |
| `NORMALIZED_TECHNICAL_RETRY` | ❌ | `ResultRepository.cs:310` |
| `NORMALIZED_REVIEW_REQUIRED` | ❌ | `ResultRepository.cs:310` |

Hệ quả: **mọi lần gọi đã chuẩn hoá xong đều hiện mã thô**. Không phải trường hợp biên — đó là
trạng thái cuối của mọi lần gọi đã kết thúc.

Đã thấy trực tiếp: `GET /call-jobs/{id}/detail.attempts[].status = NORMALIZED_FINAL`.

### 2.2 · `callbacks[].delivery_status` — từ điển gần như không dính thực tế

| | |
| --- | --- |
| Render tại | [`calls/[ivrCallJobId]/page.tsx:285`](../../../admin-ui/src/app/(console)/calls/[ivrCallJobId]/page.tsx) — `family="deliveryStatus"` |
| Nguồn giá trị | `CallbackOutboxRepository.AllowedDeliveryStatuses` — danh sách trắng cứng, [`CallbackOutboxRepository.cs:56`](../../../src/Ivr.Infrastructure/Persistence/Outbox/CallbackOutboxRepository.cs) |
| Phán quyết | **LỖI THẬT** |

| Từ điển `deliveryStatus` (7) | Có thật trong code? |
| --- | --- |
| `READY` | ✅ `CallbackOutboxSnapshotFactory.cs:89` |
| `SENDING` | ✅ `CallbackOutboxRepository.cs:130` |
| `RETRY_PENDING` | ✅ trong danh sách trắng |
| `SENT` | ❌ **0 lần xuất hiện trong `src/`** |
| `ACKED` | ❌ **0 lần** |
| `DEAD_LETTER` | ❌ **0 lần** |
| `FAILED` | ❌ chỉ tồn tại như một enum member của contract Golden Hour hiện hành, không phải delivery status |

Chiều ngược lại — 8 giá trị có thật nhưng **không có nhãn**: `DELIVERED_ACCEPTED`,
`DELIVERED_BLOCKED`, `DELIVERED_REVIEW`, `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT`,
`INVALID_DEAD_LETTER`, `AUTH_REJECTED`, `RETRY_EXHAUSTED`.

Tổng kết: 12 giá trị có thật, **3** được dịch. 4 mục trong từ điển trỏ vào giá trị mà không
nhánh thực thi nào sinh ra được.

Đã thấy trực tiếp: `callbacks[].delivery_status = DELIVERED_ACCEPTED` (kèm `core_http_status: 200`).

### 2.3 · `eligibility_decision` — lấy nhầm bộ phân loại

| | |
| --- | --- |
| Render tại | [`calls/[ivrCallJobId]/page.tsx:111`](../../../admin-ui/src/app/(console)/calls/[ivrCallJobId]/page.tsx) — `family="eligibilityDecision"` |
| Nguồn giá trị | `job.EligibilityDecision`, thẩm quyền là `EligibilityDecisions` — [`EligibilityRules.cs:3`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs) |
| Phán quyết | **LỖI THẬT** |

Từ điển được dựng từ enum **quyết định tiếp nhận** (12 giá trị `TASK_*`, có khai trong spec nên
test kiểm được). Nhưng field mà màn hình render lại là **quyết định điều kiện gọi**, 6 giá trị:

| Giá trị thật | Có nhãn? |
| --- | --- |
| `TASK_BLOCKED_OPERATIONAL` | ✅ |
| `TASK_HELD_ADMIN_REVIEW` | ✅ |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | ✅ |
| `PENDING_ELIGIBILITY` | ❌ |
| `ELIGIBLE_FOR_IVR` | ❌ |
| `IVR_CAPACITY_EXCEPTION` | ❌ |

`ELIGIBLE_FOR_IVR` là trạng thái **phổ biến nhất** của field này — mọi đơn đủ điều kiện gọi đều
mang giá trị đó. Nghĩa là ô này hiện mã thô ở phần lớn đơn.

Đã thấy trực tiếp: `detail.eligibility_decision = ELIGIBLE_FOR_IVR`.

### 2.4 · `data_quality.source` — **dương tính giả**, không sửa gì

Bộ capture nêu `source = ANALYTICS_WAREHOUSE` là ứng viên vì tên trần `source` đụng với
`event.source` bên màn tích hợp. Đọc file ràng buộc thì thấy khác:

[`FreshnessBanner.tsx:68`](../../../admin-ui/src/components/reports/FreshnessBanner.tsx) in giá trị
này **thẳng vào trong câu** — `` `${t("reports.sourceWarehouse")} (${quality.source})` `` — cố ý
không đi qua `EnumLabel`. Comment ngay trên đó nói rõ: nguồn dữ liệu được nêu **như một dữ kiện**,
không phải cảnh báo.

**Không phải lỗi.** Ghi lại ở đây vì lần capture sau sẽ gặp lại đúng dòng này, và một ứng viên
đã bị bác nên được bác bằng hồ sơ chứ không bằng trí nhớ.

## 3. Những gì bản capture chứng minh thêm

| Khẳng định | Bằng chứng |
| --- | --- |
| `voice_region` của W-0106 đã sống trên API | `detail.voice_region = "South"` → nhãn `Miền Nam` |
| Cổng phiên W-0105 chặn đúng | 11/11 route bảo vệ trả `307 → /login?next=…`; `/login` trả `200` |
| 19 giá trị enum khác dịch đúng trên dữ liệu thật | §C của bản capture |
| API cũ cổng 5005 vẫn thiếu `/analytics`, `/sim-channels` | 404; capture chạy trên cổng 5015 riêng, không đụng tiến trình của owner |

## 4. Đề xuất (đã thực hiện — xem §5)

Ba lỗi trên **không** làm hỏng màn hình — `tEnum` (NT-4) trả mã thô kèm dấu ⚠, đúng thiết kế
fail-safe. Nhưng chúng làm hỏng đúng mục tiêu W-0107: người vận hành mở màn chi tiết để quyết
định một đơn còn sống hay đã chết, và ba ô quan trọng nhất ở đó đang hiện `NORMALIZED_FINAL`,
`DELIVERED_ACCEPTED`, `ELIGIBLE_FOR_IVR`.

Việc cần làm, `Origin=UNPLANNED`, cấp Work ID từ `NEXT_WORK_ID` tại thời điểm ghi sổ:

> Bản đầu của mục này đề xuất `W-0108`. Trong lúc phần sửa đang chạy, một luồng khác đã cấp
> `W-0108` cho chuỗi ghép audio động — bộ đếm giờ ở `W-0109`. Không tự cấp ở đây: nhiều luồng
> đang dùng chung bộ đếm, và hai luồng cùng đọc `NEXT_WORK_ID` rồi cùng ghi là cách chắc chắn
> nhất để hai việc khác nhau mang cùng một mã.

1. Bổ sung **15 giá trị** vào từ điển: 4 trạng thái lần gọi + 8 delivery status + 3 eligibility decision.
2. Quyết định về 4 mục chết trong `deliveryStatus` (`SENT`/`ACKED`/`DEAD_LETTER`/`FAILED`):
   xoá, hay giữ vì có nhánh thực thi tương lai. Đây là quyết định của chủ từ điển, không phải
   của bộ capture.
3. Cân nhắc tách họ `attemptStatus` riêng thay vì nhồi vào `jobStatus` — hai bộ phân loại này
   giao nhau bằng rỗng, gộp chung là nguồn gốc của lỗi 2.1.
4. **Chốt lỗ hổng**: mở rộng `enum-coverage.test.ts` để đọc thêm các hằng số C# có thẩm quyền
   (`EligibilityDecisions`, `AllowedDeliveryStatuses`, `ApplyAttemptOutcome`) — cùng cách nó đã
   đọc file OpenAPI. Không có bước này, từ điển sẽ trôi lại y như vậy.

## 5. Đã sửa — và đã kiểm chứng lại trên stack thật

| Việc | Kết quả |
| --- | --- |
| Họ mới `attemptStatus` (10 giá trị) | `calls/[ivrCallJobId]/page.tsx` đổi từ `family="jobStatus"` sang `family="attemptStatus"` |
| `deliveryStatus` | +8 giá trị có thật; **xoá 4 mã chết** (`SENT`, `ACKED`, `FAILED`, `DEAD_LETTER`) |
| `eligibilityDecision` | +3 giá trị (`PENDING_ELIGIBILITY`, `ELIGIBLE_FOR_IVR`, `IVR_CAPACITY_EXCEPTION`) |
| Từ điển | 39 họ / 212 giá trị → **40 họ / 229 giá trị** |

### 5.1 · Chốt lỗ hổng, không chỉ vá triệu chứng

`enum-coverage.test.ts` có thêm một phép kiểm đọc **chính các hằng số C# có thẩm quyền**, cùng
cách nó đã đọc file OpenAPI:

| Họ | Thẩm quyền được đọc |
| --- | --- |
| `eligibilityDecision` | lớp `EligibilityDecisions` |
| `deliveryStatus` | `AllowedDeliveryStatuses` + hai chỗ ghi trực tiếp |
| `attemptStatus` | mọi chỗ ghi `CallAttemptEntity.Status` trong `src/` |

Chiều kiểm là **một chiều có chủ ý**: mọi giá trị backend sinh ra được **phải** có nhãn. Chiều
ngược lại không kiểm — một họ có quyền chứa giá trị spec mà runtime chưa phát ra, và bắt lỗi
điều đó là phạt người làm sớm chứ không phải người làm sai.

Mọi bộ đọc **ném lỗi khi không tìm thấy gì**. Lớp bị đổi tên mà bộ đọc trả về tập rỗng sẽ khiến
phép kiểm xanh đúng vào lúc nó cần đỏ.

### 5.2 · Bốn phép thử đột biến

| # | Đột biến | Kỳ vọng | Thực tế |
| --- | --- | --- | --- |
| M1 | Xoá `attemptStatus.NORMALIZED_FINAL` | đỏ | ✅ đỏ, nêu đúng mã và nguồn |
| M2 | Xoá `eligibilityDecision.ELIGIBLE_FOR_IVR` | đỏ | ✅ đỏ |
| M3 | Đổi tên lớp `EligibilityDecisions` | đỏ | ❌ **xanh lần đầu** — xem dưới |
| M4 | Đổi tên `AllowedDeliveryStatuses` | đỏ | ✅ đỏ sau khi sửa |

M3 phát hiện một lỗi trong chính guard: `indexOf("class EligibilityDecisions")` vẫn khớp tiền tố
của `class EligibilityDecisionsRenamed`, nên bộ đọc tưởng vẫn tìm thấy và đọc y nguyên bộ hằng số
cũ. Đã sửa thành khớp theo **biên từ**; M3 và M4 sau đó đều đỏ đúng như kỳ vọng.

Đây đúng là kiểu hỏng mà tài liệu này nói tới: một phép kiểm báo PASS ở chính tình huống nó
được viết ra để bắt.

### 5.3 · Một giá trị thứ 10 mà rà tay đã bỏ sót

Bộ quét tự tìm ra `DIALING`, thứ mà `grep` thủ công của tôi không thấy. Nó không có chỗ ghi nào —
nhưng `AdminReadService.cs:62` và `InternalAdminApiService.cs:226` **đều liệt nó là trạng thái
lần gọi đang hoạt động**. Đã cấp nhãn: nhánh đọc coi nó là trạng thái đến được thì màn hình phải
đọc được nó.

Ngược lại, `SENT`/`ACKED`/`FAILED`/`DEAD_LETTER` bị xoá vì **không chỗ nào trong repo nhắc tới
chúng** như một delivery status, và không CHECK constraint nào ràng buộc. Khác biệt giữa hai
quyết định nằm ở đó, không ở khẩu vị.

### 5.4 · Kiểm chứng

| Cổng | Kết quả |
| --- | --- |
| `Capture-ConsoleEvidence.mjs` chạy lại trên stack thật | **0 gap** (exit 0); 1 ứng viên đã phán quyết |
| admin-ui vitest | **210/210** |
| `tsc --noEmit` · `eslint --max-warnings 0` | exit 0 · exit 0 |
| `scan-pii.sh docs/evidence ci-artifacts` | `PII_SCAN_PASS` |

Ba ô trên màn chi tiết giờ đọc được: `Xong lượt gọi — đã chốt kết quả cuối`, `Core đã nhận`,
`Đủ điều kiện gọi`.

## 6. Không khẳng định

- Tracker chưa cập nhật. Bản sửa này chưa được cấp Work ID và chưa có Activity entry — chờ owner (§5).
- Chưa chụp được màn hình **sau khi đăng nhập** — cần một phiên console thật, mà mật khẩu
  bootstrap không nằm trong repo (đúng như W-0105 yêu cầu). Phần §E chỉ chứng minh cổng chặn,
  không chứng minh nội dung màn hình bên trong.
- Bộ dữ liệu là fixture MOCK có sẵn trong DB dev (`ivr_call_jobs` 35 dòng, `ivr_call_results`
  10 dòng; `/call-jobs` trả 25, capture lấy mẫu 12 job detail), **không** phải fixture 18 job/14
  kết quả mà W-0102 đã dựng. Các giá trị enum chưa xuất hiện trong fixture này thì
  bản capture không thể nói gì về chúng — độ phủ 23 giá trị là **cận dưới**, không phải toàn bộ.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` suốt quá trình; không cuộc gọi nào, không SIM thật, không
  endpoint Sales thật.
