# W-0043 — Evidence: Docker images & dev compose (`P7-1`)

Ngày: `2026-08-18` · cập nhật `2026-08-19` (thêm `IT-IMG-E2E-05`, xem §10) · Trạng thái:
`TESTS_PASS` cho **5** kiểm tra §8

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
| `IT-IMG-SBOM-06` | 3 SBOM CycloneDX (24 / 96 / 43 thành phần); mỗi cái **quét sạch cả dưới dạng SBOM lẫn dưới dạng image**; base xấu đã biết **vẫn đỏ sau vòng khứ hồi** |
| `IT-K8S-WORKER-06` | kubelet đọc được probe worker xuyên default-deny, pod khác **không**; 90s tắt DB → `0` probe hỏng, `0` restart; **2 lỗi tìm ra**, xem §12 |
| `IT-WORKER-LIVENESS-12` | 6 ca: vòng dừng bị nêu tên; grace 3 chu kỳ nhưng tối thiểu 30s; hỏng-mọi-lượt **không** phải liveness failure; registry rỗng `stalled`; tất cả tắt → `idle` vẫn qua probe; một vòng bật giữa các vòng tắt vẫn bị canh |
| `IT-IMG-E2E-05` | **3 task** đi hết stack trên **một kênh SIM**, cả hai chương trình → `IVR_CONFIRMED` / `IVR_CUSTOMER_CANCELLED` / `IVR_NO_ANSWER_FINAL`, mỗi cái `ACCEPTED` **đúng một lần**; **4 lỗi tìm ra**, xem §10–§11 |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (mở rộng cho fragment images; kiểm âm đỏ) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

## 8. Secret không nằm trong layer

App **từ chối boot** khi thiếu `IVR_INTERNAL_SERVICE_TOKEN` — đã kiểm bằng cách chạy image không có
nó: container thoát với `OptionsValidationException`. Fail-closed đúng (D-05), không phải lỗi.
`.dockerignore` loại các file môi trường, `*.pem`, `*.key`, `*.pfx` khỏi build context.

## 10. `IT-IMG-E2E-05` — và **bốn lỗi** mà bốn kiểm tra kia không thấy

`2026-08-19`. Kiểm tra mới đưa **một task** đi hết: intake → eligibility → scheduler dispatch →
mock SIM → DTMF `1` → chuẩn hoá → outbox → fake Sales.

Điều đáng nói không phải là nó xanh, mà là **stack đã không bao giờ chạy được** trong khi bốn kiểm
tra §7 vẫn xanh suốt. Bốn lỗi, xếp theo thứ tự lộ ra:

### 10.1 Image worker **không nói được tiếng Việt**

`VietnameseOrderScriptRenderer` gọi `CultureInfo.GetCultureInfo("vi-VN")` trong **static
constructor**. Base image chiseled chạy ở chế độ **globalization-invariant** — không có ICU — nên
lời gọi ném `CultureNotFoundException`, bọc thành `TypeInitializationException` ngay lần render đầu.

Chuỗi hệ quả không hề hiển nhiên: gateway dispatch bắt mọi `Exception` và ánh xạ loại lạ thành
`MOCK_DISPATCH_TECHNICAL_FAILURE` với `channelHealthy=false` → sau **3 lần**, DT-04 tự khoá kênh SIM
duy nhất. Nhìn từ ngoài, triệu chứng là "kênh hỏng", còn nguyên nhân là **thiếu một bảng locale**.

Toàn bộ test đơn vị và tích hợp đều xanh, vì chúng chạy trên máy host **có ICU**.

Sửa: dựng `NumberFormatInfo` **tường minh** (nhóm `.`, thập phân `,`) thay vì tra ICU. Hai dấu phân
cách không đáng để phụ thuộc runtime, và tự dựng còn mua được thứ ICU không cho: khách hàng nghe
**cùng một con số trên mọi máy**, chứ không phải theo phiên bản ICU của base image năm nay.
`UT-SCRIPT-VI-FORMAT-08` ghim giá trị và **đối chiếu với ICU thật ở nơi có ICU**; `UT-BOOT-05` cấm
tra culture theo tên trong toàn bộ `src/`.

