# W-0146 — M8-06 upstream session trace sign-off

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`M8_POSITION_SIGNED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED`**

Người ký phía M8: **Tôi — Module 8 / Project Owner**.

## 1. Phạm vi

- Đối chiếu upstream session với current OpenAPI, generated DTO, domain, persistence, scheduler,
  capacity incident, admin projection, tests và tài liệu nguồn.
- Chọn một đề xuất field có nguồn phía M8; tách khỏi capacity scope ID nội bộ.
- Lập OpenAPI/migration/cutover/CDC plan và exact handoff cho Module 3.
- Không sửa runtime, shared OpenAPI, generated code hoặc DB migration trước chữ ký M3.

## 2. Bằng chứng trực tiếp

| Phát hiện | Vị trí current |
| --- | --- |
| Task wire không có upstream session | `specs/api/openapi/ivr-order-confirmation.v1.yaml` schema `IvrConfirmationTaskV1`; generated `IvrServerModels.g.cs` |
| Domain/task/job không có field | `ConfirmationTaskSnapshot.cs`; `IvrPersistenceEntities.cs` |
| Capacity incident có `SessionId` non-null | `CapacityIncidentEntity`; `ivr_capacity_incidents.session_id` trong model snapshot |
| Capacity calculation tự sinh ID | `SchedulerCapacity.cs`: `MOCK-SCHED-*`, `SCHED-*`, unavailable marker |
| Deadline close tự sinh ID | `PostgresSchedulerStore.cs`: `SCHED-DEADLINE-{jobId}` |
| Admin global pause tự sinh ID | `InternalAdminApiService.cs`: `ADMIN-QUEUE-*`, `ProgramCode=ALL` |
| Eligibility incident copy capacity ID | `EligibilityRepository.cs`: `SessionId = capacity.SessionId` |
| Master dùng tên domain-specific | `04-MASTER-03-TRACEABILITY-ID.md`: `golden_hour_session_id` và rule thiếu ID không được claim Golden Hour |
| Phase-8 dùng generic name nhưng không định nghĩa semantics | `22-ĐƯỜNG CƠ SỞ ĐẦU VÀO IVR.md`: capacity/log chỉ liệt kê `session_id` |

GitNexus CLI query trên index cũ trả `0` flow và cảnh báo thiếu FTS. Không re-index; direct source tại
current HEAD là authority. W-0146 không sửa production symbol nên không có symbol impact-edit gate.

## 3. Quyết định phía M8

- Đề xuất duy nhất: `golden_hour_session_id`.
- Required/non-null cho `GOLDEN_HOUR`; prohibited/absent cho `TWENTY_FOUR_SEVEN`.
- M3/Golden Hour Core phát và sở hữu namespace/stability.
- IVR giữ nguyên giá trị/case sau JSON decoding; không normalize, sinh hoặc suy từ ID khác.
- `capacity_incident.session_id` tiếp tục là internal scope ID.
- Sau chữ ký mới thêm cột nullable riêng `golden_hour_session_id` vào task/job/incident theo store →
  enforce cutover; không backfill giả và không unique index.

Chi tiết: [M8-06 sign-off pack](../../../plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md).

## 4. Artifact cập nhật

- M8-06 sign-off pack.
- Worklist M8-06 row + handoff ngay dưới task.
- TODAY-01 S-04 + chữ ký/follow-up.
- IR-06 proposal, priority và M3 response checklist.
- V0.3 clean runtime boundary; functional/database capacity notes.
- Tracker/readiness/gate mirror và official Markdown map.

## 5. Kiểm chứng ngày 03/09/2026

| Gate | Kết quả |
| --- | --- |
| Source exact-search/current contract inventory | **PASS** |
| OpenAPI lint | **PASS** — 2 API descriptions valid |
| OpenAPI parse/schema/negative | **PASS** — 2 files; 9 task fixtures; schema negative 12; domain negative 13; compat 1/1 |
| OpenAPI negative selftest + pinned drift | **PASS** — invalid spec rejected; 3 hashes pinned; human diff current |
| API docs selftest | **PASS** — 14 generated artifact; boundary/link/topology/PII pass |
| Official Markdown map | **REGENERATED** — 623 Markdown file; W-0146 evidence/sign-off đều 0 unresolved; 201 global unresolved thuộc corpus rộng |
| Gate mirror/readiness | **PASS** — 11 gate, 144 work item, 23 open decision; rung 0; production flag false |
| Production/source diff | **PASS** — 0 diff dưới `src/`, `tests/`, `admin-ui/`, shared OpenAPI |
| `git diff --check` | **PASS** |

## 6. Residual external gates

- M3 signer/name/date/scope: `NOT_RECEIVED`.
- M3 producer commit/client revision/CDC: `NOT_RECEIVED`.
- OpenAPI change, generated DTO, domain/DB propagation: `NOT_STARTED / CODE_NOT_AUTHORIZED`.
- Shared E2E/target DB: `NOT_RUN`.

Do đó Target V1 vẫn `DRAFT`, `G-CONTRACT` vẫn `BLOCKED_EXTERNAL`,
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Handoff

M8 đã đưa ra một contract proposal có nguồn và đã ký stop rule. Bên giao task không được yêu cầu
IVR “cứ code trước” hoặc map vào `capacity_incident.session_id`; muốn mở code phải trả lại chữ ký M3,
producer commit, cutover và CDC đúng mẫu.

**Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.
