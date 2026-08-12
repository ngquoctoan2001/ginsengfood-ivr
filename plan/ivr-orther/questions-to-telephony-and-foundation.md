# Câu hỏi tích hợp IVR — gửi Telephony/Infra (SIM Gateway) & Foundation

> **LỊCH SỬ — vòng hỏi/đáp 2026-07-02.** Các câu trả lời dưới đây là bản ghi của vòng đó. Nơi nào mâu thuẫn với `plan/ivr-orther/target-contract-v1-draft.md` hoặc các quyết định `TV1-*` trong `decisions-log.md` thì **TV1-* thắng** (xem `decisions-log.md` dòng 3). Cụ thể đã bị supersede: kết luận “IVR chỉ COD”, D-10 đã khóa, callback Golden Hour là target cuối, taxonomy `CALLBACK_*`, và pilot mặc định 12 SIM. Không dùng file này làm authority cho implementation.


Người gửi: Team IVR / Module 8 (IVR Order Confirmation — phase-8 / PACK-09)
Ngày gửi: 2026-07-02
Trạng thái: ✅ **Owner tự chốt phần lớn (2026-07-02)** — Foundation điền từ docs; Telephony chốt phần IVR-owned, phần hạ tầng PENDING (SIM chưa mua).

> **Tóm tắt (bản khóa: [decisions-log.md](decisions-log.md) DF-01..DF-07, DT-01..DT-06):**
> **Foundation (owner kiêm):**
> - ✅ **QF2→DF-01:** permission `IVR_*` (phase-8/11 §5) ở Permission Core. · ✅ **QF3→DF-02:** OpenAPI 3.1 `openapi/business-platform/ivr-order-confirmation.v1.yaml`.
> - ✅ **QF4→DF-03:** release gate theo phase-8/09+MASTER-05; **sign-off = Owner (bạn)+security/privacy**. · ✅ **QF5→DF-04:** idempotency/audit dùng TECH-01. · ✅ **QF6→DF-05:** correlation MASTER-03 + reuse outbox ops-core.
> - ✅ **QF1→DF-06:** allowlist = Order Core; SIM adapter no order-write; cấp `SellableCheck`/`RecallHoldView` cho Order Core. · ⏳ **QF7→DF-07:** retention — PENDING (owner+Legal).
>
> **Telephony (SIM chưa mua → adapter port + mock):**
> - ⏳ **QT1→DT-01:** adapter port (dial/play/capture/disposition/health) ✅; protocol PENDING procurement. · ✅ **QT2→DT-02:** disposition mapping LOCKED (busy/rejected→NO_ANSWER counted; unreachable/sai số→INVALID_PHONE_FINAL; SIM/audio/DTMF error→TECHNICAL_EXCEPTION) — re-verify telco khi mua.
> - ⏳ **QT3→DT-03:** DTMF RFC2833/in-band (đề xuất). · ⏳ **QT4→DT-04:** cooldown 5s/fail≥3-disable ✅; số SIM pilot 12→24-32 PENDING mua. · ✅ **QT5→DT-05:** recording OFF. · ⏳ **QT6→DT-06:** caller-ID PENDING telco.
>
> **Còn cần người/mua sắm:** DF-07 retention (Legal), DT-01/DT-04/DT-06 (mua SIM), DF-03 sign-off (khi release). Các ô chi tiết bên dưới giữ làm biên bản.

## 0. Bối cảnh

- IVR gọi outbound xác nhận Official Order qua **Internal SIM Gateway Server** (mặc định; KHÔNG Cloud IVR/SIP/brandname trừ khi owner đổi). `ONE_SIM_ONE_ACTIVE_CALL`.
- `REAL_CUSTOMER_CALL_ALLOWED = NO` cho tới khi release gate pass (smoke + evidence + security/privacy + owner sign-off).
- **Cách trả lời:** mỗi câu có *"Đề xuất từ IVR"* — chọn **[ ] Xác nhận** / **[ ] Điều chỉnh**, điền ô **Trả lời**.

