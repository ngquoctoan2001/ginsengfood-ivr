# Chưa xong — việc còn mở của Module 8

Cập nhật: **16/09/2026** · Thay thế 25 file kế hoạch/phiếu hỏi riêng lẻ.

Phần đã đóng nằm ở [00-DA-XONG.md](00-DA-XONG.md). Quyết định nghiệp vụ đã ký nằm ở [decisions-log.md](decisions-log.md) — file đó **không dồn được** vì tài liệu của sếp trong `docs/documents/` đang trỏ vào.

Toàn văn các file đã xóa nằm trong lịch sử git: `git log --all --full-history -- plan/ivr-orther/<tên file>`.

**Đọc theo nhóm:** A là việc chỉ cần gửi đi; B là việc đã làm xong phần mình, chờ bên khác ký; C là việc còn phải tự làm.

| Nhóm | Số mục | Ai đang chặn |
| --- | --- | --- |
| A — Phiếu hỏi chưa gửi hoặc chưa có trả lời | 7 | Legal, M3, Platform, Security |
| B — M8 đã ký phần mình, chờ bên khác | 9 | M3, Product, CRM, Legal, Security, Platform |
| C — Còn việc M8 phải làm | 6 | Chính mình, hoặc chờ dữ liệu |

**Hai kế hoạch giữ file riêng:** [đường gọi production](sip-production-dial-path-plan-2026-09-16.md) — **✅ xong cả ba việc `16/09`** (`PD-01` `W-0303`, `PD-02` `W-0308`, `PD-03` `W-0305`), ước `4–7` ngày công và hết trong `1` ngày — và [32 kênh qua nhà mạng](mobile-sip-trunk-production-32-channels-plan-2026-09-15.md) (chờ hợp đồng).

> Với việc đó, **mọi thứ trong repo không phụ thuộc bên ngoài đã hết.** Ba nhóm dưới đây đều chờ người khác trả lời: A chờ phiếu được gửi và có hồi đáp, B chờ bên kia ký, C chờ dữ liệu đo hoặc endpoint M3. Không nhóm nào mở ra bằng thêm ngày công của dev.

---

# A — Phiếu hỏi chưa gửi hoặc chưa có trả lời

Tất cả đã soạn xong, **chỉ còn việc gửi**. Không cái nào chặn việc code.

### legal-od-voice-07

**Hỏi Legal về `OD-VOICE-07` · `READY_TO_DISPATCH / NOT_SENT / EXTERNAL_RESPONSE_REQUIRED`**

Chờ Legal trả lời về ghi âm và quyền dùng giọng.

### m3-call-limit

**Hỏi M3 về giới hạn số cuộc gọi · 15/09/2026 · `READY_TO_DISPATCH / NOT_SENT`**

### m3-od18-authority

**Hỏi M3 về thẩm quyền `OD-V1-18` · ⏳ CHỜ TRẢ LỜI**

Đã gửi, chưa nhận. Quyết định `OD-V1-18` (resolver nằm trong IVR, số E.164 chỉ sống trong bộ nhớ tại biên telephony) đã ký và **không mở lại**; phiếu này hỏi phần thẩm quyền còn lại.

### platform-ci-staging

**Hỏi Platform về CI và staging · 12/09/2026 · `READY_TO_DISPATCH / NOT_SENT`**

Chặn: môi trường triển khai. Hồ sơ `W-0292` ghi deploy dev còn chặn vì thiếu kết nối cluster, staging chưa chạy.

### platform-key-source

**Hỏi Platform về nguồn khóa và định danh bản phát hành · 15/09/2026 · `READY_TO_DISPATCH / NOT_SENT`**

Chặn: `TOKEN_PROTECTOR_BLOCKED_ON_PLATFORM` — bảo vệ token production cần khóa do Platform quản lý. Đây là đầu vào của PD-01 trong [kế hoạch đường gọi production](sip-production-dial-path-plan-2026-09-16.md).

