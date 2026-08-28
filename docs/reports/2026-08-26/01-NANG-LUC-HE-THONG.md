# Năng lực hệ thống — hệ thống làm được gì

> **HISTORICAL_EVIDENCE / SUPERSEDED — 2026-08-27:** Báo cáo này khóa tại baseline ngày
> 2026-08-26. Mọi mô tả IVR tự phân loại khách cũ/khách mới hoặc trusted-skip đã bị `OD-18`/
> `W-0123` thay thế: Module 3 quyết định task cần gọi, IVR chỉ thực thi. Giữ số liệu cũ để audit.

**Ngày:** 2026-08-26 · **Baseline:** `main@bdde72c`
**Nguồn:** đọc trực tiếp source, không suy luận từ tài liệu mô tả.

> Mỗi mục dưới đây đều trỏ tới file thật trong repo. Nếu một mục ghi "✅", nghĩa là **có code và
> có test**. Nếu ghi "🟡", nghĩa là **có chuỗi xử lý nhưng còn thiếu một mảnh** — mảnh đó được nêu tên.
> Không mục nào ghi "✅" chỉ vì tài liệu nói vậy.

---

## 0. Hệ thống này là gì, trong ba câu

IVR Order Confirmation là hệ thống **gọi điện tự động ra ngoài (outbound)** để xác nhận đơn hàng,
mục đích chống **đơn ảo**. Khách nhấc máy → nghe đọc tóm tắt đơn bằng tiếng Việt (giọng theo miền
của địa chỉ giao) → bấm phím `1` để xác nhận hoặc `0` để huỷ.

IVR **không sở hữu đơn hàng**. Nó không có bảng orders, không truy vấn đơn, không đổi trạng thái
đơn. Mọi thứ nó biết đều do Module 3 đẩy sang trong **một payload duy nhất**; mọi thứ nó biết được
từ khách đều được **gửi ngược lại như một tín hiệu** — Module 3 mới là bên quyết định.

IVR **fail-closed**: thiếu bất kỳ bằng chứng nào thì **không gọi**. Nó không đoán.

---

## 1. Kiến trúc — 5 project .NET + 1 console Next.js

```
                      admin-ui (Next.js 16 · TypeScript strict)
                             │  (trình duyệt CHỈ nói chuyện với Next.js server;
                             │   Next.js server là caller DUY NHẤT của Ivr.Api)
                             ▼
   Ivr.Api ───────────► Ivr.Infrastructure ───────────► Ivr.Domain
      │                       ▲                              ▲
      └──► Ivr.Contracts      │                              │
                              │                              │
   Ivr.Worker ────────────────┘──────────────────────────────┘
      │
      └──► Ivr.Contracts
```

| Project | Vai trò | Quy mô |
| --- | --- | --- |
| `Ivr.Api` | HTTP surface: intake, internal lifecycle, admin, script, dev-tooling, feature-flag, account/auth, health | **51 route** đã map (48 nghiệp vụ + 3 health probe) |
| `Ivr.Worker` | scheduler · normalizer · callback outbox · retention (định kỳ + run-once) · analytics ETL · 2 channel provisioner · heartbeat · health listener | **10 hosted service** (8 job trong `Jobs/`) |
| `Ivr.Infrastructure` | EF/PostgreSQL, audit/evidence append-only, idempotency, speech/telephony adapter, feature flag, analytics warehouse, governance/DSAR | ~30 namespace |
| `Ivr.Domain` | Quy tắc nghiệp vụ thuần: eligibility, attempt policy, disposition mapping, PII guard, tiếng Việt/vùng miền | không phụ thuộc hạ tầng (`UT-BOOT-03` khoá) |
| `Ivr.Contracts` | DTO sinh từ OpenAPI + client Sales Target V1 + client Golden Hour compat (ghim riêng) | codegen NSwag 14.7.1 |
| `admin-ui` | Console vận hành, App Router, RBAC server-side, i18n tiếng Việt | 16 trang + 4 route handler |

**Số liệu quy mô** (đo bằng `wc -l`, không kể `bin/obj/node_modules`):

| | Dòng |
| --- | ---: |
| Mã nguồn .NET (`src/**/*.cs`) | 81.838 |
| Test .NET (`tests/**/*.cs`) | 26.976 |
| Mã nguồn console (`admin-ui/src/**`) | 11.805 |
| Test console (`admin-ui/tests/**`) | 5.594 |
| Tài liệu markdown (specs/docs/plan/prompt/IR) | 535 file |

---

## 2. Luồng nghiệp vụ đầu-cuối — 7 chặng

### Chặng 1 · Nhận task (Task Intake) ✅

`POST /v1/ivr/order-confirmation/tasks` — [`TaskIntakeEndpoint.cs`](../../../src/Ivr.Api/Intake/TaskIntakeEndpoint.cs)

