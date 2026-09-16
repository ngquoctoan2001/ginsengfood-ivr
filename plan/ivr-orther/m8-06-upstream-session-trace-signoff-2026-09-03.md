# M8-06 — Upstream session trace sign-off

**Work ID:** `W-0146`

**Baseline kiểm tra:** `main@b21ec676e490`

**Trạng thái:** **`M8_POSITION_SIGNED / GOLDEN_HOUR_SESSION_ID_PROPOSED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED`**

**Người ký phía Module 8:** **Tôi — Module 8 / Project Owner** · **2026-09-03**

**External signature/artifact:** **NOT_RECEIVED**

> Module 8 đã khóa một đề xuất duy nhất và stop rule của mình. Đây chưa phải shared contract: chưa
> có chữ ký Module 3 thì không được sửa OpenAPI, generated DTO, domain, DB, scheduler hoặc test để
> biến đề xuất thành việc đã rồi.

## 1. Kết luận bắt buộc

1. Field upstream được M8 đề xuất là **`golden_hour_session_id`**. Không dùng `session_id`,
   `source_session_id` hoặc hai alias song song.
2. `golden_hour_session_id` **bắt buộc và non-null** với `GOLDEN_HOUR`; **phải vắng mặt** với
   `TWENTY_FOUR_SEVEN`.
3. `CapacityIncidentEntity.SessionId` hiện tại là **capacity scope ID nội bộ của IVR**, không phải
   Golden Hour session. Cột này phải được giữ nguyên semantics và không được nhận giá trị upstream.
4. Module 3 / Golden Hour Core là owner phát ID. IVR chỉ validate, persist nguyên giá trị/case và dùng để
   đối soát; IVR không sinh, sửa, normalize hoặc suy ra ID.
5. Chưa có chữ ký M3, producer commit/CDC và kế hoạch cutover thì trạng thái đúng là
   `CODE_NOT_AUTHORIZED`, không phải `CONTRACT_LOCKED` hay `READY_TO_IMPLEMENT`.

## 2. Current truth đã đối chiếu

| Bề mặt | Trạng thái current HEAD | Hệ quả |
| --- | --- | --- |
| `IvrConfirmationTaskV1` OpenAPI/generated DTO | Không có upstream session field | M3 gửi thêm field hôm nay sẽ bị closed schema từ chối |
| Domain snapshot | Không có upstream session | Runtime không thể mang provenance này qua intake |
| `ConfirmationTaskEntity` / `CallJobEntity` | Không có upstream session column | Không truy vết được task/job về phiên Golden Hour |
| `CapacityIncidentEntity.SessionId` | Non-null `session_id` | Tên cột có vẻ generic nhưng dữ liệu current là ID nội bộ/synthetic |
| Writers current | `MOCK-SCHED-*`, `SCHED-*`, `SCHED-DEADLINE-*`, `ADMIN-QUEUE-*`, `SCHEDULER-CAPACITY-UNAVAILABLE`, `CAPACITY-SOURCE-ERROR` | Ít nhất nhiều loại scope khác nhau đang dùng chung cột; không được đổi nghĩa ngầm |
| Admin pause incident | `ProgramCode=ALL`, scope global | Không có và không thể có một Golden Hour session duy nhất |
| Master traceability | Dùng đúng tên `golden_hour_session_id`; thiếu ID thì không được claim thuộc phiên Golden Hour | Đây là tên domain-specific có nguồn, không phải tên M8 tự đặt |
| Phase-8 capacity/log docs | Ghi generic `session_id` nhưng không định nghĩa owner, format hay quan hệ với Golden Hour | Không đủ để ghi đè master/current runtime semantics |

Kết luận: đề xuất “map thẳng session Golden Hour vào `capacity_incident.session_id`” là **sai mô
hình dữ liệu**. Nó làm global admin incident và capacity calculation session mang danh nghĩa của một
business session không tồn tại.

## 3. Đề xuất contract phía Module 8

