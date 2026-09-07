# W-0212 — Con số thứ tư, và mười giây không ai giải thích được

Ngày: 2026-09-07 · Baseline: `main@d9e6d3f` · Trạng thái: **TESTS_PASS**, mục 0.6 **đóng hẳn**.

## 1. Mục 0.6 vốn đã đúng

Vào kiểm thì phần Codex nêu (F05) **đã được xử lý sẵn từ `W-0132`**:

- `tools/capacity-sim/capacity-model.mjs` ghi rõ `40s` là occupancy, `50s` là *full channel cycle*,
  `60s` là occupancy của runtime, và nói thẳng *"unifying them is not a code decision … CAP-DRIFT-05
  deliberately does **NOT** require all three numbers to be equal, which would double-count
  cooldown"*.
- `docs/capacity-model.md` §4a có sẵn bảng ba con số kèm cảnh báo **"Không làm ba con số bằng nhau"**.
- Gate nói thẳng khi chạy:
  `CAP-DRIFT-05 PASS_DECLARED_DISAGREEMENT — … They disagree **by design**`.

Chỗ sai duy nhất là câu chữ trong **bản audit gốc** (*"ba con số ba nơi"*, đọc như một lỗi cần hòa
giải), và bản worklist sạch đã sửa từ `cf4bd4a`. Nếu dừng ở đây thì 0.6 là mục rỗng.

## 2. Nhưng có con số thứ tư

Spec V0.3 §14 (bảng giả định) viết ra một **cặp**, không phải riêng chu kỳ:

| Spec ghi | Giá trị | Nguồn spec ghi |
| --- | --- | --- |
| `AVERAGE_CALL_DURATION` | **35 giây** | giả định V0.2 §11 |
| `CONSERVATIVE_CALL_CYCLE` | **50 giây/cuộc/SIM** | giả định V0.2 §11 |
| SIM cooldown sau cuộc gọi | **5 giây** | mặc định mock |

`W-0132` lấy `50` và **bỏ lại `35`**. Kết quả: ba trong bốn con số cùng họ được `CAP-DRIFT-05` canh,
con số thứ tư thì không — nó có thể đổi mà mọi gate vẫn xanh, đúng loại lỗi `W-0132` sinh ra để chặn.

### 2.1. Và ba con số của chính spec không khớp nhau

```text
35s (occupancy)  +  5s (cooldown)  =  40s
                                      ≠  50s (cycle)
```

**Mười giây không có nguồn.** Có thể là thời gian đổ chuông/thiết lập mà dòng cooldown không phủ; có
thể chỉ là một con số cũ chưa được dẫn lại khi V0.2 §11 sang V0.3 §14. **Không quyết được từ tài
liệu.**

Chênh lệch này không vô hại. `channelsForWindow` chia window cho `callSeconds + cooldown`:

| Giả định | Chu kỳ | Cuộc/kênh, window 300s | Cuộc/kênh, window 900s |
| --- | ---: | ---: | ---: |
| Model (`40+5`) | 45s | `floor(300/45)` = **6** | `floor(900/45)` = **20** |
| Spec (`35+15`) | 50s | `floor(300/50)` = **6** | `floor(900/50)` = **18** |

Trùng nhau ở window Giờ Vàng, **lệch 10%** ở window 24/7. Đủ để đổi số SIM phải mua.

### 2.2. Comment của gate cũng sai

`capacity-selftest.mjs` ghi *"The spec never writes 50s down -- it writes ~192 calls for 32 SIM"*.
Spec **có** viết, cả `50` lẫn `35`, thành hai dòng trong bảng §14. Câu đó có lẽ đúng với một bản
spec cũ hơn. Đã sửa.

## 3. Đã làm gì

**Không đổi một giá trị nào**, và **không ép quan hệ nào** giữa `35` và `50`. Chọn đáp án cho mười
giây kia là tuyên bố một phép đo — đúng thứ `CALL_DURATION_ASSUMPTIONS` sinh ra để không làm.

- `tools/capacity-sim/capacity-model.mjs` — thêm `specAverageCallSeconds: 35` kèm lý do đầy đủ.
- `deploy/ci/scripts/capacity-selftest.mjs` — `CAP-DRIFT-05` ghim `35`; sửa comment *"spec never
  writes 50s down"*; dòng output nay in đủ bốn con số.
- `docs/capacity-model.md` §4a — bảng bốn con số, ghi mười giây chênh, và lưu ý cho lúc calibrate.
- `plan/toan-viec-can-lam-m8-2026-09-07.md` — 0.6 đóng; **B1 được thêm phạm vi**: `W-0008` phải giải
  quyết luôn mười giây này.

Cố ý **không** assert quan hệ nào giữa `35` và `50`: pin thì phát hiện được drift, còn assert một
công thức thì sẽ đóng băng một phỏng đoán thành luật.

## 4. Kiểm chứng

```text
capacity-selftest.mjs
  CAP-MODEL-01  PASS — peak sizing 21 channels          (không đổi)
  CAP-SENS-02   PASS — 27 corners, 7..72 channels        (không đổi)
  CAP-CALIB-03  PASS_UNCALIBRATED
  CAP-DRIFT-05  PASS_DECLARED_DISAGREEMENT — model occupancy 40s, spec occupancy 35s,
                spec full-cycle 50s, runtime occupancy estimate 60s
  CAPACITY_SELFTEST_PASS_UNCALIBRATED

dotnet test Ivr.sln                       894/894 PASS, 0 failed, 0 skipped
contract-freeze-verifier.mjs              CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check    TEST_TRACEABILITY_CURRENT=553 (không đổi)
docs-selftest.mjs                         API_DOCS_SELFTEST_PASS
gitnexus impact CALL_DURATION_ASSUMPTIONS risk=LOW, 0 impacted
```

Hai bất biến hành vi (`21` kênh, `27` corner / `7..72`) **giống hệt trước thay đổi** — đó là bằng
chứng số học không đổi. Không sửa runtime C#, không sửa OpenAPI, không thêm/xoá test.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại

Mục 0.6 đóng. Mười giây chênh **không** đóng — nó chuyển sang `B1`/`W-0008` với trạng thái đã khai
báo và đã ghim, thay vì vô hình như trước.

Khi `W-0008` có số đo: đặt model/runtime bằng occupancy đo được, đặt chu kỳ spec bằng
`occupancy + cooldown`, và **dẫn lại `35`** — nó là con số của spec, không phải của model. Lưu ý
luật hiện hành dùng `+5` trong khi cặp lịch sử của spec dùng `+15`, nên `50` sẽ đổi; đó là hệ quả đã
biết, không phải drift.
