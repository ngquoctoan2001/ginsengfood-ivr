# W-0258 — Một secret đọc được từ API công khai, và một gate vỡ vì đường dẫn checkout

Ngày: 2026-09-09 · Baseline: `main@8be6001` · Trạng thái: **TESTS_PASS**.

B7 và B8 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).
Không đổi schema, không migration, không đổi API contract.

## 1. B7 — `ActiveGenerations` trả thẳng secret

`RotatingCredentialProvider` có doc-comment nói rằng bản ghi audit *"quoting the value would be the
leak it exists to record"*. Nhưng:

```csharp
public sealed record CredentialGeneration(int Generation, byte[] SecretBytes, ...);
public IReadOnlyList<CredentialGeneration> ActiveGenerations { get; }   // public
```

`provider.ActiveGenerations[0].SecretBytes` trả về credential thô. `IReadOnlyList` chỉ bảo vệ danh
sách, không bảo vệ nội dung mảng — caller còn ghi đè được vào đó.

**Chưa ai gọi nó**, và đó chính là lý do nó sống sót lâu thế: đây không phải bug đang rò, mà là một
dòng code chẩn đoán tương lai cách chỗ rò. Một `JsonSerializer.Serialize(provider.ActiveGenerations)`
trong endpoint diagnostics là đủ, và người viết dòng đó sẽ tin là an toàn — vì type trông như
metadata và doc-comment nói nó là metadata.

**Sửa:** tách mô tả khỏi vật liệu xác thực.

| | Trước | Sau |
| --- | --- | --- |
| Kiểu công khai | `CredentialGeneration` mang `byte[] SecretBytes` | chỉ `Generation`, `Fingerprint`, `NotBefore`, `NotAfter` |
| Lưu trong provider | plaintext bytes | `InstalledCredential` private, chỉ giữ **SHA-256 digest** |

Provider chỉ cần **xác minh**, không bao giờ cần **tiết lộ** — nên nó không giữ plaintext nữa.

### 1.1 Hai lỗi đi kèm, cùng một gốc

**`record` chứa `byte[]` so sánh bằng tham chiếu.** Positional record sinh `Equals`/`GetHashCode`
dùng reference equality cho thành viên mảng, nên hai generation giữ cùng một credential so ra
**khác nhau**, im lặng, ở bất cứ đâu dùng equality. Bỏ mảng khỏi record là hết.

**Constant-time không constant-time.** Comment nói vòng lặp cố tình không short-circuit để không lộ
generation nào khớp. Đúng, nhưng:

```csharp
CryptographicOperations.FixedTimeEquals(suppliedBytes, generation.SecretBytes)
```

`FixedTimeEquals` trả `false` **ngay lập tức** khi hai span khác độ dài — kiểm tra đó chạy *trước*
vòng so sánh hằng thời gian. Nên độ dài credential thật vẫn rò qua thời gian phản hồi, bất kể vòng
lặp bên ngoài làm gì.

Giờ so hai digest SHA-256. Luôn 32 byte cả hai phía, không còn độ dài nào để rò.

### 1.2 `Fingerprint`: sửa cả code lẫn lời biện minh

Trước: `SHA256(secret)` cắt còn 48 bit, biện minh bằng `MinimumSecretLength = 24`.

Lập luận đó sai ở chỗ căn bản: brute force bị chặn bởi **entropy**, không phải **độ dài**. Không có
gì ở đây bắt được một giá trị cấu hình phải ngẫu nhiên — credential là thứ operator gõ vào. Một
passphrase 24 ký tự do người chọn đổ trước dictionary trong vài giây, dù fingerprint dẫn xuất kiểu
gì. Và lời biện minh sai là nửa nguy hiểm hơn: nó cho phép người sau nối `Audit` vào một endpoint
mà tin rằng như thế là an toàn.

Sau: PBKDF2-HMAC-SHA256, 210.000 vòng (khuyến nghị OWASP), salt cố định để tách miền.

**Chọn tham số bằng đo, không bằng đoán.** Tôi ước lượng ~100 ms/lần và định hạ số vòng vì sợ chậm
khởi động. Đo thật trên máy này:

```
  10000 iters ->  1.3 ms
  50000 iters ->  8.0 ms
 100000 iters -> 16.9 ms
 210000 iters -> 34.2 ms
```

34 ms. Một credential được fingerprint đúng **một lần lúc cài đặt** — nhiều nhất hai lần cho mỗi
scope cấu hình, tức tối đa 8 lần ≈ 275 ms khi khởi động, ở chỗ không ai đang chờ. Không có lý do gì
để hạ số vòng. Với 210k, một dictionary 1 triệu từ tốn khoảng 9 giờ CPU thay vì dưới một giây.

