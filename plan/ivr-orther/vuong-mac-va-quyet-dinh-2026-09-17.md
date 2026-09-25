# Vướng mắc và việc cần quyết — Module 8 (gọi điện xác nhận đơn hàng)

**Lập:** `17/09/2026` · **Duyệt:** `17/09` — Sếp + Toàn · **Mốc mã:** `main@fe3bb19`
**Trạng thái:** `ĐÃ QUYẾT 12/16` — còn **4 mục chờ Sếp** (`S1` `S2` `S5` `S6`) · S8: owner tự chạy CLI theo trả lời ngày 21/09, xem bên dưới · **Kế hoạch khắc phục:** §5, cuối file

> **Hệ thống hiện chưa thể gọi khách thật** — cố ý, có nhiều lớp chặn. Không mục nào dưới đây là
> sự cố đang xảy ra. Tất cả là việc phải xong **trước** ngày gọi khách thật.

---

## 1. Đã quyết ngày `17/09`

| Mã | Quyết định | Hệ quả phải biết | Làm ở |
| --- | --- | --- | --- |
| `S3` | **Giữ dữ liệu vĩnh viễn** — không đặt thời hạn xoá, kể cả nhật ký quản trị | Thay các kỳ hạn đã ký `10/09` (90 ngày · 180 ngày · 1 năm). Đường xoá **duy nhất** còn lại là khi khách yêu cầu ⇒ `T2` thành bắt buộc. Số điện thoại dạng đọc được nằm vĩnh viễn ⇒ đã đưa vào danh sách ký nhận rủi ro `S2`. **Toàn xác nhận `17/09`: toàn bộ dữ liệu**, không riêng nhật ký quản trị. Tài liệu gửi Module 3 đã sửa (`W-0313`); register và phiếu thời hạn lưu sửa ở `W-0316` | Lô 3 · ✅ `W-0316` |
| `S4` | **Giữ VieNeu** chạy trên máy chủ công ty | Mở lại nhánh `W-0122`. Trước khi bật ở production cần: xử lý lỗ hổng (`44` mức cao sau `W-0317`, `S2` rủi ro 3) · quyền dùng model và 12 đoạn đã render · máy chạy thật + kho bản cài nội bộ · đo tốc độ · nghe duyệt · gọi thử. Cấu hình production **giữ tắt** tới khi đủ | Lô 4 |
| `S7` | **B** — ghi nhận là giới hạn của bản đầu, không mua kho ghi nhận ngoài | Ghi ở register, **không** sửa `m8-15` (bị ghim hash) | Lô 3 · ✅ `W-0316` (`OD-V1-24`) |
| `T1` | `IR-07` **đã gửi** Module 3, đang chờ phản hồi · tách stage 3 · nới token · thêm driver chỉ-gửi-số | Vế "giữ phiếu lại" không còn áp dụng — Module 3 **đã cầm** lời hứa "chỉ gửi số là đủ" ⇒ lô này đi **đầu tiên**. Kiểm lại lúc lập kế hoạch thấy lỗi **lớn hơn** đã báo: ở production, phương án B hiện **không nhận được task nào** (Lô 1) | Lô 1 |
| `T2` | Sửa đường xoá bỏ sót `phone_e164` · trigger · gate đối chiếu | Phải xong **trước** khi Module 3 gửi số thật | Lô 2 · ✅ `W-0314` |
| `T3` | Kiểm định dạng số lúc nhận | Cùng bản `draft.31` | Lô 1 |
| `T4` | Ghi ngay: phần *"không ghi DB"* của `OD-V1-18` đã bị thay | | Lô 3 · ✅ `W-0316` |
| `T5` | Sửa `perm IVR_DEV_TOOLING` thành quyền endpoint thực đòi (`ivr.admin.write`) | Cùng bản `draft.31` | Lô 1 |
| `T6` | Ghi thành điều kiện của stage 5, **chưa làm** | | Lô 3 · ✅ `W-0316` (README `W-0311` §6) |
| `T7` | Release owner là **Toàn**; nghiệm thu **theo đợt**, tiêu chí ghi sẵn | Chỉ Toàn chuyển trạng thái sang `ACCEPTED` | Lô 5 · ◐ `W-0318` (tiêu chí + danh sách; phần duyệt là của Toàn) |
| `T8` | `W-0163` → `CANCELLED` · `W-0118` → `N/A` | ⚠️ **Khác đề xuất cũ ở `W-0118`:** kiểm lại thấy tính năng "bỏ qua khách cũ" (`OD-15`) đã bị `OD-18` thay từ `draft.21` (`W-0123`) — hạ về `N/A` theo tiền lệ `W-0105`, không khôi phục bằng chứng | Lô 3 · ✅ `W-0316` |

---

## 2. Còn chờ Sếp quyết — 5 mục

### S1 · Cần thêm người: ba thao tác bắt buộc "người khác nhau" 👤 🔴

Dự án hiện chỉ có một người làm (Toàn), nên cả ba thao tác dưới đây đang kẹt:

| # | Thao tác | Cần | Nếu thiếu |
| --- | --- | --- | --- |
| 1 | **Duyệt lời thoại** trước khi gọi khách thật | **3 người khác nhau**: người soạn · người duyệt nội dung · người duyệt quyền riêng tư | **Không gọi khách thật được** — phần mềm từ chối, chữ ký trên giấy không thay được |
| 2 | **Mở lại cuộc gọi sau khi bấm dừng khẩn cấp**, hoặc thêm số được phép gọi | **2 người**: người đề nghị khác người duyệt *(bấm dừng thì 1 người là đủ)* | Sau một sự cố, chính hệ thống chặn việc mở lại |
| 3 | **Duyệt mã nguồn** trước khi vào bản chính | 2 người | GitLab miễn phí chỉ cho duyệt **tự nguyện**; muốn bắt buộc phải lên gói trả phí |

Ngày `17/09` Sếp đồng ý **một** người thứ hai (chưa tạo tài khoản, chưa nêu là ai) — nhưng thao tác 1 cần
**ba**. Đang đề xuất với Module 3 (`M3-18`): người được dừng / cắt cuộc gọi không nên đồng thời duyệt nội
dung lời thoại. Từ `S4`: nghe duyệt 12 đoạn giọng VieNeu cũng cần người được chỉ định.

| Việc | Cách 1 *(đề xuất)* | Cách 2 |
| --- | --- | --- |
| Soạn lời thoại | Toàn | Toàn |
| Duyệt nội dung lời thoại | **Sếp** | Tech lead Module 3 |
| Duyệt quyền riêng tư | Tech lead Module 3 | **Sếp** |
| Mở lại sau dừng khẩn cấp *(đề nghị → duyệt)* | Toàn → Tech lead | Toàn → **Sếp** |

**Cách 3:** ký quyết định mới hạ luật duyệt lời thoại xuống 2 người — mất một lớp kiểm chéo.
**Đề xuất:** Cách 1, và **chưa mua** GitLab Premium — xem lại khi có thêm người viết mã.

*Nguồn: `src/Ivr.Domain/Scripts/ScriptContentContracts.cs:237-255, 291-303` · register `OD-V1-11`,
`OD-V1-20`, `OD-V1-21` · `docs/evidence/W-0061/README.md:145` · `IR-07` câu `M3-18`.*

