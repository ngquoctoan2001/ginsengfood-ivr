# W-0124 — Evidence: khắc phục phát hiện rà soát W-0123

Ngày: `2026-08-27`

Trạng thái: `TESTS_PASS` local · external gates của `W-0123` **không** được đóng bởi work này

Baseline: `main@ef09a062597f8f43dad41be751ace03ef5f5973f` + worktree `W-0123` chưa commit

Authority: `OD-18` không đổi. `W-0124` không đảo quyết định nào, chỉ đóng sáu phát hiện của lượt
rà soát.

## 1. Lượt rà soát đã xác minh được gì

Rà soát chạy lại độc lập thay vì đọc evidence. Không con số nào trong `W-0123` sai:

| Gate | Evidence khai | Chạy lại | Khớp |
| --- | --- | --- | --- |
| `dotnet build` (thêm `-warnaserror`) | 0 warning/0 error | exit `0` | ✅ |
| `dotnet test Ivr.sln` | 801/801 | 496 + 273 + 24 + 8, 0 failed | ✅ |
| admin UI | typecheck + 223/223 + build | `tsc` exit `0`, Vitest 223/223, `next build` exit `0` | ✅ |
| 7 node gate | PASS | tất cả exit `0`; traceability `484`, hash pinned `3`, portal `12` | ✅ |
| oasdiff `draft.20 → draft.21` | exit `0` | exit `0` kể cả `--fail-on WARN` | ✅ |
| oasdiff cumulative | exit `1`, pre-existing | exit `1` — **và tại `HEAD` cũng exit `1`, cùng đúng một warning** | ✅ |
| changelog regen + `CT-DOC-02` | current | `diff -u` khớp cả hai file; selftest PASS | ✅ |
| GitNexus detect_changes | LOW 151/75/0 | y hệt | ✅ |

Kiểm tra thêm ngoài bảng số, tất cả đều xác nhận cutover đúng:

- `grep '"SKIPPED"'` trong `src/` (trừ migration) → **0**. Không còn write path nào.
- `ivr_confirmation_required=false` vẫn bị `ProgramPaymentPolicy.EnsureAllowed` reject tại intake,
  nên tiền đề "task M3 gửi = lệnh gọi" được **enforce**, không phải giả định — dòng hardcode
  `IvrConfirmationRequired = true` khi persist do đó không tạo lỗ hổng.
- `Advisories` chỉ đi vào evidence JSON nội bộ, không nằm trong wire contract ⇒ làm rỗng không phá
  consumer M3.
- Admin read trả `eligibility_decision` dạng string thô, không `Enum.Parse` ⇒ row lịch sử render
  được bằng nhãn legacy.
- Phân loại vocabulary: trong file tracked (trừ portal `docs/api/`), chỉ **một** file không mang
  nhãn inline — `docs/evidence/W-0015/migration-up.sql`, vốn là historical evidence theo vị trí.

## 2. F1 — Đo delta hành vi thật

Vấn đề: `OD-18` chỉ đổi hành vi theo một chiều — đơn trước kia *có thể* bị skip nay **sẽ bị gọi**.
`W-0123` lập luận rằng hiện chưa ai bị skip vì M3 chưa gửi `trust.risk_evidence_available`. Lập luận
đúng nhưng là **suy luận**: target DB `ENV_BLOCKED`, không có gì để đếm.

Khắc phục:

| Hạng mục | Nội dung |
| --- | --- |
| Instrument | `ivr_legacy_skip_candidate_total` (`IvrTelemetry`), tag program/payment/decision |
| Call site | `TaskIntakeService.IntakeAsync`, cùng exit và cùng tag với `RecordIntakeDecision` |
| Predicate | không veto `trusted_skip_allowed=false` · `risk_flags` rỗng · `trust.risk_evidence_available=true` |
| Snapshot hỏng/thiếu | trả `false` — predicate cũ cũng đòi bằng chứng dương |
| Dashboard | panel 11 `ivr-slo-health.json`, `increase(...[1h])` theo `ivr_program` |
| Runbook | `docs/slo.md#legacy-skip-candidates` |
| Alert | **không** — cố ý |

