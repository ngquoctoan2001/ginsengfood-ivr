# Báo cáo tiến độ — IVR Order Confirmation

**Module:** GinsengFood Module 8 — IVR Order Confirmation
**Kỳ báo cáo:** 12/08/2026 → 15/08/2026
**Nhánh:** `main` · **Commit cuối:** `33740d3` · **Working tree:** sạch
**Chế độ hiện tại:** `MOCK` · `SALES_PROVIDER=FAKE_TARGET_V1` · `SIM_PROVIDER=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO`

> Nguồn số liệu: lịch sử git (53 commit, `aa0f2ff` → `33740d3`) và sổ tiến độ duy nhất
> `prompt/_execution/prompt-execution-tracker.md` (Work ID cuối `W-0102`, activity cuối `A-0190`).
> Báo cáo này **không phải** tracker thứ hai — nó chỉ đọc và tổng hợp lại.

---

## 1. Tóm tắt điều hành

Trong 4 ngày, dự án đi từ kho tài liệu nghiệp vụ thô đến một service .NET 10 chạy được
với admin console Next.js: **22 trong 54 hạng mục theo kế hoạch đã xong** — trọn vẹn
Phase 0 đến Phase 3.

| Chỉ số | Giá trị |
| --- | --- |
| Commit | 53 |
| Hạng mục hoàn thành | 22 / 54 |
| Test xanh | 483 (302 .NET + 181 admin-ui) |
| Coverage .NET | 88,80% |
| Bộ bằng chứng | 41 thư mục `docs/evidence/` |
| Gate ngoài còn mở | 11 / 11 |

Ba con số cần đọc cùng nhau:

- **22/54** là phần việc kỹ thuật IVR tự làm được.
- **11 gate** là phần phụ thuộc đội khác — hợp đồng API Sales, nhà mạng, pháp lý, hạ tầng —
  và **không đội nào trong số đó đã bàn giao**.
- Nghĩa là: code chạy được sau lớp mock, nhưng **chưa có đường nào dẫn tới cuộc gọi thật**.

---

## 2. Nhật ký theo ngày

### 2.1 · Thứ tư 12/08/2026 — 11 commit, ~712k dòng, 08:08 → 21:31

**Dựng nền: từ tài liệu đến 6 tầng hạ tầng chạy được.**
Buổi sáng là nạp và chuẩn hoá tài liệu; từ trưa trở đi là code, mỗi 1–1,5 giờ đóng một hạng mục.

| Mục | Nội dung |
| --- | --- |
| `docs` | Nạp toàn bộ tài liệu nghiệp vụ gốc (386 file): master pack, tech pack, phase 1–7. Đây là nguồn truy nguyên, không sửa. |
| `W-0001` | **Realign theo Target Contract V1.** Viết lại plan/specs/prompt cho khớp câu trả lời của đội Sales: 2 file OpenAPI, register 51 prompt, seed fake Sales, decisions-log. |
| `W-0062` | **Red-team tài liệu.** Phát hiện governance §3/§4/§6 đã bị xoá ở một commit trước, kéo theo 11 trích dẫn trong 7 prompt trỏ vào nội dung không còn tồn tại — DoD của P0-1 không kiểm chứng được. Khôi phục, gắn Work ID cho 32 prompt, gỡ hard-code attempt policy khỏi thiết kế DB, thêm lease/fencing, sinh 3 prompt mới (P1-5, P2-8, P2-9) và 9 quyết định mở `OD-V1-13..21`. |
| `P0-1` | Bootstrap solution .NET 10 — 5 project (Api, Worker, Infrastructure, Domain, Contracts) + tests + admin-ui Next.js + Docker Compose. Build 0 warning, 3/3 test, 3 health probe xanh. **Một trong hai hạng mục duy nhất được owner chấp nhận chính thức.** |
| `P0-2` | GitLab CI với 6 quality gate và 8 negative self-test — mỗi gate phải chứng minh nó biết đỏ, không chỉ biết xanh. |
| `P0-3` | Tầng cross-cutting: correlation, error envelope chuẩn, RBAC, allowlist service của Order Core, idempotency, audit chỉ-ghi-thêm, evidence store, PII guard. |
| `P0-4` | Feature flag có kiểu + kill switch **bất đối xứng**: bật luôn được ở mọi môi trường (chiều giảm rủi ro), tắt cần four-eyes. Đọc không được trạng thái thì coi như đang bật. |
| `P1-1` | Sinh code từ OpenAPI, pin phiên bản, gate chống trôi hợp đồng. Giữ client Target V1 và client Golden Hour hiện hành tách hẳn nhau. |
| `P1-2` | PostgreSQL thật: 17 bảng, 94 index, 6 trigger, migration Up/Down, outbox, lease/fencing cho kênh SIM, test bằng Testcontainers. |

