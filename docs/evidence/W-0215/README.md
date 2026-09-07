# W-0215 — Tích của hai chữ ký: mốc cắt chưa ai viết ra

Ngày: 2026-09-07 · Baseline: `main@73c0f0a` · Trạng thái: **TESTS_PASS** cho phần ghim và công bố;
**OWNER_DECISION_REQUIRED** cho việc chọn `End`.

## 1. Hai quyết định đúng, nhân với nhau ra một vấn đề

Ngày `2026-09-05`, cùng một lượt ký:

- `OD-V1-16` — khung giờ gọi `08:00–21:00` ICT.
- `OD-V1-08` — attempt 2 ở `T0 + 150s` (Giờ Vàng), `T0 + 450s` (24/7).

Không quyết định nào sai. **Tích của chúng thì chưa ai tính:**

| Program | Offset attempt 2 | T0 muộn nhất còn đủ hai cuộc |
| --- | ---: | ---: |
| `TWENTY_FOUR_SEVEN` | `450s` | **20:52:30** |
| `GOLDEN_HOUR` | `150s` | **20:57:30** |

### 1.1. Chương trình tên "24/7" lại cắt sớm hơn

Sớm hơn Giờ Vàng **5 phút**. Tên `TWENTY_FOUR_SEVEN` nói về lúc Sales nhận đơn, không phải lúc IVR
được gọi — khung giờ **không theo program** (`AttemptPolicyRegistries` ghi rõ: *"The hours of day a
call may be placed are not here. They are not per-program"*). Attempt 2 dài `450s` phải vượt cùng
một mốc `21:00`, nên nó thiệt hơn.

Đây là loại kết luận dễ đọc ngược nếu chỉ nhìn tên chương trình.

## 2. Hệ quả nặng hơn "mất một cuộc gọi"

Task phát sau mốc vẫn được nhận và vẫn được gọi **một** lần. Nếu khách không nghe:

| Tình huống | `result_type` | `recommended_core_action` |
| --- | --- | --- |
| Đủ 2 attempt, không nghe | `IVR_NO_ANSWER_FINAL` | `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` |
| Attempt 2 rơi ngoài giờ gọi | `IVR_CONFIRMATION_WINDOW_EXPIRED` | `REVALIDATE_AND_EXPIRE_CONFIRMATION` / `REVALIDATE_AND_HOLD_ADMIN_REVIEW` |

Finality đến từ việc **dùng hết attempt**; attempt không được dùng hết thì window hết hạn trước.
Nên **cùng một hành vi khách hàng cho ra hai kết quả khác nhau, quyết bởi đồng hồ treo tường** — và
hai kết quả đó dẫn Core đi hai đường khác nhau.

Consumer của M3 phải xử lý được cả hai cho cùng một kịch bản "khách không nghe máy". Trước lượt này
tài liệu bàn giao không nói gì về điều đó.

## 3. Đã làm gì

**Không đổi tham số nào.** Chọn `End` là quyết định của owner, và mục này tồn tại để owner có con số
thật mà quyết.

### 3.1. `UT-SCH-WINDOW-09` — suy, không gõ

`tests/Ivr.UnitTests/Scheduling/CallingWindowTests.cs`. Test **đọc** offset từ
`SignedProductionAttemptPolicies.Create()` và `End` từ `CallingWindowOptions`, rồi tính mốc cắt —
thay vì hard-code `20:57:30`. Nghĩa là đổi policy hoặc đổi window thì test **đỏ**, và có người phải
nhìn lại.

| Khẳng định | |
| --- | --- |
| offset GH = `150s`, offset 24/7 = `450s` | đọc từ bản ký |
| cutoff GH = `20:57:30` | suy ra |
| cutoff 24/7 = `20:52:30` | suy ra |
| tại `20:57:30` → `T0` mở, `T0+150s` **đóng** | mất attempt 2 |
| tại `20:57:29` → cả hai **mở** | vừa đủ |
| `T0+300s` (hết window) đóng | expiry là bookkeeping, chạy mọi giờ — không phải vấn đề |

Test **không** khẳng định `20:57:30` là mốc *đúng*. Nó khẳng định mốc nằm ở nơi hai chữ ký đặt nó.

### 3.2. Công bố cho M3 — IR-06 §3.4.2 mới

Bảng hai mốc cắt, cảnh báo về tên `TWENTY_FOUR_SEVEN`, và bảng hai `result_type` mà consumer phải
xử lý cho cùng một kịch bản. Trước đây M3 không có cách nào biết điều này từ tài liệu.

## 4. Kiểm chứng

```text
dotnet test Ivr.sln                        897/897 PASS, 0 failed, 0 skipped
                                           (Unit 596 · Integration 269 · Contract 24 · Chaos 8)
dotnet test --filter UT-SCH-WINDOW-09      1/1 PASS
contract-freeze-verifier.mjs               CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check     TEST_TRACEABILITY_CURRENT=556  (555→556)
docs-selftest.mjs                          API_DOCS_SELFTEST_PASS
```

Không sửa runtime, không sửa OpenAPI, không đổi tham số khung giờ.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại — owner + M3

1. **Chọn `End`.** Nếu muốn đơn đặt tới đúng `21:00` vẫn đủ hai cuộc thì con số đủ cho **cả hai**
   program là `End ≥ 21:07:30` — **không phải `21:05`** như bản audit đề xuất; `21:05` chỉ cứu được
   Giờ Vàng và vẫn bỏ rơi 24/7.
2. **Hoặc** M3 ngừng phát task từ `20:52:30` (24/7) / `20:57:30` (GH).
3. **`LOCK-05` vẫn không có nguồn.** Phiên Giờ Vàng `20:15–21:00` chỉ xuất hiện trong bản audit
   07/09, không có trong bất kỳ file nào của repo này. Nếu nó là căn cứ để chọn `End` thì phải xin
   nguồn từ M3/owner trước — đừng lấy con số từ một bản kiểm.
4. Dù chọn gì, `UT-SCH-WINDOW-09` sẽ đỏ. Sửa có chủ đích.

Ghi chú phạm vi: mục này nói về **giờ được phép quay số**. Việc confirmation window sống dài hơn
khung giờ **không** phải lỗi — `SchedulerRuntime` cố ý cho recovery và deadline-closing chạy mọi
giờ; chỉ dialling dừng.
