# Workflows — Index

Trạng thái: `TARGET_V1_DRAFT`.

| File | Luồng |
| --- | --- |
| [01-happy-path-confirm.md](01-happy-path-confirm.md) | speech + key 1 + target callback |
| [02-cancel.md](02-cancel.md) | key 0 signal; Sales decides cancellation |
| [03-no-answer-attempts.md](03-no-answer-attempts.md) | policy-versioned attempts + wait for Core timeout |
| [04-invalid-phone.md](04-invalid-phone.md) | invalid contact; advisory only |
| [05-technical-exception.md](05-technical-exception.md) | technical is not no-answer |
| [06-race-condition-revalidation.md](06-race-condition-revalidation.md) | version/state/blocker revalidation |
| [07-trusted-skip.md](07-trusted-skip.md) | future/feature-gated trust resolver |
| [08-capacity-hold.md](08-capacity-hold.md) | capacity incident |
| [09-state-machines.md](09-state-machines.md) | IVR lifecycle only |

All flows support GH+ONLINE and 24/7+COD where required, never transition order, never send notification, and run in MOCK/LAB/PROD modes. Candidate attempt schedule is only MOCK/LAB until owner approval.
