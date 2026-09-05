# Kế hoạch lab — 1 SIM thật + đơn hàng mock

Ngày: `2026-08-19` · Audit code cập nhật: `2026-08-20` · Trạng thái: **`CODE_AUDIT_REFRESHED / OWNER_DATA_REQUIRED`**
Phạm vi: tập con rút gọn của `P8-1` (`W-0048`), chạy với **1 kênh**, chỉ gọi tới **số của chính chủ sở hữu**.

---

## 0. Lab này trả lời câu gì

**"Hệ thống có thật sự gọi được, đọc đúng nội dung đơn, nhận đúng phím bấm, và ghi đúng kết quả không?"**

Đó là câu duy nhất. Nói trước những câu nó **không** trả lời, để không ai đọc kết quả rộng hơn thực tế:

- ❌ Không trả lời "32 eSIM chịu được tải bao nhiêu" — một kênh không suy ra được ba mươi hai kênh.
- ❌ Không trả lời "tích hợp Sales có đúng không" — đơn là **mock**, không có Sales thật ở đầu nào.
- ❌ Không trả lời "gọi khách có ổn không" — chỉ gọi số của chính anh, và `REAL_CUSTOMER_CALL_ALLOWED` vẫn là `NO`.
- ❌ Không đóng `W-0008`, `OD-V1-09`, `OD-V1-10`, `OD-V1-19`.
- ❌ Không được gọi là `LAB_REAL_SIM_VERIFIED` — nhãn đó đòi giao thức vendor và hồ sơ nghiệm thu đầy đủ theo `P8-1` §3.

Cái nó cho anh là thứ khác và có giá trị riêng: **bằng chứng đầu tiên rằng phần mềm này làm được việc nó sinh ra để làm.** Tám phase vừa qua chứng minh nó *đúng*; lab này chứng minh nó *chạy*.

### 0.1 Đính chính sau khi đọc lại code ngày 2026-08-20

Kế hoạch này mô tả topology đề xuất, **không phải topology đã được nối xong**. Audit trên `main@7195ba8c` xác nhận bốn khoảng trống phải đóng trước cuộc gọi thật:

1. `DispatchGate` có logic và test riêng nhưng `EvaluateAsync` **chưa có caller trong đường dial runtime**.
2. DI ngoài MOCK đang dùng `UnavailableSchedulerDispatchGateway`; `LAB_REAL_SIM` chưa có dispatch gateway thật.
3. `FilePlaybackTtsProvider` trong kế hoạch mới là phương án; code hiện chưa có provider phát file.
4. `CURRENT_GOLDEN_HOUR_COMPAT` của Sales và `LAB_REAL_SIM` chưa có tổ hợp runtime được validator phê duyệt. CDC Sales hiện hữu phải chạy ở lane MOCK riêng; one-SIM chạy với fake Sales; Target V1 chỉ nối sau khi Sales cung cấp producer/callback/auth thật.

Chi tiết audit và phiếu lấy đầu vào nằm tại `docs/evidence/W-0048/`. Không được dùng các câu khẳng định cũ bên dưới để tuyên bố lab đã sẵn sàng chạy.

### 0.2 Trạng thái bốn khoảng trống — đối soát code `2026-09-05` (`W-0190`)

Bốn mục ở §0.1 là ảnh chụp ngày `2026-08-20`. Ba mục đầu **đã được nối xong** kể từ đó; giữ nguyên §0.1 làm lịch sử audit, đọc bảng này để biết hiện trạng.