### 10.2 Kho dial-token là **cục bộ theo tiến trình**

`MockDialTokenVault.Protect()` ghi fingerprint vào bộ nhớ — nhưng nó chạy ở **API** lúc intake, còn
`ResolveAsync()` chạy ở **worker** lúc dispatch. Ba deployable riêng (DTS-04) nghĩa là hai vault
khác nhau, và bên resolve **không có** nhánh `"*"` mà bên protect đã có.

Đây là **khoảng cách mô phỏng**, không phải tính chất an toàn: vault thật sẽ dùng chung. Sửa bằng
cách cho `ResolveAsync` dùng `"*"` **chỉ khi người vận hành đã cấu hình một cái** — không có
wildcard thì hành vi giữ nguyên, fingerprint lạ vẫn bị từ chối.

### 10.3 `payload_json` là `jsonb` — nên **chưa callback nào từng được gửi**

Cột dùng `jsonb`, và `jsonb` lưu **ý nghĩa** của tài liệu chứ không lưu **byte**: PostgreSQL sắp xếp
lại khoá và chuẩn hoá khoảng trắng. Payload callback thì được **niêm bằng `payload_sha256`** lúc
enqueue và kiểm lại trước khi gửi HTTP.

Hệ quả: văn bản đọc ra **không bao giờ** băm ra đúng giá trị đã lưu → `CallbackPayloadIntegrity` từ
chối → mọi callback `INVALID_DEAD_LETTER`, **không một request nào rời tiến trình**.

Nguyên nhân gốc là một quy ước quét-tất: mọi property `string` tên kết thúc bằng `Json` được map
sang `jsonb`. Mặc định đó đúng cho dữ liệu truy vấn được, và **sai** cho một giá trị mà byte của nó
là đối tượng của một hàm băm. Sửa: miễn trừ đúng cột đó (`text`) kèm lý do, cộng một migration.

`IT-DB-OUTBOX-06` từng khẳng định `leased.PayloadSha256 == callback.PayloadSha256` — **so một giá
trị với chính nó**. Nay nó **tính lại băm từ byte đã đi vòng qua database**.

### 10.4 fake Sales trả lời một **URL mà IVR không gọi**

Stub khớp `POST /v1/orders/.*/ivr-result` và trả `202` với `{response_code, accepted}`. IVR gọi
`POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks` và chờ `200` với
`{code, callback_id, correlation_id}`. **Ba sai lệch độc lập** — URL, mã trạng thái, hình dạng
thân — nên fake trả `404` cho mọi callback thật. Đo được bằng `curl` từ mạng nội bộ, không phải suy
từ việc đọc file.

Stub mới **phản chiếu** `callback_id` và `correlation_id` thay vì trả hằng số. Đó không phải chi
tiết trang trí: transport **so cả hai** với dòng outbox và báo `CALLBACK_ACK_INVALID` nếu lệch — nên
phép phản chiếu chính là thứ biến bài kiểm tra thành một **vòng khứ hồi thật**, không phải một `200`
đóng hộp.

### 10.5 Vì sao kiểm tra này khẳng định ở **cả hai đầu**

`IT-IMG-E2E-05` đòi **cả** dòng database (`IVR_CONFIRMED` + `DELIVERED_ACCEPTED`) **và** nhật ký
`__admin/requests` của WireMock. Một mình mỗi bên đều có thể xanh trên một stack hỏng: bên này là
"IVR **tin rằng** đã gửi", bên kia là "Sales **tin rằng** đã nhận". Chỉ cặp đôi mới nói rằng một
request đã thật sự vượt qua khoảng giữa — và §10.3 chính là câu chuyện của khoảng giữa đó.

`correlation_id` cũng được khẳng định là **nguyên vẹn từ intake tới Sales**.

### 10.6 Tư thế mặc định **không đổi**