Hai lựa chọn thiết kế đáng ghi:

**Ghi ở intake, không ở eligibility.** Đọc trust metadata để *quyết định* là điều `OD-18` cấm; đọc
để *đếm producer đã gửi gì* là kiểm toán. `UT-M3-AUTHORITY-11` giữ hai việc đó tách nhau theo file,
nên counter không thể trôi ngược vào decision path.

**Không alert.** Non-zero không phải lỗi phía IVR và không có hành động runtime nào lúc 2 giờ sáng —
nó nghĩa là M3 vẫn gửi tín hiệu đã được yêu cầu ngừng gửi, việc cần làm là trao đổi tích hợp theo
`IR-06`. Nhưng mỗi increment là một cuộc gọi thật tới khách thật, nên nó lên dashboard chứ không nằm
trong log.

Test `IT-M3-AUTHORITY-12` khẳng định cả hai vế trên cùng một payload:

- payload mang đúng hình dạng skip cũ → **được đếm** và vẫn `TASK_ACCEPTED_DRY_RUN_ONLY` + tạo
  CallJob (chính payload này trước `OD-18` kết thúc bằng `TASK_SKIPPED_TRUSTED_CUSTOMER`);
- payload có `risk_flags` → **không** được đếm. Predicate cũ cũng coi risk flag là "phải gọi", nên
  không có nó counter sẽ báo cả lưu lượng intake thay vì báo delta.

## 3. F2 — CI gate đỏ sẵn

Xác minh vấn đề trước khi sửa, bằng cùng image pinned theo digest:

| So sánh | Exit |
| --- | --- |
| `draft.2 → draft.21` (worktree W-0123) `--fail-on WARN` | `1` |
| `draft.2 → draft.20` (**tại `HEAD`, trước W-0123**) `--fail-on WARN` | `1` |
| `draft.20 → draft.21` | `0` |

Cùng đúng một warning: `request-property-removed: sellable_status` trên `POST /tasks`. Vậy job
`api_contract_diff` (`allow_failure: false`) đã đỏ **trước** `W-0123`, vì `OD-17` remove
`sellable_status` **có owner duyệt**. Mọi push đều fail, và không work item nào sau này làm nó xanh
được.

Khắc phục — xoay baseline theo đúng quy trình `docs/contracts/openapi-codegen.md` và tiền lệ
`1.0.0 → draft.2`:

| Bước | Kết quả |
| --- | --- |
| Baseline mới `baselines/ivr-order-confirmation.v1.0.0-draft.20.yaml` | sha256 `e41b0fb9…` — khớp đúng hash contract đã pin trước khi `W-0123` đổi |
| Đóng băng cửa sổ cũ | `docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.2-to-v1.0.0-draft.20.md`, 141 dòng, **giữ nguyên** phần ghi removal `sellable_status` |
| Repoint | `changelog-baseline.json` + `deploy/ci/docs.gitlab-ci.yml` |
| Portal | `build-api-docs.mjs` thêm báo cáo đóng băng; `API_DOCS_GENERATED` 12 → 13 |
| Tài liệu | `docs/api-changelog.md`, `docs/contracts/openapi-codegen.md` ghi rõ lý do xoay |

Baseline `draft.2` **được giữ nguyên**, chỉ thôi là cửa so sánh hiện hành. Xoay baseline là cách một
breaking change **đã duyệt** thôi che breaking change **chưa duyệt** kế tiếp; xoá báo cáo mà nó xoay
qua mới là cách đánh mất chính sự phê duyệt đó.

Chạy lại **toàn bộ** job `api_contract_diff` trong container pinned — cả 8 bước gồm hai lần
`diff -u` changelog và `selftest-oasdiff.sh`:

```
API_CONTRACT_DIFF_JOB_PASS
JOB_EXIT=0
```

## 4. F3 — Guard chống tái phát