| §0.1 | Trạng thái hôm nay | Bằng chứng trong code |
| --- | --- | --- |
| 1. `DispatchGate` chưa có caller | ✅ **đã nối** | `AsteriskSchedulerDispatchGateway.cs:73` gọi `dispatchGate.EvaluateAsync` **trước** thao tác ARI đầu tiên |
| 2. `LAB_REAL_SIM` chưa có dispatch gateway thật | ✅ **đã có** | `AsteriskSchedulerDispatchGateway` + `AsteriskAriSimGateway` + `AsteriskLabChannelProvisioner`; overlay `docker-compose.softphone.yml` |
| 3. Chưa có provider phát file | ✅ **đã có** | `Speech/StaticFileTtsProvider.cs`; ba giọng miền cấu hình qua `Ivr__Speech__Tts__RegionalVoices__*` |
| 4. Chưa có tổ hợp `CURRENT_GOLDEN_HOUR_COMPAT` + `LAB_REAL_SIM` được duyệt | ⛔ **vẫn đúng** | one-SIM vẫn chạy với fake Sales; Target V1 chỉ nối sau khi có producer/callback/auth thật |

Nói chính xác: **phần mềm đã sẵn sàng cho lab; cái còn thiếu là SIM và thiết bị**, chứ không còn là bốn chỗ chưa nối. Điều kiện chặn thật sự nằm ở `OD-V1-09`, `OD-V1-19`, `OD-V1-20` và việc mua phần cứng.

---

## 1. Đã có sẵn — không phải làm lại

Tôi đã đọc code để xác nhận, không dựa vào trí nhớ:

| Thứ cần cho lab | Trạng thái | Ở đâu |
| --- | --- | --- |
| Chế độ `LAB_REAL_SIM` | ✅ có, có validator riêng | `IvrOptions.cs:14`, `FeatureFlagGuardrails.cs:75` |
| Cổng chặn quay số | ⚠️ logic/test có, **chưa nối đường dial runtime** | `DispatchGate.cs` — kill switch → mode → allowlist → release gate |
| Allowlist đích lab | ✅ có trong snapshot cờ | `snapshot.LabDestinationAllowlist` |
| Đăng ký kênh SIM + lease/fencing | ✅ có bảng + advisory lock | `PostgresTelephonyDispatchStore.cs` |
| Bật/tắt kênh từ console | ✅ có API + màn hình | `IvrAdminEndpoints.cs:50-52` |
| Cổng `ISimGateway` (6 phương thức) | ✅ **đã định nghĩa** | `ProviderPorts.cs:204` |
| Cổng `ITtsProvider` + cache audio | ⚠️ port/cache có; file provider chưa có, external provider vẫn fail-closed | `Speech/ITtsProvider.cs`, `AudioCache.cs` |
| Đơn hàng mock | ✅ fake Sales + seed mẫu | `docker-compose.dev.yml`, `seed/*.sample.json` |
| Nhận task | ✅ | `POST /v1/ivr/order-confirmation/tasks` |
| Chính sách số lần gọi | ✅ dùng bản ứng viên `mock-lab-v1` | `OD-V1-08` cho phép ở MOCK/LAB |

Nói chính xác: **các primitive quyết định có được gọi không đã có logic và test, nhưng chưa được nối hết vào đường quay số thật.** Phải nối gate + resolver + dispatch gateway + speech provider rồi mới được chạy lab.

---

## 2. Ba thứ đang chặn — và lối vượt cho lab

### 2.1 Chưa có adapter SIM thật

`ISimGateway` có **đúng một** hiện thực: `MockTelephonyDispatchGateway`. Không có gì nói chuyện được với thiết bị.

→ **Tôi viết adapter.** Đây là khối việc code chính.

### 2.2 Chưa có nguồn audio

`ConfigurableExternalTtsProvider` là một **chỗ trống có chủ ý** — nó ném `TTS_NOT_CONFIGURED` chứ không tổng hợp gì:

```
throw new TtsProviderNotConfiguredException(
    "No external TTS vendor adapter is available until OD-V1-19 is approved and P8-1 is implemented.");
```

→ **Lối vượt cho lab: file audio dựng sẵn.** Đơn là mock nên nội dung do ta kiểm soát hoàn toàn — chỉ cần một bộ nhỏ file wav. Không cần chọn vendor, không cần mở `OD-V1-19`, và **không có nội dung đơn nào rời khỏi mạng nội bộ** (điểm này quan trọng với PDPA về sau).