Mọi job của worker ship ở trạng thái **tắt**. Đó là mặc định đúng cho một người chạy
`docker compose up` để nhìn console: không quay số, không gọi ra ngoài. Nên tư thế E2E nằm ở
`docker-compose.e2e.yml` — đọc hết trong một màn hình — chứ không sửa file dev.

Thứ **không** được nới: `IVR_ADAPTER_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`, `SIM_PROVIDER=MOCK`.
`MockSchedulerDispatchGateway.IsReady` đòi cả ba, nên stack dựng từ overlay này **không thể** chạm
tới nhà cung cấp. Đích quay số là một **tham chiếu mờ**, không phải số điện thoại — validator từ
chối bất cứ giá trị nào phân tích được thành dữ liệu điện thoại.

Seed cũng chỉ vá đúng chỗ thiếu: script đã do migration `P2_7` gieo, kênh SIM do
`MockSimChannelProvisioner` gieo, **chỉ attempt policy là không ai gieo**. File seed chèn policy và
**khẳng định** phần còn lại tồn tại — mất script thì đỏ ngay tại chỗ, kèm một câu, thay vì lộ ra sau
ba bước dưới dạng "task được nhận rồi im lặng".

## 11. Ba nhánh, không phải ba biến thể của một nhánh

`2026-08-19`, mở rộng `IT-IMG-E2E-05`. Bộ ca được **chọn**, không phải góp nhặt cho tiện. `P7-1` §8
đòi **cả hai** chương trình, và thứ Sales hành động theo là **taxonomy DT-02** — nên mỗi ca là một
nhánh dẫn tới **một chỉ thị khác nhau** cho Sales:

| Ca | Chương trình / thanh toán | Kịch bản SIM | Kết quả | Sales được bảo làm gì |
| --- | --- | --- | --- | --- |
| `CONFIRM` | `GOLDEN_HOUR` / `ONLINE` | trả lời, DTMF `1` | `IVR_CONFIRMED` | xác thực lại rồi **xác nhận** đơn |
| `CANCEL` | `TWENTY_FOUR_SEVEN` / `COD` | trả lời, DTMF `0` | `IVR_CUSTOMER_CANCELLED` | xác thực lại rồi **huỷ theo yêu cầu khách** |
| `NOANSWER` | `GOLDEN_HOUR` / `ONLINE` | đổ chuông rồi thôi | `IVR_NO_ANSWER_FINAL` | **không đổi gì**, chờ timeout |

Ca thứ ba là ca đáng công. `TargetV1CallbackTransport` **từ chối gửi** một `IVR_NO_ANSWER_FINAL`
kèm bất kỳ yêu cầu chuyển trạng thái nào — IVR không được phép làm đơn hết hạn, worker timeout của
Sales sở hữu quyết định đó — và **cho tới nay lời từ chối ấy chưa bao giờ chạy trong container**.

### Vì sao phải thêm một policy một-lượt

`IVR_NO_ANSWER` chỉ thành **FINAL** khi lượt cuối đã dùng hết (DT-02). Với `mock-lab-v1` nghĩa là
chờ hết offset lượt hai — **150 giây** với Giờ Vàng, **450 giây** với 24/7. Một smoke ngủ bảy phút
để nhìn một giá trị taxonomy sẽ bị tắt, nên thứ thay đổi là **policy**, không phải đồng hồ:
`mock-e2e-single-v1`, cùng hình dạng, cùng guard, **một lượt**.

Đáng nêu: ring-out **vẫn tính là một lượt của khách**. Đếm nó là điều làm cho "hai lượt" nghĩa là
hai cơ hội khách đã có, chứ không phải hai lần tổng đài tình cờ hoạt động.

### Kịch bản khoá theo **task id**, không phải wildcard

`FakeSimGateway` tra theo thứ tự attempt id → task id → `"*"`. Attempt id sinh lúc dispatch nên
không cấu hình viết trước nào gọi tên được nó; khoá theo **task** nghĩa là smoke chọn kết quả bằng
cách chọn task nó gửi, và bảng ánh xạ đọc được ở **một chỗ**.

