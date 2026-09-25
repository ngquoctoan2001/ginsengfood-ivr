# PHẢN HỒI MODULE 8 — danh sách chief `25/09`

**Người gửi:** Nguyễn Quốc Toàn — dev Module 8 (IVR Order Confirmation) · **Ngày:** `25/09/2026`
**Đối tượng:** [`toan-viec-can-lam-m8-2026-09-25.md`](toan-viec-can-lam-m8-2026-09-25.md) (chief đo tại `5603204`)
**Kế hoạch thi hành, có cột trạng thái cập nhật theo từng việc:** [`ke-hoach-khac-phuc-m8-2026-09-25.md`](ke-hoach-khac-phuc-m8-2026-09-25.md)

> Soạn bằng agent theo yêu cầu của Toàn, Toàn chốt gửi `25/09`. Mọi khẳng định có `file:dòng`; phần rà dùng 10
> agent chỉ-đọc rồi đối chiếu lại trên `f3e26a6`. Chỗ nào là suy từ đọc code, chưa chạy tái hiện, ghi rõ.

---

## 0. Một màn hình

| Chief cần làm | Mục |
| --- | --- |
| Gửi nguyên văn bảng `ivr-cancel-reason-map.v1` (file `_SPEC/FIX_M8.md` không có trên máy dev) | §1 |
| Biết hai lỗi mới, cả hai nằm trên đường tới đơn thật đầu tiên | §2 |
| Quyết `Q-20…Q-29` — Module 8 đã chốt đề xuất cho từng mục | §3 |
| Sửa các chỗ danh sách `25/09` lệch với code | §4 |
| Trả lời một câu về `OD-V1-11` | §5 |

Toàn đã tự chốt 4 quyết định thuộc Module 8 (§6). Phần dev tự làm được đã commit ở `W-0354` (`f3e26a6`) và
`W-0355` (`c2435b9`); các lô tiếp theo đang chạy (§7).

---

## 1. Xin nguyên văn `ivr-cancel-reason-map.v1` (`C22`)

Danh sách dặn chép đúng bảng v1, không tự dựng. File không có trên máy dev, nên trong lúc chờ, Module 8 **vô hiệu
bảng v0** ở `IR-06` §4.3 bằng đính chính có ngày, chỉ gồm các dòng chief đã viết rõ trong danh sách (`:366-368`,
`:374`, `:406-408`):

| Đơn | Kết quả (+ action) | Xử lý phía Module 3 |
| --- | --- | --- |
| 24/7 COD | `IVR_NO_ANSWER_FINAL` | Hủy, lý do `IVR_NO_ANSWER_MAX` |
| 24/7 COD | `IVR_CONFIRMATION_WINDOW_EXPIRED` + EXPIRE | Hủy, `IVR_CONFIRMATION_EXPIRED` |
| 24/7 COD | `IVR_CONFIRMATION_WINDOW_EXPIRED` + HOLD_ADMIN_REVIEW, hoặc `IVR_CAPACITY_EXCEPTION` | Không tự hủy; người trực hủy thì `IVR_CONFIRMATION_INVALID`, fault `NONE` |
| Giờ Vàng | `IVR_NO_ANSWER_FINAL` | Xác nhận hết hiệu lực, nhả suất theo flow 05, lý do `IVR_NO_ANSWER_MAX` |
| Giờ Vàng | `IVR_CONFIRMATION_WINDOW_EXPIRED` + HOLD_ADMIN_REVIEW | `IVR_CONFIRMATION_INVALID` |

**Còn chờ nguyên văn:** `IVR_CUSTOMER_CANCELLED` và `IVR_INVALID_PHONE_FINAL` ở cả hai chương trình;
`IVR_CAPACITY_EXCEPTION` và mã lý do `WINDOW_EXPIRED` + EXPIRE của Giờ Vàng. Khi nhận, Module 8 dán nguyên văn và
thêm ca sandbox "IVR hỏng suốt cửa sổ → Module 3 không hủy đơn 24/7 COD". IR-07 chỉ gửi lại cho anh Mạnh **sau**
khi bảng v0 đã bị vô hiệu.

---

## 2. Hai lỗi mới — cả hai ngoài danh sách `25/09`

### 2.1 API luôn nhận token tĩnh của Order Core, kể cả ở production (`Q-23`)

- `src/Ivr.Api/Auth/OrderCoreAllowlistMiddleware.cs:13, :71` hỏi `LegacyCredentialAccepted(CallbackDeliveryOptions.Provider)`.
  API **không** bind `CallbackDeliveryOptions` — chỉ worker bind (`src/Ivr.Worker/Program.cs:68`) — nên trong API giá
  trị luôn là mặc định `FAKE_TARGET_V1` (`CallbackDeliveryOptions.cs:12`), và token tĩnh `ORDER_CORE_SERVICE_TOKEN`
  luôn được nhận.