### 2.3 IVR **không được phép** biết số điện thoại ← điều bất ngờ nhất

Đây là thứ quyết định toàn bộ topology, và tôi chỉ thấy khi đọc code:

```
OpaqueReferenceGuard.EnsureNotRawPhone(providerDestinationReference);
```

`DialAuthorization` — vật mà adapter nhận để quay số — **ném lỗi** nếu giá trị trông giống số điện thoại Việt Nam (10–12 chữ số bắt đầu bằng `0` hoặc `84`). Đây là D-05 được ép ở tầng kiểu dữ liệu, không phải một quy ước.

**Hệ quả: IVR về mặt cấu trúc không thể quay một số.** Nó chỉ chuyển đi được một *tham chiếu*.

→ **Lối vượt: đặt bảng tra `tham chiếu → số thật` ở tổng đài, ngoài IVR.** IVR gửi `LABDEST-01`; Asterisk tra dialplan và quay số thật. Số điện thoại của anh **không bao giờ** đi vào database, log, evidence hay git của IVR.

Đây đồng thời là câu trả lời thực dụng cho `OD-V1-18` (ranh giới tin cậy đặt ở đâu) — ở mức lab, và tôi ghi rõ nó là **lựa chọn lab**, chưa phải quyết định kiến trúc production.

---

## 3. Kiến trúc lab

```
   ┌──────────────┐   đơn mock     ┌──────────────┐
   │  fake Sales  │ ─────────────► │   Ivr.Api    │
   │  (đã có)     │  POST /tasks   │   (đã có)    │
   └──────────────┘                └──────┬───────┘
                                          │ ghi DB
                                   ┌──────▼───────┐
                                   │  PostgreSQL  │
                                   └──────┬───────┘
                                          │ lease kênh
                                   ┌──────▼───────┐
                                   │  Ivr.Worker  │
                                   │   (đã có)    │
                                   └──────┬───────┘
                                          │ ISimGateway  ← TÔI VIẾT
                                          │ (ARI: REST + WebSocket)
                                   ┌──────▼───────┐
                                   │   Asterisk   │  ← bảng tra ref→số
                                   └──────┬───────┘     nằm Ở ĐÂY
                                          │ SIP
                                   ┌──────▼───────┐
                                   │ GSM gateway  │  ← SIM của anh
                                   │   + SIM      │
                                   └──────┬───────┘
                                          │ sóng di động
                                   ┌──────▼───────┐
                                   │ máy của anh  │  ← bấm phím 1 / 0
                                   └──────────────┘
```

**Vì sao Asterisk chứ không nói SIP thẳng từ .NET:** SIP thì .NET làm được, nhưng RTP, codec, jitter buffer và DTMF thì không — đó là vài tháng việc để làm sai. Asterisk giải quyết sẵn, và ARI (REST + WebSocket, JSON) cho đúng thứ `ISimGateway` cần: quay số, phát audio, bắt sự kiện DTMF, cúp máy, và biết lý do cuộc gọi kết thúc.

**Và Asterisk cho ta một món quà:** đổi từ SIP trunk sang GSM gateway chỉ là **sửa dialplan** — không đụng một dòng code .NET nào. Đó là lý do tôi đề xuất thứ tự ở §5.

---

## 4. Anh cần chuẩn bị gì

### 4.1 Thiết bị gọi ra — ba lựa chọn

| | Cách | Giá tham khảo | Ưu | Nhược |
| --- | --- | --- | --- | --- |
| **A** | **GSM gateway 1 cổng** (GoIP-1, Dinstar UC2000 loại 1 cổng, hoặc tương đương) | ~1–4 triệu | Đúng nghĩa "SIM thật"; DTMF chuẩn; là lối sau này chạy production | Phải mua, phải cấu hình SIP |
| **B** | **USB dongle 3G + Asterisk chan_dongle** | ~200–500 nghìn | Rẻ nhất | **Kén dữ dội** — xem cảnh báo dưới |
| **C** | **SIP trunk của nhà cung cấp VoIP trong nước** | trả theo phút, không phần cứng | Nhanh nhất, chất lượng tốt nhất | **Không phải SIM** — không kiểm được lối đi qua SIM |

