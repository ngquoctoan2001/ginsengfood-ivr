# Production Blockers Closure Plan — IVR Order Confirmation

Trạng thái: `LIVING` · Mục tiêu: đóng **~35–40% còn lại** tới "gọi khách COD thật ở production" — phần **KHÔNG** phải code IVR (mua sắm + team khác + legal). Đây là thứ thật sự nâng % production; bộ `prompt/` lo service IVR ở P0-P10 và đã bổ sung **P11 External Production Closure** để prompt hóa RFQ/ticket/legal/sign-off/evidence.
Nguồn: `decisions-log.md`, `specs/_review/open-decisions-register.md`.

> Quy ước: **[HARD]** = chặn cứng gọi khách thật · **[SOFT]** = làm suy giảm nhưng vẫn go-live COD được (fail-safe) · **[LEGAL/PROC]** = ngoài kỹ thuật.

## A. Mua SIM Gateway (Telephony) — [HARD] [PROC]
| Item | Nội dung cần chốt | Owner | Chặn | Feed vào |
| --- | --- | --- | --- | --- |
| **DT-01** | **Protocol/SDK** SIM gateway (SIP/gateway API); khả năng `dial/play/capture DTMF/disposition/health` | Infra/Procurement + IVR Owner | `RealSimGateway` (P8-1); gọi thật | P11-1 → P8-1 |
| **DT-03** | DTMF **RFC2833 vs in-band** theo gateway | Infra | capture phím 1/0 | P11-1 → P8-1 |
| **DT-04** | **Số SIM pool**: pilot ~12 → launch **24–32** (chốt số thật); cooldown 5s, fail-count auto-disable | Procurement | capacity thật | P11-1 → P2-3/P8-1 |
| **DT-06** | **Caller-ID / brandname** đăng ký (anti-spam, tin cậy) | Telco/Procurement | trải nghiệm + tỉ lệ nghe | P11-1 → P8-1 |
| **DT-02** | **Re-verify disposition** telco thật (busy/rejected/unreachable/dropped) khi có SIM | IVR (sau mua) | chính xác no-answer vs technical | P11-1/P8-1 harness |

**Ticket đề xuất:** "Procure internal SIM gateway supporting programmable dial + DTMF capture + call disposition + health API; provision 24–32 SIM; register caller-ID/brandname." **Acceptance:** gateway gọi được số test, trả disposition + DTMF, health endpoint; SDK/protocol doc bàn giao cho P8-1.
**Lead time:** procurement + telco registration thường dài → **khởi động sớm song song với code P0–P7.**

## B. Hạng mục team khác phải build (cross-team) — phần lớn [SOFT]
> IVR chạy production COD-only được **không cần** các mục này (đã fail-safe), nhưng chúng nâng chất lượng/độ an toàn. Prompt Phase 4 đã viết code IVR-side dưới **feature-flag** — bật khi team kia xong.

| Item | Team | Nội dung | HARD/SOFT | IVR-side sẵn sàng |
| --- | --- | --- | --- | --- |
| **IR-SALES-OC1** | Order Core | Expose `order_version` + callback nhận `order_version_seen_by_ivr` → bật race-guard | SOFT (nay dùng state/COD/sellable recheck) | P11-2 → P4-1 flag `orderVersionRaceGuard` |
| **IR-SALES-OC2** | Order Core | Richer callback codes (thay vì chỉ `422`) | SOFT | P11-2 → P4-1 flag `richCallbackCodes` |
| **IR-SALES-OC3** | Order Core | Explicit no-answer/technical transition (nay order chờ `timeout→EXPIRED`) | SOFT (order vẫn tự expire) | P11-2 → P2-6/P4-1 (advisory) |
| **DC-05** | Order Core + CRM | Publish event `ORDER_CONFIRMED/CANCELLED/EXPIRED` sau Core decision + CRM notification template | SOFT (nay notification no-op) | P11-2 → P4-3/P4-5 consumer |
| **DC-06** | CRM | Build `CustomerTrustResolver` (`trusted_skip_allowed/risk_flags`) | SOFT (nay default require-IVR) | P11-2 → P4-3 flag `trustResolver` |
| **IR-CRM-01** | CRM/Customer Identity | Extend `crm-ads-eligibility` response (`do_not_call/opt_out_scope/reason/effective_at`) | SOFT (nay `eligible` đủ block cơ bản) | P11-2 → P4-3 flag `richDoNotCall` |

