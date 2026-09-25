# W-0357 — Lô `L3` của kế hoạch khắc phục `25/09`: tài liệu nội bộ nói sai code

Ngày 25/09/2026 · Claude, trong lượt *"rà soát tiếp việc cần làm"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

Lô `L3` của kế hoạch khắc phục `25/09` (`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`) gom sáu chỗ tài liệu nội bộ nói
một điều mà code không làm, hoặc thiếu điều mà code làm. Hai chỗ trong đó (`K-20`, `K-21`) là phần `W-0354` của
chính lượt này viết sai. Mỗi khẳng định đã được đọc lại trên code trước khi sửa; chỗ nào không kiểm được thì ghi rõ.

## Đã làm

| Mục | Sai ở đâu | Đã sửa |
| --- | --- | --- |
| `K-20` | `deploy/ci/rollback.md` §3b còn năm câu mâu thuẫn với đính chính `P03` mà `W-0354` thêm. Nặng nhất là câu `W-0354` viết: database dựng mới "rơi vào dòng `true` với bảng rỗng, và như vậy là đúng" — theo chính bảng `p03_applied` ngay trên, bảng rỗng nghĩa là **không xác định được từ schema** | Sửa năm chỗ; thêm câu phải ghi kết quả vào nhật ký triển khai **trước** khi nâng cấp, vì lần migrate kế tiếp tự chạy `P03` và xoá dấu vết. Hồ sơ `W-0306` nhận đính chính có ngày, không sửa nội dung cũ |
| `K-21` | Phép kiểm nguồn `D07` mà `W-0354` thêm vào gate `capacity-selftest.mjs` nằm trong nhánh chỉ chạy khi `sessionSeconds` bằng 45 phút — mà `sessionSeconds` luôn `null`. Không bao giờ đỏ được | Kiểm luôn chạy: con số 2.700 giây và nguồn `D07` phải còn. Bỏ "D07" khỏi `candidateSource` thì gate đỏ. Chú thích "It is unanswered" sửa cho khớp: độ dài đã có đáp án, phân bố đơn thì chưa. Hồ sơ `W-0134` nhận đính chính có ngày về tên thuộc tính mà `W-0354` đổi. Kế hoạch còn đề nghị dẫn `FIX_OWNER_CORE.md:160`; **chưa làm**, vì file đó không có trên máy dev nên không kiểm được dòng ấy |
| `K-22` | `README.md` còn badge "NOT_RUN", console Next.js, `pnpm dev`, cổng 3005, và hứa `/health/ready` trả `503` khi callback circuit mở; `deploy/ci/README.md` ghi "không runner"; chú thích `Dockerfile.worker` nói worker không mở socket | Badge ghi pipeline chạy nhưng chưa xanh; bỏ phần console và `pnpm dev`. `503` chỉ còn hai điều kiện thật: API không đăng ký circuit breaker (`AddIvrCallbackDelivery` chỉ gọi ở worker), nên kiểm callback trên API luôn là `not_configured`. Runner có từ `W-0292`. Worker có `/healthz` ở cổng 8081 (`WorkerHealthEndpoint`) mà chart dùng cho liveness |
| `K-23` | Sổ secret thiếu ba token tầng admin | Ba dòng `IVR_ADMIN_READ/WRITE/DANGER_TOKEN`; runbook thêm §7 xoay từng tầng theo giá trị chart có sẵn. Ghi hai ràng buộc code đã ép: không tầng nào dùng chung giá trị (boot từ chối), chart từ chối khi thiếu một trong hai giá trị xoay |
| `K-24` | Hash payload intake tính trên **toàn bộ** body, gồm `phone_e164` khi M3 gửi số, và nằm ở ba chỗ, một chỗ là audit append-only | Sổ dữ liệu ghi nó là dữ liệu giả danh hoá của số, DSAR không chạm tới; cổng theo tên cột không bắt được |
| `K-25` | Runbook luồng quay số production chỉ nêu ba khoá cố ý | Thêm bảng những gì vẫn chặn hoặc làm hỏng cuộc gọi thật đầu tiên dù mở cả ba khoá: `DispatchGate` từ chối mọi đích quay production; gate chỉ kiểm một lần trước khi dựng lời thoại; số nằm trong URL gửi ARI; luật quá độ về giọng; người duyệt kịch bản |

## Kiểm chứng

| Gate | Kết quả |
| --- | --- |
| `capacity-selftest.mjs` | đạt; bỏ "D07" khỏi `candidateSource` thì đỏ đúng câu báo nguồn, khôi phục thì xanh |
| `docs-selftest.mjs` | đạt sau mọi thay đổi tài liệu |
| `ci-config-selftest.mjs` | đạt sau khi sửa chú thích `Dockerfile.worker` (dòng `USER` không đổi) |
| `compliance-pack-selftest.mjs` | đạt sau khi thêm dòng vào sổ dữ liệu |

Bộ unit `815/815` chạy trên bản sao tách riêng gồm `HEAD` cộng các file của `W-0356` và `W-0357`; trong đó có test đọc
`data-inventory.md` và test đọc `rollback.md`. Quét PII trên `docs/evidence` đạt.