Ưu tiên: **P0** chặn gọi thật · **P1** cần sớm.

---

# PHẦN 1 — TELEPHONY / INFRA (Internal SIM Gateway)

### QT1 (P0) — Protocol & adapter interface của SIM Gateway production
Cần biết phần cứng/giao thức để thiết kế SIM Adapter.

**Đề xuất từ IVR:** định nghĩa một **adapter port** (dial / play script / capture DTMF / call disposition / health) để protocol có thể thay mà không đụng core. Xin cho biết: thiết bị GSM gateway (model), điều khiển qua **AT command / SIP-to-SIM / vendor HTTP API**? Có SDK/tài liệu?

- [ ] Xác nhận (adapter port + nêu protocol) · [ ] Điều chỉnh
- **Trả lời (thiết bị + protocol + SDK):** ______________________________________________
- Người trả lời / ngày: __________

### QT2 (P0) — Bảng mapping disposition (RANH GIỚI FAIL: technical ≠ no-answer)
Đây là điểm P0 dễ sai nhất. Đề xuất bảng để xác nhận:

| Tín hiệu SIM/telco | Phân loại IVR | Counted? |
| --- | --- | --- |
| answered + DTMF 1/0 | `IVR_CONFIRMED`/`IVR_CUSTOMER_CANCELLED` | có |
| answered, hết window không bấm | `IVR_NO_ANSWER_ATTEMPT`/`WRONG_INPUT` | có |
| ring timeout / không nghe | `NO_ANSWER` | có |
| **busy (máy bận)** | ❓ đề xuất `NO_ANSWER` | có? |
| **rejected (khách từ chối cuộc gọi)** | ❓ đề xuất review/`opt-out signal` | ❓ |
| **unreachable / no network / thuê bao không tồn tại** | ❓ đề xuất `INVALID_PHONE_FINAL` hoặc technical | ❓ |
| no dial tone / SIM error / audio error / DTMF capture error / dropped mid-call | `IVR_TECHNICAL_EXCEPTION` | **KHÔNG** |

**Đề xuất từ IVR:** xác nhận 4 dòng chắc chắn; 3 dòng ❓ (busy / rejected / unreachable) cần Infra xác nhận theo hành vi telco thật.

- [ ] Xác nhận bảng (điền 3 dòng ❓) · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QT3 (P1) — DTMF capture
**Đề xuất từ IVR:** DTMF `1`/`0`; capture qua RFC2833 hoặc in-band; timeout sau khi phát script; phím sai/không bấm xử lý theo rule. Xin xác nhận phương thức capture & độ trễ.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QT4 (P1) — Capacity / health / cooldown SIM
**Đề xuất từ IVR:** `SIM_COOLDOWN_AFTER_CALL=5s`; `fail_count ≥ 3 / 10 phút` → auto-disable + alert; pilot **12 SIM**, launch **24–32**, scale 64/96 (AVG_CALL 35s, CYCLE 50s). Xin xác nhận số SIM khả dụng thực tế cho pilot + concurrency thật.

- [ ] Xác nhận · [ ] Điều chỉnh (nêu số SIM/cooldown thật)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QT5 (P1) — Ghi âm (recording) — mặc định OFF
**Đề xuất từ IVR:** recording **OFF** mặc định; chỉ bật khi có consent + legal + retention (Foundation/Legal). Nếu bật: gateway có ghi được không, lưu `recording_ref` ở đâu?

- [ ] Xác nhận OFF mặc định · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QT6 (P1) — Caller-ID hiển thị cho khách
**Đề xuất từ IVR:** hiển thị số/brandname nhất quán, đáng tin (giảm bị chặn spam). Xin cho biết số gọi ra & có brandname không.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

# PHẦN 2 — FOUNDATION (auth / RBAC / evidence / release)

