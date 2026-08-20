# W-0047 — Evidence: Secret rotation & key lifecycle (`P7-5`)

Ngày: `2026-08-19` · Trạng thái: `TESTS_PASS` — 5/5 §8 cho cơ chế, cộng 4 test cho lối auth đã nối (§9);
**drill overlap đã chạy trên HTTP thật** (§9.5)

Inventory: [`docs/secret-inventory.md`](../../secret-inventory.md) ·
Runbook: [`docs/secret-rotation-runbook.md`](../../secret-rotation-runbook.md)

## 1. Khác hai slice trước: đây là **code thật**

`W-0045` và `W-0046` giao cấu hình. Slice này giao một lớp production —
`RotatingCredentialProvider` — và năm test chạy trên chính nó. Không phải kiểm YAML.

## 2. Rotation là **overlap**, không phải phép gán

Một secret đơn trị **không thể** rotate mà không downtime: có một khoảnh khắc người gọi còn cầm giá
trị cũ trong khi phía nhận đã đợi giá trị mới, và **mọi** request trong cửa sổ đó hỏng.

Nên rotation được biểu diễn là **hai giá trị cùng được nhận, một cái được ưu tiên**.

Điểm quan trọng hơn: **cửa sổ được ép trong code** (`NotAfter`), không phải bởi người nhớ chạy nửa
sau runbook. Một rotation không ai hoàn tất sẽ để giá trị bị lộ hợp lệ **vĩnh viễn** — đúng thứ
rotation sinh ra để chặn (§11).

`SEC-ROT-01` kiểm cả hai biên: một tick **trước** khi cửa sổ đóng, giá trị cũ vẫn nhận; đúng tại
biên, nó ngừng — không cần ai làm gì.

## 3. So sánh **không** short-circuit, và đó không phải chi tiết vụn

```csharp
bool match = generation.IsValidAt(now)
    && CryptographicOperations.FixedTimeEquals(suppliedBytes, generation.SecretBytes);
accepted |= match;   // tích luỹ, không return sớm
```

Trả về sớm ở lần khớp đầu sẽ làm **thời gian phản hồi tiết lộ generation nào khớp** — và trong cửa
sổ overlap, điều đó cho kẻ tấn công biết giá trị họ đang cầm có phải cái **sắp bị thu hồi** hay
không. Đó là thông tin quyết định nên tấn công ngay hay chờ.

## 4. Emergency **không có** cửa sổ, và đánh đổi là có thật

`RotateEmergency` từ chối mọi giá trị cũ **ngay lập tức**. Một overlap ở đây sẽ giữ giá trị đã lộ
hoạt động đúng bằng khoảng thời gian kẻ tấn công cần.

Đánh đổi thật: vài request đang bay sẽ rớt. Đó là đánh đổi **đúng** — mất vài request so với để một
credential đã lộ tiếp tục sống — và runbook nói thẳng thay vì che.

## 5. Audit mô tả rotation mà không mô tả secret

Một dòng audit trích dẫn giá trị **chính là** vụ rò rỉ mà nó tồn tại để ghi lại.

Audit mang: generation, kind, **fingerprint**, timestamp. `SEC-ROT-04` render toàn bộ audit thành
chuỗi rồi khẳng định **không secret nào xuất hiện**, và fingerprint thì có.

**Và điều kiện làm fingerprint an toàn được ép, không phải được giả định.** 12 hex đầu của SHA-256
chỉ an toàn khi secret đủ entropy; với secret ngắn, fingerprint bị brute-force và **bản ghi audit
trở thành oracle cho thứ nó mô tả**. Nên provider ném lỗi nếu secret dưới 24 ký tự.

Thêm một trường hợp dễ bỏ sót: rotate sang **chính giá trị đang dùng** bị từ chối. Nó trông như một
rotation trong audit và không đổi gì — **tệ hơn không rotate**, vì hồ sơ nói phơi nhiễm đã đóng.

## 6. D-05: IVR không giữ mapping, và điều đó được **ép bằng reflection**

