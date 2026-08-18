# Container images & dev stack — `W-0043` · `P7-1`

## 1. Ba deployable, ba image (DTS-04)

| Image | Base runtime | USER | Kích thước | HEALTHCHECK |
| --- | --- | --- | --- | --- |
| `ivr-api` | `aspnet:10.0-noble-chiseled` | `1654` | ~56 MB | `/health/live` |
| `ivr-worker` | `runtime:10.0-noble-chiseled` | `1654` | ~44 MB | **không có** — xem §4 |
| `ivr-admin-ui` | `node:22-alpine` | `node` | ~70 MB | `/login` |

Worker dùng `runtime` chứ không `aspnet`: nó là `Microsoft.NET.Sdk.Worker` trên
`Host.CreateApplicationBuilder` và **không mở socket nào**. Ship cả tầng ASP.NET vào đây là thêm bề
mặt tấn công cho một server không bao giờ khởi động.

## 2. Ghim bằng digest, không phải tag

Mọi base ghim theo `tag@sha256:...`. Một tag là con trỏ di động: `10.0-noble-chiseled` tháng sau
trỏ vào image khác, và một bản build "đã qua scan" khi đó sẽ ship những bit **chưa ai quét**. §11
cấm base không ghim; digest là dạng ghim duy nhất thực sự giữ.

SDK ghim ở band `10.0.2xx` vì `global.json` ghim `10.0.201` với `rollForward=latestPatch` — nó từ
chối feature band khác. Khớp theo repo chứ không nới `global.json`: cái ghim đó tồn tại để mọi bản
build dùng đúng SDK mà test đã chạy trên đó.

## 3. Healthcheck trên image không có shell

Image chiseled không có shell, không có `curl`, không có `wget`. Cách giải: chép **một binary tĩnh
busybox** (~1 MB) vào stage cuối và gọi applet trực tiếp:

```dockerfile
COPY --from=probe /bin/busybox /probe/busybox
HEALTHCHECK CMD ["/probe/busybox", "wget", "--quiet", "--spider", "http://127.0.0.1:8080/health/live"]
```

Không có shell nào được thêm vào. Cách thay thế — đổi sang base có shell — sẽ đánh đổi bề mặt tấn
công lấy một dòng healthcheck.

Probe là **liveness**, cố ý. Readiness phụ thuộc database, và dùng nó làm healthcheck container sẽ
khiến container restart liên tục trong lúc dependency có sự cố — đúng thứ `P6-1` tách ba probe ra để
tránh.

## 4. Worker không có HEALTHCHECK, và đó là quyết định

`P7-1` §6.2 giả định worker giữ health/metrics trên HTTP. Nó **không**: không mở socket nào, nên
không có gì để probe. Một kiểm tra process-liveness sẽ là diễn kịch — process chết thì container
chết theo, nên kiểm tra đó không bao giờ báo được điều gì runtime chưa biết. Ghi là khoảng trống ở
`docs/evidence/W-0043/` thay vì lấp bằng một probe luôn xanh.

## 5. Build & chạy

```bash
docker build -f deploy/docker/Dockerfile.api    -t ivr-api:dev .
docker build -f deploy/docker/Dockerfile.worker -t ivr-worker:dev .
docker build -f deploy/docker/Dockerfile.ui     -t ivr-admin-ui:dev admin-ui
docker compose -f docker-compose.dev.yml up -d --build
```

Kiểm toàn bộ như CI làm:

```bash
node deploy/ci/scripts/image-selftest.mjs
```

## 6. Quy ước tag

`<service>:<semver>-<git-sha-ngắn>`, ví dụ `ivr-api:1.0.0-4136ad1`. Semver cho người đọc, sha cho
máy: chỉ semver thì hai build khác nhau có thể mang cùng tên.

## 7. Secret

**Không secret nào nằm trong layer** (D-05). `IVR_INTERNAL_SERVICE_TOKEN` và
`ORDER_CORE_SERVICE_TOKEN` là env lúc chạy, và app **từ chối boot** nếu thiếu — đã kiểm bằng cách
chạy image không có chúng: container thoát với `OptionsValidationException`. Đó là fail-closed đúng,
không phải lỗi.

`.dockerignore` loại `**/.env*`, `*.pem`, `*.key`, `*.pfx` khỏi build context, nên chúng không thể
vào layer do sơ ý.

## 8. Hai mạng, hai mức bảo đảm khác nhau

| Mạng | | |
| --- | --- | --- |
| `ivr-internal` | `internal: true` | **không có tuyến ra ngoài**. Các fake sống ở đây, nên một mock không bao giờ chạm được endpoint Sales thật hay nhà mạng thật — dù cấu hình sai thế nào |
| `ivr-database-local` | routable | api/worker/ui ở đây để lập trình viên truy cập được, nên bảo đảm no-egress của chúng là **ở tầng ứng dụng** (`IVR_ADAPTER_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`, và test kiến trúc cấm client egress), **không** phải tầng mạng |

Phải tách hai mức vì cổng publish **không hoạt động** trên mạng `internal: true` — đo được, không
đoán. Người đọc tưởng container app bị cô lập mạng sẽ rút ra kết luận mạnh hơn thực tế.

## 9. Quét image

`IT-IMG-SCAN-04` chạy Trivy với `--exit-code 1` trên `HIGH,CRITICAL`, và kèm **một đối chứng
dương**: quét một base có lỗ hổng đã biết và đòi nó **đỏ**. Một scanner không bao giờ đỏ thì không
phân biệt được với một scanner hỏng.

Phát hiện thật trong lúc dựng: bản đầu của `ivr-admin-ui` có **7 HIGH + 1 CRITICAL**, và cả 8 nằm
trong **npm đi kèm base image** (`/usr/local/lib/node_modules/npm/...`), không phải dependency của
console. Runtime chạy `node server.js` nên không cần npm — gỡ nó đi làm sạch cả 8, đồng thời bỏ một
package manager khỏi image production, vốn là công cụ sẵn có cho bất kỳ ai lấy được shell.
