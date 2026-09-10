# W-0267 — Tám bản parser JSON thành một, và một test suýt chứng minh điều không có thật

Ngày: 2026-09-10 · Baseline: `main@a3688c7` · Trạng thái: **TESTS_PASS**.

Phần còn lại của S4 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

## 1. Vấn đề

`rejectDuplicateJsonKeys` — parser JSON viết tay từ chối khoá trùng — tồn tại **8 bản** trong
`deploy/ci/scripts/`, với **5 implementation khác nhau** theo census của `W-0259`.

Nó không phải tiện ích định dạng. Nó là thứ chặn một bundle bằng chứng khai hai lần cùng một
trường, nơi validator đọc một giá trị còn người duyệt đọc giá trị kia.

## 2. Đo trước khi sửa: chúng có thật sự khác nhau không?

Đọc 450 dòng parser sẽ trả lời "chúng khác nhau về chữ" — điều census đã nói. Câu hỏi đáng tiền là
**chúng có nhận và từ chối cùng những tài liệu không**.

Một harness vi sai trích cả 8 bản ra file riêng và chạy chúng trên **18 tài liệu đối kháng**: trùng
khoá ở gốc / lồng / trong mảng, khoá trùng viết bằng `a`, dấu nháy escape trong khoá, ký tự
điều khiển, nội dung thừa sau tài liệu, chuỗi chưa đóng, số có số 0 đứng đầu, lồng 40 tầng.

**Cả 8 đồng ý trên cả 18 trường hợp. 0 bất đồng.**

Nghĩa là 5 "implementation" khác nhau về cấu trúc, không khác về hành vi. Chọn bản đa số (4/8) không
đổi thứ bất kỳ gate nào chấp nhận — và đó là điều kiện để hợp nhất mà không cần quyết định của owner,
khác hẳn `assertIdentifier` mà `W-0260` phải hỏi.

Kiểm lại sau khi tách: bản trong `json-shape-lib.mjs` được đối chiếu trực tiếp với bản ở `HEAD`
trên các ca escape — **0 bất đồng**.

## 3. Sửa

`deploy/ci/scripts/json-shape-lib.mjs` (120 dòng) giữ parser đó một lần, ném `JsonShapeError` thay
vì gọi `fail` của từng script. Mỗi validator giữ một wrapper 10 dòng dịch lỗi đó về `fail` của mình,
nên thông điệp gate không đổi.

| | Dòng |
| --- | ---: |
| 8 validator | **+77 / −699** |
| Toàn bộ thay đổi (13 tệp) | +270 / −702 |

## 4. Test — và lý do nó không nằm trong một validator

Chỉ **3 trong 8** selftest từng đưa vào một khoá trùng thật (`capacity-registry`,
`external-decision-response`, `external-decision-routing`). Năm bản còn lại giờ đi tới cùng một
parser mà **không test nào chạm vào**. Xoá trùng lặp mà không bù test là đổi 8 bản không được kiểm
lấy 1 bản không được kiểm.

Nên 17 ca nằm trong `ci-config-selftest.mjs`, cùng chỗ với test carve-out của `W-0260` — theo đúng
quy ước sẵn có: hành vi của lib được kiểm ở đó, còn `gate-invocations.json` khai lib là
`sweepable: false` như hai lib anh em.

Ca quan trọng nhất là ca né tránh: `{"a":1,"a":2}`. So sánh **chữ thô** của khoá sẽ cho nó lọt;
phải giải escape rồi mới so.

**Chứng minh test có răng** — thay `JSON.parse(...)` bằng `textValue.slice(...)` (so khoá bằng chữ
thô) và chạy lại chính danh sách ca đó:

| | Ca đỏ |
| --- | ---: |
| Bản thật | **0 / 17** |
| Bản đột biến | **8 / 17** |

Trong đó `{"a":1,"a":2}` trả về `null` — tức khoá trùng **lọt qua**. Đó đúng là thứ ca này giữ.

## 5. Hai lỗi của tôi trong lượt này

