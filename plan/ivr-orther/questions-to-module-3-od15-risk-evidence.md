# Yêu cầu tích hợp — gửi Module 3 (`ginsengfood-business-platform`)

> **SUPERSEDED — 2026-08-27:** Toàn bộ phiếu `OD-15` này đã bị `OD-18` thay thế. Module 3
> quyết định đơn nào cần gọi; IVR chỉ thực thi và không còn yêu cầu
> `eligibility_snapshot.trust.risk_evidence_available` để quyết định call/skip. Giữ nguyên phần
> dưới làm lịch sử; authority hiện hành là
> [IR-06](../../integration-requirements/06-module-3-api-handover.md) và
> [W-0123](W-0123-m3-authoritative-call-decision-cleanup-plan.md).

**Chủ đề:** bật `OD-15` — IVR không gọi khách cũ
**Người gửi:** Team IVR / Module 8 (IVR Order Confirmation)
**Ngày gửi:** 2026-08-25 · **Trạng thái:** ⏳ CHỜ TRẢ LỜI
**Ưu tiên:** P1 — không chặn gọi thật; chưa trả lời thì IVR **gọi tất cả** (hành vi hiện tại, an toàn).

> **Đây là phiếu sign-off hẹp cho riêng `OD-15`.** Muốn xem *toàn bộ* những gì IVR cần từ Module 3 — hai chiều push, 22 field bắt buộc, callback, dial-token, auth — đọc tài liệu bàn giao: **[integration-requirements/06-module-3-api-handover.md](../../integration-requirements/06-module-3-api-handover.md)** (`OD-15` nằm ở §6 của file đó).
>
> Tài liệu này **đang hoạt động**. Khác với [questions-to-module-3-and-3.1.md](questions-to-module-3-and-3.1.md) và [questions-to-crm-3.1-followup.md](questions-to-crm-3.1-followup.md) — hai file đó là **biên bản lịch sử vòng 2026-07-02**, không dùng làm authority.

---

## 1. Bối cảnh

Owner Module 8 đã khóa **`OD-15` (2026-08-25): không gọi IVR cho khách cũ.** Xem [decisions-log.md](decisions-log.md).

Vòng hỏi trước (`QC6 → DC-06`) kết luận CRM chưa build `CustomerTrustResolver`, nên trusted-skip không khả dụng và IVR mặc định gọi tất cả. **`OD-15` bỏ hẳn ràng buộc đó.** IVR không còn cần trust score, vì khách mới **đã** được Sales báo qua `risk_flags` (`NEW_CUSTOMER`, `VERIFIED_ORDER_COUNT_0` — đúng như `seed/customers.sample.json` đang mô phỏng).

Kết quả: phần việc còn lại của Sales rút từ "build một scoring engine" xuống **một field boolean**.

## 2. Yêu cầu duy nhất (P1)

Trong payload `POST /v1/ivr/order-confirmation/tasks`, thêm vào object `eligibility_snapshot`:

```json
{
  "decision": "ELIGIBLE",
  "source_version": "sales-eligibility-v1",
  "captured_at": "2026-08-25T03:00:00Z",
  "source_available": true,
  "blockers": [],
  "trust": { "risk_evidence_available": true }
}
```

`risk_evidence_available = true` có nghĩa: **"Sales đã chạy đánh giá rủi ro cho đơn này, và `risk_flags` ở cấp task là danh sách đầy đủ."**

Không cần thêm field nào khác. `trust.resolver_version` là optional — nếu vắng, IVR lấy `source_version` cấp snapshot làm version quy trách nhiệm (giống hệt cách `voice_restriction.source_version` đang hoạt động).

- [ ] Xác nhận sẽ gửi · [ ] Điều chỉnh (xin nêu)
- **Người trả lời / ngày:** __________ · **Dự kiến release:** __________

## 3. IVR sẽ xử lý thế nào

