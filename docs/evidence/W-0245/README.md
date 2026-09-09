# W-0245 — RÚT LẠI

> ## ⛔ Lượt này sai và đã bị `W-0246` rút, cùng ngày
>
> Kết luận trung tâm — *"`+60s` không tồn tại trong bất kỳ tài liệu ký nào"* — **sai**. Nó nằm trong
> `specs/_review/open-decisions-register.md`, dòng `OD-V1-17`, nguyên văn *"TTL = cửa sổ xác nhận
> + 60s"*, ghi là một phần của lượt `CLOSED 2026-09-05`.
>
> Tôi kiểm bốn nguồn — tài liệu ký, gói phương án `T-04`, register **trong spec V0.3**, và config —
> rồi tuyên bố đã kiểm hết. **Không kiểm `specs/_review/open-decisions-register.md`**, dù chính
> `od-v1-signoff-2026-09-05` khai nó ở header là *"Nguồn"*, và dù ghi chú trong đó nói thẳng
> *"trạng thái sống của từng dòng nằm ở register"*.
>
> Nên: `W-0208` **đúng**, mâu thuẫn `2.1` là **thật**, và lời rút của lượt này là lỗi.
>
> **Phần duy nhất của lượt này còn giá trị** là §3: bốn validator thật sự đã đỏ từ `W-0221`, và
> phương pháp sweep của tôi thật sự đọc dòng usage thành PASS. Phần đó đúng và đã được sửa.
>
> Bài học, ghi cho lượt sau: *"đã kiểm hết nguồn"* chỉ đúng khi **liệt kê được nguồn nào**, và khi
> danh sách đó bắt đầu từ nguồn mà chính tài liệu tự khai — không phải từ nguồn mình nghĩ ra.

# W-0245 — Con số tôi gán cho một chữ ký không có ở đó, và bốn validator đỏ suốt hai ngày

Ngày: 2026-09-09 · Baseline: `main@beddcef` · Trạng thái: **TESTS_PASS**.

Owner cho biết `Security`, `Product`, `Platform` trong bảng owner **chính là owner**. Điều đó mở
khoá lại nhiều mục đang chờ những đội không tồn tại — mục nặng nhất là `2.1`. Đi kiểm thì `2.1`
không nặng như nó tự mô tả, vì tiền đề của nó là **của tôi và sai**.

## 1. `OD-V1-17` không nêu TTL nào

Worklist `2.1` và IR-06 đều ghi `OD-V1-17` ký *"TTL = cửa sổ xác nhận + 60s"*. Nguyên văn dòng ký:

```text
| OD-V1-17 | Token dùng lại được, gắn task_id, trần số lần resolve |
```

Không có con số. Và `OD-V1-05` thì **hoãn TTL sang `OD-V1-17`**:

```text
| OD-V1-05 | Hợp đồng token: TTL theo OD-V1-17, audit mỗi lần resolve, … |
```

⇒ TTL **chưa từng được nêu**, không phải được nêu là `+60s`.

### Đã kiểm hết nguồn khả dĩ

| Nguồn | Có `+60s` không |
| --- | --- |
| `od-v1-signoff-2026-09-05.md` | **không** — chỉ ba dòng có chữ TTL, không dòng nào cho `OD-V1-17` |
| `T-04-dial-token.md` (gói bốn phương án trước khi ký) | **không** — phương án `d` là *"reusable có TTL"*, không kèm số |
| spec V0.3 register dòng `674` | **không** — chỉ nêu câu hỏi |
| config `src/**`, `deploy/**` | **không** — không có default TTL nào |

`60s` duy nhất trong bảng ký là **retry backoff** của `OD-V1-08`/`OD-V1-16`, **ở dòng liền kề**.

### Nó vào từ đâu

`docs/evidence/W-0208/README.md:8` — lượt của chính tôi — nêu thẳng, **không dẫn nguồn**. Rồi
IR-06 `§3.4.1` và `:1117` chép theo, và worklist chép theo IR-06.

