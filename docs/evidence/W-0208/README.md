# W-0208 — TTL `dial_token`: ba guard, một giá trị hợp lệ

Ngày: 2026-09-07 · Baseline: `main@cf4bd4a` · Trạng thái: **TESTS_PASS** cho phần tài liệu và phần
ghim; **BLOCKED_EXTERNAL** cho con số TTL.

> ## ✅ Ghi chú `W-0246` (09/09/2026) — lượt này **đúng**, và vế TTL nay đã được chốt
>
> Một lượt trung gian (`W-0245`) từng dán ở đây một banner nói tiền đề của `W-0208` là sai — rằng
> `OD-V1-17` không nêu TTL nào. **Banner đó sai và đã bị gỡ.** `+60s` nằm trong
> `specs/_review/open-decisions-register.md`, chính là nguồn mà `od-v1-signoff-2026-09-05` khai ở
> header, và là nguồn `W-0245` không kiểm.
>
> `W-0208` đúng cả tiền đề lẫn kết luận: quyết định đã ký **có** mâu thuẫn với ba guard.
>
> **Owner chốt `2026-09-09`:** TTL = **đúng** confirmation-window end, thay thế vế `+60s`. Không
> tầng nào phải sửa — `IT-INTAKE-DB-03` nay ghim một hợp đồng **đã ký** thay vì một hiện trạng.

## 1. Vấn đề

`OD-V1-17` được ký `2026-09-05` với *"TTL = cửa sổ xác nhận **+ 60s**"*. Code không thi hành được
con số đó, và không phải vì thiếu một guard — vì **thừa ba**:

| Tầng | Vị trí | Luật |
| --- | --- | --- |
| Intake | `TaskIntakeService.ContactRejectionReason` | từ chối nếu expiry **<** window end → `422` + `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` |
| Persistence | `PersistenceInvariantValidator.ValidateTask` | throw nếu expiry **>** window end |
| Dispatch | `PostgresTelephonyDispatchStore.LoadAsync` | throw nếu expiry **>** `lease.Deadline` |

`lease.Deadline` = `job.expires_at`, và intake set `job.expires_at` từ chính window end
(`TaskIntakeService` — cả task lẫn job đều nhận `snapshot.ConfirmationWindow.ExpiresAt`). Nên guard
thứ ba **lặp lại** guard thứ hai chứ không thêm biên mới — nhưng nó lặp lại ở một nơi mà việc sửa
hai tầng trên sẽ không chạm tới.

Giao của ba luật là một điểm: `dial_token_expires_at == confirmation_window_expires_at`.

**Guard thứ ba là phát hiện của Codex** trong đánh giá độc lập 07/09
([`docs/review/2026-09-07-m8-independent-full-review.md`](../../review/2026-09-07-m8-independent-full-review.md)
§F01). Bản audit chief auditor 07/09 và lượt đối chiếu code trước đó **đều chỉ thấy hai tầng** —
và một bản sửa dựa trên hai tầng sẽ đi tới production rồi hỏng lúc quay số.

## 2. Vì sao đây là lỗi tệ hơn nó trông

Hai chiều lệch **không** đối xứng:

- Sớm hơn window end → từ chối **sạch ở biên**, có mã lỗi M3 đọc được, không ghi gì.
- Muộn hơn window end → **qua contact gate**, rồi hỏng **bên trong transaction**.

Chiều thứ hai là chiều `OD-V1-17` yêu cầu. Một producer làm đúng quyết định đã ký sẽ thấy request
được nhận rồi đổ, ở một tầng không có mã lỗi hợp đồng nào — nơi tệ nhất để phát hiện một contract.

Và IR-06 nói **ba điều khác nhau** về cùng trường đó: `L219` ghi `≥`, `L522` và `L534` ghi `>`,
`L983` mô tả equality. `L522`/`L534` là câu chỉ dẫn và checklist — tức tài liệu **đang chủ động bảo
M3 gửi đúng thứ sẽ bị từ chối**.

## 3. Đã làm gì

**Không sửa một dòng runtime nào.** Đổi TTL là contract change; owner của `OD-V1-17` là
Sales/Security/Telephony, không phải M8. Việc còn lại chờ `DTK-02`/`DTK-06`.

