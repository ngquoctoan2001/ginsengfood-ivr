# W-0270 — Một comment nói dối về UID, và một guard có sẵn bị tắt đúng chỗ cần

Ngày: 2026-09-10 · Baseline: `main@e36234b` · Trạng thái: **TESTS_PASS**.

C4 và C5 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

## 1. C4 — `USER $APP_UID`

Phát hiện đúng: comment viết *"Stated explicitly rather than inherited"* ngay trên `USER $APP_UID`,
mà `$APP_UID` **chính là** dạng kế thừa. Nhưng nó nhỏ hơn thực tế ở ba chỗ.

### 1.1 Ba chỗ báo cáo chưa thấy

**Không phải một file, mà ba.** `Dockerfile.api`, `Dockerfile.worker`, `Dockerfile.migrate` đều dùng
`USER $APP_UID`. Hai file sau **không có comment nào cả** — nên chúng không "nói dối", chúng im lặng,
điều còn khó thấy hơn.

**Test được viện dẫn không khẳng định thứ comment nói.** `IT-IMG-BUILD-01` khẳng định user **không
phải root** (`user !== ""`, `!== "root"`, `!== "0"`). Nó **không** khẳng định UID 1654. Nó cũng chỉ
phủ `Dockerfile.api` và `Dockerfile.worker` — `Dockerfile.migrate` không có test nào nói gì về user
của nó. Và nó `sweepable: false` (cần Docker), nên **không chạy trong gate sweep**.

**Repo đã có sẵn dạng đúng.** `deploy/tts/Dockerfile.tts:60` viết `USER 1654:1654`, và
`tts-container-selftest.mjs:29` khẳng định đúng chuỗi đó. Cách làm đúng đã tồn tại, chỉ không được
áp cho ba image kia.

### 1.2 Vì sao con số quan trọng — điều báo cáo không nêu

Ba thứ trong repo phụ thuộc vào **giá trị chính xác** của UID, và không thứ nào đọc được base image:

| Nơi | Phụ thuộc |
| --- | --- |
| `deployment-worker.yaml:35,163,164` | ghim `fsGroup`, `runAsUser`, `runAsGroup` = **1654** |
| `deployment-api.yaml:24` | `runAsNonRoot: true` **không** kèm `runAsUser` — Kubernetes phải đọc được UID **số** từ image config, nếu không pod không khởi động |
| `deploy/docker/README.md:7-8` | ghi 1654 cho cả `ivr-api` lẫn `ivr-worker` |

Một base image đổi `APP_UID` sẽ dời image và **bỏ lại cả ba**, im lặng.

### 1.3 Sửa

`USER 1654:1654` ở cả ba file, theo đúng tiền lệ `Dockerfile.tts`. Comment viết lại cho đúng thứ
dòng code làm.

Và **`CT-CI-11`** trong `ci-config-selftest.mjs` — kiểm tĩnh, nên nó **chạy trong sweep**, khác
`IT-IMG-BUILD-01`:

| Khẳng định | Tiêm thử | Kết quả |
| --- | --- | --- |
| Mọi `USER` nêu UID bằng số, không qua biến | trả `Dockerfile.worker` về `USER $APP_UID` | đỏ, nêu đúng file và dòng |
| UID của image khớp `runAsUser` của chart | đổi `Dockerfile.api` sang `1999:1999` | đỏ: *"runs as 1999 but deployment-worker.yaml pins runAsUser: 1654"* |

Khẳng định thứ hai là thứ báo cáo không đòi nhưng mới là cái giữ được lời hứa: nó buộc image và chart
vào nhau, nên hai bên không thể lệch mà không ai biết.

Tiện thể: lý do trong `gate-invocations.json` viết *"runs docker build against the UI and API
images"* — image UI đã bị xoá ở `W-0253`, và nó dựng api + worker. Đã sửa.

## 2. C5 — bốn service `Singleton` không có gì bảo vệ

### 2.1 Bảy, không phải bốn

`InternalAdminApiServiceCollectionExtensions.cs` đăng ký singleton cho **bảy** type:
`InternalAdminApiService`, `AdminReadService`, `AdminConfigReadService`, `AnalyticsReadService`,
`ScriptLifecycleApiService`, `SeedCatalog`, `DevToolingApiService`.

### 2.2 Điều báo cáo không nói: guard đã có sẵn, và bị tắt đúng chỗ cần

Báo cáo đề nghị "comment, analyzer hoặc test". Không cần cái nào — .NET đã ship đúng guard này, và
nó **đang tắt ở nơi duy nhất quan trọng**:

