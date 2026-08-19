# W-0046 — Evidence: Progressive delivery & canary (`P7-4`)

Ngày: `2026-08-19` · Trạng thái: **1/4 test đọc code thật**, 3/4 `CONFIGURATION_ONLY`;
canary run và auto-rollback demo (§10) **`NOT_RUN`**

Chiến lược đầy đủ: [`docs/progressive-delivery.md`](../../progressive-delivery.md).

## 1. Giới hạn, nói trước

Argo Rollouts **chưa cài**, và **không có Prometheus nào nhận metric của IVR** (`W-0063`,
`BLOCKED_EXTERNAL` — không có exporter OTLP nào được nối từ `W-0040`). Nên:

- `IT-CANARY-01`, `IT-BG-WORKER-02`, `IT-FLAG-RAMP-04` là **kiểm cấu hình**.
- `P7-4` §10 đòi canary run, auto-rollback demo, blue-green switch demo — **cả ba `NOT_RUN`**.

**`IT-MIGRATE-03` thì khác.** Nó đọc **migration thật** và đỏ với bất cứ thứ gì phá vỡ cửa sổ chồng
lấn hai phiên bản — chính cái cổng mà `P7-3` §5 đã nêu là còn thiếu.

## 2. Cổng thật: migration expand-contract

`P7-3` §5 ghi cái bẫy: `helm rollback` đưa **manifest** về revision cũ nhưng **không** hoàn tác
migration. Canary còn làm nó thường trực — trong suốt cửa sổ analysis, code cũ và code mới **cùng**
chạy trên **một** schema.

Cổng ép trong `Up()`:

| Cấm | Vì sao |
| --- | --- |
| `DropColumn`, `DropTable` | phiên bản cũ vẫn `SELECT` cột đó |
| `RenameColumn`, `RenameTable` | với phiên bản cũ, đổi tên = xoá |
| `AlterColumn` | thu hẹp kiểu làm hỏng ghi của phiên bản cũ |
| `nullable: false` **không** `defaultValue` | schema nhận, rồi `INSERT` của phiên bản cũ hỏng lúc chạy |

**`Down()` cố ý không bị kiểm.** Xoá thứ `Up()` vừa tạo *chính là* định nghĩa của down migration —
repo này có **63** thao tác phá nằm trong `Down()`, và một cổng bắt cả chúng sẽ đỏ với **mọi**
migration từng viết rồi bị tắt trong một tuần. Phân biệt `Up()`/`Down()` là thứ làm cổng này dùng
được thay vì thành nhiễu.

Trạng thái đo được: **5 migration, 42 `AddColumn` trong `Up()`, 0 thao tác phá, 0 cột NOT NULL thiếu
default.** Cổng xanh vì code đang đúng, không phải vì cổng dễ.

Kiểm âm: trồng `DropColumn` vào `Up()` → **đỏ**; trồng cột `nullable: false` không default → **đỏ**.

## 3. Vì sao worker không canary — lý do **không phải** tính đúng đắn

Advisory lock và lease fencing (P2-3) làm hai scheduler chạy song song **an toàn**:
`IT-SCH-CLAIM-01` đã chứng minh hai worker cùng claim tạo ra **đúng một** attempt và **một** lease.

Rủi ro thật là khác: canary giữ hai phiên bản chồng nhau **suốt thời gian analysis** — mười, hai
mươi phút hai scheduler sinh attempt xen kẽ. Đó là trạng thái **tệ nhất để debug**, vì mọi lỗi lập
lịch của bản mới đều khó quy trách.

Blue-green thu cửa sổ đó về **một lần đổi service**. Bất biến giữ an toàn vẫn là cùng cái lock;
lựa chọn ở đây là *một trạng thái khó hiểu được phép tồn tại bao lâu*.

Lý do đó được ghi **trong annotation của chính Rollout**, và `IT-BG-WORKER-02` đỏ nếu nó biến mất —
người đọc sau này không biết về advisory lock sẽ "cải tiến" nó thành canary.

