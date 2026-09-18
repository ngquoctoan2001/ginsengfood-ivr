# Chưa xong — việc còn mở của Module 8

Cập nhật: **16/09/2026** · Thay thế 25 file kế hoạch/phiếu hỏi riêng lẻ.

Phần đã đóng nằm ở [00-DA-XONG.md](00-DA-XONG.md). Quyết định nghiệp vụ đã ký nằm ở [decisions-log.md](decisions-log.md) — file đó **không dồn được** vì tài liệu của sếp trong `docs/documents/` đang trỏ vào.

Toàn văn các file đã xóa nằm trong lịch sử git: `git log --all --full-history -- plan/ivr-orther/<tên file>`.

**Đọc theo nhóm:** A là việc chỉ cần gửi đi; B là việc đã làm xong phần mình, chờ bên khác ký; C là việc còn phải tự làm.

| Nhóm | Số mục | Ai đang chặn |
| --- | --- | --- |
| A — Phiếu hỏi | **`1` gửi Sếp · `1` gửi M3 · `1` chờ M3 trả lời · `3` phiếu VieNeu đã chuyển về `S2`/`S5`** | Sếp (tiền/người/rủi ro), M3 |
| B — M8 đã ký phần mình, chờ bên khác | 9 | M3, Product, CRM, Legal, Security, Platform |
| C — Còn việc M8 phải làm | 5 | Chính mình, hoặc chờ dữ liệu |

**Hai kế hoạch giữ file riêng:** [đường gọi production](sip-production-dial-path-plan-2026-09-16.md) — **✅ xong cả ba việc `16/09`** (`PD-01` `W-0303`, `PD-02` `W-0308`, `PD-03` `W-0305`), ước `4–7` ngày công và hết trong `1` ngày — và [32 kênh qua nhà mạng](mobile-sip-trunk-production-32-channels-plan-2026-09-15.md) (chờ hợp đồng).

> Với việc đó, **mọi thứ trong repo không phụ thuộc bên ngoài đã hết.** Ba nhóm dưới đây đều chờ người khác trả lời: A chờ phiếu được gửi và có hồi đáp, B chờ bên kia ký, C chờ dữ liệu đo hoặc endpoint M3. Không nhóm nào mở ra bằng thêm ngày công của dev.

---

# A — Phiếu hỏi: đã gom về đúng người `17/09` (`W-0309`)

> ## ⚠️ Mục này từng sai hai lần, sửa cả hai `17/09`
>
> **Sai thứ nhất — “chỉ còn việc gửi” trong khi không còn gì để gửi.** `W-0297` dồn `25` tài liệu
> vào index này và **xoá cả `6` phiếu**. Còn đúng `1` file trong cây, và đó là phiếu **đã gửi rồi**.
> Toàn văn `6` phiếu nằm ở `257cbef^`; `W-0309` đã khôi phục cái cần khôi phục.
>
> **Sai thứ hai — `5/6` phiếu gửi cho phòng ban không tồn tại.** Platform, Security, Legal **không
> tồn tại như các đội riêng** trong tổ chức này — chính lập luận đang dùng để bác `A1` của bản
> `16/09`. Và `m8-13` mục `1` đã cấm sẵn: *“không gửi tới một mailbox chung nếu không xác định
> được owner có thẩm quyền.”*
>
> **Tiền lệ xử lý đã có:** `OD-V1-11` đóng ngày `2026-09-10` — *“tổ chức này không có đội
> Legal/Privacy và owner chọn không mua ý kiến pháp lý ngoài cho V1; owner nhận rủi ro.”*
> `W-0309` làm đúng như thế cho phần còn lại.

