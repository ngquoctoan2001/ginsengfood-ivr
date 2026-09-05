# IVR API Versioning and Deprecation Policy

Status: `TARGET_CONTRACT_V1=DRAFT` · Applies to IVR-owned APIs and the proposed Sales callback contract.

## Contract identity

- The major wire version is present in the path (`/v1/`) and in `contract_version` where the DTO defines it.
- OpenAPI `info.version` follows SemVer.
- Current Golden Hour compatibility is a separately pinned provider contract. It is not a synonym for Target V1.

### Version string and lifecycle state are two axes (`OD-V1-02`, W-0198)

Until `1.0.0` these were conflated, and the earlier wording here — "draft suffixes do not
represent an approved production contract" — is what conflated them. A `-draft` suffix was being
used to carry two unrelated claims at once:

| Axis | Question it answers | Where it is recorded |
| --- | --- | --- |
| `info.version` | Is the contract text still moving? | the OpenAPI file |
| Lifecycle state | Has every owning team approved it? | `TARGET_CONTRACT_V1` in the tracker |

They came apart when `OD-V1-02` was signed: the IVR-owned text stopped moving, but Sales still has
not approved anything, and there is no version string that can honestly say "frozen but not
approved". So the version string stopped trying to.

**Reading the pair.** `info.version: 1.0.0` with `TARGET_CONTRACT_V1=DRAFT` — the current state —
means the contract is frozen and owner-signed on the IVR side and carries no external approval. A
non-draft version string is therefore **not** evidence of approval, and no gate, runbook or
integration document may treat it as such; the lifecycle value is the only thing that answers that
question. `APPROVED` below still requires both owning teams, unchanged.

## SemVer rules

| Change | Required handling |
| --- | --- |
| Remove/rename a path, operation, field, response, auth mechanism or semantic ACK | breaking; introduce a new major/provider contract |
| Add a required request field or tighten validation accepted by existing clients | breaking; introduce a new major/provider contract |
| Remove or narrow enum values | breaking |
| Add enum values | treat as breaking until every consumer proves unknown-value tolerance |
| Add an optional request field or additive response field with tolerant consumers | minor |
| Clarify prose/examples without wire or semantic change | patch |

The pinned `oasdiff` gate is advisory evidence plus an enforcement mechanism;
human review remains mandatory for semantic changes that a structural diff
cannot understand.

## Lifecycle

`DRAFT → APPROVED → DEPRECATED → SUNSET → REMOVED`

- `DRAFT`: non-prod fixtures and generated clients only; no production claim.
- `APPROVED`: both owning teams have approved the exact contract and auth profile.
- `DEPRECATED`: successor is documented and dual-run/migration evidence exists.
- `SUNSET`: the announced grace period has ended and calls are rejected safely.
- `REMOVED`: code, route, credentials and documentation are removed in a reviewed major change.

## Deprecation response headers

Deprecated HTTP endpoints return all of the following when technically possible:

```http
Deprecation: true
Sunset: Tue, 11 Nov 2026 00:00:00 GMT
Link: </v2/ivr/order-confirmation>; rel="successor-version"
```

The date above is illustrative only. An actual `Sunset` value must be approved
and recorded before it is emitted.

## Grace period

- Candidate default: at least 90 calendar days after the successor is approved and available in non-prod.
- Current Golden Hour compatibility has no fixed sunset date yet. Its clock starts only after Target V1, auth, sandbox, dual-run and rollback evidence are approved by Sales and IVR owners.
- A security/privacy emergency may shorten the period only with owner and security approval plus a documented consumer notification and rollback decision.

## Portal publication

The developer portal is non-production documentation. GitLab Pages publication
remains disabled until Pages Access Control is verified and
`API_DOCS_PUBLISH_NONPROD=YES` is deliberately configured. Documentation
publication never enables a provider, credential or real-customer call path.