| Làm được gì | Chi tiết |
| --- | --- |
| Validate 22 field bắt buộc | thiếu bất kỳ field nào → `422`, không tạo job |
| Ma trận chương trình × thanh toán | chỉ nhận `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`; kết hợp khác → `422`. Enforce ở **4 tầng độc lập**: OpenAPI, intake, eligibility, `CHECK` constraint DB |
| Idempotency | header `Idempotency-Key` 8–200 ký tự; cùng key + cùng body → trả lại kết quả cũ; cùng key + khác body → `409`. Store là PostgreSQL, không phải in-memory |
| Correlation | `X-Correlation-Id` xuyên suốt; nếu body cũng có `correlation_id` mà lệch header → `422` |
| Allowlist caller | chỉ Order Core service token gọi được (`OrderCoreAllowlistMiddleware`) |
| Chặn PII trong lời thoại | `delivery_area_short` bị chặn cả bằng regex (chữ số đứng đầu, dạng `x/y`) **và** bằng detector ngữ nghĩa → `IVR_PII_POLICY_VIOLATION` |
| Outbox nguyên tử | task + job + outbox + audit + evidence ghi trong **một transaction** |
| 5 quyết định trả về | `TASK_ACCEPTED_CALL_JOB_CREATED`, `TASK_ACCEPTED_DRY_RUN_ONLY`, `TASK_SKIPPED_TRUSTED_CUSTOMER`, `TASK_HELD_ADMIN_REVIEW`, `TASK_HELD_POLICY_MISSING` |

### Chặng 2 · Kiểm điều kiện (Eligibility) ✅

[`EligibilityRules.cs`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs) ·
[`EligibilityService.cs`](../../../src/Ivr.Api/Application/EligibilityService.cs)

Cổng vào có **6 quyết định** và **35 mã lý do** (đếm trực tiếp trong `EligibilityRules.cs`).
Toàn bộ đều fail-closed — thiếu bằng chứng thì **không gọi**, chứ không phải "cứ gọi rồi tính".
Bảng dưới liệt kê các mã chính theo nhóm.

| Nhóm chặn | Mã lý do |
| --- | --- |
| Loại đơn / trạng thái | `NOT_OFFICIAL_ORDER`, `ORDER_STATE_NOT_CALLABLE`, `PROGRAM_PAYMENT_MATRIX_REJECTED` |
| Bằng chứng eligibility từ Sales | `ELIGIBILITY_SNAPSHOT_MISSING / UNKNOWN / BLOCKED / UNREADABLE / STALE`, `ELIGIBILITY_SOURCE_UNAVAILABLE`, `ELIGIBILITY_SOURCE_VERSION_MISSING`, `ELIGIBILITY_EVIDENCE_MISSING` |
| Hàng còn bán được (sellable) | `SELLABLE_SNAPSHOT_MISSING / STALE`, `SELLABLE_STATUS_UNKNOWN`, `INVENTORY_NOT_SELLABLE`, `RECALL_HOLD_ACTIVE`, `SALE_LOCK_ACTIVE`, `QUALITY_HOLD_ACTIVE`, `STOCK_UNAVAILABLE`, `BATCH_NOT_RELEASED`, `TRACE_NOT_READY` |
| Do-not-call / opt-out | `PHONE_CALL_RESTRICTION_MISSING`, `PHONE_CALL_RESTRICTED`, `PHONE_CALL_RESTRICTION_SOURCE_UNAVAILABLE` |
| Khách cũ (`OD-15`) | `TRUST_RISK_EVIDENCE_UNAVAILABLE`, `RISK_FLAGS_PRESENT_REQUIRE_IVR`, `TRUST_RESOLVER_VERSION_MISSING` |
| Liên hệ / cửa sổ / capacity | `CONTACT_INVALID`, `CONFIRMATION_WINDOW_EXPIRED`, `CAPACITY_SOURCE_UNAVAILABLE`, `CAPACITY_DEADLINE_UNAVAILABLE` |

**Điểm thiết kế đáng ghi — bất đối xứng có chủ ý:**
thiếu bằng chứng **do-not-call** → **chặn gọi**; thiếu bằng chứng **risk** → **vẫn gọi**.
Cả hai đều fail-closed nhưng đóng ngược chiều, vì thiệt hại khác nhau: một bên là gọi tới người
đã từ chối, bên kia là đơn ảo không được xác minh.

**Bỏ qua khách cũ (`OD-15`, W-0118)** ✅ code xong · 🟡 chưa chạy trên dữ liệu thật:
điều kiện skip là `trust.risk_evidence_available=true` **và** `risk_flags` rỗng. Chừng nào Module 3
chưa gửi field đó, **mọi task đủ điều kiện vẫn được gọi** và mang advisory
`TRUST_RISK_EVIDENCE_UNAVAILABLE`. Đây là cách đo tiến độ không cần ai báo cáo: **khi advisory
biến mất khỏi log, Module 3 đã bật field.**

### Chặng 3 · Lập lịch và hàng đợi (Scheduler) ✅