| Phiếu cũ | Gửi cho | Nay ở đâu |
| --- | --- | --- |
| `m3-call-limit` | M3 | ➡️ **Nhóm `B5`** (`M3-22`…`M3-25`) của [phiếu `IR-07`](../../integration-requirements/07-module-3-decision-sheet.md). Bản rời `15/09` giữ nguyên làm bản ghi khôi phục của `W-0309`, **không gửi nữa** |
| `m3-od18-authority` | M3 | ➡️ **Nhóm `B6`** (`M3-26`…`M3-30`) của [phiếu `IR-07`](../../integration-requirements/07-module-3-decision-sheet.md). Bản rời `27/08` là **bản ghi đã gửi**, giữ nguyên (đang bị ghim hash trong `external-decision-artifacts.sha256`) |
| `platform-key-source` | Platform | ➡️ **Mục `2`** của [phiếu cho Sếp](phieu-quyet-dinh-cho-sep-2026-09-17.md) |
| `platform-ci-staging` | Platform | ➡️ **Mục `1`** (tiền) + **Mục `3`** (người duyệt thứ hai) |
| `legal-od-voice-07` | Legal | ➡️ **`S2` rủi ro `4`** — quyền dùng thương mại model VieNeu và `12` đoạn đã render |
| `platform-w0122` | Platform | ➡️ **`S5`** — máy chạy VieNeu thật + kho bản cài nội bộ |
| `security-w0122-cve` | Security | ➡️ **`S2` rủi ro `3`** — `16` lỗ hổng của bản cài nền; Lô `4` thử đổi bản cài nền trước khi ký |

### Ba phiếu VieNeu: không đóng, đã chuyển về đúng người

**VieNeu-TTS tự host là bộ đọc duy nhất** — cả production lẫn lab (`OD-V1-19`, `S4` ngày `17/09`,
`W-0122`). Ba phiếu hỏi về nó vì vậy vẫn còn đối tượng. Không tạo lại ba file gửi cho những đội không
tồn tại: câu hỏi còn sống đi về `S2` (rủi ro `3` và `4`) và `S5` (máy thật, kho bản cài nội bộ) theo
Lô `4` của [vướng mắc `17/09`](vuong-mac-va-quyet-dinh-2026-09-17.md). Toàn văn ba phiếu nằm ở `257cbef^`.

`deploy/helm/ivr/values-prod.yaml` vẫn đặt `tts.enabled: false`. Đó là trạng thái **chờ cổng**: chart
tự từ chối render sidecar khi thiếu phê duyệt, image digest, bundle model hay manifest giọng.

---

### m3-call-limit

**Hỏi M3 nghĩa của “tối đa hai lần trong 10 phút” · `15/09/2026` · ✅ `RESTORED / READY_TO_DISPATCH`**

Người nhận có thật (anh lead M3). Đã kiểm lại tại `fbe6edb`: hai lịch trong phiếu vẫn khớp
`AttemptPolicyRegistries.cs` (`0s`/`150s` và `0s`/`450s`), nên cả `3` câu còn nguyên hiệu lực.
**Không sửa một chữ nội dung.**

### m3-od18-authority

**Hỏi M3 về thẩm quyền `OD-V1-18` · ⏳ CHỜ TRẢ LỜI**

Đã gửi, chưa nhận. Quyết định `OD-V1-18` (resolver nằm trong IVR, số E.164 chỉ sống trong bộ nhớ tại
biên telephony) đã ký và **không mở lại**; phiếu này hỏi phần thẩm quyền còn lại.

⚠️ **Nặng thêm từ `16/09`:** `W-0303` đã dựng `ProductionDialTokenVault` **trên chính `OD-V1-18`** và
viện dẫn nó trong doc-comment. Đường quay số production giờ đứng hẳn về một trong ba nguồn sự thật
mà `B11` của bản `16/09` đang nêu. Xem ô `B11`.

### Việc cần Sếp quyết — `4` mục

**[phieu-quyet-dinh-cho-sep-2026-09-17.md](phieu-quyet-dinh-cho-sep-2026-09-17.md)** ·
`READY_TO_DISPATCH / NOT_SENT`

`1` chỗ chạy thử (💰) · `2` nguồn khoá mở số (💰) · `3` một người duyệt thứ hai (👤) ·
`4` quyền dùng model VieNeu và `12` đoạn đã render (⚖️) — nay là rủi ro `4` của `S2` trong [vướng mắc `17/09`](vuong-mac-va-quyet-dinh-2026-09-17.md).

Viết **không thuật ngữ**, mỗi mục có *cần gì · để làm gì · nếu không có thì sao*.

# B — M8 đã ký phần mình, chờ bên khác ký

