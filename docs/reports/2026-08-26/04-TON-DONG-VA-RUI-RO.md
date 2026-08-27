# Tồn đọng và rủi ro — còn gì chưa xong

**Ngày:** 2026-08-26 · **Baseline:** `main@bdde72c`

> Bảng này phân loại theo **ai gỡ được**, không theo độ khó. Vì câu hỏi thực sự cần trả lời khi
> đọc một danh sách tồn đọng không phải "còn bao nhiêu việc" mà là **"việc này chờ ai"**.

---

## 0. Bốn nhóm

| Nhóm | Nghĩa | Số hạng mục | Ai gỡ được |
| --- | --- | ---: | --- |
| **A** | Thiếu thật, **IVR tự làm được ngay** | 3 | Dev IVR |
| **B** | Thiếu vì **cổng ngoài** chưa đóng | 11 cổng / 15 work item | Sales · Infra · Legal · Security · Release |
| **C** | Thiếu vì **chỉ owner làm được** | 4 | Owner Module 8 |
| **D** | **Cố ý hoãn** — có quyết định, không phải lỗ hổng | 7 | — |

Nhóm A của kế hoạch 22/08 (A1–A10) **đã đóng hết**. Nhóm A dưới đây là danh sách **mới**, tính đến
26/08.

---

## 1. Nhóm A — IVR tự làm được ngay

### A-01 · Endpoint TTS thật cho 3 đoạn biến thiên 🔴 CAO

**Hiện trạng.** [`ConfigurableExternalTtsProvider`](../../../src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs)
đã nói HTTP thật, vendor-neutral, trả PCM. Nhưng **chưa có endpoint nào được cấu hình**, vì chưa
mua gói (`OD-VOICE-01`).

**Vì sao là tồn đọng.** 4 đoạn cố định (203/266 ký tự) chỉ cần thu một lần. Nhưng 3 đoạn biến
thiên — **danh sách món, tổng tiền, vùng giao** — là phần khiến khách nghe đúng **đơn của chính
mình**. Không có provider, `Segmentation.Enabled` phải giữ `false`, và cuộc gọi lại quay về phát
một file chung.

**Việc phải làm.** Cấu hình endpoint + credential từ secret provider · viết test đo cache hit trên
dữ liệu thật (hiện chỉ đo trên fixture) · bật `Segmentation.Enabled=true` ở lab.

**Chặn bởi:** owner mua gói ElevenLabs Starter `$6` (`OD-VOICE-01`).

### A-02 · Đo tỉ lệ trúng cache trên dữ liệu thật 🟡 TRUNG BÌNH

Con số "đơn thứ hai chỉ tốn 2 lần gọi provider thay vì 7" (`UT-SEG-CACHESHARE-06`) là **trên
fixture**. Tỉ lệ trúng thật phụ thuộc phân bố phường/xã và các nhóm SKU thực tế. Chi phí vendor
được tính dựa trên giả định này — cần đo lại khi có đơn thật.

### A-03 · `AudioCacheKey` không gồm hash của text đã render 🟢 THẤP

`AudioCacheKey` dùng `summaryHash`, **không** gồm hash của text đã render. Sau khi đổi cách đọc số,
audio cũ trong cache vẫn được phục vụ. TTL ≤ 900s và bị chặn thêm bởi confirmation window ⇒
tự lành trong vòng 15 phút sau deploy. Hiện chỉ MOCK/LAB.

Ghi lại để không bị bất ngờ khi lên production, **không** cần sửa ngay.

---

## 2. Nhóm B — chặn bởi bên ngoài (15 work item, 11 cổng)

### 2.1 · Bảng cổng

