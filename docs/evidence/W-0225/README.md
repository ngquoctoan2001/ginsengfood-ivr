# W-0225 — Cổng mirror mở được bằng ba chữ

Ngày: 2026-09-08 · Baseline: `main@39a6243` · Trạng thái: **TESTS_PASS**.

Mục `1.4` chốt: *"theo gate từng artifact"*. Đi kiểm xem hai release blocker của `W-0122` có thật sự
là gate không, thì một cái không phải.

## 1. Phát hiện, chứng minh trước khi sửa

`MODELS.lock` có hai cổng cạnh nhau, hình dạng giống hệt. `tts-provenance-gate.mjs` đối xử với chúng
**hoàn toàn khác**:

| | `legal_gate` | `internal_mirror_gate` |
| --- | --- | --- |
| guard trong `validate()` | **có** — throw `legal approval authority invalid` | **không** |
| đòi `decision_authority` | `LEGAL_PRIVACY` | — |
| đòi người ký / ngày / reference | **cả ba** | — |
| ràng vào artifact | — | **không** |
| mutation test | `legal-authority` | **không có** |
| điều kiện mở | 5 trường, đúng thẩm quyền | `status !== "PASS"` |

Tức cổng mirror mở bằng **ba chữ**. Chạy thật trước khi sửa:

```text
internal_mirror_gate = {"status": "PASS"}
→ TTS_PROVENANCE_STRUCTURE_PASS artifacts=13 release_blockers=LEGAL
→ internal_mirror_uri still null on 13/13 artifacts
```

`INTERNAL_MIRROR` rời khỏi danh sách blocker trong khi **không một mirror nào tồn tại**.

Đây đúng hình dạng `legal_gate` từng có ngày `2026-08-28`, trước khi commit `2a4f45d` tự ký
`PASS` với `"Owner module IVR"` và `TODAY-03` phải khôi phục. Bài học đó được áp cho **một** trong
hai cổng. Cổng còn lại giữ nguyên lỗ.

## 2. Luật đã tồn tại — chỉ không ở nơi CI nhìn thấy

`deploy/tts/scripts/verify-model.py` **đã** đòi mirror thật:

```python
if any(
    not item.get("internal_mirror_uri") or not item.get("internal_mirror_digest")
    for item in lock["artifacts"]
):
    raise ModelLockError("production requires an exact internal mirror")
```

Nhưng nó nằm trong nhánh `--mode production`, và **CI không chạy file này** — job
`tts_candidate_selftest` chỉ `apk add nodejs` rồi chạy các gate Node. Image cũng không ship
`deploy/tts/scripts/`. Nên luật có thật mà không người đọc nào của CI gặp nó.

Cùng lớp lỗi `W-0221`: **một luật, hai bản, một bản lỏng hơn.**

## 3. Đã sửa — đối xứng, không phát minh thêm

Thêm `hasInternalMirrorApproval` cạnh `hasLegalPrivacyApproval`, và bản Python song sinh:

- đòi **hồ sơ quyết định**: `decided_by`, `approval_reference`, `decided_on` (`YYYY-MM-DD`) —
  đúng bộ trường `legal_gate` đang đòi;
- đòi **bằng chứng cổng mang tên nó**: mọi artifact phải có `internal_mirror_uri` không rỗng và
  `internal_mirror_digest` khớp `^(sha256:)?[a-f0-9]{64}$`;
- thêm guard trong `validate()` đối xứng với guard của legal.

### Hai chỗ cố ý **không** làm

**Không chỉ định `decision_authority`.** `LEGAL_PRIVACY` tồn tại vì owner **không được** tự ký
review pháp lý của mình. Cổng này khác: lý do trong chính lock ghi thiếu *"**owner-approved**
internal artifact or OCI mirror URI/digest"* — tức owner **là** thẩm quyền đúng ở đây. Đặt tên một
thẩm quyền khác là phát minh governance chưa ai quyết. Đòi **hồ sơ**, không đòi **danh tính**.

**Không đụng luật production.** Dòng `--mode production` giữ nguyên yêu cầu, chỉ gọi cùng helper
để hai bên không lệch nhau vì một cái dùng truthiness còn cái kia dùng regex.

## 4. Ghim

Gate nay có **10 mutation** (trước 8):

```text
mutation=mirror-bare-pass            {"status":"PASS"}                     → REJECTED
mutation=mirror-without-artifacts    hồ sơ đủ, mirror vẫn null             → REJECTED
```

Cộng một **positive fixture**: hồ sơ đủ **và** mirror đủ thì được chấp nhận — để luật là cổng chứ
không phải tường.

Chạy lại đúng probe của §1 sau khi sửa:

```text
exit: 1
Error: internal mirror approval invalid
```

Bản Python kiểm bằng 6 case, khớp Node từng case:

```text
[ok] real lock (13 null mirrors)         -> False
[ok] bare {'status':'PASS'}              -> False
[ok] full record, mirrors still null     -> False
[ok] full record + all mirrors filled    -> True
[ok] mirrors filled, no record           -> False
[ok] garbage digest                      -> False
PYTHON_MIRROR_PARITY_PASS
```

## 5. Cái bẫy để lại cho người làm mirror thật