Phần Module 8 đã chốt và ghi thành position. **Không tự đổi** khi chưa có phản hồi; vướng thì ghi ESCALATION.

### m8-05

**Hợp đồng program/result · 03/09/2026 · `M8_OWNER_SIGNED / PROGRAM_CONTRACT_LOCKED / RESULT_CONTRACT_LOCKED / M3_PRODUCT_SIGNOFF_REQUIRED / PRODUCTION_POLICY_PENDING`**

1. **Thẩm quyền:** M3 quyết định `CALL_REQUIRED`; M8 validate, thực thi, báo kết quả. M8 không phân loại khách/đơn để đảo quyết định của M3.
2. **Ma trận program:** chỉ nhận `GOLDEN_HOUR + ONLINE` và `TWENTY_FOUR_SEVEN + COD`.
3. **Wire mapping:** M3 map `24_7 → TWENTY_FOUR_SEVEN`, `PHONE_VALID → VALID`, `ELIGIBLE_FOR_IVR → ELIGIBLE` tại producer. IVR không nhận alias.
4. Result taxonomy hiện hành: **11 / 9 / 6 / 2**.

**Chờ:** M3 và Product ký + giao artifact; chính sách production chưa chốt.

### m8-06

**Trace phiên upstream · 03/09/2026 · `M8_POSITION_SIGNED / GOLDEN_HOUR_SESSION_ID_PROPOSED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED`**

1. Tên field đề xuất là **`golden_hour_session_id`**. Không dùng `session_id`, `source_session_id`, và không dùng hai alias song song.
2. Bắt buộc non-null với `GOLDEN_HOUR`; **phải vắng mặt** với `TWENTY_FOUR_SEVEN`.
3. `CapacityIncidentEntity.SessionId` hiện tại là **capacity scope ID nội bộ của IVR**, không phải field này.

**Chờ:** M3 ký contract. Chưa được phép viết code.

### m8-07

**Callback dùng chung Target V1 · 03/09/2026 · `M8_LOCAL_CALLBACK_READY / RETRY_AFTER_FIXED / ACK_MEDIA_FAIL_CLOSED_W0173 / OFFLINE_REPORT_VALIDATOR_READY_W0174 / M3_SECURITY_PLATFORM_REQUIRED / SHARED_E2E_NOT_RUN / DELIVERY_DISABLED`**

1. Target V1 là `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`, dùng cho **cả** `GOLDEN_HOUR` và `TWENTY_FOUR_SEVEN`. Endpoint Golden Hour cũ chỉ là compatibility và **không được** nhận 24/7.
2. Body là snapshot bất biến; retry giữ nguyên payload, hash, `callback_id` và `Idempotency-Key`.
3. Defect `W-0147` đã sửa.

**Chờ:** M3 + Security + Platform giao artifact; E2E chung chưa chạy; delivery vẫn tắt.

### m8-08

**Opt-out / suppression · 03/09/2026 · `M8_POSITION_SIGNED / CURRENT_LOOP_NOT_WIRED / EXPLICIT_ONLY_V1_PROPOSED / CRM_M3_LEGAL_SIGNOFF_REQUIRED / RUNTIME_NOT_AUTHORIZED`**

`C9` không phải việc "thêm một mã `IVR_OPT_OUT`". Nó là hai chiều độc lập:

1. **Inbound:** CRM/Customer Identity sở hữu registry; M3 hợp nhất vào task qua `call_restriction`. IVR **đã** chặn fail-closed khi restricted, unknown, hoặc nguồn không sẵn sàng.
2. **Outbound:** IVR quan sát tín hiệu trong cuộc gọi rồi **đề xuất** cho owner của registry — không tự ghi.

**Chờ:** CRM, M3, Legal ký. Runtime chưa được phép.

### m8-09

**Revoke / recall / freshness · 03/09/2026 · `EVIDENCE_SUBMITTED / CURRENT_OPTION_A_BEHAVIOR_PRESENT / M8_POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND / OPTION_B_NOT_IMPLEMENTED / CODE_NOT_AUTHORIZED`**

`C10 + C11 + C13` là **một bài toán lifecycle duy nhất**, không phải ba thay đổi code rời.