### platform-w0122

**Hỏi Platform về hạ tầng `W-0122` · `READY_TO_DISPATCH / NOT_SENT / EXTERNAL_RESPONSE_REQUIRED`**

Hạ tầng cho TTS self-hosted.

### security-w0122-cve

**Hỏi Security về xử lý CVE `W-0122` · `READY_TO_DISPATCH / NOT_SENT / EXTERNAL_RESPONSE_REQUIRED`**

---

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

**Chờ:** owner quyết A hay B; `D-06` runtime phía M3 chưa tìm thấy. Ma trận quyết định `RVK-01..RVK-12` còn mở.

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

### m8-16

**Spec ngân hàng giọng thu sẵn · 08/09/2026 · `SPEC_ONLY / NOT_APPROVED / NO_RECORDING_PERFORMED`**

`OD-V1-19` bỏ tên khách khỏi lời thoại, nên phần còn lại đều **hữu hạn và biết trước**. Template `v3-test-approved` còn ba placeholder động, cả ba phân rã được thành bộ đóng (số tiền, vùng giao, tên hàng/đơn vị).

Code ghép câu đã có: `RecordedSpeechComposer.cs`, `VietnameseNumberSpeller.cs`, `DeliveryRegionResolver.cs` (`W-0228`..`W-0244`).

**Còn thiếu:** chưa được duyệt, **chưa thu âm**.

### m8-17

**Fence thu hồi đơn · 09/09/2026 · `FENCES_IMPLEMENTED (W-0249) / ENDPOINT_PENDING`**

Hai fence đã cài xong — chi tiết ở [00-DA-XONG.md](00-DA-XONG.md#m8-17-fence-thu-hoi).

**Còn thiếu:** M3 chưa cấp endpoint thu hồi. Không tự dựng thay M3.

### today-03

**Bàn giao TTS · 29/08/2026 · `M8_LOCAL_COMPLETE / HANDOFF_READY / BLOCKED_EXTERNAL`**

Phần giọng đã chọn và manifest đã PASS — ghi ở [00-DA-XONG.md](00-DA-XONG.md#today-03-giong-doc).

**Còn thiếu:** TTS self-hosted VieNeu (`W-0122`) chờ Platform (hạ tầng) và Security (xử lý CVE) — hai phiếu ở nhóm A. Chưa phiếu nào được gửi.

### 14-risk-register

**Sổ rủi ro · LIVING**

Đã đóng: `R-02` (order_code), `R-04` (order status — `D-02` chốt `order_state`).

Còn mở, đáng chú ý:

- `R-05` — chưa thống nhất delivery/payment status. Chỉ ảnh hưởng khi mở inbound; V1 không mở nên chưa chặn.
- Trạng thái đơn không đồng bộ (race, `order_version`) — liên quan `m8-09` ở nhóm B.

Rủi ro mới phát sinh sau 16/09 ghi thẳng vào đây, mỗi mục một dòng kèm mức và người chịu.

---

## Việc gì đang chặn nhiều thứ nhất

| Bên | Đang chặn | Cách gỡ |
| --- | --- | --- |
| **Module 3** | m8-05, m8-06, m8-07, m8-09, m8-10, m8-17 | Một [phiếu 21 câu](../../integration-requirements/07-module-3-decision-sheet.md) đã soạn — chốt một lượt |
| **Platform** | token protector (PD-01), TTS, m8-15, môi trường triển khai | Hai phiếu ở nhóm A, chưa gửi |
| **Owner** | m8-09 (A hay B), m8-11 (bộ số attempt) | Hai quyết định, mỗi cái một trang |
| **Security / Legal** | m8-08, TTS CVE, OD-VOICE-07 | Ba phiếu ở nhóm A, chưa gửi |

**Việc gỡ được nhiều nhất là gửi 6 phiếu ở nhóm A.** Chúng đã soạn xong từ lâu, không tốn ngày công nào, và đang chặn phần lớn nhóm B.