`expectedArtifactSetSha256` phủ **cả** `internal_mirror_uri` và `internal_mirror_digest` (xem
`artifactFingerprintFields`). Nên khi Platform điền giá trị thật, gate sẽ đỏ với
`artifact provenance fingerprint drift` — **đúng lúc người ta làm đúng**.

Đó là review point hoạt động, không phải defect. Nhưng nếu không viết ra thì nó đến như một lỗi bí
ẩn — y hệt thứ `W-0223` vừa sửa cho checklist calibration. Đã ghi comment ngay trên hằng số, kèm
dây chuyền hash `W-0126` dựng: `dependency_lock_sha256` → `model_lock_sha256` → hằng số này.

## 6. Một gate thứ hai, tìm thấy vì nó nói dối tôi

Chạy `verify-api-behavior-matrix.mjs` sau khi sửa thì được:

```text
"verdict": "FAIL",  "passed": 38,  "operations": 38,  "failures": []
```

**`FAIL` ngay trên `failures: []`, với 38/38 pass.** Tôi đọc dòng đó rồi kết luận *"không phải do
tôi"* — và **sai**. Lý do thật nằm trong `report.json`, không bao giờ được in ra:

```text
"Source changed since the HTTP matrix ran; rerun the test"
```

Đối chiếu hash thì đúng **1 trong 498** file source lệch: `tts-provenance-gate.mjs` — chính file
lượt này sửa. Gate hoạt động hoàn hảo; chỉ có dòng nó in ra là vô dụng.

Nguyên nhân: `verdict` quyết bởi mảng `failures` **cấp cao**, còn `console.log` in mảng `failures`
**theo từng operation**. Hai mảng khác nhau, cùng một tên.

```js
verdict: failures.length || operationReports.some(...) ? 'FAIL' : 'PASS'
// nhưng in ra:
failures: operationReports.filter(op => op.failures.length).map(...)
```

Đã sửa: in `failures` đúng mảng quyết định verdict, và tách phần theo operation sang
`operation_failures`. Bản sửa **tự chứng minh** — lần chạy ngay sau đó nói thẳng lý do:

```text
"verdict": "FAIL",  "failures": ["Source changed since the HTTP matrix ran; rerun the test"]
```

Rồi chạy lại `ApiBehaviorMatrixTests` để làm mới observations → `verdict: PASS`, `38/38`.

> Đây là bài học `W-0216` ở dạng khác. `W-0216`: *"sửa một file được ghim thì re-pin cùng lượt."*
> Ở đây "ghim" là `source_sha256` của ma trận HTTP, và cách làm mới là **chạy lại test**, không
> phải sửa tay. Cái làm mất thời gian không phải luật đó — mà là gate không nói cho tôi biết luật
> đó vừa bị vi phạm.

## 7. Kiểm chứng

```text
tts-provenance-gate --selftest    10 mutation PASS; STRUCTURE_PASS artifacts=13
                                  release_blockers=LEGAL,INTERNAL_MIRROR (không đổi — lock vẫn null)
tts-audition-selftest             PASS voices=11 outbound=DENIED
tts-voice-acceptance-gate         PASS bindings=7 mutations=9
tts-fixed-render-selftest         PASS refusals=6
tts-helm-selftest                 PASS predial_budget=ENFORCED
tts-container-selftest            PASS nonroot=YES network=NONE
lab-converter-selftest            PASS roster=4x3 refusals=7
dotnet test Ivr.sln               900/900 PASS, 0 failed, 0 skipped
gitnexus impact hasLegalPrivacyApproval   LOW · 1 direct · 0 process · 0 module
verify-api-behavior-matrix        verdict=PASS 38/38 · 417 requests · failures=[] operation_failures=[]
api-behavior-matrix-selftest      API_MATRIX_VALIDATOR_SELFTEST=PASS (18 cases; 1 valid, 17 refusals)
gate-status.mjs                   GATE_STATUS_PASS — 223 work items
contract-freeze-verifier.mjs      CONTRACT_FREEZE=PASS
docs-selftest.mjs                 API_DOCS_SELFTEST_PASS
generate-test-traceability --check TEST_TRACEABILITY_CURRENT=557 (không đổi)
```

`MODELS.lock` **không bị sửa** — hai probe đều khôi phục nguyên trạng, `git diff` rỗng. Không đổi
một giá trị hash nào, không mở gate nào. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 8. Còn lại

- **Hai bản luật vẫn là hai bản.** Không chia sẻ code qua ranh giới Node/Python được, và bản Python
  **không chạy trong CI, không nằm trong image, không có test nào**. Đã ghi `TWIN:` ở cả hai file,
  nhưng đó là lời nhắc cho người chứ không phải cơ chế.
- **Sửa bền là hoist hai predicate vào `deploy/tts/shim/model_lock.py`** — module đã nằm trong image
  và đã được `test_shim.py` import — rồi test chúng ở đó. Không làm lượt này vì đổi `shim/` bắt buộc
  rebuild image và xác thực lại lock binding, mà offline ở đây không chạy được.
- Hai blocker của `1.4` **không đổi trạng thái**: `LEGAL` vẫn cần review licence, `INTERNAL_MIRROR`
  vẫn cần Platform. Lượt này chỉ làm cái thứ hai **không thể mở bằng ba chữ**.
