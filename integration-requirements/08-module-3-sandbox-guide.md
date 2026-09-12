# IR-08 — Môi trường thử cho Module 3

**Chủ đề:** Cách Module 3 gọi vào IVR mà không cần chờ IVR có staging
**Mốc mã:** `W-0282` · **Ngày:** `2026-09-12`
**Trạng thái:** chạy được, đã chứng minh bằng một lượt chạy thật — `24/24` ví dụ đúng như mô tả

> Tài liệu này **không** thay thế [IR-06](./06-module-3-api-handover.md). IR-06 nói hợp đồng có gì.
> Cái này nói **làm sao chạy được nó hôm nay**, trên máy của bạn, trước khi có môi trường chung.

---

## 0. Đọc trong 1 phút

| Câu hỏi | Trả lời |
| --- | --- |
| Địa chỉ ở đâu? | Tự dựng bằng 1 lệnh. Chưa có địa chỉ chung vì cổng `G-PLATFORM` đang chờ Hạ tầng |
| Tài khoản nào? | Một token dịch vụ + header `X-Source-System: order-core`. Có sẵn trong `docker-compose.dev.yml` |
| Có hạn mức không? | Có. **60 lệnh/phút** cho tài khoản `order-core`, vượt thì `429` `IVR_RATE_LIMITED` |
| Có gọi thật không? | **Không, về mặt cấu trúc.** `IVR_ADAPTER_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`, `SIM_PROVIDER=MOCK` |
| Dọn dữ liệu thế nào? | `pnpm sandbox:reset` — xoá sạch ổ đĩa, dựng lại từ trắng |

---

## 1. Dựng lên

```bash
git clone <repo> && cd ivr
pnpm sandbox:up
```

Một lệnh, khoảng 3–5 phút lần đầu (phải dựng ảnh). Xong thì API ở `http://127.0.0.1:58080`.

Kiểm tra sống:

```bash
curl -s http://127.0.0.1:58080/health/ready
```

**Muốn gọi từ máy khác** (ví dụ IVR chạy ở máy A, Module 3 ở máy B):

```bash
IVR_SANDBOX_BIND=0.0.0.0 pnpm sandbox:up
```

Cân nhắc trước khi mở: ba token quản trị của bộ dev nằm công khai trong repo này. Mở ra mạng nội bộ tin
được thì được; mở ra Internet thì không.

---

## 2. Chuẩn bị dữ liệu (bắt buộc, một lần sau mỗi lần dựng)

Stack vừa dựng lên là **cơ sở dữ liệu trắng**. Chưa có chính sách số lần gọi, nên mọi task bạn đẩy
vào sẽ bị giữ lại với `TASK_HELD_POLICY_MISSING` — trông y như IVR hỏng, thực ra là chưa nạp dữ liệu.

```bash
curl -s -X POST http://127.0.0.1:58080/v1/ivr/order-confirmation/dev/seed:load \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer dev-admin-write-token-not-a-real-secret' \
  -H 'X-Service-Scope: ivr.admin.write' \
  -H 'X-Actor-Id: module-3' \
  -H 'X-Correlation-Id: corr-prepare-001' \
  -H 'Idempotency-Key: idem-prepare-001' \
  -d '{"reason":"chuan bi sandbox","rebase_windows":true}'
```

Trả về `task_count: 9`. Lệnh này cũng ghi luôn chính sách số lần gọi, nên chạy một lần là đủ.

> `rebase_windows: true` dời cửa sổ xác nhận của từng mẫu về **bây giờ**. Các mẫu trong repo ghi cứng
> mốc tháng 8/2026; nạp nguyên văn thì mẫu nào cũng bị từ chối vì hết hạn.

---

## 3. Chạy thử toàn bộ ví dụ

```bash
pnpm sandbox:examples
```

Chạy **24 ví dụ** từ bên ngoài, đúng cách bạn sẽ gọi: 6 tình huống cuộc gọi chạy trọn vòng, 5 kiểu từ
chối, và hạn mức. Kết quả ghi ra `.artifacts/sandbox/module-3-examples.json`.

Script tự chờ API sẵn sàng, nên gõ liền sau `pnpm sandbox:up` là được. Nó cũng **tự từ chối chạy lần
hai trên cùng dữ liệu** và chỉ đúng lệnh cần gõ — lý do ở mục 9.

Đọc mã nguồn `deploy/ci/scripts/sandbox-examples.mjs` như một client mẫu — nó không tham chiếu gì tới
assembly của IVR, chỉ nói HTTP.

---

## 4. Đẩy task của chính bạn

