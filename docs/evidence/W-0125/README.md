# W-0125 — Evidence: mở khoá ba gate còn treo của W-0123

Ngày: `2026-08-27`

Trạng thái: `TESTS_PASS` cho phần làm được cục bộ · hai gate vẫn `BLOCKED_EXTERNAL`, một gate chờ owner

Baseline: `main@6760ba6`

Authority: `OD-18` không đổi. `W-0125` không sửa runtime, không đổi contract.

## 1. Vấn đề

`W-0123` đóng lại với ba gate mở, và `W-0124` cố ý **không** đóng hộ:

| Gate | Trạng thái sau W-0124 |
| --- | --- |
| M3 producer/consumer usage + sign-off | `OWNER_DATA_REQUIRED` |
| Target/staging/production DB preflight | `ENV_BLOCKED` |
| Hosted GitLab CI | `NOT_RUN` |

Cả ba đều cần người hoặc quyền truy cập ngoài repo. Nhưng "cần người ngoài" không có nghĩa là
"không làm được gì": mỗi gate đang chờ ở một mức độ sẵn sàng khác nhau, và hai trong ba đang chờ
mà **chưa có sẵn thứ để người ta thao tác**. `W-0125` đóng khoảng cách đó — biến "đang chờ" thành
"chỉ còn một thao tác".

Không gate nào được đánh dấu là đã đóng bởi work này.

## 2. Gate M3 — từ "cần hỏi" thành "một phiếu điền được"

Trước: nghĩa vụ của M3 nằm rải ở `W-0123` plan §10 (`OD18-C1..C5`), `IR-06` §10 và một phiếu
`OD-15` đã `SUPERSEDED`. Không có chỗ nào M3 có thể ngồi xuống và trả lời.

Sau: [`plan/ivr-orther/questions-to-module-3-od18-authority.md`](../../../plan/ivr-orther/questions-to-module-3-od18-authority.md)
— phiếu hẹp, mỗi câu có ô chọn và ô bằng chứng, kèm ô ký.

Hai điều được viết thẳng vào phiếu vì chúng là rủi ro thật chứ không phải thủ tục:

- **`OD18-C2`** hỏi thẳng M3 đã lọc đơn không cần gọi trước khi gửi task chưa. Đây là câu duy nhất
  quyết định `OD-18` có an toàn trên dữ liệu thật. Nếu M3 đang dựa vào IVR bỏ qua hộ thì lượng cuộc
  gọi tăng **ngay khi bản này lên**, và hai bên phải chốt trước khi deploy chứ không phải sau.
- §4 nói rõ yêu cầu cũ đã **huỷ**: M3 không cần build `trust.risk_evidence_available` nữa. Phiếu
  `OD-15` từng yêu cầu đúng field đó; không nói rõ thì M3 có thể đang làm dở một việc vô nghĩa.

§3 chỉ cho M3 cách tự kiểm chứng bằng counter `ivr_legacy_skip_candidate_total` của `W-0124` thay
vì phải tin lời IVR.

## 3. Gate DB preflight — từ "SQL trong tài liệu" thành "query được CI chạy"

Trước: SQL nằm trong `W-0123` plan §6 dưới dạng ba câu `count(*)` trong một code block. Chưa ai
chạy nó, và không có gì đảm bảo nó chạy được.

Sau: [`tools/ops/od18-legacy-skip-preflight.sql`](../../../tools/ops/od18-legacy-skip-preflight.sql)
+ [`tools/ops/Invoke-Od18Preflight.ps1`](../../../tools/ops/Invoke-Od18Preflight.ps1).

Ba điều được sửa so với SQL trong plan:

| Vấn đề trong plan | Đã sửa |
| --- | --- |
| Gộp `status='SKIPPED'` chung với decision trusted-skip trong một `count(*)` | Tách metric 3 và 4. `SKIPPED` là lifecycle value chung, còn retention test dùng; gộp lại sẽ báo row không liên quan thành lịch sử trusted-skip và lập luận sai cho việc gỡ một giá trị chẳng ai phụ thuộc |
| Không có metric nào khớp với counter runtime | Metric 7 dùng **đúng** predicate của `ivr_legacy_skip_candidate_total`, nên số trong DB và số trên dashboard so sánh được với nhau |
| So sánh JSON như chuỗi | `risk_flags_json` và `eligibility_snapshot_json` đã là `jsonb` trong schema; so sánh chuỗi sẽ **không bao giờ khớp** với `'[]'::jsonb` đã chuẩn hoá |

Điều đáng kể nhất: **query được chạy trên mọi lượt test**. `IT-M3-AUTHORITY-13` đọc chính file
`.sql` đó, bỏ meta-command của psql, và chạy trên schema PostgreSQL đã migrate:

```
Passed!  - Failed: 0, Passed: 1 — ThePreflightSqlRunsOnTheRealSchemaAndCountsTheRetiredShape
```

