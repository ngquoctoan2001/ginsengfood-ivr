# W-0133 — Đơn vị volume + độ dài phiên: phần `M8-OD-C` chưa phủ

Ngày: `2026-08-28`
Baseline: `main@8d28ba1`
Trạng thái: `DECISION_REQUEST_SUBMITTED`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu

B1 sub-task 2 ghi: *"trình Owner xác nhận input volume 800-1200 cuộc/phiên thay giả định 800
đơn/ngày"*. W-0133 làm việc đó — nhưng phạm vi bị thu hẹp lại đáng kể sau khi đọc spec.

**Docs-only.** Không sửa một dòng code nào.

## 2. Phát hiện đầu tiên: việc này đã có chủ

Spec §14.2 đã mở **`M8-OD-C`** ngày 27/08/2026 và đã hỏi đúng hai câu W-0133 định hỏi: phiên dài
bao lâu, và cao điểm bao nhiêu đơn. `M8-OD-C` còn đi xa hơn — nó tự chỉ ra ba phát biểu trong V0.3
không thể cùng đúng, rằng chúng chỉ hòa giải nếu phiên = 45 phút, và rằng **45 phút không có
nguồn**.

Nên W-0133 **không mở decision mới** cho hai câu đó và không viết lại escalation. Làm thế là nhân
bản tài liệu — đúng thứ cross-audit vẫn phê. `M8-OD-C` là authority.

## 3. Phát hiện thật: model không tiêu thụ được câu trả lời

Đây là phần `M8-OD-C` không phủ, và là lý do W-0133 vẫn có giá trị. Verify trên code tại `8d28ba1`:

1. **Không có input độ dài phiên.** `poolForProgramme` sizing bằng `windowSeconds:
   policy.windowSeconds` = `300s` (GH) / `900s` (24/7) — đó là **confirmation window của từng đơn**,
   không phải phiên. Không hằng số/tham số/field nào tên session length tồn tại trong model. Owner
   trả lời "45 phút" thì câu trả lời **không có chỗ để điền**.
2. **`peakShare` không có gốc thời gian.** `0.15`/`0.1` là tỉ lệ của đơn eligible trong *ngày*,
   không kèm phát biểu nào về việc tỉ lệ đó dồn trong bao lâu. Đây đúng là chỗ độ dài phiên lẽ ra
   phải sống, và nó trống.
3. **Spec dùng 50s, model dùng 45s.** Hòa giải của `M8-OD-C` là `32 × (2700÷50) = 1728`; model cho
   `32 × floor(2700/45) = 1920`. Thuộc tập bất đồng `CALL_DURATION_ASSUMPTIONS` mà `W-0132` khai báo
   và `CAP-DRIFT-05` đang canh.

→ Cấp `OD-19` PENDING trong `decisions-log.md` cho riêng phần kỹ thuật này, trỏ `M8-OD-C` làm
authority của phần nghiệp vụ.

## 4. Số đo — chạy bằng chính model của repo

| Cách đọc "800" | Kênh cần |
| --- | --- |
| `dailyOrders = 800` | `9` |
| `dailyOrders = 2 000` (`UNCALIBRATED_SCENARIO`) | `21` |
| 800 cuộc/phiên, phiên 45′ | `14` |
| 800 cuộc/phiên, phiên 15′ | `40` |
| 800 cuộc/phiên, phiên 5′ | `134` |

Prod ship `simPoolSize = 32`. Ranh giới đủ/không đủ rơi quanh **phiên 30 phút** (800→20, 1200→30).

### Đính chính trong lượt này

Trong hội thoại tôi từng nói model ra `21` kênh ở `dailyOrders=800`. **Sai.** `21` là kết quả của
`UNCALIBRATED_SCENARIO` (`dailyOrders=2000`) mà `CAP-MODEL-01` báo; ở `800` model ra `9`. Bảng trên
là số đã chạy lại và đúng.

## 5. Verification

| Kiểm | Kết quả |
| --- | --- |
| `poolForDay` @ `dailyOrders=800` | GH `7` + 24/7 `2` = **`9`** kênh |
| `channelsForWindow` theo độ dài phiên | `134/40/20/14/10/5` (800 cuộc, phiên `5/15/30/45/60/120′`) |
| `poolForProgramme` dùng gốc thời gian nào | `policy.windowSeconds` — confirmation window, **không** phải phiên |
| `grep` session length trong model | không tồn tại |
| Code bị sửa | `0 file` |

## 6. Residual gate

- `OWNER_DECISION_REQUIRED`: `M8-OD-C` — phiên dài bao lâu, cao điểm bao nhiêu đơn.
- `DESIGN_UNAPPROVED`: `OD-19` — model tiêu thụ câu trả lời bằng cách nào (thêm `sessionSeconds` vs
  giữ model + quy tắc quy đổi). Đề xuất hướng 1.
- `NOT_CALIBRATED`: chu kỳ cuộc gọi vẫn chờ `W-0008`. Chốt `M8-OD-C` mà không chốt chu kỳ thì vẫn
  còn hai đáp án.
- Bảng §4 **không** được dùng làm căn cứ mua: model tự khai `UNCALIBRATED`.
