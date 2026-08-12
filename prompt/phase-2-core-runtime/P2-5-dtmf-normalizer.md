# PROMPT P2-5 — DTMF Normalizer

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-5` · **Phase** 2 — Core Runtime (mock SIM) |
| **Work ID** | `W-0022` (canonical tracker §5) |
| **Prereq** | `P2-4` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 |

## 1. ROLE
Bạn là **Senior .NET Engineer**. Bạn chuẩn hoá tín hiệu thô từ SIM (DTMF + disposition) thành **result taxonomy** ổn định, phân biệt rạch ròi **technical ≠ no-answer**, gắn cờ counted/final, và sinh raw_call_event + evidence. Đây là nơi "phiên dịch" tín hiệu thành ngữ nghĩa nghiệp vụ.

## 2. CONTEXT
SIM adapter (P2-4) trả disposition thô. Normalizer biến nó thành result type (`IVR_CONFIRMED/CUSTOMER_CANCELLED/NO_ANSWER_*/INVALID_PHONE_FINAL/TECHNICAL_EXCEPTION/WRONG_INPUT/CAPACITY_EXCEPTION`) theo mapping DT-02 đã khoá. Kết quả này feed callback (P2-6). Sai mapping → tính nhầm no-answer, gọi lại sai, hoặc confirm nhầm.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/05-result-normalization-callback.md`, `specs/workflows/05-technical-exception.md`, `specs/workflows/04-invalid-phone.md`
- `plan/ivr-orther/decisions-log.md` §DT-02 (disposition mapping — LOCKED) · §DS-02

## 4. DECISIONS & CONSTRAINTS
- **DT-02 (LOCKED):** answered+`1`/`0` → confirm/cancel (counted, final); answered no-key/timeout → `NO_ANSWER_ATTEMPT`/`WRONG_INPUT` (counted); ring-timeout/busy/rejected → `NO_ANSWER` (counted; rejected KHÔNG = cancel, flag review); unreachable/sai số → `INVALID_PHONE_FINAL` (**not counted**, final riêng); SIM/audio/DTMF/network/dropped → `TECHNICAL_EXCEPTION` (**not counted**); capacity → `CAPACITY_EXCEPTION` (**not counted**).
- **Technical ≠ no-answer** (P0): tuyệt đối không map technical thành no-answer.
- `9` → `WRONG_INPUT` (KEY_9 "gặp CSKH" NOT_ENABLED giai đoạn đầu — AS-07).
- Dùng `DispositionMapper` (P1-3) — 1 nguồn mapping.

## 5. INPUTS / DEPENDENCIES
- `CallDisposition` từ P2-4; `DispositionMapper` domain (P1-3).
- DB `ivr_raw_call_event`, `ivr_results`, `ivr_technical_exceptions` (P1-2); evidence store (P0-3).

## 6. BUILD STEPS
1. `ResultNormalizer.Normalize(disposition, attemptContext)` → `NormalizedResult{ResultType, IsCounted, IsFinal, Reason, DtmfKey?, TechnicalErrorCode?}` qua `DispositionMapper`.
2. Ghi `ivr_raw_call_event` (thô, mask PII) + `ivr_results`; technical → `ivr_technical_exceptions` + KHÔNG cộng customer attempt.
3. Evidence: link result ↔ raw event ↔ attempt (MASTER-05).
4. Xác định `is_final_for_ivr` (confirm/cancel/invalid-phone/no-answer-final/window-expired) để P2-6 gửi final callback.
5. Guard: nếu disposition không map được → `TECHNICAL_EXCEPTION` (fail-safe, không nuốt).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Worker/Normalization/ResultNormalizer.cs` | Normalize |
| `src/Ivr.Infrastructure/Repositories/ResultRepository.cs`, `RawEventRepository.cs` | Persist |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-NORM-01` | unit | answered+`1`→CONFIRMED counted final; `0`→CANCELLED. |
| `UT-NORM-03` | unit | busy/rejected → `NO_ANSWER` counted (rejected KHÔNG cancel, flag review). |
| `UT-NORM-04` | unit | sim/audio/dtmf/dropped → `TECHNICAL_EXCEPTION` **not counted** (technical≠no-answer). |
| `UT-NORM-05` | unit | unreachable/sai số → `INVALID_PHONE_FINAL` **not counted**, final. |
| `UT-NORM-06` | unit | `9` → `WRONG_INPUT` (KEY_9 disabled). |
| `UT-NORM-UNMAP-07` | unit | disposition lạ → `TECHNICAL_EXCEPTION` (fail-safe). |

Trace: `specs/testing/02` (UT-NORM), smoke `M8-P0-004/005`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] technical không bao giờ = no-answer; [ ] counted/final flags đúng DT-02; [ ] evidence link đủ; [ ] PII masked ở raw event.
**Reviewer:** mapping 1 nguồn (DispositionMapper); rejected≠cancel; unmapped → technical.

## 10. EVIDENCE EXPECTED
Mapping test matrix (mọi disposition), technical-not-counted proof, invalid-phone-final sample, evidence links.

## 11. FORBIDDEN
- ❌ Map technical → no-answer (P0). ❌ Cộng technical/invalid/capacity vào customer attempt. ❌ Log DTMF/số thô không mask. ❌ Tự transition order.

## 12. DEFINITION OF DONE
- [ ] Normalizer + persist + evidence; 6 test §8 xanh; evidence §10 đủ.