> **Sếp trả lời:** Cách ☐ 1 ☐ 2 ☐ 3 ☐ khác: ________ · GitLab: ☐ mua Premium ☐ duyệt tự nguyện

### S2 · Ký nhận rủi ro của bản đầu — một chữ ký cho cả cụm ⚖️ 🔴

Cập nhật `22/09` — **Nguyễn Quốc Toàn đã xác nhận thẩm quyền đại diện công ty và chấp thuận
đủ bảy điểm, giữ nguyên các điều kiện**. [Phiếu và nguyên văn xác nhận W-0340](../../docs/evidence/W-0340/S2-owner-review.md).
Phần nhận rủi ro đã chốt có điều kiện. **Bổ sung 22/09, [W-0341](../../docs/evidence/W-0341/README.md):**
đã kiểm công bố Apache-2.0 tại đúng hai revision; FAQ VieNeu cho phép preset và audio thương mại.
Nhận định trước rằng thiếu mọi chứng từ là quá rộng. **W-0342 đã tích hợp bằng chứng vào
provenance và hai verifier**, cùng mirror URI đã kiểm; còn quyết định phát hành riêng và các gate
production. Phiếu S2 đã ký giữ nguyên lịch sử, không cần ký lại bảy điểm.

Công ty không có bộ phận pháp chế hay bảo mật riêng. Cách đã dùng ngày `10/09`: người có thẩm quyền **ký
nhận rủi ro bằng văn bản**. Hồ sơ hiện ghi người nhận là **"owner"** — cần ghi rõ là Sếp.

| # | Rủi ro | Từ đâu |
| --- | --- | --- |
| 1 | Hệ thống gọi **giữ số điện thoại khách ở dạng đọc được** — ai lấy được bản sao dữ liệu là đọc được | Chọn `17/09`: Module 3 gửi thẳng số |
| 2 | 🆕 **Giữ vĩnh viễn** mọi dữ liệu khách, gồm số điện thoại ở rủi ro 1. Cách xoá duy nhất: khi khách yêu cầu — *cập nhật 21/09: W-0330 đã bổ sung CLI để owner tự chạy theo `S8`; câu “chưa có lệnh” trước đây đã hết hiệu lực*. *Nghị định `13/2023/NĐ-CP` có nguyên tắc chỉ lưu dữ liệu cá nhân trong thời gian phù hợp với mục đích xử lý — đây là điểm rủi ro pháp lý rõ nhất trong bảng. Ghi để Sếp cân nhắc, không phải ý kiến pháp lý* | `S3` |
| 3 | **Lỗ hổng và phụ thuộc nền chạy.** Đã chọn Chainguard và đo model thật. `W-0340`, `22/09`: quét đúng image VieNeu với DB mới, không phát hiện CVE; image worker đi kèm có **0 HIGH/CRITICAL**, còn **11 MEDIUM + 6 LOW** theo cặp package/CVE. Danh sách **44 HIGH** của Debian `W-0317` là lịch sử, không phải image đang chọn. Giữ phụ thuộc nhà cung cấp và nghĩa vụ theo dõi/cập nhật | `S4`; [W-0340](../../docs/evidence/W-0340/README.md) |
| 4 | 🆕 **Quyền dùng thương mại** model VieNeu và 12 đoạn giọng đã render | `S4` |
| 5 | Người **nhấc máy không phải chủ đơn** vẫn nghe được tên món hàng và phường/quận giao | Chốt `05/09` |
| 6 | Bản đầu **không có cách** để khách nói "đừng gọi tôi nữa" qua cuộc gọi — phím 0 là huỷ đơn | Chốt `10/09` |
| 7 | **Không ghi âm** — khi khách phủ nhận đã bấm xác nhận, bằng chứng chỉ là nhật ký phím bấm | Chốt `05/09` |

Rủi ro 1 có lối lùi: quay lại mã hoá số — phải mua dịch vụ giữ khoá, và Module 3 phải xây bộ mã hoá.

**Quyết định `22/09`:** đã nhận đủ bảy điểm theo phiếu W-0340, không cần xin xác nhận lại.
Điều kiện giữ nguyên: `T2` trước dữ liệu thật; quyền model/codec/giọng phải có chứng từ;
mirror và nghiệm thu triển khai còn phải kiểm. Chữ ký nhận rủi ro không tự cấp quyền model
hoặc bật production. Rủi ro CVE được xử lý bằng lựa chọn Chainguard và quét đúng image,
không diễn giải scan không phát hiện thành bảo đảm không còn mọi lỗ hổng.

> **Nguyễn Quốc Toàn, `22/09/2026`:** *"Tôi có thẩm quyền và chấp thuận đủ 7 điểm, giữ các điều kiện đã ghi".*

### S5 · Chỗ chạy thử trước khi chạy thật 💰 🟡

Cập nhật `22/09`: vps61 đã đo VieNeu thật **128/128 đơn đạt** với **2 CPU/4 GiB** và
profile **30/90/120 giây** (W-0338); mốc 5 giây/phần ban đầu bên dưới không phải cấu hình
đã chọn. Kho artifact nội bộ qua SSH/SFTP trên chính vps61 đã kiểm **39/39 file và khôi phục
39/39 file** ([W-0340](../../docs/evidence/W-0340/mirror-target-findings.md)). Hai phần này đã có
bằng chứng; S5 triển khai toàn hệ thống, registry OCI nếu cần và backup ngoài máy chưa đóng.

Bàn giao `17/09` ghi Sếp **đã trả lời** mục này, nhưng **nội dung chưa được ghi ở đâu**. Cần ghi lại.

- **Đang có:** máy chủ thử `192.168.1.61` chạy bản **giả lập**, đã chạy trọn 3 kịch bản ngày `16/09`.
- **Chưa có:** cụm máy chủ thử như thật · cơ sở dữ liệu riêng · nơi giữ mật khẩu · tên miền + chứng thư
  bảo mật · bảng theo dõi + nơi nhận cảnh báo.
- 🆕 **Thêm vì `S4`:** máy chạy VieNeu đủ sức đọc xong mỗi câu trong 5 giây · kho bản cài nội bộ.
- **Hệ quả:** lần chạy dây chuyền triển khai gần nhất thất bại vì không có nơi để triển khai.

> **Sếp đã trả lời ngày ______:** ______________________________________________

### S6 · Nhà mạng — đang chờ báo giá 💰 🟡

**Đã chốt:** trung kế SIP của **một** nhà mạng, bắt đầu **8 kênh**, đo thật rồi mới nâng.

**Tình hình `16/09`:** **MobiFone đồng ý** cho tổng đài công ty nối thẳng (đã xin lại đúng bảng giá) ·
Viettel báo ngừng trung kế SIP (cần văn bản) · VNPT đẩy sang gói đám mây, không hợp với gọi tự động.

**Câu hỏi lớn nhất:** tên công ty có hiện với khách **ở mạng khác** không? Nếu chỉ hiện cùng mạng, khoảng
**80% khách** (Viettel, VinaPhone) vẫn thấy dãy số.

