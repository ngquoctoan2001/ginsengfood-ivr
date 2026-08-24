# Quyết định đã chốt

Ngày: `2026-08-19` · **Cả ba đã được chủ sở hữu quyết và đã thi hành.** Giữ lại nguyên văn phần
phân tích để lần sau ai đọc còn thấy đánh đổi, chứ không chỉ thấy kết luận.

| | Quyết định | Đã làm |
| --- | --- | --- |
| `OD-OPEN-01` | **Sửa prompt** — spec là nguồn sự thật | `P6-3` §3/§9 trỏ tới mục **4/8/10** có thật, kèm ghi chú vì sao sửa |
| `OD-OPEN-02` | **Cấm id đệm số 0**, pattern giữ nguyên | id đệm số 0 mười chữ số **không tồn tại** trong app/seed/fixture nên áp dụng được ngay; luật ghi ngay cạnh pattern trong `PiiGuard` |
| `OD-OPEN-03` | **Chọn `lab`** | `values-pilot.yaml` → `values-lab.yaml`; `promote_pilot` → `promote_lab`; guard giờ báo `Only lab and prod` |

---

## Phân tích gốc (giữ nguyên)

Đây là những mục tôi **không tự quyết** vì chúng đổi spec hoặc đổi chính sách privacy. Mỗi mục có
đúng ba thứ: chuyện gì, các lựa chọn, và tôi đề xuất cái nào.

---

## OD-OPEN-01 — `P6-3` trỏ tới dải test ID **không tồn tại**

**Chuyện gì.** `P6-3` §3 và §9 yêu cầu scenario phủ "fail-closed profiles `IT-12..17`" trong
`specs/testing/03-integration-test-plan.md`. Dải ID đó **không tồn tại ở đâu** trong `specs/` hay
`plan/` — grep toàn repo trả về rỗng. File được trỏ tới có **10 mục đánh số**, không có ID nào.

**Tôi đã làm gì.** Map scenario vào mục **4** (lease/fencing + crash recovery), **8**
(dependency/auth/evidence outage fail closed), **10** (migration/retention/audit/outbox recovery) và
ma trận `ARCH-05` §1 — rồi ghi rõ trong `docs/evidence/W-0042` rằng tôi **không** tuyên bố phủ một
dải ID không có thật.

**Lựa chọn.**

| | Việc phải làm | Đánh đổi |
| --- | --- | --- |
| **A** | Sửa `P6-3` §3/§9 trỏ tới 10 mục đánh số có thật | prompt là thứ sai, spec là nguồn sự thật |
| **B** | Thêm ID `IT-12..17` vào spec, gán cho từng profile fail-closed | spec dài thêm; phải quyết ID nào ứng với profile nào |

**Đề xuất: A.** Spec là nguồn sự thật và prompt đang trỏ tới thứ chưa từng tồn tại. B chỉ hợp lý
nếu anh muốn có ID ổn định để trích dẫn về sau.

**Chặn gì.** Không chặn gì đang chạy. Nó chỉ khiến `docs/evidence/W-0042` §4 phải nói "map vào mục
số" thay vì "phủ profile IT-xx".

---

## OD-OPEN-02 — PII pattern có dương tính giả trên id đệm số 0

**Chuyện gì.** Nhánh số điện thoại của `PiiGuard` là `(?<![0-9A-Za-z])0[0-9]{9}(?![0-9A-Za-z])` —
tức **mười chữ số bắt đầu bằng 0**. Một id có tiền tố chữ, một dấu gạch nối, rồi mười chữ số
đệm 0 sẽ khớp: dấu `-` không phải ký tự chữ-số nên lookbehind qua được.

**Đã cắn thật, không phải giả thuyết.** Trong phiên làm việc này nó chặn nhầm **ba lần**: một chuỗi JSON mẫu trong lúc đo hiệu năng regex, và hai token giả trong script CI kết thúc bằng một dãy mười
chữ số bắt đầu từ 0. Mỗi lần tôi sửa **nội dung của mình**, chưa lần nào nới pattern.

