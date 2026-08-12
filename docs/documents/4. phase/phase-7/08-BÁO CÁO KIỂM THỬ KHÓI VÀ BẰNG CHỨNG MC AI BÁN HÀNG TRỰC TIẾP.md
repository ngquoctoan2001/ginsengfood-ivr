# P7.2G - MC AI Live Smoke / Evidence Report

**BACKLOG:** LIVE-BLG-010  
**MODE:** SMOKE / EVIDENCE / REPORT ONLY  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Chay smoke cho Phase 7, gom evidence, lap report 14 muc. Khong sua code, khong bat live production trong file nay.

## Entry Gate

- LIVE-BLG-001 -> LIVE-BLG-009 da co report.
- P5 Gateway evidence available.
- P4 Final Response Guard evidence available.
- P3 Commerce boundary evidence available.
- P6 Ads boundary evidence available neu dung live signal.

## Smoke Matrix Toi Thieu

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7-GSMK-001 | Known live session | Context resolved |
| P7-GSMK-002 | Unknown live session | No script |
| P7-GSMK-003 | Approved script | Guard pass |
| P7-GSMK-004 | Unapproved claim | Guard fail |
| P7-GSMK-005 | Internal/private data in script | Guard fail |
| P7-GSMK-006 | Public price comment | Route Messenger/QuoteSnapshot path |
| P7-GSMK-007 | No QuoteSnapshot | No final price |
| P7-GSMK-008 | Active recall/sale lock | No sales CTA |
| P7-GSMK-009 | Troll comment | Moderation |
| P7-GSMK-010 | Real complaint | CSKH/human |
| P7-GSMK-011 | Live signal spike | Not revenue/ROAS |
| P7-GSMK-012 | Quote created in live | Funnel signal only |
| P7-GSMK-013 | Payment WAITING | Not paid/revenue |
| P7-GSMK-014 | Runtime unavailable | Fail-safe/human handoff |

## Evidence Bat Buoc

- Live session context sample.
- Script source/version.
- Claim Guard trace.
- Comment classification examples.
- Messenger handoff evidence.
- QuoteSnapshot boundary evidence.
- Moderation/complaint evidence.
- Ads-safe live signal evidence.
- Owner review request.

## Report 14 Muc

1. Scope and environment.
2. Source-of-truth checked.
3. Phase entry evidence.
4. Live session context findings.
5. Script runtime findings.
6. Claim Guard findings.
7. Comment classifier findings.
8. Messenger coordination findings.
9. Product/price boundary findings.
10. Troll/complaint route findings.
11. Ads-safe signal findings.
12. Smoke result matrix.
13. Blocker/risk register.
14. Owner decision request.

## Final Status

**PHASE 7 VALIDATION REPORT ONLY**  
**EVIDENCE SUBMITTED ONLY**  
**NOT LIVE READY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Smoke / Evidence Gate

### 9.1. Verdict vocabulary

Allowed verdicts:

| Verdict | Meaning |
|---|---|
| `NOT_RUN` | Smoke chưa chạy. |
| `PASS_LOCAL` | Smoke local pass nhưng chưa đủ accepted evidence. |
| `FAIL` | Có lỗi hoặc P0 blocker. |
| `BLOCKED_BY_DEPENDENCY` | Thiếu Gateway/Commerce/Ads/Ops/AI evidence. |
| `EVIDENCE_SUBMITTED` | Đã nộp evidence, chưa accepted. |
| `EVIDENCE_ACCEPTED` | Evidence accepted cho scope smoke cụ thể. |

Forbidden verdicts trong file này nếu thiếu accepted evidence + owner decision:

- `LIVE_READY`
- `RELEASE_READY`
- `PRODUCTION_READY`
- `GLOBAL_SMOKE_PASS`
- `COMPLETION_PASS`
- `GATEWAY_PASS`

### 9.2. Expanded P0 smoke matrix