```
POST http://127.0.0.1:58080/v1/ivr/order-confirmation/tasks
Authorization: Bearer dev-ordercore-token-not-a-real-secret
X-Source-System: order-core
X-Correlation-Id: <mã đối soát>
Idempotency-Key: <khoá cho một lệnh>
```

Thân là `IvrConfirmationTaskV1` — 23 trường bắt buộc, xem [IR-06 §3.4](./06-module-3-api-handover.md).

### Ba cái bẫy làm mất buổi đầu tiên

Cả ba đều trả lỗi **chỉ sang chỗ khác**, nên ghi ra đây trước:

| Triệu chứng | Nguyên nhân thật | Cách sửa |
| --- | --- | --- |
| `422 IVR_STATE_NOT_CALLABLE` | Chép nguyên mẫu trong tài liệu, cửa sổ xác nhận là tháng 8/2026 nên đã hết hạn | Đặt `confirmation_window_*` về hiện tại. Bốn mốc `created_at`, `confirmation_window_started_at`, `confirmation_window_expires_at`, `dial_token_expires_at` phải dời **cùng một khoảng**, giữ nguyên khoảng cách giữa chúng |
| `422 IVR_MISSING_TRACE` | `correlation_id` trong thân khác `X-Correlation-Id` trên header | Cho bằng nhau |
| `200` nhưng `TASK_HELD_ADMIN_REVIEW`, lý do `ELIGIBILITY_SOURCE_VERSION_MISSING` | `eligibility_snapshot` thiếu trường bắt buộc | Xem [IR-06 §3.7](./06-module-3-api-handover.md): bắt buộc có `source_version` và `captured_at`; `captured_at` phải nằm **trong** cửa sổ xác nhận |

---

## 5. Vòng đời một task — 5 chặng

```
Module 3 → POST /tasks                → TASK_ACCEPTED_DRY_RUN_ONLY + ivr_call_job_id
         → (xét điều kiện gọi)         → ELIGIBLE_FOR_IVR
         → IVR quay số giả lập          → bấm 1 / bấm 0 / không nghe / …
         → IVR chuẩn hoá kết quả        → IVR_CONFIRMED, IVR_CUSTOMER_CANCELLED, …
         → IVR gọi callback về Module 3 → kèm recommended_core_action
```

Theo dõi một task: `GET /call-jobs/{ivrCallJobId}/detail` (token hạng đọc). Nó trả về đủ `attempts`,
`results`, `callbacks`.

### ⚠️ Chặng 2 hiện **chưa có ai chạy**

`POST /eligibility-checks` là thứ đưa task rời khỏi trạng thái giữ để được quay số. Trong IVR hiện tại
**không có vòng lặp nào gọi nó** — worker đăng ký 10 dịch vụ nền, không cái nào làm việc này.

Đây là **việc của IVR, không phải của Module 3**: endpoint đó thuộc nhóm nội bộ và header nguồn của nó
ghi thẳng là `ivr-worker`. Bạn **không** được cấp token nội bộ.

Trong lúc chờ chốt ai sở hữu vòng lặp này, script ví dụ tự gọi giúp bước đó để cả vòng chạy được, và
nó ghi rõ đang đóng thế. Chủ Module 8 đang cần quyết: vòng lặp này thuộc về ai và nó xác thực lại
những gì (D-06).

---

## 6. Sáu tình huống cuộc gọi

Kết quả giả lập chọn theo **`task_id`**. Muốn tình huống nào thì đặt đúng `task_id` đó:

| `task_id` | Chuyện gì xảy ra | `result_type` | Module 3 có nhận được không? |
| --- | --- | --- | --- |
| `TASK-M3-CONFIRM` | Nghe máy, bấm 1 | `IVR_CONFIRMED` | **Có** |
| `TASK-M3-CANCEL` | Nghe máy, bấm 0 | `IVR_CUSTOMER_CANCELLED` | **Có** |
| `TASK-M3-BADNUMBER` | Mạng không tới được số đó | `IVR_INVALID_PHONE_FINAL` | **Có** |
| `TASK-M3-NOANSWER` | Đổ chuông không ai nghe, **còn lượt gọi** | `IVR_NO_ANSWER_ATTEMPT` | **Không** |
| `TASK-M3-WRONGKEY` | Nghe máy, bấm phím ngoài menu | `IVR_WRONG_INPUT` | **Không** |
| `TASK-M3-TECHNICAL` | Âm thanh phía IVR hỏng giữa cuộc | `IVR_TECHNICAL_EXCEPTION` | **Không** |

**Ba dòng "Không" là điều khoản hợp đồng, không phải thiếu sót.** Khi còn lượt gọi, IVR im lặng và sẽ
gọi lại. Bên nào coi im lặng là thất bại rồi tự huỷ đơn là đang huỷ đơn mà IVR vẫn đang làm.