`SEC-ROT-05` duyệt model persistence và đỏ với bất kỳ thuộc tính nào trông như destination dạng
plaintext (`PhoneNumber`, `RawPhone`, `Msisdn`, `Destination`) hoặc như key vault (`*DialToken*Key`,
`*Vault*Secret`).

Bằng reflection chứ không bằng danh sách ai đó bảo trì: **một cột thêm vào ngày mai** đúng là trường
hợp danh sách tay bỏ sót.

**Một phân biệt phải nói rõ:** `MockDialTokenVault` **có** giữ ánh xạ giả trong bộ nhớ — nhưng nó là
**ranh giới tin cậy giả lập** đứng thay cho vault ngoài, và chỉ ghi **fingerprint** xuống storage của
IVR. Đọc nhầm nó thành "IVR giữ mapping" sẽ dẫn tới kết luận sai về D-05.

## 7. Kiểm chứng

| Test | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `SEC-ROT-01` | biến `Rotate` thành phép gán (overlap = 0) | ❌ đỏ |
| `SEC-ROT-02` | — | ✅ |
| `SEC-ROT-03` | — | ✅ |
| `SEC-ROT-04` | cho audit mang cả giá trị secret | ❌ đỏ |
| `SEC-ROT-05` | trồng cột `DestinationPhoneNumber` vào entity | ❌ đỏ |

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test --filter TestId~SEC-ROT` | **5/5** |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=266` (+9) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

## 10. `IT-K8S-ROTATE-07` — fleet, và **hai chiều ngược nhau**

`2026-08-19`, trên cluster k3s thật, api **2 replica**, dò liên tục bằng cả hai token **trong lúc**
rolling restart:

| Người gọi đang cầm | Bị từ chối trong lúc rollout |
| --- | --- |
| token **cũ** | **0/4** |
| token **mới** | **2/4** |

Đo **một** cột thôi sẽ đọc thành "rotation liền mạch" — mà nó chỉ liền mạch với **một** trong hai
người gọi.

**Cột cũ** là thứ overlap mua được: pod cũ giữ `cũ` làm current, pod mới giữ `cũ` làm previous, nên
mọi pod ở mọi trạng thái của rollout đều nhận nó.

**Cột mới** là thứ overlap **không thể** mua: một pod chưa restart **chưa từng nghe nói tới** token
mới. Không cấu hình nào sửa được, vì đó không phải vấn đề cấu hình — nó là vấn đề **thứ tự**. Và đó
chính là lý do runbook §6 xếp "rollout credential" **trước** "chuyển người gọi": đảo lại thì mọi
request hỏng suốt độ dài một lần deploy.

Khẳng định `newRejections > 0` được viết như một **kỳ vọng dương**, không phải một sự khoan dung:
một lần chạy mà token mới **không bao giờ** bị từ chối nghĩa là rollout đã xong trước khi bắt đầu
dò — và khi đó cột `0/4` kia cũng **không đo được gì**.

### 10.1 Chart trước đó **không diễn đạt nổi** rotation này

`_helpers.tpl` chỉ nối `ORDER_CORE_SERVICE_TOKEN`. Không có `TOKEN_PREVIOUS`, không có
`TOKEN_PREVIOUS_RETIRES_AT`. Cơ chế overlap tồn tại trong code (`RotatingCredentialProvider`) và
trong runbook, còn trên Kubernetes hình dạng duy nhất khả dụng là **cắt cứng** — đúng cái cửa sổ mà
provider sinh ra để xoá.

Nay hai biến là **tuỳ chọn và vắng mặt mặc định**: một previous token luôn hiện diện không phải
overlap, nó là **credential sống thứ hai**. `optional: true` đặt trên **key**, không trên
reference — secret luôn tồn tại vì nó mang token hiện hành; chỉ **key rotation** bên trong mới đến
rồi đi.

`RETIRES_AT` là một **thời điểm**, không phải một khoảng: một khoảng sẽ khởi động lại theo từng pod,
nên rotation **không bao giờ kết thúc** chừng nào còn có gì đó bị lập lịch lại — và "không bao giờ
kết thúc" chính là cách một overlap biến thành credential thứ hai vĩnh viễn.

