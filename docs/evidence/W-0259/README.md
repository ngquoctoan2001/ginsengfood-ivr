# W-0259 — Chặn hai chiều tăng: bộ nhớ của một store, và bản sao của một helper

Ngày: 2026-09-09 · Baseline: `main@5d2ea1b` · Trạng thái: **TESTS_PASS**.

B5 và S4 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

**S4 chỉ đóng một phần, và phần không đóng là có chủ ý.** Mục 2.3 nói rõ tại sao, kèm số liệu.

## 1. B5 — hai từ điển không có trần

`InMemoryIdempotencyStore` là singleton, bị từ chối ngoài MOCK — nhưng **MOCK là mặc định của image
đang ship** (`IVR_EXECUTION_MODE=MOCK` trong `Dockerfile.api`), nên đây là store mà mọi soak run,
E2E dài ngày và pilot đều dùng. Đó là đúng chỗ mà "nó chết cùng tiến trình" không phải câu trả lời,
vì tiến trình được thiết kế để sống.

Hai rò rỉ riêng biệt:

| | Trước | Sau |
| --- | --- | --- |
| `keyLocks` | một `SemaphoreSlim` cho **mỗi** idempotency key, không bao giờ xoá, không bao giờ `Dispose` | mảng cố định 256 stripe |
| `records` | không TTL, không eviction; `CreatedAt` được **ghi và không ai đọc** | cửa sổ 24h + trần 10.000, quét có hạn nhịp |

### 1.1 Vì sao stripe chứ không phải đếm tham chiếu

Cách sửa thông thường cho per-key lock là đếm tham chiếu rồi xoá cái cuối cùng ra khỏi. Đó là một
nguồn race đã có tên: giữa lúc caller này `Release` và caller kia `GetOrAdd`, cái lock có thể bị gỡ
khỏi từ điển và hai caller cùng key đi vào cùng lúc.

Một mảng không bao giờ lớn lên thì không rò được, và không có race nào để hỏng. Cái giá là hai key
không liên quan chia nhau một stripe sẽ tuần tự hoá với nhau — mất một chút song song, **không mất
đúng đắn**, vì cái lock này chỉ sắp thứ tự caller chứ không quyết định gì.

`StripeOf` là FNV-1a viết tay chứ không dùng `string.GetHashCode()`, vì cái đó randomise theo tiến
trình: stripe của một key sẽ khác nhau giữa các lần chạy, và một test khẳng định hai key cụ thể
không đụng nhau có thể xanh cả tuần rồi đỏ trên CI.

### 1.2 Cửa sổ 24 giờ: chọn, không kế thừa

Không có gì để kế thừa. `Ivr:Retention:PeriodDays` ship rỗng, mà `RetentionOptions` ghi rõ *"missing
periods deliberately leave the corresponding class unarmed"* — nên `idempotency_key` **chưa được
arm**, và bảng `ivr_idempotency_keys` phía Postgres cũng không có hạn dùng nào đang chạy.

> Phát hiện phụ, không sửa ở đây: đó là quyết định cấu hình chứ không phải lỗi mã, và đặt một
> retention period là chính sách của owner. Nhưng nó có nghĩa là **bảng Postgres cũng đang lớn không
> giới hạn**, cùng một hình dạng với lỗi vừa sửa, chỉ ở tầng khác. Xếp cạnh B10 (cột
> `ExpiresAt` chết nhưng vẫn có index) — cả hai cùng trỏ vào một chỗ chưa ai đóng.

Một ngày dài hơn mọi cửa sổ retry mà hệ thống này có, và ngắn đủ để một soak run không tích lại
một tuần snapshot.

**Nó đổi một ngữ nghĩa, và điều đó đáng nói thẳng:** một retry đến sau cửa sổ sẽ **chạy lại** thay
vì replay. Đó đúng là thứ retention làm với `ivr_idempotency_keys` khi có period — một store biết
hết hạn là thiết kế, không phải nhân nhượng — còn lựa chọn thay thế là lớn đến khi tiến trình chết,
tức mất **toàn bộ** key một lúc thay vì mất cái cũ nhất.