**Cảnh báo về lựa chọn B.** Phần lớn USB dongle bán hiện nay là **data-only** — đã bị khoá thoại ở firmware. Chỉ vài model cũ (dòng Huawei E17x/E1750 đời đầu) còn kênh thoại, và phải đúng bản firmware. Nếu mua nhầm thì không có triệu chứng rõ ràng: máy nhận thiết bị, gửi SMS được, nhưng gọi thì im lặng. Tôi nêu B để anh biết nó tồn tại, **không đề xuất** — rủi ro là anh mất một tuần debug phần cứng thay vì kiểm phần mềm.

**Đề xuất của tôi: làm C trước, rồi A.** Không phải để né việc, mà vì nó tách hai loại rủi ro ra: nếu dựng A ngay và cuộc gọi không lên, anh không biết lỗi ở phần mềm tôi viết hay ở cấu hình gateway. Chạy C trước xác nhận phần mềm đúng; sau đó cắm A vào chỉ còn **một** biến số mới. Và như §3 đã nói, chuyển C→A là sửa dialplan, không sửa code.

Nếu anh muốn đi thẳng A cũng được — tôi sẽ dựng luôn, chỉ là khi hỏng thì mất thời gian khoanh vùng hơn.

### 4.2 SIM

- **1 SIM trả trước là đủ.** Không cần thuê bao trả sau, không cần đăng ký gì đặc biệt.
- Cần: **còn tiền, gọi ra được, và chưa bị nhà mạng chặn gọi ra.**
- **Không dùng SIM cá nhân đang xài hằng ngày.** Lý do không phải kỹ thuật: khi test lỗi, hệ thống có thể quay lặp, và nhà mạng có thể xem đó là hành vi bất thường mà tạm khoá chiều gọi ra. Mất một SIM test thì không sao, mất số cá nhân thì phiền.
- eSIM dùng được **nếu** thiết bị nhận eSIM. Đa số GSM gateway giá rẻ chỉ có khe SIM vật lý — kiểm tra trước khi mua.

### 4.3 Số đích để gọi tới

- 1–2 số của chính anh (máy cầm tay, hoặc máy bàn).
- ⚠️ **Đừng gửi số đó cho tôi trong chat, và đừng đưa vào repo.** Nó vào **đúng hai chỗ**, cả hai đều là file cục bộ không commit:
  - dialplan của Asterisk trên máy lab
  - một file `.env` cục bộ đã nằm trong `.gitignore`

  Tôi sẽ chuẩn bị file mẫu với chỗ trống để anh tự điền. Trong toàn bộ hệ thống, IVR chỉ thấy `LABDEST-01`.

### 4.4 Máy chạy lab

- Một máy chạy Docker. Windows + Docker Desktop (WSL2) là đủ; Linux thì mượt hơn.
- RAM ≥ 8GB (Postgres + API + Worker + Asterisk + console).
- **Nếu dùng lựa chọn A:** máy và GSM gateway phải **cùng mạng LAN**, và anh cần biết địa chỉ IP của gateway.

### 4.5 Nội dung audio

Tôi cần **một trong hai**:

- **Cách 1 (đề xuất, không tốn gì):** anh tự thu bằng điện thoại, mỗi câu một file, đọc rõ và chậm. Khoảng 6–8 câu:
  chào → đọc tên món → đọc số lượng → đọc tổng tiền → mời bấm 1 để xác nhận, 0 để huỷ → xác nhận đã nhận → cảm ơn/tạm biệt → câu báo bấm sai phím.
  Tôi sẽ gửi anh danh sách câu chính xác cần thu.