Hành vi hiện tại là **phương án A**: IVR validate snapshot lúc intake, sau đó vẫn tiếp tục attempt trong cửa sổ xác nhận. Callback mang `order_version_seen_by_ivr`; M3 được kỳ vọng revalidate state/version/blocker rồi có thể ACK `BLOCKED_BY_CORE` hoặc `REJECTED_STALE`.

**Phía M8 đã quyết `09/09`: phương án B** (`W-0248`). `W-0249` đã dựng hai fence thu hồi — lúc claim và ở
lần đọc task cuối trước khi quay số (phần M8 của `RVK-08`) — nhưng chúng **trơ** cho tới khi có endpoint thu
hồi, vì chưa gì đặt được `revoked_at`.

**Chờ Module 3:** chốt endpoint thu hồi (`M3-14` trong `IR-07`, `m8-17`) · `D-06` runtime phía M3 chưa tìm thấy
· phần của M3 trong ma trận `RVK-01..RVK-12`.

*Sửa `17/09` (`W-0313`): dòng này trước đó vẫn ghi "owner quyết A hay B" sau khi đã quyết. Các nhãn
`OPTION_B_NOT_IMPLEMENTED` · `CODE_NOT_AUTHORIZED` ở dòng trạng thái phía trên là của ngày `03/09`.*

### m8-10

**Contact / dial-token đường production · 03/09/2026 · `EVIDENCE_SUBMITTED / LOCAL_PRIVACY_SEAM_PRESENT / PRODUCTION_PATH_FAIL_CLOSED / CONTRACT_RUNTIME_MISMATCH_FOUND / M3_CONTACT_PRODUCER_NOT_FOUND / EXTERNAL_DECISIONS_REQUIRED / CODE_NOT_AUTHORIZED`**

`B5 + C12` phải xử lý như **một trust boundary production duy nhất**, không tách thành "viết resolver" và "viết adapter".

Seam nội bộ đã có và fail-closed: intake chặn số thô, chặn token hết hạn, chặn token không phủ hết cửa sổ xác nhận, chặn lỗi protection; DB chỉ nhận giá trị `enc:`. MOCK/LAB dùng fingerprint không đảo ngược và allowlist đích.

**Chờ:** ma trận `DTK-01..DTK-15`; M3 chưa xác định được producer của contact. **Đây là gói nền của PD-01** trong [kế hoạch đường gọi production](sip-production-dial-path-plan-2026-09-16.md).

### m8-11

**Chính sách attempt cho production · 03/09/2026 · `EVIDENCE_SUBMITTED / NUMERIC_SOURCE_CONFLICT_UNRESOLVED / CURRENT_WIRE_MISMATCH_FAIL_CLOSED / REGISTRY_LIFECYCLE_AND_RUNTIME_FLAG_DRIFT_FOUND`**

1. `mock-lab-v1` là **candidate**, không phải tên/version đổi nhãn thành production được.
2. **Chưa có bộ số production được ký.** Hai nguồn đang xung đột:
   - `D-10` + code candidate: Golden Hour `2 / [0,150] / 300s`; 24/7 `2 / [0,450] / 900s`
   - Hai tài liệu phase-8 business: Golden Hour `2 / [0,300] / 600s`; 24/7 khác

**Chờ:** owner chốt một bộ số. Không tự chọn.

### m8-12

**Định tuyến provenance quyết định ngoài · 03/09/2026 · `LOCAL_HANDOFF_PACKAGE_READY / EXTERNAL_DISPATCH_NOT_PERFORMED / EXTERNAL_APPROVAL_NOT_RECEIVED / NO_GATE_PROMOTION`**