Trần 10.000 tách khỏi cửa sổ vì nó tồn tại cho đúng cái burst mà nhịp quét cho lọt qua.

### 1.3 Không implement `IDisposable`

256 semaphore sống hết đời tiến trình và chết cùng nó. Dispose chúng lúc shutdown mở ra
`ObjectDisposedException` cho caller đang chờ, đổi một rò rỉ không tồn tại lấy một race có thật.

## 2. S4 — kiểm kê trước, rồi mới quyết

`W-0258` hợp nhất `isConfined` vì các bản đã phân hoá **và** sự phân hoá đó đã trả giá: bản sửa
realpath tới được 1 trong 10. Câu hỏi tự nhiên là phần còn lại thì sao. Nên: đếm.

### 2.1 Số liệu

Thân hàm được chuẩn hoá khoảng trắng rồi băm, nên định dạng lại một bản không bị tính là một cài
đặt mới. Cái gì sống sót qua đó là khác biệt thật.

| Helper | Bản sao | Cài đặt khác nhau |
| --- | ---: | ---: |
| `assertIdentifier` | 7 | **7** |
| `assertNoSensitiveValue` | 5 | **5** |
| `assertString` | 11 | 9 |
| `assertExactKeys` | 13 | 8 |
| `rejectDuplicateJsonKeys` | 8 | 5 |
| `readStrictJson` | 6 | 5 |
| `sha256` | 16 | 3 |
| `assertTimestamp` | 8 | 3 |
| `assertSha256` | 5 | 3 |
| `clone` | 12 | 2 |
| `assert` | 12 | 2 |

`assertIdentifier`: bảy bản, bảy cài đặt — **không hai bản nào giống nhau**.
`assertNoSensitiveValue`, tức lá chắn chặn một secret lọt vào evidence bundle: năm bản, năm cài đặt.

### 2.2 Khác biệt là thật, và có tải trọng

Đọc thật bảy bản `assertIdentifier` thay vì tin con số. Chúng **không** phải bảy validator cho bảy
định dạng ID khác nhau — 5/7 dùng đúng cùng một regex và đúng cùng một thông điệp lỗi. Đây là drift,
không phải chuyên biệt hoá.

Nhưng khác biệt ở những bản còn lại thì có tải trọng:

- `capacity-registry` thêm `+` vào charset
- `d06` thêm giới hạn độ dài `{2,127}` và một tuỳ chọn placeholder
- `external-decision-closure` và `target-v1-shared-e2e` gọi `assertNoSensitiveValue` sau khi kiểm —
  **năm bản kia thì không**

Cái cuối là phát hiện đáng kể: cùng một tên hàm, cùng một mục đích đã ghi, **khác nhau về thế đứng
bảo mật**.

### 2.3 Vì sao không hợp nhất — và điều này cần owner quyết

Chọn một tập ngữ nghĩa làm chuẩn sẽ **đổi việc gate chấp nhận evidence bundle nào**, theo một chiều
chưa ai quyết. Lấy bản chặt nhất thì bundle từng qua có thể trượt; lấy bản lỏng nhất thì mất một
lớp kiểm. Đó là quyết định của chủ sở hữu, không phải của một lượt refactor.

Nhóm tầm thường (`sha256`, `assert`) thì hợp nhất **được** an toàn — 15/16 bản `sha256` chỉ khác tên
tham số, cả 12 bản `assert` chỉ khác cặp ngoặc. Nhưng tôi đã đo chi phí trước:

```
files that would be touched: 25
of those, pinned elsewhere:  4
  capacity-data-intake-validator.mjs -> ghim ở 8 nơi (gồm 1 template sống và 2 bản ghi đóng băng)
```