[`DeadlineScheduler.cs`](../../../src/Ivr.Domain/Scheduling/DeadlineScheduler.cs) ·
[`PostgresSchedulerStore.cs`](../../../src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs) ·
[`SchedulerJobHost.cs`](../../../src/Ivr.Worker/Jobs/SchedulerJobHost.cs)

| Làm được gì | Chi tiết |
| --- | --- |
| Policy registry có version, có audit | `attempt_policy_version` tra trong registry; version lạ → fail-closed (`TASK_HELD_POLICY_MISSING`) |
| Lịch quay theo `T0` + offsets | `T0` = lúc Module 3 mở cửa sổ xác nhận, **không** phải lúc khách bấm đặt |
| Candidate `mock-lab-v1` | GH: 2 lần, window 5′, offsets `[0, 150]` · 24/7: 2 lần, window 15′, offsets `[0, 450]`. **Chỉ hợp lệ ở MOCK/LAB** — production chờ `OD-V1-08` |
| Deadline-aware rolling queue | job đến hạn được claim theo thứ tự deadline, không phải FIFO |
| Channel lease + fencing token | một kênh SIM chỉ có **đúng một** cuộc gọi hoạt động; advisory lock PostgreSQL chống hai worker cùng claim |
| Cooldown + quarantine + auto-disable | kênh lỗi liên tiếp bị cách ly, có metric `ivr_channel_quarantines_total` |
| Capacity incident | hết kênh → `IVR_CAPACITY_EXCEPTION`, **không** tính là lần gọi khách; sự cố có phạm vi theo job, không làm treo toàn bộ intake (W-0088) |
| Khôi phục sau crash | lease hết hạn được thu hồi; job không bị kẹt vĩnh viễn |

### Chặng 4 · Lời thoại tiếng Việt (Speech/TTS) ✅ chuỗi xử lý · 🟡 chờ file audio

Đây là phần được đầu tư sâu nhất trong hai tuần gần nhất (W-0106 + W-0108 + W-0113).

**4.1 · Sinh câu nói đúng cho từng đơn** ✅
[`VietnameseOrderScriptRenderer.cs`](../../../src/Ivr.Domain/Scripts/VietnameseOrderScriptRenderer.cs)

Kịch bản đã duyệt (`v3-test-approved`, immutable) được render với dữ liệu của **từng đơn**:
tên khách, mã đơn rút gọn, danh sách món + số lượng, tổng tiền, vùng giao.

**4.2 · Đọc số bằng chữ, không đọc chữ số** ✅
[`VietnameseNumberSpeller.cs`](../../../src/Ivr.Domain/Speech/VietnameseNumberSpeller.cs)

- `560000` → `"năm trăm sáu mươi nghìn"` (Bắc) / `"năm trăm sáu mươi ngàn"` (Trung, Nam)
- `2,5` → `"hai phẩy năm"`; phần thập phân đọc **từng chữ số**: `0,25` → `"không phẩy hai năm"`
  (gộp lại thành "hai mươi lăm" sẽ mời người nghe hiểu ra một con số khác — mà đây là con số khách
  sắp bấm phím để duyệt)
- Biến thể `linh`/`lẻ` theo miền

**4.3 · Ba giọng nữ theo miền — phân loại theo 34 đơn vị hành chính mới** ✅
[`DeliveryRegionResolver.cs`](../../../src/Ivr.Domain/Speech/DeliveryRegionResolver.cs)

Bản đồ 34 tỉnh/thành theo Nghị quyết `202/2025/QH15` → 3 miền (Bắc 15, Trung 11, Nam 8),
**cộng 29 tên tỉnh trước sáp nhập** làm alias. Không có alias thì mọi đơn còn mang tên cũ sẽ
âm thầm rơi về giọng mặc định.

Thiết kế cố ý **không** thêm field vào `PrivacySafeOrderSummary` (record đó có 95 symbol phụ thuộc
trên 2 execution flow) — miền được suy ở tầng speech như một **hàm thuần tuý** của dữ liệu đã có.

**4.4 · Ghép audio động: 4 đoạn cố định + 3 đoạn biến thiên** ✅ (W-0108)
[`SpeechSegment.cs`](../../../src/Ivr.Domain/Speech/SpeechSegment.cs) ·
[`SpeechSynthesisService.cs`](../../../src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs)

| # | Loại | Nội dung |
| --- | --- | --- |
| 1 | Fixed | `Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm ` |
| 2 | **Dynamic** | danh sách món + số lượng |
| 3 | Fixed | `, tổng tiền ` |
| 4 | **Dynamic** | tổng tiền đọc bằng chữ |
| 5 | Fixed | `, giao đến ` |
| 6 | **Dynamic** | vùng giao rút gọn |
| 7 | Fixed | `. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.` |

**203 / 266 ký tự là cố định** (đo được, `UT-SEG-MANIFEST-12` assert đúng con số này).
4 đoạn cố định × 3 miền = **12 file** cần thu/render **một lần**, sau đó chi phí runtime = 0 và
**nội dung đơn không rời mạng nội bộ** ở phần này. Chỉ 3 đoạn biến thiên mới gọi vendor TTS.