- `PRODUCTION_REAL` bắt buộc `SALES_PROVIDER=TARGET_V1` (`IvrOptionsValidator.cs:82-83`), nên ở production endpoint
  tạo lệnh gọi khách — có cả `phone_e164` — vẫn chạy bằng một secret tĩnh dùng chung, không gắn danh tính. Trái
  `W-0032` (đã ACCEPTED), `OD-V1-07`, `IR-06` `:1386`, `docs/secret-inventory.md:26`. Cờ
  `CurrentCompatTokenEnabled` (`ServiceIdentity.cs:73`) không ai đọc. Chỉ có test cho hàm thuần (`UT-AUTH-COMPAT-11`),
  không có test tích hợp nào chạy API với `TARGET_V1`.
- Ba agent rà độc lập cùng tìm ra; suy từ đọc code và DI, chưa chạy tái hiện.
- **Module 8 chốt đề xuất: sửa ngay theo hướng fail-closed** — middleware đọc `IvrOptions.SalesProvider` (API đã
  bind), thêm test tích hợp `TARGET_V1` gửi token tĩnh phải bị từ chối. Hệ quả: intake production đóng tới khi có
  issuer thật (E-1). Production chưa chạy nên hôm nay không mất gì; nhưng đường tới đơn thật đầu tiên thêm phụ thuộc
  issuer — **xin chief/Tech Lead xác nhận thứ tự** trước khi Module 8 sửa. Trước khi sửa sẽ kiểm mọi
  compose/profile đang dùng `TARGET_V1` (lab S5) để khỏi gãy lab.

### 2.2 `DispatchGate` chặn mọi địa chỉ quay production (`Q-28`)

- Vault tạo địa chỉ dạng `sip:{số}@{host}` (`ProductionDialTokenVault.cs:110-116`); gateway đưa địa chỉ đó vào gate
  (`AsteriskSchedulerDispatchGateway.cs:75-79`); `DispatchGate.cs:20` chạy `PiiGuard.EnsureSafeText` và regex số
  điện thoại (`PiiGuard.cs:40-41`) khớp ⇒ ném lỗi. Nếu lọt, `:55-58` còn đòi allowlist của lab — allowlist này không
  thể chứa số vì guardrails bắt mọi mục qua PiiGuard (`FeatureFlagGuardrails.cs:45-48`). Nhánh production (`:65-73`)
  chưa có test nào.
- Hệ quả: mở `IsReady` production xong, production vẫn từ chối mọi cuộc và ghi thành lỗi mạng. Câu "không còn việc
  lõi trong tay dev" ở `C16` vì vậy chưa đúng. Hôm nay an toàn vì cổng đang đóng.
- **Module 8 chốt đề xuất PA2:** nhánh production bỏ allowlist lab, gate nhận tham chiếu không chứa số, kèm danh sách
  pilot lưu vân tay HMAC của số (hai người duyệt; một approval có chữ ký chuyển pilot → mở) — khớp kế hoạch SIP
  (`:191`). Nếu khoá bí mật phải chờ Platform thì đo `W-0008` bằng hồ sơ `LAB_REAL_SIM` qua trunk trong lúc chờ.

---

## 3. Xin chief quyết `Q-20…Q-29` — Module 8 đã chốt đề xuất

Các mục này thuộc thẩm quyền chief/Tech Lead (`Q-27` cần thêm Sếp và Module 3 đối ký). Module 8 **không tự ký**;
đây là vị trí cuối của Module 8. Ba phương án đầy đủ cho từng mục ở kế hoạch §5, nhóm 2.