- **Làm ngay:** giao bộ phận đơn hàng đếm cơ cấu mạng của 1.000–2.000 số khách gần nhất · nộp hồ sơ tên
  định danh.
- **Quyết khi có báo giá:** chọn nhà mạng · tên chỉ hiện với khách cùng mạng thì chấp nhận hay coi là không
  đạt.

Chi tiết: [báo cáo `15/09`, cập nhật `16/09`](../../docs/reports/2026-09-15-so-sanh-mobile-sip-trunk-va-gateway-32-sim.md).

> **Sếp trả lời:** Đếm cơ cấu mạng giao cho: ______ · Hồ sơ tên định danh giao cho: ______

### S8 · Owner tự chạy CLI theo hướng dẫn — trả lời ngày 21/09

Vì `S3` (giữ vĩnh viễn), **cách xoá duy nhất** là khi khách yêu cầu (`S2` rủi ro 2). Ngày `18/09` lệnh xoá
đã được sửa để **xoá đúng**, gồm cả số điện thoại (`W-0314`). Tại thời điểm 18/09 **chưa có nút hay lệnh nào để
chạy nó**: lệnh chỉ nằm trong mã, và chỉ bài kiểm thử gọi được. Nếu hôm nay có khách yêu cầu xoá, phải có
người viết mã mới làm được.

| # | Cần quyết | Vì sao cần người quyết |
| --- | --- | --- |
| 1 | **Ai được ra lệnh xoá** dữ liệu của một khách | Xoá không hoàn tác được. Không nên gắn vào quyền xem hàng đợi — nếu gắn, ai xem được là xoá được |
| 2 | **Chạy bằng gì** | Theo câu 1 |

| Cách | Làm gì | Được | Mất |
| --- | --- | --- | --- |
| **1** *(đề xuất)* | Một lệnh chạy trên máy chủ, có ghi nhật ký, chỉ người được giao chạy | Nhanh nhất, không chờ ai | Ai vào được máy chủ là chạy được — danh sách người vào máy chủ phải thật ngắn |
| **2** | Một nút trong trang quản trị, với quyền riêng *"xoá dữ liệu khách"* | Đúng chỗ, có phân quyền | Chờ Module 3 làm trang và nền tảng phân quyền cấp quyền mới — lâu hơn |
| **3** | Tạm thời Toàn làm tay mỗi lần có yêu cầu | Không tốn gì bây giờ | Phụ thuộc một người; mỗi lần là một lần viết mã |

**Đề xuất:** Cách 1, người được giao do Sếp chỉ định. Việc xác minh người yêu cầu có đúng là khách vẫn do
bộ phận bán hàng làm — IVR không có cách biết ai là ai.

*Nguồn: `docs/compliance/dsar-runbook.md` §1, §5 · `docs/evidence/W-0314/README.md` §1.*

> **Sếp trả lời:** Người được xoá: ______ · Cách ☐ 1 ☐ 2 ☐ 3 ☐ khác: ________

**Cập nhật 21/09 — W-0330:** Khi được hỏi S8 trong task, owner trả lời “chỉ t step by step đi t chạy cho”.
Theo yêu cầu này, Codex chuẩn bị CLI và [hướng dẫn](../../docs/compliance/dsar-cli-step-by-step.md)
để chính owner vận hành bằng tài khoản OS được chỉ định; mặc định xem trước, xác nhận đúng một mã đơn
trước khi xoá, có audit cùng giao dịch. Đây không phải chỉ thị cho agent xoá dữ liệu thật hoặc ký S2.
Kết quả kỹ thuật và phạm vi được ghi tại [W-0330](../../docs/evidence/W-0330/README.md).

### Ô ký gốc ngày 17/09 — giữ nguyên; S8 đã có trả lời trong task ngày 21/09

| Người duyệt | `S1` | `S2` | `S5` | `S6` | `S8` | Ngày |
| --- | --- | --- | --- | --- | --- | --- |
| Sếp | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Nguyễn Quốc Toàn | ☐ | ✓ nhận rủi ro có điều kiện, W-0340 | ☐ | ☐ | ☐ | 22/09/2026 |

---

## 3. Đang chờ bên ngoài *(không cần quyết, chỉ để biết)*

| Chờ ai | Chờ gì | Đang chặn |
| --- | --- | --- |
| **Module 3** (tech lead) | Trả lời [phiếu `IR-07`](../../integration-requirements/07-module-3-decision-sheet.md) — **32 câu** (thêm 2 câu `25/09`). **Ghi đã gửi `17/09`**; anh Mạnh báo `25/09` chưa nhận — gửi lại kèm đính chính `25/09` · đính chính `draft.31` **đã gửi** `17/09`, chưa phản hồi · đính chính bổ sung `A-9` (`W-0313`) **chưa gửi** | Chữ ký contract (`W-0204`) · `OD-V1-01/02/03/05` · callback (`m8-07`) · `golden_hour_session_id` (`m8-06`) · endpoint thu hồi đơn (`m8-17`, `B7`) · base URL callback (`M3-21`) · nghĩa của "tối đa 2 lần trong 10 phút" (`M3-22…25`) · `14` mục nhóm C bản `16/09` · vế breaking của stage 3 |
| **Nhà mạng** | Báo giá + trả lời 10 câu (§9 báo cáo `15/09`) | Adapter production (`B12`, ~2–3 tuần sau khi có đường thật) → đo thật (`W-0008`) → hiệu chỉnh 4 số năng lực (`B1`, ~1 ngày) · nửa sau `OD-V1-09` · `OD-V1-10` (32 kênh — **cố ý chưa ký**) |
| **Bộ phận đơn hàng** | Cơ cấu mạng của khách · số đơn thật theo giờ | Chọn nhà mạng · chốt số kênh (`m8-14`). Website và livestream chưa chạy nên chưa có số đơn thật |
| **CRM** | Chỗ nhận đề xuất "không gọi lại" | `W-0034` — **cố ý hoãn** (`DEFERRED_TARGET`) |
| **Chief auditor** | Kiểm lại bằng chứng, chuyển mục xuống nhóm D · bảng map result code `C7`/`C8` (cùng M3) | Sổ `16/09`. Dev không tự chuyển nhóm |

---

## 4. Đã gỡ, đừng mở lại

| Vướng cũ | Vì sao không còn |
| --- | --- |
| Task nhận vào nhưng không bao giờ được quay số (không ai gọi `/eligibility-checks`) | Worker có `EligibilityJobHost` từ `a0790e3` (`14/09`) |
| Chưa có bộ số lần gọi production | `OD-V1-16` ✅ `05/09`: Giờ Vàng 2 lần, cách 150 giây · 24/7 2 lần, cách 450 giây |
| Nhà mạng có cho tổng đài riêng của công ty nối vào không | MobiFone **đồng ý** (`16/09`) |
| Gateway 32 SIM hay trung kế SIP | Chốt trung kế SIP, một nhà mạng, 8 kênh |
| Kho khoá để giải mã số (mục 2 phiếu `17/09`) | Hết đối tượng từ phương án B — đổi thành rủi ro 1 ở `S2` |
| Phiếu cho Platform / Security / Legal | Không có các đội này — đã gom về Sếp (`W-0309`) |
| Phiếu `IR-07` chưa gửi | **Đã gửi** Module 3 (Toàn xác nhận `17/09`) |

