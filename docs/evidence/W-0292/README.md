# W-0292 — Hosted CI và dev–staging

Ngày 14/09/2026. **BLOCKED_EXTERNAL — thiếu cluster dev/staging**. Owner duyệt push/scan/publish/deploy dev–staging và 68 fingerprint đã rà ở W-0291. Phần kiểm chứng/publish hoàn tất trên candidate `179a5eb`; không tuyên bố đã deploy.

**Kết quả hiện hành:** [pipeline 2846110576](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2846110576), candidate `179a5eb`, UI **Blocked**. [Inventory](hosted-179a5eb-summary.json): **31 PASS, 1 Failed, 2 Skipped, 2 Manual** trên 36 job cấu hình (không cộng lượt retry và job Pages do GitLab sinh). Dev fail vì không có kết nối cluster; staging skipped. Pipeline không phải PASS.

| Phạm vi | Hosted proof tại 179a5eb |
| --- | --- |
| Build/lint/full suite | [Build/test](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951078): **1.018/1.018**, coverage **89,32%**; lint/analyzer 0 warning/0 error; schema 21/21, globalization 688/688 |
| Gate/image/Kubernetes | Sweep 39/39 (22 skip theo manifest), image build/health/network/scan/SBOM/E2E 8 task PASS; Kubernetes 7/7; W-0181/W-0183 pins đã được kiểm trên hosted |
| Observability | [Runtime](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951081): trace 5 stage, metric/log/dashboard truy vấn được; LGTM dừng vẫn live và không gửi trùng callback |
| Security/privacy | [Security](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951082): Gitleaks 349 commit no leaks, fake-secret control PASS, NuGet HIGH gate PASS, npm 0 vulnerability. [PII](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951083): 418 file/2 binary PASS; negative controls PASS |
| JUnit | Full suite 1.018 case/433 name được chuẩn hóa; schema 21, globalization 688; lần chạy thứ hai đổi 0. Privacy scan trên artifact thật đã PASS |
| Publish | [Ba image](published-images-179a5eb.json) tag `0.1.0-179a5ebc` đã push, mỗi image scan HIGH/CRITICAL=0 trước push. [Portal API](https://ginsengfood-ivr-0332fa.gitlab.io) Active, mở bằng browser thấy draft.27/38 operation/non-production |
| Deploy | [Dev job](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16478951087): bootstrap Helm/kubectl PASS; [cluster unreachable](dev-deploy-blocker.log) tại localhost:8080. Staging không chạy; không thực hiện manual staging evidence hoặc lab/prod promotion |

**Bước tiếp theo:** cần chọn/cấp máy chủ hoặc cluster đích cho dev/staging, rồi cấu hình kết nối CI, PostgreSQL, hai secret của chart, registry pull, DNS/TLS. Sau đó retry deploy trên candidate có digest, smoke test và diễn tập staging. Chưa có target nên không tự cấp dịch vụ tính phí hoặc ghi nhận Kubernetes selftest như staging thật. [Phiếu hạ tầng](../../../plan/ivr-orther/questions-to-platform-ci-and-staging-2026-09-12.md) vẫn NOT_SENT; review độc lập và các cổng M3/SIM/Legal chưa đóng.

## Lịch sử trước candidate hiện hành

Candidate `6bf954d2-ba8fd82e-b638b589-2c11df77-9c88eb42` (bỏ dấu nối để lấy SHA) đã lên hai remote. [Pipeline 2845851460](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845851460) có **18/36 job PASS** tại checkpoint 14:20 +07, sau đó bị hủy khi push candidate tiếp theo; không phải full pipeline proof.

[Pipeline trước 2845815898](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845815898) chạy trên `b714694`, sau đó bị Auto-cancel khi push bản sửa bootstrap. Cấu hình hoặc local PASS không được tính là deployment proof.

Kiểm trước deploy phát hiện `alpine/helm:3.16.3` có entrypoint `helm`, nên `sh -c` bị từ chối; image cũng thiếu kubectl. Sửa riêng bootstrap dev/staging: clear entrypoint, cài kubectl và kiểm client. Container thật không có kubeconfig chạy được Helm 3.16.3/kubectl 1.31.5; cần kiểm tương thích version khi Platform xác nhận cluster. [Regression](bootstrap-regression.log) từ [script](bootstrap-regression.mjs): 4 mutation (thiếu entrypoint reset/kubectl ở mỗi môi trường) bị từ chối đúng lý do, cấu hình hợp lệ PASS. CD 5/5 và CI config PASS; GitNexus guard LOW, 1 file caller/0 process.

Ngày 14/09 owner chưa biết đích deploy. Tự kiểm [GitLab cluster](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/clusters): 0 Agent; trang CI có 2 biến API_DOCS_PUBLISH_NONPROD và IVR_W0061_PROTECTED_PROBE, không mở giá trị. Máy local `kubectl config get-contexts` rỗng, không file kubeconfig mặc định hoặc biến KUBECONFIG. **Chưa có đích cluster/credential được xác nhận**; chart còn cần secret ivr-database/ivr-app-secrets, PostgreSQL và quyền kéo registry. Không coi Kubernetes dùng tạm trong selftest là dev/staging đã triển khai.

Lab, production, real customer calls chưa được mở. M3/SIM/Platform/release gates giữ theo evidence thực tế.

## Checkpoint sau sửa bootstrap (lịch sử)

Tại checkpoint, hai remote đã nhận `6bf954d2-ba8fd82e-b638b589-2c11df77-9c88eb42`; pipeline 2845851460 đang chạy. GitLab đã tự huỷ lượt b714694 theo Auto-cancel redundant pipelines. Lượt trước có sweep 39/39, contract/E2E/review/observability PASS; không ghi là full pipeline PASS.

[Scan lịch sử candidate mới](exact-commit-gitleaks.log): 345 commit no findings. [Migration image local](migrate-scan.log): build tag ivr-migrate:w0292-6bf954d exit 0, Trivy HIGH/CRITICAL 0 finding; bổ sung phạm vi chưa có trong image-selftest (chỉ API/worker). Đây chưa phải digest registry hay hosted publish proof.

## Hosted checkpoint trước bản sửa privacy

- [Image job 16477133512](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133512): `IMAGE_SELFTEST_PASS`; build/non-root, live/no DB, Compose isolation, scan 2 image, SBOM và E2E 8 task đều PASS trong một lượt. Ca operator pause trả WINDOW_EXPIRED/hold-review đúng quyết định, không thiếu năng lực giả.
- [Kubernetes job 16477133513](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133513): `K8S_SELFTEST_PASS`, 7 marker PASS. Cụm thử trong DinD, không phải môi trường dev/staging đích.
- [Chaos job 16477133511](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133511): 8/8; [sweep job 16477133506](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/jobs/16477133506): 39/39. Contract, E2E suite, review gate, observability contracts/rules/Helm và gate mirror cũng PASS.
- Local preflight phát hiện privacy sẽ chặn 121 dòng tài liệu cũ. [W-0293](../W-0293/README.md), commit `ff9c4b5`, sửa cách trình bày; full evidence scan/selftest PASS. [Scan commit đó](privacy-candidate-gitleaks.log): 347 commits scanned, no leaks. Khi ấy còn cần push; kết quả hosted sau W-0294/W-0295 đã ghi ở đầu hồ sơ này.

[Kiểm hash đã rà](reviewed-hash-verification.log) từ [script chỉ đọc](verify-reviewed-hashes.mjs): 50/50 khớp nguồn bất biến (44 candidate + 6 portal), thử byte gốc/LF/CRLF. [Known-bad image control](known-bad-image-control.log): Trivy 0.58.1 trả đúng exit 42 và JSON có 1 HIGH/CRITICAL; không tính lỗi công cụ thành bằng chứng chặn vulnerability.
