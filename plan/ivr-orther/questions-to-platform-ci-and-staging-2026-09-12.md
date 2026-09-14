# Phiếu yêu cầu — gửi Platform / Infra

**Chủ đề:** Hai cổng hạ tầng đang chặn Module 8 — pipeline bắt buộc (`G-GITLAB`) và môi trường staging (`G-PLATFORM`)
**Người gửi:** Team Module 8 — IVR Order Confirmation
**Ngày lập:** `2026-09-12` · **Mốc mã:** `main@b0cb633`
**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT`
**Đối soát 14/09 sau đăng nhập:** [W-0285](../../docs/evidence/W-0285/README.md). Hai remote đã nhận `6bf954d`; [pipeline 2845851460](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845851460) có 18/36 job PASS tại checkpoint: full image E2E, Kubernetes 7/7, Chaos 8/8, sweep 39/39. [W-0292](../../docs/evidence/W-0292/README.md) theo dõi phần còn lại và bản sửa PII W-0293; full pipeline/publish/deploy chưa có kết quả cuối. GitLab 0 Kubernetes Agent, chưa có kubeconfig/context được xác nhận; owner chưa biết đích cluster. Phiếu vẫn chưa gửi.
**Ưu tiên:** P1. Mục A chặn mọi bằng chứng phát hành; mục B chặn toàn bộ làn C của kế hoạch hoàn thiện

> Hai mục độc lập. Trả lời được mục nào thì đóng mục đó, không cần chờ đủ hai.
> Phiếu này **không** xin gì về SIM, trunk hay nhà mạng — việc đó còn sau, và có phiếu riêng.

---

## Vì sao gửi hôm nay, không phải cuối tháng

Sáng `2026-09-12`, khi chạy tay toàn bộ các cổng để lập báo cáo tuần, phát hiện **một nửa cổng kiểm bản đóng gói đã chết 15 ngày** mà không ai biết. Nguyên nhân: nó gọi bề mặt quản trị bằng cơ chế xác thực mà `W-0128` đã xoá từ `2026-08-28`; máy chủ không báo lỗi, nó chỉ bỏ qua header cũ, nên mọi lệnh trả `401`.

Trong repo có **39 đầu việc CI và 50 bài tự kiểm đã khai**. W-0061/W-0093 lưu bằng chứng hosted CI thật, gồm pipeline `2760238052` tại `001d2f57`; đó là lịch sử, chưa chứng minh runner và toàn bộ cổng vẫn chạy trên ứng viên hiện hành. Cần đọc pipeline/jobs/runner hiện tại trước khi kết luận nguyên nhân không phát hiện lỗi.

Đây là lý do cụ thể, không phải lập luận chung: **cổng kiểm không chạy tự động thì nó mục nát âm thầm.** Xin duyệt mục A trước mục B nếu phải chọn.

---

## Mục A — Pipeline bắt buộc (`G-GITLAB`)

### A.1 Cần chính xác bốn thứ

| # | Cần gì | Ghi chú kỹ thuật |
| --- | --- | --- |
| A1 | **Tái sử dụng runner hiện có** cho `nqt20102001/ginsengfood-ivr`, thẻ **`ginsengfood-docker`** | Đã xác minh `55115499` / `ivr-docker-winhost` online, version 19.2.0; pipeline 6bf954d đã được nhận và chạy |
| A2 | Xác minh runner **vẫn chạy được Docker** (docker-in-docker hoặc socket) | DinD/sweep, full image, Kubernetes và Chaos tại 6bf954d đã PASS; còn full pipeline và bản sửa privacy |
| A3 | **Kiểm tra gói và khả năng required approvals hiện tại**, chỉ trình phương án nâng gói nếu còn thiếu | Giới hạn gói trong W-0061/W-0266 là lịch sử; phiên đăng nhập đã dùng được. Cần phương án enforcement/review độc lập tương thích main-only, không tự mua/nâng gói |
| A4 | **Một tài khoản người rà soát thứ hai** có quyền duyệt trên project | Chữ ký không tạo ra người. Hai hạng mục đang mở cần đúng hai người khác nhau |

### A.2 Runner cần bao nhiêu tài nguyên

Đo từ lượt chạy tay hôm nay trên một máy để tham chiếu:

| Nhóm việc | Thời gian thật | Ghi chú |
| --- | ---: | --- |
| Xây bản Release | ~40 giây | |
| Test đơn vị + hợp đồng + hỗn loạn | ~35 giây | |
| Test tích hợp với PostgreSQL thật | ~5–8 phút | Nặng nhất về đĩa và I/O |
| Dựng và kiểm bản đóng gói | ~12 phút | Có quét lỗ hổng, tải ảnh nền |
| Dựng cụm Kubernetes tạm | ~4 phút | Dựng k3s trong Docker |

Đề nghị tối thiểu: **4 vCPU, 8 GB RAM, 60 GB đĩa**, có Internet ra để tải ảnh nền và gói phụ thuộc.

### A.3 Điều kiện coi là xong

Một lượt đầy đủ trên runner gắn đúng SHA, kèm bằng chứng enforcement và người duyệt độc lập đáp ứng G-GITLAB. Repo hiện chỉ cho làm trên `main`: không tạo nhánh/MR thử trái AGENTS.md để tái tạo quy trình lịch sử. Owner/Platform cần xác nhận cách đáp ứng yêu cầu review trong chính sách hiện hành; không tự hạ cổng hay đổi branch protection.

---

## Mục B — Môi trường staging (`G-PLATFORM`)

### B.1 Cần chính xác sáu thứ

| # | Cần gì | Dùng để làm gì |
| --- | --- | --- |
| B1 | **Cụm Kubernetes** với namespace ivr-dev và ivr-staging, cùng kubeconfig/context cho CI | Triển khai API, worker và việc nâng cấp cơ sở dữ liệu |
| B2 | **PostgreSQL** riêng cho staging, không dùng chung với bất cứ gì đang thật | Chứa dữ liệu thử; sẽ bị xoá và nạp lại nhiều lần |
| B3 | **Xác minh kho ảnh hiện có và quyền đẩy** từ runner ở mục A; cấp phần thiếu | W-0061 có proof Registry lịch sử. Cần quyền/job/digest ở ứng viên mới, không suy kho chưa được tạo |
| B4 | **Nơi quản lý bí mật** | Token quản trị ba hạng, token dịch vụ, chuỗi kết nối. Hiện đang là giá trị giả ghi thẳng trong file, chỉ hợp cho máy cá nhân |
| B5 | **Tên miền + chứng thư TLS** cho staging | Module 3 gọi vào được, và chứng minh được phần bảo mật đường truyền |
| B6 | **Bảng theo dõi + nơi nhận cảnh báo** (Prometheus/Grafana hoặc tương đương) | Đo hàng đợi, độ trễ, tỉ lệ lỗi; chưa có nơi nhận thì không chốt được ngưỡng cảnh báo |

### B.2 Những gì Module 8 tự lo, không cần Hạ tầng làm

Biểu đồ Helm, `NetworkPolicy` mặc định chặn hết, chạy không phải quyền cao nhất, giới hạn tài nguyên, kiểm tra sống, việc dọn dữ liệu định kỳ — **đã viết và đã kiểm trên cụm tạm, đạt**. Chỉ thiếu chỗ để chạy thật.

### B.3 Điều kiện coi là xong

Triển khai được, chạy khói đạt, và diễn tập được bốn việc: cuộn dần, xanh–lam, nâng cấp cơ sở dữ liệu thất bại, và quay lui. Sau đó mới đo hiệu năng và chạy liên tục 24–72 giờ.

---

## Mốc mong muốn

| Mục | Mong có trước | Nếu chậm thì sao |
| --- | --- | --- |
| A1–A2 (runner + Docker) | `2026-09-16` | Không có bằng chứng phát hành nào tái tạo được; cổng kiểm tiếp tục mục nát như đã xảy ra |
| A3–A4 (gói + người duyệt) | `2026-09-18` | `G-GITLAB` không đóng được, dù mọi việc khác xanh |
| B1–B6 (staging) | `2026-09-16` | Làn C lùi nguyên khối: không đo được hiệu năng, không diễn tập được khôi phục, không chạy liên tục |

## Liên hệ

Mọi câu hỏi kỹ thuật về phiếu này trả lời trực tiếp trong file và gửi lại, hoặc nhắn cho owner Module 8. Cần thêm số đo nào để duyệt ngân sách thì nêu rõ **cần số gì**, sẽ đo và gửi trong ngày.