Salt cố định chứ không ngẫu nhiên vì fingerprint phải tái lập được giữa các máy — đó là toàn bộ lý
do nó tồn tại. Chính vì thế nó **cần** work factor mới có giá trị gì.

Vẫn cắt còn 12 ký tự hex: nó là cái tay cầm ngắn cho người truy vết một lần xoay khoá qua hai hệ
thống, và work factor mới là thứ mang tính bảo mật, không phải độ rộng. Không có giá trị fingerprint
nào bị ghim ở đâu — test dẫn xuất kỳ vọng qua chính hàm này — nên đổi lần nữa được nếu chi phí đoán
giảm.

### 1.3 Không làm: chặn danh sách audit

Bản rà soát ghi `audit` tăng không giới hạn trong khi `generations` có trim. Đúng về mặt mã, nhưng
**không sửa**, và lý do quan trọng hơn việc sửa: cắt bớt lịch sử xoay khoá làm hỏng một bản ghi
tuân thủ. Một bản ghi âm thầm quên tệ hơn một bản ghi to dần.

Trong tiến trình này nó giữ một mục cho mỗi credential cài đặt và một cho mỗi lần xoay — tức một
hoặc hai mục cho mỗi scope, suốt đời tiến trình. Không có gì xoay theo lịch. Đã ghi lý do vào code
thay vì để người sau "dọn dẹp" nó.

## 2. B8 — gate refuse chính input của nó dưới symlink

Mười validator, mỗi cái một bản `isConfined`, mỗi cái tự dẫn xuất root:

```js
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");   // không realpath
...
if (!isConfined(realpathSync(resolved))) fail("real path escapes repository root");
```

So một đường dẫn **đã resolve** với một root **chưa resolve** chỉ đúng khi không thành phần nào của
đường checkout là symlink. Khi có một cái — `/tmp` trên macOS, workspace link của runner, một bind
mount — `relative()` trả về chuỗi bắt đầu bằng `..` cho **mọi** tệp hợp lệ trong repo, `isConfined`
trả false, và gate từ chối chính input của nó.

Thông báo lỗi là phần tệ nhất: `real path escapes repository root` là một cáo buộc path-traversal,
nhắm vào input, trong khi thủ phạm là biến môi trường. Người debug sẽ đi tìm ở sai chỗ.

**`b3-telephony-evidence-validator.mjs:872` đã sửa đúng** — nó realpath root trước khi so. Bản sửa
đó không bao giờ tới chín bản còn lại, vì **bản sửa không lan qua bản sao được**.

### 2.1 Sửa nguyên nhân, không sửa triệu chứng

Vá mười bản là để lại đúng cơ chế đã sinh ra lỗi. Nên: `deploy/ci/scripts/repository-path-lib.mjs`
xuất `REPOSITORY_ROOT` (đã realpath) và `isConfined`, và mười validator import từ đó.

Kèm dọn: `dirname`, `relative`, `isAbsolute`, `sep`, `SCRIPT_PATH`, `fileURLToPath` giờ chết ở các
file đó — đã gỡ, chỉ gỡ khi định danh không còn xuất hiện ở đâu khác trong file.

Hai semantics đã phân hoá cũng được hợp nhất. Năm bản viết `!rel.startsWith("..")`, ba bản viết
`rel !== ".." && !rel.startsWith("..${sep}")`. Giữ dạng thứ hai: dạng đầu quá chặt, từ chối cả thư
mục hợp lệ tên `..cache`. Không dạng nào yếu hơn về traversal, nhưng một helper bảo mật tồn tại ở
hai hình thì không ai nói được cái nào là luật.

### 2.2 Đăng ký gate

`gate-sweep.mjs` đòi mọi tệp chạy được trong `scripts/` phải có trong `gate-invocations.json`. Lib
mới đăng ký theo đúng quy ước sẵn có của `tts-voice-acceptance-lib.mjs`:
`{"sweepable": false, "reason": "a library with no top-level invocation"}`.

Lần đầu tôi thêm mục này bằng `json.dumps`, và nó reformat cả tệp — 226 dòng đổi cho một mục 5
dòng. Đã hoàn nguyên và chèn bằng văn bản. Diff của một thay đổi phải bằng kích thước thay đổi đó.

### 2.3 Re-pin: cái nào sống, cái nào đóng băng

