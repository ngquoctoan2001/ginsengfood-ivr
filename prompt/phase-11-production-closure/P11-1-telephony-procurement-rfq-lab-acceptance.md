# PROMPT P11-1 — Telephony Procurement RFQ & Lab Acceptance

## 0. Meta
| | |
| --- | --- |
| **ID** | `P11-1` · **Phase** 11 — External Production Closure |
| **Prereq** | Có thể chạy song song từ `P0-1`; bắt buộc xong trước `P8-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO`; chỉ dùng số test/lab tới khi `P9-1` |
| **Stack** | Procurement package + lab verification + SIM gateway handoff |

## 1. ROLE
Bạn là **Telephony Procurement + Lab Acceptance Lead**. Bạn biến yêu cầu SIM gateway từ specs thành RFQ/ticket mua sắm, scorecard chọn vendor, và bộ acceptance lab có bằng chứng. Bạn không được giả định protocol khi chưa có vendor; mọi kết luận phải có tài liệu bàn giao hoặc log lab.

## 2. CONTEXT
Code IVR P0-P7 có thể xong với MOCK, nhưng production thật bị chặn bởi DT-01/03/04/06: gateway, DTMF mode, số SIM/capacity, caller-ID. P8-1 chỉ implement `RealSimGateway` sau khi có protocol thật. Prompt này đóng khoảng trống đó bằng artifacts mua sắm + lab handoff để P8-1 có input đủ.

## 3. SOURCE SPECS (đọc trước)
- `plan/ivr-orther/production-blockers-plan.md` §A
- `integration-requirements/03-telephony-sim-requirements.md`
- `specs/api/04-sim-adapter-contract.md`
- `specs/functional/04-call-execution-dtmf.md`, `specs/functional/06-technical-exception-capacity.md`
- `specs/testing/06-performance-test-plan.md`, `specs/testing/07-security-privacy-test-plan.md`
- `plan/ivr-orther/decisions-log.md` §DT-01/02/03/04/06, §D-05, §DF-03

## 4. DECISIONS & CONSTRAINTS
- **DT-01:** gateway phải hỗ trợ programmable `dial/play_script/capture_dtmf/report_disposition/health`.
- **DT-02:** disposition thật phải re-verify; technical không được map thành no-answer.
- **DT-03:** DTMF mode (RFC2833/in-band/vendor API) phải được vendor xác nhận.
- **DT-04:** one-sim-one-active-call, cooldown 5s, fail-count auto-disable; chốt số SIM pilot/launch bằng capacity model.
- **DT-06:** caller-ID/brandname cần đăng ký trước pilot để giảm spam/reject.
- **D-05:** token→số thật chỉ ở SIM boundary; vendor/log không được làm rò PII.

## 5. INPUTS / DEPENDENCIES
- Volume forecast từ `P10-3` nếu có; nếu chưa có, dùng giả định pilot 12 SIM, launch 24-32 SIM và đánh dấu `NEED_RECALIBRATION`.
- Vendor docs/SDK/protocol sau procurement.
- Số test nội bộ được duyệt; không dùng số khách thật.

## 6. BUILD STEPS
1. Tạo RFQ mô tả capability bắt buộc, security/PII, health/readiness, log retention, support SLA, SDK/protocol handoff, caller-ID/brandname.
2. Tạo scorecard vendor: protocol fit, DTMF reliability, disposition granularity, concurrency, observability, security, failover, cost, support.
3. Tạo lab acceptance plan: answer+DTMF `1/0`, no-answer, busy, rejected, unreachable, dropped, gateway down, network timeout, concurrent call, cooldown, fail-count disable.
4. Khi vendor/protocol có: ghi `DT-01` protocol decision record, credential/secret handoff requirements, and `RealSimGateway` implementation notes for P8-1.
5. Chạy lab với số test: capture raw vendor disposition, normalized DT-02 mapping, timing, DTMF reliability, health result, caller-ID display evidence.
6. Chốt SIM pool sizing draft: pilot, launch, burst capacity, safety margin, replacement SIM process.
7. Tạo handoff package cho P8-1: SDK/protocol docs, sandbox credentials path, test numbers, accepted/disallowed behavior, known quirks.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/procurement/ivr-sim-gateway-rfq.md` | RFQ gửi vendor/procurement |
| `docs/procurement/ivr-sim-gateway-scorecard.md` | Scorecard so sánh vendor |
| `docs/procurement/ivr-sim-lab-acceptance-plan.md` | Kịch bản lab + expected evidence |
| `docs/procurement/ivr-sim-lab-results.md` | Kết quả lab, disposition truth table, DTMF/caller-ID evidence |
| `specs/decisions/DT-01-sim-gateway-protocol.md` | Protocol/SDK được chọn, chỉ ghi khi vendor đã bàn giao |
| `specs/decisions/DT-04-sim-pool-sizing.md` | Số SIM pilot/launch và cơ sở tính |
| `specs/decisions/DT-06-caller-id-brandname.md` | Caller-ID/brandname approved |

## 8. TESTS / VERIFICATION TO RUN
| Test ID | Loại | Assert |
| --- | --- | --- |
| `PROC-SIM-RFQ-01` | review | RFQ phủ đủ dial/play/DTMF/disposition/health/security/SLA. |
| `LAB-SIM-DTMF-02` | lab | DTMF `1/0` bắt đúng trên >= 30 cuộc test, không rò số thật. |
| `LAB-SIM-DISP-03` | lab | Mọi disposition vendor map đúng DT-02 hoặc có delta doc + spec patch. |
| `LAB-SIM-CAP-04` | lab | one-sim-one-call, cooldown 5s, fail-count disable hoạt động. |
| `LAB-SIM-HEALTH-05` | lab | gateway down/timeout → health fail/readiness 503 input cho P8-1. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] RFQ đủ; [ ] scorecard có quyết định; [ ] lab results có evidence; [ ] DT-01/04/06 decision records chỉ ghi khi có proof; [ ] không dùng số khách thật.

**Reviewer:** Infra/procurement xác nhận vendor; IVR Owner xác nhận capability đủ cho P8-1; security/privacy xác nhận PII boundary.

## 10. EVIDENCE EXPECTED
Vendor protocol docs, lab logs/screenshots, raw-to-normalized disposition table, DTMF capture sample, caller-ID evidence, signed procurement acceptance.

## 11. FORBIDDEN
- ❌ Viết `RealSimGateway` dựa trên protocol đoán. ❌ Gọi khách thật trong lab. ❌ Chấp nhận gateway không có health/disposition. ❌ Lưu raw phone trong docs/logs. ❌ Ghi decision record khi chưa có evidence.

## 12. DEFINITION OF DONE
- [ ] RFQ + scorecard + lab acceptance/results + DT-01/04/06 decision records đủ; `P8-1` có input protocol thật và số test accepted.
