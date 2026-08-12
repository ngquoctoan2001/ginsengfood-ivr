# PROMPT P10-2 — Data Governance, Backup Crypto & DR Topology

## 0. Meta
| | |
| --- | --- |
| **ID** | `P10-2` · **Phase** 10 — Compliance & Maturity |
| **Prereq** | `P1-2`, `P7-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| **Stack** | PostgreSQL · K8s · crypto/backup |

## 1. ROLE
Bạn là **Senior Data/Platform Engineer**. Bạn thiết lập quản trị dữ liệu và khả năng khôi phục: phân loại dữ liệu, mã hoá at-rest/in-transit, backup có mã hoá + kiểm chứng restore, và topology DR với RTO/RPO rõ. Bạn đảm bảo dữ liệu IVR an toàn và khôi phục được.

## 2. CONTEXT
P9-2 có DR/backup ở mức ops runbook; prompt này đi sâu về **kỹ thuật dữ liệu**: crypto, backup encryption, DR topology. Kết hợp với compliance (P10-1) để dữ liệu cá nhân được bảo vệ đúng chuẩn và khôi phục được sau thảm hoạ.

## 3. SOURCE SPECS (đọc trước)
- `specs/database/05-retention-and-privacy.md`, `specs/database/06-migration-plan.md`, `specs/architecture/04-deployment-architecture.md`, `specs/architecture/05-resilience.md`
- `plan/ivr-orther/decisions-log.md` §D-05, §DF-07 · `prompt/phase-9-release-ops/P9-2-cutover-ops-runbook.md` (DR runbook)

## 4. DECISIONS & CONSTRAINTS
- **Data classification:** phân loại (PII/audit/operational) → chính sách bảo vệ + retention (DF-07) tương ứng.
- **Crypto:** at-rest (DB/backup encrypted), in-transit (TLS/mTLS) — token-vault đặc biệt (D-05).
- **Backup encrypted + restore-tested:** backup không cứu được = vô dụng → bắt buộc restore test định kỳ.
- **DR topology:** RTO/RPO xác định; multi-AZ tối thiểu (multi-region nếu yêu cầu); failover có kịch bản.
- **Retention on backup:** backup cũng tuân retention (không giữ PII quá hạn).

## 5. INPUTS / DEPENDENCIES
- Postgres (P1-2), K8s (P7-2); backup tool (pgBackRest/WAL-G), KMS (crypto keys — nối P7-5).

## 6. BUILD STEPS
1. **Data classification** `docs/data-governance.md`: bảng dữ liệu → lớp → crypto/retention.
2. **Crypto**: DB at-rest encryption + TLS in-transit; backup encrypted (key qua KMS, rotation P7-5).
3. **Backup**: schedule (full + WAL/PITR); **restore test** tự động định kỳ (verify RTO/RPO).
4. **DR topology**: multi-AZ (min); kịch bản failover; runbook DR (bổ trợ P9-2).
5. **Retention on backup**: đảm bảo backup không giữ PII quá hạn (DF-07); purge policy.
6. Test: restore từ backup encrypted; failover drill.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/data-governance.md`, `docs/dr-topology.md` | Classification + DR |
| `deploy/backup/**` | Backup encrypted + restore-test job |
| `deploy/dr/**` | Failover config/runbook |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `DG-CRYPTO-01` | security | DB at-rest encrypted + in-transit TLS; token-vault bảo vệ (D-05). |
| `DG-BACKUP-02` | drill | restore từ backup **encrypted** thành công; RPO đạt. |
| `DG-DR-03` | drill | failover multi-AZ trong RTO; không mất dữ liệu committed. |
| `DG-RETENTION-04` | integration | backup tuân retention (không giữ PII quá hạn — DF-07). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] classification đủ; [ ] crypto at-rest/in-transit; [ ] restore tested (không chỉ backup); [ ] RTO/RPO đạt; [ ] backup tuân retention.
**Reviewer:** DR topology hợp lý; key management nối P7-5; PII bảo vệ trên backup.

## 10. EVIDENCE EXPECTED
Classification doc, crypto config, restore-test log (RPO), failover drill (RTO), backup-retention proof.

## 11. FORBIDDEN
- ❌ Backup không mã hoá / không test restore. ❌ PII plaintext at-rest. ❌ Giữ PII trên backup quá retention (DF-07). ❌ DR chỉ trên giấy.

## 12. DEFINITION OF DONE
- [ ] Governance + crypto + backup restore-tested + DR; 4 test/drill §8 pass; evidence §10 đủ.