---

## 5. Kế hoạch khắc phục

> Lập `17/09` theo các quyết định ở §1. **Đã thi hành:** Lô 1 (`W-0312`) · Lô 2 (`W-0314`) · Lô 3 trừ mục `13` (`W-0313`, `W-0315`, `W-0316`) · Lô 4 phần làm được ngay (`W-0315`, `W-0317`) · Lô 5 phần của agent (`W-0318`). **Chưa:** Lô 3 mục `13` (chờ Sếp ký `S2`), Lô 4 phần **Chờ**, Lô 5 phần Toàn duyệt.
> Mỗi lô là một `W-ID`, cấp **khi bắt đầu lô** — `NEXT_WORK_ID` hiện là `W-0317`. Cấp trước dễ trùng số: `A-0637`
> đã bị cấp hai lần.

### 5.0 · Luật thi hành

Kế thừa nguyên **10 luật** ở §1 của [kế hoạch `16/09`](../ke-hoach-khac-phuc-m8-2026-09-16.md). Thêm bốn
luật rút ra ngày `17/09`:

| # | Luật | Vì sao |
| --- | --- | --- |
| 11 | Gate sweep chạy **từ Git Bash**, không bao giờ song song với `dotnet test` | PowerShell ra `36/39` vì `sh ENOENT`; `selftest-dotnet-policy.sh` tự chạy `dotnet test` riêng |
| 12 | Cảnh báo `HIGH`/`CRITICAL` ⇒ báo Toàn, rồi **đọc lại thiết kế cho tới khi thay đổi hẹp nhất** | `CLAUDE.md` · bài học `W-0311` |
| 13 | Hành vi mới chỉ áp cho **đầu vào hôm nay đang hỏng** — đầu vào đang chạy được giữ nguyên | Blast radius thật nhỏ hơn blast radius trên giấy |
| 14 | `IR-07` **đã gửi** ⇒ không sửa câu đã gửi; đính chính bằng một mục có ngày ở cuối phiếu | Bản đã gửi là hồ sơ |

### 5.1 · Thứ tự

| Lô | Việc | Mục | Chờ ai | Gấp | Rủi ro GitNexus (`17/09`) |
| --- | --- | --- | --- | --- | --- |
| **1** | `draft.31` — Module 3 gửi số là nhận được · ✅ `W-0312` `TESTS_PASS` | `T1` `T3` `T5` | Không | 🔴 | `CreateDomainSnapshot` **CRITICAL** (`26` ký hiệu · `5` luồng) · `EvaluateAsync` **HIGH** (`23` · `3` luồng) · `ValidateSchema` LOW |
| **2** | Đường xoá dữ liệu phủ `phone_e164` · ✅ `W-0314` `TESTS_PASS` | `T2` | Không | 🔴 | `RetentionTargetCatalog` LOW · `DsarService` LOW — *index không theo tham chiếu hằng; đọc tay `DsarService.cs:77`* |
| **3** | Sổ sách một lượt · ✅ trừ mục `13` (`W-0313` · `W-0315` · `W-0316`) | `S3` `S7` `T4` `T6` `T8` + chỗ lệch | Không | 🟡 | Không đụng symbol |
| **4** | Mở lại nhánh VieNeu · ✅ phần làm được ngay (`W-0315` · `W-0317`) | `S4` | Một phần: `S1` `S2` `S5` | 🟡 | Chạy khi chốt symbol |
| **5** | Nghiệm thu theo đợt · ◐ tiêu chí + danh sách đầu (`W-0318`); chờ Toàn duyệt từng đợt | `T7` | Toàn duyệt từng đợt | ⚪ | Không đụng symbol |

**Vì sao thứ tự này:** Lô 1 trước vì Module 3 đã cầm phiếu. Lô 2 trước ngày có số thật. Lô 3 sau Lô 1–2 để
ghi luôn bằng chứng của chúng — làm trước cũng được, vì chỉ là tài liệu. Lô 4 chuẩn bị song song được phần
không chờ ai. Lô 5 cuối cùng: nghiệm thu trước khi sửa là nghiệm thu thứ sắp phải mở lại.

**GitNexus:** index dựng `16/09`, trước stage 1–2 của `W-0311`. Chạy lại `gitnexus_impact` lúc bắt đầu mỗi
lô; nếu báo stale thì `npx gitnexus analyze` — lệnh này sửa `AGENTS.md`, `CLAUDE.md`, `.claude/skills`,
vốn đã bẩn sẵn và không commit.

### Lô 1 · `draft.31` — Module 3 gửi số là nhận được (`T1` `T3` `T5`)

> **✅ Đã làm `17/09` — `W-0312`, `TESTS_PASS`.** Bằng chứng:
> [`docs/evidence/W-0312/README.md`](../../docs/evidence/W-0312/README.md).
>
> | Điều kiện *"Xong khi"* | Kết quả |
> | --- | --- |
> | Task chỉ-gửi-số được nhận khi bộ mã hoá `Unavailable` | ✅ `UT-INTAKE-NUMBER-01` · `IT-INTAKE-NUMBER-DB-01` |
> | … và quay được bằng số | ✅ ở mức thành phần: sổ đếm trên Postgres thật cho cả hai task (`IT-INTAKE-NUMBER-DB-04`), vault production chọn số (`UT-TRUNK-DIAL-08`, `W-0311`). **Chưa quay thật** — chưa có tuyến nhà mạng (`S6`) |
> | Ba đột biến đều bị bắt | ✅ `7/7` — thêm `3` đột biến và `M2b` chạy tới sổ đếm thật |
> | Ví dụ sandbox xanh qua HTTP thật | ✅ `28/28` |
> | `oasdiff` không breaking | ✅ `30→31` và `27→current` `exit 0` |
>
> **Lệch khỏi kế hoạch, có lý do:** (1) luật *"số **hoặc** cặp token"* viết bằng **một** `anyOf` có nhánh
> `not`, không phải `anyOf` trong `allOf` — cách trong kế hoạch cộng `dependentRequired` làm `oasdiff` ra
> `3` ERROR, hai cách từ chối giống hệt nhau trên `11` hình dạng body. (2) Bước 10 nói đính chính `IR-07`
> *"nếu gate đòi"* — không gate nào đòi, nhưng phiếu đang sai ở `8` chỗ nên vẫn làm. (3) Thêm fixture
> chỉ-gửi-số vào seed làm lệch `11` chỗ ghi cứng thành phần seed; đã sửa cả `11` (README bằng chứng §4.1).
>
> **Bước 12 — Toàn đã gửi Module 3 ngày `17/09`:** changelog `30→31`, mục *Đính chính* ở cuối `IR-07`, và câu
> *"`draft.30` vẫn bắt buộc `dial_token`; từ `draft.31`, gửi riêng `phone_e164` là đủ."* Chưa có phản hồi.

**Vì sao gấp hơn đã báo.** Đọc đường intake khi lập kế hoạch:

| # | Phát hiện | Bằng chứng |
| --- | --- | --- |
| 1 | `dial_token` vẫn bắt buộc ở OAS, endpoint và DB ⇒ task chỉ-gửi-số bị **400** | `specs/api/openapi/ivr-order-confirmation.v1.yaml:1286-1287` · `src/Ivr.Api/Intake/TaskIntakeEndpoint.cs:307-310, 417` · `src/Ivr.Infrastructure/Persistence/Migrations/20260812142435_P1_2_InitialTargetV1Persistence.cs:145-146, 175` |
| 2 | 🆕 Intake **luôn** mã hoá token. Ở mọi chế độ khác MOCK, bộ mã hoá là `Unavailable`, và production không thay nó ⇒ task bị từ chối `DIAL_TOKEN_PROTECTION_UNAVAILABLE`. Phương án B bỏ kho khoá ⇒ **ở production không task nào được nhận**, kể cả gửi cả số lẫn token | `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs:272-285` · `src/Ivr.Infrastructure/Configuration/ServiceCollectionExtensions.cs:179` · `src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs:837-846` |
| 3 | 🆕 Sổ đếm lần quay gắn token với **task đầu tiên** dùng nó ⇒ một giá trị giả **dùng chung** làm mọi task chỉ-gửi-số sau task đầu bị từ chối | `src/Ivr.Infrastructure/Telephony/PostgresDialTokenResolveLedger.cs:64, 92-93` · `src/Ivr.Infrastructure/Telephony/DialTokenResolveEntity.cs:57` |
| 4 | Không test, không driver nào gửi body chỉ có số | `deploy/ci/scripts/sandbox-examples.mjs:264` · `tools/dev/Invoke-LocalMockE2E.mjs:377` · `deploy/ci/scripts/image-selftest.mjs:509` |

**Thiết kế — hẹp nhất có thể (luật 12, 13).** Giữ nguyên chữ ký của `CreateDomainSnapshot`,
`DialTokenReference`, `ConfirmationTaskSnapshot`, `IDialTokenResolver`. Chỉ đầu vào **hôm nay đang hỏng**
đổi hành vi:

| Đầu vào | Hôm nay | Sau Lô 1 |
| --- | --- | --- |
| Chỉ token | Nhận ở MOCK/LAB · từ chối ở production | **Không đổi** |
| Token + số, bộ mã hoá dùng được (MOCK/LAB) | Nhận, lưu token đã mã hoá | **Không đổi** — LAB vẫn quay alias |
| Token + số, bộ mã hoá `Unavailable` (production) | Từ chối | **Nhận** — lưu tham chiếu riêng của task, quay bằng số |
| Chỉ số | 400 | **Nhận** — lưu tham chiếu riêng của task, quay bằng số |
| Không số, không token | 400 | **Không đổi** |
| Số sai định dạng | Nhận, hỏng lúc quay | **400** |

**Tham chiếu riêng của task:** `enc:direct:` + SHA-256 hex của `task_id`; hết hạn đúng
`confirmation_window_expires_at`. Thoả `ck_ivr_confirmation_tasks_token_ttl`, luật `W-0302`, và bộ chặn số
trần — dãy chữ số nằm trong chuỗi hex không bị bắt (`src/Ivr.Domain/Confirmation/Identifiers.cs:106-110`).
**Duy nhất theo task**, nên sổ đếm không đụng nhau. Khi ẩn danh, trường này thành `enc:redacted` như mọi
token. Ở LAB, task chỉ-gửi-số **không quay được** — vault LAB chỉ nhận `enc:lab-sha256:`
(`src/Ivr.Infrastructure/Telephony/LabDialTokenVault.cs:62`). Cố ý: LAB chỉ quay alias trong allowlist.

**Bước**

1. Chạy lại `gitnexus_impact` cho `CreateDomainSnapshot` · `EvaluateAsync` · `ValidateSchema` ·
   `ContactRejectionReason` · `ValidateIdentityAndWindow`; báo Toàn blast radius, cảnh báo `CRITICAL`/`HIGH`.
2. **OAS** `IvrConfirmationTaskV1`: bỏ `dial_token`, `dial_token_expires_at` khỏi `required`; thêm vào
   `allOf` một `anyOf` — `required: [phone_e164]` **hoặc** `required: [dial_token, dial_token_expires_at]`.
   Sửa mô tả `phone_e164`: bỏ lời hứa *"naming the field"*. Ba summary dev-tooling (dòng `458`, `490`, `523`)
   ghi `ivr.admin.write` (`src/Ivr.Api/Auth/AdminPolicies.cs:18`). Version `1.0.0-draft.31`. Sinh lại
   `IvrServerModels.g.cs`.
3. **Endpoint** `TaskIntakeEndpoint`: chuyển hai field token từ `RequiredTaskProperties` và
   `RequiredTaskStringProperties` sang tập được phép; `ValidateSchema` thêm luật *"có số **hoặc** đủ cặp
   token"* và kiểm `phone_e164` theo `^\+84[0-9]{9}$`.
4. **Mapper** `TargetV1TaskMapper.CreateDomainSnapshot` (`TargetV1ContractMapper.cs:76`): không có token ⇒
   dùng tham chiếu riêng của task.
5. **Intake** `TaskIntakeService.EvaluateAsync` (`:272-285`): không có token ⇒ không gọi `Protect`. Có token,
   `Protect` ném lỗi, **và có số** ⇒ dùng tham chiếu riêng thay vì từ chối. Các kiểm tra riêng của token
   (`ContactRejectionReason`, luật `W-0302`) bỏ qua khi không có token.
6. **Sửa comment và tài liệu đang sai**, cùng lô: `TaskIntakeService.cs:754-757` · câu *"never written to the
   application database"* ở `ProductionDialTokenVault.cs:22-26` · mô tả `dial_token_ciphertext` trong
   `PersonalDataInventory` và `docs/compliance/data-inventory.md`.
7. **Test** — `TestId` mới:
   - chỉ số, bộ mã hoá `Unavailable` ⇒ nhận; hàng có `phone_e164` và tham chiếu riêng
   - token + số, bộ mã hoá `Unavailable` ⇒ nhận; token **không** bị mã hoá
   - chỉ token, bộ mã hoá `Unavailable` ⇒ vẫn `DIAL_TOKEN_PROTECTION_UNAVAILABLE`
   - không số, không token ⇒ 400
   - số sai định dạng ⇒ 400, ca test **sinh từ chính attribute `[RegularExpression]`** của model sinh ra từ
     OAS — áp cho mọi pattern trên `IvrConfirmationTaskV1`, không riêng `phone_e164`
   - hai task chỉ-gửi-số quay lần lượt trên Postgres thật ⇒ cả hai đều được, không
     `TOKEN_BOUND_TO_OTHER_TASK`
8. **Đột biến hai chiều**, ghi vào evidence: bỏ nhánh không-gọi-`Protect` · dùng một tham chiếu **chung** ·
   bỏ kiểm pattern ⇒ mỗi đột biến phải làm đúng test tương ứng đỏ.
9. **Chạy từ ngoài:** thêm ví dụ chỉ-gửi-số vào `sandbox-examples.mjs`; chạy `pnpm sandbox:up` rồi
   `pnpm sandbox:examples` qua HTTP thật.