**Hệ quả thật:** IR-06 là tài liệu M3 dựng theo, và nó đang bảo M3 rằng có một mâu thuẫn phải chờ
gỡ. Không có mâu thuẫn nào.

> Phần còn lại của `W-0208` **vẫn đúng**: `IT-INTAKE-DB-03` và bảng ba tầng mô tả **code**, không
> mô tả chữ ký. Chỉ tiền đề *"quyết định đã ký mâu thuẫn với code"* là sai.

## 2. `2.1` nhỏ hơn hẳn nó tự mô tả

Ba guard nhận **đúng một giá trị** — `dial_token_expires_at == window.ExpiresAt` — và **không gì đã
ký nói khác**. Không có gì phải sửa cùng lúc ở sáu chỗ.

Việc còn lại: **ký equality thành quyết định**. Và nay owner ký được, vì `Security` là owner.

Giá trị đó cũng tự đứng vững chứ không chỉ là quán tính: token hết hạn đúng cuối cửa sổ nghĩa là
không quyền quay số nào sống lâu hơn cửa sổ, còn cuộc đang gọi vẫn an toàn vì
`PostgresTelephonyDispatchStore` đã ràng `DialTokenExpiresAt <= lease.Deadline` riêng.

## 3. Và bốn validator đỏ từ `W-0221`, không ai thấy

Sửa IR-06 thì phải re-pin (bài học `W-0216`). Chạy thử **trước** khi sửa:

```text
d06-revalidation-evidence-validator      W0178_VALIDATION_FAILED: local source hash drift: IR-06
dial-token-production-bundle-validator   W0183_VALIDATION_FAILED: local source hash drift: IR-06
opt-out-suppression-bundle-validator     W0187_OPTOUT_BUNDLE_VALIDATION_FAILED: IR-06
upstream-session-signoff-validator       W0181_VALIDATION_FAILED: IR-06 drift
```

**Đã đỏ sẵn.** `W-0216` re-pin xong, rồi `W-0220` và `W-0221` sửa IR-06 tiếp mà không ghim lại.

### Vì sao sweep của tôi không thấy

Bốn cái này cần `--self-test`. Chạy trần thì chúng in **dòng hướng dẫn dùng** rồi thoát — và tôi đọc
`tail -1` như đọc kết quả. **Suốt phiên tôi đọc usage thành PASS.**

Đó là lỗi nghiêm trọng hơn bản thân bốn pin: một phương pháp kiểm sai thì mọi lượt "đã chạy 50 gate"
đều yếu hơn nó tự nhận.

### Đã sửa và quét lại toàn bộ

Re-pin 4 validator + template `W-0187` sang hash LF mới, rồi **quét mọi cặp `*_path`/`*_sha256`**
trong `deploy/ci/scripts/` và `docs/evidence/`:

```text
khớp 30 · LỆCH 0 · không tra được 0
```

Bốn cái vừa sửa là toàn bộ chỗ lệch.

## 4. Kiểm chứng

```text
4 validator --self-test    W0178/W0183/W0187/W0181 SELFTEST_PASS (trước: cả bốn FAILED)
quét pin toàn repo         30 khớp · 0 lệch
dotnet test Ivr.sln        949/949 (không đổi — lượt này không sửa code)
gate-status.mjs            GATE_STATUS_PASS
docs-selftest.mjs          API_DOCS_SELFTEST_PASS
```

Không sửa code, không đổi hành vi runtime. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **`2.1`** | **ký `dial_token_expires_at == window.ExpiresAt` thành quyết định** — code đã thế, chỉ thiếu chữ ký | Owner |
| `4d` | 20 tên cho bank D | Owner |
| `4c` | Sales còn phát dạng chỉ-có-quận không | Owner + dev M3 |
| `3c` | gắn vào `Activated → Sellable` | Owner |
| — | `legal_gate`: ý kiến ngoài, hay thêm `RISK_ACCEPTED` | Owner |

Và một việc của tôi: **đọc lại các mục `2.x` khác dưới cùng ánh sáng** — chúng đang chờ
`M3 + Security`, mà nay là owner cộng một dev.