Diff 25 tệp cộng một đợt re-pin qua 4 tệp — **để đổi lấy không một thay đổi hành vi nào**. Với hai
hàm một dòng chưa hề phân hoá có ý nghĩa, đó là tỉ lệ sai. Cái nguy hiểm không nằm ở đấy.

### 2.4 Cái làm được mà không phải quyết gì

Chốt hiện trạng. `deploy/ci/validator-helper-census.json` ghi lại con số, và
`ci-config-selftest.mjs` tính lại rồi so.

Đỏ **cả hai chiều**, có chủ ý:

- thêm một bản sao → đỏ (*"import the helper instead"*)
- bỏ một bản sao → cũng đỏ, kèm hướng dẫn hạ baseline

Chiều thứ hai không phải phiền nhiễu: nếu chỉ chặn tăng, baseline sẽ mục dần và nói dối về hiện
trạng. Một bản ghi mà chỉ đúng theo một hướng thì không phải bản ghi.

Kiểm chứng cả hai chiều bằng cách làm chúng đỏ thật: thêm một `assertIdentifier` vào
`gate-status.mjs` → `8 copies / 8 implementations, baseline 7 / 7`; đặt baseline thành 99 →
`7 copies / 7 implementations, baseline 99 / 7`.

## 3. Test

| TestId | Khẳng định |
| --- | --- |
| `UT-FND-IDEMP-05` | 500 key với trần 50 → `Count ≤ 50`, và key **mới nhất** vẫn replay không chạy lại factory (chứng minh nó đuổi cái cũ nhất) |
| `UT-FND-IDEMP-06` | Trong cửa sổ thì replay; qua cửa sổ thì chạy lại |
| `UT-FND-IDEMP-07` | Không còn trường `ConcurrentDictionary<_, SemaphoreSlim>`; có `SemaphoreSlim[]` |
| `VALIDATOR_HELPER_CENSUS_PASS` | Bản sao helper đứng yên ở baseline |

`UT-FND-IDEMP-07` kiểm bằng **hình dạng** chứ không bằng hành vi, vì rò rỉ này vô hình từ bên ngoài:
một từ điển lock per-key trông y hệt một cái có giới hạn cho tới lúc hết bộ nhớ.

Không thể kiểm chứng `UT-FND-IDEMP-05/06` bằng cách hoàn nguyên source — constructor đổi chữ ký nên
test sẽ không biên dịch. Thay vào đó dựng cùng store với trần và cửa sổ vô hiệu hoá:

```
unbounded store retained: 500  (assertion 'Count <= 50' would FAIL)
bounded store retained:   50   (ceiling 50)
```

Assertion là load-bearing, không đúng một cách tầm thường.

## 4. Kết quả

```
Unit         662/662   (+3)
Integration  275/275
Contract      24/24
Chaos          8/8
             ─────────
             969/969
```

`GATE_SWEEP_PASS` · `dotnet build Ivr.sln` — 0 warning, 0 error.

Impact analysis trước khi sửa B5: `InMemoryIdempotencyStore` — **LOW**, 0 caller truy được
(lower-bound: bind qua `IIdempotencyStore` nên DI không truy ra).

## 5. Còn lại

**11/26 đã đóng.** Không còn HIGH. Hai mục sau cần owner quyết, không phải cần thêm công:

- **S4 phần còn lại** — chọn ngữ nghĩa chuẩn cho `assertIdentifier` / `assertNoSensitiveValue` /
  `assertString` / `assertExactKeys`. Điểm cụ thể để bắt đầu: **2 trong 7 bản `assertIdentifier`
  kiểm secret, 5 bản không.** Bản nào là luật?
- **P2** — 109 index, nhiều cái trên cột boolean. Cần `pg_stat_user_indexes.idx_scan` từ staging
  hoặc production trước khi drop.
- **Retention chưa arm** — `PeriodDays` rỗng nên `ivr_idempotency_keys` không có hạn dùng. Đặt
  period là chính sách.

Còn lại thuần kỹ thuật: **S2** (951 magic string), **P3**, **S6**, **S7**, và nhóm LOW.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
