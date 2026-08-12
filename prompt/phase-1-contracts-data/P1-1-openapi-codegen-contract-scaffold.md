# PROMPT P1-1 — Target/Compat OpenAPI, Codegen and Contract Scaffold

## 0. Meta

Work `W-0014` · prereq P0-1..3 · mode `MOCK` · real integration remains blocked.

## 1. Role/outcome

Bạn là Senior .NET API/Contract Engineer. Tạo typed contracts và drift gates cho IVR-owned API, Sales callback Target V1 và Golden Hour current compatibility mà không trộn semantics. Kết quả phải build/test được dù Sales API thật chưa có.

## 2. Read first

- `prompt/README-governance.md`, tracker Work W-0014;
- `plan/ivr-orther/target-contract-v1-draft.md`;
- `specs/api/00-index.md`, `05-order-core-contracts.md`;
- cả hai file `specs/api/openapi/*.yaml`;
- `integration-requirements/01-sales-platform-requirements.md`.

## 3. Constraints

- Target task supports GH+ONLINE and 24/7+COD with required flag/version/policy/dial-token/speech summary.
- Target callback path/ACK schema is authoritative for new domain/client but still `TARGET_DRAFT` externally.
- Current `/api/v1/internal/ivr/golden-hour/callbacks` gets separate DTO/client/feature flag, never target alias.
- No raw phone/full address; no notification/order transition API.

## 4. Build

1. Validate/normalize both OpenAPI documents in CI; pin hashes and generate a human-readable contract diff.
2. Generate/implement IVR server DTOs and Sales target client in `Ivr.Contracts`; isolate generated code from domain models.
3. Define explicit `CurrentGoldenHourCallback*` compatibility DTO/client from verified current Sales schema or checked fixture; mark unsupported fields clearly.
4. Add fake Sales server/WireMock mappings for both programs and ACK/error/retry scenarios.
5. Add compatibility selection through typed provider config; invalid mode/provider combination fails startup.
6. Document codegen/regeneration/deprecation. Drift does not auto-accept upstream change.

## 5. Tests/evidence

- parse/ref validation of both OAS; generated-code compile;
- task schema accepts exact two matrix rows and rejects cross-combinations/missing flag/version/speech/token;
- speech schema rejects full-address/unknown properties fixtures;
- target callback covers every 200/409/422/429/5xx response;
- target/current DTO cannot be assigned/interchanged accidentally;
- record commands, hashes, generated diff and test report in W-0014.

## 6. Forbidden/DoD

No current endpoint as final target, no fake pass, no production base URL/credential invention. Done at `TESTS_PASS` after codegen/build/tests/evidence; external Sales approval stays W-0002/W-0005/W-0006 `BLOCKED_EXTERNAL`.