### QF1 (P0) — Service identity allowlist
Chỉ Order Core (hoặc service ủy quyền) được gọi task-intake; và Order Core cần service-cred gọi ops sellable gate (DO-03).

**Đề xuất từ IVR:** dùng service token của Foundation; allowlist = Order Core service identity cho `POST .../tasks`; SIM adapter **không** có credential ghi order. Xin cho biết: cấp/validate service token thế nào, cấu hình allowlist ở đâu, và cấp perm `SellableCheck`/`RecallHoldView` cho Order Core service-cred.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF2 (P0) — RBAC permission `IVR_*`
Admin actions: `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW`.

**Đề xuất từ IVR:** tạo các permission này trong Permission Core; enforce server-side; mọi admin action có reason + audit. Xin xác nhận nơi tạo/quản permission + role mapping.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF3 (P0) — OpenAPI & vị trí contract
**Đề xuất từ IVR:** sinh **OpenAPI 3.1** `openapi/business-platform/ivr-order-confirmation.v1.yaml` (theo phase-8/11); validate trong CI. Xin xác nhận vị trí thư mục contract chuẩn của repo + tool validate.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF4 (P0) — Release gate & Evidence Registry
`REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi gate pass.

**Đề xuất từ IVR:** theo mô hình evidence/smoke/completion-gate (MASTER-05/PACK-10/TECH-10); IVR nộp evidence packet (task/attempt/result/callback/admin/security/privacy/smoke). Xin cho biết: **ai sign-off** (Release Owner), cách tích hợp Evidence Registry cho IVR, và điều kiện mở gate + phạm vi pilot.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF5 (P1) — Idempotency store & Audit sink
**Đề xuất từ IVR:** dùng idempotency store + audit log chuẩn của Foundation (TECH-01); audit append-only. Xin cho biết service/format dùng chung (để IVR không tự chế).

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF6 (P1) — Correlation & event bus / outbox
**Đề xuất từ IVR:** `X-Correlation-Id` xuyên suốt; event IVR (nếu publish) dùng outbox pattern chuẩn của repo (tái dùng như ops-core `HttpWebhookOutboxEventDispatcher`), **không** tự định nghĩa broker mới; event không thay callback. Xin xác nhận toolchain event/outbox đã được duyệt chưa.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QF7 (P1) — Retention duration (call log / DTMF / recording / audit / raw phone-token)
**Đề xuất từ IVR:** raw phone/token TTL ngắn nhất; audit theo foundation; recording (nếu bật) theo legal. Xin ai chốt số cụ thể từng loại (Foundation/Legal).

- [ ] Xác nhận owner + cho số · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

## Tổng hợp
| Câu | Chủ đề | Nhóm | Ưu tiên |
| --- | --- | --- | --- |
| QT1 | SIM protocol & adapter port | Telephony | **P0** |
| QT2 | Disposition mapping (technical≠no-answer) | Telephony | **P0** |
| QT3 | DTMF capture | Telephony | P1 |
| QT4 | Capacity/health/cooldown/SIM count | Telephony | P1 |
| QT5 | Recording (OFF mặc định) | Telephony/Legal | P1 |
| QT6 | Caller-ID/brandname | Telephony | P1 |
| QF1 | Service identity allowlist + ops service-cred | Foundation | **P0** |
| QF2 | RBAC `IVR_*` | Foundation | **P0** |
| QF3 | OpenAPI & vị trí contract | Foundation | **P0** |
| QF4 | Release gate & Evidence Registry | Foundation | **P0** |
| QF5 | Idempotency store & audit sink | Foundation | P1 |
| QF6 | Correlation & event/outbox | Foundation | P1 |
| QF7 | Retention duration | Foundation/Legal | P1 |

**Chặn gọi thật (P0):** QT1, QT2, QF1, QF2, QF3, QF4.

## Ô tổng kết
- Người duyệt Telephony/Infra: ____________ · Ngày: ______
- Người duyệt Foundation: ____________ · Ngày: ______
- Ghi chú: ______________________________________________