Cache khoá theo **nội dung đoạn**, không theo cuộc gọi: hai đơn khác nhau nhưng giao cùng một
phường dùng chung đúng đoạn `delivery_area_short`. Đo được: đơn thứ hai chỉ tốn **2** lần gọi
provider thay vì 7 (`UT-SEG-CACHESHARE-06`).

**4.5 · Provider TTS ngoài — HTTP thật, trung lập nhà cung cấp** ✅
[`ConfigurableExternalTtsProvider.cs`](../../../src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs)

Không có tên nhà cung cấp nào trong code: endpoint, header credential, scheme và **thân request
JSON** đều là cấu hình. Trả PCM thô (không MP3) để Asterisk phát trực tiếp. Độ dài tính từ số byte
chứ không đoán. Lỗi vendor **chỉ báo status code** — thân lỗi có thể trích lại chính câu vừa gửi,
tức là nội dung đơn (`UT-TTS-EXT-HTTPERROR-04` assert điều này).

**4.6 · Ghi lại giọng đã thực sự phát** ✅ (W-0113)
[`DispatchedVoice.cs`](../../../src/Ivr.Domain/Speech/DispatchedVoice.cs)

Trước W-0113, `voice_region` được **suy lại lúc đọc** — một lần đổi bản đồ giọng giữa lúc gọi và
lúc đọc làm mọi evidence lệch. Nay giọng đã phát được **persist** cùng cuộc gọi.

**🟡 Mảnh còn thiếu:** **12 file MP3 chưa tồn tại.** Chỉ owner tạo được (cần phiên đăng nhập
ElevenLabs + mua gói Starter `$6` để có commercial license). Mọi khẳng định về audio ở đây là về
*chuỗi xử lý*, không phải về *âm thanh* — **chưa ai nghe**.

### Chặng 5 · Quay số và nhận phím (Telephony) ✅ mock + softphone · 🟡 chưa có SIM thật

| Thành phần | Trạng thái |
| --- | --- |
| Port `ISimGateway` (6 phương thức: dial/play/capture_dtmf/hangup/disposition/health) | ✅ vendor-neutral |
| `MockTelephonyDispatchGateway` | ✅ dùng cho MOCK |
| `AsteriskAriSimGateway` + ARI event pump | ✅ dial/playback/DTMF/disposition/hangup thật qua Asterisk 22.10.1 LTS |
| `AsteriskSchedulerDispatchGateway` | ✅ nối lease scheduler vào `DispatchGate` **trước** mọi thao tác telephony |
| `DispatchGate` — 4 tầng chặn | ✅ kill switch → mode → destination allowlist → release gate |
| Dial-token vault (one-use, alias-only) | ✅ `LabDialTokenVault` / `MockDialTokenVault`; **không lưu số E.164 thô** |
| Adapter SIM/carrier thật | ❌ **chưa có** — chờ `OD-V1-09` + procurement |

**Đã chứng minh chạy thật** (W-0104, `ACCEPTED` 2026-08-22, quay qua softphone MicroSIP):

| Kiểm tra | Kết quả |
| --- | --- |
| ARI originate làm MicroSIP đổ chuông | PASS |
| Không bắt máy → `IVR_NO_ANSWER_FINAL` | PASS |
| Bắt máy → `raw_call_status=ANSWERED` | PASS |
| Playback ghi `audio_status=PLAYED`, RTP PCMU tới client | PASS |
| DTMF `1` → `IVR_CONFIRMED` | PASS (`TASK-LAB-20260820110825`) |
| DTMF `0` → `IVR_CUSTOMER_CANCELLED` | PASS (`TASK-LAB-20260820110858`) |
| 3 giọng khác nhau (Neural A/B + ElevenLabs voice C), PCM 8 kHz mono | PASS |

> Đây là bằng chứng **software lab bằng dữ liệu fake**. Nó **không** là bằng chứng SIM/carrier,
> không đóng `G-LAB-SIM`, và không mở quyền gọi khách thật.

**Cắt ngang cuộc gọi đang diễn ra** ✅ (W-0111): Admin **và** Operator đều có
`IVR_CALL_TERMINATE`. API ghi yêu cầu → worker poll (mặc định ≤ 500 ms) → gateway hang up.
Đây là cơ chế **riêng**, không gộp vào kill switch: kill switch chỉ dừng cuộc **sắp** gọi.

### Chặng 6 · Chuẩn hoá kết quả (Normalization) ✅

[`DispositionMapper.cs`](../../../src/Ivr.Domain/Confirmation/DispositionMapper.cs) ·
[`ResultNormalizer.cs`](../../../src/Ivr.Worker/Normalization/ResultNormalizer.cs)

**11 giá trị `result_type`**, và ranh giới quan trọng nhất là cột thứ ba:

| Giá trị | Nghĩa | Tính là lần gọi khách? |
| --- | --- | --- |
| `IVR_CONFIRMED` | bấm `1` | ✅ |
| `IVR_CUSTOMER_CANCELLED` | bấm `0` | ✅ |
| `IVR_NO_ANSWER_ATTEMPT` | không nghe máy, còn lượt | ✅ |
| `IVR_NO_ANSWER_FINAL` | không nghe máy, hết lượt | ✅ |
| `IVR_WRONG_INPUT` | bấm sai phím | ✅ |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | hết cửa sổ | — |
| `IVR_INVALID_PHONE_FINAL` | số không tồn tại / sai số | ❌ (final riêng) |
| `IVR_TECHNICAL_EXCEPTION` | lỗi SIM/audio/mạng | ❌ |
| `IVR_CAPACITY_EXCEPTION` | hết kênh | ❌ |
| `IVR_OPERATIONAL_BLOCKED` | blocker vận hành | ❌ |
| `IVR_POLICY_BLOCKED` | policy chặn | ❌ |

**Technical ≠ no-answer** là ràng buộc P0 (`P0-IVR-004`). Máy bận → `NO_ANSWER` (tính);
khách từ chối cuộc gọi → `NO_ANSWER` (tính, **không** coi là huỷ đơn); thuê bao không tồn tại →
`INVALID_PHONE_FINAL` (không tính). Đúng một lần retry kỹ thuật có giới hạn.

### Chặng 7 · Trả kết quả về Sales (Callback outbox) ✅ target · 🟡 chờ endpoint thật

[`CallbackDispatcher.cs`](../../../src/Ivr.Infrastructure/Callbacks/CallbackDispatcher.cs) ·
[`CallbackDeliveryJobHost.cs`](../../../src/Ivr.Worker/Jobs/CallbackDeliveryJobHost.cs)

| Làm được gì | Chi tiết |
| --- | --- |
| Outbox nguyên tử | snapshot kết quả cuối + capacity ghi cùng transaction với callback record |
| Transport Target V1 | `POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks` — 13 field |
| Transport Golden Hour compat | adapter **tách biệt hoàn toàn**, sau feature flag, **không** route 24/7 qua đó |
| Byte-exact payload | payload lưu nguyên si để retry không sinh body khác (migration `P7_1_CallbackPayloadByteExact`) |
| ACK taxonomy đầy đủ | `ACCEPTED` / `DUPLICATE_ACCEPTED` / `BLOCKED_BY_CORE` / `REVIEW_REQUIRED` / `409 REJECTED_STALE` / `409 IDEMPOTENCY_CONFLICT` / 4xx → DLQ / `429` theo `Retry-After` / `5xx` retry backoff |
| Circuit breaker | mở mạch khi downstream chết; `/health/ready` trả `503` khi mạch mở |
| Race guard | gửi lại `order_version_seen_by_ivr` nguyên si; IVR **không** so sánh — Module 3 là bên duy nhất quyết định version còn tươi hay không |

**🟡 Mảnh còn thiếu:** endpoint generic phía Module 3 **chưa tồn tại**. Hiện chỉ có
`POST /api/v1/internal/ivr/golden-hour/callbacks` (riêng Giờ Vàng, ID `int64` thay vì string,
4 giá trị kết quả thay vì 11, **không có field version nào**). **Chương trình 24/7 hiện không có
lối trả kết quả nào cả.**

---

## 3. Console vận hành — 16 trang + 4 route handler

Trình duyệt **chỉ** nói chuyện với Next.js server; Next.js server là **caller duy nhất** của
`Ivr.Api`. Không có token nào đi ra trình duyệt.

| # | Màn | Route | Đọc | Ghi |
| --- | --- | --- | --- | --- |
| 1 | Đăng nhập | `/login` | — | sign-in (rate limit + lockout) |
| 2 | Dashboard | `/dashboard` | 4 tile KPI + bảng kênh SIM | pause/resume queue · enable/disable SIM |
| 3 | Hàng đợi | `/queue` | job đang chờ/đang chạy | — |
| 4 | Nhật ký cuộc gọi | `/calls` | lọc theo trạng thái, chương trình, khoảng ngày | — |
| 5 | Chi tiết cuộc gọi | `/calls/[ivrCallJobId]` | dòng thời gian attempt, disposition, **giọng đọc theo miền**, evidence ref (đã mask) | retry kỹ thuật · đưa vào admin review · **cắt ngang cuộc đang gọi** |
| 6 | Cấu hình kịch bản | `/config` | danh sách script + version + trạng thái duyệt | tạo draft · submit · duyệt (4 loại) · retire |
| 7 | Cổng runtime / feature flag | `/flags` | snapshot cờ + kill switch theo môi trường | mutate cờ (qua four-eyes gate) |
| 8 | Trạng thái tích hợp | `/integration` | card `ORDER_CORE` / `SIM_GATEWAY` / `CAPACITY_INCIDENT`, có `detail_vi` | view-only theo spec |
| 9 | Review & retry | `/review` | hàng đợi cần người xử lý | giải quyết review item · retry thủ công |
| 10 | Báo cáo | `/reports` | tổng hợp, xu hướng, breakdown, banner độ tươi dữ liệu | — |
| 11 | Seed / mock | `/seed` | dữ liệu mẫu hiện có | nạp seed · chạy scenario dry-run · áp integration profile (**chỉ non-production**) |
| 12 | Vai trò & quyền | `/roles` | ma trận 2 role × 23 quyền | — |
| 13 | Danh sách tài khoản | `/accounts` | danh sách account | tạo account |
| 14 | Chi tiết tài khoản | `/accounts/[accountId]` | thông tin account | sửa · reset password · xoá |
| 15 | Hồ sơ cá nhân | `/profile` | thông tin của chính mình | — |
| 16 | Trang gốc | `/` | điều hướng theo session | — |

