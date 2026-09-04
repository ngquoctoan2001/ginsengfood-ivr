# W-0177 — Exact local candidate freeze and verification

Ngày: `2026-09-04`
Rejected candidates: `f4201b1c9ec31790ae657f32cabd51918344f5e3`,
`973df3c50554125f7b96892d4a0ea3a84d779cc9`,
`0ae156d7d6e7dd424e362f8ee8e19ccffd2f2fe6`
Verified candidate: `59597e20246072ddc4309051e9903c264274e4d1`
Tree: `7054ee4313e11b6d1f7589fe2261d4ee323ff9f7`
Trạng thái: `TESTS_PASS_LOCAL / EXACT_CANDIDATE_VERIFIED / HOSTED_CI_AUTH_BLOCKED / NO_PUSH`.

## 1. Phạm vi

- đóng băng W-0170/W-0172..W-0176 thành commit local bất biến;
- kiểm tra từ detached clean worktree với `core.autocrlf=true`;
- chạy validator/hash/docs/security và full .NET solution gồm PostgreSQL/Chaos khi Docker sẵn sàng;
- không push, không trigger hosted CI, không dispatch external decision và không gỡ production guard.

## 2. Finding từ exact checkout

Candidate đầu gồm `f4201b1` và LF follow-up `973df3c`. Detached checkout đúng `973df3c` cho thấy:

- W-0164 và W-0174 self-test vẫn PASS;
- W-0165 từ chối `R-00-voice-gateway-rfq.md` vì artifact manifest còn pin byte CRLF cũ;
- W-0170 từ chối downstream vì prerequisite W-0165 đỏ;
- scan đủ 18 manifest member xác nhận chỉ `R-00` và `R-06` lệch;
- committed/physical blob đã là LF đúng `.gitattributes`; lỗi nằm ở hash chain chưa được xoay sau khi
  hai file được thêm vào LF policy, không phải semantic content drift.

Hai SHA-256 canonical LF:

| Artifact | SHA-256 |
| --- | --- |
| `R-00-voice-gateway-rfq.md` | `ae483fa33e5741b49a0d6c3e71bfedf51df14eec46d2a69935f45809f0e7ab37` |
| `R-06-to-trinh-mua-thiet-bi.md` | `341153903009c37fb7fcf5ea3c5bdb253a6c7cd49f908676ff7c0612e681eea4` |

## 3. Bounded remediation

Hash được xoay đúng dependency:

1. hai source row trong M8-12;
2. M8-12 và hai source row trong manifest 18 member;
3. M8-12/manifest pin trong năm message D-01..D-05 của M8-13;
4. source pins và pending template của W-0164/W-0165;
5. source pins và pending template của W-0170.

Không sửa validator algorithm/schema, source contract, runtime, DB, callback, scheduler hoặc safety
guard. GitNexus impact của ba `SOURCE_PINS` đều **LOW**, `0` direct caller, `0` process/module.

## 4. Verification đang thực hiện

| Gate | Kết quả hiện tại |
| --- | --- |
| W-0164 | **PASS** `template=1 valid=2 refusals=19` |
| W-0165 | **PASS** `template=1 valid=2 refusals=27` sau re-pin |
| W-0170 | **PASS** `valid=1 refusals=21` sau re-pin |
| W-0174 | **PASS** `valid=1 refusals=46` |
| Ba pending template | **PASS valid-not-ready**; không có routing/response/receipt thật |
| Detached clean candidate `0ae156d7` | **REJECTED** — W-0174 template bị checkout thành CRLF trong khi manifest pin LF |
| Full .NET | exact `59597e2`: **PASS `781/781`** — Contract 24, Unit 510, Integration 239, Chaos 8 |
| Security wrapper | **PASS** — NuGet/npm HIGH=0, Gitleaks negative control, history 159 commits |
| Hosted GitLab CI | **AUTH_BLOCKED / NOT_RUN** |

## 5. Boundary

Finding pin drift làm `973df3c` bị loại khỏi release candidacy. Shared-tree validator PASS chỉ cho phép
tạo candidate mới; không được hồi tố ghi `973df3c` là PASS. External dispatch vẫn `0/5`, mọi
M3/Product/Security/Platform/Telephony evidence và shared E2E vẫn chưa nhận.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Finding thứ hai từ exact checkout

Candidate re-pin `0ae156d7` sửa đúng W-0170 chain nhưng exact Windows checkout phát hiện
`docs/evidence/W-0174/shared-e2e-report.template.json` chưa được pin `eol=lf`:

- manifest/README W-0174 pin SHA-256 LF canonical
  `381b6b59126955182f53a90fab2c8032547f296e57a80ad9206ee54da958d91a`;
