# W-0227 — Gói routing sống lại, và phiếu thứ ba nó trỏ tới

Ngày: 2026-09-08 · Baseline: `main@7e589f0` · Trạng thái: **TESTS_PASS**.

Owner: *"giữ `today-03`, khôi phục luôn đi"*.

## 1. Khôi phục một, hoá ra phải khôi phục hai

`today-03` route tới **ba** phiếu:

```text
questions-to-legal-od-voice-07.md               ← W-0226 đã khôi phục
questions-to-platform-w0122-infrastructure.md   ← W-0226 đã khôi phục
questions-to-security-w0122-cve-disposition.md  ← chưa
```

Khôi phục gói mà để một link gãy thì gói vô dụng. Nên `W-0227` khôi phục nốt phiếu Security —
**131 dòng**, và nội dung không tầm thường:

- bảng đủ **16 CVE** kèm package/version/Trivy status, nhóm thành `perl-base` (8), `ncurses` (4
  cùng một CVE trên 4 package), `gzip`/`libacl1`/`libsqlite3-0` (4);
- **đo reachability trong chính image**, không suy đoán: entrypoint `["python","-m","shim.server"]`,
  `0 hit` spawn trong `deploy/tts/shim/`, `serve.py` **không** nằm trong 5 module `vieneu` được
  load, `subprocess` không được import ở cả hai lượt;
- và tự nêu giới hạn của lập luận đó: *"chỉ chứng minh 16 CVE không phải bề mặt tấn công đầu tiên"*
  — shell và perl vẫn nằm sẵn trong image làm công cụ post-exploitation, và đó là lý do có `SEC-B`.

Viết lại từ đầu là không thể — số liệu scan gắn với image digest `sha256:63fe84e6…` ngày `28/08`.

## 2. Hai cách đối xử khác nhau, có lý do

| Tài liệu | Cách xử lý | Vì sao |
| --- | --- | --- |
| Phiếu Security | **sửa tại chỗ** | là **biểu mẫu chưa gửi**; cập nhật trước khi gửi là đúng việc |
| `today-03` | **giữ nguyên byte + phụ lục** | là **tài liệu đã ký** ngày `2026-08-29`, có §5 "Xác nhận phía Module 8" |

Sửa thân một tài liệu đã ký thì mất provenance — đúng thứ `W-0130` phải dựng nhánh riêng để cứu.
Nên thân gói khôi phục nguyên vẹn và kiểm được:

```text
git show 8ed62e9^:…/today-03-…md | git hash-object --stdin
  → 2f6c951fb1750c705001db9c831baacec7265fd2
head -110 plan/ivr-orther/today-03-…md | git hash-object --stdin
  → 2f6c951fb1750c705001db9c831baacec7265fd2      ← khớp
```

Mọi cập nhật nằm ở phụ lục `P.1`–`P.5` sau dấu phân cách.

## 3. Chỗ rẽ chạm cả ba phiếu, mỗi phiếu một kiểu

`OD-V1-19` (`05/09`) đổi hướng sang *"không vendor TTS lúc chạy"*, và `items_spoken` chưa ai trả
lời có thu trước được không. Hệ quả **không giống nhau**:

| Phiếu | Nếu bỏ TTS lúc chạy |
| --- | --- |
| Legal | `L1`–`L4` phụ thuộc; **`L3` vẫn cần** — 12 đoạn cố định hiện có là audio do VieNeu render; `L5`–`L7` cần dù đường nào |
| **Security** | 16 finding **đều** thuộc base image Debian 13.6 của `ivr-tts`. Image không lên production ⇒ **không còn là câu hỏi release**, không cần `SEC-A` lẫn `SEC-B` |
| Platform | `INF-A` (mirror 201 MiB) không còn cần; `INF-B` và `INF-C` vẫn cần |

Với Security thì hệ quả **mạnh nhất**, nên phiếu ghi thẳng:

> *"Đừng ký `SEC-A` trước khi chốt chỗ rẽ. Một disposition có thời hạn review cho một image không
> bao giờ deploy là nợ giấy tờ, và nó sẽ được đọc như bằng chứng rằng image ấy đã được duyệt."*

Bảng `§6` thêm một dòng: đóng phiếu vì **image không lên production** và đóng vì **rủi ro được chấp
nhận** là hai cách đóng khác nhau, không thay nhau được.

## 4. Ba đính chính trong phụ lục `today-03`

- **`P.3`** — §2 của gói ghi *"phải trả lại đủ `L1`–`L6`"*, nhưng phiếu Legal có **bảy** câu
  (`L7`: ElevenLabs free tier ở lab trong lúc chờ). Dùng con số trong chính phiếu.
- **`P.4`** — `internal_mirror_gate` ngày `29/08` chưa có dòng riêng trong bảng §1; nay là gate
  thật sau `W-0225`, và phiếu Platform đã thêm `INF-A4` cho phần hồ sơ.
- **`P.1`** — ghi lại việc gói và cả ba phiếu bị `8ed62e9` gỡ, dù lượt dọn đó tự mô tả là chỉ gỡ
  phiếu `SUPERSEDED` thuộc vòng khác.

## 5. Kiểm chứng

```text
gate-status.mjs                  GATE_STATUS_PASS — 225 work items
contract-freeze-verifier.mjs     CONTRACT_FREEZE=PASS
docs-selftest.mjs                API_DOCS_SELFTEST_PASS
today-03 thân gói                git hash-object khớp 2f6c951… (nguyên byte)
link check                       6/6 resolve từ 00-index, worklist 1.4, W-0122 evidence, today-03
```

`dotnet test` không chạy lại: không file `.cs`/`.mjs`/`.py` nào đổi, `900/900` của `e5f46e3` vẫn
đúng với cây code này.

Không sửa code, không sửa `MODELS.lock`, không mở gate nào. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

Bốn tài liệu nay đủ bộ và liên kết đúng. **Không cái nào đã gửi** — `External dispatch:
NOT_PERFORMED` vẫn là trạng thái tự khai của gói, y như ngày `29/08`.

Việc kế tiếp không phải của M8: **chốt `items_spoken`**, rồi gửi.