**4 route handler** (không phải màn): `/reports/export` (tải CSV đã mask PII),
`/api/auth/sign-in`, `/api/auth/sign-out`, `/favicon.ico`.

**Việt hoá:** console **chỉ tiếng Việt** (`DTS-03`). Không chỉ chrome — **dữ liệu** cũng được Việt
hoá qua từ điển **42 họ / 237 giá trị** (`enums.vi.json`, đếm ngày 26/08) với component `EnumLabel`
giữ **mã gốc bên cạnh nhãn** để đối soát. Cố ý **không** dịch: `order_state` (thuộc Order Core), CSV/audit/
evidence (giữ mã gốc để đối soát).

---

## 4. Bảo mật, quyền và quyền riêng tư


| | |
| --- | --- |
| Đăng nhập | username + password, hash bằng thuật toán chậm |
| Session | **opaque token, thu hồi được**, không phải JWT tự chứa |
| Chống dò | generic `401` (không tiết lộ user tồn tại hay không) + rate limit + lockout |
| Username | **immutable, không tái sử dụng** |
| Bootstrap | 3 account khởi tạo qua `pnpm db:seed`, hỏi password **không echo**, idempotent |

### 4.2 RBAC — đúng hai role

| Role | Số quyền | Quyền |
| --- | ---: | --- |
| `Admin` | **22** | toàn bộ queue/SIM/retry/review/flag/runtime-gate/account/script (7 quyền script)/terminate/dev-tooling |
| `Operator` | **5** | `IVR_QUEUE_VIEW`, `IVR_SIM_DISABLE`, `IVR_MANUAL_RETRY`, `IVR_ACCOUNT_SELF_VIEW`, `IVR_CALL_TERMINATE` |

Operator **có** quyền cắt cuộc gọi (`OD-CALL-01`): đây là chiều **giảm** rủi ro; bắt Operator đi
tìm Admin trong lúc cuộc gọi đang chạy là thiết kế sai.

Enforcement **server-side**, fail-closed: `FailClosedAuthenticationHandler` +
`PermissionAuthorizationHandler` + `IvrAuthorizationMiddlewareResultHandler`. Guard drift test
phủ **mọi** endpoint (W-0100 đã đóng lỗ hổng 10 endpoint thiếu guard).

### 4.3 Quyền riêng tư ✅

| Cơ chế | Ở đâu |
| --- | --- |
| Không bao giờ lưu/log số điện thoại thô | chỉ `phone_ref` + `phone_masked` + `dial_token` |
| `PiiMaskingFilter` quét **toàn bộ response body** | vi phạm → `IVR_PII_POLICY_VIOLATION` |
| `PiiGuard` / `PiiMasker` | pattern số điện thoại, địa chỉ; đã được cứng hoá qua 3 vòng red-team (`LC_ALL=C`, đa byte tiếng Việt, ký tự điều khiển) |
| Ghi âm **TẮT mặc định** | `DT-05`, immutable |
| Không đọc địa chỉ đầy đủ cho khách | whitelist lời thoại; console **không** nhận `delivery_area_short`, chỉ nhận enum 3 miền |
| `SpeechPrivacyGuard` | chặn nội dung ngoài whitelist trước khi tổng hợp giọng |
| Audit append-only | trigger PostgreSQL chặn **cả UPDATE lẫn DELETE** (`IT-DB-AUDIT-07` chứng minh trực tiếp) |
| DSAR | `DsarService` + `PersonalDataInventory` + runbook |
| Retention | job có thật (`RetentionJobHost` + `RetentionRunOnceHost`), theo lớp dữ liệu, có checkpoint |

### 4.4 Ranh giới IVR **không** làm — và được ép bằng test

