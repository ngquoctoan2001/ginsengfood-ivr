# W-0288 — Chaos qua Docker service

Ngày 14/09/2026, baseline `main@701d48b`. **TESTS_PASS**, không phải ACCEPTED. Owner thực hiện: Codex theo yêu cầu tiếp tục từng task.

[Trước sửa](../W-0287/sdk-jobs-before-chaos-fix.log): SDK đúng đã restore/build, nhưng 7/8 test Chaos lỗi `Connection refused` tới loopback. Trong CI, Toxiproxy nằm trên Docker service khác container chạy test.

Thay hai địa chỉ cứng bằng `proxy.Hostname`; URL dùng UriBuilder để giữ đúng cú pháp host/port. Upstream vẫn `chaos-db:5432` trên network riêng, guard không đổi. Runtime không sửa. GitNexus upstream cho InitializeAsync và ConnectionString: LOW, 0 caller/0 process được index; phạm vi thực tế là fixture Chaos.

Kiểm chứng: [Linux/DinD 27](chaos-dind.log) **8/8 PASS**, từ 1/8 trước sửa; [Windows/Docker local](chaos-windows.log) **8/8 PASS**. Lệnh: `dotnet test tests/chaos/Ivr.ChaosTests.csproj --configuration Release` (Windows dùng `--no-restore`). NuGet audit tắt ở cả hai lượt. `dotnet format whitespace Ivr.sln --include tests/chaos/ChaosEnvironment.cs --verify-no-changes --no-restore` exit 0.

Hosted candidate NOT_RUN; real customer calls NO. Review gate false green là task tiếp theo; không suy fixture PASS thành toàn CI/release PASS.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `CHAOS-DB-02`, `CHAOS-DOWNSTREAM-01`, `CHAOS-DUPLICATE-06`, `CHAOS-RECOVERY-04`, `CHAOS-SIM-03`, `CHAOS-TERMINATE-07`, `CHAOS-TERMINATE-08`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.
