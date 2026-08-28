# W-0134 — Độ dài phiên: khai báo input, và chặn phép thay ẩu

Ngày: `2026-08-28`
Baseline: `main@8d28ba1`
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu và phạm vi đã thu hẹp

`OD-19` hướng 1 (đề xuất của `W-0133`) là "thêm `sessionSeconds` vào scenario và sizing theo nó".
W-0134 bắt đầu để làm việc đó, rồi **dừng lại sau khi đo** — vì làm thẳng thì sai và nguy hiểm.

W-0134 vì vậy chỉ: khai báo input, dựng gate, **không đổi một phép tính nào**.

## 2. Discovery — vì sao hướng 1 không làm thẳng được

Đo trên nhánh Giờ Vàng của `UNCALIBRATED_SCENARIO` (`dailyOrders=2000`), chu kỳ 45s:

| Sizing | Kênh |
| --- | --- |
| `windowSeconds = 300` (model hiện tại) | **16** |
| `sessionSeconds = 2700` (thay thẳng theo hướng 1) | **2** |
| Rải cao điểm ra từng lát 300s của phiên 2700s | **2** |

Hai dòng cuối **trùng nhau**. Đó là điểm mấu chốt: thay `windowSeconds` bằng `sessionSeconds`
không phải một phép đổi thang trung tính — nó **là** giả định khách đến đều, chỉ khác cách mặc áo.
Và chưa ai duyệt giả định đó.

Với một quyết định mua sắm, đây là chênh giữa mua **32 kênh** và mua **4**.

→ Hướng 1 cần **một quyết định thứ ba chưa ai hỏi: arrival profile**. `M8-OD-C` chỉ hỏi hai.

## 3. Đã làm

1. `SESSION_LENGTH` trong `tools/capacity-sim/capacity-model.mjs`: `answered: false`,
   `sessionSeconds: null`, `arrivalProfile: null`, `decisionId: "M8-OD-C"`,
   `sizedAgainst: "policy.windowSeconds"`, và `unsourcedSpecCandidateSeconds: 2700` — ghi lại
   **chỉ để nhận diện và từ chối**.
2. Gate `CAP-SESSION-06` trong `deploy/ci/scripts/capacity-selftest.mjs`.
3. `docs/capacity-model.md` §4b.
4. `OD-19` trong `decisions-log.md` được bổ sung phát hiện arrival profile.

**Không đổi phép tính.** `poolForProgramme`, `poolForDay`, `channelsForWindow` không bị sửa.

## 4. `CAP-SESSION-06` canh gì

- Độ dài phiên còn để mở và còn nêu đúng `M8-OD-C`.
- **Đo lại chính cái bẫy mỗi lần chạy**: nếu thay `2700s` không còn làm sizing sụp (`asSession × 4 <
  asWindow`) thì gate **đỏ** — model đã đổi hình và phải suy lại xem giả định đến-đều còn nấp trong
  đó không. Cái bẫy được đo, không phải được ghi trong comment.
- Đặt `sessionSeconds` mà `answered` vẫn `false` → đỏ.
- Đặt `sessionSeconds` mà thiếu `arrivalProfile` → đỏ, kèm đúng con số `16 → 2`.
- Đặt `sessionSeconds = 2700` → đỏ, vì đó là con số §14.1 mà chính spec gọi là giả định.
- Model phải còn sizing đúng như `sizedAgainst` khai — kiểm bằng cách **chạy thật**
  `poolForProgramme` rồi so, không tin lời khai.

## 5. Mutation proof — thang ba bậc

| Mutation | Bắt được |
| --- | --- |
| `sessionSeconds=2700`, `answered` vẫn `false` | *"a session length was set while the decision is still recorded as unanswered"* |
| thêm `answered=true`, chưa có `arrivalProfile` | *"was set with no arrivalProfile ... takes Golden Hour from 16 channels to 2"* |
| đủ cả ba, nhưng `sessionSeconds=2700` | *"the 45-minute figure from the §14.1 column header, which the spec itself calls an assumption"* |

Mỗi mutation bị chặn ở một lớp khác nhau, `exit=1` cả ba. Đã revert; selftest lại xanh.

## 6. Verification

| Gate | Kết quả |
| --- | --- |
| `capacity-selftest.mjs` | 6 check PASS; `CAP-SESSION-06 PASS_UNANSWERED` |
| **Bất biến hành vi** | `CAP-MODEL-01` vẫn `21` kênh; `CAP-SENS-02` vẫn `27` corner / `7..72`; `CAP-ALERT-04` vẫn peak `21` |
| `CAP-DRIFT-05` | vẫn `PASS_DECLARED_DISAGREEMENT` (W-0132 không bị ảnh hưởng) |
| `ci-config-selftest.mjs` | `CI_CONFIG_SELFTEST_PASS` |
| GitNexus (index refresh `50.852` node/`70.085` edge/`300` flow) | `poolForProgramme` LOW 3/2/0 process; `poolForDay` LOW 5/2/0 |
| Code production | `0 file` |

## 7. Residual gate

- `OWNER_DECISION_REQUIRED` — `M8-OD-C`: phiên dài bao lâu, cao điểm bao nhiêu đơn.
- `DESIGN_UNAPPROVED` — `OD-19`: **và giờ là ba câu, không phải hai**. Câu thứ ba là arrival
  profile. Không có nó thì hướng 1 under-size đơn mua 8 lần.
- `NOT_CALIBRATED` — chu kỳ cuộc gọi vẫn chờ `W-0008`.
- W-0134 **không** đóng `OD-19`. Nó chỉ làm cho việc đóng sai trở nên khó.
