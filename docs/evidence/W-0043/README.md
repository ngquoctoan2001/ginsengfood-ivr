# W-0043 — Evidence: Docker images & dev compose (`P7-1`)

Ngày: `2026-08-18` · Trạng thái: `TESTS_PASS` cho 4 kiểm tra §8

Tài liệu build/run/tag/scan: [`deploy/docker/README.md`](../../../deploy/docker/README.md).

## 1. Ba image, ghim bằng digest

| Image | Base | USER | Size |
| --- | --- | --- | --- |
| `ivr-api` | `aspnet:10.0-noble-chiseled` | `1654` | 57 MB |
| `ivr-worker` | `runtime:10.0-noble-chiseled` | `1654` | 44 MB |
| `ivr-admin-ui` | `node:22-alpine` | `node` | 70 MB |

Worker dùng `runtime` chứ không `aspnet` vì nó là `Microsoft.NET.Sdk.Worker` và **không mở socket
nào** — ship tầng ASP.NET vào đây là thêm bề mặt tấn công cho một server không bao giờ chạy.

Ghim bằng `tag@sha256:` chứ không tag trần: tag là con trỏ di động, và một bản build đã-qua-scan sẽ
ship những bit chưa ai quét khi tag dịch.

SDK phải ghim ở band `10.0.2xx`: `global.json` ghim `10.0.201` với `rollForward=latestPatch`, nên
image `sdk:10.0` (đang là `10.0.400`) bị từ chối. Khớp theo repo thay vì nới `global.json` — cái
ghim đó tồn tại để build dùng đúng SDK mà test đã chạy trên đó.

## 2. Healthcheck trên image không có shell

Image chiseled không có shell, `curl` hay `wget`. Giải bằng cách chép **một binary tĩnh busybox**
(~1 MB) vào stage cuối và gọi applet trực tiếp, không thêm shell nào. Probe là **liveness**, cố ý:
readiness phụ thuộc database, dùng nó làm healthcheck container sẽ khiến container restart liên tục
trong lúc dependency có sự cố — đúng thứ `P6-1` tách ba probe để tránh.

**Worker không có HEALTHCHECK.** §6.2 giả định worker giữ health/metrics trên HTTP; nó không mở
socket nào nên **không có gì để probe**. Kiểm tra process-liveness sẽ là diễn kịch — process chết
thì container chết theo. Ghi là khoảng trống thay vì lấp bằng probe luôn xanh.

## 3. Hai phát hiện thật trong lúc dựng

### 3.1 Stack **không lên được từ đầu** — không có gì chạy migration

Lần dựng tay đầu tiên xanh hết: api healthy, worker running, `/health/ready` 200, intake ghi được.
Nhưng khi self-test chạy `compose down -v` rồi lên lại từ **volume trắng**, worker chết:

```
relation "ivr_sim_channels" does not exist   (exit 139, không bao giờ quay lại)
```

Grep toàn `src/`: **không có `MigrateAsync` nào ở startup của Api hay Worker**. Lần xanh trước đó
chỉ xanh vì volume còn schema từ một phiên trước. Nói cách khác: `docker compose up` trên máy sạch
cho một database rỗng, worker chết vĩnh viễn, và readiness chỉ *trông* như thật.

Sửa: thêm service one-shot `ivr-migrate` dựng từ `dotnet ef migrations bundle`, và api/worker chờ
`service_completed_successfully` chứ không chỉ chờ nó khởi động.

**Job riêng chứ không migrate-on-startup**: nhiều replica API cùng chạy một migration là cách đã
biết để hỏng schema. Stack dev nên mô phỏng hình dạng production cần, không phải lối tắt chỉ đúng
với một replica.

Sau khi sửa, dựng lại từ volume trắng: migrate chạy rồi thoát, **21 bảng `ivr_*`** được tạo (khớp
số `IT-DB-MIGRATE-01` khẳng định), worker sống, `/health/ready` **200**.

### 3.2 Image console có **1 CRITICAL + 7 HIGH**, và chúng đến từ npm trong base

Trivy trên bản đầu của `ivr-admin-ui`: lớp OS sạch, nhưng 8 lỗ hổng ở tầng Node. Kiểm `PkgPath`:

```
usr/local/lib/node_modules/npm/node_modules/...
```

Tất cả nằm trong **npm đi kèm base image**, không phải dependency nào của console. Runtime chạy
`node server.js` và không cần npm — gỡ nó làm sạch cả 8, đồng thời bỏ một package manager khỏi
image production, vốn là công cụ sẵn có cho bất kỳ ai lấy được shell.

Đây là siết thật: không dùng cờ bỏ qua, không allowlist CVE.

## 4. Hai mạng, hai mức bảo đảm — đo chứ không đoán

Thử nghiệm trực tiếp trước khi thiết kế: dựng một mạng `internal: true`, chạy container có cổng
publish, thử cả hai chiều.

| | Kết quả |
| --- | --- |
| ingress từ host qua cổng publish | **không hoạt động** |
| egress từ container ra internet | **bị chặn** |