> **Lỗi tự tìm ra và tự sửa.** Chạy full-suite lặp lại làm lộ một lỗi ngẫu nhiên: bộ sinh GUID
> cho correlation id thỉnh thoảng tạo ra chuỗi trông như dãy số, bị chính PII guard chặn và
> trả HTTP 500. Sửa generator rồi thêm 1.000 case regression.

---

### 2.2 · Thứ năm 13/08/2026 — 27 commit, ~34k dòng, 08:42 → 22:49

**Ngày dài nhất: đưa CI lên hosted thật và mở lõi runtime.**
Hai luồng chạy song song — một luồng vật lộn với GitLab thật, một luồng viết lõi nghiệp vụ.

#### Luồng hạ tầng — `W-0061`

| Giờ | Nội dung |
| --- | --- |
| 08:42 | Pipeline hosted đầu tiên bị GitLab từ chối **trước khi sinh job**: khoá cache khai 3 file, giới hạn là 2. Sửa, và thêm self-test chặn mọi cấu hình cache key rỗng hoặc quá dài. |
| 10:36 | Runner Linux làm lộ lỗi thật của test kiến trúc: `Path.GetFullPath` trên Linux không coi dấu `\` trong MSBuild là dấu phân cách, nên tên project còn nguyên `..\`. Chuẩn hoá cả hai kiểu separator. |
| 11:18 | Dựng runner Docker tự quản riêng cho IVR. Pipeline xanh toàn bộ: 9/9 job, 98/98 test, coverage 91,5%. |
| 14:10 | Bảo vệ nhánh `main`, bật *Pipelines must succeed*, chứng minh bằng MR thật, Registry và Pages riêng tư (khách ẩn danh bị đẩy về trang đăng nhập). |
| 15:11 | Gitleaks báo lộ khoá trong một commit lịch sử — thực chất là đoạn văn mô tả trong tài liệu. Xử lý bằng cách bỏ qua **đúng một fingerprint** chứ không nới luật; sau đó chính file bằng chứng mô tả sự việc lại tạo ra false positive mới, phải sửa cách diễn đạt. |

#### Luồng sản phẩm

| Mục | Nội dung |
| --- | --- |
| `P1-3` | Domain bất biến, provider port + fake tất định, mapper chống ăn mòn giữa Target V1 và hợp đồng hiện hành, privacy guard. |
| `P1-4` | Portal tài liệu API tĩnh 11 artifact + hướng dẫn versioning/tích hợp + gate oasdiff phát hiện breaking change. **Hạng mục thứ hai được owner chấp nhận chính thức.** |
| 6 remediation | Scanner lỗ hổng fail-closed khi JSON hỏng · scanner coverage quét mọi artifact text thay vì danh sách đuôi file · catalog 16 mã lỗi chuẩn · error envelope chạy trước auth · ma trận tham chiếu project chính xác · bắn thẳng UPDATE/DELETE vào PostgreSQL để chứng minh trigger audit chỉ-ghi-thêm thật sự chặn. |
| `P1-5` | Vòng đời dữ liệu: 9 lớp dữ liệu, mặc định chạy khô, legal hold thắng retention, xoá con trước cha bằng `SKIP LOCKED`, có checkpoint để chạy tiếp. |
| `P2-7` | Vòng đời kịch bản thoại có phiên bản, duyệt riêng theo từng chế độ, renderer tiếng Việt theo whitelist biến an toàn. |
| `P2-1` | Tiếp nhận task từ Sales: validate theo thứ tự, idempotency chuẩn hoá, ghi task/job/outbox/audit trong một transaction. 8 request đồng thời → đúng `1/1/1/1/1` bản ghi. |
| `P2-2` | Điều kiện gọi và chặn: fail-closed với sellable, hạn chế thoại, thông tin liên hệ, khung giờ, capacity. Cờ bỏ qua kiểm tra bị khoá cứng ở off. |
| `P2-3` | Registry policy có phiên bản + scheduler theo deadline + thuê kênh SIM có fencing, quarantine và phục hồi. |
| `P2-4` | Adapter tổng đài mock trung lập với nhà cung cấp, kho dial-token dùng một lần có hạn và allowlist, dispatch có fencing. |

> **Ràng buộc phát sinh.** Sau khi `main` được bảo vệ, GitLab từ chối mọi push trực tiếp.
> Theo chỉ đạo của owner là không tạo branch/MR, các commit từ P2-7 đến P2-6 chỉ lên được
> GitHub; GitLab đứng lại ở một commit cũ cho tới ngày 14/08.

---

### 2.3 · Thứ sáu 14/08/2026 — 13 commit, ~15k dòng, 07:56 → 17:53

**Đóng lõi runtime, rồi dành nửa ngày đi sửa chính mình.**

| Mục | Nội dung |
| --- | --- |
| `P2-5` | Chuẩn hoá DTMF và kết quả cuộc gọi qua **một** bộ mapping duy nhất. Tách bạch "tính là lần gọi khách" với "lỗi kỹ thuật" — lỗi kỹ thuật không được tiêu tốn lượt gọi. |
| `P2-6` | Gửi kết quả về Sales: snapshot bất biến, outbox, retry có giới hạn + jitter + circuit breaker. Adapter Golden Hour hiện hành được cô lập hoàn toàn khỏi đường Target V1. |
| `P2-8` | 6 endpoint vòng đời nội bộ (chỉ service token) + 7 endpoint admin ánh xạ 1-1 với permission, response che PII. **Lần đầu push thành công lên cả GitHub lẫn GitLab.** |
| `P2-9` | Ranh giới TTS: port + model, fake tất định, khung cho nhà cung cấp ngoài ở trạng thái fail-closed, privacy guard chạy **sau** khi render, cache theo deadline. |
| 8 remediation | **Remediation Phase 1+2 — 120 file.** Tám nhóm lỗi thật được xác minh từ source rồi sửa. |

> **Những gì đợt rà soát tìm ra:**
>
> - **Mất tính liên tục dữ liệu.** API ở chế độ MOCK dùng bộ nhớ tạm trong khi scheduler
>   luôn đọc PostgreSQL — hai bên nhìn hai thế giới khác nhau. Và không có đường nào trong
>   source tạo được kênh SIM.
> - **Bốn trạng thái không có lối thoát.** Job vào là kẹt: quarantine hết hạn không tự phục
>   hồi, HELD không đóng theo deadline, khoá global đặt sai phạm vi.
> - **Hợp đồng đã trôi mà changelog nói không.** Chạy lại oasdiff bằng đúng image CI cho ra
>   **143 thay đổi (63 lỗi, 80 cảnh báo)** trong khi file changelog đã commit ghi "no changes".
> - **Test tự khẳng định.** Một số test chỉ đọc lại fixture rồi báo PASS, hoặc bỏ thiếu vế
>   bắt buộc. Bị bác và viết lại bằng chứng minh thật.
> - **Đính chính số liệu.** Coverage được sửa lại còn **88,80%** sau khi loại code sinh tự
>   động và migration khỏi mẫu số — con số cũ đã tính rộng hơn thực tế.

---

### 2.4 · Thứ bảy 15/08/2026 — 2 commit, ~26k dòng, 11:16 & 14:50

**Trọn Phase 3 trong một ngày — 12 hạng mục, 216 file.**
Hai commit gộp (đặt tên `save`) nhưng bên trong là 12 hạng mục tách bạch: 4 theo kế hoạch
và 8 phát sinh. Phần lớn hạng mục phát sinh là **API đọc còn thiếu** — mỗi màn hình dựng
lên lại lộ ra backend chưa có dữ liệu để trả.

| Mục | Nội dung |
| --- | --- |
| `P3-1` | Nền admin console theo kiến trúc BFF: trình duyệt chỉ nói chuyện với Next.js server, server là nơi duy nhất gọi `Ivr.Api`. Session, RBAC, i18n, error envelope, che PII. |
| `W-0095` | Bổ sung 3 endpoint đọc cho dashboard/call log/call detail — **P2-8 chỉ có đúng 1 operation đọc**, còn call log thì không có API nào. |
| `P3-2` | Ba màn: dashboard, nhật ký cuộc gọi, chi tiết cuộc gọi. Số điện thoại luôn che, mã đơn rút gọn, lỗi callback 422 hiển thị đúng như nó là. |
| `W-0096` | 3 endpoint đọc back-office. Phát hiện lỗi 500: **một template đã lưu không còn hợp lệ làm sập cả danh mục** vì hàm validate ném exception — chuyển sang gắn cờ từng dòng. |
| `P3-3` | 5 màn cấu hình/tích hợp/review/seed/vai trò, toàn bộ chỉ đọc. Trạng thái phụ thuộc để `NOT_WIRED` chứ không tô xanh, vì chưa có cơ chế thăm dò thật. |
| `W-0097` | Đợt thiết kế lại giao diện theo hướng Minimalism/Swiss, bảng màu slate. Gom 6 bản CSS bảng và 5 bản CSS control thành 2 module dùng chung. Test contrast bắt lỗi thật ngay lần chạy đầu (`--ivr-border-strong` chỉ đạt 1,48:1 trong khi viền control cần 3:1 theo WCAG 1.4.11). |
| `W-0098` | 4 endpoint phân tích. k-anonymity là hằng số phía server (`min_bucket_size=5`), nhóm dưới ngưỡng bị **bỏ hẳn** chứ không đưa về 0 — vì dòng 0 sẽ bị đọc thành "không có cuộc gọi nào", một mệnh đề khác và sai. |
| `P3-4` | Màn báo cáo: KPI card, biểu đồ xu hướng không dùng thư viện chart (thanh CSS ẩn khỏi screen reader + bảng số song song), export CSV bắt buộc khai lý do và ghi audit. |
| `W-0099` | Rà soát phát hiện spec khai 2 hành động *Bật/Tắt kênh SIM* mà **không màn nào** có control — quyền đã cấp cho Ops nhưng vô dụng. Thêm endpoint và bảng kênh trên dashboard. |
| `W-0100` | Guard chống trôi hợp đồng mang tên "kiểm mọi path UI chạm tới" nhưng chỉ kiểm 3 trong 12. Viết lại để **suy path thẳng từ source**, nên tự phủ hàm mới. |
| `W-0101` | Rà vòng hai, lần này đọc thẳng `specs/ui` thay vì prompt: màn đang dựng theo *những gì API tình cờ trả về* chứ không theo danh mục spec. Bù 5 field, không thêm operation nào. |
| `W-0102` | Chụp bằng chứng từ stack thật (PostgreSQL + API + UI). Fixture cố ý để một nhóm chỉ 3 dòng — **dưới ngưỡng k=5** — nên cơ chế ẩn được *chứng minh* chứ không phải chỉ khẳng định. |

> **Ba lỗi chỉ lộ ra khi chạy thật:**
>
> - **Đăng nhập hỏng dù test xanh.** `NextResponse.redirect(new URL(p, request.url))` sinh
>   Location tuyệt đối dựng từ header Host, đẩy người dùng từ `127.0.0.1` sang `localhost` —
>   khác origin, nên cookie `SameSite=Strict` vừa cấp bị bỏ lại. Lỗi này cũng sẽ xảy ra sau
>   mọi reverse proxy ghi lại Host.
> - **Guard môi trường khoá nhầm.** Chặn theo `NODE_ENV=production` — đúng với mọi lần
>   `next start`, nên sẽ khoá cả staging lẫn lab. Đổi sang allowlist môi trường.
> - **Cổng 5005 đang có người giữ.** Màn báo cáo báo lỗi nội bộ; nguyên nhân không phải code
>   mà là một `Ivr.Api` bản cũ của owner đang chiếm cổng. Không kill process của owner —
>   dựng instance riêng ở 5015 để chụp bằng chứng.

---

## 3. Tiến độ tổng thể

Kế hoạch chia làm 12 phase, tổng 54 hạng mục. Phase 0–3 đã đóng; từ Phase 4 trở đi là vùng
phụ thuộc đội khác.

| Phase | Nội dung | Trạng thái |
| --- | --- | --- |
| **P0** | Nền tảng | ✅ 4/4 xong |
| **P1** | Hợp đồng & dữ liệu | ✅ 5/5 xong |
| **P2** | Lõi runtime | ✅ 9/9 xong |
| **P3** | Admin UI | ✅ 4/4 xong |
| — | *ranh giới hiện tại* | — |
| **P4** | Tích hợp thật | 🔒 0/6 chặn ngoài |
| **P5** | Chất lượng | ⬜ 0/5 chưa bắt đầu |
| **P6** | Quan trắc | ⬜ 0/3 chưa bắt đầu |
| **P7** | Triển khai | ⬜ 0/5 chưa bắt đầu |
| **P8** | SIM thật | 🔒 0/2 chặn ngoài |
| **P9** | Phát hành | 🔒 0/2 chặn ngoài |
| **P10** | Tuân thủ | ⬜ 0/5 chưa bắt đầu |
| **P11** | Chốt sản xuất | ⬜ 0/4 chưa bắt đầu |

### 3.1 Đã dựng được những gì

| Thành phần | Nội dung | Quy mô |
| --- | --- | --- |
| `Ivr.Api` | Health probe, intake Target V1, 6 endpoint nội bộ, 7 endpoint admin, 10 endpoint đọc/phân tích | 171 file `.cs` |
| `Ivr.Worker` | Scheduler theo deadline, normalizer kết quả, dispatcher callback, job retention | 4 nhóm job |
| `Ivr.Infrastructure` | Persistence, feature flag, telephony, speech/TTS, callback, retention, audit, evidence | 17 vùng |
| `Ivr.Domain` | Model bất biến, catalog lỗi, privacy guard, kịch bản thoại, retention | — |
| PostgreSQL | Bảng, index, trigger append-only; retention phủ 18 bảng | 17 bảng / 94 index / 6 trigger |
| `admin-ui` | Next.js App Router strict TypeScript, kiến trúc BFF | 16 route |
| Hợp đồng | OpenAPI IVR `v1.0.0-draft.7` + hợp đồng callback Sales (còn draft) | 2 file |
| CI | 7 job trên GitLab runner tự quản, kèm negative self-test cho từng gate | 7 job |

### 3.2 Kiểm thử và bằng chứng

| Chỉ số | Giá trị | Ghi chú |
| --- | --- | --- |
| Test .NET | 302 / 302 | Unit, contract, integration (Testcontainers PostgreSQL thật) |
| Test admin-ui | 181 / 181 | Unit, component, E2E |
| Coverage .NET | 88,80% | Đã loại code sinh tự động và migration khỏi mẫu số |
| Build | 0 warning / 0 error | Warning-as-error đang bật |
| Bộ bằng chứng | 41 | Mỗi Work ID một thư mục dưới `docs/evidence/` |
| Ghi nhận hoạt động | 190 | Sổ chỉ ghi thêm, không xoá lịch sử |

### 3.3 ⚠️ Trạng thái nghiệm thu — điểm cần lưu ý

Trong 22 hạng mục đã xong, chỉ **2 được owner chấp nhận chính thức** (`P0-1` và `P1-4`).
20 hạng mục còn lại dừng ở mức `TESTS_PASS` — code xong, test xanh, bằng chứng đã nộp,
nhưng **chưa có reviewer độc lập ký nhận**.

Đây là hệ quả trực tiếp của `W-0061`: GitLab đang ở gói Free với đúng một thành viên, nên
không thể bật luật *bắt buộc có người duyệt độc lập*.

---

## 4. Phần chưa làm — 32 hạng mục

32 hạng mục còn lại chia làm hai loại rất khác nhau — và đây là chỗ quyết định kế hoạch
tiếp theo.

### 4.1 Loại 1 — IVR tự làm được ngay (17 hạng mục)

Không phụ thuộc đội nào khác. Đây là phần nên đưa vào sprint kế tiếp.

| Mã | Nội dung | Trạng thái |
| --- | --- | --- |
| `P5-1`…`P5-5` | Bộ test unit/integration đầy đủ, contract & E2E, hiệu năng/bảo mật, gate code review, QA khả năng tiếp cận & đa ngôn ngữ | chưa bắt đầu |
| `P6-1`…`P6-3` | Log/metric/tracing đã che PII, dashboard & SLO & cảnh báo, diễn tập chaos | chưa bắt đầu |
| `P7-1`…`P7-5` | Docker image & Compose, Helm/Kubernetes, CI/CD promotion, canary/rollback, xoay vòng secret | chưa bắt đầu |
| `P10-1`…`P10-4` | PDPA/privacy, quản trị dữ liệu & backup/DR, mô hình capacity & chi phí, pipeline phân tích | chưa bắt đầu |
| `P4-2`, `P4-3` | Xác thực hợp đồng blocker phía Sales, nối nguồn hạn chế thoại từ CRM | chưa bắt đầu |

### 4.2 Loại 2 — Chờ đội khác bàn giao (15 hạng mục)

Code có thể chuẩn bị trước sau lớp mock, nhưng **không thể đóng** nếu không có artifact thật
từ bên ngoài.

| Mã | Nội dung | Cần ai giao gì |
| --- | --- | --- |
| `P4-1` | Nối Sales provider thật + contract test | Sales: task producer đủ 2 program, callback + ACK, `order_version`, OpenAPI/sandbox |
| `P4-4` | Auth service-to-service ở production | Security/Platform: issuer/audience/scope/TTL/JWKS, quyết định mTLS |
| `P4-5`, `P4-6` | Ranh giới notification, vòng phản hồi opt-out | **Hoãn có chủ đích** — V1 không gửi tin nhắn nào |
| `P8-1`, `P8-2` | Adapter nhà cung cấp thật + chạy lab 1 SIM | Infra/vendor: protocol/SDK, SIM test, số đích được duyệt |
| `P9-1`, `P9-2` | Thực thi release gate, cutover & hypercare | Release owner: sign-off sau khi mọi gate đóng |
| `P10-5` | SLA / error budget / on-call | Chờ runbook vận hành production của `P9-2` |
| `P11-1`…`P11-4` | RFQ tổng đài, gói chốt hợp đồng, gói pháp lý/retention, bảng điều khiển sẵn sàng | Chạy song song được ngay — **không phụ thuộc code** |

### 4.3 11 gate ngoài đang mở

| Gate | Chủ sở hữu | Đường mock đang dùng | Cần gì để đóng |
| --- | --- | --- | --- |
| Hợp đồng task/callback | Sales API/Core | fake Sales + WireMock | OpenAPI được duyệt + contract test |
| Nội dung đọc đơn an toàn | Sales/Product/Privacy | fixture tóm tắt giả | Schema, ví dụ, phê duyệt privacy |
| Dial token | Sales/Security/Telephony | resolver giả | Threat model + API + test |
| Auth production | Security/Platform | JWT mock | Auth profile + credential sandbox |
| Chính sách số lần gọi | Product/Core | bản ứng viên chỉ dùng ở MOCK/LAB | Policy có chữ ký + phiên bản |
| Lab 1 SIM thật | Infra/vendor | SIM mock | Báo cáo lab + allowlist + kill switch |
| Năng lực 32 eSIM | Infra/mua sắm | bộ mô phỏng tải | Mua sắm + đo năng lực thật + failover |
| Pháp lý & retention | Legal/Privacy | tắt ghi âm, che dữ liệu | Bản review có chữ ký |
| Phát hành production | Release owner | *không có* | Go/no-go được chấp nhận |
| Nền tảng GitLab | Platform/Infra | mọi control khác đã xanh | Nâng Premium + mời reviewer thứ hai |
| Hạ tầng nền | Platform/Infra | docker-compose local | 8 hạng mục: registry, K8s, secret store, observability, warehouse… |

### 4.4 Nợ kỹ thuật và quyết định đang treo

- **Thư viện component cho admin UI** vẫn chưa chọn — hiện toàn bộ là CSS module tự viết.
- **Công thức `call_success_rate`** đang tính "huỷ đơn vẫn là gọi thành công". Công thức đã
  ghi thẳng vào mô tả hợp đồng kèm cờ *chờ owner xác nhận*, vì spec chỉ đặt tên tile chứ
  không định nghĩa.
- **`cost_per_confirmed_order`** chưa hiển thị được vì chưa có mô hình chi phí (thuộc `P10-3`).
- **Endpoint phân tích tự khai `warehouse_backed=false`** trên mọi response — đây là lớp phục
  vụ tạm, pipeline thật (`P10-4`) sẽ thay vào.
- **Bằng chứng màn hình đang ở dạng text**, chưa phải ảnh PNG. Chụp ảnh cần thêm Playwright
  vào repo — owner đã chọn phương án không thêm dependency.
- **Trạng thái fail-closed trên UI mới chỉ gắn nhãn, chưa thăm dò thật** — cơ chế thăm dò
  phụ thuộc `P6-1`.
- **Hai quyết định mở**: quyền duyệt kịch bản thoại (`OD-V1-15`) và permission cho runtime
  gate (`OD-V1-20`, hiện chưa cấp cho vai trò nào, fail-closed).

---

## 5. Đề xuất thứ tự tiếp theo

Ưu tiên phần không bị chặn, đồng thời đẩy sớm các yêu cầu ra bên ngoài vì chúng có thời gian
chờ dài nhất.

**1 · Gửi ngay gói yêu cầu ra ngoài — `P11-1` và `P11-2`**
RFQ tổng đài và gói chốt hợp đồng Sales/auth không phụ thuộc dòng code nào, nhưng chúng chặn
Phase 4, 8, 9.
*Lý do ưu tiên:* mua sắm SIM và ký hợp đồng API là hai việc lâu nhất. Bắt đầu sau sẽ thành
đường găng của cả dự án.

**2 · Phase 5 — chất lượng**
`P5-1` đến `P5-4` củng cố bộ test hiện có; `P5-5` xử lý nốt QA khả năng tiếp cận và đa ngôn
ngữ đang nợ từ Phase 3.
*Lý do ưu tiên:* 483 test hiện tại là do từng hạng mục tự sinh ra, chưa qua một chiến lược
test thống nhất.

**3 · Phase 6 — quan trắc**
`P6-1` mở khoá cả cơ chế thăm dò phụ thuộc thật cho UI (đang nợ từ `P3-3`) lẫn dữ liệu đo cho
mô hình capacity.
*Lý do ưu tiên:* đang là nút thắt của 4 hạng mục khác.

**4 · Phase 7 — đóng gói & triển khai**
`P7-1` làm được ngay với Compose. `P7-2` trở đi cần cluster và secret store — thuộc 8 hạng
mục hạ tầng đang chờ Platform.
*Khuyến nghị:* nên tách — làm phần Docker trước, hoãn phần K8s tới khi hạ tầng sẵn sàng.

**5 · Gỡ nút nghiệm thu — `W-0061`**
Nâng GitLab lên Premium và mời một reviewer độc lập để 20 hạng mục `TESTS_PASS` có đường
chuyển thành `ACCEPTED`.
*Lý do ưu tiên:* không gỡ thì mọi thứ đã làm vẫn ở trạng thái "chưa ai ngoài người viết ký nhận".

---

*Báo cáo lập ngày 15/08/2026. Bản HTML tương ứng: `docs/reports/2026-08-15-bao-cao-tien-do-ivr.html`.*
