# PROMPT P10-3 — Capacity, Cost & SIM Sizing Model

## 0. Meta
| | |
| --- | --- |
| **ID** | `P10-3` · **Phase** 10 — Compliance & Maturity |
| **Work ID** | `W-0054` (canonical tracker §5) |
| **Prereq** | `P6-2`, `P5-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| **Stack** | modeling · .NET/analysis |

## 1. ROLE
Bạn là **Senior SRE / Capacity Planner**. Bạn xây mô hình từ **1 SIM thật ở lab** tới target **32 eSIM channels**, dựa throughput/answer-duration/provider limits thực đo; không dùng pilot 12 làm baseline mặc định.

## 2. CONTEXT
Số SIM là ràng buộc vật lý + chi phí lớn. Đặt sai → hoặc không kịp gọi trong window (miss deadline, đơn expire oan), hoặc lãng phí SIM. Cần mô hình tính từ: lượng order COD/ngày, phân bố theo program (Golden Hour vs 24/7), window/spacing (D-10), thời lượng call, cooldown (DT-04). Feed số thật cho procurement (blockers plan §A).

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/03-scheduler-attempt-policy.md` (D-10 window/spacing), `specs/testing/06-performance-test-plan.md`
- `plan/ivr-orther/decisions-log.md` §D-10 · §DT-04 (SIM pool, cooldown 5s, one-call) · `plan/ivr-orther/production-blockers-plan.md` §A

## 4. DECISIONS & CONSTRAINTS
- **Ràng buộc:** one-channel-one-active-call; cooldown/policy là versioned config. Candidate GH 300/[0,150] và 24/7 900/[0,450] chỉ dùng scenario MOCK/LAB; mô hình phải nhận policy khác và không hard-code.
- **Golden Hour peak:** tải dồn trong window ngắn (5 phút) → SIM cần cho peak, không phải trung bình.
- **Model input:** orders COD/ngày, % IVR-eligible, phân bố giờ, avg call duration, success/no-answer rate (→ attempt 2), SLA on-time dispatch.
- **Output:** SIM pool tối thiểu cho pilot & launch (khoảng tin cậy), cost/tháng, scaling policy, capacity alert ngưỡng (nối P6-2).
- Con số là **model** → hiệu chỉnh với perf test (P5-3) + thực địa (P8).

## 5. INPUTS / DEPENDENCIES
- Perf test throughput (P5-3); business forecast order volume (từ Owner/Sales); DT-04 constants.

## 6. BUILD STEPS
1. **Model dung lượng**: công thức/simulation SIM cần = f(peak arrival trong window, call duration, cooldown, attempts, one-call). Golden Hour peak là ràng buộc chính.
2. **Sensitivity**: chạy với dải volume/duration/no-answer-rate → khoảng SIM (pilot vs launch) với mức tin cậy.
3. **Cost model**: SIM/tháng + gateway + infra → cost per confirmed order.
4. **Scaling policy**: khi nào thêm SIM; HPA worker ceiling (nối P7-2); capacity alert ngưỡng (P6-2).
5. **Calibrate**: đối chiếu perf test (P5-3); để lại hook hiệu chỉnh với số thật (P8).
6. Report → feed procurement (blockers §A).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/capacity-model.md` | Mô hình + công thức/simulation + kết quả |
| `docs/cost-model.md` | Chi phí + cost/confirmed-order |
| `tools/capacity-sim/**` (nếu code) | Simulation script |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CAP-MODEL-01` | verification | model tính SIM cho Golden Hour peak (không phải avg); ràng buộc one-call/cooldown đúng. |
| `CAP-SENS-02` | verification | sensitivity cho dải volume → khoảng SIM pilot/launch có căn cứ (thay giả định DT-04). |
| `CAP-CALIB-03` | verification | đối chiếu perf test (P5-3): model khớp throughput đo được (±sai số). |
| `CAP-ALERT-04` | integration | capacity alert ngưỡng khớp model (P6-2). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] peak (không avg); [ ] ràng buộc D-10/DT-04 đúng; [ ] khoảng tin cậy; [ ] calibrate với perf test; [ ] feed procurement.
**Reviewer:** giả định volume từ business; cost hợp lý; scaling policy khả thi.

## 10. EVIDENCE EXPECTED
Capacity model + kết quả SIM pilot/launch, cost model, calibration vs perf test, alert threshold mapping.

## 11. FORBIDDEN
- ❌ Tính theo trung bình bỏ qua Golden Hour peak. ❌ Bỏ ràng buộc one-call/cooldown. ❌ Con số không căn cứ (giữ giả định DT-04 mà không model).

## 12. DEFINITION OF DONE
- [ ] Capacity + cost model + scaling policy + calibrate; 4 verification §8 pass; feed procurement; evidence §10 đủ.
