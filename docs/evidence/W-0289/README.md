# W-0289 — Review gate nhận diện đúng lỗi

Ngày 14/09/2026, baseline `main@39536d4`. **TESTS_PASS**, không phải ACCEPTED. Codex thực hiện theo yêu cầu tiếp tục và commit từng task.

[Tái hiện trước sửa](before-fix.log): gate cũ in `REVIEW_GATE_SELFTEST_PASS` khi executable dotnet giả chỉ trả `155` và thông báo thiếu SDK. Các negative probe chỉ so sánh status khác 0, nên lỗi môi trường có thể bị tính thành bằng chứng gate biết từ chối dữ liệu sai.

Phạm vi: kiểm lỗi khởi chạy/signal; yêu cầu exit 1 kèm dấu hiệu đúng test order-state, PII, coverage thấp và MR traceability. Không thay runtime, rule scanner hay quyền CI. GitNexus run: LOW, 1 caller cùng file, 0 process; biến orderTransitionRejected: LOW, 0 caller.

[Regression Linux](environment-regression.log): **8/8 lỗi môi trường bị từ chối đúng lý do**, gồm SDK thiếu, compiler lỗi, SIGTERM, executable mất, PII tool lỗi, coverage tool lỗi, MR tool lỗi và output MR rỗng dù exit 0. Sau đó **4/4 probe bằng công cụ thật PASS**. Mỗi lượt đã kiểm tra fixture source được dọn sạch. Lệnh từ checkout Linux dùng riêng: `node docs/evidence/W-0289/environment-regression.mjs`; [script tái hiện](environment-regression.mjs) giữ NuGet audit tắt và chỉ thay PATH của subprocess.

`ci-config-selftest.mjs` PASS sau sửa, gồm helper census và manifest. Hosted NOT_RUN; security audit NOT_RUN; real customer calls NO.