Id cố định chứ không sinh ra: smoke xoá volume sau mỗi lần chạy nên dùng lại không phải va chạm —
và id cố định làm cho một lần đỏ **nói được ca nào hỏng**.

### Ba ca dùng chung **một kênh SIM**, và chạy tuần tự

Seed cố ý chỉ gieo **một** kênh. Có pool thì một lỗi lập lịch có thể nấp sau kênh dự phòng; ba ca
dùng chung một kênh còn chứng minh **lease được trả lại giữa các cuộc gọi**.

### Khẳng định "đúng một lần"

Sau khi kiểm từng ca, kiểm tra đòi tổng số callback fake Sales nhận **bằng đúng** số ca. Một lần
thử lại nhân đôi tín hiệu vẫn thoả mọi khẳng định phía trên — mà **một xác nhận trùng không phải
một xác nhận vô hại**.

Kiểm âm: đổi `expectedAction` của ca `NOANSWER` thành `CORE_REVALIDATE_AND_CONFIRM_ORDER` → đỏ, kèm
nguyên văn callback đã nhận. Nghĩa là khẳng định taxonomy **có tải trọng**, không chỉ `result_type`.

## 12. Worker healthcheck — và **hai lỗi** nó lôi ra

`2026-08-19`. §2 ghi "worker không có healthcheck — khoảng trống thật", và lý do cũ đúng vào thời
điểm đó: worker **không mở socket nào**, nên không có gì để probe.

Nhưng thứ đáng lo **không thoát tiến trình**. Mỗi job host bắt exception của chính nó rồi tiếp tục
poll — đúng, vì một lượt hỏng không được phép hạ cả tiến trình. Cái giá là một vòng lặp **hỏng mãi
mãi**, hoặc **treo bên trong một lời gọi không bao giờ trả về**, trông y hệt một vòng lặp khoẻ:
tiến trình sống, container chạy, và **không có gì được xử lý**.

### `IvrHeartbeat` cũ là một placeholder từ `P0`

Nó log một dòng mỗi 30 giây nói worker tồn tại. Trung thực khi chưa có xử lý nền nào; **gây hiểu
lầm** ngay khi có — một dòng ghi "heartbeat" đọc như "mọi thứ ổn", và nó vẫn nói thế trong lúc một
vòng lặp hỏng ở mọi lượt.

### Ba trạng thái, vì ba thứ khác nhau và chỉ **một** được sửa bằng restart

| Trạng thái | Nghĩa | Restart có giúp không |
| --- | --- | --- |
| `stalled` | một vòng lặp **lẽ ra chạy** đã dừng, hoặc **không vòng nào đăng ký** | **có** |
| `live` | mọi vòng lặp bật đang quay | — |
| `idle` | **không vòng nào được cấu hình chạy** | không |

Phân biệt `live` với `idle` **tìm ra khi chạy thật, không phải khi đọc code**: worker ship với mọi
vòng lặp **tắt**, nên bản hai-trạng-thái trả `503` mãi mãi trên một worker đang hành xử **đúng như
cấu hình** — và một liveness probe sẽ restart nó thành vòng lặp chết.

Cũng vì thế **"turning but failing" trả `200`**. Một vòng lặp hỏng vì PostgreSQL sập đang quay
đúng; restart không sửa được PostgreSQL, nó chỉ thêm một cơn bão restart vào giữa sự cố — đúng lúc
tệ nhất.

Và **registry rỗng là `stalled`, không phải `idle`**: rỗng nghĩa là không host nào chạm tới lời đăng
ký, tức dây nối bị gỡ hoặc mọi host chết trước khi khởi động. Đó là **lỗi**, không phải cấu hình.

### Endpoint: `HttpListener` thô, cổng riêng, chỉ liveness

Không kéo ASP.NET Core vào một worker để trả lời **một** câu hỏi. Trả `503` thay vì từ chối kết nối,
vì một kết nối bị từ chối và một tiến trình đã chết **trông giống nhau** với probe — khác biệt nằm ở
**thân phản hồi**.

