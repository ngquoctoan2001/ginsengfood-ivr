# W-0226 — Hai phiếu để gửi, và chỗ rẽ chúng phải nói ra

Ngày: 2026-09-08 · Baseline: `main@9abdb95` · Trạng thái: **TESTS_PASS**.

Owner yêu cầu soạn phiếu Platform và phiếu Legal/Privacy theo đúng hai bộ trường gate đòi. Đi tìm
chỗ đặt thì phát hiện **cả hai đã tồn tại**, và đã bị xoá.

## 1. Không soạn mới — khôi phục

```text
git log --diff-filter=D --name-only -- 'plan/ivr-orther/questions-to-*'
→ 8ed62e9  2026-09-04  "save"
     plan/ivr-orther/questions-to-legal-od-voice-07.md            (143 dòng)
     plan/ivr-orther/questions-to-platform-w0122-infrastructure.md (176 dòng)
     plan/ivr-orther/today-03-tts-handoff-pack-2026-08-29.md      (110 dòng)
```

`8ed62e9` là lượt dọn 31 file, **−18.251 dòng**. `00-index.md` tự mô tả lượt đó là gỡ các phiếu
`questions-to-*` **đã `SUPERSEDED`**, thuộc vòng `2026-07-02` và `OD-15`.

Hai phiếu này **không thuộc diện đó**: lập `2026-08-28` cho `W-0122`, chưa từng bị supersede, và
trạng thái tự khai của cả hai vẫn là `READY_TO_DISPATCH / NOT_SENT`. Chúng bị cuốn theo một lượt
dọn mà mô tả của chính lượt đó không bao gồm chúng.

Viết lại từ đầu sẽ mất phần đắt nhất: `L1`–`L7` và `INF-A1`–`INF-C4` đã được đặt câu rất chính xác,
kèm bảng pin exact, ba khoảng trống dự án tự nhận, và bảng "IVR sẽ làm gì sau khi có trả lời".
Nên: khôi phục từ `8ed62e9^`, rồi cập nhật.

## 2. `W-0122/README` nói sai về chỗ nhận

Ba dòng ở `## Gate còn cần con người/hạ tầng` ghi intake Legal và Platform *"đi qua `W-0185`"*.
Kiểm `docs/evidence/W-0185/b3-telephony-evidence.template.json` thì W-0185 có:

```json
{ "role": "LEGAL_PRIVACY_APPROVAL",    "artifact_ref": "PENDING", "sha256": "PENDING", … }
{ "role": "INTERNAL_MIRROR_ATTESTATION", "artifact_ref": "PENDING", "sha256": "PENDING", … }
{ "internal_mirror_ready": false }
```

Đó là **biểu nhận** — chỗ ghi lại rằng *đã có* phê duyệt và hash của nó. Nó **không** chứa 13 cặp
`internal_mirror_uri`/`internal_mirror_digest` mà `MODELS.lock` cần, và không chứa một câu hỏi nào
của `L1`–`L7`.

**Thay một bộ câu hỏi bằng một biểu nhận thì không còn gì để bên kia trả lời.** Đã sửa ba dòng đó:
phiếu là thứ gửi đi, W-0185 là chỗ ghi cái nhận về. Hai thứ bổ sung nhau, không thay nhau.

## 3. Cập nhật theo `W-0225`

### Phiếu Platform

- `INF-A2` thêm **định dạng bắt buộc**: `internal_mirror_digest` khớp `^(sha256:)?[a-f0-9]{64}$`.
- **`INF-A4` mới** — hồ sơ quyết định `decided_by` / `approval_reference` / `decided_on`
  (`YYYY-MM-DD`), thứ `W-0225` vừa bắt buộc. Nói thẳng rằng **trước** `W-0225` cổng này mở được
  bằng ba chữ, nay không.
- Ghi rõ `decision_authority` **cố ý không bị chỉ định**, kèm lý do khác với cổng legal.
- **Cảnh báo re-pin**: điền giá trị thật sẽ làm gate đỏ `artifact provenance fingerprint drift` một
  lần. *"Platform không làm hỏng gì cả"* — IVR re-pin. Không viết ra thì bên kia sẽ tưởng mình sai.
- Bảng §D thêm một dòng: `INF-A2` đủ nhưng thiếu `INF-A4` ⇒ gate **từ chối**, không ghi nửa vời.

### Phiếu Legal