| Thuộc tính | Đề xuất phải được M3 ký |
| --- | --- |
| Wire field | `golden_hour_session_id` |
| Owner/producer | Module 3 / Golden Hour Core |
| Kiểu | string, non-null khi có |
| Độ dài | `1..128` ký tự |
| Format | opaque string; không control character; không khoảng trắng đầu/cuối; case-sensitive; IVR giữ nguyên giá trị/case |
| Privacy | ID kỹ thuật, không chứa tên, số điện thoại, địa chỉ, token hoặc payload khách hàng |
| Golden Hour | Bắt buộc khi `program_code=GOLDEN_HOUR` |
| 24/7 | Field phải vắng mặt khi `program_code=TWENTY_FOUR_SEVEN`; `null` cũng không hợp lệ |
| Thời điểm phát | M3 phát sau khi phiên Golden Hour đã được mở/activate và trước khi tạo IVR task |
| Multiplicity | Một phiên có thể có nhiều task/order; vì vậy field không unique theo task và không tạo unique index phía IVR |
| Stability | Cùng một business session phải giữ nguyên ID qua retry, replay và mọi task thuộc phiên đó |
| Quan hệ ID | Không phải `task_id`, `order_id`, `Idempotency-Key`, `X-Correlation-Id`, call-job ID hoặc capacity scope ID |
| Retention | Đi theo retention/legal-hold/anonymization của owning task/job/incident; không kéo dài retention độc lập |
| Read | Internal service/audit only; không log ở message text, không đưa vào public export/UI nếu chưa có use case + permission đã ký |

M3 phải xác nhận namespace/uniqueness của chính `golden_hour_session_id`. IVR không dùng field này
làm primary key hoặc idempotency key; cùng session xuất hiện trên nhiều task là hợp lệ.

## 4. Quan hệ với `capacity_incident.session_id`

Hai field sau **khác nghĩa và phải cùng tồn tại** nếu shared contract được ký:

| Field | Owner | Nghĩa |
| --- | --- | --- |
| `capacity_incident.session_id` current | IVR scheduler/capacity/admin | ID của lần tính capacity hoặc scope incident nội bộ |
| `golden_hour_session_id` proposed | M3 / Golden Hour Core | Provenance của business session tạo ra task Golden Hour |

Quy tắc propagation đề xuất:

- Task Golden Hour: persist nguyên `golden_hour_session_id` từ wire.
- Call job: copy nguyên từ task; không đọc lại từ M3.
- Capacity incident gắn với một task/job Golden Hour: copy vào cột nullable riêng
  `golden_hour_session_id`; vẫn giữ `session_id` nội bộ.
- Incident 24/7, global admin pause, system-wide/unavailable hoặc incident không gắn đúng một task:
  `golden_hour_session_id=NULL`.
- Attempt/result/callback không cần duplicate field ở lượt này vì đã join/trace qua task/job. Muốn
  thêm lên callback phải có use case và contract M3 ký riêng; không lén mở rộng payload.

## 5. OpenAPI và migration plan sau chữ ký

### 5.1. OpenAPI

1. Thêm đúng một property `golden_hour_session_id` với constraint §3.
2. Nhánh `GOLDEN_HOUR + ONLINE` yêu cầu field.
3. Nhánh `TWENTY_FOUR_SEVEN + COD` cấm field, kể cả `null`.
4. Không nhận alias `session_id` hoặc `source_session_id`.
5. Regenerate DTO/client và cập nhật pinned contract hash/changelog/CDC.

Thêm property là additive ở schema shape, nhưng chuyển nó thành **required cho Golden Hour là
breaking đối với producer cũ**. Không được dán nhãn toàn bộ thay đổi là “non-breaking” để bỏ qua
cutover.

### 5.2. DB

Migration chỉ được tạo sau chữ ký, theo dạng additive nullable:

- `ivr_confirmation_tasks.golden_hour_session_id varchar(128) NULL`;
- `ivr_call_jobs.golden_hour_session_id varchar(128) NULL`;
- `ivr_capacity_incidents.golden_hour_session_id varchar(128) NULL`;
- giữ nguyên `ivr_capacity_incidents.session_id` và dữ liệu current;
- không backfill từ task ID, correlation ID, order ID hoặc capacity `session_id`;
- không unique index; chỉ cân nhắc non-unique query index khi có query/use case đo được.

Cutover hai pha:

1. **Store phase:** schema nullable + runtime accept/store; producer M3 bắt đầu gửi; đo missing/invalid.
2. **Enforce phase:** sau CDC/shared E2E và xác nhận không còn producer cũ, Golden Hour missing bị
   reject; 24/7 carrying field bị reject.

Rollback phải bỏ được cột mới mà không đụng `session_id` current. Target-DB inventory vẫn bắt buộc
trước migration; không sửa migration đã phát hành.

## 6. CDC/test bắt buộc

### Producer/contract

- Golden Hour hợp lệ có `golden_hour_session_id` → accept theo các gate còn lại.
- Golden Hour thiếu/null/rỗng/quá 128/control/edge whitespace → reject ở enforce phase.
- 24/7 không có field → hợp lệ; có field hoặc `null` → reject.
- `session_id`/`source_session_id` → reject do closed schema.
- Cùng idempotency key + cùng body/session → replay kết quả cũ; đổi session trong body → conflict.

### Persistence/propagation

- Task và job giữ đúng cùng một giá trị/case sau JSON decoding; IVR không normalize.
- Task-scoped Golden Hour incident có cả internal `session_id` và upstream
  `golden_hour_session_id`, hai giá trị không bị gộp.
- 24/7 incident, `ADMIN-QUEUE`, scheduler unavailable và system/global incident giữ upstream field
  null.
- Existing capacity/scheduler/admin tests phải chứng minh `SessionId` current không đổi semantics.
- Migration empty DB, upgrade current schema, rollback/recreate và target-DB preflight đều pass.

### Shared evidence

- M3 producer commit + generated client revision + CDC report.
- M8 consumer commit + OpenAPI diff + migration evidence.
- Shared E2E cho ít nhất một Golden Hour, một 24/7, replay, changed-session conflict và một
  capacity incident.
- Evidence phải ghi exact SHA hai repo; local fixture không thay shared E2E.

## 7. Phản hồi bị từ chối

- “Cứ dùng `session_id` cho gọn” nhưng không xử lý các internal/global writer current.
- Gửi đồng thời `session_id` và `golden_hour_session_id`, rồi để IVR tự chọn.
- Suy `golden_hour_session_id` từ task/correlation/order/capacity ID.
- Cho 24/7 gửi một ID tuỳ ý để schema nhìn cân đối.
- Sửa OpenAPI/DB trước, xin chữ ký sau.
- Gọi required-field cutover là additive/non-breaking.
- Dùng chữ ký M8 để tuyên bố M3 đã đồng ý hoặc shared integration đã xong.

## 8. M3 phải phản hồi đúng mẫu

| Mục | M3 trả lời |
| --- | --- |
| `golden_hour_session_id` name/type/length/format | `ACCEPT` / `REJECT + exact replacement` |
| GH required, 24/7 prohibited | `ACCEPT` / `REJECT + business source` |
| Owner, issue point, namespace, stability | Exact service/step/rule |
| Producer commit/client revision | Link + SHA |
| Store/enforce cutover | Mốc, rollback và owner |
| CDC/shared E2E | Link report + exact SHA hai repo |
| Người ký/ngày/phạm vi | Bắt buộc |

“OK”, comment miệng hoặc bảng không có signer/commit/CDC không phải contract acceptance.

## 9. Exit status

Phần local của M8-06 hoàn tất khi audit, đề xuất, stop rule, handoff và docs gates pass. Trạng thái
bàn giao vẫn là:

**`M8_POSITION_SIGNED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED / SHARED_E2E_NOT_RUN`**

Không nâng `ACCEPTED`, không đóng `G-CONTRACT`, không sửa wire/DB và không cho phép real customer
call khi external artifact chưa đủ.

## 10. Chữ ký

| Bên | Người ký | Ngày | Phạm vi |
| --- | --- | --- | --- |
| Module 8 / Project Owner | **Tôi — Module 8 / Project Owner** | **2026-09-03** | Đề xuất field, semantics, propagation, rollout và stop rule phía M8 |
| Module 3 / Golden Hour contract owner | `<chưa nhận>` | `<chưa nhận>` | Field source, namespace, producer, program semantics và cutover |
| Security/Privacy nếu ID không thuần opaque | `<chưa nhận>` | `<chưa nhận>` | Data classification, read/log/retention boundary |
