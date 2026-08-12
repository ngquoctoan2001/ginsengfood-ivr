# PROMPT P8-1 — Real SIM Adapter

## 0. Meta
| | |
| --- | --- |
| **ID** | `P8-1` · **Phase** 8 — SIM Pilot |
| **Prereq** | `P2-4`, **mua SIM Gateway (DT-01)** |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` (bật ở P9) · `IVR_ADAPTER_MODE=REAL` chỉ ở pilot env |
| **Stack** | .NET 10 · protocol SIM Gateway (điền sau khi mua) |

## 1. ROLE
Bạn là **Senior Telephony Integration Engineer**. Bạn hiện thực `ISimGateway` **REAL** theo protocol phần cứng đã mua, và xây harness **re-verify disposition** với tín hiệu telco thật để xác nhận mapping DT-02. Bạn giữ nguyên ranh giới PII (token→số chỉ trong adapter) và fail-safe.

## 2. CONTEXT
Port `ISimGateway` + mock đã có (P2-4). Sau khi **mua** SIM Gateway (DT-01 — điều kiện tiên quyết), implement REAL và **kiểm chứng lại** disposition mapping vì tín hiệu telco thật (busy/rejected/unreachable/dropped) có thể khác giả định. Đây là bước đầu của pilot — nhưng gọi khách thật vẫn **chưa** bật (chờ P9/DF-03).

## 3. SOURCE SPECS (đọc trước)
- `specs/api/04-sim-adapter-contract.md`, `specs/functional/04-call-execution-dtmf.md`, `specs/workflows/05-technical-exception.md`
- `plan/ivr-orther/decisions-log.md` §DT-01 (protocol — điền sau mua), §DT-02 (disposition re-verify), §DT-03 (DTMF), §DT-04 (cooldown/one-call/caller-id), §DT-06, §D-05 (token vault)
- `integration-requirements/03-telephony-sim-requirements.md`

## 4. DECISIONS & CONSTRAINTS
- **DT-01:** implement `ISimGateway` REAL theo protocol thật; giữ interface không đổi (core không sửa).
- **DT-02:** **re-verify** mapping với disposition code telco thật; cập nhật `DispositionMapper` nếu thực tế khác (giữ nguyên tắc technical≠no-answer, busy/rejected→no-answer, unreachable→invalid-phone).
- **DT-03/04:** DTMF RFC2833/in-band; one-sim-one-active-call; cooldown 5s; fail-count auto-disable; caller-ID/brandname (DT-06).
- **D-05:** token→số thật resolve trong adapter (token vault); không rò core/log.
- **Governance:** REAL adapter chỉ chạy ở **pilot env**, và **chưa** gọi khách thật cho tới P9 (dùng số test/loopback nội bộ trước).

## 5. INPUTS / DEPENDENCIES
- SIM Gateway đã mua: protocol/SDK, credentials, số SIM pool thật (DT-04), caller-ID (DT-06).
- Số test/loopback nội bộ cho verify trước khi gọi khách thật.

## 6. BUILD STEPS
1. Implement `RealSimGateway : ISimGateway` (`Dial/PlayScript/CaptureDtmf/GetDisposition/Health`) theo protocol thật; `SimGatewayFactory` cho phép REAL ở pilot.
2. **Token vault thật**: resolve `dial_token`→số trong adapter boundary; audit không lộ số.
3. **Disposition re-verify harness**: gọi số test/loopback với mọi kịch bản (answer+DTMF, busy, rejected, unreachable, no-answer, dropped, network error) → ghi disposition code telco thật → so với DT-02 → cập nhật `DispositionMapper` + tài liệu.
4. **One-call/cooldown/fail-count** enforce với SIM thật (nối P2-3 SimPoolManager); caller-ID cấu hình.
5. **Health** thật (gateway reachable) → readiness (fail-closed).
6. Giữ `REAL_CUSTOMER_CALL_ALLOWED=NO`: chỉ số test/nội bộ; gọi khách thật bật ở P9.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Sim/RealSimGateway.cs` | Impl REAL |
| `src/Ivr.Infrastructure/Sim/RealDialTokenResolver.cs` | Token vault thật |
| `tests/sim-verify/**` + `docs/disposition-reverify.md` | Harness + kết quả re-verify DT-02 |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-SIM-REAL-01` | integration | REAL adapter gọi số test → disposition thật thu được; DTMF 1/0 bắt đúng. |
| `IT-SIM-DISP-02` | integration | mọi disposition telco thật map đúng DT-02 (technical≠no-answer); cập nhật nếu lệch. |
| `IT-SIM-TOKEN-03` | integration | token→số chỉ trong adapter; log/core mask (D-05). |
| `IT-SIM-ONECALL-04` | integration | one-sim-one-active-call + cooldown 5s + fail-count auto-disable (DT-04). |
| `IT-SIM-HEALTH-05` | integration | gateway down → readiness 503 (fail-closed). |

Trace: `specs/testing/*` SIM, `integration-requirements/03`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] interface không đổi (core intact); [ ] DT-02 re-verified + doc; [ ] token trong adapter; [ ] one-call/cooldown; [ ] chưa gọi khách thật.
**Reviewer:** protocol đúng gateway mua; mapping delta ghi rõ; fail-safe; caller-ID chuẩn.

## 10. EVIDENCE EXPECTED
Real-call-to-test-number log, disposition re-verify table (DT-02 delta), token-boundary proof, one-call/cooldown demo, readiness fail-closed.

## 11. FORBIDDEN
- ❌ Gọi khách hàng thật (chỉ số test tới P9/DF-03). ❌ Sửa interface `ISimGateway` (core intact). ❌ Rò số thật ra core/log (D-05). ❌ Map technical→no-answer.

## 12. DEFINITION OF DONE
- [ ] RealSimGateway + token vault + DT-02 re-verify + one-call; 5 test §8 xanh (số test); evidence §10 đủ.