- shared tree hiện đúng LF và đúng hash trên;
- detached checkout `core.autocrlf=true` đổi file thành CRLF, SHA-256
  `0971c37ed2af098ccfe568197c472a62792c706436cf701f03b919f1df8e2f2e`;
- validator vẫn kiểm schema và trả `SHARED_E2E_TEMPLATE_VALID_NOT_READY cases=11`, nhưng manifest
  byte provenance không còn đúng, vì vậy candidate phải bị loại.

Remediation chỉ mở rộng LF policy cho `docs/evidence/W-0174/*.json` và `*.txt`; không đổi template,
manifest pin, schema, validator hoặc runtime. UI exact-checkout riêng trên `0ae156d7` đã PASS lint,
typecheck, Vitest `176/176` và production build, nhưng không thể bù cho provenance failure.

LF policy và finding này đã được đóng gói ở `1dd8dc0`; SHA đó là base cho vòng candidate tiếp theo.

## 7. Finding thứ ba: Chaos test phụ thuộc wall clock

Trên detached `f4201b1`, sau khi Integration đạt `239/239`, full Chaos đạt `7/8` và riêng
`CHAOS-DOWNSTREAM-01` tiếp tục FAIL lần hai. Batch thứ hai trả một row `RETRY_PENDING` với
`RetryCount=2` thay vì rỗng. Code runtime không đổi từ `f4201b1` qua `1dd8dc0`, nên finding này phải
được sửa trước candidate cuối.

Nguyên nhân là test gọi hành vi “immediately” nhưng dùng `TimeProvider.System`; retry mặc định chỉ
`250ms + jitter`. Các DB query/assertion giữa hai batch có thể vượt khoảng đó trên checkout sạch,
biến retry hợp lệ của runtime thành test fail. Khắc phục chỉ ở test:

- tạo `FixedTimeProvider` tại thời điểm bắt đầu scenario;
- dùng cùng clock cho `CallbackOutboxRepository`, `CallbackCircuitBreaker` và `CallbackDispatcher`;
- không đổi production dispatcher, retry delay, DB schema hay safety guard.

GitNexus trước edit cho method/class: **LOW**, `0` upstream/process/module. Post-change detect:
**MEDIUM**, `1` file, `6` symbols, `4` test flows; không có runtime flow. Verification local sau fix:
build Chaos `0 warning/0 error`, focused **1/1** và full Chaos **8/8**.

## 8. Exact candidate cuối

Candidate `59597e20246072ddc4309051e9903c264274e4d1` (tree
`7054ee4313e11b6d1f7589fe2261d4ee323ff9f7`) được checkout detached sạch với
`core.autocrlf=true`. GitLab và GitHub `main` vẫn ở `c213bf7`; candidate local ahead `6`, chưa push.

| Gate trên exact SHA | Kết quả |
| --- | --- |
| LF provenance | **PASS** — W-0174 template và W-0170 manifest member đều `i/lf w/lf` |
| W-0164 / W-0165 / W-0170 / W-0174 | **PASS** — `2/19`, `2/27`, `1/21`, `1/46` |
| Release build | **PASS** — 0 warning, 0 error |
| `dotnet format --verify-no-changes` | **PASS** |
| Contract / Unit | **PASS** — `24/24`, `510/510` |
| PostgreSQL Integration | **PASS** — `239/239` |
| Chaos | **PASS** — `8/8` |
| Admin UI | **PASS** — npm 0 vulnerability; lint; typecheck; Vitest `176/176`; production build |
| CI/docs/traceability/gate | **PASS** — docs 14; trace 485; mirror `11/175/23`, production=false |
| PII | **PASS** — `12/12` provenance/evidence artifact; scanner self-test `CT-CI-06..06h` |
| Security wrapper | **PASS** — checksum, NuGet/npm HIGH=0, negative control, Gitleaks 159 commits |
| Candidate integrity | **PASS** — detached HEAD/tree đúng; commit diff check và worktree status sạch |

W-0177 chỉ đóng local verification. Hosted GitLab pipeline vẫn `AUTH_BLOCKED / NOT_RUN`; external
dispatch vẫn `0/5`; M3/Product/Security/Platform/Telephony artifact, shared E2E, staging/UAT và
production evidence chưa nhận. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Next action: Release reviewer cho phép publish đúng `59597e2` và cung cấp pipeline URL hoặc GitLab
session/token có `read_api`; chỉ sau hosted pipeline PASS mới cập nhật hosted verdict. Không amend
candidate và không trộn kết quả của các SHA bị loại.

W-0186 sau đó phục hồi/re-pin C5 trên working tree hiện hành. Việc đó không thay verdict lịch sử của
`59597e2` và cũng chưa tạo candidate exact mới; xem [W-0186](../W-0186/README.md).
