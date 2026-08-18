# IVR merge-request review checklist

Work `W-0038` (prompt `P5-4`) · Bổ trợ cho [`.gitlab/merge_request_templates/Default.md`](../.gitlab/merge_request_templates/Default.md)

> Cổng đỏ thì **không merge**. Không tắt analyzer, không đặt `allow_failure`, không hạ ngưỡng
> coverage để cho xanh. Nếu cổng sai, sửa cổng bằng một MR riêng có lý do — không đi vòng.

## 0. Máy đã kiểm gì rồi

Đừng tốn thời gian người vào những thứ này; chúng đã chặn ở CI:

| Kiểm tra | Cổng |
| --- | --- |
| Analyzer + code style | `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild` |
| Bí mật trong lịch sử | gitleaks (P0-2) |
| Package có lỗ hổng | `dotnet list package --vulnerable`, `npm audit` |
| Coverage | ngưỡng **80%** (`Ivr.CiPolicy coverage`) |
| PII trong evidence/artifact | `scan-pii.sh` |
| Drift OpenAPI ↔ code sinh | `openapi-contract-drift.mjs` |
| Breaking change hợp đồng | `oasdiff breaking --fail-on WARN` |
| Traceability test ↔ source | `generate-test-traceability.mjs --check` |
| 8 fail gate | `IT-FAILGATE-01..08` |
| Chính cổng review có chặn không | `review-gate-selftest.mjs` (`CT-GATE-01..04`) |

## 1. Ranh giới governance — chặn merge nếu vi phạm

- [ ] **D-02** — không có đường nào IVR ghi/chuyển trạng thái đơn. `recommended_core_action` là **advisory**.
- [ ] **D-05** — không raw phone, địa chỉ đầy đủ, `dial_token→số`, hay recording trong log/DB/UI/evidence.
- [ ] **DT-02** — lỗi kỹ thuật **không** tính là lượt gọi khách và **không** thành no-answer.
- [ ] **DO-06** — thiếu dependency/policy/evidence thì **fail closed**, không "mở cửa".
- [ ] Không thêm client/credential/webhook tới Ops hoặc CRM.
- [ ] Không thêm bề mặt gửi thông báo tới khách (V1 `DISABLED`, immutable-off).
- [ ] `IVR_ADAPTER_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO` còn nguyên, trừ khi có gate riêng đã được chấp nhận.
- [ ] Mã lỗi nằm trong catalog `specs/api/06-error-codes.md`; không tự chế mã mới.

## 2. Traceability — MASTER-05

- [ ] Work ID thật (`W-XXXX`), không phải placeholder.
- [ ] Có ít nhất một dòng mapping **đã điền**, trỏ tới `docs/evidence/W-XXXX/`.
- [ ] Residual gate nêu rõ: `NONE` / `NOT_RUN` / `BLOCKED_EXTERNAL` / `DEFERRED_TARGET`.
- [ ] Mọi checkbox đã tick — một ô governance chưa tick chính là lý do cổng tồn tại.

`mr_traceability_gate` kiểm bốn mục trên tự động. Người xét cái nó không đọc được: **dòng mapping có đúng không**, không chỉ có mặt.

## 3. Test — có phải hợp đồng hành vi không

- [ ] Mỗi thay đổi hành vi có test dương **và** âm, kèm `TestId` ổn định.
- [ ] Không test cũ nào bị **nới** để cho xanh. Nếu fixture sai thì sửa fixture, và nói rõ trong MR vì sao fixture sai chứ không phải rule sai.
- [ ] Integration chạy Postgres thật (Testcontainers), không in-memory thay thế.
- [ ] Không phụ thuộc đồng hồ máy — clock tiêm vào.

## 4. Trạng thái không được tự nâng

- [ ] Không tự chuyển work item sang `ACCEPTED` — chỉ reviewer/owner.
- [ ] Không tuyên bố `CONTRACT_LOCKED`, `PRODUCTION_READY`, `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`, `LAB_REAL_SIM_VERIFIED`.
- [ ] Không đóng external gate (`W-0002`…`W-0009`) vì đã có mock.
- [ ] Evidence nói rõ **cái gì KHÔNG được chứng minh**, không chỉ cái đã chứng minh.

## 5. Điều cần con người xét

Xem [`reviewer-guide.md`](reviewer-guide.md). Đó là phần máy khó bắt: race, tái dùng idempotency key,
độ tươi snapshot, ánh xạ taxonomy, và câu hỏi khó nhất — **assertion này có đang chứng minh điều nó tuyên bố không**.