| IVR không | Ép bằng |
| --- | --- |
| Ghi/đổi trạng thái đơn | không có endpoint nào; `ArchitectureDependencyTests` |
| Gửi SMS/notification | `v1NotificationEnabled=false` **immutable**; `V1NotificationDisabledTests`; fail-gate `IT-FAILGATE-*` |
| Gọi trực tiếp Ops-core | `NoEgressIlGuard` — quét IL, chặn egress ngoài allowlist |
| Ghi note vào CRM | `D-14` |
| Gọi cho Quote/Cart/Order Draft | `NOT_OFFICIAL_ORDER`, `CHECK` constraint DB |
| Gọi khi thiếu bằng chứng | fail-closed toàn tuyến |

---

## 5. Dữ liệu — 30 bảng PostgreSQL

**23 bảng vận hành** (`ivr_*`) + **7 bảng warehouse** (schema `analytics`):

| Nhóm | Bảng |
| --- | --- |
| Task & job | `ivr_confirmation_tasks`, `ivr_call_jobs`, `ivr_call_attempts`, `ivr_task_intake_outbox` |
| Kết quả | `ivr_call_results`, `ivr_result_callbacks`, `ivr_raw_call_events`, `ivr_technical_exceptions` |
| Vận hành | `ivr_sim_channels`, `ivr_capacity_incidents`, `ivr_review_items`, `ivr_admin_actions` |
| Chính sách & kịch bản | `ivr_attempt_policies`, `ivr_script_versions`, `ivr_script_approvals` |
| Nền tảng | `ivr_audit_log`, `ivr_evidence`, `ivr_evidence_links`, `ivr_idempotency_keys`, `ivr_feature_flags`, `ivr_retention_checkpoints` |
| Analytics | `fact_call_job`, `fact_call_outcome`, `agg_kpi_daily`, `dim_program`, `dim_result_type`, `dim_script_variant`, `etl_checkpoint` |

**13 migration EF Core**, tất cả đều apply/rollback/recreate được (có test).
**16 `CHECK` constraint enum trên 8 bảng** (W-0115) cộng bất biến `final_result_status = result_type`.
Cố ý **giữ mở**: `order_state` (thuộc Order Core), reason/resolution, taxonomy chưa khoá.

**Cổng tương thích schema hai chiều** ✅ (W-0114): CI kiểm **cả hai chiều** —
binary mới trên schema cũ (`IT-SCHEMA-NEWCODE-01/02`) và migration mới dưới code cũ
(`UT-SCHEMA-BACKCOMPAT-01`). Đây là điều kiện để rolling deploy không gãy giữa chừng.

---

## 6. Hợp đồng API

| | |
| --- | --- |
| Contract IVR | `ivr-order-confirmation.v1.yaml` — **`1.0.0-draft.19`**, 43 path / **49 operation** / 93 schema |
| Contract callback ra Sales | `order-core-ivr-callback.target-v1.yaml` — **DRAFT**, chờ hai bên ký |
| Codegen | NSwag 14.7.1, pinned; drift gate ghim SHA-256 (3 hash) |
| Kiểm tương thích | `oasdiff v1.26.1` ghim theo digest; **no breaking changes** vs baseline `draft.2` |
| Portal tài liệu | Redoc tĩnh, 12 artifact, changelog + integration guide + versioning policy |
| Lint | Redocly — 2 tài liệu, 0 lỗi (từ 14 lỗi `nullable` OAS 3.0 → 0 sau W-0117) |

---

## 7. Quan sát, vận hành và triển khai

| Hạng mục | Trạng thái | Chi tiết |
| --- | --- | --- |
| Telemetry | ✅ | OpenTelemetry log/metric/trace, redacted; 6 SLI có **call site thật** trong production code |
| Health probe | ✅ | `/health/live`, `/health/ready` (**fail-closed 503** khi DB không tới được, schema lệch, hoặc mạch callback mở), `/health/startup` |
| Dashboard | ✅ | `ivr-slo-health.json` — 7 panel |
| Alert | ✅ | 5 rule + 3 file promtool test; mỗi alert có `runbook_url` trỏ đúng mục trong `docs/slo.md` |
| Chống "vạch phẳng nói dối" | ✅ | `UT-DASH-PII-04` đi ngược từ biểu thức dashboard/alert về tới call site, đỏ nếu artifact vượt phần đã instrument |
| Chaos / gameday | ✅ | project `tests/chaos` — Toxiproxy + 5 scenario (DB fault, downstream fault, partial partition, SIM fault, recovery) + blast-radius guard |
| Docker | ✅ | 6 Dockerfile (api/worker/ui/migrate/fake-sales/otel) + 3 compose (dev/e2e/softphone) |
| Kubernetes | ✅ | Helm chart: 3 deployment, service/SA, HPA+PDB, 3 NetworkPolicy, migrate hook, retention CronJob; values cho `dev/staging/lab/prod` |
| CI | ✅ cấu hình · 🟡 hosted | GitLab CI, 13 file include, stage `validate→build→test→security→privacy→publish→deploy→promote` |
| Progressive delivery | ✅ cấu hình | Argo Rollouts: canary API, blue-green worker, analysis theo SLO |
| Backup / DR | ✅ script | `backup/restore/prune` + `failover.sh` + `dr-selftest.mjs` |
| Xoay bí mật | ✅ code thật | `RotatingCredentialProvider` — dual-key overlap, emergency revoke, audit **không** ghi giá trị |
| Analytics/BI | ✅ | ETL job → schema `analytics` riêng quyền; KPI catalog có công thức; pipeline **chỉ đọc** |

