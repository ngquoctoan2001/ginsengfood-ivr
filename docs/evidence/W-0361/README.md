# W-0361 — Lô `L6` của kế hoạch khắc phục `25/09`: số điện thoại và PII

Ngày 25/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp tục l6"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Kế hoạch [`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`](../../../plan/ke-hoach-khac-phuc-m8-2026-09-25.md) xếp lô `L6`
là năm việc quanh số điện thoại khách (`K-37…K-41`). Từ phương án B (`W-0310`) Module 3 gửi thẳng số, IVR lưu nó ở
`ivr_confirmation_tasks.phone_e164`, và lời hứa với Module 3 là số chỉ được dùng ở một chỗ. Rà soát 25/09 tìm ra năm
kẽ hở của lời hứa đó: guard tên field không biết tên của số, hai record chỉ che số khi in chứ không che khi serialize,
một test "không cột nào giữ số" xanh mà không nhìn thấy cột giữ số, MOCK và lab lưu số dù không bao giờ đọc, và test
chặn số chưa đi qua cổng HTTP, kho analytics hay log lỗi.

Lô được viết trên một bản sao `git archive` của `main@d989c31`, ngoài cây chính, để cây chính luôn sạch cho phiên kia.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-37` | `PiiGuard` từ chối thêm hai tên field `phone_e164` và `msisdn` (so khớp sau khi bỏ mọi ký tự không phải chữ, số). Trước đây cả hai logger audit nhận một dòng có khoá `phone_e164`. Các dạng được phép đi thay số (`phone_masked`, `phone_ref`, `phone_validation_status`) vẫn qua, đã kiểm | `UT-PII-FIELD-01`, `UT-PII-FIELD-02` |
| `K-38` | `DialTokenResolutionRequest` và `TelephonyDispatchContext` gắn `[property: JsonIgnore]` cho `DirectPhoneE164`: `ToString` đã che số từ `W-0354`, còn JSON thì chưa. Một test phản chiếu tìm **mọi** record trong ba assembly Domain, Infrastructure, Worker có property chuỗi mang tên số (Phone, E164, Msisdn, trừ dạng Masked, Ref, Status), cài số vào, rồi đòi cả `ToString` lẫn JSON không trả lại số. `ScriptPreview` và `ScriptInputSnapshot` in dấu `[REDACTED_…]` thay vì tên khách, vùng giao và nguyên văn lời thoại, như các kiểu mang lời thoại khác | `UT-PHONE-TOSTRING-01`, `UT-PHONE-TOSTRING-02`, `UT-PHONE-TOSTRING-03` (thêm `PiiGuard.IsSafeText` và ca null của context), `UT-PHONE-TOSTRING-04`, `UT-PHONE-TOSTRING-05`, `UT-SCRIPT-TOSTRING-01` |
| `K-39` | `SEC-ROT-05` đọc mọi cột của mọi bảng trong model EF, không chỉ tên property của một entity. Mỗi cột có tên giống số phải nằm trong danh sách cho phép kèm lý do; `phone_e164` là ngoại lệ duy nhất (phương án B, ngoại lệ của `OD-V1-18`). Bản cũ lọc bốn chuỗi tên và `phone_e164` không khớp chuỗi nào, nên test xanh suốt từ ngày cột ra đời | `SEC-ROT-05` |
| `K-40` | Intake chỉ lưu `phone_e164` khi chạy `PRODUCTION_REAL`; MOCK, lab và sandbox lưu `NULL`. `ProductionDialTokenVault` là nơi duy nhất đọc cột này, nên ngoài production nó chỉ là một bản sao số khách phải canh và phải xoá. Không đổi contract; kiểm mẫu số vẫn chạy trên cái đã gửi. Ghi chú thêm ở `docs/compliance/data-inventory.md` và `PersonalDataInventory` | `IT-INTAKE-NUMBER-DB-05` (cùng một task chỉ có số, qua intake ở cả ba chế độ, mỗi chế độ có policy và kịch bản được duyệt riêng; chỉ production giữ số), `IT-INTAKE-NUMBER-DB-01`, `IT-INTAKE-NUMBER-DB-02`, `IT-INTAKE-NUMBER-DB-03` (đổi kỳ vọng MOCK thành `NULL`) |
| `K-41` | Test chặn số tìm theo số quốc gia (NSN), vì mọi cách viết đều chứa nó; chạy ETL analytics một lượt trước khi quét; đọc thêm SIM, kịch bản, trạng thái tích hợp, feature flag và lượt đọc call-job nội bộ của worker; chạy guard đầy đủ trên **mọi ô** text để bắt bản sao đã đổi định dạng; bộ ghi log của hai host test lưu cả exception. Test mới đi qua cổng HTTP intake: nhận, gửi lại, gửi lại khác thân, sai mẫu; không câu trả lời, header, dòng log hay dòng audit nào chứa số | `IT-PHONE-CONTAIN-01`, `IT-PHONE-CONTAIN-02` |
| Ghim hash | `ProviderPorts.cs` ở 2 nơi và `TaskIntakeService.cs` ở 4 nơi | `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`: `--self-test` và `--check-template` |

## Hành vi đổi, và phát hiện

- **MOCK, lab, sandbox:** một task chỉ có số nay không để lại số trong DB. Sandbox của Module 3 (`W-0282`) chạy MOCK,
  nên số Module 3 gửi thử không còn nằm trong DB sandbox. Production giữ nguyên hành vi.
- Một task nhận ở lab rồi được quay bởi production (đổi chế độ khi còn task treo) không có số và không có token giải
  được, nên vault production từ chối: fail-closed, không gọi nhầm ai. Lab chưa từng dùng cột này để quay.
- `SEC-ROT-05` trước đây tên là "không cột nào giữ ánh xạ token sang số" và chỉ đọc `ConfirmationTaskEntity`. Nay nó
  đọc cả kho analytics. Danh sách cho phép hiện có tám cột "giống số mà không phải số" (tham chiếu, dạng che, trạng thái,
  bộ đếm, cờ); thêm một cột giống số thứ chín là test đỏ và nêu tên cột.
- `ScriptInputItemSnapshot`, `SpeechItem`, `ShortDeliveryArea` vẫn in tên hàng và vùng giao khi `ToString`. Không thuộc
  `K-38`, ghi lại để lần rà sau cân nhắc.
- Câu "IVR **không bao giờ** thấy số" ở `data-inventory.md` mà `compliance-pack-selftest.mjs` đòi vẫn trái với phương
  án B; sửa câu đó thuộc `CB-10`, chờ Sếp trả lời B2.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi (`dotnet build Ivr.sln`, trên bản sao `git archive` của `d989c31` cộng lô này) |
| Đột biến | 7/7 bị bắt: bỏ `phonee164` khỏi guard; bỏ `JsonIgnore` của request; bỏ phần che số trong `PrintMembers` của context; bỏ phần che `ScriptPreview`; gỡ `phone_e164` khỏi danh sách của `SEC-ROT-05`; intake giữ số ở mọi chế độ; intake không giữ số ở chế độ nào |
| Test trên bản sao | unit `873/873`, contract `24/24`, chaos `8/8`, integration `429/430`. Ca đỏ duy nhất là `ApiBehaviorMatrixTests`, gọi `git ls-files` mà bản sao không có `.git`. Lượt đầu unit là `871/873`: bảng traceability chưa sinh lại, và `TtsBusyRetryTests` đo đồng hồ thật, `2999` ms quá trần `2500` ms khi ba dự án test chạy cùng lúc; sau khi sinh lại bảng và build lại, unit `873/873`, nhóm Governance `38/38` |
| Test trên cây chính sau khi land | `main@68e6e2af` cộng lô: unit `875/875`, contract `24/24`, integration `430/430`, chaos `8/8`, tổng `1337/1337`, build `0` cảnh báo. HEAD không đổi suốt lượt. Lượt trước đó trên cây chính đỏ đúng `ApiBehaviorMatrixTests`: cả `40/40` operation đạt, nhưng phiên kia commit `82188e6f` (phần K-31 còn lại) giữa lượt, nên chốt "source đổi trong lúc chạy" từ chối; lượt được tính là lượt build lại trên cây đã có cả hai phần |
| Test mới | 8 dòng traceability mới; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`898` → `906`; `898` đã gồm 2 test của phiên kia ở `82188e6f`) |
| Ghim hash | `ProviderPorts.cs` → `9c07954b…`, `TaskIntakeService.cs` → `041a8cde…`; hai validator đạt `--self-test` và `--check-template` |
| Gate sweep | `GATE_SWEEP_PASS 44/44 run, 26 skipped by manifest` (Git Bash, trên cây đã land); trong đó `dial-token-production-bundle-validator.mjs`, `opt-out-suppression-bundle-validator.mjs`, `compliance-pack-selftest.mjs`, `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 349 work item |
| Phạm vi | `gitnexus detect_changes` trước commit: 83 symbol trong 22 file đã theo dõi (3 file mới chưa vào index), `0` luồng bị ảnh hưởng, rủi ro `low`. Trước khi sửa, `gitnexus impact` báo `PiiGuard.EnsureSafeField` CRITICAL (44 chỗ phụ thuộc) và `TaskIntakeService.Accepted` HIGH (37); đã báo Toàn, và đã kiểm không response API hay khoá audit nào dùng hai tên field mới |

## Chưa làm

Test chặn số cho lối quay production thuộc `Q-28.2`. Mã hoá cột hoặc xoá số sau N ngày là `Q-32`, Sếp quyết.
Câu §1 của `secret-inventory` là `CB-10`.