**Ticket đề xuất (mỗi mục):** gửi kèm `integration-requirements/01-sales-platform-requirements.md` (IR-SALES-OC*) và mục CRM (DC-05/06, IR-CRM-01). **Acceptance:** contract mới có OpenAPI + test; IVR bật flag tương ứng và pass integration test.
**Sequence:** OC1 (race-guard, ưu tiên P1) → DC-05 (notify) → IR-CRM-01 (rich DNC) → OC2/OC3 → DC-06 (trust, P3.2/optional).

## C. Legal / Sign-off — [HARD cho production] [LEGAL]
| Item | Nội dung | Owner | Chặn |
| --- | --- | --- | --- |
| **DF-07** | Retention duration từng loại (call log/DTMF/raw token/audit); recording OFF | Owner + Legal | privacy review → release |
| **DT-05** | Recording: giữ **OFF**; nếu bật cần consent + legal + retention | Owner + Legal | (nếu bật) |
| **PDPA/consent** | Cơ sở pháp lý gọi transactional (COD confirm); do-not-call registry hợp lệ | Legal | tuân thủ |
| **DF-03** | **Release sign-off** = Module 8 Owner + security/privacy review; mở `REAL_CUSTOMER_CALL_ALLOWED` | Owner + Sec/Privacy | P11-3 → go-live (P9-1) |

**Acceptance:** văn bản retention policy + legal basis ký; DF-03 checklist ký (evidence ACCEPTED) → ghi `specs/decisions/DF-03-signoff.md` (P9-1).

## D. Đường tới hạn (critical path) tới production thật
```
Song song từ đầu:
  ├─ Code IVR: prompt P0→P7  (service dev-complete, MOCK)
  ├─ [A] Mua SIM (P11-1, lead time dài) ──────┐
  ├─ [B] Team khác build OC1/DC-05/IR-CRM-01 ─┤ (P11-2, SOFT, nâng chất lượng)
  └─ [C] Legal DF-07 + PDPA (P11-3) ──────────┤
                                               ▼
   SIM về → P8-1 (real adapter) + DT-02 re-verify → P8-2 pilot (scope hạn chế, DF-03)
                                               ▼
             P11-4 readiness board + DF-03 sign-off + evidence ACCEPTED → P9-1 mở REAL_CALL → P9-2 ops
```
**Chốt quan trọng:** [A] SIM và [C] Legal là **HARD** — không có 2 cái này thì **0% gọi khách thật** dù code xong. [B] là SOFT — go-live COD được, bật dần bằng flag.

## E. % đóng góp (đối chiếu câu hỏi "bao nhiêu %")
| Nếu xong | % tới production thật |
| --- | --- |
| Chỉ code IVR (prompt P0–P9 thực thi) | ~60–65% |
| + Mua SIM [A] + verify DT-02 | +~20% → ~85% |
| + Legal/DF-03 [C] | +~10% → ~95% |
| + Cross-team [B] (chất lượng đầy đủ) | +~5% → ~100% |

→ Xác nhận: **thêm prompt code IVR không kéo % lên** quá 60–65%; P11 giúp đóng [A]+[C] bằng artifacts/evidence nhưng vẫn cần owner/vendor/legal/team khác thực hiện và ký.

## F. Việc cần bạn (Owner) khởi động NGAY (song song code)
1. **Mở procurement SIM gateway** (lead time dài nhất) — dùng spec mục A làm RFQ.
2. **Gửi 6 ticket cross-team** (mục B) kèm `integration-requirements/*`.
3. **Khởi động legal** retention + PDPA (mục C) — không chờ tới release.