- **Cách 2:** anh đưa khoá API của một dịch vụ TTS tiếng Việt (FPT.AI, Viettel, Google, Azure…). Tôi dựng adapter gọi nó.

Cách 1 nghe thô hơn nhưng **kiểm đúng thứ lab này cần kiểm**: audio có phát ra loa được không, người nghe có nghe rõ không, DTMF có bắt được trong lúc đang phát không. Chất lượng giọng là việc của `OD-V1-19` sau này, không phải của lab này.

### 4.6 Bốn điều cần anh xác nhận (không tốn tiền, chỉ cần một câu)

| | Điều cần chốt | Đề xuất của tôi |
| --- | --- | --- |
| 1 | Bảng tra `tham chiếu → số thật` đặt ở Asterisk, **ngoài** IVR | đồng ý — giữ đúng D-05, và là lối duy nhất không phải sửa `OpaqueReferenceGuard` |
| 2 | Lab dùng audio dựng sẵn, **chưa** chọn vendor TTS | đồng ý — `OD-V1-19` vẫn mở, không tự đóng |
| 3 | Allowlist **chỉ** chứa số của chính anh; đổi allowlist phải qua config + khởi động lại, **không** qua API | đồng ý — đổi bằng deploy an toàn hơn đổi bằng một lời gọi API. Từ 2026-08-22 `Admin` có permission gọi API đó (`OD-V1-20`), nhưng `PendingRuntimeGateAuthorization` vẫn chặn (`409`), nên lối API thực tế **vẫn đóng**. Điều 3 giữ nguyên. |
| 4 | `REAL_CUSTOMER_CALL_ALLOWED` giữ `NO` suốt lab | bắt buộc — không có nó thì đây không còn là lab |

Điều 3 đáng nói thêm: endpoint đổi cờ (`FeatureFlagEndpoint.cs`) đòi quyền `RuntimeGateAdmin`. **Cập nhật 2026-08-22:** `OD-V1-20` đã duyệt và role `Admin` nay giữ quyền đó, nên câu cũ — *chưa vai trò nào được cấp* — không còn đúng. Nhưng kết luận thì không đổi: `FeatureFlagAdminService` kiểm `IRuntimeGateAuthorization` trước, bản production luôn trả `false`, nên lời gọi API vẫn hỏng — chỉ là hỏng bằng `409 IVR_OPERATIONAL_BLOCKED` thay vì `403`. Nạp allowlist qua config lúc khởi động vẫn là lối đi được của lab, và `OD-V1-20` vẫn **chưa** đủ để mở đường API.

---

## 5. Hướng dẫn cho anh — từng bước

### Bước 1 — Quyết định lối đi (5 phút)

Trả lời §4.6, và chọn A / B / C ở §4.1. Chỉ cần nhắn cho tôi, không cần viết gì.

### Bước 2 — Nếu chọn C (SIP trunk), làm trước để chạy nhanh

Liên hệ một nhà cung cấp VoIP trong nước, hỏi đúng ba thứ:

1. "Tôi cần **SIP trunk gọi ra** cho môi trường thử nghiệm, một kênh đồng thời."
2. "Cho tôi **host, cổng, tài khoản, mật khẩu** để đăng ký từ Asterisk."
3. "Trunk có hỗ trợ **DTMF RFC2833** không?"

Câu 3 là câu quan trọng nhất và hay bị bỏ qua. Nếu trunk chỉ hỗ trợ DTMF inband thì phím bấm có thể trượt, và anh sẽ tưởng phần mềm sai.

### Bước 3 — Nếu chọn A (GSM gateway), khi mua hỏi đúng bốn câu

