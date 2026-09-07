# W-0217 — Một work item vô hình, và một cầu nối tên bị thiếu

Ngày: 2026-09-07 · Baseline: `main@f736a0f` · Trạng thái: **TESTS_PASS**.

Rà những gì M8 còn tự làm được sau `W-0216`. Hai thứ — và cái thứ hai hoá ra khác hẳn điều bản audit
mô tả.

---

## 1. `W-0206` mang một commit nhưng không có dòng ledger nào

`W-0216` đưa `gate-status` sống lại và nó đếm 213 work item. `W-0206` không nằm trong số đó, ở đâu
cũng không.

Commit `4d0c761` mang id đó trong message và làm việc thật:

> Pipeline hosted đầu tiên sau thời gian dài **không phải fail — nó chưa từng được tạo.** GitLab trả
> `jobs:observability_helm:script config should be a string or a nested array of strings`.

Một dòng `grep -q 'name: OTEL_EXPORTER_OTLP_ENDPOINT' …`. Nháy đơn là quoting của **shell**, không
phải của YAML, nên đó là plain scalar — và plain scalar chứa `": "` là một cặp key/value. YAML đọc
thành mapping ở chỗ cần string, GitLab từ chối toàn bộ config.

Nửa đáng giá hơn, và commit đó tự nói ra: `ci-config-selftest.mjs` in `CI_CONFIG_SELFTEST_PASS` qua
mười lượt kiểm trong khi lỗi nằm ngay trong cây. **Một gate sinh ra để validate CI config lại mù
đúng lớp lỗi chặn đứng pipeline.** Nó đã được vá cùng lượt.

### Vì sao ghi hồi tố thay vì bỏ trống

`A-0588` chọn **bỏ qua** id này: ledger ghi `NEXT_WORK_ID=W-0206` nhưng một luồng song song đã dùng
id đó trong commit message mà chưa ghi dòng nào, nên dùng lại là phạm luật.

Đúng ở vế "không dùng lại". Nhưng bỏ qua để lại việc thật **vô hình với ledger và với readiness
board**. Luật ledger là *"Never reuse or renumber an issued ID, even if cancelled"* — id này **đã
được phát**, một commit đang mang nó. Ghi nó xuống mới là tuân luật; để trống là mất một work item.

Dòng ghi đúng những gì commit và diff nói, **không có evidence pack**: việc thuộc luồng song song,
tôi không viết evidence hộ người khác. Board giờ đếm **215**.

---

## 2. `C2` — không phải một loạt đổi tên

Bản audit và Codex đều nêu: bảng mapping result trong `m8-05` §3 chỉ có tên code, thiếu cột tên
trong spec, ví dụ `ATTEMPT_1_NO_ANSWER → IVR_NO_ANSWER_ATTEMPT`.

Đi tìm nguồn thì thấy khác. `ATTEMPT_1_NO_ANSWER` **có thật trong repo** —
`docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md`, mục *"Các kết quả tạm thời"* — và nó
không đứng một mình:

```text
ATTEMPT_1_NO_ANSWER      ATTEMPT_1_NO_INPUT
ATTEMPT_1_INVALID_INPUT  ATTEMPT_1_BUSY
```

Business source đặt tên theo **lượt gọi và lý do**. Runtime đặt tên theo **kết quả**:

| Business source | Runtime |
| --- | --- |
| `ATTEMPT_1_NO_ANSWER` | `IVR_NO_ANSWER_ATTEMPT` · `reason = RING_TIMEOUT` |
| `ATTEMPT_1_BUSY` | `IVR_NO_ANSWER_ATTEMPT` · `reason = BUSY` |
| `ATTEMPT_1_NO_INPUT` | `IVR_NO_ANSWER_ATTEMPT` · `reason = ANSWERED_NO_INPUT` |
| `ATTEMPT_1_INVALID_INPUT` | `IVR_WRONG_INPUT` |

Bốn tên thành **một** result code cộng một trường `reason`, và lượt gọi chuyển sang
`attempt_number`. **Thông tin không mất — nó đổi chỗ.**

Nên mô tả "đổi tên 1:1" là sai, và một cột "tên spec" cạnh mỗi dòng cũng sẽ sai: ba dòng business
source cùng trỏ về một dòng runtime.

**Hệ quả thật cho M3:** consumer đi tìm bốn result code riêng cho bốn tình huống đó **sẽ không bao
giờ thấy**. Phân biệt nằm ở `reason`. Đó là thứ phải nói ra trước khi M3 ký `§4.2`.

Đã thêm `m8-05` **§3.1**, mỗi dòng kèm nguồn, cộng hai dòng đã có sẵn trong errata V0.3:
`IVR_OPT_OUT` → không phải result code (errata #6), và ba code V0.2 §13 không có (errata #5).

### Chỗ tôi không tự nhận là đã chốt

Bốn dòng đầu của cầu nối **chưa có trong errata V0.3** — errata mới chỉ ghi `IVR_OPT_OUT` và ba code
thừa. Cách đọc `PACK-09` ở trên là của tôi, dựng từ hai nguồn trong repo. `§3.1` ghi rõ cần chief
auditor/Owner xác nhận trước khi M3 ký.

---

## 3. Thứ tôi **không** làm

**52 work item vẫn `evidence: null`**, và tôi đã kiểm: **không cái nào có file evidence trên đĩa**.
Nên đây không phải lỗi thiếu liên kết mà là evidence chưa từng được viết. Đó là công việc của người
khác; viết evidence hộ cho việc mình không làm thì evidence mất hết ý nghĩa. Ghi lại để mục `A3` của
worklist biết con số thật.

## 4. Kiểm chứng

```text
gate-status.mjs                        GATE_STATUS_PASS — 11 gates, 215 work items, 5 decisions
                                       (213 → 215: W-0206 hồi tố + W-0217)
dotnet test Ivr.sln                    897/897 PASS, 0 failed, 0 skipped
contract-freeze-verifier.mjs           CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check TEST_TRACEABILITY_CURRENT=556 (không đổi)
docs-selftest.mjs                      API_DOCS_SELFTEST_PASS
quét pin toàn repo                     32 khớp / 0 lệch
```

Không sửa runtime, không thêm/xoá test. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Còn lại

Không còn mục nào trong worklist mà M8 tự đóng được. Phần còn lại là chữ ký có tên — `X1`, `B5`,
`0.1`, `0.2`, `0.3`, `0.5` — cộng `B6` cần một database đích thật, và `B12` cần mua SIM.