| Mã | Việc | Module 8 đề xuất | Lý do chính |
| --- | --- | --- | --- |
| `Q-20` | `C13`: thu hồi task có phát callback không | Không callback; ACK đồng bộ là xác nhận duy nhất | Khớp RVK-10; không đổi contract callback và CHECK trong DB. Quyết được ngay, không cần chờ `M3-14` |
| `Q-21` | `C10`: tên trường callback trên dây | Giữ shape Module 8; §14 trỏ về OAS callback | Đã đóng băng qua gate; 11 ca E2E và smoke chief 25/09 đạt |
| `Q-22` | Đơn 24/7 `T0` 21:00:30–21:07:59 chỉ kịp 1 cuộc rồi `WINDOW_EXPIRED` ⇒ bị hủy | Mọi attempt phải nằm trong giờ gọi: IVR từ chối `T0` ≥ 21:00:30 (24/7) / ≥ 21:05:30 (Giờ Vàng); Module 3 giữ tới 08:00 như đơn đêm | Mọi đơn COD đủ 2 cuộc; `IVR_NO_ANSWER_FINAL` luôn nghĩa là đã gọi 2 cuộc; cùng chỗ sửa với `B17` |
| `Q-23` | Token tĩnh luôn được nhận (§2.1) | Sửa ngay, fail-closed | Code khớp quyết định đã ký |
| `Q-24` | `C18`: tài nguyên server test ngoài cấp phát | Công cụ từ nay theo `/home/ssv/m8`, cổng 68xx, tiền tố `m8_`; tài nguyên cũ giữ chỉ-đọc như ngoại lệ tạm rồi dọn theo nhãn; mirror production chờ kho S5 thật | Giảm va chạm ngay, không ghim lại chuỗi `MODELS.lock` hai lần |
| `Q-25` | N1: nơi lưu ngân hàng clip | Đóng vào ảnh media Asterisk có digest | Bất biến, không cần RWX, giống lab |
| `Q-26` | N1: đơn không đọc trọn được | Fail-closed lúc quay, không tính lượt khách | Không đổi contract |
| `Q-27` | Q.850 cause 20 (máy tắt/ngoài vùng) đang bị coi là số sai | Tách: cause 20 (và 3 nếu đo thấy tương tự) ⇒ không nghe máy, còn lượt thì gọi lại | Đúng nghĩa nghiệp vụ; chốt sau khi đo nhà mạng thật |
| `Q-28` | `DispatchGate` nhánh production (§2.2) | PA2 — danh sách pilot HMAC | Rào duy nhất giữ lần gọi đầu chỉ tới số nội bộ |
| `Q-29` | N1: phạm vi đoạn động ở production quá độ | Mã đơn + tổng tiền theo PACK-09 §17.2 | Theo nguồn của Sếp; bỏ được phụ thuộc danh mục SKU, DR-12 và xử lý vùng |

---

## 4. Chỗ danh sách `25/09` lệch với code

| Mục | Danh sách ghi | Thực tế |
| --- | --- | --- |
| `C16` | "Không còn việc lõi trong tay dev" | Còn: `DispatchGate` (§2.2) |
| `N1` | Khoảng 20 giờ; khôi phục `StaticFileTtsProvider` | Khoảng 36–44 giờ. `StaticFileTtsProvider` phát **một** file cho cả cuộc theo giọng, không theo nội dung đơn (`9af20d3^:…/StaticFileTtsProvider.cs:24-35`), nên không phải cơ chế cần lấy lại. Đường phát clip động chưa từng tồn tại — `RecordedSpeechComposer` trước đây có 0 caller (message `9af20d3`) — phải viết mới |
| `N1`, `C23` | Danh mục 19 SKU | Repo ghi 20 SKU (PACK-02 `:177-179`), tên công khai còn placeholder. Kịch bản v3 lệch PACK-09 §17–18: không đọc mã đơn (biến bắt buộc), đọc món/vùng (ngoài danh sách biến được phép) — vì vậy có `Q-29` |
| `B17` | Chỉ nêu đơn 24/7 | Giờ Vàng cùng lỗi: từ chối oan `T0` 07:55:00–07:57:30; `T0` 07:57:30–07:59:59 bị gọi 2 cuộc gần nhau. Đã gộp vào việc sửa `B17` |
| `B5` | Database đã ở HEAD thì schema không phân biệt được | Suy từ code: W0105 tạo `ivr_console_accounts` với cột thứ hai là `username`, còn P03 và bản `Down` cũ tạo theo alphabet (`anonymized_at`) ⇒ `attnum = 2` phân biệt được. Chưa chạy thử; Module 8 sẽ assert ở cả hai nhánh `IT-SCHEMA-EXPAND-07` trước khi ghi vào runbook |
| `A1` bước 2 | Hạ `legal_gate` | Hash `MODELS.lock` nằm ở 7 nơi, cộng tầng thứ hai (hash của `voices.json` và template W-0122 bị ghim tiếp). Siết gate tts-provenance (bước 6) **trước** khi hạ PASS sẽ làm CI đỏ |
| Dòng 106, 123 | `IVR_ADAPTER_MODE=MOCK` dùng làm bằng chứng | Biến này, `IVR_KILL_SWITCH_ENABLED` và `IVR_LAB_DESTINATION_ALLOWLIST` không code nào đọc (grep `src` = 0); `k8s-selftest.mjs:201` cũng coi chúng là bằng chứng kill switch bật. Khoá thật là `IVR_EXECUTION_MODE`, `SIM_PROVIDER`, `Ivr:Telephony:*:Enabled` và cờ DB `globalDialKillSwitch` |
| Dòng 112 | "0 file audio trong `/app` của worker" | Audio dựng sẵn nằm ở ảnh Asterisk (`deploy/lab/asterisk/Dockerfile:74`), không ở worker — tiêu chí đúng là 0 request tới TTS provider lúc gọi |
| `C15` | Hai file compose | Còn 3 chỗ cùng lỗi: profile `LocalMockE2E` của API (đã sửa ở `W-0354`), 2 host test tích hợp và 2 script `tools/dev` (đang sửa ở lô L1) |