| Smoke ID | Area | Scenario | Expected |
|---|---|---|---|
| P7-GSMK-001 | Session | Known live session + valid live plan | Context resolved, no direct send. |
| P7-GSMK-002 | Session | Unknown live session | No script/cue/handoff/measurement. |
| P7-GSMK-003 | Plan | Missing LivePlan | Hold, owner/operator review. |
| P7-GSMK-004 | Product scope | SKU outside live plan | Host cue blocked. |
| P7-GSMK-005 | Sellable | SKU not sellable | Sales cue blocked. |
| P7-GSMK-006 | Sale lock | Sale lock active | No sales CTA, risk cue. |
| P7-GSMK-007 | Recall | Recall active | No sales CTA, risk cue. |
| P7-GSMK-008 | Script | Approved product story | Host cue allowed only after guards. |
| P7-GSMK-009 | Claim | Medical treatment claim | Guard fail, no cue. |
| P7-GSMK-010 | Privacy | Internal formula/BOM in script | Guard fail, no cue. |
| P7-GSMK-011 | Urgency | Fake scarcity/urgency | Guard fail, risk cue. |
| P7-GSMK-012 | Public price | Comment asks price | Public-safe ack + Gateway private route; no final price. |
| P7-GSMK-013 | Buy intent | Comment says buy/chốt | No public order; Gateway/Commerce path only. |
| P7-GSMK-014 | Handoff | Gateway handoff success | Public ack allowed only with delivery evidence. |
| P7-GSMK-015 | Handoff | Gateway handoff fail | No false "đã gửi Messenger". |
| P7-GSMK-016 | Duplicate | Duplicate comment/webhook | No duplicate cue/handoff/order. |
| P7-GSMK-017 | Quote | No QuoteSnapshot | No final price. |
| P7-GSMK-018 | Quote | Expired QuoteSnapshot | Refresh required; no order. |
| P7-GSMK-019 | Order | Public "chốt 3 hộp" | No official order public. |
| P7-GSMK-020 | Payment | Transfer image/comment | Not PAID, not revenue. |
| P7-GSMK-021 | Complaint | Quality complaint | CSKH/Quality/human, no sales cue. |
| P7-GSMK-022 | Health risk | Adverse event/health-sensitive | Human/Quality P0, sales suppressed. |
| P7-GSMK-023 | Abuse | Heavy abuse | No public debate, moderation. |
| P7-GSMK-024 | Malicious | Spam/phishing link | Quarantine/moderation, no Messenger sales. |
| P7-GSMK-025 | Ads | Live comment spike | Signal only, not revenue/ROAS. |
| P7-GSMK-026 | Ads | Live quote created | Funnel only, not revenue. |
| P7-GSMK-027 | Ads | Verified revenue later linked | Revenue still owned by Commerce/Ads. |
| P7-GSMK-028 | Ads | Data quality fail | No scale suggestion. |
| P7-GSMK-029 | Runtime | Gateway unavailable | Fail-safe, no public/private action. |
| P7-GSMK-030 | Runtime | Commerce unavailable | No price/order cue. |
| P7-GSMK-031 | Runtime | ClaimGuard unavailable | No product/claim cue. |
| P7-GSMK-032 | Evidence | Evidence missing | No completion/pass/release claim. |
| P7-GSMK-033 | Owner | Owner sign-off missing | No release/live/go-live claim. |
| P7-GSMK-034 | Global gateway | Global Gateway blocked | No production/go-live. |

### 9.3. Evidence package schema

```yaml
phase7_evidence_package:
  package_id: string
  phase: P7
  environment: LOCAL | TEST | STAGING | PRODUCTION
  generated_at: datetime
  source_docs_checked:
    - PACK-08
    - TECH-08
    - TECH-06
    - TECH-07
    - TECH-04
    - TECH-10
  smoke_results:
    - smoke_id: string
      verdict: NOT_RUN | PASS_LOCAL | FAIL | BLOCKED_BY_DEPENDENCY | EVIDENCE_SUBMITTED | EVIDENCE_ACCEPTED
      artifacts: []
      blocker_reason: string?
  required_artifacts:
    - live_plan_sample
    - host_cue_sample
    - live_risk_cue_sample
    - private_handoff_cue_sample
    - comment_to_messenger_handoff_sample
    - live_measurement_event_sample
    - quote_snapshot_boundary_trace
    - sale_lock_recall_trace
    - claim_guard_trace
    - gateway_delivery_log
    - ads_verified_revenue_boundary_trace
  final_verdict:
    live_ready: false
    release_ready: false
    production_ready: false
    global_gateway: BLOCKED
```

### 9.4. Artifact rules

| Artifact | Rule |
|---|---|
| JSON payload | Must be schema-valid or marked `CONTRACT_MISSING`. |
| Guard trace | Must include checked rules, pass/fail, blocked actions. |
| Resolver trace | Must include input summary, output summary, fail reason. |
| Screenshot | Allowed only as UI supplement, never sole evidence. |
| PII | Must be masked. |
| Token/secret | Forbidden. |
| Owner note | Must reference evidence package ID. |

### 9.5. Report 14 mục - required detail

Each section in the report must contain:

1. Scope and environment.
2. Source-of-truth checked with file paths.
3. Phase entry evidence and missing evidence.
4. Live session context findings.
5. Live plan/product scope findings.
6. Script runtime and host cue findings.
7. Claim/privacy/fake urgency guard findings.
8. Comment classifier findings.
9. Messenger/Gateway coordination findings.
10. Commerce price/order/payment boundary findings.
11. Moderation/complaint/risk findings.
12. Ads-safe live signal findings.
13. Smoke result matrix.
14. Blocker/risk/owner decision register.

### 9.6. Release wording lock

Final report must end with:

```yaml
phase_7_status: VALIDATION_REPORT_ONLY
evidence_status: EVIDENCE_SUBMITTED_OR_PENDING
live_ready: false
release_ready: false
production_ready: false
global_gateway: BLOCKED
```

Only after accepted evidence, P0 cleared, owner sign-off, release decision, and global gateway approval may a separate release/go-live document change this wording.