| Cổng | Chặn work item nào | Chủ sở hữu | IVR đã chuẩn bị sẵn |
| --- | --- | --- | --- |
| `G-CONTRACT` | W-0002, W-0029 | Sales API/Core | fake provider + WireMock + CDC |
| `G-SPEECH` | W-0003 | Sales/Product/Privacy | DTO + validator + renderer |
| `G-DIAL` | W-0004 | Sales/Security/Telephony | resolver port + mock vault |
| `G-AUTH` | W-0006, W-0032 | Security/Platform | mock JWT + negative test |
| `G-POLICY` | W-0007 | Product/Core | policy registry versioned |
| `G-LAB-SIM` | W-0008, **W-0048**, **W-0049** | Infra + vendor | chuỗi lab đã xong |
| `G-ESIM32` | W-0008 | Infra/procurement | capacity simulator |
| `G-LEGAL` | W-0009 | Legal/Privacy | `W-0109` đã tạo lối thi hành |
| `G-RELEASE` | **W-0050**, **W-0051**, W-0009 | Release owner | evidence pack đã nộp |
| `G-GITLAB` | **W-0061** | Platform/Infra | mọi control khác hosted-PASS |
| `G-PLATFORM` | **W-0063**, W-0056 | Platform/Infra | docker-compose local |

### 2.2 · Ba chặn cứng nhất, xếp theo mức độ

