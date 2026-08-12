# PROMPT P7-5 — Secret Rotation & Key Lifecycle

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-5` · **Phase** 7 — Deployment |
| **Work ID** | `W-0047` (canonical tracker §5) |
| **Prereq** | `P7-2`, `P4-4` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Kubernetes · Vault/KMS |

## 1. ROLE
Bạn là **Senior Security/Platform Engineer**. Bạn quản trị vòng đời secret/credential/key của IVR: rotation định kỳ và khẩn cấp, zero-downtime, và bảo vệ đặc biệt cho **credential gọi dial-token resolver** (D-05). **IVR KHÔNG giữ token-vault key hay bất kỳ mapping `dial_token→số thật` nào** — mapping nằm ở token vault/SIM adapter boundary bên ngoài IVR (D-05, `specs/data/05-pii-policy.md`, `specs/database/05-retention-and-privacy.md`). Xem `OD-V1-18`. Bạn đảm bảo lộ secret không thành thảm hoạ nhờ rotation + least-exposure.

## 2. CONTEXT
IVR giữ nhiều secret: service credential (Core/ops/CRM), DB creds, SIM gateway creds, và **credential để gọi dial-token resolver** (nhạy cảm nhất, D-05). IVR **không** giữ key giải mã mapping `dial_token→số thật`; key đó thuộc token vault ngoài IVR. Không có lifecycle rotation = rủi ro lộ kéo dài. Prompt này xây rotation + KMS/Vault lifecycle + runbook.

## 3. SOURCE SPECS (đọc trước)
- `specs/data/05-pii-policy.md`, `specs/architecture/04-deployment-architecture.md`
- `plan/ivr-orther/decisions-log.md` §D-05 (token vault), §DF-06 (service cred), §DF-07 (retention), §DT-01 (SIM creds)

## 4. DECISIONS & CONSTRAINTS
- **Zero-downtime rotation:** hỗ trợ 2 key hợp lệ trong thời gian chuyển (overlap); không rớt request đang chạy.
- **Token-vault key (D-05):** ưu tiên bảo vệ; rotation không lộ số thật; scope tối thiểu, chỉ SIM adapter truy cập.
- **Rotation định kỳ + khẩn cấp** (nghi lộ) — quy trình + tự động hoá.
- **Least-exposure:** secret không vào image/log/git (nối P0-2 gitleaks, P7-1); đọc runtime từ Vault/KMS.
- **Audit:** mọi rotation ghi audit (không log giá trị secret).

## 5. INPUTS / DEPENDENCIES
- Secret store (Vault/KMS/K8s Secret — `NEED_CONFIRMATION` prod=Vault/KMS, xem tracker `W-0063`); service cred (P4-4); **resolver credential** (P2-4/P8-1). Token vault mapping key nằm ngoài scope IVR (`OD-V1-18`).

## 6. BUILD STEPS
1. **Secret inventory + classification**: liệt kê secret, độ nhạy, chủ sở hữu, TTL rotation.
2. **Rotation mechanism**: dual-key overlap (đọc key mới+cũ trong window); rotate DB/service/SIM creds + **resolver credential** zero-downtime. Không rotate token-vault mapping key (không thuộc IVR).
3. **Vault/KMS lifecycle**: dynamic secrets nếu có; lease/renew; ExternalSecret sync vào K8s.
4. **Emergency rotation runbook**: nghi lộ → rotate + revoke + verify propagation nhanh.
5. **Audit + least-exposure**: rotation audit (không giá trị); scan secret không rò (git/image/log).
6. Test rotation (định kỳ + khẩn) không downtime.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/secret-inventory.md`, `docs/secret-rotation-runbook.md` | Inventory + runbook (định kỳ + khẩn) |
| `deploy/secrets/**` | ExternalSecret/Vault config, rotation job |
| `src/Ivr.Infrastructure/Auth/RotatingCredentialProvider.cs` | Đọc dual-key overlap |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `SEC-ROT-01` | security | rotation service/DB cred zero-downtime (dual-key overlap); request đang chạy không rớt. |
| `SEC-ROT-02` | security | rotate **resolver credential** không lộ số; SIM adapter tiếp tục hoạt động (D-05). |
| `SEC-ROT-05` | security | assert IVR **không** có cấu hình/secret nào chứa mapping `dial_token→số thật`; scan config+DB+log fail nếu có (D-05). |
| `SEC-ROT-03` | security | emergency rotation revoke key cũ → key cũ bị từ chối sau window. |
| `SEC-ROT-04` | security | secret không xuất hiện git/image/log (scan). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] zero-downtime dual-key; [ ] resolver credential bảo vệ và IVR không giữ mapping key (D-05); [ ] emergency runbook; [ ] audit không lộ giá trị; [ ] least-exposure.
**Reviewer:** revoke đúng sau overlap; scope tối thiểu; propagation verify.

## 10. EVIDENCE EXPECTED
Rotation drill log (zero-downtime), resolver-credential rotation proof, `SEC-ROT-05` no-mapping-key proof, emergency revoke test, secret-scan clean, rotation audit.

## 11. FORBIDDEN
- ❌ Rotation gây downtime/rớt request. ❌ Lộ giá trị secret trong log/audit/git/image (D-05). ❌ Token-vault key scope rộng. ❌ Không revoke key cũ sau window.

## 12. DEFINITION OF DONE
- [ ] Rotation zero-downtime + lifecycle + runbook; 4 test §8 xanh; evidence §10 đủ.
