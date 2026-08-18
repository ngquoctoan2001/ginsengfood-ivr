# W-0030 — Evidence: Sales eligibility/blocker snapshot contract (`P4-2`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (mock-only — trần của slice này theo DoD `P4-2` §5)

## 1. Vấn đề slice này giải

Trước W-0030, `eligibility_snapshot` — field **bắt buộc** trên task — được kiểm đúng một thứ: nó có phải object không (`TaskIntakeEndpoint.cs:274`, `EnsureObject`). Sau đó `EligibilityService` đọc đúng **một** khoá bên trong, `decision`. Mọi thứ còn lại bị bỏ qua.

Nghĩa là Sales có thể gửi một túi JSON bất kỳ chứa `{"decision":"ELIGIBLE"}` và IVR sẽ quay số, không biết bằng chứng đó được chụp lúc nào, từ nguồn phiên bản nào, hay nguồn có sống không.

Đây chính là mục T-02 §2(b) trong [closure pack](../../contracts/target-v1-closure-pack/T-02-task-data-order-version.md): thứ bắt buộc thì không có type, thứ có type đầy đủ (`sellable_status[]`) lại optional.

## 2. Quyết định thiết kế: linked evidence reference, không siết contract

`P4-2` §2.1 cho hai lối: định nghĩa shape **trong Target task contract** *hoặc* trong **linked evidence reference**. Chọn lối thứ hai, có chủ ý:

| Lý do | Chi tiết |
| --- | --- |
| Shape chưa được owner duyệt | `OD-V1-03` còn mở. Đưa một shape bắt buộc vào OpenAPI là ngầm tuyên bố Sales đã đồng ý — đúng thứ `P4-1` §2 cấm ("never invent it"). |
| Siết schema là breaking | `docs/api-versioning.md:16` xếp "add a required request field or tighten validation" vào breaking. Cổng `oasdiff breaking --fail-on WARN` sẽ đỏ. |
| Không mất gì về mặt bảo đảm | Bảo đảm runtime nằm ở validation fail-closed, không ở chỗ shape được khai. |
| Mã lỗi hữu ích hơn | Từ chối bằng schema cho lỗi validation chung chung; từ chối trong code cho `ELIGIBILITY_SOURCE_VERSION_MISSING` — Sales biết chính xác thiếu gì. |

Kết quả: shape công bố ở [`specs/api/evidence/eligibility-snapshot.v1.schema.json`](../../../specs/api/evidence/eligibility-snapshot.v1.schema.json), trạng thái ghi thẳng trong file là `TARGET_DRAFT_NOT_OWNER_APPROVED`. OpenAPI chỉ thêm **description** trỏ tới nó — thay đổi thuần tài liệu.

**`oasdiff breaking` xác nhận: no breaking changes** (draft.2 → draft.7).

## 3. Đã xây gì

**Validate fail-closed theo thứ tự cấu trúc trước, nội dung sau** (`EligibilityRules.EvaluateSourceEligibility`):

| Thứ tự | Điều kiện | Reason code | Quyết định |
| --- | --- | --- | --- |
| 1 | không gửi gì | `ELIGIBILITY_SNAPSHOT_MISSING` | held |
| 2 | gửi thứ IVR không đọc được | `ELIGIBILITY_SNAPSHOT_UNREADABLE` | held |
| 3 | `source_available: false` | `ELIGIBILITY_SOURCE_UNAVAILABLE` | held |
| 4 | thiếu `source_version` | `ELIGIBILITY_SOURCE_VERSION_MISSING` | held |
| 5 | `captured_at` thiếu hoặc ngoài cửa sổ | `ELIGIBILITY_SNAPSHOT_STALE` | held |
| 6 | thiếu `decision` | `ELIGIBILITY_SNAPSHOT_MISSING` | held |
| 7 | `decision` không phải `ELIGIBLE` | `..._BLOCKED` / `..._UNKNOWN` | blocked / held |
| 8 | `decision=ELIGIBLE` **nhưng** `blockers[]` không rỗng | `ELIGIBILITY_SNAPSHOT_BLOCKED` | blocked |