---

## 5. Một câu hỏi

`OD-V1-11` giữ toàn bộ dữ liệu vĩnh viễn. Vế S3 này cũng thuộc mục B2 phiếu Sếp 25/09 và cũng chưa có chữ ký Sếp
trong repo. Module 8 sửa câu "rủi ro 2 Sếp ký nhận ở S2" thành "chờ Sếp ký (phiếu 25/09 mục B2)" như danh sách dặn.
**Có mở lại cả dòng `OD-V1-11` như `OD-V1-17/18` không?** Module 8 đề xuất: mở lại, cùng lý do.

---

## 6. Toàn đã chốt `25/09` (thuộc Module 8)

| Mã | Quyết định | Hệ quả |
| --- | --- | --- |
| `Q-01` | Phát hành lại IR-07: đính chính ở cuối phiếu + đánh dấu "RÚT" ngay tại dòng `M3-13`, `M3-30` và "xem đính chính" ở `M3-02`; đầu phiếu ghi `draft.33` | Người điền bảng nhanh không thể bỏ sót lời rút theo luật im lặng của phiếu |
| `Q-02` | Đính chính `C23` theo cách trung tính: rút lời hứa VieNeu đọc lúc gọi; phạm vi phần động chờ `Q-29` | Module 3 dừng dựng dữ liệu cho việc đọc tên hàng lúc gọi mà không nhận lời hứa mới |
| `Q-03` | `C22`: vô hiệu bảng v0 ngay bằng các dòng chắc chắn (§1) | Gỡ ngay dòng khiến Module 3 hủy hàng loạt đơn 24/7 COD khi IVR gặp sự cố |
| `Q-13` | `B17`: intake xét `T0` — `T0` ngoài 08:00–21:08 là từ chối, cho cả hai chương trình | Hết từ chối oan và hết hai cuộc gọi gần liền lúc 08:00; khớp luật Module 3 giữ đơn ngoài giờ. Giữ nguyên decision và reason trên dây; OpenAPI không đổi |

---

## 7. Đã làm

| Lô | Nội dung | Trạng thái |
| --- | --- | --- |
| `W-0354` (`f3e26a6`) | Phần dev của danh sách: `C15`/`C20`, `C7`, `C9`, `C14`, `C2`, `C21` (tài liệu), `B16`, `B15`, `B11` bước 0, `B13`, `B5`, `B1` bước 1, `B9` bước 2, `A1` bước 4 và 7; tìm và sửa nguyên nhân CI hosted `openapi_lint` đỏ từ 16/09 (contract `draft.33`) | Đã commit |
| `W-0355` (`c2435b9`) | Ba lỗi soak hai worker lộ ra | Đã commit |
| L1 — `W-0356` (`a6293d2`) | Vệ sinh test: TestId trùng, test lõi thiếu TestId, test và script phụ thuộc giờ gọi | Đã commit |
| L2 — `W-0358` (`3d04eee`) | Tài liệu gửi Module 3 + sổ quyết định theo chốt 25/09 + `Q-01`, `Q-02`, `Q-03`, `Q-13` (guard giờ gọi xét `T0`, `B17`) | Đã commit |
| L3 — `W-0357` (`627dda6`) | Tài liệu nội bộ: runbook rollback §3b, capacity, README, sổ secret, inventory, runbook đường quay production | Đã commit |
| `W-0037` (`31a967c`) | Soak 25/09 dừng ở phút 199, chưa có kết luận; chạy lại đủ 240 phút từ 13:41 | Đang chạy |

Các lô trên commit ngày 25/09 và được push một lần, sau 14:42 để không chạm quý đầu của soak; sha từng việc ghi ở cột trạng thái của kế hoạch. Sau lô L2,
Toàn gửi lại IR-07 cho anh Mạnh (kèm tờ bìa `IR07_TO_BIA_M3_2026-09-25.md` chief chuyển). Tờ trình rủi ro cho Sếp
(`A1` bước 1) là `Q-08` trong kế hoạch, chưa làm.