1. "Thiết bị có **đăng ký SIP** lên tổng đài của tôi được không?" *(phải là **có**; loại chỉ chạy chế độ riêng của hãng thì không dùng được)*
2. "Có hỗ trợ **DTMF RFC2833** không?"
3. "Có khe **SIM vật lý** không, hay chỉ eSIM?" *(khớp với thứ anh có)*
4. "Cho tôi **tài liệu cấu hình SIP** và **mật khẩu quản trị mặc định**."

Không mua thiết bị mà người bán không trả lời được câu 1.

### Bước 4 — Thu audio (30 phút)

Chờ tôi gửi danh sách câu. Thu bằng ứng dụng ghi âm bất kỳ, mỗi câu một file, đặt tên theo danh sách. Định dạng gì cũng được — tôi chuyển đổi.

### Bước 5 — Điền hai file cục bộ

Tôi tạo sẵn `deploy/lab/.env.example` và `deploy/lab/asterisk/extensions.conf.example` với chỗ trống có chú thích. Anh chép ra bản thật, điền số của anh và thông tin trunk/gateway. Cả hai bản thật đều đã nằm trong `.gitignore`.

### Bước 6 — Chạy

Tôi cung cấp một lệnh duy nhất. Sau đó anh mở console, bấm nút tạo đơn mock, và **điện thoại của anh sẽ đổ chuông**.

---

## 6. Việc tôi làm

| Lát | Nội dung | Ước lượng |
| --- | --- | --- |
| **L1** | `FilePlaybackTtsProvider` sau `ITtsProvider` — đọc audio dựng sẵn, đi qua `AudioCache` và `SpeechPrivacyGuard` sẵn có | nhỏ |
| **L2** | **`AsteriskAriSimGateway` sau `ISimGateway`** — dial / play / bắt DTMF / cúp máy / disposition / health, qua ARI. Ánh xạ mã kết thúc của Asterisk sang bảng disposition; mã lạ → `TECHNICAL`, **không đoán thành no-answer** (`P8-1` §2.5) | **lớn nhất** |
| **L3** | Hồ sơ cấu hình lab + Asterisk vào compose; nạp allowlist từ config lúc khởi động | vừa |
| **L4** | Đơn mock đầu-cuối: seed → fake Sales → intake → lịch → quay số; bảng tra token→`LABDEST-01` | vừa |
| **L5** | Bộ nghiệm thu 8 kịch bản §7 + hồ sơ evidence + cập nhật tracker (`W-0048` tập con) | vừa |

**L2 là chỗ rủi ro thật**, và tôi nói trước một điều: **bắt DTMF trong lúc đang phát audio** là chỗ hay sai nhất trong mọi hệ IVR. Người ta thường bấm phím **trước khi** câu mời bấm nói xong. Nếu adapter chỉ bắt phím sau khi phát xong thì nó sẽ *hoạt động* trong test của tôi và *hỏng* với người thật. Tôi sẽ làm phần bắt phím chạy song song với phát audio và cắt audio khi có phím — và kịch bản `LAB-05` ở dưới tồn tại **chỉ để ép điều đó**.

---

## 7. Kịch bản nghiệm thu

Mỗi kịch bản anh phải **cầm máy và làm đúng một việc**. Không kịch bản nào tự động được — đó là điểm của lab.

| Mã | Anh làm gì | Phải thấy gì |
| --- | --- | --- |
| `LAB-01` | Nghe máy, bấm **1** | Kết quả `CONFIRMED`; console hiện đúng; fake Sales nhận callback |
| `LAB-02` | Nghe máy, bấm **0** | Kết quả `CANCELLED` |
| `LAB-03` | Nghe máy, **không bấm gì** | `NO_INPUT` sau khi hết giờ chờ; không treo kênh |
| `LAB-04` | Nghe máy, bấm **5** | `WRONG_INPUT`; nghe câu báo sai phím |
| `LAB-05` | Bấm **1 khi câu mời còn đang nói** | `CONFIRMED`, audio **cắt ngay**; không mất phím |
| `LAB-06` | **Không nghe máy** cho tới khi tự tắt | `NO_ANSWER`; lên lịch lượt sau đúng chính sách |
| `LAB-07` | **Từ chối cuộc gọi** | `BUSY`/`REJECTED` — **không** bị ghi nhầm thành `NO_ANSWER` |
| `LAB-08` | Trong lúc đang gọi, bật **kill switch** | Cuộc đang chạy kết thúc sạch; **không cuộc mới nào** được quay |

