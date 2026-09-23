# W-0298 — Chặn task mà mọi lần gọi rơi ngoài khung giờ

Ngày 16/09/2026. Baseline `37606bc`, commit `92091a1`. `REAL_CUSTOMER_CALL_ALLOWED=NO`, không đổi
wire, không phát hành contract.

Nguồn: bản giao việc 16/09 mục `C15` — phát hiện đúng và là mục duy nhất trong 30 mục đang gây thiệt
hại tiền thật.

## Triệu chứng

Một đơn 24/7 COD đặt lúc `23:00` mang cửa sổ xác nhận `900s`. Khung giờ gọi đóng lúc `21:08` và mở
lại `08:00`, nên cả hai mốc `+0s` và `+450s` đều rơi vào giờ không được phép gọi.

```text
23:00        task vào, cửa sổ xác nhận bắt đầu
23:00–23:15  gate gọi ĐÓNG suốt → 0 cuộc gọi
23:15        sweep đóng cửa sổ → IVR_CONFIRMATION_WINDOW_EXPIRED gửi về Module 3
```

Khách **không nhận cuộc nào**, nhưng Sales nhận một mã đọc ra là *"khách không xác nhận kịp"*. Không
phân biệt được với khách phớt lờ cuộc gọi thật.

## Phương án đầu **sai** — ghi lại vì lý do đáng nhớ

Kế hoạch buổi sáng chọn *"IVR dời `T0`/cửa sổ tới giờ mở"*. Đọc code thì không làm được:

| Sự thật trên code | Hệ quả |
| --- | --- |
| `TaskIntakeService.cs:146-149` gọi `ConfirmationWindow.Create(source.Confirmation_window_started_at, source.Confirmation_window_expires_at)` | `T0` và cửa sổ **do Module 3 gửi trên wire**, IVR không tự tính |
| `W-0246`: `dial_token_expires_at` **==** window end, ghim ở 3 tầng (intake, persistence, dispatch) | Dời cửa sổ ⇒ token M3 đã cấp rơi **ra ngoài** cửa sổ mới |
| `OD-V1-05`: Module 3 là bên cấp `dial_token`; chưa có resolver production | IVR **không cấp lại token được** |
| `TaskIntakeService.cs:339-341` từ chối nếu `Confirmation_window_started_at > now` | Cửa sổ phải **đã bắt đầu** khi task tới — M3 gửi ngay tại `T0` |

Dời cửa sổ chín tiếng sang sáng hôm sau sẽ làm dial token chết trước khi quay số. Phương án đó không
khó — nó **sai**.

## Đã làm

Intake nhìn trước: nếu **mọi** lần gọi theo policy đều rơi ngoài giờ được phép gọi thì trả
`TASK_BLOCKED_OPERATIONAL` kèm `blocked_reasons` mới
`CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW`, **ngay lúc Module 3 còn giữ đơn**.

Guard đặt **sau** cổng contact có chủ đích: token hỏng là thứ Module 3 phải sửa, khung giờ đóng thì
không — lỗi sửa được phải hiện ra trước.

| File | Thay đổi |
| --- | --- |
| `src/Ivr.Domain/Policies/EligibilityRules.cs` | Thêm hằng `CallingWindowClosedForWholeConfirmationWindow` |
| `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs` | `CallingWindow` vào ctor; guard mới; `AnyAttemptFallsInsideCallingHours(window, policy)` |
| `tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs` | 3 test mới; `CreateContext(now)` nhận đồng hồ |
| `tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs` | 4 call site nhận `CallingWindow` bật thật |
| `docs/traceability-tests.md` | Sinh lại bằng `generate-test-traceability.mjs` |

## Không đổi wire

| Mặt cắt | Đổi không |
| --- | --- |
| `decision` — enum đóng | Không — `TASK_BLOCKED_OPERATIONAL` đã có sẵn trong OAS `:1365` |
| `blocked_reasons` — `array of string`, không phải enum | Không — thêm một chuỗi là additive |
| Schema / version OAS | Không — lượt này không phát hành contract |

## Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **1063/1063** pass, 0 failed, 0 skipped, exit 0 (baseline 1060 + 3 mới) |
| `gitnexus_impact` trên `CallingWindow` | **LOW**, 6 điểm chạm, toàn test |
| `gitnexus_detect_changes` | phạm vi code đúng 4 file dự kiến |
| `generate-test-traceability --check` | `TEST_TRACEABILITY_CURRENT=671` |
| `docs-selftest`, `gate-status` | PASS |

| Test | Kịch bản | Kỳ vọng |
| --- | --- | --- |
| `UT-INTAKE-NIGHT-01` | 24/7 COD, `T0` `23:00` giờ VN, hai mốc `+0s`/`+450s` đều ngoài giờ | `TASK_BLOCKED_OPERATIONAL` + reason mới, `ivr_call_job_id` null |
| `UT-INTAKE-NIGHT-02` | Cùng task, `T0` `13:00` giờ VN | Vẫn `TASK_ACCEPTED_DRY_RUN_ONLY` — guard không bắt nhầm |
| `UT-INTAKE-NIGHT-03` | `T0` `20:55` giờ VN — mốc 2 rơi `21:02:30`, còn trong khung nhờ `W-0220` dời sang `21:08` | Chấp nhận — giữ đúng một lần gọi được là đủ |

Test dùng `CallingWindow` **bật thật** thay vì tắt: cả hai fixture sẵn có đều ở
`2026-08-13T06:00:00Z` = `13:00` giờ VN, trong khung `08:00–21:08`, nên các ca cũ vẫn đúng nghĩa.

## Giới hạn còn lại

1. **e2e và sandbox không phủ guard này.** `docker-compose.e2e.yml:39-40` và
   `docker-compose.sandbox.yml:93-94` đều đặt `CallingWindow 0..1440`, nên guard nằm im ở đó. Phủ
   bằng unit test, không phủ bằng chạy ngoài. Đổi cấu hình e2e sẽ phá `W-0214`.
2. **Chưa giải quyết đơn đêm.** Đơn vẫn không được gọi; khác biệt là Module 3 biết ngay và biết đúng
   lý do. Giữ đơn tới sáng hay cấp cửa sổ khác là quyết định của Module 3, vì cửa sổ và token đều tới
   trên wire đã cấp sẵn.
3. **`IR-06 §3.4.2` chưa ghi** hành vi này cho Module 3 — chuyển sang lô tài liệu.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0298 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Chặn task mà
mọi lần gọi rơi ngoài khung giờ. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: IR-06 nay đã ghi hành vi này (bổ sung W-0304). Mọi giới hạn
trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442`
(1200/1200 test, sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới.
REAL_CUSTOMER_CALL_ALLOWED=NO.
