# W-0358 — Lô `L2` của kế hoạch khắc phục `25/09`: tài liệu gửi Module 3, sổ quyết định, guard biên buổi sáng

Ngày 25/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"chốt theo khuyến nghị Q-01, Q-02, Q-03, Q-13 …
spawn ra nhiều sub agents để tiến hành triển khai khắc phục l0, l1, l2"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch [`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) xếp lô `L2`
là các đính chính tài liệu gửi Module 3 phải xong **trước khi** gửi lại `IR-07` cho anh Mạnh, cộng việc sửa sổ quyết
định theo chốt chief `25/09`. Toàn chốt bốn quyết định thuộc Module 8 ngày 25/09: `Q-01` (phát hành lại `IR-07` bằng
đính chính cuối phiếu + đánh dấu tại chỗ), `Q-02` (đính chính lời hứa VieNeu đọc lúc gọi theo cách trung tính),
`Q-03` (vô hiệu bảng map `v0` bằng các dòng chắc chắn trong lúc chờ nguyên văn `v1`), `Q-13` (guard giờ gọi xét
thời điểm mở cửa sổ `T0`). Hai phiên chia lô qua tin nhắn: phiên kia `L1` (`W-0356`) và `L3` (`W-0357`), phiên này
`L2` và việc push.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `Q-13` (`B17`) | Guard intake nay từ chối task khi `T0` (`confirmation_window_started_at`) nằm ngoài giờ gọi, thay vì khi "không attempt nào rơi vào giờ gọi". Lý do: scheduler **gọi bù** attempt quá hạn ngay khi mở giờ, nên guard cũ vừa từ chối oan đơn 24/7 `T0` 07:45:01–07:52:29 (Giờ Vàng 07:55:01–07:57:29), vừa để lọt ca bị gọi hai cuộc sát nhau lúc 08:00 (24/7 07:52:30–07:59:59, Giờ Vàng 07:57:30–07:59:59). Decision và reason trên dây giữ nguyên; OpenAPI không đổi. Đổi tên hàm bằng `gitnexus rename`; chú thích sai "scheduler bỏ attempt ngoài giờ" được viết lại | `UT-INTAKE-MORNING-01`, `UT-INTAKE-MORNING-02`, `UT-INTAKE-EVENING-01`, `UT-INTAKE-WINDOW-SWEEP-01` |
| `IR-07` | Đính chính `25/09` bổ sung: hai reason của `200 TASK_BLOCKED_OPERATIONAL` (không reason nào retry được); dòng đổi hành vi biên sáng; phương án B phụ thuộc Sếp (mục B2); lời hứa VieNeu đọc lúc gọi rút theo luật quá độ 24/09; `A-11` thêm `draft.33`; `E-8` giới hạn tuổi phát lại; chuỗi `perm` `IVR_SCRIPT_*` là quyền được kiểm thật; câu hỏi mới `M3-31` (nghĩa `total_amount`) và `M3-32` (lọc/phân trang capacity-incidents). Đánh dấu "RÚT" tại chỗ ở `M3-13`, `M3-30`; đầu phiếu ghi `draft.33` và trạng thái gửi đúng sự thật (anh Mạnh báo chưa nhận bản 17/09) | Tài liệu |
| `IR-06` | Khối vô hiệu bảng map `v0` ở §4.3 (`C22`, chỉ các dòng chief đã viết rõ); khối biên buổi sáng (`B17`); điều kiện mới của guard; §3.9 thêm dòng `200 TASK_BLOCKED_OPERATIONAL`; `C7` (nhãn action của `IVR_NO_ANSWER_FINAL` Module 3 không làm theo); `C14`, `C21`, `C2`; `total_amount`; khối §4A.7 về chuỗi `perm`; đồng bộ trạng thái sổ quyết định; `pronunciation_hints` | `contract-freeze-verifier.mjs`, 4 validator ghim IR-06 |
| `06-error-codes` | Điều kiện guard mới; dòng bảo vệ token; dòng `total_amount` số lẻ; `409` chỉ còn cho `call_restriction` ở intake; số mã ổn định sửa 18 → 16 | `ci-config-selftest.mjs` |
| Sổ quyết định | Theo chốt chief 25/09: 11 dòng; nay 12 dòng mở / 29. Đồng bộ `04-sim-adapter-contract`, `pia`, `release-compliance-checklist`, `05-pii-policy` | `gate-status.mjs` |
| Fixture | Ca âm `NEG-SCHEMA-AMOUNT-01` (`total_amount = 210636.8` bị schema từ chối) | `validate-openapi.mjs` |
| Ghim hash | IR-06 ở 7 nơi, `TaskIntakeService.cs` ở 4 nơi, `EligibilityRules.cs` ở 2 nơi (chỉ đổi chú thích) | `d06-revalidation-evidence-validator.mjs`, `upstream-session-signoff-validator.mjs`, `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`: `--self-test` + `--check-template` |