Thân nêu tên vòng lặp, lần tick cuối và **loại** lỗi cuối. **Không bao giờ là message**: message có
thể mang connection string, giá trị một dòng dữ liệu, hoặc một số điện thoại che chưa hết — mà bất
cứ thứ gì chạm được cổng này đều đọc được.

**Không có `readinessProbe`**: không gì định tuyến lưu lượng tới worker, nên "ready" không có nghĩa
ở đây, và thêm nó chỉ tạo thêm một thứ nữa để sai.

### `IT-K8S-WORKER-06` đo hai chiều ngược nhau

Trên cluster k3s thật:

- **kubelet đọc được** cổng probe xuyên qua NetworkPolicy default-deny (lưu lượng probe đi từ node,
  không từ pod — đó là tính chất của CNI, không phải của chart, nên nó được **đo** chứ không được
  tin);
- **pod khác không đọc được** cùng cổng đó. Endpoint nói vòng nào đang chạy và hỏng thế nào;
  default-deny là thứ giữ cho nó không đọc được bởi bất cứ thứ gì rơi vào namespace.

Đo một chiều thôi thì "probe chưa từng chạy" và "cổng health là cửa mở trong namespace" đều **không
phân biệt được** với một lần xanh.

Rồi **90 giây tắt database**: `0` lần probe báo hỏng, `0` restart.

### Lỗi thứ nhất nó bắt: harness triển khai **sai thứ tự**

`helm template` xuất cả resource hook, nên `kubectl apply` một phát tạo Job và Deployment cùng lúc —
thứ tự `pre-install` của Helm **biến mất**. Worker đua với schema và **segfault**
(`relation "ivr_sim_channels" does not exist`), Kubernetes chữa bằng một lần restart, và bộ test cũ
chỉ nhìn trạng thái cuối nên không thấy. Đã tách: Job trước, chờ xong, rồi mới tới phần còn lại.

### Lỗi thứ hai: **chart không cài mới được**

Sau khi harness cài **đúng thứ tự Helm**, migrate Job không khởi động nổi:
`serviceaccount "ivr-ivr" not found`. Job là hook `pre-install`, mà ServiceAccount là resource của
release — **Helm tạo release sau hook**. Nên một `helm install` **mới tinh** treo cho tới khi hết
giờ; chỉ `helm upgrade` chạy được, vì account đã có từ lần trước.

Không ai thấy vì **chưa từng có ai cài mới**: `kubectl apply` một phát làm account tới cùng lô, và
các lần thử lại của Job sống lâu hơn khoảng trống đó.

Sửa: bỏ `serviceAccountName` khỏi Job, đặt `automountServiceAccountToken: false`. Account tồn tại để
pod **không kế thừa `default` và token của nó** — token mới là thứ quan trọng, và cách này giữ đúng
nó.

### Kiểm âm

Khẳng định về probe đo **sự kiện `Unhealthy` của kubelet**, không đo `restartCount`. Lần chạy đầu
đỏ vì một restart **không liên quan gì tới probe** (chính là segfault ở trên) — đáng tìm, đáng sửa,
nhưng không phải điều khẳng định này nói, và **một phép kiểm đỏ vì lý do khác sẽ bị đọc là nhiễu**.

## 13. SBOM — sinh ra để **được dùng**, không phải để tồn tại

`2026-08-19`. `P7-1` §6.6 ghi SBOM là **optional**, và một SBOM chỉ tồn tại thì trả lời được đúng
số không câu hỏi. Tệ hơn: một SBOM **rỗng** trả lời **"không bị ảnh hưởng"** cho mọi CVE từng được
hỏi — hình dạng nguy hiểm nhất mà một artifact bảo mật có thể mang, vì nó **trông như một giấy
chứng nhận sạch**.

Nên SBOM ở đây được kiểm bằng cách **đem ra dùng**:

1. sinh CycloneDX cho cả ba image;
2. đòi nó **liệt kê được thứ gì đó** — sàn 10 thành phần;
3. đòi nó **nói đúng tên image** nó tự nhận mô tả;
4. **đưa ngược cho scanner**: quét SBOM phải ra **cùng phán quyết** với quét image.

Bước 4 là bước bắt được "SBOM đánh rơi dữ liệu gói": cùng mức nghiêm trọng, cùng ngưỡng, **khác đầu
vào**.

### Đối chứng dương ở đúng chỗ khác với `IT-IMG-SCAN-04`

`IT-IMG-SCAN-04` đã chứng minh **scanner** đỏ trên image xấu. Ở đây đối chứng chứng minh một image
xấu **vẫn đỏ sau khi đi qua SBOM rồi quay lại** — tức bước biến đổi không âm thầm làm rơi mất chính
thứ nó tồn tại để mang.

Không có nửa này, mọi kết quả xanh phía trên đều **rỗng nghĩa**.

### Sàn 10, không phải 20

Image nhỏ nhất liệt kê **24** thành phần. Một sàn cách giá trị thật bốn đơn vị sẽ đỏ ở lần bump base
tiếp theo vì một lý do **không liên quan gì** tới điều nó đang kiểm. Câu hỏi là *"cái này có liệt kê
được gì không"*, và mười trả lời được với chỗ thở.

Kiểm âm: rút rỗng `components` của `ivr-api` → đỏ đúng câu *"An SBOM that enumerates nothing answers
'not affected' to every CVE ever asked about it"*.

### Không commit vào repo

SBOM mô tả những image **đang trôi bên dưới nó**, nên một SBOM check-in sẽ già đi thành một mô tả
đầy tự tin về thứ không còn tồn tại. Job CI **xuất bản chúng làm artifact** (`expire_in: 90 days`,
`when: always`) — đó mới là thứ trả lời được câu hỏi CVE **sáu tháng sau, về đúng image đã ship**,
mà không phải build lại; và build lại sáu tháng sau là **một image khác**.

## 9. Cái này KHÔNG chứng minh

- **Chưa có registry.** §5 đánh dấu registry `NEED_CONFIRMATION`; image mới chỉ tồn tại local. Quy
  ước tag đã ghi nhưng **chưa push lần nào**.
- ~~**Compose smoke chưa chạy hết luồng DTMF tới callback.**~~ **Đã đóng `2026-08-19`** —
  `IT-IMG-E2E-05`, xem §10 và §11. Phủ **cả hai** chương trình và **ba** nhánh DT-02 cho Sales
  ba chỉ thị khác nhau. Còn thiếu: **sai phím** (`IVR_WRONG_INPUT`), **lỗi kỹ thuật** và
  **số không hợp lệ** chưa đi qua compose — chúng có ở Testcontainers, không ở đây.
- ~~**Chưa seed attempt policy trong stack dev.**~~ **Đã đóng `2026-08-19`** —
  `deploy/docker/dev-seed/seed.sql`.
- ~~**Worker không có healthcheck** (§2).~~ **Đã đóng `2026-08-19`** — xem §12. Vẫn chưa có:
  probe cho **CronJob retention**: chế độ run-once **không đăng ký** endpoint (`Program.cs`), nên
  một pod phải-kết-thúc không giữ socket mở — nhưng cũng nghĩa là **không gì probe được nó**, và
  tín hiệu duy nhất vẫn là mã thoát. (Vòng lặp `analytics` **đã đăng ký** `2026-08-19`.)
- ~~**SBOM chưa sinh** (§6.6 ghi optional).~~ **Đã đóng `2026-08-19`** — `IT-IMG-SBOM-06`, xem §13.
- **`mock-sim` và `mock-jwt` vẫn là placeholder** từ P0-1, cố ý: cả hai mock đều **in-process** ở
  chế độ MOCK, nên không có gì trong stack gọi tới container của chúng. Thay bằng server thật sẽ là
  thêm hai tiến trình không ai nói chuyện với, đọc như nhiều phủ hơn thực tế.