---

## 8. Chất lượng — con số đo được

> **Xác minh độc lập ngày 26/08.** Bảng dưới không trích lại từ evidence pack — tôi chạy
> `dotnet test Ivr.sln` trên `main@bdde72c` tại thời điểm viết báo cáo: **exit code 0, 0 failed**,
> integration suite chạy PostgreSQL thật qua Testcontainers trong 5 phút 20 giây.

| Hạng mục | Con số |
| --- | ---: |
| Test .NET (`main@bdde72c`, chạy lại 26/08) | **774 / 774** |
| ├─ unit | 486 |
| ├─ integration (Testcontainers PostgreSQL thật) | 258 |
| ├─ contract | 22 |
| └─ chaos | 8 |
| Test console admin-ui (chạy lại 26/08) | **223 / 223** (23 file, 34 s) |
| Test có gắn mã truy vết | **474** |
| File test .NET | **94** (unit 40 · integration 43 · contract 2 · chaos 9) |
| File test console | 23 |
| `dotnet build` | 0 warning / 0 error |
| `dotnet format --verify-no-changes` | PASS |
| Coverage gate CI | ngưỡng 80% |

**Cổng CI hiện có:** build · test · coverage · format · OpenAPI lint/validate/codegen/drift ·
oasdiff breaking-change · docs selftest · PII scan (phủ **mọi** text artifact kể cả `.sql` và file
không extension) · Gitleaks · NuGet vulnerability (validate schema/severity, không chỉ exit code) ·
architecture dependency · quality gate · UI a11y/i18n · observability selftest · chaos ·
schema compatibility · DR selftest · capacity selftest · progressive-delivery selftest ·
traceability · gate-status mirror.

---

## 9. Công cụ phát triển và nghiệm thu

| Công cụ | Dùng để |
| --- | --- |
| Fake Sales provider (`FAKE_TARGET_V1`) | chạy toàn tuyến không cần Sales thật |
| Mock SIM provider | chạy toàn tuyến không cần SIM |
| Mock JWT | chạy toàn tuyến không cần auth production |
| `POST /dev/seed:load` | nạp dữ liệu mẫu qua API (**chỉ non-production**) — trước W-0112 phải làm bằng SQL tay |
| `POST /dev/scenarios/{id}:dry-run` | chạy kịch bản nghiệm thu không quay số thật |
| `POST /dev/integration-profiles/{id}:apply` | bật/tắt trạng thái tích hợp giả để demo |
| `docker-compose.softphone.yml` + MicroSIP | gọi thử miễn phí qua Asterisk, không tốn cước |
| `Convert-LabVoiceAudio.ps1` / `Convert-LabSegmentAudio.ps1` | MP3 → PCM 8 kHz mono, loudnorm, tự verify + ghim SHA-256 |
| `capacity-model.mjs` | mô phỏng capacity 1 → 32 kênh |
| GitNexus MCP | impact analysis bắt buộc trước mọi lần sửa symbol |

---

## 10. Tóm tắt: ✅ và 🟡

**✅ Hoàn chỉnh và có test:**
intake · eligibility fail-closed · scheduler + lease/fencing · disposition mapping ·
callback outbox + ACK + circuit breaker · script lifecycle + four-eyes · runtime gate ·
terminate active call · dev tooling · console 15 màn + RBAC 2 role · Việt hoá dữ liệu ·
tiếng Việt 3 miền + đọc số bằng chữ · ghép audio động · audit/evidence append-only ·
retention + DSAR · analytics warehouse · telemetry/SLO/alert · chaos · Docker/Helm/CI/CD ·
progressive delivery · backup/DR · secret rotation · schema compat gate.

**🟡 Có chuỗi xử lý, thiếu một mảnh cụ thể:**

| Mảnh thiếu | Ai gỡ được |
| --- | --- |
| 12 file MP3 đoạn cố định + gói ElevenLabs Starter | **Owner** |
| Endpoint TTS thật cho 3 đoạn biến thiên | Dev + Infra, sau khi owner mua gói |
| Adapter SIM/carrier thật + 1 SIM lab | Infra + vendor |
| Producer task 24/7 COD từ Module 3 | **Module 3** |
| Endpoint callback generic phía Module 3 | **Module 3** |
| `dial_token` — chọn 1 trong 4 phương án | Module 3 + Security |
| Auth production (issuer/JWKS/audience/scope/sandbox credential) | Security/Platform |
| Attempt policy production đã ký | Product/Core |
| Chữ ký Legal cho kịch bản + retention | Legal/Privacy |
| Hosted CI + hạ tầng K8s/secret store/observability backend | Platform/Infra |