Hai điểm đáng nói:

**Thứ tự cấu trúc trước nội dung là có chủ ý.** Một snapshot IVR không đọc được không được phép hiện ra như "một quyết định IVR không đồng ý" — hai chuyện khác nhau, cần hành động khác nhau. `UT-ELIG-EVIDENCE-13` khoá tính chất này bằng một snapshot vi phạm cùng lúc 4 điều kiện.

**Bước 8 là bước duy nhất không suy ra được từ prompt.** Nếu Sales ghi `ELIGIBLE` ở field tóm tắt mà vẫn liệt kê blocker ở phần chi tiết, tin field tóm tắt là quay số lên đúng đơn mà Sales tự gắn cờ. Chặn.

**Hash bất biến, nối vào evidence** (`P4-2` §2.3):

- Intake tính `DeterministicSnapshotHasher.Compute(eligibilityJson)` và lưu vào cột mới `ivr_confirmation_tasks.eligibility_snapshot_hash`.
- CHECK constraint `ck_ivr_confirmation_tasks_eligibility_hash` chỉ cho hex thường 64 ký tự — cột này **không thể** thành chỗ chứa evidence thứ hai.
- Khi đánh giá đạt, evidence ref `…#eligibility/snapshot/<hash>` được thêm vào `EligibilityEvaluation.EvidenceRefs`, nên callback kết quả truy được về đúng bó bằng chứng đã dùng để quyết định. **Chỉ digest đi theo, không bao giờ là thân snapshot.**

## 4. Test

| Test | Khẳng định |
| --- | --- |
| `UT-ELIG-EVIDENCE-10` | 4 dạng hỏng cấu trúc → đúng reason code, không cái nào dispatch, không cái nào tính lượt khách |
| `UT-ELIG-EVIDENCE-11` | không dấu thời gian / trước cửa sổ / ở tương lai → đều `STALE`, đều held |
| `UT-ELIG-EVIDENCE-12` | `ELIGIBLE` + blockers → vẫn blocked, signal `eligibility.snapshot.blockers` |
| `UT-ELIG-EVIDENCE-13` | vi phạm chồng nhau → báo lỗi **cấu trúc**, không báo lỗi nội dung |
| `UT-ELIG-EVIDENCE-14` | đạt → evidence ref mang hash; không có hash (row cũ trước migration) vẫn đạt, chỉ mất khả năng truy vết |
| `UT-ARCH-NO-OPS-EGRESS-05` | toàn bộ bề mặt HTTP ra ngoài đúng 2 client Sales callback; không có key/kiểu nào cho Ops client, webhook hay credential |
| `IT-ELIG-EVIDENCE-10` | pass / block / stale / source-unavailable / unreadable chạy thật trên Postgres; hash lưu **cho cả case bị từ chối** |
| `IT-ELIG-EVIDENCE-11` | database từ chối hex hoa, digest cụt và cả một thân snapshot ném vào cột hash |
| `IT-ELIG-RACE-12` | blocker phát sinh **sau khi khách bấm 1** |

`UT-ARCH-NO-OPS-EGRESS-05` là cách biến `P4-2` §2.6 ("IVR must not become a second Ops orchestrator") từ một câu trong tài liệu thành thứ CI bắt được. Thêm một `AddHttpClient` mới ở `src/` là test đỏ.

`IT-ELIG-RACE-12` phủ kịch bản `SCN-009-race-recall-after-key1` trong seed — kịch bản này **trước đây không có test nào**. Nó khẳng định điều tế nhị nhất của D-02: khách đã bấm `1` là một **sự kiện IVR quan sát được**; Sales chặn đơn sau đó không làm cái bấm phím ấy thành cái khác. Nên `IVR_CONFIRMED` giữ nguyên, `is_counted_customer_attempt` giữ nguyên, block ghi lên **tín hiệu** chứ không ghi đè **quan sát**, và task vẫn mang đúng `order_state` Sales gửi lúc intake.