`UT-M3-AUTHORITY-02` chỉ bắt kiểu tái lập dựng lại đúng type cũ. Hai guard mới không phụ thuộc tên
type:

| Test | Khoá điều gì |
| --- | --- |
| `UT-M3-AUTHORITY-10` | `EligibilityDecisions` khai báo đúng 5 hằng; `Evaluate` trên ma trận hoán vị mọi trục domain còn thấy được không bao giờ ra ngoài 4 outcome. Ma trận phải chạm cả nhánh eligible lẫn nhánh chặn, nên một rule chặn tất cả cũng đỏ |
| `UT-M3-AUTHORITY-11` | `EligibilityRules.cs`/`EligibilityService.cs` không chứa tên field trust; `RiskFlagsJson` chỉ được đọc trên dòng đồng thời nhắc `RiskScore` |

`RiskFlagsJson` không cấm thẳng được: `OD-18` giữ nó làm scheduler priority và
`SchedulerEligibilityCapacityProvider` nằm cùng file với eligibility service. Guard vì thế bound
**chỗ** được đọc chứ không bound **có được đọc hay không** — đó là ranh giới thật, và nó khớp chính
xác cái `OD-18` cho phép.

## 5. Mutation evidence

Ba mutation, mỗi cái tấn công một guard mới:

| Mutation tạm thời | Gate bị đỏ |
| --- | --- |
| Thêm hằng `TASK_SKIPPED_TRUSTED_CUSTOMER` vào `EligibilityDecisions` | `UT-M3-AUTHORITY-10` — `Assert.Equal() Failure: Collections differ` |
| Đọc `RiskFlagsJson` vào `EligibilityService.Map` | `UT-M3-AUTHORITY-11` — `Assert.All() Failure: 1 out of 2 items` |
| Bỏ loại trừ `risk_flags` khỏi predicate đếm | `IT-M3-AUTHORITY-12` — `Assert.Equal() Failure: Collections differ` |

Cả ba được revert ngay sau lượt chạy; rebuild sau revert `0 warning/0 error` và test xanh lại.

## 6. F4/F5/F6

| ID | Khắc phục |
| --- | --- |
| F4 | `docs/evidence/W-0123/README.md` §7: bảng ba thay đổi ngoài scope, lý do có mặt, lý do không revert, và lệnh tách ba commit. Không revert vì cả ba đang giữ gate xanh |
| F5 | `specs/api/06-error-codes.md` §1b viết lại dòng mapping `SKIPPED`; `specs/workflows/07-trusted-skip.md` **không** đổi tên, ghi lý do giữ ID ổn định ngay đầu file |
| F6 | `.gitattributes` thêm `deploy/ci/scripts/*.sh text eol=lf` + renormalize worktree; plan `W-0123` §5.14 đổi sang `npm --prefix admin-ui`; `admin-ui/.gitignore` chặn hai file pnpm bootstrap |

Về F5, đổi tên file là lựa chọn **bị từ chối có lý do**: `workflows/07` đang được
`specs/api/06-error-codes.md`, `specs/workflows/00-index.md` và evidence/tracker lịch sử
(`W-0118`, `W-0123`) trỏ tới. Đổi tên sẽ làm hỏng chính những bản ghi audit mà repo cấm viết lại —
cùng đúng lý do enum `TASK_SKIPPED_TRUSTED_CUSTOMER` được giữ tên.

Về F6, `selftest-oasdiff.sh` nay chạy thẳng từ worktree trong container, không cần bước
`tr -d '\r'` mà `W-0123` phải dùng:

```
CT-DOC-02 PASS — oasdiff rejected the removed operation fixture
```

## 7. GitNexus impact trước khi sửa

| Symbol | Risk | Blast radius |
| --- | --- | --- |
| `IvrTelemetry` | `LOW` | 0 upstream; thay đổi thuần additive |
| `TaskIntakeService` | `LOW` | 14 affected, 4 direct, 0 execution flow, 2 module |

Không symbol nào `HIGH`/`CRITICAL`, nên không có điểm dừng bắt buộc trước khi sửa theo `AGENTS.md`.