Phần không cần chữ ký thì làm ngay:

### 3.1. Ghim hành vi — `IT-INTAKE-DB-03`

`tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs`, chạy trên Postgres thật:

| Trường hợp | Kỳ vọng | Kết quả |
| --- | --- | --- |
| `expiry = window end` | nhận | `TASK_ACCEPTED_DRY_RUN_ONLY` |
| `expiry = window end + 60s` (con số `OD-V1-17`) | throw ở persistence | `InvalidOperationException: Dial-token expiry must remain inside the confirmation window` |
| `expiry = window end − 60s` | từ chối ở intake | `TASK_REJECTED_CONTACT_INVALID` + `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` |

Cuối test khẳng định **chỉ** case equality chạm được database.

Test này **cố ý sẽ đỏ** khi TTL đổi. Đó là mục đích: nó biến một mâu thuẫn tài liệu thành một
assertion mà không ai relax được một tầng mà không thấy.

### 3.2. Sửa tài liệu đang gây hại

`integration-requirements/06-module-3-api-handover.md`:

- **`§3.4.1` mới** — bảng ba tầng, giao là equality, và cảnh báo `OD-V1-17` chưa thi hành được.
- `L219` — `≥` → equality, trỏ `§3.4.1`.
- `L522` — `phải lớn hơn` → equality, nêu rõ chiều muộn hỏng ở tầng dưới.
- `L534` — checklist M3 tick: `>` → `=`.
- `§6` — thêm guard dispatch vào correction `W-0150`; thêm correction `W-0208` cho `OD-V1-17`.

`specs/_review/open-decisions-register.md` — thêm `Sửa 2026-09-07 (W-0208)` vào dòng `OD-V1-17`:
vế TTL chưa thi hành được, đã ghim bằng test; phần token dùng lại/binding/trần resolve **đã** có
(`W-0199`), chỉ con số TTL treo. **Không đổi trạng thái dòng** — tách trạng thái cho dòng có owner
ngoài M8 là việc của chief auditor (mục A2 của worklist).

### 3.3. Cố ý chưa làm — OpenAPI

`dial_token_expires_at` vẫn là `{ type: string, format: date-time }`, không mô tả ràng buộc.

Sửa YAML sẽ bump hash đã ghim và cần re-pin, mà re-pin là hành động có review (`W-0204`). Đúng thứ
tự là gộp nó vào lượt sửa cùng lúc khi TTL được chốt — một re-pin cho một quyết định, không phải
hai. Chú thích của chính `contract-freeze-verifier.mjs` nói cùng hướng: *"Owners approve
`06-module-3-api-handover.md`, not the YAML."*

## 4. Kiểm chứng

```text
dotnet test Ivr.sln                     892/892 PASS, 0 failed, 0 skipped
                                        (Unit 593 · Integration 266→267 · Contract 24 · Chaos 8)
dotnet test --filter IT-INTAKE-DB-03    1/1 PASS
contract-freeze-verifier.mjs            CONTRACT_FREEZE=PASS · intake=1.0.0-draft.23 required=22
openapi-contract-drift.mjs              OPENAPI_HASHES_PINNED=3 · HUMAN_DIFF_CURRENT=YES
generate-test-traceability.mjs --check  TEST_TRACEABILITY_CURRENT=551  (550→551)
docs-selftest.mjs                       API_DOCS_SELFTEST_PASS
gitnexus impact (scope=all)             risk=low · 0 affected processes
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`. TARGET_V1 delivery vẫn bị
`CallbackDeliveryOptions` chặn.

## 5. Còn lại

1. M3/Security chốt TTL (`DTK-02`/`DTK-06`). Hai lối: giữ equality và **sửa `OD-V1-17`**, hoặc lấy
   `+60s` và sửa **cả ba guard cùng lúc** + OAS + CDC.
2. Khi chốt: `IT-INTAKE-DB-03` sẽ đỏ. Sửa nó có chủ đích, đừng xóa.
3. Guard dispatch cần một test riêng khi nó không còn bị guard persistence che — hiện không thể
   dựng case chạm tới nó, vì persistence chặn trước.
