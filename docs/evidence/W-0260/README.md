# W-0260 — Bản kiểm secret thành luật, và cái luật đó hoá ra sai một chỗ

Ngày: 2026-09-09 · Baseline: `main@90eaffe` · Trạng thái: **TESTS_PASS**.

`W-0259` đưa ra một câu hỏi cho owner: trong bảy bản `assertIdentifier`, **hai bản kiểm secret và
năm bản không** — bản nào là luật?

Owner chốt ngày `2026-09-09`: **bản có kiểm secret là luật.** Lượt này thi hành quyết định đó.

Thi hành nó làm lộ ra rằng chính cái luật ấy có một lỗi, và lỗi đó chỉ hiện khi áp nó lên validator
chưa từng chạy nó.

## 1. Không thể "chỉ thêm lời gọi"

Map thực tế trước:

| Validator | có `assertIdentifier` | gọi kiểm secret | có hàm kiểm secret |
| --- | :---: | :---: | :---: |
| `attempt-policy-production-bundle` | ✓ | ✗ | ✗ |
| `capacity-registry-decision-pack` | ✓ | ✗ | ✗ |
| `d06-revalidation-evidence` | ✓ | ✗ | ✗ |
| `dial-token-production-bundle` | ✓ | ✗ | ✗ |
| `external-decision-response` | ✓ | ✗ | ✓ |
| `external-decision-closure` | ✓ | ✓ | ✓ |
| `target-v1-shared-e2e-report` | ✓ | ✓ | ✓ |
| `external-decision-routing` | — | — | ✓ |
| `upstream-session-signoff` | — | — | ✓ |

**Bốn trong năm file cần sửa không có hàm kiểm secret nào cả.** Nên câu hỏi không phải "thêm lời
gọi" mà là "gọi bản nào" — trong khi `assertNoSensitiveValue` có 5 bản với 5 cài đặt khác nhau.

Quyết định của owner chỉ có nghĩa nếu có **một** thứ để trỏ vào. Đó là
`deploy/ci/scripts/sensitive-value-lib.mjs`.

## 2. Hợp nhất: lấy hợp, trừ một thứ không thuộc về

Năm bản khác nhau ở đâu:

| Kiểm | closure | response | routing | target-v1 | upstream |
| --- | :---: | :---: | :---: | :---: | :---: |
| email | ✓ | ✓ | ✓ | ✓ | ✓ |
| phone | ✓ | ✓ | ✓ | ✓ | ✓ |
| địa chỉ | ✓ | ✓ | ✓ | ✓ | ✓ |
| từ khoá credential | ✓ | ✓ | ✓ | ✓ | ✓ (`bearer` rộng hơn) |
| **JWT `eyJ…`** | ✗ | ✓ | ✓ | ✗ | ✓ |
| **`[?#]`** | ✗ | ✗ | ✗ | ✓ | ✗ |

Bản chuẩn lấy **hợp**, gồm dạng `bearer(?:\s+|[:=])` rộng nhất (bắt được `BEARER:token` mà bốn bản
kia bỏ sót) và kiểm JWT. Lấy bản rộng nhất chính là lý do để hợp nhất thay vì chọn đại một bản.

**Trừ `[?#]`.** Nó không phải kiểm secret — thông điệp của chính nó là *"must not contain a URL
query or fragment"*, và một dấu `?` không phải dữ liệu cá nhân. Gộp nó vào luật chung sẽ áp nó lên
sáu validator chưa từng yêu cầu. Nó ở lại `target-v1` dưới tên riêng
`assertNoUrlQueryOrFragment`, đúng vai nó vẫn luôn có.

## 3. Luật sai một chỗ, và selftest bắt được chứ không phải phép đo của tôi

### 3.1 Tôi đo trước, và đo sai

Trước khi sửa, tôi instrument `assertIdentifier` ở cả bảy validator để ghi lại **mọi giá trị nó
thật sự nhận** khi chạy selftest — 365 giá trị riêng biệt — rồi áp bộ kiểm chuẩn lên đúng tập đó.

Kết quả: **8/365 bị từ chối, và cả 8 đều là fixture âm** (`BEARER SECRET`, `USER@EXAMPLE.COM`,
`CALL:+84912345678`…) mà selftest cố tình đưa vào để chứng minh validator từ chối chúng. Tôi kết
luận: an toàn.

**Kết luận đó sai.** Chạy selftest thật thì d06 đỏ:

```
W0178_VALIDATION_FAILED: candidate.config_version contains a phone-like value
```

Giá trị là `CONFIG-2026-09-04-01` — một config version hoàn toàn hợp lệ. Phép đo của tôi không bắt
được nó; selftest bắt được. Đó chính là lý do tôi chạy selftest chứ không dừng ở probe.

### 3.2 Vì sao regex phone bắt nhầm

```
/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u
```

Nó đếm 9–15 **nhóm chữ số** ngăn bởi bất cứ gì trong `[\s().-]`, và dấu `-` nằm trong tập đó.
`CONFIG-2026-09-04-01` có 10 chữ số ngăn bởi `-` → khớp.