10. **Dây chuyền contract (~8 nơi):** `contract-manifest.json` · `portal-manifest.json` ·
    `docs/api-changelog.md` · changelog `30→31` · baseline `draft.31.yaml` · con trỏ version `IR-06` · pin
    validator `IR-06`. `oasdiff` bằng image ghim (chạy từ PowerShell), cả `30→31` lẫn `27→current`, phải
    **không breaking**. Re-pin trên byte LF. Nếu gate đòi con trỏ version trong `IR-07`: thêm mục *"Đính
    chính"* có ngày ở cuối phiếu (luật 14).
11. `dotnet test Ivr.sln` · gate sweep `39/39` · sinh lại traceability · `gate-status.mjs --write` ·
    `gitnexus_detect_changes` · `ListAgents` · `git commit -- <đường dẫn>`.
12. **Toàn gửi Module 3:** changelog `30→31` kèm một dòng — *"`draft.30` vẫn bắt buộc `dial_token`; từ
    `draft.31`, gửi riêng `phone_e164` là đủ."*

**Cố ý không làm:** bắt `phone_e164` thành bắt buộc (vế breaking — chờ Module 3 xác nhận đã gửi số) · xoá 3
cột token (stage 5) · đổi hình dạng `DialTokenReference`, `ConfirmationTaskSnapshot`, `IDialTokenResolver` ·
cho lỗi 400 nêu tên field (sửa lời hứa trong OAS thay vì thêm hành vi) · cho LAB quay task chỉ-gửi-số.

**Xong khi:** task chỉ-gửi-số được nhận ở cấu hình production (bộ mã hoá `Unavailable`) và quay được bằng
số · ba đột biến đều bị bắt · ví dụ sandbox xanh qua HTTP thật · `oasdiff` không breaking.

### Lô 2 · Đường xoá dữ liệu phủ `phone_e164` (`T2`)

> **✅ Đã làm `18/09` — `W-0314`, `TESTS_PASS`.** Bằng chứng:
> [`docs/evidence/W-0314/README.md`](../../docs/evidence/W-0314/README.md).
>
> | Điều kiện *"Xong khi"* | Kết quả |
> | --- | --- |
> | Sau DSAR, câu SQL trên DB thật không thấy số đọc được nào của đơn đó | ✅ `1` → **`0`**, đơn khác vẫn `1` (`COMP-DSAR-08`, Postgres `16` thật). Câu SQL ở README §6 |
> | Gate đối chiếu xanh, và đỏ được khi đục | ✅ `COMP-PII-02` — đỏ theo **cả hai chiều** (`M1`, `M8`); `8/8` đột biến bị bắt |
>
> **Lệch khỏi kế hoạch, có lý do:** (1) Bước `3` trỏ nhầm: `P2_1_TaskIntake.cs:226-274` là thân trong
> **`Down`** (bản cũ). Dùng `Up` (`134-190`); không thì `4` cột script/policy thôi bất biến. (2) Toàn chốt
> `18/09` thêm ba việc: xoá lần hai không còn ném lỗi (`AND anonymized_at IS NULL`) · migration dọn dòng đã
> xoá kiểu cũ · `trusted_skip_allowed` vào danh mục và câu xoá. (3) Trigger **không** thêm ba cột Sales — lý
> do ở README §3. (4) Thấy khi làm: DSAR **chưa có lối chạy** ⇒ `S8` ở §2.
**Hiện trạng:** câu xoá dùng chung cho job lưu trữ và yêu cầu xoá của khách (DSAR) không có `phone_e164`;
DSAR vẫn đặt `anonymized_at` nên dòng **trông như đã ẩn danh** trong khi số còn nguyên; trigger bất biến
không biết cột này ⇒ đích quay số sửa được sau khi nhận
(`src/Ivr.Infrastructure/Retention/RetentionTargetCatalog.cs:27-31` ·
`src/Ivr.Infrastructure/Governance/DsarService.cs:75-80` ·
`src/Ivr.Infrastructure/Governance/PersonalDataInventory.cs:123-133`). Vì `S3`, DSAR là đường xoá **duy
nhất**.

**Bước**

1. `gitnexus_impact` cho `RetentionTargetCatalog`, `DsarService`; đọc tay mọi chỗ dùng
   `SpeechSnapshotRedactionSql` (`DsarService.cs:77`, `RetentionTargetCatalog.cs:73`).
2. `SpeechSnapshotRedactionSql` thêm `phone_e164 = NULL`.
3. **Migration mới** (`pnpm db:migration:add`, sửa tay `IDE0161`/`CA1062`): `CREATE OR REPLACE FUNCTION
   ivr_enforce_confirmation_task_snapshot_immutable()` dựa trên bản cuối ở
   `src/Ivr.Infrastructure/Persistence/Migrations/20260813111817_P2_1_TaskIntake.cs:226-274`, thêm
   `phone_e164` vào danh sách bất biến và `NEW.phone_e164 IS NULL` vào nhánh ẩn danh; `Down` trả lại bản cũ.
   Không `ALTER TABLE` ⇒ qua `migration-expand-guard`.
4. **Thứ tự rollout:** pod cũ chạy câu xoá thiếu `phone_e164` sẽ bị trigger mới từ chối ⇒ **không chạy DSAR
   trong lúc rollout**. Lỗi theo hướng đóng, không lộ. Ghi vào runbook.
5. **Test** (`tests/Ivr.IntegrationTests/Governance/ComplianceTests.cs`, Postgres thật): DSAR trên hàng có số
   ⇒ `phone_e164` NULL và `anonymized_at` được đặt · ẩn danh qua retention (kỳ hạn đặt trong test) ⇒ NULL ·
   `UPDATE phone_e164` trên hàng đang sống ⇒ trigger chặn.
6. **Gate đối chiếu** (`tests/Ivr.UnitTests/Governance/PersonalDataInventoryTests.cs`): mọi cột
   `ivr_confirmation_tasks` khai *"Replaced with a redacted value"* phải có mặt trong
   `SpeechSnapshotRedactionSql`. Hôm nay gate này bắt đúng **một** cột: `phone_e164`.
7. **Đột biến:** bỏ `phone_e164 = NULL` ⇒ test DSAR đỏ và gate đỏ · bỏ dòng trigger ⇒ test bất biến đỏ.
8. **Toàn chốt khi làm lô này — hệ quả của `S3`.** Ba cột khoá của Sales (`customer_id`,
   `official_contact_id`, `customer_trust_status`) khai *"xoá cùng dòng khi hết hạn"*; không còn hạn ⇒
   **không bao giờ bị xoá, kể cả khi khách yêu cầu**. Đề xuất: đưa `official_contact_id` và
   `customer_trust_status` vào câu xoá DSAR; giữ `customer_id` như `order_code` (khoá đối chiếu) và ghi vào
   `DsarService.NotErasable` kèm lý do. Sửa lời khai trong `PersonalDataInventory` và
   `docs/compliance/data-inventory.md` cho khớp.