**Lỗi 1 — literal bị nuốt dấu `\`, hai lần.** Heredoc làm `\\n` thành xuống dòng thật và `\\u0061`
thành `a`. Hậu quả không phải là test đỏ mà là **test xanh vì lý do sai**: ca escape thoái hoá
thành ca trùng khoá thường, không kiểm gì thêm. Đã sửa bằng cách dựng dấu `\` qua `chr(92)` và kiểm
lại từng byte trong tệp.

**Lỗi 2 — tôi suýt ghi nhận một khiếm khuyết không tồn tại.** Từ phép đo hỏng ở trên, tôi kết luận
parser từ chối `\n` escape trong chuỗi — JSON hợp lệ — và **đã viết bình luận trong test** gọi đó là
"ghi nhận, không tán thành", một hạn chế owner cần quyết. Khi literal được sửa đúng, ca đó **được
chấp nhận**: parser vốn đúng, chỉ phép đo sai. Ca hỏng đã pass, nên nếu không có bước sửa literal thì
lời khẳng định sai đó đã đi vào repo dưới dạng một test "đang xanh".

Giờ có hai ca cạnh nhau: `\n` escape được nhận, ký tự điều khiển thô bị từ chối.

## 6. Cây làm việc dùng chung với một agent khác

Trong lượt này một agent khác commit `W-0265` và `W-0266` vào cùng cây. Ba hệ quả, ghi lại vì chúng
ảnh hưởng tới cách đọc commit này:

- **Index bị reset hai lần.** Phần đã stage của tôi biến mất giữa chừng. Bước dựng cây cuối cùng vì
  thế làm trong `GIT_INDEX_FILE` riêng.
- **Một sửa đổi của tôi bị cuốn vào commit của họ.** Re-pin `shared_e2e_validator_sha256` trong
  `d06-revalidation-evidence-validator.mjs` nằm trong `a3688c7`, không phải commit này. Vì thế
  `a3688c7` **đang đỏ**: nó pin `target-v1-shared-e2e-report-validator.mjs` ở `42402c22…` trong khi
  tệp đó chưa được commit. Commit này đóng lại chỗ đó — đo được: gate `d06` **FAIL ở `a3688c7`,
  `ok` trên cây của tôi**.
- **Hai gate còn đỏ không phải của tôi.** `opt-out-suppression-bundle-validator` và
  `upstream-session-signoff-validator` đều báo `integration-requirements/06-module-3-api-handover.md`
  lệch pin. Tài liệu ở `HEAD` băm ra `6d729126…`; pin ở `HEAD` là `b26cc5e6…`; bản sửa **chưa
  commit** của họ ghi `768583…` — cả hai đều lệch. Hai dòng pin đó được giữ **nguyên như `HEAD`**
  trong commit này.

Cách phân tách: dựng cây từ index tạm, `git archive` cả `HEAD` lẫn cây đó ra hai thư mục sạch, chạy
gate sweep trên cả hai và **so verdict**. Chạy gate trong cây làm việc đang có sửa đổi dở của người
khác chỉ nói được "có gì đó đỏ", không nói được của ai.

## 7. Kết quả

```
Gate sweep (repo thật)   37/39 run — 2 FAIL, cả hai là pin handover của agent kia
Verdict so với HEAD      1 thay đổi duy nhất: d06  FAIL -> ok
ci-config-selftest       CI_CONFIG_SELFTEST_PASS
                         JSON_SHAPE_SINGLE_SOURCE_PASS
                         VALIDATOR_HELPER_CENSUS_PASS
```

Census hạ từ `8 copies / 5 implementations` xuống `8 copies / 1 implementation` — vẫn 8 điểm gọi
mang tên đó, nhưng chỉ còn một hành vi. Cùng hình dạng `assertNoSensitiveValue` đã có ở `W-0260`
(9/1). Guard bắt cả hai chiều, nên số này không tự trôi.

## 8. Còn lại

**19/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH. S4 đóng hết phần không cần quyết định.

Cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — tách tên xong thì 22 site thay được an toàn.
- **`phone_validation_status`** — contract nói required, intake nhận thiếu.

Phần S4 **cố ý không đụng**: `assertString` (11 bản / 9 implementation), `assertExactKeys` (13/8),
`readStrictJson` (6/5). Khác với `rejectDuplicateJsonKeys`, khác biệt của chúng **có tải**: bộ ký tự
khác nhau, giới hạn độ dài khác nhau. Hợp nhất là đổi thứ gate chấp nhận — quyết định của owner, như
`W-0259` đã ghi.

Thuần kỹ thuật còn lại: **B9**, **B10**, **C1**, **C3**, **C4**, **C5**.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