## 4. Cổng SLO chính là ngưỡng paging

Ngưỡng latency của canary **bằng đúng** ngưỡng alert `P6-2` (D-04, 5s). Một cổng canary **lỏng hơn**
ngưỡng paging sẽ promote một phiên bản rồi lập tức đánh thức người trực — khi đó canary không giảm
rủi ro, nó chỉ dời rủi ro sang ca đêm.

Bất đối xứng có chủ đích: **5** lần đọc tốt mới promote, **1** lần đọc xấu là abort.

Mọi `pause` có `duration`; pause vô hạn nghĩa là chờ người, và canary không ai nhìn sẽ nằm ở 10% cho
tới khi có người tình cờ nhận ra.

## 5. Tái dùng cổng của `P6-2` cho `deploy/rollouts`

`UT-DASH-PII-04` giờ quét **cả** `deploy/rollouts`, không chỉ `deploy/observability`.

Chỗ này cần nó hơn: một SLO query trỏ vào metric không ai phát trả về **"no data"**, và hầu hết
analysis engine đọc no-data là **"không hỏng"** — canary sẽ **tự promote trên sự im lặng**.

Kiểm âm: đổi một query sang `ivr_call_results_total` (khai báo nhưng **chưa có call site**) →
`UT-DASH-PII-04` **đỏ**; khôi phục → xanh.

## 6. Deploy ≠ release

`IT-FLAG-RAMP-04` đỏ nếu bất kỳ file rollout nào chạm feature flag. Nếu rollout bật được tính năng
thì hai việc lại là một sự kiện, và feature flag chỉ còn là tài liệu.

Và **sàn governance đi cùng canary**: cả hai rollout ghim `IVR_EXECUTION_MODE=MOCK` và
`REAL_CUSTOMER_CALL_ALLOWED=NO` trên phiên bản mới — một canary mang tư thế khác bản stable sẽ làm
ladder **phụ thuộc vào pod nào trả lời request**.

## 7. Kiểm chứng

| Test | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `IT-CANARY-01` | bỏ SLO analysis khỏi canary | ❌ đỏ |
| `IT-BG-WORKER-02` | bật `autoPromotionEnabled` | ❌ đỏ |
| `IT-MIGRATE-03` | `DropColumn` trong `Up()`; cột NOT NULL thiếu default | ❌ đỏ (cả hai) |
| `IT-FLAG-RAMP-04` | — (xem §6) | ✅ |
| `UT-DASH-PII-04` mở rộng | SLO query dùng metric chưa có call site | ❌ đỏ |
| cổng topology | `progressive_selftest` `allow_failure: true` | ❌ đỏ |

| Lệnh | Kết quả |
| --- | --- |
| `progressive-selftest.mjs` | `PROGRESSIVE_SELFTEST_PASS` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` |
| `scan-pii.sh` | `PII_SCAN_PASS` |

## 8. Cái này KHÔNG chứng minh

- **Chưa canary nào chạy**; chưa auto-rollback nào diễn ra; chưa blue-green switch nào được diễn tập.
  Argo Rollouts chưa cài, Prometheus chưa nhận gì (`W-0063`).
- **Chưa chứng minh hai phiên bản chạy song thật.** `IT-MIGRATE-03` chứng minh migration **cho phép**
  điều đó; nó **không** chứng minh đã có ai chạy hai phiên bản cùng lúc.
- **Ngưỡng fail-closed 20% là đề xuất**, chưa có baseline production.
- **Chưa cổng nào ép rằng một migration mới phải đi kèm code chịu được schema cũ** — cổng hiện tại
  chỉ chặn chiều ngược lại (schema mới phá code cũ).
- **`ivr-admin-ui` không có progressive delivery** — cố ý, vì nó đọc-thuần; ghi ra để không ai đọc
  nhầm là thiếu sót.
