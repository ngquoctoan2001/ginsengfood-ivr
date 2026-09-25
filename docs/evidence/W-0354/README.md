# W-0354 — Phần của dev trong danh sách chief `25/09`, đợt 1

Ngày 25/09/2026 · Claude, theo yêu cầu của Toàn *"rà soát tiếp việc cần làm"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu. Theo mục `A1` bước `4` của chính danh sách này,
`ACCEPTED` trong tracker là tự nghiệm thu của dev M8, không phải nghiệm thu độc lập.

## Vì sao có việc này

Sáng `25/09`, chief auditor (thay mặt Tech Lead Nguyễn Đức Mạnh) để lại
[`plan/toan-viec-can-lam-m8-2026-09-25.md`](../../../plan/toan-viec-can-lam-m8-2026-09-25.md). Chief đo tại `5603204`,
chạy thử trên server test, và xếp một thứ tự làm duy nhất ở mục "Bắt đầu từ đâu". Việc này làm những mục dev tự làm
được, không phải chờ ai. Trước khi sửa, mỗi khẳng định chief nêu đã được kiểm lại trên HEAD `1091496`. Trong lúc làm,
việc này tìm ra nguyên nhân CI hosted đỏ từ `16/09`.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `C15` (+`C20`) | `docker-compose.e2e.yml` và `docker-compose.sandbox.yml` mở khung giờ cả ngày cho `ivr-api`, như đã mở cho worker. Thêm một chỗ thứ ba cùng lỗi mà chief chưa nêu: profile `LocalMockE2E` của **API**, profile mà soak và E2E local dùng. Đính chính `IR-07` ca đơn đêm; thêm reason vào `specs/api/06-error-codes.md`; sửa `202→200` ở `IR-06` | `UT-CALLWINDOW-PARITY-01`, `UT-CALLWINDOW-PARITY-02`, `IT-INTAKE-NIGHT-REUSE-04` |
| `C7` (+`C8`, `B10`) | Đính chính `IR-07`: rút đề xuất chờ 24 giờ ở `M3-13`, `A-10` chỉ còn nghĩa "IVR không tự hủy đơn", `recommended_core_action` của `IVR_NO_ANSWER_FINAL` chỉ là nhãn | Tài liệu |
| `C9`, `C14`, `C2`, `C21` | `M3-30` rút; câu cũ `IR-06` §4.8 có đính chính; dòng `IVR_OPT_OUT` của m8-05 đính chính ở `00-CHUA-XONG`; ca phát lại muộn ghi vào `IR-06` §4A.3 và `IR-07` | Tài liệu |
| `B16` | Intake từ chối `total_amount` có phần lẻ. Contract `draft.33` thêm `multipleOf: 1` và mô tả. Renderer báo lỗi dữ liệu thành `SPEECH_RENDER_DATA_REJECTED`, nên gateway coi kênh SIM lành, không cách ly | `IT-INTAKE-AMOUNT-16`, `UT-RENDER-DATA-01`, `UT-RENDER-DATA-02`, `UT-RENDER-DATA-03`, `IT-TEL-RENDER-DATA-09` |
| `B15` | Hai record mang số điện thoại in dấu che thay cho số | `UT-PHONE-TOSTRING-01`, `UT-PHONE-TOSTRING-02`, `UT-PHONE-TOSTRING-03` |
| `B11` bước `0` | Test chạy intake → eligibility qua API → quay MOCK → chuẩn hoá → outbox → sáu màn đọc admin trên PostgreSQL thật, rồi quét **mọi cột text** của database, mọi phản hồi admin và mọi dòng log của API. Số chỉ được phép nằm ở `ivr_confirmation_tasks.phone_e164` | `IT-PHONE-CONTAIN-01` |
| `B13` (+`B14`) | Gateway Asterisk lấy chế độ của deployment thay cho `LabRealSim` gán cứng. Test chặn ở `PRODUCTION_REAL` để cho thay đổi SIP-04, như chief ghi | `UT-DISPATCH-MODE-01`, `UT-DISPATCH-MODE-02` |
| `B5` | `deploy/ci/rollback.md` §3b: phép thử `to_regclass` chỉ đúng với database chưa áp `P03`; thêm câu hỏi `p03_applied` và cách đọc một chiều | Tài liệu |
| `B1` bước `1` | Con số 45 phút ghi nguồn `D07` (`candidateSource`); vẫn không vào phép tính khi chưa có phân bố đơn | `capacity-selftest.mjs` |
| `B9` bước `2` | `IR-06` §4A.7: chuỗi `(perm IVR_…)` là nhãn, không phải quyền. Số endpoint cần `X-Actor-Id` sửa `29/31→31/33`. `IR-08` thêm hai bẫy | Tài liệu |
| `A1` bước `4`, `7` | Tracker, bảng readiness và đầu danh sách nghiệm thu ghi rõ `ACCEPTED` là tự nghiệm thu. `specs/data/05-pii-policy.md` sửa mặc định `ProductionTargetV1FieldsApproved` về `NO` | `gate-status.mjs`, `acceptance-batches.mjs` |