**① Module 3 chưa có endpoint callback generic.**
Chương trình **24/7 hiện không có lối trả kết quả nào cả**. Đây không phải "chưa tối ưu" — nó là
một nửa hệ thống không tồn tại. Chi tiết ở [03-CAN-GI-TU-MODULE-3.md §4](03-CAN-GI-TU-MODULE-3.md#4-việc-b--module-3-mở-endpoint-nhận-kết-quả).

**② Không có sandbox credential.**
Không credential ⇒ **không chạy được một test tích hợp thật nào**. Mọi con số "test xanh" hiện tại
đều là với fake provider. Đây là lý do nấc 3 (`REAL_SALES_INTEGRATION_VERIFIED`) không thể bắt đầu.

**③ Ma trận `program × payment` mâu thuẫn giữa hai nguồn.**
`DS-01` (đọc từ source Sales) nói COD-only. Target V1 nói `GOLDEN_HOUR+ONLINE` cũng callable.
IVR đang enforce phương án thứ hai ở 4 tầng. Sai = **100% task bị từ chối, im lặng, không alert**.

### 2.3 · Bốn khoảng trống của lab một SIM (`W-0048`)

Audit trên `main@7195ba8c` (2026-08-20) nêu 4 khoảng trống. **Ba trong bốn đã được đóng** bởi
W-0104:

| # | Khoảng trống | Trạng thái 26/08 |
| --- | --- | --- |
| 1 | `DispatchGate.EvaluateAsync` chưa có caller trong luồng dial runtime | ✅ đóng — `AsteriskSchedulerDispatchGateway:73` gọi trước mọi thao tác ARI |
| 2 | DI ngoài MOCK dùng `UnavailableSchedulerDispatchGateway` | ✅ đóng — `SchedulerCapacity.cs:528` đăng ký `AsteriskSchedulerDispatchGateway` cho lab |
| 3 | Chưa có provider phát file | ✅ đóng — `StaticFileTtsProvider` |
| 4 | `CURRENT_GOLDEN_HOUR_COMPAT` + `LAB_REAL_SIM` chưa có kết hợp runtime được validator duyệt | 🟡 **còn mở** — one-SIM phải chạy với fake Sales; Target V1 chỉ nối sau khi Sales có producer/callback/auth thật |

**Còn thiếu duy nhất về mặt phần cứng:** `ISimGateway` có **hai** hiện thực
(`MockTelephonyDispatchGateway`, `AsteriskAriSimGateway`), nhưng **không có cái nào nói chuyện được
với SIM/carrier thật**. Cần vendor cung cấp protocol/SDK (`OD-V1-09`).

### 2.4 · GitLab (`G-GITLAB`) — trạng thái chính xác

| Đã có | Còn thiếu |
| --- | --- |
| `main` protected, `Allowed to push and merge = No one`, force-push off, `Pipelines must succeed` on | **Required independent MR approval** — cần nâng Premium/Ultimate + mời reviewer thứ hai |
| pipeline hosted đã từng chạy PASS (12 job / 98 test / Pages) | fresh rejection probe `NOT_RUN` |
| Registry job PASS | project chỉ có **một Owner** |

⚠️ **Đáng chú ý:** `remote.origin.pushurl` hiện trỏ **GitHub** trong khi `remote.origin.url` fetch
từ **GitLab**. Hệ quả: **mọi gate CI hiện chỉ chạy local**, không có bằng chứng hosted cho các
work item gần đây (W-0104 → W-0118).

> **Cập nhật `2026-08-27` (`W-0121`).** Lối đẩy đã sửa: `remote.origin.pushurl` nay có hai giá
> trị, GitLab trước rồi GitHub, nên một `git push origin main` chạm cả hai. Đo lúc phát hiện:
> GitLab `main` đứng ở `8cd106c` còn local/GitHub ở `f4f4734` — **GitLab thiếu 3 commit**.
> Điều này định lại bản chất của rủi ro: pipeline chưa bao giờ hỏng, nó chưa bao giờ **có gì để
> chạy**. Verdict `NOT_RUN` **giữ nguyên** cho tới khi một lượt push thật đi qua. 
>
> Về dòng `Allowed to push and merge: No one` trong bảng ngay trên: cách đọc đó **đã cũ**. Một
> lượt push fast-forward thẳng vào GitLab `main` đã **thành công `2026-08-25`** tại commit
> `bdde72c`. Thứ còn lại chưa kiểm được từ máy là runner `#55115499` có online không — mọi job
> kế thừa `tags: [ginsengfood-docker]`, thiếu runner thì pipeline sinh ra rồi treo `pending`.

---

## 3. Nhóm C — chỉ owner làm được (4 hạng mục)

| # | Việc | Chặn cái gì | Chi phí |
| --- | --- | --- | --- |
| **C-1** | **Nghe và ký 3 giọng** (`OD-VOICE-05`) | trần trạng thái W-0106 là `TESTS_PASS`, không lên được `ACCEPTED` | thời gian |
| **C-2** | **Mua gói ElevenLabs Starter** (`OD-VOICE-01`) | A-01, và 12 file MP3 đoạn cố định | **`$6`/tháng** |
| **C-3** | **Render 12 MP3** đoạn cố định (4 câu × 3 miền) | toàn bộ buổi nghiệm thu "khách nghe đúng đơn của mình" | cần phiên đăng nhập ElevenLabs |
| **C-4** | **Gọi 6 lượt MicroSIP × 3 miền và NGHE** | tiêu chí nghiệm thu thứ 5 của A1 (W-0108) | 30 phút |

> **Bốn việc này là chặng găng ngắn nhất còn lại mà không phụ thuộc bên ngoài.** Tổng chi phí tiền
> mặt là `$6`. Chúng chặn đúng thứ quan trọng nhất: bằng chứng rằng **khách nghe đúng đơn của
> chính mình**, chứ không phải chỉ nghe được một bản thu chung.

**Rủi ro pháp lý cần đọc kèm C-2** (`R17`/`R18` trong plan W-0106):
- Free tier ElevenLabs **không có commercial license** — audio audition **không** được dùng cho
  cuộc gọi thật. Phải sinh lại bản production sau khi mua Starter.
- Mua Starter một tháng rồi huỷ — **chưa rõ license còn hiệu lực với audio đã sinh hay không**.
  Phải đọc và trích dẫn ToS trước khi huỷ. Nếu không xác nhận được ⇒ duy trì gói trả phí.

---

## 4. Nhóm D — cố ý hoãn, không phải lỗ hổng

| Mục | Trạng thái | Căn cứ |
| --- | --- | --- |
| Gửi SMS/notification | `DEFERRED_TARGET` (W-0033) | V1 không gửi; `v1NotificationEnabled=false` **immutable** |
| Vòng phản hồi opt-out | `DEFERRED_TARGET` (W-0034) | `P4-6`, ngoài phạm vi V1 |
| Phím `9` | `NOT_ENABLED` | `AS-07`, UI không cho bật |
| Ghi âm cuộc gọi | **OFF mặc định** | `DT-05` |
| Ngôn ngữ thứ hai trong console | không làm | `DTS-03` — console **chỉ tiếng Việt** |
| Dịch `order_state` | không dịch | `NT-3` — trạng thái đơn thuộc Order Core |
| Dịch CSV/audit/evidence | không dịch | `NT-5` — giữ mã gốc để đối soát |
| Inbound (lookup/order-by-phone/tư vấn) | future scope | `D-08` — outbound-only |

---

## 5. Quyết định còn mở — 21 `OD-V1-*` + 2 `OD-VOICE-*`

### 5.1 · Bảy quyết định `OD-V1-01..07` — hợp đồng Sales

| ID | Nội dung | Chủ sở hữu |
| --- | --- | --- |
| `OD-V1-01` | ma trận program/payment/IVR-required/callable | Sales Product/Core |
| `OD-V1-02` | path callback generic + ACK taxonomy | Sales API/Core |
| `OD-V1-03` | `order_version` — cách lộ, cách bump, hành vi stale | Sales Core |
| `OD-V1-04` | schema/nội dung/giới hạn item của speech summary | Sales/Product/Privacy |
| `OD-V1-05` | dial-token issue/resolve/TTL/one-use | Sales/Security/Telephony |
| `OD-V1-06` | ngữ nghĩa no-answer/timeout/revalidation | Sales Product/Core |
| `OD-V1-07` | auth production và mTLS | Security/Platform |

### 5.2 · Năm quyết định `OD-V1-08..12` — vận hành

`OD-V1-08` attempt policy cuối · `OD-V1-09` protocol/DTMF/disposition/allowlist của 1 SIM lab ·
`OD-V1-10` capacity/failover/caller-ID/cost của 32 eSIM · `OD-V1-11` kịch bản/legal/do-not-call/
retention · `OD-V1-12` thẩm quyền pilot/release + kill switch.

### 5.3 · Chín quyết định `OD-V1-13..21` — phát hiện từ red-team, đáng đọc kỹ

| ID | Vấn đề | Mức |
| --- | --- | --- |
| `OD-V1-13` | **`GOLDEN_HOUR + ONLINE` có thuộc scope IVR không.** Business source đọc được là COD-only (`DS-01`). Delta này **chưa được owner phê duyệt** | 🔴 |
| `OD-V1-14` | **`ivr_confirmation_required` không có business source.** `grep -rln` toàn bộ `docs/documents/` → **0 hit**. Cả OpenAPI (`enum:[true]`) và DB (`must be true`) đang gate trên một field chưa có nguồn | 🔴 |
| `OD-V1-15` | **Whitelist biến lời thoại.** Hai bộ spec active **mâu thuẫn**: bộ hẹp 4 biến vs bộ Target V1 cần thêm `items[]` + `delivery_area_short`. Mở rộng whitelist **tự nó là một quyết định privacy** | 🔴 |
| `OD-V1-16` | **Attempt policy lệch với business source.** Tài liệu phase-8 ghi GH = 2 lần/**10 phút**, 24/7 = **3** lần/15 phút; `D-10` ghi 2 lần, GH **5 phút**, 24/7 15 phút | 🔴 |
| `OD-V1-17` | **`dial_token` reuse.** Một token scalar vs ≥ 2 lần quay + retry kỹ thuật; 5 tài liệu ghi one-use; **không có endpoint reissue** trong bất kỳ contract nào | 🔴 |
| `OD-V1-18` | **Vị trí resolve `dial_token → E.164`.** `specs/api/04` nói adapter **không** nhận số; `P2-4` đặt resolver trong IVR; gateway thương mại quay E.164. **Trust boundary chưa được định nghĩa ở đâu** | 🔴 |
| `OD-V1-19` | **TTS/speech provider.** Chọn vendor kéo theo PDPA (nội dung đơn rời mạng), cost, và chấp nhận phát âm | 🟡 (đang chốt qua `OD-VOICE-01`) |
| `OD-V1-20` | **RBAC production cho runtime-gate.** Đã cấp cho Admin, nhưng **four-eyes mới có 1 chữ ký** (owner module IVR); Security/Platform + Release owner **vẫn trống** | 🟡 |
| `OD-V1-21` | **GitLab platform provisioning** | 🟡 |

### 5.4 · Hai quyết định `OD-VOICE-*` còn mở

| ID | Nội dung | Trạng thái |
| --- | --- | --- |
| `OD-VOICE-01` | nguồn giọng production — **đã đảo hướng 3 lần** (ElevenLabs → vendor Việt → ElevenLabs Starter `$6`) | `ELEVENLABS_STARTER_PROPOSED` |
| `OD-VOICE-04` | tự host / thu âm người thật thay vì thuê vendor | `OPEN` |

`OD-VOICE-02` (phân miền 34 tỉnh), `OD-VOICE-03` (một template), `OD-VOICE-05` (chốt 3 giọng)
đã `CLOSED`.

> ⚠️ **`OD-VOICE-04` có ràng buộc license nghiêm trọng cần biết:** không model TTS tiếng Việt
> open-source nào vừa chất lượng vừa sạch license. `viXTTS` = CPML non-commercial **và Coqui đã
> đóng cửa 1/2024 nên không còn ai bán license** — không thể hợp thức hoá. `F5-TTS` weights =
> CC-BY-NC. Lối sạch duy nhất là **dữ liệu giọng của chính mình** (voice actor có hợp đồng).
> Nghị định 13/2023 coi **giọng nói là dữ liệu cá nhân**.

---

## 6. Rủi ro — xếp theo mức

### 6.1 · P0 — chặn

| ID | Rủi ro | Giảm thiểu hiện có |
| --- | --- | --- |
| `R-V1-01` | ma trận producer GH ONLINE / 24-7 COD chưa khoá đủ | Target task OAS + fake producer; chờ Sales/Product ký |
| `R-V1-02` | không có speech payload ⇒ lời gọi không đọc được đơn | required summary + fixture; chờ Sales/Product/Privacy |
| `R-V1-03` | dial-token/resolver chưa có | token port/fake; chờ Security/Telephony |
| `R-V1-04` | generic callback/version/ACK/auth chưa có | target client/WireMock + current compat |
| `R-V1-05` | **candidate `D-10` bị hard-code rồi dùng cho production** | versioned registry + PROD approval guard |
| `R-V1-06` | **một SIM lab bị hiểu nhầm là bằng chứng 32-eSIM production** | tách gate/evidence rõ ràng |
| `R-V1-07` | legacy prompt/seed làm dev quay lại COD-only/current behavior | source priority, legacy label, CI contract test |
| `R-06` | chưa rõ telephony/SIM provider protocol | owner quyết protocol; adapter tách biệt |
| `R-09` | dữ liệu khách là PII | `phone_ref`/masked/token; cấm raw phone/full profile |
| `R-16` | nhầm technical failure với no-answer | `is_counted_customer_attempt=false`; mapping rõ |
| `R-21` | **release gate bị bỏ qua / tuyên bố production-ready sớm** | `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi gate pass |
| `R-22` | credential/session console bị lộ hoặc cấp quyền quá mức | hash password/token; generic 401 + lockout; session revoke; RBAC 2 role server-side |

### 6.2 · Rủi ro đặc thù của giai đoạn này

| # | Rủi ro | Mức | Vì sao đáng lo | Giảm thiểu |
| --- | --- | --- | --- | --- |
| **N-1** | **"Đã gọi được trong lab" bị đọc thành "A1 đã xong"** | 🔴 CAO | Lab W-0104 phát **một file cố định**. Một kết quả "gọi được, khách bấm 1, disposition đúng" chứng minh **chặng quay số**, không chứng minh khách nghe đúng đơn của mình | Tiêu chí nghiệm thu A1 đòi **hai đơn khác nhau ⇒ hai chuỗi audio khác nhau** (so bằng `PlaylistHash`) — đó là phép thử phân biệt được hai thứ |
| **N-2** | **Hosted CI không chạy** ⇒ mọi gate của W-0104..W-0120 chỉ là local | 🔴 CAO | `remote.origin.pushurl` trỏ GitHub. Evidence pack ghi "PASS" nhưng đó là PASS trên máy dev, không phải trên runner | **Nguyên nhân đã sửa `2026-08-27` (`W-0121`)** — `origin` nay push tới cả hai remote. Rủi ro **chưa hạ mức**: phải có một lượt push và một pipeline xanh mới hạ được. Vẫn nêu rõ `NOT_RUN` trong từng evidence pack |
| **N-3** | **`OD-15` bị Module 3 vô hiệu bằng default `false`** | 🟡 TB | Nếu Module 3 gửi `trusted_skip_allowed=false` như giá trị mặc định, **mọi đơn bị veto và không bao giờ skip** — mà không ai biết | Đã ghi thành mục checklist riêng trong tài liệu bàn giao; cần Module 3 xác nhận bằng văn bản |
| **N-4** | **`order_state` hard-code `"CONFIRMING"`** | 🟡 TB | Module 3 đổi tên state ⇒ IVR trả `ORDER_STATE_NOT_CALLABLE` cho **toàn bộ** task mới, **im lặng, không alert nào bắt được** | Cần Module 3 chọn: công bố state callable như dữ liệu, hoặc cam kết hằng số hợp đồng |
| **N-5** | **Chưa deploy lần nào** | 🟡 TB | `helm rollback --atomic` đã cấu hình nhưng chưa lượt deploy nào từng chạy. Rollback chưa từng được kiểm chứng trên hạ tầng thật | Chờ `G-PLATFORM`; DR selftest hiện chạy trên compose local |
| **N-6** | **Four-eyes của `OD-V1-20` mới có 1 chữ ký** | 🟡 TB | Lab acceptance report và `release-compliance-checklist` S-07 chỉ được đánh ✅ khi có chữ ký thứ hai | `PendingRuntimeGateAuthorization` giữ cổng đóng — thiếu chữ ký thì cờ **không đổi được**, đây là fail-safe đúng |
| **N-7** | **Chi phí vendor TTS tính trên fixture** | 🟢 THẤP | Tỉ lệ trúng cache thật phụ thuộc phân bố phường/xã thực tế | Kiến trúc lai đã cắt 68% ký tự; đo lại khi có đơn thật |
| **N-8** | **Chỉ có 1 giọng nữ miền Trung ở mỗi vendor Việt** | 🟢 THẤP (sau khi chọn ElevenLabs) | Nếu owner không ưng giọng Trung, không có phương án thay thế cùng vendor | Đã chuyển sang ElevenLabs (catalog rộng hơn); `OD-VOICE-04` là phương án lui |
| **N-9** | **FPT.AI ngừng phục vụ khách hàng cá nhân từ 6/7/2026** | 🟢 THẤP | Nếu IVR chạy trên tài khoản cá nhân thì mất dịch vụ | Đang dùng tài khoản doanh nghiệp; nhưng là tín hiệu vendor đổi chính sách — hợp đồng phải có điều khoản thông báo trước |

---

## 7. Nợ kỹ thuật đã ghi nhận

| # | Nợ | Mức | Ghi ở đâu |
| --- | --- | --- | --- |
| 1 | `AudioCacheKey` không gồm hash text đã render | 🟢 | W-0106 §8 `R15` |
| 2 | `order_state` hard-code literal `"CONFIRMING"` | 🟡 | IR-06 §3.7 |
| 3 | `items[]` chưa có giới hạn trên ⇒ đơn 40 dòng = câu thoại vài phút | 🟡 | IR-06 §3.5 |
| 4 | `bearerAuth` là `http/bearer` nên **không mang được scope** — muốn scope phải đổi sang `oauth2/clientCredentials`, tức **đổi contract** | 🟡 | IR-06 §7 |
| 5 | `CHECK` constraint cố ý **giữ mở** cho `order_state`, reason/resolution, taxonomy chưa khoá | 🟢 | W-0115 |
| 6 | Warehouse `analytics` là **một schema trong cùng database**, không phải cluster riêng | 🟢 | `docs/kpi-catalog.md` §1 — phần thật hôm nay là **ranh giới quyền** |
| 7 | PII scan toàn repo từng đỏ ở 2 file evidence W-0107 | 🟢 | W-0108 §7 |

---

## 8. Cái danh sách này **không** nói

- **Không nói còn bao nhiêu ngày.** Nhóm A ước lượng được; nhóm B thì không — nó phụ thuộc lịch
  của bên khác, và IVR không có quyền đặt lịch đó.
- **Không xếp hạng ai chậm.** Cổng ngoài mở không phải lỗi của bên nào; nó là trạng thái của một
  chương trình nhiều module chạy song song.
- **Không đóng cổng nào.** Chỉ artifact thật mới đóng được.