**Sau khi sửa, `detect_changes` lại trả `HIGH` — phải nói rõ chứ không lướt qua.** Con số:
`200` symbol / `85` file / **`12` affected process**, so với `LOW` và `0` process của `W-0123`.

Toàn bộ mức tăng đến từ **một** symbol: `IntakeAsync`, xuất hiện ở **step 2** của 12 flow
`HandleAsync → …`. Đó là đường intake chính, nên bất kỳ chỉnh sửa nào ở đó cũng kéo risk lên —
graph đúng khi cảnh báo.

Vì sao vẫn chấp nhận được, và điều gì có thể sai:

| Câu hỏi | Trả lời |
| --- | --- |
| Có đổi được `outcome` không? | Không. Đoạn thêm nằm **sau** `store.ExecuteAsync(...)` đã trả về và sau hai lệnh telemetry sẵn có; nó chỉ đọc `command.Source` và không ghi gì |
| Rủi ro thật nằm ở đâu? | Một exception ném ra **sau** khi intake đã commit sẽ biến một lượt intake thành công thành `500` |
| Chặn thế nào? | `SerializeToElement` được bọc `JsonException` + `NotSupportedException` (đúng bề mặt ném có tài liệu); `source.Risk_flags is { Count: > 0 }` là pattern null-safe; `ReadLegacyTrustMetadata` chỉ đọc property |
| Bằng chứng chạy thật | `274/274` integration test — trong đó có toàn bộ `IT-INTAKE-*` đi qua đúng đường HTTP intake này — đều xanh |

Đây là đánh đổi có chủ ý: counter phải nằm trên đường intake mới đếm được thứ nó tồn tại để đếm.
Chỗ duy nhất an toàn hơn là eligibility path, và đó lại đúng chỗ `OD-18` cấm đọc trust metadata.

## 8. Regression

| Gate | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | PASS, 0 warning/0 error |
| `dotnet test Ivr.sln` | PASS `804/804`: unit 498, integration 274, contract 24, chaos 8 |
| Traceability | `TEST_TRACEABILITY_CURRENT=487` (+3: `UT-M3-AUTHORITY-10/11`, `IT-M3-AUTHORITY-12`) |
| OpenAPI lint/validate/negative/drift | PASS; hash pinned `3`, human diff current |
| docs build/self-test | PASS; portal `13` artifact, PII/boundary/link/CI topology xanh |
| Job `api_contract_diff` đầy đủ | exit `0` (trước đó exit `1` cả ở `HEAD` lẫn `W-0123`) |
| config self-test | PASS |
| admin UI | `tsc` exit `0`; Vitest `223/223`; `next build` exit `0` |
| GitNexus detect changes | risk `HIGH` — `200`/`85`/`12 process`, toàn bộ do `IntakeAsync`; phân tích ở §7 |

Lần chạy full đầu tiên đỏ đúng một test — `UT-TRACE-01`, vì bảng traceability chưa chứa ba test mới.
Sau `traceability:write` và review diff (`484 → 487`, đúng ba dòng mới) thì xanh lại. Ghi ra đây
chứ không giấu: đó là gate làm đúng việc của nó.

## 9. Gate KHÔNG được đóng bởi W-0124

| Gate | Trạng thái | Lý do |
| --- | --- | --- |
| M3 producer/consumer usage + sign-off | `OWNER_DATA_REQUIRED` | Vẫn chưa có commit/OpenAPI/capture hay chữ ký M3. Counter F1 đo được delta **khi đã deploy**, nó không thay cho chữ ký |
| Target/staging/production DB preflight | `ENV_BLOCKED` | Vẫn không có endpoint/credential |
| Hosted GitLab CI | `NOT_RUN` | Không push trong lượt này. `api_contract_diff` xanh **cục bộ trong container pinned** không phải là một pipeline run thật |
| Real customer call | `NO` | Không cần và không được mở |

`W-0124` làm `api_contract_diff` có thể xanh; nó không chứng minh pipeline đã chạy.
