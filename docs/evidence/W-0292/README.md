# W-0292 — Hosted CI và dev–staging

Ngày 14/09/2026. **IN_PROGRESS**. Owner duyệt push/scan/publish/deploy dev–staging và 68 fingerprint đã rà ở W-0291.

Candidate `b7146949-e1446a2d-d27ab2c2-9d6bf07c-6c810f1f` (bỏ dấu nối để lấy SHA). `git push origin main` thành công tới GitLab và GitHub; `git ls-remote --heads` cả hai kho khớp candidate. Không tạo branch.

[Pipeline 2845815898](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845815898) đã khởi chạy trên candidate, **36 job**. Đang theo dõi job bắt buộc và các bước tự publish/deploy. Cấu hình hoặc local PASS không được tính là deployment proof.

Kiểm trước deploy phát hiện `alpine/helm:3.16.3` có entrypoint `helm`, nên `sh -c` bị từ chối; image cũng thiếu kubectl. Sửa riêng bootstrap dev/staging: clear entrypoint, cài kubectl và kiểm client. Container thật không có kubeconfig chạy được Helm 3.16.3/kubectl 1.31.5; cần kiểm tương thích version khi Platform xác nhận cluster. [Regression](bootstrap-regression.log) từ [script](bootstrap-regression.mjs): 4 mutation (thiếu entrypoint reset/kubectl ở mỗi môi trường) bị từ chối đúng lý do, cấu hình hợp lệ PASS. CD 5/5 và CI config PASS; GitNexus guard LOW, 1 file caller/0 process.

Ngày 14/09 owner chưa biết đích deploy. Tự kiểm [GitLab cluster](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/clusters): 0 Agent; trang CI có 2 biến API_DOCS_PUBLISH_NONPROD và IVR_W0061_PROTECTED_PROBE, không mở giá trị. Máy local `kubectl config get-contexts` rỗng, không file kubeconfig mặc định hoặc biến KUBECONFIG. **Chưa có đích cluster/credential được xác nhận**; chart còn cần secret ivr-database/ivr-app-secrets, PostgreSQL và quyền kéo registry. Không coi Kubernetes dùng tạm trong selftest là dev/staging đã triển khai.

Lab, production, real customer calls chưa được mở. M3/SIM/Platform/release gates giữ theo evidence thực tế.