SQL commit vào repo mà chưa từng chạm schema thật là một lời hứa, không phải công cụ: sai tên cột
hoặc sai kiểu so sánh sẽ lộ ra đúng lúc tệ nhất, trên môi trường quan trọng nhất, trước mặt người
thừa kế nó.

Fixture dựng ba row có chủ ý — một row lịch sử trusted-skip đầy đủ, một row mang hình dạng cũ nhưng
chưa từng bị skip, và một row có risk flag — nên không metric nào có thể "đúng" chỉ vì mọi thứ đều
khớp một row duy nhất.

**Mutation:** đổi `trusted_skip_allowed IS DISTINCT FROM false` thành `IS NOT NULL` (predicate lệch
khỏi counter runtime) → `IT-M3-AUTHORITY-13` đỏ tại `Expected: "2" / Actual: "1"`. Revert xong xanh
lại.

Một chi tiết đáng ghi trong runner: nó bỏ comment **trước** khi tách câu lệnh theo `;`. Không phải
làm đẹp — một dấu chấm phẩy nằm trong câu tiếng Anh ("Run it per environment; the numbers decide…")
sẽ cắt giữa câu và đưa nửa sau cho Postgres. Lần chạy đầu đỏ đúng vì lý do đó. Runner không được
phép hỏng vì ai đó viết comment cho tử tế.

Script PowerShell cố ý **không** fallback sang .NET client khi thiếu `psql`: một preflight âm thầm
đổi cách kết nối cũng là âm thầm đổi quyền nó được phép làm. Thiếu công cụ thì phải đưa người vận
hành tới một quyết định, không phải tới một nhánh code khác.

## 4. Gate hosted CI — sẵn sàng, chờ owner

`W-0121` đã sửa lối đẩy: `remote.origin.pushurl` có hai giá trị (GitLab trước, GitHub sau), nên một
`git push origin main` vừa kích hoạt GitLab CI vừa giữ mirror. Nhưng `W-0121` đóng ở `CODE_DONE`
với ghi chú "chưa push", nên pipeline vẫn chưa từng có gì để chạy.

Đo lúc `W-0125`:

| Nơi | `main` |
| --- | --- |
| local | `6760ba6` |
| GitLab (`origin`) | `ef09a06` |
| GitHub (`github`) | `ef09a06` |

`git push --dry-run origin main` → fast-forward `ef09a06..6760ba6` tới **cả hai** remote, exit `0`.
Nghĩa là quyền ghi và fast-forward đều hợp lệ; chỉ còn thiếu một lượt push thật.

Điều không kiểm được từ máy này: runner `#55115499` có online không. Mọi job kế thừa
`tags: [ginsengfood-docker]`, nên runner offline thì pipeline sinh ra rồi treo `pending` chứ không
đỏ — và "pending" không phải là bằng chứng cho bất cứ điều gì.

Dự đoán job nào có thể đỏ, ghi trước để lượt chạy đầu không bị đọc nhầm:

| Job | Dự đoán | Căn cứ |
| --- | --- | --- |
| `api_contract_diff` | PASS | `W-0124` chạy trọn vẹn 8 bước trong container pinned, exit `0` |
| `api_docs_verify` | PASS | `test:docs` + `test:traceability` xanh cục bộ |
| `ui_qa` | PASS | `npm ci` là đúng lệnh CI dùng; lint/typecheck/test xanh cục bộ |
| .NET test lanes | PASS nếu DinD lên | `TESTCONTAINERS_DIND_PASS` trong config selftest; local `805/805` |
| Bất kỳ job nào | `pending` vô hạn | runner offline — không phải lỗi code |

Gate này **chưa** đóng. Push là hành động của owner.

## 5. Regression

| Gate | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | PASS, 0 warning/0 error |
| `dotnet test Ivr.sln` | PASS `805/805` (+1: `IT-M3-AUTHORITY-13`) |
| Traceability | `488` |
| 7 node gate | PASS |
| PowerShell parse + mandatory params | PASS |
| GitNexus detect changes | không symbol runtime nào đổi; chỉ thêm test + tool + docs |

## 6. Gate vẫn mở sau W-0125

| Gate | Trạng thái | Còn thiếu gì |
| --- | --- | --- |
| M3 sign-off | `OWNER_DATA_REQUIRED` | Chữ ký vào phiếu `OD18-C1..C5`. Phiếu đã sẵn, IVR không tự trả lời hộ được |
| Target DB preflight | `ENV_BLOCKED` | Endpoint + credential read-only. Query đã sẵn và đã được CI chạy |
| Hosted GitLab CI | `NOT_RUN` | Một lượt `git push origin main` do owner quyết, rồi pipeline phải xanh |
| Real customer call | `NO` | Không đổi |

`W-0125` rút ngắn khoảng cách tới ba gate đó. Nó không đóng gate nào, và không được đọc như thế.