| Sales gửi | IVR làm |
| --- | --- |
| `risk_evidence_available=true` + `risk_flags` **rỗng** | **Skip** → `TASK_SKIPPED_TRUSTED_CUSTOMER`, không tạo CallJob |
| `risk_evidence_available=true` + có risk flag | **Gọi** (advisory `RISK_FLAGS_PRESENT_REQUIRE_IVR`) |
| Không gửi / `false` | **Gọi** (advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE`) ← trạng thái hôm nay |

**Quan trọng — vì sao phải có field này thay vì chỉ nhìn `risk_flags` rỗng:** list rỗng có hai nguyên nhân không phân biệt được — *đã đánh giá, không có gì* và *chưa đánh giá bao giờ*. Nếu IVR đọc cả hai là "không rủi ro" thì đúng những đơn Sales chưa kịp đánh giá sẽ bị bỏ qua xác minh. Đó là đơn ảo lọt lưới, nên IVR fail-closed về phía **gọi**.

## 4. Hai điểm cần Sales lưu ý

### 4.1. `trusted_skip_allowed` đổi nghĩa: opt-in → **veto**

| Trước (`D-12`) | Từ `OD-15` |
| --- | --- |
| `true` = bắt buộc, thiếu thì không skip | `false` = **veto**, chặn skip cho riêng đơn đó |
| | absent / `true` = không veto |

Shape trên wire **không đổi** (vẫn `boolean` optional) nên không cần Sales sửa gì để tương thích. Nhưng nếu Sales đang gửi `false` như giá trị mặc định thì **mọi đơn sẽ bị veto và không bao giờ skip** — xin đổi thành **không gửi field** khi không có ý veto.

⚠️ **Đây là điểm dễ sai nhất trong toàn bộ thay đổi. Xin xác nhận Sales không gửi `trusted_skip_allowed=false` như default.**

- [ ] Xác nhận không gửi `false` mặc định · [ ] Đang gửi `false` mặc định → sẽ sửa
- **Người trả lời / ngày:** __________

### 4.2. `customer_trust_status` không còn được dùng

IVR vẫn nhận và lưu để audit, nhưng **không** còn tham gia quyết định skip. Sales không cần cấp `TRUSTED`. Không có việc phải làm.

## 5. Danh sách `risk_flags` — xin xác nhận vẫn đúng

`OD-15` dựa hoàn toàn vào tính đầy đủ của list này, nên nó thành contract thật chứ không còn là metadata tham khảo. Theo `phase-3.1/07 §7.1` và `D-13`:

`NEW_CUSTOMER` · `VERIFIED_ORDER_COUNT_0` · không có lịch sử mua thành công · `SUSPICIOUS_DUPLICATE` · `COD_FAIL_HISTORY` · địa chỉ giao rủi ro · phone pattern nghi ngờ · giá trị đơn bất thường · hành vi Giờ Vàng rủi ro · contact vừa mới đổi

- [ ] Xác nhận danh sách + tên mã chính xác · [ ] Bổ sung/điều chỉnh (xin nêu)
- **Ngưỡng cụ thể:** thuộc Risk Policy bên Sales (`D-13`) — IVR chỉ consume boolean, không tự định nghĩa "giá trị bất thường" bằng số tiền.
- **Người trả lời / ngày:** __________

## 6. Cách đo lúc gap đóng

Không cần Sales báo. Advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` hiện xuất hiện trên **mọi** task đủ điều kiện. Khi nó biến mất khỏi log eligibility, nghĩa là Sales đã bật field và skip đang chạy.

**Rollback phía IVR:** đặt `IVR_RETURNING_CUSTOMER_SKIP_ENABLED=NO` → quay lại gọi tất cả, không cần redeploy.

## 7. OpenAPI đã cập nhật — `draft.19`

Hợp đồng đã mang sẵn mô tả mới, Sales đọc thẳng ở đó không cần đọc file này:

- `specs/api/openapi/ivr-order-confirmation.v1.yaml` → **`1.0.0-draft.19`**
- Thêm `description` cho `customer_trust_status`, `trusted_skip_allowed` (nói rõ là **veto**), `risk_flags` (nói rõ điều kiện list rỗng), và enum `IvrTaskIntakeResult.decision`
- Portal đã build lại: [docs/api/ivr-order-confirmation-v1.html](../../docs/api/ivr-order-confirmation-v1.html)

**Tương thích:** `oasdiff breaking` vs baseline `draft.2` → **no breaking changes**. Số operation (49) và schema (93) không đổi; generated DTO chỉ thêm XML doc, không đổi kiểu field nào. Service đang chạy `draft.18` không cần deploy đồng thời.

---

## Ô tổng kết
- **Người duyệt Module 3** (`ginsengfood-business-platform`): ____________ · Ngày: ______
- **Ghi chú:** ______________________________________________