Thêm ba phép kiểm tự động chạy sau mỗi lượt:

- **Không có số điện thoại ở đâu trong IVR** — quét database, log và evidence bằng chính `PiiGuard`. Đây là bằng chứng mạnh nhất mà lab này tạo ra, và nó chỉ có giá trị nếu chạy trên hệ **đã thật sự gọi**.
- **Một kênh, một cuộc** — không có hai cuộc chồng nhau trên cùng SIM.
- **Không gọi ngoài allowlist** — thử một tham chiếu không có trong allowlist và đòi bị từ chối.

---

## 8. An toàn

- Allowlist **chỉ** số của anh. Một tham chiếu không có trong đó bị `DispatchGate` chặn với lý do `DESTINATION_NOT_ALLOWLISTED`, trước khi chạm thiết bị.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` suốt lab.
- **Không ghi âm.** Không bật, không thêm cấu hình cho nó.
- Số điện thoại chỉ tồn tại ở dialplan Asterisk và file `.env` cục bộ — **không vào git**, không vào database IVR, không vào evidence.
- Đơn hàng là **mock**: không có tên, địa chỉ hay số của người thật ở bất kỳ đâu.
- Kill switch kiểm **trước** khi chạy `LAB-01`, không phải sau. Một nút dừng khẩn chưa ai thử là một nút chưa biết có chạy không.

---

## 9. Thời gian

| Việc | Ai | Bao lâu |
| --- | --- | --- |
| Trả lời §4.6 + chọn lối đi | anh | 5 phút |
| Đăng ký SIP trunk (lối C) | anh | 1–2 ngày |
| Mua GSM gateway (lối A) | anh | 2–7 ngày |
| Thu audio | anh | 30 phút |
| L1 + L3 + L4 | tôi | có thể làm **ngay**, không chờ phần cứng |
| L2 (adapter ARI) | tôi | phần lớn làm được trước; hiệu chỉnh khi có thiết bị |
| L5 + chạy 8 kịch bản | cùng làm | 1 buổi |

**Tôi bắt đầu được ngay hôm nay** với L1/L3/L4 và phần lớn L2, dựng Asterisk cục bộ và tự gọi vào chính nó để kiểm luồng. Khi thiết bị của anh về thì chỉ còn đổi dialplan.

---

## 10. Điều này KHÔNG chứng minh

Nhắc lại ở cuối vì đây là chỗ dễ đọc rộng ra nhất:

- Một SIM chạy được **không** nói gì về 32 eSIM. Không suy ra thông lượng, không suy ra chuyển đổi dự phòng, không suy ra chi phí.
- Đơn mock chạy được **không** nói gì về tích hợp Sales thật (`W-0002`, `W-0005`, `W-0006` vẫn `BLOCKED_EXTERNAL`).
- Gọi tới số của chính mình **không** nói gì về gọi khách. Chưa có kịch bản đã duyệt pháp lý, chưa có `OD-V1-11`, chưa có DF-03.
- Lab này **không đóng** `W-0008` và không cấp nhãn `LAB_REAL_SIM_VERIFIED` — nhãn đó cần giao thức vendor và hồ sơ nghiệm thu đầy đủ theo `P8-1` §3.
- Audio dựng sẵn **không** là quyết định TTS. `OD-V1-19` vẫn mở.

Cái nó chứng minh, và chứng minh chắc chắn: **phần mềm này quay được số thật, nói được, nghe được phím, và ghi đúng thứ đã xảy ra.** Chưa có gì trong tám phase vừa rồi chứng minh được điều đó.