Nên không thể có cả hai trên một mạng. Cấu trúc cuối:

| Mạng | Ai ở đó | Bảo đảm |
| --- | --- | --- |
| `ivr-internal` (`internal: true`) | fake-sales, otel, lối service-to-service | **không có tuyến ra ngoài** |
| `ivr-database-local` | postgres, api, worker, ui | routable, nên no-egress của app là **ở tầng ứng dụng**, không phải tầng mạng |

Phân biệt này ghi rõ trong chính file compose. Người đọc tưởng container app bị cô lập mạng sẽ rút
ra kết luận mạnh hơn thực tế.

`IT-IMG-COMPOSE-03` kiểm nửa cấu trúc bằng cách **thử** đi ra từ `ivr-internal` và đòi thất bại,
chứ không đọc lại `internal: true` từ chính file mình vừa viết.

## 5. Compose smoke: hai chương trình đi hết stack

Gửi payload GOLDEN_HOUR/ONLINE và TWENTY_FOUR_SEVEN/COD qua API **trong container**:

| Lần | Kết quả | Nghĩa là |
| --- | --- | --- |
| payload seed nguyên bản | `422 IVR_STATE_NOT_CALLABLE` | cửa sổ xác nhận trong seed quá hạn — fail-closed đúng |
| dời cửa sổ, sửa cả id | `400 IVR_MALFORMED_REQUEST` | schema từ chối id tôi bịa — guard đúng |
| header không khớp body | `422 IVR_MISSING_TRACE` | correlation phải khớp — guard đúng |
| header khớp | **`200`** `TASK_HELD_POLICY_MISSING` | đi hết auth → allowlist → correlation → schema → luật → **ghi xuống DB** |

Ba lần từ chối đầu **không phải thất bại của kiểm chứng** — chúng cho thấy từng guard trong chuỗi
đang thật sự chạy trong container, chứ không phải một endpoint nhận mọi thứ.

Quyết định cuối là `TASK_HELD_POLICY_MISSING` vì DB dev **chưa seed attempt policy**. Fail-closed
đúng, và ghi rõ: một quyết định **dispatch được** cần policy được đăng ký, việc stack dev chưa làm.

## 6. Quét image có đối chứng dương

`IT-IMG-SCAN-04` chạy Trivy với `--exit-code 1` trên `HIGH,CRITICAL` cho cả ba image, **rồi quét một
base có lỗ hổng đã biết và đòi nó đỏ**. Một scanner không bao giờ đỏ thì không phân biệt được với
một scanner hỏng — nửa dương này làm cho ba lần xanh kia có nghĩa.

## 7. Kiểm chứng

| Kiểm tra | Kết quả |
| --- | --- |
| `IT-IMG-BUILD-01` | 3 image build; USER `1654`/`1654`/`node`; env mặc định `REAL_CUSTOMER_CALL_ALLOWED=NO`, `IVR_ADAPTER_MODE=MOCK` |
| `IT-IMG-HEALTH-02` | `/health/live` healthy **không cần database** phía sau |
| `IT-IMG-COMPOSE-03` | stack lên từ volume trắng, api healthy, mạng fake **không ra được internet** |
| `IT-IMG-SCAN-04` | 3 image 0 HIGH/CRITICAL; đối chứng dương đỏ đúng |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (mở rộng cho fragment images; kiểm âm đỏ) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

## 8. Secret không nằm trong layer

App **từ chối boot** khi thiếu `IVR_INTERNAL_SERVICE_TOKEN` — đã kiểm bằng cách chạy image không có
nó: container thoát với `OptionsValidationException`. Fail-closed đúng (D-05), không phải lỗi.
`.dockerignore` loại các file môi trường, `*.pem`, `*.key`, `*.pfx` khỏi build context.

## 9. Cái này KHÔNG chứng minh

- **Chưa có registry.** §5 đánh dấu registry `NEED_CONFIRMATION`; image mới chỉ tồn tại local. Quy
  ước tag đã ghi nhưng **chưa push lần nào**.
- **Compose smoke chưa chạy hết luồng DTMF tới callback.** Nó chứng minh intake đi hết stack trong
  container và ghi xuống DB; luồng đầy đủ do `E2E-FLOW-*` chứng minh trên Testcontainers, **không**
  qua stack compose. §8 đòi cả hai chương trình chạy qua fake Sales / mock speech+SIM / target
  callback — phần **fake Sales nhận callback** chưa được chứng minh qua compose.
- **Chưa seed attempt policy trong stack dev**, nên chưa quyết định nào đi tới trạng thái dispatch.
- **Worker không có healthcheck** (§2) — khoảng trống thật.
- **SBOM chưa sinh** (§6.6 ghi optional).
- **`mock-sim` và `mock-jwt` vẫn là placeholder** từ P0-1, cố ý: cả hai mock đều **in-process** ở
  chế độ MOCK, nên không có gì trong stack gọi tới container của chúng. Thay bằng server thật sẽ là
  thêm hai tiến trình không ai nói chuyện với, đọc như nhiều phủ hơn thực tế.
