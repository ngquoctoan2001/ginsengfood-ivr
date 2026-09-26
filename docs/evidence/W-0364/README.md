# W-0364 — Phần II của kế hoạch khắc phục `25/09`: `Q-22.1` (vùng tối buổi tối) và `Q-24.1` (công cụ S5)

Ngày 26/09/2026 · Claude, sau câu hỏi *"chuyển qua phần 2 hả"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Toàn chốt `Q-22` PA2 và `Q-24` PA3 ngày 25/09 theo đề xuất M8 (`plan/ke-hoach-khac-phuc-m8-2026-09-25.md` §5).
Ngày 26/09 Toàn chọn lô tiếp theo gồm `Q-22`, `Q-24` và `Q-28.1–2`, và chốt 14 quyết định còn lại theo khuyến
nghị. Lô được tách làm hai commit cho dễ duyệt: bản này (`Q-22.1`, `Q-24.1`), còn `Q-28` ở mã việc sau. `Q-22.2`
chưa làm: nó sửa hàm sweep hết hạn mà phiên khác đang sửa cho `K-54`, nên đợi `K-54` vào `main`.

Phân tích tác động báo phép kiểm giờ gọi ở intake ở mức HIGH (mọi task đi qua nó: endpoint intake và seed dev),
nên Toàn duyệt trước khi sửa. Lô làm trên bản sao tách riêng của `4918948d`.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `Q-22.1` | Phép kiểm giờ gọi ở intake nay đòi **mọi** attempt của policy (`T0 + offset`) nằm trong giờ gọi, thay vì chỉ `T0` như luật `B17` ngày 25/09. Offset đầu luôn bằng 0, nên biên buổi sáng không đổi. Ở buổi tối, đơn 24/7 có `T0` từ `21:00:30` (Giờ Vàng từ `21:05:30`) trước đây được nhận, nhưng attempt 2 rơi từ `21:08` trở đi và không bao giờ được quay; khách không nghe cuộc 1 thì Module 3 nhận `IVR_CONFIRMATION_WINDOW_EXPIRED` thay vì `IVR_NO_ANSWER_FINAL`. Nay đơn đó bị từ chối như đơn đêm: cùng quyết định `TASK_BLOCKED_OPERATIONAL`, cùng reason, không lưu gì, và Module 3 giữ đơn tới `08:00`. Hàm đổi tên thành `EveryAttemptFallsInsideCallingHours`; chỗ cần sửa lấy từ `gitnexus rename` chạy thử. Tài liệu: IR-06 §3.4.2 (mốc, khối `W-0304`, bảng biên), IR-07 (câu tóm tắt, dòng `M3-02`, dòng biên), đặc tả mã lỗi, bảng so sánh Giờ Vàng với 24/7. IR-06, `TaskIntakeService.cs` và `EligibilityRules.cs` bị ghim hash ở bốn validator và ba template; lô ghim lại cả bảy chỗ | `UT-INTAKE-EVENING-01` (biên từng giây), `UT-INTAKE-EVENING-02` (vùng tối bị từ chối, không lưu gì), `UT-INTAKE-WINDOW-SWEEP-01` (mọi phút trong ngày cộng các giây ở biên, cho cả hai chương trình); `UT-INTAKE-MORNING-01`, `UT-INTAKE-MORNING-02`, `UT-INTAKE-NIGHT-01` giữ nguyên |
| `Q-24.1` | Ba công cụ S5 (`run-vieneu-s5.py`, `run-vieneu-worker-s5.py`, `full-flow-s5/launcher.py`) chỉ tạo tài nguyên trong phần máy test cấp cho M8: tên container và compose project bắt đầu bằng `m8_`, cổng publish nằm trong `6800–6899` (launcher mặc định `6843` thay cho `58443`), thư mục kết quả nằm dưới `/home/ssv/m8`. Chạy trên S5 mà ra ngoài phần đó thì công cụ dừng **trước** khi tạo thư mục kết quả; `--local-lab` không bị ràng buộc vì không chạy trên máy chung. `cases.py` nhận tên project mới. Tài nguyên các lượt trước để lại không bị động tới (giữ chỉ-đọc, dọn theo nhãn sau); 13 URI mirror trong `MODELS.lock` không đổi, chờ kho S5 thật (`CB-15`). Gói offline đã dựng mang bản công cụ cũ, nên lượt sau phải dựng lại gói. README của lab ghi quy ước này | Test Python của lab (`deploy/lab/tests`): `test_s5_launcher.py` thêm lớp `M8Allocation` (4 test), `test_full_flow_s5.py` thêm 4 test; cả thư mục 58 test |

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của bản sửa trong bản sao, build lại nếu là C#, chạy các test được nêu, rồi ghi lại
byte gốc (không dùng `git checkout`). Chín phép đều đỏ đúng chỗ, hai lượt đối chứng đều xanh. `Q-22.1` ba phép:
quay về luật chỉ xét `T0` (đỏ ở các test buổi tối và bản quét, test buổi sáng vẫn xanh), chỉ xét attempt cuối
(biên buổi sáng hỏng, đỏ ở test buổi sáng và bản quét), bỏ hẳn phép kiểm. `Q-24.1` sáu phép: nhận mọi thư mục
kết quả, nhận cổng dưới `6800`, bỏ tiền tố `m8_`, launcher bỏ qua phép kiểm trên S5, cổng mặc định về `58443`,
`cases.py` còn đòi tên project cũ. Số liệu ở [mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build bộ unit trong bản sao, analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit trong bản sao | `893/893` |
| Bộ integration trong bản sao | `433/434`, 7 phút 14 giây. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git; trên `main` nó được chạy lại (dòng cuối bảng) |
| Traceability, `generate-test-traceability.mjs --check` | sinh lại, `923` dòng, `TEST_TRACEABILITY_CURRENT` |
| Validator ghim hash, `--self-test` | `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`, `d06-revalidation-evidence-validator.mjs`, `upstream-session-signoff-validator.mjs` đạt |
| `ci-config-selftest.mjs --self-test`, `docs-selftest.mjs` | đạt |
| Gate sweep trong bản sao | `GATE_SWEEP_PASS 45/45`, 26 gate bỏ qua theo manifest; gồm quét PII (`scan-pii.sh`) và bốn validator ghim hash nêu trên |
| Trên `main` sau khi đưa vào (`08db0c19` cộng lô này), 10:24–10:28 | build 0 cảnh báo; unit `893/893`; `IT-API-MATRIX-38` cùng các test integration về intake, chặn số điện thoại, giờ gọi cả ngày và compliance `93/93`; `generate-test-traceability.mjs --check`, bốn validator ghim hash, `ci-config-selftest.mjs`, `docs-selftest.mjs`, `gate-status.mjs`, `acceptance-batches.mjs --self-test` đạt; test Python của lab `58` đạt |

## Còn lại

- `Q-22.2`: task nhận ở giây cuối trước `21:08` mà không kịp claim bị tính là thiếu dung lượng; làm sau khi
  `K-54` vào `main`, vì cùng sửa hàm sweep hết hạn.
- `Q-28.1–2` ở mã việc sau.
- Module 3: IR-07 chưa được gửi lại (`CB-01`). Bản này đã có mốc `21:00:30`, nên gửi sau lô này thì Module 3
  dựng việc giữ đơn 24/7 (`CB-08`) theo mốc mới.
- `Q-24`: dọn tài nguyên cũ trên S5 theo nhãn chưa làm. Như kế hoạch ghi ở §5, ngoại lệ tạm trên máy test chưa
  được báo cho chief.
