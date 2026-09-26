# W-0368 — Phần II của kế hoạch khắc phục `25/09`: `Q-22.2` (đơn không kịp quay vì hết giờ gọi)

Ngày 26/09/2026 · Claude, lô Toàn chọn sau câu *"chuyển qua phần 2 hả"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

`Q-22` (Toàn chốt PA2 ngày 25/09) có hai việc. `Q-22.1` (`W-0364`) cho intake từ chối đơn có lần gọi rơi ngoài giờ gọi.
`Q-22.2` là phần còn lại: một job đã xếp hàng mà hết cửa sổ khi chưa quay lần nào bị sweep hết hạn tính là thiếu dung
lượng. Sweep mở một sự cố dung lượng `OPEN` và cộng `ivr_missed_deadline_total` với lý do
`NO_DISPATCH_BEFORE_DEADLINE`, đúng con số dùng để định số SIM (M8-OD-A). Sau `Q-22.1` ca này vẫn còn khi đơn tới muộn
so với `T0` của nó: `T0` 21:00, tới lúc 21:07:50, hay tới sau 21:08. Khi đó giờ gọi đóng trước khi có kênh nào kịp nhận
đơn, nên nó không phải bằng chứng thiếu kênh. Kế hoạch ghi: phân loại lại chỉ ở sự cố và số đo, không đổi kết quả gửi
Module 3.

`Q-22.2` đợi `K-54` của phiên khác (`W-0365`) vào `main`, vì cả hai sửa cùng hàm sweep. Ngưỡng do Toàn chốt ngày 26/09:
dưới thời lượng một cuộc gọi (`ExpectedCallDurationSeconds`, mặc định 60 giây). Phân tích tác động: hàm dựng của
`PostgresSchedulerStore` ở mức CRITICAL (41 chỗ gọi, phần lớn là test), Toàn duyệt trước khi sửa; hàm sweep và
`CallingWindow` ở mức MEDIUM. Lô làm trên bản sao tách riêng của `8305ad5e`. Trong lúc làm, phiên kia commit `W-0367`
(`K-57`); hai lô không chung file mã nào, nên lô được đưa nguyên lên `main`, còn bảng traceability, tracker và
gate-status được sinh lại ở đó.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| Giờ gọi mở bao lâu | `CallingWindow.OpenTimeBetween(from, to)`: phần của một khoảng thời gian mà giờ gọi mở, đi từng phút (giờ gọi đóng mở theo phút địa phương). Giờ gọi tắt thì mở suốt; khoảng rỗng hay ngược thì bằng 0 | `UT-SCH-WINDOW-10` |
| Phân loại lại trong sweep | Một job đã xếp hàng, chưa quay lần nào, mà trong khoảng từ lúc tới (hoặc `T0`, lấy mốc muộn hơn) tới hạn cửa sổ giờ gọi thật sự đóng và chỉ còn mở chưa tới một cuộc gọi, được coi là hết giờ gọi. Sweep không mở sự cố dung lượng cho nó, để trống `capacity_incident_id`, ghi `calling_hours_ran_out` vào dòng audit, và đếm số đo với lý do `CALLING_HOURS_CLOSED_BEFORE_DISPATCH`. Trạng thái job, kết quả và callback gửi Module 3 giữ nguyên (`IVR_CAPACITY_EXCEPTION`, `NO_DISPATCH_BEFORE_DEADLINE`). Job bị eligibility giữ vì thiếu dung lượng vẫn dùng sự cố của nó. Một đơn tới muộn trong lúc giờ gọi vẫn mở suốt cũng vẫn là thiếu dung lượng: với đơn đó, cái hết là cửa sổ chứ không phải giờ gọi | `IT-SCH-HOURS-RANOUT-01` (59 giây thì hết giờ, 60 giây thì không; một đơn trưa tới muộn vẫn là thiếu dung lượng); `IT-SCH-CAPACITY-HELD-01` giữ nguyên |
| Store của scheduler được cho giờ gọi | Giờ gọi và thời lượng cuộc gọi là hai tham số tuỳ chọn của hàm dựng, nên mọi store dựng tay giữ hành vi cũ. Store do container dựng (`AddIvrScheduling` đăng ký theo kiểu, và container điền tham số tuỳ chọn khi dịch vụ đã có) nhận cả hai; thuộc tính `ClassifiesByCallingHours` cho thấy điều đó | `UT-SCH-HOURS-RANOUT-02` |
| Tài liệu | `docs/slo.md` §9 ghi lý do mới của số đo và việc luật cảnh báo hiện vẫn cộng mọi lý do (lọc theo lý do là `K-61`) | — |

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của lô trong bản sao, build lại project chứa test được nêu, chạy test, rồi ghi lại byte gốc
(không dùng `git checkout`). Bảy phép đều đỏ đúng chỗ, hai lượt đối chứng đều xanh:

- sweep không bao giờ hỏi giờ gọi;
- còn đúng một cuộc gọi giờ gọi (60 giây) cũng bị tính là hết giờ;
- đơn tới muộn trong lúc giờ gọi vẫn mở bị tính là hết giờ (bỏ điều kiện giờ gọi phải thật sự đóng trong khoảng đó);
- job hết giờ gọi vẫn mở sự cố dung lượng;
- số đo giữ lý do thiếu dung lượng;
- `OpenTimeBetween` đếm cả những phút giờ gọi đóng;
- container dựng store qua một factory bỏ hai tham số tuỳ chọn.

Năm phép đầu làm đỏ `IT-SCH-HOURS-RANOUT-01` trong khi `IT-SCH-CAPACITY-HELD-01` vẫn xanh. Phép thứ sáu làm đỏ
`UT-SCH-WINDOW-10` (`UT-SCH-WINDOW-09` xanh), phép thứ bảy làm đỏ `UT-SCH-HOURS-RANOUT-02` (`UT-SCH-WINDOW-10` xanh).
Số liệu ở [mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build cả solution trong bản sao (`8305ad5e` cộng lô này), analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit, bản sao | `921/921` |
| Bộ integration, bản sao | `447/448`, 6 phút 14 giây. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git; trên `main` nó được chạy lại (dòng cuối bảng) |
| Gate sweep, bản sao | `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest |
| Quét gitleaks mô phỏng (luật generic-api-key, entropy từ 3,5) trên các dòng lô thêm | 0 dòng đáng ngờ; lô không thêm migration hay khoá thử nào |
| Trên `main` sau khi đưa vào (`2ec04cbe` cộng lô này), 11:54–12:21 | build 0 cảnh báo; unit `924/924`; `GATE_SWEEP_PASS 46/46`, 26 gate bỏ qua theo manifest, gồm `generate-test-traceability.mjs` (sinh lại, `954` dòng), `gate-status.mjs` và quét PII hồ sơ. Integration chạy đủ bộ ba lượt: lượt đầu `454/455`, lượt hai và lượt ba `455/455`, tức là cả `IT-API-MATRIX-38` |
| Test đỏ ở lượt đầu | `IT-SCH-HOURS-RANOUT-01`, đỏ sau 24 ms. Khi xanh nó mất khoảng 1 giây, như các test cùng lớp chỉ dựng lại DB rồi quét một lần, nên 24 ms là lúc còn ở `fixture.ResetAsync()` (xoá và dựng lại DB dùng chung của nhóm test), trước khi dòng nào của lô chạy. Đầu ra lượt đó bị lọc nên mất thông báo lỗi; lượt ba có ghi `trx` và xanh, chạy riêng test cũng xanh. Test chạy ngay trước nó không để lại kết nối hay việc nền nào. Kho từng có một lỗi chập chờn cùng chỗ (`W-0040` §6, đã khép); lần này chưa bắt được nguyên nhân |

## Còn lại

- `K-61` (phiên khác): luật `IvrConfirmationDeadlineMissed` lọc theo lý do, để đơn hết giờ gọi không bật cảnh báo thiếu
  dung lượng.
- Trạng thái job vẫn là `CAPACITY_MISSED` / `CLOSED_CAPACITY` như mọi job hết hạn chưa quay: kế hoạch chỉ đòi sửa sự cố
  và số đo. Ai đọc trạng thái job thấy nó như trước; phân biệt nằm ở dòng audit (`calling_hours_ran_out`).
- Lý do mới chỉ là nhãn của số đo, không vào DB hay hợp đồng, nên `specs/ui/enum-labels.vi.json` không cần nhãn cho nó:
  nhóm `shortageReason` ở đó là lý do của sự cố dung lượng, mà đơn hết giờ gọi không mở sự cố nào.