### 10.2 Lỗi thứ hai drill lôi ra: **console không gọi được API**

Pod dò phải mang nhãn console để đi qua policy — và đó là lúc lộ ra: policy **ingress** cho phép
console → api cổng 8080, nhưng **egress** allowlist **chưa bao giờ** nhắc tới api. NetworkPolicy đòi
**cả hai đầu** đồng ý.

Console render **phía server** với `http://<release>-api:8080`, nên trên bất kỳ cluster nào thực thi
policy, **console không tải nổi một trang**.

`IT-K8S-NETPOL-04` không bắt được vì nó chỉ kiểm **egress ra Internet** — một thứ đáng lẽ **bị
chặn**. Không phép kiểm nào từng thử một chặng **đáng lẽ chạy được**, và **một bộ kiểm policy chỉ
kiểm những lần từ chối của nó thì luôn luôn xanh**.

Nay nó đo cả hai chiều: console **tới được** api, và một pod **không mang nhãn console** thì
**không** — vì nếu bất cứ thứ gì trong namespace gọi được api thì luật ingress kia chỉ là trang trí.

## 8. Cái này KHÔNG chứng minh

- **Chưa lượt rotation nào chạy trên hệ triển khai.** `SEC-ROT-01`/`-03` chứng minh **cơ chế**
  zero-downtime; mục "không request nào rớt trong cửa sổ" của runbook §5 là **`NOT_RUN`** — nó cần
  một drill trên cluster, không suy ra được từ test đơn vị.
- ~~`RotatingCredentialProvider` chưa được nối vào middleware.~~ **Đã nối** — xem §9.
- **Chưa có Vault/KMS.** `deploy/secrets/` là ExternalSecret cho hạ tầng **chưa tồn tại**
  (`W-0063`); chưa sync lần nào. Không dynamic secret, không lease-renew (§6.3).
- **Khoá ký JWT không rotate được**: `MockOidcIssuer` sinh RSA **theo tiến trình**, không persist.
  Rotation thật cần issuer thật (`W-0006`).
- **SIM gateway credential chưa tồn tại** (`W-0008`, `BLOCKED_EXTERNAL`).
- **TTL trong inventory là đề xuất**, chưa chủ sở hữu duyệt.


## 9. Đã nối vào `OrderCoreAllowlistMiddleware`

`OrderCoreCredentialSource` dựng provider từ cấu hình và middleware gọi `IsAccepted` thay cho phép
so sánh đơn trị. Bốn test mới (`SEC-ROT-06`..`-09`) chạy trên chính lối auth đó.

### 9.1 Vì sao là **hai giá trị cấu hình**, không phải reload-on-change

Token tới qua **biến môi trường**, và một tiến trình **không thể thấy env của chính nó đổi**. Nên
`IOptionsMonitor.OnChange` sẽ không bao giờ bắn ở đây — rotate-on-reload là cơ chế đúng cho cấu hình
dạng file, không phải cho env.

Điều thực sự xảy ra khi rotate là **rolling restart**, và trong lúc đó fleet có **cả pod cũ lẫn pod
mới**. Nếu pod mới chỉ nhận giá trị mới, người gọi còn cầm giá trị cũ sẽ hỏng ở đúng những pod đã
cập nhật — chính là outage mà rotation lẽ ra phải tránh. Hai giá trị cấu hình tường minh giải đúng
trạng thái đó.

### 9.2 Giá trị cũ được cài **trước** rồi mới rotate ra

`OrderCoreCredentialSource` khởi tạo provider bằng token **cũ** rồi gọi `Rotate(mới, hạn - now)` —
đúng lời gọi mà runbook mô tả, chứ không phải một nhánh code thứ hai tình cờ hành xử giống. Nhờ vậy
hạn được ép bởi chính provider.

Hệ quả: **không thể quên hoàn tất rotation.** Một `TOKEN_PREVIOUS` bỏ quên trong values file **tự
hết hiệu lực** đúng thời điểm cấu hình nói. `SEC-ROT-07` khẳng định điều đó bằng một giá trị vẫn còn
trong cấu hình nhưng đã quá hạn — và nó bị từ chối.

