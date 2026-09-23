import assert from "node:assert/strict";
import { collectTestEvidence, gateRegistryErrors, TEST_PLAN_SCHEMA } from "./acceptance-test-plan.mjs";

// Invoked by the existing acceptance-batches --self-test gate; no separate optional runner.
export function c2SelfTest({ citedTestIds, traceability, judge, indexResults, testVerdict }) {
  const trace = traceability("| `UT-X` | 1 |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n");
  assert.deepEqual(citedTestIds("UT-UI-RBAC-01 E2E-UI-AUTH-05 UT-ELIG-TRUST-03", trace),
    ["E2E-UI-AUTH-05", "UT-ELIG-TRUST-03", "UT-UI-RBAC-01"], "deleted test families must not disappear");
  const input = { evidencePath: "docs/evidence/W-0010/README.md", evidenceText: "REAL_CUSTOMER_CALL_ALLOWED=NO",
    trace, results: indexResults([{ className: "XTests", name: "First", outcome: "Passed" }]),
    sweep: { ok: true, reason: "full sweep" }, runCheck: { ok: true, reason: "bound run" } };
  const row = { id: "W-0010", prompt: "P2-1", status: "TESTS_PASS", residual: "" };
  assert.equal(judge(row, input).c2.ok, false, "zero cited tests cannot pass C2");
  const duplicate = traceability("| `UT-X` | 2 |\n| `UT-X-01` | unit | `Missing` | `tests/XTests.cs` |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n");
  assert.equal(testVerdict("UT-X-01", duplicate, input.results).ok, false,
    "every definition of a TestId must run, not just the last traceability row");
  let checks = 3;
  const slashTrace = traceability("| `UT-X` | 3 |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n| `UT-X-02` | unit | `Second` | `tests/XTests.cs` |\n| `UT-X-03` | unit | `Third` | `tests/XTests.cs` |\n");
  assert.deepEqual(citedTestIds("IT-API-TERMINATE-09/10", trace),
    ["IT-API-TERMINATE-09", "IT-API-TERMINATE-10"], "W-0207 shorthand must retain its second test"); checks += 1;
  assert.deepEqual(citedTestIds("UT-X-01/3/01", trace), ["UT-X-01", "UT-X-03"],
    "slash is a list, not a range; preserve width and deduplicate"); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06/06b/06d/06e/06f", trace),
    ["CT-CI-06", "CT-CI-06b", "CT-CI-06d", "CT-CI-06e", "CT-CI-06f"]); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06..06h", trace),
    ["CT-CI-06", "CT-CI-06b", "CT-CI-06c", "CT-CI-06d", "CT-CI-06e", "CT-CI-06f", "CT-CI-06g", "CT-CI-06h"],
    "named shell assertion ranges use the fixed runner registry; the first assertion has no a suffix"); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06b…06d/06h", trace),
    ["CT-CI-06b", "CT-CI-06c", "CT-CI-06d", "CT-CI-06h"]); checks += 1;
  for (const unknownRange of ["CT-CI-06a..06h", "CT-CI-06..06z", "UT-X-01..01h"]) {
    assert.throws(() => citedTestIds(unknownRange, trace), /invalid TestId/u,
      "named assertion ranges require both endpoints in the same fixed runner registry"); checks += 1;
  }
  assert.deepEqual(citedTestIds("UT-X-01/03..05/07", trace),
    ["UT-X-01", "UT-X-03", "UT-X-04", "UT-X-05", "UT-X-07"]); checks += 1;
  assert.deepEqual(citedTestIds("UT-X-01..03/05", trace),
    ["UT-X-01", "UT-X-02", "UT-X-03", "UT-X-05"]); checks += 1;
  assert.deepEqual(citedTestIds("W-0010/11 OD-V1-20/21 UT-X-01/IT-NEW-02", trace),
    ["IT-NEW-02", "UT-X-01"], "full IDs and non-test slash notation remain distinct"); checks += 1;
  for (const malformed of ["UT-X-01/02bad", "UT-X-01/03..02", "UT-X-01/03..9999", "UT-X-01/02b..04"]) {
    assert.throws(() => citedTestIds(malformed, trace), /invalid TestId/u,
      "a malformed continuation must not certify a valid prefix"); checks += 1;
  }
  const slashInput = { ...input, trace: slashTrace, evidenceText: `${input.evidenceText}\nUT-X-01/02` };
  assert.equal(judge(row, slashInput).c2.ok, false, "missing slash test results must reject C2"); checks += 1;
  for (const outcome of ["Failed", "NotExecuted"]) {
    assert.equal(judge(row, { ...slashInput, results: indexResults([
      { className: "XTests", name: "First", outcome: "Passed" },
      { className: "XTests", name: "Second", outcome },
    ]) }).c2.ok, false, `${outcome} slash test must reject C2`); checks += 1;
  }
  assert.equal(judge(row, { ...slashInput, results: indexResults([
    { className: "XTests", name: "First", outcome: "Passed" },
    { className: "XTests", name: "Second", outcome: "Passed" },
  ]) }).c2.ok, true, "all slash tests green can satisfy C2"); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nUT-X-01/99` }).c2.ok, false,
    "a slash test absent from traceability must reject C2"); checks += 1;
  assert.throws(() => citedTestIds("UT-X-01..9999", trace), /invalid TestId range/u); checks += 1;
  assert.throws(() => citedTestIds("UT-X-03..01", trace), /invalid TestId range/u); checks += 1;
  assert.deepEqual(citedTestIds("W-0010 OD-V1-20 M3-14 A-0663 UT-NEW-FAMILY-01", trace), ["UT-NEW-FAMILY-01"]); checks += 1;
  assert.equal(testVerdict("UT-X-01", trace, indexResults([{ className: "OtherTests", name: "First", outcome: "Passed" }])).ok,
    false, "a same-named method from a different source class cannot satisfy the test"); checks += 1;
  const documents = new Map([
    ["docs/evidence/W-0010/test-report.md", "UT-X-01"],
  ]);
  const collect = (overrides = {}) => collectTestEvidence({ workId: row.id, evidencePath: input.evidencePath,
    evidenceText: `${input.evidenceText}\n[tests](test-report.md)`, trace, extract: citedTestIds,
    read: (file) => documents.get(file) ?? null, ...overrides });
  const linked = collect();
  assert.deepEqual(linked.ids, ["UT-X-01"]);
  assert.equal(judge(row, { ...input, testEvidence: linked }).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: collect({ evidenceText: `${input.evidenceText}\n[tests](absent.md)` }) }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: collect({ promptText: "UT-UI-RBAC-01" }) }).c2.ok, false,
    "backend-only evidence cannot satisfy mandatory UI tests in the prompt"); checks += 1;
  documents.set("docs/evidence/W-0010/test-report.md", "[cycle](README.md)\nUT-X-01");
  assert.equal(collect().documents.length, 2, "cyclic attachments are read once"); checks += 1;
  const planPath = "docs/evidence/W-0010/acceptance-tests.json";
  const plan = { schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", requiredTestIds: ["UT-X-01"],
    retirements: [{ testId: "UT-UI-OLD-01", reason: "Owner retired the old behavior in a recorded decision.",
      decision: { commit: "a".repeat(40), path: "docs/evidence/W-0099/README.md", sha256: "b".repeat(64) },
      replacementTestIds: ["UT-X-01"] }] };
  const save = (value) => documents.set(planPath, JSON.stringify(value));
  save(plan);
  let validated = 0;
  const historical = { evidenceText: `${input.evidenceText}\nUT-UI-OLD-01`, validateDecision: () => { validated += 1; } };
  const replaced = collect(historical);
  assert.equal(validated, 1);
  assert.deepEqual(replaced.retired, ["UT-UI-OLD-01"]);
  assert.equal(judge(row, { ...input, testEvidence: replaced }).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: replaced,
    results: indexResults([{ className: "XTests", name: "First", outcome: "Failed" }]) }).c2.ok, false); checks += 1;
  assert(collect({ ...historical, validateDecision: () => { throw Error("decision hash mismatch"); } }).errors.length); checks += 1;
  assert(collect({ ...historical, validateDecision: undefined }).errors.length); checks += 1;
  save({ ...plan, retirements: [{ ...plan.retirements[0], replacementTestIds: ["UT-X-99"] }] });
  assert(collect(historical).errors.length); checks += 1;
  save({ ...plan, retirements: [{ ...plan.retirements[0], testId: "UT-X-01" }] });
  assert(collect({ ...historical, evidenceText: "UT-X-01" }).errors.length); checks += 1;
  save({ ...plan, retirements: [plan.retirements[0], plan.retirements[0]] });
  assert(collect(historical).errors.length); checks += 1;
  save({ ...plan, workId: "W-9999" }); assert(collect(historical).errors.length); checks += 1;
  documents.set(planPath, "{broken"); assert(collect(historical).errors.length); checks += 1;
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "retired-ui", decision: plan.retirements[0].decision,
    proposedDisposition: "CANCELLED" });
  const retiredUi = collect(historical);
  assert.match(retiredUi.errors[0], /chờ Toàn/u);
  assert.equal(judge(row, { ...input, testEvidence: retiredUi }).c2.ok, false,
    "a valid retirement is a closeout proposal, never UI acceptance"); checks += 1;
  const gateEvidence = { ...input, evidenceText: `${input.evidenceText}\nCT-CI-06`,
    gateManifest: { gates: { "selftest-pii.sh": { argv: ["sh"] } } } };
  assert.equal(judge(row, gateEvidence).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...gateEvidence, gateManifest: { gates: {} } }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...gateEvidence, sweep: { ok: false } }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCT-CI-999` }).c2.ok, false); checks += 1;

  // W-0346: registered runner assertions, plain and conditional, and the rules the registry obeys.
  const capacity = { gates: { "capacity-selftest.mjs": { argv: ["--self-test"] } } };
  const plain = judge(row, { ...input, evidenceText: `${input.evidenceText}\nCT-CI-05`,
    gateManifest: { gates: { "ci-config-selftest.mjs": { argv: ["--self-test"] } } } });
  assert.equal(plain.c2.ok, true);
  assert.deepEqual(plain.conditional, []);
  assert.equal(plain.verdict, "ĐẠT", "a plain runner PASS with nothing left to read passes"); checks += 1;
  const conditional = judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03`, gateManifest: capacity });
  assert.equal(conditional.c2.ok, true);
  assert.equal(conditional.verdict, "XEM", "a conditional runner PASS is shown to the owner, never passed silently");
  assert.match(conditional.conditional.join("; "), /^CAP-CALIB-03 PASS_UNCALIBRATED/u); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03`, gateManifest: { gates: {} } }).c2.ok,
    false, "a conditional PASS still needs its runner in the sweep"); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03 UT-X-99`, gateManifest: capacity })
    .verdict, "KHÔNG ĐẠT", "a condition never softens another failure"); checks += 1;

  const runner = [
    'process.stdout.write("UT-Z-01 PASS — plain\\n");',
    'process.stdout.write(`UT-Z-02 ${ok ? "PASS_A" : "PASS_B"} — conditional\\n`);',
    "// UT-Z-03 PASS — a comment is not a result",
    'process.stdout.write("UT-Z-04b PASS — a longer ID does not print UT-Z-04\\n");',
  ].join("\n");
  const registry = (overrides) => gateRegistryErrors({
    manifest: { gates: { "z.mjs": { argv: [] }, "off.mjs": { sweepable: false } } },
    traced: new Map([["UT-Z-09", []]]), source: (file) => (file === "z.mjs" ? runner : null), wholeGate: [],
    registry: { "UT-Z-01": "z.mjs", "UT-Z-02": "z.mjs" }, conditions: { "UT-Z-02": "PASS_A hoặc PASS_B" }, ...overrides });
  assert.deepEqual(registry({}), []); checks += 1;
  for (const [overrides, rejected] of [
    [{ registry: { "UT-Z-01": "off.mjs" }, conditions: {} }, /không chạy trong full sweep/u],
    [{ registry: { "UT-Z-09": "z.mjs" }, conditions: {} }, /test \.NET/u],
    [{ registry: { "UT-Z-03": "z.mjs" }, conditions: {} }, /không in "UT-Z-03 PASS"/u],
    [{ registry: { "UT-Z-04": "z.mjs" }, conditions: {} }, /không in "UT-Z-04 PASS"/u],
    [{ registry: { "UT-Z-02": "z.mjs" }, conditions: {} }, /có điều kiện .* chưa khai báo/u],
    [{ registry: { "UT-Z-01": "z.mjs" }, conditions: { "UT-Z-01": "x" } }, /chỉ in PASS/u],
    [{ registry: { "UT-Z-01": "z.mjs" }, conditions: { "UT-Z-05": "x" } }, /không có trong registry/u],
  ]) {
    assert.match(registry(overrides).join("; "), rejected); checks += 1;
  }
  assert.deepEqual(registry({ registry: { "UT-Z-08": "z.mjs" }, conditions: {}, wholeGate: ["UT-Z-08"] }), [],
    "a whole-gate test needs its runner in the sweep, not a per-ID line"); checks += 1;
  return checks;
}