Và chính tài liệu này bị chặn **hai lần nữa** trong lúc viết — một lần vì tôi trích nguyên văn dãy
số đó, một lần vì tôi viết một số di động làm ví dụ. Lần thứ hai là **dương tính đúng**: cổng làm
đúng việc của nó. Lần thứ nhất là **dương tính giả** — đúng thứ đang bàn. Cùng một pattern, hai kết
quả khác hẳn về bản chất, và đó là lý do quyết định này thuộc về anh chứ không phải tôi.

**Hệ quả thật.** `PiiMaskingFilter` chạy guard trên **toàn bộ response body**. Một response admin
chứa id dạng đó sẽ bị `PiiPolicyViolation` — tức **lỗi 4xx cho người dùng hợp lệ**.

**Lựa chọn.**

| | Việc phải làm | Đánh đổi |
| --- | --- | --- |
| **A** | Giữ nguyên | không giảm phát hiện; tiếp tục chặn nhầm id đệm số 0 |
| **B** | Thêm điều kiện loại trừ khi chuỗi số đứng ngay sau `<chữ>-` (ví dụ `TASK-`) | **giảm phát hiện**: một số di động thật viết ngay sau một tiền tố có gạch nối sẽ lọt |
| **C** | Cấm id đệm số 0 trong quy ước đặt tên, giữ nguyên pattern | không đụng chính sách privacy; phải rà và đổi id đang có |

**Đề xuất: C nếu id đệm số 0 không bắt buộc; A nếu có.** Tôi **không** đề xuất B: nó là thay đổi
**chính sách privacy** theo hướng thu hẹp phát hiện, và `P0` cấm tôi tự phê duyệt. B chỉ nên chọn
nếu anh chấp nhận rõ ràng rằng một số thật viết ngay sau một tiền tố có gạch nối sẽ không còn bị bắt.

**Chặn gì.** Không chặn slice nào. Nhưng nó là **rủi ro production**: một response admin hợp lệ có
thể bị từ chối.

---

## OD-OPEN-03 — Tên bậc ladder lệch giữa hai tài liệu

**Chuyện gì.** `README-governance` §6 gọi bậc thứ ba là **`lab`**; `P7-2`/`P7-3` và Helm chart gọi
nó là **`pilot`**. Cùng một bậc, hai tên.

**Tôi đã làm gì.** Giữ `pilot` cho khớp chart đã dựng, và ghi lệch này trong `docs/evidence/W-0045`.

**Đề xuất:** chốt **một** tên rồi sửa phía còn lại. `lab` mô tả đúng bản chất hơn (một SIM thật,
allowlist, kill switch) còn `pilot` nghe như đã có khách thật — mà bậc đó **chưa** cho gọi khách.

**Chặn gì.** Không chặn gì. Nhưng ladder là thứ được trích dẫn trong mọi quyết định governance, và
hai tên cho một bậc là cách hiểu nhầm bắt đầu.

---

## Không nằm trong danh sách này

| Mục | Trạng thái |
| --- | --- |
| `RETENTION_EXECUTION` | **đã đóng** — `W-0047` thêm entrypoint run-once |
| drill rotation "không request nào rớt" | **đã đóng** — `docs/evidence/W-0047` §9.5 |
| `NETPOL_ENFORCEMENT` | **đã đóng `2026-08-19`** — kết luận "cần CNI khác" là **sai**: cluster vẫn thực thi, phép đo bị đua. Xem `docs/evidence/W-0044` §5 |
| migration "code mới chịu được schema cũ" | **đã đóng `2026-08-24`** — `W-0114` thêm cổng `schema_compat_gate`. Kiểm cả hai chiều: binary mới trên schema cũ (`IT-SCHEMA-NEWCODE-01/02`) và migration mới dưới code cũ (`UT-SCHEMA-BACKCOMPAT-01`). Xem `docs/evidence/W-0114/README.md` |
