# PROMPT P9-1 — Production Customer-Call Release Gate

## 0. Meta

Work `W-0050` · high-risk external gate. Do not run just because P8 lab passed.

## 1. Outcome

Verify and obtain acceptance for real Sales integration, production auth/policy, 32 eSIM capacity, privacy/legal/security and operational evidence. Only an authorized release operation after all gates may set `REAL_CUSTOMER_CALL_ALLOWED=true`.

## 2. Required evidence

- Sales producer for GH+ONLINE and 24/7+COD; speech summary/dial-token; generic callback semantic ACK/version/revalidation/no-answer timeout on staging;
- production JWT/mTLS/secret rotation/network policy tests;
- owner-approved attempt policy version;
- 32 eSIM provisioning, measured concurrency/failover/caller-ID/cost and rollback;
- script/privacy/do-not-call/retention/legal approvals, recording and notification off;
- P0–P8/P10/P11 tests/evidence accepted; kill switch/cutover/rollback/on-call verified.

## 3. Execution

1. Reconcile canonical tracker: no required work/gate may be hidden by mock evidence.
2. Pin code/config/OpenAPI/provider/vendor baselines; run final staging smoke and failure drills.
3. Produce go/no-go dossier with explicit residual risks and signatures.
4. Verify technical guard refuses flip when any gate/status/evidence is missing.
5. If and only if authorized GO, promote production config, observe canary and retain immediate kill/rollback.
6. Record exact approval, timestamp, scope and post-change evidence in W-0050.

## 4. Forbidden

No global COD-only assumption; use exact two-program matrix. No customer call based on one-SIM lab, mock/Sales compat evidence, ticket text or unsigned report. No silent scope expansion.

## 5. Output artifacts
| Path | Nội dung |
| --- | --- |
| `docs/release/go-no-go-dossier.md` | Dossier go/no-go: từng gate, owner, evidence link, residual risk, chữ ký |
| `docs/release/gate-status.yaml` | **Machine-readable** trạng thái gate/evidence, do `P11-4` sinh và `P0-4` guardrail đọc để từ chối flip khi thiếu gate |
| `docs/evidence/W-0050/**` | Evidence của lần chạy gate |

## 6. Required input from P11-3
`docs/release/df03-signoff-input.md` (sinh bởi `P11-3`/`W-0059`) là **input bắt buộc**; không có nó thì không chạy được prompt này. `W-0059` phải hoàn thành trước `W-0050`.

## 7. Definition of Done
- [ ] Mọi gate trong `docs/release/gate-status.yaml` ở trạng thái accepted với evidence link thật.
- [ ] Technical guard đã được chứng minh **từ chối** flip khi thiếu bất kỳ gate/evidence nào (test âm bắt buộc).
- [ ] Dossier có chữ ký owner có thẩm quyền và ngày ký; không suy ra từ Git username.
- [ ] Chỉ Release owner chuyển `ACCEPTED`; prompt này **không** tự bật `REAL_CUSTOMER_CALL_ALLOWED`.