## Làm khác kế hoạch, và vì sao

- Câu "gửi dồn lúc 08:00 sẽ nhận `IVR_CAPACITY_EXCEPTION` ở eligibility" sai với code: eligibility chỉ **giữ** job
  (`CAPACITY_HELD`) và không phát kết quả nào; sweep hết hạn lại chỉ quét job `eligible`. Tài liệu ghi đúng hiện
  trạng; lỗ hổng tách thành việc riêng `K-52` trong kế hoạch.
- `DIAL_TOKEN_PROTECTION_UNAVAILABLE` chỉ xảy ra ở production (lab có bộ mã hoá riêng), và hôm nay production còn giữ
  mọi task ở `TASK_HELD_ADMIN_REVIEW` trước bước đó.
- Template `W-0178` không được ghim lại: nó nằm trong `attested-sha256.txt` của chính hồ sơ đó, là hồ sơ đã đóng.
- `OD-V1-20` không bị thu hồi; cái bị thu hồi ngày 16/09 là dòng approval do W0195 seed. Tài liệu ghi đúng như vậy.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi (`dotnet build Ivr.sln`) |
| Test | unit `832/832`, contract `24/24`, integration `412/412`, chaos `8/8`. Lượt đầu unit `831/832`: bảng traceability chưa có 4 TestId mới; sinh lại bảng bằng `generate-test-traceability.mjs` (`871` → `875` dòng) rồi chạy lại toàn bộ unit |
| Test biên mới | 4 test ở `tests/Ivr.UnitTests/Intake/IntakeMorningBoundaryTests.cs`; quét `1440` phút + mốc giây ở biên cho cả hai chương trình (`2.888` lượt gọi intake); 9 ca buổi sáng và các phút 07:53–07:59 (24/7), 07:58–07:59 (Giờ Vàng) đỏ nếu chạy trên guard cũ |
| Ghim hash | Self-test đạt: `d06-revalidation-evidence-validator.mjs` (`W0178`), `upstream-session-signoff-validator.mjs` (`W0181`), `dial-token-production-bundle-validator.mjs` (`W0183`), `opt-out-suppression-bundle-validator.mjs` (`W0187`); `--check-template` của W-0181, W-0183, W-0187 đạt; `contract-freeze-verifier`, `ci-config-selftest`, `validate-openapi` (17 ca schema âm bị từ chối) đạt |
| Gate sweep | `GATE_SWEEP_PASS 44/44 run, 26 skipped by manifest`, exit `0` (`node deploy/ci/scripts/gate-sweep.mjs` từ Git Bash, 13:41 ngày 25/09, sau khi build và test xong) |
| Phạm vi | `gitnexus detect_changes` trước commit; guard B17 có impact HIGH (31 symbol), không test hiện có nào đổi kết quả |

## Chưa làm

`C22` chờ nguyên văn bảng `v1` từ chief; `C23` sẽ cụ thể hoá khi Tech Lead chốt phạm vi đọc (`Q-29`); gửi lại `IR-07`
cho anh Mạnh là việc của Toàn sau lô này.