Đây không phải trường hợp hiếm. Từ vựng identifier của hệ thống này đầy chuỗi mang ngày tháng:

| Giá trị | Chữ số | Ở đâu |
| --- | ---: | --- |
| `CONFIG-2026-09-04-01` | 10 | selftest của d06 |
| `M8-07-SECTION-6.2026-09-04` | 12 | `docs/evidence/W-0174/shared-e2e-report.template.json` |
| `C10-C11-C13-D06.2026-09-04` | 16 | `docs/evidence/W-0178/d06-revalidation-evidence.template.json` |

Hai bản đã có sẵn kiểm secret không lộ lỗi này chỉ vì chúng không bao giờ cho giá trị mang ngày đi
qua `assertIdentifier` — `SOURCE_PINS` được so bằng đúng, không validate.

### 3.3 Carve-out, và vì sao nó không mở lỗ

Ngày ISO bị cắt khỏi giá trị **chỉ cho phép thử phone**. Mọi pattern khác vẫn thấy giá trị nguyên
vẹn, nên không thể lách chúng bằng cách đặt một ngày cạnh bên.

Mẫu ngày ghim năm/tháng/ngày hợp lệ chứ không để lỏng `\d{4}-\d{2}-\d{2}`, và đây là chỗ then chốt:

```
0912-34-5678
```

Đó là một số điện thoại đội lốt ngày. Mẫu lỏng sẽ cắt `0912-34-56` và để lọt phần còn lại. Mẫu
`(?:19|20)\d{2}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])` thì không: `0912` không phải `19xx`/`20xx`,
`34` không phải tháng. Không thể xếp một số điện thoại thành hình dạng đó.

## 4. Kết quả trên `assertNoSensitiveValue`

```
trước:  5 bản sao / 5 cài đặt khác nhau
sau:    9 bản sao / 1 cài đặt
```

Chín bản sao còn lại đều là **cùng một wrapper một dòng** trên luật dùng chung — chúng tồn tại vì
mỗi validator có `fail()` riêng với exit code và tiền tố thông điệp riêng, và một helper chung mà
tự kết thúc tiến trình sẽ phải chọn một trong số đó. `findSensitiveValue` trả về lý do; mỗi caller
giữ quy ước của mình.

`assertIdentifier` vẫn là **7 bản / 7 cài đặt**, và đó là đúng: khác biệt còn lại là giới hạn độ
dài, charset và tuỳ chọn placeholder — những thứ quyết định của owner không đụng tới. Cái đã thống
nhất là **luật**, và cả bảy giờ đều chạy nó.

## 5. Test

| Token | Khẳng định |
| --- | --- |
| `SENSITIVE_VALUE_RULE_PASS` | 12 trường hợp: 4 identifier hợp lệ mang ngày đi qua; 6 dạng nhạy cảm vẫn bị chặn; `BEARER:token` (chỉ 1/5 bản cũ bắt được) bị chặn; `0912-34-5678` vẫn bị chặn |

Kiểm chứng **cả hai cạnh** của carve-out bằng cách làm chúng đỏ thật:

- Bỏ carve-out → `findSensitiveValue("CONFIG-2026-09-04-01") returned "a phone-like value", expected null`
- Nới mẫu ngày thành `\d{4}-\d{2}-\d{2}` → `findSensitiveValue("0912-34-5678") returned null, expected "a phone-like value"`

Một carve-out trong lớp kiểm bảo mật phải chứng minh được nó không mở lỗ, không phải chỉ tuyên bố.

## 6. Re-pin

Sửa các validator lại làm drift pin, đúng ba pin sống như `W-0258`:
`d06-revalidation-evidence-validator.mjs`, `external-decision-closure-validator.mjs`,
`docs/evidence/W-0170/decision-closure-input.template.json`.

Bản ghi đóng băng dưới `W-0180`/`W-0182`/`W-0188` **không xuất hiện lần này** — chúng đã lệch từ
`W-0258` và vẫn phải giữ nguyên. Đó là hành vi đúng của một attestation, không phải thiếu sót.

## 7. Kết quả

```
Unit         662/662
Integration  275/275
Contract      24/24
Chaos          8/8
             ─────────
             969/969
```

`GATE_SWEEP_PASS` · `dotnet build Ivr.sln` — 0 warning, 0 error.

11 selftest của validator và script phụ thuộc chạy trực tiếp, đối chiếu token trong
`gate-invocations.json`.

## 8. Còn lại

**12/26 đã đóng.** Không còn HIGH. Hai mục cần owner, không cần thêm công:

- **P2** — 109 index, nhiều cái trên cột boolean. Cần `pg_stat_user_indexes.idx_scan` từ staging
  hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng nên `ivr_idempotency_keys` không có hạn dùng.

**S4 phần còn lại** giờ thuần kỹ thuật và không còn nguy hiểm: `assertString` (11/9),
`assertExactKeys` (13/8), `rejectDuplicateJsonKeys` (8/5), `readStrictJson` (6/5). Không cái nào
là kiểm bảo mật; census guard giữ chúng không tăng thêm.

Còn lại: **S2** (951 magic string), **P3**, **S6**, **S7**, nhóm LOW.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
