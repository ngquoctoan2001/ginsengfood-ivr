# PROMPT P2-4 — SIM Adapter (Port + Mock)

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-4` · **Phase** 2 — Core Runtime (mock SIM) |
| **Prereq** | `P2-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 |

## 1. ROLE
Bạn là **Senior .NET Engineer (integration/adapters)**. Bạn thiết kế một **port** trừu tượng cho SIM Gateway độc lập protocol, rồi hiện thực **mock** đọc kịch bản từ seed. Bạn giữ ranh giới PII: mapping token→số thật chỉ nằm trong adapter boundary, không rò ra IVR core.

## 2. CONTEXT
SIM Gateway **chưa mua** (DT-01), nhưng thiết kế không được chờ. Port `ISimGateway` cho phép core (scheduler P2-3) gọi `dial/play/capture/disposition/health` mà không biết phần cứng. Mock trả disposition theo `seed/call-scenarios` để chạy dry-run end-to-end. Impl REAL ở P8-1 sau khi mua.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/04-sim-adapter-contract.md`, `specs/functional/04-call-execution-dtmf.md`
- `seed/call-scenarios.sample.json`, `seed/README.md`
- `plan/ivr-orther/decisions-log.md` §DT-01 (port) · §DT-02 (disposition) · §DT-03 (DTMF) · §DT-04 (cooldown/one-call) · §D-05 (token vault)

## 4. DECISIONS & CONSTRAINTS
- **DT-01:** interface độc lập protocol: `DialAsync`, `PlayScriptAsync`, `CaptureDtmfAsync`, `GetDispositionAsync`, `HealthAsync`. `adapter_mode` config; MOCK default.
- **D-05:** `dial_token` → số thật chỉ resolve **trong adapter** (token vault boundary); IVR core chỉ giữ `dial_token`/`phone_masked`. Không log số thật.
- **DT-04:** one-SIM-one-active-call; cooldown 5s (scheduler enforce, adapter expose trạng thái).
- **MOCK:** đọc scenario theo input (order/scenario id) → trả disposition deterministic; hỗ trợ mọi nhánh (answer+1/0/9, no-answer, busy, rejected, unreachable, technical, capacity).

## 5. INPUTS / DEPENDENCIES
- `seed/call-scenarios.sample.json` (map smoke).
- Config `IVR_ADAPTER_MODE`, token vault interface (mock trả số giả từ token).

## 6. BUILD STEPS
1. Định nghĩa `ISimGateway` + DTO (`DialRequest{dialToken, scriptTemplateId, variables}`, `CallDisposition{code, dtmfKey?, durationMs, technicalError?}`) trong `Ivr.Infrastructure` (hoặc `Ivr.Contracts`).
2. `MockSimGateway : ISimGateway`: resolve scenario từ seed theo `dial_token`/order ref → trả disposition + DTMF deterministic; mô phỏng độ trễ; expose health (mock always ready trừ khi cấu hình fault-inject).
3. **Token vault boundary**: `IDialTokenResolver` — mock map token→số giả **chỉ trong adapter**; core không thấy số. Bảo đảm log adapter mask số.
4. `SimGatewayFactory` chọn impl theo `adapter_mode` (MOCK/REAL); REAL ném `NotSupported` tới P8-1.
5. Fault injection cho test (technical/capacity/dropped) để P2-5 map đúng.
6. Health `HealthAsync` nối `/health/ready` (adapter down → không dispatch, fail-closed).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Sim/ISimGateway.cs` + DTO | Port |
| `src/Ivr.Infrastructure/Sim/MockSimGateway.cs` | Mock đọc seed |
| `src/Ivr.Infrastructure/Sim/IDialTokenResolver.cs` + mock | Token vault boundary |
| `src/Ivr.Infrastructure/Sim/SimGatewayFactory.cs` | Chọn MODE |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-SIM-SCEN-01` | unit | mỗi scenario seed → disposition đúng (answer+1/0/9, no-answer, busy, unreachable, technical). |
| `UT-SIM-TOKEN-02` | unit | core không truy cập số thật; log adapter mask; token→số chỉ trong adapter (D-05). |
| `UT-SIM-MODE-03` | unit | MODE=REAL → factory ném NotSupported (chưa có SIM). |
| `UT-SIM-HEALTH-04` | unit | adapter unhealthy → `/health/ready` 503 (fail-closed). |

Trace: `specs/testing/02` (UT-SIM), `seed/call-scenarios`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] port độc lập protocol; [ ] token→số chỉ trong adapter; [ ] mock phủ mọi disposition; [ ] REAL disabled.
**Reviewer:** không rò số thật ra core/log; scenario deterministic; health nối readiness.

## 10. EVIDENCE EXPECTED
Scenario run log (mask số), token-boundary test, MODE=REAL blocked proof, health-fail → readiness 503.

## 11. FORBIDDEN
- ❌ Resolve/lưu/log số thật ngoài adapter (D-05). ❌ Impl REAL/gọi thật (P8-1, sau mua SIM). ❌ Scenario ngẫu nhiên không deterministic (khó test).

## 12. DEFINITION OF DONE
- [ ] Port + mock + token boundary + factory; 4 test §8 xanh; evidence §10 đủ.