## 5. Fixture đã sửa, rule không nới

Fixture integration cũ mang `{"decision":"ELIGIBLE"}` — dưới luật mới nó thiếu `source_version` và `captured_at` nên sẽ bị held. **Sửa fixture, không nới luật.** Fixture cũ được viết khi luật chưa tồn tại; giờ luật tồn tại thì fixture phải cấp bằng chứng hợp lệ.

Cùng loại với một lỗi trong test tôi tự viết: helper `Evidence(capturedAt: null)` rơi vào nhánh `?? mặc định hợp lệ`, nên case "không có dấu thời gian" thực ra vẫn có dấu thời gian tươi và chứng minh **sai** điều nó tuyên bố. Sửa bằng cờ `capturedAtMissing` riêng.

## 6. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | 0 warning / 0 error |
| `dotnet test Ivr.sln` | xem §9 |
| `oasdiff breaking` (draft.2 → draft.7) | **No breaking changes** |
| `oasdiff changelog` + `diff -u` (đúng lệnh CI, chạy trong image đã ghim) | `CHANGELOG_MATCH_IVR`, `CHANGELOG_MATCH_CALLBACK` |
| `openapi-contract-drift.mjs` | `OPENAPI_HASHES_PINNED=3`, `OPENAPI_HUMAN_DIFF_CURRENT=YES` |
| `validate-openapi.mjs` | schema hợp lệ; negative case vẫn bị từ chối |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` (portal HTML đã sinh lại sau khi OpenAPI đổi) |
| `impact` (GitNexus) | MEDIUM, 59 symbol, 14 direct, **0 execution flow** |
| `detect_changes` (GitNexus) | risk **low**, 53 symbol / 14 file, **0 affected process** |

Index GitNexus lệch từ `2026-08-13` nên đã chạy lại `node .gitnexus/run.cjs analyze` trước khi phân tích — index cũ không có cả `EligibilityRules`. Index mới: 42.467 node / 52.150 edge.

## 7. Cái này KHÔNG chứng minh

- **Không đóng `W-0002`, `W-0005` hay `OD-V1-03`.** Shape ở `specs/api/evidence/` là **đề xuất của IVR**, không phải hợp đồng đã ký. Sales trả lời khác thì file này đổi theo.
- **Không có sandbox thật.** Toàn bộ evidence là fake/mock; real sandbox là hạng mục riêng, `NOT_RUN`.
- **Không bật real provider, không gọi khách.** `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- **Không đổi order state.** IVR vẫn không có bề mặt nào ghi được trạng thái đơn (D-02).
- **Không đưa credential/client/webhook Ops nào vào.** `UT-ARCH-NO-OPS-EGRESS-05` khoá điều này.
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

## 8. Việc kế tiếp

| Việc | Ai |
| --- | --- |
| Gửi [T-02](../../contracts/target-v1-closure-pack/T-02-task-data-order-version.md) cho Sales Core để đóng `OD-V1-03` | owner IVR |
| Khi Sales chốt shape: cập nhật `eligibility-snapshot.v1.schema.json`, rule, fixture cùng lúc | IVR |
| `P4-3` (`W-0031`) — voice restriction / trust snapshot | tiếp theo trong lộ trình |

## 9. Số liệu test

`dotnet test Ivr.sln` — **315/315 pass, 0 fail, 0 skip**:

| Project | Trước W-0030 | Sau | Thêm |
| --- | ---: | ---: | ---: |
| `Ivr.ContractTests` | 21 | 21 | 0 |
| `Ivr.UnitTests` | 168 | 174 | +6 |
| `Ivr.IntegrationTests` | 113 | 120 | +7 |
| **Tổng** | **302** | **315** | **+13** |

Không test cũ nào bị sửa để hợp với luật mới. Hai thứ **có** đổi và đổi có lý do, ghi ở §5: fixture integration được cấp bằng chứng hợp lệ (fixture sai, không phải luật sai), và helper test của chính tôi được sửa vì nó chứng minh sai điều nó tuyên bố.