Phần đã đóng (`S-01..S-10`) ghi ở [00-DA-XONG.md](00-DA-XONG.md#m8-12-dinh-tuyen-quyet-dinh). Còn lại: **`S-11`** route errata VoLTE/procurement vốn thiếu khỏi TODAY-01, và toàn bộ việc gửi ra ngoài.

### m8-13

**Bộ tin nhắn gửi quyết định ngoài · 03/09/2026 · `LOCAL_MESSAGE_KIT_READY / RECIPIENT_IDENTITIES_REQUIRED / EXTERNAL_DISPATCH_NOT_PERFORMED / EXTERNAL_APPROVAL_NOT_RECEIVED`**

Trước khi copy bất kỳ message nào:

1. Điền `[RECIPIENT_NAME]`, `[RECIPIENT_ROLE]`, `[CHANNEL/TICKET]`, `[REQUESTED_RESPONSE_DATE]`. **Không gửi vào mailbox chung** nếu chưa xác định được người có thẩm quyền.
2. Verify lại SHA-256 của M8-12 và manifest. Lệch thì **dừng gửi**, tạo manifest mới, cập nhật message.

### today-01

**Gói ký quyết định TODAY-01 · 29/08/2026 · `M8_OWNER_SIGNED / HANDOFF_READY / EXTERNAL_SIGNATURES_REQUIRED`**

**Không gửi nguyên trạng [Target Contract V1 Closure Pack cũ](../../docs/contracts/target-v1-closure-pack/README.md)** như nguồn hiện hành duy nhất:

- README pack cũ còn ghim baseline `main@54ca239 / draft.18`; contract nay đã lên draft.27
- [T-01](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) đã được `W-0145` sửa 03/09: Flow 04/05 khóa `24_7 + COD` và `GOLDEN_HOUR + ONLINE`

**Chờ:** chữ ký bên ngoài.

---

# C — Còn việc Module 8 phải làm

### m8-14

**Gói nhận dữ liệu hiệu chỉnh dung lượng · 03/09/2026 · `LOCAL_INTAKE_BUNDLE_READY / EXTERNAL_DATA_NOT_RECEIVED / CALIBRATION_NOT_RUN / MODEL_UNCALIBRATED / NO_GATE_PROMOTION`**

Chuẩn hóa một lần nhận dữ liệu để `B1` được hiệu chỉnh bằng số đo thật, có provenance và chữ ký. Bốn nhóm dữ liệu cần nhận:

1. `TIMING` — thời gian chiếm dụng và cooldown từng attempt, từ lab thật
2. `ARRIVAL` — arrival curve đủ chi tiết của các programme mục tiêu
3. `POLICY_OUTCOME` — attempt policy đã ký + phân phối outcome/retry thực tế
4. Nhóm thứ tư theo bundle

**Stop rule:** nhóm nào chưa nhận hoặc không đạt validation thì trạng thái vẫn là `BLOCKED_EXTERNAL / MODEL_UNCALIBRATED`.

Liên quan trực tiếp tới việc chốt số kênh đăng ký với nhà mạng — xem §3 của [báo cáo trình lãnh đạo](../../docs/reports/2026-09-15-so-sanh-mobile-sip-trunk-va-gateway-32-sim.md).

### m8-15

**Hợp đồng ledger/checkpoint dung lượng · 03/09/2026 · `EVIDENCE_SUBMITTED / CONTRACT_DRAFT_READY / PLATFORM_SECURITY_M8_SIGNATURES_REQUIRED / PROVIDER_NOT_SELECTED / CODE_NOT_AUTHORIZED / EXTERNAL_TRUST_STORE_NOT_SET`**

Phần đã dựng ghi ở [00-DA-XONG.md](00-DA-XONG.md#m8-15-so-cai-dung-luong). Còn lại: ba chữ ký, chọn provider, trust store ngoài, và quyền viết code.

### m8-17

**Fence thu hồi đơn · 09/09/2026 · `FENCES_IMPLEMENTED (W-0249) / ENDPOINT_PENDING`**

Hai fence đã cài xong — chi tiết ở [00-DA-XONG.md](00-DA-XONG.md#m8-17-fence-thu-hoi).

**Còn thiếu:** M3 chưa cấp endpoint thu hồi. Không tự dựng thay M3.

### today-03

**Bàn giao TTS · 29/08/2026 · `M8_LOCAL_COMPLETE / HANDOFF_READY / BLOCKED_EXTERNAL`**

Phần giọng đã chọn và manifest đã PASS — ghi ở [00-DA-XONG.md](00-DA-XONG.md#today-03-giong-doc).

**Cập nhật `18/09` (`W-0315`) — nhánh `W-0122` mở lại theo `S4`.** VieNeu-TTS tự host là bộ đọc
duy nhất, production lẫn lab: phần cố định của kịch bản do VieNeu render sẵn (`12` đoạn, ba giọng
Owner duyệt `28/08`), món, tổng tiền và nơi giao do sidecar tổng hợp lúc gọi. Lab không còn audio nào
khác VieNeu.

**Còn thiếu trước khi bật ở production** (Lô `4` của [vướng mắc `17/09`](vuong-mac-va-quyet-dinh-2026-09-17.md)):

- Làm được ngay: quét lại image `ivr-tts` bằng Trivy ghim · thử đổi bản cài nền cho hết lỗ
  `HIGH`/`CRITICAL` · soạn cấu hình production dạng nháp.
- Chờ: đo trên máy thật (`S5`) · người được chỉ định nghe duyệt `12` đoạn và chỗ nối (`S1`) · `6`
  cuộc gọi thử MicroSIP (`S5`) · ký rủi ro `3` và `4` (`S2`).

### 14-risk-register

**Sổ rủi ro · LIVING**

Đã đóng: `R-02` (order_code), `R-04` (order status — `D-02` chốt `order_state`).

Còn mở, đáng chú ý:

- `R-05` — chưa thống nhất delivery/payment status. Chỉ ảnh hưởng khi mở inbound; V1 không mở nên chưa chặn.
- Trạng thái đơn không đồng bộ (race, `order_version`) — liên quan `m8-09` ở nhóm B.

Rủi ro mới phát sinh sau 16/09 ghi thẳng vào đây, mỗi mục một dòng kèm mức và người chịu.

---

## Việc gì đang chặn nhiều thứ nhất

_Cập nhật `17/09` (`W-0309`). Bảng cũ gom `5` bên; thật ra chỉ có **`3`** bên tồn tại._

| Bên | Đang chặn | Cách gỡ |
| --- | --- | --- |
| **Module 3** | m8-05, m8-06, m8-07, m8-09, m8-10, m8-17, `14` mục nhóm C của bản `16/09`, cộng `W-0123` *(chờ `OD-18`)* | **Đúng một** [phiếu `IR-07`, `30` câu](../../integration-requirements/07-module-3-decision-sheet.md) — bản gộp `17/09` đã nuốt cả phiếu giới hạn số cuộc gọi (`B5`) lẫn phiếu `OD-18` (`B6`). **Không gửi phiếu nào khác nữa** |
| **Sếp** | môi trường triển khai · nguồn khoá token · người duyệt thứ hai · quyền dùng model VieNeu và `12` đoạn đã render · m8-15 | **Một** [phiếu quyết định](phieu-quyet-dinh-cho-sep-2026-09-17.md), `4` mục, viết không thuật ngữ |
| **Nhà mạng** | `B12` adapter production, `B1` hiệu chỉnh `4` số năng lực | Chờ báo giá. Đường ống đã dựng xong `16/09`, thiếu tuyến |
| ~~Platform~~ ~~Security~~ ~~Legal~~ | — | **Không tồn tại như đội riêng.** Đã gom về Sếp — xem nhóm A |
| **Chính mình** | `B8` VieNeu: phần làm được ngay của Lô `4` | Quét lại Trivy, thử đổi bản cài nền, soạn cấu hình production nháp. Không tốn tiền |

> ### Câu cũ ở chỗ này đã sai, và tôi lặp lại nó ba lần trước khi kiểm
>
> Nguyên văn: *“Việc gỡ được nhiều nhất là gửi `6` phiếu ở nhóm A. Chúng đã soạn xong từ lâu,
> không tốn ngày công nào.”* **Cả hai vế đều sai:** `6` phiếu đó đã bị `W-0297` xoá khỏi cây, và
> `5/6` gửi cho phòng ban không tồn tại. Xem nhóm A.
>
> **Câu đúng thay thế:** việc gỡ được nhiều nhất là **gửi `2` phiếu** — một cho Sếp (`4` quyết
> định tiền/người/rủi ro) và một cho M3 (`3` câu ngữ nghĩa policy). Cả hai đều đã soạn xong và
> **đang nằm trong cây**, kiểm được bằng `ls`.