`IVR_NO_ANSWER_FINAL` chỉ sinh ra sau **lượt gọi cuối** (`mock-lab-v1` cho 2 lượt, cách nhau 150 giây).
Muốn xem: `node deploy/ci/scripts/sandbox-examples.mjs --result-timeout-ms 200000`.

---

## 7. Nhận kết quả về phía Module 3

Mặc định IVR gửi kết quả vào đầu nhận giả lập trong stack. Muốn nhận về **máy chủ của bạn**:

```bash
IVR_SANDBOX_CALLBACK_BASE_URL=http://host.docker.internal:9000 pnpm sandbox:up
```

IVR sẽ gọi `POST {base}/api/v1/internal/orders/{orderId}/ivr-result-callbacks`. Hình dạng thân và bộ
mã ACK bạn phải trả về nằm ở [IR-06 §4](./06-module-3-api-handover.md).

Nhắc lại một điều dễ nhầm: HTTP `200` nghĩa là **bạn đã nhận**, không phải đơn đã xác nhận. Bốn mã
ngữ nghĩa là `ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, `REVIEW_REQUIRED`.

---

## 8. Hạn mức tài khoản dịch vụ

| Tài khoản | Trần | Cửa sổ |
| --- | ---: | --- |
| `order-core` (của Module 3) | 60 lệnh | 60 giây |
| Mặc định các tài khoản khác | 600 lệnh | 60 giây |

Vượt trần thì nhận:

```json
{"error":{"code":"IVR_RATE_LIMITED","message":"Too many requests. Try again later.",
 "details":{"retry_after_seconds":"29","requests_per_window":"60"},"correlationId":"…"}}
```

Trần này **đặt thấp có chủ ý**, không phải con số năng lực thật — năng lực thật phải đo bằng SIM thật.
Để thấp để client của bạn gặp `429` ở đây, chỗ rẻ, thay vì gặp lần đầu ở production.

`retry_after_seconds` nằm trong `details` chứ không phải header `Retry-After`. Thêm header đó là một
thay đổi hợp đồng trên cả 38 lệnh, sẽ đưa vào bản hợp đồng kế tiếp.

---

## 9. Dọn dữ liệu

| Mức | Lệnh | Còn lại gì |
| --- | --- | --- |
| Xoá sạch | `pnpm sandbox:reset` | Không còn gì. Dựng lại phải nạp mẫu lại |
| Dừng quay số, giữ dữ liệu | `POST /queue:pause` (hạng `danger`) | Còn nguyên, chỉ ngừng gọi |
| Dừng hẳn các việc đang chạy | `POST /call-jobs:terminate-all` (hạng `danger`) | Còn lịch sử |

Bộ ví dụ cần một sandbox **sạch**: kết quả giả lập gắn cứng theo `task_id`, nên chạy lần hai trên cùng
ổ đĩa là gửi lại đúng `Idempotency-Key` cũ với cửa sổ thời gian mới — cùng khoá, khác nội dung, và IVR
**từ chối là đúng** (`409 IVR_IDEMPOTENCY_CONFLICT`). Script phát hiện việc này trước khi chạy và in ra
đúng lệnh cần gõ, thay vì để bạn đọc 9 dòng đỏ trông như module hỏng.

---

## 10. Những gì chưa có — nói trước để khỏi mất thời gian tìm

| Chưa có | Vì sao | Ai gỡ |
| --- | --- | --- |
| Địa chỉ sandbox dùng chung | Cổng `G-PLATFORM`: chưa có cụm, CSDL, DNS/TLS cho staging | Hạ tầng |
| Vòng lặp xét điều kiện gọi | Chưa chốt ai sở hữu và nó xác thực lại những gì | Chủ Module 8 |
| Xác thực bằng JWT thật | Sandbox dùng token tĩnh; hồ sơ xác thực production còn mở (`OD-V1-07`) | Chủ Module 8 + An ninh |
| Gọi ra số thật | Chưa có hợp đồng nhà mạng, chưa đo dung lượng | Chủ dự án |

Không mục nào chặn việc bạn đấu nối hôm nay: sandbox đủ để viết và chứng minh client.

---

## 11. Liên hệ

Câu hỏi về hợp đồng: trả lời thẳng vào phiếu [IR-07](./07-module-3-decision-sheet.md) — **21 mục, đã
gửi 10/09**. Đó là thứ đang chặn 5 cổng phát hành phía IVR.

Câu hỏi về sandbox này: nhắn owner Module 8. Nếu một ví dụ trong `pnpm sandbox:examples` đỏ trên máy
bạn mà xanh ở đây, gửi kèm `.artifacts/sandbox/module-3-examples.json`.