`WebApplication.CreateBuilder` chỉ bật `ValidateScopes`/`ValidateOnBuild` khi environment là
`Development`. `Dockerfile.api` đặt `ASPNETCORE_ENVIRONMENT=Production`. Nên trong container —
đúng nơi race xảy ra dưới tải — **không có validation nào chạy**.

Đã bật tường minh ở cả hai host. `ValidateOnBuild` duyệt toàn đồ thị lúc khởi động và **từ chối
boot** khi một singleton bắt giữ service scoped.

### 2.3 Chứng minh, ở cả hai host

Tiêm đúng kịch bản phát hiện mô tả — cho một trong các singleton đó nhận một service scoped:

```
Cannot consume scoped service 'Ivr.Api.Health.IIvrReadinessProbe'
  from singleton 'Ivr.Api.Application.IAdminReadService'.
```

Race dưới tải trở thành một lỗi lúc boot, **nêu đích danh service**.

Worker phải kiểm riêng vì hai lý do: `HostApplicationBuilder` không có `.Host` nên phải đi qua
`ConfigureContainer(new DefaultServiceProviderFactory(...))` — một API khác, có thể không có hiệu
lực; và **không test nào khởi động host của Worker**, nên suite không nói gì về nó. Đã chạy thật:

- Bình thường: Worker **vượt qua `builder.Build()`**, chỉ chết ở kết nối database — tức validation
  chạy và đồ thị sạch.
- Tiêm một singleton phụ thuộc `CallbackDispatcher` (scoped):
  `Cannot consume scoped service 'Ivr.Infrastructure.Callbacks.CallbackDispatcher' from singleton
  'ProbeCapture'.`

### 2.4 Guard cho guard

`ARCH-DI-VALIDATE-01` khẳng định cả hai `Program.cs` vẫn yêu cầu `ValidateOnBuild` và
`ValidateScopes`. Khẳng định ở **mức nguồn**, và đó là lựa chọn có lý do: một host bị gỡ lời gọi đó
vẫn pass mọi test chỉ kiểm "app khởi động được", nên hành vi không gánh được việc này.

### 2.5 Nửa còn lại của C5, cố ý không làm

Phát hiện cũng lo "ai đó thêm một field mutable". Không cấm được bằng máy mà không sai: một singleton
giữ trạng thái không tự nó là lỗi — `InMemoryIdempotencyStore` giữ trạng thái một cách chính đáng.
Nửa **nguy hiểm và máy kiểm được** — singleton bắt giữ scoped — nay đã được chặn.

## 3. Một lỗi của tôi trong lượt này

Khi tiêm vi phạm để thử `CT-CI-11`, tôi khôi phục bằng `git checkout -- <file>`. Nhưng thay đổi C4
**chưa được commit**, nên lệnh đó hoàn nguyên về `HEAD` và **xoá luôn phần sửa của tôi** ở
`Dockerfile.api` và `Dockerfile.worker`.

Phát hiện vì selftest ngay sau đó im lặng, rồi in ra chính hai file đó là vi phạm. Cùng họ với lỗi
`rm -rf` ở `W-0264`: dùng một lệnh phá huỷ trên cây có việc chưa lưu.

Đã áp dụng lại, và các lần tiêm sau khôi phục từ **bản sao trong scratch**, không từ git.

## 4. Kết quả

```
Unit         670/670   (+1)
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             985/985
```

`dotnet build Ivr.sln` — 0 warning, 0 error.

Gate sweep (repo thật): **37/39 run**, 2 FAIL — cả hai là pin
`integration-requirements/06-module-3-api-handover.md` của agent khác, đỏ từ trước lượt này.

Không file nào tôi đổi bị pin, nên lượt này **không có cascade**.

Impact analysis: **LOW**, 0 execution flow bị ảnh hưởng.

## 5. Còn lại

**25/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH. Toàn bộ phần thuần kỹ thuật đã hết.

Chỉ còn những mục cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — tách tên xong thì 22 site thay được an toàn.
- **`phone_validation_status`** — contract nói required, intake nhận thiếu.
- **`NEXT_WORK_ID` lệch 20** — tracker ghi `W-0250`, evidence đã tới `W-0270`.
- Phần S4 còn lại: `assertString` (11/9), `assertExactKeys` (13/8), `readStrictJson` (6/5) — khác biệt
  có tải, hợp nhất là đổi thứ gate chấp nhận.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