## CI hosted đỏ từ `16/09`, và vì sao không ai thấy

Runner GitLab của repo chạy trên chính máy dev, nên kết quả từng job đọc được từ event log của máy mà không cần mở
GitLab. Cách dựng lại tên job và toàn bộ số liệu nằm ở [hosted-ci-diagnosis.json](hosted-ci-diagnosis.json).

- **`openapi_lint` đỏ ở mọi pipeline log còn giữ** (`21/09`→`25/09`). Nguyên nhân: `W-0307` (`16/09`, `draft.29`) viết
  ba field của `IvrAuditEvidenceRow` bằng `nullable: true`. Từ khoá này thuộc OpenAPI 3.0, không có trong 3.1, và spec
  khai `openapi: 3.1.0`. Redocly báo 3 lỗi. Đã tái hiện trên máy này bằng đúng image của job.
- **`gate_sweep` đỏ từ `22/09` 13:48 tới `24/09`**, xanh lại ở `1091496`. Gate nào đỏ thì không xác định được, vì log
  job nằm trên gitlab.com.
- **Vì sao sweep local vẫn xanh:** manifest gate không có Redocly. Mọi gói nghiệm thu từ `16/09` ghi `GATE_SWEEP_PASS`
  trong khi hosted đỏ. Vì stage validate đỏ, các stage build/test/security/privacy/publish chưa chạy trên hosted ít
  nhất từ `21/09`.

Đã sửa bằng contract `1.0.0-draft.33`, ghi `type: [string, 'null']`, và thêm `openapi-lint-gate.mjs` vào sweep local.
Gate này có hai phép kiểm âm: fixture sai mà job hosted dùng để tự kiểm, và một bản sao spec được cấy lại đúng lỗi
`nullable`. `oasdiff v1.26.1` không báo breaking ở `32→33` lẫn `27→33`. Nó in *"became not nullable"*, ngược với nghĩa
theo 3.1; giải thích ở `docs/api-changelog.md`. NSwag sinh cùng `string?` cho cả hai cách viết. Thay đổi duy nhất trong
code sinh ra là chú thích của `Total_amount`.

## Chưa làm, và vì sao

| Mục | Chờ |
| --- | --- |
| `C22` | Nguyên văn bảng `ivr-cancel-reason-map.v1` trong `_SPEC/FIX_M8.md`. Chief dặn chép đúng, không tự dựng; file không có trên máy dev |
| `C23`, `N1`, `B8` bước (2)–(4) | Toàn quyết theo luật quá độ `24/09`. `N1` khôi phục khoảng 1.200 dòng mà `9af20d3` đã xoá; ngân hàng clip cần tên 19 SKU và một người nghe duyệt |
| `A1` bước (1)–(3), (5), (6) | Toàn quyết: tờ trình cho Sếp; đưa S2, `legal_gate` và `internal_mirror_gate` về "chờ"; sửa sổ `OD-V1-*` theo chốt `25/09`; siết gate tts-provenance |
| `B17` | Toàn chọn mô hình. Impact analysis của hàm guard là **HIGH**: mọi task đi qua nó lúc intake |
| `B7`, `C13`, `C17` | Module 3 trả lời `M3-14` |
| `A2`, `B11` (1)–(4), `C16`, `C19` | Sếp trả lời mục `B2` phiếu `25/09`. Test ở bước `0` đúng với cả hai hướng |
| `C18` | Chief cấp chỗ trên server test |
| Gửi lại `IR-07` cho anh Mạnh | Toàn gửi. Phiếu đã có đính chính `25/09` |

## Kiểm chứng

Trên cây làm việc trước khi commit, đã build lại sau lần sinh DTO cuối:

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi |
| Test | unit `813/813`, contract `24/24`, integration `409/409`, chaos `8/8` |
| Thử đột biến | `9/9` đột biến làm test tương ứng đỏ, rồi xanh lại khi khôi phục — [mutation-results.json](mutation-results.json) |
| Gate sweep | Lượt đầu `43/44`: `docs-selftest` lệch vì `contract-manifest.json` đổi sau lần sinh cổng API. Sinh lại cổng; gate đó `1/1` |
| Lint | Redocly sạch trên hai contract; `openapi-lint-gate.mjs` đạt |

Gói bằng chứng của collector tại commit sẽ chạy sau khi soak của `W-0037` kết thúc, vì CI và collector đều chạy trên
máy này. Việc nặng trong lượt này được giữ trong nửa giữa của soak để hai quý mà soak so sánh không bị nhiễu.