Sửa mười script làm drift các hash được ghim. Quét toàn repo tìm chính xác hash cũ tìm ra 11 vị trí,
thuộc **hai vai khác nhau** — và `W-0251` đã dựng sẵn luật cho việc này:

| Vai | Tệp | Xử lý |
| --- | --- | --- |
| Pin sống | `d06-revalidation-evidence-validator.mjs`, `external-decision-closure-validator.mjs`, `docs/evidence/W-0170/decision-closure-input.template.json` | **Cập nhật** — cùng commit với source |
| Bản ghi đóng băng | `docs/evidence/W-0180/`, `W-0182/`, `W-0188/` (`README.md` + `attested-sha256.txt`) | **Không đụng** |

`W-0251` viết rõ: *"W-0152 là bản ghi đóng băng thật, và nó **đúng** khi cũ. Sửa nó sẽ là làm hỏng
một bản ghi lịch sử."* Sáu tệp kia giờ không khớp HEAD, và đó là trạng thái **đúng** của một bản
attestation — nó chứng thực byte tại thời điểm work item của nó, không phải byte hôm nay.

Cũng đã kiểm tra `.gitattributes` trước khi tính hash, vì pin phụ thuộc line ending: dòng 56 có
`deploy/ci/scripts/*.mjs text eol=lf`. (Tôi grep thiếu ở lần đầu và suýt "sửa" một thứ không hỏng.)

## 3. Test

| TestId | Khẳng định |
| --- | --- |
| `SEC-ROT-06` | Không kiểu nào tới được từ provider mang `byte[]` hay tên gợi credential — kiểm bằng phản chiếu, vì một test chỉ *không gọi* thì vẫn xanh sau khi ai đó đặt property lại |
| `SEC-ROT-07` | Fingerprint tái lập được, khác nhau theo input, **không phải** SHA-256 thô, và tốn thời gian đo được |
| `PATH_CONFINEMENT_SINGLE_SOURCE_PASS` | Lib canonicalise root bằng `realpathSync`, và **không script nào** định nghĩa `isConfined` riêng |

`SEC-ROT-06` và `SEC-ROT-07` đã kiểm chứng **fail trên code chưa sửa** (hoàn nguyên tạm thời
`RotatingCredentialProvider.cs`, chạy lại, thấy hai đỏ).

Guard trong `ci-config-selftest.mjs` cũng đã chứng minh **đỏ được ở cả hai nửa**: thêm lại một
`function isConfined` cục bộ vào một validator → đỏ; bỏ `realpathSync` khỏi lib → đỏ ở một assertion
khác. `W-0126` đặt ra luật đó và `W-0250` vừa thấy một selftest vi phạm nó, nên chứng minh chứ không
khai báo.

## 4. Kết quả

```
Unit         659/659   (+2)
Integration  275/275
Contract      24/24
Chaos          8/8
             ─────────
             966/966
```

`dotnet build Ivr.sln` — 0 warning, 0 error.

13 selftest của validator và các script phụ thuộc chạy trực tiếp, đối chiếu token trong
`gate-invocations.json` chứ không phải token tôi đoán — hai lần đầu tôi đoán sai
(`W0183_…` thay vì `W0184_…`, và `CONTRACT_FREEZE_SELFTEST_PASS` thay vì
`CONTRACT_FREEZE_SELFTEST=PASS`) và suýt báo nhầm là hỏng.

Impact analysis trước khi sửa B7: `RotatingCredentialProvider` — **MEDIUM**, 6 caller trực tiếp
(2 production: `AdminCredentialSource`, `OrderCoreCredentialSource`; 4 test). Không caller nào chạm
`SecretBytes`, nên bỏ nó khỏi bề mặt công khai không phá gì.

## 5. Còn lại

**9/26 đã đóng.** Không còn HIGH. Còn:

- **B5** — `InMemoryIdempotencyStore` rò rỉ không giới hạn ở MOCK (`keyLocks` không bao giờ xoá,
  `records` không TTL).
- **P2** — 109 index, nhiều cái trên cột boolean. **Cần đo `pg_stat_user_indexes.idx_scan` trên môi
  trường thật trước khi drop** — với boolean gần như chắc chắn bằng 0, nhưng "gần như chắc chắn"
  không đủ để xoá index trên bảng hot-write.
- **S4** — phần còn lại của trùng lặp: `sha256` ×18, `assertExactKeys` ×13, `rejectDuplicateJsonKeys`
  ×8. Lượt này chỉ hợp nhất `isConfined`, cái đã thực sự gây hại.
- **S2** — 951 magic string. **P3**, **S6**, **S7**, và nhóm LOW còn lại.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