Và boot **từ chối** nếu đặt `TOKEN_PREVIOUS` mà thiếu `TOKEN_PREVIOUS_RETIRES_AT`: một giá trị cũ
không hạn sẽ sống tới khi ai đó nhớ xoá biến, tức rotation không bao giờ kết thúc.

### 9.3 Không cấu hình nghĩa là **từ chối**, không phải "so sánh hai chuỗi rỗng"

Khi không có token nào được cấu hình, provider để `null` chứ không được seed bằng chuỗi rỗng. Seed
rỗng sẽ làm một giá trị rỗng gửi lên **khớp** — tức xác thực request của không ai thành của mọi
người. `SEC-ROT-08` khẳng định điều đó, và kiểm âm (seed một placeholder) làm nó đỏ nhờ khẳng định
audit rỗng.

### 9.4 Kiểm chứng

| Test | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `SEC-ROT-06` | bỏ overlap, chỉ nhận token hiện tại | ❌ đỏ |
| `SEC-ROT-07` | bỏ kiểm hạn của giá trị cũ | ❌ đỏ |
| `SEC-ROT-08` | seed provider bằng placeholder thay vì `null` | ❌ đỏ |
| `SEC-ROT-09` | — (audit không mang giá trị) | ✅ |

Test đặt ở `Ivr.IntegrationTests` chứ không `Ivr.UnitTests`: project unit cố ý chỉ tham chiếu Domain
và Infrastructure — đúng tách lớp mà `ArchitectureDependencyTests` đang ép — còn các test này cần
kiểu options của tầng API.

### 9.5 Drill đã chạy — "không request nào rớt" **không còn là `NOT_RUN`**

Chạy image `ivr-api` thật trong container với `TOKEN=mới`, `TOKEN_PREVIOUS=cũ`,
`TOKEN_PREVIOUS_RETIRES_AT=<T>`, rồi bắn request **qua HTTP thật** bằng cả ba loại token, 2 giây
một lượt, xuyên qua ranh giới `T`:

```
01:27:50  old=400  new=400  unconfigured=403
   ... 21 lượt liên tiếp, old luôn được nhận ...
01:28:31  old=400  new=400  unconfigured=403     <- T = 01:28:31.96
01:28:33  old=403  new=400  unconfigured=403     <- cửa sổ đóng
   ... 7 lượt sau, old luôn bị từ chối ...
```

| Khẳng định | Kết quả |
| --- | --- |
| token cũ được nhận suốt cửa sổ | **21/21 lượt** |
| token cũ bị từ chối sau `T` | **7/7 lượt**, chuyển đúng tại `T` |
| token mới có lượt nào 403 không | **không, 0/28** |
| token chưa cấu hình | **403 ở cả 28 lượt** |

`400` là `IVR_MALFORMED_REQUEST` — và đó chính là tín hiệu cần: nó nghĩa là request **đi qua được
auth** rồi mới bị schema từ chối. Nếu auth từ chối thì đã là `403`, như cột `unconfigured` cho thấy.

Ba tính chất được chứng minh cùng lúc: overlap **giữ** token cũ sống, hạn **tự đóng** đúng thời
điểm cấu hình mà không ai can thiệp, và token mới **không rớt lượt nào** — tức rotation này
zero-downtime trên lối HTTP thật, không phải chỉ trong test đơn vị.

~~**Vẫn còn giới hạn:** drill chạy trên **một** container.~~ **Đã đóng `2026-08-19`** —
`IT-K8S-ROTATE-07`, xem §10.

### 9.6 Điều một container **không thể** trả lời

Drill §9.5 chứng minh hành vi **middleware** qua ranh giới `T`: một tiến trình, hai token cấu hình
sẵn. Thứ nó không chạm tới được là hành vi **fleet** trong lúc rolling restart, khi pod mang cấu
hình cũ và pod mang cấu hình mới **cùng đang phục vụ**. Đó mới là chỗ rotation thật sự đau, và nó
**vô hình** với mọi test có một replica.
