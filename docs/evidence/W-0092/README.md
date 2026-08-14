# W-0092 — Phase 1/2 contract and configuration remediation

Status: `TESTS_PASS`; `TARGET_CONTRACT_V1=DRAFT`

The valid contract/configuration drift findings are closed:

- IVR OpenAPI is `1.0.0-draft.2`; the old `1.0.0` baseline is retained and its
  `143`-change transition (`63 error`, `80 warning`) is archived. The current
  reviewed draft baseline compares cleanly to current source;
- CI pins the new baseline and runs byte-diff, breaking-change and negative
  controls. `CT-DOC-02` requires the exact removed-path diagnostic instead of
  matching the word `breaking` in a fixture filename;
- the portal now renders the archived transition as its twelfth generated page
  and every local link resolves;
- intake documents all emitted 400/401/403/409/422/429/500 responses; nullable
  optional API fields are omitted globally and verified by
  `IT-INTAKE-JSON-NULL-OMISSION-14`;
- duplicated internal copies of the Sales callback surface were removed from
  OpenAPI/generated models; contract tests use the canonical lifecycle model;
- API-06 includes `IVR_POLICY_BLOCKED`; `IVR_CONTACT_INVALID` has a real intake
  emission path; P2-8 operation counts describe 17 total operations and the
  14-operation P2-8 scope accurately;
- `IVR_INTERNAL_SERVICE_TOKEN` is a startup-validated secret, declared empty in
  appsettings, documented for host injection and supplied synthetically in CI.
  Development Compose does not run the API, so no unused container secret is
  invented;
- enabled Target/fake Target callback configuration rejects blank
  `TokenAudience` at startup;
- active governance prompts use `IVR_EXECUTION_MODE`, not `IVR_ADAPTER_MODE`.

Proof: OpenAPI lint 0 warnings; parse/refs `2/2`; schema negatives `12/12`;
domain negatives `13/13`; drift/hash/codegen stable; portal `12` files and link
self-test PASS; contract `21/21`; final full regression `281/281`.

The clean current diff is not Sales approval. External endpoint/auth/CDC and
consumer sign-off remain `BLOCKED_EXTERNAL` / `NOT_RUN`.
