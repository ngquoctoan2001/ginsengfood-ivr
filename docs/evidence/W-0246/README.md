# W-0246 — Owner ký equality cho `2.1`, và tôi rút lại lượt sai của chính mình

Ngày: 2026-09-09 · Baseline: `main@55d552d` · Trạng thái: **TESTS_PASS**.

Hai việc trong một lượt, và việc thứ hai phải làm trước.

## 1. `W-0245` sai, rút

Một giờ trước tôi khẳng định `+60s` *"không tồn tại trong bất kỳ tài liệu ký nào"*, đã kiểm
`od-v1-signoff-2026-09-05`, gói phương án `T-04-dial-token`, register **trong spec V0.3**, và config.

Nó nằm ở **`specs/_review/open-decisions-register.md`**, dòng `OD-V1-17`:

```text
✅ CLOSED 2026-09-05 — chọn phương án (d): token dùng lại được, gắn cứng vào task_id,
   TTL = cửa sổ xác nhận + 60s, số lần resolve tối đa = max_customer_attempts + …
```

Và đó **chính là nguồn `od-v1-signoff-2026-09-05` khai ở header** — dòng thứ năm của file:
`Nguồn: open-decisions-register.md`. Cùng file còn ghi thẳng *"trạng thái sống của từng dòng nằm ở
register"*.

Tôi kiểm bốn nguồn tôi nghĩ ra, rồi tuyên bố đã kiểm hết, mà **không kiểm nguồn tài liệu tự khai**.

⇒ `W-0208` **đúng cả tiền đề lẫn kết luận**. Mâu thuẫn `2.1` là **thật**.

> Bài học ghi cho lượt sau: *"đã kiểm hết nguồn"* chỉ đúng khi **liệt kê được nguồn nào**, và danh
> sách đó phải bắt đầu từ nguồn mà chính tài liệu tự khai — không phải từ nguồn mình nghĩ ra.

**Phần duy nhất của `W-0245` còn giá trị** là §3: bốn validator thật sự đỏ từ `W-0221`, và sweep của
tôi thật sự đọc dòng usage thành PASS. Phần đó đúng và đã sửa.

### Đã rút ở đâu

| File | Sửa |
| --- | --- |
| `docs/evidence/W-0245/README.md` | banner rút ở đầu, **không xoá file** |
| `docs/evidence/W-0208/README.md` | gỡ banner sai, thay bằng ghi chú `W-0208` đúng |
| `integration-requirements/06-…handover.md` | gỡ hai đoạn sai, thay bằng quyết định đã ký |
| `plan/toan-viec-can-lam-m8-…md` `2.1` | gỡ lời rút, thay bằng mục đóng |

## 2. Owner ký: TTL = **đúng** window end

Thay thế vế `"TTL = cửa sổ xác nhận + 60s"` của `OD-V1-17`.

`+60s` chưa từng thi hành được, và không vì thiếu guard — vì **thừa ba**:

| Tầng | Luật |
| --- | --- |
| Intake | từ chối nếu expiry **<** window end |
| Persistence | throw nếu expiry **>** window end |
| Dispatch | throw nếu expiry **>** `lease.Deadline` |

Giao hai luật đầu là **đúng một giá trị**. Task dựng theo `+60s` qua intake rồi ném ở persistence.

### Vì sao equality đứng vững, không chỉ là chấp nhận hiện trạng

Token hết hạn đúng cuối cửa sổ nghĩa là **không quyền quay số nào sống lâu hơn cửa sổ gọi** — tính
chất muốn có, không phải hệ quả tình cờ.

Và điều `+60s` định bảo vệ — cuộc bắt đầu sát mép vẫn quay xong — **đã có cơ chế riêng lo**: dispatch
ràng `DialTokenExpiresAt <= lease.Deadline`. `+60s` là giải pháp thứ hai cho một vấn đề đã giải, và
nó mua thêm 60 giây quyền quay số **sau khi cửa sổ đóng**.

## 3. Hệ quả — nhỏ hơn mọi tài liệu từng nói

| | |
| --- | --- |
| Code | **không đổi một dòng** — ba tầng đã ép equality |
| M3 | **không đổi gì** — equality là thứ M3 vẫn gửi |
| OpenAPI | không đổi; lượt phát hành contract thôi phải chờ mục này |
| `IT-INTAKE-DB-03` | đổi **nghĩa** mà không đổi dòng nào: từ ghim *"quyết định đã ký không thi hành được"* sang ghim **chính quyết định** |
| Worklist `2.1` | **đóng** |

Chỗ cuối đáng nói: cùng một test, cùng ba case, nhưng trước đây nó là **bằng chứng của một mâu
thuẫn**, nay là **bằng chứng của một hợp đồng**. Chỉ comment đổi.

## 4. Kiểm chứng

```text
register OD-V1-17           vế TTL cập nhật, dẫn W-0246
signoff mới                 plan/ivr-orther/od-v1-17-ttl-signoff-2026-09-09.md
re-pin IR-06                4 validator + template W-0187 → hash LF mới
quét pin toàn repo          30 khớp · 0 lệch
4 validator --self-test     W0178 / W0183 / W0187 / W0181 SELFTEST_PASS
IT-INTAKE-DB-03             4/4 PASS (chỉ comment đổi)
dotnet test Ivr.sln         949/949 PASS, 0 failed, 0 skipped
gate-status.mjs             GATE_STATUS_PASS
docs-selftest.mjs           API_DOCS_SELFTEST_PASS
```

Không sửa code runtime. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| `4d` | 20 tên cho bank D | Owner |
| `4c` | Sales còn phát dạng chỉ-có-quận không | Owner + dev M3 |
| `3c` | gắn vào `Activated → Sellable` | Owner |
| — | `legal_gate`: ý kiến ngoài, hay thêm `RISK_ACCEPTED` | Owner |

Và việc của tôi: đọc lại `2.2`–`2.6` dưới ánh sáng *"Security/Product là owner"* — `2.1` vừa cho
thấy một mục có thể nhỏ hơn nhiều so với cách nó tự mô tả.