9. Phiếu thời hạn lưu `docs/compliance/retention-period-proposal.md`: thêm `phone_e164`.
10. Kiểm và commit như bước 11 của Lô 1.

**Cố ý không làm:** đặt kỳ hạn retention (trái `S3`) · xoá hẳn dòng thay vì che.

**Xong khi:** sau DSAR, câu SQL trên DB thật không tìm thấy số điện thoại dạng đọc được nào của đơn đó
(ghi câu và kết quả vào evidence) · gate đối chiếu xanh, và đỏ được khi đục thủng.

### Lô 3 · Sổ sách một lượt (`S3` `S7` `T4` `T6` `T8` + chỗ lệch)

Chỉ tài liệu, `0` symbol.

| # | Ghi gì | Ở đâu |
| --- | --- | --- |
| 1 | ✅ **`W-0316`** — `S3` — bỏ các kỳ hạn 90 ngày · 180 ngày · 1 năm; giữ vĩnh viễn | register `OD-V1-11` (thêm *"Sửa 17/09"*, không xoá nguyên văn) · `retention-period-proposal.md` §1 · `W-0273` → `EVIDENCE_SUBMITTED` |
| 2 | ✅ **`W-0315`** — *`18/09`.* `S4` — giữ VieNeu tự host lúc chạy; *"không vendor đám mây lúc chạy"* giữ nguyên. Lab và code cũng chỉ còn VieNeu | register `OD-V1-19` · `00-CHUA-XONG.md` (`today-03`, nhóm A: nhánh `W-0122` mở lại) |
| 3 | ✅ **`W-0316`** — `S7` — kho ghi nhận ngoài là giới hạn chấp nhận của bản đầu. *Dòng mới là `OD-V1-24`* | register, dòng mới · **không** sửa `m8-15` (ghim ở `deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs:27`) |
| 4 | ✅ **`W-0316`** — `T4` — phần *"không ghi DB"* của `OD-V1-18` bị thay bởi phương án B; *"không vào log, evidence, callback"* giữ nguyên | register `OD-V1-18` |
| 5 | ✅ **`W-0316`** — `T6` — điều kiện của stage 5: phải có lối có kiểm soát qua `migration-expand-guard`. *Lối miễn duy nhất hôm nay (`migration-expand-baseline.json`) chỉ nhận migration cũ hơn mốc; chưa ai dựng lối cho bản contract* | README `W-0311` §6 |
| 6 | ✅ **`W-0316`** — `T8` — `W-0163` → `CANCELLED` (định tuyến đã thay bằng hai phiếu `W-0309`/`W-0310`) · `W-0118` → `N/A` *(HISTORICAL; bị `OD-18` thay, `W-0123`)* | tracker §5 |
| 7 | ✅ **`W-0316`** — `IR-07` đã gửi. *Thêm `integration-requirements/00-index.md`, nơi vẫn ghi "21 mục, gửi 10/09"* | dòng trạng thái `IR-07` · `00-CHUA-XONG.md` · README `W-0310` |
| 8 | ✅ **`W-0316`** — Phiếu cho Sếp đã được trả lời mục 1–4; trỏ về file này | `phieu-quyet-dinh-cho-sep-2026-09-17.md` |
| 9 | ✅ **`W-0316`** — `ca8d13b` nhầm `W-0310` ở **17 dòng / 15 file**, cộng changelog và 2 trang HTML — không chỉ tiêu đề. **Không** đổi tên migration hay baseline. *Đếm lại tại `53c2eb5`: `18` dòng / `16` file, do **ba** commit ghi — `7c4204e` (tiêu đề đúng) và `W-0314` cũng ghi nhãn sai; số `17/15` không tái tạo được. Không sửa dòng nào* | README `W-0311` · tracker §2 |
| 10 | ✅ **`W-0316`** — `A-0637` bị cấp hai lần — ghi chú tại chỗ, **không đánh số lại** lịch sử | tracker §7 |
| 11 | ✅ **`W-0316`** — Bỏ "nguồn khoá token" khỏi danh sách chờ Sếp · `m8-11` *"chưa có bộ số"* → `OD-V1-16` ✅ `05/09`. *Cũng bỏ `m8-15` khỏi danh sách đó — `S7` đã chốt* | `00-CHUA-XONG.md` |
| 12 | ✅ **`W-0316`** — `W-0007` (xung đột bộ số — đã giải `05/09`) · `W-0121` (CI hosted đã chạy ở `W-0292`) · `W-0171` (c) (`progressive-selftest` nay xanh). *`W-0121` giữ `CODE_DONE`: pipeline chạy nhưng không xanh, mà điều kiện đổi trạng thái của chính dòng đó là pipeline xanh. `W-0171` (a) cũng đã xong ở `W-0312`* | tracker §4, §5 |
| 13 | Người nhận rủi ro phương án B: *"owner"* → tên người ký `S2` — **chỉ làm sau khi Sếp ký** | `IR-07` (mục đính chính) · `data-inventory.md` · comment migration/entity · mô tả OAS ở bản contract kế tiếp |
| 14 | ✅ **`W-0316`** — Bàn giao: `13` file bẩn (`10` chỉ lệch CRLF/LF) · sweep chỉ `39/39` khi chạy từ Git Bash — *sửa `17/09`: `git status` sạch từ `5d5f96b`; sweep nay `40` gate (`W-0313`)* | `plan/BAN-GIAO-phien-moi-2026-09-17.md` |
| 15 | ✅ **`W-0313`** — *thêm khi làm Lô 1, Toàn duyệt `17/09`.* Job CI `pii_scan` (`allow_failure: false`) đỏ ở mọi commit từ `257cbef`: `41` dòng **báo nhầm** ở `13` README `W-0297`…`W-0311` — từ tiếng Việt thông dụng trùng mẫu địa chỉ. Đổi từ, **không** nới mẫu; thêm `scan-pii.sh docs/evidence` vào phần *Kiểm* của mọi lô, vì gate sweep chỉ chạy selftest của nó. **Đã làm bằng gate thay cho luật:** sweep nay chạy chính lần quét đó trên `docs/evidence` | `13` README evidence · `deploy/ci/gate-invocations.json` |
| 16 | ✅ **`W-0313`** — *như trên.* `IR-06 §3.4.1` vẫn tả intake **trước** `W-0302` (chiều *muộn* *"hỏng ở persistence"*), trái với dòng `1311` của chính nó | `IR-06` · re-pin `7` nơi |
| 17 | ✅ **`W-0313`** — *như trên.* `IR-07` `A-9` *"metadata giữ 90 ngày"* lệch với `S3` (Toàn chốt: toàn bộ dữ liệu). Cũng sửa câu giống hệt ở `IR-06` | `IR-07` mục *Đính chính bổ sung* · `IR-06` |
| 18 | ✅ **`W-0313`** — *thêm `17/09`.* `00-CHUA-XONG.md` `m8-09` vẫn ghi *"chờ owner quyết A hay B"* dù đã chọn B ngày `09/09` (`W-0248`) | `plan/ivr-orther/00-CHUA-XONG.md` |
| 19 | ✅ **`W-0313`** (follow-up `A-0640`) — *thấy khi làm `W-0313`, Toàn duyệt `17/09`.* `specs/api/06-error-codes.md` thiếu `DIAL_TOKEN_EXPIRES_AFTER_WINDOW` của `W-0302`; sửa cùng hai con số đếm trong file | `specs/api/06-error-codes.md` |
| 20 | ✅ **`W-0316`** — *làm theo đề xuất: `18/09` Toàn bảo làm cho xong.* *Thấy khi làm follow-up mục `19`.* Gate `dr-selftest.mjs` hết giờ ở trần mặc định `180s` của sweep — `W-0279` một lần, `17/09` ba lần liên tiếp; chạy lại thì PASS. Log sweep ghi gate ở `130`–`176s`, một lần đo với trần `600s` ra `193.5s`: vượt thật, không treo, nguyên nhân chưa rõ. **Đề xuất:** thêm `"timeoutMs": 300000` cho gate này, như `gate-status.mjs` đã có — vẫn bắt được treo. Không duyệt thì giữ cách `W-0279`: hết giờ thì chạy lại `--only` | `deploy/ci/gate-invocations.json` |

