# W-0303 — Luồng quay số production và bảng traceability mà HEAD đang thiếu

Ngày 16/09/2026. **TESTS_PASS**. Commit `a2808ce`, baseline `2fb76fe`. `REAL_CUSTOMER_CALL_ALLOWED=NO` — không đổi.

PD-01.1 đến PD-01.5 của [kế hoạch luồng gọi production](../../../plan/ivr-orther/sip-production-dial-path-plan-2026-09-16.md). Hai type mới, ba file sửa, và một nhánh production trong composition root bên cạnh nhánh lab.

## Đã dựng

| Phần | Nội dung |
| --- | --- |
| PD-01.1 | `ProductionDialTokenVault` + bộ chuẩn hóa số Việt Nam |
| PD-01.2 | `SipTrunkOptions`, validator, enum định dạng số, hằng số chế độ DTMF |
| PD-01.3 | `AsteriskAriOptionsValidator` tách hai profile theo execution mode |
| PD-01.4 | Nhánh DI production trong `SchedulerCapacity.cs` |
| PD-01.5 | `AsteriskAriSimGateway` dựng `PJSIP/{trunk}/sip:{số}@{host}` |

## Ba quyết định thiết kế

**Vault production không tự làm protector.** Vault lab là protector của chính nó vì "bảo vệ" của nó là dấu vân tay không đảo ngược, tự sinh được. Bảo vệ production là khóa do Platform quản lý, nên vault **nhận** protector thay vì **là** một cái. Hệ quả: deployment còn giữ `UnavailableOpaqueValueProtector` sẽ fail closed ngay lần resolve đầu thay vì quay số một thứ nó không bảo vệ nổi.

**Ledger chạy trước protector.** Token hết hạn, sai task, replay attempt, hoặc quá trần đều bị từ chối **mà không được giải mã**, nên một cuộc gọi sẽ không xảy ra cũng không bao giờ làm số khách hàng hiện ra trong tiến trình. `UT-TRUNK-DIAL-01` đếm số lần protector bị gọi và khẳng định bằng **0**.

**`SipTrunkOptions` tách khỏi `AsteriskAriOptions`.** Options kia bị validator của chính nó ghim vào alias lab; gộp trunk vào đó buộc phải nới lỏng đúng cái profile có nhiệm vụ từ chối mọi đích ngoài `LAB-A`. Validator tách theo execution mode thay vì nới: nhánh lab **không đổi hành vi**, gồm cả việc báo lỗi định danh đúng một lần chứ không một lần mỗi trường (`UT-AST-CONFIG-07`); nhánh production bỏ đích ghim vì số tới theo từng cuộc.

Production đòi **cả hai** section bật. Nửa cấu hình rơi về `UnavailableSchedulerDispatchGateway` — cái từ chối quay số — thay vì một luồng gọi thiếu một nửa (`UT-TRUNK-DI-02`).

## Cố ý chưa làm

`AsteriskSchedulerDispatchGateway.IsReady` vẫn đòi `LAB_REAL_SIM` **và** `!RealCustomerCallAllowed`. Luồng production nối đủ nhưng **cổng vẫn đóng**. Mở nó thuộc SIP-04 và cần bằng chứng phê duyệt; kế hoạch PD-01 ghi rõ không gỡ `RealCustomerCallAllowed` trong đợt này.

**Hành vi này chưa được ghim bằng test** — dựng instance gateway đó cần tám dependency. Đây phải là việc đầu tiên phần còn lại của PD-01 xử lý, nếu không người đọc sau sẽ coi là lỗi.

## Sửa một chỗ hỏng trên `main`

Commit `1651e8f` (`W-0301`) đưa `docs/traceability-tests.md` vào trong khi `SipTrunkProductionDialTests.cs` còn untracked. HEAD vì thế mang **16 dòng `UT-TRUNK`** khai một file HEAD không có, và `FailGateTests.TheTraceabilityTableMatchesTheSuiteItClaimsToDescribe` sẽ đỏ trên một checkout sạch.

Xác minh: `git show 1651e8f -- docs/traceability-tests.md` thêm 16 dòng; `git cat-file -e 1651e8f:tests/Ivr.UnitTests/Telephony/SipTrunkProductionDialTests.cs` fail.

`a2808ce` đưa bộ test vào nên bảng khớp lại. Mọi dòng thêm trong diff traceability của commit đó đều thuộc thay đổi này.

**Nguyên nhân gốc, do phiên `ivr-9f` xác định:** `generate-test-traceability.mjs` đọc **working tree**, không đọc HEAD. Sinh lại file trong lúc phiên kia đang giữ test chưa commit sẽ gấp dòng của họ vào commit của mình. Sinh lại chỉ an toàn khi cây nó đọc trùng cây đang được commit.

## Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| `dotnet test Ivr.UnitTests` | **755/755**, 0 failed, 0 skipped |
| `gate-status` | PASS |
| `compliance-pack-selftest` | PASS |
| `docs-selftest` | PASS |
| `ci-config-selftest` | PASS |
| `review-gate-selftest` | PASS |
| `generate-test-traceability --check` | PASS, 695 |

**GitNexus impact, chạy trước từng lần sửa:** `IDialTokenResolver` báo **CRITICAL** (214 symbol, 50 trực tiếp) — mức đó áp cho việc **sửa interface**, còn đây là thêm implementation thứ tư bên cạnh ba cái có sẵn, không sửa interface. `AsteriskAriOptionsValidator` **LOW** (1 trực tiếp), `AddIvrScheduling` **LOW** (0), `AsteriskAriSimGateway` **MEDIUM** (3 trực tiếp, 0 process).

`detect_changes`: ba process ảnh hưởng, cả ba là `DialAsync` — đúng method đã sửa. Toàn bộ test lab telephony vẫn xanh.

## Phối hợp hai phiên

Commit stage **đúng tám path**, không `git add -A`. Khoảng 30 file của phiên `ivr-9f` không bị chạm. Work ID `W-0303` do `ivr-9f` cấp; họ giữ `W-0302` và sẽ đặt `NEXT_WORK_ID = W-0304`, nên trường điều khiển không bị sửa ở đây.

Đính chính một đề nghị sai của phiên này: `docs/release/gate-status.yaml` và tracker **không** để lại cho ai commit sau cùng được — `deploy/ci/scripts/gate-status.mjs:27` sinh yaml từ tracker và fail khi lệch, nên hai file phải đi cùng một commit với dòng đã đổi. Quy tắc đó chỉ áp dụng cho `docs/traceability-tests.md`.
