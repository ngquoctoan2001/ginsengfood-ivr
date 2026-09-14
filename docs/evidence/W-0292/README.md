# W-0292 — Hosted CI và dev–staging

Ngày 14/09/2026. **IN_PROGRESS**. Owner duyệt push/scan/publish/deploy dev–staging và 68 fingerprint đã rà ở W-0291.

Candidate hosted hiện tại `6bf954d2-ba8fd82e-b638b589-2c11df77-9c88eb42` (bỏ dấu nối để lấy SHA). Hai remote đã nhận candidate. [Pipeline 2845851460](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845851460) có **18/36 job PASS** tại checkpoint 14:20 +07; image và Kubernetes đã PASS đầy đủ. Toàn pipeline/publish/deploy **chưa hoàn tất**.

[Pipeline trước 2845815898](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845815898) chạy trên `b714694`, sau đó bị Auto-cancel khi push bản sửa bootstrap. Cấu hình hoặc local PASS không được tính là deployment proof.

Kiểm trước deploy phát hiện `alpine/helm:3.16.3` có entrypoint `helm`, nên `sh -c` bị từ chối; image cũng thiếu kubectl. Sửa riêng bootstrap dev/staging: clear entrypoint, cài kubectl và kiểm client. Container thật không có kubeconfig chạy được Helm 3.16.3/kubectl 1.31.5; cần kiểm tương thích version khi Platform xác nhận cluster. [Regression](bootstrap-regression.log) từ [script](bootstrap-regression.mjs): 4 mutation (thiếu entrypoint reset/kubectl ở mỗi môi trường) bị từ chối đúng lý do, cấu hình hợp lệ PASS. CD 5/5 và CI config PASS; GitNexus guard LOW, 1 file caller/0 process.

Ngày 14/09 owner chưa biết đích deploy. Tự kiểm [GitLab cluster](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/clusters): 0 Agent; trang CI có 2 biến API_DOCS_PUBLISH_NONPROD và IVR_W0061_PROTECTED_PROBE, không mở giá trị. Máy local `kubectl config get-contexts` rỗng, không file kubeconfig mặc định hoặc biến KUBECONFIG. **Chưa có đích cluster/credential được xác nhận**; chart còn cần secret ivr-database/ivr-app-secrets, PostgreSQL và quyền kéo registry. Không coi Kubernetes dùng tạm trong selftest là dev/staging đã triển khai.

Lab, production, real customer calls chưa được mở. M3/SIM/Platform/release gates giữ theo evidence thực tế.

## Candidate sau sửa bootstrap

Hai remote đã nhận `6bf954d2-ba8fd82e-b638b589-2c11df77-9c88eb42`. [Pipeline 2845851460](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845851460) đang chạy; GitLab tự huỷ lượt b714694 theo Auto-cancel redundant pipelines đã bật. Lượt trước đã có sweep 39/39, contract/E2E/review/observability PASS; không ghi là full pipeline PASS.

[Scan lịch sử candidate mới](exact-commit-gitleaks.log): 345 commit no findings. [Migration image local](migrate-scan.log): build tag ivr-migrate:w0292-6bf954d exit 0, Trivy HIGH/CRITICAL 0 finding; bổ sung phạm vi chưa có trong image-selftest (chỉ API/worker). Đây chưa phải digest registry hay hosted publish proof.

## Hosted checkpoint trước bản sửa privacy

- [Image job 16477133512](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133512): `IMAGE_SELFTEST_PASS`; build/non-root, live/no DB, Compose isolation, scan 2 image, SBOM và E2E 8 task đều PASS trong một lượt. Ca operator pause trả WINDOW_EXPIRED/hold-review đúng quyết định, không thiếu năng lực giả.
- [Kubernetes job 16477133513](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133513): `K8S_SELFTEST_PASS`, 7 marker PASS. Cụm thử trong DinD, không phải môi trường dev/staging đích.
- [Chaos job 16477133511](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133511): 8/8; [sweep job 16477133506](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133506): 39/39. Contract, E2E suite, review gate, observability contracts/rules/Helm và gate mirror cũng PASS.
- Local preflight phát hiện privacy sẽ chặn 121 dòng tài liệu cũ. [W-0293](../W-0293/README.md), commit `ff9c4b5`, sửa cách trình bày; full evidence scan/selftest PASS. [Scan commit đó](privacy-candidate-gitleaks.log): 347 commits scanned, no leaks. Cần push và lấy kết quả pipeline cho bản sửa.

[Kiểm hash đã rà](reviewed-hash-verification.log) từ [script chỉ đọc](verify-reviewed-hashes.mjs): 50/50 khớp nguồn bất biến (44 candidate + 6 portal), thử byte gốc/LF/CRLF. [Known-bad image control](known-bad-image-control.log): Trivy 0.58.1 trả đúng exit 42 và JSON có 1 HIGH/CRITICAL; không tính lỗi công cụ thành bằng chứng chặn vulnerability.