**Kiểm:** gate sweep `40/40` *(từ `W-0313`)* · `gate-status.mjs --write` · quét pin `0` lệch — sửa file bị ghim thì re-pin
ngay trong lượt, tính trên byte LF.

### Lô 4 · Mở lại nhánh VieNeu (`S4`)

**Làm được ngay, không chờ ai**

1. ✅ **`W-0317`** — Quét lại image `ivr-tts` bằng Trivy ghim; cập nhật danh sách 16 lỗ hổng, có ngày.
   *Kết quả `18/09`: `57` (`54 HIGH` · `3 CRITICAL`), `13` có bản vá. Đã ghim `4` gói Debian có bản vá ⇒ còn
   `44 HIGH`, `0 CRITICAL`.*
2. ✅ **`W-0317`** — **Thử đổi bản cài nền** (hướng `SEC-B` cũ) sao cho hết lỗ `HIGH`/`CRITICAL`. Làm được thì rủi ro 3 của
   `S2` **biến mất** và Sếp không phải ký nó. Chạy lại `tts-container-selftest` và `tts-helm-selftest`, đo
   lại xem các lỗ còn lại có bị gọi tới không. *Kết quả: Debian distroless còn `21 HIGH` ngay ở image gốc;
   **Chainguard `0`**, qua cả hai selftest. **Chưa đổi hẳn** — `Dockerfile.tts` vẫn nền Debian đã vá — vì còn hai
   việc: một lượt đọc thật bằng bundle model (`~211 MB`, chưa có trên máy), và Sếp chọn ở `S2` rủi ro 3. `44` lỗ
   còn lại đã đo: không thư viện nào của chúng được nạp khi chạy.*
3. ✅ **`W-0315`** — Đưa những câu hỏi còn sống của ba phiếu cũ (`platform-w0122`, `security-w0122-cve`,
   `legal-od-voice-07` — toàn văn ở `257cbef^`) về đúng người, **không** tạo lại ba file: bảo mật và quyền
   dùng → `S2` · kho bản cài nội bộ (`OD-VOICE-07`) và máy thật (`OD-VOICE-08`) → `S5`. Ghi ở nhóm A của
   `00-CHUA-XONG.md` và mục 5 của phiếu cho Sếp.
4. ✅ **`W-0317`** — Soạn sẵn cấu hình production ở dạng **nháp** (ConfigMap nghiệm thu giọng, tham chiếu phê duyệt).
   `deploy/helm/ivr/values-prod.yaml` **vẫn** `tts.enabled: false`. *Nháp ở
   `deploy/helm/ivr/values-prod-tts.draft.yaml`: điền sẵn giọng, catalog `12` đoạn, hash `MODELS.lock`, tham chiếu
   nghiệm thu giọng; để trống đúng `14` ô còn chờ `S2`/`S5`/`S6` — render chứng minh thiếu ô nào cũng bị từ chối.*

**Chờ**

| Bước | Chờ |
| --- | --- |
| Đo trên máy thật: mỗi câu ≤ 5 giây, còn dư 20% trước lúc quay (`deploy/ci/scripts/tts-helm-selftest.mjs:70-96`) | `S5` |
| Nghe duyệt 12 đoạn cố định | `S1` (người duyệt) |
| 6 cuộc gọi thử MicroSIP · khứ hồi âm thanh · diễn tập khôi phục — phần còn thiếu của `W-0122` | `S5` |
| Ký rủi ro 3 và 4 | `S2` |
| Bật `tts.enabled: true` ở production — chart tự từ chối nếu thiếu bất kỳ điều kiện nào | Tất cả các bước trên |

**Cố ý không làm:** bật TTS ở production trước khi đủ điều kiện · dựng lại ba file phiếu gửi những đội không
tồn tại.

### Lô 5 · Nghiệm thu theo đợt (`T7`)

1. ✅ **`W-0318`** — Ghi **tiêu chí nghiệm thu** vào tracker §1. Đề xuất: một việc đạt khi đủ cả bốn điều
   - README evidence tồn tại đúng đường dẫn trong `gate-status.yaml` và mang `REAL_CUSTOMER_CALL_ALLOWED=NO`
   - mọi `TestId` trong evidence có trong `docs/traceability-tests.md` và xanh ở `HEAD`
   - cột *Residual/next* không còn việc thuộc về IVR — chỉ còn việc chờ bên ngoài
   - gate sweep `40/40` *(từ `W-0313`)* tại commit nghiệm thu — *ghi vào tracker thành "mọi gate trong manifest đều chạy", không kèm con số: `W-0318` thêm gate thứ `41`*
2. ✅ **`W-0318`** — `deploy/ci/scripts/acceptance-batches.mjs`; danh sách đầu ở `docs/release/acceptance-batches.md`: `218` ứng viên, `0` ĐẠT, `138` XEM, `80` KHÔNG ĐẠT. Script **chỉ đọc** lập danh sách đề nghị cho từng đợt theo phase (`P0`…`P11`, `UNPLANNED`): đạt hoặc không
   đạt, kèm lý do.
3. **Toàn duyệt từng đợt** và tự chuyển trạng thái sang `ACCEPTED` — agent chỉ chuẩn bị danh sách.
4. `gate-status.mjs --write` sau mỗi đợt. Nấc 1 đạt khi mọi prompt đã lên kế hoạch được nghiệm thu và không
   còn `BLOCKED_INTERNAL` (`docs/release/readiness-board.md:17-26`).

Làm sau Lô 1–3.

### Không nằm trong kế hoạch này

| Việc | Vì sao |
| --- | --- |
| Bắt `phone_e164` thành bắt buộc (vế breaking của stage 3) · đánh `SUPERSEDED` cho `OD-V1-05`, `OD-V1-17` | Chờ Module 3 xác nhận đã gửi số |
| Xoá 3 cột token (stage 5) | Chờ Module 3 cắt hẳn, và lối qua gate (`T6`) |
| Adapter production của nhà mạng (`B12`) · hiệu chỉnh năng lực (`B1`) | Chờ báo giá (`S6`) |
| Mọi việc thuộc `S1` `S2` `S5` `S6` | Chờ Sếp |
