# Đơn vị volume và độ dài phiên — phần mà `M8-OD-C` chưa phủ

**Work ID:** `W-0133` · **Ngày:** 2026-08-28 · **Baseline:** `main@8d28ba1`
**Trạng thái:** `OWNER_DECISION_REQUIRED` (business) + `DESIGN_UNAPPROVED` (IVR-side)

## 1. Cái đã có chủ — đừng viết lại

Spec `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md` §14.2 đã mở **`M8-OD-C`** ngày
27/08/2026 và đã hỏi đúng hai câu:

1. một phiên Giờ Vàng kéo dài bao lâu;
2. trong phiên đó cao điểm có bao nhiêu đơn.

`M8-OD-C` cũng đã tự chỉ ra ba phát biểu trong V0.3 không thể cùng đúng, rằng chúng chỉ hòa giải
được nếu "phiên" = 45 phút, và rằng **con số 45 phút không có nguồn** — nó xuất hiện đúng một lần
ở tiêu đề cột §14.1.

**Tài liệu này không mở decision mới cho hai câu đó.** `M8-OD-C` là authority.

## 2. Cái `M8-OD-C` chưa phủ — và là lý do W-0133 tồn tại

`M8-OD-C` là câu hỏi nghiệp vụ. Nhưng ngay cả khi Owner trả lời xong, **model sizing của IVR không
tiêu thụ được câu trả lời đó**. Ba lý do, đã verify trên code tại `8d28ba1`:

### 2.1. Model không có input độ dài phiên nào cả

`poolForProgramme` trong [`tools/capacity-sim/capacity-model.mjs`](../../tools/capacity-sim/capacity-model.mjs)
tính số kênh bằng `windowSeconds: policy.windowSeconds` — tức **300s cho GOLDEN_HOUR** và 900s cho
`TWENTY_FOUR_SEVEN`. Đó là **confirmation window của từng đơn**, không phải độ dài phiên.

Không có hằng số, tham số hay field nào tên session length trong model. Nên câu trả lời "phiên dài
45 phút" **không có chỗ để điền**.

### 2.2. `peakShare` không có gốc thời gian

Model nhận `peakShare: 0.15` (GH) và `0.1` (24/7) như một tỉ lệ của đơn eligible trong **ngày**.
Không có phát biểu nào nói tỉ lệ đó tập trung trong bao lâu. Hai người đọc `peakShare: 0.15` hoàn
toàn có thể hiểu khác nhau, và không gate nào bắt được.

Đây là chỗ độ dài phiên *lẽ ra* phải sống, và nó đang trống.

### 2.3. Chính spec dùng 50s trong khi model dùng 45s

Phép hòa giải của `M8-OD-C` là `32 × (2700 ÷ 50) = 1728`. Model dùng chu kỳ `40 + 5 = 45s`, cho
`32 × floor(2700 / 45) = 1920`.

Hai con số này thuộc đúng tập bất đồng mà `W-0132` đã khai báo trong `CALL_DURATION_ASSUMPTIONS` và
`CAP-DRIFT-05` đang canh. Chốt `M8-OD-C` mà không chốt luôn chu kỳ cuộc gọi thì vẫn còn hai đáp án.

## 3. Số để Owner quyết — tính bằng chính model của repo

### 3.1. Khoảng cách giữa hai cách đọc

| Cách đọc "800" | Số kênh cần |
| --- | --- |
| `dailyOrders = 800` (model đang chạy thế này) | **9** |
| `dailyOrders = 2 000` (`UNCALIBRATED_SCENARIO`, con số `21` mà `CAP-MODEL-01` báo) | **21** |
| 800 **cuộc/phiên**, phiên 45 phút | **14** |
| 800 **cuộc/phiên**, phiên 15 phút | **40** |
| 800 **cuộc/phiên**, phiên 5 phút | **134** |

Prod đang ship `worker.hpa.simPoolSize = 32`.

### 3.2. Số kênh theo độ dài phiên, nếu 800–1200 là **cuộc/phiên**

Tính bằng `channelsForWindow` của repo, chu kỳ 45s (`40 + 5`):

| Độ dài phiên | 800 cuộc | 1 200 cuộc | 32 kênh có đủ? |
| --- | --- | --- | --- |
| 5 phút | 134 | 200 | ❌ không |
| 15 phút | 40 | 60 | ❌ không |
| 30 phút | 20 | 30 | ⚠️ vừa sát |
| 45 phút | 14 | 20 | ✅ đủ |
| 60 phút | 10 | 15 | ✅ dư |
| 120 phút | 5 | 8 | ✅ dư nhiều |

**Ranh giới đủ/không đủ rơi quanh mốc phiên 30 phút.** Đây là lý do câu hỏi không hàn lâm: nó đổi
kết luận mua sắm từ "32 dư" sang "32 thiếu bốn lần".

Bảng trên là **cận dưới**. Nó chỉ chia đều số cuộc cho độ dài phiên, chưa tính ràng buộc mỗi đơn
còn phải được gọi trong confirmation window 5 phút của riêng nó. Ràng buộc đó chỉ làm số kênh cần
**tăng lên**, không giảm.

## 4. Đề nghị

**Owner (`M8-OD-C`, business):** chốt độ dài phiên và số đơn cao điểm trong phiên. Không có hai con
số này thì mọi số SIM ở §14 — kể cả 12 cho pilot — vẫn là phỏng đoán, đúng như `M8-OD-C` đã viết.

**IVR-side (`OD-19`, kỹ thuật — cần chốt cùng lúc):** quyết định model tiêu thụ câu trả lời đó bằng
cách nào. Hai hướng:

1. **Thêm `sessionSeconds` vào scenario** và cho `poolForProgramme` sizing theo
   `min(sessionSeconds, policy.windowSeconds)` hoặc theo arrival profile thật — model nói đúng thứ
   Owner trả lời, nhưng phải sửa model và mọi kịch bản selftest.
2. **Giữ model nguyên** và ghi rõ quy tắc quy đổi `(độ dài phiên, đơn cao điểm) → (dailyOrders,
   peakShare)`, kèm gate chặn drift — rẻ hơn, nhưng để lại một phép quy đổi bằng tay giữa câu trả
   lời của Owner và input của model.

Đề xuất hướng **1**: giữ quy đổi bằng tay chính là cách `peakShare` mất gốc thời gian ngay từ đầu.

## 5. W-0133 KHÔNG làm gì

- Không chốt thay Owner độ dài phiên hay volume.
- Không sửa model, không đổi `peakShare`, không đổi `simPoolSize`.
- Không đụng `CALL_DURATION_ASSUMPTIONS` — chu kỳ cuộc gọi vẫn chờ `W-0008`.
- Không dùng bảng §3.2 làm căn cứ mua. Nó là decision support, chạy trên một model tự khai
  `UNCALIBRATED`.