- §6 thêm bảng **đúng năm trường** gate kiểm, kèm `decided_on` phải đúng dạng `YYYY-MM-DD`.
- Ghi rõ: **chữ ký của Owner module IVR bị từ chối bằng code**, dẫn `2a4f45d` và mutation
  `legal-authority`. Tức phiếu này **không thể đóng từ bên trong dự án** — và đó là chủ ý.

## 4. Chỗ rẽ chưa ai chốt — phần quan trọng nhất lượt này

Cả hai phiếu soạn `28/08`. Ngày `2026-09-05`, `OD-V1-19` được ký:

> *"Không vendor TTS lúc chạy; thu giọng người thật, ghép chữ số, bỏ tên khách khỏi lời thoại"*

Và nó **đã vào code**. `TargetV1SpeechPolicy.cs`, template `v3-test-approved`:

```csharp
"Xin chào Quý khách. "                                    // ← không còn {{customer_display_name}}
+ "Quý khách có đơn hàng gồm {{items_spoken}}, "
+ "tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. "
```

Lý do ký (`od-v1-signoff §2.1`): chỉ tên khách là **vô hạn**; chữ số ghép được, vùng giao hữu hạn.

| Placeholder còn lại | Thu trước được? |
| --- | --- |
| `{{total_amount_display}}` | được — chữ số, cơ chế ghép có từ `W-0108` |
| `{{delivery_area_short}}` | được — tập hữu hạn phường/quận |
| `{{items_spoken}}` | **chưa ai trả lời** — tên hàng, hữu hạn theo catalog nhưng phải thu lại khi catalog đổi |

**Hệ quả:** nếu `items_spoken` thu trước được thì không cần TTS lúc chạy, và **`INF-A` — mirror 201
MiB weights — không còn cần**. Gửi Platform đi dựng mirror trước khi chốt chỗ rẽ này là có thể phí
công họ.

### Nhưng có một vế dễ bỏ sót, và nó đi ngược

**12 đoạn cố định hiện có là audio do VieNeu render**, Owner ký `28/08`. Nên `L3` — quyền thương
mại của đúng ba preset — **vẫn áp dụng cho chính những file đó**, kể cả khi model không còn chạy
lúc gọi. Trừ khi chúng được thu lại bằng giọng người.

> *"Có được chạy model không"* và *"có được dùng audio model đã tạo ra không"* là **hai câu khác
> nhau**. Bỏ model khỏi runtime trả lời câu thứ nhất, không trả lời câu thứ hai.

Chỗ rẽ thuộc Owner + Product. Đã ghi vào **cả hai phiếu** kèm bảng câu nào phụ thuộc, câu nào không
— để Legal trả lời được `L5`–`L7` ngay mà không phải chờ.

## 5. Chống tái diễn

`00-index.md` thêm bảng **phiếu đang hoạt động** với ba dòng có tên, và một câu:

> *Một phiếu `NOT_SENT` không phải phiếu chết: nó là việc chưa làm. Chỉ gỡ khi đã có trả lời hoặc
> đã được thay bằng phiếu khác có tên.*

Đó là thứ thiếu ngày `04/09`.

## 6. Kiểm chứng

```text
gate-status.mjs                  GATE_STATUS_PASS — 224 work items
contract-freeze-verifier.mjs     CONTRACT_FREEZE=PASS
docs-selftest.mjs                API_DOCS_SELFTEST_PASS
markdown link check              2 phiếu resolve; 00-index → 3 phiếu resolve
tts-provenance-gate --selftest   10 mutation PASS (không đổi)
```

`dotnet test` **không chạy lại lượt này, có chủ ý**: `git status` cho thấy không một file `.cs`,
`.mjs` hay `.py` nào đổi — chỉ markdown và `gate-status.yaml` sinh ra. Con số `900/900` gần nhất là
của `e5f46e3` trên đúng cây code này. Chạy lại 10 phút để in ra cùng con số là nghi thức, không
phải bằng chứng.

Không sửa code, không sửa test, không sửa `MODELS.lock`, không mở gate nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại

- **Gửi.** Cả hai vẫn `NOT_SENT`. Việc gửi thuộc owner/chief auditor, giống `3.4`.
- **Chốt chỗ rẽ `items_spoken`** trước khi Platform bắt tay vào `INF-A`.
- `today-03-tts-handoff-pack-2026-08-29.md` (110 dòng) cũng bị `8ed62e9` xoá. Chưa khôi phục lượt
  này — nó là pack routing, và hai phiếu nó route tới nay đã sống lại; khôi phục hay bỏ hẳn là một
  quyết định riêng, không phải mặc định.
