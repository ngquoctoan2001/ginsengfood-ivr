# W-0288 — Chaos qua Docker service

Ngày 14/09/2026, baseline `main@701d48b`. **TESTS_PASS**, không phải ACCEPTED. Owner thực hiện: Codex theo yêu cầu tiếp tục từng task.

[Trước sửa](../W-0287/sdk-jobs-before-chaos-fix.log): SDK đúng đã restore/build, nhưng 7/8 test Chaos lỗi `Connection refused` tới loopback. Trong CI, Toxiproxy nằm trên Docker service khác container chạy test.

Thay hai địa chỉ cứng bằng `proxy.Hostname`; URL dùng UriBuilder để giữ đúng cú pháp host/port. Upstream vẫn `chaos-db:5432` trên network riêng, guard không đổi. Runtime không sửa. GitNexus upstream cho InitializeAsync và ConnectionString: LOW, 0 caller/0 process được index; phạm vi thực tế là fixture Chaos.

Kiểm chứng: [Linux/DinD 27](chaos-dind.log) **8/8 PASS**, từ 1/8 trước sửa; [Windows/Docker local](chaos-windows.log) **8/8 PASS**. Lệnh: `dotnet test tests/chaos/Ivr.ChaosTests.csproj --configuration Release` (Windows dùng `--no-restore`). NuGet audit tắt ở cả hai lượt. `dotnet format whitespace Ivr.sln --include tests/chaos/ChaosEnvironment.cs --verify-no-changes --no-restore` exit 0.

Hosted candidate NOT_RUN; real customer calls NO. Review gate false green là task tiếp theo; không suy fixture PASS thành toàn CI/release PASS.
